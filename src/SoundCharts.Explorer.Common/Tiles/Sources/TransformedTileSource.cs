using System;
using System.Threading;
using System.Threading.Tasks;

namespace SoundCharts.Explorer.Tiles.Sources
{
    public delegate Task<TileData?> TileTransformDelegate(TileIndex index, TileData? data, ITileSource source, CancellationToken cancellationToken = default);

	public sealed class TransformedTileSource : ITileSource
	{
        private readonly ITileSource tileSource;
        private readonly TileTransformDelegate tileTransformDelegate;

        public TransformedTileSource(TileTransformDelegate tileTransformDelegate, ITileSource tileSource)
		{
            this.tileTransformDelegate = tileTransformDelegate ?? throw new ArgumentNullException(nameof(tileTransformDelegate));
            this.tileSource = tileSource ?? throw new ArgumentNullException(nameof(tileSource));
		}

        #region ITileSource Members

        public async Task<TileData?> GetTileAsync(TileIndex index, CancellationToken cancellationToken = default)
        {
            var data = await this.tileSource.GetTileAsync(index, cancellationToken).ConfigureAwait(false);

            data = await this.tileTransformDelegate(index, data, this.tileSource, cancellationToken).ConfigureAwait(false);

            return data;
        }

        #endregion
    }
}

