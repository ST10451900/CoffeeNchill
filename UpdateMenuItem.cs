using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Azure.Data.Tables;
using CoffeeNchill.Models;
using System.Text.Json;

namespace CoffeeNchill 
{
    public class UpdateMenuItem
    {
        private readonly ILogger _logger;
        private const string ConnectionString = "UseDevelopmentStorage=true";
        private const string TableName = "MenuItems";

        public UpdateMenuItem(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<UpdateMenuItem>();
        }

        [Function("UpdateMenuItem")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "menu/{category}/{id}")] HttpRequestData req,
            string category, string id)
        {
            var body = await new StreamReader(req.Body).ReadToEndAsync();
            var input = JsonSerializer.Deserialize<MenuItem>(body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (input == null)
            {
                var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badResponse.WriteStringAsync("Invalid request. Please provide valid JSON.");
                return badResponse;
            }

            var tableClient = new TableClient(ConnectionString, TableName);
            await tableClient.CreateIfNotExistsAsync();

            try
            {
                var existingItemResponse = await tableClient.GetEntityAsync<MenuItem>(category, id);
                var existingItem = existingItemResponse.Value;

                // Update only the allowed fields
                existingItem.Price = input.Price;
                existingItem.IsAvailable = input.IsAvailable;

                if (!string.IsNullOrWhiteSpace(input.Name)) existingItem.Name = input.Name;
                if (!string.IsNullOrWhiteSpace(input.Description)) existingItem.Description = input.Description;

                await tableClient.UpdateEntityAsync(existingItem, existingItem.ETag);

                _logger.LogInformation($"Updated menu item {id} in category {category}");

                var response = req.CreateResponse(HttpStatusCode.OK);
                await response.WriteAsJsonAsync(existingItem);
                return response;
            }
            catch (Azure.RequestFailedException ex) when (ex.Status == 404)
            {
                var notFoundResponse = req.CreateResponse(HttpStatusCode.NotFound);
                await notFoundResponse.WriteStringAsync($"Menu item with the id '{id}' in category '{category}' was not found.");
                return notFoundResponse;
            }
        }
    }
}