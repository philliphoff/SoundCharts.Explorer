using System;
using System.Net.Http;
using AppKit;
using Foundation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SoundCharts.Explorer.MacOS.Components;
using SoundCharts.Explorer.MacOS.Services.Logging;
using SoundCharts.Explorer.MacOS.Services.State;
using SoundCharts.Explorer.MacOS.Services.Tilesets;
using SoundCharts.Explorer.Tiles.Tilesets;

namespace SoundCharts.Explorer.MacOS
{
	[Register ("AppDelegate")]
	public class AppDelegate : NSApplicationDelegate
	{
		private static readonly HttpClient HttpClient = new HttpClient();

		public static readonly ServiceProvider Services =
				new ServiceCollection()
					.AddSingleton<IApplicationStateManager, ApplicationStateManager>()
					.AddSingleton<IApplicationComponent, ApplicationStateMonitor>()
					.AddSingleton<IApplicationComponent, TilesetMonitor>()
					.AddSingleton<ILoggerFactory>(
						_ =>
                        {
							return LoggerFactory.Create(
								builder =>
								{
									//builder.AddSystemdConsole(
									//	options =>
         //                               {
									//		options.IncludeScopes = true;
									//		options.TimestampFormat = "hh:mm:ss ";
         //                               });

									builder.AddProvider(new OSLoggerProvider());
								});
                        })
					.AddSingleton<ITilesetServiceClient>(_ => new HttpTilesetServiceClient(HttpClient, new Uri("http://localhost:8080/tilesets")))
					.AddSingleton<ITilesetManager, TilesetManager>()
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

