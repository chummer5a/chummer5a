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
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;

namespace Chummer
{
    public partial class SelectPACKSKit : Form
    {
        private string _strSelectedKit = string.Empty;
        private bool _blnAddAgain;
        private static string _strSelectCategory = string.Empty;
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

        private List<ListItem> _lstCategory;

        #region Control Events

        public SelectPACKSKit(Character objCharacter)
        {
            _objCharacter = objCharacter ?? throw new ArgumentNullException(nameof(objCharacter));
            InitializeComponent();
            this.UpdateLightDarkMode();
            this.TranslateWinForm();
            this.UpdateParentForToolTipControls();
            // Load the PACKS information.
            _xmlBaseChummerNode = objCharacter.LoadDataXPath("packs.xml").SelectSingleNodeAndCacheExpression("/chummer");
            _xmlGearsBaseGearsNode = objCharacter.LoadDataXPath("gear.xml").SelectSingleNodeAndCacheExpression("/chummer/gears");
            _xmlWeaponsBaseChummerNode = objCharacter.LoadDataXPath("weapons.xml").SelectSingleNodeAndCacheExpression("/chummer");
            _xmlArmorBaseChummerNode = objCharacter.LoadDataXPath("armor.xml").SelectSingleNodeAndCacheExpression("/chummer");
            _xmlQualitiesBaseQualitiesNode = objCharacter.LoadDataXPath("qualities.xml").SelectSingleNodeAndCacheExpression("/chummer/qualities");
            _xmlSkillsBaseChummerNode = objCharacter.LoadDataXPath("skills.xml").SelectSingleNodeAndCacheExpression("/chummer");
            _xmlSpellsBaseSpellsNode = objCharacter.LoadDataXPath("spells.xml").SelectSingleNodeAndCacheExpression("/chummer/spells");
            _xmlComplexFormsBaseChummerNode = objCharacter.LoadDataXPath("complexforms.xml").SelectSingleNodeAndCacheExpression("/chummer");
            _xmlVehiclesBaseChummerNode = objCharacter.LoadDataXPath("vehicles.xml").SelectSingleNodeAndCacheExpression("/chummer");
            _xmlBiowareBaseChummerNode = objCharacter.LoadDataXPath("bioware.xml").SelectSingleNodeAndCacheExpression("/chummer");
            _xmlCyberwareBaseChummerNode = objCharacter.LoadDataXPath("cyberware.xml").SelectSingleNodeAndCacheExpression("/chummer");
            _xmlPowersBasePowersNode = objCharacter.LoadDataXPath("powers.xml").SelectSingleNodeAndCacheExpression("/chummer/powers");
            _xmlMartialArtsBaseChummerNode = objCharacter.LoadDataXPath("martialarts.xml").SelectSingleNodeAndCacheExpression("/chummer");
            _lstCategory = Utils.ListItemListPool.Get();
        }

        private async void SelectPACKSKit_Load(object sender, EventArgs e)
        {
            // Populate the PACKS Category list.
            foreach (XPathNavigator objXmlCategory in _xmlBaseChummerNode.SelectAndCacheExpression("categories/category[not(hide)]"))
            {
                string strInnerText = objXmlCategory.Value;
                _lstCategory.Add(new ListItem(strInnerText, objXmlCategory.SelectSingleNodeAndCacheExpression("@translate")?.Value ?? strInnerText));
            }
            _lstCategory.Sort(CompareListItems.CompareNames);

            if (_lstCategory.Count > 0)
            {
                _lstCategory.Insert(0, new ListItem("Show All", await LanguageManager.GetStringAsync("String_ShowAll").ConfigureAwait(false)));
            }

            await cboCategory.PopulateWithListItemsAsync(_lstCategory).ConfigureAwait(false);
            // Select the first Category in the list.
            await cboCategory.DoThreadSafeAsync(x =>
            {
                if (!string.IsNullOrEmpty(_strSelectCategory))
                    x.SelectedValue = _strSelectCategory;
                if (x.SelectedIndex == -1)
                    x.SelectedIndex = 0;
            }).ConfigureAwait(false);
        }

        private async void cboCategory_SelectedIndexChanged(object sender, EventArgs e)
        {
            await RefreshCategories().ConfigureAwait(false);
        }

        private async Task RefreshCategories(CancellationToken token = default)
        {
            // Update the list of Kits based on the selected Category.

            string strFilter = "not(hide)";
            string strCategory = await cboCategory.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token: token).ConfigureAwait(false);
            if (!string.IsNullOrEmpty(strCategory) && strCategory != "Show All")
                strFilter += " and category = " + strCategory.CleanXPath();
            else
            {
                using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdCategoryFilter))
                {
                    foreach (string strItem in _lstCategory.Select(x => x.Value.ToString()))
                    {
                        if (!string.IsNullOrEmpty(strItem))
                            sbdCategoryFilter.Append("category = ").Append(strItem.CleanXPath()).Append(" or ");
                    }

                    if (sbdCategoryFilter.Length > 0)
                    {
                        sbdCategoryFilter.Length -= 4;
                        strFilter += " and (" + sbdCategoryFilter.ToString() + ')';
                    }
                }
            }

            // Retrieve the list of Kits for the selected Category.
            XPathNodeIterator xmlPacksKits = _xmlBaseChummerNode.Select("packs/pack[" + strFilter + ']');
            using (new FetchSafelyFromSafeObjectPool<List<ListItem>>(Utils.ListItemListPool, out List<ListItem> lstKit))
            {
                foreach (XPathNavigator objXmlPack in xmlPacksKits)
                {
                    string strName = objXmlPack.SelectSingleNodeAndCacheExpression("name", token: token)?.Value;
                    // Separator "<" is a hack because XML does not like it when the '<' character is used in element contents, so we can safely assume that it will never show up.
                    lstKit.Add(new ListItem(
                                   strName + '<' + objXmlPack.SelectSingleNodeAndCacheExpression("category", token: token)?.Value,
                                   objXmlPack.SelectSingleNodeAndCacheExpression("translate", token: token)?.Value ?? strName));
                }

                lstKit.Sort(CompareListItems.CompareNames);
                await lstKits.PopulateWithListItemsAsync(lstKit, token: token).ConfigureAwait(false);
                if (lstKit.Count == 0)
                    await treContents.DoThreadSafeAsync(x => x.Nodes.Clear(), token: token).ConfigureAwait(false);
            }

            await cmdDelete.DoThreadSafeAsync(x => x.Visible = false, token: token).ConfigureAwait(false);
        }

        private async void lstKits_SelectedIndexChanged(object sender, EventArgs e)
        {
            string strSelectedKit = await lstKits.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString()).ConfigureAwait(false);
            if (string.IsNullOrEmpty(strSelectedKit))
            {
                await cmdDelete.DoThreadSafeAsync(x => x.Visible = false).ConfigureAwait(false);
                return;
            }

            await treContents.DoThreadSafeAsync(x => x.Nodes.Clear()).ConfigureAwait(false);
            XPathNavigator objXmlPack;
            string[] strIdentifiers = strSelectedKit.SplitFixedSizePooledArray('<', 2, StringSplitOptions.RemoveEmptyEntries);
            try
            {
                await cmdDelete.DoThreadSafeAsync(x => x.Visible = strIdentifiers[1] == "Custom").ConfigureAwait(false);
                objXmlPack = _xmlBaseChummerNode.TryGetNodeByNameOrId("packs/pack", strIdentifiers[0], "category = " + strIdentifiers[1].CleanXPath());
            }
            finally
            {
                ArrayPool<string>.Shared.Return(strIdentifiers);
            }
            if (objXmlPack == null)
                return;

            string strSpace = await LanguageManager.GetStringAsync("String_Space").ConfigureAwait(false);
            string strBookXPath = await (await _objCharacter.GetSettingsAsync().ConfigureAwait(false)).BookXPathAsync().ConfigureAwait(false);
            foreach (XPathNavigator objXmlItem in objXmlPack.SelectChildren(XPathNodeType.Element))
            {
                if (objXmlItem.SelectSingleNodeAndCacheExpression("hide") != null)
                    continue;
                TreeNode objParent = new TreeNode();
                switch (objXmlItem.Name)
                {
                    case "attributes":
                        objParent.Text = await LanguageManager.GetStringAsync("String_SelectPACKSKit_Attributes").ConfigureAwait(false);
                        foreach (XPathNavigator objXmlAttribute in objXmlItem.SelectChildren(XPathNodeType.Element))
                        {
                            if (objXmlAttribute.SelectSingleNodeAndCacheExpression("hide") != null)
                                continue;
                            string strNameUpper = objXmlAttribute.Name.ToUpperInvariant();
                            TreeNode objChild = new TreeNode
                            {
                                Text = await LanguageManager.GetStringAsync("String_Attribute" + strNameUpper + "Short").ConfigureAwait(false) + strSpace + (objXmlAttribute.ValueAsInt - (6 - await (await _objCharacter.GetAttributeAsync(strNameUpper).ConfigureAwait(false)).GetMetatypeMaximumAsync().ConfigureAwait(false))).ToString(GlobalSettings.CultureInfo)
                            };
                            objParent.Nodes.Add(objChild);
                        }
                        break;

                    case "qualities":
                        objParent.Text = await LanguageManager.GetStringAsync("String_SelectPACKSKit_Qualities").ConfigureAwait(false);
                        // Positive Qualities.
                        foreach (XPathNavigator objXmlQuality in objXmlItem.SelectAndCacheExpression("positive/quality"))
                        {
                            if (objXmlQuality.SelectSingleNodeAndCacheExpression("hide") != null)
                                continue;
                            XPathNavigator objNode = _xmlQualitiesBaseQualitiesNode.SelectSingleNode("quality[(" + strBookXPath + ") and name = " + objXmlQuality.Value.CleanXPath() + ']');
                            if (objNode == null)
                                continue;
                            string strText = objNode.SelectSingleNodeAndCacheExpression("translate")?.Value
                                             ?? objXmlQuality.Value;
                            string strSelect = objXmlQuality.SelectSingleNodeAndCacheExpression("@select")?.Value;
                            if (!string.IsNullOrEmpty(strSelect))
                                strText += strSpace + '(' + await _objCharacter.TranslateExtraAsync(strSelect).ConfigureAwait(false) + ')';
                            TreeNode objChild = new TreeNode
                            {
                                Text = strText
                            };
                            objParent.Nodes.Add(objChild);
                        }

                        // Negative Qualities.
                        foreach (XPathNavigator objXmlQuality in objXmlItem.SelectAndCacheExpression("negative/quality"))
                        {
                            if (objXmlQuality.SelectSingleNodeAndCacheExpression("hide") != null)
                                continue;
                            XPathNavigator objNode = _xmlQualitiesBaseQualitiesNode.SelectSingleNode("quality[(" + strBookXPath + ") and name = " + objXmlQuality.Value.CleanXPath() + ']');
                            if (objNode == null)
                                continue;
                            string strText = objNode.SelectSingleNodeAndCacheExpression("translate")?.Value
                                             ?? objXmlQuality.Value;
                            string strSelect = objXmlQuality.SelectSingleNodeAndCacheExpression("@select")?.Value;
                            if (!string.IsNullOrEmpty(strSelect))
                                strText += strSpace + '(' + await _objCharacter.TranslateExtraAsync(strSelect).ConfigureAwait(false) + ')';
                            TreeNode objChild = new TreeNode
                            {
                                Text = strText
                            };
                            objParent.Nodes.Add(objChild);
                        }
                        break;

                    case "nuyenbp":
                        objParent.Text = await LanguageManager.GetStringAsync("String_SelectPACKSKit_Nuyen").ConfigureAwait(false);
                        TreeNode objNuyenChild = new TreeNode
                        {
                            Text = await LanguageManager.GetStringAsync("String_SelectPACKSKit_StartingNuyenBP").ConfigureAwait(false) + strSpace + objXmlItem.Value
                        };
                        objParent.Nodes.Add(objNuyenChild);
                        break;

                    case "skills":
                        objParent.Text = await LanguageManager.GetStringAsync("String_SelectPACKSKit_Skills").ConfigureAwait(false);
                        foreach (XPathNavigator objXmlSkill in objXmlItem.SelectAndCacheExpression("skill"))
                        {
                            if (objXmlSkill.SelectSingleNodeAndCacheExpression("hide") != null)
                                continue;
                            string strName = objXmlSkill.SelectSingleNodeAndCacheExpression("name").Value;
                            XPathNavigator objNode = _xmlSkillsBaseChummerNode.TryGetNodeByNameOrId("skills/skill", strName);
                            if (objNode.SelectSingleNodeAndCacheExpression("hide") != null)
                                continue;
                            string strText = (objNode.SelectSingleNodeAndCacheExpression("translate")?.Value ?? strName)
                                             + strSpace + (objXmlSkill.SelectSingleNodeAndCacheExpression("rating")
                                                                      ?.Value ?? string.Empty);
                            string strSpec = objXmlSkill.SelectSingleNodeAndCacheExpression("spec")?.Value;
                            if (!string.IsNullOrEmpty(strSpec))
                                strText += strSpace + '(' + strSpec + ')';
                            TreeNode objChild = new TreeNode
                            {
                                Text = strText
                            };
                            objParent.Nodes.Add(objChild);
                        }
                        foreach (XPathNavigator objXmlSkill in objXmlItem.SelectAndCacheExpression("skillgroup"))
                        {
                            if (objXmlSkill.SelectSingleNodeAndCacheExpression("hide") != null)
                                continue;
                            string strName = objXmlSkill.SelectSingleNodeAndCacheExpression("name").Value;
                            XPathNavigator objNode = _xmlSkillsBaseChummerNode.SelectSingleNode("skillgroups/name[. = " + strName.CleanXPath() + ']');
                            if (objNode.SelectSingleNodeAndCacheExpression("hide") != null)
                                continue;
                            string strText = (objNode.SelectSingleNodeAndCacheExpression("@translate")?.Value
                                              ?? strName) + strSpace
                                                          + await LanguageManager.GetStringAsync(
                                                              "String_SelectPACKSKit_Group").ConfigureAwait(false) + strSpace
                                                          + (objXmlSkill.SelectSingleNodeAndCacheExpression("rating")?.Value ?? string.Empty);
                            string strSpec = objXmlSkill.SelectSingleNodeAndCacheExpression("spec")?.Value;
                            if (!string.IsNullOrEmpty(strSpec))
                                strText += strSpace + '(' + strSpec + ')';
                            TreeNode objChild = new TreeNode
                            {
                                Text = strText
                            };
                            objParent.Nodes.Add(objChild);
                        }
                        break;

                    case "knowledgeskills":
                        objParent.Text = await LanguageManager.GetStringAsync("String_SelectPACKSKit_KnowledgeSkills").ConfigureAwait(false);
                        foreach (XPathNavigator objXmlSkill in objXmlItem.SelectAndCacheExpression("skill"))
                        {
                            if (objXmlSkill.SelectSingleNodeAndCacheExpression("hide") != null)
                                continue;
                            string strName = objXmlSkill.SelectSingleNodeAndCacheExpression("name").Value;
                            if (string.IsNullOrEmpty(strName))
                                continue;
                            XPathNavigator objNode
                                = _xmlSkillsBaseChummerNode.SelectSingleNode(
                                    "knowledgeskills/skill[name = " + strName.CleanXPath() + ']');
                            if (objNode.SelectSingleNodeAndCacheExpression("hide") != null)
                                continue;
                            string strText = (objNode != null
                                ? objNode.SelectSingleNodeAndCacheExpression("translate")
                                  ?.Value
                                  ?? strName
                                : strName) + strSpace + (objXmlSkill.SelectSingleNodeAndCacheExpression("rating")?.Value
                                                         ?? string.Empty);
                            string strSpec = objXmlSkill.SelectSingleNodeAndCacheExpression("spec")?.Value;
                            if (!string.IsNullOrEmpty(strSpec))
                                strText += strSpace + '(' + strSpec + ')';
                            TreeNode objChild = new TreeNode
                            {
                                Text = strText
                            };
                            objParent.Nodes.Add(objChild);
                        }
                        break;

                    case "selectmartialart":
                        {
                            objParent.Text = await LanguageManager.GetStringAsync("String_SelectPACKSKit_SelectMartialArt").ConfigureAwait(false);
                            int intRating = objXmlItem.SelectSingleNodeAndCacheExpression("@rating")?.ValueAsInt ?? 1;
                            string strSelect = objXmlItem.SelectSingleNodeAndCacheExpression("@select")?.Value
                                               ?? await LanguageManager.GetStringAsync(
                                                   "String_SelectPACKSKit_SelectMartialArt").ConfigureAwait(false);
                            TreeNode objMartialArt = new TreeNode
                            {
                                Text = strSelect + strSpace + await LanguageManager.GetStringAsync("String_Rating").ConfigureAwait(false) + strSpace + intRating.ToString(GlobalSettings.CultureInfo)
                            };
                            objParent.Nodes.Add(objMartialArt);
                            break;
                        }
                    case "martialarts":
                        objParent.Text = await LanguageManager.GetStringAsync("String_SelectPACKSKit_MartialArts").ConfigureAwait(false);
                        foreach (XPathNavigator objXmlArt in objXmlItem.SelectAndCacheExpression("martialart"))
                        {
                            if (objXmlArt.SelectSingleNodeAndCacheExpression("hide") != null)
                                continue;
                            string strName = objXmlArt.SelectSingleNodeAndCacheExpression("name").Value;
                            XPathNavigator objNode = _xmlMartialArtsBaseChummerNode.SelectSingleNode("martialarts/martialart[(" + strBookXPath + ") and name = " + strName.CleanXPath() + ']');
                            if (objNode == null)
                                continue;
                            string strText = (objNode != null
                                ? objNode.SelectSingleNodeAndCacheExpression("translate")
                                  ?.Value
                                  ?? strName
                                : strName) + strSpace + (objXmlArt.SelectSingleNodeAndCacheExpression("rating")?.Value
                                                         ?? string.Empty);
                            TreeNode objChild = new TreeNode
                            {
                                Text = strText
                            };
                            // Check for Techniques.
                            foreach (XPathNavigator xmlTechnique in objXmlArt.SelectAndCacheExpression("techniques/technique"))
                            {
                                if (xmlTechnique.SelectSingleNodeAndCacheExpression("hide") != null)
                                    continue;
                                string strTechniqueName = xmlTechnique.SelectSingleNodeAndCacheExpression("name").Value;
                                XPathNavigator objChildNode = _xmlMartialArtsBaseChummerNode.SelectSingleNode("techniques/technique[(" + strBookXPath + ") and name = " + strTechniqueName.CleanXPath() + ']');
                                if (objNode == null)
                                    continue;
                                TreeNode objChildChild = new TreeNode
                                {
                                    Text = objChildNode.SelectSingleNodeAndCacheExpression("@translate")?.Value ?? strTechniqueName
                                };
                                objChild.Nodes.Add(objChildChild);
                            }
                            objParent.Nodes.Add(objChild);
                        }
                        break;

                    case "powers":
                        {
                            objParent.Text = await LanguageManager.GetStringAsync("String_SelectPACKSKit_Powers").ConfigureAwait(false);
                            foreach (XPathNavigator objXmlPower in objXmlItem.SelectAndCacheExpression("power"))
                            {
                                if (objXmlPower.SelectSingleNodeAndCacheExpression("hide") != null)
                                    continue;
                                string strName = objXmlPower.SelectSingleNodeAndCacheExpression("name").Value;
                                XPathNavigator objNode = _xmlPowersBasePowersNode.SelectSingleNode("power[(" + strBookXPath + ") and name = " + strName.CleanXPath() + ']');
                                if (objNode == null)
                                    continue;
                                string strText = objNode.SelectSingleNodeAndCacheExpression("translate")?.Value
                                                 ?? strName;
                                string strSelect = objXmlPower.SelectSingleNodeAndCacheExpression("name/@select")?.Value ?? string.Empty;
                                if (!string.IsNullOrEmpty(strSelect))
                                    strText += strSpace + '(' + strSelect + ')';
                                string strRating = objXmlPower.SelectSingleNodeAndCacheExpression("rating")?.Value;
                                if (!string.IsNullOrEmpty(strRating))
                                    strText += strSpace + strRating;
                                TreeNode objChild = new TreeNode
                                {
                                    Text = strText
                                };
                                objParent.Nodes.Add(objChild);
                            }
                            break;
                        }
                    case "programs":
                        objParent.Text = await LanguageManager.GetStringAsync("String_SelectPACKSKit_Programs").ConfigureAwait(false);
                        foreach (XPathNavigator objXmlProgram in objXmlItem.SelectAndCacheExpression("program"))
                        {
                            if (objXmlProgram.SelectSingleNodeAndCacheExpression("hide") != null)
                                continue;
                            string strName = objXmlProgram.SelectSingleNodeAndCacheExpression("name").Value;
                            XPathNavigator objNode = _xmlComplexFormsBaseChummerNode.SelectSingleNode("complexforms/complexform[(" + strBookXPath + ") and name = " + strName.CleanXPath() + ']');
                            if (objNode == null)
                                continue;
                            string strText = (objNode.SelectSingleNodeAndCacheExpression("translate")?.Value ?? strName)
                                             + strSpace + (objXmlProgram.SelectSingleNodeAndCacheExpression("rating")
                                                                        ?.Value ?? string.Empty);
                            TreeNode objChild = new TreeNode
                            {
                                Text = strText
                            };
                            // Check for Program Options.
                            foreach (XPathNavigator objXmlOption in objXmlProgram.SelectAndCacheExpression("options/option"))
                            {
                                if (objXmlOption.SelectSingleNodeAndCacheExpression("hide") != null)
                                    continue;
                                string strOptionName = objXmlOption.SelectSingleNodeAndCacheExpression("name").Value;
                                XPathNavigator objChildNode = _xmlComplexFormsBaseChummerNode.SelectSingleNode("options/option[(" + strBookXPath + ") and name = " + strOptionName.CleanXPath() + ']');
                                if (objNode == null)
                                    continue;
                                string strInnerText = objChildNode.SelectSingleNodeAndCacheExpression("translate")?.Value
                                          ?? strOptionName;
                                string strRating = objXmlOption.SelectSingleNodeAndCacheExpression("rating")?.Value;
                                if (!string.IsNullOrEmpty(strRating))
                                    strInnerText += strSpace + strRating;
                                TreeNode objChildChild = new TreeNode
                                {
                                    Text = strInnerText
                                };
                                objChild.Nodes.Add(objChildChild);
                            }
                            objParent.Nodes.Add(objChild);
                        }
                        break;

                    case "spells":
                        objParent.Text = await LanguageManager.GetStringAsync("String_SelectPACKSKit_Spells").ConfigureAwait(false);
                        foreach (XPathNavigator objXmlSpell in objXmlItem.SelectAndCacheExpression("spell"))
                        {
                            if (objXmlSpell.SelectSingleNodeAndCacheExpression("hide") != null)
                                continue;
                            string strName = objXmlSpell.Value;
                            XPathNavigator objNode = _xmlSpellsBaseSpellsNode.SelectSingleNode("spell[(" + strBookXPath + ") and name = " + strName.CleanXPath() + ']');
                            if (objNode.SelectSingleNodeAndCacheExpression("hide") == null)
                                continue;
                            string strText = (objNode != null
                                ? objNode.SelectSingleNodeAndCacheExpression("translate")
                                  ?.Value
                                  ?? strName
                                : strName) + strSpace + (objXmlSpell.SelectSingleNodeAndCacheExpression("rating")?.Value
                                                         ?? string.Empty);
                            string strSelect = objXmlSpell.SelectSingleNodeAndCacheExpression("@select")?.Value;
                            if (!string.IsNullOrEmpty(strSelect))
                                strText += strSpace + '(' + strSelect + ')';
                            TreeNode objChild = new TreeNode
                            {
                                Text = strText
                            };
                            objParent.Nodes.Add(objChild);
                        }
                        break;

                    case "spirits":
                        objParent.Text = await LanguageManager.GetStringAsync("String_SelectPACKSKit_Spirits").ConfigureAwait(false);
                        foreach (XPathNavigator objXmlSpirit in objXmlItem.SelectAndCacheExpression("spirit"))
                        {
                            if (objXmlSpirit.SelectSingleNodeAndCacheExpression("hide") != null)
                                continue;
                            TreeNode objChild = new TreeNode
                            {
                                Text = objXmlSpirit.SelectSingleNodeAndCacheExpression("name").Value + strSpace + '(' +
                                       await LanguageManager.GetStringAsync("Label_Spirit_Force").ConfigureAwait(false) + strSpace + objXmlSpirit.SelectSingleNodeAndCacheExpression("force").Value + ',' + strSpace +
                                       await LanguageManager.GetStringAsync("Label_Spirit_ServicesOwed").ConfigureAwait(false) + strSpace + objXmlSpirit.SelectSingleNodeAndCacheExpression("services").Value + ')'
                            };
                            objParent.Nodes.Add(objChild);
                        }
                        break;

                    case "lifestyles":
                        objParent.Text = await LanguageManager.GetStringAsync("String_SelectPACKSKit_Lifestyles").ConfigureAwait(false);
                        foreach (XPathNavigator objXmlLifestyle in objXmlItem.SelectAndCacheExpression("lifestyle"))
                        {
                            if (objXmlLifestyle.SelectSingleNodeAndCacheExpression("hide") != null)
                                continue;
                            string strIncrement = objXmlLifestyle.SelectSingleNodeAndCacheExpression("increment")?.Value;
                            if (objXmlLifestyle.SelectSingleNodeAndCacheExpression("type")?.Value.Equals("SAFEHOUSE", StringComparison.OrdinalIgnoreCase) == true)
                                strIncrement = "week";
                            string strIncrementString;
                            int intPermanentAmount;
                            switch (strIncrement)
                            {
                                case "day":
                                case "Day":
                                    strIncrementString = await LanguageManager.GetStringAsync("String_Days").ConfigureAwait(false);
                                    intPermanentAmount = 3044;
                                    break;

                                case "week":
                                case "Week":
                                    strIncrementString = await LanguageManager.GetStringAsync("String_Weeks").ConfigureAwait(false);
                                    intPermanentAmount = 435;
                                    break;

                                default:
                                    strIncrementString = await LanguageManager.GetStringAsync("String_Months").ConfigureAwait(false);
                                    intPermanentAmount = 100;
                                    break;
                            }
                            TreeNode objChild = new TreeNode
                            {
                                Text = (objXmlLifestyle.SelectSingleNodeAndCacheExpression("translate") ?? objXmlLifestyle.SelectSingleNodeAndCacheExpression("baselifestyle")).Value
                                       + strSpace + objXmlLifestyle.SelectSingleNodeAndCacheExpression("months").Value
                                       + strSpace + strIncrementString + string.Format(GlobalSettings.CultureInfo, await LanguageManager.GetStringAsync("Label_LifestylePermanent").ConfigureAwait(false), intPermanentAmount.ToString(GlobalSettings.CultureInfo))
                            };
                            // Check for Qualities.
                            foreach (XPathNavigator objXmlQuality in objXmlLifestyle.SelectAndCacheExpression("qualities/quality"))
                            {
                                if (objXmlQuality.SelectSingleNodeAndCacheExpression("hide") != null)
                                    continue;
                                TreeNode objChildChild = new TreeNode
                                {
                                    Text = objXmlQuality.Value
                                };
                                objChild.Nodes.Add(objChildChild);
                            }
                            objParent.Nodes.Add(objChild);
                        }
                        break;

                    case "cyberwares":
                        objParent.Text = await LanguageManager.GetStringAsync("String_SelectPACKSKit_Cyberware").ConfigureAwait(false);
                        foreach (XPathNavigator objXmlCyberware in objXmlItem.SelectAndCacheExpression("cyberware"))
                        {
                            if (objXmlCyberware.SelectSingleNodeAndCacheExpression("hide") != null)
                                continue;
                            string strName = objXmlCyberware.SelectSingleNodeAndCacheExpression("name").Value;
                            XPathNavigator objNode = _xmlCyberwareBaseChummerNode.SelectSingleNode("cyberwares/cyberware[(" + strBookXPath + ") and name = " + strName.CleanXPath() + ']');
                            if (objNode == null)
                                continue;
                            string strText = objNode.SelectSingleNodeAndCacheExpression("translate")?.Value ?? strName;
                            string strRating = objXmlCyberware.SelectSingleNodeAndCacheExpression("rating")?.Value;
                            if (!string.IsNullOrEmpty(strRating))
                                strText += strSpace + await LanguageManager.GetStringAsync("String_Rating").ConfigureAwait(false) + strSpace + strRating;
                            strText += strSpace + '(' + objXmlCyberware.SelectSingleNodeAndCacheExpression("grade").Value + ')';
                            TreeNode objChild = new TreeNode
                            {
                                Text = strText
                            };
                            // Check for children.
                            foreach (XPathNavigator objXmlChild in objXmlCyberware.SelectAndCacheExpression("cyberwares/cyberware"))
                            {
                                if (objXmlChild.SelectSingleNodeAndCacheExpression("hide") != null)
                                    continue;
                                string strChildName = objXmlChild.SelectSingleNodeAndCacheExpression("name").Value;
                                XPathNavigator objChildNode = _xmlCyberwareBaseChummerNode.SelectSingleNode("cyberwares/cyberware[(" + strBookXPath + ") and name = " + strChildName.CleanXPath() + ']');
                                if (objChildNode == null)
                                    continue;
                                string strInnerText
                                    = objChildNode.SelectSingleNodeAndCacheExpression("translate")?.Value
                                      ?? strChildName;
                                strRating = objXmlChild.SelectSingleNodeAndCacheExpression("rating")?.Value;
                                if (!string.IsNullOrEmpty(strRating))
                                    strInnerText += strSpace + await LanguageManager.GetStringAsync("String_Rating").ConfigureAwait(false) + strSpace + strRating;
                                TreeNode objChildChild = new TreeNode
                                {
                                    Text = strInnerText
                                };
                                foreach (XPathNavigator objXmlGearNode in objXmlChild.SelectAndCacheExpression("gears/gear"))
                                    await WriteGear(objXmlGearNode, objChildChild).ConfigureAwait(false);
                                objChild.Nodes.Add(objChildChild);
                            }
                            foreach (XPathNavigator objXmlGearNode in objXmlCyberware.SelectAndCacheExpression("gears/gear"))
                                await WriteGear(objXmlGearNode, objChild).ConfigureAwait(false);
                            objParent.Nodes.Add(objChild);
                        }
                        break;

                    case "biowares":
                        objParent.Text = await LanguageManager.GetStringAsync("String_SelectPACKSKit_Bioware").ConfigureAwait(false);
                        foreach (XPathNavigator objXmlBioware in objXmlItem.SelectAndCacheExpression("bioware"))
                        {
                            if (objXmlBioware.SelectSingleNodeAndCacheExpression("hide") != null)
                                continue;
                            string strName = objXmlBioware.SelectSingleNodeAndCacheExpression("name").Value;
                            XPathNavigator objNode = _xmlBiowareBaseChummerNode.SelectSingleNode("biowares/bioware[(" + strBookXPath + ") and name = " + strName.CleanXPath() + ']');
                            if (objNode == null)
                                continue;
                            string strText = objNode.SelectSingleNodeAndCacheExpression("translate")?.Value ?? strName;
                            string strRating = objXmlBioware.SelectSingleNodeAndCacheExpression("rating")?.Value;
                            if (!string.IsNullOrEmpty(strRating))
                                strText += strSpace + await LanguageManager.GetStringAsync("String_Rating").ConfigureAwait(false) + strSpace + strRating;
                            strText += strSpace + '(' + objXmlBioware.SelectSingleNodeAndCacheExpression("grade").Value + ')';
                            TreeNode objChild = new TreeNode
                            {
                                Text = strText
                            };
                            // Check for children.
                            foreach (XPathNavigator objXmlChild in objXmlBioware.SelectAndCacheExpression("biowares/bioware"))
                            {
                                if (objXmlChild.SelectSingleNodeAndCacheExpression("hide") != null)
                                    continue;
                                string strChildName = objXmlChild.SelectSingleNodeAndCacheExpression("name").Value;
                                XPathNavigator objChildNode = _xmlBiowareBaseChummerNode.SelectSingleNode("biowares/bioware[(" + strBookXPath + ") and name = " + strChildName.CleanXPath() + ']');
                                if (objChildNode == null)
                                    continue;
                                string strInnerText
                                    = objChildNode.SelectSingleNodeAndCacheExpression("translate")?.Value
                                      ?? strChildName;
                                strRating = objXmlChild.SelectSingleNodeAndCacheExpression("rating")?.Value;
                                if (!string.IsNullOrEmpty(strRating))
                                {
                                    strInnerText += strSpace
                                                    + await LanguageManager.GetStringAsync("String_Rating")
                                                                           .ConfigureAwait(false)
                                                    + strSpace + strRating;
                                }

                                TreeNode objChildChild = new TreeNode
                                {
                                    Text = strInnerText
                                };
                                foreach (XPathNavigator objXmlGearNode in objXmlChild.SelectAndCacheExpression("gears/gear"))
                                    await WriteGear(objXmlGearNode, objChildChild).ConfigureAwait(false);
                                objChild.Nodes.Add(objChildChild);
                            }
                            foreach (XPathNavigator objXmlGearNode in objXmlBioware.SelectAndCacheExpression("gears/gear"))
                                await WriteGear(objXmlGearNode, objChild).ConfigureAwait(false);
                            objParent.Nodes.Add(objChild);
                        }
                        break;

                    case "armors":
                        objParent.Text = await LanguageManager.GetStringAsync("String_SelectPACKSKit_Armor").ConfigureAwait(false);
                        foreach (XPathNavigator objXmlArmor in objXmlItem.SelectAndCacheExpression("armor"))
                        {
                            if (objXmlArmor.SelectSingleNodeAndCacheExpression("hide") != null)
                                continue;
                            string strName = objXmlArmor.SelectSingleNodeAndCacheExpression("name").Value;
                            XPathNavigator objNode = _xmlArmorBaseChummerNode.SelectSingleNode("armors/armor[(" + strBookXPath + ") and name = " + strName.CleanXPath() + ']');
                            if (objNode == null)
                                continue;
                            TreeNode objChild = new TreeNode
                            {
                                Text = objNode.SelectSingleNodeAndCacheExpression("translate")?.Value ?? strName
                            };
                            // Check for children.
                            foreach (XPathNavigator objXmlChild in objXmlArmor.SelectAndCacheExpression("mods/mod"))
                            {
                                if (objXmlChild.SelectSingleNodeAndCacheExpression("hide") != null)
                                    continue;
                                string strChildName = objXmlChild.SelectSingleNodeAndCacheExpression("name").Value;
                                XPathNavigator objChildNode = _xmlArmorBaseChummerNode.SelectSingleNode("mods/mod[(" + strBookXPath + ") and name = " + strChildName.CleanXPath() + ']');
                                if (objChildNode == null)
                                    continue;
                                string strText = objChildNode.SelectSingleNodeAndCacheExpression("translate")?.Value
                                                 ?? strChildName;
                                string strRating = objXmlChild.SelectSingleNodeAndCacheExpression("rating")?.Value;
                                if (!string.IsNullOrEmpty(strRating))
                                    strText += strSpace + await LanguageManager.GetStringAsync("String_Rating").ConfigureAwait(false) + strSpace + strRating;
                                TreeNode objChildChild = new TreeNode
                                {
                                    Text = strText
                                };
                                foreach (XPathNavigator objXmlGearNode in objXmlChild.SelectAndCacheExpression("gears/gear"))
                                    await WriteGear(objXmlGearNode, objChildChild).ConfigureAwait(false);
                                objChild.Nodes.Add(objChildChild);
                            }
                            foreach (XPathNavigator objXmlGearNode in objXmlArmor.SelectAndCacheExpression("gears/gear"))
                                await WriteGear(objXmlGearNode, objChild).ConfigureAwait(false);
                            objParent.Nodes.Add(objChild);
                        }
                        break;

                    case "weapons":
                        objParent.Text = await LanguageManager.GetStringAsync("String_SelectPACKSKit_Weapons").ConfigureAwait(false);
                        foreach (XPathNavigator objXmlWeapon in objXmlItem.SelectAndCacheExpression("weapon"))
                        {
                            if (objXmlWeapon.SelectSingleNodeAndCacheExpression("hide") != null)
                                continue;
                            string strName = objXmlWeapon.SelectSingleNodeAndCacheExpression("name").Value;
                            XPathNavigator objNode = _xmlWeaponsBaseChummerNode.SelectSingleNode("weapons/weapon[(" + strBookXPath + ") and name = " + strName.CleanXPath() + ']');
                            if (objNode == null)
                                continue;
                            TreeNode objChild = new TreeNode
                            {
                                Text = objNode.SelectSingleNodeAndCacheExpression("translate")?.Value ?? strName
                            };
                            // Check for Weapon Accessories.
                            foreach (XPathNavigator objXmlAccessory in objXmlWeapon.SelectAndCacheExpression("accessories/accessory"))
                            {
                                if (objXmlAccessory.SelectSingleNodeAndCacheExpression("hide") != null)
                                    continue;
                                strName = objXmlAccessory.SelectSingleNodeAndCacheExpression("name").Value;
                                XPathNavigator objChildNode = _xmlWeaponsBaseChummerNode.SelectSingleNode("accessories/accessory[(" + strBookXPath + ") and name = " + strName.CleanXPath() + ']');
                                if (objChildNode == null)
                                    continue;
                                string strText = objChildNode.SelectSingleNodeAndCacheExpression("translate")?.Value
                                                 ?? strName;
                                string strRating = objXmlAccessory.SelectSingleNodeAndCacheExpression("rating")?.Value;
                                if (!string.IsNullOrEmpty(strRating))
                                    strText += strSpace + await LanguageManager.GetStringAsync("String_Rating").ConfigureAwait(false) + strSpace + strRating;
                                TreeNode objChildChild = new TreeNode
                                {
                                    Text = strText
                                };
                                foreach (XPathNavigator objXmlGearNode in objXmlAccessory.SelectAndCacheExpression("gears/gear"))
                                    await WriteGear(objXmlGearNode, objChildChild).ConfigureAwait(false);
                                objChild.Nodes.Add(objChildChild);
                            }
                            strName = objXmlWeapon.SelectSingleNodeAndCacheExpression("underbarrel")?.Value;
                            // Check for Underbarrel Weapons.
                            if (!string.IsNullOrEmpty(strName))
                            {
                                if (objXmlWeapon.SelectSingleNodeAndCacheExpression("hide") != null)
                                    continue;

                                XPathNavigator objChildNode = _xmlWeaponsBaseChummerNode.SelectSingleNode("weapons/weapon[(" + strBookXPath + ") and name = " + strName.CleanXPath() + ']');
                                if (objChildNode == null)
                                    continue;
                                TreeNode objChildChild = new TreeNode
                                {
                                    Text = objChildNode.SelectSingleNodeAndCacheExpression("translate")?.Value ?? strName
                                };
                                objChild.Nodes.Add(objChildChild);
                            }
                            objParent.Nodes.Add(objChild);
                        }
                        break;

                    case "gears":
                        objParent.Text = await LanguageManager.GetStringAsync("String_SelectPACKSKit_Gear").ConfigureAwait(false);
                        foreach (XPathNavigator objXmlGear in objXmlItem.SelectAndCacheExpression("gear"))
                        {
                            if (objXmlGear.SelectSingleNodeAndCacheExpression("hide") != null)
                                continue;
                            await WriteGear(objXmlGear, objParent).ConfigureAwait(false);
                        }
                        break;

                    case "vehicles":
                        objParent.Text = await LanguageManager.GetStringAsync("String_SelectPACKSKit_Vehicles").ConfigureAwait(false);
                        foreach (XPathNavigator objXmlVehicle in objXmlItem.SelectAndCacheExpression("vehicle"))
                        {
                            if (objXmlVehicle.SelectSingleNodeAndCacheExpression("hide") != null)
                                continue;
                            string strName = objXmlVehicle.SelectSingleNodeAndCacheExpression("name").Value;
                            XPathNavigator objNode = _xmlVehiclesBaseChummerNode.SelectSingleNode("vehicles/vehicle[(" + strBookXPath + ") and name = " + strName.CleanXPath() + ']');
                            if (objNode == null)
                                continue;
                            TreeNode objChild = new TreeNode
                            {
                                Text = objNode.SelectSingleNodeAndCacheExpression("translate")?.Value ?? strName
                            };
                            // Check for children.
                            foreach (XPathNavigator objXmlMod in objXmlVehicle.SelectAndCacheExpression("mods/mod"))
                            {
                                if (objXmlMod.SelectSingleNodeAndCacheExpression("hide") != null)
                                    continue;
                                strName = objXmlMod.SelectSingleNodeAndCacheExpression("name").Value;
                                XPathNavigator objChildNode = _xmlVehiclesBaseChummerNode.SelectSingleNode("mods/mod[(" + strBookXPath + ") and name = " + strName.CleanXPath() + ']');
                                if (objChildNode == null)
                                    continue;
                                string strText = objChildNode.SelectSingleNodeAndCacheExpression("translate")?.Value
                                                 ?? strName;
                                string strRating = objXmlMod.SelectSingleNodeAndCacheExpression("rating")?.Value;
                                if (!string.IsNullOrEmpty(strRating))
                                    strText += strSpace + await LanguageManager.GetStringAsync("String_Rating").ConfigureAwait(false) + strSpace + strRating;
                                TreeNode objChildChild = new TreeNode
                                {
                                    Text = strText
                                };
                                objChild.Nodes.Add(objChildChild);
                            }
                            // Check for children.
                            foreach (XPathNavigator objXmlChild in objXmlVehicle.SelectAndCacheExpression("gears/gear"))
                            {
                                if (objXmlChild.SelectSingleNodeAndCacheExpression("hide") != null)
                                    continue;
                                await WriteGear(objXmlChild, objChild).ConfigureAwait(false);
                            }
                            // Check for children.
                            foreach (XPathNavigator objXmlWeapon in objXmlVehicle.SelectAndCacheExpression("weapons/weapon"))
                            {
                                if (objXmlWeapon.SelectSingleNodeAndCacheExpression("hide") != null)
                                    continue;
                                strName = objXmlWeapon.SelectSingleNodeAndCacheExpression("name").Value;
                                XPathNavigator objChildNode = _xmlWeaponsBaseChummerNode.SelectSingleNode("weapons/weapon[(" + strBookXPath + ") and name = " + strName.CleanXPath() + ']');
                                if (objChildNode == null)
                                    continue;
                                TreeNode objChildChild = new TreeNode
                                {
                                    Text = objChildNode.SelectSingleNodeAndCacheExpression("translate")?.Value ?? strName
                                };
                                objChild.Nodes.Add(objChildChild);
                            }
                            objParent.Nodes.Add(objChild);
                        }
                        break;
                }
                objParent.ExpandAll();
                await treContents.DoThreadSafeAsync(x => x.Nodes.Add(objParent)).ConfigureAwait(false);
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
            Close();
        }

        private void cmdOKAdd_Click(object sender, EventArgs e)
        {
            _blnAddAgain = true;
            AcceptForm();
        }

        private async void cmdDelete_Click(object sender, EventArgs e)
        {
            string strSelectedKit = await lstKits.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString()).ConfigureAwait(false);
            if (string.IsNullOrEmpty(strSelectedKit))
                return;

            if (await Program.ShowScrollableMessageBoxAsync(this, string.Format(GlobalSettings.CultureInfo, await LanguageManager.GetStringAsync("Message_DeletePACKSKit").ConfigureAwait(false), strSelectedKit),
                    await LanguageManager.GetStringAsync("MessageTitle_Delete").ConfigureAwait(false), MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2).ConfigureAwait(false) == DialogResult.No)
                return;

            // Delete the selected custom PACKS Kit.
            // Find a custom PACKS Kit with the name. This is done without the XmlManager since we need to check each file individually.
            foreach (string strFile in Directory.EnumerateFiles(Utils.GetPacksFolderPath, "*_packs.xml"))
            {
                if (!Path.GetFileName(strFile).StartsWith("custom_", StringComparison.OrdinalIgnoreCase))
                    continue;
                using (new FetchSafelyFromSafeObjectPool<XmlDocument>(Utils.XmlDocumentPool, out XmlDocument objXmlDocument))
                {
                    try
                    {
                        await objXmlDocument.LoadStandardPatientAsync(strFile).ConfigureAwait(false);
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
                    if (xmlDocumentBasePacksNode?.TryGetNodeByNameOrId("pack", strSelectedKit, "category = \"Custom\"") != null)
                    {
                        using (FileStream objStream = new FileStream(strFile, FileMode.Create, FileAccess.Write, FileShare.None))
                        {
                            using (XmlWriter objWriter = Utils.GetStandardXmlWriter(objStream))
                            {
                                await objWriter.WriteStartDocumentAsync().ConfigureAwait(false);

                                // <chummer>
                                await objWriter.WriteStartElementAsync("chummer").ConfigureAwait(false);
                                // <packs>
                                await objWriter.WriteStartElementAsync("packs").ConfigureAwait(false);

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
                                                await objWriter.WriteStartElementAsync("pack").ConfigureAwait(false);
                                                objXmlNode.WriteContentTo(objWriter);
                                                // </pack>
                                                await objWriter.WriteEndElementAsync().ConfigureAwait(false);
                                            }
                                        }
                                    }
                                }

                                // </packs>
                                await objWriter.WriteEndElementAsync().ConfigureAwait(false);
                                // </chummer>
                                await objWriter.WriteEndElementAsync().ConfigureAwait(false);

                                await objWriter.WriteEndDocumentAsync().ConfigureAwait(false);
                            }
                        }
                    }
                }
            }

            // Reload the PACKS files since they have changed.
            _xmlBaseChummerNode = (await _objCharacter.LoadDataXPathAsync("packs.xml").ConfigureAwait(false)).SelectSingleNodeAndCacheExpression("/chummer");
            await RefreshCategories().ConfigureAwait(false);
        }

        #endregion Control Events

        #region Properties

        /// <summary>
        /// Whether the user wants to add another item after this one.
        /// </summary>
        public bool AddAgain => _blnAddAgain;

        /// <summary>
        /// Name of Kit that was selected in the dialogue.
        /// </summary>
        public string SelectedKit => _strSelectedKit;

        /// <summary>
        /// Category that was selected in the dialogue.
        /// </summary>
        public static string SelectedCategory => _strSelectCategory;

        #endregion Properties

        #region Methods

        /// <summary>
        /// Accept the selected item and close the form.
        /// </summary>
        private void AcceptForm()
        {
            string strSelectedKit = lstKits.SelectedValue?.ToString();
            if (string.IsNullOrEmpty(strSelectedKit))
                return;
            string[] objSelectedKit = strSelectedKit.SplitFixedSizePooledArray('<', 2, StringSplitOptions.RemoveEmptyEntries);
            try
            {
                _strSelectedKit = objSelectedKit[0];
                _strSelectCategory = objSelectedKit[1];
            }
            finally
            {
                ArrayPool<string>.Shared.Return(objSelectedKit);
            }
            DialogResult = DialogResult.OK;
            Close();
        }

        private async Task WriteGear(XPathNavigator objXmlGear, TreeNode objParent, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            XPathNavigator xmlNameNode = objXmlGear.SelectSingleNodeAndCacheExpression("name", token);
            string strName = xmlNameNode?.Value ?? string.Empty;
            string strCategory = objXmlGear.SelectSingleNodeAndCacheExpression("category", token)?.Value;
            string strFilter = await (await _objCharacter.GetSettingsAsync(token).ConfigureAwait(false)).BookXPathAsync(token: token).ConfigureAwait(false);
            XPathNavigator objNode = !string.IsNullOrEmpty(strCategory)
                ? _xmlGearsBaseGearsNode.SelectSingleNode("gear[(" + strFilter + ") and name = " + strName.CleanXPath() + " and category = " + strCategory.CleanXPath() + ']')
                : _xmlGearsBaseGearsNode.SelectSingleNode("gear[(" + strFilter + ") and name = " + strName.CleanXPath() + ']');

            if (objNode != null)
            {
                TreeNode objChild = new TreeNode
                {
                    Text = objNode.SelectSingleNodeAndCacheExpression("@translate", token)?.Value ?? strName
                };

                string strSpace = await LanguageManager.GetStringAsync("String_Space", token: token).ConfigureAwait(false);

                string strExtra = xmlNameNode != null
                    ? xmlNameNode.SelectSingleNodeAndCacheExpression("@select", token)?.Value
                    : string.Empty;
                if (!string.IsNullOrEmpty(strExtra))
                    objChild.Text += strSpace + '(' + strExtra + ')';
                strExtra = objXmlGear.SelectSingleNodeAndCacheExpression("rating", token)?.Value;
                if (!string.IsNullOrEmpty(strExtra))
                    objChild.Text += strSpace + await LanguageManager.GetStringAsync("String_Rating", token: token).ConfigureAwait(false) + strSpace + strExtra;
                strExtra = objXmlGear.SelectSingleNodeAndCacheExpression("qty", token)?.Value;
                if (!string.IsNullOrEmpty(strExtra))
                    objChild.Text += strSpace + '×' + strExtra;

                if (objParent.TreeView != null)
                    await objParent.TreeView.DoThreadSafeAsync(() => objParent.Nodes.Add(objChild), token).ConfigureAwait(false);
                else
                    objParent.Nodes.Add(objChild);

                // Check for children.
                foreach (XPathNavigator objXmlChild in objXmlGear.SelectAndCacheExpression("gears/gear", token))
                {
                    await WriteGear(objXmlChild, objChild, token).ConfigureAwait(false);
                }

                if (objChild.TreeView != null)
                    await objChild.TreeView.DoThreadSafeAsync(() => objChild.Expand(), token).ConfigureAwait(false);
                else
                    objChild.Expand();
            }
        }

        #endregion Methods
    }
}
