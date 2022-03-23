using System.Threading;
using System.Threading.Tasks;
using AppKit;
using SoundCharts.Explorer.Charts;

namespace SoundCharts.Explorer.MacOS.Utils;

internal static class ChartDataExtensions
{
    public static async Task<NSImage> ToNSImageAsync(this ChartData chart, CancellationToken cancellationToken = default)
    {
        using var stream = await chart.ImageFactory(cancellationToken).ConfigureAwait(false);

        var image = NSImage.FromStream(stream);

        image.Flipped = true;

        return image;
    }
}

