using System;

namespace SoundCharts.Explorer.Tiles.Sources
{
	public class HttpTileSets
	{
		public static readonly GetTileUriDelegate NoaaQuiltedTileSet =
			index =>
			{
				return new Uri($"https://tileservice.charts.noaa.gov/tiles/50000_1/{index.Zoom}/{index.Column}/{index.Row}.png");
			};
	}
}

