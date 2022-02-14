using ChummerHub.Client.Sinners;
using System;
using System.Linq;
using System.Windows.Forms;


namespace ChummerHub.Client.UI
{
    public partial class frmSINnerVisibility : Form
    {
        public SINnerVisibility MyVisibility
        {
            get => ucSINnerVisibility1.MyVisibility;
            set => ucSINnerVisibility1.MyVisibility = value;
        }

        public frmSINnerVisibility()
        {
            InitializeComponent();
            DialogResult = DialogResult.Ignore;
            AcceptButton = bOk;

        }

        private void BOk_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            MyVisibility.IsGroupVisible = ucSINnerVisibility1.MyCheckBoxGroupVisible.Checked;
            MyVisibility.UserRights = MyVisibility.UserRightsObservable.ToList();
            Close();
        }
    }
}
