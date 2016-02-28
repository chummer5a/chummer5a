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
﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace Chummer
{
    public partial class frmCreateCustomDrug : Form
    {
        private Dictionary<String, clsDrugComponent> dictDrugComponents;
        private List<clsNodeData> lstChoosenDrugComponents;
        private clsDrugComponent objCustomDrug;

        public frmCreateCustomDrug()
        {
            InitializeComponent();
            LoadData();

            lstChoosenDrugComponents = new List<clsNodeData>();

            foreach (var item in dictDrugComponents)
            {
                string category = item.Value.category;
                int categoryIndex = FindRootNodeIndexForCategory(category);
                if (categoryIndex == -1)
                {
                    Log.Warning(string.Format("Uknown category %s in component %s", category, item.Key));
                    return;
                }
                var node = treAvailableComponents.Nodes[categoryIndex].Nodes.Add(item.Key);
                int levelCount = item.Value.effects.Count;
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

            UpdateCustomDrugStats();
            lblDrugDescription.Text = objCustomDrug.GenerateDescription(0);
        }

        private void LoadData()
        {
            dictDrugComponents = new Dictionary<string, clsDrugComponent>();
            XmlDocument objXmlDocument = XmlManager.Instance.Load("drugcomponents.xml");
            foreach (XmlNode objXmlComponent in objXmlDocument.SelectNodes("chummer/drugcomponents/drugcomponent"))
            {
                clsDrugComponent objDrugComponent = new clsDrugComponent();
                objDrugComponent.Load(objXmlComponent);
                dictDrugComponents[objDrugComponent.name] = objDrugComponent;
            }
        }

        private void UpdateCustomDrugStats()
        {
            objCustomDrug = new clsDrugComponent();
            objCustomDrug.name = txtDrugName.Text;
            objCustomDrug.category = "Custom Drug";
            clsDrugEffect objCustomDrugEffect = new clsDrugEffect();
            objCustomDrug.effects.Add(objCustomDrugEffect);

            foreach (clsNodeData objNodeData in lstChoosenDrugComponents)
            {
                clsDrugComponent objDrugComponent = objNodeData.objDrugComponent;
                int level = objNodeData.level;
                clsDrugEffect objDrugEffect = objDrugComponent.effects[level];

                foreach (var item in objDrugEffect.attributes)
                {
                    int value;
                    objCustomDrugEffect.attributes.TryGetValue(item.Key, out value);
                    objCustomDrugEffect.attributes[item.Key] = value + item.Value;
                }

                foreach (var item in objDrugEffect.limits)
                {
                    int value;
                    objCustomDrugEffect.limits.TryGetValue(item.Key, out value);
                    objCustomDrugEffect.limits[item.Key] = value + item.Value;
                }

                foreach (string quality in objDrugEffect.qualities)
                {
                    if (!objCustomDrugEffect.qualities.Contains(quality))
                        objCustomDrugEffect.qualities.Add(quality);
                }

                foreach (string info in objDrugEffect.infos)
                {
                    objCustomDrugEffect.infos.Add(info);
                }

                objCustomDrugEffect.ini += objDrugEffect.ini;
                objCustomDrugEffect.iniDice += objDrugEffect.iniDice;
                objCustomDrugEffect.speed += objDrugEffect.speed;
                objCustomDrugEffect.duration += objDrugEffect.duration;
                objCustomDrugEffect.crashDamage += objDrugEffect.crashDamage;

                objCustomDrug.addictionRating += objDrugComponent.addictionRating;
                objCustomDrug.addictionThreshold += objDrugComponent.addictionThreshold;
                objCustomDrug.availability += objDrugComponent.availability;
                objCustomDrug.cost += objDrugComponent.cost;
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

        private void AddSelectedComponent()
        {
            clsNodeData objNodeData;
            if (treAvailableComponents.SelectedNode != null && treAvailableComponents.SelectedNode.Tag != null)
                objNodeData = (clsNodeData)treAvailableComponents.SelectedNode.Tag;
            else
                return;

            if (objNodeData.level == -1)
                return;

            int categoryIndex = FindRootNodeIndexForCategory(objNodeData.objDrugComponent.category);
            if (categoryIndex == -1)
            {
                Log.Warning(string.Format("Uknown category %s in component %s", objNodeData.objDrugComponent.category, objNodeData.objDrugComponent.name));
                return;
            }

            //prevent adding same component twice
            foreach (clsNodeData objChoosenDrugComponentData in lstChoosenDrugComponents)
            {
                if (objChoosenDrugComponentData.objDrugComponent.name == objNodeData.objDrugComponent.name)
                {
                    MessageBox.Show(this, "You cannot add the same component twice");
                    return;
                }
            }

            //drug can have only one foundation
            if (objNodeData.objDrugComponent.category == "Foundation")
            {
                foreach (clsNodeData objChoosenDrugComponentData in lstChoosenDrugComponents)
                {
                    if (objChoosenDrugComponentData.objDrugComponent.category == "Foundation")
                    {
                        MessageBox.Show(this, "Drug can only have one foundation");
                        return;
                    }
                }
            }

            //restriction for maximum level of block (CF 191)
            if (objNodeData.level + 1 > 2)
            {
                foreach (clsNodeData objFoundationNodeData in lstChoosenDrugComponents)
                {
                    if (objFoundationNodeData.objDrugComponent.category != "Foundation")
                        continue;
                    var dctFoundationAttributes = objFoundationNodeData.objDrugComponent.effects[0].attributes;
                    var dctBlockAttributes = objNodeData.objDrugComponent.effects[objNodeData.level].attributes;
                    foreach (var item in dctFoundationAttributes)
                    {
                        int blockAttrValue = 0;
                        if (item.Value < 0 &&
                            dctBlockAttributes.TryGetValue(item.Key, out blockAttrValue) &&
                            blockAttrValue > 0)
                        {
                            string message = new StringBuilder("The maximum level of a block that positively modifies an Attribute that the chosen foundation negatively modifies is Level 2. (CF 191)").
                                AppendLine().
                                Append(objFoundationNodeData.objDrugComponent.name).Append(": ").Append(item.Key).Append(item.Value.ToString("+#;-#;")).AppendLine().
                                Append(objNodeData.objDrugComponent.name).Append(": ").Append(item.Key).Append(blockAttrValue.ToString("+#;-#;")).
                                ToString();
                            MessageBox.Show(this, message);
                            return;
                        }
                    }
                }
            }


            string nodeText = objNodeData.objDrugComponent.name;
            if (objNodeData.objDrugComponent.effects.Count > 1)
                nodeText += " (level " + (objNodeData.level + 1).ToString() + ")";
            TreeNode node = treChoosenComponents.Nodes[categoryIndex].Nodes.Add(nodeText);
            node.Tag = objNodeData;
            node.EnsureVisible();

            lstChoosenDrugComponents.Add(objNodeData);
            UpdateCustomDrugStats();
            lblDrugDescription.Text = objCustomDrug.GenerateDescription(0);
        }

        public clsDrugComponent CustomDrug
        {
            get
            {
                return objCustomDrug;
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

            lstChoosenDrugComponents.Remove(objNodeData);

            UpdateCustomDrugStats();
            lblDrugDescription.Text = objCustomDrug.GenerateDescription(0);
        }

        private void txtDrugName_TextChanged(object sender, EventArgs e)
        {
            objCustomDrug.name = txtDrugName.Text;
            lblDrugDescription.Text = objCustomDrug.GenerateDescription(0);
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            objCustomDrug = null;
            DialogResult = DialogResult.Cancel;
            Close();
        }

    }

    class clsNodeData : Object
    {
        public clsDrugComponent objDrugComponent;
        public int level;
        public clsNodeData(clsDrugComponent objDrugComponent, int level = -1)
        {
            this.objDrugComponent = objDrugComponent;
            this.level = level;
        }
    }

    public class clsDrugEffect : Object
    {
        private Dictionary<string, int> _attributes;
        private Dictionary<string, int> _limits;
        private List<string> _qualities;
        private List<string> _infos;
        public int ini = 0;
        public int iniDice = 0;
        public int crashDamage = 0;
        public int speed = 0;
        public int duration = 0;

        public clsDrugEffect()
        {
            _attributes = new Dictionary<string, int>();
            _limits = new Dictionary<string, int>();
            _qualities = new List<string>();
            _infos = new List<string>();
        }

        public Dictionary<string, int> attributes
        {
            get
            {
                return _attributes;
            }
        }

        public Dictionary<string, int> limits
        {
            get
            {
                return _limits;
            }
        }

        public List<string> qualities
        {
            get
            {
                return _qualities;
            }
        }

        public List<string> infos
        {
            get
            {
                return _infos;
            }
        }
    }

    public class clsDrugComponent : Object
    {
        public string name;
        public string category;
        private List<clsDrugEffect> _effects;
        public int availability = 0;
        public int cost = 0;
        public int addictionRating = 0;
        public int addictionThreshold = 0;
        public string source;
        public int page = 0;

        public clsDrugComponent()
        {
            _effects = new List<clsDrugEffect>();
        }

        public List<clsDrugEffect> effects
        {
            get
            {
                return _effects;
            }
        }

        public void Load(XmlNode objXmlData)
        {
            objXmlData.TryGetField("name", out name);
            objXmlData.TryGetField("category", out category);
            foreach (XmlNode objXmlLevel in objXmlData.SelectNodes("effects/level"))
            {
                clsDrugEffect objDrugEffect = new clsDrugEffect();
                foreach (XmlNode objXmlEffect in objXmlLevel.SelectNodes("*"))
                {
                    string effectName;
                    int effectValue;
                    objXmlEffect.TryGetField("name", out effectName, null);
                    objXmlEffect.TryGetField("value", out effectValue, 1);
                    switch (objXmlEffect.Name)
                    {
                        case "attribute":
                            if (effectName != null)
                                objDrugEffect.attributes[effectName] = effectValue;
                            break;
                        case "limit":
                            if (effectName != null)
                                objDrugEffect.limits[effectName] = effectValue;
                            break;
                        case "quality":
                            objDrugEffect.qualities.Add(objXmlEffect.InnerText);
                            break;
                        case "info":
                            objDrugEffect.infos.Add(objXmlEffect.InnerText);
                            break;
                        case "initiative":
                            objDrugEffect.ini = int.Parse(objXmlEffect.InnerText);
                            break;
                        case "initiativedice":
                            objDrugEffect.iniDice = int.Parse(objXmlEffect.InnerText);
                            break;
                        case "crashdamage":
                            objDrugEffect.crashDamage = int.Parse(objXmlEffect.InnerText);
                            break;
                        case "speed":
                            objDrugEffect.speed = int.Parse(objXmlEffect.InnerText);
                            break;
                        case "duration":
                            objDrugEffect.duration = int.Parse(objXmlEffect.InnerText);
                            break;
                        default:
                            Log.Warning(info: string.Format("Unknown drug effect %s in component %s", objXmlEffect.Name, effectName));
                            break;
                    }
                }
                effects.Add(objDrugEffect);
            }
            objXmlData.TryGetField("availability", out availability);
            objXmlData.TryGetField("cost", out cost);
            objXmlData.TryGetField("rating", out addictionRating);
            objXmlData.TryGetField("threshold", out addictionThreshold);
            objXmlData.TryGetField("source", out source);
            objXmlData.TryGetField("page", out page);
        }

        public void Save(XmlWriter objXmlWriter)
        {
            objXmlWriter.WriteElementString("name", name);
            objXmlWriter.WriteElementString("category", category);

            objXmlWriter.WriteStartElement("effects");
            foreach (var objDrugEffect in effects)
            {
                objXmlWriter.WriteStartElement("level");
                foreach (var objAttribute in objDrugEffect.attributes)
                {
                    objXmlWriter.WriteStartElement("attribute");
                    objXmlWriter.WriteElementString("name", objAttribute.Key);
                    objXmlWriter.WriteElementString("value", objAttribute.Value.ToString());
                    objXmlWriter.WriteEndAttribute();
                }
                foreach (var objLimit in objDrugEffect.limits)
                {
                    objXmlWriter.WriteStartElement("limit");
                    objXmlWriter.WriteElementString("name", objLimit.Key);
                    objXmlWriter.WriteElementString("value", objLimit.Value.ToString());
                    objXmlWriter.WriteEndElement();
                }
                foreach (string quality in objDrugEffect.qualities)
                {
                    objXmlWriter.WriteElementString("quality", quality);
                }
                foreach (string info in objDrugEffect.infos)
                {
                    objXmlWriter.WriteElementString("info", info);
                }
                if (objDrugEffect.ini != 0)
                    objXmlWriter.WriteElementString("initiative", objDrugEffect.ini.ToString());
                if (objDrugEffect.iniDice != 0)
                    objXmlWriter.WriteElementString("initiativedice", objDrugEffect.iniDice.ToString());
                if (objDrugEffect.duration != 0)
                    objXmlWriter.WriteElementString("duration", objDrugEffect.duration.ToString());
                if (objDrugEffect.speed != 0)
                    objXmlWriter.WriteElementString("speed", objDrugEffect.speed.ToString());
                if (objDrugEffect.crashDamage != 0)
                    objXmlWriter.WriteElementString("crashdamage", objDrugEffect.crashDamage.ToString());
                objXmlWriter.WriteEndElement();
            }
            objXmlWriter.WriteEndElement();

            if (availability != 0)
                objXmlWriter.WriteElementString("availability", availability.ToString());
            if (cost != 0)
                objXmlWriter.WriteElementString("cost", cost.ToString());
            if (addictionRating != 0)
                objXmlWriter.WriteElementString("rating", addictionRating.ToString());
            if (addictionThreshold != 0)
                objXmlWriter.WriteElementString("threshold", addictionThreshold.ToString());
            if (source != null)
                objXmlWriter.WriteElementString("source", source);
            if (page != 0)
                objXmlWriter.WriteElementString("page", page.ToString());
        }

        public String GenerateDescription(int level = -1)
        {
            if (level >= effects.Count)
                return null;

            StringBuilder description = new StringBuilder();
            bool newLineFlag = false;

            description.Append(category).Append(": ").Append(name).AppendLine();

            if (level != -1)
            {
                var objDrugEffect = effects.ElementAt(level);

                foreach (var objAttribute in objDrugEffect.attributes)
                {
                    if (objAttribute.Value != 0)
                    {
                        description.Append(objAttribute.Key).Append(objAttribute.Value.ToString("+#;-#")).Append("; ");
                        newLineFlag = true;
                    }
                }
                if (newLineFlag)
                {
                    newLineFlag = false;
                    description.AppendLine();
                }

                foreach (var objLimit in objDrugEffect.limits)
                {
                    if (objLimit.Value != 0)
                    {
                        description.Append(objLimit.Key).Append(" limit ").Append(objLimit.Value.ToString("+#;-#")).Append("; ");
                        newLineFlag = true;
                    }
                }
                if (newLineFlag)
                {
                    newLineFlag = false;
                    description.AppendLine();
                }

                if (objDrugEffect.ini != 0 || objDrugEffect.iniDice != 0)
                {
                    description.Append("Initiative ");
                    if (objDrugEffect.ini != 0)
                        description.Append(objDrugEffect.ini.ToString("+#;-#"));
                    if (objDrugEffect.iniDice != 0)
                        description.Append(objDrugEffect.iniDice.ToString("+#;-#"));
                    description.AppendLine();
                }

                foreach (string quality in objDrugEffect.qualities)
                    description.Append(quality).Append(" quality").AppendLine();
                foreach (string info in objDrugEffect.infos)
                    description.Append(info).AppendLine();

                if (category == "Custom Drug" || objDrugEffect.duration != 0)
                    description.Append("Duration: 10 x ").Append(objDrugEffect.duration + 1).Append("d6 minutes").AppendLine();

                if (category == "Custom Drug" || objDrugEffect.speed != 0)
                {
                    if (3 - objDrugEffect.speed == 0)
                        description.Append("Speed: Immediate").AppendLine();
                    else
                        description.Append("Speed: ").Append(3 - objDrugEffect.speed).Append(" combat turns").AppendLine();
                }

                if (objDrugEffect.crashDamage != 0)
                    description.Append("Crash Effect: ").Append(objDrugEffect.crashDamage).Append("S damage, unresisted").AppendLine();

                description.Append("Addiction rating: ").Append(addictionRating * (level + 1)).AppendLine();
                description.Append("Addiction threshold: ").Append(addictionThreshold * (level + 1)).AppendLine();
                description.Append("Cost: ").Append(cost * (level + 1)).Append("¥").AppendLine();
                description.Append("Availability: ").Append(availability * (level + 1)).AppendLine();
            }
            else
            {
                description.Append("Addiction rating: ").Append(addictionRating).Append(" per level").AppendLine();
                description.Append("Addiction threshold: ").Append(addictionThreshold).Append(" per level").AppendLine();
                description.Append("Cost: ").Append(cost).Append("¥ per level").AppendLine();
                description.Append("Availability: ").Append(availability).Append(" per level").AppendLine();
            }

            return description.ToString();
        }
    }
}
