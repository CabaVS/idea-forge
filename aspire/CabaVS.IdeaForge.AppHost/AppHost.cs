using Aspire.Hosting.Azure;

IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

// Parameters
IResourceBuilder<ParameterResource> configContainerName = builder.AddParameter("config-container-name");
IResourceBuilder<ParameterResource> configBlobNameForAzureDevOpsMate =
    builder.AddParameter("config-blob-name-project-azuredevopsmate", true);

// Storage Account (emulated with Azurite)
IResourceBuilder<AzureStorageResource> azurite = builder.AddAzureStorage("stcabavsideaforge")
    .RunAsEmulator(config => config
        .WithBlobPort(27000)
        .WithQueuePort(27001)
        .WithTablePort(27002)
        .WithDataVolume()
        .WithLifetime(ContainerLifetime.Persistent));
IResourceBuilder<AzureBlobStorageResource> blobsResource = azurite.AddBlobs("blobs");

// Azure DevOps Mate (API)
IResourceBuilder<ProjectResource> azureDevOpsMateApi = builder.AddProject<Projects.CabaVS_AzureDevOpsMate>("aca-azuredevopsmate")
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development")
    .WithEnvironment("CVS_CONFIG_BLOB_NAME", configBlobNameForAzureDevOpsMate)
    .WithEnvironment("CVS_CONFIG_CONTAINER_NAME", configContainerName)
    .WithReference(blobsResource, "BlobStorage").WaitFor(blobsResource);

// Azure DevOps Mate (Job - Remaining Work Tracker)
builder.AddProject<Projects.CabaVS_AzureDevOpsMate_Jobs_RemainingWorkTracker>("acajob-azuredevopsmate-rwt")
    .WithEnvironment("DOTNET_ENVIRONMENT", "Development")
    .WithEnvironment("CVS_CONFIG_BLOB_NAME", configBlobNameForAzureDevOpsMate)
    .WithEnvironment("CVS_CONFIG_CONTAINER_NAME", configContainerName)
    .WithReference(blobsResource, "BlobStorage").WaitFor(blobsResource)
    .WithReference(azureDevOpsMateApi).WaitFor(azureDevOpsMateApi)
    .WithExplicitStart();

await builder.Build().RunAsync();
