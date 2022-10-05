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
    public sealed class Contact : INotifyMultiplePropertyChanged, IHasName, IHasMugshots, IHasNotes, IHasInternalId
    {
        private static Logger Log { get; } = LogManager.GetCurrentClassLogger();
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
        private Color _objColour;
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

                Utils.RunOnMainThread(() =>
                {
                    if (PropertyChanged != null)
                    {
                        foreach (string strPropertyToChange in setNamesOfChangedProperties)
                        {
                            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(strPropertyToChange));
                        }
                    }
                });

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
                    Utils.StringHashSetPool.Return(setNamesOfChangedProperties);
            }
        }

        private static readonly PropertyDependencyGraph<Contact> s_ContactDependencyGraph =
            new PropertyDependencyGraph<Contact>(
                new DependencyGraphNode<string, Contact>(nameof(NoLinkedCharacter),
                                                         new DependencyGraphNode<string, Contact>(
                                                             nameof(LinkedCharacter))
                ),
                new DependencyGraphNode<string, Contact>(nameof(Name),
                                                         new DependencyGraphNode<string, Contact>(
                                                             nameof(LinkedCharacter))
                ),
                new DependencyGraphNode<string, Contact>(nameof(DisplayGender),
                                                         new DependencyGraphNode<string, Contact>(nameof(Gender),
                                                                 new DependencyGraphNode<string, Contact>(
                                                                     nameof(LinkedCharacter))
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
                new DependencyGraphNode<string, Contact>(nameof(NotReadOnly),
                                                         new DependencyGraphNode<string, Contact>(nameof(ReadOnly))
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

        private static Character _objCharacterForCachedContactArchetypes;
        private static List<ListItem> _lstCachedContactArchetypes;

        public static List<ListItem> ContactArchetypes(Character objCharacter)
        {
            if (_lstCachedContactArchetypes != null && _objCharacterForCachedContactArchetypes == objCharacter
                                                    && !GlobalSettings.LiveCustomData)
                return _lstCachedContactArchetypes;
            _objCharacterForCachedContactArchetypes = objCharacter;
            if (_lstCachedContactArchetypes == null)
                _lstCachedContactArchetypes = Utils.ListItemListPool.Get();
            else
                _lstCachedContactArchetypes.Clear();
            _lstCachedContactArchetypes.Add(ListItem.Blank);
            XPathNavigator xmlContactsBaseNode = objCharacter.LoadDataXPath("contacts.xml")
                                                             .SelectSingleNodeAndCacheExpression("/chummer");
            if (xmlContactsBaseNode == null)
                return _lstCachedContactArchetypes;
            foreach (XPathNavigator xmlNode in xmlContactsBaseNode.SelectAndCacheExpression("contacts/contact"))
            {
                string strName = xmlNode.Value;
                _lstCachedContactArchetypes.Add(
                    new ListItem(strName, xmlNode.SelectSingleNodeAndCacheExpression("@translate")?.Value ?? strName));
            }

            _lstCachedContactArchetypes.Sort(CompareListItems.CompareNames);
            return _lstCachedContactArchetypes;
        }

        #endregion Helper Methods

        #region Constructor, Save, Load, and Print Methods

        public Contact(Character objCharacter, bool blnIsReadOnly = false)
        {
            _objCharacter = objCharacter;
            if (_objCharacter != null)
                _objCharacter.PropertyChanged += CharacterObjectOnPropertyChanged;
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
        public void Save(XmlWriter objWriter)
        {
            Utils.JoinableTaskFactory.Run(() => SaveCoreAsync(true, objWriter));
        }

        /// <summary>
        /// Save the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        public Task SaveAsync(XmlWriter objWriter)
        {
            return SaveCoreAsync(false, objWriter);
        }

        /// <summary>
        /// Save the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="blnSync"></param>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        private async Task SaveCoreAsync(bool blnSync, XmlWriter objWriter)
        {
            if (objWriter == null)
                return;
            if (blnSync)
            {
                // ReSharper disable MethodHasAsyncOverload
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
                    "colour", _objColour.ToArgb().ToString(GlobalSettings.InvariantCultureInfo));
                objWriter.WriteElementString("group", _blnIsGroup.ToString(GlobalSettings.InvariantCultureInfo));
                objWriter.WriteElementString("family", _blnFamily.ToString(GlobalSettings.InvariantCultureInfo));
                objWriter.WriteElementString("blackmail", _blnBlackmail.ToString(GlobalSettings.InvariantCultureInfo));
                objWriter.WriteElementString("free", _blnFree.ToString(GlobalSettings.InvariantCultureInfo));
                objWriter.WriteElementString("groupenabled",
                                             _blnGroupEnabled.ToString(GlobalSettings.InvariantCultureInfo));

                if (_blnReadOnly)
                    objWriter.WriteElementString("readonly", string.Empty);

                if (_strUnique != null)
                    objWriter.WriteElementString("guid", _strUnique);

                SaveMugshots(objWriter);

                objWriter.WriteEndElement();
                // ReSharper restore MethodHasAsyncOverload
            }
            else
            {
                // <contact>
                XmlElementWriteHelper objBaseElement = await objWriter.StartElementAsync("contact").ConfigureAwait(false);
                try
                {
                    await objWriter.WriteElementStringAsync("name", _strName).ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("role", _strRole).ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("location", _strLocation).ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("connection",
                                                            _intConnection.ToString(
                                                                GlobalSettings.InvariantCultureInfo)).ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync(
                        "loyalty", _intLoyalty.ToString(GlobalSettings.InvariantCultureInfo)).ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("metatype", _strMetatype).ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("gender", _strGender).ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("age", _strAge).ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("contacttype", _strType).ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("preferredpayment", _strPreferredPayment).ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("hobbiesvice", _strHobbiesVice).ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("personallife", _strPersonalLife).ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("type", _eContactType.ToString()).ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("file", _strFileName).ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("relative", _strRelativeName).ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("notes", _strNotes.CleanOfInvalidUnicodeChars()).ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("notesColor", ColorTranslator.ToHtml(_colNotes)).ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("groupname", _strGroupName).ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync(
                        "colour", _objColour.ToArgb().ToString(GlobalSettings.InvariantCultureInfo)).ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync(
                        "group", _blnIsGroup.ToString(GlobalSettings.InvariantCultureInfo)).ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync(
                        "family", _blnFamily.ToString(GlobalSettings.InvariantCultureInfo)).ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("blackmail",
                                                            _blnBlackmail.ToString(
                                                                GlobalSettings.InvariantCultureInfo)).ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync(
                        "free", _blnFree.ToString(GlobalSettings.InvariantCultureInfo)).ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("groupenabled",
                                                            _blnGroupEnabled.ToString(
                                                                GlobalSettings.InvariantCultureInfo)).ConfigureAwait(false);

                    if (_blnReadOnly)
                        await objWriter.WriteElementStringAsync("readonly", string.Empty).ConfigureAwait(false);

                    if (_strUnique != null)
                    {
                        await objWriter.WriteElementStringAsync("guid", _strUnique).ConfigureAwait(false);
                    }

                    await SaveMugshotsAsync(objWriter).ConfigureAwait(false);
                }
                finally
                {
                    // </contact>
                    await objBaseElement.DisposeAsync().ConfigureAwait(false);
                }
            }
        }

        /// <summary>
        /// Load the Contact from the XmlNode.
        /// </summary>
        /// <param name="objNode">XmlNode to load.</param>
        public void Load(XPathNavigator objNode)
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
            if (objNode.SelectSingleNodeAndCacheExpression("colour") != null)
            {
                int intTmp = _objColour.ToArgb();
                if (objNode.TryGetInt32FieldQuickly("colour", ref intTmp))
                    _objColour = Color.FromArgb(intTmp);
            }

            _blnReadOnly = objNode.SelectSingleNodeAndCacheExpression("readonly") != null;

            if (!objNode.TryGetBoolFieldQuickly("groupenabled", ref _blnGroupEnabled))
            {
                objNode.TryGetBoolFieldQuickly("mademan", ref _blnGroupEnabled);
            }

            RefreshLinkedCharacter();

            // Mugshots
            LoadMugshots(objNode);
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
            // <contact>
            XmlElementWriteHelper objBaseElement = await objWriter.StartElementAsync("contact", token: token).ConfigureAwait(false);
            try
            {
                await objWriter.WriteElementStringAsync("guid", InternalId, token: token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("name", Name, token: token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("role", await DisplayRoleMethodAsync(strLanguageToPrint, token).ConfigureAwait(false),
                                                        token: token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("location", Location, token: token).ConfigureAwait(false);
                if (!IsGroup)
                    await objWriter.WriteElementStringAsync("connection",
                                                            (await GetConnectionAsync(token).ConfigureAwait(false)).ToString(objCulture),
                                                            token: token).ConfigureAwait(false);
                else
                    await objWriter.WriteElementStringAsync("connection",
                                                            await LanguageManager.GetStringAsync(
                                                                "String_Group", strLanguageToPrint, token: token).ConfigureAwait(false) + '('
                                                            + (await GetConnectionAsync(token).ConfigureAwait(false)).ToString(objCulture)
                                                            + ')', token: token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("loyalty", (await GetLoyaltyAsync(token).ConfigureAwait(false)).ToString(objCulture),
                                                        token: token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync(
                    "metatype", await DisplayMetatypeMethodAsync(strLanguageToPrint, token).ConfigureAwait(false), token: token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync(
                    "gender", await DisplayGenderMethodAsync(strLanguageToPrint, token).ConfigureAwait(false),
                    token: token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("age", await DisplayAgeMethodAsync(strLanguageToPrint, token).ConfigureAwait(false),
                                                        token: token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("contacttype",
                                                        await DisplayTypeMethodAsync(strLanguageToPrint, token).ConfigureAwait(false),
                                                        token: token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("preferredpayment",
                                                        await DisplayPreferredPaymentMethodAsync(
                                                            strLanguageToPrint, token).ConfigureAwait(false),
                                                        token: token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("hobbiesvice",
                                                        await DisplayHobbiesViceMethodAsync(strLanguageToPrint, token).ConfigureAwait(false),
                                                        token: token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("personallife",
                                                        await DisplayPersonalLifeMethodAsync(strLanguageToPrint, token).ConfigureAwait(false),
                                                        token: token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync(
                    "type",
                    await LanguageManager.GetStringAsync("String_" + EntityType, strLanguageToPrint, token: token).ConfigureAwait(false),
                    token: token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("forcedloyalty",
                                                        (await GetForcedLoyaltyAsync(token).ConfigureAwait(false)).ToString(objCulture),
                                                        token: token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("blackmail",
                                                        Blackmail.ToString(GlobalSettings.InvariantCultureInfo),
                                                        token: token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("family", Family.ToString(GlobalSettings.InvariantCultureInfo),
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

        #endregion Constructor, Save, Load, and Print Methods

        #region Properties

        public bool ReadOnly => _blnReadOnly;

        public bool NotReadOnly => !ReadOnly;

        /// <summary>
        /// Total points used for this contact.
        /// </summary>
        public int ContactPoints
        {
            get
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

        /// <summary>
        /// Name of the Contact.
        /// </summary>
        public string Name
        {
            get => LinkedCharacter != null ? LinkedCharacter.CharacterName : _strName;
            set
            {
                if (_strName != value)
                {
                    _strName = value;
                    OnPropertyChanged();
                }
            }
        }

        public string DisplayRoleMethod(string strLanguage)
        {
            if (strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                return Role;

            return _objCharacter.LoadDataXPath("contacts.xml", strLanguage)
                                .SelectSingleNode("/chummer/contacts/contact[. = " + Role.CleanXPath() + "]/@translate")
                                ?.Value ?? Role;
        }

        public async Task<string> DisplayRoleMethodAsync(string strLanguage, CancellationToken token = default)
        {
            if (strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                return Role;

            return (await _objCharacter.LoadDataXPathAsync("contacts.xml", strLanguage, token: token).ConfigureAwait(false))
                   .SelectSingleNode("/chummer/contacts/contact[. = " + Role.CleanXPath() + "]/@translate")
                   ?.Value ?? Role;
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
            get => _strRole;
            set
            {
                if (_strRole != value)
                {
                    _strRole = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Location of the Contact.
        /// </summary>
        public string Location
        {
            get => _strLocation;
            set
            {
                if (_strLocation != value)
                {
                    _strLocation = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Contact's Connection Rating.
        /// </summary>
        public int Connection
        {
            get => Math.Min(_intConnection, ConnectionMaximum);
            set
            {
                value = Math.Max(Math.Min(value, ConnectionMaximum), 1);
                if (_intConnection != value)
                {
                    _intConnection = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Contact's Connection Rating.
        /// </summary>
        public async ValueTask<int> GetConnectionAsync(CancellationToken token = default)
        {
            return Math.Min(_intConnection, await GetConnectionMaximumAsync(token).ConfigureAwait(false));
        }

        /// <summary>
        /// Contact's Loyalty Rating (or Enemy's Incidence Rating).
        /// </summary>
        public int Loyalty
        {
            get
            {
                if (ForcedLoyalty > 0)
                    return ForcedLoyalty;
                return IsGroup ? 1 : _intLoyalty;
            }
            set
            {
                if (_intLoyalty != value)
                {
                    _intLoyalty = Math.Max(value, 1);
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Contact's Loyalty Rating (or Enemy's Incidence Rating).
        /// </summary>
        public async ValueTask<int> GetLoyaltyAsync(CancellationToken token = default)
        {
            int intForced = await GetForcedLoyaltyAsync(token).ConfigureAwait(false);
            if (intForced > 0)
                return intForced;
            return IsGroup ? 1 : _intLoyalty;
        }

        public string DisplayMetatypeMethod(string strLanguage)
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
                objMetatypeNode = objMetatypeNode
                    .SelectSingleNode("metavariants/metavariant[id = " + LinkedCharacter.MetavariantGuid
                                          .ToString("D", GlobalSettings.InvariantCultureInfo).CleanXPath() + ']');

                string strMetatypeTranslate = objMetatypeNode?.SelectSingleNodeAndCacheExpression("translate")?.Value;
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

        public async Task<string> DisplayMetatypeMethodAsync(string strLanguage, CancellationToken token = default)
        {
            string strReturn = Metatype;
            if (LinkedCharacter != null)
            {
                // Update character information fields.
                XPathNavigator objMetatypeNode = await _objCharacter.GetNodeXPathAsync(true, token: token).ConfigureAwait(false);

                strReturn
                    = (await objMetatypeNode.SelectSingleNodeAndCacheExpressionAsync("translate", token: token).ConfigureAwait(false))?.Value
                      ?? await _objCharacter.TranslateExtraAsync(await LinkedCharacter.GetMetatypeAsync(token).ConfigureAwait(false),
                                                                 strLanguage, "metatypes.xml", token).ConfigureAwait(false);

                Guid guiMetavariant = await LinkedCharacter.GetMetavariantGuidAsync(token).ConfigureAwait(false);
                if (guiMetavariant == Guid.Empty)
                    return strReturn;
                objMetatypeNode = objMetatypeNode
                    .SelectSingleNode("metavariants/metavariant[id = "
                                      + guiMetavariant.ToString("D", GlobalSettings.InvariantCultureInfo).CleanXPath()
                                      + ']');

                string strMetatypeTranslate = objMetatypeNode != null
                    ? (await objMetatypeNode.SelectSingleNodeAndCacheExpressionAsync("translate", token: token).ConfigureAwait(false))?.Value
                    : null;
                strReturn += await LanguageManager.GetStringAsync("String_Space", strLanguage, token: token).ConfigureAwait(false)
                             + '('
                             + (!string.IsNullOrEmpty(strMetatypeTranslate)
                                 ? strMetatypeTranslate
                                 : await _objCharacter.TranslateExtraAsync(
                                     await LinkedCharacter.GetMetavariantAsync(token).ConfigureAwait(false), strLanguage, "metatypes.xml",
                                     token).ConfigureAwait(false))
                             + ')';
            }
            else
                strReturn = await _objCharacter.TranslateExtraAsync(strReturn, strLanguage, "metatypes.xml", token).ConfigureAwait(false);

            return strReturn;
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
            set
            {
                if (_strMetatype != value)
                {
                    _strMetatype = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Metatype of this Contact.
        /// </summary>
        public async ValueTask<string> GetMetatypeAsync(CancellationToken token = default)
        {
            if (LinkedCharacter != null)
            {
                string strMetatype = await LinkedCharacter.GetMetatypeAsync(token).ConfigureAwait(false);
                string strMetavariant = await LinkedCharacter.GetMetavariantAsync(token).ConfigureAwait(false);

                if (!string.IsNullOrEmpty(strMetavariant) && strMetavariant != "None")
                {
                    strMetatype += await LanguageManager.GetStringAsync("String_Space", token: token).ConfigureAwait(false) + '('
                        + strMetavariant + ')';
                }

                return strMetatype;
            }

            return _strMetatype;
        }

        public string DisplayGenderMethod(string strLanguage)
        {
            if (strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                return Gender;

            return _objCharacter.LoadDataXPath("contacts.xml", strLanguage)
                                .SelectSingleNode("/chummer/genders/gender[. = " + Gender.CleanXPath() + "]/@translate")
                                ?.Value ?? Gender;
        }

        public async Task<string> DisplayGenderMethodAsync(string strLanguage, CancellationToken token = default)
        {
            if (strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                return Gender;

            return (await _objCharacter.LoadDataXPathAsync("contacts.xml", strLanguage, token: token).ConfigureAwait(false))
                   .SelectSingleNode("/chummer/genders/gender[. = " + Gender.CleanXPath() + "]/@translate")?.Value
                   ?? Gender;
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
            get => LinkedCharacter != null ? LinkedCharacter.Gender : _strGender;
            set
            {
                if (_strGender == value)
                    return;
                _strGender = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gender of this Contact.
        /// </summary>
        public async ValueTask<string> GetGenderAsync(CancellationToken token = default)
        {
            return LinkedCharacter != null ? await LinkedCharacter.GetGenderAsync(token).ConfigureAwait(false) : _strGender;
        }

        public string DisplayAgeMethod(string strLanguage)
        {
            if (strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                return Age;

            return _objCharacter.LoadDataXPath("contacts.xml", strLanguage).SelectSingleNode("/chummer/ages/age[. = " + Age.CleanXPath() + "]/@translate")?.Value ?? Age;
        }

        public async Task<string> DisplayAgeMethodAsync(string strLanguage, CancellationToken token = default)
        {
            if (strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                return Age;

            return (await _objCharacter.LoadDataXPathAsync("contacts.xml", strLanguage, token: token).ConfigureAwait(false)).SelectSingleNode("/chummer/ages/age[. = " + Age.CleanXPath() + "]/@translate")?.Value ?? Age;
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
            Age = await _objCharacter.ReverseTranslateExtraAsync(value, GlobalSettings.Language, "contacts.xml", token).ConfigureAwait(false);
        }

        /// <summary>
        /// How old is this Contact.
        /// </summary>
        public string Age
        {
            get => LinkedCharacter != null ? LinkedCharacter.Age : _strAge;
            set
            {
                if (_strAge == value)
                    return;
                _strAge = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// How old is this Contact.
        /// </summary>
        public async ValueTask<string> GetAgeAsync(CancellationToken token = default)
        {
            return LinkedCharacter != null ? await LinkedCharacter.GetAgeAsync(token).ConfigureAwait(false) : _strGender;
        }

        public string DisplayTypeMethod(string strLanguage)
        {
            if (strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                return Type;

            return _objCharacter.LoadDataXPath("contacts.xml", strLanguage).SelectSingleNode("/chummer/types/type[. = " + Type.CleanXPath() + "]/@translate")?.Value ?? Type;
        }

        public async Task<string> DisplayTypeMethodAsync(string strLanguage, CancellationToken token = default)
        {
            if (strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                return Type;

            return (await _objCharacter.LoadDataXPathAsync("contacts.xml", strLanguage, token: token).ConfigureAwait(false)).SelectSingleNode("/chummer/types/type[. = " + Type.CleanXPath() + "]/@translate")?.Value ?? Type;
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
            Type = await _objCharacter.ReverseTranslateExtraAsync(value, GlobalSettings.Language, "contacts.xml", token).ConfigureAwait(false);
        }

        /// <summary>
        /// What type of Contact is this.
        /// </summary>
        public string Type
        {
            get => _strType;
            set
            {
                if (_strType == value)
                    return;
                _strType = value;
                OnPropertyChanged();
            }
        }

        public string DisplayPreferredPaymentMethod(string strLanguage)
        {
            if (strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                return PreferredPayment;

            return _objCharacter.LoadDataXPath("contacts.xml", strLanguage)
                .SelectSingleNode("/chummer/preferredpayments/preferredpayment[. = " + PreferredPayment.CleanXPath() + "]/@translate")?.Value ?? PreferredPayment;
        }

        public async Task<string> DisplayPreferredPaymentMethodAsync(string strLanguage, CancellationToken token = default)
        {
            if (strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                return PreferredPayment;

            return (await _objCharacter.LoadDataXPathAsync("contacts.xml", strLanguage, token: token).ConfigureAwait(false))
                                .SelectSingleNode("/chummer/preferredpayments/preferredpayment[. = " + PreferredPayment.CleanXPath() + "]/@translate")?.Value ?? PreferredPayment;
        }

        public string DisplayPreferredPayment
        {
            get => DisplayPreferredPaymentMethod(GlobalSettings.Language);
            set => PreferredPayment = _objCharacter.ReverseTranslateExtra(value, GlobalSettings.Language, "contacts.xml");
        }

        public Task<string> GetDisplayPreferredPaymentAsync(CancellationToken token = default) =>
            DisplayPreferredPaymentMethodAsync(GlobalSettings.Language, token);

        public async ValueTask SetDisplayPreferredPaymentAsync(string value, CancellationToken token = default)
        {
            PreferredPayment = await _objCharacter.ReverseTranslateExtraAsync(value, GlobalSettings.Language, "contacts.xml", token).ConfigureAwait(false);
        }

        /// <summary>
        /// Preferred payment method of this Contact.
        /// </summary>
        public string PreferredPayment
        {
            get => _strPreferredPayment;
            set
            {
                if (_strPreferredPayment == value)
                    return;
                _strPreferredPayment = value;
                OnPropertyChanged();
            }
        }

        public string DisplayHobbiesViceMethod(string strLanguage)
        {
            if (strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                return HobbiesVice;

            return _objCharacter.LoadDataXPath("contacts.xml", strLanguage).SelectSingleNode("/chummer/hobbiesvices/hobbyvice[. = " + HobbiesVice.CleanXPath() + "]/@translate")?.Value
                   ?? HobbiesVice;
        }

        public async Task<string> DisplayHobbiesViceMethodAsync(string strLanguage, CancellationToken token = default)
        {
            if (strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                return HobbiesVice;

            return (await _objCharacter.LoadDataXPathAsync("contacts.xml", strLanguage, token: token).ConfigureAwait(false)).SelectSingleNode("/chummer/hobbiesvices/hobbyvice[. = " + HobbiesVice.CleanXPath() + "]/@translate")?.Value
                   ?? HobbiesVice;
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
            HobbiesVice = await _objCharacter.ReverseTranslateExtraAsync(value, GlobalSettings.Language, "contacts.xml", token).ConfigureAwait(false);
        }

        /// <summary>
        /// Hobbies/Vice of this Contact.
        /// </summary>
        public string HobbiesVice
        {
            get => _strHobbiesVice;
            set
            {
                if (_strHobbiesVice == value)
                    return;
                _strHobbiesVice = value;
                OnPropertyChanged();
            }
        }

        public string DisplayPersonalLifeMethod(string strLanguage)
        {
            if (strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                return PersonalLife;

            return _objCharacter.LoadDataXPath("contacts.xml", strLanguage).SelectSingleNode("/chummer/personallives/personallife[. = " + PersonalLife.CleanXPath() + "]/@translate")?.Value
                   ?? PersonalLife;
        }

        public async Task<string> DisplayPersonalLifeMethodAsync(string strLanguage, CancellationToken token = default)
        {
            if (strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                return PersonalLife;

            return (await _objCharacter.LoadDataXPathAsync("contacts.xml", strLanguage, token: token).ConfigureAwait(false)).SelectSingleNode("/chummer/personallives/personallife[. = " + PersonalLife.CleanXPath() + "]/@translate")?.Value
                   ?? PersonalLife;
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
            PersonalLife = await _objCharacter.ReverseTranslateExtraAsync(value, GlobalSettings.Language, "contacts.xml", token).ConfigureAwait(false);
        }

        /// <summary>
        /// Personal Life of this Contact.
        /// </summary>
        public string PersonalLife
        {
            get => _strPersonalLife;
            set
            {
                if (_strPersonalLife == value)
                    return;
                _strPersonalLife = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Is this contact a group contact?
        /// </summary>
        public bool IsGroup
        {
            get => _blnIsGroup;
            set
            {
                if (_blnIsGroup == value)
                    return;
                _blnIsGroup = value;
                OnPropertyChanged();
            }
        }

        public bool LoyaltyEnabled => !ReadOnly && !IsGroup && ForcedLoyalty <= 0;

        public async ValueTask<bool> GetLoyaltyEnabledAsync(CancellationToken token = default)
        {
            return !ReadOnly && !IsGroup && await GetForcedLoyaltyAsync(token).ConfigureAwait(false) <= 0;
        }

        public int ConnectionMaximum => CharacterObject.Created || CharacterObject.FriendsInHighPlaces ? 12 : 6;

        public async ValueTask<int> GetConnectionMaximumAsync(CancellationToken token = default)
        {
            return await CharacterObject.GetCreatedAsync(token).ConfigureAwait(false) || await CharacterObject.GetFriendsInHighPlacesAsync(token).ConfigureAwait(false)
                ? 12
                : 6;
        }

        public string QuickText => string.Format(GlobalSettings.CultureInfo, IsGroup ? "({0}/{1}G)" : "({0}/{1})", Connection, Loyalty);

        public async ValueTask<string> GetQuickTextAsync(CancellationToken token = default)
        {
            return string.Format(GlobalSettings.CultureInfo, IsGroup ? "({0}/{1}G)" : "({0}/{1})", await GetConnectionAsync(token).ConfigureAwait(false), await GetLoyaltyAsync(token).ConfigureAwait(false));
        }

        /// <summary>
        /// The Contact's type, either Contact or Enemy.
        /// </summary>
        public ContactType EntityType
        {
            get => _eContactType;
            set
            {
                if (_eContactType != value)
                {
                    _eContactType = value;
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
            get => _strFileName;
            set
            {
                if (_strFileName != value)
                {
                    _strFileName = value;
                    RefreshLinkedCharacter(!string.IsNullOrEmpty(value));
                }
            }
        }

        /// <summary>
        /// Relative path to the save file.
        /// </summary>
        public string RelativeFileName
        {
            get => _strRelativeName;
            set
            {
                if (_strRelativeName != value)
                {
                    _strRelativeName = value;
                    RefreshLinkedCharacter(!string.IsNullOrEmpty(value));
                }
            }
        }

        /// <summary>
        /// Notes.
        /// </summary>
        public string Notes
        {
            get => _strNotes;
            set
            {
                if (_strNotes != value)
                {
                    _strNotes = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Forecolor to use for Notes in treeviews.
        /// </summary>
        public Color NotesColor
        {
            get => _colNotes;
            set => _colNotes = value;
        }

        /// <summary>
        /// Group Name.
        /// </summary>
        public string GroupName
        {
            get => _strGroupName;
            set
            {
                if (_strGroupName != value)
                {
                    _strGroupName = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Contact Color.
        /// </summary>
        public Color PreferredColor
        {
            get => _objColour;
            set
            {
                if (_objColour != value)
                {
                    _objColour = value;
                    OnPropertyChanged();
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
                if (_blnFree)
                    return _blnFree;

                if (_intCachedFreeFromImprovement < 0)
                {
                    _intCachedFreeFromImprovement = ImprovementManager
                                                    .GetCachedImprovementListForValueOf(
                                                        CharacterObject, Improvement.ImprovementType.ContactMakeFree,
                                                        UniqueId).Count > 0
                        ? 1
                        : 0;
                }

                return _intCachedFreeFromImprovement > 0;
            }
            set => _blnFree = value;
        }

        /// <summary>
        /// Whether or not this is a free contact.
        /// </summary>
        public async ValueTask<bool> GetFreeAsync(CancellationToken token = default)
        {
            if (_blnFree)
                return _blnFree;

            if (_intCachedFreeFromImprovement < 0)
            {
                _intCachedFreeFromImprovement = (await ImprovementManager
                                                       .GetCachedImprovementListForValueOfAsync(
                                                           CharacterObject, Improvement.ImprovementType.ContactMakeFree,
                                                           UniqueId, token: token).ConfigureAwait(false)).Count > 0
                    ? 1
                    : 0;
            }

            return _intCachedFreeFromImprovement > 0;
        }

        public bool FreeEnabled
        {
            get
            {
                if (_intCachedFreeFromImprovement < 0)
                {
                    _intCachedFreeFromImprovement = ImprovementManager
                                                    .GetCachedImprovementListForValueOf(
                                                        CharacterObject, Improvement.ImprovementType.ContactMakeFree,
                                                        UniqueId).Count > 0
                        ? 1
                        : 0;
                }

                return _intCachedFreeFromImprovement < 1;
            }
        }

        public async ValueTask<bool> GetFreeEnabledAsync(CancellationToken token = default)
        {
            if (_intCachedFreeFromImprovement < 0)
            {
                _intCachedFreeFromImprovement = (await ImprovementManager
                                                       .GetCachedImprovementListForValueOfAsync(
                                                           CharacterObject, Improvement.ImprovementType.ContactMakeFree,
                                                           UniqueId, token: token).ConfigureAwait(false)).Count > 0
                    ? 1
                    : 0;
            }

            return _intCachedFreeFromImprovement < 1;
        }

        /// <summary>
        /// Unique ID for this contact
        /// </summary>
        public string UniqueId => _strUnique;

        public string InternalId => UniqueId;

        private int _intCachedGroupEnabled = -1;

        /// <summary>
        /// Whether or not the contact's group status can be modified through the UI
        /// </summary>
        public bool GroupEnabled
        {
            get
            {
                if (_intCachedGroupEnabled < 0)
                {
                    _intCachedGroupEnabled = !ReadOnly && ImprovementManager
                                                          .GetCachedImprovementListForValueOf(
                                                              CharacterObject,
                                                              Improvement.ImprovementType.ContactForceGroup, UniqueId)
                                                          .Count == 0
                        ? 1
                        : 0;
                }

                return _intCachedGroupEnabled > 0;
            }
        }

        /// <summary>
        /// Whether or not the contact's group status can be modified through the UI
        /// </summary>
        public async ValueTask<bool> GetGroupEnabledAsync(CancellationToken token = default)
        {
            if (_intCachedGroupEnabled < 0)
            {
                _intCachedGroupEnabled = !ReadOnly && (await ImprovementManager
                                                             .GetCachedImprovementListForValueOfAsync(
                                                                 CharacterObject,
                                                                 Improvement.ImprovementType.ContactForceGroup, UniqueId, token: token).ConfigureAwait(false))
                                                      .Count == 0
                    ? 1
                    : 0;
            }

            return _intCachedGroupEnabled > 0;
        }

        public bool Blackmail
        {
            get => _blnBlackmail;
            set
            {
                if (_blnBlackmail != value)
                {
                    _blnBlackmail = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool Family
        {
            get => _blnFamily;
            set
            {
                if (_blnFamily != value)
                {
                    _blnFamily = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _intCachedForcedLoyalty = int.MinValue;

        public int ForcedLoyalty
        {
            get
            {
                if (_intCachedForcedLoyalty != int.MinValue)
                    return _intCachedForcedLoyalty;

                int intMaxForcedLoyalty = 0;
                foreach (Improvement objImprovement in ImprovementManager.GetCachedImprovementListForValueOf(CharacterObject, Improvement.ImprovementType.ContactForcedLoyalty, UniqueId))
                {
                    intMaxForcedLoyalty = Math.Max(intMaxForcedLoyalty, objImprovement.Value.StandardRound());
                }

                return _intCachedForcedLoyalty = intMaxForcedLoyalty;
            }
        }

        public async ValueTask<int> GetForcedLoyaltyAsync(CancellationToken token = default)
        {
            if (_intCachedForcedLoyalty != int.MinValue)
                return _intCachedForcedLoyalty;

            int intMaxForcedLoyalty = 0;
            foreach (Improvement objImprovement in await ImprovementManager.GetCachedImprovementListForValueOfAsync(
                         CharacterObject, Improvement.ImprovementType.ContactForcedLoyalty, UniqueId, token: token).ConfigureAwait(false))
            {
                intMaxForcedLoyalty = Math.Max(intMaxForcedLoyalty, objImprovement.Value.StandardRound());
            }

            return _intCachedForcedLoyalty = intMaxForcedLoyalty;
        }

        public Character CharacterObject => _objCharacter;

        public Character LinkedCharacter => _objLinkedCharacter;

        public bool NoLinkedCharacter => _objLinkedCharacter == null;

        public void RefreshLinkedCharacter(bool blnShowError = false)
        {
            Character objOldLinkedCharacter = _objLinkedCharacter;
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
                    Program.ShowMessageBox(string.Format(GlobalSettings.CultureInfo, LanguageManager.GetString("Message_FileNotFound"), FileName),
                        LanguageManager.GetString("MessageTitle_FileNotFound"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            if (!blnError)
            {
                string strFile = blnUseRelative ? Path.GetFullPath(RelativeFileName) : FileName;
                if (strFile.EndsWith(".chum5", StringComparison.OrdinalIgnoreCase) || strFile.EndsWith(".chum5lz", StringComparison.OrdinalIgnoreCase))
                {
                    _objLinkedCharacter = Program.OpenCharacters.Find(x => x.FileName == strFile);
                    if (_objLinkedCharacter == null)
                    {
                        using (ThreadSafeForm<LoadingBar> frmLoadingBar = Program.CreateAndShowProgressBar(strFile, Character.NumLoadingSections))
                            _objLinkedCharacter = Program.LoadCharacter(strFile, string.Empty, false, false, frmLoadingBar.MyForm);
                    }
                    if (_objLinkedCharacter != null)
                        CharacterObject.LinkedCharacters.Add(_objLinkedCharacter);
                }
            }
            if (_objLinkedCharacter != objOldLinkedCharacter)
            {
                if (objOldLinkedCharacter != null)
                {
                    objOldLinkedCharacter.PropertyChanged -= LinkedCharacterOnPropertyChanged;
                    if (Program.OpenCharacters.Contains(objOldLinkedCharacter))
                    {
                        if (Program.OpenCharacters.All(x => x == _objLinkedCharacter || !x.LinkedCharacters.Contains(objOldLinkedCharacter))
                            && Program.MainForm.OpenFormsWithCharacters.All(x => !x.CharacterObjects.Contains(objOldLinkedCharacter)))
                            Program.OpenCharacters.Remove(objOldLinkedCharacter);
                    }
                    else
                        objOldLinkedCharacter.Dispose();
                }
                if (_objLinkedCharacter != null)
                {
                    if (string.IsNullOrEmpty(_strName) && Name != LanguageManager.GetString("String_UnnamedCharacter"))
                        _strName = Name;
                    if (string.IsNullOrEmpty(_strAge) && !string.IsNullOrEmpty(Age))
                        _strAge = Age;
                    if (string.IsNullOrEmpty(_strGender) && !string.IsNullOrEmpty(Gender))
                        _strGender = Gender;
                    if (string.IsNullOrEmpty(_strMetatype) && !string.IsNullOrEmpty(Metatype))
                        _strMetatype = Metatype;

                    _objLinkedCharacter.PropertyChanged += LinkedCharacterOnPropertyChanged;
                }
                OnPropertyChanged(nameof(LinkedCharacter));
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
                using (EnterReadLock.Enter(CharacterObject.LockObject))
                {
                    return LinkedCharacter != null ? LinkedCharacter.Mugshots : _lstMugshots;
                }
            }
        }

        /// <summary>
        /// Character's main portrait encoded using Base64.
        /// </summary>
        public Image MainMugshot
        {
            get
            {
                if (LinkedCharacter != null)
                    return LinkedCharacter.MainMugshot;
                if (MainMugshotIndex >= Mugshots.Count || MainMugshotIndex < 0)
                    return null;
                return Mugshots[MainMugshotIndex];
            }
            set
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
                        Mugshots.Add(value);
                        MainMugshotIndex = Mugshots.Count - 1;
                    }
                }
            }
        }

        /// <summary>
        /// Index of Character's main portrait. -1 if set to none.
        /// </summary>
        public int MainMugshotIndex
        {
            get => LinkedCharacter?.MainMugshotIndex ?? _intMainMugshotIndex;
            set
            {
                if (LinkedCharacter != null)
                    LinkedCharacter.MainMugshotIndex = value;
                else
                {
                    if (value < -1)
                        value = -1;
                    else if (value >= 0)
                    {
                        using (EnterReadLock.Enter(_objCharacter.LockObject))
                        {
                            if (value >= Mugshots.Count)
                                value = -1;
                        }
                    }

                    using (EnterReadLock.Enter(_objCharacter.LockObject))
                    {
                        if (_intMainMugshotIndex == value)
                            return;
                        using (_objCharacter.LockObject.EnterWriteLock())
                        {
                            _intMainMugshotIndex = value;
                            OnPropertyChanged();
                        }
                    }
                }
            }
        }

        public void SaveMugshots(XmlWriter objWriter, CancellationToken token = default)
        {
            Utils.JoinableTaskFactory.Run(() => SaveMugshotsCore(true, objWriter, token));
        }

        public Task SaveMugshotsAsync(XmlWriter objWriter, CancellationToken token = default)
        {
            return SaveMugshotsCore(false, objWriter, token);
        }

        public async Task SaveMugshotsCore(bool blnSync, XmlWriter objWriter, CancellationToken token = default)
        {
            if (objWriter == null)
                return;
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
                                                        MainMugshotIndex.ToString(GlobalSettings.InvariantCultureInfo), token: token).ConfigureAwait(false);
                // <mugshots>
                XmlElementWriteHelper objBaseElement = await objWriter.StartElementAsync("mugshots", token: token).ConfigureAwait(false);
                try
                {
                    foreach (Image imgMugshot in Mugshots)
                    {
                        await objWriter.WriteElementStringAsync(
                            "mugshot", await GlobalSettings.ImageToBase64StringForStorageAsync(imgMugshot, token).ConfigureAwait(false), token: token).ConfigureAwait(false);
                    }
                }
                finally
                {
                    // </mugshots>
                    await objBaseElement.DisposeAsync().ConfigureAwait(false);
                }
            }
        }

        public void LoadMugshots(XPathNavigator xmlSavedNode)
        {
            xmlSavedNode.TryGetInt32FieldQuickly("mainmugshotindex", ref _intMainMugshotIndex);
            XPathNodeIterator xmlMugshotsList = xmlSavedNode.SelectAndCacheExpression("mugshots/mugshot");
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

        public async ValueTask PrintMugshots(XmlWriter objWriter, CancellationToken token = default)
        {
            if (objWriter == null)
                return;
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
                        Program.ShowMessageBox(await LanguageManager.GetStringAsync("Message_Insufficient_Permissions_Warning", token: token).ConfigureAwait(false));
                    }
                }
                Guid guiImage = Guid.NewGuid();
                Image imgMainMugshot = MainMugshot;
                if (imgMainMugshot != null)
                {
                    string imgMugshotPath = Path.Combine(strMugshotsDirectoryPath,
                        guiImage.ToString("N", GlobalSettings.InvariantCultureInfo) + ".jpg");
                    imgMainMugshot.Save(imgMugshotPath);
                    // <mainmugshotpath />
                    await objWriter.WriteElementStringAsync("mainmugshotpath",
                                                            "file://" + imgMugshotPath.Replace(Path.DirectorySeparatorChar, '/'), token: token).ConfigureAwait(false);
                    // <mainmugshotbase64 />
                    await objWriter.WriteElementStringAsync("mainmugshotbase64", await imgMainMugshot.ToBase64StringAsJpegAsync(token: token).ConfigureAwait(false), token: token).ConfigureAwait(false);
                }
                // <hasothermugshots>
                await objWriter.WriteElementStringAsync("hasothermugshots",
                                                        (imgMainMugshot == null || await Mugshots.GetCountAsync(token).ConfigureAwait(false) > 1).ToString(GlobalSettings.InvariantCultureInfo), token: token).ConfigureAwait(false);
                // <othermugshots>
                XmlElementWriteHelper objOtherMugshotsElement = await objWriter.StartElementAsync("othermugshots", token: token).ConfigureAwait(false);
                try
                {
                    for (int i = 0; i < await Mugshots.GetCountAsync(token).ConfigureAwait(false); ++i)
                    {
                        if (i == MainMugshotIndex)
                            continue;
                        Image imgMugshot = await Mugshots.GetValueAtAsync(i, token).ConfigureAwait(false);
                        // <mugshot>
                        XmlElementWriteHelper objMugshotElement = await objWriter.StartElementAsync("mugshot", token: token).ConfigureAwait(false);
                        try
                        {
                            await objWriter.WriteElementStringAsync("stringbase64", await imgMugshot.ToBase64StringAsJpegAsync(token: token).ConfigureAwait(false), token: token).ConfigureAwait(false);

                            string imgMugshotPath = Path.Combine(strMugshotsDirectoryPath,
                                                                 guiImage.ToString("N", GlobalSettings.InvariantCultureInfo) +
                                                                 i.ToString(GlobalSettings.InvariantCultureInfo) + ".jpg");
                            imgMugshot.Save(imgMugshotPath);
                            await objWriter.WriteElementStringAsync("temppath",
                                                                    "file://" + imgMugshotPath.Replace(Path.DirectorySeparatorChar, '/'), token: token).ConfigureAwait(false);
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

        public void Dispose()
        {
            if (_lstCachedContactArchetypes != null)
                Utils.ListItemListPool.Return(_lstCachedContactArchetypes);
            if (_objLinkedCharacter != null && !Utils.IsUnitTest
                                            && Program.OpenCharacters.All(x => x == _objLinkedCharacter || !x.LinkedCharacters.Contains(_objLinkedCharacter))
                                            && Program.MainForm.OpenFormsWithCharacters.All(x => !x.CharacterObjects.Contains(_objLinkedCharacter)))
                Program.OpenCharacters.Remove(_objLinkedCharacter);
            foreach (Image imgMugshot in _lstMugshots)
                imgMugshot.Dispose();
            _lstMugshots.Dispose();
        }

        #endregion IHasMugshots
    }
}
