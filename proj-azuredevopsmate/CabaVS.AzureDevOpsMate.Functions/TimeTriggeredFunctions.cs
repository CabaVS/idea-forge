using System.Globalization;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using CabaVS.AzureDevOpsMate.Functions.Configuration;
using Microsoft.Extensions.Options;

namespace CabaVS.AzureDevOpsMate.Functions;

internal sealed class TimeTriggeredFunctions(ILoggerFactory loggerFactory, IHttpClientFactory httpClientFactory, IOptions<WorkItemsTrackingOptions> options)
{
    private readonly ILogger _logger = loggerFactory.CreateLogger<TimeTriggeredFunctions>();
    private readonly WorkItemsTrackingOptions _options = options.Value;

    [Function("midnight-get-remaining-work")]
    // public void Run([TimerTrigger("0 0 0 * * 2-6")] TimerInfo myTimer)
    public async Task Run([TimerTrigger("0/15 * * * * *")] TimerInfo myTimer)
    {
        var utcNow = DateOnly.FromDateTime(DateTime.UtcNow);
        _logger.LogInformation("Executing midnight-get-remaining-work function on {UtcNow}. Is Past Due? - {IsPastDue}.", utcNow, myTimer.IsPastDue);
        
        var urlTemplate = _options.UrlTemplate;
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
            
            var content = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("Response received: {Response}.", content);
        }
    }
}
