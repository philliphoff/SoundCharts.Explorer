using System;
using System.IO;
using System.Net.Http;
using System.Reactive.Linq;
using System.Threading;
using AppKit;
using CoreLocation;
using Foundation;
using MapKit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SoundCharts.Explorer.MacOS.Services.State;
using SoundCharts.Explorer.Tiles.Caches;
using SoundCharts.Explorer.Tiles.Sources;

namespace SoundCharts.Explorer.MacOS
{
	public partial class ViewController : NSViewController
	{
		private IDisposable? applicationStateListener;
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
					TileSourceOverlay tileOverlay => new TileSourceOverlayRenderer(tileOverlay),
					_ => null! // TODO: Throw instead?
				};

			var applicationStateManager = AppDelegate.Services.GetRequiredService<IApplicationStateManager>();
			var loggerFactory = AppDelegate.Services.GetRequiredService<ILoggerFactory>();

			this.applicationStateListener = applicationStateManager?.CurrentState
				.Where(update => update.State?.MapRegion is not null && update.Context != this)
				.Select(update => update.State!.MapRegion!)
				.ObserveOn(SynchronizationContext.Current)
				.Subscribe(
					region =>
					{
						this.mapView.Region =
							new MKCoordinateRegion(
								new CLLocationCoordinate2D(region.Center.Latitude, region.Center.Longitude),
								new MKCoordinateSpan(region.Span.LatitudeDegrees, region.Span.LongitudeDegrees));
					});

			this.regionChangeListener = Observable
				.FromEventPattern<EventHandler<MKMapViewChangeEventArgs>, MKMapViewChangeEventArgs>(
					action => this.mapView.RegionChanged += action,
					action => this.mapView.RegionChanged -= action)
				.Select(_ => this.mapView.Region)
				.Subscribe(
					region =>
					{
						applicationStateManager.UpdateState(
							state =>
							{
								return (state ?? new ApplicationState()) with
								{
									MapRegion = new MapRegion(
										new MapCoordinate(region.Center.Latitude, region.Center.Longitude),
										new MapSpan(region.Span.LatitudeDelta, region.Span.LongitudeDelta))
								};
							},
							this);
					});

			this.overlay = new TileSourceOverlay(
				new SwitchedTileSource(applicationStateManager, loggerFactory));

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

				this.applicationStateListener?.Dispose();
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
