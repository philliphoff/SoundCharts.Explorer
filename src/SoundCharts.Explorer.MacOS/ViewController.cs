﻿using System;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using AppKit;
using CoreLocation;
using Foundation;
using MapKit;
using Microsoft.Extensions.DependencyInjection;
using NauticalCharts;
using SixLabors.ImageSharp;
using SoundCharts.Explorer.MacOS.Services.State;
using SoundCharts.Explorer.MacOS.Utils;
using SoundCharts.Explorer.MacOS.Views.Overlays;
using SoundCharts.Explorer.Tiles;

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
					ImageOverlay imageOverlay => new ImageOverlayRenderer(imageOverlay),
					TileSourceOverlay tileOverlay => new TileSourceOverlayRenderer(tileOverlay),
					_ => null! // TODO: Throw instead?
				};

			var applicationStateManager = AppDelegate.Services.GetRequiredService<IApplicationStateManager>();

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

			//this.overlay = new TileSourceOverlay(AppDelegate.Services.GetRequiredService<IObservableTileSource>());

			//this.mapView.AddOverlay(this.overlay, MKOverlayLevel.AboveLabels);

			this.TryLoadChart()
				.ContinueWith(_ => { });
		}

		private async Task TryLoadChart()
        {
			string path = "/Users/phoff/Downloads/RM-PAC02 (31-Jan-22 Update)/BSBCHART/342401.KAP";

			using var stream = File.Open(path, FileMode.Open);

			var chart = await BsbChartReader.ReadChartAsync(stream);
			var metadata = BsbMetadataReader.ReadMetadata(chart.TextSegment);

			var image = await chart.ToNSImageAsync();

			var (bounds, center) = metadata.ToMapBounds();

			this.mapView.AddOverlay(new ImageOverlay(image, bounds, center), MKOverlayLevel.AboveLabels);
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
