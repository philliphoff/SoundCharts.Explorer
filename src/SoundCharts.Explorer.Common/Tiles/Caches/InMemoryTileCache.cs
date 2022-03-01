using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using SoundCharts.Explorer.Utilities;

namespace SoundCharts.Explorer.Tiles.Caches
{
	public sealed class InMemoryTileCache : TileCacheBase
	{
        private readonly ConcurrentDictionary<TileIndex, TileData> tiles = new();

        public override Task ClearCacheAsync(CancellationToken cancellationToken = default)
        {
            this.tiles.Clear();

            return Task.CompletedTask;
        }

        protected override Task<TileData?> GetCachedTileAsync(TileIndex index, CancellationToken cancellationToken)
        {
            return Task.FromResult(this.tiles.TryGetValue(index, out TileData? tileData) ? tileData : null);
        }

        protected override Task SetCachedTileAsync(TileIndex index, TileData data, CancellationToken cancellationToken)
        {
            this.tiles.TryAdd(index, data);

            return Task.CompletedTask;
        }
    }
}

