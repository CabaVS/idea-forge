using Azure;
using Azure.Data.Tables;

namespace CabaVS.AzureDevOpsMate.Functions.TableStorage;

internal sealed class WorkItemRemainingReport : ITableEntity
{
    public required string PartitionKey { get; set; }
    public string RowKey { get; set; } = Guid.NewGuid().ToString();
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }

    public string JsonPayload { get; set; } = string.Empty;
}
