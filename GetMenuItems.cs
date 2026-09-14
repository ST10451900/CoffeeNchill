using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Azure.Data.Tables;
using CoffeeNchill.Models;

namespace CoffeeNchill.Functions
{
    public class GetMenuItems
    {
        private readonly ILogger _logger;
        private readonly string _connectionString;
        private const string TableName = "MenuItems";

        public GetMenuItems(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<GetMenuItems>();
            _connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage")
                ?? throw new InvalidOperationException("AzureWebJobsStorage is not configured.");
        }

        [Function("GetMenuItems")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "menu")] HttpRequestData req)
        {
            var tableClient = new TableClient(_connectionString, TableName);
            await tableClient.CreateIfNotExistsAsync();

            var items = new List<MenuItem>();

            await foreach (var item in tableClient.QueryAsync<MenuItem>())
            {
                items.Add(item);
            }

            _logger.LogInformation($"Returning {items.Count} menu items");

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(items);

            return response;
        }
    }
}