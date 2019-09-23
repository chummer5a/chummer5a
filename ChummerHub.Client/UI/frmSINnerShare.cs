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
    public partial class frmSINnerShare : Form
    {
        public frmSINnerShare()
        {
            InitializeComponent();
            MyUcSINnerShare.MyFrmSINnerShare = this;
        }

        public ucSINnerShare MyUcSINnerShare
        {
            get { return ucSINnerShare1; }
        }

        
    }
}
