using System;

namespace SoundCharts.Explorer.Tiles.Sources
{
	public class HttpTileSources
	{
		public static readonly GetTileUriDelegate SoundChartsExplorerQuiltedTileSet =
			index =>
			{
				return new Uri($"https://ingress.calmmeadow-67c648ad.canadacentral.azurecontainerapps.io/api/tiles/{index.Zoom}/{index.Column}/{index.Row}.png");
			};

		public static readonly GetTileUriDelegate NoaaQuiltedTileSet =
			index =>
			{
				return new Uri($"https://tileservice.charts.noaa.gov/tiles/50000_1/{index.Zoom}/{index.Column}/{index.Row}.png");
			};
	}
}

