using SoundCharts.Explorer.TilesetService;
using SoundCharts.Explorer.TilesetService.Services;

var secretNames = new[]
{
    Constants.Secrets.ConnectionStringSecretName,
    Constants.Secrets.AccountNameSecretName,
    Constants.Secrets.AccountKeySecretName
};

var builder = WebApplication.CreateBuilder(args);

builder.Host.ConfigureServices(
    (context, services) =>
    {
        services.AddSingleton<ITilesetProvider, TilesetProvider>();
    });

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseHttpLogging();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

var tilesetProvider = app.Services.GetRequiredService<ITilesetProvider>();

app.MapGet("/tilesets", 
    () =>
    {
        return tilesetProvider.GetTilesets();
    })
    .WithName("GetTilesets");

app.MapMethods(
    "/tilesets/metadata",
    new[] {  "PATCH" },
    async () =>
    {
        var tilesets = await tilesetProvider.GetTilesets();

        foreach (var tileset in tilesets)
        {
            await tilesetProvider.UpdateTilesetMetadata(tileset.Id);
        }

        return Results.NoContent();
    })
    .WithName("UpdateTilesetsMetadata");

app.MapGet("/tilesets/{id}", 
    async (string id) =>
    {
        var tileset = await tilesetProvider.GetTilesetById(id);

        return tileset is not null
            ? Results.Ok(tileset)
            : Results.NotFound();
    })
    .WithName("GetTileset");

app.MapGet("/tilesets/{id}/download", 
    async (string id) =>
    {
        var uri = await tilesetProvider.GetTilesetDownloadUriById(id);

        return uri is not null
            ? Results.Redirect(uri.ToString())
            : Results.NotFound();
    })
    .WithName("DownloadTileset");

app.MapMethods(
    "/tilesets/{id}/metadata",
    new[] {  "PATCH" },
    async (string id) =>
    {
        await tilesetProvider.UpdateTilesetMetadata(id);

        return Results.NoContent();
    })
    .WithName("UpdateTilesetMetadata");

app.Run();
