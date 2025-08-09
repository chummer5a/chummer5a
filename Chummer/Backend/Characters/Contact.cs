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
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;
using Chummer.Annotations;
using NLog;

namespace Chummer
{
    /// <summary>
    /// Type of Contact.
    /// </summary>
    public enum ContactType
    {
        Contact = 0,
        Enemy = 1,
        Pet = 2
    }

    /// <summary>
    /// A Contact or Enemy.
    /// </summary>
    [DebuggerDisplay("{" + nameof(Name) + "} ({DisplayRoleMethod(\"en-us\")})")]
    public sealed class Contact : INotifyMultiplePropertiesChangedAsync, IHasName, IHasMugshots, IHasNotes, IHasInternalId, IHasLockObject, IHasCharacterObject
    {
        private static readonly Lazy<Logger> s_ObjLogger = new Lazy<Logger>(LogManager.GetCurrentClassLogger);
        private static Logger Log => s_ObjLogger.Value;
        private string _strName = string.Empty;
        private string _strRole = string.Empty;
        private string _strLocation = string.Empty;
        private string _strUnique = Guid.NewGuid().ToString("D", GlobalSettings.InvariantCultureInfo);

        private int _intConnection = 1;
        private int _intLoyalty = 1;
        private string _strMetatype = string.Empty;
        private string _strGender = string.Empty;
        private string _strAge = string.Empty;
        private string _strType = string.Empty;
        private string _strPreferredPayment = string.Empty;
        private string _strHobbiesVice = string.Empty;
        private string _strPersonalLife = string.Empty;

        private string _strGroupName = string.Empty;
        private ContactType _eContactType = ContactType.Contact;
        private string _strFileName = string.Empty;
        private string _strRelativeName = string.Empty;
        private Character _objLinkedCharacter;
        private string _strNotes = string.Empty;
        private Color _colNotes = ColorManager.HasNotesColor;
        private Color _objColor;
        private bool _blnIsGroup;
        private readonly Character _objCharacter;
        private bool _blnBlackmail;
        private bool _blnFamily;
        private bool _blnGroupEnabled = true;
        private bool _blnReadOnly;
        private bool _blnFree;
        private readonly ThreadSafeList<Image> _lstMugshots;
        private int _intMainMugshotIndex = -1;

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
                                = s_ContactDependencyGraph.GetWithAllDependents(this, strPropertyName, true);
                        else
                        {
                            foreach (string strLoopChangedProperty in s_ContactDependencyGraph
                                         .GetWithAllDependentsEnumerable(this, strPropertyName))
                                setNamesOfChangedProperties.Add(strLoopChangedProperty);
                        }
                    }

                    if (setNamesOfChangedProperties == null || setNamesOfChangedProperties.Count == 0)
                        return;

                    using (LockObject.EnterWriteLock())
                    {
                        if (setNamesOfChangedProperties.Contains(nameof(ForcedLoyalty)))
                            _intCachedForcedLoyalty = int.MinValue;
                        if (setNamesOfChangedProperties.Contains(nameof(GroupEnabled)))
                            _intCachedGroupEnabled = -1;
                        if (setNamesOfChangedProperties.Contains(nameof(Free)))
                            _intCachedFreeFromImprovement = -1;
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

                    if (setNamesOfChangedProperties.Contains(nameof(Free))
                        || setNamesOfChangedProperties.Contains(nameof(ContactPoints))
                        || !Free)
                    {
                        if (setNamesOfChangedProperties.Contains(nameof(IsEnemy)))
                        {
                            if (setNamesOfChangedProperties.Contains(nameof(IsGroup)) || IsGroup)
                                _objCharacter.OnMultiplePropertyChanged(nameof(Character.EnemyKarma),
                                    nameof(Character.PositiveQualityKarma));
                            else
                                _objCharacter.OnPropertyChanged(nameof(Character.EnemyKarma));
                        }
                        else if (IsEnemy)
                            _objCharacter.OnPropertyChanged(nameof(Character.EnemyKarma));
                        else if (setNamesOfChangedProperties.Contains(nameof(IsGroup)) || IsGroup)
                            _objCharacter.OnPropertyChanged(nameof(Character.PositiveQualityKarma));
                    }
                }
                finally
                {
                    if (setNamesOfChangedProperties != null)
                        Utils.StringHashSetPool.Return(ref setNamesOfChangedProperties);
                }
            }
        }

        public async Task OnMultiplePropertiesChangedAsync(IReadOnlyCollection<string> lstPropertyNames,
            CancellationToken token = default)
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
                                = await s_ContactDependencyGraph.GetWithAllDependentsAsync(this, strPropertyName, true, token).ConfigureAwait(false);
                        else
                        {
                            foreach (string strLoopChangedProperty in await s_ContactDependencyGraph
                                         .GetWithAllDependentsEnumerableAsync(this, strPropertyName, token).ConfigureAwait(false))
                                setNamesOfChangedProperties.Add(strLoopChangedProperty);
                        }
                    }

                    if (setNamesOfChangedProperties == null || setNamesOfChangedProperties.Count == 0)
                        return;

                    IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                    try
                    {
                        token.ThrowIfCancellationRequested();
                        if (setNamesOfChangedProperties.Contains(nameof(ForcedLoyalty)))
                            _intCachedForcedLoyalty = int.MinValue;
                        if (setNamesOfChangedProperties.Contains(nameof(GroupEnabled)))
                            _intCachedGroupEnabled = -1;
                        if (setNamesOfChangedProperties.Contains(nameof(Free)))
                            _intCachedFreeFromImprovement = -1;
                    }
                    finally
                    {
                        await objLocker2.DisposeAsync().ConfigureAwait(false);
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
                        List<Task> lstTasks = new List<Task>(Utils.MaxParallelBatchSize);
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
                                    PropertyChanged.Invoke(this, new PropertyChangedEventArgs(strPropertyToChange));
                                }
                            }
                        }, token).ConfigureAwait(false);
                    }

                    if (setNamesOfChangedProperties.Contains(nameof(Free))
                        || setNamesOfChangedProperties.Contains(nameof(ContactPoints))
                        || !await GetFreeAsync(token).ConfigureAwait(false))
                    {
                        if (setNamesOfChangedProperties.Contains(nameof(IsEnemy)))
                        {
                            if (setNamesOfChangedProperties.Contains(nameof(IsGroup)) ||
                                await GetIsGroupAsync(token).ConfigureAwait(false))
                                await _objCharacter.OnMultiplePropertyChangedAsync(token, nameof(Character.EnemyKarma),
                                    nameof(Character.PositiveQualityKarma)).ConfigureAwait(false);
                            else
                                await _objCharacter.OnPropertyChangedAsync(nameof(Character.EnemyKarma), token)
                                    .ConfigureAwait(false);
                        }
                        else if (await GetIsEnemyAsync(token).ConfigureAwait(false))
                            await _objCharacter.OnPropertyChangedAsync(nameof(Character.EnemyKarma), token)
                                .ConfigureAwait(false);
                        else if (setNamesOfChangedProperties.Contains(nameof(IsGroup)) ||
                                 await GetIsGroupAsync(token).ConfigureAwait(false))
                            await _objCharacter.OnPropertyChangedAsync(nameof(Character.PositiveQualityKarma), token)
                                .ConfigureAwait(false);
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

        private static readonly PropertyDependencyGraph<Contact> s_ContactDependencyGraph =
            new PropertyDependencyGraph<Contact>(
                new DependencyGraphNode<string, Contact>(nameof(NoLinkedCharacter),
                    new DependencyGraphNode<string, Contact>(nameof(LinkedCharacter))
                ),
                new DependencyGraphNode<string, Contact>(nameof(CurrentDisplayName),
                    new DependencyGraphNode<string, Contact>(nameof(Name),
                        new DependencyGraphNode<string, Contact>(nameof(LinkedCharacter))
                    )
                ),
                new DependencyGraphNode<string, Contact>(nameof(DisplayGender),
                    new DependencyGraphNode<string, Contact>(nameof(Gender),
                        new DependencyGraphNode<string, Contact>(nameof(LinkedCharacter))
                    )
                ),
                new DependencyGraphNode<string, Contact>(nameof(DisplayMetatype),
                    new DependencyGraphNode<string, Contact>(nameof(Metatype),
                        new DependencyGraphNode<string, Contact>(nameof(LinkedCharacter))
                    )
                ),
                new DependencyGraphNode<string, Contact>(nameof(DisplayAge),
                    new DependencyGraphNode<string, Contact>(nameof(Age),
                        new DependencyGraphNode<string, Contact>(nameof(LinkedCharacter))
                    )
                ),
                new DependencyGraphNode<string, Contact>(nameof(MainMugshot),
                    new DependencyGraphNode<string, Contact>(nameof(LinkedCharacter)),
                    new DependencyGraphNode<string, Contact>(nameof(Mugshots),
                        new DependencyGraphNode<string, Contact>(nameof(LinkedCharacter))
                    ),
                    new DependencyGraphNode<string, Contact>(nameof(MainMugshotIndex))
                ),
                new DependencyGraphNode<string, Contact>(nameof(IsEnemy),
                    new DependencyGraphNode<string, Contact>(nameof(EntityType))
                ),
                new DependencyGraphNode<string, Contact>(nameof(GroupEnabled),
                    new DependencyGraphNode<string, Contact>(nameof(ReadOnly))
                ),
                new DependencyGraphNode<string, Contact>(nameof(LoyaltyEnabled),
                    new DependencyGraphNode<string, Contact>(nameof(IsGroup)),
                    new DependencyGraphNode<string, Contact>(
                        nameof(ForcedLoyalty)),
                    new DependencyGraphNode<string, Contact>(nameof(ReadOnly))
                ),
                new DependencyGraphNode<string, Contact>(nameof(ContactPoints),
                    new DependencyGraphNode<string, Contact>(nameof(Free)),
                    new DependencyGraphNode<string, Contact>(nameof(Connection),
                        new DependencyGraphNode<string, Contact>(nameof(ConnectionMaximum))
                    ),
                    new DependencyGraphNode<string, Contact>(nameof(Loyalty)),
                    new DependencyGraphNode<string, Contact>(nameof(Family)),
                    new DependencyGraphNode<string, Contact>(nameof(Blackmail))
                ),
                new DependencyGraphNode<string, Contact>(nameof(QuickText),
                    new DependencyGraphNode<string, Contact>(nameof(Connection)),
                    new DependencyGraphNode<string, Contact>(nameof(IsGroup)),
                    new DependencyGraphNode<string, Contact>(nameof(Loyalty),
                        new DependencyGraphNode<string, Contact>(nameof(IsGroup)),
                        new DependencyGraphNode<string, Contact>(nameof(ForcedLoyalty))
                    )
                )
            );

        #region Helper Methods

        /// <summary>
        /// Convert a string to a ContactType.
        /// </summary>
        /// <param name="strValue">String value to convert.</param>
        public static ContactType ConvertToContactType(string strValue)
        {
            if (string.IsNullOrEmpty(strValue))
                return default;
            switch (strValue)
            {
                case "Contact":
                    return ContactType.Contact;

                case "Pet":
                    return ContactType.Pet;

                default:
                    return ContactType.Enemy;
            }
        }

        #endregion Helper Methods

        #region Constructor, Save, Load, and Print Methods

        public Contact(Character objCharacter, bool blnIsReadOnly = false)
        {
            _objCharacter = objCharacter ?? throw new ArgumentNullException(nameof(objCharacter));
            LockObject = objCharacter.LockObject;
            _objCharacter.MultiplePropertiesChangedAsync += CharacterObjectOnPropertyChanged;
            _blnReadOnly = blnIsReadOnly;
            _lstMugshots = new ThreadSafeList<Image>(3, LockObject);
        }

        private Task CharacterObjectOnPropertyChanged(object sender, MultiplePropertiesChangedEventArgs e, CancellationToken token = default)
        {
            if (token.IsCancellationRequested)
                return Task.FromCanceled(token);
            return e.PropertyNames.Contains(nameof(Character.Created)) ||
                   e.PropertyNames.Contains(nameof(Character.FriendsInHighPlaces))
                ? OnPropertyChangedAsync(nameof(ConnectionMaximum), token)
                : Task.CompletedTask;
        }

        /// <summary>
        /// Save the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public void Save(XmlWriter objWriter, CancellationToken token = default)
        {
            Utils.SafelyRunSynchronously(() => SaveCoreAsync(true, objWriter, token), token);
        }

        /// <summary>
        /// Save the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public Task SaveAsync(XmlWriter objWriter, CancellationToken token = default)
        {
            return SaveCoreAsync(false, objWriter, token);
        }

        /// <summary>
        /// Save the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="blnSync"></param>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        private async Task SaveCoreAsync(bool blnSync, XmlWriter objWriter, CancellationToken token = default)
        {
            if (objWriter == null)
                return;
            if (blnSync)
            {
                // ReSharper disable MethodHasAsyncOverload
                using (LockObject.EnterReadLock(token))
                {
                    objWriter.WriteStartElement("contact");
                    objWriter.WriteElementString("name", _strName);
                    objWriter.WriteElementString("role", _strRole);
                    objWriter.WriteElementString("location", _strLocation);
                    objWriter.WriteElementString("connection",
                                                 _intConnection.ToString(GlobalSettings.InvariantCultureInfo));
                    objWriter.WriteElementString("loyalty", _intLoyalty.ToString(GlobalSettings.InvariantCultureInfo));
                    objWriter.WriteElementString("metatype", _strMetatype);
                    objWriter.WriteElementString("gender", _strGender);
                    objWriter.WriteElementString("age", _strAge);
                    objWriter.WriteElementString("contacttype", _strType);
                    objWriter.WriteElementString("preferredpayment", _strPreferredPayment);
                    objWriter.WriteElementString("hobbiesvice", _strHobbiesVice);
                    objWriter.WriteElementString("personallife", _strPersonalLife);
                    objWriter.WriteElementString("type", _eContactType.ToString());
                    objWriter.WriteElementString("file", _strFileName);
                    objWriter.WriteElementString("relative", _strRelativeName);
                    objWriter.WriteElementString("notes", _strNotes.CleanOfXmlInvalidUnicodeChars());
                    objWriter.WriteElementString("notesColor", ColorTranslator.ToHtml(_colNotes));
                    objWriter.WriteElementString("groupname", _strGroupName);
                    objWriter.WriteElementString(
                        "colour", _objColor.ToArgb().ToString(GlobalSettings.InvariantCultureInfo));
                    objWriter.WriteElementString("group", _blnIsGroup.ToString(GlobalSettings.InvariantCultureInfo));
                    objWriter.WriteElementString("family", _blnFamily.ToString(GlobalSettings.InvariantCultureInfo));
                    objWriter.WriteElementString("blackmail",
                                                 _blnBlackmail.ToString(GlobalSettings.InvariantCultureInfo));
                    objWriter.WriteElementString("free", _blnFree.ToString(GlobalSettings.InvariantCultureInfo));
                    objWriter.WriteElementString("groupenabled",
                                                 _blnGroupEnabled.ToString(GlobalSettings.InvariantCultureInfo));

                    if (_blnReadOnly)
                        objWriter.WriteElementString("readonly", string.Empty);

                    if (_strUnique != null)
                        objWriter.WriteElementString("guid", _strUnique);

                    SaveMugshots(objWriter, token);

                    objWriter.WriteEndElement();
                }
                // ReSharper restore MethodHasAsyncOverload
            }
            else
            {
                // <contact>
                IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    XmlElementWriteHelper objBaseElement
                        = await objWriter.StartElementAsync("contact", token: token).ConfigureAwait(false);
                    try
                    {
                        await objWriter.WriteElementStringAsync("name", _strName, token: token).ConfigureAwait(false);
                        await objWriter.WriteElementStringAsync("role", _strRole, token: token).ConfigureAwait(false);
                        await objWriter.WriteElementStringAsync("location", _strLocation, token: token)
                            .ConfigureAwait(false);
                        await objWriter.WriteElementStringAsync("connection",
                                _intConnection.ToString(
                                    GlobalSettings.InvariantCultureInfo), token: token)
                            .ConfigureAwait(false);
                        await objWriter.WriteElementStringAsync(
                                "loyalty", _intLoyalty.ToString(GlobalSettings.InvariantCultureInfo),
                                token: token)
                            .ConfigureAwait(false);
                        await objWriter.WriteElementStringAsync("metatype", _strMetatype, token: token)
                            .ConfigureAwait(false);
                        await objWriter.WriteElementStringAsync("gender", _strGender, token: token)
                            .ConfigureAwait(false);
                        await objWriter.WriteElementStringAsync("age", _strAge, token: token).ConfigureAwait(false);
                        await objWriter.WriteElementStringAsync("contacttype", _strType, token: token)
                            .ConfigureAwait(false);
                        await objWriter.WriteElementStringAsync("preferredpayment", _strPreferredPayment, token: token)
                            .ConfigureAwait(false);
                        await objWriter.WriteElementStringAsync("hobbiesvice", _strHobbiesVice, token: token)
                            .ConfigureAwait(false);
                        await objWriter.WriteElementStringAsync("personallife", _strPersonalLife, token: token)
                            .ConfigureAwait(false);
                        await objWriter.WriteElementStringAsync("type", _eContactType.ToString(), token: token)
                            .ConfigureAwait(false);
                        await objWriter.WriteElementStringAsync("file", _strFileName, token: token)
                            .ConfigureAwait(false);
                        await objWriter.WriteElementStringAsync("relative", _strRelativeName, token: token)
                            .ConfigureAwait(false);
                        await objWriter
                            .WriteElementStringAsync("notes", _strNotes.CleanOfXmlInvalidUnicodeChars(), token: token)
                            .ConfigureAwait(false);
                        await objWriter
                            .WriteElementStringAsync("notesColor", ColorTranslator.ToHtml(_colNotes), token: token)
                            .ConfigureAwait(false);
                        await objWriter.WriteElementStringAsync("groupname", _strGroupName, token: token)
                            .ConfigureAwait(false);
                        await objWriter.WriteElementStringAsync(
                                "colour", _objColor.ToArgb().ToString(GlobalSettings.InvariantCultureInfo),
                                token: token)
                            .ConfigureAwait(false);
                        await objWriter.WriteElementStringAsync(
                                "group", _blnIsGroup.ToString(GlobalSettings.InvariantCultureInfo),
                                token: token)
                            .ConfigureAwait(false);
                        await objWriter.WriteElementStringAsync(
                                "family", _blnFamily.ToString(GlobalSettings.InvariantCultureInfo),
                                token: token)
                            .ConfigureAwait(false);
                        await objWriter.WriteElementStringAsync("blackmail",
                                _blnBlackmail.ToString(
                                    GlobalSettings.InvariantCultureInfo), token: token)
                            .ConfigureAwait(false);
                        await objWriter.WriteElementStringAsync(
                                "free", _blnFree.ToString(GlobalSettings.InvariantCultureInfo), token: token)
                            .ConfigureAwait(false);
                        await objWriter.WriteElementStringAsync("groupenabled",
                                _blnGroupEnabled.ToString(
                                    GlobalSettings.InvariantCultureInfo), token: token)
                            .ConfigureAwait(false);

                        if (_blnReadOnly)
                            await objWriter.WriteElementStringAsync("readonly", string.Empty, token: token)
                                .ConfigureAwait(false);

                        if (_strUnique != null)
                        {
                            await objWriter.WriteElementStringAsync("guid", _strUnique, token: token)
                                .ConfigureAwait(false);
                        }

                        await SaveMugshotsAsync(objWriter, token).ConfigureAwait(false);
                    }
                    finally
                    {
                        // </contact>
                        await objBaseElement.DisposeAsync().ConfigureAwait(false);
                    }
                }
                finally
                {
                    await objLocker.DisposeAsync().ConfigureAwait(false);
                }
            }
        }

        /// <summary>
        /// Load the Contact from the XmlNode.
        /// </summary>
        /// <param name="objNode">XmlNode to load.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public void Load(XPathNavigator objNode, CancellationToken token = default)
        {
            using (LockObject.EnterWriteLock(token))
            {
                objNode.TryGetStringFieldQuickly("name", ref _strName);
                objNode.TryGetStringFieldQuickly("role", ref _strRole);
                objNode.TryGetStringFieldQuickly("location", ref _strLocation);
                objNode.TryGetInt32FieldQuickly("connection", ref _intConnection);
                objNode.TryGetInt32FieldQuickly("loyalty", ref _intLoyalty);
                objNode.TryGetStringFieldQuickly("metatype", ref _strMetatype);
                if (!objNode.TryGetStringFieldQuickly("gender", ref _strGender))
                    objNode.TryGetStringFieldQuickly("sex", ref _strGender);
                objNode.TryGetStringFieldQuickly("age", ref _strAge);
                objNode.TryGetStringFieldQuickly("contacttype", ref _strType);
                objNode.TryGetStringFieldQuickly("preferredpayment", ref _strPreferredPayment);
                objNode.TryGetStringFieldQuickly("hobbiesvice", ref _strHobbiesVice);
                objNode.TryGetStringFieldQuickly("personallife", ref _strPersonalLife);
                string strTemp = string.Empty;
                if (objNode.TryGetStringFieldQuickly("type", ref strTemp))
                    _eContactType = ConvertToContactType(strTemp);
                objNode.TryGetStringFieldQuickly("file", ref _strFileName);
                objNode.TryGetStringFieldQuickly("notes", ref _strNotes);

                string sNotesColor = ColorTranslator.ToHtml(ColorManager.HasNotesColor);
                objNode.TryGetStringFieldQuickly("notesColor", ref sNotesColor);
                _colNotes = ColorTranslator.FromHtml(sNotesColor);

                objNode.TryGetStringFieldQuickly("groupname", ref _strGroupName);
                objNode.TryGetBoolFieldQuickly("group", ref _blnIsGroup);
                objNode.TryGetStringFieldQuickly("guid", ref _strUnique);
                objNode.TryGetBoolFieldQuickly("family", ref _blnFamily);
                objNode.TryGetBoolFieldQuickly("blackmail", ref _blnBlackmail);
                objNode.TryGetBoolFieldQuickly("free", ref _blnFree);
                if (objNode.SelectSingleNodeAndCacheExpression("colour", token) != null)
                {
                    int intTmp = _objColor.ToArgb();
                    if (objNode.TryGetInt32FieldQuickly("colour", ref intTmp))
                        _objColor = Color.FromArgb(intTmp);
                }

                _blnReadOnly = objNode.SelectSingleNodeAndCacheExpression("readonly", token) != null;

                if (!objNode.TryGetBoolFieldQuickly("groupenabled", ref _blnGroupEnabled))
                {
                    objNode.TryGetBoolFieldQuickly("mademan", ref _blnGroupEnabled);
                }

                RefreshLinkedCharacter(token: token);

                // Mugshots
                LoadMugshots(objNode, token);
            }
        }

        /// <summary>
        /// Load the Contact from the XmlNode.
        /// </summary>
        /// <param name="objNode">XmlNode to load.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public async Task LoadAsync(XPathNavigator objNode, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                objNode.TryGetStringFieldQuickly("name", ref _strName);
                objNode.TryGetStringFieldQuickly("role", ref _strRole);
                objNode.TryGetStringFieldQuickly("location", ref _strLocation);
                objNode.TryGetInt32FieldQuickly("connection", ref _intConnection);
                objNode.TryGetInt32FieldQuickly("loyalty", ref _intLoyalty);
                objNode.TryGetStringFieldQuickly("metatype", ref _strMetatype);
                if (!objNode.TryGetStringFieldQuickly("gender", ref _strGender))
                    objNode.TryGetStringFieldQuickly("sex", ref _strGender);
                objNode.TryGetStringFieldQuickly("age", ref _strAge);
                objNode.TryGetStringFieldQuickly("contacttype", ref _strType);
                objNode.TryGetStringFieldQuickly("preferredpayment", ref _strPreferredPayment);
                objNode.TryGetStringFieldQuickly("hobbiesvice", ref _strHobbiesVice);
                objNode.TryGetStringFieldQuickly("personallife", ref _strPersonalLife);
                string strTemp = string.Empty;
                if (objNode.TryGetStringFieldQuickly("type", ref strTemp))
                    _eContactType = ConvertToContactType(strTemp);
                objNode.TryGetStringFieldQuickly("file", ref _strFileName);
                objNode.TryGetStringFieldQuickly("notes", ref _strNotes);

                string strNotesColor = ColorTranslator.ToHtml(ColorManager.HasNotesColor);
                objNode.TryGetStringFieldQuickly("notesColor", ref strNotesColor);
                _colNotes = ColorTranslator.FromHtml(strNotesColor);

                objNode.TryGetStringFieldQuickly("groupname", ref _strGroupName);
                objNode.TryGetBoolFieldQuickly("group", ref _blnIsGroup);
                objNode.TryGetStringFieldQuickly("guid", ref _strUnique);
                objNode.TryGetBoolFieldQuickly("family", ref _blnFamily);
                objNode.TryGetBoolFieldQuickly("blackmail", ref _blnBlackmail);
                objNode.TryGetBoolFieldQuickly("free", ref _blnFree);
                if (objNode.SelectSingleNodeAndCacheExpression("colour", token) != null)
                {
                    int intTmp = _objColor.ToArgb();
                    if (objNode.TryGetInt32FieldQuickly("colour", ref intTmp))
                        _objColor = Color.FromArgb(intTmp);
                }

                _blnReadOnly = objNode.SelectSingleNodeAndCacheExpression("readonly", token) != null;

                if (!objNode.TryGetBoolFieldQuickly("groupenabled", ref _blnGroupEnabled))
                {
                    objNode.TryGetBoolFieldQuickly("mademan", ref _blnGroupEnabled);
                }

                await RefreshLinkedCharacterAsync(token: token).ConfigureAwait(false);

                // Mugshots
                await LoadMugshotsAsync(objNode, token).ConfigureAwait(false);
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
        /// <param name="objCulture">Culture in which to print.</param>
        /// <param name="strLanguageToPrint">Language in which to print</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public async Task Print(XmlWriter objWriter, CultureInfo objCulture, string strLanguageToPrint,
                                     CancellationToken token = default)
        {
            if (objWriter == null)
                return;
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                // <contact>
                XmlElementWriteHelper objBaseElement
                    = await objWriter.StartElementAsync("contact", token: token).ConfigureAwait(false);
                try
                {
                    await objWriter.WriteElementStringAsync("guid", InternalId, token: token).ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("name", Name, token: token).ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync(
                        "role", await DisplayRoleMethodAsync(strLanguageToPrint, token).ConfigureAwait(false),
                        token: token).ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("location", Location, token: token).ConfigureAwait(false);
                    if (!IsGroup)
                        await objWriter.WriteElementStringAsync("connection",
                            (await GetConnectionAsync(token).ConfigureAwait(false))
                            .ToString(objCulture),
                            token: token).ConfigureAwait(false);
                    else
                        await objWriter.WriteElementStringAsync("connection",
                            await LanguageManager.GetStringAsync(
                                    "String_Group", strLanguageToPrint,
                                    token: token)
                                .ConfigureAwait(false) + '('
                                                       + (await GetConnectionAsync(token)
                                                           .ConfigureAwait(false))
                                                       .ToString(objCulture)
                                                       + ')', token: token).ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync(
                        "loyalty", (await GetLoyaltyAsync(token).ConfigureAwait(false)).ToString(objCulture),
                        token: token).ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync(
                        "metatype", await DisplayMetatypeMethodAsync(strLanguageToPrint, token).ConfigureAwait(false),
                        token: token).ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync(
                        "gender", await DisplayGenderMethodAsync(strLanguageToPrint, token).ConfigureAwait(false),
                        token: token).ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync(
                        "age", await DisplayAgeMethodAsync(strLanguageToPrint, token).ConfigureAwait(false),
                        token: token).ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("contacttype",
                        await DisplayTypeMethodAsync(strLanguageToPrint, token)
                            .ConfigureAwait(false),
                        token: token).ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("preferredpayment",
                        await DisplayPreferredPaymentMethodAsync(
                            strLanguageToPrint, token).ConfigureAwait(false),
                        token: token).ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("hobbiesvice",
                        await DisplayHobbiesViceMethodAsync(
                                strLanguageToPrint, token)
                            .ConfigureAwait(false),
                        token: token).ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("personallife",
                        await DisplayPersonalLifeMethodAsync(
                                strLanguageToPrint, token)
                            .ConfigureAwait(false),
                        token: token).ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync(
                        "type",
                        await LanguageManager.GetStringAsync("String_" + EntityType, strLanguageToPrint, token: token)
                            .ConfigureAwait(false),
                        token: token).ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("forcedloyalty",
                        (await GetForcedLoyaltyAsync(token).ConfigureAwait(false))
                        .ToString(objCulture),
                        token: token).ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("blackmail",
                        Blackmail.ToString(GlobalSettings.InvariantCultureInfo),
                        token: token).ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync(
                        "family", Family.ToString(GlobalSettings.InvariantCultureInfo),
                        token: token).ConfigureAwait(false);
                    if (GlobalSettings.PrintNotes)
                        await objWriter.WriteElementStringAsync("notes", await GetNotesAsync(token).ConfigureAwait(false), token: token).ConfigureAwait(false);

                    await PrintMugshots(objWriter, token).ConfigureAwait(false);
                }
                finally
                {
                    // </contact>
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

        public bool ReadOnly
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _blnReadOnly;
            }
        }

        public async Task<bool> GetReadOnlyAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _blnReadOnly;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Total points used for this contact.
        /// </summary>
        public int ContactPoints
        {
            get
            {
                using (LockObject.EnterReadLock())
                {
                    if (Free)
                        return 0;
                    decimal decReturn = Connection + Loyalty;
                    if (Family)
                        ++decReturn;
                    if (Blackmail)
                        decReturn += 2;
                    decReturn +=
                        ImprovementManager.ValueOf(_objCharacter, Improvement.ImprovementType.ContactKarmaDiscount);
                    decReturn = Math.Max(
                        decReturn,
                        2 + ImprovementManager.ValueOf(_objCharacter, Improvement.ImprovementType.ContactKarmaMinimum));
                    return decReturn.StandardRound();
                }
            }
        }

        /// <summary>
        /// Total points used for this contact.
        /// </summary>
        public async Task<int> GetContactPointsAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (await GetFreeAsync(token).ConfigureAwait(false))
                    return 0;
                decimal decReturn = await GetConnectionAsync(token).ConfigureAwait(false) +
                                    await GetLoyaltyAsync(token).ConfigureAwait(false);
                if (await GetFamilyAsync(token).ConfigureAwait(false))
                    ++decReturn;
                if (await GetBlackmailAsync(token).ConfigureAwait(false))
                    decReturn += 2;
                decReturn +=
                    await ImprovementManager
                        .ValueOfAsync(_objCharacter, Improvement.ImprovementType.ContactKarmaDiscount, token: token)
                        .ConfigureAwait(false);
                decReturn = Math.Max(
                    decReturn,
                    2 + await ImprovementManager
                        .ValueOfAsync(_objCharacter, Improvement.ImprovementType.ContactKarmaMinimum, token: token)
                        .ConfigureAwait(false));
                return decReturn.StandardRound();
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Name of the Contact.
        /// </summary>
        public string Name
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return LinkedCharacter != null ? LinkedCharacter.CharacterName : _strName;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (Interlocked.Exchange(ref _strName, value) != value)
                        OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Name of the Contact.
        /// </summary>
        public async Task<string> GetNameAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                Character objLinkedCharacter = await GetLinkedCharacterAsync(token).ConfigureAwait(false);
                return objLinkedCharacter != null
                    ? await objLinkedCharacter.GetCharacterNameAsync(token).ConfigureAwait(false)
                    : _strName;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Name of the Contact.
        /// </summary>
        public async Task SetNameAsync(string value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (Interlocked.Exchange(ref _strName, value) != value)
                    await OnPropertyChangedAsync(nameof(Name), token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public string CurrentDisplayName => Name;

        public Task<string> GetCurrentDisplayNameAsync(CancellationToken token = default) =>
            GetNameAsync(token);

        public string DisplayRoleMethod(string strLanguage)
        {
            string strRole = Role;
            if (strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                return strRole;

            return _objCharacter.LoadDataXPath("contacts.xml", strLanguage)
                                .SelectSingleNode("/chummer/contacts/contact[. = " + strRole.CleanXPath() + "]/@translate")
                                ?.Value ?? strRole;
        }

        public async Task<string> DisplayRoleMethodAsync(string strLanguage, CancellationToken token = default)
        {
            string strRole = Role;
            if (strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                return strRole;

            return (await _objCharacter.LoadDataXPathAsync("contacts.xml", strLanguage, token: token)
                                       .ConfigureAwait(false))
                   .SelectSingleNode("/chummer/contacts/contact[. = " + strRole.CleanXPath() + "]/@translate")
                   ?.Value ?? strRole;
        }

        public string DisplayRole
        {
            get => DisplayRoleMethod(GlobalSettings.Language);
            set => Role = _objCharacter.ReverseTranslateExtra(value, GlobalSettings.Language, "contacts.xml");
        }

        public Task<string> GetDisplayRoleAsync(CancellationToken token = default) =>
            DisplayRoleMethodAsync(GlobalSettings.Language, token);

        public async Task SetDisplayRoleAsync(string value, CancellationToken token = default)
        {
            Role = await _objCharacter.ReverseTranslateExtraAsync(value, GlobalSettings.Language, "contacts.xml",
                                                                  token).ConfigureAwait(false);
        }

        /// <summary>
        /// Role of the Contact.
        /// </summary>
        public string Role
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _strRole;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (Interlocked.Exchange(ref _strRole, value) != value)
                        OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Location of the Contact.
        /// </summary>
        public string Location
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _strLocation;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (Interlocked.Exchange(ref _strLocation, value) != value)
                        OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Location of the Contact.
        /// </summary>
        public async Task<string> GetLocationAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _strLocation;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Location of the Contact.
        /// </summary>
        public async Task SetLocationAsync(string value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (Interlocked.Exchange(ref _strLocation, value) != value)
                    await OnPropertyChangedAsync(nameof(Location), token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Contact's Connection Rating.
        /// </summary>
        public int Connection
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return Math.Min(_intConnection, ConnectionMaximum);
            }
            set
            {
                value = Math.Max(Math.Min(value, ConnectionMaximum), 1);
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (Interlocked.Exchange(ref _intConnection, value) != value)
                        OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Contact's Connection Rating.
        /// </summary>
        public async Task<int> GetConnectionAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return Math.Min(_intConnection, await GetConnectionMaximumAsync(token).ConfigureAwait(false));
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Contact's Connection Rating.
        /// </summary>
        public async Task SetConnectionAsync(int value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            value = Math.Max(Math.Min(value, await GetConnectionMaximumAsync(token).ConfigureAwait(false)), 1);
            IAsyncDisposable objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (Interlocked.Exchange(ref _intConnection, value) != value)
                    await OnPropertyChangedAsync(nameof(Connection), token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Contact's Loyalty Rating (or Enemy's Incidence Rating).
        /// </summary>
        public int Loyalty
        {
            get
            {
                using (LockObject.EnterReadLock())
                {
                    if (ForcedLoyalty > 0)
                        return ForcedLoyalty;
                    return IsGroup ? 1 : _intLoyalty;
                }
            }
            set
            {
                value = Math.Max(value, 1);
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (Interlocked.Exchange(ref _intLoyalty, value) != value)
                        OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Contact's Loyalty Rating (or Enemy's Incidence Rating).
        /// </summary>
        public async Task<int> GetLoyaltyAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                int intForced = await GetForcedLoyaltyAsync(token).ConfigureAwait(false);
                if (intForced > 0)
                    return intForced;
                return await GetIsGroupAsync(token).ConfigureAwait(false) ? 1 : _intLoyalty;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Contact's Loyalty Rating (or Enemy's Incidence Rating).
        /// </summary>
        public async Task SetLoyaltyAsync(int value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            value = Math.Max(value, 1);
            IAsyncDisposable objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (Interlocked.Exchange(ref _intLoyalty, value) != value)
                    await OnPropertyChangedAsync(nameof(Loyalty), token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public string DisplayMetatypeMethod(string strLanguage)
        {
            using (LockObject.EnterReadLock())
            {
                string strReturn;
                if (LinkedCharacter != null)
                {
                    // Update character information fields.
                    XPathNavigator objMetatypeNode = _objCharacter.GetNodeXPath(true);

                    strReturn = objMetatypeNode.SelectSingleNodeAndCacheExpression("translate")?.Value
                                ?? _objCharacter.TranslateExtra(LinkedCharacter.Metatype, strLanguage, "metatypes.xml");

                    if (LinkedCharacter.MetavariantGuid == Guid.Empty)
                        return strReturn;
                    objMetatypeNode
                        = objMetatypeNode.TryGetNodeById("metavariants/metavariant", LinkedCharacter.MetavariantGuid);

                    string strMetatypeTranslate
                        = objMetatypeNode?.SelectSingleNodeAndCacheExpression("translate")?.Value;
                    strReturn += LanguageManager.GetString("String_Space", strLanguage)
                                 + '('
                                 + (!string.IsNullOrEmpty(strMetatypeTranslate)
                                     ? strMetatypeTranslate
                                     : _objCharacter.TranslateExtra(LinkedCharacter.Metavariant, strLanguage,
                                                                    "metatypes.xml"))
                                 + ')';
                }
                else
                    strReturn = _objCharacter.TranslateExtra(Metatype, strLanguage, "metatypes.xml");

                return strReturn;
            }
        }

        public async Task<string> DisplayMetatypeMethodAsync(string strLanguage, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                string strReturn;
                Character objLinkedCharacter = await GetLinkedCharacterAsync(token).ConfigureAwait(false);
                if (objLinkedCharacter != null)
                {
                    // Update character information fields.
                    XPathNavigator objMetatypeNode
                        = await _objCharacter.GetNodeXPathAsync(true, token: token).ConfigureAwait(false);

                    strReturn
                        = objMetatypeNode.SelectSingleNodeAndCacheExpression("translate", token: token)?.Value
                          ?? await _objCharacter.TranslateExtraAsync(
                              await objLinkedCharacter.GetMetatypeAsync(token).ConfigureAwait(false),
                              strLanguage, "metatypes.xml", token).ConfigureAwait(false);

                    Guid guiMetavariant = await objLinkedCharacter.GetMetavariantGuidAsync(token).ConfigureAwait(false);
                    if (guiMetavariant == Guid.Empty)
                        return strReturn;
                    objMetatypeNode
                        = objMetatypeNode.TryGetNodeById("metavariants/metavariant",
                            await objLinkedCharacter.GetMetavariantGuidAsync(token).ConfigureAwait(false));

                    string strMetatypeTranslate = objMetatypeNode
                        ?.SelectSingleNodeAndCacheExpression("translate", token: token)?.Value;
                    strReturn += await LanguageManager.GetStringAsync("String_Space", strLanguage, token: token)
                                     .ConfigureAwait(false)
                                 + '('
                                 + (!string.IsNullOrEmpty(strMetatypeTranslate)
                                     ? strMetatypeTranslate
                                     : await _objCharacter.TranslateExtraAsync(
                                         await objLinkedCharacter.GetMetavariantAsync(token).ConfigureAwait(false),
                                         strLanguage, "metatypes.xml",
                                         token).ConfigureAwait(false))
                                 + ')';
                }
                else
                    strReturn = await _objCharacter.TranslateExtraAsync(
                            await GetMetatypeAsync(token).ConfigureAwait(false), strLanguage, "metatypes.xml", token)
                        .ConfigureAwait(false);

                return strReturn;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public string DisplayMetatype
        {
            get => DisplayMetatypeMethod(GlobalSettings.Language);
            set => Metatype = _objCharacter.ReverseTranslateExtra(value, GlobalSettings.Language, "contacts.xml");
        }

        public Task<string> GetDisplayMetatypeAsync(CancellationToken token = default) =>
            DisplayMetatypeMethodAsync(GlobalSettings.Language, token);

        public async Task SetDisplayMetatypeAsync(string value, CancellationToken token = default)
        {
            await SetMetatypeAsync(await _objCharacter.ReverseTranslateExtraAsync(value, GlobalSettings.Language, "contacts.xml",
                                                                      token).ConfigureAwait(false), token).ConfigureAwait(false);
        }

        /// <summary>
        /// Metatype of this Contact.
        /// </summary>
        public string Metatype
        {
            get
            {
                using (LockObject.EnterReadLock())
                {
                    if (LinkedCharacter != null)
                    {
                        string strMetatype = LinkedCharacter.Metatype;
                        if (!string.IsNullOrEmpty(LinkedCharacter.Metavariant))
                        {
                            strMetatype += LanguageManager.GetString("String_Space") + '(' + LinkedCharacter.Metavariant
                                           + ')';
                        }

                        return strMetatype;
                    }

                    return _strMetatype;
                }
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (Interlocked.Exchange(ref _strMetatype, value) != value)
                        OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Metatype of this Contact.
        /// </summary>
        public async Task<string> GetMetatypeAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                Character objLinkedCharacter = await GetLinkedCharacterAsync(token).ConfigureAwait(false);
                if (objLinkedCharacter != null)
                {
                    string strMetatype = await objLinkedCharacter.GetMetatypeAsync(token).ConfigureAwait(false);
                    string strMetavariant = await objLinkedCharacter.GetMetavariantAsync(token).ConfigureAwait(false);

                    if (!string.IsNullOrEmpty(strMetavariant))
                    {
                        strMetatype += await LanguageManager.GetStringAsync("String_Space", token: token)
                                                            .ConfigureAwait(false) + '('
                            + strMetavariant + ')';
                    }

                    return strMetatype;
                }

                return _strMetatype;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Metatype of this Contact.
        /// </summary>
        public async Task SetMetatypeAsync(string value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (Interlocked.Exchange(ref _strMetatype, value) != value)
                    await OnPropertyChangedAsync(nameof(Metatype), token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public string DisplayGenderMethod(string strLanguage)
        {
            string strGender = Gender;
            if (strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                return strGender;

            return _objCharacter.LoadDataXPath("contacts.xml", strLanguage)
                                .SelectSingleNode("/chummer/genders/gender[. = " + strGender.CleanXPath() + "]/@translate")
                                ?.Value ?? strGender;
        }

        public async Task<string> DisplayGenderMethodAsync(string strLanguage, CancellationToken token = default)
        {
            string strGender = await GetGenderAsync(token).ConfigureAwait(false);
            if (strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                return strGender;

            return (await _objCharacter.LoadDataXPathAsync("contacts.xml", strLanguage, token: token)
                                       .ConfigureAwait(false))
                   .SelectSingleNode("/chummer/genders/gender[. = " + strGender.CleanXPath() + "]/@translate")?.Value
                   ?? strGender;
        }

        public string DisplayGender
        {
            get => DisplayGenderMethod(GlobalSettings.Language);
            set => Gender = _objCharacter.ReverseTranslateExtra(value, GlobalSettings.Language, "contacts.xml");
        }

        public Task<string> GetDisplayGenderAsync(CancellationToken token = default) =>
            DisplayGenderMethodAsync(GlobalSettings.Language, token);

        public async Task SetDisplayGenderAsync(string value, CancellationToken token = default)
        {
            await SetGenderAsync(await _objCharacter.ReverseTranslateExtraAsync(value, GlobalSettings.Language, "contacts.xml",
                                                                    token).ConfigureAwait(false), token).ConfigureAwait(false);
        }

        /// <summary>
        /// Gender of this Contact.
        /// </summary>
        public string Gender
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return LinkedCharacter != null ? LinkedCharacter.Gender : _strGender;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (Interlocked.Exchange(ref _strGender, value) != value)
                        OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gender of this Contact.
        /// </summary>
        public async Task<string> GetGenderAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                Character objLinkedCharacter = await GetLinkedCharacterAsync(token).ConfigureAwait(false);
                return objLinkedCharacter != null
                    ? await objLinkedCharacter.GetGenderAsync(token).ConfigureAwait(false)
                    : _strGender;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Gender of this Contact.
        /// </summary>
        public async Task SetGenderAsync(string value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (Interlocked.Exchange(ref _strGender, value) != value)
                    await OnPropertyChangedAsync(nameof(Gender), token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public string DisplayAgeMethod(string strLanguage)
        {
            string strAge = Age;
            if (strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                return strAge;

            return _objCharacter.LoadDataXPath("contacts.xml", strLanguage)
                                .SelectSingleNode("/chummer/ages/age[. = " + strAge.CleanXPath() + "]/@translate")?.Value
                   ?? strAge;
        }

        public async Task<string> DisplayAgeMethodAsync(string strLanguage, CancellationToken token = default)
        {
            string strAge = await GetAgeAsync(token).ConfigureAwait(false);
            if (strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                return strAge;

            return (await _objCharacter.LoadDataXPathAsync("contacts.xml", strLanguage, token: token)
                                       .ConfigureAwait(false))
                   .SelectSingleNode("/chummer/ages/age[. = " + strAge.CleanXPath() + "]/@translate")?.Value ?? strAge;
        }

        public string DisplayAge
        {
            get => DisplayAgeMethod(GlobalSettings.Language);
            set => Age = _objCharacter.ReverseTranslateExtra(value, GlobalSettings.Language, "contacts.xml");
        }

        public Task<string> GetDisplayAgeAsync(CancellationToken token = default) =>
            DisplayAgeMethodAsync(GlobalSettings.Language, token);

        public async Task SetDisplayAgeAsync(string value, CancellationToken token = default)
        {
            await SetAgeAsync(await _objCharacter.ReverseTranslateExtraAsync(value, GlobalSettings.Language, "contacts.xml", token)
                                     .ConfigureAwait(false), token).ConfigureAwait(false);
        }

        /// <summary>
        /// How old is this Contact.
        /// </summary>
        public string Age
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return LinkedCharacter != null ? LinkedCharacter.Age : _strAge;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (Interlocked.Exchange(ref _strAge, value) != value)
                        OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// How old is this Contact.
        /// </summary>
        public async Task<string> GetAgeAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                Character objLinkedCharacter = await GetLinkedCharacterAsync(token).ConfigureAwait(false);
                return objLinkedCharacter != null
                    ? await objLinkedCharacter.GetAgeAsync(token).ConfigureAwait(false)
                    : _strAge;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// How old is this Contact.
        /// </summary>
        public async Task SetAgeAsync(string value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (Interlocked.Exchange(ref _strAge, value) != value)
                    await OnPropertyChangedAsync(nameof(Age), token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public string DisplayTypeMethod(string strLanguage)
        {
            string strType = Type;
            if (strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                return strType;

            return _objCharacter.LoadDataXPath("contacts.xml", strLanguage)
                                .SelectSingleNode("/chummer/types/type[. = " + strType.CleanXPath() + "]/@translate")
                                ?.Value ?? strType;
        }

        public async Task<string> DisplayTypeMethodAsync(string strLanguage, CancellationToken token = default)
        {
            string strType = Type;
            if (strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                return strType;

            return (await _objCharacter.LoadDataXPathAsync("contacts.xml", strLanguage, token: token)
                                       .ConfigureAwait(false))
                   .SelectSingleNode("/chummer/types/type[. = " + strType.CleanXPath() + "]/@translate")?.Value ?? strType;
        }

        public string DisplayType
        {
            get => DisplayTypeMethod(GlobalSettings.Language);
            set => Type = _objCharacter.ReverseTranslateExtra(value, GlobalSettings.Language, "contacts.xml");
        }

        public Task<string> GetDisplayTypeAsync(CancellationToken token = default) =>
            DisplayTypeMethodAsync(GlobalSettings.Language, token);

        public async Task SetDisplayTypeAsync(string value, CancellationToken token = default)
        {
            Type = await _objCharacter.ReverseTranslateExtraAsync(value, GlobalSettings.Language, "contacts.xml", token)
                                      .ConfigureAwait(false);
        }

        /// <summary>
        /// What type of Contact is this.
        /// </summary>
        public string Type
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _strType;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (Interlocked.Exchange(ref _strType, value) != value)
                        OnPropertyChanged();
                }
            }
        }

        public string DisplayPreferredPaymentMethod(string strLanguage)
        {
            string strPreferredPayment = PreferredPayment;
            if (strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                return strPreferredPayment;

            return _objCharacter.LoadDataXPath("contacts.xml", strLanguage)
                                .SelectSingleNode("/chummer/preferredpayments/preferredpayment[. = "
                                                  + strPreferredPayment.CleanXPath() + "]/@translate")?.Value
                   ?? strPreferredPayment;
        }

        public async Task<string> DisplayPreferredPaymentMethodAsync(string strLanguage,
                                                                     CancellationToken token = default)
        {
            string strPreferredPayment = PreferredPayment;
            if (strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                return strPreferredPayment;

            return (await _objCharacter.LoadDataXPathAsync("contacts.xml", strLanguage, token: token)
                                       .ConfigureAwait(false))
                   .SelectSingleNode("/chummer/preferredpayments/preferredpayment[. = " + strPreferredPayment.CleanXPath()
                                     + "]/@translate")?.Value ?? strPreferredPayment;
        }

        public string DisplayPreferredPayment
        {
            get => DisplayPreferredPaymentMethod(GlobalSettings.Language);
            set => PreferredPayment
                = _objCharacter.ReverseTranslateExtra(value, GlobalSettings.Language, "contacts.xml");
        }

        public Task<string> GetDisplayPreferredPaymentAsync(CancellationToken token = default) =>
            DisplayPreferredPaymentMethodAsync(GlobalSettings.Language, token);

        public async Task SetDisplayPreferredPaymentAsync(string value, CancellationToken token = default)
        {
            PreferredPayment = await _objCharacter
                                     .ReverseTranslateExtraAsync(value, GlobalSettings.Language, "contacts.xml", token)
                                     .ConfigureAwait(false);
        }

        /// <summary>
        /// Preferred payment method of this Contact.
        /// </summary>
        public string PreferredPayment
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _strPreferredPayment;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (Interlocked.Exchange(ref _strPreferredPayment, value) != value)
                        OnPropertyChanged();
                }
            }
        }

        public string DisplayHobbiesViceMethod(string strLanguage)
        {
            string strHobbiesVice = HobbiesVice;
            if (strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                return strHobbiesVice;

            return _objCharacter.LoadDataXPath("contacts.xml", strLanguage)
                                .SelectSingleNode("/chummer/hobbiesvices/hobbyvice[. = " + strHobbiesVice.CleanXPath()
                                                  + "]/@translate")?.Value
                   ?? strHobbiesVice;
        }

        public async Task<string> DisplayHobbiesViceMethodAsync(string strLanguage, CancellationToken token = default)
        {
            string strHobbiesVice = HobbiesVice;
            if (strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                return strHobbiesVice;

            return (await _objCharacter.LoadDataXPathAsync("contacts.xml", strLanguage, token: token)
                                       .ConfigureAwait(false))
                   .SelectSingleNode("/chummer/hobbiesvices/hobbyvice[. = " + strHobbiesVice.CleanXPath() + "]/@translate")
                   ?.Value
                   ?? strHobbiesVice;
        }

        public string DisplayHobbiesVice
        {
            get => DisplayHobbiesViceMethod(GlobalSettings.Language);
            set => HobbiesVice = _objCharacter.ReverseTranslateExtra(value, GlobalSettings.Language, "contacts.xml");
        }

        public Task<string> GetDisplayHobbiesViceAsync(CancellationToken token = default) =>
            DisplayHobbiesViceMethodAsync(GlobalSettings.Language, token);

        public async Task SetDisplayHobbiesViceAsync(string value, CancellationToken token = default)
        {
            HobbiesVice = await _objCharacter
                                .ReverseTranslateExtraAsync(value, GlobalSettings.Language, "contacts.xml", token)
                                .ConfigureAwait(false);
        }

        /// <summary>
        /// Hobbies/Vice of this Contact.
        /// </summary>
        public string HobbiesVice
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _strHobbiesVice;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (Interlocked.Exchange(ref _strHobbiesVice, value) != value)
                        OnPropertyChanged();
                }
            }
        }

        public string DisplayPersonalLifeMethod(string strLanguage)
        {
            string strPersonalLife = PersonalLife;
            if (strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                return strPersonalLife;

            return _objCharacter.LoadDataXPath("contacts.xml", strLanguage)
                                .SelectSingleNode("/chummer/personallives/personallife[. = " + strPersonalLife.CleanXPath()
                                                  + "]/@translate")?.Value
                   ?? strPersonalLife;
        }

        public async Task<string> DisplayPersonalLifeMethodAsync(string strLanguage, CancellationToken token = default)
        {
            string strPersonalLife = PersonalLife;
            if (strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                return strPersonalLife;

            return (await _objCharacter.LoadDataXPathAsync("contacts.xml", strLanguage, token: token)
                                       .ConfigureAwait(false))
                   .SelectSingleNode("/chummer/personallives/personallife[. = " + strPersonalLife.CleanXPath()
                                     + "]/@translate")?.Value
                   ?? strPersonalLife;
        }

        public string DisplayPersonalLife
        {
            get => DisplayPersonalLifeMethod(GlobalSettings.Language);
            set => PersonalLife = _objCharacter.ReverseTranslateExtra(value, GlobalSettings.Language, "contacts.xml");
        }

        public Task<string> GetDisplayPersonalLifeAsync(CancellationToken token = default) =>
            DisplayPersonalLifeMethodAsync(GlobalSettings.Language, token);

        public async Task SetDisplayPersonalLifeAsync(string value, CancellationToken token = default)
        {
            PersonalLife = await _objCharacter
                                 .ReverseTranslateExtraAsync(value, GlobalSettings.Language, "contacts.xml", token)
                                 .ConfigureAwait(false);
        }

        /// <summary>
        /// Personal Life of this Contact.
        /// </summary>
        public string PersonalLife
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _strPersonalLife;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (Interlocked.Exchange(ref _strPersonalLife, value) != value)
                        OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Is this contact a group contact?
        /// </summary>
        public bool IsGroup
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _blnIsGroup;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (_blnIsGroup == value)
                        return;
                    using (LockObject.EnterWriteLock())
                    {
                        _blnIsGroup = value;
                        OnPropertyChanged();
                    }
                }
            }
        }

        /// <summary>
        /// Is this contact a group contact?
        /// </summary>
        public async Task<bool> GetIsGroupAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _blnIsGroup;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Is this contact a group contact?
        /// </summary>
        public async Task SetIsGroupAsync(bool value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_blnIsGroup == value)
                    return;
                IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    _blnIsGroup = value;
                }
                finally
                {
                    await objLocker2.DisposeAsync().ConfigureAwait(false);
                }

                await OnPropertyChangedAsync(nameof(IsGroup), token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public bool LoyaltyEnabled
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return !ReadOnly && !IsGroup && ForcedLoyalty <= 0;
            }
        }

        public async Task<bool> GetLoyaltyEnabledAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return !await GetReadOnlyAsync(token).ConfigureAwait(false) && !await GetIsGroupAsync(token).ConfigureAwait(false) && await GetForcedLoyaltyAsync(token).ConfigureAwait(false) <= 0;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public int ConnectionMaximum
        {
            get
            {
                using (CharacterObject.LockObject.EnterReadLock())
                    return CharacterObject.Created || CharacterObject.FriendsInHighPlaces ? 12 : 6;
            }
        }

        public async Task<int> GetConnectionMaximumAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await CharacterObject.LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return await CharacterObject.GetCreatedAsync(token).ConfigureAwait(false)
                       || await CharacterObject.GetFriendsInHighPlacesAsync(token).ConfigureAwait(false)
                    ? 12
                    : 6;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public string QuickText
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return string.Format(GlobalSettings.CultureInfo, IsGroup ? "({0}/{1}G)" : "({0}/{1})",
                                         Connection, Loyalty);
            }
        }

        public async Task<string> GetQuickTextAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return string.Format(GlobalSettings.CultureInfo, IsGroup ? "({0}/{1}G)" : "({0}/{1})",
                                     await GetConnectionAsync(token).ConfigureAwait(false),
                                     await GetLoyaltyAsync(token).ConfigureAwait(false));
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// The Contact's type, either Contact or Enemy.
        /// </summary>
        public ContactType EntityType
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _eContactType;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (InterlockedExtensions.Exchange(ref _eContactType, value) != value)
                        OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// The Contact's type, either Contact or Enemy.
        /// </summary>
        public async Task<ContactType> GetEntityTypeAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _eContactType;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public bool IsEnemy => EntityType == ContactType.Enemy;

        public async Task<bool> GetIsEnemyAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            return await GetEntityTypeAsync(token).ConfigureAwait(false) == ContactType.Enemy;
        }

        /// <summary>
        /// Name of the save file for this Contact.
        /// </summary>
        public string FileName
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _strFileName;
            }
            set
            {
                using (LockObject.EnterReadLock())
                {
                    if (_strFileName == value)
                        return;
                }
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (_strFileName == value)
                        return;
                    using (LockObject.EnterWriteLock())
                    {
                        if (Interlocked.Exchange(ref _strFileName, value) != value)
                        {
                            RefreshLinkedCharacter(!string.IsNullOrEmpty(value));
                            OnPropertyChanged();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Name of the save file for this Contact.
        /// </summary>
        public async Task<string> GetFileNameAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _strFileName;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Name of the save file for this Contact.
        /// </summary>
        public async Task SetFileNameAsync(string value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_strFileName == value)
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
                if (_strFileName == value)
                    return;

                IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    if (Interlocked.Exchange(ref _strFileName, value) != value)
                    {
                        await RefreshLinkedCharacterAsync(!string.IsNullOrEmpty(value), token).ConfigureAwait(false);
                        await OnPropertyChangedAsync(nameof(FileName), token).ConfigureAwait(false);
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

        /// <summary>
        /// Relative path to the save file.
        /// </summary>
        public string RelativeFileName
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _strRelativeName;
            }
            set
            {
                using (LockObject.EnterReadLock())
                {
                    if (_strRelativeName == value)
                        return;
                }
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (_strRelativeName == value)
                        return;
                    using (LockObject.EnterWriteLock())
                    {
                        if (Interlocked.Exchange(ref _strRelativeName, value) != value)
                        {
                            RefreshLinkedCharacter(!string.IsNullOrEmpty(value));
                            OnPropertyChanged();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Relative path to the save file.
        /// </summary>
        public async Task<string> GetRelativeFileNameAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _strRelativeName;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Relative path to the save file.
        /// </summary>
        public async Task SetRelativeFileNameAsync(string value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_strRelativeName == value)
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
                if (_strRelativeName == value)
                    return;

                IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    if (Interlocked.Exchange(ref _strRelativeName, value) != value)
                    {
                        await RefreshLinkedCharacterAsync(!string.IsNullOrEmpty(value), token).ConfigureAwait(false);
                        await OnPropertyChangedAsync(nameof(RelativeFileName), token).ConfigureAwait(false);
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
                    {
                        _colNotes = value;
                        OnPropertyChanged();
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

        /// <summary>
        /// Group Name.
        /// </summary>
        public string GroupName
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _strGroupName;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (Interlocked.Exchange(ref _strGroupName, value) != value)
                        OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Contact Color.
        /// </summary>
        public Color PreferredColor
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _objColor;
            }
            set
            {
                using (LockObject.EnterReadLock())
                {
                    if (_objColor == value)
                        return;
                }
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (_objColor == value)
                        return;
                    using (LockObject.EnterWriteLock())
                    {
                        _objColor = value;
                        OnPropertyChanged();
                    }
                }
            }
        }

        /// <summary>
        /// Contact Color.
        /// </summary>
        public async Task<Color> GetPreferredColorAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _objColor;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        private int _intCachedFreeFromImprovement = -1;

        /// <summary>
        /// Whether this is a free contact.
        /// </summary>
        public bool Free
        {
            get
            {
                using (LockObject.EnterReadLock())
                {
                    if (_blnFree)
                        return _blnFree;

                    if (_intCachedFreeFromImprovement < 0)
                    {
                        _intCachedFreeFromImprovement = (ImprovementManager
                                                         .GetCachedImprovementListForValueOf(
                                                             CharacterObject,
                                                             Improvement.ImprovementType.ContactMakeFree,
                                                             UniqueId).Count > 0).ToInt32();
                    }

                    return _intCachedFreeFromImprovement > 0;
                }
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (_blnFree == value)
                        return;
                    using (LockObject.EnterWriteLock())
                    {
                        _blnFree = value;
                        OnPropertyChanged();
                    }
                }
            }
        }

        /// <summary>
        /// Whether this is a free contact.
        /// </summary>
        public async Task<bool> GetFreeAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_blnFree)
                    return _blnFree;

                if (_intCachedFreeFromImprovement < 0)
                {
                    _intCachedFreeFromImprovement = ((await ImprovementManager
                                                            .GetCachedImprovementListForValueOfAsync(
                                                                CharacterObject,
                                                                Improvement.ImprovementType.ContactMakeFree,
                                                                await GetUniqueIdAsync(token).ConfigureAwait(false), token: token).ConfigureAwait(false)).Count
                                                     > 0)
                        .ToInt32();
                }

                return _intCachedFreeFromImprovement > 0;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Whether this is a free contact.
        /// </summary>
        public async Task SetFreeAsync(bool value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_blnFree == value)
                    return;
                IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    _blnFree = value;
                }
                finally
                {
                    await objLocker2.DisposeAsync().ConfigureAwait(false);
                }

                await OnPropertyChangedAsync(nameof(Free), token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public bool FreeEnabled
        {
            get
            {
                using (LockObject.EnterReadLock())
                {
                    if (_intCachedFreeFromImprovement < 0)
                    {
                        _intCachedFreeFromImprovement = (ImprovementManager
                                                         .GetCachedImprovementListForValueOf(
                                                             CharacterObject,
                                                             Improvement.ImprovementType.ContactMakeFree,
                                                             UniqueId).Count > 0).ToInt32();
                    }

                    return _intCachedFreeFromImprovement < 1;
                }
            }
        }

        public async Task<bool> GetFreeEnabledAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_intCachedFreeFromImprovement < 0)
                {
                    _intCachedFreeFromImprovement = ((await ImprovementManager
                                                         .GetCachedImprovementListForValueOfAsync(
                                                             CharacterObject,
                                                             Improvement.ImprovementType.ContactMakeFree,
                                                             await GetUniqueIdAsync(token).ConfigureAwait(false),
                                                             token: token).ConfigureAwait(false)).Count
                                                     > 0)
                        .ToInt32();
                }

                return _intCachedFreeFromImprovement < 1;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Unique ID for this contact
        /// </summary>
        public string UniqueId
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _strUnique;
            }
        }

        /// <summary>
        /// Unique ID for this contact
        /// </summary>
        public async Task<string> GetUniqueIdAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _strUnique;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public string InternalId => UniqueId;

        private int _intCachedGroupEnabled = -1;

        /// <summary>
        /// Whether the contact's group status can be modified through the UI
        /// </summary>
        public bool GroupEnabled
        {
            get
            {
                using (LockObject.EnterReadLock())
                {
                    if (_intCachedGroupEnabled < 0)
                    {
                        _intCachedGroupEnabled = (!ReadOnly && ImprovementManager
                            .GetCachedImprovementListForValueOf(
                                CharacterObject,
                                Improvement.ImprovementType.ContactForceGroup,
                                UniqueId)
                            .Count == 0).ToInt32();
                    }

                    return _intCachedGroupEnabled > 0;
                }
            }
        }

        /// <summary>
        /// Whether the contact's group status can be modified through the UI
        /// </summary>
        public async Task<bool> GetGroupEnabledAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_intCachedGroupEnabled < 0)
                {
                    _intCachedGroupEnabled = (!ReadOnly && (await ImprovementManager
                            .GetCachedImprovementListForValueOfAsync(
                                CharacterObject,
                                Improvement.ImprovementType.ContactForceGroup,
                                await GetUniqueIdAsync(token).ConfigureAwait(false), token: token)
                            .ConfigureAwait(false))
                        .Count
                        == 0).ToInt32();
                }

                return _intCachedGroupEnabled > 0;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public bool Blackmail
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _blnBlackmail;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (_blnBlackmail == value)
                        return;
                    using (LockObject.EnterWriteLock())
                    {
                        _blnBlackmail = value;
                        OnPropertyChanged();
                    }
                }
            }
        }

        public async Task<bool> GetBlackmailAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _blnBlackmail;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public async Task SetBlackmailAsync(bool value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_blnBlackmail == value)
                    return;
                IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    _blnBlackmail = value;
                }
                finally
                {
                    await objLocker2.DisposeAsync().ConfigureAwait(false);
                }

                await OnPropertyChangedAsync(nameof(Blackmail), token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public bool Family
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _blnFamily;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (_blnFamily == value)
                        return;
                    using (LockObject.EnterWriteLock())
                    {
                        _blnFamily = value;
                        OnPropertyChanged();
                    }
                }
            }
        }

        public async Task<bool> GetFamilyAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _blnFamily;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public async Task SetFamilyAsync(bool value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_blnFamily == value)
                    return;
                IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    _blnFamily = value;
                }
                finally
                {
                    await objLocker2.DisposeAsync().ConfigureAwait(false);
                }

                await OnPropertyChangedAsync(nameof(Family), token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        private int _intCachedForcedLoyalty = int.MinValue;

        public int ForcedLoyalty
        {
            get
            {
                using (LockObject.EnterReadLock())
                {
                    if (_intCachedForcedLoyalty != int.MinValue)
                        return _intCachedForcedLoyalty;

                    int intMaxForcedLoyalty = 0;
                    foreach (Improvement objImprovement in ImprovementManager.GetCachedImprovementListForValueOf(
                                 CharacterObject, Improvement.ImprovementType.ContactForcedLoyalty, UniqueId))
                    {
                        intMaxForcedLoyalty = Math.Max(intMaxForcedLoyalty, objImprovement.Value.StandardRound());
                    }

                    return _intCachedForcedLoyalty = intMaxForcedLoyalty;
                }
            }
        }

        public async Task<int> GetForcedLoyaltyAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_intCachedForcedLoyalty != int.MinValue)
                    return _intCachedForcedLoyalty;

                int intMaxForcedLoyalty = 0;
                foreach (Improvement objImprovement in await ImprovementManager.GetCachedImprovementListForValueOfAsync(
                                                                                   CharacterObject,
                                                                                   Improvement.ImprovementType
                                                                                       .ContactForcedLoyalty, await GetUniqueIdAsync(token).ConfigureAwait(false),
                                                                                   token: token)
                                                                               .ConfigureAwait(false))
                {
                    intMaxForcedLoyalty = Math.Max(intMaxForcedLoyalty, objImprovement.Value.StandardRound());
                }

                return _intCachedForcedLoyalty = intMaxForcedLoyalty;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public Character CharacterObject => _objCharacter; // readonly member, no locking required

        public Character LinkedCharacter
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _objLinkedCharacter;
            }
        }

        public async Task<Character> GetLinkedCharacterAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _objLinkedCharacter;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public bool NoLinkedCharacter
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _objLinkedCharacter == null;
            }
        }

        public async Task<bool> GetNoLinkedCharacterAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _objLinkedCharacter == null;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public void RefreshLinkedCharacter(bool blnShowError = false, CancellationToken token = default)
        {
            using (LockObject.EnterUpgradeableReadLock(token))
            {
                Character objOldLinkedCharacter = _objLinkedCharacter;
                using (LockObject.EnterWriteLock(token))
                {
                    CharacterObject.LinkedCharacters.Remove(_objLinkedCharacter);
                    bool blnError = false;
                    bool blnUseRelative = false;

                    // Make sure the file still exists before attempting to load it.
                    if (!File.Exists(FileName))
                    {
                        // If the file doesn't exist, use the relative path if one is available.
                        if (string.IsNullOrEmpty(RelativeFileName))
                            blnError = true;
                        else if (!File.Exists(Path.GetFullPath(RelativeFileName)))
                            blnError = true;
                        else
                            blnUseRelative = true;

                        if (blnError && blnShowError)
                        {
                            Program.ShowScrollableMessageBox(
                                string.Format(GlobalSettings.CultureInfo,
                                              LanguageManager.GetString("Message_FileNotFound", token: token),
                                              FileName),
                                LanguageManager.GetString("MessageTitle_FileNotFound", token: token), MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                        }
                    }

                    if (!blnError)
                    {
                        string strFile = blnUseRelative ? Path.GetFullPath(RelativeFileName) : FileName;
                        if (strFile.EndsWith(".chum5", StringComparison.OrdinalIgnoreCase)
                            || strFile.EndsWith(".chum5lz", StringComparison.OrdinalIgnoreCase))
                        {
                            if ((_objLinkedCharacter = Program.OpenCharacters.Find(x => x.FileName == strFile)) == null)
                            {
                                using (ThreadSafeForm<LoadingBar> frmLoadingBar
                                       = Program.CreateAndShowProgressBar(strFile, Character.NumLoadingSections))
                                    _objLinkedCharacter
                                        = Program.LoadCharacter(strFile, string.Empty, false, false,
                                                                frmLoadingBar.MyForm, token);
                            }

                            if (_objLinkedCharacter != null)
                                CharacterObject.LinkedCharacters.TryAdd(_objLinkedCharacter);
                        }
                    }

                    if (_objLinkedCharacter != objOldLinkedCharacter)
                    {
                        if (objOldLinkedCharacter != null)
                        {
                            if (!objOldLinkedCharacter.IsDisposed)
                            {
                                try
                                {
                                    objOldLinkedCharacter.MultiplePropertiesChangedAsync -= LinkedCharacterOnPropertyChanged;
                                }
                                catch (ObjectDisposedException)
                                {
                                    //swallow this
                                }
                            }

                            if (Program.OpenCharacters.Contains(objOldLinkedCharacter))
                            {
                                if (Program.OpenCharacters.All(x => x == _objLinkedCharacter
                                                                    || !x.LinkedCharacters.Contains(
                                                                        objOldLinkedCharacter), token)
                                    && Program.MainForm.OpenFormsWithCharacters.All(
                                        x => !x.CharacterObjects.Contains(objOldLinkedCharacter)))
                                    Program.OpenCharacters.Remove(objOldLinkedCharacter);
                            }
                            else
                                objOldLinkedCharacter.Dispose();
                        }

                        if (_objLinkedCharacter != null)
                        {
                            using (_objLinkedCharacter.LockObject.EnterReadLock(token))
                            {
                                if (string.IsNullOrEmpty(_strName)
                                    && Name != LanguageManager.GetString("String_UnnamedCharacter", token: token))
                                    _strName = Name;
                                if (string.IsNullOrEmpty(_strAge) && !string.IsNullOrEmpty(Age))
                                    _strAge = Age;
                                if (string.IsNullOrEmpty(_strGender) && !string.IsNullOrEmpty(Gender))
                                    _strGender = Gender;
                                if (string.IsNullOrEmpty(_strMetatype) && !string.IsNullOrEmpty(Metatype))
                                    _strMetatype = Metatype;

                                _objLinkedCharacter.MultiplePropertiesChangedAsync += LinkedCharacterOnPropertyChanged;
                            }
                        }

                        OnPropertyChanged(nameof(LinkedCharacter));
                    }
                }
            }
        }

        public async Task RefreshLinkedCharacterAsync(bool blnShowError = false, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                Character objOldLinkedCharacter = _objLinkedCharacter;
                IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    ConcurrentHashSet<Character> lstLinkedCharacters =
                        await CharacterObject.GetLinkedCharactersAsync(token).ConfigureAwait(false);
                    lstLinkedCharacters.Remove(_objLinkedCharacter);
                    bool blnError = false;
                    bool blnUseRelative = false;

                    // Make sure the file still exists before attempting to load it.
                    if (!File.Exists(FileName))
                    {
                        // If the file doesn't exist, use the relative path if one is available.
                        if (string.IsNullOrEmpty(RelativeFileName))
                            blnError = true;
                        else if (!File.Exists(Path.GetFullPath(RelativeFileName)))
                            blnError = true;
                        else
                            blnUseRelative = true;

                        if (blnError && blnShowError)
                        {
                            await Program.ShowScrollableMessageBoxAsync(
                                string.Format(GlobalSettings.CultureInfo,
                                    await LanguageManager.GetStringAsync("Message_FileNotFound", token: token)
                                        .ConfigureAwait(false),
                                    FileName),
                                await LanguageManager.GetStringAsync("MessageTitle_FileNotFound", token: token)
                                    .ConfigureAwait(false), MessageBoxButtons.OK,
                                MessageBoxIcon.Error, token: token).ConfigureAwait(false);
                        }
                    }

                    if (!blnError)
                    {
                        string strFile = blnUseRelative ? Path.GetFullPath(RelativeFileName) : FileName;
                        if (strFile.EndsWith(".chum5", StringComparison.OrdinalIgnoreCase)
                            || strFile.EndsWith(".chum5lz", StringComparison.OrdinalIgnoreCase))
                        {
                            if ((_objLinkedCharacter = await Program.OpenCharacters
                                    .FirstOrDefaultAsync(x => x.FileName == strFile, token: token)
                                    .ConfigureAwait(false)) == null)
                            {
                                using (ThreadSafeForm<LoadingBar> frmLoadingBar
                                       = await Program
                                           .CreateAndShowProgressBarAsync(strFile, Character.NumLoadingSections, token)
                                           .ConfigureAwait(false))
                                    _objLinkedCharacter
                                        = await Program.LoadCharacterAsync(strFile, string.Empty, false, false,
                                            frmLoadingBar.MyForm, token).ConfigureAwait(false);
                            }

                            if (_objLinkedCharacter != null)
                                lstLinkedCharacters.TryAdd(_objLinkedCharacter);
                        }
                    }

                    if (_objLinkedCharacter != objOldLinkedCharacter)
                    {
                        if (objOldLinkedCharacter != null)
                        {
                            objOldLinkedCharacter.MultiplePropertiesChangedAsync -= LinkedCharacterOnPropertyChanged;

                            if (await Program.OpenCharacters.ContainsAsync(objOldLinkedCharacter, token)
                                    .ConfigureAwait(false))
                            {
                                if (await Program.OpenCharacters.AllAsync(async x => x == _objLinkedCharacter
                                                                               || !(await x.GetLinkedCharactersAsync(token).ConfigureAwait(false)).Contains(
                                                                                   objOldLinkedCharacter), token: token)
                                        .ConfigureAwait(false)
                                    && Program.MainForm.OpenFormsWithCharacters.All(
                                        x => !x.CharacterObjects.Contains(objOldLinkedCharacter)))
                                    await Program.OpenCharacters.RemoveAsync(objOldLinkedCharacter, token)
                                        .ConfigureAwait(false);
                            }
                            else
                                await objOldLinkedCharacter.DisposeAsync().ConfigureAwait(false);
                        }

                        if (_objLinkedCharacter != null)
                        {
                            IAsyncDisposable objLocker3 = await _objLinkedCharacter.LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
                            try
                            {
                                token.ThrowIfCancellationRequested();
                                if (string.IsNullOrEmpty(_strName))
                                {
                                    string strName = await GetNameAsync(token).ConfigureAwait(false);
                                    if (strName != await LanguageManager
                                            .GetStringAsync("String_UnnamedCharacter", token: token).ConfigureAwait(false))
                                        _strName = strName;
                                }

                                if (string.IsNullOrEmpty(_strAge))
                                {
                                    string strAge = await GetAgeAsync(token).ConfigureAwait(false);
                                    if (!string.IsNullOrEmpty(strAge))
                                        _strAge = strAge;
                                }

                                if (string.IsNullOrEmpty(_strGender))
                                {
                                    string strGender = await GetGenderAsync(token).ConfigureAwait(false);
                                    if (!string.IsNullOrEmpty(strGender))
                                        _strGender = strGender;
                                }

                                if (string.IsNullOrEmpty(_strMetatype))
                                {
                                    string strMetatype = await GetMetatypeAsync(token).ConfigureAwait(false);
                                    if (!string.IsNullOrEmpty(strMetatype))
                                        _strMetatype = strMetatype;
                                }

                                _objLinkedCharacter.MultiplePropertiesChangedAsync += LinkedCharacterOnPropertyChanged;
                            }
                            finally
                            {
                                await objLocker3.DisposeAsync().ConfigureAwait(false);
                            }
                        }

                        await OnPropertyChangedAsync(nameof(LinkedCharacter), token).ConfigureAwait(false);
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

        private Task LinkedCharacterOnPropertyChanged(object sender, MultiplePropertiesChangedEventArgs e,
            CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            List<string> lstProperties = new List<string>();
            if (e.PropertyNames.Contains(nameof(Character.CharacterName)))
                lstProperties.Add(nameof(Name));
            if (e.PropertyNames.Contains(nameof(Character.Age)))
                lstProperties.Add(nameof(Age));
            if (e.PropertyNames.Contains(nameof(Character.Metatype)) ||
                e.PropertyNames.Contains(nameof(Character.Metavariant)))
                lstProperties.Add(nameof(Metatype));
            if (e.PropertyNames.Contains(nameof(Character.Mugshots)))
                lstProperties.Add(nameof(Mugshots));
            if (e.PropertyNames.Contains(nameof(Character.MainMugshot)))
                lstProperties.Add(nameof(MainMugshot));
            if (e.PropertyNames.Contains(nameof(Character.MainMugshotIndex)))
                lstProperties.Add(nameof(MainMugshotIndex));
            return lstProperties.Count > 0
                ? OnMultiplePropertiesChangedAsync(lstProperties, token)
                : Task.CompletedTask;
        }

        #endregion Properties

        #region IHasMugshots

        /// <summary>
        /// Character's portraits encoded using Base64.
        /// </summary>
        public ThreadSafeList<Image> Mugshots
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return LinkedCharacter != null ? LinkedCharacter.Mugshots : _lstMugshots;
            }
        }

        /// <summary>
        /// Character's main portrait encoded using Base64.
        /// </summary>
        public Image MainMugshot
        {
            get
            {
                using (LockObject.EnterReadLock())
                {
                    if (LinkedCharacter != null)
                        return LinkedCharacter.MainMugshot;
                    if (MainMugshotIndex >= Mugshots.Count || MainMugshotIndex < 0)
                        return null;
                    return Mugshots[MainMugshotIndex];
                }
            }
            set
            {
                if (value == null)
                {
                    MainMugshotIndex = -1;
                    return;
                }

                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (LinkedCharacter != null)
                        LinkedCharacter.MainMugshot = value;
                    else
                    {
                        int intNewMainMugshotIndex = Mugshots.IndexOf(value);
                        if (intNewMainMugshotIndex != -1)
                        {
                            MainMugshotIndex = intNewMainMugshotIndex;
                        }
                        else
                        {
                            using (Mugshots.LockObject.EnterWriteLock())
                            {
                                Mugshots.Add(value);
                                MainMugshotIndex = Mugshots.IndexOf(value);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Character's main portrait encoded using Base64.
        /// </summary>
        public async Task<Image> GetMainMugshotAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                Character objLinkedCharacter = await GetLinkedCharacterAsync(token).ConfigureAwait(false);
                if (objLinkedCharacter != null)
                    return await objLinkedCharacter.GetMainMugshotAsync(token).ConfigureAwait(false);
                int intIndex = await GetMainMugshotIndexAsync(token).ConfigureAwait(false);
                if (intIndex >= await Mugshots.GetCountAsync(token).ConfigureAwait(false) || intIndex < 0)
                    return null;

                return await Mugshots.GetValueAtAsync(intIndex, token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Character's main portrait encoded using Base64.
        /// </summary>
        public async Task SetMainMugshotAsync(Image value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (value == null)
            {
                await SetMainMugshotIndexAsync(-1, token).ConfigureAwait(false);
                return;
            }
            IAsyncDisposable objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                Character objLinkedCharacter = await GetLinkedCharacterAsync(token).ConfigureAwait(false);
                if (objLinkedCharacter != null)
                {
                    await objLinkedCharacter.SetMainMugshotAsync(value, token).ConfigureAwait(false);
                }
                else
                {
                    int intNewMainMugshotIndex = await Mugshots.IndexOfAsync(value, token).ConfigureAwait(false);
                    if (intNewMainMugshotIndex != -1)
                    {
                        await SetMainMugshotIndexAsync(intNewMainMugshotIndex, token).ConfigureAwait(false);
                    }
                    else
                    {
                        IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                        try
                        {
                            token.ThrowIfCancellationRequested();
                            IAsyncDisposable objLocker3 =
                                await Mugshots.LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                            try
                            {
                                token.ThrowIfCancellationRequested();
                                await Mugshots.AddAsync(value, token).ConfigureAwait(false);
                                await SetMainMugshotIndexAsync(await Mugshots.IndexOfAsync(value, token).ConfigureAwait(false), token).ConfigureAwait(false);
                            }
                            finally
                            {
                                await objLocker3.DisposeAsync().ConfigureAwait(false);
                            }
                        }
                        finally
                        {
                            await objLocker2.DisposeAsync().ConfigureAwait(false);
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
        /// Index of Character's main portrait. -1 if set to none.
        /// </summary>
        public int MainMugshotIndex
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return LinkedCharacter?.MainMugshotIndex ?? _intMainMugshotIndex;
            }
            set
            {
                if (value < -1)
                    value = -1;
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (LinkedCharacter != null)
                        LinkedCharacter.MainMugshotIndex = value;
                    else
                    {
                        if (value >= Mugshots.Count)
                            value = -1;

                        if (Interlocked.Exchange(ref _intMainMugshotIndex, value) != value)
                            OnPropertyChanged();
                    }
                }
            }
        }

        /// <summary>
        /// Index of Character's main portrait. -1 if set to none.
        /// </summary>
        public async Task<int> GetMainMugshotIndexAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                Character objLinkedCharacter = await GetLinkedCharacterAsync(token).ConfigureAwait(false);
                if (objLinkedCharacter != null)
                    return await objLinkedCharacter.GetMainMugshotIndexAsync(token).ConfigureAwait(false);
                return _intMainMugshotIndex;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Index of Character's main portrait. -1 if set to none.
        /// </summary>
        public async Task SetMainMugshotIndexAsync(int value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (value < -1)
                value = -1;
            IAsyncDisposable objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                Character objLinkedCharacter = await GetLinkedCharacterAsync(token).ConfigureAwait(false);
                if (objLinkedCharacter != null)
                    await objLinkedCharacter.SetMainMugshotIndexAsync(value, token).ConfigureAwait(false);
                else
                {
                    if (value >= await Mugshots.GetCountAsync(token).ConfigureAwait(false))
                        value = -1;
                    if (Interlocked.Exchange(ref _intMainMugshotIndex, value) != value)
                        await OnPropertyChangedAsync(nameof(MainMugshotIndex), token).ConfigureAwait(false);
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Index of Character's main portrait. -1 if set to none.
        /// </summary>
        public async Task ModifyMainMugshotIndexAsync(int value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (value == 0)
                return;
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                Character objLinkedCharacter = await GetLinkedCharacterAsync(token).ConfigureAwait(false);
                if (objLinkedCharacter != null)
                    await objLinkedCharacter.ModifyMainMugshotIndexAsync(value, token).ConfigureAwait(false);
                else
                {
                    int intOldValue = _intMainMugshotIndex;
                    int intNewValue = Interlocked.Add(ref _intMainMugshotIndex, value);
                    if (intNewValue < -1 || intNewValue >= await Mugshots.GetCountAsync(token).ConfigureAwait(false))
                        intNewValue = -1;
                    if (intOldValue != intNewValue)
                        await OnPropertyChangedAsync(nameof(MainMugshotIndex), token).ConfigureAwait(false);
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public void SaveMugshots(XmlWriter objWriter, CancellationToken token = default)
        {
            Utils.SafelyRunSynchronously(() => SaveMugshotsCore(true, objWriter, token), token);
        }

        public Task SaveMugshotsAsync(XmlWriter objWriter, CancellationToken token = default)
        {
            return SaveMugshotsCore(false, objWriter, token);
        }

        public async Task SaveMugshotsCore(bool blnSync, XmlWriter objWriter, CancellationToken token = default)
        {
            if (objWriter == null)
                return;
            token.ThrowIfCancellationRequested();
            IDisposable objLocker = null;
            IAsyncDisposable objLockerAsync = null;
            if (blnSync)
                // ReSharper disable once MethodHasAsyncOverload
                objLocker = LockObject.EnterReadLock(token);
            else
                objLockerAsync = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (blnSync)
                {
                    objWriter.WriteElementString("mainmugshotindex",
                                                 MainMugshotIndex.ToString(GlobalSettings.InvariantCultureInfo));
                    // <mugshot>
                    // ReSharper disable once MethodHasAsyncOverload
                    // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                    using (objWriter.StartElement("mugshots"))
                    {
                        foreach (Image imgMugshot in Mugshots)
                        {
                            // ReSharper disable once MethodHasAsyncOverload
                            objWriter.WriteElementString(
                                // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                                "mugshot", GlobalSettings.ImageToBase64StringForStorage(imgMugshot));
                        }
                    }
                    // </mugshot>
                }
                else
                {
                    await objWriter.WriteElementStringAsync("mainmugshotindex",
                        (await GetMainMugshotIndexAsync(token).ConfigureAwait(false)).ToString(
                            GlobalSettings.InvariantCultureInfo),
                        token: token).ConfigureAwait(false);
                    // <mugshots>
                    XmlElementWriteHelper objBaseElement
                        = await objWriter.StartElementAsync("mugshots", token: token).ConfigureAwait(false);
                    try
                    {
                        await Mugshots.ForEachAsync(async imgMugshot =>
                        {
                            await objWriter.WriteElementStringAsync(
                                "mugshot",
                                await GlobalSettings.ImageToBase64StringForStorageAsync(imgMugshot, token)
                                    .ConfigureAwait(false), token: token).ConfigureAwait(false);
                        }, token).ConfigureAwait(false);
                    }
                    finally
                    {
                        // </mugshots>
                        await objBaseElement.DisposeAsync().ConfigureAwait(false);
                    }
                }
            }
            finally
            {
                objLocker?.Dispose();
                if (objLockerAsync != null)
                    await objLockerAsync.DisposeAsync().ConfigureAwait(false);
            }
        }

        public void LoadMugshots(XPathNavigator xmlSavedNode, CancellationToken token = default)
        {
            using (LockObject.EnterWriteLock(token))
            {
                xmlSavedNode.TryGetInt32FieldQuickly("mainmugshotindex", ref _intMainMugshotIndex);
                XPathNodeIterator xmlMugshotsList = xmlSavedNode.SelectAndCacheExpression("mugshots/mugshot", token);
                List<string> lstMugshotsBase64 = new List<string>(xmlMugshotsList.Count);
                foreach (XPathNavigator objXmlMugshot in xmlMugshotsList)
                {
                    string strMugshot = objXmlMugshot.Value;
                    if (!string.IsNullOrWhiteSpace(strMugshot))
                    {
                        lstMugshotsBase64.Add(strMugshot);
                    }
                }

                if (lstMugshotsBase64.Count > 1)
                {
                    Image[] objMugshotImages = new Image[lstMugshotsBase64.Count];
                    Parallel.For(0, lstMugshotsBase64.Count,
                                 i => objMugshotImages[i] = lstMugshotsBase64[i].ToImage(PixelFormat.Format32bppPArgb));
                    _lstMugshots.AddRange(objMugshotImages);
                }
                else if (lstMugshotsBase64.Count == 1)
                {
                    _lstMugshots.Add(lstMugshotsBase64[0].ToImage(PixelFormat.Format32bppPArgb));
                }
            }
        }

        public async Task LoadMugshotsAsync(XPathNavigator xmlSavedNode, CancellationToken token = default)
        {
            // Mugshots
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                xmlSavedNode.TryGetInt32FieldQuickly("mainmugshotindex", ref _intMainMugshotIndex);
                XPathNodeIterator xmlMugshotsList = xmlSavedNode.SelectAndCacheExpression("mugshots/mugshot", token);
                List<string> lstMugshotsBase64 = new List<string>(xmlMugshotsList.Count);
                foreach (XPathNavigator objXmlMugshot in xmlMugshotsList)
                {
                    string strMugshot = objXmlMugshot.Value;
                    if (!string.IsNullOrWhiteSpace(strMugshot))
                    {
                        lstMugshotsBase64.Add(strMugshot);
                    }
                }

                if (lstMugshotsBase64.Count > 1)
                {
                    Task<Bitmap>[] atskMugshotImages = new Task<Bitmap>[lstMugshotsBase64.Count];
                    for (int i = 0; i < lstMugshotsBase64.Count; ++i)
                    {
                        int iLocal = i;
                        atskMugshotImages[i]
                            = Task.Run(() => lstMugshotsBase64[iLocal].ToImageAsync(PixelFormat.Format32bppPArgb, token), token);
                    }
                    await _lstMugshots.AddRangeAsync(await Task.WhenAll(atskMugshotImages).ConfigureAwait(false), token).ConfigureAwait(false);
                }
                else if (lstMugshotsBase64.Count == 1)
                {
                    await _lstMugshots.AddAsync(await lstMugshotsBase64[0].ToImageAsync(PixelFormat.Format32bppPArgb, token).ConfigureAwait(false), token).ConfigureAwait(false);
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public async Task PrintMugshots(XmlWriter objWriter, CancellationToken token = default)
        {
            if (objWriter == null)
                return;
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                Character objLinkedCharacter = await GetLinkedCharacterAsync(token).ConfigureAwait(false);
                if (objLinkedCharacter != null)
                    await objLinkedCharacter.PrintMugshots(objWriter, token).ConfigureAwait(false);
                else if (await Mugshots.GetCountAsync(token).ConfigureAwait(false) > 0)
                {
                    // Since IE is retarded and can't handle base64 images before IE9, we need to dump the image to a temporary directory and re-write the information.
                    // If you give it an extension of jpg, gif, or png, it expects the file to be in that format and won't render the image unless it was originally that type.
                    // But if you give it the extension img, it will render whatever you give it (which doesn't make any damn sense, but that's IE for you).
                    string strMugshotsDirectoryPath = Path.Combine(Utils.GetStartupPath, "mugshots");
                    if (!Directory.Exists(strMugshotsDirectoryPath))
                    {
                        try
                        {
                            Directory.CreateDirectory(strMugshotsDirectoryPath);
                        }
                        catch (UnauthorizedAccessException)
                        {
                            await Program.ShowScrollableMessageBoxAsync(await LanguageManager
                                .GetStringAsync("Message_Insufficient_Permissions_Warning",
                                    token: token).ConfigureAwait(false), token: token).ConfigureAwait(false);
                        }
                    }

                    Image imgMainMugshot = await GetMainMugshotAsync(token).ConfigureAwait(false);
                    if (imgMainMugshot != null)
                    {
                        // <mainmugshotbase64 />
                        await objWriter
                              .WriteElementStringAsync("mainmugshotbase64",
                                                       await imgMainMugshot.ToBase64StringAsJpegAsync(token: token)
                                                                           .ConfigureAwait(false), token: token)
                              .ConfigureAwait(false);
                    }

                    // <hasothermugshots>
                    await objWriter.WriteElementStringAsync("hasothermugshots",
                                                            (imgMainMugshot == null || await Mugshots
                                                                .GetCountAsync(token)
                                                                .ConfigureAwait(false) > 1)
                                                            .ToString(GlobalSettings.InvariantCultureInfo),
                                                            token: token)
                                   .ConfigureAwait(false);
                    // <othermugshots>
                    XmlElementWriteHelper objOtherMugshotsElement
                        = await objWriter.StartElementAsync("othermugshots", token: token).ConfigureAwait(false);
                    try
                    {
                        for (int i = 0; i < await Mugshots.GetCountAsync(token).ConfigureAwait(false); ++i)
                        {
                            if (i == await GetMainMugshotIndexAsync(token).ConfigureAwait(false))
                                continue;
                            Image imgMugshot = await Mugshots.GetValueAtAsync(i, token).ConfigureAwait(false);
                            // <mugshot>
                            XmlElementWriteHelper objMugshotElement
                                = await objWriter.StartElementAsync("mugshot", token: token).ConfigureAwait(false);
                            try
                            {
                                await objWriter
                                      .WriteElementStringAsync("stringbase64",
                                                               await imgMugshot.ToBase64StringAsJpegAsync(token: token)
                                                                               .ConfigureAwait(false), token: token)
                                      .ConfigureAwait(false);
                            }
                            finally
                            {
                                // </mugshot>
                                await objMugshotElement.DisposeAsync().ConfigureAwait(false);
                            }
                        }
                    }
                    finally
                    {
                        // </othermugshots>
                        await objOtherMugshotsElement.DisposeAsync().ConfigureAwait(false);
                    }
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            using (LockObject.EnterWriteLock())
            {
                if (_objLinkedCharacter != null && !Utils.IsUnitTest
                                                && Program.OpenCharacters.All(
                                                    x => x == _objLinkedCharacter
                                                         || !x.LinkedCharacters.Contains(_objLinkedCharacter))
                                                && Program.MainForm.OpenFormsWithCharacters.All(
                                                    x => !x.CharacterObjects.Contains(_objLinkedCharacter)))
                    Program.OpenCharacters.Remove(_objLinkedCharacter);
                foreach (Image imgMugshot in _lstMugshots)
                    imgMugshot.Dispose();
                _lstMugshots.Dispose();
            }
        }

        /// <inheritdoc />
        public async ValueTask DisposeAsync()
        {
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync().ConfigureAwait(false);
            try
            {
                if (_objLinkedCharacter != null && !Utils.IsUnitTest
                                                && await Program.OpenCharacters.AllAsync(
                                                                    async x => x == _objLinkedCharacter
                                                                         || !(await x.GetLinkedCharactersAsync().ConfigureAwait(false)).Contains(
                                                                             _objLinkedCharacter))
                                                                .ConfigureAwait(false)
                                                && Program.MainForm.OpenFormsWithCharacters.All(
                                                    x => !x.CharacterObjects.Contains(_objLinkedCharacter)))
                    await Program.OpenCharacters.RemoveAsync(_objLinkedCharacter).ConfigureAwait(false);
                await _lstMugshots.ForEachAsync(x => x.Dispose()).ConfigureAwait(false);
                await _lstMugshots.DisposeAsync().ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        #endregion IHasMugshots

        /// <inheritdoc />
        public AsyncFriendlyReaderWriterLock LockObject { get; }
    }
}
