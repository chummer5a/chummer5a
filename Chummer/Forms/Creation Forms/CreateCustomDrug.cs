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
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using Chummer.Backend.Equipment;
using NLog;

namespace Chummer
{
    public partial class CreateCustomDrug : Form
    {
        private static Logger Log { get; } = LogManager.GetCurrentClassLogger();
        private readonly Dictionary<string, DrugComponent> _dicDrugComponents = new Dictionary<string, DrugComponent>();
        private readonly List<DrugNodeData> _lstSelectedDrugComponents;
        private readonly List<ListItem> _lstGrade = Utils.ListItemListPool.Get();
        private readonly Character _objCharacter;
        private Drug _objDrug;
        private readonly XmlDocument _objXmlDocument;
        private double _dblCostMultiplier;
        private int _intAddictionThreshold;

        public CreateCustomDrug(Character objCharacter, Drug objDrug = null)
        {
            if (objDrug == null)
            {
                objDrug = new Drug(objCharacter);
            }
            _objCharacter = objCharacter;
            InitializeComponent();
            this.UpdateLightDarkMode();
            this.TranslateWinForm();
            _objXmlDocument = objCharacter.LoadData("drugcomponents.xml");
            LoadData();

            _lstSelectedDrugComponents = new List<DrugNodeData>(5);

            string strLevelString = LanguageManager.GetString("String_Level");
            string strSpaceString = LanguageManager.GetString("String_Space");
            foreach (KeyValuePair<string, DrugComponent> objItem in _dicDrugComponents)
            {
                string strCategory = objItem.Value.Category;
                TreeNode nodCategoryNode = treAvailableComponents.FindNode("Node_" + strCategory);
                if (nodCategoryNode == null)
                {
                    Log.Warn("Unknown category " + strCategory + " in component " + objItem.Key);
                    return;
                }
                TreeNode objNode = nodCategoryNode.Nodes.Add(objItem.Value.DisplayNameShort(GlobalSettings.Language));
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
                        TreeNode objSubNode = objNode.Nodes.Add(strLevelString + strSpaceString + (i + 1).ToString(GlobalSettings.CultureInfo));
                        objSubNode.Tag = new DrugNodeData(objItem.Value, i);
                    }
                }
            }
            treAvailableComponents.ExpandAll();
            treChosenComponents.ExpandAll();
            PopulateGrades();
            UpdateCustomDrugStats();
            lblDrugDescription.Text = objDrug.Description;
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
        private void PopulateGrades()
        {
            _lstGrade.Clear();
            foreach (Grade objGrade in _objCharacter.GetGradeList(Improvement.ImprovementSource.Drug))
            {
                _lstGrade.Add(new ListItem(objGrade.Name, objGrade.CurrentDisplayName));
            }
            cboGrade.BeginUpdate();
            cboGrade.PopulateWithListItems(_lstGrade);
            cboGrade.EndUpdate();
        }

        private void UpdateCustomDrugStats()
        {
            _objDrug = new Drug(_objCharacter)
            {
                Name = txtDrugName.Text,
                Category = "Custom Drug"
            };
            if ((_objCharacter != null) && (!string.IsNullOrEmpty(cboGrade?.SelectedValue?.ToString())))
                _objDrug.Grade = Grade.ConvertToCyberwareGrade(cboGrade.SelectedValue.ToString(),
                    Improvement.ImprovementSource.Drug, _objCharacter);

            foreach (DrugNodeData objNodeData in _lstSelectedDrugComponents)
            {
                DrugComponent objDrugComponent = objNodeData.DrugComponent;
                objDrugComponent.Level = objNodeData.Level;
                _objDrug.Components.Add(objDrugComponent);
            }
        }

        private async ValueTask AcceptForm()
        {
            // Make sure the suite and file name fields are populated.
            if (string.IsNullOrEmpty(txtDrugName.Text))
            {
                Program.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_CustomDrug_Name"), await LanguageManager.GetStringAsync("MessageTitle_CustomDrug_Name"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (_objDrug.Components.Count(o => o.Category == "Foundation") != 1)
            {
                Program.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_CustomDrug_MissingFoundation"), await LanguageManager.GetStringAsync("MessageTitle_CustomDrug_Foundation"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            _objDrug.Quantity = 1;
            DialogResult = DialogResult.OK;
            Close();
        }

        private async ValueTask AddSelectedComponent()
        {
            if (!(treAvailableComponents.SelectedNode?.Tag is DrugNodeData objNodeData) || objNodeData.Level == -1)
            {
                return;
            }

            string strCategory = objNodeData.DrugComponent.Category;
            TreeNode nodCategoryNode = treChosenComponents.FindNode("Node_" + strCategory);
            if (nodCategoryNode == null)
            {
                Log.Warn("Unknown category " + strCategory + " in component " + objNodeData.DrugComponent.Name);
                return;
            }

            //prevent adding same component multiple times.
            if (_lstSelectedDrugComponents.Count(c => c.DrugComponent.Name == objNodeData.DrugComponent.Name) >=
                objNodeData.DrugComponent.Limit && objNodeData.DrugComponent.Limit != 0)
            {
                Program.ShowMessageBox(this,
                    string.Format(GlobalSettings.CultureInfo, await LanguageManager.GetStringAsync("Message_DuplicateDrugComponentWarning"),
                        objNodeData.DrugComponent.Limit));
                return;
            }

            //drug can have only one foundation
            if (objNodeData.DrugComponent.Category == "Foundation" && _lstSelectedDrugComponents.Any(c => c.DrugComponent.Category == "Foundation"))
            {
                Program.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_DuplicateDrugFoundationWarning"));
                return;
            }

            string strSpaceString = await LanguageManager.GetStringAsync("String_Space");
            //restriction for maximum level of block (CF 191)
            if (objNodeData.Level + 1 > 2)
            {
                string strColonString = await LanguageManager.GetStringAsync("String_Colon");
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
                            string strMessage = await LanguageManager.GetStringAsync("String_MaximumDrugBlockLevel") +
                                                Environment.NewLine + Environment.NewLine +
                                                objFoundationComponent.CurrentDisplayName + strColonString +
                                                strSpaceString + objItem.Key +
                                                objItem.Value.ToString("+#;-#;", GlobalSettings.CultureInfo) +
                                                objNodeData.DrugComponent.CurrentDisplayName + strColonString +
                                                strSpaceString + objItem.Key +
                                                decBlockAttrValue.ToString("+#.#;-#.#;", GlobalSettings.CultureInfo);
                            Program.ShowMessageBox(this, strMessage);
                            return;
                        }
                    }
                }
            }

            string strNodeText = objNodeData.DrugComponent.CurrentDisplayName;
            if (objNodeData.DrugComponent.Level <= 0 && objNodeData.DrugComponent.DrugEffects.Count > 1)
                strNodeText += strSpaceString + '(' + await LanguageManager.GetStringAsync("String_Level") + strSpaceString + (objNodeData.Level + 1).ToString(GlobalSettings.CultureInfo) + ')';
            TreeNode objNewNode = nodCategoryNode.Nodes.Add(strNodeText);
            objNewNode.Tag = objNodeData;
            objNewNode.EnsureVisible();

            _lstSelectedDrugComponents.Add(objNodeData);
            UpdateCustomDrugStats();
            lblDrugDescription.Text = _objDrug.GenerateDescription(0);
        }

        public Drug CustomDrug => _objDrug;

        private void treAvailableComponents_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (treAvailableComponents.SelectedNode?.Tag is DrugNodeData objNodeData)
            {
                lblBlockDescription.Text = objNodeData.DrugComponent.GenerateDescription(objNodeData.Level);
            }
        }

        private void treChoosenComponents_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (treChosenComponents.SelectedNode?.Tag is DrugNodeData objNodeData)
            {
                lblBlockDescription.Text = objNodeData.DrugComponent.GenerateDescription(objNodeData.Level);
            }
        }

        private async void btnAddComponent_Click(object sender, EventArgs e)
        {
            await AddSelectedComponent();
        }

        private async void treAvailableComponents_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            await AddSelectedComponent();
        }

        private void btnRemoveComponent_Click(object sender, EventArgs e)
        {
            if (!(treChosenComponents.SelectedNode?.Tag is DrugNodeData objNodeData)) return;
            treChosenComponents.Nodes.Remove(treChosenComponents.SelectedNode);

            _lstSelectedDrugComponents.Remove(objNodeData);

            UpdateCustomDrugStats();
            lblDrugDescription.Text = _objDrug.GenerateDescription(0);
        }

        private void txtDrugName_TextChanged(object sender, EventArgs e)
        {
            _objDrug.Name = txtDrugName.Text;
            lblDrugDescription.Text = _objDrug.GenerateDescription(0);
        }

        private async void btnOk_Click(object sender, EventArgs e)
        {
            await AcceptForm();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            _objDrug = null;
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void cboGrade_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cboGrade.SelectedValue == null)
                return;

            // Update the Essence and Cost multipliers based on the Grade that has been selected.
            // Retrieve the information for the selected Grade.
            XmlNode objXmlGrade = _objXmlDocument.SelectSingleNode("/chummer/grades/grade[name = " + cboGrade.SelectedValue.ToString().CleanXPath() + ']');
            if (!objXmlGrade.TryGetDoubleFieldQuickly("cost", ref _dblCostMultiplier))
                _dblCostMultiplier = 1.0;
            if (!objXmlGrade.TryGetInt32FieldQuickly("addictionthreshold", ref _intAddictionThreshold))
                _intAddictionThreshold = 0;
            UpdateCustomDrugStats();
            lblDrugDescription.Text = _objDrug.GenerateDescription(0);
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
