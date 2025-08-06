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
        string envName = "CVS_CONFIGURATION_FROM_AZURE_URL")
    {
        ArgumentNullException.ThrowIfNull(builder);
        
        var blobUrl = Environment.GetEnvironmentVariable(envName) 
                            ?? throw new InvalidOperationException($"Environment variable '{envName}' is not set.");

        BlobClient blobClient = new BlobConnectionProvider(
                builder.Build(),
                useIdentity: !isDevelopment)
            .GetBlobClient(new Uri(blobUrl, UriKind.Absolute));

        Response<BlobDownloadResult>? response = blobClient.DownloadContent();

        var stream = response.Value.Content.ToStream();
        return builder.AddJsonStream(stream);
    }
}
