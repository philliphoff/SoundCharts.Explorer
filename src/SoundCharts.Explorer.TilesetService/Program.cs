using Dapr.Client;
using Dapr.Extensions.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using SoundCharts.Explorer.TilesetService;

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

app.MapGet("/tilesets", 
    () =>
    {
        return Tilesets.GetTilesets(connectionString);
    })
    .WithName("GetTilesets");

app.MapGet("/tilesets/{id}", 
    async (string id) =>
    {
        var tileset = await Tilesets.GetTilesetById(id, connectionString);

        return tileset is not null
            ? Results.Ok(tileset)
            : Results.NotFound();
    })
    .WithName("GetTileset");

app.MapGet("/tilesets/{id}/download", 
    async (string id) =>
    {
        var uri = await Tilesets.GetTilesetDownloadUriById(id, connectionString, app.Configuration[AccountNameSecretName], app.Configuration[AccountKeySecretName]);

        return uri is not null
            ? Results.Redirect(uri.ToString())
            : Results.NotFound();
    })
    .WithName("DownloadTileset");

app.Run();
