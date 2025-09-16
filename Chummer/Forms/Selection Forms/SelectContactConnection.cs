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
        private Color _objColor;
        private bool _blnFree;
        private bool _blnSkipUpdate;

        #region Control Events

        public SelectContactConnection()
        {
            InitializeComponent();
            this.UpdateLightDarkMode();
            this.TranslateWinForm();
            this.UpdateParentForToolTipControls();
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
            Close();
        }

        private void cmdOK_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void chkFreeContact_CheckedChanged(object sender, EventArgs e)
        {
            ValuesChanged();
        }

        private async void SelectContactConnection_Load(object sender, EventArgs e)
        {
            string strNone = await LanguageManager.GetStringAsync("String_None").ConfigureAwait(false);
            string strMembers = await LanguageManager.GetStringAsync("String_SelectContactConnection_Members").ConfigureAwait(false);
            // Populate the fields with their data.
            // Membership.
            await cboMembership.DoThreadSafeAsync(x =>
            {
                x.Items.Add("+0: " + strNone);
                x.Items.Add("+1: " + string.Format(GlobalSettings.CultureInfo, strMembers, "2-19"));
                x.Items.Add("+2: " + string.Format(GlobalSettings.CultureInfo, strMembers, "20-99"));
                x.Items.Add("+4: " + string.Format(GlobalSettings.CultureInfo, strMembers, "100-1000"));
                x.Items.Add("+6: " + string.Format(GlobalSettings.CultureInfo, strMembers, "1000+"));
            }).ConfigureAwait(false);

            string strAoI1 = await LanguageManager.GetStringAsync("String_SelectContactConnection_AreaDistrict").ConfigureAwait(false);
            string strAoI2 = await LanguageManager.GetStringAsync("String_SelectContactConnection_AreaSprawlwide").ConfigureAwait(false);
            string strAoI3 = await LanguageManager.GetStringAsync("String_SelectContactConnection_AreaNational").ConfigureAwait(false);
            string strAoI4 = await LanguageManager.GetStringAsync("String_SelectContactConnection_AreaGlobal").ConfigureAwait(false);
            // Area of Influence.
            await cboAreaOfInfluence.DoThreadSafeAsync(x =>
            {
                x.Items.Add("+0: " + strNone);
                x.Items.Add("+1: " + strAoI1);
                x.Items.Add("+2: " + strAoI2);
                x.Items.Add("+4: " + strAoI3);
                x.Items.Add("+6: " + strAoI4);
            }).ConfigureAwait(false);

            string strMgR1 = await LanguageManager.GetStringAsync("String_SelectContactConnection_MagicalMinority").ConfigureAwait(false);
            string strMgR2 = await LanguageManager.GetStringAsync("String_SelectContactConnection_MagicalMost").ConfigureAwait(false);
            string strMgR3 = await LanguageManager.GetStringAsync("String_SelectContactConnection_MagicalVast").ConfigureAwait(false);
            // Magical Resources.
            await cboMagicalResources.DoThreadSafeAsync(x =>
            {
                x.Items.Add("+0: " + strNone);
                x.Items.Add("+1: " + strMgR1);
                x.Items.Add("+4: " + strMgR2);
                x.Items.Add("+6: " + strMgR3);
            }).ConfigureAwait(false);

            string strMxR1 = await LanguageManager.GetStringAsync("String_SelectContactConnection_MatrixActive").ConfigureAwait(false);
            string strMxR2 = await LanguageManager.GetStringAsync("String_SelectContactConnection_MatrixBroad").ConfigureAwait(false);
            string strMxR3 = await LanguageManager.GetStringAsync("String_SelectContactConnection_MatrixPervasive").ConfigureAwait(false);
            // Matrix Resources.
            await cboMatrixResources.DoThreadSafeAsync(x =>
            {
                x.Items.Add("+0: " + strNone);
                x.Items.Add("+1: " + strMxR1);
                x.Items.Add("+2: " + strMxR2);
                x.Items.Add("+4: " + strMxR3);
            }).ConfigureAwait(false);

            // Select the appropriate field values.
            _blnSkipUpdate = true;
            try
            {
                await cboMembership
                      .DoThreadSafeAsync(x => x.SelectedIndex
                                             = x.FindString(
                                                 '+' + _intMembership.ToString(GlobalSettings.InvariantCultureInfo)))
                      .ConfigureAwait(false);
                await cboAreaOfInfluence
                      .DoThreadSafeAsync(x => x.SelectedIndex
                                             = x.FindString(
                                                 '+' + _intAreaOfInfluence.ToString(
                                                     GlobalSettings.InvariantCultureInfo))).ConfigureAwait(false);
                await cboMagicalResources
                      .DoThreadSafeAsync(x => x.SelectedIndex
                                             = x.FindString(
                                                 '+' + _intMagicalResources.ToString(
                                                     GlobalSettings.InvariantCultureInfo))).ConfigureAwait(false);
                await cboMatrixResources
                      .DoThreadSafeAsync(x => x.SelectedIndex
                                             = x.FindString(
                                                 '+' + _intMatrixResources.ToString(
                                                     GlobalSettings.InvariantCultureInfo))).ConfigureAwait(false);
                await txtGroupName.DoThreadSafeAsync(x => x.Text = _strGroupName).ConfigureAwait(false);
                await cmdChangeColor.DoThreadSafeAsync(x => x.BackColor = _objColor).ConfigureAwait(false);
                await chkFreeContact.DoThreadSafeAsync(x => x.Checked = _blnFree).ConfigureAwait(false);
            }
            finally
            {
                _blnSkipUpdate = false;
            }

            await lblTotalConnectionModifier.DoThreadSafeAsync(x => x.Text = (_intMembership + _intAreaOfInfluence + _intMagicalResources + _intMatrixResources).ToString(GlobalSettings.CultureInfo)).ConfigureAwait(false);
        }

        private async void cmdChangeColor_Click(object sender, EventArgs e)
        {
            Color objPreviewColor = ColorManager.GenerateCurrentModeColor(_objColor);
            Color objSelectedColor = _objColor;
            DialogResult eResult = await this.DoThreadSafeFuncAsync(x =>
            {
                using (ColorDialog dlgColor = new ColorDialog())
                {
                    dlgColor.Color = objPreviewColor;
                    DialogResult eReturn = dlgColor.ShowDialog(x);
                    objSelectedColor = ColorManager.GenerateModeIndependentColor(dlgColor.Color);
                    return eReturn;
                }
            }).ConfigureAwait(false);
            if (eResult != DialogResult.OK)
                return;
            if (objSelectedColor.Name == "White" || objSelectedColor.Name == "Black")
            {
                Color objColor = ColorManager.Control;
                await cmdChangeColor.DoThreadSafeAsync(x => x.BackColor = objColor).ConfigureAwait(false);
                _objColor = ColorManager.ControlLight;
            }
            else
            {
                objPreviewColor = ColorManager.GenerateCurrentModeColor(objSelectedColor);
                await cmdChangeColor.DoThreadSafeAsync(x => x.BackColor = objPreviewColor).ConfigureAwait(false);
                _objColor = objSelectedColor;
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
        /// Contact Color.
        /// </summary>
        public Color Color
        {
            get => _objColor;
            set => _objColor = value;
        }

        /// <summary>
        /// Whether this is a free contact.
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

            int.TryParse(cboMembership.Text.Substring(0, 2), System.Globalization.NumberStyles.Integer, GlobalSettings.InvariantCultureInfo, out _intMembership);
            int.TryParse(cboAreaOfInfluence.Text.Substring(0, 2), System.Globalization.NumberStyles.Integer, GlobalSettings.InvariantCultureInfo, out _intAreaOfInfluence);
            int.TryParse(cboMagicalResources.Text.Substring(0, 2), System.Globalization.NumberStyles.Integer, GlobalSettings.InvariantCultureInfo, out _intMagicalResources);
            int.TryParse(cboMatrixResources.Text.Substring(0, 2), System.Globalization.NumberStyles.Integer, GlobalSettings.InvariantCultureInfo, out _intMatrixResources);
            _strGroupName = txtGroupName.Text;
            _blnFree = chkFreeContact.Checked;

            lblTotalConnectionModifier.Text = (_intMembership + _intAreaOfInfluence + _intMagicalResources + _intMatrixResources).ToString(GlobalSettings.CultureInfo);
        }

        #endregion Methods
    }
}
