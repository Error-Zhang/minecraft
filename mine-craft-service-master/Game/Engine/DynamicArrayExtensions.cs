using System.Collections.Generic;

namespace Engine
{
	public static class DynamicArrayExtensions
	{
		public static DynamicArray<T> ToDynamicArray<T>(this IEnumerable<T> source)
		{
			return new DynamicArray<T>(source);
		}

        public static DynamicArray<T> ToDynamicArray<T>(this IReadOnlyCollection<T> source)
        {
            return new DynamicArray<T>(source);
        }

        public static DynamicArray<T> ToDynamicArray<T>(this IList<T> source)
        {
            return new DynamicArray<T>(source);
        }

        public static DynamicArray<T> ToDynamicArray<T>(this IReadOnlyList<T> source)
        {
            return new DynamicArray<T>(source);
        }

        public static DynamicArray<T> ToDynamicArray<T>(this DynamicArray<T> source)
        {
            return new DynamicArray<T>(source);
        }
	}
}