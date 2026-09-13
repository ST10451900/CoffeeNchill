using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Azure.Data.Tables;

namespace CoffeeNchill 
{
    public class DeleteMenuItem
    {
        private readonly ILogger _logger;
        private const string ConnectionString = "UseDevelopmentStorage=true";
        private const string TableName = "MenuItems";

        public DeleteMenuItem(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<DeleteMenuItem>();
        }

        [Function("DeleteMenuItem")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "menu/{category}/{id}")] HttpRequestData req,
            string category, string id)
        {
            var tableClient = new TableClient(ConnectionString, TableName);
            await tableClient.CreateIfNotExistsAsync();

            try
            {
                await tableClient.DeleteEntityAsync(category, id);
                _logger.LogInformation($"Deleted menu item {id} from category {category}");

                var response = req.CreateResponse(HttpStatusCode.NoContent);
                return response;
            }
            catch (Azure.RequestFailedException ex) when (ex.Status == 404)
            {
                var notFoundResponse = req.CreateResponse(HttpStatusCode.NotFound);
                await notFoundResponse.WriteStringAsync($"Menu item with id '{id}' in category '{category}' was not found.");
                return notFoundResponse;
            }
        }
    }
}