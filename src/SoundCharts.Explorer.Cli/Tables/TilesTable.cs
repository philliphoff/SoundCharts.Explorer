using LiteDB;
using SQLite;

namespace SoundCharts.Explorer.Cli.Tables;

[Table("tiles")]
public class TilesTable
{
    [BsonField("zoom_level")]
    [Column("zoom_level")]
    public int ZoomLevel { get; set; }

    [BsonField("tile_column")]
    [Column("tile_column")]
    public int TileColumn { get; set; }

    [BsonField("tile_row")]
    [Column("tile_row")]
    public int TileRow { get; set; }

    [BsonField("tile_data")]
    [Column("tile_data")]
    public byte[]? TileData { get; set; }
}
