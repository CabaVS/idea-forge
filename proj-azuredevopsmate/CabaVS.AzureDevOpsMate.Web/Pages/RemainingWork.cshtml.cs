using System.Globalization;
using System.Text.RegularExpressions;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using CabaVS.AzureDevOpsMate.Shared.Configuration;
using CabaVS.Shared.Infrastructure.Storage;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;

namespace CabaVS.AzureDevOpsMate.Web.Pages;

internal sealed partial class RemainingWork(
    ILogger<RemainingWork> logger,
    IOptions<RemainingWorkTrackerOptions> options,
    IBlobConnectionProvider blobConnectionProvider) : PageModel
{
    public int WorkItemId { get; private set; }
    public string SnapshotsJson { get; private set; } = "[]";
    public string ErrorMessage { get; private set; } = string.Empty;
    
    public async Task OnGet(int workItemId)
    {
        if (workItemId <= 0)
        {
            ErrorMessage = "Invalid work item id!";
            
            logger.LogWarning("Invalid work item id: {WorkItemId}", workItemId);
            return;
        }
        
        WorkItemId = workItemId;
        
        var blobsPrefix = $"rwt_{workItemId}_";
        var containerName = options.Value.ReportContainerName;
        
        logger.LogInformation("Fetching blobs with prefix {Prefix} from container {Container}",
            blobsPrefix, containerName);
        
        BlobServiceClient blobServiceClient = blobConnectionProvider.GetBlobServiceClient();
        BlobContainerClient? blobContainerClient = blobServiceClient.GetBlobContainerClient(containerName);
        
        var augmented = new List<(DateTime date, string json)>();
        await foreach (BlobItem blobItem in blobContainerClient.GetBlobsAsync(prefix: blobsPrefix))
        {
            if (!TryExtractIsoDateFromBlobName(blobItem.Name, out DateTime dt, out var iso))
            {
                logger.LogWarning("Blob name does not contain a yyyyMMdd stamp: {BlobName}", blobItem.Name);
                continue;
            }
            
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
            
            var trimmed = text.Trim();
            if (!(trimmed.StartsWith('{') && trimmed.EndsWith('}')))
            {
                logger.LogWarning("Blob {BlobName} content is not a single JSON object", blobItem.Name);
                continue;
            }
            
            var withDate = $"{{\"date\":\"{iso}\",{trimmed[1..]}";
            augmented.Add((dt, withDate));
        }
        
        SnapshotsJson = "[" + string.Join(',', augmented.OrderBy(x => x.date).Select(x => x.json)) + "]";
    }
    
    private static bool TryExtractIsoDateFromBlobName(string blobName, out DateTime date, out string isoDate)
    {
        Match m = DateFromBlobNameRegex().Match(blobName);
        if (!m.Success)
        {
            date = default;
            isoDate = "";
            return false;
        }

        var stamp = m.Groups[1].Value;
        if (!DateTime.TryParseExact(stamp, "yyyyMMdd", CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out date))
        {
            isoDate = "";
            return false;
        }
        
        isoDate = date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        return true;
    }

    [GeneratedRegex(@"_(\d{8})(?:\.\w+)?$", RegexOptions.CultureInvariant)]
    private static partial Regex DateFromBlobNameRegex();
}
