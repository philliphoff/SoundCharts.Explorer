using System;
using Foundation;
using MapKit;
using ObjCRuntime;
using SoundCharts.Explorer.Tiles;

namespace SoundCharts.Explorer.MacOS
{
	public class TileSourceOverlay : MKTileOverlay
	{
		private readonly ITileSource tileSource;

		public TileSourceOverlay(ITileSource tileSource)
		{
			this.tileSource = tileSource ?? throw new ArgumentNullException(nameof(tileSource));
		}

        public override async void LoadTileAtPath(MKTileOverlayPath path, /* [BlockProxy(typeof(NIDMKTileOverlayLoadTileCompletionHandler))] */ MKTileOverlayLoadTileCompletionHandler result)
        {
            NSData? encodedImage = null;
            NSError? error = null;

            var index = new TileIndex((int)path.X, (int)path.Y, (int)path.Z);

            try
            {
                // TODO: Add timeout?
                var data = await this.tileSource.GetTileAsync(index);

                if (data != null)
                {
                    encodedImage = NSData.FromArray(data.Data);
                }
            }
            catch (Exception e)
            {
                // TODO: Return NSError?
            }

            // NOTE: Arguments may be null.
            result(encodedImage!, error!);

        }
    }
}

