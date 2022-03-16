using AppKit;
using NauticalCharts;
using SixLabors.ImageSharp;
using System.IO;
using System.Threading.Tasks;

namespace SoundCharts.Explorer.MacOS.Utils
{
    internal static class BsbChartExtensions
    {
        public static async Task<NSImage> ToNSImageAsync(this BsbChart chart)
        {
            var image = chart.ToImage();

            using var memoryStream = new MemoryStream();

            await image.SaveAsPngAsync(memoryStream);

            memoryStream.Seek(0, SeekOrigin.Begin);

            var nsImage = NSImage.FromStream(memoryStream);

            nsImage.Flipped = true;

            return nsImage;
        }
    }
}