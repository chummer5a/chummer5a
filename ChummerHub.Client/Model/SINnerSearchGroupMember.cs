using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SINners.Models
{
    public partial class SINnerSearchGroupMember
    {
        public string Display
        {
            get
            {
                string display = this.MySINner.Alias;
                if (!String.IsNullOrEmpty(Username))
                    display += " " + this.Username;
                return display;
            }
        }
    }
}
