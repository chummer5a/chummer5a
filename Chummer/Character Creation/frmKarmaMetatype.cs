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
 using Chummer.Backend.Skills;
using MessageBox = System.Windows.Forms.MessageBox;
using System.Text;

namespace Chummer
{
    public partial class frmKarmaMetatype : Form
    {
        private readonly Character _objCharacter;

        private readonly string _strXmlFile = "metatypes.xml";

        private readonly List<ListItem> _lstCategory = new List<ListItem>();

        #region Character Events
        private void DoNothing(object sender)
        {
            // Do nothing. This is just an Event trap so an exception doesn't get thrown.
        }
        #endregion

        #region Form Events
        public frmKarmaMetatype(Character objCharacter, string strXmlFile = "metatypes.xml")
        {
            _objCharacter = objCharacter;
            _strXmlFile = strXmlFile;
            InitializeComponent();
            LanguageManager.TranslateWinForm(GlobalOptions.Language, this);

            // Attach EventHandlers for MAGEnabledChange and RESEnabledChanged since some Metatypes can enable these.
            _objCharacter.MAGEnabledChanged += DoNothing;
            _objCharacter.RESEnabledChanged += DoNothing;
            _objCharacter.DEPEnabledChanged += DoNothing;
            _objCharacter.AdeptTabEnabledChanged += DoNothing;
            _objCharacter.MagicianTabEnabledChanged += DoNothing;
            _objCharacter.TechnomancerTabEnabledChanged += DoNothing;
            _objCharacter.AdvancedProgramsTabEnabledChanged += DoNothing;
            _objCharacter.CyberwareTabDisabledChanged += DoNothing;
            _objCharacter.InitiationTabEnabledChanged += DoNothing;
            _objCharacter.CritterTabEnabledChanged += DoNothing;
        }

        private void frmMetatype_FormClosed(object sender, FormClosedEventArgs e)
        {
            // Detach EventHandlers for MAGEnabledChange and RESEnabledChanged since some Metatypes can enable these.
            _objCharacter.MAGEnabledChanged -= DoNothing;
            _objCharacter.RESEnabledChanged -= DoNothing;
            _objCharacter.DEPEnabledChanged -= DoNothing;
            _objCharacter.AdeptTabEnabledChanged -= DoNothing;
            _objCharacter.MagicianTabEnabledChanged -= DoNothing;
            _objCharacter.TechnomancerTabEnabledChanged -= DoNothing;
            _objCharacter.AdvancedProgramsTabEnabledChanged -= DoNothing;
            _objCharacter.CyberwareTabDisabledChanged -= DoNothing;
            _objCharacter.InitiationTabEnabledChanged -= DoNothing;
            _objCharacter.CritterTabEnabledChanged -= DoNothing;
        }

        private void frmMetatype_Load(object sender, EventArgs e)
        {
            // Load the Metatype information.
            XmlDocument objXmlDocument = XmlManager.Load(_strXmlFile);

            // Populate the Metatype Category list.
            XmlNodeList objXmlCategoryList = objXmlDocument.SelectNodes("/chummer/categories/category");

            // Create a list of Categories.
            XmlNode xmlMetatypesNode = objXmlDocument.SelectSingleNode("/chummer/metatypes");
            HashSet<string> lstAlreadyProcessed = new HashSet<string>();
            foreach (XmlNode objXmlCategory in objXmlCategoryList)
            {
                string strInnerText = objXmlCategory.InnerText;
                if (!lstAlreadyProcessed.Contains(strInnerText))
                {
                    lstAlreadyProcessed.Add(strInnerText);
                    if (null != xmlMetatypesNode.SelectSingleNode("metatype[category = \"" + strInnerText + "\" and (" + _objCharacter.Options.BookXPath() + ")]"))
                    {
                        _lstCategory.Add(new ListItem(strInnerText, objXmlCategory.Attributes?["translate"]?.InnerText ?? strInnerText));
                    }
                }
            }
            
            _lstCategory.Sort(CompareListItems.CompareNames);
            cboCategory.BeginUpdate();
            cboCategory.ValueMember = "Value";
            cboCategory.DisplayMember = "Name";
            cboCategory.DataSource = _lstCategory;

            // Attempt to select the default Metahuman Category. If it could not be found, select the first item in the list instead.
            cboCategory.SelectedValue = "Metahuman";
            if (cboCategory.SelectedIndex == -1 && cboCategory.Items.Count > 0)
                cboCategory.SelectedIndex = 0;

            cboCategory.EndUpdate();

            Height = cmdOK.Bottom + 40;
            lstMetatypes.Height = cmdOK.Bottom - lstMetatypes.Top;

            // Add Possession and Inhabitation to the list of Critter Tradition variations.
            tipTooltip.SetToolTip(chkPossessionBased, LanguageManager.GetString("Tip_Metatype_PossessionTradition", GlobalOptions.Language));
            tipTooltip.SetToolTip(chkBloodSpirit, LanguageManager.GetString("Tip_Metatype_BloodSpirit", GlobalOptions.Language));
            
            XmlNode objXmlPowersNode = XmlManager.Load("critterpowers.xml").SelectSingleNode("/chummer/powers");
            List<ListItem> lstMethods = new List<ListItem>
            {
                new ListItem("Possession", objXmlPowersNode?.SelectSingleNode("power[name = \"Possession\"]")?["translate"]?.InnerText ?? "Possession"),
                new ListItem("Inhabitation", objXmlPowersNode?.SelectSingleNode("power[name = \"Inhabitation\"]")?["translate"]?.InnerText ?? "Inhabitation")
            };
            
            lstMethods.Sort(CompareListItems.CompareNames);

            cboPossessionMethod.BeginUpdate();
            cboPossessionMethod.ValueMember = "Value";
            cboPossessionMethod.DisplayMember = "Name";
            cboPossessionMethod.DataSource = lstMethods;
            cboPossessionMethod.EndUpdate();

            PopulateMetatypes();
        }
        #endregion

        #region Control Events
        private bool _blnIgnoreMetatypeSelectedIndexChanged = false;
        private void lstMetatypes_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_blnIgnoreMetatypeSelectedIndexChanged)
                return;

            string strSelectedMetatype = lstMetatypes.SelectedValue?.ToString();
            // Don't attempt to do anything if nothing is selected.
            if (!string.IsNullOrEmpty(strSelectedMetatype))
            {
                XmlNode objXmlMetatype = XmlManager.Load(_strXmlFile).SelectSingleNode("/chummer/metatypes/metatype[name = \"" + strSelectedMetatype + "\"]");

                List<ListItem> lstMetavariants = new List<ListItem>
                {
                    new ListItem("None", LanguageManager.GetString("String_None", GlobalOptions.Language))
                };

                // Retrieve the list of Metavariants for the selected Metatype.
                XmlNodeList objXmlMetavariantList = objXmlMetatype.SelectNodes("metavariants/metavariant[" + _objCharacter.Options.BookXPath() + "]");
                foreach (XmlNode objXmlMetavariant in objXmlMetavariantList)
                {
                    string strName = objXmlMetavariant["name"].InnerText;
                    lstMetavariants.Add(new ListItem(strName, objXmlMetavariant["translate"]?.InnerText ?? strName));
                }

                cboMetavariant.BeginUpdate();
                cboMetavariant.ValueMember = "Value";
                cboMetavariant.DisplayMember = "Name";
                cboMetavariant.DataSource = lstMetavariants;
                cboMetavariant.Enabled = lstMetavariants.Count > 1;
                // Select the None item.
                cboMetavariant.SelectedIndex = 0;
                cboMetavariant.EndUpdate();

                // If the Metatype has Force enabled, show the Force NUD.
                string strEssMax = objXmlMetatype["essmax"]?.InnerText;
                if (objXmlMetatype["forcecreature"] != null || (!string.IsNullOrEmpty(strEssMax) && strEssMax.Contains("D6")))
                {
                    lblForceLabel.Visible = true;
                    nudForce.Visible = true;

                    if (!string.IsNullOrEmpty(strEssMax) && strEssMax.Contains("D6"))
                    {
                        int intPos = strEssMax.IndexOf("D6") - 1;
                        lblForceLabel.Text = strEssMax.Substring(intPos, 3);
                        nudForce.Maximum = Convert.ToInt32(strEssMax.Substring(intPos, 1)) * 6;
                    }
                    else
                    {
                        lblForceLabel.Text = LanguageManager.GetString("String_Force", GlobalOptions.Language);
                        nudForce.Maximum = 100;
                    }
                }
                else
                {
                    lblForceLabel.Visible = false;
                    nudForce.Visible = false;
                }
            }
            else
            {
                // Clear the Metavariant list if nothing is currently selected.
                List<ListItem> lstMetavariants = new List<ListItem>
                {
                    new ListItem("None", LanguageManager.GetString("String_None", GlobalOptions.Language))
                };

                cboMetavariant.BeginUpdate();
                cboMetavariant.ValueMember = "Value";
                cboMetavariant.DisplayMember = "Name";
                cboMetavariant.DataSource = lstMetavariants;
                cboMetavariant.Enabled = false;
                cboMetavariant.EndUpdate();

                lblForceLabel.Visible = false;
                nudForce.Visible = false;
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
            XmlNode objXmlMetavariant = null;
            string strSelectedMetatype = lstMetatypes.SelectedValue?.ToString();
            if (!string.IsNullOrEmpty(strSelectedMetatype))
            {
                objXmlMetatype = objXmlDocument.SelectSingleNode("/chummer/metatypes/metatype[name = \"" + strSelectedMetatype + "\"]");
                string strSelectedMetavariant = cboMetavariant.SelectedValue?.ToString();
                if (objXmlMetatype != null && string.IsNullOrEmpty(strSelectedMetavariant) && strSelectedMetavariant != "None")
                {
                    objXmlMetavariant = objXmlMetatype?.SelectSingleNode("metavariants/metavariant[name = \"" + strSelectedMetavariant + "\"]");
                }
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

                StringBuilder strbldQualities = new StringBuilder();
                // Build a list of the Metavariant's Qualities.
                foreach (XmlNode objXmlQuality in objXmlMetavariant.SelectNodes("qualities/*/quality"))
                {
                    if (GlobalOptions.Language != GlobalOptions.DefaultLanguage)
                    {
                        XmlNode objQuality = objXmlQualityDocument.SelectSingleNode("/chummer/qualities/quality[name = \"" + objXmlQuality.InnerText + "\"]");
                        strbldQualities.Append(objQuality["translate"]?.InnerText ?? objXmlQuality.InnerText);

                        string strSelect = objXmlQuality.Attributes["select"]?.InnerText;
                        if (!string.IsNullOrEmpty(strSelect))
                        {
                            strbldQualities.Append(" (");
                            strbldQualities.Append(LanguageManager.TranslateExtra(strSelect, GlobalOptions.Language));
                            strbldQualities.Append(')');
                        }
                    }
                    else
                    {
                        strbldQualities.Append(objXmlQuality.InnerText);
                        string strSelect = objXmlQuality.Attributes["select"]?.InnerText;
                        if (!string.IsNullOrEmpty(strSelect))
                        {
                            strbldQualities.Append(" (");
                            strbldQualities.Append(strSelect);
                            strbldQualities.Append(')');
                        }
                    }
                    strbldQualities.Append('\n');
                }

                if (strbldQualities.Length == 0)
                    lblQualities.Text = LanguageManager.GetString("String_None", GlobalOptions.Language);
                else
                    lblQualities.Text = strbldQualities.ToString();

                lblBP.Text = objXmlMetavariant["karma"].InnerText;
            }
            else if (objXmlMetatype != null)
            {
                if (objXmlMetatype["forcecreature"] == null)
                {
                    lblBOD.Text = string.Format("{0}/{1} ({2})",
                        objXmlMetatype["bodmin"]?.InnerText ?? "0",
                        objXmlMetatype["bodmax"]?.InnerText ?? "0",
                        objXmlMetatype["bodaug"]?.InnerText ?? "0");
                    lblAGI.Text = string.Format("{0}/{1} ({2})",
                        objXmlMetatype["agimin"]?.InnerText ?? "0",
                        objXmlMetatype["agimax"]?.InnerText ?? "0",
                        objXmlMetatype["agiaug"]?.InnerText ?? "0");
                    lblREA.Text = string.Format("{0}/{1} ({2})",
                        objXmlMetatype["reamin"]?.InnerText ?? "0",
                        objXmlMetatype["reamax"]?.InnerText ?? "0",
                        objXmlMetatype["reaaug"]?.InnerText ?? "0");
                    lblSTR.Text = string.Format("{0}/{1} ({2})",
                        objXmlMetatype["strmin"]?.InnerText ?? "0",
                        objXmlMetatype["strmax"]?.InnerText ?? "0",
                        objXmlMetatype["straug"]?.InnerText ?? "0");
                    lblCHA.Text = string.Format("{0}/{1} ({2})",
                        objXmlMetatype["chamin"]?.InnerText ?? "0",
                        objXmlMetatype["chamax"]?.InnerText ?? "0",
                        objXmlMetatype["chaaug"]?.InnerText ?? "0");
                    lblINT.Text = string.Format("{0}/{1} ({2})",
                        objXmlMetatype["intmin"]?.InnerText ?? "0",
                        objXmlMetatype["intmax"]?.InnerText ?? "0",
                        objXmlMetatype["intaug"]?.InnerText ?? "0");
                    lblLOG.Text = string.Format("{0}/{1} ({2})",
                        objXmlMetatype["logmin"]?.InnerText ?? "0",
                        objXmlMetatype["logmax"]?.InnerText ?? "0",
                        objXmlMetatype["logaug"]?.InnerText ?? string.Empty);
                    lblWIL.Text = string.Format("{0}/{1} ({2})",
                        objXmlMetatype["wilmin"]?.InnerText ?? "0",
                        objXmlMetatype["wilmax"]?.InnerText ?? "0",
                        objXmlMetatype["wilaug"]?.InnerText ?? "0");
                    lblINI.Text = string.Format("{0}/{1} ({2})",
                        objXmlMetatype["inimin"]?.InnerText ?? "0",
                        objXmlMetatype["inimax"]?.InnerText ?? "0",
                        objXmlMetatype["iniaug"]?.InnerText ?? "0");
                }
                else
                {
                    lblBOD.Text = objXmlMetatype["bodmin"]?.InnerText ?? "0";
                    lblAGI.Text = objXmlMetatype["agimin"]?.InnerText ?? "0";
                    lblREA.Text = objXmlMetatype["reamin"]?.InnerText ?? "0";
                    lblSTR.Text = objXmlMetatype["strmin"]?.InnerText ?? "0";
                    lblCHA.Text = objXmlMetatype["chamin"]?.InnerText ?? "0";
                    lblINT.Text = objXmlMetatype["intmin"]?.InnerText ?? "0";
                    lblLOG.Text = objXmlMetatype["logmin"]?.InnerText ?? "0";
                    lblWIL.Text = objXmlMetatype["wilmin"]?.InnerText ?? "0";
                    lblINI.Text = objXmlMetatype["inimin"]?.InnerText ?? "0";
                }

                StringBuilder strbldQualities = new StringBuilder();
                // Build a list of the Metatype's Positive Qualities.
                foreach (XmlNode objXmlQuality in objXmlMetatype.SelectNodes("qualities/*/quality"))
                {
                    if (GlobalOptions.Language != GlobalOptions.DefaultLanguage)
                    {
                        XmlNode objQuality = objXmlQualityDocument.SelectSingleNode("/chummer/qualities/quality[name = \"" + objXmlQuality.InnerText + "\"]");
                        strbldQualities.Append(objQuality["translate"]?.InnerText ?? objXmlQuality.InnerText);

                        string strSelect = objXmlQuality.Attributes["select"]?.InnerText;
                        if (!string.IsNullOrEmpty(strSelect))
                        {
                            strbldQualities.Append(" (");
                            strbldQualities.Append(LanguageManager.TranslateExtra(strSelect, GlobalOptions.Language));
                            strbldQualities.Append(')');
                        }
                    }
                    else
                    {
                        strbldQualities.Append(objXmlQuality.InnerText);
                        string strSelect = objXmlQuality.Attributes["select"]?.InnerText;
                        if (!string.IsNullOrEmpty(strSelect))
                        {
                            strbldQualities.Append(" (");
                            strbldQualities.Append(strSelect);
                            strbldQualities.Append(')');
                        }
                    }
                    strbldQualities.Append('\n');
                }

                if (strbldQualities.Length == 0)
                    lblQualities.Text = LanguageManager.GetString("String_None", GlobalOptions.Language);
                else
                    lblQualities.Text = strbldQualities.ToString();

                lblBP.Text = objXmlMetatype["karma"].InnerText;
            }
            else
            {
                lblBP.Text = "0";
                lblQualities.Text = LanguageManager.GetString("String_None", GlobalOptions.Language);
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
            Cursor = Cursors.WaitCursor;
            string strSelectedMetatype = lstMetatypes.SelectedValue?.ToString();
            if (!string.IsNullOrEmpty(strSelectedMetatype))
            {
                string strSelectedMetatypeCategory = cboCategory.SelectedValue?.ToString();
                string strSelectedMetavariant = cboMetavariant.SelectedValue.ToString();

                XmlDocument objXmlDocument = XmlManager.Load(_strXmlFile);

                // If this is a Shapeshifter, a Metavariant must be selected. Default to Human if None is selected.
                if (strSelectedMetatypeCategory == "Shapeshifter" && strSelectedMetavariant == "None")
                    strSelectedMetavariant = "Human";

                XmlNode objXmlMetatype = objXmlDocument.SelectSingleNode("/chummer/metatypes/metatype[name = \"" + strSelectedMetatype + "\"]");
                XmlNode objXmlMetavariant = objXmlMetatype?.SelectSingleNode("metavariants/metavariant[name = \"" + strSelectedMetavariant + "\"]");
                int intForce = 0;
                if (nudForce.Visible)
                    intForce = decimal.ToInt32(nudForce.Value);

                // Set Metatype information.
                int intMinModifier = 0;
                int intMaxModifier = 0;
                //TODO: What the hell is this for?
                /*if (_strXmlFile == "critters.xml")
                {
                    if (strSelectedMetatypeCategory == "Technocritters")
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
                if (strSelectedMetatypeCategory == "Shapeshifter")
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

                _objCharacter.Metatype = strSelectedMetatype;
                _objCharacter.MetatypeCategory = strSelectedMetatypeCategory;
                _objCharacter.MetatypeBP = Convert.ToInt32(lblBP.Text);
                _objCharacter.Metavariant = strSelectedMetavariant;

                string strMovement = objXmlMetatype["movement"]?.InnerText;
                if (!string.IsNullOrEmpty(strMovement))
                    _objCharacter.Movement = strMovement;
                // Load the Qualities file.
                XmlDocument objXmlQualityDocument = XmlManager.Load("qualities.xml");

                // Determine if the Metatype has any bonuses.
                XmlNode xmlBonusNode = charNode.SelectSingleNode("bonus");
                if (xmlBonusNode != null)
                    ImprovementManager.CreateImprovements(_objCharacter, Improvement.ImprovementSource.Metatype, strSelectedMetatype, xmlBonusNode, false, 1, strSelectedMetatype);

                List<Weapon> objWeapons = new List<Weapon>();
                // Create the Qualities that come with the Metatype.
                foreach (XmlNode objXmlQualityItem in charNode.SelectNodes("qualities/*/quality"))
                {
                    XmlNode objXmlQuality = objXmlQualityDocument.SelectSingleNode("/chummer/qualities/quality[name = \"" + objXmlQualityItem.InnerText + "\"]");
                    TreeNode objNode = new TreeNode();
                    List<TreeNode> objWeaponNodes = new List<TreeNode>();
                    Quality objQuality = new Quality(_objCharacter);
                    string strForceValue = objXmlQualityItem.Attributes["select"]?.InnerText ?? string.Empty;
                    QualitySource objSource = objXmlQualityItem.Attributes["removable"]?.InnerText == bool.TrueString ? QualitySource.MetatypeRemovable : QualitySource.Metatype;
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

                    string strRating = objXmlPower.Attributes["rating"]?.InnerText;
                    if (!string.IsNullOrEmpty(strRating))
                        intRating = Convert.ToInt32(strRating);
                    string strSelect = objXmlPower.Attributes["select"]?.InnerText;
                    if (!string.IsNullOrEmpty(strSelect))
                        strForcedValue = strSelect;

                    objPower.Create(objXmlCritterPower, objNode, intRating, strForcedValue);
                    objPower.CountTowardsLimit = false;
                    _objCharacter.CritterPowers.Add(objPower);
                }

                //Load any natural weapons the character has. 
                foreach (XmlNode objXmlNaturalWeapon in charNode.SelectNodes("nautralweapons/naturalweapon"))
                {
                    Weapon objWeapon = new Weapon(_objCharacter)
                    {
                        Name = objXmlNaturalWeapon["name"].InnerText,
                        Category = LanguageManager.GetString("Tab_Critter", GlobalOptions.Language),
                        WeaponType = "Melee",
                        Reach = Convert.ToInt32(objXmlNaturalWeapon["reach"].InnerText),
                        Damage = objXmlNaturalWeapon["damage"].InnerText,
                        AP = objXmlNaturalWeapon["ap"].InnerText,
                        Mode = "0",
                        RC = "0",
                        Concealability = 0,
                        Avail = "0",
                        Cost = 0,
                        UseSkill = objXmlNaturalWeapon["useskill"].InnerText,
                        Source = objXmlNaturalWeapon["source"].InnerText,
                        Page = objXmlNaturalWeapon["page"].InnerText
                    };

                    _objCharacter.Weapons.Add(objWeapon);
                }

                if (strSelectedMetatypeCategory == "Spirits")
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
                            ImprovementManager.CreateImprovements(_objCharacter, Improvement.ImprovementSource.Metatype, strSelectedMetatype, bonusNode, false, 1, strSelectedMetatype);
                        }
                    }
                    //If this is a Blood Spirit, add their free Critter Powers.
                    if (chkBloodSpirit.Checked)
                    {
                        XmlNode objXmlCritterPower;
                        TreeNode objNode;
                        CritterPower objPower;

                        //Energy Drain.
                        if (_objCharacter.CritterPowers.All(objFindPower => objFindPower.Name != "Energy Drain"))
                        {
                            objXmlCritterPower = objXmlDocument.SelectSingleNode("/chummer/powers/power[name = \"Energy Drain\"]");
                            objNode = new TreeNode();
                            objPower = new CritterPower(_objCharacter);
                            objPower.Create(objXmlCritterPower, objNode, 0, string.Empty);
                            objPower.CountTowardsLimit = false;
                            _objCharacter.CritterPowers.Add(objPower);
                        }

                        // Fear.
                        if (_objCharacter.CritterPowers.All(objFindPower => objFindPower.Name != "Fear"))
                        {
                            objXmlCritterPower = objXmlDocument.SelectSingleNode("/chummer/powers/power[name = \"Fear\"]");
                            objNode = new TreeNode();
                            objPower = new CritterPower(_objCharacter);
                            objPower.Create(objXmlCritterPower, objNode, 0, string.Empty);
                            objPower.CountTowardsLimit = false;
                            _objCharacter.CritterPowers.Add(objPower);
                        }

                        // Natural Weapon.
                        objXmlCritterPower = objXmlDocument.SelectSingleNode("/chummer/powers/power[name = \"Natural Weapon\"]");
                        objNode = new TreeNode();
                        objPower = new CritterPower(_objCharacter);
                        objPower.Create(objXmlCritterPower, objNode, 0, "DV " + intForce.ToString() + "P, AP 0");
                        objPower.CountTowardsLimit = false;
                        _objCharacter.CritterPowers.Add(objPower);

                        // Evanescence.
                        if (_objCharacter.CritterPowers.All(objFindPower => objFindPower.Name != "Evanescence"))
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
                        foreach (CritterPower objCritterPower in _objCharacter.CritterPowers.Where(objCritterPower => objCritterPower.Name == "Materialization"))
                        {
                            _objCharacter.CritterPowers.Remove(objCritterPower);
                            break;
                        }

                        //Add the selected Power.
                        XmlNode objXmlCritterPower = objXmlDocument.SelectSingleNode("/chummer/powers/power[name = \"" + cboPossessionMethod.SelectedValue?.ToString() + "\"]");
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
                    string strRating = objXmlSkill.Attributes?["rating"]?.InnerText;
                    if (objSkill != null)
                        objSkill.Karma = strRating == "F" ? intForce : Convert.ToInt32(strRating);
                }
                //Set the Skill Group Ratings for the Critter.
                foreach (XmlNode objXmlSkill in charNode.SelectNodes("skills/group"))
                {
                    string strRating = objXmlSkill.Attributes?["rating"]?.InnerText;
                    if (!string.IsNullOrEmpty(strRating))
                    {
                        foreach (SkillGroup objSkill in _objCharacter.SkillsSection.SkillGroups.Where(objSkill => objSkill.Name == objXmlSkill.InnerText))
                        {
                            objSkill.Karma = strRating == "F" ? intForce : Convert.ToInt32(strRating);
                        }
                    }
                }

                //Set the Knowledge Skill Ratings for the Critter.
                foreach (XmlNode objXmlSkill in charNode.SelectNodes("skills/knowledge"))
                {
                    string strRating = objXmlSkill.Attributes?["rating"]?.InnerText;
                    List<KnowledgeSkill> lstKnowledgeSkills = _objCharacter.SkillsSection.KnowledgeSkills.Where(objSkill => objSkill.Name == objXmlSkill.InnerText).ToList();
                    if (lstKnowledgeSkills.Count > 0)
                    {
                        if (!string.IsNullOrEmpty(strRating))
                        {
                            foreach (KnowledgeSkill objSkill in lstKnowledgeSkills)
                            {
                                objSkill.Karma = strRating == "F" ? intForce : Convert.ToInt32(strRating);
                            }
                        }
                    }
                    else
                    {
                        XmlNode objXmlSkillNode = objSkillDocument.SelectSingleNode("/chummer/knowledgeskills/skill[name = \"" + objXmlSkill.InnerText + "\"]");
                        if (objXmlSkillNode != null)
                        {
                            KnowledgeSkill objSkill = Skill.FromData(objXmlSkillNode, _objCharacter) as KnowledgeSkill;
                            if (!string.IsNullOrEmpty(strRating))
                            {
                                objSkill.Karma = strRating == "F" ? intForce : Convert.ToInt32(strRating);
                            }
                            _objCharacter.SkillsSection.KnowledgeSkills.Add(objSkill);
                        }
                        else
                        {
                            KnowledgeSkill objSkill = new KnowledgeSkill(_objCharacter, objXmlSkill.InnerText)
                            {
                                Type = objXmlSkill.Attributes?["category"]?.InnerText
                            };

                            if (!string.IsNullOrEmpty(strRating))
                            {
                                objSkill.Karma = strRating == "F" ? intForce : Convert.ToInt32(strRating);
                            }
                            _objCharacter.SkillsSection.KnowledgeSkills.Add(objSkill);
                        }
                    }
                }

                // Add any Complex Forms the Critter comes with (typically Sprites)
                XmlDocument objXmlProgramDocument = XmlManager.Load("complexforms.xml");
                foreach (XmlNode objXmlComplexForm in charNode.SelectNodes("complexforms/complexform"))
                {
                    XmlNode objXmlComplexFormData = objXmlProgramDocument.SelectSingleNode("/chummer/complexforms/complexform[name = \"" + objXmlComplexForm.InnerText + "\"]");
                    if (objXmlComplexFormData != null)
                    {
                        string strForceValue = objXmlComplexForm.Attributes?["select"]?.InnerText ?? string.Empty;
                        TreeNode objNode = new TreeNode();
                        ComplexForm objProgram = new ComplexForm(_objCharacter);
                        objProgram.Create(objXmlComplexFormData, objNode, null, strForceValue);
                        _objCharacter.ComplexForms.Add(objProgram);
                    }
                }

                // Add any Advanced Programs the Critter comes with (typically Sprites)
                XmlDocument objXmlAIProgramDocument = XmlManager.Load("programs.xml");
                foreach (XmlNode objXmlAIProgram in charNode.SelectNodes("programs/program"))
                {
                    XmlNode objXmlAIProgramData = objXmlAIProgramDocument.SelectSingleNode("/chummer/programs/program[name = \"" + objXmlAIProgram.InnerText + "\"]");
                    if (objXmlAIProgramData != null)
                    {
                        string strForceValue = objXmlAIProgram.Attributes?["select"]?.InnerText ?? string.Empty;
                        TreeNode objNode = new TreeNode();
                        AIProgram objProgram = new AIProgram(_objCharacter);
                        objProgram.Create(objXmlAIProgramData, objNode, objXmlAIProgramData["category"]?.InnerText == "Advanced Programs", strForceValue);
                        _objCharacter.AIPrograms.Add(objProgram);
                    }
                }

                // Add any Gear the Critter comes with (typically Programs for A.I.s)
                XmlDocument objXmlGearDocument = XmlManager.Load("gear.xml");
                foreach (XmlNode objXmlGear in charNode.SelectNodes("gears/gear"))
                {
                    XmlNode objXmlGearItem = objXmlGearDocument.SelectSingleNode("/chummer/gears/gear[name = \"" + objXmlGear.InnerText + "\"]");
                    if (objXmlGearItem != null)
                    {
                        string strRating = objXmlGear.Attributes?["rating"]?.InnerText;
                        int intRating = 0;
                        if (!string.IsNullOrEmpty(strRating))
                            intRating = ExpressionToInt(strRating, decimal.ToInt32(nudForce.Value), 0);
                        string strForceValue = objXmlGear.Attributes?["select"]?.InnerText ?? string.Empty;
                        TreeNode objNode = new TreeNode();
                        Gear objGear = new Gear(_objCharacter);
                        List<Weapon> lstWeapons = new List<Weapon>();
                        List<TreeNode> lstWeaponNodes = new List<TreeNode>();
                        objGear.Create(objXmlGearItem, objNode, intRating, lstWeapons, lstWeaponNodes, strForceValue);
                        objGear.Cost = "0";
                        _objCharacter.Gear.Add(objGear);
                    }
                }

                // Sprites can never have Physical Attributes
                if (_objCharacter.DEPEnabled || strSelectedMetatype.EndsWith("Sprite"))
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
                MessageBox.Show(LanguageManager.GetString("Message_Metatype_SelectMetatype", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_Metatype_SelectMetatype", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            Cursor = Cursors.Default;
        }

        /// <summary>
        /// Convert Force, 1D6, or 2D6 into a usable value.
        /// </summary>
        /// <param name="strIn">Expression to convert.</param>
        /// <param name="intForce">Force value to use.</param>
        /// <param name="intOffset">Dice offset.</param>
        /// <returns></returns>
        private static int ExpressionToInt(string strIn, int intForce, int intOffset)
        {
            if (string.IsNullOrWhiteSpace(strIn))
                return intOffset;
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
                    return 1;
            }
            else if (intValue < 0)
                return 0;
            return intValue;
        }

        /// <summary>
        /// Convert Force, 1D6, or 2D6 into a usable value.
        /// </summary>
        /// <param name="strIn">Expression to convert.</param>
        /// <param name="intForce">Force value to use.</param>
        /// <param name="intOffset">Dice offset.</param>
        /// <returns></returns>
        private static string ExpressionToString(string strIn, int intForce, int intOffset)
        {
            return ExpressionToInt(strIn, intForce, intOffset).ToString();
        }

        /// <summary>
        /// Populate the list of Metatypes.
        /// </summary>
        void PopulateMetatypes()
        {
            string strSelectedCategory = cboCategory.SelectedValue?.ToString();
            if (!string.IsNullOrEmpty(strSelectedCategory))
            {
                List<ListItem> lstMetatype = new List<ListItem>();

                foreach (XmlNode objXmlMetatype in XmlManager.Load(_strXmlFile).SelectNodes("/chummer/metatypes/metatype[(" + _objCharacter.Options.BookXPath() + ") and category = \"" + strSelectedCategory + "\"]"))
                {
                    string strName = objXmlMetatype["name"]?.InnerText ?? string.Empty;
                    lstMetatype.Add(new ListItem(strName, objXmlMetatype["translate"]?.InnerText ?? strName));
                }

                lstMetatype.Sort(CompareListItems.CompareNames);

                lstMetatypes.BeginUpdate();
                lstMetatypes.ValueMember = "Value";
                lstMetatypes.DisplayMember = "Name";
                _blnIgnoreMetatypeSelectedIndexChanged = true;
                lstMetatypes.DataSource = lstMetatype;
                _blnIgnoreMetatypeSelectedIndexChanged = false;

                // Attempt to select the default Human item. If it could not be found, select the first item in the list instead.
                lstMetatypes.SelectedValue = "Human";
                if (lstMetatypes.SelectedIndex == -1 && lstMetatypes.Items.Count > 0)
                    lstMetatypes.SelectedIndex = 0;

                lstMetatypes.EndUpdate();

                if (strSelectedCategory.EndsWith("Spirits"))
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

        private void chkPossessionBased_CheckedChanged(object sender, EventArgs e)
        {
            cboPossessionMethod.Enabled = chkPossessionBased.Checked;
        }
        #endregion
    }
}
