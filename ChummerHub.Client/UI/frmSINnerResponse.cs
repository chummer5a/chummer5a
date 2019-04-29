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
    public partial class frmSINnerResponse : Form
    {
        public frmSINnerResponse()
        {
            InitializeComponent();
        }

        public SINnerResponseUI SINnerResponseUI
        {
            get { return this.siNnerResponseUI1; }
        }
    }
}
