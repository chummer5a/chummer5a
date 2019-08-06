using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Forms;
using Chummer;
using Control = System.Windows.Controls.Control;
using UserControl = System.Windows.Forms.UserControl;
using ChummerHub.Client.Model;

namespace ChummerHub.Client.UI
{
    public partial class ucSINnerResponseUI : UserControl
    {
        public ResultBase Result { get; internal set; }

        public ucSINnerResponseUI()
        {
            InitializeComponent();
            this.tbSINnerResponseMyExpection.SetToolTip("In case you want to report this error, please make sure that the whole errortext is visible/available for the developer.");
          
        }

        private void BOk_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.Control found = this.Parent;
            while (found != null)
            {
                if (found is Form foundForm)
                {
                    foundForm.Close();
                    return;
                }
                found = found.Parent;
            }
        }

        private void TbSINnerResponseErrorText_VisibleChanged(object sender, EventArgs e)
        {
            if (Result != null)
            {
                this.tbSINnerResponseErrorText.Text = Result.ErrorText;
                this.tbSINnerResponseMyExpection.Text = Result.MyException?.ToString();
            }
        }
    }
}
