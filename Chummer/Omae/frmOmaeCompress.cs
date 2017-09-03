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
using System.Collections.Generic;
using System.IO;
using System.ServiceModel;
using System.Windows.Forms;
using Chummer.OmaeService;

namespace Chummer
{
    public partial class frmOmaeCompress : Form
    {
        private readonly OmaeHelper _objOmaeHelper = new OmaeHelper();
        private List<string> _lstFiles = new List<string>();

        #region Control Events
        public frmOmaeCompress()
        {
            InitializeComponent();
        }

        private void cmdCompress_Click(object sender, EventArgs e)
        {
            _lstFiles.Clear();
            GetDirectories(txtFilePath.Text);
            _objOmaeHelper.CompressMutipleToFile(_lstFiles, txtDestination.Text);
            MessageBox.Show("Done");
        }
        #endregion

        #region Methods
        private void GetDirectories(string strDirectory)
        {
            foreach (string strFound in Directory.GetDirectories(strDirectory))
            {
                GetDirectories(strFound);
            }
            foreach (string strFile in Directory.GetFiles(strDirectory, "*.chum5"))
                _lstFiles.Add(strFile);
        }
        #endregion
    }
}