using System;
using AppKit;
using Foundation;

namespace SoundCharts.Explorer.MacOS
{
	[Register("AppViewController")]
	public partial class AppViewController : NSWindowController
	{
		public AppViewController(IntPtr handle)
			: base(handle)
		{
		}

		partial void OnToggleSidebar(NSObject sender)
        {
			if (this.ContentViewController is NSSplitViewController splitViewController)
            {
				splitViewController.ToggleSidebar(sender);
            }
        }
	}
}

