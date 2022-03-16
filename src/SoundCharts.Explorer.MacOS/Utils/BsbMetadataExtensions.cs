using System;
using System.Linq;
using CoreLocation;
using MapKit;
using NauticalCharts;

namespace SoundCharts.Explorer.MacOS.Utils
{
	internal static class BsbMetadataExtensions
	{
		public static (MKMapRect Bounds, CLLocationCoordinate2D Center) ToMapBounds(this BsbMetadata metadata)
		{
			var left = metadata.Border.Select(b => b.Longitude).Aggregate(180.0, (x, y) => Math.Min(x, y));
			var right = metadata.Border.Select(b => b.Longitude).Aggregate(-180.0, (x, y) => Math.Max(x, y));
			var top = metadata.Border.Select(b => b.Latitude).Aggregate(-90.0, (x, y) => Math.Max(x, y));
			var bottom = metadata.Border.Select(b => b.Latitude).Aggregate(90.0, (x, y) => Math.Min(x, y));

			var bottomLeft = new CLLocationCoordinate2D(bottom, left);
			var topRight = new CLLocationCoordinate2D(top, right);

			var bottomLeftPoint = MKMapPoint.FromCoordinate(bottomLeft);
			var topRightPoint = MKMapPoint.FromCoordinate(topRight);

			var bounds = new MKMapRect(
				Math.Min(bottomLeftPoint.X, topRightPoint.X),
				Math.Min(bottomLeftPoint.Y, topRightPoint.Y),
				Math.Abs(topRightPoint.X - bottomLeftPoint.X),
				Math.Abs(topRightPoint.Y - bottomLeftPoint.Y));

			var center = new CLLocationCoordinate2D(
				bottomLeft.Latitude + ((topRight.Latitude - bottomLeft.Latitude) / 2),
				bottomLeft.Longitude = ((topRight.Longitude - bottomLeft.Longitude) / 2));

			return (bounds, center);
		}
	}
}

