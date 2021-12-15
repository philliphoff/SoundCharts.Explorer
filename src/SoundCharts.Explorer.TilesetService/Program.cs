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

app.MapGet("/tilesets", 
    () =>
    {
        var tilesets = new[]
        {
            new Tileset("MBTILES_01", "https://foo.bar/mbtiles/01.mbtiles")
        };

        return tilesets;
    })
    .WithName("GetTilesets");

app.Run();

record Tileset(string Name, string Url);
