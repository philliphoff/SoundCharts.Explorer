// WARNING
//
// This file has been generated automatically by Visual Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace SoundCharts.Explorer.MacOS.Views.SourceList
{
	partial class OfflineTilesetsSwitchView
	{
		[Outlet]
		AppKit.NSSwitch offlineSwitch { get; set; }

		[Action ("OnOfflineSwitchAction:")]
		partial void OnOfflineSwitchAction (Foundation.NSObject sender);
		
		void ReleaseDesignerOutlets ()
		{
			if (offlineSwitch != null) {
				offlineSwitch.Dispose ();
				offlineSwitch = null;
			}
		}
	}
}
