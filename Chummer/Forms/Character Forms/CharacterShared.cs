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
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;
using Chummer.Backend.Attributes;
using Chummer.Backend.Equipment;
using Chummer.UI.Attributes;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using NLog;

namespace Chummer
{
    /// <summary>
    /// Contains functionality shared between frmCreate and frmCareer
    /// </summary>
    [DesignerCategory("")]
    public class CharacterShared : Form
    {
        private static Logger Log { get; } = LogManager.GetCurrentClassLogger();
        private static TelemetryClient TelemetryClient { get; } = new TelemetryClient();
        private readonly Character _objCharacter;
        private bool _blnIsDirty;
        private bool _blnIsRefreshing;
        private bool _blnLoading = true;
        private CharacterSheetViewer _frmPrintView;

        protected CharacterShared(Character objCharacter)
        {
            _objCharacter = objCharacter;
            _objCharacter.PropertyChanged += RecacheSettingsOnSettingsChange;
            string name = "Show_Form_" + GetType();
            PageViewTelemetry pvt = new PageViewTelemetry(name)
            {
                Id = Guid.NewGuid().ToString(),
                Name = name,
                Timestamp = DateTimeOffset.UtcNow
            };
            pvt.Context.Operation.Name = "Operation CharacterShared.Constructor()";
            pvt.Properties.Add("Name", objCharacter?.Name);
            pvt.Properties.Add("Path", objCharacter?.FileName);
            Shown += delegate
            {
                pvt.Duration = DateTimeOffset.UtcNow - pvt.Timestamp;
                if (objCharacter != null && Uri.TryCreate(objCharacter.FileName, UriKind.Absolute, out Uri uriResult))
                {
                    pvt.Url = uriResult;
                }
                TelemetryClient.TrackPageView(pvt);
            };
        }

        private void RecacheSettingsOnSettingsChange(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Character.Settings))
            {
                _objCachedSettings = null;
                IsCharacterUpdateRequested = true;
                IsDirty = true;
            }
        }

        [Obsolete("This constructor is for use by form designers only.", true)]
        protected CharacterShared()
        {
        }

        /// <summary>
        /// Set up data bindings to set Dirty flag and/or the flag to request a character update when specific collections change
        /// </summary>
        /// <param name="blnAddBindings"></param>
        protected void SetupCommonCollectionDatabindings(bool blnAddBindings)
        {
            if (blnAddBindings)
            {
                CharacterObject.Spells.CollectionChanged += MakeDirtyWithCharacterUpdate;
                CharacterObject.ComplexForms.CollectionChanged += MakeDirtyWithCharacterUpdate;
                CharacterObject.Arts.CollectionChanged += MakeDirtyWithCharacterUpdate;
                CharacterObject.Enhancements.CollectionChanged += MakeDirtyWithCharacterUpdate;
                CharacterObject.Metamagics.CollectionChanged += MakeDirtyWithCharacterUpdate;
                CharacterObject.InitiationGrades.CollectionChanged += MakeDirtyWithCharacterUpdate;
                CharacterObject.Powers.ListChanged += MakeDirtyWithCharacterUpdate;
                CharacterObject.AIPrograms.CollectionChanged += MakeDirtyWithCharacterUpdate;
                CharacterObject.CritterPowers.CollectionChanged += MakeDirtyWithCharacterUpdate;
                CharacterObject.Qualities.CollectionChanged += MakeDirtyWithCharacterUpdate;
                CharacterObject.MartialArts.CollectionChanged += MakeDirtyWithCharacterUpdate;
                CharacterObject.Lifestyles.CollectionChanged += MakeDirtyWithCharacterUpdate;
                CharacterObject.Contacts.CollectionChanged += MakeDirtyWithCharacterUpdate;
                CharacterObject.Spirits.CollectionChanged += MakeDirtyWithCharacterUpdate;
                CharacterObject.Armor.CollectionChanged += MakeDirtyWithCharacterUpdate;
                CharacterObject.ArmorLocations.CollectionChanged += MakeDirty;
                CharacterObject.Weapons.CollectionChanged += MakeDirtyWithCharacterUpdate;
                CharacterObject.WeaponLocations.CollectionChanged += MakeDirty;
                CharacterObject.Gear.CollectionChanged += MakeDirtyWithCharacterUpdate;
                CharacterObject.GearLocations.CollectionChanged += MakeDirty;
                CharacterObject.Drugs.CollectionChanged += MakeDirtyWithCharacterUpdate;
                CharacterObject.Cyberware.CollectionChanged += MakeDirtyWithCharacterUpdate;
                CharacterObject.Vehicles.CollectionChanged += MakeDirtyWithCharacterUpdate;
                CharacterObject.VehicleLocations.CollectionChanged += MakeDirty;

                CharacterObject.Improvements.CollectionChanged += MakeDirtyWithCharacterUpdate;
                CharacterObject.ImprovementGroups.CollectionChanged += MakeDirty;
                CharacterObject.Calendar.ListChanged += MakeDirty;
                CharacterObject.SustainedCollection.CollectionChanged += MakeDirty;
                CharacterObject.ExpenseEntries.CollectionChanged += MakeDirtyWithCharacterUpdate;
            }
            else
            {
                CharacterObject.Spells.CollectionChanged -= MakeDirtyWithCharacterUpdate;
                CharacterObject.ComplexForms.CollectionChanged -= MakeDirtyWithCharacterUpdate;
                CharacterObject.Arts.CollectionChanged -= MakeDirtyWithCharacterUpdate;
                CharacterObject.Enhancements.CollectionChanged -= MakeDirtyWithCharacterUpdate;
                CharacterObject.Metamagics.CollectionChanged -= MakeDirtyWithCharacterUpdate;
                CharacterObject.InitiationGrades.CollectionChanged -= MakeDirtyWithCharacterUpdate;
                CharacterObject.Powers.ListChanged -= MakeDirtyWithCharacterUpdate;
                CharacterObject.AIPrograms.CollectionChanged -= MakeDirtyWithCharacterUpdate;
                CharacterObject.CritterPowers.CollectionChanged -= MakeDirtyWithCharacterUpdate;
                CharacterObject.Qualities.CollectionChanged -= MakeDirtyWithCharacterUpdate;
                CharacterObject.MartialArts.CollectionChanged -= MakeDirtyWithCharacterUpdate;
                CharacterObject.Lifestyles.CollectionChanged -= MakeDirtyWithCharacterUpdate;
                CharacterObject.Contacts.CollectionChanged -= MakeDirtyWithCharacterUpdate;
                CharacterObject.Spirits.CollectionChanged -= MakeDirtyWithCharacterUpdate;
                CharacterObject.Armor.CollectionChanged -= MakeDirtyWithCharacterUpdate;
                CharacterObject.ArmorLocations.CollectionChanged -= MakeDirty;
                CharacterObject.Weapons.CollectionChanged -= MakeDirtyWithCharacterUpdate;
                CharacterObject.WeaponLocations.CollectionChanged -= MakeDirty;
                CharacterObject.Gear.CollectionChanged -= MakeDirtyWithCharacterUpdate;
                CharacterObject.GearLocations.CollectionChanged -= MakeDirty;
                CharacterObject.Drugs.CollectionChanged -= MakeDirtyWithCharacterUpdate;
                CharacterObject.Cyberware.CollectionChanged -= MakeDirtyWithCharacterUpdate;
                CharacterObject.Vehicles.CollectionChanged -= MakeDirtyWithCharacterUpdate;
                CharacterObject.VehicleLocations.CollectionChanged -= MakeDirty;

                CharacterObject.Improvements.CollectionChanged -= MakeDirtyWithCharacterUpdate;
                CharacterObject.ImprovementGroups.CollectionChanged -= MakeDirty;
                CharacterObject.Calendar.ListChanged -= MakeDirty;
                CharacterObject.SustainedCollection.CollectionChanged -= MakeDirty;
                CharacterObject.ExpenseEntries.CollectionChanged -= MakeDirtyWithCharacterUpdate;
            }
        }

        /// <summary>
        /// Wrapper for relocating contact forms.
        /// </summary>
        protected readonly struct TransportWrapper : IEquatable<TransportWrapper>
        {
            public Control Control { get; }

            public TransportWrapper(Control objControl)
            {
                Control = objControl;
            }

            public bool Equals(TransportWrapper other)
            {
                return Control.Equals(other.Control);
            }

            public override bool Equals(object obj)
            {
                return Control.Equals(obj);
            }

            public static bool operator ==(TransportWrapper objX, TransportWrapper objY)
            {
                return objX.Equals(objY);
            }

            public static bool operator !=(TransportWrapper objX, TransportWrapper objY)
            {
                return !objX.Equals(objY);
            }

            public static bool operator ==(TransportWrapper objX, object objY)
            {
                return objX.Equals(objY);
            }

            public static bool operator !=(TransportWrapper objX, object objY)
            {
                return !objX.Equals(objY);
            }

            public static bool operator ==(object objX, TransportWrapper objY)
            {
                return objX?.Equals(objY) ?? false;
            }

            public static bool operator !=(object objX, TransportWrapper objY)
            {
                return objX?.Equals(objY) ?? false;
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

        protected Stopwatch AutosaveStopWatch { get; } = Stopwatch.StartNew();
        
        /// <summary>
        /// Automatically Save the character to a backup folder.
        /// </summary>
        protected async Task AutoSaveCharacter()
        {
            using (CursorWait.New(this, true))
            {
                try
                {
                    string strAutosavePath = Utils.GetAutosavesFolderPath;

                    if (!Directory.Exists(strAutosavePath))
                    {
                        try
                        {
                            Directory.CreateDirectory(strAutosavePath);
                        }
                        catch (UnauthorizedAccessException)
                        {
                            Program.ShowMessageBox(
                                this, await LanguageManager.GetStringAsync("Message_Insufficient_Permissions_Warning"));
                            return;
                        }
                    }

                    string strShowFileName = CharacterObject.FileName
                                                            .SplitNoAlloc(
                                                                Path.DirectorySeparatorChar,
                                                                StringSplitOptions.RemoveEmptyEntries).LastOrDefault();

                    if (string.IsNullOrEmpty(strShowFileName))
                        strShowFileName = CharacterObject.CharacterName + ".chum5";
                    foreach (char invalidChar in Path.GetInvalidFileNameChars())
                    {
                        strShowFileName = strShowFileName.Replace(invalidChar, '_');
                    }

                    string strFilePath = Path.Combine(strAutosavePath, strShowFileName);
                    if (!await CharacterObject.SaveAsync(strFilePath, false, false))
                    {
                        Log.Info("Autosave failed for character " + CharacterObject.CharacterName + " ("
                                 + CharacterObject.FileName + ')');
                    }
                }
                finally
                {
                    AutosaveStopWatch.Restart();
                }
            }
        }

        /// <summary>
        /// Edit and update a Limit Modifier.
        /// </summary>
        /// <param name="treLimit"></param>
        protected async ValueTask UpdateLimitModifier(TreeView treLimit)
        {
            if (treLimit == null || treLimit.SelectedNode.Level == 0)
                return;
            using (CursorWait.New(this))
            {
                TreeNode objSelectedNode = treLimit.SelectedNode;
                string strGuid = (objSelectedNode?.Tag as IHasInternalId)?.InternalId ?? string.Empty;
                if (string.IsNullOrEmpty(strGuid) || strGuid.IsEmptyGuid())
                    return;
                LimitModifier objLimitModifier = CharacterObject.LimitModifiers.FindById(strGuid);
                //If the LimitModifier couldn't be found (Ie it comes from an Improvement or the user hasn't properly selected a treenode, fail out early.
                if (objLimitModifier == null)
                {
                    Program.ShowMessageBox(this, await LanguageManager.GetStringAsync("Warning_NoLimitFound"));
                    return;
                }

                using (SelectLimitModifier frmPickLimitModifier
                       = new SelectLimitModifier(objLimitModifier, "Physical", "Mental", "Social"))
                {
                    await frmPickLimitModifier.ShowDialogSafeAsync(this);

                    if (frmPickLimitModifier.DialogResult == DialogResult.Cancel)
                        return;

                    //Remove the old LimitModifier to ensure we don't double up.
                    CharacterObject.LimitModifiers.Remove(objLimitModifier);
                    // Create the new limit modifier.
                    objLimitModifier = new LimitModifier(CharacterObject, strGuid);
                    objLimitModifier.Create(frmPickLimitModifier.SelectedName, frmPickLimitModifier.SelectedBonus,
                                            frmPickLimitModifier.SelectedLimitType,
                                            frmPickLimitModifier.SelectedCondition, true);

                    CharacterObject.LimitModifiers.Add(objLimitModifier);

                    IsCharacterUpdateRequested = true;

                    IsDirty = true;
                }
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="objNotes"></param>
        /// <param name="treNode"></param>
        protected async ValueTask WriteNotes(IHasNotes objNotes, TreeNode treNode)
        {
            if (objNotes == null)
                return;
            using (CursorWait.New(this))
            {
                using (EditNotes frmItemNotes = new EditNotes(objNotes.Notes, objNotes.NotesColor))
                {
                    await frmItemNotes.ShowDialogSafeAsync(this);
                    if (frmItemNotes.DialogResult != DialogResult.OK)
                        return;
                    objNotes.Notes = frmItemNotes.Notes;
                    objNotes.NotesColor = frmItemNotes.NotesColor;
                    IsDirty = true;
                    if (treNode != null)
                    {
                        treNode.ForeColor = objNotes.PreferredColor;
                        treNode.ToolTipText = objNotes.Notes.WordWrap();
                    }
                }
            }
        }

        #region Refresh Treeviews and Panels

        protected void RefreshAttributes(FlowLayoutPanel pnlAttributes, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs = null, Label lblName = null, int intKarmaWidth = -1, int intValueWidth = -1, int intLimitsWidth = -1)
        {
            if (pnlAttributes == null)
                return;
            using (CursorWait.New(this))
            {
                if (notifyCollectionChangedEventArgs == null ||
                    notifyCollectionChangedEventArgs.Action == NotifyCollectionChangedAction.Reset)
                {
                    pnlAttributes.SuspendLayout();
                    pnlAttributes.Controls.Clear();
                    if (CharacterObject.AttributeSection.Attributes.Count > 0)
                    {
                        int intNameWidth = lblName?.PreferredWidth ?? 0;
                        Control[] aobjControls = new Control[CharacterObject.AttributeSection.Attributes.Count];
                        for (int i = 0; i < CharacterObject.AttributeSection.Attributes.Count; ++i)
                        {
                            AttributeControl objControl =
                                new AttributeControl(CharacterObject.AttributeSection.Attributes[i]);
                            objControl.MinimumSize = new Size(pnlAttributes.ClientSize.Width,
                                objControl.MinimumSize.Height);
                            objControl.MaximumSize = new Size(pnlAttributes.ClientSize.Width,
                                objControl.MaximumSize.Height);
                            objControl.ValueChanged += MakeDirtyWithCharacterUpdate;
                            intNameWidth = Math.Max(intNameWidth, objControl.NameWidth);
                            aobjControls[i] = objControl;
                        }

                        if (lblName != null)
                            lblName.MinimumSize = new Size(intNameWidth, lblName.MinimumSize.Height);
                        foreach (AttributeControl objControl in aobjControls.OfType<AttributeControl>())
                            objControl.UpdateWidths(intNameWidth, intKarmaWidth, intValueWidth, intLimitsWidth);
                        pnlAttributes.Controls.AddRange(aobjControls);
                    }

                    pnlAttributes.ResumeLayout();
                }
                else
                {
                    switch (notifyCollectionChangedEventArgs.Action)
                    {
                        case NotifyCollectionChangedAction.Add:
                            {
                                bool blnVaryingAddedWidths = false;
                                int intNewNameWidth = -1;
                                Control[] aobjControls = new Control[notifyCollectionChangedEventArgs.NewItems.Count];
                                for (int i = 0; i < notifyCollectionChangedEventArgs.NewItems.Count; ++i)
                                {
                                    AttributeControl objControl =
                                        new AttributeControl(
                                            notifyCollectionChangedEventArgs.NewItems[i] as CharacterAttrib);
                                    objControl.MinimumSize = new Size(pnlAttributes.ClientSize.Width,
                                        objControl.MinimumSize.Height);
                                    objControl.MaximumSize = new Size(pnlAttributes.ClientSize.Width,
                                        objControl.MaximumSize.Height);
                                    objControl.ValueChanged += MakeDirtyWithCharacterUpdate;
                                    if (intNewNameWidth < 0)
                                        intNewNameWidth = objControl.NameWidth;
                                    else if (intNewNameWidth < objControl.NameWidth)
                                    {
                                        intNewNameWidth = objControl.NameWidth;
                                        blnVaryingAddedWidths = true;
                                    }

                                    aobjControls[i] = objControl;
                                }

                                int intOldNameWidth = lblName?.Width ??
                                                      (pnlAttributes.Controls.Count > 0
                                                          ? pnlAttributes.Controls[0].Width
                                                          : 0);
                                if (intNewNameWidth > intOldNameWidth)
                                {
                                    if (lblName != null)
                                        lblName.MinimumSize = new Size(intNewNameWidth, lblName.MinimumSize.Height);
                                    foreach (AttributeControl objControl in pnlAttributes.Controls)
                                        objControl.UpdateWidths(intNewNameWidth, intKarmaWidth, intValueWidth,
                                            intLimitsWidth);
                                    if (blnVaryingAddedWidths)
                                        foreach (AttributeControl objControl in aobjControls.OfType<AttributeControl>())
                                            objControl.UpdateWidths(intNewNameWidth, intKarmaWidth, intValueWidth,
                                                intLimitsWidth);
                                }
                                else
                                {
                                    foreach (AttributeControl objControl in aobjControls.OfType<AttributeControl>())
                                        objControl.UpdateWidths(intOldNameWidth, intKarmaWidth, intValueWidth,
                                            intLimitsWidth);
                                }

                                pnlAttributes.Controls.AddRange(aobjControls);
                            }
                            break;

                        case NotifyCollectionChangedAction.Remove:
                            {
                                foreach (CharacterAttrib objAttrib in notifyCollectionChangedEventArgs.OldItems)
                                {
                                    foreach (AttributeControl objControl in pnlAttributes.Controls)
                                    {
                                        if (objControl.AttributeName == objAttrib.Abbrev)
                                        {
                                            objControl.ValueChanged -= MakeDirtyWithCharacterUpdate;
                                            pnlAttributes.Controls.Remove(objControl);
                                            objControl.Dispose();
                                        }
                                    }

                                    if (!CharacterObject.Created)
                                    {
                                        objAttrib.Base = 0;
                                        objAttrib.Karma = 0;
                                    }
                                }
                            }
                            break;

                        case NotifyCollectionChangedAction.Replace:
                            {
                                foreach (CharacterAttrib objAttrib in notifyCollectionChangedEventArgs.OldItems)
                                {
                                    foreach (AttributeControl objControl in pnlAttributes.Controls)
                                    {
                                        if (objControl.AttributeName == objAttrib.Abbrev)
                                        {
                                            objControl.ValueChanged -= MakeDirtyWithCharacterUpdate;
                                            pnlAttributes.Controls.Remove(objControl);
                                            objControl.Dispose();
                                        }
                                    }

                                    if (!CharacterObject.Created)
                                    {
                                        objAttrib.Base = 0;
                                        objAttrib.Karma = 0;
                                    }
                                }

                                bool blnVaryingAddedWidths = false;
                                int intNewNameWidth = -1;
                                Control[] aobjControls = new Control[notifyCollectionChangedEventArgs.NewItems.Count];
                                for (int i = 0; i < notifyCollectionChangedEventArgs.NewItems.Count; ++i)
                                {
                                    AttributeControl objControl =
                                        new AttributeControl(
                                            notifyCollectionChangedEventArgs.NewItems[i] as CharacterAttrib);
                                    objControl.MinimumSize = new Size(pnlAttributes.ClientSize.Width,
                                        objControl.MinimumSize.Height);
                                    objControl.MaximumSize = new Size(pnlAttributes.ClientSize.Width,
                                        objControl.MaximumSize.Height);
                                    objControl.ValueChanged += MakeDirtyWithCharacterUpdate;
                                    if (intNewNameWidth < 0)
                                        intNewNameWidth = objControl.NameWidth;
                                    else if (intNewNameWidth < objControl.NameWidth)
                                    {
                                        intNewNameWidth = objControl.NameWidth;
                                        blnVaryingAddedWidths = true;
                                    }

                                    aobjControls[i] = objControl;
                                }

                                int intOldNameWidth = lblName?.Width ??
                                                      (pnlAttributes.Controls.Count > 0
                                                          ? pnlAttributes.Controls[0].Width
                                                          : 0);
                                if (intNewNameWidth > intOldNameWidth)
                                {
                                    if (lblName != null)
                                        lblName.MinimumSize = new Size(intNewNameWidth, lblName.MinimumSize.Height);
                                    foreach (AttributeControl objControl in pnlAttributes.Controls)
                                        objControl.UpdateWidths(intNewNameWidth, intKarmaWidth, intValueWidth,
                                            intLimitsWidth);
                                    if (blnVaryingAddedWidths)
                                        foreach (AttributeControl objControl in aobjControls.OfType<AttributeControl>())
                                            objControl.UpdateWidths(intNewNameWidth, intKarmaWidth, intValueWidth,
                                                intLimitsWidth);
                                }
                                else
                                {
                                    foreach (AttributeControl objControl in aobjControls.OfType<AttributeControl>())
                                        objControl.UpdateWidths(intOldNameWidth, intKarmaWidth, intValueWidth,
                                            intLimitsWidth);
                                }

                                pnlAttributes.Controls.AddRange(aobjControls);
                            }
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Clears and updates the TreeView for Spells. Typically called as part of AddQuality or UpdateCharacterInfo.
        /// </summary>
        /// <param name="treSpells">Spells tree.</param>
        /// <param name="treMetamagic">Initiations tree.</param>
        /// <param name="cmsSpell">ContextMenuStrip that will be added to spells in the spell tree.</param>
        /// <param name="cmsInitiationNotes">ContextMenuStrip that will be added to spells in the initiations tree.</param>
        /// <param name="notifyCollectionChangedEventArgs">Arguments for the change to the underlying ObservableCollection.</param>
        protected void RefreshSpells(TreeView treSpells, TreeView treMetamagic, ContextMenuStrip cmsSpell, ContextMenuStrip cmsInitiationNotes, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs = null)
        {
            if (treSpells == null)
                return;
            TreeNode objCombatNode = null;
            TreeNode objDetectionNode = null;
            TreeNode objHealthNode = null;
            TreeNode objIllusionNode = null;
            TreeNode objManipulationNode = null;
            TreeNode objRitualsNode = null;
            TreeNode objEnchantmentsNode = null;
            using (CursorWait.New(this))
            {
                if (notifyCollectionChangedEventArgs == null ||
                    notifyCollectionChangedEventArgs.Action == NotifyCollectionChangedAction.Reset)
                {
                    string strSelectedId = (treSpells.SelectedNode?.Tag as IHasInternalId)?.InternalId ?? string.Empty;
                    string strSelectedMetamagicId =
                        (treMetamagic?.SelectedNode?.Tag as IHasInternalId)?.InternalId ?? string.Empty;

                    // Clear the default nodes of entries.
                    treSpells.Nodes.Clear();

                    // Add the Spells that exist.
                    foreach (Spell objSpell in CharacterObject.Spells)
                    {
                        if (objSpell.Grade > 0)
                        {
                            treMetamagic?.FindNodeByTag(objSpell)?.Remove();
                        }

                        AddToTree(objSpell, false);
                    }

                    treSpells.SortCustomAlphabetically(strSelectedId);
                    if (treMetamagic != null)
                        treMetamagic.SelectedNode = treMetamagic.FindNode(strSelectedMetamagicId);
                }
                else
                {
                    objCombatNode = treSpells.FindNode("Node_SelectedCombatSpells", false);
                    objDetectionNode = treSpells.FindNode("Node_SelectedDetectionSpells", false);
                    objHealthNode = treSpells.FindNode("Node_SelectedHealthSpells", false);
                    objIllusionNode = treSpells.FindNode("Node_SelectedIllusionSpells", false);
                    objManipulationNode = treSpells.FindNode("Node_SelectedManipulationSpells", false);
                    objRitualsNode = treSpells.FindNode("Node_SelectedGeomancyRituals", false);
                    objEnchantmentsNode = treSpells.FindNode("Node_SelectedEnchantments", false);
                    switch (notifyCollectionChangedEventArgs.Action)
                    {
                        case NotifyCollectionChangedAction.Add:
                            {
                                foreach (Spell objSpell in notifyCollectionChangedEventArgs.NewItems)
                                {
                                    AddToTree(objSpell);
                                }

                                break;
                            }
                        case NotifyCollectionChangedAction.Remove:
                            {
                                foreach (Spell objSpell in notifyCollectionChangedEventArgs.OldItems)
                                {
                                    TreeNode objNode = treSpells.FindNodeByTag(objSpell);
                                    if (objNode != null)
                                    {
                                        TreeNode objParent = objNode.Parent;
                                        objNode.Remove();
                                        if (objParent.Level == 0 && objParent.Nodes.Count == 0)
                                            objParent.Remove();
                                    }

                                    if (objSpell.Grade > 0)
                                    {
                                        treMetamagic?.FindNode(objSpell.InternalId)?.Remove();
                                    }
                                }

                                break;
                            }
                        case NotifyCollectionChangedAction.Replace:
                            {
                                List<TreeNode> lstOldParents =
                                    new List<TreeNode>(notifyCollectionChangedEventArgs.OldItems.Count);
                                foreach (Spell objSpell in notifyCollectionChangedEventArgs.OldItems)
                                {
                                    TreeNode objNode = treSpells.FindNodeByTag(objSpell);
                                    if (objNode != null)
                                    {
                                        lstOldParents.Add(objNode.Parent);
                                        objNode.Remove();
                                    }

                                    if (objSpell.Grade > 0)
                                    {
                                        treMetamagic?.FindNode(objSpell.InternalId)?.Remove();
                                    }
                                }

                                foreach (Spell objSpell in notifyCollectionChangedEventArgs.NewItems)
                                {
                                    AddToTree(objSpell);
                                }

                                foreach (TreeNode objOldParent in lstOldParents)
                                {
                                    if (objOldParent.Level == 0 && objOldParent.Nodes.Count == 0)
                                        objOldParent.Remove();
                                }

                                break;
                            }
                    }
                }
            }

            void AddToTree(Spell objSpell, bool blnSingleAdd = true)
            {
                TreeNode objNode = objSpell.CreateTreeNode(cmsSpell);
                if (objNode == null)
                    return;
                TreeNode objParentNode = null;
                switch (objSpell.Category)
                {
                    case "Combat":
                        if (objCombatNode == null)
                        {
                            objCombatNode = new TreeNode
                            {
                                Tag = "Node_SelectedCombatSpells",
                                Text = LanguageManager.GetString("Node_SelectedCombatSpells")
                            };
                            treSpells.Nodes.Insert(0, objCombatNode);
                            objCombatNode.Expand();
                        }
                        objParentNode = objCombatNode;
                        break;

                    case "Detection":
                        if (objDetectionNode == null)
                        {
                            objDetectionNode = new TreeNode
                            {
                                Tag = "Node_SelectedDetectionSpells",
                                Text = LanguageManager.GetString("Node_SelectedDetectionSpells")
                            };
                            treSpells.Nodes.Insert(objCombatNode == null ? 0 : 1, objDetectionNode);
                            objDetectionNode.Expand();
                        }
                        objParentNode = objDetectionNode;
                        break;

                    case "Health":
                        if (objHealthNode == null)
                        {
                            objHealthNode = new TreeNode
                            {
                                Tag = "Node_SelectedHealthSpells",
                                Text = LanguageManager.GetString("Node_SelectedHealthSpells")
                            };
                            treSpells.Nodes.Insert((objCombatNode == null ? 0 : 1) +
                                (objDetectionNode == null ? 0 : 1), objHealthNode);
                            objHealthNode.Expand();
                        }
                        objParentNode = objHealthNode;
                        break;

                    case "Illusion":
                        if (objIllusionNode == null)
                        {
                            objIllusionNode = new TreeNode
                            {
                                Tag = "Node_SelectedIllusionSpells",
                                Text = LanguageManager.GetString("Node_SelectedIllusionSpells")
                            };
                            treSpells.Nodes.Insert((objCombatNode == null ? 0 : 1) +
                                (objDetectionNode == null ? 0 : 1) +
                                (objHealthNode == null ? 0 : 1), objIllusionNode);
                            objIllusionNode.Expand();
                        }
                        objParentNode = objIllusionNode;
                        break;

                    case "Manipulation":
                        if (objManipulationNode == null)
                        {
                            objManipulationNode = new TreeNode
                            {
                                Tag = "Node_SelectedManipulationSpells",
                                Text = LanguageManager.GetString("Node_SelectedManipulationSpells")
                            };
                            treSpells.Nodes.Insert((objCombatNode == null ? 0 : 1) +
                                (objDetectionNode == null ? 0 : 1) +
                                (objHealthNode == null ? 0 : 1) +
                                (objIllusionNode == null ? 0 : 1), objManipulationNode);
                            objManipulationNode.Expand();
                        }
                        objParentNode = objManipulationNode;
                        break;

                    case "Rituals":
                        if (objRitualsNode == null)
                        {
                            objRitualsNode = new TreeNode
                            {
                                Tag = "Node_SelectedGeomancyRituals",
                                Text = LanguageManager.GetString("Node_SelectedGeomancyRituals")
                            };
                            treSpells.Nodes.Insert((objCombatNode == null ? 0 : 1) +
                                (objDetectionNode == null ? 0 : 1) +
                                (objHealthNode == null ? 0 : 1) +
                                (objIllusionNode == null ? 0 : 1) +
                                (objManipulationNode == null ? 0 : 1), objRitualsNode);
                            objRitualsNode.Expand();
                        }
                        objParentNode = objRitualsNode;
                        break;

                    case "Enchantments":
                        if (objEnchantmentsNode == null)
                        {
                            objEnchantmentsNode = new TreeNode
                            {
                                Tag = "Node_SelectedEnchantments",
                                Text = LanguageManager.GetString("Node_SelectedEnchantments")
                            };
                            treSpells.Nodes.Add(objEnchantmentsNode);
                            objEnchantmentsNode.Expand();
                        }
                        objParentNode = objEnchantmentsNode;
                        break;
                }
                if (objSpell.Grade > 0)
                {
                    InitiationGrade objGrade = CharacterObject.InitiationGrades.FirstOrDefault(x => x.Grade == objSpell.Grade);
                    if (objGrade != null)
                    {
                        TreeNode nodMetamagicParent = treMetamagic?.FindNodeByTag(objGrade);
                        if (nodMetamagicParent != null)
                        {
                            TreeNodeCollection nodMetamagicParentChildren = nodMetamagicParent.Nodes;
                            TreeNode objMetamagicNode = objSpell.CreateTreeNode(cmsInitiationNotes, true);
                            int intNodesCount = nodMetamagicParentChildren.Count;
                            int intTargetIndex = 0;
                            for (; intTargetIndex < intNodesCount; ++intTargetIndex)
                            {
                                if (CompareTreeNodes.CompareText(nodMetamagicParentChildren[intTargetIndex], objMetamagicNode) >= 0)
                                {
                                    break;
                                }
                            }

                            nodMetamagicParentChildren.Insert(intTargetIndex, objMetamagicNode);
                            if (blnSingleAdd)
                                treMetamagic.SelectedNode = objMetamagicNode;
                        }
                    }
                }

                if (objParentNode == null)
                    return;
                if (blnSingleAdd)
                {
                    TreeNodeCollection lstParentNodeChildren = objParentNode.Nodes;
                    int intNodesCount = lstParentNodeChildren.Count;
                    int intTargetIndex = 0;
                    for (; intTargetIndex < intNodesCount; ++intTargetIndex)
                    {
                        if (CompareTreeNodes.CompareText(lstParentNodeChildren[intTargetIndex], objNode) >= 0)
                        {
                            break;
                        }
                    }

                    lstParentNodeChildren.Insert(intTargetIndex, objNode);
                    treSpells.SelectedNode = objNode;
                }
                else
                    objParentNode.Nodes.Add(objNode);
            }
        }

        protected void RefreshAIPrograms(TreeView treAIPrograms, ContextMenuStrip cmsAdvancedProgram, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs = null)
        {
            if (treAIPrograms == null)
                return;
            TreeNode objParentNode = null;
            using (CursorWait.New(this))
            {
                if (notifyCollectionChangedEventArgs == null ||
                    notifyCollectionChangedEventArgs.Action == NotifyCollectionChangedAction.Reset)
                {
                    string strSelectedId =
                        (treAIPrograms.SelectedNode?.Tag as IHasInternalId)?.InternalId ?? string.Empty;

                    treAIPrograms.Nodes.Clear();

                    // Add AI Programs.
                    foreach (AIProgram objAIProgram in CharacterObject.AIPrograms)
                    {
                        AddToTree(objAIProgram, false);
                    }

                    treAIPrograms.SortCustomAlphabetically(strSelectedId);
                }
                else
                {
                    objParentNode = treAIPrograms.FindNode("Node_SelectedAIPrograms", false);
                    switch (notifyCollectionChangedEventArgs.Action)
                    {
                        case NotifyCollectionChangedAction.Add:
                            {
                                foreach (AIProgram objAIProgram in notifyCollectionChangedEventArgs.NewItems)
                                {
                                    AddToTree(objAIProgram);
                                }

                                break;
                            }
                        case NotifyCollectionChangedAction.Remove:
                            {
                                foreach (AIProgram objAIProgram in notifyCollectionChangedEventArgs.OldItems)
                                {
                                    TreeNode objNode = treAIPrograms.FindNodeByTag(objAIProgram);
                                    if (objNode != null)
                                    {
                                        TreeNode objParent = objNode.Parent;
                                        objNode.Remove();
                                        if (objParent.Level == 0 && objParent.Nodes.Count == 0)
                                            objParent.Remove();
                                    }
                                }

                                break;
                            }
                        case NotifyCollectionChangedAction.Replace:
                            {
                                List<TreeNode> lstOldParents =
                                    new List<TreeNode>(notifyCollectionChangedEventArgs.OldItems.Count);
                                foreach (AIProgram objAIProgram in notifyCollectionChangedEventArgs.OldItems)
                                {
                                    TreeNode objNode = treAIPrograms.FindNodeByTag(objAIProgram);
                                    if (objNode != null)
                                    {
                                        lstOldParents.Add(objNode.Parent);
                                        objNode.Remove();
                                    }
                                }

                                foreach (AIProgram objAIProgram in notifyCollectionChangedEventArgs.NewItems)
                                {
                                    AddToTree(objAIProgram);
                                }

                                foreach (TreeNode objOldParent in lstOldParents)
                                {
                                    if (objOldParent.Level == 0 && objOldParent.Nodes.Count == 0)
                                        objOldParent.Remove();
                                }

                                break;
                            }
                    }
                }
            }

            void AddToTree(AIProgram objAIProgram, bool blnSingleAdd = true)
            {
                TreeNode objNode = objAIProgram.CreateTreeNode(cmsAdvancedProgram);
                if (objNode == null)
                    return;

                if (objParentNode == null)
                {
                    objParentNode = new TreeNode
                    {
                        Tag = "Node_SelectedAIPrograms",
                        Text = LanguageManager.GetString("Node_SelectedAIPrograms")
                    };
                    treAIPrograms.Nodes.Add(objParentNode);
                    objParentNode.Expand();
                }

                if (blnSingleAdd)
                {
                    TreeNodeCollection lstParentNodeChildren = objParentNode.Nodes;
                    int intNodesCount = lstParentNodeChildren.Count;
                    int intTargetIndex = 0;
                    for (; intTargetIndex < intNodesCount; ++intTargetIndex)
                    {
                        if (CompareTreeNodes.CompareText(lstParentNodeChildren[intTargetIndex], objNode) >= 0)
                        {
                            break;
                        }
                    }

                    lstParentNodeChildren.Insert(intTargetIndex, objNode);
                    treAIPrograms.SelectedNode = objNode;
                }
                else
                    objParentNode.Nodes.Add(objNode);
            }
        }

        protected void RefreshComplexForms(TreeView treComplexForms, TreeView treMetamagic, ContextMenuStrip cmsComplexForm, ContextMenuStrip cmsInitiationNotes, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs = null)
        {
            if (treComplexForms == null)
                return;
            TreeNode objParentNode = null;
            using (CursorWait.New(this))
            {
                if (notifyCollectionChangedEventArgs == null ||
                    notifyCollectionChangedEventArgs.Action == NotifyCollectionChangedAction.Reset)
                {
                    string strSelectedId =
                        (treComplexForms.SelectedNode?.Tag as IHasInternalId)?.InternalId ?? string.Empty;
                    string strSelectedMetamagicId =
                        (treMetamagic?.SelectedNode?.Tag as IHasInternalId)?.InternalId ?? string.Empty;

                    treComplexForms.Nodes.Clear();

                    // Add Complex Forms.
                    foreach (ComplexForm objComplexForm in CharacterObject.ComplexForms)
                    {
                        if (objComplexForm.Grade > 0)
                        {
                            treMetamagic?.FindNodeByTag(objComplexForm)?.Remove();
                        }

                        AddToTree(objComplexForm, false);
                    }

                    treComplexForms.SortCustomAlphabetically(strSelectedId);
                    if (treMetamagic != null)
                        treMetamagic.SelectedNode = treMetamagic.FindNode(strSelectedMetamagicId);
                }
                else
                {
                    objParentNode = treComplexForms.FindNode("Node_SelectedAdvancedComplexForms", false);
                    switch (notifyCollectionChangedEventArgs.Action)
                    {
                        case NotifyCollectionChangedAction.Add:
                            {
                                foreach (ComplexForm objComplexForm in notifyCollectionChangedEventArgs.NewItems)
                                {
                                    AddToTree(objComplexForm);
                                }

                                break;
                            }
                        case NotifyCollectionChangedAction.Remove:
                            {
                                foreach (ComplexForm objComplexForm in notifyCollectionChangedEventArgs.OldItems)
                                {
                                    TreeNode objNode = treComplexForms.FindNodeByTag(objComplexForm);
                                    if (objNode != null)
                                    {
                                        TreeNode objParent = objNode.Parent;
                                        objNode.Remove();
                                        if (objParent.Level == 0 && objParent.Nodes.Count == 0)
                                            objParent.Remove();
                                    }

                                    if (objComplexForm.Grade > 0)
                                    {
                                        treMetamagic?.FindNodeByTag(objComplexForm)?.Remove();
                                    }
                                }

                                break;
                            }
                        case NotifyCollectionChangedAction.Replace:
                            {
                                List<TreeNode> lstOldParents =
                                    new List<TreeNode>(notifyCollectionChangedEventArgs.OldItems.Count);
                                foreach (ComplexForm objComplexForm in notifyCollectionChangedEventArgs.OldItems)
                                {
                                    TreeNode objNode = treComplexForms.FindNodeByTag(objComplexForm);
                                    if (objNode != null)
                                    {
                                        lstOldParents.Add(objNode.Parent);
                                        objNode.Remove();
                                    }

                                    if (objComplexForm.Grade > 0)
                                    {
                                        treMetamagic?.FindNodeByTag(objComplexForm)?.Remove();
                                    }
                                }

                                foreach (ComplexForm objComplexForm in notifyCollectionChangedEventArgs.NewItems)
                                {
                                    AddToTree(objComplexForm);
                                }

                                foreach (TreeNode objOldParent in lstOldParents)
                                {
                                    if (objOldParent.Level == 0 && objOldParent.Nodes.Count == 0)
                                        objOldParent.Remove();
                                }

                                break;
                            }
                    }
                }
            }

            void AddToTree(ComplexForm objComplexForm, bool blnSingleAdd = true)
            {
                TreeNode objNode = objComplexForm.CreateTreeNode(cmsComplexForm);
                if (objNode == null)
                    return;
                if (objParentNode == null)
                {
                    objParentNode = new TreeNode
                    {
                        Tag = "Node_SelectedAdvancedComplexForms",
                        Text = LanguageManager.GetString("Node_SelectedAdvancedComplexForms")
                    };
                    treComplexForms.Nodes.Add(objParentNode);
                    objParentNode.Expand();
                }
                if (objComplexForm.Grade > 0)
                {
                    InitiationGrade objGrade = CharacterObject.InitiationGrades.FirstOrDefault(x => x.Grade == objComplexForm.Grade);
                    if (objGrade != null)
                    {
                        TreeNode nodMetamagicParent = treMetamagic?.FindNodeByTag(objGrade);
                        if (nodMetamagicParent != null)
                        {
                            TreeNodeCollection nodMetamagicParentChildren = nodMetamagicParent.Nodes;
                            TreeNode objMetamagicNode = objComplexForm.CreateTreeNode(cmsInitiationNotes);
                            int intNodesCount = nodMetamagicParentChildren.Count;
                            int intTargetIndex = 0;
                            for (; intTargetIndex < intNodesCount; ++intTargetIndex)
                            {
                                if (CompareTreeNodes.CompareText(nodMetamagicParentChildren[intTargetIndex], objMetamagicNode) >= 0)
                                {
                                    break;
                                }
                            }

                            nodMetamagicParentChildren.Insert(intTargetIndex, objMetamagicNode);
                            if (blnSingleAdd)
                                treMetamagic.SelectedNode = objMetamagicNode;
                        }
                    }
                }
                if (blnSingleAdd)
                {
                    TreeNodeCollection lstParentNodeChildren = objParentNode.Nodes;
                    int intNodesCount = lstParentNodeChildren.Count;
                    int intTargetIndex = 0;
                    for (; intTargetIndex < intNodesCount; ++intTargetIndex)
                    {
                        if (CompareTreeNodes.CompareText(lstParentNodeChildren[intTargetIndex], objNode) >= 0)
                        {
                            break;
                        }
                    }

                    lstParentNodeChildren.Insert(intTargetIndex, objNode);
                    treComplexForms.SelectedNode = objNode;
                }
                else
                    objParentNode.Nodes.Add(objNode);
            }
        }

        protected void RefreshInitiationGrades(TreeView treMetamagic, ContextMenuStrip cmsMetamagic, ContextMenuStrip cmsInitiationNotes, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs = null)
        {
            if (treMetamagic == null)
                return;
            using (CursorWait.New(this))
            {
                if (notifyCollectionChangedEventArgs == null ||
                    notifyCollectionChangedEventArgs.Action == NotifyCollectionChangedAction.Reset)
                {
                    string strSelectedId =
                        (treMetamagic.SelectedNode?.Tag as IHasInternalId)?.InternalId ?? string.Empty;
                    TreeNodeCollection lstRootNodes = treMetamagic.Nodes;
                    lstRootNodes.Clear();

                    foreach (InitiationGrade objGrade in CharacterObject.InitiationGrades)
                    {
                        AddToTree(objGrade);
                    }

                    int intOffset = lstRootNodes.Count;
                    foreach (Metamagic objMetamagic in CharacterObject.Metamagics)
                    {
                        if (objMetamagic.Grade < 0)
                        {
                            TreeNode objNode = objMetamagic.CreateTreeNode(cmsInitiationNotes, true);
                            if (objNode != null)
                            {
                                int intNodesCount = lstRootNodes.Count;
                                int intTargetIndex = intOffset;
                                for (; intTargetIndex < intNodesCount; ++intTargetIndex)
                                {
                                    if (CompareTreeNodes.CompareText(lstRootNodes[intTargetIndex], objNode) >= 0)
                                    {
                                        break;
                                    }
                                }

                                lstRootNodes.Insert(intTargetIndex, objNode);
                                objNode.Expand();
                            }
                        }
                    }

                    treMetamagic.SelectedNode = treMetamagic.FindNode(strSelectedId);
                }
                else
                {
                    switch (notifyCollectionChangedEventArgs.Action)
                    {
                        case NotifyCollectionChangedAction.Add:
                            {
                                int intNewIndex = notifyCollectionChangedEventArgs.NewStartingIndex;
                                foreach (InitiationGrade objGrade in notifyCollectionChangedEventArgs.NewItems)
                                {
                                    AddToTree(objGrade, intNewIndex);
                                    ++intNewIndex;
                                }
                            }
                            break;

                        case NotifyCollectionChangedAction.Remove:
                            {
                                foreach (InitiationGrade objGrade in notifyCollectionChangedEventArgs.OldItems)
                                {
                                    treMetamagic.FindNodeByTag(objGrade)?.Remove();
                                }
                            }
                            break;

                        case NotifyCollectionChangedAction.Replace:
                            {
                                foreach (InitiationGrade objGrade in notifyCollectionChangedEventArgs.OldItems)
                                {
                                    treMetamagic.FindNodeByTag(objGrade)?.Remove();
                                }

                                int intNewIndex = notifyCollectionChangedEventArgs.NewStartingIndex;
                                foreach (InitiationGrade objGrade in notifyCollectionChangedEventArgs.NewItems)
                                {
                                    AddToTree(objGrade, intNewIndex);
                                    ++intNewIndex;
                                }
                            }
                            break;

                        case NotifyCollectionChangedAction.Move:
                            {
                                int intNewIndex = notifyCollectionChangedEventArgs.NewStartingIndex;
                                foreach (InitiationGrade objGrade in notifyCollectionChangedEventArgs.OldItems)
                                {
                                    TreeNode nodGrade = treMetamagic.FindNodeByTag(objGrade);
                                    if (nodGrade != null)
                                    {
                                        nodGrade.Remove();
                                        treMetamagic.Nodes.Insert(intNewIndex, nodGrade);
                                        ++intNewIndex;
                                    }
                                }
                            }
                            break;
                    }
                }
            }

            void AddToTree(InitiationGrade objInitiationGrade, int intIndex = -1)
            {
                TreeNode nodGrade = objInitiationGrade.CreateTreeNode(cmsMetamagic);
                TreeNodeCollection lstParentNodeChildren = nodGrade.Nodes;
                foreach (Art objArt in CharacterObject.Arts)
                {
                    if (objArt.Grade == objInitiationGrade.Grade)
                    {
                        TreeNode objNode = objArt.CreateTreeNode(cmsInitiationNotes, true);
                        if (objNode == null)
                            continue;
                        int intNodesCount = lstParentNodeChildren.Count;
                        int intTargetIndex = 0;
                        for (; intTargetIndex < intNodesCount; ++intTargetIndex)
                        {
                            if (CompareTreeNodes.CompareText(lstParentNodeChildren[intTargetIndex], objNode) >= 0)
                            {
                                break;
                            }
                        }
                        lstParentNodeChildren.Insert(intTargetIndex, objNode);
                    }
                }
                foreach (Metamagic objMetamagic in CharacterObject.Metamagics)
                {
                    if (objMetamagic.Grade == objInitiationGrade.Grade)
                    {
                        TreeNode objNode = objMetamagic.CreateTreeNode(cmsInitiationNotes, true);
                        if (objNode == null)
                            continue;
                        int intNodesCount = lstParentNodeChildren.Count;
                        int intTargetIndex = 0;
                        for (; intTargetIndex < intNodesCount; ++intTargetIndex)
                        {
                            if (CompareTreeNodes.CompareText(lstParentNodeChildren[intTargetIndex], objNode) >= 0)
                            {
                                break;
                            }
                        }
                        lstParentNodeChildren.Insert(intTargetIndex, objNode);
                    }
                }
                foreach (Spell objSpell in CharacterObject.Spells)
                {
                    if (objSpell.Grade == objInitiationGrade.Grade)
                    {
                        TreeNode objNode = objSpell.CreateTreeNode(cmsInitiationNotes, true);
                        if (objNode == null)
                            continue;
                        int intNodesCount = lstParentNodeChildren.Count;
                        int intTargetIndex = 0;
                        for (; intTargetIndex < intNodesCount; ++intTargetIndex)
                        {
                            if (CompareTreeNodes.CompareText(lstParentNodeChildren[intTargetIndex], objNode) >= 0)
                            {
                                break;
                            }
                        }
                        lstParentNodeChildren.Insert(intTargetIndex, objNode);
                    }
                }
                foreach (ComplexForm objComplexForm in CharacterObject.ComplexForms)
                {
                    if (objComplexForm.Grade == objInitiationGrade.Grade)
                    {
                        TreeNode objNode = objComplexForm.CreateTreeNode(cmsInitiationNotes);
                        if (objNode == null)
                            continue;
                        int intNodesCount = lstParentNodeChildren.Count;
                        int intTargetIndex = 0;
                        for (; intTargetIndex < intNodesCount; ++intTargetIndex)
                        {
                            if (CompareTreeNodes.CompareText(lstParentNodeChildren[intTargetIndex], objNode) >= 0)
                            {
                                break;
                            }
                        }
                        lstParentNodeChildren.Insert(intTargetIndex, objNode);
                    }
                }
                foreach (Enhancement objEnhancement in CharacterObject.Enhancements)
                {
                    if (objEnhancement.Grade == objInitiationGrade.Grade)
                    {
                        TreeNode objNode = objEnhancement.CreateTreeNode(cmsInitiationNotes, true);
                        if (objNode == null)
                            continue;
                        int intNodesCount = lstParentNodeChildren.Count;
                        int intTargetIndex = 0;
                        for (; intTargetIndex < intNodesCount; ++intTargetIndex)
                        {
                            if (CompareTreeNodes.CompareText(lstParentNodeChildren[intTargetIndex], objNode) >= 0)
                            {
                                break;
                            }
                        }
                        lstParentNodeChildren.Insert(intTargetIndex, objNode);
                    }
                }
                foreach (Power objPower in CharacterObject.Powers)
                {
                    foreach (Enhancement objEnhancement in objPower.Enhancements)
                    {
                        if (objEnhancement.Grade == objInitiationGrade.Grade)
                        {
                            TreeNode objNode = objEnhancement.CreateTreeNode(cmsInitiationNotes, true);
                            if (objNode == null)
                                continue;
                            int intNodesCount = lstParentNodeChildren.Count;
                            int intTargetIndex = 0;
                            for (; intTargetIndex < intNodesCount; ++intTargetIndex)
                            {
                                if (CompareTreeNodes.CompareText(lstParentNodeChildren[intTargetIndex], objNode) >= 0)
                                {
                                    break;
                                }
                            }
                            lstParentNodeChildren.Insert(intTargetIndex, objNode);
                        }
                    }
                }
                nodGrade.Expand();
                if (intIndex < 0)
                    treMetamagic.Nodes.Add(nodGrade);
                else
                    treMetamagic.Nodes.Insert(intIndex, nodGrade);
            }
        }

        protected void RefreshArtCollection(TreeView treMetamagic, ContextMenuStrip cmsMetamagic, ContextMenuStrip cmsInitiationNotes, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            if (treMetamagic == null || notifyCollectionChangedEventArgs == null)
                return;
            using (CursorWait.New(this))
            {
                switch (notifyCollectionChangedEventArgs.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        {
                            foreach (Art objArt in notifyCollectionChangedEventArgs.NewItems)
                            {
                                AddToTree(objArt);
                            }
                        }
                        break;

                    case NotifyCollectionChangedAction.Remove:
                        {
                            foreach (Art objArt in notifyCollectionChangedEventArgs.OldItems)
                            {
                                treMetamagic.FindNodeByTag(objArt)?.Remove();
                            }
                        }
                        break;

                    case NotifyCollectionChangedAction.Replace:
                        {
                            foreach (Art objArt in notifyCollectionChangedEventArgs.OldItems)
                            {
                                treMetamagic.FindNodeByTag(objArt)?.Remove();
                            }

                            foreach (Art objArt in notifyCollectionChangedEventArgs.NewItems)
                            {
                                AddToTree(objArt);
                            }
                        }
                        break;

                    case NotifyCollectionChangedAction.Reset:
                        {
                            RefreshInitiationGrades(treMetamagic, cmsMetamagic, cmsInitiationNotes);
                        }
                        break;
                }
            }

            void AddToTree(Art objArt, bool blnSingleAdd = true)
            {
                InitiationGrade objGrade = CharacterObject.InitiationGrades.FirstOrDefault(x => x.Grade == objArt.Grade);

                if (objGrade != null)
                {
                    TreeNode nodMetamagicParent = treMetamagic.FindNodeByTag(objGrade);
                    if (nodMetamagicParent != null)
                    {
                        TreeNodeCollection nodMetamagicParentChildren = nodMetamagicParent.Nodes;
                        TreeNode objNode = objArt.CreateTreeNode(cmsInitiationNotes, true);
                        if (objNode == null)
                            return;
                        int intNodesCount = nodMetamagicParentChildren.Count;
                        int intTargetIndex = 0;
                        for (; intTargetIndex < intNodesCount; ++intTargetIndex)
                        {
                            if (CompareTreeNodes.CompareText(nodMetamagicParentChildren[intTargetIndex], objNode) >= 0)
                            {
                                break;
                            }
                        }
                        nodMetamagicParentChildren.Insert(intTargetIndex, objNode);
                        nodMetamagicParent.Expand();
                        if (blnSingleAdd)
                            treMetamagic.SelectedNode = objNode;
                    }
                }
            }
        }

        protected void RefreshEnhancementCollection(TreeView treMetamagic, ContextMenuStrip cmsMetamagic, ContextMenuStrip cmsInitiationNotes, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            if (treMetamagic == null || notifyCollectionChangedEventArgs == null)
                return;

            using (CursorWait.New(this))
            {
                switch (notifyCollectionChangedEventArgs.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        {
                            foreach (Enhancement objEnhancement in notifyCollectionChangedEventArgs.NewItems)
                            {
                                AddToTree(objEnhancement);
                            }
                        }
                        break;

                    case NotifyCollectionChangedAction.Remove:
                        {
                            foreach (Enhancement objEnhancement in notifyCollectionChangedEventArgs.OldItems)
                            {
                                treMetamagic.FindNodeByTag(objEnhancement)?.Remove();
                            }
                        }
                        break;

                    case NotifyCollectionChangedAction.Replace:
                        {
                            foreach (Enhancement objEnhancement in notifyCollectionChangedEventArgs.OldItems)
                            {
                                treMetamagic.FindNodeByTag(objEnhancement)?.Remove();
                            }

                            foreach (Enhancement objEnhancement in notifyCollectionChangedEventArgs.NewItems)
                            {
                                AddToTree(objEnhancement);
                            }
                        }
                        break;

                    case NotifyCollectionChangedAction.Reset:
                        {
                            RefreshInitiationGrades(treMetamagic, cmsMetamagic, cmsInitiationNotes);
                        }
                        break;
                }
            }

            void AddToTree(Enhancement objEnhancement, bool blnSingleAdd = true)
            {
                InitiationGrade objGrade = CharacterObject.InitiationGrades.FirstOrDefault(x => x.Grade == objEnhancement.Grade);

                if (objGrade != null)
                {
                    TreeNode nodMetamagicParent = treMetamagic.FindNodeByTag(objGrade);
                    if (nodMetamagicParent != null)
                    {
                        TreeNodeCollection nodMetamagicParentChildren = nodMetamagicParent.Nodes;
                        TreeNode objNode = objEnhancement.CreateTreeNode(cmsInitiationNotes, true);
                        if (objNode == null)
                            return;
                        int intNodesCount = nodMetamagicParentChildren.Count;
                        int intTargetIndex = 0;
                        for (; intTargetIndex < intNodesCount; ++intTargetIndex)
                        {
                            if (CompareTreeNodes.CompareText(nodMetamagicParentChildren[intTargetIndex], objNode) >= 0)
                            {
                                break;
                            }
                        }
                        nodMetamagicParentChildren.Insert(intTargetIndex, objNode);
                        nodMetamagicParent.Expand();
                        if (blnSingleAdd)
                            treMetamagic.SelectedNode = objNode;
                    }
                }
            }
        }

        protected void RefreshPowerCollectionListChanged(TreeView treMetamagic, ContextMenuStrip cmsMetamagic, ContextMenuStrip cmsInitiationNotes, ListChangedEventArgs e = null)
        {
            using (CursorWait.New(this))
            {
                switch (e?.ListChangedType)
                {
                    case ListChangedType.ItemAdded:
                        {
                            CharacterObject.Powers[e.NewIndex].Enhancements.AddTaggedCollectionChanged(treMetamagic,
                                MakeDirtyWithCharacterUpdate);
                            CharacterObject.Powers[e.NewIndex].Enhancements.AddTaggedCollectionChanged(treMetamagic,
                                (x, y) => RefreshEnhancementCollection(treMetamagic, cmsMetamagic, cmsInitiationNotes, y));
                        }
                        break;

                    case ListChangedType.Reset:
                        {
                            RefreshInitiationGrades(treMetamagic, cmsMetamagic, cmsInitiationNotes);
                        }
                        break;

                    case ListChangedType.ItemDeleted:
                    case ListChangedType.ItemChanged:
                        break;
                    case ListChangedType.ItemMoved:
                    case ListChangedType.PropertyDescriptorAdded:
                    case ListChangedType.PropertyDescriptorDeleted:
                    case ListChangedType.PropertyDescriptorChanged:
                        return;
                    case null:
                        {
                            foreach (Power objPower in CharacterObject.Powers)
                            {
                                objPower.Enhancements.AddTaggedCollectionChanged(treMetamagic,
                                    MakeDirtyWithCharacterUpdate);
                                objPower.Enhancements.AddTaggedCollectionChanged(treMetamagic,
                                    (x, y) => RefreshEnhancementCollection(treMetamagic, cmsMetamagic, cmsInitiationNotes,
                                        y));
                            }
                        }
                        break;
                }
            }

            MakeDirtyWithCharacterUpdate(this, EventArgs.Empty);
        }

        protected void RefreshPowerCollectionBeforeRemove(TreeView treMetamagic, RemovingOldEventArgs removingOldEventArgs)
        {
            using (CursorWait.New(this))
            {
                if (removingOldEventArgs?.OldObject is Power objPower)
                {
                    objPower.Enhancements.RemoveTaggedCollectionChanged(treMetamagic);
                }
            }
        }

        protected void RefreshMetamagicCollection(TreeView treMetamagic, ContextMenuStrip cmsMetamagic, ContextMenuStrip cmsInitiationNotes, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            if (treMetamagic == null || notifyCollectionChangedEventArgs == null)
                return;
            using (CursorWait.New(this))
            {
                switch (notifyCollectionChangedEventArgs.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        {
                            foreach (Metamagic objMetamagic in notifyCollectionChangedEventArgs.NewItems)
                            {
                                AddToTree(objMetamagic);
                            }
                        }
                        break;

                    case NotifyCollectionChangedAction.Remove:
                        {
                            foreach (Metamagic objMetamagic in notifyCollectionChangedEventArgs.OldItems)
                            {
                                treMetamagic.FindNodeByTag(objMetamagic)?.Remove();
                            }
                        }
                        break;

                    case NotifyCollectionChangedAction.Replace:
                        {
                            foreach (Metamagic objMetamagic in notifyCollectionChangedEventArgs.OldItems)
                            {
                                treMetamagic.FindNodeByTag(objMetamagic)?.Remove();
                            }

                            foreach (Metamagic objMetamagic in notifyCollectionChangedEventArgs.NewItems)
                            {
                                AddToTree(objMetamagic);
                            }
                        }
                        break;

                    case NotifyCollectionChangedAction.Reset:
                        {
                            RefreshInitiationGrades(treMetamagic, cmsMetamagic, cmsInitiationNotes);
                        }
                        break;
                }
            }

            void AddToTree(Metamagic objMetamagic, bool blnSingleAdd = true)
            {
                if (objMetamagic.Grade < 0)
                {
                    TreeNodeCollection nodMetamagicParentChildren = treMetamagic.Nodes;
                    TreeNode objNode = objMetamagic.CreateTreeNode(cmsInitiationNotes, true);
                    if (objNode == null)
                        return;
                    int intNodesCount = nodMetamagicParentChildren.Count;
                    int intTargetIndex = CharacterObject.InitiationGrades.Count;
                    for (; intTargetIndex < intNodesCount; ++intTargetIndex)
                    {
                        if (CompareTreeNodes.CompareText(nodMetamagicParentChildren[intTargetIndex], objNode) >= 0)
                        {
                            break;
                        }
                    }
                    nodMetamagicParentChildren.Insert(intTargetIndex, objNode);
                    objNode.Expand();
                    if (blnSingleAdd)
                        treMetamagic.SelectedNode = objNode;
                }
                else
                {
                    InitiationGrade objGrade = CharacterObject.InitiationGrades.FirstOrDefault(x => x.Grade == objMetamagic.Grade);

                    if (objGrade != null)
                    {
                        TreeNode nodMetamagicParent = treMetamagic.FindNodeByTag(objGrade);
                        if (nodMetamagicParent != null)
                        {
                            TreeNodeCollection nodMetamagicParentChildren = nodMetamagicParent.Nodes;
                            TreeNode objNode = objMetamagic.CreateTreeNode(cmsInitiationNotes, true);
                            if (objNode == null)
                                return;
                            int intNodesCount = nodMetamagicParentChildren.Count;
                            int intTargetIndex = 0;
                            for (; intTargetIndex < intNodesCount; ++intTargetIndex)
                            {
                                if (CompareTreeNodes.CompareText(nodMetamagicParentChildren[intTargetIndex], objNode) >= 0)
                                {
                                    break;
                                }
                            }
                            nodMetamagicParentChildren.Insert(intTargetIndex, objNode);
                            objNode.Expand();
                            if (blnSingleAdd)
                                treMetamagic.SelectedNode = objNode;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Clears and updates the TreeView for Critter Powers. Typically called as part of AddQuality or UpdateCharacterInfo.
        /// </summary>
        /// <param name="treCritterPowers">TreeNode that will be cleared and populated.</param>
        /// <param name="cmsCritterPowers">ContextMenuStrip that will be added to each power.</param>
        /// <param name="notifyCollectionChangedEventArgs">Arguments for the change to the underlying ObservableCollection.</param>
        protected void RefreshCritterPowers(TreeView treCritterPowers, ContextMenuStrip cmsCritterPowers, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs = null)
        {
            if (treCritterPowers == null)
                return;
            TreeNode objPowersNode = null;
            TreeNode objWeaknessesNode = null;
            using (CursorWait.New(this))
            {
                if (notifyCollectionChangedEventArgs == null || notifyCollectionChangedEventArgs.Action == NotifyCollectionChangedAction.Reset)
                {
                    string strSelectedId = (treCritterPowers.SelectedNode?.Tag as IHasInternalId)?.InternalId ??
                                           string.Empty;
                    treCritterPowers.Nodes.Clear();
                    // Add the Critter Powers that exist.
                    foreach (CritterPower objPower in CharacterObject.CritterPowers)
                    {
                        AddToTree(objPower, false);
                    }

                    treCritterPowers.SortCustomAlphabetically(strSelectedId);
                }
                else
                {
                    objPowersNode = treCritterPowers.FindNode("Node_CritterPowers", false);
                    objWeaknessesNode = treCritterPowers.FindNode("Node_CritterWeaknesses", false);
                    switch (notifyCollectionChangedEventArgs.Action)
                    {
                        case NotifyCollectionChangedAction.Add:
                            {
                                foreach (CritterPower objPower in notifyCollectionChangedEventArgs.NewItems)
                                {
                                    AddToTree(objPower);
                                }

                                break;
                            }
                        case NotifyCollectionChangedAction.Remove:
                            {
                                foreach (CritterPower objPower in notifyCollectionChangedEventArgs.OldItems)
                                {
                                    TreeNode objNode = treCritterPowers.FindNodeByTag(objPower);
                                    if (objNode != null)
                                    {
                                        TreeNode objParent = objNode.Parent;
                                        objNode.Remove();
                                        if (objParent.Level == 0 && objParent.Nodes.Count == 0)
                                            objParent.Remove();
                                    }
                                }

                                break;
                            }
                        case NotifyCollectionChangedAction.Replace:
                            {
                                List<TreeNode> lstOldParents =
                                    new List<TreeNode>(notifyCollectionChangedEventArgs.OldItems.Count);
                                foreach (CritterPower objPower in notifyCollectionChangedEventArgs.OldItems)
                                {
                                    TreeNode objNode = treCritterPowers.FindNode(objPower.InternalId);
                                    if (objNode != null)
                                    {
                                        lstOldParents.Add(objNode.Parent);
                                        objNode.Remove();
                                    }
                                }

                                foreach (CritterPower objPower in notifyCollectionChangedEventArgs.NewItems)
                                {
                                    AddToTree(objPower);
                                }

                                foreach (TreeNode objOldParent in lstOldParents)
                                {
                                    if (objOldParent.Level == 0 && objOldParent.Nodes.Count == 0)
                                        objOldParent.Remove();
                                }

                                break;
                            }
                    }
                }
            }

            void AddToTree(CritterPower objPower, bool blnSingleAdd = true)
            {
                TreeNode objNode = objPower.CreateTreeNode(cmsCritterPowers);
                if (objNode == null)
                    return;
                TreeNode objParentNode;
                switch (objPower.Category)
                {
                    case "Weakness":
                        if (objWeaknessesNode == null)
                        {
                            objWeaknessesNode = new TreeNode
                            {
                                Tag = "Node_CritterWeaknesses",
                                Text = LanguageManager.GetString("Node_CritterWeaknesses")
                            };
                            treCritterPowers.Nodes.Add(objWeaknessesNode);
                            objWeaknessesNode.Expand();
                        }
                        objParentNode = objWeaknessesNode;
                        break;

                    default:
                        if (objPowersNode == null)
                        {
                            objPowersNode = new TreeNode
                            {
                                Tag = "Node_CritterPowers",
                                Text = LanguageManager.GetString("Node_CritterPowers")
                            };
                            treCritterPowers.Nodes.Insert(0, objPowersNode);
                            objPowersNode.Expand();
                        }
                        objParentNode = objPowersNode;
                        break;
                }
                if (blnSingleAdd)
                {
                    TreeNodeCollection lstParentNodeChildren = objParentNode.Nodes;
                    int intNodesCount = lstParentNodeChildren.Count;
                    int intTargetIndex = 0;
                    for (; intTargetIndex < intNodesCount; ++intTargetIndex)
                    {
                        if (CompareTreeNodes.CompareText(lstParentNodeChildren[intTargetIndex], objNode) >= 0)
                        {
                            break;
                        }
                    }
                    lstParentNodeChildren.Insert(intTargetIndex, objNode);
                    treCritterPowers.SelectedNode = objNode;
                }
                else
                    objParentNode.Nodes.Add(objNode);
            }
        }

        /// <summary>
        /// Refreshes the list of qualities into the selected TreeNode. If the same number of
        /// </summary>
        /// <param name="treQualities">TreeView to insert the qualities into.</param>
        /// <param name="cmsQuality">ContextMenuStrip to add to each Quality node.</param>
        /// <param name="notifyCollectionChangedEventArgs">Arguments for the change to the underlying ObservableCollection.</param>
        protected void RefreshQualities(TreeView treQualities, ContextMenuStrip cmsQuality, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs = null)
        {
            if (treQualities == null)
                return;
            TreeNode objPositiveQualityRoot = null;
            TreeNode objNegativeQualityRoot = null;
            TreeNode objLifeModuleRoot = null;

            using (CursorWait.New(this))
            {
                if (notifyCollectionChangedEventArgs == null ||
                    notifyCollectionChangedEventArgs.Action == NotifyCollectionChangedAction.Reset)
                {
                    string strSelectedNode =
                        (treQualities.SelectedNode?.Tag as IHasInternalId)?.InternalId ?? string.Empty;

                    // Create the root nodes.
                    foreach (Quality objQuality in CharacterObject.Qualities)
                        objQuality.PropertyChanged -= AddedQualityOnPropertyChanged;
                    treQualities.Nodes.Clear();

                    // Multiple instances of the same quality are combined into just one entry with a number next to it (e.g. 6 discrete entries of "Focused Concentration" become "Focused Concentration 6")
                    using (new FetchSafelyFromPool<HashSet<string>>(Utils.StringHashSetPool,
                                                                    out HashSet<string> setQualitiesToPrint))
                    {
                        foreach (Quality objQuality in CharacterObject.Qualities)
                        {
                            setQualitiesToPrint.Add(objQuality.SourceIDString + '|' +
                                                    objQuality.GetSourceName(GlobalSettings.Language) + '|' +
                                                    objQuality.Extra);
                        }

                        // Add Qualities
                        foreach (Quality objQuality in CharacterObject.Qualities)
                        {
                            if (!setQualitiesToPrint.Remove(objQuality.SourceIDString + '|' +
                                                            objQuality.GetSourceName(GlobalSettings.Language) + '|' +
                                                            objQuality.Extra))
                                continue;

                            AddToTree(objQuality, false);
                        }
                    }

                    treQualities.SortCustomAlphabetically(strSelectedNode);
                }
                else
                {
                    objPositiveQualityRoot = treQualities.FindNodeByTag("Node_SelectedPositiveQualities", false);
                    objNegativeQualityRoot = treQualities.FindNodeByTag("Node_SelectedNegativeQualities", false);
                    objLifeModuleRoot = treQualities.FindNodeByTag("String_LifeModules", false);
                    bool blnDoNameRefresh = false;
                    switch (notifyCollectionChangedEventArgs.Action)
                    {
                        case NotifyCollectionChangedAction.Add:
                            {
                                foreach (Quality objQuality in notifyCollectionChangedEventArgs.NewItems)
                                {
                                    if (objQuality.Levels > 1)
                                        blnDoNameRefresh = true;
                                    else
                                        AddToTree(objQuality);
                                }

                                break;
                            }
                        case NotifyCollectionChangedAction.Remove:
                            {
                                foreach (Quality objQuality in notifyCollectionChangedEventArgs.OldItems)
                                {
                                    if (objQuality.Levels > 0)
                                        blnDoNameRefresh = true;
                                    else
                                    {
                                        TreeNode objNode = treQualities.FindNodeByTag(objQuality);
                                        if (objNode != null)
                                        {
                                            TreeNode objParent = objNode.Parent;
                                            objNode.Remove();
                                            objQuality.PropertyChanged -= AddedQualityOnPropertyChanged;
                                            if (objParent.Level == 0 && objParent.Nodes.Count == 0)
                                                objParent.Remove();
                                        }
                                    }
                                }

                                break;
                            }
                        case NotifyCollectionChangedAction.Replace:
                            {
                                List<TreeNode> lstOldParents =
                                    new List<TreeNode>(notifyCollectionChangedEventArgs.OldItems.Count);
                                foreach (Quality objQuality in notifyCollectionChangedEventArgs.OldItems)
                                {
                                    if (objQuality.Levels > 0)
                                        blnDoNameRefresh = true;
                                    else
                                    {
                                        TreeNode objNode = treQualities.FindNodeByTag(objQuality);
                                        if (objNode != null)
                                        {
                                            if (objNode.Parent != null)
                                                lstOldParents.Add(objNode.Parent);
                                            objNode.Remove();
                                            objQuality.PropertyChanged -= AddedQualityOnPropertyChanged;
                                        }
                                        else
                                        {
                                            RefreshQualityNames(treQualities);
                                        }
                                    }
                                }

                                foreach (Quality objQuality in notifyCollectionChangedEventArgs.NewItems)
                                {
                                    if (objQuality.Levels > 1)
                                        blnDoNameRefresh = true;
                                    else
                                        AddToTree(objQuality);
                                }

                                foreach (TreeNode objOldParent in lstOldParents)
                                {
                                    if (objOldParent.Level == 0 && objOldParent.Nodes.Count == 0)
                                        objOldParent.Remove();
                                }

                                break;
                            }
                    }

                    if (blnDoNameRefresh)
                        RefreshQualityNames(treQualities);
                }
            }

            void AddToTree(Quality objQuality, bool blnSingleAdd = true)
            {
                TreeNode objNode = objQuality.CreateTreeNode(cmsQuality, treQualities);
                if (objNode == null)
                    return;
                TreeNode objParentNode = null;
                switch (objQuality.Type)
                {
                    case QualityType.Positive:
                        if (objPositiveQualityRoot == null)
                        {
                            objPositiveQualityRoot = new TreeNode
                            {
                                Tag = "Node_SelectedPositiveQualities",
                                Text = LanguageManager.GetString("Node_SelectedPositiveQualities")
                            };
                            treQualities.Nodes.Insert(0, objPositiveQualityRoot);
                            objPositiveQualityRoot.Expand();
                        }
                        objParentNode = objPositiveQualityRoot;
                        break;

                    case QualityType.Negative:
                        if (objNegativeQualityRoot == null)
                        {
                            objNegativeQualityRoot = new TreeNode
                            {
                                Tag = "Node_SelectedNegativeQualities",
                                Text = LanguageManager.GetString("Node_SelectedNegativeQualities")
                            };
                            treQualities.Nodes.Insert(objLifeModuleRoot != null && objPositiveQualityRoot == null ? 0 : 1, objNegativeQualityRoot);
                            objNegativeQualityRoot.Expand();
                        }
                        objParentNode = objNegativeQualityRoot;
                        break;

                    case QualityType.LifeModule:
                        if (objLifeModuleRoot == null)
                        {
                            objLifeModuleRoot = new TreeNode
                            {
                                Tag = "String_LifeModules",
                                Text = LanguageManager.GetString("String_LifeModules")
                            };
                            treQualities.Nodes.Add(objLifeModuleRoot);
                            objLifeModuleRoot.Expand();
                        }
                        objParentNode = objLifeModuleRoot;
                        break;
                }

                if (objParentNode != null)
                {
                    if (blnSingleAdd)
                    {
                        TreeNodeCollection lstParentNodeChildren = objParentNode.Nodes;
                        int intNodesCount = lstParentNodeChildren.Count;
                        int intTargetIndex = 0;
                        for (; intTargetIndex < intNodesCount; ++intTargetIndex)
                        {
                            if (CompareTreeNodes.CompareText(lstParentNodeChildren[intTargetIndex], objNode) >= 0)
                            {
                                break;
                            }
                        }

                        lstParentNodeChildren.Insert(intTargetIndex, objNode);
                        treQualities.SelectedNode = objNode;
                    }
                    else
                        objParentNode.Nodes.Add(objNode);
                    objQuality.PropertyChanged += AddedQualityOnPropertyChanged;
                }
            }

            void AddedQualityOnPropertyChanged(object sender, PropertyChangedEventArgs e)
            {
                switch (e.PropertyName)
                {
                    case nameof(Quality.Suppressed):
                        {
                            if (!(sender is Quality objQuality))
                                return;
                            TreeNode objNode = treQualities.FindNodeByTag(objQuality);
                            if (objNode == null)
                                return;
                            Font objOldFont = objNode.NodeFont;
                            //Treenodes store their font as null when inheriting from the treeview; have to pull it from the treeview directly to set the fontstyle.
                            objNode.NodeFont = new Font(treQualities.Font,
                                objQuality.Suppressed ? FontStyle.Strikeout : FontStyle.Regular);
                            // Dispose the old font if it's not null so that we don't leak memory
                            objOldFont?.Dispose();
                            break;
                        }
                    case nameof(Quality.Notes):
                        {
                            if (!(sender is Quality objQuality))
                                return;
                            TreeNode objNode = treQualities.FindNodeByTag(objQuality);
                            if (objNode == null)
                                return;
                            objNode.ToolTipText = objQuality.Notes.WordWrap();
                            break;
                        }
                }
            }
        }

        /// <summary>
        /// Refreshes all the names of qualities in the nodes
        /// </summary>
        /// <param name="treQualities">TreeView to insert the qualities into.</param>
        protected void RefreshQualityNames(TreeView treQualities)
        {
            if (treQualities == null || treQualities.GetNodeCount(false) <= 0)
                return;
            using (CursorWait.New(this))
            {
                TreeNode objSelectedNode = treQualities.SelectedNode;
                foreach (TreeNode objQualityTypeNode in treQualities.Nodes)
                {
                    foreach (TreeNode objQualityNode in objQualityTypeNode.Nodes)
                    {
                        objQualityNode.Text = ((Quality)objQualityNode.Tag).CurrentDisplayName;
                    }
                }

                treQualities.SortCustomAlphabetically(objSelectedNode?.Tag);
            }
        }

        #endregion Refresh Treeviews and Panels

        /// <summary>
        /// Method for removing old <addqualities /> nodes from existing characters.
        /// </summary>
        /// <param name="objNodeList">XmlNode to load. Expected to be addqualities/addquality</param>
        protected void RemoveAddedQualities(XPathNodeIterator objNodeList)
        {
            if (objNodeList == null || objNodeList.Count <= 0)
                return;
            using (CursorWait.New(this))
            {
                foreach (XPathNavigator objNode in objNodeList)
                {
                    Quality objQuality = CharacterObject.Qualities.FirstOrDefault(x => x.Name == objNode.Value);
                    if (objQuality != null)
                    {
                        objQuality.DeleteQuality();
                        ImprovementManager.RemoveImprovements(CharacterObject, Improvement.ImprovementSource.CritterPower,
                                                              objQuality.InternalId);
                    }
                }
            }
        }

        #region Locations

        protected void RefreshArmorLocations(TreeView treArmor, ContextMenuStrip cmsArmorLocation, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            if (treArmor == null || notifyCollectionChangedEventArgs == null)
                return;

            using (CursorWait.New(this))
            {
                string strSelectedId = (treArmor.SelectedNode?.Tag as IHasInternalId)?.InternalId ?? string.Empty;

                TreeNode nodRoot = treArmor.FindNode("Node_SelectedImprovements", false);
                RefreshLocation(treArmor, nodRoot, cmsArmorLocation, notifyCollectionChangedEventArgs, strSelectedId,
                    "Node_SelectedArmor");
            }
        }

        protected void RefreshGearLocations(TreeView treGear, ContextMenuStrip cmsGearLocation, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            if (treGear == null || notifyCollectionChangedEventArgs == null)
                return;

            using (CursorWait.New(this))
            {
                string strSelectedId = (treGear.SelectedNode?.Tag as IHasInternalId)?.InternalId ?? string.Empty;

                TreeNode nodRoot = treGear.FindNode("Node_SelectedGear", false);
                RefreshLocation(treGear, nodRoot, cmsGearLocation, notifyCollectionChangedEventArgs, strSelectedId,
                    "Node_SelectedGear");
            }
        }

        protected void RefreshVehicleLocations(TreeView treVehicles, ContextMenuStrip cmsVehicleLocation, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            if (treVehicles == null || notifyCollectionChangedEventArgs == null)
                return;

            TreeNode nodRoot = treVehicles.FindNode("Node_SelectedVehicles", false);
            string strSelectedId = (treVehicles.SelectedNode?.Tag as IHasInternalId)?.InternalId ?? string.Empty;
            RefreshLocation(treVehicles, nodRoot, cmsVehicleLocation, notifyCollectionChangedEventArgs, strSelectedId,
                "Node_SelectedVehicles");
        }

        protected void RefreshLocationsInVehicle(TreeView treVehicles, Vehicle objVehicle, ContextMenuStrip cmsVehicleLocation, Func<int> funcOffset, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            if (treVehicles == null || objVehicle == null || notifyCollectionChangedEventArgs == null)
                return;

            using (CursorWait.New(this))
            {
                string strSelectedId = (treVehicles.SelectedNode?.Tag as IHasInternalId)?.InternalId ?? string.Empty;

                TreeNode nodRoot = treVehicles.FindNodeByTag(objVehicle);
                RefreshLocation(treVehicles, nodRoot, cmsVehicleLocation, funcOffset, notifyCollectionChangedEventArgs,
                    strSelectedId, "Node_SelectedVehicles", false);
            }
        }

        protected void RefreshWeaponLocations(TreeView treWeapons, ContextMenuStrip cmsWeaponLocation, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            if (treWeapons == null || notifyCollectionChangedEventArgs == null)
                return;

            using (CursorWait.New(this))
            {
                string strSelectedId = (treWeapons.SelectedNode?.Tag as IHasInternalId)?.InternalId ?? string.Empty;

                TreeNode nodRoot = treWeapons.FindNode("Node_SelectedWeapons", false);
                RefreshLocation(treWeapons, nodRoot, cmsWeaponLocation, notifyCollectionChangedEventArgs, strSelectedId,
                    "Node_SelectedWeapons");
            }
        }

        protected void RefreshCustomImprovementLocations(TreeView treImprovements, ContextMenuStrip cmsImprovementLocation, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            if (treImprovements == null || notifyCollectionChangedEventArgs == null)
                return;

            using (CursorWait.New(this))
            {
                string strSelectedId =
                    (treImprovements.SelectedNode?.Tag as IHasInternalId)?.InternalId ?? string.Empty;

                TreeNode nodRoot = treImprovements.FindNode("Node_SelectedImprovements", false);

                switch (notifyCollectionChangedEventArgs.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        {
                            int intNewIndex = notifyCollectionChangedEventArgs.NewStartingIndex;
                            foreach (string strLocation in notifyCollectionChangedEventArgs.NewItems)
                            {
                                TreeNode objLocation = new TreeNode
                                {
                                    Tag = strLocation,
                                    Text = strLocation,
                                    ContextMenuStrip = cmsImprovementLocation
                                };
                                treImprovements.Nodes.Insert(intNewIndex, objLocation);
                                ++intNewIndex;
                            }
                        }
                        break;

                    case NotifyCollectionChangedAction.Remove:
                        {
                            foreach (string strLocation in notifyCollectionChangedEventArgs.OldItems)
                            {
                                TreeNode objNode = treImprovements.FindNodeByTag(strLocation, false);
                                if (objNode != null)
                                {
                                    objNode.Remove();
                                    if (objNode.Nodes.Count > 0)
                                    {
                                        if (nodRoot == null)
                                        {
                                            nodRoot = new TreeNode
                                            {
                                                Tag = "Node_SelectedImprovements",
                                                Text = LanguageManager.GetString("Node_SelectedImprovements")
                                            };
                                            treImprovements.Nodes.Insert(0, nodRoot);
                                        }

                                        for (int i = objNode.Nodes.Count - 1; i >= 0; --i)
                                        {
                                            TreeNode nodImprovement = objNode.Nodes[i];
                                            nodImprovement.Remove();
                                            nodRoot.Nodes.Add(nodImprovement);
                                        }
                                    }
                                }
                            }
                        }
                        break;

                    case NotifyCollectionChangedAction.Replace:
                        {
                            int intNewItemsIndex = 0;
                            foreach (string strLocation in notifyCollectionChangedEventArgs.OldItems)
                            {
                                TreeNode objNode = treImprovements.FindNodeByTag(strLocation, false);
                                if (objNode != null)
                                {
                                    if (notifyCollectionChangedEventArgs
                                        .NewItems[intNewItemsIndex] is string objNewLocation)
                                    {
                                        objNode.Tag = objNewLocation;
                                        objNode.Text = objNewLocation;
                                    }

                                    ++intNewItemsIndex;
                                }
                            }
                        }
                        break;

                    case NotifyCollectionChangedAction.Move:
                        {
                            List<Tuple<string, TreeNode>> lstMoveNodes =
                                new List<Tuple<string, TreeNode>>(notifyCollectionChangedEventArgs.OldItems.Count);
                            foreach (string strLocation in notifyCollectionChangedEventArgs.OldItems)
                            {
                                TreeNode objLocation = treImprovements.FindNode(strLocation, false);
                                if (objLocation != null)
                                {
                                    lstMoveNodes.Add(new Tuple<string, TreeNode>(strLocation, objLocation));
                                    objLocation.Remove();
                                }
                            }

                            int intNewIndex = notifyCollectionChangedEventArgs.NewStartingIndex;
                            foreach (string strLocation in notifyCollectionChangedEventArgs.NewItems)
                            {
                                Tuple<string, TreeNode> objLocationTuple =
                                    lstMoveNodes.Find(x => x.Item1 == strLocation);
                                if (objLocationTuple != null)
                                {
                                    treImprovements.Nodes.Insert(intNewIndex, objLocationTuple.Item2);
                                    ++intNewIndex;
                                    lstMoveNodes.Remove(objLocationTuple);
                                }
                            }
                        }
                        break;

                    case NotifyCollectionChangedAction.Reset:
                        {
                            foreach (string strLocation in CharacterObject.ImprovementGroups)
                            {
                                TreeNode objLocation = treImprovements.FindNode(strLocation, false);
                                if (objLocation != null)
                                {
                                    objLocation.Remove();
                                    if (objLocation.Nodes.Count > 0)
                                    {
                                        if (nodRoot == null)
                                        {
                                            nodRoot = new TreeNode
                                            {
                                                Tag = "Node_SelectedImprovements",
                                                Text = LanguageManager.GetString("Node_SelectedImprovements")
                                            };
                                            treImprovements.Nodes.Insert(0, nodRoot);
                                        }

                                        for (int i = objLocation.Nodes.Count - 1; i >= 0; --i)
                                        {
                                            TreeNode nodImprovement = objLocation.Nodes[i];
                                            nodImprovement.Remove();
                                            nodRoot.Nodes.Add(nodImprovement);
                                        }
                                    }
                                }
                            }
                        }
                        break;
                }

                treImprovements.SelectedNode = treImprovements.FindNode(strSelectedId);
            }
        }

        private void RefreshLocation(TreeView treSelected, TreeNode nodRoot, ContextMenuStrip cmsLocation,
            NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs, string strSelectedId, string strNodeName)
        {
            RefreshLocation(treSelected, nodRoot, cmsLocation, null, notifyCollectionChangedEventArgs, strSelectedId, strNodeName);
        }

        private void RefreshLocation(TreeView treSelected, TreeNode nodRoot, ContextMenuStrip cmsLocation,
            Func<int> funcOffset,
            NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs, string strSelectedId, string strNodeName,
            bool rootSibling = true)
        {
            using (CursorWait.New(this))
            {
                switch (notifyCollectionChangedEventArgs.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        {
                            int intNewIndex = notifyCollectionChangedEventArgs.NewStartingIndex;
                            if (funcOffset != null)
                                intNewIndex += funcOffset.Invoke();
                            foreach (Location objLocation in notifyCollectionChangedEventArgs.NewItems)
                            {
                                if (rootSibling)
                                {
                                    treSelected.Nodes.Insert(intNewIndex, objLocation.CreateTreeNode(cmsLocation));
                                }
                                else
                                {
                                    nodRoot.Nodes.Insert(intNewIndex, objLocation.CreateTreeNode(cmsLocation));
                                }

                                ++intNewIndex;
                            }
                        }
                        break;

                    case NotifyCollectionChangedAction.Remove:
                        {
                            foreach (Location objLocation in notifyCollectionChangedEventArgs.OldItems)
                            {
                                TreeNode nodLocation = treSelected.FindNodeByTag(objLocation, false);
                                if (nodLocation == null)
                                    continue;
                                if (nodLocation.Nodes.Count > 0)
                                {
                                    if (nodRoot == null)
                                    {
                                        nodRoot = new TreeNode
                                        {
                                            Tag = strNodeName,
                                            Text = LanguageManager.GetString(strNodeName)
                                        };
                                        treSelected.Nodes.Insert(0, nodRoot);
                                    }

                                    for (int i = nodLocation.Nodes.Count - 1; i >= 0; --i)
                                    {
                                        TreeNode nodWeapon = nodLocation.Nodes[i];
                                        nodWeapon.Remove();
                                        nodRoot.Nodes.Add(nodWeapon);
                                    }
                                }

                                nodLocation.Remove();
                            }
                        }
                        break;

                    case NotifyCollectionChangedAction.Replace:
                        {
                            int intNewItemsIndex = 0;
                            foreach (Location objLocation in notifyCollectionChangedEventArgs.OldItems)
                            {
                                TreeNode objNode = treSelected.FindNodeByTag(objLocation, false);
                                if (objNode != null)
                                {
                                    if (notifyCollectionChangedEventArgs.NewItems[intNewItemsIndex] is Location
                                        objNewLocation)
                                    {
                                        objNode.Tag = objNewLocation;
                                        objNode.Text = objNewLocation.DisplayName();
                                    }

                                    ++intNewItemsIndex;
                                }
                            }
                        }
                        break;

                    case NotifyCollectionChangedAction.Move:
                        {
                            List<Tuple<Location, TreeNode>> lstMoveNodes =
                                new List<Tuple<Location, TreeNode>>(notifyCollectionChangedEventArgs.OldItems.Count);
                            foreach (Location objLocation in notifyCollectionChangedEventArgs.OldItems)
                            {
                                TreeNode objNode = treSelected.FindNodeByTag(objLocation, false);
                                if (objNode != null)
                                {
                                    lstMoveNodes.Add(new Tuple<Location, TreeNode>(objLocation, objNode));
                                    objLocation.Remove(false);
                                }
                            }

                            int intNewIndex = notifyCollectionChangedEventArgs.NewStartingIndex;
                            foreach (Location objLocation in notifyCollectionChangedEventArgs.NewItems)
                            {
                                Tuple<Location, TreeNode> objLocationTuple =
                                    lstMoveNodes.Find(x => x.Item1 == objLocation);
                                if (objLocationTuple != null)
                                {
                                    treSelected.Nodes.Insert(intNewIndex, objLocationTuple.Item2);
                                    ++intNewIndex;
                                    lstMoveNodes.Remove(objLocationTuple);
                                }
                            }
                        }
                        break;

                    case NotifyCollectionChangedAction.Reset:
                        {
                            List<Location> lstLocations = new List<Location>(
                                CharacterObject.ArmorLocations.Count + CharacterObject.WeaponLocations.Count
                                                                   + CharacterObject.GearLocations.Count
                                                                   + CharacterObject.VehicleLocations.Count
                                                                   + CharacterObject.Vehicles.Count);
                            lstLocations.AddRange(CharacterObject.ArmorLocations);
                            lstLocations.AddRange(CharacterObject.WeaponLocations);
                            lstLocations.AddRange(CharacterObject.GearLocations);
                            lstLocations.AddRange(CharacterObject.VehicleLocations);
                            lstLocations.AddRange(CharacterObject.Vehicles.SelectMany(x => x.Locations));
                            foreach (Location objLocation in lstLocations)
                            {
                                TreeNode nodLocation = treSelected.FindNode(objLocation.InternalId, false);
                                if (nodLocation == null)
                                    continue;
                                if (nodLocation.Nodes.Count > 0)
                                {
                                    if (nodRoot == null)
                                    {
                                        nodRoot = new TreeNode
                                        {
                                            Tag = strNodeName,
                                            Text = LanguageManager.GetString(strNodeName)
                                        };
                                        treSelected.Nodes.Insert(0, nodRoot);
                                    }

                                    for (int i = nodLocation.Nodes.Count - 1; i >= 0; --i)
                                    {
                                        TreeNode nodWeapon = nodLocation.Nodes[i];
                                        nodWeapon.Remove();
                                        nodRoot.Nodes.Add(nodWeapon);
                                    }
                                }

                                objLocation.Remove(false);
                            }
                        }
                        break;
                }

                treSelected.SelectedNode = treSelected.FindNode(strSelectedId);
            }
        }

        #endregion Locations

        protected void RefreshWeapons(TreeView treWeapons, ContextMenuStrip cmsWeaponLocation, ContextMenuStrip cmsWeapon, ContextMenuStrip cmsWeaponAccessory, ContextMenuStrip cmsWeaponAccessoryGear, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs = null)
        {
            if (treWeapons == null)
                return;
            using (CursorWait.New(this))
            {
                string strSelectedId = (treWeapons.SelectedNode?.Tag as IHasInternalId)?.InternalId ?? string.Empty;

                TreeNode nodRoot = null;

                if (notifyCollectionChangedEventArgs == null ||
                    notifyCollectionChangedEventArgs.Action == NotifyCollectionChangedAction.Reset)
                {
                    treWeapons.SuspendLayout();
                    treWeapons.Nodes.Clear();

                    // Start by populating Locations.
                    foreach (Location objLocation in CharacterObject.WeaponLocations)
                    {
                        treWeapons.Nodes.Add(objLocation.CreateTreeNode(cmsWeaponLocation));
                    }

                    foreach (Weapon objWeapon in CharacterObject.Weapons)
                    {
                        AddToTree(objWeapon, -1, false);
                        objWeapon.SetupChildrenWeaponsCollectionChanged(true, treWeapons, cmsWeapon, cmsWeaponAccessory,
                            cmsWeaponAccessoryGear);
                    }

                    treWeapons.SelectedNode = treWeapons.FindNode(strSelectedId);
                    treWeapons.ResumeLayout();
                }
                else
                {
                    nodRoot = treWeapons.FindNode("Node_SelectedWeapons", false);

                    switch (notifyCollectionChangedEventArgs.Action)
                    {
                        case NotifyCollectionChangedAction.Add:
                            {
                                int intNewIndex = notifyCollectionChangedEventArgs.NewStartingIndex;
                                foreach (Weapon objWeapon in notifyCollectionChangedEventArgs.NewItems)
                                {
                                    AddToTree(objWeapon, intNewIndex);
                                    ++intNewIndex;
                                    objWeapon.SetupChildrenWeaponsCollectionChanged(true, treWeapons, cmsWeapon,
                                        cmsWeaponAccessory, cmsWeaponAccessoryGear);
                                }
                            }
                            break;

                        case NotifyCollectionChangedAction.Remove:
                            {
                                foreach (Weapon objWeapon in notifyCollectionChangedEventArgs.OldItems)
                                {
                                    objWeapon.SetupChildrenWeaponsCollectionChanged(false, treWeapons);
                                    treWeapons.FindNode(objWeapon.InternalId)?.Remove();
                                }

                                if (nodRoot != null && nodRoot.Nodes.Count == 0)
                                {
                                    nodRoot.Remove();
                                }
                            }
                            break;

                        case NotifyCollectionChangedAction.Replace:
                            {
                                foreach (Weapon objWeapon in notifyCollectionChangedEventArgs.OldItems)
                                {
                                    objWeapon.SetupChildrenWeaponsCollectionChanged(false, treWeapons);
                                    treWeapons.FindNode(objWeapon.InternalId)?.Remove();
                                }

                                int intNewIndex = notifyCollectionChangedEventArgs.NewStartingIndex;
                                foreach (Weapon objWeapon in notifyCollectionChangedEventArgs.NewItems)
                                {
                                    AddToTree(objWeapon, intNewIndex);
                                    ++intNewIndex;
                                    objWeapon.SetupChildrenWeaponsCollectionChanged(true, treWeapons, cmsWeapon,
                                        cmsWeaponAccessory, cmsWeaponAccessoryGear);
                                }

                                if (nodRoot != null && nodRoot.Nodes.Count == 0)
                                {
                                    nodRoot.Remove();
                                }

                                treWeapons.SelectedNode = treWeapons.FindNode(strSelectedId);
                            }
                            break;

                        case NotifyCollectionChangedAction.Move:
                            {
                                foreach (Weapon objWeapon in notifyCollectionChangedEventArgs.OldItems)
                                {
                                    treWeapons.FindNode(objWeapon.InternalId)?.Remove();
                                }

                                int intNewIndex = notifyCollectionChangedEventArgs.NewStartingIndex;
                                foreach (Weapon objWeapon in notifyCollectionChangedEventArgs.NewItems)
                                {
                                    AddToTree(objWeapon, intNewIndex);
                                    ++intNewIndex;
                                }

                                if (nodRoot != null && nodRoot.Nodes.Count == 0)
                                {
                                    nodRoot.Remove();
                                }

                                treWeapons.SelectedNode = treWeapons.FindNode(strSelectedId);
                            }
                            break;
                    }
                }

                void AddToTree(Weapon objWeapon, int intIndex = -1, bool blnSingleAdd = true)
                {
                    TreeNode objNode = objWeapon.CreateTreeNode(cmsWeapon, cmsWeaponAccessory, cmsWeaponAccessoryGear);
                    if (objNode == null)
                        return;
                    TreeNode nodParent = null;
                    if (objWeapon.Location != null)
                    {
                        nodParent = treWeapons.FindNode(objWeapon.Location.InternalId, false);
                    }

                    if (nodParent == null)
                    {
                        if (nodRoot == null)
                        {
                            nodRoot = new TreeNode
                            {
                                Tag = "Node_SelectedWeapons",
                                Text = LanguageManager.GetString("Node_SelectedWeapons")
                            };
                            treWeapons.Nodes.Insert(0, nodRoot);
                        }

                        nodParent = nodRoot;
                    }

                    if (intIndex >= 0)
                        nodParent.Nodes.Insert(intIndex, objNode);
                    else
                        nodParent.Nodes.Add(objNode);
                    nodParent.Expand();
                    if (blnSingleAdd)
                        treWeapons.SelectedNode = objNode;
                }
            }
        }

        protected void RefreshArmor(TreeView treArmor, ContextMenuStrip cmsArmorLocation, ContextMenuStrip cmsArmor, ContextMenuStrip cmsArmorMod, ContextMenuStrip cmsArmorGear, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs = null)
        {
            if (treArmor == null)
                return;
            using (CursorWait.New(this))
            {
                string strSelectedId = (treArmor.SelectedNode?.Tag as IHasInternalId)?.InternalId ?? string.Empty;

                TreeNode nodRoot = null;

                if (notifyCollectionChangedEventArgs == null ||
                    notifyCollectionChangedEventArgs.Action == NotifyCollectionChangedAction.Reset)
                {
                    treArmor.SuspendLayout();
                    treArmor.Nodes.Clear();

                    // Start by adding Locations.
                    foreach (Location objLocation in CharacterObject.ArmorLocations)
                    {
                        treArmor.Nodes.Add(objLocation.CreateTreeNode(cmsArmorLocation));
                    }

                    // Add Armor.
                    foreach (Armor objArmor in CharacterObject.Armor)
                    {
                        AddToTree(objArmor, -1, false);
                        objArmor.ArmorMods.AddTaggedCollectionChanged(treArmor, MakeDirtyWithCharacterUpdate);
                        objArmor.ArmorMods.AddTaggedCollectionChanged(treArmor,
                            (x, y) => RefreshArmorMods(treArmor, objArmor, cmsArmorMod, cmsArmorGear, y));
                        objArmor.GearChildren.AddTaggedCollectionChanged(treArmor, MakeDirtyWithCharacterUpdate);
                        objArmor.GearChildren.AddTaggedCollectionChanged(treArmor,
                            (x, y) => objArmor.RefreshChildrenGears(treArmor, cmsArmorGear,
                                () => objArmor.ArmorMods.Count, y));
                        foreach (Gear objGear in objArmor.GearChildren)
                            objGear.SetupChildrenGearsCollectionChanged(true, treArmor, cmsArmorGear);
                        foreach (ArmorMod objArmorMod in objArmor.ArmorMods)
                        {
                            objArmorMod.GearChildren.AddTaggedCollectionChanged(treArmor, MakeDirtyWithCharacterUpdate);
                            objArmorMod.GearChildren.AddTaggedCollectionChanged(treArmor,
                                (x, y) => objArmorMod.RefreshChildrenGears(treArmor, cmsArmorGear, null, y));
                            foreach (Gear objGear in objArmorMod.GearChildren)
                                objGear.SetupChildrenGearsCollectionChanged(true, treArmor, cmsArmorGear);
                        }
                    }

                    treArmor.SelectedNode = treArmor.FindNode(strSelectedId);
                    treArmor.ResumeLayout();
                }
                else
                {
                    nodRoot = treArmor.FindNode("Node_SelectedArmor", false);

                    switch (notifyCollectionChangedEventArgs.Action)
                    {
                        case NotifyCollectionChangedAction.Add:
                            {
                                int intNewIndex = notifyCollectionChangedEventArgs.NewStartingIndex;
                                foreach (Armor objArmor in notifyCollectionChangedEventArgs.NewItems)
                                {
                                    AddToTree(objArmor, intNewIndex);
                                    ++intNewIndex;
                                    objArmor.ArmorMods.AddTaggedCollectionChanged(treArmor, MakeDirtyWithCharacterUpdate);
                                    objArmor.ArmorMods.AddTaggedCollectionChanged(treArmor,
                                        (x, y) => RefreshArmorMods(treArmor, objArmor, cmsArmorMod, cmsArmorGear, y));
                                    objArmor.GearChildren.AddTaggedCollectionChanged(treArmor, MakeDirtyWithCharacterUpdate);
                                    objArmor.GearChildren.AddTaggedCollectionChanged(treArmor,
                                        (x, y) => objArmor.RefreshChildrenGears(treArmor, cmsArmorGear,
                                            () => objArmor.ArmorMods.Count, y));
                                    foreach (Gear objGear in objArmor.GearChildren)
                                        objGear.SetupChildrenGearsCollectionChanged(true, treArmor, cmsArmorGear);
                                    foreach (ArmorMod objArmorMod in objArmor.ArmorMods)
                                    {
                                        objArmorMod.GearChildren.AddTaggedCollectionChanged(treArmor, MakeDirtyWithCharacterUpdate);
                                        objArmorMod.GearChildren.AddTaggedCollectionChanged(treArmor,
                                            (x, y) => objArmorMod.RefreshChildrenGears(treArmor, cmsArmorGear, null, y));
                                        foreach (Gear objGear in objArmorMod.GearChildren)
                                            objGear.SetupChildrenGearsCollectionChanged(true, treArmor, cmsArmorGear);
                                    }
                                }
                            }
                            break;

                        case NotifyCollectionChangedAction.Remove:
                            {
                                foreach (Armor objArmor in notifyCollectionChangedEventArgs.OldItems)
                                {
                                    objArmor.ArmorMods.RemoveTaggedCollectionChanged(treArmor);
                                    objArmor.GearChildren.RemoveTaggedCollectionChanged(treArmor);
                                    foreach (Gear objGear in objArmor.GearChildren)
                                        objGear.SetupChildrenGearsCollectionChanged(false, treArmor);
                                    foreach (ArmorMod objArmorMod in objArmor.ArmorMods)
                                    {
                                        objArmorMod.GearChildren.RemoveTaggedCollectionChanged(treArmor);
                                        foreach (Gear objGear in objArmorMod.GearChildren)
                                            objGear.SetupChildrenGearsCollectionChanged(true, treArmor, cmsArmorGear);
                                    }

                                    treArmor.FindNode(objArmor.InternalId)?.Remove();
                                }

                                if (nodRoot != null && nodRoot.Nodes.Count == 0)
                                {
                                    nodRoot.Remove();
                                }
                            }
                            break;

                        case NotifyCollectionChangedAction.Replace:
                            {
                                foreach (Armor objArmor in notifyCollectionChangedEventArgs.OldItems)
                                {
                                    objArmor.ArmorMods.RemoveTaggedCollectionChanged(treArmor);
                                    objArmor.GearChildren.RemoveTaggedCollectionChanged(treArmor);
                                    foreach (Gear objGear in objArmor.GearChildren)
                                        objGear.SetupChildrenGearsCollectionChanged(false, treArmor);
                                    foreach (ArmorMod objArmorMod in objArmor.ArmorMods)
                                    {
                                        objArmorMod.GearChildren.RemoveTaggedCollectionChanged(treArmor);
                                        foreach (Gear objGear in objArmorMod.GearChildren)
                                            objGear.SetupChildrenGearsCollectionChanged(true, treArmor, cmsArmorGear);
                                    }

                                    treArmor.FindNode(objArmor.InternalId)?.Remove();
                                }

                                int intNewIndex = notifyCollectionChangedEventArgs.NewStartingIndex;
                                foreach (Armor objArmor in notifyCollectionChangedEventArgs.NewItems)
                                {
                                    AddToTree(objArmor, intNewIndex);
                                    ++intNewIndex;
                                    objArmor.ArmorMods.AddTaggedCollectionChanged(treArmor, MakeDirtyWithCharacterUpdate);
                                    objArmor.ArmorMods.AddTaggedCollectionChanged(treArmor,
                                        (x, y) => RefreshArmorMods(treArmor, objArmor, cmsArmorMod, cmsArmorGear, y));
                                    objArmor.GearChildren.AddTaggedCollectionChanged(treArmor, MakeDirtyWithCharacterUpdate);
                                    objArmor.GearChildren.AddTaggedCollectionChanged(treArmor,
                                        (x, y) => objArmor.RefreshChildrenGears(treArmor, cmsArmorGear,
                                            () => objArmor.ArmorMods.Count, y));
                                    foreach (Gear objGear in objArmor.GearChildren)
                                        objGear.SetupChildrenGearsCollectionChanged(true, treArmor, cmsArmorGear);
                                    foreach (ArmorMod objArmorMod in objArmor.ArmorMods)
                                    {
                                        objArmorMod.GearChildren.AddTaggedCollectionChanged(treArmor, MakeDirtyWithCharacterUpdate);
                                        objArmorMod.GearChildren.AddTaggedCollectionChanged(treArmor,
                                            (x, y) => objArmorMod.RefreshChildrenGears(treArmor, cmsArmorGear, null, y));
                                        foreach (Gear objGear in objArmorMod.GearChildren)
                                            objGear.SetupChildrenGearsCollectionChanged(true, treArmor, cmsArmorGear);
                                    }
                                }

                                if (nodRoot != null && nodRoot.Nodes.Count == 0)
                                {
                                    nodRoot.Remove();
                                }

                                treArmor.SelectedNode = treArmor.FindNode(strSelectedId);
                            }
                            break;

                        case NotifyCollectionChangedAction.Move:
                            {
                                foreach (Armor objArmor in notifyCollectionChangedEventArgs.OldItems)
                                {
                                    treArmor.FindNode(objArmor.InternalId)?.Remove();
                                }

                                int intNewIndex = notifyCollectionChangedEventArgs.NewStartingIndex;
                                foreach (Armor objArmor in notifyCollectionChangedEventArgs.NewItems)
                                {
                                    AddToTree(objArmor, intNewIndex);
                                    ++intNewIndex;
                                }

                                if (nodRoot != null && nodRoot.Nodes.Count == 0)
                                {
                                    nodRoot.Remove();
                                }

                                treArmor.SelectedNode = treArmor.FindNode(strSelectedId);
                            }
                            break;
                    }
                }

                void AddToTree(Armor objArmor, int intIndex = -1, bool blnSingleAdd = true)
                {
                    TreeNode objNode = objArmor.CreateTreeNode(cmsArmor, cmsArmorMod, cmsArmorGear);
                    if (objNode == null)
                        return;
                    TreeNode nodParent = null;
                    if (objArmor.Location != null)
                    {
                        nodParent = treArmor.FindNode(objArmor.Location.InternalId, false);
                    }

                    if (nodParent == null)
                    {
                        if (nodRoot == null)
                        {
                            nodRoot = new TreeNode
                            {
                                Tag = "Node_SelectedArmor",
                                Text = LanguageManager.GetString("Node_SelectedArmor")
                            };
                            treArmor.Nodes.Insert(0, nodRoot);
                        }

                        nodParent = nodRoot;
                    }

                    if (intIndex >= 0)
                        nodParent.Nodes.Insert(intIndex, objNode);
                    else
                        nodParent.Nodes.Add(objNode);
                    nodParent.Expand();
                    if (blnSingleAdd)
                        treArmor.SelectedNode = objNode;
                }
            }
        }

        protected void RefreshArmorMods(TreeView treArmor, Armor objArmor, ContextMenuStrip cmsArmorMod, ContextMenuStrip cmsArmorGear, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            if (treArmor == null || objArmor == null || notifyCollectionChangedEventArgs == null)
                return;
            TreeNode nodArmor = treArmor.FindNode(objArmor.InternalId);
            if (nodArmor == null)
                return;
            using (CursorWait.New(this))
            {
                switch (notifyCollectionChangedEventArgs.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        {
                            int intNewIndex = notifyCollectionChangedEventArgs.NewStartingIndex;
                            foreach (ArmorMod objArmorMod in notifyCollectionChangedEventArgs.NewItems)
                            {
                                AddToTree(objArmorMod, intNewIndex);
                                objArmorMod.GearChildren.AddTaggedCollectionChanged(treArmor, MakeDirtyWithCharacterUpdate);
                                objArmorMod.GearChildren.AddTaggedCollectionChanged(treArmor,
                                    (x, y) => objArmorMod.RefreshChildrenGears(treArmor, cmsArmorGear, null, y));
                                foreach (Gear objGear in objArmorMod.GearChildren)
                                    objGear.SetupChildrenGearsCollectionChanged(true, treArmor, cmsArmorGear);
                                ++intNewIndex;
                            }
                        }
                        break;

                    case NotifyCollectionChangedAction.Remove:
                        {
                            foreach (ArmorMod objArmorMod in notifyCollectionChangedEventArgs.OldItems)
                            {
                                objArmorMod.GearChildren.RemoveTaggedCollectionChanged(treArmor);
                                foreach (Gear objGear in objArmorMod.GearChildren)
                                    objGear.SetupChildrenGearsCollectionChanged(false, treArmor);
                                nodArmor.FindNode(objArmorMod.InternalId)?.Remove();
                            }
                        }
                        break;

                    case NotifyCollectionChangedAction.Replace:
                        {
                            string strSelectedId =
                                (treArmor.SelectedNode?.Tag as IHasInternalId)?.InternalId ?? string.Empty;
                            foreach (ArmorMod objArmorMod in notifyCollectionChangedEventArgs.OldItems)
                            {
                                objArmorMod.GearChildren.RemoveTaggedCollectionChanged(treArmor);
                                foreach (Gear objGear in objArmorMod.GearChildren)
                                    objGear.SetupChildrenGearsCollectionChanged(false, treArmor);
                                nodArmor.FindNode(objArmorMod.InternalId)?.Remove();
                            }

                            int intNewIndex = notifyCollectionChangedEventArgs.NewStartingIndex;
                            foreach (ArmorMod objArmorMod in notifyCollectionChangedEventArgs.NewItems)
                            {
                                AddToTree(objArmorMod, intNewIndex);
                                objArmorMod.GearChildren.AddTaggedCollectionChanged(treArmor, MakeDirtyWithCharacterUpdate);
                                objArmorMod.GearChildren.AddTaggedCollectionChanged(treArmor,
                                    (x, y) => objArmorMod.RefreshChildrenGears(treArmor, cmsArmorGear, null, y));
                                foreach (Gear objGear in objArmorMod.GearChildren)
                                    objGear.SetupChildrenGearsCollectionChanged(true, treArmor, cmsArmorGear);
                                ++intNewIndex;
                            }

                            treArmor.SelectedNode = treArmor.FindNode(strSelectedId);
                        }
                        break;

                    case NotifyCollectionChangedAction.Move:
                        {
                            string strSelectedId =
                                (treArmor.SelectedNode?.Tag as IHasInternalId)?.InternalId ?? string.Empty;
                            foreach (ArmorMod objArmorMod in notifyCollectionChangedEventArgs.OldItems)
                            {
                                nodArmor.FindNode(objArmorMod.InternalId)?.Remove();
                            }

                            int intNewIndex = notifyCollectionChangedEventArgs.NewStartingIndex;
                            foreach (ArmorMod objArmorMod in notifyCollectionChangedEventArgs.NewItems)
                            {
                                AddToTree(objArmorMod, intNewIndex);
                                ++intNewIndex;
                            }

                            treArmor.SelectedNode = treArmor.FindNode(strSelectedId);
                        }
                        break;

                    case NotifyCollectionChangedAction.Reset:
                        {
                            for (int i = nodArmor.Nodes.Count - 1; i >= 0; --i)
                            {
                                TreeNode objNode = nodArmor.Nodes[i];
                                if (objNode.Tag is ArmorMod objNodeMod && !ReferenceEquals(objNodeMod.Parent, objArmor))
                                {
                                    objNode.Remove();
                                }
                            }
                        }
                        break;
                }

                void AddToTree(ArmorMod objArmorMod, int intIndex = -1, bool blnSingleAdd = true)
                {
                    TreeNode objNode = objArmorMod.CreateTreeNode(cmsArmorMod, cmsArmorGear);
                    if (objNode == null)
                        return;
                    if (intIndex >= 0)
                        nodArmor.Nodes.Insert(intIndex, objNode);
                    else
                        nodArmor.Nodes.Add(objNode);
                    nodArmor.Expand();
                    if (blnSingleAdd)
                        treArmor.SelectedNode = objNode;
                }
            }
        }

        protected void RefreshGears(TreeView treGear, ContextMenuStrip cmsGearLocation, ContextMenuStrip cmsGear, bool blnCommlinksOnly, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs = null)
        {
            if (treGear == null)
                return;
            using (CursorWait.New(this))
            {
                string strSelectedId = (treGear.SelectedNode?.Tag as IHasInternalId)?.InternalId ?? string.Empty;

                TreeNode nodRoot = null;

                if (notifyCollectionChangedEventArgs == null ||
                    notifyCollectionChangedEventArgs.Action == NotifyCollectionChangedAction.Reset)
                {
                    treGear.SuspendLayout();
                    treGear.Nodes.Clear();

                    // Start by populating Locations.
                    foreach (Location objLocation in CharacterObject.GearLocations)
                    {
                        treGear.Nodes.Add(objLocation.CreateTreeNode(cmsGearLocation));
                    }

                    // Add Gear.
                    foreach (Gear objGear in CharacterObject.Gear)
                    {
                        AddToTree(objGear, -1, false);
                        objGear.SetupChildrenGearsCollectionChanged(true, treGear, cmsGear);
                    }

                    treGear.SelectedNode = treGear.FindNode(strSelectedId);
                    treGear.ResumeLayout();
                }
                else
                {
                    nodRoot = treGear.FindNode("Node_SelectedGear", false);

                    switch (notifyCollectionChangedEventArgs.Action)
                    {
                        case NotifyCollectionChangedAction.Add:
                            {
                                int intNewIndex = notifyCollectionChangedEventArgs.NewStartingIndex;
                                foreach (Gear objGear in notifyCollectionChangedEventArgs.NewItems)
                                {
                                    AddToTree(objGear, intNewIndex);
                                    objGear.SetupChildrenGearsCollectionChanged(true, treGear, cmsGear);
                                    ++intNewIndex;
                                }
                            }
                            break;

                        case NotifyCollectionChangedAction.Remove:
                            {
                                foreach (Gear objGear in notifyCollectionChangedEventArgs.OldItems)
                                {
                                    objGear.SetupChildrenGearsCollectionChanged(false, treGear);
                                    treGear.FindNodeByTag(objGear)?.Remove();
                                }
                            }
                            break;

                        case NotifyCollectionChangedAction.Replace:
                            {
                                foreach (Gear objGear in notifyCollectionChangedEventArgs.OldItems)
                                {
                                    objGear.SetupChildrenGearsCollectionChanged(false, treGear, cmsGear);
                                    treGear.FindNodeByTag(objGear)?.Remove();
                                }

                                int intNewIndex = notifyCollectionChangedEventArgs.NewStartingIndex;
                                foreach (Gear objGear in notifyCollectionChangedEventArgs.NewItems)
                                {
                                    AddToTree(objGear, intNewIndex);
                                    objGear.Children.AddTaggedCollectionChanged(treGear, MakeDirtyWithCharacterUpdate);
                                    objGear.Children.AddTaggedCollectionChanged(treGear,
                                        (x, y) => objGear.RefreshChildrenGears(treGear, cmsGear, null, y));
                                    objGear.SetupChildrenGearsCollectionChanged(true, treGear, cmsGear);
                                    ++intNewIndex;
                                }

                                treGear.SelectedNode = treGear.FindNode(strSelectedId);
                            }
                            break;

                        case NotifyCollectionChangedAction.Move:
                            {
                                foreach (Gear objGear in notifyCollectionChangedEventArgs.OldItems)
                                {
                                    treGear.FindNodeByTag(objGear)?.Remove();
                                }

                                int intNewIndex = notifyCollectionChangedEventArgs.NewStartingIndex;
                                foreach (Gear objGear in notifyCollectionChangedEventArgs.NewItems)
                                {
                                    AddToTree(objGear, intNewIndex);
                                    ++intNewIndex;
                                }

                                treGear.SelectedNode = treGear.FindNode(strSelectedId);
                            }
                            break;
                    }
                }

                void AddToTree(Gear objGear, int intIndex = -1, bool blnSingleAdd = true)
                {
                    if (blnCommlinksOnly && !objGear.IsCommlink)
                        return;

                    TreeNode objNode = objGear.CreateTreeNode(cmsGear);
                    if (objNode == null)
                        return;
                    TreeNode nodParent = null;
                    if (objGear.Location != null)
                    {
                        nodParent = treGear.FindNodeByTag(objGear.Location, false);
                    }

                    if (nodParent == null)
                    {
                        if (nodRoot == null)
                        {
                            nodRoot = new TreeNode
                            {
                                Tag = "Node_SelectedGear",
                                Text = LanguageManager.GetString("Node_SelectedGear")
                            };
                            treGear.Nodes.Insert(0, nodRoot);
                        }

                        nodParent = nodRoot;
                    }

                    if (intIndex >= 0)
                        nodParent.Nodes.Insert(intIndex, objNode);
                    else
                        nodParent.Nodes.Add(objNode);
                    nodParent.Expand();
                    if (blnSingleAdd)
                        treGear.SelectedNode = objNode;
                }
            }
        }

        protected void RefreshDrugs(TreeView treGear, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs = null)
        {
            if (treGear == null)
                return;
            using (CursorWait.New(this))
            {
                string strSelectedId = (treGear.SelectedNode?.Tag as IHasInternalId)?.InternalId ?? string.Empty;

                TreeNode nodRoot = null;

                if (notifyCollectionChangedEventArgs == null ||
                    notifyCollectionChangedEventArgs.Action == NotifyCollectionChangedAction.Reset)
                {
                    treGear.SuspendLayout();
                    treGear.Nodes.Clear();

                    // Add Gear.
                    foreach (Drug d in CharacterObject.Drugs)
                    {
                        AddToTree(d, -1, false);
                    }

                    treGear.SelectedNode = treGear.FindNode(strSelectedId);
                    treGear.ResumeLayout();
                }
                else
                {
                    nodRoot = treGear.FindNode("Node_SelectedDrugs", false);

                    switch (notifyCollectionChangedEventArgs.Action)
                    {
                        case NotifyCollectionChangedAction.Add:
                            {
                                int intNewIndex = notifyCollectionChangedEventArgs.NewStartingIndex;
                                foreach (Drug d in notifyCollectionChangedEventArgs.NewItems)
                                {
                                    AddToTree(d, intNewIndex);
                                    ++intNewIndex;
                                }
                            }
                            break;

                        case NotifyCollectionChangedAction.Remove:
                            {
                                foreach (Drug d in notifyCollectionChangedEventArgs.OldItems)
                                {
                                    treGear.FindNodeByTag(d)?.Remove();
                                }
                            }
                            break;

                        case NotifyCollectionChangedAction.Replace:
                        case NotifyCollectionChangedAction.Move:
                            {
                                foreach (Drug d in notifyCollectionChangedEventArgs.OldItems)
                                {
                                    treGear.FindNodeByTag(d)?.Remove();
                                }

                                int intNewIndex = notifyCollectionChangedEventArgs.NewStartingIndex;
                                foreach (Drug d in notifyCollectionChangedEventArgs.NewItems)
                                {
                                    AddToTree(d, intNewIndex);
                                    ++intNewIndex;
                                }

                                treGear.SelectedNode = treGear.FindNode(strSelectedId);
                            }
                            break;
                    }
                }

                void AddToTree(Drug objGear, int intIndex = -1, bool blnSingleAdd = true)
                {
                    TreeNode objNode = objGear.CreateTreeNode();
                    if (objNode == null)
                        return;
                    if (nodRoot == null)
                    {
                        nodRoot = new TreeNode
                        {
                            Tag = "Node_SelectedDrugs",
                            Text = LanguageManager.GetString("Node_SelectedDrugs")
                        };
                        treGear.Nodes.Insert(0, nodRoot);
                    }

                    if (intIndex >= 0)
                        nodRoot.Nodes.Insert(intIndex, objNode);
                    else
                        nodRoot.Nodes.Add(objNode);
                    nodRoot.Expand();
                    if (blnSingleAdd)
                        treGear.SelectedNode = objNode;
                }
            }
        }

        protected void RefreshCyberware(TreeView treCyberware, ContextMenuStrip cmsCyberware, ContextMenuStrip cmsCyberwareGear, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs = null)
        {
            if (treCyberware == null)
                return;

            TreeNode objCyberwareRoot = null;
            TreeNode objBiowareRoot = null;
            TreeNode objModularRoot = null;
            TreeNode objHoleNode = null;
            TreeNode objAntiHoleNode = null;

            using (CursorWait.New(this))
            {
                string strSelectedId = (treCyberware.SelectedNode?.Tag as IHasInternalId)?.InternalId ?? string.Empty;

                if (notifyCollectionChangedEventArgs == null ||
                    notifyCollectionChangedEventArgs.Action == NotifyCollectionChangedAction.Reset)
                {
                    treCyberware.SuspendLayout();
                    treCyberware.Nodes.Clear();

                    foreach (Cyberware objCyberware in CharacterObject.Cyberware)
                    {
                        AddToTree(objCyberware, false);
                        objCyberware.SetupChildrenCyberwareCollectionChanged(true, treCyberware, cmsCyberware,
                            cmsCyberwareGear);
                    }

                    treCyberware.SortCustomAlphabetically(strSelectedId);
                    treCyberware.ResumeLayout();
                }
                else
                {
                    objCyberwareRoot = treCyberware.FindNode("Node_SelectedCyberware", false);
                    objBiowareRoot = treCyberware.FindNode("Node_SelectedBioware", false);
                    objModularRoot = treCyberware.FindNode("Node_UnequippedModularCyberware", false);
                    objHoleNode =
                        treCyberware.FindNode(
                            Cyberware.EssenceHoleGUID.ToString("D", GlobalSettings.InvariantCultureInfo), false);
                    objAntiHoleNode =
                        treCyberware.FindNode(
                            Cyberware.EssenceAntiHoleGUID.ToString("D", GlobalSettings.InvariantCultureInfo), false);
                    switch (notifyCollectionChangedEventArgs.Action)
                    {
                        case NotifyCollectionChangedAction.Add:
                            {
                                foreach (Cyberware objCyberware in notifyCollectionChangedEventArgs.NewItems)
                                {
                                    AddToTree(objCyberware);
                                    objCyberware.SetupChildrenCyberwareCollectionChanged(true, treCyberware, cmsCyberware,
                                        cmsCyberwareGear);
                                }
                            }
                            break;

                        case NotifyCollectionChangedAction.Remove:
                            {
                                foreach (Cyberware objCyberware in notifyCollectionChangedEventArgs.OldItems)
                                {
                                    objCyberware.SetupChildrenCyberwareCollectionChanged(false, treCyberware);
                                    TreeNode objNode = treCyberware.FindNodeByTag(objCyberware);
                                    if (objNode != null)
                                    {
                                        TreeNode objParent = objNode.Parent;
                                        objNode.Remove();
                                        if (objParent != null && objParent.Level == 0 && objParent.Nodes.Count == 0)
                                            objParent.Remove();
                                    }
                                }
                            }
                            break;

                        case NotifyCollectionChangedAction.Replace:
                            {
                                List<TreeNode> lstOldParentNodes =
                                    new List<TreeNode>(notifyCollectionChangedEventArgs.OldItems.Count);

                                foreach (Cyberware objCyberware in notifyCollectionChangedEventArgs.OldItems)
                                {
                                    objCyberware.SetupChildrenCyberwareCollectionChanged(false, treCyberware);
                                    TreeNode objNode = treCyberware.FindNodeByTag(objCyberware);
                                    if (objNode != null)
                                    {
                                        TreeNode objParent = objNode.Parent;
                                        objNode.Remove();
                                        if (objParent != null && objParent.Level == 0)
                                            lstOldParentNodes.Add(objParent);
                                    }
                                }

                                foreach (Cyberware objCyberware in notifyCollectionChangedEventArgs.NewItems)
                                {
                                    AddToTree(objCyberware);
                                    objCyberware.SetupChildrenCyberwareCollectionChanged(true, treCyberware, cmsCyberware,
                                        cmsCyberwareGear);
                                }

                                foreach (TreeNode objOldParent in lstOldParentNodes)
                                {
                                    if (objOldParent.Nodes.Count == 0)
                                        objOldParent.Remove();
                                }

                                treCyberware.SelectedNode = treCyberware.FindNode(strSelectedId);
                            }
                            break;
                    }
                }

                void AddToTree(Cyberware objCyberware, bool blnSingleAdd = true)
                {
                    if (objCyberware.SourceID == Cyberware.EssenceHoleGUID)
                    {
                        if (objHoleNode == null)
                        {
                            objHoleNode = objCyberware.CreateTreeNode(null, null);
                            treCyberware.Nodes.Insert(3, objHoleNode);
                        }

                        if (blnSingleAdd)
                            treCyberware.SelectedNode = objHoleNode;
                        return;
                    }

                    if (objCyberware.SourceID == Cyberware.EssenceAntiHoleGUID)
                    {
                        if (objAntiHoleNode == null)
                        {
                            objAntiHoleNode = objCyberware.CreateTreeNode(null, null);
                            treCyberware.Nodes.Insert(3, objAntiHoleNode);
                        }

                        if (blnSingleAdd)
                            treCyberware.SelectedNode = objAntiHoleNode;
                        return;
                    }

                    TreeNode objNode = objCyberware.CreateTreeNode(cmsCyberware, cmsCyberwareGear);
                    if (objNode == null)
                        return;

                    TreeNode nodParent = null;
                    switch (objCyberware.SourceType)
                    {
                        case Improvement.ImprovementSource.Cyberware when objCyberware.IsModularCurrentlyEquipped:
                            {
                                if (objCyberwareRoot == null)
                                {
                                    objCyberwareRoot = new TreeNode
                                    {
                                        Tag = "Node_SelectedCyberware",
                                        Text = LanguageManager.GetString("Node_SelectedCyberware")
                                    };
                                    treCyberware.Nodes.Insert(0, objCyberwareRoot);
                                    objCyberwareRoot.Expand();
                                }

                                nodParent = objCyberwareRoot;
                                break;
                            }
                        case Improvement.ImprovementSource.Cyberware:
                            {
                                if (objModularRoot == null)
                                {
                                    objModularRoot = new TreeNode
                                    {
                                        Tag = "Node_UnequippedModularCyberware",
                                        Text = LanguageManager.GetString("Node_UnequippedModularCyberware")
                                    };
                                    int intIndex = 0;
                                    if (objBiowareRoot != null || objCyberwareRoot != null)
                                        intIndex = objBiowareRoot != null && objCyberwareRoot != null ? 2 : 1;
                                    treCyberware.Nodes.Insert(intIndex, objModularRoot);
                                    objModularRoot.Expand();
                                }

                                nodParent = objModularRoot;
                                break;
                            }
                        case Improvement.ImprovementSource.Bioware:
                            {
                                if (objBiowareRoot == null)
                                {
                                    objBiowareRoot = new TreeNode
                                    {
                                        Tag = "Node_SelectedBioware",
                                        Text = LanguageManager.GetString("Node_SelectedBioware")
                                    };
                                    treCyberware.Nodes.Insert(objCyberwareRoot == null ? 0 : 1, objBiowareRoot);
                                    objBiowareRoot.Expand();
                                }

                                nodParent = objBiowareRoot;
                                break;
                            }
                    }

                    if (nodParent != null)
                    {
                        if (blnSingleAdd)
                        {
                            TreeNodeCollection lstParentNodeChildren = nodParent.Nodes;
                            int intNodesCount = lstParentNodeChildren.Count;
                            int intTargetIndex = 0;
                            for (; intTargetIndex < intNodesCount; ++intTargetIndex)
                            {
                                if (CompareTreeNodes.CompareText(lstParentNodeChildren[intTargetIndex], objNode) >= 0)
                                {
                                    break;
                                }
                            }

                            lstParentNodeChildren.Insert(intTargetIndex, objNode);
                            treCyberware.SelectedNode = objNode;
                        }
                        else
                            nodParent.Nodes.Add(objNode);
                    }
                }
            }
        }

        protected void RefreshVehicles(TreeView treVehicles, ContextMenuStrip cmsVehicleLocation, ContextMenuStrip cmsVehicle, ContextMenuStrip cmsVehicleWeapon, ContextMenuStrip cmsVehicleWeaponAccessory, ContextMenuStrip cmsVehicleWeaponAccessoryGear, ContextMenuStrip cmsVehicleGear, ContextMenuStrip cmsVehicleWeaponMount, ContextMenuStrip cmsCyberware, ContextMenuStrip cmsCyberwareGear, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs = null)
        {
            if (treVehicles == null)
                return;
            using (CursorWait.New(this))
            {
                string strSelectedId = (treVehicles.SelectedNode?.Tag as IHasInternalId)?.InternalId ?? string.Empty;

                TreeNode nodRoot = null;

                if (notifyCollectionChangedEventArgs == null ||
                    notifyCollectionChangedEventArgs.Action == NotifyCollectionChangedAction.Reset)
                {
                    treVehicles.SuspendLayout();
                    treVehicles.Nodes.Clear();

                    // Start by populating Locations.
                    foreach (Location objLocation in CharacterObject.VehicleLocations)
                    {
                        treVehicles.Nodes.Add(objLocation.CreateTreeNode(cmsVehicleLocation));
                    }

                    // Add Vehicles.
                    foreach (Vehicle objVehicle in CharacterObject.Vehicles)
                    {
                        AddToTree(objVehicle, -1, false);
                        objVehicle.Mods.AddTaggedCollectionChanged(treVehicles, MakeDirtyWithCharacterUpdate);
                        objVehicle.Mods.AddTaggedCollectionChanged(treVehicles,
                            (x, y) => objVehicle.RefreshVehicleMods(treVehicles, cmsVehicle, cmsCyberware,
                                cmsCyberwareGear, cmsVehicleWeapon, cmsVehicleWeaponAccessory,
                                cmsVehicleWeaponAccessoryGear, null, y));
                        objVehicle.WeaponMounts.AddTaggedCollectionChanged(treVehicles, MakeDirtyWithCharacterUpdate);
                        objVehicle.WeaponMounts.AddTaggedCollectionChanged(treVehicles,
                            (x, y) => objVehicle.RefreshVehicleWeaponMounts(treVehicles, cmsVehicleWeaponMount,
                                cmsVehicleWeapon, cmsVehicleWeaponAccessory, cmsVehicleWeaponAccessoryGear,
                                cmsCyberware, cmsCyberwareGear, cmsVehicle, () => objVehicle.Mods.Count, y));
                        objVehicle.Weapons.AddTaggedCollectionChanged(treVehicles, MakeDirtyWithCharacterUpdate);
                        objVehicle.Weapons.AddTaggedCollectionChanged(treVehicles,
                            (x, y) => objVehicle.RefreshChildrenWeapons(treVehicles, cmsVehicleWeapon,
                                cmsVehicleWeaponAccessory, cmsVehicleWeaponAccessoryGear,
                                () => objVehicle.Mods.Count + (objVehicle.WeaponMounts.Count > 0 ? 1 : 0), y));
                        foreach (VehicleMod objMod in objVehicle.Mods)
                        {
                            objMod.Cyberware.AddTaggedCollectionChanged(treVehicles, MakeDirtyWithCharacterUpdate);
                            objMod.Cyberware.AddTaggedCollectionChanged(treVehicles,
                                (x, y) => objMod.RefreshChildrenCyberware(treVehicles, cmsCyberware, cmsCyberwareGear,
                                    null, y));
                            foreach (Cyberware objCyberware in objMod.Cyberware)
                                objCyberware.SetupChildrenCyberwareCollectionChanged(true, treVehicles, cmsCyberware,
                                    cmsCyberwareGear);
                            objMod.Weapons.AddTaggedCollectionChanged(treVehicles, MakeDirtyWithCharacterUpdate);
                            objMod.Weapons.AddTaggedCollectionChanged(treVehicles,
                                (x, y) => objMod.RefreshChildrenWeapons(treVehicles, cmsVehicleWeapon,
                                    cmsVehicleWeaponAccessory, cmsVehicleWeaponAccessoryGear,
                                    () => objMod.Cyberware.Count, y));
                            foreach (Weapon objWeapon in objMod.Weapons)
                                objWeapon.SetupChildrenWeaponsCollectionChanged(true, treVehicles, cmsVehicleWeapon,
                                    cmsVehicleWeaponAccessory, cmsVehicleWeaponAccessoryGear);
                        }

                        foreach (WeaponMount objMount in objVehicle.WeaponMounts)
                        {
                            objMount.Mods.AddTaggedCollectionChanged(treVehicles, MakeDirtyWithCharacterUpdate);
                            objMount.Mods.AddTaggedCollectionChanged(treVehicles,
                                (x, y) => objMount.RefreshVehicleMods(treVehicles, cmsVehicle, cmsCyberware,
                                    cmsCyberwareGear, cmsVehicleWeapon, cmsVehicleWeaponAccessory,
                                    cmsVehicleWeaponAccessoryGear, null, y));
                            objMount.Weapons.AddTaggedCollectionChanged(treVehicles, MakeDirtyWithCharacterUpdate);
                            objMount.Weapons.AddTaggedCollectionChanged(treVehicles,
                                (x, y) => objMount.RefreshChildrenWeapons(treVehicles, cmsVehicleWeapon,
                                    cmsVehicleWeaponAccessory, cmsVehicleWeaponAccessoryGear, () => objMount.Mods.Count,
                                    y));
                            foreach (Weapon objWeapon in objMount.Weapons)
                                objWeapon.SetupChildrenWeaponsCollectionChanged(true, treVehicles, cmsVehicleWeapon,
                                    cmsVehicleWeaponAccessory, cmsVehicleWeaponAccessoryGear);
                            foreach (VehicleMod objMod in objMount.Mods)
                            {
                                foreach (Cyberware objCyberware in objMod.Cyberware)
                                    objCyberware.SetupChildrenCyberwareCollectionChanged(true, treVehicles,
                                        cmsCyberware, cmsCyberwareGear);
                                foreach (Weapon objWeapon in objMod.Weapons)
                                    objWeapon.SetupChildrenWeaponsCollectionChanged(true, treVehicles, cmsVehicleWeapon,
                                        cmsVehicleWeaponAccessory, cmsVehicleWeaponAccessoryGear);
                            }
                        }

                        foreach (Weapon objWeapon in objVehicle.Weapons)
                            objWeapon.SetupChildrenWeaponsCollectionChanged(true, treVehicles, cmsVehicleWeapon,
                                cmsVehicleWeaponAccessory, cmsVehicleWeaponAccessoryGear);
                        objVehicle.GearChildren.AddTaggedCollectionChanged(treVehicles, MakeDirtyWithCharacterUpdate);
                        objVehicle.GearChildren.AddTaggedCollectionChanged(treVehicles,
                            (x, y) => objVehicle.RefreshChildrenGears(treVehicles, cmsVehicleGear,
                                () => objVehicle.Mods.Count + objVehicle.Weapons.Count +
                                      (objVehicle.WeaponMounts.Count > 0 ? 1 : 0), y));
                        foreach (Gear objGear in objVehicle.GearChildren)
                            objGear.SetupChildrenGearsCollectionChanged(true, treVehicles, cmsVehicleGear);
                        objVehicle.Locations.AddTaggedCollectionChanged(treVehicles, MakeDirty);
                        objVehicle.Locations.AddTaggedCollectionChanged(treVehicles,
                            (x, y) => RefreshLocationsInVehicle(treVehicles, objVehicle, cmsVehicleLocation,
                                () => objVehicle.Mods.Count + objVehicle.Weapons.Count +
                                      (objVehicle.WeaponMounts.Count > 0 ? 1 : 0) +
                                      objVehicle.GearChildren.Count(z => z.Location == null), y));
                    }

                    treVehicles.SelectedNode = treVehicles.FindNode(strSelectedId);
                    treVehicles.ResumeLayout();
                }
                else
                {
                    nodRoot = treVehicles.FindNode("Node_SelectedVehicles", false);

                    switch (notifyCollectionChangedEventArgs.Action)
                    {
                        case NotifyCollectionChangedAction.Add:
                            {
                                int intNewIndex = notifyCollectionChangedEventArgs.NewStartingIndex;
                                foreach (Vehicle objVehicle in notifyCollectionChangedEventArgs.NewItems)
                                {
                                    AddToTree(objVehicle, intNewIndex);
                                    objVehicle.Mods.AddTaggedCollectionChanged(treVehicles, MakeDirtyWithCharacterUpdate);
                                    objVehicle.Mods.AddTaggedCollectionChanged(treVehicles,
                                        (x, y) => objVehicle.RefreshVehicleMods(treVehicles, cmsVehicle, cmsCyberware,
                                            cmsCyberwareGear, cmsVehicleWeapon, cmsVehicleWeaponAccessory,
                                            cmsVehicleWeaponAccessoryGear, null, y));
                                    objVehicle.WeaponMounts.AddTaggedCollectionChanged(treVehicles, MakeDirtyWithCharacterUpdate);
                                    objVehicle.WeaponMounts.AddTaggedCollectionChanged(treVehicles,
                                        (x, y) => objVehicle.RefreshVehicleWeaponMounts(treVehicles, cmsVehicleWeaponMount,
                                            cmsVehicleWeapon, cmsVehicleWeaponAccessory, cmsVehicleWeaponAccessoryGear,
                                            cmsCyberware, cmsCyberwareGear, cmsVehicle, () => objVehicle.Mods.Count, y));
                                    objVehicle.Weapons.AddTaggedCollectionChanged(treVehicles, MakeDirtyWithCharacterUpdate);
                                    objVehicle.Weapons.AddTaggedCollectionChanged(treVehicles,
                                        (x, y) => objVehicle.RefreshChildrenWeapons(treVehicles, cmsVehicleWeapon,
                                            cmsVehicleWeaponAccessory, cmsVehicleWeaponAccessoryGear,
                                            () => objVehicle.Mods.Count + (objVehicle.WeaponMounts.Count > 0 ? 1 : 0), y));
                                    foreach (VehicleMod objMod in objVehicle.Mods)
                                    {
                                        objMod.Cyberware.AddTaggedCollectionChanged(treVehicles, MakeDirtyWithCharacterUpdate);
                                        objMod.Cyberware.AddTaggedCollectionChanged(treVehicles,
                                            (x, y) => objMod.RefreshChildrenCyberware(treVehicles, cmsCyberware,
                                                cmsCyberwareGear, null, y));
                                        foreach (Cyberware objCyberware in objMod.Cyberware)
                                            objCyberware.SetupChildrenCyberwareCollectionChanged(true, treVehicles,
                                                cmsCyberware, cmsCyberwareGear);
                                        objMod.Weapons.AddTaggedCollectionChanged(treVehicles, MakeDirtyWithCharacterUpdate);
                                        objMod.Weapons.AddTaggedCollectionChanged(treVehicles,
                                            (x, y) => objMod.RefreshChildrenWeapons(treVehicles, cmsVehicleWeapon,
                                                cmsVehicleWeaponAccessory, cmsVehicleWeaponAccessoryGear,
                                                () => objMod.Cyberware.Count, y));
                                        foreach (Weapon objWeapon in objMod.Weapons)
                                            objWeapon.SetupChildrenWeaponsCollectionChanged(true, treVehicles,
                                                cmsVehicleWeapon, cmsVehicleWeaponAccessory, cmsVehicleWeaponAccessoryGear);
                                    }

                                    foreach (WeaponMount objMount in objVehicle.WeaponMounts)
                                    {
                                        objMount.Mods.AddTaggedCollectionChanged(treVehicles, MakeDirtyWithCharacterUpdate);
                                        objMount.Mods.AddTaggedCollectionChanged(treVehicles,
                                            (x, y) => objMount.RefreshVehicleMods(treVehicles, cmsVehicle, cmsCyberware,
                                                cmsCyberwareGear, cmsVehicleWeapon, cmsVehicleWeaponAccessory,
                                                cmsVehicleWeaponAccessoryGear, null, y));
                                        objMount.Weapons.AddTaggedCollectionChanged(treVehicles, MakeDirtyWithCharacterUpdate);
                                        objMount.Weapons.AddTaggedCollectionChanged(treVehicles,
                                            (x, y) => objMount.RefreshChildrenWeapons(treVehicles, cmsVehicleWeapon,
                                                cmsVehicleWeaponAccessory, cmsVehicleWeaponAccessoryGear,
                                                () => objMount.Mods.Count, y));
                                        foreach (Weapon objWeapon in objMount.Weapons)
                                            objWeapon.SetupChildrenWeaponsCollectionChanged(true, treVehicles,
                                                cmsVehicleWeapon, cmsVehicleWeaponAccessory, cmsVehicleWeaponAccessoryGear);
                                        foreach (VehicleMod objMod in objMount.Mods)
                                        {
                                            foreach (Cyberware objCyberware in objMod.Cyberware)
                                                objCyberware.SetupChildrenCyberwareCollectionChanged(true, treVehicles,
                                                    cmsCyberware, cmsCyberwareGear);
                                            foreach (Weapon objWeapon in objMod.Weapons)
                                                objWeapon.SetupChildrenWeaponsCollectionChanged(true, treVehicles,
                                                    cmsVehicleWeapon, cmsVehicleWeaponAccessory,
                                                    cmsVehicleWeaponAccessoryGear);
                                        }
                                    }

                                    foreach (Weapon objWeapon in objVehicle.Weapons)
                                        objWeapon.SetupChildrenWeaponsCollectionChanged(true, treVehicles, cmsVehicleWeapon,
                                            cmsVehicleWeaponAccessory, cmsVehicleWeaponAccessoryGear);
                                    objVehicle.GearChildren.AddTaggedCollectionChanged(treVehicles, MakeDirtyWithCharacterUpdate);
                                    objVehicle.GearChildren.AddTaggedCollectionChanged(treVehicles,
                                        (x, y) => objVehicle.RefreshChildrenGears(treVehicles, cmsVehicleGear,
                                            () => objVehicle.Mods.Count + objVehicle.Weapons.Count +
                                                  (objVehicle.WeaponMounts.Count > 0 ? 1 : 0), y));
                                    foreach (Gear objGear in objVehicle.GearChildren)
                                        objGear.SetupChildrenGearsCollectionChanged(true, treVehicles, cmsVehicleGear);
                                    objVehicle.Locations.AddTaggedCollectionChanged(treVehicles, MakeDirty);
                                    objVehicle.Locations.AddTaggedCollectionChanged(treVehicles,
                                        (x, y) => RefreshLocationsInVehicle(treVehicles, objVehicle, cmsVehicleLocation,
                                            () => objVehicle.Mods.Count + objVehicle.Weapons.Count +
                                                  (objVehicle.WeaponMounts.Count > 0 ? 1 : 0) +
                                                  objVehicle.GearChildren.Count(z => z.Location == null), y));
                                    ++intNewIndex;
                                }
                            }
                            break;

                        case NotifyCollectionChangedAction.Remove:
                            {
                                foreach (Vehicle objVehicle in notifyCollectionChangedEventArgs.OldItems)
                                {
                                    objVehicle.Mods.RemoveTaggedCollectionChanged(treVehicles);
                                    objVehicle.WeaponMounts.RemoveTaggedCollectionChanged(treVehicles);
                                    objVehicle.Weapons.RemoveTaggedCollectionChanged(treVehicles);
                                    foreach (VehicleMod objMod in objVehicle.Mods)
                                    {
                                        objMod.Cyberware.RemoveTaggedCollectionChanged(treVehicles);
                                        foreach (Cyberware objCyberware in objMod.Cyberware)
                                            objCyberware.SetupChildrenCyberwareCollectionChanged(false, treVehicles);
                                        objMod.Weapons.RemoveTaggedCollectionChanged(treVehicles);
                                        foreach (Weapon objWeapon in objMod.Weapons)
                                            objWeapon.SetupChildrenWeaponsCollectionChanged(false, treVehicles);
                                    }

                                    foreach (WeaponMount objMount in objVehicle.WeaponMounts)
                                    {
                                        objMount.Mods.RemoveTaggedCollectionChanged(treVehicles);
                                        objMount.Weapons.RemoveTaggedCollectionChanged(treVehicles);
                                        foreach (Weapon objWeapon in objMount.Weapons)
                                            objWeapon.SetupChildrenWeaponsCollectionChanged(false, treVehicles);
                                        foreach (VehicleMod objMod in objMount.Mods)
                                        {
                                            objMod.Cyberware.RemoveTaggedCollectionChanged(treVehicles);
                                            foreach (Cyberware objCyberware in objMod.Cyberware)
                                                objCyberware.SetupChildrenCyberwareCollectionChanged(false, treVehicles);
                                            objMod.Weapons.RemoveTaggedCollectionChanged(treVehicles);
                                            foreach (Weapon objWeapon in objMod.Weapons)
                                                objWeapon.SetupChildrenWeaponsCollectionChanged(false, treVehicles);
                                        }
                                    }

                                    foreach (Weapon objWeapon in objVehicle.Weapons)
                                        objWeapon.SetupChildrenWeaponsCollectionChanged(false, treVehicles);
                                    objVehicle.GearChildren.RemoveTaggedCollectionChanged(treVehicles);
                                    foreach (Gear objGear in objVehicle.GearChildren)
                                        objGear.SetupChildrenGearsCollectionChanged(false, treVehicles);
                                    objVehicle.Locations.RemoveTaggedCollectionChanged(treVehicles);
                                    treVehicles.FindNodeByTag(objVehicle)?.Remove();
                                }
                            }
                            break;

                        case NotifyCollectionChangedAction.Replace:
                            {
                                foreach (Vehicle objVehicle in notifyCollectionChangedEventArgs.OldItems)
                                {
                                    objVehicle.Mods.RemoveTaggedCollectionChanged(treVehicles);
                                    objVehicle.WeaponMounts.RemoveTaggedCollectionChanged(treVehicles);
                                    objVehicle.Weapons.RemoveTaggedCollectionChanged(treVehicles);
                                    foreach (VehicleMod objMod in objVehicle.Mods)
                                    {
                                        objMod.Cyberware.RemoveTaggedCollectionChanged(treVehicles);
                                        foreach (Cyberware objCyberware in objMod.Cyberware)
                                            objCyberware.SetupChildrenCyberwareCollectionChanged(false, treVehicles);
                                        objMod.Weapons.RemoveTaggedCollectionChanged(treVehicles);
                                        foreach (Weapon objWeapon in objMod.Weapons)
                                            objWeapon.SetupChildrenWeaponsCollectionChanged(false, treVehicles);
                                    }

                                    foreach (WeaponMount objMount in objVehicle.WeaponMounts)
                                    {
                                        objMount.Mods.RemoveTaggedCollectionChanged(treVehicles);
                                        objMount.Weapons.RemoveTaggedCollectionChanged(treVehicles);
                                        foreach (Weapon objWeapon in objMount.Weapons)
                                            objWeapon.SetupChildrenWeaponsCollectionChanged(false, treVehicles);
                                        foreach (VehicleMod objMod in objMount.Mods)
                                        {
                                            objMod.Cyberware.RemoveTaggedCollectionChanged(treVehicles);
                                            foreach (Cyberware objCyberware in objMod.Cyberware)
                                                objCyberware.SetupChildrenCyberwareCollectionChanged(false, treVehicles);
                                            objMod.Weapons.RemoveTaggedCollectionChanged(treVehicles);
                                            foreach (Weapon objWeapon in objMod.Weapons)
                                                objWeapon.SetupChildrenWeaponsCollectionChanged(false, treVehicles);
                                        }
                                    }

                                    foreach (Weapon objWeapon in objVehicle.Weapons)
                                        objWeapon.SetupChildrenWeaponsCollectionChanged(false, treVehicles);
                                    objVehicle.GearChildren.RemoveTaggedCollectionChanged(treVehicles);
                                    foreach (Gear objGear in objVehicle.GearChildren)
                                        objGear.SetupChildrenGearsCollectionChanged(false, treVehicles);
                                    objVehicle.Locations.RemoveTaggedCollectionChanged(treVehicles);
                                    treVehicles.FindNodeByTag(objVehicle)?.Remove();
                                }

                                int intNewIndex = notifyCollectionChangedEventArgs.NewStartingIndex;
                                foreach (Vehicle objVehicle in notifyCollectionChangedEventArgs.NewItems)
                                {
                                    AddToTree(objVehicle, intNewIndex);
                                    objVehicle.Mods.AddTaggedCollectionChanged(treVehicles, MakeDirtyWithCharacterUpdate);
                                    objVehicle.Mods.AddTaggedCollectionChanged(treVehicles,
                                        (x, y) => objVehicle.RefreshVehicleMods(treVehicles, cmsVehicle, cmsCyberware,
                                            cmsCyberwareGear, cmsVehicleWeapon, cmsVehicleWeaponAccessory,
                                            cmsVehicleWeaponAccessoryGear, null, y));
                                    objVehicle.WeaponMounts.AddTaggedCollectionChanged(treVehicles, MakeDirtyWithCharacterUpdate);
                                    objVehicle.WeaponMounts.AddTaggedCollectionChanged(treVehicles,
                                        (x, y) => objVehicle.RefreshVehicleWeaponMounts(treVehicles, cmsVehicleWeaponMount,
                                            cmsVehicleWeapon, cmsVehicleWeaponAccessory, cmsVehicleWeaponAccessoryGear,
                                            cmsCyberware, cmsCyberwareGear, cmsVehicle, () => objVehicle.Mods.Count, y));
                                    objVehicle.Weapons.AddTaggedCollectionChanged(treVehicles, MakeDirtyWithCharacterUpdate);
                                    objVehicle.Weapons.AddTaggedCollectionChanged(treVehicles,
                                        (x, y) => objVehicle.RefreshChildrenWeapons(treVehicles, cmsVehicleWeapon,
                                            cmsVehicleWeaponAccessory, cmsVehicleWeaponAccessoryGear,
                                            () => objVehicle.Mods.Count + (objVehicle.WeaponMounts.Count > 0 ? 1 : 0), y));
                                    foreach (VehicleMod objMod in objVehicle.Mods)
                                    {
                                        objMod.Cyberware.AddTaggedCollectionChanged(treVehicles, MakeDirtyWithCharacterUpdate);
                                        objMod.Cyberware.AddTaggedCollectionChanged(treVehicles,
                                            (x, y) => objMod.RefreshChildrenCyberware(treVehicles, cmsCyberware,
                                                cmsCyberwareGear, null, y));
                                        foreach (Cyberware objCyberware in objMod.Cyberware)
                                            objCyberware.SetupChildrenCyberwareCollectionChanged(true, treVehicles,
                                                cmsCyberware, cmsCyberwareGear);
                                        objMod.Weapons.AddTaggedCollectionChanged(treVehicles, MakeDirtyWithCharacterUpdate);
                                        objMod.Weapons.AddTaggedCollectionChanged(treVehicles,
                                            (x, y) => objMod.RefreshChildrenWeapons(treVehicles, cmsVehicleWeapon,
                                                cmsVehicleWeaponAccessory, cmsVehicleWeaponAccessoryGear,
                                                () => objMod.Cyberware.Count, y));
                                        foreach (Weapon objWeapon in objMod.Weapons)
                                            objWeapon.SetupChildrenWeaponsCollectionChanged(true, treVehicles,
                                                cmsVehicleWeapon, cmsVehicleWeaponAccessory, cmsVehicleWeaponAccessoryGear);
                                    }

                                    foreach (WeaponMount objMount in objVehicle.WeaponMounts)
                                    {
                                        objMount.Mods.AddTaggedCollectionChanged(treVehicles, MakeDirtyWithCharacterUpdate);
                                        objMount.Mods.AddTaggedCollectionChanged(treVehicles,
                                            (x, y) => objMount.RefreshVehicleMods(treVehicles, cmsVehicle, cmsCyberware,
                                                cmsCyberwareGear, cmsVehicleWeapon, cmsVehicleWeaponAccessory,
                                                cmsVehicleWeaponAccessoryGear, null, y));
                                        objMount.Weapons.AddTaggedCollectionChanged(treVehicles, MakeDirtyWithCharacterUpdate);
                                        objMount.Weapons.AddTaggedCollectionChanged(treVehicles,
                                            (x, y) => objMount.RefreshChildrenWeapons(treVehicles, cmsVehicleWeapon,
                                                cmsVehicleWeaponAccessory, cmsVehicleWeaponAccessoryGear,
                                                () => objMount.Mods.Count, y));
                                        foreach (Weapon objWeapon in objMount.Weapons)
                                            objWeapon.SetupChildrenWeaponsCollectionChanged(true, treVehicles,
                                                cmsVehicleWeapon, cmsVehicleWeaponAccessory, cmsVehicleWeaponAccessoryGear);
                                        foreach (VehicleMod objMod in objMount.Mods)
                                        {
                                            objMod.Cyberware.AddTaggedCollectionChanged(treVehicles, MakeDirtyWithCharacterUpdate);
                                            objMod.Cyberware.AddTaggedCollectionChanged(treVehicles,
                                                (x, y) => objMod.RefreshChildrenCyberware(treVehicles, cmsCyberware,
                                                    cmsCyberwareGear, null, y));
                                            foreach (Cyberware objCyberware in objMod.Cyberware)
                                                objCyberware.SetupChildrenCyberwareCollectionChanged(true, treVehicles,
                                                    cmsCyberware, cmsCyberwareGear);
                                            objMod.Weapons.AddTaggedCollectionChanged(treVehicles, MakeDirtyWithCharacterUpdate);
                                            objMod.Weapons.AddTaggedCollectionChanged(treVehicles,
                                                (x, y) => objMod.RefreshChildrenWeapons(treVehicles, cmsVehicleWeapon,
                                                    cmsVehicleWeaponAccessory, cmsVehicleWeaponAccessoryGear,
                                                    () => objMod.Cyberware.Count, y));
                                            foreach (Weapon objWeapon in objMod.Weapons)
                                                objWeapon.SetupChildrenWeaponsCollectionChanged(true, treVehicles,
                                                    cmsVehicleWeapon, cmsVehicleWeaponAccessory,
                                                    cmsVehicleWeaponAccessoryGear);
                                        }
                                    }

                                    foreach (Weapon objWeapon in objVehicle.Weapons)
                                        objWeapon.SetupChildrenWeaponsCollectionChanged(true, treVehicles, cmsVehicleWeapon,
                                            cmsVehicleWeaponAccessory, cmsVehicleWeaponAccessoryGear);
                                    objVehicle.GearChildren.AddTaggedCollectionChanged(treVehicles, MakeDirtyWithCharacterUpdate);
                                    objVehicle.GearChildren.AddTaggedCollectionChanged(treVehicles,
                                        (x, y) => objVehicle.RefreshChildrenGears(treVehicles, cmsVehicleGear,
                                            () => objVehicle.Mods.Count + objVehicle.Weapons.Count +
                                                  (objVehicle.WeaponMounts.Count > 0 ? 1 : 0), y));
                                    foreach (Gear objGear in objVehicle.GearChildren)
                                        objGear.SetupChildrenGearsCollectionChanged(true, treVehicles, cmsVehicleGear);
                                    objVehicle.Locations.AddTaggedCollectionChanged(treVehicles, MakeDirtyWithCharacterUpdate);
                                    objVehicle.Locations.AddTaggedCollectionChanged(treVehicles,
                                        (x, y) => RefreshLocationsInVehicle(treVehicles, objVehicle, cmsVehicleLocation,
                                            () => objVehicle.Mods.Count + objVehicle.Weapons.Count +
                                                  (objVehicle.WeaponMounts.Count > 0 ? 1 : 0) +
                                                  objVehicle.GearChildren.Count(z => z.Location != null), y));
                                    ++intNewIndex;
                                }

                                treVehicles.SelectedNode = treVehicles.FindNode(strSelectedId);
                            }
                            break;

                        case NotifyCollectionChangedAction.Move:
                            {
                                foreach (Vehicle objVehicle in notifyCollectionChangedEventArgs.OldItems)
                                {
                                    treVehicles.FindNodeByTag(objVehicle)?.Remove();
                                }

                                int intNewIndex = notifyCollectionChangedEventArgs.NewStartingIndex;
                                foreach (Vehicle objVehicle in notifyCollectionChangedEventArgs.NewItems)
                                {
                                    AddToTree(objVehicle, intNewIndex);
                                    ++intNewIndex;
                                }

                                treVehicles.SelectedNode = treVehicles.FindNode(strSelectedId);
                            }
                            break;
                    }
                }

                void AddToTree(Vehicle objVehicle, int intIndex = -1, bool blnSingleAdd = true)
                {
                    TreeNode objNode = objVehicle.CreateTreeNode(cmsVehicle, cmsVehicleLocation, cmsVehicleWeapon,
                        cmsVehicleWeaponAccessory, cmsVehicleWeaponAccessoryGear, cmsVehicleGear, cmsVehicleWeaponMount,
                        cmsCyberware, cmsCyberwareGear);
                    if (objNode == null)
                        return;

                    TreeNode nodParent = null;
                    if (objVehicle.Location != null)
                    {
                        nodParent = treVehicles.FindNodeByTag(objVehicle.Location, false);
                    }

                    if (nodParent == null)
                    {
                        if (nodRoot == null)
                        {
                            nodRoot = new TreeNode
                            {
                                Tag = "Node_SelectedVehicles",
                                Text = LanguageManager.GetString("Node_SelectedVehicles")
                            };
                            treVehicles.Nodes.Insert(0, nodRoot);
                        }

                        nodParent = nodRoot;
                    }

                    if (intIndex >= 0)
                        nodParent.Nodes.Insert(intIndex, objNode);
                    else
                        nodParent.Nodes.Add(objNode);
                    nodParent.Expand();
                    if (blnSingleAdd)
                        treVehicles.SelectedNode = objNode;
                }
            }
        }

        public void RefreshFociFromGear(TreeView treFoci, ContextMenuStrip cmsFocus, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs = null)
        {
            if (treFoci == null)
                return;
            using (CursorWait.New(this))
            {
                string strSelectedId = (treFoci.SelectedNode?.Tag as IHasInternalId)?.InternalId ?? string.Empty;

                if (notifyCollectionChangedEventArgs == null ||
                    notifyCollectionChangedEventArgs.Action == NotifyCollectionChangedAction.Reset)
                {
                    treFoci.SuspendLayout();
                    treFoci.Nodes.Clear();

                    int intFociTotal = 0;

                    int intMaxFocusTotal = CharacterObject.MAG.TotalValue * 5;
                    if (CharacterObjectSettings.MysAdeptSecondMAGAttribute && CharacterObject.IsMysticAdept)
                        intMaxFocusTotal = Math.Min(intMaxFocusTotal, CharacterObject.MAGAdept.TotalValue * 5);

                    foreach (Gear objGear in CharacterObject.Gear)
                    {
                        switch (objGear.Category)
                        {
                            case "Foci":
                            case "Metamagic Foci":
                                {
                                    TreeNode objNode = objGear.CreateTreeNode(cmsFocus);
                                    if (objNode == null)
                                        continue;
                                    objNode.Text = objNode.Text.CheapReplace(LanguageManager.GetString("String_Rating"),
                                        () => LanguageManager.GetString(objGear.RatingLabel));
                                    for (int i = CharacterObject.Foci.Count - 1; i >= 0; --i)
                                    {
                                        if (i < CharacterObject.Foci.Count)
                                        {
                                            Focus objFocus = CharacterObject.Foci[i];
                                            if (objFocus.GearObject == objGear)
                                            {
                                                intFociTotal += objFocus.Rating;
                                                // Do not let the number of BP spend on bonded Foci exceed MAG * 5.
                                                if (intFociTotal > intMaxFocusTotal && !CharacterObject.IgnoreRules)
                                                {
                                                    objGear.Bonded = false;
                                                    CharacterObject.Foci.RemoveAt(i);
                                                    objNode.Checked = false;
                                                }
                                                else
                                                    objNode.Checked = true;
                                            }
                                        }
                                    }

                                    AddToTree(objNode, false);
                                }
                                break;

                            case "Stacked Focus":
                                {
                                    foreach (StackedFocus objStack in CharacterObject.StackedFoci)
                                    {
                                        if (objStack.GearId == objGear.InternalId)
                                        {
                                            ImprovementManager.RemoveImprovements(CharacterObject,
                                                Improvement.ImprovementSource.StackedFocus, objStack.InternalId);

                                            if (objStack.Bonded)
                                            {
                                                foreach (Gear objFociGear in objStack.Gear)
                                                {
                                                    if (!string.IsNullOrEmpty(objFociGear.Extra))
                                                        ImprovementManager.ForcedValue = objFociGear.Extra;
                                                    ImprovementManager.CreateImprovements(CharacterObject,
                                                        Improvement.ImprovementSource.StackedFocus, objStack.InternalId,
                                                        objFociGear.Bonus, objFociGear.Rating,
                                                        objFociGear.DisplayNameShort(GlobalSettings.Language));
                                                    if (objFociGear.WirelessOn)
                                                        ImprovementManager.CreateImprovements(CharacterObject,
                                                            Improvement.ImprovementSource.StackedFocus, objStack.InternalId,
                                                            objFociGear.WirelessBonus, objFociGear.Rating,
                                                            objFociGear.DisplayNameShort(GlobalSettings.Language));
                                                }
                                            }

                                            AddToTree(objStack.CreateTreeNode(objGear, cmsFocus), false);
                                        }
                                    }
                                }
                                break;
                        }
                    }

                    treFoci.SortCustomAlphabetically(strSelectedId);
                    treFoci.ResumeLayout();
                }
                else
                {
                    switch (notifyCollectionChangedEventArgs.Action)
                    {
                        case NotifyCollectionChangedAction.Add:
                            {
                                bool blnWarned = false;
                                int intMaxFocusTotal = CharacterObject.MAG.TotalValue * 5;
                                if (CharacterObjectSettings.MysAdeptSecondMAGAttribute && CharacterObject.IsMysticAdept)
                                    intMaxFocusTotal = Math.Min(intMaxFocusTotal, CharacterObject.MAGAdept.TotalValue * 5);

                                HashSet<Gear> setNewGears = new HashSet<Gear>();
                                foreach (Gear objGear in notifyCollectionChangedEventArgs.NewItems)
                                    setNewGears.Add(objGear);

                                int intFociTotal = CharacterObject.Foci.Where(x => !setNewGears.Contains(x.GearObject))
                                    .Sum(x => x.Rating);

                                foreach (Gear objGear in notifyCollectionChangedEventArgs.NewItems)
                                {
                                    switch (objGear.Category)
                                    {
                                        case "Foci":
                                        case "Metamagic Foci":
                                            {
                                                TreeNode objNode = objGear.CreateTreeNode(cmsFocus);
                                                if (objNode == null)
                                                    continue;
                                                objNode.Text = objNode.Text.CheapReplace(
                                                    LanguageManager.GetString("String_Rating"),
                                                    () => LanguageManager.GetString("String_Force"));
                                                for (int i = CharacterObject.Foci.Count - 1; i >= 0; --i)
                                                {
                                                    if (i < CharacterObject.Foci.Count)
                                                    {
                                                        Focus objFocus = CharacterObject.Foci[i];
                                                        if (objFocus.GearObject == objGear)
                                                        {
                                                            intFociTotal += objFocus.Rating;
                                                            // Do not let the number of BP spend on bonded Foci exceed MAG * 5.
                                                            if (intFociTotal > intMaxFocusTotal && !CharacterObject.IgnoreRules)
                                                            {
                                                                // Mark the Gear a Bonded.
                                                                objGear.Bonded = false;
                                                                CharacterObject.Foci.RemoveAt(i);
                                                                objNode.Checked = false;
                                                                if (!blnWarned)
                                                                {
                                                                    Program.ShowMessageBox(this,
                                                                        LanguageManager.GetString("Message_FocusMaximumForce"),
                                                                        LanguageManager.GetString("MessageTitle_FocusMaximum"),
                                                                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                                                                    blnWarned = true;
                                                                    break;
                                                                }
                                                            }
                                                            else
                                                                objNode.Checked = true;
                                                        }
                                                    }
                                                }

                                                AddToTree(objNode);
                                            }
                                            break;

                                        case "Stacked Focus":
                                            {
                                                foreach (StackedFocus objStack in CharacterObject.StackedFoci)
                                                {
                                                    if (objStack.GearId == objGear.InternalId)
                                                    {
                                                        ImprovementManager.RemoveImprovements(CharacterObject,
                                                            Improvement.ImprovementSource.StackedFocus, objStack.InternalId);

                                                        if (objStack.Bonded)
                                                        {
                                                            foreach (Gear objFociGear in objStack.Gear)
                                                            {
                                                                if (!string.IsNullOrEmpty(objFociGear.Extra))
                                                                    ImprovementManager.ForcedValue = objFociGear.Extra;
                                                                ImprovementManager.CreateImprovements(CharacterObject,
                                                                    Improvement.ImprovementSource.StackedFocus,
                                                                    objStack.InternalId, objFociGear.Bonus, objFociGear.Rating,
                                                                    objFociGear.DisplayNameShort(GlobalSettings.Language));
                                                                if (objFociGear.WirelessOn)
                                                                    ImprovementManager.CreateImprovements(CharacterObject,
                                                                        Improvement.ImprovementSource.StackedFocus,
                                                                        objStack.InternalId, objFociGear.WirelessBonus,
                                                                        objFociGear.Rating,
                                                                        objFociGear.DisplayNameShort(GlobalSettings.Language));
                                                            }
                                                        }

                                                        AddToTree(objStack.CreateTreeNode(objGear, cmsFocus));
                                                    }
                                                }
                                            }
                                            break;
                                    }
                                }
                            }
                            break;

                        case NotifyCollectionChangedAction.Remove:
                            {
                                foreach (Gear objGear in notifyCollectionChangedEventArgs.OldItems)
                                {
                                    switch (objGear.Category)
                                    {
                                        case "Foci":
                                        case "Metamagic Foci":
                                            {
                                                for (int i = CharacterObject.Foci.Count - 1; i >= 0; --i)
                                                {
                                                    if (i < CharacterObject.Foci.Count)
                                                    {
                                                        Focus objFocus = CharacterObject.Foci[i];
                                                        if (objFocus.GearObject == objGear)
                                                        {
                                                            CharacterObject.Foci.RemoveAt(i);
                                                        }
                                                    }
                                                }

                                                treFoci.FindNodeByTag(objGear)?.Remove();
                                            }
                                            break;

                                        case "Stacked Focus":
                                            {
                                                for (int i = CharacterObject.StackedFoci.Count - 1; i >= 0; --i)
                                                {
                                                    if (i < CharacterObject.StackedFoci.Count)
                                                    {
                                                        StackedFocus objStack = CharacterObject.StackedFoci[i];
                                                        if (objStack.GearId == objGear.InternalId)
                                                        {
                                                            CharacterObject.StackedFoci.RemoveAt(i);
                                                            treFoci.FindNodeByTag(objStack)?.Remove();
                                                        }
                                                    }
                                                }
                                            }
                                            break;
                                    }
                                }
                            }
                            break;

                        case NotifyCollectionChangedAction.Replace:
                            {
                                foreach (Gear objGear in notifyCollectionChangedEventArgs.OldItems)
                                {
                                    switch (objGear.Category)
                                    {
                                        case "Foci":
                                        case "Metamagic Foci":
                                            {
                                                for (int i = CharacterObject.Foci.Count - 1; i >= 0; --i)
                                                {
                                                    if (i < CharacterObject.Foci.Count)
                                                    {
                                                        Focus objFocus = CharacterObject.Foci[i];
                                                        if (objFocus.GearObject == objGear)
                                                        {
                                                            CharacterObject.Foci.RemoveAt(i);
                                                        }
                                                    }
                                                }

                                                treFoci.FindNodeByTag(objGear)?.Remove();
                                            }
                                            break;

                                        case "Stacked Focus":
                                            {
                                                for (int i = CharacterObject.StackedFoci.Count - 1; i >= 0; --i)
                                                {
                                                    if (i < CharacterObject.StackedFoci.Count)
                                                    {
                                                        StackedFocus objStack = CharacterObject.StackedFoci[i];
                                                        if (objStack.GearId == objGear.InternalId)
                                                        {
                                                            CharacterObject.StackedFoci.RemoveAt(i);
                                                            treFoci.FindNodeByTag(objStack)?.Remove();
                                                        }
                                                    }
                                                }
                                            }
                                            break;
                                    }
                                }

                                bool blnWarned = false;
                                int intMaxFocusTotal = CharacterObject.MAG.TotalValue * 5;
                                if (CharacterObjectSettings.MysAdeptSecondMAGAttribute && CharacterObject.IsMysticAdept)
                                    intMaxFocusTotal = Math.Min(intMaxFocusTotal, CharacterObject.MAGAdept.TotalValue * 5);

                                HashSet<Gear> setNewGears = new HashSet<Gear>();
                                foreach (Gear objGear in notifyCollectionChangedEventArgs.NewItems)
                                    setNewGears.Add(objGear);

                                int intFociTotal = CharacterObject.Foci.Where(x => !setNewGears.Contains(x.GearObject))
                                    .Sum(x => x.Rating);

                                foreach (Gear objGear in notifyCollectionChangedEventArgs.NewItems)
                                {
                                    switch (objGear.Category)
                                    {
                                        case "Foci":
                                        case "Metamagic Foci":
                                            {
                                                TreeNode objNode = objGear.CreateTreeNode(cmsFocus);
                                                if (objNode == null)
                                                    continue;
                                                objNode.Text = objNode.Text.CheapReplace(
                                                    LanguageManager.GetString("String_Rating"),
                                                    () => LanguageManager.GetString("String_Force"));
                                                for (int i = CharacterObject.Foci.Count - 1; i >= 0; --i)
                                                {
                                                    if (i < CharacterObject.Foci.Count)
                                                    {
                                                        Focus objFocus = CharacterObject.Foci[i];
                                                        if (objFocus.GearObject == objGear)
                                                        {
                                                            intFociTotal += objFocus.Rating;
                                                            // Do not let the number of BP spend on bonded Foci exceed MAG * 5.
                                                            if (intFociTotal > intMaxFocusTotal && !CharacterObject.IgnoreRules)
                                                            {
                                                                // Mark the Gear a Bonded.
                                                                objGear.Bonded = false;
                                                                CharacterObject.Foci.RemoveAt(i);
                                                                objNode.Checked = false;
                                                                if (!blnWarned)
                                                                {
                                                                    Program.ShowMessageBox(this,
                                                                        LanguageManager.GetString("Message_FocusMaximumForce"),
                                                                        LanguageManager.GetString("MessageTitle_FocusMaximum"),
                                                                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                                                                    blnWarned = true;
                                                                    break;
                                                                }
                                                            }
                                                            else
                                                                objNode.Checked = true;
                                                        }
                                                    }
                                                }

                                                AddToTree(objNode);
                                            }
                                            break;

                                        case "Stacked Focus":
                                            {
                                                foreach (StackedFocus objStack in CharacterObject.StackedFoci)
                                                {
                                                    if (objStack.GearId == objGear.InternalId)
                                                    {
                                                        ImprovementManager.RemoveImprovements(CharacterObject,
                                                            Improvement.ImprovementSource.StackedFocus, objStack.InternalId);

                                                        if (objStack.Bonded)
                                                        {
                                                            foreach (Gear objFociGear in objStack.Gear)
                                                            {
                                                                if (!string.IsNullOrEmpty(objFociGear.Extra))
                                                                    ImprovementManager.ForcedValue = objFociGear.Extra;
                                                                ImprovementManager.CreateImprovements(CharacterObject,
                                                                    Improvement.ImprovementSource.StackedFocus,
                                                                    objStack.InternalId, objFociGear.Bonus, objFociGear.Rating,
                                                                    objFociGear.DisplayNameShort(GlobalSettings.Language));
                                                                if (objFociGear.WirelessOn)
                                                                    ImprovementManager.CreateImprovements(CharacterObject,
                                                                        Improvement.ImprovementSource.StackedFocus,
                                                                        objStack.InternalId, objFociGear.WirelessBonus,
                                                                        objFociGear.Rating,
                                                                        objFociGear.DisplayNameShort(GlobalSettings.Language));
                                                            }
                                                        }

                                                        AddToTree(objStack.CreateTreeNode(objGear, cmsFocus));
                                                    }
                                                }
                                            }
                                            break;
                                    }
                                }
                            }
                            break;
                    }
                }

                void AddToTree(TreeNode objNode, bool blnSingleAdd = true)
                {
                    TreeNodeCollection lstParentNodeChildren = treFoci.Nodes;
                    if (blnSingleAdd)
                    {
                        int intNodesCount = lstParentNodeChildren.Count;
                        int intTargetIndex = 0;
                        for (; intTargetIndex < intNodesCount; ++intTargetIndex)
                        {
                            if (CompareTreeNodes.CompareText(lstParentNodeChildren[intTargetIndex], objNode) >= 0)
                            {
                                break;
                            }
                        }

                        lstParentNodeChildren.Insert(intTargetIndex, objNode);
                        treFoci.SelectedNode = objNode;
                    }
                    else
                        lstParentNodeChildren.Add(objNode);
                }
            }
        }

        protected void RefreshMartialArts(TreeView treMartialArts, ContextMenuStrip cmsMartialArts, ContextMenuStrip cmsTechnique, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs = null)
        {
            if (treMartialArts == null)
                return;
            using (CursorWait.New(this))
            {
                string strSelectedId = (treMartialArts.SelectedNode?.Tag as IHasInternalId)?.InternalId ?? string.Empty;

                TreeNode objMartialArtsParentNode = null;
                TreeNode objQualityNode = null;

                if (notifyCollectionChangedEventArgs == null ||
                    notifyCollectionChangedEventArgs.Action == NotifyCollectionChangedAction.Reset)
                {
                    treMartialArts.SuspendLayout();
                    treMartialArts.Nodes.Clear();

                    foreach (MartialArt objMartialArt in CharacterObject.MartialArts)
                    {
                        AddToTree(objMartialArt, false);
                        objMartialArt.Techniques.AddTaggedCollectionChanged(treMartialArts, MakeDirtyWithCharacterUpdate);
                        objMartialArt.Techniques.AddTaggedCollectionChanged(treMartialArts,
                            (x, y) => RefreshMartialArtTechniques(treMartialArts, objMartialArt, cmsTechnique, y));
                    }

                    treMartialArts.SortCustomAlphabetically(strSelectedId);
                    treMartialArts.ResumeLayout();
                }
                else
                {
                    objMartialArtsParentNode = treMartialArts.FindNode("Node_SelectedMartialArts", false);
                    objQualityNode = treMartialArts.FindNode("Node_SelectedQualities", false);
                    switch (notifyCollectionChangedEventArgs.Action)
                    {
                        case NotifyCollectionChangedAction.Add:
                            {
                                foreach (MartialArt objMartialArt in notifyCollectionChangedEventArgs.NewItems)
                                {
                                    AddToTree(objMartialArt);
                                    objMartialArt.Techniques.AddTaggedCollectionChanged(treMartialArts, MakeDirtyWithCharacterUpdate);
                                    objMartialArt.Techniques.AddTaggedCollectionChanged(treMartialArts,
                                        (x, y) => RefreshMartialArtTechniques(treMartialArts, objMartialArt, cmsTechnique,
                                            y));
                                }
                            }
                            break;

                        case NotifyCollectionChangedAction.Remove:
                            {
                                foreach (MartialArt objMartialArt in notifyCollectionChangedEventArgs.OldItems)
                                {
                                    objMartialArt.Techniques.RemoveTaggedCollectionChanged(treMartialArts);
                                    TreeNode objNode = treMartialArts.FindNodeByTag(objMartialArt);
                                    if (objNode != null)
                                    {
                                        TreeNode objParent = objNode.Parent;
                                        objNode.Remove();
                                        if (objParent.Nodes.Count == 0)
                                            objParent.Remove();
                                    }
                                }
                            }
                            break;

                        case NotifyCollectionChangedAction.Replace:
                            {
                                List<TreeNode> lstOldParents =
                                    new List<TreeNode>(notifyCollectionChangedEventArgs.OldItems.Count);
                                foreach (MartialArt objMartialArt in notifyCollectionChangedEventArgs.OldItems)
                                {
                                    objMartialArt.Techniques.RemoveTaggedCollectionChanged(treMartialArts);

                                    TreeNode objNode = treMartialArts.FindNodeByTag(objMartialArt);
                                    if (objNode != null)
                                    {
                                        lstOldParents.Add(objNode.Parent);
                                        objNode.Remove();
                                    }
                                }

                                foreach (MartialArt objMartialArt in notifyCollectionChangedEventArgs.NewItems)
                                {
                                    AddToTree(objMartialArt);
                                    objMartialArt.Techniques.AddTaggedCollectionChanged(treMartialArts, MakeDirtyWithCharacterUpdate);
                                    objMartialArt.Techniques.AddTaggedCollectionChanged(treMartialArts,
                                        (x, y) => RefreshMartialArtTechniques(treMartialArts, objMartialArt, cmsTechnique,
                                            y));
                                }

                                foreach (TreeNode objOldParent in lstOldParents)
                                {
                                    if (objOldParent.Nodes.Count == 0)
                                        objOldParent.Remove();
                                }
                            }
                            break;
                    }
                }

                void AddToTree(MartialArt objMartialArt, bool blnSingleAdd = true)
                {
                    TreeNode objNode = objMartialArt.CreateTreeNode(cmsMartialArts, cmsTechnique);
                    if (objNode == null)
                        return;

                    TreeNode objParentNode;
                    if (objMartialArt.IsQuality)
                    {
                        if (objQualityNode == null)
                        {
                            objQualityNode = new TreeNode
                            {
                                Tag = "Node_SelectedQualities",
                                Text = LanguageManager.GetString("Node_SelectedQualities")
                            };
                            treMartialArts.Nodes.Add(objQualityNode);
                            objQualityNode.Expand();
                        }

                        objParentNode = objQualityNode;
                    }
                    else
                    {
                        if (objMartialArtsParentNode == null)
                        {
                            objMartialArtsParentNode = new TreeNode
                            {
                                Tag = "Node_SelectedMartialArts",
                                Text = LanguageManager.GetString("Node_SelectedMartialArts")
                            };
                            treMartialArts.Nodes.Insert(0, objMartialArtsParentNode);
                            objMartialArtsParentNode.Expand();
                        }

                        objParentNode = objMartialArtsParentNode;
                    }

                    if (blnSingleAdd)
                    {
                        TreeNodeCollection lstParentNodeChildren = objParentNode.Nodes;
                        int intNodesCount = lstParentNodeChildren.Count;
                        int intTargetIndex = 0;
                        for (; intTargetIndex < intNodesCount; ++intTargetIndex)
                        {
                            if (CompareTreeNodes.CompareText(lstParentNodeChildren[intTargetIndex], objNode) >= 0)
                            {
                                break;
                            }
                        }

                        lstParentNodeChildren.Insert(intTargetIndex, objNode);
                        treMartialArts.SelectedNode = objNode;
                    }
                    else
                        objParentNode.Nodes.Add(objNode);

                    objParentNode.Expand();
                }
            }
        }

        protected void RefreshMartialArtTechniques(TreeView treMartialArts, MartialArt objMartialArt, ContextMenuStrip cmsTechnique, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            if (treMartialArts == null || objMartialArt == null || notifyCollectionChangedEventArgs == null)
                return;
            TreeNode nodMartialArt = treMartialArts.FindNodeByTag(objMartialArt);
            if (nodMartialArt == null)
                return;
            using (CursorWait.New(this))
            {
                switch (notifyCollectionChangedEventArgs.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        {
                            foreach (MartialArtTechnique objTechnique in notifyCollectionChangedEventArgs.NewItems)
                            {
                                AddToTree(objTechnique);
                            }
                        }
                        break;

                    case NotifyCollectionChangedAction.Remove:
                        {
                            foreach (MartialArtTechnique objTechnique in notifyCollectionChangedEventArgs.OldItems)
                            {
                                nodMartialArt.FindNodeByTag(objTechnique)?.Remove();
                            }
                        }
                        break;

                    case NotifyCollectionChangedAction.Replace:
                        {
                            foreach (MartialArtTechnique objTechnique in notifyCollectionChangedEventArgs.OldItems)
                            {
                                nodMartialArt.FindNodeByTag(objTechnique)?.Remove();
                            }

                            foreach (MartialArtTechnique objTechnique in notifyCollectionChangedEventArgs.NewItems)
                            {
                                AddToTree(objTechnique);
                            }
                        }
                        break;

                    case NotifyCollectionChangedAction.Reset:
                        {
                            string strSelectedId = (treMartialArts.SelectedNode?.Tag as IHasInternalId)?.InternalId ??
                                                   string.Empty;

                            nodMartialArt.Nodes.Clear();

                            foreach (MartialArtTechnique objTechnique in objMartialArt.Techniques)
                            {
                                AddToTree(objTechnique, false);
                            }

                            treMartialArts.SortCustomAlphabetically(strSelectedId);
                        }
                        break;
                }

                void AddToTree(MartialArtTechnique objTechnique, bool blnSingleAdd = true)
                {
                    TreeNode objNode = objTechnique.CreateTreeNode(cmsTechnique);
                    if (objNode == null)
                        return;

                    if (blnSingleAdd)
                    {
                        TreeNodeCollection lstParentNodeChildren = nodMartialArt.Nodes;
                        int intNodesCount = lstParentNodeChildren.Count;
                        int intTargetIndex = 0;
                        for (; intTargetIndex < intNodesCount; ++intTargetIndex)
                        {
                            if (CompareTreeNodes.CompareText(lstParentNodeChildren[intTargetIndex], objNode) >= 0)
                            {
                                break;
                            }
                        }

                        lstParentNodeChildren.Insert(intTargetIndex, objNode);
                        treMartialArts.SelectedNode = objNode;
                    }
                    else
                        nodMartialArt.Nodes.Add(objNode);

                    nodMartialArt.Expand();
                }
            }
        }

        /// <summary>
        /// Refresh the list of Improvements.
        /// </summary>
        protected void RefreshCustomImprovements(TreeView treImprovements, TreeView treLimit, ContextMenuStrip cmsImprovementLocation, ContextMenuStrip cmsImprovement, ContextMenuStrip cmsLimitModifier, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs = null)
        {
            if (treImprovements == null)
                return;
            using (CursorWait.New(this))
            {
                string strSelectedId =
                    (treImprovements.SelectedNode?.Tag as IHasInternalId)?.InternalId ?? string.Empty;

                TreeNode objRoot;

                if (notifyCollectionChangedEventArgs == null ||
                    notifyCollectionChangedEventArgs.Action == NotifyCollectionChangedAction.Reset)
                {
                    treImprovements.SuspendLayout();
                    treImprovements.Nodes.Clear();

                    objRoot = new TreeNode
                    {
                        Tag = "Node_SelectedImprovements",
                        Text = LanguageManager.GetString("Node_SelectedImprovements")
                    };
                    treImprovements.Nodes.Add(objRoot);

                    // Add the Locations.
                    foreach (string strGroup in CharacterObject.ImprovementGroups)
                    {
                        TreeNode objGroup = new TreeNode
                        {
                            Tag = strGroup,
                            Text = strGroup,
                            ContextMenuStrip = cmsImprovementLocation
                        };
                        treImprovements.Nodes.Add(objGroup);
                    }

                    foreach (Improvement objImprovement in CharacterObject.Improvements)
                    {
                        if (objImprovement.ImproveSource == Improvement.ImprovementSource.Custom ||
                            objImprovement.ImproveSource == Improvement.ImprovementSource.Drug)
                        {
                            AddToTree(objImprovement, false);
                        }
                    }

                    // Sort the list of Custom Improvements in alphabetical order based on their Custom Name within each Group.
                    treImprovements.SortCustomAlphabetically(strSelectedId);
                    treImprovements.ResumeLayout();
                }
                else
                {
                    objRoot = treImprovements.FindNode("Node_SelectedImprovements", false);
                    TreeNode[] aobjLimitNodes =
                    {
                        treLimit?.FindNode("Node_Physical", false),
                        treLimit?.FindNode("Node_Mental", false),
                        treLimit?.FindNode("Node_Social", false),
                        treLimit?.FindNode("Node_Astral", false)
                    };

                    switch (notifyCollectionChangedEventArgs.Action)
                    {
                        case NotifyCollectionChangedAction.Add:
                            {
                                foreach (Improvement objImprovement in notifyCollectionChangedEventArgs.NewItems)
                                {
                                    if (objImprovement.ImproveSource == Improvement.ImprovementSource.Custom ||
                                        objImprovement.ImproveSource == Improvement.ImprovementSource.Drug)
                                    {
                                        AddToTree(objImprovement);
                                        AddToLimitTree(objImprovement);
                                    }
                                }

                                break;
                            }
                        case NotifyCollectionChangedAction.Remove:
                            {
                                foreach (Improvement objImprovement in notifyCollectionChangedEventArgs.OldItems)
                                {
                                    if (objImprovement.ImproveSource == Improvement.ImprovementSource.Custom ||
                                        objImprovement.ImproveSource == Improvement.ImprovementSource.Drug)
                                    {
                                        TreeNode objNode = treImprovements.FindNodeByTag(objImprovement);
                                        if (objNode != null)
                                        {
                                            TreeNode objParent = objNode.Parent;
                                            objNode.Remove();
                                            if (objParent.Tag.ToString() == "Node_SelectedImprovements" &&
                                                objParent.Nodes.Count == 0)
                                                objParent.Remove();
                                        }

                                        objNode = treLimit?.FindNodeByTag(objImprovement);
                                        if (objNode != null)
                                        {
                                            TreeNode objParent = objNode.Parent;
                                            objNode.Remove();
                                            if (objParent.Level == 0 && objParent.Nodes.Count == 0)
                                                objParent.Remove();
                                        }
                                    }
                                }

                                break;
                            }
                        case NotifyCollectionChangedAction.Replace:
                            {
                                List<TreeNode> lstOldParents =
                                    new List<TreeNode>(notifyCollectionChangedEventArgs.OldItems.Count);
                                foreach (Improvement objImprovement in notifyCollectionChangedEventArgs.OldItems)
                                {
                                    if (objImprovement.ImproveSource == Improvement.ImprovementSource.Custom ||
                                        objImprovement.ImproveSource == Improvement.ImprovementSource.Drug)
                                    {
                                        TreeNode objNode = treImprovements.FindNodeByTag(objImprovement);
                                        if (objNode != null)
                                        {
                                            lstOldParents.Add(objNode.Parent);
                                            objNode.Remove();
                                        }

                                        objNode = treLimit?.FindNodeByTag(objImprovement);
                                        if (objNode != null)
                                        {
                                            lstOldParents.Add(objNode.Parent);
                                            objNode.Remove();
                                        }
                                    }
                                }

                                foreach (Improvement objImprovement in notifyCollectionChangedEventArgs.NewItems)
                                {
                                    if (objImprovement.ImproveSource == Improvement.ImprovementSource.Custom ||
                                        objImprovement.ImproveSource == Improvement.ImprovementSource.Drug)
                                    {
                                        AddToTree(objImprovement);
                                        AddToLimitTree(objImprovement);
                                    }
                                }

                                foreach (TreeNode objOldParent in lstOldParents)
                                {
                                    if (objOldParent.Level == 0 && objOldParent.Nodes.Count == 0)
                                        objOldParent.Remove();
                                }

                                break;
                            }
                    }

                    void AddToLimitTree(Improvement objImprovement)
                    {
                        if (treLimit == null)
                            return;
                        int intTargetLimit = -1;
                        switch (objImprovement.ImproveType)
                        {
                            case Improvement.ImprovementType.LimitModifier:
                                intTargetLimit = (int)Enum.Parse(typeof(LimitType), objImprovement.ImprovedName);
                                break;

                            case Improvement.ImprovementType.PhysicalLimit:
                                intTargetLimit = (int)LimitType.Physical;
                                break;

                            case Improvement.ImprovementType.MentalLimit:
                                intTargetLimit = (int)LimitType.Mental;
                                break;

                            case Improvement.ImprovementType.SocialLimit:
                                intTargetLimit = (int)LimitType.Social;
                                break;
                        }

                        if (intTargetLimit != -1)
                        {
                            TreeNode objParentNode = aobjLimitNodes[intTargetLimit];
                            if (objParentNode == null)
                            {
                                switch (intTargetLimit)
                                {
                                    case 0:
                                        objParentNode = new TreeNode
                                        {
                                            Tag = "Node_Physical",
                                            Text = LanguageManager.GetString("Node_Physical")
                                        };
                                        treLimit.Nodes.Insert(0, objParentNode);
                                        break;

                                    case 1:
                                        objParentNode = new TreeNode
                                        {
                                            Tag = "Node_Mental",
                                            Text = LanguageManager.GetString("Node_Mental")
                                        };
                                        treLimit.Nodes.Insert(aobjLimitNodes[0] == null ? 0 : 1, objParentNode);
                                        break;

                                    case 2:
                                        objParentNode = new TreeNode
                                        {
                                            Tag = "Node_Social",
                                            Text = LanguageManager.GetString("Node_Social")
                                        };
                                        treLimit.Nodes.Insert(
                                            (aobjLimitNodes[0] == null ? 0 : 1) + (aobjLimitNodes[1] == null ? 0 : 1),
                                            objParentNode);
                                        break;

                                    case 3:
                                        objParentNode = new TreeNode
                                        {
                                            Tag = "Node_Astral",
                                            Text = LanguageManager.GetString("Node_Astral")
                                        };
                                        treLimit.Nodes.Add(objParentNode);
                                        break;
                                }

                                objParentNode?.Expand();
                            }

                            string strName = objImprovement.UniqueName + LanguageManager.GetString("String_Colon") +
                                             LanguageManager.GetString("String_Space");
                            if (objImprovement.Value > 0)
                                strName += '+';
                            strName += objImprovement.Value.ToString(GlobalSettings.CultureInfo);
                            if (!string.IsNullOrEmpty(objImprovement.Condition))
                                strName += ',' + LanguageManager.GetString("String_Space") + objImprovement.Condition;
                            if (objParentNode?.Nodes.ContainsKey(strName) == false)
                            {
                                TreeNode objNode = new TreeNode
                                {
                                    Name = strName,
                                    Text = strName,
                                    Tag = objImprovement.SourceName,
                                    ContextMenuStrip = cmsLimitModifier,
                                    ForeColor = objImprovement.PreferredColor,
                                    ToolTipText = objImprovement.Notes.WordWrap()
                                };
                                if (string.IsNullOrEmpty(objImprovement.ImprovedName))
                                {
                                    switch (objImprovement.ImproveType)
                                    {
                                        case Improvement.ImprovementType.SocialLimit:
                                            objImprovement.ImprovedName = "Social";
                                            break;

                                        case Improvement.ImprovementType.MentalLimit:
                                            objImprovement.ImprovedName = "Mental";
                                            break;

                                        default:
                                            objImprovement.ImprovedName = "Physical";
                                            break;
                                    }
                                }

                                TreeNodeCollection lstParentNodeChildren = objParentNode.Nodes;
                                int intNodesCount = lstParentNodeChildren.Count;
                                int intTargetIndex = 0;
                                for (; intTargetIndex < intNodesCount; ++intTargetIndex)
                                {
                                    if (CompareTreeNodes.CompareText(lstParentNodeChildren[intTargetIndex], objNode) >=
                                        0)
                                    {
                                        break;
                                    }
                                }

                                lstParentNodeChildren.Insert(intTargetIndex, objNode);
                                treLimit.SelectedNode = objNode;
                            }
                        }
                    }
                }

                void AddToTree(Improvement objImprovement, bool blnSingleAdd = true)
                {
                    TreeNode objNode = objImprovement.CreateTreeNode(cmsImprovement);

                    TreeNode objParentNode = objRoot;
                    if (!string.IsNullOrEmpty(objImprovement.CustomGroup))
                    {
                        foreach (TreeNode objFind in treImprovements.Nodes)
                        {
                            if (objFind.Text == objImprovement.CustomGroup)
                            {
                                objParentNode = objFind;
                                break;
                            }
                        }
                    }
                    else
                    {
                        if (objParentNode == null)
                        {
                            objParentNode = new TreeNode
                            {
                                Tag = "Node_SelectedImprovements",
                                Text = LanguageManager.GetString("Node_SelectedImprovements")
                            };
                            treImprovements.Nodes.Add(objParentNode);
                        }
                    }

                    if (blnSingleAdd)
                    {
                        TreeNodeCollection lstParentNodeChildren = objParentNode.Nodes;
                        int intNodesCount = lstParentNodeChildren.Count;
                        int intTargetIndex = 0;
                        for (; intTargetIndex < intNodesCount; ++intTargetIndex)
                        {
                            if (CompareTreeNodes.CompareText(lstParentNodeChildren[intTargetIndex], objNode) >= 0)
                            {
                                break;
                            }
                        }

                        lstParentNodeChildren.Insert(intTargetIndex, objNode);
                        treImprovements.SelectedNode = objNode;
                    }
                    else
                        objParentNode.Nodes.Add(objNode);

                    objParentNode.Expand();
                }
            }
        }

        protected void RefreshLifestyles(TreeView treLifestyles, ContextMenuStrip cmsBasicLifestyle,
                                         ContextMenuStrip cmsAdvancedLifestyle, NotifyCollectionChangedEventArgs e = null)
        {
            if (treLifestyles == null)
                return;
            using (CursorWait.New(this))
            {
                string strSelectedId = (treLifestyles.SelectedNode?.Tag as IHasInternalId)?.InternalId ?? string.Empty;
                TreeNode objParentNode = null;

                if (e == null || e.Action == NotifyCollectionChangedAction.Reset)
                {
                    treLifestyles.SuspendLayout();
                    treLifestyles.Nodes.Clear();

                    if (CharacterObject.Lifestyles.Count > 0)
                    {
                        foreach (Lifestyle objLifestyle in CharacterObject.Lifestyles)
                        {
                            AddToTree(objLifestyle, false);
                        }

                        treLifestyles.SortCustomAlphabetically(strSelectedId);
                    }

                    treLifestyles.ResumeLayout();
                }
                else
                {
                    objParentNode = treLifestyles.FindNode("Node_SelectedLifestyles", false);
                    switch (e.Action)
                    {
                        case NotifyCollectionChangedAction.Add:
                            {
                                foreach (Lifestyle objLifestyle in e.NewItems)
                                {
                                    AddToTree(objLifestyle);
                                }

                                break;
                            }
                        case NotifyCollectionChangedAction.Remove:
                        {
                            foreach (Lifestyle objLifestyle in e.OldItems)
                            {
                                TreeNode objNode = treLifestyles.FindNodeByTag(objLifestyle);
                                if (objNode != null)
                                {
                                    TreeNode objParent = objNode.Parent;
                                    objNode.Remove();
                                    if (objParent.Level == 0 && objParent.Nodes.Count == 0)
                                        objParent.Remove();
                                }
                            }

                            break;
                        }
                        case NotifyCollectionChangedAction.Replace:
                        {
                            HashSet<TreeNode> setOldParentNodes = new HashSet<TreeNode>();
                            foreach (Lifestyle objLifestyle in e.OldItems)
                            {
                                TreeNode objNode = treLifestyles.FindNodeByTag(objLifestyle);
                                if (objNode != null)
                                {
                                    setOldParentNodes.Add(objNode.Parent);
                                    objNode.Remove();
                                }
                            }

                            foreach (Lifestyle objLifestyle in e.NewItems)
                            {
                                AddToTree(objLifestyle);
                            }

                            foreach (TreeNode nodOldParent in setOldParentNodes)
                            {
                                if (nodOldParent.Level == 0 && nodOldParent.Nodes.Count == 0)
                                    nodOldParent.Remove();
                            }
                            
                            break;
                        }
                    }
                }

                void AddToTree(Lifestyle objLifestyle, bool blnSingleAdd = true)
                {
                    TreeNode objNode = objLifestyle.CreateTreeNode(cmsBasicLifestyle, cmsAdvancedLifestyle);
                    if (objNode == null)
                        return;

                    if (objParentNode == null)
                    {
                        objParentNode = new TreeNode
                        {
                            Tag = "Node_SelectedLifestyles",
                            Text = LanguageManager.GetString("Node_SelectedLifestyles")
                        };
                        treLifestyles.Nodes.Add(objParentNode);
                        objParentNode.Expand();
                    }

                    if (blnSingleAdd)
                    {
                        TreeNodeCollection lstParentNodeChildren = objParentNode.Nodes;
                        int intNodesCount = lstParentNodeChildren.Count;
                        int intTargetIndex = 0;
                        for (; intTargetIndex < intNodesCount; ++intTargetIndex)
                        {
                            if (CompareTreeNodes.CompareText(lstParentNodeChildren[intTargetIndex], objNode) >= 0)
                            {
                                break;
                            }
                        }

                        lstParentNodeChildren.Insert(intTargetIndex, objNode);
                        treLifestyles.SelectedNode = objNode;
                    }
                    else
                        objParentNode.Nodes.Add(objNode);
                }
            }
        }

        /// <summary>
        /// Refresh the Calendar List.
        /// </summary>
        public void RefreshCalendar(ListView lstCalendar, ListChangedEventArgs listChangedEventArgs = null)
        {
            if (lstCalendar == null)
                return;
            using (CursorWait.New(this))
            {
                if (listChangedEventArgs == null || listChangedEventArgs.ListChangedType == ListChangedType.Reset)
                {
                    lstCalendar.SuspendLayout();
                    lstCalendar.Items.Clear();
                    foreach (CalendarWeek objWeek in CharacterObject.Calendar)
                    {
                        ListViewItem.ListViewSubItem objNoteItem = new ListViewItem.ListViewSubItem
                        {
                            Text = objWeek.Notes,
                            ForeColor = objWeek.PreferredColor
                        };
                        ListViewItem.ListViewSubItem objInternalIdItem = new ListViewItem.ListViewSubItem
                        {
                            Text = objWeek.InternalId,
                            ForeColor = objWeek.PreferredColor
                        };

                        ListViewItem objItem = new ListViewItem
                        {
                            Text = objWeek.CurrentDisplayName,
                            ForeColor = objWeek.PreferredColor
                        };
                        objItem.SubItems.Add(objNoteItem);
                        objItem.SubItems.Add(objInternalIdItem);

                        lstCalendar.Items.Add(objItem);
                    }

                    lstCalendar.ResumeLayout();
                }
                else
                {
                    switch (listChangedEventArgs.ListChangedType)
                    {
                        case ListChangedType.ItemAdded:
                            {
                                int intInsertIndex = listChangedEventArgs.NewIndex;
                                CalendarWeek objWeek = CharacterObject.Calendar[intInsertIndex];

                                ListViewItem.ListViewSubItem objNoteItem = new ListViewItem.ListViewSubItem
                                {
                                    Text = objWeek.Notes,
                                    ForeColor = objWeek.PreferredColor
                                };
                                ListViewItem.ListViewSubItem objInternalIdItem = new ListViewItem.ListViewSubItem
                                {
                                    Text = objWeek.InternalId,
                                    ForeColor = objWeek.PreferredColor
                                };

                                ListViewItem objItem = new ListViewItem
                                {
                                    Text = objWeek.CurrentDisplayName,
                                    ForeColor = objWeek.PreferredColor
                                };
                                objItem.SubItems.Add(objNoteItem);
                                objItem.SubItems.Add(objInternalIdItem);

                                lstCalendar.Items.Insert(intInsertIndex, objItem);
                            }
                            break;

                        case ListChangedType.ItemDeleted:
                            {
                                lstCalendar.Items.RemoveAt(listChangedEventArgs.NewIndex);
                            }
                            break;

                        case ListChangedType.ItemChanged:
                            {
                                lstCalendar.Items.RemoveAt(listChangedEventArgs.NewIndex);
                                int intInsertIndex = listChangedEventArgs.NewIndex;
                                CalendarWeek objWeek = CharacterObject.Calendar[intInsertIndex];

                                ListViewItem.ListViewSubItem objNoteItem = new ListViewItem.ListViewSubItem
                                {
                                    Text = objWeek.Notes,
                                    ForeColor = objWeek.PreferredColor
                                };
                                ListViewItem.ListViewSubItem objInternalIdItem = new ListViewItem.ListViewSubItem
                                {
                                    Text = objWeek.InternalId,
                                    ForeColor = objWeek.PreferredColor
                                };

                                ListViewItem objItem = new ListViewItem
                                {
                                    Text = objWeek.CurrentDisplayName,
                                    ForeColor = objWeek.PreferredColor
                                };
                                objItem.SubItems.Add(objNoteItem);
                                objItem.SubItems.Add(objInternalIdItem);

                                lstCalendar.Items.Insert(intInsertIndex, objItem);
                            }
                            break;

                        case ListChangedType.ItemMoved:
                            {
                                lstCalendar.Items.RemoveAt(listChangedEventArgs.OldIndex);
                                int intInsertIndex = listChangedEventArgs.NewIndex;
                                CalendarWeek objWeek = CharacterObject.Calendar[intInsertIndex];

                                ListViewItem.ListViewSubItem objNoteItem = new ListViewItem.ListViewSubItem
                                {
                                    Text = objWeek.Notes,
                                    ForeColor = objWeek.PreferredColor
                                };
                                ListViewItem.ListViewSubItem objInternalIdItem = new ListViewItem.ListViewSubItem
                                {
                                    Text = objWeek.InternalId,
                                    ForeColor = objWeek.PreferredColor
                                };

                                ListViewItem objItem = new ListViewItem
                                {
                                    Text = objWeek.CurrentDisplayName,
                                    ForeColor = objWeek.PreferredColor
                                };
                                objItem.SubItems.Add(objNoteItem);
                                objItem.SubItems.Add(objInternalIdItem);

                                lstCalendar.Items.Insert(intInsertIndex, objItem);
                            }
                            break;
                    }
                }
            }
        }

        public void RefreshContacts(FlowLayoutPanel panContacts, FlowLayoutPanel panEnemies, FlowLayoutPanel panPets, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs = null)
        {
            if (panContacts == null && panEnemies == null && panPets == null)
                return;
            using (CursorWait.New(this))
            {
                if (notifyCollectionChangedEventArgs == null ||
                    notifyCollectionChangedEventArgs.Action == NotifyCollectionChangedAction.Reset)
                {
                    panContacts?.SuspendLayout();
                    panEnemies?.SuspendLayout();
                    panPets?.SuspendLayout();
                    panContacts?.Controls.Clear();
                    panEnemies?.Controls.Clear();
                    panPets?.Controls.Clear();
                    foreach (Contact objContact in CharacterObject.Contacts)
                    {
                        switch (objContact.EntityType)
                        {
                            case ContactType.Contact:
                                {
                                    if (panContacts == null)
                                        break;
                                    ContactControl objContactControl = new ContactControl(objContact);
                                    // Attach an EventHandler for the ConnectionRatingChanged, LoyaltyRatingChanged, DeleteContact, FileNameChanged Events and OtherCostChanged
                                    objContactControl.ContactDetailChanged += MakeDirtyWithCharacterUpdate;
                                    objContactControl.DeleteContact += DeleteContact;
                                    objContactControl.MouseDown += DragContactControl;

                                    panContacts.Controls.Add(objContactControl);
                                }
                                break;

                            case ContactType.Enemy:
                                {
                                    if (panEnemies == null || !CharacterObjectSettings.EnableEnemyTracking)
                                        break;
                                    ContactControl objContactControl = new ContactControl(objContact);
                                    // Attach an EventHandler for the ConnectionRatingChanged, LoyaltyRatingChanged, DeleteContact, FileNameChanged Events and OtherCostChanged
                                    objContactControl.ContactDetailChanged += MakeDirtyWithCharacterUpdate;
                                    objContactControl.DeleteContact += DeleteEnemy;
                                    objContactControl.MouseDown += DragContactControl;

                                    panEnemies.Controls.Add(objContactControl);
                                }
                                break;

                            case ContactType.Pet:
                                {
                                    if (panPets == null)
                                        break;
                                    PetControl objContactControl = new PetControl(objContact);
                                    // Attach an EventHandler for the ConnectionRatingChanged, LoyaltyRatingChanged, DeleteContact, FileNameChanged Events and OtherCostChanged
                                    objContactControl.ContactDetailChanged += MakeDirtyWithCharacterUpdate;
                                    objContactControl.DeleteContact += DeletePet;
                                    objContactControl.MouseDown += DragContactControl;

                                    panPets.Controls.Add(objContactControl);
                                }
                                break;
                        }
                    }

                    panContacts?.ResumeLayout();
                    panEnemies?.ResumeLayout();
                    panPets?.ResumeLayout();
                }
                else
                {
                    switch (notifyCollectionChangedEventArgs.Action)
                    {
                        case NotifyCollectionChangedAction.Add:
                            {
                                foreach (Contact objLoopContact in notifyCollectionChangedEventArgs.NewItems)
                                {
                                    switch (objLoopContact.EntityType)
                                    {
                                        case ContactType.Contact:
                                            {
                                                if (panContacts == null)
                                                    break;
                                                ContactControl objContactControl = new ContactControl(objLoopContact);
                                                // Attach an EventHandler for the ConnectionRatingChanged, LoyaltyRatingChanged, DeleteContact, FileNameChanged Events and OtherCostChanged
                                                objContactControl.ContactDetailChanged += MakeDirtyWithCharacterUpdate;
                                                objContactControl.DeleteContact += DeleteContact;
                                                objContactControl.MouseDown += DragContactControl;

                                                panContacts.Controls.Add(objContactControl);
                                            }
                                            break;

                                        case ContactType.Enemy:
                                            {
                                                if (panEnemies == null || !CharacterObjectSettings.EnableEnemyTracking)
                                                    break;
                                                ContactControl objContactControl = new ContactControl(objLoopContact);
                                                // Attach an EventHandler for the ConnectionRatingChanged, LoyaltyRatingChanged, DeleteContact, FileNameChanged Events and OtherCostChanged
                                                objContactControl.ContactDetailChanged += MakeDirtyWithCharacterUpdate;
                                                objContactControl.DeleteContact += DeleteEnemy;
                                                //objContactControl.MouseDown += DragContactControl;

                                                panEnemies.Controls.Add(objContactControl);
                                            }
                                            break;

                                        case ContactType.Pet:
                                            {
                                                if (panPets == null)
                                                    break;
                                                PetControl objPetControl = new PetControl(objLoopContact);
                                                // Attach an EventHandler for the ConnectionRatingChanged, LoyaltyRatingChanged, DeleteContact, FileNameChanged Events and OtherCostChanged
                                                objPetControl.ContactDetailChanged += MakeDirtyWithCharacterUpdate;
                                                objPetControl.DeleteContact += DeletePet;
                                                //objPetControl.MouseDown += DragContactControl;

                                                panPets.Controls.Add(objPetControl);
                                            }
                                            break;
                                    }
                                }
                            }
                            break;

                        case NotifyCollectionChangedAction.Remove:
                            {
                                foreach (Contact objLoopContact in notifyCollectionChangedEventArgs.OldItems)
                                {
                                    switch (objLoopContact.EntityType)
                                    {
                                        case ContactType.Contact:
                                            {
                                                if (panContacts == null)
                                                    break;
                                                for (int i = panContacts.Controls.Count - 1; i >= 0; i--)
                                                {
                                                    if (panContacts.Controls[i] is ContactControl objContactControl &&
                                                        objContactControl.ContactObject == objLoopContact)
                                                    {
                                                        panContacts.Controls.RemoveAt(i);
                                                        objContactControl.ContactDetailChanged -= MakeDirtyWithCharacterUpdate;
                                                        objContactControl.DeleteContact -= DeleteContact;
                                                        objContactControl.MouseDown -= DragContactControl;
                                                        objContactControl.Dispose();
                                                    }
                                                }
                                            }
                                            break;

                                        case ContactType.Enemy:
                                            {
                                                if (panEnemies == null)
                                                    break;
                                                for (int i = panEnemies.Controls.Count - 1; i >= 0; i--)
                                                {
                                                    if (panEnemies.Controls[i] is ContactControl objContactControl &&
                                                        objContactControl.ContactObject == objLoopContact)
                                                    {
                                                        panEnemies.Controls.RemoveAt(i);
                                                        objContactControl.ContactDetailChanged -= MakeDirtyWithCharacterUpdate;
                                                        objContactControl.DeleteContact -= DeleteEnemy;
                                                        objContactControl.Dispose();
                                                    }
                                                }
                                            }
                                            break;

                                        case ContactType.Pet:
                                            {
                                                if (panPets == null)
                                                    break;
                                                for (int i = panPets.Controls.Count - 1; i >= 0; i--)
                                                {
                                                    if (panPets.Controls[i] is PetControl objPetControl &&
                                                        objPetControl.ContactObject == objLoopContact)
                                                    {
                                                        panPets.Controls.RemoveAt(i);
                                                        objPetControl.ContactDetailChanged -= MakeDirtyWithCharacterUpdate;
                                                        objPetControl.DeleteContact -= DeletePet;
                                                        objPetControl.Dispose();
                                                    }
                                                }
                                            }
                                            break;
                                    }
                                }
                            }
                            break;

                        case NotifyCollectionChangedAction.Replace:
                            {
                                foreach (Contact objLoopContact in notifyCollectionChangedEventArgs.OldItems)
                                {
                                    switch (objLoopContact.EntityType)
                                    {
                                        case ContactType.Contact:
                                            {
                                                if (panContacts == null)
                                                    break;
                                                for (int i = panContacts.Controls.Count - 1; i >= 0; i--)
                                                {
                                                    if (panContacts.Controls[i] is ContactControl objContactControl &&
                                                        objContactControl.ContactObject == objLoopContact)
                                                    {
                                                        panContacts.Controls.RemoveAt(i);
                                                        objContactControl.ContactDetailChanged -= MakeDirtyWithCharacterUpdate;
                                                        objContactControl.DeleteContact -= DeleteContact;
                                                        objContactControl.MouseDown -= DragContactControl;
                                                        objContactControl.Dispose();
                                                    }
                                                }
                                            }
                                            break;

                                        case ContactType.Enemy:
                                            {
                                                if (panEnemies == null)
                                                    break;
                                                for (int i = panEnemies.Controls.Count - 1; i >= 0; i--)
                                                {
                                                    if (panEnemies.Controls[i] is ContactControl objContactControl &&
                                                        objContactControl.ContactObject == objLoopContact)
                                                    {
                                                        panEnemies.Controls.RemoveAt(i);
                                                        objContactControl.ContactDetailChanged -= MakeDirtyWithCharacterUpdate;
                                                        objContactControl.DeleteContact -= DeleteEnemy;
                                                        objContactControl.Dispose();
                                                    }
                                                }
                                            }
                                            break;

                                        case ContactType.Pet:
                                            {
                                                if (panPets == null)
                                                    break;
                                                for (int i = panPets.Controls.Count - 1; i >= 0; i--)
                                                {
                                                    if (panPets.Controls[i] is PetControl objPetControl &&
                                                        objPetControl.ContactObject == objLoopContact)
                                                    {
                                                        panPets.Controls.RemoveAt(i);
                                                        objPetControl.ContactDetailChanged -= MakeDirtyWithCharacterUpdate;
                                                        objPetControl.DeleteContact -= DeletePet;
                                                        objPetControl.Dispose();
                                                    }
                                                }
                                            }
                                            break;
                                    }
                                }

                                foreach (Contact objLoopContact in notifyCollectionChangedEventArgs.NewItems)
                                {
                                    switch (objLoopContact.EntityType)
                                    {
                                        case ContactType.Contact:
                                            {
                                                if (panContacts == null)
                                                    break;
                                                ContactControl objContactControl = new ContactControl(objLoopContact);
                                                // Attach an EventHandler for the ConnectionRatingChanged, LoyaltyRatingChanged, DeleteContact, FileNameChanged Events and OtherCostChanged
                                                objContactControl.ContactDetailChanged += MakeDirtyWithCharacterUpdate;
                                                objContactControl.DeleteContact += DeleteContact;
                                                objContactControl.MouseDown += DragContactControl;

                                                panContacts.Controls.Add(objContactControl);
                                            }
                                            break;

                                        case ContactType.Enemy:
                                            {
                                                if (panEnemies == null || !CharacterObjectSettings.EnableEnemyTracking)
                                                    break;
                                                ContactControl objContactControl = new ContactControl(objLoopContact);
                                                // Attach an EventHandler for the ConnectionRatingChanged, LoyaltyRatingChanged, DeleteContact, FileNameChanged Events and OtherCostChanged
                                                objContactControl.ContactDetailChanged += MakeDirtyWithCharacterUpdate;
                                                objContactControl.DeleteContact += DeleteEnemy;
                                                //objContactControl.MouseDown += DragContactControl;

                                                panEnemies.Controls.Add(objContactControl);
                                            }
                                            break;

                                        case ContactType.Pet:
                                            {
                                                if (panPets == null)
                                                    break;
                                                PetControl objPetControl = new PetControl(objLoopContact);
                                                // Attach an EventHandler for the ConnectionRatingChanged, LoyaltyRatingChanged, DeleteContact, FileNameChanged Events and OtherCostChanged
                                                objPetControl.ContactDetailChanged += MakeDirtyWithCharacterUpdate;
                                                objPetControl.DeleteContact += DeletePet;
                                                //objPetControl.MouseDown += DragContactControl;

                                                panPets.Controls.Add(objPetControl);
                                            }
                                            break;
                                    }
                                }
                            }
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Refreshes the all panels for sustained objects (spells, complex forms, critter powers)
        /// </summary>
        /// <param name="pnlSustainedSpells">Panel for sustained spells.</param>
        /// <param name="pnlSustainedComplexForms">Panel for sustained complex forms.</param>
        /// <param name="pnlSustainedCritterPowers">Panel for sustained critter powers.</param>
        /// <param name="chkPsycheActiveMagician">Checkbox for Psyche in the tab for spells.</param>
        /// <param name="chkPsycheActiveTechnomancer">Checkbox for Psyche in the tab for complex forms.</param>
        /// <param name="notifyCollectionChangedEventArgs"></param>
        public void RefreshSustainedSpells(Panel pnlSustainedSpells, Panel pnlSustainedComplexForms, Panel pnlSustainedCritterPowers, CheckBox chkPsycheActiveMagician, CheckBox chkPsycheActiveTechnomancer, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs = null)
        {
            if (pnlSustainedSpells == null && pnlSustainedComplexForms == null && pnlSustainedCritterPowers == null)
                return;

            using (CursorWait.New(this))
            {
                Panel DetermineRefreshingPanel(SustainedObject objSustained, Panel flpSustainedSpellsParam,
                                               Panel flpSustainedComplexFormsParam, Panel flpSustainedCritterPowersParam)
                {
                    // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
                    switch (objSustained.LinkedObjectType)
                    {
                        case Improvement.ImprovementSource.Spell:
                            return flpSustainedSpellsParam;

                        case Improvement.ImprovementSource.ComplexForm:
                            return flpSustainedComplexFormsParam;

                        case Improvement.ImprovementSource.CritterPower:
                            return flpSustainedCritterPowersParam;
                    }

                    return null;
                }

                if (notifyCollectionChangedEventArgs == null ||
                    notifyCollectionChangedEventArgs.Action == NotifyCollectionChangedAction.Reset)
                {
                    if (chkPsycheActiveMagician != null)
                        chkPsycheActiveMagician.Visible = false;
                    if (chkPsycheActiveTechnomancer != null)
                        chkPsycheActiveTechnomancer.Visible = false;
                    if (pnlSustainedSpells != null)
                    {
                        pnlSustainedSpells.Controls.Clear();
                        pnlSustainedSpells.Visible = false;
                    }
                    if (pnlSustainedComplexForms != null)
                    {
                        pnlSustainedComplexForms.Controls.Clear();
                        pnlSustainedComplexForms.Visible = false;
                    }
                    if (pnlSustainedCritterPowers != null)
                    {
                        pnlSustainedCritterPowers.Controls.Clear();
                        pnlSustainedCritterPowers.Visible = false;
                    }
                    foreach (SustainedObject objSustained in CharacterObject.SustainedCollection)
                    {
                        Panel refreshingPanel = DetermineRefreshingPanel(objSustained, pnlSustainedSpells,
                            pnlSustainedComplexForms, pnlSustainedCritterPowers);

                        if (refreshingPanel == null)
                            continue;

                        refreshingPanel.Visible = true;
                        switch (objSustained.LinkedObjectType)
                        {
                            case Improvement.ImprovementSource.Spell:
                                if (chkPsycheActiveMagician != null)
                                    chkPsycheActiveMagician.Visible = true;
                                break;

                            case Improvement.ImprovementSource.ComplexForm:
                                if (chkPsycheActiveTechnomancer != null)
                                    chkPsycheActiveTechnomancer.Visible = true;
                                break;
                        }

                        int intSustainedObjects = refreshingPanel.Controls.Count;

                        SustainedObjectControl objSustainedObjectControl = new SustainedObjectControl(objSustained);

                        objSustainedObjectControl.SustainedObjectDetailChanged += MakeDirtyWithCharacterUpdate;
                        objSustainedObjectControl.UnsustainObject += DeleteSustainedObject;

                        objSustainedObjectControl.Top = intSustainedObjects * objSustainedObjectControl.Height;

                        refreshingPanel.Controls.Add(objSustainedObjectControl);
                    }
                }
                else
                {
                    switch (notifyCollectionChangedEventArgs.Action)
                    {
                        case NotifyCollectionChangedAction.Add:
                            {
                                foreach (SustainedObject objSustained in notifyCollectionChangedEventArgs.NewItems)
                                {
                                    Panel refreshingPanel = DetermineRefreshingPanel(objSustained, pnlSustainedSpells,
                                        pnlSustainedComplexForms, pnlSustainedCritterPowers);

                                    if (refreshingPanel == null)
                                        continue;

                                    refreshingPanel.Visible = true;
                                    switch (objSustained.LinkedObjectType)
                                    {
                                        case Improvement.ImprovementSource.Spell:
                                            if (chkPsycheActiveMagician != null)
                                                chkPsycheActiveMagician.Visible = true;
                                            break;

                                        case Improvement.ImprovementSource.ComplexForm:
                                            if (chkPsycheActiveTechnomancer != null)
                                                chkPsycheActiveTechnomancer.Visible = true;
                                            break;
                                    }

                                    int intSustainedObjects = refreshingPanel.Controls.Count;

                                    SustainedObjectControl objSustainedObjectControl =
                                        new SustainedObjectControl(objSustained);

                                    objSustainedObjectControl.SustainedObjectDetailChanged += MakeDirtyWithCharacterUpdate;
                                    objSustainedObjectControl.UnsustainObject += DeleteSustainedObject;

                                    objSustainedObjectControl.Top = intSustainedObjects * objSustainedObjectControl.Height;

                                    refreshingPanel.Controls.Add(objSustainedObjectControl);
                                }
                            }
                            break;

                        case NotifyCollectionChangedAction.Remove:
                            {
                                foreach (SustainedObject objSustained in notifyCollectionChangedEventArgs.OldItems)
                                {
                                    Panel refreshingPanel = DetermineRefreshingPanel(objSustained, pnlSustainedSpells,
                                        pnlSustainedComplexForms, pnlSustainedCritterPowers);

                                    if (refreshingPanel == null)
                                        continue;

                                    int intMoveUpAmount = 0;
                                    int intSustainedObjects = refreshingPanel.Controls.Count;

                                    for (int i = 0; i < intSustainedObjects; ++i)
                                    {
                                        Control objLoopControl = refreshingPanel.Controls[i];
                                        if (objLoopControl is SustainedObjectControl objSustainedSpellControl &&
                                            objSustainedSpellControl.LinkedSustainedObject == objSustained)
                                        {
                                            intMoveUpAmount = objSustainedSpellControl.Height;

                                            refreshingPanel.Controls.RemoveAt(i);

                                            objSustainedSpellControl.SustainedObjectDetailChanged -=
                                                MakeDirtyWithCharacterUpdate;
                                            objSustainedSpellControl.UnsustainObject -= DeleteSustainedObject;
                                            objSustainedSpellControl.Dispose();
                                            --i;
                                            --intSustainedObjects;
                                        }
                                        else if (intMoveUpAmount != 0)
                                        {
                                            objLoopControl.Top -= intMoveUpAmount;
                                        }
                                    }

                                    if (intSustainedObjects == 0)
                                    {
                                        refreshingPanel.Visible = false;
                                        if (refreshingPanel == pnlSustainedSpells)
                                        {
                                            if (chkPsycheActiveMagician != null)
                                                chkPsycheActiveMagician.Visible = false;
                                        }
                                        else if (refreshingPanel == pnlSustainedComplexForms && chkPsycheActiveTechnomancer != null)
                                        {
                                            chkPsycheActiveTechnomancer.Visible = false;
                                        }
                                    }
                                }
                            }
                            break;

                        case NotifyCollectionChangedAction.Replace:
                            {
                                int intSustainedObjects;

                                foreach (SustainedObject objSustained in notifyCollectionChangedEventArgs.OldItems)
                                {
                                    Panel refreshingPanel = DetermineRefreshingPanel(objSustained, pnlSustainedSpells,
                                        pnlSustainedComplexForms, pnlSustainedCritterPowers);

                                    if (refreshingPanel == null)
                                        continue;

                                    int intMoveUpAmount = 0;
                                    intSustainedObjects = refreshingPanel.Controls.Count;

                                    for (int i = 0; i < intSustainedObjects; ++i)
                                    {
                                        Control objLoopControl = refreshingPanel.Controls[i];
                                        if (objLoopControl is SustainedObjectControl objSustainedSpellControl &&
                                            objSustainedSpellControl.LinkedSustainedObject == objSustained)
                                        {
                                            intMoveUpAmount = objSustainedSpellControl.Height;
                                            refreshingPanel.Controls.RemoveAt(i);
                                            objSustainedSpellControl.SustainedObjectDetailChanged -=
                                                MakeDirtyWithCharacterUpdate;
                                            objSustainedSpellControl.UnsustainObject -= DeleteSustainedObject;
                                            objSustainedSpellControl.Dispose();
                                            --i;
                                            --intSustainedObjects;
                                        }
                                        else if (intMoveUpAmount != 0)
                                        {
                                            objLoopControl.Top -= intMoveUpAmount;
                                        }
                                    }

                                    if (intSustainedObjects == 0)
                                    {
                                        refreshingPanel.Visible = false;
                                        if (refreshingPanel == pnlSustainedSpells)
                                        {
                                            if (chkPsycheActiveMagician != null)
                                                chkPsycheActiveMagician.Visible = false;
                                        }
                                        else if (refreshingPanel == pnlSustainedComplexForms && chkPsycheActiveTechnomancer != null)
                                        {
                                            chkPsycheActiveTechnomancer.Visible = false;
                                        }
                                    }
                                }

                                foreach (SustainedObject objSustained in notifyCollectionChangedEventArgs.NewItems)
                                {
                                    Panel refreshingPanel = DetermineRefreshingPanel(objSustained, pnlSustainedSpells,
                                        pnlSustainedComplexForms, pnlSustainedCritterPowers);

                                    if (refreshingPanel == null)
                                        continue;

                                    refreshingPanel.Visible = true;
                                    switch (objSustained.LinkedObjectType)
                                    {
                                        case Improvement.ImprovementSource.Spell:
                                            if (chkPsycheActiveMagician != null)
                                                chkPsycheActiveMagician.Visible = true;
                                            break;

                                        case Improvement.ImprovementSource.ComplexForm:
                                            if (chkPsycheActiveTechnomancer != null)
                                                chkPsycheActiveTechnomancer.Visible = true;
                                            break;
                                    }

                                    intSustainedObjects = refreshingPanel.Controls.Count;

                                    SustainedObjectControl objSustainedObjectControl =
                                        new SustainedObjectControl(objSustained);

                                    objSustainedObjectControl.SustainedObjectDetailChanged += MakeDirtyWithCharacterUpdate;
                                    objSustainedObjectControl.UnsustainObject += DeleteSustainedObject;

                                    objSustainedObjectControl.Top = intSustainedObjects * objSustainedObjectControl.Height;

                                    refreshingPanel.Controls.Add(objSustainedObjectControl);
                                }
                            }
                            break;
                    }
                }
            }
        }

        public void DeleteSustainedObject(object sender, EventArgs e)
        {
            if (sender is SustainedObjectControl objSender)
            {
                SustainedObject objSustainedObject = objSender.LinkedSustainedObject;

                if (!CommonFunctions.ConfirmDelete(string.Format(LanguageManager.GetString("Message_DeleteSustainedSpell"), objSustainedObject.CurrentDisplayName)))
                    return;

                CharacterObject.SustainedCollection.Remove(objSustainedObject);
            }
        }

        /// <summary>
        /// Moves a tree node to a specified spot in it's parent node collection.
        /// Will persist between loads if the node's object is an ICanSort
        /// </summary>
        /// <param name="objNode">The item to move</param>
        /// <param name="intNewIndex">The new index in the parent array</param>
        public void MoveTreeNode(TreeNode objNode, int intNewIndex)
        {
            if (!(objNode?.Tag is ICanSort objSortable))
                return;

            TreeView treOwningTree = objNode.TreeView;
            TreeNode objParent = objNode.Parent;
            TreeNodeCollection lstNodes = objParent?.Nodes ?? treOwningTree?.Nodes;

            if (lstNodes == null)
                return;
            using (CursorWait.New(this))
            {
                List<ICanSort> lstSorted = lstNodes.Cast<TreeNode>().Select(n => n.Tag).OfType<ICanSort>().ToList();

                // Anything that can't be sorted gets sent to the front of the list, so subtract that number from our new
                // sorting index and make sure we're still inside the array
                intNewIndex = Math.Min(lstSorted.Count - 1,
                    Math.Max(0, intNewIndex + lstSorted.Count - lstNodes.Count));

                lstSorted.Remove(objSortable);
                lstSorted.Insert(intNewIndex, objSortable);

                // Update the sort field of everything in the array. Doing it this way means we only t
                for (int i = 0; i < lstSorted.Count; ++i)
                {
                    lstSorted[i].SortOrder = i;
                }

                // Sort the actual tree
                treOwningTree.SortCustomOrder();

                IsDirty = true;
            }
        }

        /// <summary>
        /// Adds the selected Object and child items to the clipboard as appropriate.
        /// </summary>
        /// <param name="selectedObject"></param>
        public void CopyObject(object selectedObject)
        {
            using (CursorWait.New(this))
            {
                switch (selectedObject)
                {
                    case Armor objCopyArmor:
                        {
                            XmlDocument objCharacterXml = new XmlDocument { XmlResolver = null };
                            using (MemoryStream objStream = new MemoryStream())
                            {
                                using (XmlWriter objWriter = Utils.GetStandardXmlWriter(objStream))
                                {
                                    objWriter.WriteStartDocument();

                                    // </characters>
                                    objWriter.WriteStartElement("character");

                                    objCopyArmor.Save(objWriter);
                                    GlobalSettings.ClipboardContentType = ClipboardContentType.Armor;

                                    if (!objCopyArmor.WeaponID.IsEmptyGuid())
                                    {
                                        // <weapons>
                                        objWriter.WriteStartElement("weapons");
                                        // Copy any Weapon that comes with the Gear.
                                        foreach (Weapon objCopyWeapon in CharacterObject.Weapons.DeepWhere(
                                                     x => x.Children,
                                                     x => x.ParentID == objCopyArmor.InternalId))
                                        {
                                            objCopyWeapon.Save(objWriter);
                                        }

                                        objWriter.WriteEndElement();
                                    }

                                    // </characters>
                                    objWriter.WriteEndElement();

                                    // Finish the document and flush the Writer and Stream.
                                    objWriter.WriteEndDocument();
                                    objWriter.Flush();
                                }

                                // Read the stream.
                                objStream.Position = 0;

                                using (StreamReader objReader = new StreamReader(objStream, Encoding.UTF8, true))
                                using (XmlReader objXmlReader =
                                       XmlReader.Create(objReader, GlobalSettings.SafeXmlReaderSettings))
                                    // Put the stream into an XmlDocument
                                    objCharacterXml.Load(objXmlReader);
                            }

                            GlobalSettings.Clipboard = objCharacterXml;
                            break;
                        }
                    case ArmorMod objCopyArmorMod:
                        {
                            XmlDocument objCharacterXml = new XmlDocument { XmlResolver = null };
                            using (MemoryStream objStream = new MemoryStream())
                            {
                                using (XmlWriter objWriter = Utils.GetStandardXmlWriter(objStream))
                                {
                                    objWriter.WriteStartDocument();

                                    // </characters>
                                    objWriter.WriteStartElement("character");

                                    objCopyArmorMod.Save(objWriter);
                                    GlobalSettings.ClipboardContentType = ClipboardContentType.Armor;

                                    if (!objCopyArmorMod.WeaponID.IsEmptyGuid())
                                    {
                                        // <weapons>
                                        objWriter.WriteStartElement("weapons");
                                        // Copy any Weapon that comes with the Gear.
                                        foreach (Weapon objCopyWeapon in CharacterObject.Weapons.DeepWhere(
                                                     x => x.Children,
                                                     x => x.ParentID == objCopyArmorMod.InternalId))
                                        {
                                            objCopyWeapon.Save(objWriter);
                                        }

                                        objWriter.WriteEndElement();
                                    }

                                    // </characters>
                                    objWriter.WriteEndElement();

                                    // Finish the document and flush the Writer and Stream.
                                    objWriter.WriteEndDocument();
                                    objWriter.Flush();
                                }

                                // Read the stream.
                                objStream.Position = 0;

                                using (StreamReader objReader = new StreamReader(objStream, Encoding.UTF8, true))
                                using (XmlReader objXmlReader =
                                       XmlReader.Create(objReader, GlobalSettings.SafeXmlReaderSettings))
                                    // Put the stream into an XmlDocument
                                    objCharacterXml.Load(objXmlReader);
                            }

                            GlobalSettings.Clipboard = objCharacterXml;
                            break;
                        }
                    case Cyberware objCopyCyberware:
                        {
                            XmlDocument objCharacterXml = new XmlDocument { XmlResolver = null };
                            using (MemoryStream objStream = new MemoryStream())
                            {
                                using (XmlWriter objWriter = Utils.GetStandardXmlWriter(objStream))
                                {
                                    objWriter.WriteStartDocument();

                                    // </characters>
                                    objWriter.WriteStartElement("character");

                                    objCopyCyberware.Save(objWriter);
                                    GlobalSettings.ClipboardContentType = ClipboardContentType.Cyberware;

                                    if (!objCopyCyberware.WeaponID.IsEmptyGuid())
                                    {
                                        // <weapons>
                                        objWriter.WriteStartElement("weapons");
                                        // Copy any Weapon that comes with the Gear.
                                        foreach (Weapon objCopyWeapon in CharacterObject.Weapons.DeepWhere(
                                                     x => x.Children,
                                                     x => x.ParentID == objCopyCyberware.InternalId))
                                        {
                                            objCopyWeapon.Save(objWriter);
                                        }

                                        objWriter.WriteEndElement();
                                    }

                                    if (!objCopyCyberware.VehicleID.IsEmptyGuid())
                                    {
                                        // <vehicles>
                                        objWriter.WriteStartElement("vehicles");
                                        // Copy any Vehicle that comes with the Gear.
                                        foreach (Vehicle objCopyVehicle in CharacterObject.Vehicles.Where(x =>
                                                     x.ParentID == objCopyCyberware.InternalId))
                                        {
                                            objCopyVehicle.Save(objWriter);
                                        }

                                        objWriter.WriteEndElement();
                                    }

                                    // </characters>
                                    objWriter.WriteEndElement();

                                    // Finish the document and flush the Writer and Stream.
                                    objWriter.WriteEndDocument();
                                    objWriter.Flush();
                                }

                                // Read the stream.
                                objStream.Position = 0;

                                using (StreamReader objReader = new StreamReader(objStream, Encoding.UTF8, true))
                                using (XmlReader objXmlReader =
                                       XmlReader.Create(objReader, GlobalSettings.SafeXmlReaderSettings))
                                    // Put the stream into an XmlDocument
                                    objCharacterXml.Load(objXmlReader);
                            }

                            GlobalSettings.Clipboard = objCharacterXml;
                            //Clipboard.SetText(objCharacterXml.OuterXml);
                            break;
                        }
                    case Gear objCopyGear:
                        {
                            XmlDocument objCharacterXml = new XmlDocument { XmlResolver = null };
                            using (MemoryStream objStream = new MemoryStream())
                            {
                                using (XmlWriter objWriter = Utils.GetStandardXmlWriter(objStream))
                                {
                                    objWriter.WriteStartDocument();

                                    // </characters>
                                    objWriter.WriteStartElement("character");

                                    objCopyGear.Save(objWriter);
                                    GlobalSettings.ClipboardContentType = ClipboardContentType.Gear;

                                    if (!objCopyGear.WeaponID.IsEmptyGuid())
                                    {
                                        // <weapons>
                                        objWriter.WriteStartElement("weapons");
                                        // Copy any Weapon that comes with the Gear.
                                        foreach (Weapon objCopyWeapon in CharacterObject.Weapons.DeepWhere(
                                                     x => x.Children,
                                                     x => x.ParentID == objCopyGear.InternalId))
                                        {
                                            objCopyWeapon.Save(objWriter);
                                        }

                                        objWriter.WriteEndElement();
                                    }

                                    // </characters>
                                    objWriter.WriteEndElement();

                                    // Finish the document and flush the Writer and Stream.
                                    objWriter.WriteEndDocument();
                                    objWriter.Flush();
                                }

                                // Read the stream.
                                objStream.Position = 0;

                                using (StreamReader objReader = new StreamReader(objStream, Encoding.UTF8, true))
                                using (XmlReader objXmlReader =
                                       XmlReader.Create(objReader, GlobalSettings.SafeXmlReaderSettings))
                                    // Put the stream into an XmlDocument
                                    objCharacterXml.Load(objXmlReader);
                            }

                            GlobalSettings.Clipboard = objCharacterXml;
                            break;
                        }
                    case Lifestyle objCopyLifestyle:
                        {
                            XmlDocument objCharacterXml = new XmlDocument { XmlResolver = null };
                            using (MemoryStream objStream = new MemoryStream())
                            {
                                using (XmlWriter objWriter = Utils.GetStandardXmlWriter(objStream))
                                {
                                    objWriter.WriteStartDocument();

                                    // </characters>
                                    objWriter.WriteStartElement("character");

                                    objCopyLifestyle.Save(objWriter);

                                    // </characters>
                                    objWriter.WriteEndElement();

                                    // Finish the document and flush the Writer and Stream.
                                    objWriter.WriteEndDocument();
                                    objWriter.Flush();

                                    // Read the stream.
                                    objStream.Position = 0;

                                    using (StreamReader objReader = new StreamReader(objStream, Encoding.UTF8, true))
                                    using (XmlReader objXmlReader =
                                           XmlReader.Create(objReader, GlobalSettings.SafeXmlReaderSettings))
                                        // Put the stream into an XmlDocument
                                        objCharacterXml.Load(objXmlReader);
                                }
                            }

                            GlobalSettings.Clipboard = objCharacterXml;
                            GlobalSettings.ClipboardContentType = ClipboardContentType.Lifestyle;
                            //Clipboard.SetText(objCharacterXml.OuterXml);
                            break;
                        }
                    case Vehicle objCopyVehicle:
                        {
                            XmlDocument objCharacterXml = new XmlDocument { XmlResolver = null };
                            using (MemoryStream objStream = new MemoryStream())
                            {
                                using (XmlWriter objWriter = Utils.GetStandardXmlWriter(objStream))
                                {
                                    objWriter.WriteStartDocument();

                                    // </characters>
                                    objWriter.WriteStartElement("character");

                                    objCopyVehicle.Save(objWriter);

                                    // </characters>
                                    objWriter.WriteEndElement();

                                    // Finish the document and flush the Writer and Stream.
                                    objWriter.WriteEndDocument();
                                    objWriter.Flush();
                                }

                                // Read the stream.
                                objStream.Position = 0;

                                using (StreamReader objReader = new StreamReader(objStream, Encoding.UTF8, true))
                                using (XmlReader objXmlReader =
                                       XmlReader.Create(objReader, GlobalSettings.SafeXmlReaderSettings))
                                    // Put the stream into an XmlDocument
                                    objCharacterXml.Load(objXmlReader);
                            }

                            GlobalSettings.Clipboard = objCharacterXml;
                            GlobalSettings.ClipboardContentType = ClipboardContentType.Vehicle;
                            //Clipboard.SetText(objCharacterXml.OuterXml);
                            break;
                        }
                    case Weapon objCopyWeapon:
                        {
                            // Do not let the user copy Gear or Cyberware Weapons.
                            if (objCopyWeapon.Category == "Gear" || objCopyWeapon.Cyberware)
                                return;
                            
                            XmlDocument objCharacterXml = new XmlDocument { XmlResolver = null };
                            using (MemoryStream objStream = new MemoryStream())
                            {
                                using (XmlWriter objWriter = Utils.GetStandardXmlWriter(objStream))
                                {
                                    objWriter.WriteStartDocument();

                                    // </characters>
                                    objWriter.WriteStartElement("character");

                                    objCopyWeapon.Save(objWriter);

                                    // </characters>
                                    objWriter.WriteEndElement();

                                    // Finish the document and flush the Writer and Stream.
                                    objWriter.WriteEndDocument();
                                    objWriter.Flush();
                                }

                                // Read the stream.
                                objStream.Position = 0;

                                using (StreamReader objReader = new StreamReader(objStream, Encoding.UTF8, true))
                                using (XmlReader objXmlReader =
                                       XmlReader.Create(objReader, GlobalSettings.SafeXmlReaderSettings))
                                    // Put the stream into an XmlDocument
                                    objCharacterXml.Load(objXmlReader);
                            }

                            GlobalSettings.Clipboard = objCharacterXml;
                            GlobalSettings.ClipboardContentType = ClipboardContentType.Weapon;
                            break;
                        }
                    case WeaponAccessory objCopyAccessory:
                        {
                            // Do not let the user copy accessories that are unique to its parent.
                            if (objCopyAccessory.IncludedInWeapon)
                                return;
                            
                            XmlDocument objCharacterXml = new XmlDocument { XmlResolver = null };
                            using (MemoryStream objStream = new MemoryStream())
                            {
                                using (XmlWriter objWriter = Utils.GetStandardXmlWriter(objStream))
                                {
                                    objWriter.WriteStartDocument();

                                    // </characters>
                                    objWriter.WriteStartElement("character");

                                    objCopyAccessory.Save(objWriter);

                                    // </characters>
                                    objWriter.WriteEndElement();

                                    // Finish the document and flush the Writer and Stream.
                                    objWriter.WriteEndDocument();
                                    objWriter.Flush();
                                }

                                // Read the stream.
                                objStream.Position = 0;

                                using (StreamReader objReader = new StreamReader(objStream, Encoding.UTF8, true))
                                using (XmlReader objXmlReader =
                                       XmlReader.Create(objReader, GlobalSettings.SafeXmlReaderSettings))
                                    // Put the stream into an XmlDocument
                                    objCharacterXml.Load(objXmlReader);
                            }

                            GlobalSettings.Clipboard = objCharacterXml;
                            GlobalSettings.ClipboardContentType = ClipboardContentType.WeaponAccessory;
                            break;
                        }
                }
            }
        }

        #region ContactControl Events

        protected void DragContactControl(object sender, MouseEventArgs e)
        {
            if (sender is Control source)
                source.DoDragDrop(new TransportWrapper(source), DragDropEffects.Move);
        }

        protected void AddContact()
        {
            Contact objContact = new Contact(CharacterObject)
            {
                EntityType = ContactType.Contact
            };
            CharacterObject.Contacts.Add(objContact);

            IsCharacterUpdateRequested = true;

            IsDirty = true;
        }

        protected void DeleteContact(object sender, EventArgs e)
        {
            if (sender is ContactControl objSender)
            {
                if (!CommonFunctions.ConfirmDelete(LanguageManager.GetString("Message_DeleteContact")))
                    return;

                CharacterObject.Contacts.Remove(objSender.ContactObject);

                IsCharacterUpdateRequested = true;

                IsDirty = true;
            }
        }

        #endregion ContactControl Events

        #region PetControl Events

        protected void AddPet()
        {
            Contact objContact = new Contact(CharacterObject)
            {
                EntityType = ContactType.Pet
            };

            CharacterObject.Contacts.Add(objContact);

            IsCharacterUpdateRequested = true;

            IsDirty = true;
        }

        protected void DeletePet(object sender, EventArgs e)
        {
            if (sender is PetControl objSender)
            {
                if (!CommonFunctions.ConfirmDelete(LanguageManager.GetString("Message_DeleteContact")))
                    return;

                CharacterObject.Contacts.Remove(objSender.ContactObject);

                IsCharacterUpdateRequested = true;

                IsDirty = true;
            }
        }

        #endregion PetControl Events

        #region EnemyControl Events

        protected void AddEnemy()
        {
            // Handle the ConnectionRatingChanged Event for the ContactControl object.
            Contact objContact = new Contact(CharacterObject)
            {
                EntityType = ContactType.Enemy
            };

            CharacterObject.Contacts.Add(objContact);

            IsCharacterUpdateRequested = true;

            IsDirty = true;
        }

        protected void DeleteEnemy(object sender, EventArgs e)
        {
            if (sender is ContactControl objSender)
            {
                if (!CommonFunctions.ConfirmDelete(LanguageManager.GetString("Message_DeleteEnemy")))
                    return;

                CharacterObject.Contacts.Remove(objSender.ContactObject);

                IsCharacterUpdateRequested = true;

                IsDirty = true;
            }
        }

        #endregion EnemyControl Events

        #region Additional Relationships Tab Control Events

        protected async ValueTask AddContactsFromFile()
        {
            using (CursorWait.New(this))
            {
                XPathDocument xmlDoc;
                // Displays an OpenFileDialog so the user can select the XML to read.
                using (OpenFileDialog dlgOpenFileDialog = new OpenFileDialog
                       {
                           Filter = await LanguageManager.GetStringAsync("DialogFilter_Xml") + '|' +
                                    await LanguageManager.GetStringAsync("DialogFilter_All")
                       })
                {
                    // Show the Dialog.
                    // If the user cancels out, return early.
                    if (dlgOpenFileDialog.ShowDialog(this) != DialogResult.OK)
                        return;

                    try
                    {
                        using (StreamReader objStreamReader =
                               new StreamReader(dlgOpenFileDialog.FileName, Encoding.UTF8, true))
                        using (XmlReader objXmlReader =
                               XmlReader.Create(objStreamReader, GlobalSettings.SafeXmlReaderSettings))
                            xmlDoc = new XPathDocument(objXmlReader);
                    }
                    catch (IOException ex)
                    {
                        Program.ShowMessageBox(this, ex.ToString());
                        return;
                    }
                    catch (XmlException ex)
                    {
                        Program.ShowMessageBox(this, ex.ToString());
                        return;
                    }
                }

                foreach (XPathNavigator xmlContact in await xmlDoc.CreateNavigator()
                                                                  .SelectAndCacheExpressionAsync(
                                                                      "/chummer/contacts/contact"))
                {
                    Contact objContact = new Contact(CharacterObject);
                    objContact.Load(xmlContact);
                    CharacterObject.Contacts.Add(objContact);
                }
            }
        }

        #endregion Additional Relationships Tab Control Events

        public void RefreshSpirits(Panel panSpirits, Panel panSprites, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs = null)
        {
            if (panSpirits == null && panSprites == null)
                return;
            using (CursorWait.New(this))
            {
                if (notifyCollectionChangedEventArgs == null ||
                    notifyCollectionChangedEventArgs.Action == NotifyCollectionChangedAction.Reset)
                {
                    panSpirits?.SuspendLayout();
                    panSprites?.SuspendLayout();
                    panSpirits?.Controls.Clear();
                    panSprites?.Controls.Clear();
                    int intSpirits = -1;
                    int intSprites = -1;
                    foreach (Spirit objSpirit in CharacterObject.Spirits)
                    {
                        bool blnIsSpirit = objSpirit.EntityType == SpiritType.Spirit;
                        if (blnIsSpirit)
                        {
                            if (panSpirits == null)
                                continue;
                        }
                        else if (panSprites == null)
                            continue;

                        SpiritControl objSpiritControl = new SpiritControl(objSpirit);

                        // Attach an EventHandler for the ServicesOwedChanged Event.
                        objSpiritControl.ContactDetailChanged += MakeDirtyWithCharacterUpdate;
                        objSpiritControl.DeleteSpirit += DeleteSpirit;

                        objSpiritControl.RebuildSpiritList(CharacterObject.MagicTradition);

                        if (blnIsSpirit)
                        {
                            ++intSpirits;
                            objSpiritControl.Top = intSpirits * objSpiritControl.Height;
                            panSpirits.Controls.Add(objSpiritControl);
                        }
                        else
                        {
                            ++intSprites;
                            objSpiritControl.Top = intSprites * objSpiritControl.Height;
                            panSprites.Controls.Add(objSpiritControl);
                        }
                    }

                    panSpirits?.ResumeLayout();
                    panSprites?.ResumeLayout();
                }
                else
                {
                    switch (notifyCollectionChangedEventArgs.Action)
                    {
                        case NotifyCollectionChangedAction.Add:
                            {
                                int intSpirits = panSpirits?.Controls.Count ?? 0;
                                int intSprites = panSprites?.Controls.Count ?? 0;
                                foreach (Spirit objSpirit in notifyCollectionChangedEventArgs.NewItems)
                                {
                                    bool blnIsSpirit = objSpirit.EntityType == SpiritType.Spirit;
                                    if (blnIsSpirit)
                                    {
                                        if (panSpirits == null)
                                            continue;
                                    }
                                    else if (panSprites == null)
                                        continue;

                                    SpiritControl objSpiritControl = new SpiritControl(objSpirit);

                                    // Attach an EventHandler for the ServicesOwedChanged Event.
                                    objSpiritControl.ContactDetailChanged += MakeDirtyWithCharacterUpdate;
                                    objSpiritControl.DeleteSpirit += DeleteSpirit;

                                    objSpiritControl.RebuildSpiritList(CharacterObject.MagicTradition);

                                    if (blnIsSpirit)
                                    {
                                        objSpiritControl.Top = intSpirits * objSpiritControl.Height;
                                        panSpirits.Controls.Add(objSpiritControl);
                                        ++intSpirits;
                                    }
                                    else
                                    {
                                        objSpiritControl.Top = intSprites * objSpiritControl.Height;
                                        panSprites.Controls.Add(objSpiritControl);
                                        ++intSprites;
                                    }
                                }
                            }
                            break;

                        case NotifyCollectionChangedAction.Remove:
                            {
                                foreach (Spirit objSpirit in notifyCollectionChangedEventArgs.OldItems)
                                {
                                    int intMoveUpAmount = 0;
                                    if (objSpirit.EntityType == SpiritType.Spirit)
                                    {
                                        if (panSpirits == null)
                                            continue;
                                        int intSpirits = panSpirits.Controls.Count;
                                        for (int i = 0; i < intSpirits; ++i)
                                        {
                                            Control objLoopControl = panSpirits.Controls[i];
                                            if (objLoopControl is SpiritControl objSpiritControl &&
                                                objSpiritControl.SpiritObject == objSpirit)
                                            {
                                                intMoveUpAmount = objSpiritControl.Height;
                                                panSpirits.Controls.RemoveAt(i);
                                                objSpiritControl.ContactDetailChanged -= MakeDirtyWithCharacterUpdate;
                                                objSpiritControl.DeleteSpirit -= DeleteSpirit;
                                                objSpiritControl.Dispose();
                                                --i;
                                                --intSpirits;
                                            }
                                            else if (intMoveUpAmount != 0)
                                            {
                                                objLoopControl.Top -= intMoveUpAmount;
                                            }
                                        }
                                    }
                                    else if (panSprites != null)
                                    {
                                        int intSprites = panSprites.Controls.Count;
                                        for (int i = 0; i < intSprites; ++i)
                                        {
                                            Control objLoopControl = panSprites.Controls[i];
                                            if (objLoopControl is SpiritControl objSpiritControl &&
                                                objSpiritControl.SpiritObject == objSpirit)
                                            {
                                                intMoveUpAmount = objSpiritControl.Height;
                                                panSprites.Controls.RemoveAt(i);
                                                objSpiritControl.ContactDetailChanged -= MakeDirtyWithCharacterUpdate;
                                                objSpiritControl.DeleteSpirit -= DeleteSpirit;
                                                objSpiritControl.Dispose();
                                                --i;
                                                --intSprites;
                                            }
                                            else if (intMoveUpAmount != 0)
                                            {
                                                objLoopControl.Top -= intMoveUpAmount;
                                            }
                                        }
                                    }
                                }
                            }
                            break;

                        case NotifyCollectionChangedAction.Replace:
                            {
                                int intSpirits = panSpirits?.Controls.Count ?? 0;
                                int intSprites = panSprites?.Controls.Count ?? 0;
                                foreach (Spirit objSpirit in notifyCollectionChangedEventArgs.OldItems)
                                {
                                    int intMoveUpAmount = 0;
                                    if (objSpirit.EntityType == SpiritType.Spirit)
                                    {
                                        if (panSpirits == null)
                                            continue;
                                        for (int i = 0; i < intSpirits; ++i)
                                        {
                                            Control objLoopControl = panSpirits.Controls[i];
                                            if (objLoopControl is SpiritControl objSpiritControl &&
                                                objSpiritControl.SpiritObject == objSpirit)
                                            {
                                                intMoveUpAmount = objSpiritControl.Height;
                                                panSpirits.Controls.RemoveAt(i);
                                                objSpiritControl.ContactDetailChanged -= MakeDirtyWithCharacterUpdate;
                                                objSpiritControl.DeleteSpirit -= DeleteSpirit;
                                                objSpiritControl.Dispose();
                                                --i;
                                                --intSpirits;
                                            }
                                            else if (intMoveUpAmount != 0)
                                            {
                                                objLoopControl.Top -= intMoveUpAmount;
                                            }
                                        }
                                    }
                                    else if (panSprites != null)
                                    {
                                        for (int i = 0; i < intSprites; ++i)
                                        {
                                            Control objLoopControl = panSprites.Controls[i];
                                            if (objLoopControl is SpiritControl objSpiritControl &&
                                                objSpiritControl.SpiritObject == objSpirit)
                                            {
                                                intMoveUpAmount = objSpiritControl.Height;
                                                panSprites.Controls.RemoveAt(i);
                                                objSpiritControl.ContactDetailChanged -= MakeDirtyWithCharacterUpdate;
                                                objSpiritControl.DeleteSpirit -= DeleteSpirit;
                                                objSpiritControl.Dispose();
                                                --i;
                                                --intSprites;
                                            }
                                            else if (intMoveUpAmount != 0)
                                            {
                                                objLoopControl.Top -= intMoveUpAmount;
                                            }
                                        }
                                    }
                                }

                                foreach (Spirit objSpirit in notifyCollectionChangedEventArgs.NewItems)
                                {
                                    bool blnIsSpirit = objSpirit.EntityType == SpiritType.Spirit;
                                    if (blnIsSpirit)
                                    {
                                        if (panSpirits == null)
                                            continue;
                                    }
                                    else if (panSprites == null)
                                        continue;

                                    SpiritControl objSpiritControl = new SpiritControl(objSpirit);

                                    // Attach an EventHandler for the ServicesOwedChanged Event.
                                    objSpiritControl.ContactDetailChanged += MakeDirtyWithCharacterUpdate;
                                    objSpiritControl.DeleteSpirit += DeleteSpirit;

                                    objSpiritControl.RebuildSpiritList(CharacterObject.MagicTradition);

                                    if (blnIsSpirit)
                                    {
                                        objSpiritControl.Top = intSpirits * objSpiritControl.Height;
                                        panSpirits.Controls.Add(objSpiritControl);
                                        ++intSpirits;
                                    }
                                    else
                                    {
                                        objSpiritControl.Top = intSprites * objSpiritControl.Height;
                                        panSprites.Controls.Add(objSpiritControl);
                                        ++intSprites;
                                    }
                                }
                            }
                            break;
                    }
                }
            }
        }

        #region SpiritControl Events

        protected void AddSpirit()
        {
            // The number of bound Spirits cannot exceed the character's CHA.
            if (!CharacterObject.IgnoreRules && CharacterObject.Spirits.Count(x => x.EntityType == SpiritType.Spirit && x.Bound && !x.Fettered) >= CharacterObject.BoundSpiritLimit)
            {
                Program.ShowMessageBox(this, string.Format(GlobalSettings.CultureInfo, LanguageManager.GetString("Message_BoundSpiritLimit"), CharacterObject.Settings.BoundSpiritExpression, CharacterObject.BoundSpiritLimit),
                    LanguageManager.GetString("MessageTitle_BoundSpiritLimit"),
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            Spirit objSpirit = new Spirit(CharacterObject)
            {
                EntityType = SpiritType.Spirit,
                Force = CharacterObject.MaxSpiritForce
            };
            CharacterObject.Spirits.Add(objSpirit);

            IsCharacterUpdateRequested = true;

            IsDirty = true;
        }

        protected void AddSprite()
        {
            // In create, all sprites are added as Bound/Registered. The number of registered Sprites cannot exceed the character's LOG.
            if (!CharacterObject.IgnoreRules &&
                CharacterObject.Spirits.Count(x => x.EntityType == SpiritType.Sprite && x.Bound && !x.Fettered) >=
                CharacterObject.RegisteredSpriteLimit)
            {
                Program.ShowMessageBox(this, string.Format(GlobalSettings.CultureInfo, LanguageManager.GetString("Message_RegisteredSpriteLimit"), CharacterObject.Settings.RegisteredSpriteExpression, CharacterObject.RegisteredSpriteLimit),
                    LanguageManager.GetString("MessageTitle_RegisteredSpriteLimit"),
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            Spirit objSprite = new Spirit(CharacterObject)
            {
                EntityType = SpiritType.Sprite,
                Force = CharacterObject.MaxSpriteLevel
            };
            CharacterObject.Spirits.Add(objSprite);

            IsCharacterUpdateRequested = true;

            IsDirty = true;
        }

        protected void DeleteSpirit(object sender, EventArgs e)
        {
            if (!(sender is SpiritControl objSender))
                return;
            Spirit objSpirit = objSender.SpiritObject;
            bool blnIsSpirit = objSpirit.EntityType == SpiritType.Spirit;
            if (!CommonFunctions.ConfirmDelete(LanguageManager.GetString(blnIsSpirit ? "Message_DeleteSpirit" : "Message_DeleteSprite")))
                return;
            objSpirit.Fettered = false; // Fettered spirits consume MAG.
            CharacterObject.Spirits.Remove(objSpirit);

            IsCharacterUpdateRequested = true;

            IsDirty = true;
        }

        #endregion SpiritControl Events

        /// <summary>
        /// Add a mugshot to the character.
        /// </summary>
        protected async ValueTask<bool> AddMugshot()
        {
            using (CursorWait.New(this))
            {
                using (OpenFileDialog dlgOpenFileDialog = new OpenFileDialog())
                {
                    if (!string.IsNullOrWhiteSpace(GlobalSettings.RecentImageFolder) &&
                        Directory.Exists(GlobalSettings.RecentImageFolder))
                    {
                        dlgOpenFileDialog.InitialDirectory = GlobalSettings.RecentImageFolder;
                    }
                    // Prompt the user to select an image to associate with this character.

                    ImageCodecInfo[] lstCodecs = ImageCodecInfo.GetImageEncoders();
                    string strFormat = "{0}" + await LanguageManager.GetStringAsync("String_Space") + "({1})|{1}";
                    dlgOpenFileDialog.Filter = string.Format(
                        GlobalSettings.InvariantCultureInfo,
                        await LanguageManager.GetStringAsync("DialogFilter_ImagesPrefix") + "({1})|{1}|{0}|" +
                        await LanguageManager.GetStringAsync("DialogFilter_All"),
                        string.Join("|",
                                    lstCodecs.Select(codec => string.Format(GlobalSettings.CultureInfo,
                                                                            strFormat, codec.CodecName,
                                                                            codec.FilenameExtension))),
                        string.Join(";", lstCodecs.Select(codec => codec.FilenameExtension)));

                    bool blnMakeLoop = true;
                    while (blnMakeLoop)
                    {
                        blnMakeLoop = false;
                        if (dlgOpenFileDialog.ShowDialog(this) != DialogResult.OK)
                            return false;
                        if (!File.Exists(dlgOpenFileDialog.FileName))
                        {
                            Program.ShowMessageBox(string.Format(
                                                       await LanguageManager.GetStringAsync(
                                                           "Message_File_Cannot_Be_Read_Accessed"),
                                                       dlgOpenFileDialog.FileName));
                            blnMakeLoop = true;
                        }
                    }

                    // Convert the image to a string using Base64.
                    GlobalSettings.RecentImageFolder = Path.GetDirectoryName(dlgOpenFileDialog.FileName);

                    using (Bitmap bmpMugshot = new Bitmap(dlgOpenFileDialog.FileName, true))
                    {
                        if (bmpMugshot.PixelFormat == PixelFormat.Format32bppPArgb)
                        {
                            await CharacterObject.Mugshots.AddAsync(
                                bmpMugshot.Clone() as Bitmap); // Clone makes sure file handle is closed
                        }
                        else
                        {
                            await CharacterObject.Mugshots.AddAsync(
                                bmpMugshot.ConvertPixelFormat(PixelFormat.Format32bppPArgb));
                        }
                    }

                    if (CharacterObject.MainMugshotIndex == -1)
                        CharacterObject.MainMugshotIndex = CharacterObject.Mugshots.Count - 1;
                }

                return true;
            }
        }

        /// <summary>
        /// Update the mugshot info of a character.
        /// </summary>
        /// <param name="picMugshot"></param>
        /// <param name="intCurrentMugshotIndexInList"></param>
        protected void UpdateMugshot(PictureBox picMugshot, int intCurrentMugshotIndexInList)
        {
            if (picMugshot == null)
                return;
            if (intCurrentMugshotIndexInList < 0 || intCurrentMugshotIndexInList >= CharacterObject.Mugshots.Count || CharacterObject.Mugshots[intCurrentMugshotIndexInList] == null)
            {
                picMugshot.Image = null;
                return;
            }

            Image imgMugshot = CharacterObject.Mugshots[intCurrentMugshotIndexInList];

            try
            {
                picMugshot.SizeMode = imgMugshot != null && picMugshot.Height >= imgMugshot.Height && picMugshot.Width >= imgMugshot.Width
                    ? PictureBoxSizeMode.CenterImage
                    : PictureBoxSizeMode.Zoom;
            }
            catch (ArgumentException) // No other way to catch when the Image is not null, but is disposed
            {
                picMugshot.SizeMode = PictureBoxSizeMode.Zoom;
            }
            picMugshot.Image = imgMugshot;
        }

        /// <summary>
        /// Remove a mugshot of a character.
        /// </summary>
        /// <param name="intCurrentMugshotIndexInList"></param>
        protected void RemoveMugshot(int intCurrentMugshotIndexInList)
        {
            if (intCurrentMugshotIndexInList < 0 || intCurrentMugshotIndexInList >= CharacterObject.Mugshots.Count)
            {
                return;
            }

            CharacterObject.Mugshots.RemoveAt(intCurrentMugshotIndexInList);
            if (intCurrentMugshotIndexInList == CharacterObject.MainMugshotIndex)
            {
                CharacterObject.MainMugshotIndex = -1;
            }
            else if (intCurrentMugshotIndexInList < CharacterObject.MainMugshotIndex)
            {
                --CharacterObject.MainMugshotIndex;
            }
        }

        /// <summary>
        /// Whether or not the character has changes that can be saved
        /// </summary>
        public bool IsDirty
        {
            get => _blnIsDirty;
            set
            {
                if (_blnIsDirty != value)
                {
                    _blnIsDirty = value;
                    UpdateWindowTitle(true);
                }
            }
        }

        /// <summary>
        /// Whether or not the form is currently in the middle of refreshing some UI elements
        /// </summary>
        public bool IsRefreshing
        {
            get => _blnIsRefreshing;
            set => _blnIsRefreshing = value;
            /*
            {
                if (_blnIsRefreshing != value)
                {
                    _blnIsRefreshing = value;
                    if (value)
                        SuspendLayout();
                    else if (!IsLoading)
                        ResumeLayout();
                }
            }
            */
        }

        public bool IsLoading
        {
            get => _blnLoading;
            set => _blnLoading = value;
            /*
            {
                if (_blnLoading != value)
                {
                    _blnLoading = value;
                    if (value)
                        SuspendLayout();
                    else if (!IsRefreshing)
                        ResumeLayout();
                }
            }
            */
        }

        public void MakeDirtyWithCharacterUpdate(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Move)
                return;

            IsCharacterUpdateRequested = true;

            IsDirty = true;
        }

        public void MakeDirtyWithCharacterUpdate(object sender, ListChangedEventArgs e)
        {
            if (e.ListChangedType != ListChangedType.ItemAdded
                && e.ListChangedType != ListChangedType.ItemChanged
                && e.ListChangedType != ListChangedType.ItemDeleted
                && e.ListChangedType != ListChangedType.Reset)
                return;

            IsCharacterUpdateRequested = true;

            IsDirty = true;
        }

        public void MakeDirtyWithCharacterUpdate(object sender, EventArgs e)
        {
            IsCharacterUpdateRequested = true;

            IsDirty = true;
        }

        public void MakeDirty(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Move)
                return;

            IsDirty = true;
        }

        public void MakeDirty(object sender, EventArgs e)
        {
            IsDirty = true;
        }

        public bool IsCharacterUpdateRequested { get; set; }

        public Character CharacterObject => _objCharacter;

        private CharacterSettings _objCachedSettings;

        protected CharacterSettings CharacterObjectSettings => _objCachedSettings ?? (_objCachedSettings = CharacterObject?.Settings);

        protected virtual string FormMode => string.Empty;

        protected void ShiftTabsOnMouseScroll(object sender, MouseEventArgs e)
        {
            if (e == null)
                return;
            //TODO: Global option to switch behaviour on/off, method to emulate clicking the scroll buttons instead of changing the selected index,
            //allow wrapping back to first/last tab item based on scroll direction
            if (sender is TabControl tabControl && e.Location.Y <= tabControl.ItemSize.Height)
            {
                int intScrollAmount = e.Delta;
                int intSelectedTabIndex = tabControl.SelectedIndex;

                if (intScrollAmount < 0)
                {
                    if (intSelectedTabIndex < tabControl.TabCount - 1)
                        tabControl.SelectedIndex = intSelectedTabIndex + 1;
                }
                else if (intSelectedTabIndex > 0)
                    tabControl.SelectedIndex = intSelectedTabIndex - 1;
            }
        }

        /// <summary>
        /// Update the Window title to show the Character's name and unsaved changes status.
        /// </summary>
        public void UpdateWindowTitle(bool blnCanSkip)
        {
            if (Text.EndsWith('*') == _blnIsDirty && blnCanSkip)
                return;

            string strSpace = LanguageManager.GetString("String_Space");
            string strTitle = CharacterObject.CharacterName + strSpace + '-' + strSpace + FormMode + strSpace + '(' + CharacterObjectSettings.Name + ')';
            if (_blnIsDirty)
                strTitle += '*';
            this.QueueThreadSafe(() => Text = strTitle);
        }

        /// <summary>
        /// Save the Character.
        /// </summary>
        public virtual async ValueTask<bool> SaveCharacter(bool blnNeedConfirm = true, bool blnDoCreated = false)
        {
            using (CursorWait.New(this))
            {
                // If the Character does not have a file name, trigger the Save As menu item instead.
                if (string.IsNullOrEmpty(CharacterObject.FileName))
                {
                    return await SaveCharacterAs(blnDoCreated);
                }

                if (blnDoCreated)
                {
                    // If the Created is checked, make sure the user wants to actually save this character.
                    if (blnNeedConfirm && !await ConfirmSaveCreatedCharacter())
                        return false;
                    // If this character has just been saved as Created, close this form and re-open the character which will open it in the Career window instead.
                    return await SaveCharacterAsCreated();
                }

                using (LoadingBar frmProgressBar = await Program.CreateAndShowProgressBarAsync())
                {
                    frmProgressBar.PerformStep(CharacterObject.CharacterName,
                                               LoadingBar.ProgressBarTextPatterns.Saving);
                    if (!await CharacterObject.SaveAsync())
                        return false;
                    GlobalSettings.MostRecentlyUsedCharacters.Insert(0, CharacterObject.FileName);
                    IsDirty = false;
                }

                return true;
            }
        }

        /// <summary>
        /// Save the Character using the Save As dialogue box.
        /// </summary>
        public virtual async ValueTask<bool> SaveCharacterAs(bool blnDoCreated = false)
        {
            using (CursorWait.New(this))
            {
                // If the Created is checked, make sure the user wants to actually save this character.
                if (blnDoCreated && !await ConfirmSaveCreatedCharacter())
                {
                    return false;
                }

                using (SaveFileDialog saveFileDialog = new SaveFileDialog
                       {
                           Filter = await LanguageManager.GetStringAsync("DialogFilter_Chum5") + '|' +
                                    await LanguageManager.GetStringAsync("DialogFilter_All")
                       })
                {
                    string strShowFileName = CharacterObject.FileName
                                                            .SplitNoAlloc(
                                                                Path.DirectorySeparatorChar,
                                                                StringSplitOptions.RemoveEmptyEntries)
                                                            .LastOrDefault();

                    if (string.IsNullOrEmpty(strShowFileName))
                        strShowFileName = CharacterObject.CharacterName;

                    saveFileDialog.FileName = strShowFileName;

                    if (saveFileDialog.ShowDialog(this) != DialogResult.OK)
                        return false;

                    CharacterObject.FileName = saveFileDialog.FileName;
                }

                return await SaveCharacter(false, blnDoCreated);
            }
        }

        /// <summary>
        /// Save the character as Created and re-open it in Career Mode.
        /// </summary>
        public virtual Task<bool> SaveCharacterAsCreated() { return Task.FromResult(false); }

        /// <summary>
        /// Verify that the user wants to save this character as Created.
        /// </summary>
        public virtual Task<bool> ConfirmSaveCreatedCharacter() { return Task.FromResult(true); }

        /// <summary>
        /// The frmViewer window being used by the character.
        /// </summary>
        public CharacterSheetViewer PrintWindow
        {
            get => _frmPrintView;
            set => _frmPrintView = value;
        }

        public async ValueTask DoPrint()
        {
            using (CursorWait.New(this))
            {
                // If a reference to the Viewer window does not yet exist for this character, open a new Viewer window and set the reference to it.
                // If a Viewer window already exists for this character, use it instead.
                if (_frmPrintView == null)
                {
                    _frmPrintView = new CharacterSheetViewer();
                    await _frmPrintView.SetCharacters(CharacterObject);
                    _frmPrintView.Show();
                }
                else
                {
                    _frmPrintView.Activate();
                }
            }
        }

        public async ValueTask DoExport()
        {
            using (ExportCharacter frmExportCharacter = new ExportCharacter(CharacterObject))
                await frmExportCharacter.ShowDialogSafeAsync(this);
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _objCharacter.PropertyChanged -= RecacheSettingsOnSettingsChange;
                _frmPrintView?.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Vehicles Tab

        public async ValueTask PurchaseVehicleGear(Vehicle objSelectedVehicle, Location objLocation = null)
        {
            using (CursorWait.New(this))
            {
                XmlDocument objXmlDocument = await CharacterObject.LoadDataAsync("gear.xml");
                bool blnAddAgain;

                do
                {
                    using (SelectGear frmPickGear = new SelectGear(CharacterObject, 0, 1, objSelectedVehicle))
                    {
                        await frmPickGear.ShowDialogSafeAsync(this);

                        if (frmPickGear.DialogResult == DialogResult.Cancel)
                            break;
                        blnAddAgain = frmPickGear.AddAgain;

                        // Open the Gear XML file and locate the selected piece.
                        XmlNode objXmlGear = objXmlDocument.SelectSingleNode("/chummer/gears/gear[id = " +
                                                                             frmPickGear.SelectedGear.CleanXPath()
                                                                             + ']');

                        // Create the new piece of Gear.
                        List<Weapon> lstWeapons = new List<Weapon>(1);

                        Gear objGear = new Gear(CharacterObject);
                        objGear.Create(objXmlGear, frmPickGear.SelectedRating, lstWeapons, string.Empty, false);

                        if (objGear.InternalId.IsEmptyGuid())
                            continue;

                        objGear.Quantity = frmPickGear.SelectedQty;
                        objGear.DiscountCost = frmPickGear.BlackMarketDiscount;

                        // Reduce the cost for Do It Yourself components.
                        if (frmPickGear.DoItYourself)
                            objGear.Cost = '(' + objGear.Cost + ") * 0.5";
                        // If the item was marked as free, change its cost.
                        if (frmPickGear.FreeCost)
                            objGear.Cost = "0";

                        if (CharacterObject.Created)
                        {
                            decimal decCost = objGear.TotalCost;

                            // Multiply the cost if applicable.
                            char chrAvail = (await objGear.TotalAvailTupleAsync()).Suffix;
                            switch (chrAvail)
                            {
                                case 'R' when CharacterObjectSettings.MultiplyRestrictedCost:
                                    decCost *= CharacterObjectSettings.RestrictedCostMultiplier;
                                    break;

                                case 'F' when CharacterObjectSettings.MultiplyForbiddenCost:
                                    decCost *= CharacterObjectSettings.ForbiddenCostMultiplier;
                                    break;
                            }

                            // Check the item's Cost and make sure the character can afford it.
                            if (!frmPickGear.FreeCost)
                            {
                                if (decCost > CharacterObject.Nuyen)
                                {
                                    Program.ShowMessageBox(this,
                                                           await LanguageManager.GetStringAsync(
                                                               "Message_NotEnoughNuyen"),
                                                           await LanguageManager.GetStringAsync(
                                                               "MessageTitle_NotEnoughNuyen"),
                                                           MessageBoxButtons.OK,
                                                           MessageBoxIcon.Information);
                                    continue;
                                }

                                // Create the Expense Log Entry.
                                ExpenseLogEntry objExpense = new ExpenseLogEntry(CharacterObject);
                                objExpense.Create(decCost * -1,
                                                  await LanguageManager.GetStringAsync(
                                                      "String_ExpensePurchaseVehicleGear") +
                                                  await LanguageManager.GetStringAsync("String_Space") +
                                                  objGear.CurrentDisplayNameShort, ExpenseType.Nuyen,
                                                  DateTime.Now);
                                CharacterObject.ExpenseEntries.AddWithSort(objExpense);
                                CharacterObject.Nuyen -= decCost;

                                ExpenseUndo objUndo = new ExpenseUndo();
                                objUndo.CreateNuyen(NuyenExpenseType.AddVehicleGear, objGear.InternalId, 1);
                                objExpense.Undo = objUndo;
                            }
                        }

                        Gear objExistingGear = null;
                        // If this is Ammunition, see if the character already has it on them.
                        if ((objGear.Category == "Ammunition" ||
                             !string.IsNullOrEmpty(objGear.AmmoForWeaponType)) && frmPickGear.Stack)
                        {
                            objExistingGear =
                                objSelectedVehicle.GearChildren.FirstOrDefault(x =>
                                                                                   objGear.IsIdenticalToOtherGear(x));
                        }

                        if (objExistingGear != null)
                        {
                            // A match was found, so increase the quantity instead.
                            objExistingGear.Quantity += objGear.Quantity;
                        }
                        else
                        {
                            // Add the Gear to the Vehicle.
                            objLocation?.Children.Add(objGear);
                            objSelectedVehicle.GearChildren.Add(objGear);
                            objGear.Parent = objSelectedVehicle;

                            foreach (Weapon objWeapon in lstWeapons)
                            {
                                objLocation?.Children.Add(objGear);
                                objWeapon.ParentVehicle = objSelectedVehicle;
                                objSelectedVehicle.Weapons.Add(objWeapon);
                            }
                        }
                    }

                    IsCharacterUpdateRequested = true;

                    IsDirty = true;
                } while (blnAddAgain);
            }
        }

        #endregion Vehicles Tab
    }
}
