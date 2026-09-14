using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Azure.Data.Tables;
using CoffeeNchill.Models;
using System.Text.Json;

namespace CoffeeNchill.Functions
{
    public class CreateMenuItem
    {
        private readonly ILogger _logger;
        private readonly string _connectionString;
        private const string TableName = "MenuItems";

        public CreateMenuItem(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<CreateMenuItem>();
            _connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage")
                ?? throw new InvalidOperationException("AzureWebJobsStorage is not configured.");
        }

        [Function("CreateMenuItem")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "menu")] HttpRequestData req)
        {
            // Read and deserialize the request body
            var body = await new StreamReader(req.Body).ReadToEndAsync();
            var input = JsonSerializer.Deserialize<MenuItem>(body, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (input == null || string.IsNullOrWhiteSpace(input.Name) || string.IsNullOrWhiteSpace(input.Category))
            {
                var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badResponse.WriteStringAsync("Request must include at least 'Name' and 'Category'.");
                return badResponse;
            }

            // Table Storage bookkeeping fields
            input.PartitionKey = input.Category;
            input.RowKey = Guid.NewGuid().ToString();

            // Ensure the table exists, then add the entity
            var tableClient = new TableClient(_connectionString, TableName);
            await tableClient.CreateIfNotExistsAsync();
            await tableClient.AddEntityAsync(input);

            _logger.LogInformation($"Created menu item {input.Name} in category {input.Category}");

            var response = req.CreateResponse(HttpStatusCode.Created);
            await response.WriteAsJsonAsync(input);
            return response;
        }
    }
}