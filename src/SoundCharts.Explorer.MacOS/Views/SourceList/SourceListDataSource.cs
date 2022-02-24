using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using AppKit;
using Foundation;
using SoundCharts.Explorer.MacOS.Services.State;
using SoundCharts.Explorer.MacOS.Services.Tilesets;

namespace SoundCharts.Explorer.MacOS.Views.SourceList
{
    internal sealed class SourceListDataSource : NSOutlineViewDataSource
    {
        private readonly IApplicationStateManager applicationStateManager;
        private readonly IDisposable stateSubscription;
        private readonly IDisposable subscription;
        private SourceListItem[] items = Array.Empty<SourceListItem>();
        private readonly OfflineTilesetsSwitchItem switchItem = new OfflineTilesetsSwitchItem("Use Offline Tilesets");

        public SourceListDataSource(IApplicationStateManager applicationStateManager, ITilesetManager tilesetManager)
        {            
            if (tilesetManager is null)
            {
                throw new ArgumentNullException(nameof(tilesetManager));
            }

            this.applicationStateManager = applicationStateManager ?? throw new ArgumentNullException(nameof(applicationStateManager));

            this.stateSubscription =
                applicationStateManager
                    .CurrentState
                    .Select(state => state.State?.OfflineTilesets?.Enabled)
                    .DistinctUntilChanged()
                    .ObserveOn(SynchronizationContext.Current)
                    .Subscribe(
                        enabled =>
                        {
                            this.switchItem.OfflineTilesetsEnabled = enabled == true;
                        });

            this.switchItem.OfflineTilesetsEnabledChanged += OnOfflineTilesetsEnabledChanged;

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
                                        new SourceListItem[] { this.switchItem }
                                            .Concat(
                                                tilesets
                                                    .Select(tileset =>
                                                        new TilesetItem(
                                                            tileset.Id,
                                                            async () =>
                                                            {
                                                                switch (tileset.State)
                                                                {
                                                                    case TilesetState.Downloaded:

                                                                        await tilesetManager.DeleteTilesetAsync(tileset.Id);

                                                                        break;

                                                                    case TilesetState.NotDownloaded:

                                                                        await tilesetManager.DownloadTilesetAsync(tileset.Id);

                                                                        break;
                                                                }
                                                            },
                                                            tileset.State == TilesetState.NotDownloaded))
                                                    .OrderBy(tileset => tileset.Title)))
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
                    this.stateSubscription.Dispose();
                    this.subscription.Dispose();

                    this.switchItem.OfflineTilesetsEnabledChanged -= this.OnOfflineTilesetsEnabledChanged;
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        private void OnOfflineTilesetsEnabledChanged(object sender, EventArgs e)
        {
            // NOTE: Copy current value for benefit of later callback.
            // TODO: Consider making value part of event args.
            bool enabled = this.switchItem.OfflineTilesetsEnabled;

            this.applicationStateManager.UpdateState(
                state => state is not null
                    ? state with { OfflineTilesets = new OfflineTilesets(enabled) }
                    : null);
        }
    }
}

