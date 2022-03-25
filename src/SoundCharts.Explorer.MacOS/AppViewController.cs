using System;
using AppKit;
using Foundation;
using ObjCRuntime;

namespace SoundCharts.Explorer.MacOS
{
	[Register("AppViewController")]
	public partial class AppViewController : NSWindowController
	{
		public AppViewController(NativeHandle handle)
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

