using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Chummer
{
    public interface IHasWirelessBonus
    {
        bool WirelessOn { get; set; }

        XmlNode WirelessBonus { get; }
    }
}
