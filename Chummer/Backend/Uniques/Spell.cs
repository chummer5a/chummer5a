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
using Chummer.Backend.Skills;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace Chummer
{
    /// <summary>
    /// A Magician Spell.
    /// </summary>
    [HubClassTag("SourceID", true, "Name", "Extra")]
    [DebuggerDisplay("{DisplayName(GlobalOptions.DefaultLanguage)}")]
    public class Spell : IHasInternalId, IHasName, IHasXmlNode, IHasNotes, ICanRemove, IHasSource
    {
        private Guid _guiID;
        private Guid _sourceID = Guid.Empty;
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
        private string _strNotes = string.Empty;
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
            if (objXmlSpellNode.TryGetStringFieldQuickly("name", ref _strName))
                _objCachedMyXmlNode = null;
            objXmlSpellNode.TryGetField("id", Guid.TryParse, out _sourceID);
            _blnExtended = blnExtended;

            ImprovementManager.ForcedValue = strForcedValue;
            if (objXmlSpellNode["bonus"] != null)
            {
                if (!ImprovementManager.CreateImprovements(_objCharacter, Improvement.ImprovementSource.Spell, _guiID.ToString("D"), objXmlSpellNode["bonus"], false, 1, DisplayNameShort(GlobalOptions.Language)))
                {
                    _guiID = Guid.Empty;
                    return;
                }
                if (!string.IsNullOrEmpty(ImprovementManager.SelectedValue))
                {
                    _strExtra = ImprovementManager.SelectedValue;
                }
            }
            if (!objXmlSpellNode.TryGetStringFieldQuickly("altnotes", ref _strNotes))
                objXmlSpellNode.TryGetStringFieldQuickly("notes", ref _strNotes);
            objXmlSpellNode.TryGetStringFieldQuickly("descriptor", ref _strDescriptors);
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

            /*
            if (string.IsNullOrEmpty(_strNotes))
            {
                _strNotes = CommonFunctions.GetText($"{_strSource} {_strPage}", Name);
            }*/
        }

        private SourceString _objCachedSourceDetail;
        public SourceString SourceDetail => _objCachedSourceDetail ?? (_objCachedSourceDetail =
                                                new SourceString(Source, DisplayPage(GlobalOptions.Language), GlobalOptions.Language));

        /// <summary>
        /// Save the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        public void Save(XmlTextWriter objWriter)
        {
            objWriter.WriteStartElement("spell");
            objWriter.WriteElementString("sourceid", _sourceID.ToString("D"));
            objWriter.WriteElementString("guid", _guiID.ToString("D"));
            objWriter.WriteElementString("name", _strName);
            objWriter.WriteElementString("descriptors", _strDescriptors);
            objWriter.WriteElementString("category", _strCategory);
            objWriter.WriteElementString("type", _strType);
            objWriter.WriteElementString("range", _strRange);
            objWriter.WriteElementString("damage", _strDamage);
            objWriter.WriteElementString("duration", _strDuration);
            objWriter.WriteElementString("dv", _strDV);
            objWriter.WriteElementString("limited", _blnLimited.ToString());
            objWriter.WriteElementString("extended", _blnExtended.ToString());
            objWriter.WriteElementString("alchemical", _blnAlchemical.ToString());
            objWriter.WriteElementString("source", _strSource);
            objWriter.WriteElementString("page", _strPage);
            objWriter.WriteElementString("extra", _strExtra);
            objWriter.WriteElementString("notes", _strNotes);
            objWriter.WriteElementString("freebonus", _blnFreeBonus.ToString());
            objWriter.WriteElementString("usesunarmed", _blnUsesUnarmed.ToString());
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
            objNode.TryGetField("guid", Guid.TryParse, out _guiID);
            if (objNode.TryGetStringFieldQuickly("name", ref _strName))
                _objCachedMyXmlNode = null;

            if (objNode["sourceid"] == null || !objNode.TryGetField("sourceid", Guid.TryParse, out _sourceID))
            {
                XmlNode objSpellNode = GetNode();
                objSpellNode?.TryGetField("id", Guid.TryParse, out _sourceID);
            }
            objNode.TryGetStringFieldQuickly("descriptors", ref _strDescriptors);
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
            if (objNode.TryGetBoolFieldQuickly("limited", ref _blnLimited) && _blnLimited && _objCharacter.LastSavedVersion <= new Version("5.197.30"))
            {
                GetNode()?.TryGetStringFieldQuickly("dv", ref _strDV);
            }
            objNode.TryGetBoolFieldQuickly("extended", ref _blnExtended);
            objNode.TryGetBoolFieldQuickly("freebonus", ref _blnFreeBonus);
            objNode.TryGetBoolFieldQuickly("usesunarmed", ref _blnUsesUnarmed);
            objNode.TryGetBoolFieldQuickly("alchemical", ref _blnAlchemical);
            objNode.TryGetStringFieldQuickly("source", ref _strSource);
            objNode.TryGetStringFieldQuickly("page", ref _strPage);

            objNode.TryGetStringFieldQuickly("extra", ref _strExtra);
            objNode.TryGetStringFieldQuickly("notes", ref _strNotes);
        }

        /// <summary>
        /// Print the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        /// <param name="objCulture">Culture in which to print numbers.</param>
        /// <param name="strLanguageToPrint">Language in which to print.</param>
        public void Print(XmlTextWriter objWriter, CultureInfo objCulture, string strLanguageToPrint)
        {
            objWriter.WriteStartElement("spell");
            if (Limited)
                objWriter.WriteElementString("name", DisplayNameShort(strLanguageToPrint) + LanguageManager.GetString("String_Space", strLanguageToPrint) + '(' + LanguageManager.GetString("String_SpellLimited", strLanguageToPrint) + ')');
            else if (Alchemical)
                objWriter.WriteElementString("name", DisplayNameShort(strLanguageToPrint) + LanguageManager.GetString("String_Space", strLanguageToPrint) + '(' + LanguageManager.GetString("String_SpellAlchemical", strLanguageToPrint) + ')');
            else
                objWriter.WriteElementString("name", DisplayNameShort(strLanguageToPrint));
            objWriter.WriteElementString("name_english", Name);
            objWriter.WriteElementString("descriptors", DisplayDescriptors(strLanguageToPrint));
            objWriter.WriteElementString("category", DisplayCategory(strLanguageToPrint));
            objWriter.WriteElementString("category_english", Category);
            objWriter.WriteElementString("type", DisplayType(strLanguageToPrint));
            objWriter.WriteElementString("range", DisplayRange(strLanguageToPrint));
            objWriter.WriteElementString("damage", DisplayDamage(strLanguageToPrint));
            objWriter.WriteElementString("duration", DisplayDuration(strLanguageToPrint));
            objWriter.WriteElementString("dv", DisplayDV(strLanguageToPrint));
            objWriter.WriteElementString("alchemy", Alchemical.ToString());
            objWriter.WriteElementString("dicepool", DicePool.ToString(objCulture));
            objWriter.WriteElementString("source", CommonFunctions.LanguageBookShort(Source, strLanguageToPrint));
            objWriter.WriteElementString("page", DisplayPage(strLanguageToPrint));
            objWriter.WriteElementString("extra", LanguageManager.TranslateExtra(Extra, strLanguageToPrint));
            if (_objCharacter.Options.PrintNotes)
                objWriter.WriteElementString("notes", Notes);
            objWriter.WriteEndElement();
        }
        #endregion

        #region Properties

        public Guid SourceID => _sourceID;

        /// <summary>
        /// Internal identifier which will be used to identify this Spell in the Improvement system.
        /// </summary>
        public string InternalId => _guiID.ToString("D");

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
            set => _strDescriptors = value;
        }

        /// <summary>
        /// Translated Descriptors.
        /// </summary>
        public string DisplayDescriptors(string strLanguage)
        {
            StringBuilder objReturn = new StringBuilder();

            string[] strDescriptorsIn = Descriptors.Split(',');
            bool blnExtendedFound = false;
            foreach (string strDescriptor in strDescriptorsIn)
            {
                switch (strDescriptor.Trim())
                {
                    case "Active":
                        objReturn.Append(LanguageManager.GetString("String_DescActive", strLanguage));
                        break;
                    case "Adept":
                        objReturn.Append(LanguageManager.GetString("String_DescAdept", strLanguage));
                        break;
                    case "Alchemical Preparation":
                        objReturn.Append(LanguageManager.GetString("String_DescAlchemicalPreparation", strLanguage));
                        break;
                    case "Anchored":
                        objReturn.Append(LanguageManager.GetString("String_DescAnchored", strLanguage));
                        break;
                    case "Area":
                        objReturn.Append(LanguageManager.GetString("String_DescArea", strLanguage));
                        break;
                    case "Blood":
                        objReturn.Append(LanguageManager.GetString("String_DescBlood", strLanguage));
                        break;
                    case "Contractual":
                        objReturn.Append(LanguageManager.GetString("String_DescContractual", strLanguage));
                        break;
                    case "Direct":
                        objReturn.Append(LanguageManager.GetString("String_DescDirect", strLanguage));
                        break;
                    case "Directional":
                        objReturn.Append(LanguageManager.GetString("String_DescDirectional", strLanguage));
                        break;
                    case "Elemental":
                        objReturn.Append(LanguageManager.GetString("String_DescElemental", strLanguage));
                        break;
                    case "Environmental":
                        objReturn.Append(LanguageManager.GetString("String_DescEnvironmental", strLanguage));
                        break;
                    case "Extended Area":
                        blnExtendedFound = true;
                        objReturn.Append(LanguageManager.GetString("String_DescExtendedArea", GlobalOptions.Language));
                        break;
                    case "Geomancy":
                        objReturn.Append(LanguageManager.GetString("String_DescGeomancy", strLanguage));
                        break;
                    case "Indirect":
                        objReturn.Append(LanguageManager.GetString("String_DescIndirect", strLanguage));
                        break;
                    case "Mana":
                        objReturn.Append(LanguageManager.GetString("String_DescMana", strLanguage));
                        break;
                    case "Material Link":
                        objReturn.Append(LanguageManager.GetString("String_DescMaterialLink", strLanguage));
                        break;
                    case "Mental":
                        objReturn.Append(LanguageManager.GetString("String_DescMental", strLanguage));
                        break;
                    case "Minion":
                        objReturn.Append(LanguageManager.GetString("String_DescMinion", strLanguage));
                        break;
                    case "Multi-Sense":
                        objReturn.Append(LanguageManager.GetString("String_DescMultiSense", strLanguage));
                        break;
                    case "Negative":
                        objReturn.Append(LanguageManager.GetString("String_DescNegative", strLanguage));
                        break;
                    case "Obvious":
                        objReturn.Append(LanguageManager.GetString("String_DescObvious", strLanguage));
                        break;
                    case "Organic Link":
                        objReturn.Append(LanguageManager.GetString("String_DescOrganicLink", strLanguage));
                        break;
                    case "Passive":
                        objReturn.Append(LanguageManager.GetString("String_DescPassive", strLanguage));
                        break;
                    case "Physical":
                        objReturn.Append(LanguageManager.GetString("String_DescPhysical", strLanguage));
                        break;
                    case "Psychic":
                        objReturn.Append(LanguageManager.GetString("String_DescPsychic", strLanguage));
                        break;
                    case "Realistic":
                        objReturn.Append(LanguageManager.GetString("String_DescRealistic", strLanguage));
                        break;
                    case "Single-Sense":
                        objReturn.Append(LanguageManager.GetString("String_DescSingleSense", strLanguage));
                        break;
                    case "Touch":
                        objReturn.Append(LanguageManager.GetString("String_DescTouch", strLanguage));
                        break;
                    case "Spell":
                        objReturn.Append(LanguageManager.GetString("String_DescSpell", strLanguage));
                        break;
                    case "Spotter":
                        objReturn.Append(LanguageManager.GetString("String_DescSpotter", strLanguage));
                        break;
                }
                objReturn.Append(", ");
            }

            // If Extended Area was not found and the Extended flag is enabled, add Extended Area to the list of Descriptors.
            if (Extended && !blnExtendedFound)
                objReturn.Append(LanguageManager.GetString("String_DescExtendedArea", strLanguage) + ", ");

            // Remove the trailing comma.
            if (objReturn.Length >= 2)
                objReturn.Length -= 2;

            return objReturn.ToString();
        }

        /// <summary>
        /// Translated Category.
        /// </summary>
        public string DisplayCategory(string strLanguage)
        {
            if (strLanguage == GlobalOptions.DefaultLanguage)
                return Category;

            return XmlManager.Load("spells.xml", strLanguage).SelectSingleNode("/chummer/categories/category[. = \"" + Category + "\"]/@translate")?.InnerText ?? Category;
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
            if (strLanguage != GlobalOptions.DefaultLanguage)
            {
                strReturn = strReturn.CheapReplace("F", () => LanguageManager.GetString("String_SpellForce", strLanguage))
                    .CheapReplace("Overflow damage", () => LanguageManager.GetString("String_SpellOverflowDamage", strLanguage))
                    .CheapReplace("Damage Value", () => LanguageManager.GetString("String_SpellDamageValue", strLanguage))
                    .CheapReplace("Toxin DV", () => LanguageManager.GetString("String_SpellToxinDV", strLanguage))
                    .CheapReplace("Disease DV", () => LanguageManager.GetString("String_SpellDiseaseDV", strLanguage))
                    .CheapReplace("Radiation Power", () => LanguageManager.GetString("String_SpellRadiationPower", strLanguage));
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
                StringBuilder strTip = new StringBuilder(LanguageManager.GetString("Tip_SpellDrainBase", GlobalOptions.Language));
                string strSpaceCharacter = LanguageManager.GetString("String_Space", GlobalOptions.Language);
                int intMAG = _objCharacter.MAG.TotalValue;
                string strDV = DV;
                for (int i = 1; i <= intMAG * 2; i++)
                {
                    // Calculate the Spell's Drain for the current Force.
                    object xprResult = CommonFunctions.EvaluateInvariantXPath(strDV.Replace("F", i.ToString()).Replace("/", " div "), out bool blnIsSuccess);

                    if (blnIsSuccess && strDV != "Special")
                    {
                        int intDV = Convert.ToInt32(Math.Floor(Convert.ToDouble(xprResult.ToString(), GlobalOptions.InvariantCultureInfo)));

                        // Drain cannot be lower than 2.
                        if (intDV < 2)
                            intDV = 2;
                        strTip.Append(Environment.NewLine + LanguageManager.GetString("String_Force", GlobalOptions.Language) + strSpaceCharacter + i.ToString(GlobalOptions.CultureInfo)
                                      + LanguageManager.GetString("String_Colon", GlobalOptions.Language) + strSpaceCharacter + intDV);

                        if (Limited)
                        {
                            strTip.Append($"{strSpaceCharacter}({LanguageManager.GetString("String_SpellLimited", GlobalOptions.Language)}{strSpaceCharacter}:{strSpaceCharacter}-2");
                        }
                        if (Extended && !Name.EndsWith("Extended"))
                        {
                            strTip.Append($"{strSpaceCharacter}({LanguageManager.GetString("String_SpellExtended", GlobalOptions.Language)}{strSpaceCharacter}:{strSpaceCharacter}+2");
                        }
                    }
                    else
                    {
                        strTip.Clear();
                        strTip.Append(LanguageManager.GetString("Tip_SpellDrainSeeDescription", GlobalOptions.Language));
                        break;
                    }
                }

                List<Improvement> lstDrainImprovements = RelevantImprovements(o => o.ImproveType == Improvement.ImprovementType.DrainValue || o.ImproveType == Improvement.ImprovementType.SpellCategoryDrain).ToList();
                if (lstDrainImprovements.Count <= 0) return strTip.ToString();
                strTip.Append(Environment.NewLine + LanguageManager.GetString("Label_Bonus", GlobalOptions.Language));
                foreach (Improvement objLoopImprovement in lstDrainImprovements)
                {
                    strTip.Append($"{Environment.NewLine}{_objCharacter.GetObjectName(objLoopImprovement, GlobalOptions.Language)}{strSpaceCharacter}({objLoopImprovement.Value:0;-0;0})");
                }

                return strTip.ToString();
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
            if (strLanguage != GlobalOptions.DefaultLanguage)
            {
                strReturn = strReturn.CheapReplace("Self", () => LanguageManager.GetString("String_SpellRangeSelf", strLanguage))
                    .CheapReplace("LOS", () => LanguageManager.GetString("String_SpellRangeLineOfSight", strLanguage))
                    .CheapReplace("LOI", () => LanguageManager.GetString("String_SpellRangeLineOfInfluence", strLanguage))
                    .CheapReplace("T", () => LanguageManager.GetString("String_SpellRangeTouch", strLanguage))
                    .CheapReplace("(A)", () => "(" + LanguageManager.GetString("String_SpellRangeArea", strLanguage) + ')')
                    .CheapReplace("MAG", () => LanguageManager.GetString("String_AttributeMAGShort", strLanguage));
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
            switch (Damage)
            {
                case "P":
                    return LanguageManager.GetString("String_DamagePhysical", strLanguage);
                case "S":
                    return LanguageManager.GetString("String_DamageStun", strLanguage);
                default:
                    return LanguageManager.GetString("String_None", strLanguage);
            }
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
                if (!Limited && (!Extended || Name.EndsWith("Extended")) && !RelevantImprovements(o =>
                        o.ImproveType == Improvement.ImprovementType.DrainValue ||
                         o.ImproveType == Improvement.ImprovementType.SpellCategoryDrain).Any()) return strReturn;
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

                strDV = RelevantImprovements(i => i.ImproveType == Improvement.ImprovementType.DrainValue ||
                                                  i.ImproveType == Improvement.ImprovementType.SpellCategoryDrain)
                    .Aggregate(strDV, (current, imp) => current + $" + {imp.Value:0;-0;0}");
                if (Limited)
                {
                    strDV += " + -2";
                }
                if (Extended && !Name.EndsWith("Extended"))
                {
                    strDV += " + 2";
                }
                object xprResult = CommonFunctions.EvaluateInvariantXPath(strDV.TrimStart('+'), out bool blnIsSuccess);
                if (!blnIsSuccess) return strReturn;
                if (force)
                {
                    strReturn = $"F{xprResult:+0;-0;}";
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
            if (strLanguage == GlobalOptions.DefaultLanguage)
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
            set => _strExtra = LanguageManager.ReverseTranslateExtra(value, GlobalOptions.Language);
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
            set => _blnExtended = value;
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
        /// The name of the object as it should be displayed on printouts (translated name only).
        /// </summary>
        public string DisplayNameShort(string strLanguage)
        {
            string strReturn = strLanguage != GlobalOptions.DefaultLanguage ? GetNode(strLanguage)?["translate"]?.InnerText ?? Name : Name;
            if (Extended && !Name.EndsWith("Extended"))
                strReturn += ", " + LanguageManager.GetString("String_SpellExtended", strLanguage);

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
                strReturn += LanguageManager.GetString("String_Space", strLanguage) + '(' + LanguageManager.TranslateExtra(Extra, strLanguage) + ')';
            }
            return strReturn;
        }

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
        #endregion

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
                else if (Category == "Enchantments")
                {
                    return _objCharacter.SkillsSection.GetActiveSkill("Artificing");
                }
                else if (Category == "Rituals")
                {
                    return _objCharacter.SkillsSection.GetActiveSkill("Ritual Spellcasting");
                }
                else
                {
                    return _objCharacter.SkillsSection.GetActiveSkill(UsesUnarmed ? "Unarmed Combat" : "Spellcasting");
                }
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
                    intReturn = UsesUnarmed ? objSkill.PoolOtherAttribute(_objCharacter.MAG.TotalValue, "MAG") : objSkill.Pool;
                    // Add any Specialization bonus if applicable.
                    if (objSkill.HasSpecialization(Category))
                        intReturn += 2;
                }

                // Include any Improvements to the Spell's dicepool.
                intReturn += RelevantImprovements(x =>
                    x.ImproveType == Improvement.ImprovementType.SpellCategory ||
                    x.ImproveType == Improvement.ImprovementType.SpellDicePool).Sum(x => x.Value);

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
                string strSpaceCharacter = LanguageManager.GetString("String_Space", GlobalOptions.Language);
                string strReturn = string.Empty;
                Skill objSkill = Skill;
                if (objSkill != null)
                {
                    int intPool = UsesUnarmed ? objSkill.PoolOtherAttribute(_objCharacter.MAG.TotalValue, "MAG") : objSkill.Pool;
                    strReturn = objSkill.FormattedDicePool(intPool, strSpaceCharacter, Category);
                }

                // Include any Improvements to the Spell Category or Spell Name.
                return RelevantImprovements(x =>
                    x.ImproveType == Improvement.ImprovementType.SpellCategory ||
                    x.ImproveType == Improvement.ImprovementType.SpellDicePool).Aggregate(strReturn,
                    (current, objImprovement) =>
                        current +
                        $"{strSpaceCharacter}+{strSpaceCharacter}{_objCharacter.GetObjectName(objImprovement, GlobalOptions.Language)}{strSpaceCharacter}({objImprovement.Value.ToString(GlobalOptions.CultureInfo)})"
                        );
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
                _objCachedMyXmlNode = XmlManager.Load("spells.xml", strLanguage).SelectSingleNode("/chummer/spells/spell[name = \"" + Name + "\" and category = \"" + Category + "\"]");
                _strCachedXmlNodeLanguage = strLanguage;
            }
            return _objCachedMyXmlNode;
        }



        private IEnumerable<Improvement> RelevantImprovements(Func<Improvement, bool> funcWherePredicate = null)
        {
            foreach (Improvement objImprovement in _objCharacter.Improvements.Where(i => i.Enabled || funcWherePredicate?.Invoke(i) == true))
            {
                switch (objImprovement.ImproveType)
                {
                    case Improvement.ImprovementType.SpellDicePool:
                        if (objImprovement.ImprovedName == Name || objImprovement.ImprovedName == SourceID.ToString())
                            yield return objImprovement;
                        break;
                    case Improvement.ImprovementType.SpellCategory:
                    case Improvement.ImprovementType.SpellCategoryDrain:
                        if (objImprovement.ImprovedName == Category)
                            yield return objImprovement;
                        break;
                    case Improvement.ImprovementType.DrainValue:
                        yield return objImprovement;
                        break;
                }
            }
        }
        #endregion

        #region UI Methods
        public TreeNode CreateTreeNode(ContextMenuStrip cmsSpell, bool blnAddCategory = false)
        {
            if (Grade != 0 && !string.IsNullOrEmpty(Source) && !_objCharacter.Options.BookEnabled(Source))
                return null;

            string strText = DisplayName(GlobalOptions.Language);
            if (blnAddCategory)
            {
                if (Category == "Rituals")
                    strText = LanguageManager.GetString("Label_Ritual", GlobalOptions.Language) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + strText;
                else if (Category == "Enchantments")
                    strText = LanguageManager.GetString("Label_Enchantment", GlobalOptions.Language) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + strText;
            }
            TreeNode objNode = new TreeNode
            {
                Name = InternalId,
                Text = strText,
                Tag = this,
                ContextMenuStrip = cmsSpell,
                ForeColor = PreferredColor,
                ToolTipText = Notes.WordWrap(100)
            };

            return objNode;
        }

        public Color PreferredColor
        {
            get
            {
                if (!string.IsNullOrEmpty(Notes))
                {
                    return Color.SaddleBrown;
                }
                if (Grade != 0)
                {
                    return SystemColors.GrayText;
                }

                return SystemColors.WindowText;
            }
        }
        #endregion

        public bool Remove(Character characterObject, bool blnConfirmDelete = true)
        {
            if (blnConfirmDelete)
            {
                string strMessage = LanguageManager.GetString("Message_DeleteSpell", GlobalOptions.Language);
                if (!characterObject.ConfirmDelete(strMessage)) return false;
            }

            characterObject.Spells.Remove(this);
            ImprovementManager.RemoveImprovements(characterObject, Improvement.ImprovementSource.Spell, InternalId);
            return true;

        }

        public void SetSourceDetail(Control sourceControl)
        {
            if (_objCachedSourceDetail?.Language != GlobalOptions.Language)
                _objCachedSourceDetail = null;
            SourceDetail.SetControl(sourceControl);
        }
    }
}
