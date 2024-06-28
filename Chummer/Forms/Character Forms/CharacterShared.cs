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
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;
using Chummer.Backend.Attributes;
using Chummer.Backend.Equipment;
using Chummer.UI.Attributes;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.IO;
using NLog;
using OperationCanceledException = System.OperationCanceledException;

namespace Chummer
{
    /// <summary>
    /// Contains functionality shared between frmCreate and frmCareer
    /// </summary>
    [DesignerCategory("")]
    public class CharacterShared : Form, IHasCharacterObjects
    {
        private static readonly Lazy<Logger> s_ObjLogger = new Lazy<Logger>(LogManager.GetCurrentClassLogger);
        private static Logger Log => s_ObjLogger.Value;
        private static TelemetryClient TelemetryClient { get; } = new TelemetryClient();
        private readonly Character _objCharacter;
        private int _intIsDirty;
        private int _intRefreshingCount;
        private int _intLoadingCount = 1;
        private int _intUpdatingCount;
        private FileSystemWatcher _objCharacterFileWatcher;
        protected readonly SaveFileDialog dlgSaveFile;

        protected CancellationTokenSource GenericCancellationTokenSource { get; } = new CancellationTokenSource();

        protected CancellationToken GenericToken { get; }

        protected CharacterShared(Character objCharacter)
        {
            GenericToken = GenericCancellationTokenSource.Token;
            _objCharacter = objCharacter;
            CancellationTokenRegistration objCancellationRegistration
                = GenericToken.Register(() => Interlocked.Exchange(ref _objUpdateCharacterInfoCancellationTokenSource, null)?.Cancel(false));
            Disposed += (sender, args) => objCancellationRegistration.Dispose();
            _objCharacter.MultiplePropertiesChangedAsync += CharacterPropertyChanged;
            dlgSaveFile = new SaveFileDialog();
            Load += OnLoad;
            Program.MainForm.OpenCharacterEditorForms?.Add(this);
            string name = "Show_Form_" + GetType();
            PageViewTelemetry pvt = new PageViewTelemetry(name)
            {
                Id = Guid.NewGuid().ToString(),
                Name = name,
                Timestamp = DateTimeOffset.UtcNow
            };
            pvt.Context.Operation.Name = "Operation CharacterShared.Constructor()";
            pvt.Properties.Add("Name", objCharacter?.Name);
            string strCharacterFileName = objCharacter?.FileName; // Store this in a local so that we avoid possible weird semaphore collisions in the Shown delegate
            pvt.Properties.Add("Path", strCharacterFileName);
            Shown += (o, args) =>
            {
                pvt.Duration = DateTimeOffset.UtcNow - pvt.Timestamp;
                if (strCharacterFileName != null && Uri.TryCreate(strCharacterFileName, UriKind.Absolute, out Uri uriResult))
                {
                    pvt.Url = uriResult;
                }
                TelemetryClient.TrackPageView(pvt);
            };
            if (GlobalSettings.LiveUpdateCleanCharacterFiles && !string.IsNullOrEmpty(strCharacterFileName) && File.Exists(strCharacterFileName))
            {
                _objCharacterFileWatcher = new FileSystemWatcher(Path.GetDirectoryName(strCharacterFileName) ?? Path.GetPathRoot(strCharacterFileName), Path.GetFileName(strCharacterFileName));
                _objCharacterFileWatcher.Changed += LiveUpdateFromCharacterFile;
            }
            _tmrCharacterUpdateRequestTimer.Elapsed += CharacterUpdateRequestTimerOnElapsed;
            _tmrCharacterUpdateRequestTimer.Start();
            _tmrAutosaveRequestTimer.Elapsed += AutosaveRequestTimerOnElapsed;
            AutosaveStopwatch.Start();
            _tmrAutosaveRequestTimer.Start();
        }

        private async void AutosaveRequestTimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                GenericToken.ThrowIfCancellationRequested();
                if (IsDirty && AutosaveStopwatch?.Elapsed.Minutes >= 5)
                {
                    await AutoSaveCharacter(GenericToken).ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException)
            {
                // swallow this
            }
        }

        [Obsolete("This constructor is for use by form designers only.", true)]
        protected CharacterShared()
        {
        }

        private async void OnLoad(object sender, EventArgs e)
        {
            try
            {
                dlgSaveFile.Filter = await LanguageManager.GetStringAsync("DialogFilter_Chum5", token: GenericToken).ConfigureAwait(false) + '|' +
                                     await LanguageManager.GetStringAsync("DialogFilter_Chum5lz", token: GenericToken).ConfigureAwait(false) + '|' +
                                     await LanguageManager.GetStringAsync("DialogFilter_All", token: GenericToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
        }

        protected virtual void LiveUpdateFromCharacterFile(object sender, FileSystemEventArgs e)
        {
        }

        private async Task CharacterPropertyChanged(object sender, MultiplePropertiesChangedEventArgs e, CancellationToken token = default)
        {
            if (e.PropertyNames.Contains(nameof(Character.FileName)))
            {
                FileSystemWatcher objNewWatcher = null;
                if (GlobalSettings.LiveUpdateCleanCharacterFiles)
                {
                    string strFileName = Path.GetFileName(CharacterObject.FileName);
                    if (!string.IsNullOrEmpty(strFileName))
                    {
                        objNewWatcher = new FileSystemWatcher(
                            Path.GetDirectoryName(CharacterObject.FileName)
                            ?? Path.GetPathRoot(CharacterObject.FileName), strFileName);
                        objNewWatcher.Changed += LiveUpdateFromCharacterFile;
                    }
                }
                Interlocked.Exchange(ref _objCharacterFileWatcher, objNewWatcher)?.Dispose();
            }

            if (e.PropertyNames.Contains(nameof(Character.Settings)))
            {
                _objCachedSettings = null;
                try
                {
                    await MakeDirtyWithCharacterUpdate(token).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    //swallow this
                }
            }
        }

        /// <summary>
        /// Set up data bindings to set Dirty flag and/or the flag to request a character update when specific collections change
        /// </summary>
        /// <param name="blnAddBindings"></param>
        protected void SetupCommonCollectionDatabindings(bool blnAddBindings)
        {
            if (blnAddBindings)
            {
                CharacterObject.Spells.CollectionChangedAsync += MakeDirtyWithCharacterUpdate;
                CharacterObject.ComplexForms.CollectionChangedAsync += MakeDirtyWithCharacterUpdate;
                CharacterObject.Arts.CollectionChangedAsync += MakeDirtyWithCharacterUpdate;
                CharacterObject.Enhancements.CollectionChangedAsync += MakeDirtyWithCharacterUpdate;
                CharacterObject.Metamagics.CollectionChangedAsync += MakeDirtyWithCharacterUpdate;
                CharacterObject.InitiationGrades.CollectionChangedAsync += MakeDirtyWithCharacterUpdate;
                CharacterObject.Powers.ListChangedAsync += MakeDirtyWithCharacterUpdate;
                CharacterObject.AIPrograms.CollectionChangedAsync += MakeDirtyWithCharacterUpdate;
                CharacterObject.CritterPowers.CollectionChangedAsync += MakeDirtyWithCharacterUpdate;
                CharacterObject.Qualities.CollectionChangedAsync += MakeDirtyWithCharacterUpdate;
                CharacterObject.MartialArts.CollectionChangedAsync += MakeDirtyWithCharacterUpdate;
                CharacterObject.Lifestyles.CollectionChangedAsync += MakeDirtyWithCharacterUpdate;
                CharacterObject.Contacts.CollectionChangedAsync += MakeDirtyWithCharacterUpdate;
                CharacterObject.Spirits.CollectionChangedAsync += MakeDirtyWithCharacterUpdate;
                CharacterObject.Armor.CollectionChangedAsync += MakeDirtyWithCharacterUpdate;
                CharacterObject.ArmorLocations.CollectionChangedAsync += MakeDirty;
                CharacterObject.Weapons.CollectionChangedAsync += MakeDirtyWithCharacterUpdate;
                CharacterObject.WeaponLocations.CollectionChangedAsync += MakeDirty;
                CharacterObject.Gear.CollectionChangedAsync += MakeDirtyWithCharacterUpdate;
                CharacterObject.GearLocations.CollectionChangedAsync += MakeDirty;
                CharacterObject.Drugs.CollectionChangedAsync += MakeDirtyWithCharacterUpdate;
                CharacterObject.Cyberware.CollectionChangedAsync += MakeDirtyWithCharacterUpdate;
                CharacterObject.Vehicles.CollectionChangedAsync += MakeDirtyWithCharacterUpdate;
                CharacterObject.VehicleLocations.CollectionChangedAsync += MakeDirty;

                CharacterObject.Improvements.CollectionChangedAsync += MakeDirtyWithCharacterUpdate;
                CharacterObject.ImprovementGroups.CollectionChangedAsync += MakeDirty;
                CharacterObject.Calendar.ListChangedAsync += MakeDirty;
                CharacterObject.SustainedCollection.CollectionChangedAsync += MakeDirty;
                CharacterObject.ExpenseEntries.CollectionChangedAsync += MakeDirtyWithCharacterUpdate;
                CharacterObject.LimitModifiers.CollectionChangedAsync += MakeDirtyWithCharacterUpdate;
            }
            else
            {
                CharacterObject.Spells.CollectionChangedAsync -= MakeDirtyWithCharacterUpdate;
                CharacterObject.ComplexForms.CollectionChangedAsync -= MakeDirtyWithCharacterUpdate;
                CharacterObject.Arts.CollectionChangedAsync -= MakeDirtyWithCharacterUpdate;
                CharacterObject.Enhancements.CollectionChangedAsync -= MakeDirtyWithCharacterUpdate;
                CharacterObject.Metamagics.CollectionChangedAsync -= MakeDirtyWithCharacterUpdate;
                CharacterObject.InitiationGrades.CollectionChangedAsync -= MakeDirtyWithCharacterUpdate;
                CharacterObject.Powers.ListChangedAsync -= MakeDirtyWithCharacterUpdate;
                CharacterObject.AIPrograms.CollectionChangedAsync -= MakeDirtyWithCharacterUpdate;
                CharacterObject.CritterPowers.CollectionChangedAsync -= MakeDirtyWithCharacterUpdate;
                CharacterObject.Qualities.CollectionChangedAsync -= MakeDirtyWithCharacterUpdate;
                CharacterObject.MartialArts.CollectionChangedAsync -= MakeDirtyWithCharacterUpdate;
                CharacterObject.Lifestyles.CollectionChangedAsync -= MakeDirtyWithCharacterUpdate;
                CharacterObject.Contacts.CollectionChangedAsync -= MakeDirtyWithCharacterUpdate;
                CharacterObject.Spirits.CollectionChangedAsync -= MakeDirtyWithCharacterUpdate;
                CharacterObject.Armor.CollectionChangedAsync -= MakeDirtyWithCharacterUpdate;
                CharacterObject.ArmorLocations.CollectionChangedAsync -= MakeDirty;
                CharacterObject.Weapons.CollectionChangedAsync -= MakeDirtyWithCharacterUpdate;
                CharacterObject.WeaponLocations.CollectionChangedAsync -= MakeDirty;
                CharacterObject.Gear.CollectionChangedAsync -= MakeDirtyWithCharacterUpdate;
                CharacterObject.GearLocations.CollectionChangedAsync -= MakeDirty;
                CharacterObject.Drugs.CollectionChangedAsync -= MakeDirtyWithCharacterUpdate;
                CharacterObject.Cyberware.CollectionChangedAsync -= MakeDirtyWithCharacterUpdate;
                CharacterObject.Vehicles.CollectionChangedAsync -= MakeDirtyWithCharacterUpdate;
                CharacterObject.VehicleLocations.CollectionChangedAsync -= MakeDirty;

                CharacterObject.Improvements.CollectionChangedAsync -= MakeDirtyWithCharacterUpdate;
                CharacterObject.ImprovementGroups.CollectionChangedAsync -= MakeDirty;
                CharacterObject.Calendar.ListChangedAsync -= MakeDirty;
                CharacterObject.SustainedCollection.CollectionChangedAsync -= MakeDirty;
                CharacterObject.ExpenseEntries.CollectionChangedAsync -= MakeDirtyWithCharacterUpdate;
                CharacterObject.LimitModifiers.CollectionChangedAsync -= MakeDirtyWithCharacterUpdate;
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

        protected Stopwatch AutosaveStopwatch => _stpAutosaveStopwatch;

        private System.Timers.Timer _tmrAutosaveRequestTimer = new System.Timers.Timer(1000 * 300);

        private DebuggableSemaphoreSlim _objAutosaveSemaphore = new DebuggableSemaphoreSlim();

        private int _intAutosaveTimeoutsCount;

        private const int MaximumAutosaveTimeouts = 3;

        /// <summary>
        /// Automatically Save the character to a backup folder.
        /// </summary>
        protected async Task AutoSaveCharacter(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            // If we have timed out more than we are allowed to, stop autosaving
            if (_intAutosaveTimeoutsCount >= MaximumAutosaveTimeouts)
                return;
            // Local for thread safety
            DebuggableSemaphoreSlim objAutosaveSemaphore = _objAutosaveSemaphore;
            try
            {
                if (objAutosaveSemaphore == null || !await objAutosaveSemaphore.WaitAsync(0, token).ConfigureAwait(false))
                    return;
            }
            catch (ObjectDisposedException)
            {
                return;
            }

            try
            {
                _tmrAutosaveRequestTimer?.Stop();
                CursorWait objCursorWait = await CursorWait.NewAsync(this, true, token).ConfigureAwait(false);
                try
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
                                await Program.ShowScrollableMessageBoxAsync(
                                    this,
                                    await LanguageManager
                                        .GetStringAsync("Message_Insufficient_Permissions_Warning", token: token)
                                        .ConfigureAwait(false), token: token).ConfigureAwait(false);
                                _intAutosaveTimeoutsCount = MaximumAutosaveTimeouts; // Stop trying to autosave because we aren't going to be granted permissions until a restart
                                return;
                            }
                        }

                        IAsyncDisposable objLocker = await CharacterObject.LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
                        try
                        {
                            string strCharacterName = await CharacterObject.GetCharacterNameAsync(token)
                                .ConfigureAwait(false);
                            string strAutosaveFileName =
                                Path.GetFileNameWithoutExtension(await CharacterObject.GetFileNameAsync(token)
                                    .ConfigureAwait(false));

                            // Autosaves are uncompressed to make them happen as fast as possible
                            string strExtension = ".chum5";
                            if (string.IsNullOrEmpty(strAutosaveFileName))
                                strAutosaveFileName = strCharacterName.CleanForFileName() + strExtension;
                            else
                                strAutosaveFileName += strExtension;

                            string strFilePath = Path.Combine(strAutosavePath, strAutosaveFileName);
                            using (CancellationTokenSource objTimeoutSource =
                                   new CancellationTokenSource(TimeSpan.FromMinutes(1.0)))
                            {
                                CancellationToken objTimeoutToken = objTimeoutSource.Token;
                                using (CancellationTokenSource objJoinedSource =
                                       CancellationTokenSource.CreateLinkedTokenSource(objTimeoutToken, token))
                                {
                                    try
                                    {
                                        if (!await CharacterObject
                                                .SaveAsync(strFilePath, false, false, token: objJoinedSource.Token)
                                                .ConfigureAwait(false))
                                        {
                                            Log.Info("Autosave failed for character " + strCharacterName + " (" +
                                                     strAutosaveFileName + ')');
                                        }
                                        else
                                            _intAutosaveTimeoutsCount =
                                                0; // We have successfully autosaved once, stop timing out
                                    }
                                    catch (OperationCanceledException)
                                    {
                                        if (objTimeoutSource.IsCancellationRequested)
                                        {
                                            int intAutosaveTimeoutsCount =
                                                Interlocked.Increment(ref _intAutosaveTimeoutsCount);
                                            if (!token.IsCancellationRequested)
                                            {
                                                if (intAutosaveTimeoutsCount >= MaximumAutosaveTimeouts)
                                                {
                                                    Log.Error("Autosave timed out too many times for character " +
                                                              strCharacterName + " (" + strAutosaveFileName + ')');
                                                }
                                                else
                                                {
                                                    Log.Info("Autosave timed out for character " + strCharacterName +
                                                             " (" + strAutosaveFileName + ')');
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        finally
                        {
                            await objLocker.DisposeAsync().ConfigureAwait(false);
                        }
                    }
                    finally
                    {
                        AutosaveStopwatch?.Restart();
                    }
                }
                finally
                {
                    await objCursorWait.DisposeAsync().ConfigureAwait(false);
                }
            }
            finally
            {
                _tmrAutosaveRequestTimer?.Start();
                objAutosaveSemaphore.Release();
            }
        }

        /// <summary>
        /// Edit and update a Limit Modifier.
        /// </summary>
        /// <param name="treLimit"></param>
        /// <param name="token"></param>
        protected async Task UpdateLimitModifier(TreeView treLimit, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (treLimit == null)
                return;
            TreeNode objSelectedNode = await treLimit.DoThreadSafeFuncAsync(x => x.SelectedNode, token).ConfigureAwait(false);
            if (objSelectedNode == null || objSelectedNode.Level == 0)
                return;
            CursorWait objCursorWait = await CursorWait.NewAsync(this, token: token).ConfigureAwait(false);
            SkipUpdate = true;
            try
            {
                string strGuid = (objSelectedNode.Tag as IHasInternalId)?.InternalId ?? string.Empty;
                if (string.IsNullOrEmpty(strGuid) || strGuid.IsEmptyGuid())
                    return;
                LimitModifier objLimitModifier = CharacterObject.LimitModifiers.FindById(strGuid);
                //If the LimitModifier couldn't be found (Ie it comes from an Improvement or the user hasn't properly selected a treenode, fail out early.
                if (objLimitModifier == null)
                {
                    await Program.ShowScrollableMessageBoxAsync(this, await LanguageManager.GetStringAsync("Warning_NoLimitFound", token: token).ConfigureAwait(false), token: token).ConfigureAwait(false);
                    return;
                }

                using (ThreadSafeForm<SelectLimitModifier> frmPickLimitModifier =
                       await ThreadSafeForm<SelectLimitModifier>.GetAsync(
                           () => new SelectLimitModifier(objLimitModifier, "Physical", "Mental", "Social"), token).ConfigureAwait(false))
                {
                    if (await frmPickLimitModifier.ShowDialogSafeAsync(this, token).ConfigureAwait(false) == DialogResult.Cancel)
                        return;

                    //Remove the old LimitModifier to ensure we don't double up.
                    await CharacterObject.LimitModifiers.RemoveAsync(objLimitModifier, token).ConfigureAwait(false);
                    // Create the new limit modifier.
                    LimitModifier objNewLimitModifier = new LimitModifier(CharacterObject, strGuid);
                    objNewLimitModifier.Create(frmPickLimitModifier.MyForm.SelectedName,
                                               frmPickLimitModifier.MyForm.SelectedBonus,
                                               frmPickLimitModifier.MyForm.SelectedLimitType,
                                               frmPickLimitModifier.MyForm.SelectedCondition, true);

                    await CharacterObject.LimitModifiers.AddAsync(objNewLimitModifier, token).ConfigureAwait(false);
                }
            }
            finally
            {
                SkipUpdate = false;
                await objCursorWait.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Edit the notes of an item tagged to a tree node
        /// </summary>
        /// <param name="treNode"></param>
        /// <param name="token"></param>
        protected async Task WriteNotes(TreeNode treNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (!(treNode?.Tag is IHasNotes objNotes))
                return;
            CursorWait objCursorWait = await CursorWait.NewAsync(this, token: token).ConfigureAwait(false);
            SkipUpdate = true;
            try
            {
                using (ThreadSafeForm<EditNotes> frmItemNotes =
                       await ThreadSafeForm<EditNotes>.GetAsync(
                           () => new EditNotes(objNotes.Notes, objNotes.NotesColor, token), token).ConfigureAwait(false))
                {
                    if (await frmItemNotes.ShowDialogSafeAsync(this, token).ConfigureAwait(false) != DialogResult.OK)
                        return;
                    objNotes.Notes = frmItemNotes.MyForm.Notes;
                    objNotes.NotesColor = frmItemNotes.MyForm.NotesColor;
                    await SetDirty(true, token).ConfigureAwait(false);
                    TreeView objTreeView = treNode.TreeView;
                    if (objTreeView != null)
                    {
                        await objTreeView.DoThreadSafeAsync(() =>
                        {
                            treNode.ForeColor = objNotes.PreferredColor;
                            treNode.ToolTipText = objNotes.Notes.WordWrap();
                        }, token).ConfigureAwait(false);
                    }
                    else
                    {
                        treNode.ForeColor = objNotes.PreferredColor;
                        treNode.ToolTipText = objNotes.Notes.WordWrap();
                    }
                }
            }
            finally
            {
                SkipUpdate = false;
                await objCursorWait.DisposeAsync().ConfigureAwait(false);
            }
        }

        #region Refresh Treeviews and Panels

        protected Task RefreshAttributesClearBindings(FlowLayoutPanel pnlAttributes, CancellationToken token = default)
        {
            return pnlAttributes.DoThreadSafeAsync(x =>
            {
                foreach (AttributeControl objControl in x.Controls)
                {
                    objControl.ValueChanged -= MakeDirtyWithCharacterUpdate;
                }
            }, token);
        }

        protected async Task RefreshAttributes(FlowLayoutPanel pnlAttributes, NotifyCollectionChangedEventArgs e = null, Label lblName = null, int intKarmaWidth = -1, int intValueWidth = -1, int intLimitsWidth = -1, CancellationToken token = default)
        {
            if (pnlAttributes == null)
                return;
            CursorWait objCursorWait = await CursorWait.NewAsync(this, token: token).ConfigureAwait(false);
            SkipUpdate = true;
            try
            {
                if (e == null ||
                    e.Action == NotifyCollectionChangedAction.Reset)
                {
                    ThreadSafeObservableCollection<CharacterAttrib> lstAttributes =
                        await (await CharacterObject.GetAttributeSectionAsync(token).ConfigureAwait(false)).GetAttributesAsync(token).ConfigureAwait(false);
                    IAsyncDisposable objLocker = await lstAttributes.LockObject.EnterReadLockAsync(token)
                        .ConfigureAwait(false);
                    try
                    {
                        token.ThrowIfCancellationRequested();
                        int intAttributeCount = await lstAttributes.GetCountAsync(token).ConfigureAwait(false);
                        if (intAttributeCount <= 0)
                        {
                            await pnlAttributes.DoThreadSafeAsync(x =>
                            {
                                x.SuspendLayout();
                                try
                                {
                                    x.Controls.Clear();
                                }
                                finally
                                {
                                    x.ResumeLayout();
                                }
                            }, token).ConfigureAwait(false);

                            return;
                        }

                        await pnlAttributes.DoThreadSafeAsync(x =>
                        {
                            x.SuspendLayout();
                            try
                            {
                                x.Controls.Clear();
                                int intNameWidth = lblName?.PreferredWidth ?? 0;
                                Control[] aobjControls
                                    = new Control[intAttributeCount];
                                for (int i = 0; i < intAttributeCount; ++i)
                                {
                                    AttributeControl objControl =
                                        new AttributeControl(lstAttributes[i], GenericToken);
                                    objControl.MinimumSize
                                        = new Size(x.ClientSize.Width, objControl.MinimumSize.Height);
                                    objControl.MaximumSize
                                        = new Size(x.ClientSize.Width, objControl.MaximumSize.Height);
                                    objControl.ValueChanged += MakeDirtyWithCharacterUpdate;
                                    intNameWidth = Math.Max(intNameWidth, objControl.NameWidth);
                                    aobjControls[i] = objControl;
                                }

                                if (lblName != null)
                                    lblName.MinimumSize = new Size(intNameWidth, lblName.MinimumSize.Height);
                                foreach (AttributeControl objControl in aobjControls.OfType<AttributeControl>())
                                    objControl.UpdateWidths(intNameWidth, intKarmaWidth, intValueWidth,
                                        intLimitsWidth, token);
                                x.Controls.AddRange(aobjControls);
                            }
                            finally
                            {
                                x.ResumeLayout();
                            }
                        }, token).ConfigureAwait(false);
                    }
                    finally
                    {
                        await objLocker.DisposeAsync().ConfigureAwait(false);
                    }
                }
                else
                {
                    switch (e.Action)
                    {
                        case NotifyCollectionChangedAction.Add:
                        {
                            int intNewNameWidth = -1;
                            Control[] aobjControls = new Control[e.NewItems.Count];
                            await pnlAttributes.DoThreadSafeAsync(x =>
                            {
                                for (int i = 0; i < e.NewItems.Count; ++i)
                                {
                                    AttributeControl objControl =
                                        new AttributeControl(
                                            e.NewItems[i] as CharacterAttrib, GenericToken);
                                    objControl.MinimumSize = new Size(x.ClientSize.Width,
                                                                      objControl.MinimumSize.Height);
                                    objControl.MaximumSize = new Size(x.ClientSize.Width,
                                                                      objControl.MaximumSize.Height);
                                    objControl.ValueChanged += MakeDirtyWithCharacterUpdate;
                                    intNewNameWidth = Math.Max(intNewNameWidth, objControl.NameWidth);
                                    aobjControls[i] = objControl;
                                }

                                int intOldNameWidth = 0;
                                if (lblName != null)
                                    intOldNameWidth = lblName.Width;
                                else
                                {
                                    foreach (Control objControl in x.Controls)
                                    {
                                        if (objControl is AttributeControl objAttributeControl)
                                        {
                                            intOldNameWidth = objAttributeControl.NameWidth;
                                            break;
                                        }
                                    }
                                }

                                if (intNewNameWidth > intOldNameWidth)
                                {
                                    if (lblName != null)
                                        lblName.MinimumSize = new Size(intNewNameWidth, lblName.MinimumSize.Height);
                                    x.Controls.AddRange(aobjControls);
                                    foreach (AttributeControl objControl in x.Controls)
                                        objControl.UpdateWidths(intNewNameWidth, intKarmaWidth, intValueWidth,
                                                                intLimitsWidth, token);
                                }
                                else
                                {
                                    foreach (AttributeControl objControl in aobjControls.OfType<AttributeControl>())
                                        objControl.UpdateWidths(intOldNameWidth, intKarmaWidth, intValueWidth,
                                                                intLimitsWidth, token);
                                    x.Controls.AddRange(aobjControls);
                                }
                            }, token).ConfigureAwait(false);
                            break;
                        }

                        case NotifyCollectionChangedAction.Remove:
                        {
                            foreach (CharacterAttrib objAttrib in e.OldItems)
                            {
                                IAsyncDisposable objLocker =
                                    await objAttrib.LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
                                try
                                {
                                    token.ThrowIfCancellationRequested();
                                    await pnlAttributes.DoThreadSafeAsync(x =>
                                    {
                                        foreach (AttributeControl objControl in x.Controls)
                                        {
                                            if (objControl.AttributeName == objAttrib.Abbrev)
                                            {
                                                objControl.ValueChanged -= MakeDirtyWithCharacterUpdate;
                                                x.Controls.Remove(objControl);
                                                objControl.Dispose();
                                            }
                                        }
                                    }, token).ConfigureAwait(false);

                                    if (!await CharacterObject.GetCreatedAsync(token).ConfigureAwait(false))
                                    {
                                        await objAttrib.SetBaseAsync(0, token).ConfigureAwait(false);
                                        await objAttrib.SetKarmaAsync(0, token).ConfigureAwait(false);
                                    }
                                }
                                finally
                                {
                                    await objLocker.DisposeAsync().ConfigureAwait(false);
                                }
                            }

                            break;
                        }

                        case NotifyCollectionChangedAction.Replace:
                        {
                            foreach (CharacterAttrib objAttrib in e.OldItems)
                            {
                                IAsyncDisposable objLocker =
                                    await objAttrib.LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
                                try
                                {
                                    token.ThrowIfCancellationRequested();
                                    await pnlAttributes.DoThreadSafeAsync(x =>
                                    {
                                        foreach (AttributeControl objControl in x.Controls)
                                        {
                                            if (objControl.AttributeName == objAttrib.Abbrev)
                                            {
                                                objControl.ValueChanged -= MakeDirtyWithCharacterUpdate;
                                                x.Controls.Remove(objControl);
                                                objControl.Dispose();
                                            }
                                        }
                                    }, token).ConfigureAwait(false);

                                    if (!await CharacterObject.GetCreatedAsync(token).ConfigureAwait(false))
                                    {
                                        await objAttrib.SetBaseAsync(0, token).ConfigureAwait(false);
                                        await objAttrib.SetKarmaAsync(0, token).ConfigureAwait(false);
                                    }
                                }
                                finally
                                {
                                    await objLocker.DisposeAsync().ConfigureAwait(false);
                                }
                            }

                            int intNewNameWidth = -1;
                            Control[] aobjControls = new Control[e.NewItems.Count];
                            await pnlAttributes.DoThreadSafeAsync(x =>
                            {
                                for (int i = 0; i < e.NewItems.Count; ++i)
                                {
                                    AttributeControl objControl =
                                        new AttributeControl(
                                            e.NewItems[i] as CharacterAttrib, GenericToken);
                                    objControl.MinimumSize = new Size(x.ClientSize.Width,
                                                                      objControl.MinimumSize.Height);
                                    objControl.MaximumSize = new Size(x.ClientSize.Width,
                                                                      objControl.MaximumSize.Height);
                                    objControl.ValueChanged += MakeDirtyWithCharacterUpdate;
                                    intNewNameWidth = Math.Max(intNewNameWidth, objControl.NameWidth);
                                    aobjControls[i] = objControl;
                                }

                                int intOldNameWidth = 0;
                                if (lblName != null)
                                    intOldNameWidth = lblName.Width;
                                else
                                {
                                    foreach (Control objControl in x.Controls)
                                    {
                                        if (objControl is AttributeControl objAttributeControl)
                                        {
                                            intOldNameWidth = objAttributeControl.NameWidth;
                                            break;
                                        }
                                    }
                                }

                                if (intNewNameWidth > intOldNameWidth)
                                {
                                    if (lblName != null)
                                        lblName.MinimumSize = new Size(intNewNameWidth, lblName.MinimumSize.Height);
                                    x.Controls.AddRange(aobjControls);
                                    foreach (AttributeControl objControl in x.Controls)
                                        objControl.UpdateWidths(intNewNameWidth, intKarmaWidth, intValueWidth,
                                                                intLimitsWidth, token);
                                }
                                else
                                {
                                    foreach (AttributeControl objControl in aobjControls.OfType<AttributeControl>())
                                        objControl.UpdateWidths(intOldNameWidth, intKarmaWidth, intValueWidth,
                                                                intLimitsWidth, token);
                                    x.Controls.AddRange(aobjControls);
                                }
                            }, token).ConfigureAwait(false);
                            break;
                        }
                    }
                }
            }
            finally
            {
                SkipUpdate = false;
                await objCursorWait.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Clears and updates the TreeView for Spells. Typically called as part of AddQuality or UpdateCharacterInfo.
        /// </summary>
        /// <param name="treSpells">Spells tree.</param>
        /// <param name="treMetamagic">Initiations tree.</param>
        /// <param name="cmsSpell">ContextMenuStrip that will be added to spells in the spell tree.</param>
        /// <param name="cmsInitiationNotes">ContextMenuStrip that will be added to spells in the initiations tree.</param>
        /// <param name="e">Arguments for the change to the underlying ObservableCollection.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        protected async Task RefreshSpells(TreeView treSpells, TreeView treMetamagic, ContextMenuStrip cmsSpell, ContextMenuStrip cmsInitiationNotes, NotifyCollectionChangedEventArgs e = null, CancellationToken token = default)
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
            CursorWait objCursorWait = await CursorWait.NewAsync(this, token: token).ConfigureAwait(false);
            SkipUpdate = true;
            try
            {
                if (e == null ||
                    e.Action == NotifyCollectionChangedAction.Reset)
                {
                    string strSelectedId
                        = (await treSpells.DoThreadSafeFuncAsync(x => x.SelectedNode?.Tag, token).ConfigureAwait(false) as
                            IHasInternalId)?.InternalId ?? string.Empty;
                    string strSelectedMetamagicId =
                        treMetamagic != null
                            ? (await treMetamagic.DoThreadSafeFuncAsync(x => x.SelectedNode?.Tag, token).ConfigureAwait(false) as
                                IHasInternalId)?.InternalId ?? string.Empty
                            : string.Empty;

                    // Clear the default nodes of entries.
                    await treSpells.DoThreadSafeAsync(x => x.Nodes.Clear(), token).ConfigureAwait(false);

                    // Add the Spells that exist.
                    await CharacterObject.Spells.ForEachAsync(async objSpell =>
                    {
                        if (objSpell.Grade > 0 && treMetamagic != null)
                        {
                            await treMetamagic.DoThreadSafeAsync(x => x.FindNodeByTag(objSpell)?.Remove(),
                                                                 token).ConfigureAwait(false);
                        }

                        await AddToTree(objSpell, false).ConfigureAwait(false);
                    }, token).ConfigureAwait(false);

                    await treSpells.DoThreadSafeAsync(x => x.SortCustomAlphabetically(strSelectedId), token).ConfigureAwait(false);
                    if (treMetamagic != null)
                        await treMetamagic.DoThreadSafeAsync(x => x.SelectedNode = x.FindNode(strSelectedMetamagicId),
                                                             token).ConfigureAwait(false);
                }
                else
                {
                    await treSpells.DoThreadSafeAsync(x =>
                    {
                        objCombatNode = x.FindNode("Node_SelectedCombatSpells", false);
                        objDetectionNode = x.FindNode("Node_SelectedDetectionSpells", false);
                        objHealthNode = x.FindNode("Node_SelectedHealthSpells", false);
                        objIllusionNode = x.FindNode("Node_SelectedIllusionSpells", false);
                        objManipulationNode = x.FindNode("Node_SelectedManipulationSpells", false);
                        objRitualsNode = x.FindNode("Node_SelectedGeomancyRituals", false);
                        objEnchantmentsNode = x.FindNode("Node_SelectedEnchantments", false);
                    }, token).ConfigureAwait(false);
                    switch (e.Action)
                    {
                        case NotifyCollectionChangedAction.Add:
                        {
                            foreach (Spell objSpell in e.NewItems)
                            {
                                await AddToTree(objSpell).ConfigureAwait(false);
                            }

                            break;
                        }
                        case NotifyCollectionChangedAction.Remove:
                        {
                            foreach (Spell objSpell in e.OldItems)
                            {
                                await treSpells.DoThreadSafeAsync(x =>
                                {
                                    TreeNode objNode = x.FindNodeByTag(objSpell);
                                    if (objNode != null)
                                    {
                                        TreeNode objParent = objNode.Parent;
                                        objNode.Remove();
                                        if (objParent.Level == 0 && objParent.Nodes.Count == 0)
                                            objParent.Remove();
                                    }
                                }, token).ConfigureAwait(false);

                                if (objSpell.Grade > 0 && treMetamagic != null)
                                {
                                    await treMetamagic.DoThreadSafeAsync(
                                        x => x.FindNodeByTag(objSpell)?.Remove(), token).ConfigureAwait(false);
                                }
                            }

                            break;
                        }
                        case NotifyCollectionChangedAction.Replace:
                        {
                            List<TreeNode> lstOldParents =
                                new List<TreeNode>(e.OldItems.Count);
                            foreach (Spell objSpell in e.OldItems)
                            {
                                await treSpells.DoThreadSafeAsync(x =>
                                {
                                    TreeNode objNode = x.FindNodeByTag(objSpell);
                                    if (objNode != null)
                                    {
                                        lstOldParents.Add(objNode.Parent);
                                        objNode.Remove();
                                    }
                                }, token).ConfigureAwait(false);

                                if (objSpell.Grade > 0 && treMetamagic != null)
                                {
                                    await treMetamagic.DoThreadSafeAsync(
                                        x => x.FindNodeByTag(objSpell)?.Remove(), token).ConfigureAwait(false);
                                }
                            }

                            foreach (Spell objSpell in e.NewItems)
                            {
                                await AddToTree(objSpell).ConfigureAwait(false);
                            }

                            await treSpells.DoThreadSafeAsync(() =>
                            {
                                foreach (TreeNode objOldParent in lstOldParents)
                                {
                                    if (objOldParent.Level == 0 && objOldParent.Nodes.Count == 0)
                                        objOldParent.Remove();
                                }
                            }, token).ConfigureAwait(false);

                            break;
                        }
                    }
                }
            }
            finally
            {
                SkipUpdate = false;
                await objCursorWait.DisposeAsync().ConfigureAwait(false);
            }

            async Task AddToTree(Spell objSpell, bool blnSingleAdd = true)
            {
                TreeNode objNode = objSpell.CreateTreeNode(cmsSpell);
                if (objNode == null)
                    return;
                TreeNode objParentNode;
                switch (objSpell.Category)
                {
                    case "Combat":
                        if (objCombatNode == null)
                        {
                            objCombatNode = new TreeNode
                            {
                                Tag = "Node_SelectedCombatSpells",
                                Text = await LanguageManager.GetStringAsync("Node_SelectedCombatSpells", token: token).ConfigureAwait(false)
                            };
                            await treSpells.DoThreadSafeAsync(x =>
                            {
                                // ReSharper disable once AssignNullToNotNullAttribute
                                x.Nodes.Insert(0, objCombatNode);
                                objCombatNode.Expand();
                            }, token).ConfigureAwait(false);
                        }
                        objParentNode = objCombatNode;
                        break;

                    case "Detection":
                        if (objDetectionNode == null)
                        {
                            objDetectionNode = new TreeNode
                            {
                                Tag = "Node_SelectedDetectionSpells",
                                Text = await LanguageManager.GetStringAsync("Node_SelectedDetectionSpells", token: token).ConfigureAwait(false)
                            };
                            await treSpells.DoThreadSafeAsync(x =>
                            {
                                // ReSharper disable once AssignNullToNotNullAttribute
                                x.Nodes.Insert((objCombatNode != null).ToInt32(), objDetectionNode);
                                objDetectionNode.Expand();
                            }, token).ConfigureAwait(false);
                        }
                        objParentNode = objDetectionNode;
                        break;

                    case "Health":
                        if (objHealthNode == null)
                        {
                            objHealthNode = new TreeNode
                            {
                                Tag = "Node_SelectedHealthSpells",
                                Text = await LanguageManager.GetStringAsync("Node_SelectedHealthSpells", token: token).ConfigureAwait(false)
                            };
                            await treSpells.DoThreadSafeAsync(x =>
                            {
                                x.Nodes.Insert((objCombatNode != null).ToInt32() +
                                               // ReSharper disable once AssignNullToNotNullAttribute
                                               (objDetectionNode != null).ToInt32(), objHealthNode);
                                objHealthNode.Expand();
                            }, token).ConfigureAwait(false);
                        }
                        objParentNode = objHealthNode;
                        break;

                    case "Illusion":
                        if (objIllusionNode == null)
                        {
                            objIllusionNode = new TreeNode
                            {
                                Tag = "Node_SelectedIllusionSpells",
                                Text = await LanguageManager.GetStringAsync("Node_SelectedIllusionSpells", token: token).ConfigureAwait(false)
                            };
                            await treSpells.DoThreadSafeAsync(x =>
                            {
                                x.Nodes.Insert((objCombatNode != null).ToInt32() +
                                               (objDetectionNode != null).ToInt32() +
                                               // ReSharper disable once AssignNullToNotNullAttribute
                                               (objHealthNode != null).ToInt32(), objIllusionNode);
                                objIllusionNode.Expand();
                            }, token).ConfigureAwait(false);
                        }
                        objParentNode = objIllusionNode;
                        break;

                    case "Manipulation":
                        if (objManipulationNode == null)
                        {
                            objManipulationNode = new TreeNode
                            {
                                Tag = "Node_SelectedManipulationSpells",
                                Text = await LanguageManager.GetStringAsync("Node_SelectedManipulationSpells", token: token).ConfigureAwait(false)
                            };
                            await treSpells.DoThreadSafeAsync(x =>
                            {
                                x.Nodes.Insert((objCombatNode != null).ToInt32() +
                                               (objDetectionNode != null).ToInt32() +
                                               (objHealthNode != null).ToInt32() +
                                               // ReSharper disable once AssignNullToNotNullAttribute
                                               (objIllusionNode != null).ToInt32(), objManipulationNode);
                                objManipulationNode.Expand();
                            }, token).ConfigureAwait(false);
                        }
                        objParentNode = objManipulationNode;
                        break;

                    case "Rituals":
                        if (objRitualsNode == null)
                        {
                            objRitualsNode = new TreeNode
                            {
                                Tag = "Node_SelectedGeomancyRituals",
                                Text = await LanguageManager.GetStringAsync("Node_SelectedGeomancyRituals", token: token).ConfigureAwait(false)
                            };
                            await treSpells.DoThreadSafeAsync(x =>
                            {
                                x.Nodes.Insert((objCombatNode != null).ToInt32() +
                                               (objDetectionNode != null).ToInt32() +
                                               (objHealthNode != null).ToInt32() +
                                               (objIllusionNode != null).ToInt32() +
                                               // ReSharper disable once AssignNullToNotNullAttribute
                                               (objManipulationNode != null).ToInt32(), objRitualsNode);
                                objRitualsNode.Expand();
                            }, token).ConfigureAwait(false);
                        }
                        objParentNode = objRitualsNode;
                        break;

                    case "Enchantments":
                        if (objEnchantmentsNode == null)
                        {
                            objEnchantmentsNode = new TreeNode
                            {
                                Tag = "Node_SelectedEnchantments",
                                Text = await LanguageManager.GetStringAsync("Node_SelectedEnchantments", token: token).ConfigureAwait(false)
                            };
                            await treSpells.DoThreadSafeAsync(x =>
                            {
                                // ReSharper disable once AssignNullToNotNullAttribute
                                x.Nodes.Add(objEnchantmentsNode);
                                objEnchantmentsNode.Expand();
                            }, token).ConfigureAwait(false);
                        }
                        objParentNode = objEnchantmentsNode;
                        break;
                    default:
                        TreeNode objSpellNode = treSpells.FindNodeByTag(objSpell.Category);
                        if (objSpellNode == null)
                        {
                            objSpellNode = new TreeNode
                            {
                                Tag = objSpell.Category,
                                Text = await objSpell.DisplayCategoryAsync(GlobalSettings.Language, token).ConfigureAwait(false)
                            };
                            await treSpells.DoThreadSafeAsync(x =>
                            {
                                // ReSharper disable once AssignNullToNotNullAttribute
                                x.Nodes.Add(objSpellNode);
                                objSpellNode.Expand();
                            }, token).ConfigureAwait(false);
                        }
                        objParentNode = objSpellNode;
                        break;
                }
                if (objSpell.Grade > 0)
                {
                    InitiationGrade objGrade = await CharacterObject.InitiationGrades.FirstOrDefaultAsync(x => x.Grade == objSpell.Grade, token).ConfigureAwait(false);
                    if (objGrade != null && treMetamagic != null)
                    {
                        await treMetamagic.DoThreadSafeAsync(x =>
                        {
                            TreeNode nodMetamagicParent = x.FindNodeByTag(objGrade);
                            if (nodMetamagicParent != null)
                            {
                                TreeNodeCollection nodMetamagicParentChildren = nodMetamagicParent.Nodes;
                                TreeNode objMetamagicNode = objSpell.CreateTreeNode(cmsInitiationNotes, true);
                                int intNodesCount = nodMetamagicParentChildren.Count;
                                int intTargetIndex = 0;
                                for (; intTargetIndex < intNodesCount; ++intTargetIndex)
                                {
                                    if (CompareTreeNodes.CompareText(nodMetamagicParentChildren[intTargetIndex],
                                                                     objMetamagicNode) >= 0)
                                    {
                                        break;
                                    }
                                }

                                nodMetamagicParentChildren.Insert(intTargetIndex, objMetamagicNode);
                                if (blnSingleAdd)
                                    x.SelectedNode = objMetamagicNode;
                            }
                        }, token).ConfigureAwait(false);
                    }
                }

                if (objParentNode == null)
                    return;
                await treSpells.DoThreadSafeAsync(x =>
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
                        x.SelectedNode = objNode;
                    }
                    else
                        objParentNode.Nodes.Add(objNode);
                }, token).ConfigureAwait(false);
            }
        }

        protected async Task RefreshAIPrograms(TreeView treAIPrograms, ContextMenuStrip cmsAdvancedProgram, NotifyCollectionChangedEventArgs e = null, CancellationToken token = default)
        {
            if (treAIPrograms == null)
                return;
            TreeNode objParentNode = null;
            CursorWait objCursorWait = await CursorWait.NewAsync(this, token: token).ConfigureAwait(false);
            SkipUpdate = true;
            try
            {
                if (e == null ||
                    e.Action == NotifyCollectionChangedAction.Reset)
                {
                    string strSelectedId =
                        (await treAIPrograms.DoThreadSafeFuncAsync(x => x.SelectedNode?.Tag, token).ConfigureAwait(false) as
                            IHasInternalId)?.InternalId ?? string.Empty;

                    await treAIPrograms.DoThreadSafeAsync(x => x.Nodes.Clear(), token).ConfigureAwait(false);

                    // Add AI Programs.
                    await CharacterObject.AIPrograms
                                         .ForEachAsync(objAIProgram => AddToTree(objAIProgram, false), token)
                                         .ConfigureAwait(false);

                    await treAIPrograms.DoThreadSafeAsync(x => x.SortCustomAlphabetically(strSelectedId), token).ConfigureAwait(false);
                }
                else
                {
                    objParentNode
                        = await treAIPrograms.DoThreadSafeFuncAsync(x => x.FindNode("Node_SelectedAIPrograms", false),
                                                                    token).ConfigureAwait(false);
                    switch (e.Action)
                    {
                        case NotifyCollectionChangedAction.Add:
                        {
                            foreach (AIProgram objAIProgram in e.NewItems)
                            {
                                await AddToTree(objAIProgram).ConfigureAwait(false);
                            }

                            break;
                        }
                        case NotifyCollectionChangedAction.Remove:
                        {
                            await treAIPrograms.DoThreadSafeAsync(x =>
                            {
                                foreach (AIProgram objAIProgram in e.OldItems)
                                {
                                    TreeNode objNode = x.FindNodeByTag(objAIProgram);
                                    if (objNode != null)
                                    {
                                        TreeNode objParent = objNode.Parent;
                                        objNode.Remove();
                                        if (objParent.Level == 0 && objParent.Nodes.Count == 0)
                                            objParent.Remove();
                                    }
                                }
                            }, token).ConfigureAwait(false);

                            break;
                        }
                        case NotifyCollectionChangedAction.Replace:
                        {
                            List<TreeNode> lstOldParents =
                                new List<TreeNode>(e.OldItems.Count);
                            await treAIPrograms.DoThreadSafeAsync(x =>
                            {
                                foreach (AIProgram objAIProgram in e.OldItems)
                                {
                                    TreeNode objNode = x.FindNodeByTag(objAIProgram);
                                    if (objNode != null)
                                    {
                                        lstOldParents.Add(objNode.Parent);
                                        objNode.Remove();
                                    }
                                }
                            }, token).ConfigureAwait(false);

                            foreach (AIProgram objAIProgram in e.NewItems)
                            {
                                await AddToTree(objAIProgram).ConfigureAwait(false);
                            }

                            await treAIPrograms.DoThreadSafeAsync(() =>
                            {
                                foreach (TreeNode objOldParent in lstOldParents)
                                {
                                    if (objOldParent.Level == 0 && objOldParent.Nodes.Count == 0)
                                        objOldParent.Remove();
                                }
                            }, token).ConfigureAwait(false);

                            break;
                        }
                    }
                }
            }
            finally
            {
                SkipUpdate = false;
                await objCursorWait.DisposeAsync().ConfigureAwait(false);
            }

            async Task AddToTree(AIProgram objAIProgram, bool blnSingleAdd = true)
            {
                TreeNode objNode = objAIProgram.CreateTreeNode(cmsAdvancedProgram);
                if (objNode == null)
                    return;

                if (objParentNode == null)
                {
                    objParentNode = new TreeNode
                    {
                        Tag = "Node_SelectedAIPrograms",
                        Text = await LanguageManager.GetStringAsync("Node_SelectedAIPrograms", token: token)
                            .ConfigureAwait(false)
                    };
                    await treAIPrograms.DoThreadSafeAsync(x =>
                    {
                        // ReSharper disable once AssignNullToNotNullAttribute
                        x.Nodes.Add(objParentNode);
                        objParentNode.Expand();
                    }, token).ConfigureAwait(false);
                }

                await treAIPrograms.DoThreadSafeAsync(x =>
                {
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
                        x.SelectedNode = objNode;
                    }
                    else
                        objParentNode.Nodes.Add(objNode);
                }, token).ConfigureAwait(false);
            }
        }

        protected async Task RefreshComplexForms(TreeView treComplexForms, TreeView treMetamagic, ContextMenuStrip cmsComplexForm, ContextMenuStrip cmsInitiationNotes, NotifyCollectionChangedEventArgs e = null, CancellationToken token = default)
        {
            if (treComplexForms == null)
                return;
            TreeNode objParentNode = null;
            CursorWait objCursorWait = await CursorWait.NewAsync(this, token: token).ConfigureAwait(false);
            SkipUpdate = true;
            try
            {
                if (e == null ||
                    e.Action == NotifyCollectionChangedAction.Reset)
                {
                    string strSelectedId =
                        (await treComplexForms.DoThreadSafeFuncAsync(x => x.SelectedNode?.Tag, token).ConfigureAwait(false) as
                            IHasInternalId)?.InternalId ?? string.Empty;
                    string strSelectedMetamagicId =
                        treMetamagic != null
                            ? (await treMetamagic.DoThreadSafeFuncAsync(x => x.SelectedNode?.Tag, token).ConfigureAwait(false) as
                                IHasInternalId)?.InternalId ?? string.Empty
                            : string.Empty;

                    await treComplexForms.DoThreadSafeAsync(x => x.Nodes.Clear(), token).ConfigureAwait(false);

                    // Add Complex Forms.
                    await CharacterObject.ComplexForms.ForEachAsync(async objComplexForm =>
                    {
                        if (objComplexForm.Grade > 0 && treMetamagic != null)
                        {
                            await treMetamagic.DoThreadSafeAsync(x => x.FindNodeByTag(objComplexForm)?.Remove(),
                                                                 token).ConfigureAwait(false);
                        }

                        await AddToTree(objComplexForm, false).ConfigureAwait(false);
                    }, token).ConfigureAwait(false);

                    await treComplexForms.DoThreadSafeAsync(x => x.SortCustomAlphabetically(strSelectedId),
                                                            token).ConfigureAwait(false);
                    if (treMetamagic != null)
                        await treMetamagic.DoThreadSafeAsync(x => x.SelectedNode = x.FindNode(strSelectedMetamagicId),
                                                             token).ConfigureAwait(false);
                }
                else
                {
                    objParentNode
                        = await treComplexForms.DoThreadSafeFuncAsync(
                            x => x.FindNode("Node_SelectedAdvancedComplexForms", false), token).ConfigureAwait(false);
                    switch (e.Action)
                    {
                        case NotifyCollectionChangedAction.Add:
                        {
                            foreach (ComplexForm objComplexForm in e.NewItems)
                            {
                                await AddToTree(objComplexForm).ConfigureAwait(false);
                            }

                            break;
                        }
                        case NotifyCollectionChangedAction.Remove:
                        {
                            foreach (ComplexForm objComplexForm in e.OldItems)
                            {
                                await treComplexForms.DoThreadSafeAsync(x =>
                                {
                                    TreeNode objNode = x.FindNodeByTag(objComplexForm);
                                    if (objNode != null)
                                    {
                                        TreeNode objParent = objNode.Parent;
                                        objNode.Remove();
                                        if (objParent.Level == 0 && objParent.Nodes.Count == 0)
                                            objParent.Remove();
                                    }
                                }, token).ConfigureAwait(false);

                                if (objComplexForm.Grade > 0 && treMetamagic != null)
                                {
                                    await treMetamagic.DoThreadSafeAsync(
                                        x => x.FindNodeByTag(objComplexForm)?.Remove(), token).ConfigureAwait(false);
                                }
                            }

                            break;
                        }
                        case NotifyCollectionChangedAction.Replace:
                        {
                            List<TreeNode> lstOldParents =
                                new List<TreeNode>(e.OldItems.Count);
                            foreach (ComplexForm objComplexForm in e.OldItems)
                            {
                                await treComplexForms.DoThreadSafeAsync(x =>
                                {
                                    TreeNode objNode = x.FindNodeByTag(objComplexForm);
                                    if (objNode != null)
                                    {
                                        lstOldParents.Add(objNode.Parent);
                                        objNode.Remove();
                                    }
                                }, token).ConfigureAwait(false);

                                if (objComplexForm.Grade > 0 && treMetamagic != null)
                                {
                                    await treMetamagic.DoThreadSafeAsync(
                                        x => x.FindNodeByTag(objComplexForm)?.Remove(), token).ConfigureAwait(false);
                                }
                            }

                            foreach (ComplexForm objComplexForm in e.NewItems)
                            {
                                await AddToTree(objComplexForm).ConfigureAwait(false);
                            }

                            await treComplexForms.DoThreadSafeAsync(() =>
                            {
                                foreach (TreeNode objOldParent in lstOldParents)
                                {
                                    if (objOldParent.Level == 0 && objOldParent.Nodes.Count == 0)
                                        objOldParent.Remove();
                                }
                            }, token).ConfigureAwait(false);

                            break;
                        }
                    }
                }
            }
            finally
            {
                SkipUpdate = false;
                await objCursorWait.DisposeAsync().ConfigureAwait(false);
            }

            async Task AddToTree(ComplexForm objComplexForm, bool blnSingleAdd = true)
            {
                TreeNode objNode = objComplexForm.CreateTreeNode(cmsComplexForm);
                if (objNode == null)
                    return;
                if (objParentNode == null)
                {
                    objParentNode = new TreeNode
                    {
                        Tag = "Node_SelectedAdvancedComplexForms",
                        Text = await LanguageManager.GetStringAsync("Node_SelectedAdvancedComplexForms", token: token).ConfigureAwait(false)
                    };
                    await treComplexForms.DoThreadSafeAsync(x =>
                    {
                        // ReSharper disable once AssignNullToNotNullAttribute
                        x.Nodes.Add(objParentNode);
                        objParentNode.Expand();
                    }, token).ConfigureAwait(false);
                }
                if (objComplexForm.Grade > 0)
                {
                    InitiationGrade objGrade = await CharacterObject.InitiationGrades.FirstOrDefaultAsync(x => x.Grade == objComplexForm.Grade, token).ConfigureAwait(false);
                    if (objGrade != null && treMetamagic != null)
                    {
                        await treMetamagic.DoThreadSafeAsync(x =>
                        {
                            TreeNode nodMetamagicParent = x.FindNodeByTag(objGrade);
                            if (nodMetamagicParent != null)
                            {
                                TreeNodeCollection nodMetamagicParentChildren = nodMetamagicParent.Nodes;
                                TreeNode objMetamagicNode = objComplexForm.CreateTreeNode(cmsInitiationNotes);
                                int intNodesCount = nodMetamagicParentChildren.Count;
                                int intTargetIndex = 0;
                                for (; intTargetIndex < intNodesCount; ++intTargetIndex)
                                {
                                    if (CompareTreeNodes.CompareText(nodMetamagicParentChildren[intTargetIndex],
                                                                     objMetamagicNode) >= 0)
                                    {
                                        break;
                                    }
                                }

                                nodMetamagicParentChildren.Insert(intTargetIndex, objMetamagicNode);
                                if (blnSingleAdd)
                                    x.SelectedNode = objMetamagicNode;
                            }
                        }, token).ConfigureAwait(false);
                    }
                }

                await treComplexForms.DoThreadSafeAsync(x =>
                {
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
                        x.SelectedNode = objNode;
                    }
                    else
                        objParentNode.Nodes.Add(objNode);
                }, token).ConfigureAwait(false);
            }
        }

        protected async Task RefreshInitiationGrades(TreeView treMetamagic, ContextMenuStrip cmsMetamagic, ContextMenuStrip cmsInitiationNotes, NotifyCollectionChangedEventArgs e = null, CancellationToken token = default)
        {
            if (treMetamagic == null)
                return;
            CursorWait objCursorWait = await CursorWait.NewAsync(this, token: token).ConfigureAwait(false);
            try
            {
                SkipUpdate = true;
                if (e == null ||
                    e.Action == NotifyCollectionChangedAction.Reset)
                {
                    (string strSelectedId, TreeNodeCollection lstRootNodes) = await treMetamagic.DoThreadSafeFuncAsync(
                        x =>
                        {
                            string strReturn =
                                (x.SelectedNode?.Tag as IHasInternalId)?.InternalId ?? string.Empty;
                            TreeNodeCollection lstReturn = x.Nodes;
                            lstReturn.Clear();
                            return new Tuple<string, TreeNodeCollection>(strReturn, lstReturn);
                        }, token).ConfigureAwait(false);

                    await CharacterObject.InitiationGrades.ForEachAsync(objGrade => AddToTree(objGrade), token)
                                         .ConfigureAwait(false);
                    int intOffset = lstRootNodes.Count;
                    await CharacterObject.Metamagics.ForEachAsync(objMetamagic =>
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
                    }, token).ConfigureAwait(false);
                    await treMetamagic.DoThreadSafeAsync(x => x.SelectedNode = x.FindNode(strSelectedId), token).ConfigureAwait(false);
                }
                else
                {
                    switch (e.Action)
                    {
                        case NotifyCollectionChangedAction.Add:
                        {
                            int intNewIndex = e.NewStartingIndex;
                            foreach (InitiationGrade objGrade in e.NewItems)
                            {
                                await AddToTree(objGrade, intNewIndex).ConfigureAwait(false);
                                ++intNewIndex;
                            }
                        }
                            break;

                        case NotifyCollectionChangedAction.Remove:
                        {
                            await treMetamagic.DoThreadSafeAsync(x =>
                            {
                                foreach (InitiationGrade objGrade in e.OldItems)
                                {
                                    x.FindNodeByTag(objGrade)?.Remove();
                                }
                            }, token).ConfigureAwait(false);
                        }
                            break;

                        case NotifyCollectionChangedAction.Replace:
                        {
                            await treMetamagic.DoThreadSafeAsync(x =>
                            {
                                foreach (InitiationGrade objGrade in e.OldItems)
                                {
                                    x.FindNodeByTag(objGrade)?.Remove();
                                }
                            }, token).ConfigureAwait(false);

                            int intNewIndex = e.NewStartingIndex;
                            foreach (InitiationGrade objGrade in e.NewItems)
                            {
                                await AddToTree(objGrade, intNewIndex).ConfigureAwait(false);
                                ++intNewIndex;
                            }
                        }
                            break;

                        case NotifyCollectionChangedAction.Move:
                        {
                            int intNewIndex = e.NewStartingIndex;
                            await treMetamagic.DoThreadSafeAsync(x =>
                            {
                                foreach (InitiationGrade objGrade in e.OldItems)
                                {
                                    TreeNode nodGrade = x.FindNodeByTag(objGrade);
                                    if (nodGrade != null)
                                    {
                                        nodGrade.Remove();
                                        x.Nodes.Insert(intNewIndex, nodGrade);
                                        ++intNewIndex;
                                    }
                                }
                            }, token).ConfigureAwait(false);
                        }
                            break;
                    }
                }
            }
            finally
            {
                SkipUpdate = false;
                await objCursorWait.DisposeAsync().ConfigureAwait(false);
            }

            async Task AddToTree(InitiationGrade objInitiationGrade, int intIndex = -1)
            {
                TreeNode nodGrade = objInitiationGrade.CreateTreeNode(cmsMetamagic);
                TreeNodeCollection lstParentNodeChildren = nodGrade.Nodes;
                await CharacterObject.Arts.ForEachAsync(objArt =>
                {
                    if (objArt.Grade == objInitiationGrade.Grade)
                    {
                        TreeNode objNode = objArt.CreateTreeNode(cmsInitiationNotes, true);
                        if (objNode == null)
                            return;
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
                }, token).ConfigureAwait(false);

                await CharacterObject.Metamagics.ForEachAsync(objMetamagic =>
                {
                    if (objMetamagic.Grade == objInitiationGrade.Grade)
                    {
                        TreeNode objNode = objMetamagic.CreateTreeNode(cmsInitiationNotes, true);
                        if (objNode == null)
                            return;
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
                }, token).ConfigureAwait(false);

                await CharacterObject.Spells.ForEachAsync(objSpell =>
                {
                    if (objSpell.Grade == objInitiationGrade.Grade)
                    {
                        TreeNode objNode = objSpell.CreateTreeNode(cmsInitiationNotes, true);
                        if (objNode == null)
                            return;
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
                }, token).ConfigureAwait(false);

                await CharacterObject.ComplexForms.ForEachAsync(objComplexForm =>
                {
                    if (objComplexForm.Grade == objInitiationGrade.Grade)
                    {
                        TreeNode objNode = objComplexForm.CreateTreeNode(cmsInitiationNotes);
                        if (objNode == null)
                            return;
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
                }, token).ConfigureAwait(false);

                await CharacterObject.Enhancements.ForEachAsync(objEnhancement =>
                {
                    if (objEnhancement.Grade == objInitiationGrade.Grade)
                    {
                        TreeNode objNode = objEnhancement.CreateTreeNode(cmsInitiationNotes, true);
                        if (objNode == null)
                            return;
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
                }, token).ConfigureAwait(false);

                await CharacterObject.Powers.ForEachAsync(objPower =>
                {
                    return objPower.Enhancements.ForEachAsync(objEnhancement =>
                    {
                        if (objEnhancement.Grade == objInitiationGrade.Grade)
                        {
                            TreeNode objNode = objEnhancement.CreateTreeNode(cmsInitiationNotes, true);
                            if (objNode == null)
                                return;
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
                    }, token);
                }, token).ConfigureAwait(false);

                nodGrade.Expand();
                await treMetamagic.DoThreadSafeAsync(x =>
                {
                    if (intIndex < 0)
                        x.Nodes.Add(nodGrade);
                    else
                        x.Nodes.Insert(intIndex, nodGrade);
                }, token).ConfigureAwait(false);
            }
        }

        protected async Task RefreshArtCollection(TreeView treMetamagic, ContextMenuStrip cmsMetamagic, ContextMenuStrip cmsInitiationNotes, NotifyCollectionChangedEventArgs e, CancellationToken token = default)
        {
            if (treMetamagic == null || e == null)
                return;
            CursorWait objCursorWait = await CursorWait.NewAsync(this, token: token).ConfigureAwait(false);
            SkipUpdate = true;
            try
            {
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                    {
                        foreach (Art objArt in e.NewItems)
                        {
                            await AddToTree(objArt).ConfigureAwait(false);
                        }
                    }
                        break;

                    case NotifyCollectionChangedAction.Remove:
                    {
                        await treMetamagic.DoThreadSafeAsync(x =>
                        {
                            foreach (Art objArt in e.OldItems)
                            {
                                x.FindNodeByTag(objArt)?.Remove();
                            }
                        }, token).ConfigureAwait(false);
                    }
                        break;

                    case NotifyCollectionChangedAction.Replace:
                    {
                        await treMetamagic.DoThreadSafeAsync(x =>
                        {
                            foreach (Art objArt in e.OldItems)
                            {
                                x.FindNodeByTag(objArt)?.Remove();
                            }
                        }, token).ConfigureAwait(false);

                        foreach (Art objArt in e.NewItems)
                        {
                            await AddToTree(objArt).ConfigureAwait(false);
                        }
                    }
                        break;

                    case NotifyCollectionChangedAction.Reset:
                    {
                        await RefreshInitiationGrades(treMetamagic, cmsMetamagic, cmsInitiationNotes, token: token).ConfigureAwait(false);
                    }
                        break;
                }
            }
            finally
            {
                SkipUpdate = false;
                await objCursorWait.DisposeAsync().ConfigureAwait(false);
            }

            async Task AddToTree(Art objArt, bool blnSingleAdd = true)
            {
                InitiationGrade objGrade = await CharacterObject.InitiationGrades.FirstOrDefaultAsync(x => x.Grade == objArt.Grade, token).ConfigureAwait(false);

                if (objGrade != null)
                {
                    await treMetamagic.DoThreadSafeAsync(x =>
                    {
                        TreeNode nodMetamagicParent = x.FindNodeByTag(objGrade);
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
                                if (CompareTreeNodes.CompareText(nodMetamagicParentChildren[intTargetIndex], objNode)
                                    >= 0)
                                {
                                    break;
                                }
                            }

                            nodMetamagicParentChildren.Insert(intTargetIndex, objNode);
                            nodMetamagicParent.Expand();
                            if (blnSingleAdd)
                                x.SelectedNode = objNode;
                        }
                    }, token).ConfigureAwait(false);
                }
            }
        }

        protected async Task RefreshEnhancementCollection(TreeView treMetamagic, ContextMenuStrip cmsMetamagic, ContextMenuStrip cmsInitiationNotes, NotifyCollectionChangedEventArgs e, CancellationToken token = default)
        {
            if (treMetamagic == null || e == null)
                return;

            CursorWait objCursorWait = await CursorWait.NewAsync(this, token: token).ConfigureAwait(false);
            SkipUpdate = true;
            try
            {
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                    {
                        foreach (Enhancement objEnhancement in e.NewItems)
                        {
                            await AddToTree(objEnhancement).ConfigureAwait(false);
                        }
                    }
                        break;

                    case NotifyCollectionChangedAction.Remove:
                    {
                        await treMetamagic.DoThreadSafeAsync(x =>
                        {
                            foreach (Enhancement objEnhancement in e.OldItems)
                            {
                                x.FindNodeByTag(objEnhancement)?.Remove();
                            }
                        }, token).ConfigureAwait(false);
                    }
                        break;

                    case NotifyCollectionChangedAction.Replace:
                    {
                        await treMetamagic.DoThreadSafeAsync(x =>
                        {
                            foreach (Enhancement objEnhancement in e.OldItems)
                            {
                                x.FindNodeByTag(objEnhancement)?.Remove();
                            }
                        }, token).ConfigureAwait(false);

                        foreach (Enhancement objEnhancement in e.NewItems)
                        {
                            await AddToTree(objEnhancement).ConfigureAwait(false);
                        }
                    }
                        break;

                    case NotifyCollectionChangedAction.Reset:
                    {
                        await RefreshInitiationGrades(treMetamagic, cmsMetamagic, cmsInitiationNotes, token: token).ConfigureAwait(false);
                    }
                        break;
                }
            }
            finally
            {
                SkipUpdate = false;
                await objCursorWait.DisposeAsync().ConfigureAwait(false);
            }

            async Task AddToTree(Enhancement objEnhancement, bool blnSingleAdd = true)
            {
                InitiationGrade objGrade = await CharacterObject.InitiationGrades.FirstOrDefaultAsync(x => x.Grade == objEnhancement.Grade, token).ConfigureAwait(false);

                if (objGrade != null)
                {
                    await treMetamagic.DoThreadSafeAsync(x =>
                    {
                        TreeNode nodMetamagicParent = x.FindNodeByTag(objGrade);
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
                                if (CompareTreeNodes.CompareText(nodMetamagicParentChildren[intTargetIndex], objNode)
                                    >= 0)
                                {
                                    break;
                                }
                            }

                            nodMetamagicParentChildren.Insert(intTargetIndex, objNode);
                            nodMetamagicParent.Expand();
                            if (blnSingleAdd)
                                x.SelectedNode = objNode;
                        }
                    }, token).ConfigureAwait(false);
                }
            }
        }

        protected async Task RefreshPowerCollectionListChanged(TreeView treMetamagic, ContextMenuStrip cmsMetamagic, ContextMenuStrip cmsInitiationNotes, ListChangedEventArgs e = null, CancellationToken token = default)
        {
            CursorWait objCursorWait = await CursorWait.NewAsync(this, token: token).ConfigureAwait(false);
            SkipUpdate = true;
            try
            {
                switch (e?.ListChangedType)
                {
                    case ListChangedType.ItemAdded:
                    {
                        CharacterObject.Powers[e.NewIndex].Enhancements
                                             .AddTaggedCollectionChanged(
                                                 treMetamagic, MakeDirtyWithCharacterUpdate);
                        CharacterObject.Powers[e.NewIndex].Enhancements
                                             .AddTaggedCollectionChanged(treMetamagic, FuncDelegateToAdd);

                        Task FuncDelegateToAdd(object x, NotifyCollectionChangedEventArgs y, CancellationToken innerToken = default) =>
                            RefreshEnhancementCollection(treMetamagic, cmsMetamagic, cmsInitiationNotes,
                                y, innerToken);
                    }
                        break;

                    case ListChangedType.Reset:
                    {
                        await RefreshInitiationGrades(treMetamagic, cmsMetamagic, cmsInitiationNotes, token: token).ConfigureAwait(false);
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
                        await CharacterObject.Powers.ForEachWithSideEffectsAsync(objPower =>
                        {
                            objPower.Enhancements.AddTaggedCollectionChanged(treMetamagic,
                                MakeDirtyWithCharacterUpdate);
                            objPower.Enhancements.AddTaggedCollectionChanged(
                                treMetamagic, FuncDelegateToAdd);

                            Task FuncDelegateToAdd(object x, NotifyCollectionChangedEventArgs y, CancellationToken innerToken = default) =>
                                RefreshEnhancementCollection(treMetamagic, cmsMetamagic, cmsInitiationNotes,
                                    y, innerToken);
                        }, token).ConfigureAwait(false);
                    }
                        break;
                }
            }
            finally
            {
                SkipUpdate = false;
                await objCursorWait.DisposeAsync().ConfigureAwait(false);
            }

            await MakeDirtyWithCharacterUpdate(token).ConfigureAwait(false);
        }

        protected async Task RefreshPowerCollectionBeforeRemove(TreeView treMetamagic, RemovingOldEventArgs removingOldEventArgs, CancellationToken token = default)
        {
            CursorWait objCursorWait = await CursorWait.NewAsync(this, token: token).ConfigureAwait(false);
            SkipUpdate = true;
            try
            {
                if (removingOldEventArgs?.OldObject is Power objPower)
                {
                    await objPower.Enhancements.RemoveTaggedAsyncCollectionChangedAsync(treMetamagic, token).ConfigureAwait(false);
                }
            }
            finally
            {
                SkipUpdate = false;
                await objCursorWait.DisposeAsync().ConfigureAwait(false);
            }
        }

        protected async Task RefreshMetamagicCollection(TreeView treMetamagic, ContextMenuStrip cmsMetamagic, ContextMenuStrip cmsInitiationNotes, NotifyCollectionChangedEventArgs e, CancellationToken token = default)
        {
            if (treMetamagic == null || e == null)
                return;
            CursorWait objCursorWait = await CursorWait.NewAsync(this, token: token).ConfigureAwait(false);
            SkipUpdate = true;
            try
            {
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                    {
                        foreach (Metamagic objMetamagic in e.NewItems)
                        {
                            await AddToTree(objMetamagic).ConfigureAwait(false);
                        }
                    }
                        break;

                    case NotifyCollectionChangedAction.Remove:
                    {
                        await treMetamagic.DoThreadSafeAsync(x =>
                        {
                            foreach (Metamagic objMetamagic in e.OldItems)
                            {
                                x.FindNodeByTag(objMetamagic)?.Remove();
                            }
                        }, token).ConfigureAwait(false);
                    }
                        break;

                    case NotifyCollectionChangedAction.Replace:
                    {
                        await treMetamagic.DoThreadSafeAsync(x =>
                        {
                            foreach (Metamagic objMetamagic in e.OldItems)
                            {
                                x.FindNodeByTag(objMetamagic)?.Remove();
                            }
                        }, token).ConfigureAwait(false);

                        foreach (Metamagic objMetamagic in e.NewItems)
                        {
                            await AddToTree(objMetamagic).ConfigureAwait(false);
                        }
                    }
                        break;

                    case NotifyCollectionChangedAction.Reset:
                    {
                        await RefreshInitiationGrades(treMetamagic, cmsMetamagic, cmsInitiationNotes, token: token).ConfigureAwait(false);
                    }
                        break;
                }
            }
            finally
            {
                SkipUpdate = false;
                await objCursorWait.DisposeAsync().ConfigureAwait(false);
            }

            async Task AddToTree(Metamagic objMetamagic, bool blnSingleAdd = true)
            {
                if (objMetamagic.Grade < 0)
                {
                    await treMetamagic.DoThreadSafeAsync(x =>
                    {
                        TreeNodeCollection nodMetamagicParentChildren = x.Nodes;
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
                            x.SelectedNode = objNode;
                    }, token).ConfigureAwait(false);
                }
                else
                {
                    InitiationGrade objGrade = await CharacterObject.InitiationGrades.FirstOrDefaultAsync(x => x.Grade == objMetamagic.Grade, token).ConfigureAwait(false);

                    if (objGrade != null)
                    {
                        await treMetamagic.DoThreadSafeAsync(x =>
                        {
                            TreeNode nodMetamagicParent = x.FindNodeByTag(objGrade);
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
                                    if (CompareTreeNodes.CompareText(nodMetamagicParentChildren[intTargetIndex],
                                                                     objNode) >= 0)
                                    {
                                        break;
                                    }
                                }

                                nodMetamagicParentChildren.Insert(intTargetIndex, objNode);
                                objNode.Expand();
                                if (blnSingleAdd)
                                    x.SelectedNode = objNode;
                            }
                        }, token).ConfigureAwait(false);
                    }
                }
            }
        }

        /// <summary>
        /// Clears and updates the TreeView for Critter Powers. Typically called as part of AddQuality or UpdateCharacterInfo.
        /// </summary>
        /// <param name="treCritterPowers">TreeNode that will be cleared and populated.</param>
        /// <param name="cmsCritterPowers">ContextMenuStrip that will be added to each power.</param>
        /// <param name="e">Arguments for the change to the underlying ObservableCollection.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        protected async Task RefreshCritterPowers(TreeView treCritterPowers, ContextMenuStrip cmsCritterPowers, NotifyCollectionChangedEventArgs e = null, CancellationToken token = default)
        {
            if (treCritterPowers == null)
                return;
            TreeNode objPowersNode = null;
            TreeNode objWeaknessesNode = null;
            CursorWait objCursorWait = await CursorWait.NewAsync(this, token: token).ConfigureAwait(false);
            SkipUpdate = true;
            try
            {
                if (e == null
                    || e.Action == NotifyCollectionChangedAction.Reset)
                {
                    string strSelectedId
                        = (await treCritterPowers.DoThreadSafeFuncAsync(x => x.SelectedNode?.Tag, token).ConfigureAwait(false) as
                              IHasInternalId)
                          ?.InternalId ??
                          string.Empty;
                    await treCritterPowers.DoThreadSafeAsync(x => x.Nodes.Clear(), token).ConfigureAwait(false);
                    // Add the Critter Powers that exist.
                    await CharacterObject.CritterPowers.ForEachAsync(objPower => AddToTree(objPower, false), token)
                                         .ConfigureAwait(false);

                    await treCritterPowers.DoThreadSafeAsync(x => x.SortCustomAlphabetically(strSelectedId),
                                                             token).ConfigureAwait(false);
                }
                else
                {
                    await treCritterPowers.DoThreadSafeAsync(x =>
                    {
                        objPowersNode = x.FindNode("Node_CritterPowers", false);
                        objWeaknessesNode = x.FindNode("Node_CritterWeaknesses", false);
                    }, token).ConfigureAwait(false);
                    switch (e.Action)
                    {
                        case NotifyCollectionChangedAction.Add:
                        {
                            foreach (CritterPower objPower in e.NewItems)
                            {
                                await AddToTree(objPower).ConfigureAwait(false);
                            }

                            break;
                        }
                        case NotifyCollectionChangedAction.Remove:
                        {
                            await treCritterPowers.DoThreadSafeAsync(x =>
                            {
                                foreach (CritterPower objPower in e.OldItems)
                                {
                                    TreeNode objNode = x.FindNodeByTag(objPower);
                                    if (objNode != null)
                                    {
                                        TreeNode objParent = objNode.Parent;
                                        objNode.Remove();
                                        if (objParent.Level == 0 && objParent.Nodes.Count == 0)
                                            objParent.Remove();
                                    }
                                }
                            }, token).ConfigureAwait(false);

                            break;
                        }
                        case NotifyCollectionChangedAction.Replace:
                        {
                            List<TreeNode> lstOldParents =
                                new List<TreeNode>(e.OldItems.Count);
                            await treCritterPowers.DoThreadSafeAsync(x =>
                            {
                                foreach (CritterPower objPower in e.OldItems)
                                {
                                    TreeNode objNode = x.FindNode(objPower.InternalId);
                                    if (objNode != null)
                                    {
                                        lstOldParents.Add(objNode.Parent);
                                        objNode.Remove();
                                    }
                                }
                            }, token).ConfigureAwait(false);

                            foreach (CritterPower objPower in e.NewItems)
                            {
                                await AddToTree(objPower).ConfigureAwait(false);
                            }

                            await treCritterPowers.DoThreadSafeAsync(() =>
                            {
                                foreach (TreeNode objOldParent in lstOldParents)
                                {
                                    if (objOldParent.Level == 0 && objOldParent.Nodes.Count == 0)
                                        objOldParent.Remove();
                                }
                            }, token).ConfigureAwait(false);

                            break;
                        }
                    }
                }
            }
            finally
            {
                SkipUpdate = false;
                await objCursorWait.DisposeAsync().ConfigureAwait(false);
            }

            async Task AddToTree(CritterPower objPower, bool blnSingleAdd = true)
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
                                Text = await LanguageManager.GetStringAsync("Node_CritterWeaknesses", token: token).ConfigureAwait(false)
                            };
                            await treCritterPowers.DoThreadSafeAsync(x =>
                            {
                                // ReSharper disable once AssignNullToNotNullAttribute
                                x.Nodes.Add(objWeaknessesNode);
                                objWeaknessesNode.Expand();
                            }, token).ConfigureAwait(false);
                        }
                        objParentNode = objWeaknessesNode;
                        break;

                    default:
                        if (objPowersNode == null)
                        {
                            objPowersNode = new TreeNode
                            {
                                Tag = "Node_CritterPowers",
                                Text = await LanguageManager.GetStringAsync("Node_CritterPowers", token: token).ConfigureAwait(false)
                            };
                            await treCritterPowers.DoThreadSafeAsync(x =>
                            {
                                // ReSharper disable once AssignNullToNotNullAttribute
                                x.Nodes.Insert(0, objPowersNode);
                                objPowersNode.Expand();
                            }, token).ConfigureAwait(false);
                        }
                        objParentNode = objPowersNode;
                        break;
                }

                await treCritterPowers.DoThreadSafeAsync(x =>
                {
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
                        x.SelectedNode = objNode;
                    }
                    else
                        objParentNode.Nodes.Add(objNode);
                }, token).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Refreshes the list of qualities into the selected TreeNode. If the same number of
        /// </summary>
        /// <param name="treQualities">TreeView to insert the qualities into.</param>
        /// <param name="cmsQuality">ContextMenuStrip to add to each Quality node.</param>
        /// <param name="fntNormal">Normal font to use for qualities.</param>
        /// <param name="fntStrikeout">Font to use for disabled qualities (e.g. cybereyes-disabled Low Light Vision).</param>
        /// <param name="e">Arguments for the change to the underlying ObservableCollection.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        protected async Task RefreshQualities(TreeView treQualities, ContextMenuStrip cmsQuality, Font fntNormal, Font fntStrikeout, NotifyCollectionChangedEventArgs e = null, CancellationToken token = default)
        {
            if (treQualities == null)
                return;
            TreeNode objPositiveQualityRoot = null;
            TreeNode objNegativeQualityRoot = null;
            TreeNode objLifeModuleRoot = null;

            CursorWait objCursorWait = await CursorWait.NewAsync(this, token: token).ConfigureAwait(false);
            SkipUpdate = true;
            try
            {
                if (e == null ||
                    e.Action == NotifyCollectionChangedAction.Reset)
                {
                    string strSelectedNode =
                        (await treQualities.DoThreadSafeFuncAsync(x => x.SelectedNode?.Tag, token).ConfigureAwait(false) as
                            IHasInternalId)?.InternalId ?? string.Empty;

                    // Create the root nodes.
                    await CharacterObject.Qualities.ForEachWithSideEffectsAsync(x => x.MultiplePropertiesChangedAsync -= AddedQualityOnPropertyChanged, token).ConfigureAwait(false);

                    await treQualities.DoThreadSafeAsync(x => x.Nodes.Clear(), token).ConfigureAwait(false);

                    // Multiple instances of the same quality are combined into just one entry with a number next to it (e.g. 6 discrete entries of "Focused Concentration" become "Focused Concentration 6")
                    using (new FetchSafelyFromPool<HashSet<string>>(Utils.StringHashSetPool,
                                                                    out HashSet<string> setQualitiesToPrint))
                    {
                        await CharacterObject.Qualities.ForEachAsync(async objQuality =>
                        {
                            setQualitiesToPrint.Add(objQuality.SourceIDString + '|' +
                                                    await objQuality.GetSourceNameAsync(GlobalSettings.Language, token)
                                                                    .ConfigureAwait(false) + '|' +
                                                    objQuality.Extra);
                        }, token).ConfigureAwait(false);

                        // Add Qualities
                        await CharacterObject.Qualities.ForEachAsync(async objQuality =>
                        {
                            if (!setQualitiesToPrint.Remove(objQuality.SourceIDString + '|' +
                                                            await objQuality
                                                                  .GetSourceNameAsync(GlobalSettings.Language, token)
                                                                  .ConfigureAwait(false)
                                                            + '|' +
                                                            objQuality.Extra))
                                return;

                            await AddToTree(objQuality, false).ConfigureAwait(false);
                        }, token).ConfigureAwait(false);
                    }

                    await treQualities.DoThreadSafeAsync(x => x.SortCustomAlphabetically(strSelectedNode),
                                                         token).ConfigureAwait(false);
                }
                else
                {
                    await treQualities.DoThreadSafeAsync(x =>
                    {
                        objPositiveQualityRoot = x.FindNodeByTag("Node_SelectedPositiveQualities", false);
                        objNegativeQualityRoot = x.FindNodeByTag("Node_SelectedNegativeQualities", false);
                        objLifeModuleRoot = x.FindNodeByTag("String_LifeModules", false);
                    }, token).ConfigureAwait(false);
                    bool blnDoNameRefresh = false;
                    switch (e.Action)
                    {
                        case NotifyCollectionChangedAction.Add:
                        {
                            foreach (Quality objQuality in e.NewItems)
                            {
                                if (await objQuality.GetLevelsAsync(token).ConfigureAwait(false) > 1)
                                    blnDoNameRefresh = true;
                                else
                                    await AddToTree(objQuality).ConfigureAwait(false);
                            }

                            break;
                        }
                        case NotifyCollectionChangedAction.Remove:
                        {
                            foreach (Quality objQuality in e.OldItems)
                            {
                                if (await objQuality.GetLevelsAsync(token).ConfigureAwait(false) > 0)
                                    blnDoNameRefresh = true;
                                else
                                {
                                    await treQualities.DoThreadSafeAsync(x =>
                                    {
                                        TreeNode objNode = x.FindNodeByTag(objQuality);
                                        if (objNode != null)
                                        {
                                            TreeNode objParent = objNode.Parent;
                                            objNode.Remove();
                                            if (objParent.Level == 0 && objParent.Nodes.Count == 0)
                                                objParent.Remove();
                                        }
                                    }, token).ConfigureAwait(false);
                                    objQuality.MultiplePropertiesChangedAsync -= AddedQualityOnPropertyChanged;
                                }
                            }

                            break;
                        }
                        case NotifyCollectionChangedAction.Replace:
                        {
                            List<TreeNode> lstOldParents =
                                new List<TreeNode>(e.OldItems.Count);
                            foreach (Quality objQuality in e.OldItems)
                            {
                                if (await objQuality.GetLevelsAsync(token).ConfigureAwait(false) > 0)
                                    blnDoNameRefresh = true;
                                else
                                {
                                    TreeNode objNode
                                        = await treQualities.DoThreadSafeFuncAsync(
                                            x => x.FindNodeByTag(objQuality), token).ConfigureAwait(false);
                                    if (objNode != null)
                                    {
                                        await treQualities.DoThreadSafeAsync(() =>
                                        {
                                            if (objNode.Parent != null)
                                                lstOldParents.Add(objNode.Parent);
                                            objNode.Remove();
                                        }, token).ConfigureAwait(false);
                                        objQuality.MultiplePropertiesChangedAsync -= AddedQualityOnPropertyChanged;
                                    }
                                    else
                                    {
                                        await RefreshQualityNames(treQualities, token).ConfigureAwait(false);
                                    }
                                }
                            }

                            foreach (Quality objQuality in e.NewItems)
                            {
                                if (await objQuality.GetLevelsAsync(token).ConfigureAwait(false) > 1)
                                    blnDoNameRefresh = true;
                                else
                                    await AddToTree(objQuality).ConfigureAwait(false);
                            }

                            await treQualities.DoThreadSafeAsync(() =>
                            {
                                foreach (TreeNode objOldParent in lstOldParents)
                                {
                                    if (objOldParent.Level == 0 && objOldParent.Nodes.Count == 0)
                                        objOldParent.Remove();
                                }
                            }, token).ConfigureAwait(false);

                            break;
                        }
                    }

                    if (blnDoNameRefresh)
                        await RefreshQualityNames(treQualities, token).ConfigureAwait(false);
                }
            }
            finally
            {
                SkipUpdate = false;
                await objCursorWait.DisposeAsync().ConfigureAwait(false);
            }

            async Task AddToTree(Quality objQuality, bool blnSingleAdd = true)
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
                                Text = await LanguageManager.GetStringAsync("Node_SelectedPositiveQualities", token: token).ConfigureAwait(false)
                            };
                            await treQualities.DoThreadSafeAsync(x =>
                            {
                                // ReSharper disable once AssignNullToNotNullAttribute
                                x.Nodes.Insert(0, objPositiveQualityRoot);
                                objPositiveQualityRoot.Expand();
                            }, token).ConfigureAwait(false);
                        }
                        objParentNode = objPositiveQualityRoot;
                        break;

                    case QualityType.Negative:
                        if (objNegativeQualityRoot == null)
                        {
                            objNegativeQualityRoot = new TreeNode
                            {
                                Tag = "Node_SelectedNegativeQualities",
                                Text = await LanguageManager.GetStringAsync("Node_SelectedNegativeQualities", token: token).ConfigureAwait(false)
                            };
                            await treQualities.DoThreadSafeAsync(x =>
                            {
                                x.Nodes.Insert((objLifeModuleRoot == null || objPositiveQualityRoot != null).ToInt32(),
                                    // ReSharper disable once AssignNullToNotNullAttribute
                                    objNegativeQualityRoot);
                                objNegativeQualityRoot.Expand();
                            }, token).ConfigureAwait(false);
                        }
                        objParentNode = objNegativeQualityRoot;
                        break;

                    case QualityType.LifeModule:
                        if (objLifeModuleRoot == null)
                        {
                            objLifeModuleRoot = new TreeNode
                            {
                                Tag = "String_LifeModules",
                                Text = await LanguageManager.GetStringAsync("String_LifeModules", token: token).ConfigureAwait(false)
                            };
                            await treQualities.DoThreadSafeAsync(x =>
                            {
                                // ReSharper disable once AssignNullToNotNullAttribute
                                x.Nodes.Add(objLifeModuleRoot);
                                objLifeModuleRoot.Expand();
                            }, token).ConfigureAwait(false);
                        }
                        objParentNode = objLifeModuleRoot;
                        break;
                }

                if (objParentNode != null)
                {
                    await treQualities.DoThreadSafeAsync(x =>
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
                            x.SelectedNode = objNode;
                        }
                        else
                            objParentNode.Nodes.Add(objNode);
                    }, token).ConfigureAwait(false);
                    objQuality.MultiplePropertiesChangedAsync += AddedQualityOnPropertyChanged;
                }
            }

            async Task AddedQualityOnPropertyChanged(object sender, MultiplePropertiesChangedEventArgs e2, CancellationToken innerToken = default)
            {
                innerToken.ThrowIfCancellationRequested();
                if (!(sender is Quality objQuality))
                    return;
                if (e2.PropertyNames.Contains(nameof(Quality.Suppressed))
                    || e2.PropertyNames.Contains(nameof(Quality.Notes)))
                {
                    TreeNode objNode = treQualities.FindNodeByTag(objQuality);
                    if (objNode == null)
                        return;
                    if (e2.PropertyNames.Contains(nameof(Quality.Suppressed)))
                        objNode.NodeFont = await objQuality.GetSuppressedAsync(innerToken).ConfigureAwait(false)
                            ? fntStrikeout
                            : fntNormal;
                    if (e2.PropertyNames.Contains(nameof(Quality.Notes)))
                        objNode.ToolTipText =
                            (await objQuality.GetNotesAsync(innerToken).ConfigureAwait(false)).WordWrap();
                }
            }
        }

        /// <summary>
        /// Refreshes all the names of qualities in the nodes
        /// </summary>
        /// <param name="treQualities">TreeView to insert the qualities into.</param>
        /// <param name="token">Cancellation token to use.</param>
        protected async Task RefreshQualityNames(TreeView treQualities, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (treQualities == null)
                return;
            int intTopLevelNodeCount = await treQualities.DoThreadSafeFuncAsync(x => x.GetNodeCount(false), token)
                                                         .ConfigureAwait(false);
            if (intTopLevelNodeCount <= 0)
                return;
            CursorWait objCursorWait = await CursorWait.NewAsync(this, token: token).ConfigureAwait(false);
            SkipUpdate = true;
            try
            {
                List<Tuple<TreeNode, Task<string>>> lstNames = new List<Tuple<TreeNode, Task<string>>>(intTopLevelNodeCount);
                TreeNode objSelectedNode = await treQualities.DoThreadSafeFuncAsync(x =>
                {
                    foreach (TreeNode objQualityTypeNode in x.Nodes)
                    {
                        foreach (TreeNode objQualityNode in objQualityTypeNode.Nodes)
                        {
                            if (objQualityNode.Tag is Quality objLoopQuality)
                                lstNames.Add(new Tuple<TreeNode, Task<string>>(
                                                 objQualityNode,
                                                 objLoopQuality.GetCurrentDisplayNameAsync(token)));
                        }
                    }

                    return x.SelectedNode;
                }, token).ConfigureAwait(false);
                foreach (Tuple<TreeNode, Task<string>> tupLoop in lstNames)
                {
                    string strLoopText = await tupLoop.Item2.ConfigureAwait(false);
                    await treQualities.DoThreadSafeAsync(() => tupLoop.Item1.Text = strLoopText, token).ConfigureAwait(false);
                }
                await treQualities.DoThreadSafeAsync(x => x.SortCustomAlphabetically(objSelectedNode?.Tag), token).ConfigureAwait(false);
            }
            finally
            {
                SkipUpdate = false;
                await objCursorWait.DisposeAsync().ConfigureAwait(false);
            }
        }

        #endregion Refresh Treeviews and Panels

        /// <summary>
        /// Method for removing old <addqualities /> nodes from existing characters.
        /// </summary>
        /// <param name="objNodeList">XmlNode to load. Expected to be addqualities/addquality</param>
        /// <param name="token">CancellationToken to listen to.</param>
        protected async Task RemoveAddedQualities(XPathNodeIterator objNodeList, CancellationToken token = default)
        {
            if (objNodeList == null || objNodeList.Count <= 0)
                return;
            CursorWait objCursorWait = await CursorWait.NewAsync(this, token: token).ConfigureAwait(false);
            SkipUpdate = true;
            try
            {
                foreach (XPathNavigator objNode in objNodeList)
                {
                    Quality objQuality = await CharacterObject.Qualities.FirstOrDefaultAsync(x => x.Name == objNode.Value, token).ConfigureAwait(false);
                    if (objQuality != null)
                    {
                        string strInternalId = objQuality.InternalId;
                        await objQuality.DeleteQualityAsync(token: token).ConfigureAwait(false);
                        await ImprovementManager.RemoveImprovementsAsync(
                            CharacterObject, Improvement.ImprovementSource.CritterPower,
                            strInternalId, token).ConfigureAwait(false);
                    }
                }
            }
            finally
            {
                SkipUpdate = false;
                await objCursorWait.DisposeAsync().ConfigureAwait(false);
            }
        }

        #region Locations

        protected async Task RefreshArmorLocations(TreeView treArmor, ContextMenuStrip cmsArmorLocation, NotifyCollectionChangedEventArgs e, CancellationToken token = default)
        {
            if (treArmor == null || e == null)
                return;

            CursorWait objCursorWait = await CursorWait.NewAsync(this, token: token).ConfigureAwait(false);
            try
            {
                string strSelectedId
                    = (await treArmor.DoThreadSafeFuncAsync(x => x.SelectedNode?.Tag, token).ConfigureAwait(false) as IHasInternalId)
                    ?.InternalId ?? string.Empty;

                TreeNode nodRoot
                    = await treArmor.DoThreadSafeFuncAsync(x => x.FindNode("Node_SelectedArmor", false), token).ConfigureAwait(false);
                await RefreshLocation(treArmor, nodRoot, cmsArmorLocation, _objCharacter.ArmorLocations, e,
                                      strSelectedId,
                                      "Node_SelectedArmor", token).ConfigureAwait(false);
            }
            finally
            {
                await objCursorWait.DisposeAsync().ConfigureAwait(false);
            }
        }

        protected async Task RefreshGearLocations(TreeView treGear, ContextMenuStrip cmsGearLocation, NotifyCollectionChangedEventArgs e, CancellationToken token = default)
        {
            if (treGear == null || e == null)
                return;

            CursorWait objCursorWait = await CursorWait.NewAsync(this, token: token).ConfigureAwait(false);
            try
            {
                string strSelectedId
                    = (await treGear.DoThreadSafeFuncAsync(x => x.SelectedNode?.Tag, token).ConfigureAwait(false) as IHasInternalId)
                    ?.InternalId ?? string.Empty;

                TreeNode nodRoot
                    = await treGear.DoThreadSafeFuncAsync(x => x.FindNode("Node_SelectedGear", false), token).ConfigureAwait(false);
                await RefreshLocation(treGear, nodRoot, cmsGearLocation, _objCharacter.GearLocations, e,
                                      strSelectedId,
                                      "Node_SelectedGear", token).ConfigureAwait(false);
            }
            finally
            {
                await objCursorWait.DisposeAsync().ConfigureAwait(false);
            }
        }

        protected async Task RefreshVehicleLocations(TreeView treVehicles, ContextMenuStrip cmsVehicleLocation, NotifyCollectionChangedEventArgs e, CancellationToken token = default)
        {
            if (treVehicles == null || e == null)
                return;

            CursorWait objCursorWait = await CursorWait.NewAsync(this, token: token).ConfigureAwait(false);
            try
            {
                string strSelectedId
                    = (await treVehicles.DoThreadSafeFuncAsync(x => x.SelectedNode?.Tag,
                                                               token).ConfigureAwait(false) as IHasInternalId)?.InternalId
                      ?? string.Empty;

                TreeNode nodRoot
                    = await treVehicles.DoThreadSafeFuncAsync(x => x.FindNode("Node_SelectedVehicles", false),
                                                              token).ConfigureAwait(false);
                await RefreshLocation(treVehicles, nodRoot, cmsVehicleLocation, _objCharacter.VehicleLocations, e,
                                      strSelectedId,
                                      "Node_SelectedVehicles", token).ConfigureAwait(false);
            }
            finally
            {
                await objCursorWait.DisposeAsync().ConfigureAwait(false);
            }
        }

        protected async Task RefreshLocationsInVehicle(TreeView treVehicles, Vehicle objVehicle, ContextMenuStrip cmsVehicleLocation, Func<int> funcOffset, NotifyCollectionChangedEventArgs e, CancellationToken token = default)
        {
            if (treVehicles == null || objVehicle == null || e == null)
                return;

            CursorWait objCursorWait = await CursorWait.NewAsync(this, token: token).ConfigureAwait(false);
            try
            {
                string strSelectedId
                    = (await treVehicles.DoThreadSafeFuncAsync(x => x.SelectedNode?.Tag,
                                                               token).ConfigureAwait(false) as IHasInternalId)?.InternalId
                      ?? string.Empty;

                TreeNode nodRoot
                    = await treVehicles.DoThreadSafeFuncAsync(x => x.FindNodeByTag(objVehicle), token).ConfigureAwait(false);
                await RefreshLocation(treVehicles, nodRoot, cmsVehicleLocation, funcOffset, objVehicle.Locations,
                                      e,
                                      strSelectedId, "Node_SelectedVehicles", false, token).ConfigureAwait(false);
            }
            finally
            {
                await objCursorWait.DisposeAsync().ConfigureAwait(false);
            }
        }

        protected async Task RefreshWeaponLocations(TreeView treWeapons, ContextMenuStrip cmsWeaponLocation, NotifyCollectionChangedEventArgs e, CancellationToken token = default)
        {
            if (treWeapons == null || e == null)
                return;

            CursorWait objCursorWait = await CursorWait.NewAsync(this, token: token).ConfigureAwait(false);
            try
            {
                string strSelectedId
                    = (await treWeapons.DoThreadSafeFuncAsync(x => x.SelectedNode?.Tag, token).ConfigureAwait(false) as IHasInternalId)
                    ?.InternalId ?? string.Empty;

                TreeNode nodRoot
                    = await treWeapons.DoThreadSafeFuncAsync(x => x.FindNode("Node_SelectedWeapons", false),
                                                             token).ConfigureAwait(false);
                await RefreshLocation(treWeapons, nodRoot, cmsWeaponLocation, _objCharacter.WeaponLocations, e,
                                      strSelectedId,
                                      "Node_SelectedWeapons", token).ConfigureAwait(false);
            }
            finally
            {
                await objCursorWait.DisposeAsync().ConfigureAwait(false);
            }
        }

        protected async Task RefreshCustomImprovementLocations(TreeView treImprovements, ContextMenuStrip cmsImprovementLocation, NotifyCollectionChangedEventArgs e, CancellationToken token = default)
        {
            if (treImprovements == null || e == null)
                return;

            CursorWait objCursorWait = await CursorWait.NewAsync(this, token: token).ConfigureAwait(false);
            try
            {
                string strSelectedId =
                    (await treImprovements.DoThreadSafeFuncAsync(x => x.SelectedNode?.Tag, token).ConfigureAwait(false) as
                        IHasInternalId)?.InternalId ?? string.Empty;

                TreeNode nodRoot
                    = await treImprovements.DoThreadSafeFuncAsync(x => x.FindNode("Node_SelectedImprovements", false),
                                                                  token).ConfigureAwait(false);

                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                    {
                        int intNewIndex = e.NewStartingIndex;
                        foreach (string strLocation in e.NewItems)
                        {
                            TreeNode objLocation = new TreeNode
                            {
                                Tag = strLocation,
                                Text = strLocation,
                                ContextMenuStrip = cmsImprovementLocation
                            };
                            int index = Interlocked.Increment(ref intNewIndex) - 1;
                            await treImprovements.DoThreadSafeAsync(x => x.Nodes.Insert(index, objLocation), token).ConfigureAwait(false);
                        }
                    }
                        break;

                    case NotifyCollectionChangedAction.Remove:
                    {
                        foreach (string strLocation in e.OldItems)
                        {
                            TreeNode objNode
                                = await treImprovements.DoThreadSafeFuncAsync(
                                    x => x.FindNodeByTag(strLocation, false), token).ConfigureAwait(false);
                            if (objNode != null)
                            {
                                await treImprovements.DoThreadSafeAsync(() => objNode.Remove(), token).ConfigureAwait(false);
                                if (objNode.Nodes.Count > 0)
                                {
                                    if (nodRoot == null)
                                    {
                                        nodRoot = new TreeNode
                                        {
                                            Tag = "Node_SelectedImprovements",
                                            Text = await LanguageManager.GetStringAsync("Node_SelectedImprovements", token: token).ConfigureAwait(false)
                                        };
                                        TreeNode root = nodRoot;
                                        await treImprovements.DoThreadSafeAsync(
                                            x => x.Nodes.Insert(0, root), token).ConfigureAwait(false);
                                    }

                                    TreeNode root2 = nodRoot;
                                    await treImprovements.DoThreadSafeAsync(() =>
                                    {
                                        for (int i = objNode.Nodes.Count - 1; i >= 0; --i)
                                        {
                                            TreeNode nodImprovement = objNode.Nodes[i];
                                            nodImprovement.Remove();
                                            root2.Nodes.Add(nodImprovement);
                                        }
                                    }, token).ConfigureAwait(false);
                                }
                            }
                        }
                    }
                        break;

                    case NotifyCollectionChangedAction.Replace:
                    {
                        int intNewItemsIndex = 0;
                        foreach (string strLocation in e.OldItems)
                        {
                            TreeNode objNode
                                = await treImprovements.DoThreadSafeFuncAsync(
                                    x => x.FindNodeByTag(strLocation, false), token).ConfigureAwait(false);
                            if (objNode != null)
                            {
                                if (e
                                        .NewItems[intNewItemsIndex] is string objNewLocation)
                                {
                                    await treImprovements.DoThreadSafeAsync(() =>
                                    {
                                        objNode.Tag = objNewLocation;
                                        objNode.Text = objNewLocation;
                                    }, token).ConfigureAwait(false);
                                }

                                ++intNewItemsIndex;
                            }
                        }
                    }
                        break;

                    case NotifyCollectionChangedAction.Move:
                    {
                        List<Tuple<string, TreeNode>> lstMoveNodes =
                            new List<Tuple<string, TreeNode>>(e.OldItems.Count);
                        foreach (string strLocation in e.OldItems)
                        {
                            TreeNode objLocation
                                = await treImprovements.DoThreadSafeFuncAsync(
                                    x => x.FindNode(strLocation, false), token).ConfigureAwait(false);
                            if (objLocation != null)
                            {
                                lstMoveNodes.Add(new Tuple<string, TreeNode>(strLocation, objLocation));
                                objLocation.Remove();
                            }
                        }

                        int intNewIndex = e.NewStartingIndex;
                        foreach (string strLocation in e.NewItems)
                        {
                            Tuple<string, TreeNode> objLocationTuple =
                                lstMoveNodes.Find(x => x.Item1 == strLocation);
                            if (objLocationTuple != null)
                            {
                                int index = Interlocked.Increment(ref intNewIndex) - 1;
                                await treImprovements.DoThreadSafeAsync(
                                    x => x.Nodes.Insert(index, objLocationTuple.Item2), token).ConfigureAwait(false);
                                lstMoveNodes.Remove(objLocationTuple);
                            }
                        }
                    }
                        break;

                    case NotifyCollectionChangedAction.Reset:
                    {
                        await CharacterObject.ImprovementGroups.ForEachAsync(async strLocation =>
                        {
                            TreeNode objLocation
                                = await treImprovements.DoThreadSafeFuncAsync(
                                    x => x.FindNode(strLocation, false), token).ConfigureAwait(false);
                            if (objLocation != null)
                            {
                                await treImprovements.DoThreadSafeAsync(() => objLocation.Remove(), token)
                                                     .ConfigureAwait(false);
                                if (objLocation.Nodes.Count > 0)
                                {
                                    if (nodRoot == null)
                                    {
                                        nodRoot = new TreeNode
                                        {
                                            Tag = "Node_SelectedImprovements",
                                            Text = await LanguageManager
                                                         .GetStringAsync("Node_SelectedImprovements", token: token)
                                                         .ConfigureAwait(false)
                                        };
                                        TreeNode root = nodRoot;
                                        await treImprovements.DoThreadSafeAsync(
                                            x => x.Nodes.Insert(0, root), token).ConfigureAwait(false);
                                    }

                                    TreeNode root2 = nodRoot;
                                    await treImprovements.DoThreadSafeAsync(() =>
                                    {
                                        for (int i = objLocation.Nodes.Count - 1; i >= 0; --i)
                                        {
                                            TreeNode nodImprovement = objLocation.Nodes[i];
                                            nodImprovement.Remove();
                                            root2.Nodes.Add(nodImprovement);
                                        }
                                    }, token).ConfigureAwait(false);
                                }
                            }
                        }, token).ConfigureAwait(false);
                    }
                        break;
                }

                await treImprovements.DoThreadSafeAsync(x => x.SelectedNode = x.FindNode(strSelectedId), token).ConfigureAwait(false);
            }
            finally
            {
                await objCursorWait.DisposeAsync().ConfigureAwait(false);
            }
        }

        private Task RefreshLocation(TreeView treSelected, TreeNode nodRoot, ContextMenuStrip cmsLocation,
                                                ICollection<Location> lstLocations,
                                                NotifyCollectionChangedEventArgs e,
                                                string strSelectedId, string strNodeName,
                                                CancellationToken token = default)
        {
            return RefreshLocation(treSelected, nodRoot, cmsLocation, () => (nodRoot != null).ToInt32(), lstLocations,
                e, strSelectedId, strNodeName, token: token);
        }

        private async Task RefreshLocation(TreeView treSelected, TreeNode nodRoot, ContextMenuStrip cmsLocation,
                                                Func<int> funcOffset, ICollection<Location> lstLocations,
                                                NotifyCollectionChangedEventArgs e, string strSelectedId, string strNodeName,
                                                bool rootSibling = true, CancellationToken token = default)
        {
            CursorWait objCursorWait = await CursorWait.NewAsync(this, token: token).ConfigureAwait(false);
            try
            {
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                    {
                        int intNewIndex = e.NewStartingIndex;
                        if (funcOffset != null)
                            Interlocked.Add(ref intNewIndex, funcOffset.Invoke());
                        foreach (Location objLocation in e.NewItems)
                        {
                            TreeNode objNode = await objLocation.CreateTreeNode(cmsLocation, token).ConfigureAwait(false);
                            await treSelected.DoThreadSafeAsync(x =>
                            {
                                int index = Interlocked.Increment(ref intNewIndex) - 1;
                                if (rootSibling)
                                {
                                    x.Nodes.Insert(index, objNode);
                                }
                                else
                                {
                                    nodRoot.Nodes.Insert(index, objNode);
                                }
                            }, token).ConfigureAwait(false);
                        }
                    }
                        break;

                    case NotifyCollectionChangedAction.Remove:
                    {
                        foreach (Location objLocation in e.OldItems)
                        {
                            TreeNode nodLocation
                                = await treSelected.DoThreadSafeFuncAsync(
                                    x => x.FindNodeByTag(objLocation, false), token).ConfigureAwait(false);
                            if (nodLocation == null)
                                continue;
                            if (nodLocation.Nodes.Count > 0)
                            {
                                if (nodRoot == null)
                                {
                                    nodRoot = new TreeNode
                                    {
                                        Tag = strNodeName,
                                        Text = await LanguageManager.GetStringAsync(strNodeName, token: token).ConfigureAwait(false)
                                    };
                                    TreeNode root = nodRoot;
                                    await treSelected.DoThreadSafeAsync(x => x.Nodes.Insert(0, root), token).ConfigureAwait(false);
                                }

                                TreeNode root2 = nodRoot;
                                await treSelected.DoThreadSafeAsync(() =>
                                {
                                    for (int i = nodLocation.Nodes.Count - 1; i >= 0; --i)
                                    {
                                        TreeNode nodWeapon = nodLocation.Nodes[i];
                                        nodWeapon.Remove();
                                        root2.Nodes.Add(nodWeapon);
                                    }
                                }, token).ConfigureAwait(false);
                            }

                            await treSelected.DoThreadSafeAsync(() => nodLocation.Remove(), token).ConfigureAwait(false);
                        }
                    }
                        break;

                    case NotifyCollectionChangedAction.Replace:
                    {
                        int intNewItemsIndex = 0;
                        foreach (Location objLocation in e.OldItems)
                        {
                            TreeNode objNode
                                = await treSelected.DoThreadSafeFuncAsync(
                                    x => x.FindNodeByTag(objLocation, false), token).ConfigureAwait(false);
                            if (objNode != null)
                            {
                                if (e.NewItems[intNewItemsIndex] is Location
                                    objNewLocation)
                                {
                                    string strText = await objNewLocation.GetCurrentDisplayNameAsync(token).ConfigureAwait(false);
                                    await treSelected.DoThreadSafeAsync(() =>
                                    {
                                        objNode.Tag = objNewLocation;
                                        objNode.Text = strText;
                                    }, token).ConfigureAwait(false);
                                }

                                Interlocked.Increment(ref intNewItemsIndex);
                            }
                        }
                    }
                        break;

                    case NotifyCollectionChangedAction.Move:
                    {
                        List<Tuple<Location, TreeNode>> lstMoveNodes =
                            new List<Tuple<Location, TreeNode>>(e.OldItems.Count);
                        foreach (Location objLocation in e.OldItems)
                        {
                            TreeNode objNode
                                = await treSelected.DoThreadSafeFuncAsync(
                                    x => x.FindNodeByTag(objLocation, false), token).ConfigureAwait(false);
                            if (objNode != null)
                            {
                                lstMoveNodes.Add(new Tuple<Location, TreeNode>(objLocation, objNode));
                                await treSelected.DoThreadSafeAsync(() => objNode.Remove(), token).ConfigureAwait(false);
                            }
                        }

                        int intNewIndex = e.NewStartingIndex;
                        if (funcOffset != null)
                            Interlocked.Add(ref intNewIndex, funcOffset.Invoke());
                        foreach (Location objLocation in e.NewItems)
                        {
                            Tuple<Location, TreeNode> objLocationTuple =
                                lstMoveNodes.Find(x => x.Item1 == objLocation);
                            if (objLocationTuple != null)
                            {
                                int index = Interlocked.Increment(ref intNewIndex) - 1;
                                await treSelected.DoThreadSafeAsync(
                                    x => x.Nodes.Insert(index, objLocationTuple.Item2), token).ConfigureAwait(false);
                                lstMoveNodes.Remove(objLocationTuple);
                            }
                        }
                    }
                        break;

                    case NotifyCollectionChangedAction.Reset:
                    {
                        List<TreeNode> lstNodesNeedingProcessing = new List<TreeNode>(lstLocations.Count);
                        await treSelected.DoThreadSafeAsync(x =>
                        {
                            for (int i = x.Nodes.Count - 1; i >= 0; --i)
                            {
                                TreeNode objLoop = x.Nodes[i];
                                if (objLoop?.Tag is Location objLocation && !lstLocations.Contains(objLocation))
                                {
                                    foreach (TreeNode objInnerLoop in objLoop.Nodes)
                                    {
                                        lstNodesNeedingProcessing.Add(objInnerLoop);
                                        objInnerLoop.Remove();
                                    }
                                    objLoop.Remove();
                                }
                            }
                        }, token).ConfigureAwait(false);
                        if (lstNodesNeedingProcessing.Count > 0)
                        {
                            if (nodRoot == null)
                            {
                                nodRoot = new TreeNode
                                {
                                    Tag = strNodeName,
                                    Text = await LanguageManager.GetStringAsync(strNodeName, token: token).ConfigureAwait(false)
                                };
                                TreeNode root = nodRoot;
                                await treSelected.DoThreadSafeAsync(x => x.Nodes.Insert(0, root), token).ConfigureAwait(false);
                            }

                            TreeNode root2 = nodRoot;
                            await treSelected.DoThreadSafeAsync(() =>
                            {
                                foreach (TreeNode objNode in lstNodesNeedingProcessing)
                                    root2.Nodes.Add(objNode);
                            }, token).ConfigureAwait(false);
                        }
                    }
                        break;
                }

                await treSelected.DoThreadSafeAsync(x => x.SelectedNode = x.FindNode(strSelectedId), token).ConfigureAwait(false);
            }
            finally
            {
                await objCursorWait.DisposeAsync().ConfigureAwait(false);
            }
        }

        #endregion Locations

        protected async Task RefreshWeaponsClearBindings(TreeView treWeapons, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (treWeapons == null)
                return;
            CursorWait objCursorWait = await CursorWait.NewAsync(this, token: token).ConfigureAwait(false);
            try
            {
                SkipUpdate = true;
                try
                {
                    await CharacterObject.Weapons.ForEachWithSideEffectsAsync(
                        objWeapon =>
                            objWeapon.SetupChildrenWeaponsCollectionChangedAsync(false, treWeapons, token: token),
                        token).ConfigureAwait(false);
                }
                finally
                {
                    SkipUpdate = false;
                }
            }
            finally
            {
                await objCursorWait.DisposeAsync().ConfigureAwait(false);
            }
        }

        protected async Task RefreshWeapons(TreeView treWeapons, ContextMenuStrip cmsWeaponLocation, ContextMenuStrip cmsWeapon, ContextMenuStrip cmsWeaponAccessory, ContextMenuStrip cmsWeaponAccessoryGear, NotifyCollectionChangedEventArgs e = null, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (treWeapons == null)
                return;
            CursorWait objCursorWait = await CursorWait.NewAsync(this, token: token).ConfigureAwait(false);
            SkipUpdate = true;
            try
            {
                string strSelectedId
                    = (await treWeapons.DoThreadSafeFuncAsync(x => x.SelectedNode?.Tag, token).ConfigureAwait(false) as IHasInternalId)
                    ?.InternalId ?? string.Empty;

                TreeNode nodRoot = null;

                if (e == null ||
                    e.Action == NotifyCollectionChangedAction.Reset)
                {
                    await treWeapons.DoThreadSafeAsync(x => x.SuspendLayout(), token).ConfigureAwait(false);
                    try
                    {
                        await treWeapons.DoThreadSafeAsync(x => x.Nodes.Clear(), token).ConfigureAwait(false);

                        // Start by populating Locations.
                        await CharacterObject.WeaponLocations
                            .ForEachAsync(
                                async objLocation =>
                                {
                                    TreeNode objNode = await objLocation.CreateTreeNode(cmsWeaponLocation, token).ConfigureAwait(false);
                                    await treWeapons.DoThreadSafeAsync(
                                        x => x.Nodes.Add(objNode),
                                        token).ConfigureAwait(false);
                                }, token).ConfigureAwait(false);

                        await CharacterObject.Weapons.ForEachWithSideEffectsAsync(async objWeapon =>
                        {
                            await AddToTree(objWeapon, -1, false).ConfigureAwait(false);
                            await objWeapon.SetupChildrenWeaponsCollectionChangedAsync(
                                true, treWeapons, cmsWeapon, cmsWeaponAccessory,
                                cmsWeaponAccessoryGear, MakeDirtyWithCharacterUpdate, token).ConfigureAwait(false);
                        }, token).ConfigureAwait(false);

                        await treWeapons.DoThreadSafeAsync(x => x.SelectedNode = x.FindNode(strSelectedId),
                                                           token).ConfigureAwait(false);
                    }
                    finally
                    {
                        await treWeapons.DoThreadSafeAsync(x => x.ResumeLayout(), GenericToken).ConfigureAwait(false);
                    }
                }
                else
                {
                    nodRoot = await treWeapons.DoThreadSafeFuncAsync(x => x.FindNode("Node_SelectedWeapons", false),
                                                                     token).ConfigureAwait(false);

                    switch (e.Action)
                    {
                        case NotifyCollectionChangedAction.Add:
                        {
                            int intNewIndex = e.NewStartingIndex;
                            foreach (Weapon objWeapon in e.NewItems)
                            {
                                await AddToTree(objWeapon, intNewIndex).ConfigureAwait(false);
                                ++intNewIndex;
                                await objWeapon.SetupChildrenWeaponsCollectionChangedAsync(true, treWeapons, cmsWeapon,
                                    cmsWeaponAccessory, cmsWeaponAccessoryGear, MakeDirtyWithCharacterUpdate, token).ConfigureAwait(false);
                            }
                        }
                            break;

                        case NotifyCollectionChangedAction.Remove:
                        {
                            foreach (Weapon objWeapon in e.OldItems)
                            {
                                await objWeapon.SetupChildrenWeaponsCollectionChangedAsync(false, treWeapons, token: token).ConfigureAwait(false);
                                await treWeapons.DoThreadSafeAsync(x => x.FindNode(objWeapon.InternalId)?.Remove(),
                                                                   token).ConfigureAwait(false);
                            }

                            await treWeapons.DoThreadSafeAsync(() =>
                            {
                                if (nodRoot != null && nodRoot.Nodes.Count == 0)
                                {
                                    nodRoot.Remove();
                                }
                            }, token).ConfigureAwait(false);
                        }
                            break;

                        case NotifyCollectionChangedAction.Replace:
                        {
                            foreach (Weapon objWeapon in e.OldItems)
                            {
                                await objWeapon.SetupChildrenWeaponsCollectionChangedAsync(false, treWeapons, token: token).ConfigureAwait(false);
                                await treWeapons.DoThreadSafeAsync(x => x.FindNode(objWeapon.InternalId)?.Remove(),
                                                                   token).ConfigureAwait(false);
                            }

                            int intNewIndex = e.NewStartingIndex;
                            foreach (Weapon objWeapon in e.NewItems)
                            {
                                await AddToTree(objWeapon, intNewIndex).ConfigureAwait(false);
                                ++intNewIndex;
                                await objWeapon.SetupChildrenWeaponsCollectionChangedAsync(true, treWeapons, cmsWeapon,
                                    cmsWeaponAccessory, cmsWeaponAccessoryGear, MakeDirtyWithCharacterUpdate, token).ConfigureAwait(false);
                            }

                            await treWeapons.DoThreadSafeAsync(x =>
                            {
                                if (nodRoot != null && nodRoot.Nodes.Count == 0)
                                {
                                    nodRoot.Remove();
                                }

                                x.SelectedNode = x.FindNode(strSelectedId);
                            }, token).ConfigureAwait(false);
                        }
                            break;

                        case NotifyCollectionChangedAction.Move:
                        {
                            await treWeapons.DoThreadSafeAsync(x =>
                            {
                                foreach (Weapon objWeapon in e.OldItems)
                                {
                                    x.FindNode(objWeapon.InternalId)?.Remove();
                                }
                            }, token).ConfigureAwait(false);

                            int intNewIndex = e.NewStartingIndex;
                            foreach (Weapon objWeapon in e.NewItems)
                            {
                                await AddToTree(objWeapon, intNewIndex).ConfigureAwait(false);
                                ++intNewIndex;
                            }

                            await treWeapons.DoThreadSafeAsync(x =>
                            {
                                if (nodRoot != null && nodRoot.Nodes.Count == 0)
                                {
                                    nodRoot.Remove();
                                }

                                x.SelectedNode = x.FindNode(strSelectedId);
                            }, token).ConfigureAwait(false);
                            break;
                        }
                    }
                }

                async ValueTask AddToTree(Weapon objWeapon, int intIndex = -1, bool blnSingleAdd = true)
                {
                    TreeNode objNode = await objWeapon.CreateTreeNode(cmsWeapon, cmsWeaponAccessory, cmsWeaponAccessoryGear, token).ConfigureAwait(false);
                    if (objNode == null)
                        return;
                    TreeNode nodParent = null;
                    if (objWeapon.Location != null)
                    {
                        nodParent = await treWeapons.DoThreadSafeFuncAsync(
                            x => x.FindNode(objWeapon.Location.InternalId, false), token).ConfigureAwait(false);
                    }

                    if (nodParent == null)
                    {
                        if (nodRoot == null)
                        {
                            nodRoot = new TreeNode
                            {
                                Tag = "Node_SelectedWeapons",
                                Text = await LanguageManager.GetStringAsync("Node_SelectedWeapons", token: token).ConfigureAwait(false)
                            };
                            // ReSharper disable once AssignNullToNotNullAttribute
                            await treWeapons.DoThreadSafeAsync(x => x.Nodes.Insert(0, nodRoot), token).ConfigureAwait(false);
                        }

                        nodParent = nodRoot;
                    }

                    await treWeapons.DoThreadSafeAsync(x =>
                    {
                        if (nodParent == null)
                            return;
                        if (intIndex >= 0)
                            nodParent.Nodes.Insert(intIndex, objNode);
                        else
                            nodParent.Nodes.Add(objNode);
                        nodParent.Expand();
                        if (blnSingleAdd)
                            x.SelectedNode = objNode;
                    }, token).ConfigureAwait(false);
                }
            }
            finally
            {
                SkipUpdate = false;
                await objCursorWait.DisposeAsync().ConfigureAwait(false);
            }
        }

        protected async Task RefreshArmorClearBindings(TreeView treArmor, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (treArmor == null)
                return;
            CursorWait objCursorWait = await CursorWait.NewAsync(this, token: token).ConfigureAwait(false);
            try
            {
                SkipUpdate = true;
                try
                {
                    await CharacterObject.Armor.ForEachWithSideEffectsAsync(async objArmor =>
                    {
                        await objArmor.ArmorMods.RemoveTaggedAsyncBeforeClearCollectionChangedAsync(treArmor, token)
                            .ConfigureAwait(false);
                        await objArmor.ArmorMods.RemoveTaggedAsyncCollectionChangedAsync(treArmor, token)
                            .ConfigureAwait(false);
                        await objArmor.GearChildren.RemoveTaggedAsyncBeforeClearCollectionChangedAsync(treArmor, token)
                            .ConfigureAwait(false);
                        await objArmor.GearChildren.RemoveTaggedAsyncCollectionChangedAsync(treArmor, token)
                            .ConfigureAwait(false);
                        await objArmor.GearChildren
                            .ForEachWithSideEffectsAsync(
                                objGear =>
                                    objGear.SetupChildrenGearsCollectionChangedAsync(false, treArmor, token: token),
                                token)
                            .ConfigureAwait(false);
                        await objArmor.ArmorMods.ForEachWithSideEffectsAsync(async objArmorMod =>
                        {
                            await objArmorMod.GearChildren
                                .RemoveTaggedAsyncBeforeClearCollectionChangedAsync(treArmor, token)
                                .ConfigureAwait(false);
                            await objArmorMod.GearChildren.RemoveTaggedAsyncCollectionChangedAsync(treArmor, token)
                                .ConfigureAwait(false);
                            await objArmorMod.GearChildren
                                .ForEachWithSideEffectsAsync(
                                    objGear =>
                                        objGear.SetupChildrenGearsCollectionChangedAsync(
                                            false, treArmor, token: token),
                                    token)
                                .ConfigureAwait(false);
                        }, token).ConfigureAwait(false);
                    }, token).ConfigureAwait(false);
                }
                finally
                {
                    SkipUpdate = false;
                }
            }
            finally
            {
                await objCursorWait.DisposeAsync().ConfigureAwait(false);
            }
        }

        protected async Task RefreshArmor(TreeView treArmor, ContextMenuStrip cmsArmorLocation, ContextMenuStrip cmsArmor, ContextMenuStrip cmsArmorMod, ContextMenuStrip cmsArmorGear, NotifyCollectionChangedEventArgs e = null, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (treArmor == null)
                return;
            CursorWait objCursorWait = await CursorWait.NewAsync(this, token: token).ConfigureAwait(false);
            try
            {
                SkipUpdate = true;
                try
                {
                    string strSelectedId
                        = (await treArmor.DoThreadSafeFuncAsync(x => x.SelectedNode?.Tag, token)
                            .ConfigureAwait(false) as IHasInternalId)
                        ?.InternalId ?? string.Empty;

                    TreeNode nodRoot = null;

                    if (e == null ||
                        e.Action == NotifyCollectionChangedAction.Reset)
                    {
                        await treArmor.DoThreadSafeAsync(x => x.SuspendLayout(), token).ConfigureAwait(false);
                        try
                        {
                            await treArmor.DoThreadSafeAsync(x => x.Nodes.Clear(), token).ConfigureAwait(false);

                            // Start by populating Locations.
                            await CharacterObject.ArmorLocations
                                .ForEachAsync(
                                    async objLocation =>
                                    {
                                        TreeNode objNode = await objLocation.CreateTreeNode(cmsArmorLocation, token)
                                            .ConfigureAwait(false);
                                        await treArmor.DoThreadSafeAsync(
                                            x => x.Nodes.Add(objNode),
                                            token).ConfigureAwait(false);
                                    }, token).ConfigureAwait(false);

                            // Add Armor.
                            await CharacterObject.Armor.ForEachWithSideEffectsAsync(async objArmor =>
                            {
                                await AddToTree(objArmor, -1, false).ConfigureAwait(false);

                                Task FuncArmorModsBeforeClearToAdd(object x, NotifyCollectionChangedEventArgs y,
                                    CancellationToken innerToken = default) =>
                                    RefreshArmorModsClearBindings(treArmor, objArmor, innerToken);

                                Task FuncArmorModsToAdd(object x, NotifyCollectionChangedEventArgs y,
                                    CancellationToken innerToken = default) =>
                                    RefreshArmorMods(treArmor, objArmor, cmsArmorMod, cmsArmorGear, y, innerToken);

                                Task FuncArmorGearBeforeClearToAdd(object x, NotifyCollectionChangedEventArgs y,
                                    CancellationToken innerToken = default) =>
                                    objArmor.RefreshChildrenGearsClearBindings(treArmor, y, innerToken);

                                Task FuncArmorGearToAdd(object x, NotifyCollectionChangedEventArgs y,
                                    CancellationToken innerToken = default) =>
                                    objArmor.RefreshChildrenGears(
                                        treArmor, cmsArmorGear, null, () => objArmor.ArmorMods.Count, y,
                                        MakeDirtyWithCharacterUpdate, token: innerToken);

                                objArmor.ArmorMods.AddTaggedCollectionChanged(
                                    treArmor, MakeDirtyWithCharacterUpdate);
                                objArmor.ArmorMods.AddTaggedCollectionChanged(treArmor,
                                    FuncArmorModsToAdd);
                                objArmor.ArmorMods.AddTaggedBeforeClearCollectionChanged(treArmor,
                                    FuncArmorModsBeforeClearToAdd);
                                objArmor.GearChildren.AddTaggedCollectionChanged(
                                    treArmor, MakeDirtyWithCharacterUpdate);
                                objArmor.GearChildren.AddTaggedCollectionChanged(treArmor,
                                    FuncArmorGearToAdd);
                                objArmor.GearChildren.AddTaggedBeforeClearCollectionChanged(treArmor,
                                    FuncArmorGearBeforeClearToAdd);
                                await objArmor.GearChildren.ForEachWithSideEffectsAsync(objGear =>
                                        objGear.SetupChildrenGearsCollectionChangedAsync(
                                            true, treArmor, cmsArmorGear, null,
                                            MakeDirtyWithCharacterUpdate, token: token), token)
                                    .ConfigureAwait(false);
                                await objArmor.ArmorMods.ForEachWithSideEffectsAsync(objArmorMod =>
                                {
                                    Task FuncDelegateBeforeClearToAdd(object x, NotifyCollectionChangedEventArgs y,
                                        CancellationToken innerToken = default) =>
                                        objArmorMod.RefreshChildrenGearsClearBindings(treArmor, y, innerToken);

                                    Task FuncDelegateToAdd(object x, NotifyCollectionChangedEventArgs y,
                                        CancellationToken innerToken = default) =>
                                        objArmorMod.RefreshChildrenGears(
                                            treArmor, cmsArmorGear, null, null, y, MakeDirtyWithCharacterUpdate,
                                            token: innerToken);

                                    objArmorMod.GearChildren.AddTaggedCollectionChanged(
                                        treArmor, MakeDirtyWithCharacterUpdate);
                                    objArmorMod.GearChildren.AddTaggedCollectionChanged(
                                        treArmor, FuncDelegateToAdd);
                                    objArmorMod.GearChildren.AddTaggedBeforeClearCollectionChanged(treArmor,
                                        FuncDelegateBeforeClearToAdd);
                                    return objArmorMod.GearChildren.ForEachWithSideEffectsAsync(objGear =>
                                            objGear
                                                .SetupChildrenGearsCollectionChangedAsync(
                                                    true, treArmor, cmsArmorGear,
                                                    null,
                                                    MakeDirtyWithCharacterUpdate, token: token),
                                        token);
                                }, token).ConfigureAwait(false);
                            }, token).ConfigureAwait(false);

                            await treArmor.DoThreadSafeAsync(x => x.SelectedNode = x.FindNode(strSelectedId), token)
                                .ConfigureAwait(false);
                        }
                        finally
                        {
                            await treArmor.DoThreadSafeAsync(x => x.ResumeLayout(), GenericToken).ConfigureAwait(false);
                        }
                    }
                    else
                    {
                        nodRoot = await treArmor.DoThreadSafeFuncAsync(x => x.FindNode("Node_SelectedArmor", false),
                            token).ConfigureAwait(false);

                        switch (e.Action)
                        {
                            case NotifyCollectionChangedAction.Add:
                            {
                                int intNewIndex = e.NewStartingIndex;
                                foreach (Armor objArmor in e.NewItems)
                                {
                                    await AddToTree(objArmor, intNewIndex).ConfigureAwait(false);

                                    Task FuncArmorModsBeforeClearToAdd(object x, NotifyCollectionChangedEventArgs y,
                                        CancellationToken innerToken = default) =>
                                        RefreshArmorModsClearBindings(treArmor, objArmor, innerToken);

                                    Task FuncArmorModsToAdd(object x, NotifyCollectionChangedEventArgs y,
                                        CancellationToken innerToken = default) =>
                                        RefreshArmorMods(treArmor, objArmor, cmsArmorMod, cmsArmorGear, y, innerToken);

                                    Task FuncArmorGearBeforeClearToAdd(object x, NotifyCollectionChangedEventArgs y,
                                        CancellationToken innerToken = default) =>
                                        objArmor.RefreshChildrenGearsClearBindings(treArmor, y, innerToken);

                                    Task FuncArmorGearToAdd(object x, NotifyCollectionChangedEventArgs y,
                                        CancellationToken innerToken = default) =>
                                        objArmor.RefreshChildrenGears(
                                            treArmor, cmsArmorGear, null, () => objArmor.ArmorMods.Count, y,
                                            MakeDirtyWithCharacterUpdate, token: innerToken);

                                    objArmor.ArmorMods.AddTaggedCollectionChanged(
                                        treArmor, MakeDirtyWithCharacterUpdate);
                                    objArmor.ArmorMods.AddTaggedCollectionChanged(treArmor,
                                        FuncArmorModsToAdd);
                                    objArmor.ArmorMods.AddTaggedBeforeClearCollectionChanged(treArmor,
                                        FuncArmorModsBeforeClearToAdd);
                                    objArmor.GearChildren.AddTaggedCollectionChanged(
                                        treArmor, MakeDirtyWithCharacterUpdate);
                                    objArmor.GearChildren.AddTaggedCollectionChanged(treArmor,
                                        FuncArmorGearToAdd);
                                    objArmor.GearChildren.AddTaggedBeforeClearCollectionChanged(treArmor,
                                        FuncArmorGearBeforeClearToAdd);
                                    await objArmor.GearChildren.ForEachWithSideEffectsAsync(objGear =>
                                            objGear.SetupChildrenGearsCollectionChangedAsync(
                                                true, treArmor, cmsArmorGear, null,
                                                MakeDirtyWithCharacterUpdate, token: token), token)
                                        .ConfigureAwait(false);
                                    await objArmor.ArmorMods.ForEachWithSideEffectsAsync(objArmorMod =>
                                    {
                                        Task FuncDelegateBeforeClearToAdd(object x, NotifyCollectionChangedEventArgs y,
                                            CancellationToken innerToken = default) =>
                                            objArmorMod.RefreshChildrenGearsClearBindings(treArmor, y, innerToken);

                                        Task FuncDelegateToAdd(object x, NotifyCollectionChangedEventArgs y,
                                            CancellationToken innerToken = default) =>
                                            objArmorMod.RefreshChildrenGears(
                                                treArmor, cmsArmorGear, null, null, y, MakeDirtyWithCharacterUpdate,
                                                token: innerToken);

                                        objArmorMod.GearChildren.AddTaggedCollectionChanged(
                                            treArmor, MakeDirtyWithCharacterUpdate);
                                        objArmorMod.GearChildren.AddTaggedCollectionChanged(
                                            treArmor, FuncDelegateToAdd);
                                        objArmorMod.GearChildren.AddTaggedBeforeClearCollectionChanged(treArmor,
                                            FuncDelegateBeforeClearToAdd);
                                        return objArmorMod.GearChildren.ForEachWithSideEffectsAsync(objGear =>
                                                objGear
                                                    .SetupChildrenGearsCollectionChangedAsync(
                                                        true, treArmor, cmsArmorGear,
                                                        null,
                                                        MakeDirtyWithCharacterUpdate, token: token),
                                            token);
                                    }, token).ConfigureAwait(false);

                                    ++intNewIndex;
                                }

                                break;
                            }
                            case NotifyCollectionChangedAction.Remove:
                            {
                                foreach (Armor objArmor in e.OldItems)
                                {
                                    await objArmor.ArmorMods
                                        .RemoveTaggedAsyncBeforeClearCollectionChangedAsync(treArmor, token)
                                        .ConfigureAwait(false);
                                    await objArmor.ArmorMods.RemoveTaggedAsyncCollectionChangedAsync(treArmor, token)
                                        .ConfigureAwait(false);
                                    await objArmor.GearChildren
                                        .RemoveTaggedAsyncBeforeClearCollectionChangedAsync(treArmor, token)
                                        .ConfigureAwait(false);
                                    await objArmor.GearChildren.RemoveTaggedAsyncCollectionChangedAsync(treArmor, token)
                                        .ConfigureAwait(false);
                                    await objArmor.GearChildren
                                        .ForEachWithSideEffectsAsync(
                                            objGear =>
                                                objGear.SetupChildrenGearsCollectionChangedAsync(false, treArmor,
                                                    token: token),
                                            token)
                                        .ConfigureAwait(false);
                                    await objArmor.ArmorMods.ForEachWithSideEffectsAsync(async objArmorMod =>
                                    {
                                        await objArmorMod.GearChildren
                                            .RemoveTaggedAsyncBeforeClearCollectionChangedAsync(treArmor, token)
                                            .ConfigureAwait(false);
                                        await objArmorMod.GearChildren
                                            .RemoveTaggedAsyncCollectionChangedAsync(treArmor, token)
                                            .ConfigureAwait(false);
                                        await objArmorMod.GearChildren
                                            .ForEachWithSideEffectsAsync(
                                                objGear =>
                                                    objGear.SetupChildrenGearsCollectionChangedAsync(
                                                        false, treArmor, token: token),
                                                token)
                                            .ConfigureAwait(false);
                                    }, token).ConfigureAwait(false);

                                    await treArmor.DoThreadSafeAsync(x => x.FindNode(objArmor.InternalId)?.Remove(),
                                        token).ConfigureAwait(false);
                                }

                                await treArmor.DoThreadSafeAsync(() =>
                                {
                                    if (nodRoot != null && nodRoot.Nodes.Count == 0)
                                    {
                                        nodRoot.Remove();
                                    }
                                }, token).ConfigureAwait(false);

                                break;
                            }
                            case NotifyCollectionChangedAction.Replace:
                            {
                                foreach (Armor objArmor in e.OldItems)
                                {
                                    await objArmor.ArmorMods
                                        .RemoveTaggedAsyncBeforeClearCollectionChangedAsync(treArmor, token)
                                        .ConfigureAwait(false);
                                    await objArmor.ArmorMods.RemoveTaggedAsyncCollectionChangedAsync(treArmor, token)
                                        .ConfigureAwait(false);
                                    await objArmor.GearChildren
                                        .RemoveTaggedAsyncBeforeClearCollectionChangedAsync(treArmor, token)
                                        .ConfigureAwait(false);
                                    await objArmor.GearChildren.RemoveTaggedAsyncCollectionChangedAsync(treArmor, token)
                                        .ConfigureAwait(false);
                                    await objArmor.GearChildren
                                        .ForEachWithSideEffectsAsync(
                                            objGear =>
                                                objGear.SetupChildrenGearsCollectionChangedAsync(false, treArmor,
                                                    token: token),
                                            token)
                                        .ConfigureAwait(false);
                                    await objArmor.ArmorMods.ForEachWithSideEffectsAsync(async objArmorMod =>
                                    {
                                        await objArmorMod.GearChildren
                                            .RemoveTaggedAsyncBeforeClearCollectionChangedAsync(treArmor, token)
                                            .ConfigureAwait(false);
                                        await objArmorMod.GearChildren
                                            .RemoveTaggedAsyncCollectionChangedAsync(treArmor, token)
                                            .ConfigureAwait(false);
                                        await objArmorMod.GearChildren
                                            .ForEachWithSideEffectsAsync(
                                                objGear =>
                                                    objGear.SetupChildrenGearsCollectionChangedAsync(
                                                        false, treArmor, token: token),
                                                token)
                                            .ConfigureAwait(false);
                                    }, token).ConfigureAwait(false);

                                    await treArmor.DoThreadSafeAsync(x => x.FindNode(objArmor.InternalId)?.Remove(),
                                        token).ConfigureAwait(false);
                                }

                                int intNewIndex = e.NewStartingIndex;
                                foreach (Armor objArmor in e.NewItems)
                                {
                                    await AddToTree(objArmor, intNewIndex).ConfigureAwait(false);

                                    Task FuncArmorModsBeforeClearToAdd(object x, NotifyCollectionChangedEventArgs y,
                                        CancellationToken innerToken = default) =>
                                        RefreshArmorModsClearBindings(treArmor, objArmor, innerToken);

                                    Task FuncArmorModsToAdd(object x, NotifyCollectionChangedEventArgs y,
                                        CancellationToken innerToken = default) =>
                                        RefreshArmorMods(treArmor, objArmor, cmsArmorMod, cmsArmorGear, y, innerToken);

                                    Task FuncArmorGearBeforeClearToAdd(object x, NotifyCollectionChangedEventArgs y,
                                        CancellationToken innerToken = default) =>
                                        objArmor.RefreshChildrenGearsClearBindings(treArmor, y, innerToken);

                                    Task FuncArmorGearToAdd(object x, NotifyCollectionChangedEventArgs y,
                                        CancellationToken innerToken = default) =>
                                        objArmor.RefreshChildrenGears(
                                            treArmor, cmsArmorGear, null, () => objArmor.ArmorMods.Count, y,
                                            MakeDirtyWithCharacterUpdate, token: innerToken);

                                    objArmor.ArmorMods.AddTaggedCollectionChanged(
                                        treArmor, MakeDirtyWithCharacterUpdate);
                                    objArmor.ArmorMods.AddTaggedCollectionChanged(treArmor,
                                        FuncArmorModsToAdd);
                                    objArmor.ArmorMods.AddTaggedBeforeClearCollectionChanged(treArmor,
                                        FuncArmorModsBeforeClearToAdd);
                                    objArmor.GearChildren.AddTaggedCollectionChanged(
                                        treArmor, MakeDirtyWithCharacterUpdate);
                                    objArmor.GearChildren.AddTaggedCollectionChanged(treArmor,
                                        FuncArmorGearToAdd);
                                    objArmor.GearChildren.AddTaggedBeforeClearCollectionChanged(treArmor,
                                        FuncArmorGearBeforeClearToAdd);
                                    await objArmor.GearChildren.ForEachWithSideEffectsAsync(objGear =>
                                            objGear.SetupChildrenGearsCollectionChangedAsync(
                                                true, treArmor, cmsArmorGear, null,
                                                MakeDirtyWithCharacterUpdate, token: token), token)
                                        .ConfigureAwait(false);
                                    await objArmor.ArmorMods.ForEachWithSideEffectsAsync(objArmorMod =>
                                    {
                                        Task FuncDelegateBeforeClearToAdd(object x, NotifyCollectionChangedEventArgs y,
                                            CancellationToken innerToken = default) =>
                                            objArmorMod.RefreshChildrenGearsClearBindings(treArmor, y, innerToken);

                                        Task FuncDelegateToAdd(object x, NotifyCollectionChangedEventArgs y,
                                            CancellationToken innerToken = default) =>
                                            objArmorMod.RefreshChildrenGears(
                                                treArmor, cmsArmorGear, null, null, y, MakeDirtyWithCharacterUpdate,
                                                token: innerToken);

                                        objArmorMod.GearChildren.AddTaggedCollectionChanged(
                                            treArmor, MakeDirtyWithCharacterUpdate);
                                        objArmorMod.GearChildren.AddTaggedCollectionChanged(
                                            treArmor, FuncDelegateToAdd);
                                        objArmorMod.GearChildren.AddTaggedBeforeClearCollectionChanged(treArmor,
                                            FuncDelegateBeforeClearToAdd);
                                        return objArmorMod.GearChildren.ForEachWithSideEffectsAsync(objGear =>
                                                objGear
                                                    .SetupChildrenGearsCollectionChangedAsync(
                                                        true, treArmor, cmsArmorGear,
                                                        null,
                                                        MakeDirtyWithCharacterUpdate, token: token),
                                            token);
                                    }, token).ConfigureAwait(false);

                                    ++intNewIndex;
                                }

                                await treArmor.DoThreadSafeAsync(x =>
                                {
                                    if (nodRoot != null && nodRoot.Nodes.Count == 0)
                                    {
                                        nodRoot.Remove();
                                    }

                                    x.SelectedNode = x.FindNode(strSelectedId);
                                }, token).ConfigureAwait(false);
                                break;
                            }
                            case NotifyCollectionChangedAction.Move:
                            {
                                await treArmor.DoThreadSafeAsync(x =>
                                {
                                    foreach (Armor objArmor in e.OldItems)
                                    {
                                        x.FindNode(objArmor.InternalId)?.Remove();
                                    }
                                }, token).ConfigureAwait(false);

                                int intNewIndex = e.NewStartingIndex;
                                foreach (Armor objArmor in e.NewItems)
                                {
                                    await AddToTree(objArmor, intNewIndex).ConfigureAwait(false);
                                    ++intNewIndex;
                                }

                                await treArmor.DoThreadSafeAsync(x =>
                                {
                                    if (nodRoot != null && nodRoot.Nodes.Count == 0)
                                    {
                                        nodRoot.Remove();
                                    }

                                    x.SelectedNode = x.FindNode(strSelectedId);
                                }, token).ConfigureAwait(false);
                                break;
                            }
                        }
                    }

                    async ValueTask AddToTree(Armor objArmor, int intIndex = -1, bool blnSingleAdd = true)
                    {
                        TreeNode objNode = await objArmor.CreateTreeNode(cmsArmor, cmsArmorMod, cmsArmorGear, token)
                            .ConfigureAwait(false);
                        if (objNode == null)
                            return;
                        TreeNode nodParent = null;
                        if (objArmor.Location != null)
                        {
                            nodParent = await treArmor.DoThreadSafeFuncAsync(
                                x => x.FindNode(objArmor.Location.InternalId, false), token).ConfigureAwait(false);
                        }

                        if (nodParent == null)
                        {
                            if (nodRoot == null)
                            {
                                nodRoot = new TreeNode
                                {
                                    Tag = "Node_SelectedArmor",
                                    Text = await LanguageManager.GetStringAsync("Node_SelectedArmor", token: token)
                                        .ConfigureAwait(false)
                                };
                                // ReSharper disable once AssignNullToNotNullAttribute
                                await treArmor.DoThreadSafeAsync(x => x.Nodes.Insert(0, nodRoot), token)
                                    .ConfigureAwait(false);
                            }

                            nodParent = nodRoot;
                        }

                        await treArmor.DoThreadSafeAsync(x =>
                        {
                            if (nodParent == null)
                                return;
                            if (intIndex >= 0)
                                nodParent.Nodes.Insert(intIndex, objNode);
                            else
                                nodParent.Nodes.Add(objNode);
                            nodParent.Expand();
                            if (blnSingleAdd)
                                x.SelectedNode = objNode;
                        }, token).ConfigureAwait(false);
                    }
                }
                finally
                {
                    SkipUpdate = false;
                }
            }
            finally
            {
                await objCursorWait.DisposeAsync().ConfigureAwait(false);
            }
        }

        protected async Task RefreshArmorModsClearBindings(TreeView treArmor, Armor objArmor, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (treArmor == null || objArmor == null)
                return;
            CursorWait objCursorWait = await CursorWait.NewAsync(this, token: token).ConfigureAwait(false);
            try
            {
                SkipUpdate = true;
                try
                {
                    await objArmor.ArmorMods.ForEachWithSideEffectsAsync(async objArmorMod =>
                    {
                        await objArmorMod.GearChildren
                            .RemoveTaggedAsyncBeforeClearCollectionChangedAsync(treArmor, token)
                            .ConfigureAwait(false);
                        await objArmorMod.GearChildren.RemoveTaggedAsyncCollectionChangedAsync(treArmor, token)
                            .ConfigureAwait(false);
                        await objArmorMod.GearChildren.ForEachWithSideEffectsAsync(
                            objGear => objGear.SetupChildrenGearsCollectionChangedAsync(false, treArmor, token: token),
                            token).ConfigureAwait(false);
                    }, token).ConfigureAwait(false);
                }
                finally
                {
                    SkipUpdate = false;
                }
            }
            finally
            {
                await objCursorWait.DisposeAsync().ConfigureAwait(false);
            }
        }

        protected async Task RefreshArmorMods(TreeView treArmor, Armor objArmor, ContextMenuStrip cmsArmorMod, ContextMenuStrip cmsArmorGear, NotifyCollectionChangedEventArgs e, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (treArmor == null || objArmor == null || e == null)
                return;
            TreeNode nodArmor = await treArmor.DoThreadSafeFuncAsync(x => x.FindNode(objArmor.InternalId), token).ConfigureAwait(false);
            if (nodArmor == null)
                return;
            CursorWait objCursorWait = await CursorWait.NewAsync(this, token: token).ConfigureAwait(false);
            try
            {
                SkipUpdate = true;
                try
                {
                    switch (e.Action)
                    {
                        case NotifyCollectionChangedAction.Add:
                        {
                            int intNewIndex = e.NewStartingIndex;
                            foreach (ArmorMod objArmorMod in e.NewItems)
                            {
                                await AddToTree(objArmorMod, intNewIndex).ConfigureAwait(false);

                                Task FuncDelegateBeforeClearToAdd(object x, NotifyCollectionChangedEventArgs y,
                                    CancellationToken innerToken = default) =>
                                    objArmorMod.RefreshChildrenGearsClearBindings(treArmor, y, innerToken);

                                Task FuncDelegateToAdd(object x, NotifyCollectionChangedEventArgs y,
                                    CancellationToken innerToken = default) =>
                                    objArmorMod.RefreshChildrenGears(
                                        treArmor, cmsArmorGear, null, null, y, MakeDirtyWithCharacterUpdate,
                                        token: innerToken);

                                objArmorMod.GearChildren.AddTaggedCollectionChanged(
                                    treArmor, MakeDirtyWithCharacterUpdate);
                                objArmorMod.GearChildren.AddTaggedCollectionChanged(
                                    treArmor, FuncDelegateToAdd);
                                objArmorMod.GearChildren.AddTaggedBeforeClearCollectionChanged(treArmor,
                                    FuncDelegateBeforeClearToAdd);
                                await objArmorMod.GearChildren.ForEachWithSideEffectsAsync(objGear =>
                                            objGear.SetupChildrenGearsCollectionChangedAsync(
                                                true, treArmor, cmsArmorGear, null, MakeDirtyWithCharacterUpdate,
                                                token),
                                        token)
                                    .ConfigureAwait(false);
                                ++intNewIndex;
                            }

                            break;
                        }
                        case NotifyCollectionChangedAction.Remove:
                        {
                            foreach (ArmorMod objArmorMod in e.OldItems)
                            {
                                await objArmorMod.GearChildren
                                    .RemoveTaggedAsyncBeforeClearCollectionChangedAsync(treArmor, token)
                                    .ConfigureAwait(false);
                                await objArmorMod.GearChildren.RemoveTaggedAsyncCollectionChangedAsync(treArmor, token)
                                    .ConfigureAwait(false);
                                await objArmorMod.GearChildren
                                    .ForEachWithSideEffectsAsync(
                                        objGear => objGear.SetupChildrenGearsCollectionChangedAsync(false, treArmor,
                                            token: token), token).ConfigureAwait(false);
                                await treArmor.DoThreadSafeAsync(
                                    () => nodArmor.FindNode(objArmorMod.InternalId)?.Remove(),
                                    token).ConfigureAwait(false);
                            }

                            break;
                        }
                        case NotifyCollectionChangedAction.Replace:
                        {
                            string strSelectedId
                                = (await treArmor.DoThreadSafeFuncAsync(x => x.SelectedNode?.Tag, token)
                                        .ConfigureAwait(false) as
                                    IHasInternalId)
                                ?.InternalId ?? string.Empty;
                            foreach (ArmorMod objArmorMod in e.OldItems)
                            {
                                await objArmorMod.GearChildren
                                    .RemoveTaggedAsyncBeforeClearCollectionChangedAsync(treArmor, token)
                                    .ConfigureAwait(false);
                                await objArmorMod.GearChildren.RemoveTaggedAsyncCollectionChangedAsync(treArmor, token)
                                    .ConfigureAwait(false);
                                await objArmorMod.GearChildren
                                    .ForEachWithSideEffectsAsync(
                                        objGear => objGear.SetupChildrenGearsCollectionChangedAsync(false, treArmor,
                                            token: token), token).ConfigureAwait(false);
                                await treArmor.DoThreadSafeAsync(
                                    () => nodArmor.FindNode(objArmorMod.InternalId)?.Remove(),
                                    token).ConfigureAwait(false);
                            }

                            int intNewIndex = e.NewStartingIndex;
                            foreach (ArmorMod objArmorMod in e.NewItems)
                            {
                                await AddToTree(objArmorMod, intNewIndex).ConfigureAwait(false);

                                Task FuncDelegateBeforeClearToAdd(object x, NotifyCollectionChangedEventArgs y,
                                    CancellationToken innerToken = default) =>
                                    objArmorMod.RefreshChildrenGearsClearBindings(treArmor, y, innerToken);

                                Task FuncDelegateToAdd(object x, NotifyCollectionChangedEventArgs y,
                                    CancellationToken innerToken = default) =>
                                    objArmorMod.RefreshChildrenGears(
                                        treArmor, cmsArmorGear, null, null, y, MakeDirtyWithCharacterUpdate,
                                        token: innerToken);

                                objArmorMod.GearChildren.AddTaggedCollectionChanged(
                                    treArmor, MakeDirtyWithCharacterUpdate);
                                objArmorMod.GearChildren.AddTaggedCollectionChanged(
                                    treArmor, FuncDelegateToAdd);
                                objArmorMod.GearChildren.AddTaggedBeforeClearCollectionChanged(treArmor,
                                    FuncDelegateBeforeClearToAdd);
                                await objArmorMod.GearChildren.ForEachWithSideEffectsAsync(objGear =>
                                            objGear.SetupChildrenGearsCollectionChangedAsync(
                                                true, treArmor, cmsArmorGear, null, MakeDirtyWithCharacterUpdate,
                                                token),
                                        token)
                                    .ConfigureAwait(false);
                                ++intNewIndex;
                            }

                            await treArmor.DoThreadSafeAsync(x => x.SelectedNode = x.FindNode(strSelectedId), token)
                                .ConfigureAwait(false);
                            break;
                        }
                        case NotifyCollectionChangedAction.Move:
                        {
                            string strSelectedId
                                = (await treArmor.DoThreadSafeFuncAsync(x => x.SelectedNode?.Tag, token)
                                        .ConfigureAwait(false) as
                                    IHasInternalId)
                                ?.InternalId ?? string.Empty;
                            await treArmor.DoThreadSafeAsync(() =>
                            {
                                foreach (ArmorMod objArmorMod in e.OldItems)
                                {
                                    nodArmor.FindNode(objArmorMod.InternalId)?.Remove();
                                }
                            }, token).ConfigureAwait(false);

                            int intNewIndex = e.NewStartingIndex;
                            foreach (ArmorMod objArmorMod in e.NewItems)
                            {
                                await AddToTree(objArmorMod, intNewIndex).ConfigureAwait(false);
                                ++intNewIndex;
                            }

                            await treArmor.DoThreadSafeAsync(x => x.SelectedNode = x.FindNode(strSelectedId), token)
                                .ConfigureAwait(false);
                            break;
                        }
                        case NotifyCollectionChangedAction.Reset:
                        {
                            await treArmor.DoThreadSafeAsync(() =>
                            {
                                for (int i = nodArmor.Nodes.Count - 1; i >= 0; --i)
                                {
                                    TreeNode objNode = nodArmor.Nodes[i];
                                    if (objNode.Tag is ArmorMod objNodeMod &&
                                        !ReferenceEquals(objNodeMod.Parent, objArmor))
                                    {
                                        objNode.Remove();
                                    }
                                }
                            }, token).ConfigureAwait(false);

                            break;
                        }
                    }

                    async ValueTask AddToTree(ArmorMod objArmorMod, int intIndex = -1, bool blnSingleAdd = true)
                    {
                        TreeNode objNode = await objArmorMod.CreateTreeNode(cmsArmorMod, cmsArmorGear, token)
                            .ConfigureAwait(false);
                        if (objNode != null)
                        {
                            await treArmor.DoThreadSafeAsync(x =>
                            {
                                if (intIndex >= 0)
                                    nodArmor.Nodes.Insert(intIndex, objNode);
                                else
                                    nodArmor.Nodes.Add(objNode);
                                nodArmor.Expand();
                                if (blnSingleAdd)
                                    x.SelectedNode = objNode;
                            }, token).ConfigureAwait(false);
                        }
                    }
                }
                finally
                {
                    SkipUpdate = false;
                }
            }
            finally
            {
                await objCursorWait.DisposeAsync().ConfigureAwait(false);
            }
        }

        protected async Task RefreshGearsClearBindings(TreeView treGear, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (treGear == null)
                return;
            CursorWait objCursorWait = await CursorWait.NewAsync(this, token: token).ConfigureAwait(false);
            try
            {
                SkipUpdate = true;
                try
                {
                    await CharacterObject.Gear.ForEachWithSideEffectsAsync(
                        objGear => objGear.SetupChildrenGearsCollectionChangedAsync(false, treGear, token: token),
                        token).ConfigureAwait(false);
                }
                finally
                {
                    SkipUpdate = false;
                }
            }
            finally
            {
                await objCursorWait.DisposeAsync().ConfigureAwait(false);
            }
        }

        protected async Task RefreshGears(TreeView treGear, ContextMenuStrip cmsGearLocation, ContextMenuStrip cmsGear, ContextMenuStrip cmsCustomGear, bool blnCommlinksOnly, bool blnHideLoadedAmmo, NotifyCollectionChangedEventArgs e = null, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (treGear == null)
                return;
            CursorWait objCursorWait = await CursorWait.NewAsync(this, token: token).ConfigureAwait(false);
            try
            {
                SkipUpdate = true;
                try
                {
                    string strSelectedId
                        = (await treGear.DoThreadSafeFuncAsync(x => x.SelectedNode?.Tag, token)
                            .ConfigureAwait(false) as IHasInternalId)
                        ?.InternalId ?? string.Empty;

                    TreeNode nodRoot = null;

                    if (e == null ||
                        e.Action == NotifyCollectionChangedAction.Reset)
                    {
                        await treGear.DoThreadSafeAsync(x => x.SuspendLayout(), token).ConfigureAwait(false);
                        try
                        {
                            await treGear.DoThreadSafeAsync(x => x.Nodes.Clear(), token).ConfigureAwait(false);

                            // Start by populating Locations.
                            await CharacterObject.GearLocations
                                .ForEachAsync(
                                    async objLocation =>
                                    {
                                        TreeNode objNode = await objLocation.CreateTreeNode(cmsGearLocation, token)
                                            .ConfigureAwait(false);
                                        await treGear.DoThreadSafeAsync(
                                            x => x.Nodes.Add(objNode),
                                            token).ConfigureAwait(false);
                                    }, token).ConfigureAwait(false);

                            // Add Gear.
                            await CharacterObject.Gear.ForEachWithSideEffectsAsync(async objGear =>
                            {
                                await AddToTree(objGear, -1, false).ConfigureAwait(false);
                                await objGear.SetupChildrenGearsCollectionChangedAsync(
                                        true, treGear, cmsGear, cmsCustomGear, MakeDirtyWithCharacterUpdate, token)
                                    .ConfigureAwait(false);
                            }, token).ConfigureAwait(false);

                            await treGear.DoThreadSafeAsync(x => x.SelectedNode = x.FindNode(strSelectedId), token)
                                .ConfigureAwait(false);
                        }
                        finally
                        {
                            await treGear.DoThreadSafeAsync(x => x.ResumeLayout(), GenericToken).ConfigureAwait(false);
                        }
                    }
                    else
                    {
                        nodRoot = await treGear.DoThreadSafeFuncAsync(x => x.FindNode("Node_SelectedGear", false),
                            token).ConfigureAwait(false);

                        switch (e.Action)
                        {
                            case NotifyCollectionChangedAction.Add:
                            {
                                int intNewIndex = e.NewStartingIndex;
                                foreach (Gear objGear in e.NewItems)
                                {
                                    await AddToTree(objGear, intNewIndex).ConfigureAwait(false);
                                    await objGear.SetupChildrenGearsCollectionChangedAsync(
                                            true, treGear, cmsGear, cmsCustomGear, MakeDirtyWithCharacterUpdate, token)
                                        .ConfigureAwait(false);
                                    ++intNewIndex;
                                }
                            }
                                break;

                            case NotifyCollectionChangedAction.Remove:
                            {
                                foreach (Gear objGear in e.OldItems)
                                {
                                    await objGear.SetupChildrenGearsCollectionChangedAsync(false, treGear, token: token)
                                        .ConfigureAwait(false);
                                    await treGear.DoThreadSafeAsync(x => x.FindNodeByTag(objGear)?.Remove(), token)
                                        .ConfigureAwait(false);
                                }
                            }
                                break;

                            case NotifyCollectionChangedAction.Replace:
                            {
                                foreach (Gear objGear in e.OldItems)
                                {
                                    await objGear.SetupChildrenGearsCollectionChangedAsync(false, treGear, token: token)
                                        .ConfigureAwait(false);
                                    await treGear.DoThreadSafeAsync(x => x.FindNodeByTag(objGear)?.Remove(), token)
                                        .ConfigureAwait(false);
                                }

                                int intNewIndex = e.NewStartingIndex;
                                foreach (Gear objGear in e.NewItems)
                                {
                                    await AddToTree(objGear, intNewIndex).ConfigureAwait(false);
                                    await objGear.SetupChildrenGearsCollectionChangedAsync(
                                        true, treGear, cmsGear, cmsCustomGear, MakeDirtyWithCharacterUpdate,
                                        token: token).ConfigureAwait(false);
                                    ++intNewIndex;
                                }

                                await treGear.DoThreadSafeAsync(x => x.SelectedNode = x.FindNode(strSelectedId),
                                    token).ConfigureAwait(false);
                            }
                                break;

                            case NotifyCollectionChangedAction.Move:
                            {
                                await treGear.DoThreadSafeAsync(x =>
                                {
                                    foreach (Gear objGear in e.OldItems)
                                    {
                                        x.FindNodeByTag(objGear)?.Remove();
                                    }
                                }, token).ConfigureAwait(false);

                                int intNewIndex = e.NewStartingIndex;
                                foreach (Gear objGear in e.NewItems)
                                {
                                    await AddToTree(objGear, intNewIndex).ConfigureAwait(false);
                                    ++intNewIndex;
                                }

                                await treGear.DoThreadSafeAsync(x => x.SelectedNode = x.FindNode(strSelectedId),
                                    token).ConfigureAwait(false);
                            }
                                break;
                        }
                    }

                    async ValueTask AddToTree(Gear objGear, int intIndex = -1, bool blnSingleAdd = true)
                    {
                        if (blnCommlinksOnly && !await objGear.GetIsCommlinkAsync(token).ConfigureAwait(false))
                            return;

                        if (blnHideLoadedAmmo && objGear.LoadedIntoClip != null)
                            return;

                        TreeNode objNode = await objGear.CreateTreeNode(cmsGear, cmsCustomGear, token)
                            .ConfigureAwait(false);
                        if (objNode == null)
                            return;
                        TreeNode nodParent = null;
                        if (objGear.Location != null)
                        {
                            nodParent = await treGear.DoThreadSafeFuncAsync(
                                x => x.FindNodeByTag(objGear.Location, false),
                                token).ConfigureAwait(false);
                        }

                        if (nodParent == null)
                        {
                            if (nodRoot == null)
                            {
                                nodRoot = new TreeNode
                                {
                                    Tag = "Node_SelectedGear",
                                    Text = await LanguageManager.GetStringAsync("Node_SelectedGear", token: token)
                                        .ConfigureAwait(false)
                                };
                                // ReSharper disable once AssignNullToNotNullAttribute
                                await treGear.DoThreadSafeAsync(x => x.Nodes.Insert(0, nodRoot), token)
                                    .ConfigureAwait(false);
                            }

                            nodParent = nodRoot;
                        }

                        await treGear.DoThreadSafeAsync(x =>
                        {
                            if (nodParent == null)
                                return;
                            if (intIndex >= 0)
                                nodParent.Nodes.Insert(intIndex, objNode);
                            else
                                nodParent.Nodes.Add(objNode);
                            nodParent.Expand();
                            if (blnSingleAdd)
                                x.SelectedNode = objNode;
                        }, token).ConfigureAwait(false);
                    }
                }
                finally
                {
                    SkipUpdate = false;
                }
            }
            finally
            {
                await objCursorWait.DisposeAsync().ConfigureAwait(false);
            }
        }

        protected async Task RefreshDrugs(TreeView treGear, NotifyCollectionChangedEventArgs e = null, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (treGear == null)
                return;
            CursorWait objCursorWait = await CursorWait.NewAsync(this, token: token).ConfigureAwait(false);
            try
            {
                SkipUpdate = true;
                try
                {
                    string strSelectedId
                        = (await treGear.DoThreadSafeFuncAsync(x => x.SelectedNode?.Tag, token)
                            .ConfigureAwait(false) as IHasInternalId)
                        ?.InternalId ?? string.Empty;

                    TreeNode nodRoot = null;

                    if (e == null ||
                        e.Action == NotifyCollectionChangedAction.Reset)
                    {
                        await treGear.DoThreadSafeAsync(x => x.SuspendLayout(), token).ConfigureAwait(false);
                        try
                        {
                            await treGear.DoThreadSafeAsync(x => x.Nodes.Clear(), token).ConfigureAwait(false);

                            // Add Drugs.
                            await CharacterObject.Drugs
                                .ForEachAsync(objDrug => AddToTree(objDrug, -1, false), token)
                                .ConfigureAwait(false);

                            await treGear.DoThreadSafeAsync(x => x.SelectedNode = x.FindNode(strSelectedId), token)
                                .ConfigureAwait(false);
                        }
                        finally
                        {
                            await treGear.DoThreadSafeAsync(x => x.ResumeLayout(), GenericToken).ConfigureAwait(false);
                        }
                    }
                    else
                    {
                        nodRoot = await treGear.DoThreadSafeFuncAsync(x => x.FindNode("Node_SelectedDrugs", false),
                            token).ConfigureAwait(false);

                        switch (e.Action)
                        {
                            case NotifyCollectionChangedAction.Add:
                            {
                                int intNewIndex = e.NewStartingIndex;
                                foreach (Drug d in e.NewItems)
                                {
                                    await AddToTree(d, intNewIndex).ConfigureAwait(false);
                                    ++intNewIndex;
                                }
                            }
                                break;

                            case NotifyCollectionChangedAction.Remove:
                            {
                                await treGear.DoThreadSafeAsync(x =>
                                {
                                    foreach (Drug d in e.OldItems)
                                    {
                                        x.FindNodeByTag(d)?.Remove();
                                    }
                                }, token).ConfigureAwait(false);
                            }
                                break;

                            case NotifyCollectionChangedAction.Replace:
                            case NotifyCollectionChangedAction.Move:
                            {
                                await treGear.DoThreadSafeAsync(x =>
                                {
                                    foreach (Drug d in e.OldItems)
                                    {
                                        x.FindNodeByTag(d)?.Remove();
                                    }
                                }, token).ConfigureAwait(false);

                                int intNewIndex = e.NewStartingIndex;
                                foreach (Drug d in e.NewItems)
                                {
                                    await AddToTree(d, intNewIndex).ConfigureAwait(false);
                                    ++intNewIndex;
                                }

                                await treGear.DoThreadSafeAsync(x => x.SelectedNode = x.FindNode(strSelectedId),
                                    token).ConfigureAwait(false);
                            }
                                break;
                        }
                    }

                    async Task AddToTree(Drug objGear, int intIndex = -1, bool blnSingleAdd = true)
                    {
                        TreeNode objNode = objGear.CreateTreeNode();
                        if (objNode == null)
                            return;
                        if (nodRoot == null)
                        {
                            nodRoot = new TreeNode
                            {
                                Tag = "Node_SelectedDrugs",
                                Text = await LanguageManager.GetStringAsync("Node_SelectedDrugs", token: token)
                                    .ConfigureAwait(false)
                            };
                            // ReSharper disable once AssignNullToNotNullAttribute
                            await treGear.DoThreadSafeAsync(x => x.Nodes.Insert(0, nodRoot), token)
                                .ConfigureAwait(false);
                        }

                        await treGear.DoThreadSafeAsync(x =>
                        {
                            if (nodRoot == null)
                                return;
                            if (intIndex >= 0)
                                nodRoot.Nodes.Insert(intIndex, objNode);
                            else
                                nodRoot.Nodes.Add(objNode);
                            nodRoot.Expand();
                            if (blnSingleAdd)
                                x.SelectedNode = objNode;
                        }, token).ConfigureAwait(false);
                    }
                }
                finally
                {
                    SkipUpdate = false;
                }
            }
            finally
            {
                await objCursorWait.DisposeAsync().ConfigureAwait(false);
            }
        }

        protected async Task RefreshCyberwareClearBindings(TreeView treCyberware, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (treCyberware == null)
                return;
            CursorWait objCursorWait = await CursorWait.NewAsync(this, token: token).ConfigureAwait(false);
            try
            {
                SkipUpdate = true;
                try
                {
                    await CharacterObject.Cyberware.ForEachWithSideEffectsAsync(
                        objCyberware =>
                            objCyberware.SetupChildrenCyberwareCollectionChangedAsync(false, treCyberware,
                                token: token),
                        token).ConfigureAwait(false);
                }
                finally
                {
                    SkipUpdate = false;
                }
            }
            finally
            {
                await objCursorWait.DisposeAsync().ConfigureAwait(false);
            }
        }

        protected async Task RefreshCyberware(TreeView treCyberware, ContextMenuStrip cmsCyberware, ContextMenuStrip cmsCyberwareGear, NotifyCollectionChangedEventArgs e = null, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (treCyberware == null)
                return;

            TreeNode objCyberwareRoot = null;
            TreeNode objBiowareRoot = null;
            TreeNode objModularRoot = null;
            TreeNode objModularBioRoot = null;
            TreeNode objHoleNode = null;
            TreeNode objAntiHoleNode = null;

            CursorWait objCursorWait = await CursorWait.NewAsync(this, token: token).ConfigureAwait(false);
            try
            {
                SkipUpdate = true;
                try
                {
                    string strSelectedId
                        = (await treCyberware.DoThreadSafeFuncAsync(x => x.SelectedNode?.Tag, token)
                                .ConfigureAwait(false) as
                            IHasInternalId)?.InternalId ?? string.Empty;

                    if (e == null ||
                        e.Action == NotifyCollectionChangedAction.Reset)
                    {
                        await treCyberware.DoThreadSafeAsync(x => x.SuspendLayout(), token).ConfigureAwait(false);
                        try
                        {
                            await treCyberware.DoThreadSafeAsync(x => x.Nodes.Clear(), token).ConfigureAwait(false);

                            await CharacterObject.Cyberware.ForEachWithSideEffectsAsync(async objCyberware =>
                            {
                                await AddToTree(objCyberware, false).ConfigureAwait(false);
                                await objCyberware.SetupChildrenCyberwareCollectionChangedAsync(true, treCyberware,
                                    cmsCyberware,
                                    cmsCyberwareGear, MakeDirtyWithCharacterUpdate, token).ConfigureAwait(false);
                            }, token).ConfigureAwait(false);

                            await treCyberware.DoThreadSafeAsync(x => x.SortCustomAlphabetically(strSelectedId),
                                token).ConfigureAwait(false);
                        }
                        finally
                        {
                            await treCyberware.DoThreadSafeAsync(x => x.ResumeLayout(), GenericToken)
                                .ConfigureAwait(false);
                        }
                    }
                    else
                    {
                        await treCyberware.DoThreadSafeAsync(x =>
                        {
                            objCyberwareRoot = x.FindNode("Node_SelectedCyberware", false);
                            objBiowareRoot = x.FindNode("Node_SelectedBioware", false);
                            objModularRoot = x.FindNode("Node_UnequippedModularCyberware", false);
                            objModularBioRoot = x.FindNode("Node_UnequippedModularBioware", false);
                            objHoleNode = x.FindNode(
                                Cyberware.EssenceHoleGUID.ToString("D", GlobalSettings.InvariantCultureInfo), false);
                            objAntiHoleNode
                                = x.FindNode(
                                    Cyberware.EssenceAntiHoleGUID.ToString("D", GlobalSettings.InvariantCultureInfo),
                                    false);
                        }, token).ConfigureAwait(false);
                        switch (e.Action)
                        {
                            case NotifyCollectionChangedAction.Add:
                            {
                                foreach (Cyberware objCyberware in e.NewItems)
                                {
                                    await AddToTree(objCyberware).ConfigureAwait(false);
                                    await objCyberware.SetupChildrenCyberwareCollectionChangedAsync(true, treCyberware,
                                        cmsCyberware,
                                        cmsCyberwareGear, MakeDirtyWithCharacterUpdate, token).ConfigureAwait(false);
                                }
                            }
                                break;

                            case NotifyCollectionChangedAction.Remove:
                            {
                                foreach (Cyberware objCyberware in e.OldItems)
                                {
                                    await objCyberware
                                        .SetupChildrenCyberwareCollectionChangedAsync(false, treCyberware, token: token)
                                        .ConfigureAwait(false);
                                    await treCyberware.DoThreadSafeAsync(x =>
                                    {
                                        TreeNode objNode = x.FindNodeByTag(objCyberware);
                                        if (objNode != null)
                                        {
                                            TreeNode objParent = objNode.Parent;
                                            objNode.Remove();
                                            if (objParent != null && objParent.Level == 0 && objParent.Nodes.Count == 0)
                                                objParent.Remove();
                                        }
                                    }, token).ConfigureAwait(false);
                                }
                            }
                                break;

                            case NotifyCollectionChangedAction.Replace:
                            {
                                List<TreeNode> lstOldParentNodes =
                                    new List<TreeNode>(e.OldItems.Count);

                                foreach (Cyberware objCyberware in e.OldItems)
                                {
                                    await objCyberware
                                        .SetupChildrenCyberwareCollectionChangedAsync(false, treCyberware, token: token)
                                        .ConfigureAwait(false);
                                    await treCyberware.DoThreadSafeAsync(x =>
                                    {
                                        TreeNode objNode = x.FindNodeByTag(objCyberware);
                                        if (objNode != null)
                                        {
                                            TreeNode objParent = objNode.Parent;
                                            objNode.Remove();
                                            if (objParent != null && objParent.Level == 0)
                                                lstOldParentNodes.Add(objParent);
                                        }
                                    }, token).ConfigureAwait(false);
                                }

                                foreach (Cyberware objCyberware in e.NewItems)
                                {
                                    await AddToTree(objCyberware).ConfigureAwait(false);
                                    await objCyberware.SetupChildrenCyberwareCollectionChangedAsync(true, treCyberware,
                                        cmsCyberware,
                                        cmsCyberwareGear, MakeDirtyWithCharacterUpdate, token).ConfigureAwait(false);
                                }

                                await treCyberware.DoThreadSafeAsync(x =>
                                {
                                    foreach (TreeNode objOldParent in lstOldParentNodes)
                                    {
                                        if (objOldParent.Nodes.Count == 0)
                                            objOldParent.Remove();
                                    }

                                    x.SelectedNode = x.FindNode(strSelectedId);
                                }, token).ConfigureAwait(false);
                            }
                                break;
                        }
                    }

                    async Task AddToTree(Cyberware objCyberware, bool blnSingleAdd = true)
                    {
                        if (await objCyberware.GetSourceIDAsync(token).ConfigureAwait(false) ==
                            Cyberware.EssenceHoleGUID)
                        {
                            bool blnAddHoleNode = false;
                            if (objHoleNode == null)
                            {
                                blnAddHoleNode = true;
                                objHoleNode = await objCyberware.CreateTreeNode(null, null, token)
                                    .ConfigureAwait(false);
                            }

                            await treCyberware.DoThreadSafeAsync(x =>
                            {
                                if (blnAddHoleNode)
                                {
                                    // ReSharper disable once AssignNullToNotNullAttribute
                                    x.Nodes.Add(objHoleNode);
                                }

                                if (blnSingleAdd)
                                    x.SelectedNode = objHoleNode;
                            }, token).ConfigureAwait(false);
                            return;
                        }

                        if (await objCyberware.GetSourceIDAsync(token).ConfigureAwait(false) ==
                            Cyberware.EssenceAntiHoleGUID)
                        {
                            bool blnAddAntiHoleNode = false;
                            if (objAntiHoleNode == null)
                            {
                                blnAddAntiHoleNode = true;
                                objAntiHoleNode = await objCyberware.CreateTreeNode(null, null, token)
                                    .ConfigureAwait(false);
                            }

                            await treCyberware.DoThreadSafeAsync(x =>
                            {
                                if (blnAddAntiHoleNode)
                                {
                                    // ReSharper disable once AssignNullToNotNullAttribute
                                    x.Nodes.Add(objAntiHoleNode);
                                }

                                if (blnSingleAdd)
                                    x.SelectedNode = objAntiHoleNode;
                            }, token).ConfigureAwait(false);
                            return;
                        }

                        TreeNode objNode = await objCyberware.CreateTreeNode(cmsCyberware, cmsCyberwareGear, token)
                            .ConfigureAwait(false);
                        if (objNode == null)
                            return;

                        TreeNode nodParent = null;
                        switch (objCyberware.SourceType)
                        {
                            case Improvement.ImprovementSource.Cyberware:
                            {
                                if (await objCyberware.GetIsModularCurrentlyEquippedAsync(token).ConfigureAwait(false))
                                {
                                    if (objCyberwareRoot == null)
                                    {
                                        objCyberwareRoot = new TreeNode
                                        {
                                            Tag = "Node_SelectedCyberware",
                                            Text = await LanguageManager
                                                .GetStringAsync("Node_SelectedCyberware", token: token)
                                                .ConfigureAwait(false)
                                        };
                                        await treCyberware.DoThreadSafeAsync(x =>
                                        {
                                            // ReSharper disable once AssignNullToNotNullAttribute
                                            x.Nodes.Insert(0, objCyberwareRoot);
                                            objCyberwareRoot.Expand();
                                        }, token).ConfigureAwait(false);
                                    }

                                    nodParent = objCyberwareRoot;
                                }
                                else
                                {
                                    if (objModularRoot == null)
                                    {
                                        objModularRoot = new TreeNode
                                        {
                                            Tag = "Node_UnequippedModularCyberware",
                                            Text = await LanguageManager
                                                .GetStringAsync("Node_UnequippedModularCyberware", token: token)
                                                .ConfigureAwait(false)
                                        };
                                        await treCyberware.DoThreadSafeAsync(x =>
                                        {
                                            int intIndex = (objCyberwareRoot != null).ToInt32() +
                                                           (objBiowareRoot != null).ToInt32();
                                            // ReSharper disable once AssignNullToNotNullAttribute
                                            x.Nodes.Insert(intIndex, objModularRoot);
                                            objModularRoot.Expand();
                                        }, token).ConfigureAwait(false);
                                    }

                                    nodParent = objModularRoot;
                                }

                                break;
                            }
                            case Improvement.ImprovementSource.Bioware:
                            {
                                if (await objCyberware.GetIsModularCurrentlyEquippedAsync(token).ConfigureAwait(false))
                                {
                                    if (objBiowareRoot == null)
                                    {
                                        objBiowareRoot = new TreeNode
                                        {
                                            Tag = "Node_SelectedBioware",
                                            Text = await LanguageManager
                                                .GetStringAsync("Node_SelectedBioware", token: token)
                                                .ConfigureAwait(false)
                                        };
                                        await treCyberware.DoThreadSafeAsync(x =>
                                        {
                                            // ReSharper disable once AssignNullToNotNullAttribute
                                            x.Nodes.Insert((objCyberwareRoot != null).ToInt32(), objBiowareRoot);
                                            objBiowareRoot.Expand();
                                        }, token).ConfigureAwait(false);
                                    }

                                    nodParent = objBiowareRoot;
                                }
                                else
                                {
                                    if (objModularBioRoot == null)
                                    {
                                        objModularBioRoot = new TreeNode
                                        {
                                            Tag = "Node_UnequippedModularBioware",
                                            Text = await LanguageManager
                                                .GetStringAsync("Node_UnequippedModularBioware", token: token)
                                                .ConfigureAwait(false)
                                        };
                                        await treCyberware.DoThreadSafeAsync(x =>
                                        {
                                            int intIndex = (objCyberwareRoot != null).ToInt32() +
                                                           (objBiowareRoot != null).ToInt32() +
                                                           (objModularRoot != null).ToInt32();
                                            // ReSharper disable once AssignNullToNotNullAttribute
                                            x.Nodes.Insert(intIndex, objModularBioRoot);
                                            objModularBioRoot.Expand();
                                        }, token).ConfigureAwait(false);
                                    }

                                    nodParent = objModularBioRoot;
                                }

                                break;
                            }
                        }

                        if (nodParent != null)
                        {
                            await treCyberware.DoThreadSafeAsync(x =>
                            {
                                if (blnSingleAdd)
                                {
                                    TreeNodeCollection lstParentNodeChildren = nodParent.Nodes;
                                    int intNodesCount = lstParentNodeChildren.Count;
                                    int intTargetIndex = 0;
                                    for (; intTargetIndex < intNodesCount; ++intTargetIndex)
                                    {
                                        if (CompareTreeNodes.CompareText(lstParentNodeChildren[intTargetIndex], objNode)
                                            >= 0)
                                        {
                                            break;
                                        }
                                    }

                                    lstParentNodeChildren.Insert(intTargetIndex, objNode);
                                    x.SelectedNode = objNode;
                                }
                                else
                                    nodParent.Nodes.Add(objNode);
                            }, token).ConfigureAwait(false);
                        }
                    }
                }
                finally
                {
                    SkipUpdate = false;
                }
            }
            finally
            {
                await objCursorWait.DisposeAsync().ConfigureAwait(false);
            }
        }

        protected async Task RefreshVehiclesClearBindings(TreeView treVehicles, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (treVehicles == null)
                return;
            CursorWait objCursorWait = await CursorWait.NewAsync(this, token: token).ConfigureAwait(false);
            try
            {
                SkipUpdate = true;
                try
                {
                    await CharacterObject.Vehicles.ForEachWithSideEffectsAsync(
                        async objVehicle =>
                        {
                            await objVehicle.Mods.RemoveTaggedAsyncBeforeClearCollectionChangedAsync(treVehicles, token)
                                .ConfigureAwait(false);
                            await objVehicle.Mods.RemoveTaggedAsyncCollectionChangedAsync(treVehicles, token)
                                .ConfigureAwait(false);
                            await objVehicle.WeaponMounts
                                .RemoveTaggedAsyncBeforeClearCollectionChangedAsync(treVehicles, token)
                                .ConfigureAwait(false);
                            await objVehicle.WeaponMounts.RemoveTaggedAsyncCollectionChangedAsync(treVehicles, token)
                                .ConfigureAwait(false);
                            await objVehicle.Weapons
                                .RemoveTaggedAsyncBeforeClearCollectionChangedAsync(treVehicles, token)
                                .ConfigureAwait(false);
                            await objVehicle.Weapons.RemoveTaggedAsyncCollectionChangedAsync(treVehicles, token)
                                .ConfigureAwait(false);
                            await objVehicle.Mods.ForEachWithSideEffectsAsync(async objMod =>
                            {
                                await objMod.Cyberware
                                    .RemoveTaggedAsyncBeforeClearCollectionChangedAsync(treVehicles, token)
                                    .ConfigureAwait(false);
                                await objMod.Cyberware.RemoveTaggedAsyncCollectionChangedAsync(treVehicles, token)
                                    .ConfigureAwait(false);
                                await objMod.Cyberware.ForEachWithSideEffectsAsync(objCyberware =>
                                        objCyberware
                                            .SetupChildrenCyberwareCollectionChangedAsync(
                                                false, treVehicles, token: token), token)
                                    .ConfigureAwait(false);
                                await objMod.Weapons
                                    .RemoveTaggedAsyncBeforeClearCollectionChangedAsync(treVehicles, token)
                                    .ConfigureAwait(false);
                                await objMod.Weapons.RemoveTaggedAsyncCollectionChangedAsync(treVehicles, token)
                                    .ConfigureAwait(false);
                                await objMod.Weapons.ForEachWithSideEffectsAsync(objWeapon =>
                                        objWeapon
                                            .SetupChildrenWeaponsCollectionChangedAsync(
                                                false, treVehicles, token: token), token)
                                    .ConfigureAwait(false);
                            }, token).ConfigureAwait(false);

                            await objVehicle.WeaponMounts.ForEachWithSideEffectsAsync(async objMount =>
                            {
                                await objMount.Weapons
                                    .RemoveTaggedAsyncBeforeClearCollectionChangedAsync(treVehicles, token)
                                    .ConfigureAwait(false);
                                await objMount.Weapons.RemoveTaggedAsyncCollectionChangedAsync(treVehicles, token)
                                    .ConfigureAwait(false);
                                await objMount.Weapons.ForEachWithSideEffectsAsync(objWeapon =>
                                        objWeapon
                                            .SetupChildrenWeaponsCollectionChangedAsync(
                                                false, treVehicles, token: token), token)
                                    .ConfigureAwait(false);
                                await objMount.Mods
                                    .RemoveTaggedAsyncBeforeClearCollectionChangedAsync(treVehicles, token)
                                    .ConfigureAwait(false);
                                await objMount.Mods.RemoveTaggedAsyncCollectionChangedAsync(treVehicles, token)
                                    .ConfigureAwait(false);
                                await objMount.Mods.ForEachWithSideEffectsAsync(async objMod =>
                                {
                                    await objMod.Cyberware
                                        .RemoveTaggedAsyncBeforeClearCollectionChangedAsync(treVehicles, token)
                                        .ConfigureAwait(false);
                                    await objMod.Cyberware.RemoveTaggedAsyncCollectionChangedAsync(treVehicles, token)
                                        .ConfigureAwait(false);
                                    await objMod.Cyberware.ForEachWithSideEffectsAsync(objCyberware =>
                                            objCyberware
                                                .SetupChildrenCyberwareCollectionChangedAsync(
                                                    false, treVehicles, token: token), token)
                                        .ConfigureAwait(false);
                                    await objMod.Weapons
                                        .RemoveTaggedAsyncBeforeClearCollectionChangedAsync(treVehicles, token)
                                        .ConfigureAwait(false);
                                    await objMod.Weapons.RemoveTaggedAsyncCollectionChangedAsync(treVehicles, token)
                                        .ConfigureAwait(false);
                                    await objMod.Weapons.ForEachWithSideEffectsAsync(objWeapon =>
                                            objWeapon
                                                .SetupChildrenWeaponsCollectionChangedAsync(
                                                    false, treVehicles, token: token), token)
                                        .ConfigureAwait(false);
                                }, token).ConfigureAwait(false);
                            }, token).ConfigureAwait(false);

                            await objVehicle.Weapons.ForEachWithSideEffectsAsync(objWeapon =>
                                    objWeapon
                                        .SetupChildrenWeaponsCollectionChangedAsync(
                                            false, treVehicles, token: token), token)
                                .ConfigureAwait(false);
                            await objVehicle.GearChildren
                                .RemoveTaggedAsyncBeforeClearCollectionChangedAsync(treVehicles, token)
                                .ConfigureAwait(false);
                            await objVehicle.GearChildren.RemoveTaggedAsyncCollectionChangedAsync(treVehicles, token)
                                .ConfigureAwait(false);
                            await objVehicle.GearChildren.ForEachWithSideEffectsAsync(objGear =>
                                    objGear
                                        .SetupChildrenGearsCollectionChangedAsync(
                                            false, treVehicles, token: token), token)
                                .ConfigureAwait(false);
                            await objVehicle.Locations.RemoveTaggedAsyncCollectionChangedAsync(treVehicles, token)
                                .ConfigureAwait(false);
                        },
                        token).ConfigureAwait(false);
                }
                finally
                {
                    SkipUpdate = false;
                }
            }
            finally
            {
                await objCursorWait.DisposeAsync().ConfigureAwait(false);
            }
        }

        protected async Task RefreshVehicles(TreeView treVehicles, ContextMenuStrip cmsVehicleLocation, ContextMenuStrip cmsVehicle, ContextMenuStrip cmsVehicleWeapon, ContextMenuStrip cmsVehicleWeaponAccessory, ContextMenuStrip cmsVehicleWeaponAccessoryGear, ContextMenuStrip cmsVehicleGear, ContextMenuStrip cmsVehicleWeaponMount, ContextMenuStrip cmsCyberware, ContextMenuStrip cmsCyberwareGear, NotifyCollectionChangedEventArgs e = null, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (treVehicles == null)
                return;
            CursorWait objCursorWait = await CursorWait.NewAsync(this, token: token).ConfigureAwait(false);
            try
            {
                SkipUpdate = true;
                try
                {
                    string strSelectedId
                        = (await treVehicles.DoThreadSafeFuncAsync(x => x.SelectedNode?.Tag,
                              token).ConfigureAwait(false) as IHasInternalId)?.InternalId
                          ?? string.Empty;

                    TreeNode nodRoot = null;

                    if (e == null ||
                        e.Action == NotifyCollectionChangedAction.Reset)
                    {
                        await treVehicles.DoThreadSafeAsync(x => x.SuspendLayout(), token).ConfigureAwait(false);
                        try
                        {
                            await treVehicles.DoThreadSafeAsync(x => x.Nodes.Clear(), token).ConfigureAwait(false);

                            // Start by populating Locations.
                            await CharacterObject.VehicleLocations
                                .ForEachAsync(
                                    async objLocation =>
                                    {
                                        TreeNode objNode = await objLocation.CreateTreeNode(cmsVehicleLocation, token)
                                            .ConfigureAwait(false);
                                        await treVehicles.DoThreadSafeAsync(
                                            x => x.Nodes.Add(objNode),
                                            token).ConfigureAwait(false);
                                    }, token).ConfigureAwait(false);

                            // Add Vehicles.
                            await CharacterObject.Vehicles.ForEachWithSideEffectsAsync(async objVehicle =>
                            {
                                await AddToTree(objVehicle, -1, false).ConfigureAwait(false);

                                Task FuncVehicleModsBeforeClearToAdd(object x, NotifyCollectionChangedEventArgs y,
                                    CancellationToken innerToken = default) =>
                                    objVehicle.RefreshVehicleModsClearBindings(treVehicles, y, innerToken);

                                Task FuncVehicleModsToAdd(object x, NotifyCollectionChangedEventArgs y,
                                    CancellationToken innerToken = default) =>
                                    objVehicle.RefreshVehicleMods(
                                        treVehicles, cmsVehicle, cmsCyberware, cmsCyberwareGear, cmsVehicleWeapon,
                                        cmsVehicleWeaponAccessory, cmsVehicleWeaponAccessoryGear, null, y,
                                        MakeDirtyWithCharacterUpdate, token: innerToken);

                                Task FuncVehicleWeaponMountsBeforeClearToAdd(object x,
                                    NotifyCollectionChangedEventArgs y,
                                    CancellationToken innerToken = default) =>
                                    objVehicle.RefreshVehicleWeaponMountsClearBindings(treVehicles, y, innerToken);

                                Task FuncVehicleWeaponMountsToAdd(object x, NotifyCollectionChangedEventArgs y,
                                    CancellationToken innerToken = default) =>
                                    objVehicle.RefreshVehicleWeaponMounts(
                                        treVehicles, cmsVehicleWeaponMount, cmsVehicleWeapon,
                                        cmsVehicleWeaponAccessory, cmsVehicleWeaponAccessoryGear, cmsCyberware,
                                        cmsCyberwareGear, cmsVehicle, () => objVehicle.Mods.Count, y,
                                        MakeDirtyWithCharacterUpdate, token: innerToken);

                                Task FuncVehicleWeaponsBeforeClearToAdd(object x, NotifyCollectionChangedEventArgs y,
                                    CancellationToken innerToken = default) =>
                                    objVehicle.RefreshChildrenWeaponsClearBindings(treVehicles, y, innerToken);

                                Task FuncVehicleWeaponsToAdd(object x, NotifyCollectionChangedEventArgs y,
                                    CancellationToken innerToken = default) =>
                                    objVehicle.RefreshChildrenWeapons(
                                        treVehicles, cmsVehicleWeapon, cmsVehicleWeaponAccessory,
                                        cmsVehicleWeaponAccessoryGear,
                                        () => objVehicle.Mods.Count + (objVehicle.WeaponMounts.Count > 0).ToInt32(),
                                        y, MakeDirtyWithCharacterUpdate, token: innerToken);

                                objVehicle.Mods.AddTaggedCollectionChanged(
                                    treVehicles, MakeDirtyWithCharacterUpdate);
                                objVehicle.Mods.AddTaggedCollectionChanged(
                                    treVehicles, FuncVehicleModsToAdd);
                                objVehicle.Mods.AddTaggedBeforeClearCollectionChanged(
                                    treVehicles, FuncVehicleModsBeforeClearToAdd);
                                objVehicle.WeaponMounts.AddTaggedCollectionChanged(
                                    treVehicles, MakeDirtyWithCharacterUpdate);
                                objVehicle.WeaponMounts.AddTaggedCollectionChanged(
                                    treVehicles, FuncVehicleWeaponMountsToAdd);
                                objVehicle.WeaponMounts.AddTaggedBeforeClearCollectionChanged(
                                    treVehicles, FuncVehicleWeaponMountsBeforeClearToAdd);
                                objVehicle.Weapons.AddTaggedCollectionChanged(
                                    treVehicles, MakeDirtyWithCharacterUpdate);
                                objVehicle.Weapons.AddTaggedCollectionChanged(
                                    treVehicles, FuncVehicleWeaponsToAdd);
                                objVehicle.Weapons.AddTaggedBeforeClearCollectionChanged(
                                    treVehicles, FuncVehicleWeaponsBeforeClearToAdd);
                                await objVehicle.Mods.ForEachWithSideEffectsAsync(async objMod =>
                                {
                                    Task FuncVehicleModCyberwareBeforeClearToAdd(object x,
                                        NotifyCollectionChangedEventArgs y,
                                        CancellationToken innerToken = default) =>
                                        objMod.RefreshChildrenCyberwareClearBindings(treVehicles, y, innerToken);

                                    Task FuncVehicleModCyberwareToAdd(
                                        object x, NotifyCollectionChangedEventArgs y,
                                        CancellationToken innerToken = default) =>
                                        objMod.RefreshChildrenCyberware(
                                            treVehicles, cmsCyberware, cmsCyberwareGear, null, y,
                                            MakeDirtyWithCharacterUpdate, token: innerToken);

                                    Task FuncVehicleModWeaponsBeforeClearToAdd(object x,
                                        NotifyCollectionChangedEventArgs y,
                                        CancellationToken innerToken = default) =>
                                        objMod.RefreshChildrenWeaponsClearBindings(treVehicles, y, innerToken);

                                    Task FuncVehicleModWeaponsToAdd(
                                        object x, NotifyCollectionChangedEventArgs y,
                                        CancellationToken innerToken = default) =>
                                        objMod.RefreshChildrenWeapons(
                                            treVehicles, cmsVehicleWeapon, cmsVehicleWeaponAccessory,
                                            cmsVehicleWeaponAccessoryGear, () => objMod.Cyberware.Count, y,
                                            MakeDirtyWithCharacterUpdate, token: innerToken);

                                    objMod.Cyberware.AddTaggedCollectionChanged(
                                        treVehicles, MakeDirtyWithCharacterUpdate);
                                    objMod.Cyberware.AddTaggedCollectionChanged(
                                        treVehicles, FuncVehicleModCyberwareToAdd);
                                    objMod.Cyberware.AddTaggedBeforeClearCollectionChanged(
                                        treVehicles, FuncVehicleModCyberwareBeforeClearToAdd);
                                    await objMod.Cyberware.ForEachWithSideEffectsAsync(objCyberware =>
                                                objCyberware
                                                    .SetupChildrenCyberwareCollectionChangedAsync(
                                                        true, treVehicles,
                                                        cmsCyberware, cmsCyberwareGear,
                                                        MakeDirtyWithCharacterUpdate, token),
                                            token)
                                        .ConfigureAwait(false);
                                    objMod.Weapons.AddTaggedCollectionChanged(
                                        treVehicles, MakeDirtyWithCharacterUpdate);
                                    objMod.Weapons.AddTaggedCollectionChanged(
                                        treVehicles, FuncVehicleModWeaponsToAdd);
                                    objMod.Weapons.AddTaggedBeforeClearCollectionChanged(
                                        treVehicles, FuncVehicleModWeaponsBeforeClearToAdd);
                                    await objMod.Weapons.ForEachWithSideEffectsAsync(objWeapon =>
                                            objWeapon
                                                .SetupChildrenWeaponsCollectionChangedAsync(
                                                    true, treVehicles,
                                                    cmsVehicleWeapon,
                                                    cmsVehicleWeaponAccessory,
                                                    cmsVehicleWeaponAccessoryGear,
                                                    MakeDirtyWithCharacterUpdate, token), token)
                                        .ConfigureAwait(false);
                                }, token).ConfigureAwait(false);

                                await objVehicle.WeaponMounts.ForEachWithSideEffectsAsync(async objMount =>
                                {
                                    Task FuncWeaponMountVehicleModBeforeClearToAdd(object x,
                                        NotifyCollectionChangedEventArgs y,
                                        CancellationToken innerToken = default) =>
                                        objMount.RefreshVehicleModsClearBindings(treVehicles, y, innerToken);

                                    Task FuncWeaponMountVehicleModToAdd(
                                        object x, NotifyCollectionChangedEventArgs y,
                                        CancellationToken innerToken = default) =>
                                        objMount.RefreshVehicleMods(treVehicles, cmsVehicle, cmsCyberware,
                                            cmsCyberwareGear, cmsVehicleWeapon,
                                            cmsVehicleWeaponAccessory,
                                            cmsVehicleWeaponAccessoryGear, null, y,
                                            MakeDirtyWithCharacterUpdate, token: innerToken);

                                    Task FuncWeaponMountWeaponsBeforeClearToAdd(object x,
                                        NotifyCollectionChangedEventArgs y,
                                        CancellationToken innerToken = default) =>
                                        objMount.RefreshChildrenWeaponsClearBindings(treVehicles, y, innerToken);

                                    Task FuncWeaponMountWeaponsToAdd(
                                        object x, NotifyCollectionChangedEventArgs y,
                                        CancellationToken innerToken = default) =>
                                        objMount.RefreshChildrenWeapons(
                                            treVehicles, cmsVehicleWeapon, cmsVehicleWeaponAccessory,
                                            cmsVehicleWeaponAccessoryGear, () => objMount.Mods.Count, y,
                                            MakeDirtyWithCharacterUpdate, token: innerToken);

                                    objMount.Mods.AddTaggedCollectionChanged(
                                        treVehicles, MakeDirtyWithCharacterUpdate);
                                    objMount.Mods.AddTaggedCollectionChanged(
                                        treVehicles, FuncWeaponMountVehicleModToAdd);
                                    objMount.Mods.AddTaggedBeforeClearCollectionChanged(
                                        treVehicles, FuncWeaponMountVehicleModBeforeClearToAdd);
                                    objMount.Weapons.AddTaggedCollectionChanged(
                                        treVehicles, MakeDirtyWithCharacterUpdate);
                                    objMount.Weapons.AddTaggedCollectionChanged(
                                        treVehicles, FuncWeaponMountWeaponsToAdd);
                                    objMount.Weapons.AddTaggedBeforeClearCollectionChanged(
                                        treVehicles, FuncWeaponMountWeaponsBeforeClearToAdd);
                                    await objMount.Weapons.ForEachWithSideEffectsAsync(objWeapon =>
                                                objWeapon
                                                    .SetupChildrenWeaponsCollectionChangedAsync(
                                                        true, treVehicles,
                                                        cmsVehicleWeapon,
                                                        cmsVehicleWeaponAccessory,
                                                        cmsVehicleWeaponAccessoryGear,
                                                        MakeDirtyWithCharacterUpdate, token),
                                            token)
                                        .ConfigureAwait(false);
                                    await objMount.Mods.ForEachWithSideEffectsAsync(async objMod =>
                                    {
                                        Task FuncWeaponMountVehicleModCyberwareBeforeClearToAdd(object x,
                                            NotifyCollectionChangedEventArgs y,
                                            CancellationToken innerToken = default) =>
                                            objMod.RefreshChildrenCyberwareClearBindings(treVehicles, y, innerToken);

                                        Task FuncWeaponMountVehicleModCyberwareToAdd(
                                            object x, NotifyCollectionChangedEventArgs y,
                                            CancellationToken innerToken = default) =>
                                            objMod.RefreshChildrenCyberware(
                                                treVehicles, cmsCyberware, cmsCyberwareGear, null, y,
                                                MakeDirtyWithCharacterUpdate, token: innerToken);

                                        Task FuncWeaponMountVehicleModWeaponsToAddBeforeClearToAdd(object x,
                                            NotifyCollectionChangedEventArgs y,
                                            CancellationToken innerToken = default) =>
                                            objMod.RefreshChildrenWeaponsClearBindings(treVehicles, y, innerToken);

                                        Task FuncWeaponMountVehicleModWeaponsToAdd(
                                            object x, NotifyCollectionChangedEventArgs y,
                                            CancellationToken innerToken = default) =>
                                            objMod.RefreshChildrenWeapons(
                                                treVehicles, cmsVehicleWeapon, cmsVehicleWeaponAccessory,
                                                cmsVehicleWeaponAccessoryGear, () => objMod.Cyberware.Count, y,
                                                MakeDirtyWithCharacterUpdate, token: innerToken);

                                        objMod.Cyberware.AddTaggedCollectionChanged(
                                            treVehicles, MakeDirtyWithCharacterUpdate);
                                        objMod.Cyberware.AddTaggedCollectionChanged(
                                            treVehicles, FuncWeaponMountVehicleModCyberwareToAdd);
                                        objMod.Cyberware.AddTaggedBeforeClearCollectionChanged(
                                            treVehicles, FuncWeaponMountVehicleModCyberwareBeforeClearToAdd);
                                        await objMod.Cyberware.ForEachWithSideEffectsAsync(objCyberware =>
                                                    objCyberware
                                                        .SetupChildrenCyberwareCollectionChangedAsync(
                                                            true, treVehicles,
                                                            cmsCyberware, cmsCyberwareGear,
                                                            MakeDirtyWithCharacterUpdate, token),
                                                token)
                                            .ConfigureAwait(false);
                                        objMod.Weapons.AddTaggedCollectionChanged(
                                            treVehicles, MakeDirtyWithCharacterUpdate);
                                        objMod.Weapons.AddTaggedCollectionChanged(
                                            treVehicles, FuncWeaponMountVehicleModWeaponsToAdd);
                                        objMod.Weapons.AddTaggedBeforeClearCollectionChanged(
                                            treVehicles, FuncWeaponMountVehicleModWeaponsToAddBeforeClearToAdd);
                                        await objMod.Weapons.ForEachWithSideEffectsAsync(objWeapon =>
                                                    objWeapon
                                                        .SetupChildrenWeaponsCollectionChangedAsync(
                                                            true, treVehicles,
                                                            cmsVehicleWeapon,
                                                            cmsVehicleWeaponAccessory,
                                                            cmsVehicleWeaponAccessoryGear,
                                                            MakeDirtyWithCharacterUpdate, token),
                                                token)
                                            .ConfigureAwait(false);
                                    }, token).ConfigureAwait(false);
                                }, token).ConfigureAwait(false);

                                await objVehicle.Weapons.ForEachWithSideEffectsAsync(objWeapon =>
                                        objWeapon
                                            .SetupChildrenWeaponsCollectionChangedAsync(
                                                true, treVehicles, cmsVehicleWeapon,
                                                cmsVehicleWeaponAccessory,
                                                cmsVehicleWeaponAccessoryGear,
                                                MakeDirtyWithCharacterUpdate, token), token)
                                    .ConfigureAwait(false);

                                Task FuncDelegateBeforeClearToAdd(object x, NotifyCollectionChangedEventArgs y,
                                    CancellationToken innerToken = default) =>
                                    objVehicle.RefreshChildrenGearsClearBindings(treVehicles, y, innerToken);

                                Task FuncVehicleGearToAdd(object x, NotifyCollectionChangedEventArgs y,
                                    CancellationToken innerToken = default) =>
                                    objVehicle.RefreshChildrenGears(
                                        treVehicles, cmsVehicleGear, null,
                                        () => objVehicle.Mods.Count + objVehicle.Weapons.Count
                                                                    + (objVehicle.WeaponMounts.Count > 0).ToInt32(),
                                        y, MakeDirtyWithCharacterUpdate, token: innerToken);

                                Task FuncVehicleLocationsToAdd(object x, NotifyCollectionChangedEventArgs y,
                                    CancellationToken innerToken = default) =>
                                    RefreshLocationsInVehicle(treVehicles, objVehicle, cmsVehicleLocation,
                                        () => objVehicle.Mods.Count
                                              + objVehicle.Weapons.Count
                                              + (objVehicle.WeaponMounts.Count > 0)
                                              .ToInt32()
                                              + objVehicle.GearChildren.Count(
                                                  z => z.Location == null, innerToken), y, innerToken);

                                objVehicle.GearChildren.AddTaggedCollectionChanged(
                                    treVehicles, MakeDirtyWithCharacterUpdate);
                                objVehicle.GearChildren.AddTaggedCollectionChanged(
                                    treVehicles, FuncVehicleGearToAdd);
                                objVehicle.GearChildren.AddTaggedBeforeClearCollectionChanged(
                                    treVehicles, FuncDelegateBeforeClearToAdd);
                                await objVehicle.GearChildren.ForEachWithSideEffectsAsync(objGear =>
                                            objGear
                                                .SetupChildrenGearsCollectionChangedAsync(
                                                    true, treVehicles,
                                                    cmsVehicleGear, null,
                                                    MakeDirtyWithCharacterUpdate, token),
                                        token)
                                    .ConfigureAwait(false);
                                objVehicle.Locations.AddTaggedCollectionChanged(
                                    treVehicles, MakeDirtyWithCharacterUpdate);
                                objVehicle.Locations.AddTaggedCollectionChanged(
                                    treVehicles, FuncVehicleLocationsToAdd);
                            }, token).ConfigureAwait(false);

                            await treVehicles.DoThreadSafeAsync(x => x.SelectedNode = x.FindNode(strSelectedId),
                                token).ConfigureAwait(false);
                        }
                        finally
                        {
                            await treVehicles.DoThreadSafeAsync(x => x.ResumeLayout(), GenericToken)
                                .ConfigureAwait(false);
                        }
                    }
                    else
                    {
                        nodRoot = await treVehicles.DoThreadSafeFuncAsync(
                            x => x.FindNode("Node_SelectedVehicles", false),
                            token).ConfigureAwait(false);

                        switch (e.Action)
                        {
                            case NotifyCollectionChangedAction.Add:
                            {
                                int intNewIndex = e.NewStartingIndex;
                                foreach (Vehicle objVehicle in e.NewItems)
                                {
                                    await AddToTree(objVehicle, intNewIndex).ConfigureAwait(false);

                                    Task FuncVehicleModsBeforeClearToAdd(object x, NotifyCollectionChangedEventArgs y,
                                        CancellationToken innerToken = default) =>
                                        objVehicle.RefreshVehicleModsClearBindings(treVehicles, y, innerToken);

                                    Task FuncVehicleModsToAdd(object x, NotifyCollectionChangedEventArgs y,
                                        CancellationToken innerToken = default) =>
                                        objVehicle.RefreshVehicleMods(
                                            treVehicles, cmsVehicle, cmsCyberware, cmsCyberwareGear, cmsVehicleWeapon,
                                            cmsVehicleWeaponAccessory, cmsVehicleWeaponAccessoryGear, null, y,
                                            MakeDirtyWithCharacterUpdate, token: innerToken);

                                    Task FuncVehicleWeaponMountsBeforeClearToAdd(object x,
                                        NotifyCollectionChangedEventArgs y,
                                        CancellationToken innerToken = default) =>
                                        objVehicle.RefreshVehicleWeaponMountsClearBindings(treVehicles, y, innerToken);

                                    Task FuncVehicleWeaponMountsToAdd(object x, NotifyCollectionChangedEventArgs y,
                                        CancellationToken innerToken = default) =>
                                        objVehicle.RefreshVehicleWeaponMounts(
                                            treVehicles, cmsVehicleWeaponMount, cmsVehicleWeapon,
                                            cmsVehicleWeaponAccessory, cmsVehicleWeaponAccessoryGear, cmsCyberware,
                                            cmsCyberwareGear, cmsVehicle, () => objVehicle.Mods.Count, y,
                                            MakeDirtyWithCharacterUpdate, token: innerToken);

                                    Task FuncVehicleWeaponsBeforeClearToAdd(object x,
                                        NotifyCollectionChangedEventArgs y,
                                        CancellationToken innerToken = default) =>
                                        objVehicle.RefreshChildrenWeaponsClearBindings(treVehicles, y, innerToken);

                                    Task FuncVehicleWeaponsToAdd(object x, NotifyCollectionChangedEventArgs y,
                                        CancellationToken innerToken = default) =>
                                        objVehicle.RefreshChildrenWeapons(
                                            treVehicles, cmsVehicleWeapon, cmsVehicleWeaponAccessory,
                                            cmsVehicleWeaponAccessoryGear,
                                            () => objVehicle.Mods.Count + (objVehicle.WeaponMounts.Count > 0).ToInt32(),
                                            y, MakeDirtyWithCharacterUpdate, token: innerToken);

                                    objVehicle.Mods.AddTaggedCollectionChanged(
                                        treVehicles, MakeDirtyWithCharacterUpdate);
                                    objVehicle.Mods.AddTaggedCollectionChanged(
                                        treVehicles, FuncVehicleModsToAdd);
                                    objVehicle.Mods.AddTaggedBeforeClearCollectionChanged(
                                        treVehicles, FuncVehicleModsBeforeClearToAdd);
                                    objVehicle.WeaponMounts.AddTaggedCollectionChanged(
                                        treVehicles, MakeDirtyWithCharacterUpdate);
                                    objVehicle.WeaponMounts.AddTaggedCollectionChanged(
                                        treVehicles, FuncVehicleWeaponMountsToAdd);
                                    objVehicle.WeaponMounts.AddTaggedBeforeClearCollectionChanged(
                                        treVehicles, FuncVehicleWeaponMountsBeforeClearToAdd);
                                    objVehicle.Weapons.AddTaggedCollectionChanged(
                                        treVehicles, MakeDirtyWithCharacterUpdate);
                                    objVehicle.Weapons.AddTaggedCollectionChanged(
                                        treVehicles, FuncVehicleWeaponsToAdd);
                                    objVehicle.Weapons.AddTaggedBeforeClearCollectionChanged(
                                        treVehicles, FuncVehicleWeaponsBeforeClearToAdd);
                                    await objVehicle.Mods.ForEachWithSideEffectsAsync(async objMod =>
                                    {
                                        Task FuncVehicleModCyberwareBeforeClearToAdd(object x,
                                            NotifyCollectionChangedEventArgs y,
                                            CancellationToken innerToken = default) =>
                                            objMod.RefreshChildrenCyberwareClearBindings(treVehicles, y, innerToken);

                                        Task FuncVehicleModCyberwareToAdd(
                                            object x, NotifyCollectionChangedEventArgs y,
                                            CancellationToken innerToken = default) =>
                                            objMod.RefreshChildrenCyberware(
                                                treVehicles, cmsCyberware, cmsCyberwareGear, null, y,
                                                MakeDirtyWithCharacterUpdate, token: innerToken);

                                        Task FuncVehicleModWeaponsBeforeClearToAdd(object x,
                                            NotifyCollectionChangedEventArgs y,
                                            CancellationToken innerToken = default) =>
                                            objMod.RefreshChildrenWeaponsClearBindings(treVehicles, y, innerToken);

                                        Task FuncVehicleModWeaponsToAdd(
                                            object x, NotifyCollectionChangedEventArgs y,
                                            CancellationToken innerToken = default) =>
                                            objMod.RefreshChildrenWeapons(
                                                treVehicles, cmsVehicleWeapon, cmsVehicleWeaponAccessory,
                                                cmsVehicleWeaponAccessoryGear, () => objMod.Cyberware.Count, y,
                                                MakeDirtyWithCharacterUpdate, token: innerToken);

                                        objMod.Cyberware.AddTaggedCollectionChanged(
                                            treVehicles, MakeDirtyWithCharacterUpdate);
                                        objMod.Cyberware.AddTaggedCollectionChanged(
                                            treVehicles, FuncVehicleModCyberwareToAdd);
                                        objMod.Cyberware.AddTaggedBeforeClearCollectionChanged(
                                            treVehicles, FuncVehicleModCyberwareBeforeClearToAdd);
                                        await objMod.Cyberware.ForEachWithSideEffectsAsync(objCyberware =>
                                                    objCyberware
                                                        .SetupChildrenCyberwareCollectionChangedAsync(
                                                            true, treVehicles,
                                                            cmsCyberware, cmsCyberwareGear,
                                                            MakeDirtyWithCharacterUpdate, token),
                                                token)
                                            .ConfigureAwait(false);
                                        objMod.Weapons.AddTaggedCollectionChanged(
                                            treVehicles, MakeDirtyWithCharacterUpdate);
                                        objMod.Weapons.AddTaggedCollectionChanged(
                                            treVehicles, FuncVehicleModWeaponsToAdd);
                                        objMod.Weapons.AddTaggedBeforeClearCollectionChanged(
                                            treVehicles, FuncVehicleModWeaponsBeforeClearToAdd);
                                        await objMod.Weapons.ForEachWithSideEffectsAsync(objWeapon =>
                                                objWeapon
                                                    .SetupChildrenWeaponsCollectionChangedAsync(
                                                        true, treVehicles,
                                                        cmsVehicleWeapon,
                                                        cmsVehicleWeaponAccessory,
                                                        cmsVehicleWeaponAccessoryGear,
                                                        MakeDirtyWithCharacterUpdate, token), token)
                                            .ConfigureAwait(false);
                                    }, token).ConfigureAwait(false);

                                    await objVehicle.WeaponMounts.ForEachWithSideEffectsAsync(async objMount =>
                                    {
                                        Task FuncWeaponMountVehicleModBeforeClearToAdd(object x,
                                            NotifyCollectionChangedEventArgs y,
                                            CancellationToken innerToken = default) =>
                                            objMount.RefreshVehicleModsClearBindings(treVehicles, y, innerToken);

                                        Task FuncWeaponMountVehicleModToAdd(
                                            object x, NotifyCollectionChangedEventArgs y,
                                            CancellationToken innerToken = default) =>
                                            objMount.RefreshVehicleMods(treVehicles, cmsVehicle, cmsCyberware,
                                                cmsCyberwareGear, cmsVehicleWeapon,
                                                cmsVehicleWeaponAccessory,
                                                cmsVehicleWeaponAccessoryGear, null, y,
                                                MakeDirtyWithCharacterUpdate, token: innerToken);

                                        Task FuncWeaponMountWeaponsBeforeClearToAdd(object x,
                                            NotifyCollectionChangedEventArgs y,
                                            CancellationToken innerToken = default) =>
                                            objMount.RefreshChildrenWeaponsClearBindings(treVehicles, y, innerToken);

                                        Task FuncWeaponMountWeaponsToAdd(
                                            object x, NotifyCollectionChangedEventArgs y,
                                            CancellationToken innerToken = default) =>
                                            objMount.RefreshChildrenWeapons(
                                                treVehicles, cmsVehicleWeapon, cmsVehicleWeaponAccessory,
                                                cmsVehicleWeaponAccessoryGear, () => objMount.Mods.Count, y,
                                                MakeDirtyWithCharacterUpdate, token: innerToken);

                                        objMount.Mods.AddTaggedCollectionChanged(
                                            treVehicles, MakeDirtyWithCharacterUpdate);
                                        objMount.Mods.AddTaggedCollectionChanged(
                                            treVehicles, FuncWeaponMountVehicleModToAdd);
                                        objMount.Mods.AddTaggedBeforeClearCollectionChanged(
                                            treVehicles, FuncWeaponMountVehicleModBeforeClearToAdd);
                                        objMount.Weapons.AddTaggedCollectionChanged(
                                            treVehicles, MakeDirtyWithCharacterUpdate);
                                        objMount.Weapons.AddTaggedCollectionChanged(
                                            treVehicles, FuncWeaponMountWeaponsToAdd);
                                        objMount.Weapons.AddTaggedBeforeClearCollectionChanged(
                                            treVehicles, FuncWeaponMountWeaponsBeforeClearToAdd);
                                        await objMount.Weapons.ForEachWithSideEffectsAsync(objWeapon =>
                                                    objWeapon
                                                        .SetupChildrenWeaponsCollectionChangedAsync(
                                                            true, treVehicles,
                                                            cmsVehicleWeapon,
                                                            cmsVehicleWeaponAccessory,
                                                            cmsVehicleWeaponAccessoryGear,
                                                            MakeDirtyWithCharacterUpdate, token),
                                                token)
                                            .ConfigureAwait(false);
                                        await objMount.Mods.ForEachWithSideEffectsAsync(async objMod =>
                                        {
                                            Task FuncWeaponMountVehicleModCyberwareBeforeClearToAdd(object x,
                                                NotifyCollectionChangedEventArgs y,
                                                CancellationToken innerToken = default) =>
                                                objMod.RefreshChildrenCyberwareClearBindings(treVehicles, y,
                                                    innerToken);

                                            Task FuncWeaponMountVehicleModCyberwareToAdd(
                                                object x, NotifyCollectionChangedEventArgs y,
                                                CancellationToken innerToken = default) =>
                                                objMod.RefreshChildrenCyberware(
                                                    treVehicles, cmsCyberware, cmsCyberwareGear, null, y,
                                                    MakeDirtyWithCharacterUpdate, token: innerToken);

                                            Task FuncWeaponMountVehicleModWeaponsToAddBeforeClearToAdd(object x,
                                                NotifyCollectionChangedEventArgs y,
                                                CancellationToken innerToken = default) =>
                                                objMod.RefreshChildrenWeaponsClearBindings(treVehicles, y, innerToken);

                                            Task FuncWeaponMountVehicleModWeaponsToAdd(
                                                object x, NotifyCollectionChangedEventArgs y,
                                                CancellationToken innerToken = default) =>
                                                objMod.RefreshChildrenWeapons(
                                                    treVehicles, cmsVehicleWeapon, cmsVehicleWeaponAccessory,
                                                    cmsVehicleWeaponAccessoryGear, () => objMod.Cyberware.Count, y,
                                                    MakeDirtyWithCharacterUpdate, token: innerToken);

                                            objMod.Cyberware.AddTaggedCollectionChanged(
                                                treVehicles, MakeDirtyWithCharacterUpdate);
                                            objMod.Cyberware.AddTaggedCollectionChanged(
                                                treVehicles, FuncWeaponMountVehicleModCyberwareToAdd);
                                            objMod.Cyberware.AddTaggedBeforeClearCollectionChanged(
                                                treVehicles, FuncWeaponMountVehicleModCyberwareBeforeClearToAdd);
                                            await objMod.Cyberware.ForEachWithSideEffectsAsync(objCyberware =>
                                                        objCyberware
                                                            .SetupChildrenCyberwareCollectionChangedAsync(
                                                                true, treVehicles,
                                                                cmsCyberware, cmsCyberwareGear,
                                                                MakeDirtyWithCharacterUpdate, token),
                                                    token)
                                                .ConfigureAwait(false);
                                            objMod.Weapons.AddTaggedCollectionChanged(
                                                treVehicles, MakeDirtyWithCharacterUpdate);
                                            objMod.Weapons.AddTaggedCollectionChanged(
                                                treVehicles, FuncWeaponMountVehicleModWeaponsToAdd);
                                            objMod.Weapons.AddTaggedBeforeClearCollectionChanged(
                                                treVehicles, FuncWeaponMountVehicleModWeaponsToAddBeforeClearToAdd);
                                            await objMod.Weapons.ForEachWithSideEffectsAsync(objWeapon =>
                                                        objWeapon
                                                            .SetupChildrenWeaponsCollectionChangedAsync(
                                                                true, treVehicles,
                                                                cmsVehicleWeapon,
                                                                cmsVehicleWeaponAccessory,
                                                                cmsVehicleWeaponAccessoryGear,
                                                                MakeDirtyWithCharacterUpdate, token),
                                                    token)
                                                .ConfigureAwait(false);
                                        }, token).ConfigureAwait(false);
                                    }, token).ConfigureAwait(false);

                                    await objVehicle.Weapons.ForEachWithSideEffectsAsync(objWeapon =>
                                            objWeapon
                                                .SetupChildrenWeaponsCollectionChangedAsync(
                                                    true, treVehicles, cmsVehicleWeapon,
                                                    cmsVehicleWeaponAccessory,
                                                    cmsVehicleWeaponAccessoryGear,
                                                    MakeDirtyWithCharacterUpdate, token), token)
                                        .ConfigureAwait(false);

                                    Task FuncDelegateBeforeClearToAdd(object x, NotifyCollectionChangedEventArgs y,
                                        CancellationToken innerToken = default) =>
                                        objVehicle.RefreshChildrenGearsClearBindings(treVehicles, y, innerToken);

                                    Task FuncVehicleGearToAdd(object x, NotifyCollectionChangedEventArgs y,
                                        CancellationToken innerToken = default) =>
                                        objVehicle.RefreshChildrenGears(
                                            treVehicles, cmsVehicleGear, null,
                                            () => objVehicle.Mods.Count + objVehicle.Weapons.Count
                                                                        + (objVehicle.WeaponMounts.Count > 0).ToInt32(),
                                            y, MakeDirtyWithCharacterUpdate, token: innerToken);

                                    Task FuncVehicleLocationsToAdd(object x, NotifyCollectionChangedEventArgs y,
                                        CancellationToken innerToken = default) =>
                                        RefreshLocationsInVehicle(treVehicles, objVehicle, cmsVehicleLocation,
                                            () => objVehicle.Mods.Count
                                                  + objVehicle.Weapons.Count
                                                  + (objVehicle.WeaponMounts.Count > 0)
                                                  .ToInt32()
                                                  + objVehicle.GearChildren.Count(
                                                      z => z.Location == null, innerToken), y, innerToken);

                                    objVehicle.GearChildren.AddTaggedCollectionChanged(
                                        treVehicles, MakeDirtyWithCharacterUpdate);
                                    objVehicle.GearChildren.AddTaggedCollectionChanged(
                                        treVehicles, FuncVehicleGearToAdd);
                                    objVehicle.GearChildren.AddTaggedBeforeClearCollectionChanged(
                                        treVehicles, FuncDelegateBeforeClearToAdd);
                                    await objVehicle.GearChildren.ForEachWithSideEffectsAsync(objGear =>
                                                objGear
                                                    .SetupChildrenGearsCollectionChangedAsync(
                                                        true, treVehicles,
                                                        cmsVehicleGear, null,
                                                        MakeDirtyWithCharacterUpdate, token),
                                            token)
                                        .ConfigureAwait(false);
                                    objVehicle.Locations.AddTaggedCollectionChanged(
                                        treVehicles, MakeDirtyWithCharacterUpdate);
                                    objVehicle.Locations.AddTaggedCollectionChanged(
                                        treVehicles, FuncVehicleLocationsToAdd);

                                    ++intNewIndex;
                                }

                                break;
                            }
                            case NotifyCollectionChangedAction.Remove:
                            {
                                foreach (Vehicle objVehicle in e.OldItems)
                                {
                                    await objVehicle.Mods
                                        .RemoveTaggedAsyncBeforeClearCollectionChangedAsync(treVehicles, token)
                                        .ConfigureAwait(false);
                                    await objVehicle.Mods.RemoveTaggedAsyncCollectionChangedAsync(treVehicles, token)
                                        .ConfigureAwait(false);
                                    await objVehicle.WeaponMounts
                                        .RemoveTaggedAsyncBeforeClearCollectionChangedAsync(treVehicles, token)
                                        .ConfigureAwait(false);
                                    await objVehicle.WeaponMounts
                                        .RemoveTaggedAsyncCollectionChangedAsync(treVehicles, token)
                                        .ConfigureAwait(false);
                                    await objVehicle.Weapons
                                        .RemoveTaggedAsyncBeforeClearCollectionChangedAsync(treVehicles, token)
                                        .ConfigureAwait(false);
                                    await objVehicle.Weapons.RemoveTaggedAsyncCollectionChangedAsync(treVehicles, token)
                                        .ConfigureAwait(false);
                                    await objVehicle.Mods.ForEachWithSideEffectsAsync(async objMod =>
                                    {
                                        await objMod.Cyberware
                                            .RemoveTaggedAsyncBeforeClearCollectionChangedAsync(treVehicles, token)
                                            .ConfigureAwait(false);
                                        await objMod.Cyberware
                                            .RemoveTaggedAsyncCollectionChangedAsync(treVehicles, token)
                                            .ConfigureAwait(false);
                                        await objMod.Cyberware.ForEachWithSideEffectsAsync(objCyberware =>
                                                objCyberware
                                                    .SetupChildrenCyberwareCollectionChangedAsync(
                                                        false, treVehicles, token: token), token)
                                            .ConfigureAwait(false);
                                        await objMod.Weapons
                                            .RemoveTaggedAsyncBeforeClearCollectionChangedAsync(treVehicles, token)
                                            .ConfigureAwait(false);
                                        await objMod.Weapons.RemoveTaggedAsyncCollectionChangedAsync(treVehicles, token)
                                            .ConfigureAwait(false);
                                        await objMod.Weapons.ForEachWithSideEffectsAsync(objWeapon =>
                                                objWeapon
                                                    .SetupChildrenWeaponsCollectionChangedAsync(
                                                        false, treVehicles, token: token), token)
                                            .ConfigureAwait(false);
                                    }, token).ConfigureAwait(false);

                                    await objVehicle.WeaponMounts.ForEachWithSideEffectsAsync(async objMount =>
                                    {
                                        await objMount.Weapons
                                            .RemoveTaggedAsyncBeforeClearCollectionChangedAsync(treVehicles, token)
                                            .ConfigureAwait(false);
                                        await objMount.Weapons
                                            .RemoveTaggedAsyncCollectionChangedAsync(treVehicles, token)
                                            .ConfigureAwait(false);
                                        await objMount.Weapons.ForEachWithSideEffectsAsync(objWeapon =>
                                                objWeapon
                                                    .SetupChildrenWeaponsCollectionChangedAsync(
                                                        false, treVehicles, token: token), token)
                                            .ConfigureAwait(false);
                                        await objMount.Mods
                                            .RemoveTaggedAsyncBeforeClearCollectionChangedAsync(treVehicles, token)
                                            .ConfigureAwait(false);
                                        await objMount.Mods.RemoveTaggedAsyncCollectionChangedAsync(treVehicles, token)
                                            .ConfigureAwait(false);
                                        await objMount.Mods.ForEachWithSideEffectsAsync(async objMod =>
                                        {
                                            await objMod.Cyberware
                                                .RemoveTaggedAsyncBeforeClearCollectionChangedAsync(treVehicles, token)
                                                .ConfigureAwait(false);
                                            await objMod.Cyberware
                                                .RemoveTaggedAsyncCollectionChangedAsync(treVehicles, token)
                                                .ConfigureAwait(false);
                                            await objMod.Cyberware.ForEachWithSideEffectsAsync(objCyberware =>
                                                    objCyberware
                                                        .SetupChildrenCyberwareCollectionChangedAsync(
                                                            false, treVehicles, token: token), token)
                                                .ConfigureAwait(false);
                                            await objMod.Weapons
                                                .RemoveTaggedAsyncBeforeClearCollectionChangedAsync(treVehicles, token)
                                                .ConfigureAwait(false);
                                            await objMod.Weapons
                                                .RemoveTaggedAsyncCollectionChangedAsync(treVehicles, token)
                                                .ConfigureAwait(false);
                                            await objMod.Weapons.ForEachWithSideEffectsAsync(objWeapon =>
                                                    objWeapon
                                                        .SetupChildrenWeaponsCollectionChangedAsync(
                                                            false, treVehicles, token: token), token)
                                                .ConfigureAwait(false);
                                        }, token).ConfigureAwait(false);
                                    }, token).ConfigureAwait(false);

                                    await objVehicle.Weapons.ForEachWithSideEffectsAsync(objWeapon =>
                                            objWeapon
                                                .SetupChildrenWeaponsCollectionChangedAsync(
                                                    false, treVehicles, token: token), token)
                                        .ConfigureAwait(false);
                                    await objVehicle.GearChildren
                                        .RemoveTaggedAsyncBeforeClearCollectionChangedAsync(treVehicles, token)
                                        .ConfigureAwait(false);
                                    await objVehicle.GearChildren
                                        .RemoveTaggedAsyncCollectionChangedAsync(treVehicles, token)
                                        .ConfigureAwait(false);
                                    await objVehicle.GearChildren.ForEachWithSideEffectsAsync(objGear =>
                                            objGear
                                                .SetupChildrenGearsCollectionChangedAsync(
                                                    false, treVehicles, token: token), token)
                                        .ConfigureAwait(false);
                                    await objVehicle.Locations
                                        .RemoveTaggedAsyncCollectionChangedAsync(treVehicles, token)
                                        .ConfigureAwait(false);
                                    await treVehicles.DoThreadSafeAsync(x => x.FindNodeByTag(objVehicle)?.Remove(),
                                        token).ConfigureAwait(false);
                                }

                                break;
                            }
                            case NotifyCollectionChangedAction.Replace:
                            {
                                foreach (Vehicle objVehicle in e.OldItems)
                                {
                                    await objVehicle.Mods
                                        .RemoveTaggedAsyncBeforeClearCollectionChangedAsync(treVehicles, token)
                                        .ConfigureAwait(false);
                                    await objVehicle.Mods.RemoveTaggedAsyncCollectionChangedAsync(treVehicles, token)
                                        .ConfigureAwait(false);
                                    await objVehicle.WeaponMounts
                                        .RemoveTaggedAsyncBeforeClearCollectionChangedAsync(treVehicles, token)
                                        .ConfigureAwait(false);
                                    await objVehicle.WeaponMounts
                                        .RemoveTaggedAsyncCollectionChangedAsync(treVehicles, token)
                                        .ConfigureAwait(false);
                                    await objVehicle.Weapons
                                        .RemoveTaggedAsyncBeforeClearCollectionChangedAsync(treVehicles, token)
                                        .ConfigureAwait(false);
                                    await objVehicle.Weapons.RemoveTaggedAsyncCollectionChangedAsync(treVehicles, token)
                                        .ConfigureAwait(false);
                                    await objVehicle.Mods.ForEachWithSideEffectsAsync(async objMod =>
                                    {
                                        await objMod.Cyberware
                                            .RemoveTaggedAsyncBeforeClearCollectionChangedAsync(treVehicles, token)
                                            .ConfigureAwait(false);
                                        await objMod.Cyberware
                                            .RemoveTaggedAsyncCollectionChangedAsync(treVehicles, token)
                                            .ConfigureAwait(false);
                                        await objMod.Cyberware.ForEachWithSideEffectsAsync(objCyberware =>
                                                objCyberware
                                                    .SetupChildrenCyberwareCollectionChangedAsync(
                                                        false, treVehicles, token: token), token)
                                            .ConfigureAwait(false);
                                        await objMod.Weapons
                                            .RemoveTaggedAsyncBeforeClearCollectionChangedAsync(treVehicles, token)
                                            .ConfigureAwait(false);
                                        await objMod.Weapons.RemoveTaggedAsyncCollectionChangedAsync(treVehicles, token)
                                            .ConfigureAwait(false);
                                        await objMod.Weapons.ForEachWithSideEffectsAsync(objWeapon =>
                                                objWeapon
                                                    .SetupChildrenWeaponsCollectionChangedAsync(
                                                        false, treVehicles, token: token), token)
                                            .ConfigureAwait(false);
                                    }, token).ConfigureAwait(false);

                                    await objVehicle.WeaponMounts.ForEachWithSideEffectsAsync(async objMount =>
                                    {
                                        await objMount.Weapons
                                            .RemoveTaggedAsyncBeforeClearCollectionChangedAsync(treVehicles, token)
                                            .ConfigureAwait(false);
                                        await objMount.Weapons
                                            .RemoveTaggedAsyncCollectionChangedAsync(treVehicles, token)
                                            .ConfigureAwait(false);
                                        await objMount.Weapons.ForEachWithSideEffectsAsync(objWeapon =>
                                                objWeapon
                                                    .SetupChildrenWeaponsCollectionChangedAsync(
                                                        false, treVehicles, token: token), token)
                                            .ConfigureAwait(false);
                                        await objMount.Mods
                                            .RemoveTaggedAsyncBeforeClearCollectionChangedAsync(treVehicles, token)
                                            .ConfigureAwait(false);
                                        await objMount.Mods.RemoveTaggedAsyncCollectionChangedAsync(treVehicles, token)
                                            .ConfigureAwait(false);
                                        await objMount.Mods.ForEachWithSideEffectsAsync(async objMod =>
                                        {
                                            await objMod.Cyberware
                                                .RemoveTaggedAsyncBeforeClearCollectionChangedAsync(treVehicles, token)
                                                .ConfigureAwait(false);
                                            await objMod.Cyberware
                                                .RemoveTaggedAsyncCollectionChangedAsync(treVehicles, token)
                                                .ConfigureAwait(false);
                                            await objMod.Cyberware.ForEachWithSideEffectsAsync(objCyberware =>
                                                    objCyberware
                                                        .SetupChildrenCyberwareCollectionChangedAsync(
                                                            false, treVehicles, token: token), token)
                                                .ConfigureAwait(false);
                                            await objMod.Weapons
                                                .RemoveTaggedAsyncBeforeClearCollectionChangedAsync(treVehicles, token)
                                                .ConfigureAwait(false);
                                            await objMod.Weapons
                                                .RemoveTaggedAsyncCollectionChangedAsync(treVehicles, token)
                                                .ConfigureAwait(false);
                                            await objMod.Weapons.ForEachWithSideEffectsAsync(objWeapon =>
                                                    objWeapon
                                                        .SetupChildrenWeaponsCollectionChangedAsync(
                                                            false, treVehicles, token: token), token)
                                                .ConfigureAwait(false);
                                        }, token).ConfigureAwait(false);
                                    }, token).ConfigureAwait(false);

                                    await objVehicle.Weapons.ForEachWithSideEffectsAsync(objWeapon =>
                                            objWeapon
                                                .SetupChildrenWeaponsCollectionChangedAsync(
                                                    false, treVehicles, token: token), token)
                                        .ConfigureAwait(false);
                                    await objVehicle.GearChildren
                                        .RemoveTaggedAsyncBeforeClearCollectionChangedAsync(treVehicles, token)
                                        .ConfigureAwait(false);
                                    await objVehicle.GearChildren
                                        .RemoveTaggedAsyncCollectionChangedAsync(treVehicles, token)
                                        .ConfigureAwait(false);
                                    await objVehicle.GearChildren.ForEachWithSideEffectsAsync(objGear =>
                                            objGear
                                                .SetupChildrenGearsCollectionChangedAsync(
                                                    false, treVehicles, token: token), token)
                                        .ConfigureAwait(false);
                                    await objVehicle.Locations
                                        .RemoveTaggedAsyncCollectionChangedAsync(treVehicles, token)
                                        .ConfigureAwait(false);
                                    await treVehicles.DoThreadSafeAsync(x => x.FindNodeByTag(objVehicle)?.Remove(),
                                        token).ConfigureAwait(false);
                                }

                                int intNewIndex = e.NewStartingIndex;
                                foreach (Vehicle objVehicle in e.NewItems)
                                {
                                    await AddToTree(objVehicle, intNewIndex).ConfigureAwait(false);

                                    Task FuncVehicleModsBeforeClearToAdd(object x, NotifyCollectionChangedEventArgs y,
                                        CancellationToken innerToken = default) =>
                                        objVehicle.RefreshVehicleModsClearBindings(treVehicles, y, innerToken);

                                    Task FuncVehicleModsToAdd(object x, NotifyCollectionChangedEventArgs y,
                                        CancellationToken innerToken = default) =>
                                        objVehicle.RefreshVehicleMods(
                                            treVehicles, cmsVehicle, cmsCyberware, cmsCyberwareGear, cmsVehicleWeapon,
                                            cmsVehicleWeaponAccessory, cmsVehicleWeaponAccessoryGear, null, y,
                                            MakeDirtyWithCharacterUpdate, token: innerToken);

                                    Task FuncVehicleWeaponMountsBeforeClearToAdd(object x,
                                        NotifyCollectionChangedEventArgs y,
                                        CancellationToken innerToken = default) =>
                                        objVehicle.RefreshVehicleWeaponMountsClearBindings(treVehicles, y, innerToken);

                                    Task FuncVehicleWeaponMountsToAdd(object x, NotifyCollectionChangedEventArgs y,
                                        CancellationToken innerToken = default) =>
                                        objVehicle.RefreshVehicleWeaponMounts(
                                            treVehicles, cmsVehicleWeaponMount, cmsVehicleWeapon,
                                            cmsVehicleWeaponAccessory, cmsVehicleWeaponAccessoryGear, cmsCyberware,
                                            cmsCyberwareGear, cmsVehicle, () => objVehicle.Mods.Count, y,
                                            MakeDirtyWithCharacterUpdate, token: innerToken);

                                    Task FuncVehicleWeaponsBeforeClearToAdd(object x,
                                        NotifyCollectionChangedEventArgs y,
                                        CancellationToken innerToken = default) =>
                                        objVehicle.RefreshChildrenWeaponsClearBindings(treVehicles, y, innerToken);

                                    Task FuncVehicleWeaponsToAdd(object x, NotifyCollectionChangedEventArgs y,
                                        CancellationToken innerToken = default) =>
                                        objVehicle.RefreshChildrenWeapons(
                                            treVehicles, cmsVehicleWeapon, cmsVehicleWeaponAccessory,
                                            cmsVehicleWeaponAccessoryGear,
                                            () => objVehicle.Mods.Count + (objVehicle.WeaponMounts.Count > 0).ToInt32(),
                                            y, MakeDirtyWithCharacterUpdate, token: innerToken);

                                    objVehicle.Mods.AddTaggedCollectionChanged(
                                        treVehicles, MakeDirtyWithCharacterUpdate);
                                    objVehicle.Mods.AddTaggedCollectionChanged(
                                        treVehicles, FuncVehicleModsToAdd);
                                    objVehicle.Mods.AddTaggedBeforeClearCollectionChanged(
                                        treVehicles, FuncVehicleModsBeforeClearToAdd);
                                    objVehicle.WeaponMounts.AddTaggedCollectionChanged(
                                        treVehicles, MakeDirtyWithCharacterUpdate);
                                    objVehicle.WeaponMounts.AddTaggedCollectionChanged(
                                        treVehicles, FuncVehicleWeaponMountsToAdd);
                                    objVehicle.WeaponMounts.AddTaggedBeforeClearCollectionChanged(
                                        treVehicles, FuncVehicleWeaponMountsBeforeClearToAdd);
                                    objVehicle.Weapons.AddTaggedCollectionChanged(
                                        treVehicles, MakeDirtyWithCharacterUpdate);
                                    objVehicle.Weapons.AddTaggedCollectionChanged(
                                        treVehicles, FuncVehicleWeaponsToAdd);
                                    objVehicle.Weapons.AddTaggedBeforeClearCollectionChanged(
                                        treVehicles, FuncVehicleWeaponsBeforeClearToAdd);
                                    await objVehicle.Mods.ForEachWithSideEffectsAsync(async objMod =>
                                    {
                                        Task FuncVehicleModCyberwareBeforeClearToAdd(object x,
                                            NotifyCollectionChangedEventArgs y,
                                            CancellationToken innerToken = default) =>
                                            objMod.RefreshChildrenCyberwareClearBindings(treVehicles, y, innerToken);

                                        Task FuncVehicleModCyberwareToAdd(
                                            object x, NotifyCollectionChangedEventArgs y,
                                            CancellationToken innerToken = default) =>
                                            objMod.RefreshChildrenCyberware(
                                                treVehicles, cmsCyberware, cmsCyberwareGear, null, y,
                                                MakeDirtyWithCharacterUpdate, token: innerToken);

                                        Task FuncVehicleModWeaponsBeforeClearToAdd(object x,
                                            NotifyCollectionChangedEventArgs y,
                                            CancellationToken innerToken = default) =>
                                            objMod.RefreshChildrenWeaponsClearBindings(treVehicles, y, innerToken);

                                        Task FuncVehicleModWeaponsToAdd(
                                            object x, NotifyCollectionChangedEventArgs y,
                                            CancellationToken innerToken = default) =>
                                            objMod.RefreshChildrenWeapons(
                                                treVehicles, cmsVehicleWeapon, cmsVehicleWeaponAccessory,
                                                cmsVehicleWeaponAccessoryGear, () => objMod.Cyberware.Count, y,
                                                MakeDirtyWithCharacterUpdate, token: innerToken);

                                        objMod.Cyberware.AddTaggedCollectionChanged(
                                            treVehicles, MakeDirtyWithCharacterUpdate);
                                        objMod.Cyberware.AddTaggedCollectionChanged(
                                            treVehicles, FuncVehicleModCyberwareToAdd);
                                        objMod.Cyberware.AddTaggedBeforeClearCollectionChanged(
                                            treVehicles, FuncVehicleModCyberwareBeforeClearToAdd);
                                        await objMod.Cyberware.ForEachWithSideEffectsAsync(objCyberware =>
                                                    objCyberware
                                                        .SetupChildrenCyberwareCollectionChangedAsync(
                                                            true, treVehicles,
                                                            cmsCyberware, cmsCyberwareGear,
                                                            MakeDirtyWithCharacterUpdate, token),
                                                token)
                                            .ConfigureAwait(false);
                                        objMod.Weapons.AddTaggedCollectionChanged(
                                            treVehicles, MakeDirtyWithCharacterUpdate);
                                        objMod.Weapons.AddTaggedCollectionChanged(
                                            treVehicles, FuncVehicleModWeaponsToAdd);
                                        objMod.Weapons.AddTaggedBeforeClearCollectionChanged(
                                            treVehicles, FuncVehicleModWeaponsBeforeClearToAdd);
                                        await objMod.Weapons.ForEachWithSideEffectsAsync(objWeapon =>
                                                objWeapon
                                                    .SetupChildrenWeaponsCollectionChangedAsync(
                                                        true, treVehicles,
                                                        cmsVehicleWeapon,
                                                        cmsVehicleWeaponAccessory,
                                                        cmsVehicleWeaponAccessoryGear,
                                                        MakeDirtyWithCharacterUpdate, token), token)
                                            .ConfigureAwait(false);
                                    }, token).ConfigureAwait(false);

                                    await objVehicle.WeaponMounts.ForEachWithSideEffectsAsync(async objMount =>
                                    {
                                        Task FuncWeaponMountVehicleModBeforeClearToAdd(object x,
                                            NotifyCollectionChangedEventArgs y,
                                            CancellationToken innerToken = default) =>
                                            objMount.RefreshVehicleModsClearBindings(treVehicles, y, innerToken);

                                        Task FuncWeaponMountVehicleModToAdd(
                                            object x, NotifyCollectionChangedEventArgs y,
                                            CancellationToken innerToken = default) =>
                                            objMount.RefreshVehicleMods(treVehicles, cmsVehicle, cmsCyberware,
                                                cmsCyberwareGear, cmsVehicleWeapon,
                                                cmsVehicleWeaponAccessory,
                                                cmsVehicleWeaponAccessoryGear, null, y,
                                                MakeDirtyWithCharacterUpdate, token: innerToken);

                                        Task FuncWeaponMountWeaponsBeforeClearToAdd(object x,
                                            NotifyCollectionChangedEventArgs y,
                                            CancellationToken innerToken = default) =>
                                            objMount.RefreshChildrenWeaponsClearBindings(treVehicles, y, innerToken);

                                        Task FuncWeaponMountWeaponsToAdd(
                                            object x, NotifyCollectionChangedEventArgs y,
                                            CancellationToken innerToken = default) =>
                                            objMount.RefreshChildrenWeapons(
                                                treVehicles, cmsVehicleWeapon, cmsVehicleWeaponAccessory,
                                                cmsVehicleWeaponAccessoryGear, () => objMount.Mods.Count, y,
                                                MakeDirtyWithCharacterUpdate, token: innerToken);

                                        objMount.Mods.AddTaggedCollectionChanged(
                                            treVehicles, MakeDirtyWithCharacterUpdate);
                                        objMount.Mods.AddTaggedCollectionChanged(
                                            treVehicles, FuncWeaponMountVehicleModToAdd);
                                        objMount.Mods.AddTaggedBeforeClearCollectionChanged(
                                            treVehicles, FuncWeaponMountVehicleModBeforeClearToAdd);
                                        objMount.Weapons.AddTaggedCollectionChanged(
                                            treVehicles, MakeDirtyWithCharacterUpdate);
                                        objMount.Weapons.AddTaggedCollectionChanged(
                                            treVehicles, FuncWeaponMountWeaponsToAdd);
                                        objMount.Weapons.AddTaggedBeforeClearCollectionChanged(
                                            treVehicles, FuncWeaponMountWeaponsBeforeClearToAdd);
                                        await objMount.Weapons.ForEachWithSideEffectsAsync(objWeapon =>
                                                    objWeapon
                                                        .SetupChildrenWeaponsCollectionChangedAsync(
                                                            true, treVehicles,
                                                            cmsVehicleWeapon,
                                                            cmsVehicleWeaponAccessory,
                                                            cmsVehicleWeaponAccessoryGear,
                                                            MakeDirtyWithCharacterUpdate, token),
                                                token)
                                            .ConfigureAwait(false);
                                        await objMount.Mods.ForEachWithSideEffectsAsync(async objMod =>
                                        {
                                            Task FuncWeaponMountVehicleModCyberwareBeforeClearToAdd(object x,
                                                NotifyCollectionChangedEventArgs y,
                                                CancellationToken innerToken = default) =>
                                                objMod.RefreshChildrenCyberwareClearBindings(treVehicles, y,
                                                    innerToken);

                                            Task FuncWeaponMountVehicleModCyberwareToAdd(
                                                object x, NotifyCollectionChangedEventArgs y,
                                                CancellationToken innerToken = default) =>
                                                objMod.RefreshChildrenCyberware(
                                                    treVehicles, cmsCyberware, cmsCyberwareGear, null, y,
                                                    MakeDirtyWithCharacterUpdate, token: innerToken);

                                            Task FuncWeaponMountVehicleModWeaponsToAddBeforeClearToAdd(object x,
                                                NotifyCollectionChangedEventArgs y,
                                                CancellationToken innerToken = default) =>
                                                objMod.RefreshChildrenWeaponsClearBindings(treVehicles, y, innerToken);

                                            Task FuncWeaponMountVehicleModWeaponsToAdd(
                                                object x, NotifyCollectionChangedEventArgs y,
                                                CancellationToken innerToken = default) =>
                                                objMod.RefreshChildrenWeapons(
                                                    treVehicles, cmsVehicleWeapon, cmsVehicleWeaponAccessory,
                                                    cmsVehicleWeaponAccessoryGear, () => objMod.Cyberware.Count, y,
                                                    MakeDirtyWithCharacterUpdate, token: innerToken);

                                            objMod.Cyberware.AddTaggedCollectionChanged(
                                                treVehicles, MakeDirtyWithCharacterUpdate);
                                            objMod.Cyberware.AddTaggedCollectionChanged(
                                                treVehicles, FuncWeaponMountVehicleModCyberwareToAdd);
                                            objMod.Cyberware.AddTaggedBeforeClearCollectionChanged(
                                                treVehicles, FuncWeaponMountVehicleModCyberwareBeforeClearToAdd);
                                            await objMod.Cyberware.ForEachWithSideEffectsAsync(objCyberware =>
                                                        objCyberware
                                                            .SetupChildrenCyberwareCollectionChangedAsync(
                                                                true, treVehicles,
                                                                cmsCyberware, cmsCyberwareGear,
                                                                MakeDirtyWithCharacterUpdate, token),
                                                    token)
                                                .ConfigureAwait(false);
                                            objMod.Weapons.AddTaggedCollectionChanged(
                                                treVehicles, MakeDirtyWithCharacterUpdate);
                                            objMod.Weapons.AddTaggedCollectionChanged(
                                                treVehicles, FuncWeaponMountVehicleModWeaponsToAdd);
                                            objMod.Weapons.AddTaggedBeforeClearCollectionChanged(
                                                treVehicles, FuncWeaponMountVehicleModWeaponsToAddBeforeClearToAdd);
                                            await objMod.Weapons.ForEachWithSideEffectsAsync(objWeapon =>
                                                        objWeapon
                                                            .SetupChildrenWeaponsCollectionChangedAsync(
                                                                true, treVehicles,
                                                                cmsVehicleWeapon,
                                                                cmsVehicleWeaponAccessory,
                                                                cmsVehicleWeaponAccessoryGear,
                                                                MakeDirtyWithCharacterUpdate, token),
                                                    token)
                                                .ConfigureAwait(false);
                                        }, token).ConfigureAwait(false);
                                    }, token).ConfigureAwait(false);

                                    await objVehicle.Weapons.ForEachWithSideEffectsAsync(objWeapon =>
                                            objWeapon
                                                .SetupChildrenWeaponsCollectionChangedAsync(
                                                    true, treVehicles, cmsVehicleWeapon,
                                                    cmsVehicleWeaponAccessory,
                                                    cmsVehicleWeaponAccessoryGear,
                                                    MakeDirtyWithCharacterUpdate, token), token)
                                        .ConfigureAwait(false);

                                    Task FuncDelegateBeforeClearToAdd(object x, NotifyCollectionChangedEventArgs y,
                                        CancellationToken innerToken = default) =>
                                        objVehicle.RefreshChildrenGearsClearBindings(treVehicles, y, innerToken);

                                    Task FuncVehicleGearToAdd(object x, NotifyCollectionChangedEventArgs y,
                                        CancellationToken innerToken = default) =>
                                        objVehicle.RefreshChildrenGears(
                                            treVehicles, cmsVehicleGear, null,
                                            () => objVehicle.Mods.Count + objVehicle.Weapons.Count
                                                                        + (objVehicle.WeaponMounts.Count > 0).ToInt32(),
                                            y, MakeDirtyWithCharacterUpdate, token: innerToken);

                                    Task FuncVehicleLocationsToAdd(object x, NotifyCollectionChangedEventArgs y,
                                        CancellationToken innerToken = default) =>
                                        RefreshLocationsInVehicle(treVehicles, objVehicle, cmsVehicleLocation,
                                            () => objVehicle.Mods.Count
                                                  + objVehicle.Weapons.Count
                                                  + (objVehicle.WeaponMounts.Count > 0)
                                                  .ToInt32()
                                                  + objVehicle.GearChildren.Count(
                                                      z => z.Location == null, innerToken), y, innerToken);

                                    objVehicle.GearChildren.AddTaggedCollectionChanged(
                                        treVehicles, MakeDirtyWithCharacterUpdate);
                                    objVehicle.GearChildren.AddTaggedCollectionChanged(
                                        treVehicles, FuncVehicleGearToAdd);
                                    objVehicle.GearChildren.AddTaggedBeforeClearCollectionChanged(
                                        treVehicles, FuncDelegateBeforeClearToAdd);
                                    await objVehicle.GearChildren.ForEachWithSideEffectsAsync(objGear =>
                                                objGear
                                                    .SetupChildrenGearsCollectionChangedAsync(
                                                        true, treVehicles,
                                                        cmsVehicleGear, null,
                                                        MakeDirtyWithCharacterUpdate, token),
                                            token)
                                        .ConfigureAwait(false);
                                    objVehicle.Locations.AddTaggedCollectionChanged(
                                        treVehicles, MakeDirtyWithCharacterUpdate);
                                    objVehicle.Locations.AddTaggedCollectionChanged(
                                        treVehicles, FuncVehicleLocationsToAdd);

                                    ++intNewIndex;
                                }

                                await treVehicles.DoThreadSafeAsync(x => x.SelectedNode = x.FindNode(strSelectedId),
                                    token).ConfigureAwait(false);
                                break;
                            }
                            case NotifyCollectionChangedAction.Move:
                            {
                                await treVehicles.DoThreadSafeAsync(x =>
                                {
                                    foreach (Vehicle objVehicle in e.OldItems)
                                    {
                                        x.FindNodeByTag(objVehicle)?.Remove();
                                    }
                                }, token).ConfigureAwait(false);

                                int intNewIndex = e.NewStartingIndex;
                                foreach (Vehicle objVehicle in e.NewItems)
                                {
                                    await AddToTree(objVehicle, intNewIndex).ConfigureAwait(false);
                                    ++intNewIndex;
                                }

                                await treVehicles.DoThreadSafeAsync(x => x.SelectedNode = x.FindNode(strSelectedId),
                                    token).ConfigureAwait(false);
                                break;
                            }
                        }
                    }

                    async ValueTask AddToTree(Vehicle objVehicle, int intIndex = -1, bool blnSingleAdd = true)
                    {
                        TreeNode objNode = await objVehicle.CreateTreeNode(cmsVehicle, cmsVehicleLocation,
                            cmsVehicleWeapon,
                            cmsVehicleWeaponAccessory,
                            cmsVehicleWeaponAccessoryGear, cmsVehicleGear,
                            cmsVehicleWeaponMount,
                            cmsCyberware, cmsCyberwareGear, token).ConfigureAwait(false);
                        if (objNode == null)
                            return;

                        TreeNode nodParent = null;
                        if (objVehicle.Location != null)
                        {
                            nodParent = await treVehicles.DoThreadSafeFuncAsync(
                                x => x.FindNodeByTag(objVehicle.Location, false), token).ConfigureAwait(false);
                        }

                        if (nodParent == null)
                        {
                            if (nodRoot == null)
                            {
                                nodRoot = new TreeNode
                                {
                                    Tag = "Node_SelectedVehicles",
                                    Text = await LanguageManager.GetStringAsync("Node_SelectedVehicles", token: token)
                                        .ConfigureAwait(false)
                                };
                                await treVehicles.DoThreadSafeAsync(x => x.Nodes.Insert(0, nodRoot), token)
                                    .ConfigureAwait(false);
                            }

                            nodParent = nodRoot;
                        }

                        await treVehicles.DoThreadSafeAsync(x =>
                        {
                            if (intIndex >= 0)
                                nodParent.Nodes.Insert(intIndex, objNode);
                            else
                                nodParent.Nodes.Add(objNode);
                            nodParent.Expand();
                            if (blnSingleAdd)
                                x.SelectedNode = objNode;
                        }, token).ConfigureAwait(false);
                    }
                }
                finally
                {
                    SkipUpdate = false;
                }
            }
            finally
            {
                await objCursorWait.DisposeAsync().ConfigureAwait(false);
            }
        }

        public async Task RefreshFociFromGear(TreeView treFoci, ContextMenuStrip cmsFocus, NotifyCollectionChangedEventArgs e = null, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (treFoci == null)
                return;
            CursorWait objCursorWait = await CursorWait.NewAsync(this, token: token).ConfigureAwait(false);
            try
            {
                SkipUpdate = true;
                try
                {
                    string strSelectedId
                        = (await treFoci.DoThreadSafeFuncAsync(x => x.SelectedNode?.Tag, token)
                            .ConfigureAwait(false) as IHasInternalId)
                        ?.InternalId ?? string.Empty;

                    if (e == null ||
                        e.Action == NotifyCollectionChangedAction.Reset)
                    {
                        await treFoci.DoThreadSafeAsync(x => x.SuspendLayout(), token).ConfigureAwait(false);
                        try
                        {
                            await treFoci.DoThreadSafeAsync(x => x.Nodes.Clear(), token).ConfigureAwait(false);

                            int intFociTotal = 0;

                            int intMaxFocusTotal = await (await CharacterObject.GetAttributeAsync("MAG", token: token)
                                    .ConfigureAwait(false))
                                .GetTotalValueAsync(token).ConfigureAwait(false) * 5;
                            if (await CharacterObject.GetIsMysticAdeptAsync(token).ConfigureAwait(false) &&
                                await CharacterObjectSettings.GetMysAdeptSecondMAGAttributeAsync(token)
                                    .ConfigureAwait(false))
                                intMaxFocusTotal = Math.Min(intMaxFocusTotal,
                                    await (await CharacterObject.GetAttributeAsync("MAGAdept", token: token)
                                            .ConfigureAwait(false))
                                        .GetTotalValueAsync(token).ConfigureAwait(false) * 5);

                            await (await CharacterObject.GetGearAsync(token).ConfigureAwait(false)).ForEachAsync(
                                async objGear =>
                                {
                                    switch (objGear.Category)
                                    {
                                        case "Foci":
                                        case "Metamagic Foci":
                                        {
                                            TreeNode objNode = await objGear.CreateTreeNode(cmsFocus, null, token)
                                                .ConfigureAwait(false);
                                            if (objNode == null)
                                                return;
                                            objNode.Text = await objNode.Text.CheapReplaceAsync(
                                                await LanguageManager.GetStringAsync("String_Rating", token: token)
                                                    .ConfigureAwait(false),
                                                () => LanguageManager.GetStringAsync(objGear.RatingLabel, token: token),
                                                token: token).ConfigureAwait(false);
                                            for (int i = await CharacterObject.Foci.GetCountAsync(token).ConfigureAwait(false) - 1; i >= 0; --i)
                                            {
                                                if (i < await CharacterObject.Foci.GetCountAsync(token).ConfigureAwait(false))
                                                {
                                                    Focus objFocus = await CharacterObject.Foci.GetValueAtAsync(i, token).ConfigureAwait(false);
                                                    if (objFocus.GearObject == objGear)
                                                    {
                                                        intFociTotal += objFocus.Rating;
                                                        // Do not let the number of BP spend on bonded Foci exceed MAG * 5.
                                                        if (intFociTotal > intMaxFocusTotal &&
                                                            !await CharacterObject.GetIgnoreRulesAsync(token).ConfigureAwait(false))
                                                        {
                                                            objGear.Bonded = false;
                                                            await CharacterObject.Foci.RemoveAtAsync(i, token: token)
                                                                .ConfigureAwait(false);
                                                            objNode.Checked = false;
                                                        }
                                                        else
                                                            objNode.Checked = true;
                                                    }
                                                }
                                            }

                                            await AddToTree(objNode, false).ConfigureAwait(false);
                                        }
                                            break;

                                        case "Stacked Focus":
                                        {
                                            await CharacterObject.StackedFoci.ForEachAsync(async objStack =>
                                            {
                                                if (objStack.GearId == objGear.InternalId)
                                                {
                                                    await ImprovementManager.RemoveImprovementsAsync(CharacterObject,
                                                            Improvement.ImprovementSource.StackedFocus,
                                                            objStack.InternalId, token)
                                                        .ConfigureAwait(false);

                                                    if (objStack.Bonded)
                                                    {
                                                        await objStack.Gear.ForEachAsync(async objFociGear =>
                                                        {
                                                            if (!string.IsNullOrEmpty(objFociGear.Extra))
                                                                ImprovementManager.ForcedValue = objFociGear.Extra;
                                                            await ImprovementManager.CreateImprovementsAsync(
                                                                CharacterObject,
                                                                Improvement.ImprovementSource.StackedFocus,
                                                                objStack.InternalId,
                                                                objFociGear.Bonus,
                                                                await objFociGear.GetRatingAsync(token)
                                                                    .ConfigureAwait(false),
                                                                await objFociGear.DisplayNameShortAsync(
                                                                        GlobalSettings.Language, token)
                                                                    .ConfigureAwait(false),
                                                                token: token).ConfigureAwait(false);
                                                            if (objFociGear.WirelessOn)
                                                                await ImprovementManager.CreateImprovementsAsync(
                                                                    CharacterObject,
                                                                    Improvement.ImprovementSource.StackedFocus,
                                                                    objStack.InternalId,
                                                                    objFociGear.WirelessBonus,
                                                                    await objFociGear.GetRatingAsync(token)
                                                                        .ConfigureAwait(false),
                                                                    await objFociGear.DisplayNameShortAsync(
                                                                            GlobalSettings.Language, token)
                                                                        .ConfigureAwait(false),
                                                                    token: token).ConfigureAwait(false);
                                                        }, token).ConfigureAwait(false);
                                                    }

                                                    await AddToTree(
                                                            await objStack.CreateTreeNode(objGear, cmsFocus, token)
                                                                .ConfigureAwait(false), false)
                                                        .ConfigureAwait(false);
                                                }
                                            }, token).ConfigureAwait(false);
                                        }
                                            break;
                                    }
                                }, token).ConfigureAwait(false);

                            await treFoci.DoThreadSafeAsync(x => x.SortCustomAlphabetically(strSelectedId), token)
                                .ConfigureAwait(false);
                        }
                        finally
                        {
                            await treFoci.DoThreadSafeAsync(x => x.ResumeLayout(), GenericToken).ConfigureAwait(false);
                        }
                    }
                    else
                    {
                        switch (e.Action)
                        {
                            case NotifyCollectionChangedAction.Add:
                            {
                                bool blnWarned = false;
                                int intMaxFocusTotal = await (await CharacterObject
                                        .GetAttributeAsync("MAG", token: token).ConfigureAwait(false))
                                    .GetTotalValueAsync(token).ConfigureAwait(false) * 5;
                                if (await CharacterObject.GetIsMysticAdeptAsync(token).ConfigureAwait(false) &&
                                    await CharacterObjectSettings.GetMysAdeptSecondMAGAttributeAsync(token)
                                        .ConfigureAwait(false))
                                    intMaxFocusTotal = Math.Min(intMaxFocusTotal,
                                        await (await CharacterObject.GetAttributeAsync("MAGAdept", token: token)
                                                .ConfigureAwait(false))
                                            .GetTotalValueAsync(token).ConfigureAwait(false) * 5);

                                HashSet<Gear> setNewGears = new HashSet<Gear>();
                                foreach (Gear objGear in e.NewItems)
                                    setNewGears.Add(objGear);

                                int intFociTotal = await CharacterObject.Foci
                                    .SumAsync(x => !setNewGears.Contains(x.GearObject), x => x.Rating, token)
                                    .ConfigureAwait(false);

                                foreach (Gear objGear in e.NewItems)
                                {
                                    switch (objGear.Category)
                                    {
                                        case "Foci":
                                        case "Metamagic Foci":
                                        {
                                            TreeNode objNode = await objGear.CreateTreeNode(cmsFocus, null, token)
                                                .ConfigureAwait(false);
                                            if (objNode == null)
                                                continue;
                                            objNode.Text = await objNode.Text.CheapReplaceAsync(
                                                await LanguageManager.GetStringAsync("String_Rating", token: token)
                                                    .ConfigureAwait(false),
                                                () => LanguageManager.GetStringAsync("String_Force", token: token),
                                                token: token).ConfigureAwait(false);
                                            for (int i = await CharacterObject.Foci.GetCountAsync(token).ConfigureAwait(false) - 1; i >= 0; --i)
                                            {
                                                if (i < await CharacterObject.Foci.GetCountAsync(token).ConfigureAwait(false))
                                                {
                                                    Focus objFocus = await CharacterObject.Foci.GetValueAtAsync(i, token).ConfigureAwait(false);
                                                    if (objFocus.GearObject == objGear)
                                                    {
                                                        intFociTotal += objFocus.Rating;
                                                        // Do not let the number of BP spend on bonded Foci exceed MAG * 5.
                                                        if (intFociTotal > intMaxFocusTotal &&
                                                            !await CharacterObject.GetIgnoreRulesAsync(token).ConfigureAwait(false))
                                                        {
                                                            // Mark the Gear a Bonded.
                                                            objGear.Bonded = false;
                                                            await CharacterObject.Foci.RemoveAtAsync(i, token: token)
                                                                .ConfigureAwait(false);
                                                            objNode.Checked = false;
                                                            if (!blnWarned)
                                                            {
                                                                await Program.ShowScrollableMessageBoxAsync(this,
                                                                    await LanguageManager.GetStringAsync(
                                                                            "Message_FocusMaximumForce", token: token)
                                                                        .ConfigureAwait(false),
                                                                    await LanguageManager.GetStringAsync(
                                                                            "MessageTitle_FocusMaximum", token: token)
                                                                        .ConfigureAwait(false),
                                                                    MessageBoxButtons.OK, MessageBoxIcon.Information,
                                                                    token: token).ConfigureAwait(false);
                                                                blnWarned = true;
                                                                break;
                                                            }
                                                        }
                                                        else
                                                            objNode.Checked = true;
                                                    }
                                                }
                                            }

                                            await AddToTree(objNode).ConfigureAwait(false);
                                        }
                                            break;

                                        case "Stacked Focus":
                                        {
                                            await CharacterObject.StackedFoci.ForEachAsync(async objStack =>
                                            {
                                                if (objStack.GearId == objGear.InternalId)
                                                {
                                                    await ImprovementManager.RemoveImprovementsAsync(CharacterObject,
                                                        Improvement.ImprovementSource.StackedFocus, objStack.InternalId,
                                                        token).ConfigureAwait(false);

                                                    if (objStack.Bonded)
                                                    {
                                                        await objStack.Gear.ForEachAsync(async objFociGear =>
                                                        {
                                                            if (!string.IsNullOrEmpty(objFociGear.Extra))
                                                                ImprovementManager.ForcedValue = objFociGear.Extra;
                                                            await ImprovementManager.CreateImprovementsAsync(
                                                                CharacterObject,
                                                                Improvement.ImprovementSource.StackedFocus,
                                                                objStack.InternalId, objFociGear.Bonus,
                                                                await objFociGear.GetRatingAsync(token)
                                                                    .ConfigureAwait(false),
                                                                await objFociGear.DisplayNameShortAsync(
                                                                        GlobalSettings.Language, token)
                                                                    .ConfigureAwait(false),
                                                                token: token).ConfigureAwait(false);
                                                            if (objFociGear.WirelessOn)
                                                                await ImprovementManager.CreateImprovementsAsync(
                                                                        CharacterObject,
                                                                        Improvement.ImprovementSource.StackedFocus,
                                                                        objStack.InternalId, objFociGear.WirelessBonus,
                                                                        await objFociGear.GetRatingAsync(token)
                                                                            .ConfigureAwait(false),
                                                                        await objFociGear.DisplayNameShortAsync(
                                                                                GlobalSettings.Language, token)
                                                                            .ConfigureAwait(false), token: token)
                                                                    .ConfigureAwait(false);
                                                        }, token).ConfigureAwait(false);
                                                    }

                                                    await AddToTree(await objStack
                                                            .CreateTreeNode(objGear, cmsFocus, token)
                                                            .ConfigureAwait(false))
                                                        .ConfigureAwait(false);
                                                }
                                            }, token).ConfigureAwait(false);
                                        }
                                            break;
                                    }
                                }
                            }
                                break;

                            case NotifyCollectionChangedAction.Remove:
                            {
                                foreach (Gear objGear in e.OldItems)
                                {
                                    switch (objGear.Category)
                                    {
                                        case "Foci":
                                        case "Metamagic Foci":
                                        {
                                            for (int i = await CharacterObject.Foci.GetCountAsync(token).ConfigureAwait(false) - 1; i >= 0; --i)
                                            {
                                                if (i < await CharacterObject.Foci.GetCountAsync(token).ConfigureAwait(false))
                                                {
                                                    Focus objFocus = await CharacterObject.Foci.GetValueAtAsync(i, token).ConfigureAwait(false);
                                                    if (objFocus.GearObject == objGear)
                                                    {
                                                        await CharacterObject.Foci.RemoveAtAsync(i, token: token)
                                                            .ConfigureAwait(false);
                                                    }
                                                }
                                            }

                                            await treFoci.DoThreadSafeAsync(x => x.FindNodeByTag(objGear)?.Remove(),
                                                token).ConfigureAwait(false);
                                        }
                                            break;

                                        case "Stacked Focus":
                                        {
                                            for (int i = await CharacterObject.StackedFoci.GetCountAsync(token).ConfigureAwait(false) - 1; i >= 0; --i)
                                            {
                                                if (i < await CharacterObject.StackedFoci.GetCountAsync(token).ConfigureAwait(false))
                                                {
                                                    StackedFocus objStack = await CharacterObject.StackedFoci.GetValueAtAsync(i, token).ConfigureAwait(false);
                                                    if (objStack.GearId == objGear.InternalId)
                                                    {
                                                        await CharacterObject.StackedFoci.RemoveAtAsync(i, token: token)
                                                            .ConfigureAwait(false);
                                                        await treFoci.DoThreadSafeAsync(
                                                            x =>
                                                            {
                                                                x.FindNodeByTag(objStack)?.Remove();
                                                                objStack.Dispose();
                                                            }, token).ConfigureAwait(false);
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
                                foreach (Gear objGear in e.OldItems)
                                {
                                    switch (objGear.Category)
                                    {
                                        case "Foci":
                                        case "Metamagic Foci":
                                        {
                                            for (int i = await CharacterObject.Foci.GetCountAsync(token).ConfigureAwait(false) - 1; i >= 0; --i)
                                            {
                                                if (i < await CharacterObject.Foci.GetCountAsync(token).ConfigureAwait(false))
                                                {
                                                    Focus objFocus = await CharacterObject.Foci.GetValueAtAsync(i, token).ConfigureAwait(false);
                                                    if (objFocus.GearObject == objGear)
                                                    {
                                                        await CharacterObject.Foci.RemoveAtAsync(i, token: token)
                                                            .ConfigureAwait(false);
                                                    }
                                                }
                                            }

                                            await treFoci.DoThreadSafeAsync(x => x.FindNodeByTag(objGear)?.Remove(),
                                                token).ConfigureAwait(false);
                                        }
                                            break;

                                        case "Stacked Focus":
                                        {
                                            for (int i = await CharacterObject.StackedFoci.GetCountAsync(token).ConfigureAwait(false) - 1; i >= 0; --i)
                                            {
                                                if (i < await CharacterObject.StackedFoci.GetCountAsync(token).ConfigureAwait(false))
                                                {
                                                    StackedFocus objStack = await CharacterObject.StackedFoci.GetValueAtAsync(i, token).ConfigureAwait(false);
                                                    if (objStack.GearId == objGear.InternalId)
                                                    {
                                                        await CharacterObject.StackedFoci.RemoveAtAsync(i, token: token)
                                                            .ConfigureAwait(false);
                                                        await treFoci.DoThreadSafeAsync(
                                                            x =>
                                                            {
                                                                x.FindNodeByTag(objStack)?.Remove();
                                                                objStack.Dispose();
                                                            }, token).ConfigureAwait(false);
                                                    }
                                                }
                                            }
                                        }
                                            break;
                                    }
                                }

                                bool blnWarned = false;
                                int intMaxFocusTotal = await (await CharacterObject
                                        .GetAttributeAsync("MAG", token: token).ConfigureAwait(false))
                                    .GetTotalValueAsync(token).ConfigureAwait(false) * 5;
                                if (await CharacterObject.GetIsMysticAdeptAsync(token).ConfigureAwait(false) &&
                                    await CharacterObjectSettings.GetMysAdeptSecondMAGAttributeAsync(token)
                                        .ConfigureAwait(false))
                                    intMaxFocusTotal = Math.Min(intMaxFocusTotal,
                                        await (await CharacterObject.GetAttributeAsync("MAGAdept", token: token)
                                                .ConfigureAwait(false))
                                            .GetTotalValueAsync(token).ConfigureAwait(false) * 5);

                                HashSet<Gear> setNewGears = new HashSet<Gear>();
                                foreach (Gear objGear in e.NewItems)
                                    setNewGears.Add(objGear);

                                int intFociTotal = await CharacterObject.Foci
                                    .SumAsync(x => !setNewGears.Contains(x.GearObject), x => x.Rating, token)
                                    .ConfigureAwait(false);

                                foreach (Gear objGear in e.NewItems)
                                {
                                    switch (objGear.Category)
                                    {
                                        case "Foci":
                                        case "Metamagic Foci":
                                        {
                                            TreeNode objNode = await objGear.CreateTreeNode(cmsFocus, null, token)
                                                .ConfigureAwait(false);
                                            if (objNode == null)
                                                continue;
                                            objNode.Text = await objNode.Text.CheapReplaceAsync(
                                                await LanguageManager.GetStringAsync("String_Rating", token: token)
                                                    .ConfigureAwait(false),
                                                () => LanguageManager.GetString("String_Force", token: token),
                                                token: token).ConfigureAwait(false);
                                            for (int i = await CharacterObject.Foci.GetCountAsync(token).ConfigureAwait(false) - 1; i >= 0; --i)
                                            {
                                                if (i < await CharacterObject.Foci.GetCountAsync(token).ConfigureAwait(false))
                                                {
                                                    Focus objFocus = await CharacterObject.Foci.GetValueAtAsync(i, token).ConfigureAwait(false);
                                                    if (objFocus.GearObject == objGear)
                                                    {
                                                        intFociTotal += objFocus.Rating;
                                                        // Do not let the number of BP spend on bonded Foci exceed MAG * 5.
                                                        if (intFociTotal > intMaxFocusTotal &&
                                                            !await CharacterObject.GetIgnoreRulesAsync(token).ConfigureAwait(false))
                                                        {
                                                            // Mark the Gear a Bonded.
                                                            objGear.Bonded = false;
                                                            await CharacterObject.Foci.RemoveAtAsync(i, token: token)
                                                                .ConfigureAwait(false);
                                                            objNode.Checked = false;
                                                            if (!blnWarned)
                                                            {
                                                                await Program.ShowScrollableMessageBoxAsync(this,
                                                                    await LanguageManager.GetStringAsync(
                                                                            "Message_FocusMaximumForce", token: token)
                                                                        .ConfigureAwait(false),
                                                                    await LanguageManager.GetStringAsync(
                                                                            "MessageTitle_FocusMaximum", token: token)
                                                                        .ConfigureAwait(false),
                                                                    MessageBoxButtons.OK, MessageBoxIcon.Information,
                                                                    token: token).ConfigureAwait(false);
                                                                blnWarned = true;
                                                                break;
                                                            }
                                                        }
                                                        else
                                                            objNode.Checked = true;
                                                    }
                                                }
                                            }

                                            await AddToTree(objNode).ConfigureAwait(false);
                                        }
                                            break;

                                        case "Stacked Focus":
                                        {
                                            await CharacterObject.StackedFoci.ForEachAsync(async objStack =>
                                            {
                                                if (objStack.GearId == objGear.InternalId)
                                                {
                                                    await ImprovementManager.RemoveImprovementsAsync(CharacterObject,
                                                        Improvement.ImprovementSource.StackedFocus, objStack.InternalId,
                                                        token).ConfigureAwait(false);

                                                    if (objStack.Bonded)
                                                    {
                                                        await objStack.Gear.ForEachAsync(async objFociGear =>
                                                        {
                                                            if (!string.IsNullOrEmpty(objFociGear.Extra))
                                                                ImprovementManager.ForcedValue = objFociGear.Extra;
                                                            await ImprovementManager.CreateImprovementsAsync(
                                                                CharacterObject,
                                                                Improvement.ImprovementSource.StackedFocus,
                                                                objStack.InternalId, objFociGear.Bonus,
                                                                await objFociGear.GetRatingAsync(token)
                                                                    .ConfigureAwait(false),
                                                                await objFociGear.DisplayNameShortAsync(
                                                                        GlobalSettings.Language, token)
                                                                    .ConfigureAwait(false),
                                                                token: token).ConfigureAwait(false);
                                                            if (objFociGear.WirelessOn)
                                                                await ImprovementManager.CreateImprovementsAsync(
                                                                        CharacterObject,
                                                                        Improvement.ImprovementSource.StackedFocus,
                                                                        objStack.InternalId, objFociGear.WirelessBonus,
                                                                        await objFociGear.GetRatingAsync(token)
                                                                            .ConfigureAwait(false),
                                                                        await objFociGear.DisplayNameShortAsync(
                                                                                GlobalSettings.Language, token)
                                                                            .ConfigureAwait(false), token: token)
                                                                    .ConfigureAwait(false);
                                                        }, token).ConfigureAwait(false);
                                                    }

                                                    await AddToTree(await objStack
                                                            .CreateTreeNode(objGear, cmsFocus, token)
                                                            .ConfigureAwait(false))
                                                        .ConfigureAwait(false);
                                                }
                                            }, token).ConfigureAwait(false);
                                        }
                                            break;
                                    }
                                }
                            }
                                break;
                        }
                    }

                    Task AddToTree(TreeNode objNode, bool blnSingleAdd = true)
                    {
                        return treFoci.DoThreadSafeAsync(x =>
                        {
                            TreeNodeCollection lstParentNodeChildren = x.Nodes;
                            if (blnSingleAdd)
                            {
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
                                x.SelectedNode = objNode;
                            }
                            else
                                lstParentNodeChildren.Add(objNode);
                        }, token);
                    }
                }
                finally
                {
                    SkipUpdate = false;
                }
            }
            finally
            {
                await objCursorWait.DisposeAsync().ConfigureAwait(false);
            }
        }

        protected async Task RefreshMartialArtsClearBindings(TreeView treMartialArts, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (treMartialArts == null)
                return;
            CursorWait objCursorWait = await CursorWait.NewAsync(this, token: token).ConfigureAwait(false);
            try
            {
                SkipUpdate = true;
                try
                {
                    await CharacterObject.MartialArts.ForEachWithSideEffectsAsync(
                        objMartialArt =>
                            objMartialArt.Techniques.RemoveTaggedAsyncCollectionChangedAsync(treMartialArts, token),
                        token).ConfigureAwait(false);
                }
                finally
                {
                    SkipUpdate = false;
                }
            }
            finally
            {
                await objCursorWait.DisposeAsync().ConfigureAwait(false);
            }
        }

        protected async Task RefreshMartialArts(TreeView treMartialArts, ContextMenuStrip cmsMartialArts, ContextMenuStrip cmsTechnique, NotifyCollectionChangedEventArgs e = null, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (treMartialArts == null)
                return;
            CursorWait objCursorWait = await CursorWait.NewAsync(this, token: token).ConfigureAwait(false);
            try
            {
                SkipUpdate = true;
                try
                {
                    string strSelectedId
                        = (await treMartialArts.DoThreadSafeFuncAsync(x => x.SelectedNode?.Tag, token)
                                .ConfigureAwait(false) as
                            IHasInternalId)?.InternalId ?? string.Empty;

                    TreeNode objMartialArtsParentNode = null;
                    TreeNode objQualityNode = null;

                    if (e == null ||
                        e.Action == NotifyCollectionChangedAction.Reset)
                    {
                        await treMartialArts.DoThreadSafeAsync(x => x.SuspendLayout(), token).ConfigureAwait(false);
                        try
                        {
                            await treMartialArts.DoThreadSafeAsync(x => x.Nodes.Clear(), token).ConfigureAwait(false);

                            await CharacterObject.MartialArts.ForEachWithSideEffectsAsync(async objMartialArt =>
                            {
                                await AddToTree(objMartialArt, false).ConfigureAwait(false);
                                objMartialArt.Techniques.AddTaggedCollectionChanged(
                                    treMartialArts, MakeDirtyWithCharacterUpdate);
                                objMartialArt.Techniques.AddTaggedCollectionChanged(
                                    treMartialArts, FuncDelegateToAdd);

                                Task FuncDelegateToAdd(object x, NotifyCollectionChangedEventArgs y,
                                    CancellationToken innerToken = default) =>
                                    RefreshMartialArtTechniques(treMartialArts, objMartialArt, cmsTechnique, y,
                                        innerToken);
                            }, token).ConfigureAwait(false);

                            await treMartialArts.DoThreadSafeAsync(x => x.SortCustomAlphabetically(strSelectedId),
                                token).ConfigureAwait(false);
                        }
                        finally
                        {
                            await treMartialArts.DoThreadSafeAsync(x => x.ResumeLayout(), GenericToken)
                                .ConfigureAwait(false);
                        }
                    }
                    else
                    {
                        objMartialArtsParentNode
                            = await treMartialArts.DoThreadSafeFuncAsync(
                                x => x.FindNode("Node_SelectedMartialArts", false),
                                token).ConfigureAwait(false);
                        objQualityNode
                            = await treMartialArts.DoThreadSafeFuncAsync(
                                x => x.FindNode("Node_SelectedQualities", false),
                                token).ConfigureAwait(false);
                        switch (e.Action)
                        {
                            case NotifyCollectionChangedAction.Add:
                            {
                                foreach (MartialArt objMartialArt in e.NewItems)
                                {
                                    await AddToTree(objMartialArt).ConfigureAwait(false);
                                    objMartialArt.Techniques.AddTaggedCollectionChanged(
                                        treMartialArts, MakeDirtyWithCharacterUpdate);
                                    objMartialArt.Techniques.AddTaggedCollectionChanged(
                                        treMartialArts, FuncDelegateToAdd);

                                    Task FuncDelegateToAdd(object x, NotifyCollectionChangedEventArgs y,
                                        CancellationToken innerToken = default) =>
                                        RefreshMartialArtTechniques(treMartialArts, objMartialArt, cmsTechnique, y,
                                            innerToken);
                                }
                            }
                                break;

                            case NotifyCollectionChangedAction.Remove:
                            {
                                foreach (MartialArt objMartialArt in e.OldItems)
                                {
                                    await objMartialArt.Techniques
                                        .RemoveTaggedAsyncCollectionChangedAsync(treMartialArts, token)
                                        .ConfigureAwait(false);
                                    await treMartialArts.DoThreadSafeAsync(x =>
                                    {
                                        TreeNode objNode = x.FindNodeByTag(objMartialArt);
                                        if (objNode != null)
                                        {
                                            TreeNode objParent = objNode.Parent;
                                            objNode.Remove();
                                            if (objParent.Nodes.Count == 0)
                                                objParent.Remove();
                                        }
                                    }, token).ConfigureAwait(false);
                                }

                                break;
                            }
                            case NotifyCollectionChangedAction.Replace:
                            {
                                List<TreeNode> lstOldParents =
                                    new List<TreeNode>(e.OldItems.Count);
                                foreach (MartialArt objMartialArt in e.OldItems)
                                {
                                    await objMartialArt.Techniques
                                        .RemoveTaggedAsyncCollectionChangedAsync(treMartialArts, token)
                                        .ConfigureAwait(false);
                                    await treMartialArts.DoThreadSafeAsync(x =>
                                    {
                                        TreeNode objNode = x.FindNodeByTag(objMartialArt);
                                        if (objNode != null)
                                        {
                                            lstOldParents.Add(objNode.Parent);
                                            objNode.Remove();
                                        }
                                    }, token).ConfigureAwait(false);
                                }

                                foreach (MartialArt objMartialArt in e.NewItems)
                                {
                                    await AddToTree(objMartialArt).ConfigureAwait(false);
                                    objMartialArt.Techniques.AddTaggedCollectionChanged(
                                        treMartialArts, MakeDirtyWithCharacterUpdate);
                                    objMartialArt.Techniques.AddTaggedCollectionChanged(
                                        treMartialArts, FuncDelegateToAdd);

                                    Task FuncDelegateToAdd(object x, NotifyCollectionChangedEventArgs y,
                                        CancellationToken innerToken = default) =>
                                        RefreshMartialArtTechniques(treMartialArts, objMartialArt, cmsTechnique, y,
                                            innerToken);
                                }

                                await treMartialArts.DoThreadSafeAsync(() =>
                                {
                                    foreach (TreeNode objOldParent in lstOldParents)
                                    {
                                        if (objOldParent.Nodes.Count == 0)
                                            objOldParent.Remove();
                                    }
                                }, token).ConfigureAwait(false);
                            }
                                break;
                        }
                    }

                    async Task AddToTree(MartialArt objMartialArt, bool blnSingleAdd = true)
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
                                    Text = await LanguageManager.GetStringAsync("Node_SelectedQualities", token: token)
                                        .ConfigureAwait(false)
                                };
                                await treMartialArts.DoThreadSafeAsync(x =>
                                {
                                    // ReSharper disable once AssignNullToNotNullAttribute
                                    x.Nodes.Add(objQualityNode);
                                    objQualityNode.Expand();
                                }, token).ConfigureAwait(false);
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
                                    Text = await LanguageManager
                                        .GetStringAsync("Node_SelectedMartialArts", token: token).ConfigureAwait(false)
                                };
                                await treMartialArts.DoThreadSafeAsync(x =>
                                {
                                    // ReSharper disable once AssignNullToNotNullAttribute
                                    x.Nodes.Insert(0, objMartialArtsParentNode);
                                    objMartialArtsParentNode.Expand();
                                }, token).ConfigureAwait(false);
                            }

                            objParentNode = objMartialArtsParentNode;
                        }

                        await treMartialArts.DoThreadSafeAsync(x =>
                        {
                            if (objParentNode == null)
                                return;
                            if (blnSingleAdd)
                            {
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
                                x.SelectedNode = objNode;
                            }
                            else
                                objParentNode.Nodes.Add(objNode);

                            objParentNode.Expand();
                        }, token).ConfigureAwait(false);
                    }
                }
                finally
                {
                    SkipUpdate = false;
                }
            }
            finally
            {
                await objCursorWait.DisposeAsync().ConfigureAwait(false);
            }
        }

        private async Task RefreshMartialArtTechniques(TreeView treMartialArts, MartialArt objMartialArt, ContextMenuStrip cmsTechnique, NotifyCollectionChangedEventArgs e, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (treMartialArts == null || objMartialArt == null || e == null)
                return;
            TreeNode nodMartialArt = await treMartialArts.DoThreadSafeFuncAsync(x => x.FindNodeByTag(objMartialArt), token).ConfigureAwait(false);
            if (nodMartialArt == null)
                return;
            CursorWait objCursorWait = await CursorWait.NewAsync(this, token: token).ConfigureAwait(false);
            try
            {
                SkipUpdate = true;
                try
                {
                    switch (e.Action)
                    {
                        case NotifyCollectionChangedAction.Add:
                        {
                            await treMartialArts.DoThreadSafeAsync(() =>
                            {
                                foreach (MartialArtTechnique objTechnique in e.NewItems)
                                {
                                    AddToTree(objTechnique);
                                }
                            }, token).ConfigureAwait(false);
                        }
                            break;

                        case NotifyCollectionChangedAction.Remove:
                        {
                            await treMartialArts.DoThreadSafeAsync(() =>
                            {
                                foreach (MartialArtTechnique objTechnique in e.OldItems)
                                {
                                    nodMartialArt.FindNodeByTag(objTechnique)?.Remove();
                                }
                            }, token).ConfigureAwait(false);
                        }
                            break;

                        case NotifyCollectionChangedAction.Replace:
                        {
                            await treMartialArts.DoThreadSafeAsync(() =>
                            {
                                foreach (MartialArtTechnique objTechnique in e.OldItems)
                                {
                                    nodMartialArt.FindNodeByTag(objTechnique)?.Remove();
                                }

                                foreach (MartialArtTechnique objTechnique in e.NewItems)
                                {
                                    AddToTree(objTechnique);
                                }
                            }, token).ConfigureAwait(false);
                        }
                            break;

                        case NotifyCollectionChangedAction.Reset:
                        {
                            await treMartialArts.DoThreadSafeAsync(x =>
                            {
                                string strSelectedId =
                                    (x.SelectedNode?.Tag as IHasInternalId)?.InternalId ?? string.Empty;

                                nodMartialArt.Nodes.Clear();

                                foreach (MartialArtTechnique objTechnique in objMartialArt.Techniques)
                                {
                                    AddToTree(objTechnique, false);
                                }

                                x.SortCustomAlphabetically(strSelectedId);
                            }, token).ConfigureAwait(false);
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
                finally
                {
                    SkipUpdate = false;
                }
            }
            finally
            {
                await objCursorWait.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Refresh the list of Improvements.
        /// </summary>
        protected async Task RefreshCustomImprovements(TreeView treImprovements, TreeView treLimit, ContextMenuStrip cmsImprovementLocation, ContextMenuStrip cmsImprovement, ContextMenuStrip cmsLimitModifier, NotifyCollectionChangedEventArgs e = null, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (treImprovements == null)
                return;
            CursorWait objCursorWait = await CursorWait.NewAsync(this, token: token).ConfigureAwait(false);
            try
            {
                SkipUpdate = true;
                try
                {
                    string strSelectedId =
                        (await treImprovements.DoThreadSafeFuncAsync(x => x.SelectedNode?.Tag as IHasInternalId,
                            token).ConfigureAwait(false))?.InternalId ?? string.Empty;

                    TreeNode objRoot;

                    if (e == null ||
                        e.Action == NotifyCollectionChangedAction.Reset)
                    {
                        await treImprovements.DoThreadSafeAsync(x => x.SuspendLayout(), token).ConfigureAwait(false);
                        try
                        {
                            await treImprovements.DoThreadSafeAsync(x => x.Nodes.Clear(), token).ConfigureAwait(false);

                            objRoot = new TreeNode
                            {
                                Tag = "Node_SelectedImprovements",
                                Text = await LanguageManager.GetStringAsync("Node_SelectedImprovements", token: token)
                                    .ConfigureAwait(false)
                            };
                            await treImprovements.DoThreadSafeAsync(x => x.Nodes.Add(objRoot), token)
                                .ConfigureAwait(false);

                            // Add the Locations.
                            await CharacterObject.ImprovementGroups.ForEachAsync(strGroup =>
                            {
                                TreeNode objGroup = new TreeNode
                                {
                                    Tag = strGroup,
                                    Text = strGroup,
                                    ContextMenuStrip = cmsImprovementLocation
                                };
                                return treImprovements.DoThreadSafeAsync(x => x.Nodes.Add(objGroup), token);
                            }, token).ConfigureAwait(false);

                            await CharacterObject.Improvements.ForEachAsync(objImprovement =>
                            {
                                if (objImprovement.ImproveSource == Improvement.ImprovementSource.Custom ||
                                    objImprovement.ImproveSource == Improvement.ImprovementSource.Drug)
                                {
                                    return AddToTree(objImprovement, false);
                                }

                                return Task.CompletedTask;
                            }, token).ConfigureAwait(false);

                            // Sort the list of Custom Improvements in alphabetical order based on their Custom Name within each Group.
                            await treImprovements.DoThreadSafeAsync(x => x.SortCustomAlphabetically(strSelectedId),
                                token).ConfigureAwait(false);
                        }
                        finally
                        {
                            await treImprovements.DoThreadSafeAsync(x => x.ResumeLayout(), GenericToken)
                                .ConfigureAwait(false);
                        }
                    }
                    else
                    {
                        objRoot = await treImprovements.DoThreadSafeFuncAsync(
                            x => x.FindNode("Node_SelectedImprovements", false), token).ConfigureAwait(false);
                        TreeNode[] aobjLimitNodes = new TreeNode[4];
                        if (treLimit != null)
                            await treLimit.DoThreadSafeAsync(x =>
                            {
                                aobjLimitNodes[0] = x.FindNode("Node_Physical", false);
                                aobjLimitNodes[1] = x.FindNode("Node_Mental", false);
                                aobjLimitNodes[2] = x.FindNode("Node_Social", false);
                                aobjLimitNodes[3] = x.FindNode("Node_Astral", false);
                            }, token).ConfigureAwait(false);

                        switch (e.Action)
                        {
                            case NotifyCollectionChangedAction.Add:
                            {
                                foreach (Improvement objImprovement in e.NewItems)
                                {
                                    if (objImprovement.ImproveSource == Improvement.ImprovementSource.Custom ||
                                        objImprovement.ImproveSource == Improvement.ImprovementSource.Drug)
                                    {
                                        await AddToTree(objImprovement).ConfigureAwait(false);
                                        await AddToLimitTree(objImprovement).ConfigureAwait(false);
                                    }
                                }

                                break;
                            }
                            case NotifyCollectionChangedAction.Remove:
                            {
                                await treImprovements.DoThreadSafeAsync(x =>
                                {
                                    foreach (Improvement objImprovement in e.OldItems)
                                    {
                                        if (objImprovement.ImproveSource == Improvement.ImprovementSource.Custom ||
                                            objImprovement.ImproveSource == Improvement.ImprovementSource.Drug)
                                        {
                                            TreeNode objNode = x.FindNodeByTag(objImprovement);
                                            if (objNode != null)
                                            {
                                                TreeNode objParent = objNode.Parent;
                                                objNode.Remove();
                                                if (objParent.Tag.ToString() == "Node_SelectedImprovements" &&
                                                    objParent.Nodes.Count == 0)
                                                    objParent.Remove();
                                            }

                                            treLimit?.DoThreadSafe(y =>
                                            {
                                                objNode = y.FindNodeByTag(objImprovement);
                                                if (objNode != null)
                                                {
                                                    TreeNode objParent = objNode.Parent;
                                                    objNode.Remove();
                                                    if (objParent.Level == 0 && objParent.Nodes.Count == 0)
                                                        objParent.Remove();
                                                }
                                            }, token);
                                        }
                                    }
                                }, token).ConfigureAwait(false);

                                break;
                            }
                            case NotifyCollectionChangedAction.Replace:
                            {
                                List<TreeNode> lstOldParents =
                                    new List<TreeNode>(e.OldItems.Count);
                                await treImprovements.DoThreadSafeAsync(x =>
                                {
                                    foreach (Improvement objImprovement in e.OldItems)
                                    {
                                        if (objImprovement.ImproveSource == Improvement.ImprovementSource.Custom ||
                                            objImprovement.ImproveSource == Improvement.ImprovementSource.Drug)
                                        {
                                            TreeNode objNode = x.FindNodeByTag(objImprovement);
                                            if (objNode != null)
                                            {
                                                lstOldParents.Add(objNode.Parent);
                                                objNode.Remove();
                                            }

                                            treLimit?.DoThreadSafe(y =>
                                            {
                                                objNode = y.FindNodeByTag(objImprovement);
                                                if (objNode != null)
                                                {
                                                    lstOldParents.Add(objNode.Parent);
                                                    objNode.Remove();
                                                }
                                            }, token);
                                        }
                                    }
                                }, token).ConfigureAwait(false);

                                foreach (Improvement objImprovement in e.NewItems)
                                {
                                    if (objImprovement.ImproveSource == Improvement.ImprovementSource.Custom ||
                                        objImprovement.ImproveSource == Improvement.ImprovementSource.Drug)
                                    {
                                        await AddToTree(objImprovement).ConfigureAwait(false);
                                        await AddToLimitTree(objImprovement).ConfigureAwait(false);
                                    }
                                }

                                await treImprovements.DoThreadSafeAsync(() =>
                                {
                                    foreach (TreeNode objOldParent in lstOldParents)
                                    {
                                        if (objOldParent.Level == 0 && objOldParent.Nodes.Count == 0)
                                            objOldParent.Remove();
                                    }
                                }, token).ConfigureAwait(false);

                                break;
                            }
                        }

                        async ValueTask AddToLimitTree(Improvement objImprovement)
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
                                                Text = await LanguageManager
                                                    .GetStringAsync("Node_Physical", token: token).ConfigureAwait(false)
                                            };
                                            await treLimit.DoThreadSafeAsync(
                                                x => x.Nodes.Insert(0, objParentNode), token).ConfigureAwait(false);
                                            break;

                                        case 1:
                                            objParentNode = new TreeNode
                                            {
                                                Tag = "Node_Mental",
                                                Text = await LanguageManager.GetStringAsync("Node_Mental", token: token)
                                                    .ConfigureAwait(false)
                                            };
                                            await treLimit.DoThreadSafeAsync(
                                                x => x.Nodes.Insert((aobjLimitNodes[0] != null).ToInt32(),
                                                    objParentNode),
                                                token).ConfigureAwait(false);
                                            break;

                                        case 2:
                                            objParentNode = new TreeNode
                                            {
                                                Tag = "Node_Social",
                                                Text = await LanguageManager.GetStringAsync("Node_Social", token: token)
                                                    .ConfigureAwait(false)
                                            };
                                            await treLimit.DoThreadSafeAsync(x => x.Nodes.Insert(
                                                (aobjLimitNodes[0] != null).ToInt32()
                                                + (aobjLimitNodes[1] != null).ToInt32(),
                                                objParentNode), token).ConfigureAwait(false);
                                            break;

                                        case 3:
                                            objParentNode = new TreeNode
                                            {
                                                Tag = "Node_Astral",
                                                Text = await LanguageManager.GetStringAsync("Node_Astral", token: token)
                                                    .ConfigureAwait(false)
                                            };
                                            await treLimit.DoThreadSafeAsync(x => x.Nodes.Add(objParentNode), token)
                                                .ConfigureAwait(false);
                                            break;
                                    }

                                    if (objParentNode != null)
                                        await treLimit.DoThreadSafeAsync(() => objParentNode.Expand(), token)
                                            .ConfigureAwait(false);
                                }

                                string strName = objImprovement.UniqueName
                                                 + await LanguageManager.GetStringAsync("String_Colon", token: token)
                                                     .ConfigureAwait(false) +
                                                 await LanguageManager.GetStringAsync("String_Space", token: token)
                                                     .ConfigureAwait(false);
                                if (objImprovement.Value > 0)
                                    strName += '+';
                                strName += objImprovement.Value.ToString(GlobalSettings.CultureInfo);
                                if (!string.IsNullOrEmpty(objImprovement.Condition))
                                    strName += ',' + await LanguageManager.GetStringAsync("String_Space", token: token)
                                                       .ConfigureAwait(false)
                                                   + objImprovement.Condition;
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

                                    await treLimit.DoThreadSafeAsync(x =>
                                    {
                                        TreeNodeCollection lstParentNodeChildren = objParentNode.Nodes;
                                        int intNodesCount = lstParentNodeChildren.Count;
                                        int intTargetIndex = 0;
                                        for (; intTargetIndex < intNodesCount; ++intTargetIndex)
                                        {
                                            if (CompareTreeNodes.CompareText(lstParentNodeChildren[intTargetIndex],
                                                    objNode)
                                                >=
                                                0)
                                            {
                                                break;
                                            }
                                        }

                                        lstParentNodeChildren.Insert(intTargetIndex, objNode);
                                        x.SelectedNode = objNode;
                                    }, token).ConfigureAwait(false);
                                }
                            }
                        }
                    }

                    async Task AddToTree(Improvement objImprovement, bool blnSingleAdd = true)
                    {
                        TreeNode objNode = objImprovement.CreateTreeNode(cmsImprovement);

                        TreeNode objParentNode = objRoot;
                        if (!string.IsNullOrEmpty(objImprovement.CustomGroup))
                        {
                            await treImprovements.DoThreadSafeAsync(x =>
                            {
                                foreach (TreeNode objFind in x.Nodes)
                                {
                                    if (objFind.Text == objImprovement.CustomGroup)
                                    {
                                        objParentNode = objFind;
                                        break;
                                    }
                                }
                            }, token).ConfigureAwait(false);
                        }
                        else
                        {
                            if (objParentNode == null)
                            {
                                objParentNode = new TreeNode
                                {
                                    Tag = "Node_SelectedImprovements",
                                    Text = await LanguageManager
                                        .GetStringAsync("Node_SelectedImprovements", token: token).ConfigureAwait(false)
                                };
                                await treImprovements.DoThreadSafeAsync(x => x.Nodes.Add(objParentNode), token)
                                    .ConfigureAwait(false);
                            }
                        }

                        await treImprovements.DoThreadSafeAsync(x =>
                        {
                            if (blnSingleAdd)
                            {
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
                                x.SelectedNode = objNode;
                            }
                            else
                                objParentNode.Nodes.Add(objNode);

                            objParentNode.Expand();
                        }, token).ConfigureAwait(false);
                    }
                }
                finally
                {
                    SkipUpdate = false;
                }
            }
            finally
            {
                await objCursorWait.DisposeAsync().ConfigureAwait(false);
            }
        }

        protected async Task RefreshLifestyles(TreeView treLifestyles, ContextMenuStrip cmsBasicLifestyle,
                                         ContextMenuStrip cmsAdvancedLifestyle, NotifyCollectionChangedEventArgs e = null, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (treLifestyles == null)
                return;
            CursorWait objCursorWait = await CursorWait.NewAsync(this, token: token).ConfigureAwait(false);
            try
            {
                SkipUpdate = true;
                try
                {
                    string strSelectedId
                        = (await treLifestyles.DoThreadSafeFuncAsync(x => x.SelectedNode?.Tag, token)
                                .ConfigureAwait(false) as
                            IHasInternalId)?.InternalId ?? string.Empty;
                    TreeNode objParentNode = null;

                    if (e == null || e.Action == NotifyCollectionChangedAction.Reset)
                    {
                        await treLifestyles.DoThreadSafeAsync(x => x.SuspendLayout(), token).ConfigureAwait(false);
                        try
                        {
                            await treLifestyles.DoThreadSafeAsync(x => x.Nodes.Clear(), token).ConfigureAwait(false);

                            if (await CharacterObject.Lifestyles.GetCountAsync(token).ConfigureAwait(false) > 0)
                            {
                                await CharacterObject.Lifestyles
                                    .ForEachAsync(objLifestyle => AddToTree(objLifestyle, false),
                                        token).ConfigureAwait(false);

                                await treLifestyles.DoThreadSafeAsync(x => x.SortCustomAlphabetically(strSelectedId),
                                    token).ConfigureAwait(false);
                            }
                        }
                        finally
                        {
                            await treLifestyles.DoThreadSafeAsync(x => x.ResumeLayout(), GenericToken)
                                .ConfigureAwait(false);
                        }
                    }
                    else
                    {
                        objParentNode
                            = await treLifestyles.DoThreadSafeFuncAsync(
                                x => x.FindNode("Node_SelectedLifestyles", false),
                                token).ConfigureAwait(false);
                        switch (e.Action)
                        {
                            case NotifyCollectionChangedAction.Add:
                            {
                                foreach (Lifestyle objLifestyle in e.NewItems)
                                {
                                    await AddToTree(objLifestyle).ConfigureAwait(false);
                                }

                                break;
                            }
                            case NotifyCollectionChangedAction.Remove:
                            {
                                await treLifestyles.DoThreadSafeAsync(x =>
                                {
                                    foreach (Lifestyle objLifestyle in e.OldItems)
                                    {
                                        TreeNode objNode = x.FindNodeByTag(objLifestyle);
                                        if (objNode != null)
                                        {
                                            TreeNode objParent = objNode.Parent;
                                            objNode.Remove();
                                            if (objParent.Level == 0 && objParent.Nodes.Count == 0)
                                                objParent.Remove();
                                        }
                                    }
                                }, token).ConfigureAwait(false);

                                break;
                            }
                            case NotifyCollectionChangedAction.Replace:
                            {
                                HashSet<TreeNode> setOldParentNodes = new HashSet<TreeNode>();
                                await treLifestyles.DoThreadSafeAsync(x =>
                                {
                                    foreach (Lifestyle objLifestyle in e.OldItems)
                                    {
                                        TreeNode objNode = x.FindNodeByTag(objLifestyle);
                                        if (objNode != null)
                                        {
                                            setOldParentNodes.Add(objNode.Parent);
                                            objNode.Remove();
                                        }
                                    }
                                }, token).ConfigureAwait(false);

                                foreach (Lifestyle objLifestyle in e.NewItems)
                                {
                                    await AddToTree(objLifestyle).ConfigureAwait(false);
                                }

                                await treLifestyles.DoThreadSafeAsync(() =>
                                {
                                    foreach (TreeNode nodOldParent in setOldParentNodes)
                                    {
                                        if (nodOldParent.Level == 0 && nodOldParent.Nodes.Count == 0)
                                            nodOldParent.Remove();
                                    }
                                }, token).ConfigureAwait(false);

                                break;
                            }
                        }
                    }

                    async Task AddToTree(Lifestyle objLifestyle, bool blnSingleAdd = true)
                    {
                        TreeNode objNode = objLifestyle.CreateTreeNode(cmsBasicLifestyle, cmsAdvancedLifestyle);
                        if (objNode == null)
                            return;

                        if (objParentNode == null)
                        {
                            objParentNode = new TreeNode
                            {
                                Tag = "Node_SelectedLifestyles",
                                Text = await LanguageManager.GetStringAsync("Node_SelectedLifestyles", token: token)
                                    .ConfigureAwait(false)
                            };
                            await treLifestyles.DoThreadSafeAsync(x =>
                            {
                                // ReSharper disable once AssignNullToNotNullAttribute
                                x.Nodes.Add(objParentNode);
                                objParentNode.Expand();
                            }, token).ConfigureAwait(false);
                        }

                        await treLifestyles.DoThreadSafeAsync(x =>
                        {
                            if (objParentNode == null)
                                return;
                            if (blnSingleAdd)
                            {
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
                                x.SelectedNode = objNode;
                            }
                            else
                                objParentNode.Nodes.Add(objNode);
                        }, token).ConfigureAwait(false);
                    }
                }
                finally
                {
                    SkipUpdate = false;
                }
            }
            finally
            {
                await objCursorWait.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Refresh the Calendar List.
        /// </summary>
        public async Task RefreshCalendar(ListView lstCalendar, ListChangedEventArgs listChangedEventArgs = null, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (lstCalendar == null)
                return;
            CursorWait objCursorWait = await CursorWait.NewAsync(this, token: token).ConfigureAwait(false);
            try
            {
                SkipUpdate = true;
                try
                {
                    if (listChangedEventArgs == null || listChangedEventArgs.ListChangedType == ListChangedType.Reset)
                    {
                        await lstCalendar.DoThreadSafeAsync(x => x.SuspendLayout(), token).ConfigureAwait(false);
                        try
                        {
                            await lstCalendar.DoThreadSafeAsync(x => x.Items.Clear(), token).ConfigureAwait(false);
                            await CharacterObject.Calendar.ForEachAsync(async objWeek =>
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
                                    Text = await objWeek.GetCurrentDisplayNameAsync(token).ConfigureAwait(false),
                                    ForeColor = objWeek.PreferredColor
                                };
                                objItem.SubItems.Add(objNoteItem);
                                objItem.SubItems.Add(objInternalIdItem);

                                await lstCalendar.DoThreadSafeAsync(x => x.Items.Add(objItem), token)
                                    .ConfigureAwait(false);
                            }, token).ConfigureAwait(false);
                        }
                        finally
                        {
                            await lstCalendar.DoThreadSafeAsync(x => x.ResumeLayout(), GenericToken)
                                .ConfigureAwait(false);
                        }
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
                                    Text = await objWeek.GetCurrentDisplayNameAsync(token).ConfigureAwait(false),
                                    ForeColor = objWeek.PreferredColor
                                };
                                objItem.SubItems.Add(objNoteItem);
                                objItem.SubItems.Add(objInternalIdItem);

                                await lstCalendar.DoThreadSafeAsync(x => x.Items.Insert(intInsertIndex, objItem),
                                    token).ConfigureAwait(false);
                            }
                                break;

                            case ListChangedType.ItemDeleted:
                            {
                                await lstCalendar.DoThreadSafeAsync(
                                    x => x.Items.RemoveAt(listChangedEventArgs.NewIndex),
                                    token).ConfigureAwait(false);
                            }
                                break;

                            case ListChangedType.ItemChanged:
                            {
                                await lstCalendar.DoThreadSafeAsync(
                                    x => x.Items.RemoveAt(listChangedEventArgs.NewIndex),
                                    token).ConfigureAwait(false);
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
                                    Text = await objWeek.GetCurrentDisplayNameAsync(token).ConfigureAwait(false),
                                    ForeColor = objWeek.PreferredColor
                                };
                                objItem.SubItems.Add(objNoteItem);
                                objItem.SubItems.Add(objInternalIdItem);

                                await lstCalendar.DoThreadSafeAsync(x => x.Items.Insert(intInsertIndex, objItem),
                                    token).ConfigureAwait(false);
                            }
                                break;

                            case ListChangedType.ItemMoved:
                            {
                                await lstCalendar.DoThreadSafeAsync(
                                    x => x.Items.RemoveAt(listChangedEventArgs.OldIndex),
                                    token).ConfigureAwait(false);
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
                                    Text = await objWeek.GetCurrentDisplayNameAsync(token).ConfigureAwait(false),
                                    ForeColor = objWeek.PreferredColor
                                };
                                objItem.SubItems.Add(objNoteItem);
                                objItem.SubItems.Add(objInternalIdItem);

                                await lstCalendar.DoThreadSafeAsync(x => x.Items.Insert(intInsertIndex, objItem),
                                    token).ConfigureAwait(false);
                            }
                                break;
                        }
                    }
                }
                finally
                {
                    SkipUpdate = false;
                }
            }
            finally
            {
                await objCursorWait.DisposeAsync().ConfigureAwait(false);
            }
        }

        public async Task RefreshContactsClearBindings(FlowLayoutPanel panContacts, FlowLayoutPanel panEnemies,
            FlowLayoutPanel panPets, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            CursorWait objCursorWait = await CursorWait.NewAsync(this, token: token).ConfigureAwait(false);
            try
            {
                SkipUpdate = true;
                try
                {
                    if (panContacts != null)
                    {
                        await panContacts.DoThreadSafeAsync(x =>
                        {
                            x.SuspendLayout();
                            try
                            {
                                for (int i = x.Controls.Count - 1; i >= 0; --i)
                                {
                                    token.ThrowIfCancellationRequested();
                                    if (!(x.Controls[i] is ContactControl objContactControl))
                                        continue;
                                    objContactControl.ContactDetailChanged -= MakeDirtyWithCharacterUpdate;
                                    objContactControl.DeleteContact -= DeleteContact;
                                    objContactControl.MouseDown -= DragContactControl;
                                }
                            }
                            finally
                            {
                                x.ResumeLayout();
                            }
                        }, token).ConfigureAwait(false);
                    }

                    if (panEnemies != null)
                    {
                        await panEnemies.DoThreadSafeAsync(x =>
                        {
                            x.SuspendLayout();
                            try
                            {
                                for (int i = x.Controls.Count - 1; i >= 0; --i)
                                {
                                    token.ThrowIfCancellationRequested();
                                    if (!(x.Controls[i] is ContactControl objContactControl))
                                        continue;
                                    objContactControl.ContactDetailChanged -= MakeDirtyWithCharacterUpdate;
                                    objContactControl.DeleteContact -= DeleteEnemy;
                                    objContactControl.MouseDown -= DragContactControl;
                                }
                            }
                            finally
                            {
                                x.ResumeLayout();
                            }
                        }, token).ConfigureAwait(false);
                    }

                    if (panPets != null)
                    {
                        await panPets.DoThreadSafeAsync(x =>
                        {
                            x.SuspendLayout();
                            try
                            {
                                for (int i = x.Controls.Count - 1; i >= 0; --i)
                                {
                                    token.ThrowIfCancellationRequested();
                                    if (!(x.Controls[i] is PetControl objPetControl))
                                        continue;
                                    objPetControl.ContactDetailChanged -= MakeDirtyWithCharacterUpdate;
                                    objPetControl.DeleteContact -= DeletePet;
                                    objPetControl.MouseDown -= DragContactControl;
                                }
                            }
                            finally
                            {
                                x.ResumeLayout();
                            }
                        }, token).ConfigureAwait(false);
                    }
                }
                finally
                {
                    SkipUpdate = false;
                }
            }
            finally
            {
                await objCursorWait.DisposeAsync().ConfigureAwait(false);
            }
        }

        public async Task RefreshContacts(FlowLayoutPanel panContacts, FlowLayoutPanel panEnemies, FlowLayoutPanel panPets, NotifyCollectionChangedEventArgs e = null, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (panContacts == null && panEnemies == null && panPets == null)
                return;

            CursorWait objCursorWait = await CursorWait.NewAsync(this, token: token).ConfigureAwait(false);
            try
            {
                SkipUpdate = true;
                try
                {
                    if (e == null ||
                        e.Action == NotifyCollectionChangedAction.Reset)
                    {
                        if (panContacts != null)
                            await panContacts.DoThreadSafeAsync(x => x.SuspendLayout(), token).ConfigureAwait(false);
                        if (panEnemies != null)
                            await panEnemies.DoThreadSafeAsync(x => x.SuspendLayout(), token).ConfigureAwait(false);
                        if (panPets != null)
                            await panPets.DoThreadSafeAsync(x => x.SuspendLayout(), token).ConfigureAwait(false);
                        try
                        {
                            if (panContacts != null)
                                await panContacts.DoThreadSafeAsync(x => x.Controls.Clear(), token)
                                    .ConfigureAwait(false);
                            if (panEnemies != null)
                                await panEnemies.DoThreadSafeAsync(x => x.Controls.Clear(), token)
                                    .ConfigureAwait(false);
                            if (panPets != null)
                                await panPets.DoThreadSafeAsync(x => x.Controls.Clear(), token).ConfigureAwait(false);
                            await CharacterObject.Contacts.ForEachAsync(objContact =>
                            {
                                switch (objContact.EntityType)
                                {
                                    case ContactType.Contact:
                                    {
                                        if (panContacts == null)
                                            break;
                                        return this.DoThreadSafeAsync(() =>
                                        {
                                            ContactControl objContactControl =
                                                new ContactControl(objContact, GenericToken);
                                            // Attach an EventHandler for the ConnectionRatingChanged, LoyaltyRatingChanged, DeleteContact, FileNameChanged Events and OtherCostChanged
                                            objContactControl.ContactDetailChanged += MakeDirtyWithCharacterUpdate;
                                            objContactControl.DeleteContact += DeleteContact;
                                            objContactControl.MouseDown += DragContactControl;

                                            panContacts.Controls.Add(objContactControl);
                                        }, token);
                                    }

                                    case ContactType.Enemy:
                                    {
                                        if (panEnemies == null || !CharacterObjectSettings.EnableEnemyTracking)
                                            break;
                                        return this.DoThreadSafeAsync(() =>
                                        {
                                            ContactControl objContactControl =
                                                new ContactControl(objContact, GenericToken);
                                            // Attach an EventHandler for the ConnectionRatingChanged, LoyaltyRatingChanged, DeleteContact, FileNameChanged Events and OtherCostChanged
                                            objContactControl.ContactDetailChanged += MakeDirtyWithCharacterUpdate;
                                            objContactControl.DeleteContact += DeleteEnemy;
                                            objContactControl.MouseDown += DragContactControl;

                                            panEnemies.Controls.Add(objContactControl);
                                        }, token);
                                    }

                                    case ContactType.Pet:
                                    {
                                        if (panPets == null)
                                            break;
                                        return this.DoThreadSafeAsync(() =>
                                        {
                                            PetControl objContactControl = new PetControl(objContact, GenericToken);
                                            // Attach an EventHandler for the ConnectionRatingChanged, LoyaltyRatingChanged, DeleteContact, FileNameChanged Events and OtherCostChanged
                                            objContactControl.ContactDetailChanged += MakeDirtyWithCharacterUpdate;
                                            objContactControl.DeleteContact += DeletePet;
                                            objContactControl.MouseDown += DragContactControl;

                                            panPets.Controls.Add(objContactControl);
                                        }, token);
                                    }
                                }

                                return Task.CompletedTask;
                            }, GenericToken).ConfigureAwait(false);
                        }
                        finally
                        {
                            if (panContacts != null)
                                await panContacts.DoThreadSafeAsync(x => x.ResumeLayout(), GenericToken)
                                    .ConfigureAwait(false);
                            if (panEnemies != null)
                                await panEnemies.DoThreadSafeAsync(x => x.ResumeLayout(), GenericToken)
                                    .ConfigureAwait(false);
                            if (panPets != null)
                                await panPets.DoThreadSafeAsync(x => x.ResumeLayout(), GenericToken)
                                    .ConfigureAwait(false);
                        }
                    }
                    else
                    {
                        switch (e.Action)
                        {
                            case NotifyCollectionChangedAction.Add:
                            {
                                foreach (Contact objContact in e.NewItems)
                                {
                                    switch (objContact.EntityType)
                                    {
                                        case ContactType.Contact:
                                        {
                                            if (panContacts == null)
                                                break;
                                            await panContacts.DoThreadSafeAsync(x =>
                                            {
                                                ContactControl objContactControl =
                                                    new ContactControl(objContact, GenericToken);
                                                // Attach an EventHandler for the ConnectionRatingChanged, LoyaltyRatingChanged, DeleteContact, FileNameChanged Events and OtherCostChanged
                                                objContactControl.ContactDetailChanged += MakeDirtyWithCharacterUpdate;
                                                objContactControl.DeleteContact += DeleteContact;
                                                objContactControl.MouseDown += DragContactControl;

                                                x.Controls.Add(objContactControl);
                                            }, token).ConfigureAwait(false);
                                        }
                                            break;

                                        case ContactType.Enemy:
                                        {
                                            if (panEnemies == null || !CharacterObjectSettings.EnableEnemyTracking)
                                                break;
                                            await panEnemies.DoThreadSafeAsync(x =>
                                            {
                                                ContactControl objContactControl =
                                                    new ContactControl(objContact, GenericToken);
                                                // Attach an EventHandler for the ConnectionRatingChanged, LoyaltyRatingChanged, DeleteContact, FileNameChanged Events and OtherCostChanged
                                                objContactControl.ContactDetailChanged += MakeDirtyWithCharacterUpdate;
                                                objContactControl.DeleteContact += DeleteEnemy;
                                                objContactControl.MouseDown += DragContactControl;

                                                x.Controls.Add(objContactControl);
                                            }, token).ConfigureAwait(false);
                                        }
                                            break;

                                        case ContactType.Pet:
                                        {
                                            if (panPets == null)
                                                break;
                                            await panPets.DoThreadSafeAsync(x =>
                                            {
                                                PetControl objContactControl = new PetControl(objContact, GenericToken);
                                                // Attach an EventHandler for the ConnectionRatingChanged, LoyaltyRatingChanged, DeleteContact, FileNameChanged Events and OtherCostChanged
                                                objContactControl.ContactDetailChanged += MakeDirtyWithCharacterUpdate;
                                                objContactControl.DeleteContact += DeletePet;
                                                objContactControl.MouseDown += DragContactControl;

                                                x.Controls.Add(objContactControl);
                                            }, token).ConfigureAwait(false);
                                        }
                                            break;
                                    }
                                }
                            }
                                break;

                            case NotifyCollectionChangedAction.Remove:
                            {
                                foreach (Contact objContact in e.OldItems)
                                {
                                    switch (objContact.EntityType)
                                    {
                                        case ContactType.Contact:
                                        {
                                            if (panContacts == null)
                                                break;
                                            await panContacts.DoThreadSafeAsync(x =>
                                            {
                                                for (int i = x.Controls.Count - 1; i >= 0; i--)
                                                {
                                                    if (x.Controls[i] is ContactControl objContactControl &&
                                                        objContactControl.ContactObject == objContact)
                                                    {
                                                        objContactControl.ContactDetailChanged
                                                            -= MakeDirtyWithCharacterUpdate;
                                                        objContactControl.DeleteContact -= DeleteContact;
                                                        objContactControl.MouseDown -= DragContactControl;
                                                        x.Controls.RemoveAt(i);
                                                    }
                                                }
                                            }, token).ConfigureAwait(false);
                                        }
                                            break;

                                        case ContactType.Enemy:
                                        {
                                            if (panEnemies == null)
                                                break;
                                            await panEnemies.DoThreadSafeAsync(x =>
                                            {
                                                for (int i = x.Controls.Count - 1; i >= 0; i--)
                                                {
                                                    if (x.Controls[i] is ContactControl objContactControl
                                                        && objContactControl.ContactObject == objContact)
                                                    {
                                                        objContactControl.ContactDetailChanged
                                                            -= MakeDirtyWithCharacterUpdate;
                                                        objContactControl.DeleteContact -= DeleteEnemy;
                                                        x.Controls.RemoveAt(i);
                                                    }
                                                }
                                            }, token).ConfigureAwait(false);
                                        }
                                            break;

                                        case ContactType.Pet:
                                        {
                                            if (panPets == null)
                                                break;
                                            await panPets.DoThreadSafeAsync(x =>
                                            {
                                                for (int i = x.Controls.Count - 1; i >= 0; i--)
                                                {
                                                    if (x.Controls[i] is PetControl objPetControl &&
                                                        objPetControl.ContactObject == objContact)
                                                    {
                                                        objPetControl.ContactDetailChanged
                                                            -= MakeDirtyWithCharacterUpdate;
                                                        objPetControl.DeleteContact -= DeletePet;
                                                        x.Controls.RemoveAt(i);
                                                    }
                                                }
                                            }, token).ConfigureAwait(false);
                                        }
                                            break;
                                    }
                                }
                            }
                                break;

                            case NotifyCollectionChangedAction.Replace:
                            {
                                foreach (Contact objContact in e.OldItems)
                                {
                                    switch (objContact.EntityType)
                                    {
                                        case ContactType.Contact:
                                        {
                                            if (panContacts == null)
                                                break;
                                            await panContacts.DoThreadSafeAsync(x =>
                                            {
                                                for (int i = x.Controls.Count - 1; i >= 0; i--)
                                                {
                                                    if (x.Controls[i] is ContactControl objContactControl &&
                                                        objContactControl.ContactObject == objContact)
                                                    {
                                                        objContactControl.ContactDetailChanged
                                                            -= MakeDirtyWithCharacterUpdate;
                                                        objContactControl.DeleteContact -= DeleteContact;
                                                        objContactControl.MouseDown -= DragContactControl;
                                                        x.Controls.RemoveAt(i);
                                                    }
                                                }
                                            }, token).ConfigureAwait(false);
                                        }
                                            break;

                                        case ContactType.Enemy:
                                        {
                                            if (panEnemies == null)
                                                break;
                                            await panEnemies.DoThreadSafeAsync(x =>
                                            {
                                                for (int i = x.Controls.Count - 1; i >= 0; i--)
                                                {
                                                    if (x.Controls[i] is ContactControl objContactControl
                                                        && objContactControl.ContactObject == objContact)
                                                    {
                                                        objContactControl.ContactDetailChanged
                                                            -= MakeDirtyWithCharacterUpdate;
                                                        objContactControl.DeleteContact -= DeleteEnemy;
                                                        x.Controls.RemoveAt(i);
                                                    }
                                                }
                                            }, token).ConfigureAwait(false);
                                        }
                                            break;

                                        case ContactType.Pet:
                                        {
                                            if (panPets == null)
                                                break;
                                            await panPets.DoThreadSafeAsync(x =>
                                            {
                                                for (int i = x.Controls.Count - 1; i >= 0; i--)
                                                {
                                                    if (x.Controls[i] is PetControl objPetControl &&
                                                        objPetControl.ContactObject == objContact)
                                                    {
                                                        objPetControl.ContactDetailChanged
                                                            -= MakeDirtyWithCharacterUpdate;
                                                        objPetControl.DeleteContact -= DeletePet;
                                                        x.Controls.RemoveAt(i);
                                                    }
                                                }
                                            }, token).ConfigureAwait(false);
                                        }
                                            break;
                                    }
                                }

                                foreach (Contact objContact in e.NewItems)
                                {
                                    switch (objContact.EntityType)
                                    {
                                        case ContactType.Contact:
                                        {
                                            if (panContacts == null)
                                                break;
                                            await panContacts.DoThreadSafeAsync(x =>
                                            {
                                                ContactControl objContactControl =
                                                    new ContactControl(objContact, GenericToken);
                                                // Attach an EventHandler for the ConnectionRatingChanged, LoyaltyRatingChanged, DeleteContact, FileNameChanged Events and OtherCostChanged
                                                objContactControl.ContactDetailChanged += MakeDirtyWithCharacterUpdate;
                                                objContactControl.DeleteContact += DeleteContact;
                                                objContactControl.MouseDown += DragContactControl;

                                                x.Controls.Add(objContactControl);
                                            }, token).ConfigureAwait(false);
                                        }
                                            break;

                                        case ContactType.Enemy:
                                        {
                                            if (panEnemies == null || !CharacterObjectSettings.EnableEnemyTracking)
                                                break;
                                            await panEnemies.DoThreadSafeAsync(x =>
                                            {
                                                ContactControl objContactControl =
                                                    new ContactControl(objContact, GenericToken);
                                                // Attach an EventHandler for the ConnectionRatingChanged, LoyaltyRatingChanged, DeleteContact, FileNameChanged Events and OtherCostChanged
                                                objContactControl.ContactDetailChanged += MakeDirtyWithCharacterUpdate;
                                                objContactControl.DeleteContact += DeleteEnemy;
                                                objContactControl.MouseDown += DragContactControl;

                                                x.Controls.Add(objContactControl);
                                            }, token).ConfigureAwait(false);
                                        }
                                            break;

                                        case ContactType.Pet:
                                        {
                                            if (panPets == null)
                                                break;
                                            await panPets.DoThreadSafeAsync(x =>
                                            {
                                                PetControl objContactControl = new PetControl(objContact, GenericToken);
                                                // Attach an EventHandler for the ConnectionRatingChanged, LoyaltyRatingChanged, DeleteContact, FileNameChanged Events and OtherCostChanged
                                                objContactControl.ContactDetailChanged += MakeDirtyWithCharacterUpdate;
                                                objContactControl.DeleteContact += DeletePet;
                                                objContactControl.MouseDown += DragContactControl;

                                                x.Controls.Add(objContactControl);
                                            }, token).ConfigureAwait(false);
                                        }
                                            break;
                                    }
                                }
                            }
                                break;
                        }
                    }
                }
                finally
                {
                    SkipUpdate = false;
                }
            }
            finally
            {
                await objCursorWait.DisposeAsync().ConfigureAwait(false);
            }
        }

        public async Task RefreshSustainedSpellsClearBindings(Panel pnlSustainedSpells, Panel pnlSustainedComplexForms, Panel pnlSustainedCritterPowers, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            CursorWait objCursorWait = await CursorWait.NewAsync(this, token: token).ConfigureAwait(false);
            try
            {
                SkipUpdate = true;
                try
                {
                    if (pnlSustainedSpells != null)
                    {
                        await pnlSustainedSpells.DoThreadSafeAsync(x =>
                        {
                            x.SuspendLayout();
                            try
                            {
                                for (int i = x.Controls.Count - 1; i >= 0; --i)
                                {
                                    token.ThrowIfCancellationRequested();
                                    if (!(x.Controls[i] is SustainedObjectControl objSustainedObjectControl))
                                        continue;
                                    objSustainedObjectControl.SustainedObjectDetailChanged -=
                                        MakeDirtyWithCharacterUpdate;
                                    objSustainedObjectControl.UnsustainObject -= DeleteSustainedObject;
                                }
                            }
                            finally
                            {
                                x.ResumeLayout();
                            }
                        }, token).ConfigureAwait(false);
                    }

                    if (pnlSustainedComplexForms != null)
                    {
                        await pnlSustainedComplexForms.DoThreadSafeAsync(x =>
                        {
                            x.SuspendLayout();
                            try
                            {
                                for (int i = x.Controls.Count - 1; i >= 0; --i)
                                {
                                    token.ThrowIfCancellationRequested();
                                    if (!(x.Controls[i] is SustainedObjectControl objSustainedObjectControl))
                                        continue;
                                    objSustainedObjectControl.SustainedObjectDetailChanged -=
                                        MakeDirtyWithCharacterUpdate;
                                    objSustainedObjectControl.UnsustainObject -= DeleteSustainedObject;
                                }
                            }
                            finally
                            {
                                x.ResumeLayout();
                            }
                        }, token).ConfigureAwait(false);
                    }

                    if (pnlSustainedCritterPowers != null)
                    {
                        await pnlSustainedCritterPowers.DoThreadSafeAsync(x =>
                        {
                            x.SuspendLayout();
                            try
                            {
                                for (int i = x.Controls.Count - 1; i >= 0; --i)
                                {
                                    token.ThrowIfCancellationRequested();
                                    if (!(x.Controls[i] is SustainedObjectControl objSustainedObjectControl))
                                        continue;
                                    objSustainedObjectControl.SustainedObjectDetailChanged -=
                                        MakeDirtyWithCharacterUpdate;
                                    objSustainedObjectControl.UnsustainObject -= DeleteSustainedObject;
                                }
                            }
                            finally
                            {
                                x.ResumeLayout();
                            }
                        }, token).ConfigureAwait(false);
                    }
                }
                finally
                {
                    SkipUpdate = false;
                }
            }
            finally
            {
                await objCursorWait.DisposeAsync().ConfigureAwait(false);
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
        /// <param name="e"></param>
        /// <param name="token">Cancellation token to listen to.</param>
        public async Task RefreshSustainedSpells(Panel pnlSustainedSpells, Panel pnlSustainedComplexForms, Panel pnlSustainedCritterPowers, CheckBox chkPsycheActiveMagician, CheckBox chkPsycheActiveTechnomancer, NotifyCollectionChangedEventArgs e = null, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (pnlSustainedSpells == null && pnlSustainedComplexForms == null && pnlSustainedCritterPowers == null)
                return;

            CursorWait objCursorWait = await CursorWait.NewAsync(this, token: token).ConfigureAwait(false);
            try
            {
                SkipUpdate = true;
                try
                {
                    Panel DetermineRefreshingPanel(SustainedObject objSustained, Panel flpSustainedSpellsParam,
                        Panel flpSustainedComplexFormsParam,
                        Panel flpSustainedCritterPowersParam)
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

                    if (e == null ||
                        e.Action == NotifyCollectionChangedAction.Reset)
                    {
                        await chkPsycheActiveMagician.DoThreadSafeAsync(x =>
                        {
                            if (x != null)
                                x.Visible = false;
                        }, token).ConfigureAwait(false);
                        await chkPsycheActiveTechnomancer.DoThreadSafeAsync(x =>
                        {
                            if (x != null)
                                x.Visible = false;
                        }, token).ConfigureAwait(false);
                        await pnlSustainedSpells.DoThreadSafeAsync(x =>
                        {
                            if (x != null)
                            {
                                x.Controls.Clear();
                                x.Visible = false;
                            }
                        }, token).ConfigureAwait(false);
                        await pnlSustainedComplexForms.DoThreadSafeAsync(x =>
                        {
                            if (x != null)
                            {
                                x.Controls.Clear();
                                x.Visible = false;
                            }
                        }, token).ConfigureAwait(false);
                        await pnlSustainedCritterPowers.DoThreadSafeAsync(x =>
                        {
                            if (x != null)
                            {
                                x.Controls.Clear();
                                x.Visible = false;
                            }
                        }, token).ConfigureAwait(false);
                        await CharacterObject.SustainedCollection.ForEachAsync(objSustained =>
                        {
                            Panel refreshingPanel = DetermineRefreshingPanel(objSustained, pnlSustainedSpells,
                                pnlSustainedComplexForms,
                                pnlSustainedCritterPowers);

                            if (refreshingPanel == null)
                                return Task.CompletedTask;

                            return refreshingPanel.DoThreadSafeAsync(x =>
                            {
                                x.Visible = true;
                                switch (objSustained.LinkedObjectType)
                                {
                                    case Improvement.ImprovementSource.Spell:
                                        chkPsycheActiveMagician.DoThreadSafe(y =>
                                        {
                                            if (y != null)
                                                y.Visible = true;
                                        }, token);
                                        break;

                                    case Improvement.ImprovementSource.ComplexForm:
                                        chkPsycheActiveTechnomancer.DoThreadSafe(y =>
                                        {
                                            if (y != null)
                                                y.Visible = true;
                                        }, token);
                                        break;
                                }

                                int intSustainedObjects = x.Controls.Count;

                                SustainedObjectControl objSustainedObjectControl =
                                    new SustainedObjectControl(objSustained, GenericToken);

                                objSustainedObjectControl.SustainedObjectDetailChanged += MakeDirtyWithCharacterUpdate;
                                objSustainedObjectControl.UnsustainObject += DeleteSustainedObject;

                                objSustainedObjectControl.Top = intSustainedObjects * objSustainedObjectControl.Height;

                                x.Controls.Add(objSustainedObjectControl);
                            }, token);
                        }, token).ConfigureAwait(false);
                    }
                    else
                    {
                        switch (e.Action)
                        {
                            case NotifyCollectionChangedAction.Add:
                            {
                                foreach (SustainedObject objSustained in e.NewItems)
                                {
                                    Panel refreshingPanel = DetermineRefreshingPanel(objSustained, pnlSustainedSpells,
                                        pnlSustainedComplexForms, pnlSustainedCritterPowers);

                                    if (refreshingPanel == null)
                                        continue;

                                    await refreshingPanel.DoThreadSafeAsync(x =>
                                    {
                                        x.Visible = true;
                                        switch (objSustained.LinkedObjectType)
                                        {
                                            case Improvement.ImprovementSource.Spell:
                                                chkPsycheActiveMagician.DoThreadSafe(y =>
                                                {
                                                    if (y != null)
                                                        y.Visible = true;
                                                }, token);
                                                break;

                                            case Improvement.ImprovementSource.ComplexForm:
                                                chkPsycheActiveTechnomancer.DoThreadSafe(y =>
                                                {
                                                    if (y != null)
                                                        y.Visible = true;
                                                }, token);
                                                break;
                                        }

                                        int intSustainedObjects = x.Controls.Count;

                                        SustainedObjectControl objSustainedObjectControl
                                            = new SustainedObjectControl(objSustained, GenericToken);

                                        objSustainedObjectControl.SustainedObjectDetailChanged
                                            += MakeDirtyWithCharacterUpdate;
                                        objSustainedObjectControl.UnsustainObject += DeleteSustainedObject;

                                        objSustainedObjectControl.Top
                                            = intSustainedObjects * objSustainedObjectControl.Height;

                                        x.Controls.Add(objSustainedObjectControl);
                                    }, token).ConfigureAwait(false);
                                }
                            }
                                break;

                            case NotifyCollectionChangedAction.Remove:
                            {
                                foreach (SustainedObject objSustained in e.OldItems)
                                {
                                    Panel refreshingPanel = DetermineRefreshingPanel(objSustained, pnlSustainedSpells,
                                        pnlSustainedComplexForms, pnlSustainedCritterPowers);

                                    if (refreshingPanel == null)
                                        continue;

                                    int intMoveUpAmount = 0;
                                    await refreshingPanel.DoThreadSafeAsync(x =>
                                    {
                                        int intSustainedObjects = x.Controls.Count;

                                        for (int i = 0; i < intSustainedObjects; ++i)
                                        {
                                            Control objLoopControl = x.Controls[i];
                                            if (objLoopControl is SustainedObjectControl objSustainedSpellControl &&
                                                objSustainedSpellControl.LinkedSustainedObject == objSustained)
                                            {
                                                intMoveUpAmount = objSustainedSpellControl.Height;

                                                x.Controls.RemoveAt(i);

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
                                            x.Visible = false;
                                            if (x == pnlSustainedSpells)
                                            {
                                                chkPsycheActiveMagician.DoThreadSafe(y =>
                                                {
                                                    if (y != null)
                                                        y.Visible = false;
                                                }, token);
                                            }
                                            else if (x == pnlSustainedComplexForms)
                                            {
                                                chkPsycheActiveTechnomancer.DoThreadSafe(y =>
                                                {
                                                    if (y != null)
                                                        y.Visible = false;
                                                }, token);
                                            }
                                        }
                                    }, token).ConfigureAwait(false);
                                }
                            }
                                break;

                            case NotifyCollectionChangedAction.Replace:
                            {
                                foreach (SustainedObject objSustained in e.OldItems)
                                {
                                    Panel refreshingPanel = DetermineRefreshingPanel(objSustained, pnlSustainedSpells,
                                        pnlSustainedComplexForms, pnlSustainedCritterPowers);

                                    if (refreshingPanel == null)
                                        continue;

                                    int intMoveUpAmount = 0;
                                    await refreshingPanel.DoThreadSafeAsync(x =>
                                    {
                                        int intSustainedObjects = x.Controls.Count;

                                        for (int i = 0; i < intSustainedObjects; ++i)
                                        {
                                            Control objLoopControl = x.Controls[i];
                                            if (objLoopControl is SustainedObjectControl objSustainedSpellControl &&
                                                objSustainedSpellControl.LinkedSustainedObject == objSustained)
                                            {
                                                intMoveUpAmount = objSustainedSpellControl.Height;

                                                x.Controls.RemoveAt(i);

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
                                            x.Visible = false;
                                            if (x == pnlSustainedSpells)
                                            {
                                                chkPsycheActiveMagician.DoThreadSafe(y =>
                                                {
                                                    if (y != null)
                                                        y.Visible = false;
                                                }, token);
                                            }
                                            else if (x == pnlSustainedComplexForms)
                                            {
                                                chkPsycheActiveTechnomancer.DoThreadSafe(y =>
                                                {
                                                    if (y != null)
                                                        y.Visible = false;
                                                }, token);
                                            }
                                        }
                                    }, token).ConfigureAwait(false);
                                }

                                foreach (SustainedObject objSustained in e.NewItems)
                                {
                                    Panel refreshingPanel = DetermineRefreshingPanel(objSustained, pnlSustainedSpells,
                                        pnlSustainedComplexForms, pnlSustainedCritterPowers);

                                    if (refreshingPanel == null)
                                        continue;

                                    await refreshingPanel.DoThreadSafeAsync(x =>
                                    {
                                        x.Visible = true;
                                        switch (objSustained.LinkedObjectType)
                                        {
                                            case Improvement.ImprovementSource.Spell:
                                                chkPsycheActiveMagician.DoThreadSafe(y =>
                                                {
                                                    if (y != null)
                                                        y.Visible = true;
                                                }, token);
                                                break;

                                            case Improvement.ImprovementSource.ComplexForm:
                                                chkPsycheActiveTechnomancer.DoThreadSafe(y =>
                                                {
                                                    if (y != null)
                                                        y.Visible = true;
                                                }, token);
                                                break;
                                        }

                                        int intSustainedObjects = x.Controls.Count;

                                        SustainedObjectControl objSustainedObjectControl
                                            = new SustainedObjectControl(objSustained, GenericToken);

                                        objSustainedObjectControl.SustainedObjectDetailChanged
                                            += MakeDirtyWithCharacterUpdate;
                                        objSustainedObjectControl.UnsustainObject += DeleteSustainedObject;

                                        objSustainedObjectControl.Top
                                            = intSustainedObjects * objSustainedObjectControl.Height;

                                        x.Controls.Add(objSustainedObjectControl);
                                    }, token).ConfigureAwait(false);
                                }
                            }
                                break;
                        }
                    }
                }
                finally
                {
                    SkipUpdate = false;
                }
            }
            finally
            {
                await objCursorWait.DisposeAsync().ConfigureAwait(false);
            }
        }

        public async Task DeleteSustainedObject(object sender, EventArgs e, CancellationToken token = default)
        {
            if (token.IsCancellationRequested || GenericToken.IsCancellationRequested)
                return;
            if (!(sender is SustainedObjectControl objSender))
                return;
            CancellationTokenSource objSource = null;
            if (token != GenericToken)
            {
                objSource = CancellationTokenSource.CreateLinkedTokenSource(token, GenericToken);
                token = objSource.Token;
            }

            try
            {
                CursorWait objCursorWait = await CursorWait.NewAsync(this, token: token).ConfigureAwait(false);
                try
                {
                    SkipUpdate = true;
                    try
                    {
                        SustainedObject objSustainedObject = objSender.LinkedSustainedObject;

                        if (!await CommonFunctions.ConfirmDeleteAsync(
                                    string.Format(
                                        await LanguageManager.GetStringAsync("Message_DeleteSustainedSpell",
                                            token: token).ConfigureAwait(false),
                                        await objSustainedObject.GetCurrentDisplayNameAsync(token)
                                            .ConfigureAwait(false)),
                                    token)
                                .ConfigureAwait(false))
                            return;

                        await CharacterObject.SustainedCollection.RemoveAsync(objSustainedObject, token)
                            .ConfigureAwait(false);
                    }
                    catch (OperationCanceledException)
                    {
                        //swallow this
                    }
                    finally
                    {
                        SkipUpdate = false;
                    }
                }
                finally
                {
                    await objCursorWait.DisposeAsync().ConfigureAwait(false);
                }
            }
            finally
            {
                objSource?.Dispose();
            }
        }

        /// <summary>
        /// Moves a tree node to a specified spot in it's parent node collection.
        /// Will persist between loads if the node's object is an ICanSort
        /// </summary>
        /// <param name="objNode">The item to move</param>
        /// <param name="intNewIndex">The new index in the parent array</param>
        /// <param name="blnRetainTopLevelOrder">Whether we should retain the order of the top-level nodes when we sort the entire tree. Necessary if the top-level nodes are not based around ICanSort stuff</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public async Task MoveTreeNode(TreeNode objNode, int intNewIndex, bool blnRetainTopLevelOrder, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (!(objNode?.Tag is ICanSort objSortable))
                return;

            TreeView treOwningTree = objNode.TreeView;
            TreeNode objParent = objNode.Parent;
            TreeNodeCollection lstNodes = objParent?.Nodes ?? treOwningTree?.Nodes;

            if (lstNodes == null)
                return;
            CursorWait objCursorWait = await CursorWait.NewAsync(this, token: token).ConfigureAwait(false);
            try
            {
                SkipUpdate = true;
                try
                {
                    List<ICanSort> lstSorted = treOwningTree != null
                        ? await treOwningTree.DoThreadSafeFuncAsync(
                                () => lstNodes.Cast<TreeNode>().Select(n => n.Tag).OfType<ICanSort>().ToList(), token)
                            .ConfigureAwait(false)
                        : lstNodes.Cast<TreeNode>().Select(n => n.Tag).OfType<ICanSort>().ToList();

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
                    if (treOwningTree != null)
                        await treOwningTree.DoThreadSafeAsync(x => x.SortCustomOrder(blnRetainTopLevelOrder), token)
                            .ConfigureAwait(false);

                    await SetDirty(true, token).ConfigureAwait(false);
                }
                finally
                {
                    SkipUpdate = false;
                }
            }
            finally
            {
                await objCursorWait.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Adds the selected Object and child items to the clipboard as appropriate.
        /// </summary>
        protected async Task CopyObject(object selectedObject, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            using (await CursorWait.NewAsync(this, token: token).ConfigureAwait(false))
            {
                IAsyncDisposable objLocker = await GlobalSettings.EnterClipboardUpgradeableReadLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    switch (selectedObject)
                    {
                        case Armor objCopyArmor:
                        {
                            XmlDocument objCharacterXml = new XmlDocument { XmlResolver = null };
                            using (RecyclableMemoryStream objStream =
                                   new RecyclableMemoryStream(Utils.MemoryStreamManager))
                            {
                                using (XmlWriter objWriter = Utils.GetStandardXmlWriter(objStream))
                                {
                                    await objWriter.WriteStartDocumentAsync().ConfigureAwait(false);

                                    // </characters>
                                    objWriter.WriteStartElement("character");

                                    objCopyArmor.Save(objWriter);

                                    if (!objCopyArmor.WeaponID.IsEmptyGuid())
                                    {
                                        // <weapons>
                                        objWriter.WriteStartElement("weapons");
                                        // Copy any Weapon that comes with the Gear.
                                        foreach (Weapon objCopyWeapon in await CharacterObject.Weapons.DeepWhereAsync(
                                                         x => x.Children,
                                                         x => x.ParentID == objCopyArmor.InternalId, token)
                                                     .ConfigureAwait(false))
                                        {
                                            objCopyWeapon.Save(objWriter);
                                        }

                                        await objWriter.WriteEndElementAsync().ConfigureAwait(false);
                                    }

                                    // </characters>
                                    await objWriter.WriteEndElementAsync().ConfigureAwait(false);

                                    // Finish the document and flush the Writer and Stream.
                                    await objWriter.WriteEndDocumentAsync().ConfigureAwait(false);
                                    await objWriter.FlushAsync().ConfigureAwait(false);
                                }

                                // Read the stream.
                                objStream.Position = 0;

                                using (StreamReader objReader = new StreamReader(objStream, Encoding.UTF8, true))
                                using (XmlReader objXmlReader =
                                       XmlReader.Create(objReader, GlobalSettings.SafeXmlReaderSettings))
                                    // Put the stream into an XmlDocument
                                    await Task.Run(() => objCharacterXml.Load(objXmlReader), GenericToken);
                            }

                            await GlobalSettings
                                .SetClipboardAsync(objCharacterXml, ClipboardContentType.Armor, GenericToken)
                                .ConfigureAwait(false);
                            break;
                        }
                        case ArmorMod objCopyArmorMod:
                        {
                            XmlDocument objCharacterXml = new XmlDocument { XmlResolver = null };
                            using (RecyclableMemoryStream objStream =
                                   new RecyclableMemoryStream(Utils.MemoryStreamManager))
                            {
                                using (XmlWriter objWriter = Utils.GetStandardXmlWriter(objStream))
                                {
                                    await objWriter.WriteStartDocumentAsync().ConfigureAwait(false);

                                    // </characters>
                                    objWriter.WriteStartElement("character");

                                    objCopyArmorMod.Save(objWriter);

                                    if (!objCopyArmorMod.WeaponID.IsEmptyGuid())
                                    {
                                        // <weapons>
                                        objWriter.WriteStartElement("weapons");
                                        // Copy any Weapon that comes with the Gear.
                                        foreach (Weapon objCopyWeapon in await CharacterObject.Weapons.DeepWhereAsync(
                                                         x => x.Children,
                                                         x => x.ParentID == objCopyArmorMod.InternalId, token)
                                                     .ConfigureAwait(false))
                                        {
                                            objCopyWeapon.Save(objWriter);
                                        }

                                        await objWriter.WriteEndElementAsync().ConfigureAwait(false);
                                    }

                                    // </characters>
                                    await objWriter.WriteEndElementAsync().ConfigureAwait(false);

                                    // Finish the document and flush the Writer and Stream.
                                    await objWriter.WriteEndDocumentAsync().ConfigureAwait(false);
                                    await objWriter.FlushAsync().ConfigureAwait(false);
                                }

                                // Read the stream.
                                objStream.Position = 0;

                                using (StreamReader objReader = new StreamReader(objStream, Encoding.UTF8, true))
                                using (XmlReader objXmlReader =
                                       XmlReader.Create(objReader, GlobalSettings.SafeXmlReaderSettings))
                                    // Put the stream into an XmlDocument
                                    await Task.Run(() => objCharacterXml.Load(objXmlReader), GenericToken);
                            }

                            await GlobalSettings
                                .SetClipboardAsync(objCharacterXml, ClipboardContentType.ArmorMod, GenericToken)
                                .ConfigureAwait(false);
                            break;
                        }
                        case Cyberware objCopyCyberware:
                        {
                            XmlDocument objCharacterXml = new XmlDocument { XmlResolver = null };
                            using (RecyclableMemoryStream objStream =
                                   new RecyclableMemoryStream(Utils.MemoryStreamManager))
                            {
                                using (XmlWriter objWriter = Utils.GetStandardXmlWriter(objStream))
                                {
                                    await objWriter.WriteStartDocumentAsync().ConfigureAwait(false);

                                    // </characters>
                                    objWriter.WriteStartElement("character");

                                    objCopyCyberware.Save(objWriter);

                                    if (!objCopyCyberware.WeaponID.IsEmptyGuid())
                                    {
                                        // <weapons>
                                        objWriter.WriteStartElement("weapons");
                                        // Copy any Weapon that comes with the Gear.
                                        foreach (Weapon objCopyWeapon in await CharacterObject.Weapons.DeepWhereAsync(
                                                         x => x.Children,
                                                         x => x.ParentID == objCopyCyberware.InternalId, token)
                                                     .ConfigureAwait(false))
                                        {
                                            objCopyWeapon.Save(objWriter);
                                        }

                                        await objWriter.WriteEndElementAsync().ConfigureAwait(false);
                                    }

                                    if (!objCopyCyberware.VehicleID.IsEmptyGuid())
                                    {
                                        // <vehicles>
                                        objWriter.WriteStartElement("vehicles");
                                        // Copy any Vehicle that comes with the Gear.
                                        await CharacterObject.Vehicles.ForEachAsync(objCopyVehicle =>
                                        {
                                            if (objCopyVehicle.ParentID == objCopyCyberware.InternalId)
                                            {
                                                // ReSharper disable once AccessToDisposedClosure
                                                objCopyVehicle.Save(objWriter);
                                            }
                                        }, GenericToken).ConfigureAwait(false);

                                        await objWriter.WriteEndElementAsync().ConfigureAwait(false);
                                    }

                                    // </characters>
                                    await objWriter.WriteEndElementAsync().ConfigureAwait(false);

                                    // Finish the document and flush the Writer and Stream.
                                    await objWriter.WriteEndDocumentAsync().ConfigureAwait(false);
                                    await objWriter.FlushAsync().ConfigureAwait(false);
                                }

                                // Read the stream.
                                objStream.Position = 0;

                                using (StreamReader objReader = new StreamReader(objStream, Encoding.UTF8, true))
                                using (XmlReader objXmlReader =
                                       XmlReader.Create(objReader, GlobalSettings.SafeXmlReaderSettings))
                                    // Put the stream into an XmlDocument
                                    await Task.Run(() => objCharacterXml.Load(objXmlReader), GenericToken);
                            }

                            await GlobalSettings
                                .SetClipboardAsync(objCharacterXml, ClipboardContentType.Cyberware, GenericToken)
                                .ConfigureAwait(false);
                            break;
                        }
                        case Gear objCopyGear:
                        {
                            XmlDocument objCharacterXml = new XmlDocument { XmlResolver = null };
                            using (RecyclableMemoryStream objStream =
                                   new RecyclableMemoryStream(Utils.MemoryStreamManager))
                            {
                                using (XmlWriter objWriter = Utils.GetStandardXmlWriter(objStream))
                                {
                                    await objWriter.WriteStartDocumentAsync().ConfigureAwait(false);

                                    // </characters>
                                    objWriter.WriteStartElement("character");

                                    objCopyGear.Save(objWriter);

                                    if (!objCopyGear.WeaponID.IsEmptyGuid())
                                    {
                                        // <weapons>
                                        objWriter.WriteStartElement("weapons");
                                        // Copy any Weapon that comes with the Gear.
                                        foreach (Weapon objCopyWeapon in await CharacterObject.Weapons.DeepWhereAsync(
                                                         x => x.Children,
                                                         x => x.ParentID == objCopyGear.InternalId, token)
                                                     .ConfigureAwait(false))
                                        {
                                            objCopyWeapon.Save(objWriter);
                                        }

                                        await objWriter.WriteEndElementAsync().ConfigureAwait(false);
                                    }

                                    // </characters>
                                    await objWriter.WriteEndElementAsync().ConfigureAwait(false);

                                    // Finish the document and flush the Writer and Stream.
                                    await objWriter.WriteEndDocumentAsync().ConfigureAwait(false);
                                    await objWriter.FlushAsync().ConfigureAwait(false);
                                }

                                // Read the stream.
                                objStream.Position = 0;

                                using (StreamReader objReader = new StreamReader(objStream, Encoding.UTF8, true))
                                using (XmlReader objXmlReader =
                                       XmlReader.Create(objReader, GlobalSettings.SafeXmlReaderSettings))
                                    // Put the stream into an XmlDocument
                                    await Task.Run(() => objCharacterXml.Load(objXmlReader), GenericToken);
                            }

                            await GlobalSettings
                                .SetClipboardAsync(objCharacterXml, ClipboardContentType.Gear, GenericToken)
                                .ConfigureAwait(false);
                            break;
                        }
                        case Lifestyle objCopyLifestyle:
                        {
                            XmlDocument objCharacterXml = new XmlDocument { XmlResolver = null };
                            using (RecyclableMemoryStream objStream =
                                   new RecyclableMemoryStream(Utils.MemoryStreamManager))
                            {
                                using (XmlWriter objWriter = Utils.GetStandardXmlWriter(objStream))
                                {
                                    await objWriter.WriteStartDocumentAsync().ConfigureAwait(false);

                                    // </characters>
                                    objWriter.WriteStartElement("character");

                                    objCopyLifestyle.Save(objWriter);

                                    // </characters>
                                    await objWriter.WriteEndElementAsync().ConfigureAwait(false);

                                    // Finish the document and flush the Writer and Stream.
                                    await objWriter.WriteEndDocumentAsync().ConfigureAwait(false);
                                    await objWriter.FlushAsync().ConfigureAwait(false);

                                    // Read the stream.
                                    objStream.Position = 0;

                                    using (StreamReader objReader = new StreamReader(objStream, Encoding.UTF8, true))
                                    using (XmlReader objXmlReader =
                                           XmlReader.Create(objReader, GlobalSettings.SafeXmlReaderSettings))
                                        // Put the stream into an XmlDocument
                                        await Task.Run(() => objCharacterXml.Load(objXmlReader), GenericToken);
                                }
                            }

                            await GlobalSettings
                                .SetClipboardAsync(objCharacterXml, ClipboardContentType.Lifestyle, GenericToken)
                                .ConfigureAwait(false);
                            break;
                        }
                        case Vehicle objCopyVehicle:
                        {
                            XmlDocument objCharacterXml = new XmlDocument { XmlResolver = null };
                            using (RecyclableMemoryStream objStream =
                                   new RecyclableMemoryStream(Utils.MemoryStreamManager))
                            {
                                using (XmlWriter objWriter = Utils.GetStandardXmlWriter(objStream))
                                {
                                    await objWriter.WriteStartDocumentAsync().ConfigureAwait(false);

                                    // </characters>
                                    objWriter.WriteStartElement("character");

                                    objCopyVehicle.Save(objWriter);

                                    // </characters>
                                    await objWriter.WriteEndElementAsync().ConfigureAwait(false);

                                    // Finish the document and flush the Writer and Stream.
                                    await objWriter.WriteEndDocumentAsync().ConfigureAwait(false);
                                    await objWriter.FlushAsync().ConfigureAwait(false);
                                }

                                // Read the stream.
                                objStream.Position = 0;

                                using (StreamReader objReader = new StreamReader(objStream, Encoding.UTF8, true))
                                using (XmlReader objXmlReader =
                                       XmlReader.Create(objReader, GlobalSettings.SafeXmlReaderSettings))
                                    // Put the stream into an XmlDocument
                                    await Task.Run(() => objCharacterXml.Load(objXmlReader), GenericToken);
                            }

                            await GlobalSettings
                                .SetClipboardAsync(objCharacterXml, ClipboardContentType.Vehicle, GenericToken)
                                .ConfigureAwait(false);
                            break;
                        }
                        case Weapon objCopyWeapon:
                        {
                            // Do not let the user copy Gear or Cyberware Weapons.
                            if (objCopyWeapon.Category == "Gear" || objCopyWeapon.Cyberware)
                                return;

                            XmlDocument objCharacterXml = new XmlDocument { XmlResolver = null };
                            using (RecyclableMemoryStream objStream =
                                   new RecyclableMemoryStream(Utils.MemoryStreamManager))
                            {
                                using (XmlWriter objWriter = Utils.GetStandardXmlWriter(objStream))
                                {
                                    await objWriter.WriteStartDocumentAsync().ConfigureAwait(false);

                                    // </characters>
                                    objWriter.WriteStartElement("character");

                                    objCopyWeapon.Save(objWriter);

                                    // </characters>
                                    await objWriter.WriteEndElementAsync().ConfigureAwait(false);

                                    // Finish the document and flush the Writer and Stream.
                                    await objWriter.WriteEndDocumentAsync().ConfigureAwait(false);
                                    await objWriter.FlushAsync().ConfigureAwait(false);
                                }

                                // Read the stream.
                                objStream.Position = 0;

                                using (StreamReader objReader = new StreamReader(objStream, Encoding.UTF8, true))
                                using (XmlReader objXmlReader =
                                       XmlReader.Create(objReader, GlobalSettings.SafeXmlReaderSettings))
                                    // Put the stream into an XmlDocument
                                    await Task.Run(() => objCharacterXml.Load(objXmlReader), GenericToken);
                            }

                            await GlobalSettings
                                .SetClipboardAsync(objCharacterXml, ClipboardContentType.Weapon, GenericToken)
                                .ConfigureAwait(false);
                            break;
                        }
                        case WeaponAccessory objCopyAccessory:
                        {
                            // Do not let the user copy accessories that are unique to its parent.
                            if (objCopyAccessory.IncludedInWeapon)
                                return;

                            XmlDocument objCharacterXml = new XmlDocument { XmlResolver = null };
                            using (RecyclableMemoryStream objStream =
                                   new RecyclableMemoryStream(Utils.MemoryStreamManager))
                            {
                                using (XmlWriter objWriter = Utils.GetStandardXmlWriter(objStream))
                                {
                                    await objWriter.WriteStartDocumentAsync().ConfigureAwait(false);

                                    // </characters>
                                    objWriter.WriteStartElement("character");

                                    objCopyAccessory.Save(objWriter);

                                    // </characters>
                                    await objWriter.WriteEndElementAsync().ConfigureAwait(false);

                                    // Finish the document and flush the Writer and Stream.
                                    await objWriter.WriteEndDocumentAsync().ConfigureAwait(false);
                                    await objWriter.FlushAsync().ConfigureAwait(false);
                                }

                                // Read the stream.
                                objStream.Position = 0;

                                using (StreamReader objReader = new StreamReader(objStream, Encoding.UTF8, true))
                                using (XmlReader objXmlReader =
                                       XmlReader.Create(objReader, GlobalSettings.SafeXmlReaderSettings))
                                    // Put the stream into an XmlDocument
                                    await Task.Run(() => objCharacterXml.Load(objXmlReader), GenericToken);
                            }

                            await GlobalSettings
                                .SetClipboardAsync(objCharacterXml, ClipboardContentType.WeaponAccessory, GenericToken)
                                .ConfigureAwait(false);
                            break;
                        }
                    }
                }
                finally
                {
                    await objLocker.DisposeAsync().ConfigureAwait(false);
                }
            }
        }

        #region ContactControl Events

        protected void DragContactControl(object sender, MouseEventArgs e)
        {
            if (sender is Control source)
                source.DoDragDrop(new TransportWrapper(source), DragDropEffects.Move);
        }

        protected async Task AddContact(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            Contact objContact = new Contact(CharacterObject)
            {
                EntityType = ContactType.Contact
            };
            await CharacterObject.Contacts.AddAsync(objContact, token: token).ConfigureAwait(false);
            await MakeDirtyWithCharacterUpdate(token).ConfigureAwait(false);
        }

        protected async Task DeleteContact(object sender, EventArgs e, CancellationToken token = default)
        {
            if (token.IsCancellationRequested || GenericToken.IsCancellationRequested)
                return;
            if (!(sender is ContactControl objSender))
                return;
            CancellationTokenSource objSource = null;
            if (token != GenericToken)
            {
                objSource = CancellationTokenSource.CreateLinkedTokenSource(token, GenericToken);
                token = objSource.Token;
            }

            try
            {
                if (!await CommonFunctions
                        .ConfirmDeleteAsync(
                            await LanguageManager.GetStringAsync("Message_DeleteContact", token: token)
                                .ConfigureAwait(false), token).ConfigureAwait(false))
                    return;

                await CharacterObject.Contacts.RemoveAsync(objSender.ContactObject, token: token)
                    .ConfigureAwait(false);
                await MakeDirtyWithCharacterUpdate(token).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
            finally
            {
                objSource?.Dispose();
            }
        }

        #endregion ContactControl Events

        #region PetControl Events

        protected async Task AddPet(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            Contact objContact = new Contact(CharacterObject)
            {
                EntityType = ContactType.Pet
            };

            await CharacterObject.Contacts.AddAsync(objContact, token: token).ConfigureAwait(false);
            await MakeDirtyWithCharacterUpdate(token).ConfigureAwait(false);
        }

        protected async Task DeletePet(object sender, EventArgs e, CancellationToken token = default)
        {
            if (token.IsCancellationRequested || GenericToken.IsCancellationRequested)
                return;
            if (!(sender is PetControl objSender))
                return;
            CancellationTokenSource objSource = null;
            if (token != GenericToken)
            {
                objSource = CancellationTokenSource.CreateLinkedTokenSource(token, GenericToken);
                token = objSource.Token;
            }

            try
            {
                if (!await CommonFunctions
                        .ConfirmDeleteAsync(
                            await LanguageManager.GetStringAsync("Message_DeleteContact", token: token)
                                .ConfigureAwait(false), token).ConfigureAwait(false))
                    return;

                await CharacterObject.Contacts.RemoveAsync(objSender.ContactObject, token: token)
                    .ConfigureAwait(false);
                await MakeDirtyWithCharacterUpdate(token).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
            finally
            {
                objSource?.Dispose();
            }
        }

        #endregion PetControl Events

        #region EnemyControl Events

        protected async Task AddEnemy(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            // Handle the ConnectionRatingChanged Event for the ContactControl object.
            Contact objContact = new Contact(CharacterObject)
            {
                EntityType = ContactType.Enemy
            };

            await CharacterObject.Contacts.AddAsync(objContact, token: token).ConfigureAwait(false);
            await MakeDirtyWithCharacterUpdate(token).ConfigureAwait(false);
        }

        protected async Task DeleteEnemy(object sender, EventArgs e, CancellationToken token = default)
        {
            if (token.IsCancellationRequested || GenericToken.IsCancellationRequested)
                return;
            if (!(sender is ContactControl objSender))
                return;
            CancellationTokenSource objSource = null;
            if (token != GenericToken)
            {
                objSource = CancellationTokenSource.CreateLinkedTokenSource(token, GenericToken);
                token = objSource.Token;
            }

            try
            {
                if (!await CommonFunctions
                        .ConfirmDeleteAsync(
                            await LanguageManager.GetStringAsync("Message_DeleteEnemy", token: token)
                                .ConfigureAwait(false), token).ConfigureAwait(false))
                    return;

                await CharacterObject.Contacts.RemoveAsync(objSender.ContactObject, token: token)
                    .ConfigureAwait(false);
                await MakeDirtyWithCharacterUpdate(token).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
            finally
            {
                objSource?.Dispose();
            }
        }

        #endregion EnemyControl Events

        #region Additional Relationships Tab Control Events

        protected async Task AddContactsFromFile(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            CursorWait objCursorWait = await CursorWait.NewAsync(this, token: token).ConfigureAwait(false);
            try
            {
                SkipUpdate = true;
                try
                {
                    XPathDocument xmlDoc;
                    string strFileName = string.Empty;
                    string strFilter = await LanguageManager.GetStringAsync("DialogFilter_Xml", token: token)
                                           .ConfigureAwait(false) + '|' +
                                       await LanguageManager.GetStringAsync("DialogFilter_All", token: token)
                                           .ConfigureAwait(false);
                    // Displays an OpenFileDialog so the user can select the XML to read.
                    DialogResult eResult = await this.DoThreadSafeFuncAsync(x =>
                    {
                        using (OpenFileDialog dlgOpenFile = new OpenFileDialog())
                        {
                            dlgOpenFile.Filter = strFilter;
                            // Show the Dialog.
                            DialogResult eReturn = dlgOpenFile.ShowDialog(x);
                            strFileName = dlgOpenFile.FileName;
                            return eReturn;
                        }
                    }, token).ConfigureAwait(false);
                    // If the user cancels out, return early.
                    if (eResult != DialogResult.OK)
                        return;
                    try
                    {
                        xmlDoc = await XPathDocumentExtensions.LoadStandardFromFileAsync(strFileName, token: token)
                            .ConfigureAwait(false);
                    }
                    catch (IOException ex)
                    {
                        await Program.ShowScrollableMessageBoxAsync(this, ex.ToString(), token: token)
                            .ConfigureAwait(false);
                        return;
                    }
                    catch (XmlException ex)
                    {
                        await Program.ShowScrollableMessageBoxAsync(this, ex.ToString(), token: token)
                            .ConfigureAwait(false);
                        return;
                    }

                    foreach (XPathNavigator xmlContact in xmlDoc.CreateNavigator()
                                 .SelectAndCacheExpression(
                                     "/chummer/contacts/contact", token: token))
                    {
                        Contact objContact = new Contact(CharacterObject);
                        await objContact.LoadAsync(xmlContact, token).ConfigureAwait(false);
                        await CharacterObject.Contacts.AddAsync(objContact, token).ConfigureAwait(false);
                    }
                }
                finally
                {
                    SkipUpdate = false;
                }
            }
            finally
            {
                await objCursorWait.DisposeAsync().ConfigureAwait(false);
            }
        }

        #endregion Additional Relationships Tab Control Events

        public async Task RefreshSpiritsClearBindings(Panel panSpirits, Panel panSprites, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            CursorWait objCursorWait = await CursorWait.NewAsync(this, token: token).ConfigureAwait(false);
            try
            {
                SkipUpdate = true;
                try
                {
                    if (panSpirits != null)
                    {
                        await panSpirits.DoThreadSafeAsync(x =>
                        {
                            x.SuspendLayout();
                            try
                            {
                                for (int i = x.Controls.Count - 1; i >= 0; --i)
                                {
                                    token.ThrowIfCancellationRequested();
                                    if (!(x.Controls[i] is SpiritControl objSpiritControl))
                                        continue;
                                    objSpiritControl.ContactDetailChanged -= MakeDirtyWithCharacterUpdate;
                                    objSpiritControl.DeleteSpirit -= DeleteSpirit;
                                }
                            }
                            finally
                            {
                                x.ResumeLayout();
                            }
                        }, token).ConfigureAwait(false);
                    }

                    if (panSprites != null)
                    {
                        await panSprites.DoThreadSafeAsync(x =>
                        {
                            x.SuspendLayout();
                            try
                            {
                                for (int i = x.Controls.Count - 1; i >= 0; --i)
                                {
                                    token.ThrowIfCancellationRequested();
                                    if (!(x.Controls[i] is SpiritControl objSpiritControl))
                                        continue;
                                    objSpiritControl.ContactDetailChanged -= MakeDirtyWithCharacterUpdate;
                                    objSpiritControl.DeleteSpirit -= DeleteSpirit;
                                }
                            }
                            finally
                            {
                                x.ResumeLayout();
                            }
                        }, token).ConfigureAwait(false);
                    }
                }
                finally
                {
                    SkipUpdate = false;
                }
            }
            finally
            {
                await objCursorWait.DisposeAsync().ConfigureAwait(false);
            }
        }

        public async Task RefreshSpirits(Panel panSpirits, Panel panSprites, NotifyCollectionChangedEventArgs e = null, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (panSpirits == null && panSprites == null)
                return;
            CursorWait objCursorWait = await CursorWait.NewAsync(this, token: token).ConfigureAwait(false);
            try
            {
                SkipUpdate = true;
                try
                {
                    if (e == null ||
                        e.Action == NotifyCollectionChangedAction.Reset)
                    {
                        if (panSpirits != null)
                            await panSpirits.DoThreadSafeAsync(x => x.SuspendLayout(), token).ConfigureAwait(false);
                        if (panSprites != null)
                            await panSprites.DoThreadSafeAsync(x => x.SuspendLayout(), token).ConfigureAwait(false);
                        try
                        {
                            if (panSpirits != null)
                                await panSpirits.DoThreadSafeAsync(x => x.Controls.Clear(), token)
                                    .ConfigureAwait(false);
                            if (panSprites != null)
                                await panSprites.DoThreadSafeAsync(x => x.Controls.Clear(), token)
                                    .ConfigureAwait(false);
                            int intSpirits = -1;
                            int intSprites = -1;
                            await CharacterObject.Spirits.ForEachAsync(async objSpirit =>
                            {
                                bool blnIsSpirit = await objSpirit.GetEntityTypeAsync(token).ConfigureAwait(false) ==
                                                   SpiritType.Spirit;
                                if (blnIsSpirit)
                                {
                                    if (panSpirits == null)
                                        return;
                                }
                                else if (panSprites == null)
                                    return;

                                SpiritControl objSpiritControl
                                    = await this.DoThreadSafeFuncAsync(() => new SpiritControl(objSpirit, GenericToken),
                                            token)
                                        .ConfigureAwait(false);

                                // Attach an EventHandler for the ServicesOwedChanged Event.
                                objSpiritControl.ContactDetailChanged += MakeDirtyWithCharacterUpdate;
                                objSpiritControl.DeleteSpirit += DeleteSpirit;

                                await objSpiritControl.RebuildSpiritList(CharacterObject.MagicTradition, token)
                                    .ConfigureAwait(false);

                                if (blnIsSpirit)
                                {
                                    int index = Interlocked.Increment(ref intSpirits);
                                    await objSpiritControl.DoThreadSafeAsync(
                                        x => x.Top = index * x.Height, token).ConfigureAwait(false);
                                    await panSpirits.DoThreadSafeAsync(x => x.Controls.Add(objSpiritControl), token)
                                        .ConfigureAwait(false);
                                }
                                else
                                {
                                    int index = Interlocked.Increment(ref intSprites);
                                    await objSpiritControl.DoThreadSafeAsync(
                                        x => x.Top = index * x.Height, token).ConfigureAwait(false);
                                    await panSprites.DoThreadSafeAsync(x => x.Controls.Add(objSpiritControl), token)
                                        .ConfigureAwait(false);
                                }
                            }, token).ConfigureAwait(false);
                        }
                        finally
                        {
                            if (panSpirits != null)
                                await panSpirits.DoThreadSafeAsync(x => x.ResumeLayout(), GenericToken)
                                    .ConfigureAwait(false);
                            if (panSprites != null)
                                await panSprites.DoThreadSafeAsync(x => x.ResumeLayout(), GenericToken)
                                    .ConfigureAwait(false);
                        }
                    }
                    else
                    {
                        switch (e.Action)
                        {
                            case NotifyCollectionChangedAction.Add:
                            {
                                int intSpirits = panSpirits != null
                                    ? await panSpirits.DoThreadSafeFuncAsync(x => x.Controls.Count, token)
                                        .ConfigureAwait(false)
                                    : 0;
                                int intSprites = panSprites != null
                                    ? await panSprites.DoThreadSafeFuncAsync(x => x.Controls.Count, token)
                                        .ConfigureAwait(false)
                                    : 0;
                                foreach (Spirit objSpirit in e.NewItems)
                                {
                                    bool blnIsSpirit =
                                        await objSpirit.GetEntityTypeAsync(token).ConfigureAwait(false) ==
                                        SpiritType.Spirit;
                                    if (blnIsSpirit)
                                    {
                                        if (panSpirits == null)
                                            continue;
                                    }
                                    else if (panSprites == null)
                                        continue;

                                    SpiritControl objSpiritControl
                                        = await this.DoThreadSafeFuncAsync(
                                            () => new SpiritControl(objSpirit, GenericToken),
                                            token).ConfigureAwait(false);

                                    // Attach an EventHandler for the ServicesOwedChanged Event.
                                    objSpiritControl.ContactDetailChanged += MakeDirtyWithCharacterUpdate;
                                    objSpiritControl.DeleteSpirit += DeleteSpirit;

                                    await objSpiritControl.RebuildSpiritList(CharacterObject.MagicTradition, token)
                                        .ConfigureAwait(false);

                                    if (blnIsSpirit)
                                    {
                                        int index = Interlocked.Increment(ref intSpirits);
                                        await objSpiritControl.DoThreadSafeAsync(
                                            x => x.Top = index * x.Height, token).ConfigureAwait(false);
                                        await panSpirits.DoThreadSafeAsync(x => x.Controls.Add(objSpiritControl),
                                            token).ConfigureAwait(false);
                                    }
                                    else
                                    {
                                        int index = Interlocked.Increment(ref intSprites) - 1;
                                        await objSpiritControl.DoThreadSafeAsync(
                                            x => x.Top = index * x.Height, token).ConfigureAwait(false);
                                        await panSprites.DoThreadSafeAsync(x => x.Controls.Add(objSpiritControl),
                                            token).ConfigureAwait(false);
                                    }
                                }
                            }
                                break;

                            case NotifyCollectionChangedAction.Remove:
                            {
                                foreach (Spirit objSpirit in e.OldItems)
                                {
                                    int intMoveUpAmount = 0;
                                    if (await objSpirit.GetEntityTypeAsync(token).ConfigureAwait(false) ==
                                        SpiritType.Spirit)
                                    {
                                        if (panSpirits == null)
                                            continue;
                                        int intSpirits
                                            = await panSpirits.DoThreadSafeFuncAsync(x => x.Controls.Count, token)
                                                .ConfigureAwait(false);
                                        for (int i = 0; i < intSpirits; ++i)
                                        {
                                            int i1 = i;
                                            Control objLoopControl
                                                = await panSpirits.DoThreadSafeFuncAsync(x => x.Controls[i1], token)
                                                    .ConfigureAwait(false);
                                            if (objLoopControl is SpiritControl objSpiritControl &&
                                                objSpiritControl.SpiritObject == objSpirit)
                                            {
                                                intMoveUpAmount
                                                    = await objSpiritControl.DoThreadSafeFuncAsync(
                                                        x => x.Height, token).ConfigureAwait(false);
                                                await panSpirits.DoThreadSafeAsync(
                                                    x => x.Controls.RemoveAt(i1), token).ConfigureAwait(false);
                                                await objSpiritControl.DoThreadSafeAsync(x =>
                                                {
                                                    x.ContactDetailChanged
                                                        -= MakeDirtyWithCharacterUpdate;
                                                    x.DeleteSpirit -= DeleteSpirit;
                                                    x.Dispose();
                                                }, token).ConfigureAwait(false);
                                                --i;
                                                --intSpirits;
                                            }
                                            else if (intMoveUpAmount != 0)
                                            {
                                                int intAmount = intMoveUpAmount;
                                                await objLoopControl.DoThreadSafeAsync(
                                                    x => x.Top -= intAmount, token).ConfigureAwait(false);
                                            }
                                        }
                                    }
                                    else if (panSprites != null)
                                    {
                                        int intSprites = await panSprites
                                            .DoThreadSafeFuncAsync(x => x.Controls.Count, token).ConfigureAwait(false);
                                        for (int i = 0; i < intSprites; ++i)
                                        {
                                            int i1 = i;
                                            Control objLoopControl
                                                = await panSprites.DoThreadSafeFuncAsync(x => x.Controls[i1], token)
                                                    .ConfigureAwait(false);
                                            if (objLoopControl is SpiritControl objSpiritControl &&
                                                objSpiritControl.SpiritObject == objSpirit)
                                            {
                                                intMoveUpAmount
                                                    = await objSpiritControl.DoThreadSafeFuncAsync(
                                                        x => x.Height, token).ConfigureAwait(false);
                                                await panSprites.DoThreadSafeAsync(
                                                    x => x.Controls.RemoveAt(i1), token).ConfigureAwait(false);
                                                await objSpiritControl.DoThreadSafeAsync(x =>
                                                {
                                                    x.ContactDetailChanged
                                                        -= MakeDirtyWithCharacterUpdate;
                                                    x.DeleteSpirit -= DeleteSpirit;
                                                    x.Dispose();
                                                }, token).ConfigureAwait(false);
                                                --i;
                                                --intSprites;
                                            }
                                            else if (intMoveUpAmount != 0)
                                            {
                                                int intAmount = intMoveUpAmount;
                                                await objLoopControl.DoThreadSafeAsync(
                                                    x => x.Top -= intAmount, token).ConfigureAwait(false);
                                            }
                                        }
                                    }
                                }
                            }
                                break;

                            case NotifyCollectionChangedAction.Replace:
                            {
                                int intSpirits = panSpirits != null
                                    ? await panSpirits.DoThreadSafeFuncAsync(x => x.Controls.Count, token)
                                        .ConfigureAwait(false)
                                    : 0;
                                int intSprites = panSprites != null
                                    ? await panSprites.DoThreadSafeFuncAsync(x => x.Controls.Count, token)
                                        .ConfigureAwait(false)
                                    : 0;
                                foreach (Spirit objSpirit in e.OldItems)
                                {
                                    int intMoveUpAmount = 0;
                                    if (await objSpirit.GetEntityTypeAsync(token).ConfigureAwait(false) ==
                                        SpiritType.Spirit)
                                    {
                                        if (panSpirits == null)
                                            continue;
                                        for (int i = 0; i < intSpirits; ++i)
                                        {
                                            int i1 = i;
                                            Control objLoopControl
                                                = await panSpirits.DoThreadSafeFuncAsync(x => x.Controls[i1], token)
                                                    .ConfigureAwait(false);
                                            if (objLoopControl is SpiritControl objSpiritControl &&
                                                objSpiritControl.SpiritObject == objSpirit)
                                            {
                                                intMoveUpAmount
                                                    = await objSpiritControl.DoThreadSafeFuncAsync(
                                                        x => x.Height, token).ConfigureAwait(false);
                                                await panSpirits.DoThreadSafeAsync(
                                                    x => x.Controls.RemoveAt(i1), token).ConfigureAwait(false);
                                                await objSpiritControl.DoThreadSafeAsync(x =>
                                                {
                                                    x.ContactDetailChanged
                                                        -= MakeDirtyWithCharacterUpdate;
                                                    x.DeleteSpirit -= DeleteSpirit;
                                                    x.Dispose();
                                                }, token).ConfigureAwait(false);
                                                --i;
                                                --intSpirits;
                                            }
                                            else if (intMoveUpAmount != 0)
                                            {
                                                int intAmount = intMoveUpAmount;
                                                await objLoopControl.DoThreadSafeAsync(
                                                    x => x.Top -= intAmount, token).ConfigureAwait(false);
                                            }
                                        }
                                    }
                                    else if (panSprites != null)
                                    {
                                        for (int i = 0; i < intSprites; ++i)
                                        {
                                            int i1 = i;
                                            Control objLoopControl = await panSprites
                                                .DoThreadSafeFuncAsync(x => x.Controls[i1], token)
                                                .ConfigureAwait(false);
                                            if (objLoopControl is SpiritControl objSpiritControl &&
                                                objSpiritControl.SpiritObject == objSpirit)
                                            {
                                                intMoveUpAmount
                                                    = await objSpiritControl.DoThreadSafeFuncAsync(
                                                        x => x.Height, token).ConfigureAwait(false);
                                                await panSprites.DoThreadSafeAsync(
                                                    x => x.Controls.RemoveAt(i1), token).ConfigureAwait(false);
                                                await objSpiritControl.DoThreadSafeAsync(x =>
                                                {
                                                    x.ContactDetailChanged
                                                        -= MakeDirtyWithCharacterUpdate;
                                                    x.DeleteSpirit -= DeleteSpirit;
                                                    x.Dispose();
                                                }, token).ConfigureAwait(false);
                                                --i;
                                                --intSprites;
                                            }
                                            else if (intMoveUpAmount != 0)
                                            {
                                                int intAmount = intMoveUpAmount;
                                                await objLoopControl.DoThreadSafeAsync(
                                                    x => x.Top -= intAmount, token).ConfigureAwait(false);
                                            }
                                        }
                                    }
                                }

                                foreach (Spirit objSpirit in e.NewItems)
                                {
                                    bool blnIsSpirit =
                                        await objSpirit.GetEntityTypeAsync(token).ConfigureAwait(false) ==
                                        SpiritType.Spirit;
                                    if (blnIsSpirit)
                                    {
                                        if (panSpirits == null)
                                            continue;
                                    }
                                    else if (panSprites == null)
                                        continue;

                                    SpiritControl objSpiritControl
                                        = await this.DoThreadSafeFuncAsync(
                                            () => new SpiritControl(objSpirit, GenericToken),
                                            token).ConfigureAwait(false);

                                    // Attach an EventHandler for the ServicesOwedChanged Event.
                                    objSpiritControl.ContactDetailChanged += MakeDirtyWithCharacterUpdate;
                                    objSpiritControl.DeleteSpirit += DeleteSpirit;

                                    await objSpiritControl.RebuildSpiritList(CharacterObject.MagicTradition, token)
                                        .ConfigureAwait(false);

                                    if (blnIsSpirit)
                                    {
                                        int index = Interlocked.Increment(ref intSpirits) - 1;
                                        await objSpiritControl.DoThreadSafeAsync(
                                            x => x.Top = index * x.Height, token).ConfigureAwait(false);
                                        await panSpirits.DoThreadSafeAsync(x => x.Controls.Add(objSpiritControl),
                                            token).ConfigureAwait(false);
                                    }
                                    else
                                    {
                                        int index = Interlocked.Increment(ref intSprites) - 1;
                                        await objSpiritControl.DoThreadSafeAsync(
                                            x => x.Top = index * x.Height, token).ConfigureAwait(false);
                                        await panSprites.DoThreadSafeAsync(x => x.Controls.Add(objSpiritControl),
                                            token).ConfigureAwait(false);
                                    }
                                }
                            }
                                break;
                        }
                    }
                }
                finally
                {
                    SkipUpdate = false;
                }
            }
            finally
            {
                await objCursorWait.DisposeAsync().ConfigureAwait(false);
            }
        }

        #region SpiritControl Events

        protected async Task AddSpirit(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            // The number of bound Spirits cannot exceed the character's CHA.
            if (!await CharacterObject.GetIgnoreRulesAsync(token).ConfigureAwait(false) && await CharacterObject.Spirits
                    .CountAsync(
                        async x => await x.GetEntityTypeAsync(token).ConfigureAwait(false) == SpiritType.Spirit &&
                                   await x.GetBoundAsync(token).ConfigureAwait(false) && !await x.GetFetteredAsync(token).ConfigureAwait(false), token).ConfigureAwait(false) >=
                CharacterObject.BoundSpiritLimit)
            {
                await Program.ShowScrollableMessageBoxAsync(
                    this,
                    string.Format(GlobalSettings.CultureInfo,
                        await LanguageManager.GetStringAsync("Message_BoundSpiritLimit", token: token)
                            .ConfigureAwait(false),
                        await CharacterObject.Settings.GetBoundSpiritExpressionAsync(token).ConfigureAwait(false), CharacterObject.BoundSpiritLimit),
                    await LanguageManager.GetStringAsync("MessageTitle_BoundSpiritLimit", token: token)
                        .ConfigureAwait(false),
                    MessageBoxButtons.OK, MessageBoxIcon.Information, token: token).ConfigureAwait(false);
                return;
            }

            Spirit objSpirit = new Spirit(CharacterObject)
            {
                EntityType = SpiritType.Spirit,
                Force = await CharacterObject.GetMaxSpiritForceAsync(token).ConfigureAwait(false)
            };
            await CharacterObject.Spirits.AddAsync(objSpirit, token: token).ConfigureAwait(false);
            await MakeDirtyWithCharacterUpdate(token).ConfigureAwait(false);
        }

        protected async Task AddSprite(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            // In create, all sprites are added as Bound/Registered. The number of registered Sprites cannot exceed the character's LOG.
            if (!await CharacterObject.GetIgnoreRulesAsync(token).ConfigureAwait(false) &&
                await CharacterObject.Spirits
                    .CountAsync(
                        async x => await x.GetEntityTypeAsync(token).ConfigureAwait(false) == SpiritType.Sprite &&
                                   await x.GetBoundAsync(token).ConfigureAwait(false) && !await x.GetFetteredAsync(token).ConfigureAwait(false), token).ConfigureAwait(false) >=
                CharacterObject.RegisteredSpriteLimit)
            {
                await Program.ShowScrollableMessageBoxAsync(
                    this,
                    string.Format(GlobalSettings.CultureInfo,
                        await LanguageManager.GetStringAsync("Message_RegisteredSpriteLimit", token: token)
                            .ConfigureAwait(false),
                        await CharacterObject.Settings.GetRegisteredSpriteExpressionAsync(token).ConfigureAwait(false),
                        CharacterObject.RegisteredSpriteLimit),
                    await LanguageManager.GetStringAsync("MessageTitle_RegisteredSpriteLimit", token: token)
                        .ConfigureAwait(false),
                    MessageBoxButtons.OK, MessageBoxIcon.Information, token: token).ConfigureAwait(false);
                return;
            }

            Spirit objSprite = new Spirit(CharacterObject)
            {
                EntityType = SpiritType.Sprite,
                Force = await CharacterObject.GetMaxSpriteLevelAsync(token).ConfigureAwait(false)
            };
            await CharacterObject.Spirits.AddAsync(objSprite, token: token).ConfigureAwait(false);
            await MakeDirtyWithCharacterUpdate(token).ConfigureAwait(false);
        }

        protected async Task DeleteSpirit(object sender, EventArgs e, CancellationToken token = default)
        {
            if (token.IsCancellationRequested || GenericToken.IsCancellationRequested)
                return;
            if (!(sender is SpiritControl objSender))
                return;
            CancellationTokenSource objSource = null;
            if (token != GenericToken)
            {
                objSource = CancellationTokenSource.CreateLinkedTokenSource(token, GenericToken);
                token = objSource.Token;
            }

            try
            {
                Spirit objSpirit = objSender.SpiritObject;
                bool blnIsSpirit = await objSpirit.GetEntityTypeAsync(token).ConfigureAwait(false) ==
                                   SpiritType.Spirit;
                if (!await CommonFunctions
                        .ConfirmDeleteAsync(
                            await LanguageManager
                                .GetStringAsync(blnIsSpirit ? "Message_DeleteSpirit" : "Message_DeleteSprite",
                                    token: token).ConfigureAwait(false), token).ConfigureAwait(false))
                    return;
                await objSpirit.SetFetteredAsync(false, GenericToken)
                    .ConfigureAwait(false); // Fettered spirits consume MAG.
                await CharacterObject.Spirits.RemoveAsync(objSpirit, token: token).ConfigureAwait(false);
                await MakeDirtyWithCharacterUpdate(token).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
            finally
            {
                objSource?.Dispose();
            }
        }

        #endregion SpiritControl Events

        /// <summary>
        /// Add a mugshot to the character.
        /// </summary>
        protected async Task<bool> AddMugshot(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            CursorWait objCursorWait = await CursorWait.NewAsync(this, token: token).ConfigureAwait(false);
            try
            {
                SkipUpdate = true;
                try
                {
                    ImageCodecInfo[] lstCodecs = ImageCodecInfo.GetImageEncoders();
                    string strFormat = "{0}"
                                       + await LanguageManager.GetStringAsync("String_Space", token: token)
                                           .ConfigureAwait(false) + "({1})|{1}";
                    string strFilter = string.Format(
                        GlobalSettings.InvariantCultureInfo,
                        await LanguageManager.GetStringAsync("DialogFilter_ImagesPrefix", token: token)
                            .ConfigureAwait(false) + "({1})|{1}|{0}|" +
                        await LanguageManager.GetStringAsync("DialogFilter_All", token: token).ConfigureAwait(false),
                        string.Join("|",
                            lstCodecs.Select(codec => string.Format(GlobalSettings.CultureInfo,
                                strFormat, codec.CodecName,
                                codec.FilenameExtension))),
                        string.Join(";", lstCodecs.Select(codec => codec.FilenameExtension)));
                    string strInitialDirectory = string.Empty;
                    if (!string.IsNullOrWhiteSpace(GlobalSettings.RecentImageFolder) &&
                        Directory.Exists(GlobalSettings.RecentImageFolder))
                    {
                        strInitialDirectory = GlobalSettings.RecentImageFolder;
                    }

                    string strFileName = string.Empty;
                    string strErrorString = await LanguageManager.GetStringAsync(
                            "Message_File_Cannot_Be_Read_Accessed",
                            token: token)
                        .ConfigureAwait(false);

                    // Prompt the user to select an image to associate with this character.
                    bool blnMakeLoop = true;
                    while (blnMakeLoop)
                    {
                        token.ThrowIfCancellationRequested();
                        blnMakeLoop = false;
                        DialogResult eResult = await this.DoThreadSafeFuncAsync(x =>
                        {
                            using (OpenFileDialog dlgOpenFile = new OpenFileDialog())
                            {
                                dlgOpenFile.InitialDirectory = strInitialDirectory;
                                dlgOpenFile.Filter = strFilter;
                                DialogResult eReturn = dlgOpenFile.ShowDialog(x);
                                strFileName = dlgOpenFile.FileName;
                                return eReturn;
                            }
                        }, token: token).ConfigureAwait(false);
                        if (eResult != DialogResult.OK)
                            return false;
                        token.ThrowIfCancellationRequested();
                        if (!File.Exists(strFileName))
                        {
                            await Program
                                .ShowScrollableMessageBoxAsync(string.Format(strErrorString, strFileName), token: token)
                                .ConfigureAwait(false);
                            blnMakeLoop = true;
                        }
                    }

                    // Convert the image to a string using Base64.
                    GlobalSettings.RecentImageFolder = Path.GetDirectoryName(strFileName);

                    using (Bitmap bmpMugshot = new Bitmap(strFileName, true))
                    {
                        if (bmpMugshot.PixelFormat == PixelFormat.Format32bppPArgb)
                        {
                            await CharacterObject.Mugshots.AddAsync(
                                    bmpMugshot.Clone() as Bitmap, token)
                                .ConfigureAwait(false); // Clone makes sure file handle is closed
                        }
                        else
                        {
                            await CharacterObject.Mugshots.AddAsync(
                                    bmpMugshot.ConvertPixelFormat(PixelFormat.Format32bppPArgb), token)
                                .ConfigureAwait(false);
                        }
                    }

                    if (CharacterObject.MainMugshotIndex == -1)
                        CharacterObject.MainMugshotIndex = CharacterObject.Mugshots.Count - 1;

                    return true;
                }
                finally
                {
                    SkipUpdate = false;
                }
            }
            finally
            {
                await objCursorWait.DisposeAsync().ConfigureAwait(false);
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

        protected enum ItemTreeViewTypes
        {
            Misc,
            Weapons,
            Armor,
            Gear,
            Vehicles,
            Improvements
        }

        protected MouseButtons DragButton { get; set; } = MouseButtons.None;
        protected bool DraggingGear { get; set; }

        protected async Task DoTreeDragDrop(object sender, DragEventArgs e, TreeView treView, ItemTreeViewTypes eType, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            Point pt = ((TreeView)sender).PointToClient(new Point(e.X, e.Y));
            TreeNode nodDestination = await ((TreeView)sender).DoThreadSafeFuncAsync(x => x.GetNodeAt(pt), token).ConfigureAwait(false);

            TreeNode objSelected = await treView.DoThreadSafeFuncAsync(x => x.SelectedNode, token).ConfigureAwait(false);
            for (TreeNode nodLoop = nodDestination; nodLoop != null; nodLoop = nodLoop.Parent)
            {
                if (nodLoop == objSelected)
                    return;
            }

            int intNewIndex = 0;
            if (nodDestination != null)
            {
                intNewIndex = nodDestination.Index;
            }
            else
            {
                int intNodesCount = await treView.DoThreadSafeFuncAsync(x => x.Nodes.Count, token).ConfigureAwait(false);
                if (intNodesCount > 0)
                {
                    await treView.DoThreadSafeAsync(x =>
                    {
                        intNewIndex = x.Nodes[intNodesCount - 1].Nodes.Count;
                        nodDestination = x.Nodes[intNodesCount - 1];
                    }, token).ConfigureAwait(false);
                }
            }

            bool requireParentSortable = false;
            // Put the weapon in the right location (or lack thereof)
            await treView.DoThreadSafeAsync(() =>
            {
                switch (eType)
                {
                    case ItemTreeViewTypes.Misc:
                        requireParentSortable = true;
                        break;
                    case ItemTreeViewTypes.Weapons:
                    {
                        if (objSelected.Level == 1)
                            CharacterObject.MoveWeaponNode(intNewIndex, nodDestination, objSelected, token: token);
                        else
                            CharacterObject.MoveWeaponRoot(intNewIndex, nodDestination, objSelected);
                        break;
                    }
                    case ItemTreeViewTypes.Armor:
                    {
                        if (objSelected.Level == 1)
                            CharacterObject.MoveArmorNode(intNewIndex, nodDestination, objSelected, token: token);
                        else
                            CharacterObject.MoveArmorRoot(intNewIndex, nodDestination, objSelected);
                        break;
                    }
                    case ItemTreeViewTypes.Gear:
                    {
                        switch (DragButton)
                        {
                            // If the item was moved using the left mouse button, change the order of things.
                            case MouseButtons.Left when objSelected.Level == 1:
                                CharacterObject.MoveGearNode(intNewIndex, nodDestination, objSelected, token: token);
                                break;

                            case MouseButtons.Left:
                                CharacterObject.MoveGearRoot(intNewIndex, nodDestination, objSelected);
                                break;

                            case MouseButtons.Right:
                                CharacterObject.MoveGearParent(objSelected, objSelected, token: token);
                                break;
                        }
                        break;
                    }
                    case ItemTreeViewTypes.Vehicles:
                    {
                        if (!DraggingGear)
                        {
                            CharacterObject.MoveVehicleNode(intNewIndex, nodDestination, objSelected, token: token);
                        }
                        else
                        {
                            CharacterObject.MoveVehicleGearParent(nodDestination, objSelected, token: token);
                            DraggingGear = false;
                        }
                        break;
                    }
                    case ItemTreeViewTypes.Improvements:
                    {
                        if (objSelected.Level == 1)
                            CharacterObject.MoveImprovementNode(nodDestination, objSelected, token: token);
                        else
                            CharacterObject.MoveImprovementRoot(intNewIndex, nodDestination, objSelected);
                        break;
                    }
                    default:
                        throw new ArgumentOutOfRangeException(nameof(eType), eType, null);
                }
            }, token).ConfigureAwait(false);

            // Put the weapon in the right order in the tree
            await MoveTreeNode(await treView.DoThreadSafeFuncAsync(x => x.FindNodeByTag(objSelected.Tag), token).ConfigureAwait(false), intNewIndex, requireParentSortable, token).ConfigureAwait(false);

            await treView.DoThreadSafeAsync(x =>
            {
                // Update the entire tree to prevent any holes in the sort order
                x.CacheSortOrder();
                // Clear the background color for all Nodes.
                x.ClearNodeBackground(null);
                // Store our new order so it's loaded properly the next time we open the character
                x.CacheSortOrder();
            }, token).ConfigureAwait(false);

            await SetDirty(true, token).ConfigureAwait(false);
        }

        /// <summary>
        /// Whether the character has changes that can be saved
        /// </summary>
        public bool IsDirty
        {
            get => _intIsDirty > 0;
            set
            {
                int intNewValue = value.ToInt32();
                if (Interlocked.Exchange(ref _intIsDirty, intNewValue) == intNewValue)
                    return;
                UpdateWindowTitle(true);
            }
        }

        public async Task SetDirty(bool blnValue, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            int intNewValue = blnValue.ToInt32();
            if (Interlocked.Exchange(ref _intIsDirty, intNewValue) != intNewValue)
                await UpdateWindowTitleAsync(true, token).ConfigureAwait(false);
        }

        /// <summary>
        /// Whether the form is currently in the middle of refreshing some UI elements
        /// </summary>
        public bool IsRefreshing
        {
            get => _intRefreshingCount > 0;
            set
            {
                if (value || Interlocked.Decrement(ref _intRefreshingCount) < 0)
                    Interlocked.Increment(ref _intRefreshingCount);
            }
        }

        public bool IsLoading
        {
            get => _intLoadingCount > 0;
            set
            {
                if (value || Interlocked.Decrement(ref _intLoadingCount) < 0)
                    Interlocked.Increment(ref _intLoadingCount);
            }
        }

        public bool IsFinishedInitializing { get; protected set; }

        public async Task MakeDirtyWithCharacterUpdate(object sender, NotifyCollectionChangedEventArgs e, CancellationToken token = default)
        {
            if (e.Action == NotifyCollectionChangedAction.Move)
                return;

            try
            {
                RequestCharacterUpdate(token);
                await MakeDirty(sender, e, token).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                // swallow this
            }
        }

        public async Task MakeDirtyWithCharacterUpdate(object sender, ListChangedEventArgs e, CancellationToken token = default)
        {
            if (e.ListChangedType != ListChangedType.ItemAdded
                && e.ListChangedType != ListChangedType.ItemChanged
                && e.ListChangedType != ListChangedType.ItemDeleted
                && e.ListChangedType != ListChangedType.Reset)
                return;

            try
            {
                RequestCharacterUpdate(token);
                await MakeDirty(sender, e, token).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                // swallow this
            }
        }

        public async Task MakeDirtyWithCharacterUpdate(object sender, EventArgs e, CancellationToken token)
        {
            try
            {
                RequestCharacterUpdate(token);
                await MakeDirty(sender, e, token).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                // swallow this
            }
        }

        public async void MakeDirtyWithCharacterUpdate(object sender, EventArgs e)
        {
            try
            {
                RequestCharacterUpdate(GenericToken);
                await SetDirty(true, GenericToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                // swallow this
            }
        }

        public async Task MakeDirtyWithCharacterUpdate(CancellationToken token = default)
        {
            try
            {
                RequestCharacterUpdate(token);
                await SetDirty(true, token).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                // swallow this
            }
        }

        public async Task MakeDirty(object sender, NotifyCollectionChangedEventArgs e, CancellationToken token = default)
        {
            if (e.Action == NotifyCollectionChangedAction.Move)
                return;

            try
            {
                await SetDirty(true, token).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                // swallow this
            }
        }

        public async Task MakeDirty(object sender, ListChangedEventArgs e, CancellationToken token = default)
        {
            if (e.ListChangedType != ListChangedType.ItemAdded
                && e.ListChangedType != ListChangedType.ItemChanged
                && e.ListChangedType != ListChangedType.ItemDeleted
                && e.ListChangedType != ListChangedType.Reset)
                return;

            try
            {
                await SetDirty(true, token).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                // swallow this
            }
        }

        public async Task MakeDirty(object sender, EventArgs e, CancellationToken token)
        {
            try
            {
                await SetDirty(true, token).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                // swallow this
            }
        }

        public async void MakeDirty(object sender, EventArgs e)
        {
            try
            {
                await SetDirty(true, GenericToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                // swallow this
            }
        }

        private System.Timers.Timer _tmrCharacterUpdateRequestTimer = new System.Timers.Timer(500) { AutoReset = true };

        private async void CharacterUpdateRequestTimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            if (IsLoading)
                return;
            if (GenericToken.IsCancellationRequested)
                return;
            if (_intCharacterUpdateRequested == 0)
                return;
            DebuggableSemaphoreSlim objCharacterUpdateStartingSemaphore = CharacterUpdateStartingSemaphore;
            if (objCharacterUpdateStartingSemaphore == null)
                return;
            if (!await objCharacterUpdateStartingSemaphore.WaitAsync(0, GenericToken).ConfigureAwait(false))
                return;
            try
            {
                try
                {
                    await ActuallyRequestCharacterUpdate(false, GenericToken).ConfigureAwait(false);
                }
                catch
                {
                    objCharacterUpdateStartingSemaphore.Release();
                    throw;
                }
            }
            catch
            {
                objCharacterUpdateStartingSemaphore.Release();
                throw;
            }
        }

        protected void ProcessCharacterUpdateHasStarted()
        {
            _intCharacterUpdateRequested = 0;
            try
            {
                CharacterUpdateStartingSemaphore?.Release();
            }
            catch (SemaphoreFullException)
            {
                // Potential sign of bad code, but not fatal
                Utils.BreakIfDebug();
            }
        }

        private int _intCharacterUpdateRequested;

        /// <summary>
        /// Request a character update to happen at the next convenient time.
        /// The delay to expect should usually be around 1 second.
        /// </summary>
        /// <param name="token"></param>
        public void RequestCharacterUpdate(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            GenericToken.ThrowIfCancellationRequested();
            if (IsLoading || CharacterUpdateStartingSemaphore?.CurrentCount == 0)
                return;
            Interlocked.CompareExchange(ref _intCharacterUpdateRequested, 1, 0);
        }

        /// <summary>
        /// Request a character update and then await it immediately. Because this is done so rarely, there is no delay.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task RequestAndProcessCharacterUpdate(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            GenericToken.ThrowIfCancellationRequested();
            if (IsLoading)
                return;
            DebuggableSemaphoreSlim objCharacterUpdateStartingSemaphore = CharacterUpdateStartingSemaphore;
            if (objCharacterUpdateStartingSemaphore == null)
                return;
            CancellationTokenSource objSource = null;
            if (token == CancellationToken.None)
            {
                token = GenericToken;
            }
            else if (token != GenericToken)
            {
                objSource = CancellationTokenSource.CreateLinkedTokenSource(token, GenericToken);
                token = objSource.Token;
            }

            try
            {
                int intOldUpdateRequested;
                if (await objCharacterUpdateStartingSemaphore.WaitAsync(0, token).ConfigureAwait(false))
                    intOldUpdateRequested = Interlocked.CompareExchange(ref _intCharacterUpdateRequested, 1, 0);
                else
                {
                    intOldUpdateRequested = Interlocked.Exchange(ref _intCharacterUpdateRequested, -1);
                    do
                    {
                        _objUpdateCharacterInfoCancellationTokenSource?.Cancel(false);
                    } while (!await objCharacterUpdateStartingSemaphore.WaitAsync(1000, token).ConfigureAwait(false));

                    _intCharacterUpdateRequested = 1;
                }

                try
                {
                    try
                    {
                        await ActuallyRequestCharacterUpdate(true, token).ConfigureAwait(false);
                    }
                    catch
                    {
                        objCharacterUpdateStartingSemaphore.Release();
                        throw;
                    }
                }
                catch
                {
                    Interlocked.CompareExchange(ref _intCharacterUpdateRequested, intOldUpdateRequested, 1);
                    objCharacterUpdateStartingSemaphore.Release();
                    throw;
                }
            }
            finally
            {
                objSource?.Dispose();
            }
        }

        private async Task ActuallyRequestCharacterUpdate(bool blnAlsoProcessUpdate, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            GenericToken.ThrowIfCancellationRequested();
            if (IsLoading)
                return;
            CancellationTokenSource objSource = null;
            if (token == CancellationToken.None)
            {
                token = GenericToken;
            }
            else if (token != GenericToken)
            {
                objSource = CancellationTokenSource.CreateLinkedTokenSource(token, GenericToken);
                token = objSource.Token;
            }

            Task tskNew;
            try
            {
                Task tskTemp = Task.CompletedTask;
                CancellationTokenSource objNewSource = new CancellationTokenSource();
                CancellationToken objNewToken = objNewSource.Token;
                try
                {
                    CancellationTokenSource objOldSource
                        = Interlocked.Exchange(ref _objUpdateCharacterInfoCancellationTokenSource, objNewSource);
                    if (objOldSource != null)
                    {
                        if (!objOldSource.IsCancellationRequested)
                        {
                            try
                            {
                                objOldSource.Cancel(false);
                            }
                            catch (ObjectDisposedException)
                            {
                                //swallow this
                            }
                        }

                        objOldSource.Dispose();
                    }

                    token.ThrowIfCancellationRequested();
                    Task tskOld = Interlocked.Exchange(ref _tskUpdateCharacterInfo, tskTemp);
                    if (tskOld?.IsCompleted == false)
                    {
                        try
                        {
                            await tskOld.ConfigureAwait(false);
                        }
                        catch (OperationCanceledException)
                        {
                            //swallow this
                        }
                    }
                }
                catch
                {
                    Interlocked.CompareExchange(ref _objUpdateCharacterInfoCancellationTokenSource, null, objNewSource)?.Dispose();
                    CharacterUpdateStartingSemaphore?.Release();
                    throw;
                }

                tskNew = blnAlsoProcessUpdate
                    ? Task.Run(() => DoUpdateCharacterInfo(objNewToken), objNewToken)
                    : Utils.RunInEmptyExecutionContext(
                        () => Task.Run(() => DoUpdateCharacterInfo(objNewToken), objNewToken));

                if (Interlocked.CompareExchange(ref _tskUpdateCharacterInfo, tskNew, tskTemp) != tskTemp)
                {
                    Interlocked.CompareExchange(ref _objUpdateCharacterInfoCancellationTokenSource, null, objNewSource);
                    try
                    {
                        objNewSource.Cancel(false);
                    }
                    finally
                    {
                        objNewSource.Dispose();
                    }
                    try
                    {
                        await tskNew.ConfigureAwait(false);
                    }
                    catch (OperationCanceledException)
                    {
                        //swallow this
                    }

                    return;
                }
            }
            finally
            {
                objSource?.Dispose();
            }

            if (blnAlsoProcessUpdate)
            {
                try
                {
                    await tskNew.ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    //swallow this
                }
            }
        }

        public bool IsCharacterUpdateRequested
        {
            get
            {
                if (_intCharacterUpdateRequested > 0)
                    return true;
                // Local for thread safety
                DebuggableSemaphoreSlim objCharacterUpdateStartingSemaphore = CharacterUpdateStartingSemaphore;
                if (objCharacterUpdateStartingSemaphore == null)
                    return false;
                if (objCharacterUpdateStartingSemaphore.CurrentCount == 0)
                    return true;
                Task tskTemp = _tskUpdateCharacterInfo; // Need to do this in case the task gets swapped out by an interlock in between the null check and the IsCompleted check
                return tskTemp?.IsCompleted == false;
            }
        }

        private DebuggableSemaphoreSlim _objCharacterUpdateStartingSemaphore = new DebuggableSemaphoreSlim();

        protected DebuggableSemaphoreSlim CharacterUpdateStartingSemaphore => _objCharacterUpdateStartingSemaphore;

        protected Task UpdateCharacterInfoTask => _tskUpdateCharacterInfo;

        private Task _tskUpdateCharacterInfo = Task.CompletedTask;

        private CancellationTokenSource _objUpdateCharacterInfoCancellationTokenSource;

        protected virtual Task DoUpdateCharacterInfo(CancellationToken token = default)
        {
            return token.IsCancellationRequested ? Task.FromCanceled(token) : Task.CompletedTask;
        }

        protected bool SkipUpdate
        {
            get => _intUpdatingCount > 0;
            set
            {
                if (value)
                    Interlocked.Increment(ref _intUpdatingCount);
                else
                {
                    int intCurrentUpdatingCount = Interlocked.Decrement(ref _intUpdatingCount);
                    if (intCurrentUpdatingCount < 0)
                        Interlocked.CompareExchange(ref _intUpdatingCount, 0, intCurrentUpdatingCount);
                }
            }
        }

        public Character CharacterObject => _objCharacter;

        public IEnumerable<Character> CharacterObjects => _objCharacter?.Yield() ?? Enumerable.Empty<Character>();

        private CharacterSettings _objCachedSettings;
        private Stopwatch _stpAutosaveStopwatch = Utils.StopwatchPool.Get();

        protected CharacterSettings CharacterObjectSettings => _objCachedSettings ?? (_objCachedSettings = CharacterObject?.Settings);

        protected virtual string FormMode => string.Empty;

        /// <summary>
        /// Update the Window title to show the Character's name and unsaved changes status.
        /// </summary>
        protected void UpdateWindowTitle(bool blnCanSkip)
        {
            if (Text.EndsWith('*') == IsDirty && blnCanSkip)
                return;

            string strSpace = LanguageManager.GetString("String_Space", token: GenericToken);
            string strTitle = CharacterObject.CharacterName + strSpace + '-' + strSpace + FormMode + strSpace + '(' + CharacterObjectSettings.Name + ')';
            if (IsDirty)
                strTitle += '*';
            this.DoThreadSafe((x, y) => x.Text = strTitle, token: GenericToken);
        }

        /// <summary>
        /// Update the Window title to show the Character's name and unsaved changes status.
        /// </summary>
        protected async Task UpdateWindowTitleAsync(bool blnCanSkip, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            CancellationTokenSource objSource = null;
            if (token == CancellationToken.None)
            {
                token = GenericToken;
            }
            else if (token != GenericToken)
            {
                objSource = CancellationTokenSource.CreateLinkedTokenSource(token, GenericToken);
                token = objSource.Token;
            }

            try
            {
                token.ThrowIfCancellationRequested();
                if (Text.EndsWith('*') == IsDirty && blnCanSkip)
                    return;
                string strSpace = await LanguageManager.GetStringAsync("String_Space", token: token).ConfigureAwait(false);
                string strTitle = CharacterObject.CharacterName + strSpace + '-' + strSpace + FormMode + strSpace + '('
                                  + CharacterObjectSettings.Name + ')';
                if (IsDirty)
                    strTitle += '*';
                await this.DoThreadSafeAsync(x => x.Text = strTitle, token).ConfigureAwait(false);
            }
            finally
            {
                objSource?.Dispose();
            }
        }

        /// <summary>
        /// Save the Character.
        /// </summary>
        public virtual async Task<bool> SaveCharacter(bool blnNeedConfirm = true, bool blnDoCreated = false, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            CancellationTokenSource objSource = null;
            if (token == CancellationToken.None)
            {
                token = GenericToken;
            }
            else if (token != GenericToken)
            {
                objSource = CancellationTokenSource.CreateLinkedTokenSource(token, GenericToken);
                token = objSource.Token;
            }

            try
            {
                CursorWait objCursorWait = await CursorWait.NewAsync(this, token: token).ConfigureAwait(false);
                try
                {
                    // If the Character does not have a file name, trigger the Save As menu item instead.
                    if (string.IsNullOrEmpty(CharacterObject.FileName))
                    {
                        return await SaveCharacterAs(blnDoCreated, token).ConfigureAwait(false);
                    }

                    if (blnDoCreated)
                    {
                        // If the Created is checked, make sure the user wants to actually save this character.
                        if (blnNeedConfirm && !await ConfirmSaveCreatedCharacter(token).ConfigureAwait(false))
                            return false;
                        // If this character has just been saved as Created, close this form and re-open the character which will open it in the Career window instead.
                        return await SaveCharacterAsCreated(token).ConfigureAwait(false);
                    }

                    using (ThreadSafeForm<LoadingBar> frmLoadingBar
                           = await Program.CreateAndShowProgressBarAsync(token: token).ConfigureAwait(false))
                    {
                        await frmLoadingBar.MyForm.PerformStepAsync(CharacterObject.CharacterName,
                                                                    LoadingBar.ProgressBarTextPatterns.Saving, token).ConfigureAwait(false);
                        if (_objCharacterFileWatcher != null)
                            _objCharacterFileWatcher.Changed -= LiveUpdateFromCharacterFile;
                        try
                        {
                            if (!await CharacterObject.SaveAsync(token: token).ConfigureAwait(false))
                                return false;
                        }
                        finally
                        {
                            if (_objCharacterFileWatcher != null)
                                _objCharacterFileWatcher.Changed += LiveUpdateFromCharacterFile;
                        }

                        await GlobalSettings.MostRecentlyUsedCharacters.InsertAsync(0, CharacterObject.FileName, token).ConfigureAwait(false);
                        await SetDirty(false, token).ConfigureAwait(false);
                    }

                    return true;
                }
                finally
                {
                    await objCursorWait.DisposeAsync().ConfigureAwait(false);
                }
            }
            finally
            {
                objSource?.Dispose();
            }
        }

        /// <summary>
        /// Save the Character using the Save As dialogue box.
        /// </summary>
        public virtual async Task<bool> SaveCharacterAs(bool blnDoCreated = false, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            CancellationTokenSource objSource = null;
            if (token == CancellationToken.None)
            {
                token = GenericToken;
            }
            else if (token != GenericToken)
            {
                objSource = CancellationTokenSource.CreateLinkedTokenSource(token, GenericToken);
                token = objSource.Token;
            }

            try
            {
                CursorWait objCursorWait = await CursorWait.NewAsync(this, token: token).ConfigureAwait(false);
                try
                {
                    // If the Created is checked, make sure the user wants to actually save this character.
                    if (blnDoCreated && !await ConfirmSaveCreatedCharacter(token).ConfigureAwait(false))
                    {
                        return false;
                    }

                    string strOldFileName = await CharacterObject.GetFileNameAsync(token).ConfigureAwait(false);
                    string strShowFileName = Path.GetFileName(strOldFileName);
                    if (string.IsNullOrEmpty(strShowFileName))
                    {
                        strShowFileName = (await CharacterObject.GetCharacterNameAsync(token).ConfigureAwait(false)).CleanForFileName();
                    }

                    dlgSaveFile.FileName = strShowFileName;
                    if (await this.DoThreadSafeFuncAsync(x => dlgSaveFile.ShowDialog(x), token: token).ConfigureAwait(false)
                        != DialogResult.OK)
                        return false;

                    string strFileName = dlgSaveFile.FileName;
                    if (!string.IsNullOrEmpty(strFileName)
                        && !strFileName.EndsWith(".chum5", StringComparison.OrdinalIgnoreCase)
                        && !strFileName.EndsWith(".chum5lz", StringComparison.OrdinalIgnoreCase))
                    {
                        strFileName += strShowFileName.EndsWith(".chum5lz", StringComparison.OrdinalIgnoreCase)
                            ? ".chum5lz"
                            : ".chum5";
                    }
                    try
                    {
                        await CharacterObject.SetFileNameAsync(strFileName, token).ConfigureAwait(false);
                        bool blnReturn = await SaveCharacter(false, blnDoCreated, token).ConfigureAwait(false);
                        if (!blnReturn)
                            await CharacterObject.SetFileNameAsync(strOldFileName, token).ConfigureAwait(false);
                        return blnReturn;
                    }
                    catch
                    {
                        await CharacterObject.SetFileNameAsync(strOldFileName, CancellationToken.None).ConfigureAwait(false);
                        throw;
                    }
                }
                finally
                {
                    await objCursorWait.DisposeAsync().ConfigureAwait(false);
                }
            }
            finally
            {
                objSource?.Dispose();
            }
        }

        /// <summary>
        /// Save the character as Created and re-open it in Career Mode.
        /// </summary>
        public virtual Task<bool> SaveCharacterAsCreated(CancellationToken token = default) { return Task.FromResult(false); }

        /// <summary>
        /// Verify that the user wants to save this character as Created.
        /// </summary>
        public virtual Task<bool> ConfirmSaveCreatedCharacter(CancellationToken token = default) { return Task.FromResult(true); }

        public async Task DoPrint(CancellationToken token = default)
        {
            if (!await Program.SwitchToOpenPrintCharacter(CharacterObject, token).ConfigureAwait(false))
                await Program.OpenCharacterForPrinting(CharacterObject, token: token).ConfigureAwait(false);
        }

        public async Task DoExport(CancellationToken token = default)
        {
            if (!await Program.SwitchToOpenExportCharacter(CharacterObject, token).ConfigureAwait(false))
                await Program.OpenCharacterForExport(CharacterObject, token: token).ConfigureAwait(false);
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_objCharacter?.IsDisposed == false)
                {
                    _objCharacter.MultiplePropertiesChangedAsync -= CharacterPropertyChanged;
                }

                Interlocked.Exchange(ref _objCharacterFileWatcher, null)?.Dispose();
                CancellationTokenSource objTemp = Interlocked.Exchange(ref _objUpdateCharacterInfoCancellationTokenSource, null);
                if (objTemp != null)
                {
                    try
                    {
                        objTemp.Cancel(false);
                    }
                    catch (ObjectDisposedException)
                    {
                        //swallow this
                    }
                    finally
                    {
                        objTemp.Dispose();
                    }
                }
                dlgSaveFile?.Dispose();
                Interlocked.Exchange(ref _tmrAutosaveRequestTimer, null)?.Dispose();
                Interlocked.Exchange(ref _tmrCharacterUpdateRequestTimer, null)?.Dispose();
                Interlocked.Exchange(ref _objCharacterUpdateStartingSemaphore, null)?.Dispose();
                Interlocked.Exchange(ref _objAutosaveSemaphore, null)?.Dispose();
                if (_stpAutosaveStopwatch != null)
                    Utils.StopwatchPool.Return(ref _stpAutosaveStopwatch);
            }
            base.Dispose(disposing);
        }

        protected async Task RemoveSelectedObject(object objSelected, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            GenericToken.ThrowIfCancellationRequested();
            if (!(objSelected is ICanRemove objRemovable))
                return;
            CancellationTokenSource objSource = null;
            if (token != GenericToken)
            {
                objSource = CancellationTokenSource.CreateLinkedTokenSource(token, GenericToken);
                token = objSource.Token;
            }

            try
            {
                await objRemovable.RemoveAsync(token: token).ConfigureAwait(false);
            }
            finally
            {
                objSource?.Dispose();
            }
        }

        #region Vehicles Tab

        public async Task PurchaseVehicleGear(Vehicle objSelectedVehicle, Location objLocation = null, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            CursorWait objCursorWait = await CursorWait.NewAsync(this, token: token).ConfigureAwait(false);
            try
            {
                XmlDocument objXmlDocument = await CharacterObject.LoadDataAsync("gear.xml", token: token).ConfigureAwait(false);
                bool blnAddAgain;

                do
                {
                    using (ThreadSafeForm<SelectGear> frmPickGear
                           = await ThreadSafeForm<SelectGear>.GetAsync(
                               () => new SelectGear(CharacterObject, 0, 1, objSelectedVehicle), token).ConfigureAwait(false))
                    {
                        if (await frmPickGear.ShowDialogSafeAsync(this, token).ConfigureAwait(false) == DialogResult.Cancel)
                            break;
                        SkipUpdate = true;
                        try
                        {
                            blnAddAgain = frmPickGear.MyForm.AddAgain;

                            // Open the Gear XML file and locate the selected piece.
                            XmlNode objXmlGear = objXmlDocument.TryGetNodeByNameOrId("/chummer/gears/gear",
                                frmPickGear.MyForm.SelectedGear);

                            // Create the new piece of Gear.
                            List<Weapon> lstWeapons = new List<Weapon>(1);

                            Gear objGear = new Gear(CharacterObject);
                            await objGear.CreateAsync(objXmlGear, frmPickGear.MyForm.SelectedRating, lstWeapons, string.Empty,
                                false, token: token).ConfigureAwait(false);

                            if (objGear.InternalId.IsEmptyGuid())
                                continue;

                            objGear.Quantity = frmPickGear.MyForm.SelectedQty;
                            objGear.DiscountCost = frmPickGear.MyForm.BlackMarketDiscount;

                            // Reduce the cost for Do It Yourself components.
                            if (frmPickGear.MyForm.DoItYourself)
                                objGear.Cost = '(' + objGear.Cost + ") * 0.5";
                            // If the item was marked as free, change its cost.
                            if (frmPickGear.MyForm.FreeCost)
                                objGear.Cost = "0";

                            if (await CharacterObject.GetCreatedAsync(token).ConfigureAwait(false))
                            {
                                decimal decCost = await objGear.GetTotalCostAsync(token).ConfigureAwait(false);

                                // Multiply the cost if applicable.
                                char chrAvail = (await objGear.TotalAvailTupleAsync(token: token).ConfigureAwait(false))
                                    .Suffix;
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
                                if (!frmPickGear.MyForm.FreeCost)
                                {
                                    if (decCost > CharacterObject.Nuyen)
                                    {
                                        await Program.ShowScrollableMessageBoxAsync(this,
                                            await LanguageManager.GetStringAsync(
                                                "Message_NotEnoughNuyen", token: token).ConfigureAwait(false),
                                            await LanguageManager.GetStringAsync(
                                                "MessageTitle_NotEnoughNuyen", token: token).ConfigureAwait(false),
                                            MessageBoxButtons.OK,
                                            MessageBoxIcon.Information, token: token).ConfigureAwait(false);
                                        continue;
                                    }

                                    // Create the Expense Log Entry.
                                    ExpenseLogEntry objExpense = new ExpenseLogEntry(CharacterObject);
                                    objExpense.Create(decCost * -1,
                                        await LanguageManager.GetStringAsync(
                                            "String_ExpensePurchaseVehicleGear", token: token).ConfigureAwait(false) +
                                        await LanguageManager.GetStringAsync("String_Space", token: token)
                                            .ConfigureAwait(false) +
                                        await objGear.GetCurrentDisplayNameShortAsync(token).ConfigureAwait(false),
                                        ExpenseType.Nuyen,
                                        DateTime.Now);
                                    await CharacterObject.ExpenseEntries.AddWithSortAsync(objExpense, token: token)
                                        .ConfigureAwait(false);
                                    await CharacterObject.ModifyNuyenAsync(-decCost, token).ConfigureAwait(false);

                                    ExpenseUndo objUndo = new ExpenseUndo();
                                    objUndo.CreateNuyen(NuyenExpenseType.AddVehicleGear, objGear.InternalId, 1);
                                    objExpense.Undo = objUndo;
                                }
                            }

                            Gear objExistingGear = null;
                            // If this is Ammunition, see if the character already has it on them.
                            if ((objGear.Category == "Ammunition" ||
                                 !string.IsNullOrEmpty(objGear.AmmoForWeaponType)) && frmPickGear.MyForm.Stack)
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
                                if (objLocation != null)
                                    await objLocation.Children.AddAsync(objGear, token).ConfigureAwait(false);
                                await objSelectedVehicle.GearChildren.AddAsync(objGear, token).ConfigureAwait(false);
                                await objGear.SetParentAsync(objSelectedVehicle, token).ConfigureAwait(false);

                                foreach (Weapon objWeapon in lstWeapons)
                                {
                                    if (objLocation != null)
                                        await objLocation.Children.AddAsync(objGear, token).ConfigureAwait(false);
                                    objWeapon.ParentVehicle = objSelectedVehicle;
                                    await objSelectedVehicle.Weapons.AddAsync(objWeapon, token).ConfigureAwait(false);
                                }
                            }
                        }
                        finally
                        {
                            SkipUpdate = false;
                        }
                    }

                    await MakeDirtyWithCharacterUpdate(token).ConfigureAwait(false);
                } while (blnAddAgain);
            }
            finally
            {
                await objCursorWait.DisposeAsync().ConfigureAwait(false);
            }
        }

        #endregion Vehicles Tab
    }
}
