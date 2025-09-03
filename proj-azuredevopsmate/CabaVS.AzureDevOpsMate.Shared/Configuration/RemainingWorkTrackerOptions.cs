namespace CabaVS.AzureDevOpsMate.Shared.Configuration;

public sealed class RemainingWorkTrackerOptions
{
    public string ApiUrlBase { get; set; } = string.Empty;
    public string ApiUrlForRemainingWork { get; set; } = string.Empty;
    public string ApiUrlForReportedInfo { get; set; } = string.Empty;
    public string ReportContainerName { get; set; } = string.Empty;

    public ToTrackItem[] ToTrackItems { get; set; } = [];
    
    public sealed class ToTrackItem
    {
        public int WorkItemId { get; set; }
        public DateOnly From { get; set; }
        public DateOnly To { get; set; }
    }
}
