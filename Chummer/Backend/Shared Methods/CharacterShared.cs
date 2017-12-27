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
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Chummer.Backend.Equipment;
using Chummer.Backend.Skills;
using System.Xml;
using System.Xml.XPath;
using Chummer.Backend.Attributes;
using TheArtOfDev.HtmlRenderer.WinForms;
using System.Text;

namespace Chummer
{
    /// <summary>
    /// Contains functionality shared between frmCreate and frmCareer
    /// </summary>
    [System.ComponentModel.DesignerCategory("")]
    public class CharacterShared : Form
    {
        private readonly Character _objCharacter;
        private readonly CharacterOptions _objOptions;
        private bool _blnIsDirty = false;
        private bool _blnRequestCharacterUpdate = false;

        public CharacterShared(Character objCharacter)
        {
            _objCharacter = objCharacter;
            _objOptions = _objCharacter.Options;
            _objCharacter.CharacterNameChanged += objCharacter_CharacterNameChanged;
            _gunneryCached = new Lazy<Skill>(() => _objCharacter.SkillsSection.GetActiveSkill("Gunnery"));
        }

        [Obsolete("This constructor is for use by form designers only.", true)]
        public CharacterShared()
        {
            _gunneryCached = new Lazy<Skill>(() => _objCharacter.SkillsSection.GetActiveSkill("Gunnery"));
        }

        /// <summary>
        /// Wrapper for relocating contact forms. 
        /// </summary>
        protected struct TransportWrapper
        {
            public Control Control { get; }

            public TransportWrapper(Control objControl)
            {
                Control = objControl;
            }

            public override bool Equals(object obj)
            {
                return Control.Equals(obj);
            }

            public override int GetHashCode()
            {
                return Control.GetHashCode();
            }

            public override string ToString()
            {
                return Control.ToString();
            }
        }

        public Stopwatch Autosave_StopWatch { get; } = Stopwatch.StartNew();
        /// <summary>
        /// Automatically Save the character to a backup folder.
        /// </summary>
        public void AutoSaveCharacter()
        {
            Cursor = Cursors.WaitCursor;
            string strAutosavePath = Path.Combine(Application.StartupPath, "saves", "autosave");
            if (!Directory.Exists(strAutosavePath))
            {
                try
                {
                    Directory.CreateDirectory(strAutosavePath);
                }
                catch (UnauthorizedAccessException)
                {
                    Cursor = Cursors.Default;
                    MessageBox.Show(LanguageManager.GetString("Message_Insufficient_Permissions_Warning", GlobalOptions.Language));
                    Autosave_StopWatch.Restart();
                    return;
                }
            }
            
            string[] strFile = _objCharacter.FileName.Split(Path.DirectorySeparatorChar);
            string strShowFileName = strFile[strFile.Length - 1];

            if (string.IsNullOrEmpty(strShowFileName))
                strShowFileName = _objCharacter.CharacterName;
            string strFilePath = Path.Combine(strAutosavePath, strShowFileName);
            _objCharacter.Save(strFilePath);
            Cursor = Cursors.Default;
            Autosave_StopWatch.Restart();
        }

        /// <summary>
        /// Update the label and tooltip for the character's Condition Monitors.
        /// </summary>
        /// <param name="lblPhysical"></param>
        /// <param name="lblStun"></param>
        /// <param name="tipTooltip"></param>
        /// <param name="_objImprovementManager"></param>
        protected void UpdateConditionMonitor(Label lblPhysical, Label lblStun, ToolTip tipTooltip)
        {
            // Condition Monitor.
            int intCMPhysical = _objCharacter.PhysicalCM;
            int intCMStun = _objCharacter.StunCM;

            // Update the Condition Monitor labels.
            lblPhysical.Text = intCMPhysical.ToString();
            lblStun.Text = intCMStun.ToString();
            if (tipTooltip != null)
            {
                int intBOD = _objCharacter.BOD.TotalValue;
                int intWIL = _objCharacter.WIL.TotalValue;
                string strCM = $"8 + ({_objCharacter.BOD.DisplayAbbrev}/2)({(intBOD + 1) / 2})";
                if (ImprovementManager.ValueOf(_objCharacter, Improvement.ImprovementType.PhysicalCM) != 0)
                    strCM += " + " + LanguageManager.GetString("Tip_Modifiers", GlobalOptions.Language) + " (" +
                             ImprovementManager.ValueOf(_objCharacter, Improvement.ImprovementType.PhysicalCM).ToString() + ")";
                tipTooltip.SetToolTip(lblPhysical, strCM);
                strCM = $"8 + ({_objCharacter.WIL.DisplayAbbrev}/2)({(intWIL + 1) / 2})";
                if (ImprovementManager.ValueOf(_objCharacter, Improvement.ImprovementType.StunCM) != 0)
                    strCM += " + " + LanguageManager.GetString("Tip_Modifiers", GlobalOptions.Language) + " (" +
                             ImprovementManager.ValueOf(_objCharacter, Improvement.ImprovementType.StunCM).ToString() + ")";
                tipTooltip.SetToolTip(lblStun, strCM);
            }
        }

        /// <summary>
        /// Update the label and tooltip for the character's Armor Rating.
        /// </summary>
        /// <param name="lblArmor"></param>
        /// <param name="tipTooltip"></param>
        /// <param name="objImprovementManager"></param>
        /// <param name="lblCMArmor"></param>
        protected void UpdateArmorRating(Label lblArmor, ToolTip tipTooltip, Label lblCMArmor = null)
        {
            // Armor Ratings.
            int intTotalArmorRating = _objCharacter.TotalArmorRating;
            int intArmorRating = _objCharacter.ArmorRating;
            lblArmor.Text = intTotalArmorRating.ToString();
            if (tipTooltip != null)
            {
                string strArmorToolTip = LanguageManager.GetString("Tip_Armor", GlobalOptions.Language) + " (" + intArmorRating.ToString() + ")";
                if (intArmorRating != intTotalArmorRating)
                    strArmorToolTip += " + " + LanguageManager.GetString("Tip_Modifiers", GlobalOptions.Language) + " (" +
                                       (intTotalArmorRating - intArmorRating).ToString() + ")";
                tipTooltip.SetToolTip(lblArmor, strArmorToolTip);
                if (lblCMArmor != null)
                {
                    lblCMArmor.Text = intTotalArmorRating.ToString();
                    tipTooltip.SetToolTip(lblCMArmor, strArmorToolTip);
                }
            }

            // Remove any Improvements from Armor Encumbrance.
            ImprovementManager.RemoveImprovements(_objCharacter, Improvement.ImprovementSource.ArmorEncumbrance, "Armor Encumbrance");
            // Create the Armor Encumbrance Improvements.
            int intEncumbrance = _objCharacter.ArmorEncumbrance;
            if (intEncumbrance < 0)
            {
                ImprovementManager.CreateImprovement(_objCharacter, "AGI", Improvement.ImprovementSource.ArmorEncumbrance, "Armor Encumbrance",
                    Improvement.ImprovementType.Attribute, "precedence-1", 0, 1, 0, 0, intEncumbrance);
                ImprovementManager.CreateImprovement(_objCharacter, "REA", Improvement.ImprovementSource.ArmorEncumbrance, "Armor Encumbrance",
                    Improvement.ImprovementType.Attribute, "precedence-1", 0, 1, 0, 0, intEncumbrance);
            }
        }

        /// <summary>
        /// Update the labels and tooltips for the character's Limits.
        /// </summary>
        /// <param name="lblPhysical"></param>
        /// <param name="lblMental"></param>
        /// <param name="lblSocial"></param>
        /// <param name="lblAstral"></param>
        /// <param name="tipTooltip"></param>
        protected void RefreshLimits(Label lblPhysical, Label lblMental, Label lblSocial, Label lblAstral, ToolTip tipTooltip)
        {
            lblPhysical.Text = _objCharacter.LimitPhysical.ToString();
            lblMental.Text = _objCharacter.LimitMental.ToString();
            lblSocial.Text = _objCharacter.LimitSocial.ToString();

            if (tipTooltip != null)
            {
                StringBuilder objPhysical = new StringBuilder(
                    $"({_objCharacter.STR.DisplayAbbrev} [{_objCharacter.STR.TotalValue}] * 2) + {_objCharacter.BOD.DisplayAbbrev} [{_objCharacter.BOD.TotalValue}] + {_objCharacter.REA.DisplayAbbrev} [{_objCharacter.REA.TotalValue}] / 3");
                StringBuilder objMental = new StringBuilder(
                    $"({_objCharacter.LOG.DisplayAbbrev} [{_objCharacter.LOG.TotalValue}] * 2) + {_objCharacter.INT.DisplayAbbrev} [{_objCharacter.INT.TotalValue}] + {_objCharacter.WIL.DisplayAbbrev} [{_objCharacter.WIL.TotalValue}] / 3");
                StringBuilder objSocial = new StringBuilder(
                    $"({_objCharacter.CHA.DisplayAbbrev} [{_objCharacter.CHA.TotalValue}] * 2) + {_objCharacter.WIL.DisplayAbbrev} [{_objCharacter.WIL.TotalValue}] + {_objCharacter.ESS.DisplayAbbrev} [{_objCharacter.Essence.ToString(GlobalOptions.CultureInfo)}] / 3");

                foreach (Improvement objLoopImprovement in _objCharacter.Improvements.Where(
                    objLoopImprovment => (objLoopImprovment.ImproveType == Improvement.ImprovementType.PhysicalLimit
                    || objLoopImprovment.ImproveType == Improvement.ImprovementType.SocialLimit
                    || objLoopImprovment.ImproveType == Improvement.ImprovementType.MentalLimit) && objLoopImprovment.Enabled))
                {
                    switch (objLoopImprovement.ImproveType)
                    {
                        case Improvement.ImprovementType.PhysicalLimit:
                            objPhysical.Append($" + {_objCharacter.GetObjectName(objLoopImprovement, GlobalOptions.Language)} ({objLoopImprovement.Value})");
                            break;
                        case Improvement.ImprovementType.MentalLimit:
                            objMental.Append($" + {_objCharacter.GetObjectName(objLoopImprovement, GlobalOptions.Language)} ({objLoopImprovement.Value})");
                            break;
                        case Improvement.ImprovementType.SocialLimit:
                            objSocial.Append($" + {_objCharacter.GetObjectName(objLoopImprovement, GlobalOptions.Language)} ({objLoopImprovement.Value})");
                            break;
                    }
                }

                tipTooltip.SetToolTip(lblPhysical, objPhysical.ToString());
                tipTooltip.SetToolTip(lblMental, objMental.ToString());
                tipTooltip.SetToolTip(lblSocial, objSocial.ToString());
            }

            lblAstral.Text = _objCharacter.LimitAstral.ToString();
        }

        private static Lazy<Skill> _gunneryCached;

        public static int MountedGunManualOperationDicePool(Weapon weapon)
        {
            return _gunneryCached.Value.Pool;
        }

        public static int MountedGunCommandDeviceDicePool(Character objCharacter)
        {
            return _gunneryCached.Value.PoolOtherAttribute(objCharacter.LOG.TotalValue);
        }

        public static int MountedGunDogBrainDicePool(Weapon weapon, Vehicle vehicle)
        {
            int pilotRating = vehicle.Pilot;

            Gear maybeAutoSoft = vehicle.Gear.DeepFirstOrDefault(x => x.Children, x => x.Name == "[Weapon] Targeting Autosoft" && (x.Extra == weapon.Name || x.Extra == weapon.DisplayName(GlobalOptions.Language)));

            if (maybeAutoSoft != null)
            {
                return maybeAutoSoft.Rating + pilotRating;
            }

            return 0;
        }

        /// <summary>
        /// Edit and update a Limit Modifier.
        /// </summary>
        /// <param name="treLimit"></param>
        /// <param name="cmsLimitModifier"></param>
        protected void UpdateLimitModifier(TreeView treLimit, ContextMenuStrip cmsLimitModifier)
        {
            TreeNode objSelectedNode = treLimit.SelectedNode;
            LimitModifier objLimitModifier = _objCharacter.LimitModifiers.FindById(treLimit.SelectedNode.Tag.ToString());
            //If the LimitModifier couldn't be found (Ie it comes from an Improvement or the user hasn't properly selected a treenode, fail out early.
            if (objLimitModifier == null)
            {
                MessageBox.Show(LanguageManager.GetString("Warning_NoLimitFound", GlobalOptions.Language));
                return;
            }
            frmSelectLimitModifier frmPickLimitModifier = new frmSelectLimitModifier(objLimitModifier);
            frmPickLimitModifier.ShowDialog(this);

            if (frmPickLimitModifier.DialogResult == DialogResult.Cancel)
                return;

            //Remove the old LimitModifier to ensure we don't double up.
            _objCharacter.LimitModifiers.Remove(objLimitModifier);
            // Create the new limit modifier.
            TreeNode objNode = new TreeNode();
            objLimitModifier = new LimitModifier(_objCharacter);
            string strLimit = treLimit.SelectedNode.Parent.Text;
            string strCondition = frmPickLimitModifier.SelectedCondition;
            objLimitModifier.Create(frmPickLimitModifier.SelectedName, frmPickLimitModifier.SelectedBonus, strLimit,
                strCondition, objNode);
            objLimitModifier.Guid = new Guid(objSelectedNode.Tag.ToString());
            if (objLimitModifier.InternalId == Guid.Empty.ToString())
                return;

            _objCharacter.LimitModifiers.Add(objLimitModifier);

            //Add the new treeview node for the LimitModifier.
            objNode.ContextMenuStrip = cmsLimitModifier;
            objNode.Text = objLimitModifier.DisplayName;
            objNode.Tag = objLimitModifier.InternalId;
            objSelectedNode.Parent.Nodes.Add(objNode);
            objSelectedNode.Remove();
        }

        /// <summary>
        /// Clears and updates the treeview for Spells. Typically called as part of AddQuality or UpdateCharacterInfo.
        /// </summary>
        /// <param name="treSpells">Treenode that will be cleared and populated.</param>
        /// <param name="cmsSpell">ContextMenuStrip that will be added to each power.</param>
        protected static void RefreshSpells(TreeView treSpells, ContextMenuStrip cmsSpell, Character _objCharacter)
        {
            //Clear the default nodes of entries.
            foreach (TreeNode objNode in treSpells.Nodes)
            {
                objNode.Nodes.Clear();
            }
            //Add the Spells that exist.
            foreach (Spell s in _objCharacter.Spells)
            {
                treSpells.Add(s, cmsSpell);
            }
        }

        /// <summary>
        /// Clears and updates the treeview for Critter Powers. Typically called as part of AddQuality or UpdateCharacterInfo.
        /// </summary>
        /// <param name="treCritterPowers">Treenode that will be cleared and populated.</param>
        /// <param name="cmsCritterPowers">ContextMenuStrip that will be added to each power.</param>
        protected void RefreshCritterPowers(TreeView treCritterPowers, ContextMenuStrip cmsCritterPowers)
        {
            //Clear the default nodes of entries.
            foreach (TreeNode objNode in treCritterPowers.Nodes)
            {
                objNode.Nodes.Clear();
            }
            //Add the Critter Powers that exist.
            foreach (CritterPower objPower in _objCharacter.CritterPowers)
            {
                TreeNode objNode = new TreeNode
                {
                    Text = objPower.DisplayName(GlobalOptions.Language),
                    Tag = objPower.InternalId,
                    ContextMenuStrip = cmsCritterPowers
                };
                if (!string.IsNullOrEmpty(objPower.Notes))
                    objNode.ForeColor = Color.SaddleBrown;
                objNode.ToolTipText = objPower.Notes.WordWrap(100);

                if (objPower.Category != "Weakness")
                {
                    treCritterPowers.Nodes[0].Nodes.Add(objNode);
                    treCritterPowers.Nodes[0].Expand();
                }
                else
                {
                    treCritterPowers.Nodes[1].Nodes.Add(objNode);
                    treCritterPowers.Nodes[1].Expand();
                }
            }
        }

        /// <summary>
        /// Refreshes the list of qualities into the selected TreeNode. If the same number of 
        /// </summary>
        /// <param name="treQualities">Treeview to insert the qualities into.</param>
        /// <param name="cmsQuality">ContextMenuStrip to add to each Quality node.</param>
        /// <param name="blnForce">Forces a refresh of the TreeNode despite a match.</param>
        protected void RefreshQualities(TreeView treQualities, ContextMenuStrip cmsQuality, bool blnForce = false)
        {
            //Count the child nodes in each treenode.
            int intQualityCount = 0;
            if (!blnForce)
            {
                foreach (TreeNode objTreeNode in treQualities.Nodes)
                {
                    intQualityCount += objTreeNode.Nodes.Count;
                }
            }

            //If the node count is the same as the quality count, there's no need to do anything.
            if (blnForce || intQualityCount != _objCharacter.Qualities.Count)
            {
                // Multiple instances of the same quality are combined into just one entry with a number next to it (e.g. 6 discrete entries of "Focused Concentration" become "Focused Concentration 6")
                HashSet<string> strQualitiesToPrint = new HashSet<string>();
                foreach (TreeNode objTreeNode in treQualities.Nodes)
                {
                    objTreeNode.Nodes.Clear();
                }
                foreach (Quality objQuality in _objCharacter.Qualities)
                {
                    strQualitiesToPrint.Add(objQuality.QualityId + " " + objQuality.GetSourceName(GlobalOptions.Language) + " " + objQuality.Extra);
                }
                // Populate the Qualities list.
                foreach (Quality objQuality in _objCharacter.Qualities)
                {
                    if (!strQualitiesToPrint.Remove(objQuality.QualityId + " " + objQuality.GetSourceName(GlobalOptions.Language) + " " + objQuality.Extra))
                        continue;
                    TreeNode objNode = new TreeNode
                    {
                        Text = objQuality.DisplayName(GlobalOptions.Language),
                        Tag = objQuality.InternalId,
                        ContextMenuStrip = cmsQuality
                    };

                    if (!string.IsNullOrEmpty(objQuality.Notes))
                        objNode.ForeColor = Color.SaddleBrown;
                    else if (objQuality.OriginSource == QualitySource.Metatype ||
                            objQuality.OriginSource == QualitySource.MetatypeRemovable ||
                            objQuality.OriginSource == QualitySource.Improvement)
                    {
                        objNode.ForeColor = SystemColors.GrayText;
                    }
                    objNode.ToolTipText = objQuality.Notes.WordWrap(100);

                    switch (objQuality.Type)
                    {
                        case QualityType.Positive:
                            treQualities.Nodes[0].Nodes.Add(objNode);
                            treQualities.Nodes[0].Expand();
                            break;
                        case QualityType.Negative:
                            treQualities.Nodes[1].Nodes.Add(objNode);
                            treQualities.Nodes[1].Expand();
                            break;
                        case QualityType.LifeModule:
                            treQualities.Nodes[2].Nodes.Add(objNode);
                            treQualities.Nodes[2].Expand();
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Refreshes all the names of qualities in the nodes
        /// </summary>
        /// <param name="treQualities">Treeview to insert the qualities into.</param>
        protected void RefreshQualityNames(TreeView treQualities)
        {
            TreeNode objSelectedNode = null;
            foreach (Quality objQuality in _objCharacter.Qualities)
            {
                for (int i = 0; i <= 1; i++)
                {
                    foreach (TreeNode objTreeNode in treQualities.Nodes[i].Nodes)
                    {
                        if (objSelectedNode == null && objTreeNode == treQualities.SelectedNode)
                            objSelectedNode = objTreeNode;
                        if (objTreeNode.Tag.ToString() == objQuality.InternalId)
                        {
                            objTreeNode.Text = objQuality.DisplayName(GlobalOptions.Language);
                            goto NextQuality;
                        }
                    }
                }
                NextQuality:;
            }
            if (objSelectedNode != null)
                treQualities.SelectedNode = objSelectedNode;
        }

        /// <summary>
        /// Method for removing old <addqualities /> nodes from existing characters.
        /// </summary>
        /// <param name="objNodeList">XmlNode to load. Expected to be addqualities/addquality</param>
        /// <param name="treQualities"></param>
        /// <param name="_objImprovementManager"></param>
        protected void RemoveAddedQualities(XmlNodeList objNodeList, TreeView treQualities)
        {
            foreach (XmlNode objNode in objNodeList)
            {
                foreach (Quality objQuality in _objCharacter.Qualities)
                {
                    if (objQuality.Name == objNode.InnerText)
                    {
                        _objCharacter.Qualities.Remove(objQuality);
                        ImprovementManager.RemoveImprovements(_objCharacter, Improvement.ImprovementSource.CritterPower, objQuality.InternalId);
                        if (!_objCharacter.Qualities.Any(objExistingQuality => objExistingQuality.Name == objQuality.Name && objExistingQuality.Extra == objQuality.Extra))
                        {
                            switch (objQuality.Type)
                            {
                                case QualityType.Positive:
                                    foreach (TreeNode nodQuality in treQualities.Nodes[0].Nodes)
                                    {
                                        if (nodQuality.Text == objQuality.Name)
                                            nodQuality.Remove();
                                    }
                                    break;
                                case QualityType.Negative:
                                    foreach (TreeNode nodQuality in treQualities.Nodes[1].Nodes)
                                    {
                                        if (nodQuality.Text == objQuality.Name)
                                            nodQuality.Remove();
                                    }
                                    break;
                            }
                        }
                        else
                            RefreshQualityNames(treQualities);
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Add a mugshot to the character.
        /// </summary>
        protected bool AddMugshot()
        {
            bool blnSuccess = false;
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (!string.IsNullOrWhiteSpace(_objOptions.RecentImageFolder) && Directory.Exists(_objOptions.RecentImageFolder))
            {
                openFileDialog.InitialDirectory = _objOptions.RecentImageFolder;
            }
            // Prompt the user to select an image to associate with this character.

            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageEncoders();
            openFileDialog.Filter = string.Format("All image files ({1})|{1}|{0}|All files|*",
                string.Join("|",
                    codecs.Select(codec => string.Format("{0} ({1})|{1}", codec.CodecName, codec.FilenameExtension)).ToArray()),
                string.Join(";", codecs.Select(codec => codec.FilenameExtension).ToArray()));

            if (openFileDialog.ShowDialog(this) == DialogResult.OK)
            {
                blnSuccess = true;
                // Convert the image to a string usinb Base64.
                _objOptions.RecentImageFolder = Path.GetDirectoryName(openFileDialog.FileName);

                Bitmap imgMugshot = (new Bitmap(openFileDialog.FileName, true)).ConvertPixelFormat(PixelFormat.Format32bppPArgb);

                _objCharacter.Mugshots.Add(imgMugshot);
                if (_objCharacter.MainMugshotIndex == -1)
                    _objCharacter.MainMugshotIndex = _objCharacter.Mugshots.Count - 1;
            }
            return blnSuccess;
        }

        /// <summary>
        /// Update the mugshot info of a character.
        /// </summary>
        /// <param name="picMugshot"></param>
        /// <param name="intCurrentMugshotIndexInList"></param>
        protected bool UpdateMugshot(PictureBox picMugshot, int intCurrentMugshotIndexInList)
        {
            if (intCurrentMugshotIndexInList < 0 || intCurrentMugshotIndexInList >= _objCharacter.Mugshots.Count || _objCharacter.Mugshots[intCurrentMugshotIndexInList] == null)
            {
                picMugshot.Image = null;
                return false;
            }

            Image imgMugshot = _objCharacter.Mugshots[intCurrentMugshotIndexInList];

            if (imgMugshot != null && picMugshot.Height >= imgMugshot.Height && picMugshot.Width >= imgMugshot.Width)
                picMugshot.SizeMode = PictureBoxSizeMode.CenterImage;
            else
                picMugshot.SizeMode = PictureBoxSizeMode.Zoom;
            picMugshot.Image = imgMugshot;

            return true;
        }

        /// <summary>
        /// Remove a mugshot of a character.
        /// </summary>
        /// <param name="intCurrentMugshotIndexInList"></param>
        protected void RemoveMugshot(int intCurrentMugshotIndexInList)
        {
            if (intCurrentMugshotIndexInList < 0 || intCurrentMugshotIndexInList >= _objCharacter.Mugshots.Count)
            {
                return;
            }

            _objCharacter.Mugshots.RemoveAt(intCurrentMugshotIndexInList);
            if (intCurrentMugshotIndexInList == _objCharacter.MainMugshotIndex)
            {
                _objCharacter.MainMugshotIndex = -1;
            }
            else if (intCurrentMugshotIndexInList < _objCharacter.MainMugshotIndex)
            {
                _objCharacter.MainMugshotIndex -= 1;
            }
        }

        public bool IsDirty
        {
            get
            {
                return _blnIsDirty;
            }
            set
            {
                if (_blnIsDirty != value)
                {
                    _blnIsDirty = value;
                    UpdateWindowTitle(true);
                }
            }
        }

        public void ScheduleCharacterUpdate()
        {
            _blnRequestCharacterUpdate = true;
        }

        public bool IsCharacterUpdateRequested
        {
            get
            {
                return _blnRequestCharacterUpdate;
            }
            set
            {
                _blnRequestCharacterUpdate = value;
            }
        }

        public Character CharacterObject
        {
            get
            {
                return _objCharacter;
            }
        }

        public CharacterOptions CharacterObjectOptions
        {
            get
            {
                return _objOptions;
            }
        }

        private void objCharacter_CharacterNameChanged(object sender)
        {
            UpdateWindowTitle(false);
        }

        public virtual string FormMode
        {
            get
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Update the Window title to show the Character's name and unsaved changes status.
        /// </summary>
        public void UpdateWindowTitle(bool blnCanSkip)
        {
            if (Text.EndsWith('*') == _blnIsDirty && blnCanSkip)
                return;
            
            string strTitle = _objCharacter.CharacterName + " - " + FormMode + " (" + _objCharacter.Options.Name + ")";
            if (_blnIsDirty)
                strTitle += '*';
            this.DoThreadSafe(() => Text = strTitle);
        }

        /// <summary>
        /// Save the Character.
        /// </summary>
        public virtual bool SaveCharacter(bool blnNeedConfirm = true, bool blnDoCreated = false)
        {
            // If the Character does not have a file name, trigger the Save As menu item instead.
            if (string.IsNullOrEmpty(_objCharacter.FileName))
            {
                return SaveCharacterAs();
            }
            // If the Created is checked, make sure the user wants to actually save this character.
            if (blnDoCreated)
            {
                if (blnNeedConfirm && !ConfirmSaveCreatedCharacter())
                {
                    return false;
                }
            }

            Cursor = Cursors.WaitCursor;
            if (_objCharacter.Save())
            {
                GlobalOptions.AddToMRUList(_objCharacter.FileName, "mru", true, true);
                IsDirty = false;
                Cursor = Cursors.Default;

                // If this character has just been saved as Created, close this form and re-open the character which will open it in the Career window instead.
                if (blnDoCreated)
                {
                    SaveCharacterAsCreated();
                }

                return true;
            }
            Cursor = Cursors.Default;
            return false;
        }

        /// <summary>
        /// Save the Character using the Save As dialogue box.
        /// </summary>
        public virtual bool SaveCharacterAs(bool blnDoCreated = false)
        {
            // If the Created is checked, make sure the user wants to actually save this character.
            if (blnDoCreated)
            {
                if (!ConfirmSaveCreatedCharacter())
                {
                    return false;
                }
            }

            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Chummer5 Files (*.chum5)|*.chum5|All Files (*.*)|*.*"
            };

            string strShowFileName = string.Empty;
            string[] strFile = _objCharacter.FileName.Split(Path.DirectorySeparatorChar);
            strShowFileName = strFile[strFile.Length - 1];

            if (string.IsNullOrEmpty(strShowFileName))
                strShowFileName = _objCharacter.CharacterName;

            saveFileDialog.FileName = strShowFileName;

            if (saveFileDialog.ShowDialog(this) == DialogResult.OK)
            {
                _objCharacter.FileName = saveFileDialog.FileName;
                return SaveCharacter(false);
            }

            return false;
        }

        /// <summary>
        /// Save the character as Created and re-open it in Career Mode.
        /// </summary>
        public virtual void SaveCharacterAsCreated() { }

        /// <summary>
        /// Verify that the user wants to save this character as Created.
        /// </summary>
        public virtual bool ConfirmSaveCreatedCharacter() { return true; }

        /// <summary>
        /// Processes the string strDrain into a calculated Drain dicepool and appropriate display attributes and labels.
        /// </summary>
        /// <param name="strDrain"></param>
        /// <param name="objImprovementManager"></param>
        /// <param name="drain"></param>
        /// <param name="attributeText"></param>
        /// <param name="valueText"></param>
        /// <param name="tooltip"></param>
        public void CalculateTraditionDrain(string strDrain, Improvement.ImprovementType drain, Label attributeText = null, Label valueText = null, ToolTip tooltip = null)
        {
            if (string.IsNullOrWhiteSpace(strDrain) || (attributeText == null && valueText == null && tooltip == null))
                return;
            StringBuilder objDrain = valueText != null ? new StringBuilder(strDrain) : null;
            StringBuilder objDisplayDrain = attributeText != null ? new StringBuilder(strDrain) : null;
            StringBuilder objTip = tooltip != null ? new StringBuilder(strDrain) : null;
            int intDrain = 0;
            // Update the Fading CharacterAttribute Value.
            foreach (string strAttribute in AttributeSection.AttributeStrings)
            {
                CharacterAttrib objAttrib = _objCharacter.GetAttribute(strAttribute);
                if (strDrain.Contains(objAttrib.Abbrev))
                {
                    string strAttribTotalValue = objAttrib.TotalValue.ToString();
                    objDrain?.Replace(objAttrib.Abbrev, strAttribTotalValue);
                    objDisplayDrain?.Replace(objAttrib.Abbrev, objAttrib.DisplayAbbrev);
                    objTip?.Replace(objAttrib.Abbrev, objAttrib.DisplayAbbrev + " (" + strAttribTotalValue + ")");
                }
            }
            if (objDrain != null)
            {
                try
                {
                    intDrain = Convert.ToInt32(Math.Ceiling((double)CommonFunctions.EvaluateInvariantXPath(objDrain.ToString())));
                }
                catch (XPathException) { }
                catch (OverflowException) { } // Result is text and not a double
                catch (InvalidCastException) { } // Result is text and not a double
            }

            if (valueText != null || tooltip != null)
            {
                int intBonusDrain = ImprovementManager.ValueOf(_objCharacter, drain);
                if (intBonusDrain != 0)
                {
                    intDrain += intBonusDrain;
                    objTip?.Append(" + " + LanguageManager.GetString("Tip_Modifiers", GlobalOptions.Language) + " (" + intBonusDrain.ToString() + ")");
                }
            }

            if (attributeText != null)
                attributeText.Text = objDisplayDrain.ToString();
            if (valueText != null)
                valueText.Text = intDrain.ToString();
            if (tooltip != null)
                tooltip.SetToolTip(valueText, objTip.ToString());
        }
    }
}
