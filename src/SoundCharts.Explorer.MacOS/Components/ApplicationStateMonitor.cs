using System;
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

                    return stateString is not null
                        ? JsonSerializer.Deserialize<ApplicationState>(stateString)
                        : null;
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
