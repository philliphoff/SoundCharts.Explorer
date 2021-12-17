using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Dapr.Client;
using Dapr.Extensions.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

const string UseEnvironmentVariablesName = "tileset-service-use-environment-variables";
const string ConnectionStringSecretName = "tileset-service-connection-string";
const string AccountNameSecretName = "tileset-service-account-name";
const string AccountKeySecretName = "tileset-service-account-key";

var secretNames = new[]
{
    ConnectionStringSecretName,
    AccountNameSecretName,
    AccountKeySecretName
};

var builder = WebApplication.CreateBuilder(args);

builder.Host.ConfigureAppConfiguration(
    (context, config) =>
    {
        var useEnvironmentVariables = Environment.GetEnvironmentVariable(UseEnvironmentVariablesName);

        if (!String.IsNullOrEmpty(useEnvironmentVariables))
        {
            config.AddInMemoryCollection(secretNames.Select(secretname => new KeyValuePair<string, string>(secretname, Environment.GetEnvironmentVariable(secretname))));
        }
        else
        {
            var daprClient = new DaprClientBuilder().Build();
            var secretDescriptors = secretNames.Select(secretName => new DaprSecretDescriptor(secretName)).ToList();

            config.AddDaprSecretStore("service-secrets", secretDescriptors, daprClient);
        }
    });

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

string connectionString = app.Configuration[ConnectionStringSecretName];

var blobServiceClient = new BlobServiceClient(connectionString);
var containerClient = blobServiceClient.GetBlobContainerClient("tilesets");

app.MapGet("/tilesets", 
    async () =>
    {
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
    })
    .WithName("GetTilesets");

app.MapGet("/tilesets/{id}", 
    async (string id) =>
    {
        var taggedBlob = await blobServiceClient.FindBlobsByTagsAsync($"\"sc-tileset-id\" = '{id}'").FirstOrDefaultAsync();

        if (taggedBlob == null)
        {
            return Results.NotFound();
        }

        var blobContainerClient = blobServiceClient.GetBlobContainerClient(taggedBlob.BlobContainerName);
        var blobClient = blobContainerClient.GetBlobClient(taggedBlob.BlobName);

        var properties = await blobClient.GetPropertiesAsync();

        return Results.Ok(new Tileset(id, taggedBlob.BlobName, "TODO: Get SAS-based URL"));
    })
    .WithName("GetTileset");

app.MapGet("/tilesets/{id}/download", 
    async (string id) =>
    {
        var taggedBlob = await blobServiceClient.FindBlobsByTagsAsync($"\"sc-tileset-id\" = '{id}'").FirstOrDefaultAsync();

        if (taggedBlob == null)
        {
            return Results.NotFound();
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
                    app.Configuration[AccountNameSecretName],
                    app.Configuration[AccountKeySecretName]))
        };

        return Results.Redirect(blobUriBuilder.ToString());
    })
    .WithName("DownloadTileset");

app.Run();

record Tileset(string Id, string Name, string Url);
