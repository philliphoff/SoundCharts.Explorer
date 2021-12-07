using System;
using System.Reactive.Subjects;

namespace SoundCharts.Explorer.MacOS.Services.State
{
	internal sealed class ApplicationStateManager : IApplicationStateManager, IDisposable
	{
        private readonly BehaviorSubject<ApplicationState?> currentState = new(null);
        private readonly object stateLock = new();

        #region IApplicationStateManager Members

        public IObservable<ApplicationState?> CurrentState => this.currentState;

        public void UpdateState(ApplicationStateUpdater setter)
        {
            lock (this.stateLock)
            {
                this.currentState.OnNext(setter(this.currentState.Value));
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

