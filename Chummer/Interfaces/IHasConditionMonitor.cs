using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chummer
{
    interface IHasPhysicalConditionMonitor
    {
        int PhysicalCM { get; }
        int PhysicalCMFilled { get; set; }
    }
    interface IHasMatrixConditionMonitor
    {
        int MatrixCM { get; }
        int MatrixCMFilled { get; set; }
    }
    interface IHasStunConditionMonitor
    {
        int StunCM { get; }
        int StunCMFilled { get; set; }
    }
}
