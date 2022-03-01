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
        private FileTileCache? fileTileCache;
        private readonly InMemoryTileCache inMemoryCache = new InMemoryTileCache();
        private readonly IObservableTileSource offlineTileSource;
        private readonly IObservableTileSource onlineTileSource;
        private readonly IDisposable stateSubscription;
        private ITileSource? tileSource;
        private readonly SemaphoreSlim updateLock = new SemaphoreSlim(1, 1);
        private bool useOnline;

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
                    .SelectMany(
                        async enabled =>
                        {
                            this.useOnline = enabled != true;

                            await this.UpdateAsync();

                            return enabled;
                        })
                    .Subscribe(
                        _ =>
                        {
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

            this.fileTileCache?.Dispose();
            this.inMemoryCache.Dispose();

            this.updateLock.Dispose();
        }

        #endregion

        private void OnTilesChanged(object sender, TilesChangedEventArgs e)
        {
            // TODO: Trap errors.
            this.UpdateAsync()
                .ContinueWith(_ => { });
        }

        private void NotifyTilesChanged()
        {
            this.TilesChanged?.Invoke(this, new TilesChangedEventArgs(Enumerable.Empty<TileIndex>()));
        }

        private async Task UpdateAsync()
        {
            string cacheDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".soundcharts", "explorer", "caches");
            string displayCacheDirectory = Path.Combine(cacheDirectory, "display", useOnline ? "online" : "offline");

            this.fileTileCache?.Dispose();

            this.fileTileCache = new FileTileCache(displayCacheDirectory);

            await this.fileTileCache.ClearCacheAsync();

            await this.inMemoryCache.ClearCacheAsync();

            this.tileSource =
                new CachedTileSource(
                    this.inMemoryCache,
                    new CachedTileSource(
                        this.fileTileCache,
                        new TransformedTileSource(
                            TileSourceTransforms.OverzoomedTransform,
                            new TransformedTileSource(
                                TileSourceTransforms.EmptyTileTransformAsync,
                                useOnline ? onlineTileSource : offlineTileSource))));

            this.NotifyTilesChanged();
        }
    }
}
