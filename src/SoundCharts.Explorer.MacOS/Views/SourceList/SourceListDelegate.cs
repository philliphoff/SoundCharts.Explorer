using System;
using AppKit;
using Foundation;
using SoundCharts.Explorer.MacOS.Views.SourceList.Objects;

namespace SoundCharts.Explorer.MacOS.Views.SourceList
{
	internal sealed class SourceListDelegate : NSOutlineViewDelegate
	{
        public override nfloat GetRowHeight(NSOutlineView outlineView, NSObject item)
        {
            return item switch
            {
                OfflineTilesetObject => 36,
                _ => 17
            };
        }

        public override NSCell GetCell(NSOutlineView outlineView, NSTableColumn tableColumn, NSObject item)
        {
            nint row = outlineView.RowForItem(item);
            return tableColumn.DataCellForRow(row);
        }

        public override NSView GetView(NSOutlineView outlineView, NSTableColumn? tableColumn, NSObject item)
        {
            return item switch
            {
                ChartCollectionChartObject fileItem => this.MakeView(outlineView, fileItem),
                HeaderObject header => this.MakeView(outlineView, header),
                SwitchObject offlineTilesetsSwitch => this.MakeView(outlineView, offlineTilesetsSwitch),
                OfflineTilesetObject tileset => this.MakeView(outlineView, tileset),
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

        public override void SelectionDidChange(NSNotification notification)
        {
            if (notification.Object is NSOutlineView view && view.SelectedRowCount > 0)
            {
                var item = view.ItemAtRow(view.SelectedRow);
            }
        }

        private NSView MakeView(NSOutlineView outlineView, ChartCollectionChartObject fileItem)
        {
            var view = (NSTableCellView)outlineView.MakeView("CollectionFileCell", this);

            view.TextField.StringValue = fileItem.Title;

            return view;
        }

        private NSView MakeView(NSOutlineView outlineView, HeaderObject item)
        {
            var view = (NSTableCellView)outlineView.MakeView("HeaderCell", this);

            view.TextField.StringValue = item.Title;

            return view;
        }

        private NSView MakeView(NSOutlineView outlineView, SwitchObject item)
        {
            var view = (OfflineTilesetsSwitchView)outlineView.MakeView("OfflineTilesetsSwitchCell", this);

            view.Initialize(item);

            return view;
        }

        private NSView MakeView(NSOutlineView outlineView, OfflineTilesetObject item)
        {
            var view = (OfflineTilesetView)outlineView.MakeView("OfflineTilesetView", this);

            view.Initialize(
                item.Title,
                item.Description,
                item.DownloadAsync,
                item.IsDownloadAction);

            return view;
        }
    }
}

