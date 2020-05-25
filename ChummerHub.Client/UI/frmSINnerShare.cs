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

        public ucSINnerShare MyUcSINnerShare => ucSINnerShare1;
    }
}
