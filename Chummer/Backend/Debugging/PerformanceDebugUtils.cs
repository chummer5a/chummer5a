using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Chummer
{
	internal static class PerformanceDebugUtils
	{
		public static TimeSpan TaskEnd(this Stopwatch sw, string task)
		{
			TimeSpan elapsed;
			sw.Stop();

			elapsed = sw.Elapsed;
			Trace.WriteLine($"{task} finished in {elapsed.TotalMilliseconds} ms");
			
			sw.Restart();
			return elapsed;

		}

	}
}
