using System;

namespace SoundCharts.Explorer.Tiles
{
	public sealed class TileData
	{
		public TileData(TileFormat format, byte[] data)
		{
			this.Data = data ?? throw new ArgumentNullException(nameof(data));
			this.Format = format;
		}

		public byte[] Data { get; }

		public TileFormat Format { get; }
	}
}

