using System;
using AppKit;
using Foundation;

namespace SoundCharts.Explorer.MacOS
{
	[Register("AppViewController")]
	public class AppViewController : NSWindowController
	{
		public AppViewController(IntPtr handle)
			: base(handle)
		{
		}
	}
}

