namespace SoundCharts.Explorer.Tiles;

public sealed record TileBounds(TileCoordinate BottomLeft, TileCoordinate TopRight)
{
    public bool Overlaps(TileBounds other)
    {
        if (this.TopRight.Longitude < other.BottomLeft.Longitude || other.TopRight.Longitude < this.BottomLeft.Longitude)
        {
            return false;
        }

        if (this.BottomLeft.Latitude > other.TopRight.Latitude || other.BottomLeft.Latitude > this.TopRight.Latitude)
        {
            return false;
        }

        return true;
    }
}
