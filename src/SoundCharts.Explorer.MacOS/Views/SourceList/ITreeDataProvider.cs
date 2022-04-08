using System;
using System.Collections.Immutable;

namespace SoundCharts.Explorer.MacOS.Views.SourceList;

internal interface ITreeDataProvider<TModel>
{
    IObservable<IImmutableSet<TModel>> ChangedData { get; }

    IImmutableList<TModel> GetChildren(TModel? element = default);

    TreeObject GetObject(TModel element);
}

