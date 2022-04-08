using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using AppKit;
using Foundation;

namespace SoundCharts.Explorer.MacOS.Views.SourceList;

internal sealed class SourceListTreeDataSource<TModel> : NSOutlineViewDataSource, ISourceListDataSource
    where TModel : notnull
{
    private readonly ITreeDataProvider<TModel> treeDataProvider;

    private IImmutableDictionary<NSObject, TModel> objectToModel = ImmutableDictionary<NSObject, TModel>.Empty;
    private IImmutableDictionary<TModel, NSObject> modelToObject = ImmutableDictionary<TModel, NSObject>.Empty;

    public SourceListTreeDataSource(ITreeDataProvider<TModel> treeDataProvider)
    {
        this.treeDataProvider = treeDataProvider ?? throw new ArgumentNullException(nameof(treeDataProvider));

        this.ChangedData =
            this.treeDataProvider
                .ChangedData
                .SubscribeOn(SynchronizationContext.Current)
                .Select(items => items.Select(item => this.modelToObject[item]).ToImmutableHashSet());
    }

    #region ISourceListDataSource Members

    public IObservable<IImmutableSet<NSObject>> ChangedData { get; }

    #endregion

    public override NSObject GetChild(NSOutlineView outlineView, nint childIndex, NSObject? item)
    {
        var children = this.GetChildren(item);

        var child = children[(int)childIndex];

        var childObject = this.treeDataProvider.GetObject(child);

        this.objectToModel = this.objectToModel.SetItem(childObject, child);
        this.modelToObject = this.modelToObject.SetItem(child, childObject);

        return childObject;
    }

    private IImmutableList<TModel> GetChildren(NSObject? item)
    {
        TModel? model;

        if (item is null)
        {
            model = default;
        }
        else
        {
            model = this.objectToModel[item];
        }

        return this.treeDataProvider.GetChildren(model);
    }

    public override nint GetChildrenCount(NSOutlineView outlineView, NSObject? item)
    {
        var children = this.GetChildren(item);

        return children.Count;
    }

    public override bool ItemExpandable(NSOutlineView outlineView, NSObject item)
    {
        return item is TreeObject treeObject && treeObject.IsExpandable;
    }
}
