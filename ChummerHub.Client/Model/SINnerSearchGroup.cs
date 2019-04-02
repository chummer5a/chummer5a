using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chummer;

namespace SINners.Models
{
    public partial class SINnerSearchGroup
    {
        public override string ToString()
        {
            return GroupDisplayname;
        }
        public string GroupDisplayname
        {
            get
            {
                string ret = Groupname;
                if (!(String.IsNullOrEmpty(Language)))
                {
                    var list = frmOptions.GetSheetLanguageList();
                    var found = from a in list where a.Value != null && (string)a.Value == Language select a;
                    if (found.Any())
                        ret += " - " + found.FirstOrDefault();
                    else
                        ret += " - " + Language;
                    if ((this.MyMembers != null)
                        && (this.MyMembers.Any()))
                    {
                        ret += ": " + MyMembers.Count;
                    }
                }
                return ret;
            }
        }
    }
}
