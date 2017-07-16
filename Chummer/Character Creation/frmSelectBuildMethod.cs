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
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;

namespace Chummer
{
    public partial class frmSelectBuildMethod : Form
    {
        private readonly Character _objCharacter;
        private readonly CharacterOptions _objOptions;
        private bool _blnUseCurrentValues = false;
        int intQualityLimits = 0;
        int intNuyenBP = 0;

        #region Control Events
        public frmSelectBuildMethod(Character objCharacter, bool blnUseCurrentValues = false)
        {
            _objCharacter = objCharacter;
            _objOptions = _objCharacter.Options;
            _blnUseCurrentValues = blnUseCurrentValues;
            InitializeComponent();
            LanguageManager.Instance.Load(GlobalOptions.Instance.Language, this);

            // Populate the Build Method list.
            List<ListItem> lstBuildMethod = new List<ListItem>();
            ListItem objKarma = new ListItem();
            objKarma.Value = "Karma";
            objKarma.Name = LanguageManager.Instance.GetString("String_Karma");

            ListItem objPriority = new ListItem();
            objPriority.Value = "Priority";
            objPriority.Name = LanguageManager.Instance.GetString("String_Priority");

            ListItem objSumtoTen = new ListItem();
            objSumtoTen.Value = "SumtoTen";
            objSumtoTen.Name = LanguageManager.Instance.GetString("String_SumtoTen");

            if (GlobalOptions.Instance.LifeModuleEnabled)
            {
                ListItem objLifeModule = new ListItem();
                objLifeModule.Value = "LifeModule";
                objLifeModule.Name = LanguageManager.Instance.GetString("String_LifeModule");
                lstBuildMethod.Add(objLifeModule);
            }

            lstBuildMethod.Add(objPriority);
            lstBuildMethod.Add(objKarma);
            lstBuildMethod.Add(objSumtoTen);
            cboBuildMethod.BeginUpdate();
            cboBuildMethod.ValueMember = "Value";
            cboBuildMethod.DisplayMember = "Name";
            cboBuildMethod.DataSource = lstBuildMethod;

            cboBuildMethod.SelectedValue = _objOptions.BuildMethod;
            cboBuildMethod.EndUpdate();
            nudKarma.Value = _objOptions.BuildPoints;
            nudMaxAvail.Value = _objOptions.Availability;
            

            // Load the Priority information.

            XmlDocument objXmlDocumentGameplayOptions = XmlManager.Instance.Load("gameplayoptions.xml");

            // Populate the Gameplay Options list.
            string strDefault = string.Empty;
            XmlNodeList objXmlGameplayOptionList = objXmlDocumentGameplayOptions.SelectNodes("/chummer/gameplayoptions/gameplayoption");
            
            foreach (XmlNode objXmlGameplayOption in objXmlGameplayOptionList)
            {
                string strName = objXmlGameplayOption["name"].InnerText;
                if (objXmlGameplayOption["default"]?.InnerText == "yes")
                    strDefault = strName;
                ListItem lstGameplay = new ListItem();
                cboGamePlay.Items.Add(strName);
            }
            cboGamePlay.Text = strDefault;
            XmlNode objXmlSelectedGameplayOption = objXmlDocumentGameplayOptions.SelectSingleNode("/chummer/gameplayoptions/gameplayoption[name = \"" + cboGamePlay.Text + "\"]");
            intQualityLimits = Convert.ToInt32(objXmlSelectedGameplayOption["karma"].InnerText);
            intNuyenBP = Convert.ToInt32(objXmlSelectedGameplayOption["maxnuyen"].InnerText);
            toolTip1.SetToolTip(chkIgnoreRules, LanguageManager.Instance.GetString("Tip_SelectKarma_IgnoreRules"));

            if (blnUseCurrentValues)
            {
                cboBuildMethod.SelectedValue = "Karma";
                nudKarma.Value = objCharacter.BuildKarma;

                nudMaxAvail.Value = objCharacter.MaximumAvailability;

                cboBuildMethod.Enabled = false;
            }
        }

        private void cmdOK_Click(object sender, EventArgs e)
        {
            switch (cboBuildMethod.SelectedValue.ToString())
            {
                case "Karma":
                    _objCharacter.NuyenMaximumBP = Convert.ToInt32(nudMaxNuyen.Value);
                    _objCharacter.BuildMethod = CharacterBuildMethod.Karma;
                    break;
                case "Priority":
                    _objCharacter.NuyenMaximumBP = intNuyenBP;
                    _objCharacter.BuildMethod = CharacterBuildMethod.Priority;
                    break;
                case "SumtoTen":
                    _objCharacter.NuyenMaximumBP = intNuyenBP;
                    _objCharacter.BuildMethod = CharacterBuildMethod.SumtoTen;
                    _objCharacter.SumtoTen = Convert.ToInt32(nudSumtoTen.Value);
                    break;
                case "LifeModule":
                    _objCharacter.NuyenMaximumBP = Convert.ToInt32(nudMaxNuyen.Value);
                    _objCharacter.BuildMethod = CharacterBuildMethod.LifeModule;
                    break;
            }
            _objCharacter.BuildPoints = 0;
            _objCharacter.BuildKarma = Convert.ToInt32(nudKarma.Value);
            _objCharacter.GameplayOption = cboGamePlay.Text;
            _objCharacter.GameplayOptionQualityLimit = intQualityLimits;
            _objCharacter.IgnoreRules = chkIgnoreRules.Checked;
            _objCharacter.MaximumAvailability = Convert.ToInt32(nudMaxAvail.Value);
            DialogResult = DialogResult.OK;
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void cboBuildMethod_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cboBuildMethod.SelectedValue == null)
            {
                lblDescription.Text = LanguageManager.Instance.GetString("String_SelectBP_PrioritySummary");
                nudKarma.Visible = false;
            }
            else
            {
                if (cboBuildMethod.SelectedValue.ToString() == "Karma")
                {
                    if (_objOptions.BuildMethod == "Karma")
                    {
                        lblDescription.Text = LanguageManager.Instance.GetString("String_SelectBP_KarmaSummary").Replace("{0}", _objOptions.BuildPoints.ToString());
                        if (!_blnUseCurrentValues)
                            nudKarma.Value = _objOptions.BuildPoints;
                    }
                    else
                    {
                        lblDescription.Text = LanguageManager.Instance.GetString("String_SelectBP_KarmaSummary").Replace("{0}", "800");
                        if (!_blnUseCurrentValues)
                            nudKarma.Value = 800;
                    }
                    nudMaxNuyen.Visible = true;
                    nudKarma.Visible = true;
                    nudSumtoTen.Visible = false;
                }
                else if (cboBuildMethod.SelectedValue.ToString() == "Priority")
                {
                    lblDescription.Text = LanguageManager.Instance.GetString("String_SelectBP_PrioritySummary");
                    nudKarma.Visible = false;
                    nudMaxNuyen.Visible = false;
                    nudSumtoTen.Visible = false;
                }
                else if (cboBuildMethod.SelectedValue.ToString() == "SumtoTen")
                {
                    lblDescription.Text = LanguageManager.Instance.GetString("String_SelectBP_PrioritySummary");
                    nudKarma.Visible = false;
                    nudMaxNuyen.Visible = false;
                    nudSumtoTen.Visible = true;
                }
                else if (cboBuildMethod.SelectedValue.ToString() == "LifeModule")
                {
                    lblDescription.Text =
                        String.Format(LanguageManager.Instance.GetString("String_SelectBP_LifeModuleSummary"), 750);
                    nudKarma.Visible = true;
                    nudMaxNuyen.Visible = true;
                    nudSumtoTen.Visible = false;

                    if (!_blnUseCurrentValues)
                        nudKarma.Value = 750;
                }
                lblStartingKarma.Visible = nudKarma.Visible;
                lblSumToX.Visible = nudSumtoTen.Visible;
                lblMaxNuyen.Visible = nudMaxNuyen.Visible;
            }
        }

        private void frmSelectBuildMethod_Load(object sender, EventArgs e)
        {
            Height = cmdOK.Bottom + 40;
            cboBuildMethod_SelectedIndexChanged(this, e);
        }
        #endregion

        private void cboGamePlay_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Load the Priority information.
            XmlDocument objXmlDocumentGameplayOption = XmlManager.Instance.Load("gameplayoptions.xml");
            XmlNode objXmlGameplayOption = objXmlDocumentGameplayOption.SelectSingleNode("/chummer/gameplayoptions/gameplayoption[name = \"" + cboGamePlay.Text + "\"]");
            nudMaxAvail.Value = Convert.ToInt32(objXmlGameplayOption["maxavailability"].InnerText);
            intQualityLimits = Convert.ToInt32(objXmlGameplayOption["karma"].InnerText);
            intNuyenBP = Convert.ToInt32(objXmlGameplayOption["maxnuyen"].InnerText);
        }
    }
}