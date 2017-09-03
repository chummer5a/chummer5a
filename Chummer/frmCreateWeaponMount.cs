using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Xml;

namespace Chummer
{
    public partial class frmCreateWeaponMount : Form
    {
        private readonly List<object> _lstVisibility;
        private readonly List<object> _lstFlexibility;
        private readonly List<object> _lstControl;
        private readonly List<object> _lstSize;
        private bool _loading = true;
        private XmlDocument _xmlDoc;

        public frmCreateWeaponMount()
        {
            _lstControl = new List<object>();
            _lstFlexibility = new List<object>();
            _lstSize = new List<object>();
            _lstVisibility = new List<object>();
            InitializeComponent();
        }

        private void frmCreateWeaponMount_Load(object sender, EventArgs e)
        {
            _xmlDoc = XmlManager.Instance.Load("vehicles.xml");

            // Populate the Armor Category list.
            XmlNodeList nodeList = _xmlDoc.SelectNodes("/chummer/weaponmounts/weaponmount");
            if (nodeList != null)
                foreach (XmlNode node in nodeList)
                {
                    ListItem objItem = new ListItem();
                    objItem.Value = node["id"].InnerText;
                    objItem.Name = node.Attributes?["translate"]?.InnerText ?? node["name"].InnerText;
                    switch (node["category"].InnerText)
                    {
                        case "Visibility":
                            _lstVisibility.Add(objItem);
                            break;
                        case "Flexibility":
                            _lstFlexibility.Add(objItem);
                            break;
                        case "Control":
                            _lstControl.Add(objItem);
                            break;
                        case "Size":
                            _lstSize.Add(objItem);
                            break;
                        default:
                            Utils.BreakIfDebug();
                            break;
                    }


                }
            cboSize.BeginUpdate();
            cboSize.ValueMember = "Value";
            cboSize.DisplayMember = "Name";
            cboSize.DataSource = _lstSize;
            cboSize.EndUpdate();

            cboVisibility.BeginUpdate();
            cboVisibility.ValueMember = "Value";
            cboVisibility.DisplayMember = "Name";
            cboVisibility.DataSource = _lstVisibility;
            cboVisibility.EndUpdate();

            cboFlexibility.BeginUpdate();
            cboFlexibility.ValueMember = "Value";
            cboFlexibility.DisplayMember = "Name";
            cboFlexibility.DataSource = _lstFlexibility;
            cboFlexibility.EndUpdate();

            cboControl.BeginUpdate();
            cboControl.ValueMember = "Value";
            cboControl.DisplayMember = "Name";
            cboControl.DataSource = _lstControl;
            cboControl.EndUpdate();
            _loading = false;
            comboBox_SelectedIndexChanged(null, null);
        }

        private void cmdOK_Click(object sender, EventArgs e)
        {

        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void comboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_loading) return;
            XmlNode node = _xmlDoc.SelectSingleNode("/chummer/weaponmounts/weaponmount[id = \"" + cboSize.SelectedValue + "\"]");
            int cost = Convert.ToInt32(node["cost"].InnerText);
            int avail = 0;
            string availSuffix = string.Empty;
            int slots = Convert.ToInt32(node["slots"].InnerText);

            if (node["avail"].InnerText.EndsWith("F") || node["avail"].InnerText.EndsWith("R"))
            {
                availSuffix = node["avail"].InnerText.Substring(node["avail"].InnerText.Length - 1, 1);
                avail = Convert.ToInt32(node["avail"].InnerText.Substring(0, node["avail"].InnerText.Length - 1));
            }
            List<object> boxes = new List<object>
            {
                cboVisibility.SelectedValue,
                cboFlexibility.SelectedValue,
                cboControl.SelectedValue
            };
            foreach (object box in boxes)
            {
                node = _xmlDoc.SelectSingleNode("/chummer/weaponmounts/weaponmount[id = \"" + box + "\"]");
                if (node == null) continue;
                avail += Convert.ToInt32(node["avail"].InnerText);
                cost += Convert.ToInt32(node["cost"].InnerText);
                slots += Convert.ToInt32(node["slots"].InnerText);
            }

            lblCost.Text = $"{cost:###,###,##0¥}";
            lblSlots.Text = slots.ToString();
            lblAvailability.Text = $"{avail}{availSuffix}";
        }
    }
}
