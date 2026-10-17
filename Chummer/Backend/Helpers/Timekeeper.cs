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

// Uncomment this define to control whether timer data is collected into a dictionary that can be printed into a log later on
//#define COLLECTTIMERDATA

using System;
#if COLLECTTIMERDATA || DEBUG
using System.Collections.Concurrent;
using System.Diagnostics;
#if COLLECTTIMERDATA
using System.Collections.Generic;
using System.Text;
#endif
#endif
using NLog;

namespace Chummer
{
    public static class Timekeeper
    {
        private static readonly Lazy<Logger> s_ObjLogger = new Lazy<Logger>(LogManager.GetCurrentClassLogger);
        private static Logger Log => s_ObjLogger.Value;
#if COLLECTTIMERDATA || DEBUG
        private static readonly Stopwatch s_Time = new Stopwatch();
        private static readonly ConcurrentDictionary<string, TimeSpan> s_DictionaryStarts = new ConcurrentDictionary<string, TimeSpan>();
#if COLLECTTIMERDATA
        private static readonly ConcurrentDictionary<string, ValueTuple<TimeSpan, int>> s_DictionaryStatistics = new ConcurrentDictionary<string, ValueTuple<TimeSpan, int>>();
#endif
#endif

        static Timekeeper()
        {
#if COLLECTTIMERDATA|| DEBUG
            s_Time.Start();
#endif
        }

        [CLSCompliant(false)]
        public static CustomActivity StartSyncron(string taskname, CustomActivity parentActivity, CustomActivity.OperationType operationType, string target)
        {
            CustomActivity dependencyActivity = new CustomActivity(taskname, parentActivity, operationType, target);
#if COLLECTTIMERDATA|| DEBUG
            s_DictionaryStarts.TryAdd(taskname, s_Time.Elapsed);
#endif
            return dependencyActivity;
        }

        [CLSCompliant(false)]
        public static CustomActivity StartSyncron(string taskname, CustomActivity parentActivity)
        {
            CustomActivity dependencyActivity = new CustomActivity(taskname, parentActivity);
#if COLLECTTIMERDATA || DEBUG
            s_DictionaryStarts.TryAdd(taskname, s_Time.Elapsed);
#endif
            return dependencyActivity;
        }

        public static TimeSpan Elapsed(string taskname)
        {
#if COLLECTTIMERDATA || DEBUG
            if (s_DictionaryStarts.TryGetValue(taskname, out TimeSpan objStartTimeSpan))
            {
                return s_Time.Elapsed - objStartTimeSpan;
            }

            return TimeSpan.Zero;
#else
            throw new NotSupportedException();
#endif
        }

#if COLLECTTIMERDATA || DEBUG
        public static TimeSpan Finish(string taskname)
        {
            TimeSpan final = TimeSpan.Zero;
            if (s_DictionaryStarts.TryRemove(taskname, out TimeSpan objStartTimeSpan))
            {
                final = s_Time.Elapsed - objStartTimeSpan;

#if DEBUG
                string strLogEntry = string.Concat("Task \"", taskname, "\" finished in ", final.ToString("c", GlobalSettings.InvariantCultureInfo));
                //Log.Trace(strLogEntry);

                Debug.WriteLine(strLogEntry);
#endif
#if COLLECTTIMERDATA
                s_DictionaryStatistics.AddOrUpdate(taskname, _ => new ValueTuple<TimeSpan, int>(final, 1),
                                                   (_, y) => new ValueTuple<TimeSpan, int>(
                                                       y.Item1 + final, y.Item2 + 1));
#endif
            }
            else
            {
                Debug.WriteLine("Non started task \"" + taskname + "\" finished");
            }
            return final;
        }
#else
        public static void Finish(string taskname)
        {
            // Deliberately empty so that we can still call this function, but the compiler will error out if we'd want to use the TimeSpan this function would normally create
        }
#endif

        public static void MakeLog()
        {
#if COLLECTTIMERDATA
            string strLog;
            using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool,
                                                          out StringBuilder sbdLog))
            {
                sbdLog.AppendLine("Time statistics");
                foreach (KeyValuePair<string, ValueTuple<TimeSpan, int>> keyValuePair in s_DictionaryStatistics)
                {
                    sbdLog.Append('\t', keyValuePair.Key)
                        .Append('(', keyValuePair.Value.Item2.ToString(GlobalSettings.InvariantCultureInfo), ')')
                        .AppendLine(" = ", keyValuePair.Value.Item1.ToString());
                }

                strLog = sbdLog.ToString();
            }

            Debug.WriteLine(strLog);
            Log.Info(strLog);
#else
            throw new NotSupportedException();
#endif
        }
    }
}
