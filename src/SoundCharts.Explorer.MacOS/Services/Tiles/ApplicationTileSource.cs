using System;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using SoundCharts.Explorer.MacOS.Services.State;
using SoundCharts.Explorer.Tiles;
using SoundCharts.Explorer.Tiles.Caches;
using SoundCharts.Explorer.Tiles.Sources;

namespace SoundCharts.Explorer.MacOS.Services.Tiles
{
    internal sealed class ApplicationTileSource : IObservableTileSource, IDisposable
    {
        private readonly IObservableTileSource offlineTileSource;
        private readonly IObservableTileSource onlineTileSource;
        private readonly IDisposable stateSubscription;
        private ITileSource? tileSource;

        public ApplicationTileSource(
            IApplicationStateManager applicationStateManager,
            OfflineTileSource offlineTileSource,
            OnlineTileSource onlineTileSource)
        {
            if (applicationStateManager is null)
            {
                throw new ArgumentNullException(nameof(applicationStateManager));
            }

            this.offlineTileSource = offlineTileSource ?? throw new ArgumentNullException(nameof(offlineTileSource));
            this.onlineTileSource = onlineTileSource ?? throw new ArgumentNullException(nameof(onlineTileSource));

            this.offlineTileSource.TilesChanged += this.OnTilesChanged;
            this.onlineTileSource.TilesChanged += this.OnTilesChanged;

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
                            string displayCacheDirectory = Path.Combine(cacheDirectory, "display", useOnline ? "online" : "offline");

                            this.tileSource =
                                new CachedTileSource(
                                    new InMemoryTileCache(), // TODO: Dispose of cache.
                                    new CachedTileSource(
                                        new FileTileCache(displayCacheDirectory), // TODO: Dispose of cache.
                                        new TransformedTileSource(
                                            TileSourceTransforms.OverzoomedTransform,
                                            new TransformedTileSource(
                                                TileSourceTransforms.EmptyTileTransformAsync,
                                                useOnline ? onlineTileSource : offlineTileSource))));


                            this.NotifyTilesChanged();
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
            this.offlineTileSource.TilesChanged -= this.OnTilesChanged;
            this.onlineTileSource.TilesChanged -= this.OnTilesChanged;

            this.stateSubscription.Dispose();
        }

        #endregion

        private void OnTilesChanged(object sender, TilesChangedEventArgs e)
        {
            this.NotifyTilesChanged();
        }

        private void NotifyTilesChanged()
        {
            this.TilesChanged?.Invoke(this, new TilesChangedEventArgs(Enumerable.Empty<TileIndex>()));
        }
    }
}
