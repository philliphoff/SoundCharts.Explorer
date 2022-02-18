using System;
using System.Collections.Immutable;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SoundCharts.Explorer.MacOS.Services.Tilesets
{
	internal sealed record CachedTileset(string Id, string Path);

	internal interface ITilesetCache
	{
		IObservable<IImmutableSet<CachedTileset>> Tilesets { get; }

		Task AddTilesetAsync(string id, Stream stream, CancellationToken cancellationToken = default);
		Task RemoveTilesetAsync(string id, CancellationToken cancellationToken = default);
	}
}
