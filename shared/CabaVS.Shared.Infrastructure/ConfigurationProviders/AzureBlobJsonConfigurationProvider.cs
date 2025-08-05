using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using CabaVS.Shared.Infrastructure.Storage;
using Microsoft.Extensions.Configuration;

namespace CabaVS.Shared.Infrastructure.ConfigurationProviders;

public static class AzureBlobJsonConfigurationProvider
{
    public static IConfigurationBuilder AddJsonStreamFromBlob(
        this IConfigurationBuilder builder,
        bool isDevelopment,
        string blobNameEnvVar = "CVS_CONFIG_BLOB_NAME",
        string containerNameEnvVar = "CVS_CONFIG_CONTAINER_NAME")
    {
        ArgumentNullException.ThrowIfNull(builder);
        
        var containerName = Environment.GetEnvironmentVariable(containerNameEnvVar) 
                            ?? throw new InvalidOperationException($"Environment variable '{containerNameEnvVar}' is not set.");
        var blobName = Environment.GetEnvironmentVariable(blobNameEnvVar)
                       ?? throw new InvalidOperationException($"Environment variable '{blobNameEnvVar}' is not set.");

        BlobClient blobClient = new BlobServiceClientProvider(
                builder.Build(),
                useIdentity: !isDevelopment)
            .GetBlobServiceClient()
            .GetBlobContainerClient(containerName)
            .GetBlobClient(blobName);

        Response<BlobDownloadResult>? response = blobClient.DownloadContent();

        var stream = response.Value.Content.ToStream();
        return builder.AddJsonStream(stream);
    }
}
