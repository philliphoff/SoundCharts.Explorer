using System;
using System.Diagnostics;
using System.IO;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using SoundCharts.Explorer.Charts.Sources;

namespace SoundCharts.Explorer.MacOS.Services.Collections;

internal sealed class ChartProcessor : IChartProcessor, IDisposable
{
    private readonly IChartSource chartSource;
    private readonly Subject<ProcessedChart> processedCharts = new ();
    private readonly CancellationTokenSource disposalTokenSource = new ();
    private readonly Channel<Uri> channel = Channel.CreateUnbounded<Uri>(new UnboundedChannelOptions { SingleReader = true });

    public ChartProcessor(IChartSource chartSource)
    {
        this.chartSource = chartSource ?? throw new ArgumentNullException(nameof(chartSource));

        this.ReadSubmittionsAsync()
            .ContinueWith(_ => { });
    }

    #region IChartProcessor Members

    public IObservable<ProcessedChart> ProcessedCharts => this.processedCharts;

    public void SubmitChart(Uri name)
    {
        var result = this.channel.Writer.TryWrite(name);

        Debug.Assert(result, "Writing to an unbounded channel should always succeed.");
    }

    #endregion

    #region IDisposable Members

    public void Dispose()
    {
        if (!this.disposalTokenSource.IsCancellationRequested)
        {
            this.disposalTokenSource.Cancel();

            this.channel.Writer.Complete();
        }
    }

    #endregion

    private async Task ReadSubmittionsAsync()
    {
        try
        {
            while (await this.channel.Reader.WaitToReadAsync(this.disposalTokenSource.Token).ConfigureAwait(false))
            {
                var result = this.channel.Reader.TryRead(out var name);

                Debug.Assert(result, "The sole consumer of a channel should successfully read from the channel after waiting.");
                Debug.Assert(name is not null, "Names should be non-null.");

                if (result && name is not null)
                {
                    var chart = await this.chartSource.GetChartAsync(name, this.disposalTokenSource.Token).ConfigureAwait(false);

                    if (chart is not null && !this.disposalTokenSource.IsCancellationRequested)
                    {
                        this.processedCharts.OnNext(new ProcessedChart(name, chart.Metadata.Name ?? "Unknown Chart"));
                    }
                }
            }
        }
        catch (OperationCanceledException)
        {
            // NOTE: No-op.
        }

        this.processedCharts.Dispose();
        this.disposalTokenSource.Dispose();
    }
}

