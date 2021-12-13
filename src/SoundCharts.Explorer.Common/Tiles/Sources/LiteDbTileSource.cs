using System;
using System.Threading;
using System.Threading.Tasks;
using LiteDB;
using Microsoft.Extensions.Logging;

namespace SoundCharts.Explorer.Tiles.Sources;

public class LiteDbTileSource : ITileSource, IDisposable
{
    private readonly Lazy<TileBounds?> bounds;
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

        this.bounds = new(
            () =>
            {
                var metadata = this.db.GetCollection<MetadataTable>("metadata");
                var boundsMetadata = metadata.FindOne(m => m.Name == "bounds");

                if (boundsMetadata?.Value is not null)
                {
                    string[] boundsParts = boundsMetadata.Value.Split(",");

                    if (boundsParts.Length == 4
                        && Double.TryParse(boundsParts[0], out double left)
                        && Double.TryParse(boundsParts[1], out double bottom)
                        && Double.TryParse(boundsParts[2], out double right)
                        && Double.TryParse(boundsParts[3], out double top))
                    {
                        return new TileBounds(
                            new TileCoordinate(bottom, left),
                            new TileCoordinate(top, right));
                    }
                }

                return null;
            });
    }

    #region ITileSource Members

    public async Task<TileData?> GetTileAsync(TileIndex index, CancellationToken cancellationToken = default)
    {
        this.logger?.LogInformation("Getting tile at {Index}...", index);

        var bounds = this.bounds.Value;

        if (bounds is not null)
        {
            var tileBounds = index.GetBounds();

            if (!tileBounds.Overlaps(bounds))
            {
                this.logger?.LogInformation("Tile outside of tileset bounds.");

                return null;
            }
        }

        this.logger?.LogInformation("Tile within tileset bounds; getting tile...");

        return await Task.Run(
            () =>
            {
                // In TMS schema (used by MBTiles), the Y-axis is reversed from the standard Google tiling scheme.
                int row = (1 << index.Zoom) - 1 - index.Row;

                var result = tiles.FindOne(tile => tile.TileColumn == index.Column && tile.TileRow == row && tile.ZoomLevel == index.Zoom);

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
            }).ConfigureAwait(false);
    }

    #endregion

    #region IDisposable Members

    public void Dispose()
    {
        this.db.Dispose();
    }

    #endregion

    private sealed class MetadataTable
    {
        [BsonField("name")]
        public string? Name { get; set; }

        [BsonField("value")]
        public string? Value { get; set; }
    }

    private sealed class TilesTable
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