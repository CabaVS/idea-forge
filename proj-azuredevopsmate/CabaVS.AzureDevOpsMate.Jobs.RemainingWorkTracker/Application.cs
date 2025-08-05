using System.Globalization;
using Azure.Storage.Blobs;
using CabaVS.AzureDevOpsMate.Shared.Configuration;
using CabaVS.Shared.Infrastructure.Storage;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CabaVS.AzureDevOpsMate.Jobs.RemainingWorkTracker;

internal sealed class Application(
    ILogger<Application> logger,
    IHttpClientFactory httpClientFactory,
    IBlobServiceClientProvider blobServiceClientProvider,
    IOptions<RemainingWorkTrackerOptions> options)
{
    public async Task RunAsync()
    {
        logger.LogInformation("Remaining Work Tracker started at {Timestamp} UTC.", DateTime.UtcNow);
        
        using HttpClient httpClient = httpClientFactory.CreateClient();
        httpClient.BaseAddress = new Uri(options.Value.ApiUrlBase);
        
        RemainingWorkTrackerOptions.ToTrackItem[] itemsToTrack = options.Value.ToTrackItems;
        if (itemsToTrack is { Length: 0 })
        {
            logger.LogInformation("No items to track.");
            return;
        }

        foreach (RemainingWorkTrackerOptions.ToTrackItem item in itemsToTrack)
        {
            logger.LogInformation("Processing item {WorkItemId} from {From} to {To}.", item.WorkItemId, item.From, item.To);
            
            HttpResponseMessage response = await httpClient.GetAsync(
                new Uri(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        options.Value.ApiUrlForRemainingWork,
                        item.WorkItemId)));
            if (!response.IsSuccessStatusCode)
            {
                logger.LogError("Failed to get remaining work for item {WorkItemId} from {From} to {To}.", item.WorkItemId, item.From, item.To);
                continue;
            }
            
            logger.LogInformation("Response received. Proceeding to blob upload.");

            BlobServiceClient blobServiceClient = blobServiceClientProvider.GetBlobServiceClient();
            _ = blobServiceClient.AccountName;
        }
        
        logger.LogInformation("Remaining Work Tracker finished at {Timestamp} UTC.", DateTime.UtcNow);
    }
}
