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
﻿using System;
using System.Windows.Forms;
using Chummer.OmaeService;

namespace Chummer
{
    public partial class frmOmaeAccount : Form
    {
        private string _strUserName = string.Empty;
        private readonly OmaeHelper _objOmaeHelper = new OmaeHelper();

        #region Control Events
        public frmOmaeAccount(string strUserName)
        {
            InitializeComponent();
            LanguageManager.Instance.Load(GlobalOptions.Instance.Language, this);
            _strUserName = strUserName;
            MoveControls();
        }

        private void frmOmaeAccount_Load(object sender, EventArgs e)
        {
            omaeSoapClient objService = _objOmaeHelper.GetOmaeService();
            txtEmail.Text = objService.GetEmailAddress(_strUserName);
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void cmdOK_Click(object sender, EventArgs e)
        {
            omaeSoapClient objService = _objOmaeHelper.GetOmaeService();
            objService.SetEmailAddress(_strUserName, txtEmail.Text);
            DialogResult = DialogResult.OK;
        }
        #endregion

        #region Methods
        private void MoveControls()
        {
            txtEmail.Left = lblEmail.Left + lblEmail.Width + 6;
            txtEmail.Width = Width - txtEmail.Left - 19;
        }
        #endregion
    }
}