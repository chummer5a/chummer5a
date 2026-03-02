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
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Chummer.Annotations;

namespace Chummer
{
    [DebuggerDisplay("{DisplayName(null, \"en-us\")}")]
    public sealed class CalendarWeek : IHasInternalId, IComparable, INotifyMultiplePropertiesChangedAsync, IEquatable<CalendarWeek>, IComparable<CalendarWeek>, IHasNotes, IHasLockObject
    {
        private Guid _guiID;
        private bool _blnIsLongYear;
        private bool _blnIsLeapYear = true;
        private int _intYear = 2072;
        private int _intWeek = 1;
        private string _strNotes = string.Empty;
        private Color _colNotes = ColorManager.HasNotesColor;

        public event PropertyChangedEventHandler PropertyChanged;

        private readonly ConcurrentHashSet<PropertyChangedAsyncEventHandler> _setPropertyChangedAsync =
            new ConcurrentHashSet<PropertyChangedAsyncEventHandler>();

        public event PropertyChangedAsyncEventHandler PropertyChangedAsync
        {
            add => _setPropertyChangedAsync.TryAdd(value);
            remove => _setPropertyChangedAsync.Remove(value);
        }

        public event MultiplePropertiesChangedEventHandler MultiplePropertiesChanged;

        private readonly ConcurrentHashSet<MultiplePropertiesChangedAsyncEventHandler> _setMultiplePropertiesChangedAsync =
            new ConcurrentHashSet<MultiplePropertiesChangedAsyncEventHandler>();

        public event MultiplePropertiesChangedAsyncEventHandler MultiplePropertiesChangedAsync
        {
            add => _setMultiplePropertiesChangedAsync.TryAdd(value);
            remove => _setMultiplePropertiesChangedAsync.Remove(value);
        }

        [NotifyPropertyChangedInvocator]
        public void OnPropertyChanged([CallerMemberName] string strPropertyName = null)
        {
            this.OnMultiplePropertyChanged(strPropertyName);
        }

        public Task OnPropertyChangedAsync(string strPropertyName, CancellationToken token = default)
        {
            return this.OnMultiplePropertyChangedAsync(token, strPropertyName);
        }

        public void OnMultiplePropertiesChanged(IReadOnlyCollection<string> lstPropertyNames)
        {
            using (LockObject.EnterUpgradeableReadLock())
            {
                if (_setMultiplePropertiesChangedAsync.Count > 0)
                {
                    MultiplePropertiesChangedEventArgs objArgs =
                        new MultiplePropertiesChangedEventArgs(lstPropertyNames);
                    List<Func<Task>> lstFuncs = new List<Func<Task>>(_setMultiplePropertiesChangedAsync.Count);
                    foreach (MultiplePropertiesChangedAsyncEventHandler objEvent in _setMultiplePropertiesChangedAsync)
                    {
                        lstFuncs.Add(() => objEvent.Invoke(this, objArgs));
                    }

                    Utils.RunWithoutThreadLock(lstFuncs);
                    if (MultiplePropertiesChanged != null)
                    {
                        Utils.RunOnMainThread(() =>
                        {
                            // ReSharper disable once AccessToModifiedClosure
                            MultiplePropertiesChanged?.Invoke(this, objArgs);
                        });
                    }
                }
                else if (MultiplePropertiesChanged != null)
                {
                    MultiplePropertiesChangedEventArgs objArgs =
                        new MultiplePropertiesChangedEventArgs(lstPropertyNames);
                    Utils.RunOnMainThread(() =>
                    {
                        // ReSharper disable once AccessToModifiedClosure
                        MultiplePropertiesChanged?.Invoke(this, objArgs);
                    });
                }

                if (_setPropertyChangedAsync.Count > 0)
                {
                    List<PropertyChangedEventArgs> lstArgsList = lstPropertyNames.Select(x => new PropertyChangedEventArgs(x)).ToList();
                    List<Func<Task>> lstFuncs = new List<Func<Task>>(lstArgsList.Count * _setPropertyChangedAsync.Count);
                    foreach (PropertyChangedAsyncEventHandler objEvent in _setPropertyChangedAsync)
                    {
                        foreach (PropertyChangedEventArgs objArg in lstArgsList)
                            lstFuncs.Add(() => objEvent.Invoke(this, objArg));
                    }

                    Utils.RunWithoutThreadLock(lstFuncs);
                    if (PropertyChanged != null)
                    {
                        Utils.RunOnMainThread(() =>
                        {
                            if (PropertyChanged != null)
                            {
                                // ReSharper disable once AccessToModifiedClosure
                                foreach (PropertyChangedEventArgs objArgs in lstArgsList)
                                {
                                    PropertyChanged.Invoke(this, objArgs);
                                }
                            }
                        });
                    }
                }
                else if (PropertyChanged != null)
                {
                    Utils.RunOnMainThread(() =>
                    {
                        if (PropertyChanged != null)
                        {
                            foreach (string strPropertyToChange in lstPropertyNames)
                            {
                                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(strPropertyToChange));
                            }
                        }
                    });
                }
            }
        }

        public async Task OnMultiplePropertiesChangedAsync(IReadOnlyCollection<string> lstPropertyNames, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_setMultiplePropertiesChangedAsync.Count > 0)
                {
                    MultiplePropertiesChangedEventArgs objArgs =
                        new MultiplePropertiesChangedEventArgs(lstPropertyNames);
                    await ParallelExtensions.ForEachAsync(_setMultiplePropertiesChangedAsync, objEvent => objEvent.Invoke(this, objArgs, token), token).ConfigureAwait(false);
                    if (MultiplePropertiesChanged != null)
                    {
                        await Utils.RunOnMainThreadAsync(() =>
                        {
                            // ReSharper disable once AccessToModifiedClosure
                            MultiplePropertiesChanged?.Invoke(this, objArgs);
                        }, token: token).ConfigureAwait(false);
                    }
                }
                else if (MultiplePropertiesChanged != null)
                {
                    MultiplePropertiesChangedEventArgs objArgs =
                        new MultiplePropertiesChangedEventArgs(lstPropertyNames);
                    await Utils.RunOnMainThreadAsync(() =>
                    {
                        // ReSharper disable once AccessToModifiedClosure
                        MultiplePropertiesChanged?.Invoke(this, objArgs);
                    }, token: token).ConfigureAwait(false);
                }

                if (_setPropertyChangedAsync.Count > 0)
                {
                    List<PropertyChangedEventArgs> lstArgsList =
                        lstPropertyNames.Select(x => new PropertyChangedEventArgs(x)).ToList();
                    List<ValueTuple<PropertyChangedAsyncEventHandler, PropertyChangedEventArgs>> lstAsyncEventsList
                            = new List<ValueTuple<PropertyChangedAsyncEventHandler, PropertyChangedEventArgs>>(lstArgsList.Count * _setPropertyChangedAsync.Count);
                    foreach (PropertyChangedAsyncEventHandler objEvent in _setPropertyChangedAsync)
                    {
                        foreach (PropertyChangedEventArgs objArg in lstArgsList)
                        {
                            lstAsyncEventsList.Add(new ValueTuple<PropertyChangedAsyncEventHandler, PropertyChangedEventArgs>(objEvent, objArg));
                        }
                    }
                    await ParallelExtensions.ForEachAsync(lstAsyncEventsList, tupEvent => tupEvent.Item1.Invoke(this, tupEvent.Item2, token), token).ConfigureAwait(false);

                    if (PropertyChanged != null)
                    {
                        await Utils.RunOnMainThreadAsync(() =>
                        {
                            if (PropertyChanged != null)
                            {
                                // ReSharper disable once AccessToModifiedClosure
                                foreach (PropertyChangedEventArgs objArgs in lstArgsList)
                                {
                                    token.ThrowIfCancellationRequested();
                                    PropertyChanged.Invoke(this, objArgs);
                                }
                            }
                        }, token).ConfigureAwait(false);
                    }
                }
                else if (PropertyChanged != null)
                {
                    await Utils.RunOnMainThreadAsync(() =>
                    {
                        if (PropertyChanged != null)
                        {
                            // ReSharper disable once AccessToModifiedClosure
                            foreach (string strPropertyToChange in lstPropertyNames)
                            {
                                token.ThrowIfCancellationRequested();
                                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(strPropertyToChange));
                            }
                        }
                    }, token).ConfigureAwait(false);
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        #region Constructor, Save, Load, and Print Methods

        /// <summary>
        /// Create a new CalendarWeek entry, following the ISO 8601 standard
        /// </summary>
        public CalendarWeek()
        {
            // Create the GUID for the new CalendarWeek.
            _guiID = Guid.NewGuid();
        }

        /// <summary>
        /// Create a new CalendarWeek entry, following the ISO 8601 standard
        /// </summary>
        public CalendarWeek(int intYear, int intWeek)
        {
            if (intYear <= 0)
                --intYear; // Account for year 0 not existing
            // Create the GUID for the new CalendarWeek.
            _guiID = Guid.NewGuid();
            _intYear = intYear;
            _blnIsLongYear = intYear.IsYearLongYear(out _blnIsLeapYear);
            int intWeeksInYear = IsLongYear ? 53 : 52;
            if (intWeek < 1 || intWeek > intWeeksInYear)
            {
                while (intWeek < 1)
                {
                    --intYear;
                    _blnIsLongYear = intYear.IsYearLongYear(out _blnIsLeapYear);
                    intWeek += _blnIsLongYear ? 53 : 52;
                }
                while (intWeek > intWeeksInYear)
                {
                    ++intYear;
                    intWeek -= intWeeksInYear;
                    _blnIsLongYear = intYear.IsYearLongYear(out _blnIsLeapYear);
                    intWeeksInYear = _blnIsLongYear ? 53 : 52;
                }
                if (intYear <= 0)
                    --intYear; // Account for there being no year 0
                _intYear = intYear;
            }
            _intWeek = intWeek;
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
                objWriter.WriteStartElement("week");
                objWriter.WriteElementString("guid", _guiID.ToString("D", GlobalSettings.InvariantCultureInfo));
                objWriter.WriteElementString("year", _intYear.ToString(GlobalSettings.InvariantCultureInfo));
                objWriter.WriteElementString("week", _intWeek.ToString(GlobalSettings.InvariantCultureInfo));
                objWriter.WriteElementString("notes", _strNotes.CleanOfXmlInvalidUnicodeChars());
                objWriter.WriteElementString("notesColor", ColorTranslator.ToHtml(_colNotes));
                objWriter.WriteEndElement();
            }
        }

        /// <summary>
        /// Load the Calendar Week from the XmlNode.
        /// </summary>
        /// <param name="objNode">XmlNode to load.</param>
        public void Load(XmlNode objNode)
        {
            using (LockObject.EnterWriteLock())
            {
                objNode.TryGetField("guid", Guid.TryParse, out _guiID);
                if (objNode.TryGetInt32FieldQuickly("year", ref _intYear))
                    _blnIsLongYear = _intYear.IsYearLongYear(out _blnIsLeapYear);
                objNode.TryGetInt32FieldQuickly("week", ref _intWeek);
                objNode.TryGetMultiLineStringFieldQuickly("notes", ref _strNotes);
                string sNotesColor = ColorTranslator.ToHtml(ColorManager.HasNotesColor);
                objNode.TryGetStringFieldQuickly("notesColor", ref sNotesColor);
                _colNotes = ColorTranslator.FromHtml(sNotesColor);
            }
        }

        /// <summary>
        /// Load the Calendar Week from the XmlNode.
        /// </summary>
        /// <param name="objNode">XmlNode to load.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public async Task LoadAsync(XmlNode objNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                objNode.TryGetField("guid", Guid.TryParse, out _guiID);
                if (objNode.TryGetInt32FieldQuickly("year", ref _intYear))
                    _blnIsLongYear = _intYear.IsYearLongYear(out _blnIsLeapYear);
                objNode.TryGetInt32FieldQuickly("week", ref _intWeek);
                objNode.TryGetMultiLineStringFieldQuickly("notes", ref _strNotes);
                string sNotesColor = ColorTranslator.ToHtml(ColorManager.HasNotesColor);
                objNode.TryGetStringFieldQuickly("notesColor", ref sNotesColor);
                _colNotes = ColorTranslator.FromHtml(sNotesColor);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Print the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        /// <param name="objCulture">Culture in which to print numbers.</param>
        /// <param name="blnPrintNotes">Whether to print notes attached to the CalendarWeek.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public async Task Print(XmlWriter objWriter, CultureInfo objCulture, bool blnPrintNotes = true, CancellationToken token = default)
        {
            if (objWriter == null)
                return;
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                // <week>
                XmlElementWriteHelper objBaseElement
                    = await objWriter.StartElementAsync("week", token: token).ConfigureAwait(false);
                try
                {
                    await objWriter.WriteElementStringAsync("guid", InternalId, token: token).ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("year", (await GetYearAsync(token).ConfigureAwait(false)).ToString(objCulture), token: token)
                        .ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("week", (await GetWeekAsync(token).ConfigureAwait(false)).ToString(objCulture), token: token)
                        .ConfigureAwait(false);
                    if (blnPrintNotes)
                        await objWriter.WriteElementStringAsync("notes", await GetNotesAsync(token).ConfigureAwait(false), token: token).ConfigureAwait(false);
                }
                finally
                {
                    // </week>
                    await objBaseElement.DisposeAsync().ConfigureAwait(false);
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        #endregion Constructor, Save, Load, and Print Methods

        #region Properties

        /// <summary>
        /// Internal identifier which will be used to identify this Calendar Week in the Improvement system.
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
        /// Year of the leap-week calendar following ISO 8601 standard (so there can be discrepancies around last and first weeks of a year).
        /// </summary>
        public int Year
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _intYear;
            }
            set
            {
                if (value == 0)
                    throw new ArgumentOutOfRangeException(nameof(value));
                using (LockObject.EnterReadLock())
                {
                    if (_intYear == value)
                        return;
                }
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (_intYear == value)
                        return;
                    using (LockObject.EnterWriteLock())
                    {
                        if (Interlocked.Exchange(ref _intYear, value) != value)
                        {
                            bool blnOldIsLongYear = _blnIsLongYear;
                            bool blnOldIsLeapYear = _blnIsLeapYear;
                            _blnIsLongYear = value.IsYearLongYear(out _blnIsLeapYear);
                            if (blnOldIsLongYear != _blnIsLongYear)
                            {
                                if (blnOldIsLongYear && !_blnIsLongYear && _intWeek == 53)
                                {
                                    --_intWeek;
                                    if (blnOldIsLeapYear != _blnIsLeapYear)
                                        this.OnMultiplePropertyChanged(nameof(Year), nameof(IsLongYear), nameof(IsLeapYear), nameof(Week));
                                    else
                                        this.OnMultiplePropertyChanged(nameof(Year), nameof(IsLongYear), nameof(Week));
                                }
                                else if (blnOldIsLeapYear != _blnIsLeapYear)
                                    this.OnMultiplePropertyChanged(nameof(Year), nameof(IsLongYear), nameof(IsLeapYear));
                                else
                                    this.OnMultiplePropertyChanged(nameof(Year), nameof(IsLongYear));
                            }
                            else if (blnOldIsLeapYear != _blnIsLeapYear)
                                this.OnMultiplePropertyChanged(nameof(Year), nameof(IsLeapYear));
                            else
                                OnPropertyChanged();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Year of the leap-week calendar following ISO 8601 standard (so there can be discrepancies around last and first weeks of a year).
        /// </summary>
        public async Task<int> GetYearAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _intYear;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }

        }

        /// <summary>
        /// Year of the leap-week calendar following ISO 8601 standard (so there can be discrepancies around last and first weeks of a year).
        /// </summary>
        public async Task SetYearAsync(int value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (value == 0)
                throw new ArgumentOutOfRangeException(nameof(value));
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_intYear == value)
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
                if (_intYear == value)
                    return;
                IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    if (Interlocked.Exchange(ref _intYear, value) != value)
                    {
                        bool blnOldIsLongYear = _blnIsLongYear;
                        bool blnOldIsLeapYear = _blnIsLeapYear;
                        _blnIsLongYear = value.IsYearLongYear(out _blnIsLeapYear);
                        if (blnOldIsLongYear != _blnIsLongYear)
                        {
                            if (blnOldIsLongYear && !_blnIsLongYear && _intWeek == 53)
                            {
                                --_intWeek;
                                if (blnOldIsLeapYear != _blnIsLeapYear)
                                    this.OnMultiplePropertyChanged(nameof(Year), nameof(IsLongYear), nameof(IsLeapYear), nameof(Week));
                                else
                                    this.OnMultiplePropertyChanged(nameof(Year), nameof(IsLongYear), nameof(Week));
                            }
                            else if (blnOldIsLeapYear != _blnIsLeapYear)
                                this.OnMultiplePropertyChanged(nameof(Year), nameof(IsLongYear), nameof(IsLeapYear));
                            else
                                this.OnMultiplePropertyChanged(nameof(Year), nameof(IsLongYear));
                        }
                        else if (blnOldIsLeapYear != _blnIsLeapYear)
                            this.OnMultiplePropertyChanged(nameof(Year), nameof(IsLeapYear));
                        else
                            OnPropertyChanged();
                    }
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

        public string CurrentDisplayName => DisplayName(GlobalSettings.CultureInfo, GlobalSettings.Language);

        /// <summary>
        /// Year and Week to display, following ISO 8601.
        /// </summary>
        public string DisplayName(CultureInfo objCulture, string strLanguage)
        {
            using (LockObject.EnterReadLock())
            {
                if (objCulture == null)
                    objCulture = GlobalSettings.CultureInfo;
                string strReturn = string.Format(
                    objCulture, LanguageManager.GetString("String_WeekDisplay", strLanguage)
                    , Year
                    , Week);
                return strReturn;
            }
        }

        public Task<string> GetCurrentDisplayNameAsync(CancellationToken token = default) => DisplayNameAsync(GlobalSettings.CultureInfo, GlobalSettings.Language, token);

        /// <summary>
        /// Year and Week to display, following ISO 8601
        /// </summary>
        public async Task<string> DisplayNameAsync(CultureInfo objCulture, string strLanguage, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (objCulture == null)
                    objCulture = GlobalSettings.CultureInfo;
                string strReturn = string.Format(
                    objCulture, await LanguageManager.GetStringAsync("String_WeekDisplay", strLanguage, token: token)
                                                     .ConfigureAwait(false)
                    , await GetYearAsync(token).ConfigureAwait(false)
                    , await GetWeekAsync(token).ConfigureAwait(false));
                return strReturn;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Week of the year, following ISO 8601.
        /// </summary>
        public int Week
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _intWeek;
            }
            set
            {
                using (LockObject.EnterReadLock())
                {
                    if (_intWeek == value)
                        return;
                }
                using (LockObject.EnterUpgradeableReadLock())
                {
                    int intWeeksInYear = IsLongYear ? 53 : 52;
                    if (_intWeek == value)
                        return;
                    if (value < 1 || value > intWeeksInYear)
                    {
                        int intNewYear = Year;
                        while (value < 1)
                        {
                            --intNewYear;
                            value += intNewYear.IsYearLongYear() ? 53 : 52;
                        }
                        while (value > intWeeksInYear)
                        {
                            ++intNewYear;
                            value -= intWeeksInYear;
                            intWeeksInYear = intNewYear.IsYearLongYear() ? 53 : 52;
                        }
                        if (intNewYear <= 0)
                            --intNewYear; // Account for there being no year 0
                        using (LockObject.EnterWriteLock())
                        {
                            if (Interlocked.Exchange(ref _intYear, intNewYear) != intNewYear)
                            {
                                bool blnOldIsLongYear = _blnIsLongYear;
                                bool blnOldIsLeapYear = _blnIsLeapYear;
                                _blnIsLongYear = value.IsYearLongYear(out _blnIsLeapYear);
                                if (Interlocked.Exchange(ref _intWeek, value) != value)
                                {
                                    if (blnOldIsLongYear != _blnIsLongYear)
                                    {
                                        if (blnOldIsLeapYear != _blnIsLeapYear)
                                            this.OnMultiplePropertyChanged(nameof(Year), nameof(IsLongYear), nameof(IsLeapYear), nameof(Week));
                                        else
                                            this.OnMultiplePropertyChanged(nameof(Year), nameof(IsLongYear), nameof(Week));
                                    }
                                    else if (blnOldIsLeapYear != _blnIsLeapYear)
                                        this.OnMultiplePropertyChanged(nameof(Year), nameof(IsLeapYear), nameof(Week));
                                    else
                                        this.OnMultiplePropertyChanged(nameof(Year), nameof(Week));
                                }
                                else if (blnOldIsLongYear != _blnIsLongYear)
                                {
                                    if (blnOldIsLeapYear != _blnIsLeapYear)
                                        this.OnMultiplePropertyChanged(nameof(Year), nameof(IsLongYear), nameof(IsLeapYear));
                                    else
                                        this.OnMultiplePropertyChanged(nameof(Year), nameof(IsLongYear));
                                }
                                else if (blnOldIsLeapYear != _blnIsLeapYear)
                                    this.OnMultiplePropertyChanged(nameof(Year), nameof(IsLeapYear));
                                else
                                    OnPropertyChanged(nameof(Year));
                            }
                            else if (Interlocked.Exchange(ref _intWeek, value) != value)
                            {
                                OnPropertyChanged();
                            }
                        }
                    }
                    else
                    {
                        if (Interlocked.Exchange(ref _intWeek, value) != value)
                            OnPropertyChanged();
                    }
                }
            }
        }

        public async Task<int> GetWeekAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _intWeek;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }

        }

        public async Task SetWeekAsync(int value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_intWeek == value)
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
                int intWeeksInYear = await GetIsLongYearAsync(token).ConfigureAwait(false) ? 53 : 52;
                if (value < 1 || value > intWeeksInYear)
                {
                    int intNewYear = await GetYearAsync(token).ConfigureAwait(false);
                    while (value < 1)
                    {
                        --intNewYear;
                        value += intNewYear.IsYearLongYear() ? 53 : 52;
                    }
                    while (value > intWeeksInYear)
                    {
                        ++intNewYear;
                        value -= intWeeksInYear;
                        intWeeksInYear = intNewYear.IsYearLongYear() ? 53 : 52;
                    }
                    if (intNewYear <= 0)
                        --intNewYear; // Account for there being no year 0
                    IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                    try
                    {
                        token.ThrowIfCancellationRequested();
                        if (Interlocked.Exchange(ref _intYear, intNewYear) != intNewYear)
                        {
                            bool blnOldIsLongYear = _blnIsLongYear;
                            bool blnOldIsLeapYear = _blnIsLeapYear;
                            _blnIsLongYear = value.IsYearLongYear(out _blnIsLeapYear);
                            if (Interlocked.Exchange(ref _intWeek, value) != value)
                            {
                                if (blnOldIsLongYear != _blnIsLongYear)
                                {
                                    if (blnOldIsLeapYear != _blnIsLeapYear)
                                        await this.OnMultiplePropertyChangedAsync(token, nameof(Year), nameof(IsLongYear), nameof(IsLeapYear), nameof(Week)).ConfigureAwait(false);
                                    else
                                        await this.OnMultiplePropertyChangedAsync(token, nameof(Year), nameof(IsLongYear), nameof(Week)).ConfigureAwait(false);
                                }
                                else if (blnOldIsLeapYear != _blnIsLeapYear)
                                    await this.OnMultiplePropertyChangedAsync(token, nameof(Year), nameof(IsLeapYear), nameof(Week)).ConfigureAwait(false);
                                else
                                    await this.OnMultiplePropertyChangedAsync(token, nameof(Year), nameof(Week)).ConfigureAwait(false);
                            }
                            else if (blnOldIsLongYear != _blnIsLongYear)
                            {
                                if (blnOldIsLeapYear != _blnIsLeapYear)
                                    await this.OnMultiplePropertyChangedAsync(token, nameof(Year), nameof(IsLongYear), nameof(IsLeapYear)).ConfigureAwait(false);
                                else
                                    await this.OnMultiplePropertyChangedAsync(token, nameof(Year), nameof(IsLongYear)).ConfigureAwait(false);
                            }
                            else if (blnOldIsLeapYear != _blnIsLeapYear)
                                await this.OnMultiplePropertyChangedAsync(token, nameof(Year), nameof(IsLeapYear));
                            else
                                await OnPropertyChangedAsync(nameof(Year), token).ConfigureAwait(false);
                        }
                        else if (Interlocked.Exchange(ref _intWeek, value) != value)
                        {
                            await OnPropertyChangedAsync(nameof(Week), token).ConfigureAwait(false);
                        }
                    }
                    finally
                    {
                        await objLocker2.DisposeAsync().ConfigureAwait(false);
                    }
                }
                else
                {
                    if (Interlocked.Exchange(ref _intWeek, value) != value)
                        await OnPropertyChangedAsync(nameof(Week), token).ConfigureAwait(false);
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public async Task ModifyWeekAsync(int value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (value == 0)
                return;
            IAsyncDisposable objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                await SetWeekAsync(await GetWeekAsync(token).ConfigureAwait(false), token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public bool IsLongYear
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _blnIsLongYear;
            }
        }

        public async Task<bool> GetIsLongYearAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _blnIsLongYear;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public bool IsLeapYear
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _blnIsLeapYear;
            }
        }

        public async Task<bool> GetIsLeapYearAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _blnIsLeapYear;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
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
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (Interlocked.Exchange(ref _strNotes, value) != value)
                        OnPropertyChanged();
                }
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
            IAsyncDisposable objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                // No need to write lock because interlocked guarantees safety
                if (Interlocked.Exchange(ref _strNotes, value) == value)
                    return;
                await OnPropertyChangedAsync(nameof(Notes), token).ConfigureAwait(false);
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
                        _colNotes = value;
                    OnPropertyChanged();
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
                    await OnPropertyChangedAsync(nameof(NotesColor), token).ConfigureAwait(false);
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

        public int CompareTo(object obj)
        {
            if (obj is CalendarWeek objWeek)
                return CompareTo(objWeek);
            return -string.Compare(CurrentDisplayName, obj?.ToString() ?? string.Empty, false, GlobalSettings.CultureInfo);
        }

        public int CompareTo(CalendarWeek other)
        {
            using (LockObject.EnterReadLock())
            using (other.LockObject.EnterReadLock())
            {
                int intReturn = Year.CompareTo(other.Year);
                if (intReturn == 0)
                    intReturn = Week.CompareTo(other.Week);
                return -intReturn;
            }
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
                return true;
            return obj is CalendarWeek objOther && Equals(objOther);
        }

        public bool Equals(CalendarWeek other)
        {
            if (other is null)
                return false;
            if (ReferenceEquals(this, other))
                return true;
            using (LockObject.EnterReadLock())
            using (other.LockObject.EnterReadLock())
                return Year == other.Year && Week == other.Week;
        }

        public override int GetHashCode()
        {
            using (LockObject.EnterReadLock())
                return (InternalId, Year, Week).GetHashCode();
        }

        /// <inheritdoc />
        public ValueTask DisposeAsync()
        {
            // to help the GC
            PropertyChanged = null;
            MultiplePropertiesChanged = null;
            _setPropertyChangedAsync.Clear();
            _setMultiplePropertiesChangedAsync.Clear();
            return LockObject.DisposeAsync();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            // to help the GC
            PropertyChanged = null;
            MultiplePropertiesChanged = null;
            _setPropertyChangedAsync.Clear();
            _setMultiplePropertiesChangedAsync.Clear();
            LockObject.Dispose();
        }

        public static bool operator ==(CalendarWeek left, CalendarWeek right)
        {
            if (left is null)
            {
                return right is null;
            }

            return left.Equals(right);
        }

        public static bool operator !=(CalendarWeek left, CalendarWeek right)
        {
            return !(left == right);
        }

        public static bool operator <(CalendarWeek left, CalendarWeek right)
        {
            return left is null ? !(right is null) : left.CompareTo(right) < 0;
        }

        public static bool operator <=(CalendarWeek left, CalendarWeek right)
        {
            return left is null || left.CompareTo(right) <= 0;
        }

        public static bool operator >(CalendarWeek left, CalendarWeek right)
        {
            return !(left is null) && left.CompareTo(right) > 0;
        }

        public static bool operator >=(CalendarWeek left, CalendarWeek right)
        {
            return left is null ? right is null : left.CompareTo(right) >= 0;
        }

        #endregion Properties

        /// <inheritdoc />
        public AsyncFriendlyReaderWriterLock LockObject { get; } = new AsyncFriendlyReaderWriterLock();
    }
}
