using System;
using System.Net.Http;
using AppKit;
using Foundation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SoundCharts.Explorer.Charts;
using SoundCharts.Explorer.Charts.Sources;
using SoundCharts.Explorer.MacOS.Components;
using SoundCharts.Explorer.MacOS.Services.Collections;
using SoundCharts.Explorer.MacOS.Services.Http;
using SoundCharts.Explorer.MacOS.Services.Logging;
using SoundCharts.Explorer.MacOS.Services.State;
using SoundCharts.Explorer.MacOS.Services.Tiles;
using SoundCharts.Explorer.MacOS.Services.Tilesets;
using SoundCharts.Explorer.Tiles;
using SoundCharts.Explorer.Tiles.Tilesets;

namespace SoundCharts.Explorer.MacOS
{
	[Register ("AppDelegate")]
	public class AppDelegate : NSApplicationDelegate
	{
		public static readonly ServiceProvider Services =
				new ServiceCollection()
					.AddSingleton<IApplicationStateManager, ApplicationStateManager>()
					.AddSingleton<IApplicationComponent, ApplicationStateMonitor>()
					.AddSingleton<IChartSource>(
						_ =>
                        {
							return new CachedChartSource(
								new InMemoryChartCache(),
								new LocalChartSource());
                        })
					.AddSingleton<IChartCollectionManager, ChartCollectionManager>()
					.AddSingleton<IHttpClientManager, HttpClientManager>()
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

									builder.AddMacConsole();
								});
						})
					.AddSingleton<IObservableTileSource, ApplicationTileSource>()
					.AddSingleton<ITilesetServiceClient>(serviceProvider => new HttpTilesetServiceClient(serviceProvider.GetRequiredService<IHttpClientManager>().CurrentClient, HttpTilesets.SoundChartsExplorerTilesets))
					.AddSingleton<ITilesetCache, TilesetCache>()
					.AddSingleton<ITilesetManager, TilesetManager>()
					.AddSingleton<OfflineTileSource>()
					.AddSingleton<OnlineTileSource>()
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

