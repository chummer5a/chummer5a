using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chummer
{
    interface ICanSell : ICanRemove
    {
        void Sell(Character characterObject, decimal percentage);
    }
}
