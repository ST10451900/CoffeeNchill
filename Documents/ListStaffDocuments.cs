using Azure.Storage.Files.Shares;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using System.Net;
namespace CoffeeNChill.Documents;

public class ListStaffDocuments
{
    private readonly ShareServiceClient _shareServiceClient;
    public ListStaffDocuments(ShareServiceClient shareServiceClient)
    {
        _shareServiceClient = shareServiceClient;
    }
    [Function("ListStaffDocuments")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(
            AuthorizationLevel.Anonymous,
            "get",
            Route = "documents")]
        HttpRequestData request)
    {
        var response = request.CreateResponse();
        try
        {
            // ---------------------------------------------------------
            // 1. Get the staff-docs Azure File Share
            // ---------------------------------------------------------
            var shareClient =
                StaffDocumentStorage.GetShareClient(
                    _shareServiceClient);
            // ---------------------------------------------------------
            // 2. Create the share if it does not already exist
            // ---------------------------------------------------------
            await shareClient.CreateIfNotExistsAsync();
            // ---------------------------------------------------------
            // 3. Get the root directory
            // ---------------------------------------------------------
            var directoryClient =
                shareClient.GetRootDirectoryClient();
            // ---------------------------------------------------------
            // 4. Create a list to store the documents
            // ---------------------------------------------------------
            var documents = new List<object>();
            // ---------------------------------------------------------
            // 5. Retrieve files and directories
            // ---------------------------------------------------------
            await foreach (var item in
                directoryClient.GetFilesAndDirectoriesAsync())
            {
                // -----------------------------------------------------
                // 6. Only add files, not directories
                // -----------------------------------------------------
                if (!item.IsDirectory)
                {
                    documents.Add(new
                    {
                        fileName = item.Name,
                        size = item.FileSize,
                        lastModified = item.Properties.LastModified
                    });
                }
            }
            // ---------------------------------------------------------
            // 7. Return the documents as JSON
            // ---------------------------------------------------------
            response.StatusCode = HttpStatusCode.OK;
            await response.WriteAsJsonAsync(documents);
            return response;
        }
        catch (Exception ex)
        {
            // ---------------------------------------------------------
            // 8. Handle unexpected errors
            // ---------------------------------------------------------
            response.StatusCode =
                HttpStatusCode.InternalServerError;
            await response.WriteAsJsonAsync(new
            {
                message = "An error occurred while listing documents.",
                error = ex.Message
            });
            return response;
        }
    }
}