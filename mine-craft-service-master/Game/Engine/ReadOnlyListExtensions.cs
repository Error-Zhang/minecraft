using System.Collections.Generic;
using System.Linq;

namespace Engine;

public static class ReadOnlyListExtensions
{
	public static ReadOnlyList<T> ToReadOnlyList<T>(this IEnumerable<T> source)
	{
		return new ReadOnlyList<T>(source.ToArray());
	}

	public static ReadOnlyList<T> ToReadOnlyList<T>(this IList<T> source)
	{
		return new ReadOnlyList<T>(source);
	}
}
