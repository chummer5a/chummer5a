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
        private readonly string _strDefaultOption = "Standard";
        private readonly XPathNavigator _xmlGameplayOptionsDataGameplayOptionsNode; 
        int intQualityLimits;
        decimal decNuyenBP;

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
            cboBuildMethod.ValueMember = "Value";
            cboBuildMethod.DisplayMember = "Name";
            cboBuildMethod.DataSource = lstBuildMethod;
            cboBuildMethod.SelectedValue = _objCharacter.Options.BuildMethod;
            cboBuildMethod.EndUpdate();

            nudKarma.Value = _objCharacter.Options.BuildPoints;
            nudMaxAvail.Value = _objCharacter.Options.Availability;

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
                            _strDefaultOption = strName;
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

            toolTip1.SetToolTip(chkIgnoreRules, LanguageManager.GetString("Tip_SelectKarma_IgnoreRules", GlobalOptions.Language));

            if (blnUseCurrentValues)
            {
                cboGamePlay.SelectedValue = _objCharacter.GameplayOption;
                if (cboGamePlay.SelectedIndex == -1)
                    cboGamePlay.SelectedValue = _strDefaultOption;

                cboBuildMethod.Enabled = false;
                cboBuildMethod.SelectedValue = _objCharacter.BuildMethod.ToString();

                nudKarma.Value = objCharacter.BuildKarma;
                nudMaxNuyen.Value = decNuyenBP = _objCharacter.NuyenMaximumBP;

                intQualityLimits = _objCharacter.GameplayOptionQualityLimit;
                chkIgnoreRules.Checked = _objCharacter.IgnoreRules;
                nudMaxAvail.Value = objCharacter.MaximumAvailability;
                nudSumtoTen.Value = objCharacter.SumtoTen;
            }
            else if (_xmlGameplayOptionsDataGameplayOptionsNode != null)
            {
                XPathNavigator objXmlSelectedGameplayOption = _xmlGameplayOptionsDataGameplayOptionsNode.SelectSingleNode("gameplayoption[name = \"" + cboGamePlay.SelectedValue.ToString() + "\"]");
                objXmlSelectedGameplayOption.TryGetInt32FieldQuickly("karma", ref intQualityLimits);
                objXmlSelectedGameplayOption.TryGetDecFieldQuickly("maxnuyen", ref decNuyenBP);
            }
        }

        private void cmdOK_Click(object sender, EventArgs e)
        {
            switch (cboBuildMethod.SelectedValue.ToString())
            {
                case "Karma":
                    _objCharacter.NuyenMaximumBP = decimal.ToInt32(nudMaxNuyen.Value);
                    _objCharacter.BuildMethod = CharacterBuildMethod.Karma;
                    break;
                case "Priority":
                    _objCharacter.NuyenMaximumBP = decNuyenBP;
                    _objCharacter.BuildMethod = CharacterBuildMethod.Priority;
                    break;
                case "SumtoTen":
                    _objCharacter.NuyenMaximumBP = decNuyenBP;
                    _objCharacter.BuildMethod = CharacterBuildMethod.SumtoTen;
                    _objCharacter.SumtoTen = decimal.ToInt32(nudSumtoTen.Value);
                    break;
                case "LifeModule":
                    _objCharacter.NuyenMaximumBP = decimal.ToInt32(nudMaxNuyen.Value);
                    _objCharacter.BuildMethod = CharacterBuildMethod.LifeModule;
                    break;
            }

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
            _objCharacter.GameplayOptionQualityLimit = intQualityLimits;
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
            nudKarma.Visible = false;
            nudMaxNuyen.Visible = false;
            nudSumtoTen.Visible = false;
            lblStartingKarma.Visible = false;
            lblSumToX.Visible = false;
            lblMaxNuyen.Visible = false;
            lblDescription.Text = LanguageManager.GetString("String_SelectBP_PrioritySummary", GlobalOptions.Language);

            string strSelectedBuildMethod = cboBuildMethod.SelectedValue?.ToString();
            switch (strSelectedBuildMethod)
            {
                case "Karma":
                    nudKarma.Value = _objCharacter.Options.BuildMethod == "Karma" ? _objCharacter.Options.BuildPoints : 800;
                    lblDescription.Text = string.Format(LanguageManager.GetString("String_SelectBP_KarmaSummary", GlobalOptions.Language), nudKarma.Value.ToString(GlobalOptions.InvariantCultureInfo));
                    nudKarma.Visible = true;
                    nudMaxNuyen.Visible = true;
                    lblStartingKarma.Visible = true;
                    lblMaxNuyen.Visible = true;
                    break;
                case "LifeModule":
                    nudKarma.Value = 750;
                    lblDescription.Text = string.Format(LanguageManager.GetString("String_SelectBP_LifeModuleSummary", GlobalOptions.Language), nudKarma.Value.ToString(GlobalOptions.InvariantCultureInfo));
                    nudKarma.Visible = true;
                    nudMaxNuyen.Visible = true;
                    lblStartingKarma.Visible = true;
                    lblMaxNuyen.Visible = true;
                    break;
                case "SumtoTen":
                    nudSumtoTen.Visible = true;
                    lblSumToX.Visible = true;
                    break;
            }
        }

        private void frmSelectBuildMethod_Load(object sender, EventArgs e)
        {
            Height = cmdOK.Bottom + 40;
            cboBuildMethod_SelectedIndexChanged(this, e);
        }

        private void cboGamePlay_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Load the Priority information.
            XPathNavigator objXmlGameplayOption = _xmlGameplayOptionsDataGameplayOptionsNode.SelectSingleNode("gameplayoption[name = \"" + cboGamePlay.SelectedValue?.ToString() + "\"]");
            if (objXmlGameplayOption != null)
            {
                int intTemp = 0;
                if (objXmlGameplayOption.TryGetInt32FieldQuickly("maxavailability", ref intTemp))
                    nudMaxAvail.Value = intTemp;
                objXmlGameplayOption.TryGetInt32FieldQuickly("karma", ref intQualityLimits);
                objXmlGameplayOption.TryGetDecFieldQuickly("maxnuyen", ref decNuyenBP);
            }
        }
        #endregion
    }
}
