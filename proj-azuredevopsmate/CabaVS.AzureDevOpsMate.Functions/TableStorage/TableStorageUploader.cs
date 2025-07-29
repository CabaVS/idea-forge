using System.Globalization;
using Azure.Data.Tables;

namespace CabaVS.AzureDevOpsMate.Functions.TableStorage;

public sealed class TableStorageUploader(TableClient tableClient)
{
    public async Task<(string PartitionKey, string RowKey)> UploadAsync(int workItemId, string report, CancellationToken cancellationToken = default)
    {
        await tableClient.CreateIfNotExistsAsync(cancellationToken);

        var entity = new WorkItemRemainingReport
        {
            PartitionKey = workItemId.ToString(CultureInfo.InvariantCulture),
            JsonPayload = report
        };
        
        await tableClient.AddEntityAsync(entity, cancellationToken);
        
        return (entity.PartitionKey, entity.RowKey);
    }
}
