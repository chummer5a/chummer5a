using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using Chummer.Backend.Equipment;

namespace Chummer
{
    public partial class frmCreateCustomDrug : Form
	{
		private Dictionary<String, DrugComponent> dictDrugComponents;
        private readonly List<clsNodeData> lstSelectedDrugComponents;
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
				objDrug.GUID = new Guid();
	        }
	        _objCharacter = objCharacter;
            InitializeComponent();
            LoadData();

            lstSelectedDrugComponents = new List<clsNodeData>();

            foreach (var item in dictDrugComponents)
            {
                string category = item.Value.Category;
                int categoryIndex = FindRootNodeIndexForCategory(category);
                if (categoryIndex == -1)
                {
                    Log.Warning(string.Format("Uknown category %s in component %s", category, item.Key));
                    return;
                }
                var node = treAvailableComponents.Nodes[categoryIndex].Nodes.Add(item.Key);
                int levelCount = item.Value.DrugEffects.Count;
                if (levelCount == 1)
                {
                    node.Tag = new clsNodeData(item.Value, 0);
                }
                else
                {
                    node.Tag = new clsNodeData(item.Value);
                    for (int i = 0; i < levelCount; i++)
                    {
                        var subNode = node.Nodes.Add("Level " + (i + 1).ToString());
                        subNode.Tag = new clsNodeData(item.Value, i);
                    }
                }
            }
            treAvailableComponents.ExpandAll();
            treChoosenComponents.ExpandAll();
	        PopulateGrades();
			UpdateCustomDrugStats();
            lblDrugDescription.Text = objDrug.Description;
        }

        private void LoadData()
        {
            dictDrugComponents = new Dictionary<string, DrugComponent>();
            foreach (XmlNode objXmlComponent in _objXmlDocument.SelectNodes("chummer/drugcomponents/drugcomponent"))
            {
                DrugComponent objDrugComponent = new DrugComponent();
                objDrugComponent.Load(objXmlComponent);
                dictDrugComponents[objDrugComponent.Name] = objDrugComponent;
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
			cboGrade.DataSource = null;
			cboGrade.ValueMember = "Value";
			cboGrade.DisplayMember = "Name";
			cboGrade.DataSource = _lstGrade;
		}

		private void UpdateCustomDrugStats()
        {
            _objDrug = new Drug(_objCharacter)
            {
                Name = txtDrugName.Text,
                Category = "Custom Drug",
                Grade = cboGrade.SelectedValue.ToString()
            };

            foreach (clsNodeData objNodeData in lstSelectedDrugComponents)
            {
                DrugComponent objDrugComponent = objNodeData.objDrugComponent;
                objDrugComponent.Level = objNodeData.level;
                _objDrug.Components.Add(objDrugComponent);
            }
        }

        private int FindRootNodeIndexForCategory(string category)
        {
            switch (category)
            {
                case "Foundation":
                    return 0;
                case "Block":
                    return 1;
                case "Enhancer":
                    return 2;
                default:
                    return -1;
            }
		}

		private void AcceptForm()
		{
			_objDrug.Name = txtDrugName.Text;
			_objDrug.Quantity = 1;
		}

		private void AddSelectedComponent()
        {
            clsNodeData objNodeData;
            if (treAvailableComponents.SelectedNode?.Tag != null)
                objNodeData = (clsNodeData)treAvailableComponents.SelectedNode.Tag;
            else
                return;

            if (objNodeData.level == -1)
                return;

            int categoryIndex = FindRootNodeIndexForCategory(objNodeData.objDrugComponent.Category);
            if (categoryIndex == -1)
            {
                Log.Warning(string.Format("Uknown category %s in component %s", objNodeData.objDrugComponent.Category, objNodeData.objDrugComponent.Name));
                return;
            }

            //prevent adding same component twice
            if (lstSelectedDrugComponents.Any(c => c.objDrugComponent.Name == objNodeData.objDrugComponent.Name))
            {
                MessageBox.Show(this, LanguageManager.GetString("Message_DuplicateDrugComponentWarning"));
                return;
            }

            //drug can have only one foundation
            if (objNodeData.objDrugComponent.Category == "Foundation")
            {
                if (lstSelectedDrugComponents.Any(c => c.objDrugComponent.Category == "Foundation"))
                {
                    MessageBox.Show(this, LanguageManager.GetString("Message_DuplicateDrugFoundationWarning"));
                    return;
                }
            }

            //restriction for maximum level of block (CF 191)
            if (objNodeData.level + 1 > 2)
            {
                foreach (clsNodeData objFoundationNodeData in lstSelectedDrugComponents)
                {
                    if (objFoundationNodeData.objDrugComponent.Category != "Foundation")
                        continue;
                    var dctFoundationAttributes = objFoundationNodeData.objDrugComponent.DrugEffects[0].Attributes;
                    var dctBlockAttributes = objNodeData.objDrugComponent.DrugEffects[objNodeData.level].Attributes;
                    foreach (var item in dctFoundationAttributes)
                    {
                        if (item.Value < 0 &&
                            dctBlockAttributes.TryGetValue(item.Key, out var blockAttrValue) &&
                            blockAttrValue > 0)
                        {
                            string message = new StringBuilder("The maximum level of a block that positively modifies an Attribute that the chosen foundation negatively modifies is Level 2. (CF 191)").
                                AppendLine().
                                Append(objFoundationNodeData.objDrugComponent.Name).Append(": ").Append(item.Key).Append(item.Value.ToString("+#;-#;")).AppendLine().
                                Append(objNodeData.objDrugComponent.Name).Append(": ").Append(item.Key).Append(blockAttrValue.ToString("+#;-#;")).
                                ToString();
                            MessageBox.Show(this, message);
                            return;
                        }
                    }
                }
            }


            string nodeText = objNodeData.objDrugComponent.Name;
            if (objNodeData.objDrugComponent.DrugEffects.Count > 1)
                nodeText += " (level " + (objNodeData.level + 1).ToString() + ")";
            TreeNode node = treChoosenComponents.Nodes[categoryIndex].Nodes.Add(nodeText);
            node.Tag = objNodeData;
            node.EnsureVisible();

            lstSelectedDrugComponents.Add(objNodeData);
            UpdateCustomDrugStats();
            lblDrugDescription.Text = _objDrug.GenerateDescription(0);
        }

        public Drug CustomDrug
        {
            get
            {
                return _objDrug;
            }
        }

        private void treAvailableComponents_AfterSelect(object sender, TreeViewEventArgs e)
        {
            clsNodeData objNodeData;
            if (treAvailableComponents.SelectedNode != null && treAvailableComponents.SelectedNode.Tag != null)
                objNodeData = (clsNodeData)treAvailableComponents.SelectedNode.Tag;
            else
                return;

            lblBlockDescription.Text = objNodeData.objDrugComponent.GenerateDescription(objNodeData.level);
        }

        private void treChoosenComponents_AfterSelect(object sender, TreeViewEventArgs e)
        {
            clsNodeData objNodeData;
            if (treChoosenComponents.SelectedNode != null && treChoosenComponents.SelectedNode.Tag != null)
                objNodeData = (clsNodeData)treChoosenComponents.SelectedNode.Tag;
            else
                return;

            lblBlockDescription.Text = objNodeData.objDrugComponent.GenerateDescription(objNodeData.level);
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
            clsNodeData objNodeData;
            if (treChoosenComponents.SelectedNode != null && treChoosenComponents.SelectedNode.Tag != null)
                objNodeData = (clsNodeData)treChoosenComponents.SelectedNode.Tag;
            else
                return;

            treChoosenComponents.Nodes.Remove(treChoosenComponents.SelectedNode);

            lstSelectedDrugComponents.Remove(objNodeData);

            UpdateCustomDrugStats();
            lblDrugDescription.Text = _objDrug.GenerateDescription(0);
        }

        private void txtDrugName_TextChanged(object sender, EventArgs e)
        {
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
			_dblCostMultiplier = Convert.ToDouble(objXmlGrade["cost"].InnerText, GlobalOptions.CultureInfo);
			objXmlGrade.TryGetField("addictionthreshold", out _intAddictionThreshold, 0);
			UpdateCustomDrugStats();
			lblDrugDescription.Text = _objDrug.GenerateDescription(0);
		}
	}

	class clsNodeData : Object
    {
        public DrugComponent objDrugComponent;
        public int level;
        public clsNodeData(DrugComponent objDrugComponent, int level = -1)
        {
            this.objDrugComponent = objDrugComponent;
            this.level = level;
        }
    }
}
