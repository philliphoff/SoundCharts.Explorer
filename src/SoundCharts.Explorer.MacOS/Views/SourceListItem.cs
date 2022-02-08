using System;
using Foundation;

namespace SoundCharts.Explorer.MacOS.Views
{
	public class SourceListItem : NSObject
	{
		public SourceListItem(string title)
		{
			this.Title = title;
		}

		public string Title { get; }
	}
}
