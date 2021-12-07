using System;
using System.Reactive.Subjects;

namespace SoundCharts.Explorer.MacOS.Services.State
{
	internal sealed class ApplicationStateManager : IApplicationStateManager, IDisposable
	{
        private readonly BehaviorSubject<ApplicationStateUpdate> currentState = new(new ApplicationStateUpdate());
        private readonly object stateLock = new();

        #region IApplicationStateManager Members

        public IObservable<ApplicationStateUpdate> CurrentState => this.currentState;

        public void UpdateState(ApplicationStateUpdater setter, object? context = default)
        {
            lock (this.stateLock)
            {
                this.currentState.OnNext(new ApplicationStateUpdate(setter(this.currentState.Value.State), context));
            }
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            this.currentState.Dispose();
        }

        #endregion
    }
}

