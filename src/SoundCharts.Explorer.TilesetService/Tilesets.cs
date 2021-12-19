using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;

namespace SoundCharts.Explorer.TilesetService;

internal record Tileset(string Id, string Name, string Url);

internal static class Tilesets
{
    public static async Task<IEnumerable<Tileset>> GetTilesets(string connectionString)
    {
        var blobServiceClient = new BlobServiceClient(connectionString);
        var containerClient = blobServiceClient.GetBlobContainerClient("tilesets");

        var tilesets = new List<Tileset>();

        await foreach (var hierarchyItem in containerClient.GetBlobsByHierarchyAsync(
            traits: BlobTraits.Metadata | BlobTraits.Tags,
            prefix: "litedb/",
            delimiter: "/"))
        {
            string id = hierarchyItem.Blob.Tags["sc-tileset-id"];

            tilesets.Add(new Tileset(id, hierarchyItem.Blob.Name, "TODO: Get SAS-based URL"));
        }

        return tilesets;
    }

    public static async Task<Tileset?> GetTilesetById(string id, string connectionString)
    {
        var blobServiceClient = new BlobServiceClient(connectionString);
        var taggedBlob = await blobServiceClient.FindBlobsByTagsAsync($"\"sc-tileset-id\" = '{id}'").FirstOrDefaultAsync();

        if (taggedBlob is null)
        {
            return null;
        }

        var blobContainerClient = blobServiceClient.GetBlobContainerClient(taggedBlob.BlobContainerName);
        var blobClient = blobContainerClient.GetBlobClient(taggedBlob.BlobName);

        var properties = await blobClient.GetPropertiesAsync();

        return new Tileset(id, taggedBlob.BlobName, "TODO: Get SAS-based URL");
    }

    public static async Task<Uri?> GetTilesetDownloadUriById(string id, string connectionString, string accountName, string accountKey)
    {
        var blobServiceClient = new BlobServiceClient(connectionString);
        var taggedBlob = await blobServiceClient.FindBlobsByTagsAsync($"\"sc-tileset-id\" = '{id}'").FirstOrDefaultAsync();

        if (taggedBlob is null)
        {
            return null;
        }

        var blobContainerClient = blobServiceClient.GetBlobContainerClient(taggedBlob.BlobContainerName);
        var blobClient = blobContainerClient.GetBlobClient(taggedBlob.BlobName);

        var sasBuilder = new BlobSasBuilder(
            BlobSasPermissions.Read,
            DateTimeOffset.UtcNow.AddMinutes(5))
        {
            BlobContainerName = taggedBlob.BlobContainerName,
            BlobName = taggedBlob.BlobName,
            Resource = "b",
            StartsOn = DateTimeOffset.UtcNow.AddMinutes(-5)
        };

        var blobUriBuilder = new BlobUriBuilder(blobClient.Uri)
        {
            Sas = sasBuilder.ToSasQueryParameters(
                new StorageSharedKeyCredential(
                    accountName,
                    accountKey))
        };

        return blobUriBuilder.ToUri();
    }
}
