using System;
using System.Windows.Forms;
using Chummer;
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
            tbSINnerResponseMyExpection.SetToolTip("In case you want to report this error, please make sure that the whole errortext is visible/available for the developer.");
        }

        private void BOk_Click(object sender, EventArgs e)
        {
            Control found = Parent;
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
                tbSINnerResponseErrorText.Text = Result.ErrorText;
                tbSINnerResponseMyExpection.Text = Result.MyException?.ToString();
                tbInstallationId.Text = Chummer.Properties.Settings.Default.UploadClientId.ToString();
            }
        }
    }
}
