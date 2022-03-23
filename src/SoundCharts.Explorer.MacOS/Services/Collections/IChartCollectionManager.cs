using System;
using System.Collections.Immutable;

namespace SoundCharts.Explorer.MacOS.Services.Collections
{
	internal abstract record ChartCollection(string Name, IImmutableSet<string> Charts);

	internal interface IChartCollectionManager
	{
		IObservable<IImmutableSet<ChartCollection>> Collections { get; }

		void AddLocalCollection(string name, string path);
		void RemoveCollection(ChartCollection collection);
	}
}
