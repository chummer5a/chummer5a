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
using System.IO;
using System.Text;

namespace Chummer
{
    public partial class frmSelectPACKSKit : Form
    {
        private string _strSelectedKit = string.Empty;
        private bool _blnAddAgain = false;
        private static string _strSelectCategory = string.Empty;
        private readonly Character _objCharacter;

        // Not readonly because content can change while form is up
        private XmlDocument _objXmlDocument = null;

        private List<ListItem> _lstCategory = new List<ListItem>();

        #region Control Events
        public frmSelectPACKSKit(Character objCharacter)
        {
            InitializeComponent();
            LanguageManager.Load(GlobalOptions.Language, this);
            _objCharacter = objCharacter;
            // Load the PACKS information.
            _objXmlDocument = XmlManager.Load("packs.xml");
        }

        private void frmSelectPACKSKit_Load(object sender, EventArgs e)
        {
            foreach (Label objLabel in Controls.OfType<Label>())
            {
                if (objLabel.Text.StartsWith("["))
                    objLabel.Text = string.Empty;
            }

            // Populate the PACKS Category list.
            XmlNodeList objXmlCategoryList = _objXmlDocument.SelectNodes("/chummer/categories/category");
            foreach (XmlNode objXmlCategory in objXmlCategoryList)
            {
                if (objXmlCategory["hide"] != null)
                    continue;
                ListItem objItem = new ListItem();
                objItem.Value = objXmlCategory.InnerText;
                objItem.Name = objXmlCategory.Attributes?["translate"]?.InnerText ?? objXmlCategory.InnerText;
                _lstCategory.Add(objItem);
            }
            cboCategory.BeginUpdate();
            cboCategory.ValueMember = "Value";
            cboCategory.DisplayMember = "Name";
            cboCategory.DataSource = _lstCategory;

            // Select the first Category in the list.
            if (string.IsNullOrEmpty(_strSelectCategory))
                cboCategory.SelectedIndex = 0;
            else
                cboCategory.SelectedValue = _strSelectCategory;

            if (cboCategory.SelectedIndex == -1)
                cboCategory.SelectedIndex = 0;
            cboCategory.EndUpdate();
        }

        private void cboCategory_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Update the list of Kits based on the selected Category.
            List<ListItem> lstKit = new List<ListItem>();

            // Retrieve the list of Kits for the selected Category.
            XmlNodeList objXmlPacksList = _objXmlDocument.SelectNodes("/chummer/packs/pack[category = \"" + cboCategory.SelectedValue + "\"]");
            foreach (XmlNode objXmlPack in objXmlPacksList)
            {
                if (objXmlPack["hide"] != null)
                    continue;
                ListItem objItem = new ListItem();
                objItem.Value = objXmlPack["name"].InnerText;
                objItem.Name = objXmlPack["translate"]?.InnerText ?? objXmlPack["name"].InnerText;
                lstKit.Add(objItem);
            }
            SortListItem objSort = new SortListItem();
            lstKit.Sort(objSort.Compare);
            lstKits.BeginUpdate();
            lstKits.DataSource = null;
            lstKits.ValueMember = "Value";
            lstKits.DisplayMember = "Name";
            lstKits.DataSource = lstKit;
            lstKits.EndUpdate();

            if (lstKits.Items.Count == 0)
                treContents.Nodes.Clear();

            if (cboCategory.SelectedValue.ToString() == "Custom")
                cmdDelete.Visible = true;
            else
                cmdDelete.Visible = false;
        }

        private void lstKits_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(lstKits.Text))
                return;

            treContents.Nodes.Clear();
            XmlNode objXmlPack = _objXmlDocument.SelectSingleNode("/chummer/packs/pack[name = \"" + lstKits.SelectedValue + "\" and category = \"" + cboCategory.SelectedValue + "\"]");
            XmlDocument objXmlItemDocument = null;

            XmlDocument objXmlGearDocument = XmlManager.Load("gear.xml");

            foreach (XmlNode objXmlItem in objXmlPack.ChildNodes)
            {
                if (objXmlItem["hide"] != null)
                    continue;
                TreeNode objParent = new TreeNode();
                switch (objXmlItem.Name)
                {
                    case "attributes":
                        objParent.Text = LanguageManager.GetString("String_SelectPACKSKit_Attributes");
                        treContents.Nodes.Add(objParent);
                        foreach (XmlNode objXmlAttribute in objXmlItem.ChildNodes)
                        {
                            if (objXmlAttribute["hide"] != null)
                                continue;
                            TreeNode objChild = new TreeNode();
                            objChild.Text = LanguageManager.GetString("String_Attribute" + objXmlAttribute.Name.ToUpper() + "Short") + " " + (Convert.ToInt32(objXmlAttribute.InnerText) - (6 - _objCharacter.GetAttribute(objXmlAttribute.Name.ToUpper()).MetatypeMaximum)).ToString();

                            objParent.Nodes.Add(objChild);
                            objParent.Expand();
                        }
                        break;
                    case "qualities":
                        objXmlItemDocument = XmlManager.Load("qualities.xml");

                        objParent.Text = LanguageManager.GetString("String_SelectPACKSKit_Qualities");
                        treContents.Nodes.Add(objParent);
                        // Positive Qualities.
                        foreach (XmlNode objXmlQuality in objXmlItem.SelectNodes("positive/quality"))
                        {
                            if (objXmlQuality["hide"] != null)
                                continue;
                            XmlNode objNode = objXmlItemDocument.SelectSingleNode("/chummer/qualities/quality[name = \"" + objXmlQuality.InnerText + "\"]");
                            if (objNode["hide"] != null)
                                continue;
                            TreeNode objChild = new TreeNode();
                            objChild.Text = objNode["translate"]?.InnerText ?? objXmlQuality.InnerText;

                            if (objXmlQuality.Attributes["select"] != null)
                                objChild.Text += $" ({LanguageManager.TranslateExtra(objXmlQuality.Attributes["select"].InnerText)})";
                            objParent.Nodes.Add(objChild);
                            objParent.Expand();
                        }

                        // Negative Qualities.
                        foreach (XmlNode objXmlQuality in objXmlItem.SelectNodes("negative/quality"))
                        {
                            if (objXmlQuality["hide"] != null)
                                continue;
                            XmlNode objNode = objXmlItemDocument.SelectSingleNode("/chummer/qualities/quality[name = \"" + objXmlQuality.InnerText + "\"]");
                            if (objNode["hide"] != null)
                                continue;
                            TreeNode objChild = new TreeNode();
                            objChild.Text = objNode["translate"]?.InnerText ?? objXmlQuality.InnerText;

                            if (objXmlQuality.Attributes["select"] != null)
                                objChild.Text += $" ({LanguageManager.TranslateExtra(objXmlQuality.Attributes["select"].InnerText)})";
                            objParent.Nodes.Add(objChild);
                            objParent.Expand();
                        }
                        break;
                    case "nuyenbp":
                        objParent.Text = LanguageManager.GetString("String_SelectPACKSKit_Nuyen");
                        treContents.Nodes.Add(objParent);
                        TreeNode objNuyenChild = new TreeNode();
                        objNuyenChild.Text = LanguageManager.GetString("String_SelectPACKSKit_StartingNuyenBP") + " " + objXmlItem.InnerText;
                        objParent.Nodes.Add(objNuyenChild);
                        objParent.Expand();
                        break;
                    case "skills":
                        objXmlItemDocument = XmlManager.Load("skills.xml");

                        objParent.Text = LanguageManager.GetString("String_SelectPACKSKit_Skills");
                        treContents.Nodes.Add(objParent);
                        foreach (XmlNode objXmlSkill in objXmlItem.SelectNodes("skill"))
                        {
                            if (objXmlSkill["hide"] != null)
                                continue;
                            XmlNode objNode = objXmlItemDocument.SelectSingleNode("/chummer/skills/skill[name = \"" + objXmlSkill["name"].InnerText + "\"]");
                            if (objNode["hide"] != null)
                                continue;
                            TreeNode objChild = new TreeNode();
                            objChild.Text = objNode["translate"]?.InnerText ?? objXmlSkill["name"].InnerText;
                            objChild.Text += " " + objXmlSkill["rating"].InnerText;

                            if (objXmlSkill["spec"] != null)
                                objChild.Text += " (" + objXmlSkill["spec"].InnerText + ")";
                            objParent.Nodes.Add(objChild);
                            objParent.Expand();
                        }
                        foreach (XmlNode objXmlSkill in objXmlItem.SelectNodes("skillgroup"))
                        {
                            if (objXmlSkill["hide"] != null)
                                continue;
                            XmlNode objNode = objXmlItemDocument.SelectSingleNode("/chummer/skillgroups/name[. = \"" + objXmlSkill["name"].InnerText + "\"]");
                            if (objNode["hide"] != null)
                                continue;
                            TreeNode objChild = new TreeNode();
                            objChild.Text = objNode.Attributes["translate"]?.InnerText ?? objXmlSkill["name"].InnerText;
                            objChild.Text += $" {LanguageManager.GetString("String_SelectPACKSKit_Group")} {objXmlSkill["rating"].InnerText}";

                            if (objXmlSkill["spec"] != null)
                                objChild.Text += $" ({objXmlSkill["spec"].InnerText})";
                            objParent.Nodes.Add(objChild);
                            objParent.Expand();
                        }
                        break;
                    case "knowledgeskills":
                        objXmlItemDocument = XmlManager.Load("skills.xml");

                        objParent.Text = LanguageManager.GetString("String_SelectPACKSKit_KnowledgeSkills");
                        treContents.Nodes.Add(objParent);
                        foreach (XmlNode objXmlSkill in objXmlItem.SelectNodes("skill"))
                        {
                            if (objXmlSkill["hide"] != null)
                                continue;
                            TreeNode objChild = new TreeNode();
                            if (objXmlSkill["name"] != null)
                            {
                                XmlNode objNode = objXmlItemDocument.SelectSingleNode("/chummer/knowledgeskills/skill[name = \"" + objXmlSkill["name"].InnerText + "\"]");
                                if (objNode["hide"] != null)
                                    continue;
                                objChild.Text = objNode?["translate"]?.InnerText ?? objXmlSkill["name"].InnerText;
                            }
                            objChild.Text += " " + objXmlSkill["rating"].InnerText;

                            if (objXmlSkill["spec"] != null)
                                objChild.Text += $" ({objXmlSkill["spec"].InnerText})";
                            objParent.Nodes.Add(objChild);
                            objParent.Expand();
                        }
                        break;
                    case "selectmartialart":
                        objParent.Text = LanguageManager.GetString("String_SelectPACKSKit_SelectMartialArt");
                        treContents.Nodes.Add(objParent);

                        int intRating = 1;
                        string strSelect = LanguageManager.GetString("String_SelectPACKSKit_SelectMartialArt");
                        if (objXmlItem.Attributes["select"] != null)
                            strSelect = objXmlItem.Attributes["select"].InnerText;
                        if (objXmlItem.Attributes["rating"] != null)
                            intRating = Convert.ToInt32(objXmlItem.Attributes["rating"].InnerText);

                        TreeNode objMartialArt = new TreeNode();
                        objMartialArt.Text = strSelect + " " + LanguageManager.GetString("String_Rating") + " " + intRating.ToString();
                        objParent.Nodes.Add(objMartialArt);
                        objParent.Expand();
                        break;
                    case "martialarts":
                        objXmlItemDocument = XmlManager.Load("martialarts.xml");

                        objParent.Text = LanguageManager.GetString("String_SelectPACKSKit_MartialArts");
                        treContents.Nodes.Add(objParent);
                        foreach (XmlNode objXmlArt in objXmlItem.SelectNodes("martialart"))
                        {
                            if (objXmlArt["hide"] != null)
                                continue;
                            XmlNode objNode = objXmlItemDocument.SelectSingleNode("/chummer/martialarts/martialart[name = \"" + objXmlArt["name"].InnerText + "\"]");
                            if (objNode["hide"] != null)
                                continue;
                            TreeNode objChild = new TreeNode();
                            objChild.Text = objNode["translate"]?.InnerText ?? objXmlArt["name"].InnerText;
                            objChild.Text += " " + objXmlArt["rating"].InnerText;

                            // Check for Advantages.
                            foreach (XmlNode objXmlAdvantage in objXmlArt.SelectNodes("techniques/technique"))
                            {
                                if (objXmlAdvantage["hide"] != null)
                                    continue;
                                XmlNode objChildNode = objXmlItemDocument.SelectSingleNode("/chummer/martialarts/martialart[name = \"" + objXmlArt["name"].InnerText + "\"]/techniques/technique[name = \"" + objXmlAdvantage.InnerText + "\"]");
                                if (objNode["hide"] != null)
                                    continue;
                                TreeNode objChildChild = new TreeNode();
                                objChildChild.Text = objChildNode.Attributes["translate"]?.InnerText ?? objXmlAdvantage["name"].InnerText;

                                objChild.Nodes.Add(objChildChild);
                                objChild.Expand();
                            }

                            objParent.Nodes.Add(objChild);
                            objParent.Expand();
                        }

                        foreach (XmlNode objXmlManeuver in objXmlItem.SelectNodes("maneuver"))
                        {
                            if (objXmlManeuver["hide"] != null)
                                continue;
                            XmlNode objNode = objXmlItemDocument.SelectSingleNode("/chummer/maneuvers/maneuver[name = \"" + objXmlManeuver.InnerText + "\"]");
                            if (objNode["hide"] != null)
                                continue;
                            TreeNode objChild = new TreeNode();
                            objChild.Text = objNode["translate"]?.InnerText ?? objXmlManeuver["name"].InnerText;
                            objChild.Text += " " + objXmlManeuver["rating"].InnerText;

                            objParent.Nodes.Add(objChild);
                            objParent.Expand();
                        }
                        break;
                    case "powers":
                        objXmlItemDocument = XmlManager.Load("powers.xml");

                        objParent.Text = LanguageManager.GetString("String_SelectPACKSKit_Powers");
                        treContents.Nodes.Add(objParent);
                        foreach (XmlNode objXmlPower in objXmlItem.SelectNodes("power"))
                        {
                            if (objXmlPower["hide"] != null)
                                continue;
                            XmlNode objNode = objXmlItemDocument.SelectSingleNode("/chummer/powers/power[name = \"" + objXmlPower["name"].InnerText + "\"]");
                            if (objNode["hide"] != null)
                                continue;
                            TreeNode objChild = new TreeNode();
                            objChild.Text = objNode["translate"]?.InnerText ?? objXmlPower["name"].InnerText;

                            if (objXmlPower["name"].Attributes["select"] != null)
                                objChild.Text += " (" + objXmlPower["name"].Attributes["select"].InnerText + ")";
                            if (objXmlPower["rating"] != null)
                                objChild.Text += " " + objXmlPower["rating"].InnerText;
                            objParent.Nodes.Add(objChild);
                            objParent.Expand();
                        }
                        break;
                    case "programs":
                        objXmlItemDocument = XmlManager.Load("complexforms.xml");

                        objParent.Text = LanguageManager.GetString("String_SelectPACKSKit_Programs");
                        treContents.Nodes.Add(objParent);
                        foreach (XmlNode objXmlProgram in objXmlItem.SelectNodes("program"))
                        {
                            if (objXmlProgram["hide"] != null)
                                continue;
                            XmlNode objNode = objXmlItemDocument.SelectSingleNode("/chummer/complexforms/complexform[name = \"" + objXmlProgram["name"].InnerText + "\"]");
                            if (objNode["hide"] != null)
                                continue;
                            TreeNode objChild = new TreeNode();
                            objChild.Text = objNode["translate"]?.InnerText ?? objXmlProgram["name"].InnerText;
                            objChild.Text += " " + objXmlProgram["rating"].InnerText;

                            // Check for Program Options.
                            foreach (XmlNode objXmlOption in objXmlProgram.SelectNodes("options/option"))
                            {
                                if (objXmlOption["hide"] != null)
                                    continue;
                                XmlNode objChildNode = objXmlItemDocument.SelectSingleNode("/chummer/options/option[name = \"" + objXmlOption["name"].InnerText + "\"]");
                                if (objNode["hide"] != null)
                                    continue;
                                TreeNode objChildChild = new TreeNode();
                                objChildChild.Text = objChildNode["translate"]?.InnerText ?? objXmlOption["name"].InnerText;

                                objChildChild.Text = objXmlOption["name"].InnerText;
                                if (objXmlOption["rating"] != null)
                                    objChildChild.Text += " " + objXmlOption["rating"].InnerText;
                                objChild.Nodes.Add(objChildChild);
                                objChild.Expand();
                            }

                            objParent.Nodes.Add(objChild);
                            objParent.Expand();
                        }
                        break;
                    case "spells":
                        objXmlItemDocument = XmlManager.Load("spells.xml");

                        objParent.Text = LanguageManager.GetString("String_SelectPACKSKit_Spells");
                        treContents.Nodes.Add(objParent);
                        foreach (XmlNode objXmlSpell in objXmlItem.SelectNodes("spell"))
                        {
                            if (objXmlSpell["hide"] != null)
                                continue;
                            XmlNode objNode = objXmlItemDocument.SelectSingleNode("/chummer/spells/spell[name = \"" + objXmlSpell.InnerText + "\"]");
                            if (objNode["hide"] != null)
                                continue;
                            TreeNode objChild = new TreeNode();
                            objChild.Text = objNode["translate"]?.InnerText ?? objXmlSpell.InnerText;

                            if (objXmlSpell.Attributes["select"] != null)
                                objChild.Text += " (" + objXmlSpell.Attributes["select"].InnerText + ")";
                            objParent.Nodes.Add(objChild);
                            objParent.Expand();
                        }
                        break;
                    case "spirits":

                        objParent.Text = LanguageManager.GetString("String_SelectPACKSKit_Spirits");
                        treContents.Nodes.Add(objParent);
                        foreach (XmlNode objXmlSpirit in objXmlItem.SelectNodes("spirit"))
                        {
                            if (objXmlSpirit["hide"] != null)
                                continue;
                            TreeNode objChild = new TreeNode();
                            objChild.Text = objXmlSpirit["name"].InnerText + " (" + LanguageManager.GetString("Label_Spirit_Force") + " " + objXmlSpirit["force"].InnerText + ", " + LanguageManager.GetString("Label_Spirit_ServicesOwed") + " " + objXmlSpirit["services"].InnerText + ")";
                            objParent.Nodes.Add(objChild);
                            objParent.Expand();
                        }
                        break;
                    case "lifestyles":

                        objParent.Text = LanguageManager.GetString("String_SelectPACKSKit_Lifestyles");
                        treContents.Nodes.Add(objParent);
                        foreach (XmlNode objXmlLifestyle in objXmlItem.SelectNodes("lifestyle"))
                        {
                            if (objXmlLifestyle["hide"] != null)
                                continue;
                            TreeNode objChild = new TreeNode();
                            objChild.Text = string.Format("{0} {1} {2}", objXmlLifestyle["name"].InnerText, objXmlLifestyle["months"].InnerText, LanguageManager.GetString("Label_LifestyleMonths"));

                            // Check for Qualities.
                            foreach (XmlNode objXmlQuality in objXmlLifestyle.SelectNodes("qualities/quality"))
                            {
                                if (objXmlQuality["hide"] != null)
                                    continue;
                                TreeNode objChildChild = new TreeNode();
                                objChildChild.Text = objXmlQuality.InnerText;
                                objChild.Nodes.Add(objChildChild);
                                objChild.Expand();
                            }

                            objParent.Nodes.Add(objChild);
                            objParent.Expand();
                        }
                        break;
                    case "cyberwares":
                        objXmlItemDocument = XmlManager.Load("cyberware.xml");

                        objParent.Text = LanguageManager.GetString("String_SelectPACKSKit_Cyberware");
                        treContents.Nodes.Add(objParent);
                        foreach (XmlNode objXmlCyberware in objXmlItem.SelectNodes("cyberware"))
                        {
                            if (objXmlCyberware["hide"] != null)
                                continue;
                            XmlNode objNode = objXmlItemDocument.SelectSingleNode("/chummer/cyberwares/cyberware[name = \"" + objXmlCyberware["name"].InnerText + "\"]");
                            if (objNode["hide"] != null)
                                continue;
                            TreeNode objChild = new TreeNode();
                            objChild.Text = objNode["translate"]?.InnerText ?? objXmlCyberware["name"].InnerText;

                            if (objXmlCyberware["rating"] != null)
                                objChild.Text += " Rating " + objXmlCyberware["rating"].InnerText;
                            objChild.Text += " (" + objXmlCyberware["grade"].InnerText + ")";

                            // Check for children.
                            foreach (XmlNode objXmlChild in objXmlCyberware.SelectNodes("cyberwares/cyberware"))
                            {
                                if (objXmlChild["hide"] != null)
                                    continue;
                                XmlNode objChildNode = objXmlItemDocument.SelectSingleNode("/chummer/cyberwares/cyberware[name = \"" + objXmlChild["name"].InnerText + "\"]");
                                if (objChildNode["hide"] != null)
                                    continue;
                                TreeNode objChildChild = new TreeNode();
                                objChildChild.Text = objChildNode["translate"]?.InnerText ?? objXmlChild["name"].InnerText;

                                if (objXmlChild["rating"] != null)
                                    objChildChild.Text += " " + LanguageManager.GetString("String_Rating") + " " + objXmlChild["rating"].InnerText;

                                foreach (XmlNode objXmlGearNode in objXmlChild.SelectNodes("gears/gear"))
                                    WriteGear(objXmlGearDocument, objXmlGearNode, objChildChild);
                                objChild.Expand();

                                objChild.Nodes.Add(objChildChild);
                                objChild.Expand();
                            }

                            foreach (XmlNode objXmlGearNode in objXmlCyberware.SelectNodes("gears/gear"))
                                WriteGear(objXmlGearDocument, objXmlGearNode, objChild);
                            objChild.Expand();

                            objParent.Nodes.Add(objChild);
                            objParent.Expand();
                        }
                        break;
                    case "biowares":
                        objXmlItemDocument = XmlManager.Load("bioware.xml");

                        objParent.Text = LanguageManager.GetString("String_SelectPACKSKit_Bioware");
                        treContents.Nodes.Add(objParent);
                        foreach (XmlNode objXmlBioware in objXmlItem.SelectNodes("bioware"))
                        {
                            if (objXmlBioware["hide"] != null)
                                continue;
                            XmlNode objNode = objXmlItemDocument.SelectSingleNode("/chummer/biowares/bioware[name = \"" + objXmlBioware["name"].InnerText + "\"]");
                            if (objNode["hide"] != null)
                                continue;
                            TreeNode objChild = new TreeNode();
                            objChild.Text = objNode["translate"]?.InnerText ?? objXmlBioware["name"].InnerText;

                            if (objXmlBioware["rating"] != null)
                                objChild.Text += " " + LanguageManager.GetString("String_Rating") + " " + objXmlBioware["rating"].InnerText;
                            objChild.Text += " (" + objXmlBioware["grade"].InnerText + ")";

                            foreach (XmlNode objXmlGearNode in objXmlBioware.SelectNodes("gears/gear"))
                                WriteGear(objXmlGearDocument, objXmlGearNode, objChild);
                            objChild.Expand();

                            objParent.Nodes.Add(objChild);
                            objParent.Expand();
                        }
                        break;
                    case "armors":
                        objXmlItemDocument = XmlManager.Load("armor.xml");

                        objParent.Text = LanguageManager.GetString("String_SelectPACKSKit_Armor");
                        treContents.Nodes.Add(objParent);
                        foreach (XmlNode objXmlArmor in objXmlItem.SelectNodes("armor"))
                        {
                            if (objXmlArmor["hide"] != null)
                                continue;
                            XmlNode objNode = objXmlItemDocument.SelectSingleNode("/chummer/armors/armor[name = \"" + objXmlArmor["name"].InnerText + "\"]");
                            if (objNode["hide"] != null)
                                continue;
                            TreeNode objChild = new TreeNode();
                            objChild.Text = objNode["translate"]?.InnerText ?? objXmlArmor["name"].InnerText;

                            // Check for children.
                            foreach (XmlNode objXmlChild in objXmlArmor.SelectNodes("mods/mod"))
                            {
                                if (objXmlChild["hide"] != null)
                                    continue;
                                XmlNode objChildNode = objXmlItemDocument.SelectSingleNode("/chummer/mods/mod[name = \"" + objXmlChild["name"].InnerText + "\"]");
                                if (objChildNode["hide"] != null)
                                    continue;
                                TreeNode objChildChild = new TreeNode();
                                objChildChild.Text = objChildNode["translate"]?.InnerText ?? objXmlChild["name"].InnerText;

                                if (objXmlChild["rating"] != null)
                                    objChildChild.Text += " " + LanguageManager.GetString("String_Rating") + " " + objXmlChild["rating"].InnerText;
                                objChild.Nodes.Add(objChildChild);
                                objChild.Expand();
                            }

                            foreach (XmlNode objXmlGearNode in objXmlArmor.SelectNodes("gears/gear"))
                                WriteGear(objXmlGearDocument, objXmlGearNode, objChild);
                            objChild.Expand();

                            objParent.Nodes.Add(objChild);
                            objParent.Expand();
                        }
                        break;
                    case "weapons":
                        objXmlItemDocument = XmlManager.Load("weapons.xml");

                        objParent.Text = LanguageManager.GetString("String_SelectPACKSKit_Weapons");
                        treContents.Nodes.Add(objParent);
                        foreach (XmlNode objXmlWeapon in objXmlItem.SelectNodes("weapon"))
                        {
                            if (objXmlWeapon["hide"] != null)
                                continue;
                            XmlNode objNode = objXmlItemDocument.SelectSingleNode("/chummer/weapons/weapon[name = \"" + objXmlWeapon["name"].InnerText + "\"]");
                            if (objNode["hide"] != null)
                                continue;
                            TreeNode objChild = new TreeNode();
                            objChild.Text = objNode["translate"]?.InnerText ?? objXmlWeapon["name"].InnerText;

                            // Check for Weapon Accessories.
                            foreach (XmlNode objXmlAccessory in objXmlWeapon.SelectNodes("accessories/accessory"))
                            {
                                if (objXmlAccessory["hide"] != null)
                                    continue;
                                XmlNode objChildNode = objXmlItemDocument.SelectSingleNode("/chummer/accessories/accessory[name = \"" + objXmlAccessory["name"].InnerText + "\"]");
                                if (objChildNode["hide"] != null)
                                    continue;
                                TreeNode objChildChild = new TreeNode();
                                objChildChild.Text = objChildNode["translate"]?.InnerText ?? objXmlAccessory["name"].InnerText;

                                if (objXmlAccessory["rating"] != null)
                                    objChildChild.Text += " " + LanguageManager.GetString("String_Rating") + " " + objXmlAccessory["rating"].InnerText;

                                foreach (XmlNode objXmlGearNode in objXmlAccessory.SelectNodes("gears/gear"))
                                    WriteGear(objXmlGearDocument, objXmlGearNode, objChildChild);
                                objChildChild.Expand();

                                objChild.Nodes.Add(objChildChild);
                                objChild.Expand();
                            }

                            // Check for Weapon Mods.
                            foreach (XmlNode objXmlMod in objXmlWeapon.SelectNodes("mods/mod"))
                            {
                                if (objXmlMod["hide"] != null)
                                    continue;
                                XmlNode objChildNode = objXmlItemDocument.SelectSingleNode("/chummer/mods/mod[name = \"" + objXmlMod["name"].InnerText + "\"]");
                                if (objChildNode["hide"] != null)
                                    continue;
                                TreeNode objChildChild = new TreeNode();
                                objChildChild.Text = objChildNode["translate"]?.InnerText ?? objXmlMod["name"].InnerText;

                                if (objXmlMod["rating"] != null)
                                    objChildChild.Text += " " + LanguageManager.GetString("String_Rating") + " " + objXmlMod["rating"].InnerText;
                                objChild.Nodes.Add(objChildChild);
                                objChild.Expand();
                            }

                            // Check for Underbarrel Weapons.
                            if (objXmlWeapon["underbarrel"] != null)
                            {
                                if (objXmlWeapon["hide"] != null)
                                    continue;
                                XmlNode objChildNode = objXmlItemDocument.SelectSingleNode("/chummer/weapons/weapon[name = \"" + objXmlWeapon["underbarrel"].InnerText + "\"]");
                                if (objChildNode["hide"] != null)
                                    continue;
                                TreeNode objChildChild = new TreeNode();
                                objChildChild.Text = objChildNode["translate"]?.InnerText ?? objXmlWeapon["name"].InnerText;

                                objChild.Nodes.Add(objChildChild);
                                objChild.Expand();
                            }

                            objParent.Nodes.Add(objChild);
                            objParent.Expand();
                        }
                        break;
                    case "gears":
                        objXmlItemDocument = XmlManager.Load("gear.xml");

                        objParent.Text = LanguageManager.GetString("String_SelectPACKSKit_Gear");
                        treContents.Nodes.Add(objParent);
                        foreach (XmlNode objXmlGear in objXmlItem.SelectNodes("gear"))
                        {
                            if (objXmlGear["hide"] != null)
                                continue;
                            WriteGear(objXmlItemDocument, objXmlGear, objParent);
                            objParent.Expand();
                        }
                        break;
                    case "vehicles":
                        objXmlItemDocument = XmlManager.Load("vehicles.xml");

                        objParent.Text = LanguageManager.GetString("String_SelectPACKSKit_Vehicles");
                        treContents.Nodes.Add(objParent);
                        foreach (XmlNode objXmlVehicle in objXmlItem.SelectNodes("vehicle"))
                        {
                            if (objXmlVehicle["hide"] != null)
                                continue;
                            XmlNode objNode = objXmlItemDocument.SelectSingleNode("/chummer/vehicles/vehicle[name = \"" + objXmlVehicle["name"].InnerText + "\"]");
                            if (objNode["hide"] != null)
                                continue;
                            TreeNode objChild = new TreeNode();
                            objChild.Text = objNode["translate"]?.InnerText ?? objXmlVehicle["name"].InnerText;

                            // Check for children.
                            foreach (XmlNode objXmlMod in objXmlVehicle.SelectNodes("mods/mod"))
                            {
                                if (objXmlMod["hide"] != null)
                                    continue;
                                XmlNode objChildNode = objXmlItemDocument.SelectSingleNode("/chummer/mods/mod[name = \"" + objXmlMod["name"].InnerText + "\"]");
                                if (objChildNode["hide"] != null)
                                    continue;
                                TreeNode objChildChild = new TreeNode();
                                objChildChild.Text = objChildNode["translate"]?.InnerText ?? objXmlMod["name"].InnerText;

                                if (objXmlMod["rating"] != null)
                                    objChildChild.Text += " " + LanguageManager.GetString("String_Rating") + " " + objXmlMod["rating"].InnerText;
                                objChild.Nodes.Add(objChildChild);
                                objChild.Expand();
                            }

                            // Check for children.
                            foreach (XmlNode objXmlChild in objXmlVehicle.SelectNodes("gears/gear"))
                            {
                                if (objXmlChild["hide"] != null)
                                    continue;
                                WriteGear(objXmlGearDocument, objXmlChild, objChild);
                                objChild.Expand();
                            }

                            // Check for children.
                            foreach (XmlNode objXmlWeapon in objXmlVehicle.SelectNodes("weapons/weapon"))
                            {
                                if (objXmlWeapon["hide"] != null)
                                    continue;
                                XmlDocument objXmlWeaponDocument = XmlManager.Load("weapons.xml");
                                XmlNode objChildNode = objXmlWeaponDocument.SelectSingleNode("/chummer/weapons/weapon[name = \"" + objXmlWeapon["name"].InnerText + "\"]");
                                if (objChildNode["hide"] != null)
                                    continue;
                                TreeNode objChildChild = new TreeNode();
                                objChildChild.Text = objChildNode["translate"]?.InnerText ?? objXmlWeapon["name"].InnerText;

                                if (objXmlWeapon["rating"] != null)
                                    objChildChild.Text += " " + LanguageManager.GetString("String_Rating") + " " + objXmlWeapon["rating"].InnerText;
                                objChild.Nodes.Add(objChildChild);
                                objChild.Expand();
                            }

                            objParent.Nodes.Add(objChild);
                            objParent.Expand();
                        }
                        break;
                }
            }
        }

        private void cmdOK_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(lstKits.Text))
                AcceptForm();
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void lstKits_DoubleClick(object sender, EventArgs e)
        {
            cmdOK_Click(sender, e);
        }

        private void cmdOKAdd_Click(object sender, EventArgs e)
        {
            _blnAddAgain = true;
            cmdOK_Click(sender, e);
        }

        private void cmdDelete_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(lstKits.Text))
                return;

            if (MessageBox.Show(LanguageManager.GetString("Message_DeletePACKSKit").Replace("{0}", lstKits.Text), LanguageManager.GetString("MessageTitle_Delete"), MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.No)
                return;

            // Delete the selectec custom PACKS Kit.
            // Find a custom PACKS Kit with the name. This is done without the XmlManager since we need to check each file individually.
            XmlDocument objXmlDocument = new XmlDocument();
            string strCustomPath = Path.Combine(Application.StartupPath, "data");
            foreach (string strFile in Directory.GetFiles(strCustomPath, "custom*_packs.xml"))
            {
                objXmlDocument.Load(strFile);
                XmlNodeList objXmlPACKSList = objXmlDocument.SelectNodes("/chummer/packs/pack[name = \"" + lstKits.SelectedValue + "\" and category = \"Custom\"]");
                if (objXmlPACKSList.Count > 0)
                {
                    // Read in the entire file.
                    XmlDocument objXmlCurrentDocument = new XmlDocument();
                    objXmlCurrentDocument.Load(strFile);

                    FileStream objStream = new FileStream(strFile, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
                    XmlTextWriter objWriter = new XmlTextWriter(objStream, Encoding.Unicode);
                    objWriter.Formatting = Formatting.Indented;
                    objWriter.Indentation = 1;
                    objWriter.IndentChar = '\t';
                    objWriter.WriteStartDocument();

                    // <chummer>
                    objWriter.WriteStartElement("chummer");
                    // <packs>
                    objWriter.WriteStartElement("packs");

                    // If this is not a new file, write out the current contents.
                    XmlNodeList objXmlNodeList = objXmlCurrentDocument.SelectNodes("/chummer/packs/*");
                    foreach (XmlNode objXmlNode in objXmlNodeList)
                    {
                        if (objXmlNode["name"].InnerText != lstKits.SelectedValue.ToString())
                        {
                            // <pack>
                            objWriter.WriteStartElement("pack");
                            objXmlNode.WriteContentTo(objWriter);
                            // </pack>
                            objWriter.WriteEndElement();
                        }
                    }

                    // </packs>
                    objWriter.WriteEndElement();
                    // </chummer>
                    objWriter.WriteEndElement();

                    objWriter.WriteEndDocument();
                    objWriter.Close();
                    objStream.Close();
                }
            }

            // Reload the PACKS files since they have changed.
            _objXmlDocument = XmlManager.Load("packs.xml");
            cboCategory_SelectedIndexChanged(sender, e);
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
        /// Name of Kit that was selected in the dialogue.
        /// </summary>
        public string SelectedKit
        {
            get
            {
                return _strSelectedKit;
            }
        }

        /// <summary>
        /// Category that was selected in the dialogue.
        /// </summary>
        public string SelectedCategory
        {
            get
            {
                return _strSelectCategory;
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Accept the selected item and close the form.
        /// </summary>
        private void AcceptForm()
        {
            _strSelectedKit = lstKits.SelectedValue.ToString();
            _strSelectCategory = cboCategory.SelectedValue.ToString();
            DialogResult = DialogResult.OK;
        }

        private void WriteGear(XmlDocument objXmlItemDocument, XmlNode objXmlGear, TreeNode objParent)
        {
            XmlNode objNode;

            if (objXmlGear["category"] != null)
                objNode = objXmlItemDocument.SelectSingleNode("/chummer/gears/gear[name = \"" + objXmlGear["name"].InnerText + "\" and category = \"" + objXmlGear["category"].InnerText + "\"]");
            else
                objNode = objXmlItemDocument.SelectSingleNode("/chummer/gears/gear[name = \"" + objXmlGear["name"].InnerText + "\"]");

            TreeNode objChild = new TreeNode();
            if (objNode != null) {
            objChild.Text = objNode["translate"]?.InnerText ?? objXmlGear["name"].InnerText;

            if (objXmlGear["name"].Attributes["select"] != null)
                objChild.Text += " (" + objXmlGear["name"].Attributes["select"].InnerText + ")";
            if (objXmlGear["rating"] != null)
                objChild.Text += " " + LanguageManager.GetString("String_Rating") + " " + objXmlGear["rating"].InnerText;
            if (objXmlGear["qty"] != null)
                objChild.Text += " x" + objXmlGear["qty"].InnerText;

            objParent.Nodes.Add(objChild);
            }

            // Check for children.
            foreach (XmlNode objXmlChild in objXmlGear.SelectNodes("gears/gear"))
            {
                WriteGear(objXmlItemDocument, objXmlChild, objChild);
            }

            objChild.Expand();
        }
        #endregion
    }
}
