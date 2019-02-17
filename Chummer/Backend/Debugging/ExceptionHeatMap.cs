using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Text;

namespace Chummer.Debugging
{
    class ExceptionHeatMap
    {
        Dictionary<string, int> _map = new Dictionary<string, int>();

        public void OnException(object sender, FirstChanceExceptionEventArgs e)
        {
            //Notes down the line number of every first chance exception.
            //Then counts the occurences. Should make it easier to find what throws the most exceptions
            StackTrace trace = new StackTrace(e.Exception, true);

            StackFrame frame = trace.GetFrame(0);
            // This kind of resolves a crash due to other applications querying Chummer's frames. 
            // Specifically, the NVDA screen reader. See https://github.com/chummer5a/chummer5a/issues/1888
            // In theory shouldn't mask any existing issues?
            if (frame == null) return;
            string heat = $"{frame.GetFileName()}:{frame.GetFileLineNumber()}";

            int intTmp;
            if (_map.TryGetValue(heat, out intTmp))
            {
                _map[heat] += intTmp + 1;
            }
            else
            {
                _map.Add(heat, 1);
            }
        }

        public string GenerateInfo()
        {
            StringBuilder builder = new StringBuilder(Environment.NewLine);
            int lenght = -1;
            IOrderedEnumerable<KeyValuePair<string, int>> exceptions = from i in _map
                orderby -i.Value
                select i;

            foreach (KeyValuePair<string, int> exception in exceptions)
            {
                builder.Append('\t'); builder.Append('\t');
                lenght = Math.Max((int)Math.Ceiling(Math.Log10(exception.Value)), lenght);
                builder.Append(exception.Value.ToString($"D{lenght}"));

                builder.Append(" - ").AppendLine(exception.Key);
            }

            return builder.ToString();
        }
    }
}
