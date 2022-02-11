using System;

namespace SoundCharts.Explorer.MacOS.Views.SourceList
{
    internal sealed class OfflineTilesetsSwitchItem : SourceListItem
    {
        public OfflineTilesetsSwitchItem(string title)
            : base(title)
        {
        }

        public bool Enabled { get; }
    }
}
