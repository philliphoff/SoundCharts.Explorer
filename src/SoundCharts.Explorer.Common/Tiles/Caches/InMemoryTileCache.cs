using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using SoundCharts.Explorer.Utilities;

namespace SoundCharts.Explorer.Tiles.Caches
{
	public sealed class InMemoryTileCache : ITileCache, IDisposable
	{
        private readonly TaskCache<TileIndex, TileData?> taskCache = new ();
        private readonly ConcurrentDictionary<TileIndex, TileData> tiles = new ();

        public InMemoryTileCache()
		{
		}

        #region ITileCache Members

        public Task<TileData?> GetTileAsync(TileIndex index, TileCacheMissDelegate cacheMissDelegate, CancellationToken cancellationToken = default)
        {
            return this.taskCache.GetTaskAsync(
                index,
                async (_, ct) =>
                {
                    if (!this.tiles.TryGetValue(index, out TileData? tileData))
                    {
                        tileData = await cacheMissDelegate(index, ct).ConfigureAwait(false);

                        if (tileData is not null)
                        {
                            this.tiles.TryAdd(index, tileData);
                        }
                    }

                    return tileData;
                },
                cancellationToken);
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            this.taskCache.Dispose();
        }

        #endregion
    }
}

