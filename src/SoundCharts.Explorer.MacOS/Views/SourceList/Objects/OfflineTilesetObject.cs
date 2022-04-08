using System;
using System.Threading.Tasks;

namespace SoundCharts.Explorer.MacOS.Views.SourceList.Objects;

internal sealed class OfflineTilesetObject : TreeObject
{
    public OfflineTilesetObject(string title, string description, Func<Task>? downloadAsync, bool isDownloadAction)
    {
        this.TilesetDescription = description;
        this.DownloadAsync = downloadAsync ?? (() => Task.FromResult(true));
        this.IsDownloadAction = isDownloadAction;
        this.Title = title;
    }

    public string TilesetDescription { get; }

    public Func<Task> DownloadAsync { get; }

    public bool IsDownloadAction { get; }

    public string Title { get; }
}

