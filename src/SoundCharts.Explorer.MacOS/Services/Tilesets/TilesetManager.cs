using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using SoundCharts.Explorer.Tiles.Tilesets;

namespace SoundCharts.Explorer.MacOS.Services.Tilesets
{
	internal sealed class TilesetManager : ITilesetManager, IDisposable
	{
        private readonly ITilesetCache cache;
        private readonly ITilesetServiceClient client;

        private IDictionary<string, ManagedTileset>? tilesets;
        private readonly Subject<IImmutableSet<ManagedTileset>> subject = new Subject<IImmutableSet<ManagedTileset>>();
        private readonly IDisposable cacheSubscription;

        public TilesetManager(ITilesetCache cache, ITilesetServiceClient client)
		{
            this.cache = cache ?? throw new ArgumentNullException(nameof(cache));
            this.client = client ?? throw new ArgumentNullException(nameof(client));

            this.cacheSubscription =
                this.cache
                    .Tilesets
                    .SelectMany(
                        cachedTilesets =>
                        {
                            // TODO: Update tilesets.

                            return Task.FromResult(true);
                        })
                    .Subscribe();
		}

        #region ITilesetManager Members

        public IObservable<IImmutableSet<ManagedTileset>> Tilesets => this.subject;

        public async Task DeleteTilesetAsync(string id, CancellationToken cancellationToken = default)
        {
            await this.cache.RemoveTilesetAsync(id, cancellationToken).ConfigureAwait(false);
        }

        public async Task DownloadTilesetAsync(string id, CancellationToken cancellationToken = default)
        {
            var stream = await this.client.DownloadTilesetAsync(id, cancellationToken).ConfigureAwait(false);

            await this.cache.AddTilesetAsync(id, stream, cancellationToken);
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            this.subject.Dispose();
            this.cacheSubscription.Dispose();
        }

        #endregion

        private async Task<IImmutableSet<ManagedTileset>> UpdateTilesetsAsync(IImmutableSet<CachedTileset> cachedTilesets)
        {
            // TODO: Need appropriate locking.
            // TODO: Add cancellation?
            var availableTilesets = await this.client.GetTilesetsAsync().ConfigureAwait(false);

            var updatedTilesets = availableTilesets.ToDictionary(tileset => tileset.Id, tileset => new ManagedTileset(tileset.Id, TilesetState.NotDownloaded));

            foreach (var tileset in cachedTilesets)
            {
                if (this.tilesets.TryGetValue(tileset.Id, out ManagedTileset managedTileset) && managedTileset.State == TilesetState.NotDownloaded)
                {
                    this.tilesets[tileset.Id] = managedTileset with { State = TilesetState.Downloaded };
                }
                else
                {
                    this.tilesets.Add(tileset.Id, new ManagedTileset(tileset.Id, TilesetState.Downloaded));
                }
            }

            return updatedTilesets.Values.ToImmutableHashSet();
        }
    }
}
