using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;
using Chummer.Skills;

namespace Chummer
{
    public partial class frmSelectSpec : Form
    {
        private readonly Skill _objSkill;
        private string _strForceItem = string.Empty;
        private XmlDocument _objXmlDocument = new XmlDocument();

        #region Control Events
        public frmSelectSpec(Skill skill)
        {
            _objSkill = skill;
            InitializeComponent();
            LanguageManager.Instance.Load(GlobalOptions.Instance.Language, this);
            MoveControls();
        }

        private void frmSelectSpec_Load(object sender, EventArgs e)
        {
            List<ListItem> lstItems = new List<ListItem>();

            _objXmlDocument = XmlManager.Instance.Load("skills.xml");

            if (_objSkill.CharacterObject.BuildMethod == CharacterBuildMethod.Karma)
            {
                chkKarma.Checked = true;
                chkKarma.Visible = false;
            }
            XmlNode objXmlSkill = _objXmlDocument.SelectSingleNode("/chummer/skills/skill[name = \"" + _objSkill.Name + "\"]");
            if (Mode == "Knowledge")
            {
                objXmlSkill = _objXmlDocument.SelectSingleNode("/chummer/knowledgeskills/skill[name = \"" + _objSkill.Name + "\"]");
                if (objXmlSkill == null)
                {
                    objXmlSkill = _objXmlDocument.SelectSingleNode("/chummer/knowledgeskills/skill[translate = \"" + _objSkill.Name + "\"]");
                }
            }
            // Populate the Skill's Specializations (if any).
            ListItem objItem = new ListItem();
            objItem.Value = "Custom";
            objItem.Name = string.Empty;
            lstItems.Add(objItem);
            foreach (XmlNode objXmlSpecialization in objXmlSkill.SelectNodes("specs/spec"))
            {
                objItem = new ListItem();
                objItem.Value = objXmlSpecialization.InnerText;
                objItem.Name = objXmlSpecialization.Attributes["translate"]?.InnerText ?? objXmlSpecialization.InnerText;
                lstItems.Add(objItem);

                if (_objSkill.SkillCategory == "Combat Active")
                {
                    // Look through the Weapons file and grab the names of items that are part of the appropriate Category or use the matching Skill.
                    XmlDocument objXmlWeaponDocument = XmlManager.Instance.Load("weapons.xml");
                    //Might need to include skill name or might miss some values?
                    XmlNodeList objXmlWeaponList = objXmlWeaponDocument.SelectNodes("/chummer/weapons/weapon[spec = \"" + objXmlSpecialization.InnerText + "\"]");
                    foreach (XmlNode objXmlWeapon in objXmlWeaponList)
                    {
                        objItem = new ListItem();
                        objItem.Value = objXmlWeapon["name"].InnerText;
                        objItem.Name = objXmlWeapon.Attributes?["translate"]?.InnerText ?? objXmlWeapon.InnerText;
                        lstItems.Add(objItem);
                    }
                }
            }
            // Populate the lists.
            cboSpec.BeginUpdate();
            cboSpec.ValueMember = "Value";
            cboSpec.DisplayMember = "Name";
            cboSpec.DataSource = lstItems;

            // If there's only 1 value in the list, the character doesn't have a choice, so just accept it.
            if (cboSpec.Items.Count == 1 && AllowAutoSelect)
                AcceptForm();

            if (!string.IsNullOrEmpty(_strForceItem))
            {
                cboSpec.SelectedIndex = cboSpec.FindStringExact(_strForceItem);
                if (cboSpec.SelectedIndex != -1)
                    AcceptForm();
                else
                {
                    cboSpec.DataSource = null;
                    List<ListItem> lstSingle = new List<ListItem>();
                    objItem = new ListItem();
                    objItem.Value = _strForceItem;
                    objItem.Name = _strForceItem;
                    lstSingle.Add(objItem);
                    cboSpec.ValueMember = "Value";
                    cboSpec.DisplayMember = "Name";
                    cboSpec.DataSource = lstSingle;
                    cboSpec.SelectedIndex = 0;
                    AcceptForm();
                }
            }
            cboSpec.EndUpdate();
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void cmdOK_Click(object sender, EventArgs e)
        {
            AcceptForm();
        }

        private void cboSpec_DropDown(object sender, EventArgs e)
        {
            // Resize the width of the DropDown so that the longest name fits.
            ComboBox objSender = (ComboBox)sender;
            int intWidth = objSender.DropDownWidth;
            Graphics objGraphics = objSender.CreateGraphics();
            Font objFont = objSender.Font;
            int intScrollWidth = (objSender.Items.Count > objSender.MaxDropDownItems) ? SystemInformation.VerticalScrollBarWidth : 0;
            int intNewWidth;
            foreach (ListItem objItem in ((ComboBox)sender).Items)
            {
                intNewWidth = (int)objGraphics.MeasureString(objItem.Name, objFont).Width + intScrollWidth;
                if (intWidth < intNewWidth)
                {
                    intWidth = intNewWidth;
                }
            }
            objSender.DropDownWidth = intWidth;
        }
        private void cboSpec_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cboSpec.SelectedValue.ToString() == "Custom")
            {
                cboSpec.DropDownStyle = ComboBoxStyle.DropDown;
            }
            else
            {
                cboSpec.DropDownStyle = ComboBoxStyle.DropDownList;
            }
        }
        #endregion

        #region Properties

        /// <summary>
        /// Name of the item that was selected.
        /// </summary>
        public string SelectedItem
        {
            get
            {
                if (cboSpec.SelectedValue != null && cboSpec.SelectedValue.ToString() != "Custom")
                {
                    return cboSpec.SelectedValue.ToString();
                }
                else
                {
                    return cboSpec.Text;
                }
            }
        }

        /// <summary>
        /// Whether or not the Form should be accepted if there is only one item left in the list.
        /// </summary>
        public bool AllowAutoSelect { get; set; } = true;

        /// <summary>
        /// Type of skill that we're selecting. Used to differentiate knowledge skills.
        /// </summary>
        public string Mode { get; set; }


        /// <summary>
        /// Whether or not to force the .
        /// </summary>
        public bool BuyWithKarma
        {
            get
            {
                return chkKarma.Checked;
            }
            set
            {
                chkKarma.Checked = value;
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Accept the selected item and close the form.
        /// </summary>
        private void AcceptForm()
        {
            DialogResult = DialogResult.OK;
        }

        private void MoveControls()
        {
            cboSpec.Left = lblAmmoLabel.Left + lblAmmoLabel.Width + 6;
            cboSpec.Width = Width - cboSpec.Left - 19;
        }
        #endregion
    }
}
