using System;
using System.Threading;
using System.Threading.Tasks;

namespace SoundCharts.Explorer.Charts;

public delegate Task<ChartData?> ChartCacheMissDelegate(Uri name, CancellationToken cancellationToken = default);

public interface IChartCache
{
	Task<ChartData?> GetChartAsync(Uri name, ChartCacheMissDelegate cacheMissDelegate, CancellationToken cancellationToken = default);
}
