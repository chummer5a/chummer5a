using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chummer
{
    internal interface IHasAvailability
    {
        AvailabilityValue TotalAvailTuple(bool blnCheckChildren = true);
    }
}
