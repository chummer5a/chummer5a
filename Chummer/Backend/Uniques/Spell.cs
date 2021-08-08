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
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using Chummer.Backend.Attributes;
using Chummer.Backend.Skills;
using NLog;

namespace Chummer
{
    /// <summary>
    /// A Magician Spell.
    /// </summary>
    [HubClassTag("SourceID", true, "Name", "Extra")]
    [DebuggerDisplay("{DisplayName(GlobalOptions.DefaultLanguage)}")]
    public class Spell : IHasInternalId, IHasName, IHasXmlNode, IHasNotes, ICanRemove, IHasSource
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private Guid _guiID;
        private Guid _guiSourceID = Guid.Empty;
        private string _strName = string.Empty;
        private string _strDescriptors = string.Empty;
        private string _strCategory = string.Empty;
        private string _strType = string.Empty;
        private string _strRange = string.Empty;
        private string _strDamage = string.Empty;
        private string _strDuration = string.Empty;
        private string _strDV = string.Empty;
        private string _strSource = string.Empty;
        private string _strPage = string.Empty;
        private string _strExtra = string.Empty;
        private bool _blnLimited;
        private bool _blnExtended;
        private bool _blnCustomExtended;
        private string _strNotes = string.Empty;
        private Color _colNotes = ColorManager.HasNotesColor;
        private readonly Character _objCharacter;
        private bool _blnAlchemical;
        private bool _blnFreeBonus;
        private bool _blnUsesUnarmed;
        private int _intGrade;

        private Improvement.ImprovementSource _objImprovementSource = Improvement.ImprovementSource.Spell;

        #region Constructor, Create, Save, Load, and Print Methods

        public Spell(Character objCharacter)
        {
            // Create the GUID for the new Spell.
            _guiID = Guid.NewGuid();
            _objCharacter = objCharacter;
        }

        /// Create a Spell from an XmlNode.
        /// <param name="objXmlSpellNode">XmlNode to create the object from.</param>
        /// <param name="strForcedValue">Value to forcefully select for any ImprovementManager prompts.</param>
        /// <param name="blnLimited">Whether or not the Spell should be marked as Limited.</param>
        /// <param name="blnExtended">Whether or not the Spell should be marked as Extended.</param>
        /// <param name="blnAlchemical">Whether or not the Spell is one for an alchemical preparation.</param>
        /// <param name="objSource">Enum representing the actual type of spell this object represents. Used for initiation benefits that would grant spells.</param>
        public void Create(XmlNode objXmlSpellNode, string strForcedValue = "", bool blnLimited = false, bool blnExtended = false, bool blnAlchemical = false, Improvement.ImprovementSource objSource = Improvement.ImprovementSource.Spell)
        {
            if (!objXmlSpellNode.TryGetField("id", Guid.TryParse, out _guiSourceID))
            {
                Log.Warn(new object[] { "Missing id field for xmlnode", objXmlSpellNode });
                Utils.BreakIfDebug();
            }
            objXmlSpellNode.TryGetStringFieldQuickly("name", ref _strName);

            ImprovementManager.ForcedValue = strForcedValue;
            if (objXmlSpellNode["bonus"] != null)
            {
                if (!ImprovementManager.CreateImprovements(_objCharacter, Improvement.ImprovementSource.Spell, _guiID.ToString("D", GlobalOptions.InvariantCultureInfo), objXmlSpellNode["bonus"], 1, DisplayNameShort(GlobalOptions.Language)))
                {
                    _guiID = Guid.Empty;
                    return;
                }
                if (!string.IsNullOrEmpty(ImprovementManager.SelectedValue))
                {
                    _strExtra = ImprovementManager.SelectedValue;
                }
            }
            if (!objXmlSpellNode.TryGetMultiLineStringFieldQuickly("altnotes", ref _strNotes))
                objXmlSpellNode.TryGetMultiLineStringFieldQuickly("notes", ref _strNotes);

            String sNotesColor = ColorTranslator.ToHtml(ColorManager.HasNotesColor);
            objXmlSpellNode.TryGetStringFieldQuickly("notesColor", ref sNotesColor);
            _colNotes = ColorTranslator.FromHtml(sNotesColor);

            if (objXmlSpellNode.TryGetStringFieldQuickly("descriptor", ref _strDescriptors))
                UpdateHashDescriptors();
            objXmlSpellNode.TryGetStringFieldQuickly("category", ref _strCategory);
            objXmlSpellNode.TryGetStringFieldQuickly("type", ref _strType);
            objXmlSpellNode.TryGetStringFieldQuickly("range", ref _strRange);
            objXmlSpellNode.TryGetStringFieldQuickly("damage", ref _strDamage);
            objXmlSpellNode.TryGetStringFieldQuickly("duration", ref _strDuration);
            objXmlSpellNode.TryGetStringFieldQuickly("dv", ref _strDV);
            _blnLimited = blnLimited;
            _blnAlchemical = blnAlchemical;
            objXmlSpellNode.TryGetStringFieldQuickly("source", ref _strSource);
            objXmlSpellNode.TryGetStringFieldQuickly("page", ref _strPage);
            _objImprovementSource = objSource;

            _blnExtended = blnExtended;
            if (blnExtended)
            {
                _blnCustomExtended = !HashDescriptors.Any(x =>
                    string.Equals(x.Trim(), "Extended Area", StringComparison.OrdinalIgnoreCase));
            }

            /*
            if (string.IsNullOrEmpty(_strNotes))
            {
                _strNotes = CommonFunctions.GetText(_strSource + ' ' + _strPage, Name);
            }
            */
        }

        private SourceString _objCachedSourceDetail;

        public SourceString SourceDetail
        {
            get
            {
                if (_objCachedSourceDetail == default)
                    _objCachedSourceDetail = new SourceString(Source,
                        DisplayPage(GlobalOptions.Language), GlobalOptions.Language, GlobalOptions.CultureInfo,
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
            objWriter.WriteStartElement("spell");
            objWriter.WriteElementString("sourceid", SourceIDString);
            objWriter.WriteElementString("guid", InternalId);
            objWriter.WriteElementString("name", _strName);
            objWriter.WriteElementString("descriptors", _strDescriptors);
            objWriter.WriteElementString("category", _strCategory);
            objWriter.WriteElementString("type", _strType);
            objWriter.WriteElementString("range", _strRange);
            objWriter.WriteElementString("damage", _strDamage);
            objWriter.WriteElementString("duration", _strDuration);
            objWriter.WriteElementString("dv", _strDV);
            objWriter.WriteElementString("limited", _blnLimited.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("extended", _blnExtended.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("customextended", _blnCustomExtended.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("alchemical", _blnAlchemical.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("source", _strSource);
            objWriter.WriteElementString("page", _strPage);
            objWriter.WriteElementString("extra", _strExtra);
            objWriter.WriteElementString("notes", System.Text.RegularExpressions.Regex.Replace(_strNotes, @"[\u0000-\u0008\u000B\u000C\u000E-\u001F]", string.Empty));
            objWriter.WriteElementString("notesColor", ColorTranslator.ToHtml(_colNotes));
            objWriter.WriteElementString("freebonus", _blnFreeBonus.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("usesunarmed", _blnUsesUnarmed.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("improvementsource", _objImprovementSource.ToString());
            objWriter.WriteElementString("grade", _intGrade.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteEndElement();

            if (Grade >= 0)
                _objCharacter.SourceProcess(_strSource);
        }

        /// <summary>
        /// Load the Spell from the XmlNode.
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
            objNode.TryGetStringFieldQuickly("name", ref _strName);
            if (!objNode.TryGetGuidFieldQuickly("sourceid", ref _guiSourceID))
            {
                XmlNode node = GetNode(GlobalOptions.Language);
                node?.TryGetGuidFieldQuickly("id", ref _guiSourceID);
            }

            if (objNode.TryGetStringFieldQuickly("descriptors", ref _strDescriptors))
                UpdateHashDescriptors();
            objNode.TryGetStringFieldQuickly("category", ref _strCategory);
            objNode.TryGetStringFieldQuickly("type", ref _strType);
            objNode.TryGetStringFieldQuickly("range", ref _strRange);
            objNode.TryGetStringFieldQuickly("damage", ref _strDamage);
            objNode.TryGetStringFieldQuickly("duration", ref _strDuration);
            if (objNode["improvementsource"] != null)
            {
                _objImprovementSource = Improvement.ConvertToImprovementSource(objNode["improvementsource"].InnerText);
            }
            objNode.TryGetInt32FieldQuickly("grade", ref _intGrade);
            objNode.TryGetStringFieldQuickly("dv", ref _strDV);
            if (objNode.TryGetBoolFieldQuickly("limited", ref _blnLimited) && _blnLimited && _objCharacter.LastSavedVersion <= new Version(5, 197, 30))
            {
                GetNode()?.TryGetStringFieldQuickly("dv", ref _strDV);
            }
            objNode.TryGetBoolFieldQuickly("extended", ref _blnExtended);
            if (_blnExtended)
            {
                if (!objNode.TryGetBoolFieldQuickly("customextended", ref _blnCustomExtended))
                {
                    _blnCustomExtended = !HashDescriptors.Any(x =>
                        string.Equals(x.Trim(), "Extended Area", StringComparison.OrdinalIgnoreCase));
                }
            }
            else
                _blnCustomExtended = false;
            objNode.TryGetBoolFieldQuickly("freebonus", ref _blnFreeBonus);
            objNode.TryGetBoolFieldQuickly("usesunarmed", ref _blnUsesUnarmed);
            objNode.TryGetBoolFieldQuickly("alchemical", ref _blnAlchemical);
            objNode.TryGetStringFieldQuickly("source", ref _strSource);
            objNode.TryGetStringFieldQuickly("page", ref _strPage);

            objNode.TryGetStringFieldQuickly("extra", ref _strExtra);
            objNode.TryGetMultiLineStringFieldQuickly("notes", ref _strNotes);

            string sNotesColor = ColorTranslator.ToHtml(ColorManager.HasNotesColor);
            objNode.TryGetStringFieldQuickly("notesColor", ref sNotesColor);
            _colNotes = ColorTranslator.FromHtml(sNotesColor);
        }

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
            objWriter.WriteStartElement("spell");
            objWriter.WriteElementString("guid", InternalId);
            objWriter.WriteElementString("sourceid", SourceIDString);
            objWriter.WriteElementString("name", DisplayNameShort(strLanguageToPrint));
            objWriter.WriteElementString("fullname", DisplayName(strLanguageToPrint));
            objWriter.WriteElementString("name_english", Name);
            objWriter.WriteElementString("descriptors", DisplayDescriptors(strLanguageToPrint));
            objWriter.WriteElementString("category", DisplayCategory(strLanguageToPrint));
            objWriter.WriteElementString("category_english", Category);
            objWriter.WriteElementString("type", DisplayType(strLanguageToPrint));
            objWriter.WriteElementString("range", DisplayRange(strLanguageToPrint));
            objWriter.WriteElementString("damage", DisplayDamage(strLanguageToPrint));
            objWriter.WriteElementString("duration", DisplayDuration(strLanguageToPrint));
            objWriter.WriteElementString("dv", DisplayDV(strLanguageToPrint));
            objWriter.WriteElementString("alchemy", Alchemical.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("dicepool", DicePool.ToString(objCulture));
            objWriter.WriteElementString("source", _objCharacter.LanguageBookShort(Source, strLanguageToPrint));
            objWriter.WriteElementString("page", DisplayPage(strLanguageToPrint));

            objWriter.WriteElementString("extra", _objCharacter.TranslateExtra(Extra, strLanguageToPrint));
            if (GlobalOptions.PrintNotes)
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
        public string SourceIDString => _guiSourceID.ToString("D", GlobalOptions.InvariantCultureInfo);

        /// <summary>
        /// Internal identifier which will be used to identify this Spell in the Improvement system.
        /// </summary>
        public string InternalId => _guiID.ToString("D", GlobalOptions.InvariantCultureInfo);

        /// <summary>
        /// Spell's name.
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
        /// Spell's grade.
        /// </summary>
        public int Grade
        {
            get => _intGrade;
            set => _intGrade = value;
        }

        /// <summary>
        /// Spell's descriptors.
        /// </summary>
        public string Descriptors
        {
            get => _strDescriptors;
            set
            {
                if (_strDescriptors == value)
                    return;
                _strDescriptors = value;
                UpdateHashDescriptors();
                if (Extended)
                {
                    _blnCustomExtended = !HashDescriptors.Any(x =>
                        string.Equals(x.Trim(), "Extended Area", StringComparison.OrdinalIgnoreCase));
                }
            }
        }

        private void UpdateHashDescriptors()
        {
            HashDescriptors.Clear();
            foreach (string strDescriptor in Descriptors.SplitNoAlloc(',', StringSplitOptions.RemoveEmptyEntries))
                HashDescriptors.Add(strDescriptor);
        }

        public HashSet<string> HashDescriptors { get; } = new HashSet<string>();

        /// <summary>
        /// Translated Descriptors.
        /// </summary>
        public string DisplayDescriptors(string strLanguage = "")
        {
            if (string.IsNullOrWhiteSpace(Descriptors))
                return LanguageManager.GetString("String_None", strLanguage);
            StringBuilder objReturn = new StringBuilder();
            string strSpace = LanguageManager.GetString("String_Space", strLanguage);
            if (HashDescriptors.Count > 0)
            {
                foreach (string strDescriptor in HashDescriptors)
                {
                    switch (strDescriptor.Trim())
                    {
                        case "Alchemical Preparation":
                            objReturn.Append(LanguageManager.GetString("String_DescAlchemicalPreparation", strLanguage));
                            break;

                        case "Extended Area":
                            objReturn.Append(LanguageManager.GetString("String_DescExtendedArea", strLanguage));
                            break;

                        case "Material Link":
                            objReturn.Append(LanguageManager.GetString("String_DescMaterialLink", strLanguage));
                            break;

                        case "Multi-Sense":
                            objReturn.Append(LanguageManager.GetString("String_DescMultiSense", strLanguage));
                            break;

                        case "Organic Link":
                            objReturn.Append(LanguageManager.GetString("String_DescOrganicLink", strLanguage));
                            break;

                        case "Single-Sense":
                            objReturn.Append(LanguageManager.GetString("String_DescSingleSense", strLanguage));
                            break;

                        default:
                            objReturn.Append(LanguageManager.GetString("String_Desc" + strDescriptor.Trim(),
                                strLanguage));
                            break;
                    }

                    objReturn.Append(',' + strSpace);
                }
            }

            // If Extended Area was not found and the Extended flag is enabled, add Extended Area to the list of Descriptors.
            if (Extended && _blnCustomExtended)
                objReturn.Append(LanguageManager.GetString("String_DescExtendedArea", strLanguage) + ',' + strSpace);

            // Remove the trailing comma.
            if (objReturn.Length >= strSpace.Length + 1)
                objReturn.Length -= strSpace.Length + 1;

            return objReturn.ToString();
        }

        /// <summary>
        /// Translated Category.
        /// </summary>
        public string DisplayCategory(string strLanguage)
        {
            if (strLanguage.Equals(GlobalOptions.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                return Category;

            return _objCharacter.LoadDataXPath("spells.xml", strLanguage).SelectSingleNode("/chummer/categories/category[. = " + Category.CleanXPath() + "]/@translate")?.Value ?? Category;
        }

        /// <summary>
        /// Spell's category.
        /// </summary>
        public string Category
        {
            get => _strCategory;
            set => _strCategory = value;
        }

        /// <summary>
        /// Spell's type.
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

                default:
                    return LanguageManager.GetString("String_SpellTypePhysical", strLanguage);
            }
        }

        /// <summary>
        /// Translated Drain Value.
        /// </summary>
        public string DisplayDV(string strLanguage)
        {
            string strReturn = DV.Replace('/', '÷');
            if (!strLanguage.Equals(GlobalOptions.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
            {
                strReturn = strReturn.CheapReplace("F", () => LanguageManager.GetString("String_SpellForce", strLanguage))
                    .CheapReplace("Overflow damage", () => LanguageManager.GetString("String_SpellOverflowDamage", strLanguage))
                    .CheapReplace("Damage Value", () => LanguageManager.GetString("String_SpellDamageValue", strLanguage))
                    .CheapReplace("Toxin DV", () => LanguageManager.GetString("String_SpellToxinDV", strLanguage))
                    .CheapReplace("Disease DV", () => LanguageManager.GetString("String_SpellDiseaseDV", strLanguage))
                    .CheapReplace("Radiation Power", () => LanguageManager.GetString("String_SpellRadiationPower", strLanguage))
                    .CheapReplace("Special", () => LanguageManager.GetString("String_Special", strLanguage));
            }

            return strReturn;
        }

        /// <summary>
        /// Drain Tooltip.
        /// </summary>
        public string DVTooltip
        {
            get
            {
                StringBuilder sbdTip = new StringBuilder(LanguageManager.GetString("Tip_SpellDrainBase"));
                string strSpace = LanguageManager.GetString("String_Space");
                int intMAG = _objCharacter.MAG.TotalValue;
                string strDV = DV;
                for (int i = 1; i <= intMAG * 2; i++)
                {
                    // Calculate the Spell's Drain for the current Force.
                    object xprResult = CommonFunctions.EvaluateInvariantXPath(strDV.Replace("F", i.ToString(GlobalOptions.InvariantCultureInfo)).Replace("/", " div "), out bool blnIsSuccess);

                    if (blnIsSuccess && strDV != "Special")
                    {
                        int intDV = ((double)xprResult).StandardRound();

                        // Drain cannot be lower than 2.
                        if (intDV < 2)
                            intDV = 2;
                        sbdTip.Append(Environment.NewLine + LanguageManager.GetString("String_Force") + strSpace + i.ToString(GlobalOptions.CultureInfo) + LanguageManager.GetString("String_Colon") + strSpace + intDV.ToString(GlobalOptions.CultureInfo));

                        string strLabelFormat = strSpace + "({0}" + LanguageManager.GetString("String_Colon") + strSpace + "{1})";
                        if (Limited)
                        {
                            sbdTip.AppendFormat(GlobalOptions.CultureInfo, strLabelFormat, LanguageManager.GetString("String_SpellLimited"), -2);
                        }
                        if (Extended && _blnCustomExtended)
                        {
                            sbdTip.AppendFormat(GlobalOptions.CultureInfo, strLabelFormat, LanguageManager.GetString("String_SpellExtended"), '+' + 2.ToString(GlobalOptions.CultureInfo));
                        }
                    }
                    else
                    {
                        sbdTip.Clear();
                        sbdTip.Append(LanguageManager.GetString("Tip_SpellDrainSeeDescription"));
                        break;
                    }
                }

                List<Improvement> lstDrainImprovements = RelevantImprovements(o => o.ImproveType == Improvement.ImprovementType.DrainValue || o.ImproveType == Improvement.ImprovementType.SpellCategoryDrain || o.ImproveType == Improvement.ImprovementType.SpellDescriptorDrain).ToList();
                if (lstDrainImprovements.Count <= 0)
                    return sbdTip.ToString();
                sbdTip.Append(Environment.NewLine + LanguageManager.GetString("Label_Bonus"));
                foreach (Improvement objLoopImprovement in lstDrainImprovements)
                {
                    sbdTip.Append(Environment.NewLine + _objCharacter.GetObjectName(objLoopImprovement) + strSpace + '(' + objLoopImprovement.Value.ToString("0;-0;0") + ')');
                }

                return sbdTip.ToString();
            }
        }

        /// <summary>
        /// Spell's range.
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
            string strReturn = Range;
            if (!strLanguage.Equals(GlobalOptions.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
            {
                strReturn = strReturn.CheapReplace("Self", () => LanguageManager.GetString("String_SpellRangeSelf", strLanguage))
                    .CheapReplace("LOS", () => LanguageManager.GetString("String_SpellRangeLineOfSight", strLanguage))
                    .CheapReplace("LOI", () => LanguageManager.GetString("String_SpellRangeLineOfInfluence", strLanguage))
                    .CheapReplace("Touch", () => LanguageManager.GetString("String_SpellRangeTouch", strLanguage)) // Short form to remain export-friendly
                    .CheapReplace("T", () => LanguageManager.GetString("String_SpellRangeTouch", strLanguage))
                    .CheapReplace("(A)", () => "(" + LanguageManager.GetString("String_SpellRangeArea", strLanguage) + ')')
                    .CheapReplace("MAG", () => LanguageManager.GetString("String_AttributeMAGShort", strLanguage))
                    .CheapReplace("Special", () => LanguageManager.GetString("String_Special", strLanguage));
            }

            return strReturn;
        }

        /// <summary>
        /// Spell's damage.
        /// </summary>
        public string Damage
        {
            get => _strDamage;
            set => _strDamage = value;
        }

        /// <summary>
        /// Translated Damage.
        /// </summary>
        public string DisplayDamage(string strLanguage)
        {
            if (Damage != "S" && Damage != "P")
                return LanguageManager.GetString("String_None", strLanguage);
            StringBuilder sBld = new StringBuilder("0");

            foreach (var improvement in RelevantImprovements(i =>
                i.ImproveType == Improvement.ImprovementType.SpellDescriptorDamage ||
                i.ImproveType == Improvement.ImprovementType.SpellCategoryDamage))
                sBld.AppendFormat(GlobalOptions.InvariantCultureInfo, " + {0:0;-0;0}", improvement.Value);
            string output = sBld.ToString();

            object xprResult = CommonFunctions.EvaluateInvariantXPath(output.TrimStart('+'), out bool blnIsSuccess);
            sBld = blnIsSuccess ? new StringBuilder(xprResult.ToString()) : new StringBuilder();

            switch (Damage)
            {
                case "P":
                    sBld.Append(LanguageManager.GetString("String_DamagePhysical", strLanguage));
                    break;

                case "S":
                    sBld.Append(LanguageManager.GetString("String_DamageStun", strLanguage));
                    break;
            }

            return sBld.ToString();
        }

        /// <summary>
        /// Spell's duration.
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
            switch (Duration)
            {
                case "P":
                    return LanguageManager.GetString("String_SpellDurationPermanent", strLanguage);

                case "S":
                    return LanguageManager.GetString("String_SpellDurationSustained", strLanguage);

                case "I":
                    return LanguageManager.GetString("String_SpellDurationInstant", strLanguage);

                case "Special":
                    return LanguageManager.GetString("String_SpellDurationSpecial", strLanguage);

                default:
                    return LanguageManager.GetString("String_None", strLanguage);
            }
        }

        /// <summary>
        /// Spell's drain value.
        /// </summary>
        public string DV
        {
            get
            {
                string strReturn = _strDV;
                if (!Limited && !_blnCustomExtended && !RelevantImprovements(o =>
                        o.ImproveType == Improvement.ImprovementType.DrainValue
                        || o.ImproveType == Improvement.ImprovementType.SpellCategoryDrain
                        || o.ImproveType == Improvement.ImprovementType.SpellDescriptorDrain, true).Any())
                    return strReturn;
                bool force = strReturn.StartsWith('F');
                string strDV = strReturn.TrimStartOnce('F');
                //Navigator can't do math on a single value, so inject a mathable value.
                if (string.IsNullOrEmpty(strDV))
                {
                    strDV = "0";
                }
                else
                {
                    int intPos = strReturn.IndexOf('-');
                    if (intPos != -1)
                    {
                        strDV = strReturn.Substring(intPos);
                    }
                    else
                    {
                        intPos = strReturn.IndexOf('+');
                        if (intPos != -1)
                        {
                            strDV = strReturn.Substring(intPos);
                        }
                    }
                }

                StringBuilder sbdDV = new StringBuilder(strDV);
                foreach (Improvement objImprovement in RelevantImprovements(i =>
                    i.ImproveType == Improvement.ImprovementType.DrainValue
                    || i.ImproveType == Improvement.ImprovementType.SpellCategoryDrain
                    || i.ImproveType == Improvement.ImprovementType.SpellDescriptorDrain))
                {
                    sbdDV.AppendFormat(GlobalOptions.InvariantCultureInfo, "{0:+0;-0;+0}", objImprovement.Value);
                }

                if (Limited)
                {
                    sbdDV.Append("-2");
                }
                if (Extended && _blnCustomExtended)
                {
                    sbdDV.Append("+2");
                }
                object xprResult = CommonFunctions.EvaluateInvariantXPath(sbdDV.ToString(), out bool blnIsSuccess);
                if (!blnIsSuccess)
                    return strReturn;
                if (force)
                {
                    strReturn = string.Format(GlobalOptions.InvariantCultureInfo, "F{0:+0;-0;}", xprResult);
                }
                else if (xprResult.ToString() != "0")
                {
                    strReturn += xprResult;
                }
                return strReturn;
            }
            set => _strDV = value;
        }

        /// <summary>
        /// Spell's Source.
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
            if (strLanguage.Equals(GlobalOptions.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                return Page;
            string s = GetNode(strLanguage)?["altpage"]?.InnerText ?? Page;
            return !string.IsNullOrWhiteSpace(s) ? s : Page;
        }

        /// <summary>
        /// Extra information from Improvement dialogues.
        /// </summary>
        public string Extra
        {
            get => _strExtra;
            set => _strExtra = _objCharacter.ReverseTranslateExtra(value);
        }

        /// <summary>
        /// Whether or not the Spell is Limited.
        /// </summary>
        public bool Limited
        {
            get => _blnLimited;
            set => _blnLimited = value;
        }

        /// <summary>
        /// Whether or not the Spell is Extended.
        /// </summary>
        public bool Extended
        {
            get => _blnExtended;
            set
            {
                _blnExtended = value;
                if (value)
                {
                    _blnCustomExtended = !HashDescriptors.Any(x =>
                        string.Equals(x.Trim(), "Extended Area", StringComparison.OrdinalIgnoreCase));
                }
                else
                    _blnCustomExtended = false;
            }
        }

        /// <summary>
        /// Whether or not the Spell is Alchemical.
        /// </summary>
        public bool Alchemical
        {
            get => _blnAlchemical;
            set => _blnAlchemical = value;
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
        /// The name of the object as it should be displayed on printouts (translated name only).
        /// </summary>
        public string DisplayNameShort(string strLanguage)
        {
            string strReturn = !strLanguage.Equals(GlobalOptions.DefaultLanguage, StringComparison.OrdinalIgnoreCase) ? GetNode(strLanguage)?["translate"]?.InnerText ?? Name : Name;
            if (Extended && _blnCustomExtended)
                strReturn += ',' + LanguageManager.GetString("String_Space", strLanguage) + LanguageManager.GetString("String_SpellExtended", strLanguage);

            return strReturn;
        }

        /// <summary>
        /// The name of the object as it should be displayed in lists.
        /// </summary>
        public string DisplayName(string strLanguage)
        {
            string strReturn = DisplayNameShort(strLanguage);

            if (Limited)
                strReturn += LanguageManager.GetString("String_Space", strLanguage) + '(' + LanguageManager.GetString("String_SpellLimited", strLanguage) + ')';
            if (Alchemical)
                strReturn += LanguageManager.GetString("String_Space", strLanguage) + '(' + LanguageManager.GetString("String_SpellAlchemical", strLanguage) + ')';
            if (!string.IsNullOrEmpty(Extra))
            {
                // Attempt to retrieve the CharacterAttribute name.
                strReturn += LanguageManager.GetString("String_Space", strLanguage) + '(' + _objCharacter.TranslateExtra(Extra, strLanguage) + ')';
            }
            return strReturn;
        }

        public string CurrentDisplayName => DisplayName(GlobalOptions.Language);

        /// <summary>
        /// Does the spell cost Karma? Typically provided by improvements.
        /// </summary>
        public bool FreeBonus
        {
            get => _blnFreeBonus;
            set => _blnFreeBonus = value;
        }

        /// <summary>
        /// Does the spell use Unarmed in place of Spellcasting for its casting test?
        /// </summary>
        public bool UsesUnarmed
        {
            get => _blnUsesUnarmed;
            set => _blnUsesUnarmed = value;
        }

        #endregion Properties

        #region ComplexProperties

        /// <summary>
        /// Skill used by this spell
        /// </summary>
        public Skill Skill
        {
            get
            {
                if (Alchemical)
                {
                    return _objCharacter.SkillsSection.GetActiveSkill("Alchemy");
                }
                if (Category == "Enchantments")
                {
                    return _objCharacter.SkillsSection.GetActiveSkill("Artificing");
                }

                return Category == "Rituals"
                    ? _objCharacter.SkillsSection.GetActiveSkill("Ritual Spellcasting")
                    : _objCharacter.SkillsSection.GetActiveSkill(UsesUnarmed ? "Unarmed Combat" : "Spellcasting");
            }
        }

        /// <summary>
        /// The Dice Pool size for the Active Skill required to cast the Spell.
        /// </summary>
        public int DicePool
        {
            get
            {
                int intReturn = 0;
                Skill objSkill = Skill;
                if (objSkill != null)
                {
                    intReturn = UsesUnarmed ? objSkill.PoolOtherAttribute("MAG") : objSkill.Pool;
                    // Add any Specialization bonus if applicable.
                    intReturn += Skill.GetSpecializationBonus(Category);
                }

                // Include any Improvements to the Spell's dicepool.
                intReturn += RelevantImprovements(x =>
                    x.ImproveType == Improvement.ImprovementType.SpellCategory
                    || x.ImproveType == Improvement.ImprovementType.SpellDicePool).Sum(x => x.Value).StandardRound();

                return intReturn;
            }
        }

        /// <summary>
        /// Tooltip information for the Dice Pool.
        /// </summary>
        public string DicePoolTooltip
        {
            get
            {
                string strSpace = LanguageManager.GetString("String_Space");
                StringBuilder sbdReturn = new StringBuilder();
                string strFormat = strSpace + "{0}" + strSpace + "({1})";
                Skill objSkill = Skill;
                CharacterAttrib objAttrib = _objCharacter.GetAttribute(UsesUnarmed ? "MAG" : (objSkill?.Attribute ?? "MAG"));
                if (objAttrib != null)
                {
                    sbdReturn.AppendFormat(GlobalOptions.CultureInfo, strFormat,
                        objAttrib.DisplayNameFormatted, objAttrib.DisplayValue);
                }
                if (objSkill != null)
                {
                    int intPool = UsesUnarmed ? objSkill.PoolOtherAttribute("MAG") : objSkill.Pool;
                    if (objAttrib != null)
                        intPool -= objAttrib.TotalValue;
                    if (sbdReturn.Length > 0)
                        sbdReturn.Append(strSpace + '+' + strSpace);
                    sbdReturn.Append(objSkill.FormattedDicePool(intPool, Category));
                }

                // Include any Improvements to the Spell Category or Spell Name.
                foreach (Improvement objImprovement in RelevantImprovements(x => x.ImproveType == Improvement.ImprovementType.SpellCategory || x.ImproveType == Improvement.ImprovementType.SpellDicePool))
                {
                    if (sbdReturn.Length > 0)
                        sbdReturn.Append(strSpace + '+' + strSpace);
                    sbdReturn.AppendFormat(GlobalOptions.CultureInfo, strFormat,
                        _objCharacter.GetObjectName(objImprovement), objImprovement.Value);
                }

                return sbdReturn.ToString();
            }
        }

        private XmlNode _objCachedMyXmlNode;
        private string _strCachedXmlNodeLanguage = string.Empty;

        public XmlNode GetNode()
        {
            return GetNode(GlobalOptions.Language);
        }

        public XmlNode GetNode(string strLanguage)
        {
            if (_objCachedMyXmlNode == null || strLanguage != _strCachedXmlNodeLanguage || GlobalOptions.LiveCustomData)
            {
                _objCachedMyXmlNode = _objCharacter.LoadData("spells.xml", strLanguage)
                    .SelectSingleNode(SourceID == Guid.Empty
                        ? "/chummer/spells/spell[name = " + Name.CleanXPath() + ']'
                        : string.Format(GlobalOptions.InvariantCultureInfo,
                            "/chummer/spells/spell[id = {0} or id = {1}]",
                            SourceIDString.CleanXPath(), SourceIDString.ToUpperInvariant().CleanXPath()));
                _strCachedXmlNodeLanguage = strLanguage;
            }
            return _objCachedMyXmlNode;
        }

        private IEnumerable<Improvement> RelevantImprovements(Func<Improvement, bool> funcWherePredicate = null, bool blnExitAfterFirst = false)
        {
            foreach (Improvement objImprovement in _objCharacter.Improvements.Where(i => i.Enabled && funcWherePredicate?.Invoke(i) == true))
            {
                switch (objImprovement.ImproveType)
                {
                    case Improvement.ImprovementType.SpellDicePool:
                        if (objImprovement.ImprovedName == Name || objImprovement.ImprovedName == SourceID.ToString())
                        {
                            yield return objImprovement;
                            if (blnExitAfterFirst)
                                yield break;
                        }
                        break;

                    case Improvement.ImprovementType.SpellCategory:
                        if (objImprovement.ImprovedName == Category)
                        {
                            // SR5 318: Regardless of the number of bonded foci you have,
                            // only one focus may add its Force to a dicepool for any given test.
                            // We need to do some checking to make sure this is the most powerful focus before we add it in
                            if (objImprovement.ImproveSource == Improvement.ImprovementSource.Gear)
                            {
                                //TODO: THIS IS NOT SAFE. While we can mostly assume that Gear that add to SpellCategory are Foci, it's not reliable.
                                // we are returning either the original improvement, null or a newly instantiated improvement
                                Improvement bestFocus = CompareFocusPower(objImprovement);
                                if (bestFocus != null)
                                {
                                    yield return bestFocus;
                                    if (blnExitAfterFirst)
                                        yield break;
                                }

                                break;
                            }

                            yield return objImprovement;
                            if (blnExitAfterFirst)
                                yield break;
                        }
                        break;

                    case Improvement.ImprovementType.SpellCategoryDamage:
                    case Improvement.ImprovementType.SpellCategoryDrain:
                        if (objImprovement.ImprovedName == Category)
                        {
                            yield return objImprovement;
                            if (blnExitAfterFirst)
                                yield break;
                        }
                        break;

                    case Improvement.ImprovementType.SpellDescriptorDrain:
                    case Improvement.ImprovementType.SpellDescriptorDamage:
                        if (HashDescriptors.Count > 0)
                        {
                            bool blnAllow = false;
                            foreach (string strDescriptor in objImprovement.ImprovedName.SplitNoAlloc(',', StringSplitOptions.RemoveEmptyEntries))
                            {
                                if (strDescriptor.StartsWith("NOT", StringComparison.Ordinal))
                                {
                                    if (HashDescriptors.Contains(strDescriptor.TrimStartOnce("NOT(").TrimEndOnce(')')))
                                    {
                                        blnAllow = false;
                                        break;
                                    }
                                }
                                else
                                {
                                    blnAllow = HashDescriptors.Contains(strDescriptor);
                                }
                            }

                            if (blnAllow)
                            {
                                yield return objImprovement;
                                if (blnExitAfterFirst)
                                    yield break;
                            }
                        }
                        break;

                    case Improvement.ImprovementType.DrainValue:
                        {
                            yield return objImprovement;
                            if (blnExitAfterFirst)
                                yield break;
                        }
                        break;
                }
            }
        }

        /// <summary>
        /// Method to check we are only applying the highest focus to the spell dicepool
        /// </summary>
        private Improvement CompareFocusPower(Improvement objImprovement)
        {
            var list = _objCharacter.Foci
                .Where(x => x.GearObject.Bonus.InnerText == "MAGRating" && x.GearObject.Bonded).ToList();
            if (list.Count > 0)
            {
                // get any bonded foci that add to the base magic stat and return the highest rated one's rating
                var powerFocusRating = list.Select(x => x.Rating).Max();

                // If our focus is higher, add in a partial bonus
                if (powerFocusRating > 0 && powerFocusRating < objImprovement.Value)
                {
                    // This is hackz -- because we don't want to lose the original improvement's value
                    // we instantiate a fake version of the improvement that isn't saved to represent the diff
                    return new Improvement(_objCharacter)
                    {
                        Value = objImprovement.Value - powerFocusRating,
                        SourceName = objImprovement.SourceName,
                        ImprovedName = objImprovement.ImprovedName,
                        ImproveSource = objImprovement.ImproveSource,
                        ImproveType = objImprovement.ImproveType,
                    };
                }
                return powerFocusRating > 0 ? null : objImprovement;
            }

            return objImprovement;
        }

        #endregion ComplexProperties

        #region UI Methods

        public TreeNode CreateTreeNode(ContextMenuStrip cmsSpell, bool blnAddCategory = false)
        {
            if (Grade != 0 && !string.IsNullOrEmpty(Source) && !_objCharacter.Options.BookEnabled(Source))
                return null;

            string strText = CurrentDisplayName;
            if (blnAddCategory)
            {
                switch (Category)
                {
                    case "Rituals":
                        strText = LanguageManager.GetString("Label_Ritual") + LanguageManager.GetString("String_Space") + strText;
                        break;

                    case "Enchantments":
                        strText = LanguageManager.GetString("Label_Enchantment") + LanguageManager.GetString("String_Space") + strText;
                        break;
                }
            }
            TreeNode objNode = new TreeNode
            {
                Name = InternalId,
                Text = strText,
                Tag = this,
                ContextMenuStrip = cmsSpell,
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
            if (blnConfirmDelete)
            {
                string strMessage = LanguageManager.GetString("Message_DeleteSpell");
                if (!CommonFunctions.ConfirmDelete(strMessage))
                    return false;
            }

            _objCharacter.Spells.Remove(this);
            ImprovementManager.RemoveImprovements(_objCharacter, Improvement.ImprovementSource.Spell, InternalId);
            return true;
        }

        public void SetSourceDetail(Control sourceControl)
        {
            if (_objCachedSourceDetail.Language != GlobalOptions.Language)
                _objCachedSourceDetail = default;
            SourceDetail.SetControl(sourceControl);
        }
    }
}
