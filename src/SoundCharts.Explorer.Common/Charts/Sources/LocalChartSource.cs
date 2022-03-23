using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using NauticalCharts;
using SixLabors.ImageSharp;

namespace SoundCharts.Explorer.Charts.Sources;

public sealed class LocalChartSource : IChartSource
{
    #region IChartSource Members

    public async Task<ChartData?> GetChartAsync(Uri name, CancellationToken cancellationToken = default)
    {
        if (name.Scheme != "file")
        {
            throw new ArgumentException(nameof(name));
        }

        var stream = File.OpenRead(name.LocalPath);
        var chart = await BsbChartReader.ReadChartAsync(stream, cancellationToken).ConfigureAwait(false);

        var metadata = BsbMetadataReader.ReadMetadata(chart.TextSegment);

        return new ChartData(
            metadata,
            async ct =>
            {
                // TODO: Add ToImageAsync()?
                var image = await Task.Run(() => chart.ToImage()).ConfigureAwait(false);

                var stream = new MemoryStream();

                await image.SaveAsPngAsync(stream, ct).ConfigureAwait(false);

                stream.Seek(0, SeekOrigin.Begin);

                return stream;
            });
    }

    #endregion
}
