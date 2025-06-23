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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.XPath;
using Chummer.Annotations;

namespace Chummer.Backend.Attributes
{
    public sealed class AttributeSection : INotifyMultiplePropertiesChangedAsync, IHasLockObject, IHasCharacterObject
    {
        private int _intLoading = 1;

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
                HashSet<string> setNamesOfChangedProperties = null;
                try
                {
                    foreach (string strPropertyName in lstPropertyNames)
                    {
                        if (setNamesOfChangedProperties == null)
                            setNamesOfChangedProperties
                                = s_AttributeSectionDependencyGraph.GetWithAllDependents(this, strPropertyName, true);
                        else
                        {
                            foreach (string strLoopChangedProperty in s_AttributeSectionDependencyGraph
                                         .GetWithAllDependentsEnumerable(this, strPropertyName))
                                setNamesOfChangedProperties.Add(strLoopChangedProperty);
                        }
                    }

                    if (setNamesOfChangedProperties == null || setNamesOfChangedProperties.Count == 0)
                        return;

                    if (setNamesOfChangedProperties.Contains(nameof(AttributeCategory)) && _objCharacter.Created)
                    {
                        ResetBindings();
                        ForceAttributePropertyChangedNotificationAll(nameof(CharacterAttrib.MetatypeMaximum),
                            nameof(CharacterAttrib.MetatypeMinimum));
                    }

                    if (_setMultiplePropertiesChangedAsync.Count > 0)
                    {
                        MultiplePropertiesChangedEventArgs objArgs =
                            new MultiplePropertiesChangedEventArgs(setNamesOfChangedProperties.ToArray());
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
                            new MultiplePropertiesChangedEventArgs(setNamesOfChangedProperties.ToArray());
                        Utils.RunOnMainThread(() =>
                        {
                            // ReSharper disable once AccessToModifiedClosure
                            MultiplePropertiesChanged?.Invoke(this, objArgs);
                        });
                    }

                    if (_setPropertyChangedAsync.Count > 0)
                    {
                        List<PropertyChangedEventArgs> lstArgsList = setNamesOfChangedProperties.Select(x => new PropertyChangedEventArgs(x)).ToList();
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
                                // ReSharper disable once AccessToModifiedClosure
                                foreach (string strPropertyToChange in setNamesOfChangedProperties)
                                {
                                    PropertyChanged.Invoke(this, new PropertyChangedEventArgs(strPropertyToChange));
                                }
                            }
                        });
                    }
                }
                finally
                {
                    if (setNamesOfChangedProperties != null)
                        Utils.StringHashSetPool.Return(ref setNamesOfChangedProperties);
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
                HashSet<string> setNamesOfChangedProperties = null;
                try
                {
                    foreach (string strPropertyName in lstPropertyNames)
                    {
                        if (setNamesOfChangedProperties == null)
                            setNamesOfChangedProperties
                                = await s_AttributeSectionDependencyGraph.GetWithAllDependentsAsync(this, strPropertyName, true, token).ConfigureAwait(false);
                        else
                        {
                            foreach (string strLoopChangedProperty in await s_AttributeSectionDependencyGraph
                                         .GetWithAllDependentsEnumerableAsync(this, strPropertyName, token).ConfigureAwait(false))
                                setNamesOfChangedProperties.Add(strLoopChangedProperty);
                        }
                    }

                    if (setNamesOfChangedProperties == null || setNamesOfChangedProperties.Count == 0)
                        return;

                    if (setNamesOfChangedProperties.Contains(nameof(AttributeCategory)) && await _objCharacter.GetCreatedAsync(token).ConfigureAwait(false))
                    {
                        await ResetBindingsAsync(token).ConfigureAwait(false);
                        await ForceAttributePropertyChangedNotificationAllAsync(token,
                            nameof(CharacterAttrib.MetatypeMaximum),
                            nameof(CharacterAttrib.MetatypeMinimum)).ConfigureAwait(false);
                    }

                    if (_setMultiplePropertiesChangedAsync.Count > 0)
                    {
                        MultiplePropertiesChangedEventArgs objArgs =
                            new MultiplePropertiesChangedEventArgs(setNamesOfChangedProperties.ToArray());
                        List<Task> lstTasks = new List<Task>(Utils.MaxParallelBatchSize);
                        int i = 0;
                        foreach (MultiplePropertiesChangedAsyncEventHandler objEvent in _setMultiplePropertiesChangedAsync)
                        {
                            lstTasks.Add(objEvent.Invoke(this, objArgs, token));
                            if (++i < Utils.MaxParallelBatchSize)
                                continue;
                            await Task.WhenAll(lstTasks).ConfigureAwait(false);
                            lstTasks.Clear();
                            i = 0;
                        }

                        await Task.WhenAll(lstTasks).ConfigureAwait(false);
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
                            new MultiplePropertiesChangedEventArgs(setNamesOfChangedProperties.ToArray());
                        await Utils.RunOnMainThreadAsync(() =>
                        {
                            // ReSharper disable once AccessToModifiedClosure
                            MultiplePropertiesChanged?.Invoke(this, objArgs);
                        }, token: token).ConfigureAwait(false);
                    }

                    if (_setPropertyChangedAsync.Count > 0)
                    {
                        List<PropertyChangedEventArgs> lstArgsList = setNamesOfChangedProperties
                            .Select(x => new PropertyChangedEventArgs(x)).ToList();
                        List<Task> lstTasks =
                            new List<Task>(Math.Min(lstArgsList.Count * _setPropertyChangedAsync.Count,
                                Utils.MaxParallelBatchSize));
                        int i = 0;
                        foreach (PropertyChangedAsyncEventHandler objEvent in _setPropertyChangedAsync)
                        {
                            foreach (PropertyChangedEventArgs objArg in lstArgsList)
                            {
                                lstTasks.Add(objEvent.Invoke(this, objArg, token));
                                if (++i < Utils.MaxParallelBatchSize)
                                    continue;
                                await Task.WhenAll(lstTasks).ConfigureAwait(false);
                                lstTasks.Clear();
                                i = 0;
                            }
                        }

                        await Task.WhenAll(lstTasks).ConfigureAwait(false);

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
                                foreach (string strPropertyToChange in setNamesOfChangedProperties)
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
                    if (setNamesOfChangedProperties != null)
                        Utils.StringHashSetPool.Return(ref setNamesOfChangedProperties);
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        private static readonly PropertyDependencyGraph<AttributeSection> s_AttributeSectionDependencyGraph =
            new PropertyDependencyGraph<AttributeSection>(
            );

        private readonly ThreadSafeObservableCollection<CharacterAttrib> _lstAttributes;

        public ThreadSafeObservableCollection<CharacterAttrib> Attributes
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _lstAttributes;
            }
        }

        public async Task<ThreadSafeObservableCollection<CharacterAttrib>> GetAttributesAsync(
            CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _lstAttributes;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        private void InitializeAttributesList(CancellationToken token = default)
        {
            using (LockObject.EnterWriteLock(token))
            {
                _lstAttributes.LockObject.SetParent(token: token);
                try
                {
                    // Not creating a new collection here so that CollectionChanged events from previous list are kept
                    _lstAttributes.Clear();

                    foreach (string strAbbrev in PhysicalAttributes.Concat(MentalAttributes))
                        _lstAttributes.Add(GetAttributeByName(strAbbrev, token));

                    _lstAttributes.Add(GetAttributeByName("EDG", token));

                    if (_objCharacter.MAGEnabled)
                    {
                        _lstAttributes.Add(GetAttributeByName("MAG", token));
                        if (_objCharacter.Settings.MysAdeptSecondMAGAttribute && _objCharacter.IsMysticAdept)
                            _lstAttributes.Add(GetAttributeByName("MAGAdept", token));
                    }

                    if (_objCharacter.RESEnabled)
                    {
                        _lstAttributes.Add(GetAttributeByName("RES", token));
                    }

                    if (_objCharacter.DEPEnabled)
                    {
                        _lstAttributes.Add(GetAttributeByName("DEP", token));
                    }
                }
                finally
                {
                    _lstAttributes.LockObject.SetParent(LockObject, token: token);
                }
            }
        }

        private async Task InitializeAttributesListAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker =
                await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                await _lstAttributes.LockObject.SetParentAsync(token: token).ConfigureAwait(false);
                try
                {
                    // Not creating a new collection here so that CollectionChanged events from previous list are kept
                    await _lstAttributes.ClearAsync(token).ConfigureAwait(false);

                    foreach (string strAbbrev in PhysicalAttributes.Concat(MentalAttributes))
                        await _lstAttributes
                            .AddAsync(await GetAttributeByNameAsync(strAbbrev, token).ConfigureAwait(false), token)
                            .ConfigureAwait(false);

                    await _lstAttributes
                        .AddAsync(await GetAttributeByNameAsync("EDG", token).ConfigureAwait(false), token)
                        .ConfigureAwait(false);

                    if (await _objCharacter.GetMAGEnabledAsync(token).ConfigureAwait(false))
                    {
                        await _lstAttributes
                            .AddAsync(await GetAttributeByNameAsync("MAG", token).ConfigureAwait(false), token)
                            .ConfigureAwait(false);
                        if (await (await _objCharacter.GetSettingsAsync(token).ConfigureAwait(false))
                                .GetMysAdeptSecondMAGAttributeAsync(token).ConfigureAwait(false) &&
                            await _objCharacter.GetIsMysticAdeptAsync(token).ConfigureAwait(false))
                            await _lstAttributes
                                .AddAsync(await GetAttributeByNameAsync("MAGAdept", token).ConfigureAwait(false),
                                    token).ConfigureAwait(false);
                    }

                    if (await _objCharacter.GetRESEnabledAsync(token).ConfigureAwait(false))
                    {
                        await _lstAttributes
                            .AddAsync(await GetAttributeByNameAsync("RES", token).ConfigureAwait(false), token)
                            .ConfigureAwait(false);
                    }

                    if (await _objCharacter.GetDEPEnabledAsync(token).ConfigureAwait(false))
                    {
                        await _lstAttributes
                            .AddAsync(await GetAttributeByNameAsync("DEP", token).ConfigureAwait(false), token)
                            .ConfigureAwait(false);
                    }
                }
                finally
                {
                    await _lstAttributes.LockObject.SetParentAsync(LockObject, token: token).ConfigureAwait(false);
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public static readonly ReadOnlyCollection<string> AttributeStrings = Array.AsReadOnly(new[]
            {"BOD", "AGI", "REA", "STR", "CHA", "INT", "LOG", "WIL", "EDG", "MAG", "MAGAdept", "RES", "ESS", "DEP"});

        public static readonly ReadOnlyCollection<string> PhysicalAttributes = Array.AsReadOnly(new[]
            {"BOD", "AGI", "REA", "STR"});

        public static readonly ReadOnlyCollection<string> MentalAttributes = Array.AsReadOnly(new[]
            {"CHA", "INT", "LOG", "WIL"});

        public static string GetAttributeEnglishName(string strAbbrev)
        {
            switch (strAbbrev)
            {
                case "BOD":
                    return "Body";

                case "AGI":
                    return "Agility";

                case "REA":
                    return "Reaction";

                case "STR":
                    return "Strength";

                case "CHA":
                    return "Charisma";

                case "INT":
                    return "Intuition";

                case "LOG":
                    return "Logic";

                case "WIL":
                    return "Willpower";

                case "EDG":
                    return "Edge";

                case "MAG":
                    return "Magic";

                case "MAGAdept":
                    return "Magic (Adept)";

                case "RES":
                    return "Resonance";

                case "ESS":
                    return "Essence";

                case "DEP":
                    return "Depth";

                default:
                    return string.Empty;
            }
        }

        private readonly ConcurrentDictionary<Tuple<string, CharacterAttrib.AttributeCategory>, CharacterAttrib>
            _dicAttributes = new ConcurrentDictionary<Tuple<string, CharacterAttrib.AttributeCategory>, CharacterAttrib>();
        private readonly Character _objCharacter;
        private CharacterAttrib.AttributeCategory _eAttributeCategory = CharacterAttrib.AttributeCategory.Standard;
        private readonly ThreadSafeObservableCollection<CharacterAttrib> _lstNormalAttributes;
        private readonly ThreadSafeObservableCollection<CharacterAttrib> _lstSpecialAttributes;

        private readonly
            ConcurrentDictionary<string, UiPropertyChangerTracker>
            _dicUIPropertyChangers =
                new ConcurrentDictionary<string, UiPropertyChangerTracker>();

        private readonly struct UiPropertyChangerTracker : IEquatable<UiPropertyChangerTracker>
        {
            public string Abbrev { get; }
            public List<MultiplePropertiesChangedEventHandler> PropertyChangedList { get; }
            public List<MultiplePropertiesChangedAsyncEventHandler> AsyncPropertyChangedList { get; }

            public UiPropertyChangerTracker(string strAbbrev)
            {
                Abbrev = strAbbrev ?? string.Empty;
                PropertyChangedList = new List<MultiplePropertiesChangedEventHandler>();
                AsyncPropertyChangedList = new List<MultiplePropertiesChangedAsyncEventHandler>();
            }

            public bool Equals(UiPropertyChangerTracker other)
            {
                return string.Equals(Abbrev, other.Abbrev, StringComparison.Ordinal)
                       && PropertyChangedList.SequenceEqual(other.PropertyChangedList)
                       && AsyncPropertyChangedList.SequenceEqual(other.AsyncPropertyChangedList);
            }

            public override bool Equals(object obj)
            {
                return obj is UiPropertyChangerTracker other && Equals(other);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    int hashCode = Abbrev.GetHashCode();
                    hashCode = (hashCode * 397) ^ PropertyChangedList.GetHashCode();
                    hashCode = (hashCode * 397) ^ AsyncPropertyChangedList.GetHashCode();
                    return hashCode;
                }
            }
        }

        public AsyncFriendlyReaderWriterLock LockObject { get; }

        public Character CharacterObject => _objCharacter; // readonly member, no locking required

        #region Constructor, Save, Load, Print Methods

        public AttributeSection(Character objCharacter)
        {
            _objCharacter = objCharacter ?? throw new ArgumentNullException(nameof(objCharacter));
            LockObject = objCharacter.LockObject;
            _lstAttributes = new ThreadSafeObservableCollection<CharacterAttrib>(LockObject);
            _lstNormalAttributes = new ThreadSafeObservableCollection<CharacterAttrib>(LockObject);
            _lstSpecialAttributes = new ThreadSafeObservableCollection<CharacterAttrib>(LockObject);
            AttributeList.CollectionChangedAsync += AttributeListOnCollectionChanged;
            AttributeList.BeforeClearCollectionChangedAsync += AttributeListOnBeforeClearCollectionChanged;
            SpecialAttributeList.CollectionChangedAsync += SpecialAttributeListOnCollectionChanged;
            SpecialAttributeList.BeforeClearCollectionChangedAsync += SpecialAttributeListOnBeforeClearCollectionChanged;
            _objCharacter.MultiplePropertiesChangedAsync += CharacterOnMultiplePropertiesChangedAsync;
        }

        private Task CharacterOnMultiplePropertiesChangedAsync(object sender, MultiplePropertiesChangedEventArgs e, CancellationToken token)
        {
            if (token.IsCancellationRequested)
                return Task.FromCanceled(token);
            if (e.PropertyNames.Contains(nameof(Character.MAGEnabled)) ||
                e.PropertyNames.Contains(nameof(Character.RESEnabled)) ||
                e.PropertyNames.Contains(nameof(Character.DEPEnabled)))
            {
                return InitializeAttributesListAsync(token);
            }

            return Task.CompletedTask;
        }

        private async Task SpecialAttributeListOnBeforeClearCollectionChanged(object sender,
            NotifyCollectionChangedEventArgs e, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            foreach (CharacterAttrib objAttribute in e.OldItems)
            {
                if (objAttribute == await GetAttributeByNameAsync(objAttribute.Abbrev, token).ConfigureAwait(false))
                {
                    objAttribute.MultiplePropertiesChangedAsync -= RunExtraAsyncPropertyChanged(objAttribute.Abbrev);
                }

                Tuple<string, CharacterAttrib.AttributeCategory> tupKey =
                    new Tuple<string, CharacterAttrib.AttributeCategory>(objAttribute.Abbrev,
                        objAttribute.MetatypeCategory);
                _dicAttributes.TryRemove(tupKey, out _);
                await objAttribute.DisposeAsync().ConfigureAwait(false);
            }
        }

        private async Task AttributeListOnBeforeClearCollectionChanged(object sender,
            NotifyCollectionChangedEventArgs e, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            foreach (CharacterAttrib objAttribute in e.OldItems)
            {
                if (objAttribute == await GetAttributeByNameAsync(objAttribute.Abbrev, token).ConfigureAwait(false))
                {
                    objAttribute.MultiplePropertiesChangedAsync -= RunExtraAsyncPropertyChanged(objAttribute.Abbrev);
                }

                Tuple<string, CharacterAttrib.AttributeCategory> tupKey =
                    new Tuple<string, CharacterAttrib.AttributeCategory>(objAttribute.Abbrev,
                        objAttribute.MetatypeCategory);
                _dicAttributes.TryRemove(tupKey, out _);
                await objAttribute.DisposeAsync().ConfigureAwait(false);
            }
        }

        private async Task AttributeListOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e,
            CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (CharacterAttrib objAttribute in e.NewItems)
                    {
                        Tuple<string, CharacterAttrib.AttributeCategory> tupKey =
                            new Tuple<string, CharacterAttrib.AttributeCategory>(objAttribute.Abbrev,
                                objAttribute.MetatypeCategory);
                        _dicAttributes.AddOrUpdate(tupKey, objAttribute, (x, y) =>
                        {
                            y.Dispose();
                            return objAttribute;
                        });
                        if (objAttribute ==
                            await GetAttributeByNameAsync(objAttribute.Abbrev, token).ConfigureAwait(false))
                        {
                            objAttribute.MultiplePropertiesChangedAsync += RunExtraAsyncPropertyChanged(objAttribute.Abbrev);
                        }
                    }

                    break;

                case NotifyCollectionChangedAction.Remove:
                    foreach (CharacterAttrib objAttribute in e.OldItems)
                    {
                        if (objAttribute ==
                            await GetAttributeByNameAsync(objAttribute.Abbrev, token).ConfigureAwait(false))
                        {
                            objAttribute.MultiplePropertiesChangedAsync -= RunExtraAsyncPropertyChanged(objAttribute.Abbrev);
                        }

                        Tuple<string, CharacterAttrib.AttributeCategory> tupKey =
                            new Tuple<string, CharacterAttrib.AttributeCategory>(objAttribute.Abbrev,
                                objAttribute.MetatypeCategory);
                        _dicAttributes.TryRemove(tupKey, out _);
                        await objAttribute.DisposeAsync().ConfigureAwait(false);
                    }

                    break;

                case NotifyCollectionChangedAction.Replace:
                    HashSet<CharacterAttrib> setNewAttribs = e.NewItems.OfType<CharacterAttrib>().ToHashSet();
                    foreach (CharacterAttrib objAttribute in e.OldItems)
                    {
                        if (setNewAttribs.Contains(objAttribute))
                            continue;
                        if (objAttribute ==
                            await GetAttributeByNameAsync(objAttribute.Abbrev, token).ConfigureAwait(false))
                        {
                            objAttribute.MultiplePropertiesChangedAsync -= RunExtraAsyncPropertyChanged(objAttribute.Abbrev);
                        }

                        Tuple<string, CharacterAttrib.AttributeCategory> tupKey =
                            new Tuple<string, CharacterAttrib.AttributeCategory>(objAttribute.Abbrev,
                                objAttribute.MetatypeCategory);
                        _dicAttributes.TryRemove(tupKey, out _);
                        await objAttribute.DisposeAsync().ConfigureAwait(false);
                    }

                    foreach (CharacterAttrib objAttribute in setNewAttribs)
                    {
                        Tuple<string, CharacterAttrib.AttributeCategory> tupKey =
                            new Tuple<string, CharacterAttrib.AttributeCategory>(objAttribute.Abbrev,
                                objAttribute.MetatypeCategory);
                        _dicAttributes.AddOrUpdate(tupKey, objAttribute, (x, y) =>
                        {
                            y.Dispose();
                            return objAttribute;
                        });
                        if (objAttribute ==
                            await GetAttributeByNameAsync(objAttribute.Abbrev, token).ConfigureAwait(false))
                        {
                            objAttribute.MultiplePropertiesChangedAsync += RunExtraAsyncPropertyChanged(objAttribute.Abbrev);
                        }
                    }

                    break;
                case NotifyCollectionChangedAction.Reset:
                    await AttributeList.ForEachAsync(async objAttribute =>
                    {
                        Tuple<string, CharacterAttrib.AttributeCategory> tupKey =
                            new Tuple<string, CharacterAttrib.AttributeCategory>(objAttribute.Abbrev,
                                objAttribute.MetatypeCategory);
                        _dicAttributes.AddOrUpdate(tupKey, objAttribute, (x, y) =>
                        {
                            y.Dispose();
                            return objAttribute;
                        });
                        if (objAttribute == await GetAttributeByNameAsync(objAttribute.Abbrev, token)
                                .ConfigureAwait(false))
                        {
                            objAttribute.MultiplePropertiesChangedAsync += RunExtraAsyncPropertyChanged(objAttribute.Abbrev);
                        }
                    }, token).ConfigureAwait(false);

                    break;
            }
        }

        private async Task SpecialAttributeListOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e,
            CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (CharacterAttrib objAttribute in e.NewItems)
                    {
                        Tuple<string, CharacterAttrib.AttributeCategory> tupKey =
                            new Tuple<string, CharacterAttrib.AttributeCategory>(objAttribute.Abbrev,
                                objAttribute.MetatypeCategory);
                        _dicAttributes.AddOrUpdate(tupKey, objAttribute, (x, y) =>
                        {
                            y.Dispose();
                            return objAttribute;
                        });
                        if (objAttribute ==
                            await GetAttributeByNameAsync(objAttribute.Abbrev, token).ConfigureAwait(false))
                        {
                            objAttribute.MultiplePropertiesChangedAsync += RunExtraAsyncPropertyChanged(objAttribute.Abbrev);
                        }
                    }

                    break;

                case NotifyCollectionChangedAction.Remove:
                    foreach (CharacterAttrib objAttribute in e.OldItems)
                    {
                        if (objAttribute ==
                            await GetAttributeByNameAsync(objAttribute.Abbrev, token).ConfigureAwait(false))
                        {
                            objAttribute.MultiplePropertiesChangedAsync -= RunExtraAsyncPropertyChanged(objAttribute.Abbrev);
                        }

                        Tuple<string, CharacterAttrib.AttributeCategory> tupKey =
                            new Tuple<string, CharacterAttrib.AttributeCategory>(objAttribute.Abbrev,
                                objAttribute.MetatypeCategory);
                        _dicAttributes.TryRemove(tupKey, out _);
                        await objAttribute.DisposeAsync().ConfigureAwait(false);
                    }

                    break;

                case NotifyCollectionChangedAction.Replace:
                    HashSet<CharacterAttrib> setNewAttribs = e.NewItems.OfType<CharacterAttrib>().ToHashSet();
                    foreach (CharacterAttrib objAttribute in e.OldItems)
                    {
                        if (setNewAttribs.Contains(objAttribute))
                            continue;
                        if (objAttribute ==
                            await GetAttributeByNameAsync(objAttribute.Abbrev, token).ConfigureAwait(false))
                        {
                            objAttribute.MultiplePropertiesChangedAsync -= RunExtraAsyncPropertyChanged(objAttribute.Abbrev);
                        }

                        Tuple<string, CharacterAttrib.AttributeCategory> tupKey =
                            new Tuple<string, CharacterAttrib.AttributeCategory>(objAttribute.Abbrev,
                                objAttribute.MetatypeCategory);
                        _dicAttributes.TryRemove(tupKey, out _);
                        await objAttribute.DisposeAsync().ConfigureAwait(false);
                    }

                    foreach (CharacterAttrib objAttribute in setNewAttribs)
                    {
                        Tuple<string, CharacterAttrib.AttributeCategory> tupKey =
                            new Tuple<string, CharacterAttrib.AttributeCategory>(objAttribute.Abbrev,
                                objAttribute.MetatypeCategory);
                        _dicAttributes.AddOrUpdate(tupKey, objAttribute, (x, y) =>
                        {
                            y.Dispose();
                            return objAttribute;
                        });
                        if (objAttribute ==
                            await GetAttributeByNameAsync(objAttribute.Abbrev, token).ConfigureAwait(false))
                        {
                            objAttribute.MultiplePropertiesChangedAsync += RunExtraAsyncPropertyChanged(objAttribute.Abbrev);
                        }
                    }

                    break;
                case NotifyCollectionChangedAction.Reset:
                    await SpecialAttributeList.ForEachAsync(async objAttribute =>
                    {
                        Tuple<string, CharacterAttrib.AttributeCategory> tupKey =
                            new Tuple<string, CharacterAttrib.AttributeCategory>(objAttribute.Abbrev,
                                objAttribute.MetatypeCategory);
                        _dicAttributes.AddOrUpdate(tupKey, objAttribute, (x, y) =>
                        {
                            y.Dispose();
                            return objAttribute;
                        });
                        if (objAttribute == await GetAttributeByNameAsync(objAttribute.Abbrev, token)
                                .ConfigureAwait(false))
                        {
                            objAttribute.MultiplePropertiesChangedAsync += RunExtraAsyncPropertyChanged(objAttribute.Abbrev);
                        }
                    }, token).ConfigureAwait(false);

                    break;
            }
        }

        public void Dispose()
        {
            using (LockObject.EnterWriteLock())
            {
                foreach (CharacterAttrib objAttribute in _dicAttributes.Values)
                {
                    if (objAttribute == GetAttributeByName(objAttribute.Abbrev))
                    {
                        objAttribute.MultiplePropertiesChangedAsync -= RunExtraAsyncPropertyChanged(objAttribute.Abbrev);
                    }
                    objAttribute.Dispose();
                }

                _lstNormalAttributes.Dispose();
                _lstSpecialAttributes.Dispose();
                _lstAttributes.Dispose();
            }
        }

        public async ValueTask DisposeAsync()
        {
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync().ConfigureAwait(false);
            try
            {
                foreach (CharacterAttrib objAttribute in _dicAttributes.Values)
                {
                    if (objAttribute == await GetAttributeByNameAsync(objAttribute.Abbrev).ConfigureAwait(false))
                    {
                        objAttribute.MultiplePropertiesChangedAsync -= RunExtraAsyncPropertyChanged(objAttribute.Abbrev);
                    }
                    await objAttribute.DisposeAsync().ConfigureAwait(false);
                }

                await _lstNormalAttributes.DisposeAsync().ConfigureAwait(false);
                await _lstSpecialAttributes.DisposeAsync().ConfigureAwait(false);
                await _lstAttributes.DisposeAsync().ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        internal void Save(XmlWriter objWriter, CancellationToken token = default)
        {
            using (LockObject.EnterReadLock(token))
            {
                foreach (CharacterAttrib objAttribute in AllAttributes)
                {
                    objAttribute.Save(objWriter);
                }
            }
        }

        public void Create(XmlNode charNode, int intValue, int intMinModifier = 0, int intMaxModifier = 0, CancellationToken token = default)
        {
            if (charNode == null)
                return;
            using (LockObject.EnterWriteLock(token))
            {
                Interlocked.Increment(ref _intLoading);
                try
                {
                    using (Timekeeper.StartSyncron("create_char_attrib", null,
                                                       CustomActivity.OperationType.RequestOperation,
                                                       charNode.InnerText))
                    {
                        CharacterAttrib objBod = GetAttributeByName("BOD", token);
                        int intOldBODBase = objBod?.Base ?? 0;
                        int intOldBODKarma = objBod?.Karma ?? 0;
                        CharacterAttrib objAgi = GetAttributeByName("AGI", token);
                        int intOldAGIBase = objAgi?.Base ?? 0;
                        int intOldAGIKarma = objAgi?.Karma ?? 0;
                        CharacterAttrib objRea = GetAttributeByName("REA", token);
                        int intOldREABase = objRea?.Base ?? 0;
                        int intOldREAKarma = objRea?.Karma ?? 0;
                        CharacterAttrib objStr = GetAttributeByName("STR", token);
                        int intOldSTRBase = objStr?.Base ?? 0;
                        int intOldSTRKarma = objStr?.Karma ?? 0;
                        CharacterAttrib objCha = GetAttributeByName("CHA", token);
                        int intOldCHABase = objCha?.Base ?? 0;
                        int intOldCHAKarma = objCha?.Karma ?? 0;
                        CharacterAttrib objInt = GetAttributeByName("INT", token);
                        int intOldINTBase = objInt?.Base ?? 0;
                        int intOldINTKarma = objInt?.Karma ?? 0;
                        CharacterAttrib objLog = GetAttributeByName("LOG", token);
                        int intOldLOGBase = objLog?.Base ?? 0;
                        int intOldLOGKarma = objLog?.Karma ?? 0;
                        CharacterAttrib objWil = GetAttributeByName("WIL", token);
                        int intOldWILBase = objWil?.Base ?? 0;
                        int intOldWILKarma = objWil?.Karma ?? 0;
                        CharacterAttrib objEdg = GetAttributeByName("EDG", token);
                        int intOldEDGBase = objEdg?.Base ?? 0;
                        int intOldEDGKarma = objEdg?.Karma ?? 0;
                        CharacterAttrib objMag = GetAttributeByName("MAG", token);
                        int intOldMAGBase = objMag?.Base ?? 0;
                        int intOldMAGKarma = objMag?.Karma ?? 0;
                        CharacterAttrib objMagAdept = GetAttributeByName("MAGAdept", token);
                        int intOldMAGAdeptBase = objMagAdept?.Base ?? 0;
                        int intOldMAGAdeptKarma = objMagAdept?.Karma ?? 0;
                        CharacterAttrib objRes = GetAttributeByName("RES", token);
                        int intOldRESBase = objRes?.Base ?? 0;
                        int intOldRESKarma = objRes?.Karma ?? 0;
                        CharacterAttrib objDep = GetAttributeByName("DEP", token);
                        int intOldDEPBase = objDep?.Base ?? 0;
                        int intOldDEPKarma = objDep?.Karma ?? 0;
                        AttributeList.Clear();
                        SpecialAttributeList.Clear();

                        foreach (string strAttribute in AttributeStrings)
                        {
                            CharacterAttrib objAttribute;
                            switch (CharacterAttrib.ConvertToAttributeCategory(strAttribute))
                            {
                                case CharacterAttrib.AttributeCategory.Special:
                                    objAttribute = new CharacterAttrib(_objCharacter, strAttribute,
                                                                       CharacterAttrib.AttributeCategory.Special);
                                    SpecialAttributeList.Add(objAttribute);
                                    break;

                                case CharacterAttrib.AttributeCategory.Standard:
                                    objAttribute = new CharacterAttrib(_objCharacter, strAttribute,
                                                                       CharacterAttrib.AttributeCategory.Standard);
                                    AttributeList.Add(objAttribute);
                                    break;

                                default:
                                    continue;
                            }
                        }

                        objBod = GetAttributeByName("BOD", token);
                        objAgi = GetAttributeByName("AGI", token);
                        objRea = GetAttributeByName("REA", token);
                        objStr = GetAttributeByName("STR", token);
                        objCha = GetAttributeByName("CHA", token);
                        objInt = GetAttributeByName("INT", token);
                        objLog = GetAttributeByName("LOG", token);
                        objWil = GetAttributeByName("WIL", token);
                        objEdg = GetAttributeByName("EDG", token);
                        objMag = GetAttributeByName("MAG", token);
                        objMagAdept = GetAttributeByName("MAGAdept", token);
                        objRes = GetAttributeByName("RES", token);
                        objDep = GetAttributeByName("DEP", token);

                        objBod.AssignLimits(
                            CommonFunctions.ExpressionToInt(charNode["bodmin"]?.InnerText, intValue, intMinModifier, token: token),
                            CommonFunctions.ExpressionToInt(charNode["bodmax"]?.InnerText, intValue, intMaxModifier, token: token),
                            CommonFunctions.ExpressionToInt(charNode["bodaug"]?.InnerText, intValue, intMaxModifier, token: token));
                        objAgi.AssignLimits(
                            CommonFunctions.ExpressionToInt(charNode["agimin"]?.InnerText, intValue, intMinModifier, token: token),
                            CommonFunctions.ExpressionToInt(charNode["agimax"]?.InnerText, intValue, intMaxModifier, token: token),
                            CommonFunctions.ExpressionToInt(charNode["agiaug"]?.InnerText, intValue, intMaxModifier, token: token));
                        objRea.AssignLimits(
                            CommonFunctions.ExpressionToInt(charNode["reamin"]?.InnerText, intValue, intMinModifier, token: token),
                            CommonFunctions.ExpressionToInt(charNode["reamax"]?.InnerText, intValue, intMaxModifier, token: token),
                            CommonFunctions.ExpressionToInt(charNode["reaaug"]?.InnerText, intValue, intMaxModifier, token: token));
                        objStr.AssignLimits(
                            CommonFunctions.ExpressionToInt(charNode["strmin"]?.InnerText, intValue, intMinModifier, token: token),
                            CommonFunctions.ExpressionToInt(charNode["strmax"]?.InnerText, intValue, intMaxModifier, token: token),
                            CommonFunctions.ExpressionToInt(charNode["straug"]?.InnerText, intValue, intMaxModifier, token: token));
                        objCha.AssignLimits(
                            CommonFunctions.ExpressionToInt(charNode["chamin"]?.InnerText, intValue, intMinModifier, token: token),
                            CommonFunctions.ExpressionToInt(charNode["chamax"]?.InnerText, intValue, intMaxModifier, token: token),
                            CommonFunctions.ExpressionToInt(charNode["chaaug"]?.InnerText, intValue, intMaxModifier, token: token));
                        objInt.AssignLimits(
                            CommonFunctions.ExpressionToInt(charNode["intmin"]?.InnerText, intValue, intMinModifier, token: token),
                            CommonFunctions.ExpressionToInt(charNode["intmax"]?.InnerText, intValue, intMaxModifier, token: token),
                            CommonFunctions.ExpressionToInt(charNode["intaug"]?.InnerText, intValue, intMaxModifier, token: token));
                        objLog.AssignLimits(
                            CommonFunctions.ExpressionToInt(charNode["logmin"]?.InnerText, intValue, intMinModifier, token: token),
                            CommonFunctions.ExpressionToInt(charNode["logmax"]?.InnerText, intValue, intMaxModifier, token: token),
                            CommonFunctions.ExpressionToInt(charNode["logaug"]?.InnerText, intValue, intMaxModifier, token: token));
                        objWil.AssignLimits(
                            CommonFunctions.ExpressionToInt(charNode["wilmin"]?.InnerText, intValue, intMinModifier, token: token),
                            CommonFunctions.ExpressionToInt(charNode["wilmax"]?.InnerText, intValue, intMaxModifier, token: token),
                            CommonFunctions.ExpressionToInt(charNode["wilaug"]?.InnerText, intValue, intMaxModifier, token: token));
                        objMag.AssignLimits(
                            CommonFunctions.ExpressionToInt(charNode["magmin"]?.InnerText, intValue, intMinModifier, token: token),
                            CommonFunctions.ExpressionToInt(charNode["magmax"]?.InnerText, intValue, intMaxModifier, token: token),
                            CommonFunctions.ExpressionToInt(charNode["magaug"]?.InnerText, intValue, intMaxModifier, token: token));
                        objRes.AssignLimits(
                            CommonFunctions.ExpressionToInt(charNode["resmin"]?.InnerText, intValue, intMinModifier, token: token),
                            CommonFunctions.ExpressionToInt(charNode["resmax"]?.InnerText, intValue, intMaxModifier, token: token),
                            CommonFunctions.ExpressionToInt(charNode["resaug"]?.InnerText, intValue, intMaxModifier, token: token));
                        objEdg.AssignLimits(
                            CommonFunctions.ExpressionToInt(charNode["edgmin"]?.InnerText, intValue, intMinModifier, token: token),
                            CommonFunctions.ExpressionToInt(charNode["edgmax"]?.InnerText, intValue, intMaxModifier, token: token),
                            CommonFunctions.ExpressionToInt(charNode["edgaug"]?.InnerText, intValue, intMaxModifier, token: token));
                        objDep.AssignLimits(
                            CommonFunctions.ExpressionToInt(charNode["depmin"]?.InnerText, intValue, intMinModifier, token: token),
                            CommonFunctions.ExpressionToInt(charNode["depmax"]?.InnerText, intValue, intMaxModifier, token: token),
                            CommonFunctions.ExpressionToInt(charNode["depaug"]?.InnerText, intValue, intMaxModifier, token: token));
                        objMagAdept.AssignLimits(
                            CommonFunctions.ExpressionToInt(charNode["magmin"]?.InnerText, intValue, intMinModifier, token: token),
                            CommonFunctions.ExpressionToInt(charNode["magmax"]?.InnerText, intValue, intMaxModifier, token: token),
                            CommonFunctions.ExpressionToInt(charNode["magaug"]?.InnerText, intValue, intMaxModifier, token: token));
                        GetAttributeByName("ESS", token).AssignLimits(
                            CommonFunctions.ExpressionToInt(charNode["essmin"]?.InnerText, intValue, token: token),
                            CommonFunctions.ExpressionToInt(charNode["essmax"]?.InnerText, intValue, token: token),
                            CommonFunctions.ExpressionToInt(charNode["essaug"]?.InnerText, intValue, token: token));

                        objBod.Base = Math.Min(intOldBODBase, objBod.PriorityMaximum);
                        objBod.Karma = Math.Min(intOldBODKarma, objBod.KarmaMaximum);
                        objAgi.Base = Math.Min(intOldAGIBase, objAgi.PriorityMaximum);
                        objAgi.Karma = Math.Min(intOldAGIKarma, objAgi.KarmaMaximum);
                        objRea.Base = Math.Min(intOldREABase, objRea.PriorityMaximum);
                        objRea.Karma = Math.Min(intOldREAKarma, objRea.KarmaMaximum);
                        objStr.Base = Math.Min(intOldSTRBase, objStr.PriorityMaximum);
                        objStr.Karma = Math.Min(intOldSTRKarma, objStr.KarmaMaximum);
                        objCha.Base = Math.Min(intOldCHABase, objCha.PriorityMaximum);
                        objCha.Karma = Math.Min(intOldCHAKarma, objCha.KarmaMaximum);
                        objInt.Base = Math.Min(intOldINTBase, objInt.PriorityMaximum);
                        objInt.Karma = Math.Min(intOldINTKarma, objInt.KarmaMaximum);
                        objLog.Base = Math.Min(intOldLOGBase, objLog.PriorityMaximum);
                        objLog.Karma = Math.Min(intOldLOGKarma, objLog.KarmaMaximum);
                        objWil.Base = Math.Min(intOldWILBase, objWil.PriorityMaximum);
                        objWil.Karma = Math.Min(intOldWILKarma, objWil.KarmaMaximum);
                        objEdg.Base = Math.Min(intOldEDGBase, objEdg.PriorityMaximum);
                        objEdg.Karma = Math.Min(intOldEDGKarma, objEdg.KarmaMaximum);

                        if (_objCharacter.MAGEnabled)
                        {
                            objMag.Base = Math.Min(intOldMAGBase, objMag.PriorityMaximum);
                            objMag.Karma = Math.Min(intOldMAGKarma, objMag.KarmaMaximum);
                            if (_objCharacter.Settings.MysAdeptSecondMAGAttribute && _objCharacter.IsMysticAdept)
                            {
                                objMagAdept.Base =
                                    Math.Min(intOldMAGAdeptBase, objMagAdept.PriorityMaximum);
                                objMagAdept.Karma =
                                    Math.Min(intOldMAGAdeptKarma, objMagAdept.KarmaMaximum);
                            }
                        }

                        if (_objCharacter.RESEnabled)
                        {
                            objRes.Base = Math.Min(intOldRESBase, objRes.PriorityMaximum);
                            objRes.Karma = Math.Min(intOldRESKarma, objRes.KarmaMaximum);
                        }

                        if (_objCharacter.DEPEnabled)
                        {
                            objDep.Base = Math.Min(intOldDEPBase, objDep.PriorityMaximum);
                            objDep.Karma = Math.Min(intOldDEPKarma, objDep.KarmaMaximum);
                        }

                        InitializeAttributesList(token);
                        ResetBindings(token);

                        //Timekeeper.Finish("create_char_attrib");
                    }
                }
                finally
                {
                    Interlocked.Decrement(ref _intLoading);
                }
            }
        }

        public async Task CreateAsync(XmlNode charNode, int intValue, int intMinModifier = 0, int intMaxModifier = 0, CancellationToken token = default)
        {
            if (charNode == null)
                return;
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                Interlocked.Increment(ref _intLoading);
                try
                {
                    using (Timekeeper.StartSyncron("create_char_attrib", null,
                               CustomActivity.OperationType.RequestOperation,
                               charNode.InnerText))
                    {
                        CharacterAttrib objBod = await GetAttributeByNameAsync("BOD", token).ConfigureAwait(false);
                        int intOldBODBase = objBod != null ? await objBod.GetBaseAsync(token).ConfigureAwait(false) : 0;
                        int intOldBODKarma =
                            objBod != null ? await objBod.GetKarmaAsync(token).ConfigureAwait(false) : 0;
                        CharacterAttrib objAgi = await GetAttributeByNameAsync("AGI", token).ConfigureAwait(false);
                        int intOldAGIBase = objAgi != null ? await objAgi.GetBaseAsync(token).ConfigureAwait(false) : 0;
                        int intOldAGIKarma =
                            objAgi != null ? await objAgi.GetKarmaAsync(token).ConfigureAwait(false) : 0;
                        CharacterAttrib objRea = await GetAttributeByNameAsync("REA", token).ConfigureAwait(false);
                        int intOldREABase = objRea != null ? await objRea.GetBaseAsync(token).ConfigureAwait(false) : 0;
                        int intOldREAKarma =
                            objRea != null ? await objRea.GetKarmaAsync(token).ConfigureAwait(false) : 0;
                        CharacterAttrib objStr = await GetAttributeByNameAsync("STR", token).ConfigureAwait(false);
                        int intOldSTRBase = objStr != null ? await objStr.GetBaseAsync(token).ConfigureAwait(false) : 0;
                        int intOldSTRKarma =
                            objStr != null ? await objStr.GetKarmaAsync(token).ConfigureAwait(false) : 0;
                        CharacterAttrib objCha = await GetAttributeByNameAsync("CHA", token).ConfigureAwait(false);
                        int intOldCHABase = objCha != null ? await objCha.GetBaseAsync(token).ConfigureAwait(false) : 0;
                        int intOldCHAKarma =
                            objCha != null ? await objCha.GetKarmaAsync(token).ConfigureAwait(false) : 0;
                        CharacterAttrib objInt = await GetAttributeByNameAsync("INT", token).ConfigureAwait(false);
                        int intOldINTBase = objInt != null ? await objInt.GetBaseAsync(token).ConfigureAwait(false) : 0;
                        int intOldINTKarma =
                            objInt != null ? await objInt.GetKarmaAsync(token).ConfigureAwait(false) : 0;
                        CharacterAttrib objLog = await GetAttributeByNameAsync("LOG", token).ConfigureAwait(false);
                        int intOldLOGBase = objLog != null ? await objLog.GetBaseAsync(token).ConfigureAwait(false) : 0;
                        int intOldLOGKarma =
                            objLog != null ? await objLog.GetKarmaAsync(token).ConfigureAwait(false) : 0;
                        CharacterAttrib objWil = await GetAttributeByNameAsync("WIL", token).ConfigureAwait(false);
                        int intOldWILBase = objWil != null ? await objWil.GetBaseAsync(token).ConfigureAwait(false) : 0;
                        int intOldWILKarma =
                            objWil != null ? await objWil.GetKarmaAsync(token).ConfigureAwait(false) : 0;
                        CharacterAttrib objEdg = await GetAttributeByNameAsync("EDG", token).ConfigureAwait(false);
                        int intOldEDGBase = objEdg != null ? await objEdg.GetBaseAsync(token).ConfigureAwait(false) : 0;
                        int intOldEDGKarma =
                            objEdg != null ? await objEdg.GetKarmaAsync(token).ConfigureAwait(false) : 0;
                        CharacterAttrib objMag = await GetAttributeByNameAsync("MAG", token).ConfigureAwait(false);
                        int intOldMAGBase = objMag != null ? await objMag.GetBaseAsync(token).ConfigureAwait(false) : 0;
                        int intOldMAGKarma =
                            objMag != null ? await objMag.GetKarmaAsync(token).ConfigureAwait(false) : 0;
                        CharacterAttrib objMagAdept =
                            await GetAttributeByNameAsync("MAGAdept", token).ConfigureAwait(false);
                        int intOldMAGAdeptBase = objMagAdept != null
                            ? await objMagAdept.GetBaseAsync(token).ConfigureAwait(false)
                            : 0;
                        int intOldMAGAdeptKarma = objMagAdept != null
                            ? await objMagAdept.GetKarmaAsync(token).ConfigureAwait(false)
                            : 0;
                        CharacterAttrib objRes = await GetAttributeByNameAsync("RES", token).ConfigureAwait(false);
                        int intOldRESBase = objRes != null ? await objRes.GetBaseAsync(token).ConfigureAwait(false) : 0;
                        int intOldRESKarma =
                            objRes != null ? await objRes.GetKarmaAsync(token).ConfigureAwait(false) : 0;
                        CharacterAttrib objDep = await GetAttributeByNameAsync("DEP", token).ConfigureAwait(false);
                        int intOldDEPBase = objDep != null ? await objDep.GetBaseAsync(token).ConfigureAwait(false) : 0;
                        int intOldDEPKarma =
                            objDep != null ? await objDep.GetKarmaAsync(token).ConfigureAwait(false) : 0;
                        await AttributeList.ClearAsync(token).ConfigureAwait(false);
                        await SpecialAttributeList.ClearAsync(token).ConfigureAwait(false);

                        foreach (string strAttribute in AttributeStrings)
                        {
                            CharacterAttrib objAttribute;
                            switch (CharacterAttrib.ConvertToAttributeCategory(strAttribute))
                            {
                                case CharacterAttrib.AttributeCategory.Special:
                                    objAttribute = new CharacterAttrib(_objCharacter, strAttribute,
                                        CharacterAttrib.AttributeCategory.Special);
                                    await SpecialAttributeList.AddAsync(objAttribute, token).ConfigureAwait(false);
                                    break;

                                case CharacterAttrib.AttributeCategory.Standard:
                                    objAttribute = new CharacterAttrib(_objCharacter, strAttribute,
                                        CharacterAttrib.AttributeCategory.Standard);
                                    await AttributeList.AddAsync(objAttribute, token).ConfigureAwait(false);
                                    break;

                                default:
                                    continue;
                            }
                        }

                        objBod = await GetAttributeByNameAsync("BOD", token).ConfigureAwait(false);
                        objAgi = await GetAttributeByNameAsync("AGI", token).ConfigureAwait(false);
                        objRea = await GetAttributeByNameAsync("REA", token).ConfigureAwait(false);
                        objStr = await GetAttributeByNameAsync("STR", token).ConfigureAwait(false);
                        objCha = await GetAttributeByNameAsync("CHA", token).ConfigureAwait(false);
                        objInt = await GetAttributeByNameAsync("INT", token).ConfigureAwait(false);
                        objLog = await GetAttributeByNameAsync("LOG", token).ConfigureAwait(false);
                        objWil = await GetAttributeByNameAsync("WIL", token).ConfigureAwait(false);
                        objEdg = await GetAttributeByNameAsync("EDG", token).ConfigureAwait(false);
                        objMag = await GetAttributeByNameAsync("MAG", token).ConfigureAwait(false);
                        objMagAdept = await GetAttributeByNameAsync("MAGAdept", token).ConfigureAwait(false);
                        objRes = await GetAttributeByNameAsync("RES", token).ConfigureAwait(false);
                        objDep = await GetAttributeByNameAsync("DEP", token).ConfigureAwait(false);

                        await objBod.AssignLimitsAsync(
                            await CommonFunctions.ExpressionToIntAsync(charNode["bodmin"]?.InnerText, intValue,
                                intMinModifier, token: token).ConfigureAwait(false),
                            await CommonFunctions.ExpressionToIntAsync(charNode["bodmax"]?.InnerText, intValue,
                                intMaxModifier, token: token).ConfigureAwait(false),
                            await CommonFunctions.ExpressionToIntAsync(charNode["bodaug"]?.InnerText, intValue,
                                intMaxModifier, token: token).ConfigureAwait(false), token).ConfigureAwait(false);
                        await objAgi.AssignLimitsAsync(
                            await CommonFunctions.ExpressionToIntAsync(charNode["agimin"]?.InnerText, intValue,
                                intMinModifier, token: token).ConfigureAwait(false),
                            await CommonFunctions.ExpressionToIntAsync(charNode["agimax"]?.InnerText, intValue,
                                intMaxModifier, token: token).ConfigureAwait(false),
                            await CommonFunctions.ExpressionToIntAsync(charNode["agiaug"]?.InnerText, intValue,
                                intMaxModifier, token: token).ConfigureAwait(false), token).ConfigureAwait(false);
                        await objRea.AssignLimitsAsync(
                            await CommonFunctions.ExpressionToIntAsync(charNode["reamin"]?.InnerText, intValue,
                                intMinModifier, token: token).ConfigureAwait(false),
                            await CommonFunctions.ExpressionToIntAsync(charNode["reamax"]?.InnerText, intValue,
                                intMaxModifier, token: token).ConfigureAwait(false),
                            await CommonFunctions.ExpressionToIntAsync(charNode["reaaug"]?.InnerText, intValue,
                                intMaxModifier, token: token).ConfigureAwait(false), token).ConfigureAwait(false);
                        await objStr.AssignLimitsAsync(
                            await CommonFunctions.ExpressionToIntAsync(charNode["strmin"]?.InnerText, intValue,
                                intMinModifier, token: token).ConfigureAwait(false),
                            await CommonFunctions.ExpressionToIntAsync(charNode["strmax"]?.InnerText, intValue,
                                intMaxModifier, token: token).ConfigureAwait(false),
                            await CommonFunctions.ExpressionToIntAsync(charNode["straug"]?.InnerText, intValue,
                                intMaxModifier, token: token).ConfigureAwait(false), token).ConfigureAwait(false);
                        await objCha.AssignLimitsAsync(
                            await CommonFunctions.ExpressionToIntAsync(charNode["chamin"]?.InnerText, intValue,
                                intMinModifier, token: token).ConfigureAwait(false),
                            await CommonFunctions.ExpressionToIntAsync(charNode["chamax"]?.InnerText, intValue,
                                intMaxModifier, token: token).ConfigureAwait(false),
                            await CommonFunctions.ExpressionToIntAsync(charNode["chaaug"]?.InnerText, intValue,
                                intMaxModifier, token: token).ConfigureAwait(false), token).ConfigureAwait(false);
                        await objInt.AssignLimitsAsync(
                            await CommonFunctions.ExpressionToIntAsync(charNode["intmin"]?.InnerText, intValue,
                                intMinModifier, token: token).ConfigureAwait(false),
                            await CommonFunctions.ExpressionToIntAsync(charNode["intmax"]?.InnerText, intValue,
                                intMaxModifier, token: token).ConfigureAwait(false),
                            await CommonFunctions.ExpressionToIntAsync(charNode["intaug"]?.InnerText, intValue,
                                intMaxModifier, token: token).ConfigureAwait(false), token).ConfigureAwait(false);
                        await objLog.AssignLimitsAsync(
                            await CommonFunctions.ExpressionToIntAsync(charNode["logmin"]?.InnerText, intValue,
                                intMinModifier, token: token).ConfigureAwait(false),
                            await CommonFunctions.ExpressionToIntAsync(charNode["logmax"]?.InnerText, intValue,
                                intMaxModifier, token: token).ConfigureAwait(false),
                            await CommonFunctions.ExpressionToIntAsync(charNode["logaug"]?.InnerText, intValue,
                                intMaxModifier, token: token).ConfigureAwait(false), token).ConfigureAwait(false);
                        await objWil.AssignLimitsAsync(
                            await CommonFunctions.ExpressionToIntAsync(charNode["wilmin"]?.InnerText, intValue,
                                intMinModifier, token: token).ConfigureAwait(false),
                            await CommonFunctions.ExpressionToIntAsync(charNode["wilmax"]?.InnerText, intValue,
                                intMaxModifier, token: token).ConfigureAwait(false),
                            await CommonFunctions.ExpressionToIntAsync(charNode["wilaug"]?.InnerText, intValue,
                                intMaxModifier, token: token).ConfigureAwait(false), token).ConfigureAwait(false);
                        await objMag.AssignLimitsAsync(
                            await CommonFunctions.ExpressionToIntAsync(charNode["magmin"]?.InnerText, intValue,
                                intMinModifier, token: token).ConfigureAwait(false),
                            await CommonFunctions.ExpressionToIntAsync(charNode["magmax"]?.InnerText, intValue,
                                intMaxModifier, token: token).ConfigureAwait(false),
                            await CommonFunctions.ExpressionToIntAsync(charNode["magaug"]?.InnerText, intValue,
                                intMaxModifier, token: token).ConfigureAwait(false), token).ConfigureAwait(false);
                        await objRes.AssignLimitsAsync(
                            await CommonFunctions.ExpressionToIntAsync(charNode["resmin"]?.InnerText, intValue,
                                intMinModifier, token: token).ConfigureAwait(false),
                            await CommonFunctions.ExpressionToIntAsync(charNode["resmax"]?.InnerText, intValue,
                                intMaxModifier, token: token).ConfigureAwait(false),
                            await CommonFunctions.ExpressionToIntAsync(charNode["resaug"]?.InnerText, intValue,
                                intMaxModifier, token: token).ConfigureAwait(false), token).ConfigureAwait(false);
                        await objEdg.AssignLimitsAsync(
                            await CommonFunctions.ExpressionToIntAsync(charNode["edgmin"]?.InnerText, intValue,
                                intMinModifier, token: token).ConfigureAwait(false),
                            await CommonFunctions.ExpressionToIntAsync(charNode["edgmax"]?.InnerText, intValue,
                                intMaxModifier, token: token).ConfigureAwait(false),
                            await CommonFunctions.ExpressionToIntAsync(charNode["edgaug"]?.InnerText, intValue,
                                intMaxModifier, token: token).ConfigureAwait(false), token).ConfigureAwait(false);
                        await objDep.AssignLimitsAsync(
                            await CommonFunctions.ExpressionToIntAsync(charNode["depmin"]?.InnerText, intValue,
                                intMinModifier, token: token).ConfigureAwait(false),
                            await CommonFunctions.ExpressionToIntAsync(charNode["depmax"]?.InnerText, intValue,
                                intMaxModifier, token: token).ConfigureAwait(false),
                            await CommonFunctions.ExpressionToIntAsync(charNode["depaug"]?.InnerText, intValue,
                                intMaxModifier, token: token).ConfigureAwait(false), token).ConfigureAwait(false);
                        await objMagAdept.AssignLimitsAsync(
                            await CommonFunctions.ExpressionToIntAsync(charNode["magmin"]?.InnerText, intValue,
                                intMinModifier, token: token).ConfigureAwait(false),
                            await CommonFunctions.ExpressionToIntAsync(charNode["magmax"]?.InnerText, intValue,
                                intMaxModifier, token: token).ConfigureAwait(false),
                            await CommonFunctions.ExpressionToIntAsync(charNode["magaug"]?.InnerText, intValue,
                                intMaxModifier, token: token).ConfigureAwait(false), token).ConfigureAwait(false);
                        await (await GetAttributeByNameAsync("ESS", token).ConfigureAwait(false)).AssignLimitsAsync(
                            await CommonFunctions.ExpressionToIntAsync(charNode["essmin"]?.InnerText, intValue,
                                token: token).ConfigureAwait(false),
                            await CommonFunctions.ExpressionToIntAsync(charNode["essmax"]?.InnerText, intValue,
                                token: token).ConfigureAwait(false),
                            await CommonFunctions.ExpressionToIntAsync(charNode["essaug"]?.InnerText, intValue,
                                token: token).ConfigureAwait(false), token).ConfigureAwait(false);

                        await objBod.SetBaseAsync(
                            Math.Min(intOldBODBase, await objBod.GetPriorityMaximumAsync(token).ConfigureAwait(false)),
                            token).ConfigureAwait(false);
                        await objBod.SetKarmaAsync(
                            Math.Min(intOldBODKarma, await objBod.GetKarmaMaximumAsync(token).ConfigureAwait(false)),
                            token).ConfigureAwait(false);
                        await objAgi.SetBaseAsync(
                            Math.Min(intOldAGIBase, await objAgi.GetPriorityMaximumAsync(token).ConfigureAwait(false)),
                            token).ConfigureAwait(false);
                        await objAgi.SetKarmaAsync(
                            Math.Min(intOldAGIKarma, await objAgi.GetKarmaMaximumAsync(token).ConfigureAwait(false)),
                            token).ConfigureAwait(false);
                        await objRea.SetBaseAsync(
                            Math.Min(intOldREABase, await objRea.GetPriorityMaximumAsync(token).ConfigureAwait(false)),
                            token).ConfigureAwait(false);
                        await objRea.SetKarmaAsync(
                            Math.Min(intOldREAKarma, await objRea.GetKarmaMaximumAsync(token).ConfigureAwait(false)),
                            token).ConfigureAwait(false);
                        await objStr.SetBaseAsync(
                            Math.Min(intOldSTRBase, await objStr.GetPriorityMaximumAsync(token).ConfigureAwait(false)),
                            token).ConfigureAwait(false);
                        await objStr.SetKarmaAsync(
                            Math.Min(intOldSTRKarma, await objStr.GetKarmaMaximumAsync(token).ConfigureAwait(false)),
                            token).ConfigureAwait(false);
                        await objCha.SetBaseAsync(
                            Math.Min(intOldCHABase, await objCha.GetPriorityMaximumAsync(token).ConfigureAwait(false)),
                            token).ConfigureAwait(false);
                        await objCha.SetKarmaAsync(
                            Math.Min(intOldCHAKarma, await objCha.GetKarmaMaximumAsync(token).ConfigureAwait(false)),
                            token).ConfigureAwait(false);
                        await objInt.SetBaseAsync(
                            Math.Min(intOldINTBase, await objInt.GetPriorityMaximumAsync(token).ConfigureAwait(false)),
                            token).ConfigureAwait(false);
                        await objInt.SetKarmaAsync(
                            Math.Min(intOldINTKarma, await objInt.GetKarmaMaximumAsync(token).ConfigureAwait(false)),
                            token).ConfigureAwait(false);
                        await objLog.SetBaseAsync(
                            Math.Min(intOldLOGBase, await objLog.GetPriorityMaximumAsync(token).ConfigureAwait(false)),
                            token).ConfigureAwait(false);
                        await objLog.SetKarmaAsync(
                            Math.Min(intOldLOGKarma, await objLog.GetKarmaMaximumAsync(token).ConfigureAwait(false)),
                            token).ConfigureAwait(false);
                        await objWil.SetBaseAsync(
                            Math.Min(intOldWILBase, await objWil.GetPriorityMaximumAsync(token).ConfigureAwait(false)),
                            token).ConfigureAwait(false);
                        await objWil.SetKarmaAsync(
                            Math.Min(intOldWILKarma, await objWil.GetKarmaMaximumAsync(token).ConfigureAwait(false)),
                            token).ConfigureAwait(false);
                        await objEdg.SetBaseAsync(
                            Math.Min(intOldEDGBase, await objEdg.GetPriorityMaximumAsync(token).ConfigureAwait(false)),
                            token).ConfigureAwait(false);
                        await objEdg.SetKarmaAsync(
                            Math.Min(intOldEDGKarma, await objEdg.GetKarmaMaximumAsync(token).ConfigureAwait(false)),
                            token).ConfigureAwait(false);

                        if (await _objCharacter.GetMAGEnabledAsync(token).ConfigureAwait(false))
                        {
                            await objMag.SetBaseAsync(
                                    Math.Min(intOldMAGBase,
                                        await objMag.GetPriorityMaximumAsync(token).ConfigureAwait(false)), token)
                                .ConfigureAwait(false);
                            await objMag.SetKarmaAsync(
                                    Math.Min(intOldMAGKarma,
                                        await objMag.GetKarmaMaximumAsync(token).ConfigureAwait(false)), token)
                                .ConfigureAwait(false);
                            if (await _objCharacter.Settings.GetMysAdeptSecondMAGAttributeAsync(token)
                                    .ConfigureAwait(false)
                                && await _objCharacter.GetIsMysticAdeptAsync(token).ConfigureAwait(false))
                            {
                                await objMagAdept.SetBaseAsync(
                                    Math.Min(intOldMAGAdeptBase,
                                        await objMagAdept.GetPriorityMaximumAsync(token).ConfigureAwait(false)),
                                    token).ConfigureAwait(false);
                                await objMagAdept.SetKarmaAsync(
                                    Math.Min(intOldMAGAdeptKarma,
                                        await objMagAdept.GetKarmaMaximumAsync(token).ConfigureAwait(false)),
                                    token).ConfigureAwait(false);
                            }
                        }

                        if (await _objCharacter.GetRESEnabledAsync(token).ConfigureAwait(false))
                        {
                            await objRes.SetBaseAsync(
                                    Math.Min(intOldRESBase,
                                        await objRes.GetPriorityMaximumAsync(token).ConfigureAwait(false)), token)
                                .ConfigureAwait(false);
                            await objRes.SetKarmaAsync(
                                    Math.Min(intOldRESKarma,
                                        await objRes.GetKarmaMaximumAsync(token).ConfigureAwait(false)), token)
                                .ConfigureAwait(false);
                        }

                        if (await _objCharacter.GetDEPEnabledAsync(token).ConfigureAwait(false))
                        {
                            await objDep.SetBaseAsync(
                                    Math.Min(intOldDEPBase,
                                        await objDep.GetPriorityMaximumAsync(token).ConfigureAwait(false)), token)
                                .ConfigureAwait(false);
                            await objDep.SetKarmaAsync(
                                    Math.Min(intOldDEPKarma,
                                        await objDep.GetKarmaMaximumAsync(token).ConfigureAwait(false)), token)
                                .ConfigureAwait(false);
                        }

                        await InitializeAttributesListAsync(token).ConfigureAwait(false);
                        await ResetBindingsAsync(token).ConfigureAwait(false);

                        //Timekeeper.Finish("create_char_attrib");
                    }
                }
                finally
                {
                    Interlocked.Decrement(ref _intLoading);
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public void Load(XmlNode xmlSavedCharacterNode, CancellationToken token = default)
        {
            Utils.SafelyRunSynchronously(() => LoadCoreAsync(true, xmlSavedCharacterNode, token), token);
        }

        public Task LoadAsync(XmlNode xmlSavedCharacterNode, CancellationToken token = default)
        {
            return LoadCoreAsync(false, xmlSavedCharacterNode, token);
        }

        private async Task LoadCoreAsync(bool blnSync, XmlNode xmlSavedCharacterNode, CancellationToken token = default)
        {
            if (xmlSavedCharacterNode == null)
                return;
            IDisposable objLocker = null;
            IAsyncDisposable objLockerAsync = null;
            if (blnSync)
                // ReSharper disable once MethodHasAsyncOverload
                objLocker = LockObject.EnterWriteLock(token);
            else
                objLockerAsync = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                Interlocked.Increment(ref _intLoading);
                try
                {
                    //Timekeeper.Start("load_char_attrib");
                    if (blnSync)
                    {
                        // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                        AttributeList.Clear();
                        // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                        SpecialAttributeList.Clear();
                    }
                    else
                    {
                        await AttributeList.ClearAsync(token).ConfigureAwait(false);
                        await SpecialAttributeList.ClearAsync(token).ConfigureAwait(false);
                    }

                    XPathNavigator xmlCharNode = blnSync
                        // ReSharper disable once MethodHasAsyncOverload
                        ? _objCharacter.GetNodeXPath(token)
                        : await _objCharacter.GetNodeXPathAsync(token).ConfigureAwait(false);
                    // We only want to remake attributes for shifters in career mode, because they only get their second set of attributes when exporting from create mode into career mode
                    XPathNavigator xmlCharNodeAnimalForm =
                        _objCharacter.MetatypeCategory == "Shapeshifter" && _objCharacter.Created
                            ? blnSync
                                // ReSharper disable once MethodHasAsyncOverload
                                ? _objCharacter.GetNodeXPath(true, token: token)
                                : await _objCharacter.GetNodeXPathAsync(true, token: token).ConfigureAwait(false)
                            : null;
                    foreach (string strAttribute in AttributeStrings)
                    {
                        XmlNodeList lstAttributeNodes =
                            xmlSavedCharacterNode.SelectNodes("attributes/attribute[name = " + strAttribute.CleanXPath()
                                                              +
                                                              ']');
                        // Couldn't find the appropriate attribute in the loaded file, so regenerate it from scratch.
                        if (lstAttributeNodes == null || lstAttributeNodes.Count == 0 || xmlCharNodeAnimalForm != null
                            && _objCharacter.LastSavedVersion < new ValueVersion(5, 200, 25))
                        {
                            CharacterAttrib objAttribute;
                            switch (CharacterAttrib.ConvertToAttributeCategory(strAttribute))
                            {
                                case CharacterAttrib.AttributeCategory.Special:
                                    objAttribute = new CharacterAttrib(_objCharacter, strAttribute,
                                                                       CharacterAttrib.AttributeCategory.Special);
                                    if (blnSync)
                                    {
                                        // ReSharper disable once MethodHasAsyncOverload
                                        objAttribute = RemakeAttribute(objAttribute, xmlCharNode, token);
                                        // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                                        SpecialAttributeList.Add(objAttribute);
                                    }
                                    else
                                    {
                                        objAttribute = await RemakeAttributeAsync(objAttribute, xmlCharNode, token).ConfigureAwait(false);
                                        await SpecialAttributeList.AddAsync(objAttribute, token).ConfigureAwait(false);
                                    }

                                    break;

                                case CharacterAttrib.AttributeCategory.Standard:
                                    objAttribute = new CharacterAttrib(_objCharacter, strAttribute,
                                                                       CharacterAttrib.AttributeCategory.Standard);
                                    if (blnSync)
                                    {
                                        // ReSharper disable once MethodHasAsyncOverload
                                        objAttribute = RemakeAttribute(objAttribute, xmlCharNode, token);
                                        // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                                        AttributeList.Add(objAttribute);
                                    }
                                    else
                                    {
                                        objAttribute = await RemakeAttributeAsync(objAttribute, xmlCharNode, token).ConfigureAwait(false);
                                        await AttributeList.AddAsync(objAttribute, token).ConfigureAwait(false);
                                    }

                                    break;

                                default:
                                    continue;
                            }

                            if (xmlCharNodeAnimalForm == null)
                                continue;
                            objAttribute = new CharacterAttrib(_objCharacter, strAttribute,
                                                               CharacterAttrib.AttributeCategory.Shapeshifter);
                            objAttribute = blnSync
                                // ReSharper disable once MethodHasAsyncOverload
                                ? RemakeAttribute(objAttribute, xmlCharNodeAnimalForm, token)
                                : await RemakeAttributeAsync(objAttribute, xmlCharNodeAnimalForm, token).ConfigureAwait(false);
                            switch (CharacterAttrib.ConvertToAttributeCategory(objAttribute.Abbrev))
                            {
                                case CharacterAttrib.AttributeCategory.Special:
                                    if (blnSync)
                                        // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                                        SpecialAttributeList.Add(objAttribute);
                                    else
                                        await SpecialAttributeList.AddAsync(objAttribute, token).ConfigureAwait(false);
                                    break;

                                case CharacterAttrib.AttributeCategory.Standard:
                                    if (blnSync)
                                        // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                                        AttributeList.Add(objAttribute);
                                    else
                                        await AttributeList.AddAsync(objAttribute, token).ConfigureAwait(false);
                                    break;

                                default:
                                    continue;
                            }
                        }
                        else
                        {
                            foreach (XmlNode xmlAttributeNode in lstAttributeNodes)
                            {
                                CharacterAttrib objAttribute;
                                switch (CharacterAttrib.ConvertToAttributeCategory(strAttribute))
                                {
                                    case CharacterAttrib.AttributeCategory.Special:
                                        objAttribute = new CharacterAttrib(_objCharacter, strAttribute,
                                                                           CharacterAttrib.AttributeCategory.Special);
                                        objAttribute.Load(xmlAttributeNode);
                                        if (blnSync)
                                            // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                                            SpecialAttributeList.Add(objAttribute);
                                        else
                                            await SpecialAttributeList.AddAsync(objAttribute, token).ConfigureAwait(false);
                                        break;

                                    case CharacterAttrib.AttributeCategory.Standard:
                                        objAttribute = new CharacterAttrib(_objCharacter, strAttribute,
                                                                           CharacterAttrib.AttributeCategory.Standard);
                                        objAttribute.Load(xmlAttributeNode);
                                        if (blnSync)
                                            // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                                            AttributeList.Add(objAttribute);
                                        else
                                            await AttributeList.AddAsync(objAttribute, token).ConfigureAwait(false);
                                        break;

                                    default:
                                        continue;
                                }
                            }
                        }
                    }

                    if (blnSync)
                        // ReSharper disable once MethodHasAsyncOverload
                        ResetBindings(token);
                    else
                        await ResetBindingsAsync(token).ConfigureAwait(false);
                }
                finally
                {
                    Interlocked.Decrement(ref _intLoading);
                }
                //Timekeeper.Finish("load_char_attrib");
            }
            finally
            {
                if (blnSync)
                    // ReSharper disable once MethodHasAsyncOverload
                    objLocker.Dispose();
                else
                    await objLockerAsync.DisposeAsync().ConfigureAwait(false);
            }
        }

        [CLSCompliant(false)]
        public void LoadFromHeroLab(XPathNavigator xmlStatBlockBaseNode, CustomActivity parentActivity, CancellationToken token = default)
        {
            if (xmlStatBlockBaseNode == null)
                return;
            using (LockObject.EnterWriteLock(token))
            using (Timekeeper.StartSyncron("load_char_attrib", parentActivity))
            {
                Interlocked.Increment(ref _intLoading);
                try
                {
                    AttributeList.Clear();
                    SpecialAttributeList.Clear();
                    XPathNavigator xmlCharNode = _objCharacter.GetNodeXPath(token);
                    // We only want to remake attributes for shifters in career mode, because they only get their second set of attributes when exporting from create mode into career mode
                    XPathNavigator xmlCharNodeAnimalForm =
                        _objCharacter.MetatypeCategory == "Shapeshifter" && _objCharacter.Created
                            ? _objCharacter.GetNodeXPath(true, token: token)
                            : null;
                    foreach (string strAttribute in AttributeStrings)
                    {
                        // First, remake the attribute

                        CharacterAttrib objAttribute = null;
                        switch (CharacterAttrib.ConvertToAttributeCategory(strAttribute))
                        {
                            case CharacterAttrib.AttributeCategory.Special:
                                objAttribute = new CharacterAttrib(_objCharacter, strAttribute,
                                    CharacterAttrib.AttributeCategory.Special);
                                objAttribute = RemakeAttribute(objAttribute, xmlCharNode, token);
                                SpecialAttributeList.Add(objAttribute);
                                break;

                            case CharacterAttrib.AttributeCategory.Standard:
                                objAttribute = new CharacterAttrib(_objCharacter, strAttribute,
                                    CharacterAttrib.AttributeCategory.Standard);
                                objAttribute = RemakeAttribute(objAttribute, xmlCharNode, token);
                                AttributeList.Add(objAttribute);
                                break;
                        }

                        if (xmlCharNodeAnimalForm != null)
                        {
                            switch (CharacterAttrib.ConvertToAttributeCategory(strAttribute))
                            {
                                case CharacterAttrib.AttributeCategory.Special:
                                    objAttribute = new CharacterAttrib(_objCharacter, strAttribute,
                                        CharacterAttrib.AttributeCategory.Special);
                                    objAttribute = RemakeAttribute(objAttribute, xmlCharNodeAnimalForm, token);
                                    SpecialAttributeList.Add(objAttribute);
                                    break;

                                case CharacterAttrib.AttributeCategory.Shapeshifter:
                                    objAttribute = new CharacterAttrib(_objCharacter, strAttribute,
                                        CharacterAttrib.AttributeCategory
                                            .Shapeshifter);
                                    objAttribute = RemakeAttribute(objAttribute, xmlCharNodeAnimalForm, token);
                                    AttributeList.Add(objAttribute);
                                    break;
                            }
                        }

                        // Then load in attribute karma levels (we'll adjust these later if the character is in Create mode)
                        if (strAttribute == "ESS"
                           ) // Not Essence though, this will get modified automatically instead of having its value set to the one HeroLab displays
                            continue;
                        XPathNavigator xmlHeroLabAttributeNode =
                            xmlStatBlockBaseNode.SelectSingleNode(
                                "attributes/attribute[@name = " + GetAttributeEnglishName(strAttribute).CleanXPath()
                                                                +
                                                                ']');
                        XPathNavigator xmlAttributeBaseNode =
                            xmlHeroLabAttributeNode?.SelectSingleNodeAndCacheExpression("@base", token);
                        if (xmlAttributeBaseNode != null &&
                            int.TryParse(xmlAttributeBaseNode.Value, out int intHeroLabAttributeBaseValue))
                        {
                            int intAttributeMinimumValue = GetAttributeByName(strAttribute, token).MetatypeMinimum;
                            if (intHeroLabAttributeBaseValue == intAttributeMinimumValue) continue;
                            if (objAttribute != null)
                                objAttribute.Karma = intHeroLabAttributeBaseValue - intAttributeMinimumValue;
                        }
                    }

                    if (!_objCharacter.Created && _objCharacter.EffectiveBuildMethodUsesPriorityTables)
                    {
                        // Allocate Attribute Points
                        int intAttributePointCount = _objCharacter.TotalAttributes;
                        CharacterAttrib objAttributeToPutPointsInto;
                        // First loop through attributes where costs can be 100% covered with points
                        do
                        {
                            objAttributeToPutPointsInto = null;
                            int intAttributeToPutPointsIntoTotalKarmaCost = 0;
                            foreach (CharacterAttrib objLoopAttribute in AttributeList)
                            {
                                if (objLoopAttribute.Karma == 0)
                                    continue;
                                // Put points into the attribute with the highest total karma cost.
                                // In case of ties, pick the one that would need more points to cover it (the other one will hopefully get picked up at a later cycle)
                                int intLoopTotalKarmaCost = objLoopAttribute.TotalKarmaCost;
                                if (objAttributeToPutPointsInto == null ||
                                    (objLoopAttribute.Karma <= intAttributePointCount &&
                                     (intLoopTotalKarmaCost > intAttributeToPutPointsIntoTotalKarmaCost ||
                                      (intLoopTotalKarmaCost == intAttributeToPutPointsIntoTotalKarmaCost &&
                                       objLoopAttribute.Karma > objAttributeToPutPointsInto.Karma))))
                                {
                                    objAttributeToPutPointsInto = objLoopAttribute;
                                    intAttributeToPutPointsIntoTotalKarmaCost = intLoopTotalKarmaCost;
                                }
                            }

                            if (objAttributeToPutPointsInto != null)
                            {
                                objAttributeToPutPointsInto.Base = objAttributeToPutPointsInto.Karma;
                                intAttributePointCount -= objAttributeToPutPointsInto.Karma;
                                objAttributeToPutPointsInto.Karma = 0;
                            }
                        } while (objAttributeToPutPointsInto != null && intAttributePointCount > 0);

                        // If any points left over, then put them all into the attribute with the highest karma cost
                        if (intAttributePointCount > 0 && AttributeList.Any(x => x.Karma != 0, token))
                        {
                            int intHighestTotalKarmaCost = 0;
                            foreach (CharacterAttrib objLoopAttribute in AttributeList)
                            {
                                if (objLoopAttribute.Karma == 0)
                                    continue;
                                // Put points into the attribute with the highest total karma cost.
                                // In case of ties, pick the one that would need more points to cover it (the other one will hopefully get picked up at a later cycle)
                                int intLoopTotalKarmaCost = objLoopAttribute.TotalKarmaCost;
                                if (objAttributeToPutPointsInto == null ||
                                    intLoopTotalKarmaCost > intHighestTotalKarmaCost ||
                                    (intLoopTotalKarmaCost == intHighestTotalKarmaCost &&
                                     objLoopAttribute.Karma > objAttributeToPutPointsInto.Karma))
                                {
                                    objAttributeToPutPointsInto = objLoopAttribute;
                                    intHighestTotalKarmaCost = intLoopTotalKarmaCost;
                                }
                            }

                            if (objAttributeToPutPointsInto != null)
                            {
                                objAttributeToPutPointsInto.Base = intAttributePointCount;
                                objAttributeToPutPointsInto.Karma -= intAttributePointCount;
                            }
                        }

                        // Allocate Special Attribute Points
                        intAttributePointCount = _objCharacter.TotalSpecial;
                        // First loop through attributes where costs can be 100% covered with points
                        do
                        {
                            objAttributeToPutPointsInto = null;
                            int intAttributeToPutPointsIntoTotalKarmaCost = 0;
                            foreach (CharacterAttrib objLoopAttribute in SpecialAttributeList)
                            {
                                if (objLoopAttribute.Karma == 0)
                                    continue;
                                // Put points into the attribute with the highest total karma cost.
                                // In case of ties, pick the one that would need more points to cover it (the other one will hopefully get picked up at a later cycle)
                                int intLoopTotalKarmaCost = objLoopAttribute.TotalKarmaCost;
                                if (objAttributeToPutPointsInto == null ||
                                    (objLoopAttribute.Karma <= intAttributePointCount &&
                                     (intLoopTotalKarmaCost > intAttributeToPutPointsIntoTotalKarmaCost ||
                                      (intLoopTotalKarmaCost == intAttributeToPutPointsIntoTotalKarmaCost &&
                                       objLoopAttribute.Karma > objAttributeToPutPointsInto.Karma))))
                                {
                                    objAttributeToPutPointsInto = objLoopAttribute;
                                    intAttributeToPutPointsIntoTotalKarmaCost = intLoopTotalKarmaCost;
                                }
                            }

                            if (objAttributeToPutPointsInto != null)
                            {
                                objAttributeToPutPointsInto.Base = objAttributeToPutPointsInto.Karma;
                                intAttributePointCount -= objAttributeToPutPointsInto.Karma;
                                objAttributeToPutPointsInto.Karma = 0;
                            }
                        } while (objAttributeToPutPointsInto != null);

                        // If any points left over, then put them all into the attribute with the highest karma cost
                        if (intAttributePointCount > 0 && SpecialAttributeList.Any(x => x.Karma != 0, token))
                        {
                            int intHighestTotalKarmaCost = 0;
                            foreach (CharacterAttrib objLoopAttribute in SpecialAttributeList)
                            {
                                if (objLoopAttribute.Karma == 0)
                                    continue;
                                // Put points into the attribute with the highest total karma cost.
                                // In case of ties, pick the one that would need more points to cover it (the other one will hopefully get picked up at a later cycle)
                                int intLoopTotalKarmaCost = objLoopAttribute.TotalKarmaCost;
                                if (objAttributeToPutPointsInto == null ||
                                    intLoopTotalKarmaCost > intHighestTotalKarmaCost ||
                                    (intLoopTotalKarmaCost == intHighestTotalKarmaCost &&
                                     objLoopAttribute.Karma > objAttributeToPutPointsInto.Karma))
                                {
                                    objAttributeToPutPointsInto = objLoopAttribute;
                                    intHighestTotalKarmaCost = intLoopTotalKarmaCost;
                                }
                            }

                            if (objAttributeToPutPointsInto != null)
                            {
                                objAttributeToPutPointsInto.Base = intAttributePointCount;
                                objAttributeToPutPointsInto.Karma -= intAttributePointCount;
                            }
                        }
                    }

                    ResetBindings(token);
                }
                finally
                {
                    Interlocked.Decrement(ref _intLoading);
                }
                //Timekeeper.Finish("load_char_attrib");
            }
        }

        private static CharacterAttrib RemakeAttribute(CharacterAttrib objNewAttribute, XPathNavigator objCharacterNode, CancellationToken token = default)
        {
            if (objNewAttribute == null)
                return null;
            if (objCharacterNode == null)
                throw new ArgumentNullException(nameof(objCharacterNode));
            string strAttributeLower = objNewAttribute.Abbrev.ToLowerInvariant();
            if (strAttributeLower == "magadept")
                strAttributeLower = "mag";
            int intMinValue = 1;
            int intMaxValue = 1;
            int intAugValue = 1;

            // This statement is wrapped in a try/catch since trying 1 div 2 results in an error with XSLT.
            try
            {
                (bool blnIsSuccess, object objProcess) = CommonFunctions.EvaluateInvariantXPath(
                    objCharacterNode.SelectSingleNode(strAttributeLower + "min")?.Value.Replace("/", " div ")
                                    .Replace('F', '0').Replace("1D6", "0").Replace("2D6", "0") ?? "1", token);
                if (blnIsSuccess)
                    intMinValue = ((double)objProcess).StandardRound();
            }
            catch (XPathException) { intMinValue = 1; }
            catch (OverflowException) { intMinValue = 1; }
            catch (InvalidCastException) { intMinValue = 1; }
            try
            {
                (bool blnIsSuccess, object objProcess) = CommonFunctions.EvaluateInvariantXPath(
                    objCharacterNode.SelectSingleNode(strAttributeLower + "max")?.Value.Replace("/", " div ")
                                    .Replace('F', '0').Replace("1D6", "0").Replace("2D6", "0") ?? "1", token);
                if (blnIsSuccess)
                    intMaxValue = ((double)objProcess).StandardRound();
            }
            catch (XPathException) { intMaxValue = 1; }
            catch (OverflowException) { intMaxValue = 1; }
            catch (InvalidCastException) { intMaxValue = 1; }
            try
            {
                (bool blnIsSuccess, object objProcess) = CommonFunctions.EvaluateInvariantXPath(
                    objCharacterNode.SelectSingleNode(strAttributeLower + "aug")?.Value.Replace("/", " div ")
                                    .Replace('F', '0').Replace("1D6", "0").Replace("2D6", "0") ?? "1", token);
                if (blnIsSuccess)
                    intAugValue = ((double)objProcess).StandardRound();
            }
            catch (XPathException) { intAugValue = 1; }
            catch (OverflowException) { intAugValue = 1; }
            catch (InvalidCastException) { intAugValue = 1; }

            objNewAttribute.AssignBaseKarmaLimits(
                objCharacterNode.SelectSingleNodeAndCacheExpression("base", token)?.ValueAsInt ?? 0,
                objCharacterNode.SelectSingleNodeAndCacheExpression("base", token)?.ValueAsInt ?? 0, intMinValue, intMaxValue,
                intAugValue);
            return objNewAttribute;
        }

        private static async Task<CharacterAttrib> RemakeAttributeAsync(CharacterAttrib objNewAttribute, XPathNavigator objCharacterNode, CancellationToken token = default)
        {
            if (objNewAttribute == null)
                return null;
            if (objCharacterNode == null)
                throw new ArgumentNullException(nameof(objCharacterNode));
            string strAttributeLower = objNewAttribute.Abbrev.ToLowerInvariant();
            if (strAttributeLower == "magadept")
                strAttributeLower = "mag";
            int intMinValue = 1;
            int intMaxValue = 1;
            int intAugValue = 1;

            // This statement is wrapped in a try/catch since trying 1 div 2 results in an error with XSLT.
            try
            {
                (bool blnIsSuccess, object objProcess) = await CommonFunctions.EvaluateInvariantXPathAsync(
                    objCharacterNode.SelectSingleNode(strAttributeLower + "min")?.Value.Replace("/", " div ")
                                    .Replace('F', '0').Replace("1D6", "0").Replace("2D6", "0") ?? "1", token).ConfigureAwait(false);
                if (blnIsSuccess)
                    intMinValue = ((double)objProcess).StandardRound();
            }
            catch (XPathException) { intMinValue = 1; }
            catch (OverflowException) { intMinValue = 1; }
            catch (InvalidCastException) { intMinValue = 1; }
            try
            {
                (bool blnIsSuccess, object objProcess) = await CommonFunctions.EvaluateInvariantXPathAsync(
                    objCharacterNode.SelectSingleNode(strAttributeLower + "max")?.Value.Replace("/", " div ")
                                    .Replace('F', '0').Replace("1D6", "0").Replace("2D6", "0") ?? "1", token).ConfigureAwait(false);
                if (blnIsSuccess)
                    intMaxValue = ((double)objProcess).StandardRound();
            }
            catch (XPathException) { intMaxValue = 1; }
            catch (OverflowException) { intMaxValue = 1; }
            catch (InvalidCastException) { intMaxValue = 1; }
            try
            {
                (bool blnIsSuccess, object objProcess) = await CommonFunctions.EvaluateInvariantXPathAsync(
                    objCharacterNode.SelectSingleNode(strAttributeLower + "aug")?.Value.Replace("/", " div ")
                                    .Replace('F', '0').Replace("1D6", "0").Replace("2D6", "0") ?? "1", token).ConfigureAwait(false);
                if (blnIsSuccess)
                    intAugValue = ((double)objProcess).StandardRound();
            }
            catch (XPathException) { intAugValue = 1; }
            catch (OverflowException) { intAugValue = 1; }
            catch (InvalidCastException) { intAugValue = 1; }

            await objNewAttribute.AssignBaseKarmaLimitsAsync(
                objCharacterNode.SelectSingleNodeAndCacheExpression("base", token)?.ValueAsInt ?? 0,
                objCharacterNode.SelectSingleNodeAndCacheExpression("base", token)?.ValueAsInt ?? 0, intMinValue, intMaxValue,
                intAugValue, token).ConfigureAwait(false);
            return objNewAttribute;
        }

        internal async Task Print(XmlWriter objWriter, CultureInfo objCulture, string strLanguageToPrint, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (await _objCharacter.GetMetatypeCategoryAsync(token).ConfigureAwait(false) == "Shapeshifter")
                {
                    XPathNavigator xmlNode = await _objCharacter.GetNodeXPathAsync(true, token: token).ConfigureAwait(false);

                    if (AttributeCategory == CharacterAttrib.AttributeCategory.Standard)
                    {
                        await objWriter.WriteElementStringAsync("attributecategory",
                                                                xmlNode != null
                                                                    ? xmlNode
                                                                        .SelectSingleNodeAndCacheExpression(
                                                                            "name/@translate", token: token)
                                                                    ?.Value ?? await _objCharacter.DisplayMetatypeAsync(strLanguageToPrint, token).ConfigureAwait(false)
                                                                    : await _objCharacter.DisplayMetatypeAsync(strLanguageToPrint, token).ConfigureAwait(false), token: token).ConfigureAwait(false);
                    }
                    else
                    {
                        xmlNode = xmlNode?.TryGetNodeById("metavariants/metavariant", await _objCharacter.GetMetavariantGuidAsync(token).ConfigureAwait(false));
                        await objWriter.WriteElementStringAsync("attributecategory",
                            xmlNode?.Value ?? await _objCharacter.GetMetavariantAsync(token).ConfigureAwait(false), token: token).ConfigureAwait(false);
                    }
                }

                await objWriter.WriteElementStringAsync("attributecategory_english", AttributeCategory.ToString(), token: token).ConfigureAwait(false);
                foreach (CharacterAttrib att in AllAttributes)
                {
                    await att.Print(objWriter, objCulture, strLanguageToPrint, token).ConfigureAwait(false);
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        #endregion Constructor, Save, Load, Print Methods

        #region Methods

        public CharacterAttrib GetAttributeByName(string abbrev, CancellationToken token = default)
        {
            CharacterAttrib objReturn;
            if (_objCharacter.MetatypeCategory == "Shapeshifter" && _objCharacter.Created)
            {
                using (LockObject.EnterReadLock(token))
                {
                    Tuple<string, CharacterAttrib.AttributeCategory> tupKey =
                        new Tuple<string, CharacterAttrib.AttributeCategory>(abbrev, AttributeCategory);
                    if (_dicAttributes.TryGetValue(tupKey, out objReturn))
                        return objReturn;
                    _dicAttributes.TryGetValue(
                        new Tuple<string, CharacterAttrib.AttributeCategory>(abbrev, CharacterAttrib.AttributeCategory.Special),
                        out objReturn);
                }
            }
            else
            {
                using (LockObject.EnterReadLock(token))
                {
                    if (_dicAttributes.TryGetValue(
                            new Tuple<string, CharacterAttrib.AttributeCategory>(abbrev,
                                CharacterAttrib.AttributeCategory.Standard), out objReturn))
                        return objReturn;
                    _dicAttributes.TryGetValue(
                        new Tuple<string, CharacterAttrib.AttributeCategory>(abbrev, CharacterAttrib.AttributeCategory.Special),
                        out objReturn);
                }
            }

            return objReturn;
        }

        public async Task<CharacterAttrib> GetAttributeByNameAsync(string abbrev, CancellationToken token = default)
        {
            CharacterAttrib objReturn;
            if (await _objCharacter.GetMetatypeCategoryAsync(token).ConfigureAwait(false) == "Shapeshifter"
                && await _objCharacter.GetCreatedAsync(token).ConfigureAwait(false))
            {
                token.ThrowIfCancellationRequested();
                IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    Tuple<string, CharacterAttrib.AttributeCategory> tupKey =
                        new Tuple<string, CharacterAttrib.AttributeCategory>(abbrev, await GetAttributeCategoryAsync(token).ConfigureAwait(false));
                    if (_dicAttributes.TryGetValue(tupKey, out objReturn))
                        return objReturn;
                    _dicAttributes.TryGetValue(
                        new Tuple<string, CharacterAttrib.AttributeCategory>(abbrev,
                            CharacterAttrib.AttributeCategory.Special), out objReturn);
                }
                finally
                {
                    await objLocker.DisposeAsync().ConfigureAwait(false);
                }
            }
            else
            {
                token.ThrowIfCancellationRequested();
                IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    if (_dicAttributes.TryGetValue(
                            new Tuple<string, CharacterAttrib.AttributeCategory>(abbrev,
                                CharacterAttrib.AttributeCategory.Standard), out objReturn))
                        return objReturn;
                    _dicAttributes.TryGetValue(
                        new Tuple<string, CharacterAttrib.AttributeCategory>(abbrev,
                            CharacterAttrib.AttributeCategory.Special), out objReturn);
                }
                finally
                {
                    await objLocker.DisposeAsync().ConfigureAwait(false);
                }
            }

            return objReturn;
        }

        public void RegisterPropertyChangedForActiveAttribute(string strAbbrev,
            MultiplePropertiesChangedEventHandler funcPropertyChanged)
        {
            UiPropertyChangerTracker objNewValue = new UiPropertyChangerTracker(strAbbrev);
            objNewValue.PropertyChangedList.Add(funcPropertyChanged);
            _dicUIPropertyChangers.AddOrUpdate(strAbbrev,
                objNewValue,
                (x, y) =>
                {
                    y.PropertyChangedList.Add(funcPropertyChanged);
                    return y;
                });
        }

        public void RegisterAsyncPropertyChangedForActiveAttribute(string strAbbrev,
            MultiplePropertiesChangedAsyncEventHandler funcPropertyChanged)
        {
            UiPropertyChangerTracker objNewValue = new UiPropertyChangerTracker(strAbbrev);
            objNewValue.AsyncPropertyChangedList.Add(funcPropertyChanged);
            _dicUIPropertyChangers.AddOrUpdate(strAbbrev,
                objNewValue,
                (x, y) =>
                {
                    y.AsyncPropertyChangedList.Add(funcPropertyChanged);
                    return y;
                });
        }

        public void DeregisterPropertyChangedForActiveAttribute(string strAbbrev,
            MultiplePropertiesChangedEventHandler funcPropertyChanged)
        {
            if (_dicUIPropertyChangers.TryGetValue(strAbbrev, out UiPropertyChangerTracker objEvents))
            {
                objEvents.PropertyChangedList.Remove(funcPropertyChanged);
            }
        }

        public void DeregisterAsyncPropertyChangedForActiveAttribute(string strAbbrev,
            MultiplePropertiesChangedAsyncEventHandler funcPropertyChanged)
        {
            if (_dicUIPropertyChangers.TryGetValue(strAbbrev, out UiPropertyChangerTracker objEvents))
            {
                objEvents.AsyncPropertyChangedList.Remove(funcPropertyChanged);
            }
        }

        private MultiplePropertiesChangedEventHandler RunExtraPropertyChanged(string strAbbrev)
        {
            switch (strAbbrev)
            {
                case "BOD":
                    return BODVariant;
                case "AGI":
                    return AGIVariant;
                case "REA":
                    return REAVariant;
                case "STR":
                    return STRVariant;
                case "CHA":
                    return CHAVariant;
                case "INT":
                    return INTVariant;
                case "LOG":
                    return LOGVariant;
                case "WIL":
                    return WILVariant;
                case "EDG":
                    return EDGVariant;
                case "ESS":
                    return ESSVariant;
                case "MAG":
                    return MAGVariant;
                case "MAGAdept":
                    return MAGAdeptVariant;
                case "RES":
                    return RESVariant;
                case "DEP":
                    return DEPVariant;
                default:
                    throw new ArgumentException("Invalid attribute key provided.", nameof(strAbbrev));
            }

            void CommonCode(UiPropertyChangerTracker objEvents, MultiplePropertiesChangedEventArgs e)
            {
                if (objEvents.AsyncPropertyChangedList.Count != 0)
                {
                    Func<Task>[] aFuncs = new Func<Task>[objEvents.AsyncPropertyChangedList.Count];
                    for (int i = 0; i < objEvents.AsyncPropertyChangedList.Count; ++i)
                    {
                        int i1 = i;
                        aFuncs[i] = () => objEvents.AsyncPropertyChangedList[i1].Invoke(this, e);
                    }

                    Utils.RunWithoutThreadLock(aFuncs);
                    if (objEvents.PropertyChangedList.Count != 0)
                    {
                        Utils.RunOnMainThread(() =>
                        {
                            foreach (MultiplePropertiesChangedEventHandler funcToRun in objEvents.PropertyChangedList)
                            {
                                funcToRun?.Invoke(this, e);
                            }
                        });
                    }
                }
                else if (objEvents.PropertyChangedList.Count != 0)
                {
                    Utils.RunOnMainThread(() =>
                    {
                        foreach (MultiplePropertiesChangedEventHandler funcToRun in objEvents.PropertyChangedList)
                        {
                            funcToRun?.Invoke(this, e);
                        }
                    });
                }
            }

            void BODVariant(object sender, MultiplePropertiesChangedEventArgs e)
            {
                if (_dicUIPropertyChangers.TryGetValue("BOD", out UiPropertyChangerTracker objEvents))
                    CommonCode(objEvents, e);
            }
            void AGIVariant(object sender, MultiplePropertiesChangedEventArgs e)
            {
                if (_dicUIPropertyChangers.TryGetValue("AGI", out UiPropertyChangerTracker objEvents))
                    CommonCode(objEvents, e);
            }
            void REAVariant(object sender, MultiplePropertiesChangedEventArgs e)
            {
                if (_dicUIPropertyChangers.TryGetValue("REA", out UiPropertyChangerTracker objEvents))
                    CommonCode(objEvents, e);
            }
            void STRVariant(object sender, MultiplePropertiesChangedEventArgs e)
            {
                if (_dicUIPropertyChangers.TryGetValue("STR", out UiPropertyChangerTracker objEvents))
                    CommonCode(objEvents, e);
            }
            void CHAVariant(object sender, MultiplePropertiesChangedEventArgs e)
            {
                if (_dicUIPropertyChangers.TryGetValue("CHA", out UiPropertyChangerTracker objEvents))
                    CommonCode(objEvents, e);
            }
            void INTVariant(object sender, MultiplePropertiesChangedEventArgs e)
            {
                if (_dicUIPropertyChangers.TryGetValue("INT", out UiPropertyChangerTracker objEvents))
                    CommonCode(objEvents, e);
            }
            void LOGVariant(object sender, MultiplePropertiesChangedEventArgs e)
            {
                if (_dicUIPropertyChangers.TryGetValue("LOG", out UiPropertyChangerTracker objEvents))
                    CommonCode(objEvents, e);
            }
            void WILVariant(object sender, MultiplePropertiesChangedEventArgs e)
            {
                if (_dicUIPropertyChangers.TryGetValue("WIL", out UiPropertyChangerTracker objEvents))
                    CommonCode(objEvents, e);
            }
            void EDGVariant(object sender, MultiplePropertiesChangedEventArgs e)
            {
                if (_dicUIPropertyChangers.TryGetValue("EDG", out UiPropertyChangerTracker objEvents))
                    CommonCode(objEvents, e);
            }
            void ESSVariant(object sender, MultiplePropertiesChangedEventArgs e)
            {
                if (_dicUIPropertyChangers.TryGetValue("ESS", out UiPropertyChangerTracker objEvents))
                    CommonCode(objEvents, e);
            }
            void MAGVariant(object sender, MultiplePropertiesChangedEventArgs e)
            {
                if (_dicUIPropertyChangers.TryGetValue("MAG", out UiPropertyChangerTracker objEvents))
                    CommonCode(objEvents, e);
            }
            void MAGAdeptVariant(object sender, MultiplePropertiesChangedEventArgs e)
            {
                if (_dicUIPropertyChangers.TryGetValue("MAGAdept", out UiPropertyChangerTracker objEvents))
                    CommonCode(objEvents, e);
            }
            void RESVariant(object sender, MultiplePropertiesChangedEventArgs e)
            {
                if (_dicUIPropertyChangers.TryGetValue("RES", out UiPropertyChangerTracker objEvents))
                    CommonCode(objEvents, e);
            }
            void DEPVariant(object sender, MultiplePropertiesChangedEventArgs e)
            {
                if (_dicUIPropertyChangers.TryGetValue("DEP", out UiPropertyChangerTracker objEvents))
                    CommonCode(objEvents, e);
            }
        }

        private MultiplePropertiesChangedAsyncEventHandler RunExtraAsyncPropertyChanged(string strAbbrev)
        {
            switch (strAbbrev)
            {
                case "BOD":
                    return BODVariant;
                case "AGI":
                    return AGIVariant;
                case "REA":
                    return REAVariant;
                case "STR":
                    return STRVariant;
                case "CHA":
                    return CHAVariant;
                case "INT":
                    return INTVariant;
                case "LOG":
                    return LOGVariant;
                case "WIL":
                    return WILVariant;
                case "EDG":
                    return EDGVariant;
                case "ESS":
                    return ESSVariant;
                case "MAG":
                    return MAGVariant;
                case "MAGAdept":
                    return MAGAdeptVariant;
                case "RES":
                    return RESVariant;
                case "DEP":
                    return DEPVariant;
                default:
                    throw new ArgumentException("Invalid attribute key provided.", nameof(strAbbrev));
            }

            async Task CommonCode(UiPropertyChangerTracker objEvents, MultiplePropertiesChangedEventArgs e, CancellationToken token = default)
            {
                if (objEvents.AsyncPropertyChangedList.Count != 0)
                {
                    List<Task> lstTasks = new List<Task>(Math.Min(objEvents.AsyncPropertyChangedList.Count, Utils.MaxParallelBatchSize));
                    int i = 0;
                    foreach (MultiplePropertiesChangedAsyncEventHandler objEvent in objEvents.AsyncPropertyChangedList)
                    {
                        lstTasks.Add(objEvent.Invoke(this, e, token));
                        if (++i < Utils.MaxParallelBatchSize)
                            continue;
                        await Task.WhenAll(lstTasks).ConfigureAwait(false);
                        lstTasks.Clear();
                        i = 0;
                    }
                    await Task.WhenAll(lstTasks).ConfigureAwait(false);

                    if (objEvents.PropertyChangedList.Count != 0)
                    {
                        await Utils.RunOnMainThreadAsync(() =>
                        {
                            foreach (MultiplePropertiesChangedEventHandler funcToRun in objEvents.PropertyChangedList)
                            {
                                funcToRun?.Invoke(this, e);
                            }
                        }, token).ConfigureAwait(false);
                    }
                }
                else if (objEvents.PropertyChangedList.Count != 0)
                {
                    await Utils.RunOnMainThreadAsync(() =>
                    {
                        foreach (MultiplePropertiesChangedEventHandler funcToRun in objEvents.PropertyChangedList)
                        {
                            funcToRun?.Invoke(this, e);
                        }
                    }, token).ConfigureAwait(false);
                }
            }

            Task BODVariant(object sender, MultiplePropertiesChangedEventArgs e, CancellationToken token = default)
            {
                if (token.IsCancellationRequested)
                    return Task.FromCanceled(token);
                return _dicUIPropertyChangers.TryGetValue("BOD", out UiPropertyChangerTracker objEvents)
                    ? CommonCode(objEvents, e, token)
                    : Task.CompletedTask;
            }
            Task AGIVariant(object sender, MultiplePropertiesChangedEventArgs e, CancellationToken token = default)
            {
                if (token.IsCancellationRequested)
                    return Task.FromCanceled(token);
                return _dicUIPropertyChangers.TryGetValue("AGI", out UiPropertyChangerTracker objEvents)
                    ? CommonCode(objEvents, e, token)
                    : Task.CompletedTask;
            }
            Task REAVariant(object sender, MultiplePropertiesChangedEventArgs e, CancellationToken token = default)
            {
                if (token.IsCancellationRequested)
                    return Task.FromCanceled(token);
                return _dicUIPropertyChangers.TryGetValue("REA", out UiPropertyChangerTracker objEvents)
                    ? CommonCode(objEvents, e, token)
                    : Task.CompletedTask;
            }
            Task STRVariant(object sender, MultiplePropertiesChangedEventArgs e, CancellationToken token = default)
            {
                if (token.IsCancellationRequested)
                    return Task.FromCanceled(token);
                return _dicUIPropertyChangers.TryGetValue("STR", out UiPropertyChangerTracker objEvents)
                    ? CommonCode(objEvents, e, token)
                    : Task.CompletedTask;
            }
            Task CHAVariant(object sender, MultiplePropertiesChangedEventArgs e, CancellationToken token = default)
            {
                if (token.IsCancellationRequested)
                    return Task.FromCanceled(token);
                return _dicUIPropertyChangers.TryGetValue("CHA", out UiPropertyChangerTracker objEvents)
                    ? CommonCode(objEvents, e, token)
                    : Task.CompletedTask;
            }
            Task INTVariant(object sender, MultiplePropertiesChangedEventArgs e, CancellationToken token = default)
            {
                if (token.IsCancellationRequested)
                    return Task.FromCanceled(token);
                return _dicUIPropertyChangers.TryGetValue("INT", out UiPropertyChangerTracker objEvents)
                    ? CommonCode(objEvents, e, token)
                    : Task.CompletedTask;
            }
            Task LOGVariant(object sender, MultiplePropertiesChangedEventArgs e, CancellationToken token = default)
            {
                if (token.IsCancellationRequested)
                    return Task.FromCanceled(token);
                return _dicUIPropertyChangers.TryGetValue("LOG", out UiPropertyChangerTracker objEvents)
                    ? CommonCode(objEvents, e, token)
                    : Task.CompletedTask;
            }
            Task WILVariant(object sender, MultiplePropertiesChangedEventArgs e, CancellationToken token = default)
            {
                if (token.IsCancellationRequested)
                    return Task.FromCanceled(token);
                return _dicUIPropertyChangers.TryGetValue("WIL", out UiPropertyChangerTracker objEvents)
                    ? CommonCode(objEvents, e, token)
                    : Task.CompletedTask;
            }
            Task EDGVariant(object sender, MultiplePropertiesChangedEventArgs e, CancellationToken token = default)
            {
                if (token.IsCancellationRequested)
                    return Task.FromCanceled(token);
                return _dicUIPropertyChangers.TryGetValue("EDG", out UiPropertyChangerTracker objEvents)
                    ? CommonCode(objEvents, e, token)
                    : Task.CompletedTask;
            }
            Task ESSVariant(object sender, MultiplePropertiesChangedEventArgs e, CancellationToken token = default)
            {
                if (token.IsCancellationRequested)
                    return Task.FromCanceled(token);
                return _dicUIPropertyChangers.TryGetValue("ESS", out UiPropertyChangerTracker objEvents)
                    ? CommonCode(objEvents, e, token)
                    : Task.CompletedTask;
            }
            Task MAGVariant(object sender, MultiplePropertiesChangedEventArgs e, CancellationToken token = default)
            {
                if (token.IsCancellationRequested)
                    return Task.FromCanceled(token);
                return _dicUIPropertyChangers.TryGetValue("MAG", out UiPropertyChangerTracker objEvents)
                    ? CommonCode(objEvents, e, token)
                    : Task.CompletedTask;
            }
            Task MAGAdeptVariant(object sender, MultiplePropertiesChangedEventArgs e, CancellationToken token = default)
            {
                if (token.IsCancellationRequested)
                    return Task.FromCanceled(token);
                return _dicUIPropertyChangers.TryGetValue("MAGAdept", out UiPropertyChangerTracker objEvents)
                    ? CommonCode(objEvents, e, token)
                    : Task.CompletedTask;
            }
            Task RESVariant(object sender, MultiplePropertiesChangedEventArgs e, CancellationToken token = default)
            {
                if (token.IsCancellationRequested)
                    return Task.FromCanceled(token);
                return _dicUIPropertyChangers.TryGetValue("RES", out UiPropertyChangerTracker objEvents)
                    ? CommonCode(objEvents, e, token)
                    : Task.CompletedTask;
            }
            Task DEPVariant(object sender, MultiplePropertiesChangedEventArgs e, CancellationToken token = default)
            {
                if (token.IsCancellationRequested)
                    return Task.FromCanceled(token);
                return _dicUIPropertyChangers.TryGetValue("DEP", out UiPropertyChangerTracker objEvents)
                    ? CommonCode(objEvents, e, token)
                    : Task.CompletedTask;
            }
        }

        internal void ForceAttributePropertyChangedNotificationAll(params string[] lstNames)
        {
            foreach (CharacterAttrib att in AttributeList)
            {
                att.OnMultiplePropertiesChanged(Array.AsReadOnly(lstNames));
            }
        }

        internal Task ForceAttributePropertyChangedNotificationAllAsync(params string[] lstNames)
        {
            return ForceAttributePropertyChangedNotificationAllAsync(default, lstNames);
        }

        internal Task ForceAttributePropertyChangedNotificationAllAsync(CancellationToken token, params string[] lstNames)
        {
            ReadOnlyCollection<string> lstNamesToUse = Array.AsReadOnly(lstNames);
            return AttributeList.ForEachWithSideEffectsAsync(
                att => att.OnMultiplePropertiesChangedAsync(lstNamesToUse, token), token);
        }

        public static void CopyAttribute(CharacterAttrib objSource, CharacterAttrib objTarget, string strMetavariantXPath, XPathNavigator xmlDoc)
        {
            if (objSource == null || objTarget == null)
                return;
            using (objTarget.LockObject.EnterUpgradeableReadLock())
            {
                int intBase;
                int intKarma;
                int intMinimum = 0;
                int intMaximum = 0;
                int intAugmentedMaximum = 0;
                using (objSource.LockObject.EnterReadLock())
                {
                    intBase = objSource.Base;
                    intKarma = objSource.Karma;
                    string strSourceAbbrev = objSource.Abbrev.ToLowerInvariant();
                    if (strSourceAbbrev == "magadept")
                        strSourceAbbrev = "mag";
                    XPathNavigator node = !string.IsNullOrEmpty(strMetavariantXPath)
                        ? xmlDoc?.SelectSingleNode(strMetavariantXPath)
                        : null;
                    if (node != null)
                    {
                        int.TryParse(node.SelectSingleNodeAndCacheExpression(strSourceAbbrev + "min")?.Value,
                            NumberStyles.Any,
                            GlobalSettings.InvariantCultureInfo, out intMinimum);
                        int.TryParse(node.SelectSingleNodeAndCacheExpression(strSourceAbbrev + "max")?.Value,
                            NumberStyles.Any,
                            GlobalSettings.InvariantCultureInfo, out intMaximum);
                        int.TryParse(node.SelectSingleNodeAndCacheExpression(strSourceAbbrev + "aug")?.Value,
                            NumberStyles.Any,
                            GlobalSettings.InvariantCultureInfo, out intAugmentedMaximum);
                        intMaximum = Math.Max(intMaximum, intMinimum);
                        intAugmentedMaximum = Math.Max(intAugmentedMaximum, intMaximum);
                    }
                }

                objTarget.AssignBaseKarmaLimits(intBase, intKarma, intMinimum, intMaximum, intAugmentedMaximum);
            }
        }

        public static async Task CopyAttributeAsync(CharacterAttrib objSource, CharacterAttrib objTarget,
            string strMetavariantXPath, XPathNavigator xmlDoc, CancellationToken token = default)
        {
            if (objSource == null || objTarget == null)
                return;
            IAsyncDisposable objLocker =
                await objTarget.LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                int intBase;
                int intKarma;
                int intMinimum = 0;
                int intMaximum = 0;
                int intAugmentedMaximum = 0;
                IAsyncDisposable objLocker2 =
                    await objSource.LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    intBase = await objSource.GetBaseAsync(token).ConfigureAwait(false);
                    intKarma = await objSource.GetKarmaAsync(token).ConfigureAwait(false);
                    string strSourceAbbrev = objSource.Abbrev.ToLowerInvariant();
                    if (strSourceAbbrev == "magadept")
                        strSourceAbbrev = "mag";
                    XPathNavigator node = !string.IsNullOrEmpty(strMetavariantXPath)
                        ? xmlDoc?.SelectSingleNode(strMetavariantXPath)
                        : null;
                    if (node != null)
                    {
                        int.TryParse(node.SelectSingleNodeAndCacheExpression(strSourceAbbrev + "min", token)?.Value,
                            NumberStyles.Any,
                            GlobalSettings.InvariantCultureInfo, out intMinimum);
                        int.TryParse(node.SelectSingleNodeAndCacheExpression(strSourceAbbrev + "max", token)?.Value,
                            NumberStyles.Any,
                            GlobalSettings.InvariantCultureInfo, out intMaximum);
                        int.TryParse(node.SelectSingleNodeAndCacheExpression(strSourceAbbrev + "aug", token)?.Value,
                            NumberStyles.Any,
                            GlobalSettings.InvariantCultureInfo, out intAugmentedMaximum);
                        intMaximum = Math.Max(intMaximum, intMinimum);
                        intAugmentedMaximum = Math.Max(intAugmentedMaximum, intMaximum);
                    }
                }
                finally
                {
                    await objLocker2.DisposeAsync().ConfigureAwait(false);
                }

                await objTarget
                    .AssignBaseKarmaLimitsAsync(intBase, intKarma, intMinimum, intMaximum, intAugmentedMaximum, token)
                    .ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public string ProcessAttributesInXPath(string strInput, IReadOnlyDictionary<string, int> dicValueOverrides = null, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (string.IsNullOrEmpty(strInput))
                return strInput;
            using (LockObject.EnterReadLock(token))
            {
                string strReturn = strInput;
                foreach (string strCharAttributeName in AttributeStrings)
                {
                    token.ThrowIfCancellationRequested();
                    Lazy<CharacterAttrib> objAttribute = new Lazy<CharacterAttrib>(() => _objCharacter.GetAttribute(strCharAttributeName, token: token));
                    string strNeedleCommon = '{' + strCharAttributeName;
                    strReturn = strReturn
                                .CheapReplace(strNeedleCommon + '}', () =>
                                                  (dicValueOverrides != null && dicValueOverrides.TryGetValue(strCharAttributeName, out int intOverride)
                                                      ? intOverride
                                                      : objAttribute.Value.TotalValue)
                                                  .ToString(GlobalSettings.InvariantCultureInfo))
                                .CheapReplace(strNeedleCommon + "Unaug}", () =>
                                                  (dicValueOverrides != null && dicValueOverrides.TryGetValue(strCharAttributeName + "Unaug", out int intOverride)
                                                      ? intOverride
                                                      : objAttribute.Value.Value)
                                                  .ToString(GlobalSettings.InvariantCultureInfo))
                                .CheapReplace(strNeedleCommon + "Base}", () =>
                                                  (dicValueOverrides != null && dicValueOverrides.TryGetValue(strCharAttributeName + "Base", out int intOverride)
                                                      ? intOverride
                                                      : objAttribute.Value.TotalBase)
                                                  .ToString(GlobalSettings.InvariantCultureInfo))
                                .CheapReplace(strNeedleCommon + "Minimum}", () =>
                                    (dicValueOverrides != null && dicValueOverrides.TryGetValue(strCharAttributeName + "Minimum", out int intOverride)
                                        ? intOverride
                                        : objAttribute.Value.TotalMinimum)
                                    .ToString(GlobalSettings.InvariantCultureInfo))
                                .CheapReplace(strNeedleCommon + "Maximum}", () =>
                                    (dicValueOverrides != null && dicValueOverrides.TryGetValue(strCharAttributeName + "Maximum", out int intOverride)
                                        ? intOverride
                                        : objAttribute.Value.TotalMaximum)
                                    .ToString(GlobalSettings.InvariantCultureInfo));
                }

                return strReturn;
            }
        }

        public void ProcessAttributesInXPath(StringBuilder sbdInput, string strOriginal = "", IReadOnlyDictionary<string, int> dicValueOverrides = null, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (sbdInput == null || sbdInput.Length <= 0)
                return;
            if (string.IsNullOrEmpty(strOriginal))
                strOriginal = sbdInput.ToString();
            using (LockObject.EnterReadLock(token))
            {
                foreach (string strCharAttributeName in AttributeStrings)
                {
                    token.ThrowIfCancellationRequested();
                    Lazy<CharacterAttrib> objAttribute = new Lazy<CharacterAttrib>(() => _objCharacter.GetAttribute(strCharAttributeName, token: token));
                    string strNeedleCommon = '{' + strCharAttributeName;
                    sbdInput.CheapReplace(strOriginal, strNeedleCommon + '}', () =>
                        (dicValueOverrides != null && dicValueOverrides.TryGetValue(strCharAttributeName, out int intOverride)
                            ? intOverride
                            : objAttribute.Value.TotalValue)
                        .ToString(GlobalSettings.InvariantCultureInfo));
                    sbdInput.CheapReplace(strOriginal, strNeedleCommon + "Unaug}", () =>
                        (dicValueOverrides != null && dicValueOverrides.TryGetValue(strCharAttributeName + "Unaug", out int intOverride)
                            ? intOverride
                            : objAttribute.Value.Value)
                        .ToString(GlobalSettings.InvariantCultureInfo));
                    sbdInput.CheapReplace(strOriginal, strNeedleCommon + "Base}", () =>
                        (dicValueOverrides != null && dicValueOverrides.TryGetValue(strCharAttributeName + "Base", out int intOverride)
                            ? intOverride
                            : objAttribute.Value.TotalBase)
                        .ToString(GlobalSettings.InvariantCultureInfo));
                    sbdInput.CheapReplace(strOriginal, strNeedleCommon + "Minimum}", () =>
                        (dicValueOverrides != null && dicValueOverrides.TryGetValue(strCharAttributeName + "Minimum", out int intOverride)
                            ? intOverride
                            : objAttribute.Value.TotalMinimum)
                        .ToString(GlobalSettings.InvariantCultureInfo));
                    sbdInput.CheapReplace(strOriginal, strNeedleCommon + "Maximum}", () =>
                        (dicValueOverrides != null && dicValueOverrides.TryGetValue(strCharAttributeName + "Maximum", out int intOverride)
                            ? intOverride
                            : objAttribute.Value.TotalMaximum)
                        .ToString(GlobalSettings.InvariantCultureInfo));
                }
            }
        }

        public async Task<string> ProcessAttributesInXPathAsync(string strInput, IReadOnlyDictionary<string, int> dicValueOverrides = null, CancellationToken token = default)
        {
            if (string.IsNullOrEmpty(strInput))
                return strInput;
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                string strReturn = strInput;
                foreach (string strCharAttributeName in AttributeStrings)
                {
                    Microsoft.VisualStudio.Threading.AsyncLazy<CharacterAttrib> objAttribute =
                        new Microsoft.VisualStudio.Threading.AsyncLazy<CharacterAttrib>(
                            () => _objCharacter.GetAttributeAsync(strCharAttributeName, token: token), Utils.JoinableTaskFactory);
                    string strNeedleCommon = '{' + strCharAttributeName;
                    strReturn = await (await (await (await (await strReturn
                                        .CheapReplaceAsync(strNeedleCommon + '}', async () =>
                                            (dicValueOverrides != null && dicValueOverrides.TryGetValue(strCharAttributeName, out int intOverride)
                                                ? intOverride
                                                : await (await objAttribute.GetValueAsync(token).ConfigureAwait(false)).GetTotalValueAsync(token).ConfigureAwait(false))
                                            .ToString(GlobalSettings.InvariantCultureInfo), token: token)
                                        .ConfigureAwait(false))
                                    .CheapReplaceAsync(strNeedleCommon + "Unaug}", async () =>
                                        (dicValueOverrides != null && dicValueOverrides.TryGetValue(strCharAttributeName + "Unaug", out int intOverride)
                                            ? intOverride
                                            : await (await objAttribute.GetValueAsync(token).ConfigureAwait(false)).GetValueAsync(token).ConfigureAwait(false))
                                        .ToString(GlobalSettings.InvariantCultureInfo), token: token)
                                    .ConfigureAwait(false))
                                .CheapReplaceAsync(strNeedleCommon + "Base}", async () =>
                                    (dicValueOverrides != null && dicValueOverrides.TryGetValue(strCharAttributeName + "Base", out int intOverride)
                                        ? intOverride
                                        : await (await objAttribute.GetValueAsync(token).ConfigureAwait(false)).GetTotalBaseAsync(token).ConfigureAwait(false))
                                    .ToString(GlobalSettings.InvariantCultureInfo), token: token).ConfigureAwait(false))
                            .CheapReplaceAsync(strNeedleCommon + "Minimum}", async () =>
                                (dicValueOverrides != null && dicValueOverrides.TryGetValue(strCharAttributeName + "Minimum", out int intOverride)
                                    ? intOverride
                                    : await (await objAttribute.GetValueAsync(token).ConfigureAwait(false)).GetTotalMinimumAsync(token).ConfigureAwait(false))
                                .ToString(GlobalSettings.InvariantCultureInfo), token: token).ConfigureAwait(false))
                        .CheapReplaceAsync(strNeedleCommon + "Maximum}", async () =>
                            (dicValueOverrides != null && dicValueOverrides.TryGetValue(strCharAttributeName + "Maximum", out int intOverride)
                                ? intOverride
                                : await (await objAttribute.GetValueAsync(token).ConfigureAwait(false)).GetTotalMaximumAsync(token).ConfigureAwait(false))
                            .ToString(GlobalSettings.InvariantCultureInfo), token: token).ConfigureAwait(false);
                }

                return strReturn;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public async Task ProcessAttributesInXPathAsync(StringBuilder sbdInput, string strOriginal = "", IReadOnlyDictionary<string, int> dicValueOverrides = null, CancellationToken token = default)
        {
            if (sbdInput == null || sbdInput.Length <= 0)
                return;
            if (string.IsNullOrEmpty(strOriginal))
                strOriginal = sbdInput.ToString();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                foreach (string strCharAttributeName in AttributeStrings)
                {
                    Microsoft.VisualStudio.Threading.AsyncLazy<CharacterAttrib> objAttribute =
                        new Microsoft.VisualStudio.Threading.AsyncLazy<CharacterAttrib>(
                            () => _objCharacter.GetAttributeAsync(strCharAttributeName, token: token), Utils.JoinableTaskFactory);
                    string strNeedleCommon = '{' + strCharAttributeName;
                    await sbdInput.CheapReplaceAsync(strOriginal, strNeedleCommon + '}', async () =>
                        (dicValueOverrides != null && dicValueOverrides.TryGetValue(strCharAttributeName, out int intOverride)
                            ? intOverride
                            : await (await objAttribute.GetValueAsync(token).ConfigureAwait(false)).GetTotalValueAsync(token).ConfigureAwait(false))
                        .ToString(GlobalSettings.InvariantCultureInfo), token: token).ConfigureAwait(false);
                    await sbdInput.CheapReplaceAsync(strOriginal, strNeedleCommon + "Unaug}", async () =>
                        (dicValueOverrides != null && dicValueOverrides.TryGetValue(strCharAttributeName + "Unaug", out int intOverride)
                            ? intOverride
                            : await (await objAttribute.GetValueAsync(token).ConfigureAwait(false)).GetValueAsync(token).ConfigureAwait(false))
                        .ToString(GlobalSettings.InvariantCultureInfo), token: token).ConfigureAwait(false);
                    await sbdInput.CheapReplaceAsync(strOriginal, strNeedleCommon + "Base}", async () =>
                        (dicValueOverrides != null && dicValueOverrides.TryGetValue(strCharAttributeName + "Base", out int intOverride)
                            ? intOverride
                            : await (await objAttribute.GetValueAsync(token).ConfigureAwait(false)).GetTotalBaseAsync(token).ConfigureAwait(false))
                        .ToString(GlobalSettings.InvariantCultureInfo), token: token).ConfigureAwait(false);
                    await sbdInput.CheapReplaceAsync(strOriginal, strNeedleCommon + "Minimum}", async () =>
                        (dicValueOverrides != null && dicValueOverrides.TryGetValue(strCharAttributeName + "Minimum", out int intOverride)
                            ? intOverride
                            : await (await objAttribute.GetValueAsync(token).ConfigureAwait(false)).GetTotalMinimumAsync(token).ConfigureAwait(false))
                        .ToString(GlobalSettings.InvariantCultureInfo), token: token).ConfigureAwait(false);
                    await sbdInput.CheapReplaceAsync(strOriginal, strNeedleCommon + "Maximum}", async () =>
                        (dicValueOverrides != null && dicValueOverrides.TryGetValue(strCharAttributeName + "Maximum", out int intOverride)
                            ? intOverride
                            : await (await objAttribute.GetValueAsync(token).ConfigureAwait(false)).GetTotalMaximumAsync(token).ConfigureAwait(false))
                        .ToString(GlobalSettings.InvariantCultureInfo), token: token).ConfigureAwait(false);
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public string ProcessAttributesInXPathForTooltip(string strInput, CultureInfo objCultureInfo = null, string strLanguage = "", bool blnShowValues = true, IReadOnlyDictionary<string, int> dicValueOverrides = null, CancellationToken token = default)
        {
            if (string.IsNullOrEmpty(strInput))
                return strInput;
            if (objCultureInfo == null)
                objCultureInfo = GlobalSettings.CultureInfo;
            if (string.IsNullOrEmpty(strLanguage))
                strLanguage = GlobalSettings.Language;
            string strSpace = LanguageManager.GetString("String_Space", strLanguage, token: token);
            string strReturn = strInput;
            using (LockObject.EnterReadLock(token))
            {
                foreach (string strCharAttributeName in AttributeStrings)
                {
                    Lazy<CharacterAttrib> objAttribute = new Lazy<CharacterAttrib>(() => _objCharacter.GetAttribute(strCharAttributeName, token: token));
                    string strNeedleCommon = '{' + strCharAttributeName;
                    strReturn = strReturn
                                .CheapReplace(strNeedleCommon + '}', () =>
                                {
                                    string strInnerReturn = objAttribute.Value.DisplayNameShort(strLanguage);
                                    if (blnShowValues)
                                    {
                                        if (dicValueOverrides == null
                                            || !dicValueOverrides.TryGetValue(
                                                strCharAttributeName, out int intAttributeValue))
                                            intAttributeValue = objAttribute.Value.TotalValue;
                                        strInnerReturn += strSpace + '(' + intAttributeValue.ToString(objCultureInfo)
                                                          + ')';
                                    }

                                    return strInnerReturn;
                                })
                                .CheapReplace(strNeedleCommon + "Unaug}", () =>
                                {
                                    string strInnerReturn = objAttribute.Value.DisplayNameShort(strLanguage);
                                    if (blnShowValues)
                                    {
                                        if (dicValueOverrides == null
                                            || !dicValueOverrides.TryGetValue(
                                                strCharAttributeName + "Unaug", out int intAttributeValue))
                                            intAttributeValue = objAttribute.Value.Value;
                                        strInnerReturn += strSpace + '(' + intAttributeValue.ToString(objCultureInfo)
                                                          + ')';
                                    }

                                    return string.Format(objCultureInfo,
                                                         LanguageManager.GetString(
                                                             "String_NaturalAttribute", strLanguage, token: token), strInnerReturn);
                                })
                                .CheapReplace(strNeedleCommon + "Base}", () =>
                                {
                                    string strInnerReturn = objAttribute.Value.DisplayNameShort(strLanguage);
                                    if (blnShowValues)
                                    {
                                        if (dicValueOverrides == null
                                            || !dicValueOverrides.TryGetValue(
                                                strCharAttributeName + "Base", out int intAttributeValue))
                                            intAttributeValue = objAttribute.Value.TotalBase;
                                        strInnerReturn += strSpace + '(' + intAttributeValue.ToString(objCultureInfo)
                                                          + ')';
                                    }

                                    return string.Format(objCultureInfo,
                                                         LanguageManager.GetString("String_BaseAttribute", strLanguage, token: token),
                                                         strInnerReturn);
                                })
                                .CheapReplace(strNeedleCommon + "Minimum}", () =>
                                {
                                    string strInnerReturn = objAttribute.Value.DisplayNameShort(strLanguage);
                                    if (blnShowValues)
                                    {
                                        if (dicValueOverrides == null
                                            || !dicValueOverrides.TryGetValue(
                                                strCharAttributeName + "Minimum", out int intAttributeValue))
                                            intAttributeValue = objAttribute.Value.TotalMinimum;
                                        strInnerReturn += strSpace + '(' + intAttributeValue.ToString(objCultureInfo)
                                                          + ')';
                                    }

                                    return string.Format(objCultureInfo,
                                        LanguageManager.GetString("String_MinimumAttribute", strLanguage, token: token),
                                        strInnerReturn);
                                })
                                .CheapReplace(strNeedleCommon + "Maximum}", () =>
                                {
                                    string strInnerReturn = objAttribute.Value.DisplayNameShort(strLanguage);
                                    if (blnShowValues)
                                    {
                                        if (dicValueOverrides == null
                                            || !dicValueOverrides.TryGetValue(
                                                strCharAttributeName + "Maximum", out int intAttributeValue))
                                            intAttributeValue = objAttribute.Value.TotalMaximum;
                                        strInnerReturn += strSpace + '(' + intAttributeValue.ToString(objCultureInfo)
                                                          + ')';
                                    }

                                    return string.Format(objCultureInfo,
                                        LanguageManager.GetString("String_MaximumAttribute", strLanguage, token: token),
                                        strInnerReturn);
                                });
                }
            }

            return strReturn;
        }

        public void ProcessAttributesInXPathForTooltip(StringBuilder sbdInput, string strOriginal = "", CultureInfo objCultureInfo = null, string strLanguage = "", bool blnShowValues = true, IReadOnlyDictionary<string, int> dicValueOverrides = null, CancellationToken token = default)
        {
            if (sbdInput == null || sbdInput.Length <= 0)
                return;
            if (string.IsNullOrEmpty(strOriginal))
                strOriginal = sbdInput.ToString();
            if (objCultureInfo == null)
                objCultureInfo = GlobalSettings.CultureInfo;
            if (string.IsNullOrEmpty(strLanguage))
                strLanguage = GlobalSettings.Language;
            string strSpace = LanguageManager.GetString("String_Space", strLanguage, token: token);
            using (LockObject.EnterReadLock(token))
            {
                foreach (string strCharAttributeName in AttributeStrings)
                {
                    Lazy<CharacterAttrib> objAttribute = new Lazy<CharacterAttrib>(() => _objCharacter.GetAttribute(strCharAttributeName, token: token));
                    string strNeedleCommon = '{' + strCharAttributeName;
                    sbdInput.CheapReplace(strOriginal, strNeedleCommon + '}', () =>
                    {
                        string strInnerReturn = objAttribute.Value.DisplayNameShort(strLanguage);
                        if (blnShowValues)
                        {
                            if (dicValueOverrides == null
                                || !dicValueOverrides.TryGetValue(strCharAttributeName, out int intAttributeValue))
                                intAttributeValue = objAttribute.Value.TotalValue;
                            strInnerReturn += strSpace + '(' + intAttributeValue.ToString(objCultureInfo) + ')';
                        }

                        return strInnerReturn;
                    });
                    sbdInput.CheapReplace(strOriginal, strNeedleCommon + "Unaug}", () =>
                    {
                        string strInnerReturn = objAttribute.Value.DisplayNameShort(strLanguage);
                        if (blnShowValues)
                        {
                            if (dicValueOverrides == null
                                || !dicValueOverrides.TryGetValue(strCharAttributeName + "Unaug",
                                                                  out int intAttributeValue))
                                intAttributeValue = objAttribute.Value.Value;
                            strInnerReturn += strSpace + '(' + intAttributeValue.ToString(objCultureInfo) + ')';
                        }

                        return string.Format(objCultureInfo,
                                             LanguageManager.GetString("String_NaturalAttribute", strLanguage, token: token),
                                             strInnerReturn);
                    });
                    sbdInput.CheapReplace(strOriginal, strNeedleCommon + "Base}", () =>
                    {
                        string strInnerReturn = objAttribute.Value.DisplayNameShort(strLanguage);
                        if (blnShowValues)
                        {
                            if (dicValueOverrides == null
                                || !dicValueOverrides.TryGetValue(strCharAttributeName + "Base",
                                                                  out int intAttributeValue))
                                intAttributeValue = objAttribute.Value.TotalBase;
                            strInnerReturn += strSpace + '(' + intAttributeValue.ToString(objCultureInfo) + ')';
                        }

                        return string.Format(objCultureInfo,
                                             LanguageManager.GetString("String_BaseAttribute", strLanguage, token: token),
                                             strInnerReturn);
                    });
                    sbdInput.CheapReplace(strOriginal, strNeedleCommon + "Minimum}", () =>
                    {
                        string strInnerReturn = objAttribute.Value.DisplayNameShort(strLanguage);
                        if (blnShowValues)
                        {
                            if (dicValueOverrides == null
                                || !dicValueOverrides.TryGetValue(strCharAttributeName + "Minimum",
                                    out int intAttributeValue))
                                intAttributeValue = objAttribute.Value.TotalMinimum;
                            strInnerReturn += strSpace + '(' + intAttributeValue.ToString(objCultureInfo) + ')';
                        }

                        return string.Format(objCultureInfo,
                            LanguageManager.GetString("String_MinimumAttribute", strLanguage, token: token),
                            strInnerReturn);
                    });
                    sbdInput.CheapReplace(strOriginal, strNeedleCommon + "Maximum}", () =>
                    {
                        string strInnerReturn = objAttribute.Value.DisplayNameShort(strLanguage);
                        if (blnShowValues)
                        {
                            if (dicValueOverrides == null
                                || !dicValueOverrides.TryGetValue(strCharAttributeName + "Maximum",
                                    out int intAttributeValue))
                                intAttributeValue = objAttribute.Value.TotalMaximum;
                            strInnerReturn += strSpace + '(' + intAttributeValue.ToString(objCultureInfo) + ')';
                        }

                        return string.Format(objCultureInfo,
                            LanguageManager.GetString("String_MaximumAttribute", strLanguage, token: token),
                            strInnerReturn);
                    });
                }
            }
        }

        public async Task<string> ProcessAttributesInXPathForTooltipAsync(string strInput, CultureInfo objCultureInfo = null, string strLanguage = "", bool blnShowValues = true, IAsyncReadOnlyDictionary<string, int> dicValueOverrides = null, CancellationToken token = default)
        {
            if (string.IsNullOrEmpty(strInput))
                return strInput;
            if (objCultureInfo == null)
                objCultureInfo = GlobalSettings.CultureInfo;
            if (string.IsNullOrEmpty(strLanguage))
                strLanguage = GlobalSettings.Language;
            string strSpace = await LanguageManager.GetStringAsync("String_Space", strLanguage, token: token).ConfigureAwait(false);
            string strReturn = strInput;
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                foreach (string strCharAttributeName in AttributeStrings)
                {
                    Microsoft.VisualStudio.Threading.AsyncLazy<CharacterAttrib> objAttribute =
                        new Microsoft.VisualStudio.Threading.AsyncLazy<CharacterAttrib>(
                            () => _objCharacter.GetAttributeAsync(strCharAttributeName, token: token), Utils.JoinableTaskFactory);
                    string strNeedleCommon = '{' + strCharAttributeName;
                    strReturn = await strReturn
                        .CheapReplaceAsync(strNeedleCommon + '}', async () =>
                        {
                            string strInnerReturn = await (await objAttribute.GetValueAsync(token).ConfigureAwait(false))
                                .DisplayNameShortAsync(strLanguage, token).ConfigureAwait(false);
                            if (blnShowValues)
                            {
                                if (dicValueOverrides == null
                                    || !dicValueOverrides.TryGetValue(
                                        strCharAttributeName, out int intAttributeValue))
                                    intAttributeValue = await (await objAttribute.GetValueAsync(token)
                                            .ConfigureAwait(false))
                                        .GetTotalValueAsync(token).ConfigureAwait(false);
                                strInnerReturn
                                    += strSpace + '(' + intAttributeValue.ToString(objCultureInfo)
                                       + ')';
                            }

                            return strInnerReturn;
                        }, token: token)
                        .CheapReplaceAsync(strNeedleCommon + "Unaug}", async () =>
                        {
                            string strInnerReturn = await (await objAttribute.GetValueAsync(token).ConfigureAwait(false))
                                .DisplayNameShortAsync(strLanguage, token).ConfigureAwait(false);
                            if (blnShowValues)
                            {
                                if (dicValueOverrides == null
                                    || !dicValueOverrides.TryGetValue(
                                        strCharAttributeName + "Unaug", out int intAttributeValue))
                                    intAttributeValue = await (await objAttribute.GetValueAsync(token)
                                            .ConfigureAwait(false))
                                        .GetValueAsync(token).ConfigureAwait(false);
                                strInnerReturn
                                    += strSpace + '(' + intAttributeValue.ToString(objCultureInfo)
                                       + ')';
                            }

                            return string.Format(objCultureInfo,
                                await LanguageManager.GetStringAsync(
                                    "String_NaturalAttribute", strLanguage, token: token).ConfigureAwait(false),
                                strInnerReturn);
                        }, token: token)
                        .CheapReplaceAsync(strNeedleCommon + "Base}", async () =>
                        {
                            string strInnerReturn = await (await objAttribute.GetValueAsync(token).ConfigureAwait(false))
                                .DisplayNameShortAsync(strLanguage, token).ConfigureAwait(false);
                            if (blnShowValues)
                            {
                                if (dicValueOverrides == null
                                    || !dicValueOverrides.TryGetValue(
                                        strCharAttributeName + "Base", out int intAttributeValue))
                                    intAttributeValue = await (await objAttribute.GetValueAsync(token)
                                            .ConfigureAwait(false))
                                        .GetTotalBaseAsync(token).ConfigureAwait(false);
                                strInnerReturn
                                    += strSpace + '(' + intAttributeValue.ToString(objCultureInfo)
                                       + ')';
                            }

                            return string.Format(objCultureInfo,
                                await LanguageManager.GetStringAsync(
                                    "String_BaseAttribute", strLanguage, token: token).ConfigureAwait(false),
                                strInnerReturn);
                        }, token: token)
                        .CheapReplaceAsync(strNeedleCommon + "Minimum}", async () =>
                        {
                            string strInnerReturn = await (await objAttribute.GetValueAsync(token).ConfigureAwait(false))
                                .DisplayNameShortAsync(strLanguage, token).ConfigureAwait(false);
                            if (blnShowValues)
                            {
                                if (dicValueOverrides == null
                                    || !dicValueOverrides.TryGetValue(
                                        strCharAttributeName + "Minimum", out int intAttributeValue))
                                    intAttributeValue = await (await objAttribute.GetValueAsync(token)
                                            .ConfigureAwait(false))
                                        .GetTotalMinimumAsync(token).ConfigureAwait(false);
                                strInnerReturn
                                    += strSpace + '(' + intAttributeValue.ToString(objCultureInfo)
                                       + ')';
                            }

                            return string.Format(objCultureInfo,
                                await LanguageManager.GetStringAsync(
                                    "String_MinimumAttribute", strLanguage, token: token).ConfigureAwait(false),
                                strInnerReturn);
                        }, token: token)
                        .CheapReplaceAsync(strNeedleCommon + "Maximum}", async () =>
                        {
                            string strInnerReturn = await (await objAttribute.GetValueAsync(token).ConfigureAwait(false))
                                .DisplayNameShortAsync(strLanguage, token).ConfigureAwait(false);
                            if (blnShowValues)
                            {
                                if (dicValueOverrides == null
                                    || !dicValueOverrides.TryGetValue(
                                        strCharAttributeName + "Maximum", out int intAttributeValue))
                                    intAttributeValue = await (await objAttribute.GetValueAsync(token)
                                            .ConfigureAwait(false))
                                        .GetTotalMaximumAsync(token).ConfigureAwait(false);
                                strInnerReturn
                                    += strSpace + '(' + intAttributeValue.ToString(objCultureInfo)
                                       + ')';
                            }

                            return string.Format(objCultureInfo,
                                await LanguageManager.GetStringAsync(
                                    "String_MaximumAttribute", strLanguage, token: token).ConfigureAwait(false),
                                strInnerReturn);
                        }, token: token).ConfigureAwait(false);
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }

            return strReturn;
        }

        public async Task ProcessAttributesInXPathForTooltipAsync(StringBuilder sbdInput, string strOriginal = "",
            CultureInfo objCultureInfo = null, string strLanguage = "", bool blnShowValues = true,
            IAsyncReadOnlyDictionary<string, int> dicValueOverrides = null, CancellationToken token = default)
        {
            if (sbdInput == null || sbdInput.Length <= 0)
                return;
            if (string.IsNullOrEmpty(strOriginal))
                strOriginal = sbdInput.ToString();
            if (objCultureInfo == null)
                objCultureInfo = GlobalSettings.CultureInfo;
            if (string.IsNullOrEmpty(strLanguage))
                strLanguage = GlobalSettings.Language;
            string strSpace = await LanguageManager.GetStringAsync("String_Space", strLanguage, token: token)
                .ConfigureAwait(false);
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                foreach (string strCharAttributeName in AttributeStrings)
                {
                    Microsoft.VisualStudio.Threading.AsyncLazy<CharacterAttrib> objAttribute =
                        new Microsoft.VisualStudio.Threading.AsyncLazy<CharacterAttrib>(
                            () => _objCharacter.GetAttributeAsync(strCharAttributeName, token: token), Utils.JoinableTaskFactory);
                    string strNeedleCommon = '{' + strCharAttributeName;
                    await sbdInput.CheapReplaceAsync(strOriginal, strNeedleCommon + '}', async () =>
                    {
                        string strInnerReturn = await (await objAttribute.GetValueAsync(token).ConfigureAwait(false))
                            .DisplayNameShortAsync(strLanguage, token).ConfigureAwait(false);
                        if (blnShowValues)
                        {
                            if (dicValueOverrides == null
                                || !dicValueOverrides.TryGetValue(strCharAttributeName, out int intAttributeValue))
                                intAttributeValue =
                                    await (await objAttribute.GetValueAsync(token)
                                        .ConfigureAwait(false)).GetTotalValueAsync(token).ConfigureAwait(false);
                            strInnerReturn += strSpace + '(' + intAttributeValue.ToString(objCultureInfo) + ')';
                        }

                        return strInnerReturn;
                    }, token: token).ConfigureAwait(false);
                    await sbdInput.CheapReplaceAsync(strOriginal, strNeedleCommon + "Unaug}", async () =>
                    {
                        string strInnerReturn = await (await objAttribute.GetValueAsync(token).ConfigureAwait(false))
                            .DisplayNameShortAsync(strLanguage, token).ConfigureAwait(false);
                        if (blnShowValues)
                        {
                            if (dicValueOverrides == null
                                || !dicValueOverrides.TryGetValue(strCharAttributeName + "Unaug",
                                    out int intAttributeValue))
                                intAttributeValue =
                                    await (await objAttribute.GetValueAsync(token)
                                        .ConfigureAwait(false)).GetValueAsync(token).ConfigureAwait(false);
                            strInnerReturn += strSpace + '(' + intAttributeValue.ToString(objCultureInfo) + ')';
                        }

                        return string.Format(objCultureInfo,
                            await LanguageManager.GetStringAsync("String_NaturalAttribute", strLanguage, token: token)
                                .ConfigureAwait(false),
                            strInnerReturn);
                    }, token: token).ConfigureAwait(false);
                    await sbdInput.CheapReplaceAsync(strOriginal, strNeedleCommon + "Base}", async () =>
                    {
                        string strInnerReturn = await (await objAttribute.GetValueAsync(token).ConfigureAwait(false))
                            .DisplayNameShortAsync(strLanguage, token).ConfigureAwait(false);
                        if (blnShowValues)
                        {
                            if (dicValueOverrides == null
                                || !dicValueOverrides.TryGetValue(strCharAttributeName + "Base",
                                    out int intAttributeValue))
                                intAttributeValue =
                                    await (await objAttribute.GetValueAsync(token)
                                        .ConfigureAwait(false)).GetTotalBaseAsync(token).ConfigureAwait(false);
                            strInnerReturn += strSpace + '(' + intAttributeValue.ToString(objCultureInfo) + ')';
                        }

                        return string.Format(objCultureInfo,
                            await LanguageManager.GetStringAsync("String_BaseAttribute", strLanguage, token: token)
                                .ConfigureAwait(false),
                            strInnerReturn);
                    }, token: token).ConfigureAwait(false);
                    await sbdInput.CheapReplaceAsync(strOriginal, strNeedleCommon + "Minimum}", async () =>
                    {
                        string strInnerReturn = await (await objAttribute.GetValueAsync(token).ConfigureAwait(false))
                            .DisplayNameShortAsync(strLanguage, token).ConfigureAwait(false);
                        if (blnShowValues)
                        {
                            if (dicValueOverrides == null
                                || !dicValueOverrides.TryGetValue(strCharAttributeName + "Minimum",
                                    out int intAttributeValue))
                                intAttributeValue =
                                    await (await objAttribute.GetValueAsync(token)
                                        .ConfigureAwait(false)).GetTotalMinimumAsync(token).ConfigureAwait(false);
                            strInnerReturn += strSpace + '(' + intAttributeValue.ToString(objCultureInfo) + ')';
                        }

                        return string.Format(objCultureInfo,
                            await LanguageManager.GetStringAsync("String_MinimumAttribute", strLanguage, token: token)
                                .ConfigureAwait(false),
                            strInnerReturn);
                    }, token: token).ConfigureAwait(false);
                    await sbdInput.CheapReplaceAsync(strOriginal, strNeedleCommon + "Maximum}", async () =>
                    {
                        string strInnerReturn = await (await objAttribute.GetValueAsync(token).ConfigureAwait(false))
                            .DisplayNameShortAsync(strLanguage, token).ConfigureAwait(false);
                        if (blnShowValues)
                        {
                            if (dicValueOverrides == null
                                || !dicValueOverrides.TryGetValue(strCharAttributeName + "Maximum",
                                    out int intAttributeValue))
                                intAttributeValue =
                                    await (await objAttribute.GetValueAsync(token)
                                        .ConfigureAwait(false)).GetTotalMaximumAsync(token).ConfigureAwait(false);
                            strInnerReturn += strSpace + '(' + intAttributeValue.ToString(objCultureInfo) + ')';
                        }

                        return string.Format(objCultureInfo,
                            await LanguageManager.GetStringAsync("String_MaximumAttribute", strLanguage, token: token)
                                .ConfigureAwait(false),
                            strInnerReturn);
                    }, token: token).ConfigureAwait(false);
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        internal void Reset(bool blnFirstTime = false, CancellationToken token = default)
        {
            using (LockObject.EnterWriteLock(token))
            {
                Interlocked.Increment(ref _intLoading);
                try
                {
                    AttributeList.Clear();
                    SpecialAttributeList.Clear();
                    foreach (string strAttribute in AttributeStrings)
                    {
                        token.ThrowIfCancellationRequested();
                        CharacterAttrib objAttribute;
                        switch (CharacterAttrib.ConvertToAttributeCategory(strAttribute))
                        {
                            case CharacterAttrib.AttributeCategory.Special:
                                objAttribute = new CharacterAttrib(_objCharacter, strAttribute,
                                                                   CharacterAttrib.AttributeCategory.Special);
                                SpecialAttributeList.Add(objAttribute);
                                break;

                            case CharacterAttrib.AttributeCategory.Standard:
                                objAttribute = new CharacterAttrib(_objCharacter, strAttribute,
                                                                   CharacterAttrib.AttributeCategory.Standard);
                                AttributeList.Add(objAttribute);
                                break;
                            default:
                                continue;
                        }
                    }

                    if (blnFirstTime)
                        InitializeAttributesList(token);
                    ResetBindings(token);
                }
                finally
                {
                    if (blnFirstTime)
                        Interlocked.Add(ref _intLoading, -2);
                    else
                        Interlocked.Decrement(ref _intLoading);
                }
            }
        }

        internal async Task ResetAsync(bool blnFirstTime = false, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                Interlocked.Increment(ref _intLoading);
                try
                {
                    ThreadSafeObservableCollection<CharacterAttrib> lstAttributes = await GetAttributeListAsync(token).ConfigureAwait(false);
                    ThreadSafeObservableCollection<CharacterAttrib> lstSpecialAttributes = await GetSpecialAttributeListAsync(token).ConfigureAwait(false);
                    await lstAttributes.ClearAsync(token).ConfigureAwait(false);
                    await lstSpecialAttributes.ClearAsync(token).ConfigureAwait(false);
                    foreach (string strAttribute in AttributeStrings)
                    {
                        CharacterAttrib objAttribute;
                        switch (CharacterAttrib.ConvertToAttributeCategory(strAttribute))
                        {
                            case CharacterAttrib.AttributeCategory.Special:
                                objAttribute = new CharacterAttrib(_objCharacter, strAttribute,
                                                                   CharacterAttrib.AttributeCategory.Special);
                                await lstSpecialAttributes.AddAsync(objAttribute, token).ConfigureAwait(false);
                                break;

                            case CharacterAttrib.AttributeCategory.Standard:
                                objAttribute = new CharacterAttrib(_objCharacter, strAttribute,
                                                                   CharacterAttrib.AttributeCategory.Standard);
                                await lstAttributes.AddAsync(objAttribute, token).ConfigureAwait(false);
                                break;
                            default:
                                continue;
                        }
                    }

                    if (blnFirstTime)
                        await InitializeAttributesListAsync(token).ConfigureAwait(false);
                    await ResetBindingsAsync(token).ConfigureAwait(false);
                }
                finally
                {
                    if (blnFirstTime)
                        Interlocked.Add(ref _intLoading, -2);
                    else
                        Interlocked.Decrement(ref _intLoading);
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public static CharacterAttrib.AttributeCategory ConvertAttributeCategory(string s)
        {
            switch (s)
            {
                case "Shapeshifter":
                    return CharacterAttrib.AttributeCategory.Shapeshifter;

                case "Special":
                    return CharacterAttrib.AttributeCategory.Special;

                case "Metahuman":
                case "Standard":
                    return CharacterAttrib.AttributeCategory.Standard;
            }
            return CharacterAttrib.AttributeCategory.Standard;
        }

        /// <summary>
        /// Reset the databindings for all character attributes.
        /// This method is used to support hot-swapping attributes for shapeshifters.
        /// </summary>
        public void ResetBindings(CancellationToken token = default)
        {
            using (LockObject.EnterWriteLock(token))
            {
                token.ThrowIfCancellationRequested();
                _objCharacter.RefreshAttributeBindings(token);
                foreach (CharacterAttrib objAttribute in AttributeList)
                {
                    token.ThrowIfCancellationRequested();
                    objAttribute.MultiplePropertiesChangedAsync -= RunExtraAsyncPropertyChanged(objAttribute.Abbrev);
                    if (objAttribute == GetAttributeByName(objAttribute.Abbrev, token))
                    {
                        objAttribute.MultiplePropertiesChangedAsync += RunExtraAsyncPropertyChanged(objAttribute.Abbrev);
                    }
                }
                foreach (CharacterAttrib objAttribute in SpecialAttributeList)
                {
                    token.ThrowIfCancellationRequested();
                    objAttribute.MultiplePropertiesChangedAsync -= RunExtraAsyncPropertyChanged(objAttribute.Abbrev);
                    if (objAttribute == GetAttributeByName(objAttribute.Abbrev, token))
                    {
                        objAttribute.MultiplePropertiesChangedAsync += RunExtraAsyncPropertyChanged(objAttribute.Abbrev);
                    }
                }
                List<string> lstProperties = typeof(CharacterAttrib).GetProperties().Where(x => x.CanRead).Select(x => x.Name).ToList();
                foreach (string strAbbrev in AttributeStrings)
                {
                    token.ThrowIfCancellationRequested();
                    if (_dicUIPropertyChangers.TryGetValue(strAbbrev, out UiPropertyChangerTracker objEvents))
                    {
                        token.ThrowIfCancellationRequested();
                        if (objEvents.AsyncPropertyChangedList.Count != 0)
                        {
                            MultiplePropertiesChangedEventArgs e = new MultiplePropertiesChangedEventArgs(lstProperties);
                            Func<Task>[] aFuncs = new Func<Task>[objEvents.AsyncPropertyChangedList.Count];
                            for (int i = 0; i < objEvents.AsyncPropertyChangedList.Count; ++i)
                            {
                                int i1 = i;
                                aFuncs[i] = () => objEvents.AsyncPropertyChangedList[i1].Invoke(this, e, token);
                            }

                            Utils.RunWithoutThreadLock(aFuncs, token);
                            if (objEvents.PropertyChangedList.Count != 0)
                            {
                                Utils.RunOnMainThread(() =>
                                {
                                    foreach (MultiplePropertiesChangedEventHandler funcToRun in objEvents.PropertyChangedList)
                                    {
                                        funcToRun?.Invoke(this, e);
                                    }
                                }, token: token);
                            }
                        }
                        else if (objEvents.PropertyChangedList.Count != 0)
                        {
                            MultiplePropertiesChangedEventArgs e = new MultiplePropertiesChangedEventArgs(lstProperties);
                            Utils.RunOnMainThread(() =>
                            {
                                foreach (MultiplePropertiesChangedEventHandler funcToRun in objEvents.PropertyChangedList)
                                {
                                    funcToRun?.Invoke(this, e);
                                }
                            }, token: token);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Reset the databindings for all character attributes.
        /// This method is used to support hot-swapping attributes for shapeshifters.
        /// </summary>
        public async Task ResetBindingsAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                await _objCharacter.RefreshAttributeBindingsAsync(token).ConfigureAwait(false);
                await (await GetAttributeListAsync(token).ConfigureAwait(false)).ForEachWithSideEffectsAsync(async objAttribute =>
                {
                    objAttribute.MultiplePropertiesChangedAsync -= RunExtraAsyncPropertyChanged(objAttribute.Abbrev);
                    if (objAttribute == await GetAttributeByNameAsync(objAttribute.Abbrev, token).ConfigureAwait(false))
                    {
                        objAttribute.MultiplePropertiesChangedAsync +=
                            RunExtraAsyncPropertyChanged(objAttribute.Abbrev);
                    }
                }, token).ConfigureAwait(false);
                await (await GetSpecialAttributeListAsync(token).ConfigureAwait(false)).ForEachWithSideEffectsAsync(async objAttribute =>
                {
                    objAttribute.MultiplePropertiesChangedAsync -= RunExtraAsyncPropertyChanged(objAttribute.Abbrev);
                    if (objAttribute == await GetAttributeByNameAsync(objAttribute.Abbrev, token).ConfigureAwait(false))
                    {
                        objAttribute.MultiplePropertiesChangedAsync +=
                            RunExtraAsyncPropertyChanged(objAttribute.Abbrev);
                    }
                }, token).ConfigureAwait(false);
                List<string> lstProperties = typeof(CharacterAttrib).GetProperties().Where(x => x.CanRead).Select(x => x.Name).ToList();
                foreach (string strAbbrev in AttributeStrings)
                {
                    token.ThrowIfCancellationRequested();
                    if (_dicUIPropertyChangers.TryGetValue(strAbbrev, out UiPropertyChangerTracker objEvents))
                    {
                        token.ThrowIfCancellationRequested();
                        if (objEvents.AsyncPropertyChangedList.Count != 0)
                        {
                            MultiplePropertiesChangedEventArgs
                                e = new MultiplePropertiesChangedEventArgs(lstProperties);
                            List<Task> lstTasks = new List<Task>(Math.Min(objEvents.AsyncPropertyChangedList.Count,
                                Utils.MaxParallelBatchSize));
                            int i = 0;
                            foreach (MultiplePropertiesChangedAsyncEventHandler objEvent in objEvents
                                         .AsyncPropertyChangedList)
                            {
                                lstTasks.Add(objEvent.Invoke(this, e, token));
                                if (++i < Utils.MaxParallelBatchSize)
                                    continue;
                                await Task.WhenAll(lstTasks).ConfigureAwait(false);
                                lstTasks.Clear();
                                i = 0;
                            }

                            await Task.WhenAll(lstTasks).ConfigureAwait(false);

                            if (objEvents.PropertyChangedList.Count != 0)
                            {
                                await Utils.RunOnMainThreadAsync(() =>
                                {
                                    foreach (MultiplePropertiesChangedEventHandler funcToRun in objEvents
                                                 .PropertyChangedList)
                                    {
                                        funcToRun?.Invoke(this, e);
                                    }
                                }, token).ConfigureAwait(false);
                            }
                        }
                        else if (objEvents.PropertyChangedList.Count != 0)
                        {
                            MultiplePropertiesChangedEventArgs
                                e = new MultiplePropertiesChangedEventArgs(lstProperties);
                            await Utils.RunOnMainThreadAsync(() =>
                            {
                                foreach (MultiplePropertiesChangedEventHandler funcToRun in objEvents
                                             .PropertyChangedList)
                                {
                                    funcToRun?.Invoke(this, e);
                                }
                            }, token).ConfigureAwait(false);
                        }
                    }
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Can an attribute be increased to its metatype maximum?
        /// Note: if the attribute is already at its metatype maximum, this method assumes that we just raised it and will decrease it if it's illegal.
        /// </summary>
        /// <param name="objAttribute">Attribute to raise to its metatype maximum.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        /// <returns></returns>
        public async Task<bool> CanRaiseAttributeToMetatypeMax(CharacterAttrib objAttribute,
                                                                    CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (await _objCharacter.GetCreatedAsync(token).ConfigureAwait(false)
                    || await _objCharacter.GetIgnoreRulesAsync(token).ConfigureAwait(false))
                    return true;
                CharacterAttrib.AttributeCategory eCategory = objAttribute.MetatypeCategory;
                if (eCategory == CharacterAttrib.AttributeCategory.Special
                    || _objCharacter.Settings.MaxNumberMaxAttributesCreate
                    >= await AttributeList.GetCountAsync(token).ConfigureAwait(false))
                    return true;
                return await AttributeList.CountAsync(async x => x.MetatypeCategory == eCategory
                                                                 && x != objAttribute
                                                                 && await x.GetAtMetatypeMaximumAsync(token)
                                                                           .ConfigureAwait(false), token)
                                          .ConfigureAwait(false)
                       < _objCharacter.Settings.MaxNumberMaxAttributesCreate;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        #endregion Methods

        #region Properties

        /// <summary>
        /// Character's Attributes.
        /// </summary>
        [HubTag(true)]
        public ThreadSafeObservableCollection<CharacterAttrib> AttributeList
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _lstNormalAttributes;
            }
        }

        public async Task<ThreadSafeObservableCollection<CharacterAttrib>> GetAttributeListAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _lstNormalAttributes;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Character's Attributes.
        /// </summary>
        public ThreadSafeObservableCollection<CharacterAttrib> SpecialAttributeList
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _lstSpecialAttributes;
            }
        }

        public async Task<ThreadSafeObservableCollection<CharacterAttrib>> GetSpecialAttributeListAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _lstSpecialAttributes;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public IReadOnlyCollection<CharacterAttrib> AllAttributes
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _dicAttributes.Values as IReadOnlyCollection<CharacterAttrib>;
            }
        }

        public async Task<IReadOnlyCollection<CharacterAttrib>> GetAllAttributesAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _dicAttributes.Values as IReadOnlyCollection<CharacterAttrib>;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public CharacterAttrib.AttributeCategory AttributeCategory
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _eAttributeCategory;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    // No need to write lock because interlocked guarantees safety
                    if (InterlockedExtensions.Exchange(ref _eAttributeCategory, value) == value)
                        return;
                    OnPropertyChanged();
                }
            }
        }

        public async Task<CharacterAttrib.AttributeCategory> GetAttributeCategoryAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _eAttributeCategory;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public async Task SetAttributeCategoryAsync(CharacterAttrib.AttributeCategory value, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                // No need to write lock because interlocked guarantees safety
                if (InterlockedExtensions.Exchange(ref _eAttributeCategory, value) == value)
                    return;
                await OnPropertyChangedAsync(nameof(AttributeCategory), token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        #endregion Properties
    }
}
