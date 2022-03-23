using System;
using System.Collections.Immutable;

namespace SoundCharts.Explorer.MacOS.Services.State
{
	internal sealed record LocalChartCollection(string Name, string Path);

	internal sealed record ChartCollections
    {
		public IImmutableSet<LocalChartCollection>? Local { get; init; }
	}

	internal sealed record OfflineTilesets(bool Enabled);

	internal sealed record MapCoordinate(double Latitude, double Longitude);

	internal sealed record MapSpan(double LatitudeDegrees, double LongitudeDegrees);

	internal sealed record MapRegion(MapCoordinate Center, MapSpan Span);

	internal sealed record Map
    {
		public IImmutableSet<Uri>? Charts { get; init; }

		public MapRegion? Region { get; init; }
    }

	internal sealed record ApplicationState
	{
		public string? ApiEndpoint { get; init; }

		public ChartCollections? ChartCollections { get; init; }

		public Map? Map { get; init; }

		public OfflineTilesets? OfflineTilesets { get; init; }
	}
}
