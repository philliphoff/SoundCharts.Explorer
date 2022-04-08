using SoundCharts.Explorer.MacOS.Views.SourceList.Objects;

namespace SoundCharts.Explorer.MacOS.Views.SourceList.Model;

internal sealed class ChartCollectionChartItem : ExplorerItem
{
    private readonly string description;
    private readonly string title;

    public ChartCollectionChartItem(string title, string description)
    {
        this.description = description;
        this.title = title;
    }

    public override TreeObject GetObject()
    {
        return new ChartCollectionChartObject(this.title, this.description);
    }
}
