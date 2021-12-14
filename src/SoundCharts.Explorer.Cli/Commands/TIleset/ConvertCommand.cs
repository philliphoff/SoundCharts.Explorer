using System.CommandLine;
using System.CommandLine.Invocation;
using LiteDB;
using SoundCharts.Explorer.Cli.Tables;
using SQLite;

namespace SoundCharts.Explorer.Cli.Commands.Tileset;

internal sealed class ConvertCommand : Command
{
    public ConvertCommand()
        : base("convert", "Convert tileset from MBTiles to LiteTiles format")
    {
        this.Add(
            new Option<FileInfo>("--input", "Input file")
            {
                IsRequired = true
            });
            
        this.Add(
            new Option<FileInfo>("--output", "Output file")
            {
                IsRequired = true
            });

        this.Handler = CommandHandler.Create<FileInfo, FileInfo>(
            (input, output) =>
            {
                var db = new SQLiteConnection(input.FullName);

                var metadata = db.Query<MetadataTable>("SELECT * FROM metadata");
                var tiles = db.Query<TilesTable>("SELECT * FROM tiles");

                if (File.Exists(output.FullName))
                {
                    // TODO: Require "--force" to delete an existing DB.
                    File.Delete(output.FullName);
                }

                using (var litedb = new LiteDatabase(output.FullName))
                {
                    var collection = litedb.GetCollection<MetadataTable>("metadata");

                    foreach (var item in metadata)
                    {
                        collection.Insert(item);
                    }

                    var tilesLiteDb = litedb.GetCollection<TilesTable>("tiles");

                    tilesLiteDb.EnsureIndex(tile => tile.TileIndex, unique: true);

                    foreach (var item in tiles)
                    {
                        item.TileIndex = $"z{item.ZoomLevel}x{item.TileColumn}y{item.TileRow}";

                        tilesLiteDb.Insert(item);
                    }
                }
            });
    }
}
