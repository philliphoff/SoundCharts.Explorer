using System;
using System.Threading;
using System.Threading.Tasks;

namespace SoundCharts.Explorer.Tiles
{
	public delegate Task<TileData?> TileCacheMissDelegate(TileIndex index, CancellationToken cancellationToken = default);

	public interface ITileCache
	{
		Task<TileData?> GetTileAsync(TileIndex index, TileCacheMissDelegate cacheMissDelegate, CancellationToken cancellationToken = default);
	}
}

