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
using Chummer.Annotations;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;

namespace Chummer
{
    /// <summary>
    /// Type of Contact.
    /// </summary>
    public enum ContactType
    {
        Contact = 0,
        Enemy = 1,
        Pet = 2,
    }

    /// <summary>
    /// A Contact or Enemy.
    /// </summary>
    [DebuggerDisplay("{" + nameof(Name) + "} ({DisplayRoleMethod(GlobalOptions.DefaultLanguage)})")]
    public class Contact : INotifyMultiplePropertyChanged, IHasName, IHasMugshots, IHasNotes
    {
        private string _strName = string.Empty;
        private string _strRole = string.Empty;
        private string _strLocation = string.Empty;
        private string _strUnique = Guid.NewGuid().ToString("D");

        private int _intConnection = 1;
        private int _intLoyalty = 1;
        private string _strMetatype = string.Empty;
        private string _strSex = string.Empty;
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
        private Color _objColour;
        private bool _blnIsGroup;
        private readonly Character _objCharacter;
        private bool _blnBlackmail;
        private bool _blnFamily;
        private bool _blnGroupEnabled = true;
        private bool _blnReadOnly;
        private bool _blnFree;
        private readonly List<Image> _lstMugshots = new List<Image>();
        private int _intMainMugshotIndex = -1;
        private int _intKarmaMinimum = 2;

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        public void OnPropertyChanged([CallerMemberName] string strPropertyName = null)
        {
            OnMultiplePropertyChanged(strPropertyName);
        }

        public void OnMultiplePropertyChanged(params string[] lstPropertyNames)
        {
            ICollection<string> lstNamesOfChangedProperties = null;
            foreach (string strPropertyName in lstPropertyNames)
            {
                if (lstNamesOfChangedProperties == null)
                    lstNamesOfChangedProperties = ContactDependencyGraph.GetWithAllDependants(strPropertyName);
                else
                {
                    foreach (string strLoopChangedProperty in ContactDependencyGraph.GetWithAllDependants(strPropertyName))
                        lstNamesOfChangedProperties.Add(strLoopChangedProperty);
                }
            }

            if ((lstNamesOfChangedProperties?.Count > 0) != true)
                return;

            if (lstNamesOfChangedProperties.Contains(nameof(ForcedLoyalty)))
            {
                _intCachedForcedLoyalty = int.MinValue;
            }
            if (lstNamesOfChangedProperties.Contains(nameof(GroupEnabled)))
            {
                _intCachedGroupEnabled = -1;
            }
            if (lstNamesOfChangedProperties.Contains(nameof(Free)))
            {
                _intCachedFreeFromImprovement = -1;
            }

            foreach (string strPropertyToChange in lstNamesOfChangedProperties)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(strPropertyToChange));
            }
        }

        private static readonly DependancyGraph<string> ContactDependencyGraph =
            new DependancyGraph<string>(
                new DependancyGraphNode<string>(nameof(NoLinkedCharacter),
                    new DependancyGraphNode<string>(nameof(LinkedCharacter))
                ),
                new DependancyGraphNode<string>(nameof(Name),
                    new DependancyGraphNode<string>(nameof(LinkedCharacter))
                ),
                new DependancyGraphNode<string>(nameof(DisplaySex),
                    new DependancyGraphNode<string>(nameof(Sex),
                        new DependancyGraphNode<string>(nameof(LinkedCharacter))
                    )
                ),
                new DependancyGraphNode<string>(nameof(DisplayMetatype),
                    new DependancyGraphNode<string>(nameof(Metatype),
                        new DependancyGraphNode<string>(nameof(LinkedCharacter))
                    )
                ),
                new DependancyGraphNode<string>(nameof(DisplayAge),
                    new DependancyGraphNode<string>(nameof(Age),
                        new DependancyGraphNode<string>(nameof(LinkedCharacter))
                    )
                ),
                new DependancyGraphNode<string>(nameof(MainMugshot),
                    new DependancyGraphNode<string>(nameof(LinkedCharacter)),
                    new DependancyGraphNode<string>(nameof(Mugshots),
                        new DependancyGraphNode<string>(nameof(LinkedCharacter))
                    ),
                    new DependancyGraphNode<string>(nameof(MainMugshotIndex))
                ),
                new DependancyGraphNode<string>(nameof(IsNotEnemy),
                    new DependancyGraphNode<string>(nameof(EntityType))
                ),
                new DependancyGraphNode<string>(nameof(NotReadOnly),
                    new DependancyGraphNode<string>(nameof(ReadOnly))
                ),
                new DependancyGraphNode<string>(nameof(GroupEnabled),
                    new DependancyGraphNode<string>(nameof(ReadOnly))
                ),
                new DependancyGraphNode<string>(nameof(LoyaltyEnabled),
                    new DependancyGraphNode<string>(nameof(IsGroup)),
                    new DependancyGraphNode<string>(nameof(ForcedLoyalty)),
                    new DependancyGraphNode<string>(nameof(ReadOnly))
                ),
                new DependancyGraphNode<string>(nameof(ContactPoints),
                    new DependancyGraphNode<string>(nameof(Free)),
                    new DependancyGraphNode<string>(nameof(Connection),
                        new DependancyGraphNode<string>(nameof(ConnectionMaximum))
                    ),
                    new DependancyGraphNode<string>(nameof(Loyalty)),
                    new DependancyGraphNode<string>(nameof(Family)),
                    new DependancyGraphNode<string>(nameof(Blackmail))
                ),
                new DependancyGraphNode<string>(nameof(QuickText),
                    new DependancyGraphNode<string>(nameof(Connection)),
                    new DependancyGraphNode<string>(nameof(IsGroup)),
                    new DependancyGraphNode<string>(nameof(Loyalty),
                        new DependancyGraphNode<string>(nameof(IsGroup)),
                        new DependancyGraphNode<string>(nameof(ForcedLoyalty))
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
                return default(ContactType);
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
        #endregion

        #region Constructor, Save, Load, and Print Methods
        public Contact(Character objCharacter, bool blnIsReadOnly = false)
        {
            _objCharacter = objCharacter;
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
        public void Save(XmlTextWriter objWriter)
        {
            objWriter.WriteStartElement("contact");
            objWriter.WriteElementString("name", _strName);
            objWriter.WriteElementString("role", _strRole);
            objWriter.WriteElementString("location", _strLocation);
            objWriter.WriteElementString("connection", _intConnection.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("loyalty", _intLoyalty.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("metatype", _strMetatype);
            objWriter.WriteElementString("sex", _strSex);
            objWriter.WriteElementString("age", _strAge);
            objWriter.WriteElementString("contacttype", _strType);
            objWriter.WriteElementString("preferredpayment", _strPreferredPayment);
            objWriter.WriteElementString("hobbiesvice", _strHobbiesVice);
            objWriter.WriteElementString("personallife", _strPersonalLife);
            objWriter.WriteElementString("type", _eContactType.ToString());
            objWriter.WriteElementString("file", _strFileName);
            objWriter.WriteElementString("relative", _strRelativeName);
            objWriter.WriteElementString("notes", _strNotes);
            objWriter.WriteElementString("groupname", _strGroupName);
            objWriter.WriteElementString("colour", _objColour.ToArgb().ToString());
            objWriter.WriteElementString("group", _blnIsGroup.ToString());
            objWriter.WriteElementString("family", _blnFamily.ToString());
            objWriter.WriteElementString("blackmail", _blnBlackmail.ToString());
            objWriter.WriteElementString("free", _blnFree.ToString());
            objWriter.WriteElementString("groupenabled", _blnGroupEnabled.ToString());

            if (_blnReadOnly)
                objWriter.WriteElementString("readonly", string.Empty);

            if (_strUnique != null)
            {
                objWriter.WriteElementString("guid", _strUnique);
            }

            SaveMugshots(objWriter);

            /* Disabled for now because we cannot change any properties in the linked character anyway
            if (LinkedCharacter?.IsSaving == false && !Program.MainForm.OpenCharacterForms.Any(x => x.CharacterObject == LinkedCharacter))
                LinkedCharacter.Save();
                */

            objWriter.WriteEndElement();
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
            objNode.TryGetStringFieldQuickly("sex", ref _strSex);
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
            objNode.TryGetStringFieldQuickly("groupname", ref _strGroupName);
            objNode.TryGetBoolFieldQuickly("group", ref _blnIsGroup);
            objNode.TryGetStringFieldQuickly("guid", ref _strUnique);
            objNode.TryGetBoolFieldQuickly("family", ref _blnFamily);
            objNode.TryGetBoolFieldQuickly("blackmail", ref _blnBlackmail);
            objNode.TryGetBoolFieldQuickly("free", ref _blnFree);
            if (objNode.SelectSingleNode("colour") != null)
            {
                int intTmp = _objColour.ToArgb();
                if (objNode.TryGetInt32FieldQuickly("colour", ref intTmp))
                    _objColour = Color.FromArgb(intTmp);
            }

            _blnReadOnly = objNode.SelectSingleNode("readonly") != null;

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
        public void Print(XmlTextWriter objWriter, CultureInfo objCulture, string strLanguageToPrint)
        {
            objWriter.WriteStartElement("contact");
            objWriter.WriteElementString("name", Name);
            objWriter.WriteElementString("role", DisplayRoleMethod(strLanguageToPrint));
            objWriter.WriteElementString("location", Location);
            if (!IsGroup)
                objWriter.WriteElementString("connection", Connection.ToString(objCulture));
            else
                objWriter.WriteElementString("connection", LanguageManager.GetString("String_Group", strLanguageToPrint) + "(" + Connection.ToString(objCulture) + ')');
            objWriter.WriteElementString("loyalty", Loyalty.ToString(objCulture));
            objWriter.WriteElementString("metatype", DisplayMetatypeMethod(strLanguageToPrint));
            objWriter.WriteElementString("sex", DisplaySexMethod(strLanguageToPrint));
            objWriter.WriteElementString("age", DisplayAgeMethod(strLanguageToPrint));
            objWriter.WriteElementString("contacttype", DisplayTypeMethod(strLanguageToPrint));
            objWriter.WriteElementString("preferredpayment", DisplayPreferredPaymentMethod(strLanguageToPrint));
            objWriter.WriteElementString("hobbiesvice", DisplayHobbiesViceMethod(strLanguageToPrint));
            objWriter.WriteElementString("personallife", DisplayPersonalLifeMethod(strLanguageToPrint));
            objWriter.WriteElementString("type", LanguageManager.GetString("String_" + EntityType.ToString(), strLanguageToPrint));
            objWriter.WriteElementString("forcedloyalty", ForcedLoyalty.ToString(objCulture));
            objWriter.WriteElementString("blackmail", Blackmail.ToString());
            objWriter.WriteElementString("family", Family.ToString());
            if (CharacterObject.Options.PrintNotes)
                objWriter.WriteElementString("notes", Notes);

            PrintMugshots(objWriter);

            objWriter.WriteEndElement();
        }
        #endregion

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
                int intReturn = Connection + Loyalty;
                if (Family)
                    intReturn += 1;
                if (Blackmail)
                    intReturn += 2;
                intReturn +=
                    ImprovementManager.ValueOf(_objCharacter, Improvement.ImprovementType.ContactKarmaDiscount);
                intReturn = Math.Max(intReturn,
                    _intKarmaMinimum + ImprovementManager.ValueOf(_objCharacter, Improvement.ImprovementType.ContactKarmaMinimum));
                return intReturn;
            }
        }

        /// <summary>
        /// Name of the Contact.
        /// </summary>
        public string Name
        {
            get
            {
                if (LinkedCharacter != null)
                    return LinkedCharacter.CharacterName;
                return _strName;
            }
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
            if (strLanguage == GlobalOptions.DefaultLanguage)
                return Role;

            return XmlManager.Load("contacts.xml", strLanguage).SelectSingleNode("/chummer/contacts/contact[text() = \"" + Role + "\"]/@translate")?.InnerText ?? Role;
        }

        public string DisplayRole
        {
            get => DisplayRoleMethod(GlobalOptions.Language);
            set => Role = LanguageManager.ReverseTranslateExtra(value, GlobalOptions.Language);
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
                if (IsGroup)
                    return 1;
                return _intLoyalty;
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
                XmlDocument objMetatypeDoc = XmlManager.Load("metatypes.xml", strLanguage);
                XmlNode objMetatypeNode = objMetatypeDoc.SelectSingleNode("/chummer/metatypes/metatype[name = \"" + LinkedCharacter.Metatype + "\"]");
                if (objMetatypeNode == null)
                {
                    objMetatypeDoc = XmlManager.Load("critters.xml", strLanguage);
                    objMetatypeNode = objMetatypeDoc.SelectSingleNode("/chummer/metatypes/metatype[name = \"" + LinkedCharacter.Metatype + "\"]");
                }

                strReturn = objMetatypeNode?["translate"]?.InnerText ?? LanguageManager.TranslateExtra(LinkedCharacter.Metatype, strLanguage);

                if (!string.IsNullOrEmpty(LinkedCharacter.Metavariant))
                {
                    objMetatypeNode = objMetatypeNode?.SelectSingleNode("metavariants/metavariant[name = \"" + LinkedCharacter.Metavariant + "\"]");

                    string strMetatypeTranslate = objMetatypeNode?["translate"]?.InnerText;
                    strReturn += !string.IsNullOrEmpty(strMetatypeTranslate)
                        ? LanguageManager.GetString("String_Space", strLanguage) + '(' + strMetatypeTranslate + ')'
                        : LanguageManager.GetString("String_Space", strLanguage) + '(' + LanguageManager.TranslateExtra(LinkedCharacter.Metavariant, strLanguage) + ')';
                }
            }
            else
                strReturn = LanguageManager.TranslateExtra(strReturn, strLanguage);
            return strReturn;
        }

        public string DisplayMetatype
        {
            get => DisplayMetatypeMethod(GlobalOptions.Language);
            set => Metatype = LanguageManager.ReverseTranslateExtra(value, GlobalOptions.Language);
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
                        strMetatype += LanguageManager.GetString("String_Space", GlobalOptions.Language) + '(' + LinkedCharacter.Metavariant + ')';
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

        public string DisplaySexMethod(string strLanguage)
        {
            if (strLanguage == GlobalOptions.DefaultLanguage)
                return Sex;

            return XmlManager.Load("contacts.xml", strLanguage).SelectSingleNode("/chummer/sexes/sex[text() = \"" + Sex + "\"]/@translate")?.InnerText ?? Sex;
        }

        public string DisplaySex
        {
            get => DisplaySexMethod(GlobalOptions.Language);
            set => Sex = LanguageManager.ReverseTranslateExtra(value, GlobalOptions.Language);
        }

        /// <summary>
        /// Gender of this Contact.
        /// </summary>
        public string Sex
        {
            get
            {
                if (LinkedCharacter != null)
                    return LinkedCharacter.Sex;
                return _strSex;
            }
            set
            {
                if (_strSex != value)
                {
                    _strSex = value;
                    OnPropertyChanged();
                }
            }
        }

        public string DisplayAgeMethod(string strLanguage)
        {
            if (strLanguage == GlobalOptions.DefaultLanguage)
                return Age;

            return XmlManager.Load("contacts.xml", strLanguage).SelectSingleNode("/chummer/ages/age[text() = \"" + Age + "\"]/@translate")?.InnerText ?? Age;
        }

        public string DisplayAge
        {
            get => DisplayAgeMethod(GlobalOptions.Language);
            set => Age = LanguageManager.ReverseTranslateExtra(value, GlobalOptions.Language);
        }

        /// <summary>
        /// How old is this Contact.
        /// </summary>
        public string Age
        {
            get
            {
                if (LinkedCharacter != null)
                    return LinkedCharacter.Age;
                return _strAge;
            }
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
            if (strLanguage == GlobalOptions.DefaultLanguage)
                return Type;

            return XmlManager.Load("contacts.xml", strLanguage).SelectSingleNode("/chummer/types/type[text() = \"" + Type + "\"]/@translate")?.InnerText ?? Type;
        }

        public string DisplayType
        {
            get => DisplayTypeMethod(GlobalOptions.Language);
            set => Type = LanguageManager.ReverseTranslateExtra(value, GlobalOptions.Language);
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
            if (strLanguage == GlobalOptions.DefaultLanguage)
                return PreferredPayment;

            return XmlManager.Load("contacts.xml", strLanguage).SelectSingleNode("/chummer/preferredpayments/preferredpayment[text() = \"" + PreferredPayment + "\"]/@translate")?.InnerText ?? PreferredPayment;
        }

        public string DisplayPreferredPayment
        {
            get => DisplayPreferredPaymentMethod(GlobalOptions.Language);
            set => PreferredPayment = LanguageManager.ReverseTranslateExtra(value, GlobalOptions.Language);
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
            if (strLanguage == GlobalOptions.DefaultLanguage)
                return HobbiesVice;

            return XmlManager.Load("contacts.xml", strLanguage).SelectSingleNode("/chummer/hobbiesvices/hobbyvice[text() = \"" + HobbiesVice + "\"]/@translate")?.InnerText ?? HobbiesVice;
        }

        public string DisplayHobbiesVice
        {
            get => DisplayHobbiesViceMethod(GlobalOptions.Language);
            set => HobbiesVice = LanguageManager.ReverseTranslateExtra(value, GlobalOptions.Language);
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
            if (strLanguage == GlobalOptions.DefaultLanguage)
                return PersonalLife;

            return XmlManager.Load("contacts.xml", strLanguage).SelectSingleNode("/chummer/personallives/personallife[text() = \"" + PersonalLife + "\"]/@translate")?.InnerText ?? PersonalLife;
        }

        public string DisplayPersonalLife
        {
            get => DisplayPersonalLifeMethod(GlobalOptions.Language);
            set => PersonalLife = LanguageManager.ReverseTranslateExtra(value, GlobalOptions.Language);
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

        public string QuickText => $"({Connection}/{(IsGroup ? $"{Loyalty}G" : Loyalty.ToString(GlobalOptions.CultureInfo))})";

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

        public bool IsNotEnemy => EntityType != ContactType.Enemy;

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
        /// Contact Colour.
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
                    _intCachedFreeFromImprovement = CharacterObject.Improvements.Any(x => x.ImproveType == Improvement.ImprovementType.ContactMakeFree && GUID == x.ImprovedName && x.Enabled) ? 1 : 0;
                }

                return _intCachedFreeFromImprovement > 0;
            }
            set => _blnFree = value;
        }

        public bool FreeEnabled => _intCachedFreeFromImprovement < 1;

        /// <summary>
        /// Unique ID for this contact
        /// </summary>
        public string GUID => _strUnique;

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
                    _intCachedGroupEnabled = !ReadOnly && !CharacterObject.Improvements.Any(x => x.ImproveType == Improvement.ImprovementType.ContactForceGroup && GUID == x.ImprovedName && x.Enabled) ? 1 : 0;
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
                foreach (Improvement objImprovement in CharacterObject.Improvements)
                {
                    if (objImprovement.ImproveType == Improvement.ImprovementType.ContactForcedLoyalty && objImprovement.ImprovedName == GUID && objImprovement.Enabled)
                    {
                        intMaxForcedLoyalty = Math.Max(intMaxForcedLoyalty, objImprovement.Value);
                    }
                }

                return _intCachedForcedLoyalty = intMaxForcedLoyalty;
            }
        }

        public Character CharacterObject => _objCharacter;

        public Character LinkedCharacter => _objLinkedCharacter;

        public bool NoLinkedCharacter => _objLinkedCharacter == null;

        public async void RefreshLinkedCharacter(bool blnShowError = false)
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
                    Program.MainForm.ShowMessageBox(string.Format(LanguageManager.GetString("Message_FileNotFound", GlobalOptions.Language), FileName),
                        LanguageManager.GetString("MessageTitle_FileNotFound", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            if (!blnError)
            {
                string strFile = blnUseRelative ? Path.GetFullPath(RelativeFileName) : FileName;
                if (strFile.EndsWith(".chum5"))
                {
                    Character objOpenCharacter = Program.MainForm.OpenCharacters.FirstOrDefault(x => x.FileName == strFile);
                    _objLinkedCharacter = objOpenCharacter ?? (await Program.MainForm.LoadCharacter(strFile, string.Empty, false, false));
                    if (_objLinkedCharacter != null)
                        CharacterObject.LinkedCharacters.Add(_objLinkedCharacter);
                }
            }
            if (_objLinkedCharacter != objOldLinkedCharacter)
            {
                if (objOldLinkedCharacter != null)
                {
                    objOldLinkedCharacter.PropertyChanged -= LinkedCharacterOnPropertyChanged;
                    if (!Program.MainForm.OpenCharacters.Any(x => x.LinkedCharacters.Contains(objOldLinkedCharacter) && x != objOldLinkedCharacter))
                    {
                        Program.MainForm.OpenCharacters.Remove(objOldLinkedCharacter);
                        objOldLinkedCharacter.DeleteCharacter();
                    }
                }
                if (_objLinkedCharacter != null)
                {
                    if (string.IsNullOrEmpty(_strName) && Name != LanguageManager.GetString("String_UnnamedCharacter", GlobalOptions.Language))
                        _strName = Name;
                    if (string.IsNullOrEmpty(_strAge) && !string.IsNullOrEmpty(Age))
                        _strAge = Age;
                    if (string.IsNullOrEmpty(_strSex) && !string.IsNullOrEmpty(Sex))
                        _strSex = Sex;
                    if (string.IsNullOrEmpty(_strMetatype) && !string.IsNullOrEmpty(Metatype))
                        _strMetatype = Metatype;

                    _objLinkedCharacter.PropertyChanged += LinkedCharacterOnPropertyChanged;
                }
                OnPropertyChanged(nameof(LinkedCharacter));
            }
        }

        private void LinkedCharacterOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Character.Name))
                OnPropertyChanged(nameof(Name));
            else if (e.PropertyName == nameof(Character.Age))
                OnPropertyChanged(nameof(Age));
            else if (e.PropertyName == nameof(Character.Sex))
                OnPropertyChanged(nameof(Sex));
            else if (e.PropertyName == nameof(Character.Metatype) || e.PropertyName == nameof(Character.Metavariant))
                OnPropertyChanged(nameof(Metatype));
            else if (e.PropertyName == nameof(Character.Mugshots))
                OnPropertyChanged(nameof(Mugshots));
            else if (e.PropertyName == nameof(Character.MainMugshot))
                OnPropertyChanged(nameof(MainMugshot));
            else if (e.PropertyName == nameof(Character.MainMugshotIndex))
                OnPropertyChanged(nameof(MainMugshotIndex));
        }
        #endregion

        #region IHasMugshots
        /// <summary>
        /// Character's portraits encoded using Base64.
        /// </summary>
        public IList<Image> Mugshots
        {
            get
            {
                if (LinkedCharacter != null)
                    return LinkedCharacter.Mugshots;

                return _lstMugshots;
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
            get
            {
                if (LinkedCharacter != null)
                    return LinkedCharacter.MainMugshotIndex;

                return _intMainMugshotIndex;
            }
            set
            {
                if (LinkedCharacter != null)
                    LinkedCharacter.MainMugshotIndex = value;
                else
                {
                    if (value >= _lstMugshots.Count || value < -1)
                        value = -1;
                    if (_intMainMugshotIndex != value)
                    {
                        _intMainMugshotIndex = value;
                        OnPropertyChanged();
                    }
                }
            }
        }

        public void SaveMugshots(XmlTextWriter objWriter)
        {
            objWriter.WriteElementString("mainmugshotindex", MainMugshotIndex.ToString());
            // <mugshot>
            objWriter.WriteStartElement("mugshots");
            foreach (Image imgMugshot in Mugshots)
            {
                objWriter.WriteElementString("mugshot", imgMugshot.ToBase64String());
            }
            // </mugshot>
            objWriter.WriteEndElement();
        }

        public void LoadMugshots(XPathNavigator xmlSavedNode)
        {
            xmlSavedNode.TryGetInt32FieldQuickly("mainmugshotindex", ref _intMainMugshotIndex);
            XPathNodeIterator xmlMugshotsList = xmlSavedNode.Select("mugshots/mugshot");
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
                Parallel.For(0, lstMugshotsBase64.Count, i =>
                {
                    objMugshotImages[i] = lstMugshotsBase64[i].ToImage(System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
                });
                _lstMugshots.AddRange(objMugshotImages);
            }
            else if (lstMugshotsBase64.Count == 1)
            {
                _lstMugshots.Add(lstMugshotsBase64[0].ToImage(System.Drawing.Imaging.PixelFormat.Format32bppPArgb));
            }
        }

        public void PrintMugshots(XmlTextWriter objWriter)
        {
            if (LinkedCharacter != null)
                LinkedCharacter.PrintMugshots(objWriter);
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
                        Program.MainForm.ShowMessageBox(LanguageManager.GetString("Message_Insufficient_Permissions_Warning", GlobalOptions.Language));
                    }
                }
                Guid guiImage = Guid.NewGuid();
                string imgMugshotPath = Path.Combine(strMugshotsDirectoryPath, guiImage.ToString("N") + ".img");
                Image imgMainMugshot = MainMugshot;
                if (imgMainMugshot != null)
                {
                    imgMainMugshot.Save(imgMugshotPath);
                    // <mainmugshotpath />
                    objWriter.WriteElementString("mainmugshotpath", "file://" + imgMugshotPath.Replace(Path.DirectorySeparatorChar, '/'));
                    // <mainmugshotbase64 />
                    objWriter.WriteElementString("mainmugshotbase64", imgMainMugshot.ToBase64String());
                }
                // <othermugshots>
                objWriter.WriteElementString("hasothermugshots", (imgMainMugshot == null || Mugshots.Count > 1).ToString());
                objWriter.WriteStartElement("othermugshots");
                for (int i = 0; i < Mugshots.Count; ++i)
                {
                    if (i == MainMugshotIndex)
                        continue;
                    Image imgMugshot = Mugshots[i];
                    objWriter.WriteStartElement("mugshot");

                    objWriter.WriteElementString("stringbase64", imgMugshot.ToBase64String());

                    imgMugshotPath = Path.Combine(strMugshotsDirectoryPath, guiImage.ToString("N") + i.ToString() + ".img");
                    imgMugshot.Save(imgMugshotPath);
                    objWriter.WriteElementString("temppath", "file://" + imgMugshotPath.Replace(Path.DirectorySeparatorChar, '/'));

                    objWriter.WriteEndElement();
                }
                // </mugshots>
                objWriter.WriteEndElement();
            }
        }
        #endregion
    }
}
