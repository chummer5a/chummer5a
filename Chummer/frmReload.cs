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
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
 using Chummer.Backend.Equipment;

namespace Chummer
{
    public partial class frmReload : Form
    {
        private List<Gear> _lstAmmo = new List<Gear>();
        private List<string> _lstCount = new List<string>();

        #region Control Events
        public frmReload()
        {
            InitializeComponent();
            LanguageManager.Instance.Load(GlobalOptions.Instance.Language, this);
            MoveControls();
        }

        private void frmReload_Load(object sender, EventArgs e)
        {
            List<ListItem> lstAmmo = new List<ListItem>();

            // Add each of the items to a new List since we need to also grab their plugin information.
            foreach (Gear objGear in _lstAmmo)
            {
                ListItem objAmmo = new ListItem();
                objAmmo.Value = objGear.InternalId;
                objAmmo.Name = objGear.DisplayNameShort;
                objAmmo.Name += " x" + objGear.Quantity.ToString();
                if (objGear.Parent != null)
                {
                    if (!string.IsNullOrEmpty(objGear.Parent.DisplayNameShort))
                    {
                        objAmmo.Name += " (" + objGear.Parent.DisplayNameShort;
                        if (!string.IsNullOrEmpty(objGear.Parent.Location))
                            objAmmo.Name += " @ " + objGear.Parent.Location;
                        objAmmo.Name += ")";
                    }
                }
                if (!string.IsNullOrEmpty(objGear.Location))
                    objAmmo.Name += " (" + objGear.Location + ")";
                // Retrieve the plugin information if it has any.
                if (objGear.Children.Count > 0)
                {
                    string strPlugins = string.Empty;
                    foreach (Gear objChild in objGear.Children)
                    {
                        strPlugins += objChild.DisplayNameShort + ", ";
                    }
                    // Remove the trailing comma.
                    strPlugins = strPlugins.Substring(0, strPlugins.Length - 2);
                    // Append the plugin information to the name.
                    objAmmo.Name += " [" + strPlugins + "]";
                }
                lstAmmo.Add(objAmmo);
            }

            // Populate the lists.
            cboAmmo.BeginUpdate();
            cboAmmo.ValueMember = "Value";
            cboAmmo.DisplayMember = "Name";
            cboAmmo.DataSource = lstAmmo;
            cboAmmo.EndUpdate();

            cboType.BeginUpdate();
            cboType.DataSource = _lstCount;
            cboType.EndUpdate();

            // If there's only 1 value in each list, the character doesn't have a choice, so just accept it.
            if (cboAmmo.Items.Count == 1 && cboType.Items.Count == 1)
                AcceptForm();
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void cmdOK_Click(object sender, EventArgs e)
        {
            AcceptForm();
        }

        private void cboAmmo_DropDown(object sender, EventArgs e)
        {
            // Resize the width of the DropDown so that the longest name fits.
            ComboBox objSender = (ComboBox)sender;
            int intWidth = objSender.DropDownWidth;
            Graphics objGraphics = objSender.CreateGraphics();
            Font objFont = objSender.Font;
            int intScrollWidth = (objSender.Items.Count > objSender.MaxDropDownItems) ? SystemInformation.VerticalScrollBarWidth : 0;
            int intNewWidth;
            foreach (ListItem objItem in ((ComboBox)sender).Items)
            {
                intNewWidth = (int)objGraphics.MeasureString(objItem.Name, objFont).Width + intScrollWidth;
                if (intWidth < intNewWidth)
                {
                    intWidth = intNewWidth;
                }
            }
            objSender.DropDownWidth = intWidth;
        }
        #endregion

        #region Properties
        /// <summary>
        /// List of Ammo Gear that the user can selected.
        /// </summary>
        public List<Gear> Ammo
        {
            set
            {
                _lstAmmo = value;
            }
        }
        
        /// <summary>
        /// List of ammunition that the user can select.
        /// </summary>
        public List<string> Count
        {
            set
            {
                _lstCount = value;
            }
        }

        /// <summary>
        /// Name of the ammunition that was selected.
        /// </summary>
        public string SelectedAmmo
        {
            get
            {
                return cboAmmo.SelectedValue.ToString();
            }
        }

        /// <summary>
        /// Number of rounds that were selected to be loaded.
        /// </summary>
        public int SelectedCount
        {
            get
            {
                return Convert.ToInt32(cboType.Text);
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Accept the selected item and close the form.
        /// </summary>
        private void AcceptForm()
        {
            DialogResult = DialogResult.OK;
        }

        private void MoveControls()
        {
            int intWidth = Math.Max(lblAmmoLabel.Width, lblTypeLabel.Width);
            cboAmmo.Left = lblAmmoLabel.Left + intWidth + 6;
            cboAmmo.Width = Width - cboAmmo.Left - 19;
            cboType.Left = lblTypeLabel.Left + intWidth + 6;
            cboType.Width = Width - cboType.Left - 19;
        }
        #endregion
    }
}