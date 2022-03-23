using System;
using System.Threading;
using System.Threading.Tasks;

namespace SoundCharts.Explorer.Charts.Sources;

public interface IChartSource
{
    Task<ChartData?> GetChartAsync(Uri name, CancellationToken cancellationToken = default);
}
