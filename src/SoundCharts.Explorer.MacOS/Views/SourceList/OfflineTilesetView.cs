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
    }
}

