using System;
using System.Collections.Generic;

namespace UIWidgets {
	/// <summary>
	/// For each extensions.
	/// </summary>
	public static class Extensions
	{
		/// <summary>
		/// Foreach with index.
		/// </summary>
		/// <param name="enumerable">Enumerable.</param>
		/// <param name="handler">Handler.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T,int> handler)
		{
			int i = 0;
			foreach (T item in enumerable)
			{
				handler(item, i);
				i++;
			}
		}
		
		/// <summary>
		/// Foreach.
		/// </summary>
		/// <param name="enumerable">Enumerable.</param>
		/// <param name="handler">Handler.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> handler)
		{
			foreach (T item in enumerable)
			{
				handler(item);
			}
		}
		
		/// <summary>
		/// Convert IEnumerable<T> to ObservableList<T>.
		/// </summary>
		/// <returns>The observable list.</returns>
		/// <param name="enumerable">Enumerable.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public static ObservableList<T> ToObservableList<T>(this IEnumerable<T> enumerable)
		{
			return new ObservableList<T>(enumerable);
		}
		
		public static float SumFloat<T>(this IList<T> list, Func<T,float> calculate)
		{
			var result = 0f;
			for (int i = 0; i < list.Count; i++)
			{
				result += calculate(list[i]);
			}
			return result;
		}
		
		public static float SumFloat<T>(this ObservableList<T> list, Func<T,float> calculate)
		{
			var result = 0f;
			for (int i = 0; i < list.Count; i++)
			{
				result += calculate(list[i]);
			}
			return result;
		}
	}
}