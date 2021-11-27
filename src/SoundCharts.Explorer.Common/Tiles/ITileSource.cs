using System;
using System.Threading;
using System.Threading.Tasks;

namespace SoundCharts.Explorer.Tiles
{
	public interface ITileSource
	{
		Task<TileData> GetTileAsync(TileIndex index, CancellationToken cancellationToken = default);
	}
}
