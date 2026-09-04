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
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using Chummer.Backend.Equipment;

namespace Chummer
{
    public partial class CreateCustomDrug : Form, IHasCharacterObject
    {
        private readonly Dictionary<string, DrugComponent> _dicDrugComponents;
        private readonly List<DrugNodeData> _lstSelectedDrugComponents;
        private List<ListItem> _lstGrade;
        private readonly Character _objCharacter;
        private Drug _objDrug;
        private readonly XmlDocument _objXmlDocument;

        public Character CharacterObject => _objCharacter;

        public CreateCustomDrug(Character objCharacter)
        {
            _objCharacter = objCharacter ?? throw new ArgumentNullException(nameof(objCharacter));
            _objDrug = new Drug(objCharacter);
            InitializeComponent();
            this.UpdateLightDarkMode();
            this.TranslateWinForm();
            this.UpdateParentForToolTipControls();
            _objXmlDocument = objCharacter.LoadData(DrugsData.ComponentsFileName);
            XmlNodeList xmlComponentsNodeList = _objXmlDocument.SelectNodes("chummer/drugcomponents/drugcomponent");
            _dicDrugComponents = new Dictionary<string, DrugComponent>(xmlComponentsNodeList?.Count ?? 0);
            if (xmlComponentsNodeList?.Count > 0)
            {
                foreach (XmlNode objXmlComponent in xmlComponentsNodeList)
                {
                    DrugComponent objDrugComponent = new DrugComponent(_objCharacter);
                    objDrugComponent.Load(objXmlComponent);
                    if (string.Equals(objDrugComponent.Category, "BTLs", StringComparison.OrdinalIgnoreCase))
                        continue;
                    _dicDrugComponents[objDrugComponent.Name] = objDrugComponent;
                }
            }
            _lstSelectedDrugComponents = new List<DrugNodeData>(5);
            _lstGrade = Utils.ListItemListPool.Get();
        }

        private async void CreateCustomDrug_Load(object sender, EventArgs e)
        {
            string strLevelString = await LanguageManager.GetStringAsync("String_Level").ConfigureAwait(false);
            string strSpaceString = await LanguageManager.GetStringAsync("String_Space").ConfigureAwait(false);
            foreach (KeyValuePair<string, DrugComponent> objItem in _dicDrugComponents)
            {
                string strCategory = objItem.Value.Category;
                TreeNode nodCategoryNode = await GetOrCreateCategoryNode(treAvailableComponents, strCategory).ConfigureAwait(false);
                if (nodCategoryNode == null)
                    continue;

                string strName = await objItem.Value.GetCurrentDisplayNameShortAsync().ConfigureAwait(false);
                TreeNode objNode = await treAvailableComponents.DoThreadSafeFuncAsync(() => nodCategoryNode.Nodes.Add(strName)).ConfigureAwait(false);
                int intLevelCount = objItem.Value.DrugEffects.Count;
                if (intLevelCount == 1)
                {
                    objNode.Tag = new DrugNodeData(objItem.Value, 0);
                }
                else
                {
                    objNode.Tag = new DrugNodeData(objItem.Value);
                    for (int i = 0; i < intLevelCount; i++)
                    {
                        int i1 = i;
                        TreeNode objSubNode = await treAvailableComponents.DoThreadSafeFuncAsync(
                            () => objNode.Nodes.Add(strLevelString + strSpaceString
                                                                   + (i1 + 1).ToString(GlobalSettings.CultureInfo))).ConfigureAwait(false);
                        objSubNode.Tag = new DrugNodeData(objItem.Value, i);
                    }
                }
            }
            await treAvailableComponents.DoThreadSafeAsync(x => x.ExpandAll()).ConfigureAwait(false);
            await treChosenComponents.DoThreadSafeAsync(x => x.ExpandAll()).ConfigureAwait(false);
            await PopulateGrades().ConfigureAwait(false);
            await UpdateCustomDrugStats().ConfigureAwait(false);
            string strDescription = await _objDrug.GenerateDescriptionAsync(0).ConfigureAwait(false);
            await lblDrugDescription.DoThreadSafeAsync(x => x.Text = strDescription).ConfigureAwait(false);
        }

        /// <summary>
        /// Populate the list of Drug Grades.
        /// </summary>
        private async Task PopulateGrades(CancellationToken token = default)
        {
            _lstGrade.Clear();
            foreach (Grade objGrade in await _objCharacter.GetGradesListAsync(Improvement.ImprovementSource.Drug, token: token).ConfigureAwait(false))
            {
                _lstGrade.Add(new ListItem(objGrade.Name, await objGrade.GetCurrentDisplayNameAsync(token).ConfigureAwait(false)));
            }
            await cboGrade.PopulateWithListItemsAsync(_lstGrade, token: token).ConfigureAwait(false);
        }

        private async Task UpdateCustomDrugStats(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            Drug objNewDrug = new Drug(_objCharacter)
            {
                Category = "Custom Drugs"
            };
            await objNewDrug.SetNameAsync(await txtDrugName.DoThreadSafeFuncAsync(x => x.Text, token).ConfigureAwait(false), token).ConfigureAwait(false);
            if (_objCharacter != null)
            {
                string strSelectedGrade = cboGrade != null
                    ? await cboGrade.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token).ConfigureAwait(false)
                    : string.Empty;
                if (!string.IsNullOrEmpty(strSelectedGrade))
                    objNewDrug.Grade = await Grade.ConvertToCyberwareGradeAsync(strSelectedGrade, Improvement.ImprovementSource.Drug, _objCharacter, token).ConfigureAwait(false);
            }

            foreach (DrugNodeData objNodeData in _lstSelectedDrugComponents)
            {
                DrugComponent objDrugComponent = objNodeData.DrugComponent;
                objDrugComponent.Level = objNodeData.Level;
                await objNewDrug.Components.AddAsync(objDrugComponent, token: token).ConfigureAwait(false);
            }

            Drug objOldDrug = Interlocked.Exchange(ref _objDrug, objNewDrug);
            if (objOldDrug != null)
                await objOldDrug.DisposeAsync().ConfigureAwait(false);
        }

        private async Task AcceptForm(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (string.IsNullOrEmpty(txtDrugName.Text))
            {
                await Program.ShowScrollableMessageBoxAsync(this, await LanguageManager.GetStringAsync("Message_CustomDrug_Name", token: token).ConfigureAwait(false), await LanguageManager.GetStringAsync("MessageTitle_CustomDrug_Name", token: token).ConfigureAwait(false), MessageBoxButtons.OK, MessageBoxIcon.Information, token: token).ConfigureAwait(false);
                return;
            }

            if (await _objDrug.Components.CountAsync(o => o.Category == "Foundation", token).ConfigureAwait(false) != 1)
            {
                await Program.ShowScrollableMessageBoxAsync(this, await LanguageManager.GetStringAsync("Message_CustomDrug_MissingFoundation", token: token).ConfigureAwait(false), await LanguageManager.GetStringAsync("MessageTitle_CustomDrug_Foundation", token: token).ConfigureAwait(false), MessageBoxButtons.OK, MessageBoxIcon.Information, token: token).ConfigureAwait(false);
                return;
            }

            _objDrug.Quantity = 1;
            await this.DoThreadSafeAsync(x =>
            {
                x.DialogResult = DialogResult.OK;
                x.Close();
            }, token).ConfigureAwait(false);
        }

        private async Task AddSelectedComponent(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (!(await treAvailableComponents.DoThreadSafeFuncAsync(x => x.SelectedNode?.Tag, token).ConfigureAwait(false) is DrugNodeData objNodeData) || objNodeData.Level == -1)
            {
                return;
            }

            string strCategory = objNodeData.DrugComponent.Category;
            TreeNode nodCategoryNode = await GetOrCreateCategoryNode(treChosenComponents, strCategory, token).ConfigureAwait(false);
            if (nodCategoryNode == null)
                return;

            if (_lstSelectedDrugComponents.Count(c => c.DrugComponent.Name == objNodeData.DrugComponent.Name) >=
                objNodeData.DrugComponent.Limit && objNodeData.DrugComponent.Limit != 0)
            {
                await Program.ShowScrollableMessageBoxAsync(this,
                    string.Format(GlobalSettings.CultureInfo, await LanguageManager.GetStringAsync("Message_DuplicateDrugComponentWarning", token: token).ConfigureAwait(false),
                        objNodeData.DrugComponent.Limit), token: token).ConfigureAwait(false);
                return;
            }

            if (objNodeData.DrugComponent.Category == "Foundation" && _lstSelectedDrugComponents.Exists(c => c.DrugComponent.Category == "Foundation"))
            {
                await Program.ShowScrollableMessageBoxAsync(this, await LanguageManager.GetStringAsync("Message_DuplicateDrugFoundationWarning", token: token).ConfigureAwait(false), token: token).ConfigureAwait(false);
                return;
            }

            string strSpaceString = await LanguageManager.GetStringAsync("String_Space", token: token).ConfigureAwait(false);
            if (objNodeData.Level + 1 > 2)
            {
                string strColonString = await LanguageManager.GetStringAsync("String_Colon", token: token).ConfigureAwait(false);
                foreach (DrugComponent objFoundationComponent in _lstSelectedDrugComponents.Select(x => x.DrugComponent))
                {
                    if (objFoundationComponent.Category != "Foundation")
                        continue;
                    Dictionary<string, decimal> dctFoundationAttributes = objFoundationComponent.DrugEffects[0].Attributes;
                    Dictionary<string, decimal> dctBlockAttributes = objNodeData.DrugComponent.DrugEffects[objNodeData.Level].Attributes;
                    foreach (KeyValuePair<string, decimal> objItem in dctFoundationAttributes)
                    {
                        if (objItem.Value < 0 &&
                            dctBlockAttributes.TryGetValue(objItem.Key, out decimal decBlockAttrValue) &&
                            decBlockAttrValue > 0)
                        {
                            string strMessage = await LanguageManager.GetStringAsync("String_MaximumDrugBlockLevel", token: token).ConfigureAwait(false) +
                                                Environment.NewLine + Environment.NewLine +
                                                await objFoundationComponent.GetCurrentDisplayNameAsync(token).ConfigureAwait(false) + strColonString +
                                                strSpaceString + objItem.Key +
                                                objItem.Value.ToString("+#;-#;", GlobalSettings.CultureInfo) +
                                                await objNodeData.DrugComponent.GetCurrentDisplayNameAsync(token).ConfigureAwait(false) + strColonString +
                                                strSpaceString + objItem.Key +
                                                decBlockAttrValue.ToString("+#.#;-#.#;", GlobalSettings.CultureInfo);
                            await Program.ShowScrollableMessageBoxAsync(this, strMessage, token: token).ConfigureAwait(false);
                            return;
                        }
                    }
                }
            }

            DrugComponent objClone = objNodeData.DrugComponent.Clone();
            objClone.Level = objNodeData.Level;
            DrugNodeData objChosenData = new DrugNodeData(objClone, objNodeData.Level);
            string strNodeText = await objClone.GetCurrentDisplayNameAsync(token).ConfigureAwait(false);
            await treChosenComponents.DoThreadSafeAsync(() =>
            {
                TreeNode objNewNode = nodCategoryNode.Nodes.Add(strNodeText);
                objNewNode.Tag = objChosenData;
                objNewNode.EnsureVisible();
            }, token).ConfigureAwait(false);
            _lstSelectedDrugComponents.Add(objChosenData);
            await UpdateCustomDrugStats(token).ConfigureAwait(false);
            string strDescription = await _objDrug.GenerateDescriptionAsync(0, token: token).ConfigureAwait(false);
            await lblDrugDescription.DoThreadSafeAsync(x => x.Text = strDescription, token).ConfigureAwait(false);
        }

        public Drug CustomDrug => _objDrug;

        private static async Task<TreeNode> GetOrCreateCategoryNode(TreeView treComponents, string strCategory,
            CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            string strTag = "Node_" + strCategory;
            TreeNode nodCategoryNode = await treComponents
                .DoThreadSafeFuncAsync(x => x.FindNode(strTag), token).ConfigureAwait(false);
            if (nodCategoryNode != null)
                return nodCategoryNode;
            string strText = await LanguageManager.GetStringAsync(strTag, false, token).ConfigureAwait(false);
            if (string.IsNullOrEmpty(strText))
                strText = strCategory;
            return await treComponents.DoThreadSafeFuncAsync(() =>
            {
                TreeNode objNew = treComponents.Nodes.Add(strText);
                objNew.Name = strTag;
                objNew.Tag = strTag;
                return objNew;
            }, token).ConfigureAwait(false);
        }

        private async void treAvailableComponents_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (await treAvailableComponents.DoThreadSafeFuncAsync(x => x.SelectedNode?.Tag).ConfigureAwait(false) is DrugNodeData objNodeData)
            {
                string strDescription = await objNodeData.DrugComponent.GenerateDescriptionAsync(objNodeData.Level).ConfigureAwait(false);
                await lblBlockDescription.DoThreadSafeAsync(x => x.Text = strDescription).ConfigureAwait(false);
            }
        }

        private async void treChoosenComponents_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (await treChosenComponents.DoThreadSafeFuncAsync(x => x.SelectedNode?.Tag).ConfigureAwait(false) is DrugNodeData objNodeData)
            {
                string strDescription = await objNodeData.DrugComponent.GenerateDescriptionAsync(objNodeData.Level).ConfigureAwait(false);
                await lblBlockDescription.DoThreadSafeAsync(x => x.Text = strDescription).ConfigureAwait(false);
            }
        }

        private async void btnAddComponent_Click(object sender, EventArgs e)
        {
            await AddSelectedComponent().ConfigureAwait(false);
        }

        private async void treAvailableComponents_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            await AddSelectedComponent().ConfigureAwait(false);
        }

        private async void btnRemoveComponent_Click(object sender, EventArgs e)
        {
            if (!(await treChosenComponents.DoThreadSafeFuncAsync(x => x.SelectedNode?.Tag).ConfigureAwait(false) is DrugNodeData objNodeData))
                return;
            await treChosenComponents.DoThreadSafeAsync(x => x.Nodes.Remove(x.SelectedNode)).ConfigureAwait(false);

            _lstSelectedDrugComponents.Remove(objNodeData);

            await UpdateCustomDrugStats().ConfigureAwait(false);
            string strDescription = await _objDrug.GenerateDescriptionAsync(0).ConfigureAwait(false);
            await lblDrugDescription.DoThreadSafeAsync(x => x.Text = strDescription).ConfigureAwait(false);
        }

        private async void txtDrugName_TextChanged(object sender, EventArgs e)
        {
            await _objDrug.SetNameAsync(await txtDrugName.DoThreadSafeFuncAsync(x => x.Text).ConfigureAwait(false)).ConfigureAwait(false);
            string strDescription = await _objDrug.GenerateDescriptionAsync(0).ConfigureAwait(false);
            await lblDrugDescription.DoThreadSafeAsync(x => x.Text = strDescription).ConfigureAwait(false);
        }

        private async void btnOk_Click(object sender, EventArgs e)
        {
            await AcceptForm().ConfigureAwait(false);
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Interlocked.Exchange(ref _objDrug, null)?.Dispose();
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private async void cboGrade_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cboGrade.SelectedValue == null)
                return;

            await UpdateCustomDrugStats().ConfigureAwait(false);
            string strDescription = await _objDrug.GenerateDescriptionAsync(0).ConfigureAwait(false);
            await lblDrugDescription.DoThreadSafeAsync(x => x.Text = strDescription).ConfigureAwait(false);
        }

        private sealed class DrugNodeData
        {
            public DrugComponent DrugComponent { get; }
            public int Level { get; }

            public DrugNodeData(DrugComponent objDrugComponent, int level = -1)
            {
                DrugComponent = objDrugComponent;
                Level = level;
            }
        }
    }
}
