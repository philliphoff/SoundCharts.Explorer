using System;

namespace SoundCharts.Explorer.Tiles;

public enum TileIndexFormat
{
    Unknown = 0,
    Tms,
    Xyz
}

public sealed record TileIndex(int Column, int Row, int Zoom, TileIndexFormat Format = TileIndexFormat.Xyz)
{
    public TileIndex ToTms()
    {
        return this.Format switch
        {
            TileIndexFormat.Tms => this,
            TileIndexFormat.Xyz => this with { Row = (1 << this.Zoom) - 1 - this.Row },
            _ => throw new InvalidOperationException("Format conversion not supported.")
        };
    }

    /// <remarks>
    /// Adapted from https://wiki.openstreetmap.org/wiki/Slippy_map_tilenames#C.23
    /// </remarks>
    public TileBounds GetBounds()
    {
        double left = tilex2long(this.Column, this.Zoom);
        double top = tiley2lat(this.Row, this.Zoom);
        double right = tilex2long(this.Column + 1, this.Zoom);
        double bottom = tiley2lat(this.Row + 1, this.Zoom);

        return new TileBounds(
            new TileCoordinate(bottom, left),
            new TileCoordinate(top, right));
    }

    private static double tilex2long(int x, int z)
    {
        return x / (double)(1 << z) * 360.0 - 180;
    }

    private static double tiley2lat(int y, int z)
    {
        double n = Math.PI - 2.0 * Math.PI * y / (double)(1 << z);
        return 180.0 / Math.PI * Math.Atan(0.5 * (Math.Exp(n) - Math.Exp(-n)));
    }
}

