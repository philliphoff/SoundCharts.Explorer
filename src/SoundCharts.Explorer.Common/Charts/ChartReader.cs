using System.IO;
using System.Threading;
using System.Threading.Tasks;
using NauticalCharts;
using SixLabors.ImageSharp;

namespace SoundCharts.Explorer.Charts;

public static class ChartReader
{
    public static Task<ChartData?> FromPathAsync(string path, CancellationToken cancellationToken = default)
    {
        using var stream = File.OpenRead(path);

        return FromStreamAsync(stream, cancellationToken);
    }

    public static async Task<ChartData?> FromStreamAsync(Stream stream, CancellationToken cancellationToken = default)
    {
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
}
