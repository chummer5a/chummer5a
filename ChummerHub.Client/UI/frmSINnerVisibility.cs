using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ChummerHub.Client.UI
{
    public partial class frmSINnerVisibility : Form
    {
        public SINners.Models.SINnerVisibility MyVisibility
        {
            get { return this.ucSINnerVisibility1.MyVisibility; }
            set { this.ucSINnerVisibility1.MyVisibility = value; }
        }

        public frmSINnerVisibility()
        {
            InitializeComponent();
            this.DialogResult = DialogResult.Ignore;
            this.AcceptButton = bOk;
        }

        

        private void BOk_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
