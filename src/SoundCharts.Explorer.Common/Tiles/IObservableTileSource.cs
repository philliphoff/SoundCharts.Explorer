using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace SoundCharts.Explorer.Tiles
{
    public sealed class TilesChangedEventArgs : EventArgs
    {
        public TilesChangedEventArgs(IEnumerable<TileIndex> tiles)
        {
            this.Tiles = tiles?.ToImmutableHashSet<TileIndex>() ?? ImmutableHashSet<TileIndex>.Empty;
        }

        public IImmutableSet<TileIndex> Tiles { get; }
    }

    public interface IObservableTileSource : ITileSource
    {
        public event EventHandler<TilesChangedEventArgs>? TilesChanged;
    }
}
