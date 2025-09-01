using Aspire.Hosting.Azure;

IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

// Parameters
IResourceBuilder<ParameterResource> configUrlForAzdm =
    builder.AddParameter("config-url-for-project-azuredevopsmate", true);

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
    .WithEnvironment("CVS_CONFIGURATION_FROM_AZURE_URL", configUrlForAzdm)
    .WithReference(blobsResource, "BlobStorage").WaitFor(blobsResource);

// Azure DevOps Mate (Web)
builder.AddProject<Projects.CabaVS_AzureDevOpsMate_Web>("aca-azuredevopsmate-web")
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development")
    .WithEnvironment("CVS_CONFIGURATION_FROM_AZURE_URL", configUrlForAzdm)
    .WithReference(blobsResource, "BlobStorage").WaitFor(blobsResource)
    .WithReference(azureDevOpsMateApi).WaitFor(azureDevOpsMateApi);

// Azure DevOps Mate (Job - Remaining Work Tracker)
builder.AddProject<Projects.CabaVS_AzureDevOpsMate_Jobs_RemainingWorkTracker>("acajob-azuredevopsmate-rwt")
    .WithEnvironment("DOTNET_ENVIRONMENT", "Development")
    .WithEnvironment("CVS_CONFIGURATION_FROM_AZURE_URL", configUrlForAzdm)
    .WithReference(blobsResource, "BlobStorage").WaitFor(blobsResource)
    .WithReference(azureDevOpsMateApi).WaitFor(azureDevOpsMateApi)
    .WithExplicitStart();

await builder.Build().RunAsync();
