using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chummer
{
    public interface ICanPaste
    {
        /// <summary>
        /// Does this object allow the current clipboard content to be pasted into it?
        /// </summary>
        bool AllowPasteXml { get; }

        bool AllowPasteObject(object input);
    }
}
