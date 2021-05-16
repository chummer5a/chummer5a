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
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
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
    [DebuggerDisplay("{Name}, \"{CritterName}\"")]
    public sealed class Spirit : IHasInternalId, IHasName, IHasXmlNode, IHasMugshots, INotifyPropertyChanged, IHasNotes
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
        private Color _colNotes = Color.Empty;
        private Character _objLinkedCharacter;

        private readonly List<Image> _lstMugshots = new List<Image>(1);
        private int _intMainMugshotIndex = -1;

        #region Helper Methods
        /// <summary>
        /// Convert a string to a SpiritType.
        /// </summary>
        /// <param name="strValue">String value to convert.</param>
        public static SpiritType ConvertToSpiritType(string strValue)
        {
            if (string.IsNullOrEmpty(strValue))
                return default;
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
            CharacterObject = objCharacter;
        }

        /// <summary>
        /// Save the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        public void Save(XmlTextWriter objWriter)
        {
            if (objWriter == null)
                return;
            objWriter.WriteStartElement("spirit");
            objWriter.WriteElementString("guid", _guiId.ToString("D", GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("name", _strName);
            objWriter.WriteElementString("crittername", _strCritterName);
            objWriter.WriteElementString("services", _intServicesOwed.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("force", _intForce.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("bound", _blnBound.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("fettered", _blnFettered.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("type", _eEntityType.ToString());
            objWriter.WriteElementString("file", _strFileName);
            objWriter.WriteElementString("relative", _strRelativeName);
            objWriter.WriteElementString("notes", System.Text.RegularExpressions.Regex.Replace(_strNotes, @"[\u0000-\u0008\u000B\u000C\u000E-\u001F]", ""));
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
        public void Load(XPathNavigator objNode)
        {
            if (objNode == null)
                return;
            objNode.TryGetField("guid", Guid.TryParse, out _guiId);
            if (objNode.TryGetStringFieldQuickly("name", ref _strName))
                _objCachedMyXmlNode = null;
            string strTemp = string.Empty;
            if (objNode.TryGetStringFieldQuickly("type", ref strTemp))
                _eEntityType = ConvertToSpiritType(strTemp);
            objNode.TryGetStringFieldQuickly("crittername", ref _strCritterName);
            objNode.TryGetInt32FieldQuickly("services", ref _intServicesOwed);
            objNode.TryGetInt32FieldQuickly("force", ref _intForce);
            Force = _intForce;
            objNode.TryGetBoolFieldQuickly("bound", ref _blnBound);
            objNode.TryGetBoolFieldQuickly("fettered", ref _blnFettered);
            objNode.TryGetStringFieldQuickly("file", ref _strFileName);
            objNode.TryGetStringFieldQuickly("relative", ref _strRelativeName);
            objNode.TryGetStringFieldQuickly("notes", ref _strNotes);

            RefreshLinkedCharacter(false);

            LoadMugshots(objNode);
        }

        private static readonly string[] s_astrPrintAttributeLabels = {"bod", "agi", "rea", "str", "cha", "int", "wil", "log", "ini"};

        /// <summary>
        /// Print the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        /// <param name="objCulture">Culture in which to print numbers.</param>
        /// <param name="strLanguageToPrint">Language in which to print.</param>
        public void Print(XmlTextWriter objWriter, CultureInfo objCulture, string strLanguageToPrint)
        {
            if (objWriter == null)
                return;
            // Translate the Critter name if applicable.
            string strName = Name;
            XmlNode objXmlCritterNode = GetNode(strLanguageToPrint);
            if (strLanguageToPrint != GlobalOptions.DefaultLanguage)
            {
                strName = objXmlCritterNode?["translate"]?.InnerText ?? Name;
            }

            objWriter.WriteStartElement("spirit");
            objWriter.WriteElementString("guid", InternalId);
            objWriter.WriteElementString("name", strName);
            objWriter.WriteElementString("name_english", Name);
            objWriter.WriteElementString("crittername", CritterName);
            objWriter.WriteElementString("fettered", Fettered.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("bound", Bound.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("services", ServicesOwed.ToString(objCulture));
            objWriter.WriteElementString("force", Force.ToString(objCulture));
            objWriter.WriteElementString("ratinglabel", LanguageManager.GetString(RatingLabel, strLanguageToPrint));

            if (objXmlCritterNode != null)
            {
                //Attributes for spirits, named differently as to not confuse <attribute>

                Dictionary<string, int> dicAttributes = new Dictionary<string, int>();
                objWriter.WriteStartElement("spiritattributes");
                foreach (string strAttribute in s_astrPrintAttributeLabels)
                {
                    string strInner = string.Empty;
                    if (objXmlCritterNode.TryGetStringFieldQuickly(strAttribute, ref strInner))
                    {
                        object objProcess = CommonFunctions.EvaluateInvariantXPath(strInner.Replace("F", _intForce.ToString(GlobalOptions.InvariantCultureInfo)), out bool blnIsSuccess);
                        int intValue = Math.Max(blnIsSuccess ? ((double)objProcess).StandardRound() : _intForce, 1);
                        objWriter.WriteElementString(strAttribute, intValue.ToString(objCulture));

                        dicAttributes[strAttribute] = intValue;
                    }
                }

                objWriter.WriteEndElement();

                
                if (_objLinkedCharacter != null)
                {
                    //Dump skills, (optional)powers if present to output

                    XPathNavigator xmlSpiritPowersBaseChummerNode = _objLinkedCharacter.LoadDataXPath("spiritpowers.xml", strLanguageToPrint).SelectSingleNode("/chummer");
                    XPathNavigator xmlCritterPowersBaseChummerNode = _objLinkedCharacter.LoadDataXPath("critterpowers.xml", strLanguageToPrint).SelectSingleNode("/chummer");



                    XmlNode xmlPowersNode = objXmlCritterNode["powers"];
                    if (xmlPowersNode != null)
                    {
                        objWriter.WriteStartElement("powers");
                        foreach (XmlNode objXmlPowerNode in xmlPowersNode.ChildNodes)
                        {
                            PrintPowerInfo(objWriter, xmlSpiritPowersBaseChummerNode, xmlCritterPowersBaseChummerNode, objXmlPowerNode, strLanguageToPrint);
                        }
                        objWriter.WriteEndElement();
                    }

                    xmlPowersNode = objXmlCritterNode["optionalpowers"];
                    if (xmlPowersNode != null)
                    {
                        objWriter.WriteStartElement("optionalpowers");
                        foreach (XmlNode objXmlPowerNode in xmlPowersNode.ChildNodes)
                        {
                            PrintPowerInfo(objWriter, xmlSpiritPowersBaseChummerNode, xmlCritterPowersBaseChummerNode, objXmlPowerNode, strLanguageToPrint);
                        }
                        objWriter.WriteEndElement();
                    }

                    xmlPowersNode = objXmlCritterNode["skills"];
                    if (xmlPowersNode != null)
                    {
                        XPathNavigator xmlSkillsDocument = CharacterObject.LoadDataXPath("skills.xml", strLanguageToPrint);
                        objWriter.WriteStartElement("skills");
                        foreach (XmlNode xmlSkillNode in xmlPowersNode.ChildNodes)
                        {
                            string strAttrName = xmlSkillNode.Attributes?["attr"]?.Value ?? string.Empty;
                            if (!dicAttributes.TryGetValue(strAttrName, out int intAttrValue))
                                intAttrValue = _intForce;
                            int intDicepool = intAttrValue + _intForce;

                            string strEnglishName = xmlSkillNode.InnerText;
                            string strTranslatedName = xmlSkillsDocument.SelectSingleNode("/chummer/skills/skill[name = " + strEnglishName.CleanXPath() + "]/translate")?.Value ??
                                                       xmlSkillsDocument.SelectSingleNode("/chummer/knowledgeskills/skill[name = " + strEnglishName.CleanXPath() + "]/translate")?.Value ?? strEnglishName;
                            objWriter.WriteStartElement("skill");
                            objWriter.WriteElementString("name", strTranslatedName);
                            objWriter.WriteElementString("name_english", strEnglishName);
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
                            PrintPowerInfo(objWriter, xmlSpiritPowersBaseChummerNode, xmlCritterPowersBaseChummerNode, objXmlPowerNode, strLanguageToPrint);
                        }
                        objWriter.WriteEndElement();
                    }
                }
                //Page in book for reference
                string strSource = string.Empty;
                string strPage = string.Empty;

                if (objXmlCritterNode.TryGetStringFieldQuickly("source", ref strSource))
                    objWriter.WriteElementString("source", CharacterObject.LanguageBookShort(strSource, strLanguageToPrint));
                if (objXmlCritterNode.TryGetStringFieldQuickly("altpage", ref strPage) || objXmlCritterNode.TryGetStringFieldQuickly("page", ref strPage))
                    objWriter.WriteElementString("page", strPage);
            }

            objWriter.WriteElementString("bound", Bound.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("type", EntityType.ToString());

            if (GlobalOptions.PrintNotes)
                objWriter.WriteElementString("notes", Notes);
            PrintMugshots(objWriter);
            objWriter.WriteEndElement();
        }

        private void PrintPowerInfo(XmlTextWriter objWriter, XPathNavigator xmlSpiritPowersBaseChummerNode, XPathNavigator xmlCritterPowersBaseChummerNode, XmlNode xmlPowerEntryNode, string strLanguageToPrint = "")
        {
            StringBuilder sbdExtra = new StringBuilder();
            string strSelect = xmlPowerEntryNode.SelectSingleNode("@select")?.Value;
            if (!string.IsNullOrEmpty(strSelect))
                sbdExtra.Append(CharacterObject.TranslateExtra(strSelect, strLanguageToPrint));
            string strSource = string.Empty;
            string strPage = string.Empty;
            string strPowerName = xmlPowerEntryNode.InnerText;
            string strEnglishName = strPowerName;
            string strEnglishCategory = string.Empty;
            string strCategory = string.Empty;
            string strDisplayType = string.Empty;
            string strDisplayAction = string.Empty;
            string strDisplayRange = string.Empty;
            string strDisplayDuration = string.Empty;
            XPathNavigator objXmlPowerNode = xmlSpiritPowersBaseChummerNode.SelectSingleNode("powers/power[name = " + strPowerName.CleanXPath() + "]") ??
                                             xmlSpiritPowersBaseChummerNode.SelectSingleNode("powers/power[starts-with(" + strPowerName.CleanXPath() + ", name)]") ??
                                             xmlCritterPowersBaseChummerNode.SelectSingleNode("powers/power[name = " + strPowerName.CleanXPath() + "]") ??
                                             xmlCritterPowersBaseChummerNode.SelectSingleNode("powers/power[starts-with(" + strPowerName.CleanXPath() + ", name)]");
            if (objXmlPowerNode != null)
            {
                objXmlPowerNode.TryGetStringFieldQuickly("source", ref strSource);
                if (!objXmlPowerNode.TryGetStringFieldQuickly("altpage", ref strPage))
                    objXmlPowerNode.TryGetStringFieldQuickly("page", ref strPage);

                objXmlPowerNode.TryGetStringFieldQuickly("name", ref strEnglishName);
                bool blnExtrasAdded = false;
                foreach (string strLoopExtra in strPowerName.TrimStartOnce(strEnglishName).Trim().TrimStartOnce('(').TrimEndOnce(')').SplitNoAlloc(',', StringSplitOptions.RemoveEmptyEntries))
                {
                    blnExtrasAdded = true;
                    sbdExtra.Append(CharacterObject.TranslateExtra(strLoopExtra, strLanguageToPrint) + ", ");
                }
                if (blnExtrasAdded)
                    sbdExtra.Length -= 2;

                if (!objXmlPowerNode.TryGetStringFieldQuickly("translate", ref strPowerName))
                    strPowerName = strEnglishName;

                objXmlPowerNode.TryGetStringFieldQuickly("category", ref strEnglishCategory);

                strCategory = xmlSpiritPowersBaseChummerNode.SelectSingleNode("categories/category[. = " + strEnglishCategory.CleanXPath() + "]/@translate")?.Value ?? strEnglishCategory;

                switch (objXmlPowerNode.SelectSingleNode("type")?.Value)
                {
                    case "M":
                        strDisplayType = LanguageManager.GetString("String_SpellTypeMana", strLanguageToPrint);
                        break;
                    case "P":
                        strDisplayType = LanguageManager.GetString("String_SpellTypePhysical", strLanguageToPrint);
                        break;
                }
                switch (objXmlPowerNode.SelectSingleNode("action")?.Value)
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
                switch (objXmlPowerNode.SelectSingleNode("duration")?.Value)
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

                if (objXmlPowerNode.TryGetStringFieldQuickly("range", ref strDisplayRange) && strLanguageToPrint != GlobalOptions.DefaultLanguage)
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
            objWriter.WriteElementString("extra", sbdExtra.ToString());
            objWriter.WriteElementString("category", strCategory);
            objWriter.WriteElementString("category_english", strEnglishCategory);
            objWriter.WriteElementString("type", strDisplayType);
            objWriter.WriteElementString("action", strDisplayAction);
            objWriter.WriteElementString("range", strDisplayRange);
            objWriter.WriteElementString("duration", strDisplayDuration);
            objWriter.WriteElementString("source", CharacterObject.LanguageBookShort(strSource, strLanguageToPrint));
            objWriter.WriteElementString("page", strPage);
            objWriter.WriteEndElement();
        }
        #endregion

        #region Properties
        /// <summary>
        /// The Character object being used by the Spirit.
        /// </summary>
        public Character CharacterObject { get; }

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
                    OnPropertyChanged();
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
            set
            {
                if (_strCritterName != value)
                {
                    _strCritterName = value;
                    OnPropertyChanged();
                }
            }
        }

        public string RatingLabel
        {
            get
            {
                switch (EntityType)
                {
                    case SpiritType.Spirit:
                        return "String_Force";
                    case SpiritType.Sprite:
                        return "String_Level";
                    default:
                        return "String_Rating";
                }
            }
        }

        /// <summary>
        /// Number of Services the Spirit owes.
        /// </summary>
        public int ServicesOwed
        {
            get => _intServicesOwed;
            set
            {
                if (!CharacterObject.Created && !CharacterObject.IgnoreRules)
                {
                    // Retrieve the character's Summoning Skill Rating.
                    int intSkillValue = CharacterObject.SkillsSection.GetActiveSkill(EntityType == SpiritType.Spirit ? "Summoning" : "Compiling")?.Rating ?? 0;

                    if (value > intSkillValue)
                    {
                        Program.MainForm.ShowMessageBox(LanguageManager.GetString(EntityType == SpiritType.Spirit ? "Message_SpiritServices" : "Message_SpriteServices"),
                            LanguageManager.GetString(EntityType == SpiritType.Spirit ? "MessageTitle_SpiritServices" : "MessageTitle_SpriteServices"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                        value = intSkillValue;
                    }
                }
                if (_intServicesOwed != value)
                {
                    _intServicesOwed = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// The Spirit's Force.
        /// </summary>
        public int Force
        {
            get => _intForce;
            set
            {
                switch (EntityType)
                {
                    case SpiritType.Spirit when value > CharacterObject.MaxSpiritForce:
                        value = CharacterObject.MaxSpiritForce;
                        break;
                    case SpiritType.Sprite when value > CharacterObject.MaxSpriteLevel:
                        value = CharacterObject.MaxSpriteLevel;
                        break;
                }

                if (_intForce == value) return;
                _intForce = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Whether or not the Spirit is Bound.
        /// </summary>
        public bool Bound
        {
            get => _blnBound;
            set
            {
                if (_blnBound != value)
                {
                    _blnBound = value;
                    OnPropertyChanged();
                }
            }
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
                    OnPropertyChanged();
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
                    OnPropertyChanged();
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
                    OnPropertyChanged();
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

        private bool _blnFettered;
        private int _intCachedAllowFettering = int.MinValue;

        public bool AllowFettering
        {
            get
            {
                if (_intCachedAllowFettering < 0)
                    _intCachedAllowFettering = (EntityType == SpiritType.Spirit ||
                    EntityType == SpiritType.Sprite && CharacterObject.AllowSpriteFettering)
                        ? 1
                        : 0;
                return _intCachedAllowFettering > 0;
            }
        }
        /// <summary>
        /// Whether the sprite/spirit has unlimited services due to Fettering.
        /// See KC 91 and SG 192 for sprites and spirits, respectively.
        /// </summary>
        public bool Fettered
        {
            get
            {
                if (_intCachedAllowFettering < 0)
                    _intCachedAllowFettering = CharacterObject.AllowSpriteFettering
                        ? 1
                        : 0;
                return _blnFettered && _intCachedAllowFettering > 0;
            }

            set
            {
                if (_blnFettered == value) return;
                if (value)
                {
                    //Technomancers require the Sprite Pet Complex Form to Fetter sprites.
                    if (!CharacterObject.AllowSpriteFettering && EntityType == SpiritType.Sprite) return;

                    //Only one Fettered spirit is permitted.
                    if (CharacterObject.Spirits.Any(objSpirit => objSpirit.Fettered)) return;

                    if (CharacterObject.Created)
                    {
                        // Sprites only cost Force in Karma to become Fettered. Spirits cost Force * 3.
                        int fetteringCost = EntityType == SpiritType.Spirit ? Force * 3 : Force;
                        if (!CommonFunctions.ConfirmKarmaExpense(string.Format(GlobalOptions.CultureInfo, LanguageManager.GetString("Message_ConfirmKarmaExpenseSpend")
                            , Name
                            , fetteringCost.ToString(GlobalOptions.CultureInfo))))
                        {
                            return;
                        }

                        // Create the Expense Log Entry.
                        ExpenseLogEntry objExpense = new ExpenseLogEntry(CharacterObject);
                        objExpense.Create(fetteringCost * -1,
                            LanguageManager.GetString("String_ExpenseFetteredSpirit") + LanguageManager.GetString("String_Space") + Name,
                            ExpenseType.Karma, DateTime.Now);
                        CharacterObject.ExpenseEntries.AddWithSort(objExpense);
                        CharacterObject.Karma -= fetteringCost;

                        ExpenseUndo objUndo = new ExpenseUndo();
                        objUndo.CreateKarma(KarmaExpenseType.SpiritFettering, InternalId);
                        objExpense.Undo = objUndo;
                    }

                    if (EntityType == SpiritType.Spirit)
                    {
                        ImprovementManager.CreateImprovement(CharacterObject, "MAG",
                            Improvement.ImprovementSource.SpiritFettering, string.Empty,
                            Improvement.ImprovementType.Attribute, string.Empty, 0, 1, 0, 0, -1);
                        ImprovementManager.Commit(CharacterObject);
                    }
                }
                else
                {
                    ImprovementManager.RemoveImprovements(CharacterObject, Improvement.ImprovementSource.SpiritFettering);
                }
                _blnFettered = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Color used by the Spirit's control in UI.
        /// Placeholder to prevent me having to deal with multiple interfaces.
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

        public string InternalId => _guiId.ToString("D", GlobalOptions.InvariantCultureInfo);

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
                    lstNamesOfChangedProperties = SpiritDependencyGraph.GetWithAllDependents(this, strPropertyName);
                else
                {
                    foreach (string strLoopChangedProperty in SpiritDependencyGraph.GetWithAllDependents(this, strPropertyName))
                        lstNamesOfChangedProperties.Add(strLoopChangedProperty);
                }
            }

            if (lstNamesOfChangedProperties == null || lstNamesOfChangedProperties.Count == 0)
                return;

            foreach (string strPropertyToChange in lstNamesOfChangedProperties)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(strPropertyToChange));
            }
        }

        private static readonly DependencyGraph<string, Spirit> SpiritDependencyGraph =
            new DependencyGraph<string, Spirit>(
                new DependencyGraphNode<string, Spirit>(nameof(NoLinkedCharacter),
                    new DependencyGraphNode<string, Spirit>(nameof(LinkedCharacter))
                ),
                new DependencyGraphNode<string, Spirit>(nameof(CritterName),
                    new DependencyGraphNode<string, Spirit>(nameof(LinkedCharacter))
                ),
                new DependencyGraphNode<string, Spirit>(nameof(MainMugshot),
                    new DependencyGraphNode<string, Spirit>(nameof(LinkedCharacter)),
                    new DependencyGraphNode<string, Spirit>(nameof(Mugshots),
                        new DependencyGraphNode<string, Spirit>(nameof(LinkedCharacter))
                    ),
                    new DependencyGraphNode<string, Spirit>(nameof(MainMugshotIndex))
                )
            );

        private XmlNode _objCachedMyXmlNode;
        private string _strCachedXmlNodeLanguage = string.Empty;
        private Color _objColour;

        public XmlNode GetNode()
        {
            return GetNode(GlobalOptions.Language);
        }

        public XmlNode GetNode(string strLanguage)
        {
            if (_objCachedMyXmlNode == null || strLanguage != _strCachedXmlNodeLanguage || GlobalOptions.LiveCustomData)
            {
                _objCachedMyXmlNode = CharacterObject
                    .LoadData(_eEntityType == SpiritType.Spirit ? "traditions.xml" : "streams.xml", strLanguage)
                    .SelectSingleNode("/chummer/spirits/spirit[name = " + Name.CleanXPath() + "]");
                _strCachedXmlNodeLanguage = strLanguage;
            }
            return _objCachedMyXmlNode;
        }

        public Character LinkedCharacter => _objLinkedCharacter;

        public bool NoLinkedCharacter => _objLinkedCharacter == null;

        public void RefreshLinkedCharacter(bool blnShowError)
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
                    Program.MainForm.ShowMessageBox(string.Format(GlobalOptions.CultureInfo, LanguageManager.GetString("Message_FileNotFound"), FileName),
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
                    if (string.IsNullOrEmpty(_strCritterName) && CritterName != LanguageManager.GetString("String_UnnamedCharacter"))
                        _strCritterName = CritterName;

                    _objLinkedCharacter.PropertyChanged += LinkedCharacterOnPropertyChanged;
                }
                OnPropertyChanged(nameof(LinkedCharacter));
            }
        }

        private void LinkedCharacterOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Character.Name))
                OnPropertyChanged(nameof(CritterName));
            else if (e.PropertyName == nameof(Character.Mugshots))
                OnPropertyChanged(nameof(Mugshots));
            else if (e.PropertyName == nameof(Character.MainMugshot))
                OnPropertyChanged(nameof(MainMugshot));
            else if (e.PropertyName == nameof(Character.MainMugshotIndex))
                OnPropertyChanged(nameof(MainMugshotIndex));
            else if (e.PropertyName == nameof(Character.AllowSpriteFettering))
            {
                _intCachedAllowFettering = int.MinValue;
                OnPropertyChanged(nameof(AllowFettering));
                OnPropertyChanged(nameof(Fettered));
            }
        }
        #endregion

        #region IHasMugshots
        /// <summary>
        /// Character's portraits encoded using Base64.
        /// </summary>
        public List<Image> Mugshots
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
            if (objWriter == null)
                return;
            objWriter.WriteElementString("mainmugshotindex", MainMugshotIndex.ToString(GlobalOptions.InvariantCultureInfo));
            // <mugshot>
            objWriter.WriteStartElement("mugshots");
            foreach (Image imgMugshot in Mugshots)
            {
                objWriter.WriteElementString("mugshot", GlobalOptions.ImageToBase64StringForStorage(imgMugshot));
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
                Parallel.For(0, lstMugshotsBase64.Count, i => objMugshotImages[i] = lstMugshotsBase64[i].ToImage(PixelFormat.Format32bppPArgb));
                _lstMugshots.AddRange(objMugshotImages);
            }
            else if (lstMugshotsBase64.Count == 1)
            {
                _lstMugshots.Add(lstMugshotsBase64[0].ToImage(PixelFormat.Format32bppPArgb));
            }
        }

        public void PrintMugshots(XmlTextWriter objWriter)
        {
            if (objWriter == null)
                return;
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
                        Program.MainForm.ShowMessageBox(LanguageManager.GetString("Message_Insufficient_Permissions_Warning"));
                    }
                }
                Guid guiImage = Guid.NewGuid();
                string imgMugshotPath = Path.Combine(strMugshotsDirectoryPath, guiImage.ToString("N", GlobalOptions.InvariantCultureInfo) + ".img");
                Image imgMainMugshot = MainMugshot;
                if (imgMainMugshot != null)
                {
                    imgMainMugshot.Save(imgMugshotPath);
                    // <mainmugshotpath />
                    objWriter.WriteElementString("mainmugshotpath", "file://" + imgMugshotPath.Replace(Path.DirectorySeparatorChar, '/'));
                    // <mainmugshotbase64 />
                    objWriter.WriteElementString("mainmugshotbase64", GlobalOptions.ImageToBase64StringForStorage(imgMainMugshot));
                }
                // <othermugshots>
                objWriter.WriteElementString("hasothermugshots", (imgMainMugshot == null || Mugshots.Count > 1).ToString(GlobalOptions.InvariantCultureInfo));
                objWriter.WriteStartElement("othermugshots");
                for (int i = 0; i < Mugshots.Count; ++i)
                {
                    if (i == MainMugshotIndex)
                        continue;
                    Image imgMugshot = Mugshots[i];
                    objWriter.WriteStartElement("mugshot");

                    objWriter.WriteElementString("stringbase64", GlobalOptions.ImageToBase64StringForStorage(imgMugshot));

                    imgMugshotPath = Path.Combine(strMugshotsDirectoryPath, guiImage.ToString("N", GlobalOptions.InvariantCultureInfo) + i.ToString(GlobalOptions.InvariantCultureInfo) + ".img");
                    imgMugshot.Save(imgMugshotPath);
                    objWriter.WriteElementString("temppath", "file://" + imgMugshotPath.Replace(Path.DirectorySeparatorChar, '/'));

                    objWriter.WriteEndElement();
                }
                // </mugshots>
                objWriter.WriteEndElement();
            }
        }

        public void Dispose()
        {
            if (_objLinkedCharacter != null && !Utils.IsUnitTest
                                            && Program.MainForm.OpenCharacters.Contains(_objLinkedCharacter)
                                            && Program.MainForm.OpenCharacters.All(x => !x.LinkedCharacters.Contains(_objLinkedCharacter))
                                            && Program.MainForm.OpenCharacterForms.All(x => x.CharacterObject != _objLinkedCharacter))
                Program.MainForm.OpenCharacters.Remove(_objLinkedCharacter);
        }
        #endregion
    }
}
