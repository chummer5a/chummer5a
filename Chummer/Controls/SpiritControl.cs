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
using System.Xml.XPath;
 using Chummer.Backend.Equipment;
 using Chummer.Skills;

namespace Chummer
{
    public partial class SpiritControl : UserControl
    {
        private Spirit _objSpirit;
        private readonly bool _blnCareer = false;

        // Events.
        public Action<object> ServicesOwedChanged;
        public Action<object> ForceChanged;
        public Action<object> BoundChanged;
        public Action<object> FetteredChanged;
        public Action<object> DeleteSpirit;
        public Action<object> FileNameChanged;

        #region Control Events
        public SpiritControl(bool blnCareer = false)
        {
            InitializeComponent();
            LanguageManager.Instance.Load(GlobalOptions.Instance.Language, this);
            _blnCareer = blnCareer;
            chkBound.Enabled = blnCareer;
        }

        private void nudServices_ValueChanged(object sender, EventArgs e)
        {
            // Raise the ServicesOwedChanged Event when the NumericUpDown's Value changes.
            // The entire SpiritControl is passed as an argument so the handling event can evaluate its contents.
            _objSpirit.ServicesOwed = Convert.ToInt32(nudServices.Value);
            ServicesOwedChanged(this);
        }

        private void cmdDelete_Click(object sender, EventArgs e)
        {
            // Raise the DeleteSpirit Event when the user has confirmed their desire to delete the Spirit.
            // The entire SpiritControl is passed as an argument so the handling event can evaluate its contents.
            DeleteSpirit(this);
        }

        private void nudForce_ValueChanged(object sender, EventArgs e)
        {
            // Raise the ForceChanged Event when the NumericUpDown's Value changes.
            // The entire SpiritControl is passed as an argument so the handling event can evaluate its contents.
            _objSpirit.Force = Convert.ToInt32(nudForce.Value);
            ForceChanged(this);
        }

        private void chkBound_CheckedChanged(object sender, EventArgs e)
        {
            // Raise the BoundChanged Event when the Checkbox's Checked status changes.
            // The entire SpiritControl is passed as an argument so the handling event can evaluate its contents.
            _objSpirit.Bound = chkBound.Checked;
            BoundChanged(this);
        }
        private void chkFettered_CheckedChanged(object sender, EventArgs e)
        {
            if (chkFettered.Checked)
            {
                //Only one Fettered spirit is permitted. 
                if (_objSpirit.CharacterObject.Spirits.Any(objSpirit => objSpirit.Fettered))
                {
                    chkFettered.Checked = false;
                    return;
                }
                _objSpirit.CharacterObject.ObjImprovementManager.CreateImprovement("MAG", Improvement.ImprovementSource.SpiritFettering, "Spirit Fettering", Improvement.ImprovementType.Attribute, string.Empty, 0, 1, 0, 0, -1);
            }
            else
            {
                _objSpirit.CharacterObject.ObjImprovementManager.RemoveImprovements(Improvement.ImprovementSource.SpiritFettering, "Spirit Fettering");
            }
            _objSpirit.Fettered = chkFettered.Checked;

            // Raise the FetteredChanged Event when the Checkbox's Checked status changes.
            // The entire SpiritControl is passed as an argument so the handling event can evaluate its contents.
            FetteredChanged(this);
        }

        private void SpiritControl_Load(object sender, EventArgs e)
        {
            DoubleBuffered = true;
            if (_blnCareer)
                nudForce.Enabled = true;
            Width = cmdDelete.Left + cmdDelete.Width;
        }

        private void cboSpiritName_TextChanged(object sender, EventArgs e)
        {
            _objSpirit.Name = cboSpiritName.Text;
        }

        private void cboSpiritName_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cboSpiritName.SelectedValue != null)
                _objSpirit.Name = cboSpiritName.SelectedValue.ToString();
            ForceChanged(this);
        }

        private void txtCritterName_TextChanged(object sender, EventArgs e)
        {
            _objSpirit.CritterName = txtCritterName.Text;
            ForceChanged(this);
        }

        private void tsContactOpen_Click(object sender, EventArgs e)
        {
            bool blnError = false;
            bool blnUseRelative = false;

            // Make sure the file still exists before attempting to load it.
            if (!File.Exists(_objSpirit.FileName))
            {
                // If the file doesn't exist, use the relative path if one is available.
                if (string.IsNullOrEmpty(_objSpirit.RelativeFileName))
                    blnError = true;
                else
                {
                    MessageBox.Show(Path.GetFullPath(_objSpirit.RelativeFileName));
                    if (!File.Exists(Path.GetFullPath(_objSpirit.RelativeFileName)))
                        blnError = true;
                    else
                        blnUseRelative = true;
                }

                if (blnError)
                {
                    MessageBox.Show(LanguageManager.Instance.GetString("Message_FileNotFound").Replace("{0}", _objSpirit.FileName), LanguageManager.Instance.GetString("MessageTitle_FileNotFound"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
            if (Path.GetExtension(_objSpirit.FileName) == "chum5")
            {
                if (!blnUseRelative)
                    GlobalOptions.Instance.MainForm.LoadCharacter(_objSpirit.FileName, false);
                else
                {
                    string strFile = Path.GetFullPath(_objSpirit.RelativeFileName);
                    GlobalOptions.Instance.MainForm.LoadCharacter(strFile, false);
                }
            }
            else
            {
                if (!blnUseRelative)
                    System.Diagnostics.Process.Start(_objSpirit.FileName);
                else
                {
                    string strFile = Path.GetFullPath(_objSpirit.RelativeFileName);
                    System.Diagnostics.Process.Start(strFile);
                }
            }
        }

        private void tsRemoveCharacter_Click(object sender, EventArgs e)
        {
            // Remove the file association from the Contact.
            if (MessageBox.Show(LanguageManager.Instance.GetString("Message_RemoveCharacterAssociation"), LanguageManager.Instance.GetString("MessageTitle_RemoveCharacterAssociation"), MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                _objSpirit.FileName = string.Empty;
                _objSpirit.RelativeFileName = string.Empty;
                if (_objSpirit.EntityType ==  SpiritType.Spirit)
                    tipTooltip.SetToolTip(imgLink, LanguageManager.Instance.GetString("Tip_Spirit_LinkSpirit"));
                else
                    tipTooltip.SetToolTip(imgLink, LanguageManager.Instance.GetString("Tip_Sprite_LinkSprite"));

                // Set the relative path.
                Uri uriApplication = new Uri(@Application.StartupPath);
                Uri uriFile = new Uri(@_objSpirit.FileName);
                Uri uriRelative = uriApplication.MakeRelativeUri(uriFile);
                _objSpirit.RelativeFileName = "../" + uriRelative.ToString();

                FileNameChanged(this);
            }
        }

        private void tsAttachCharacter_Click(object sender, EventArgs e)
        {
            // Prompt the user to select a save file to associate with this Contact.
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Chummer5 Files (*.chum5)|*.chum5|All Files (*.*)|*.*";

            if (openFileDialog.ShowDialog(this) == DialogResult.OK)
            {
                _objSpirit.FileName = openFileDialog.FileName;
                if (_objSpirit.EntityType == SpiritType.Spirit)
                    tipTooltip.SetToolTip(imgLink, LanguageManager.Instance.GetString("Tip_Spirit_OpenFile"));
                else
                    tipTooltip.SetToolTip(imgLink, LanguageManager.Instance.GetString("Tip_Sprite_OpenFile"));
                FileNameChanged(this);
            }
        }

        private void tsCreateCharacter_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(cboSpiritName.Text))
            {
                MessageBox.Show(LanguageManager.Instance.GetString("Message_SelectCritterType"), LanguageManager.Instance.GetString("MessageTitle_SelectCritterType"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            CreateCritter(cboSpiritName.SelectedValue.ToString(), Convert.ToInt32(nudForce.Value));
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
            frmNotes frmSpritNotes = new frmNotes();
            frmSpritNotes.Notes = _objSpirit.Notes;
            frmSpritNotes.ShowDialog(this);

            if (frmSpritNotes.DialogResult == DialogResult.OK)
                _objSpirit.Notes = frmSpritNotes.Notes;

            string strTooltip = string.Empty;
            if (_objSpirit.EntityType == SpiritType.Spirit)
                strTooltip = LanguageManager.Instance.GetString("Tip_Spirit_EditNotes");
            else
                strTooltip = LanguageManager.Instance.GetString("Tip_Sprite_EditNotes");
            if (!string.IsNullOrEmpty(_objSpirit.Notes))
                strTooltip += "\n\n" + _objSpirit.Notes;
            tipTooltip.SetToolTip(imgNotes, CommonFunctions.WordWrap(strTooltip, 100));
        }

        private void ContextMenu_Opening(object sender, CancelEventArgs e)
        {
            foreach (ToolStripItem objItem in ((ContextMenuStrip)sender).Items)
            {
                if (objItem.Tag != null)
                {
                    objItem.Text = LanguageManager.Instance.GetString(objItem.Tag.ToString());
                }
            }
        }
        #endregion

        #region Properties
        /// <summary>
        /// Spirit object this is linked to.
        /// </summary>
        public Spirit SpiritObject
        {
            get
            {
                return _objSpirit;
            }
            set
            {
                _objSpirit = value;
            }
        }

        /// <summary>
        /// Spirit Metatype name.
        /// </summary>
        public string SpiritName
        {
            get
            {
                return _objSpirit.Name;
            }
            set
            {
                cboSpiritName.Text = value;
                _objSpirit.Name = value;
            }
        }

        /// <summary>
        /// Spirit name.
        /// </summary>
        public string CritterName
        {
            get
            {
                return _objSpirit.CritterName;
            }
            set
            {
                txtCritterName.Text = value;
                _objSpirit.CritterName = value;
            }
        }

        /// <summary>
        /// Indicates if this is a Spirit or Sprite. For labeling purposes only.
        /// </summary>
        public SpiritType EntityType
        {
            get
            {
                return _objSpirit.EntityType;
            }
            set
            {
                _objSpirit.EntityType = value;
                if (value == SpiritType.Spirit)
                {
                    lblForce.Text = LanguageManager.Instance.GetString("Label_Spirit_Force");
                    chkBound.Text = LanguageManager.Instance.GetString("Checkbox_Spirit_Bound");
                    if (!string.IsNullOrEmpty(_objSpirit.FileName))
                        tipTooltip.SetToolTip(imgLink, LanguageManager.Instance.GetString("Tip_Spirit_OpenFile"));
                    else
                        tipTooltip.SetToolTip(imgLink, LanguageManager.Instance.GetString("Tip_Spirit_LinkSpirit"));

                    string strTooltip = LanguageManager.Instance.GetString("Tip_Spirit_EditNotes");
                    if (!string.IsNullOrEmpty(_objSpirit.Notes))
                        strTooltip += "\n\n" + _objSpirit.Notes;
                    tipTooltip.SetToolTip(imgNotes, CommonFunctions.WordWrap(strTooltip, 100));
                }
                else
                {
                    lblForce.Text = LanguageManager.Instance.GetString("Label_Sprite_Rating");
                    chkBound.Text = LanguageManager.Instance.GetString("Label_Sprite_Registered");
                    if (!string.IsNullOrEmpty(_objSpirit.FileName))
                        tipTooltip.SetToolTip(imgLink, "Open the linked Sprite save file.");
                    else
                        tipTooltip.SetToolTip(imgLink, "Link this Sprite to a Chummer save file.");

                    string strTooltip = LanguageManager.Instance.GetString("Tip_Sprite_EditNotes");
                    if (!string.IsNullOrEmpty(_objSpirit.Notes))
                        strTooltip += "\n\n" + _objSpirit.Notes;
                    tipTooltip.SetToolTip(imgNotes, CommonFunctions.WordWrap(strTooltip, 100));
                }
            }
        }

        /// <summary>
        /// Services owed.
        /// </summary>
        public int ServicesOwed
        {
            get
            {
                return _objSpirit.ServicesOwed;
            }
            set
            {
                nudServices.Value = value;
                _objSpirit.ServicesOwed = value;
            }
        }

        /// <summary>
        /// Force of the Spirit.
        /// </summary>
        public int Force
        {
            get
            {
                return _objSpirit.Force;
            }
            set
            {
                nudForce.Value = value;
                _objSpirit.Force = value;
            }
        }

        /// <summary>
        /// Maximum Force of the Spirit.
        /// </summary>
        public int ForceMaximum
        {
            get
            {
                return Convert.ToInt32(nudForce.Maximum);
            }
            set
            {
                nudForce.Maximum = value;
            }
        }

        /// <summary>
        /// Whether or not the Spirit is Bound.
        /// </summary>
        public bool Bound
        {
            get
            {
                return _objSpirit.Bound;
            }
            set
            {
                chkBound.Checked = value;
                _objSpirit.Bound = value;
            }
        }

        /// <summary>
        /// Whether or not the Spirit is Fettered.
        /// </summary>
        public bool Fettered
        {
            get
            {
                return _objSpirit.Fettered;
            }
            set
            {
                chkFettered.Checked = value;
                _objSpirit.Fettered = value;
            }
        }
        #endregion

        #region Methods
        // Rebuild the list of Spirits/Sprites based on the character's selected Tradition/Stream.
        public void RebuildSpiritList(string strTradition)
        {
            string strCurrentValue = string.Empty;
            if (strTradition.Length == 0)
            {
                return;
            }
            if (cboSpiritName.SelectedValue != null)
                strCurrentValue = cboSpiritName.SelectedValue.ToString();
            else
                strCurrentValue = _objSpirit.Name;

            XmlDocument objXmlDocument = new XmlDocument();
            XmlDocument objXmlCritterDocument = new XmlDocument();
            if (_objSpirit.EntityType == SpiritType.Spirit)
                objXmlDocument = XmlManager.Instance.Load("traditions.xml");
            else
                objXmlDocument = XmlManager.Instance.Load("streams.xml");
            objXmlCritterDocument = XmlManager.Instance.Load("critters.xml");

            List<ListItem> lstCritters = new List<ListItem>();
            if (strTradition == "Custom")
            {
                ListItem objCombat = new ListItem();
                objCombat.Value = _objSpirit.CharacterObject.SpiritCombat;
                objCombat.Name = _objSpirit.CharacterObject.SpiritCombat;
                lstCritters.Add(objCombat);

                ListItem objDetection = new ListItem();
                objDetection.Value = _objSpirit.CharacterObject.SpiritDetection;
                objDetection.Name = _objSpirit.CharacterObject.SpiritDetection;
                lstCritters.Add(objDetection);

                ListItem objHealth = new ListItem();
                objHealth.Value = _objSpirit.CharacterObject.SpiritHealth;
                objHealth.Name = _objSpirit.CharacterObject.SpiritHealth;
                lstCritters.Add(objHealth);

                ListItem objIllusion = new ListItem();
                objIllusion.Value = _objSpirit.CharacterObject.SpiritIllusion;
                objIllusion.Name = _objSpirit.CharacterObject.SpiritIllusion;
                lstCritters.Add(objIllusion);

                ListItem objManipulation = new ListItem();
                objManipulation.Value = _objSpirit.CharacterObject.SpiritManipulation;
                objManipulation.Name = _objSpirit.CharacterObject.SpiritManipulation;
                lstCritters.Add(objManipulation);
            }
            else
            {
                foreach (XmlNode objXmlSpirit in objXmlDocument.SelectSingleNode("/chummer/traditions/tradition[name = \"" + strTradition + "\"]/spirits").ChildNodes)
                {
                    ListItem objItem = new ListItem();
                    objItem.Value = objXmlSpirit.InnerText;
                    XmlNode objXmlCritterNode = objXmlCritterDocument.SelectSingleNode("/chummer/metatypes/metatype[name = \"" + objXmlSpirit.InnerText + "\"]");
                    objItem.Name = objXmlCritterNode["translate"]?.InnerText ?? objXmlSpirit.InnerText;

                    lstCritters.Add(objItem);
                }
            }

            if (_objSpirit.CharacterObject.RESEnabled)
            {
                // Add any additional Sprites the character has Access to through Sprite Link.
                foreach (Improvement objImprovement in _objSpirit.CharacterObject.Improvements)
                {
                    if (objImprovement.ImproveType == Improvement.ImprovementType.AddSprite)
                    {
                        ListItem objItem = new ListItem();
                        objItem.Value = objImprovement.ImprovedName;
                        objItem.Name = objImprovement.ImprovedName;
                        lstCritters.Add(objItem);
                    }
                }
            }

            //Add Ally Spirit to MAG-enabled traditions.
            if (_objSpirit.CharacterObject.MAGEnabled)
            {
                ListItem objItem = new ListItem();
                objItem.Value = "Ally Spirit";
                XmlNode objXmlCritterNode = objXmlCritterDocument.SelectSingleNode("/chummer/metatypes/metatype[name = \"" + objItem.Value + "\"]");
                if (objXmlCritterNode["translate"] != null)
                    objItem.Name = objXmlCritterNode["translate"].InnerText;
                else
                    objItem.Name = objItem.Value;
                lstCritters.Add(objItem);
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
            // The Critter should use the same settings file as the character.
            Character objCharacter = new Character();
            objCharacter.SettingsFile = _objSpirit.CharacterObject.SettingsFile;

            // Override the defaults for the setting.
            objCharacter.IgnoreRules = true;
            objCharacter.IsCritter = true;
            objCharacter.BuildMethod = CharacterBuildMethod.Karma;
            objCharacter.BuildPoints = 0;

            if (!string.IsNullOrEmpty(txtCritterName.Text))
                objCharacter.Name = txtCritterName.Text;

            // Ask the user to select a filename for the new character.
            string strForce = LanguageManager.Instance.GetString("String_Force");
            if (_objSpirit.EntityType == SpiritType.Sprite)
                strForce = LanguageManager.Instance.GetString("String_Rating");
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Chummer5 Files (*.chum5)|*.chum5|All Files (*.*)|*.*";
            saveFileDialog.FileName = strCritterName + " (" + strForce + " " + _objSpirit.Force.ToString() + ").chum5";
            if (saveFileDialog.ShowDialog(this) == DialogResult.OK)
            {
                string strFileName = saveFileDialog.FileName;
                objCharacter.FileName = strFileName;
            }
            else
                return;

            // Code from frmMetatype.
            ImprovementManager objImprovementManager = new ImprovementManager(objCharacter);
            XmlDocument objXmlDocument = XmlManager.Instance.Load("critters.xml");

            XmlNode objXmlMetatype = objXmlDocument.SelectSingleNode("/chummer/metatypes/metatype[name = \"" + strCritterName + "\"]");

            // If the Critter could not be found, show an error and get out of here.
            if (objXmlMetatype == null)
            {
                MessageBox.Show(LanguageManager.Instance.GetString("Message_UnknownCritterType").Replace("{0}", strCritterName), LanguageManager.Instance.GetString("MessageTitle_SelectCritterType"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Set Metatype information.
            if (strCritterName == "Ally Spirit")
            {
                objCharacter.BOD.AssignLimits(ExpressionToString(objXmlMetatype["bodmin"].InnerText, intForce, 0), ExpressionToString(objXmlMetatype["bodmax"].InnerText, intForce, 0), ExpressionToString(objXmlMetatype["bodaug"].InnerText, intForce, 0));
                objCharacter.AGI.AssignLimits(ExpressionToString(objXmlMetatype["agimin"].InnerText, intForce, 0), ExpressionToString(objXmlMetatype["agimax"].InnerText, intForce, 0), ExpressionToString(objXmlMetatype["agiaug"].InnerText, intForce, 0));
                objCharacter.REA.AssignLimits(ExpressionToString(objXmlMetatype["reamin"].InnerText, intForce, 0), ExpressionToString(objXmlMetatype["reamax"].InnerText, intForce, 0), ExpressionToString(objXmlMetatype["reaaug"].InnerText, intForce, 0));
                objCharacter.STR.AssignLimits(ExpressionToString(objXmlMetatype["strmin"].InnerText, intForce, 0), ExpressionToString(objXmlMetatype["strmax"].InnerText, intForce, 0), ExpressionToString(objXmlMetatype["straug"].InnerText, intForce, 0));
                objCharacter.CHA.AssignLimits(ExpressionToString(objXmlMetatype["chamin"].InnerText, intForce, 0), ExpressionToString(objXmlMetatype["chamax"].InnerText, intForce, 0), ExpressionToString(objXmlMetatype["chaaug"].InnerText, intForce, 0));
                objCharacter.INT.AssignLimits(ExpressionToString(objXmlMetatype["intmin"].InnerText, intForce, 0), ExpressionToString(objXmlMetatype["intmax"].InnerText, intForce, 0), ExpressionToString(objXmlMetatype["intaug"].InnerText, intForce, 0));
                objCharacter.LOG.AssignLimits(ExpressionToString(objXmlMetatype["logmin"].InnerText, intForce, 0), ExpressionToString(objXmlMetatype["logmax"].InnerText, intForce, 0), ExpressionToString(objXmlMetatype["logaug"].InnerText, intForce, 0));
                objCharacter.WIL.AssignLimits(ExpressionToString(objXmlMetatype["wilmin"].InnerText, intForce, 0), ExpressionToString(objXmlMetatype["wilmax"].InnerText, intForce, 0), ExpressionToString(objXmlMetatype["wilaug"].InnerText, intForce, 0));
                objCharacter.MAG.AssignLimits(ExpressionToString(objXmlMetatype["magmin"].InnerText, intForce, 0), ExpressionToString(objXmlMetatype["magmax"].InnerText, intForce, 0), ExpressionToString(objXmlMetatype["magaug"].InnerText, intForce, 0));
                objCharacter.RES.AssignLimits(ExpressionToString(objXmlMetatype["resmin"].InnerText, intForce, 0), ExpressionToString(objXmlMetatype["resmax"].InnerText, intForce, 0), ExpressionToString(objXmlMetatype["resaug"].InnerText, intForce, 0));
                objCharacter.EDG.AssignLimits(ExpressionToString(objXmlMetatype["edgmin"].InnerText, intForce, 0), ExpressionToString(objXmlMetatype["edgmax"].InnerText, intForce, 0), ExpressionToString(objXmlMetatype["edgaug"].InnerText, intForce, 0));
                objCharacter.ESS.AssignLimits(ExpressionToString(objXmlMetatype["essmin"].InnerText, intForce, 0), ExpressionToString(objXmlMetatype["essmax"].InnerText, intForce, 0), ExpressionToString(objXmlMetatype["essaug"].InnerText, intForce, 0));
            }
            else
            {
                int intMinModifier = -3;
                objCharacter.BOD.AssignLimits(ExpressionToString(objXmlMetatype["bodmin"].InnerText, intForce, intMinModifier), ExpressionToString(objXmlMetatype["bodmin"].InnerText, intForce, 3), ExpressionToString(objXmlMetatype["bodmin"].InnerText, intForce, 3));
                objCharacter.AGI.AssignLimits(ExpressionToString(objXmlMetatype["agimin"].InnerText, intForce, intMinModifier), ExpressionToString(objXmlMetatype["agimin"].InnerText, intForce, 3), ExpressionToString(objXmlMetatype["agimin"].InnerText, intForce, 3));
                objCharacter.REA.AssignLimits(ExpressionToString(objXmlMetatype["reamin"].InnerText, intForce, intMinModifier), ExpressionToString(objXmlMetatype["reamin"].InnerText, intForce, 3), ExpressionToString(objXmlMetatype["reamin"].InnerText, intForce, 3));
                objCharacter.STR.AssignLimits(ExpressionToString(objXmlMetatype["strmin"].InnerText, intForce, intMinModifier), ExpressionToString(objXmlMetatype["strmin"].InnerText, intForce, 3), ExpressionToString(objXmlMetatype["strmin"].InnerText, intForce, 3));
                objCharacter.CHA.AssignLimits(ExpressionToString(objXmlMetatype["chamin"].InnerText, intForce, intMinModifier), ExpressionToString(objXmlMetatype["chamin"].InnerText, intForce, 3), ExpressionToString(objXmlMetatype["chamin"].InnerText, intForce, 3));
                objCharacter.INT.AssignLimits(ExpressionToString(objXmlMetatype["intmin"].InnerText, intForce, intMinModifier), ExpressionToString(objXmlMetatype["intmin"].InnerText, intForce, 3), ExpressionToString(objXmlMetatype["intmin"].InnerText, intForce, 3));
                objCharacter.LOG.AssignLimits(ExpressionToString(objXmlMetatype["logmin"].InnerText, intForce, intMinModifier), ExpressionToString(objXmlMetatype["logmin"].InnerText, intForce, 3), ExpressionToString(objXmlMetatype["logmin"].InnerText, intForce, 3));
                objCharacter.WIL.AssignLimits(ExpressionToString(objXmlMetatype["wilmin"].InnerText, intForce, intMinModifier), ExpressionToString(objXmlMetatype["wilmin"].InnerText, intForce, 3), ExpressionToString(objXmlMetatype["wilmin"].InnerText, intForce, 3));
                objCharacter.MAG.AssignLimits(ExpressionToString(objXmlMetatype["magmin"].InnerText, intForce, intMinModifier), ExpressionToString(objXmlMetatype["magmin"].InnerText, intForce, 3), ExpressionToString(objXmlMetatype["magmin"].InnerText, intForce, 3));
                objCharacter.RES.AssignLimits(ExpressionToString(objXmlMetatype["resmin"].InnerText, intForce, intMinModifier), ExpressionToString(objXmlMetatype["resmin"].InnerText, intForce, 3), ExpressionToString(objXmlMetatype["resmin"].InnerText, intForce, 3));
                objCharacter.EDG.AssignLimits(ExpressionToString(objXmlMetatype["edgmin"].InnerText, intForce, intMinModifier), ExpressionToString(objXmlMetatype["edgmin"].InnerText, intForce, 3), ExpressionToString(objXmlMetatype["edgmin"].InnerText, intForce, 3));
                objCharacter.ESS.AssignLimits(ExpressionToString(objXmlMetatype["essmin"].InnerText, intForce, 0), ExpressionToString(objXmlMetatype["essmax"].InnerText, intForce, 0), ExpressionToString(objXmlMetatype["essaug"].InnerText, intForce, 0));
            }

            // If we're working with a Critter, set the Attributes to their default values.
            objCharacter.BOD.MetatypeMinimum = Convert.ToInt32(ExpressionToString(objXmlMetatype["bodmin"].InnerText, intForce, 0));
            objCharacter.AGI.MetatypeMinimum = Convert.ToInt32(ExpressionToString(objXmlMetatype["agimin"].InnerText, intForce, 0));
            objCharacter.REA.MetatypeMinimum = Convert.ToInt32(ExpressionToString(objXmlMetatype["reamin"].InnerText, intForce, 0));
            objCharacter.STR.MetatypeMinimum = Convert.ToInt32(ExpressionToString(objXmlMetatype["strmin"].InnerText, intForce, 0));
            objCharacter.CHA.MetatypeMinimum = Convert.ToInt32(ExpressionToString(objXmlMetatype["chamin"].InnerText, intForce, 0));
            objCharacter.INT.MetatypeMinimum = Convert.ToInt32(ExpressionToString(objXmlMetatype["intmin"].InnerText, intForce, 0));
            objCharacter.LOG.MetatypeMinimum = Convert.ToInt32(ExpressionToString(objXmlMetatype["logmin"].InnerText, intForce, 0));
            objCharacter.WIL.MetatypeMinimum = Convert.ToInt32(ExpressionToString(objXmlMetatype["wilmin"].InnerText, intForce, 0));
            objCharacter.MAG.MetatypeMinimum = Convert.ToInt32(ExpressionToString(objXmlMetatype["magmin"].InnerText, intForce, 0));
            objCharacter.RES.MetatypeMinimum = Convert.ToInt32(ExpressionToString(objXmlMetatype["resmin"].InnerText, intForce, 0));
            objCharacter.EDG.MetatypeMinimum = Convert.ToInt32(ExpressionToString(objXmlMetatype["edgmin"].InnerText, intForce, 0));
            objCharacter.ESS.MetatypeMinimum = Convert.ToInt32(ExpressionToString(objXmlMetatype["essmax"].InnerText, intForce, 0));

            // Sprites can never have Physical Attributes or WIL.
            if (objXmlMetatype["category"].InnerText.EndsWith("Sprite"))
            {
                objCharacter.BOD.AssignLimits("0", "0", "0");
                objCharacter.AGI.AssignLimits("0", "0", "0");
                objCharacter.REA.AssignLimits("0", "0", "0");
                objCharacter.STR.AssignLimits("0", "0", "0");
                objCharacter.WIL.AssignLimits("0", "0", "0");
            }

            objCharacter.Metatype = strCritterName;
            objCharacter.MetatypeCategory = objXmlMetatype["category"].InnerText;
            objCharacter.Metavariant = string.Empty;
            objCharacter.MetatypeBP = 0;

            if (objXmlMetatype["movement"] != null)
                objCharacter.Movement = objXmlMetatype["movement"].InnerText;
            // Load the Qualities file.
            XmlDocument objXmlQualityDocument = XmlManager.Instance.Load("qualities.xml");

            // Determine if the Metatype has any bonuses.
            if (objXmlMetatype.InnerXml.Contains("bonus"))
                objImprovementManager.CreateImprovements(Improvement.ImprovementSource.Metatype, strCritterName, objXmlMetatype.SelectSingleNode("bonus"), false, 1, strCritterName);

            // Create the Qualities that come with the Metatype.
            foreach (XmlNode objXmlQualityItem in objXmlMetatype.SelectNodes("qualities/positive/quality"))
            {
                XmlNode objXmlQuality = objXmlQualityDocument.SelectSingleNode("/chummer/qualities/quality[name = \"" + objXmlQualityItem.InnerText + "\"]");
                TreeNode objNode = new TreeNode();
                List<Weapon> objWeapons = new List<Weapon>();
                List<TreeNode> objWeaponNodes = new List<TreeNode>();
                Quality objQuality = new Quality(objCharacter);
                string strForceValue = string.Empty;
                if (objXmlQualityItem.Attributes["select"] != null)
                    strForceValue = objXmlQualityItem.Attributes["select"].InnerText;
                QualitySource objSource = new QualitySource();
                objSource = QualitySource.Metatype;
                if (objXmlQualityItem.Attributes["removable"] != null)
                    objSource = QualitySource.MetatypeRemovable;
                objQuality.Create(objXmlQuality, objCharacter, objSource, objNode, objWeapons, objWeaponNodes, strForceValue);
                objCharacter.Qualities.Add(objQuality);

                // Add any created Weapons to the character.
                foreach (Weapon objWeapon in objWeapons)
                    objCharacter.Weapons.Add(objWeapon);
            }
            foreach (XmlNode objXmlQualityItem in objXmlMetatype.SelectNodes("qualities/negative/quality"))
            {
                XmlNode objXmlQuality = objXmlQualityDocument.SelectSingleNode("/chummer/qualities/quality[name = \"" + objXmlQualityItem.InnerText + "\"]");
                TreeNode objNode = new TreeNode();
                List<Weapon> objWeapons = new List<Weapon>();
                List<TreeNode> objWeaponNodes = new List<TreeNode>();
                Quality objQuality = new Quality(objCharacter);
                string strForceValue = string.Empty;
                if (objXmlQualityItem.Attributes["select"] != null)
                    strForceValue = objXmlQualityItem.Attributes["select"].InnerText;
                QualitySource objSource = new QualitySource();
                objSource = QualitySource.Metatype;
                if (objXmlQualityItem.Attributes["removable"] != null)
                    objSource = QualitySource.MetatypeRemovable;
                objQuality.Create(objXmlQuality, objCharacter, objSource, objNode, objWeapons, objWeaponNodes, strForceValue);
                objCharacter.Qualities.Add(objQuality);

                // Add any created Weapons to the character.
                foreach (Weapon objWeapon in objWeapons)
                    objCharacter.Weapons.Add(objWeapon);
            }

            // Add any Critter Powers the Metatype/Critter should have.
            XmlNode objXmlCritter = objXmlDocument.SelectSingleNode("/chummer/metatypes/metatype[name = \"" + objCharacter.Metatype + "\"]");

            objXmlDocument = XmlManager.Instance.Load("critterpowers.xml");
            foreach (XmlNode objXmlPower in objXmlCritter.SelectNodes("powers/power"))
            {
                XmlNode objXmlCritterPower = objXmlDocument.SelectSingleNode("/chummer/powers/power[name = \"" + objXmlPower.InnerText + "\"]");
                TreeNode objNode = new TreeNode();
                CritterPower objPower = new CritterPower(objCharacter);
                string strForcedValue = string.Empty;
                int intRating = 0;

                if (objXmlPower.Attributes["rating"] != null)
                    intRating = Convert.ToInt32(objXmlPower.Attributes["rating"].InnerText);
                if (objXmlPower.Attributes["select"] != null)
                    strForcedValue = objXmlPower.Attributes["select"].InnerText;

                objPower.Create(objXmlCritterPower, objCharacter, objNode, intRating, strForcedValue);
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
                    objImprovementManager.CreateImprovements(Improvement.ImprovementSource.Metatype, objCharacter.Metatype, bonusNode, false, 1, objCharacter.Metatype);
                }
            }
            // Add any Complex Forms the Critter comes with (typically Sprites)
            XmlDocument objXmlProgramDocument = XmlManager.Instance.Load("complexforms.xml");
            foreach (XmlNode objXmlComplexForm in objXmlCritter.SelectNodes("complexforms/complexform"))
            {
                string strForceValue = string.Empty;
                if (objXmlComplexForm.Attributes["select"] != null)
                    strForceValue = objXmlComplexForm.Attributes["select"].InnerText;
                XmlNode objXmlProgram = objXmlProgramDocument.SelectSingleNode("/chummer/complexforms/complexform[name = \"" + objXmlComplexForm.InnerText + "\"]");
                TreeNode objNode = new TreeNode();
                ComplexForm objProgram = new ComplexForm(objCharacter);
                objProgram.Create(objXmlProgram, objCharacter, objNode, strForceValue);
                objCharacter.ComplexForms.Add(objProgram);
            }

            // Add any Gear the Critter comes with (typically Programs for A.I.s)
            XmlDocument objXmlGearDocument = XmlManager.Instance.Load("gear.xml");
            foreach (XmlNode objXmlGear in objXmlCritter.SelectNodes("gears/gear"))
            {
                int intRating = 0;
                if (objXmlGear.Attributes["rating"] != null)
                    intRating = Convert.ToInt32(ExpressionToString(objXmlGear.Attributes["rating"].InnerText, Convert.ToInt32(nudForce.Value), 0));
                string strForceValue = string.Empty;
                if (objXmlGear.Attributes["select"] != null)
                    strForceValue = objXmlGear.Attributes["select"].InnerText;
                XmlNode objXmlGearItem = objXmlGearDocument.SelectSingleNode("/chummer/gears/gear[name = \"" + objXmlGear.InnerText + "\"]");
                TreeNode objNode = new TreeNode();
                Gear objGear = new Gear(objCharacter);
                List<Weapon> lstWeapons = new List<Weapon>();
                List<TreeNode> lstWeaponNodes = new List<TreeNode>();
                objGear.Create(objXmlGearItem, objCharacter, objNode, intRating, lstWeapons, lstWeaponNodes, strForceValue);
                objGear.Cost = "0";
                objGear.Cost3 = "0";
                objGear.Cost6 = "0";
                objGear.Cost10 = "0";
                objCharacter.Gear.Add(objGear);
            }

            // Add the Unarmed Attack Weapon to the character.
            objXmlDocument = XmlManager.Instance.Load("weapons.xml");
            XmlNode objXmlWeapon = objXmlDocument.SelectSingleNode("/chummer/weapons/weapon[name = \"Unarmed Attack\"]");
            if (objXmlWeapon != null)
            {
                TreeNode objDummy = new TreeNode();
                Weapon objWeapon = new Weapon(objCharacter);
                objWeapon.Create(objXmlWeapon, objCharacter, objDummy, null, null);
                objCharacter.Weapons.Add(objWeapon);
            }

            objCharacter.Alias = strCritterName;
            objCharacter.Created = true;
            objCharacter.Save();

            string strOpenFile = objCharacter.FileName;
            objCharacter = null;

            // Link the newly-created Critter to the Spirit.
            _objSpirit.FileName = strOpenFile;
            if (_objSpirit.EntityType == SpiritType.Spirit)
                tipTooltip.SetToolTip(imgLink, LanguageManager.Instance.GetString("Tip_Spirit_OpenFile"));
            else
                tipTooltip.SetToolTip(imgLink, LanguageManager.Instance.GetString("Tip_Sprite_OpenFile"));
            FileNameChanged(this);

            GlobalOptions.Instance.MainForm.LoadCharacter(strOpenFile, true);
        }

        /// <summary>
        /// Convert Force, 1D6, or 2D6 into a usable value.
        /// </summary>
        /// <param name="strIn">Expression to convert.</param>
        /// <param name="intForce">Force value to use.</param>
        /// <param name="intOffset">Dice offset.</param>
        /// <returns></returns>
        public string ExpressionToString(string strIn, int intForce, int intOffset)
        {
            int intValue = 0;
            XmlDocument objXmlDocument = new XmlDocument();
            XPathNavigator nav = objXmlDocument.CreateNavigator();
            XPathExpression xprAttribute = nav.Compile(strIn.Replace("/", " div ").Replace("F", intForce.ToString()).Replace("1D6", intForce.ToString()).Replace("2D6", intForce.ToString()));
            object xprEvaluateResult = null;
            // This statement is wrapped in a try/catch since trying 1 div 2 results in an error with XSLT.
            try
            {
                xprEvaluateResult = nav.Evaluate(xprAttribute);
            }
            catch (XPathException) { }
            if (xprEvaluateResult != null && xprEvaluateResult.GetType() == typeof(Double))
                intValue = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(xprEvaluateResult.ToString(),GlobalOptions.InvariantCultureInfo)));
            intValue += intOffset;
            if (intForce > 0)
            {
                if (intValue < 1)
                    intValue = 1;
            }
            else
            {
                if (intValue < 0)
                    intValue = 0;
            }
            return intValue.ToString();
        }
        #endregion

    }
}