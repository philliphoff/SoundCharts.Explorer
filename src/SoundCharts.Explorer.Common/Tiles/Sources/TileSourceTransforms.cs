using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace SoundCharts.Explorer.Tiles.Sources
{
	public static class TileSourceTransforms
	{
		public static Task<TileData?> EmptyTileTransformAsync(TileIndex index, TileData? data, ITileSource source, CancellationToken cancellationToken = default)
        {
            if (data is null || data.Data.Length == 0)
            {
                return Task.FromResult<TileData?>(null);
            }

			return Task.Run(
				() =>
				{
                    bool isEmpty = true;

                    // TODO: Pass in image format?
                    using (var image = Image.Load<Rgba32>(data.Data))
                    {
                        image.ProcessPixelRows(
                            accessor =>
                            {
                                for (int i = 0; i < accessor.Height; i++)
                                {
                                    var pixelRow = accessor.GetRowSpan(i);

                                    foreach (var pixel in pixelRow)
                                    {
                                        if (pixel.A != 0)
                                        {
                                            isEmpty = false;

                                            return;
                                        }
                                    }
                                }
                            });
                    }

                    return isEmpty ? null : data;
                },
				cancellationToken);
        }

        public static async Task<TileData?> OverzoomedTransform(TileIndex index, TileData? data, ITileSource source, CancellationToken cancellationToken = default)
        {
            // TODO: Ensure done off the main thread.
            if (data is not null)
            {
                return data;
            }

            int sourceZoom = index.Zoom - 1;
            int minZoom = 0; // TODO: Do we define a minimum zoom?

            while (sourceZoom >= minZoom)
            {
                var enclosingIndex = GetEnclosingTileForOverzoomedPath(index, sourceZoom);
                var enclosingTileData = await source.GetTileAsync(enclosingIndex, cancellationToken).ConfigureAwait(false);

                if (enclosingTileData is not null)
                {
                    return await ExtractTileAtPathAsync(index, enclosingTileData, enclosingIndex, cancellationToken).ConfigureAwait(false);
                }

                sourceZoom--;
            }

            return null;
        }

        private static TileIndex GetEnclosingTileForOverzoomedPath(TileIndex index, int sourceZoom)
        {
            // For the overzoomed tile specified by path, figure out which tile from
            // level _mbtilesMaximumZ encloses that same location
            Debug.Assert(index.Zoom > sourceZoom && index.Zoom < 30, "GetEnclosingTileForOverzoomedPath assertion", "Path zoom: {0}, zoom: {1}", index.Zoom, sourceZoom);

            // Use integer division to get the quotient and discard the remainder...
            int divisor = 1 << (index.Zoom - sourceZoom);

            return new TileIndex(
                index.Column / divisor,
                index.Row / divisor,
                sourceZoom
            );
        }

        private static async Task<TileData?> ExtractTileAtPathAsync(TileIndex destPath, TileData tile, TileIndex sourcePath, CancellationToken cancellationToken)
        {
            Debug.Assert(sourcePath.Zoom < destPath.Zoom && destPath.Zoom < 30, "ExtractTileAtPathAsync assertion", "Source Zoom: {0}, Dest Zoom: {1}", sourcePath.Zoom, destPath.Zoom);

            // Calculate the path to use for cropping within the source tile. Note that the
            // coordinate system for UIImage is upsidedown from the XYZ tile coordinate system.
            //
            int normalizedSideLength = 1 << (destPath.Zoom - sourcePath.Zoom);
            float x = destPath.Column % normalizedSideLength;
            float y = destPath.Row % normalizedSideLength;

            // TODO: Pass in image type?
            using (var sourceImage = Image.Load(tile.Data))
            {
                float sourceImageHeight = sourceImage.Height;
                float sourceImageWidth = sourceImage.Width;

                float scalingHeight = sourceImageHeight / normalizedSideLength;
                float scalingWidth = sourceImageWidth / normalizedSideLength;

                // TODO: Further investigate when this happens.
                if (scalingHeight < 1 || scalingWidth < 1)
                {
                    return null;
                }

                // Calculate the rect to use for scaling
                //
                var scalingRect = new Rectangle(
                    /* x */ (int)(x * scalingWidth),
                    /* y */ (int)(y * scalingHeight),
                    /* width */ (int)scalingWidth,
                    /* height */ (int)scalingHeight);

                var destSize = new Size(
                    /* width */ 256,
                    /* height */ 256);

                sourceImage.Mutate(
                    context =>
                    {
                        context
                            .Crop(scalingRect)
                            .Resize(destSize);
                    });

                using (var memoryStream = new MemoryStream())
                {
                    await sourceImage.SaveAsPngAsync(memoryStream, cancellationToken).ConfigureAwait(false);

                    return new TileData(TileFormat.Png, memoryStream.GetBuffer());
                }
            }
        }
    }
}

