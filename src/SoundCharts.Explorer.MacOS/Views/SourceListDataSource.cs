using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using AppKit;
using Foundation;
using SoundCharts.Explorer.MacOS.Services.Tilesets;

namespace SoundCharts.Explorer.MacOS.Views
{
    internal sealed class SourceListDataSource : NSOutlineViewDataSource
    {
        private readonly IDisposable subscription;
        private SourceListItem[] tilesets = Array.Empty<SourceListItem>();

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
                            this.tilesets = tilesets.Select(tileset => new SourceListItem(tileset.Id)).ToArray();

                            this.TilesetsChanged?.Invoke(this, EventArgs.Empty);
                        });
        }

        public event EventHandler? TilesetsChanged;

        public override NSObject GetChild(NSOutlineView outlineView, nint childIndex, NSObject? item)
        {
            return item is null && childIndex >= 0 && childIndex < this.tilesets.Length
                ? this.tilesets[childIndex]
                : base.GetChild(outlineView, childIndex, item);
        }

        public override nint GetChildrenCount(NSOutlineView outlineView, NSObject? item)
        {
            return item is null
                ? this.tilesets.Length
                : base.GetChildrenCount(outlineView, item);
        }

        public override NSObject GetObjectValue(NSOutlineView outlineView, NSTableColumn? tableColumn, NSObject? item)
        {
            return item is SourceListItem sourceListItem
                ? new NSString(sourceListItem.Title)
                : base.GetObjectValue(outlineView, tableColumn, item);
        }

        public override bool ItemExpandable(NSOutlineView outlineView, NSObject item)
        {
            return false;
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

