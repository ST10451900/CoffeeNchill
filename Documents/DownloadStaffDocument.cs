using Azure.Storage.Files.Shares;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using System.Net;
namespace CoffeeNChill.Documents;

public class DownloadStaffDocument
{
    private readonly ShareServiceClient _shareServiceClient;
    public DownloadStaffDocument(ShareServiceClient shareServiceClient)
    {
        _shareServiceClient = shareServiceClient;
    }
    [Function("DownloadStaffDocument")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(
            AuthorizationLevel.Anonymous,
            "get",
            Route = "documents/download/{fileName}")]
        HttpRequestData request,
        string fileName)
    {
        var response = request.CreateResponse();
        try
        {
            // 1. Validate the file name
            if (string.IsNullOrWhiteSpace(fileName))
            {
                response.StatusCode = HttpStatusCode.BadRequest;
                await response.WriteStringAsync(
                    "A file name is required.");
                return response;
            }
            // 2. Prevent path traversal
            fileName = Path.GetFileName(fileName);
            if (string.IsNullOrWhiteSpace(fileName))
            {
                response.StatusCode = HttpStatusCode.BadRequest;
                await response.WriteStringAsync(
                    "Invalid file name.");
                return response;
            }
            // 3. Get the staff-docs Azure File Share
            var shareClient =
                StaffDocumentStorage.GetShareClient(
                    _shareServiceClient);
            // 4. Create the share if it does not exist
            await shareClient.CreateIfNotExistsAsync();
            // 5. Get the root directory
            var directoryClient =
                shareClient.GetRootDirectoryClient();
            // 6. Get the requested file
            var fileClient =
                directoryClient.GetFileClient(fileName);
            // 7. Check whether the file exists
            if (!await fileClient.ExistsAsync())
            {
                response.StatusCode = HttpStatusCode.NotFound;
                await response.WriteStringAsync(
                    $"File '{fileName}' was not found.");
                return response;
            }
            // 8. Download the file from Azure
            var downloadResponse =
                await fileClient.DownloadAsync();
            // 9. Return the file to the client
            response.StatusCode = HttpStatusCode.OK;
            response.Headers.Add(
                "Content-Type",
                "application/pdf");
            response.Headers.Add(
                "Content-Disposition",
                $"attachment; filename=\"{fileName}\"");
            await downloadResponse.Value.Content.CopyToAsync(
                response.Body);
            return response;
        }
        catch (Exception ex)
        {
            // 10. Handle unexpected errors
            response.StatusCode =
                HttpStatusCode.InternalServerError;
            await response.WriteAsJsonAsync(new
            {
                message =
                    "An error occurred while downloading the file.",
                error = ex.Message
            });
            return response;
        }
    }
}