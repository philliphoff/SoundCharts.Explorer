using SoundCharts.Explorer.Tiles;
using SoundCharts.Explorer.Tiles.Sources;

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

app.MapGet("/tiles/{z}/{x}/{y}.png",
    (int x, int y, int z) =>
    {
        var tileIndex = new TileIndex(x, y, z);

        var url = HttpTileSets.NoaaQuiltedTileSet(tileIndex);

        return Results.Redirect(url.ToString());
    })
    .WithName("GetTile");

app.Run();
