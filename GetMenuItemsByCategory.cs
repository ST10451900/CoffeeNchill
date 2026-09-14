using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Azure.Data.Tables;
using CoffeeNchill.Models;

namespace CoffeeNchill
{
    public class GetMenuItemsByCategory
    {
        private readonly ILogger _logger;
        private readonly string _connectionString;
        private const string TableName = "MenuItems";

        public GetMenuItemsByCategory(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<GetMenuItemsByCategory>();
            _connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage")
                ?? throw new InvalidOperationException("AzureWebJobsStorage is not configured.");
        }

        [Function("GetMenuItemsByCategory")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "menu/category/{category}")] HttpRequestData req,
            string category)
        {
            var tableClient = new TableClient(_connectionString, TableName);
            await tableClient.CreateIfNotExistsAsync();

            var items = new List<MenuItem>();

            await foreach (var item in tableClient.QueryAsync<MenuItem>(filter: $"PartitionKey eq '{category}'"))
            {
                items.Add(item);
            }

            // Added error handling: Returns 404 if no items were found 
            if (items.Count == 0)
            {
                var notFoundResponse = req.CreateResponse(HttpStatusCode.NotFound);
                await notFoundResponse.WriteStringAsync($"No menu items were found for this category '{category}'.");
                return notFoundResponse;
            }

            _logger.LogInformation($"Returning {items.Count} menu items for category {category}");

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(items);
            return response;
        }
    }
}