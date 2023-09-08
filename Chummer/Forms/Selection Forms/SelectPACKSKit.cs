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

        private List<ListItem> _lstCategory = Utils.ListItemListPool.Get();

        #region Control Events

        public SelectPACKSKit(Character objCharacter)
        {
            Disposed += (sender, args) => Utils.ListItemListPool.Return(ref _lstCategory);
            InitializeComponent();
            this.UpdateLightDarkMode();
            this.TranslateWinForm();
            _objCharacter = objCharacter;
            // Load the PACKS information.
            _xmlBaseChummerNode = _objCharacter.LoadDataXPath("packs.xml").SelectSingleNodeAndCacheExpression("/chummer");
            _xmlGearsBaseGearsNode = _objCharacter.LoadDataXPath("gear.xml").SelectSingleNodeAndCacheExpression("/chummer/gears");
            _xmlWeaponsBaseChummerNode = _objCharacter.LoadDataXPath("weapons.xml").SelectSingleNodeAndCacheExpression("/chummer");
            _xmlArmorBaseChummerNode = _objCharacter.LoadDataXPath("armor.xml").SelectSingleNodeAndCacheExpression("/chummer");
            _xmlQualitiesBaseQualitiesNode = _objCharacter.LoadDataXPath("qualities.xml").SelectSingleNodeAndCacheExpression("/chummer/qualities");
            _xmlSkillsBaseChummerNode = _objCharacter.LoadDataXPath("skills.xml").SelectSingleNodeAndCacheExpression("/chummer");
            _xmlSpellsBaseSpellsNode = _objCharacter.LoadDataXPath("spells.xml").SelectSingleNodeAndCacheExpression("/chummer/spells");
            _xmlComplexFormsBaseChummerNode = _objCharacter.LoadDataXPath("complexforms.xml").SelectSingleNodeAndCacheExpression("/chummer");
            _xmlVehiclesBaseChummerNode = _objCharacter.LoadDataXPath("vehicles.xml").SelectSingleNodeAndCacheExpression("/chummer");
            _xmlBiowareBaseChummerNode = _objCharacter.LoadDataXPath("bioware.xml").SelectSingleNodeAndCacheExpression("/chummer");
            _xmlCyberwareBaseChummerNode = _objCharacter.LoadDataXPath("cyberware.xml").SelectSingleNodeAndCacheExpression("/chummer");
            _xmlPowersBasePowersNode = _objCharacter.LoadDataXPath("powers.xml").SelectSingleNodeAndCacheExpression("/chummer/powers");
            _xmlMartialArtsBaseChummerNode = _objCharacter.LoadDataXPath("martialarts.xml").SelectSingleNodeAndCacheExpression("/chummer");
        }

        private async void SelectPACKSKit_Load(object sender, EventArgs e)
        {
            // Populate the PACKS Category list.
            foreach (XPathNavigator objXmlCategory in await _xmlBaseChummerNode.SelectAndCacheExpressionAsync("categories/category[not(hide)]").ConfigureAwait(false))
            {
                string strInnerText = objXmlCategory.Value;
                _lstCategory.Add(new ListItem(strInnerText, (await objXmlCategory.SelectSingleNodeAndCacheExpressionAsync("@translate").ConfigureAwait(false))?.Value ?? strInnerText));
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

        private async ValueTask RefreshCategories(CancellationToken token = default)
        {
            // Update the list of Kits based on the selected Category.

            string strFilter = "not(hide)";
            string strCategory = await cboCategory.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token: token).ConfigureAwait(false);
            if (!string.IsNullOrEmpty(strCategory) && strCategory != "Show All")
                strFilter += " and category = " + strCategory.CleanXPath();
            else
            {
                using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdCategoryFilter))
                {
                    foreach (string strItem in _lstCategory.Select(x => x.Value.ToString()))
                    {
                        if (!string.IsNullOrEmpty(strItem))
                            sbdCategoryFilter.Append("category = ").Append(strItem.CleanXPath()).Append(" or ");
                    }

                    if (sbdCategoryFilter.Length > 0)
                    {
                        strFilter += " and (" + sbdCategoryFilter.ToString().TrimEndOnce(" or ") + ')';
                    }
                }
            }

            // Retrieve the list of Kits for the selected Category.
            XPathNodeIterator xmlPacksKits = _xmlBaseChummerNode.Select("packs/pack[" + strFilter + ']');
            using (new FetchSafelyFromPool<List<ListItem>>(Utils.ListItemListPool, out List<ListItem> lstKit))
            {
                foreach (XPathNavigator objXmlPack in xmlPacksKits)
                {
                    string strName = (await objXmlPack.SelectSingleNodeAndCacheExpressionAsync("name", token: token).ConfigureAwait(false))?.Value;
                    // Separator "<" is a hack because XML does not like it when the '<' character is used in element contents, so we can safely assume that it will never show up.
                    lstKit.Add(new ListItem(
                                   strName + '<' + (await objXmlPack.SelectSingleNodeAndCacheExpressionAsync("category", token: token).ConfigureAwait(false))?.Value,
                                   (await objXmlPack.SelectSingleNodeAndCacheExpressionAsync("translate", token: token).ConfigureAwait(false))?.Value ?? strName));
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
            string[] strIdentifiers = strSelectedKit.Split('<', StringSplitOptions.RemoveEmptyEntries);
            await cmdDelete.DoThreadSafeAsync(x => x.Visible = strIdentifiers[1] == "Custom").ConfigureAwait(false);
            XPathNavigator objXmlPack = _xmlBaseChummerNode.SelectSingleNode("packs/pack[name = " + strIdentifiers[0].CleanXPath() + " and category = " + strIdentifiers[1].CleanXPath() + ']');
            if (objXmlPack == null)
            {
                return;
            }

            string strSpace = await LanguageManager.GetStringAsync("String_Space").ConfigureAwait(false);
            foreach (XPathNavigator objXmlItem in objXmlPack.SelectChildren(XPathNodeType.Element))
            {
                if (await objXmlItem.SelectSingleNodeAndCacheExpressionAsync("hide").ConfigureAwait(false) != null)
                    continue;
                TreeNode objParent = new TreeNode();
                switch (objXmlItem.Name)
                {
                    case "attributes":
                        objParent.Text = await LanguageManager.GetStringAsync("String_SelectPACKSKit_Attributes").ConfigureAwait(false);
                        foreach (XPathNavigator objXmlAttribute in objXmlItem.SelectChildren(XPathNodeType.Element))
                        {
                            if (await objXmlAttribute.SelectSingleNodeAndCacheExpressionAsync("hide").ConfigureAwait(false) != null)
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
                        foreach (XPathNavigator objXmlQuality in await objXmlItem.SelectAndCacheExpressionAsync("positive/quality").ConfigureAwait(false))
                        {
                            if (await objXmlQuality.SelectSingleNodeAndCacheExpressionAsync("hide").ConfigureAwait(false) != null)
                                continue;
                            XPathNavigator objNode = _xmlQualitiesBaseQualitiesNode.SelectSingleNode("quality[(" + await _objCharacter.Settings.BookXPathAsync().ConfigureAwait(false) + ") and name = " + objXmlQuality.Value.CleanXPath() + ']');
                            if (objNode == null)
                                continue;
                            string strText = (await objNode.SelectSingleNodeAndCacheExpressionAsync("translate").ConfigureAwait(false))?.Value
                                             ?? objXmlQuality.Value;
                            string strSelect = (await objXmlQuality.SelectSingleNodeAndCacheExpressionAsync("@select").ConfigureAwait(false))?.Value;
                            if (!string.IsNullOrEmpty(strSelect))
                                strText += strSpace + '(' + await _objCharacter.TranslateExtraAsync(strSelect).ConfigureAwait(false) + ')';
                            TreeNode objChild = new TreeNode
                            {
                                Text = strText
                            };
                            objParent.Nodes.Add(objChild);
                        }

                        // Negative Qualities.
                        foreach (XPathNavigator objXmlQuality in await objXmlItem.SelectAndCacheExpressionAsync("negative/quality").ConfigureAwait(false))
                        {
                            if (await objXmlQuality.SelectSingleNodeAndCacheExpressionAsync("hide").ConfigureAwait(false) != null)
                                continue;
                            XPathNavigator objNode = _xmlQualitiesBaseQualitiesNode.SelectSingleNode("quality[(" + await _objCharacter.Settings.BookXPathAsync().ConfigureAwait(false) + ") and name = " + objXmlQuality.Value.CleanXPath() + ']');
                            if (objNode == null)
                                continue;
                            string strText = (await objNode.SelectSingleNodeAndCacheExpressionAsync("translate").ConfigureAwait(false))?.Value
                                             ?? objXmlQuality.Value;
                            string strSelect = (await objXmlQuality.SelectSingleNodeAndCacheExpressionAsync("@select").ConfigureAwait(false))?.Value;
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
                        foreach (XPathNavigator objXmlSkill in await objXmlItem.SelectAndCacheExpressionAsync("skill").ConfigureAwait(false))
                        {
                            if (await objXmlSkill.SelectSingleNodeAndCacheExpressionAsync("hide").ConfigureAwait(false) != null)
                                continue;
                            string strName = (await objXmlSkill.SelectSingleNodeAndCacheExpressionAsync("name").ConfigureAwait(false)).Value;
                            XPathNavigator objNode = _xmlSkillsBaseChummerNode.SelectSingleNode("skills/skill[name = " + strName.CleanXPath() + ']');
                            if (await objNode.SelectSingleNodeAndCacheExpressionAsync("hide").ConfigureAwait(false) != null)
                                continue;
                            string strText = ((await objNode.SelectSingleNodeAndCacheExpressionAsync("translate").ConfigureAwait(false))?.Value ?? strName)
                                             + strSpace + ((await objXmlSkill.SelectSingleNodeAndCacheExpressionAsync("rating").ConfigureAwait(false))
                                                                      ?.Value ?? string.Empty);
                            string strSpec = (await objXmlSkill.SelectSingleNodeAndCacheExpressionAsync("spec").ConfigureAwait(false))?.Value;
                            if (!string.IsNullOrEmpty(strSpec))
                                strText += strSpace + '(' + strSpec + ')';
                            TreeNode objChild = new TreeNode
                            {
                                Text = strText
                            };
                            objParent.Nodes.Add(objChild);
                        }
                        foreach (XPathNavigator objXmlSkill in await objXmlItem.SelectAndCacheExpressionAsync("skillgroup").ConfigureAwait(false))
                        {
                            if (await objXmlSkill.SelectSingleNodeAndCacheExpressionAsync("hide").ConfigureAwait(false) != null)
                                continue;
                            string strName = (await objXmlSkill.SelectSingleNodeAndCacheExpressionAsync("name").ConfigureAwait(false)).Value;
                            XPathNavigator objNode = _xmlSkillsBaseChummerNode.SelectSingleNode("skillgroups/name[. = " + strName.CleanXPath() + ']');
                            if (await objNode.SelectSingleNodeAndCacheExpressionAsync("hide").ConfigureAwait(false) != null)
                                continue;
                            string strText = ((await objNode.SelectSingleNodeAndCacheExpressionAsync("@translate").ConfigureAwait(false))?.Value
                                              ?? strName) + strSpace
                                                          + await LanguageManager.GetStringAsync(
                                                              "String_SelectPACKSKit_Group").ConfigureAwait(false) + strSpace
                                                          + ((await objXmlSkill.SelectSingleNodeAndCacheExpressionAsync("rating").ConfigureAwait(false))?.Value ?? string.Empty);
                            string strSpec = (await objXmlSkill.SelectSingleNodeAndCacheExpressionAsync("spec").ConfigureAwait(false))?.Value;
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
                        foreach (XPathNavigator objXmlSkill in await objXmlItem.SelectAndCacheExpressionAsync("skill").ConfigureAwait(false))
                        {
                            if (await objXmlSkill.SelectSingleNodeAndCacheExpressionAsync("hide").ConfigureAwait(false) != null)
                                continue;
                            string strName = (await objXmlSkill.SelectSingleNodeAndCacheExpressionAsync("name").ConfigureAwait(false)).Value;
                            if (string.IsNullOrEmpty(strName))
                                continue;
                            XPathNavigator objNode
                                = _xmlSkillsBaseChummerNode.SelectSingleNode(
                                    "knowledgeskills/skill[name = " + strName.CleanXPath() + ']');
                            if (await objNode.SelectSingleNodeAndCacheExpressionAsync("hide").ConfigureAwait(false) != null)
                                continue;
                            string strText = (objNode != null
                                ? ((await objNode.SelectSingleNodeAndCacheExpressionAsync("translate").ConfigureAwait(false))
                                   ?.Value
                                   ?? strName)
                                : strName) + strSpace + ((await objXmlSkill.SelectSingleNodeAndCacheExpressionAsync("rating").ConfigureAwait(false))?.Value
                                                         ?? string.Empty);
                            string strSpec = (await objXmlSkill.SelectSingleNodeAndCacheExpressionAsync("spec").ConfigureAwait(false))?.Value;
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
                            int intRating = (await objXmlItem.SelectSingleNodeAndCacheExpressionAsync("@rating").ConfigureAwait(false))?.ValueAsInt ?? 1;
                            string strSelect = (await objXmlItem.SelectSingleNodeAndCacheExpressionAsync("@select").ConfigureAwait(false))?.Value
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
                        foreach (XPathNavigator objXmlArt in await objXmlItem.SelectAndCacheExpressionAsync("martialart").ConfigureAwait(false))
                        {
                            if (await objXmlArt.SelectSingleNodeAndCacheExpressionAsync("hide").ConfigureAwait(false) != null)
                                continue;
                            string strName = (await objXmlArt.SelectSingleNodeAndCacheExpressionAsync("name").ConfigureAwait(false)).Value;
                            XPathNavigator objNode = _xmlMartialArtsBaseChummerNode.SelectSingleNode("martialarts/martialart[(" + await _objCharacter.Settings.BookXPathAsync().ConfigureAwait(false) + ") and name = " + strName.CleanXPath() + ']');
                            if (objNode == null)
                                continue;
                            string strText = (objNode != null
                                ? ((await objNode.SelectSingleNodeAndCacheExpressionAsync("translate").ConfigureAwait(false))
                                   ?.Value
                                   ?? strName)
                                : strName) + strSpace + ((await objXmlArt.SelectSingleNodeAndCacheExpressionAsync("rating").ConfigureAwait(false))?.Value
                                                         ?? string.Empty);
                            TreeNode objChild = new TreeNode
                            {
                                Text = strText
                            };
                            // Check for Techniques.
                            foreach (XPathNavigator xmlTechnique in await objXmlArt.SelectAndCacheExpressionAsync("techniques/technique").ConfigureAwait(false))
                            {
                                if (await xmlTechnique.SelectSingleNodeAndCacheExpressionAsync("hide").ConfigureAwait(false) != null)
                                    continue;
                                string strTechniqueName = (await xmlTechnique.SelectSingleNodeAndCacheExpressionAsync("name").ConfigureAwait(false)).Value;
                                XPathNavigator objChildNode = _xmlMartialArtsBaseChummerNode.SelectSingleNode("techniques/technique[(" + await _objCharacter.Settings.BookXPathAsync().ConfigureAwait(false) + ") and name = " + strTechniqueName.CleanXPath() + ']');
                                if (objNode == null)
                                    continue;
                                TreeNode objChildChild = new TreeNode
                                {
                                    Text = (await objChildNode.SelectSingleNodeAndCacheExpressionAsync("@translate").ConfigureAwait(false))?.Value ?? strTechniqueName
                                };
                                objChild.Nodes.Add(objChildChild);
                            }
                            objParent.Nodes.Add(objChild);
                        }
                        break;

                    case "powers":
                        {
                            objParent.Text = await LanguageManager.GetStringAsync("String_SelectPACKSKit_Powers").ConfigureAwait(false);
                            foreach (XPathNavigator objXmlPower in await objXmlItem.SelectAndCacheExpressionAsync("power").ConfigureAwait(false))
                            {
                                if (await objXmlPower.SelectSingleNodeAndCacheExpressionAsync("hide").ConfigureAwait(false) != null)
                                    continue;
                                string strName = (await objXmlPower.SelectSingleNodeAndCacheExpressionAsync("name").ConfigureAwait(false)).Value;
                                XPathNavigator objNode = _xmlPowersBasePowersNode.SelectSingleNode("power[(" + await _objCharacter.Settings.BookXPathAsync().ConfigureAwait(false) + ") and name = " + strName.CleanXPath() + ']');
                                if (objNode == null)
                                    continue;
                                string strText = (await objNode.SelectSingleNodeAndCacheExpressionAsync("translate").ConfigureAwait(false))?.Value
                                                 ?? strName;
                                string strSelect = (await objXmlPower.SelectSingleNodeAndCacheExpressionAsync("name/@select").ConfigureAwait(false))?.Value ?? string.Empty;
                                if (!string.IsNullOrEmpty(strSelect))
                                    strText += strSpace + '(' + strSelect + ')';
                                string strRating = (await objXmlPower.SelectSingleNodeAndCacheExpressionAsync("rating").ConfigureAwait(false))?.Value;
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
                        foreach (XPathNavigator objXmlProgram in await objXmlItem.SelectAndCacheExpressionAsync("program").ConfigureAwait(false))
                        {
                            if (await objXmlProgram.SelectSingleNodeAndCacheExpressionAsync("hide").ConfigureAwait(false) != null)
                                continue;
                            string strName = (await objXmlProgram.SelectSingleNodeAndCacheExpressionAsync("name").ConfigureAwait(false)).Value;
                            XPathNavigator objNode = _xmlComplexFormsBaseChummerNode.SelectSingleNode("complexforms/complexform[(" + await _objCharacter.Settings.BookXPathAsync().ConfigureAwait(false) + ") and name = " + strName.CleanXPath() + ']');
                            if (objNode == null)
                                continue;
                            string strText = ((await objNode.SelectSingleNodeAndCacheExpressionAsync("translate").ConfigureAwait(false))?.Value ?? strName)
                                             + strSpace + ((await objXmlProgram.SelectSingleNodeAndCacheExpressionAsync("rating").ConfigureAwait(false))
                                                                        ?.Value ?? string.Empty);
                            TreeNode objChild = new TreeNode
                            {
                                Text = strText
                            };
                            // Check for Program Options.
                            foreach (XPathNavigator objXmlOption in await objXmlProgram.SelectAndCacheExpressionAsync("options/option").ConfigureAwait(false))
                            {
                                if (await objXmlOption.SelectSingleNodeAndCacheExpressionAsync("hide").ConfigureAwait(false) != null)
                                    continue;
                                string strOptionName = (await objXmlOption.SelectSingleNodeAndCacheExpressionAsync("name").ConfigureAwait(false)).Value;
                                XPathNavigator objChildNode = _xmlComplexFormsBaseChummerNode.SelectSingleNode("options/option[(" + await _objCharacter.Settings.BookXPathAsync().ConfigureAwait(false) + ") and name = " + strOptionName.CleanXPath() + ']');
                                if (objNode == null)
                                    continue;
                                string strInnerText = (await objChildNode.SelectSingleNodeAndCacheExpressionAsync("translate").ConfigureAwait(false))?.Value
                                          ?? strOptionName;
                                string strRating = (await objXmlOption.SelectSingleNodeAndCacheExpressionAsync("rating").ConfigureAwait(false))?.Value;
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
                        foreach (XPathNavigator objXmlSpell in await objXmlItem.SelectAndCacheExpressionAsync("spell").ConfigureAwait(false))
                        {
                            if (await objXmlSpell.SelectSingleNodeAndCacheExpressionAsync("hide").ConfigureAwait(false) != null)
                                continue;
                            string strName = objXmlSpell.Value;
                            XPathNavigator objNode = _xmlSpellsBaseSpellsNode.SelectSingleNode("spell[(" + await _objCharacter.Settings.BookXPathAsync().ConfigureAwait(false) + ") and name = " + strName.CleanXPath() + ']');
                            if (await objNode.SelectSingleNodeAndCacheExpressionAsync("hide").ConfigureAwait(false) == null)
                                continue;
                            string strText = (objNode != null
                                ? ((await objNode.SelectSingleNodeAndCacheExpressionAsync("translate").ConfigureAwait(false))
                                   ?.Value
                                   ?? strName)
                                : strName) + strSpace + ((await objXmlSpell.SelectSingleNodeAndCacheExpressionAsync("rating").ConfigureAwait(false))?.Value
                                                         ?? string.Empty);
                            string strSelect = (await objXmlSpell.SelectSingleNodeAndCacheExpressionAsync("@select").ConfigureAwait(false))?.Value;
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
                        foreach (XPathNavigator objXmlSpirit in await objXmlItem.SelectAndCacheExpressionAsync("spirit").ConfigureAwait(false))
                        {
                            if (await objXmlSpirit.SelectSingleNodeAndCacheExpressionAsync("hide").ConfigureAwait(false) != null)
                                continue;
                            TreeNode objChild = new TreeNode
                            {
                                Text = (await objXmlSpirit.SelectSingleNodeAndCacheExpressionAsync("name").ConfigureAwait(false)).Value + strSpace + '(' +
                                       await LanguageManager.GetStringAsync("Label_Spirit_Force").ConfigureAwait(false) + strSpace + (await objXmlSpirit.SelectSingleNodeAndCacheExpressionAsync("force").ConfigureAwait(false)).Value + ',' + strSpace +
                                       await LanguageManager.GetStringAsync("Label_Spirit_ServicesOwed").ConfigureAwait(false) + strSpace + (await objXmlSpirit.SelectSingleNodeAndCacheExpressionAsync("services").ConfigureAwait(false)).Value + ')'
                            };
                            objParent.Nodes.Add(objChild);
                        }
                        break;

                    case "lifestyles":
                        objParent.Text = await LanguageManager.GetStringAsync("String_SelectPACKSKit_Lifestyles").ConfigureAwait(false);
                        foreach (XPathNavigator objXmlLifestyle in await objXmlItem.SelectAndCacheExpressionAsync("lifestyle").ConfigureAwait(false))
                        {
                            if (await objXmlLifestyle.SelectSingleNodeAndCacheExpressionAsync("hide").ConfigureAwait(false) != null)
                                continue;
                            string strIncrement = (await objXmlLifestyle.SelectSingleNodeAndCacheExpressionAsync("increment").ConfigureAwait(false))?.Value;
                            if ((await objXmlLifestyle.SelectSingleNodeAndCacheExpressionAsync("type").ConfigureAwait(false))?.Value.Equals("SAFEHOUSE", StringComparison.OrdinalIgnoreCase) == true)
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
                                Text = (await objXmlLifestyle.SelectSingleNodeAndCacheExpressionAsync("translate").ConfigureAwait(false) ?? await objXmlLifestyle.SelectSingleNodeAndCacheExpressionAsync("baselifestyle").ConfigureAwait(false)).Value
                                       + strSpace + (await objXmlLifestyle.SelectSingleNodeAndCacheExpressionAsync("months").ConfigureAwait(false)).Value
                                       + strSpace + strIncrementString + string.Format(GlobalSettings.CultureInfo, await LanguageManager.GetStringAsync("Label_LifestylePermanent").ConfigureAwait(false), intPermanentAmount.ToString(GlobalSettings.CultureInfo))
                            };
                            // Check for Qualities.
                            foreach (XPathNavigator objXmlQuality in await objXmlLifestyle.SelectAndCacheExpressionAsync("qualities/quality").ConfigureAwait(false))
                            {
                                if (await objXmlQuality.SelectSingleNodeAndCacheExpressionAsync("hide").ConfigureAwait(false) != null)
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
                        foreach (XPathNavigator objXmlCyberware in await objXmlItem.SelectAndCacheExpressionAsync("cyberware").ConfigureAwait(false))
                        {
                            if (await objXmlCyberware.SelectSingleNodeAndCacheExpressionAsync("hide").ConfigureAwait(false) != null)
                                continue;
                            string strName = (await objXmlCyberware.SelectSingleNodeAndCacheExpressionAsync("name").ConfigureAwait(false)).Value;
                            XPathNavigator objNode = _xmlCyberwareBaseChummerNode.SelectSingleNode("cyberwares/cyberware[(" + await _objCharacter.Settings.BookXPathAsync().ConfigureAwait(false) + ") and name = " + strName.CleanXPath() + ']');
                            if (objNode == null)
                                continue;
                            string strText = (await objNode.SelectSingleNodeAndCacheExpressionAsync("translate").ConfigureAwait(false))?.Value ?? strName;
                            string strRating = (await objXmlCyberware.SelectSingleNodeAndCacheExpressionAsync("rating").ConfigureAwait(false))?.Value;
                            if (!string.IsNullOrEmpty(strRating))
                                strText += strSpace + await LanguageManager.GetStringAsync("String_Rating").ConfigureAwait(false) + strSpace + strRating;
                            strText += strSpace + '(' + (await objXmlCyberware.SelectSingleNodeAndCacheExpressionAsync("grade").ConfigureAwait(false)).Value + ')';
                            TreeNode objChild = new TreeNode
                            {
                                Text = strText
                            };
                            // Check for children.
                            foreach (XPathNavigator objXmlChild in await objXmlCyberware.SelectAndCacheExpressionAsync("cyberwares/cyberware").ConfigureAwait(false))
                            {
                                if (await objXmlChild.SelectSingleNodeAndCacheExpressionAsync("hide").ConfigureAwait(false) != null)
                                    continue;
                                string strChildName = (await objXmlChild.SelectSingleNodeAndCacheExpressionAsync("name").ConfigureAwait(false)).Value;
                                XPathNavigator objChildNode = _xmlCyberwareBaseChummerNode.SelectSingleNode("cyberwares/cyberware[(" + await _objCharacter.Settings.BookXPathAsync().ConfigureAwait(false) + ") and name = " + strChildName.CleanXPath() + ']');
                                if (objChildNode == null)
                                    continue;
                                string strInnerText
                                    = (await objChildNode.SelectSingleNodeAndCacheExpressionAsync("translate").ConfigureAwait(false))?.Value
                                      ?? strChildName;
                                strRating = (await objXmlChild.SelectSingleNodeAndCacheExpressionAsync("rating").ConfigureAwait(false))?.Value;
                                if (!string.IsNullOrEmpty(strRating))
                                    strInnerText += strSpace + await LanguageManager.GetStringAsync("String_Rating").ConfigureAwait(false) + strSpace + strRating;
                                TreeNode objChildChild = new TreeNode
                                {
                                    Text = strInnerText
                                };
                                foreach (XPathNavigator objXmlGearNode in await objXmlChild.SelectAndCacheExpressionAsync("gears/gear").ConfigureAwait(false))
                                    await WriteGear(objXmlGearNode, objChildChild).ConfigureAwait(false);
                                objChild.Nodes.Add(objChildChild);
                            }
                            foreach (XPathNavigator objXmlGearNode in await objXmlCyberware.SelectAndCacheExpressionAsync("gears/gear").ConfigureAwait(false))
                                await WriteGear(objXmlGearNode, objChild).ConfigureAwait(false);
                            objParent.Nodes.Add(objChild);
                        }
                        break;

                    case "biowares":
                        objParent.Text = await LanguageManager.GetStringAsync("String_SelectPACKSKit_Bioware").ConfigureAwait(false);
                        foreach (XPathNavigator objXmlBioware in await objXmlItem.SelectAndCacheExpressionAsync("bioware").ConfigureAwait(false))
                        {
                            if (await objXmlBioware.SelectSingleNodeAndCacheExpressionAsync("hide").ConfigureAwait(false) != null)
                                continue;
                            string strName = (await objXmlBioware.SelectSingleNodeAndCacheExpressionAsync("name").ConfigureAwait(false)).Value;
                            XPathNavigator objNode = _xmlBiowareBaseChummerNode.SelectSingleNode("biowares/bioware[(" + await _objCharacter.Settings.BookXPathAsync().ConfigureAwait(false) + ") and name = " + strName.CleanXPath() + ']');
                            if (objNode == null)
                                continue;
                            string strText = (await objNode.SelectSingleNodeAndCacheExpressionAsync("translate").ConfigureAwait(false))?.Value ?? strName;
                            string strRating = (await objXmlBioware.SelectSingleNodeAndCacheExpressionAsync("rating").ConfigureAwait(false))?.Value;
                            if (!string.IsNullOrEmpty(strRating))
                                strText += strSpace + await LanguageManager.GetStringAsync("String_Rating").ConfigureAwait(false) + strSpace + strRating;
                            strText += strSpace + '(' + (await objXmlBioware.SelectSingleNodeAndCacheExpressionAsync("grade").ConfigureAwait(false)).Value + ')';
                            TreeNode objChild = new TreeNode
                            {
                                Text = strText
                            };
                            // Check for children.
                            foreach (XPathNavigator objXmlChild in await objXmlBioware.SelectAndCacheExpressionAsync("biowares/bioware").ConfigureAwait(false))
                            {
                                if (await objXmlChild.SelectSingleNodeAndCacheExpressionAsync("hide").ConfigureAwait(false) != null)
                                    continue;
                                string strChildName = (await objXmlChild.SelectSingleNodeAndCacheExpressionAsync("name").ConfigureAwait(false)).Value;
                                XPathNavigator objChildNode = _xmlBiowareBaseChummerNode.SelectSingleNode("biowares/bioware[(" + await _objCharacter.Settings.BookXPathAsync().ConfigureAwait(false) + ") and name = " + strChildName.CleanXPath() + ']');
                                if (objChildNode == null)
                                    continue;
                                string strInnerText
                                    = (await objChildNode.SelectSingleNodeAndCacheExpressionAsync("translate").ConfigureAwait(false))?.Value
                                      ?? strChildName;
                                strRating = (await objXmlChild.SelectSingleNodeAndCacheExpressionAsync("rating").ConfigureAwait(false))?.Value;
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
                                foreach (XPathNavigator objXmlGearNode in await objXmlChild.SelectAndCacheExpressionAsync("gears/gear").ConfigureAwait(false))
                                    await WriteGear(objXmlGearNode, objChildChild).ConfigureAwait(false);
                                objChild.Nodes.Add(objChildChild);
                            }
                            foreach (XPathNavigator objXmlGearNode in await objXmlBioware.SelectAndCacheExpressionAsync("gears/gear").ConfigureAwait(false))
                                await WriteGear(objXmlGearNode, objChild).ConfigureAwait(false);
                            objParent.Nodes.Add(objChild);
                        }
                        break;

                    case "armors":
                        objParent.Text = await LanguageManager.GetStringAsync("String_SelectPACKSKit_Armor").ConfigureAwait(false);
                        foreach (XPathNavigator objXmlArmor in await objXmlItem.SelectAndCacheExpressionAsync("armor").ConfigureAwait(false))
                        {
                            if (await objXmlArmor.SelectSingleNodeAndCacheExpressionAsync("hide").ConfigureAwait(false) != null)
                                continue;
                            string strName = (await objXmlArmor.SelectSingleNodeAndCacheExpressionAsync("name").ConfigureAwait(false)).Value;
                            XPathNavigator objNode = _xmlArmorBaseChummerNode.SelectSingleNode("armors/armor[(" + await _objCharacter.Settings.BookXPathAsync().ConfigureAwait(false) + ") and name = " + strName.CleanXPath() + ']');
                            if (objNode == null)
                                continue;
                            TreeNode objChild = new TreeNode
                            {
                                Text = (await objNode.SelectSingleNodeAndCacheExpressionAsync("translate").ConfigureAwait(false))?.Value ?? strName
                            };
                            // Check for children.
                            foreach (XPathNavigator objXmlChild in await objXmlArmor.SelectAndCacheExpressionAsync("mods/mod").ConfigureAwait(false))
                            {
                                if (await objXmlChild.SelectSingleNodeAndCacheExpressionAsync("hide").ConfigureAwait(false) != null)
                                    continue;
                                string strChildName = (await objXmlChild.SelectSingleNodeAndCacheExpressionAsync("name").ConfigureAwait(false)).Value;
                                XPathNavigator objChildNode = _xmlArmorBaseChummerNode.SelectSingleNode("mods/mod[(" + await _objCharacter.Settings.BookXPathAsync().ConfigureAwait(false) + ") and name = " + strChildName.CleanXPath() + ']');
                                if (objChildNode == null)
                                    continue;
                                string strText = (await objChildNode.SelectSingleNodeAndCacheExpressionAsync("translate").ConfigureAwait(false))?.Value
                                                 ?? strChildName;
                                string strRating = (await objXmlChild.SelectSingleNodeAndCacheExpressionAsync("rating").ConfigureAwait(false))?.Value;
                                if (!string.IsNullOrEmpty(strRating))
                                    strText += strSpace + await LanguageManager.GetStringAsync("String_Rating").ConfigureAwait(false) + strSpace + strRating;
                                TreeNode objChildChild = new TreeNode
                                {
                                    Text = strText
                                };
                                foreach (XPathNavigator objXmlGearNode in await objXmlChild.SelectAndCacheExpressionAsync("gears/gear").ConfigureAwait(false))
                                    await WriteGear(objXmlGearNode, objChildChild).ConfigureAwait(false);
                                objChild.Nodes.Add(objChildChild);
                            }
                            foreach (XPathNavigator objXmlGearNode in await objXmlArmor.SelectAndCacheExpressionAsync("gears/gear").ConfigureAwait(false))
                                await WriteGear(objXmlGearNode, objChild).ConfigureAwait(false);
                            objParent.Nodes.Add(objChild);
                        }
                        break;

                    case "weapons":
                        objParent.Text = await LanguageManager.GetStringAsync("String_SelectPACKSKit_Weapons").ConfigureAwait(false);
                        foreach (XPathNavigator objXmlWeapon in await objXmlItem.SelectAndCacheExpressionAsync("weapon").ConfigureAwait(false))
                        {
                            if (await objXmlWeapon.SelectSingleNodeAndCacheExpressionAsync("hide").ConfigureAwait(false) != null)
                                continue;
                            string strName = (await objXmlWeapon.SelectSingleNodeAndCacheExpressionAsync("name").ConfigureAwait(false)).Value;
                            XPathNavigator objNode = _xmlWeaponsBaseChummerNode.SelectSingleNode("weapons/weapon[(" + await _objCharacter.Settings.BookXPathAsync().ConfigureAwait(false) + ") and name = " + strName.CleanXPath() + ']');
                            if (objNode == null)
                                continue;
                            TreeNode objChild = new TreeNode
                            {
                                Text = (await objNode.SelectSingleNodeAndCacheExpressionAsync("translate").ConfigureAwait(false))?.Value ?? strName
                            };
                            // Check for Weapon Accessories.
                            foreach (XPathNavigator objXmlAccessory in await objXmlWeapon.SelectAndCacheExpressionAsync("accessories/accessory").ConfigureAwait(false))
                            {
                                if (await objXmlAccessory.SelectSingleNodeAndCacheExpressionAsync("hide").ConfigureAwait(false) != null)
                                    continue;
                                strName = (await objXmlAccessory.SelectSingleNodeAndCacheExpressionAsync("name").ConfigureAwait(false)).Value;
                                XPathNavigator objChildNode = _xmlWeaponsBaseChummerNode.SelectSingleNode("accessories/accessory[(" + await _objCharacter.Settings.BookXPathAsync().ConfigureAwait(false) + ") and name = " + strName.CleanXPath() + ']');
                                if (objChildNode == null)
                                    continue;
                                string strText = (await objChildNode.SelectSingleNodeAndCacheExpressionAsync("translate").ConfigureAwait(false))?.Value
                                                 ?? strName;
                                string strRating = (await objXmlAccessory.SelectSingleNodeAndCacheExpressionAsync("rating").ConfigureAwait(false))?.Value;
                                if (!string.IsNullOrEmpty(strRating))
                                    strText += strSpace + await LanguageManager.GetStringAsync("String_Rating").ConfigureAwait(false) + strSpace + strRating;
                                TreeNode objChildChild = new TreeNode
                                {
                                    Text = strText
                                };
                                foreach (XPathNavigator objXmlGearNode in await objXmlAccessory.SelectAndCacheExpressionAsync("gears/gear").ConfigureAwait(false))
                                    await WriteGear(objXmlGearNode, objChildChild).ConfigureAwait(false);
                                objChild.Nodes.Add(objChildChild);
                            }
                            strName = (await objXmlWeapon.SelectSingleNodeAndCacheExpressionAsync("underbarrel").ConfigureAwait(false))?.Value;
                            // Check for Underbarrel Weapons.
                            if (!string.IsNullOrEmpty(strName))
                            {
                                if (await objXmlWeapon.SelectSingleNodeAndCacheExpressionAsync("hide").ConfigureAwait(false) != null)
                                    continue;

                                XPathNavigator objChildNode = _xmlWeaponsBaseChummerNode.SelectSingleNode("weapons/weapon[(" + await _objCharacter.Settings.BookXPathAsync().ConfigureAwait(false) + ") and name = " + strName.CleanXPath() + ']');
                                if (objChildNode == null)
                                    continue;
                                TreeNode objChildChild = new TreeNode
                                {
                                    Text = (await objChildNode.SelectSingleNodeAndCacheExpressionAsync("translate").ConfigureAwait(false))?.Value ?? strName
                                };
                                objChild.Nodes.Add(objChildChild);
                            }
                            objParent.Nodes.Add(objChild);
                        }
                        break;

                    case "gears":
                        objParent.Text = await LanguageManager.GetStringAsync("String_SelectPACKSKit_Gear").ConfigureAwait(false);
                        foreach (XPathNavigator objXmlGear in await objXmlItem.SelectAndCacheExpressionAsync("gear").ConfigureAwait(false))
                        {
                            if (await objXmlGear.SelectSingleNodeAndCacheExpressionAsync("hide").ConfigureAwait(false) != null)
                                continue;
                            await WriteGear(objXmlGear, objParent).ConfigureAwait(false);
                        }
                        break;

                    case "vehicles":
                        objParent.Text = await LanguageManager.GetStringAsync("String_SelectPACKSKit_Vehicles").ConfigureAwait(false);
                        foreach (XPathNavigator objXmlVehicle in await objXmlItem.SelectAndCacheExpressionAsync("vehicle").ConfigureAwait(false))
                        {
                            if (await objXmlVehicle.SelectSingleNodeAndCacheExpressionAsync("hide").ConfigureAwait(false) != null)
                                continue;
                            string strName = (await objXmlVehicle.SelectSingleNodeAndCacheExpressionAsync("name").ConfigureAwait(false)).Value;
                            XPathNavigator objNode = _xmlVehiclesBaseChummerNode.SelectSingleNode("vehicles/vehicle[(" + await _objCharacter.Settings.BookXPathAsync().ConfigureAwait(false) + ") and name = " + strName.CleanXPath() + ']');
                            if (objNode == null)
                                continue;
                            TreeNode objChild = new TreeNode
                            {
                                Text = (await objNode.SelectSingleNodeAndCacheExpressionAsync("translate").ConfigureAwait(false))?.Value ?? strName
                            };
                            // Check for children.
                            foreach (XPathNavigator objXmlMod in await objXmlVehicle.SelectAndCacheExpressionAsync("mods/mod").ConfigureAwait(false))
                            {
                                if (await objXmlMod.SelectSingleNodeAndCacheExpressionAsync("hide").ConfigureAwait(false) != null)
                                    continue;
                                strName = (await objXmlMod.SelectSingleNodeAndCacheExpressionAsync("name").ConfigureAwait(false)).Value;
                                XPathNavigator objChildNode = _xmlVehiclesBaseChummerNode.SelectSingleNode("mods/mod[(" + await _objCharacter.Settings.BookXPathAsync().ConfigureAwait(false) + ") and name = " + strName.CleanXPath() + ']');
                                if (objChildNode == null)
                                    continue;
                                string strText = (await objChildNode.SelectSingleNodeAndCacheExpressionAsync("translate").ConfigureAwait(false))?.Value
                                                 ?? strName;
                                string strRating = (await objXmlMod.SelectSingleNodeAndCacheExpressionAsync("rating").ConfigureAwait(false))?.Value;
                                if (!string.IsNullOrEmpty(strRating))
                                    strText += strSpace + await LanguageManager.GetStringAsync("String_Rating").ConfigureAwait(false) + strSpace + strRating;
                                TreeNode objChildChild = new TreeNode
                                {
                                    Text = strText
                                };
                                objChild.Nodes.Add(objChildChild);
                            }
                            // Check for children.
                            foreach (XPathNavigator objXmlChild in await objXmlVehicle.SelectAndCacheExpressionAsync("gears/gear").ConfigureAwait(false))
                            {
                                if (await objXmlChild.SelectSingleNodeAndCacheExpressionAsync("hide").ConfigureAwait(false) != null)
                                    continue;
                                await WriteGear(objXmlChild, objChild).ConfigureAwait(false);
                            }
                            // Check for children.
                            foreach (XPathNavigator objXmlWeapon in await objXmlVehicle.SelectAndCacheExpressionAsync("weapons/weapon").ConfigureAwait(false))
                            {
                                if (await objXmlWeapon.SelectSingleNodeAndCacheExpressionAsync("hide").ConfigureAwait(false) != null)
                                    continue;
                                strName = (await objXmlWeapon.SelectSingleNodeAndCacheExpressionAsync("name").ConfigureAwait(false)).Value;
                                XPathNavigator objChildNode = _xmlWeaponsBaseChummerNode.SelectSingleNode("weapons/weapon[(" + await _objCharacter.Settings.BookXPathAsync().ConfigureAwait(false) + ") and name = " + strName.CleanXPath() + ']');
                                if (objChildNode == null)
                                    continue;
                                TreeNode objChildChild = new TreeNode
                                {
                                    Text = (await objChildNode.SelectSingleNodeAndCacheExpressionAsync("translate").ConfigureAwait(false))?.Value ?? strName
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

            if (Program.ShowScrollableMessageBox(this, string.Format(GlobalSettings.CultureInfo, await LanguageManager.GetStringAsync("Message_DeletePACKSKit").ConfigureAwait(false), strSelectedKit),
                                                 await LanguageManager.GetStringAsync("MessageTitle_Delete").ConfigureAwait(false), MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.No)
                return;

            // Delete the selected custom PACKS Kit.
            // Find a custom PACKS Kit with the name. This is done without the XmlManager since we need to check each file individually.
            foreach (string strFile in Directory.EnumerateFiles(Utils.GetDataFolderPath, "*_packs.xml"))
            {
                if (!Path.GetFileName(strFile).StartsWith("custom_", StringComparison.OrdinalIgnoreCase))
                    continue;
                XmlDocument objXmlDocument = new XmlDocument { XmlResolver = null };
                try
                {
                    await objXmlDocument.LoadStandardAsync(strFile).ConfigureAwait(false);
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

            // Reload the PACKS files since they have changed.
            _xmlBaseChummerNode = await (await _objCharacter.LoadDataXPathAsync("packs.xml").ConfigureAwait(false)).SelectSingleNodeAndCacheExpressionAsync("/chummer").ConfigureAwait(false);
            await RefreshCategories().ConfigureAwait(false);
        }

        #endregion Control Events

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
            string[] objSelectedKit = strSelectedKit.Split('<', StringSplitOptions.RemoveEmptyEntries);
            _strSelectedKit = objSelectedKit[0];
            _strSelectCategory = objSelectedKit[1];
            DialogResult = DialogResult.OK;
            Close();
        }

        private async ValueTask WriteGear(XPathNavigator objXmlGear, TreeNode objParent)
        {
            XPathNavigator xmlNameNode = await objXmlGear.SelectSingleNodeAndCacheExpressionAsync("name").ConfigureAwait(false);
            string strName = xmlNameNode?.Value ?? string.Empty;
            string strCategory = (await objXmlGear.SelectSingleNodeAndCacheExpressionAsync("category").ConfigureAwait(false))?.Value;
            XPathNavigator objNode = !string.IsNullOrEmpty(strCategory)
                ? _xmlGearsBaseGearsNode.SelectSingleNode("gear[(" + await _objCharacter.Settings.BookXPathAsync().ConfigureAwait(false) + ") and name = " + strName.CleanXPath() + " and category = " + strCategory.CleanXPath() + ']')
                : _xmlGearsBaseGearsNode.SelectSingleNode("gear[(" + await _objCharacter.Settings.BookXPathAsync().ConfigureAwait(false) + ") and name = " + strName.CleanXPath() + ']');

            if (objNode != null)
            {
                TreeNode objChild = new TreeNode
                {
                    Text = (await objNode.SelectSingleNodeAndCacheExpressionAsync("@translate").ConfigureAwait(false))?.Value ?? strName
                };

                string strSpace = await LanguageManager.GetStringAsync("String_Space").ConfigureAwait(false);

                string strExtra = xmlNameNode != null
                    ? (await xmlNameNode.SelectSingleNodeAndCacheExpressionAsync("@select").ConfigureAwait(false))?.Value
                    : string.Empty;
                if (!string.IsNullOrEmpty(strExtra))
                    objChild.Text += strSpace + '(' + strExtra + ')';
                strExtra = (await objXmlGear.SelectSingleNodeAndCacheExpressionAsync("rating").ConfigureAwait(false))?.Value;
                if (!string.IsNullOrEmpty(strExtra))
                    objChild.Text += strSpace + await LanguageManager.GetStringAsync("String_Rating").ConfigureAwait(false) + strSpace + strExtra;
                strExtra = (await objXmlGear.SelectSingleNodeAndCacheExpressionAsync("qty").ConfigureAwait(false))?.Value;
                if (!string.IsNullOrEmpty(strExtra))
                    objChild.Text += strSpace + 'x' + strExtra;

                if (objParent.TreeView != null)
                    await objParent.TreeView.DoThreadSafeAsync(() => objParent.Nodes.Add(objChild)).ConfigureAwait(false);
                else
                    objParent.Nodes.Add(objChild);

                // Check for children.
                foreach (XPathNavigator objXmlChild in await objXmlGear.SelectAndCacheExpressionAsync("gears/gear").ConfigureAwait(false))
                {
                    await WriteGear(objXmlChild, objChild).ConfigureAwait(false);
                }

                if (objChild.TreeView != null)
                    await objChild.TreeView.DoThreadSafeAsync(() => objChild.Expand()).ConfigureAwait(false);
                else
                    objChild.Expand();
            }
        }

        #endregion Methods
    }
}
