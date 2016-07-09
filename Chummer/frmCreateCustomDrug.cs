using System;
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
        private List<clsNodeData> lstSelectedDrugComponents;
		private List<ListItem> _lstGrade = new List<ListItem>();
		private readonly Character _objCharacter;
	    private Drug objDrug = new Drug();
		XmlDocument _objXmlDocument = XmlManager.Instance.Load("drugcomponents.xml");
		private double _dblCostMultiplier;
		private int _intAddictionThreshold;

		public frmCreateCustomDrug(Character objCharacter, Drug objDrug = null)
        {
	        if (objDrug == null)
	        {
				objDrug = new Drug();
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
            lblDrugDescription.Text = objDrug.GenerateDescription(0);
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
			GradeList objGradeList;
			objGradeList = GlobalOptions.DrugGrades;

			_lstGrade.Clear();
			foreach (Grade objGrade in objGradeList)
			{
				ListItem objItem = new ListItem();
				objItem.Value = objGrade.Name;
				objItem.Name = objGrade.DisplayName;
				_lstGrade.Add(objItem);
			}
			cboGrade.DataSource = null;
			cboGrade.ValueMember = "Value";
			cboGrade.DisplayMember = "Name";
			cboGrade.DataSource = _lstGrade;
		}

		private void UpdateCustomDrugStats()
        {
			objDrug = new Drug();
            objDrug.Name = txtDrugName.Text;
			objDrug.Category = "Custom Drug";
			DrugEffect objDrugEffect = new DrugEffect();
			objDrug.Effects.Add(objDrugEffect);
			objDrug.Grade = cboGrade.SelectedValue.ToString();

            foreach (clsNodeData objNodeData in lstSelectedDrugComponents)
            {
                DrugComponent objDrugComponent = objNodeData.objDrugComponent;
				objDrug.Components.Add(objDrugComponent);
                objDrugComponent.Level = objNodeData.level;
                objDrugEffect = objDrugComponent.DrugEffects[objDrugComponent.Level];

                foreach (var item in objDrugEffect.attributes.ToList())
                {
                    int value;
                    objDrugEffect.attributes.TryGetValue(item.Key, out value);
                    objDrugEffect.attributes[item.Key] = value + item.Value;
                }

                foreach (var item in objDrugEffect.limits.ToList())
				{
                    int value;
                    objDrugEffect.limits.TryGetValue(item.Key, out value);
                    objDrugEffect.limits[item.Key] = value + item.Value;
                }

                foreach (string quality in objDrugEffect.qualities.ToList())
				{
                    if (!objDrugEffect.qualities.Contains(quality))
                        objDrugEffect.qualities.Add(quality);
                }

                foreach (string info in objDrugEffect.infos.ToList())
				{
                    objDrugEffect.infos.Add(info);
                }

                objDrugEffect.ini += objDrugEffect.ini;
                objDrugEffect.iniDice += objDrugEffect.iniDice;
                objDrugEffect.speed += objDrugEffect.speed;
                objDrugEffect.duration += objDrugEffect.duration;
                objDrugEffect.crashDamage += objDrugEffect.crashDamage;

                objDrug.AddictionRating += objDrugComponent.AddictionRating;
				objDrug.AddictionThreshold += objDrugComponent.AddictionThreshold;
				objDrug.Availability += objDrugComponent.Availability;
				objDrug.Cost += objDrugComponent.Cost;
            }
			objDrug.Cost = Convert.ToInt32((Convert.ToDouble(objDrug.Cost, GlobalOptions.Instance.CultureInfo) * _dblCostMultiplier));
			objDrug.AddictionThreshold += _intAddictionThreshold;
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
			objDrug.Name = txtDrugName.Text;
			objDrug.Quantity = 1;
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
            foreach (clsNodeData objSelectedDrugComponentData in lstSelectedDrugComponents)
            {
                if (objSelectedDrugComponentData.objDrugComponent.Name == objNodeData.objDrugComponent.Name)
                {
                    MessageBox.Show(this, "You cannot add the same component twice");
                    return;
                }
            }

            //drug can have only one foundation
            if (objNodeData.objDrugComponent.Category == "Foundation")
            {
                foreach (clsNodeData objSelectedDrugComponentData in lstSelectedDrugComponents)
                {
                    if (objSelectedDrugComponentData.objDrugComponent.Category == "Foundation")
                    {
                        MessageBox.Show(this, "Drug can only have one foundation");
                        return;
                    }
                }
            }

            //restriction for maximum level of block (CF 191)
            if (objNodeData.level + 1 > 2)
            {
                foreach (clsNodeData objFoundationNodeData in lstSelectedDrugComponents)
                {
                    if (objFoundationNodeData.objDrugComponent.Category != "Foundation")
                        continue;
                    var dctFoundationAttributes = objFoundationNodeData.objDrugComponent.DrugEffects[0].attributes;
                    var dctBlockAttributes = objNodeData.objDrugComponent.DrugEffects[objNodeData.level].attributes;
                    foreach (var item in dctFoundationAttributes)
                    {
                        int blockAttrValue = 0;
                        if (item.Value < 0 &&
                            dctBlockAttributes.TryGetValue(item.Key, out blockAttrValue) &&
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
            lblDrugDescription.Text = objDrug.GenerateDescription(0);
        }

        public Drug CustomDrug
        {
            get
            {
                return objDrug;
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
            lblDrugDescription.Text = objDrug.GenerateDescription(0);
        }

        private void txtDrugName_TextChanged(object sender, EventArgs e)
        {
            lblDrugDescription.Text = objDrug.GenerateDescription(0);
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
	        AcceptForm();
            DialogResult = DialogResult.OK;
            Close();
        }

		private void btnCancel_Click(object sender, EventArgs e)
        {
            objDrug = null;
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
			_dblCostMultiplier = Convert.ToDouble(objXmlGrade["cost"].InnerText, GlobalOptions.Instance.CultureInfo);
			objXmlGrade.TryGetField("addictionthreshold", out _intAddictionThreshold, 0);
			UpdateCustomDrugStats();
			lblDrugDescription.Text = objDrug.GenerateDescription(0);
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
