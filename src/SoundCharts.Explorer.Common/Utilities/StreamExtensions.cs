using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SoundCharts.Explorer.Utilities;

public static class StreamExtensions
{
	public static async Task<byte[]> CopyToArrayAsync(this Stream stream, CancellationToken cancellationToken = default)
    {
        var buffer = new byte[stream.Length];

        using (var memoryStream = new MemoryStream(buffer))
        {
            await stream.CopyToAsync(memoryStream, cancellationToken);
        }

        return buffer;
    }
}

