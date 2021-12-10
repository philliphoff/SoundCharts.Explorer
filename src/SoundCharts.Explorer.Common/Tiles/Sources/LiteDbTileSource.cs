using System;
using System.Threading;
using System.Threading.Tasks;
using LiteDB;

namespace SoundCharts.Explorer.Tiles.Sources;

public class LiteDbTileSource : ITileSource
{
    private readonly string path;

    public LiteDbTileSource(string path)
    {
        this.path = path ?? throw new ArgumentNullException(nameof(path));
    }

    #region ITileSource Members

    public Task<TileData?> GetTileAsync(TileIndex index, CancellationToken cancellationToken = default)
    {
        // TODO: Keep connection open and reuse it.
        using var litedb = new LiteDatabase(this.path);
        
        var tilesLiteDb = litedb.GetCollection<TilesTable>("tiles");

        var result = tilesLiteDb
            .Query()
            .Where(tile => tile.TileColumn == index.Column && tile.TileRow == index.Row && tile.ZoomLevel == index.Zoom)
            .Select(tile => tile.TileData)
            .FirstOrDefault();

        return result is not null
            ? Task.FromResult<TileData?>(new TileData(TileFormat.Png, result))
            : Task.FromResult<TileData?>(null);
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