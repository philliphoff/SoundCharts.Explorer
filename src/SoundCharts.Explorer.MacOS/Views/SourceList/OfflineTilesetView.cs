using System;
using AppKit;
using Foundation;

namespace SoundCharts.Explorer.MacOS.Views.SourceList
{
	[Register("OfflineTilesetView")]
	public partial class OfflineTilesetView : NSView
	{
        public OfflineTilesetView(IntPtr handle)
            : base(handle)
        {
        }

        public void Initialize(string title, string description)
        {
            this.TitleTextField.StringValue = title;
            this.DescriptionTextField.StringValue = description;
        }
    }
}

