using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;

namespace Chummer
{
    /// <summary>
    /// Type of Spirit.
    /// </summary>
    public enum SpiritType
    {
        Spirit = 0,
        Sprite = 1,
    }

    /// <summary>
    /// A Magician's Spirit or Technomancer's Sprite.
    /// </summary>
    public class Spirit : IHasInternalId, IHasName, IHasXmlNode, IHasMugshots, INotifyPropertyChanged
    {
        private Guid _guiId;
        private string _strName = string.Empty;
        private string _strCritterName = string.Empty;
        private int _intServicesOwed;
        private SpiritType _eEntityType = SpiritType.Spirit;
        private bool _blnBound = true;
        private int _intForce = 1;
        private string _strFileName = string.Empty;
        private string _strRelativeName = string.Empty;
        private string _strNotes = string.Empty;
        private readonly Character _objCharacter;
        private Character _objLinkedCharacter;

        private readonly List<Image> _lstMugshots = new List<Image>();
        private int _intMainMugshotIndex = -1;

        #region Helper Methods
        /// <summary>
        /// Convert a string to a SpiritType.
        /// </summary>
        /// <param name="strValue">String value to convert.</param>
        public static SpiritType ConvertToSpiritType(string strValue)
        {
            if (string.IsNullOrEmpty(strValue))
                return default(SpiritType);
            switch (strValue)
            {
                case "Spirit":
                    return SpiritType.Spirit;
                default:
                    return SpiritType.Sprite;
            }
        }
        #endregion

        #region Constructor, Save, Load, and Print Methods
        public Spirit(Character objCharacter)
        {
            // Create the GUID for the new Spirit.
            _objCharacter = objCharacter;
        }

        /// <summary>
        /// Save the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        public void Save(XmlTextWriter objWriter)
        {
            objWriter.WriteStartElement("spirit");
            objWriter.WriteElementString("guid", _guiId.ToString("D"));
            objWriter.WriteElementString("name", _strName);
            objWriter.WriteElementString("crittername", _strCritterName);
            objWriter.WriteElementString("services", _intServicesOwed.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("force", _intForce.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("bound", _blnBound.ToString());
            objWriter.WriteElementString("fettered", _blnFettered.ToString());
            objWriter.WriteElementString("type", _eEntityType.ToString());
            objWriter.WriteElementString("file", _strFileName);
            objWriter.WriteElementString("relative", _strRelativeName);
            objWriter.WriteElementString("notes", _strNotes);
            SaveMugshots(objWriter);
            objWriter.WriteEndElement();

            /* Disabled for now because we cannot change any properties in the linked character anyway
            if (LinkedCharacter?.IsSaving == false && !Program.MainForm.OpenCharacterForms.Any(x => x.CharacterObject == LinkedCharacter))
                LinkedCharacter.Save();
                */
        }

        /// <summary>
        /// Load the Spirit from the XmlNode.
        /// </summary>
        /// <param name="objNode">XmlNode to load.</param>
        public void Load(XmlNode objNode)
        {
            if (objNode == null)
                return;
            objNode.TryGetField("guid", Guid.TryParse, out _guiId);
            if (objNode.TryGetStringFieldQuickly("name", ref _strName))
                _objCachedMyXmlNode = null;
            objNode.TryGetStringFieldQuickly("crittername", ref _strCritterName);
            objNode.TryGetInt32FieldQuickly("services", ref _intServicesOwed);
            objNode.TryGetInt32FieldQuickly("force", ref _intForce);
            objNode.TryGetBoolFieldQuickly("bound", ref _blnBound);
            objNode.TryGetBoolFieldQuickly("fettered", ref _blnFettered);
            _eEntityType = ConvertToSpiritType(objNode["type"]?.InnerText);
            objNode.TryGetStringFieldQuickly("file", ref _strFileName);
            objNode.TryGetStringFieldQuickly("relative", ref _strRelativeName);
            objNode.TryGetStringFieldQuickly("notes", ref _strNotes);

            RefreshLinkedCharacter(false);

            LoadMugshots(objNode);
        }

        /// <summary>
        /// Print the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        public void Print(XmlTextWriter objWriter, CultureInfo objCulture, string strLanguageToPrint)
        {
            // Translate the Critter name if applicable.
            string strName = Name;
            XmlNode objXmlCritterNode = GetNode(strLanguageToPrint);
            if (strLanguageToPrint != GlobalOptions.DefaultLanguage)
            {
                strName = objXmlCritterNode?["translate"]?.InnerText ?? Name;
            }

            objWriter.WriteStartElement("spirit");
            objWriter.WriteElementString("name", strName);
            objWriter.WriteElementString("name_english", Name);
            objWriter.WriteElementString("crittername", CritterName);
            objWriter.WriteElementString("fettered", Fettered.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("bound", Bound.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("services", ServicesOwed.ToString(objCulture));
            objWriter.WriteElementString("force", Force.ToString(objCulture));

            if (objXmlCritterNode != null)
            {
                //Attributes for spirits, named differently as to not confuse <attribtue>

                Dictionary<string, int> dicAttributes = new Dictionary<string, int>();
                objWriter.WriteStartElement("spiritattributes");
                foreach (string strAttribute in new String[] { "bod", "agi", "rea", "str", "cha", "int", "wil", "log", "ini" })
                {
                    string strInner = string.Empty;
                    if (objXmlCritterNode.TryGetStringFieldQuickly(strAttribute, ref strInner))
                    {
                        int intValue = 1;
                        try
                        {
                            intValue = Convert.ToInt32(CommonFunctions.EvaluateInvariantXPath(strInner.Replace("F", _intForce.ToString())));
                        }
                        catch (XPathException)
                        {
                            if (!int.TryParse(strInner, out intValue))
                            {
                                intValue = _intForce; //if failed to parse, default to force
                            }
                        }
                        catch (OverflowException)
                        {
                            if (!int.TryParse(strInner, out intValue))
                            {
                                intValue = _intForce; //if failed to parse, default to force
                            }
                        }
                        catch (FormatException)
                        {
                            if (!int.TryParse(strInner, out intValue))
                            {
                                intValue = _intForce; //if failed to parse, default to force
                            }
                        }
                        intValue = Math.Max(intValue, 1); //Min value is 1
                        objWriter.WriteElementString(strAttribute, intValue.ToString(objCulture));

                        dicAttributes[strAttribute] = intValue;
                    }
                }

                objWriter.WriteEndElement();

                //Dump skills, (optional)powers if present to output

                XmlDocument objXmlPowersDocument = XmlManager.Load("spiritpowers.xml", strLanguageToPrint);
                XmlNode xmlPowersNode = objXmlCritterNode["powers"];
                if (xmlPowersNode != null)
                {
                    objWriter.WriteStartElement("powers");
                    foreach (XmlNode objXmlPowerNode in xmlPowersNode.ChildNodes)
                    {
                        PrintPowerInfo(objWriter, objXmlPowersDocument, objXmlPowerNode.InnerText, GlobalOptions.Language);
                    }
                    objWriter.WriteEndElement();
                }
                xmlPowersNode = objXmlCritterNode["optionalpowers"];
                if (xmlPowersNode != null)
                {
                    objWriter.WriteStartElement("optionalpowers");
                    foreach (XmlNode objXmlPowerNode in xmlPowersNode.ChildNodes)
                    {
                        PrintPowerInfo(objWriter, objXmlPowersDocument, objXmlPowerNode.InnerText, GlobalOptions.Language);
                    }
                    objWriter.WriteEndElement();
                }

                xmlPowersNode = objXmlCritterNode["skills"];
                if (xmlPowersNode != null)
                {
                    objWriter.WriteStartElement("skills");
                    foreach (XmlNode xmlSkillNode in xmlPowersNode.ChildNodes)
                    {
                        string strAttrName = xmlSkillNode.Attributes?["attr"]?.Value;
                        if (!dicAttributes.TryGetValue(strAttrName, out int intAttrValue))
                            intAttrValue = _intForce;
                        int intDicepool = intAttrValue + _intForce;

                        objWriter.WriteStartElement("skill");
                        objWriter.WriteElementString("name", xmlSkillNode.InnerText);
                        objWriter.WriteElementString("attr", strAttrName);
                        objWriter.WriteElementString("pool", intDicepool.ToString(objCulture));
                        objWriter.WriteEndElement();
                    }
                    objWriter.WriteEndElement();
                }

                xmlPowersNode = objXmlCritterNode["weaknesses"];
                if (xmlPowersNode != null)
                {
                    objWriter.WriteStartElement("weaknesses");
                    foreach (XmlNode objXmlPowerNode in xmlPowersNode.ChildNodes)
                    {
                        PrintPowerInfo(objWriter, objXmlPowersDocument, objXmlPowerNode.InnerText, GlobalOptions.Language);
                    }
                    objWriter.WriteEndElement();
                }

                //Page in book for reference
                string strSource = string.Empty;
                string strPage = string.Empty;

                if (objXmlCritterNode.TryGetStringFieldQuickly("source", ref strSource))
                    objWriter.WriteElementString("source", CommonFunctions.LanguageBookShort(strSource, strLanguageToPrint));
                if (objXmlCritterNode.TryGetStringFieldQuickly("altpage", ref strPage) || objXmlCritterNode.TryGetStringFieldQuickly("page", ref strPage))
                    objWriter.WriteElementString("page", strPage);
            }

            objWriter.WriteElementString("bound", Bound.ToString());
            objWriter.WriteElementString("type", EntityType.ToString());

            if (_objCharacter.Options.PrintNotes)
                objWriter.WriteElementString("notes", Notes);
            PrintMugshots(objWriter);
            objWriter.WriteEndElement();
        }

        private static void PrintPowerInfo(XmlTextWriter objWriter, XmlDocument objXmlDocument, string strPowerName, string strLanguageToPrint)
        {
            StringBuilder strExtra = new StringBuilder();
            string strSource = string.Empty;
            string strPage = string.Empty;
            string strEnglishName = strPowerName;
            string strEnglishCategory = string.Empty;
            string strCategory = string.Empty;
            string strDisplayType = string.Empty;
            string strDisplayAction = string.Empty;
            string strDisplayRange = string.Empty;
            string strDisplayDuration = string.Empty;
            XmlNode objXmlPowerNode = objXmlDocument.SelectSingleNode("/chummer/powers/power[name = \"" + strPowerName + "\"]") ??
                objXmlDocument.SelectSingleNode("/chummer/powers/power[starts-with(\"" + strPowerName + "\", name)]");
            if (objXmlPowerNode != null)
            {
                objXmlPowerNode.TryGetStringFieldQuickly("source", ref strSource);
                if (!objXmlPowerNode.TryGetStringFieldQuickly("altpage", ref strPage))
                    objXmlPowerNode.TryGetStringFieldQuickly("page", ref strPage);

                objXmlPowerNode.TryGetStringFieldQuickly("name", ref strEnglishName);
                string[] lstExtras = strPowerName.TrimStart(strEnglishName).Trim().TrimStart('(').TrimEnd(')').Split(',');
                foreach (string strLoopExtra in lstExtras)
                {
                    strExtra.Append(LanguageManager.TranslateExtra(strLoopExtra, strLanguageToPrint));
                    strExtra.Append(", ");
                }
                if (strExtra.Length > 0)
                    strExtra.Length -= 2;

                if (objXmlPowerNode.TryGetStringFieldQuickly("translate", ref strPowerName))
                    strPowerName = strEnglishName;

                objXmlPowerNode.TryGetStringFieldQuickly("category", ref strEnglishCategory);

                strCategory = objXmlDocument.SelectSingleNode("/chummer/categories/category[. = \"" + strEnglishCategory + "\"]/@translate")?.InnerText ?? strEnglishCategory;

                switch (objXmlPowerNode["type"]?.InnerText)
                {
                    case "M":
                        strDisplayType = LanguageManager.GetString("String_SpellTypeMana", strLanguageToPrint);
                        break;
                    case "P":
                        strDisplayType = LanguageManager.GetString("String_SpellTypePhysical", strLanguageToPrint);
                        break;
                }
                switch (objXmlPowerNode["action"]?.InnerText)
                {
                    case "Auto":
                        strDisplayAction = LanguageManager.GetString("String_ActionAutomatic", strLanguageToPrint);
                        break;
                    case "Free":
                        strDisplayAction = LanguageManager.GetString("String_ActionFree", strLanguageToPrint);
                        break;
                    case "Simple":
                        strDisplayAction = LanguageManager.GetString("String_ActionSimple", strLanguageToPrint);
                        break;
                    case "Complex":
                        strDisplayAction = LanguageManager.GetString("String_ActionComplex", strLanguageToPrint);
                        break;
                    case "Special":
                        strDisplayAction = LanguageManager.GetString("String_SpellDurationSpecial", strLanguageToPrint);
                        break;
                }
                switch (objXmlPowerNode["duration"]?.InnerText)
                {
                    case "Instant":
                        strDisplayDuration = LanguageManager.GetString("String_SpellDurationInstantLong", strLanguageToPrint);
                        break;
                    case "Sustained":
                        strDisplayDuration = LanguageManager.GetString("String_SpellDurationSustained", strLanguageToPrint);
                        break;
                    case "Always":
                        strDisplayDuration = LanguageManager.GetString("String_SpellDurationAlways", strLanguageToPrint);
                        break;
                    case "Special":
                        strDisplayDuration = LanguageManager.GetString("String_SpellDurationSpecial", strLanguageToPrint);
                        break;
                }

                if (objXmlPowerNode.TryGetStringFieldQuickly("range", ref strDisplayRange))
                {
                    strDisplayRange = strDisplayRange.CheapReplace("Self", () => LanguageManager.GetString("String_SpellRangeSelf", strLanguageToPrint))
                        .CheapReplace("Special", () => LanguageManager.GetString("String_SpellDurationSpecial", strLanguageToPrint))
                        .CheapReplace("LOS", () => LanguageManager.GetString("String_SpellRangeLineOfSight", strLanguageToPrint))
                        .CheapReplace("LOI", () => LanguageManager.GetString("String_SpellRangeLineOfInfluence", strLanguageToPrint))
                        .CheapReplace("T", () => LanguageManager.GetString("String_SpellRangeTouch", strLanguageToPrint))
                        .CheapReplace("(A)", () => '(' + LanguageManager.GetString("String_SpellRangeArea", strLanguageToPrint) + ')')
                        .CheapReplace("MAG", () => LanguageManager.GetString("String_AttributeMAGShort", strLanguageToPrint));
                }
            }

            if (string.IsNullOrEmpty(strDisplayType))
                strDisplayType = LanguageManager.GetString("String_None", strLanguageToPrint);
            if (string.IsNullOrEmpty(strDisplayAction))
                strDisplayAction = LanguageManager.GetString("String_None", strLanguageToPrint);

            objWriter.WriteStartElement("critterpower");
            objWriter.WriteElementString("name", strPowerName);
            objWriter.WriteElementString("name_english", strEnglishName);
            objWriter.WriteElementString("extra", strExtra.ToString());
            objWriter.WriteElementString("category", strCategory);
            objWriter.WriteElementString("category_english", strEnglishCategory);
            objWriter.WriteElementString("type", strDisplayType);
            objWriter.WriteElementString("action", strDisplayAction);
            objWriter.WriteElementString("range", strDisplayRange);
            objWriter.WriteElementString("duration", strDisplayDuration);
            objWriter.WriteElementString("source", CommonFunctions.LanguageBookShort(strSource, strLanguageToPrint));
            objWriter.WriteElementString("page", strPage);
            objWriter.WriteEndElement();
        }
        #endregion

        #region Properties
        /// <summary>
        /// The Character object being used by the Spirit.
        /// </summary>
        public Character CharacterObject => _objCharacter;

        /// <summary>
        /// Name of the Spirit's Metatype.
        /// </summary>
        public string Name
        {
            get => _strName;
            set
            {
                if (_strName != value)
                {
                    _objCachedMyXmlNode = null;
                    _strName = value;
                }
            }
        }

        /// <summary>
        /// Name of the Spirit.
        /// </summary>
        public string CritterName
        {
            get
            {
                if (LinkedCharacter != null)
                    return LinkedCharacter.CharacterName;
                return _strCritterName;
            }
            set => _strCritterName = value;
        }

        /// <summary>
        /// Number of Services the Spirit owes.
        /// </summary>
        public int ServicesOwed
        {
            get => _intServicesOwed;
            set
            {
                if (_intServicesOwed != value)
                {
                    _intServicesOwed = value;
                    if (!CharacterObject.Created)
                    {
                        // Retrieve the character's Summoning Skill Rating.
                        int intSkillValue = CharacterObject.SkillsSection.GetActiveSkill(EntityType == SpiritType.Spirit ? "Summoning" : "Compiling")?.Rating ?? 0;

                        if (_intServicesOwed > intSkillValue && !CharacterObject.IgnoreRules)
                        {
                            MessageBox.Show(LanguageManager.GetString(EntityType == SpiritType.Spirit ? "Message_SpiritServices" : "Message_SpriteServices", GlobalOptions.Language),
                                LanguageManager.GetString(EntityType == SpiritType.Spirit ? "MessageTitle_SpiritServices" : "MessageTitle_SpriteServices", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                            _intServicesOwed = intSkillValue;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// The Spirit's Force.
        /// </summary>
        public int Force
        {
            get => _intForce;
            set => _intForce = value;
        }

        /// <summary>
        /// Whether or not the Spirit is Bound.
        /// </summary>
        public bool Bound
        {
            get => _blnBound;
            set => _blnBound = value;
        }

        /// <summary>
        /// The Spirit's type, either Spirit or Sprite.
        /// </summary>
        public SpiritType EntityType
        {
            get => _eEntityType;
            set
            {
                if (_eEntityType != value)
                {
                    _objCachedMyXmlNode = null;
                    _eEntityType = value;
                }
            }
        }

        /// <summary>
        /// Name of the save file for this Spirit/Sprite.
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
            set => _strNotes = value;
        }

        private bool _blnFettered = false;
        public bool Fettered
        {
            get => _blnFettered;
            set
            {
                if (_blnFettered != value)
                {
                    if (value)
                    {
                        //Only one Fettered spirit is permitted. 
                        if (CharacterObject.Spirits.Any(objSpirit => objSpirit.Fettered))
                        {
                            return;
                        }
                        if (CharacterObject.Created)
                        {
                            int FetteringCost = Force * 3;
                            if (!CharacterObject.ConfirmKarmaExpense(LanguageManager.GetString("Message_ConfirmKarmaExpenseSpend", GlobalOptions.Language)
                                        .Replace("{0}", Name)
                                        .Replace("{1}", FetteringCost.ToString())))
                            {
                                return;
                            }

                            // Create the Expense Log Entry.
                            ExpenseLogEntry objExpense = new ExpenseLogEntry(CharacterObject);
                            objExpense.Create(FetteringCost * -1,
                                LanguageManager.GetString("String_ExpenseFetteredSpirit", GlobalOptions.Language) + ' ' + Name,
                                ExpenseType.Karma, DateTime.Now);
                            CharacterObject.ExpenseEntries.Add(objExpense);
                            CharacterObject.Karma -= FetteringCost;

                            ExpenseUndo objUndo = new ExpenseUndo();
                            objUndo.CreateKarma(KarmaExpenseType.SpiritFettering, InternalId);
                            objExpense.Undo = objUndo;
                        }
                        ImprovementManager.CreateImprovement(CharacterObject, EntityType == SpiritType.Spirit ? "MAG" : "RES", Improvement.ImprovementSource.SpiritFettering, string.Empty, Improvement.ImprovementType.Attribute, string.Empty, 0, 1, 0, 0, -1);
                        ImprovementManager.Commit(CharacterObject);
                    }
                    else
                    {
                        ImprovementManager.RemoveImprovements(CharacterObject, Improvement.ImprovementSource.SpiritFettering);
                    }
                    _blnFettered = value;
                }
            }
        }

        public string InternalId => _guiId.ToString("D");

        public event PropertyChangedEventHandler PropertyChanged;

        private XmlNode _objCachedMyXmlNode = null;
        private string _strCachedXmlNodeLanguage = string.Empty;

        public XmlNode GetNode()
        {
            return GetNode(GlobalOptions.Language);
        }

        public XmlNode GetNode(string strLanguage)
        {
            if (_objCachedMyXmlNode == null || strLanguage != _strCachedXmlNodeLanguage || GlobalOptions.LiveCustomData)
            {
                _objCachedMyXmlNode = XmlManager.Load(_eEntityType == SpiritType.Spirit ? "traditions.xml" : "streams.xml", strLanguage).SelectSingleNode("/chummer/spirits/spirit[name = \"" + Name + "\"]");
                _strCachedXmlNodeLanguage = strLanguage;
            }
            return _objCachedMyXmlNode;
        }

        public Character LinkedCharacter
        {
            get => _objLinkedCharacter;
        }

        public bool NoLinkedCharacter
        {
            get => _objLinkedCharacter == null;
        }

        public void RefreshLinkedCharacter(bool blnShowError)
        {
            Character _objOldLinkedCharacter = _objLinkedCharacter;
            _objCharacter.LinkedCharacters.Remove(_objLinkedCharacter);
            _objLinkedCharacter = null;
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
                    MessageBox.Show(LanguageManager.GetString("Message_FileNotFound", GlobalOptions.Language).Replace("{0}", FileName), LanguageManager.GetString("MessageTitle_FileNotFound", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            if (!blnError)
            {
                string strFile = blnUseRelative ? Path.GetFullPath(RelativeFileName) : FileName;
                if (strFile.EndsWith(".chum5"))
                {
                    Character objOpenCharacter = Program.MainForm.OpenCharacters.FirstOrDefault(x => x.FileName == strFile);
                    if (objOpenCharacter != null)
                        _objLinkedCharacter = objOpenCharacter;
                    else
                        _objLinkedCharacter = Program.MainForm.LoadCharacter(strFile, string.Empty, false, false);
                    if (_objLinkedCharacter != null)
                        _objCharacter.LinkedCharacters.Add(_objLinkedCharacter);
                }
            }
            if (_objLinkedCharacter != _objOldLinkedCharacter)
            {
                if (_objOldLinkedCharacter != null)
                {
                    if (!Program.MainForm.OpenCharacters.Any(x => x.LinkedCharacters.Contains(_objOldLinkedCharacter) && x != _objOldLinkedCharacter))
                    {
                        Program.MainForm.OpenCharacters.Remove(_objOldLinkedCharacter);
                        _objOldLinkedCharacter.Dispose();
                    }
                }
                if (_objLinkedCharacter != null)
                {
                    if (string.IsNullOrEmpty(_strCritterName) && CritterName != LanguageManager.GetString("String_UnnamedCharacter", GlobalOptions.Language))
                        _strCritterName = CritterName;
                }
                PropertyChangedEventHandler objPropertyChanged = PropertyChanged;
                if (objPropertyChanged != null)
                {
                    objPropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(CritterName)));
                    objPropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(NoLinkedCharacter)));
                }
            }
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
                else
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
                else if (MainMugshotIndex >= Mugshots.Count || MainMugshotIndex < 0)
                    return null;
                else
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
                else
                    return _intMainMugshotIndex;
            }
            set
            {
                if (LinkedCharacter != null)
                    LinkedCharacter.MainMugshotIndex = value;
                else if (value >= _lstMugshots.Count || value < -1)
                    _intMainMugshotIndex = -1;
                else
                    _intMainMugshotIndex = value;
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

        public void LoadMugshots(XmlNode xmlSavedNode)
        {
            xmlSavedNode.TryGetInt32FieldQuickly("mainmugshotindex", ref _intMainMugshotIndex);
            using (XmlNodeList xmlMugshotsList = xmlSavedNode.SelectNodes("mugshots/mugshot"))
            {
                if (xmlMugshotsList != null)
                {
                    List<string> lstMugshotsBase64 = new List<string>(xmlMugshotsList.Count);
                    foreach (XmlNode objXmlMugshot in xmlMugshotsList)
                    {
                        string strMugshot = objXmlMugshot.InnerText;
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
                string strMugshotsDirectoryPath = Path.Combine(Application.StartupPath, "mugshots");
                if (!Directory.Exists(strMugshotsDirectoryPath))
                {
                    try
                    {
                        Directory.CreateDirectory(strMugshotsDirectoryPath);
                    }
                    catch (UnauthorizedAccessException)
                    {
                        MessageBox.Show(LanguageManager.GetString("Message_Insufficient_Permissions_Warning", GlobalOptions.Language));
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
