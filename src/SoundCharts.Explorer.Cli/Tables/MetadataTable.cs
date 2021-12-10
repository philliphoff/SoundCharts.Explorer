using LiteDB;
using SQLite;

namespace SoundCharts.Explorer.Cli.Tables;

[Table("metadata")]
public class MetadataTable
{
    [BsonField("name")]
    [Column("name")]
    public string? Name { get; set; }

    [BsonField("value")]
    [Column("value")]
    public string? Value { get; set; }
}
