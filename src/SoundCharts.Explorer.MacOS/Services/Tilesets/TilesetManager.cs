using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using SoundCharts.Explorer.Tiles.Tilesets;

namespace SoundCharts.Explorer.MacOS.Services.Tilesets
{
    internal sealed class TilesetManager : ITilesetManager, IDisposable
	{
        private readonly ITilesetCache cache;
        private readonly ITilesetServiceClient client;

        public TilesetManager(ITilesetCache cache, ITilesetServiceClient client)
		{
            this.cache = cache ?? throw new ArgumentNullException(nameof(cache));
            this.client = client ?? throw new ArgumentNullException(nameof(client));

            this.Tilesets =
                this.cache
                    .Tilesets
                    .SelectMany(
                        async cachedTilesets =>
                        {
                            var availableTilesets = await this.client.GetTilesetsAsync().ConfigureAwait(false);

                            var updatedTilesets = availableTilesets.ToDictionary(tileset => tileset.Id, tileset => new ManagedTileset(tileset.Id, TilesetState.NotDownloaded));

                            foreach (var tileset in cachedTilesets)
                            {
                                if (updatedTilesets.TryGetValue(tileset.Id, out ManagedTileset managedTileset) && managedTileset.State == TilesetState.NotDownloaded)
                                {
                                    updatedTilesets[tileset.Id] = managedTileset with { State = TilesetState.Downloaded };
                                }
                                else
                                {
                                    updatedTilesets.Add(tileset.Id, new ManagedTileset(tileset.Id, TilesetState.Downloaded));
                                }
                            }

                            return updatedTilesets.Values.ToImmutableHashSet();
                        });
		}

        #region ITilesetManager Members

        public IObservable<IImmutableSet<ManagedTileset>> Tilesets { get; }

        public async Task DeleteTilesetAsync(string id, CancellationToken cancellationToken = default)
        {
            await this.cache.RemoveTilesetAsync(id, cancellationToken).ConfigureAwait(false);
        }

        public async Task DownloadTilesetAsync(string id, CancellationToken cancellationToken = default)
        {
            var stream = await this.client.DownloadTilesetAsync(id, cancellationToken).ConfigureAwait(false);

            if (stream is not null)
            {
                await this.cache.AddTilesetAsync(id, stream, cancellationToken);
            }
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
        }

        #endregion
    }
}
