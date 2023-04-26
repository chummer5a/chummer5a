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
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;
using Chummer.Annotations;

namespace Chummer.Backend.Attributes
{
    public sealed class AttributeSection : INotifyMultiplePropertyChanged, IHasLockObject
    {
        private int _intLoading = 1;

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        public void OnPropertyChanged([CallerMemberName] string strPropertyName = null)
        {
            this.OnMultiplePropertyChanged(strPropertyName);
        }

        public void OnMultiplePropertyChanged(IReadOnlyCollection<string> lstPropertyNames)
        {
            using (EnterReadLock.Enter(LockObject))
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

                    if (PropertyChanged != null)
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

        private static readonly PropertyDependencyGraph<AttributeSection> s_AttributeSectionDependencyGraph =
            new PropertyDependencyGraph<AttributeSection>(
            );

        private bool _blnAttributesInitialized;
        private readonly AsyncFriendlyReaderWriterLock _objAttributesInitializerLock = new AsyncFriendlyReaderWriterLock();
        private readonly ThreadSafeObservableCollection<CharacterAttrib> _lstAttributes = new ThreadSafeObservableCollection<CharacterAttrib>();

        public ThreadSafeObservableCollection<CharacterAttrib> Attributes
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                {
                    using (EnterReadLock.Enter(_objAttributesInitializerLock))
                    {
                        if (!_blnAttributesInitialized)
                        {
                            InitializeAttributesList();
                        }
                    }
                    return _lstAttributes;
                }
            }
        }

        public async ValueTask<ThreadSafeObservableCollection<CharacterAttrib>> GetAttributesAsync(CancellationToken token = default)
        {
            using (await EnterReadLock.EnterAsync(LockObject, token).ConfigureAwait(false))
            {
                using (await EnterReadLock.EnterAsync(_objAttributesInitializerLock, token).ConfigureAwait(false))
                {
                    if (!_blnAttributesInitialized)
                    {
                        await InitializeAttributesListAsync(token).ConfigureAwait(false);
                    }
                }

                return _lstAttributes;
            }
        }

        private void InitializeAttributesList()
        {
            using (EnterReadLock.Enter(_objCharacter.LockObject))
            {
                using (_objAttributesInitializerLock.EnterWriteLock())
                {
                    _blnAttributesInitialized = true;

                    // Not creating a new collection here so that CollectionChanged events from previous list are kept
                    _lstAttributes.Clear();
                    _lstAttributes.Add(_objCharacter.BOD);
                    _lstAttributes.Add(_objCharacter.AGI);
                    _lstAttributes.Add(_objCharacter.REA);
                    _lstAttributes.Add(_objCharacter.STR);
                    _lstAttributes.Add(_objCharacter.CHA);
                    _lstAttributes.Add(_objCharacter.INT);
                    _lstAttributes.Add(_objCharacter.LOG);
                    _lstAttributes.Add(_objCharacter.WIL);
                    _lstAttributes.Add(_objCharacter.EDG);

                    if (_objCharacter.MAGEnabled)
                    {
                        _lstAttributes.Add(_objCharacter.MAG);
                        if (_objCharacter.Settings.MysAdeptSecondMAGAttribute && _objCharacter.IsMysticAdept)
                            _lstAttributes.Add(_objCharacter.MAGAdept);
                    }

                    if (_objCharacter.RESEnabled)
                    {
                        _lstAttributes.Add(_objCharacter.RES);
                    }

                    if (_objCharacter.DEPEnabled)
                    {
                        _lstAttributes.Add(_objCharacter.DEP);
                    }
                }
            }
        }

        private async ValueTask InitializeAttributesListAsync(CancellationToken token = default)
        {
            using (await EnterReadLock.EnterAsync(_objCharacter.LockObject, token).ConfigureAwait(false))
            {
                IAsyncDisposable objLocker = await _objAttributesInitializerLock.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    _blnAttributesInitialized = true;

                    // Not creating a new collection here so that CollectionChanged events from previous list are kept
                    await _lstAttributes.ClearAsync(token).ConfigureAwait(false);
                    await _lstAttributes.AddAsync(await GetAttributeByNameAsync("BOD", token).ConfigureAwait(false), token).ConfigureAwait(false);
                    await _lstAttributes.AddAsync(await GetAttributeByNameAsync("AGI", token).ConfigureAwait(false), token).ConfigureAwait(false);
                    await _lstAttributes.AddAsync(await GetAttributeByNameAsync("REA", token).ConfigureAwait(false), token).ConfigureAwait(false);
                    await _lstAttributes.AddAsync(await GetAttributeByNameAsync("STR", token).ConfigureAwait(false), token).ConfigureAwait(false);
                    await _lstAttributes.AddAsync(await GetAttributeByNameAsync("CHA", token).ConfigureAwait(false), token).ConfigureAwait(false);
                    await _lstAttributes.AddAsync(await GetAttributeByNameAsync("INT", token).ConfigureAwait(false), token).ConfigureAwait(false);
                    await _lstAttributes.AddAsync(await GetAttributeByNameAsync("LOG", token).ConfigureAwait(false), token).ConfigureAwait(false);
                    await _lstAttributes.AddAsync(await GetAttributeByNameAsync("WIL", token).ConfigureAwait(false), token).ConfigureAwait(false);
                    await _lstAttributes.AddAsync(await GetAttributeByNameAsync("EDG", token).ConfigureAwait(false), token).ConfigureAwait(false);

                    if (await _objCharacter.GetMAGEnabledAsync(token).ConfigureAwait(false))
                    {
                        await _lstAttributes.AddAsync(await GetAttributeByNameAsync("MAG", token).ConfigureAwait(false), token).ConfigureAwait(false);
                        if (await (await _objCharacter.GetSettingsAsync(token).ConfigureAwait(false)).GetMysAdeptSecondMAGAttributeAsync(token).ConfigureAwait(false) &&
                            await _objCharacter.GetIsMysticAdeptAsync(token).ConfigureAwait(false))
                            await _lstAttributes.AddAsync(await GetAttributeByNameAsync("MAGAdept", token).ConfigureAwait(false), token).ConfigureAwait(false);
                    }

                    if (await _objCharacter.GetRESEnabledAsync(token).ConfigureAwait(false))
                    {
                        await _lstAttributes.AddAsync(await GetAttributeByNameAsync("RES", token).ConfigureAwait(false), token).ConfigureAwait(false);
                    }

                    if (await _objCharacter.GetDEPEnabledAsync(token).ConfigureAwait(false))
                    {
                        await _lstAttributes.AddAsync(await GetAttributeByNameAsync("DEP", token).ConfigureAwait(false), token).ConfigureAwait(false);
                    }
                }
                finally
                {
                    await objLocker.DisposeAsync().ConfigureAwait(false);
                }
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

        private readonly LockingDictionary<string, BindingSource> _dicBindings = new LockingDictionary<string, BindingSource>(AttributeStrings.Count);
        private readonly Character _objCharacter;
        private CharacterAttrib.AttributeCategory _eAttributeCategory = CharacterAttrib.AttributeCategory.Standard;
        private readonly ThreadSafeObservableCollection<CharacterAttrib> _lstNormalAttributes = new ThreadSafeObservableCollection<CharacterAttrib>();
        private readonly ThreadSafeObservableCollection<CharacterAttrib> _lstSpecialAttributes = new ThreadSafeObservableCollection<CharacterAttrib>();

        #region Constructor, Save, Load, Print Methods

        public AttributeSection(Character character)
        {
            _objCharacter = character;
            AttributeList.CollectionChanged += AttributeListOnCollectionChanged;
            AttributeList.BeforeClearCollectionChanged += AttributeListOnBeforeClearCollectionChanged;
            SpecialAttributeList.CollectionChanged += SpecialAttributeListOnCollectionChanged;
            SpecialAttributeList.BeforeClearCollectionChanged += SpecialAttributeListOnBeforeClearCollectionChanged;
        }

        private void SpecialAttributeListOnBeforeClearCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            using (EnterReadLock.Enter(LockObject))
            {
                if (_intLoading > 0)
                    return;
                foreach (CharacterAttrib objAttribute in e.OldItems)
                {
                    objAttribute.Dispose();
                    if (_dicBindings.TryGetValue(objAttribute.Abbrev, out BindingSource objBindingSource))
                        objBindingSource.DataSource = GetAttributeByName(objAttribute.Abbrev);
                }
            }
        }

        private void AttributeListOnBeforeClearCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            using (EnterReadLock.Enter(LockObject))
            {
                if (_intLoading > 0)
                    return;
                foreach (CharacterAttrib objAttribute in e.OldItems)
                {
                    objAttribute.Dispose();
                    if (_dicBindings.TryGetValue(objAttribute.Abbrev, out BindingSource objBindingSource))
                        objBindingSource.DataSource = GetAttributeByName(objAttribute.Abbrev);
                }
            }
        }

        private void AttributeListOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            using (EnterReadLock.Enter(LockObject))
            {
                if (_intLoading > 0)
                    return;
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        foreach (CharacterAttrib objAttribute in e.NewItems)
                        {
                            if (_dicBindings.TryGetValue(objAttribute.Abbrev, out BindingSource objBindingSource))
                                objBindingSource.DataSource = GetAttributeByName(objAttribute.Abbrev);
                        }

                        break;

                    case NotifyCollectionChangedAction.Remove:
                        foreach (CharacterAttrib objAttribute in e.OldItems)
                        {
                            objAttribute.Dispose();
                            if (_dicBindings.TryGetValue(objAttribute.Abbrev, out BindingSource objBindingSource))
                                objBindingSource.DataSource = GetAttributeByName(objAttribute.Abbrev);
                        }

                        break;

                    case NotifyCollectionChangedAction.Replace:
                        HashSet<CharacterAttrib> setNewAttribs = e.NewItems.OfType<CharacterAttrib>().ToHashSet();
                        foreach (CharacterAttrib objAttribute in e.OldItems)
                        {
                            if (setNewAttribs.Contains(objAttribute))
                                continue;
                            objAttribute.Dispose();
                            if (_dicBindings.TryGetValue(objAttribute.Abbrev, out BindingSource objBindingSource))
                                objBindingSource.DataSource = GetAttributeByName(objAttribute.Abbrev);
                        }

                        foreach (CharacterAttrib objAttribute in setNewAttribs)
                        {
                            if (_dicBindings.TryGetValue(objAttribute.Abbrev, out BindingSource objBindingSource))
                                objBindingSource.DataSource = GetAttributeByName(objAttribute.Abbrev);
                        }

                        break;
                }
            }
        }

        private void SpecialAttributeListOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            using (EnterReadLock.Enter(LockObject))
            {
                if (_intLoading > 0)
                    return;
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        foreach (CharacterAttrib objAttribute in e.NewItems)
                        {
                            if (_dicBindings.TryGetValue(objAttribute.Abbrev, out BindingSource objBindingSource))
                                objBindingSource.DataSource = GetAttributeByName(objAttribute.Abbrev);
                        }

                        break;

                    case NotifyCollectionChangedAction.Remove:
                        foreach (CharacterAttrib objAttribute in e.OldItems)
                        {
                            objAttribute.Dispose();
                            if (_dicBindings.TryGetValue(objAttribute.Abbrev, out BindingSource objBindingSource))
                                objBindingSource.DataSource = GetAttributeByName(objAttribute.Abbrev);
                        }

                        break;

                    case NotifyCollectionChangedAction.Replace:
                        HashSet<CharacterAttrib> setNewAttribs = e.NewItems.OfType<CharacterAttrib>().ToHashSet();
                        foreach (CharacterAttrib objAttribute in e.OldItems)
                        {
                            if (setNewAttribs.Contains(objAttribute))
                                continue;
                            objAttribute.Dispose();
                            if (_dicBindings.TryGetValue(objAttribute.Abbrev, out BindingSource objBindingSource))
                                objBindingSource.DataSource = GetAttributeByName(objAttribute.Abbrev);
                        }

                        foreach (CharacterAttrib objAttribute in setNewAttribs)
                        {
                            if (_dicBindings.TryGetValue(objAttribute.Abbrev, out BindingSource objBindingSource))
                                objBindingSource.DataSource = GetAttributeByName(objAttribute.Abbrev);
                        }

                        break;
                }
            }
        }

        public void Dispose()
        {
            using (LockObject.EnterWriteLock())
            {
                foreach (BindingSource objSource in _dicBindings.Values)
                    objSource.Dispose();
                _dicBindings.Dispose();
                _lstNormalAttributes.ForEach(x => x.Dispose());
                _lstNormalAttributes.Dispose();
                _lstSpecialAttributes.ForEach(x => x.Dispose());
                _lstSpecialAttributes.Dispose();
                _lstAttributes.Dispose();
                _objAttributesInitializerLock.Dispose();
            }
            LockObject.Dispose();
        }

        public async ValueTask DisposeAsync()
        {
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync().ConfigureAwait(false);
            try
            {
                foreach (BindingSource objSource in await _dicBindings.GetValuesAsync().ConfigureAwait(false))
                    objSource.Dispose();
                await _dicBindings.DisposeAsync().ConfigureAwait(false);
                await _lstNormalAttributes.ForEachAsync(x => x.DisposeAsync().AsTask()).ConfigureAwait(false);
                await _lstNormalAttributes.DisposeAsync().ConfigureAwait(false);
                await _lstSpecialAttributes.ForEachAsync(x => x.DisposeAsync().AsTask()).ConfigureAwait(false);
                await _lstSpecialAttributes.DisposeAsync().ConfigureAwait(false);
                await _lstAttributes.DisposeAsync().ConfigureAwait(false);
                await _objAttributesInitializerLock.DisposeAsync().ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            await LockObject.DisposeAsync().ConfigureAwait(false);
        }

        internal void Save(XmlWriter objWriter, CancellationToken token = default)
        {
            using (EnterReadLock.Enter(LockObject, token))
            {
                foreach (CharacterAttrib objAttribute in AttributeList)
                {
                    objAttribute.Save(objWriter);
                }

                foreach (CharacterAttrib objAttribute in SpecialAttributeList)
                {
                    objAttribute.Save(objWriter);
                }
            }
        }

        public void Create(XmlNode charNode, int intValue, int intMinModifier = 0, int intMaxModifier = 0, CancellationToken token = default)
        {
            if (charNode == null)
                return;
            using (_objCharacter.LockObject.EnterWriteLock(token))
            using (LockObject.EnterWriteLock(token))
            {
                Interlocked.Increment(ref _intLoading);
                try
                {
                    using (_ = Timekeeper.StartSyncron("create_char_attrib", null,
                                                       CustomActivity.OperationType.RequestOperation,
                                                       charNode.InnerText))
                    {
                        int intOldBODBase = _objCharacter.BOD.Base;
                        int intOldBODKarma = _objCharacter.BOD.Karma;
                        int intOldAGIBase = _objCharacter.AGI.Base;
                        int intOldAGIKarma = _objCharacter.AGI.Karma;
                        int intOldREABase = _objCharacter.REA.Base;
                        int intOldREAKarma = _objCharacter.REA.Karma;
                        int intOldSTRBase = _objCharacter.STR.Base;
                        int intOldSTRKarma = _objCharacter.STR.Karma;
                        int intOldCHABase = _objCharacter.CHA.Base;
                        int intOldCHAKarma = _objCharacter.CHA.Karma;
                        int intOldINTBase = _objCharacter.INT.Base;
                        int intOldINTKarma = _objCharacter.INT.Karma;
                        int intOldLOGBase = _objCharacter.LOG.Base;
                        int intOldLOGKarma = _objCharacter.LOG.Karma;
                        int intOldWILBase = _objCharacter.WIL.Base;
                        int intOldWILKarma = _objCharacter.WIL.Karma;
                        int intOldEDGBase = _objCharacter.EDG.Base;
                        int intOldEDGKarma = _objCharacter.EDG.Karma;
                        int intOldMAGBase = _objCharacter.MAG.Base;
                        int intOldMAGKarma = _objCharacter.MAG.Karma;
                        int intOldMAGAdeptBase = _objCharacter.MAGAdept.Base;
                        int intOldMAGAdeptKarma = _objCharacter.MAGAdept.Karma;
                        int intOldRESBase = _objCharacter.RES.Base;
                        int intOldRESKarma = _objCharacter.RES.Karma;
                        int intOldDEPBase = _objCharacter.DEP.Base;
                        int intOldDEPKarma = _objCharacter.DEP.Karma;
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
                            }
                        }

                        _objCharacter.BOD.AssignLimits(
                            CommonFunctions.ExpressionToInt(charNode["bodmin"]?.InnerText, intValue, intMinModifier),
                            CommonFunctions.ExpressionToInt(charNode["bodmax"]?.InnerText, intValue, intMaxModifier),
                            CommonFunctions.ExpressionToInt(charNode["bodaug"]?.InnerText, intValue, intMaxModifier));
                        _objCharacter.AGI.AssignLimits(
                            CommonFunctions.ExpressionToInt(charNode["agimin"]?.InnerText, intValue, intMinModifier),
                            CommonFunctions.ExpressionToInt(charNode["agimax"]?.InnerText, intValue, intMaxModifier),
                            CommonFunctions.ExpressionToInt(charNode["agiaug"]?.InnerText, intValue, intMaxModifier));
                        _objCharacter.REA.AssignLimits(
                            CommonFunctions.ExpressionToInt(charNode["reamin"]?.InnerText, intValue, intMinModifier),
                            CommonFunctions.ExpressionToInt(charNode["reamax"]?.InnerText, intValue, intMaxModifier),
                            CommonFunctions.ExpressionToInt(charNode["reaaug"]?.InnerText, intValue, intMaxModifier));
                        _objCharacter.STR.AssignLimits(
                            CommonFunctions.ExpressionToInt(charNode["strmin"]?.InnerText, intValue, intMinModifier),
                            CommonFunctions.ExpressionToInt(charNode["strmax"]?.InnerText, intValue, intMaxModifier),
                            CommonFunctions.ExpressionToInt(charNode["straug"]?.InnerText, intValue, intMaxModifier));
                        _objCharacter.CHA.AssignLimits(
                            CommonFunctions.ExpressionToInt(charNode["chamin"]?.InnerText, intValue, intMinModifier),
                            CommonFunctions.ExpressionToInt(charNode["chamax"]?.InnerText, intValue, intMaxModifier),
                            CommonFunctions.ExpressionToInt(charNode["chaaug"]?.InnerText, intValue, intMaxModifier));
                        _objCharacter.INT.AssignLimits(
                            CommonFunctions.ExpressionToInt(charNode["intmin"]?.InnerText, intValue, intMinModifier),
                            CommonFunctions.ExpressionToInt(charNode["intmax"]?.InnerText, intValue, intMaxModifier),
                            CommonFunctions.ExpressionToInt(charNode["intaug"]?.InnerText, intValue, intMaxModifier));
                        _objCharacter.LOG.AssignLimits(
                            CommonFunctions.ExpressionToInt(charNode["logmin"]?.InnerText, intValue, intMinModifier),
                            CommonFunctions.ExpressionToInt(charNode["logmax"]?.InnerText, intValue, intMaxModifier),
                            CommonFunctions.ExpressionToInt(charNode["logaug"]?.InnerText, intValue, intMaxModifier));
                        _objCharacter.WIL.AssignLimits(
                            CommonFunctions.ExpressionToInt(charNode["wilmin"]?.InnerText, intValue, intMinModifier),
                            CommonFunctions.ExpressionToInt(charNode["wilmax"]?.InnerText, intValue, intMaxModifier),
                            CommonFunctions.ExpressionToInt(charNode["wilaug"]?.InnerText, intValue, intMaxModifier));
                        _objCharacter.MAG.AssignLimits(
                            CommonFunctions.ExpressionToInt(charNode["magmin"]?.InnerText, intValue, intMinModifier),
                            CommonFunctions.ExpressionToInt(charNode["magmax"]?.InnerText, intValue, intMaxModifier),
                            CommonFunctions.ExpressionToInt(charNode["magaug"]?.InnerText, intValue, intMaxModifier));
                        _objCharacter.RES.AssignLimits(
                            CommonFunctions.ExpressionToInt(charNode["resmin"]?.InnerText, intValue, intMinModifier),
                            CommonFunctions.ExpressionToInt(charNode["resmax"]?.InnerText, intValue, intMaxModifier),
                            CommonFunctions.ExpressionToInt(charNode["resaug"]?.InnerText, intValue, intMaxModifier));
                        _objCharacter.EDG.AssignLimits(
                            CommonFunctions.ExpressionToInt(charNode["edgmin"]?.InnerText, intValue, intMinModifier),
                            CommonFunctions.ExpressionToInt(charNode["edgmax"]?.InnerText, intValue, intMaxModifier),
                            CommonFunctions.ExpressionToInt(charNode["edgaug"]?.InnerText, intValue, intMaxModifier));
                        _objCharacter.DEP.AssignLimits(
                            CommonFunctions.ExpressionToInt(charNode["depmin"]?.InnerText, intValue, intMinModifier),
                            CommonFunctions.ExpressionToInt(charNode["depmax"]?.InnerText, intValue, intMaxModifier),
                            CommonFunctions.ExpressionToInt(charNode["depaug"]?.InnerText, intValue, intMaxModifier));
                        _objCharacter.MAGAdept.AssignLimits(
                            CommonFunctions.ExpressionToInt(charNode["magmin"]?.InnerText, intValue, intMinModifier),
                            CommonFunctions.ExpressionToInt(charNode["magmax"]?.InnerText, intValue, intMaxModifier),
                            CommonFunctions.ExpressionToInt(charNode["magaug"]?.InnerText, intValue, intMaxModifier));
                        _objCharacter.ESS.AssignLimits(
                            CommonFunctions.ExpressionToInt(charNode["essmin"]?.InnerText, intValue),
                            CommonFunctions.ExpressionToInt(charNode["essmax"]?.InnerText, intValue),
                            CommonFunctions.ExpressionToInt(charNode["essaug"]?.InnerText, intValue));

                        _objCharacter.BOD.Base = Math.Min(intOldBODBase, _objCharacter.BOD.PriorityMaximum);
                        _objCharacter.BOD.Karma = Math.Min(intOldBODKarma, _objCharacter.BOD.KarmaMaximum);
                        _objCharacter.AGI.Base = Math.Min(intOldAGIBase, _objCharacter.AGI.PriorityMaximum);
                        _objCharacter.AGI.Karma = Math.Min(intOldAGIKarma, _objCharacter.AGI.KarmaMaximum);
                        _objCharacter.REA.Base = Math.Min(intOldREABase, _objCharacter.REA.PriorityMaximum);
                        _objCharacter.REA.Karma = Math.Min(intOldREAKarma, _objCharacter.REA.KarmaMaximum);
                        _objCharacter.STR.Base = Math.Min(intOldSTRBase, _objCharacter.STR.PriorityMaximum);
                        _objCharacter.STR.Karma = Math.Min(intOldSTRKarma, _objCharacter.STR.KarmaMaximum);
                        _objCharacter.CHA.Base = Math.Min(intOldCHABase, _objCharacter.CHA.PriorityMaximum);
                        _objCharacter.CHA.Karma = Math.Min(intOldCHAKarma, _objCharacter.CHA.KarmaMaximum);
                        _objCharacter.INT.Base = Math.Min(intOldINTBase, _objCharacter.INT.PriorityMaximum);
                        _objCharacter.INT.Karma = Math.Min(intOldINTKarma, _objCharacter.INT.KarmaMaximum);
                        _objCharacter.LOG.Base = Math.Min(intOldLOGBase, _objCharacter.LOG.PriorityMaximum);
                        _objCharacter.LOG.Karma = Math.Min(intOldLOGKarma, _objCharacter.LOG.KarmaMaximum);
                        _objCharacter.WIL.Base = Math.Min(intOldWILBase, _objCharacter.WIL.PriorityMaximum);
                        _objCharacter.WIL.Karma = Math.Min(intOldWILKarma, _objCharacter.WIL.KarmaMaximum);
                        _objCharacter.EDG.Base = Math.Min(intOldEDGBase, _objCharacter.EDG.PriorityMaximum);
                        _objCharacter.EDG.Karma = Math.Min(intOldEDGKarma, _objCharacter.EDG.KarmaMaximum);

                        if (_objCharacter.MAGEnabled)
                        {
                            _objCharacter.MAG.Base = Math.Min(intOldMAGBase, _objCharacter.MAG.PriorityMaximum);
                            _objCharacter.MAG.Karma = Math.Min(intOldMAGKarma, _objCharacter.MAG.KarmaMaximum);
                            if (_objCharacter.Settings.MysAdeptSecondMAGAttribute && _objCharacter.IsMysticAdept)
                            {
                                _objCharacter.MAGAdept.Base =
                                    Math.Min(intOldMAGAdeptBase, _objCharacter.MAGAdept.PriorityMaximum);
                                _objCharacter.MAGAdept.Karma =
                                    Math.Min(intOldMAGAdeptKarma, _objCharacter.MAGAdept.KarmaMaximum);
                            }
                        }

                        if (_objCharacter.RESEnabled)
                        {
                            _objCharacter.RES.Base = Math.Min(intOldRESBase, _objCharacter.RES.PriorityMaximum);
                            _objCharacter.RES.Karma = Math.Min(intOldRESKarma, _objCharacter.RES.KarmaMaximum);
                        }

                        if (_objCharacter.DEPEnabled)
                        {
                            _objCharacter.DEP.Base = Math.Min(intOldDEPBase, _objCharacter.DEP.PriorityMaximum);
                            _objCharacter.DEP.Karma = Math.Min(intOldDEPKarma, _objCharacter.DEP.KarmaMaximum);
                        }

                        InitializeAttributesList();
                        ResetBindings();

                        //Timekeeper.Finish("create_char_attrib");
                    }
                }
                finally
                {
                    Interlocked.Decrement(ref _intLoading);
                }
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
                            ? (blnSync
                                // ReSharper disable once MethodHasAsyncOverload
                                ? _objCharacter.GetNodeXPath(true, token: token)
                                : await _objCharacter.GetNodeXPathAsync(true, token: token).ConfigureAwait(false))
                            : null;
                    foreach (string strAttribute in AttributeStrings)
                    {
                        XmlNodeList lstAttributeNodes =
                            xmlSavedCharacterNode.SelectNodes("attributes/attribute[name = " + strAttribute.CleanXPath()
                                                              +
                                                              ']');
                        // Couldn't find the appropriate attribute in the loaded file, so regenerate it from scratch.
                        if (lstAttributeNodes == null || lstAttributeNodes.Count == 0 || xmlCharNodeAnimalForm != null
                            &&
                            _objCharacter.LastSavedVersion < new Version(5, 200, 25))
                        {
                            CharacterAttrib objAttribute;
                            switch (CharacterAttrib.ConvertToAttributeCategory(strAttribute))
                            {
                                case CharacterAttrib.AttributeCategory.Special:
                                    objAttribute = new CharacterAttrib(_objCharacter, strAttribute,
                                                                       CharacterAttrib.AttributeCategory.Special);
                                    if (blnSync)
                                    {
                                        // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                                        objAttribute = RemakeAttribute(objAttribute, xmlCharNode);
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
                                        // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                                        objAttribute = RemakeAttribute(objAttribute, xmlCharNode);
                                        // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                                        AttributeList.Add(objAttribute);
                                    }
                                    else
                                    {
                                        objAttribute = await RemakeAttributeAsync(objAttribute, xmlCharNode, token).ConfigureAwait(false);
                                        await AttributeList.AddAsync(objAttribute, token).ConfigureAwait(false);
                                    }

                                    break;
                            }

                            if (xmlCharNodeAnimalForm == null)
                                continue;
                            objAttribute = new CharacterAttrib(_objCharacter, strAttribute,
                                                               CharacterAttrib.AttributeCategory.Shapeshifter);
                            objAttribute = blnSync
                                // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                                ? RemakeAttribute(objAttribute, xmlCharNodeAnimalForm)
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
                                }
                            }
                        }
                    }

                    if (blnSync)
                        // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                        ResetBindings();
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
            {
                using (_ = Timekeeper.StartSyncron("load_char_attrib", parentActivity))
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
                                    objAttribute = RemakeAttribute(objAttribute, xmlCharNode);
                                    SpecialAttributeList.Add(objAttribute);
                                    break;

                                case CharacterAttrib.AttributeCategory.Standard:
                                    objAttribute = new CharacterAttrib(_objCharacter, strAttribute,
                                                                       CharacterAttrib.AttributeCategory.Standard);
                                    objAttribute = RemakeAttribute(objAttribute, xmlCharNode);
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
                                        objAttribute = RemakeAttribute(objAttribute, xmlCharNodeAnimalForm);
                                        SpecialAttributeList.Add(objAttribute);
                                        break;

                                    case CharacterAttrib.AttributeCategory.Shapeshifter:
                                        objAttribute = new CharacterAttrib(_objCharacter, strAttribute,
                                                                           CharacterAttrib.AttributeCategory
                                                                               .Shapeshifter);
                                        objAttribute = RemakeAttribute(objAttribute, xmlCharNodeAnimalForm);
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
                            XPathNavigator xmlAttributeBaseNode = xmlHeroLabAttributeNode?.SelectSingleNodeAndCacheExpression("@base");
                            if (xmlAttributeBaseNode != null &&
                                int.TryParse(xmlAttributeBaseNode.Value, out int intHeroLabAttributeBaseValue))
                            {
                                int intAttributeMinimumValue = GetAttributeByName(strAttribute).MetatypeMinimum;
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
                            if (intAttributePointCount > 0 && AttributeList.Any(x => x.Karma != 0))
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
                            if (intAttributePointCount > 0 && SpecialAttributeList.Any(x => x.Karma != 0))
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

                        ResetBindings();
                    }
                    finally
                    {
                        Interlocked.Decrement(ref _intLoading);
                    }
                    //Timekeeper.Finish("load_char_attrib");
                }
            }
        }

        private static CharacterAttrib RemakeAttribute(CharacterAttrib objNewAttribute, XPathNavigator objCharacterNode)
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
                                    .Replace('F', '0').Replace("1D6", "0").Replace("2D6", "0") ?? "1");
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
                                    .Replace('F', '0').Replace("1D6", "0").Replace("2D6", "0") ?? "1");
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
                                    .Replace('F', '0').Replace("1D6", "0").Replace("2D6", "0") ?? "1");
                if (blnIsSuccess)
                    intAugValue = ((double)objProcess).StandardRound();
            }
            catch (XPathException) { intAugValue = 1; }
            catch (OverflowException) { intAugValue = 1; }
            catch (InvalidCastException) { intAugValue = 1; }

            objNewAttribute.AssignBaseKarmaLimits(
                objCharacterNode.SelectSingleNodeAndCacheExpression("base")?.ValueAsInt ?? 0,
                objCharacterNode.SelectSingleNodeAndCacheExpression("base")?.ValueAsInt ?? 0, intMinValue, intMaxValue,
                intAugValue);
            return objNewAttribute;
        }

        private static async ValueTask<CharacterAttrib> RemakeAttributeAsync(CharacterAttrib objNewAttribute, XPathNavigator objCharacterNode, CancellationToken token = default)
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
                (await objCharacterNode.SelectSingleNodeAndCacheExpressionAsync("base", token).ConfigureAwait(false))?.ValueAsInt ?? 0,
                (await objCharacterNode.SelectSingleNodeAndCacheExpressionAsync("base", token).ConfigureAwait(false))?.ValueAsInt ?? 0, intMinValue, intMaxValue,
                intAugValue, token).ConfigureAwait(false);
            return objNewAttribute;
        }

        internal async ValueTask Print(XmlWriter objWriter, CultureInfo objCulture, string strLanguageToPrint, CancellationToken token = default)
        {
            using (await EnterReadLock.EnterAsync(LockObject, token).ConfigureAwait(false))
            {
                if (_objCharacter.MetatypeCategory == "Shapeshifter")
                {
                    XPathNavigator xmlNode = await _objCharacter.GetNodeXPathAsync(true, token: token).ConfigureAwait(false);

                    if (AttributeCategory == CharacterAttrib.AttributeCategory.Standard)
                    {
                        await objWriter.WriteElementStringAsync("attributecategory",
                                                                xmlNode != null
                                                                    ? (await xmlNode
                                                                             .SelectSingleNodeAndCacheExpressionAsync(
                                                                                 "name/@translate", token: token).ConfigureAwait(false))
                                                                    ?.Value ?? _objCharacter.Metatype
                                                                    : _objCharacter.Metatype, token: token).ConfigureAwait(false);
                    }
                    else
                    {
                        xmlNode = xmlNode?.SelectSingleNode("metavariants/metavariant[id = "
                                                            + _objCharacter.MetavariantGuid
                                                                           .ToString(
                                                                               "D", GlobalSettings.InvariantCultureInfo)
                                                                           .CleanXPath() + ']');
                        await objWriter.WriteElementStringAsync("attributecategory",
                                                                xmlNode?.Value ?? _objCharacter.Metavariant, token: token).ConfigureAwait(false);
                    }
                }

                await objWriter.WriteElementStringAsync("attributecategory_english", AttributeCategory.ToString(), token: token).ConfigureAwait(false);
                foreach (CharacterAttrib att in AttributeList)
                {
                    await att.Print(objWriter, objCulture, strLanguageToPrint, token).ConfigureAwait(false);
                }

                foreach (CharacterAttrib att in SpecialAttributeList)
                {
                    await att.Print(objWriter, objCulture, strLanguageToPrint, token).ConfigureAwait(false);
                }
            }
        }

        #endregion Constructor, Save, Load, Print Methods

        #region Methods

        public CharacterAttrib GetAttributeByName(string abbrev)
        {
            using (EnterReadLock.Enter(LockObject))
            {
                bool blnGetShifterAttribute = _objCharacter.MetatypeCategory == "Shapeshifter" && _objCharacter.Created
                    && AttributeCategory == CharacterAttrib.AttributeCategory.Shapeshifter;
                CharacterAttrib objReturn
                    = AttributeList.Find(att => att.Abbrev == abbrev
                                                && (att.MetatypeCategory
                                                    == CharacterAttrib.AttributeCategory.Shapeshifter)
                                                == blnGetShifterAttribute)
                      ?? SpecialAttributeList.Find(att => att.Abbrev == abbrev);
                return objReturn;
            }
        }

        public async ValueTask<CharacterAttrib> GetAttributeByNameAsync(string abbrev, CancellationToken token = default)
        {
            using (await EnterReadLock.EnterAsync(LockObject, token).ConfigureAwait(false))
            {
                bool blnGetShifterAttribute = await _objCharacter.GetMetatypeCategoryAsync(token).ConfigureAwait(false) == "Shapeshifter" && await _objCharacter.GetCreatedAsync(token).ConfigureAwait(false)
                    && await GetAttributeCategoryAsync(token).ConfigureAwait(false) == CharacterAttrib.AttributeCategory.Shapeshifter;
                CharacterAttrib objReturn
                    = (await GetAttributeListAsync(token).ConfigureAwait(false)).Find(att => att.Abbrev == abbrev
                                                           && (att.MetatypeCategory
                                                               == CharacterAttrib.AttributeCategory.Shapeshifter)
                                                           == blnGetShifterAttribute)
                      ?? (await GetSpecialAttributeListAsync(token).ConfigureAwait(false)).Find(att => att.Abbrev == abbrev);
                return objReturn;
            }
        }

        public BindingSource GetAttributeBindingByName(string abbrev, CancellationToken token = default)
        {
            using (EnterReadLock.Enter(LockObject, token))
                return _dicBindings.TryGetValue(abbrev, out BindingSource objAttributeBinding, token) ? objAttributeBinding : null;
        }

        public async ValueTask<BindingSource> GetAttributeBindingByNameAsync(string abbrev, CancellationToken token = default)
        {
            using (await EnterReadLock.EnterAsync(LockObject, token).ConfigureAwait(false))
            {
                (bool blnSuccess, BindingSource objAttributeBinding) = await _dicBindings.TryGetValueAsync(abbrev, token).ConfigureAwait(false);
                return blnSuccess ? objAttributeBinding : null;
            }
        }

        internal void ForceAttributePropertyChangedNotificationAll(params string[] lstNames)
        {
            foreach (CharacterAttrib att in AttributeList)
            {
                att.OnMultiplePropertyChanged(Array.AsReadOnly(lstNames));
            }
        }

        public static void CopyAttribute(CharacterAttrib objSource, CharacterAttrib objTarget, string strMetavariantXPath, XmlDocument xmlDoc)
        {
            if (objSource == null || objTarget == null)
                return;
            using (EnterReadLock.Enter(objSource.LockObject))
            {
                string strSourceAbbrev = objSource.Abbrev.ToLowerInvariant();
                if (strSourceAbbrev == "magadept")
                    strSourceAbbrev = "mag";
                XmlNode node = !string.IsNullOrEmpty(strMetavariantXPath)
                    ? xmlDoc?.SelectSingleNode(strMetavariantXPath)
                    : null;
                if (node != null)
                {
                    int.TryParse(node[strSourceAbbrev + "min"]?.InnerText, NumberStyles.Any,
                                 GlobalSettings.InvariantCultureInfo, out int intMinimum);
                    int.TryParse(node[strSourceAbbrev + "max"]?.InnerText, NumberStyles.Any,
                                 GlobalSettings.InvariantCultureInfo, out int intMaximum);
                    int.TryParse(node[strSourceAbbrev + "aug"]?.InnerText, NumberStyles.Any,
                                 GlobalSettings.InvariantCultureInfo, out int intAugmentedMaximum);
                    intMaximum = Math.Max(intMaximum, intMinimum);
                    intAugmentedMaximum = Math.Max(intAugmentedMaximum, intMaximum);
                    objTarget.AssignBaseKarmaLimits(objSource.Base, objSource.Karma, intMinimum, intMaximum, intAugmentedMaximum);
                }
            }
        }

        public static async ValueTask CopyAttributeAsync(CharacterAttrib objSource, CharacterAttrib objTarget, string strMetavariantXPath, XmlDocument xmlDoc, CancellationToken token = default)
        {
            if (objSource == null || objTarget == null)
                return;
            using (await EnterReadLock.EnterAsync(objSource.LockObject, token).ConfigureAwait(false))
            {
                string strSourceAbbrev = objSource.Abbrev.ToLowerInvariant();
                if (strSourceAbbrev == "magadept")
                    strSourceAbbrev = "mag";
                XmlNode node = !string.IsNullOrEmpty(strMetavariantXPath)
                    ? xmlDoc?.SelectSingleNode(strMetavariantXPath)
                    : null;
                if (node != null)
                {
                    int.TryParse(node[strSourceAbbrev + "min"]?.InnerText, NumberStyles.Any,
                                 GlobalSettings.InvariantCultureInfo, out int intMinimum);
                    int.TryParse(node[strSourceAbbrev + "max"]?.InnerText, NumberStyles.Any,
                                 GlobalSettings.InvariantCultureInfo, out int intMaximum);
                    int.TryParse(node[strSourceAbbrev + "aug"]?.InnerText, NumberStyles.Any,
                                 GlobalSettings.InvariantCultureInfo, out int intAugmentedMaximum);
                    intMaximum = Math.Max(intMaximum, intMinimum);
                    intAugmentedMaximum = Math.Max(intAugmentedMaximum, intMaximum);
                    await objTarget.AssignBaseKarmaLimitsAsync(objSource.Base, objSource.Karma, intMinimum, intMaximum, intAugmentedMaximum, token).ConfigureAwait(false);
                }
            }
        }

        public string ProcessAttributesInXPath(string strInput, IReadOnlyDictionary<string, int> dicValueOverrides = null, CancellationToken token = default)
        {
            if (string.IsNullOrEmpty(strInput))
                return strInput;
            using (EnterReadLock.Enter(LockObject, token))
            {
                string strReturn = strInput;
                foreach (string strCharAttributeName in AttributeStrings)
                {
                    strReturn = strReturn
                                .CheapReplace('{' + strCharAttributeName + '}', () =>
                                                  (dicValueOverrides?.ContainsKey(strCharAttributeName) == true
                                                      ? dicValueOverrides[strCharAttributeName]
                                                      : _objCharacter.GetAttribute(strCharAttributeName).TotalValue)
                                                  .ToString(GlobalSettings.InvariantCultureInfo))
                                .CheapReplace('{' + strCharAttributeName + "Unaug}", () =>
                                                  (dicValueOverrides?.ContainsKey(strCharAttributeName + "Unaug")
                                                   == true
                                                      ? dicValueOverrides[strCharAttributeName + "Unaug"]
                                                      : _objCharacter.GetAttribute(strCharAttributeName).Value)
                                                  .ToString(GlobalSettings.InvariantCultureInfo))
                                .CheapReplace('{' + strCharAttributeName + "Base}", () =>
                                                  (dicValueOverrides?.ContainsKey(strCharAttributeName + "Base") == true
                                                      ? dicValueOverrides[strCharAttributeName + "Base"]
                                                      : _objCharacter.GetAttribute(strCharAttributeName).TotalBase)
                                                  .ToString(GlobalSettings.InvariantCultureInfo));
                }

                return strReturn;
            }
        }

        public void ProcessAttributesInXPath(StringBuilder sbdInput, string strOriginal = "", IReadOnlyDictionary<string, int> dicValueOverrides = null, CancellationToken token = default)
        {
            if (sbdInput == null || sbdInput.Length <= 0)
                return;
            if (string.IsNullOrEmpty(strOriginal))
                strOriginal = sbdInput.ToString();
            using (EnterReadLock.Enter(LockObject, token))
            {
                foreach (string strCharAttributeName in AttributeStrings)
                {
                    sbdInput.CheapReplace(strOriginal, '{' + strCharAttributeName + '}', () =>
                                              (dicValueOverrides?.ContainsKey(strCharAttributeName) == true
                                                  ? dicValueOverrides[strCharAttributeName]
                                                  : _objCharacter.GetAttribute(strCharAttributeName).TotalValue)
                                              .ToString(GlobalSettings.InvariantCultureInfo));
                    sbdInput.CheapReplace(strOriginal, '{' + strCharAttributeName + "Unaug}", () =>
                                              (dicValueOverrides?.ContainsKey(strCharAttributeName + "Unaug") == true
                                                  ? dicValueOverrides[strCharAttributeName + "Unaug"]
                                                  : _objCharacter.GetAttribute(strCharAttributeName).Value)
                                              .ToString(GlobalSettings.InvariantCultureInfo));
                    sbdInput.CheapReplace(strOriginal, '{' + strCharAttributeName + "Base}", () =>
                                              (dicValueOverrides?.ContainsKey(strCharAttributeName + "Base") == true
                                                  ? dicValueOverrides[strCharAttributeName + "Base"]
                                                  : _objCharacter.GetAttribute(strCharAttributeName).TotalBase)
                                              .ToString(GlobalSettings.InvariantCultureInfo));
                }
            }
        }

        public async Task<string> ProcessAttributesInXPathAsync(string strInput, IReadOnlyDictionary<string, int> dicValueOverrides = null, CancellationToken token = default)
        {
            if (string.IsNullOrEmpty(strInput))
                return strInput;
            using (await EnterReadLock.EnterAsync(LockObject, token).ConfigureAwait(false))
            {
                string strReturn = strInput;
                foreach (string strCharAttributeName in AttributeStrings)
                {
                    CharacterAttrib objAttribute = await _objCharacter.GetAttributeAsync(strCharAttributeName, token: token).ConfigureAwait(false);
                    strReturn = await (await (await strReturn
                                                    .CheapReplaceAsync('{' + strCharAttributeName + '}', async () =>
                                                                           (dicValueOverrides?.ContainsKey(strCharAttributeName) == true
                                                                               ? dicValueOverrides[strCharAttributeName]
                                                                               : await objAttribute.GetTotalValueAsync(token).ConfigureAwait(false))
                                                                           .ToString(GlobalSettings.InvariantCultureInfo), token: token).ConfigureAwait(false))
                                             .CheapReplaceAsync('{' + strCharAttributeName + "Unaug}", async () =>
                                                                    (dicValueOverrides?.ContainsKey(strCharAttributeName + "Unaug")
                                                                     == true
                                                                        ? dicValueOverrides[strCharAttributeName + "Unaug"]
                                                                        : await objAttribute.GetValueAsync(token).ConfigureAwait(false))
                                                                    .ToString(GlobalSettings.InvariantCultureInfo), token: token).ConfigureAwait(false))
                                      .CheapReplaceAsync('{' + strCharAttributeName + "Base}", async () =>
                                                             (dicValueOverrides?.ContainsKey(strCharAttributeName + "Base") == true
                                                                 ? dicValueOverrides[strCharAttributeName + "Base"]
                                                                 : await objAttribute.GetTotalBaseAsync(token).ConfigureAwait(false))
                                                             .ToString(GlobalSettings.InvariantCultureInfo), token: token).ConfigureAwait(false);
                }

                return strReturn;
            }
        }

        public async Task ProcessAttributesInXPathAsync(StringBuilder sbdInput, string strOriginal = "", IReadOnlyDictionary<string, int> dicValueOverrides = null, CancellationToken token = default)
        {
            if (sbdInput == null || sbdInput.Length <= 0)
                return;
            if (string.IsNullOrEmpty(strOriginal))
                strOriginal = sbdInput.ToString();
            using (await EnterReadLock.EnterAsync(LockObject, token).ConfigureAwait(false))
            {
                foreach (string strCharAttributeName in AttributeStrings)
                {
                    CharacterAttrib objAttribute = await _objCharacter.GetAttributeAsync(strCharAttributeName, token: token).ConfigureAwait(false);
                    await sbdInput.CheapReplaceAsync(strOriginal, '{' + strCharAttributeName + '}', async () =>
                                                         (dicValueOverrides?.ContainsKey(strCharAttributeName) == true
                                                             ? dicValueOverrides[strCharAttributeName]
                                                             : await objAttribute.GetTotalValueAsync(token).ConfigureAwait(false))
                                                         .ToString(GlobalSettings.InvariantCultureInfo), token: token).ConfigureAwait(false);
                    await sbdInput.CheapReplaceAsync(strOriginal, '{' + strCharAttributeName + "Unaug}", async () =>
                                                         (dicValueOverrides?.ContainsKey(strCharAttributeName + "Unaug") == true
                                                             ? dicValueOverrides[strCharAttributeName + "Unaug"]
                                                             : await objAttribute.GetValueAsync(token).ConfigureAwait(false))
                                                         .ToString(GlobalSettings.InvariantCultureInfo), token: token).ConfigureAwait(false);
                    await sbdInput.CheapReplaceAsync(strOriginal, '{' + strCharAttributeName + "Base}", async () =>
                                                         (dicValueOverrides?.ContainsKey(strCharAttributeName + "Base") == true
                                                             ? dicValueOverrides[strCharAttributeName + "Base"]
                                                             : await objAttribute.GetTotalBaseAsync(token).ConfigureAwait(false))
                                                         .ToString(GlobalSettings.InvariantCultureInfo), token: token).ConfigureAwait(false);
                }
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
            using (EnterReadLock.Enter(LockObject, token))
            {
                foreach (string strCharAttributeName in AttributeStrings)
                {
                    strReturn = strReturn
                                .CheapReplace('{' + strCharAttributeName + '}', () =>
                                {
                                    string strInnerReturn = _objCharacter.GetAttribute(strCharAttributeName)
                                                                         .DisplayNameShort(strLanguage);
                                    if (blnShowValues)
                                    {
                                        if (dicValueOverrides == null
                                            || !dicValueOverrides.TryGetValue(
                                                strCharAttributeName, out int intAttributeValue))
                                            intAttributeValue = _objCharacter.GetAttribute(strCharAttributeName)
                                                                             .TotalValue;
                                        strInnerReturn += strSpace + '(' + intAttributeValue.ToString(objCultureInfo)
                                                          + ')';
                                    }

                                    return strInnerReturn;
                                })
                                .CheapReplace('{' + strCharAttributeName + "Unaug}", () =>
                                {
                                    string strInnerReturn = _objCharacter.GetAttribute(strCharAttributeName)
                                                                         .DisplayNameShort(strLanguage);
                                    if (blnShowValues)
                                    {
                                        if (dicValueOverrides == null
                                            || !dicValueOverrides.TryGetValue(
                                                strCharAttributeName + "Unaug", out int intAttributeValue))
                                            intAttributeValue = _objCharacter.GetAttribute(strCharAttributeName).Value;
                                        strInnerReturn += strSpace + '(' + intAttributeValue.ToString(objCultureInfo)
                                                          + ')';
                                    }

                                    return string.Format(objCultureInfo,
                                                         LanguageManager.GetString(
                                                             "String_NaturalAttribute", strLanguage), strInnerReturn);
                                })
                                .CheapReplace('{' + strCharAttributeName + "Base}", () =>
                                {
                                    string strInnerReturn = _objCharacter.GetAttribute(strCharAttributeName)
                                                                         .DisplayNameShort(strLanguage);
                                    if (blnShowValues)
                                    {
                                        if (dicValueOverrides == null
                                            || !dicValueOverrides.TryGetValue(
                                                strCharAttributeName + "Base", out int intAttributeValue))
                                            intAttributeValue = _objCharacter.GetAttribute(strCharAttributeName)
                                                                             .TotalBase;
                                        strInnerReturn += strSpace + '(' + intAttributeValue.ToString(objCultureInfo)
                                                          + ')';
                                    }

                                    return string.Format(objCultureInfo,
                                                         LanguageManager.GetString("String_BaseAttribute", strLanguage),
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
            using (EnterReadLock.Enter(LockObject, token))
            {
                foreach (string strCharAttributeName in AttributeStrings)
                {
                    sbdInput.CheapReplace(strOriginal, '{' + strCharAttributeName + '}', () =>
                    {
                        string strInnerReturn = _objCharacter.GetAttribute(strCharAttributeName)
                                                             .DisplayNameShort(strLanguage);
                        if (blnShowValues)
                        {
                            if (dicValueOverrides == null
                                || !dicValueOverrides.TryGetValue(strCharAttributeName, out int intAttributeValue))
                                intAttributeValue = _objCharacter.GetAttribute(strCharAttributeName).TotalValue;
                            strInnerReturn += strSpace + '(' + intAttributeValue.ToString(objCultureInfo) + ')';
                        }

                        return strInnerReturn;
                    });
                    sbdInput.CheapReplace(strOriginal, '{' + strCharAttributeName + "Unaug}", () =>
                    {
                        string strInnerReturn = _objCharacter.GetAttribute(strCharAttributeName)
                                                             .DisplayNameShort(strLanguage);
                        if (blnShowValues)
                        {
                            if (dicValueOverrides == null
                                || !dicValueOverrides.TryGetValue(strCharAttributeName + "Unaug",
                                                                  out int intAttributeValue))
                                intAttributeValue = _objCharacter.GetAttribute(strCharAttributeName).Value;
                            strInnerReturn += strSpace + '(' + intAttributeValue.ToString(objCultureInfo) + ')';
                        }

                        return string.Format(objCultureInfo,
                                             LanguageManager.GetString("String_NaturalAttribute", strLanguage),
                                             strInnerReturn);
                    });
                    sbdInput.CheapReplace(strOriginal, '{' + strCharAttributeName + "Base}", () =>
                    {
                        string strInnerReturn = _objCharacter.GetAttribute(strCharAttributeName)
                                                             .DisplayNameShort(strLanguage);
                        if (blnShowValues)
                        {
                            if (dicValueOverrides == null
                                || !dicValueOverrides.TryGetValue(strCharAttributeName + "Base",
                                                                  out int intAttributeValue))
                                intAttributeValue = _objCharacter.GetAttribute(strCharAttributeName).TotalBase;
                            strInnerReturn += strSpace + '(' + intAttributeValue.ToString(objCultureInfo) + ')';
                        }

                        return string.Format(objCultureInfo,
                                             LanguageManager.GetString("String_BaseAttribute", strLanguage),
                                             strInnerReturn);
                    });
                }
            }
        }

        public async ValueTask<string> ProcessAttributesInXPathForTooltipAsync(string strInput, CultureInfo objCultureInfo = null, string strLanguage = "", bool blnShowValues = true, IAsyncReadOnlyDictionary<string, int> dicValueOverrides = null, CancellationToken token = default)
        {
            if (string.IsNullOrEmpty(strInput))
                return strInput;
            if (objCultureInfo == null)
                objCultureInfo = GlobalSettings.CultureInfo;
            if (string.IsNullOrEmpty(strLanguage))
                strLanguage = GlobalSettings.Language;
            string strSpace = await LanguageManager.GetStringAsync("String_Space", strLanguage, token: token).ConfigureAwait(false);
            string strReturn = strInput;
            using (await EnterReadLock.EnterAsync(LockObject, token).ConfigureAwait(false))
            {
                foreach (string strCharAttributeName in AttributeStrings)
                {
                    strReturn = await strReturn
                                      .CheapReplaceAsync('{' + strCharAttributeName + '}', async () =>
                                      {
                                          string strInnerReturn = await (await _objCharacter.GetAttributeAsync(strCharAttributeName, token: token).ConfigureAwait(false))
                                                                        .DisplayNameShortAsync(strLanguage, token).ConfigureAwait(false);
                                          if (blnShowValues)
                                          {
                                              if (dicValueOverrides == null
                                                  || !dicValueOverrides.TryGetValue(
                                                      strCharAttributeName, out int intAttributeValue))
                                                  intAttributeValue = await (await _objCharacter.GetAttributeAsync(strCharAttributeName, token: token).ConfigureAwait(false))
                                                                            .GetTotalValueAsync(token).ConfigureAwait(false);
                                              strInnerReturn
                                                  += strSpace + '(' + intAttributeValue.ToString(objCultureInfo)
                                                     + ')';
                                          }

                                          return strInnerReturn;
                                      }, token: token)
                                      .CheapReplaceAsync('{' + strCharAttributeName + "Unaug}", async () =>
                                      {
                                          string strInnerReturn = await (await _objCharacter.GetAttributeAsync(strCharAttributeName, token: token).ConfigureAwait(false))
                                                                        .DisplayNameShortAsync(strLanguage, token).ConfigureAwait(false);
                                          if (blnShowValues)
                                          {
                                              if (dicValueOverrides == null
                                                  || !dicValueOverrides.TryGetValue(
                                                      strCharAttributeName + "Unaug", out int intAttributeValue))
                                                  intAttributeValue = await (await _objCharacter.GetAttributeAsync(strCharAttributeName, token: token).ConfigureAwait(false))
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
                                      .CheapReplaceAsync('{' + strCharAttributeName + "Base}", async () =>
                                      {
                                          string strInnerReturn = await (await _objCharacter.GetAttributeAsync(strCharAttributeName, token: token).ConfigureAwait(false))
                                                                        .DisplayNameShortAsync(strLanguage, token).ConfigureAwait(false);
                                          if (blnShowValues)
                                          {
                                              if (dicValueOverrides == null
                                                  || !dicValueOverrides.TryGetValue(
                                                      strCharAttributeName + "Base", out int intAttributeValue))
                                                  intAttributeValue = await (await _objCharacter.GetAttributeAsync(strCharAttributeName, token: token).ConfigureAwait(false))
                                                                            .GetTotalBaseAsync(token).ConfigureAwait(false);
                                              strInnerReturn
                                                  += strSpace + '(' + intAttributeValue.ToString(objCultureInfo)
                                                     + ')';
                                          }

                                          return string.Format(objCultureInfo,
                                                               await LanguageManager.GetStringAsync(
                                                                   "String_BaseAttribute", strLanguage, token: token).ConfigureAwait(false),
                                                               strInnerReturn);
                                      }, token: token).ConfigureAwait(false);
                }
            }

            return strReturn;
        }

        public async ValueTask ProcessAttributesInXPathForTooltipAsync(StringBuilder sbdInput, string strOriginal = "", CultureInfo objCultureInfo = null, string strLanguage = "", bool blnShowValues = true, IAsyncReadOnlyDictionary<string, int> dicValueOverrides = null, CancellationToken token = default)
        {
            if (sbdInput == null || sbdInput.Length <= 0)
                return;
            if (string.IsNullOrEmpty(strOriginal))
                strOriginal = sbdInput.ToString();
            if (objCultureInfo == null)
                objCultureInfo = GlobalSettings.CultureInfo;
            if (string.IsNullOrEmpty(strLanguage))
                strLanguage = GlobalSettings.Language;
            string strSpace = await LanguageManager.GetStringAsync("String_Space", strLanguage, token: token).ConfigureAwait(false);
            using (await EnterReadLock.EnterAsync(LockObject, token).ConfigureAwait(false))
            {
                foreach (string strCharAttributeName in AttributeStrings)
                {
                    await sbdInput.CheapReplaceAsync(strOriginal, '{' + strCharAttributeName + '}', async () =>
                    {
                        string strInnerReturn = await (await _objCharacter.GetAttributeAsync(strCharAttributeName, token: token).ConfigureAwait(false))
                                                      .DisplayNameShortAsync(strLanguage, token).ConfigureAwait(false);
                        if (blnShowValues)
                        {
                            if (dicValueOverrides == null
                                || !dicValueOverrides.TryGetValue(strCharAttributeName, out int intAttributeValue))
                                intAttributeValue = await (await _objCharacter.GetAttributeAsync(strCharAttributeName, token: token).ConfigureAwait(false)).GetTotalValueAsync(token).ConfigureAwait(false);
                            strInnerReturn += strSpace + '(' + intAttributeValue.ToString(objCultureInfo) + ')';
                        }

                        return strInnerReturn;
                    }, token: token).ConfigureAwait(false);
                    await sbdInput.CheapReplaceAsync(strOriginal, '{' + strCharAttributeName + "Unaug}", async () =>
                    {
                        string strInnerReturn = await (await _objCharacter.GetAttributeAsync(strCharAttributeName, token: token).ConfigureAwait(false))
                                                      .DisplayNameShortAsync(strLanguage, token).ConfigureAwait(false);
                        if (blnShowValues)
                        {
                            if (dicValueOverrides == null
                                || !dicValueOverrides.TryGetValue(strCharAttributeName + "Unaug",
                                                                  out int intAttributeValue))
                                intAttributeValue = await (await _objCharacter.GetAttributeAsync(strCharAttributeName, token: token).ConfigureAwait(false)).GetValueAsync(token).ConfigureAwait(false);
                            strInnerReturn += strSpace + '(' + intAttributeValue.ToString(objCultureInfo) + ')';
                        }

                        return string.Format(objCultureInfo,
                                             await LanguageManager.GetStringAsync("String_NaturalAttribute", strLanguage, token: token).ConfigureAwait(false),
                                             strInnerReturn);
                    }, token: token).ConfigureAwait(false);
                    await sbdInput.CheapReplaceAsync(strOriginal, '{' + strCharAttributeName + "Base}", async () =>
                    {
                        string strInnerReturn = await (await _objCharacter.GetAttributeAsync(strCharAttributeName, token: token).ConfigureAwait(false))
                                                      .DisplayNameShortAsync(strLanguage, token).ConfigureAwait(false);
                        if (blnShowValues)
                        {
                            if (dicValueOverrides == null
                                || !dicValueOverrides.TryGetValue(strCharAttributeName + "Base",
                                                                  out int intAttributeValue))
                                intAttributeValue = await (await _objCharacter.GetAttributeAsync(strCharAttributeName, token: token).ConfigureAwait(false)).GetTotalBaseAsync(token).ConfigureAwait(false);
                            strInnerReturn += strSpace + '(' + intAttributeValue.ToString(objCultureInfo) + ')';
                        }

                        return string.Format(objCultureInfo,
                                             await LanguageManager.GetStringAsync("String_BaseAttribute", strLanguage, token: token).ConfigureAwait(false),
                                             strInnerReturn);
                    }, token: token).ConfigureAwait(false);
                }
            }
        }

        internal void Reset(bool blnFirstTime = false)
        {
            using (LockObject.EnterWriteLock())
            {
                Interlocked.Increment(ref _intLoading);
                try
                {
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
                        }
                    }

                    if (blnFirstTime)
                    {
                        foreach (BindingSource objSource in _dicBindings.Values)
                            objSource.Dispose();
                        _dicBindings.Clear();
                        foreach (string strAttributeString in AttributeStrings)
                        {
                            _dicBindings.Add(strAttributeString, new BindingSource());
                        }
                    }

                    ResetBindings();
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

        internal async ValueTask ResetAsync(bool blnFirstTime = false, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
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
                        }
                    }

                    if (blnFirstTime)
                    {
                        foreach (BindingSource objSource in _dicBindings.Values)
                            objSource.Dispose();
                        await _dicBindings.ClearAsync(token).ConfigureAwait(false);
                        foreach (string strAttributeString in AttributeStrings)
                        {
                            await _dicBindings.AddAsync(strAttributeString, new BindingSource(), token).ConfigureAwait(false);
                        }
                    }

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
        public void ResetBindings()
        {
            using (_objCharacter.LockObject.EnterWriteLock())
            using (LockObject.EnterWriteLock())
            {
                foreach (KeyValuePair<string, BindingSource> objBindingEntry in _dicBindings)
                {
                    objBindingEntry.Value.DataSource = GetAttributeByName(objBindingEntry.Key);
                }
                _objCharacter.RefreshAttributeBindings();
                foreach (KeyValuePair<string, BindingSource> objBindingEntry in _dicBindings)
                {
                    objBindingEntry.Value.ResetBindings(false);
                }
            }
        }

        /// <summary>
        /// Reset the databindings for all character attributes.
        /// This method is used to support hot-swapping attributes for shapeshifters.
        /// </summary>
        public async ValueTask ResetBindingsAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker1 = await _objCharacter.LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    await _dicBindings.ForEachAsync(
                        async objBindingEntry => objBindingEntry.Value.DataSource =
                            await GetAttributeByNameAsync(objBindingEntry.Key, token).ConfigureAwait(false), token: token).ConfigureAwait(false);
                    await _objCharacter.RefreshAttributeBindingsAsync(token).ConfigureAwait(false);
                    await _dicBindings.ForEachAsync(objBindingEntry => objBindingEntry.Value.ResetBindings(false),
                        token: token).ConfigureAwait(false);
                }
                finally
                {
                    await objLocker2.DisposeAsync().ConfigureAwait(false);
                }
            }
            finally
            {
                await objLocker1.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Can an attribute be increased to its metatype maximum?
        /// Note: if the attribute is already at its metatype maximum, this method assumes that we just raised it and will decrease it if it's illegal.
        /// </summary>
        /// <param name="objAttribute">Attribute to raise to its metatype maximum.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        /// <returns></returns>
        public async ValueTask<bool> CanRaiseAttributeToMetatypeMax(CharacterAttrib objAttribute,
                                                                    CancellationToken token = default)
        {
            using (await EnterReadLock.EnterAsync(LockObject, token).ConfigureAwait(false))
            {
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
                using (EnterReadLock.Enter(LockObject))
                    return _lstNormalAttributes;
            }
        }

        public async ValueTask<ThreadSafeObservableCollection<CharacterAttrib>> GetAttributeListAsync(CancellationToken token = default)
        {
            using (await EnterReadLock.EnterAsync(LockObject, token).ConfigureAwait(false))
                return _lstNormalAttributes;
        }

        /// <summary>
        /// Character's Attributes.
        /// </summary>
        public ThreadSafeObservableCollection<CharacterAttrib> SpecialAttributeList
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                    return _lstSpecialAttributes;
            }
        }

        public async ValueTask<ThreadSafeObservableCollection<CharacterAttrib>> GetSpecialAttributeListAsync(CancellationToken token = default)
        {
            using (await EnterReadLock.EnterAsync(LockObject, token).ConfigureAwait(false))
                return _lstSpecialAttributes;
        }

        public CharacterAttrib.AttributeCategory AttributeCategory
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                    return _eAttributeCategory;
            }
            set
            {
                using (EnterReadLock.Enter(LockObject))
                {
                    // No need to write lock because interlocked guarantees safety
                    if (InterlockedExtensions.Exchange(ref _eAttributeCategory, value) == value)
                        return;

                    if (_objCharacter.Created)
                    {
                        ResetBindings();
                        ForceAttributePropertyChangedNotificationAll(nameof(CharacterAttrib.MetatypeMaximum),
                                                                     nameof(CharacterAttrib.MetatypeMinimum));
                    }

                    OnPropertyChanged();
                }
            }
        }

        public async ValueTask<CharacterAttrib.AttributeCategory> GetAttributeCategoryAsync(CancellationToken token = default)
        {
            using (await EnterReadLock.EnterAsync(LockObject, token).ConfigureAwait(false))
                return _eAttributeCategory;
        }

        #endregion Properties

        public AsyncFriendlyReaderWriterLock LockObject { get; } = new AsyncFriendlyReaderWriterLock();
    }
}
