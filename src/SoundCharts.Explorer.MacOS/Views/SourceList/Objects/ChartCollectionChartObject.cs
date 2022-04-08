namespace SoundCharts.Explorer.MacOS.Views.SourceList.Objects;

internal sealed class ChartCollectionChartObject : TreeObject
{
    public ChartCollectionChartObject(string title, string chartDescription)
        : base()
    {
        this.ChartDescription = chartDescription;
        this.Title = title;
    }

    public string ChartDescription { get; }

    public string Title { get; }
}
