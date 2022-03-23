using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SoundCharts.Explorer.Utilities;

namespace SoundCharts.Explorer.Charts;

public sealed class InMemoryChartCache : ChartCacheBase
{
    private readonly ConcurrentDictionary<Uri, ChartData> charts = new();

    public override Task ClearCacheAsync(CancellationToken cancellationToken = default)
    {
        this.charts.Clear();

        return Task.CompletedTask;
    }

    protected override Task<ChartData?> GetCachedTileAsync(Uri name, CancellationToken cancellationToken)
    {
        return Task.FromResult(this.charts.TryGetValue(name, out ChartData? tileData) ? tileData : null);
    }

    protected override async Task SetCachedTileAsync(Uri name, ChartData data, CancellationToken cancellationToken)
    {
        using var stream = await data.ImageFactory(cancellationToken).ConfigureAwait(false);

        var buffer = await stream.CopyToArrayAsync(cancellationToken).ConfigureAwait(false);

        this.charts.TryAdd(name, new ChartData(data.Metadata, _ => Task.FromResult<Stream>(new MemoryStream(buffer))));
    }
}
