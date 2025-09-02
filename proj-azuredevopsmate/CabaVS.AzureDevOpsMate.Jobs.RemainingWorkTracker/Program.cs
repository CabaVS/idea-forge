using Azure.Monitor.OpenTelemetry.Exporter;
using CabaVS.AzureDevOpsMate.Jobs.RemainingWorkTracker;
using CabaVS.AzureDevOpsMate.Shared;
using CabaVS.AzureDevOpsMate.Shared.Configuration;
using CabaVS.Shared.Infrastructure.ConfigurationProviders;
using CabaVS.Shared.Infrastructure.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;

using IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((context, config) => config.AddJsonStreamFromBlob(
        context.HostingEnvironment.IsDevelopment()))
    .ConfigureServices((context, services) =>
    {
        services.Configure<RemainingWorkTrackerOptions>(
            context.Configuration.GetSection(AzureDevOpsMateConstants.ConfigSectionNames.RemainingWorkTracker));
        
        services.AddOpenTelemetry()
            .ConfigureResource(_ => ResourceBuilder.CreateDefault())
            .WithMetrics(metrics =>
            {
                MeterProviderBuilder meterProviderBuilder = metrics
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation();
                if (context.HostingEnvironment.IsDevelopment())
                {
                    meterProviderBuilder.AddOtlpExporter();
                }
                else
                {
                    meterProviderBuilder.AddAzureMonitorMetricExporter();
                }
            })
            .WithTracing(tracing =>
            {
                TracerProviderBuilder tracerProviderBuilder = tracing
                    .AddSource(Constants.ActivityNames.RemainingWorkTracker)
                    .AddHttpClientInstrumentation();
                if (context.HostingEnvironment.IsDevelopment())
                {
                    tracerProviderBuilder.AddOtlpExporter();
                }
                else
                {
                    tracerProviderBuilder.AddAzureMonitorTraceExporter();
                }
            });
        
        services.AddHttpClient(
            AzureDevOpsMateConstants.HttpClientNames.AcaAzureDevOpsMate,
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
        
        services.AddHostedService<Application>();
    })
    .UseSerilog((context, config) => config.ReadFrom.Configuration(context.Configuration))
    .Build();

await host.RunAsync();
