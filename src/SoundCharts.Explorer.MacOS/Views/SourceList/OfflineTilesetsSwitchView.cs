using System;
using AppKit;
using Foundation;
using ObjCRuntime;

namespace SoundCharts.Explorer.MacOS.Views.SourceList
{
    [Register("OfflineTilesetsSwitchView")]
    internal partial class OfflineTilesetsSwitchView : NSTableCellView
    {
        private OfflineTilesetsSwitchItem? item;

        public OfflineTilesetsSwitchView()
        {
        }

        public OfflineTilesetsSwitchView(NativeHandle handle)
            : base(handle)
        {
        }

        public OfflineTilesetsSwitchView(NSCoder coder)
            : base(coder)
        {
        }

        public OfflineTilesetsSwitchView(NSObjectFlag flag)
            : base(flag)
        {
        }

        public void Initialize(OfflineTilesetsSwitchItem item)
        {
            this.item = item ?? throw new ArgumentNullException(nameof(item));
            this.item.OfflineTilesetsEnabledChanged += this.OnOfflineTilesetsEnabledChanged;

            this.offlineSwitch.State = this.item.OfflineTilesetsEnabled ? 1 : 0;

            this.TextField.StringValue = this.item.Title;
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing && this.item != null)
                {
                    this.item.OfflineTilesetsEnabledChanged -= this.OnOfflineTilesetsEnabledChanged;
                    this.item = null;
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        private void OnOfflineTilesetsEnabledChanged(object sender, EventArgs e)
        {
            if (this.item != null)
            {
                this.offlineSwitch.State = this.item.OfflineTilesetsEnabled ? 1 : 0;
            }
        }

        partial void OnOfflineSwitchAction(NSObject sender)
        {
            if (this.item != null)
            {
                this.item.OfflineTilesetsEnabled = this.offlineSwitch.State == 1;
            }
        }
    }
}
