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
using System.Text;
using System.Windows.Forms;
 using System.Xml.XPath;

namespace Chummer
{
    public partial class frmSelectCritterPower : Form
    {
        private string _strSelectedPower = string.Empty;
        private int _intSelectedRating;
        private static string s_StrSelectCategory = string.Empty;
        private decimal _decPowerPoints;
        private bool _blnAddAgain;

        private readonly XPathNavigator _xmlBaseCritterPowerDataNode;
        private readonly XPathNavigator _xmlMetatypeDataNode;
        private readonly Character _objCharacter;

        private readonly List<ListItem> _lstCategory = new List<ListItem>();

        #region Control Events
        public frmSelectCritterPower(Character objCharacter)
        {
            InitializeComponent();
            LanguageManager.TranslateWinForm(GlobalOptions.Language, this);
            _objCharacter = objCharacter;
            MoveControls();
            _xmlBaseCritterPowerDataNode = XmlManager.Load("critterpowers.xml").GetFastNavigator().SelectSingleNode("/chummer");
            if (_objCharacter.IsCritter)
            {
                _xmlMetatypeDataNode = XmlManager.Load("critters.xml").GetFastNavigator().SelectSingleNode("/chummer/metatypes/metatype[name = \"" + _objCharacter.Metatype + "\"]");
                if (_xmlMetatypeDataNode != null && !string.IsNullOrEmpty(_objCharacter.Metavariant) && _objCharacter.Metavariant != "None")
                {
                    _xmlMetatypeDataNode = _xmlMetatypeDataNode.SelectSingleNode("/metavariants/metavariant[name = \"" + _objCharacter.Metavariant + "\"]") ?? _xmlMetatypeDataNode;
                }
            }
            if (_xmlMetatypeDataNode == null)
            {
                _xmlMetatypeDataNode = XmlManager.Load("metatypes.xml").GetFastNavigator().SelectSingleNode("/chummer/metatypes/metatype[name = \"" + _objCharacter.Metatype + "\"]");
                if (_xmlMetatypeDataNode != null && !string.IsNullOrEmpty(_objCharacter.Metavariant) && _objCharacter.Metavariant != "None")
                {
                    _xmlMetatypeDataNode = _xmlMetatypeDataNode.SelectSingleNode("/metavariants/metavariant[name = \"" + _objCharacter.Metavariant + "\"]") ?? _xmlMetatypeDataNode;
                }
            }
        }

        private void frmSelectCritterPower_Load(object sender, EventArgs e)
        {
            // Populate the Category list.
            foreach (XPathNavigator objXmlCategory in _xmlBaseCritterPowerDataNode.Select("categories/category"))
            {
                string strInnerText = objXmlCategory.Value;
                _lstCategory.Add(new ListItem(strInnerText, objXmlCategory.SelectSingleNode("@translate")?.Value ?? strInnerText));
            }

            // Remove Optional Powers if the Critter does not have access to them.
            if (_xmlMetatypeDataNode.SelectSingleNode("optionalpowers") == null)
            {
                foreach (ListItem objItem in _lstCategory)
                {
                    if (objItem.Value.ToString() == "Allowed Optional Powers")
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
                    if (objItem.Value.ToString() == "Free Spirit")
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
                    if (objItem.Value.ToString() == "Toxic Critter Powers")
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
                    if (objItem.Value.ToString() == "Emergent")
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
                    if (objItem.Value.ToString() == "Echoes")
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
                    if (objItem.Value.ToString() == "Shapeshifter")
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
                    if (objItem.Value.ToString() == "Drake") continue;
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
            _blnAddAgain = false;
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
                XPathNavigator objXmlPower = _xmlBaseCritterPowerDataNode.SelectSingleNode("powers/power[id = \"" + strSelectedPower + "\"]");
                if (objXmlPower != null)
                {
                    lblCritterPowerCategory.Text = objXmlPower.SelectSingleNode("category")?.Value ?? string.Empty;

                    switch (objXmlPower.SelectSingleNode("type")?.Value)
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

                    switch (objXmlPower.SelectSingleNode("action")?.Value)
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

                    string strRange = objXmlPower.SelectSingleNode("range")?.Value ?? string.Empty;
                    if (!string.IsNullOrEmpty(strRange))
                    {
                        strRange = strRange.CheapReplace("Self", () => LanguageManager.GetString("String_SpellRangeSelf", GlobalOptions.Language))
                            .CheapReplace("Special", () => LanguageManager.GetString("String_SpellDurationSpecial", GlobalOptions.Language))
                            .CheapReplace("LOS", () => LanguageManager.GetString("String_SpellRangeLineOfSight", GlobalOptions.Language))
                            .CheapReplace("LOI", () => LanguageManager.GetString("String_SpellRangeLineOfInfluence", GlobalOptions.Language))
                            .CheapReplace("T", () => LanguageManager.GetString("String_SpellRangeTouch", GlobalOptions.Language))
                            .CheapReplace("(A)", () => "(" + LanguageManager.GetString("String_SpellRangeArea", GlobalOptions.Language) + ')')
                            .CheapReplace("MAG", () => LanguageManager.GetString("String_AttributeMAGShort", GlobalOptions.Language));
                    }
                    lblCritterPowerRange.Text = strRange;

                    string strDuration = objXmlPower.SelectSingleNode("duration")?.Value ?? string.Empty;
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

                    string strSource = objXmlPower.SelectSingleNode("source")?.Value ?? LanguageManager.GetString("String_Unknown", GlobalOptions.Language);
                    string strPage = objXmlPower.SelectSingleNode("altpage")?.Value ?? objXmlPower.SelectSingleNode("page")?.Value ?? LanguageManager.GetString("String_Unknown", GlobalOptions.Language);
                    string strSpaceCharacter = LanguageManager.GetString("String_Space", GlobalOptions.Language);
                    lblCritterPowerSource.Text = CommonFunctions.LanguageBookShort(strSource, GlobalOptions.Language) + strSpaceCharacter + strPage;
                    GlobalOptions.ToolTipProcessor.SetToolTip(lblCritterPowerSource, CommonFunctions.LanguageBookLong(strSource, GlobalOptions.Language) + strSpaceCharacter + LanguageManager.GetString("String_Page", GlobalOptions.Language) + ' ' + strPage);

                    nudCritterPowerRating.Enabled = objXmlPower.SelectSingleNode("rating") != null;

                    lblKarma.Text = objXmlPower.SelectSingleNode("karma")?.Value ?? "0";

                    // If the character is a Free Spirit, populate the Power Points Cost as well.
                    if (_objCharacter.Metatype == "Free Spirit")
                    {
                        XPathNavigator xmlOptionalPowerCostNode = _xmlMetatypeDataNode.SelectSingleNode("optionalpowers/power[. = \"" + objXmlPower.SelectSingleNode("name")?.Value + "\"]/@cost");
                        if (xmlOptionalPowerCostNode != null)
                        {
                            lblPowerPoints.Text = xmlOptionalPowerCostNode.Value;
                            lblPowerPoints.Visible = true;
                            lblPowerPointsLabel.Visible = true;
                        }
                    }
                }
            }
        }

        private void cboCategory_SelectedIndexChanged(object sender, EventArgs e)
        {
            trePowers.Nodes.Clear();

            string strCategory = cboCategory.SelectedValue?.ToString();

            List<string> lstPowerWhitelist = new List<string>();

            // If the Critter is only allowed certain Powers, display only those.
            XPathNavigator xmlOptionalPowers = _xmlMetatypeDataNode.SelectSingleNode("optionalpowers");
            if (xmlOptionalPowers != null)
            {
                foreach (XPathNavigator xmlNode in xmlOptionalPowers.Select("power"))
                    lstPowerWhitelist.Add(xmlNode.Value);

                // Determine if the Critter has a physical presence Power (Materialization, Possession, or Inhabitation).
                bool blnPhysicalPresence = _objCharacter.CritterPowers.Any(x => x.Name == "Materialization" || x.Name == "Possession" || x.Name == "Inhabitation");

                // Add any Critter Powers the Critter comes with that have been manually deleted so they can be re-added.
                foreach (XPathNavigator objXmlCritterPower in _xmlMetatypeDataNode.Select("powers/power"))
                {
                    bool blnAddPower = true;
                    // Make sure the Critter doesn't already have the Power.
                    foreach (CritterPower objCheckPower in _objCharacter.CritterPowers)
                    {
                        if (objCheckPower.Name == objXmlCritterPower.Value)
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
                        lstPowerWhitelist.Add(objXmlCritterPower.Value);

                        // If Manifestation is one of the Powers, also include Inhabitation and Possess if they're not already in the list.
                        if (!blnPhysicalPresence)
                        {
                            if (objXmlCritterPower.Value == "Materialization")
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
                    strFilter += " and (" + objCategoryFilter.ToString().TrimEndOnce(" or ") + ')';
                }
                if (!blnHasToxic)
                    strFilter += " and (not(toxic) or toxic != \"True\")";
            }
            foreach (XPathNavigator objXmlPower in _xmlBaseCritterPowerDataNode.Select("powers/power[" + strFilter + "]"))
            {
                string strPowerName = objXmlPower.SelectSingleNode("name")?.Value ?? LanguageManager.GetString("String_Unknown", GlobalOptions.Language);
                if (!lstPowerWhitelist.Contains(strPowerName) && lstPowerWhitelist.Count != 0) continue;
                TreeNode objNode = new TreeNode
                {
                    Tag = objXmlPower.SelectSingleNode("id")?.Value ?? string.Empty,
                    Text = objXmlPower.SelectSingleNode("translate")?.Value ?? strPowerName
                };
                trePowers.Nodes.Add(objNode);
            }
            trePowers.Sort();
        }

        private void trePowers_DoubleClick(object sender, EventArgs e)
        {
            _blnAddAgain = false;
            AcceptForm();
        }

        private void cmdOKAdd_Click(object sender, EventArgs e)
        {
            _blnAddAgain = true;
            AcceptForm();
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

            XPathNavigator objXmlPower = _xmlBaseCritterPowerDataNode.SelectSingleNode("powers/power[id = \"" + strSelectedPower + "\"]");
            if (objXmlPower == null)
                return;

            if (nudCritterPowerRating.Enabled)
                _intSelectedRating = decimal.ToInt32(nudCritterPowerRating.Value);

            s_StrSelectCategory = cboCategory.SelectedValue?.ToString() ?? string.Empty;
            _strSelectedPower = strSelectedPower;

            // If the character is a Free Spirit (PC, not the Critter version), populate the Power Points Cost as well.
            if (_objCharacter.Metatype == "Free Spirit" && !_objCharacter.IsCritter)
            {
                XPathNavigator objXmlOptionalPowerCost = _xmlMetatypeDataNode.SelectSingleNode("optionalpowers/power[. = \"" + objXmlPower.SelectSingleNode("name")?.Value + "\"]/@cost");
                if (objXmlOptionalPowerCost != null)
                    _decPowerPoints = Convert.ToDecimal(objXmlOptionalPowerCost.Value, GlobalOptions.InvariantCultureInfo);
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
        public bool AddAgain => _blnAddAgain;

        /// <summary>
        /// Criter Power that was selected in the dialogue.
        /// </summary>
        public string SelectedPower => _strSelectedPower;

        /// <summary>
        /// Rating for the Critter Power that was selected in the dialogue.
        /// </summary>
        public int SelectedRating => _intSelectedRating;

        /// <summary>
        /// Power Point cost for the Critter Power (only applies to Free Spirits).
        /// </summary>
        public decimal PowerPoints => _decPowerPoints;

        #endregion
    }
}
