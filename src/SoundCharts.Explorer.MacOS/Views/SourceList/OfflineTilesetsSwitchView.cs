using System;
using AppKit;
using Foundation;

namespace SoundCharts.Explorer.MacOS.Views.SourceList
{
    [Register("OfflineTilesetsSwitchView")]
    public partial class OfflineTilesetsSwitchView : NSTableCellView
    {
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

        public NSSwitch OfflineSwitch => this.offlineSwitch;
    }
}
