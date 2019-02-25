using SINners.Models;
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
    public partial class frmSINnerGroupSearch : Form
    {
        private SINner MySinner { get; }
        public frmSINnerGroupSearch(SINner sinner)
        {
            MySinner = sinner;
            InitializeComponent();
            this.siNnerGroupSearch1.MySinner = MySinner;
        }
    }
}
