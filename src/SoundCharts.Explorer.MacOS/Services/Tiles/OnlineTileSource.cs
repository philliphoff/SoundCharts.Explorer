using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using SoundCharts.Explorer.Tiles;
using SoundCharts.Explorer.Tiles.Caches;
using SoundCharts.Explorer.Tiles.Sources;

namespace SoundCharts.Explorer.MacOS.Services.Tiles
{
    internal sealed class OnlineTileSource : IObservableTileSource, IDisposable
    {
        private readonly ITileSource tileSource;

        public OnlineTileSource()
        {
            string cacheDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".soundcharts", "explorer", "caches");
            string noaaCacheDirectory = Path.Combine(cacheDirectory, "noaa");

            this.tileSource =
                new TransformedTileSource(
                    TileSourceTransforms.EmptyTileTransformAsync,
                    new CachedTileSource(
                        new FileTileCache(noaaCacheDirectory), // TODO: Dispose of cache.
                        new HttpTileSource(new HttpClient(), HttpTileSets.NoaaQuiltedTileSet)));
        }

        #region IObservableTileSource Members

        public event EventHandler<TilesChangedEventArgs>? TilesChanged;

        public Task<TileData?> GetTileAsync(TileIndex index, CancellationToken cancellationToken = default)
        {
            return this.tileSource.GetTileAsync(index, cancellationToken);
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
        }

        #endregion
    }
}
