using AppKit;
using Foundation;
using Microsoft.Extensions.DependencyInjection;
using SoundCharts.Explorer.MacOS.Services.State;

namespace SoundCharts.Explorer.MacOS
{
	[Register ("AppDelegate")]
	public class AppDelegate : NSApplicationDelegate
	{
		public static ServiceProvider? Services { get; private set; }

		public AppDelegate ()
		{
		}

		public override void DidFinishLaunching (NSNotification notification)
		{
			Services =
				new ServiceCollection()
					.AddSingleton<IApplicationStateManager, ApplicationStateManager>()
					.BuildServiceProvider();
		}

		public override void WillTerminate (NSNotification notification)
		{
			// Insert code here to tear down your application
		}
	}
}

