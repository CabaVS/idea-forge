namespace CabaVS.AzureDevOpsMate.Functions.Configuration;

public sealed class WorkItemsTrackingOptions
{
    public string UrlTemplateForRemainingWork { get; set; } = string.Empty;
    public string TableAccountUrl { get; set; } = string.Empty;
    public string TableName { get; set; } = string.Empty;
    public ToTrackItem[] ToTrackItems { get; set; } = [];
}

public sealed class ToTrackItem
{
    public int WorkItemId { get; set; }
    public DateOnly From { get; set; }
    public DateOnly To { get; set; }
}
