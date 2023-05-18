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
using NLog;

namespace Chummer
{
    public partial class CreateCustomDrug : Form
    {
        private static readonly Lazy<Logger> s_ObjLogger = new Lazy<Logger>(LogManager.GetCurrentClassLogger);
        private static Logger Log => s_ObjLogger.Value;
        private readonly Dictionary<string, DrugComponent> _dicDrugComponents = new Dictionary<string, DrugComponent>();
        private readonly List<DrugNodeData> _lstSelectedDrugComponents;
        private List<ListItem> _lstGrade = Utils.ListItemListPool.Get();
        private readonly Character _objCharacter;
        private Drug _objDrug;
        private readonly XmlDocument _objXmlDocument;
        private double _dblCostMultiplier;
        private int _intAddictionThreshold;

        public CreateCustomDrug(Character objCharacter)
        {
            Disposed += (sender, args) =>
            {
                if (DialogResult != DialogResult.OK)
                    Interlocked.Exchange(ref _objDrug, null)?.Dispose();
                Utils.ListItemListPool.Return(ref _lstGrade);
            };
            _objDrug = new Drug(objCharacter);
            _objCharacter = objCharacter;
            InitializeComponent();
            this.UpdateLightDarkMode();
            this.TranslateWinForm();
            _objXmlDocument = objCharacter.LoadData("drugcomponents.xml");
            LoadData();

            _lstSelectedDrugComponents = new List<DrugNodeData>(5);
        }

        private async void CreateCustomDrug_Load(object sender, EventArgs e)
        {
            string strLevelString = await LanguageManager.GetStringAsync("String_Level").ConfigureAwait(false);
            string strSpaceString = await LanguageManager.GetStringAsync("String_Space").ConfigureAwait(false);
            foreach (KeyValuePair<string, DrugComponent> objItem in _dicDrugComponents)
            {
                string strCategory = objItem.Value.Category;
                TreeNode nodCategoryNode = await treAvailableComponents.DoThreadSafeFuncAsync(x => x.FindNode("Node_" + strCategory)).ConfigureAwait(false);
                if (nodCategoryNode == null)
                {
                    Log.Warn("Unknown category " + strCategory + " in component " + objItem.Key);
                    return;
                }

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

        private void LoadData()
        {
            XmlNodeList xmlComponentsNodeList = _objXmlDocument.SelectNodes("chummer/drugcomponents/drugcomponent");
            if (xmlComponentsNodeList?.Count > 0)
            {
                foreach (XmlNode objXmlComponent in xmlComponentsNodeList)
                {
                    DrugComponent objDrugComponent = new DrugComponent(_objCharacter);
                    objDrugComponent.Load(objXmlComponent);
                    _dicDrugComponents[objDrugComponent.Name] = objDrugComponent;
                }
            }
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

        private async ValueTask UpdateCustomDrugStats(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            Drug objNewDrug = new Drug(_objCharacter)
            {
                Name = await txtDrugName.DoThreadSafeFuncAsync(x => x.Text, token).ConfigureAwait(false),
                Category = "Custom Drug"
            };
            if (_objCharacter != null)
            {
                string strSelectedGrade = cboGrade != null
                    ? await cboGrade.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token).ConfigureAwait(false)
                    : string.Empty;
                if (!string.IsNullOrEmpty(strSelectedGrade))
                    objNewDrug.Grade = Grade.ConvertToCyberwareGrade(strSelectedGrade, Improvement.ImprovementSource.Drug, _objCharacter);
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

        private async ValueTask AcceptForm(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            // Make sure the suite and file name fields are populated.
            if (string.IsNullOrEmpty(txtDrugName.Text))
            {
                Program.ShowScrollableMessageBox(this, await LanguageManager.GetStringAsync("Message_CustomDrug_Name", token: token).ConfigureAwait(false), await LanguageManager.GetStringAsync("MessageTitle_CustomDrug_Name", token: token).ConfigureAwait(false), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (_objDrug.Components.Count(o => o.Category == "Foundation") != 1)
            {
                Program.ShowScrollableMessageBox(this, await LanguageManager.GetStringAsync("Message_CustomDrug_MissingFoundation", token: token).ConfigureAwait(false), await LanguageManager.GetStringAsync("MessageTitle_CustomDrug_Foundation", token: token).ConfigureAwait(false), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            _objDrug.Quantity = 1;
            await this.DoThreadSafeAsync(x =>
            {
                x.DialogResult = DialogResult.OK;
                x.Close();
            }, token).ConfigureAwait(false);
        }

        private async ValueTask AddSelectedComponent(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (!(await treAvailableComponents.DoThreadSafeFuncAsync(x => x.SelectedNode?.Tag, token).ConfigureAwait(false) is DrugNodeData objNodeData) || objNodeData.Level == -1)
            {
                return;
            }

            string strCategory = objNodeData.DrugComponent.Category;
            TreeNode nodCategoryNode = await treChosenComponents.DoThreadSafeFuncAsync(x => x.FindNode("Node_" + strCategory), token).ConfigureAwait(false);
            if (nodCategoryNode == null)
            {
                Log.Warn("Unknown category " + strCategory + " in component " + objNodeData.DrugComponent.Name);
                return;
            }

            //prevent adding same component multiple times.
            if (_lstSelectedDrugComponents.Count(c => c.DrugComponent.Name == objNodeData.DrugComponent.Name) >=
                objNodeData.DrugComponent.Limit && objNodeData.DrugComponent.Limit != 0)
            {
                Program.ShowScrollableMessageBox(this,
                    string.Format(GlobalSettings.CultureInfo, await LanguageManager.GetStringAsync("Message_DuplicateDrugComponentWarning", token: token).ConfigureAwait(false),
                        objNodeData.DrugComponent.Limit));
                return;
            }

            //drug can have only one foundation
            if (objNodeData.DrugComponent.Category == "Foundation" && _lstSelectedDrugComponents.Any(c => c.DrugComponent.Category == "Foundation"))
            {
                Program.ShowScrollableMessageBox(this, await LanguageManager.GetStringAsync("Message_DuplicateDrugFoundationWarning", token: token).ConfigureAwait(false));
                return;
            }

            string strSpaceString = await LanguageManager.GetStringAsync("String_Space", token: token).ConfigureAwait(false);
            //restriction for maximum level of block (CF 191)
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
                            Program.ShowScrollableMessageBox(this, strMessage);
                            return;
                        }
                    }
                }
            }

            string strNodeText = await objNodeData.DrugComponent.GetCurrentDisplayNameAsync(token).ConfigureAwait(false);
            if (objNodeData.DrugComponent.Level <= 0 && objNodeData.DrugComponent.DrugEffects.Count > 1)
                strNodeText += strSpaceString + '(' + await LanguageManager.GetStringAsync("String_Level", token: token).ConfigureAwait(false) + strSpaceString + (objNodeData.Level + 1).ToString(GlobalSettings.CultureInfo) + ')';
            await treChosenComponents.DoThreadSafeAsync(() =>
            {
                TreeNode objNewNode = nodCategoryNode.Nodes.Add(strNodeText);
                objNewNode.Tag = objNodeData;
                objNewNode.EnsureVisible();
            }, token).ConfigureAwait(false);
            _lstSelectedDrugComponents.Add(objNodeData);
            await UpdateCustomDrugStats(token).ConfigureAwait(false);
            string strDescription = await _objDrug.GenerateDescriptionAsync(0, token: token).ConfigureAwait(false);
            await lblDrugDescription.DoThreadSafeAsync(x => x.Text = strDescription, token).ConfigureAwait(false);
        }

        public Drug CustomDrug => _objDrug;

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
            _objDrug.Name = await txtDrugName.DoThreadSafeFuncAsync(x => x.Text).ConfigureAwait(false);
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

            // Update the Essence and Cost multipliers based on the Grade that has been selected.
            // Retrieve the information for the selected Grade.
            XmlNode objXmlGrade = _objXmlDocument.SelectSingleNode("/chummer/grades/grade[name = " + (await cboGrade.DoThreadSafeFuncAsync(x => x.SelectedValue.ToString()).ConfigureAwait(false)).CleanXPath() + ']');
            if (!objXmlGrade.TryGetDoubleFieldQuickly("cost", ref _dblCostMultiplier))
                _dblCostMultiplier = 1.0;
            if (!objXmlGrade.TryGetInt32FieldQuickly("addictionthreshold", ref _intAddictionThreshold))
                _intAddictionThreshold = 0;
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
