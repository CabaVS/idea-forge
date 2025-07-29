using Azure;
using Azure.Identity;
using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Configuration;

namespace CabaVS.Shared.Infrastructure;

public static class AzureBlobJsonConfigurationProvider
{
    public static IConfigurationBuilder AddJsonStreamFromBlob(this IConfigurationBuilder builder, bool isDevelopment, string envName = "CVS_CONFIGURATION_FROM_AZURE_URL")
    {
        ArgumentNullException.ThrowIfNull(builder);
        
        var configUrl = Environment.GetEnvironmentVariable(envName);
        if (string.IsNullOrWhiteSpace(configUrl))
        {
            return builder;
        }
        
        BlobClient blobClient;
        if (isDevelopment)
        {
            var configuration = (IConfiguration)builder;
            
            var connectionString = configuration.GetConnectionString("blobs") ??
                                   configuration["blobs"] ??
                                   throw new InvalidOperationException("Connection string 'blobs' is not configured.");
            
            var (accountName, accountKey) = ParseAccountCredentials(connectionString);

            blobClient = new BlobClient(new Uri(configUrl), new StorageSharedKeyCredential(accountName, accountKey));
        }
        else
        {
            blobClient = new BlobClient(new Uri(configUrl), new DefaultAzureCredential());
        }

        Response<BlobDownloadResult>? response = blobClient.DownloadContent();

        var stream = response.Value.Content.ToStream();
        return builder.AddJsonStream(stream);
    }
    
    private static (string AccountName, string AccountKey) ParseAccountCredentials(string connectionString)
    {
        var parts = connectionString.Split(';', StringSplitOptions.RemoveEmptyEntries);

        string? accountName = null;
        string? accountKey = null;

        foreach (var part in parts)
        {
            if (part.StartsWith("AccountName=", StringComparison.OrdinalIgnoreCase))
            {
                accountName = part["AccountName=".Length..];
            }
            else if (part.StartsWith("AccountKey=", StringComparison.OrdinalIgnoreCase))
            {
                accountKey = part["AccountKey=".Length..];
            }
        }

        return string.IsNullOrEmpty(accountName) || string.IsNullOrEmpty(accountKey)
            ? throw new InvalidOperationException("Invalid connection string: missing AccountName or AccountKey.")
            : (accountName, accountKey);
    }
}
