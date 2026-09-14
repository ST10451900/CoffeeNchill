using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Azure.Storage.Files.Shares;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

// Connection to the real Azure Storage account for staff documents
var fileConnectionString =
    Environment.GetEnvironmentVariable("AzureFilesConnection")
    ?? throw new InvalidOperationException(
        "AzureFilesConnection is not configured.");

builder.Services.AddSingleton(
    new ShareServiceClient(fileConnectionString)
);

builder.Build().Run();