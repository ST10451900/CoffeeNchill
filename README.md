Member 3: Staff Documents

Staff Document Management

The Staff Documents component implements document management using Azure File Share with the local Azurite storage emulator. The system provides functionality for staff documents to be uploaded, listed, and downloaded through HTTP endpoints.

The Azure File Share used for this functionality is:

staff-docs

Azure File Share provides a shared network-folder-style storage location where staff documents can be stored and accessed by the application.

Implemented API Endpoints

Method	Endpoint	Description
POST	/api/documents/upload	Uploads a staff document
GET	/api/documents	Lists documents stored in the staff-docs share
GET	/api/documents/download/{fileName}	Downloads a specific staff document

Upload Document

Endpoint:

POST /api/documents/upload

The endpoint accepts the uploaded document using multipart/form-data.

The form-data field must be named:

file

Example:

Key: file
Type: File
Value: example.pdf

A successful upload returns an HTTP 201 Created response.

The upload functionality validates the request before sending the file to Azure File Share.

List Documents

Endpoint:

GET /api/documents

This endpoint retrieves the documents currently stored in the staff-docs Azure File Share.

A successful request returns an HTTP 200 OK response containing the available documents.

Download Document

Endpoint:

GET /api/documents/download/{fileName}

This endpoint downloads a document from the staff-docs Azure File Share.

If the requested document does not exist, the API returns:

404 Not Found

Validation and Error Handling

The Staff Documents functionality includes validation and error handling for common invalid requests.

Situation	HTTP Response
Successful upload	201 Created
Successful list	200 OK
Successful download	200 OK
Missing uploaded file	400 Bad Request
Empty uploaded file	400 Bad Request
Invalid filename	400 Bad Request
Document not found	404 Not Found
Unexpected storage error	500 Internal Server Error

Filename Security

Filename safety is handled using Path.GetFileName().

This ensures that directory information supplied as part of a filename is removed before the file is accessed. This helps prevent unsafe file paths and directory traversal attempts.

Azure File Share Components

The implementation uses the Azure Storage Files Shares SDK.

ShareServiceClient is responsible for connecting to the Azure Storage account.

ShareClient represents the specific Azure File Share used by the application.

CreateIfNotExistsAsync() ensures that the required Azure File Share is available before the application attempts to use it.

The application uses the local Azurite emulator during development and testing.

Postman Testing

The following Postman tests were performed:

1. Successful PDF upload.
2. Upload request without a file.
3. Listing stored documents.
4. Downloading an existing document.
5. Attempting to download a document that does not exist.

The tests confirm that the API endpoints communicate correctly with the staff-docs Azure File Share.

Video Demonstration

The Member 3 demonstration will show:

1. Azurite running locally.
2. The staff-docs Azure File Share being created.
3. A PDF being uploaded through Postman.
4. The uploaded document appearing in the document list.
5. The PDF being downloaded successfully.
6. A request for a missing document returning 404 Not Found.
7. The relevant Staff Documents source code.
8. How the application connects to Azure File Share.
9. How unsafe filenames and file paths are prevented.

AI-use Disclosure

AI tools were used for general planning, explanation of Azure File Share concepts, and proofreading. The implementation was reviewed, adapted, tested, and explained by the student.

The student understands the implemented functionality and can explain the use of ShareServiceClient, ShareClient, CreateIfNotExistsAsync(), multipart form-data, file streams, HTTP status codes, and filename security.
