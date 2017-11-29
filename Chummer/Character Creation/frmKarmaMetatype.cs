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
using System.Windows;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;
 using Chummer.Backend.Equipment;
 using Chummer.Skills;
using MessageBox = System.Windows.Forms.MessageBox;
using System.Text;

namespace Chummer
{
    public partial class frmKarmaMetatype : Form
    {
        private readonly Character _objCharacter;

        private string _strXmlFile = "metatypes.xml";

        private readonly List<ListItem> _lstCategory = new List<ListItem>();

        #region Character Events
        private void objCharacter_MAGEnabledChanged(object sender)
        {
            // Do nothing. This is just an Event trap so an exception doesn't get thrown.
        }

        private void objCharacter_RESEnabledChanged(object sender)
        {
            // Do nothing. This is just an Event trap so an exception doesn't get thrown.
        }

        private void objCharacter_DEPEnabledChanged(object sender)
        {
            // Do nothing. This is just an Event trap so an exception doesn't get thrown.
        }

        private void objCharacter_AdeptTabEnabledChanged(object sender)
        {
            // Do nothing. This is just an Event trap so an exception doesn't get thrown.
        }

        private void objCharacter_MagicianTabEnabledChanged(object sender)
        {
            // Do nothing. This is just an Event trap so an exception doesn't get thrown.
        }

        private void objCharacter_TechnomancerTabEnabledChanged(object sender)
        {
            // Do nothing. This is just an Event trap so an exception doesn't get thrown.
        }

        private void objCharacter_AdvancedProgramsTabEnabledChanged(object sender)
        {
            // Do nothing. This is just an Event trap so an exception doesn't get thrown.
        }

        private void objCharacter_CyberwareTabDisabledChanged(object sender)
        {
            // Do nothing. This is just an Event trap so an exception doesn't get thrown.
        }

        private void objCharacter_InitiationTabEnabledChanged(object sender)
        {
            // Do nothing. This is just an Event trap so an exception doesn't get thrown.
        }

        private void objCharacter_CritterTabEnabledChanged(object sender)
        {
            // Do nothing. This is just an Event trap so an exception doesn't get thrown.
        }
        #endregion

        #region Properties
        /// <summary>
        /// XML file to read Metatype/Critter information from.
        /// </summary>
        public string XmlFile
        {
            set
            {
                _strXmlFile = value;
            }
        }
        #endregion

        #region Form Events
        public frmKarmaMetatype(Character objCharacter)
        {
            _objCharacter = objCharacter;
            InitializeComponent();
            LanguageManager.Load(GlobalOptions.Language, this);

            // Attach EventHandlers for MAGEnabledChange and RESEnabledChanged since some Metatypes can enable these.
            _objCharacter.MAGEnabledChanged += objCharacter_MAGEnabledChanged;
            _objCharacter.RESEnabledChanged += objCharacter_RESEnabledChanged;
            _objCharacter.DEPEnabledChanged += objCharacter_DEPEnabledChanged;
            _objCharacter.AdeptTabEnabledChanged += objCharacter_AdeptTabEnabledChanged;
            _objCharacter.MagicianTabEnabledChanged += objCharacter_MagicianTabEnabledChanged;
            _objCharacter.TechnomancerTabEnabledChanged += objCharacter_TechnomancerTabEnabledChanged;
            _objCharacter.AdvancedProgramsTabEnabledChanged += objCharacter_AdvancedProgramsTabEnabledChanged;
            _objCharacter.CyberwareTabDisabledChanged += objCharacter_CyberwareTabDisabledChanged;
            _objCharacter.InitiationTabEnabledChanged += objCharacter_InitiationTabEnabledChanged;
            _objCharacter.CritterTabEnabledChanged += objCharacter_CritterTabEnabledChanged;
        }

        private void frmMetatype_FormClosed(object sender, FormClosedEventArgs e)
        {
            // Detach EventHandlers for MAGEnabledChange and RESEnabledChanged since some Metatypes can enable these.
            _objCharacter.MAGEnabledChanged -= objCharacter_MAGEnabledChanged;
            _objCharacter.RESEnabledChanged -= objCharacter_RESEnabledChanged;
            _objCharacter.DEPEnabledChanged -= objCharacter_DEPEnabledChanged;
            _objCharacter.AdeptTabEnabledChanged -= objCharacter_AdeptTabEnabledChanged;
            _objCharacter.MagicianTabEnabledChanged -= objCharacter_MagicianTabEnabledChanged;
            _objCharacter.TechnomancerTabEnabledChanged -= objCharacter_TechnomancerTabEnabledChanged;
            _objCharacter.AdvancedProgramsTabEnabledChanged -= objCharacter_AdvancedProgramsTabEnabledChanged;
            _objCharacter.CyberwareTabDisabledChanged -= objCharacter_CyberwareTabDisabledChanged;
            _objCharacter.InitiationTabEnabledChanged -= objCharacter_InitiationTabEnabledChanged;
            _objCharacter.CritterTabEnabledChanged -= objCharacter_CritterTabEnabledChanged;
        }

        private void frmMetatype_Load(object sender, EventArgs e)
        {
            // Load the Metatype information.
            XmlDocument objXmlDocument = XmlManager.Load(_strXmlFile);

            // Populate the Metatype Category list.
            XmlNodeList objXmlCategoryList = objXmlDocument.SelectNodes("/chummer/categories/category");

            // Create a list of Categories.
            HashSet<string> lstAlreadyProcessed = new HashSet<string>();
            foreach (XmlNode objXmlCategory in objXmlCategoryList)
            {
                string strInnerText = objXmlCategory.InnerText;
                if (!lstAlreadyProcessed.Contains(strInnerText))
                {
                    lstAlreadyProcessed.Add(strInnerText);
                    string strXPath = "/chummer/metatypes/metatype[category = \"" + strInnerText + "\" and (" + _objCharacter.Options.BookXPath() + ")]";

                    XmlNode objFirstItem = objXmlDocument.SelectSingleNode(strXPath);
                    if (objFirstItem != null)
                    {
                        ListItem objItem = new ListItem();
                        objItem.Value = strInnerText;
                        objItem.Name = objXmlCategory.Attributes?["translate"]?.InnerText ?? strInnerText;
                        _lstCategory.Add(objItem);
                    }
                }
            }

            SortListItem objSort = new SortListItem();
            _lstCategory.Sort(objSort.Compare);
            cboCategory.BeginUpdate();
            cboCategory.ValueMember = "Value";
            cboCategory.DisplayMember = "Name";
            cboCategory.DataSource = _lstCategory;

            // Attempt to select the default Metahuman Category. If it could not be found, select the first item in the list instead.
            if (cboCategory.Items.Contains("Metahuman"))
            {
                cboCategory.SelectedValue = "Metahuman";
            }
            else
            {
                cboCategory.SelectedIndex = 0;
            }

            if (cboCategory.Items.Contains("Human"))
            {
                lstMetatypes.SelectedValue = "Human";
            }
            else
            {
                cboCategory.SelectedIndex = 0;
            }
            cboCategory.EndUpdate();

            Height = cmdOK.Bottom + 40;
            lstMetatypes.Height = cmdOK.Bottom - lstMetatypes.Top;

            // Add Possession and Inhabitation to the list of Critter Tradition variations.
            tipTooltip.SetToolTip(chkPossessionBased, LanguageManager.GetString("Tip_Metatype_PossessionTradition"));
            tipTooltip.SetToolTip(chkBloodSpirit, LanguageManager.GetString("Tip_Metatype_BloodSpirit"));

            objXmlDocument = XmlManager.Load("critterpowers.xml");
            XmlNode objXmlPossession = objXmlDocument.SelectSingleNode("/chummer/powers/power[name = \"Possession\"]");
            XmlNode objXmlInhabitation = objXmlDocument.SelectSingleNode("/chummer/powers/power[name = \"Inhabitation\"]");
            List<ListItem> lstMethods = new List<ListItem>();

            ListItem objPossession = new ListItem();
            objPossession.Value = "Possession";
            objPossession.Name = objXmlPossession["translate"]?.InnerText ?? objXmlPossession["name"].InnerText;

            ListItem objInhabitation = new ListItem();
            objInhabitation.Value = "Inhabitation";
            objInhabitation.Name = objXmlInhabitation["translate"]?.InnerText ?? objXmlInhabitation["name"].InnerText;

            lstMethods.Add(objInhabitation);
            lstMethods.Add(objPossession);

            SortListItem objSortPossession = new SortListItem();
            lstMethods.Sort(objSortPossession.Compare);
            cboPossessionMethod.BeginUpdate();
            cboPossessionMethod.ValueMember = "Value";
            cboPossessionMethod.DisplayMember = "Name";
            cboPossessionMethod.DataSource = lstMethods;
            cboPossessionMethod.SelectedIndex = cboPossessionMethod.FindStringExact(objPossession.Name);
            cboPossessionMethod.EndUpdate();
            PopulateMetatypes();
        }
        #endregion

        #region Control Events
        private void lstMetatypes_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Don't attempt to do anything if nothing is selected.
            if (!string.IsNullOrEmpty(lstMetatypes.Text))
            {
                XmlDocument objXmlDocument = XmlManager.Load(_strXmlFile);
                XmlDocument objXmlQualityDocument = XmlManager.Load("qualities.xml");
                XmlNode objXmlMetatype = objXmlDocument.SelectSingleNode("/chummer/metatypes/metatype[name = \"" + lstMetatypes.SelectedValue + "\"]");

                lblBP.Text = objXmlMetatype["karma"].InnerText;
                if (objXmlMetatype["forcecreature"] == null)
                {
                    lblBOD.Text = string.Format("{0}/{1} ({2})", objXmlMetatype["bodmin"].InnerText, objXmlMetatype["bodmax"].InnerText, objXmlMetatype["bodaug"].InnerText);
                    lblAGI.Text = string.Format("{0}/{1} ({2})", objXmlMetatype["agimin"].InnerText, objXmlMetatype["agimax"].InnerText, objXmlMetatype["agiaug"].InnerText);
                    lblREA.Text = string.Format("{0}/{1} ({2})", objXmlMetatype["reamin"].InnerText, objXmlMetatype["reamax"].InnerText, objXmlMetatype["reaaug"].InnerText);
                    lblSTR.Text = string.Format("{0}/{1} ({2})", objXmlMetatype["strmin"].InnerText, objXmlMetatype["strmax"].InnerText, objXmlMetatype["straug"].InnerText);
                    lblCHA.Text = string.Format("{0}/{1} ({2})", objXmlMetatype["chamin"].InnerText, objXmlMetatype["chamax"].InnerText, objXmlMetatype["chaaug"].InnerText);
                    lblINT.Text = string.Format("{0}/{1} ({2})", objXmlMetatype["intmin"].InnerText, objXmlMetatype["intmax"].InnerText, objXmlMetatype["intaug"].InnerText);
                    lblLOG.Text = string.Format("{0}/{1} ({2})", objXmlMetatype["logmin"].InnerText, objXmlMetatype["logmax"].InnerText, objXmlMetatype["logaug"].InnerText);
                    lblWIL.Text = string.Format("{0}/{1} ({2})", objXmlMetatype["wilmin"].InnerText, objXmlMetatype["wilmax"].InnerText, objXmlMetatype["wilaug"].InnerText);
                    lblINI.Text = string.Format("{0}/{1} ({2})", objXmlMetatype["inimin"].InnerText, objXmlMetatype["inimax"].InnerText, objXmlMetatype["iniaug"].InnerText);
                }
                else
                {
                    lblBOD.Text = objXmlMetatype["bodmin"].InnerText;
                    lblAGI.Text = objXmlMetatype["agimin"].InnerText;
                    lblREA.Text = objXmlMetatype["reamin"].InnerText;
                    lblSTR.Text = objXmlMetatype["strmin"].InnerText;
                    lblCHA.Text = objXmlMetatype["chamin"].InnerText;
                    lblINT.Text = objXmlMetatype["intmin"].InnerText;
                    lblLOG.Text = objXmlMetatype["logmin"].InnerText;
                    lblWIL.Text = objXmlMetatype["wilmin"].InnerText;
                    lblINI.Text = objXmlMetatype["inimin"].InnerText;
                }

                List<ListItem> lstMetavariants = new List<ListItem>();
                ListItem objNone = new ListItem();
                objNone.Value = "None";
                objNone.Name = LanguageManager.GetString("String_None");
                lstMetavariants.Add(objNone);

                // Retrieve the list of Metavariants for the selected Metatype.
                XmlNodeList objXmlMetavariantList = objXmlMetatype.SelectNodes("metavariants/metavariant[" + _objCharacter.Options.BookXPath() + "]");
                foreach (XmlNode objXmlMetavariant in objXmlMetavariantList)
                {
                    ListItem objMetavariant = new ListItem();
                    objMetavariant.Value = objXmlMetavariant["name"].InnerText;
                    objMetavariant.Name = objXmlMetavariant["translate"]?.InnerText ?? objXmlMetavariant["name"].InnerText;
                    lstMetavariants.Add(objMetavariant);
                }

                cboMetavariant.BeginUpdate();
                cboMetavariant.ValueMember = "Value";
                cboMetavariant.DisplayMember = "Name";
                cboMetavariant.DataSource = lstMetavariants;

                // Select the None item.
                cboMetavariant.SelectedIndex = 0;
                cboMetavariant.EndUpdate();
                lblBP.Text = objXmlMetatype["karma"].InnerText;

                // If the Metatype has Force enabled, show the Force NUD.
                if (objXmlMetatype["forcecreature"] != null || objXmlMetatype["essmax"].InnerText.Contains("D6"))
                {
                    lblForceLabel.Visible = true;
                    nudForce.Visible = true;

                    if (objXmlMetatype["essmax"].InnerText.Contains("D6"))
                    {
                        int intPos = objXmlMetatype["essmax"].InnerText.IndexOf("D6") - 1;
                        lblForceLabel.Text = objXmlMetatype["essmax"].InnerText.Substring(intPos, 3);
                        nudForce.Maximum = Convert.ToInt32(objXmlMetatype["essmax"].InnerText.Substring(intPos, 1)) * 6;
                    }
                    else
                    {
                        lblForceLabel.Text = LanguageManager.GetString("String_Force");
                        nudForce.Maximum = 100;
                    }
                }
                else
                {
                    lblForceLabel.Visible = false;
                    nudForce.Visible = false;
                }
                    string strQualities = string.Empty;
                    // Build a list of the Metavariant's Positive Qualities.
                    foreach (XmlNode objXmlQuality in objXmlMetatype.SelectNodes("qualities/positive/quality"))
                    {
                            if (GlobalOptions.Language != GlobalOptions.DefaultLanguage)
                            {
                                XmlNode objQuality = objXmlQualityDocument.SelectSingleNode("/chummer/qualities/quality[name = \"" + objXmlQuality.InnerText + "\"]");
                            strQualities += objQuality["translate"]?.InnerText ?? objXmlQuality.InnerText;

                            if (!string.IsNullOrEmpty(objXmlQuality.Attributes["select"]?.InnerText))
                                    strQualities += " (" + LanguageManager.TranslateExtra(objXmlQuality.Attributes["select"].InnerText) + ")";
                            }
                            else
                            {
                                strQualities += objXmlQuality.InnerText;
                            if (!string.IsNullOrEmpty(objXmlQuality.Attributes["select"]?.InnerText))
                                    strQualities += " (" + objXmlQuality.Attributes["select"].InnerText + ")";
                            }
                        strQualities += "\n";
                    }
                    // Build a list of the Metavariant's Negative Qualities.
                    foreach (XmlNode objXmlQuality in objXmlMetatype.SelectNodes("qualities/negative/quality"))
                    {
                            if (GlobalOptions.Language != GlobalOptions.DefaultLanguage)
                            {
                                XmlNode objQuality = objXmlQualityDocument.SelectSingleNode("/chummer/qualities/quality[name = \"" + objXmlQuality.InnerText + "\"]");
                            strQualities += objQuality["translate"]?.InnerText ?? objXmlQuality.InnerText;

                            if (!string.IsNullOrEmpty(objXmlQuality.Attributes["select"]?.InnerText))
                                    strQualities += " (" + LanguageManager.TranslateExtra(objXmlQuality.Attributes["select"].InnerText) + ")";
                            }
                            else
                            {
                                strQualities += objXmlQuality.InnerText;
                            if (!string.IsNullOrEmpty(objXmlQuality.Attributes["select"]?.InnerText))
                                    strQualities += " (" + objXmlQuality.Attributes["select"].InnerText + ")";
                            }
                        strQualities += "\n";
                    }
                    lblQualities.Text = strQualities;
                }
            else
            {
                // Clear the Metavariant list if nothing is currently selected.
                List<ListItem> lstMetavariants = new List<ListItem>();
                ListItem objNone = new ListItem();
                objNone.Value = "None";
                objNone.Name = LanguageManager.GetString("String_None");
                lstMetavariants.Add(objNone);

                cboMetavariant.BeginUpdate();
                cboMetavariant.ValueMember = "Value";
                cboMetavariant.DisplayMember = "Name";
                cboMetavariant.DataSource = lstMetavariants;
                cboMetavariant.EndUpdate();
            }
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
            XmlDocument objXmlDocument = XmlManager.Load(_strXmlFile);
            XmlDocument objXmlQualityDocument = XmlManager.Load("qualities.xml");

            XmlNode objXmlMetatype = null;
            if (lstMetatypes.SelectedValue != null)
            {
                objXmlDocument.SelectSingleNode("/chummer/metatypes/metatype[name = \"" + lstMetatypes.SelectedValue + "\"]");
            }

            XmlNode objXmlMetavariant = null;
            if (cboMetavariant.SelectedValue != null && cboMetavariant.SelectedValue.ToString() != "None")
            {
                objXmlMetavariant = objXmlMetatype?.SelectSingleNode("metavariants/metavariant[name = \"" + cboMetavariant.SelectedValue + "\"]");
            }

            if (objXmlMetavariant != null)
            {
                if (objXmlMetatype["forcecreature"] == null)
                {
                    lblBOD.Text = string.Format("{0}/{1} ({2})",
                        objXmlMetavariant["bodmin"]?.InnerText ?? objXmlMetatype["bodmin"]?.InnerText ?? "0",
                        objXmlMetavariant["bodmax"]?.InnerText ?? objXmlMetatype["bodmax"]?.InnerText ?? "0",
                        objXmlMetavariant["bodaug"]?.InnerText ?? objXmlMetatype["bodaug"]?.InnerText ?? "0");
                    lblAGI.Text = string.Format("{0}/{1} ({2})",
                        objXmlMetavariant["agimin"]?.InnerText ?? objXmlMetatype["agimin"]?.InnerText ?? "0",
                        objXmlMetavariant["agimax"]?.InnerText ?? objXmlMetatype["agimax"]?.InnerText ?? "0",
                        objXmlMetavariant["agiaug"]?.InnerText ?? objXmlMetatype["agiaug"]?.InnerText ?? "0");
                    lblREA.Text = string.Format("{0}/{1} ({2})",
                        objXmlMetavariant["reamin"]?.InnerText ?? objXmlMetatype["reamin"]?.InnerText ?? "0",
                        objXmlMetavariant["reamax"]?.InnerText ?? objXmlMetatype["reamax"]?.InnerText ?? "0",
                        objXmlMetavariant["reaaug"]?.InnerText ?? objXmlMetatype["reaaug"]?.InnerText ?? "0");
                    lblSTR.Text = string.Format("{0}/{1} ({2})",
                        objXmlMetavariant["strmin"]?.InnerText ?? objXmlMetatype["strmin"]?.InnerText ?? "0",
                        objXmlMetavariant["strmax"]?.InnerText ?? objXmlMetatype["strmax"]?.InnerText ?? "0",
                        objXmlMetavariant["straug"]?.InnerText ?? objXmlMetatype["straug"]?.InnerText ?? "0");
                    lblCHA.Text = string.Format("{0}/{1} ({2})",
                        objXmlMetavariant["chamin"]?.InnerText ?? objXmlMetatype["chamin"]?.InnerText ?? "0",
                        objXmlMetavariant["chamax"]?.InnerText ?? objXmlMetatype["chamax"]?.InnerText ?? "0",
                        objXmlMetavariant["chaaug"]?.InnerText ?? objXmlMetatype["chaaug"]?.InnerText ?? "0");
                    lblINT.Text = string.Format("{0}/{1} ({2})",
                        objXmlMetavariant["intmin"]?.InnerText ?? objXmlMetatype["intmin"]?.InnerText ?? "0",
                        objXmlMetavariant["intmax"]?.InnerText ?? objXmlMetatype["intmax"]?.InnerText ?? "0",
                        objXmlMetavariant["intaug"]?.InnerText ?? objXmlMetatype["intaug"]?.InnerText ?? "0");
                    lblLOG.Text = string.Format("{0}/{1} ({2})",
                        objXmlMetavariant["logmin"]?.InnerText ?? objXmlMetatype["logmin"]?.InnerText ?? "0",
                        objXmlMetavariant["logmax"]?.InnerText ?? objXmlMetatype["logmax"]?.InnerText ?? "0",
                        objXmlMetavariant["logaug"]?.InnerText ?? objXmlMetatype["logaug"]?.InnerText ?? string.Empty);
                    lblWIL.Text = string.Format("{0}/{1} ({2})",
                        objXmlMetavariant["wilmin"]?.InnerText ?? objXmlMetatype["wilmin"]?.InnerText ?? "0",
                        objXmlMetavariant["wilmax"]?.InnerText ?? objXmlMetatype["wilmax"]?.InnerText ?? "0",
                        objXmlMetavariant["wilaug"]?.InnerText ?? objXmlMetatype["wilaug"]?.InnerText ?? "0");
                    lblINI.Text = string.Format("{0}/{1} ({2})",
                        objXmlMetavariant["inimin"]?.InnerText ?? objXmlMetatype["inimin"]?.InnerText ?? "0",
                        objXmlMetavariant["inimax"]?.InnerText ?? objXmlMetatype["inimax"]?.InnerText ?? "0",
                        objXmlMetavariant["iniaug"]?.InnerText ?? objXmlMetatype["iniaug"]?.InnerText ?? "0");
                }
                else
                {
                    lblBOD.Text = objXmlMetavariant["bodmin"]?.InnerText ?? objXmlMetatype["bodmin"]?.InnerText ?? "0";
                    lblAGI.Text = objXmlMetavariant["agimin"]?.InnerText ?? objXmlMetatype["agimin"]?.InnerText ?? "0";
                    lblREA.Text = objXmlMetavariant["reamin"]?.InnerText ?? objXmlMetatype["reamin"]?.InnerText ?? "0";
                    lblSTR.Text = objXmlMetavariant["strmin"]?.InnerText ?? objXmlMetatype["strmin"]?.InnerText ?? "0";
                    lblCHA.Text = objXmlMetavariant["chamin"]?.InnerText ?? objXmlMetatype["chamin"]?.InnerText ?? "0";
                    lblINT.Text = objXmlMetavariant["intmin"]?.InnerText ?? objXmlMetatype["intmin"]?.InnerText ?? "0";
                    lblLOG.Text = objXmlMetavariant["logmin"]?.InnerText ?? objXmlMetatype["logmin"]?.InnerText ?? "0";
                    lblWIL.Text = objXmlMetavariant["wilmin"]?.InnerText ?? objXmlMetatype["wilmin"]?.InnerText ?? "0";
                    lblINI.Text = objXmlMetavariant["inimin"]?.InnerText ?? objXmlMetatype["inimin"]?.InnerText ?? "0";
                }

                StringBuilder objQualities = new StringBuilder();
                // Build a list of the Metavariant's Positive Qualities.
                foreach (XmlNode objXmlQuality in objXmlMetavariant.SelectNodes("qualities/positive/quality"))
                {
                    if (GlobalOptions.Language != GlobalOptions.DefaultLanguage)
                    {
                        XmlNode objQuality = objXmlQualityDocument.SelectSingleNode("/chummer/qualities/quality[name = \"" + objXmlQuality.InnerText + "\"]");
                        objQualities.Append(objQuality?["translate"]?.InnerText ?? objXmlQuality.InnerText);

                        if (!string.IsNullOrEmpty(objXmlQuality.Attributes["select"]?.InnerText))
                            objQualities.Append(" (" + LanguageManager.TranslateExtra(objXmlQuality.Attributes["select"].InnerText) + ")");
                    }
                    else
                    {
                        objQualities.Append(objXmlQuality.InnerText);
                        if (!string.IsNullOrEmpty(objXmlQuality.Attributes["select"]?.InnerText))
                            objQualities.Append(" (" + objXmlQuality.Attributes["select"].InnerText + ")");
                    }
                    objQualities.Append("\n");
                }
                // Build a list of the Metavariant's Negative Qualities.
                foreach (XmlNode objXmlQuality in objXmlMetavariant.SelectNodes("qualities/negative/quality"))
                {
                    if (GlobalOptions.Language != GlobalOptions.DefaultLanguage)
                    {
                        XmlNode objQuality = objXmlQualityDocument.SelectSingleNode("/chummer/qualities/quality[name = \"" + objXmlQuality.InnerText + "\"]");
                        objQualities.Append(objQuality?["translate"]?.InnerText ?? objXmlQuality.InnerText);

                        if (!string.IsNullOrEmpty(objXmlQuality.Attributes["select"]?.InnerText))
                            objQualities.Append(" (" + LanguageManager.TranslateExtra(objXmlQuality.Attributes["select"].InnerText) + ")");
                    }
                    else
                    {
                        objQualities.Append(objXmlQuality.InnerText);
                        if (!string.IsNullOrEmpty(objXmlQuality.Attributes["select"]?.InnerText))
                            objQualities.Append(" (" + objXmlQuality.Attributes["select"].InnerText + ")");
                    }
                    objQualities.Append("\n");
                }
                if (objQualities.Length == 0)
                    lblQualities.Text = LanguageManager.GetString("String_None");
                else
                    lblQualities.Text = objQualities.ToString();
                lblBP.Text = objXmlMetavariant["karma"].InnerText;
            }
            else if (objXmlMetatype != null)
            {
                StringBuilder objQualities = new StringBuilder();
                // Build a list of the Metavariant's Positive Qualities.
                foreach (XmlNode objXmlQuality in objXmlMetatype.SelectNodes("qualities/positive/quality"))
                {
                    if (GlobalOptions.Language != GlobalOptions.DefaultLanguage)
                    {
                        XmlNode objQuality = objXmlQualityDocument.SelectSingleNode("/chummer/qualities/quality[name = \"" + objXmlQuality.InnerText + "\"]");
                        objQualities.Append(objQuality?["translate"]?.InnerText ?? objXmlQuality.InnerText);

                        if (!string.IsNullOrEmpty(objXmlQuality.Attributes["select"]?.InnerText))
                            objQualities.Append(" (" + LanguageManager.TranslateExtra(objXmlQuality.Attributes["select"].InnerText) + ")");
                    }
                    else
                    {
                        objQualities.Append(objXmlQuality.InnerText);
                        if (!string.IsNullOrEmpty(objXmlQuality.Attributes["select"]?.InnerText))
                            objQualities.Append(" (" + objXmlQuality.Attributes["select"].InnerText + ")");
                    }
                    objQualities.Append("\n");
                }
                // Build a list of the Metavariant's Negative Qualities.
                foreach (XmlNode objXmlQuality in objXmlMetatype.SelectNodes("qualities/negative/quality"))
                {
                    if (GlobalOptions.Language != GlobalOptions.DefaultLanguage)
                    {
                        XmlNode objQuality = objXmlQualityDocument.SelectSingleNode("/chummer/qualities/quality[name = \"" + objXmlQuality.InnerText + "\"]");
                        objQualities.Append(objQuality?["translate"]?.InnerText ?? objXmlQuality.InnerText);

                        if (!string.IsNullOrEmpty(objXmlQuality.Attributes["select"]?.InnerText))
                            objQualities.Append(" (" + LanguageManager.TranslateExtra(objXmlQuality.Attributes["select"].InnerText) + ")");
                    }
                    else
                    {
                        objQualities.Append(objXmlQuality.InnerText);
                        if (!string.IsNullOrEmpty(objXmlQuality.Attributes["select"]?.InnerText))
                            objQualities.Append(" (" + objXmlQuality.Attributes["select"].InnerText + ")");
                    }
                    objQualities.Append("\n");
                }
                if (objQualities.Length == 0)
                    lblQualities.Text = LanguageManager.GetString("String_None");
                else
                    lblQualities.Text = objQualities.ToString();
                lblBP.Text = objXmlMetatype["karma"].InnerText;
            }
            else
            {
                lblBP.Text = "0";
                lblQualities.Text = LanguageManager.GetString("String_None");
            }
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void cboCategory_SelectedIndexChanged(object sender, EventArgs e)
        {
            PopulateMetatypes();
        }
        #endregion

        #region Custom Methods
        /// <summary>
        /// A Metatype has been selected, so fill in all of the necessary Character information.
        /// </summary>
        private void MetatypeSelected()
        {
            if (!string.IsNullOrEmpty(lstMetatypes.Text))
            {
                XmlDocument objXmlDocument = XmlManager.Load(_strXmlFile);

                // If this is a Shapeshifter, a Metavariant must be selected. Default to Human if None is selected.
                if (cboCategory.SelectedValue.ToString() == "Shapeshifter" && cboMetavariant.SelectedValue.ToString() == "None")
                    cboMetavariant.SelectedValue = "Human";

                XmlNode objXmlMetatype = objXmlDocument.SelectSingleNode("/chummer/metatypes/metatype[name = \"" + lstMetatypes.SelectedValue + "\"]");
                XmlNode objXmlMetavariant = objXmlMetatype?.SelectSingleNode("metavariants/metavariant[name = \"" + cboMetavariant.SelectedValue + "\"]");
                int intForce = 0;
                if (nudForce.Visible)
                    intForce = decimal.ToInt32(nudForce.Value);

                // Set Metatype information.
                int intMinModifier = 0;
                int intMaxModifier = 0;
                //TODO: What the hell is this for?
                /*if (_strXmlFile == "critters.xml")
                {
                    if (cboCategory.SelectedValue.ToString() == "Technocritters")
                    {
                        intMinModifier = -1;
                        intMaxModifier = 1;
                    }
                    else
                    {
                        intMinModifier = -3;
                        intMaxModifier = 3;
                    }
                }*/

                XmlNode charNode;
                if (cboCategory.SelectedValue.ToString() == "Shapeshifter")
                {
                    charNode = objXmlMetatype;
                }
                else
                {
                    charNode = objXmlMetavariant ?? objXmlMetatype;
                }
                // Set Metatype information.
                _objCharacter.BOD.AssignLimits(ExpressionToString(charNode["bodmin"]?.InnerText, intForce, intMinModifier), ExpressionToString(charNode["bodmax"]?.InnerText, intForce, intMaxModifier), ExpressionToString(charNode["bodaug"]?.InnerText, intForce, intMaxModifier));
                _objCharacter.AGI.AssignLimits(ExpressionToString(charNode["agimin"]?.InnerText, intForce, intMinModifier), ExpressionToString(charNode["agimax"]?.InnerText, intForce, intMaxModifier), ExpressionToString(charNode["agiaug"]?.InnerText, intForce, intMaxModifier));
                _objCharacter.REA.AssignLimits(ExpressionToString(charNode["reamin"]?.InnerText, intForce, intMinModifier), ExpressionToString(charNode["reamax"]?.InnerText, intForce, intMaxModifier), ExpressionToString(charNode["reaaug"]?.InnerText, intForce, intMaxModifier));
                _objCharacter.STR.AssignLimits(ExpressionToString(charNode["strmin"]?.InnerText, intForce, intMinModifier), ExpressionToString(charNode["strmax"]?.InnerText, intForce, intMaxModifier), ExpressionToString(charNode["straug"]?.InnerText, intForce, intMaxModifier));
                _objCharacter.CHA.AssignLimits(ExpressionToString(charNode["chamin"]?.InnerText, intForce, intMinModifier), ExpressionToString(charNode["chamax"]?.InnerText, intForce, intMaxModifier), ExpressionToString(charNode["chaaug"]?.InnerText, intForce, intMaxModifier));
                _objCharacter.INT.AssignLimits(ExpressionToString(charNode["intmin"]?.InnerText, intForce, intMinModifier), ExpressionToString(charNode["intmax"]?.InnerText, intForce, intMaxModifier), ExpressionToString(charNode["intaug"]?.InnerText, intForce, intMaxModifier));
                _objCharacter.LOG.AssignLimits(ExpressionToString(charNode["logmin"]?.InnerText, intForce, intMinModifier), ExpressionToString(charNode["logmax"]?.InnerText, intForce, intMaxModifier), ExpressionToString(charNode["logaug"]?.InnerText, intForce, intMaxModifier));
                _objCharacter.WIL.AssignLimits(ExpressionToString(charNode["wilmin"]?.InnerText, intForce, intMinModifier), ExpressionToString(charNode["wilmax"]?.InnerText, intForce, intMaxModifier), ExpressionToString(charNode["wilaug"]?.InnerText, intForce, intMaxModifier));
                _objCharacter.MAG.AssignLimits(ExpressionToString(charNode["magmin"]?.InnerText, intForce, intMinModifier), ExpressionToString(charNode["magmax"]?.InnerText, intForce, intMaxModifier), ExpressionToString(charNode["magaug"]?.InnerText, intForce, intMaxModifier));
                _objCharacter.MAGAdept.AssignLimits(ExpressionToString(charNode["magmin"]?.InnerText, intForce, intMinModifier), ExpressionToString(charNode["magmax"]?.InnerText, intForce, intMaxModifier), ExpressionToString(charNode["magaug"]?.InnerText, intForce, intMaxModifier));
                _objCharacter.RES.AssignLimits(ExpressionToString(charNode["resmin"]?.InnerText, intForce, intMinModifier), ExpressionToString(charNode["resmax"]?.InnerText, intForce, intMaxModifier), ExpressionToString(charNode["resaug"]?.InnerText, intForce, intMaxModifier));
                _objCharacter.EDG.AssignLimits(ExpressionToString(charNode["edgmin"]?.InnerText, intForce, intMinModifier), ExpressionToString(charNode["edgmax"]?.InnerText, intForce, intMaxModifier), ExpressionToString(charNode["edgaug"]?.InnerText, intForce, intMaxModifier));
                _objCharacter.ESS.AssignLimits(ExpressionToString(charNode["essmin"]?.InnerText, intForce, 0), ExpressionToString(charNode["essmax"]?.InnerText, intForce, 0), ExpressionToString(charNode["essaug"]?.InnerText, intForce, 0));
                _objCharacter.DEP.AssignLimits(ExpressionToString(charNode["depmin"]?.InnerText, intForce, intMinModifier), ExpressionToString(charNode["depmax"]?.InnerText, intForce, intMaxModifier), ExpressionToString(charNode["depaug"]?.InnerText, intForce, intMaxModifier));

                _objCharacter.Metatype = lstMetatypes.SelectedValue.ToString();
                _objCharacter.MetatypeCategory = cboCategory.SelectedValue.ToString();
                _objCharacter.MetatypeBP = Convert.ToInt32(lblBP.Text);
                if (cboMetavariant.SelectedValue == null || cboMetavariant.SelectedValue.ToString() == "None")
                {
                    _objCharacter.Metavariant = string.Empty;
                }
                else
                {
                    _objCharacter.Metavariant = cboMetavariant.SelectedValue.ToString();
                }
                                
                if (objXmlMetatype?["movement"] != null)
                    _objCharacter.Movement = objXmlMetatype["movement"].InnerText;
                // Load the Qualities file.
                XmlDocument objXmlQualityDocument = XmlManager.Load("qualities.xml");

                // Determine if the Metatype has any bonuses.
                if (charNode.InnerXml.Contains("bonus") == true)
                    ImprovementManager.CreateImprovements(_objCharacter, Improvement.ImprovementSource.Metatype, lstMetatypes.SelectedValue.ToString(), objXmlMetatype.SelectSingleNode("bonus"), false, 1, lstMetatypes.SelectedValue.ToString());

                List<Weapon> objWeapons = new List<Weapon>();
                // Create the Qualities that come with the Metatype.
                foreach (XmlNode objXmlQualityItem in charNode.SelectNodes("qualities/positive/quality"))
                {
                    XmlNode objXmlQuality = objXmlQualityDocument.SelectSingleNode("/chummer/qualities/quality[name = \"" + objXmlQualityItem.InnerText + "\"]");
                    TreeNode objNode = new TreeNode();
                    List<TreeNode> objWeaponNodes = new List<TreeNode>();
                    Quality objQuality = new Quality(_objCharacter);
                    string strForceValue = string.Empty;
                    if (objXmlQualityItem.Attributes["select"] != null)
                        strForceValue = objXmlQualityItem.Attributes["select"].InnerText;
                    QualitySource objSource = QualitySource.Metatype;
                    if (objXmlQualityItem.Attributes["removable"] != null)
                        objSource = QualitySource.MetatypeRemovable;
                    objQuality.Create(objXmlQuality, _objCharacter, objSource, objNode, objWeapons, objWeaponNodes, strForceValue);
                    objQuality.ContributeToLimit = false;
                    _objCharacter.Qualities.Add(objQuality);
                }
                //Load any negative quality the character has. 
                foreach (XmlNode objXmlQualityItem in charNode.SelectNodes("qualities/negative/quality"))
                {
                    XmlNode objXmlQuality = objXmlQualityDocument.SelectSingleNode("/chummer/qualities/quality[name = \"" + objXmlQualityItem.InnerText + "\"]");
                    TreeNode objNode = new TreeNode();
                    List<TreeNode> objWeaponNodes = new List<TreeNode>();
                    Quality objQuality = new Quality(_objCharacter);
                    string strForceValue = string.Empty;
                    if (objXmlQualityItem.Attributes["select"] != null)
                        strForceValue = objXmlQualityItem.Attributes["select"].InnerText;
                    QualitySource objSource = new QualitySource();
                    objSource = QualitySource.Metatype;
                    if (objXmlQualityItem.Attributes["removable"] != null)
                        objSource = QualitySource.MetatypeRemovable;
                    objQuality.Create(objXmlQuality, _objCharacter, objSource, objNode, objWeapons, objWeaponNodes, strForceValue);
                    objQuality.ContributeToLimit = false;
                    _objCharacter.Qualities.Add(objQuality);
                }

                //Load any critter powers the character has. 
                objXmlDocument = XmlManager.Load("critterpowers.xml");
                foreach (XmlNode objXmlPower in charNode.SelectNodes("powers/power"))
                {
                    XmlNode objXmlCritterPower = objXmlDocument.SelectSingleNode("/chummer/powers/power[name = \"" + objXmlPower.InnerText + "\"]");
                    TreeNode objNode = new TreeNode();
                    CritterPower objPower = new CritterPower(_objCharacter);
                    string strForcedValue = string.Empty;
                    int intRating = 0;

                    if (objXmlPower.Attributes["rating"] != null)
                        intRating = Convert.ToInt32(objXmlPower.Attributes["rating"].InnerText);
                    if (objXmlPower.Attributes["select"] != null)
                        strForcedValue = objXmlPower.Attributes["select"].InnerText;

                    objPower.Create(objXmlCritterPower, objNode, intRating, strForcedValue);
                    objPower.CountTowardsLimit = false;
                    _objCharacter.CritterPowers.Add(objPower);
                }

                //Load any natural weapons the character has. 
                foreach (XmlNode objXmlNaturalWeapon in charNode.SelectNodes("nautralweapons/naturalweapon"))
                {
                    Weapon objWeapon = new Weapon(_objCharacter);
                    objWeapon.Name = objXmlNaturalWeapon["name"].InnerText;
                    objWeapon.Category = LanguageManager.GetString("Tab_Critter");
                    objWeapon.WeaponType = "Melee";
                    objWeapon.Reach = Convert.ToInt32(objXmlNaturalWeapon["reach"].InnerText);
                    objWeapon.Damage = objXmlNaturalWeapon["damage"].InnerText;
                    objWeapon.AP = objXmlNaturalWeapon["ap"].InnerText;
                    objWeapon.Mode = "0";
                    objWeapon.RC = "0";
                    objWeapon.Concealability = 0;
                    objWeapon.Avail = "0";
                    objWeapon.Cost = 0;
                    objWeapon.UseSkill = objXmlNaturalWeapon["useskill"].InnerText;
                    objWeapon.Source = objXmlNaturalWeapon["source"].InnerText;
                    objWeapon.Page = objXmlNaturalWeapon["page"].InnerText;

                    _objCharacter.Weapons.Add(objWeapon);
                }

                if (cboCategory.SelectedValue.ToString() == "Spirits")
                {
                    if (charNode["optionalpowers"] != null)
                    {
                        //For every 3 full points of Force a spirit has, it may gain one Optional Power.
                        for (int i = intForce -3; i >= 0; i -= 3)
                        {
                            XmlDocument objDummyDocument = new XmlDocument();
                            XmlNode bonusNode = objDummyDocument.CreateNode(XmlNodeType.Element, "bonus", null);
                            objDummyDocument.AppendChild(bonusNode);
                            XmlNode powerNode = objDummyDocument.ImportNode(charNode["optionalpowers"].CloneNode(true),true);
                            objDummyDocument.ImportNode(powerNode, true);
                            bonusNode.AppendChild(powerNode);
                            ImprovementManager.CreateImprovements(_objCharacter, Improvement.ImprovementSource.Metatype, lstMetatypes.SelectedValue.ToString(), bonusNode, false, 1, lstMetatypes.SelectedValue.ToString());
                        }
                    }
                    //If this is a Blood Spirit, add their free Critter Powers.
                    if (chkBloodSpirit.Checked)
                    {
                        XmlNode objXmlCritterPower;
                        TreeNode objNode;
                        CritterPower objPower;
                        bool blnAddPower = _objCharacter.CritterPowers.All(objFindPower => objFindPower.Name != "Energy Drain");

                        //Energy Drain.
                        if (blnAddPower)
                        {
                            objXmlCritterPower = objXmlDocument.SelectSingleNode("/chummer/powers/power[name = \"Energy Drain\"]");
                            objNode = new TreeNode();
                            objPower = new CritterPower(_objCharacter);
                            objPower.Create(objXmlCritterPower, objNode, 0, string.Empty);
                            objPower.CountTowardsLimit = false;
                            _objCharacter.CritterPowers.Add(objPower);
                        }

                        //Fear.
                        blnAddPower = _objCharacter.CritterPowers.All(objFindPower => objFindPower.Name != "Fear");
                        if (blnAddPower)
                        {
                            objXmlCritterPower = objXmlDocument.SelectSingleNode("/chummer/powers/power[name = \"Fear\"]");
                            objNode = new TreeNode();
                            objPower = new CritterPower(_objCharacter);
                            objPower.Create(objXmlCritterPower, objNode, 0, string.Empty);
                            objPower.CountTowardsLimit = false;
                            _objCharacter.CritterPowers.Add(objPower);
                        }

                        //Natural Weapon.
                        objXmlCritterPower = objXmlDocument.SelectSingleNode("/chummer/powers/power[name = \"Natural Weapon\"]");
                        objNode = new TreeNode();
                        objPower = new CritterPower(_objCharacter);
                        objPower.Create(objXmlCritterPower, objNode, 0, "DV " + intForce.ToString() + "P, AP 0");
                        objPower.CountTowardsLimit = false;
                        _objCharacter.CritterPowers.Add(objPower);

                        //Evanescence.
                        blnAddPower = _objCharacter.CritterPowers.All(objFindPower => objFindPower.Name != "Evanescence");
                        if (blnAddPower)
                        {
                            objXmlCritterPower = objXmlDocument.SelectSingleNode("/chummer/powers/power[name = \"Evanescence\"]");
                            objNode = new TreeNode();
                            objPower = new CritterPower(_objCharacter);
                            objPower.Create(objXmlCritterPower, objNode, 0, string.Empty);
                            objPower.CountTowardsLimit = false;
                            _objCharacter.CritterPowers.Add(objPower);
                        }
                    }

                    //Remove the Critter's Materialization Power if they have it. Add the Possession or Inhabitation Power if the Possession-based Tradition checkbox is checked.
                    if (chkPossessionBased.Checked)
                    {
                        foreach (
                            CritterPower objCritterPower in
                                _objCharacter.CritterPowers.Where(objCritterPower => objCritterPower.Name == "Materialization"))
                        {
                            _objCharacter.CritterPowers.Remove(objCritterPower);
                            break;
                        }

                        //Add the selected Power.
                        XmlNode objXmlCritterPower =
                            objXmlDocument.SelectSingleNode("/chummer/powers/power[name = \"" +
                                                            cboPossessionMethod.SelectedValue.ToString() + "\"]");
                        TreeNode objNode = new TreeNode();
                        CritterPower objPower = new CritterPower(_objCharacter);
                        objPower.Create(objXmlCritterPower, objNode, 0, string.Empty);
                        objPower.CountTowardsLimit = false;
                        _objCharacter.CritterPowers.Add(objPower);
                    }
                }
                
                // Add any created Weapons to the character.
                foreach (Weapon objWeapon in objWeapons)
                    _objCharacter.Weapons.Add(objWeapon);

                XmlDocument objSkillDocument = XmlManager.Load("skills.xml");
                //Set the Active Skill Ratings for the Critter.
                foreach (XmlNode objXmlSkill in charNode.SelectNodes("skills/skill"))
                {
                    XmlNode objXmlSkillNode = objSkillDocument.SelectSingleNode("/chummer/skills/skill[name = \"" + objXmlSkill.InnerText + "\"]");
                    Skill.FromData(objXmlSkillNode, _objCharacter);
                    Skill objSkill = _objCharacter.SkillsSection.GetActiveSkill(objXmlSkill.InnerText);
                    if (objSkill != null)
                        objSkill.Karma = objXmlSkill.Attributes["rating"].InnerText == "F" ? intForce : Convert.ToInt32(objXmlSkill.Attributes["rating"].InnerText);
                }
                //Set the Skill Group Ratings for the Critter.
                foreach (XmlNode objXmlSkill in charNode.SelectNodes("skills/group"))
                {
                    foreach (SkillGroup objSkill in _objCharacter.SkillsSection.SkillGroups.Where(objSkill => objSkill.Name == objXmlSkill.InnerText))
                    {
                        if (objXmlSkill.Attributes?["rating"]?.InnerText != null)
                        {
                            objSkill.Karma = objXmlSkill.Attributes["rating"].InnerText == "F" ? intForce : Convert.ToInt32(objXmlSkill.Attributes["rating"].InnerText);
                        }
                    }
                }

                //Set the Knowledge Skill Ratings for the Critter.
                foreach (XmlNode objXmlSkill in charNode.SelectNodes("skills/knowledge"))
                {
                    XmlNode objXmlSkillNode = objSkillDocument.SelectSingleNode("/chummer/knowledgeskills/skill[name = \"" + objXmlSkill.InnerText + "\"]");
                    if (_objCharacter.SkillsSection.KnowledgeSkills.Any(objSkill => objSkill.Name == objXmlSkill.InnerText))
                    {
                        foreach (
                            KnowledgeSkill objSkill in
                            _objCharacter.SkillsSection.KnowledgeSkills.Where(objSkill => objSkill.Name == objXmlSkill.InnerText))
                        {
                            if (objXmlSkill.Attributes?["rating"]?.InnerText != null)
                            {
                                objSkill.Karma = objXmlSkill.Attributes["rating"].InnerText == "F"
                                    ? intForce
                                    : Convert.ToInt32(objXmlSkill.Attributes["rating"].InnerText);
                            }
                        }
                    }
                    else
                    {
                        if (objXmlSkillNode != null)
                        {
                            KnowledgeSkill objSkill = Skill.FromData(objXmlSkillNode, _objCharacter) as KnowledgeSkill;
                            if (objXmlSkill.Attributes?["rating"]?.InnerText != null)
                            {
                                objSkill.Karma = objXmlSkill.Attributes["rating"].InnerText == "F"
                                    ? intForce
                                    : Convert.ToInt32(objXmlSkill.Attributes["rating"].InnerText);
                            }
                            _objCharacter.SkillsSection.KnowledgeSkills.Add(objSkill);
                        }
                        else
                        {
                            var objSkill = new KnowledgeSkill(_objCharacter, objXmlSkill.InnerText)
                            {
                                Type = objXmlSkill.Attributes?["category"]?.InnerText
                            };

                            if (objXmlSkill.Attributes?["rating"]?.InnerText != null)
                            {
                                objSkill.Karma = objXmlSkill.Attributes["rating"].InnerText == "F"
                                    ? intForce
                                    : Convert.ToInt32(objXmlSkill.Attributes["rating"].InnerText);
                            }
                            _objCharacter.SkillsSection.KnowledgeSkills.Add(objSkill);
                        }
                    }
                }

                // Add any Complex Forms the Critter comes with (typically Sprites)
                XmlDocument objXmlProgramDocument = XmlManager.Load("complexforms.xml");
                foreach (XmlNode objXmlComplexForm in charNode.SelectNodes("complexforms/complexform"))
                {
                    string strForceValue = string.Empty;
                    if (objXmlComplexForm.Attributes["select"] != null)
                        strForceValue = objXmlComplexForm.Attributes["select"].InnerText;
                    XmlNode objXmlProgram = objXmlProgramDocument.SelectSingleNode("/chummer/complexforms/complexform[name = \"" + objXmlComplexForm.InnerText + "\"]");
                    TreeNode objNode = new TreeNode();
                    ComplexForm objProgram = new ComplexForm(_objCharacter);
                    objProgram.Create(objXmlProgram, objNode, strForceValue);
                    _objCharacter.ComplexForms.Add(objProgram);
                }

                // Add any Advanced Programs the Critter comes with (typically Sprites)
                XmlDocument objXmlAIProgramDocument = XmlManager.Load("programs.xml");
                foreach (XmlNode objXmlAIProgram in charNode.SelectNodes("programs/program"))
                {
                    string strForceValue = string.Empty;
                    if (objXmlAIProgram.Attributes["select"] != null)
                        strForceValue = objXmlAIProgram.Attributes["select"].InnerText;
                    XmlNode objXmlProgram = objXmlAIProgramDocument.SelectSingleNode("/chummer/programs/program[name = \"" + objXmlAIProgram.InnerText + "\"]");
                    if (objXmlProgram != null)
                    {
                        TreeNode objNode = new TreeNode();
                        AIProgram objProgram = new AIProgram(_objCharacter);
                        objProgram.Create(objXmlProgram, objNode, objXmlProgram["category"]?.InnerText == "Advanced Programs", strForceValue);
                        _objCharacter.AIPrograms.Add(objProgram);
                    }
                }

                // Add any Gear the Critter comes with (typically Programs for A.I.s)
                XmlDocument objXmlGearDocument = XmlManager.Load("gear.xml");
                foreach (XmlNode objXmlGear in charNode.SelectNodes("gears/gear"))
                {
                    int intRating = 0;
                    if (objXmlGear.Attributes["rating"] != null)
                        intRating = Convert.ToInt32(ExpressionToString(objXmlGear.Attributes["rating"].InnerText, decimal.ToInt32(nudForce.Value), 0));
                    string strForceValue = string.Empty;
                    if (objXmlGear.Attributes["select"] != null)
                        strForceValue = objXmlGear.Attributes["select"].InnerText;
                    XmlNode objXmlGearItem = objXmlGearDocument.SelectSingleNode("/chummer/gears/gear[name = \"" + objXmlGear.InnerText + "\"]");
                    TreeNode objNode = new TreeNode();
                    Gear objGear = new Gear(_objCharacter);
                    List<Weapon> lstWeapons = new List<Weapon>();
                    List<TreeNode> lstWeaponNodes = new List<TreeNode>();
                    objGear.Create(objXmlGearItem, objNode, intRating, lstWeapons, lstWeaponNodes, strForceValue);
                    objGear.Cost = "0";
                    _objCharacter.Gear.Add(objGear);
                }

                // Sprites can never have Physical Attributes
                if (_objCharacter.DEPEnabled || lstMetatypes.SelectedValue.ToString().EndsWith("Sprite"))
                {
                    _objCharacter.BOD.AssignLimits("0", "0", "0");
                    _objCharacter.AGI.AssignLimits("0", "0", "0");
                    _objCharacter.REA.AssignLimits("0", "0", "0");
                    _objCharacter.STR.AssignLimits("0", "0", "0");
                    _objCharacter.MAG.AssignLimits("0", "0", "0");
                    _objCharacter.MAGAdept.AssignLimits("0", "0", "0");
                }

                /*
                int x;
                int.TryParse(lblBP.Text, out x);
                _objCharacter.BuildKarma = _objCharacter.BuildKarma - x;
                */

                DialogResult = DialogResult.OK;
                Close();
            }
            else
            {
                MessageBox.Show(LanguageManager.GetString("Message_Metatype_SelectMetatype"), LanguageManager.GetString("MessageTitle_Metatype_SelectMetatype"), MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

        }

        /// <summary>
        /// Convert Force, 1D6, or 2D6 into a usable value.
        /// </summary>
        /// <param name="strIn">Expression to convert.</param>
        /// <param name="intForce">Force value to use.</param>
        /// <param name="intOffset">Dice offset.</param>
        /// <returns></returns>
        private string ExpressionToString(string strIn, int intForce, int intOffset)
        {
            if (string.IsNullOrWhiteSpace(strIn))
                return intOffset.ToString();
            int intValue = 1;
            string strForce = intForce.ToString();
            // This statement is wrapped in a try/catch since trying 1 div 2 results in an error with XSLT.
            try
            {
                intValue = Convert.ToInt32(Math.Ceiling((double)CommonFunctions.EvaluateInvariantXPath(strIn.Replace("/", " div ").Replace("F", strForce).Replace("1D6", strForce).Replace("2D6", strForce))));
            }
            catch (XPathException) { }
            catch (OverflowException) { } // Result is text and not a double
            catch (InvalidCastException) { }

            intValue += intOffset;
            if (intForce > 0)
            {
                if (intValue < 1)
                    intValue = 1;
            }
            else if (intValue < 0)
                intValue = 0;
            return intValue.ToString();
        }

        /// <summary>
        /// Populate the list of Metatypes.
        /// </summary>
        void PopulateMetatypes()
        {
            XmlDocument objXmlDocument = XmlManager.Load(_strXmlFile);
            List<ListItem> lstMetatype = new List<ListItem>();

            XmlNodeList objXmlMetatypeList = objXmlDocument.SelectNodes("/chummer/metatypes/metatype[(" + _objCharacter.Options.BookXPath() + ") and category = \"" + cboCategory.SelectedValue + "\"]");

            foreach (XmlNode objXmlMetatype in objXmlMetatypeList)
            {
                ListItem objItem = new ListItem();
                objItem.Value = objXmlMetatype["name"]?.InnerText;
                objItem.Name = objXmlMetatype["translate"]?.InnerText ?? objItem.Value;
                lstMetatype.Add(objItem);
            }
            SortListItem objSort = new SortListItem();
            lstMetatype.Sort(objSort.Compare);
            lstMetatypes.BeginUpdate();
            lstMetatypes.DataSource = null;
            lstMetatypes.ValueMember = "Value";
            lstMetatypes.DisplayMember = "Name";
            lstMetatypes.DataSource = lstMetatype;
            lstMetatypes.SelectedIndex = -1;
            lstMetatypes.EndUpdate();

            if (cboCategory.SelectedValue.ToString().EndsWith("Spirits"))
            {
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

        private void chkPossessionBased_CheckedChanged(object sender, EventArgs e)
        {
            cboPossessionMethod.Enabled = chkPossessionBased.Checked;
        }
        #endregion

    }
}
