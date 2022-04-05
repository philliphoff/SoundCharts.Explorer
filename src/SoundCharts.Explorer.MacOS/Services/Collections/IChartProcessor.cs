using System;

namespace SoundCharts.Explorer.MacOS.Services.Collections;

// TODO: Include whether processing was successful.
internal sealed record ProcessedChart(Uri Name, string Title);

internal interface IChartProcessor
{
    IObservable<ProcessedChart> ProcessedCharts { get; }

    void SubmitChart(Uri name);
}
