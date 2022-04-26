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
using System.Drawing;
using System.Windows.Forms;

namespace Chummer
{
    public partial class SelectContactConnection : Form
    {
        private int _intMembership;
        private int _intAreaOfInfluence;
        private int _intMagicalResources;
        private int _intMatrixResources;
        private string _strGroupName = string.Empty;
        private Color _objColour;
        private bool _blnFree;
        private bool _blnSkipUpdate;

        #region Control Events

        public SelectContactConnection()
        {
            InitializeComponent();
            this.UpdateLightDarkMode();
            this.TranslateWinForm();
        }

        private void cboMembership_SelectedIndexChanged(object sender, EventArgs e)
        {
            ValuesChanged();
        }

        private void cboAreaOfInfluence_SelectedIndexChanged(object sender, EventArgs e)
        {
            ValuesChanged();
        }

        private void cboMagicalResources_SelectedIndexChanged(object sender, EventArgs e)
        {
            ValuesChanged();
        }

        private void cboMatrixResources_SelectedIndexChanged(object sender, EventArgs e)
        {
            ValuesChanged();
        }

        private void txtGroupName_TextChanged(object sender, EventArgs e)
        {
            ValuesChanged();
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void cmdOK_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }

        private void chkFreeContact_CheckedChanged(object sender, EventArgs e)
        {
            ValuesChanged();
        }

        private async void SelectContactConnection_Load(object sender, EventArgs e)
        {
            string strNone = await LanguageManager.GetStringAsync("String_None");
            string strMembers = await LanguageManager.GetStringAsync("String_SelectContactConnection_Members");
            // Populate the fields with their data.
            // Membership.
            await cboMembership.DoThreadSafeAsync(x =>
            {
                x.Items.Add("+0: " + strNone);
                x.Items.Add("+1: " + string.Format(GlobalSettings.CultureInfo, strMembers, "2-19"));
                x.Items.Add("+2: " + string.Format(GlobalSettings.CultureInfo, strMembers, "20-99"));
                x.Items.Add("+4: " + string.Format(GlobalSettings.CultureInfo, strMembers, "100-1000"));
                x.Items.Add("+6: " + string.Format(GlobalSettings.CultureInfo, strMembers, "1000+"));
            });

            string strAoI1 = await LanguageManager.GetStringAsync("String_SelectContactConnection_AreaDistrict");
            string strAoI2 = await LanguageManager.GetStringAsync("String_SelectContactConnection_AreaSprawlwide");
            string strAoI3 = await LanguageManager.GetStringAsync("String_SelectContactConnection_AreaNational");
            string strAoI4 = await LanguageManager.GetStringAsync("String_SelectContactConnection_AreaGlobal");
            // Area of Influence.
            await cboAreaOfInfluence.DoThreadSafeAsync(x =>
            {
                x.Items.Add("+0: " + strNone);
                x.Items.Add("+1: " + strAoI1);
                x.Items.Add("+2: " + strAoI2);
                x.Items.Add("+4: " + strAoI3);
                x.Items.Add("+6: " + strAoI4);
            });

            string strMgR1 = await LanguageManager.GetStringAsync("String_SelectContactConnection_MagicalMinority");
            string strMgR2 = await LanguageManager.GetStringAsync("String_SelectContactConnection_MagicalMost");
            string strMgR3 = await LanguageManager.GetStringAsync("String_SelectContactConnection_MagicalVast");
            // Magical Resources.
            await cboMagicalResources.DoThreadSafeAsync(x =>
            {
                x.Items.Add("+0: " + strNone);
                x.Items.Add("+1: " + strMgR1);
                x.Items.Add("+4: " + strMgR2);
                x.Items.Add("+6: " + strMgR3);
            });

            string strMxR1 = await LanguageManager.GetStringAsync("String_SelectContactConnection_MatrixActive");
            string strMxR2 = await LanguageManager.GetStringAsync("String_SelectContactConnection_MatrixBroad");
            string strMxR3 = await LanguageManager.GetStringAsync("String_SelectContactConnection_MatrixPervasive");
            // Matrix Resources.
            await cboMatrixResources.DoThreadSafeAsync(x =>
            {
                x.Items.Add("+0: " + strNone);
                x.Items.Add("+1: " + strMxR1);
                x.Items.Add("+2: " + strMxR2);
                x.Items.Add("+4: " + strMxR3);
            });

            // Select the appropriate field values.
            _blnSkipUpdate = true;
            await cboMembership.DoThreadSafeAsync(x => x.SelectedIndex = x.FindString('+' + _intMembership.ToString(GlobalSettings.InvariantCultureInfo)));
            await cboAreaOfInfluence.DoThreadSafeAsync(x => x.SelectedIndex = x.FindString('+' + _intAreaOfInfluence.ToString(GlobalSettings.InvariantCultureInfo)));
            await cboMagicalResources.DoThreadSafeAsync(x => x.SelectedIndex = x.FindString('+' + _intMagicalResources.ToString(GlobalSettings.InvariantCultureInfo)));
            await cboMatrixResources.DoThreadSafeAsync(x => x.SelectedIndex = x.FindString('+' + _intMatrixResources.ToString(GlobalSettings.InvariantCultureInfo)));
            await txtGroupName.DoThreadSafeAsync(x => x.Text = _strGroupName);
            await cmdChangeColour.DoThreadSafeAsync(x => x.BackColor = _objColour);
            await chkFreeContact.DoThreadSafeAsync(x => x.Checked = _blnFree);
            _blnSkipUpdate = false;

            await lblTotalConnectionModifier.DoThreadSafeAsync(x => x.Text = (_intMembership + _intAreaOfInfluence + _intMagicalResources + _intMatrixResources).ToString(GlobalSettings.CultureInfo));
        }

        private async void cmdChangeColour_Click(object sender, EventArgs e)
        {
            using (ColorDialog dlgColor = await this.DoThreadSafeFuncAsync(() => new ColorDialog()))
            {
                if (await this.DoThreadSafeFuncAsync(x => dlgColor.ShowDialog(x)) != DialogResult.OK)
                    return;
                if (dlgColor.Color.Name == "White" || dlgColor.Color.Name == "Black")
                {
                    Color objColor = await ColorManager.ControlAsync;
                    await cmdChangeColour.DoThreadSafeAsync(x => x.BackColor = objColor);
                    _objColour = objColor;
                }
                else
                {
                    await cmdChangeColour.DoThreadSafeAsync(x => x.BackColor = dlgColor.Color);
                    _objColour = dlgColor.Color;
                }
            }
        }

        #endregion Control Events

        #region Properties

        /// <summary>
        /// Membership.
        /// </summary>
        public int Membership
        {
            get => _intMembership;
            set => _intMembership = value;
        }

        /// <summary>
        /// Area of Influence.
        /// </summary>
        public int AreaOfInfluence
        {
            get => _intAreaOfInfluence;
            set => _intAreaOfInfluence = value;
        }

        /// <summary>
        /// Magical Resources.
        /// </summary>
        public int MagicalResources
        {
            get => _intMagicalResources;
            set => _intMagicalResources = value;
        }

        /// <summary>
        /// Matrix Resources.
        /// </summary>
        public int MatrixResources
        {
            get => _intMatrixResources;
            set => _intMatrixResources = value;
        }

        /// <summary>
        /// Group Name.
        /// </summary>
        public string GroupName
        {
            get => _strGroupName;
            set => _strGroupName = value;
        }

        /// <summary>
        /// Contact Colour.
        /// </summary>
        public Color Colour
        {
            get => _objColour;
            set => _objColour = value;
        }

        /// <summary>
        /// Whether or not this is a free contact.
        /// </summary>
        public bool Free
        {
            get => _blnFree;
            set => _blnFree = value;
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Refresh the Connection Group information.
        /// </summary>
        private void ValuesChanged()
        {
            if (_blnSkipUpdate)
                return;

            _intMembership = Convert.ToInt32(cboMembership.Text.Substring(0, 2), GlobalSettings.InvariantCultureInfo);
            _intAreaOfInfluence = Convert.ToInt32(cboAreaOfInfluence.Text.Substring(0, 2), GlobalSettings.InvariantCultureInfo);
            _intMagicalResources = Convert.ToInt32(cboMagicalResources.Text.Substring(0, 2), GlobalSettings.InvariantCultureInfo);
            _intMatrixResources = Convert.ToInt32(cboMatrixResources.Text.Substring(0, 2), GlobalSettings.InvariantCultureInfo);
            _strGroupName = txtGroupName.Text;
            _blnFree = chkFreeContact.Checked;

            lblTotalConnectionModifier.Text = (_intMembership + _intAreaOfInfluence + _intMagicalResources + _intMatrixResources).ToString(GlobalSettings.CultureInfo);
        }

        #endregion Methods
    }
}
