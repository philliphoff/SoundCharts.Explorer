using System;
using System.Collections.Immutable;
using AppKit;
using Foundation;

namespace SoundCharts.Explorer.MacOS.Views.SourceList
{
	internal interface ISourceListDataSource : INSOutlineViewDataSource
	{
		IObservable<IImmutableSet<NSObject>> ChangedData { get; }
	}
}
