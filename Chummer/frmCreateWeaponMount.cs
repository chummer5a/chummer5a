using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using Chummer.Backend.Equipment;

namespace Chummer
{
	public partial class frmCreateWeaponMount : Form
	{
		private readonly List<object> _lstVisibility;
		private readonly List<object> _lstFlexibility;
		private readonly List<object> _lstControl;
		private readonly List<object> _lstSize;
		private bool _loading = true;
	    private readonly Vehicle _vehicle;
	    private readonly Character _objCharacter;
		private XmlDocument _xmlDoc;

        public WeaponMount WeaponMount { get; internal set; }

        public frmCreateWeaponMount(Vehicle vehicle, Character character)
		{
			_lstControl = new List<object>();
			_lstFlexibility = new List<object>();
			_lstSize = new List<object>();
			_lstVisibility = new List<object>();
		    _vehicle = vehicle;
		    _objCharacter = character;
			InitializeComponent();
		}

        private void frmCreateWeaponMount_Load(object sender, EventArgs e)
        {
            _xmlDoc = XmlManager.Load("vehicles.xml");

            // Populate the Armor Category list.
            XmlNodeList nodeList = _xmlDoc.SelectNodes("/chummer/weaponmounts/weaponmount");
            
            if (nodeList != null)
                foreach (XmlNode node in nodeList)
                {
                    bool add = !(node["optionaldrone"] != null && !_vehicle.IsDrone);
                    if (add)
                    {
                        ListItem objItem = new ListItem(node["id"].InnerText, node.Attributes?["translate"]?.InnerText ?? node["name"].InnerText);
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
            nudMarkup.Visible = AllowDiscounts;
            lblMarkupLabel.Visible = AllowDiscounts;
            lblMarkupPercentLabel.Visible = AllowDiscounts;
            _loading = false;
            comboBox_SelectedIndexChanged(null, null);
        }

		private void cmdOK_Click(object sender, EventArgs e)
		{
            TreeNode tree = new TreeNode();
            //TODO: THIS IS UGLY AS SHIT, FIX BETTER
            XmlNode node = _xmlDoc.SelectSingleNode("/chummer/weaponmounts/weaponmount[id = \"" + cboSize.SelectedValue + "\"]");
            if (node["forbidden"] != null)
            {
                XmlNodeList list = node.SelectNodes("/forbidden/control");
                XmlNode check = _xmlDoc.SelectSingleNode("/chummer/weaponmounts/weaponmount[id = \"" + cboControl.SelectedValue + "\"]");
                if (list.Cast<XmlNode>().Any(n => n.InnerText == check["name"].InnerText))
                    return;
                list = node.SelectNodes("/forbidden/flexibility");
                check = _xmlDoc.SelectSingleNode("/chummer/weaponmounts/weaponmount[id = \"" + cboFlexibility.SelectedValue + "\"]");
                if (list.Cast<XmlNode>().Any(n => n.InnerText == check["name"].InnerText))
                    return;
                list = node.SelectNodes("/forbidden/visibility");
                check = _xmlDoc.SelectSingleNode("/chummer/weaponmounts/weaponmount[id = \"" + cboVisibility.SelectedValue + "\"]");
                if (list.Cast<XmlNode>().Any(n => n.InnerText == check["name"].InnerText))
                    return;
            }
            if (node["required"] != null)
            {
                bool requirementsMet = true;
                XmlNodeList list = node.SelectNodes("/required/control");
                XmlNode check = _xmlDoc.SelectSingleNode("/chummer/weaponmounts/weaponmount[id = \"" + cboControl.SelectedValue + "\"]");
                if (list.Count > 0)
                {
                    requirementsMet = requirementsMet && list.Cast<XmlNode>().Any(n => n.InnerText == check["name"].InnerText);
                    if (!requirementsMet)
                        return;
                }
                list = node.SelectNodes("/required/flexibility");
                if (list.Count > 0)
                {
                    check = _xmlDoc.SelectSingleNode("/chummer/weaponmounts/weaponmount[id = \"" + cboFlexibility.SelectedValue + "\"]");
                    requirementsMet = requirementsMet && list.Cast<XmlNode>().Any(n => n.InnerText == check["name"].InnerText);
                    if (!requirementsMet)
                        return;
                }
                list = node.SelectNodes("/required/visibility");
                if (list.Count > 0)
                {
                    check = _xmlDoc.SelectSingleNode("/chummer/weaponmounts/weaponmount[id = \"" + cboVisibility.SelectedValue + "\"]");
                    requirementsMet = requirementsMet && list.Cast<XmlNode>().Any(n => n.InnerText == check["name"].InnerText);
                    if (!requirementsMet)
                        return;
                }
            }
            WeaponMount mount = new WeaponMount(_objCharacter, _vehicle);
            mount.Create(node, tree, _vehicle);
            WeaponMountOption option = new WeaponMountOption(_objCharacter);
            option.Create(cboControl.SelectedValue.ToString(), mount.WeaponMountOptions);
            option = new WeaponMountOption(_objCharacter);
            option.Create(cboFlexibility.SelectedValue.ToString(), mount.WeaponMountOptions);
            option = new WeaponMountOption(_objCharacter);
            option.Create(cboVisibility.SelectedValue.ToString(), mount.WeaponMountOptions);
            WeaponMount = mount;
            tree.Text = mount.DisplayName;
            DialogResult = DialogResult.OK;
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void comboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateInfo();
        }

        private void chkFreeItem_CheckedChanged(object sender, EventArgs e)
        {
            UpdateInfo();
        }

	    public bool FreeCost => chkFreeItem.Checked;

	    public decimal Markup => nudMarkup.Value;

	    public bool AllowDiscounts { get; set; } = false;
        private void nudMarkup_ValueChanged(object sender, EventArgs e)
        {
            UpdateInfo();
        }

	    private void UpdateInfo()
	    {
	        if (_loading) return;
	        XmlNode node = _xmlDoc.SelectSingleNode("/chummer/weaponmounts/weaponmount[id = \"" + cboSize.SelectedValue + "\"]");
	        decimal cost = 0;
	        if (!chkFreeItem.Checked) cost = Convert.ToInt32(node["cost"].InnerText);
	        int avail = 0;
	        string availSuffix = string.Empty;
	        int slots = Convert.ToInt32(node["slots"].InnerText);

	        if (node["avail"].InnerText.EndsWith('F') || node["avail"].InnerText.EndsWith('R'))
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
	            if (!chkFreeItem.Checked) cost += Convert.ToInt32(node["cost"].InnerText);
	            slots += Convert.ToInt32(node["slots"].InnerText);
	        }
	        cost *= 1 + (nudMarkup.Value / 100.0m);
	        lblCost.Text = cost.ToString(_objCharacter.Options.NuyenFormat, GlobalOptions.CultureInfo) + '¥';
	        lblSlots.Text = slots.ToString();
	        lblAvailability.Text = $"{avail}{availSuffix}";
        }
    }
}
