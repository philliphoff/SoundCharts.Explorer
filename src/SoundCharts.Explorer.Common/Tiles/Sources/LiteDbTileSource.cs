using System;
using System.Threading;
using System.Threading.Tasks;
using LiteDB;
using Microsoft.Extensions.Logging;

namespace SoundCharts.Explorer.Tiles.Sources;

public class LiteDbTileSource : ITileSource, IDisposable
{
    private readonly LiteDatabase db;
    private readonly ILogger<LiteDbTileSource>? logger;
    private readonly ILiteCollection<TilesTable> tiles;

    public LiteDbTileSource(string path, ILoggerFactory? loggerFactory = null)
    {
        if (path is null)
        {
            throw new ArgumentNullException(nameof(path));
        }

        this.db = new LiteDatabase($"Filename={path};ReadOnly=true;Connection=direct");
        this.tiles = this.db.GetCollection<TilesTable>("tiles");

        this.logger = loggerFactory?.CreateLogger<LiteDbTileSource>();
    }

    #region ITileSource Members

    public Task<TileData?> GetTileAsync(TileIndex index, CancellationToken cancellationToken = default)
    {
        return Task.Run(
            () =>
            {
                using var scope = this.logger?.BeginScope("Getting tile at index {Index}...", index);

                var result = tiles.FindOne(tile => tile.TileColumn == index.Column && tile.TileRow == index.Row && tile.ZoomLevel == index.Zoom);

                if (result?.TileData is not null)
                {
                    this.logger?.LogInformation("Found tile.");

                    return new TileData(TileFormat.Png, result.TileData);
                }
                else
                {
                    this.logger?.LogInformation("Tile not found.");

                    return null;
                }
            });
    }

    #endregion

    #region IDisposable Members

    public void Dispose()
    {
        this.db.Dispose();
    }

    #endregion

    public sealed class TilesTable
    {
        [BsonField("zoom_level")]
        public int ZoomLevel { get; set; }

        [BsonField("tile_column")]
        public int TileColumn { get; set; }

        [BsonField("tile_row")]
        public int TileRow { get; set; }

        [BsonField("tile_data")]
        public byte[]? TileData { get; set; }
    }
}