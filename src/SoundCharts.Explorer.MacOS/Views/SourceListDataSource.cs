using System;
using AppKit;

namespace SoundCharts.Explorer.MacOS.Views
{
	internal sealed class SourceListDataSource : NSOutlineViewDataSource
	{
		private readonly SourceListView view;

		public SourceListDataSource(SourceListView view)
		{
			this.view = view ?? throw new ArgumentNullException(nameof(view));
		}
	}
}

