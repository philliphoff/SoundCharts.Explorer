using System;
using AppKit;
using Foundation;

namespace SoundCharts.Explorer.MacOS.Views
{
	internal sealed class SourceListDelegate : NSOutlineViewDelegate
	{
		private readonly SourceListView view;

		public SourceListDelegate(SourceListView view)
		{
			this.view = view ?? throw new ArgumentNullException(nameof(view));
		}

        public override NSView GetView(NSOutlineView outlineView, NSTableColumn? tableColumn, NSObject item)
        {
			if (item is SourceListItem sourceListItem)
            {
				var view = (NSTableCellView)outlineView.MakeView("HeaderCell", this);

				view.TextField.StringValue = sourceListItem.Title;

				return view;
            }
			else
            {
				return base.GetView(outlineView, tableColumn, item);
            }
        }
    }
}

