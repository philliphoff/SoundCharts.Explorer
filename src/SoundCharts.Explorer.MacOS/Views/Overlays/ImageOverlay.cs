using AppKit;
using CoreLocation;
using MapKit;

namespace SoundCharts.Explorer.MacOS.Views.Overlays
{
	internal sealed class ImageOverlay : MKOverlay
	{
		public ImageOverlay(NSImage image, MKMapRect bounds, CLLocationCoordinate2D center)
		{
			this.BoundingMapRect = bounds;
			this.Coordinate = center;
			this.Image = image;
		}

        public override MKMapRect BoundingMapRect { get; }

        public override CLLocationCoordinate2D Coordinate { get; }

		public NSImage Image { get; }
    }
}

