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
using System.Linq;
using System.Windows.Forms;

namespace Chummer
{
    public partial class frmSelectAttribute : Form
    {
        private string _strReturnValue = string.Empty;

        private readonly List<ListItem> _lstAttributes = null;

        #region Control Events
        public frmSelectAttribute()
        {
            InitializeComponent();
            LanguageManager.Translate(GlobalOptions.Language, this);

            // Build the list of Attributes.
            _lstAttributes = new List<ListItem>
            {
                new ListItem("BOD", LanguageManager.GetString("String_AttributeBODShort", GlobalOptions.Language)),
                new ListItem("AGI", LanguageManager.GetString("String_AttributeAGIShort", GlobalOptions.Language)),
                new ListItem("REA", LanguageManager.GetString("String_AttributeREAShort", GlobalOptions.Language)),
                new ListItem("STR", LanguageManager.GetString("String_AttributeSTRShort", GlobalOptions.Language)),
                new ListItem("CHA", LanguageManager.GetString("String_AttributeCHAShort", GlobalOptions.Language)),
                new ListItem("INT", LanguageManager.GetString("String_AttributeINTShort", GlobalOptions.Language)),
                new ListItem("LOG", LanguageManager.GetString("String_AttributeLOGShort", GlobalOptions.Language)),
                new ListItem("WIL", LanguageManager.GetString("String_AttributeWILShort", GlobalOptions.Language)),
                new ListItem("EDG", LanguageManager.GetString("String_AttributeEDGShort", GlobalOptions.Language))
            };

            cboAttribute.BeginUpdate();
            cboAttribute.ValueMember = "Value";
            cboAttribute.DisplayMember = "Name";
            cboAttribute.DataSource = _lstAttributes;
            cboAttribute.EndUpdate();
        }

        private void cmdOK_Click(object sender, EventArgs e)
        {
            _strReturnValue = cboAttribute.SelectedValue.ToString();
            DialogResult = DialogResult.OK;
        }

        private void frmSelectAttribute_Load(object sender, EventArgs e)
        {
            // Select the first Attribute in the list.
            cboAttribute.SelectedIndex = 0;
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
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
            _lstAttributes.Add(new ListItem("MAG", LanguageManager.GetString("String_AttributeMAGShort", GlobalOptions.Language)));
            cboAttribute.BeginUpdate();
            cboAttribute.DataSource = null;
            cboAttribute.ValueMember = "Value";
            cboAttribute.DisplayMember = "Name";
            cboAttribute.DataSource = _lstAttributes;
            cboAttribute.EndUpdate();
        }

        /// <summary>
        /// Add Adept MAG to the list of selectable Attributes.
        /// </summary>
        public void AddMAGAdept()
        {
            _lstAttributes.Add(new ListItem("MAGAdept", LanguageManager.GetString("String_AttributeMAGShort", GlobalOptions.Language) + " (" + LanguageManager.GetString("String_DescAdept", GlobalOptions.Language) + ")"));
            cboAttribute.BeginUpdate();
            cboAttribute.DataSource = null;
            cboAttribute.ValueMember = "Value";
            cboAttribute.DisplayMember = "Name";
            cboAttribute.DataSource = _lstAttributes;
            cboAttribute.EndUpdate();
        }

        /// <summary>
        /// Add RES to the list of selectable Attributes.
        /// </summary>
        public void AddRES()
        {
            _lstAttributes.Add(new ListItem("RES", LanguageManager.GetString("String_AttributeRESShort", GlobalOptions.Language)));
            cboAttribute.BeginUpdate();
            cboAttribute.DataSource = null;
            cboAttribute.ValueMember = "Value";
            cboAttribute.DisplayMember = "Name";
            cboAttribute.DataSource = _lstAttributes;
            cboAttribute.EndUpdate();
        }

        /// <summary>
        /// Add DEP to the list of selectable Attributes.
        /// </summary>
        public void AddDEP()
        {
            _lstAttributes.Add(new ListItem("DEP", LanguageManager.GetString("String_AttributeDEPShort", GlobalOptions.Language)));
            cboAttribute.BeginUpdate();
            cboAttribute.DataSource = null;
            cboAttribute.ValueMember = "Value";
            cboAttribute.DisplayMember = "Name";
            cboAttribute.DataSource = _lstAttributes;
            cboAttribute.EndUpdate();
        }

        /// <summary>
        /// Limit the list to a single Attribute.
        /// </summary>
        /// <param name="strValue">Single Attribute to display.</param>
        public void SingleAttribute(string strValue)
        {
            List<ListItem> lstItems = new List<ListItem>
            {
                new ListItem(strValue, strValue)
            };
            cboAttribute.BeginUpdate();
            cboAttribute.DataSource = null;
            cboAttribute.ValueMember = "Value";
            cboAttribute.DisplayMember = "Name";
            cboAttribute.DataSource = lstItems;
            cboAttribute.EndUpdate();
        }

        /// <summary>
        /// Limit the list to a few Attributes.
        /// </summary>
        /// <param name="strValue">List of Attributes.</param>
        public void LimitToList(IEnumerable<string> strValue, Character objCharacter)
        {
            _lstAttributes.Clear();
            foreach (string strAttribute in strValue)
            {
                if (strAttribute == "MAGAdept")
                {
                    if (objCharacter.Options.MysAdeptSecondMAGAttribute && objCharacter.IsMysticAdept)
                    {
                        _lstAttributes.Add(new ListItem("MAGAdept", LanguageManager.GetString("String_AttributeMAGShort", GlobalOptions.Language) + " (" + LanguageManager.GetString("String_DescAdept", GlobalOptions.Language) + ")"));
                    }
                    if (!_lstAttributes.Any(x => x.Value == "MAG"))
                        _lstAttributes.Add(new ListItem("MAG", LanguageManager.GetString("String_AttributeMAGShort", GlobalOptions.Language)));
                }
                else
                {
                    _lstAttributes.Add(new ListItem(strAttribute, LanguageManager.GetString("String_Attribute" + strAttribute + "Short", GlobalOptions.Language)));
                    if (strAttribute == "MAG" && objCharacter.Options.MysAdeptSecondMAGAttribute && objCharacter.IsMysticAdept && !_lstAttributes.Any(x => x.Value == "MAGAdept"))
                        _lstAttributes.Add(new ListItem("MAGAdept", LanguageManager.GetString("String_AttributeMAGShort", GlobalOptions.Language) + " (" + LanguageManager.GetString("String_DescAdept", GlobalOptions.Language) + ")"));
                }
            }
            cboAttribute.BeginUpdate();
            cboAttribute.DataSource = null;
            cboAttribute.ValueMember = "Value";
            cboAttribute.DisplayMember = "Name";
            cboAttribute.DataSource = _lstAttributes;
            cboAttribute.EndUpdate();
        }

        /// <summary>
        /// Exclude the list of Attributes.
        /// </summary>
        /// <param name="strValue">List of Attributes.</param>
        public void RemoveFromList(IEnumerable<string> strValue)
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
            cboAttribute.BeginUpdate();
            cboAttribute.DataSource = null;
            cboAttribute.ValueMember = "Value";
            cboAttribute.DisplayMember = "Name";
            cboAttribute.DataSource = _lstAttributes;
            cboAttribute.EndUpdate();
        }
        #endregion
    }
}
