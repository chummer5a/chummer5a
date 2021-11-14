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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using NLog;

namespace Chummer
{
    public static class Timekeeper
    {
        private static Logger Log { get; } = LogManager.GetCurrentClassLogger();
        private static readonly Stopwatch s_Time = new Stopwatch();
        private static readonly LockingDictionary<string, TimeSpan> s_DictionaryStarts = new LockingDictionary<string, TimeSpan>();
        private static readonly LockingDictionary<string, Tuple<TimeSpan, int>> s_DictionaryStatistics = new LockingDictionary<string, Tuple<TimeSpan, int>>();

        static Timekeeper()
        {
            s_Time.Start();
        }

        public static CustomActivity StartSyncron(string taskname, CustomActivity parentActivity, CustomActivity.OperationType operationType, string target)
        {
            var dependencyActivity = new CustomActivity(taskname, parentActivity, operationType, target);
            s_DictionaryStarts.TryAdd(taskname, s_Time.Elapsed);
            return dependencyActivity;
        }

        public static CustomActivity StartSyncron(string taskname, CustomActivity parentActivity)
        {
            var dependencyActivity = new CustomActivity(taskname, parentActivity);
            s_DictionaryStarts.TryAdd(taskname, s_Time.Elapsed);
            return dependencyActivity;
        }

        public static TimeSpan Elapsed(string taskname)
        {
            if (s_DictionaryStarts.TryGetValue(taskname, out TimeSpan objStartTimeSpan))
            {
                return s_Time.Elapsed - objStartTimeSpan;
            }
            else
            {
                return TimeSpan.Zero;
            }
        }

        public static TimeSpan Finish(string taskname)
        {
            TimeSpan final = TimeSpan.Zero;
            if (s_DictionaryStarts.TryRemove(taskname, out TimeSpan objStartTimeSpan))
            {
                final = s_Time.Elapsed - objStartTimeSpan;

#if DEBUG
                string strLogEntry = string.Format(GlobalSettings.InvariantCultureInfo, "Task \"{0}\" finished in {1}",
                    taskname, final);
                //Log.Trace(strLogEntry);

                Debug.WriteLine(strLogEntry);
#endif

                if (s_DictionaryStatistics.TryGetValue(taskname, out Tuple<TimeSpan, int> existing))
                {
                    s_DictionaryStatistics[taskname] = new Tuple<TimeSpan, int>(existing.Item1 + final, existing.Item2 + 1);
                }
                else
                {
                    s_DictionaryStatistics.TryAdd(taskname, new Tuple<TimeSpan, int>(final, 1));
                }
            }
            else
            {
                Debug.WriteLine("Non started task \"" + taskname + "\" finished");
            }
            return final;
        }

        public static void MakeLog()
        {
            StringBuilder sbdLog = new StringBuilder("Time statistics" + Environment.NewLine);

            foreach (KeyValuePair<string, Tuple<TimeSpan, int>> keyValuePair in s_DictionaryStatistics)
            {
                sbdLog.AppendFormat(GlobalSettings.InvariantCultureInfo, "\t{0}({1}) = {2}{3}",
                    keyValuePair.Key, keyValuePair.Value.Item2, keyValuePair.Value.Item1, Environment.NewLine);
            }

            string strLog = sbdLog.ToString();
            Debug.WriteLine(strLog);
            Log.Info(strLog);
        }
    }
}
