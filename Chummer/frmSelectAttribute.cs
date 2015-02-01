using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Chummer
{
    public partial class frmSelectAttribute : Form
    {
        private string _strReturnValue = "";

		private List<ListItem> _lstAttributes = new List<ListItem>();

		#region Control Events
		public frmSelectAttribute()
        {
            InitializeComponent();
			LanguageManager.Instance.Load(GlobalOptions.Instance.Language, this);

			// Build the list of Attributes.
			ListItem objBOD = new ListItem();
			ListItem objAGI = new ListItem();
			ListItem objREA = new ListItem();
			ListItem objSTR = new ListItem();
			ListItem objCHA = new ListItem();
			ListItem objINT = new ListItem();
			ListItem objLOG = new ListItem();
			ListItem objWIL = new ListItem();
			ListItem objEDG = new ListItem();
			objBOD.Value = "BOD";
			objBOD.Name = LanguageManager.Instance.GetString("String_AttributeBODShort");
			objAGI.Value = "AGI";
			objAGI.Name = LanguageManager.Instance.GetString("String_AttributeAGIShort");
			objREA.Value = "REA";
			objREA.Name = LanguageManager.Instance.GetString("String_AttributeREAShort");
			objSTR.Value = "STR";
			objSTR.Name = LanguageManager.Instance.GetString("String_AttributeSTRShort");
			objCHA.Value = "CHA";
			objCHA.Name = LanguageManager.Instance.GetString("String_AttributeCHAShort");
			objINT.Value = "INT";
			objINT.Name = LanguageManager.Instance.GetString("String_AttributeINTShort");
			objLOG.Value = "LOG";
			objLOG.Name = LanguageManager.Instance.GetString("String_AttributeLOGShort");
			objWIL.Value = "WIL";
			objWIL.Name = LanguageManager.Instance.GetString("String_AttributeWILShort");
			objEDG.Value = "EDG";
			objEDG.Name = LanguageManager.Instance.GetString("String_AttributeEDGShort");
			_lstAttributes.Add(objBOD);
			_lstAttributes.Add(objAGI);
			_lstAttributes.Add(objREA);
			_lstAttributes.Add(objSTR);
			_lstAttributes.Add(objCHA);
			_lstAttributes.Add(objINT);
			_lstAttributes.Add(objLOG);
			_lstAttributes.Add(objWIL);
			_lstAttributes.Add(objEDG);

			cboAttribute.ValueMember = "Value";
			cboAttribute.DisplayMember = "Name";
			cboAttribute.DataSource = _lstAttributes;
        }

		private void cmdOK_Click(object sender, EventArgs e)
        {
			_strReturnValue = cboAttribute.SelectedValue.ToString();
            this.DialogResult = DialogResult.OK;
        }

        private void frmSelectAttribute_Load(object sender, EventArgs e)
        {
            // Select the first Attribute in the list.
            cboAttribute.SelectedIndex = 0;
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

		private void frmSelectAttribute_Shown(object sender, EventArgs e)
		{
			// If only a single Attribute is in the list when the form is shown,
			// click the OK button since the user really doesn't have a choice.
			if (cboAttribute.Items.Count == 1)
				cmdOK_Click(sender, e);
		}
		#endregion

		#region Properties
		/// <summary>
        /// Attribute that was selected in the dialogue.
        /// </summary>
        public string SelectedAttribute
        {
            get
            {
                return _strReturnValue;
            }
        }

        /// <summary>
        /// Description to display on the form.
        /// </summary>
        public string Description
        {
            set
            {
                lblDescription.Text = value;
            }
        }

		/// <summary>
		/// Whether or not the Do not affect Metatype Maximum checkbox should be shown on the form.
		/// </summary>
		public bool ShowMetatypeMaximum
		{
			set
			{
				chkDoNotAffectMetatypeMaximum.Visible = value;
			}
		}

		/// <summary>
		/// Whether or not the Metatype Maximum value should be affected as well.
		/// </summary>
		public bool DoNotAffectMetatypeMaximum
		{
			get
			{
				return chkDoNotAffectMetatypeMaximum.Checked;
			}
		}
		#endregion

		#region Methods
		/// <summary>
        /// Add MAG to the list of selectable Attributes.
        /// </summary>
        public void AddMAG()
        {
			ListItem objMAG = new ListItem();
			objMAG.Value = "MAG";
			objMAG.Name = LanguageManager.Instance.GetString("String_AttributeMAGShort");
			_lstAttributes.Add(objMAG);
			cboAttribute.DataSource = null;
			cboAttribute.DataSource = _lstAttributes;
			cboAttribute.ValueMember = "Value";
			cboAttribute.DisplayMember = "Name";
        }

        /// <summary>
        /// Add RES to the list of selectable Attributes.
        /// </summary>
        public void AddRES()
        {
			ListItem objRES = new ListItem();
			objRES.Value = "RES";
			objRES.Name = LanguageManager.Instance.GetString("String_AttributeRESShort");
			_lstAttributes.Add(objRES);
			cboAttribute.DataSource = null;
			cboAttribute.DataSource = _lstAttributes;
			cboAttribute.ValueMember = "Value";
			cboAttribute.DisplayMember = "Name";
        }

		/// <summary>
		/// Limit the list to a single Attribute.
		/// </summary>
		/// <param name="strValue">Single Attribute to display.</param>
		public void SingleAttribute(string strValue)
		{
			List<ListItem> lstItems = new List<ListItem>();
			ListItem objItem = new ListItem();
			objItem.Value = strValue;
			objItem.Name = strValue;
			lstItems.Add(objItem);
			cboAttribute.DataSource = null;
			cboAttribute.ValueMember = "Value";
			cboAttribute.DisplayMember = "Name";
			cboAttribute.DataSource = lstItems;
		}

		/// <summary>
		/// Limit the list to a few Attributes.
		/// </summary>
		/// <param name="strValue">List of Attributes.</param>
		public void LimitToList(List<string> strValue)
		{
			_lstAttributes.Clear();
			foreach (string strAttribute in strValue)
			{
				ListItem objItem = new ListItem();
				objItem.Value = strAttribute;
				objItem.Name = LanguageManager.Instance.GetString("String_Attribute" + strAttribute + "Short");
				_lstAttributes.Add(objItem);
			}
			cboAttribute.DataSource = null;
			cboAttribute.DataSource = _lstAttributes;
			cboAttribute.ValueMember = "Value";
			cboAttribute.DisplayMember = "Name";
		}

		/// <summary>
		/// Exclude the list of Attributes.
		/// </summary>
		/// <param name="strValue">List of Attributes.</param>
		public void RemoveFromList(List<string> strValue)
		{
			foreach (string strAttribute in strValue)
			{
				foreach (ListItem objItem in _lstAttributes)
				{
					if (objItem.Value == strAttribute)
					{
						_lstAttributes.Remove(objItem);
						break;
					}
				}
			}
			cboAttribute.DataSource = null;
			cboAttribute.DataSource = _lstAttributes;
			cboAttribute.ValueMember = "Value";
			cboAttribute.DisplayMember = "Name";
		}
		#endregion
    }
}