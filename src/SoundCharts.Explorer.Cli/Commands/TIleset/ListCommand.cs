using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using LiteDB;
using SoundCharts.Explorer.Cli.Tables;

namespace SoundCharts.Explorer.Cli.Commands.Tileset;

internal sealed class ListCommand : Command
{
	public ListCommand()
		: base("list", "List contents of a tileset")
	{
        this.Add(
            new Option<FileInfo>("--input", "Input file")
            {
                IsRequired = true
            });

        this.Handler = CommandHandler.Create<FileInfo>(
            input =>
            {
                using var litedb = new LiteDatabase(input.FullName);

                var metadata = litedb.GetCollection<MetadataTable>("metadata");

                foreach (var item in metadata.FindAll())
                {
                    Console.WriteLine("Metadata {0}: {1}", item.Name, item.Value);
                }

                var tiles = litedb.GetCollection<TilesTable>("tiles")
                    .FindAll()
                    .OrderBy(x => x.ZoomLevel)
                    .OrderBy(x => x.TileColumn)
                    .OrderBy(x => x.TileRow);

                foreach (var item in tiles)
                {
                    Console.WriteLine("Tile z{0} - x{1} y{2}", item.ZoomLevel, item.TileColumn, item.TileRow);
                }
            });
    }
}

