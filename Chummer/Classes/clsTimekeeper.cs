/*  This file is part of Chummer5a.
 *
 *  Chummer5a is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  Chummer5a is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with Chummer5a.  If not, see <http://www.gnu.org/licenses/>.
 *
 *  You can obtain the full source code for Chummer5a at
 *  https://github.com/chummer5a/chummer5a
 */
﻿using System;
using System.Collections.Concurrent;
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
        private static readonly ConcurrentDictionary<String, TimeSpan> Starts = new ConcurrentDictionary<string, TimeSpan>(); 
        private static readonly ConcurrentDictionary<string, Tuple<TimeSpan, int>> Statistics = new ConcurrentDictionary<string, Tuple<TimeSpan, int>>();

        static Timekeeper ()
        {
            time.Start();
        }

        public static void Start(string taskname)
        {
            Starts.TryAdd(taskname, time.Elapsed);
        }

        public static TimeSpan Elapsed(string taskname)
        {
            if (Starts.TryGetValue(taskname, out TimeSpan objStartTimeSpan))
            {
                return time.Elapsed - objStartTimeSpan;
            }
            else
            {
                return TimeSpan.Zero;
            }
        }

        public static TimeSpan Finish(string taskname)
        {
            if (Starts.TryRemove(taskname, out TimeSpan objStartTimeSpan))
            {
                TimeSpan final = time.Elapsed - objStartTimeSpan;

                string logentry = $"Task \"{taskname}\" finished in {final}";
                Chummer.Log.Info(logentry);

                Debug.WriteLine(logentry);

                if (Statistics.TryGetValue(taskname, out Tuple<TimeSpan, int> existing))
                {
                    Statistics[taskname] = new Tuple<TimeSpan, int>(existing.Item1 + final, existing.Item2 + 1);
                }
                else
                {
                    Statistics.TryAdd(taskname, new Tuple<TimeSpan, int>(final, 1));
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
