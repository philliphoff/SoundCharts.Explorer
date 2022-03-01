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
        private readonly ReaderWriterLockSlim directoryLock = new ReaderWriterLockSlim();

		public FileTileCache(string directory)
		{
            this.directory = directory ?? throw new ArgumentNullException(nameof(directory));
		}

        public override Task ClearCacheAsync(CancellationToken cancellationToken = default)
        {
            return Task.Run(
                () =>
                {
                    this.directoryLock.EnterWriteLock();

                    try
                    {
                        Directory.Delete(this.directory, recursive: true);
                    }
                    catch (DirectoryNotFoundException)
                    {
                        // NOTE: No-op.
                    }
                    finally
                    {
                        this.directoryLock.ExitWriteLock();
                    }
                });
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    this.directoryLock.Dispose();
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        protected override Task<TileData?> GetCachedTileAsync(TileIndex index, CancellationToken cancellationToken)
        {
            return Task.Run(
                () =>
                {
                    var (directory, fileName) = GetFileNameForTile(index);

                    string filePath = Path.Combine(this.directory, directory, fileName + ".png");

                    var format = TileFormat.Png;

                    cancellationToken.ThrowIfCancellationRequested();

                    if (!File.Exists(filePath))
                    {
                        filePath = Path.ChangeExtension(filePath, ".jpg");
                        format = TileFormat.Jpeg;
                    }

                    cancellationToken.ThrowIfCancellationRequested();

                    if (!File.Exists(filePath))
                    {
                        return null;
                    }

                    cancellationToken.ThrowIfCancellationRequested();

                    var data = File.ReadAllBytes(filePath);

                    return new TileData(format, data);
                });
        }

        protected override Task SetCachedTileAsync(TileIndex index, TileData data, CancellationToken cancellationToken)
        {
            var extension = data.Format switch
            {
                TileFormat.Jpeg => ".jpg",
                TileFormat.Png => ".png",
                _ => throw new InvalidOperationException("Unsupported tile format.")
            };

            return Task.Run(
                () =>
                {
                    var (relativeDirectory, fileName) = GetFileNameForTile(index);

                    string directory = Path.Combine(this.directory, relativeDirectory);
                    string filePath = Path.Combine(directory, fileName + extension);

                    this.directoryLock.EnterWriteLock();

                    try
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        Directory.CreateDirectory(directory);

                        cancellationToken.ThrowIfCancellationRequested();

                        File.WriteAllBytes(filePath, data.Data);
                    }
                    finally
                    {
                        this.directoryLock.ExitWriteLock();
                    }
                });
        }

        private static (string relativeDirectory, string fileName) GetFileNameForTile(TileIndex index)
        {
            return (index.Zoom.ToString(CultureInfo.InvariantCulture), $"X{index.Column}-Y{index.Row}");
        }
    }
}

