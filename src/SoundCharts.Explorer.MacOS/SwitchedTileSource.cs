using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SoundCharts.Explorer.MacOS.Services.State;
using SoundCharts.Explorer.Tiles;
using SoundCharts.Explorer.Tiles.Caches;
using SoundCharts.Explorer.Tiles.Sources;

namespace SoundCharts.Explorer.MacOS
{
    internal sealed class SwitchedTileSource : IObservableTileSource, IDisposable
    {
        private readonly IDisposable stateSubscription;
        private ITileSource? tileSource;

        public SwitchedTileSource(IApplicationStateManager applicationStateManager, ILoggerFactory loggerFactory)
        {
            this.stateSubscription =
                applicationStateManager
                    .CurrentState
                    .Select(state => state.State?.OfflineTilesets?.Enabled)
                    .DistinctUntilChanged()
                    .Subscribe(
                        enabled =>
                        {
                            bool useOnline = enabled != true;

                            string cacheDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".soundcharts", "explorer", "caches");
                            string displayCacheDirectory = Path.Combine(cacheDirectory, "display");
                            string noaaCacheDirectory = Path.Combine(cacheDirectory, "noaa");

                            if (useOnline)
                            {
                                this.tileSource =
                                    new CachedTileSource(
                                        new InMemoryTileCache(), // TODO: Dispose of cache.
            					        new CachedTileSource(
            						        new FileTileCache(displayCacheDirectory), // TODO: Dispose of cache.
            						        new TransformedTileSource(
            							        TileSourceTransforms.OverzoomedTransform,
            							        new TransformedTileSource(
            								        TileSourceTransforms.EmptyTileTransformAsync,
                                                    new CachedTileSource(
                                            	        new FileTileCache(noaaCacheDirectory), // TODO: Dispose of cache.
                                            	        new HttpTileSource(new HttpClient(), HttpTileSets.NoaaQuiltedTileSet))))));
                            }
                            else
                            {
                                this.tileSource =
                                    new CachedTileSource(
                                        new InMemoryTileCache(), // TODO: Dispose of cache.
                                            new LiteDbTileSource("/Users/phoff/Downloads/litedb/MBTILES_06.litedb", loggerFactory));
                            }

                            this.TilesChanged?.Invoke(this, new TilesChangedEventArgs(Enumerable.Empty<TileIndex>()));
                        });
        }

        #region IObservableTileSource Members

        public event EventHandler<TilesChangedEventArgs>? TilesChanged;

        public Task<TileData?> GetTileAsync(TileIndex index, CancellationToken cancellationToken = default)
        {
            // The tileSource can be replaced at any time, so copy the reference...
            var currentTileSource = this.tileSource;

            return currentTileSource is not null
                ? currentTileSource.GetTileAsync(index, cancellationToken)
                : Task.FromResult<TileData?>(null);
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            this.stateSubscription.Dispose();
        }

        #endregion
    }
}
