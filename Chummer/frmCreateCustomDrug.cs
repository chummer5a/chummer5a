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
using System.Xml;
using Chummer.Backend.Equipment;

namespace Chummer
{
    public partial class frmCreateCustomDrug : Form
	{
		private readonly Dictionary<string, DrugComponent> _dicDrugComponents = new Dictionary<string, DrugComponent>();
        private readonly List<clsNodeData> _lstSelectedDrugComponents;
		private readonly List<ListItem> _lstGrade = new List<ListItem>();
		private readonly Character _objCharacter;
	    private Drug _objDrug;
	    readonly XmlDocument _objXmlDocument = XmlManager.Load("drugcomponents.xml");
		private double _dblCostMultiplier;
		private int _intAddictionThreshold;

		public frmCreateCustomDrug(Character objCharacter, Drug objDrug = null)
        {
	        if (objDrug == null)
	        {
	            objDrug = new Drug(objCharacter);
	        }
	        _objCharacter = objCharacter;
            InitializeComponent();
            LanguageManager.TranslateWinForm(GlobalOptions.Language, this);
            LoadData();

            _lstSelectedDrugComponents = new List<clsNodeData>();

            string strLevelString = LanguageManager.GetString("String_Level");
            string strSpaceString = LanguageManager.GetString("String_Space");
            foreach (KeyValuePair<string, DrugComponent> objItem in _dicDrugComponents)
            {
                string strCategory = objItem.Value.Category;
                TreeNode nodCategoryNode = treAvailableComponents.FindNode("Node_" + strCategory);
                if (nodCategoryNode == null)
                {
                    Log.Warning($"Unknown category {strCategory} in component {objItem.Key}");
                    return;
                }
                TreeNode objNode = nodCategoryNode.Nodes.Add(objItem.Value.DisplayNameShort(GlobalOptions.Language));
                int intLevelCount = objItem.Value.DrugEffects.Count;
                if (intLevelCount == 1)
                {
                    objNode.Tag = new clsNodeData(objItem.Value, 0);
                }
                else
                {
                    objNode.Tag = new clsNodeData(objItem.Value);
                    for (int i = 0; i < intLevelCount; i++)
                    {
                        TreeNode objSubNode = objNode.Nodes.Add(strLevelString + strSpaceString + (i + 1).ToString());
                        objSubNode.Tag = new clsNodeData(objItem.Value, i);
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
		    IList<Grade>  objGradeList = _objCharacter.GetGradeList(Improvement.ImprovementSource.Drug);

			_lstGrade.Clear();
			foreach (Grade objGrade in objGradeList)
			{
			    _lstGrade.Add(new ListItem(objGrade.Name, objGrade.DisplayName(GlobalOptions.Language)));
			}
            cboGrade.BeginUpdate();
			cboGrade.DataSource = null;
			cboGrade.ValueMember = "Value";
			cboGrade.DisplayMember = "Name";
			cboGrade.DataSource = _lstGrade;
            cboGrade.EndUpdate();
		}

		private void UpdateCustomDrugStats()
        {
            _objDrug = new Drug(_objCharacter)
            {
                Name = txtDrugName.Text,
                Category = "Custom Drug",
                Grade = cboGrade.SelectedValue.ToString()
            };

            foreach (clsNodeData objNodeData in _lstSelectedDrugComponents)
            {
                DrugComponent objDrugComponent = objNodeData.DrugComponent;
                objDrugComponent.Level = objNodeData.Level;
                _objDrug.Components.Add(objDrugComponent);
            }
        }

		private void AcceptForm()
		{
			_objDrug.Quantity = 1;
		}

		private void AddSelectedComponent()
        {
            if (!(treAvailableComponents.SelectedNode?.Tag is clsNodeData objNodeData) || objNodeData.Level == -1)
            {
                return;
            }

            string strCategory = objNodeData.DrugComponent.Category;
            TreeNode nodCategoryNode = treChosenComponents.FindNode("Node_" + strCategory);
            if (nodCategoryNode == null)
            {
                Log.Warning($"Unknown category {strCategory} in component {objNodeData.DrugComponent.Name}");
                return;
            }

            if (!string.IsNullOrEmpty(txtDrugName.Text))

            //prevent adding same component twice
            if (_lstSelectedDrugComponents.Any(c => c.DrugComponent.Name == objNodeData.DrugComponent.Name))
            {
                MessageBox.Show(this, LanguageManager.GetString("Message_DuplicateDrugComponentWarning"));
                return;
            }

            //drug can have only one foundation
            if (objNodeData.DrugComponent.Category == "Foundation")
            {
                if (_lstSelectedDrugComponents.Any(c => c.DrugComponent.Category == "Foundation"))
                {
                    MessageBox.Show(this, LanguageManager.GetString("Message_DuplicateDrugFoundationWarning"));
                    return;
                }
            }

            string strSpaceString = LanguageManager.GetString("String_Space");
            string strColonString = LanguageManager.GetString("String_Colon");
            //restriction for maximum level of block (CF 191)
            if (objNodeData.Level + 1 > 2)
            {
                foreach (clsNodeData objFoundationNodeData in _lstSelectedDrugComponents)
                {
                    if (objFoundationNodeData.DrugComponent.Category != "Foundation")
                        continue;
                    Dictionary<string, int> dctFoundationAttributes = objFoundationNodeData.DrugComponent.DrugEffects[0].Attributes;
                    Dictionary<string, int> dctBlockAttributes = objNodeData.DrugComponent.DrugEffects[objNodeData.Level].Attributes;
                    foreach (KeyValuePair<string, int> objItem in dctFoundationAttributes)
                    {
                        if (objItem.Value < 0 &&
                            dctBlockAttributes.TryGetValue(objItem.Key, out int intBlockAttrValue) &&
                            intBlockAttrValue > 0)
                        {
                            string message = new StringBuilder(LanguageManager.GetString("String_MaximumDrugBlockLevel")).
                                AppendLine().
                                Append(objFoundationNodeData.DrugComponent.CurrentDisplayName).Append(strColonString).Append(strSpaceString).Append(objItem.Key).Append(objItem.Value.ToString("+#;-#;")).AppendLine().
                                Append(objNodeData.DrugComponent.CurrentDisplayName).Append(strColonString).Append(strSpaceString).Append(objItem.Key).Append(intBlockAttrValue.ToString("+#;-#;")).
                                ToString();
                            MessageBox.Show(this, message);
                            return;
                        }
                    }
                }
            }


            string strNodeText = objNodeData.DrugComponent.CurrentDisplayName;
            if (objNodeData.DrugComponent.Level <= 0 && objNodeData.DrugComponent.DrugEffects.Count > 1)
                strNodeText += strSpaceString + '(' + LanguageManager.GetString("String_Level") + strSpaceString + (objNodeData.Level + 1).ToString(GlobalOptions.CultureInfo) + ")";
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
            if (treAvailableComponents.SelectedNode?.Tag is clsNodeData objNodeData)
            {
                lblBlockDescription.Text = objNodeData.DrugComponent.GenerateDescription(objNodeData.Level);
            }
        }

        private void treChoosenComponents_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (treChosenComponents.SelectedNode?.Tag is clsNodeData objNodeData)
            {
                lblBlockDescription.Text = objNodeData.DrugComponent.GenerateDescription(objNodeData.Level);
            }
        }

        private void btnAddComponent_Click(object sender, EventArgs e)
        {
            AddSelectedComponent();
        }

        private void treAvailableComponents_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            AddSelectedComponent();
        }

        private void btnRemoveComponent_Click(object sender, EventArgs e)
        {
            if (treChosenComponents.SelectedNode?.Tag is clsNodeData objNodeData)
            {
                treChosenComponents.Nodes.Remove(treChosenComponents.SelectedNode);

                _lstSelectedDrugComponents.Remove(objNodeData);

                UpdateCustomDrugStats();
                lblDrugDescription.Text = _objDrug.GenerateDescription(0);
            }
        }

        private void txtDrugName_TextChanged(object sender, EventArgs e)
        {
            _objDrug.Name = txtDrugName.Text;
            lblDrugDescription.Text = _objDrug.GenerateDescription(0);
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
	        AcceptForm();
            DialogResult = DialogResult.OK;
            Close();
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
			XmlNode objXmlGrade = _objXmlDocument.SelectSingleNode("/chummer/grades/grade[name = \"" + cboGrade.SelectedValue + "\"]");
		    if (!objXmlGrade.TryGetDoubleFieldQuickly("cost", ref _dblCostMultiplier))
		        _dblCostMultiplier = 1.0;
            if (!objXmlGrade.TryGetInt32FieldQuickly("addictionthreshold", ref _intAddictionThreshold))
		        _intAddictionThreshold = 0;
            UpdateCustomDrugStats();
			lblDrugDescription.Text = _objDrug.GenerateDescription(0);
		}
	}

	class clsNodeData : Object
    {
        public DrugComponent DrugComponent { get; }
        public int Level { get; }
        public clsNodeData(DrugComponent objDrugComponent, int level = -1)
        {
            DrugComponent = objDrugComponent;
            Level = level;
        }
    }
}
