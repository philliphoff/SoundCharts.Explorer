using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using SoundCharts.Explorer.Utilities;

namespace SoundCharts.Explorer.MacOS.Services.Tilesets
{
	internal sealed class TilesetCache : ITilesetCache
	{
        private readonly string tilesetCacheDirectory;
        private readonly IObservable<IImmutableSet<CachedTileset>> tilesets;

        public TilesetCache()
		{
            string cacheDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".soundcharts", "explorer", "caches");

            this.tilesetCacheDirectory = Path.Combine(cacheDirectory, "tilesets");

            this.tilesets = Observable.Defer(
                () =>
                {
                    Directory.CreateDirectory(this.tilesetCacheDirectory);

                    return FileSystem
                        .CreateDirectoryObserver(this.tilesetCacheDirectory, "*.litedb")
                        .Select(files => files.Select(file => new CachedTileset(Path.GetFileNameWithoutExtension(file), file)).ToImmutableHashSet());
                });
        }

        #region ITilesetCache Members

        public IObservable<IImmutableSet<CachedTileset>> Tilesets => this.tilesets;

        public async Task AddTilesetAsync(string id, Stream stream, CancellationToken cancellationToken = default)
        {
            string tempFilename = Path.Combine(this.tilesetCacheDirectory, $"{id}.litedb.temp");

            Directory.CreateDirectory(this.tilesetCacheDirectory);

            using var file = File.OpenWrite(tempFilename);

            await stream.CopyToAsync(file).ConfigureAwait(false);

            string finalFileName = Path.Combine(this.tilesetCacheDirectory, $"{id}.litedb");

            File.Move(tempFilename, finalFileName);
        }

        public Task RemoveTilesetAsync(string id, CancellationToken cancellationToken = default)
        {
            return Task.Run(
                () =>
                {
                    string finalFileName = Path.Combine(this.tilesetCacheDirectory, $"{id}.litedb");
                    File.Delete(finalFileName);
                });
        }

        #endregion
    }
}

