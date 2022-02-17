using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SoundCharts.Explorer.Tiles;
using SoundCharts.Explorer.Tiles.Sources;

namespace SoundCharts.Explorer.MacOS.Services.Tiles
{
    internal sealed class OfflineTileSource : IObservableTileSource
    {
        private readonly ITileSource tileSource;

        public OfflineTileSource(ILoggerFactory loggerFactory)
        {
            this.tileSource =
                new LiteDbTileSource("/Users/phoff/Downloads/litedb/MBTILES_06.litedb", loggerFactory);
        }

        #region IObservableTileSource Members

        public event EventHandler<TilesChangedEventArgs>? TilesChanged;

        public Task<TileData?> GetTileAsync(TileIndex index, CancellationToken cancellationToken = default)
        {
            return this.tileSource.GetTileAsync(index, cancellationToken);
        }

        #endregion
    }
}
