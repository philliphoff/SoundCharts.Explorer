using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SoundCharts.Explorer.Tiles.Tilesets;

namespace SoundCharts.Explorer.MacOS.Services.Tilesets
{
	public interface ITilesetManager
	{
		IObservable<Tileset[]> Tilesets { get; }

		Task RefreshAsync(CancellationToken cancellationToken = default);
	}
}
