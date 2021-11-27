using System;
using System.Net.Http;
using AppKit;
using Foundation;
using MapKit;
using SoundCharts.Explorer.Tiles.Sources;

#nullable enable

namespace SoundCharts.Explorer.MacOS
{
	public partial class ViewController : NSViewController
	{
		private TileSourceOverlay? overlay;

		public ViewController (IntPtr handle) : base (handle)
		{
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			// Do any additional setup after loading the view.

			this.mapView.OverlayRenderer =
				(_, overlay) => overlay switch
				{
					MKTileOverlay tileOverlay => new MKTileOverlayRenderer(tileOverlay),
					_ => null! // TODO: Throw instead?
				};

			this.overlay = new TileSourceOverlay(new HttpTileSource(new HttpClient(), HttpTileSets.NoaaQuiltedTileSet));

			this.mapView.AddOverlay(this.overlay, MKOverlayLevel.AboveLabels);
		}

        protected override void Dispose(bool disposing)
        {
			try
            {
				if (this.overlay != null)
				{
					this.mapView.RemoveOverlay(this.overlay);
				}
            }
			finally
            {
				base.Dispose(disposing);
            }
        }

        public override NSObject RepresentedObject {
			get {
				return base.RepresentedObject;
			}
			set {
				base.RepresentedObject = value;
				// Update the view, if already loaded.
			}
		}
	}
}
