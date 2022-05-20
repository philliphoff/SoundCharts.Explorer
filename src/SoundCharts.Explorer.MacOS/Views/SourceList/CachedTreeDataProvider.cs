using System;
using System.Collections.Immutable;
using System.Reactive.Linq;

namespace SoundCharts.Explorer.MacOS.Views.SourceList;

internal sealed class CachedTreeDataProvider<TModel, TObject> : ITreeDataProvider<TModel, TObject>
    where TModel : notnull
{
    private readonly ITreeDataProvider<TModel, TObject> provider;
    private IImmutableDictionary<TModel, CachedModel> cache = ImmutableDictionary<TModel, CachedModel>.Empty;
    private CachedModel root = new (default, default);

    public CachedTreeDataProvider(ITreeDataProvider<TModel, TObject> provider)
    {
        this.provider = provider ?? throw new ArgumentNullException(nameof(provider));
    }

    #region ITreeDataProvider<TModel, TObject> Members

    public IObservable<IImmutableSet<TModel>> ChangedData =>
        this.provider
            .ChangedData
            .Do(
                _ =>
                {
                    // TODO: Only reset affected elements from cache.
                    this.cache = ImmutableDictionary<TModel, CachedModel>.Empty;
                    this.root = new (default, default);
                });

    public IImmutableList<TModel> GetChildren(TModel? element = default)
    {
        if (element is null)
        {
            if (this.root.Children is null)
            {
                this.root = this.root with { Children = this.provider.GetChildren(default) };
            }

            return this.root.Children;
        }
        else
        {
            if (!this.cache.TryGetValue(element, out CachedModel value))
            {
                value = new CachedModel(default, default);
            }

            if (value.Children is null)
            {
                value = value with { Children = this.provider.GetChildren(element) };

                this.cache = this.cache.SetItem(element, value);
            }

            return value.Children;
        }
    }

    public TObject GetObject(TModel element)
    {
        if (!this.cache.TryGetValue(element, out CachedModel value))
        {
            value = new CachedModel(default, default);
        }

        if (value.Object is null)
        {
            value = value with { Object = this.provider.GetObject(element) };

            this.cache = this.cache.SetItem(element, value);
        }

        return value.Object;
    }

    #endregion

    private sealed record CachedModel(IImmutableList<TModel>? Children, TObject? Object);
}

