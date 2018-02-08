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

namespace Chummer
{
    public partial class frmSelectPACKSKit : Form
    {
        private string _strSelectedKit = string.Empty;
        private bool _blnAddAgain;
        private static string s_StrSelectCategory = string.Empty;
        private readonly Character _objCharacter;

        // Not readonly because content can change while form is up
        private XmlDocument _objXmlDocument;

        private readonly List<ListItem> _lstCategory = new List<ListItem>();

        #region Control Events
        public frmSelectPACKSKit(Character objCharacter)
        {
            InitializeComponent();
            LanguageManager.TranslateWinForm(GlobalOptions.Language, this);
            _objCharacter = objCharacter;
            // Load the PACKS information.
            _objXmlDocument = XmlManager.Load("packs.xml");
        }

        private void frmSelectPACKSKit_Load(object sender, EventArgs e)
        {
            // Populate the PACKS Category list.
            XmlNodeList objXmlCategoryList = _objXmlDocument.SelectNodes("/chummer/categories/category[not(hide)]");
            foreach (XmlNode objXmlCategory in objXmlCategoryList)
            {
                string strInnerText = objXmlCategory.InnerText;
                _lstCategory.Add(new ListItem(strInnerText, objXmlCategory.Attributes?["translate"]?.InnerText ?? strInnerText));
            }
            _lstCategory.Sort(CompareListItems.CompareNames);

            if (_lstCategory.Count > 0)
            {
                _lstCategory.Insert(0, new ListItem("Show All", LanguageManager.GetString("String_ShowAll", GlobalOptions.Language)));
            }

            cboCategory.BeginUpdate();
            cboCategory.ValueMember = "Value";
            cboCategory.DisplayMember = "Name";
            cboCategory.DataSource = _lstCategory;

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
                strFilter += " and category = \"" + cboCategory.SelectedValue + '\"';
            else
            {
                StringBuilder objCategoryFilter = new StringBuilder();
                foreach (string strItem in _lstCategory.Select(x => x.Value))
                {
                    if (!string.IsNullOrEmpty(strItem))
                        objCategoryFilter.Append("category = \"" + strItem + "\" or ");
                }
                if (objCategoryFilter.Length > 0)
                {
                    strFilter += " and (" + objCategoryFilter.ToString().TrimEnd(" or ") + ')';
                }
            }

            // Retrieve the list of Kits for the selected Category.
            XmlNodeList objXmlPacksList = _objXmlDocument.SelectNodes("/chummer/packs/pack[" + strFilter + "]");
            foreach (XmlNode objXmlPack in objXmlPacksList)
            {
                string strName = objXmlPack["name"].InnerText;
                // Separator "<" is a hack because XML does not like it when the '<' character is used in element contents, so we can safely assume that it will never show up.
                lstKit.Add(new ListItem(strName + '<' + objXmlPack["category"].InnerText, objXmlPack["translate"]?.InnerText ?? strName));
            }
            lstKit.Sort(CompareListItems.CompareNames);
            lstKits.BeginUpdate();
            lstKits.DataSource = null;
            lstKits.ValueMember = "Value";
            lstKits.DisplayMember = "Name";
            lstKits.DataSource = lstKit;
            lstKits.EndUpdate();

            if (lstKit.Count == 0)
                treContents.Nodes.Clear();

            cmdDelete.Visible = false;
        }

        private void lstKits_SelectedIndexChanged(object sender, EventArgs e)
        {
            string strSelectedKit = lstKits.SelectedValue?.ToString();
            if (string.IsNullOrEmpty(strSelectedKit))
                return;

            treContents.Nodes.Clear();
            string[] strIdentifiers = strSelectedKit.Split('<');
            XmlNode objXmlPack = _objXmlDocument.SelectSingleNode("/chummer/packs/pack[name = \"" + strIdentifiers[0] + "\" and category = \"" + strIdentifiers[1] + "\"]");
            cmdDelete.Visible = strIdentifiers[1] == "Custom";

            XmlDocument objXmlGearDocument = XmlManager.Load("gear.xml");
            XmlDocument objXmlWeaponDocument = XmlManager.Load("weapons.xml");

            foreach (XmlNode objXmlItem in objXmlPack.ChildNodes)
            {
                if (objXmlItem["hide"] != null)
                    continue;
                TreeNode objParent = new TreeNode();
                XmlDocument objXmlItemDocument;
                switch (objXmlItem.Name)
                {
                    case "attributes":
                        objParent.Text = LanguageManager.GetString("String_SelectPACKSKit_Attributes", GlobalOptions.Language);
                        treContents.Nodes.Add(objParent);
                        foreach (XmlNode objXmlAttribute in objXmlItem.ChildNodes)
                        {
                            if (objXmlAttribute["hide"] != null)
                                continue;
                            string strNameUpper = objXmlAttribute.Name.ToUpper();
                            TreeNode objChild = new TreeNode
                            {
                                Text = LanguageManager.GetString("String_Attribute" + strNameUpper + "Short", GlobalOptions.Language) + ' ' + (Convert.ToInt32(objXmlAttribute.InnerText) - (6 - _objCharacter.GetAttribute(strNameUpper).MetatypeMaximum)).ToString()
                            };

                            objParent.Nodes.Add(objChild);
                            objParent.Expand();
                        }
                        break;
                    case "qualities":
                        objXmlItemDocument = XmlManager.Load("qualities.xml");

                        objParent.Text = LanguageManager.GetString("String_SelectPACKSKit_Qualities", GlobalOptions.Language);
                        treContents.Nodes.Add(objParent);
                        // Positive Qualities.
                        foreach (XmlNode objXmlQuality in objXmlItem.SelectNodes("positive/quality"))
                        {
                            if (objXmlQuality["hide"] != null)
                                continue;
                            XmlNode objNode = objXmlItemDocument.SelectSingleNode("/chummer/qualities/quality[(" + _objCharacter.Options.BookXPath() + ") and name = \"" + objXmlQuality.InnerText + "\"]");
                            if (objNode == null)
                                continue;
                            TreeNode objChild = new TreeNode
                            {
                                Text = objNode["translate"]?.InnerText ?? objXmlQuality.InnerText
                            };

                            string strSelect = objXmlQuality.Attributes["select"].InnerText;
                            if (!string.IsNullOrEmpty(strSelect))
                                objChild.Text += $" ({LanguageManager.TranslateExtra(strSelect, GlobalOptions.Language)})";
                            objParent.Nodes.Add(objChild);
                            objParent.Expand();
                        }

                        // Negative Qualities.
                        foreach (XmlNode objXmlQuality in objXmlItem.SelectNodes("negative/quality"))
                        {
                            if (objXmlQuality["hide"] != null)
                                continue;
                            XmlNode objNode = objXmlItemDocument.SelectSingleNode("/chummer/qualities/quality[(" + _objCharacter.Options.BookXPath() + ") and name = \"" + objXmlQuality.InnerText + "\"]");
                            if (objNode == null)
                                continue;
                            TreeNode objChild = new TreeNode
                            {
                                Text = objNode["translate"]?.InnerText ?? objXmlQuality.InnerText
                            };

                            string strSelect = objXmlQuality.Attributes["select"].InnerText;
                            if (!string.IsNullOrEmpty(strSelect))
                                objChild.Text += $" ({LanguageManager.TranslateExtra(strSelect, GlobalOptions.Language)})";
                            objParent.Nodes.Add(objChild);
                            objParent.Expand();
                        }
                        break;
                    case "nuyenbp":
                        objParent.Text = LanguageManager.GetString("String_SelectPACKSKit_Nuyen", GlobalOptions.Language);
                        treContents.Nodes.Add(objParent);
                        TreeNode objNuyenChild = new TreeNode
                        {
                            Text = LanguageManager.GetString("String_SelectPACKSKit_StartingNuyenBP", GlobalOptions.Language) + ' ' + objXmlItem.InnerText
                        };
                        objParent.Nodes.Add(objNuyenChild);
                        objParent.Expand();
                        break;
                    case "skills":
                        objXmlItemDocument = XmlManager.Load("skills.xml");

                        objParent.Text = LanguageManager.GetString("String_SelectPACKSKit_Skills", GlobalOptions.Language);
                        treContents.Nodes.Add(objParent);
                        foreach (XmlNode objXmlSkill in objXmlItem.SelectNodes("skill"))
                        {
                            if (objXmlSkill["hide"] != null)
                                continue;
                            string strName = objXmlSkill["name"].InnerText;
                            XmlNode objNode = objXmlItemDocument.SelectSingleNode("/chummer/skills/skill[name = \"" + strName + "\"]");
                            if (objNode["hide"] != null)
                                continue;
                            TreeNode objChild = new TreeNode
                            {
                                Text = objNode["translate"]?.InnerText ?? strName
                            };
                            objChild.Text += ' ' + objXmlSkill["rating"].InnerText;

                            string strSpec = objXmlSkill["spec"]?.InnerText;
                            if (!string.IsNullOrEmpty(strSpec))
                                objChild.Text += " (" + strSpec + ')';
                            objParent.Nodes.Add(objChild);
                            objParent.Expand();
                        }
                        foreach (XmlNode objXmlSkill in objXmlItem.SelectNodes("skillgroup"))
                        {
                            if (objXmlSkill["hide"] != null)
                                continue;
                            string strName = objXmlSkill["name"].InnerText;
                            XmlNode objNode = objXmlItemDocument.SelectSingleNode("/chummer/skillgroups/name[. = \"" + strName + "\"]");
                            if (objNode["hide"] != null)
                                continue;
                            TreeNode objChild = new TreeNode
                            {
                                Text = objNode.Attributes["translate"]?.InnerText ?? strName
                            };
                            objChild.Text += $" {LanguageManager.GetString("String_SelectPACKSKit_Group", GlobalOptions.Language)} {objXmlSkill["rating"].InnerText}";

                            string strSpec = objXmlSkill["spec"]?.InnerText;
                            if (!string.IsNullOrEmpty(strSpec))
                                objChild.Text += " (" + strSpec + ')';
                            objParent.Nodes.Add(objChild);
                            objParent.Expand();
                        }
                        break;
                    case "knowledgeskills":
                        objXmlItemDocument = XmlManager.Load("skills.xml");

                        objParent.Text = LanguageManager.GetString("String_SelectPACKSKit_KnowledgeSkills", GlobalOptions.Language);
                        treContents.Nodes.Add(objParent);
                        foreach (XmlNode objXmlSkill in objXmlItem.SelectNodes("skill"))
                        {
                            if (objXmlSkill["hide"] != null)
                                continue;
                            TreeNode objChild = new TreeNode();
                            string strName = objXmlSkill["name"].InnerText;
                            if (!string.IsNullOrEmpty(strName))
                            {
                                XmlNode objNode = objXmlItemDocument.SelectSingleNode("/chummer/knowledgeskills/skill[name = \"" + strName + "\"]");
                                if (objNode["hide"] != null)
                                    continue;
                                objChild.Text = objNode?["translate"]?.InnerText ?? strName;
                            }
                            objChild.Text += ' ' + objXmlSkill["rating"].InnerText;

                            string strSpec = objXmlSkill["spec"]?.InnerText;
                            if (!string.IsNullOrEmpty(strSpec))
                                objChild.Text += " (" + strSpec + ')';
                            objParent.Nodes.Add(objChild);
                            objParent.Expand();
                        }
                        break;
                    case "selectmartialart":
                        {
                            objParent.Text = LanguageManager.GetString("String_SelectPACKSKit_SelectMartialArt", GlobalOptions.Language);
                            treContents.Nodes.Add(objParent);

                            int intRating = Convert.ToInt32(objXmlItem.Attributes["rating"]?.InnerText ?? "1");
                            string strSelect = objXmlItem.Attributes["select"]?.InnerText ?? LanguageManager.GetString("String_SelectPACKSKit_SelectMartialArt", GlobalOptions.Language);

                            TreeNode objMartialArt = new TreeNode
                            {
                                Text = strSelect + ' ' + LanguageManager.GetString("String_Rating", GlobalOptions.Language) + ' ' + intRating.ToString()
                            };
                            objParent.Nodes.Add(objMartialArt);
                            objParent.Expand();
                            break;
                        }
                    case "martialarts":
                        objXmlItemDocument = XmlManager.Load("martialarts.xml");

                        objParent.Text = LanguageManager.GetString("String_SelectPACKSKit_MartialArts", GlobalOptions.Language);
                        treContents.Nodes.Add(objParent);
                        foreach (XmlNode objXmlArt in objXmlItem.SelectNodes("martialart"))
                        {
                            if (objXmlArt["hide"] != null)
                                continue;
                            string strName = objXmlArt["name"].InnerText;
                            XmlNode objNode = objXmlItemDocument.SelectSingleNode("/chummer/martialarts/martialart[(" + _objCharacter.Options.BookXPath() + ") and name = \"" + strName + "\"]");
                            if (objNode == null)
                                continue;
                            TreeNode objChild = new TreeNode
                            {
                                Text = objNode["translate"]?.InnerText ?? strName
                            };
                            objChild.Text += ' ' + objXmlArt["rating"].InnerText;

                            // Check for Advantages.
                            foreach (XmlNode objXmlAdvantage in objXmlArt.SelectNodes("techniques/technique"))
                            {
                                if (objXmlAdvantage["hide"] != null)
                                    continue;
                                string strAdvantageName = objXmlAdvantage["name"].InnerText;
                                XmlNode objChildNode = objXmlItemDocument.SelectSingleNode("/chummer/techniques/technique[(" + _objCharacter.Options.BookXPath() + ") and name = \"" + strAdvantageName + "\"]");
                                if (objNode == null)
                                    continue;
                                TreeNode objChildChild = new TreeNode
                                {
                                    Text = objChildNode.Attributes["translate"]?.InnerText ?? strAdvantageName
                                };

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
                            string strAdvantageName = objXmlManeuver["name"].InnerText;
                            XmlNode objNode = objXmlItemDocument.SelectSingleNode("/chummer/maneuvers/maneuver[(" + _objCharacter.Options.BookXPath() + ") and name = \"" + strAdvantageName + "\"]");
                            if (objNode == null)
                                continue;
                            TreeNode objChild = new TreeNode
                            {
                                Text = objNode["translate"]?.InnerText ?? strAdvantageName
                            };
                            objChild.Text += ' ' + objXmlManeuver["rating"].InnerText;

                            objParent.Nodes.Add(objChild);
                            objParent.Expand();
                        }
                        break;
                    case "powers":
                        {
                            objXmlItemDocument = XmlManager.Load("powers.xml");

                            objParent.Text = LanguageManager.GetString("String_SelectPACKSKit_Powers", GlobalOptions.Language);
                            treContents.Nodes.Add(objParent);
                            foreach (XmlNode objXmlPower in objXmlItem.SelectNodes("power"))
                            {
                                if (objXmlPower["hide"] != null)
                                    continue;
                                string strName = objXmlPower["name"].InnerText;
                                XmlNode objNode = objXmlItemDocument.SelectSingleNode("/chummer/powers/power[(" + _objCharacter.Options.BookXPath() + ") and name = \"" + strName + "\"]");
                                if (objNode == null)
                                    continue;
                                TreeNode objChild = new TreeNode
                                {
                                    Text = objNode["translate"]?.InnerText ?? strName
                                };

                                string strSelect = objXmlPower.SelectSingleNode("name/@select")?.InnerText ?? string.Empty;
                                if (!string.IsNullOrEmpty(strSelect))
                                    objChild.Text += " (" + strSelect + ')';
                                string strRating = objXmlPower["rating"]?.InnerText;
                                if (!string.IsNullOrEmpty(strRating))
                                    objChild.Text += ' ' + strRating;
                                objParent.Nodes.Add(objChild);
                                objParent.Expand();
                            }
                            break;
                        }
                    case "programs":
                        objXmlItemDocument = XmlManager.Load("complexforms.xml");

                        objParent.Text = LanguageManager.GetString("String_SelectPACKSKit_Programs", GlobalOptions.Language);
                        treContents.Nodes.Add(objParent);
                        foreach (XmlNode objXmlProgram in objXmlItem.SelectNodes("program"))
                        {
                            if (objXmlProgram["hide"] != null)
                                continue;
                            string strName = objXmlProgram["name"].InnerText;
                            XmlNode objNode = objXmlItemDocument.SelectSingleNode("/chummer/complexforms/complexform[(" + _objCharacter.Options.BookXPath() + ") and name = \"" + strName + "\"]");
                            if (objNode == null)
                                continue;
                            TreeNode objChild = new TreeNode
                            {
                                Text = objNode["translate"]?.InnerText ?? strName
                            };
                            objChild.Text += ' ' + objXmlProgram["rating"].InnerText;

                            // Check for Program Options.
                            foreach (XmlNode objXmlOption in objXmlProgram.SelectNodes("options/option"))
                            {
                                if (objXmlOption["hide"] != null)
                                    continue;
                                string strOptionName = objXmlOption["name"].InnerText;
                                XmlNode objChildNode = objXmlItemDocument.SelectSingleNode("/chummer/options/option[(" + _objCharacter.Options.BookXPath() + ") and name = \"" + strOptionName + "\"]");
                                if (objNode == null)
                                    continue;
                                TreeNode objChildChild = new TreeNode
                                {
                                    Text = objChildNode["translate"]?.InnerText ?? strOptionName
                                };
                                
                                if (objXmlOption["rating"] != null)
                                    objChildChild.Text += ' ' + objXmlOption["rating"].InnerText;
                                objChild.Nodes.Add(objChildChild);
                                objChild.Expand();
                            }

                            objParent.Nodes.Add(objChild);
                            objParent.Expand();
                        }
                        break;
                    case "spells":
                        objXmlItemDocument = XmlManager.Load("spells.xml");

                        objParent.Text = LanguageManager.GetString("String_SelectPACKSKit_Spells", GlobalOptions.Language);
                        treContents.Nodes.Add(objParent);
                        foreach (XmlNode objXmlSpell in objXmlItem.SelectNodes("spell"))
                        {
                            if (objXmlSpell["hide"] != null)
                                continue;
                            string strName = objXmlSpell.InnerText;
                            XmlNode objNode = objXmlItemDocument.SelectSingleNode("/chummer/spells/spell[(" + _objCharacter.Options.BookXPath() + ") and name = \"" + strName + "\"]");
                            if (objNode["hide"] == null)
                                continue;
                            TreeNode objChild = new TreeNode
                            {
                                Text = objNode["translate"]?.InnerText ?? strName
                            };

                            string strSelect = objXmlSpell.Attributes["select"]?.InnerText;
                            if (!string.IsNullOrEmpty(strSelect))
                                objChild.Text += " (" + strSelect + ')';
                            objParent.Nodes.Add(objChild);
                            objParent.Expand();
                        }
                        break;
                    case "spirits":

                        objParent.Text = LanguageManager.GetString("String_SelectPACKSKit_Spirits", GlobalOptions.Language);
                        treContents.Nodes.Add(objParent);
                        foreach (XmlNode objXmlSpirit in objXmlItem.SelectNodes("spirit"))
                        {
                            if (objXmlSpirit["hide"] != null)
                                continue;
                            TreeNode objChild = new TreeNode
                            {
                                Text = objXmlSpirit["name"].InnerText + " (" + LanguageManager.GetString("Label_Spirit_Force", GlobalOptions.Language) + ' ' + objXmlSpirit["force"].InnerText + ", " + LanguageManager.GetString("Label_Spirit_ServicesOwed", GlobalOptions.Language) + ' ' + objXmlSpirit["services"].InnerText + ')'
                            };
                            objParent.Nodes.Add(objChild);
                            objParent.Expand();
                        }
                        break;
                    case "lifestyles":

                        objParent.Text = LanguageManager.GetString("String_SelectPACKSKit_Lifestyles", GlobalOptions.Language);
                        treContents.Nodes.Add(objParent);
                        foreach (XmlNode objXmlLifestyle in objXmlItem.SelectNodes("lifestyle"))
                        {
                            if (objXmlLifestyle["hide"] != null)
                                continue;

                            string strIncrement = objXmlLifestyle["increment"]?.InnerText;
                            if (objXmlLifestyle["type"]?.InnerText.ToLower() == "safehouse")
                                strIncrement = "week";
                            string strIncrementString;
                            int intPermanentAmount;
                            switch (strIncrement)
                            {
                                case "day":
                                case "Day":
                                    strIncrementString = LanguageManager.GetString("String_Days", GlobalOptions.Language);
                                    intPermanentAmount = 3044;
                                    break;
                                case "week":
                                case "Week":
                                    strIncrementString = LanguageManager.GetString("String_Weeks", GlobalOptions.Language);
                                    intPermanentAmount = 435;
                                    break;
                                default:
                                    strIncrementString = LanguageManager.GetString("String_Months", GlobalOptions.Language);
                                    intPermanentAmount = 100;
                                    break;
                            }

                            TreeNode objChild = new TreeNode
                            {
                                Text = string.Format("{0} {1} {2}", (objXmlLifestyle["translate"] ?? objXmlLifestyle["name"]).InnerText, objXmlLifestyle["months"].InnerText, strIncrementString + LanguageManager.GetString("Label_LifestylePermanent", GlobalOptions.Language).Replace("{0}", intPermanentAmount.ToString(GlobalOptions.CultureInfo)))
                            };

                            // Check for Qualities.
                            foreach (XmlNode objXmlQuality in objXmlLifestyle.SelectNodes("qualities/quality"))
                            {
                                if (objXmlQuality["hide"] != null)
                                    continue;
                                TreeNode objChildChild = new TreeNode
                                {
                                    Text = objXmlQuality.InnerText
                                };
                                objChild.Nodes.Add(objChildChild);
                                objChild.Expand();
                            }

                            objParent.Nodes.Add(objChild);
                            objParent.Expand();
                        }
                        break;
                    case "cyberwares":
                        objXmlItemDocument = XmlManager.Load("cyberware.xml");

                        objParent.Text = LanguageManager.GetString("String_SelectPACKSKit_Cyberware", GlobalOptions.Language);
                        treContents.Nodes.Add(objParent);
                        foreach (XmlNode objXmlCyberware in objXmlItem.SelectNodes("cyberware"))
                        {
                            if (objXmlCyberware["hide"] != null)
                                continue;
                            string strName = objXmlCyberware["name"].InnerText;
                            XmlNode objNode = objXmlItemDocument.SelectSingleNode("/chummer/cyberwares/cyberware[(" + _objCharacter.Options.BookXPath() + ") and name = \"" + strName + "\"]");
                            if (objNode == null)
                                continue;
                            TreeNode objChild = new TreeNode
                            {
                                Text = objNode["translate"]?.InnerText ?? strName
                            };

                            string strRating = objXmlCyberware["rating"]?.InnerText;
                            if (!string.IsNullOrEmpty(strRating))
                                objChild.Text += ' ' + LanguageManager.GetString("String_Rating", GlobalOptions.Language) + ' ' + strRating;
                            objChild.Text += " (" + objXmlCyberware["grade"].InnerText + ')';

                            // Check for children.
                            foreach (XmlNode objXmlChild in objXmlCyberware.SelectNodes("cyberwares/cyberware"))
                            {
                                if (objXmlChild["hide"] != null)
                                    continue;
                                string strChildName = objXmlChild["name"].InnerText;
                                XmlNode objChildNode = objXmlItemDocument.SelectSingleNode("/chummer/cyberwares/cyberware[(" + _objCharacter.Options.BookXPath() + ") and name = \"" + strChildName + "\"]");
                                if (objChildNode == null)
                                    continue;
                                TreeNode objChildChild = new TreeNode
                                {
                                    Text = objChildNode["translate"]?.InnerText ?? strChildName
                                };

                                strRating = objXmlChild["rating"]?.InnerText;
                                if (!string.IsNullOrEmpty(strRating))
                                    objChildChild.Text += ' ' + LanguageManager.GetString("String_Rating", GlobalOptions.Language) + ' ' + strRating;

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

                        objParent.Text = LanguageManager.GetString("String_SelectPACKSKit_Bioware", GlobalOptions.Language);
                        treContents.Nodes.Add(objParent);
                        foreach (XmlNode objXmlBioware in objXmlItem.SelectNodes("bioware"))
                        {
                            if (objXmlBioware["hide"] != null)
                                continue;
                            string strName = objXmlBioware["name"].InnerText;
                            XmlNode objNode = objXmlItemDocument.SelectSingleNode("/chummer/biowares/bioware[(" + _objCharacter.Options.BookXPath() + ") and name = \"" + strName + "\"]");
                            if (objNode == null)
                                continue;
                            TreeNode objChild = new TreeNode
                            {
                                Text = objNode["translate"]?.InnerText ?? strName
                            };

                            string strRating = objXmlBioware["rating"]?.InnerText;
                            if (!string.IsNullOrEmpty(strRating))
                                objChild.Text += ' ' + LanguageManager.GetString("String_Rating", GlobalOptions.Language) + ' ' + strRating;
                            objChild.Text += " (" + objXmlBioware["grade"].InnerText + ')';

                            // Check for children.
                            foreach (XmlNode objXmlChild in objXmlBioware.SelectNodes("biowares/bioware"))
                            {
                                if (objXmlChild["hide"] != null)
                                    continue;
                                string strChildName = objXmlChild["name"].InnerText;
                                XmlNode objChildNode = objXmlItemDocument.SelectSingleNode("/chummer/biowares/bioware[(" + _objCharacter.Options.BookXPath() + ") and name = \"" + strChildName + "\"]");
                                if (objChildNode == null)
                                    continue;
                                TreeNode objChildChild = new TreeNode
                                {
                                    Text = objChildNode["translate"]?.InnerText ?? strChildName
                                };

                                strRating = objXmlChild["rating"]?.InnerText;
                                if (!string.IsNullOrEmpty(strRating))
                                    objChildChild.Text += ' ' + LanguageManager.GetString("String_Rating", GlobalOptions.Language) + ' ' + strRating;

                                foreach (XmlNode objXmlGearNode in objXmlChild.SelectNodes("gears/gear"))
                                    WriteGear(objXmlGearDocument, objXmlGearNode, objChildChild);
                                objChild.Expand();

                                objChild.Nodes.Add(objChildChild);
                                objChild.Expand();
                            }

                            foreach (XmlNode objXmlGearNode in objXmlBioware.SelectNodes("gears/gear"))
                                WriteGear(objXmlGearDocument, objXmlGearNode, objChild);
                            objChild.Expand();

                            objParent.Nodes.Add(objChild);
                            objParent.Expand();
                        }
                        break;
                    case "armors":
                        objXmlItemDocument = XmlManager.Load("armor.xml");

                        objParent.Text = LanguageManager.GetString("String_SelectPACKSKit_Armor", GlobalOptions.Language);
                        treContents.Nodes.Add(objParent);
                        foreach (XmlNode objXmlArmor in objXmlItem.SelectNodes("armor"))
                        {
                            if (objXmlArmor["hide"] != null)
                                continue;
                            string strName = objXmlArmor["name"].InnerText;
                            XmlNode objNode = objXmlItemDocument.SelectSingleNode("/chummer/armors/armor[(" + _objCharacter.Options.BookXPath() + ") and name = \"" + strName + "\"]");
                            if (objNode == null)
                                continue;
                            TreeNode objChild = new TreeNode
                            {
                                Text = objNode["translate"]?.InnerText ?? strName
                            };

                            // Check for children.
                            foreach (XmlNode objXmlChild in objXmlArmor.SelectNodes("mods/mod"))
                            {
                                if (objXmlChild["hide"] != null)
                                    continue;
                                string strChildName = objXmlChild["name"].InnerText;
                                XmlNode objChildNode = objXmlItemDocument.SelectSingleNode("/chummer/mods/mod[(" + _objCharacter.Options.BookXPath() + ") and name = \"" + strChildName + "\"]");
                                if (objChildNode == null)
                                    continue;
                                TreeNode objChildChild = new TreeNode
                                {
                                    Text = objChildNode["translate"]?.InnerText ?? strChildName
                                };

                                string strRating = objXmlChild["rating"]?.InnerText;
                                if (!string.IsNullOrEmpty(strRating))
                                    objChildChild.Text += ' ' + LanguageManager.GetString("String_Rating", GlobalOptions.Language) + ' ' + strRating;

                                foreach (XmlNode objXmlGearNode in objXmlChild.SelectNodes("gears/gear"))
                                    WriteGear(objXmlGearDocument, objXmlGearNode, objChildChild);

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
                        objParent.Text = LanguageManager.GetString("String_SelectPACKSKit_Weapons", GlobalOptions.Language);
                        treContents.Nodes.Add(objParent);
                        foreach (XmlNode objXmlWeapon in objXmlItem.SelectNodes("weapon"))
                        {
                            if (objXmlWeapon["hide"] != null)
                                continue;
                            string strName = objXmlWeapon["name"].InnerText;
                            XmlNode objNode = objXmlWeaponDocument.SelectSingleNode("/chummer/weapons/weapon[(" + _objCharacter.Options.BookXPath() + ") and name = \"" + strName + "\"]");
                            if (objNode == null)
                                continue;
                            TreeNode objChild = new TreeNode
                            {
                                Text = objNode["translate"]?.InnerText ?? strName
                            };

                            // Check for Weapon Accessories.
                            foreach (XmlNode objXmlAccessory in objXmlWeapon.SelectNodes("accessories/accessory"))
                            {
                                if (objXmlAccessory["hide"] != null)
                                    continue;
                                strName = objXmlAccessory["name"].InnerText;
                                XmlNode objChildNode = objXmlWeaponDocument.SelectSingleNode("/chummer/accessories/accessory[(" + _objCharacter.Options.BookXPath() + ") and name = \"" + strName + "\"]");
                                if (objChildNode == null)
                                    continue;
                                TreeNode objChildChild = new TreeNode
                                {
                                    Text = objChildNode["translate"]?.InnerText ?? strName
                                };

                                string strRating = objXmlAccessory["rating"]?.InnerText;
                                if (!string.IsNullOrEmpty(strRating))
                                    objChildChild.Text += ' ' + LanguageManager.GetString("String_Rating", GlobalOptions.Language) + ' ' + strRating;

                                foreach (XmlNode objXmlGearNode in objXmlAccessory.SelectNodes("gears/gear"))
                                    WriteGear(objXmlGearDocument, objXmlGearNode, objChildChild);
                                objChildChild.Expand();

                                objChild.Nodes.Add(objChildChild);
                                objChild.Expand();
                            }

                            strName = objXmlWeapon["underbarrel"]?.InnerText;
                            // Check for Underbarrel Weapons.
                            if (!string.IsNullOrEmpty(strName))
                            {
                                if (objXmlWeapon["hide"] != null)
                                    continue;
                                
                                XmlNode objChildNode = objXmlWeaponDocument.SelectSingleNode("/chummer/weapons/weapon[(" + _objCharacter.Options.BookXPath() + ") and name = \"" + strName + "\"]");
                                if (objChildNode == null)
                                    continue;
                                TreeNode objChildChild = new TreeNode
                                {
                                    Text = objChildNode["translate"]?.InnerText ?? strName
                                };

                                objChild.Nodes.Add(objChildChild);
                                objChild.Expand();
                            }

                            objParent.Nodes.Add(objChild);
                            objParent.Expand();
                        }
                        break;
                    case "gears":
                        objParent.Text = LanguageManager.GetString("String_SelectPACKSKit_Gear", GlobalOptions.Language);
                        treContents.Nodes.Add(objParent);
                        foreach (XmlNode objXmlGear in objXmlItem.SelectNodes("gear"))
                        {
                            if (objXmlGear["hide"] != null)
                                continue;
                            WriteGear(objXmlGearDocument, objXmlGear, objParent);
                            objParent.Expand();
                        }
                        break;
                    case "vehicles":
                        objXmlItemDocument = XmlManager.Load("vehicles.xml");

                        objParent.Text = LanguageManager.GetString("String_SelectPACKSKit_Vehicles", GlobalOptions.Language);
                        treContents.Nodes.Add(objParent);
                        foreach (XmlNode objXmlVehicle in objXmlItem.SelectNodes("vehicle"))
                        {
                            if (objXmlVehicle["hide"] != null)
                                continue;
                            string strName = objXmlVehicle["name"].InnerText;
                            XmlNode objNode = objXmlItemDocument.SelectSingleNode("/chummer/vehicles/vehicle[(" + _objCharacter.Options.BookXPath() + ") and name = \"" + strName + "\"]");
                            if (objNode == null)
                                continue;
                            TreeNode objChild = new TreeNode
                            {
                                Text = objNode["translate"]?.InnerText ?? strName
                            };

                            // Check for children.
                            foreach (XmlNode objXmlMod in objXmlVehicle.SelectNodes("mods/mod"))
                            {
                                if (objXmlMod["hide"] != null)
                                    continue;
                                strName = objXmlMod["name"].InnerText;
                                XmlNode objChildNode = objXmlItemDocument.SelectSingleNode("/chummer/mods/mod[(" + _objCharacter.Options.BookXPath() + ") and name = \"" + strName + "\"]");
                                if (objChildNode == null)
                                    continue;
                                TreeNode objChildChild = new TreeNode
                                {
                                    Text = objChildNode["translate"]?.InnerText ?? strName
                                };

                                string strRating = objXmlMod["rating"]?.InnerText;
                                if (!string.IsNullOrEmpty(strRating))
                                    objChildChild.Text += ' ' + LanguageManager.GetString("String_Rating", GlobalOptions.Language) + ' ' + strRating;
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
                                strName = objXmlWeapon["name"].InnerText;
                                XmlNode objChildNode = objXmlWeaponDocument.SelectSingleNode("/chummer/weapons/weapon[(" + _objCharacter.Options.BookXPath() + ") and name = \"" + strName + "\"]");
                                if (objChildNode == null)
                                    continue;
                                TreeNode objChildChild = new TreeNode
                                {
                                    Text = objChildNode["translate"]?.InnerText ?? strName
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
            string strSelectedKit = lstKits.SelectedValue?.ToString();
            if (string.IsNullOrEmpty(strSelectedKit))
                return;

            if (MessageBox.Show(LanguageManager.GetString("Message_DeletePACKSKit", GlobalOptions.Language).Replace("{0}", strSelectedKit), LanguageManager.GetString("MessageTitle_Delete", GlobalOptions.Language), MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.No)
                return;

            // Delete the selectec custom PACKS Kit.
            // Find a custom PACKS Kit with the name. This is done without the XmlManager since we need to check each file individually.
            XmlDocument objXmlDocument = new XmlDocument();
            string strCustomPath = Path.Combine(Application.StartupPath, "data");
            foreach (string strFile in Directory.GetFiles(strCustomPath, "custom*_packs.xml"))
            {
                try
                {
                    using (StreamReader objStreamReader = new StreamReader(strFile, Encoding.UTF8, true))
                    {
                        objXmlDocument.Load(objStreamReader);
                    }
                }
                catch (IOException)
                {
                    continue;
                }
                catch (XmlException)
                {
                    continue;
                }
                XmlNodeList objXmlPACKSList = objXmlDocument.SelectNodes("/chummer/packs/pack[name = \"" + strSelectedKit + "\" and category = \"Custom\"]");
                if (objXmlPACKSList.Count > 0)
                {
                    // Read in the entire file.
                    XmlDocument objXmlCurrentDocument = new XmlDocument();
                    try
                    {
                        using (StreamReader objStreamReader = new StreamReader(strFile, Encoding.UTF8, true))
                        {
                            objXmlCurrentDocument.Load(objStreamReader);
                        }
                    }
                    catch (IOException)
                    {
                        continue;
                    }
                    catch (XmlException)
                    {
                        continue;
                    }

                    FileStream objStream = new FileStream(strFile, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
                    XmlTextWriter objWriter = new XmlTextWriter(objStream, Encoding.UTF8)
                    {
                        Formatting = Formatting.Indented,
                        Indentation = 1,
                        IndentChar = '\t'
                    };
                    objWriter.WriteStartDocument();

                    // <chummer>
                    objWriter.WriteStartElement("chummer");
                    // <packs>
                    objWriter.WriteStartElement("packs");

                    // If this is not a new file, write out the current contents.
                    XmlNodeList objXmlNodeList = objXmlCurrentDocument.SelectNodes("/chummer/packs/*");
                    foreach (XmlNode objXmlNode in objXmlNodeList)
                    {
                        if (objXmlNode["name"].InnerText != strSelectedKit)
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
            string[] objSelectedKit = strSelectedKit.Split('<');
            _strSelectedKit = objSelectedKit[0];
            s_StrSelectCategory = objSelectedKit[1];
            DialogResult = DialogResult.OK;
        }

        private void WriteGear(XmlDocument objXmlItemDocument, XmlNode objXmlGear, TreeNode objParent)
        {
            XmlNode xmlNameNode = objXmlGear["name"];
            string strName = xmlNameNode.InnerText;
            string strCategory = objXmlGear["category"]?.InnerText;
            XmlNode objNode = !string.IsNullOrEmpty(strCategory)
                ? objXmlItemDocument.SelectSingleNode("/chummer/gears/gear[(" + _objCharacter.Options.BookXPath() + ") and name = \"" + strName + "\" and category = \"" + strCategory + "\"]")
                : objXmlItemDocument.SelectSingleNode("/chummer/gears/gear[(" + _objCharacter.Options.BookXPath() + ") and name = \"" + strName + "\"]");

            if (objNode != null)
            {
                TreeNode objChild = new TreeNode
                {
                    Text = objNode["translate"]?.InnerText ?? strName
                };

                string strExtra = xmlNameNode.Attributes["select"]?.InnerText;
                if (!string.IsNullOrEmpty(strExtra))
                    objChild.Text += " (" + strExtra + ')';
                strExtra = objXmlGear["rating"]?.InnerText;
                if (!string.IsNullOrEmpty(strExtra))
                    objChild.Text += ' ' + LanguageManager.GetString("String_Rating", GlobalOptions.Language) + ' ' + strExtra;
                strExtra = objXmlGear["qty"]?.InnerText;
                if (!string.IsNullOrEmpty(strExtra))
                    objChild.Text += " x" + strExtra;

                objParent.Nodes.Add(objChild);

                // Check for children.
                foreach (XmlNode objXmlChild in objXmlGear.SelectNodes("gears/gear"))
                {
                    WriteGear(objXmlItemDocument, objXmlChild, objChild);
                }

                objChild.Expand();
            }
        }
        #endregion
    }
}
