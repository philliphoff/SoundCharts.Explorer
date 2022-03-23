using System;
using System.Collections.Immutable;
using System.Reactive.Linq;
using System.Text.Json;
using System.Threading;
using Foundation;
using SoundCharts.Explorer.MacOS.Services.State;

namespace SoundCharts.Explorer.MacOS.Components
{
	internal sealed class ApplicationStateMonitor : IApplicationComponent, IDisposable
	{
        private readonly IApplicationStateManager applicationStateManager;
        private readonly NSUserDefaults userDefaults = new();

        private IDisposable? applicationStateListener;

        public ApplicationStateMonitor(IApplicationStateManager applicationStateManager)
		{
            this.applicationStateManager = applicationStateManager ?? throw new ArgumentNullException(nameof(applicationStateManager));
		}

        #region IApplicationComponent Members

        public void Initialize()
        {
            this.applicationStateManager.UpdateState(
                _ =>
                {
                    string stateString = this.userDefaults.StringForKey("state");

                    ApplicationState? state =
                        stateString is not null
                            ? JsonSerializer.Deserialize<ApplicationState>(stateString)
                            : null;

                    // TODO: Remove hardcoded collection.
                    if (state is not null)
                    {
                        state = state with
                        {
                            ChartCollections =
                                new ChartCollections
                                {
                                    Local =
                                        ImmutableHashSet<LocalChartCollection>
                                            .Empty
                                            .Add(new LocalChartCollection("Canadian Pacific South", "/Users/phoff/Downloads/RM-PAC02 (31-Jan-22 Update)/BSBCHART"))
                                },
                            Map = (state.Map ?? new Map()) with
                            {
                                Charts = ImmutableHashSet<Uri>.Empty.Add(new Uri("/Users/phoff/Downloads/RM-PAC02 (31-Jan-22 Update)/BSBCHART/342401.KAP"))
                            }
                        };
                    }

                    return state;
                },
                this);

            this.applicationStateListener =
                this.applicationStateManager
                    .CurrentState
                    .Where(update => update.Context != this)
                    .ObserveOn(SynchronizationContext.Current)
                    .Subscribe(
                        update =>
                        {
                            if (update.State is not null)
                            {
                                this.userDefaults.SetString(JsonSerializer.Serialize(update.State), "state");
                            }
                            else
                            {
                                this.userDefaults.SetString(null, "state");
                            }
                        });
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            this.applicationStateListener?.Dispose();
            this.userDefaults.Dispose();
        }

        #endregion
    }
}
