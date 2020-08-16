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
using System.Xml;
using System.Xml.XPath;
 using System.Text;

namespace Chummer
{
    public partial class frmKarmaMetatype : Form
    {
        private bool _blnLoading = true;

        private readonly Character _objCharacter;
        private string _strCurrentPossessionMethod;

        private readonly XPathNavigator _xmlBaseMetatypeDataNode;
        private readonly XPathNavigator _xmlBaseQualityDataNode;
        private readonly XmlNode _xmlSkillsDocumentKnowledgeSkillsNode;
        private readonly XmlNode _xmlMetatypeDocumentMetatypesNode;
        private readonly XmlNode _xmlQualityDocumentQualitiesNode;
        private readonly XmlNode _xmlCritterPowerDocumentPowersNode;

        #region Form Events
        public frmKarmaMetatype(Character objCharacter, string strXmlFile = "metatypes.xml")
        {
            _objCharacter = objCharacter;
            InitializeComponent();
            this.TranslateWinForm();

            XmlDocument xmlMetatypeDoc = XmlManager.Load(strXmlFile);
            _xmlMetatypeDocumentMetatypesNode = xmlMetatypeDoc.SelectSingleNode("/chummer/metatypes");
            _xmlBaseMetatypeDataNode = xmlMetatypeDoc.GetFastNavigator().SelectSingleNode("/chummer");
            _xmlSkillsDocumentKnowledgeSkillsNode = XmlManager.Load("skills.xml").SelectSingleNode("/chummer/knowledgeskills");
            XmlDocument xmlQualityDoc = XmlManager.Load("qualities.xml");
            _xmlQualityDocumentQualitiesNode = xmlQualityDoc.SelectSingleNode("/chummer/qualities");
            _xmlBaseQualityDataNode = xmlQualityDoc.GetFastNavigator().SelectSingleNode("/chummer");
            _xmlCritterPowerDocumentPowersNode = XmlManager.Load("critterpowers.xml").SelectSingleNode("/chummer/powers");
        }

        private void frmMetatype_Load(object sender, EventArgs e)
        {
            // Populate the Metatype Category list.
            List<ListItem> lstCategories = new List<ListItem>(3);

            // Create a list of Categories.
            XPathNavigator xmlMetatypesNode = _xmlBaseMetatypeDataNode.SelectSingleNode("metatypes");
            if (xmlMetatypesNode != null)
            {
                HashSet<string> lstAlreadyProcessed = new HashSet<string>();
                foreach (XPathNavigator objXmlCategory in _xmlBaseMetatypeDataNode.Select("categories/category"))
                {
                    string strInnerText = objXmlCategory.Value;
                    if (!lstAlreadyProcessed.Contains(strInnerText))
                    {
                        lstAlreadyProcessed.Add(strInnerText);
                        if (null != xmlMetatypesNode.SelectSingleNode(string.Format(GlobalOptions.InvariantCultureInfo,
                            "metatype[category = {0} and ({1})]",
                            strInnerText.CleanXPath(), _objCharacter.Options.BookXPath())))
                        {
                            lstCategories.Add(new ListItem(strInnerText, objXmlCategory.SelectSingleNode("@translate")?.Value
                                                                         ?? strInnerText));
                        }
                    }
                }
            }

            lstCategories.Sort(CompareListItems.CompareNames);
            cboCategory.BeginUpdate();
            cboCategory.ValueMember = nameof(ListItem.Value);
            cboCategory.DisplayMember = nameof(ListItem.Name);
            cboCategory.DataSource = lstCategories;

            // Attempt to select the default Metahuman Category. If it could not be found, select the first item in the list instead.
            cboCategory.SelectedValue = _objCharacter.MetatypeCategory;
            if (cboCategory.SelectedIndex == -1 && cboCategory.Items.Count > 0)
                cboCategory.SelectedIndex = 0;

            cboCategory.EndUpdate();

            // Add Possession and Inhabitation to the list of Critter Tradition variations.
            chkPossessionBased.SetToolTip(LanguageManager.GetString("Tip_Metatype_PossessionTradition"));
            chkBloodSpirit.SetToolTip(LanguageManager.GetString("Tip_Metatype_BloodSpirit"));

            List<ListItem> lstMethods = new List<ListItem>(2)
            {
                new ListItem("Possession", _xmlCritterPowerDocumentPowersNode?.SelectSingleNode("power[name = \"Possession\"]/translate")?.InnerText ?? "Possession"),
                new ListItem("Inhabitation", _xmlCritterPowerDocumentPowersNode?.SelectSingleNode("power[name = \"Inhabitation\"]/translate")?.InnerText ?? "Inhabitation")
            };

            lstMethods.Sort(CompareListItems.CompareNames);

            foreach (CritterPower objPower in _objCharacter.CritterPowers)
            {
                string strPowerName = objPower.Name;
                if (lstMethods.Any(x => strPowerName.Equals(x.Value.ToString(), StringComparison.OrdinalIgnoreCase)))
                {
                    _strCurrentPossessionMethod = strPowerName;
                    break;
                }
            }

            cboPossessionMethod.BeginUpdate();
            cboPossessionMethod.ValueMember = nameof(ListItem.Value);
            cboPossessionMethod.DisplayMember = nameof(ListItem.Name);
            cboPossessionMethod.DataSource = lstMethods;
            cboPossessionMethod.EndUpdate();

            PopulateMetatypes();
            PopulateMetavariants();
            RefreshSelectedMetavariant();

            _blnLoading = false;
        }
        #endregion

        #region Control Events
        private void lstMetatypes_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_blnLoading)
                return;

            PopulateMetavariants();
        }

        private void lstMetatypes_DoubleClick(object sender, EventArgs e)
        {
            MetatypeSelected();
        }

        private void cmdOK_Click(object sender, EventArgs e)
        {
            MetatypeSelected();
        }

        private void cboMetavariant_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_blnLoading)
                return;

            RefreshSelectedMetavariant();
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void cboCategory_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_blnLoading)
                return;
            PopulateMetatypes();
        }

        private void chkPossessionBased_CheckedChanged(object sender, EventArgs e)
        {
            cboPossessionMethod.Enabled = chkPossessionBased.Checked;
        }
        #endregion

        #region Custom Methods
        /// <summary>
        /// A Metatype has been selected, so fill in all of the necessary Character information.
        /// </summary>
        private void MetatypeSelected()
        {
            using (new CursorWait(this))
            {
                string strSelectedMetatype = lstMetatypes.SelectedValue?.ToString();
                if (!string.IsNullOrEmpty(strSelectedMetatype))
                {
                    string strSelectedMetatypeCategory = cboCategory.SelectedValue?.ToString();
                    string strSelectedMetavariant = cboMetavariant.SelectedValue?.ToString() ?? Guid.Empty.ToString();

                    XmlNode objXmlMetatype = _xmlMetatypeDocumentMetatypesNode.SelectSingleNode("metatype[id = \"" + strSelectedMetatype + "\"]");
                    if (objXmlMetatype == null)
                    {
                        Program.MainForm.ShowMessageBox(this, LanguageManager.GetString("Message_Metatype_SelectMetatype"), LanguageManager.GetString("MessageTitle_Metatype_SelectMetatype"), MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                        return;
                    }

                    int intForce = 0;
                    if (nudForce.Visible)
                        intForce = decimal.ToInt32(nudForce.Value);

                    // If this is a Shapeshifter, a Metavariant must be selected. Default to Human if None is selected.
                    if (strSelectedMetatypeCategory == "Shapeshifter" && strSelectedMetavariant == Guid.Empty.ToString())
                        strSelectedMetavariant = objXmlMetatype.SelectSingleNode("metavariants/metavariant[name = \"Human\"]/id")?.InnerText ?? "None";
                    if (_objCharacter.MetatypeGuid.ToString("D", GlobalOptions.InvariantCultureInfo) != strSelectedMetatype
                        || _objCharacter.MetavariantGuid.ToString("D", GlobalOptions.InvariantCultureInfo) != strSelectedMetavariant)
                        _objCharacter.Create(strSelectedMetatypeCategory, strSelectedMetatype, strSelectedMetavariant, objXmlMetatype,
                            intForce, _xmlQualityDocumentQualitiesNode, _xmlCritterPowerDocumentPowersNode, _xmlSkillsDocumentKnowledgeSkillsNode);

                    DialogResult = DialogResult.OK;
                    Close();
                }
                else
                {
                    Program.MainForm.ShowMessageBox(this, LanguageManager.GetString("Message_Metatype_SelectMetatype"), LanguageManager.GetString("MessageTitle_Metatype_SelectMetatype"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void RefreshSelectedMetavariant()
        {
            string strSpace = LanguageManager.GetString("String_Space");
            XPathNavigator objXmlMetatype = null;
            XPathNavigator objXmlMetavariant = null;
            string strSelectedMetatype = lstMetatypes.SelectedValue?.ToString();
            if (!string.IsNullOrEmpty(strSelectedMetatype))
            {
                objXmlMetatype = _xmlBaseMetatypeDataNode.SelectSingleNode("metatypes/metatype[id = \"" + strSelectedMetatype + "\"]");
                string strSelectedMetavariant = cboMetavariant.SelectedValue?.ToString();
                if (objXmlMetatype != null && !string.IsNullOrEmpty(strSelectedMetavariant) && strSelectedMetavariant != "None")
                {
                    objXmlMetavariant = objXmlMetatype.SelectSingleNode("metavariants/metavariant[id = \"" + strSelectedMetavariant + "\"]");
                }
            }

            string strAttributeFormat = "{0}/{1}{2}({3})";
            if (objXmlMetavariant != null)
            {
                cmdOK.Enabled = true;
                if (objXmlMetatype.SelectSingleNode("forcecreature") == null)
                {
                    lblBOD.Text = string.Format(GlobalOptions.CultureInfo, strAttributeFormat, objXmlMetavariant.SelectSingleNode("bodmin")?.Value ?? 0.ToString(GlobalOptions.CultureInfo),
                        objXmlMetavariant.SelectSingleNode("bodmax")?.Value ?? 0.ToString(GlobalOptions.CultureInfo), strSpace, objXmlMetavariant.SelectSingleNode("bodaug")?.Value ?? 0.ToString(GlobalOptions.CultureInfo));
                    lblAGI.Text = string.Format(GlobalOptions.CultureInfo, strAttributeFormat, objXmlMetavariant.SelectSingleNode("agimin")?.Value ?? 0.ToString(GlobalOptions.CultureInfo),
                        objXmlMetavariant.SelectSingleNode("agimax")?.Value ?? 0.ToString(GlobalOptions.CultureInfo), strSpace, objXmlMetavariant.SelectSingleNode("agiaug")?.Value ?? 0.ToString(GlobalOptions.CultureInfo));
                    lblREA.Text = string.Format(GlobalOptions.CultureInfo, strAttributeFormat, objXmlMetavariant.SelectSingleNode("reamin")?.Value ?? 0.ToString(GlobalOptions.CultureInfo),
                        objXmlMetavariant.SelectSingleNode("reamax")?.Value ?? 0.ToString(GlobalOptions.CultureInfo), strSpace, objXmlMetavariant.SelectSingleNode("reaaug")?.Value ?? 0.ToString(GlobalOptions.CultureInfo));
                    lblSTR.Text = string.Format(GlobalOptions.CultureInfo, strAttributeFormat, objXmlMetavariant.SelectSingleNode("strmin")?.Value ?? 0.ToString(GlobalOptions.CultureInfo),
                        objXmlMetavariant.SelectSingleNode("strmax")?.Value ?? 0.ToString(GlobalOptions.CultureInfo), strSpace, objXmlMetavariant.SelectSingleNode("straug")?.Value ?? 0.ToString(GlobalOptions.CultureInfo));
                    lblCHA.Text = string.Format(GlobalOptions.CultureInfo, strAttributeFormat, objXmlMetavariant.SelectSingleNode("chamin")?.Value ?? 0.ToString(GlobalOptions.CultureInfo),
                        objXmlMetavariant.SelectSingleNode("chamax")?.Value ?? 0.ToString(GlobalOptions.CultureInfo), strSpace, objXmlMetavariant.SelectSingleNode("chaaug")?.Value ?? 0.ToString(GlobalOptions.CultureInfo));
                    lblINT.Text = string.Format(GlobalOptions.CultureInfo, strAttributeFormat, objXmlMetavariant.SelectSingleNode("intmin")?.Value ?? 0.ToString(GlobalOptions.CultureInfo),
                        objXmlMetavariant.SelectSingleNode("intmax")?.Value ?? 0.ToString(GlobalOptions.CultureInfo), strSpace, objXmlMetavariant.SelectSingleNode("intaug")?.Value ?? 0.ToString(GlobalOptions.CultureInfo));
                    lblLOG.Text = string.Format(GlobalOptions.CultureInfo, strAttributeFormat, objXmlMetavariant.SelectSingleNode("logmin")?.Value ?? 0.ToString(GlobalOptions.CultureInfo),
                        objXmlMetavariant.SelectSingleNode("logmax")?.Value ?? 0.ToString(GlobalOptions.CultureInfo), strSpace, objXmlMetavariant.SelectSingleNode("logaug")?.Value ?? 0.ToString(GlobalOptions.CultureInfo));
                    lblWIL.Text = string.Format(GlobalOptions.CultureInfo, strAttributeFormat, objXmlMetavariant.SelectSingleNode("wilmin")?.Value ?? 0.ToString(GlobalOptions.CultureInfo),
                        objXmlMetavariant.SelectSingleNode("wilmax")?.Value ?? 0.ToString(GlobalOptions.CultureInfo), strSpace, objXmlMetavariant.SelectSingleNode("wilaug")?.Value ?? 0.ToString(GlobalOptions.CultureInfo));
                    lblINI.Text = string.Format(GlobalOptions.CultureInfo, strAttributeFormat, objXmlMetavariant.SelectSingleNode("inimin")?.Value ?? 0.ToString(GlobalOptions.CultureInfo),
                        objXmlMetavariant.SelectSingleNode("inimax")?.Value ?? 0.ToString(GlobalOptions.CultureInfo), strSpace, objXmlMetavariant.SelectSingleNode("iniaug")?.Value ?? 0.ToString(GlobalOptions.CultureInfo));
                }
                else
                {
                    lblBOD.Text = objXmlMetavariant.SelectSingleNode("bodmin")?.Value ?? objXmlMetatype.SelectSingleNode("bodmin")?.Value ?? 0.ToString(GlobalOptions.CultureInfo);
                    lblAGI.Text = objXmlMetavariant.SelectSingleNode("agimin")?.Value ?? objXmlMetatype.SelectSingleNode("agimin")?.Value ?? 0.ToString(GlobalOptions.CultureInfo);
                    lblREA.Text = objXmlMetavariant.SelectSingleNode("reamin")?.Value ?? objXmlMetatype.SelectSingleNode("reamin")?.Value ?? 0.ToString(GlobalOptions.CultureInfo);
                    lblSTR.Text = objXmlMetavariant.SelectSingleNode("strmin")?.Value ?? objXmlMetatype.SelectSingleNode("strmin")?.Value ?? 0.ToString(GlobalOptions.CultureInfo);
                    lblCHA.Text = objXmlMetavariant.SelectSingleNode("chamin")?.Value ?? objXmlMetatype.SelectSingleNode("chamin")?.Value ?? 0.ToString(GlobalOptions.CultureInfo);
                    lblINT.Text = objXmlMetavariant.SelectSingleNode("intmin")?.Value ?? objXmlMetatype.SelectSingleNode("intmin")?.Value ?? 0.ToString(GlobalOptions.CultureInfo);
                    lblLOG.Text = objXmlMetavariant.SelectSingleNode("logmin")?.Value ?? objXmlMetatype.SelectSingleNode("logmin")?.Value ?? 0.ToString(GlobalOptions.CultureInfo);
                    lblWIL.Text = objXmlMetavariant.SelectSingleNode("wilmin")?.Value ?? objXmlMetatype.SelectSingleNode("wilmin")?.Value ?? 0.ToString(GlobalOptions.CultureInfo);
                    lblINI.Text = objXmlMetavariant.SelectSingleNode("inimin")?.Value ?? objXmlMetatype.SelectSingleNode("inimin")?.Value ?? 0.ToString(GlobalOptions.CultureInfo);
                }

                // ReSharper disable once IdentifierTypo
                StringBuilder sbdQualities = new StringBuilder();
                // Build a list of the Metavariant's Qualities.
                foreach (XPathNavigator objXmlQuality in objXmlMetavariant.Select("qualities/*/quality"))
                {
                    if (GlobalOptions.Language != GlobalOptions.DefaultLanguage)
                    {
                        sbdQualities.Append(_xmlBaseQualityDataNode.SelectSingleNode("qualities/quality[name = \"" + objXmlQuality.Value + "\"]/translate")?.Value ?? objXmlQuality.Value);

                        string strSelect = objXmlQuality.SelectSingleNode("@select")?.Value;
                        if (!string.IsNullOrEmpty(strSelect))
                        {
                            sbdQualities.Append(LanguageManager.GetString("String_Space") + '(');
                            sbdQualities.Append(LanguageManager.TranslateExtra(strSelect));
                            sbdQualities.Append(')');
                        }
                    }
                    else
                    {
                        sbdQualities.Append(objXmlQuality.Value);
                        string strSelect = objXmlQuality.SelectSingleNode("@select")?.Value;
                        if (!string.IsNullOrEmpty(strSelect))
                        {
                            sbdQualities.Append(LanguageManager.GetString("String_Space") + '(');
                            sbdQualities.Append(strSelect);
                            sbdQualities.Append(')');
                        }
                    }
                    sbdQualities.Append(Environment.NewLine);
                }

                lblQualities.Text = sbdQualities.Length == 0 ? LanguageManager.GetString("String_None") : sbdQualities.ToString();

                lblKarma.Text = objXmlMetavariant.SelectSingleNode("karma")?.Value ?? 0.ToString(GlobalOptions.CultureInfo);
            }
            else if (objXmlMetatype != null)
            {
                cmdOK.Enabled = true;
                if (objXmlMetatype.SelectSingleNode("forcecreature") == null)
                {
                    lblBOD.Text = string.Format(GlobalOptions.CultureInfo, strAttributeFormat, objXmlMetatype.SelectSingleNode("bodmin")?.Value ?? 0.ToString(GlobalOptions.CultureInfo),
                        objXmlMetatype.SelectSingleNode("bodmax")?.Value ?? 0.ToString(GlobalOptions.CultureInfo), strSpace, objXmlMetatype.SelectSingleNode("bodaug")?.Value ?? 0.ToString(GlobalOptions.CultureInfo));
                    lblAGI.Text = string.Format(GlobalOptions.CultureInfo, strAttributeFormat, objXmlMetatype.SelectSingleNode("agimin")?.Value ?? 0.ToString(GlobalOptions.CultureInfo),
                        objXmlMetatype.SelectSingleNode("agimax")?.Value ?? 0.ToString(GlobalOptions.CultureInfo), strSpace, objXmlMetatype.SelectSingleNode("agiaug")?.Value ?? 0.ToString(GlobalOptions.CultureInfo));
                    lblREA.Text = string.Format(GlobalOptions.CultureInfo, strAttributeFormat, objXmlMetatype.SelectSingleNode("reamin")?.Value ?? 0.ToString(GlobalOptions.CultureInfo),
                        objXmlMetatype.SelectSingleNode("reamax")?.Value ?? 0.ToString(GlobalOptions.CultureInfo), strSpace, objXmlMetatype.SelectSingleNode("reaaug")?.Value ?? 0.ToString(GlobalOptions.CultureInfo));
                    lblSTR.Text = string.Format(GlobalOptions.CultureInfo, strAttributeFormat, objXmlMetatype.SelectSingleNode("strmin")?.Value ?? 0.ToString(GlobalOptions.CultureInfo),
                        objXmlMetatype.SelectSingleNode("strmax")?.Value ?? 0.ToString(GlobalOptions.CultureInfo), strSpace, objXmlMetatype.SelectSingleNode("straug")?.Value ?? 0.ToString(GlobalOptions.CultureInfo));
                    lblCHA.Text = string.Format(GlobalOptions.CultureInfo, strAttributeFormat, objXmlMetatype.SelectSingleNode("chamin")?.Value ?? 0.ToString(GlobalOptions.CultureInfo),
                        objXmlMetatype.SelectSingleNode("chamax")?.Value ?? 0.ToString(GlobalOptions.CultureInfo), strSpace, objXmlMetatype.SelectSingleNode("chaaug")?.Value ?? 0.ToString(GlobalOptions.CultureInfo));
                    lblINT.Text = string.Format(GlobalOptions.CultureInfo, strAttributeFormat, objXmlMetatype.SelectSingleNode("intmin")?.Value ?? 0.ToString(GlobalOptions.CultureInfo),
                        objXmlMetatype.SelectSingleNode("intmax")?.Value ?? 0.ToString(GlobalOptions.CultureInfo), strSpace, objXmlMetatype.SelectSingleNode("intaug")?.Value ?? 0.ToString(GlobalOptions.CultureInfo));
                    lblLOG.Text = string.Format(GlobalOptions.CultureInfo, strAttributeFormat, objXmlMetatype.SelectSingleNode("logmin")?.Value ?? 0.ToString(GlobalOptions.CultureInfo),
                        objXmlMetatype.SelectSingleNode("logmax")?.Value ?? 0.ToString(GlobalOptions.CultureInfo), strSpace, objXmlMetatype.SelectSingleNode("logaug")?.Value ?? 0.ToString(GlobalOptions.CultureInfo));
                    lblWIL.Text = string.Format(GlobalOptions.CultureInfo, strAttributeFormat, objXmlMetatype.SelectSingleNode("wilmin")?.Value ?? 0.ToString(GlobalOptions.CultureInfo),
                        objXmlMetatype.SelectSingleNode("wilmax")?.Value ?? 0.ToString(GlobalOptions.CultureInfo), strSpace, objXmlMetatype.SelectSingleNode("wilaug")?.Value ?? 0.ToString(GlobalOptions.CultureInfo));
                    lblINI.Text = string.Format(GlobalOptions.CultureInfo, strAttributeFormat, objXmlMetatype.SelectSingleNode("inimin")?.Value ?? 0.ToString(GlobalOptions.CultureInfo),
                        objXmlMetatype.SelectSingleNode("inimax")?.Value ?? 0.ToString(GlobalOptions.CultureInfo), strSpace, objXmlMetatype.SelectSingleNode("iniaug")?.Value ?? 0.ToString(GlobalOptions.CultureInfo));
                }
                else
                {
                    lblBOD.Text = objXmlMetatype.SelectSingleNode("bodmin")?.Value ?? 0.ToString(GlobalOptions.CultureInfo);
                    lblAGI.Text = objXmlMetatype.SelectSingleNode("agimin")?.Value ?? 0.ToString(GlobalOptions.CultureInfo);
                    lblREA.Text = objXmlMetatype.SelectSingleNode("reamin")?.Value ?? 0.ToString(GlobalOptions.CultureInfo);
                    lblSTR.Text = objXmlMetatype.SelectSingleNode("strmin")?.Value ?? 0.ToString(GlobalOptions.CultureInfo);
                    lblCHA.Text = objXmlMetatype.SelectSingleNode("chamin")?.Value ?? 0.ToString(GlobalOptions.CultureInfo);
                    lblINT.Text = objXmlMetatype.SelectSingleNode("intmin")?.Value ?? 0.ToString(GlobalOptions.CultureInfo);
                    lblLOG.Text = objXmlMetatype.SelectSingleNode("logmin")?.Value ?? 0.ToString(GlobalOptions.CultureInfo);
                    lblWIL.Text = objXmlMetatype.SelectSingleNode("wilmin")?.Value ?? 0.ToString(GlobalOptions.CultureInfo);
                    lblINI.Text = objXmlMetatype.SelectSingleNode("inimin")?.Value ?? 0.ToString(GlobalOptions.CultureInfo);
                }

                // ReSharper disable once IdentifierTypo
                StringBuilder sbdQualities = new StringBuilder();
                // Build a list of the Metatype's Positive Qualities.
                foreach (XPathNavigator objXmlQuality in objXmlMetatype.Select("qualities/*/quality"))
                {
                    if (GlobalOptions.Language != GlobalOptions.DefaultLanguage)
                    {
                        sbdQualities.Append(_xmlBaseQualityDataNode.SelectSingleNode("qualities/quality[name = \"" + objXmlQuality.Value + "\"]/translate")?.Value ?? objXmlQuality.Value);

                        string strSelect = objXmlQuality.SelectSingleNode("@select")?.Value;
                        if (!string.IsNullOrEmpty(strSelect))
                        {
                            sbdQualities.Append(LanguageManager.GetString("String_Space") + '(');
                            sbdQualities.Append(LanguageManager.TranslateExtra(strSelect));
                            sbdQualities.Append(')');
                        }
                    }
                    else
                    {
                        sbdQualities.Append(objXmlQuality.Value);
                        string strSelect = objXmlQuality.SelectSingleNode("@select")?.Value;
                        if (!string.IsNullOrEmpty(strSelect))
                        {
                            sbdQualities.Append(LanguageManager.GetString("String_Space") + '(');
                            sbdQualities.Append(strSelect);
                            sbdQualities.Append(')');
                        }
                    }
                    sbdQualities.Append(Environment.NewLine);
                }

                lblQualities.Text = sbdQualities.Length == 0 ? LanguageManager.GetString("String_None") : sbdQualities.ToString();

                lblKarma.Text = objXmlMetatype.SelectSingleNode("karma")?.Value ?? 0.ToString(GlobalOptions.CultureInfo);
            }
            else
            {
                lblBOD.Text = string.Empty;
                lblAGI.Text = string.Empty;
                lblREA.Text = string.Empty;
                lblSTR.Text = string.Empty;
                lblCHA.Text = string.Empty;
                lblINT.Text = string.Empty;
                lblLOG.Text = string.Empty;
                lblWIL.Text = string.Empty;
                lblINI.Text = string.Empty;

                lblQualities.Text = string.Empty;

                lblKarma.Text = string.Empty;

                cmdOK.Enabled = false;
            }
            lblBODLabel.Visible = !string.IsNullOrEmpty(lblBOD.Text);
            lblAGILabel.Visible = !string.IsNullOrEmpty(lblAGI.Text);
            lblREALabel.Visible = !string.IsNullOrEmpty(lblREA.Text);
            lblSTRLabel.Visible = !string.IsNullOrEmpty(lblSTR.Text);
            lblCHALabel.Visible = !string.IsNullOrEmpty(lblCHA.Text);
            lblINTLabel.Visible = !string.IsNullOrEmpty(lblINT.Text);
            lblLOGLabel.Visible = !string.IsNullOrEmpty(lblLOG.Text);
            lblWILLabel.Visible = !string.IsNullOrEmpty(lblWIL.Text);
            lblINILabel.Visible = !string.IsNullOrEmpty(lblINI.Text);
            lblQualitiesLabel.Visible = !string.IsNullOrEmpty(lblQualities.Text);
            lblKarma.Visible = !string.IsNullOrEmpty(lblKarma.Text);
        }

        private void PopulateMetavariants()
        {
            string strSelectedMetatype = lstMetatypes.SelectedValue?.ToString();
            XPathNavigator objXmlMetatype = null;
            if (!string.IsNullOrEmpty(strSelectedMetatype))
                objXmlMetatype = _xmlBaseMetatypeDataNode.SelectSingleNode("metatypes/metatype[id = \"" + strSelectedMetatype + "\"]");
            // Don't attempt to do anything if nothing is selected.
            if (objXmlMetatype != null)
            {
                List<ListItem> lstMetavariants = new List<ListItem>(5)
                {
                    new ListItem(Guid.Empty, LanguageManager.GetString("String_None"))
                };
                foreach (XPathNavigator objXmlMetavariant in objXmlMetatype.Select("metavariants/metavariant[" + _objCharacter.Options.BookXPath() + "]"))
                {
                    string strId = objXmlMetavariant.SelectSingleNode("id")?.Value;
                    if (!string.IsNullOrEmpty(strId))
                    {
                        lstMetavariants.Add(new ListItem(strId,
                            objXmlMetavariant.SelectSingleNode("translate")?.Value
                            ?? objXmlMetavariant.SelectSingleNode("name")?.Value
                            ?? LanguageManager.GetString("String_Unknown")));
                    }
                }

                // Retrieve the list of Metavariants for the selected Metatype.

                bool blnOldLoading = _blnLoading;
                string strOldSelectedValue = cboMetavariant.SelectedValue?.ToString() ?? _objCharacter?.MetavariantGuid.ToString("D", GlobalOptions.InvariantCultureInfo);
                _blnLoading = true;
                cboMetavariant.BeginUpdate();
                cboMetavariant.DataSource = null;
                cboMetavariant.ValueMember = nameof(ListItem.Value);
                cboMetavariant.DisplayMember = nameof(ListItem.Name);
                cboMetavariant.DataSource = lstMetavariants;
                cboMetavariant.Enabled = lstMetavariants.Count > 1;
                _blnLoading = blnOldLoading;
                if (!string.IsNullOrEmpty(strOldSelectedValue))
                {
                    if (cboMetavariant.SelectedValue?.ToString() == strOldSelectedValue)
                        cboMetavariant_SelectedIndexChanged(null, null);
                    else
                        cboMetavariant.SelectedValue = strOldSelectedValue;
                }
                if (cboMetavariant.SelectedIndex == -1 && lstMetavariants.Count > 0)
                    cboMetavariant.SelectedIndex = 0;
                cboMetavariant.EndUpdate();

                // If the Metatype has Force enabled, show the Force NUD.
                string strEssMax = objXmlMetatype.SelectSingleNode("essmax")?.Value ?? string.Empty;
                int intPos = strEssMax.IndexOf("D6", StringComparison.Ordinal);
                if (objXmlMetatype.SelectSingleNode("forcecreature") != null || intPos != -1)
                {
                    lblForceLabel.Visible = true;
                    nudForce.Visible = true;

                    if (intPos != -1)
                    {
                        if (intPos > 0)
                        {
                            intPos -= 1;
                            lblForceLabel.Text = strEssMax.Substring(intPos, 3).Replace("D6", LanguageManager.GetString("String_D6"));
                            nudForce.Maximum = Convert.ToInt32(strEssMax.Substring(intPos, 1), GlobalOptions.InvariantCultureInfo) * 6;
                        }
                        else
                        {
                            lblForceLabel.Text = 1.ToString(GlobalOptions.CultureInfo) + LanguageManager.GetString("String_D6");
                            nudForce.Maximum = 6;
                        }
                    }
                    else
                    {
                        // TODO: Unhardcode whether Force is called "Force" or "Level"
                        lblForceLabel.Text = LanguageManager.GetString(objXmlMetatype.SelectSingleNode("bodmax")?.Value == "0" &&
                                                                       objXmlMetatype.SelectSingleNode("agimax")?.Value == "0" &&
                                                                       objXmlMetatype.SelectSingleNode("reamax")?.Value == "0" &&
                                                                       objXmlMetatype.SelectSingleNode("strmax")?.Value == "0" &&
                                                                       objXmlMetatype.SelectSingleNode("magmin")?.Value.Contains('F') != true
                            ? "String_Level"
                            : "String_Force");
                        nudForce.Maximum = 100;
                    }
                }
                else
                {
                    lblForceLabel.Visible = false;
                    nudForce.Visible = false;
                }
                lblMetavariantLabel.Visible = true;
                cboMetavariant.Visible = true;
            }
            else
            {
                lblMetavariantLabel.Visible = false;
                cboMetavariant.Visible = false;
                // Clear the Metavariant list if nothing is currently selected.
                List<ListItem> lstMetavariants = new List<ListItem>(5)
                {
                    new ListItem(Guid.Empty, LanguageManager.GetString("String_None"))
                };

                bool blnOldLoading = _blnLoading;
                _blnLoading = true;
                cboMetavariant.BeginUpdate();
                cboMetavariant.DataSource = null;
                cboMetavariant.ValueMember = nameof(ListItem.Value);
                cboMetavariant.DisplayMember = nameof(ListItem.Name);
                cboMetavariant.DataSource = lstMetavariants;
                cboMetavariant.Enabled = false;
                _blnLoading = blnOldLoading;
                cboMetavariant.SelectedIndex = 0;
                cboMetavariant.EndUpdate();

                lblForceLabel.Visible = false;
                nudForce.Visible = false;
            }
        }

        /// <summary>
        /// Populate the list of Metatypes.
        /// </summary>
        private void PopulateMetatypes()
        {
            string strSelectedCategory = cboCategory.SelectedValue?.ToString();
            if (!string.IsNullOrEmpty(strSelectedCategory))
            {
                List<ListItem> lstMetatypeItems = new List<ListItem>();
                foreach (XPathNavigator xmlMetatype in _xmlBaseMetatypeDataNode.Select(
                    string.Format(GlobalOptions.InvariantCultureInfo, "metatypes/metatype[({0}) and category = {1}]",
                        _objCharacter.Options.BookXPath(), strSelectedCategory.CleanXPath())))
                {
                    string strId = xmlMetatype.SelectSingleNode("id")?.Value;
                    if (!string.IsNullOrEmpty(strId))
                    {
                        lstMetatypeItems.Add(new ListItem(strId,
                            xmlMetatype.SelectSingleNode("translate")?.Value
                            ?? xmlMetatype.SelectSingleNode("name")?.Value
                            ?? LanguageManager.GetString("String_Unknown")));
                    }
                }

                lstMetatypeItems.Sort(CompareListItems.CompareNames);

                bool blnOldLoading = _blnLoading;
                string strOldSelected = lstMetatypes.SelectedValue?.ToString() ?? _objCharacter?.MetatypeGuid.ToString("D", GlobalOptions.InvariantCultureInfo);
                if (strOldSelected == Guid.Empty.ToString("D", GlobalOptions.InvariantCultureInfo))
                    strOldSelected = _objCharacter.GetNode(true)?.SelectSingleNode("id")?.Value ?? string.Empty;
                _blnLoading = true;
                lstMetatypes.BeginUpdate();
                lstMetatypes.DataSource = null;
                lstMetatypes.ValueMember = nameof(ListItem.Value);
                lstMetatypes.DisplayMember = nameof(ListItem.Name);
                lstMetatypes.DataSource = lstMetatypeItems;
                _blnLoading = blnOldLoading;
                // Attempt to select the default Human item. If it could not be found, select the first item in the list instead.
                if (!string.IsNullOrEmpty(strOldSelected))
                {
                    if (lstMetatypes.SelectedValue?.ToString() == strOldSelected)
                        lstMetatypes_SelectedIndexChanged(this, EventArgs.Empty);
                    else
                        lstMetatypes.SelectedValue = strOldSelected;
                }
                if (lstMetatypes.SelectedIndex == -1 && lstMetatypeItems.Count > 0)
                    lstMetatypes.SelectedIndex = 0;

                lstMetatypes.EndUpdate();

                if (strSelectedCategory.EndsWith("Spirits", StringComparison.Ordinal))
                {
                    if (!chkPossessionBased.Visible && !string.IsNullOrEmpty(_strCurrentPossessionMethod))
                    {
                        chkPossessionBased.Checked = true;
                        cboPossessionMethod.SelectedValue = _strCurrentPossessionMethod;
                    }
                    chkBloodSpirit.Visible = true;
                    chkPossessionBased.Visible = true;
                    cboPossessionMethod.Visible = true;
                }
                else
                {
                    chkBloodSpirit.Checked = false;
                    chkBloodSpirit.Visible = false;
                    chkPossessionBased.Visible = false;
                    chkPossessionBased.Checked = false;
                    cboPossessionMethod.Visible = false;
                }
            }
            else
            {
                lstMetatypes.BeginUpdate();
                lstMetatypes.DataSource = null;
                lstMetatypes.EndUpdate();

                chkBloodSpirit.Checked = false;
                chkBloodSpirit.Visible = false;
                chkPossessionBased.Visible = false;
                chkPossessionBased.Checked = false;
                cboPossessionMethod.Visible = false;
            }
        }
        #endregion
    }
}
