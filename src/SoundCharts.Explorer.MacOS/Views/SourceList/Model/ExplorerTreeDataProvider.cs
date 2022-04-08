using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using SoundCharts.Explorer.MacOS.Services.Collections;
using SoundCharts.Explorer.MacOS.Services.State;
using SoundCharts.Explorer.MacOS.Services.Tilesets;

namespace SoundCharts.Explorer.MacOS.Views.SourceList.Model;

internal sealed class ExplorerTreeDataProvider : ITreeDataProvider<ExplorerItem>, IDisposable
{
    private readonly IDisposable collectionsSubscription;
    private readonly Subject<IImmutableSet<ExplorerItem>> changedData = new ();
    private readonly ITilesetManager tilesetManager;
    private readonly IDisposable tilesetsSubscription;

    private IImmutableSet<ChartCollection> currentCollections = ImmutableHashSet<ChartCollection>.Empty;
    private IImmutableSet<ManagedTileset> currentTilesets = ImmutableHashSet<ManagedTileset>.Empty;

    public ExplorerTreeDataProvider(
            IApplicationStateManager applicationStateManager,
            IChartCollectionManager chartCollectionManager,
            ITilesetManager tilesetManager)
    {
        this.tilesetManager = tilesetManager ?? throw new ArgumentNullException(nameof(tilesetManager));

        this.collectionsSubscription =
            chartCollectionManager
                .Collections
                .Subscribe(
                    collections =>
                    {
                        this.currentCollections = collections;

                        this.NotifyOfChangedData();
                    });

        this.tilesetsSubscription =
            tilesetManager
                .Tilesets
                .Subscribe(
                    tilesets =>
                    {
                        this.currentTilesets = tilesets;

                        this.NotifyOfChangedData();
                    });
    }

    #region ITreeDataProvider<ExplorerItem> Members

    public IObservable<IImmutableSet<ExplorerItem>> ChangedData => this.changedData;

    public IImmutableList<ExplorerItem> GetChildren(ExplorerItem? element = default)
    {
        if (element is null)
        {
            return ImmutableList.Create<ExplorerItem>(
                    ToChartCollectionsItem(this.currentCollections),
                    this.ToTilesetsItem(this.currentTilesets)
                );
        }
        else
        {
            return element.GetChildren();
        }
    }

    public TreeObject GetObject(ExplorerItem element)
    {
        return element.GetObject();
    }

    #endregion

    #region IDisposable Members

    public void Dispose()
    {
        this.collectionsSubscription.Dispose();
        this.tilesetsSubscription.Dispose();

        this.changedData.Dispose();
    }

    #endregion

    private void NotifyOfChangedData(IImmutableSet<ExplorerItem>? items = default)
    {
        this.changedData.OnNext(items ?? ImmutableHashSet<ExplorerItem>.Empty);
    }

    private static ExplorerItem ToChartCollectionsItem(IImmutableSet<ChartCollection> collections)
    {
        return new HeaderItem(
            "Collections",
            () => collections.Select(ToChartCollectionItem).ToImmutableList());
    }

    private static ExplorerItem ToChartCollectionItem(ChartCollection collection)
    {
        return new HeaderItem(
            collection.Name,
            () => collection.Charts.Select(ToChartItem).ToImmutableList());
    }

    private static ExplorerItem ToChartItem(ChartCollectionChart chart)
    {
        return new ChartCollectionChartItem(chart.Title, "TODO: Description");
    }

    private ExplorerItem ToTilesetsItem(IImmutableSet<ManagedTileset> tilesets)
    {
        return new HeaderItem(
            "Tilesets",
            () => tilesets.Select(this.ToTilesetItem).ToImmutableList());
    }

    private ExplorerItem ToTilesetItem(ManagedTileset tileset)
    {
        return new OfflineTilesetItem(
            tileset.Description ?? tileset.Id,
            tileset.Name ?? tileset.Id,
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
            tileset.State == TilesetState.NotDownloaded);
    }
}

