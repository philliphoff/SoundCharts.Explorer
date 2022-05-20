using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using SoundCharts.Explorer.MacOS.Services.Collections;
using SoundCharts.Explorer.MacOS.Services.State;
using SoundCharts.Explorer.MacOS.Services.Tilesets;

namespace SoundCharts.Explorer.MacOS.Views.SourceList.Model;

internal sealed class ExplorerTreeDataProvider : ITreeDataProvider<ExplorerItem, TreeObject>, IDisposable
{
    private readonly IDisposable collectionsSubscription;
    private readonly Subject<IImmutableSet<ExplorerItem>> changedData = new ();
    private readonly OfflineTilesetsItem tilesets;

    private IImmutableSet<ChartCollection> currentCollections = ImmutableHashSet<ChartCollection>.Empty;

    public ExplorerTreeDataProvider(
            IApplicationStateManager applicationStateManager,
            IChartCollectionManager chartCollectionManager,
            ITilesetManager tilesetManager)
    {
        this.tilesets = new (tilesetManager, items => this.changedData.OnNext(items.Any() ? items : ImmutableHashSet.Create<ExplorerItem>(this.tilesets!)));

        this.collectionsSubscription =
            chartCollectionManager
                .Collections
                .DistinctUntilChanged()
                .Subscribe(
                    collections =>
                    {
                        this.currentCollections = collections;

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
                    this.tilesets);
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

        this.changedData.Dispose();

        this.tilesets.Dispose();
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
}

