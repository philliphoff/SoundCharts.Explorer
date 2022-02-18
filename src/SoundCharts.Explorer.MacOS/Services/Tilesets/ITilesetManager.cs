using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using SoundCharts.Explorer.Tiles.Tilesets;

namespace SoundCharts.Explorer.MacOS.Services.Tilesets
{
	internal enum TilesetState
    {
		Unknown = 0,
		NotDownloaded,
		Downloaded
    }

	internal sealed record ManagedTileset(string Id, TilesetState State);

	internal interface ITilesetManager
	{
		IObservable<IImmutableSet<ManagedTileset>> Tilesets { get; }

		Task DeleteTilesetAsync(string id, CancellationToken cancellationToken = default);
		Task DownloadTilesetAsync(string id, CancellationToken cancellationToken = default);
	}
}
