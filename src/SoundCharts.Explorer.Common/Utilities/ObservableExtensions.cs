using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reactive.Linq;

namespace SoundCharts.Explorer.Utilities;

public sealed record ChangeSet<T>(IImmutableSet<T> Added, IImmutableSet<T> Changed, IImmutableSet<T> Removed);

public static class ObservableExtensions
{
	public static IObservable<ChangeSet<T>> ToChangeSet<T>(this IObservable<IImmutableSet<T>> observable, IEqualityComparer<T>? comparer = default)
	{
		if (observable is null)
        {
			throw new ArgumentNullException(nameof(observable));
        }

		var emptySet = ImmutableHashSet.Create<T>(comparer);

		return observable
			.StartWith(emptySet)
			.Buffer(2, 1)
			.Select(
				buffer =>
                {
					var previous = buffer[0] ?? emptySet;
					var current = buffer[1] ?? emptySet;

					var added = current.Except(previous);
					var removed = previous.Except(current);

					return new ChangeSet<T>(added, emptySet, removed);
                });
	}
}

