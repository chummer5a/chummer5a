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
    [DebuggerDisplay("{" + nameof(Name) + "} ({DisplayRoleMethod(GlobalSettings.DefaultLanguage)})")]
    public sealed class Contact : INotifyMultiplePropertyChanged, IHasName, IHasMugshots, IHasNotes, IHasInternalId, IHasLockObject
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
        private readonly ThreadSafeList<Image> _lstMugshots = new ThreadSafeList<Image>(3);
        private int _intMainMugshotIndex = -1;

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        public void OnPropertyChanged([CallerMemberName] string strPropertyName = null)
        {
            this.OnMultiplePropertyChanged(strPropertyName);
        }

        public void OnMultiplePropertyChanged(IReadOnlyCollection<string> lstPropertyNames)
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
                        {
                            _intCachedForcedLoyalty = int.MinValue;
                        }

                        if (setNamesOfChangedProperties.Contains(nameof(GroupEnabled)))
                        {
                            _intCachedGroupEnabled = -1;
                        }

                        if (setNamesOfChangedProperties.Contains(nameof(Free)))
                        {
                            _intCachedFreeFromImprovement = -1;
                        }
                    }

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

                    if (!Free
                        || setNamesOfChangedProperties.Contains(nameof(Free))
                        || setNamesOfChangedProperties.Contains(nameof(ContactPoints)))
                    {
                        if (IsEnemy || setNamesOfChangedProperties.Contains(nameof(IsEnemy)))
                        {
                            _objCharacter.OnPropertyChanged(nameof(Character.EnemyKarma));
                        }

                        if ((!IsEnemy || setNamesOfChangedProperties.Contains(nameof(IsEnemy)))
                            && (IsGroup || setNamesOfChangedProperties.Contains(nameof(IsGroup))))
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

        private static readonly PropertyDependencyGraph<Contact> s_ContactDependencyGraph =
            new PropertyDependencyGraph<Contact>(
                new DependencyGraphNode<string, Contact>(nameof(NoLinkedCharacter),
                                                         new DependencyGraphNode<string, Contact>(
                                                             nameof(LinkedCharacter)
                                                         )
                ),
                new DependencyGraphNode<string, Contact>(nameof(CurrentDisplayName),
                                                         new DependencyGraphNode<string, Contact>(nameof(Name),
                                                                 new DependencyGraphNode<string, Contact>(
                                                                     nameof(LinkedCharacter)
                                                                 )
                                                         )
                ),
                new DependencyGraphNode<string, Contact>(nameof(DisplayGender),
                                                         new DependencyGraphNode<string, Contact>(nameof(Gender),
                                                                 new DependencyGraphNode<string, Contact>(
                                                                     nameof(LinkedCharacter)
                                                                 )
                                                         )
                ),
                new DependencyGraphNode<string, Contact>(nameof(DisplayMetatype),
                                                         new DependencyGraphNode<string, Contact>(nameof(Metatype),
                                                                 new DependencyGraphNode<string, Contact>(
                                                                     nameof(LinkedCharacter))
                                                         )
                ),
                new DependencyGraphNode<string, Contact>(nameof(DisplayAge),
                                                         new DependencyGraphNode<string, Contact>(nameof(Age),
                                                                 new DependencyGraphNode<string, Contact>(
                                                                     nameof(LinkedCharacter))
                                                         )
                ),
                new DependencyGraphNode<string, Contact>(nameof(MainMugshot),
                                                         new DependencyGraphNode<string, Contact>(
                                                             nameof(LinkedCharacter)),
                                                         new DependencyGraphNode<string, Contact>(nameof(Mugshots),
                                                                 new DependencyGraphNode<string, Contact>(
                                                                     nameof(LinkedCharacter))
                                                         ),
                                                         new DependencyGraphNode<string, Contact>(
                                                             nameof(MainMugshotIndex))
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
                                                                 new DependencyGraphNode<string, Contact>(
                                                                     nameof(ConnectionMaximum))
                                                         ),
                                                         new DependencyGraphNode<string, Contact>(nameof(Loyalty)),
                                                         new DependencyGraphNode<string, Contact>(nameof(Family)),
                                                         new DependencyGraphNode<string, Contact>(nameof(Blackmail))
                ),
                new DependencyGraphNode<string, Contact>(nameof(QuickText),
                                                         new DependencyGraphNode<string, Contact>(nameof(Connection)),
                                                         new DependencyGraphNode<string, Contact>(nameof(IsGroup)),
                                                         new DependencyGraphNode<string, Contact>(nameof(Loyalty),
                                                                 new DependencyGraphNode<string, Contact>(
                                                                     nameof(IsGroup)),
                                                                 new DependencyGraphNode<string, Contact>(
                                                                     nameof(ForcedLoyalty))
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
            _objCharacter = objCharacter;
            if (_objCharacter != null)
            {
                using (_objCharacter.LockObject.EnterWriteLock())
                    _objCharacter.PropertyChanged += CharacterObjectOnPropertyChanged;
            }
            _blnReadOnly = blnIsReadOnly;
        }

        private void CharacterObjectOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Character.Created) || e.PropertyName == nameof(Character.FriendsInHighPlaces))
                OnPropertyChanged(nameof(ConnectionMaximum));
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
                    objWriter.WriteElementString("notes", _strNotes.CleanOfInvalidUnicodeChars());
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
                using (await LockObject.EnterReadLockAsync(token).ConfigureAwait(false))
                {
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
                              .WriteElementStringAsync("notes", _strNotes.CleanOfInvalidUnicodeChars(), token: token)
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

                string sNotesColor = ColorTranslator.ToHtml(await ColorManager.GetHasNotesColorAsync(token).ConfigureAwait(false));
                objNode.TryGetStringFieldQuickly("notesColor", ref sNotesColor);
                _colNotes = ColorTranslator.FromHtml(sNotesColor);

                objNode.TryGetStringFieldQuickly("groupname", ref _strGroupName);
                objNode.TryGetBoolFieldQuickly("group", ref _blnIsGroup);
                objNode.TryGetStringFieldQuickly("guid", ref _strUnique);
                objNode.TryGetBoolFieldQuickly("family", ref _blnFamily);
                objNode.TryGetBoolFieldQuickly("blackmail", ref _blnBlackmail);
                objNode.TryGetBoolFieldQuickly("free", ref _blnFree);
                if (await objNode.SelectSingleNodeAndCacheExpressionAsync("colour", token).ConfigureAwait(false) != null)
                {
                    int intTmp = _objColor.ToArgb();
                    if (objNode.TryGetInt32FieldQuickly("colour", ref intTmp))
                        _objColor = Color.FromArgb(intTmp);
                }

                _blnReadOnly = await objNode.SelectSingleNodeAndCacheExpressionAsync("readonly", token).ConfigureAwait(false) != null;

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
        public async ValueTask Print(XmlWriter objWriter, CultureInfo objCulture, string strLanguageToPrint,
                                     CancellationToken token = default)
        {
            if (objWriter == null)
                return;
            using (await LockObject.EnterReadLockAsync(token).ConfigureAwait(false))
            {
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
                        await objWriter.WriteElementStringAsync("notes", Notes, token: token).ConfigureAwait(false);

                    await PrintMugshots(objWriter, token).ConfigureAwait(false);
                }
                finally
                {
                    // </contact>
                    await objBaseElement.DisposeAsync().ConfigureAwait(false);
                }
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
        public async ValueTask<string> GetNameAsync(CancellationToken token = default)
        {
            using (await LockObject.EnterReadLockAsync(token).ConfigureAwait(false))
            {
                return LinkedCharacter != null
                    ? await LinkedCharacter.GetCharacterNameAsync(token).ConfigureAwait(false)
                    : _strName;
            }
        }

        public string CurrentDisplayName => Name;

        public ValueTask<string> GetCurrentDisplayNameAsync(CancellationToken token = default) =>
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

        public async ValueTask SetDisplayRoleAsync(string value, CancellationToken token = default)
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
        public async ValueTask<int> GetConnectionAsync(CancellationToken token = default)
        {
            using (await LockObject.EnterReadLockAsync(token).ConfigureAwait(false))
                return Math.Min(_intConnection, await GetConnectionMaximumAsync(token).ConfigureAwait(false));
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
        public async ValueTask<int> GetLoyaltyAsync(CancellationToken token = default)
        {
            using (await LockObject.EnterReadLockAsync(token).ConfigureAwait(false))
            {
                int intForced = await GetForcedLoyaltyAsync(token).ConfigureAwait(false);
                if (intForced > 0)
                    return intForced;
                return IsGroup ? 1 : _intLoyalty;
            }
        }

        public string DisplayMetatypeMethod(string strLanguage)
        {
            using (LockObject.EnterReadLock())
            {
                string strReturn = Metatype;
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
                    strReturn = _objCharacter.TranslateExtra(strReturn, strLanguage, "metatypes.xml");

                return strReturn;
            }
        }

        public async Task<string> DisplayMetatypeMethodAsync(string strLanguage, CancellationToken token = default)
        {
            using (await LockObject.EnterReadLockAsync(token).ConfigureAwait(false))
            {
                string strReturn = Metatype;
                if (LinkedCharacter != null)
                {
                    // Update character information fields.
                    XPathNavigator objMetatypeNode
                        = await _objCharacter.GetNodeXPathAsync(true, token: token).ConfigureAwait(false);

                    strReturn
                        = (await objMetatypeNode.SelectSingleNodeAndCacheExpressionAsync("translate", token: token)
                                                .ConfigureAwait(false))?.Value
                          ?? await _objCharacter.TranslateExtraAsync(
                              await LinkedCharacter.GetMetatypeAsync(token).ConfigureAwait(false),
                              strLanguage, "metatypes.xml", token).ConfigureAwait(false);

                    Guid guiMetavariant = await LinkedCharacter.GetMetavariantGuidAsync(token).ConfigureAwait(false);
                    if (guiMetavariant == Guid.Empty)
                        return strReturn;
                    objMetatypeNode
                        = objMetatypeNode.TryGetNodeById("metavariants/metavariant", LinkedCharacter.MetavariantGuid);

                    string strMetatypeTranslate = objMetatypeNode != null
                        ? (await objMetatypeNode.SelectSingleNodeAndCacheExpressionAsync("translate", token: token)
                                                .ConfigureAwait(false))?.Value
                        : null;
                    strReturn += await LanguageManager.GetStringAsync("String_Space", strLanguage, token: token)
                                                      .ConfigureAwait(false)
                                 + '('
                                 + (!string.IsNullOrEmpty(strMetatypeTranslate)
                                     ? strMetatypeTranslate
                                     : await _objCharacter.TranslateExtraAsync(
                                         await LinkedCharacter.GetMetavariantAsync(token).ConfigureAwait(false),
                                         strLanguage, "metatypes.xml",
                                         token).ConfigureAwait(false))
                                 + ')';
                }
                else
                    strReturn = await _objCharacter.TranslateExtraAsync(strReturn, strLanguage, "metatypes.xml", token)
                                                   .ConfigureAwait(false);

                return strReturn;
            }
        }

        public string DisplayMetatype
        {
            get => DisplayMetatypeMethod(GlobalSettings.Language);
            set => Metatype = _objCharacter.ReverseTranslateExtra(value, GlobalSettings.Language, "contacts.xml");
        }

        public Task<string> GetDisplayMetatypeAsync(CancellationToken token = default) =>
            DisplayMetatypeMethodAsync(GlobalSettings.Language, token);

        public async ValueTask SetDisplayMetatypeAsync(string value, CancellationToken token = default)
        {
            Metatype = await _objCharacter.ReverseTranslateExtraAsync(value, GlobalSettings.Language, "contacts.xml",
                                                                      token).ConfigureAwait(false);
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

                        if (!string.IsNullOrEmpty(LinkedCharacter.Metavariant) && LinkedCharacter.Metavariant != "None")
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
        public async ValueTask<string> GetMetatypeAsync(CancellationToken token = default)
        {
            using (await LockObject.EnterReadLockAsync(token).ConfigureAwait(false))
            {
                if (LinkedCharacter != null)
                {
                    string strMetatype = await LinkedCharacter.GetMetatypeAsync(token).ConfigureAwait(false);
                    string strMetavariant = await LinkedCharacter.GetMetavariantAsync(token).ConfigureAwait(false);

                    if (!string.IsNullOrEmpty(strMetavariant) && strMetavariant != "None")
                    {
                        strMetatype += await LanguageManager.GetStringAsync("String_Space", token: token)
                                                            .ConfigureAwait(false) + '('
                            + strMetavariant + ')';
                    }

                    return strMetatype;
                }

                return _strMetatype;
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
            string strGender = Gender;
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

        public async ValueTask SetDisplayGenderAsync(string value, CancellationToken token = default)
        {
            Gender = await _objCharacter.ReverseTranslateExtraAsync(value, GlobalSettings.Language, "contacts.xml",
                                                                    token).ConfigureAwait(false);
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
        public async ValueTask<string> GetGenderAsync(CancellationToken token = default)
        {
            using (await LockObject.EnterReadLockAsync(token).ConfigureAwait(false))
            {
                return LinkedCharacter != null
                    ? await LinkedCharacter.GetGenderAsync(token).ConfigureAwait(false)
                    : _strGender;
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
            string strAge = Age;
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

        public async ValueTask SetDisplayAgeAsync(string value, CancellationToken token = default)
        {
            Age = await _objCharacter.ReverseTranslateExtraAsync(value, GlobalSettings.Language, "contacts.xml", token)
                                     .ConfigureAwait(false);
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
        public async ValueTask<string> GetAgeAsync(CancellationToken token = default)
        {
            using (await LockObject.EnterReadLockAsync(token).ConfigureAwait(false))
            {
                return LinkedCharacter != null
                    ? await LinkedCharacter.GetAgeAsync(token).ConfigureAwait(false)
                    : _strGender;
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

        public async ValueTask SetDisplayTypeAsync(string value, CancellationToken token = default)
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

        public async ValueTask SetDisplayPreferredPaymentAsync(string value, CancellationToken token = default)
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

        public async ValueTask SetDisplayHobbiesViceAsync(string value, CancellationToken token = default)
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

        public async ValueTask SetDisplayPersonalLifeAsync(string value, CancellationToken token = default)
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

        public bool LoyaltyEnabled
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return !ReadOnly && !IsGroup && ForcedLoyalty <= 0;
            }
        }

        public async ValueTask<bool> GetLoyaltyEnabledAsync(CancellationToken token = default)
        {
            using (await LockObject.EnterReadLockAsync(token).ConfigureAwait(false))
                return !ReadOnly && !IsGroup && await GetForcedLoyaltyAsync(token).ConfigureAwait(false) <= 0;
        }

        public int ConnectionMaximum
        {
            get
            {
                using (CharacterObject.LockObject.EnterReadLock())
                    return CharacterObject.Created || CharacterObject.FriendsInHighPlaces ? 12 : 6;
            }
        }

        public async ValueTask<int> GetConnectionMaximumAsync(CancellationToken token = default)
        {
            using (await CharacterObject.LockObject.EnterReadLockAsync(token).ConfigureAwait(false))
            {
                return await CharacterObject.GetCreatedAsync(token).ConfigureAwait(false)
                       || await CharacterObject.GetFriendsInHighPlacesAsync(token).ConfigureAwait(false)
                    ? 12
                    : 6;
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

        public async ValueTask<string> GetQuickTextAsync(CancellationToken token = default)
        {
            using (await LockObject.EnterReadLockAsync(token).ConfigureAwait(false))
            {
                return string.Format(GlobalSettings.CultureInfo, IsGroup ? "({0}/{1}G)" : "({0}/{1})",
                                     await GetConnectionAsync(token).ConfigureAwait(false),
                                     await GetLoyaltyAsync(token).ConfigureAwait(false));
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

        public bool IsEnemy => EntityType == ContactType.Enemy;

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
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (Interlocked.Exchange(ref _strFileName, value) != value)
                        RefreshLinkedCharacter(!string.IsNullOrEmpty(value));
                }
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
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (Interlocked.Exchange(ref _strRelativeName, value) != value)
                        RefreshLinkedCharacter(!string.IsNullOrEmpty(value));
                }
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

        private int _intCachedFreeFromImprovement = -1;

        /// <summary>
        /// Whether or not this is a free contact.
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
                    }
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Whether or not this is a free contact.
        /// </summary>
        public async ValueTask<bool> GetFreeAsync(CancellationToken token = default)
        {
            using (await LockObject.EnterReadLockAsync(token).ConfigureAwait(false))
            {
                if (_blnFree)
                    return _blnFree;

                if (_intCachedFreeFromImprovement < 0)
                {
                    _intCachedFreeFromImprovement = ((await ImprovementManager
                                                            .GetCachedImprovementListForValueOfAsync(
                                                                CharacterObject,
                                                                Improvement.ImprovementType.ContactMakeFree,
                                                                UniqueId, token: token).ConfigureAwait(false)).Count
                                                     > 0)
                        .ToInt32();
                }

                return _intCachedFreeFromImprovement > 0;
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

        public async ValueTask<bool> GetFreeEnabledAsync(CancellationToken token = default)
        {
            using (await LockObject.EnterReadLockAsync(token).ConfigureAwait(false))
            {
                if (_intCachedFreeFromImprovement < 0)
                {
                    _intCachedFreeFromImprovement = ((await ImprovementManager
                                                            .GetCachedImprovementListForValueOfAsync(
                                                                CharacterObject,
                                                                Improvement.ImprovementType.ContactMakeFree,
                                                                UniqueId, token: token).ConfigureAwait(false)).Count
                                                     > 0)
                        .ToInt32();
                }

                return _intCachedFreeFromImprovement < 1;
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

        public string InternalId => UniqueId;

        private int _intCachedGroupEnabled = -1;

        /// <summary>
        /// Whether or not the contact's group status can be modified through the UI
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
        /// Whether or not the contact's group status can be modified through the UI
        /// </summary>
        public async ValueTask<bool> GetGroupEnabledAsync(CancellationToken token = default)
        {
            using (await LockObject.EnterReadLockAsync(token).ConfigureAwait(false))
            {
                if (_intCachedGroupEnabled < 0)
                {
                    _intCachedGroupEnabled = (!ReadOnly && (await ImprovementManager
                                                                  .GetCachedImprovementListForValueOfAsync(
                                                                      CharacterObject,
                                                                      Improvement.ImprovementType.ContactForceGroup,
                                                                      UniqueId, token: token).ConfigureAwait(false))
                        .Count
                        == 0).ToInt32();
                }

                return _intCachedGroupEnabled > 0;
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

        public async ValueTask<int> GetForcedLoyaltyAsync(CancellationToken token = default)
        {
            using (await LockObject.EnterReadLockAsync(token).ConfigureAwait(false))
            {
                if (_intCachedForcedLoyalty != int.MinValue)
                    return _intCachedForcedLoyalty;

                int intMaxForcedLoyalty = 0;
                foreach (Improvement objImprovement in await ImprovementManager.GetCachedImprovementListForValueOfAsync(
                                                                                   CharacterObject,
                                                                                   Improvement.ImprovementType
                                                                                       .ContactForcedLoyalty, UniqueId,
                                                                                   token: token)
                                                                               .ConfigureAwait(false))
                {
                    intMaxForcedLoyalty = Math.Max(intMaxForcedLoyalty, objImprovement.Value.StandardRound());
                }

                return _intCachedForcedLoyalty = intMaxForcedLoyalty;
            }
        }

        public Character CharacterObject
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _objCharacter;
            }
        }

        public Character LinkedCharacter
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _objLinkedCharacter;
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
                                CharacterObject.LinkedCharacters.Add(_objLinkedCharacter);
                        }
                    }

                    if (_objLinkedCharacter != objOldLinkedCharacter)
                    {
                        if (objOldLinkedCharacter != null)
                        {
                            using (objOldLinkedCharacter.LockObject.EnterWriteLock(token))
                                objOldLinkedCharacter.PropertyChanged -= LinkedCharacterOnPropertyChanged;
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
                            using (_objLinkedCharacter.LockObject.EnterUpgradeableReadLock(token))
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

                                using (_objLinkedCharacter.LockObject.EnterWriteLock(token))
                                    _objLinkedCharacter.PropertyChanged += LinkedCharacterOnPropertyChanged;
                            }
                        }

                        OnPropertyChanged(nameof(LinkedCharacter));
                    }
                }
            }
        }

        public async Task RefreshLinkedCharacterAsync(bool blnShowError = false, CancellationToken token = default)
        {
            using (await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false))
            {
                Character objOldLinkedCharacter = _objLinkedCharacter;
                IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    await CharacterObject.LinkedCharacters.RemoveAsync(_objLinkedCharacter, token)
                                         .ConfigureAwait(false);
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
                                              await LanguageManager.GetStringAsync("Message_FileNotFound", token: token).ConfigureAwait(false),
                                              FileName),
                                await LanguageManager.GetStringAsync("MessageTitle_FileNotFound", token: token).ConfigureAwait(false), MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                        }
                    }

                    if (!blnError)
                    {
                        string strFile = blnUseRelative ? Path.GetFullPath(RelativeFileName) : FileName;
                        if (strFile.EndsWith(".chum5", StringComparison.OrdinalIgnoreCase)
                            || strFile.EndsWith(".chum5lz", StringComparison.OrdinalIgnoreCase))
                        {
                            if ((_objLinkedCharacter = await Program.OpenCharacters.FirstOrDefaultAsync(x => x.FileName == strFile, token: token).ConfigureAwait(false)) == null)
                            {
                                using (ThreadSafeForm<LoadingBar> frmLoadingBar
                                       = await Program.CreateAndShowProgressBarAsync(strFile, Character.NumLoadingSections, token).ConfigureAwait(false))
                                    _objLinkedCharacter
                                        = await Program.LoadCharacterAsync(strFile, string.Empty, false, false,
                                                                           frmLoadingBar.MyForm, token).ConfigureAwait(false);
                            }

                            if (_objLinkedCharacter != null)
                                await CharacterObject.LinkedCharacters.AddAsync(_objLinkedCharacter, token).ConfigureAwait(false);
                        }
                    }

                    if (_objLinkedCharacter != objOldLinkedCharacter)
                    {
                        if (objOldLinkedCharacter != null)
                        {
                            IAsyncDisposable objLocker2 = await objOldLinkedCharacter.LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                            try
                            {
                                token.ThrowIfCancellationRequested();
                                objOldLinkedCharacter.PropertyChanged -= LinkedCharacterOnPropertyChanged;
                            }
                            finally
                            {
                                await objLocker2.DisposeAsync().ConfigureAwait(false);
                            }

                            if (await Program.OpenCharacters.ContainsAsync(objOldLinkedCharacter, token).ConfigureAwait(false))
                            {
                                if (await Program.OpenCharacters.AllAsync(async x => x == _objLinkedCharacter
                                                                              || !await x.LinkedCharacters.ContainsAsync(
                                                                                  objOldLinkedCharacter, token).ConfigureAwait(false), token: token).ConfigureAwait(false)
                                    && Program.MainForm.OpenFormsWithCharacters.All(
                                        x => !x.CharacterObjects.Contains(objOldLinkedCharacter)))
                                    await Program.OpenCharacters.RemoveAsync(objOldLinkedCharacter, token).ConfigureAwait(false);
                            }
                            else
                                await objOldLinkedCharacter.DisposeAsync().ConfigureAwait(false);
                        }

                        if (_objLinkedCharacter != null)
                        {
                            using (await _objLinkedCharacter.LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false))
                            {
                                if (string.IsNullOrEmpty(_strName)
                                    && Name != await LanguageManager.GetStringAsync("String_UnnamedCharacter", token: token).ConfigureAwait(false))
                                    _strName = Name;
                                if (string.IsNullOrEmpty(_strAge) && !string.IsNullOrEmpty(Age))
                                    _strAge = Age;
                                if (string.IsNullOrEmpty(_strGender) && !string.IsNullOrEmpty(Gender))
                                    _strGender = Gender;
                                if (string.IsNullOrEmpty(_strMetatype) && !string.IsNullOrEmpty(Metatype))
                                    _strMetatype = Metatype;

                                IAsyncDisposable objLocker2 = await _objLinkedCharacter.LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                                try
                                {
                                    token.ThrowIfCancellationRequested();
                                    _objLinkedCharacter.PropertyChanged += LinkedCharacterOnPropertyChanged;
                                }
                                finally
                                {
                                    await objLocker2.DisposeAsync().ConfigureAwait(false);
                                }
                            }
                        }

                        OnPropertyChanged(nameof(LinkedCharacter));
                    }
                }
                finally
                {
                    await objLocker.DisposeAsync().ConfigureAwait(false);
                }
            }
        }

        private void LinkedCharacterOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(Character.Name):
                    OnPropertyChanged(nameof(Name));
                    break;

                case nameof(Character.Age):
                    OnPropertyChanged(nameof(Age));
                    break;

                case nameof(Character.Gender):
                    OnPropertyChanged(nameof(Gender));
                    break;

                case nameof(Character.Metatype):
                case nameof(Character.Metavariant):
                    OnPropertyChanged(nameof(Metatype));
                    break;

                case nameof(Character.Mugshots):
                    OnPropertyChanged(nameof(Mugshots));
                    break;

                case nameof(Character.MainMugshot):
                    OnPropertyChanged(nameof(MainMugshot));
                    break;

                case nameof(Character.MainMugshotIndex):
                    OnPropertyChanged(nameof(MainMugshotIndex));
                    break;
            }
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
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (LinkedCharacter != null)
                        LinkedCharacter.MainMugshot = value;
                    else
                    {
                        if (value == null)
                        {
                            MainMugshotIndex = -1;
                            return;
                        }

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
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (LinkedCharacter != null)
                        LinkedCharacter.MainMugshotIndex = value;
                    else
                    {
                        if (value < -1 || value >= Mugshots.Count)
                            value = -1;

                        if (Interlocked.Exchange(ref _intMainMugshotIndex, value) != value)
                            OnPropertyChanged();
                    }
                }
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
            // ReSharper disable once MethodHasAsyncOverload
            using (blnSync ? LockObject.EnterReadLock(token) : await LockObject.EnterReadLockAsync(token).ConfigureAwait(false))
            {
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
                                                            MainMugshotIndex.ToString(
                                                                GlobalSettings.InvariantCultureInfo),
                                                            token: token).ConfigureAwait(false);
                    // <mugshots>
                    XmlElementWriteHelper objBaseElement
                        = await objWriter.StartElementAsync("mugshots", token: token).ConfigureAwait(false);
                    try
                    {
                        foreach (Image imgMugshot in Mugshots)
                        {
                            await objWriter.WriteElementStringAsync(
                                "mugshot",
                                await GlobalSettings.ImageToBase64StringForStorageAsync(imgMugshot, token)
                                                    .ConfigureAwait(false), token: token).ConfigureAwait(false);
                        }
                    }
                    finally
                    {
                        // </mugshots>
                        await objBaseElement.DisposeAsync().ConfigureAwait(false);
                    }
                }
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
                XPathNodeIterator xmlMugshotsList = await xmlSavedNode.SelectAndCacheExpressionAsync("mugshots/mugshot", token).ConfigureAwait(false);
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
                            = Task.Run(() => lstMugshotsBase64[iLocal].ToImageAsync(PixelFormat.Format32bppPArgb, token).AsTask(), token);
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

        public async ValueTask PrintMugshots(XmlWriter objWriter, CancellationToken token = default)
        {
            if (objWriter == null)
                return;
            using (await LockObject.EnterReadLockAsync(token).ConfigureAwait(false))
            {
                if (LinkedCharacter != null)
                    await LinkedCharacter.PrintMugshots(objWriter, token).ConfigureAwait(false);
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
                            Program.ShowScrollableMessageBox(await LanguageManager
                                                                   .GetStringAsync("Message_Insufficient_Permissions_Warning",
                                                                       token: token).ConfigureAwait(false));
                        }
                    }

                    Guid guiImage = Guid.NewGuid();
                    Image imgMainMugshot = MainMugshot;
                    if (imgMainMugshot != null)
                    {
                        string imgMugshotPath = Path.Combine(strMugshotsDirectoryPath,
                                                             guiImage.ToString("N", GlobalSettings.InvariantCultureInfo)
                                                             + ".jpg");
                        imgMainMugshot.Save(imgMugshotPath);
                        // <mainmugshotpath />
                        await objWriter.WriteElementStringAsync("mainmugshotpath",
                                                                "file://" + imgMugshotPath.Replace(
                                                                    Path.DirectorySeparatorChar, '/'), token: token)
                                       .ConfigureAwait(false);
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
                            if (i == MainMugshotIndex)
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

                                string imgMugshotPath = Path.Combine(strMugshotsDirectoryPath,
                                                                     guiImage.ToString(
                                                                         "N", GlobalSettings.InvariantCultureInfo) +
                                                                     i.ToString(GlobalSettings.InvariantCultureInfo)
                                                                     + ".jpg");
                                imgMugshot.Save(imgMugshotPath);
                                await objWriter.WriteElementStringAsync("temppath",
                                                                        "file://" + imgMugshotPath.Replace(
                                                                            Path.DirectorySeparatorChar, '/'),
                                                                        token: token)
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
            LockObject.Dispose();
        }

        /// <inheritdoc />
        public async ValueTask DisposeAsync()
        {
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync().ConfigureAwait(false);
            try
            {
                if (_objLinkedCharacter != null && !Utils.IsUnitTest
                                                && await Program.OpenCharacters.AllAsync(
                                                                    x => x == _objLinkedCharacter
                                                                         || !x.LinkedCharacters.Contains(
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

            await LockObject.DisposeAsync().ConfigureAwait(false);
        }

        #endregion IHasMugshots

        /// <inheritdoc />
        public AsyncFriendlyReaderWriterLock LockObject { get; } = new AsyncFriendlyReaderWriterLock();
    }
}
