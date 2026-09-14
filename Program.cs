using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Azure.Storage.Files.Shares;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

var storageConnectionString =
    Environment.GetEnvironmentVariable("AzureWebJobsStorage")
    ?? throw new InvalidOperationException(
        "AzureWebJobsStorage is not configured.");

builder.Services.AddSingleton(
    new ShareServiceClient(storageConnectionString)
);

builder.Build().Run();