using System;
using System.Threading;
using System.Threading.Tasks;

namespace SoundCharts.Explorer.Tiles.Sources
{
	public sealed class CachedTileSource : ITileSource
	{
		private readonly ITileCache tileCache;
		private readonly TileCacheMissDelegate cacheMissDelegate;

		public CachedTileSource(ITileCache tileCache, ITileSource tileSource)
			: this(tileCache, tileSource.GetTileAsync)
		{
		}

		public CachedTileSource(ITileCache tileCache, TileCacheMissDelegate cacheMissDelegate)
		{
			this.tileCache = tileCache ?? throw new ArgumentNullException(nameof(tileCache));
			this.cacheMissDelegate = cacheMissDelegate ?? throw new ArgumentNullException(nameof(cacheMissDelegate));
		}

        #region ITileSource Members

        public Task<TileData?> GetTileAsync(TileIndex index, CancellationToken cancellationToken = default)
        {
			return this.tileCache.GetTileAsync(index, this.cacheMissDelegate, cancellationToken);
        }

        #endregion
    }
}

