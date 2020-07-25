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
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using NLog;

namespace Chummer
{
    /// <summary>
    /// A Technomancer Program or Complex Form.
    /// </summary>
    [HubClassTag("SourceID", true, "Name", "Extra")]
    [DebuggerDisplay("{DisplayName(GlobalOptions.DefaultLanguage)}")]
    public class ComplexForm : IHasInternalId, IHasName, IHasXmlNode, IHasNotes, ICanRemove, IHasSource
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private Guid _guiID;
        private Guid _guiSourceID = Guid.Empty;
        private string _strName = string.Empty;
        private string _strTarget = string.Empty;
        private string _strDuration = string.Empty;
        private string _strFV = string.Empty;
        private string _strSource = string.Empty;
        private string _strPage = string.Empty;
        private string _strNotes = string.Empty;
        private string _strExtra = string.Empty;
        private int _intGrade;
        private readonly Character _objCharacter;
        private SourceString _objCachedSourceDetail;

        #region Constructor, Create, Save, Load, and Print Methods
        public ComplexForm(Character objCharacter)
        {
            // Create the GUID for the new Complex Form.
            _guiID = Guid.NewGuid();
            _objCharacter = objCharacter;
        }

        /// Create a Complex Form from an XmlNode.
        /// <param name="objXmlComplexFormNode">XmlNode to create the object from.</param>
        /// <param name="strExtra">Value to forcefully select for any ImprovementManager prompts.</param>
        public void Create(XmlNode objXmlComplexFormNode, string strExtra = "")
        {
            if (!objXmlComplexFormNode.TryGetField("id", Guid.TryParse, out _guiSourceID))
            {
                Log.Warn(new object[] { "Missing id field for complex form xmlnode", objXmlComplexFormNode });
                Utils.BreakIfDebug();
            }
            objXmlComplexFormNode.TryGetField("id", Guid.TryParse, out _guiSourceID);
            objXmlComplexFormNode.TryGetStringFieldQuickly("name", ref _strName);
            objXmlComplexFormNode.TryGetStringFieldQuickly("target", ref _strTarget);
            objXmlComplexFormNode.TryGetStringFieldQuickly("source", ref _strSource);
            objXmlComplexFormNode.TryGetStringFieldQuickly("page", ref _strPage);
            objXmlComplexFormNode.TryGetStringFieldQuickly("duration", ref _strDuration);
            objXmlComplexFormNode.TryGetStringFieldQuickly("fv", ref _strFV);
            if (!objXmlComplexFormNode.TryGetStringFieldQuickly("altnotes", ref _strNotes))
                objXmlComplexFormNode.TryGetStringFieldQuickly("notes", ref _strNotes);
            if (objXmlComplexFormNode["bonus"] != null)
            {
                ImprovementManager.ForcedValue = strExtra;
                if (!ImprovementManager.CreateImprovements(_objCharacter, Improvement.ImprovementSource.Spell, _guiID.ToString("D", GlobalOptions.InvariantCultureInfo), objXmlComplexFormNode["bonus"], 1, CurrentDisplayName))
                {
                    _guiID = Guid.Empty;
                    return;
                }
                if (!string.IsNullOrEmpty(ImprovementManager.SelectedValue))
                {
                    _strExtra = ImprovementManager.SelectedValue;
                }
            }
            else
            {
                _strExtra = strExtra;
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
            objWriter.WriteStartElement("complexform");
            objWriter.WriteElementString("sourceid", SourceIDString);
            objWriter.WriteElementString("guid", InternalId);
            objWriter.WriteElementString("name", _strName);
            objWriter.WriteElementString("target", _strTarget);
            objWriter.WriteElementString("duration", _strDuration);
            objWriter.WriteElementString("fv", _strFV);
            objWriter.WriteElementString("extra", _strExtra);
            objWriter.WriteElementString("source", _strSource);
            objWriter.WriteElementString("page", _strPage);
            objWriter.WriteElementString("notes", _strNotes);
            objWriter.WriteElementString("grade", _intGrade.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteEndElement();

            if (Grade >= 0)
                _objCharacter.SourceProcess(_strSource);
        }

        /// <summary>
        /// Load the Complex Form from the XmlNode.
        /// </summary>
        /// <param name="objNode">XmlNode to load.</param>
        public void Load(XmlNode objNode)
        {
            if (!objNode.TryGetField("guid", Guid.TryParse, out _guiID))
            {
                _guiID = Guid.NewGuid();
            }
            objNode.TryGetStringFieldQuickly("name", ref _strName);
            if(!objNode.TryGetGuidFieldQuickly("sourceid", ref _guiSourceID))
            {
                XmlNode node = GetNode(GlobalOptions.Language);
                node?.TryGetGuidFieldQuickly("id", ref _guiSourceID);
            }

            objNode.TryGetStringFieldQuickly("target", ref _strTarget);
            objNode.TryGetStringFieldQuickly("source", ref _strSource);
            objNode.TryGetStringFieldQuickly("page", ref _strPage);
            objNode.TryGetStringFieldQuickly("duration", ref _strDuration);
            objNode.TryGetStringFieldQuickly("extra", ref _strExtra);
            objNode.TryGetStringFieldQuickly("fv", ref _strFV);
            objNode.TryGetStringFieldQuickly("notes", ref _strNotes);
            objNode.TryGetInt32FieldQuickly("grade", ref _intGrade);
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
            objWriter.WriteStartElement("complexform");
            objWriter.WriteElementString("guid", InternalId);
            objWriter.WriteElementString("sourceid", SourceIDString);
            objWriter.WriteElementString("name", DisplayNameShort(strLanguageToPrint));
            objWriter.WriteElementString("fullname", DisplayName(strLanguageToPrint));
            objWriter.WriteElementString("name_english", Name);
            objWriter.WriteElementString("duration", DisplayDuration(strLanguageToPrint));
            objWriter.WriteElementString("fv", DisplayFV(strLanguageToPrint));
            objWriter.WriteElementString("target", DisplayTarget(strLanguageToPrint));
            objWriter.WriteElementString("source", _objCharacter.LanguageBookShort(Source, strLanguageToPrint));
            objWriter.WriteElementString("page", DisplayPage(strLanguageToPrint));
            if (_objCharacter.Options.PrintNotes)
                objWriter.WriteElementString("notes", Notes);
            objWriter.WriteEndElement();
        }
        #endregion

        #region Properties
        /// <summary>
        /// Identifier of the object within data files.
        /// </summary>
        public Guid SourceID
        {
            get => _guiSourceID;
            set
            {
                if (_guiSourceID == value) return;
                _guiSourceID = value;
                _objCachedMyXmlNode = null;
            }
        }

        /// <summary>
        /// String-formatted identifier of the <inheritdoc cref="SourceID"/> from the data files.
        /// </summary>
        public string SourceIDString => _guiSourceID.ToString("D", GlobalOptions.InvariantCultureInfo);

        /// <summary>
        /// Internal identifier which will be used to identify this Complex Form in the Improvement system.
        /// </summary>
        public string InternalId => _guiID.ToString("D", GlobalOptions.InvariantCultureInfo);

        /// <summary>
        /// Complex Form's name.
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
        public SourceString SourceDetail => _objCachedSourceDetail = _objCachedSourceDetail
                                                                     ?? new SourceString(Source, DisplayPage(GlobalOptions.Language), GlobalOptions.Language, _objCharacter);

        /// <summary>
        /// Complex Form's extra info.
        /// </summary>
        public string Extra
        {
            get => _strExtra;
            set => _strExtra = _objCharacter.ReverseTranslateExtra(value);
        }

        /// <summary>
        /// Complex Form's grade.
        /// </summary>
        public int Grade
        {
            get => _intGrade;
            set => _intGrade = value;
        }

        /// <summary>
        /// The name of the object as it should be displayed on printouts (translated name only).
        /// </summary>
        public string DisplayNameShort(string strLanguage)
        {
            string strReturn = _strName;
            // Get the translated name if applicable.
            if (strLanguage != GlobalOptions.DefaultLanguage)
                strReturn = GetNode(strLanguage)?["translate"]?.InnerText ?? _strName;

            return strReturn;
        }

        public string DisplayName(string strLanguage)
        {
            string strReturn = DisplayNameShort(strLanguage);

            if (!string.IsNullOrEmpty(Extra))
            {
                string strExtra = Extra;
                if (strLanguage != GlobalOptions.DefaultLanguage)
                    strExtra = _objCharacter.TranslateExtra(Extra, strLanguage);
                strReturn += LanguageManager.GetString("String_Space", strLanguage) + '(' + strExtra + ')';
            }
            return strReturn;
        }

        /// <summary>
        /// The name of the object as it should be displayed in lists. Name (Extra).
        /// </summary>
        public string CurrentDisplayName => DisplayName(GlobalOptions.Language);

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
        /// Complex Form's Duration.
        /// </summary>
        public string Duration
        {
            get => _strDuration;
            set => _strDuration = value;
        }

        /// <summary>
        /// Translated Fading Value.
        /// </summary>
        public string DisplayFV(string strLanguage)
        {
            string strReturn = FV.Replace('/', '÷');
            if (strLanguage != GlobalOptions.DefaultLanguage)
            {
                strReturn = strReturn.CheapReplace("L", () => LanguageManager.GetString("String_ComplexFormLevel", strLanguage))
                    .CheapReplace("Overflow damage", () => LanguageManager.GetString("String_SpellOverflowDamage", strLanguage))
                    .CheapReplace("Damage Value", () => LanguageManager.GetString("String_SpellDamageValue", strLanguage))
                    .CheapReplace("Toxin DV", () => LanguageManager.GetString("String_SpellToxinDV", strLanguage))
                    .CheapReplace("Disease DV", () => LanguageManager.GetString("String_SpellDiseaseDV", strLanguage))
                    .CheapReplace("Radiation Power", () => LanguageManager.GetString("String_SpellRadiationPower", strLanguage));
            }
            return strReturn;
        }

        /// <summary>
        /// Fading Tooltip.
        /// </summary>
        public string FVTooltip
        {
            get
            {
                StringBuilder strTip = new StringBuilder(LanguageManager.GetString("Tip_ComplexFormFadingBase"));
                int intRES = _objCharacter.RES.TotalValue;
                string strFV = FV;
                string strSpace = LanguageManager.GetString("String_Space");
                for (int i = 1; i <= intRES * 2; i++)
                {
                    // Calculate the Complex Form's Fading for the current Level.
                    object xprResult = CommonFunctions.EvaluateInvariantXPath(strFV.Replace("L", i.ToString(GlobalOptions.InvariantCultureInfo)).Replace("/", " div "), out bool blnIsSuccess);

                    if (blnIsSuccess && strFV != "Special")
                    {
                        int intFV = Convert.ToInt32(Math.Floor(Convert.ToDouble(xprResult.ToString(), GlobalOptions.InvariantCultureInfo)));

                        // Fading cannot be lower than 2.
                        if (intFV < 2)
                            intFV = 2;
                        strTip.Append(Environment.NewLine + LanguageManager.GetString("String_Level") + strSpace + i.ToString(GlobalOptions.CultureInfo) +
                                      LanguageManager.GetString("String_Colon") + strSpace + intFV.ToString(GlobalOptions.CultureInfo));
                    }
                    else
                    {
                        strTip.Clear();
                        strTip.Append(LanguageManager.GetString("Tip_ComplexFormFadingSeeDescription"));
                        break;
                    }
                }

                List<Improvement> lstFadingImprovements = _objCharacter.Improvements.Where(o => o.ImproveType == Improvement.ImprovementType.FadingValue && o.Enabled).ToList();
                if (lstFadingImprovements.Count > 0)
                {
                    strTip.Append(Environment.NewLine + LanguageManager.GetString("Label_Bonus"));
                    foreach (Improvement objLoopImprovement in lstFadingImprovements)
                    {
                        strTip.Append(Environment.NewLine + _objCharacter.GetObjectName(objLoopImprovement) + strSpace + '(' + objLoopImprovement.Value.ToString("0;-0;0") + ')');
                    }
                }

                return strTip.ToString();
            }
        }

        /// <summary>
        /// The Complex Form's FV.
        /// </summary>
        public string FV
        {
            get
            {
                string strReturn = _strFV;
                bool force = strReturn.StartsWith('L');
                if (_objCharacter.Improvements.Any(o => o.ImproveType == Improvement.ImprovementType.FadingValue && o.Enabled))
                {
                    string strFV = strReturn.TrimStartOnce('L');
                    //Navigator can't do math on a single value, so inject a mathable value.
                    if (string.IsNullOrEmpty(strFV))
                    {
                        strFV = "0";
                    }
                    else
                    {
                        int intPos = strReturn.IndexOf('-');
                        if (intPos != -1)
                        {
                            strFV = strReturn.Substring(intPos);
                        }
                        else
                        {
                            intPos = strReturn.IndexOf('+');
                            if (intPos != -1)
                            {
                                strFV = strReturn.Substring(intPos);
                            }
                        }
                    }
                    foreach (Improvement imp in _objCharacter.Improvements.Where(i => i.ImproveType == Improvement.ImprovementType.FadingValue && i.Enabled))
                    {
                        strFV += " + " + imp.Value.ToString("0;-0;0", GlobalOptions.InvariantCultureInfo);
                    }
                    object xprResult = CommonFunctions.EvaluateInvariantXPath(strFV.TrimStart('+'), out bool blnIsSuccess);
                    if (blnIsSuccess)
                    {
                        if (force)
                        {
                            strReturn = string.Format(GlobalOptions.CultureInfo, "L{0:+0;-0;}", xprResult);
                        }
                        else if (xprResult.ToString() != "0")
                        {
                            strReturn += xprResult;
                        }
                    }
                }
                return strReturn;
            }
            set => _strFV = value;
        }

        /// <summary>
        /// Translated Duration.
        /// </summary>
        public string DisplayTarget(string strLanguage)
        {
            switch (Target)
            {
                case "Persona":
                    return LanguageManager.GetString("String_ComplexFormTargetPersona", strLanguage);
                case "Device":
                    return LanguageManager.GetString("String_ComplexFormTargetDevice", strLanguage);
                case "File":
                    return LanguageManager.GetString("String_ComplexFormTargetFile", strLanguage);
                case "Self":
                    return LanguageManager.GetString("String_SpellRangeSelf", strLanguage);
                case "Sprite":
                    return LanguageManager.GetString("String_ComplexFormTargetSprite", strLanguage);
                case "Host":
                    return LanguageManager.GetString("String_ComplexFormTargetHost", strLanguage);
                case "IC":
                    return LanguageManager.GetString("String_ComplexFormTargetIC", strLanguage);
                default:
                    return LanguageManager.GetString("String_None", strLanguage);
            }
        }

        /// <summary>
        /// The Complex Form's Target.
        /// </summary>
        public string Target
        {
            get => _strTarget;
            set => _strTarget = value;
        }

        /// <summary>
        /// Complex Form's Source.
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
        /// Notes.
        /// </summary>
        public string Notes
        {
            get => _strNotes;
            set => _strNotes = value;
        }

        public Skill Skill => _objCharacter.SkillsSection.GetActiveSkill("Software");

        /// <summary>
        /// The Dice Pool size for the Active Skill required to thread the Complex Form.
        /// </summary>
        public int DicePool
        {
            get
            {
                int intReturn = 0;
                if (Skill != null)
                {
                    intReturn = Skill.PoolOtherAttribute(_objCharacter.RES.TotalValue, "RES");
                    // Add any Specialization bonus if applicable.
                    intReturn += Skill.GetSpecializationBonus(CurrentDisplayName);
                }

                // Include any Improvements to Threading.
                intReturn += ImprovementManager.ValueOf(_objCharacter, Improvement.ImprovementType.ActionDicePool, false, "Threading");

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
                string strReturn = string.Empty;
                if (Skill != null)
                {
                    strReturn = Skill.FormattedDicePool(Skill.PoolOtherAttribute(_objCharacter.RES.TotalValue, "RES"), CurrentDisplayName);
                }

                // Include any Improvements to the Spell Category.
                foreach (Improvement objImprovement in _objCharacter.Improvements
                    .Where(objImprovement => objImprovement.ImproveType == Improvement.ImprovementType.ActionDicePool && objImprovement.Enabled
                    && objImprovement.ImprovedName == "Threading"))
                {
                    strReturn += string.Format(GlobalOptions.CultureInfo, "{0}+{0}{1}{0}({2})",
                        strSpace, _objCharacter.GetObjectName(objImprovement), objImprovement.Value);
                }

                return strReturn;
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
                _objCachedMyXmlNode = _objCharacter.LoadData("complexforms.xml", strLanguage)
                    .SelectSingleNode(SourceID == Guid.Empty
                        ? $"/chummer/complexforms/complexform[name = \"{Name}\"]"
                        : $"/chummer/complexforms/complexform[id = \"{SourceIDString}\" or id = \"{SourceIDString.ToUpperInvariant()}\"]");
                _strCachedXmlNodeLanguage = strLanguage;
            }
            return _objCachedMyXmlNode;
        }
        #endregion

        #region UI Methods
        public TreeNode CreateTreeNode(ContextMenuStrip cmsComplexForm)
        {
            if (Grade != 0 && !string.IsNullOrEmpty(Source) && !_objCharacter.Options.BookEnabled(Source))
                return null;

            TreeNode objNode = new TreeNode
            {
                Name = InternalId,
                Text = CurrentDisplayName,
                Tag = this,
                ContextMenuStrip = cmsComplexForm,
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

        public bool Remove(bool blnConfirmDelete = true)
        {
            if (blnConfirmDelete)
            {
                if (!CommonFunctions.ConfirmDelete(LanguageManager.GetString("Message_DeleteComplexForm")))
                    return false;
            }

            ImprovementManager.RemoveImprovements(_objCharacter, Improvement.ImprovementSource.ComplexForm, InternalId);

            return _objCharacter.ComplexForms.Remove(this);
        }

        public void SetSourceDetail(Control sourceControl)
        {
            if (_objCachedSourceDetail?.Language != GlobalOptions.Language)
                _objCachedSourceDetail = null;
            SourceDetail.SetControl(sourceControl);
        }
    }
}
