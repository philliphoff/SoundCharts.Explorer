using System;
using MapKit;

namespace SoundCharts.Explorer.MacOS
{
    internal sealed class TileSourceOverlayRenderer : MKTileOverlayRenderer
    {
        private readonly TileSourceOverlay overlay;

        public TileSourceOverlayRenderer(TileSourceOverlay overlay)
            : base(overlay)
        {
            this.overlay = overlay;
            this.overlay.NeedsReload += this.OnNeedsReload;
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    this.overlay.NeedsReload -= this.OnNeedsReload;
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        private void OnNeedsReload(object sender, EventArgs e)
        {
            this.ReloadData();
        }
    }
}
