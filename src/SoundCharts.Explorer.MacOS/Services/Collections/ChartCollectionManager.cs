using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using SoundCharts.Explorer.MacOS.Services.State;

namespace SoundCharts.Explorer.MacOS.Services.Collections
{
	internal sealed class ChartCollectionManager : IChartCollectionManager
	{
        private readonly IApplicationStateManager applicationStateManager;

		public ChartCollectionManager(IApplicationStateManager applicationStateManager)
		{
            this.applicationStateManager = applicationStateManager ?? throw new ArgumentNullException(nameof(applicationStateManager));

            this.Collections =
                applicationStateManager
                    .CurrentState
                    .Select(state => state.State?.ChartCollections?.Local)
                    .DistinctUntilChanged()
                    .Select(
                        collections =>
                        {
                            if (collections is null)
                            {
                                return ImmutableHashSet<ChartCollection>.Empty;
                            }

                            return collections.Select(ToCollection).ToImmutableHashSet();
                        });
		}

        #region ICollectionManager Members

        public IObservable<IImmutableSet<ChartCollection>> Collections { get; }

        public void AddLocalCollection(string name, string path)
        {
            this.applicationStateManager
                .UpdateState(
                    state =>
                    {
                        state = state ?? new ApplicationState();

                        var collections = state.ChartCollections ?? new ChartCollections();
                        var local = state?.ChartCollections?.Local ?? ImmutableHashSet<LocalChartCollection>.Empty;

                        local = local.Add(new LocalChartCollection(name, path));

                        return state! with { ChartCollections = collections with { Local = local } };
                    });
        }

        public void RemoveCollection(ChartCollection collection)
        {
            if (collection is not ChartCollectionFromLocal collectionFromLocal)
            {
                throw new ArgumentException(nameof(collection));
            }

            this.applicationStateManager
                .UpdateState(
                    state =>
                    {
                        state = state ?? new ApplicationState();

                        var collections = state.ChartCollections ?? new ChartCollections();
                        var local = state?.ChartCollections?.Local ?? ImmutableHashSet<LocalChartCollection>.Empty;

                        local = local.Remove(collectionFromLocal.Local);

                        return state! with { ChartCollections = collections with { Local = local } };
                    });
        }

        #endregion

        private static ChartCollection ToCollection(LocalChartCollection collection)
        {
            return new ChartCollectionFromLocal(
                collection.Name,
                Directory.GetFiles(collection.Path, "*.KAP").ToImmutableHashSet(),
                collection);
        }

        private sealed record ChartCollectionFromLocal(string Name, IImmutableSet<string> Charts, LocalChartCollection Local) : ChartCollection(Name, Charts);
    }
}

