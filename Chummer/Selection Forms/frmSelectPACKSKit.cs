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
using System.IO;
using System.Text;
 using System.Xml.XPath;

namespace Chummer
{
    public partial class frmSelectPACKSKit : Form
    {
        private string _strSelectedKit = string.Empty;
        private bool _blnAddAgain;
        private static string s_StrSelectCategory = string.Empty;
        private readonly Character _objCharacter;

        // Not readonly because content can change while form is up
        private XPathNavigator _xmlBaseChummerNode;
        private readonly XPathNavigator _xmlGearsBaseGearsNode;
        private readonly XPathNavigator _xmlBiowareBaseChummerNode;
        private readonly XPathNavigator _xmlCyberwareBaseChummerNode;
        private readonly XPathNavigator _xmlWeaponsBaseChummerNode;
        private readonly XPathNavigator _xmlArmorBaseChummerNode;
        private readonly XPathNavigator _xmlQualitiesBaseQualitiesNode;
        private readonly XPathNavigator _xmlSkillsBaseChummerNode;
        private readonly XPathNavigator _xmlSpellsBaseSpellsNode;
        private readonly XPathNavigator _xmlComplexFormsBaseChummerNode;
        private readonly XPathNavigator _xmlVehiclesBaseChummerNode;
        private readonly XPathNavigator _xmlPowersBasePowersNode;
        private readonly XPathNavigator _xmlMartialArtsBaseChummerNode;

        private readonly List<ListItem> _lstCategory = new List<ListItem>();

        #region Control Events
        public frmSelectPACKSKit(Character objCharacter)
        {
            InitializeComponent();
            this.UpdateLightDarkMode();
            this.TranslateWinForm();
            _objCharacter = objCharacter;
            // Load the PACKS information.
            _xmlBaseChummerNode = _objCharacter.LoadDataXPath("packs.xml").SelectSingleNode("/chummer");
            _xmlGearsBaseGearsNode = _objCharacter.LoadDataXPath("gear.xml").SelectSingleNode("/chummer/gears");
            _xmlWeaponsBaseChummerNode = _objCharacter.LoadDataXPath("weapons.xml").SelectSingleNode("/chummer");
            _xmlArmorBaseChummerNode = _objCharacter.LoadDataXPath("armor.xml").SelectSingleNode("/chummer");
            _xmlQualitiesBaseQualitiesNode = _objCharacter.LoadDataXPath("qualities.xml").SelectSingleNode("/chummer/qualities");
            _xmlSkillsBaseChummerNode = _objCharacter.LoadDataXPath("skills.xml").SelectSingleNode("/chummer");
            _xmlSpellsBaseSpellsNode = _objCharacter.LoadDataXPath("spells.xml").SelectSingleNode("/chummer/spells");
            _xmlComplexFormsBaseChummerNode = _objCharacter.LoadDataXPath("complexforms.xml").SelectSingleNode("/chummer");
            _xmlVehiclesBaseChummerNode = _objCharacter.LoadDataXPath("vehicles.xml").SelectSingleNode("/chummer");
            _xmlBiowareBaseChummerNode = _objCharacter.LoadDataXPath("bioware.xml").SelectSingleNode("/chummer");
            _xmlCyberwareBaseChummerNode = _objCharacter.LoadDataXPath("cyberware.xml").SelectSingleNode("/chummer");
            _xmlPowersBasePowersNode = _objCharacter.LoadDataXPath("powers.xml").SelectSingleNode("/chummer/powers");
            _xmlMartialArtsBaseChummerNode = _objCharacter.LoadDataXPath("martialarts.xml").SelectSingleNode("/chummer");
        }

        private void frmSelectPACKSKit_Load(object sender, EventArgs e)
        {
            // Populate the PACKS Category list.
            foreach (XPathNavigator objXmlCategory in _xmlBaseChummerNode.Select("categories/category[not(hide)]"))
            {
                string strInnerText = objXmlCategory.Value;
                _lstCategory.Add(new ListItem(strInnerText, objXmlCategory.SelectSingleNode("@translate")?.Value ?? strInnerText));
            }
            _lstCategory.Sort(CompareListItems.CompareNames);

            if (_lstCategory.Count > 0)
            {
                _lstCategory.Insert(0, new ListItem("Show All", LanguageManager.GetString("String_ShowAll")));
            }

            cboCategory.BeginUpdate();
            cboCategory.PopulateWithListItems(_lstCategory);
            // Select the first Category in the list.
            if (!string.IsNullOrEmpty(s_StrSelectCategory))
                cboCategory.SelectedValue = s_StrSelectCategory;
            if (cboCategory.SelectedIndex == -1)
                cboCategory.SelectedIndex = 0;
            cboCategory.EndUpdate();
        }

        private void cboCategory_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Update the list of Kits based on the selected Category.
            List<ListItem> lstKit = new List<ListItem>();

            string strFilter = "not(hide)";
            string strCategory = cboCategory.SelectedValue?.ToString();
            if (!string.IsNullOrEmpty(strCategory) && strCategory != "Show All")
                strFilter += " and category = " + cboCategory.SelectedValue.ToString().CleanXPath();
            else
            {
                StringBuilder objCategoryFilter = new StringBuilder();
                foreach (string strItem in _lstCategory.Select(x => x.Value))
                {
                    if (!string.IsNullOrEmpty(strItem))
                        objCategoryFilter.Append("category = " + strItem.CleanXPath() + " or ");
                }
                if (objCategoryFilter.Length > 0)
                {
                    strFilter += " and (" + objCategoryFilter.ToString().TrimEndOnce(" or ") + ')';
                }
            }

            // Retrieve the list of Kits for the selected Category.
            foreach (XPathNavigator objXmlPack in _xmlBaseChummerNode.Select("packs/pack[" + strFilter + "]"))
            {
                string strName = objXmlPack.SelectSingleNode("name")?.Value;
                // Separator "<" is a hack because XML does not like it when the '<' character is used in element contents, so we can safely assume that it will never show up.
                lstKit.Add(new ListItem(strName + '<' + objXmlPack.SelectSingleNode("category")?.Value, objXmlPack.SelectSingleNode("translate")?.Value ?? strName));
            }
            lstKit.Sort(CompareListItems.CompareNames);
            lstKits.BeginUpdate();
            lstKits.PopulateWithListItems(lstKit);
            lstKits.EndUpdate();

            if (lstKit.Count == 0)
                treContents.Nodes.Clear();

            cmdDelete.Visible = false;
        }

        private void lstKits_SelectedIndexChanged(object sender, EventArgs e)
        {
            string strSelectedKit = lstKits.SelectedValue?.ToString();
            if (string.IsNullOrEmpty(strSelectedKit))
            {
                cmdDelete.Visible = false;
                return;
            }

            treContents.Nodes.Clear();
            string[] strIdentifiers = strSelectedKit.Split('<', StringSplitOptions.RemoveEmptyEntries);
            cmdDelete.Visible = strIdentifiers[1] == "Custom";
            XPathNavigator objXmlPack = _xmlBaseChummerNode.SelectSingleNode("packs/pack[name = " + strIdentifiers[0].CleanXPath() + " and category = " + strIdentifiers[1].CleanXPath() + "]");
            if (objXmlPack == null)
            {
                return;
            }

            string strSpace = LanguageManager.GetString("String_Space");
            foreach (XPathNavigator objXmlItem in objXmlPack.SelectChildren(XPathNodeType.Element))
            {
                if (objXmlItem.SelectSingleNode("hide") != null)
                    continue;
                TreeNode objParent = new TreeNode();
                switch (objXmlItem.Name)
                {
                    case "attributes":
                        objParent.Text = LanguageManager.GetString("String_SelectPACKSKit_Attributes");
                        treContents.Nodes.Add(objParent);
                        foreach (XPathNavigator objXmlAttribute in objXmlItem.SelectChildren(XPathNodeType.Element))
                        {
                            if (objXmlAttribute.SelectSingleNode("hide") != null)
                                continue;
                            string strNameUpper = objXmlAttribute.Name.ToUpperInvariant();
                            TreeNode objChild = new TreeNode
                            {
                                Text = LanguageManager.GetString("String_Attribute" + strNameUpper + "Short") + strSpace + (Convert.ToInt32(objXmlAttribute.Value, GlobalOptions.InvariantCultureInfo) - (6 - _objCharacter.GetAttribute(strNameUpper).MetatypeMaximum)).ToString(GlobalOptions.CultureInfo)
                            };

                            objParent.Nodes.Add(objChild);
                            objParent.Expand();
                        }
                        break;
                    case "qualities":
                        objParent.Text = LanguageManager.GetString("String_SelectPACKSKit_Qualities");
                        treContents.Nodes.Add(objParent);
                        // Positive Qualities.
                        foreach (XPathNavigator objXmlQuality in objXmlItem.Select("positive/quality"))
                        {
                            if (objXmlQuality.SelectSingleNode("hide") != null)
                                continue;
                            XPathNavigator objNode = _xmlQualitiesBaseQualitiesNode.SelectSingleNode("quality[(" + _objCharacter.Options.BookXPath() + ") and name = " + objXmlQuality.Value.CleanXPath() + "]");
                            if (objNode == null)
                                continue;
                            TreeNode objChild = new TreeNode
                            {
                                Text = objNode.SelectSingleNode("translate")?.Value ?? objXmlQuality.Value
                            };

                            string strSelect = objXmlQuality.SelectSingleNode("@select")?.Value;
                            if (!string.IsNullOrEmpty(strSelect))
                                objChild.Text += strSpace + '('+ _objCharacter.TranslateExtra(strSelect)+ ')';
                            objParent.Nodes.Add(objChild);
                            objParent.Expand();
                        }

                        // Negative Qualities.
                        foreach (XPathNavigator objXmlQuality in objXmlItem.Select("negative/quality"))
                        {
                            if (objXmlQuality.SelectSingleNode("hide") != null)
                                continue;
                            XPathNavigator objNode = _xmlQualitiesBaseQualitiesNode.SelectSingleNode("quality[(" + _objCharacter.Options.BookXPath() + ") and name = " + objXmlQuality.Value.CleanXPath() + "]");
                            if (objNode == null)
                                continue;
                            TreeNode objChild = new TreeNode
                            {
                                Text = objNode.SelectSingleNode("translate")?.Value ?? objXmlQuality.Value
                            };

                            string strSelect = objXmlQuality.SelectSingleNode("@select")?.Value;
                            if (!string.IsNullOrEmpty(strSelect))
                                objChild.Text += strSpace + '(' + _objCharacter.TranslateExtra(strSelect) + ')';
                            objParent.Nodes.Add(objChild);
                            objParent.Expand();
                        }
                        break;
                    case "nuyenbp":
                        objParent.Text = LanguageManager.GetString("String_SelectPACKSKit_Nuyen");
                        treContents.Nodes.Add(objParent);
                        TreeNode objNuyenChild = new TreeNode
                        {
                            Text = LanguageManager.GetString("String_SelectPACKSKit_StartingNuyenBP") + strSpace + objXmlItem.Value
                        };
                        objParent.Nodes.Add(objNuyenChild);
                        objParent.Expand();
                        break;
                    case "skills":
                        objParent.Text = LanguageManager.GetString("String_SelectPACKSKit_Skills");
                        treContents.Nodes.Add(objParent);
                        foreach (XPathNavigator objXmlSkill in objXmlItem.Select("skill"))
                        {
                            if (objXmlSkill.SelectSingleNode("hide") != null)
                                continue;
                            string strName = objXmlSkill.SelectSingleNode("name").Value;
                            XPathNavigator objNode = _xmlSkillsBaseChummerNode.SelectSingleNode("skills/skill[name = " + strName.CleanXPath() + "]");
                            if (objNode.SelectSingleNode("hide") != null)
                                continue;
                            TreeNode objChild = new TreeNode
                            {
                                Text = objNode.SelectSingleNode("translate")?.Value ?? strName
                            };
                            objChild.Text += strSpace + objXmlSkill.SelectSingleNode("rating")?.Value;

                            string strSpec = objXmlSkill.SelectSingleNode("spec")?.Value;
                            if (!string.IsNullOrEmpty(strSpec))
                                objChild.Text += strSpace + '(' + strSpec + ')';
                            objParent.Nodes.Add(objChild);
                            objParent.Expand();
                        }
                        foreach (XPathNavigator objXmlSkill in objXmlItem.Select("skillgroup"))
                        {
                            if (objXmlSkill.SelectSingleNode("hide") != null)
                                continue;
                            string strName = objXmlSkill.SelectSingleNode("name").Value;
                            XPathNavigator objNode = _xmlSkillsBaseChummerNode.SelectSingleNode("skillgroups/name[. = " + strName.CleanXPath() + "]");
                            if (objNode.SelectSingleNode("hide") != null)
                                continue;
                            TreeNode objChild = new TreeNode
                            {
                                Text = objNode.SelectSingleNode("@translate")?.Value ?? strName
                            };
                            objChild.Text += strSpace + LanguageManager.GetString("String_SelectPACKSKit_Group")+ strSpace + objXmlSkill.SelectSingleNode("rating")?.Value;

                            string strSpec = objXmlSkill.SelectSingleNode("spec")?.Value;
                            if (!string.IsNullOrEmpty(strSpec))
                                objChild.Text += strSpace + '(' + strSpec + ')';
                            objParent.Nodes.Add(objChild);
                            objParent.Expand();
                        }
                        break;
                    case "knowledgeskills":
                        objParent.Text = LanguageManager.GetString("String_SelectPACKSKit_KnowledgeSkills");
                        treContents.Nodes.Add(objParent);
                        foreach (XPathNavigator objXmlSkill in objXmlItem.Select("skill"))
                        {
                            if (objXmlSkill.SelectSingleNode("hide") != null)
                                continue;
                            TreeNode objChild = new TreeNode();
                            string strName = objXmlSkill.SelectSingleNode("name").Value;
                            if (!string.IsNullOrEmpty(strName))
                            {
                                XPathNavigator objNode = _xmlSkillsBaseChummerNode.SelectSingleNode("knowledgeskills/skill[name = " + strName.CleanXPath() + "]");
                                if (objNode.SelectSingleNode("hide") != null)
                                    continue;
                                objChild.Text = objNode?.SelectSingleNode("translate")?.Value ?? strName;
                            }
                            objChild.Text += strSpace + objXmlSkill.SelectSingleNode("rating")?.Value;

                            string strSpec = objXmlSkill.SelectSingleNode("spec")?.Value;
                            if (!string.IsNullOrEmpty(strSpec))
                                objChild.Text += strSpace + '(' + strSpec + ')';
                            objParent.Nodes.Add(objChild);
                            objParent.Expand();
                        }
                        break;
                    case "selectmartialart":
                        {
                            objParent.Text = LanguageManager.GetString("String_SelectPACKSKit_SelectMartialArt");
                            treContents.Nodes.Add(objParent);

                            int intRating = Convert.ToInt32(objXmlItem.SelectSingleNode("@rating")?.Value ?? "1", GlobalOptions.InvariantCultureInfo);
                            string strSelect = objXmlItem.SelectSingleNode("@select")?.Value ?? LanguageManager.GetString("String_SelectPACKSKit_SelectMartialArt");

                            TreeNode objMartialArt = new TreeNode
                            {
                                Text = strSelect + strSpace + LanguageManager.GetString("String_Rating") + strSpace + intRating.ToString(GlobalOptions.CultureInfo)
                            };
                            objParent.Nodes.Add(objMartialArt);
                            objParent.Expand();
                            break;
                        }
                    case "martialarts":
                        objParent.Text = LanguageManager.GetString("String_SelectPACKSKit_MartialArts");
                        treContents.Nodes.Add(objParent);
                        foreach (XPathNavigator objXmlArt in objXmlItem.Select("martialart"))
                        {
                            if (objXmlArt.SelectSingleNode("hide") != null)
                                continue;
                            string strName = objXmlArt.SelectSingleNode("name").Value;
                            XPathNavigator objNode = _xmlMartialArtsBaseChummerNode.SelectSingleNode("martialarts/martialart[(" + _objCharacter.Options.BookXPath() + ") and name = " + strName.CleanXPath() + "]");
                            if (objNode == null)
                                continue;
                            TreeNode objChild = new TreeNode
                            {
                                Text = objNode.SelectSingleNode("translate")?.Value ?? strName
                            };
                            objChild.Text += strSpace + objXmlArt.SelectSingleNode("rating")?.Value;

                            // Check for Techniques.
                            foreach (XPathNavigator xmlTechnique in objXmlArt.Select("techniques/technique"))
                            {
                                if (xmlTechnique.SelectSingleNode("hide") != null)
                                    continue;
                                string strTechniqueName = xmlTechnique.SelectSingleNode("name").Value;
                                XPathNavigator objChildNode = _xmlMartialArtsBaseChummerNode.SelectSingleNode("techniques/technique[(" + _objCharacter.Options.BookXPath() + ") and name = " + strTechniqueName.CleanXPath() + "]");
                                if (objNode == null)
                                    continue;
                                TreeNode objChildChild = new TreeNode
                                {
                                    Text = objChildNode.SelectSingleNode("@translate")?.Value ?? strTechniqueName
                                };

                                objChild.Nodes.Add(objChildChild);
                                objChild.Expand();
                            }

                            objParent.Nodes.Add(objChild);
                            objParent.Expand();
                        }
                        break;
                    case "powers":
                        {
                            objParent.Text = LanguageManager.GetString("String_SelectPACKSKit_Powers");
                            treContents.Nodes.Add(objParent);
                            foreach (XPathNavigator objXmlPower in objXmlItem.Select("power"))
                            {
                                if (objXmlPower.SelectSingleNode("hide") != null)
                                    continue;
                                string strName = objXmlPower.SelectSingleNode("name").Value;
                                XPathNavigator objNode = _xmlPowersBasePowersNode.SelectSingleNode("power[(" + _objCharacter.Options.BookXPath() + ") and name = " + strName.CleanXPath() + "]");
                                if (objNode == null)
                                    continue;
                                TreeNode objChild = new TreeNode
                                {
                                    Text = objNode.SelectSingleNode("translate")?.Value ?? strName
                                };

                                string strSelect = objXmlPower.SelectSingleNode("name/@select")?.Value ?? string.Empty;
                                if (!string.IsNullOrEmpty(strSelect))
                                    objChild.Text += strSpace + '(' + strSelect + ')';
                                string strRating = objXmlPower.SelectSingleNode("rating")?.Value;
                                if (!string.IsNullOrEmpty(strRating))
                                    objChild.Text += strSpace + strRating;
                                objParent.Nodes.Add(objChild);
                                objParent.Expand();
                            }
                            break;
                        }
                    case "programs":
                        objParent.Text = LanguageManager.GetString("String_SelectPACKSKit_Programs");
                        treContents.Nodes.Add(objParent);
                        foreach (XPathNavigator objXmlProgram in objXmlItem.Select("program"))
                        {
                            if (objXmlProgram.SelectSingleNode("hide") != null)
                                continue;
                            string strName = objXmlProgram.SelectSingleNode("name").Value;
                            XPathNavigator objNode = _xmlComplexFormsBaseChummerNode.SelectSingleNode("complexforms/complexform[(" + _objCharacter.Options.BookXPath() + ") and name = " + strName.CleanXPath() + "]");
                            if (objNode == null)
                                continue;
                            TreeNode objChild = new TreeNode
                            {
                                Text = objNode.SelectSingleNode("translate")?.Value ?? strName
                            };
                            objChild.Text += strSpace + objXmlProgram.SelectSingleNode("rating")?.Value;

                            // Check for Program Options.
                            foreach (XPathNavigator objXmlOption in objXmlProgram.Select("options/option"))
                            {
                                if (objXmlOption.SelectSingleNode("hide") != null)
                                    continue;
                                string strOptionName = objXmlOption.SelectSingleNode("name").Value;
                                XPathNavigator objChildNode = _xmlComplexFormsBaseChummerNode.SelectSingleNode("options/option[(" + _objCharacter.Options.BookXPath() + ") and name = " + strOptionName.CleanXPath() + "]");
                                if (objNode == null)
                                    continue;
                                TreeNode objChildChild = new TreeNode
                                {
                                    Text = objChildNode.SelectSingleNode("translate")?.Value ?? strOptionName
                                };

                                string strRating = objXmlOption.SelectSingleNode("rating")?.Value;
                                if (!string.IsNullOrEmpty(strRating))
                                    objChildChild.Text += strSpace + strRating;
                                objChild.Nodes.Add(objChildChild);
                                objChild.Expand();
                            }

                            objParent.Nodes.Add(objChild);
                            objParent.Expand();
                        }
                        break;
                    case "spells":
                        objParent.Text = LanguageManager.GetString("String_SelectPACKSKit_Spells");
                        treContents.Nodes.Add(objParent);
                        foreach (XPathNavigator objXmlSpell in objXmlItem.Select("spell"))
                        {
                            if (objXmlSpell.SelectSingleNode("hide") != null)
                                continue;
                            string strName = objXmlSpell.Value;
                            XPathNavigator objNode = _xmlSpellsBaseSpellsNode.SelectSingleNode("spell[(" + _objCharacter.Options.BookXPath() + ") and name = " + strName.CleanXPath() + "]");
                            if (objNode.SelectSingleNode("hide") == null)
                                continue;
                            TreeNode objChild = new TreeNode
                            {
                                Text = objNode.SelectSingleNode("translate")?.Value ?? strName
                            };

                            string strSelect = objXmlSpell.SelectSingleNode("@select")?.Value;
                            if (!string.IsNullOrEmpty(strSelect))
                                objChild.Text += strSpace + '(' + strSelect + ')';
                            objParent.Nodes.Add(objChild);
                            objParent.Expand();
                        }
                        break;
                    case "spirits":

                        objParent.Text = LanguageManager.GetString("String_SelectPACKSKit_Spirits");
                        treContents.Nodes.Add(objParent);
                        foreach (XPathNavigator objXmlSpirit in objXmlItem.Select("spirit"))
                        {
                            if (objXmlSpirit.SelectSingleNode("hide") != null)
                                continue;
                            TreeNode objChild = new TreeNode
                            {
                                Text = objXmlSpirit.SelectSingleNode("name").Value + strSpace + '(' +
                                       LanguageManager.GetString("Label_Spirit_Force") + strSpace + objXmlSpirit.SelectSingleNode("force").Value + ',' + strSpace +
                                       LanguageManager.GetString("Label_Spirit_ServicesOwed") + strSpace + objXmlSpirit.SelectSingleNode("services").Value + ')'
                            };
                            objParent.Nodes.Add(objChild);
                            objParent.Expand();
                        }
                        break;
                    case "lifestyles":

                        objParent.Text = LanguageManager.GetString("String_SelectPACKSKit_Lifestyles");
                        treContents.Nodes.Add(objParent);
                        foreach (XPathNavigator objXmlLifestyle in objXmlItem.Select("lifestyle"))
                        {
                            if (objXmlLifestyle.SelectSingleNode("hide") != null)
                                continue;

                            string strIncrement = objXmlLifestyle.SelectSingleNode("increment")?.Value;
                            if (objXmlLifestyle.SelectSingleNode("type")?.Value.ToUpperInvariant() == "SAFEHOUSE")
                                strIncrement = "week";
                            string strIncrementString;
                            int intPermanentAmount;
                            switch (strIncrement)
                            {
                                case "day":
                                case "Day":
                                    strIncrementString = LanguageManager.GetString("String_Days");
                                    intPermanentAmount = 3044;
                                    break;
                                case "week":
                                case "Week":
                                    strIncrementString = LanguageManager.GetString("String_Weeks");
                                    intPermanentAmount = 435;
                                    break;
                                default:
                                    strIncrementString = LanguageManager.GetString("String_Months");
                                    intPermanentAmount = 100;
                                    break;
                            }

                            TreeNode objChild = new TreeNode
                            {
                                Text = (objXmlLifestyle.SelectSingleNode("translate") ?? objXmlLifestyle.SelectSingleNode("baselifestyle")).Value
                                       + strSpace + objXmlLifestyle.SelectSingleNode("months").Value
                                       + strSpace + strIncrementString + string.Format(GlobalOptions.CultureInfo, LanguageManager.GetString("Label_LifestylePermanent"), intPermanentAmount.ToString(GlobalOptions.CultureInfo))
                            };

                            // Check for Qualities.
                            foreach (XPathNavigator objXmlQuality in objXmlLifestyle.Select("qualities/quality"))
                            {
                                if (objXmlQuality.SelectSingleNode("hide") != null)
                                    continue;
                                TreeNode objChildChild = new TreeNode
                                {
                                    Text = objXmlQuality.Value
                                };
                                objChild.Nodes.Add(objChildChild);
                                objChild.Expand();
                            }

                            objParent.Nodes.Add(objChild);
                            objParent.Expand();
                        }
                        break;
                    case "cyberwares":
                        objParent.Text = LanguageManager.GetString("String_SelectPACKSKit_Cyberware");
                        treContents.Nodes.Add(objParent);
                        foreach (XPathNavigator objXmlCyberware in objXmlItem.Select("cyberware"))
                        {
                            if (objXmlCyberware.SelectSingleNode("hide") != null)
                                continue;
                            string strName = objXmlCyberware.SelectSingleNode("name").Value;
                            XPathNavigator objNode = _xmlCyberwareBaseChummerNode.SelectSingleNode("cyberwares/cyberware[(" + _objCharacter.Options.BookXPath() + ") and name = " + strName.CleanXPath() + "]");
                            if (objNode == null)
                                continue;
                            TreeNode objChild = new TreeNode
                            {
                                Text = objNode.SelectSingleNode("translate")?.Value ?? strName
                            };

                            string strRating = objXmlCyberware.SelectSingleNode("rating")?.Value;
                            if (!string.IsNullOrEmpty(strRating))
                                objChild.Text += strSpace + LanguageManager.GetString("String_Rating") + strSpace + strRating;
                            objChild.Text += strSpace + '(' + objXmlCyberware.SelectSingleNode("grade").Value + ')';

                            // Check for children.
                            foreach (XPathNavigator objXmlChild in objXmlCyberware.Select("cyberwares/cyberware"))
                            {
                                if (objXmlChild.SelectSingleNode("hide") != null)
                                    continue;
                                string strChildName = objXmlChild.SelectSingleNode("name").Value;
                                XPathNavigator objChildNode = _xmlCyberwareBaseChummerNode.SelectSingleNode("cyberwares/cyberware[(" + _objCharacter.Options.BookXPath() + ") and name = " + strChildName.CleanXPath() + "]");
                                if (objChildNode == null)
                                    continue;
                                TreeNode objChildChild = new TreeNode
                                {
                                    Text = objChildNode.SelectSingleNode("translate")?.Value ?? strChildName
                                };

                                strRating = objXmlChild.SelectSingleNode("rating")?.Value;
                                if (!string.IsNullOrEmpty(strRating))
                                    objChildChild.Text += strSpace + LanguageManager.GetString("String_Rating") + strSpace + strRating;

                                foreach (XPathNavigator objXmlGearNode in objXmlChild.Select("gears/gear"))
                                    WriteGear(objXmlGearNode, objChildChild);
                                objChild.Expand();

                                objChild.Nodes.Add(objChildChild);
                                objChild.Expand();
                            }

                            foreach (XPathNavigator objXmlGearNode in objXmlCyberware.Select("gears/gear"))
                                WriteGear(objXmlGearNode, objChild);
                            objChild.Expand();

                            objParent.Nodes.Add(objChild);
                            objParent.Expand();
                        }
                        break;
                    case "biowares":
                        objParent.Text = LanguageManager.GetString("String_SelectPACKSKit_Bioware");
                        treContents.Nodes.Add(objParent);
                        foreach (XPathNavigator objXmlBioware in objXmlItem.Select("bioware"))
                        {
                            if (objXmlBioware.SelectSingleNode("hide") != null)
                                continue;
                            string strName = objXmlBioware.SelectSingleNode("name").Value;
                            XPathNavigator objNode = _xmlBiowareBaseChummerNode.SelectSingleNode("biowares/bioware[(" + _objCharacter.Options.BookXPath() + ") and name = " + strName.CleanXPath() + "]");
                            if (objNode == null)
                                continue;
                            TreeNode objChild = new TreeNode
                            {
                                Text = objNode.SelectSingleNode("translate")?.Value ?? strName
                            };

                            string strRating = objXmlBioware.SelectSingleNode("rating")?.Value;
                            if (!string.IsNullOrEmpty(strRating))
                                objChild.Text += strSpace + LanguageManager.GetString("String_Rating") + strSpace + strRating;
                            objChild.Text += strSpace + '(' + objXmlBioware.SelectSingleNode("grade").Value + ')';

                            // Check for children.
                            foreach (XPathNavigator objXmlChild in objXmlBioware.Select("biowares/bioware"))
                            {
                                if (objXmlChild.SelectSingleNode("hide") != null)
                                    continue;
                                string strChildName = objXmlChild.SelectSingleNode("name").Value;
                                XPathNavigator objChildNode = _xmlBiowareBaseChummerNode.SelectSingleNode("biowares/bioware[(" + _objCharacter.Options.BookXPath() + ") and name = " + strChildName.CleanXPath() + "]");
                                if (objChildNode == null)
                                    continue;
                                TreeNode objChildChild = new TreeNode
                                {
                                    Text = objChildNode.SelectSingleNode("translate")?.Value ?? strChildName
                                };

                                strRating = objXmlChild.SelectSingleNode("rating")?.Value;
                                if (!string.IsNullOrEmpty(strRating))
                                    objChildChild.Text += strSpace + LanguageManager.GetString("String_Rating") + strSpace + strRating;

                                foreach (XPathNavigator objXmlGearNode in objXmlChild.Select("gears/gear"))
                                    WriteGear(objXmlGearNode, objChildChild);
                                objChild.Expand();

                                objChild.Nodes.Add(objChildChild);
                                objChild.Expand();
                            }

                            foreach (XPathNavigator objXmlGearNode in objXmlBioware.Select("gears/gear"))
                                WriteGear(objXmlGearNode, objChild);
                            objChild.Expand();

                            objParent.Nodes.Add(objChild);
                            objParent.Expand();
                        }
                        break;
                    case "armors":
                        objParent.Text = LanguageManager.GetString("String_SelectPACKSKit_Armor");
                        treContents.Nodes.Add(objParent);
                        foreach (XPathNavigator objXmlArmor in objXmlItem.Select("armor"))
                        {
                            if (objXmlArmor.SelectSingleNode("hide") != null)
                                continue;
                            string strName = objXmlArmor.SelectSingleNode("name").Value;
                            XPathNavigator objNode = _xmlArmorBaseChummerNode.SelectSingleNode("armors/armor[(" + _objCharacter.Options.BookXPath() + ") and name = " + strName.CleanXPath() + "]");
                            if (objNode == null)
                                continue;
                            TreeNode objChild = new TreeNode
                            {
                                Text = objNode.SelectSingleNode("translate")?.Value ?? strName
                            };

                            // Check for children.
                            foreach (XPathNavigator objXmlChild in objXmlArmor.Select("mods/mod"))
                            {
                                if (objXmlChild.SelectSingleNode("hide") != null)
                                    continue;
                                string strChildName = objXmlChild.SelectSingleNode("name").Value;
                                XPathNavigator objChildNode = _xmlArmorBaseChummerNode.SelectSingleNode("mods/mod[(" + _objCharacter.Options.BookXPath() + ") and name = " + strChildName.CleanXPath() + "]");
                                if (objChildNode == null)
                                    continue;
                                TreeNode objChildChild = new TreeNode
                                {
                                    Text = objChildNode.SelectSingleNode("translate")?.Value ?? strChildName
                                };

                                string strRating = objXmlChild.SelectSingleNode("rating")?.Value;
                                if (!string.IsNullOrEmpty(strRating))
                                    objChildChild.Text += strSpace + LanguageManager.GetString("String_Rating") + strSpace + strRating;

                                foreach (XPathNavigator objXmlGearNode in objXmlChild.Select("gears/gear"))
                                    WriteGear(objXmlGearNode, objChildChild);

                                objChild.Nodes.Add(objChildChild);
                                objChild.Expand();
                            }

                            foreach (XPathNavigator objXmlGearNode in objXmlArmor.Select("gears/gear"))
                                WriteGear(objXmlGearNode, objChild);
                            objChild.Expand();

                            objParent.Nodes.Add(objChild);
                            objParent.Expand();
                        }
                        break;
                    case "weapons":
                        objParent.Text = LanguageManager.GetString("String_SelectPACKSKit_Weapons");
                        treContents.Nodes.Add(objParent);
                        foreach (XPathNavigator objXmlWeapon in objXmlItem.Select("weapon"))
                        {
                            if (objXmlWeapon.SelectSingleNode("hide") != null)
                                continue;
                            string strName = objXmlWeapon.SelectSingleNode("name").Value;
                            XPathNavigator objNode = _xmlWeaponsBaseChummerNode.SelectSingleNode("weapons/weapon[(" + _objCharacter.Options.BookXPath() + ") and name = " + strName.CleanXPath() + "]");
                            if (objNode == null)
                                continue;
                            TreeNode objChild = new TreeNode
                            {
                                Text = objNode.SelectSingleNode("translate")?.Value ?? strName
                            };

                            // Check for Weapon Accessories.
                            foreach (XPathNavigator objXmlAccessory in objXmlWeapon.Select("accessories/accessory"))
                            {
                                if (objXmlAccessory.SelectSingleNode("hide") != null)
                                    continue;
                                strName = objXmlAccessory.SelectSingleNode("name").Value;
                                XPathNavigator objChildNode = _xmlWeaponsBaseChummerNode.SelectSingleNode("accessories/accessory[(" + _objCharacter.Options.BookXPath() + ") and name = " + strName.CleanXPath() + "]");
                                if (objChildNode == null)
                                    continue;
                                TreeNode objChildChild = new TreeNode
                                {
                                    Text = objChildNode.SelectSingleNode("translate")?.Value ?? strName
                                };

                                string strRating = objXmlAccessory.SelectSingleNode("rating")?.Value;
                                if (!string.IsNullOrEmpty(strRating))
                                    objChildChild.Text += strSpace + LanguageManager.GetString("String_Rating") + strSpace + strRating;

                                foreach (XPathNavigator objXmlGearNode in objXmlAccessory.Select("gears/gear"))
                                    WriteGear(objXmlGearNode, objChildChild);
                                objChildChild.Expand();

                                objChild.Nodes.Add(objChildChild);
                                objChild.Expand();
                            }

                            strName = objXmlWeapon.SelectSingleNode("underbarrel")?.Value;
                            // Check for Underbarrel Weapons.
                            if (!string.IsNullOrEmpty(strName))
                            {
                                if (objXmlWeapon.SelectSingleNode("hide") != null)
                                    continue;

                                XPathNavigator objChildNode = _xmlWeaponsBaseChummerNode.SelectSingleNode("weapons/weapon[(" + _objCharacter.Options.BookXPath() + ") and name = " + strName.CleanXPath() + "]");
                                if (objChildNode == null)
                                    continue;
                                TreeNode objChildChild = new TreeNode
                                {
                                    Text = objChildNode.SelectSingleNode("translate")?.Value ?? strName
                                };

                                objChild.Nodes.Add(objChildChild);
                                objChild.Expand();
                            }

                            objParent.Nodes.Add(objChild);
                            objParent.Expand();
                        }
                        break;
                    case "gears":
                        objParent.Text = LanguageManager.GetString("String_SelectPACKSKit_Gear");
                        treContents.Nodes.Add(objParent);
                        foreach (XPathNavigator objXmlGear in objXmlItem.Select("gear"))
                        {
                            if (objXmlGear.SelectSingleNode("hide") != null)
                                continue;
                            WriteGear(objXmlGear, objParent);
                            objParent.Expand();
                        }
                        break;
                    case "vehicles":
                        objParent.Text = LanguageManager.GetString("String_SelectPACKSKit_Vehicles");
                        treContents.Nodes.Add(objParent);
                        foreach (XPathNavigator objXmlVehicle in objXmlItem.Select("vehicle"))
                        {
                            if (objXmlVehicle.SelectSingleNode("hide") != null)
                                continue;
                            string strName = objXmlVehicle.SelectSingleNode("name").Value;
                            XPathNavigator objNode = _xmlVehiclesBaseChummerNode.SelectSingleNode("vehicles/vehicle[(" + _objCharacter.Options.BookXPath() + ") and name = " + strName.CleanXPath() + "]");
                            if (objNode == null)
                                continue;
                            TreeNode objChild = new TreeNode
                            {
                                Text = objNode.SelectSingleNode("translate")?.Value ?? strName
                            };

                            // Check for children.
                            foreach (XPathNavigator objXmlMod in objXmlVehicle.Select("mods/mod"))
                            {
                                if (objXmlMod.SelectSingleNode("hide") != null)
                                    continue;
                                strName = objXmlMod.SelectSingleNode("name").Value;
                                XPathNavigator objChildNode = _xmlVehiclesBaseChummerNode.SelectSingleNode("mods/mod[(" + _objCharacter.Options.BookXPath() + ") and name = " + strName.CleanXPath() + "]");
                                if (objChildNode == null)
                                    continue;
                                TreeNode objChildChild = new TreeNode
                                {
                                    Text = objChildNode.SelectSingleNode("translate")?.Value ?? strName
                                };

                                string strRating = objXmlMod.SelectSingleNode("rating")?.Value;
                                if (!string.IsNullOrEmpty(strRating))
                                    objChildChild.Text += strSpace + LanguageManager.GetString("String_Rating") + strSpace + strRating;
                                objChild.Nodes.Add(objChildChild);
                                objChild.Expand();
                            }

                            // Check for children.
                            foreach (XPathNavigator objXmlChild in objXmlVehicle.Select("gears/gear"))
                            {
                                if (objXmlChild.SelectSingleNode("hide") != null)
                                    continue;
                                WriteGear(objXmlChild, objChild);
                                objChild.Expand();
                            }

                            // Check for children.
                            foreach (XPathNavigator objXmlWeapon in objXmlVehicle.Select("weapons/weapon"))
                            {
                                if (objXmlWeapon.SelectSingleNode("hide") != null)
                                    continue;
                                strName = objXmlWeapon.SelectSingleNode("name").Value;
                                XPathNavigator objChildNode = _xmlWeaponsBaseChummerNode.SelectSingleNode("weapons/weapon[(" + _objCharacter.Options.BookXPath() + ") and name = " + strName.CleanXPath() + "]");
                                if (objChildNode == null)
                                    continue;
                                TreeNode objChildChild = new TreeNode
                                {
                                    Text = objChildNode.SelectSingleNode("translate")?.Value ?? strName
                                };

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
            _blnAddAgain = false;
            AcceptForm();
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void cmdOKAdd_Click(object sender, EventArgs e)
        {
            _blnAddAgain = true;
            AcceptForm();
        }

        private void cmdDelete_Click(object sender, EventArgs e)
        {
            string strSelectedKit = lstKits.SelectedValue?.ToString();
            if (string.IsNullOrEmpty(strSelectedKit))
                return;

            if (Program.MainForm.ShowMessageBox(this, string.Format(GlobalOptions.CultureInfo, LanguageManager.GetString("Message_DeletePACKSKit"), strSelectedKit),
                    LanguageManager.GetString("MessageTitle_Delete"), MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.No)
                return;

            // Delete the selected custom PACKS Kit.
            // Find a custom PACKS Kit with the name. This is done without the XmlManager since we need to check each file individually.
            string strCustomPath = Path.Combine(Utils.GetStartupPath, "data");
            foreach (string strFile in Directory.GetFiles(strCustomPath, "custom*_packs.xml"))
            {
                XmlDocument objXmlDocument = new XmlDocument { XmlResolver = null };
                try
                {
                    objXmlDocument.LoadStandard(strFile);
                }
                catch (IOException)
                {
                    continue;
                }
                catch (XmlException)
                {
                    continue;
                }

                XmlNode xmlDocumentBasePacksNode = objXmlDocument.SelectSingleNode("/chummer/packs");
                if (xmlDocumentBasePacksNode?.SelectSingleNode("pack[name = " + strSelectedKit.CleanXPath() + " and category = \"Custom\"]") != null)
                {
                    using (FileStream objStream = new FileStream(strFile, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
                    {
                        using (XmlTextWriter objWriter = new XmlTextWriter(objStream, Encoding.UTF8)
                        {
                            Formatting = Formatting.Indented,
                            Indentation = 1,
                            IndentChar = '\t'
                        })
                        {
                            objWriter.WriteStartDocument();

                            // <chummer>
                            objWriter.WriteStartElement("chummer");
                            // <packs>
                            objWriter.WriteStartElement("packs");

                            // If this is not a new file, write out the current contents.
                            using (XmlNodeList objXmlNodeList = xmlDocumentBasePacksNode.SelectNodes("*"))
                            {
                                if (objXmlNodeList?.Count > 0)
                                {
                                    foreach (XmlNode objXmlNode in objXmlNodeList)
                                    {
                                        if (objXmlNode["name"]?.InnerText != strSelectedKit)
                                        {
                                            // <pack>
                                            objWriter.WriteStartElement("pack");
                                            objXmlNode.WriteContentTo(objWriter);
                                            // </pack>
                                            objWriter.WriteEndElement();
                                        }
                                    }
                                }
                            }

                            // </packs>
                            objWriter.WriteEndElement();
                            // </chummer>
                            objWriter.WriteEndElement();

                            objWriter.WriteEndDocument();
                        }
                    }
                }
            }

            // Reload the PACKS files since they have changed.
            _xmlBaseChummerNode = _objCharacter.LoadDataXPath("packs.xml").SelectSingleNode("/chummer");
            cboCategory_SelectedIndexChanged(sender, e);
        }
        #endregion

        #region Properties
        /// <summary>
        /// Whether or not the user wants to add another item after this one.
        /// </summary>
        public bool AddAgain => _blnAddAgain;

        /// <summary>
        /// Name of Kit that was selected in the dialogue.
        /// </summary>
        public string SelectedKit => _strSelectedKit;

        /// <summary>
        /// Category that was selected in the dialogue.
        /// </summary>
        public static string SelectedCategory => s_StrSelectCategory;

        #endregion

        #region Methods
        /// <summary>
        /// Accept the selected item and close the form.
        /// </summary>
        private void AcceptForm()
        {
            string strSelectedKit = lstKits.SelectedValue?.ToString();
            if (string.IsNullOrEmpty(strSelectedKit))
                return;
            string[] objSelectedKit = strSelectedKit.Split('<', StringSplitOptions.RemoveEmptyEntries);
            _strSelectedKit = objSelectedKit[0];
            s_StrSelectCategory = objSelectedKit[1];
            DialogResult = DialogResult.OK;
        }

        private void WriteGear(XPathNavigator objXmlGear, TreeNode objParent)
        {
            string strSpace = LanguageManager.GetString("String_Space");
            XPathNavigator xmlNameNode = objXmlGear.SelectSingleNode("name");
            string strName = xmlNameNode?.Value ?? string.Empty;
            string strCategory = objXmlGear.SelectSingleNode("category")?.Value;
            XPathNavigator objNode = !string.IsNullOrEmpty(strCategory)
                ? _xmlGearsBaseGearsNode.SelectSingleNode("gear[(" + _objCharacter.Options.BookXPath() + ") and name = " + strName.CleanXPath() + " and category = " + strCategory.CleanXPath() + "]")
                : _xmlGearsBaseGearsNode.SelectSingleNode("gear[(" + _objCharacter.Options.BookXPath() + ") and name = " + strName.CleanXPath() + "]");

            if (objNode != null)
            {
                TreeNode objChild = new TreeNode
                {
                    Text = objNode.SelectSingleNode("@translate")?.Value ?? strName
                };

                string strExtra = xmlNameNode?.SelectSingleNode("@select")?.Value;
                if (!string.IsNullOrEmpty(strExtra))
                    objChild.Text += strSpace + '(' + strExtra + ')';
                strExtra = objXmlGear.SelectSingleNode("rating")?.Value;
                if (!string.IsNullOrEmpty(strExtra))
                    objChild.Text += strSpace + LanguageManager.GetString("String_Rating") + strSpace + strExtra;
                strExtra = objXmlGear.SelectSingleNode("qty")?.Value;
                if (!string.IsNullOrEmpty(strExtra))
                    objChild.Text += strSpace + 'x' + strExtra;

                objParent.Nodes.Add(objChild);

                // Check for children.
                foreach (XPathNavigator objXmlChild in objXmlGear.Select("gears/gear"))
                {
                    WriteGear(objXmlChild, objChild);
                }

                objChild.Expand();
            }
        }
        #endregion
    }
}
