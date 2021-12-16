using Azure.Storage.Blobs;

var builder = WebApplication.CreateBuilder(args);

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

string connectionString = Environment.GetEnvironmentVariable("AZURE_STORAGE_CONNECTION_STRING");

app.MapGet("/tilesets", 
    async () =>
    {
        var blobServiceClient = new BlobServiceClient(connectionString);
        var containerClient = blobServiceClient.GetBlobContainerClient("tilesets");

        var tilesets = new List<Tileset>();

        await foreach (var blob in containerClient.GetBlobsByHierarchyAsync(prefix: "litedb/", delimiter: "/"))
        {
            tilesets.Add(new Tileset(blob.Blob.Name, "TODO: Get SAS-based URL"));
        }

        return tilesets;
    })
    .WithName("GetTilesets");

app.Run();

record Tileset(string Name, string Url);
