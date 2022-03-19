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
using System.Threading.Tasks;
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

        private async void SpiritControl_Load(object sender, EventArgs e)
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
                string strText = await LanguageManager.GetStringAsync("Label_Spirit_Force");
                await lblForce.DoThreadSafeAsync(x => x.Text = strText);
                strText = await LanguageManager.GetStringAsync("Checkbox_Spirit_Bound");
                await chkBound.DoThreadSafeAsync(x => x.Text = strText);
                await cmdLink.SetToolTipTextAsync(await LanguageManager.GetStringAsync(!string.IsNullOrEmpty(_objSpirit.FileName) ? "Tip_Spirit_OpenFile" : "Tip_Spirit_LinkSpirit"));
                string strTooltip = await LanguageManager.GetStringAsync("Tip_Spirit_EditNotes");
                if (!string.IsNullOrEmpty(_objSpirit.Notes))
                    strTooltip += Environment.NewLine + Environment.NewLine + _objSpirit.Notes;
                await cmdNotes.SetToolTipTextAsync(strTooltip.WordWrap());
            }
            else
            {
                string strText = await LanguageManager.GetStringAsync("Label_Sprite_Rating");
                await lblForce.DoThreadSafeAsync(x => x.Text = strText);
                strText = await LanguageManager.GetStringAsync("Label_Sprite_TasksOwed");
                await lblServices.DoThreadSafeAsync(x => x.Text = strText);
                strText = await LanguageManager.GetStringAsync("Label_Sprite_Registered");
                await chkBound.DoThreadSafeAsync(x => x.Text = strText);
                strText = await LanguageManager.GetStringAsync("Checkbox_Sprite_Pet");
                await chkFettered.DoThreadSafeAsync(x => x.Text = strText);
                await cmdLink.SetToolTipTextAsync(await LanguageManager.GetStringAsync(!string.IsNullOrEmpty(_objSpirit.FileName) ? "Tip_Sprite_OpenFile" : "Tip_Sprite_LinkSpirit"));
                string strTooltip = await LanguageManager.GetStringAsync("Tip_Sprite_EditNotes");
                if (!string.IsNullOrEmpty(_objSpirit.Notes))
                    strTooltip += Environment.NewLine + Environment.NewLine + _objSpirit.Notes;
                await cmdNotes.SetToolTipTextAsync(strTooltip.WordWrap());
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

        private async void tsContactOpen_Click(object sender, EventArgs e)
        {
            if (_objSpirit.LinkedCharacter != null)
            {
                Character objOpenCharacter = await Program.OpenCharacters.ContainsAsync(_objSpirit.LinkedCharacter)
                    ? _objSpirit.LinkedCharacter
                    : null;
                using (CursorWait.New(ParentForm))
                {
                    if (objOpenCharacter == null)
                        objOpenCharacter = await Program.LoadCharacterAsync(_objSpirit.LinkedCharacter.FileName);
                    if (!await Program.SwitchToOpenCharacter(objOpenCharacter))
                        await Program.OpenCharacter(objOpenCharacter);
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
                        Program.ShowMessageBox(string.Format(GlobalSettings.CultureInfo, await LanguageManager.GetStringAsync("Message_FileNotFound"), _objSpirit.FileName), await LanguageManager.GetStringAsync("MessageTitle_FileNotFound"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }
                string strFile = blnUseRelative ? Path.GetFullPath(_objSpirit.RelativeFileName) : _objSpirit.FileName;
                System.Diagnostics.Process.Start(strFile);
            }
        }

        private async void tsRemoveCharacter_Click(object sender, EventArgs e)
        {
            // Remove the file association from the Contact.
            if (Program.ShowMessageBox(await LanguageManager.GetStringAsync("Message_RemoveCharacterAssociation"), await LanguageManager.GetStringAsync("MessageTitle_RemoveCharacterAssociation"), MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                _objSpirit.FileName = string.Empty;
                _objSpirit.RelativeFileName = string.Empty;
                string strText = await LanguageManager.GetStringAsync(
                    _objSpirit.EntityType == SpiritType.Spirit ? "Tip_Spirit_LinkSpirit" : "Tip_Sprite_LinkSprite");
                await cmdLink.SetToolTipTextAsync(strText);

                // Set the relative path.
                Uri uriApplication = new Uri(Utils.GetStartupPath);
                Uri uriFile = new Uri(_objSpirit.FileName);
                Uri uriRelative = uriApplication.MakeRelativeUri(uriFile);
                _objSpirit.RelativeFileName = "../" + uriRelative;

                ContactDetailChanged?.Invoke(this, e);
            }
        }

        private async void tsAttachCharacter_Click(object sender, EventArgs e)
        {
            // Prompt the user to select a save file to associate with this Contact.
            using (OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = await LanguageManager.GetStringAsync("DialogFilter_Chum5") + '|' + await LanguageManager.GetStringAsync("DialogFilter_All")
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
                    string strText = await LanguageManager.GetStringAsync(
                        _objSpirit.EntityType == SpiritType.Spirit ? "Tip_Spirit_OpenFile" : "Tip_Sprite_OpenFile");
                    await cmdLink.SetToolTipTextAsync(strText);
                    ContactDetailChanged?.Invoke(this, e);
                }
            }
        }

        private async void tsCreateCharacter_Click(object sender, EventArgs e)
        {
            string strSpiritName = await cboSpiritName.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString());
            if (string.IsNullOrEmpty(strSpiritName))
            {
                Program.ShowMessageBox(await LanguageManager.GetStringAsync("Message_SelectCritterType"), await LanguageManager.GetStringAsync("MessageTitle_SelectCritterType"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            await CreateCritter(strSpiritName, nudForce.ValueAsInt);
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

        private async void cmdNotes_Click(object sender, EventArgs e)
        {
            using (EditNotes frmSpritNotes = new EditNotes(_objSpirit.Notes, _objSpirit.NotesColor))
            {
                await frmSpritNotes.ShowDialogSafeAsync(this);
                if (frmSpritNotes.DialogResult != DialogResult.OK)
                    return;
                _objSpirit.Notes = frmSpritNotes.Notes;
            }

            string strTooltip = await LanguageManager.GetStringAsync(_objSpirit.EntityType == SpiritType.Spirit ? "Tip_Spirit_EditNotes" : "Tip_Sprite_EditNotes");

            if (!string.IsNullOrEmpty(_objSpirit.Notes))
                strTooltip += Environment.NewLine + Environment.NewLine + _objSpirit.Notes;
            await cmdNotes.SetToolTipTextAsync(strTooltip.WordWrap());

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
        public async void RebuildSpiritListOnTraditionChange(object sender, PropertyChangedEventArgs e)
        {
            if (e?.PropertyName == nameof(Character.MagicTradition))
            {
                await RebuildSpiritList(_objSpirit.CharacterObject.MagicTradition);
            }
        }

        // Rebuild the list of Spirits/Sprites based on the character's selected Tradition/Stream.
        public async ValueTask RebuildSpiritList(Tradition objTradition)
        {
            if (objTradition == null)
                return;
            string strCurrentValue = await cboSpiritName.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString()) ?? _objSpirit.Name;

            XPathNavigator objXmlDocument = await _objSpirit.CharacterObject.LoadDataXPathAsync(_objSpirit.EntityType == SpiritType.Spirit
                ? "traditions.xml"
                : "streams.xml");

            using (new FetchSafelyFromPool<HashSet<string>>(Utils.StringHashSetPool,
                                                            out HashSet<string> setLimitCategories))
            {
                foreach (Improvement objImprovement in await ImprovementManager.GetCachedImprovementListForValueOfAsync(
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
                                    "/chummer/spirits/spirit[name = " + strSpiritCombat.CleanXPath() + ']');
                            string strTranslatedName = objXmlCritterNode != null
                                ? (await objXmlCritterNode.SelectSingleNodeAndCacheExpressionAsync("translate"))?.Value
                                  ?? strSpiritCombat
                                : strSpiritCombat;
                            lstCritters.Add(new ListItem(strSpiritCombat, strTranslatedName));
                        }

                        if ((setLimitCategories.Count == 0 || setLimitCategories.Contains(strSpiritDetection))
                            && !string.IsNullOrWhiteSpace(strSpiritDetection))
                        {
                            XPathNavigator objXmlCritterNode
                                = objXmlDocument.SelectSingleNode(
                                    "/chummer/spirits/spirit[name = " + strSpiritDetection.CleanXPath() + ']');
                            string strTranslatedName = objXmlCritterNode != null
                                ? (await objXmlCritterNode.SelectSingleNodeAndCacheExpressionAsync("translate"))?.Value
                                  ?? strSpiritDetection
                                : strSpiritDetection;
                            lstCritters.Add(new ListItem(strSpiritDetection, strTranslatedName));
                        }

                        if ((setLimitCategories.Count == 0 || setLimitCategories.Contains(strSpiritHealth))
                            && !string.IsNullOrWhiteSpace(strSpiritHealth))
                        {
                            XPathNavigator objXmlCritterNode
                                = objXmlDocument.SelectSingleNode(
                                    "/chummer/spirits/spirit[name = " + strSpiritHealth.CleanXPath() + ']');
                            string strTranslatedName = objXmlCritterNode != null
                                ? (await objXmlCritterNode.SelectSingleNodeAndCacheExpressionAsync("translate"))?.Value
                                  ?? strSpiritHealth
                                : strSpiritHealth;
                            lstCritters.Add(new ListItem(strSpiritHealth, strTranslatedName));
                        }

                        if ((setLimitCategories.Count == 0 || setLimitCategories.Contains(strSpiritIllusion))
                            && !string.IsNullOrWhiteSpace(strSpiritIllusion))
                        {
                            XPathNavigator objXmlCritterNode
                                = objXmlDocument.SelectSingleNode(
                                    "/chummer/spirits/spirit[name = " + strSpiritIllusion.CleanXPath() + ']');
                            string strTranslatedName = objXmlCritterNode != null
                                ? (await objXmlCritterNode.SelectSingleNodeAndCacheExpressionAsync("translate"))?.Value
                                  ?? strSpiritIllusion
                                : strSpiritIllusion;
                            lstCritters.Add(new ListItem(strSpiritIllusion, strTranslatedName));
                        }

                        if ((setLimitCategories.Count == 0 || setLimitCategories.Contains(strSpiritManipulation))
                            && !string.IsNullOrWhiteSpace(strSpiritManipulation))
                        {
                            XPathNavigator objXmlCritterNode
                                = objXmlDocument.SelectSingleNode(
                                    "/chummer/spirits/spirit[name = " + strSpiritManipulation.CleanXPath() + ']');
                            string strTranslatedName = objXmlCritterNode != null
                                ? (await objXmlCritterNode.SelectSingleNodeAndCacheExpressionAsync("translate"))?.Value
                                  ?? strSpiritManipulation
                                : strSpiritManipulation;
                            lstCritters.Add(new ListItem(strSpiritManipulation, strTranslatedName));
                        }
                    }
                    else
                    {
                        if ((await objTradition.GetNodeXPathAsync())?.SelectSingleNode("spirits/spirit[. = \"All\"]") != null)
                        {
                            if (setLimitCategories.Count == 0)
                            {
                                foreach (XPathNavigator objXmlCritterNode in await objXmlDocument.SelectAndCacheExpressionAsync(
                                             "/chummer/spirits/spirit"))
                                {
                                    string strSpiritName = (await objXmlCritterNode.SelectSingleNodeAndCacheExpressionAsync("name"))
                                                                            ?.Value;
                                    lstCritters.Add(new ListItem(strSpiritName,
                                                                 (await objXmlCritterNode
                                                                     .SelectSingleNodeAndCacheExpressionAsync("translate"))
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
                                    string strTranslatedName = objXmlCritterNode != null
                                        ? (await objXmlCritterNode.SelectSingleNodeAndCacheExpressionAsync("translate"))?.Value
                                          ?? strSpiritName
                                        : strSpiritName;
                                    lstCritters.Add(new ListItem(strSpiritName, strTranslatedName));
                                }
                            }
                        }
                        else
                        {
                            XPathNodeIterator xmlSpiritList = (await objTradition.GetNodeXPathAsync())?.Select("spirits/*");
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
                                        string strTranslatedName = objXmlCritterNode != null
                                            ? (await objXmlCritterNode.SelectSingleNodeAndCacheExpressionAsync("translate"))?.Value
                                              ?? strSpiritName
                                            : strSpiritName;
                                        lstCritters.Add(new ListItem(strSpiritName, strTranslatedName));
                                    }
                                }
                            }
                        }
                    }

                    // Add any additional Spirits and Sprites the character has Access to through improvements.

                    if (_objSpirit.CharacterObject.MAGEnabled)
                    {
                        foreach (Improvement objImprovement in await ImprovementManager.GetCachedImprovementListForValueOfAsync(
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
                        foreach (Improvement objImprovement in await ImprovementManager.GetCachedImprovementListForValueOfAsync(
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
                    
                    await cboSpiritName.PopulateWithListItemsAsync(lstCritters);
                    // Set the control back to its original value.
                    await cboSpiritName.DoThreadSafeAsync(x => x.SelectedValue = strCurrentValue);
                }
            }
        }

        /// <summary>
        /// Create a Critter, put them into Career Mode, link them, and open the newly-created Critter.
        /// </summary>
        /// <param name="strCritterName">Name of the Critter's Metatype.</param>
        /// <param name="intForce">Critter's Force.</param>
        private async ValueTask CreateCritter(string strCritterName, int intForce)
        {
            // Code from frmMetatype.
            XmlDocument objXmlDocument = await _objSpirit.CharacterObject.LoadDataAsync("critters.xml");

            XmlNode objXmlMetatype = objXmlDocument.SelectSingleNode("/chummer/metatypes/metatype[name = " + strCritterName.CleanXPath() + ']');

            // If the Critter could not be found, show an error and get out of here.
            if (objXmlMetatype == null)
            {
                Program.ShowMessageBox(string.Format(GlobalSettings.CultureInfo, await LanguageManager.GetStringAsync("Message_UnknownCritterType"), strCritterName), await LanguageManager.GetStringAsync("MessageTitle_SelectCritterType"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            using (CursorWait.New(ParentForm))
            {
                // The Critter should use the same settings file as the character.
                Character objCharacter = new Character
                {
                    SettingsKey = _objSpirit.CharacterObject.SettingsKey,
                    // Override the defaults for the setting.
                    IgnoreRules = true,
                    IsCritter = true,
                    Alias = strCritterName,
                    Created = true
                };
                try
                {
                    string strCritterCharacterName = await txtCritterName.DoThreadSafeFuncAsync(x => x.Text);
                    if (!string.IsNullOrEmpty(strCritterCharacterName))
                        objCharacter.Name = strCritterCharacterName;

                    string strSpace = await LanguageManager.GetStringAsync("String_Space");
                    using (SaveFileDialog saveFileDialog = new SaveFileDialog
                           {
                               Filter = await LanguageManager.GetStringAsync("DialogFilter_Chum5") + '|'
                                   + await LanguageManager.GetStringAsync("DialogFilter_All"),
                               FileName = strCritterName + strSpace + '('
                                          + await LanguageManager.GetStringAsync(_objSpirit.RatingLabel) + strSpace
                                          + _objSpirit.Force.ToString(GlobalSettings.InvariantCultureInfo) + ").chum5"
                           })
                    {
                        if (saveFileDialog.ShowDialog(this) != DialogResult.OK)
                            return;
                        string strFileName = saveFileDialog.FileName;
                        objCharacter.FileName = strFileName;
                    }

                    objCharacter.Create(objXmlMetatype["category"]?.InnerText, objXmlMetatype["id"]?.InnerText,
                                        string.Empty, objXmlMetatype, intForce);
                    objCharacter.MetatypeBP = 0;
                    using (Program.MainProgressBar = await Program.CreateAndShowProgressBarAsync())
                    {
                        Program.MainProgressBar.PerformStep(objCharacter.CharacterName,
                                                            LoadingBar.ProgressBarTextPatterns.Saving);
                        if (!await objCharacter.SaveAsync())
                            return;
                    }

                    // Link the newly-created Critter to the Spirit.
                    string strText = await LanguageManager.GetStringAsync(
                        _objSpirit.EntityType == SpiritType.Spirit ? "Tip_Spirit_OpenFile" : "Tip_Sprite_OpenFile");
                    await cmdLink.SetToolTipTextAsync(strText);
                    ContactDetailChanged?.Invoke(this, EventArgs.Empty);

                    await Program.OpenCharacter(objCharacter);
                }
                finally
                {
                    await objCharacter
                        .DisposeAsync(); // Fine here because Dispose()/DisposeAsync() code is skipped if the character is open in a form
                }
            }
        }

        #endregion Methods
    }
}
