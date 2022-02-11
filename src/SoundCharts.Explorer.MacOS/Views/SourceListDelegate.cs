using System;
using AppKit;
using Foundation;

namespace SoundCharts.Explorer.MacOS.Views
{
	internal sealed class SourceListDelegate : NSOutlineViewDelegate
	{
        public override NSCell GetCell(NSOutlineView outlineView, NSTableColumn tableColumn, NSObject item)
        {
            nint row = outlineView.RowForItem(item);
            return tableColumn.DataCellForRow(row);
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

        public override bool IsGroupItem(NSOutlineView outlineView, NSObject item)
        {
            return true;
        }

        public override bool ShouldEditTableColumn(NSOutlineView outlineView, NSTableColumn? tableColumn, NSObject item)
        {
            return false;
        }
    }
}

