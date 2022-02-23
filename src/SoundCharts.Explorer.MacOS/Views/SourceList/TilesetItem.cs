using System;
using System.Threading.Tasks;

namespace SoundCharts.Explorer.MacOS.Views.SourceList
{
    internal sealed class TilesetItem : SourceListItem
    {
        public TilesetItem(string title, Func<Task>? downloadAsync = null)
            : base(title)
        {
            this.DownloadAsync = downloadAsync ?? (() => Task.FromResult(true));
        }

        public Func<Task> DownloadAsync { get; }
    }
}
