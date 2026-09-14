using Azure.Storage.Files.Shares;

namespace CoffeeNChill.Documents;

public static class StaffDocumentStorage
{
    private const string ShareName = "staff-docs";

    public static ShareClient GetShareClient(
        ShareServiceClient shareServiceClient)
    {
        return shareServiceClient.GetShareClient(ShareName);
    }
}
