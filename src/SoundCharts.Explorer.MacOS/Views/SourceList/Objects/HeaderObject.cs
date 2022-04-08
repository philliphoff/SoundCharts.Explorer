namespace SoundCharts.Explorer.MacOS.Views.SourceList.Objects;

internal sealed class HeaderObject : TreeObject
{
    public HeaderObject(string title)
        : base(isExpandable: true)
    {
        this.Title = title;
    }

    public string Title { get; }
}
