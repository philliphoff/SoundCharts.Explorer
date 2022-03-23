using System;
using System.Threading;
using System.Threading.Tasks;

namespace SoundCharts.Explorer.Charts.Sources;

public sealed class CachedChartSource : IChartSource
{
    private readonly IChartCache cache;
    private readonly ChartCacheMissDelegate cacheMissDelegate;

    public CachedChartSource(IChartCache cache, IChartSource chartSource)
        : this(cache, chartSource.GetChartAsync)
    {
    }

    public CachedChartSource(IChartCache cache, ChartCacheMissDelegate cacheMissDelegate)
	{
        this.cache = cache ?? throw new ArgumentNullException(nameof(cache));
        this.cacheMissDelegate = cacheMissDelegate ?? throw new ArgumentNullException(nameof(cacheMissDelegate));
	}

    #region IChartSource Members

    public Task<ChartData?> GetChartAsync(Uri name, CancellationToken cancellationToken = default)
    {
        return this.cache.GetChartAsync(name, this.cacheMissDelegate, cancellationToken);
    }

    #endregion
}

