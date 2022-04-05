using System.Collections.Generic;
using System.Linq;

namespace SoundCharts.Explorer.Utilities;

public static class EnumerableExtensions
{
    public static IEnumerable<T> WhereNonNull<T>(this IEnumerable<T?> values)
    {
        return values
            .Where(value => value is not null)
            .Select(value => value!);
    }
}
