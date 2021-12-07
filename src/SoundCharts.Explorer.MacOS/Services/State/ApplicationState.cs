using System;

namespace SoundCharts.Explorer.MacOS.Services.State
{
	internal sealed record MapCoordinate(double Latitude, double Longitude);

	internal sealed record MapSpan(double LatitudeDegrees, double LongitudeDegrees);

	internal sealed record MapRegion(MapCoordinate Center, MapSpan Span);

	internal sealed record ApplicationState
	{
		public MapRegion? MapRegion { get; init; }
	}
}
