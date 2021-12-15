using System;
using AppKit;

namespace SoundCharts.Explorer.MacOS.Views
{
	internal sealed class SourceListDelegate : NSOutlineViewDelegate
	{
		private readonly SourceListView view;

		public SourceListDelegate(SourceListView view)
		{
			this.view = view ?? throw new ArgumentNullException(nameof(view));
		}
	}
}

