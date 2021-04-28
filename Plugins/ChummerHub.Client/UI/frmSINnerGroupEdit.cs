using System;
using System.Windows.Forms;
using ChummerHub.Client.Sinners;
using NLog;

namespace ChummerHub.Client.UI
{
    public partial class frmSINnerGroupEdit : Form
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        //public frmSINnerGroupEdit()
        //{
        //    InitializeComponent();
        //}

        public frmSINnerGroupEdit(SINnerGroup group, bool onlyPWHash)
        {
            InitializeComponent();
            MySINnerGroupCreate.MyGroup = group;
            if (group?.Id == null || group.Id == Guid.Empty)
                MySINnerGroupCreate.EditMode = true;
            MySINnerGroupCreate.InitializeMe(onlyPWHash);
        }

        public ucSINnerGroupCreate MySINnerGroupCreate => siNnerGroupCreate1;
    }
}
