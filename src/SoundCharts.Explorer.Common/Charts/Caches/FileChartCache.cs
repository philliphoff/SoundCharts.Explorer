using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using NauticalCharts;

namespace SoundCharts.Explorer.Charts.Caches
{
    public sealed class FileChartCache : ChartCacheBase
    {
        private readonly string directory;
        private readonly ReaderWriterLockSlim directoryLock = new ReaderWriterLockSlim();

        public FileChartCache(string directory)
        {
            this.directory = directory ?? throw new ArgumentNullException(nameof(directory));
        }

        public override Task ClearCacheAsync(CancellationToken cancellationToken = default)
        {
            return Task.Run(
                () =>
                {
                    if (Directory.Exists(this.directory))
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

        protected override Task<ChartData?> GetCachedTileAsync(Uri name, CancellationToken cancellationToken)
        {
            return Task.Run(
                () =>
                {
                    var (_, metadataPath, imagePath) = GetFileNamesForChart(name);

                    cancellationToken.ThrowIfCancellationRequested();

                    if (!File.Exists(metadataPath))
                    {
                        return null;
                    }

                    cancellationToken.ThrowIfCancellationRequested();

                    if (!File.Exists(imagePath))
                    {
                        return null;
                    }

                    cancellationToken.ThrowIfCancellationRequested();

                    var metadata = JsonSerializer.Deserialize<BsbMetadata>(File.ReadAllText(metadataPath));

                    if (metadata is null)
                    {
                        return null;
                    }

                    return new ChartData(
                        metadata,
                        _ => Task.FromResult<Stream>(File.OpenRead(imagePath)));
                });
        }

        protected override async Task SetCachedTileAsync(Uri name, ChartData data, CancellationToken cancellationToken)
        {
            var imageStream = await data.ImageFactory(cancellationToken).ConfigureAwait(false);

            await Task.Run(
                () =>
                {
                    var (relativePath, metadataPath, imagePath) = GetFileNamesForChart(name);

                    this.directoryLock.EnterWriteLock();

                    try
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        Directory.CreateDirectory(relativePath);

                        cancellationToken.ThrowIfCancellationRequested();

                        File.WriteAllText(metadataPath, JsonSerializer.Serialize(data.Metadata));

                        cancellationToken.ThrowIfCancellationRequested();

                        using var imageFileStream = File.OpenWrite(imagePath);

                        imageStream.CopyTo(imageFileStream);
                    }
                    finally
                    {
                        this.directoryLock.ExitWriteLock();
                    }
                }).ConfigureAwait(false);
        }

        private (string RelativePath, string MetadataPath, string ImagePath) GetFileNamesForChart(Uri name)
        {
            var path = name.LocalPath;
            var cachePath = Path.Join(this.directory, path);

            var relativePath = Path.GetDirectoryName(cachePath);
            var metadataPath = Path.ChangeExtension(cachePath, ".json");
            var imagePath = Path.ChangeExtension(cachePath, ".png");

            return (relativePath, metadataPath, imagePath);
        }
    }
}
