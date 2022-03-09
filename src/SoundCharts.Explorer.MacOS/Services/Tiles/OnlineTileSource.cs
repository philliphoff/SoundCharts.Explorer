using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using SoundCharts.Explorer.MacOS.Services.Http;
using SoundCharts.Explorer.Tiles;
using SoundCharts.Explorer.Tiles.Caches;
using SoundCharts.Explorer.Tiles.Sources;

namespace SoundCharts.Explorer.MacOS.Services.Tiles
{
    internal sealed class OnlineTileSource : IObservableTileSource, IDisposable
    {
        private readonly FileTileCache tileCache;
        private readonly ITileSource tileSource;

        public OnlineTileSource(IHttpClientManager httpClientManager)
        {
            if (httpClientManager is null)
            {
                throw new ArgumentNullException(nameof(httpClientManager));
            }

            string cacheDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".soundcharts", "explorer", "caches");
            string noaaCacheDirectory = Path.Combine(cacheDirectory, "noaa");

            this.tileCache = new FileTileCache(noaaCacheDirectory);

            this.tileSource =
                new TransformedTileSource(
                    TileSourceTransforms.EmptyTileTransformAsync,
                    new CachedTileSource(
                        this.tileCache,
                        new HttpTileSource(httpClientManager.CurrentClient, HttpTileSources.NoaaQuiltedTileSet)));
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
