using CabaVS.AzureDevOpsMate.Jobs.RemainingWorkTracker;
using CabaVS.AzureDevOpsMate.Shared.Configuration;
using CabaVS.Shared.Infrastructure.ConfigurationProviders;
using CabaVS.Shared.Infrastructure.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

using IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((context, config) => config.AddJsonStreamFromBlob(
        context.HostingEnvironment.IsDevelopment()))
    .ConfigureServices((context, services) =>
    {
        services.Configure<RemainingWorkTrackerOptions>(
            context.Configuration.GetSection("RemainingWorkTracker"));
        
        services.AddHttpClient(
            Constants.HttpClientNames.AcaAzureDevOpsMate,
            (sp, client) =>
            {
                IConfiguration configuration = sp.GetRequiredService<IConfiguration>();
                IOptions<RemainingWorkTrackerOptions> options = sp.GetRequiredService<IOptions<RemainingWorkTrackerOptions>>();

                client.BaseAddress = new Uri(
                    configuration["services:aca-azuredevopsmate:https:0"]
                    ?? options.Value.ApiUrlBase
                );
            });

        services.AddSingleton<IBlobConnectionProvider>(
            _ => new BlobConnectionProvider(context.Configuration, !context.HostingEnvironment.IsDevelopment()));
        
        services.AddSingleton<Application>();
    })
    .Build();

Application application = host.Services.GetRequiredService<Application>();
await application.RunAsync();
