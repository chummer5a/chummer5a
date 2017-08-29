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
            LanguageManager.Instance.Load(GlobalOptions.Instance.Language, this);

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
            XmlDocument objXmlDocument = XmlManager.Instance.Load(_strXmlFile);

            // Populate the Metatype Category list.
            XmlNodeList objXmlCategoryList = objXmlDocument.SelectNodes("/chummer/categories/category");

            // Create a list of any Categories that should not be in the list.
            List<string> lstRemoveCategory = new List<string>();
            foreach (XmlNode objXmlCategory in objXmlCategoryList)
            {
                bool blnRemoveItem = true;

                string strXPath = "/chummer/metatypes/metatype[category = \"" + objXmlCategory.InnerText + "\" and (" + _objCharacter.Options.BookXPath() + ")]";

                XmlNodeList objItems = objXmlDocument.SelectNodes(strXPath);
                if (objItems.Count > 0)
                    blnRemoveItem = false;

                if (blnRemoveItem)
                    lstRemoveCategory.Add(objXmlCategory.InnerText);
            }

            foreach (XmlNode objXmlCategory in objXmlCategoryList)
            {
                // Make sure the Category isn't in the exclusion list.
                bool blnAddItem = true;
                foreach (string strCategory in lstRemoveCategory)
                {
                    if (strCategory == objXmlCategory.InnerText)
                        blnAddItem = false;
                }
                // Also make sure it is not already in the Category list.
                foreach (ListItem objItem in _lstCategory)
                {
                    if (objItem.Value == objXmlCategory.InnerText)
                        blnAddItem = false;
                }

                if (blnAddItem)
                {
                    ListItem objItem = new ListItem();
                    objItem.Value = objXmlCategory.InnerText;
                    objItem.Name = objXmlCategory.Attributes?["translate"]?.InnerText ?? objXmlCategory.InnerText;
                    _lstCategory.Add(objItem);
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
            tipTooltip.SetToolTip(chkPossessionBased, LanguageManager.Instance.GetString("Tip_Metatype_PossessionTradition"));
            tipTooltip.SetToolTip(chkBloodSpirit, LanguageManager.Instance.GetString("Tip_Metatype_BloodSpirit"));

            objXmlDocument = XmlManager.Instance.Load("critterpowers.xml");
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
                XmlDocument objXmlDocument = XmlManager.Instance.Load(_strXmlFile);
                XmlDocument objXmlQualityDocument = XmlManager.Instance.Load("qualities.xml");
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
                objNone.Name = LanguageManager.Instance.GetString("String_None");
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
                        lblForceLabel.Text = LanguageManager.Instance.GetString("String_Force");
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
                            if (GlobalOptions.Instance.Language != "en-us")
                            {
                                XmlNode objQuality = objXmlQualityDocument.SelectSingleNode("/chummer/qualities/quality[name = \"" + objXmlQuality.InnerText + "\"]");
                            strQualities += objQuality["translate"]?.InnerText ?? objXmlQuality.InnerText;

                            if (!string.IsNullOrEmpty(objXmlQuality.Attributes["select"]?.InnerText))
                                    strQualities += " (" + LanguageManager.Instance.TranslateExtra(objXmlQuality.Attributes["select"].InnerText) + ")";
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
                            if (GlobalOptions.Instance.Language != "en-us")
                            {
                                XmlNode objQuality = objXmlQualityDocument.SelectSingleNode("/chummer/qualities/quality[name = \"" + objXmlQuality.InnerText + "\"]");
                            strQualities += objQuality["translate"]?.InnerText ?? objXmlQuality.InnerText;

                            if (!string.IsNullOrEmpty(objXmlQuality.Attributes["select"]?.InnerText))
                                    strQualities += " (" + LanguageManager.Instance.TranslateExtra(objXmlQuality.Attributes["select"].InnerText) + ")";
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
                objNone.Name = LanguageManager.Instance.GetString("String_None");
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
            XmlDocument objXmlDocument = XmlManager.Instance.Load(_strXmlFile);
            XmlDocument objXmlQualityDocument = XmlManager.Instance.Load("qualities.xml");

            if (cboMetavariant.SelectedValue.ToString() != "None")
            {
                XmlNode objXmlMetavariant = objXmlDocument.SelectSingleNode("/chummer/metatypes/metatype[name = \"" + lstMetatypes.SelectedValue + "\"]/metavariants/metavariant[name = \"" + cboMetavariant.SelectedValue + "\"]");
                XmlNode objXmlMetatype = objXmlDocument.SelectSingleNode("/chummer/metatypes/metatype[name = \"" + lstMetatypes.SelectedValue + "\"]");
                lblBP.Text = objXmlMetavariant["karma"].InnerText;
                if (objXmlMetatype?["forcecreature"] == null)
                {
                    lblBOD.Text = string.Format("{0}/{1} ({2})",
                        objXmlMetavariant["bodmin"]?.InnerText ?? objXmlMetatype["bodmin"]?.InnerText,
                        objXmlMetavariant["bodmax"]?.InnerText ?? objXmlMetatype["bodmax"]?.InnerText,
                        objXmlMetavariant["bodaug"]?.InnerText ?? objXmlMetatype["bodaug"]?.InnerText);
                    lblAGI.Text = string.Format("{0}/{1} ({2})",
                        objXmlMetavariant["agimin"]?.InnerText ?? objXmlMetatype["agimin"]?.InnerText,
                        objXmlMetavariant["agimax"]?.InnerText ?? objXmlMetatype["agimax"]?.InnerText,
                        objXmlMetavariant["agiaug"]?.InnerText ?? objXmlMetatype["agiaug"]?.InnerText);
                    lblREA.Text = string.Format("{0}/{1} ({2})",
                        objXmlMetavariant["reamin"]?.InnerText ?? objXmlMetatype["reamin"]?.InnerText,
                        objXmlMetavariant["reamax"]?.InnerText ?? objXmlMetatype["reamax"]?.InnerText,
                        objXmlMetavariant["reaaug"]?.InnerText ?? objXmlMetatype["reaaug"]?.InnerText);
                    lblSTR.Text = string.Format("{0}/{1} ({2})",
                        objXmlMetavariant["strmin"]?.InnerText ?? objXmlMetatype["strmin"]?.InnerText,
                        objXmlMetavariant["strmax"]?.InnerText ?? objXmlMetatype["strmax"]?.InnerText,
                        objXmlMetavariant["straug"]?.InnerText ?? objXmlMetatype["straug"]?.InnerText);
                    lblCHA.Text = string.Format("{0}/{1} ({2})",
                        objXmlMetavariant["chamin"]?.InnerText ?? objXmlMetatype["chamin"]?.InnerText,
                        objXmlMetavariant["chamax"]?.InnerText ?? objXmlMetatype["chamax"]?.InnerText,
                        objXmlMetavariant["chaaug"]?.InnerText ?? objXmlMetatype["chaaug"]?.InnerText);
                    lblINT.Text = string.Format("{0}/{1} ({2})",
                        objXmlMetavariant["intmin"]?.InnerText ?? objXmlMetatype["intmin"]?.InnerText,
                        objXmlMetavariant["intmax"]?.InnerText ?? objXmlMetatype["intmax"]?.InnerText,
                        objXmlMetavariant["intaug"]?.InnerText ?? objXmlMetatype["intaug"]?.InnerText);
                    lblLOG.Text = string.Format("{0}/{1} ({2})",
                        objXmlMetavariant["logmin"]?.InnerText ?? objXmlMetatype["logmin"]?.InnerText,
                        objXmlMetavariant["logmax"]?.InnerText ?? objXmlMetatype["logmax"]?.InnerText,
                        objXmlMetavariant["logaug"]?.InnerText ?? objXmlMetatype["logaug"]?.InnerText);
                    lblWIL.Text = string.Format("{0}/{1} ({2})",
                        objXmlMetavariant["wilmin"]?.InnerText ?? objXmlMetatype["wilmin"]?.InnerText,
                        objXmlMetavariant["wilmax"]?.InnerText ?? objXmlMetatype["wilmax"]?.InnerText,
                        objXmlMetavariant["wilaug"]?.InnerText ?? objXmlMetatype["wilaug"]?.InnerText);
                    lblINI.Text = string.Format("{0}/{1} ({2})",
                        objXmlMetavariant["inimin"]?.InnerText ?? objXmlMetatype["inimin"]?.InnerText,
                        objXmlMetavariant["inimax"]?.InnerText ?? objXmlMetatype["inimax"]?.InnerText,
                        objXmlMetavariant["iniaug"]?.InnerText ?? objXmlMetatype["iniaug"]?.InnerText);
                }
                else
                {
                    lblBOD.Text = objXmlMetavariant["bodmin"]?.InnerText ?? objXmlMetatype["bodmin"]?.InnerText;
                    lblAGI.Text = objXmlMetavariant["agimin"]?.InnerText ?? objXmlMetatype["agimin"]?.InnerText;
                    lblREA.Text = objXmlMetavariant["reamin"]?.InnerText ?? objXmlMetatype["reamin"]?.InnerText;
                    lblSTR.Text = objXmlMetavariant["strmin"]?.InnerText ?? objXmlMetatype["strmin"]?.InnerText;
                    lblCHA.Text = objXmlMetavariant["chamin"]?.InnerText ?? objXmlMetatype["chamin"]?.InnerText;
                    lblINT.Text = objXmlMetavariant["intmin"]?.InnerText ?? objXmlMetatype["intmin"]?.InnerText;
                    lblLOG.Text = objXmlMetavariant["logmin"]?.InnerText ?? objXmlMetatype["logmin"]?.InnerText;
                    lblWIL.Text = objXmlMetavariant["wilmin"]?.InnerText ?? objXmlMetatype["wilmin"]?.InnerText;
                    lblINI.Text = objXmlMetavariant["inimin"]?.InnerText ?? objXmlMetatype["inimin"]?.InnerText;
                }

                string strQualities = string.Empty;
                // Build a list of the Metavariant's Positive Qualities.
                foreach (XmlNode objXmlQuality in objXmlMetavariant.SelectNodes("qualities/positive/quality"))
                {
                        if (GlobalOptions.Instance.Language != "en-us")
                        {
                            XmlNode objQuality = objXmlQualityDocument.SelectSingleNode("/chummer/qualities/quality[name = \"" + objXmlQuality.InnerText + "\"]");
                            strQualities += objQuality["translate"]?.InnerText ?? objXmlQuality.InnerText;

                            if (!string.IsNullOrEmpty(objXmlQuality.Attributes["select"]?.InnerText))
                                strQualities += " (" + LanguageManager.Instance.TranslateExtra(objXmlQuality.Attributes["select"].InnerText) + ")";
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
                foreach (XmlNode objXmlQuality in objXmlMetavariant.SelectNodes("qualities/negative/quality"))
                {
                        if (GlobalOptions.Instance.Language != "en-us")
                        {
                            XmlNode objQuality = objXmlQualityDocument.SelectSingleNode("/chummer/qualities/quality[name = \"" + objXmlQuality.InnerText + "\"]");
                            strQualities += objQuality["translate"]?.InnerText ?? objXmlQuality.InnerText;

                            if (!string.IsNullOrEmpty(objXmlQuality.Attributes["select"]?.InnerText))
                                strQualities += " (" + LanguageManager.Instance.TranslateExtra(objXmlQuality.Attributes["select"].InnerText) + ")";
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
            else if (lstMetatypes.SelectedValue != null)
            {
                XmlNode objXmlMetatype = objXmlDocument.SelectSingleNode("/chummer/metatypes/metatype[name = \"" + lstMetatypes.SelectedValue + "\"]");
                string strQualities = string.Empty;
                // Build a list of the Metavariant's Positive Qualities.
                foreach (XmlNode objXmlQuality in objXmlMetatype.SelectNodes("qualities/positive/quality"))
                {
                        if (GlobalOptions.Instance.Language != "en-us")
                        {
                            XmlNode objQuality = objXmlQualityDocument.SelectSingleNode("/chummer/qualities/quality[name = \"" + objXmlQuality.InnerText + "\"]");
                            strQualities += objQuality["translate"]?.InnerText ?? objXmlQuality.InnerText;

                            if (!string.IsNullOrEmpty(objXmlQuality.Attributes["select"]?.InnerText))
                                strQualities += " (" + LanguageManager.Instance.TranslateExtra(objXmlQuality.Attributes["select"].InnerText) + ")";
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
                        if (GlobalOptions.Instance.Language != "en-us")
                        {
                            XmlNode objQuality = objXmlQualityDocument.SelectSingleNode("/chummer/qualities/quality[name = \"" + objXmlQuality.InnerText + "\"]");
                        strQualities += objQuality["translate"]?.InnerText ?? objXmlQuality.InnerText;

                        if (!string.IsNullOrEmpty(objXmlQuality.Attributes["select"]?.InnerText))
                                strQualities += " (" + LanguageManager.Instance.TranslateExtra(objXmlQuality.Attributes["select"].InnerText) + ")";
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
                lblBP.Text = objXmlMetatype["karma"].InnerText;
            }
            else
            {
                lblBP.Text = "0";
                lblQualities.Text = "None";
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
                ImprovementManager objImprovementManager = new ImprovementManager(_objCharacter);
                XmlDocument objXmlDocument = XmlManager.Instance.Load(_strXmlFile);
                XmlNode objXmlMetatype = objXmlDocument.SelectSingleNode("/chummer/metatypes/metatype[name = \"" + lstMetatypes.SelectedValue + "\"]");
                XmlNode objXmlMetavariant = objXmlDocument.SelectSingleNode("/chummer/metatypes/metatype[name = \"" + lstMetatypes.SelectedValue + "\"]/metavariants/metavariant[name = \"" + cboMetavariant.SelectedValue + "\"]");
                int intForce = 0;
                if (nudForce.Visible)
                    intForce = Convert.ToInt32(nudForce.Value);

                // Set Metatype information.

                if (objXmlMetavariant != null && cboMetavariant.SelectedValue.ToString() != "None")
                {
                    _objCharacter.BOD.AssignLimits(ExpressionToString(objXmlMetavariant["bodmin"]?.InnerText, intForce, 0), ExpressionToString(objXmlMetavariant["bodmax"]?.InnerText, intForce, 0), ExpressionToString(objXmlMetavariant["bodaug"]?.InnerText, intForce, 0));
                    _objCharacter.AGI.AssignLimits(ExpressionToString(objXmlMetavariant["agimin"]?.InnerText, intForce, 0), ExpressionToString(objXmlMetavariant["agimax"]?.InnerText, intForce, 0), ExpressionToString(objXmlMetavariant["agiaug"]?.InnerText, intForce, 0));
                    _objCharacter.REA.AssignLimits(ExpressionToString(objXmlMetavariant["reamin"]?.InnerText, intForce, 0), ExpressionToString(objXmlMetavariant["reamax"]?.InnerText, intForce, 0), ExpressionToString(objXmlMetavariant["reaaug"]?.InnerText, intForce, 0));
                    _objCharacter.STR.AssignLimits(ExpressionToString(objXmlMetavariant["strmin"]?.InnerText, intForce, 0), ExpressionToString(objXmlMetavariant["strmax"]?.InnerText, intForce, 0), ExpressionToString(objXmlMetavariant["straug"]?.InnerText, intForce, 0));
                    _objCharacter.CHA.AssignLimits(ExpressionToString(objXmlMetavariant["chamin"]?.InnerText, intForce, 0), ExpressionToString(objXmlMetavariant["chamax"]?.InnerText, intForce, 0), ExpressionToString(objXmlMetavariant["chaaug"]?.InnerText, intForce, 0));
                    _objCharacter.INT.AssignLimits(ExpressionToString(objXmlMetavariant["intmin"]?.InnerText, intForce, 0), ExpressionToString(objXmlMetavariant["intmax"]?.InnerText, intForce, 0), ExpressionToString(objXmlMetavariant["intaug"]?.InnerText, intForce, 0));
                    _objCharacter.LOG.AssignLimits(ExpressionToString(objXmlMetavariant["logmin"]?.InnerText, intForce, 0), ExpressionToString(objXmlMetavariant["logmax"]?.InnerText, intForce, 0), ExpressionToString(objXmlMetavariant["logaug"]?.InnerText, intForce, 0));
                    _objCharacter.WIL.AssignLimits(ExpressionToString(objXmlMetavariant["wilmin"]?.InnerText, intForce, 0), ExpressionToString(objXmlMetavariant["wilmax"]?.InnerText, intForce, 0), ExpressionToString(objXmlMetavariant["wilaug"]?.InnerText, intForce, 0));
                    _objCharacter.MAG.AssignLimits(ExpressionToString(objXmlMetavariant["magmin"]?.InnerText, intForce, 0), ExpressionToString(objXmlMetavariant["magmax"]?.InnerText, intForce, 0), ExpressionToString(objXmlMetavariant["magaug"]?.InnerText, intForce, 0));
                    _objCharacter.RES.AssignLimits(ExpressionToString(objXmlMetavariant["resmin"]?.InnerText, intForce, 0), ExpressionToString(objXmlMetavariant["resmax"]?.InnerText, intForce, 0), ExpressionToString(objXmlMetavariant["resaug"]?.InnerText, intForce, 0));
                    _objCharacter.EDG.AssignLimits(ExpressionToString(objXmlMetavariant["edgmin"]?.InnerText, intForce, 0), ExpressionToString(objXmlMetavariant["edgmax"]?.InnerText, intForce, 0), ExpressionToString(objXmlMetavariant["edgaug"]?.InnerText, intForce, 0));
                    _objCharacter.ESS.AssignLimits(ExpressionToString(objXmlMetavariant["essmin"]?.InnerText, intForce, 0), ExpressionToString(objXmlMetavariant["essmax"]?.InnerText, intForce, 0), ExpressionToString(objXmlMetavariant["essaug"]?.InnerText, intForce, 0));
                    _objCharacter.DEP.AssignLimits(ExpressionToString(objXmlMetavariant["depmin"]?.InnerText, intForce, 0), ExpressionToString(objXmlMetavariant["depmax"]?.InnerText, intForce, 0), ExpressionToString(objXmlMetavariant["depaug"]?.InnerText, intForce, 0));
                    }
                else if (objXmlMetatype != null && (_strXmlFile != "critters.xml" || lstMetatypes.SelectedValue.ToString() == "Ally Spirit"))
                {
                    _objCharacter.BOD.AssignLimits(ExpressionToString(objXmlMetatype["bodmin"]?.InnerText, intForce, 0), ExpressionToString(objXmlMetatype["bodmax"]?.InnerText, intForce, 0), ExpressionToString(objXmlMetatype["bodaug"]?.InnerText, intForce, 0));
                    _objCharacter.AGI.AssignLimits(ExpressionToString(objXmlMetatype["agimin"]?.InnerText, intForce, 0), ExpressionToString(objXmlMetatype["agimax"]?.InnerText, intForce, 0), ExpressionToString(objXmlMetatype["agiaug"]?.InnerText, intForce, 0));
                    _objCharacter.REA.AssignLimits(ExpressionToString(objXmlMetatype["reamin"]?.InnerText, intForce, 0), ExpressionToString(objXmlMetatype["reamax"]?.InnerText, intForce, 0), ExpressionToString(objXmlMetatype["reaaug"]?.InnerText, intForce, 0));
                    _objCharacter.STR.AssignLimits(ExpressionToString(objXmlMetatype["strmin"]?.InnerText, intForce, 0), ExpressionToString(objXmlMetatype["strmax"]?.InnerText, intForce, 0), ExpressionToString(objXmlMetatype["straug"]?.InnerText, intForce, 0));
                    _objCharacter.CHA.AssignLimits(ExpressionToString(objXmlMetatype["chamin"]?.InnerText, intForce, 0), ExpressionToString(objXmlMetatype["chamax"]?.InnerText, intForce, 0), ExpressionToString(objXmlMetatype["chaaug"]?.InnerText, intForce, 0));
                    _objCharacter.INT.AssignLimits(ExpressionToString(objXmlMetatype["intmin"]?.InnerText, intForce, 0), ExpressionToString(objXmlMetatype["intmax"]?.InnerText, intForce, 0), ExpressionToString(objXmlMetatype["intaug"]?.InnerText, intForce, 0));
                    _objCharacter.LOG.AssignLimits(ExpressionToString(objXmlMetatype["logmin"]?.InnerText, intForce, 0), ExpressionToString(objXmlMetatype["logmax"]?.InnerText, intForce, 0), ExpressionToString(objXmlMetatype["logaug"]?.InnerText, intForce, 0));
                    _objCharacter.WIL.AssignLimits(ExpressionToString(objXmlMetatype["wilmin"]?.InnerText, intForce, 0), ExpressionToString(objXmlMetatype["wilmax"]?.InnerText, intForce, 0), ExpressionToString(objXmlMetatype["wilaug"]?.InnerText, intForce, 0));
                    _objCharacter.MAG.AssignLimits(ExpressionToString(objXmlMetatype["magmin"]?.InnerText, intForce, 0), ExpressionToString(objXmlMetatype["magmax"]?.InnerText, intForce, 0), ExpressionToString(objXmlMetatype["magaug"]?.InnerText, intForce, 0));
                    _objCharacter.RES.AssignLimits(ExpressionToString(objXmlMetatype["resmin"]?.InnerText, intForce, 0), ExpressionToString(objXmlMetatype["resmax"]?.InnerText, intForce, 0), ExpressionToString(objXmlMetatype["resaug"]?.InnerText, intForce, 0));
                    _objCharacter.EDG.AssignLimits(ExpressionToString(objXmlMetatype["edgmin"]?.InnerText, intForce, 0), ExpressionToString(objXmlMetatype["edgmax"]?.InnerText, intForce, 0), ExpressionToString(objXmlMetatype["edgaug"]?.InnerText, intForce, 0));
                    _objCharacter.ESS.AssignLimits(ExpressionToString(objXmlMetatype["essmin"]?.InnerText, intForce, 0), ExpressionToString(objXmlMetatype["essmax"]?.InnerText, intForce, 0), ExpressionToString(objXmlMetatype["essaug"]?.InnerText, intForce, 0));
                    _objCharacter.DEP.AssignLimits(ExpressionToString(objXmlMetatype["depmin"]?.InnerText, intForce, 0), ExpressionToString(objXmlMetatype["depmax"]?.InnerText, intForce, 0), ExpressionToString(objXmlMetatype["depaug"]?.InnerText, intForce, 0));
                    }
                else if (objXmlMetatype != null)
                {
                    int intMinModifier = -3;
                    int intMaxModifier = 3;
                    if (cboCategory.SelectedValue.ToString() == "Technocritters")
                    {
                        intMinModifier = -1;
                        intMaxModifier = 1;
                    }
                    _objCharacter.BOD.AssignLimits(ExpressionToString(objXmlMetatype["bodmin"]?.InnerText, intForce, intMinModifier), ExpressionToString(objXmlMetatype["bodmin"]?.InnerText, intForce, intMaxModifier), ExpressionToString(objXmlMetatype["bodmin"]?.InnerText, intForce, intMaxModifier));
                    _objCharacter.AGI.AssignLimits(ExpressionToString(objXmlMetatype["agimin"]?.InnerText, intForce, intMinModifier), ExpressionToString(objXmlMetatype["agimin"]?.InnerText, intForce, intMaxModifier), ExpressionToString(objXmlMetatype["agimin"]?.InnerText, intForce, intMaxModifier));
                    _objCharacter.REA.AssignLimits(ExpressionToString(objXmlMetatype["reamin"]?.InnerText, intForce, intMinModifier), ExpressionToString(objXmlMetatype["reamin"]?.InnerText, intForce, intMaxModifier), ExpressionToString(objXmlMetatype["reamin"]?.InnerText, intForce, intMaxModifier));
                    _objCharacter.STR.AssignLimits(ExpressionToString(objXmlMetatype["strmin"]?.InnerText, intForce, intMinModifier), ExpressionToString(objXmlMetatype["strmin"]?.InnerText, intForce, intMaxModifier), ExpressionToString(objXmlMetatype["strmin"]?.InnerText, intForce, intMaxModifier));
                    _objCharacter.CHA.AssignLimits(ExpressionToString(objXmlMetatype["chamin"]?.InnerText, intForce, intMinModifier), ExpressionToString(objXmlMetatype["chamin"]?.InnerText, intForce, intMaxModifier), ExpressionToString(objXmlMetatype["chamin"]?.InnerText, intForce, intMaxModifier));
                    _objCharacter.INT.AssignLimits(ExpressionToString(objXmlMetatype["intmin"]?.InnerText, intForce, intMinModifier), ExpressionToString(objXmlMetatype["intmin"]?.InnerText, intForce, intMaxModifier), ExpressionToString(objXmlMetatype["intmin"]?.InnerText, intForce, intMaxModifier));
                    _objCharacter.LOG.AssignLimits(ExpressionToString(objXmlMetatype["logmin"]?.InnerText, intForce, intMinModifier), ExpressionToString(objXmlMetatype["logmin"]?.InnerText, intForce, intMaxModifier), ExpressionToString(objXmlMetatype["logmin"]?.InnerText, intForce, intMaxModifier));
                    _objCharacter.WIL.AssignLimits(ExpressionToString(objXmlMetatype["wilmin"]?.InnerText, intForce, intMinModifier), ExpressionToString(objXmlMetatype["wilmin"]?.InnerText, intForce, intMaxModifier), ExpressionToString(objXmlMetatype["wilmin"]?.InnerText, intForce, intMaxModifier));
                    _objCharacter.MAG.AssignLimits(ExpressionToString(objXmlMetatype["magmin"]?.InnerText, intForce, intMinModifier), ExpressionToString(objXmlMetatype["magmin"]?.InnerText, intForce, intMaxModifier), ExpressionToString(objXmlMetatype["magmin"]?.InnerText, intForce, intMaxModifier));
                    _objCharacter.RES.AssignLimits(ExpressionToString(objXmlMetatype["resmin"]?.InnerText, intForce, intMinModifier), ExpressionToString(objXmlMetatype["resmin"]?.InnerText, intForce, intMaxModifier), ExpressionToString(objXmlMetatype["resmin"]?.InnerText, intForce, intMaxModifier));
                    _objCharacter.EDG.AssignLimits(ExpressionToString(objXmlMetatype["edgmin"]?.InnerText, intForce, intMinModifier), ExpressionToString(objXmlMetatype["edgmin"]?.InnerText, intForce, intMaxModifier), ExpressionToString(objXmlMetatype["edgmin"]?.InnerText, intForce, intMaxModifier));
                    _objCharacter.ESS.AssignLimits(ExpressionToString(objXmlMetatype["essmin"]?.InnerText, intForce, 0), ExpressionToString(objXmlMetatype["essmax"]?.InnerText, intForce, 0), ExpressionToString(objXmlMetatype["essaug"]?.InnerText, intForce, 0));
                    _objCharacter.DEP.AssignLimits(ExpressionToString(objXmlMetatype["depmin"]?.InnerText, intForce, intMinModifier), ExpressionToString(objXmlMetatype["depmin"]?.InnerText, intForce, intMaxModifier), ExpressionToString(objXmlMetatype["depmin"]?.InnerText, intForce, intMaxModifier));
                }

                //TODO: Move this into AttributeSection when I get around to implementing that. This is an ugly hack that shouldn't be necessary, but eh.
                _objCharacter.AttributeList.Clear();
                _objCharacter.SpecialAttributeList.Clear();
                _objCharacter.AttributeList.Add(_objCharacter.BOD);
                _objCharacter.AttributeList.Add(_objCharacter.AGI);
                _objCharacter.AttributeList.Add(_objCharacter.REA);
                _objCharacter.AttributeList.Add(_objCharacter.STR);
                _objCharacter.AttributeList.Add(_objCharacter.CHA);
                _objCharacter.AttributeList.Add(_objCharacter.INT);
                _objCharacter.AttributeList.Add(_objCharacter.LOG);
                _objCharacter.AttributeList.Add(_objCharacter.WIL);
                _objCharacter.SpecialAttributeList.Add(_objCharacter.EDG);
                _objCharacter.SpecialAttributeList.Add(_objCharacter.MAG);
                _objCharacter.SpecialAttributeList.Add(_objCharacter.RES);
                _objCharacter.SpecialAttributeList.Add(_objCharacter.DEP);

                // If this is a Shapeshifter, a Metavariant must be selected. Default to Human if None is selected.
                if (cboCategory.SelectedValue.ToString() == "Shapeshifter" && cboMetavariant.SelectedValue.ToString() == "None")
                    cboMetavariant.SelectedValue = "Human";

                _objCharacter.Metatype = lstMetatypes.SelectedValue.ToString();
                _objCharacter.MetatypeCategory = cboCategory.SelectedValue.ToString();
                if (cboMetavariant.SelectedValue.ToString() == "None")
                {
                    _objCharacter.Metavariant = string.Empty;
                    _objCharacter.MetatypeBP = Convert.ToInt32(lblBP.Text);
                }
                else
                {
                    _objCharacter.Metavariant = cboMetavariant.SelectedValue.ToString();
                    _objCharacter.MetatypeBP = Convert.ToInt32(lblBP.Text);
                }

                if (objXmlMetatype?["movement"] != null)
                    _objCharacter.Movement = objXmlMetatype["movement"].InnerText;
                // Load the Qualities file.
                XmlDocument objXmlQualityDocument = XmlManager.Instance.Load("qualities.xml");

                if (cboMetavariant.SelectedValue.ToString() == "None")
                {
                    // Determine if the Metatype has any bonuses.
                    if (objXmlMetatype?.InnerXml.Contains("bonus") == true)
                        objImprovementManager.CreateImprovements(Improvement.ImprovementSource.Metatype, lstMetatypes.SelectedValue.ToString(), objXmlMetatype.SelectSingleNode("bonus"), false, 1, lstMetatypes.SelectedValue.ToString());

                    // Create the Qualities that come with the Metatype.
                    foreach (XmlNode objXmlQualityItem in objXmlMetatype?.SelectNodes("qualities/positive/quality"))
                    {
                        XmlNode objXmlQuality = objXmlQualityDocument.SelectSingleNode("/chummer/qualities/quality[name = \"" + objXmlQualityItem.InnerText + "\"]");
                        TreeNode objNode = new TreeNode();
                        List<Weapon> objWeapons = new List<Weapon>();
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

                        // Add any created Weapons to the character.
                        foreach (Weapon objWeapon in objWeapons)
                            _objCharacter.Weapons.Add(objWeapon);
                    }
                    foreach (XmlNode objXmlQualityItem in objXmlMetatype.SelectNodes("qualities/negative/quality"))
                    {
                        XmlNode objXmlQuality = objXmlQualityDocument.SelectSingleNode("/chummer/qualities/quality[name = \"" + objXmlQualityItem.InnerText + "\"]");
                        TreeNode objNode = new TreeNode();
                        List<Weapon> objWeapons = new List<Weapon>();
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

                        // Add any created Weapons to the character.
                        foreach (Weapon objWeapon in objWeapons)
                            _objCharacter.Weapons.Add(objWeapon);
                    }
                }

                // If a Metavariant has been selected, locate it in the file.
                if (cboMetavariant.SelectedValue.ToString() != "None")
                {
                    // Determine if the Metavariant has any bonuses.
                    if (objXmlMetavariant.InnerXml.Contains("bonus"))
                    {
                        objImprovementManager.CreateImprovements(Improvement.ImprovementSource.Metavariant, cboMetavariant.SelectedValue.ToString(), objXmlMetavariant.SelectSingleNode("bonus"), false, 1, cboMetavariant.SelectedValue.ToString());
                    }

                    // Create the Qualities that come with the Metatype.
                    foreach (XmlNode objXmlQualityItem in objXmlMetavariant.SelectNodes("qualities/positive/quality"))
                    {
                        XmlNode objXmlQuality = objXmlQualityDocument.SelectSingleNode("/chummer/qualities/quality[name = \"" + objXmlQualityItem.InnerText + "\"]");
                        TreeNode objNode = new TreeNode();
                        List<Weapon> objWeapons = new List<Weapon>();
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

                        // Add any created Weapons to the character.
                        foreach (Weapon objWeapon in objWeapons)
                            _objCharacter.Weapons.Add(objWeapon);
                    }
                    foreach (XmlNode objXmlQualityItem in objXmlMetavariant.SelectNodes("qualities/negative/quality"))
                    {
                        XmlNode objXmlQuality = objXmlQualityDocument.SelectSingleNode("/chummer/qualities/quality[name = \"" + objXmlQualityItem.InnerText + "\"]");
                        TreeNode objNode = new TreeNode();
                        List<Weapon> objWeapons = new List<Weapon>();
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

                        // Add any created Weapons to the character.
                        foreach (Weapon objWeapon in objWeapons)
                            _objCharacter.Weapons.Add(objWeapon);
                    }
                }

                // Add any Critter Powers the Metatype/Critter should have.
                XmlNode objXmlCritter = objXmlDocument.SelectSingleNode("/chummer/metatypes/metatype[name = \"" + _objCharacter.Metatype + "\"]");

                objXmlDocument = XmlManager.Instance.Load("critterpowers.xml");
                foreach (XmlNode objXmlPower in objXmlCritter.SelectNodes("powers/power"))
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

                    objPower.Create(objXmlCritterPower, _objCharacter, objNode, intRating, strForcedValue);
                    objPower.CountTowardsLimit = false;
                    _objCharacter.CritterPowers.Add(objPower);
                }

                // Add any Critter Powers the Metavariant should have.
                if (cboMetavariant.SelectedValue.ToString() != "None")
                {
                    foreach (XmlNode objXmlPower in objXmlMetavariant.SelectNodes("powers/power"))
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

                        objPower.Create(objXmlCritterPower, _objCharacter, objNode, intRating, strForcedValue);
                        objPower.CountTowardsLimit = false;
                        _objCharacter.CritterPowers.Add(objPower);
                    }
                }

                // Add any Natural Weapons the Metavariant should have.
                if (cboMetavariant.SelectedValue.ToString() != "None")
                {
                    if (objXmlMetavariant["naturalweapons"] != null)
                    {
                        foreach (XmlNode objXmlNaturalWeapon in objXmlMetavariant["naturalweapons"].SelectNodes("naturalweapon"))
                        {
                            Weapon objWeapon = new Weapon(_objCharacter);
                            objWeapon.Name = objXmlNaturalWeapon["name"].InnerText;
                            objWeapon.Category = LanguageManager.Instance.GetString("Tab_Critter");
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
                    }
                }

                if (cboCategory.SelectedValue.ToString() == "Spirits")
                {
                    if (objXmlMetatype["optionalpowers"] != null)
                    {
                        //For every 3 full points of Force a spirit has, it may gain one Optional Power.
                        for (int i = intForce -3; i >= 0; i -= 3)
                        {
                            XmlDocument objDummyDocument = new XmlDocument();
                            XmlNode bonusNode = objDummyDocument.CreateNode(XmlNodeType.Element, "bonus", null);
                            objDummyDocument.AppendChild(bonusNode);
                            XmlNode powerNode = objDummyDocument.ImportNode(objXmlMetatype["optionalpowers"].CloneNode(true),true);
                            objDummyDocument.ImportNode(powerNode, true);
                            bonusNode.AppendChild(powerNode);
                            objImprovementManager.CreateImprovements(Improvement.ImprovementSource.Metatype, lstMetatypes.SelectedValue.ToString(), bonusNode, false, 1, lstMetatypes.SelectedValue.ToString());
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
                            objPower.Create(objXmlCritterPower, _objCharacter, objNode, 0, string.Empty);
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
                            objPower.Create(objXmlCritterPower, _objCharacter, objNode, 0, string.Empty);
                            objPower.CountTowardsLimit = false;
                            _objCharacter.CritterPowers.Add(objPower);
                        }

                        //Natural Weapon.
                        objXmlCritterPower = objXmlDocument.SelectSingleNode("/chummer/powers/power[name = \"Natural Weapon\"]");
                        objNode = new TreeNode();
                        objPower = new CritterPower(_objCharacter);
                        objPower.Create(objXmlCritterPower, _objCharacter, objNode, 0, "DV " + intForce.ToString() + "P, AP 0");
                        objPower.CountTowardsLimit = false;
                        _objCharacter.CritterPowers.Add(objPower);

                        //Evanescence.
                        blnAddPower = _objCharacter.CritterPowers.All(objFindPower => objFindPower.Name != "Evanescence");
                        if (blnAddPower)
                        {
                            objXmlCritterPower = objXmlDocument.SelectSingleNode("/chummer/powers/power[name = \"Evanescence\"]");
                            objNode = new TreeNode();
                            objPower = new CritterPower(_objCharacter);
                            objPower.Create(objXmlCritterPower, _objCharacter, objNode, 0, string.Empty);
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
                        objPower.Create(objXmlCritterPower, _objCharacter, objNode, 0, string.Empty);
                        objPower.CountTowardsLimit = false;
                        _objCharacter.CritterPowers.Add(objPower);
                    }
                }
                XmlDocument objSkillDocument = XmlManager.Instance.Load("skills.xml");
                //Set the Active Skill Ratings for the Critter.
                foreach (XmlNode objXmlSkill in objXmlCritter.SelectNodes("skills/skill"))
                {
                    XmlNode objXmlSkillNode = objSkillDocument.SelectSingleNode("/chummer/skills/skill[name = \"" + objXmlSkill.InnerText + "\"]");
                    Skill.FromData(objXmlSkillNode, _objCharacter);
                    foreach (Skill objSkill in _objCharacter.SkillsSection.Skills.Where(objSkill => objSkill.Name == objXmlSkill.InnerText))
                    {
                        objSkill.Karma = objXmlSkill.Attributes["rating"].InnerText == "F" ? intForce : Convert.ToInt32(objXmlSkill.Attributes["rating"].InnerText);
                    }
                }
                //Set the Skill Group Ratings for the Critter.
                foreach (XmlNode objXmlSkill in objXmlCritter.SelectNodes("skills/group"))
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
                foreach (XmlNode objXmlSkill in objXmlCritter.SelectNodes("skills/knowledge"))
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
                XmlDocument objXmlProgramDocument = XmlManager.Instance.Load("complexforms.xml");
                foreach (XmlNode objXmlComplexForm in objXmlCritter.SelectNodes("complexforms/complexform"))
                {
                    string strForceValue = string.Empty;
                    if (objXmlComplexForm.Attributes["select"] != null)
                        strForceValue = objXmlComplexForm.Attributes["select"].InnerText;
                    XmlNode objXmlProgram = objXmlProgramDocument.SelectSingleNode("/chummer/complexforms/complexform[name = \"" + objXmlComplexForm.InnerText + "\"]");
                    TreeNode objNode = new TreeNode();
                    ComplexForm objProgram = new ComplexForm(_objCharacter);
                    objProgram.Create(objXmlProgram, _objCharacter, objNode, strForceValue);
                    _objCharacter.ComplexForms.Add(objProgram);
                }

                // Add any Advanced Programs the Critter comes with (typically Sprites)
                XmlDocument objXmlAIProgramDocument = XmlManager.Instance.Load("programs.xml");
                foreach (XmlNode objXmlAIProgram in objXmlCritter.SelectNodes("programs/program"))
                {
                    string strForceValue = string.Empty;
                    if (objXmlAIProgram.Attributes["select"] != null)
                        strForceValue = objXmlAIProgram.Attributes["select"].InnerText;
                    XmlNode objXmlProgram = objXmlAIProgramDocument.SelectSingleNode("/chummer/programs/program[name = \"" + objXmlAIProgram.InnerText + "\"]");
                    if (objXmlProgram != null)
                    {
                        TreeNode objNode = new TreeNode();
                        AIProgram objProgram = new AIProgram(_objCharacter);
                        objProgram.Create(objXmlProgram, _objCharacter, objNode, objXmlProgram["category"]?.InnerText == "Advanced Programs", strForceValue);
                        _objCharacter.AIPrograms.Add(objProgram);
                    }
                }

                // Add any Gear the Critter comes with (typically Programs for A.I.s)
                XmlDocument objXmlGearDocument = XmlManager.Instance.Load("gear.xml");
                foreach (XmlNode objXmlGear in objXmlCritter.SelectNodes("gears/gear"))
                {
                    int intRating = 0;
                    if (objXmlGear.Attributes["rating"] != null)
                        intRating = Convert.ToInt32(ExpressionToString(objXmlGear.Attributes["rating"].InnerText, Convert.ToInt32(nudForce.Value), 0));
                    string strForceValue = string.Empty;
                    if (objXmlGear.Attributes["select"] != null)
                        strForceValue = objXmlGear.Attributes["select"].InnerText;
                    XmlNode objXmlGearItem = objXmlGearDocument.SelectSingleNode("/chummer/gears/gear[name = \"" + objXmlGear.InnerText + "\"]");
                    TreeNode objNode = new TreeNode();
                    Gear objGear = new Gear(_objCharacter);
                    List<Weapon> lstWeapons = new List<Weapon>();
                    List<TreeNode> lstWeaponNodes = new List<TreeNode>();
                    objGear.Create(objXmlGearItem, _objCharacter, objNode, intRating, lstWeapons, lstWeaponNodes, strForceValue);
                    objGear.Cost = "0";
                    objGear.Cost3 = "0";
                    objGear.Cost6 = "0";
                    objGear.Cost10 = "0";
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
                }

                int x;
                       int.TryParse(lblBP.Text, out x);
                    //_objCharacter.BuildKarma = _objCharacter.BuildKarma - x;

                DialogResult = DialogResult.OK;
                Close();
            }
            else
            {
                MessageBox.Show(LanguageManager.Instance.GetString("Message_Metatype_SelectMetatype"), LanguageManager.Instance.GetString("MessageTitle_Metatype_SelectMetatype"), MessageBoxButtons.OK, MessageBoxIcon.Information);
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
            XmlDocument objXmlDocument = new XmlDocument();
            XPathNavigator nav = objXmlDocument.CreateNavigator();
            XPathExpression xprAttribute = nav.Compile(strIn.Replace("/", " div ").Replace("F", intForce.ToString()).Replace("1D6", intForce.ToString()).Replace("2D6", intForce.ToString()));
            object xprEvaluateResult = null;
            // This statement is wrapped in a try/catch since trying 1 div 2 results in an error with XSLT.
            try
            {
                xprEvaluateResult = nav.Evaluate(xprAttribute);
            }
            catch(XPathException) { }
            if (xprEvaluateResult is double)
                intValue = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(xprEvaluateResult.ToString(), GlobalOptions.InvariantCultureInfo)));
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
            XmlDocument objXmlDocument = XmlManager.Instance.Load(_strXmlFile);
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
