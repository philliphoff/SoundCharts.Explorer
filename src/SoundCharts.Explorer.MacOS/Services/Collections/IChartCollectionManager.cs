using System;
using System.Collections.Immutable;

namespace SoundCharts.Explorer.MacOS.Services.Collections
{
	internal sealed record ChartCollectionChart(Uri Name, string Title);

	internal sealed record ChartCollection(string Id, string Name, IImmutableSet<ChartCollectionChart> Charts);

	internal interface IChartCollectionManager
	{
		IObservable<IImmutableSet<ChartCollection>> Collections { get; }

		string AddLocalCollection(string name, string path);
		void RemoveCollection(string id);
	}
}
