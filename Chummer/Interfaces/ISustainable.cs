using Chummer.Backend.Skills;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using NLog;

namespace Chummer
{
    public interface ISustainable
    {
        int Force { get; set; }
        int NetHits { get; set; }
        bool SelfSustained { get; set; }

        void Load(XmlNode objNode);
        void Print(XmlTextWriter objWriter, CultureInfo objCulture, string strLanguageToPrint);
        void Save(XmlTextWriter objWriter);
    }
}
