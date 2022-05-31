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
    private readonly ITreeDataProvider<TModel, NSObject> treeDataProvider;

    private IImmutableDictionary<NSObject, TModel> objectToModel = ImmutableDictionary<NSObject, TModel>.Empty;
    private IImmutableDictionary<TModel, NSObject> modelToObject = ImmutableDictionary<TModel, NSObject>.Empty;
    private IImmutableList<TModel>? rootChildren;

    public SourceListTreeDataSource(ITreeDataProvider<TModel, NSObject> treeDataProvider)
    {
        this.treeDataProvider = new CachedTreeDataProvider<TModel, NSObject>(treeDataProvider ?? throw new ArgumentNullException(nameof(treeDataProvider)));

        this.ChangedData =
            this.treeDataProvider
                .ChangedData
                .Select(
                    items =>
                    {
                        // TODO: Support individual model changes.
                        var changedObjects = items.Select(item => this.modelToObject[item]).ToImmutableHashSet();

                        this.objectToModel = ImmutableDictionary<NSObject, TModel>.Empty;
                        this.modelToObject = ImmutableDictionary<TModel, NSObject>.Empty;
                        this.rootChildren = null;

                        return changedObjects;
                    });
    }

    #region ISourceListDataSource Members

    public IObservable<IImmutableSet<NSObject>> ChangedData { get; }

    #endregion

    public override NSObject GetChild(NSOutlineView outlineView, nint childIndex, NSObject? item)
    {
        var children = this.GetChildren(item);

        if (childIndex >= children.Count)
        {
            return null!;
        }

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
        else if (this.objectToModel.TryGetValue(item, out model))
        {
            model = this.objectToModel[item];
        }
        else
        {
            return ImmutableList<TModel>.Empty;
        }

        IImmutableList<TModel>? children = null;

        if (model is null)
        {
            children = this.rootChildren;

            if (children is null)
            {
                this.rootChildren = children = this.treeDataProvider.GetChildren(model);
            }
        }
        else
        {
            children = this.treeDataProvider.GetChildren(model);
        }

        return children;
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
