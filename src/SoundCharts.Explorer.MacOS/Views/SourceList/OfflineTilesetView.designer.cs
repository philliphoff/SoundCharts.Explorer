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
	partial class OfflineTilesetView
	{
		[Outlet]
		AppKit.NSTextField DescriptionTextField { get; set; }

		[Outlet]
		AppKit.NSTextField TitleTextField { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (TitleTextField != null) {
				TitleTextField.Dispose ();
				TitleTextField = null;
			}

			if (DescriptionTextField != null) {
				DescriptionTextField.Dispose ();
				DescriptionTextField = null;
			}
		}
	}
}
