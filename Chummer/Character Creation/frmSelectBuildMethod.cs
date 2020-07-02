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
 using System.Linq;
 using System.Windows.Forms;
 using System.Xml.XPath;

namespace Chummer
{
    public sealed partial class frmSelectBuildMethod : Form
    {
        private readonly Character _objCharacter;
        private readonly string _strDefaultOption = GlobalOptions.DefaultGameplayOption;
        private readonly XPathNavigator _xmlGameplayOptionsDataGameplayOptionsNode;

        #region Control Events
        public frmSelectBuildMethod(Character objCharacter, bool blnUseCurrentValues = false)
        {
            _objCharacter = objCharacter ?? throw new ArgumentNullException(nameof(objCharacter));
            InitializeComponent();
            this.TranslateWinForm();

            _xmlGameplayOptionsDataGameplayOptionsNode = _objCharacter.LoadDataXPath("gameplayoptions.xml").CreateNavigator().SelectSingleNode("/chummer/gameplayoptions");

            string strSpace = LanguageManager.GetString("String_Space");
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

                        if (objXmlGameplayOption.SelectSingleNode("priorityarrays") != null)
                        {
                            XPathNodeIterator iterator = objXmlGameplayOption.Select("priorityarrays/priorityarray");
                            lstGameplayOptions.AddRange(iterator.Cast<XPathNavigator>()
                                .Select(node => new ListItem(strName + '|' + node.Value,
                                    (objXmlGameplayOption.SelectSingleNode("translate")?.Value ?? strName) + strSpace + '(' + node.Value + ')')));
                        }
                        else
                        {
                            lstGameplayOptions.Add(new ListItem(strName,
                                objXmlGameplayOption.SelectSingleNode("translate")?.Value ?? strName));
                        }
                    }
                }
            }

            cboCharacterOption.BeginUpdate();
            cboCharacterOption.ValueMember = nameof(ListItem.Value);
            cboCharacterOption.DisplayMember = nameof(ListItem.Name);
            cboCharacterOption.DataSource = lstGameplayOptions;
            cboCharacterOption.SelectedValue = _strDefaultOption;
            cboCharacterOption.EndUpdate();

            chkIgnoreRules.SetToolTip(LanguageManager.GetString("Tip_SelectKarma_IgnoreRules"));

            if (blnUseCurrentValues)
            {
                string strGameplayOption = _objCharacter.GameplayOption;
                if (!string.IsNullOrEmpty(_objCharacter.PriorityArray))
                    strGameplayOption += '|' + _objCharacter.PriorityArray;
                cboCharacterOption.SelectedValue = strGameplayOption;
                if (cboCharacterOption.SelectedIndex == -1)
                    cboCharacterOption.SelectedValue = _strDefaultOption;

                cboBuildMethod.Enabled = false;
                cboBuildMethod.SelectedValue = _objCharacter.BuildMethod.ToString();

                nudKarma.Value = objCharacter.BuildKarma;
                nudMaxNuyen.Value = _decNuyenBP = _objCharacter.NuyenMaximumBP;

                _intQualityLimits = _objCharacter.GameplayOptionQualityLimit;
                chkIgnoreRules.Checked = _objCharacter.IgnoreRules;
                nudMaxAvail.Value = Math.Min(objCharacter.MaximumAvailability, nudMaxAvail.Maximum);
                nudSumtoTen.Value = objCharacter.SumtoTen;
            }
            else if (_xmlGameplayOptionsDataGameplayOptionsNode != null)
            {
                string strSelectedGameplayOption = cboCharacterOption.SelectedValue?.ToString();
                if (strSelectedGameplayOption != null && strSelectedGameplayOption.IndexOf('|') != -1)
                {
                    strSelectedGameplayOption = strSelectedGameplayOption.Split('|')[0];
                }
                XPathNavigator objXmlSelectedGameplayOption = _xmlGameplayOptionsDataGameplayOptionsNode.SelectSingleNode("gameplayoption[name = \"" + (strSelectedGameplayOption ?? string.Empty) + "\"]");
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
            string strSelectedGameplayOption = cboCharacterOption.SelectedValue?.ToString() ?? string.Empty;
            if (string.IsNullOrEmpty(strSelectedGameplayOption))
            {
                return;
            }
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

            string strPriorityArray = string.Empty;
            if (strSelectedGameplayOption.IndexOf('|') != -1)
            {
                strPriorityArray = strSelectedGameplayOption.Split('|')[1];
                strSelectedGameplayOption = strSelectedGameplayOption.Split('|')[0];
            }

            XPathNavigator xmlGameplayOption =
                _xmlGameplayOptionsDataGameplayOptionsNode.SelectSingleNode("gameplayoption[name = \"" + strSelectedGameplayOption + "\"]");

            if (xmlGameplayOption != null)
            {
                _objCharacter.BannedWareGrades.Clear();
                foreach (XPathNavigator xmlNode in xmlGameplayOption.Select("bannedwaregrades/grade"))
                            _objCharacter.BannedWareGrades.Add(xmlNode.Value);

                int intTemp = 0;
                if (xmlGameplayOption.TryGetInt32FieldQuickly("karma", ref intTemp))
                    _objCharacter.GameplayOptionQualityLimit = _objCharacter.MaxKarma = intTemp;
                decimal decTemp = 0;
                if (xmlGameplayOption.TryGetDecFieldQuickly("maxnuyen", ref decTemp))
                    _objCharacter.MaxNuyen = decTemp;
            }

            _objCharacter.PriorityArray = strPriorityArray;
            _objCharacter.BuildKarma = decimal.ToInt32(nudKarma.Value);
            _objCharacter.GameplayOption = strSelectedGameplayOption;
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

        private void frmSelectBuildMethod_Load(object sender, EventArgs e)
        {
            cboGamePlay_SelectedIndexChanged(this, e);
        }

        private void cboGamePlay_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Load the Priority information.
            string strSelectedGameplayOption = cboCharacterOption.SelectedValue?.ToString();
            if (strSelectedGameplayOption != null && strSelectedGameplayOption.IndexOf('|') != -1)
            {
                strSelectedGameplayOption = strSelectedGameplayOption.Split('|')[0];
            }
            XPathNavigator objXmlGameplayOption = _xmlGameplayOptionsDataGameplayOptionsNode.SelectSingleNode("gameplayoption[name = \"" + (strSelectedGameplayOption  ?? string.Empty) + "\"]");
            if (objXmlGameplayOption != null)
            {
                string strTemp = string.Empty;
                int intTemp = 0;
                if (objXmlGameplayOption.TryGetStringFieldQuickly("buildmethod", ref strTemp))
                {
                    lblBuildMethod.Text = strTemp;
                    switch (strTemp)
                    {
                        case "Karma":
                            lblBuildMethod.Text = LanguageManager.GetString("String_Karma");
                            goto default;
                        case "Priority":
                            lblBuildMethod.Text = LanguageManager.GetString("String_Priority");
                            lblBuildMethodParamLabel.Text = LanguageManager.GetString("Label_SelectBP_Priorities");
                            if (objXmlGameplayOption.TryGetStringFieldQuickly("priorities", ref strTemp))
                            {
                                lblBuildMethodParam.Text = strTemp;
                                lblBuildMethodParamLabel.Visible = true;
                                lblBuildMethodParam.Visible = true;
                            }
                            else
                            {
                                lblBuildMethodParamLabel.Visible = false;
                                lblBuildMethodParam.Visible = false;
                            }
                            break;
                        case "SumtoTen":
                            lblBuildMethod.Text = LanguageManager.GetString("String_SumtoTen");
                            lblBuildMethodParamLabel.Text = LanguageManager.GetString("String_SumtoTen");
                            if (objXmlGameplayOption.TryGetInt32FieldQuickly("sumtoten", ref intTemp))
                            {
                                lblBuildMethodParam.Text = intTemp.ToString(GlobalOptions.CultureInfo);
                                lblBuildMethodParamLabel.Visible = true;
                                lblBuildMethodParam.Visible = true;
                            }
                            else
                            {
                                lblBuildMethodParamLabel.Visible = false;
                                lblBuildMethodParam.Visible = false;
                            }
                            break;
                        case "LifeModule":
                            lblBuildMethod.Text = LanguageManager.GetString("String_LifeModule");
                            goto default;
                        default:
                            lblBuildMethodParamLabel.Visible = false;
                            lblBuildMethodParam.Visible = false;
                            break;
                    }
                    lblBuildMethodLabel.Visible = true;
                    lblBuildMethod.Visible = true;
                }
                else
                {
                    lblBuildMethodLabel.Visible = false;
                    lblBuildMethod.Visible = false;
                    lblBuildMethodParamLabel.Visible = false;
                    lblBuildMethodParam.Visible = false;
                }

                if (objXmlGameplayOption.TryGetInt32FieldQuickly("maxavailability", ref intTemp))
                {
                    lblMaxAvailLabel.Visible = true;
                    lblMaxAvail.Text = intTemp.ToString(GlobalOptions.CultureInfo);
                    lblMaxAvail.Visible = true;
                }
                else
                {
                    lblMaxAvailLabel.Visible = false;
                    lblMaxAvail.Visible = false;
                }

                if (objXmlGameplayOption.TryGetInt32FieldQuickly("karma", ref intTemp))
                {
                    lblKarmaLabel.Visible = true;
                    lblKarma.Text = intTemp.ToString(GlobalOptions.CultureInfo);
                    lblKarma.Visible = true;
                }
                else
                {
                    lblKarmaLabel.Visible = false;
                    lblKarma.Visible = false;
                }

                if (objXmlGameplayOption.TryGetInt32FieldQuickly("maxnuyen", ref intTemp))
                {
                    lblMaxNuyenLabel.Visible = true;
                    lblMaxNuyen.Text = intTemp.ToString(GlobalOptions.CultureInfo);
                    lblMaxNuyen.Visible = true;
                }
                else
                {
                    lblMaxNuyenLabel.Visible = false;
                    lblMaxNuyen.Visible = false;
                }

                if (objXmlGameplayOption.TryGetInt32FieldQuickly("maxdevicerating", ref intTemp))
                {
                    lblMaxDRLabel.Visible = true;
                    lblMaxDR.Text = intTemp.ToString(GlobalOptions.CultureInfo);
                    lblMaxDR.Visible = true;
                }
                else
                {
                    lblMaxDRLabel.Visible = false;
                    lblMaxDR.Visible = false;
                }
            }
        }
        #endregion
    }
}
