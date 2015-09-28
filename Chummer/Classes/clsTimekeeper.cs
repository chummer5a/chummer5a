using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Chummer
{
	static class Timekeeper
	{
		static Stopwatch time = new Stopwatch();
		private static readonly Dictionary<String, TimeSpan> Starts = new Dictionary<string, TimeSpan>(); 
		private static readonly Dictionary<string, Tuple<TimeSpan, int>> Statistics = new Dictionary<string, Tuple<TimeSpan, int>>();

		static Timekeeper ()
		{
			time.Start();
		}

		public static void Start(string taskname)
		{
			if (!Starts.ContainsKey(taskname))
			{
				Starts.Add(taskname, time.Elapsed);
			}
		}

		public static TimeSpan Elapsed(string taskname)
		{
			if (Starts.ContainsKey(taskname))
			{
				return time.Elapsed - Starts[taskname];
			}
			else
			{
				return TimeSpan.Zero;
			}
		}

		public static TimeSpan Finish(string taskname)
		{
			if (Starts.ContainsKey(taskname))
			{
				TimeSpan final = time.Elapsed - Starts[taskname];

				Starts.Remove(taskname);
				string logentry = $"Task \"{taskname}\" finished in {final}";
                Chummer.Log.Info(logentry);

				Debug.WriteLine(logentry);
				
				if (Statistics.ContainsKey(taskname))
				{
					Tuple<TimeSpan, int> existing = Statistics[taskname];
					Statistics[taskname] = new Tuple<TimeSpan, int>(existing.Item1 + final, existing.Item2 + 1);
				}
				else
				{
					Statistics.Add(taskname, new Tuple<TimeSpan, int>(final, 1));
				}
				
				return final;
			}
			else
			{
				Debug.WriteLine("Non started task \"" + taskname + "\" finished");
				return TimeSpan.Zero;
			}
		}

		public static void Log()
		{
			StringBuilder sb = new StringBuilder("Time statistics\n");

			foreach (KeyValuePair<string, Tuple<TimeSpan, int>> keyValuePair in Statistics)
			{
				sb.AppendLine($"\t{keyValuePair.Key}({keyValuePair.Value.Item2}) = {keyValuePair.Value.Item1}");
			}

			string strined = sb.ToString();
			Debug.WriteLine(strined);
			Chummer.Log.Info(strined);
		}

	}
}
