using System;
using AppKit;
using Foundation;

namespace SoundCharts.Explorer.MacOS.Views
{
	[Register("SourceListView")]
	internal sealed class SourceListView : NSOutlineView
	{
		public SourceListView()
		{
		}

		public SourceListView(IntPtr handle)
			: base(handle)
		{
		}

		public SourceListView(NSCoder coder)
			: base(coder)
		{
		}

		public SourceListView(NSObjectFlag flag)
			: base(flag)
		{
		}
	}
}

