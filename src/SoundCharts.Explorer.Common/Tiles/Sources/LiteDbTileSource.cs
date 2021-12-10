using System;
using System.Threading;
using System.Threading.Tasks;
using LiteDB;

namespace SoundCharts.Explorer.Tiles.Sources;

public class LiteDbTileSource : ITileSource, IDisposable
{
    private readonly LiteDatabase db;
    private readonly ILiteCollection<TilesTable> tiles;

    public LiteDbTileSource(string path)
    {
        if (path is null)
        {
            throw new ArgumentNullException(nameof(path));
        }

        this.db = new LiteDatabase($"Filename={path};ReadOnly=true;Connection=direct");
        this.tiles = this.db.GetCollection<TilesTable>("tiles");
    }

    #region ITileSource Members

    public Task<TileData?> GetTileAsync(TileIndex index, CancellationToken cancellationToken = default)
    {
        return Task.Run(
            () =>
            {
                var result = tiles.FindOne(tile => tile.TileColumn == index.Column && tile.TileRow == index.Row && tile.ZoomLevel == index.Zoom);

                return result?.TileData is not null
                    ? new TileData(TileFormat.Png, result.TileData)
                    : null;
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