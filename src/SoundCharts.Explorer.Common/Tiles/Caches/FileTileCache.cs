using System;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SoundCharts.Explorer.Tiles.Caches
{
	public sealed class FileTileCache : TileCacheBase
	{
        private readonly string directory;

		public FileTileCache(string directory)
		{
            this.directory = directory ?? throw new ArgumentNullException(nameof(directory));
		}

        protected override async Task<TileData?> GetCachedTileAsync(TileIndex index, CancellationToken cancellationToken)
        {
            var (directory, fileName) = GetFileNameForTile(index);

            string filePath = Path.Combine(this.directory, directory, fileName + ".png");

            var format = TileFormat.Png;

            if (!File.Exists(filePath))
            {
                filePath = Path.ChangeExtension(filePath, ".jpg");
                format = TileFormat.Jpeg;
            }

            if (!File.Exists(filePath))
            {
                return null;
            }

            var data = await File.ReadAllBytesAsync(filePath, cancellationToken).ConfigureAwait(false);

            return new TileData(format, data);
        }

        protected override async Task SetCachedTileAsync(TileIndex index, TileData data, CancellationToken cancellationToken)
        {
            var extension = data.Format switch
            {
                TileFormat.Jpeg => ".jpg",
                TileFormat.Png => ".png",
                _ => throw new InvalidOperationException("Unsupported tile format.")
            };

            var (relativeDirectory, fileName) = GetFileNameForTile(index);

            string directory = Path.Combine(this.directory, relativeDirectory);

            Directory.CreateDirectory(directory);

            string filePath = Path.Combine(directory, fileName + extension);

            await File.WriteAllBytesAsync(filePath, data.Data, cancellationToken).ConfigureAwait(false);
        }

        private static (string relativeDirectory, string fileName) GetFileNameForTile(TileIndex index)
        {
            return (index.Zoom.ToString(CultureInfo.InvariantCulture), $"X{index.Column}-Y{index.Row}");
        }
    }
}

