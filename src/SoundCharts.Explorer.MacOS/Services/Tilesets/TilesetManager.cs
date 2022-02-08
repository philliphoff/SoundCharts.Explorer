using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using SoundCharts.Explorer.Tiles.Tilesets;

namespace SoundCharts.Explorer.MacOS.Services.Tilesets
{
	internal sealed class TilesetManager : ITilesetManager, IDisposable
	{
        private readonly ITilesetServiceClient client;

        private readonly BehaviorSubject<Tileset[]> tilesets = new BehaviorSubject<Tileset[]>(Array.Empty<Tileset>());

        public TilesetManager(ITilesetServiceClient client)
		{
            this.client = client ?? throw new ArgumentNullException(nameof(client));
		}

        #region ITilesetManager Members

        public IObservable<Tileset[]> Tilesets => this.tilesets;

        public async Task RefreshAsync(CancellationToken cancellationToken = default)
        {
            var tilesets = await this.client.GetTilesetsAsync(cancellationToken);

            this.tilesets.OnNext(tilesets.ToArray());
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            this.tilesets.Dispose();
        }

        #endregion
    }
}
