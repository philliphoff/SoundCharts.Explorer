using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using AppKit;
using Foundation;
using SoundCharts.Explorer.MacOS.Services.Tilesets;

namespace SoundCharts.Explorer.MacOS.Views.SourceList
{
    internal sealed class SourceListDataSource : NSOutlineViewDataSource
    {
        private readonly IDisposable subscription;
        private SourceListItem[] items = Array.Empty<SourceListItem>();

        public SourceListDataSource(ITilesetManager tilesetManager)
        {
            if (tilesetManager is null)
            {
                throw new ArgumentNullException(nameof(tilesetManager));
            }

            this.subscription =
                tilesetManager
                    .Tilesets
                    .ObserveOn(SynchronizationContext.Current)
                    .Subscribe(
                        tilesets =>
                        {
                            this.items =
                                new[]
                                {
                                    new HeaderItem(
                                        "Tilesets",
                                        tilesets.Select(tileset => new TilesetItem(tileset.Id)))
                                };

                            this.ItemsChanged?.Invoke(this, EventArgs.Empty);
                        });
        }

        public event EventHandler? ItemsChanged;

        public override NSObject GetChild(NSOutlineView outlineView, nint childIndex, NSObject? item)
        {
            return item switch
            {
                HeaderItem header when childIndex >= 0 && childIndex < header.Children.Length => header.Children[childIndex],
                null when childIndex >= 0 && childIndex < this.items.Length => items[childIndex],
                _ => base.GetChild(outlineView, childIndex, item)
            };
        }

        public override nint GetChildrenCount(NSOutlineView outlineView, NSObject? item)
        {
            return item switch
            {
                HeaderItem header => header.Children.Length,
                null => this.items.Length,
                _ => base.GetChildrenCount(outlineView, item)
            };
        }

        public override NSObject GetObjectValue(NSOutlineView outlineView, NSTableColumn? tableColumn, NSObject? item)
        {
            return item switch
            {
                SourceListItem sourceListItem => new NSString(sourceListItem.Title),
                _ => base.GetObjectValue(outlineView, tableColumn, item)
            };
        }

        public override bool ItemExpandable(NSOutlineView outlineView, NSObject item)
        {
            return item is HeaderItem;
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    this.subscription.Dispose();
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }
    }
}

