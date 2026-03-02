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
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Chummer.Annotations;
using NLog;

namespace Chummer
{
    public sealed class SustainedObject : IHasInternalId, INotifyPropertyChangedAsync, IHasCharacterObject
    {
        private static readonly Lazy<Logger> s_ObjLogger = new Lazy<Logger>(LogManager.GetCurrentClassLogger);
        private static Logger Log => s_ObjLogger.Value;
        private Guid _guiID;
        private readonly Character _objCharacter;
        private bool _blnSelfSustained = true;
        private int _intForce;
        private int _intNetHits;
        private readonly IHasInternalId _objLinkedObject;
        private readonly Improvement.ImprovementSource _eLinkedObjectType;

        public Character CharacterObject => _objCharacter; // readonly member, no locking needed

        #region Constructor, Create, Save, Load, and Print Methods

        public SustainedObject(Character objCharacter, IHasInternalId objLinkedObject)
        {
            // Create the GUID for the new Spell.
            _guiID = Guid.NewGuid();
            _objCharacter = objCharacter;
            _objLinkedObject = objLinkedObject;
            switch (objLinkedObject)
            {
                case Spell _:
                    _eLinkedObjectType = Improvement.ImprovementSource.Spell;
                    break;

                case ComplexForm _:
                    _eLinkedObjectType = Improvement.ImprovementSource.ComplexForm;
                    break;

                case CritterPower _:
                    _eLinkedObjectType = Improvement.ImprovementSource.CritterPower;
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(objLinkedObject));
            }
        }

        public SustainedObject(Character objCharacter, XmlNode objNode)
        {
            _objCharacter = objCharacter;
            if (objNode == null)
            {
                _guiID = Guid.Empty;
                return;
            }
            string strLinkedId = string.Empty;
            if (!objNode.TryGetStringFieldQuickly("linkedobject", ref strLinkedId))
            {
                _guiID = Guid.Empty;
                return;
            }
            string strType = string.Empty;
            if (objNode.TryGetStringFieldQuickly("linkedobjecttype", ref strType))
                _eLinkedObjectType = Improvement.ConvertToImprovementSource(strType);
            else
            {
                _guiID = Guid.Empty;
                return;
            }
            IEnumerable<IHasInternalId> lstToSearch;
            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
            switch (_eLinkedObjectType)
            {
                case Improvement.ImprovementSource.Spell:
                    lstToSearch = objCharacter.Spells;
                    break;

                case Improvement.ImprovementSource.ComplexForm:
                    lstToSearch = objCharacter.ComplexForms;
                    break;

                case Improvement.ImprovementSource.CritterPower:
                    lstToSearch = objCharacter.CritterPowers;
                    break;

                default:
                    _guiID = Guid.Empty;
                    return;
            }
            _objLinkedObject = lstToSearch.FirstOrDefault(x => x.InternalId == strLinkedId);
            if (_objLinkedObject == null)
            {
                Utils.BreakIfDebug();
                _guiID = Guid.Empty;
                return;
            }
            objNode.TryGetInt32FieldQuickly("force", ref _intForce);
            objNode.TryGetInt32FieldQuickly("nethits", ref _intNetHits);
            objNode.TryGetBoolFieldQuickly("self", ref _blnSelfSustained);
            // Create the GUID for the new Spell.
            _guiID = Guid.NewGuid();
        }

        /// <summary>
        /// Save the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        public void Save(XmlWriter objWriter)
        {
            if (objWriter == null)
                return;
            objWriter.WriteStartElement("sustainedobject");
            objWriter.WriteElementString("linkedobject", _objLinkedObject.InternalId);
            objWriter.WriteElementString("linkedobjecttype", _eLinkedObjectType.ToString());
            objWriter.WriteElementString("force", _intForce.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("nethits", _intNetHits.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("self", _blnSelfSustained.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteEndElement();
        }

        /// <summary>
        /// Print the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        /// <param name="objCulture">Culture in which to print numbers.</param>
        /// <param name="strLanguageToPrint">Language in which to print.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public async Task Print(XmlWriter objWriter, CultureInfo objCulture, string strLanguageToPrint, CancellationToken token = default)
        {
            if (objWriter == null)
                return;
            // <sustainedobject>
            XmlElementWriteHelper objBaseElement = await objWriter.StartElementAsync("sustainedobject", token).ConfigureAwait(false);
            try
            {
                await objWriter
                      .WriteElementStringAsync(
                          "name", await DisplayNameShortAsync(strLanguageToPrint, token).ConfigureAwait(false), token)
                      .ConfigureAwait(false);
                await objWriter
                      .WriteElementStringAsync(
                          "fullname", await DisplayNameAsync(strLanguageToPrint, token).ConfigureAwait(false), token)
                      .ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("name_english", await GetNameAsync(token).ConfigureAwait(false), token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("force", (await GetForceAsync(token).ConfigureAwait(false)).ToString(objCulture), token)
                               .ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("nethits", (await GetNetHitsAsync(token).ConfigureAwait(false)).ToString(objCulture), token)
                               .ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("self", (await GetSelfSustainedAsync(token).ConfigureAwait(false)).ToString(objCulture), token)
                               .ConfigureAwait(false);
            }
            finally
            {
                // </sustainedobject>
                await objBaseElement.DisposeAsync().ConfigureAwait(false);
            }
        }

        #endregion Constructor, Create, Save, Load, and Print Methods

        #region Properties

        /// <summary>
        /// Is the spell sustained by yourself?
        /// </summary>
        public bool SelfSustained
        {
            get
            {
                using (_objCharacter.LockObject.EnterReadLock())
                    return _blnSelfSustained;
            }
            set
            {
                using (_objCharacter.LockObject.EnterReadLock())
                {
                    if (_blnSelfSustained == value)
                        return;
                }
                using (_objCharacter.LockObject.EnterUpgradeableReadLock())
                {
                    if (_blnSelfSustained == value)
                        return;
                    using (_objCharacter.LockObject.EnterWriteLock())
                    {
                        _blnSelfSustained = value;
                        OnPropertyChanged();
                    }
                }
            }
        }

        /// <summary>
        /// Is the spell sustained by yourself?
        /// </summary>
        public async Task<bool> GetSelfSustainedAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await _objCharacter.LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _blnSelfSustained;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Is the spell sustained by yourself?
        /// </summary>
        public async Task SetSelfSustainedAsync(bool value, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await _objCharacter.LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_blnSelfSustained == value)
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            objLocker = await _objCharacter.LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_blnSelfSustained == value)
                    return;
                IAsyncDisposable objLocker2 = await _objCharacter.LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    _blnSelfSustained = value;
                    await OnPropertyChangedAsync(nameof(SelfSustained), token).ConfigureAwait(false);
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
        /// Force of the sustained spell
        /// </summary>
        public int Force
        {
            get
            {
                using (_objCharacter.LockObject.EnterReadLock())
                    return _intForce;
            }
            set
            {
                using (_objCharacter.LockObject.EnterReadLock())
                {
                    if (_intForce == value)
                        return;
                }
                using (_objCharacter.LockObject.EnterUpgradeableReadLock())
                {
                    if (_intForce == value)
                        return;
                    using (_objCharacter.LockObject.EnterWriteLock())
                    {
                        if (Interlocked.Exchange(ref _intForce, value) != value)
                            OnPropertyChanged();
                    }
                }
            }
        }

        /// <summary>
        /// Force of the sustained spell
        /// </summary>
        public async Task<int> GetForceAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await _objCharacter.LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _intForce;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Force of the sustained spell
        /// </summary>
        public async Task SetForceAsync(int value, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await _objCharacter.LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_intForce == value)
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            objLocker = await _objCharacter.LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_intForce == value)
                    return;
                IAsyncDisposable objLocker2 = await _objCharacter.LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    if (Interlocked.Exchange(ref _intForce, value) != value)
                        await OnPropertyChangedAsync(nameof(Force), token).ConfigureAwait(false);
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
        /// The Net Hits the Sustained Spell has
        /// </summary>
        public int NetHits
        {
            get
            {
                using (_objCharacter.LockObject.EnterReadLock())
                    return _intNetHits;
            }
            set
            {
                using (_objCharacter.LockObject.EnterReadLock())
                {
                    if (_intNetHits == value)
                        return;
                }
                using (_objCharacter.LockObject.EnterUpgradeableReadLock())
                {
                    if (_intNetHits == value)
                        return;
                    using (_objCharacter.LockObject.EnterWriteLock())
                    {
                        if (Interlocked.Exchange(ref _intNetHits, value) != value)
                            OnPropertyChanged();
                    }
                }
            }
        }

        /// <summary>
        /// The Net Hits the Sustained Spell has
        /// </summary>
        public async Task<int> GetNetHitsAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await _objCharacter.LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _intNetHits;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// The Net Hits the Sustained Spell has
        /// </summary>
        public async Task SetNetHitsAsync(int value, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await _objCharacter.LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_intNetHits == value)
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            objLocker = await _objCharacter.LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_intNetHits == value)
                    return;
                IAsyncDisposable objLocker2 = await _objCharacter.LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    if (Interlocked.Exchange(ref _intNetHits, value) != value)
                        await OnPropertyChangedAsync(nameof(NetHits), token).ConfigureAwait(false);
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

        public string InternalId => _guiID.ToString("D", GlobalSettings.InvariantCultureInfo);

        public IHasInternalId LinkedObject => _objLinkedObject;

        public Improvement.ImprovementSource LinkedObjectType => _eLinkedObjectType;

        /// <summary>
        /// The name of the object as it should be displayed on printouts (translated name only).
        /// </summary>
        public string DisplayNameShort(string strLanguage)
        {
            // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
            switch (_eLinkedObjectType)
            {
                case Improvement.ImprovementSource.Spell:
                    return (_objLinkedObject as Spell)?.DisplayNameShort(strLanguage);

                case Improvement.ImprovementSource.ComplexForm:
                    return (_objLinkedObject as ComplexForm)?.DisplayNameShort(strLanguage);

                case Improvement.ImprovementSource.CritterPower:
                    return (_objLinkedObject as CritterPower)?.DisplayNameShort(strLanguage);
            }
            return LanguageManager.GetString("String_Unknown", strLanguage);
        }

        /// <summary>
        /// The name of the object as it should be displayed on printouts (translated name only).
        /// </summary>
        public Task<string> DisplayNameShortAsync(string strLanguage, CancellationToken token = default)
        {
            if (token.IsCancellationRequested)
                return Task.FromCanceled<string>(token);
            // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
            switch (_eLinkedObjectType)
            {
                case Improvement.ImprovementSource.Spell:
                    if (_objLinkedObject is Spell objSpell)
                        return objSpell.DisplayNameShortAsync(strLanguage, token);
                    break;

                case Improvement.ImprovementSource.ComplexForm:
                    if (_objLinkedObject is ComplexForm objComplexForm)
                        return objComplexForm.DisplayNameShortAsync(strLanguage, token);
                    break;

                case Improvement.ImprovementSource.CritterPower:
                    if (_objLinkedObject is CritterPower objCritterPower)
                        return objCritterPower.DisplayNameShortAsync(strLanguage, token);
                    break;
            }
            return LanguageManager.GetStringAsync("String_Unknown", strLanguage, token: token);
        }

        /// <summary>
        /// The name of the object as it should be displayed in lists.
        /// </summary>
        public string DisplayName(string strLanguage)
        {
            // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
            switch (_eLinkedObjectType)
            {
                case Improvement.ImprovementSource.Spell:
                    return (_objLinkedObject as Spell)?.DisplayName(strLanguage);

                case Improvement.ImprovementSource.ComplexForm:
                    return (_objLinkedObject as ComplexForm)?.DisplayName(strLanguage);

                case Improvement.ImprovementSource.CritterPower:
                    return (_objLinkedObject as CritterPower)?.DisplayName(strLanguage);
            }
            return LanguageManager.GetString("String_Unknown", strLanguage);
        }

        /// <summary>
        /// The name of the object as it should be displayed on printouts (translated name only).
        /// </summary>
        public Task<string> DisplayNameAsync(string strLanguage, CancellationToken token = default)
        {
            if (token.IsCancellationRequested)
                return Task.FromCanceled<string>(token);
            // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
            switch (_eLinkedObjectType)
            {
                case Improvement.ImprovementSource.Spell:
                    if (_objLinkedObject is Spell objSpell)
                        return objSpell.DisplayNameAsync(strLanguage, token);
                    break;

                case Improvement.ImprovementSource.ComplexForm:
                    if (_objLinkedObject is ComplexForm objComplexForm)
                        return objComplexForm.DisplayNameAsync(strLanguage, token);
                    break;

                case Improvement.ImprovementSource.CritterPower:
                    if (_objLinkedObject is CritterPower objCritterPower)
                        return objCritterPower.DisplayNameAsync(strLanguage, token);
                    break;
            }
            return LanguageManager.GetStringAsync("String_Unknown", strLanguage, token: token);
        }

        public string CurrentDisplayName => DisplayName(GlobalSettings.Language);

        public Task<string> GetCurrentDisplayNameAsync(CancellationToken token = default) =>
            DisplayNameAsync(GlobalSettings.Language, token);

        public string Name
        {
            get
            {
                // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
                switch (_eLinkedObjectType)
                {
                    case Improvement.ImprovementSource.Spell:
                        return (_objLinkedObject as Spell)?.Name;

                    case Improvement.ImprovementSource.ComplexForm:
                        return (_objLinkedObject as ComplexForm)?.Name;

                    case Improvement.ImprovementSource.CritterPower:
                        return (_objLinkedObject as CritterPower)?.Name;
                }
                return LanguageManager.GetString("String_Unknown", GlobalSettings.DefaultLanguage);
            }
        }

        public Task<string> GetNameAsync(CancellationToken token = default)
        {
            if (token.IsCancellationRequested)
                return Task.FromCanceled<string>(token);
            // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
            switch (_eLinkedObjectType)
            {
                case Improvement.ImprovementSource.Spell:
                    if (_objLinkedObject is Spell objSpell)
                        return objSpell.GetNameAsync(token);
                    break;

                case Improvement.ImprovementSource.ComplexForm:
                    if (_objLinkedObject is ComplexForm objComplexForm)
                        return objComplexForm.GetNameAsync(token);
                    break;

                case Improvement.ImprovementSource.CritterPower:
                    if (_objLinkedObject is CritterPower objCritterPower)
                        return Task.FromResult(objCritterPower.Name);
                    break;
            }
            return LanguageManager.GetStringAsync("String_Unknown", GlobalSettings.DefaultLanguage, token: token);
        }

        public bool HasSustainingPenalty => LinkedObjectType != Improvement.ImprovementSource.CritterPower && SelfSustained;

        public Task<bool> GetHasSustainingPenaltyAsync(CancellationToken token = default)
        {
            if (token.IsCancellationRequested)
                return Task.FromCanceled<bool>(token);
            if (LinkedObjectType != Improvement.ImprovementSource.CritterPower)
                return Task.FromResult(false);
            return GetSelfSustainedAsync(token);
        }

        #endregion Properties

        public event PropertyChangedEventHandler PropertyChanged;

        private readonly ConcurrentHashSet<PropertyChangedAsyncEventHandler> _setPropertyChangedAsync =
            new ConcurrentHashSet<PropertyChangedAsyncEventHandler>();

        public event PropertyChangedAsyncEventHandler PropertyChangedAsync
        {
            add => _setPropertyChangedAsync.TryAdd(value);
            remove => _setPropertyChangedAsync.Remove(value);
        }

        [NotifyPropertyChangedInvocator]
        public void OnPropertyChanged([CallerMemberName] string strPropertyName = null)
        {
            PropertyChangedEventArgs objArgs = new PropertyChangedEventArgs(strPropertyName);
            if (_setPropertyChangedAsync.Count > 0)
                Utils.RunWithoutThreadLock(_setPropertyChangedAsync.Select(x => new Func<Task>(() => x.Invoke(this, objArgs))));
            if (PropertyChanged != null)
                Utils.RunOnMainThread(() => PropertyChanged?.Invoke(this, objArgs));
            if (strPropertyName == nameof(SelfSustained) && _objCharacter != null)
                _objCharacter.RefreshSustainingPenalties();
        }

        public async Task OnPropertyChangedAsync(string strPropertyName, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            PropertyChangedEventArgs objArgs = new PropertyChangedEventArgs(strPropertyName);
            if (_setPropertyChangedAsync.Count > 0)
                await ParallelExtensions.ForEachAsync(_setPropertyChangedAsync, x => x.Invoke(this, objArgs, token), token).ConfigureAwait(false);
            token.ThrowIfCancellationRequested();
            if (PropertyChanged != null)
                await Utils.RunOnMainThreadAsync(() => PropertyChanged?.Invoke(this, objArgs), token).ConfigureAwait(false);
            token.ThrowIfCancellationRequested();
            if (strPropertyName == nameof(SelfSustained) && _objCharacter != null)
                await _objCharacter.RefreshSustainingPenaltiesAsync(token).ConfigureAwait(false);
        }
    }
}
