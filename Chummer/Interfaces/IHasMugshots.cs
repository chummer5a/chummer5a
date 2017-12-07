using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Chummer
{
    public interface IHasMugshots
    {
        List<Image> Mugshots { get; }
        Image MainMugshot { get; set; }
        int MainMugshotIndex { get; set; }

        void SaveMugshots(XmlTextWriter objWriter);
        void LoadMugshots(XmlNode xmlSavedNode);
        void PrintMugshots(XmlTextWriter objWriter);
    }
}
