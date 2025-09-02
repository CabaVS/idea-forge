using Azure.Monitor.OpenTelemetry.Exporter;
using CabaVS.AzureDevOpsMate.Shared;
using CabaVS.AzureDevOpsMate.Shared.Configuration;
using CabaVS.Shared.Infrastructure.ConfigurationProviders;
using CabaVS.Shared.Infrastructure.Storage;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Configuration
builder.Configuration.AddJsonStreamFromBlob(
    builder.Environment.IsDevelopment());

builder.Services.Configure<RemainingWorkTrackerOptions>(
    builder.Configuration.GetSection(AzureDevOpsMateConstants.ConfigSectionNames.RemainingWorkTracker));

// Storage
builder.Services.AddSingleton<IBlobConnectionProvider>(
    _ => new BlobConnectionProvider(
        builder.Configuration, 
        !builder.Environment.IsDevelopment()));

// Logging
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();
builder.Host.UseSerilog();

// Open Telemetry
builder.Services.AddOpenTelemetry()
    .ConfigureResource(_ => ResourceBuilder.CreateDefault())
    .WithMetrics(metrics =>
    {
        MeterProviderBuilder meterProviderBuilder = metrics
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddRuntimeInstrumentation();
        if (builder.Environment.IsDevelopment())
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
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation();
        if (builder.Environment.IsDevelopment())
        {
            tracerProviderBuilder.AddOtlpExporter();
        }
        else
        {
            tracerProviderBuilder.AddAzureMonitorTraceExporter();
        }
    });

// Razor Pages
builder.Services.AddRazorPages();

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages() 
    .WithStaticAssets();

await app.RunAsync();
