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
using System.Windows.Forms;
using System.Xml;

namespace Chummer
{
    public partial class frmSelectCritterPower : Form
    {
        private string _strSelectedPower = string.Empty;
        private int _intSelectedRating = 0;
        private static string _strSelectCategory = string.Empty;
        private double _dblPowerPoints = 0.0;
        private bool _blnAddAgain = false;

        private XmlDocument _objXmlDocument = new XmlDocument();
        private XmlDocument _objXmlCritterDocument = new XmlDocument();
        private readonly Character _objCharacter;

        private List<ListItem> _lstCategory = new List<ListItem>();

        #region Control Events
        public frmSelectCritterPower(Character objCharacter)
        {
            InitializeComponent();
            LanguageManager.Instance.Load(GlobalOptions.Instance.Language, this);
            _objCharacter = objCharacter;
            MoveControls();
        }

        private void frmSelectCritterPower_Load(object sender, EventArgs e)
        {
            foreach (Label objLabel in Controls.OfType<Label>())
            {
                if (objLabel.Text.StartsWith("["))
                    objLabel.Text = string.Empty;
            }

            _objXmlDocument = XmlManager.Instance.Load("critterpowers.xml");

            // Populate the Category list.
            XmlNodeList objXmlCategoryList = _objXmlDocument.SelectNodes("/chummer/categories/category");
            foreach (XmlNode objXmlCategory in objXmlCategoryList)
            {
                ListItem objItem = new ListItem();
                objItem.Value = objXmlCategory.InnerText;
                objItem.Name = objXmlCategory.Attributes?["translate"]?.InnerText ?? objXmlCategory.InnerText;
                _lstCategory.Add(objItem);
            }

            if (_objCharacter.IsCritter)
                _objXmlCritterDocument = XmlManager.Instance.Load("critters.xml");
            else
                _objXmlCritterDocument = XmlManager.Instance.Load("metatypes.xml");
            XmlNode objXmlCritter = _objXmlCritterDocument.SelectSingleNode("/chummer/metatypes/metatype[name = \"" + _objCharacter.Metatype + "\"]");

            if (objXmlCritter == null)
            {
                _objXmlCritterDocument = XmlManager.Instance.Load("metatypes.xml");
                objXmlCritter = _objXmlCritterDocument.SelectSingleNode("/chummer/metatypes/metatype[name = \"" + _objCharacter.Metatype + "\"]");
            }

            // Remove Optional Powers if the Critter does not have access to them.
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

            bool blnIsDrake = false;
            foreach (Quality objQuality in _objCharacter.Qualities)
            {
                if (objQuality.Name == "Dracoform (Eastern Drake)" || objQuality.Name == "Dracoform (Western Drake)" ||
                    objQuality.Name == "Dracoform (Sea Drake)" || objQuality.Name == "Dracoform (Feathered Drake)")
                {
                    blnIsDrake = true;
                }
            }

            if (!blnIsDrake)
            {
                foreach (ListItem objItem in _lstCategory)
                {
                    if (objItem.Value != "Drake")
                    {
                        _lstCategory.Remove(objItem);
                        break;
                    }
                }
            }
            SortListItem objSort = new SortListItem();
            _lstCategory.Sort(objSort.Compare);
            cboCategory.BeginUpdate();
            cboCategory.DataSource = null;
            cboCategory.ValueMember = "Value";
            cboCategory.DisplayMember = "Name";
            cboCategory.DataSource = _lstCategory;
            cboCategory.EndUpdate();

            if (blnIsDrake)
            {
                foreach (ListItem objItem in cboCategory.Items)
                {
                    if (objItem.Value == "Drake")
                    {
                        cboCategory.SelectedItem = objItem;
                        cboCategory.Enabled = false;
                        break;
                    }
                }
            }
            // Select the first Category in the list.
            if (string.IsNullOrEmpty(_strSelectCategory))
                cboCategory.SelectedIndex = 0;
            else if (cboCategory.Items.Contains(_strSelectCategory))
            {
                cboCategory.SelectedValue = _strSelectCategory;
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
            if (!string.IsNullOrEmpty(trePowers.SelectedNode.Tag.ToString()))
            {
                XmlNode objXmlPower = _objXmlDocument.SelectSingleNode("/chummer/powers/power[id = \"" + trePowers.SelectedNode.Tag + "\"]");
                if (objXmlPower != null)
                {
                    if (objXmlPower["category"] != null)
                        lblCritterPowerCategory.Text = objXmlPower["category"].InnerText;

                    if (objXmlPower["type"] != null)
                    {
                        switch (objXmlPower["type"].InnerText)
                        {
                            case "M":
                                lblCritterPowerType.Text = LanguageManager.Instance.GetString("String_SpellTypeMana");
                                break;
                            case "P":
                                lblCritterPowerType.Text = LanguageManager.Instance.GetString("String_SpellTypePhysical");
                                break;
                            default:
                                lblCritterPowerType.Text = string.Empty;
                                break;
                        }
                    }

                    if (objXmlPower["action"] != null)
                    {
                        switch (objXmlPower["action"].InnerText)
                        {
                            case "Auto":
                                lblCritterPowerAction.Text = LanguageManager.Instance.GetString("String_ActionAutomatic");
                                break;
                            case "Free":
                                lblCritterPowerAction.Text = LanguageManager.Instance.GetString("String_ActionFree");
                                break;
                            case "Simple":
                                lblCritterPowerAction.Text = LanguageManager.Instance.GetString("String_ActionSimple");
                                break;
                            case "Complex":
                                lblCritterPowerAction.Text = LanguageManager.Instance.GetString("String_ActionComplex");
                                break;
                            case "Special":
                                lblCritterPowerAction.Text = LanguageManager.Instance.GetString("String_SpellDurationSpecial");
                                break;
                        }
                    }

                    if (objXmlPower["range"] != null)
                    {
                        string strRange = objXmlPower["range"].InnerText;
                        strRange = strRange.Replace("Self", LanguageManager.Instance.GetString("String_SpellRangeSelf"));
                        strRange = strRange.Replace("Special", LanguageManager.Instance.GetString("String_SpellDurationSpecial"));
                        strRange = strRange.Replace("LOS", LanguageManager.Instance.GetString("String_SpellRangeLineOfSight"));
                        strRange = strRange.Replace("LOI", LanguageManager.Instance.GetString("String_SpellRangeLineOfInfluence"));
                        strRange = strRange.Replace("T", LanguageManager.Instance.GetString("String_SpellRangeTouch"));
                        strRange = strRange.Replace("(A)", "(" + LanguageManager.Instance.GetString("String_SpellRangeArea") + ")");
                        strRange = strRange.Replace("MAG", LanguageManager.Instance.GetString("String_AttributeMAGShort"));
                        lblCritterPowerRange.Text = strRange;
                    }

                    if (objXmlPower["duration"] != null)
                    {
                        switch (objXmlPower["duration"].InnerText)
                        {
                            case "Instant":
                                lblCritterPowerDuration.Text = LanguageManager.Instance.GetString("String_SpellDurationInstantLong");
                                break;
                            case "Sustained":
                                lblCritterPowerDuration.Text = LanguageManager.Instance.GetString("String_SpellDurationSustained");
                                break;
                            case "Always":
                                lblCritterPowerDuration.Text = LanguageManager.Instance.GetString("String_SpellDurationAlways");
                                break;
                            case "Special":
                                lblCritterPowerDuration.Text = LanguageManager.Instance.GetString("String_SpellDurationSpecial");
                                break;
                            default:
                                lblCritterPowerDuration.Text = objXmlPower["duration"].InnerText;
                                break;
                        }
                    }

                    string strBook = string.Empty;
                    string strPage = string.Empty;
                    if (objXmlPower["source"] != null)
                        strBook = _objCharacter.Options.LanguageBookShort(objXmlPower["source"].InnerText);
                    if (objXmlPower["page"] != null)
                        strPage = objXmlPower["page"].InnerText;
                    if (objXmlPower["altpage"] != null)
                        strPage = objXmlPower["altpage"].InnerText;
                    lblCritterPowerSource.Text = strBook + " " + strPage;
                    if (objXmlPower["source"] != null)
                        tipTooltip.SetToolTip(lblCritterPowerSource, _objCharacter.Options.LanguageBookLong(objXmlPower["source"].InnerText) + " " + LanguageManager.Instance.GetString("String_Page") + " " + strPage);

                    nudCritterPowerRating.Enabled = objXmlPower["rating"] != null;

                    lblKarma.Text = objXmlPower["karma"] != null ? objXmlPower["karma"]?.InnerText : "0";

                    // If the character is a Free Spirit, populate the Power Points Cost as well.
                    if (_objCharacter.Metatype == "Free Spirit")
                    {
                        XmlNode objXmlCritter = _objXmlCritterDocument.SelectSingleNode("/chummer/metatypes/metatype[name = \"" + _objCharacter.Metatype + "\"]");
                        XmlNode objXmlCritterPower = objXmlCritter.SelectSingleNode("optionalpowers/power[. = \"" + trePowers.SelectedNode.Tag + "\"]");
                        lblPowerPoints.Text = objXmlCritterPower.Attributes["cost"].InnerText;
                        lblPowerPoints.Visible = true;
                        lblPowerPointsLabel.Visible = true;
                    }
                }
            }
        }

        private void cboCategory_SelectedIndexChanged(object sender, EventArgs e)
        {
            XmlNode objXmlCritter = _objXmlCritterDocument.SelectSingleNode("/chummer/metatypes/metatype[name = \"" + _objCharacter.Metatype + "\"]");

            trePowers.Nodes.Clear();

            if (cboCategory.SelectedValue.ToString() == "Toxic Critter Powers")
            {
                // Display the special Toxic Critter Powers.
                foreach (XmlNode objXmlPower in _objXmlDocument.SelectNodes("/chummer/powers/power[toxic = \"yes\"]"))
                {
                    TreeNode objNode = new TreeNode();
                    objNode.Tag = objXmlPower["id"].InnerText;
                    objNode.Text = objXmlPower["translate"]?.InnerText ?? objXmlPower["name"].InnerText;
                    trePowers.Nodes.Add(objNode);
                }
            }
            else if (cboCategory.SelectedValue.ToString() == "Weakness")
            {
                // Display the special Toxic Critter Powers.
                foreach (XmlNode objXmlPower in _objXmlDocument.SelectNodes("/chummer/powers/power[category = \"Weakness\"]"))
                {
                    TreeNode objNode = new TreeNode();
                    objNode.Tag = objXmlPower["id"].InnerText;
                    objNode.Text = objXmlPower["translate"]?.InnerText ?? objXmlPower["name"].InnerText;
                    trePowers.Nodes.Add(objNode);
                }
            }
            else
            {
                // If the Critter is only allowed certain Powers, display only those.
                if (objXmlCritter["optionalpowers"] != null)
                {
                    foreach (XmlNode objXmlCritterPower in objXmlCritter.SelectNodes("optionalpowers/power"))
                    {
                        XmlNode objXmlPower = _objXmlDocument.SelectSingleNode("/chummer/powers/power[name = \"" + objXmlCritterPower.InnerText + "\"]");
                        TreeNode objNode = new TreeNode();
                        objNode.Tag = objXmlPower["id"].InnerText;
                        objNode.Text = objXmlPower["translate"]?.InnerText ?? objXmlPower["name"].InnerText;
                        trePowers.Nodes.Add(objNode);
                    }

                    // Determine if the Critter has a physical presence Power (Materialization, Possession, or Inhabitation).
                    bool blnPhysicalPresence = false;
                    foreach (CritterPower objCheckPower in _objCharacter.CritterPowers)
                    {
                        if (objCheckPower.Name == "Materialization" || objCheckPower.Name == "Possession" || objCheckPower.Name == "Inhabitation")
                        {
                            blnPhysicalPresence = true;
                            break;
                        }
                    }

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
                            XmlNode objXmlPower = _objXmlDocument.SelectSingleNode("/chummer/powers/power[name = \"" + objXmlCritterPower.InnerText + "\"]");
                            TreeNode objNode = new TreeNode();
                            objNode.Tag = objXmlPower["id"].InnerText;
                            objNode.Text = objXmlPower["translate"]?.InnerText ?? objXmlPower["name"].InnerText;
                            trePowers.Nodes.Add(objNode);

                            // If Manifestation is one of the Powers, also include Inhabitation and Possess if they're not already in the list.
                            if (!blnPhysicalPresence){
                                if (objXmlPower["name"].InnerText == "Materialization")
                                {
                                    bool blnFound = false;
                                    foreach (TreeNode objCheckNode in trePowers.Nodes)
                                    {
                                        if (objCheckNode.Tag.ToString() == "Possession")
                                        {
                                            blnFound = true;
                                            break;
                                        }
                                    }
                                    if (!blnFound)
                                    {
                                        XmlNode objXmlPossessionPower = _objXmlDocument.SelectSingleNode("/chummer/powers/power[name = \"Possession\"]");
                                        TreeNode objPossessionNode = new TreeNode();
                                        objPossessionNode.Tag = objXmlPossessionPower["name"].InnerText;
                                        objPossessionNode.Text = objXmlPossessionPower["translate"]?.InnerText ?? objXmlPossessionPower["name"].InnerText;
                                        trePowers.Nodes.Add(objPossessionNode);
                                    }

                                    blnFound = trePowers.Nodes.Cast<TreeNode>().Any(objCheckNode => objCheckNode.Tag.ToString() == "Inhabitation");

                                    if (!blnFound)
                                    {
                                        XmlNode objXmlPossessionPower = _objXmlDocument.SelectSingleNode("/chummer/powers/power[name = \"Inhabitation\"]");
                                        TreeNode objPossessionNode = new TreeNode();
                                        objPossessionNode.Tag = objXmlPossessionPower["name"].InnerText;
                                        objPossessionNode.Text = objXmlPossessionPower["translate"]?.InnerText ?? objXmlPossessionPower["name"].InnerText;
                                        trePowers.Nodes.Add(objPossessionNode);
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    foreach (XmlNode objXmlPower in _objXmlDocument.SelectNodes("/chummer/powers/power[category = \"" + cboCategory.SelectedValue + "\"]"))
                    {
                        TreeNode objNode = new TreeNode();
                        objNode.Tag = objXmlPower["id"].InnerText;
                        objNode.Text = objXmlPower["translate"]?.InnerText ?? objXmlPower["name"].InnerText;
                        trePowers.Nodes.Add(objNode);
                    }
                }
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
            if (trePowers.SelectedNode == null)
                return;

            if (nudCritterPowerRating.Enabled)
                _intSelectedRating = Convert.ToInt32(nudCritterPowerRating.Value);
            _strSelectCategory = cboCategory.SelectedValue.ToString();
            _strSelectedPower = trePowers.SelectedNode.Tag.ToString();

            // If the character is a Free Spirit (PC, not the Critter version), populate the Power Points Cost as well.
            if (_objCharacter.Metatype == "Free Spirit" && !_objCharacter.IsCritter)
            {
                XmlNode objXmlCritter = _objXmlCritterDocument.SelectSingleNode("/chummer/metatypes/metatype[name = \"" + _objCharacter.Metatype + "\"]");
                XmlNode objXmlPower = objXmlCritter.SelectSingleNode("optionalpowers/power[. = \"" + trePowers.SelectedNode.Tag + "\"]");
                _dblPowerPoints = Convert.ToDouble(objXmlPower.Attributes["cost"].InnerText, GlobalOptions.InvariantCultureInfo);
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
        public double PowerPoints
        {
            get
            {
                return _dblPowerPoints;
            }
        }
        #endregion

        private void lblCritterPowerSource_Click(object sender, EventArgs e)
        {
            CommonFunctions.StaticOpenPDF(lblCritterPowerSource.Text, _objCharacter);
        }
    }
}
