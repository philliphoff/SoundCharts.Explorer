using System;
using System.Threading;
using System.Threading.Tasks;
using SoundCharts.Explorer.Utilities;

namespace SoundCharts.Explorer.Charts;

public abstract class ChartCacheBase : IChartCache, IDisposable
{
    private readonly TaskCache<Uri, ChartData?> tasks = new();

    private bool disposedValue;

    #region IChartCache Members

    public Task<ChartData?> GetChartAsync(Uri name, ChartCacheMissDelegate cacheMissDelegate, CancellationToken cancellationToken = default)
    {
        return this.tasks.GetTaskAsync(
            name,
            async (_, ct) =>
            {
                var data = await this.GetCachedTileAsync(name, ct).ConfigureAwait(false);

                if (data is null)
                {
                    data = await cacheMissDelegate(name, ct).ConfigureAwait(false);

                    if (data is not null)
                    {
                        await this.SetCachedTileAsync(name, data, ct).ConfigureAwait(false);
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

    public abstract Task ClearCacheAsync(CancellationToken cancellationToken = default);

    protected abstract Task<ChartData?> GetCachedTileAsync(Uri name, CancellationToken cancellationToken);

    protected abstract Task SetCachedTileAsync(Uri name, ChartData data, CancellationToken cancellationToken);
}
