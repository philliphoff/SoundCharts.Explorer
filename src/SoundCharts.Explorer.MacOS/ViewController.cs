using System;
using System.IO;
using System.Net.Http;
using System.Reactive.Linq;
using AppKit;
using Foundation;
using MapKit;
using Microsoft.Extensions.DependencyInjection;
using SoundCharts.Explorer.MacOS.Services.State;
using SoundCharts.Explorer.Tiles.Caches;
using SoundCharts.Explorer.Tiles.Sources;

namespace SoundCharts.Explorer.MacOS
{
	public partial class ViewController : NSViewController
	{
		private TileSourceOverlay? overlay;
		private IDisposable? regionChangeListener;

		public ViewController (IntPtr handle) : base (handle)
		{
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			// Do any additional setup after loading the view.

			this.mapView.OverlayRenderer =
				(_, overlay) => overlay switch
				{
					MKTileOverlay tileOverlay => new MKTileOverlayRenderer(tileOverlay),
					_ => null! // TODO: Throw instead?
				};

			this.regionChangeListener = Observable
				.FromEventPattern<EventHandler<MKMapViewChangeEventArgs>, MKMapViewChangeEventArgs>(
					action => this.mapView.RegionChanged += action,
					action => this.mapView.RegionChanged -= action)
				.Select(_ => this.mapView.Region)
				.Subscribe(
					region =>
					{
						// TODO: Push changes to application state.
						var applicationStateManager = AppDelegate.Services?.GetService<IApplicationStateManager>();

						applicationStateManager?.UpdateState(
							state =>
							{
								return (state ?? new ApplicationState()) with
								{
									MapRegion = new MapRegion(
										new MapCoordinate(region.Center.Latitude, region.Center.Latitude),
										new MapSpan(region.Span.LatitudeDelta, region.Span.LongitudeDelta))
								};
							});
					});

			string cacheDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".soundcharts", "explorer", "caches");
			string displayCacheDirectory = Path.Combine(cacheDirectory, "display");
			string noaaCacheDirectory = Path.Combine(cacheDirectory, "noaa");

			this.overlay = new TileSourceOverlay(
				new CachedTileSource(
					new InMemoryTileCache(), // TODO: Dispose of cache.
					new CachedTileSource(
						new FileTileCache(displayCacheDirectory), // TODO: Dispose of cache.
						new TransformedTileSource(
							TileSourceTransforms.OverzoomedTransform,
							new TransformedTileSource(
								TileSourceTransforms.EmptyTileTransformAsync,
								new CachedTileSource(
									new FileTileCache(noaaCacheDirectory), // TODO: Dispose of cache.
									new HttpTileSource(new HttpClient(), HttpTileSets.NoaaQuiltedTileSet)))))));

			this.mapView.AddOverlay(this.overlay, MKOverlayLevel.AboveLabels);
		}

        protected override void Dispose(bool disposing)
        {
			try
            {
				if (this.overlay != null)
				{
					this.mapView.RemoveOverlay(this.overlay);
				}

				this.regionChangeListener?.Dispose();
            }
			finally
            {
				base.Dispose(disposing);
            }
        }

        public override NSObject RepresentedObject {
			get {
				return base.RepresentedObject;
			}
			set {
				base.RepresentedObject = value;
				// Update the view, if already loaded.
			}
		}
	}
}
