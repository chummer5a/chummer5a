using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chummer
{
    public interface ISustainable
    {
        int Force { get; set; }
        int NetHits { get; set; }
        bool SelfSustained { get; set; }
    }
}
