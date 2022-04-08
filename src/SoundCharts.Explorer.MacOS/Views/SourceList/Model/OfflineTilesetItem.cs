using System;
using System.Threading.Tasks;
using SoundCharts.Explorer.MacOS.Views.SourceList.Objects;

namespace SoundCharts.Explorer.MacOS.Views.SourceList.Model;

internal sealed class OfflineTilesetItem : ExplorerItem
{
    public OfflineTilesetItem(string title, string description, Func<Task>? downloadAsync, bool isDownloadAction)
    {
        this.Description = description;
        this.DownloadAsync = downloadAsync ?? (() => Task.FromResult(true));
        this.IsDownloadAction = isDownloadAction;
        this.Title = title;
    }

    public string Description { get; }

    public Func<Task> DownloadAsync { get; }

    public bool IsDownloadAction { get; }

    public string Title { get; }

    public override TreeObject GetObject()
    {
        return new OfflineTilesetObject(this.Title, this.Description, this.DownloadAsync, this.IsDownloadAction);
    }
}
