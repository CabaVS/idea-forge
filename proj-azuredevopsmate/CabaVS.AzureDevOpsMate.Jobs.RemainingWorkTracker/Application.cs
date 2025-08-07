using System.Diagnostics;
using System.Globalization;
using Azure.Storage.Blobs;
using CabaVS.AzureDevOpsMate.Shared.Configuration;
using CabaVS.Shared.Infrastructure.Storage;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CabaVS.AzureDevOpsMate.Jobs.RemainingWorkTracker;

internal sealed class Application(
    ILogger<Application> logger,
    IHttpClientFactory httpClientFactory,
    IBlobConnectionProvider blobConnectionProvider,
    IOptions<RemainingWorkTrackerOptions> options,
    IHostApplicationLifetime lifetime) : IHostedService
{
    private static readonly ActivitySource ActivitySource = new(Constants.ActivityNames.RemainingWorkTracker);

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using Activity? activity = ActivitySource.StartActivity();
        
        logger.LogInformation("Remaining Work Tracker started at {Timestamp} UTC.", DateTime.UtcNow);
        
        RemainingWorkTrackerOptions.ToTrackItem[] itemsToTrack = options.Value.ToTrackItems;
        if (itemsToTrack is { Length: 0 })
        {
            logger.LogInformation("No items to track.");
            return;
        }
        
        using HttpClient httpClient = httpClientFactory.CreateClient(Constants.HttpClientNames.AcaAzureDevOpsMate);

        foreach (RemainingWorkTrackerOptions.ToTrackItem item in itemsToTrack)
        {
            logger.LogInformation("Processing item {WorkItemId} from {From} to {To}.", item.WorkItemId, item.From, item.To);

            HttpResponseMessage response = await httpClient.GetAsync(
                new Uri(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        options.Value.ApiUrlForRemainingWork,
                        item.WorkItemId),
                    UriKind.Relative),
                cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                logger.LogError("Failed to get remaining work for item {WorkItemId} from {From} to {To}.", item.WorkItemId, item.From, item.To);
                continue;
            }
            
            logger.LogInformation("Response received. Proceeding to blob upload.");

            BlobServiceClient blobServiceClient = blobConnectionProvider.GetBlobServiceClient();
            _ = blobServiceClient.AccountName;
        }
        
        logger.LogInformation("Remaining Work Tracker finished at {Timestamp} UTC.", DateTime.UtcNow);
        
        lifetime.StopApplication();
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
