using Azure.Storage;
using Azure.Storage.Files.Shares;

using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

// Register Azure File Storage client for dependency injection
string? storageConnectionString =
    Environment.GetEnvironmentVariable("AzureStorageConnection");

if (!string.IsNullOrWhiteSpace(storageConnectionString))
{
    builder.Services.AddSingleton(
        new ShareServiceClient(storageConnectionString));
}


builder.Build().Run();