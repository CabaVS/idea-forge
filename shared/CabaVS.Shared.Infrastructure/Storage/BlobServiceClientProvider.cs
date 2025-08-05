using Azure.Identity;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;

namespace CabaVS.Shared.Infrastructure.Storage;

public interface IBlobServiceClientProvider
{
    BlobServiceClient GetBlobServiceClient();
}

public sealed class BlobServiceClientProvider(
    IConfiguration configuration,
    bool useIdentity = true,
    string connectionStringName = "BlobStorage") : IBlobServiceClientProvider
{
    public BlobServiceClient GetBlobServiceClient()
    {
        var connectionString = configuration.GetConnectionString(connectionStringName) ??
                         configuration[connectionStringName] ??
                         throw new InvalidOperationException($"Connection string '{connectionStringName}' is not configured.");
        
        return useIdentity
            ? new BlobServiceClient(new Uri(connectionString), new DefaultAzureCredential())
            : new BlobServiceClient(connectionString);
    }
}
