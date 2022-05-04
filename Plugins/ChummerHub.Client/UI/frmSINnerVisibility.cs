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
