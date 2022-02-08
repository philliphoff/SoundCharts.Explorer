using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SoundCharts.Explorer.Tiles.Tilesets
{
	public sealed record Tileset(string Id);

	public interface ITilesetServiceClient
	{
		Task<IEnumerable<Tileset>> GetTilesetsAsync(CancellationToken cancellationToken = default);

		Task<Tileset> GetTilesetAsync(string id, CancellationToken cancellationToken = default);

		Task<Stream> DownloadTilesetAsync(string id, CancellationToken cancellationToken = default);
	}
}

