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
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace Chummer
{
    public partial class frmSelectCritterPower : Form
    {
        private string _strSelectedPower = string.Empty;
        private int _intSelectedRating = 0;
        private static string s_StrSelectCategory = string.Empty;
        private decimal _decPowerPoints = 0.0m;
        private bool _blnAddAgain = false;

        private readonly XmlDocument _objXmlDocument = null;
        private readonly XmlDocument _objXmlCritterDocument = null;
        private readonly Character _objCharacter;

        private readonly List<ListItem> _lstCategory = new List<ListItem>();

        #region Control Events
        public frmSelectCritterPower(Character objCharacter)
        {
            InitializeComponent();
            LanguageManager.TranslateWinForm(GlobalOptions.Language, this);
            _objCharacter = objCharacter;
            MoveControls();
            _objXmlDocument = XmlManager.Load("critterpowers.xml");
            if (_objCharacter.IsCritter)
                _objXmlCritterDocument = XmlManager.Load("critters.xml");
            else
                _objXmlCritterDocument = XmlManager.Load("metatypes.xml");
            if (_objXmlCritterDocument.SelectSingleNode("/chummer/metatypes/metatype[name = \"" + _objCharacter.Metatype + "\"]") == null)
            {
                _objXmlCritterDocument = XmlManager.Load("metatypes.xml");
            }
        }

        private void frmSelectCritterPower_Load(object sender, EventArgs e)
        {
            foreach (Label objLabel in Controls.OfType<Label>())
            {
                if (objLabel.Text.StartsWith('['))
                    objLabel.Text = string.Empty;
            }

            // Populate the Category list.
            XmlNodeList objXmlCategoryList = _objXmlDocument.SelectNodes("/chummer/categories/category");
            foreach (XmlNode objXmlCategory in objXmlCategoryList)
            {
                string strInnerText = objXmlCategory.InnerText;
                _lstCategory.Add(new ListItem(strInnerText, objXmlCategory.Attributes?["translate"]?.InnerText ?? strInnerText));
            }

            // Remove Optional Powers if the Critter does not have access to them.
            XmlNode objXmlCritter = _objXmlCritterDocument.SelectSingleNode("/chummer/metatypes/metatype[name = \"" + _objCharacter.Metatype + "\"]");
            if (objXmlCritter["optionalpowers"] == null)
            {
                foreach (ListItem objItem in _lstCategory)
                {
                    if (objItem.Value == "Allowed Optional Powers")
                    {
                        _lstCategory.Remove(objItem);
                        break;
                    }
                }
            }

            // Remove Free Spirit Powers if the critter is not a Free Spirit.
            if (_objCharacter.Metatype != "Free Spirit")
            {
                foreach (ListItem objItem in _lstCategory)
                {
                    if (objItem.Value == "Free Spirit")
                    {
                        _lstCategory.Remove(objItem);
                        break;
                    }
                }
            }

            // Remove Toxic Critter Powers if the critter is not a Toxic Critter.
            if (_objCharacter.MetatypeCategory != "Toxic Critters")
            {
                foreach (ListItem objItem in _lstCategory)
                {
                    if (objItem.Value == "Toxic Critter Powers")
                    {
                        _lstCategory.Remove(objItem);
                        break;
                    }
                }
            }

            // Remove Emergent Powers if the critter is not a Sprite or A.I.
            if (!_objCharacter.MetatypeCategory.EndsWith("Sprites") && !_objCharacter.MetatypeCategory.EndsWith("Sprite") && !_objCharacter.MetatypeCategory.EndsWith("A.I.s") & _objCharacter.MetatypeCategory != "Technocritters" && _objCharacter.MetatypeCategory != "Protosapients")
            {
                foreach (ListItem objItem in _lstCategory)
                {
                    if (objItem.Value == "Emergent")
                    {
                        _lstCategory.Remove(objItem);
                        break;
                    }
                }
            }

            // Remove Echoes Powers if the critter is not a Free Sprite.
            if (!_objCharacter.IsFreeSprite)
            {
                foreach (ListItem objItem in _lstCategory)
                {
                    if (objItem.Value == "Echoes")
                    {
                        _lstCategory.Remove(objItem);
                        break;
                    }
                }
            }

            // Remove Shapeshifter Powers if the critter is not a Shapeshifter.
            if (_objCharacter.MetatypeCategory != "Shapeshifter")
            {
                foreach (ListItem objItem in _lstCategory)
                {
                    if (objItem.Value == "Shapeshifter")
                    {
                        _lstCategory.Remove(objItem);
                        break;
                    }
                }
            }

            bool blnIsDrake = _objCharacter.Qualities.Any(objQuality =>
            objQuality.Name == "Dracoform (Eastern Drake)" || objQuality.Name == "Dracoform (Western Drake)" ||
            objQuality.Name == "Dracoform (Sea Drake)" || objQuality.Name == "Dracoform (Feathered Drake)");

            if (!blnIsDrake)
            {
                foreach (ListItem objItem in _lstCategory)
                {
                    if (objItem.Value == "Drake") continue;
                    _lstCategory.Remove(objItem);
                    break;
                }
            }
            _lstCategory.Sort(CompareListItems.CompareNames);

            if (_lstCategory.Count > 0)
            {
                _lstCategory.Insert(0, new ListItem("Show All", LanguageManager.GetString("String_ShowAll", GlobalOptions.Language)));
            }

            cboCategory.BeginUpdate();
            cboCategory.DataSource = null;
            cboCategory.ValueMember = "Value";
            cboCategory.DisplayMember = "Name";
            cboCategory.DataSource = _lstCategory;
            cboCategory.EndUpdate();

            // Select the first Category in the list.
            if (string.IsNullOrEmpty(s_StrSelectCategory))
                cboCategory.SelectedIndex = 0;
            else if (cboCategory.Items.Contains(s_StrSelectCategory))
            {
                cboCategory.SelectedValue = s_StrSelectCategory;
            }

            if (cboCategory.SelectedIndex == -1)
                cboCategory.SelectedIndex = 0;
        }

        private void cmdOK_Click(object sender, EventArgs e)
        {
            AcceptForm();
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void trePowers_AfterSelect(object sender, TreeViewEventArgs e)
        {
            lblPowerPoints.Visible = false;
            lblPowerPointsLabel.Visible = false;
            string strSelectedPower = trePowers.SelectedNode.Tag?.ToString();
            if (!string.IsNullOrEmpty(strSelectedPower))
            {
                XmlNode objXmlPower = _objXmlDocument.SelectSingleNode("/chummer/powers/power[id = \"" + strSelectedPower + "\"]");
                if (objXmlPower != null)
                {
                    lblCritterPowerCategory.Text = objXmlPower["category"]?.InnerText ?? string.Empty;

                    switch (objXmlPower["type"]?.InnerText)
                    {
                        case "M":
                            lblCritterPowerType.Text = LanguageManager.GetString("String_SpellTypeMana", GlobalOptions.Language);
                            break;
                        case "P":
                            lblCritterPowerType.Text = LanguageManager.GetString("String_SpellTypePhysical", GlobalOptions.Language);
                            break;
                        default:
                            lblCritterPowerType.Text = string.Empty;
                            break;
                    }

                    switch (objXmlPower["action"]?.InnerText)
                    {
                        case "Auto":
                            lblCritterPowerAction.Text = LanguageManager.GetString("String_ActionAutomatic", GlobalOptions.Language);
                            break;
                        case "Free":
                            lblCritterPowerAction.Text = LanguageManager.GetString("String_ActionFree", GlobalOptions.Language);
                            break;
                        case "Simple":
                            lblCritterPowerAction.Text = LanguageManager.GetString("String_ActionSimple", GlobalOptions.Language);
                            break;
                        case "Complex":
                            lblCritterPowerAction.Text = LanguageManager.GetString("String_ActionComplex", GlobalOptions.Language);
                            break;
                        case "Special":
                            lblCritterPowerAction.Text = LanguageManager.GetString("String_SpellDurationSpecial", GlobalOptions.Language);
                            break;
                        default:
                            lblCritterPowerAction.Text = string.Empty;
                            break;
                    }

                    string strRange = objXmlPower["range"]?.InnerText ?? string.Empty;
                    if (!string.IsNullOrEmpty(strRange))
                    {
                        strRange = strRange.CheapReplace("Self", () => LanguageManager.GetString("String_SpellRangeSelf", GlobalOptions.Language));
                        strRange = strRange.CheapReplace("Special", () => LanguageManager.GetString("String_SpellDurationSpecial", GlobalOptions.Language));
                        strRange = strRange.CheapReplace("LOS", () => LanguageManager.GetString("String_SpellRangeLineOfSight", GlobalOptions.Language));
                        strRange = strRange.CheapReplace("LOI", () => LanguageManager.GetString("String_SpellRangeLineOfInfluence", GlobalOptions.Language));
                        strRange = strRange.CheapReplace("T", () => LanguageManager.GetString("String_SpellRangeTouch", GlobalOptions.Language));
                        strRange = strRange.CheapReplace("(A)", () => "(" + LanguageManager.GetString("String_SpellRangeArea", GlobalOptions.Language) + ')');
                        strRange = strRange.CheapReplace("MAG", () => LanguageManager.GetString("String_AttributeMAGShort", GlobalOptions.Language));
                    }
                    lblCritterPowerRange.Text = strRange;

                    string strDuration = objXmlPower["duration"]?.InnerText ?? string.Empty;
                    switch (strDuration)
                    {
                        case "Instant":
                            lblCritterPowerDuration.Text = LanguageManager.GetString("String_SpellDurationInstantLong", GlobalOptions.Language);
                            break;
                        case "Sustained":
                            lblCritterPowerDuration.Text = LanguageManager.GetString("String_SpellDurationSustained", GlobalOptions.Language);
                            break;
                        case "Always":
                            lblCritterPowerDuration.Text = LanguageManager.GetString("String_SpellDurationAlways", GlobalOptions.Language);
                            break;
                        case "Special":
                            lblCritterPowerDuration.Text = LanguageManager.GetString("String_SpellDurationSpecial", GlobalOptions.Language);
                            break;
                        default:
                            lblCritterPowerDuration.Text = strDuration;
                            break;
                    }

                    string strSource = objXmlPower["source"]?.InnerText ?? string.Empty;
                    string strPage = objXmlPower["altpage"]?.InnerText ?? objXmlPower["page"]?.InnerText ?? string.Empty;
                    lblCritterPowerSource.Text = CommonFunctions.LanguageBookShort(strSource, GlobalOptions.Language) + ' ' + strPage;
                    tipTooltip.SetToolTip(lblCritterPowerSource, CommonFunctions.LanguageBookLong(strSource, GlobalOptions.Language) + ' ' + LanguageManager.GetString("String_Page", GlobalOptions.Language) + ' ' + strPage);

                    nudCritterPowerRating.Enabled = objXmlPower["rating"] != null;

                    lblKarma.Text = objXmlPower["karma"]?.InnerText ?? "0";

                    // If the character is a Free Spirit, populate the Power Points Cost as well.
                    if (_objCharacter.Metatype == "Free Spirit")
                    {
                        XmlNode objXmlCritter = _objXmlCritterDocument.SelectSingleNode("/chummer/metatypes/metatype[name = \"" + _objCharacter.Metatype + "\"]");
                        XmlNode objXmlCritterPower = objXmlCritter.SelectSingleNode("optionalpowers/power[. = \"" + objXmlPower["name"]?.InnerText + "\"]");
                        lblPowerPoints.Text = objXmlCritterPower.Attributes["cost"].InnerText;
                        lblPowerPoints.Visible = true;
                        lblPowerPointsLabel.Visible = true;
                    }
                }
            }
        }

        private void cboCategory_SelectedIndexChanged(object sender, EventArgs e)
        {
            XmlNode objXmlCritter = _objXmlCritterDocument.SelectSingleNode("/chummer/metatypes/metatype[name = \"" + _objCharacter.Metatype + "\" and (" + _objCharacter.Options.BookXPath() + ")]");

            trePowers.Nodes.Clear();

            string strCategory = cboCategory.SelectedValue?.ToString();

            List<string> lstPowerWhitelist = new List<string>();

            // If the Critter is only allowed certain Powers, display only those.
            XmlNode xmlOptionalPowers = objXmlCritter["optionalpowers"];
            if (xmlOptionalPowers != null)
            {
                foreach (XmlNode xmlNode in xmlOptionalPowers.SelectNodes("power"))
                    lstPowerWhitelist.Add(xmlNode.InnerText);

                // Determine if the Critter has a physical presence Power (Materialization, Possession, or Inhabitation).
                bool blnPhysicalPresence = _objCharacter.CritterPowers.Any(x => x.Name == "Materialization" || x.Name == "Possession" || x.Name == "Inhabitation");

                // Add any Critter Powers the Critter comes with that have been manually deleted so they can be re-added.
                foreach (XmlNode objXmlCritterPower in objXmlCritter.SelectNodes("powers/power"))
                {
                    bool blnAddPower = true;
                    // Make sure the Critter doesn't already have the Power.
                    foreach (CritterPower objCheckPower in _objCharacter.CritterPowers)
                    {
                        if (objCheckPower.Name == objXmlCritterPower.InnerText)
                        {
                            blnAddPower = false;
                            break;
                        }
                        if ((objCheckPower.Name == "Materialization" || objCheckPower.Name == "Possession" || objCheckPower.Name == "Inhabitation") && blnPhysicalPresence)
                        {
                            blnAddPower = false;
                            break;
                        }
                    }

                    if (blnAddPower)
                    {
                        lstPowerWhitelist.Add(objXmlCritterPower.InnerText);

                        // If Manifestation is one of the Powers, also include Inhabitation and Possess if they're not already in the list.
                        if (!blnPhysicalPresence)
                        {
                            if (objXmlCritterPower.InnerText == "Materialization")
                            {
                                bool blnFoundPossession = false;
                                bool blnFoundInhabitation = false;
                                foreach (string strCheckPower in lstPowerWhitelist)
                                {
                                    if (strCheckPower == "Possession")
                                        blnFoundPossession = true;
                                    else if (strCheckPower == "Inhabitation")
                                        blnFoundInhabitation = true;
                                    if (blnFoundInhabitation && blnFoundPossession)
                                        break;
                                }
                                if (!blnFoundPossession)
                                {
                                    lstPowerWhitelist.Add("Possession");
                                }
                                if (!blnFoundInhabitation)
                                {
                                    lstPowerWhitelist.Add("Inhabitation");
                                }
                            }
                        }
                    }
                }
            }

            string strFilter = "(" + _objCharacter.Options.BookXPath() + ')';
            if (!string.IsNullOrEmpty(strCategory) && strCategory != "Show All")
            {
                if (strCategory == "Toxic Critter Powers")
                    strFilter += " and (category = \"" + strCategory + "\" or toxic = \"True\")";
                else
                    strFilter += " and category = \"" + strCategory + '\"';
            }
            else
            {
                bool blnHasToxic = false;
                StringBuilder objCategoryFilter = new StringBuilder();
                foreach (string strItem in _lstCategory.Select(x => x.Value))
                {
                    if (!string.IsNullOrEmpty(strItem))
                    {
                        objCategoryFilter.Append("category = \"" + strItem + "\" or ");
                        if (strItem == "Toxic Critter Powers")
                        {
                            objCategoryFilter.Append("toxic = \"True\" or ");
                            blnHasToxic = true;
                        }
                    }
                }
                if (objCategoryFilter.Length > 0)
                {
                    strFilter += " and (" + objCategoryFilter.ToString().TrimEnd(" or ") + ')';
                }
                if (!blnHasToxic)
                    strFilter += " and (not(toxic) or toxic != \"True\")";
            }
            foreach (XmlNode objXmlPower in _objXmlDocument.SelectNodes("/chummer/powers/power[" + strFilter + "]"))
            {
                string strPowerName = objXmlPower["name"].InnerText;
                if (!lstPowerWhitelist.Contains(strPowerName) && lstPowerWhitelist.Count != 0) continue;
                TreeNode objNode = new TreeNode
                {
                    Tag = objXmlPower["id"].InnerText,
                    Text = objXmlPower["translate"]?.InnerText ?? strPowerName
                };
                trePowers.Nodes.Add(objNode);
            }
            trePowers.Sort();
        }

        private void trePowers_DoubleClick(object sender, EventArgs e)
        {
            AcceptForm();
        }

        private void cmdOKAdd_Click(object sender, EventArgs e)
        {
            _blnAddAgain = true;
            cmdOK_Click(sender, e);
        }
        #endregion

        #region Methods
        /// <summary>
        /// Accept the selected item and close the form.
        /// </summary>
        private void AcceptForm()
        {
            string strSelectedPower = trePowers.SelectedNode?.Tag.ToString();
            if (string.IsNullOrEmpty(strSelectedPower))
                return;

            if (nudCritterPowerRating.Enabled)
                _intSelectedRating = decimal.ToInt32(nudCritterPowerRating.Value);
            s_StrSelectCategory = cboCategory.SelectedValue?.ToString() ?? string.Empty;
            _strSelectedPower = strSelectedPower;

            // If the character is a Free Spirit (PC, not the Critter version), populate the Power Points Cost as well.
            if (_objCharacter.Metatype == "Free Spirit" && !_objCharacter.IsCritter)
            {
                XmlNode objXmlCritter = _objXmlCritterDocument.SelectSingleNode("/chummer/metatypes/metatype[name = \"" + _objCharacter.Metatype + "\"]");
                XmlNode objXmlPower = objXmlCritter.SelectSingleNode("optionalpowers/power[. = \"" + trePowers.SelectedNode.Tag + "\"]");
                _decPowerPoints = Convert.ToDecimal(objXmlPower.Attributes["cost"].InnerText, GlobalOptions.InvariantCultureInfo);
            }

            DialogResult = DialogResult.OK;
        }

        private void MoveControls()
        {
            int intWidth = Math.Max(lblCritterPowerCategoryLabel.Width, lblCritterPowerTypeLabel.Width);
            intWidth = Math.Max(intWidth, lblCritterPowerActionLabel.Width);
            intWidth = Math.Max(intWidth, lblCritterPowerRangeLabel.Width);
            intWidth = Math.Max(intWidth, lblCritterPowerDurationLabel.Width);
            intWidth = Math.Max(intWidth, lblCritterPowerRatingLabel.Width);
            intWidth = Math.Max(intWidth, lblCritterPowerSourceLabel.Width);
            intWidth = Math.Max(intWidth, lblPowerPointsLabel.Width);

            lblCritterPowerCategory.Left = lblCritterPowerCategoryLabel.Left + intWidth + 6;
            lblCritterPowerType.Left = lblCritterPowerTypeLabel.Left + intWidth + 6;
            lblCritterPowerAction.Left = lblCritterPowerActionLabel.Left + intWidth + 6;
            lblCritterPowerRange.Left = lblCritterPowerRangeLabel.Left + intWidth + 6;
            lblCritterPowerDuration.Left = lblCritterPowerDurationLabel.Left + intWidth + 6;
            nudCritterPowerRating.Left = lblCritterPowerRatingLabel.Left + intWidth + 6;
            lblCritterPowerSource.Left = lblCritterPowerSourceLabel.Left + intWidth + 6;
            lblPowerPoints.Left = lblPowerPointsLabel.Left + intWidth + 6;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Whether or not the user wants to add another item after this one.
        /// </summary>
        public bool AddAgain
        {
            get
            {
                return _blnAddAgain;
            }
        }

        /// <summary>
        /// Criter Power that was selected in the dialogue.
        /// </summary>
        public string SelectedPower
        {
            get
            {
                return _strSelectedPower;
            }
        }

        /// <summary>
        /// Rating for the Critter Power that was selected in the dialogue.
        /// </summary>
        public int SelectedRating
        {
            get
            {
                return _intSelectedRating;
            }
        }

        /// <summary>
        /// Power Point cost for the Critter Power (only applies to Free Spirits).
        /// </summary>
        public decimal PowerPoints
        {
            get
            {
                return _decPowerPoints;
            }
        }
        #endregion
    }
}
