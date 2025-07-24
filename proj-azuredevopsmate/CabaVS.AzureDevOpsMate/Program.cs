using System.Globalization;
using System.Xml.Linq;
using CabaVS.AzureDevOpsMate.Configuration;
using CabaVS.AzureDevOpsMate.Constants;
using CabaVS.AzureDevOpsMate.Extensions;
using CabaVS.AzureDevOpsMate.Models;
using CabaVS.Shared.Infrastructure;
using Microsoft.Extensions.Options;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Configuration
builder.Configuration.AddJsonStreamFromBlob(
    builder.Environment.IsDevelopment());

builder.Services.Configure<AzureDevOpsOptions>(
    builder.Configuration.GetSection("AzureDevOps"));
builder.Services.Configure<TeamsDefinitionOptions>(
    builder.Configuration.GetSection("TeamsDefinition"));

// Services
builder.Services.AddSingleton(sp =>
{
    AzureDevOpsOptions options = sp.GetRequiredService<IOptions<AzureDevOpsOptions>>().Value;

    if (string.IsNullOrWhiteSpace(options.AccessToken))
    {
        throw new InvalidOperationException("Access Token is not configured.");
    }

    if (string.IsNullOrWhiteSpace(options.OrganizationUrl))
    {
        throw new InvalidOperationException("Organization URL is not configured.");
    }

    var credentials = new VssBasicCredential(string.Empty, options.AccessToken);
    var connection = new VssConnection(new Uri(options.OrganizationUrl), credentials);

    WorkItemTrackingHttpClient? client = connection.GetClient<WorkItemTrackingHttpClient>();
    return client ?? throw new InvalidOperationException("Failed to create WorkItemTrackingHttpClient.");
});

WebApplication app = builder.Build();

// Analyzes the 'ReportingInfo' HTML field of a specified Azure DevOps work item and returns total effort grouped by team
app.MapGet(
    "api/work-items/{workItemId:int}/reporting-info",
    async (
        int workItemId,
        WorkItemTrackingHttpClient workItemClient,
        IOptions<TeamsDefinitionOptions> teamsDefinitionOptions,
        CancellationToken cancellationToken) =>
    {
        WorkItem? workItem = await workItemClient.GetWorkItemAsync(
            workItemId,
            fields: [FieldNames.ReportingInfo],
            cancellationToken: cancellationToken);
        if (workItem is null)
        {
            return Results.NotFound();
        }

        var reportingInfo = workItem.Fields.GetCastedValueOrDefault(FieldNames.ReportingInfo, string.Empty);
        if (string.IsNullOrWhiteSpace(reportingInfo))
        {
            return Results.Ok(Array.Empty<object>());
        }

        var sanitizedHtml = reportingInfo.Replace("&nbsp;", " ", StringComparison.InvariantCulture);
        var parsed = XDocument.Parse(sanitizedHtml)
            .Descendants("tr")
            .Select(tr => tr.Elements("td").Select(td => td.Value.Trim()).ToArray())
            .Where(row => row is { Length: 4 })
            .Select(row => new
            {
                Date = DateOnly.ParseExact(row[0], "dd.MM.yyyy", CultureInfo.InvariantCulture),
                Reporter = row[1],
                Amount = double.Parse(row[2].Replace(",", ".", StringComparison.InvariantCulture),
                    CultureInfo.InvariantCulture),
                Comment = row[3]
            });

        var groupedByReporter = parsed.GroupBy(x => x.Reporter)
            .Select(g => new { Reporter = g.Key, Total = g.Sum(x => x.Amount) })
            .OrderByDescending(x => x.Total)
            .ThenBy(x => x.Reporter);
        var groupedByTeam = groupedByReporter
            .Select(x =>
            {
                var team = teamsDefinitionOptions.Value
                    .Teams
                    .SingleOrDefault(y => y.Value.Contains(x.Reporter),
                        new KeyValuePair<string, HashSet<string>>($"UNKNOWN TEAM on {x.Reporter}", []))
                    .Key;
                return new { Team = team, Amount = x.Total };
            })
            .GroupBy(x => x.Team)
            .Select(g => new { Team = g.Key, Total = g.Sum(x => x.Amount) })
            .OrderByDescending(x => x.Total)
            .ThenBy(x => x.Team);

        return Results.Ok(groupedByTeam);
    });

// Traverses a hierarchy of work items and groups a sum of remaining work by a team
app.MapGet(
    "api/work-items/{workItemId:int}/remaining-work",
    async (
        int workItemId,
        WorkItemTrackingHttpClient workItemClient,
        IOptions<TeamsDefinitionOptions> teamsDefinitionOptions,
        CancellationToken cancellationToken) =>
    {
        WorkItem? root = await workItemClient.GetWorkItemAsync(
            workItemId,
            fields: [FieldNames.Title],
            cancellationToken: cancellationToken);
        if (root is null)
        {
            return Results.NotFound();
        }

        var toTraverse = new HashSet<int> { root.Id!.Value };
        var collected = new List<WorkItem>();

        do
        {
            var currentBatch = (await Task.WhenAll(toTraverse
                .Chunk(200)
                .Select(batch => workItemClient.GetWorkItemsAsync(
                    ids: batch,
                    expand: WorkItemExpand.Relations,
                    errorPolicy: WorkItemErrorPolicy.Omit,
                    cancellationToken: cancellationToken))))
                .SelectMany(x => x)
                .DistinctBy(x => x.Id)
                .ToList();

            IEnumerable<WorkItem> tasksOrBugs = currentBatch
                .Where(wi => wi.Fields.GetCastedValueOrDefault(FieldNames.WorkItemType, string.Empty) is "Task" or "Bug")
                .ToArray();
            IEnumerable<WorkItem> otherTypes = currentBatch
                .ExceptBy(tasksOrBugs.Select(wi => wi.Id), wi => wi.Id)
                .ToArray();

            collected.AddRange(
                tasksOrBugs
                    .Where(wi => wi.Fields.GetCastedValueOrDefault(FieldNames.State, string.Empty) is not "Closed" and not "Removed"));

            toTraverse = otherTypes
                .Where(wi => wi.Relations is { Count: > 0 })
                .SelectMany(wi => wi.Relations)
                .Where(r => r.Rel == RelationshipNames.ParentToChild)
                .Select(r => int.Parse(r.Url.Split('/').LastOrDefault() ?? string.Empty, CultureInfo.InvariantCulture))
                .ToHashSet();
        } while (toTraverse.Count > 0);

        var groupedByAssignee = collected
            .Select(wi =>
            {
                var tags = wi.Fields.GetCastedValueOrDefault(FieldNames.Tags, string.Empty)?
                    .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries) ?? [];
                
                return new
                {
                    Assignee = wi.Fields.GetValueOrDefault(FieldNames.AssignedTo) is IdentityRef identityRef
                        ? identityRef.UniqueName.Split('@').FirstOrDefault(string.Empty)
                        : string.Empty,
                    RemainingWork = wi.Fields.GetCastedValueOrDefault(FieldNames.RemainingWork, 0.0),
                    RemainingWorkType = wi.Fields.GetCastedValueOrDefault(FieldNames.WorkItemType, string.Empty) is "Bug"
                        ? RemainingWorkType.Bugs
                        : tags.DetermineFromTags()
                };
            })
            .GroupBy(x => x.Assignee)
            .Select(g =>
            {
                var lookupByType = g.ToLookup(x => x.RemainingWorkType);
                return new
                {
                    Assignee = string.IsNullOrWhiteSpace(g.Key) ? "UNKNOWN ASSIGNEE" : g.Key.ToUpperInvariant(),
                    TotalRemainingWork = new RemainingWorkModel(
                        lookupByType[RemainingWorkType.Bugs].Sum(x => x.RemainingWork),
                        lookupByType[RemainingWorkType.Functionality].Sum(x => x.RemainingWork),
                        lookupByType[RemainingWorkType.Requirements].Sum(x => x.RemainingWork),
                        lookupByType[RemainingWorkType.Technical].Sum(x => x.RemainingWork),
                        lookupByType[RemainingWorkType.Other].Sum(x => x.RemainingWork))
                };
            })
            .OrderByDescending(x => x.TotalRemainingWork)
            .ThenBy(x => x.Assignee);
        var groupedByTeam = groupedByAssignee
            .Select(x => new
            {
                Team = teamsDefinitionOptions.Value.Teams.SingleOrDefault(
                    y => y.Value.Contains(x.Assignee),
                    new KeyValuePair<string, HashSet<string>>($"UNKNOWN TEAM on {x.Assignee}", [])).Key,
                RemainingWork = x.TotalRemainingWork
            })
            .GroupBy(x => x.Team)
            .Select(g => new
            {
                Team = g.Key,
                TotalRemainingWork = g.Select(x => x.RemainingWork).Sum()
            })
            .OrderByDescending(x => x.TotalRemainingWork)
            .ThenBy(x => x.Team);
        
        return Results.Ok(
            new
            {
                Id = root.Id!.Value,
                Title = root.Fields.GetCastedValueOrDefault(FieldNames.Title, string.Empty),
                Report = groupedByTeam
            });
    });

await app.RunAsync();
