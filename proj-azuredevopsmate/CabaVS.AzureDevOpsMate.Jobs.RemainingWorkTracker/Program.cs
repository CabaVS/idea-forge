using CabaVS.AzureDevOpsMate.Jobs.RemainingWorkTracker;
using CabaVS.AzureDevOpsMate.Shared.Configuration;
using CabaVS.Shared.Infrastructure.ConfigurationProviders;
using CabaVS.Shared.Infrastructure.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((context, config) => config.AddJsonStreamFromBlob(
        context.HostingEnvironment.IsDevelopment()))
    .ConfigureServices((context, services) =>
    {
        services.Configure<RemainingWorkTrackerOptions>(
            context.Configuration.GetSection("RemainingWorkTracker"));
        
        services.AddHttpClient();

        services.AddSingleton<IBlobServiceClientProvider>(
            _ => new BlobServiceClientProvider(context.Configuration, !context.HostingEnvironment.IsDevelopment()));
        
        services.AddSingleton<Application>();
    })
    .Build();

Application application = host.Services.GetRequiredService<Application>();
await application.RunAsync();
