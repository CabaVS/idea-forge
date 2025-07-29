using Azure.Data.Tables;
using Azure.Identity;
using CabaVS.AzureDevOpsMate.Functions.Configuration;
using CabaVS.AzureDevOpsMate.Functions.TableStorage;
using CabaVS.Shared.Infrastructure;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

FunctionsApplicationBuilder builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Services.AddHttpClient();

// ASPNETCORE_ENVIRONMENT is not being honored by IsDevelopment method
var isDevelopment = builder.Configuration["ASPNETCORE_ENVIRONMENT"] is "Development" ||
                    builder.Environment.IsDevelopment();

builder.Configuration.AddJsonStreamFromBlob(isDevelopment);

builder.Services.Configure<WorkItemsTrackingOptions>(
    builder.Configuration.GetSection("WorkItemsTracking"));

builder.Services.AddSingleton(provider =>
{
    IConfiguration configuration = provider.GetRequiredService<IConfiguration>();
    IOptions<WorkItemsTrackingOptions> options = provider.GetRequiredService<IOptions<WorkItemsTrackingOptions>>();

    return isDevelopment
        ? new TableClient(
            configuration.GetConnectionString("tables") ?? configuration["tables"],
            options.Value.TableName)
        : new TableClient(
            new Uri(options.Value.TableAccountUrl),
            options.Value.TableName,
            new DefaultAzureCredential());
});

builder.Services.AddSingleton<TableStorageUploader>();

await builder.Build().RunAsync();
