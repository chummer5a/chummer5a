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
            this.CenterToParent();
            this.parentControl = init;
            
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

                this.nudInit.Value = Int32.Parse(objCharacter.InitiativePasses);
                this.txtName.Text = objCharacter.Name;
                this.nudInitStart.Value = Int32.Parse(objCharacter.Initiative.Split(' ')[0]);
                this._character = objCharacter;
            }
        }

        /// <summary>
        /// Closes the add token dialog
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// passes the character back to the dashboard init user control
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnOK_Click(object sender, EventArgs e)
        {
            if (this._character != null)
            {
                this._character.InitRoll = this.chkAutoRollInit.Checked ? new Random().Next((int)this.nudInit.Value, ((int)this.nudInit.Value) * 6) + ((int)this.nudInitStart.Value) : Int32.MinValue;
                this._character.InitialInit = (int)this.nudInitStart.Value;
                this._character.Delayed = false;
                this._character.InitPasses = (int)this.nudInit.Value;
                this._character.Name = this.txtName.Text;
            }
            else
                this._character = new Character()
                {
                    Name = this.txtName.Text,
                    InitPasses = (int)this.nudInit.Value,
                    InitRoll = this.chkAutoRollInit.Checked ? new Random().Next((int)this.nudInit.Value, ((int)this.nudInit.Value) * 6) + ((int)this.nudInitStart.Value) : Int32.MinValue,
                    Delayed = false,
                    InitialInit = (int)this.nudInitStart.Value
                };
            this.parentControl.AddToken(this._character);
            this.Close();
        }
    }
}
