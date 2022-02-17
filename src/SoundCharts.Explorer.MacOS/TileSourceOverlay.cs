using System;
using Foundation;
using MapKit;
using SoundCharts.Explorer.Tiles;

namespace SoundCharts.Explorer.MacOS
{
	internal sealed class TileSourceOverlay : MKTileOverlay
	{
		private readonly IObservableTileSource tileSource;

		public TileSourceOverlay(IObservableTileSource tileSource)
		{
			this.tileSource = tileSource ?? throw new ArgumentNullException(nameof(tileSource));
            this.tileSource.TilesChanged += this.OnTilesChanged;
		}

        public event EventHandler? NeedsReload;

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
            catch
            {
                // TODO: Return NSError?
            }

            // NOTE: Arguments may be null.
            result(encodedImage!, error!);
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    this.tileSource.TilesChanged -= this.OnTilesChanged;
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        private void OnTilesChanged(object sender, TilesChangedEventArgs e)
        {
            this.NeedsReload?.Invoke(this, EventArgs.Empty);
        }
    }
}

