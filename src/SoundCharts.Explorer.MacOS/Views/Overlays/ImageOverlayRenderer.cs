using System;
using AppKit;
using CoreGraphics;
using MapKit;
using ObjCRuntime;

namespace SoundCharts.Explorer.MacOS.Views.Overlays
{
	internal sealed class ImageOverlayRenderer : MKOverlayRenderer
	{
		public ImageOverlayRenderer(ImageOverlay overlay)
			: base(overlay)
		{
		}

        public override void DrawMapRect(MKMapRect mapRect, nfloat zoomScale, CGContext context)
        {
			var overlay = (ImageOverlay)this.Overlay;

			var drawRect = this.RectForMapRect(overlay.BoundingMapRect);

			NSGraphicsContext.GlobalSaveGraphicsState();

			var ctx = NSGraphicsContext.FromCGContext(context, false);

			NSGraphicsContext.CurrentContext = ctx;

			try
            {
				overlay.Image.Draw(drawRect);
            }
			finally
            {
				NSGraphicsContext.GlobalRestoreGraphicsState();
            }

            base.DrawMapRect(mapRect, zoomScale, context);
        }
    }
}

