using System;
using AppKit;
using CoreLocation;
using MapKit;

namespace SoundCharts.Explorer.MacOS.Views.Overlays
{
	internal sealed class ChartOverlay : MKOverlay
	{
		public ChartOverlay()
		{
			// TODO: Finish sketch.
			this.Image = NSImage.FromStream(null);
		}

        public override MKMapRect BoundingMapRect => throw new NotImplementedException();

        public override CLLocationCoordinate2D Coordinate => throw new NotImplementedException();

		public NSImage Image { get; }
    }
}

