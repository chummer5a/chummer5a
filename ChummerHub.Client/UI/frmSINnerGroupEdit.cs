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

        public frmSINnerGroupEdit(SINnerGroup group, bool onlyPWHash)
        {
            InitializeComponent();
            MySINnerGroupCreate.MyGroup = group;
            if (group.Id == null || group.Id == Guid.Empty)
                MySINnerGroupCreate.EditMode = true;
            MySINnerGroupCreate.InitializeMe(onlyPWHash);
        }
        

        public SINnerGroupCreate MySINnerGroupCreate { get { return this.siNnerGroupCreate1; } }
    }
}
