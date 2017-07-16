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
using Chummer.Skills;
using System.Xml;
using System.Xml.XPath;
using Chummer.Backend.Attributes;
using TheArtOfDev.HtmlRenderer.WinForms;

namespace Chummer
{
    /// <summary>
    /// Contains functionality shared between frmCreate and frmCareer
    /// </summary>
    [System.ComponentModel.DesignerCategory("")]
    public class CharacterShared : Form
    {
        protected Character _objCharacter;
        protected MainController _objController;
        protected CharacterOptions _objOptions;
        protected CommonFunctions _objFunctions;

        public CharacterShared()
        {
            _gunneryCached = new Lazy<Skill>(() => _objCharacter.SkillsSection.Skills.First(x => x.Name == "Gunnery"));
        }

        /// <summary>
        /// Wrapper for relocating contact forms. 
        /// </summary>
        public class TransportWrapper
        {
            private readonly Control _control;

            public TransportWrapper(Control control)
            {
                _control = control;
            }

            public Control Control
            {
                get { return _control; }
            }
        }

        public Stopwatch Autosave_StopWatch = Stopwatch.StartNew();
        /// <summary>
        /// Automatically Save the character to a backup folder.
        /// </summary>
        public void AutoSaveCharacter()
        {
            if (!Directory.Exists(Path.Combine(Application.StartupPath, "saves", "autosave")))
                Directory.CreateDirectory(Path.Combine(Application.StartupPath, "saves", "autosave"));
            
            string[] strFile = _objCharacter.FileName.Split(Path.DirectorySeparatorChar);
            string strShowFileName = strFile[strFile.Length - 1];

            if (string.IsNullOrEmpty(strShowFileName))
                strShowFileName = _objCharacter.Alias;
            string strFilePath = Path.Combine(Application.StartupPath, "saves", "autosave", strShowFileName);
            try
            {
                _objCharacter.Save(strFilePath);
            }
            catch
            {
                // ignored, no point crashing the application if we can't backup.
            }
            Autosave_StopWatch.Restart();
        }
        protected void RedlinerCheck()
        {
            string strSeekerImprovPrefix = "SEEKER";
            var lstSeekerAttributes = new List<string>();
            var lstSeekerImprovements = new List<Improvement>();
            //Get attributes affected by redliner/cyber singularity seeker
            foreach (Improvement objLoopImprovement in _objCharacter.Improvements)
            {
                if (objLoopImprovement.ImproveType == Improvement.ImprovementType.Seeker)
                {
                    lstSeekerAttributes.Add(objLoopImprovement.ImprovedName);
                }
                else if ((objLoopImprovement.ImproveType == Improvement.ImprovementType.Attribute ||
                       objLoopImprovement.ImproveType == Improvement.ImprovementType.PhysicalCM) &&
                      objLoopImprovement.SourceName.Contains(strSeekerImprovPrefix))
                {
                    lstSeekerImprovements.Add(objLoopImprovement);
                }
            }

            //if neither contains anything, it is safe to exit
            if (lstSeekerImprovements.Count == 0 && lstSeekerAttributes.Count == 0)
            {
                _objCharacter.RedlinerBonus = 0;
                return;
            }

            //Calculate bonus from cyberlimbs
            int count = 0;
            foreach (Cyberware objCyberware in _objCharacter.Cyberware)
            {
                count += objCyberware.CyberlimbCount;
            }
            count = Math.Min(count/2, 2);
            if (lstSeekerImprovements.Any(x => x.ImprovedName == "STR" || x.ImprovedName == "AGI"))
            {
                _objCharacter.RedlinerBonus = count;
            }
            else
            {
                _objCharacter.RedlinerBonus = 0;
            }

            for (int i = 0; i < lstSeekerAttributes.Count; i++)
            {
                Improvement objImprove =
                    lstSeekerImprovements.FirstOrDefault(
                        x =>
                            x.SourceName == strSeekerImprovPrefix + "_" + lstSeekerAttributes[i] &&
                            x.Value == (lstSeekerAttributes[i] == "BOX" ? count*-3 : count));
                if (objImprove != null)
                {
                    lstSeekerAttributes.RemoveAt(i);
                    lstSeekerImprovements.Remove(objImprove);
                    i--;
                }
            }
            //Improvement manager defines the functions we need to manipulate improvements
            //When the locals (someday) gets moved to this class, this can be removed and use
            //the local
            Lazy<ImprovementManager> manager = new Lazy<ImprovementManager>(() => new ImprovementManager(_objCharacter));

            // Remove which qualites have been removed or which values have changed
            foreach (Improvement improvement in lstSeekerImprovements)
            {
                manager.Value.RemoveImprovements(improvement.ImproveSource, improvement.SourceName);
            }

            // Add new improvements or old improvements with new values
            foreach (string attribute in lstSeekerAttributes)
            {
                if (attribute == "BOX")
                {
                    manager.Value.CreateImprovement(attribute, Improvement.ImprovementSource.Quality,
                        strSeekerImprovPrefix + "_" + attribute, Improvement.ImprovementType.PhysicalCM,
                        Guid.NewGuid().ToString(), count*-3);
                }
                else
                {
                    manager.Value.CreateImprovement(attribute, Improvement.ImprovementSource.Quality,
                        strSeekerImprovPrefix + "_" + attribute, Improvement.ImprovementType.Attribute,
                        Guid.NewGuid().ToString(), count, 1, 0, 0, count);
                }
            }
            if (manager.IsValueCreated)
            {
                manager.Value.Commit(); //REFACTOR! WHEN MOVING MANAGER, change this to bool
            }
        }

        /// <summary>
        /// Update the label and tooltip for the character's Condition Monitors.
        /// </summary>
        /// <param name="lblPhysical"></param>
        /// <param name="lblStun"></param>
        /// <param name="tipTooltip"></param>
        /// <param name="_objImprovementManager"></param>
        protected void UpdateConditionMonitor(Label lblPhysical, Label lblStun, HtmlToolTip tipTooltip,
            ImprovementManager _objImprovementManager)
        {
            // Condition Monitor.
            int intBOD = _objCharacter.BOD.TotalValue;
            int intWIL = _objCharacter.WIL.TotalValue;
            int intCMPhysical = _objCharacter.PhysicalCM;
            int intCMStun = _objCharacter.StunCM;

            // Update the Condition Monitor labels.
            lblPhysical.Text = intCMPhysical.ToString();
            lblStun.Text = intCMStun.ToString();
            string strCM = $"8 + ({_objCharacter.BOD.DisplayAbbrev}/2)({(intBOD + 1)/2})";
            if (_objImprovementManager.ValueOf(Improvement.ImprovementType.PhysicalCM) != 0)
                strCM += " + " + LanguageManager.Instance.GetString("Tip_Modifiers") + " (" +
                         _objImprovementManager.ValueOf(Improvement.ImprovementType.PhysicalCM) + ")";
            tipTooltip.SetToolTip(lblPhysical, strCM);
            strCM = $"8 + ({_objCharacter.WIL.DisplayAbbrev}/2)({(intWIL + 1) / 2})";
            if (_objImprovementManager.ValueOf(Improvement.ImprovementType.StunCM) != 0)
                strCM += " + " + LanguageManager.Instance.GetString("Tip_Modifiers") + " (" +
                         _objImprovementManager.ValueOf(Improvement.ImprovementType.StunCM) + ")";
            tipTooltip.SetToolTip(lblStun, strCM);
        }

        /// <summary>
        /// Update the label and tooltip for the character's Armor Rating.
        /// </summary>
        /// <param name="lblArmor"></param>
        /// <param name="tipTooltip"></param>
        /// <param name="objImprovementManager"></param>
        /// <param name="lblCMArmor"></param>
        protected void UpdateArmorRating(Label lblArmor, HtmlToolTip tipTooltip, ImprovementManager objImprovementManager,
            Label lblCMArmor = null)
        {
            // Armor Ratings.
            lblArmor.Text = _objCharacter.TotalArmorRating.ToString();
            string strArmorToolTip = LanguageManager.Instance.GetString("Tip_Armor") + " (" + _objCharacter.ArmorRating + ")";
            if (_objCharacter.ArmorRating != _objCharacter.TotalArmorRating)
                strArmorToolTip += " + " + LanguageManager.Instance.GetString("Tip_Modifiers") + " (" +
                                   (_objCharacter.TotalArmorRating - _objCharacter.ArmorRating) + ")";
            tipTooltip.SetToolTip(lblArmor, strArmorToolTip);
            if (lblCMArmor != null)
            {
                lblCMArmor.Text = _objCharacter.TotalArmorRating.ToString();
                tipTooltip.SetToolTip(lblCMArmor, strArmorToolTip);
            }

            // Remove any Improvements from Armor Encumbrance.
            objImprovementManager.RemoveImprovements(Improvement.ImprovementSource.ArmorEncumbrance, "Armor Encumbrance");
            // Create the Armor Encumbrance Improvements.
            if (_objCharacter.ArmorEncumbrance < 0)
            {
                objImprovementManager.CreateImprovement("AGI", Improvement.ImprovementSource.ArmorEncumbrance, "Armor Encumbrance",
                    Improvement.ImprovementType.Attribute, "precedence-1", 0, 1, 0, 0, _objCharacter.ArmorEncumbrance);
                objImprovementManager.CreateImprovement("REA", Improvement.ImprovementSource.ArmorEncumbrance, "Armor Encumbrance",
                    Improvement.ImprovementType.Attribute, "precedence-1", 0, 1, 0, 0, _objCharacter.ArmorEncumbrance);
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
        protected void RefreshLimits(Label lblPhysical, Label lblMental, Label lblSocial, Label lblAstral, HtmlToolTip tipTooltip)
        {
            lblPhysical.Text = _objCharacter.LimitPhysical;
            lblMental.Text = _objCharacter.LimitMental.ToString();
            lblSocial.Text = _objCharacter.LimitSocial.ToString();

            string strPhysical =
                $"({_objCharacter.STR.DisplayAbbrev} [{_objCharacter.STR.TotalValue}] * 2) + {_objCharacter.BOD.DisplayAbbrev} [{_objCharacter.BOD.TotalValue}] + {_objCharacter.REA.DisplayAbbrev} [{_objCharacter.REA.TotalValue}] / 3";
            string strMental =
                $"({_objCharacter.LOG.DisplayAbbrev} [{_objCharacter.LOG.TotalValue}] * 2) + {_objCharacter.INT.DisplayAbbrev} [{_objCharacter.INT.TotalValue}] + {_objCharacter.WIL.DisplayAbbrev} [{_objCharacter.WIL.TotalValue}] / 3";
            string strSocial =
                $"({_objCharacter.CHA.DisplayAbbrev} [{_objCharacter.CHA.TotalValue}] * 2) + {_objCharacter.WIL.DisplayAbbrev} [{_objCharacter.WIL.TotalValue}] + {_objCharacter.ESS.DisplayAbbrev} [{_objCharacter.Essence.ToString(GlobalOptions.CultureInfo)}] / 3";

            foreach (Improvement objLoopImprovement in _objCharacter.Improvements.Where(
                objLoopImprovment => (objLoopImprovment.ImproveType == Improvement.ImprovementType.PhysicalLimit 
                || objLoopImprovment.ImproveType == Improvement.ImprovementType.SocialLimit 
                || objLoopImprovment.ImproveType == Improvement.ImprovementType.MentalLimit) && objLoopImprovment.Enabled))
                {
                    switch (objLoopImprovement.ImproveType)
                    {
                        case Improvement.ImprovementType.PhysicalLimit:
                            strPhysical += $" + {_objCharacter.GetObjectName(objLoopImprovement)} ({objLoopImprovement.Value})";
                            break;
                        case Improvement.ImprovementType.MentalLimit:
                            strMental += $" + {_objCharacter.GetObjectName(objLoopImprovement)} ({objLoopImprovement.Value})";
                            break;
                        case Improvement.ImprovementType.SocialLimit:
                            strSocial += $" + {_objCharacter.GetObjectName(objLoopImprovement)} ({objLoopImprovement.Value})";
                            break;
                    }
                }

            tipTooltip.SetToolTip(lblPhysical, strPhysical);
            tipTooltip.SetToolTip(lblMental, strMental);
            tipTooltip.SetToolTip(lblSocial, strSocial);

            lblAstral.Text = _objCharacter.LimitAstral.ToString();
        }

        private readonly Lazy<Skill> _gunneryCached;

        protected int MountedGunManualOperationDicePool(Weapon weapon)
        {
            return _gunneryCached.Value.Pool;
        }

        protected int MountedGunCommandDeviceDicePool(Weapon weapon)
        {
            return _gunneryCached.Value.PoolOtherAttribute(_objCharacter.LOG.TotalValue);
        }

        protected int MountedGunDogBrainDicePool(Weapon weapon, Vehicle vehicle)
        {
            int pilotRating = vehicle.Pilot;

            Gear maybeAutoSoft =
                vehicle.Gear.SelectMany(x => x.ThisAndAllChildren())
                    .FirstOrDefault(x => x.Name == "[Weapon] Targeting Autosoft" && (x.Extra == weapon.Name || x.Extra == weapon.DisplayName));

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
            LimitModifier objLimitModifier = CommonFunctions.FindByIdWithNameCheck(treLimit.SelectedNode.Tag.ToString(),
                _objCharacter.LimitModifiers);
            //If the LimitModifier couldn't be found (Ie it comes from an Improvement or the user hasn't properly selected a treenode, fail out early.
            if (objLimitModifier == null)
            {
                MessageBox.Show(LanguageManager.Instance.GetString("Warning_NoLimitFound"));
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
                strCondition, _objCharacter, objNode);
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
        protected void RefreshSpells(helpers.TreeView treSpells, ContextMenuStrip cmsSpell, Character _objCharacter)
        {
            //Clear the default nodes of entries.
            foreach (TreeNode objNode in treSpells.Nodes)
            {
                objNode.Nodes.Clear();
            }
            //Add the Spells that exist.
            foreach (Spell s in _objCharacter.Spells)
            {
                treSpells.Add(s, cmsSpell, _objCharacter);
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
                TreeNode objNode = new TreeNode();
                objNode.Text = objPower.DisplayName;
                objNode.Tag = objPower.InternalId;
                objNode.ContextMenuStrip = cmsCritterPowers;
                if (!string.IsNullOrEmpty(objPower.Notes))
                    objNode.ForeColor = Color.SaddleBrown;
                objNode.ToolTipText = CommonFunctions.WordWrap(objPower.Notes, 100);

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
            foreach (TreeNode objTreeNode in treQualities.Nodes)
            {
                intQualityCount += objTreeNode.Nodes.Count;
            }

            //If the node count is the same as the quality count, there's no need to do anything.
            if (intQualityCount != _objCharacter.Qualities.Count || blnForce)
            {
                foreach (TreeNode objTreeNode in treQualities.Nodes)
                {
                    objTreeNode.Nodes.Clear();
                }
                // Populate the Qualities list.
                foreach (Quality objQuality in _objCharacter.Qualities)
                {
                    TreeNode objNode = new TreeNode();
                    objNode.Text = objQuality.DisplayName;
                    objNode.Tag = objQuality.InternalId;
                    objNode.ContextMenuStrip = cmsQuality;

                    if (!string.IsNullOrEmpty(objQuality.Notes))
                        objNode.ForeColor = Color.SaddleBrown;
                    else
                    {
                        if (objQuality.OriginSource == QualitySource.Metatype ||
                            objQuality.OriginSource == QualitySource.MetatypeRemovable)
                            objNode.ForeColor = SystemColors.GrayText;
                    }
                    objNode.ToolTipText = CommonFunctions.WordWrap(objQuality.Notes, 100);

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
        /// Method for removing old <addqualities /> nodes from existing characters.
        /// </summary>
        /// <param name="objNodeList">XmlNode to load. Expected to be addqualities/addquality</param>
        /// <param name="treQualities"></param>
        /// <param name="_objImprovementManager"></param>
        protected void RemoveAddedQualities(XmlNodeList objNodeList, TreeView treQualities,
            ImprovementManager _objImprovementManager)
        {
            foreach (XmlNode objNode in objNodeList)
            {
                foreach (Quality objQuality in _objCharacter.Qualities)
                {
                    if (objQuality.Name == objNode.InnerText)
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
                        _objCharacter.Qualities.Remove(objQuality);
                        _objImprovementManager.RemoveImprovements(Improvement.ImprovementSource.CritterPower, objQuality.InternalId);
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Add a mugshot to the character.
        /// </summary>
        /// <param name="picMugshot"></param>
        protected bool AddMugshot(PictureBox picMugshot)
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

                Image imgMugshot = new Bitmap(openFileDialog.FileName, true);
                MemoryStream objStream = new MemoryStream();
                imgMugshot.Save(objStream, imgMugshot.RawFormat);
                string strResult = Convert.ToBase64String(objStream.ToArray());
                objStream.Close();

                _objCharacter.Mugshots.Add(strResult);
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
            if (intCurrentMugshotIndexInList < 0 || intCurrentMugshotIndexInList >= _objCharacter.Mugshots.Count || string.IsNullOrEmpty(_objCharacter.Mugshots[intCurrentMugshotIndexInList]))
            {
                picMugshot.Image = null;
                return false;
            }

            Image imgMugshot = null;
            byte[] bytImage = Convert.FromBase64String(_objCharacter.Mugshots[intCurrentMugshotIndexInList]);
            if (bytImage.Length > 0)
            {
                MemoryStream objImageStream = new MemoryStream(bytImage, 0, bytImage.Length);
                objImageStream.Write(bytImage, 0, bytImage.Length);
                imgMugshot = Image.FromStream(objImageStream, true);
                objImageStream.Close();
            }

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
                _objCharacter.MainMugshotIndex = 0;
            }
            else if (intCurrentMugshotIndexInList < _objCharacter.MainMugshotIndex)
            {
                _objCharacter.MainMugshotIndex -= 1;
            }
        }


        /// <summary>
        /// Processes the string strDrain into a calculated Drain dicepool and appropriate display attributes and labels.
        /// </summary>
        /// <param name="strDrain"></param>
        /// <param name="objImprovementManager"></param>
        /// <param name="drain"></param>
        /// <param name="attributeText"></param>
        /// <param name="valueText"></param>
        /// <param name="tooltip"></param>
        public void CalculateTraditionDrain(string strDrain, ImprovementManager objImprovementManager, Improvement.ImprovementType drain, Label attributeText, Label valueText, ToolTip tooltip)
        {
            if (string.IsNullOrWhiteSpace(strDrain))
                return;
            string strDisplayDrain = strDrain;
            string strTip = strDrain;
            var intDrain = 0;
            // Update the Fading CharacterAttribute Value.
            var objXmlDocument = new XmlDocument();
            XPathNavigator nav = objXmlDocument.CreateNavigator();
            foreach (string strAttribute in Character.AttributeStrings)
            {
                CharacterAttrib objAttrib = _objCharacter.GetAttribute(strAttribute);
                strDrain = strDrain.Replace(objAttrib.Abbrev, objAttrib.TotalValue.ToString());
                strDisplayDrain = strDisplayDrain.Replace(objAttrib.Abbrev, objAttrib.DisplayAbbrev);
            }
            XPathExpression xprFading = nav.Compile(strDrain);
            object o = nav.Evaluate(xprFading);
            if (o != null) intDrain = Convert.ToInt32(o.ToString());
            intDrain += objImprovementManager.ValueOf(drain);
            attributeText.Text = strDisplayDrain;
            valueText.Text = intDrain.ToString();
            strTip = Character.AttributeStrings.Select(strAttribute => _objCharacter.GetAttribute(strAttribute)).Aggregate(strTip, (current, objAttrib) => current.Replace(objAttrib.Abbrev, objAttrib.DisplayAbbrev + " (" + objAttrib.TotalValue.ToString() + ")"));
            tooltip.SetToolTip(valueText, strTip);
        }
    }
}