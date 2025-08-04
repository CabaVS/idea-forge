using Aspire.Hosting.Azure;

IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

// Parameters
IResourceBuilder<ParameterResource> configUrlForAzureDevOpsMate =
    builder.AddParameter("project-config-url-azuredevopsmate", true);

// Storage Account (emulated with Azurite)
IResourceBuilder<AzureStorageResource> azurite = builder.AddAzureStorage("stcabavsideaforge")
    .RunAsEmulator(config => config
        .WithBlobPort(27000)
        .WithQueuePort(27001)
        .WithTablePort(27002)
        .WithDataVolume()
        .WithLifetime(ContainerLifetime.Persistent));
IResourceBuilder<AzureBlobStorageResource> blobsResource = azurite.AddBlobs("blobs");

// Azure DevOps Mate
builder.AddProject<Projects.CabaVS_AzureDevOpsMate>("aca-azuredevopsmate")
    .WithEnvironment("CVS_CONFIGURATION_FROM_AZURE_URL", configUrlForAzureDevOpsMate)
    .WithReference(blobsResource).WaitFor(blobsResource);

await builder.Build().RunAsync();
