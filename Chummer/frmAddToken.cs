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
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Chummer
{
    public partial class frmAddToken : Form
    {
        // used when the user has filled out the information
        private InitiativeUserControl parentControl;
        private Character _character;

        public frmAddToken(InitiativeUserControl init)
        {
            InitializeComponent();
            //LanguageManager.Instance.Load(GlobalOptions.Instance.Language, this);
            CenterToParent();
            parentControl = init;
            
        }

        /// <summary>
        /// Show the Open File dialogue, then load the selected character.
        /// </summary>
        private void OpenFile(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Chummer5 Files (*.chum5)|*.chum5|All Files (*.*)|*.*";

            if (openFileDialog.ShowDialog(this) == DialogResult.OK)
                LoadCharacter(openFileDialog.FileName);
        }

        /// <summary>
        /// Loads the character
        /// </summary>
        /// <param name="fileName"></param>
        private void LoadCharacter(string fileName)
        {
            if (File.Exists(fileName) && fileName.EndsWith("chum5"))
            {
                bool blnLoaded = false;
                Character objCharacter = new Character();
                objCharacter.FileName = fileName;
                blnLoaded = objCharacter.Load();

                if (!blnLoaded)
                {
                    ;   // TODO edward setup error page
                    return; // we obviously cannot init
                }

                nudInit.Value = objCharacter.InitiativeDice;
                txtName.Text = objCharacter.Name;
                nudInitStart.Value = Int32.Parse(objCharacter.Initiative.Split(' ')[0]);
                _character = objCharacter;
            }
        }

        /// <summary>
        /// Closes the add token dialog
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        /// <summary>
        /// passes the character back to the dashboard init user control
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnOK_Click(object sender, EventArgs e)
        {
            if (_character != null)
            {
                _character.InitRoll = chkAutoRollInit.Checked ? new Random().Next((int)nudInit.Value, ((int)nudInit.Value) * 6) + ((int)nudInitStart.Value) : Int32.MinValue;
                _character.InitialInit = (int)nudInitStart.Value;
                _character.Delayed = false;
                _character.InitPasses = (int)nudInit.Value;
                _character.Name = txtName.Text;
            }
            else
                _character = new Character()
                {
                    Name = txtName.Text,
                    InitPasses = (int)nudInit.Value,
                    InitRoll = chkAutoRollInit.Checked ? new Random().Next((int)nudInit.Value, ((int)nudInit.Value) * 6) + ((int)nudInitStart.Value) : Int32.MinValue,
                    Delayed = false,
                    InitialInit = (int)nudInitStart.Value
                };
            parentControl.AddToken(_character);
            Close();
        }
    }
}
