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
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;
using Chummer.Annotations;
using NLog;

namespace Chummer.Backend.Equipment
{
    [DebuggerDisplay("{DisplayName(GlobalSettings.DefaultLanguage)}")]
    public sealed class LifestyleQuality : IHasInternalId, IHasName, IHasXmlDataNode, IHasNotes, IHasSource, ICanRemove, INotifyMultiplePropertyChanged, IHasLockObject
    {
        private static readonly Lazy<Logger> s_ObjLogger = new Lazy<Logger>(LogManager.GetCurrentClassLogger);
        private static Logger Log => s_ObjLogger.Value;

        private Guid _guiID;
        private Guid _guiSourceID;
        private string _strName = string.Empty;
        private string _strCategory = string.Empty;
        private string _strExtra = string.Empty;
        private string _strSource = string.Empty;
        private string _strPage = string.Empty;
        private string _strNotes = string.Empty;
        private Color _colNotes = ColorManager.HasNotesColor;
        private bool _blnUseLPCost = true;
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
        private int _intComfortsMaximum;
        private int _intComforts;
        private HashSet<string> _setAllowedFreeLifestyles = Utils.StringHashSetPool.Get();
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
            using (LockObject.EnterWriteLock())
            {
                _objParentLifestyle = objParentLifestyle;
                if (!objXmlLifestyleQuality.TryGetField("id", Guid.TryParse, out _guiSourceID))
                {
                    Log.Warn(new object[] {"Missing id field for xmlnode", objXmlLifestyleQuality});
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
                    _eType = ConvertToLifestyleQualityType(_strCategory);
                OriginSource = objLifestyleQualitySource;
                objXmlLifestyleQuality.TryGetInt32FieldQuickly("areamaximum", ref _intAreaMaximum);
                objXmlLifestyleQuality.TryGetInt32FieldQuickly("comfortsmaximum", ref _intComfortsMaximum);
                objXmlLifestyleQuality.TryGetInt32FieldQuickly("securitymaximum", ref _intSecurityMaximum);
                objXmlLifestyleQuality.TryGetInt32FieldQuickly("area", ref _intArea);
                objXmlLifestyleQuality.TryGetInt32FieldQuickly("comforts", ref _intComforts);
                objXmlLifestyleQuality.TryGetInt32FieldQuickly("security", ref _intSecurity);
                objXmlLifestyleQuality.TryGetBoolFieldQuickly("print", ref _blnPrint);
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
                    foreach (string strLoopLifestyle in strAllowedFreeLifestyles.SplitNoAlloc(
                                 ',', StringSplitOptions.RemoveEmptyEntries))
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

                    string strGearNotes
                        = CommonFunctions.GetTextFromPdf(Source + ' ' + Page, strEnglishNameOnPage, _objCharacter);

                    if (string.IsNullOrEmpty(strGearNotes)
                        && !GlobalSettings.Language.Equals(GlobalSettings.DefaultLanguage,
                                                           StringComparison.OrdinalIgnoreCase))
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
                                                               InternalId, xmlBonus, 1, CurrentDisplayNameShort))
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
                if (objLifestyleQualitySource == QualitySource.BuiltIn)
                    Free = true;
            }
        }

        private SourceString _objCachedSourceDetail;

        public SourceString SourceDetail
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                {
                    if (_objCachedSourceDetail == default)
                        _objCachedSourceDetail = SourceString.GetSourceString(Source,
                                                                              DisplayPage(GlobalSettings.Language),
                                                                              GlobalSettings.Language,
                                                                              GlobalSettings.CultureInfo,
                                                                              _objCharacter);
                    return _objCachedSourceDetail;
                }
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
            using (EnterReadLock.Enter(LockObject))
            {
                objWriter.WriteStartElement("lifestylequality");
                objWriter.WriteElementString("sourceid", SourceIDString);
                objWriter.WriteElementString("guid", InternalId);
                objWriter.WriteElementString("name", _strName);
                objWriter.WriteElementString("category", _strCategory);
                objWriter.WriteElementString("extra", _strExtra);
                objWriter.WriteElementString("cost", _strCost);
                objWriter.WriteElementString("multiplier",
                                             _intMultiplier.ToString(GlobalSettings.InvariantCultureInfo));
                objWriter.WriteElementString("basemultiplier",
                                             _intBaseMultiplier.ToString(GlobalSettings.InvariantCultureInfo));
                objWriter.WriteElementString("lp", _intLP.ToString(GlobalSettings.InvariantCultureInfo));
                objWriter.WriteElementString("areamaximum",
                                             _intAreaMaximum.ToString(GlobalSettings.InvariantCultureInfo));
                objWriter.WriteElementString("comfortsmaximum",
                                             _intComfortsMaximum.ToString(GlobalSettings.InvariantCultureInfo));
                objWriter.WriteElementString("securitymaximum",
                                             _intSecurityMaximum.ToString(GlobalSettings.InvariantCultureInfo));
                objWriter.WriteElementString("area", _intArea.ToString(GlobalSettings.InvariantCultureInfo));
                objWriter.WriteElementString("comforts", _intComforts.ToString(GlobalSettings.InvariantCultureInfo));
                objWriter.WriteElementString("security", _intSecurity.ToString(GlobalSettings.InvariantCultureInfo));
                objWriter.WriteElementString("uselpcost", _blnUseLPCost.ToString(GlobalSettings.InvariantCultureInfo));
                objWriter.WriteElementString("print", _blnPrint.ToString(GlobalSettings.InvariantCultureInfo));
                objWriter.WriteElementString("lifestylequalitytype", Type.ToString());
                objWriter.WriteElementString("lifestylequalitysource", OriginSource.ToString());
                objWriter.WriteElementString("free", _blnFree.ToString(GlobalSettings.InvariantCultureInfo));
                objWriter.WriteElementString("isfreegrid",
                                             _blnIsFreeGrid.ToString(GlobalSettings.InvariantCultureInfo));
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
        }

        /// <summary>
        ///     Load the CharacterAttribute from the XmlNode.
        /// </summary>
        /// <param name="objNode">XmlNode to load.</param>
        /// <param name="objParentLifestyle">Lifestyle object to which this LifestyleQuality belongs.</param>
        public void Load(XmlNode objNode, Lifestyle objParentLifestyle)
        {
            using (LockObject.EnterWriteLock())
            {
                _objParentLifestyle = objParentLifestyle;
                if (!objNode.TryGetField("guid", Guid.TryParse, out _guiID))
                    _guiID = Guid.NewGuid();
                objNode.TryGetStringFieldQuickly("name", ref _strName);
                _objCachedMyXmlNode = null;
                _objCachedMyXPathNode = null;
                Lazy<XPathNavigator> objMyNode = new Lazy<XPathNavigator>(() => this.GetNodeXPath());
                if (!objNode.TryGetGuidFieldQuickly("sourceid", ref _guiSourceID))
                {
                    objMyNode.Value?.TryGetGuidFieldQuickly("id", ref _guiSourceID);
                }

                objNode.TryGetStringFieldQuickly("extra", ref _strExtra);
                objNode.TryGetInt32FieldQuickly("lp", ref _intLP);
                objNode.TryGetStringFieldQuickly("cost", ref _strCost);
                objNode.TryGetInt32FieldQuickly("multiplier", ref _intMultiplier);
                objNode.TryGetInt32FieldQuickly("basemultiplier", ref _intBaseMultiplier);
                if (!objNode.TryGetBoolFieldQuickly("uselpcost", ref _blnUseLPCost))
                    objNode.TryGetBoolFieldQuickly("contributetolimit", ref _blnUseLPCost);
                if (!objNode.TryGetInt32FieldQuickly("areamaximum", ref _intAreaMaximum))
                    objMyNode.Value?.TryGetInt32FieldQuickly("areamaximum", ref _intAreaMaximum);
                if (!objNode.TryGetInt32FieldQuickly("area", ref _intArea))
                    objMyNode.Value?.TryGetInt32FieldQuickly("area", ref _intArea);
                if (!objNode.TryGetInt32FieldQuickly("securitymaximum", ref _intSecurityMaximum))
                    objMyNode.Value?.TryGetInt32FieldQuickly("securitymaximum", ref _intSecurityMaximum);
                if (!objNode.TryGetInt32FieldQuickly("security", ref _intSecurity))
                    objMyNode.Value?.TryGetInt32FieldQuickly("security", ref _intSecurity);
                if (!objNode.TryGetInt32FieldQuickly("comforts", ref _intComforts))
                    objMyNode.Value?.TryGetInt32FieldQuickly("comforts", ref _intComforts);
                if (!objNode.TryGetInt32FieldQuickly("comfortsmaximum", ref _intComfortsMaximum))
                    objMyNode.Value?.TryGetInt32FieldQuickly("comfortsmaximum", ref _intComfortsMaximum);
                objNode.TryGetBoolFieldQuickly("print", ref _blnPrint);
                if (objNode["lifestylequalitytype"] != null)
                    _eType = ConvertToLifestyleQualityType(objNode["lifestylequalitytype"].InnerText);
                if (objNode["lifestylequalitysource"] != null)
                    OriginSource = ConvertToLifestyleQualitySource(objNode["lifestylequalitysource"].InnerText);
                if (!objNode.TryGetStringFieldQuickly("category", ref _strCategory)
                    && objMyNode.Value?.TryGetStringFieldQuickly("category", ref _strCategory) != true)
                    _strCategory = string.Empty;
                objNode.TryGetBoolFieldQuickly("free", ref _blnFree);
                objNode.TryGetBoolFieldQuickly("isfreegrid", ref _blnIsFreeGrid);
                objNode.TryGetStringFieldQuickly("source", ref _strSource);
                objNode.TryGetStringFieldQuickly("page", ref _strPage);
                string strAllowedFreeLifestyles = string.Empty;
                if (!objNode.TryGetStringFieldQuickly("allowed", ref strAllowedFreeLifestyles)
                    && objMyNode.Value?.TryGetStringFieldQuickly("allowed", ref strAllowedFreeLifestyles) != true)
                    strAllowedFreeLifestyles = string.Empty;
                _setAllowedFreeLifestyles.Clear();
                foreach (string strLoopLifestyle in strAllowedFreeLifestyles.SplitNoAlloc(
                             ',', StringSplitOptions.RemoveEmptyEntries))
                    _setAllowedFreeLifestyles.Add(strLoopLifestyle);
                Bonus = objNode["bonus"];
                objNode.TryGetMultiLineStringFieldQuickly("notes", ref _strNotes);

                string sNotesColor = ColorTranslator.ToHtml(ColorManager.HasNotesColor);
                objNode.TryGetStringFieldQuickly("notesColor", ref sNotesColor);
                _colNotes = ColorTranslator.FromHtml(sNotesColor);

                LegacyShim();
            }
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
            using (LockObject.EnterWriteLock())
            {
                XPathNavigator objXmlDocument = _objCharacter.LoadDataXPath("lifestyles.xml");
                XPathNavigator objLifestyleQualityNode = this.GetNodeXPath()
                                                         ?? objXmlDocument.SelectSingleNode(
                                                             "/chummer/qualities/quality[name = " + Name.CleanXPath()
                                                             + ']');
                if (objLifestyleQualityNode == null)
                {
                    using (new FetchSafelyFromPool<List<ListItem>>(Utils.ListItemListPool,
                                                                   out List<ListItem> lstQualities))
                    {
                        foreach (XPathNavigator xmlNode in objXmlDocument.SelectAndCacheExpression(
                                     "/chummer/qualities/quality"))
                        {
                            lstQualities.Add(new ListItem(xmlNode.SelectSingleNodeAndCacheExpression("id")?.Value,
                                                          xmlNode.SelectSingleNodeAndCacheExpression("translate")?.Value
                                                          ?? xmlNode.SelectSingleNodeAndCacheExpression("name")
                                                                    ?.Value));
                        }

                        using (ThreadSafeForm<SelectItem> frmSelect = ThreadSafeForm<SelectItem>.Get(
                                   () => new SelectItem
                                   {
                                       Description = string.Format(GlobalSettings.CultureInfo,
                                                                   LanguageManager.GetString(
                                                                       "String_intCannotFindLifestyleQuality"),
                                                                   _strName)
                                   }))
                        {
                            frmSelect.MyForm.SetGeneralItemsMode(lstQualities);
                            if (frmSelect.ShowDialogSafe(_objCharacter) == DialogResult.Cancel)
                            {
                                _guiID = Guid.Empty;
                                return;
                            }

                            objLifestyleQualityNode =
                                objXmlDocument.SelectSingleNode("/chummer/qualities/quality[id = "
                                                                + frmSelect.MyForm.SelectedItem.CleanXPath() + ']');
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
                    ComfortsMaximum = intTemp;
                if (objLifestyleQualityNode.TryGetInt32FieldQuickly("securitymaximum", ref intTemp))
                    SecurityMaximum = intTemp;
                if (objLifestyleQualityNode.TryGetInt32FieldQuickly("area", ref intTemp))
                    Area = intTemp;
                if (objLifestyleQualityNode.TryGetInt32FieldQuickly("comforts", ref intTemp))
                    Comforts = intTemp;
                if (objLifestyleQualityNode.TryGetInt32FieldQuickly("security", ref intTemp))
                    Security = intTemp;
                if (objLifestyleQualityNode.TryGetInt32FieldQuickly("multiplier", ref intTemp))
                    Multiplier = intTemp;
                if (objLifestyleQualityNode.TryGetInt32FieldQuickly("multiplierbaseonly", ref intTemp))
                    BaseMultiplier = intTemp;
            }
        }

        /// <summary>
        ///     Print the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        /// <param name="objCulture">Culture in which to print.</param>
        /// <param name="strLanguageToPrint">Language in which to print</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public async ValueTask Print(XmlWriter objWriter, CultureInfo objCulture, string strLanguageToPrint, CancellationToken token = default)
        {
            if (objWriter == null)
                return;
            using (await EnterReadLock.EnterAsync(LockObject, token).ConfigureAwait(false))
            {
                if (!AllowPrint)
                    return;
                // <quality>
                XmlElementWriteHelper objBaseElement
                    = await objWriter.StartElementAsync("quality", token).ConfigureAwait(false);
                try
                {
                    await objWriter.WriteElementStringAsync("guid", InternalId, token).ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("sourceid", SourceIDString, token).ConfigureAwait(false);
                    await objWriter
                          .WriteElementStringAsync(
                              "name", await DisplayNameShortAsync(strLanguageToPrint, token).ConfigureAwait(false),
                              token).ConfigureAwait(false);
                    await objWriter
                          .WriteElementStringAsync(
                              "fullname", await DisplayNameAsync(strLanguageToPrint, token).ConfigureAwait(false),
                              token).ConfigureAwait(false);
                    await objWriter
                          .WriteElementStringAsync("formattedname",
                                                   await FormattedDisplayNameAsync(
                                                       objCulture, strLanguageToPrint, token).ConfigureAwait(false),
                                                   token).ConfigureAwait(false);
                    await objWriter
                          .WriteElementStringAsync(
                              "extra",
                              await _objCharacter.TranslateExtraAsync(Extra, strLanguageToPrint, token: token)
                                                 .ConfigureAwait(false), token).ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("lp", LP.ToString(objCulture), token).ConfigureAwait(false);
                    await objWriter
                          .WriteElementStringAsync(
                              "cost", Cost.ToString(_objCharacter.Settings.NuyenFormat, objCulture), token)
                          .ConfigureAwait(false);
                    string strLifestyleQualityType = Type.ToString();
                    if (!strLanguageToPrint.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                    {
                        XPathNavigator objNode
                            = (await _objCharacter
                                     .LoadDataXPathAsync("lifestyles.xml", strLanguageToPrint, token: token)
                                     .ConfigureAwait(false))
                            .SelectSingleNode("/chummer/categories/category[. = " + strLifestyleQualityType.CleanXPath()
                                              + ']');
                        if (objNode != null)
                            strLifestyleQualityType
                                = (await objNode.SelectSingleNodeAndCacheExpressionAsync("@translate", token)
                                                .ConfigureAwait(false))?.Value ?? strLifestyleQualityType;
                    }

                    await objWriter.WriteElementStringAsync("lifestylequalitytype", strLifestyleQualityType, token)
                                   .ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("lifestylequalitytype_english", Type.ToString(), token)
                                   .ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("lifestylequalitysource", OriginSource.ToString(), token)
                                   .ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("free", Free.ToString(), token).ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("freebylifestyle", CanBeFreeByLifestyle.ToString(), token)
                                   .ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("isfreegrid", IsFreeGrid.ToString(), token)
                                   .ConfigureAwait(false);
                    await objWriter
                          .WriteElementStringAsync(
                              "source",
                              await _objCharacter.LanguageBookShortAsync(Source, strLanguageToPrint, token)
                                                 .ConfigureAwait(false), token).ConfigureAwait(false);
                    await objWriter
                          .WriteElementStringAsync(
                              "page", await DisplayPageAsync(strLanguageToPrint, token).ConfigureAwait(false), token)
                          .ConfigureAwait(false);
                    if (GlobalSettings.PrintNotes)
                        await objWriter.WriteElementStringAsync("notes", Notes, token).ConfigureAwait(false);
                }
                finally
                {
                    // </quality>
                    await objBaseElement.DisposeAsync().ConfigureAwait(false);
                }
            }
        }

        #endregion Constructor, Create, Save, Load, and Print Methods

        #region Properties

        /// <summary>
        ///     Internal identifier which will be used to identify this LifestyleQuality in the Improvement system.
        /// </summary>
        public string InternalId
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                    return _guiID.ToString("D", GlobalSettings.InvariantCultureInfo);
            }
        }

        /// <summary>
        ///     Identifier of the object within data files.
        /// </summary>
        public Guid SourceID
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                    return _guiSourceID;
            }
        }

        /// <summary>
        ///     String-formatted identifier of the <inheritdoc cref="SourceID" /> from the data files.
        /// </summary>
        public string SourceIDString => SourceID.ToString("D", GlobalSettings.InvariantCultureInfo);

        /// <summary>
        ///     LifestyleQuality's name.
        /// </summary>
        public string Name
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                    return _strName;
            }
            set
            {
                using (EnterReadLock.Enter(LockObject))
                {
                    if (Interlocked.Exchange(ref _strName, value) == value)
                        return;
                    using (LockObject.EnterWriteLock())
                    {
                        _objCachedMyXmlNode = null;
                        _objCachedMyXPathNode = null;
                    }

                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        ///     LifestyleQuality's parent lifestyle.
        /// </summary>
        public Lifestyle ParentLifestyle
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                    return _objParentLifestyle;
            }
        }

        /// <summary>
        ///     Extra information that should be applied to the name, like a linked CharacterAttribute.
        /// </summary>
        public string Extra
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                    return _strExtra;
            }
            set
            {
                using (EnterReadLock.Enter(LockObject))
                {
                    if (Interlocked.Exchange(ref _strExtra, value) != value)
                        OnPropertyChanged();
                }
            }
        }

        /// <summary>
        ///     Sourcebook.
        /// </summary>
        public string Source
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                    return _strSource;
            }
            set
            {
                using (EnterReadLock.Enter(LockObject))
                {
                    if (Interlocked.Exchange(ref _strSource, value) != value)
                        OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Sourcebook Page Number.
        /// </summary>
        public string Page
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                    return _strPage;
            }
            set
            {
                using (EnterReadLock.Enter(LockObject))
                {
                    if (Interlocked.Exchange(ref _strPage, value) != value)
                        OnPropertyChanged();
                }
            }
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
            using (EnterReadLock.Enter(LockObject))
            {
                string s = this.GetNodeXPath(strLanguage)?.SelectSingleNodeAndCacheExpression("altpage")?.Value ?? Page;
                return !string.IsNullOrWhiteSpace(s) ? s : Page;
            }
        }

        /// <summary>
        /// Sourcebook Page Number using a given language file.
        /// Returns Page if not found or the string is empty.
        /// </summary>
        /// <param name="strLanguage">Language file keyword to use.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        /// <returns></returns>
        public async ValueTask<string> DisplayPageAsync(string strLanguage, CancellationToken token = default)
        {
            if (strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                return Page;
            using (await EnterReadLock.EnterAsync(LockObject, token).ConfigureAwait(false))
            {
                XPathNavigator objNode = await this.GetNodeXPathAsync(strLanguage, token: token).ConfigureAwait(false);
                string s = objNode != null
                    ? (await objNode.SelectSingleNodeAndCacheExpressionAsync("altpage", token: token)
                                    .ConfigureAwait(false))?.Value ?? Page
                    : Page;
                return !string.IsNullOrWhiteSpace(s) ? s : Page;
            }
        }

        /// <summary>
        ///     Bonus node from the XML file.
        /// </summary>
        public XmlNode Bonus
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                    return _xmlBonus;
            }
            set
            {
                using (EnterReadLock.Enter(LockObject))
                {
                    if (Interlocked.Exchange(ref _xmlBonus, value) != value)
                        OnPropertyChanged();
                }
            }
        }

        /// <summary>
        ///     LifestyleQuality Type.
        /// </summary>
        public QualityType Type
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                    return _eType;
            }
        }

        /// <summary>
        ///     Source of the LifestyleQuality.
        /// </summary>
        public QualitySource OriginSource
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                    return _eOriginSource;
            }
            set
            {
                using (EnterReadLock.Enter(LockObject))
                {
                    if (InterlockedExtensions.Exchange(ref _eOriginSource, value) != value)
                        OnPropertyChanged();
                }
            }
        }

        /// <summary>
        ///     Number of Build Points the LifestyleQuality costs.
        /// </summary>
        public int LP
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                    return Free || !UseLPCost ? 0 : _intLP;
            }
            set
            {
                using (EnterReadLock.Enter(LockObject))
                {
                    if (Interlocked.Exchange(ref _intLP, value) != value)
                        OnPropertyChanged();
                }
            }
        }

        /// <summary>
        ///     The name of the object as it should be displayed on printouts (translated name only).
        /// </summary>
        public string DisplayNameShort(string strLanguage)
        {
            if (strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                return Name;

            using (EnterReadLock.Enter(LockObject))
                return this.GetNodeXPath(strLanguage)?.SelectSingleNodeAndCacheExpression("translate")?.Value ?? Name;
        }

        /// <summary>
        ///     The name of the object as it should be displayed on printouts (translated name only).
        /// </summary>
        public async ValueTask<string> DisplayNameShortAsync(string strLanguage, CancellationToken token = default)
        {
            if (strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                return Name;

            using (await EnterReadLock.EnterAsync(LockObject, token).ConfigureAwait(false))
            {
                XPathNavigator objNode = await this.GetNodeXPathAsync(strLanguage, token: token).ConfigureAwait(false);
                return objNode != null
                    ? (await objNode.SelectSingleNodeAndCacheExpressionAsync("translate", token: token)
                                    .ConfigureAwait(false))?.Value ?? Name
                    : Name;
            }
        }

        public string CurrentDisplayNameShort => DisplayNameShort(GlobalSettings.Language);

        public ValueTask<string> GetCurrentDisplayNameShortAsync(CancellationToken token = default) =>
            DisplayNameShortAsync(GlobalSettings.Language, token);

        /// <summary>
        ///     The name of the object as it should be displayed in lists. Name (Extra).
        /// </summary>
        public string DisplayName(string strLanguage)
        {
            using (EnterReadLock.Enter(LockObject))
            {
                string strReturn = DisplayNameShort(strLanguage);

                if (!string.IsNullOrEmpty(Extra))
                    // Attempt to retrieve the CharacterAttribute name.
                    strReturn += LanguageManager.GetString("String_Space", strLanguage) + '(' +
                                 _objCharacter.TranslateExtra(Extra, strLanguage) + ')';
                return strReturn;
            }
        }

        /// <summary>
        ///     The name of the object as it should be displayed in lists. Name (Extra).
        /// </summary>
        public async ValueTask<string> DisplayNameAsync(string strLanguage, CancellationToken token = default)
        {
            using (await EnterReadLock.EnterAsync(LockObject, token).ConfigureAwait(false))
            {
                string strReturn = await DisplayNameShortAsync(strLanguage, token).ConfigureAwait(false);

                if (!string.IsNullOrEmpty(Extra))
                    // Attempt to retrieve the CharacterAttribute name.
                    strReturn += await LanguageManager.GetStringAsync("String_Space", strLanguage, token: token)
                                                      .ConfigureAwait(false) + '(' +
                                 await _objCharacter.TranslateExtraAsync(Extra, strLanguage, token: token)
                                                    .ConfigureAwait(false) + ')';
                return strReturn;
            }
        }

        public string CurrentDisplayName => DisplayName(GlobalSettings.Language);

        public ValueTask<string> GetCurrentDisplayNameAsync(CancellationToken token = default) => DisplayNameAsync(GlobalSettings.Language, token);

        public string FormattedDisplayName(CultureInfo objCulture, string strLanguage)
        {
            using (EnterReadLock.Enter(LockObject))
            {
                string strReturn = DisplayName(strLanguage);
                string strSpace = LanguageManager.GetString("String_Space", strLanguage);

                if (Multiplier > 0)
                    strReturn += strSpace + "[+" + Multiplier.ToString(objCulture) + "%]";
                else if (Multiplier < 0)
                    strReturn += strSpace + '[' + Multiplier.ToString(objCulture) + "%]";

                if (Cost > 0)
                    strReturn += strSpace + "[+" + Cost.ToString(_objCharacter.Settings.NuyenFormat, objCulture)
                                 + LanguageManager.GetString("String_NuyenSymbol") + ']';
                else if (Cost < 0)
                    strReturn += strSpace + '[' + Cost.ToString(_objCharacter.Settings.NuyenFormat, objCulture)
                                 + LanguageManager.GetString("String_NuyenSymbol") + ']';
                return strReturn;
            }
        }

        public async ValueTask<string> FormattedDisplayNameAsync(CultureInfo objCulture, string strLanguage, CancellationToken token = default)
        {
            using (await EnterReadLock.EnterAsync(LockObject, token).ConfigureAwait(false))
            {
                string strReturn = await DisplayNameAsync(strLanguage, token).ConfigureAwait(false);
                string strSpace = await LanguageManager.GetStringAsync("String_Space", strLanguage, token: token)
                                                       .ConfigureAwait(false);

                if (Multiplier > 0)
                    strReturn += strSpace + "[+" + Multiplier.ToString(objCulture) + "%]";
                else if (Multiplier < 0)
                    strReturn += strSpace + '[' + Multiplier.ToString(objCulture) + "%]";

                if (Cost > 0)
                    strReturn += strSpace + "[+" + Cost.ToString(_objCharacter.Settings.NuyenFormat, objCulture)
                                 + await LanguageManager.GetStringAsync("String_NuyenSymbol", token: token)
                                                        .ConfigureAwait(false) + ']';
                else if (Cost < 0)
                    strReturn += strSpace + '[' + Cost.ToString(_objCharacter.Settings.NuyenFormat, objCulture)
                                 + await LanguageManager.GetStringAsync("String_NuyenSymbol", token: token)
                                                        .ConfigureAwait(false) + ']';
                return strReturn;
            }
        }

        public string CurrentFormattedDisplayName => FormattedDisplayName(GlobalSettings.CultureInfo, GlobalSettings.Language);

        public ValueTask<string> GetCurrentFormattedDisplayNameAsync(CancellationToken token = default) =>
            FormattedDisplayNameAsync(GlobalSettings.CultureInfo, GlobalSettings.Language, token);

        /// <summary>
        ///     Whether or not the LifestyleQuality appears on the printouts.
        /// </summary>
        public bool AllowPrint
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                    return _blnPrint;
            }
            set
            {
                using (EnterReadLock.Enter(LockObject))
                {
                    if (_blnPrint == value)
                        return;
                    using (LockObject.EnterWriteLock())
                        _blnPrint = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        ///     Notes.
        /// </summary>
        public string Notes
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                    return _strNotes;
            }
            set
            {
                using (EnterReadLock.Enter(LockObject))
                {
                    if (Interlocked.Exchange(ref _strNotes, value) != value)
                        OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Forecolor to use for Notes in treeviews.
        /// </summary>
        public Color NotesColor
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                    return _colNotes;
            }
            set
            {
                using (EnterReadLock.Enter(LockObject))
                {
                    if (_colNotes == value)
                        return;
                    using (LockObject.EnterWriteLock())
                        _colNotes = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        ///     Nuyen cost of the Quality.
        /// </summary>
        public decimal Cost
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                {
                    if (Free || UseLPCost)
                        return 0;
                    if (!decimal.TryParse(CostString, NumberStyles.Any, GlobalSettings.InvariantCultureInfo,
                                          out decimal decReturn))
                    {
                        (bool blnIsSuccess, object objProcess) = CommonFunctions.EvaluateInvariantXPath(CostString);
                        if (blnIsSuccess)
                            return Convert.ToDecimal(objProcess, GlobalSettings.InvariantCultureInfo);
                    }

                    return decReturn;
                }
            }
        }

        /// <summary>
        ///     String for the nuyen cost of the Quality.
        /// </summary>
        public string CostString
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                {
                    return string.IsNullOrWhiteSpace(_strCost) ? "0" : _strCost;
                }
            }
            set
            {
                using (EnterReadLock.Enter(LockObject))
                {
                    if (Interlocked.Exchange(ref _strCost, value) != value)
                        OnPropertyChanged();
                }
            }
        }

        /// <summary>
        ///     Does the Quality have a Nuyen or LP cost?
        /// </summary>
        public bool Free
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                    return _blnFree || OriginSource == QualitySource.BuiltIn;
            }
            set
            {
                using (EnterReadLock.Enter(LockObject))
                {
                    if (_blnFree == value)
                        return;
                    using (LockObject.EnterWriteLock())
                        _blnFree = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsFreeGrid
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                    return _blnIsFreeGrid;
            }
            set
            {
                using (EnterReadLock.Enter(LockObject))
                {
                    if (_blnIsFreeGrid == value)
                        return;
                    using (LockObject.EnterWriteLock())
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

                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Whether or not this Quality costs LP.
        /// </summary>
        public bool UseLPCost
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                    return CanBeFreeByLifestyle && _blnUseLPCost;
            }
            set
            {
                using (EnterReadLock.Enter(LockObject))
                {
                    if (!value && !CanBeFreeByLifestyle)
                        return;
                    if (_blnUseLPCost == value)
                        return;
                    using (LockObject.EnterWriteLock())
                        _blnUseLPCost = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Can this Quality have no nuyen costs based on the base lifestyle?
        /// </summary>
        public bool CanBeFreeByLifestyle
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
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
        }

        /// <summary>
        ///     Comforts LP is increased/reduced by this Quality.
        /// </summary>
        public int Comforts
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                    return _intComforts;
            }
            set
            {
                using (EnterReadLock.Enter(LockObject))
                {
                    if (Interlocked.Exchange(ref _intComforts, value) != value)
                        OnPropertyChanged();
                }
            }
        }

        /// <summary>
        ///     Comforts LP maximum is increased/reduced by this Quality.
        /// </summary>
        public int ComfortsMaximum
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                    return _intComfortsMaximum;
            }
            set
            {
                using (EnterReadLock.Enter(LockObject))
                {
                    if (Interlocked.Exchange(ref _intComfortsMaximum, value) != value)
                        OnPropertyChanged();
                }
            }
        }

        /// <summary>
        ///     Security LP value is increased/reduced by this Quality.
        /// </summary>
        public int SecurityMaximum
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                    return _intSecurityMaximum;
            }
            set
            {
                using (EnterReadLock.Enter(LockObject))
                {
                    if (Interlocked.Exchange(ref _intSecurityMaximum, value) != value)
                        OnPropertyChanged();
                }
            }
        }

        /// <summary>
        ///     Security LP value is increased/reduced by this Quality.
        /// </summary>
        public int Security
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                    return _intSecurity;
            }
            set
            {
                using (EnterReadLock.Enter(LockObject))
                {
                    if (Interlocked.Exchange(ref _intSecurity, value) != value)
                        OnPropertyChanged();
                }
            }
        }

        /// <summary>
        ///     Percentage by which the quality increases the overall Lifestyle Cost.
        /// </summary>
        public int Multiplier
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                    return Free || UseLPCost ? 0 : _intMultiplier;
            }
            set
            {
                using (EnterReadLock.Enter(LockObject))
                {
                    if (Interlocked.Exchange(ref _intMultiplier, value) != value)
                        OnPropertyChanged();
                }
            }
        }

        /// <summary>
        ///     Percentage by which the quality increases the Lifestyle Cost ONLY, without affecting other qualities.
        /// </summary>
        public int BaseMultiplier
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                    return Free || UseLPCost ? 0 : _intBaseMultiplier;
            }
            set
            {
                using (EnterReadLock.Enter(LockObject))
                {
                    if (Interlocked.Exchange(ref _intBaseMultiplier, value) != value)
                        OnPropertyChanged();
                }
            }
        }

        /// <summary>
        ///     Category of the Quality.
        /// </summary>
        public string Category
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                    return _strCategory;
            }
            set
            {
                using (EnterReadLock.Enter(LockObject))
                {
                    if (Interlocked.Exchange(ref _strCategory, value) != value)
                        OnPropertyChanged();
                }
            }
        }

        /// <summary>
        ///     Area/Neighborhood LP Cost/Benefit of the Quality.
        /// </summary>
        public int AreaMaximum
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                    return _intAreaMaximum;
            }
            set
            {
                using (EnterReadLock.Enter(LockObject))
                {
                    if (Interlocked.Exchange(ref _intAreaMaximum, value) != value)
                        OnPropertyChanged();
                }
            }
        }

        /// <summary>
        ///     Area/Neighborhood minimum is increased/reduced by this Quality.
        /// </summary>
        public int Area
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                    return _intArea;
            }
            set
            {
                using (EnterReadLock.Enter(LockObject))
                {
                    if (Interlocked.Exchange(ref _intArea, value) != value)
                        OnPropertyChanged();
                }
            }
        }

        public async Task<XmlNode> GetNodeCoreAsync(bool blnSync, string strLanguage, CancellationToken token = default)
        {
            using (await EnterReadLock.EnterAsync(LockObject, token).ConfigureAwait(false))
            {
                if (_objCachedMyXmlNode != null && strLanguage == _strCachedXmlNodeLanguage
                                                && !GlobalSettings.LiveCustomData)
                    return _objCachedMyXmlNode;
                _objCachedMyXmlNode = (blnSync
                        // ReSharper disable once MethodHasAsyncOverload
                        ? _objCharacter.LoadData("lifestyles.xml", strLanguage, token: token)
                        : await _objCharacter.LoadDataAsync("lifestyles.xml", strLanguage, token: token)
                                             .ConfigureAwait(false))
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
        }

        private XPathNavigator _objCachedMyXPathNode;
        private string _strCachedXPathNodeLanguage = string.Empty;
        private QualitySource _eOriginSource = QualitySource.Selected;
        private XmlNode _xmlBonus;
        private QualityType _eType = QualityType.Positive;
        private Lifestyle _objParentLifestyle;

        public async Task<XPathNavigator> GetNodeXPathCoreAsync(bool blnSync, string strLanguage, CancellationToken token = default)
        {
            using (await EnterReadLock.EnterAsync(LockObject, token).ConfigureAwait(false))
            {
                if (_objCachedMyXPathNode != null && strLanguage == _strCachedXPathNodeLanguage
                                                  && !GlobalSettings.LiveCustomData)
                    return _objCachedMyXPathNode;
                _objCachedMyXPathNode = (blnSync
                        // ReSharper disable once MethodHasAsyncOverload
                        ? _objCharacter.LoadDataXPath("lifestyles.xml", strLanguage, token: token)
                        : await _objCharacter.LoadDataXPathAsync("lifestyles.xml", strLanguage, token: token)
                                             .ConfigureAwait(false))
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
        }

        #endregion Properties

        #region UI Methods

        public TreeNode CreateTreeNode()
        {
            using (EnterReadLock.Enter(LockObject))
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
        }

        public Color PreferredColor
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
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
        }

        #endregion UI Methods

        public void SetSourceDetail(Control sourceControl)
        {
            using (EnterReadLock.Enter(LockObject))
            {
                if (_objCachedSourceDetail.Language != GlobalSettings.Language)
                    _objCachedSourceDetail = default;
                SourceDetail.SetControl(sourceControl);
            }
        }

        public async Task SetSourceDetailAsync(Control sourceControl, CancellationToken token = default)
        {
            using (await EnterReadLock.EnterAsync(LockObject, token).ConfigureAwait(false))
            {
                if (_objCachedSourceDetail.Language != GlobalSettings.Language)
                    _objCachedSourceDetail = default;
                await SourceDetail.SetControlAsync(sourceControl, token).ConfigureAwait(false);
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            using (LockObject.EnterWriteLock())
                Utils.StringHashSetPool.Return(ref _setAllowedFreeLifestyles);
            LockObject.Dispose();
        }

        /// <inheritdoc />
        public async ValueTask DisposeAsync()
        {
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync().ConfigureAwait(false);
            try
            {
                Utils.StringHashSetPool.Return(ref _setAllowedFreeLifestyles);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public bool Remove(bool blnConfirmDelete = true)
        {
            if (blnConfirmDelete && !CommonFunctions.ConfirmDelete(LanguageManager.GetString("Message_DeleteComplexForm")))
                return false;

            ImprovementManager.RemoveImprovements(_objCharacter, Improvement.ImprovementSource.Quality, InternalId);

            bool blnReturn = ParentLifestyle.LifestyleQualities.Remove(this);
            Dispose();
            return blnReturn;
        }

        public async ValueTask<bool> RemoveAsync(bool blnConfirmDelete = true, CancellationToken token = default)
        {
            if (blnConfirmDelete && !await CommonFunctions
                                           .ConfirmDeleteAsync(
                                               await LanguageManager
                                                     .GetStringAsync("Message_DeleteComplexForm", token: token)
                                                     .ConfigureAwait(false), token).ConfigureAwait(false))
                return false;

            await ImprovementManager
                  .RemoveImprovementsAsync(_objCharacter, Improvement.ImprovementSource.Quality, InternalId, token)
                  .ConfigureAwait(false);

            bool blnReturn = await ParentLifestyle.LifestyleQualities.RemoveAsync(this, token).ConfigureAwait(false);
            await DisposeAsync().ConfigureAwait(false);
            return blnReturn;
        }

        private static readonly PropertyDependencyGraph<LifestyleQuality> s_LifestyleQualityDependencyGraph =
            new PropertyDependencyGraph<LifestyleQuality>(
                new DependencyGraphNode<string, LifestyleQuality>(nameof(CurrentDisplayName),
                    new DependencyGraphNode<string, LifestyleQuality>(nameof(DisplayName),
                        new DependencyGraphNode<string, LifestyleQuality>(nameof(DisplayNameShort),
                            new DependencyGraphNode<string, LifestyleQuality>(nameof(Name))
                        ),
                        new DependencyGraphNode<string, LifestyleQuality>(nameof(Extra))
                    )
                ),
                new DependencyGraphNode<string, LifestyleQuality>(nameof(CurrentDisplayNameShort),
                    new DependencyGraphNode<string, LifestyleQuality>(nameof(DisplayNameShort))
                ),
                new DependencyGraphNode<string, LifestyleQuality>(nameof(CurrentFormattedDisplayName),
                    new DependencyGraphNode<string, LifestyleQuality>(nameof(FormattedDisplayName),
                        new DependencyGraphNode<string, LifestyleQuality>(nameof(DisplayName)),
                        new DependencyGraphNode<string, LifestyleQuality>(nameof(Cost),
                            new DependencyGraphNode<string, LifestyleQuality>(nameof(Free),
                                new DependencyGraphNode<string, LifestyleQuality>(nameof(OriginSource), x => !x._blnFree)
                            ),
                            new DependencyGraphNode<string, LifestyleQuality>(nameof(UseLPCost), x => !x.Free),
                            new DependencyGraphNode<string, LifestyleQuality>(nameof(CostString), x => !x.Free && !x.UseLPCost)
                        ),
                        new DependencyGraphNode<string, LifestyleQuality>(nameof(Multiplier),
                            new DependencyGraphNode<string, LifestyleQuality>(nameof(Free)),
                            new DependencyGraphNode<string, LifestyleQuality>(nameof(UseLPCost), x => !x.Free)
                        )
                    )
                ),
                new DependencyGraphNode<string, LifestyleQuality>(nameof(UseLPCost),
                    new DependencyGraphNode<string, LifestyleQuality>(nameof(CanBeFreeByLifestyle))
                ),
                new DependencyGraphNode<string, LifestyleQuality>(nameof(LP),
                    new DependencyGraphNode<string, LifestyleQuality>(nameof(Free)),
                    new DependencyGraphNode<string, LifestyleQuality>(nameof(UseLPCost), x => !x.Free)
                ),
                new DependencyGraphNode<string, LifestyleQuality>(nameof(BaseMultiplier),
                    new DependencyGraphNode<string, LifestyleQuality>(nameof(Free)),
                    new DependencyGraphNode<string, LifestyleQuality>(nameof(UseLPCost), x => !x.Free)
                )
            );

        /// <inheritdoc />
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        public void OnPropertyChanged([CallerMemberName] string strPropertyName = null)
        {
            this.OnMultiplePropertyChanged(strPropertyName);
        }

        /// <inheritdoc />
        public void OnMultiplePropertyChanged(IReadOnlyCollection<string> lstPropertyNames)
        {
            using (EnterReadLock.Enter(LockObject))
            {
                HashSet<string> setNamesOfChangedProperties = null;
                try
                {
                    foreach (string strPropertyName in lstPropertyNames)
                    {
                        if (setNamesOfChangedProperties == null)
                            setNamesOfChangedProperties
                                = s_LifestyleQualityDependencyGraph.GetWithAllDependents(this, strPropertyName, true);
                        else
                        {
                            foreach (string strLoopChangedProperty in s_LifestyleQualityDependencyGraph
                                         .GetWithAllDependentsEnumerable(this, strPropertyName))
                                setNamesOfChangedProperties.Add(strLoopChangedProperty);
                        }
                    }

                    if (setNamesOfChangedProperties == null || setNamesOfChangedProperties.Count == 0)
                        return;

                    if (ParentLifestyle != null)
                    {
                        using (new FetchSafelyFromPool<HashSet<string>>(Utils.StringHashSetPool,
                                                                        out HashSet<string>
                                                                            setParentLifestyleNamesOfChangedProperties))
                        {
                            if (setNamesOfChangedProperties.Contains(nameof(LP))
                                && (!Free || setNamesOfChangedProperties.Contains(nameof(Free)))
                                && (UseLPCost || setNamesOfChangedProperties.Contains(nameof(UseLPCost))))
                            {
                                setParentLifestyleNamesOfChangedProperties.Add(nameof(Lifestyle.TotalLP));
                            }

                            if (setNamesOfChangedProperties.Contains(nameof(Free)))
                            {
                                if (UseLPCost || setNamesOfChangedProperties.Contains(nameof(UseLPCost)))
                                {
                                    setParentLifestyleNamesOfChangedProperties.Add(nameof(Lifestyle.TotalLP));
                                    if (!CanBeFreeByLifestyle
                                        || setNamesOfChangedProperties.Contains(nameof(CanBeFreeByLifestyle)))
                                    {
                                        setParentLifestyleNamesOfChangedProperties.Add(
                                            nameof(Lifestyle.TotalMonthlyCost));
                                        setParentLifestyleNamesOfChangedProperties.Add(
                                            nameof(Lifestyle.CostMultiplier));
                                        setParentLifestyleNamesOfChangedProperties.Add(
                                            nameof(Lifestyle.BaseCostMultiplier));
                                    }
                                }
                                else
                                {
                                    setParentLifestyleNamesOfChangedProperties.Add(nameof(Lifestyle.TotalMonthlyCost));
                                    setParentLifestyleNamesOfChangedProperties.Add(nameof(Lifestyle.CostMultiplier));
                                    setParentLifestyleNamesOfChangedProperties.Add(
                                        nameof(Lifestyle.BaseCostMultiplier));
                                }
                            }

                            if (setNamesOfChangedProperties.Contains(nameof(Cost))
                                && (!Free || setNamesOfChangedProperties.Contains(nameof(Free)))
                                && (!UseLPCost || setNamesOfChangedProperties.Contains(nameof(UseLPCost))))
                            {
                                setParentLifestyleNamesOfChangedProperties.Add(nameof(Lifestyle.TotalMonthlyCost));
                            }

                            if (setNamesOfChangedProperties.Contains(nameof(UseLPCost))
                                && (!Free || setNamesOfChangedProperties.Contains(nameof(Free))))
                            {
                                setParentLifestyleNamesOfChangedProperties.Add(nameof(Lifestyle.TotalLP));
                                setParentLifestyleNamesOfChangedProperties.Add(nameof(Lifestyle.TotalMonthlyCost));
                                setParentLifestyleNamesOfChangedProperties.Add(nameof(Lifestyle.CostMultiplier));
                                setParentLifestyleNamesOfChangedProperties.Add(nameof(Lifestyle.BaseCostMultiplier));
                            }

                            if (setNamesOfChangedProperties.Contains(nameof(IsFreeGrid)))
                                setParentLifestyleNamesOfChangedProperties.Add(nameof(Lifestyle.LifestyleQualities));
                            if (setNamesOfChangedProperties.Contains(nameof(Multiplier))
                                && (!Free || setNamesOfChangedProperties.Contains(nameof(Free)))
                                && (!UseLPCost || setNamesOfChangedProperties.Contains(nameof(UseLPCost))))
                            {
                                setParentLifestyleNamesOfChangedProperties.Add(nameof(Lifestyle.CostMultiplier));
                            }

                            if (setNamesOfChangedProperties.Contains(nameof(BaseMultiplier))
                                && (!Free || setNamesOfChangedProperties.Contains(nameof(Free)))
                                && (!UseLPCost || setNamesOfChangedProperties.Contains(nameof(UseLPCost))))
                            {
                                setParentLifestyleNamesOfChangedProperties.Add(nameof(Lifestyle.BaseCostMultiplier));
                            }

                            if (setNamesOfChangedProperties.Contains(nameof(ComfortsMaximum)))
                                setParentLifestyleNamesOfChangedProperties.Add(nameof(Lifestyle.TotalComfortsMaximum));
                            if (setNamesOfChangedProperties.Contains(nameof(Comforts)))
                            {
                                setParentLifestyleNamesOfChangedProperties.Add(nameof(Lifestyle.TotalComforts));
                                setParentLifestyleNamesOfChangedProperties.Add(nameof(Lifestyle.ComfortsDelta));
                            }

                            if (setNamesOfChangedProperties.Contains(nameof(SecurityMaximum)))
                                setParentLifestyleNamesOfChangedProperties.Add(nameof(Lifestyle.TotalSecurityMaximum));
                            if (setNamesOfChangedProperties.Contains(nameof(Security)))
                            {
                                setParentLifestyleNamesOfChangedProperties.Add(nameof(Lifestyle.TotalSecurity));
                                setParentLifestyleNamesOfChangedProperties.Add(nameof(Lifestyle.SecurityDelta));
                            }

                            if (setNamesOfChangedProperties.Contains(nameof(AreaMaximum)))
                                setParentLifestyleNamesOfChangedProperties.Add(nameof(Lifestyle.TotalAreaMaximum));
                            if (setNamesOfChangedProperties.Contains(nameof(Area)))
                            {
                                setParentLifestyleNamesOfChangedProperties.Add(nameof(Lifestyle.TotalArea));
                                setParentLifestyleNamesOfChangedProperties.Add(nameof(Lifestyle.AreaDelta));
                            }

                            if (setParentLifestyleNamesOfChangedProperties.Count > 0)
                                ParentLifestyle.OnMultiplePropertyChanged(setParentLifestyleNamesOfChangedProperties);
                        }
                    }

                    if (PropertyChanged != null)
                    {
                        Utils.RunOnMainThread(() =>
                        {
                            if (PropertyChanged != null)
                            {
                                // ReSharper disable once AccessToModifiedClosure
                                foreach (string strPropertyToChange in setNamesOfChangedProperties)
                                {
                                    PropertyChanged.Invoke(this, new PropertyChangedEventArgs(strPropertyToChange));
                                }
                            }
                        });
                    }
                }
                finally
                {
                    if (setNamesOfChangedProperties != null)
                        Utils.StringHashSetPool.Return(ref setNamesOfChangedProperties);
                }
            }
        }

        /// <inheritdoc />
        public AsyncFriendlyReaderWriterLock LockObject { get; } = new AsyncFriendlyReaderWriterLock();
    }
}
