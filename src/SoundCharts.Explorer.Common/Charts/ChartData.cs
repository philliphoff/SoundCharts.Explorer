using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using NauticalCharts;

namespace SoundCharts.Explorer.Charts;

public sealed record ChartData(BsbMetadata Metadata, Func<CancellationToken, Task<Stream>> ImageFactory);
