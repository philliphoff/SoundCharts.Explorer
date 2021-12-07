using System;

namespace SoundCharts.Explorer.MacOS.Services.State
{
	internal delegate ApplicationState? ApplicationStateUpdater(ApplicationState? state);

	internal interface IApplicationStateManager
	{
		IObservable<ApplicationState?> CurrentState { get; }

		void UpdateState(ApplicationStateUpdater setter);
	}
}

