using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using AppKit;
using CoreLocation;
using Foundation;
using MapKit;
using Microsoft.Extensions.DependencyInjection;
using ObjCRuntime;
using SoundCharts.Explorer.Charts.Sources;
using SoundCharts.Explorer.MacOS.Services.State;
using SoundCharts.Explorer.MacOS.Views.Overlays;

namespace SoundCharts.Explorer.MacOS
{
    public partial class ViewController : NSViewController
	{
		private IDisposable? applicationStateListener;
		private ChartOverlayManager? chartOverlayManager;
		private TileSourceOverlay? overlay;
		private IDisposable? regionChangeListener;

		public ViewController (NativeHandle handle) : base (handle)
		{
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			// Do any additional setup after loading the view.

			this.mapView.OverlayRenderer =
				(_, overlay) => overlay switch
				{
					ImageOverlay imageOverlay => new ImageOverlayRenderer(imageOverlay),
					TileSourceOverlay tileOverlay => new TileSourceOverlayRenderer(tileOverlay),
					_ => null! // TODO: Throw instead?
				};

			var applicationStateManager = AppDelegate.Services.GetRequiredService<IApplicationStateManager>();

			this.applicationStateListener = applicationStateManager.CurrentState
				.Where(update => update.State?.Map?.Region is not null && update.Context != this)
				.Select(update => update.State!.Map!.Region!)
				.ObserveOn(SynchronizationContext.Current)
				.Subscribe(
					region =>
					{
						this.mapView.Region =
							new MKCoordinateRegion(
								new CLLocationCoordinate2D(region.Center.Latitude, region.Center.Longitude),
								new MKCoordinateSpan(region.Span.LatitudeDegrees, region.Span.LongitudeDegrees));
					});

			var chartsObserver =

			this.chartOverlayManager = new ChartOverlayManager(
				applicationStateManager.CurrentState
					.Where(update => update.State?.Map?.Charts is not null)
					.Select(update => update.State!.Map!.Charts!)
					.DistinctUntilChanged(),
				AppDelegate.Services.GetRequiredService<IChartSource>(),
				this.mapView);

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
									Map = (state?.Map ?? new Map()) with
									{
										Region =
											new MapRegion(
												new MapCoordinate(region.Center.Latitude, region.Center.Longitude),
												new MapSpan(region.Span.LatitudeDelta, region.Span.LongitudeDelta))
									}
								};
							},
							this);
					});

			//this.overlay = new TileSourceOverlay(AppDelegate.Services.GetRequiredService<IObservableTileSource>());

			//this.mapView.AddOverlay(this.overlay, MKOverlayLevel.AboveLabels);
		}

        protected override void Dispose(bool disposing)
        {
			try
            {
				if (this.overlay != null)
				{
					this.mapView.RemoveOverlay(this.overlay);
				}

				this.chartOverlayManager?.Dispose();

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
