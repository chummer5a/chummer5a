using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chummer
{
    public interface IHasNotes
    {
        string Notes { get; set; }

        Color PreferredColor { get; }
    }
}
