using System;
using System.Threading;
using System.Threading.Tasks;
using SixLabors.ImageSharp;

namespace SoundCharts.Explorer.Tiles.Sources
{
	public static class TileSourceTransforms
	{
		public static Task<TileData?> EmptyTileTransformAsync(TileIndex index, TileData? data, CancellationToken cancellationToken = default)
        {
            if (data is null || data.Data.Length == 0)
            {
                return Task.FromResult<TileData?>(null);
            }

			return Task.Run(
				() =>
				{
                    // TODO: Pass in image format?
                    using (var image = Image.Load(data.Data))
                    {
                        for (int i = 0; i < image.Height; i++)
                        {
                            var pixelRow = image.GetPixelRowSpan(i);

                            foreach (var pixel in pixelRow)
                            {
                                if (pixel.A != 0)
                                {
                                    return data;
                                }
                            }
                        }
                    }

                    return null;
                },
				cancellationToken);
        }
	}
}

