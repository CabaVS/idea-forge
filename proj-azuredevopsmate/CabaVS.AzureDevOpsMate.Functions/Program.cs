using CabaVS.AzureDevOpsMate.Functions.Configuration;
using CabaVS.Shared.Infrastructure;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

FunctionsApplicationBuilder builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Services.AddHttpClient();

// ASPNETCORE_ENVIRONMENT is not being honored by IsDevelopment method
var isDevelopment = builder.Configuration["ASPNETCORE_ENVIRONMENT"] is "Development" ||
                    builder.Environment.IsDevelopment();

builder.Configuration.AddJsonStreamFromBlob(isDevelopment);

builder.Services.Configure<WorkItemsTrackingOptions>(
    builder.Configuration.GetSection("WorkItemsTracking"));

await builder.Build().RunAsync();
