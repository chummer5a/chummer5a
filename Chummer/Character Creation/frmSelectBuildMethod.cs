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
 using System.Xml.XPath;

namespace Chummer
{
    public sealed partial class frmSelectBuildMethod : Form
    {
        private readonly Character _objCharacter;
        private readonly string _strDefaultOption = GlobalOptions.DefaultGameplayOption;
        private readonly XPathNavigator _xmlGameplayOptionsDataGameplayOptionsNode;
        private readonly int _intDefaultMaxAvail = 12;
        private readonly int _intDefaultSumToTen = 10;
        private readonly int _intDefaultPointBuyKarma = 800;
        private readonly int _intDefaultLifeModulesKarma = 750;
        private int _intQualityLimits;
        private decimal _decNuyenBP;

        #region Control Events
        public frmSelectBuildMethod(Character objCharacter, bool blnUseCurrentValues = false)
        {
            _objCharacter = objCharacter;
            InitializeComponent();
            LanguageManager.TranslateWinForm(GlobalOptions.Language, this);

            _xmlGameplayOptionsDataGameplayOptionsNode = XmlManager.Load("gameplayoptions.xml").GetFastNavigator().SelectSingleNode("/chummer/gameplayoptions");

            // Populate the Build Method list.
            List<ListItem> lstBuildMethod = new List<ListItem>
            {
                new ListItem("Karma", LanguageManager.GetString("String_Karma", GlobalOptions.Language)),
                new ListItem("Priority", LanguageManager.GetString("String_Priority", GlobalOptions.Language)),
                new ListItem("SumtoTen", LanguageManager.GetString("String_SumtoTen", GlobalOptions.Language)),
            };

            if (GlobalOptions.LifeModuleEnabled)
            {
                lstBuildMethod.Add(new ListItem("LifeModule", LanguageManager.GetString("String_LifeModule", GlobalOptions.Language)));
            }

            cboBuildMethod.BeginUpdate();
            cboBuildMethod.ValueMember = nameof(ListItem.Value);
            cboBuildMethod.DisplayMember = nameof(ListItem.Name);
            cboBuildMethod.DataSource = lstBuildMethod;
            cboBuildMethod.SelectedValue = GlobalOptions.DefaultBuildMethod;
            cboBuildMethod.EndUpdate();
            
            // Populate the Gameplay Options list.
            List<ListItem> lstGameplayOptions = new List<ListItem>();
            if (_xmlGameplayOptionsDataGameplayOptionsNode != null)
            {
                foreach (XPathNavigator objXmlGameplayOption in _xmlGameplayOptionsDataGameplayOptionsNode.Select("gameplayoption"))
                {
                    string strName = objXmlGameplayOption.SelectSingleNode("name")?.Value;
                    if (!string.IsNullOrEmpty(strName))
                    {
                        if (objXmlGameplayOption.SelectSingleNode("default")?.Value == bool.TrueString)
                        {
                            objXmlGameplayOption.TryGetInt32FieldQuickly("maxavailability", ref _intDefaultMaxAvail);
                            objXmlGameplayOption.TryGetInt32FieldQuickly("sumtoten", ref _intDefaultSumToTen);
                            objXmlGameplayOption.TryGetInt32FieldQuickly("pointbuykarma", ref _intDefaultPointBuyKarma);
                            objXmlGameplayOption.TryGetInt32FieldQuickly("lifemoduleskarma", ref _intDefaultLifeModulesKarma);
                        }
                        lstGameplayOptions.Add(new ListItem(strName, objXmlGameplayOption.SelectSingleNode("translate")?.Value ?? strName));
                    }
                }
            }

            cboGamePlay.BeginUpdate();
            cboGamePlay.ValueMember = "Value";
            cboGamePlay.DisplayMember = "Name";
            cboGamePlay.DataSource = lstGameplayOptions;
            cboGamePlay.SelectedValue = _strDefaultOption;
            cboGamePlay.EndUpdate();

            chkIgnoreRules.SetToolTip(LanguageManager.GetString("Tip_SelectKarma_IgnoreRules", GlobalOptions.Language));

            if (blnUseCurrentValues)
            {
                cboGamePlay.SelectedValue = _objCharacter.GameplayOption;
                if (cboGamePlay.SelectedIndex == -1)
                    cboGamePlay.SelectedValue = _strDefaultOption;

                cboBuildMethod.Enabled = false;
                cboBuildMethod.SelectedValue = _objCharacter.BuildMethod.ToString();

                nudKarma.Value = objCharacter.BuildKarma;
                nudMaxNuyen.Value = _decNuyenBP = _objCharacter.NuyenMaximumBP;

                _intQualityLimits = _objCharacter.GameplayOptionQualityLimit;
                chkIgnoreRules.Checked = _objCharacter.IgnoreRules;
                nudMaxAvail.Value = objCharacter.MaximumAvailability;
                nudSumtoTen.Value = objCharacter.SumtoTen;
            }
            else if (_xmlGameplayOptionsDataGameplayOptionsNode != null)
            {
                XPathNavigator objXmlSelectedGameplayOption = _xmlGameplayOptionsDataGameplayOptionsNode.SelectSingleNode("gameplayoption[name = \"" + cboGamePlay.SelectedValue.ToString() + "\"]");
                objXmlSelectedGameplayOption.TryGetInt32FieldQuickly("karma", ref _intQualityLimits);
                objXmlSelectedGameplayOption.TryGetDecFieldQuickly("maxnuyen", ref _decNuyenBP);
                nudMaxNuyen.Value = _decNuyenBP;

                nudKarma.Value = _intQualityLimits;
                int intTemp = _intDefaultMaxAvail;
                objXmlSelectedGameplayOption.TryGetInt32FieldQuickly("maxavailability", ref intTemp);
                nudMaxAvail.Value = intTemp;
                intTemp = _intDefaultSumToTen;
                objXmlSelectedGameplayOption.TryGetInt32FieldQuickly("sumtoten", ref intTemp);
                nudSumtoTen.Value = intTemp;
            }
        }

        private void cmdOK_Click(object sender, EventArgs e)
        {
            switch (cboBuildMethod.SelectedValue.ToString())
            {
                case "Karma":
                    _objCharacter.BuildMethod = CharacterBuildMethod.Karma;
                    break;
                case "Priority":
                    _objCharacter.BuildMethod = CharacterBuildMethod.Priority;
                    break;
                case "SumtoTen":
                    _objCharacter.BuildMethod = CharacterBuildMethod.SumtoTen;
                    break;
                case "LifeModule":
                    _objCharacter.BuildMethod = CharacterBuildMethod.LifeModule;
                    break;
            }
            _objCharacter.NuyenMaximumBP = decimal.ToInt32(nudMaxNuyen.Value);
            _objCharacter.SumtoTen = decimal.ToInt32(nudSumtoTen.Value);

            XPathNavigator xmlGameplayOption = _xmlGameplayOptionsDataGameplayOptionsNode.SelectSingleNode("gameplayoption[name = \"" + cboGamePlay.SelectedValue.ToString() + "\"]");
            if (xmlGameplayOption != null)
            {
                _objCharacter.BannedWareGrades.Clear();
                foreach (XPathNavigator xmlNode in xmlGameplayOption.Select("bannedwaregrades/grade"))
                            _objCharacter.BannedWareGrades.Add(xmlNode.Value);

                int intTemp = 0;
                if (!_objCharacter.Options.FreeContactsMultiplierEnabled && xmlGameplayOption.TryGetInt32FieldQuickly("contactmultiplier", ref intTemp))
                    _objCharacter.ContactMultiplier = intTemp;
                if (xmlGameplayOption.TryGetInt32FieldQuickly("karma", ref intTemp))
                    _objCharacter.GameplayOptionQualityLimit = _objCharacter.MaxKarma = intTemp;
                decimal decTemp = 0;
                if (xmlGameplayOption.TryGetDecFieldQuickly("maxnuyen", ref decTemp))
                    _objCharacter.MaxNuyen = decTemp;
            }
            _objCharacter.BuildKarma = decimal.ToInt32(nudKarma.Value);
            _objCharacter.GameplayOption = cboGamePlay.SelectedValue.ToString();
            _objCharacter.GameplayOptionQualityLimit = _intQualityLimits;
            _objCharacter.IgnoreRules = chkIgnoreRules.Checked;
            _objCharacter.MaximumAvailability = decimal.ToInt32(nudMaxAvail.Value);
            DialogResult = DialogResult.OK;
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void cboBuildMethod_SelectedIndexChanged(object sender, EventArgs e)
        {
            nudSumtoTen.Visible = false;
            lblSumToX.Visible = false;
            XPathNavigator xmlSelectedGameplayOption = _xmlGameplayOptionsDataGameplayOptionsNode.SelectSingleNode("gameplayoption[name = \"" + cboGamePlay.SelectedValue?.ToString() + "\"]");
            string strSelectedBuildMethod = cboBuildMethod.SelectedValue?.ToString();
            switch (strSelectedBuildMethod)
            {
                case "Karma":
                {
                    int intKarmaValue = _intDefaultPointBuyKarma;
                    xmlSelectedGameplayOption?.TryGetInt32FieldQuickly("pointbuykarma", ref intKarmaValue);
                    nudKarma.Value = intKarmaValue;
                    nudKarma.Enabled = true;
                    nudMaxNuyen.Value = 225 + _decNuyenBP;
                    nudMaxNuyen.Enabled = true;
                    lblDescription.Text = string.Format(LanguageManager.GetString("String_SelectBP_KarmaSummary", GlobalOptions.Language), nudKarma.Value.ToString(GlobalOptions.InvariantCultureInfo));
                    break;
                }
                case "LifeModule":
                {
                    int intKarmaValue = _intDefaultLifeModulesKarma;
                    xmlSelectedGameplayOption?.TryGetInt32FieldQuickly("lifemoduleskarma", ref intKarmaValue);
                    nudKarma.Value = intKarmaValue;
                    nudKarma.Enabled = true;
                    nudMaxNuyen.Value = 225 + _decNuyenBP;
                    nudMaxNuyen.Enabled = true;
                    lblDescription.Text = string.Format(LanguageManager.GetString("String_SelectBP_LifeModuleSummary", GlobalOptions.Language), nudKarma.Value.ToString(GlobalOptions.InvariantCultureInfo));
                    break;
                }
                case "SumtoTen":
                    nudSumtoTen.Visible = true;
                    lblSumToX.Visible = true;
                    goto default;
                default:
                    nudKarma.Value = _intQualityLimits;
                    nudKarma.Enabled = false;
                    nudMaxNuyen.Value = _decNuyenBP;
                    nudMaxNuyen.Enabled = false;
                    lblDescription.Text = LanguageManager.GetString("String_SelectBP_PrioritySummary", GlobalOptions.Language);
                    break;
            }

            if (!nudSumtoTen.Visible)
            {
                int intSumToTenValue = _intDefaultSumToTen;
                xmlSelectedGameplayOption?.TryGetInt32FieldQuickly("sumtoten", ref intSumToTenValue);
                nudSumtoTen.Value = intSumToTenValue;
            }
        }

        private void frmSelectBuildMethod_Load(object sender, EventArgs e)
        {
            cboBuildMethod_SelectedIndexChanged(this, e);
        }

        private void cboGamePlay_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Load the Priority information.
            XPathNavigator objXmlGameplayOption = _xmlGameplayOptionsDataGameplayOptionsNode.SelectSingleNode("gameplayoption[name = \"" + cboGamePlay.SelectedValue?.ToString() + "\"]");
            if (objXmlGameplayOption != null)
            {
                int intTemp = _intDefaultMaxAvail;
                objXmlGameplayOption.TryGetInt32FieldQuickly("maxavailability", ref intTemp);
                nudMaxAvail.Value = intTemp;

                intTemp = _intDefaultSumToTen;
                objXmlGameplayOption.TryGetInt32FieldQuickly("sumtoten", ref intTemp);
                nudSumtoTen.Value = intTemp;

                objXmlGameplayOption.TryGetInt32FieldQuickly("karma", ref _intQualityLimits);
                objXmlGameplayOption.TryGetDecFieldQuickly("maxnuyen", ref _decNuyenBP);
                string strSelectedBuildMethod = cboBuildMethod.SelectedValue?.ToString();
                if (strSelectedBuildMethod == "Karma")
                {
                    intTemp = _intDefaultPointBuyKarma;
                    objXmlGameplayOption.TryGetInt32FieldQuickly("pointbuykarma", ref intTemp);
                    nudKarma.Value = intTemp;
                }
                else if (strSelectedBuildMethod == "LifeModule")
                {
                    intTemp = _intDefaultLifeModulesKarma;
                    objXmlGameplayOption.TryGetInt32FieldQuickly("lifemoduleskarma", ref intTemp);
                    nudKarma.Value = intTemp;
                }
                else
                {
                    nudKarma.Value = _intQualityLimits;
                    nudMaxNuyen.Value = _decNuyenBP;
                }
            }
        }
        #endregion
    }
}
