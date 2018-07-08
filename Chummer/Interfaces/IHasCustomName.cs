using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chummer
{
    interface IHasCustomName
    {
        string DisplayName(string s);

        string CustomName { get; set; }
    }
}
