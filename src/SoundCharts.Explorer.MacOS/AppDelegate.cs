using AppKit;
using Foundation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
					.AddSingleton<ILoggerFactory>(
						_ =>
                        {
							return LoggerFactory.Create(
								builder =>
								{
									builder.AddSystemdConsole(
										options =>
                                        {
											options.IncludeScopes = true;
											options.TimestampFormat = "hh:mm:ss ";
                                        });
								});
                        })
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

