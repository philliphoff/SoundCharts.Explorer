using System;
using System.Globalization;
using System.Threading.Tasks;
using AppKit;
using Foundation;
using ObjCRuntime;

namespace SoundCharts.Explorer.MacOS.Views.SourceList
{
	[Register("OfflineTilesetView")]
	public partial class OfflineTilesetView : NSView
	{
        private Func<Task>? onAction;

        public OfflineTilesetView(NativeHandle handle)
            : base(handle)
        {
        }

        public override void AwakeFromNib()
        {
            base.AwakeFromNib();

            this.ActionButton.Hidden = true;

            var area = new NSTrackingArea(this.Bounds, NSTrackingAreaOptions.ActiveAlways | NSTrackingAreaOptions.MouseEnteredAndExited, this, null);

            this.AddTrackingArea(area);
        }

        public void Initialize(string title, string description, Func<Task> onAction, bool isDownloadAction)
        {
            var culture = CultureInfo.CurrentCulture;

            // NOTE: TextInfo.ToTitleCase() will not modify all-uppercase words, so first convert to lowercase.
            this.TitleTextField.StringValue = culture.TextInfo.ToTitleCase(title.ToLower(culture));

            this.DescriptionTextField.StringValue = description;

            if (isDownloadAction)
            {
                this.ActionButton.Image = NSImage.GetSystemSymbol("multiply.circle", null);
                this.TilesetImage.Image = NSImage.GetSystemSymbol("map.circle", null);
            }
            else
            {
                this.ActionButton.Image = NSImage.GetSystemSymbol("multiply.circle.fill", null);
                this.TilesetImage.Image = NSImage.GetSystemSymbol("map.circle.fill", null);
            }

            this.onAction = onAction;
        }

        partial void OnActionButtonAction(NSObject sender)
        {
            this.PerformActionAsync()
                .ContinueWith(_ => { /* TODO: Handle errors. */ });
        }

        public override void MouseEntered(NSEvent theEvent)
        {
            this.ActionButton.Hidden = false;

            base.MouseEntered(theEvent);
        }

        public override void MouseExited(NSEvent theEvent)
        {
            this.ActionButton.Hidden = true;

            base.MouseExited(theEvent);
        }

        private async Task PerformActionAsync()
        {
            var action = this.onAction;

            if (action is not null)
            {
                this.ActionButton.Enabled = false;

                try
                {
                    await action();
                }
                finally
                {
                    this.ActionButton.Enabled = true;
                }
            }
        }
    }
}

