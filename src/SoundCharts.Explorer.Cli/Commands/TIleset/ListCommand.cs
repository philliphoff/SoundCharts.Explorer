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

        this.Add(new Option<bool>("--metadata", "List metadata"));
        this.Add(new Option<bool>("--tiles", "List tiles"));

        this.Handler = CommandHandler.Create<FileInfo, bool, bool>(
            (input, metadata, tiles) =>
            {
                using var litedb = new LiteDatabase(input.FullName);

                bool listAll = !metadata && !tiles;

                if (listAll || metadata)
                {
                    var metadataTable = litedb.GetCollection<MetadataTable>("metadata");

                    foreach (var item in metadataTable.FindAll())
                    {
                        Console.WriteLine("Metadata {0}: {1}", item.Name, item.Value);
                    }
                }

                if (listAll || tiles)
                {
                    var tilesTable = litedb.GetCollection<TilesTable>("tiles")
                        .FindAll()
                        .OrderBy(x => x.TileRow)
                        .OrderBy(x => x.TileColumn)
                        .OrderBy(x => x.ZoomLevel);

                    foreach (var item in tilesTable)
                    {
                        Console.WriteLine("Tile z{0} - x{1} y{2}", item.ZoomLevel, item.TileColumn, item.TileRow);
                    }
                }
            });
    }
}

