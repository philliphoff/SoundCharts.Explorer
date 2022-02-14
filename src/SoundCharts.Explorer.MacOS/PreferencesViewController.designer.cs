// WARNING
//
// This file has been generated automatically by Visual Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace SoundCharts.Explorer.MacOS
{
	[Register ("PreferencesViewController")]
	partial class PreferencesViewController
	{
		[Outlet]
		AppKit.NSTextField apiEndpointTextField { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (apiEndpointTextField != null) {
				apiEndpointTextField.Dispose ();
				apiEndpointTextField = null;
			}
		}
	}
}
