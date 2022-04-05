using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using SoundCharts.Explorer.MacOS.Services.State;
using SoundCharts.Explorer.Utilities;

namespace SoundCharts.Explorer.MacOS.Services.Collections
{
	internal sealed class ChartCollectionManager : IChartCollectionManager, IDisposable
	{
        private readonly IApplicationStateManager applicationStateManager;
        private readonly BehaviorSubject<IImmutableSet<ChartCollection>> collections = new (ImmutableHashSet<ChartCollection>.Empty);
        private readonly object collectionsLock = new ();
        private readonly IDisposable currentStateSubscription;
        private readonly IDisposable processedChartsSubscription;

        public ChartCollectionManager(IApplicationStateManager applicationStateManager, IChartProcessor chartProcessor)
		{
            this.applicationStateManager = applicationStateManager ?? throw new ArgumentNullException(nameof(applicationStateManager));

            this.currentStateSubscription =
                applicationStateManager
                    .CurrentState
                    .Select(state => state.State?.ChartCollections?.Local ?? ImmutableHashSet<LocalChartCollection>.Empty)
                    .DistinctUntilChanged()
                    .ToChangeSet()
                    .Subscribe(
                        changes =>
                        {
                            lock (this.collectionsLock)
                            {
                                var current = this.collections.Value;

                                foreach (var collection in changes.Removed.Select(c => current.FirstOrDefault(cc => cc.Name == c.Name)).WhereNonNull())
                                {
                                    var existing = current.FirstOrDefault(c => c.Name == collection.Name);

                                    if (existing != null)
                                    {
                                        current = current.Remove(existing);
                                    }
                                }

                                foreach (var collection in changes.Added)
                                {
                                    var newCollection = ToCollection(collection);

                                    foreach (var chart in newCollection.Charts)
                                    {
                                        chartProcessor.SubmitChart(chart.Name);
                                    }

                                    current = current.Add(newCollection);
                                }

                                // TODO: Goes outside lock block?
                                this.collections.OnNext(current);
                            }
                        });

            this.processedChartsSubscription =
                chartProcessor
                    .ProcessedCharts
                    .Subscribe(
                        processedChart =>
                        {
                            lock (this.collectionsLock)
                            {
                                var current = this.collections.Value;

                                foreach (var collection in current)
                                {
                                    var chart = collection.Charts.FirstOrDefault(chart => chart.Name == processedChart.Name);

                                    if (chart != null)
                                    {
                                        var newChart = new ChartCollectionChart(processedChart.Name, processedChart.Title);
                                        var newCharts = collection.Charts.Remove(chart).Add(newChart);
                                        var newCollection = collection with { Charts = newCharts };

                                        current = current.Remove(collection).Add(newCollection);
                                    }
                                }

                                // TODO: Goes outside lock block?
                                this.collections.OnNext(current);
                            }
                        });
		}

        #region ICollectionManager Members

        public IObservable<IImmutableSet<ChartCollection>> Collections => this.collections;

        public string AddLocalCollection(string name, string path)
        {
            string id = Guid.NewGuid().ToString();

            this.applicationStateManager
                .UpdateState(
                    state =>
                    {
                        state = state ?? new ApplicationState();

                        var collections = state.ChartCollections ?? new ChartCollections();
                        var local = state?.ChartCollections?.Local ?? ImmutableHashSet<LocalChartCollection>.Empty;

                        local = local.Add(new LocalChartCollection(id, name, path));

                        return state! with { ChartCollections = collections with { Local = local } };
                    });

            return id;
        }

        public void RemoveCollection(string id)
        {
            this.applicationStateManager
                .UpdateState(
                    state =>
                    {
                        if (state?.ChartCollections?.Local is not null)
                        {
                            var collection = state.ChartCollections.Local.FirstOrDefault(c => c.Id == id);

                            if (collection is not null)
                            {
                                return state with { ChartCollections = state.ChartCollections with { Local = state.ChartCollections.Local.Remove(collection) } };
                            }

                        }

                        return state;
                    });
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            this.currentStateSubscription.Dispose();
            this.processedChartsSubscription.Dispose();

            this.collections.Dispose();
        }

        #endregion

        private static ChartCollection ToCollection(LocalChartCollection collection)
        {
            return new ChartCollection(
                collection.Id,
                collection.Name,
                Directory
                    .GetFiles(collection.Path, "*.KAP")
                    .Select(path => new ChartCollectionChart(new Uri(path), Path.GetFileNameWithoutExtension(path)))
                    .ToImmutableHashSet());
        }
    }
}

