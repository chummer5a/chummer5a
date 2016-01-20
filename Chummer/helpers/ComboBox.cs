using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chummer.helpers
{
    class ComboBox : System.Windows.Forms.ComboBox
    {
        public bool IsInitalized(bool isLoading)
        {
            return (isLoading || this.SelectedValue == null || string.IsNullOrEmpty(this.SelectedValue.ToString()));
        }
    }
}
