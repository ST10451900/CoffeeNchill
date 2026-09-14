using Azure;
using Azure.Storage.Files.Shares;
using HttpMultipartParser;
using Azure.Storage.Files.Shares.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using System.Net;
namespace CoffeeNChill.Documents;

public class UploadStaffDocument
{
    public const string ShareName = "staff-docs";
    private readonly ShareServiceClient _shareServiceClient;
    public UploadStaffDocument(ShareServiceClient shareServiceClient)
    {
        _shareServiceClient = shareServiceClient;
    }
    [Function("UploadStaffDocument")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(
            AuthorizationLevel.Anonymous,
            "post",
            Route = "documents/upload")]
        HttpRequestData request)
    {
        var response = request.CreateResponse();
        try
        {
            // ---------------------------------------------------------
            // 1. Check that the request is multipart/form-data
            // ---------------------------------------------------------
            if (!request.Headers.TryGetValues(
                    "Content-Type",
                    out var contentTypeValues))
            {
                response.StatusCode = HttpStatusCode.BadRequest;
                await response.WriteStringAsync(
                    "Content-Type header is missing.");
                return response;
            }
            string contentType =
                contentTypeValues.FirstOrDefault() ?? string.Empty;
            if (!contentType.Contains(
                    "multipart/form-data",
                    StringComparison.OrdinalIgnoreCase))
            {
                response.StatusCode = HttpStatusCode.BadRequest;
                await response.WriteStringAsync(
                    "The request must use multipart/form-data.");
                return response;
            }
            // ---------------------------------------------------------
            // 2. Parse the multipart/form-data request
            // ---------------------------------------------------------
            var parser = await MultipartFormDataParser.ParseAsync(
                request.Body);
            // ---------------------------------------------------------
            // 3. Find the uploaded file
            // ---------------------------------------------------------
            var uploadedFile = parser.Files.FirstOrDefault();
            if (uploadedFile == null)
            {
                response.StatusCode = HttpStatusCode.BadRequest;
                await response.WriteStringAsync(
                    "No file was uploaded.");
                return response;
            }
            // ---------------------------------------------------------
            // 4. Get and validate the file name
            // ---------------------------------------------------------
            string originalFileName = uploadedFile.FileName;
            if (string.IsNullOrWhiteSpace(originalFileName))
            {
                response.StatusCode = HttpStatusCode.BadRequest;
                await response.WriteStringAsync(
                    "The uploaded file does not have a valid file name.");
                return response;
            }
            string fileName = Path.GetFileName(originalFileName);
            if (string.IsNullOrWhiteSpace(fileName))
            {
                response.StatusCode = HttpStatusCode.BadRequest;
                await response.WriteStringAsync(
                    "The uploaded file does not have a valid file name.");
                return response;
            }
            // ---------------------------------------------------------
            // 5. Only allow PDF documents
            // ---------------------------------------------------------
            if (!fileName.EndsWith(
                    ".pdf",
                    StringComparison.OrdinalIgnoreCase))
            {
                response.StatusCode = HttpStatusCode.BadRequest;
                await response.WriteStringAsync(
                    "Only PDF files are allowed.");
                return response;
            }
            // ---------------------------------------------------------
            // 6. Get the uploaded file stream
            // ---------------------------------------------------------
            Stream fileStream = uploadedFile.Data;
            if (fileStream == null)
            {
                response.StatusCode = HttpStatusCode.BadRequest;
                await response.WriteStringAsync(
                    "The uploaded file could not be read.");
                return response;
            }
            // ---------------------------------------------------------
            // 7. Check that the file is not empty
            // ---------------------------------------------------------
            if (fileStream.Length <= 0)
            {
                response.StatusCode = HttpStatusCode.BadRequest;
                await response.WriteStringAsync(
                    "The uploaded file is empty.");
                return response;
            }
            long fileLength = fileStream.Length;
            // ---------------------------------------------------------
            // 8. Get the Azure File Share client
            // ---------------------------------------------------------
            var shareClient =
                StaffDocumentStorage.GetShareClient(
                    _shareServiceClient);
            // ---------------------------------------------------------
            // 9. Create the staff-docs share if necessary
            // ---------------------------------------------------------
            await shareClient.CreateIfNotExistsAsync();
            // ---------------------------------------------------------
            // 10. Get the root directory
            // ---------------------------------------------------------
            var directoryClient =
                shareClient.GetRootDirectoryClient();
            // ---------------------------------------------------------
            // 11. Create the Azure file client
            // ---------------------------------------------------------
            var fileClient =
                directoryClient.GetFileClient(fileName);
            // ---------------------------------------------------------
            // 12. Create the file with the correct size
            // ---------------------------------------------------------
            await fileClient.CreateAsync(fileLength);
            // ---------------------------------------------------------
            // 13. Upload the contents
            // ---------------------------------------------------------
            fileStream.Position = 0;
            await fileClient.UploadRangeAsync(
                new HttpRange(0, fileLength),
                fileStream);
            // ---------------------------------------------------------
            // 14. Return successful response
            // ---------------------------------------------------------
            response.StatusCode = HttpStatusCode.Created;
            await response.WriteAsJsonAsync(new
            {
                message = "File uploaded successfully.",
                fileName = fileName,
                size = fileLength
            });
            return response;
        }
        catch (Exception ex)
        {
            // ---------------------------------------------------------
            // 15. Handle unexpected errors
            // ---------------------------------------------------------
            response.StatusCode =
                HttpStatusCode.InternalServerError;
            await response.WriteAsJsonAsync(new
            {
                message = "An error occurred while uploading the file.",
                error = ex.Message
            });
            return response;
        }
    }
}