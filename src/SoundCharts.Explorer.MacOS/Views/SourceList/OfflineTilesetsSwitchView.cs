using System;
using AppKit;
using Foundation;
using SoundCharts.Explorer.MacOS.Views.SourceList.Objects;

namespace SoundCharts.Explorer.MacOS.Views.SourceList
{
    [Register("OfflineTilesetsSwitchView")]
    internal partial class OfflineTilesetsSwitchView : NSTableCellView
    {
        private SwitchObject? item;

        public OfflineTilesetsSwitchView()
        {
        }

        public OfflineTilesetsSwitchView(IntPtr handle)
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

        public void Initialize(SwitchObject item)
        {
            this.item = item ?? throw new ArgumentNullException(nameof(item));
            //this.item.OfflineTilesetsEnabledChanged += this.OnOfflineTilesetsEnabledChanged;

            this.offlineSwitch.State = this.item.Enabled ? 1 : 0;

            this.TextField.StringValue = this.item.Label;
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing && this.item != null)
                {
                    // TODO: Fix this.
                    //this.item.OfflineTilesetsEnabledChanged -= this.OnOfflineTilesetsEnabledChanged;
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
                this.offlineSwitch.State = this.item.Enabled ? 1 : 0;
            }
        }

        partial void OnOfflineSwitchAction(NSObject sender)
        {
            if (this.item != null)
            {
                this.item.Enabled = this.offlineSwitch.State == 1;
            }
        }
    }
}
