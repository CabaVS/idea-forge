using Aspire.Hosting.Azure;

IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

// Parameters
IResourceBuilder<ParameterResource> configUrlForAzureDevOpsMate =
    builder.AddParameter("azuredevopsmate-config-url", true);

// Storage Account (emulated with Azurite)
IResourceBuilder<AzureStorageResource> azurite = builder.AddAzureStorage("stcabavsideaforge")
    .RunAsEmulator(config => config
        .WithBlobPort(27000)
        .WithQueuePort(27001)
        .WithTablePort(27002)
        .WithDataVolume()
        .WithLifetime(ContainerLifetime.Persistent));
IResourceBuilder<AzureBlobStorageResource> blobsResource = azurite.AddBlobs("blobs");
IResourceBuilder<AzureTableStorageResource> tablesResource = azurite.AddTables("tables");

// Azure DevOps Mate
builder.AddProject<Projects.CabaVS_AzureDevOpsMate>("aca-azuredevopsmateapi")
    .WithEnvironment("CVS_CONFIGURATION_FROM_AZURE_URL", configUrlForAzureDevOpsMate)
    .WithReference(blobsResource).WaitFor(blobsResource);

builder.AddAzureFunctionsProject<Projects.CabaVS_AzureDevOpsMate_Functions>("func-azuredevopsmateapi")
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development") // Not loaded from launchSettings.json
    .WithEnvironment("CVS_CONFIGURATION_FROM_AZURE_URL", configUrlForAzureDevOpsMate)
    .WithHostStorage(azurite)
    .WithReference(blobsResource).WaitFor(blobsResource)
    .WithReference(tablesResource).WaitFor(tablesResource);

await builder.Build().RunAsync();
