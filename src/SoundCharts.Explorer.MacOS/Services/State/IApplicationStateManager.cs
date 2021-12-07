using System;

namespace SoundCharts.Explorer.MacOS.Services.State
{
	internal delegate ApplicationState? ApplicationStateUpdater(ApplicationState? state);

	internal record ApplicationStateUpdate(ApplicationState? State = default, object? Context = default);

	internal interface IApplicationStateManager
	{
		IObservable<ApplicationStateUpdate> CurrentState { get; }

		void UpdateState(ApplicationStateUpdater setter, object? context = default);
	}
}

