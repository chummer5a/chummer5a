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
using System.Linq;
using System.Windows.Forms;
using System.Xml;
 using Chummer.Backend.Equipment;
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
            LanguageManager.TranslateWinForm(GlobalOptions.Language, this);
            foreach (ToolStripItem objItem in cmsSpirit.Items)
            {
                LanguageManager.TranslateToolStripItemsRecursively(objItem, GlobalOptions.Language);
            }
        }

        private void SpiritControl_Load(object sender, EventArgs e)
        {
            DoubleBuffered = true;
            bool blnIsSpirit = _objSpirit.EntityType == SpiritType.Spirit;
            nudForce.DataBindings.Add("Enabled", _objSpirit.CharacterObject, nameof(Character.Created), false,
                DataSourceUpdateMode.OnPropertyChanged);
            chkBound.DataBindings.Add("Checked", _objSpirit, nameof(_objSpirit.Bound), false,
                DataSourceUpdateMode.OnPropertyChanged);
            chkBound.DataBindings.Add("Enabled", _objSpirit.CharacterObject, nameof(Character.Created), false,
                DataSourceUpdateMode.OnPropertyChanged);
            cboSpiritName.DataBindings.Add("Text", _objSpirit, nameof(_objSpirit.Name), false,
                DataSourceUpdateMode.OnPropertyChanged);
            txtCritterName.DataBindings.Add("Text", _objSpirit, nameof(_objSpirit.CritterName), false,
                DataSourceUpdateMode.OnPropertyChanged);
            txtCritterName.DataBindings.Add("Enabled", _objSpirit, nameof(_objSpirit.NoLinkedCharacter), false,
                DataSourceUpdateMode.OnPropertyChanged);
            nudForce.DataBindings.Add("Maximum", _objSpirit.CharacterObject, blnIsSpirit ? nameof(Character.MaxSpiritForce) : nameof(Character.MaxSpriteLevel), false,
                DataSourceUpdateMode.OnPropertyChanged);
            nudServices.DataBindings.Add("Value", _objSpirit, nameof(_objSpirit.ServicesOwed), false,
                DataSourceUpdateMode.OnPropertyChanged);
            nudForce.DataBindings.Add("Value", _objSpirit, nameof(_objSpirit.Force), false,
                DataSourceUpdateMode.OnPropertyChanged);
            Width = cmdDelete.Left + cmdDelete.Width;

            if (blnIsSpirit)
            {
                chkFettered.DataBindings.Add("Checked", _objSpirit, nameof(_objSpirit.Fettered), false,
                    DataSourceUpdateMode.OnPropertyChanged);
                lblForce.Text = LanguageManager.GetString("Label_Spirit_Force", GlobalOptions.Language);
                chkBound.Text = LanguageManager.GetString("Checkbox_Spirit_Bound", GlobalOptions.Language);
                imgLink.SetToolTip(LanguageManager.GetString(!string.IsNullOrEmpty(_objSpirit.FileName) ? "Tip_Spirit_OpenFile" : "Tip_Spirit_LinkSpirit", GlobalOptions.Language));

                string strTooltip = LanguageManager.GetString("Tip_Spirit_EditNotes", GlobalOptions.Language);
                if (!string.IsNullOrEmpty(_objSpirit.Notes))
                    strTooltip += Environment.NewLine + Environment.NewLine + _objSpirit.Notes;
                imgNotes.SetToolTip(strTooltip.WordWrap(100));
            }
            else
            {
                chkFettered.Visible = false;
                lblForce.Text = LanguageManager.GetString("Label_Sprite_Rating", GlobalOptions.Language);
                chkBound.Text = LanguageManager.GetString("Label_Sprite_Registered", GlobalOptions.Language);
                imgLink.SetToolTip(LanguageManager.GetString(!string.IsNullOrEmpty(_objSpirit.FileName) ? "Tip_Sprite_OpenFile" : "Tip_Sprite_LinkSpirit", GlobalOptions.Language));

                string strTooltip = LanguageManager.GetString("Tip_Sprite_EditNotes", GlobalOptions.Language);
                if (!string.IsNullOrEmpty(_objSpirit.Notes))
                    strTooltip += Environment.NewLine + Environment.NewLine + _objSpirit.Notes;
                imgNotes.SetToolTip(strTooltip.WordWrap(100));
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
                Character objOpenCharacter = Program.MainForm.OpenCharacters.FirstOrDefault(x => x == _objSpirit.LinkedCharacter);
                Cursor = Cursors.WaitCursor;
                if (objOpenCharacter == null || !Program.MainForm.SwitchToOpenCharacter(objOpenCharacter, true))
                {
                    objOpenCharacter = Program.MainForm.LoadCharacter(_objSpirit.LinkedCharacter.FileName);
                    Program.MainForm.OpenCharacter(objOpenCharacter);
                }
                Cursor = Cursors.Default;
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
                        MessageBox.Show(string.Format(LanguageManager.GetString("Message_FileNotFound", GlobalOptions.Language), _objSpirit.FileName), LanguageManager.GetString("MessageTitle_FileNotFound", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            if (MessageBox.Show(LanguageManager.GetString("Message_RemoveCharacterAssociation", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_RemoveCharacterAssociation", GlobalOptions.Language), MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                _objSpirit.FileName = string.Empty;
                _objSpirit.RelativeFileName = string.Empty;
                imgLink.SetToolTip(LanguageManager.GetString(_objSpirit.EntityType == SpiritType.Spirit ? "Tip_Spirit_LinkSpirit" : "Tip_Sprite_LinkSprite", GlobalOptions.Language));

                // Set the relative path.
                Uri uriApplication = new Uri(Utils.GetStartupPath);
                Uri uriFile = new Uri(_objSpirit.FileName);
                Uri uriRelative = uriApplication.MakeRelativeUri(uriFile);
                _objSpirit.RelativeFileName = "../" + uriRelative.ToString();

                ContactDetailChanged?.Invoke(this, e);
            }
        }

        private void tsAttachCharacter_Click(object sender, EventArgs e)
        {
            // Prompt the user to select a save file to associate with this Contact.
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = LanguageManager.GetString("DialogFilter_Chum5", GlobalOptions.Language) + '|' + LanguageManager.GetString("DialogFilter_All", GlobalOptions.Language)
            };
            if (!string.IsNullOrEmpty(_objSpirit.FileName) && File.Exists(_objSpirit.FileName))
            {
                openFileDialog.InitialDirectory = Path.GetDirectoryName(_objSpirit.FileName);
                openFileDialog.FileName = Path.GetFileName(_objSpirit.FileName);
            }
            if (openFileDialog.ShowDialog(this) == DialogResult.OK)
            {
                _objSpirit.FileName = openFileDialog.FileName;
                imgLink.SetToolTip(LanguageManager.GetString(_objSpirit.EntityType == SpiritType.Spirit ? "Tip_Spirit_OpenFile" : "Tip_Sprite_OpenFile", GlobalOptions.Language));
                ContactDetailChanged?.Invoke(this, e);
            }
        }

        private void tsCreateCharacter_Click(object sender, EventArgs e)
        {
            string strSpiritName = cboSpiritName.SelectedValue?.ToString();
            if (string.IsNullOrEmpty(strSpiritName))
            {
                MessageBox.Show(LanguageManager.GetString("Message_SelectCritterType", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_SelectCritterType", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            CreateCritter(strSpiritName, decimal.ToInt32(nudForce.Value));
        }

        private void imgLink_Click(object sender, EventArgs e)
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
            cmsSpirit.Show(imgLink, imgLink.Left - 646, imgLink.Top);
        }

        private void imgNotes_Click(object sender, EventArgs e)
        {
            frmNotes frmSpritNotes = new frmNotes
            {
                Notes = _objSpirit.Notes
            };
            frmSpritNotes.ShowDialog(this);

            if (frmSpritNotes.DialogResult == DialogResult.OK && _objSpirit.Notes != frmSpritNotes.Notes)
            {
                _objSpirit.Notes = frmSpritNotes.Notes;

                string strTooltip = LanguageManager.GetString(_objSpirit.EntityType == SpiritType.Spirit ? "Tip_Spirit_EditNotes" : "Tip_Sprite_EditNotes", GlobalOptions.Language);

                if (!string.IsNullOrEmpty(_objSpirit.Notes))
                    strTooltip += Environment.NewLine + Environment.NewLine + _objSpirit.Notes;
                imgNotes.SetToolTip(strTooltip.WordWrap(100));

                ContactDetailChanged?.Invoke(this, e);
            }
        }
        #endregion

        #region Properties
        /// <summary>
        /// Spirit object this is linked to.
        /// </summary>
        public Spirit SpiritObject => _objSpirit;

        #endregion

        #region Methods
        // Rebuild the list of Spirits/Sprites based on the character's selected Tradition/Stream.
        public void RebuildSpiritListOnTraditionChange(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Character.MagicTradition))
            {
                RebuildSpiritList(_objSpirit.CharacterObject.MagicTradition);
            }
        }

        // Rebuild the list of Spirits/Sprites based on the character's selected Tradition/Stream.
        public void RebuildSpiritList(Tradition objTradition)
        {
            if (objTradition == null)
            {
                return;
            }
            string strCurrentValue = cboSpiritName.SelectedValue?.ToString() ?? _objSpirit.Name;

            XmlDocument objXmlDocument = _objSpirit.EntityType == SpiritType.Spirit ? XmlManager.Load("traditions.xml") : XmlManager.Load("streams.xml");
            XmlDocument objXmlCritterDocument = XmlManager.Load("critters.xml");

            HashSet<string> lstLimitCategories = new HashSet<string>();
            foreach (Improvement improvement in _objSpirit.CharacterObject.Improvements.Where(x => x.ImproveType == Improvement.ImprovementType.LimitSpiritCategory && x.Enabled))
            {
                lstLimitCategories.Add(improvement.ImprovedName);
            }

            List<ListItem> lstCritters = new List<ListItem>();
            if (objTradition.IsCustomTradition)
            {
                string strSpiritCombat = objTradition.SpiritCombat;
                string strSpiritDetection = objTradition.SpiritDetection;
                string strSpiritHealth = objTradition.SpiritHealth;
                string strSpiritIllusion = objTradition.SpiritIllusion;
                string strSpiritManipulation = objTradition.SpiritManipulation;

                if (lstLimitCategories.Count == 0 || lstLimitCategories.Contains(strSpiritCombat))
                {
                    XmlNode objXmlCritterNode = objXmlDocument.SelectSingleNode("/chummer/spirits/spirit[name = \"" + strSpiritCombat + "\"]");
                    lstCritters.Add(new ListItem(strSpiritCombat, objXmlCritterNode?["translate"]?.InnerText ?? strSpiritCombat));
                }

                if (lstLimitCategories.Count == 0 || lstLimitCategories.Contains(strSpiritDetection))
                {
                    XmlNode objXmlCritterNode = objXmlDocument.SelectSingleNode("/chummer/spirits/spirit[name = \"" + strSpiritDetection + "\"]");
                    lstCritters.Add(new ListItem(strSpiritDetection, objXmlCritterNode?["translate"]?.InnerText ?? strSpiritDetection));
                }

                if (lstLimitCategories.Count == 0 || lstLimitCategories.Contains(strSpiritHealth))
                {
                    XmlNode objXmlCritterNode = objXmlDocument.SelectSingleNode("/chummer/spirits/spirit[name = \"" + strSpiritHealth + "\"]");
                    lstCritters.Add(new ListItem(strSpiritHealth, objXmlCritterNode?["translate"]?.InnerText ?? strSpiritHealth));
                }

                if (lstLimitCategories.Count == 0 || lstLimitCategories.Contains(strSpiritIllusion))
                {
                    XmlNode objXmlCritterNode = objXmlDocument.SelectSingleNode("/chummer/spirits/spirit[name = \"" + strSpiritIllusion + "\"]");
                    lstCritters.Add(new ListItem(strSpiritIllusion, objXmlCritterNode?["translate"]?.InnerText ?? strSpiritIllusion));
                }

                if (lstLimitCategories.Count == 0 || lstLimitCategories.Contains(strSpiritManipulation))
                {
                    XmlNode objXmlCritterNode = objXmlDocument.SelectSingleNode("/chummer/spirits/spirit[name = \"" + strSpiritManipulation + "\"]");
                    lstCritters.Add(new ListItem(strSpiritManipulation, objXmlCritterNode?["translate"]?.InnerText ?? strSpiritManipulation));
                }
            }
            else
            {
                if (objTradition.GetNode()?.SelectSingleNode("spirits/spirit[. = \"All\"]") != null)
                {
                    if (lstLimitCategories.Count == 0)
                    {
                        using (XmlNodeList xmlSpiritList = objXmlDocument.SelectNodes("/chummer/spirits/spirit"))
                            if (xmlSpiritList != null)
                                foreach (XmlNode objXmlCritterNode in xmlSpiritList)
                                {
                                    string strSpiritName = objXmlCritterNode["name"]?.InnerText;
                                    lstCritters.Add(new ListItem(strSpiritName, objXmlCritterNode["translate"]?.InnerText ?? strSpiritName));
                                }
                    }
                    else
                    {
                        foreach (string strSpiritName in lstLimitCategories)
                        {
                            XmlNode objXmlCritterNode = objXmlDocument.SelectSingleNode("/chummer/spirits/spirit[name = \"" + strSpiritName + "\"]");
                            lstCritters.Add(new ListItem(strSpiritName, objXmlCritterNode?["translate"]?.InnerText ?? strSpiritName));
                        }
                    }
                }
                else
                {
                    using (XmlNodeList xmlSpiritList = objTradition.GetNode()?.SelectSingleNode("spirits")?.ChildNodes)
                        if (xmlSpiritList != null)
                            foreach (XmlNode objXmlSpirit in xmlSpiritList)
                            {
                                string strSpiritName = objXmlSpirit.InnerText;
                                if (lstLimitCategories.Count == 0 || lstLimitCategories.Contains(strSpiritName))
                                {
                                    XmlNode objXmlCritterNode = objXmlDocument.SelectSingleNode("/chummer/spirits/spirit[name = \"" + strSpiritName + "\"]");
                                    lstCritters.Add(new ListItem(strSpiritName, objXmlCritterNode?["translate"]?.InnerText ?? strSpiritName));
                                }
                            }
                }
            }

            if (_objSpirit.CharacterObject.RESEnabled)
            {
                // Add any additional Sprites the character has Access to through Sprite Link.
                foreach (Improvement objImprovement in _objSpirit.CharacterObject.Improvements)
                {
                    if (objImprovement.ImproveType == Improvement.ImprovementType.AddSprite && objImprovement.Enabled)
                    {
                        XmlNode objXmlCritterNode = objXmlDocument.SelectSingleNode("/chummer/spirits/spirit[name = \"" + objImprovement.ImprovedName + "\"]");
                        lstCritters.Add(new ListItem(objImprovement.ImprovedName, objXmlCritterNode?["translate"]?.InnerText ?? objImprovement.ImprovedName));
                    }
                }
            }

            //Add Ally Spirit to MAG-enabled traditions.
            if (_objSpirit.CharacterObject.MAGEnabled)
            {
                XmlNode objXmlCritterNode = objXmlCritterDocument.SelectSingleNode("/chummer/metatypes/metatype[name = \"Ally Spirit\"]");
                lstCritters.Add(new ListItem("Ally Spirit", objXmlCritterNode?["translate"]?.InnerText ?? "Ally Spirit"));
            }

            cboSpiritName.BeginUpdate();
            cboSpiritName.DisplayMember = "Name";
            cboSpiritName.ValueMember = "Value";
            cboSpiritName.DataSource = lstCritters;

            // Set the control back to its original value.
            cboSpiritName.SelectedValue = strCurrentValue;
            cboSpiritName.EndUpdate();
        }

        /// <summary>
        /// Create a Critter, put them into Career Mode, link them, and open the newly-created Critter.
        /// </summary>
        /// <param name="strCritterName">Name of the Critter's Metatype.</param>
        /// <param name="intForce">Critter's Force.</param>
        private void CreateCritter(string strCritterName, int intForce)
        {
            // Code from frmMetatype.
            XmlDocument objXmlDocument = XmlManager.Load("critters.xml");

            XmlNode objXmlMetatype = objXmlDocument.SelectSingleNode("/chummer/metatypes/metatype[name = \"" + strCritterName + "\"]");

            // If the Critter could not be found, show an error and get out of here.
            if (objXmlMetatype == null)
            {
                MessageBox.Show(string.Format(LanguageManager.GetString("Message_UnknownCritterType", GlobalOptions.Language), strCritterName), LanguageManager.GetString("MessageTitle_SelectCritterType", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // The Critter should use the same settings file as the character.
            Character objCharacter = new Character
            {
                SettingsFile = _objSpirit.CharacterObject.SettingsFile,

                // Override the defaults for the setting.
                IgnoreRules = true,
                IsCritter = true,
                BuildMethod = CharacterBuildMethod.Karma
            };

            if (!string.IsNullOrEmpty(txtCritterName.Text))
                objCharacter.Name = txtCritterName.Text;

            // Ask the user to select a filename for the new character.
            string strForce = LanguageManager.GetString("String_Force", GlobalOptions.Language);
            if (_objSpirit.EntityType == SpiritType.Sprite)
                strForce = LanguageManager.GetString("String_Rating", GlobalOptions.Language);

            string strSpaceCharacter = LanguageManager.GetString("String_Space", GlobalOptions.Language);
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = LanguageManager.GetString("DialogFilter_Chum5", GlobalOptions.Language) + '|' + LanguageManager.GetString("DialogFilter_All", GlobalOptions.Language),
                FileName = strCritterName + strSpaceCharacter + '(' + strForce + strSpaceCharacter + _objSpirit.Force.ToString() + ").chum5"
            };
            if (saveFileDialog.ShowDialog(this) == DialogResult.OK)
            {
                string strFileName = saveFileDialog.FileName;
                objCharacter.FileName = strFileName;
            }
            else
            {
                objCharacter.DeleteCharacter();
                return;
            }

            Cursor = Cursors.WaitCursor;

            // Set Metatype information.
            if (strCritterName == "Ally Spirit")
            {
                objCharacter.BOD.AssignLimits(ExpressionToString(objXmlMetatype["bodmin"]?.InnerText, intForce, 0), ExpressionToString(objXmlMetatype["bodmax"]?.InnerText, intForce, 0), ExpressionToString(objXmlMetatype["bodaug"]?.InnerText, intForce, 0));
                objCharacter.AGI.AssignLimits(ExpressionToString(objXmlMetatype["agimin"]?.InnerText, intForce, 0), ExpressionToString(objXmlMetatype["agimax"]?.InnerText, intForce, 0), ExpressionToString(objXmlMetatype["agiaug"]?.InnerText, intForce, 0));
                objCharacter.REA.AssignLimits(ExpressionToString(objXmlMetatype["reamin"]?.InnerText, intForce, 0), ExpressionToString(objXmlMetatype["reamax"]?.InnerText, intForce, 0), ExpressionToString(objXmlMetatype["reaaug"]?.InnerText, intForce, 0));
                objCharacter.STR.AssignLimits(ExpressionToString(objXmlMetatype["strmin"]?.InnerText, intForce, 0), ExpressionToString(objXmlMetatype["strmax"]?.InnerText, intForce, 0), ExpressionToString(objXmlMetatype["straug"]?.InnerText, intForce, 0));
                objCharacter.CHA.AssignLimits(ExpressionToString(objXmlMetatype["chamin"]?.InnerText, intForce, 0), ExpressionToString(objXmlMetatype["chamax"]?.InnerText, intForce, 0), ExpressionToString(objXmlMetatype["chaaug"]?.InnerText, intForce, 0));
                objCharacter.INT.AssignLimits(ExpressionToString(objXmlMetatype["intmin"]?.InnerText, intForce, 0), ExpressionToString(objXmlMetatype["intmax"]?.InnerText, intForce, 0), ExpressionToString(objXmlMetatype["intaug"]?.InnerText, intForce, 0));
                objCharacter.LOG.AssignLimits(ExpressionToString(objXmlMetatype["logmin"]?.InnerText, intForce, 0), ExpressionToString(objXmlMetatype["logmax"]?.InnerText, intForce, 0), ExpressionToString(objXmlMetatype["logaug"]?.InnerText, intForce, 0));
                objCharacter.WIL.AssignLimits(ExpressionToString(objXmlMetatype["wilmin"]?.InnerText, intForce, 0), ExpressionToString(objXmlMetatype["wilmax"]?.InnerText, intForce, 0), ExpressionToString(objXmlMetatype["wilaug"]?.InnerText, intForce, 0));
                objCharacter.MAG.AssignLimits(ExpressionToString(objXmlMetatype["magmin"]?.InnerText, intForce, 0), ExpressionToString(objXmlMetatype["magmax"]?.InnerText, intForce, 0), ExpressionToString(objXmlMetatype["magaug"]?.InnerText, intForce, 0));
                objCharacter.RES.AssignLimits(ExpressionToString(objXmlMetatype["resmin"]?.InnerText, intForce, 0), ExpressionToString(objXmlMetatype["resmax"]?.InnerText, intForce, 0), ExpressionToString(objXmlMetatype["resaug"]?.InnerText, intForce, 0));
                objCharacter.EDG.AssignLimits(ExpressionToString(objXmlMetatype["edgmin"]?.InnerText, intForce, 0), ExpressionToString(objXmlMetatype["edgmax"]?.InnerText, intForce, 0), ExpressionToString(objXmlMetatype["edgaug"]?.InnerText, intForce, 0));
                objCharacter.ESS.AssignLimits(ExpressionToString(objXmlMetatype["essmin"]?.InnerText, intForce, 0), ExpressionToString(objXmlMetatype["essmax"]?.InnerText, intForce, 0), ExpressionToString(objXmlMetatype["essaug"]?.InnerText, intForce, 0));
            }
            else
            {
                int intMinModifier = -3;
                objCharacter.BOD.AssignLimits(ExpressionToString(objXmlMetatype["bodmin"]?.InnerText, intForce, intMinModifier), ExpressionToString(objXmlMetatype["bodmin"]?.InnerText, intForce, 3), ExpressionToString(objXmlMetatype["bodmin"]?.InnerText, intForce, 3));
                objCharacter.AGI.AssignLimits(ExpressionToString(objXmlMetatype["agimin"]?.InnerText, intForce, intMinModifier), ExpressionToString(objXmlMetatype["agimin"]?.InnerText, intForce, 3), ExpressionToString(objXmlMetatype["agimin"]?.InnerText, intForce, 3));
                objCharacter.REA.AssignLimits(ExpressionToString(objXmlMetatype["reamin"]?.InnerText, intForce, intMinModifier), ExpressionToString(objXmlMetatype["reamin"]?.InnerText, intForce, 3), ExpressionToString(objXmlMetatype["reamin"]?.InnerText, intForce, 3));
                objCharacter.STR.AssignLimits(ExpressionToString(objXmlMetatype["strmin"]?.InnerText, intForce, intMinModifier), ExpressionToString(objXmlMetatype["strmin"]?.InnerText, intForce, 3), ExpressionToString(objXmlMetatype["strmin"]?.InnerText, intForce, 3));
                objCharacter.CHA.AssignLimits(ExpressionToString(objXmlMetatype["chamin"]?.InnerText, intForce, intMinModifier), ExpressionToString(objXmlMetatype["chamin"]?.InnerText, intForce, 3), ExpressionToString(objXmlMetatype["chamin"]?.InnerText, intForce, 3));
                objCharacter.INT.AssignLimits(ExpressionToString(objXmlMetatype["intmin"]?.InnerText, intForce, intMinModifier), ExpressionToString(objXmlMetatype["intmin"]?.InnerText, intForce, 3), ExpressionToString(objXmlMetatype["intmin"]?.InnerText, intForce, 3));
                objCharacter.LOG.AssignLimits(ExpressionToString(objXmlMetatype["logmin"]?.InnerText, intForce, intMinModifier), ExpressionToString(objXmlMetatype["logmin"]?.InnerText, intForce, 3), ExpressionToString(objXmlMetatype["logmin"]?.InnerText, intForce, 3));
                objCharacter.WIL.AssignLimits(ExpressionToString(objXmlMetatype["wilmin"]?.InnerText, intForce, intMinModifier), ExpressionToString(objXmlMetatype["wilmin"]?.InnerText, intForce, 3), ExpressionToString(objXmlMetatype["wilmin"]?.InnerText, intForce, 3));
                objCharacter.MAG.AssignLimits(ExpressionToString(objXmlMetatype["magmin"]?.InnerText, intForce, intMinModifier), ExpressionToString(objXmlMetatype["magmin"]?.InnerText, intForce, 3), ExpressionToString(objXmlMetatype["magmin"]?.InnerText, intForce, 3));
                objCharacter.RES.AssignLimits(ExpressionToString(objXmlMetatype["resmin"]?.InnerText, intForce, intMinModifier), ExpressionToString(objXmlMetatype["resmin"]?.InnerText, intForce, 3), ExpressionToString(objXmlMetatype["resmin"]?.InnerText, intForce, 3));
                objCharacter.EDG.AssignLimits(ExpressionToString(objXmlMetatype["edgmin"]?.InnerText, intForce, intMinModifier), ExpressionToString(objXmlMetatype["edgmin"]?.InnerText, intForce, 3), ExpressionToString(objXmlMetatype["edgmin"]?.InnerText, intForce, 3));
                objCharacter.ESS.AssignLimits(ExpressionToString(objXmlMetatype["essmin"]?.InnerText, intForce, 0), ExpressionToString(objXmlMetatype["essmax"]?.InnerText, intForce, 0), ExpressionToString(objXmlMetatype["essaug"]?.InnerText, intForce, 0));
            }

            // If we're working with a Critter, set the Attributes to their default values.
            objCharacter.BOD.MetatypeMinimum = ExpressionToInt(objXmlMetatype["bodmin"]?.InnerText, intForce, 0);
            objCharacter.AGI.MetatypeMinimum = ExpressionToInt(objXmlMetatype["agimin"]?.InnerText, intForce, 0);
            objCharacter.REA.MetatypeMinimum = ExpressionToInt(objXmlMetatype["reamin"]?.InnerText, intForce, 0);
            objCharacter.STR.MetatypeMinimum = ExpressionToInt(objXmlMetatype["strmin"]?.InnerText, intForce, 0);
            objCharacter.CHA.MetatypeMinimum = ExpressionToInt(objXmlMetatype["chamin"]?.InnerText, intForce, 0);
            objCharacter.INT.MetatypeMinimum = ExpressionToInt(objXmlMetatype["intmin"]?.InnerText, intForce, 0);
            objCharacter.LOG.MetatypeMinimum = ExpressionToInt(objXmlMetatype["logmin"]?.InnerText, intForce, 0);
            objCharacter.WIL.MetatypeMinimum = ExpressionToInt(objXmlMetatype["wilmin"]?.InnerText, intForce, 0);
            objCharacter.MAG.MetatypeMinimum = ExpressionToInt(objXmlMetatype["magmin"]?.InnerText, intForce, 0);
            objCharacter.RES.MetatypeMinimum = ExpressionToInt(objXmlMetatype["resmin"]?.InnerText, intForce, 0);
            objCharacter.EDG.MetatypeMinimum = ExpressionToInt(objXmlMetatype["edgmin"]?.InnerText, intForce, 0);
            objCharacter.ESS.MetatypeMinimum = ExpressionToInt(objXmlMetatype["essmax"]?.InnerText, intForce, 0);

            // Sprites can never have Physical Attributes.
            if (objXmlMetatype["category"].InnerText.EndsWith("Sprite"))
            {
                objCharacter.BOD.AssignLimits("0", "0", "0");
                objCharacter.AGI.AssignLimits("0", "0", "0");
                objCharacter.REA.AssignLimits("0", "0", "0");
                objCharacter.STR.AssignLimits("0", "0", "0");
            }

            objCharacter.Metatype = strCritterName;
            objCharacter.MetatypeCategory = objXmlMetatype["category"].InnerText;
            objCharacter.Metavariant = string.Empty;
            objCharacter.MetatypeBP = 0;

            if (objXmlMetatype["movement"] != null)
                objCharacter.Movement = objXmlMetatype["movement"].InnerText;
            // Load the Qualities file.
            XmlDocument objXmlQualityDocument = XmlManager.Load("qualities.xml");

            // Determine if the Metatype has any bonuses.
            if (objXmlMetatype.InnerXml.Contains("bonus"))
                ImprovementManager.CreateImprovements(objCharacter, Improvement.ImprovementSource.Metatype, strCritterName, objXmlMetatype.SelectSingleNode("bonus"), false, 1, strCritterName);

            // Create the Qualities that come with the Metatype.
            foreach (XmlNode objXmlQualityItem in objXmlMetatype.SelectNodes("qualities/*/quality"))
            {
                XmlNode objXmlQuality = objXmlQualityDocument.SelectSingleNode("/chummer/qualities/quality[name = \"" + objXmlQualityItem.InnerText + "\"]");
                List<Weapon> lstWeapons = new List<Weapon>();
                Quality objQuality = new Quality(objCharacter);
                string strForceValue = objXmlQualityItem.Attributes?["select"]?.InnerText ?? string.Empty;
                QualitySource objSource = objXmlQualityItem.Attributes["removable"]?.InnerText == bool.TrueString ? QualitySource.MetatypeRemovable : QualitySource.Metatype;
                objQuality.Create(objXmlQuality, objSource, lstWeapons, strForceValue);
                objCharacter.Qualities.Add(objQuality);

                // Add any created Weapons to the character.
                foreach (Weapon objWeapon in lstWeapons)
                    objCharacter.Weapons.Add(objWeapon);
            }

            // Add any Critter Powers the Metatype/Critter should have.
            XmlNode objXmlCritter = objXmlDocument.SelectSingleNode("/chummer/metatypes/metatype[name = \"" + objCharacter.Metatype + "\"]");

            objXmlDocument = XmlManager.Load("critterpowers.xml");
            foreach (XmlNode objXmlPower in objXmlCritter.SelectNodes("powers/power"))
            {
                XmlNode objXmlCritterPower = objXmlDocument.SelectSingleNode("/chummer/powers/power[name = \"" + objXmlPower.InnerText + "\"]");
                CritterPower objPower = new CritterPower(objCharacter);
                string strForcedValue = objXmlPower.Attributes?["select"]?.InnerText ?? string.Empty;
                int intRating = Convert.ToInt32(objXmlPower.Attributes?["rating"]?.InnerText);

                objPower.Create(objXmlCritterPower, intRating, strForcedValue);
                objCharacter.CritterPowers.Add(objPower);
            }

            if (objXmlCritter["optionalpowers"] != null)
            {
                //For every 3 full points of Force a spirit has, it may gain one Optional Power.
                for (int i = intForce - 3; i >= 0; i -= 3)
                {
                    XmlDocument objDummyDocument = new XmlDocument();
                    XmlNode bonusNode = objDummyDocument.CreateNode(XmlNodeType.Element, "bonus", null);
                    objDummyDocument.AppendChild(bonusNode);
                    XmlNode powerNode = objDummyDocument.ImportNode(objXmlMetatype["optionalpowers"].CloneNode(true), true);
                    objDummyDocument.ImportNode(powerNode, true);
                    bonusNode.AppendChild(powerNode);
                    ImprovementManager.CreateImprovements(objCharacter, Improvement.ImprovementSource.Metatype, objCharacter.Metatype, bonusNode, false, 1, objCharacter.Metatype);
                }
            }
            // Add any Complex Forms the Critter comes with (typically Sprites)
            XmlDocument objXmlProgramDocument = XmlManager.Load("complexforms.xml");
            foreach (XmlNode objXmlComplexForm in objXmlCritter.SelectNodes("complexforms/complexform"))
            {
                string strForceValue = objXmlComplexForm.Attributes?["select"]?.InnerText ?? string.Empty;
                XmlNode objXmlComplexFormData = objXmlProgramDocument.SelectSingleNode("/chummer/complexforms/complexform[name = \"" + objXmlComplexForm.InnerText + "\"]");
                ComplexForm objComplexForm = new ComplexForm(objCharacter);
                objComplexForm.Create(objXmlComplexFormData, strForceValue);
                objCharacter.ComplexForms.Add(objComplexForm);
            }

            // Add any Gear the Critter comes with (typically Programs for A.I.s)
            XmlDocument objXmlGearDocument = XmlManager.Load("gear.xml");
            foreach (XmlNode objXmlGear in objXmlCritter.SelectNodes("gears/gear"))
            {
                int intRating = 0;
                if (objXmlGear.Attributes["rating"] != null)
                    intRating = ExpressionToInt(objXmlGear.Attributes["rating"].InnerText, decimal.ToInt32(nudForce.Value), 0);
                string strForceValue = objXmlGear.Attributes?["select"]?.InnerText ?? string.Empty;
                XmlNode objXmlGearItem = objXmlGearDocument.SelectSingleNode("/chummer/gears/gear[name = " + objXmlGear.InnerText.CleanXPath() + "]");
                Gear objGear = new Gear(objCharacter);
                List<Weapon> lstWeapons = new List<Weapon>();
                objGear.Create(objXmlGearItem, intRating, lstWeapons, strForceValue);
                objGear.Cost = "0";
                objCharacter.Gear.Add(objGear);
            }

            // Add the Unarmed Attack Weapon to the character.
            objXmlDocument = XmlManager.Load("weapons.xml");
            XmlNode objXmlWeapon = objXmlDocument.SelectSingleNode("/chummer/weapons/weapon[name = \"Unarmed Attack\"]");
            if (objXmlWeapon != null)
            {
                List<Weapon> lstWeapons = new List<Weapon>();
                Weapon objWeapon = new Weapon(objCharacter);
                objWeapon.Create(objXmlWeapon, lstWeapons);
                objWeapon.ParentID = Guid.NewGuid().ToString("D"); // Unarmed Attack can never be removed
                objCharacter.Weapons.Add(objWeapon);
                foreach (Weapon objLoopWeapon in lstWeapons)
                    objCharacter.Weapons.Add(objLoopWeapon);
            }

            objCharacter.Alias = strCritterName;
            objCharacter.Created = true;
            if (!objCharacter.Save())
            {
                Cursor = Cursors.Default;
                objCharacter.DeleteCharacter();
                return;
            }

            string strOpenFile = objCharacter.FileName;
            objCharacter.DeleteCharacter();

            // Link the newly-created Critter to the Spirit.
            _objSpirit.FileName = strOpenFile;
            imgLink.SetToolTip(LanguageManager.GetString(_objSpirit.EntityType == SpiritType.Spirit ? "Tip_Spirit_OpenFile" : "Tip_Sprite_OpenFile", GlobalOptions.Language));
            ContactDetailChanged?.Invoke(this, null);

            Character objOpenCharacter = Program.MainForm.LoadCharacter(strOpenFile);
            Cursor = Cursors.Default;
            Program.MainForm.OpenCharacter(objOpenCharacter);
        }

        /// <summary>
        /// Convert Force, 1D6, or 2D6 into a usable value.
        /// </summary>
        /// <param name="strIn">Expression to convert.</param>
        /// <param name="intForce">Force value to use.</param>
        /// <param name="intOffset">Dice offset.</param>
        /// <returns></returns>
        public static int ExpressionToInt(string strIn, int intForce, int intOffset)
        {
            int intValue = 0;
            string strForce = intForce.ToString();
            // This statement is wrapped in a try/catch since trying 1 div 2 results in an error with XSLT.
            try
            {
                object objProcess = CommonFunctions.EvaluateInvariantXPath(strIn.Replace("/", " div ").Replace("F", strForce).Replace("1D6", strForce).Replace("2D6", strForce), out bool blnIsSuccess);
                if (blnIsSuccess)
                    intValue = Convert.ToInt32(Math.Ceiling((double)objProcess));
            }
            catch (OverflowException) { } // Result is text and not a double
            catch (InvalidCastException) { } // Result is text and not a double
            intValue += intOffset;
            if (intForce > 0)
            {
                if (intValue < 1)
                    return 1;
            }
            else if (intValue < 0)
                return 0;
            return intValue;
        }

        /// <summary>
        /// Convert Force, 1D6, or 2D6 into a usable value.
        /// </summary>
        /// <param name="strIn">Expression to convert.</param>
        /// <param name="intForce">Force value to use.</param>
        /// <param name="intOffset">Dice offset.</param>
        /// <returns></returns>
        public static string ExpressionToString(string strIn, int intForce, int intOffset)
        {
            return ExpressionToInt(strIn, intForce, intOffset).ToString();
        }
        #endregion
    }
}
