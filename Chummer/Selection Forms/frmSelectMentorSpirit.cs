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
 using System;
using System.Collections.Generic;
 using System.Windows.Forms;
 using System.Xml.XPath;

namespace Chummer
{
    public partial class frmSelectMentorSpirit : Form
    {
        private bool _blnSkipRefresh = true;
        private string _strForceMentor = string.Empty;

        private readonly string _strXmlFile;
        private readonly XPathNavigator _xmlBaseMentorSpiritDataNode;
        private readonly Character _objCharacter;

        #region Control Events
        public frmSelectMentorSpirit(Character objCharacter, string strXmlFile = "mentors.xml")
        {
            InitializeComponent();

            // Load the Mentor information.
            _strXmlFile = strXmlFile;
            _xmlBaseMentorSpiritDataNode = XmlManager.Load(strXmlFile).GetFastNavigator().SelectSingleNode("/chummer");
            if (strXmlFile == "paragons.xml")
                Tag = "Title_SelectMentorSpirit_Paragon";

            LanguageManager.TranslateWinForm(GlobalOptions.Language, this);
            _objCharacter = objCharacter;
        }

        private void frmSelectMentorSpirit_Load(object sender, EventArgs e)
        {
            string strForceId = string.Empty;
            List<ListItem> lstMentors = new List<ListItem>();

            // Populate the Mentor list.
            foreach (XPathNavigator objXmlMentor in _xmlBaseMentorSpiritDataNode.Select("mentors/mentor[(" + _objCharacter.Options.BookXPath() + ")]"))
            {
                string strName = objXmlMentor.SelectSingleNode("name")?.Value ?? LanguageManager.GetString("String_Unknown", GlobalOptions.Language);
                string strId = objXmlMentor.SelectSingleNode("id")?.Value ?? string.Empty;
                if (strName == _strForceMentor)
                    strForceId = strId;
                lstMentors.Add(new ListItem(strId, objXmlMentor.SelectSingleNode("translate")?.Value ?? strName));
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
            if (_blnSkipRefresh)
                return;

            XPathNavigator objXmlMentor = null;
            string strSelectedId = lstMentor.SelectedValue?.ToString();
            if (!string.IsNullOrEmpty(strSelectedId))
                objXmlMentor = _xmlBaseMentorSpiritDataNode.SelectSingleNode("mentors/mentor[id = \"" + lstMentor.SelectedValue + "\"]");
            if (objXmlMentor != null)
            {
                // Get the information for the selected Mentor.
                lblAdvantage.Text = objXmlMentor.SelectSingleNode("altadvantage")?.Value ??
                                    objXmlMentor.SelectSingleNode("advantage")?.Value ??
                                    LanguageManager.GetString("String_Unknown", GlobalOptions.Language);
                lblDisadvantage.Text = objXmlMentor.SelectSingleNode("altdisadvantage")?.Value ??
                                       objXmlMentor.SelectSingleNode("disadvantage")?.Value ??
                                       LanguageManager.GetString("String_Unknown", GlobalOptions.Language);

                cboChoice1.BeginUpdate();
                cboChoice2.BeginUpdate();
                cboChoice1.DataSource = null;
                cboChoice2.DataSource = null;

                // If the Mentor offers a choice of bonuses, build the list and let the user select one.
                XPathNavigator xmlChoices = objXmlMentor.SelectSingleNode("choices");
                if (xmlChoices != null)
                {
                    List<ListItem> lstChoice1 = new List<ListItem>();
                    List<ListItem> lstChoice2 = new List<ListItem>();

                    foreach (XPathNavigator objChoice in xmlChoices.Select("choice"))
                    {
                        string strName = objChoice.SelectSingleNode("name")?.Value ?? string.Empty;
                        if ((_objCharacter.AdeptEnabled || !strName.StartsWith("Adept:")) && (_objCharacter.MagicianEnabled || !strName.StartsWith("Magician:")))
                        {
                            if (objChoice.SelectSingleNode("@set")?.Value == "2")
                                lstChoice2.Add(new ListItem(strName, objChoice.SelectSingleNode("translate")?.Value ?? strName));
                            else
                                lstChoice1.Add(new ListItem(strName, objChoice.SelectSingleNode("translate")?.Value ?? strName));
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

                string strSource = objXmlMentor.SelectSingleNode("source")?.Value ?? LanguageManager.GetString("String_Unknown", GlobalOptions.Language);
                string strPage = objXmlMentor.SelectSingleNode("altpage")?.Value ?? objXmlMentor.SelectSingleNode("page")?.Value ?? LanguageManager.GetString("String_Unknown", GlobalOptions.Language);
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
            set => _strForceMentor = value;
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
                XPathNavigator objXmlMentor = _xmlBaseMentorSpiritDataNode.SelectSingleNode("mentors/mentor[id = \"" + strSelectedId + "\"]");
                if (objXmlMentor == null)
                    return;

                SelectedMentor = strSelectedId;
                
                DialogResult = DialogResult.OK;
            }
        }
        #endregion
    }
}
