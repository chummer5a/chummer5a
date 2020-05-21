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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Text;

namespace Chummer.Backend
{
    public sealed class ExceptionHeatMap
    {
        readonly ConcurrentDictionary<string, int> _map = new ConcurrentDictionary<string, int>();

        public void OnException(object sender, FirstChanceExceptionEventArgs e)
        {
            if (e == null)
                return;
            //Notes down the line number of every first chance exception.
            //Then counts the occurrences. Should make it easier to find what throws the most exceptions
            StackTrace trace = new StackTrace(e.Exception, true);

            StackFrame frame = trace.GetFrame(0);
            // This kind of resolves a crash due to other applications querying Chummer's frames.
            // Specifically, the NVDA screen reader. See https://github.com/chummer5a/chummer5a/issues/1888
            // In theory shouldn't mask any existing issues?
            if (frame == null)
                return;
            string heat = $"{frame.GetFileName()}:{frame.GetFileLineNumber()}";

            if (_map.TryGetValue(heat, out int intTmp))
            {
                _map[heat] += intTmp + 1;
            }
            else
            {
                _map.TryAdd(heat, 1);
            }
        }

        public string GenerateInfo()
        {
            StringBuilder builder = new StringBuilder(Environment.NewLine);
            int length = -1;
            IOrderedEnumerable<KeyValuePair<string, int>> exceptions = from i in _map
                orderby -i.Value
                select i;

            foreach (KeyValuePair<string, int> exception in exceptions)
            {
                builder.Append('\t'); builder.Append('\t');
                length = Math.Max((int)Math.Ceiling(Math.Log10(exception.Value)), length);
                builder.Append(exception.Value.ToString($"D{length}", GlobalOptions.InvariantCultureInfo));

                builder.Append(" - ").AppendLine(exception.Key);
            }

            return builder.ToString();
        }
    }
}
