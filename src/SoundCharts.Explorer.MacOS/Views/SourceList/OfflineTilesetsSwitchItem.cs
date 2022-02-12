using System;

namespace SoundCharts.Explorer.MacOS.Views.SourceList
{
    internal sealed class OfflineTilesetsSwitchItem : SourceListItem
    {
        private bool offlineTilesetsEnabled;

        public OfflineTilesetsSwitchItem(string title)
            : base(title)
        {
        }

        public bool OfflineTilesetsEnabled
        {
            get => this.offlineTilesetsEnabled;
            set
            {
                if (this.offlineTilesetsEnabled != value)
                {
                    this.offlineTilesetsEnabled = value;

                    this.OfflineTilesetsEnabledChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public event EventHandler? OfflineTilesetsEnabledChanged;
    }
}
