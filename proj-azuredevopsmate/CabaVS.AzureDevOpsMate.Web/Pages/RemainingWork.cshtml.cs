using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using CabaVS.AzureDevOpsMate.Shared.Configuration;
using CabaVS.Shared.Infrastructure.Storage;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;

namespace CabaVS.AzureDevOpsMate.Web.Pages;

internal sealed class RemainingWork(
    ILogger<RemainingWork> logger,
    IOptions<RemainingWorkTrackerOptions> options,
    IBlobConnectionProvider blobConnectionProvider) : PageModel
{
    public string SnapshotsJson { get; private set; } = "{}";
    
    public async Task OnGet(int workItemId)
    {
        if (workItemId <= 0)
        {
            logger.LogWarning("Invalid work item id: {WorkItemId}", workItemId);
            return;
        }
        
        var blobsPrefix = $"rwt_{workItemId}_";
        var containerName = options.Value.ReportContainerName;
        
        logger.LogInformation("Fetching blobs with prefix {Prefix} from container {Container}",
            blobsPrefix, containerName);
        
        BlobServiceClient blobServiceClient = blobConnectionProvider.GetBlobServiceClient();
        BlobContainerClient? blobContainerClient = blobServiceClient.GetBlobContainerClient(containerName);
        
        var allSnapshots = new List<string>();
        await foreach (BlobItem blobItem in blobContainerClient.GetBlobsAsync(prefix: blobsPrefix))
        {
            BlobClient blobClient = blobContainerClient.GetBlobClient(blobItem.Name);
            
            Response<BlobDownloadResult>? downloadResponse = await blobClient.DownloadContentAsync();
            if (downloadResponse is not { HasValue: true })
            {
                logger.LogWarning("Blob {BlobName} has no content", blobItem.Name);
                continue;
            }
            
            var text = downloadResponse.Value.Content.ToString();
            logger.LogInformation("Downloaded blob {BlobName}, Content length: {Length}",
                blobItem.Name, text.Length);
            
            allSnapshots.Add(text);
        }
        
        SnapshotsJson = $"[{string.Join(',', allSnapshots)}]";
    }
}
