/*  This file is part of Chummer5a.
 *
 *  Chummer5a is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  Chummer5a is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with Chummer5a.  If not, see <http://www.gnu.org/licenses/>.
 *
 *  You can obtain the full source code for Chummer5a at
 *  https://github.com/chummer5a/chummer5a
 */
using System;
using System.Windows.Forms;
using Chummer;
using UserControl = System.Windows.Forms.UserControl;
using ChummerHub.Client.Sinners;

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
