using AppKit;
using Foundation;
using Microsoft.Extensions.DependencyInjection;
using SoundCharts.Explorer.MacOS.Components;
using SoundCharts.Explorer.MacOS.Services.State;

namespace SoundCharts.Explorer.MacOS
{
	[Register ("AppDelegate")]
	public class AppDelegate : NSApplicationDelegate
	{
		public static readonly ServiceProvider Services =
				new ServiceCollection()
					.AddSingleton<IApplicationStateManager, ApplicationStateManager>()
					.AddSingleton<IApplicationComponent, ApplicationStateMonitor>()
					.BuildServiceProvider();

		public AppDelegate ()
		{
		}

		public override void DidFinishLaunching (NSNotification notification)
		{
			foreach (var component in Services.GetServices<IApplicationComponent>())
            {
				component.Initialize();
            }
		}

		public override void WillTerminate (NSNotification notification)
		{
			// Insert code here to tear down your application
		}
	}
}

