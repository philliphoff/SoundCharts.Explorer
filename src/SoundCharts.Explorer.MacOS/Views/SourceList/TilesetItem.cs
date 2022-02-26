using System;
using System.Threading.Tasks;

namespace SoundCharts.Explorer.MacOS.Views.SourceList
{
    internal sealed class TilesetItem : SourceListItem
    {
        public TilesetItem(string title, string description, Func<Task>? downloadAsync, bool isDowloadAction)
            : base(title)
        {
            this.Description = description;
            this.DownloadAsync = downloadAsync ?? (() => Task.FromResult(true));
            this.IsDownloadAction = isDowloadAction;
        }

        public new string Description { get; }

        public Func<Task> DownloadAsync { get; }

        public bool IsDownloadAction { get; }
    }
}
