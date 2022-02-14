using System;

namespace SoundCharts.Explorer.MacOS.Services.State
{
	internal sealed record OfflineTilesets(bool Enabled);

	internal sealed record MapCoordinate(double Latitude, double Longitude);

	internal sealed record MapSpan(double LatitudeDegrees, double LongitudeDegrees);

	internal sealed record MapRegion(MapCoordinate Center, MapSpan Span);

	internal sealed record ApplicationState
	{
		public string? ApiEndpoint { get; init; }

		public MapRegion? MapRegion { get; init; }

		public OfflineTilesets? OfflineTilesets { get; init; }
	}
}
