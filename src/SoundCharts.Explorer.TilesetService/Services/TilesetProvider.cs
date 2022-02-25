using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using LiteDB;

namespace SoundCharts.Explorer.TilesetService.Services;

internal sealed class TilesetProvider : ITilesetProvider
{
    private readonly string connectionString;
    private readonly string accountName;
    private readonly string accountKey;

    public TilesetProvider(IConfiguration configuration)
    {
        this.connectionString = configuration[Constants.Secrets.ConnectionStringSecretName];
        this.accountName = configuration[Constants.Secrets.AccountNameSecretName];
        this.accountKey = configuration[Constants.Secrets.AccountKeySecretName];
    }

    public async Task<IEnumerable<Tileset>> GetTilesets()
    {
        var blobServiceClient = new BlobServiceClient(this.connectionString);
        var containerClient = blobServiceClient.GetBlobContainerClient("tilesets");

        var tilesets = new List<Tileset>();

        await foreach (var hierarchyItem in containerClient.GetBlobsByHierarchyAsync(
            traits: BlobTraits.Metadata | BlobTraits.Tags,
            prefix: "litedb/",
            delimiter: "/"))
        {
            string id = hierarchyItem.Blob.Tags["sc-tileset-id"];

            hierarchyItem.Blob.Metadata.TryGetValue("sctilesetdescription", out string? description);
            hierarchyItem.Blob.Metadata.TryGetValue("sctilesetname", out string? name);

            tilesets.Add(new Tileset(id, name ?? hierarchyItem.Blob.Name, description, "TODO: Get SAS-based URL"));
        }

        return tilesets;
    }

    public async Task<Tileset?> GetTilesetById(string id)
    {
        var blobServiceClient = new BlobServiceClient(this.connectionString);
        var taggedBlob = await blobServiceClient.FindBlobsByTagsAsync($"\"sc-tileset-id\" = '{id}'").FirstOrDefaultAsync();

        if (taggedBlob is null)
        {
            return null;
        }

        var blobContainerClient = blobServiceClient.GetBlobContainerClient(taggedBlob.BlobContainerName);
        var blobClient = blobContainerClient.GetBlobClient(taggedBlob.BlobName);

        var properties = await blobClient.GetPropertiesAsync();

        properties.Value.Metadata.TryGetValue("sctilesetdescription", out string? description);
        properties.Value.Metadata.TryGetValue("sctilesetname", out string? name);

        return new Tileset(id, name ?? taggedBlob.BlobName, description, "TODO: Get SAS-based URL");
    }

    public async Task<Uri?> GetTilesetDownloadUriById(string id)
    {
        var blobServiceClient = new BlobServiceClient(this.connectionString);
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
                    this.accountName,
                    this.accountKey))
        };

        return blobUriBuilder.ToUri();
    }

    public async Task UpdateTilesetMetadata(string id)
    {
        var blobServiceClient = new BlobServiceClient(this.connectionString);
        var taggedBlob = await blobServiceClient.FindBlobsByTagsAsync($"\"sc-tileset-id\" = '{id}'").FirstOrDefaultAsync();

        if (taggedBlob is null)
        {
            return;
        }

        var blobContainerClient = blobServiceClient.GetBlobContainerClient(taggedBlob.BlobContainerName);
        var blobClient = blobContainerClient.GetBlobClient(taggedBlob.BlobName);

        string tempFilePath = Path.GetTempFileName();

        using (var fileStream = File.OpenWrite(tempFilePath))
        {
            await blobClient.DownloadToAsync(fileStream);
        }

        IDictionary<string, string> metadata;

        try
        {
            using (var litedb = new LiteDatabase(tempFilePath))
            {
                metadata =
                    litedb
                        .GetCollection<MetadataTable>("metadata")
                        .FindAll()
                        .ToDictionary(m => m.Name!, m => m.Value!);
            }
        }
        finally
        {
            File.Delete(tempFilePath);
        }

        metadata.TryGetValue("description", out string? description);
        metadata.TryGetValue("name", out string? name);

        // NOTE: Metadata keys must be adhere to rules for naming C# identifiers.

        await blobClient.SetMetadataAsync(
            new Dictionary<string, string>
            {
                { "sctilesetdescription", description ?? String.Empty },
                { "sctilesetname", name ?? String.Empty }
            });
    }

    private sealed class MetadataTable
    {
        [BsonField("name")]
        public string? Name { get; set; }

        [BsonField("value")]
        public string? Value { get; set; }
    }
}
