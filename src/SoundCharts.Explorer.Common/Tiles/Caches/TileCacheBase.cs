using System;
using System.Threading;
using System.Threading.Tasks;
using SoundCharts.Explorer.Utilities;

namespace SoundCharts.Explorer.Tiles.Caches
{
	public abstract class TileCacheBase : ITileCache, IDisposable
	{
        private readonly TaskCache<TileIndex, TileData?> tasks = new();

        private bool disposedValue;

        #region ITileCache Members

        public Task<TileData?> GetTileAsync(TileIndex index, TileCacheMissDelegate cacheMissDelegate, CancellationToken cancellationToken = default)
        {
            return this.tasks.GetTaskAsync(
                index,
                async (_, ct) =>
                {
                    var data = await this.GetCachedTileAsync(index, ct).ConfigureAwait(false);

                    if (data is null)
                    {
                        data = await cacheMissDelegate(index, ct).ConfigureAwait(false);

                        if (data is not null)
                        {
                            await this.SetCachedTileAsync(index, data, ct).ConfigureAwait(false);
                        }
                    }

                    return data;
                },
                cancellationToken);
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(disposing: true);

            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    this.tasks.Dispose();
                }

                disposedValue = true;
            }
        }

        #endregion

        protected abstract Task<TileData?> GetCachedTileAsync(TileIndex index, CancellationToken cancellationToken);

        protected abstract Task SetCachedTileAsync(TileIndex index, TileData data, CancellationToken cancellationToken);
    }
}

