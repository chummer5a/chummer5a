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
using System.Xml;

namespace Chummer
{
    public partial class frmSelectMentorSpirit : Form
    {
        private XmlNode _nodBonus;
        private XmlNode _nodChoice1Bonus;
        private XmlNode _nodChoice2Bonus;
        private string _strXmlFile = "mentors.xml";

        private XmlDocument _objXmlDocument = new XmlDocument();
        private readonly Character _objCharacter;

        private List<ListItem> _lstCategory = new List<ListItem>();

        #region Control Events
        public frmSelectMentorSpirit(Character objCharacter)
        {
            InitializeComponent();
            LanguageManager.Instance.Load(GlobalOptions.Instance.Language, this);
            _objCharacter = objCharacter;
        }

        private void frmSelectMentorSpirit_Load(object sender, EventArgs e)
        {
            if (_strXmlFile == "paragons.xml")
                Text = LanguageManager.Instance.GetString("Title_SelectMentorSpirit_Paragon");

            foreach (Label objLabel in Controls.OfType<Label>())
            {
                if (objLabel.Text.StartsWith("["))
                    objLabel.Text = string.Empty;
            }

            // Load the Mentor information.
            _objXmlDocument = XmlManager.Instance.Load(_strXmlFile);

            List<ListItem> lstMentors = new List<ListItem>();

            // Populate the Mentor list.
            XmlNodeList objXmlMentorList = _objXmlDocument.SelectNodes("/chummer/mentors/mentor[(" + _objCharacter.Options.BookXPath() + ")]");
            foreach (XmlNode objXmlMentor in objXmlMentorList)
            {
                ListItem objItem = new ListItem();
                objItem.Value = objXmlMentor["id"].InnerText;
                objItem.Name = objXmlMentor["translate"]?.InnerText ?? objXmlMentor["name"].InnerText;
                lstMentors.Add(objItem);
            }
            SortListItem objSort = new SortListItem();
            lstMentors.Sort(objSort.Compare);
            lstMentor.BeginUpdate();
            lstMentor.DataSource = null;
            lstMentor.ValueMember = "Value";
            lstMentor.DisplayMember = "Name";
            lstMentor.DataSource = lstMentors;
            lstMentor.EndUpdate();
        }

        private void cmdOK_Click(object sender, EventArgs e)
        {
            AcceptForm();
        }

        private void lstMentor_DoubleClick(object sender, EventArgs e)
        {
            AcceptForm();
        }

        private void lstMentor_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(lstMentor.Text))
                return;

            // Get the information for the selected Mentor.
            XmlNode objXmlMentor = _objXmlDocument.SelectSingleNode("/chummer/mentors/mentor[id = \"" + lstMentor.SelectedValue + "\"]");

            lblAdvantage.Text = objXmlMentor["altadvantage"]?.InnerText ?? objXmlMentor["advantage"].InnerText;
            lblDisadvantage.Text = objXmlMentor["altdisadvantage"]?.InnerText ?? objXmlMentor["disadvantage"].InnerText;

            cboChoice1.BeginUpdate();
            cboChoice2.BeginUpdate();
            cboChoice1.DataSource = null;
            cboChoice2.DataSource = null;

            // If the Mentor offers a choice of bonuses, build the list and let the user select one.
            if (objXmlMentor["choices"] != null)
            {
                List<ListItem> lstChoice1 = new List<ListItem>();
                List<ListItem> lstChoice2 = new List<ListItem>();

                foreach (XmlNode objChoice in objXmlMentor["choices"].SelectNodes("choice"))
                {                    
                    bool blnShow = !(objChoice["name"].InnerText.StartsWith("Adept:") && !_objCharacter.AdeptEnabled);
                    if (objChoice["name"].InnerText.StartsWith("Magician:") && !_objCharacter.MagicianEnabled)
                        blnShow = false;

                    if (!blnShow) continue;
                    ListItem objItem = new ListItem();
                    objItem.Value = objChoice["name"].InnerText;
                    objItem.Name = objChoice["translate"]?.InnerText ?? objChoice["name"].InnerText;

                    if (objChoice.Attributes["set"] != null)
                    {
                        if (objChoice.Attributes["set"].InnerText == "2")
                            lstChoice2.Add(objItem);
                        else
                            lstChoice1.Add(objItem);
                    }
                    else
                        lstChoice1.Add(objItem);
                }

                lblChoice1.Visible = true;
                cboChoice1.Visible = true;
                cboChoice1.ValueMember = "Value";
                cboChoice1.DisplayMember = "Name";
                cboChoice1.DataSource = lstChoice1;

                lblChoice1.Top = lblAdvantage.Top + lblAdvantage.Height + 6;
                cboChoice1.Top = lblChoice1.Top + lblChoice1.Height + 3;
                lblChoice2.Top = cboChoice1.Top + cboChoice1.Height + 6;
                cboChoice2.Top = lblChoice2.Top + lblChoice2.Height + 3;

                if (lstChoice2.Count > 0)
                {
                    lblChoice2.Visible = true;
                    cboChoice2.Visible = true;
                    cboChoice2.ValueMember = "Value";
                    cboChoice2.DisplayMember = "Name";
                    cboChoice2.DataSource = lstChoice2;
                    chkMentorMask.Top = cboChoice2.Top + cboChoice2.Height + 6;                    
                }
                else 
                {
                    lblChoice2.Visible = false;
                    cboChoice2.Visible = false;
                    chkMentorMask.Top = cboChoice1.Top + cboChoice1.Height + 6;
                }

                lblDisadvantageLabel.Top = Math.Max(chkMentorMask.Top + chkMentorMask.Height + 6, 133);
                lblDisadvantage.Top = Math.Max(chkMentorMask.Top + chkMentorMask.Height + 6, 133);
            }
            else
            {
                lblChoice1.Visible = false;
                cboChoice1.Visible = false;
                lblChoice2.Visible = false;
                cboChoice2.Visible = false;
            }
            cboChoice1.EndUpdate();
            cboChoice2.EndUpdate();

            string strBook = _objCharacter.Options.LanguageBookShort(objXmlMentor["source"].InnerText);
            string strPage = objXmlMentor["page"].InnerText;
            if (objXmlMentor["altpage"] != null)
                strPage = objXmlMentor["altpage"].InnerText;
            lblSource.Text = strBook + " " + strPage;

            tipTooltip.SetToolTip(lblSource, _objCharacter.Options.LanguageBookLong(objXmlMentor["source"].InnerText) + " " + LanguageManager.Instance.GetString("String_Page") + " " + strPage);
        }
        #endregion

        #region Properties
        /// <summary>
        /// XML file to read from. Default mentors.xml.
        /// </summary>
        public string XmlFile
        {
            set
            {
                _strXmlFile = value;
            }
        }

        /// <summary>
        /// Mentor that was selected in the dialogue.
        /// </summary>
        public string SelectedMentor { get; private set; } = string.Empty;

        /// <summary>
        /// First choice that was selected in the dialogue.
        /// </summary>
        public string Choice1 => cboChoice1.SelectedValue?.ToString() ?? string.Empty;

        /// <summary>
        /// Second choice that was selected in the dialogue.
        /// </summary>
        public string Choice2 => cboChoice2.SelectedValue?.ToString() ?? string.Empty;

        /// <summary>
        /// Bonus Node for the Mentor that was selected.
        /// </summary>
        public XmlNode BonusNode => _nodBonus;

        /// <summary>
        /// Bonus Node for the first choice that was selected.
        /// </summary>
        public XmlNode Choice1BonusNode => _nodChoice1Bonus;

        /// <summary>
        /// Bonus Node for the second choice that was selected.
        /// </summary>
        public XmlNode Choice2BonusNode => _nodChoice2Bonus;

        /// <summary>
        /// Whether the character manifests the Mentor's Mask. Used externally to create improvements.
        /// </summary>
        public bool MentorsMask => chkMentorMask.Checked;
        #endregion

        #region Methods
        /// <summary>
        /// Accept the selected item and close the form.
        /// </summary>
        private void AcceptForm()
        {
            if (string.IsNullOrEmpty(lstMentor.Text)) return;
            XmlNode objXmlMentor = _objXmlDocument.SelectSingleNode("/chummer/mentors/mentor[id = \"" + lstMentor.SelectedValue + "\"]");

            SelectedMentor = objXmlMentor["id"].InnerText;

            if (objXmlMentor.InnerXml.Contains("<bonus>"))
                _nodBonus = objXmlMentor.SelectSingleNode("bonus");

            if (cboChoice1.SelectedValue != null)
            {
                XmlNode objChoice = objXmlMentor.SelectSingleNode("choices/choice[name = \"" + cboChoice1.SelectedValue + "\"]");
                if (objChoice.InnerXml.Contains("<bonus>"))
                    _nodChoice1Bonus = objChoice.SelectSingleNode("bonus");
            }

            if (cboChoice2.SelectedValue != null)
            {
                XmlNode objChoice = objXmlMentor.SelectSingleNode("choices/choice[name = \"" + cboChoice2.SelectedValue + "\"]");
                if (objChoice.InnerXml.Contains("<bonus>"))
                    _nodChoice2Bonus = objChoice.SelectSingleNode("bonus");
            }

            DialogResult = DialogResult.OK;
        }
        #endregion

        private void lblSource_Click(object sender, EventArgs e)
        {
            CommonFunctions.StaticOpenPDF(lblSource.Text, _objCharacter);
        }
    }
}