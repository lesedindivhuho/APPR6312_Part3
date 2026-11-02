using Azure.Core;
using Azure.Data.Tables;
using Azure.Monitor.OpenTelemetry.Exporter;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Azure.Functions.Worker.OpenTelemetry;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("APPLICATIONINSIGHTS_CONNECTION_STRING")))
{
    builder.Services.AddOpenTelemetry()
        .UseFunctionsWorkerDefaults()
        .UseAzureMonitorExporter();
}

// Table storage for project updates. Locally this is Azurite (UseDevelopmentStorage=true).
var storageConnection = builder.Configuration["AzureWebJobsStorage"] ?? "UseDevelopmentStorage=true";
var tableOptions = new TableClientOptions();
tableOptions.Retry.MaxRetries = 1;
tableOptions.Retry.Mode = RetryMode.Fixed;
tableOptions.Retry.NetworkTimeout = TimeSpan.FromSeconds(5);

builder.Services.AddSingleton(new TableServiceClient(storageConnection, tableOptions));

builder.Build().Run();
