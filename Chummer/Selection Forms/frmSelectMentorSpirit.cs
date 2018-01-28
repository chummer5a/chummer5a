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
        private bool _blnSkipRefresh = true;
        private XmlNode _nodBonus;
        private XmlNode _nodChoice1Bonus;
        private XmlNode _nodChoice2Bonus;
        private string _strForceMentor = string.Empty;

        private readonly string _strXmlFile = "mentors.xml";
        private readonly XmlDocument _objXmlDocument;
        private readonly Character _objCharacter;

        private readonly List<ListItem> _lstCategory = new List<ListItem>();

        #region Control Events
        public frmSelectMentorSpirit(Character objCharacter, string strXmlFile = "mentors.xml")
        {
            InitializeComponent();

            // Load the Mentor information.
            _strXmlFile = strXmlFile;
            _objXmlDocument = XmlManager.Load(strXmlFile);
            if (strXmlFile == "paragons.xml")
                Tag = "Title_SelectMentorSpirit_Paragon";

            LanguageManager.TranslateWinForm(GlobalOptions.Language, this);
            _objCharacter = objCharacter;
        }

        private void frmSelectMentorSpirit_Load(object sender, EventArgs e)
        {
            string strForceId = string.Empty;
            List<string> lstMentorNames = new List<string>();
            List<ListItem> lstMentors = new List<ListItem>();

            // Populate the Mentor list.
            XmlNodeList objXmlMentorList = _objXmlDocument.SelectNodes("/chummer/mentors/mentor[(" + _objCharacter.Options.BookXPath() + ")]");
            foreach (XmlNode objXmlMentor in objXmlMentorList)
            {
                string strName = objXmlMentor["name"].InnerText;
                string strId = objXmlMentor["id"].InnerText;
                if (strName == _strForceMentor)
                    strForceId = strId;
                lstMentors.Add(new ListItem(strId, objXmlMentor["translate"]?.InnerText ?? strName));
            }
            lstMentors.Sort(CompareListItems.CompareNames);
            lstMentor.BeginUpdate();
            lstMentor.ValueMember = "Value";
            lstMentor.DisplayMember = "Name";
            lstMentor.DataSource = lstMentors;
            lstMentor.EndUpdate();
            _blnSkipRefresh = false;
            if (!string.IsNullOrEmpty(strForceId))
            {
                lstMentor.SelectedValue = strForceId;
                lstMentor.Enabled = false;
            }
            chkMentorMask.Visible = _strXmlFile == "mentors.xml" && _objCharacter.Options.Books.Contains("FA");
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
            string strSelectedId = lstMentor.SelectedValue?.ToString();
            if (!_blnSkipRefresh && !string.IsNullOrEmpty(strSelectedId))
            {
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

                    foreach (XmlNode objChoice in objXmlMentor.SelectNodes("choices/choice"))
                    {
                        string strName = objChoice["name"].InnerText;
                        if ((_objCharacter.AdeptEnabled || !strName.StartsWith("Adept:")) && (_objCharacter.MagicianEnabled || !strName.StartsWith("Magician:")))
                        {
                            if (objChoice.Attributes["set"]?.InnerText == "2")
                                lstChoice2.Add(new ListItem(strName, objChoice["translate"]?.InnerText ?? strName));
                            else
                                lstChoice1.Add(new ListItem(strName, objChoice["translate"]?.InnerText ?? strName));
                        }
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

                string strSource = objXmlMentor["source"].InnerText;
                string strPage = objXmlMentor["altpage"]?.InnerText ?? objXmlMentor["page"].InnerText;
                lblSource.Text = CommonFunctions.LanguageBookShort(strSource, GlobalOptions.Language) + ' ' + strPage;

                tipTooltip.SetToolTip(lblSource, CommonFunctions.LanguageBookLong(strSource, GlobalOptions.Language) + ' ' + LanguageManager.GetString("String_Page", GlobalOptions.Language) + ' ' + strPage);
                cmdOK.Enabled = true;
            }
            else
            {
                lblAdvantage.Text = string.Empty;
                lblDisadvantage.Text = string.Empty;
                lblChoice1.Visible = false;
                lblChoice2.Visible = false;
                cboChoice1.BeginUpdate();
                cboChoice1.Visible = false;
                cboChoice1.DataSource = null;
                cboChoice1.EndUpdate();
                cboChoice2.BeginUpdate();
                cboChoice2.Visible = false;
                cboChoice2.DataSource = null;
                cboChoice2.EndUpdate();
                lblSource.Text = string.Empty;
                tipTooltip.SetToolTip(lblSource, string.Empty);
                cmdOK.Enabled = false;
            }
        }
        #endregion

        #region Properties
        /// <summary>
        /// Forced selection for mentor spirit
        /// </summary>
        public string ForcedMentor
        {
            set
            {
                _strForceMentor = value;
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
            string strSelectedId = lstMentor.SelectedValue?.ToString();
            if (!string.IsNullOrEmpty(strSelectedId))
            {
                XmlNode objXmlMentor = _objXmlDocument.SelectSingleNode("/chummer/mentors/mentor[id = \"" + strSelectedId + "\"]");

                SelectedMentor = strSelectedId;

                _nodBonus = objXmlMentor["bonus"];

                string strChoice1 = cboChoice1.SelectedValue?.ToString();
                if (!string.IsNullOrEmpty(strChoice1))
                {
                    _nodChoice1Bonus = objXmlMentor.SelectSingleNode("choices/choice[name = \"" + strChoice1 + "\"]/bonus");
                }
                string strChoice2 = cboChoice2.SelectedValue?.ToString();
                if (!string.IsNullOrEmpty(strChoice2))
                {
                    _nodChoice2Bonus = objXmlMentor.SelectSingleNode("choices/choice[name = \"" + strChoice2 + "\"]/bonus");
                }

                DialogResult = DialogResult.OK;
            }
        }
        #endregion
    }
}
