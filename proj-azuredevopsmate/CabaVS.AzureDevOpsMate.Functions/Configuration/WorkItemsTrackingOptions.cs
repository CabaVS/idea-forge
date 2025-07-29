namespace CabaVS.AzureDevOpsMate.Functions.Configuration;

internal sealed class WorkItemsTrackingOptions
{
    public string UrlTemplate { get; set; } = string.Empty;
    public ToTrackItem[] ToTrackItems { get; set; } = [];
}

internal sealed class ToTrackItem
{
    public int WorkItemId { get; set; }
    public DateOnly From { get; set; }
    public DateOnly To { get; set; }
}
