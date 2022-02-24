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
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
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
        private readonly ThreadSafeList<Image> _lstMugshots = new ThreadSafeList<Image>(1);
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

                foreach (string strPropertyToChange in setNamesOfChangedProperties)
                {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(strPropertyToChange));
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
                        _objCharacter.OnPropertyChanged(nameof(Character.PositiveQualityKarmaTotal));
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
                    new DependencyGraphNode<string, Contact>(nameof(LinkedCharacter))
                ),
                new DependencyGraphNode<string, Contact>(nameof(Name),
                    new DependencyGraphNode<string, Contact>(nameof(LinkedCharacter))
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
                new DependencyGraphNode<string, Contact>(nameof(NotReadOnly),
                    new DependencyGraphNode<string, Contact>(nameof(ReadOnly))
                ),
                new DependencyGraphNode<string, Contact>(nameof(GroupEnabled),
                    new DependencyGraphNode<string, Contact>(nameof(ReadOnly))
                ),
                new DependencyGraphNode<string, Contact>(nameof(LoyaltyEnabled),
                    new DependencyGraphNode<string, Contact>(nameof(IsGroup)),
                    new DependencyGraphNode<string, Contact>(nameof(ForcedLoyalty)),
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

        private static Character _objCharacterForCachedContactArchetypes;
        private static List<ListItem> _lstCachedContactArchetypes;

        public static List<ListItem> ContactArchetypes(Character objCharacter)
        {
            if (_lstCachedContactArchetypes != null && _objCharacterForCachedContactArchetypes == objCharacter && !GlobalSettings.LiveCustomData)
                return _lstCachedContactArchetypes;
            _objCharacterForCachedContactArchetypes = objCharacter;
            if (_lstCachedContactArchetypes == null)
                _lstCachedContactArchetypes = Utils.ListItemListPool.Get();
            else
                _lstCachedContactArchetypes.Clear();
            _lstCachedContactArchetypes.Add(ListItem.Blank);
            XPathNavigator xmlContactsBaseNode = objCharacter.LoadDataXPath("contacts.xml").SelectSingleNodeAndCacheExpression("/chummer");
            if (xmlContactsBaseNode == null)
                return _lstCachedContactArchetypes;
            foreach (XPathNavigator xmlNode in xmlContactsBaseNode.SelectAndCacheExpression("contacts/contact"))
            {
                string strName = xmlNode.Value;
                _lstCachedContactArchetypes.Add(new ListItem(strName, xmlNode.SelectSingleNodeAndCacheExpression("@translate")?.Value ?? strName));
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
            SaveCoreAsync(true, objWriter).GetAwaiter().GetResult();
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
        [SuppressMessage("ReSharper", "MethodHasAsyncOverload")]
        private async Task SaveCoreAsync(bool blnSync, XmlWriter objWriter)
        {
            if (objWriter == null)
                return;
            if (blnSync)
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
                objWriter.WriteElementString(
                    "notes",
                    System.Text.RegularExpressions.Regex.Replace(_strNotes, @"[\u0000-\u0008\u000B\u000C\u000E-\u001F]",
                                                                 string.Empty));
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
                {
                    objWriter.WriteElementString("guid", _strUnique);
                }
                
                SaveMugshots(objWriter);
                
                objWriter.WriteEndElement();
            }
            else
            {
                await objWriter.WriteStartElementAsync("contact");
                await objWriter.WriteElementStringAsync("name", _strName);
                await objWriter.WriteElementStringAsync("role", _strRole);
                await objWriter.WriteElementStringAsync("location", _strLocation);
                await objWriter.WriteElementStringAsync("connection",
                                                        _intConnection.ToString(GlobalSettings.InvariantCultureInfo));
                await objWriter.WriteElementStringAsync(
                    "loyalty", _intLoyalty.ToString(GlobalSettings.InvariantCultureInfo));
                await objWriter.WriteElementStringAsync("metatype", _strMetatype);
                await objWriter.WriteElementStringAsync("gender", _strGender);
                await objWriter.WriteElementStringAsync("age", _strAge);
                await objWriter.WriteElementStringAsync("contacttype", _strType);
                await objWriter.WriteElementStringAsync("preferredpayment", _strPreferredPayment);
                await objWriter.WriteElementStringAsync("hobbiesvice", _strHobbiesVice);
                await objWriter.WriteElementStringAsync("personallife", _strPersonalLife);
                await objWriter.WriteElementStringAsync("type", _eContactType.ToString());
                await objWriter.WriteElementStringAsync("file", _strFileName);
                await objWriter.WriteElementStringAsync("relative", _strRelativeName);
                await objWriter.WriteElementStringAsync(
                    "notes",
                    System.Text.RegularExpressions.Regex.Replace(_strNotes, @"[\u0000-\u0008\u000B\u000C\u000E-\u001F]",
                                                                 string.Empty));
                await objWriter.WriteElementStringAsync("notesColor", ColorTranslator.ToHtml(_colNotes));
                await objWriter.WriteElementStringAsync("groupname", _strGroupName);
                await objWriter.WriteElementStringAsync(
                    "colour", _objColour.ToArgb().ToString(GlobalSettings.InvariantCultureInfo));
                await objWriter.WriteElementStringAsync(
                    "group", _blnIsGroup.ToString(GlobalSettings.InvariantCultureInfo));
                await objWriter.WriteElementStringAsync(
                    "family", _blnFamily.ToString(GlobalSettings.InvariantCultureInfo));
                await objWriter.WriteElementStringAsync("blackmail",
                                                        _blnBlackmail.ToString(GlobalSettings.InvariantCultureInfo));
                await objWriter.WriteElementStringAsync("free", _blnFree.ToString(GlobalSettings.InvariantCultureInfo));
                await objWriter.WriteElementStringAsync("groupenabled",
                                                        _blnGroupEnabled.ToString(GlobalSettings.InvariantCultureInfo));

                if (_blnReadOnly)
                    await objWriter.WriteElementStringAsync("readonly", string.Empty);

                if (_strUnique != null)
                {
                    await objWriter.WriteElementStringAsync("guid", _strUnique);
                }

                await SaveMugshotsAsync(objWriter);

                await objWriter.WriteEndElementAsync();
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
        public async ValueTask Print(XmlWriter objWriter, CultureInfo objCulture, string strLanguageToPrint)
        {
            if (objWriter == null)
                return;
            await objWriter.WriteStartElementAsync("contact");
            await objWriter.WriteElementStringAsync("guid", InternalId);
            await objWriter.WriteElementStringAsync("name", Name);
            await objWriter.WriteElementStringAsync("role", DisplayRoleMethod(strLanguageToPrint));
            await objWriter.WriteElementStringAsync("location", Location);
            if (!IsGroup)
                await objWriter.WriteElementStringAsync("connection", Connection.ToString(objCulture));
            else
                await objWriter.WriteElementStringAsync("connection", await LanguageManager.GetStringAsync("String_Group", strLanguageToPrint) + '(' + Connection.ToString(objCulture) + ')');
            await objWriter.WriteElementStringAsync("loyalty", Loyalty.ToString(objCulture));
            await objWriter.WriteElementStringAsync("metatype", DisplayMetatypeMethod(strLanguageToPrint));
            await objWriter.WriteElementStringAsync("gender", DisplayGenderMethod(strLanguageToPrint));
            await objWriter.WriteElementStringAsync("age", DisplayAgeMethod(strLanguageToPrint));
            await objWriter.WriteElementStringAsync("contacttype", DisplayTypeMethod(strLanguageToPrint));
            await objWriter.WriteElementStringAsync("preferredpayment", DisplayPreferredPaymentMethod(strLanguageToPrint));
            await objWriter.WriteElementStringAsync("hobbiesvice", DisplayHobbiesViceMethod(strLanguageToPrint));
            await objWriter.WriteElementStringAsync("personallife", DisplayPersonalLifeMethod(strLanguageToPrint));
            await objWriter.WriteElementStringAsync("type", await LanguageManager.GetStringAsync("String_" + EntityType, strLanguageToPrint));
            await objWriter.WriteElementStringAsync("forcedloyalty", ForcedLoyalty.ToString(objCulture));
            await objWriter.WriteElementStringAsync("blackmail", Blackmail.ToString(GlobalSettings.InvariantCultureInfo));
            await objWriter.WriteElementStringAsync("family", Family.ToString(GlobalSettings.InvariantCultureInfo));
            if (GlobalSettings.PrintNotes)
                await objWriter.WriteElementStringAsync("notes", Notes);

            await PrintMugshots(objWriter);

            await objWriter.WriteEndElementAsync();
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
                decReturn = Math.Max(decReturn, 2 + ImprovementManager.ValueOf(_objCharacter, Improvement.ImprovementType.ContactKarmaMinimum));
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

            return _objCharacter.LoadDataXPath("contacts.xml", strLanguage).SelectSingleNode("/chummer/contacts/contact[. = " + Role.CleanXPath() + "]/@translate")?.Value ?? Role;
        }

        public string DisplayRole
        {
            get => DisplayRoleMethod(GlobalSettings.Language);
            set => Role = _objCharacter.ReverseTranslateExtra(value, GlobalSettings.Language, "contacts.xml");
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

        public string DisplayMetatypeMethod(string strLanguage)
        {
            string strReturn = Metatype;
            if (LinkedCharacter != null)
            {
                // Update character information fields.
                XPathNavigator objMetatypeNode = _objCharacter.GetNodeXPath(true);

                strReturn = objMetatypeNode.SelectSingleNodeAndCacheExpression("translate")?.Value ?? _objCharacter.TranslateExtra(LinkedCharacter.Metatype, strLanguage, "metatypes.xml");

                if (LinkedCharacter.MetavariantGuid == Guid.Empty)
                    return strReturn;
                objMetatypeNode = objMetatypeNode
                    .SelectSingleNode("metavariants/metavariant[id = " + LinkedCharacter.MetavariantGuid.ToString("D", GlobalSettings.InvariantCultureInfo).CleanXPath() + ']');

                string strMetatypeTranslate = objMetatypeNode?.SelectSingleNodeAndCacheExpression("translate")?.Value;
                strReturn += LanguageManager.GetString("String_Space", strLanguage)
                             + '('
                             + (!string.IsNullOrEmpty(strMetatypeTranslate)
                                 ? strMetatypeTranslate
                                 : _objCharacter.TranslateExtra(LinkedCharacter.Metavariant, strLanguage, "metatypes.xml"))
                             + ')';
            }
            else
                strReturn = _objCharacter.TranslateExtra(strReturn, strLanguage, "metatypes.xml");
            return strReturn;
        }

        public string DisplayMetatype
        {
            get => DisplayMetatypeMethod(GlobalSettings.Language);
            set => Metatype = _objCharacter.ReverseTranslateExtra(value, GlobalSettings.Language, "contacts.xml");
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
                        strMetatype += LanguageManager.GetString("String_Space") + '(' + LinkedCharacter.Metavariant + ')';
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

        public string DisplayGenderMethod(string strLanguage)
        {
            if (strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                return Gender;

            return _objCharacter.LoadDataXPath("contacts.xml", strLanguage).SelectSingleNode("/chummer/genders/gender[. = " + Gender.CleanXPath() + "]/@translate")?.Value ?? Gender;
        }

        public string DisplayGender
        {
            get => DisplayGenderMethod(GlobalSettings.Language);
            set => Gender = _objCharacter.ReverseTranslateExtra(value, GlobalSettings.Language, "contacts.xml");
        }

        /// <summary>
        /// Gender of this Contact.
        /// </summary>
        public string Gender
        {
            get => LinkedCharacter != null ? LinkedCharacter.Gender : _strGender;
            set
            {
                if (_strGender != value)
                {
                    _strGender = value;
                    OnPropertyChanged();
                }
            }
        }

        public string DisplayAgeMethod(string strLanguage)
        {
            if (strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                return Age;

            return _objCharacter.LoadDataXPath("contacts.xml", strLanguage).SelectSingleNode("/chummer/ages/age[. = " + Age.CleanXPath() + "]/@translate")?.Value ?? Age;
        }

        public string DisplayAge
        {
            get => DisplayAgeMethod(GlobalSettings.Language);
            set => Age = _objCharacter.ReverseTranslateExtra(value, GlobalSettings.Language, "contacts.xml");
        }

        /// <summary>
        /// How old is this Contact.
        /// </summary>
        public string Age
        {
            get => LinkedCharacter != null ? LinkedCharacter.Age : _strAge;
            set
            {
                if (_strAge != value)
                {
                    _strAge = value;
                    OnPropertyChanged();
                }
            }
        }

        public string DisplayTypeMethod(string strLanguage)
        {
            if (strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                return Type;

            return _objCharacter.LoadDataXPath("contacts.xml", strLanguage).SelectSingleNode("/chummer/types/type[. = " + Type.CleanXPath() + "]/@translate")?.Value ?? Type;
        }

        public string DisplayType
        {
            get => DisplayTypeMethod(GlobalSettings.Language);
            set => Type = _objCharacter.ReverseTranslateExtra(value, GlobalSettings.Language, "contacts.xml");
        }

        /// <summary>
        /// What type of Contact is this.
        /// </summary>
        public string Type
        {
            get => _strType;
            set
            {
                if (_strType != value)
                {
                    _strType = value;
                    OnPropertyChanged();
                }
            }
        }

        public string DisplayPreferredPaymentMethod(string strLanguage)
        {
            if (strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                return PreferredPayment;

            return _objCharacter.LoadDataXPath("contacts.xml", strLanguage)
                .SelectSingleNode("/chummer/preferredpayments/preferredpayment[. = " + PreferredPayment.CleanXPath() + "]/@translate")?.Value ?? PreferredPayment;
        }

        public string DisplayPreferredPayment
        {
            get => DisplayPreferredPaymentMethod(GlobalSettings.Language);
            set => PreferredPayment = _objCharacter.ReverseTranslateExtra(value, GlobalSettings.Language, "contacts.xml");
        }

        /// <summary>
        /// Preferred payment method of this Contact.
        /// </summary>
        public string PreferredPayment
        {
            get => _strPreferredPayment;
            set
            {
                if (_strPreferredPayment != value)
                {
                    _strPreferredPayment = value;
                    OnPropertyChanged();
                }
            }
        }

        public string DisplayHobbiesViceMethod(string strLanguage)
        {
            if (strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                return HobbiesVice;

            try
            {
                return _objCharacter.LoadDataXPath("contacts.xml", strLanguage).SelectSingleNode("/chummer/hobbiesvices/hobbyvice[. = " + HobbiesVice.CleanXPath() + "]/@translate")?.Value
                       ?? HobbiesVice;
            }
            catch (Exception e)
            {
                string msg = "Could not LoadData for " + strLanguage + " of hobbiesvices " + HobbiesVice + ". Is it missing in contacts.xml?";
                Log.Error(e, msg);
                throw;
            }
        }

        public string DisplayHobbiesVice
        {
            get => DisplayHobbiesViceMethod(GlobalSettings.Language);
            set => HobbiesVice = _objCharacter.ReverseTranslateExtra(value, GlobalSettings.Language, "contacts.xml");
        }

        /// <summary>
        /// Hobbies/Vice of this Contact.
        /// </summary>
        public string HobbiesVice
        {
            get => _strHobbiesVice;
            set
            {
                if (_strHobbiesVice != value)
                {
                    _strHobbiesVice = value;
                    OnPropertyChanged();
                }
            }
        }

        public string DisplayPersonalLifeMethod(string strLanguage)
        {
            if (strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                return PersonalLife;

            return _objCharacter.LoadDataXPath("contacts.xml", strLanguage).SelectSingleNode("/chummer/personallives/personallife[. = " + PersonalLife.CleanXPath() + "]/@translate")?.Value
                   ?? PersonalLife;
        }

        public string DisplayPersonalLife
        {
            get => DisplayPersonalLifeMethod(GlobalSettings.Language);
            set => PersonalLife = _objCharacter.ReverseTranslateExtra(value, GlobalSettings.Language, "contacts.xml");
        }

        /// <summary>
        /// Personal Life of this Contact.
        /// </summary>
        public string PersonalLife
        {
            get => _strPersonalLife;
            set
            {
                if (_strPersonalLife != value)
                {
                    _strPersonalLife = value;
                    OnPropertyChanged();
                }
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
                if (_blnIsGroup != value)
                {
                    _blnIsGroup = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool LoyaltyEnabled => !ReadOnly && !IsGroup && ForcedLoyalty <= 0;

        public int ConnectionMaximum => CharacterObject.Created || CharacterObject.FriendsInHighPlaces ? 12 : 6;

        public string QuickText => string.Format(GlobalSettings.CultureInfo, IsGroup ? "({0}/{1}G)" : "({0}/{1})", Connection, Loyalty);

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

        public bool FreeEnabled => _intCachedFreeFromImprovement < 1;

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
                if (strFile.EndsWith(".chum5", StringComparison.OrdinalIgnoreCase))
                {
                    Character objOpenCharacter = Program.MainForm.OpenCharacters.FirstOrDefault(x => x.FileName == strFile);
                    _objLinkedCharacter = objOpenCharacter ?? Program.MainForm.LoadCharacter(strFile, string.Empty, false, false);
                    if (_objLinkedCharacter != null)
                        CharacterObject.LinkedCharacters.Add(_objLinkedCharacter);
                }
            }
            if (_objLinkedCharacter != objOldLinkedCharacter)
            {
                if (objOldLinkedCharacter != null)
                {
                    objOldLinkedCharacter.PropertyChanged -= LinkedCharacterOnPropertyChanged;
                    if (Program.MainForm.OpenCharacters.Contains(objOldLinkedCharacter))
                    {
                        if (Program.MainForm.OpenCharacters.All(x => !x.LinkedCharacters.Contains(objOldLinkedCharacter))
                            && Program.MainForm.OpenCharacterForms.All(x => x.CharacterObject != objOldLinkedCharacter))
                            Program.MainForm.OpenCharacters.Remove(objOldLinkedCharacter);
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
                using (new EnterReadLock(CharacterObject.LockObject))
                {
                    if (LinkedCharacter != null)
                        return LinkedCharacter.Mugshots;

                    using (new EnterReadLock(_lstMugshots.LockObject))
                        return _lstMugshots;
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
                        using (new EnterReadLock(_objCharacter.LockObject))
                        {
                            if (value >= Mugshots.Count)
                                value = -1;
                        }
                    }

                    using (new EnterUpgradeableReadLock(_objCharacter.LockObject))
                    {
                        if (_intMainMugshotIndex == value)
                            return;
                        using (new EnterWriteLock(_objCharacter.LockObject))
                        {
                            _intMainMugshotIndex = value;
                            OnPropertyChanged();
                        }
                    }
                }
            }
        }

        public void SaveMugshots(XmlWriter objWriter)
        {
            SaveMugshotsCore(true, objWriter).GetAwaiter().GetResult();
        }

        public Task SaveMugshotsAsync(XmlWriter objWriter)
        {
            return SaveMugshotsCore(false, objWriter);
        }

        public async Task SaveMugshotsCore(bool blnSync, XmlWriter objWriter)
        {
            if (objWriter == null)
                return;
            if (blnSync)
            {
                objWriter.WriteElementString("mainmugshotindex",
                                             MainMugshotIndex.ToString(GlobalSettings.InvariantCultureInfo));
                // <mugshot>
                objWriter.WriteStartElement("mugshots");
                foreach (Image imgMugshot in Mugshots)
                {
                    // ReSharper disable once MethodHasAsyncOverload
                    objWriter.WriteElementString("mugshot", GlobalSettings.ImageToBase64StringForStorage(imgMugshot));
                }

                // </mugshot>
                // ReSharper disable once MethodHasAsyncOverload
                objWriter.WriteEndElement();
            }
            else
            {
                await objWriter.WriteElementStringAsync("mainmugshotindex",
                                                        MainMugshotIndex.ToString(GlobalSettings.InvariantCultureInfo));
                // <mugshot>
                await objWriter.WriteStartElementAsync("mugshots");
                foreach (Image imgMugshot in Mugshots)
                {
                    await objWriter.WriteElementStringAsync(
                        "mugshot", await GlobalSettings.ImageToBase64StringForStorageAsync(imgMugshot));
                }

                // </mugshot>
                await objWriter.WriteEndElementAsync();
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

        public async ValueTask PrintMugshots(XmlWriter objWriter)
        {
            if (objWriter == null)
                return;
            if (LinkedCharacter != null)
                await LinkedCharacter.PrintMugshots(objWriter);
            else if (Mugshots.Count > 0)
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
                        Program.ShowMessageBox(await LanguageManager.GetStringAsync("Message_Insufficient_Permissions_Warning"));
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
                        "file://" + imgMugshotPath.Replace(Path.DirectorySeparatorChar, '/'));
                    // <mainmugshotbase64 />
                    await objWriter.WriteElementStringAsync("mainmugshotbase64", await imgMainMugshot.ToBase64StringAsJpegAsync());
                }
                // <othermugshots>
                await objWriter.WriteElementStringAsync("hasothermugshots",
                    (imgMainMugshot == null || Mugshots.Count > 1).ToString(GlobalSettings.InvariantCultureInfo));
                await objWriter.WriteStartElementAsync("othermugshots");
                for (int i = 0; i < Mugshots.Count; ++i)
                {
                    if (i == MainMugshotIndex)
                        continue;
                    Image imgMugshot = Mugshots[i];
                    await objWriter.WriteStartElementAsync("mugshot");

                    await objWriter.WriteElementStringAsync("stringbase64", await imgMugshot.ToBase64StringAsJpegAsync());

                    string imgMugshotPath = Path.Combine(strMugshotsDirectoryPath,
                        guiImage.ToString("N", GlobalSettings.InvariantCultureInfo) +
                        i.ToString(GlobalSettings.InvariantCultureInfo) + ".jpg");
                    imgMugshot.Save(imgMugshotPath);
                    await objWriter.WriteElementStringAsync("temppath",
                        "file://" + imgMugshotPath.Replace(Path.DirectorySeparatorChar, '/'));

                    await objWriter.WriteEndElementAsync();
                }
                // </mugshots>
                await objWriter.WriteEndElementAsync();
            }
        }

        public void Dispose()
        {
            if (_lstCachedContactArchetypes != null)
                Utils.ListItemListPool.Return(_lstCachedContactArchetypes);
            if (_objLinkedCharacter != null && !Utils.IsUnitTest
                                            && Program.MainForm.OpenCharacters.Contains(_objLinkedCharacter)
                                            && Program.MainForm.OpenCharacters.All(x => !x.LinkedCharacters.Contains(_objLinkedCharacter))
                                            && Program.MainForm.OpenCharacterForms.All(x => x.CharacterObject != _objLinkedCharacter))
                Program.MainForm.OpenCharacters.Remove(_objLinkedCharacter);
            foreach (Image imgMugshot in _lstMugshots)
                imgMugshot.Dispose();
            _lstMugshots.Dispose();
        }

        #endregion IHasMugshots
    }
}
