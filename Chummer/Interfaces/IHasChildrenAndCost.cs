using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chummer
{
    public interface IHasChildrenAndCost<T> : IHasChildren<T>
    {
        decimal TotalCost { get; }
        int ChildCostMultiplier { get; }
    }
}
