using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using LiteDB;
using SoundCharts.Explorer.Cli.Tables;

namespace SoundCharts.Explorer.Cli.Commands.Tileset;

internal sealed class ExtractCommand : Command
{
	public ExtractCommand()
		: base("extract", "Extract tile from tileset")
	{
        this.Add(
            new Option<FileInfo>("--input", "Input file")
            {
                IsRequired = true
            });

        this.Add(
            new Option<int>("--zoom-level", "Zoom level")
            {
                IsRequired = true
            });

        this.Add(
            new Option<int>("--tile-column", "Tile column")
            {
                IsRequired = true
            });

        this.Add(
            new Option<int>("--tile-row", "Tile row")
            {
                IsRequired = true
            });

        this.Add(
            new Option<FileInfo>("--output", "Output file")
            {
                IsRequired = true
            });

        this.Handler = CommandHandler.Create<FileInfo, int, int, int, FileInfo>(
            async (input, tileColumn, tileRow, zoomLevel, output) =>
            {
                using var litedb = new LiteDatabase(input.FullName);

                var tile = litedb.GetCollection<TilesTable>("tiles")
                    .FindOne(t => t.ZoomLevel == zoomLevel && t.TileColumn == tileColumn && t.TileRow == tileRow);

                if (tile?.TileData is not null)
                {
                    using var buffer = new MemoryStream(tile.TileData);

                    await File.WriteAllBytesAsync(output.FullName, tile.TileData).ConfigureAwait(false);
                }
                else
                {
                    Console.WriteLine("Tile not found");
                }
            });
    }
}

