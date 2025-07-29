using System.Globalization;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using CabaVS.AzureDevOpsMate.Functions.Configuration;
using CabaVS.AzureDevOpsMate.Functions.TableStorage;
using Microsoft.Extensions.Options;

namespace CabaVS.AzureDevOpsMate.Functions;

internal sealed class TimeTriggeredFunctions(
    ILoggerFactory loggerFactory,
    IHttpClientFactory httpClientFactory,
    IOptions<WorkItemsTrackingOptions> options,
    TableStorageUploader tableStorageUploader)
{
    private readonly ILogger _logger = loggerFactory.CreateLogger<TimeTriggeredFunctions>();
    private readonly WorkItemsTrackingOptions _options = options.Value;

    [Function("midnight-get-remaining-work")]
    public async Task Run([TimerTrigger("0 0 0 * * 2-6")] TimerInfo myTimer)
    {
        var utcNow = DateOnly.FromDateTime(DateTime.UtcNow);
        _logger.LogInformation("Executing midnight-get-remaining-work function on {UtcNow}. Is Past Due? - {IsPastDue}.", utcNow, myTimer.IsPastDue);
        
        var urlTemplate = _options.UrlTemplateForRemainingWork;
        if (string.IsNullOrWhiteSpace(urlTemplate))
        {
            throw new InvalidOperationException("URL template is not configured.");
        }
        
        ToTrackItem[] toTrackItems = _options.ToTrackItems
            .Where(x => x.From <= utcNow)
            .Where(x => x.To >= utcNow)
            .ToArray();
        if (toTrackItems is { Length: 0 })
        {
            _logger.LogInformation("No work items to track found. Skipping.");
            return;
        }
        
        using HttpClient httpClient = httpClientFactory.CreateClient();
        
        foreach (var workItemId in toTrackItems.Select(x => x.WorkItemId))
        {
            _logger.LogInformation("Processing item for {WorkItemId}.", workItemId);
            
            HttpResponseMessage response = await httpClient.GetAsync(
                new Uri(string.Format(CultureInfo.InvariantCulture, urlTemplate, workItemId)));
            
            response.EnsureSuccessStatusCode();
            _logger.LogInformation("Received a successful response from API.");
            
            var (partitionKey, rowKey) = await tableStorageUploader.UploadAsync(
                workItemId,
                await response.Content.ReadAsStringAsync());
            _logger.LogInformation("Response uploaded to table storage with Row Key = {RowKey} and Partition Key = {PartitionKey}.", rowKey, partitionKey);
        }
    }
}
