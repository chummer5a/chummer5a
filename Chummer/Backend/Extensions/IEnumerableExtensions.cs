using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chummer.Backend
{
	static class IEnumerableExtensions
	{
		public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
		{
			if(action == null) throw new ArgumentNullException(nameof(action));

			foreach (T t in enumerable)
			{
				action(t);
			}
		}
	}
}
