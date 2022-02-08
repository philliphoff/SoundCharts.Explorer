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
	[Register ("SourceListViewController")]
	partial class SourceListViewController
	{
		[Outlet]
		SoundCharts.Explorer.MacOS.Views.SourceListView sourceListView { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (sourceListView != null) {
				sourceListView.Dispose ();
				sourceListView = null;
			}
		}
	}
}
