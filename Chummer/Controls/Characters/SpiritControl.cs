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
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;
using Chummer.Backend.Uniques;

namespace Chummer
{
    public partial class SpiritControl : UserControl
    {
        private readonly Spirit _objSpirit;
        private bool _blnLoading = true;

        // Events.
        public event EventHandler ContactDetailChanged;

        public event EventHandler DeleteSpirit;

        #region Control Events

        public SpiritControl(Spirit objSpirit)
        {
            _objSpirit = objSpirit;
            InitializeComponent();
            this.UpdateLightDarkMode();
            this.TranslateWinForm();
            foreach (ToolStripItem tssItem in cmsSpirit.Items)
            {
                tssItem.UpdateLightDarkMode();
                tssItem.TranslateToolStripItemsRecursively();
            }
        }

        private void SpiritControl_Load(object sender, EventArgs e)
        {
            bool blnIsSpirit = _objSpirit.EntityType == SpiritType.Spirit;
            nudForce.DoOneWayDataBinding("Enabled", _objSpirit.CharacterObject, nameof(Character.Created));
            chkBound.DoDataBinding("Checked", _objSpirit, nameof(Spirit.Bound));
            chkBound.DoOneWayDataBinding("Enabled", _objSpirit.CharacterObject, nameof(Character.Created));
            cboSpiritName.DoDataBinding("Text", _objSpirit, nameof(Spirit.Name));
            txtCritterName.DoDataBinding("Text", _objSpirit, nameof(Spirit.CritterName));
            txtCritterName.DoOneWayDataBinding("Enabled", _objSpirit, nameof(Spirit.NoLinkedCharacter));
            nudForce.DoOneWayDataBinding("Maximum", _objSpirit.CharacterObject, blnIsSpirit ? nameof(Character.MaxSpiritForce) : nameof(Character.MaxSpriteLevel));
            nudServices.DoDataBinding("Value", _objSpirit, nameof(Spirit.ServicesOwed));
            nudForce.DoDataBinding("Value", _objSpirit, nameof(Spirit.Force));
            chkFettered.DoOneWayDataBinding("Enabled", _objSpirit, nameof(Spirit.AllowFettering));
            chkFettered.DoDataBinding("Checked", _objSpirit, nameof(Spirit.Fettered));
            if (blnIsSpirit)
            {
                lblForce.Text = LanguageManager.GetString("Label_Spirit_Force");
                chkBound.Text = LanguageManager.GetString("Checkbox_Spirit_Bound");
                cmdLink.ToolTipText = LanguageManager.GetString(!string.IsNullOrEmpty(_objSpirit.FileName) ? "Tip_Spirit_OpenFile" : "Tip_Spirit_LinkSpirit");

                string strTooltip = LanguageManager.GetString("Tip_Spirit_EditNotes");
                if (!string.IsNullOrEmpty(_objSpirit.Notes))
                    strTooltip += Environment.NewLine + Environment.NewLine + _objSpirit.Notes;
                cmdNotes.ToolTipText = strTooltip.WordWrap();
            }
            else
            {
                lblForce.Text = LanguageManager.GetString("Label_Sprite_Rating");
                lblServices.Text = LanguageManager.GetString("Label_Sprite_TasksOwed");
                chkBound.Text = LanguageManager.GetString("Label_Sprite_Registered");
                chkFettered.Text = LanguageManager.GetString("Checkbox_Sprite_Pet");
                cmdLink.ToolTipText = LanguageManager.GetString(!string.IsNullOrEmpty(_objSpirit.FileName) ? "Tip_Sprite_OpenFile" : "Tip_Sprite_LinkSpirit");

                string strTooltip = LanguageManager.GetString("Tip_Sprite_EditNotes");
                if (!string.IsNullOrEmpty(_objSpirit.Notes))
                    strTooltip += Environment.NewLine + Environment.NewLine + _objSpirit.Notes;
                cmdNotes.ToolTipText = strTooltip.WordWrap();
            }

            _objSpirit.CharacterObject.PropertyChanged += RebuildSpiritListOnTraditionChange;

            _blnLoading = false;
        }

        public void UnbindSpiritControl()
        {
            _objSpirit.CharacterObject.PropertyChanged -= RebuildSpiritListOnTraditionChange;

            foreach (Control objControl in Controls)
            {
                objControl.DataBindings.Clear();
            }
        }

        private void chkFettered_CheckedChanged(object sender, EventArgs e)
        {
            // Raise the ContactDetailChanged Event when the Checkbox's Checked status changes.
            // The entire SpiritControl is passed as an argument so the handling event can evaluate its contents.
            if (!_blnLoading)
                ContactDetailChanged?.Invoke(this, e);
        }

        private void nudServices_ValueChanged(object sender, EventArgs e)
        {
            // Raise the ContactDetailChanged Event when the NumericUpDown's Value changes.
            // The entire SpiritControl is passed as an argument so the handling event can evaluate its contents.
            if (!_blnLoading)
                ContactDetailChanged?.Invoke(this, e);
        }

        private void cmdDelete_Click(object sender, EventArgs e)
        {
            // Raise the DeleteSpirit Event when the user has confirmed their desire to delete the Spirit.
            // The entire SpiritControl is passed as an argument so the handling event can evaluate its contents.
            DeleteSpirit?.Invoke(this, e);
        }

        private void nudForce_ValueChanged(object sender, EventArgs e)
        {
            // Raise the ContactDetailChanged Event when the NumericUpDown's Value changes.
            // The entire SpiritControl is passed as an argument so the handling event can evaluate its contents.
            if (!_blnLoading)
                ContactDetailChanged?.Invoke(this, e);
        }

        private void chkBound_CheckedChanged(object sender, EventArgs e)
        {
            // Raise the ContactDetailChanged Event when the Checkbox's Checked status changes.
            // The entire SpiritControl is passed as an argument so the handling event can evaluate its contents.
            if (!_blnLoading)
                ContactDetailChanged?.Invoke(this, e);
        }

        private void cboSpiritName_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!_blnLoading)
                ContactDetailChanged?.Invoke(this, e);
        }

        private void txtCritterName_TextChanged(object sender, EventArgs e)
        {
            if (!_blnLoading)
                ContactDetailChanged?.Invoke(this, e);
        }

        private void tsContactOpen_Click(object sender, EventArgs e)
        {
            if (_objSpirit.LinkedCharacter != null)
            {
                Character objOpenCharacter = Program.MainForm.OpenCharacters.Contains(_objSpirit.LinkedCharacter)
                    ? _objSpirit.LinkedCharacter
                    : null;
                using (new CursorWait(this))
                {
                    if (objOpenCharacter == null || !Program.MainForm.SwitchToOpenCharacter(objOpenCharacter, true))
                    {
                        objOpenCharacter = Program.MainForm.LoadCharacter(_objSpirit.LinkedCharacter.FileName);
                        Program.MainForm.OpenCharacter(objOpenCharacter);
                    }
                }
            }
            else
            {
                bool blnUseRelative = false;

                // Make sure the file still exists before attempting to load it.
                if (!File.Exists(_objSpirit.FileName))
                {
                    bool blnError = false;
                    // If the file doesn't exist, use the relative path if one is available.
                    if (string.IsNullOrEmpty(_objSpirit.RelativeFileName))
                        blnError = true;
                    else if (!File.Exists(Path.GetFullPath(_objSpirit.RelativeFileName)))
                        blnError = true;
                    else
                        blnUseRelative = true;

                    if (blnError)
                    {
                        Program.MainForm.ShowMessageBox(string.Format(GlobalSettings.CultureInfo, LanguageManager.GetString("Message_FileNotFound"), _objSpirit.FileName), LanguageManager.GetString("MessageTitle_FileNotFound"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }
                string strFile = blnUseRelative ? Path.GetFullPath(_objSpirit.RelativeFileName) : _objSpirit.FileName;
                System.Diagnostics.Process.Start(strFile);
            }
        }

        private void tsRemoveCharacter_Click(object sender, EventArgs e)
        {
            // Remove the file association from the Contact.
            if (Program.MainForm.ShowMessageBox(LanguageManager.GetString("Message_RemoveCharacterAssociation"), LanguageManager.GetString("MessageTitle_RemoveCharacterAssociation"), MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                _objSpirit.FileName = string.Empty;
                _objSpirit.RelativeFileName = string.Empty;
                cmdLink.ToolTipText = LanguageManager.GetString(_objSpirit.EntityType == SpiritType.Spirit ? "Tip_Spirit_LinkSpirit" : "Tip_Sprite_LinkSprite");

                // Set the relative path.
                Uri uriApplication = new Uri(Utils.GetStartupPath);
                Uri uriFile = new Uri(_objSpirit.FileName);
                Uri uriRelative = uriApplication.MakeRelativeUri(uriFile);
                _objSpirit.RelativeFileName = "../" + uriRelative;

                ContactDetailChanged?.Invoke(this, e);
            }
        }

        private void tsAttachCharacter_Click(object sender, EventArgs e)
        {
            // Prompt the user to select a save file to associate with this Contact.
            using (OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = LanguageManager.GetString("DialogFilter_Chum5") + '|' + LanguageManager.GetString("DialogFilter_All")
            })
            {
                if (!string.IsNullOrEmpty(_objSpirit.FileName) && File.Exists(_objSpirit.FileName))
                {
                    openFileDialog.InitialDirectory = Path.GetDirectoryName(_objSpirit.FileName);
                    openFileDialog.FileName = Path.GetFileName(_objSpirit.FileName);
                }

                if (openFileDialog.ShowDialog(this) == DialogResult.OK)
                {
                    _objSpirit.FileName = openFileDialog.FileName;
                    cmdLink.ToolTipText = LanguageManager.GetString(_objSpirit.EntityType == SpiritType.Spirit ? "Tip_Spirit_OpenFile" : "Tip_Sprite_OpenFile");
                    ContactDetailChanged?.Invoke(this, e);
                }
            }
        }

        private void tsCreateCharacter_Click(object sender, EventArgs e)
        {
            string strSpiritName = cboSpiritName.SelectedValue?.ToString();
            if (string.IsNullOrEmpty(strSpiritName))
            {
                Program.MainForm.ShowMessageBox(LanguageManager.GetString("Message_SelectCritterType"), LanguageManager.GetString("MessageTitle_SelectCritterType"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            CreateCritter(strSpiritName, nudForce.ValueAsInt);
        }

        private void cmdLink_Click(object sender, EventArgs e)
        {
            // Determine which options should be shown based on the FileName value.
            if (!string.IsNullOrEmpty(_objSpirit.FileName))
            {
                tsAttachCharacter.Visible = false;
                tsCreateCharacter.Visible = false;
                tsContactOpen.Visible = true;
                tsRemoveCharacter.Visible = true;
            }
            else
            {
                tsAttachCharacter.Visible = true;
                tsCreateCharacter.Visible = true;
                tsContactOpen.Visible = false;
                tsRemoveCharacter.Visible = false;
            }
            cmsSpirit.Show(cmdLink, cmdLink.Left - 646, cmdLink.Top);
        }

        private void cmdNotes_Click(object sender, EventArgs e)
        {
            using (EditNotes frmSpritNotes = new EditNotes(_objSpirit.Notes, _objSpirit.NotesColor))
            {
                frmSpritNotes.ShowDialogSafe(this);
                if (frmSpritNotes.DialogResult != DialogResult.OK)
                    return;
                _objSpirit.Notes = frmSpritNotes.Notes;
            }

            string strTooltip = LanguageManager.GetString(_objSpirit.EntityType == SpiritType.Spirit ? "Tip_Spirit_EditNotes" : "Tip_Sprite_EditNotes");

            if (!string.IsNullOrEmpty(_objSpirit.Notes))
                strTooltip += Environment.NewLine + Environment.NewLine + _objSpirit.Notes;
            cmdNotes.ToolTipText = strTooltip.WordWrap();

            ContactDetailChanged?.Invoke(this, e);
        }

        #endregion Control Events

        #region Properties

        /// <summary>
        /// Spirit object this is linked to.
        /// </summary>
        public Spirit SpiritObject => _objSpirit;

        #endregion Properties

        #region Methods

        // Rebuild the list of Spirits/Sprites based on the character's selected Tradition/Stream.
        public void RebuildSpiritListOnTraditionChange(object sender, PropertyChangedEventArgs e)
        {
            if (e?.PropertyName == nameof(Character.MagicTradition))
            {
                RebuildSpiritList(_objSpirit.CharacterObject.MagicTradition);
            }
        }

        // Rebuild the list of Spirits/Sprites based on the character's selected Tradition/Stream.
        public void RebuildSpiritList(Tradition objTradition)
        {
            if (objTradition == null)
                return;
            string strCurrentValue = cboSpiritName.SelectedValue?.ToString() ?? _objSpirit.Name;

            XPathNavigator objXmlDocument = _objSpirit.CharacterObject.LoadDataXPath(_objSpirit.EntityType == SpiritType.Spirit
                    ? "traditions.xml"
                    : "streams.xml");

            using (new FetchSafelyFromPool<HashSet<string>>(Utils.StringHashSetPool,
                                                            out HashSet<string> setLimitCategories))
            {
                foreach (Improvement objImprovement in ImprovementManager.GetCachedImprovementListForValueOf(
                             _objSpirit.CharacterObject, Improvement.ImprovementType.LimitSpiritCategory))
                {
                    setLimitCategories.Add(objImprovement.ImprovedName);
                }

                using (new FetchSafelyFromPool<List<ListItem>>(Utils.ListItemListPool, out List<ListItem> lstCritters))
                {
                    if (objTradition.IsCustomTradition)
                    {
                        string strSpiritCombat = objTradition.SpiritCombat;
                        string strSpiritDetection = objTradition.SpiritDetection;
                        string strSpiritHealth = objTradition.SpiritHealth;
                        string strSpiritIllusion = objTradition.SpiritIllusion;
                        string strSpiritManipulation = objTradition.SpiritManipulation;

                        if ((setLimitCategories.Count == 0 || setLimitCategories.Contains(strSpiritCombat))
                            && !string.IsNullOrWhiteSpace(strSpiritCombat))
                        {
                            XPathNavigator objXmlCritterNode
                                = objXmlDocument.SelectSingleNode(
                                    "/chummer/spirits/spirit[name = " + strSpiritCombat.CleanXPath() + "]");
                            lstCritters.Add(new ListItem(strSpiritCombat,
                                                         objXmlCritterNode
                                                             ?.SelectSingleNodeAndCacheExpression("translate")
                                                             ?.Value ?? strSpiritCombat));
                        }

                        if ((setLimitCategories.Count == 0 || setLimitCategories.Contains(strSpiritDetection))
                            && !string.IsNullOrWhiteSpace(strSpiritDetection))
                        {
                            XPathNavigator objXmlCritterNode
                                = objXmlDocument.SelectSingleNode(
                                    "/chummer/spirits/spirit[name = " + strSpiritDetection.CleanXPath() + "]");
                            lstCritters.Add(new ListItem(strSpiritDetection,
                                                         objXmlCritterNode
                                                             ?.SelectSingleNodeAndCacheExpression("translate")
                                                             ?.Value ?? strSpiritDetection));
                        }

                        if ((setLimitCategories.Count == 0 || setLimitCategories.Contains(strSpiritHealth))
                            && !string.IsNullOrWhiteSpace(strSpiritHealth))
                        {
                            XPathNavigator objXmlCritterNode
                                = objXmlDocument.SelectSingleNode(
                                    "/chummer/spirits/spirit[name = " + strSpiritHealth.CleanXPath() + "]");
                            lstCritters.Add(new ListItem(strSpiritHealth,
                                                         objXmlCritterNode
                                                             ?.SelectSingleNodeAndCacheExpression("translate")
                                                             ?.Value ?? strSpiritHealth));
                        }

                        if ((setLimitCategories.Count == 0 || setLimitCategories.Contains(strSpiritIllusion))
                            && !string.IsNullOrWhiteSpace(strSpiritIllusion))
                        {
                            XPathNavigator objXmlCritterNode
                                = objXmlDocument.SelectSingleNode(
                                    "/chummer/spirits/spirit[name = " + strSpiritIllusion.CleanXPath() + "]");
                            lstCritters.Add(new ListItem(strSpiritIllusion,
                                                         objXmlCritterNode
                                                             ?.SelectSingleNodeAndCacheExpression("translate")
                                                             ?.Value ?? strSpiritIllusion));
                        }

                        if ((setLimitCategories.Count == 0 || setLimitCategories.Contains(strSpiritManipulation))
                            && !string.IsNullOrWhiteSpace(strSpiritManipulation))
                        {
                            XPathNavigator objXmlCritterNode
                                = objXmlDocument.SelectSingleNode(
                                    "/chummer/spirits/spirit[name = " + strSpiritManipulation.CleanXPath() + ']');
                            lstCritters.Add(new ListItem(strSpiritManipulation,
                                                         objXmlCritterNode
                                                             ?.SelectSingleNodeAndCacheExpression("translate")
                                                             ?.Value ?? strSpiritManipulation));
                        }
                    }
                    else
                    {
                        if (objTradition.GetNodeXPath()?.SelectSingleNode("spirits/spirit[. = \"All\"]") != null)
                        {
                            if (setLimitCategories.Count == 0)
                            {
                                foreach (XPathNavigator objXmlCritterNode in objXmlDocument.SelectAndCacheExpression(
                                             "/chummer/spirits/spirit"))
                                {
                                    string strSpiritName = objXmlCritterNode.SelectSingleNodeAndCacheExpression("name")
                                                                            ?.Value;
                                    lstCritters.Add(new ListItem(strSpiritName,
                                                                 objXmlCritterNode
                                                                     .SelectSingleNodeAndCacheExpression("translate")
                                                                     ?.Value
                                                                 ?? strSpiritName));
                                }
                            }
                            else
                            {
                                foreach (string strSpiritName in setLimitCategories)
                                {
                                    XPathNavigator objXmlCritterNode
                                        = objXmlDocument.SelectSingleNode(
                                            "/chummer/spirits/spirit[name = " + strSpiritName.CleanXPath() + ']');
                                    lstCritters.Add(new ListItem(strSpiritName,
                                                                 objXmlCritterNode
                                                                     ?.SelectSingleNodeAndCacheExpression("translate")
                                                                     ?.Value ?? strSpiritName));
                                }
                            }
                        }
                        else
                        {
                            XPathNodeIterator xmlSpiritList = objTradition.GetNodeXPath()?.Select("spirits/*");
                            if (xmlSpiritList != null)
                            {
                                foreach (XPathNavigator objXmlSpirit in xmlSpiritList)
                                {
                                    string strSpiritName = objXmlSpirit.Value;
                                    if (setLimitCategories.Count == 0 || setLimitCategories.Contains(strSpiritName))
                                    {
                                        XPathNavigator objXmlCritterNode
                                            = objXmlDocument.SelectSingleNode(
                                                "/chummer/spirits/spirit[name = " + strSpiritName.CleanXPath()
                                                + ']');
                                        lstCritters.Add(new ListItem(strSpiritName,
                                                                     objXmlCritterNode
                                                                         ?.SelectSingleNodeAndCacheExpression(
                                                                             "translate")?.Value ?? strSpiritName));
                                    }
                                }
                            }
                        }
                    }

                    // Add any additional Spirits and Sprites the character has Access to through improvements.

                    if (_objSpirit.CharacterObject.MAGEnabled)
                    {
                        foreach (Improvement objImprovement in ImprovementManager.GetCachedImprovementListForValueOf(
                                     _objSpirit.CharacterObject, Improvement.ImprovementType.AddSpirit))
                        {
                            string strImprovedName = objImprovement.ImprovedName;
                            if (!string.IsNullOrEmpty(strImprovedName))
                            {
                                lstCritters.Add(new ListItem(strImprovedName,
                                                             objXmlDocument
                                                                 .SelectSingleNode(
                                                                     "/chummer/spirits/spirit[name = "
                                                                     + strImprovedName.CleanXPath() + "]/translate")
                                                                 ?.Value
                                                             ?? strImprovedName));
                            }
                        }
                    }

                    if (_objSpirit.CharacterObject.RESEnabled)
                    {
                        foreach (Improvement objImprovement in ImprovementManager.GetCachedImprovementListForValueOf(
                                     _objSpirit.CharacterObject, Improvement.ImprovementType.AddSprite))
                        {
                            string strImprovedName = objImprovement.ImprovedName;
                            if (!string.IsNullOrEmpty(strImprovedName))
                            {
                                lstCritters.Add(new ListItem(strImprovedName,
                                                             objXmlDocument
                                                                 .SelectSingleNode(
                                                                     "/chummer/spirits/spirit[name = "
                                                                     + strImprovedName.CleanXPath() + "]/translate")
                                                                 ?.Value
                                                             ?? strImprovedName));
                            }
                        }
                    }

                    cboSpiritName.BeginUpdate();
                    cboSpiritName.PopulateWithListItems(lstCritters);
                    // Set the control back to its original value.
                    cboSpiritName.SelectedValue = strCurrentValue;
                    cboSpiritName.EndUpdate();
                }
            }
        }

        /// <summary>
        /// Create a Critter, put them into Career Mode, link them, and open the newly-created Critter.
        /// </summary>
        /// <param name="strCritterName">Name of the Critter's Metatype.</param>
        /// <param name="intForce">Critter's Force.</param>
        private void CreateCritter(string strCritterName, int intForce)
        {
            // Code from frmMetatype.
            XmlDocument objXmlDocument = _objSpirit.CharacterObject.LoadData("critters.xml");

            XmlNode objXmlMetatype = objXmlDocument.SelectSingleNode("/chummer/metatypes/metatype[name = " + strCritterName.CleanXPath() + "]");

            // If the Critter could not be found, show an error and get out of here.
            if (objXmlMetatype == null)
            {
                Program.MainForm.ShowMessageBox(string.Format(GlobalSettings.CultureInfo, LanguageManager.GetString("Message_UnknownCritterType"), strCritterName), LanguageManager.GetString("MessageTitle_SelectCritterType"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            using (new CursorWait(this))
            {
                // The Critter should use the same settings file as the character.
                using (Character objCharacter = new Character
                {
                    SettingsKey = _objSpirit.CharacterObject.SettingsKey,
                    // Override the defaults for the setting.
                    IgnoreRules = true,
                    IsCritter = true,
                    Alias = strCritterName,
                    Created = true
                })
                {
                    if (!string.IsNullOrEmpty(txtCritterName.Text))
                        objCharacter.Name = txtCritterName.Text;

                    string strSpace = LanguageManager.GetString("String_Space");
                    using (SaveFileDialog saveFileDialog = new SaveFileDialog
                    {
                        Filter = LanguageManager.GetString("DialogFilter_Chum5") + '|' + LanguageManager.GetString("DialogFilter_All"),
                        FileName = strCritterName + strSpace + '(' + LanguageManager.GetString(_objSpirit.RatingLabel) + strSpace + _objSpirit.Force.ToString(GlobalSettings.InvariantCultureInfo) + ").chum5"
                    })
                    {
                        if (saveFileDialog.ShowDialog(this) != DialogResult.OK)
                            return;
                        string strFileName = saveFileDialog.FileName;
                        objCharacter.FileName = strFileName;
                    }

                    objCharacter.Create(objXmlMetatype["category"]?.InnerText, objXmlMetatype["id"]?.InnerText, string.Empty, objXmlMetatype, intForce);
                    objCharacter.MetatypeBP = 0;
                    using (LoadingBar frmProgressBar = ChummerMainForm.CreateAndShowProgressBar())
                    {
                        frmProgressBar.PerformStep(objCharacter.CharacterName, LoadingBar.ProgressBarTextPatterns.Saving);
                        if (!objCharacter.Save())
                            return;
                    }

                    // Link the newly-created Critter to the Spirit.
                    cmdLink.ToolTipText = LanguageManager.GetString(_objSpirit.EntityType == SpiritType.Spirit ? "Tip_Spirit_OpenFile" : "Tip_Sprite_OpenFile");
                    ContactDetailChanged?.Invoke(this, EventArgs.Empty);

                    Program.MainForm.OpenCharacter(objCharacter);
                }
            }
        }

        #endregion Methods
    }
}
