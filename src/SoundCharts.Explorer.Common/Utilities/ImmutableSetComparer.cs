using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace SoundCharts.Explorer.Utilities
{
	public class ImmutableSetComparer<T> : EqualityComparer<IImmutableSet<T>>
	{
        public override bool Equals(IImmutableSet<T> x, IImmutableSet<T> y)
        {
            if (x is null && y is null)
            {
                return true;
            }
            else if (x?.Count != y?.Count)
            {
                return false;
            }
            else
            {
                return x.All(xn => y!.Contains(xn));
            }
        }

        public override int GetHashCode(IImmutableSet<T> obj)
        {
            var hashCode = new HashCode();

            if (obj is not null)
            {
                foreach (var value in obj)
                {
                    hashCode.Add(value);
                }
            }

            return hashCode.ToHashCode();
        }
    }
}

