using System.Globalization;
using CabaVS.AzureDevOpsMate.Shared;
using CabaVS.AzureDevOpsMate.Shared.Configuration;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;

namespace CabaVS.AzureDevOpsMate.Web.Pages;

internal sealed class RemainingWorkCurrent(
    ILogger<RemainingWork> logger,
    IHttpClientFactory httpClientFactory,
    IOptions<RemainingWorkTrackerOptions> options) : PageModel
{
    public int WorkItemId { get; private set; }
    public string SnapshotJson { get; private set; } = "{}";
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
        
        using HttpClient httpClient = httpClientFactory.CreateClient(AzureDevOpsMateConstants.HttpClientNames.AcaAzureDevOpsMate);

        HttpResponseMessage response = await httpClient.GetAsync(
            new Uri(
                string.Format(
                    CultureInfo.InvariantCulture,
                    options.Value.ApiUrlForRemainingWork,
                    workItemId),
                UriKind.Relative));
        if (!response.IsSuccessStatusCode)
        {
            ErrorMessage = $"Failed to get remaining work for item {workItemId}";
            
            logger.LogWarning("Failed to get remaining work for item {WorkItemId}", workItemId);
            return;
        }
        
        logger.LogInformation("Response received.");
        
        SnapshotJson = await response.Content.ReadAsStringAsync();
    }
}
