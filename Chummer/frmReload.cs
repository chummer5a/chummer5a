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
            LanguageManager.TranslateWinForm(GlobalOptions.Instance.Language, this);
        }

        private void frmReload_Load(object sender, EventArgs e)
        {
            List<ListItem> lstAmmo = new List<ListItem>();
            string strSpace = LanguageManager.GetString("String_Space", GlobalOptions.Instance.Language);
            // Add each of the items to a new List since we need to also grab their plugin information.
            foreach (Gear objGear in _lstAmmo)
            {
                string strName = objGear.DisplayNameShort(GlobalOptions.Instance.Language) + " x" + objGear.Quantity.ToString(GlobalOptions.Instance.InvariantCultureInfo);
                if (objGear.Rating > 0)
                    strName += strSpace + '(' + LanguageManager.GetString("String_Rating", GlobalOptions.Instance.Language) + strSpace + objGear.Rating.ToString(GlobalOptions.Instance.CultureInfo) + ')';

                if (objGear.Parent is Gear objParent)
                {
                    if (!string.IsNullOrEmpty(objParent.DisplayNameShort(GlobalOptions.Instance.Language)))
                    {
                        strName += strSpace + '(' + objParent.DisplayNameShort(GlobalOptions.Instance.Language);
                        if (objParent.Location != null)
                            strName += strSpace + '@' + strSpace + objParent.Location.DisplayName(GlobalOptions.Instance.Language);
                        strName += ')';
                    }
                }
                else if (objGear.Location != null)
                    strName += strSpace + '(' + objGear.Location.DisplayName(GlobalOptions.Instance.Language) + ')';
                
                // Retrieve the plugin information if it has any.
                if (objGear.Children.Count > 0)
                {
                    string strPlugins = string.Empty;
                    foreach (Gear objChild in objGear.Children)
                    {
                        strPlugins += objChild.DisplayNameShort(GlobalOptions.Instance.Language) + ',' + strSpace;
                    }
                    // Remove the trailing comma.
                    strPlugins = strPlugins.Substring(0, strPlugins.Length - 1 - strSpace.Length);
                    // Append the plugin information to the name.
                    strName += strSpace + '[' + strPlugins + ']';
                }
                lstAmmo.Add(new ListItem(objGear.InternalId, strName));
            }

            // Populate the lists.
            cboAmmo.BeginUpdate();
            cboAmmo.DataSource = null;
            cboAmmo.ValueMember = nameof(ListItem.Value);
            cboAmmo.DisplayMember = nameof(ListItem.Name);
            cboAmmo.DataSource = lstAmmo;
            cboAmmo.EndUpdate();

            cboType.BeginUpdate();
            cboType.DataSource = null;
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
        #endregion

        #region Properties
        /// <summary>
        /// List of Ammo Gear that the user can selected.
        /// </summary>
        public List<Gear> Ammo
        {
            set => _lstAmmo = value;
        }

        /// <summary>
        /// List of ammunition that the user can select.
        /// </summary>
        public List<string> Count
        {
            set => _lstCount = value;
        }

        /// <summary>
        /// Name of the ammunition that was selected.
        /// </summary>
        public string SelectedAmmo => cboAmmo.SelectedValue?.ToString() ?? string.Empty;

        /// <summary>
        /// Number of rounds that were selected to be loaded.
        /// </summary>
        public int SelectedCount => Convert.ToInt32(cboType.Text);

        #endregion

        #region Methods
        /// <summary>
        /// Accept the selected item and close the form.
        /// </summary>
        private void AcceptForm()
        {
            DialogResult = DialogResult.OK;
        }
        #endregion
    }
}
