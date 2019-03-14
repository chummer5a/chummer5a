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
    public partial class frmSINnerGroupEdit : Form
    {
        //public frmSINnerGroupEdit()
        //{
        //    InitializeComponent();
        //}

        public frmSINnerGroupEdit(SINnerGroup group)
        {
            InitializeComponent();
            MySINnerGroupCreate.MyGroup = group;
            MySINnerGroupCreate.InitializeMe();
        }
        

        public SINnerGroupCreate MySINnerGroupCreate { get { return this.siNnerGroupCreate1; } }
    }
}
