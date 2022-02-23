using System;
using System.Threading.Tasks;
using AppKit;
using Foundation;

namespace SoundCharts.Explorer.MacOS.Views.SourceList
{
	[Register("OfflineTilesetView")]
	public partial class OfflineTilesetView : NSView
	{
        private Func<Task>? onAction;

        public OfflineTilesetView(IntPtr handle)
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

        public void Initialize(string title, string description, Func<Task> onAction)
        {
            this.TitleTextField.StringValue = title;
            this.DescriptionTextField.StringValue = description;

            this.onAction = onAction;
        }

        partial void OnActionButtonAction(NSObject sender)
        {
            this.onAction?.Invoke();
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
    }
}

