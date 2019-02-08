using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chummer
{
    interface IHasStolenProperty
    {
        bool Stolen { get; set; }

        decimal StolenTotalCost { get; }
    }
}
