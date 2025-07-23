using System.Globalization;
using System.Xml.Linq;
using CabaVS.AzureDevOpsMate.Configuration;
using CabaVS.AzureDevOpsMate.Constants;
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
    async (int workItemId, WorkItemTrackingHttpClient workItemClient, IOptions<TeamsDefinitionOptions> teamsDefinitionOptions) =>
{
    WorkItem? workItem = await workItemClient.GetWorkItemAsync(workItemId);
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
            Amount = double.Parse(row[2].Replace(",", ".", StringComparison.InvariantCulture), CultureInfo.InvariantCulture),
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

await app.RunAsync();
