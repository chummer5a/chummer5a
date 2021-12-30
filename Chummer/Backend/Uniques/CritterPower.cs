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
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using NLog;

namespace Chummer
{
    /// <summary>
    /// A Critter Power.
    /// </summary>
    [HubClassTag("SourceID", true, "Name", "Extra")]
    [DebuggerDisplay("{DisplayName(GlobalSettings.DefaultLanguage)}")]
    public class CritterPower : IHasInternalId, IHasName, IHasXmlNode, IHasNotes, ICanRemove, IHasSource, ICanSort
    {
        private static Logger Log { get; } = LogManager.GetCurrentClassLogger();
        private Guid _guiID;
        private Guid _guiSourceID;
        private string _strName = string.Empty;
        private string _strCategory = string.Empty;
        private string _strType = string.Empty;
        private string _strAction = string.Empty;
        private string _strRange = string.Empty;
        private string _strDuration = string.Empty;
        private string _strExtra = string.Empty;
        private string _strSource = string.Empty;
        private string _strPage = string.Empty;
        private int _intKarma;
        private decimal _decPowerPoints;
        private XmlNode _nodBonus;
        private string _strNotes = string.Empty;
        private Color _colNotes = ColorManager.HasNotesColor;
        private readonly Character _objCharacter;
        private bool _blnCountTowardsLimit = true;
        private int _intRating;
        private int _intGrade;
        private int _intSortOrder;

        #region Constructor, Create, Save, Load, and Print Methods

        public CritterPower(Character objCharacter)
        {
            // Create the GUID for the new Power.
            _guiID = Guid.NewGuid();
            _objCharacter = objCharacter;
        }

        /// Create a Critter Power from an XmlNode.
        /// <param name="objXmlPowerNode">XmlNode to create the object from.</param>
        /// <param name="intRating">Selected Rating for the Gear.</param>
        /// <param name="strForcedValue">Value to forcefully select for any ImprovementManager prompts.</param>
        public void Create(XmlNode objXmlPowerNode, int intRating = 0, string strForcedValue = "")
        {
            if (!objXmlPowerNode.TryGetField("id", Guid.TryParse, out _guiSourceID))
            {
                Log.Warn(new object[] { "Missing id field for power xmlnode", objXmlPowerNode });
                Utils.BreakIfDebug();
            }
            if (objXmlPowerNode.TryGetStringFieldQuickly("name", ref _strName))
                _objCachedMyXmlNode = null;
            _intRating = intRating;
            _nodBonus = objXmlPowerNode.SelectSingleNode("bonus");
            if (!objXmlPowerNode.TryGetMultiLineStringFieldQuickly("altnotes", ref _strNotes))
                objXmlPowerNode.TryGetMultiLineStringFieldQuickly("notes", ref _strNotes);

            string sNotesColor = ColorTranslator.ToHtml(ColorManager.HasNotesColor);
            objXmlPowerNode.TryGetStringFieldQuickly("notesColor", ref sNotesColor);
            _colNotes = ColorTranslator.FromHtml(sNotesColor);

            // If the piece grants a bonus, pass the information to the Improvement Manager.
            if (_nodBonus != null)
            {
                ImprovementManager.ForcedValue = strForcedValue;
                if (!ImprovementManager.CreateImprovements(_objCharacter, Improvement.ImprovementSource.CritterPower, _guiID.ToString("D", GlobalSettings.InvariantCultureInfo), _nodBonus, intRating, DisplayNameShort(GlobalSettings.Language)))
                {
                    _guiID = Guid.Empty;
                    return;
                }
                if (!string.IsNullOrEmpty(ImprovementManager.SelectedValue))
                {
                    _strExtra = ImprovementManager.SelectedValue;
                }
                else if (intRating != 0)
                    _strExtra = intRating.ToString(GlobalSettings.InvariantCultureInfo);
            }
            else if (intRating != 0)
                _strExtra = intRating.ToString(GlobalSettings.InvariantCultureInfo);
            else
                _strExtra = strForcedValue;
            objXmlPowerNode.TryGetStringFieldQuickly("category", ref _strCategory);
            objXmlPowerNode.TryGetStringFieldQuickly("type", ref _strType);
            objXmlPowerNode.TryGetStringFieldQuickly("action", ref _strAction);
            objXmlPowerNode.TryGetStringFieldQuickly("range", ref _strRange);
            objXmlPowerNode.TryGetStringFieldQuickly("duration", ref _strDuration);
            objXmlPowerNode.TryGetStringFieldQuickly("source", ref _strSource);
            objXmlPowerNode.TryGetStringFieldQuickly("page", ref _strPage);
            objXmlPowerNode.TryGetInt32FieldQuickly("karma", ref _intKarma);

            if (string.IsNullOrEmpty(Notes))
            {
                Notes = CommonFunctions.GetBookNotes(objXmlPowerNode, Name, CurrentDisplayName, Source, Page,
                    DisplayPage(GlobalSettings.Language), _objCharacter);
            }
        }

        private SourceString _objCachedSourceDetail;

        public SourceString SourceDetail
        {
            get
            {
                if (_objCachedSourceDetail == default)
                    _objCachedSourceDetail = new SourceString(Source,
                        DisplayPage(GlobalSettings.Language), GlobalSettings.Language, GlobalSettings.CultureInfo,
                        _objCharacter);
                return _objCachedSourceDetail;
            }
        }

        /// <summary>
        /// Save the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        public void Save(XmlTextWriter objWriter)
        {
            if (objWriter == null)
                return;
            objWriter.WriteStartElement("critterpower");
            objWriter.WriteElementString("sourceid", SourceIDString);
            objWriter.WriteElementString("guid", InternalId);
            objWriter.WriteElementString("name", _strName);
            objWriter.WriteElementString("extra", _strExtra);
            objWriter.WriteElementString("rating", _intRating.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("category", _strCategory);
            objWriter.WriteElementString("type", _strType);
            objWriter.WriteElementString("action", _strAction);
            objWriter.WriteElementString("range", _strRange);
            objWriter.WriteElementString("duration", _strDuration);
            objWriter.WriteElementString("grade", _intGrade.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("source", _strSource);
            objWriter.WriteElementString("page", _strPage);
            objWriter.WriteElementString("karma", _intKarma.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("points", _decPowerPoints.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("counttowardslimit", _blnCountTowardsLimit.ToString(GlobalSettings.InvariantCultureInfo));
            if (_nodBonus != null)
                objWriter.WriteRaw("<bonus>" + _nodBonus.InnerXml + "</bonus>");
            else
                objWriter.WriteElementString("bonus", string.Empty);
            objWriter.WriteElementString("notes", System.Text.RegularExpressions.Regex.Replace(_strNotes, @"[\u0000-\u0008\u000B\u000C\u000E-\u001F]", ""));
            objWriter.WriteElementString("notesColor", ColorTranslator.ToHtml(_colNotes));
            objWriter.WriteElementString("sortorder", _intSortOrder.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteEndElement();
        }

        /// <summary>
        /// Load the Critter Power from the XmlNode.
        /// </summary>
        /// <param name="objNode">XmlNode to load.</param>
        public void Load(XmlNode objNode)
        {
            if (objNode == null)
                return;
            if (!objNode.TryGetField("guid", Guid.TryParse, out _guiID))
            {
                _guiID = Guid.NewGuid();
            }
            if (objNode.TryGetStringFieldQuickly("name", ref _strName))
                _objCachedMyXmlNode = null;
            if (!objNode.TryGetGuidFieldQuickly("sourceid", ref _guiSourceID))
            {
                XmlNode node = GetNode(GlobalSettings.Language);
                node?.TryGetGuidFieldQuickly("id", ref _guiSourceID);
            }
            objNode.TryGetStringFieldQuickly("extra", ref _strExtra);
            objNode.TryGetStringFieldQuickly("category", ref _strCategory);
            objNode.TryGetStringFieldQuickly("type", ref _strType);
            objNode.TryGetStringFieldQuickly("action", ref _strAction);
            objNode.TryGetStringFieldQuickly("range", ref _strRange);
            objNode.TryGetStringFieldQuickly("duration", ref _strDuration);
            objNode.TryGetStringFieldQuickly("source", ref _strSource);
            objNode.TryGetStringFieldQuickly("page", ref _strPage);
            objNode.TryGetInt32FieldQuickly("karma", ref _intKarma);
            objNode.TryGetDecFieldQuickly("points", ref _decPowerPoints);
            objNode.TryGetBoolFieldQuickly("counttowardslimit", ref _blnCountTowardsLimit);
            objNode.TryGetInt32FieldQuickly("grade", ref _intGrade);
            _nodBonus = objNode["bonus"];
            objNode.TryGetMultiLineStringFieldQuickly("notes", ref _strNotes);

            string sNotesColor = ColorTranslator.ToHtml(ColorManager.HasNotesColor);
            objNode.TryGetStringFieldQuickly("notesColor", ref sNotesColor);
            _colNotes = ColorTranslator.FromHtml(sNotesColor);

            objNode.TryGetInt32FieldQuickly("sortorder", ref _intSortOrder);
        }

        /// <summary>
        /// Print the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        /// <param name="strLanguageToPrint">Language in which to print</param>
        public void Print(XmlTextWriter objWriter, string strLanguageToPrint)
        {
            if (objWriter == null)
                return;
            objWriter.WriteStartElement("critterpower");
            objWriter.WriteElementString("guid", InternalId);
            objWriter.WriteElementString("sourceid", SourceIDString);
            objWriter.WriteElementString("name", DisplayNameShort(strLanguageToPrint));
            objWriter.WriteElementString("fullname", DisplayName(strLanguageToPrint));
            objWriter.WriteElementString("name_english", Name);
            objWriter.WriteElementString("extra", _objCharacter.TranslateExtra(_strExtra, strLanguageToPrint));
            objWriter.WriteElementString("category", DisplayCategory(strLanguageToPrint));
            objWriter.WriteElementString("category_english", Category);
            objWriter.WriteElementString("type", DisplayType(strLanguageToPrint));
            objWriter.WriteElementString("action", DisplayAction(strLanguageToPrint));
            objWriter.WriteElementString("range", DisplayRange(strLanguageToPrint));
            objWriter.WriteElementString("duration", DisplayDuration(strLanguageToPrint));
            objWriter.WriteElementString("karma", Karma.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("source", _objCharacter.LanguageBookShort(Source, strLanguageToPrint));
            objWriter.WriteElementString("page", DisplayPage(strLanguageToPrint));
            if (GlobalSettings.PrintNotes)
                objWriter.WriteElementString("notes", Notes);
            objWriter.WriteEndElement();
        }

        #endregion Constructor, Create, Save, Load, and Print Methods

        #region Properties

        /// <summary>
        /// Identifier of the object within data files.
        /// </summary>
        public Guid SourceID => _guiSourceID;

        /// <summary>
        /// String-formatted identifier of the <inheritdoc cref="SourceID"/> from the data files.
        /// </summary>
        public string SourceIDString => _guiSourceID.ToString("D", GlobalSettings.InvariantCultureInfo);

        /// <summary>
        /// Internal identifier which will be used to identify this Critter Power in the Improvement system.
        /// </summary>
        public string InternalId => _guiID.ToString("D", GlobalSettings.InvariantCultureInfo);

        /// <summary>
        /// Paid levels of the power.
        /// </summary>
        public int Rating
        {
            get => _intRating;
            set
            {
                if (Extra == Rating.ToString(GlobalSettings.InvariantCultureInfo))
                {
                    Extra = value.ToString(GlobalSettings.InvariantCultureInfo);
                }
                _intRating = value;
            }
        }

        /// <summary>
        /// Total rating of the power, including any bonus levels from Improvements.
        /// </summary>
        public int TotalRating
        {
            get
            {
                return Rating + ImprovementManager
                                .GetCachedImprovementListForValueOf(_objCharacter,
                                                                    Improvement.ImprovementType.CritterPowerLevel, Name)
                                .Sum(objImprovement => objImprovement.Rating);
            }
        }

        /// <summary>
        /// Power's name.
        /// </summary>
        public string Name
        {
            get => _strName;
            set
            {
                if (_strName != value)
                    _objCachedMyXmlNode = null;
                _strName = value;
            }
        }

        /// <summary>
        /// The name of the object as it should be displayed on printouts (translated name only).
        /// </summary>
        public string DisplayNameShort(string strLanguage)
        {
            if (strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                return Name;

            return GetNode(strLanguage)?["translate"]?.InnerText ?? Name;
        }

        /// <summary>
        /// The name of the object as it should be displayed in lists. Name (Extra).
        /// </summary>
        public string DisplayName(string strLanguage)
        {
            string strReturn = DisplayNameShort(strLanguage);

            if (!string.IsNullOrEmpty(Extra))
            {
                // Attempt to retrieve the CharacterAttribute name.
                strReturn += LanguageManager.GetString("String_Space", strLanguage) + '(' + _objCharacter.TranslateExtra(Extra, strLanguage) + ')';
            }

            return strReturn;
        }

        /// <summary>
        /// The name of the object as it should be displayed in lists. Name (Extra).
        /// </summary>
        public string CurrentDisplayName => DisplayName(GlobalSettings.Language);

        /// <summary>
        /// Extra information that should be applied to the name, like a linked CharacterAttribute.
        /// </summary>
        public string Extra
        {
            get => _strExtra;
            set => _strExtra = _objCharacter.ReverseTranslateExtra(value);
        }

        /// <summary>
        /// Grade of the Critter power
        /// </summary>
        public int Grade
        {
            get => _intGrade;
            set => _intGrade = value;
        }

        /// <summary>
        /// Sourcebook.
        /// </summary>
        public string Source
        {
            get => _strSource;
            set => _strSource = value;
        }

        /// <summary>
        /// Sourcebook Page Number.
        /// </summary>
        public string Page
        {
            get => _strPage;
            set => _strPage = value;
        }

        /// <summary>
        /// Sourcebook Page Number using a given language file.
        /// Returns Page if not found or the string is empty.
        /// </summary>
        /// <param name="strLanguage">Language file keyword to use.</param>
        /// <returns></returns>
        public string DisplayPage(string strLanguage)
        {
            if (strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                return Page;
            string s = GetNode(strLanguage)?["altpage"]?.InnerText ?? Page;
            return !string.IsNullOrWhiteSpace(s) ? s : Page;
        }

        /// <summary>
        /// Bonus node from the XML file.
        /// </summary>
        public XmlNode Bonus
        {
            get => _nodBonus;
            set => _nodBonus = value;
        }

        /// <summary>
        /// Translated Category.
        /// </summary>
        public string DisplayCategory(string strLanguage)
        {
            // Get the translated name if applicable.
            if (strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                return Category;

            return _objCharacter.LoadDataXPath("critterpowers.xml", strLanguage).SelectSingleNode("/chummer/categories/category[. = " + Category.CleanXPath() + "]/@translate")?.Value ?? Category;
        }

        /// <summary>
        /// Category.
        /// </summary>
        public string Category
        {
            get => _strCategory;
            set => _strCategory = value;
        }

        /// <summary>
        /// Type.
        /// </summary>
        public string Type
        {
            get => _strType;
            set => _strType = value;
        }

        /// <summary>
        /// Translated Type.
        /// </summary>
        public string DisplayType(string strLanguage)
        {
            switch (Type)
            {
                case "M":
                    return LanguageManager.GetString("String_SpellTypeMana", strLanguage);

                case "P":
                    return LanguageManager.GetString("String_SpellTypePhysical", strLanguage);
            }

            return LanguageManager.GetString("String_None", strLanguage);
        }

        /// <summary>
        /// Action.
        /// </summary>
        public string Action
        {
            get => _strAction;
            set => _strAction = value;
        }

        /// <summary>
        /// Translated Action.
        /// </summary>
        public string DisplayAction(string strLanguage)
        {
            switch (Action)
            {
                case "Auto":
                    return LanguageManager.GetString("String_ActionAutomatic", strLanguage);

                case "Free":
                    return LanguageManager.GetString("String_ActionFree", strLanguage);

                case "Simple":
                    return LanguageManager.GetString("String_ActionSimple", strLanguage);

                case "Complex":
                    return LanguageManager.GetString("String_ActionComplex", strLanguage);

                case "Special":
                    return LanguageManager.GetString("String_SpellDurationSpecial", strLanguage);
            }

            return LanguageManager.GetString("String_None", strLanguage);
        }

        /// <summary>
        /// Range.
        /// </summary>
        public string Range
        {
            get => _strRange;
            set => _strRange = value;
        }

        /// <summary>
        /// Translated Range.
        /// </summary>
        public string DisplayRange(string strLanguage)
        {
            return Range.CheapReplace("Self", () => LanguageManager.GetString("String_SpellRangeSelf", strLanguage))
                        .CheapReplace("Special", () => LanguageManager.GetString("String_SpellDurationSpecial", strLanguage))
                        .CheapReplace("LOS", () => LanguageManager.GetString("String_SpellRangeLineOfSight", strLanguage))
                        .CheapReplace("LOI", () => LanguageManager.GetString("String_SpellRangeLineOfInfluence", strLanguage))
                        .CheapReplace("Touch", () => LanguageManager.GetString("String_SpellRangeTouch", strLanguage)) // Short form to remain export-friendly
                        .CheapReplace("T", () => LanguageManager.GetString("String_SpellRangeTouch", strLanguage))
                        .CheapReplace("(A)", () => '(' + LanguageManager.GetString("String_SpellRangeArea", strLanguage) + ')')
                        .CheapReplace("MAG", () => LanguageManager.GetString("String_AttributeMAGShort", strLanguage));
        }

        /// <summary>
        /// Duration.
        /// </summary>
        public string Duration
        {
            get => _strDuration;
            set => _strDuration = value;
        }

        /// <summary>
        /// Translated Duration.
        /// </summary>
        public string DisplayDuration(string strLanguage)
        {
            string strReturn = Duration;

            switch (strReturn)
            {
                case "Instant":
                    return LanguageManager.GetString("String_SpellDurationInstantLong", strLanguage);

                case "Sustained":
                    return LanguageManager.GetString("String_SpellDurationSustained", strLanguage);

                case "Always":
                    return LanguageManager.GetString("String_SpellDurationAlways", strLanguage);

                case "Special":
                    return LanguageManager.GetString("String_SpellDurationSpecial", strLanguage);
            }

            return strReturn;
        }

        /// <summary>
        /// Power Points used by the Critter Power (Free Spirits only).
        /// </summary>
        public decimal PowerPoints
        {
            get => _decPowerPoints;
            set => _decPowerPoints = value;
        }

        /// <summary>
        /// Notes.
        /// </summary>
        public string Notes
        {
            get => _strNotes;
            set => _strNotes = value;
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
        /// Whether or not the Critter Power counts towards their total number of Critter Powers.
        /// </summary>
        public bool CountTowardsLimit
        {
            get => _blnCountTowardsLimit;
            set => _blnCountTowardsLimit = value;
        }

        /// <summary>
        /// Karma that the Critter must pay to take the power.
        /// </summary>
        public int Karma
        {
            get => _intKarma;
            set => _intKarma = value;
        }

        /// <summary>
        /// Used by our sorting algorithm to remember which order the user moves things to
        /// </summary>
        public int SortOrder
        {
            get => _intSortOrder;
            set => _intSortOrder = value;
        }

        private XmlNode _objCachedMyXmlNode;
        private string _strCachedXmlNodeLanguage = string.Empty;

        public XmlNode GetNode()
        {
            return GetNode(GlobalSettings.Language);
        }

        public XmlNode GetNode(string strLanguage)
        {
            if (_objCachedMyXmlNode == null || strLanguage != _strCachedXmlNodeLanguage || GlobalSettings.LiveCustomData)
            {
                _objCachedMyXmlNode = _objCharacter.LoadData("critterpowers.xml", strLanguage)
                                                   .SelectSingleNode(SourceID == Guid.Empty
                                                                         ? "/chummer/powers/power[name = "
                                                                           + Name.CleanXPath() + ']'
                                                                         : "/chummer/powers/power[id = "
                                                                           + SourceIDString.CleanXPath()
                                                                           + " or id = " + SourceIDString
                                                                               .ToUpperInvariant().CleanXPath()
                                                                           + ']');
                _strCachedXmlNodeLanguage = strLanguage;
            }
            return _objCachedMyXmlNode;
        }

        #endregion Properties

        #region UI Methods

        public TreeNode CreateTreeNode(ContextMenuStrip cmsCritterPower)
        {
            if (Grade != 0 && !string.IsNullOrEmpty(Source) && !_objCharacter.Settings.BookEnabled(Source))
                return null;

            TreeNode objNode = new TreeNode
            {
                Name = InternalId,
                ContextMenuStrip = cmsCritterPower,
                Text = CurrentDisplayName,
                Tag = this,
                ForeColor = PreferredColor,
                ToolTipText = Notes.WordWrap()
            };
            return objNode;
        }

        public Color PreferredColor
        {
            get
            {
                if (!string.IsNullOrEmpty(Notes))
                {
                    return Grade != 0
                        ? ColorManager.GenerateCurrentModeDimmedColor(NotesColor)
                        : ColorManager.GenerateCurrentModeColor(NotesColor);
                }
                return Grade != 0
                    ? ColorManager.GrayText
                    : ColorManager.WindowText;
            }
        }

        #endregion UI Methods

        public bool Remove(bool blnConfirmDelete = true)
        {
            if (blnConfirmDelete && !CommonFunctions.ConfirmDelete(LanguageManager.GetString("Message_DeleteCritterPower")))
                return false;

            ImprovementManager.RemoveImprovements(_objCharacter, Improvement.ImprovementSource.CritterPower, InternalId);

            return _objCharacter.CritterPowers.Remove(this);
        }

        public void SetSourceDetail(Control sourceControl)
        {
            if (_objCachedSourceDetail.Language != GlobalSettings.Language)
                _objCachedSourceDetail = default;
            SourceDetail.SetControl(sourceControl);
        }
    }
}
