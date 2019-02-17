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
            TimeSpan objStartTimeSpan;
            if (Starts.TryGetValue(taskname, out objStartTimeSpan))
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
            TimeSpan objStartTimeSpan;
            if (Starts.TryGetValue(taskname, out objStartTimeSpan))
            {
                TimeSpan final = time.Elapsed - objStartTimeSpan;

                Starts.Remove(taskname);
                string logentry = $"Task \"{taskname}\" finished in {final}";
                Chummer.Log.Info(logentry);

                Debug.WriteLine(logentry);

                Tuple<TimeSpan, int> existing;
                if (Statistics.TryGetValue(taskname, out existing))
                {
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
