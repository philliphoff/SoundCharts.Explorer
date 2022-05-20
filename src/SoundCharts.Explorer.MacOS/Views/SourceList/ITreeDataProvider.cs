using System;
using System.Collections.Immutable;

namespace SoundCharts.Explorer.MacOS.Views.SourceList;

internal interface ITreeDataProvider<TModel, out TObject>
{
    IObservable<IImmutableSet<TModel>> ChangedData { get; }

    IImmutableList<TModel> GetChildren(TModel? element = default);

    TObject GetObject(TModel element);
}

