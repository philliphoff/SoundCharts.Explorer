using Dapr.Client;
using Dapr.Extensions.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using SoundCharts.Explorer.TilesetService;
using SoundCharts.Explorer.TilesetService.Services;

var secretNames = new[]
{
    Constants.Secrets.ConnectionStringSecretName,
    Constants.Secrets.AccountNameSecretName,
    Constants.Secrets.AccountKeySecretName
};

var builder = WebApplication.CreateBuilder(args);

builder.Host.ConfigureAppConfiguration(
    (context, config) =>
    {
        var useEnvironmentVariables = Environment.GetEnvironmentVariable(Constants.Secrets.UseEnvironmentVariablesName);

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

app.Run();
