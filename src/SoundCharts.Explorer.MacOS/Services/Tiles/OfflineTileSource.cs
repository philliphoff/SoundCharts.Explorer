using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SoundCharts.Explorer.MacOS.Services.Tilesets;
using SoundCharts.Explorer.Tiles;
using SoundCharts.Explorer.Tiles.Sources;

namespace SoundCharts.Explorer.MacOS.Services.Tiles
{
    internal sealed class OfflineTileSource : IObservableTileSource, IDisposable
    {
        private readonly IDisposable cacheSubscription;
        private IEnumerable<LiteDbTileSource> tileSources = Enumerable.Empty<LiteDbTileSource>();

        public OfflineTileSource(ITilesetCache cache, ILoggerFactory loggerFactory)
        {
            this.cacheSubscription =
                cache
                    .Tilesets
                    .Subscribe(
                        tilesets =>
                        {
                            this.ResetSources();

                            this.tileSources = tilesets.Select(tileset => new LiteDbTileSource(tileset.Path, loggerFactory)).ToArray();

                            this.TilesChanged?.Invoke(this, new TilesChangedEventArgs(Enumerable.Empty<TileIndex>()));
                        });
        }

        #region IObservableTileSource Members

        public event EventHandler<TilesChangedEventArgs>? TilesChanged;

        public async Task<TileData?> GetTileAsync(TileIndex index, CancellationToken cancellationToken = default)
        {
            // TODO: Run in parallel and return first real data.
            foreach (var tileSource in this.tileSources)
            {
                var data = await tileSource.GetTileAsync(index, cancellationToken).ConfigureAwait(false);

                if (data is not null)
                {
                    return data;
                }
            }

            return null;
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            this.ResetSources();
        }

        #endregion

        private void ResetSources()
        {
            // TODO: Resolve race if getting images at the same time.
            foreach (var tileSource in this.tileSources)
            {
                tileSource.Dispose();
            }

            this.tileSources = Enumerable.Empty<LiteDbTileSource>();
        }
    }
}
