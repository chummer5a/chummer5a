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
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace Chummer
{
    /// <summary>
    /// A Location.
    /// </summary>
    [DebuggerDisplay("{nameof(Name)}")]
    public sealed class Location : IHasInternalId, IHasName, IHasNotes, ICanRemove, ICanSort, IHasLockObject, IHasCharacterObject
    {
        private Guid _guiID;
        private string _strName;
        private string _strNotes = string.Empty;
        private Color _colNotes = ColorManager.HasNotesColor;
        private int _intSortOrder;
        private readonly Character _objCharacter;
        private readonly ThreadSafeObservableCollection<IHasLocation> _lstChildren;

        public Character CharacterObject => _objCharacter; // readonly member, no locking needed

        #region Constructor, Create, Save, Load, and Print Methods

        public Location(Character objCharacter, ICollection<Location> objParent, string strName = "")
        {
            // Create the GUID for the new art.
            _guiID = Guid.NewGuid();
            _objCharacter = objCharacter ?? throw new ArgumentNullException(nameof(objCharacter));
            LockObject = objCharacter.LockObject;
            _lstChildren = new ThreadSafeObservableCollection<IHasLocation>(LockObject);
            _strName = strName;
            Parent = objParent;
            Children.CollectionChanged += ChildrenOnCollectionChanged;
            Children.BeforeClearCollectionChanged += ChildrenOnBeforeClearCollectionChanged;
        }

        /// <summary>
        /// Save the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        public void Save(XmlWriter objWriter)
        {
            if (objWriter == null)
                return;
            using (LockObject.EnterReadLock())
            {
                objWriter.WriteStartElement("location");
                objWriter.WriteElementString("guid", _guiID.ToString("D", GlobalSettings.InvariantCultureInfo));
                objWriter.WriteElementString("name", _strName);
                objWriter.WriteElementString("notes", _strNotes.CleanOfInvalidUnicodeChars());
                objWriter.WriteElementString("notesColor", ColorTranslator.ToHtml(_colNotes));
                objWriter.WriteElementString("sortorder", _intSortOrder.ToString(GlobalSettings.InvariantCultureInfo));
                objWriter.WriteEndElement();
            }
        }

        /// <summary>
        /// Load the Metamagic from the XmlNode.
        /// </summary>
        /// <param name="objNode">XmlNode to load.</param>
        public void Load(XmlNode objNode)
        {
            using (LockObject.EnterWriteLock())
            {
                if (!objNode.TryGetField("guid", Guid.TryParse, out _guiID))
                {
                    _guiID = Guid.NewGuid();
                    _strName = objNode.InnerText;
                }
                else
                {
                    objNode.TryGetStringFieldQuickly("name", ref _strName);
                    objNode.TryGetMultiLineStringFieldQuickly("notes", ref _strNotes);

                    string sNotesColor = ColorTranslator.ToHtml(ColorManager.HasNotesColor);
                    objNode.TryGetStringFieldQuickly("notesColor", ref sNotesColor);
                    _colNotes = ColorTranslator.FromHtml(sNotesColor);
                }

                objNode.TryGetInt32FieldQuickly("sortorder", ref _intSortOrder);

                if (Parent?.Contains(this) == false)
                    Parent.Add(this);
            }
        }

        /// <summary>
        /// Print the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        /// <param name="strLanguageToPrint">Language in which to print</param>
        public async Task Print(XmlWriter objWriter, string strLanguageToPrint, CancellationToken token = default)
        {
            if (objWriter == null)
                return;
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                await objWriter.WriteStartElementAsync("location", token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("guid", InternalId, token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("name", await DisplayNameShortAsync(strLanguageToPrint, token).ConfigureAwait(false), token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("fullname", await DisplayNameAsync(strLanguageToPrint, token).ConfigureAwait(false), token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("name_english", Name, token).ConfigureAwait(false);
                if (GlobalSettings.PrintNotes)
                    await objWriter.WriteElementStringAsync("notes", await GetNotesAsync(token).ConfigureAwait(false), token).ConfigureAwait(false);
                await objWriter.WriteEndElementAsync().ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        #endregion Constructor, Create, Save, Load, and Print Methods

        #region Properties

        /// <summary>
        /// Internal identifier which will be used to identify this Metamagic in the Improvement system.
        /// </summary>
        public string InternalId
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _guiID.ToString("D", GlobalSettings.InvariantCultureInfo);
            }
        }

        /// <summary>
        /// Metamagic name.
        /// </summary>
        public string Name
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _strName;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                    _strName = value;
            }
        }

        public string CurrentDisplayName => DisplayName(GlobalSettings.Language);

        public string CurrentDisplayNameShort => DisplayNameShort(GlobalSettings.Language);

        /// <summary>
        /// The name of the object as it should be displayed on printouts (translated name only).
        /// </summary>
        public string DisplayNameShort(string strLanguage = "")
        {
            if (string.IsNullOrEmpty(strLanguage) || strLanguage == GlobalSettings.Language)
                return Name;
            using (LockObject.EnterReadLock())
            {
                return _objCharacter.TranslateExtra(
                    !GlobalSettings.Language.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase)
                        ? LanguageManager.ReverseTranslateExtra(Name, GlobalSettings.Language, _objCharacter)
                        : Name, strLanguage);
            }
        }

        /// <summary>
        /// The name of the object as it should be displayed in lists. Name (Extra).
        /// </summary>
        public string DisplayName(string strLanguage = "")
        {
            if (string.IsNullOrEmpty(strLanguage))
                strLanguage = GlobalSettings.Language;
            return DisplayNameShort(strLanguage);
        }

        public Task<string> GetCurrentDisplayNameAsync(CancellationToken token = default)
        {
            return DisplayNameAsync(GlobalSettings.Language, token);
        }

        public Task<string> GetCurrentDisplayNameShortAsync(CancellationToken token = default)
        {
            return DisplayNameShortAsync(GlobalSettings.Language, token);
        }

        /// <summary>
        /// The name of the object as it should be displayed on printouts (translated name only).
        /// </summary>
        public async Task<string> DisplayNameShortAsync(string strLanguage = "", CancellationToken token = default)
        {
            if (string.IsNullOrEmpty(strLanguage) || strLanguage == GlobalSettings.Language)
                return Name;
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return await _objCharacter.TranslateExtraAsync(
                    !GlobalSettings.Language.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase)
                        ? await LanguageManager
                                .ReverseTranslateExtraAsync(Name, GlobalSettings.Language, _objCharacter, token: token)
                                .ConfigureAwait(false)
                        : Name, strLanguage, token: token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// The name of the object as it should be displayed in lists. Name (Extra).
        /// </summary>
        public Task<string> DisplayNameAsync(string strLanguage = "", CancellationToken token = default)
        {
            if (string.IsNullOrEmpty(strLanguage))
                strLanguage = GlobalSettings.Language;
            return DisplayNameShortAsync(strLanguage, token);
        }

        /// <summary>
        /// Notes.
        /// </summary>
        public string Notes
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _strNotes;
            }
            set
            {
                using (LockObject.EnterReadLock())
                {
                    if (_strNotes == value)
                        return;
                }
                using (LockObject.EnterUpgradeableReadLock())
                    _strNotes = value;
            }
        }

        public async Task<string> GetNotesAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _strNotes;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public async Task SetNotesAsync(string value, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_strNotes == value)
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                _strNotes = value;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Forecolor to use for Notes in treeviews.
        /// </summary>
        public Color NotesColor
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _colNotes;
            }
            set
            {
                using (LockObject.EnterReadLock())
                {
                    if (_colNotes == value)
                        return;
                }

                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (_colNotes == value)
                        return;
                    using (LockObject.EnterWriteLock())
                    {
                        _colNotes = value;
                    }
                }
            }
        }

        public async Task<Color> GetNotesColorAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _colNotes;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public async Task SetNotesColorAsync(Color value, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (value == _colNotes)
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }

            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_colNotes == value)
                    return;
                IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    _colNotes = value;
                }
                finally
                {
                    await objLocker2.DisposeAsync().ConfigureAwait(false);
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Used by our sorting algorithm to remember which order the user moves things to
        /// </summary>
        public int SortOrder
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _intSortOrder;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                    _intSortOrder = value;
            }
        }

        public ThreadSafeObservableCollection<IHasLocation> Children
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _lstChildren;
            }
        }

        public ICollection<Location> Parent { get; }

        #endregion Properties

        #region UI Methods

        public async Task<TreeNode> CreateTreeNode(ContextMenuStrip cmsLocation, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                TreeNode objNode = new TreeNode
                {
                    Name = InternalId,
                    Text = await GetCurrentDisplayNameAsync(token).ConfigureAwait(false),
                    Tag = this,
                    ContextMenuStrip = cmsLocation,
                    ForeColor = await GetPreferredColorAsync(token).ConfigureAwait(false),
                    ToolTipText = (await GetNotesAsync(token).ConfigureAwait(false)).WordWrap()
                };

                return objNode;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public Color PreferredColor
        {
            get
            {
                using (LockObject.EnterReadLock())
                {
                    return !string.IsNullOrEmpty(Notes)
                        ? ColorManager.GenerateCurrentModeColor(NotesColor)
                        : ColorManager.WindowText;
                }
            }
        }

        public async Task<Color> GetPreferredColorAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return !string.IsNullOrEmpty(await GetNotesAsync(token).ConfigureAwait(false))
                    ? ColorManager.GenerateCurrentModeColor(await GetNotesColorAsync(token).ConfigureAwait(false))
                    : ColorManager.WindowText;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        #endregion UI Methods

        private void ChildrenOnBeforeClearCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            foreach (IHasLocation objOldItem in e.OldItems)
                objOldItem.Location = null;
        }

        private void ChildrenOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (IHasLocation objNewItem in e.NewItems)
                        objNewItem.Location = this;
                    break;

                case NotifyCollectionChangedAction.Remove:
                    foreach (IHasLocation objOldItem in e.OldItems)
                        objOldItem.Location = null;
                    break;

                case NotifyCollectionChangedAction.Replace:
                    HashSet<IHasLocation> setNewItems = e.NewItems.OfType<IHasLocation>().ToHashSet();
                    foreach (IHasLocation objOldItem in e.OldItems)
                    {
                        if (!setNewItems.Contains(objOldItem))
                            objOldItem.Location = null;
                    }
                    foreach (IHasLocation objNewItem in setNewItems)
                        objNewItem.Location = this;
                    break;

                case NotifyCollectionChangedAction.Reset:
                    foreach (IHasLocation objItem in Children.AsEnumerableWithSideEffects())
                        objItem.Location = this;
                    break;
            }
        }

        public bool Remove(bool blnConfirmDelete = true)
        {
            if (blnConfirmDelete && !CommonFunctions.ConfirmDelete(LanguageManager.GetString("Message_DeleteGearLocation")))
                return false;

            foreach (IHasLocation item in Children.AsEnumerableWithSideEffects())
                item.Location = null;

            bool blnReturn = Parent?.Contains(this) != true || Parent.Remove(this);

            Dispose();

            return blnReturn;
        }

        public async Task<bool> RemoveAsync(bool blnConfirmDelete = true, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (blnConfirmDelete && !await CommonFunctions
                    .ConfirmDeleteAsync(
                        await LanguageManager.GetStringAsync("Message_DeleteGearLocation", token: token)
                            .ConfigureAwait(false), token).ConfigureAwait(false))
                return false;

            await Children.ForEachWithSideEffectsAsync(x => x.Location = null, token: token).ConfigureAwait(false);

            bool blnReturn = Parent?.Contains(this) != true || Parent.Remove(this);

            await DisposeAsync().ConfigureAwait(false);

            return blnReturn;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            using (LockObject.EnterWriteLock())
                _lstChildren.Dispose();
        }

        /// <inheritdoc />
        public async ValueTask DisposeAsync()
        {
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync().ConfigureAwait(false);
            try
            {
                await _lstChildren.DisposeAsync().ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <inheritdoc />
        public AsyncFriendlyReaderWriterLock LockObject { get; }
    }
}
