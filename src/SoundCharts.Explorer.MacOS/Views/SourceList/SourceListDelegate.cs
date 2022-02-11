using System;
using AppKit;
using Foundation;

namespace SoundCharts.Explorer.MacOS.Views.SourceList
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
            return item switch
            {
                HeaderItem header => this.MakeView(outlineView, header),
                OfflineTilesetsSwitchItem offlineTilesetsSwitch => this.MakeView(outlineView, offlineTilesetsSwitch),
                TilesetItem tileset => this.MakeView(outlineView, tileset),
                _ => base.GetView(outlineView, tableColumn, item)
            };
        }

        public override bool IsGroupItem(NSOutlineView outlineView, NSObject item)
        {
            return item is HeaderItem;
        }

        public override bool ShouldEditTableColumn(NSOutlineView outlineView, NSTableColumn? tableColumn, NSObject item)
        {
            return false;
        }

        private NSView MakeView(NSOutlineView outlineView, HeaderItem item)
        {
            var view = (NSTableCellView)outlineView.MakeView("HeaderCell", this);

            view.TextField.StringValue = item.Title;

            return view;
        }

        private NSView MakeView(NSOutlineView outlineView, OfflineTilesetsSwitchItem item)
        {
            var view = (OfflineTilesetsSwitchView)outlineView.MakeView("OfflineTilesetsSwitchCell", this);

            view.TextField.StringValue = item.Title;

            view.OfflineSwitch.State = 1;

            return view;
        }

        private NSView MakeView(NSOutlineView outlineView, TilesetItem item)
        {
            var view = (NSTableCellView)outlineView.MakeView("DataCell", this);

            view.TextField.StringValue = item.Title;

            return view;
        }
    }
}

