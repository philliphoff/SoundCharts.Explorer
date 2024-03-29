﻿using Foundation;

namespace SoundCharts.Explorer.MacOS.Views.SourceList
{
	internal abstract class SourceListItem : NSObject
	{
		public SourceListItem(string title)
		{
			this.Title = title;
		}

		public string Title { get; }
	}
}
