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
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;
using NLog;

namespace Chummer.Backend.Equipment
{
    [DebuggerDisplay("{DisplayName(GlobalSettings.DefaultLanguage)}")]
    public sealed class LifestyleQuality : IHasInternalId, IHasName, IHasXmlDataNode, IHasNotes, IHasSource, ICanRemove, IDisposable
    {
        private static Logger Log { get; } = LogManager.GetCurrentClassLogger();
        private Guid _guiID;
        private Guid _guiSourceID;
        private string _strName = string.Empty;
        private string _strCategory = string.Empty;
        private string _strExtra = string.Empty;
        private string _strSource = string.Empty;
        private string _strPage = string.Empty;
        private string _strNotes = string.Empty;
        private Color _colNotes = ColorManager.HasNotesColor;
        private bool _blnContributeToLP = true;
        private bool _blnPrint = true;
        private int _intLP;
        private string _strCost = string.Empty;
        private int _intMultiplier;
        private int _intBaseMultiplier;
        private XmlNode _objCachedMyXmlNode;
        private string _strCachedXmlNodeLanguage = string.Empty;
        private int _intAreaMaximum;
        private int _intArea;
        private int _intSecurity;
        private int _intSecurityMaximum;
        private int _intComfortMaximum;
        private int _intComfort;
        private readonly HashSet<string> _setAllowedFreeLifestyles = Utils.StringHashSetPool.Get();
        private readonly Character _objCharacter;
        private bool _blnFree;
        private bool _blnIsFreeGrid;

        #region Helper Methods

        /// <summary>
        ///     Convert a string to a LifestyleQualityType.
        /// </summary>
        /// <param name="strValue">String value to convert.</param>
        public static QualityType ConvertToLifestyleQualityType(string strValue)
        {
            switch (strValue)
            {
                case "Negative":
                    return QualityType.Negative;

                case "Positive":
                    return QualityType.Positive;

                case "Contracts":
                    return QualityType.Contracts;

                default:
                    return QualityType.Entertainment;
            }
        }

        /// <summary>
        /// Convert a string to a LifestyleQualitySource.
        /// </summary>
        /// <param name="strValue">String value to convert.</param>
        public static QualitySource ConvertToLifestyleQualitySource(string strValue)
        {
            switch (strValue)
            {
                case "BuiltIn":
                    return QualitySource.BuiltIn;
                case "Heritage":
                    return QualitySource.Heritage;
                case "Improvement":
                    return QualitySource.Improvement;
                default:
                    return QualitySource.Selected;
            }
        }

        #endregion Helper Methods

        #region Constructor, Create, Save, Load, and Print Methods

        public LifestyleQuality(Character objCharacter)
        {
            // Create the GUID for the new LifestyleQuality.
            _guiID = Guid.NewGuid();
            _objCharacter = objCharacter;
        }

        /// <summary>
        ///     Create a LifestyleQuality from an XmlNode.
        /// </summary>
        /// <param name="objXmlLifestyleQuality">XmlNode to create the object from.</param>
        /// <param name="objCharacter">Character object the LifestyleQuality will be added to.</param>
        /// <param name="objParentLifestyle">Lifestyle object to which the LifestyleQuality will be added.</param>
        /// <param name="objLifestyleQualitySource">Source of the LifestyleQuality.</param>
        /// <param name="strExtra">Forced value for the LifestyleQuality's Extra string (also used by its bonus node).</param>
        public void Create(XmlNode objXmlLifestyleQuality, Lifestyle objParentLifestyle, Character objCharacter,
            QualitySource objLifestyleQualitySource, string strExtra = "")
        {
            ParentLifestyle = objParentLifestyle;
            if (!objXmlLifestyleQuality.TryGetField("id", Guid.TryParse, out _guiSourceID))
            {
                Log.Warn(new object[] { "Missing id field for xmlnode", objXmlLifestyleQuality });
                Utils.BreakIfDebug();
            }
            else
            {
                _objCachedMyXmlNode = null;
                _objCachedMyXPathNode = null;
            }

            if (objXmlLifestyleQuality.TryGetStringFieldQuickly("name", ref _strName))
            {
                _objCachedMyXmlNode = null;
                _objCachedMyXPathNode = null;
            }

            objXmlLifestyleQuality.TryGetInt32FieldQuickly("lp", ref _intLP);
            objXmlLifestyleQuality.TryGetStringFieldQuickly("cost", ref _strCost);
            objXmlLifestyleQuality.TryGetInt32FieldQuickly("multiplier", ref _intMultiplier);
            objXmlLifestyleQuality.TryGetInt32FieldQuickly("multiplierbaseonly", ref _intBaseMultiplier);
            if (objXmlLifestyleQuality.TryGetStringFieldQuickly("category", ref _strCategory))
                Type = ConvertToLifestyleQualityType(_strCategory);
            OriginSource = objLifestyleQualitySource;
            objXmlLifestyleQuality.TryGetInt32FieldQuickly("areamaximum", ref _intAreaMaximum);
            objXmlLifestyleQuality.TryGetInt32FieldQuickly("comfortsmaximum", ref _intComfortMaximum);
            objXmlLifestyleQuality.TryGetInt32FieldQuickly("securitymaximum", ref _intSecurityMaximum);
            objXmlLifestyleQuality.TryGetInt32FieldQuickly("area", ref _intArea);
            objXmlLifestyleQuality.TryGetInt32FieldQuickly("comforts", ref _intComfort);
            objXmlLifestyleQuality.TryGetInt32FieldQuickly("security", ref _intSecurity);
            objXmlLifestyleQuality.TryGetBoolFieldQuickly("print", ref _blnPrint);
            objXmlLifestyleQuality.TryGetBoolFieldQuickly("contributetolimit", ref _blnContributeToLP);
            if (!objXmlLifestyleQuality.TryGetMultiLineStringFieldQuickly("altnotes", ref _strNotes))
                objXmlLifestyleQuality.TryGetMultiLineStringFieldQuickly("notes", ref _strNotes);

            string sNotesColor = ColorTranslator.ToHtml(ColorManager.HasNotesColor);
            objXmlLifestyleQuality.TryGetStringFieldQuickly("notesColor", ref sNotesColor);
            _colNotes = ColorTranslator.FromHtml(sNotesColor);

            objXmlLifestyleQuality.TryGetStringFieldQuickly("source", ref _strSource);
            objXmlLifestyleQuality.TryGetStringFieldQuickly("page", ref _strPage);

            if (string.IsNullOrEmpty(Notes))
            {
                Notes = CommonFunctions.GetBookNotes(objXmlLifestyleQuality, Name, CurrentDisplayName, Source, Page,
                    DisplayPage(GlobalSettings.Language), _objCharacter);
            }

            _setAllowedFreeLifestyles.Clear();
            string strAllowedFreeLifestyles = string.Empty;
            if (objXmlLifestyleQuality.TryGetStringFieldQuickly("allowed", ref strAllowedFreeLifestyles))
            {
                foreach (string strLoopLifestyle in strAllowedFreeLifestyles.SplitNoAlloc(',', StringSplitOptions.RemoveEmptyEntries))
                    _setAllowedFreeLifestyles.Add(strLoopLifestyle);
            }
            _strExtra = strExtra;
            if (!string.IsNullOrEmpty(_strExtra))
            {
                int intParenthesesIndex = _strExtra.IndexOf('(');
                if (intParenthesesIndex != -1)
                    _strExtra = intParenthesesIndex + 1 < strExtra.Length
                        ? strExtra.Substring(intParenthesesIndex + 1).TrimEndOnce(')')
                        : string.Empty;
            }

            if (string.IsNullOrEmpty(Notes))
            {
                string strEnglishNameOnPage = Name;
                string strNameOnPage = string.Empty;
                // make sure we have something and not just an empty tag
                if (objXmlLifestyleQuality.TryGetStringFieldQuickly("nameonpage", ref strNameOnPage) &&
                    !string.IsNullOrEmpty(strNameOnPage))
                    strEnglishNameOnPage = strNameOnPage;

                string strGearNotes = CommonFunctions.GetTextFromPdf(Source + ' ' + Page, strEnglishNameOnPage, _objCharacter);

                if (string.IsNullOrEmpty(strGearNotes) && !GlobalSettings.Language.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                {
                    string strTranslatedNameOnPage = CurrentDisplayName;

                    // don't check again it is not translated
                    if (strTranslatedNameOnPage != _strName)
                    {
                        // if we found <altnameonpage>, and is not empty and not the same as english we must use that instead
                        if (objXmlLifestyleQuality.TryGetStringFieldQuickly("altnameonpage", ref strNameOnPage)
                            && !string.IsNullOrEmpty(strNameOnPage) && strNameOnPage != strEnglishNameOnPage)
                            strTranslatedNameOnPage = strNameOnPage;

                        Notes = CommonFunctions.GetTextFromPdf(Source + ' ' + DisplayPage(GlobalSettings.Language),
                            strTranslatedNameOnPage, _objCharacter);
                    }
                }
                else
                    Notes = strGearNotes;
            }
            // If the item grants a bonus, pass the information to the Improvement Manager.
            XmlNode xmlBonus = objXmlLifestyleQuality["bonus"];
            if (xmlBonus != null)
            {
                string strOldForced = ImprovementManager.ForcedValue;
                if (!string.IsNullOrEmpty(_strExtra))
                    ImprovementManager.ForcedValue = _strExtra;
                if (!ImprovementManager.CreateImprovements(objCharacter, Improvement.ImprovementSource.Quality,
                    InternalId, xmlBonus, 1, DisplayNameShort(GlobalSettings.Language)))
                {
                    _guiID = Guid.Empty;
                    ImprovementManager.ForcedValue = strOldForced;
                    return;
                }

                if (!string.IsNullOrEmpty(ImprovementManager.SelectedValue))
                    _strExtra = ImprovementManager.SelectedValue;
                ImprovementManager.ForcedValue = strOldForced;
            }

            // Built-In Qualities appear as grey text to show that they cannot be removed.
            if (objLifestyleQualitySource == QualitySource.BuiltIn) Free = true;
        }

        private SourceString _objCachedSourceDetail;

        public SourceString SourceDetail
        {
            get
            {
                if (_objCachedSourceDetail == default)
                    _objCachedSourceDetail = SourceString.GetSourceString(Source,
                        DisplayPage(GlobalSettings.Language), GlobalSettings.Language, GlobalSettings.CultureInfo,
                        _objCharacter);
                return _objCachedSourceDetail;
            }
        }

        /// <summary>
        ///     Save the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        public void Save(XmlWriter objWriter)
        {
            if (objWriter == null)
                return;
            objWriter.WriteStartElement("lifestylequality");
            objWriter.WriteElementString("sourceid", SourceIDString);
            objWriter.WriteElementString("guid", InternalId);
            objWriter.WriteElementString("name", _strName);
            objWriter.WriteElementString("category", _strCategory);
            objWriter.WriteElementString("extra", _strExtra);
            objWriter.WriteElementString("cost", _strCost);
            objWriter.WriteElementString("multiplier", _intMultiplier.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("basemultiplier",
                _intBaseMultiplier.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("lp", _intLP.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("areamaximum", _intAreaMaximum.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("comfortsmaximum", _intComfortMaximum.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("securitymaximum", _intSecurityMaximum.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("area", _intArea.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("comforts", _intComfort.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("security", _intSecurity.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("contributetolimit", _blnContributeToLP.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("print", _blnPrint.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("lifestylequalitytype", Type.ToString());
            objWriter.WriteElementString("lifestylequalitysource", OriginSource.ToString());
            objWriter.WriteElementString("free", _blnFree.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("isfreegrid", _blnIsFreeGrid.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("source", _strSource);
            objWriter.WriteElementString("page", _strPage);
            objWriter.WriteElementString("allowed", _setAllowedFreeLifestyles.Count > 0
                ? string.Join(",", _setAllowedFreeLifestyles)
                : string.Empty);
            if (Bonus != null)
                objWriter.WriteRaw("<bonus>" + Bonus.InnerXml + "</bonus>");
            else
                objWriter.WriteElementString("bonus", string.Empty);
            objWriter.WriteElementString("notes", _strNotes.CleanOfInvalidUnicodeChars());
            objWriter.WriteElementString("notesColor", ColorTranslator.ToHtml(_colNotes));
            objWriter.WriteEndElement();
        }

        /// <summary>
        ///     Load the CharacterAttribute from the XmlNode.
        /// </summary>
        /// <param name="objNode">XmlNode to load.</param>
        /// <param name="objParentLifestyle">Lifestyle object to which this LifestyleQuality belongs.</param>
        public void Load(XmlNode objNode, Lifestyle objParentLifestyle)
        {
            ParentLifestyle = objParentLifestyle;
            if (!objNode.TryGetField("guid", Guid.TryParse, out _guiID))
                _guiID = Guid.NewGuid();
            objNode.TryGetStringFieldQuickly("name", ref _strName);
            _objCachedMyXmlNode = null;
            _objCachedMyXPathNode = null;
            Lazy<XPathNavigator> objMyNode = new Lazy<XPathNavigator>(this.GetNodeXPath);
            if (!objNode.TryGetGuidFieldQuickly("sourceid", ref _guiSourceID))
            {
                objMyNode.Value?.TryGetGuidFieldQuickly("id", ref _guiSourceID);
            }
            objNode.TryGetStringFieldQuickly("extra", ref _strExtra);
            objNode.TryGetInt32FieldQuickly("lp", ref _intLP);
            objNode.TryGetStringFieldQuickly("cost", ref _strCost);
            objNode.TryGetInt32FieldQuickly("multiplier", ref _intMultiplier);
            objNode.TryGetInt32FieldQuickly("basemultiplier", ref _intBaseMultiplier);
            objNode.TryGetBoolFieldQuickly("contributetolimit", ref _blnContributeToLP);
            if (!objNode.TryGetInt32FieldQuickly("areamaximum", ref _intAreaMaximum))
                objMyNode.Value?.TryGetInt32FieldQuickly("areamaximum", ref _intAreaMaximum);
            if (!objNode.TryGetInt32FieldQuickly("area", ref _intArea))
                objMyNode.Value?.TryGetInt32FieldQuickly("area", ref _intArea);
            if (!objNode.TryGetInt32FieldQuickly("securitymaximum", ref _intSecurityMaximum))
                objMyNode.Value?.TryGetInt32FieldQuickly("securitymaximum", ref _intSecurityMaximum);
            if (!objNode.TryGetInt32FieldQuickly("security", ref _intSecurity))
                objMyNode.Value?.TryGetInt32FieldQuickly("security", ref _intSecurity);
            if (!objNode.TryGetInt32FieldQuickly("comforts", ref _intComfort))
                objMyNode.Value?.TryGetInt32FieldQuickly("comforts", ref _intComfort);
            if (!objNode.TryGetInt32FieldQuickly("comfortsmaximum", ref _intComfortMaximum))
                objMyNode.Value?.TryGetInt32FieldQuickly("comfortsmaximum", ref _intComfortMaximum);
            objNode.TryGetBoolFieldQuickly("print", ref _blnPrint);
            if (objNode["lifestylequalitytype"] != null)
                Type = ConvertToLifestyleQualityType(objNode["lifestylequalitytype"].InnerText);
            if (objNode["lifestylequalitysource"] != null)
                OriginSource = ConvertToLifestyleQualitySource(objNode["lifestylequalitysource"].InnerText);
            if (!objNode.TryGetStringFieldQuickly("category", ref _strCategory) && objMyNode.Value?.TryGetStringFieldQuickly("category", ref _strCategory) != true)
                _strCategory = string.Empty;
            objNode.TryGetBoolFieldQuickly("free", ref _blnFree);
            objNode.TryGetBoolFieldQuickly("isfreegrid", ref _blnIsFreeGrid);
            objNode.TryGetStringFieldQuickly("source", ref _strSource);
            objNode.TryGetStringFieldQuickly("page", ref _strPage);
            string strAllowedFreeLifestyles = string.Empty;
            if (!objNode.TryGetStringFieldQuickly("allowed", ref strAllowedFreeLifestyles) && objMyNode.Value?.TryGetStringFieldQuickly("allowed", ref strAllowedFreeLifestyles) != true)
                strAllowedFreeLifestyles = string.Empty;
            _setAllowedFreeLifestyles.Clear();
            foreach (string strLoopLifestyle in strAllowedFreeLifestyles.SplitNoAlloc(',', StringSplitOptions.RemoveEmptyEntries))
                _setAllowedFreeLifestyles.Add(strLoopLifestyle);
            Bonus = objNode["bonus"];
            objNode.TryGetMultiLineStringFieldQuickly("notes", ref _strNotes);

            string sNotesColor = ColorTranslator.ToHtml(ColorManager.HasNotesColor);
            objNode.TryGetStringFieldQuickly("notesColor", ref sNotesColor);
            _colNotes = ColorTranslator.FromHtml(sNotesColor);

            LegacyShim();
        }

        /// <summary>
        ///     Performs actions based on the character's last loaded AppVersion attribute.
        /// </summary>
        private void LegacyShim()
        {
            if (Utils.IsUnitTest)
                return;
            //Unstored Cost and LP values prior to 5.190.2 nightlies.
            if (_objCharacter.LastSavedVersion > new Version(5, 190, 0))
                return;
            XPathNavigator objXmlDocument = _objCharacter.LoadDataXPath("lifestyles.xml");
            XPathNavigator objLifestyleQualityNode = this.GetNodeXPath()
                                                     ?? objXmlDocument.SelectSingleNode("/chummer/qualities/quality[name = " + Name.CleanXPath() + ']');
            if (objLifestyleQualityNode == null)
            {
                using (new FetchSafelyFromPool<List<ListItem>>(Utils.ListItemListPool, out List<ListItem> lstQualities))
                {
                    foreach (XPathNavigator xmlNode in objXmlDocument.SelectAndCacheExpression(
                                 "/chummer/qualities/quality"))
                    {
                        lstQualities.Add(new ListItem(xmlNode.SelectSingleNode("id")?.Value,
                                                      xmlNode.SelectSingleNodeAndCacheExpression("translate")?.Value
                                                      ?? xmlNode.SelectSingleNode("name")?.Value));
                    }

                    using (SelectItem frmSelect = new SelectItem
                           {
                               Description = string.Format(GlobalSettings.CultureInfo,
                                                           LanguageManager.GetString(
                                                               "String_intCannotFindLifestyleQuality"), _strName)
                           })
                    {
                        frmSelect.SetGeneralItemsMode(lstQualities);
                        if (frmSelect.ShowDialogSafe(_objCharacter) == DialogResult.Cancel)
                        {
                            _guiID = Guid.Empty;
                            return;
                        }

                        objLifestyleQualityNode =
                            objXmlDocument.SelectSingleNode("/chummer/qualities/quality[id = "
                                                            + frmSelect.SelectedItem.CleanXPath() + ']');
                    }
                }
            }

            int intTemp = 0;
            string strTemp = string.Empty;
            if (objLifestyleQualityNode.TryGetStringFieldQuickly("cost", ref strTemp))
                CostString = strTemp;
            if (objLifestyleQualityNode.TryGetInt32FieldQuickly("lp", ref intTemp))
                LP = intTemp;
            if (objLifestyleQualityNode.TryGetInt32FieldQuickly("areamaximum", ref intTemp))
                AreaMaximum = intTemp;
            if (objLifestyleQualityNode.TryGetInt32FieldQuickly("comfortsmaximum", ref intTemp))
                ComfortMaximum = intTemp;
            if (objLifestyleQualityNode.TryGetInt32FieldQuickly("securitymaximum", ref intTemp))
                SecurityMaximum = intTemp;
            if (objLifestyleQualityNode.TryGetInt32FieldQuickly("area", ref intTemp))
                Area = intTemp;
            if (objLifestyleQualityNode.TryGetInt32FieldQuickly("comforts", ref intTemp))
                Comfort = intTemp;
            if (objLifestyleQualityNode.TryGetInt32FieldQuickly("security", ref intTemp))
                Security = intTemp;
            if (objLifestyleQualityNode.TryGetInt32FieldQuickly("multiplier", ref intTemp))
                Multiplier = intTemp;
            if (objLifestyleQualityNode.TryGetInt32FieldQuickly("multiplierbaseonly", ref intTemp))
                BaseMultiplier = intTemp;
        }

        /// <summary>
        ///     Print the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        /// <param name="objCulture">Culture in which to print.</param>
        /// <param name="strLanguageToPrint">Language in which to print</param>
        public async ValueTask Print(XmlWriter objWriter, CultureInfo objCulture, string strLanguageToPrint)
        {
            if (!AllowPrint || objWriter == null)
                return;
            // <quality>
            XmlElementWriteHelper objBaseElement = await objWriter.StartElementAsync("quality");
            try
            {
                await objWriter.WriteElementStringAsync("guid", InternalId);
                await objWriter.WriteElementStringAsync("sourceid", SourceIDString);
                await objWriter.WriteElementStringAsync("name", await DisplayNameShortAsync(strLanguageToPrint));
                await objWriter.WriteElementStringAsync("fullname", await DisplayNameAsync(strLanguageToPrint));
                await objWriter.WriteElementStringAsync("formattedname", await FormattedDisplayNameAsync(objCulture, strLanguageToPrint));
                await objWriter.WriteElementStringAsync("extra", await _objCharacter.TranslateExtraAsync(Extra, strLanguageToPrint));
                await objWriter.WriteElementStringAsync("lp", LP.ToString(objCulture));
                await objWriter.WriteElementStringAsync("cost", Cost.ToString(_objCharacter.Settings.NuyenFormat, objCulture));
                string strLifestyleQualityType = Type.ToString();
                if (!strLanguageToPrint.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                {
                    XPathNavigator objNode = (await _objCharacter.LoadDataXPathAsync("lifestyles.xml", strLanguageToPrint))
                        .SelectSingleNode("/chummer/categories/category[. = " + strLifestyleQualityType.CleanXPath() + ']');
                    if (objNode != null)
                        strLifestyleQualityType = (await objNode.SelectSingleNodeAndCacheExpressionAsync("@translate"))?.Value ?? strLifestyleQualityType;
                }

                await objWriter.WriteElementStringAsync("lifestylequalitytype", strLifestyleQualityType);
                await objWriter.WriteElementStringAsync("lifestylequalitytype_english", Type.ToString());
                await objWriter.WriteElementStringAsync("lifestylequalitysource", OriginSource.ToString());
                await objWriter.WriteElementStringAsync("free", Free.ToString());
                await objWriter.WriteElementStringAsync("freebylifestyle", FreeByLifestyle.ToString());
                await objWriter.WriteElementStringAsync("isfreegrid", IsFreeGrid.ToString());
                await objWriter.WriteElementStringAsync("source", await _objCharacter.LanguageBookShortAsync(Source, strLanguageToPrint));
                await objWriter.WriteElementStringAsync("page", await DisplayPageAsync(strLanguageToPrint));
                if (GlobalSettings.PrintNotes)
                    await objWriter.WriteElementStringAsync("notes", Notes);
            }
            finally
            {
                // </quality>
                await objBaseElement.DisposeAsync();
            }
        }

        #endregion Constructor, Create, Save, Load, and Print Methods

        #region Properties

        /// <summary>
        ///     Internal identifier which will be used to identify this LifestyleQuality in the Improvement system.
        /// </summary>
        public string InternalId => _guiID.ToString("D", GlobalSettings.InvariantCultureInfo);

        /// <summary>
        ///     Identifier of the object within data files.
        /// </summary>
        public Guid SourceID => _guiSourceID;

        /// <summary>
        ///     String-formatted identifier of the <inheritdoc cref="SourceID" /> from the data files.
        /// </summary>
        public string SourceIDString => SourceID.ToString("D", GlobalSettings.InvariantCultureInfo);

        /// <summary>
        ///     LifestyleQuality's name.
        /// </summary>
        public string Name
        {
            get => _strName;
            set
            {
                if (_strName == value)
                    return;
                _objCachedMyXmlNode = null;
                _objCachedMyXPathNode = null;
                _strName = value;
            }
        }

        /// <summary>
        ///     LifestyleQuality's parent lifestyle.
        /// </summary>
        public Lifestyle ParentLifestyle { get; set; }

        /// <summary>
        ///     Extra information that should be applied to the name, like a linked CharacterAttribute.
        /// </summary>
        public string Extra
        {
            get => _strExtra;
            set => _strExtra = value;
        }

        /// <summary>
        ///     Sourcebook.
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
            string s = this.GetNodeXPath(strLanguage)?.SelectSingleNodeAndCacheExpression("altpage")?.Value ?? Page;
            return !string.IsNullOrWhiteSpace(s) ? s : Page;
        }

        /// <summary>
        /// Sourcebook Page Number using a given language file.
        /// Returns Page if not found or the string is empty.
        /// </summary>
        /// <param name="strLanguage">Language file keyword to use.</param>
        /// <returns></returns>
        public async ValueTask<string> DisplayPageAsync(string strLanguage)
        {
            if (strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                return Page;
            XPathNavigator objNode = await this.GetNodeXPathAsync(strLanguage);
            string s = objNode != null
                ? (await objNode.SelectSingleNodeAndCacheExpressionAsync("altpage"))?.Value ?? Page
                : Page;
            return !string.IsNullOrWhiteSpace(s) ? s : Page;
        }

        /// <summary>
        ///     Bonus node from the XML file.
        /// </summary>
        public XmlNode Bonus { get; set; }

        /// <summary>
        ///     LifestyleQuality Type.
        /// </summary>
        public QualityType Type { get; private set; } = QualityType.Positive;

        /// <summary>
        ///     Source of the LifestyleQuality.
        /// </summary>
        public QualitySource OriginSource { get; set; } = QualitySource.Selected;

        /// <summary>
        ///     Number of Build Points the LifestyleQuality costs.
        /// </summary>
        public int LP
        {
            get => Free || !ContributesLP ? 0 : _intLP;
            set
            {
                if (_intLP == value)
                    return;
                _intLP = value;
                if (!Free && ContributesLP)
                    ParentLifestyle?.OnPropertyChanged(nameof(Lifestyle.TotalLP));
            }
        }

        /// <summary>
        ///     The name of the object as it should be displayed on printouts (translated name only).
        /// </summary>
        public string DisplayNameShort(string strLanguage)
        {
            if (strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                return Name;

            return this.GetNodeXPath(strLanguage)?.SelectSingleNodeAndCacheExpression("translate")?.Value ?? Name;
        }

        /// <summary>
        ///     The name of the object as it should be displayed on printouts (translated name only).
        /// </summary>
        public async ValueTask<string> DisplayNameShortAsync(string strLanguage)
        {
            if (strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                return Name;

            XPathNavigator objNode = await this.GetNodeXPathAsync(strLanguage);
            return objNode != null ? (await objNode.SelectSingleNodeAndCacheExpressionAsync("translate"))?.Value ?? Name : Name;
        }

        public string CurrentDisplayNameShort => DisplayNameShort(GlobalSettings.Language);

        /// <summary>
        ///     The name of the object as it should be displayed in lists. Name (Extra).
        /// </summary>
        public string DisplayName(string strLanguage)
        {
            string strReturn = DisplayNameShort(strLanguage);

            if (!string.IsNullOrEmpty(Extra))
                // Attempt to retrieve the CharacterAttribute name.
                strReturn += LanguageManager.GetString("String_Space", strLanguage) + '(' +
                             _objCharacter.TranslateExtra(Extra, strLanguage) + ')';
            return strReturn;
        }

        /// <summary>
        ///     The name of the object as it should be displayed in lists. Name (Extra).
        /// </summary>
        public async ValueTask<string> DisplayNameAsync(string strLanguage)
        {
            string strReturn = await DisplayNameShortAsync(strLanguage);

            if (!string.IsNullOrEmpty(Extra))
                // Attempt to retrieve the CharacterAttribute name.
                strReturn += await LanguageManager.GetStringAsync("String_Space", strLanguage) + '(' +
                             await _objCharacter.TranslateExtraAsync(Extra, strLanguage) + ')';
            return strReturn;
        }

        public string CurrentDisplayName => DisplayName(GlobalSettings.Language);

        public string FormattedDisplayName(CultureInfo objCulture, string strLanguage)
        {
            string strReturn = DisplayName(strLanguage);
            string strSpace = LanguageManager.GetString("String_Space", strLanguage);

            if (Multiplier > 0)
                strReturn += strSpace + "[+" + Multiplier.ToString(objCulture) + "%]";
            else if (Multiplier < 0)
                strReturn += strSpace + '[' + Multiplier.ToString(objCulture) + "%]";

            if (Cost > 0)
                strReturn += strSpace + "[+" + Cost.ToString(_objCharacter.Settings.NuyenFormat, objCulture) + "¥]";
            else if (Cost < 0)
                strReturn += strSpace + '[' + Cost.ToString(_objCharacter.Settings.NuyenFormat, objCulture) + "¥]";
            return strReturn;
        }

        public async ValueTask<string> FormattedDisplayNameAsync(CultureInfo objCulture, string strLanguage)
        {
            string strReturn = await DisplayNameAsync(strLanguage);
            string strSpace = await LanguageManager.GetStringAsync("String_Space", strLanguage);

            if (Multiplier > 0)
                strReturn += strSpace + "[+" + Multiplier.ToString(objCulture) + "%]";
            else if (Multiplier < 0)
                strReturn += strSpace + '[' + Multiplier.ToString(objCulture) + "%]";

            if (Cost > 0)
                strReturn += strSpace + "[+" + Cost.ToString(_objCharacter.Settings.NuyenFormat, objCulture) + "¥]";
            else if (Cost < 0)
                strReturn += strSpace + '[' + Cost.ToString(_objCharacter.Settings.NuyenFormat, objCulture) + "¥]";
            return strReturn;
        }

        public string CurrentFormattedDisplayName => FormattedDisplayName(GlobalSettings.CultureInfo, GlobalSettings.Language);

        /// <summary>
        ///     Whether or not the LifestyleQuality appears on the printouts.
        /// </summary>
        public bool AllowPrint
        {
            get => _blnPrint;
            set => _blnPrint = value;
        }

        /// <summary>
        ///     Notes.
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
        ///     Nuyen cost of the Quality.
        /// </summary>
        public decimal Cost
        {
            get
            {
                if (Free || FreeByLifestyle)
                    return 0;
                if (!decimal.TryParse(CostString, NumberStyles.Any, GlobalSettings.InvariantCultureInfo,
                    out decimal decReturn))
                {
                    object objProcess = CommonFunctions.EvaluateInvariantXPath(CostString, out bool blnIsSuccess);
                    if (blnIsSuccess)
                        return Convert.ToDecimal(objProcess, GlobalSettings.InvariantCultureInfo);
                }

                return decReturn;
            }
        }

        /// <summary>
        ///     String for the nuyen cost of the Quality.
        /// </summary>
        public string CostString
        {
            get => string.IsNullOrWhiteSpace(_strCost) ? "0" : _strCost;
            set
            {
                if (_strCost == value)
                    return;
                _strCost = value;
                if (!Free && !FreeByLifestyle)
                    ParentLifestyle?.OnPropertyChanged(nameof(Lifestyle.TotalMonthlyCost));
            }
        }

        /// <summary>
        ///     Does the Quality have a Nuyen or LP cost?
        /// </summary>
        public bool Free
        {
            get => _blnFree || OriginSource == QualitySource.BuiltIn;
            set
            {
                if (_blnFree == value)
                    return;
                _blnFree = value;
                if (ContributesLP)
                {
                    if (!FreeByLifestyle)
                        ParentLifestyle?.OnMultiplePropertyChanged(nameof(Lifestyle.TotalLP), nameof(Lifestyle.TotalMonthlyCost), nameof(Lifestyle.CostMultiplier), nameof(Lifestyle.BaseCostMultiplier));
                    else
                        ParentLifestyle?.OnPropertyChanged(nameof(Lifestyle.TotalLP));
                }
                else if (!FreeByLifestyle)
                    ParentLifestyle?.OnMultiplePropertyChanged(nameof(Lifestyle.TotalMonthlyCost), nameof(Lifestyle.CostMultiplier), nameof(Lifestyle.BaseCostMultiplier));
            }
        }

        public bool IsFreeGrid
        {
            get => _blnIsFreeGrid;
            set
            {
                if (_blnIsFreeGrid == value)
                    return;
                _blnIsFreeGrid = value;
                switch (value)
                {
                    case true when OriginSource == QualitySource.Selected:
                        OriginSource = QualitySource.BuiltIn;
                        break;
                    case false when OriginSource == QualitySource.BuiltIn:
                        OriginSource = QualitySource.Selected;
                        break;
                }
                ParentLifestyle?.OnPropertyChanged(nameof(Lifestyle.LifestyleQualities));
            }
        }

        public bool ContributesLP
        {
            get => _blnContributeToLP;
            set
            {
                if (_blnContributeToLP == value)
                    return;
                _blnContributeToLP = value;
                if (value && !Free)
                    ParentLifestyle?.OnPropertyChanged(nameof(Lifestyle.TotalLP));
            }
        }

        /// <summary>
        ///     Are the costs of this Quality included in base lifestyle costs?
        /// </summary>
        public bool FreeByLifestyle
        {
            get
            {
                if (Type != QualityType.Entertainment && Type != QualityType.Contracts)
                    return false;
                if (_setAllowedFreeLifestyles.Count == 0)
                    return false;
                string strBaseLifestyle = ParentLifestyle?.BaseLifestyle;
                if (string.IsNullOrEmpty(strBaseLifestyle))
                    return false;
                if (_setAllowedFreeLifestyles.Contains(strBaseLifestyle))
                    return true;
                string strEquivalentLifestyle = Lifestyle.GetEquivalentLifestyle(strBaseLifestyle);
                return _setAllowedFreeLifestyles.Contains(strEquivalentLifestyle);
            }
        }

        /// <summary>
        ///     Comfort LP is increased/reduced by this Quality.
        /// </summary>
        public int Comfort
        {
            get => _intComfort;
            set
            {
                if (_intComfort == value)
                    return;
                _intComfort = value;
                ParentLifestyle?.OnMultiplePropertyChanged(nameof(Lifestyle.TotalComforts), nameof(Lifestyle.ComfortsDelta));
            }
        }

        /// <summary>
        ///     Comfort LP maximum is increased/reduced by this Quality.
        /// </summary>
        public int ComfortMaximum
        {
            get => _intComfortMaximum;
            set
            {
                if (_intComfortMaximum == value)
                    return;
                _intComfortMaximum = value;
                ParentLifestyle?.OnPropertyChanged(nameof(Lifestyle.TotalComfortsMaximum));
            }
        }

        /// <summary>
        ///     Security LP value is increased/reduced by this Quality.
        /// </summary>
        public int SecurityMaximum
        {
            get => _intSecurityMaximum;
            set
            {
                if (_intSecurityMaximum == value)
                    return;
                _intSecurityMaximum = value;
                ParentLifestyle?.OnPropertyChanged(nameof(Lifestyle.TotalSecurityMaximum));
            }
        }

        /// <summary>
        ///     Security LP value is increased/reduced by this Quality.
        /// </summary>
        public int Security
        {
            get => _intSecurity;
            set
            {
                if (_intSecurity == value)
                    return;
                _intSecurity = value;
                ParentLifestyle?.OnMultiplePropertyChanged(nameof(Lifestyle.TotalSecurity), nameof(Lifestyle.SecurityDelta));
            }
        }

        /// <summary>
        ///     Percentage by which the quality increases the overall Lifestyle Cost.
        /// </summary>
        public int Multiplier
        {
            get => Free || FreeByLifestyle ? 0 : _intMultiplier;
            set
            {
                if (_intMultiplier == value)
                    return;
                _intMultiplier = value;
                if (!Free && !FreeByLifestyle)
                    ParentLifestyle?.OnPropertyChanged(nameof(Lifestyle.CostMultiplier));
            }
        }

        /// <summary>
        ///     Percentage by which the quality increases the Lifestyle Cost ONLY, without affecting other qualities.
        /// </summary>
        public int BaseMultiplier
        {
            get => Free || FreeByLifestyle ? 0 : _intBaseMultiplier;
            set
            {
                if (_intBaseMultiplier == value)
                    return;
                _intBaseMultiplier = value;
                if (!Free && !FreeByLifestyle)
                    ParentLifestyle?.OnPropertyChanged(nameof(Lifestyle.BaseCostMultiplier));
            }
        }

        /// <summary>
        ///     Category of the Quality.
        /// </summary>
        public string Category
        {
            get => _strCategory;
            set => _strCategory = value;
        }

        /// <summary>
        ///     Area/Neighborhood LP Cost/Benefit of the Quality.
        /// </summary>
        public int AreaMaximum
        {
            get => _intAreaMaximum;
            set
            {
                if (_intAreaMaximum == value)
                    return;
                _intAreaMaximum = value;
                ParentLifestyle?.OnPropertyChanged(nameof(Lifestyle.TotalAreaMaximum));
            }
        }

        /// <summary>
        ///     Area/Neighborhood minimum is increased/reduced by this Quality.
        /// </summary>
        public int Area
        {
            get => _intArea;
            set
            {
                if (_intArea == value)
                    return;
                _intArea = value;
                ParentLifestyle?.OnMultiplePropertyChanged(nameof(Lifestyle.TotalArea), nameof(Lifestyle.AreaDelta));
            }
        }

        public async Task<XmlNode> GetNodeCoreAsync(bool blnSync, string strLanguage)
        {
            if (_objCachedMyXmlNode != null && strLanguage == _strCachedXmlNodeLanguage
                                            && !GlobalSettings.LiveCustomData)
                return _objCachedMyXmlNode;
            _objCachedMyXmlNode = (blnSync
                    // ReSharper disable once MethodHasAsyncOverload
                    ? _objCharacter.LoadData("lifestyles.xml", strLanguage)
                    : await _objCharacter.LoadDataAsync("lifestyles.xml", strLanguage))
                .SelectSingleNode(SourceID == Guid.Empty
                                      ? "/chummer/qualities/quality[name = "
                                        + Name.CleanXPath() + ']'
                                      : "/chummer/qualities/quality[id = "
                                        + SourceIDString.CleanXPath() + " or id = "
                                        + SourceIDString.ToUpperInvariant()
                                                        .CleanXPath()
                                        + ']');
            _strCachedXmlNodeLanguage = strLanguage;
            return _objCachedMyXmlNode;
        }

        private XPathNavigator _objCachedMyXPathNode;
        private string _strCachedXPathNodeLanguage = string.Empty;

        public async Task<XPathNavigator> GetNodeXPathCoreAsync(bool blnSync, string strLanguage)
        {
            if (_objCachedMyXPathNode != null && strLanguage == _strCachedXPathNodeLanguage
                                              && !GlobalSettings.LiveCustomData)
                return _objCachedMyXPathNode;
            _objCachedMyXPathNode = (blnSync
                    // ReSharper disable once MethodHasAsyncOverload
                    ? _objCharacter.LoadDataXPath("lifestyles.xml", strLanguage)
                    : await _objCharacter.LoadDataXPathAsync("lifestyles.xml", strLanguage))
                .SelectSingleNode(SourceID == Guid.Empty
                                      ? "/chummer/qualities/quality[name = "
                                        + Name.CleanXPath() + ']'
                                      : "/chummer/qualities/quality[id = "
                                        + SourceIDString.CleanXPath() + " or id = "
                                        + SourceIDString.ToUpperInvariant()
                                                        .CleanXPath()
                                        + ']');
            _strCachedXPathNodeLanguage = strLanguage;
            return _objCachedMyXPathNode;
        }

        #endregion Properties

        #region UI Methods

        public TreeNode CreateTreeNode()
        {
            if (OriginSource == QualitySource.BuiltIn && !string.IsNullOrEmpty(Source) &&
                !_objCharacter.Settings.BookEnabled(Source))
                return null;

            TreeNode objNode = new TreeNode
            {
                Name = InternalId,
                Text = CurrentFormattedDisplayName,
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
                    return OriginSource == QualitySource.BuiltIn
                        ? ColorManager.GenerateCurrentModeDimmedColor(NotesColor)
                        : ColorManager.GenerateCurrentModeColor(NotesColor);
                }
                return OriginSource == QualitySource.BuiltIn
                    ? ColorManager.GrayText
                    : ColorManager.WindowText;
            }
        }

        #endregion UI Methods

        public void SetSourceDetail(Control sourceControl)
        {
            if (_objCachedSourceDetail.Language != GlobalSettings.Language)
                _objCachedSourceDetail = default;
            SourceDetail.SetControl(sourceControl);
        }

        public Task SetSourceDetailAsync(Control sourceControl, CancellationToken token = default)
        {
            if (_objCachedSourceDetail.Language != GlobalSettings.Language)
                _objCachedSourceDetail = default;
            return SourceDetail.SetControlAsync(sourceControl, token);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Utils.StringHashSetPool.Return(_setAllowedFreeLifestyles);
        }

        public bool Remove(bool blnConfirmDelete = true)
        {
            if (blnConfirmDelete && !CommonFunctions.ConfirmDelete(LanguageManager.GetString("Message_DeleteComplexForm")))
                return false;

            ImprovementManager.RemoveImprovements(_objCharacter, Improvement.ImprovementSource.Quality, InternalId);

            return ParentLifestyle.LifestyleQualities.Remove(this);
        }
    }
}
