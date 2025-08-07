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
using System.Collections.Specialized;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;
using NLog;

namespace Chummer.Backend.Equipment
{
    public sealed class Drug : IHasName, IHasSourceId, IHasXmlDataNode, ICanSort, IHasStolenProperty, ICanRemove, IDisposable, IAsyncDisposable, IHasCharacterObject, IHasNotes
    {
        private static readonly Lazy<Logger> s_ObjLogger = new Lazy<Logger>(LogManager.GetCurrentClassLogger);
        private static Logger Log => s_ObjLogger.Value;
        private Guid _guiSourceID = Guid.Empty;
        private Guid _guiID;
        private string _strName = string.Empty;
        private string _strCategory = string.Empty;
        private string _strAvailability = "0";
        private string _strDuration;
        private string _strDescription = string.Empty;
        private string _strEffectDescription = string.Empty;
        private readonly Dictionary<string, decimal> _dicCachedAttributes = new Dictionary<string, decimal>();
        private readonly List<string> _lstCachedInfos = new List<string>();
        private readonly Dictionary<string, int> _dicCachedLimits = new Dictionary<string, int>();
        private readonly List<XmlNode> _lstCachedQualities = new List<XmlNode>();
        private string _strGrade = string.Empty;
        private decimal _decCost;
        private int _intAddictionThreshold;
        private int _intAddictionRating;
        private const int _intSpeed = 9;
        private decimal _decQty;
        private int _intSortOrder;
        private readonly Character _objCharacter;
        private bool _blnStolen;
        private bool _blnCachedAttributeFlag;
        private XmlNode _objCachedMyXmlNode;
        private string _strCachedXmlNodeLanguage;
        private string _strSource;
        private string _strPage;
        private int _intDurationDice;
        private string _strNotes = string.Empty;
        private Color _colNotes = ColorManager.HasNotesColor;

        #region Constructor, Create, Save, Load, and Print Methods

        public Drug(Character objCharacter)
        {
            _objCharacter = objCharacter;
            // Create the GUID for the new Drug.
            _guiID = Guid.NewGuid();
            _lstComponents = new ThreadSafeObservableCollection<DrugComponent>(objCharacter.LockObject);
            Components.CollectionChanged += ComponentsChanged;
        }

        private void ComponentsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            _intCachedCrashDamage = int.MinValue;
            _intCachedDuration = int.MinValue;
            _intCachedInitiative = int.MinValue;
            _intCachedInitiativeDice = int.MinValue;
            _intCachedSpeed = int.MinValue;
            _blnCachedQualityFlag = false;
            _blnCachedLimitFlag = false;
            _blnCachedAttributeFlag = false;
            _strDescription = string.Empty;
        }

        public void Create(XmlNode objXmlData)
        {
            objXmlData.TryGetField("guid", Guid.TryParse, out _guiID);
            objXmlData.TryGetStringFieldQuickly("name", ref _strName);
            _objCachedMyXmlNode = null;
            _objCachedMyXPathNode = null;
            objXmlData.TryGetStringFieldQuickly("category", ref _strCategory);
            if (objXmlData["sourceid"] == null || !objXmlData.TryGetField("sourceid", Guid.TryParse, out _guiSourceID))
            {
                this.GetNodeXPath()?.TryGetField("id", Guid.TryParse, out _guiSourceID);
            }
            objXmlData.TryGetStringFieldQuickly("availability", ref _strAvailability);
            objXmlData.TryGetDecFieldQuickly("cost", ref _decCost);
            objXmlData.TryGetDecFieldQuickly("quantity", ref _decQty);
            objXmlData.TryGetInt32FieldQuickly("rating", ref _intAddictionRating);
            objXmlData.TryGetInt32FieldQuickly("threshold", ref _intAddictionThreshold);
            objXmlData.TryGetStringFieldQuickly("grade", ref _strGrade);
            objXmlData.TryGetInt32FieldQuickly("sortorder", ref _intSortOrder);
            objXmlData.TryGetBoolFieldQuickly("stolen", ref _blnStolen);
            objXmlData.TryGetStringFieldQuickly("duration", ref _strDuration);
            objXmlData.TryGetInt32FieldQuickly("durationdice", ref _intDurationDice);
            DurationTimescale = CommonFunctions.ConvertStringToTimescale(objXmlData["timescale"]?.InnerText);

            objXmlData.TryGetField("source", out _strSource);
            objXmlData.TryGetField("page", out _strPage);
            if (!objXmlData.TryGetMultiLineStringFieldQuickly("altnotes", ref _strNotes))
                objXmlData.TryGetMultiLineStringFieldQuickly("notes", ref _strNotes);

            string sNotesColor = ColorTranslator.ToHtml(ColorManager.HasNotesColor);
            objXmlData.TryGetStringFieldQuickly("notesColor", ref sNotesColor);
            _colNotes = ColorTranslator.FromHtml(sNotesColor);
        }

        public void Load(XmlNode objXmlData)
        {
            objXmlData.TryGetStringFieldQuickly("name", ref _strName);
            _objCachedMyXmlNode = null;
            _objCachedMyXPathNode = null;
            if (!objXmlData.TryGetGuidFieldQuickly("sourceid", ref _guiSourceID))
            {
                this.GetNodeXPath()?.TryGetGuidFieldQuickly("id", ref _guiSourceID);
            }
            objXmlData.TryGetStringFieldQuickly("category", ref _strCategory);
            Grade = Grade.ConvertToCyberwareGrade(objXmlData["grade"]?.InnerText, Improvement.ImprovementSource.Drug, _objCharacter);

            XmlNodeList xmlComponentsNodeList = objXmlData.SelectNodes("drugcomponents/drugcomponent");
            if (xmlComponentsNodeList?.Count > 0)
            {
                foreach (XmlNode objXmlLevel in xmlComponentsNodeList)
                {
                    DrugComponent c = new DrugComponent(_objCharacter);
                    c.Load(objXmlLevel);
                    Components.Add(c);
                }
            }

            objXmlData.TryGetStringFieldQuickly("availability", ref _strAvailability);
            objXmlData.TryGetDecFieldQuickly("cost", ref _decCost);
            objXmlData.TryGetDecFieldQuickly("quantity", ref _decQty);
            objXmlData.TryGetInt32FieldQuickly("rating", ref _intAddictionRating);
            objXmlData.TryGetInt32FieldQuickly("threshold", ref _intAddictionThreshold);
            objXmlData.TryGetStringFieldQuickly("grade", ref _strGrade);
            objXmlData.TryGetInt32FieldQuickly("sortorder", ref _intSortOrder);
            objXmlData.TryGetBoolFieldQuickly("stolen", ref _blnStolen);
            objXmlData.TryGetField("source", out _strSource);
            objXmlData.TryGetField("page", out _strPage);
            objXmlData.TryGetMultiLineStringFieldQuickly("notes", ref _strNotes);

            string sNotesColor = ColorTranslator.ToHtml(ColorManager.HasNotesColor);
            objXmlData.TryGetStringFieldQuickly("notesColor", ref sNotesColor);
            _colNotes = ColorTranslator.FromHtml(sNotesColor);
        }

        public void Save(XmlWriter objXmlWriter)
        {
            if (objXmlWriter == null)
                return;
            objXmlWriter.WriteStartElement("drug");
            objXmlWriter.WriteElementString("sourceid", SourceIDString);
            objXmlWriter.WriteElementString("guid", InternalId);
            objXmlWriter.WriteElementString("name", _strName);
            objXmlWriter.WriteElementString("category", _strCategory);
            objXmlWriter.WriteElementString("quantity", _decQty.ToString(GlobalSettings.InvariantCultureInfo));
            objXmlWriter.WriteStartElement("drugcomponents");
            foreach (DrugComponent objDrugComponent in Components)
            {
                objXmlWriter.WriteStartElement("drugcomponent");
                objDrugComponent.Save(objXmlWriter);
                objXmlWriter.WriteEndElement();
            }
            objXmlWriter.WriteEndElement();
            objXmlWriter.WriteElementString("availability", _strAvailability);
            if (_decCost != 0)
                objXmlWriter.WriteElementString("cost", _decCost.ToString(GlobalSettings.InvariantCultureInfo));
            if (_intAddictionRating != 0)
                objXmlWriter.WriteElementString("rating", _intAddictionRating.ToString(GlobalSettings.InvariantCultureInfo));
            if (_intAddictionThreshold != 0)
                objXmlWriter.WriteElementString("threshold", _intAddictionThreshold.ToString(GlobalSettings.InvariantCultureInfo));
            if (Grade != null)
                objXmlWriter.WriteElementString("grade", Grade.Name);
            objXmlWriter.WriteElementString("sortorder", _intSortOrder.ToString(GlobalSettings.InvariantCultureInfo));
            objXmlWriter.WriteElementString("stolen", _blnStolen.ToString(GlobalSettings.InvariantCultureInfo));
            objXmlWriter.WriteElementString("source", _strSource);
            objXmlWriter.WriteElementString("page", _strPage);
            objXmlWriter.WriteElementString("notes", _strNotes.CleanOfXmlInvalidUnicodeChars());
            objXmlWriter.WriteElementString("notesColor", ColorTranslator.ToHtml(_colNotes));
            objXmlWriter.WriteEndElement();
        }

        /// <summary>
        /// Print the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        /// <param name="objCulture">Culture in which to print.</param>
        /// <param name="strLanguageToPrint">Language in which to print</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public async Task Print(XmlWriter objWriter, CultureInfo objCulture, string strLanguageToPrint, CancellationToken token = default)
        {
            if (objWriter == null)
                return;
            // <drug>
            XmlElementWriteHelper objBaseElement = await objWriter.StartElementAsync("drug", token).ConfigureAwait(false);
            try
            {
                await objWriter.WriteElementStringAsync("guid", InternalId, token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("sourceid", SourceIDString, token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("name", await DisplayNameShortAsync(strLanguageToPrint, token).ConfigureAwait(false), token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("name_english", Name, token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("category", await DisplayCategoryAsync(strLanguageToPrint, token).ConfigureAwait(false), token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("category_english", Category, token).ConfigureAwait(false);
                if (Grade != null)
                    await objWriter.WriteElementStringAsync("grade", await Grade.DisplayNameAsync(strLanguageToPrint, token).ConfigureAwait(false), token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("qty", Quantity.ToString("#,0.##", objCulture), token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("addictionthreshold", (await GetAddictionThresholdAsync(token).ConfigureAwait(false)).ToString(objCulture), token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("addictionrating", (await GetAddictionRatingAsync(token).ConfigureAwait(false)).ToString(objCulture), token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("initiative", (await GetInitiativeAsync(token).ConfigureAwait(false)).ToString(objCulture), token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("initiativedice", (await GetInitiativeDiceAsync(token).ConfigureAwait(false)).ToString(objCulture), token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("speed", (await GetSpeedAsync(token).ConfigureAwait(false)).ToString(objCulture), token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("duration", await GetDisplayDurationAsync(objCulture, strLanguageToPrint, token).ConfigureAwait(false), token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("duration_english", await GetDisplayDurationAsync(GlobalSettings.CultureInfo, GlobalSettings.DefaultLanguage, token).ConfigureAwait(false), token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("crashdamage", (await GetCrashDamageAsync(token).ConfigureAwait(false)).ToString(objCulture), token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync(
                    "avail", await TotalAvailAsync(GlobalSettings.CultureInfo, strLanguageToPrint, token).ConfigureAwait(false), token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("avail_english",
                                                        await TotalAvailAsync(GlobalSettings.CultureInfo,
                                                                              GlobalSettings.DefaultLanguage, token).ConfigureAwait(false), token).ConfigureAwait(false);
                string strNuyenFormat = await (await _objCharacter.GetSettingsAsync(token).ConfigureAwait(false)).GetNuyenFormatAsync(token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync(
                    "cost", (await GetTotalCostAsync(token).ConfigureAwait(false)).ToString(strNuyenFormat, objCulture), token).ConfigureAwait(false);

                // <attributes>
                XmlElementWriteHelper objAttributesElement = await objWriter.StartElementAsync("attributes", token).ConfigureAwait(false);
                try
                {
                    foreach (KeyValuePair<string, decimal> objAttribute in await GetAttributesAsync(token).ConfigureAwait(false))
                    {
                        if (objAttribute.Value != 0)
                        {
                            // <attribute>
                            XmlElementWriteHelper objAttributeElement = await objWriter.StartElementAsync("attribute", token).ConfigureAwait(false);
                            try
                            {
                                await objWriter.WriteElementStringAsync(
                                    "name",
                                    await LanguageManager.GetStringAsync(
                                        "String_Attribute" + objAttribute.Key + "Short",
                                        strLanguageToPrint, token: token).ConfigureAwait(false), token).ConfigureAwait(false);
                                await objWriter.WriteElementStringAsync("name_english", objAttribute.Key, token).ConfigureAwait(false);
                                await objWriter.WriteElementStringAsync(
                                    "value", objAttribute.Value.ToString("+#.#;-#.#", objCulture), token).ConfigureAwait(false);
                            }
                            finally
                            {
                                // </attribute>
                                await objAttributeElement.DisposeAsync().ConfigureAwait(false);
                            }
                        }
                    }
                }
                finally
                {
                    // </attributes>
                    await objAttributesElement.DisposeAsync().ConfigureAwait(false);
                }

                // <limits>
                XmlElementWriteHelper objLimitsElement = await objWriter.StartElementAsync("limits", token).ConfigureAwait(false);
                try
                {
                    foreach (KeyValuePair<string, int> objLimit in await GetLimitsAsync(token).ConfigureAwait(false))
                    {
                        if (objLimit.Value != 0)
                        {
                            // <limit>
                            XmlElementWriteHelper objLimitElement = await objWriter.StartElementAsync("limit", token).ConfigureAwait(false);
                            try
                            {
                                await objWriter.WriteElementStringAsync(
                                    "name",
                                    await LanguageManager.GetStringAsync("Node_" + objLimit.Key, strLanguageToPrint, token: token).ConfigureAwait(false), token).ConfigureAwait(false);
                                await objWriter.WriteElementStringAsync("name_english", objLimit.Key, token).ConfigureAwait(false);
                                await objWriter.WriteElementStringAsync(
                                    "value", objLimit.Value.ToString("+#;-#", objCulture), token).ConfigureAwait(false);
                            }
                            finally
                            {
                                // </limit>
                                await objLimitElement.DisposeAsync().ConfigureAwait(false);
                            }
                        }
                    }
                }
                finally
                {
                    // </limits>
                    await objLimitsElement.DisposeAsync().ConfigureAwait(false);
                }

                // <qualities>
                XmlElementWriteHelper objQualitiesElement = await objWriter.StartElementAsync("qualities", token).ConfigureAwait(false);
                try
                {
                    foreach (string strQualityText in (await GetQualitiesAsync(token).ConfigureAwait(false)).Select(x => x.InnerText))
                    {
                        // <quality>
                        XmlElementWriteHelper objQualityElement = await objWriter.StartElementAsync("quality", token).ConfigureAwait(false);
                        try
                        {
                            await objWriter.WriteElementStringAsync(
                                "name", await _objCharacter.TranslateExtraAsync(strQualityText, strLanguageToPrint, token: token).ConfigureAwait(false), token).ConfigureAwait(false);
                            await objWriter.WriteElementStringAsync("name_english", strQualityText, token).ConfigureAwait(false);
                        }
                        finally
                        {
                            // </quality>
                            await objQualityElement.DisposeAsync().ConfigureAwait(false);
                        }
                    }
                }
                finally
                {
                    // </qualities>
                    await objQualitiesElement.DisposeAsync().ConfigureAwait(false);
                }

                // <infos>
                XmlElementWriteHelper objInfosElement = await objWriter.StartElementAsync("infos", token).ConfigureAwait(false);
                try
                {
                    foreach (string strInfo in await GetInfosAsync(token).ConfigureAwait(false))
                    {
                        // <info>
                        XmlElementWriteHelper objInfoElement = await objWriter.StartElementAsync("info", token).ConfigureAwait(false);
                        try
                        {
                            await objWriter.WriteElementStringAsync(
                                "name", await _objCharacter.TranslateExtraAsync(strInfo, strLanguageToPrint, token: token).ConfigureAwait(false), token).ConfigureAwait(false);
                            await objWriter.WriteElementStringAsync("name_english", strInfo, token).ConfigureAwait(false);
                        }
                        finally
                        {
                            // </info>
                            await objInfoElement.DisposeAsync().ConfigureAwait(false);
                        }
                    }
                }
                finally
                {
                    // </infos>
                    await objInfosElement.DisposeAsync().ConfigureAwait(false);
                }

                if (GlobalSettings.PrintNotes)
                    await objWriter.WriteElementStringAsync("notes", await GetNotesAsync(token).ConfigureAwait(false), token).ConfigureAwait(false);
            }
            finally
            {
                // </drug>
                await objBaseElement.DisposeAsync().ConfigureAwait(false);
            }
        }

        #endregion Constructor, Create, Save, Load, and Print Methods

        #region Properties

        /// <summary>
        /// Internal identifier which will be used to identify this item.
        /// </summary>
        public string InternalId => _guiID.ToString();

        /// <summary>
        /// Grade level of the Cyberware.
        /// </summary>
        public Grade Grade { get; set; }

        /// <summary>
        /// Compiled description of the drug.
        /// </summary>
        public string Description
        {
            get
            {
                if (string.IsNullOrEmpty(_strDescription))
                    _strDescription = GenerateDescription(0);
                return _strDescription;
            }
            set => _strDescription = value;
        }

        /// <summary>
        /// Compiled description of the drug.
        /// </summary>
        public async Task<string> GetDescriptionAsync(CancellationToken token = default)
        {
            if (string.IsNullOrEmpty(_strDescription))
                _strDescription = await GenerateDescriptionAsync(0, token: token).ConfigureAwait(false);
            return _strDescription;
        }

        /// <summary>
        /// Compiled description of the drug's Effects.
        /// </summary>
        public string EffectDescription
        {
            get
            {
                if (string.IsNullOrEmpty(_strEffectDescription))
                    _strEffectDescription = GenerateDescription(0, true);
                return _strEffectDescription;
            }
            set => _strEffectDescription = value;
        }

        /// <summary>
        /// Compiled description of the drug's Effects.
        /// </summary>
        public async Task<string> GetEffectDescriptionAsync(CancellationToken token = default)
        {
            if (string.IsNullOrEmpty(_strDescription))
                _strEffectDescription = await GenerateDescriptionAsync(0, true, token: token).ConfigureAwait(false);
            return _strEffectDescription;
        }

        /// <summary>
        /// Components of the Drug.
        /// </summary>
        public ThreadSafeObservableCollection<DrugComponent> Components => _lstComponents;

        /// <summary>
        /// Name of the Drug.
        /// </summary>
        public string Name
        {
            get => _strName;
            set => _strName = _objCharacter.ReverseTranslateExtra(value);
        }

        /// <summary>
        /// Translated Category.
        /// </summary>
        public string DisplayCategory(string strLanguage)
        {
            if (strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                return Category;

            return _objCharacter.LoadDataXPath("gear.xml")
                                .SelectSingleNodeAndCacheExpression(
                                    "/chummer/categories/category[. = " + Category.CleanXPath() + "]/@translate")?.Value
                   ?? Category;
        }

        /// <summary>
        /// Translated Category.
        /// </summary>
        public async Task<string> DisplayCategoryAsync(string strLanguage, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                return Category;

            return (await _objCharacter.LoadDataXPathAsync("gear.xml", token: token).ConfigureAwait(false))
                .SelectSingleNodeAndCacheExpression(
                    "/chummer/categories/category[. = " + Category.CleanXPath() + "]/@translate", token)?.Value ?? Category;
        }

        /// <summary>
        /// Category of the Drug.
        /// </summary>
        public string Category
        {
            get => _strCategory;
            set => _strCategory = value;
        }

        private decimal _decCachedCost = decimal.MinValue;

        /// <summary>
        /// Base cost of the Drug.
        /// </summary>
        public decimal Cost
        {
            get
            {
                return _decCachedCost != decimal.MinValue
                    ? _decCachedCost
                    : _decCachedCost = Components.Sum(d => d.ActiveDrugEffect != null, d => d.CostPerLevel);
            }
        }

        /// <summary>
        /// Base cost of the Drug.
        /// </summary>
        public async Task<decimal> GetCostAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            return _decCachedCost != decimal.MinValue
                    ? _decCachedCost
                    : _decCachedCost = await Components.SumAsync(d => d.ActiveDrugEffect != null,
                                            d => d.GetCostPerLevelAsync(token), token).ConfigureAwait(false);
        }

        /// <summary>
        /// Total cost of the Drug.
        /// </summary>
        public decimal TotalCost => Cost * Quantity;

        /// <summary>
        /// Total cost of the Drug.
        /// </summary>
        public async Task<decimal> GetTotalCostAsync(CancellationToken token = default) =>
            await GetCostAsync(token).ConfigureAwait(false) * Quantity;

        public decimal StolenTotalCost => Stolen ? TotalCost : 0;

        public decimal NonStolenTotalCost => Stolen ? 0 : TotalCost;

        public async Task<decimal> GetStolenTotalCostAsync(CancellationToken token = default) =>
            Stolen ? await GetTotalCostAsync(token).ConfigureAwait(false) : 0;

        public async Task<decimal> GetNonStolenTotalCostAsync(CancellationToken token = default) =>
            Stolen ? 0 : await GetTotalCostAsync(token).ConfigureAwait(false);

        /// <summary>
        /// Total amount of the Drug held by the character.
        /// </summary>
        public decimal Quantity
        {
            get => _decQty;
            set => _decQty = value;
        }

        /// <summary>
        /// Availability of the Drug.
        /// </summary>
        public string Availability => _strAvailability;

        /// <summary>
        /// Total Availability in the program's current language.
        /// </summary>
        public string DisplayTotalAvail => TotalAvail(GlobalSettings.CultureInfo, GlobalSettings.Language);

        /// <summary>
        /// Total Availability in the program's current language.
        /// </summary>
        public Task<string> GetDisplayTotalAvailAsync(CancellationToken token = default) => TotalAvailAsync(GlobalSettings.CultureInfo, GlobalSettings.Language, token);

        /// <summary>
        /// Total Availability.
        /// </summary>
        public string TotalAvail(CultureInfo objCulture, string strLanguage)
        {
            return TotalAvailTuple().ToString(objCulture, strLanguage);
        }

        /// <summary>
        /// Calculated Availability of the Vehicle.
        /// </summary>
        public async Task<string> TotalAvailAsync(CultureInfo objCulture, string strLanguage, CancellationToken token = default)
        {
            return await (await TotalAvailTupleAsync(token: token).ConfigureAwait(false)).ToStringAsync(objCulture, strLanguage, token).ConfigureAwait(false);
        }

        /// <summary>
        /// Total Availability as a triple.
        /// </summary>
        public AvailabilityValue TotalAvailTuple(bool blnCheckChildren = true)
        {
            bool blnModifyParentAvail = false;
            string strAvail = Availability;
            char chrLastAvailChar = ' ';
            int intAvail = 0;
            if (strAvail.Length > 0)
            {
                chrLastAvailChar = strAvail[strAvail.Length - 1];
                if (chrLastAvailChar == 'F' || chrLastAvailChar == 'R')
                {
                    strAvail = strAvail.Substring(0, strAvail.Length - 1);
                }

                blnModifyParentAvail = strAvail.StartsWith('+', '-');
                if (strAvail.DoesNeedXPathProcessingToBeConvertedToNumber(out decimal decValue))
                {
                    using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdAvail))
                    {
                        sbdAvail.Append(strAvail.TrimStart('+'));
                        _objCharacter.ProcessAttributesInXPath(sbdAvail, strAvail);
                        (bool blnIsSuccess, object objProcess)
                            = CommonFunctions.EvaluateInvariantXPath(sbdAvail.ToString());
                        if (blnIsSuccess)
                            intAvail += ((double)objProcess).StandardRound();
                    }
                }
                else
                    intAvail += decValue.StandardRound();
            }
            if (blnCheckChildren)
            {
                // Run through the Accessories and add in their availability.
                foreach (AvailabilityValue objLoopAvail in Components.Select(x => x.TotalAvailTuple))
                {
                    if (objLoopAvail.AddToParent)
                        intAvail += objLoopAvail.Value;
                    if (objLoopAvail.Suffix == 'F')
                        chrLastAvailChar = 'F';
                    else if (chrLastAvailChar != 'F' && objLoopAvail.Suffix == 'R')
                        chrLastAvailChar = 'R';
                }
            }

            if (intAvail < 0)
                intAvail = 0;

            return new AvailabilityValue(intAvail, chrLastAvailChar, blnModifyParentAvail);
        }

        /// <summary>
        /// Total Availability as a triple.
        /// </summary>
        public async Task<AvailabilityValue> TotalAvailTupleAsync(bool blnCheckChildren = true, CancellationToken token = default)
        {
            bool blnModifyParentAvail = false;
            string strAvail = Availability;
            char chrLastAvailChar = ' ';
            int intAvail = 0;
            if (strAvail.Length > 0)
            {
                chrLastAvailChar = strAvail[strAvail.Length - 1];
                if (chrLastAvailChar == 'F' || chrLastAvailChar == 'R')
                {
                    strAvail = strAvail.Substring(0, strAvail.Length - 1);
                }

                blnModifyParentAvail = strAvail.StartsWith('+', '-');
                if (strAvail.DoesNeedXPathProcessingToBeConvertedToNumber(out decimal decValue))
                {
                    using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdAvail))
                    {
                        sbdAvail.Append(strAvail.TrimStart('+'));
                        await _objCharacter.ProcessAttributesInXPathAsync(sbdAvail, strAvail, token: token).ConfigureAwait(false);
                        (bool blnIsSuccess, object objProcess)
                            = await CommonFunctions.EvaluateInvariantXPathAsync(sbdAvail.ToString(), token).ConfigureAwait(false);
                        if (blnIsSuccess)
                            intAvail += ((double)objProcess).StandardRound();
                    }
                }
                else
                    intAvail += decValue.StandardRound();
            }
            if (blnCheckChildren)
            {
                // Run through the Accessories and add in their availability.
                intAvail += await Components.SumAsync(async objComponent =>
                {
                    AvailabilityValue objLoopAvail
                        = await objComponent.GetTotalAvailTupleAsync(token).ConfigureAwait(false);
                    if (objLoopAvail.Suffix == 'F')
                        chrLastAvailChar = 'F';
                    else if (chrLastAvailChar != 'F' && objLoopAvail.Suffix == 'R')
                        chrLastAvailChar = 'R';
                    return objLoopAvail.AddToParent ? await objLoopAvail.GetValueAsync(token).ConfigureAwait(false) : 0;
                }, token).ConfigureAwait(false);
            }

            if (intAvail < 0)
                intAvail = 0;

            return new AvailabilityValue(intAvail, chrLastAvailChar, blnModifyParentAvail);
        }

        private int _intCachedAddictionThreshold = int.MinValue;

        /// <summary>
        /// Addiction Threshold of the Drug.
        /// </summary>
        public int AddictionThreshold
        {
            get
            {
                return _intCachedAddictionThreshold != int.MinValue
                    ? _intCachedAddictionThreshold
                    : _intCachedAddictionThreshold = Components.Sum(d => d.ActiveDrugEffect != null, d => d.AddictionThreshold);
            }
        }

        /// <summary>
        /// Addiction Threshold of the Drug.
        /// </summary>
        public async Task<int> GetAddictionThresholdAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            return _intCachedAddictionRating != int.MinValue
                    ? _intCachedAddictionThreshold
                    : _intCachedAddictionThreshold = await Components.SumAsync(d => d.ActiveDrugEffect != null, d => d.AddictionThreshold, token).ConfigureAwait(false);
        }

        private int _intCachedAddictionRating = int.MinValue;

        /// <summary>
        /// Addiction Rating of the Drug.
        /// </summary>
        public int AddictionRating
        {
            get
            {
                return _intCachedAddictionRating != int.MinValue
                    ? _intCachedAddictionRating
                    : _intCachedAddictionRating = Components.Sum(d => d.ActiveDrugEffect != null, d => d.AddictionRating);
            }
        }

        /// <summary>
        /// Addiction Rating of the Drug.
        /// </summary>
        public async Task<int> GetAddictionRatingAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            return _intCachedAddictionRating != int.MinValue
                    ? _intCachedAddictionRating
                    : _intCachedAddictionRating = await Components.SumAsync(d => d.ActiveDrugEffect != null, d => d.AddictionRating, token).ConfigureAwait(false);
        }

        private bool _blnCachedLimitFlag;

        public Dictionary<string, int> Limits
        {
            get
            {
                if (_blnCachedLimitFlag)
                    return _dicCachedLimits;
                _dicCachedLimits.Clear();
                foreach (DrugComponent objComponent in Components)
                {
                    DrugEffect objDrugEffect = objComponent.ActiveDrugEffect;
                    if (objDrugEffect != null && objDrugEffect.Limits.Count > 0)
                    {
                        foreach (KeyValuePair<string, int> kvpLimit in objDrugEffect.Limits)
                        {
                            string strKey = kvpLimit.Key;
                            if (_dicCachedLimits.TryGetValue(strKey, out int intExistingValue))
                                _dicCachedLimits[strKey] = intExistingValue + kvpLimit.Value;
                            else
                                _dicCachedLimits.Add(strKey, kvpLimit.Value);
                        }
                    }
                }
                _blnCachedLimitFlag = true;
                return _dicCachedLimits;
            }
        }

        public async Task<Dictionary<string, int>> GetLimitsAsync(CancellationToken token = default)
        {
            if (_blnCachedLimitFlag)
                return _dicCachedLimits;
            _dicCachedLimits.Clear();
            await Components.ForEachAsync(objComponent =>
            {
                DrugEffect objDrugEffect = objComponent.ActiveDrugEffect;
                if (objDrugEffect != null && objDrugEffect.Limits.Count > 0)
                {
                    foreach (KeyValuePair<string, int> kvpLimit in objDrugEffect.Limits)
                    {
                        string strKey = kvpLimit.Key;
                        if (_dicCachedLimits.TryGetValue(strKey, out int intExistingValue))
                            _dicCachedLimits[strKey] = intExistingValue + kvpLimit.Value;
                        else
                            _dicCachedLimits.Add(strKey, kvpLimit.Value);
                    }
                }
            }, token).ConfigureAwait(false);
            _blnCachedLimitFlag = true;
            return _dicCachedLimits;
        }

        private bool _blnCachedQualityFlag;

        public List<XmlNode> Qualities
        {
            get
            {
                if (_blnCachedQualityFlag)
                    return _lstCachedQualities;
                _lstCachedQualities.Clear();
                foreach (DrugComponent objComponent in Components)
                {
                    DrugEffect objDrugEffect = objComponent.ActiveDrugEffect;
                    if (objDrugEffect != null)
                    {
                        foreach (XmlNode objQuality in objDrugEffect.Qualities)
                        {
                            if (!_lstCachedQualities.Contains(objQuality))
                                _lstCachedQualities.Add(objQuality);
                        }
                    }
                }
                _blnCachedQualityFlag = true;
                return _lstCachedQualities;
            }
        }

        public async Task<List<XmlNode>> GetQualitiesAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (_blnCachedQualityFlag)
                return _lstCachedQualities;
            _lstCachedQualities.Clear();
            await Components.ForEachAsync(objComponent =>
            {
                DrugEffect objDrugEffect = objComponent.ActiveDrugEffect;
                if (objDrugEffect != null)
                {
                    foreach (XmlNode objQuality in objDrugEffect.Qualities)
                    {
                        if (!_lstCachedQualities.Contains(objQuality))
                            _lstCachedQualities.Add(objQuality);
                    }
                }
            }, token).ConfigureAwait(false);
            _blnCachedQualityFlag = true;
            return _lstCachedQualities;
        }

        private bool _blnCachedInfoFlag;

        public List<string> Infos
        {
            get
            {
                if (_blnCachedInfoFlag)
                    return _lstCachedInfos;
                _lstCachedInfos.Clear();
                foreach (DrugComponent objComponent in Components)
                {
                    DrugEffect objDrugEffect = objComponent.ActiveDrugEffect;
                    if (objDrugEffect != null)
                    {
                        foreach (string strInfo in objDrugEffect.Infos)
                        {
                            if (!_lstCachedInfos.Contains(strInfo))
                                _lstCachedInfos.Add(strInfo);
                        }
                    }
                }
                _blnCachedInfoFlag = true;
                return _lstCachedInfos;
            }
        }

        public async Task<List<string>> GetInfosAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (_blnCachedInfoFlag)
                return _lstCachedInfos;
            _lstCachedInfos.Clear();
            await Components.ForEachAsync(objComponent =>
            {
                DrugEffect objDrugEffect = objComponent.ActiveDrugEffect;
                if (objDrugEffect != null)
                {
                    foreach (string strInfo in objDrugEffect.Infos)
                    {
                        if (!_lstCachedInfos.Contains(strInfo))
                            _lstCachedInfos.Add(strInfo);
                    }
                }
            }, token).ConfigureAwait(false);
            _blnCachedInfoFlag = true;
            return _lstCachedInfos;
        }

        private int _intCachedInitiative = int.MinValue;

        public int Initiative
        {
            get
            {
                return _intCachedInitiative != int.MinValue
                    ? _intCachedInitiative
                    : _intCachedInitiative = Components.Sum(d => d.ActiveDrugEffect != null, d => d.ActiveDrugEffect.Initiative);
            }
        }

        public async Task<int> GetInitiativeAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            return _intCachedInitiative != int.MinValue
                    ? _intCachedInitiative
                    : _intCachedInitiative = await Components.SumAsync(d => d.ActiveDrugEffect != null, d => d.ActiveDrugEffect.Initiative, token).ConfigureAwait(false);
        }

        private int _intCachedInitiativeDice = int.MinValue;

        public int InitiativeDice
        {
            get
            {
                return _intCachedInitiativeDice != int.MinValue
                    ? _intCachedInitiativeDice
                    : _intCachedInitiativeDice = Components.Sum(d => d.ActiveDrugEffect != null, d => d.ActiveDrugEffect.InitiativeDice);
            }
        }

        public async Task<int> GetInitiativeDiceAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            return _intCachedInitiativeDice != int.MinValue
                    ? _intCachedInitiativeDice
                    : _intCachedInitiativeDice = await Components.SumAsync(d => d.ActiveDrugEffect != null, d => d.ActiveDrugEffect.InitiativeDice, token).ConfigureAwait(false);
        }

        private int _intCachedSpeed = int.MinValue;

        /// <summary>
        /// How quickly the Drug takes effect, in seconds. A Combat Turn is considered
        /// to be 3 seconds, so anything with a Speed below 3 is considered to be Immediate.
        /// </summary>
        public int Speed
        {
            get
            {
                return _intCachedSpeed != int.MinValue
                    ? _intCachedSpeed
                    : _intCachedSpeed = Components.Sum(d => d.ActiveDrugEffect != null, d => d.ActiveDrugEffect.Speed) + _intSpeed;
            }
        }

        /// <summary>
        /// How quickly the Drug takes effect, in seconds. A Combat Turn is considered
        /// to be 3 seconds, so anything with a Speed below 3 is considered to be Immediate.
        /// </summary>
        public async Task<int> GetSpeedAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            return _intCachedSpeed != int.MinValue
                    ? _intCachedSpeed
                    : _intCachedSpeed = await Components.SumAsync(d => d.ActiveDrugEffect != null, d => d.ActiveDrugEffect.Speed, token).ConfigureAwait(false) + _intSpeed;
        }

        private int _intCachedDuration = int.MinValue;

        public int Duration
        {
            get
            {
                if (_intCachedDuration != int.MinValue)
                    return _intCachedDuration;
                string strDuration = _strDuration;
                if (string.IsNullOrWhiteSpace(strDuration))
                    return _intCachedDuration = 0;

                if (strDuration.DoesNeedXPathProcessingToBeConvertedToNumber(out decimal decDuration))
                {
                    using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdDuration))
                    {
                        sbdDuration.Append(strDuration);
                        // If the value contain an CharacterAttribute name, replace it with the character's CharacterAttribute.
                        _objCharacter.ProcessAttributesInXPath(sbdDuration, strDuration);
                        (bool blnIsSuccess, object objProcess) = CommonFunctions.EvaluateInvariantXPath(sbdDuration.ToString());
                        if (blnIsSuccess)
                            decDuration = Convert.ToDecimal((double)objProcess);
                    }
                }

                decDuration += Components.Sum(d => d.ActiveDrugEffect != null, d => d.ActiveDrugEffect.Duration) +
                               ImprovementManager.ValueOf(_objCharacter, Improvement.ImprovementType.DrugDuration);
                List<Improvement> lstImprovements = ImprovementManager.GetCachedImprovementListForValueOf(_objCharacter, Improvement.ImprovementType.DrugDurationMultiplier);
                if (lstImprovements.Count > 0)
                {
                    decimal decMultiplier = 1;
                    foreach (Improvement objImprovement in lstImprovements)
                    {
                        decMultiplier -= 1.0m - objImprovement.Value / 100m;
                    }
                    decDuration *= 1.0m - decMultiplier;
                }
                return _intCachedDuration = decDuration.StandardRound();
            }
        }

        public async Task<int> GetDurationAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (_intCachedDuration != int.MinValue)
                return _intCachedDuration;
            string strDuration = _strDuration;
            if (string.IsNullOrWhiteSpace(strDuration))
                return _intCachedDuration = 0;

            if (strDuration.DoesNeedXPathProcessingToBeConvertedToNumber(out decimal decDuration))
            {
                using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdDuration))
                {
                    sbdDuration.Append(strDuration);
                    // If the value contain an CharacterAttribute name, replace it with the character's CharacterAttribute.
                    await _objCharacter
                        .ProcessAttributesInXPathAsync(sbdDuration, strDuration, token: token).ConfigureAwait(false);
                    (bool blnIsSuccess, object objProcess) = await CommonFunctions.EvaluateInvariantXPathAsync(sbdDuration.ToString(), token).ConfigureAwait(false);
                    if (blnIsSuccess)
                        decDuration = Convert.ToDecimal((double)objProcess);
                }
            }

            decDuration += await Components.SumAsync(d => d.ActiveDrugEffect != null, d => d.ActiveDrugEffect.Duration, token).ConfigureAwait(false) +
                           await ImprovementManager.ValueOfAsync(_objCharacter, Improvement.ImprovementType.DrugDuration, token: token).ConfigureAwait(false);
            List<Improvement> lstImprovements = await ImprovementManager.GetCachedImprovementListForValueOfAsync(_objCharacter, Improvement.ImprovementType.DrugDurationMultiplier, token: token).ConfigureAwait(false);
            if (lstImprovements.Count > 0)
            {
                decimal decMultiplier = 1;
                foreach (Improvement objImprovement in lstImprovements)
                {
                    decMultiplier -= 1.0m - objImprovement.Value / 100m;
                }
                decDuration *= 1.0m - decMultiplier;
            }
            return _intCachedDuration = decDuration.StandardRound();
        }

        public CommonFunctions.Timescale DurationTimescale { get; private set; }

        private string _strCachedDisplayDuration;

        public string CurrentDisplayDuration => GetDisplayDuration(GlobalSettings.CultureInfo, GlobalSettings.Language);

        public Task<string> GetCurrentDisplayDurationAsync(CancellationToken token = default) => GetDisplayDurationAsync(GlobalSettings.CultureInfo, GlobalSettings.Language, token);

        public string GetDisplayDuration(CultureInfo objCulture, string strLanguage)
        {
            bool blnDoCache = strLanguage.Equals(GlobalSettings.Language, StringComparison.OrdinalIgnoreCase) && objCulture == GlobalSettings.CultureInfo;
            if (!string.IsNullOrWhiteSpace(_strCachedDisplayDuration) && blnDoCache)
                return _strCachedDisplayDuration;
            int intDuration = Duration;
            if (intDuration > 0)
            {
                string strSpace = LanguageManager.GetString("String_Space", strLanguage);
                string strDisplayDuration = intDuration.ToString(objCulture) + strSpace;
                if (DurationDice > 0)
                {
                    strDisplayDuration += '×' + strSpace + DurationDice.ToString(objCulture) +
                                            LanguageManager.GetString("String_D6", strLanguage) + strSpace;
                }
                if (blnDoCache)
                    return _strCachedDisplayDuration = strDisplayDuration + CommonFunctions.GetTimescaleString(DurationTimescale, intDuration > 1, strLanguage);
                return strDisplayDuration + CommonFunctions.GetTimescaleString(DurationTimescale, intDuration > 1, strLanguage);
            }
            if (blnDoCache)
                return _strCachedDisplayDuration = CommonFunctions.GetTimescaleString(DurationTimescale, false, strLanguage);
            return CommonFunctions.GetTimescaleString(DurationTimescale, false, strLanguage);
        }

        public async Task<string> GetDisplayDurationAsync(CultureInfo objCulture, string strLanguage, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            bool blnDoCache = strLanguage.Equals(GlobalSettings.Language, StringComparison.OrdinalIgnoreCase) && objCulture == GlobalSettings.CultureInfo;
            if (!string.IsNullOrWhiteSpace(_strCachedDisplayDuration) && blnDoCache)
                return _strCachedDisplayDuration;
            int intDuration = await GetDurationAsync(token).ConfigureAwait(false);
            if (intDuration > 0)
            {
                string strSpace = await LanguageManager.GetStringAsync("String_Space", strLanguage, token: token).ConfigureAwait(false);
                string strDisplayDuration = intDuration.ToString(objCulture) + strSpace;
                if (DurationDice > 0)
                {
                    strDisplayDuration += '×' + strSpace + DurationDice.ToString(objCulture) +
                                          await LanguageManager.GetStringAsync("String_D6", strLanguage, token: token).ConfigureAwait(false) + strSpace;
                }
                return _strCachedDisplayDuration = strDisplayDuration + await CommonFunctions.GetTimescaleStringAsync(DurationTimescale, intDuration > 1, token: token).ConfigureAwait(false);
            }
            return _strCachedDisplayDuration = await CommonFunctions.GetTimescaleStringAsync(DurationTimescale, false, token: token).ConfigureAwait(false);
        }

        public int DurationDice { get; set; }

        private int _intCachedCrashDamage = int.MinValue;

        public int CrashDamage
        {
            get
            {
                return _intCachedCrashDamage != int.MinValue
                    ? _intCachedCrashDamage
                    : _intCachedCrashDamage = Components.Sum(d => d.ActiveDrugEffect != null, d => d.ActiveDrugEffect.CrashDamage);
            }
        }

        public async Task<int> GetCrashDamageAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            return _intCachedSpeed != int.MinValue
                    ? _intCachedSpeed
                    : _intCachedSpeed = await Components.SumAsync(d => d.ActiveDrugEffect != null, d => d.ActiveDrugEffect.CrashDamage, token).ConfigureAwait(false);
        }

        /// <summary>
        /// Used by our sorting algorithm to remember which order the user moves things to
        /// </summary>
        public int SortOrder
        {
            get => _intSortOrder;
            set => _intSortOrder = value;
        }

        /// <summary>
        /// Notes.
        /// </summary>
        public string Notes
        {
            get => _strNotes;
            set => _strNotes = value;
        }

        public Task<string> GetNotesAsync(CancellationToken token = default)
        {
            if (token.IsCancellationRequested)
                return Task.FromCanceled<string>(token);
            return Task.FromResult(_strNotes);
        }

        public Task SetNotesAsync(string value, CancellationToken token = default)
        {
            if (token.IsCancellationRequested)
                return Task.FromCanceled(token);
            _strNotes = value;
            return Task.CompletedTask;
        }

        /// <summary>
        /// Forecolor to use for Notes in treeviews.
        /// </summary>
        public Color NotesColor
        {
            get => _colNotes;
            set => _colNotes = value;
        }

        public Task<Color> GetNotesColorAsync(CancellationToken token = default)
        {
            if (token.IsCancellationRequested)
                return Task.FromCanceled<Color>(token);
            return Task.FromResult(_colNotes);
        }

        public Task SetNotesColorAsync(Color value, CancellationToken token = default)
        {
            if (token.IsCancellationRequested)
                return Task.FromCanceled(token);
            _colNotes = value;
            return Task.CompletedTask;
        }

        /// <summary>
        /// The name of the object as it should appear on printouts (translated name only).
        /// </summary>
        public string DisplayNameShort(string strLanguage)
        {
            return strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase)
                ? Name
                : _objCharacter.TranslateExtra(Name, strLanguage);
        }

        /// <summary>
        /// The name of the object as it should appear on printouts (translated name only).
        /// </summary>
        public async Task<string> DisplayNameShortAsync(string strLanguage, CancellationToken token = default)
        {
            return strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase)
                ? Name
                : await _objCharacter.TranslateExtraAsync(Name, strLanguage, token: token).ConfigureAwait(false);
        }

        public string CurrentDisplayNameShort => DisplayNameShort(GlobalSettings.Language);

        public Task<string> GetCurrentDisplayNameShortAsync(CancellationToken token = default) =>
            DisplayNameShortAsync(GlobalSettings.Language, token);

        /// <summary>
        /// The name of the object as it should be displayed in lists. Qty Name (Rating) (Extra).
        /// </summary>
        public string DisplayName(CultureInfo objCulture, string strLanguage)
        {
            string strReturn = DisplayNameShort(strLanguage);
            if (Quantity != 1)
                strReturn = Quantity.ToString("#,0.##", objCulture) + LanguageManager.GetString("String_Space", strLanguage) + strReturn;
            return strReturn;
        }

        /// <summary>
        /// The name of the object as it should be displayed in lists. Qty Name (Rating) (Extra).
        /// </summary>
        public async Task<string> DisplayNameAsync(CultureInfo objCulture, string strLanguage, CancellationToken token = default)
        {
            string strReturn = await DisplayNameShortAsync(strLanguage, token).ConfigureAwait(false);
            if (Quantity != 1)
                strReturn = Quantity.ToString("#,0.##", objCulture) + await LanguageManager.GetStringAsync("String_Space", strLanguage, token: token).ConfigureAwait(false) + strReturn;
            return strReturn;
        }

        public string CurrentDisplayName => DisplayName(GlobalSettings.CultureInfo, GlobalSettings.Language);

        public Task<string> GetCurrentDisplayNameAsync(CancellationToken token = default) =>
            DisplayNameAsync(GlobalSettings.CultureInfo, GlobalSettings.Language, token);

        public Dictionary<string, decimal> Attributes
        {
            get
            {
                if (_blnCachedAttributeFlag)
                    return _dicCachedAttributes;
                _dicCachedAttributes.Clear();
                foreach (DrugComponent objComponent in Components)
                {
                    DrugEffect objDrugEffect = objComponent.ActiveDrugEffect;
                    if (objDrugEffect != null && objDrugEffect.Attributes.Count > 0)
                    {
                        foreach (KeyValuePair<string, decimal> objAttributeEntry in objDrugEffect.Attributes)
                        {
                            if (_dicCachedAttributes.TryGetValue(objAttributeEntry.Key, out decimal decExistingValue))
                                _dicCachedAttributes[objAttributeEntry.Key] = decExistingValue + objAttributeEntry.Value;
                            else
                                _dicCachedAttributes.Add(objAttributeEntry.Key, objAttributeEntry.Value);
                        }
                    }
                }
                _blnCachedAttributeFlag = true;
                return _dicCachedAttributes;
            }
        }

        public async Task<Dictionary<string, decimal>> GetAttributesAsync(CancellationToken token = default)
        {
            if (_blnCachedAttributeFlag)
                return _dicCachedAttributes;
            _dicCachedAttributes.Clear();
            await Components.ForEachAsync(objComponent =>
            {
                DrugEffect objDrugEffect = objComponent.ActiveDrugEffect;
                if (objDrugEffect != null && objDrugEffect.Attributes.Count > 0)
                {
                    foreach (KeyValuePair<string, decimal> objAttributeEntry in objDrugEffect.Attributes)
                    {
                        if (_dicCachedAttributes.TryGetValue(objAttributeEntry.Key, out decimal decExistingValue))
                            _dicCachedAttributes[objAttributeEntry.Key] = decExistingValue + objAttributeEntry.Value;
                        else
                            _dicCachedAttributes.Add(objAttributeEntry.Key, objAttributeEntry.Value);
                    }
                }
            }, token).ConfigureAwait(false);
            _blnCachedAttributeFlag = true;
            return _dicCachedAttributes;
        }

        public Color PreferredColor =>
            !string.IsNullOrEmpty(Notes)
                ? ColorManager.HasNotesColor
                : ColorManager.WindowText;

        public async Task<Color> GetPreferredColorAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (!string.IsNullOrEmpty(await GetNotesAsync(token).ConfigureAwait(false)))
                return ColorManager.GenerateCurrentModeColor(await GetNotesColorAsync(token).ConfigureAwait(false));
            return ColorManager.WindowText;
        }

        /// <summary>
        /// Identifier of the object within data files.
        /// </summary>
        public Guid SourceID => _guiSourceID;

        /// <summary>
        /// String-formatted identifier of the <inheritdoc cref="SourceID"/> from the data files.
        /// </summary>
        public string SourceIDString => _guiSourceID.ToString("D", GlobalSettings.InvariantCultureInfo);

        public bool Stolen
        {
            get => _blnStolen;
            set => _blnStolen = value;
        }

        public Character CharacterObject => _objCharacter;

        #endregion Properties

        #region UI Methods

        /// <summary>
        /// Add a piece of Armor to the Armor TreeView.
        /// </summary>
        public async Task<TreeNode> CreateTreeNode(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            //if (!string.IsNullOrEmpty(ParentID) && !string.IsNullOrEmpty(Source) && !_objCharacter.Settings.BookEnabled(Source))
            //return null;

            TreeNode objNode = new TreeNode
            {
                Name = InternalId,
                Text = await GetCurrentDisplayNameAsync(token).ConfigureAwait(false),
                Tag = this,
                ForeColor = await GetPreferredColorAsync(token).ConfigureAwait(false),
                ToolTipText = (await GetNotesAsync(token).ConfigureAwait(false)).WordWrap()
            };

            TreeNodeCollection lstChildNodes = objNode.Nodes;

            if (lstChildNodes.Count > 0)
                objNode.Expand();

            return objNode;
        }

        #endregion UI Methods

        #region Methods

        public string GenerateDescription(int intLevel = -1, bool blnEffectsOnly = false, string strLanguage = "", CultureInfo objCulture = null, bool blnDoCache = true)
        {
            if (string.IsNullOrEmpty(strLanguage))
                strLanguage = GlobalSettings.Language;
            if (objCulture == null)
                objCulture = GlobalSettings.CultureInfo;
            using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool,
                                                          out StringBuilder sbdDescription))
            {
                string strSpace = LanguageManager.GetString("String_Space", strLanguage);
                if (!blnEffectsOnly)
                {
                    string strName = DisplayNameShort(strLanguage);
                    if (!string.IsNullOrWhiteSpace(strName))
                        sbdDescription.AppendLine(strName);
                }

                if (intLevel != -1)
                {
                    bool blnNewLineFlag = false;
                    foreach (KeyValuePair<string, decimal> objAttribute in Attributes)
                    {
                        if (objAttribute.Value != 0)
                        {
                            if (blnNewLineFlag)
                            {
                                sbdDescription.Append(',').Append(strSpace);
                            }

                            sbdDescription
                                .Append(LanguageManager.GetString("String_Attribute" + objAttribute.Key + "Short",
                                                                  strLanguage)).Append(strSpace)
                                .Append(objAttribute.Value.ToString("+#.#;-#.#", GlobalSettings.CultureInfo));
                            blnNewLineFlag = true;
                        }
                    }

                    if (blnNewLineFlag)
                    {
                        blnNewLineFlag = false;
                        sbdDescription.AppendLine();
                    }

                    foreach (KeyValuePair<string, int> objLimit in Limits)
                    {
                        if (objLimit.Value != 0)
                        {
                            if (blnNewLineFlag)
                            {
                                sbdDescription.Append(',').Append(strSpace);
                            }

                            sbdDescription.Append(LanguageManager.GetString("Node_" + objLimit.Key, strLanguage))
                                          .Append(strSpace)
                                          .Append(LanguageManager.GetString("String_Limit", strLanguage))
                                          .Append(strSpace)
                                          .Append(objLimit.Value.ToString(" +#;-#", GlobalSettings.CultureInfo));
                            blnNewLineFlag = true;
                        }
                    }

                    if (blnNewLineFlag)
                    {
                        sbdDescription.AppendLine();
                    }

                    if (Initiative != 0 || InitiativeDice != 0)
                    {
                        sbdDescription.Append(LanguageManager.GetString("String_AttributeINILong", strLanguage))
                                      .Append(strSpace);
                        if (Initiative != 0)
                        {
                            sbdDescription.Append(Initiative.ToString("+#;-#", GlobalSettings.CultureInfo));
                            if (InitiativeDice != 0)
                                sbdDescription.Append(InitiativeDice.ToString("+#;-#", GlobalSettings.CultureInfo))
                                              .Append(LanguageManager.GetString("String_D6", strLanguage));
                        }
                        else if (InitiativeDice != 0)
                            sbdDescription.Append(InitiativeDice.ToString("+#;-#", GlobalSettings.CultureInfo))
                                          .Append(LanguageManager.GetString("String_D6", strLanguage));

                        sbdDescription.AppendLine();
                    }

                    foreach (XmlNode nodQuality in Qualities)
                    {
                        sbdDescription.Append(_objCharacter.TranslateExtra(nodQuality.InnerText, strLanguage))
                                      .Append(strSpace)
                                      .AppendLine(LanguageManager.GetString("String_Quality", strLanguage));
                    }

                    foreach (string strInfo in Infos)
                        sbdDescription.AppendLine(_objCharacter.TranslateExtra(strInfo, strLanguage));

                    if (Category == "Custom Drug" || Duration != 0)
                        sbdDescription.Append(LanguageManager.GetString("Label_Duration", strLanguage))
                                      .AppendLine(GetDisplayDuration(objCulture, strLanguage));

                    if (Category == "Custom Drug" || Speed != 0)
                    {
                        sbdDescription.Append(LanguageManager.GetString("Label_Speed"))
                                      .Append(LanguageManager.GetString("String_Colon", strLanguage)).Append(strSpace);
                        if (Speed <= 0)
                            sbdDescription.AppendLine(LanguageManager.GetString("String_Immediate"));
                        else if (Speed <= 60)
                            sbdDescription.Append((Speed / 3).ToString(GlobalSettings.CultureInfo)).Append(strSpace)
                                          .AppendLine(LanguageManager.GetString("String_CombatTurns"));
                        else
                            sbdDescription.Append(Speed.ToString(GlobalSettings.CultureInfo))
                                          .AppendLine(LanguageManager.GetString("String_Seconds"));
                    }

                    if (CrashDamage != 0)
                        sbdDescription.Append(LanguageManager.GetString("Label_CrashEffect", strLanguage))
                                      .Append(strSpace)
                                      .Append(CrashDamage.ToString(objCulture))
                                      .Append(LanguageManager.GetString("String_DamageStun", strLanguage))
                                      .Append(strSpace)
                                      .AppendLine(LanguageManager.GetString("String_DamageUnresisted", strLanguage));
                    if (!blnEffectsOnly)
                    {
                        sbdDescription.Append(LanguageManager.GetString("Label_AddictionRating", strLanguage))
                                      .Append(strSpace)
                                      .AppendLine((AddictionRating * (intLevel + 1)).ToString(objCulture))
                                      .Append(LanguageManager.GetString("Label_AddictionThreshold", strLanguage))
                                      .Append(strSpace)
                                      .AppendLine((AddictionThreshold * (intLevel + 1)).ToString(objCulture))
                                      .Append(LanguageManager.GetString("Label_Cost", strLanguage)).Append(strSpace)
                                      .Append((Cost * (intLevel + 1)).ToString(
                                                  _objCharacter.Settings.NuyenFormat, objCulture)).AppendLine(LanguageManager.GetString("String_NuyenSymbol"))
                                      .Append(LanguageManager.GetString("Label_Avail", strLanguage)).Append(strSpace)
                                      .AppendLine(TotalAvail(objCulture, strLanguage));
                    }
                }
                else if (!blnEffectsOnly)
                {
                    sbdDescription.Append(LanguageManager.GetString("Label_AddictionRating", strLanguage))
                                  .Append(strSpace)
                                  .AppendLine(0.ToString(objCulture))
                                  .Append(LanguageManager.GetString("Label_AddictionThreshold", strLanguage))
                                  .Append(strSpace).AppendLine(0.ToString(objCulture))
                                  .Append(LanguageManager.GetString("Label_Cost", strLanguage)).Append(strSpace)
                                  .Append((Cost * (intLevel + 1)).ToString(
                                              _objCharacter.Settings.NuyenFormat, objCulture))
                                  .AppendLine(LanguageManager.GetString("String_NuyenSymbol"))
                                  .Append(LanguageManager.GetString("Label_Avail", strLanguage)).Append(strSpace)
                                  .AppendLine(TotalAvail(objCulture, strLanguage));
                }

                string strReturn = sbdDescription.ToString();
                if (blnDoCache)
                    _strDescription = strReturn;
                return strReturn;
            }
        }

        public async Task<string> GenerateDescriptionAsync(int intLevel = -1, bool blnEffectsOnly = false, string strLanguage = "", CultureInfo objCulture = null, bool blnDoCache = true, CancellationToken token = default)
        {
            if (string.IsNullOrEmpty(strLanguage))
                strLanguage = GlobalSettings.Language;
            if (objCulture == null)
                objCulture = GlobalSettings.CultureInfo;
            using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool,
                                                          out StringBuilder sbdDescription))
            {
                string strSpace = await LanguageManager.GetStringAsync("String_Space", strLanguage, token: token).ConfigureAwait(false);
                if (!blnEffectsOnly)
                {
                    string strName = await DisplayNameShortAsync(strLanguage, token).ConfigureAwait(false);
                    if (!string.IsNullOrWhiteSpace(strName))
                        sbdDescription.AppendLine(strName);
                }

                if (intLevel != -1)
                {
                    bool blnNewLineFlag = false;
                    foreach (KeyValuePair<string, decimal> objAttribute in await GetAttributesAsync(token).ConfigureAwait(false))
                    {
                        if (objAttribute.Value != 0)
                        {
                            if (blnNewLineFlag)
                            {
                                sbdDescription.Append(',').Append(strSpace);
                            }

                            sbdDescription
                                .Append(await LanguageManager.GetStringAsync("String_Attribute" + objAttribute.Key + "Short",
                                                                             strLanguage, token: token).ConfigureAwait(false)).Append(strSpace)
                                .Append(objAttribute.Value.ToString("+#.#;-#.#", GlobalSettings.CultureInfo));
                            blnNewLineFlag = true;
                        }
                    }

                    if (blnNewLineFlag)
                    {
                        blnNewLineFlag = false;
                        sbdDescription.AppendLine();
                    }

                    foreach (KeyValuePair<string, int> objLimit in await GetLimitsAsync(token).ConfigureAwait(false))
                    {
                        if (objLimit.Value != 0)
                        {
                            if (blnNewLineFlag)
                            {
                                sbdDescription.Append(',').Append(strSpace);
                            }

                            sbdDescription.Append(await LanguageManager.GetStringAsync("Node_" + objLimit.Key, strLanguage, token: token).ConfigureAwait(false))
                                          .Append(strSpace)
                                          .Append(await LanguageManager.GetStringAsync("String_Limit", strLanguage, token: token).ConfigureAwait(false))
                                          .Append(strSpace)
                                          .Append(objLimit.Value.ToString(" +#;-#", GlobalSettings.CultureInfo));
                            blnNewLineFlag = true;
                        }
                    }

                    if (blnNewLineFlag)
                    {
                        sbdDescription.AppendLine();
                    }

                    int intInit = await GetInitiativeAsync(token).ConfigureAwait(false);
                    int intInitDice = await GetInitiativeDiceAsync(token).ConfigureAwait(false);
                    if (intInit != 0 || intInitDice != 0)
                    {
                        sbdDescription.Append(await LanguageManager.GetStringAsync("String_AttributeINILong", strLanguage, token: token).ConfigureAwait(false))
                                      .Append(strSpace);
                        if (intInit != 0)
                        {
                            sbdDescription.Append(intInit.ToString("+#;-#", GlobalSettings.CultureInfo));
                            if (intInitDice != 0)
                                sbdDescription.Append(intInitDice.ToString("+#;-#", GlobalSettings.CultureInfo))
                                              .Append(await LanguageManager.GetStringAsync("String_D6", strLanguage, token: token).ConfigureAwait(false));
                        }
                        else if (intInitDice != 0)
                            sbdDescription.Append(intInitDice.ToString("+#;-#", GlobalSettings.CultureInfo))
                                          .Append(await LanguageManager.GetStringAsync("String_D6", strLanguage, token: token).ConfigureAwait(false));

                        sbdDescription.AppendLine();
                    }

                    foreach (XmlNode nodQuality in await GetQualitiesAsync(token).ConfigureAwait(false))
                    {
                        sbdDescription.Append(await _objCharacter.TranslateExtraAsync(nodQuality.InnerText, strLanguage, token: token).ConfigureAwait(false))
                                      .Append(strSpace)
                                      .AppendLine(await LanguageManager.GetStringAsync("String_Quality", strLanguage, token: token).ConfigureAwait(false));
                    }

                    foreach (string strInfo in await GetInfosAsync(token).ConfigureAwait(false))
                        sbdDescription.AppendLine(await _objCharacter.TranslateExtraAsync(strInfo, strLanguage, token: token).ConfigureAwait(false));

                    if (Category == "Custom Drug" || await GetDurationAsync(token).ConfigureAwait(false) != 0)
                        sbdDescription.Append(await LanguageManager.GetStringAsync("Label_Duration", strLanguage, token: token).ConfigureAwait(false))
                                      .AppendLine(await GetDisplayDurationAsync(objCulture, strLanguage, token).ConfigureAwait(false));

                    int intSpeed = await GetSpeedAsync(token).ConfigureAwait(false);
                    if (Category == "Custom Drug" || intSpeed != 0)
                    {
                        sbdDescription.Append(await LanguageManager.GetStringAsync("Label_Speed", token: token).ConfigureAwait(false))
                                      .Append(await LanguageManager.GetStringAsync("String_Colon", strLanguage, token: token).ConfigureAwait(false)).Append(strSpace);
                        if (intSpeed <= 0)
                            sbdDescription.AppendLine(await LanguageManager.GetStringAsync("String_Immediate", token: token).ConfigureAwait(false));
                        else if (intSpeed <= 60)
                            sbdDescription.Append((intSpeed / 3).ToString(GlobalSettings.CultureInfo)).Append(strSpace)
                                          .AppendLine(await LanguageManager.GetStringAsync("String_CombatTurns", token: token).ConfigureAwait(false));
                        else
                            sbdDescription.Append(intSpeed.ToString(GlobalSettings.CultureInfo))
                                          .AppendLine(await LanguageManager.GetStringAsync("String_Seconds", token: token).ConfigureAwait(false));
                    }

                    int intCrashDamage = await GetCrashDamageAsync(token).ConfigureAwait(false);
                    if (intCrashDamage != 0)
                        sbdDescription.Append(await LanguageManager.GetStringAsync("Label_CrashEffect", strLanguage, token: token).ConfigureAwait(false))
                                      .Append(strSpace)
                                      .Append(intCrashDamage.ToString(objCulture))
                                      .Append(await LanguageManager.GetStringAsync("String_DamageStun", strLanguage, token: token).ConfigureAwait(false))
                                      .Append(strSpace)
                                      .AppendLine(await LanguageManager.GetStringAsync("String_DamageUnresisted", strLanguage, token: token).ConfigureAwait(false));
                    if (!blnEffectsOnly)
                    {
                        string strNuyenFormat = await (await _objCharacter.GetSettingsAsync(token).ConfigureAwait(false)).GetNuyenFormatAsync(token).ConfigureAwait(false);
                        sbdDescription.Append(await LanguageManager.GetStringAsync("Label_AddictionRating", strLanguage, token: token).ConfigureAwait(false))
                                      .Append(strSpace)
                                      .AppendLine((await GetAddictionRatingAsync(token).ConfigureAwait(false) * (intLevel + 1)).ToString(objCulture))
                                      .Append(await LanguageManager.GetStringAsync("Label_AddictionThreshold", strLanguage, token: token).ConfigureAwait(false))
                                      .Append(strSpace)
                                      .AppendLine((await GetAddictionThresholdAsync(token).ConfigureAwait(false) * (intLevel + 1)).ToString(objCulture))
                                      .Append(await LanguageManager.GetStringAsync("Label_Cost", strLanguage, token: token).ConfigureAwait(false)).Append(strSpace)
                                      .Append((await GetCostAsync(token).ConfigureAwait(false) * (intLevel + 1)).ToString(
                                                  strNuyenFormat, objCulture)).AppendLine(await LanguageManager.GetStringAsync("String_NuyenSymbol", token: token).ConfigureAwait(false))
                                      .Append(await LanguageManager.GetStringAsync("Label_Avail", strLanguage, token: token).ConfigureAwait(false)).Append(strSpace)
                                      .AppendLine(await TotalAvailAsync(objCulture, strLanguage, token).ConfigureAwait(false));
                    }
                }
                else if (!blnEffectsOnly)
                {
                    string strNuyenFormat = await (await _objCharacter.GetSettingsAsync(token).ConfigureAwait(false)).GetNuyenFormatAsync(token).ConfigureAwait(false);
                    sbdDescription.Append(await LanguageManager.GetStringAsync("Label_AddictionRating", strLanguage, token: token).ConfigureAwait(false))
                                  .Append(strSpace)
                                  .AppendLine(0.ToString(objCulture))
                                  .Append(await LanguageManager.GetStringAsync("Label_AddictionThreshold", strLanguage, token: token).ConfigureAwait(false))
                                  .Append(strSpace).AppendLine(0.ToString(objCulture))
                                  .Append(await LanguageManager.GetStringAsync("Label_Cost", strLanguage, token: token).ConfigureAwait(false)).Append(strSpace)
                                  .Append((await GetCostAsync(token).ConfigureAwait(false) * (intLevel + 1)).ToString(
                                              strNuyenFormat, objCulture))
                                  .AppendLine(await LanguageManager.GetStringAsync("String_NuyenSymbol", token: token).ConfigureAwait(false))
                                  .Append(await LanguageManager.GetStringAsync("Label_Avail", strLanguage, token: token).ConfigureAwait(false)).Append(strSpace)
                                  .AppendLine(await TotalAvailAsync(objCulture, strLanguage, token).ConfigureAwait(false));
                }

                string strReturn = sbdDescription.ToString();
                if (blnDoCache)
                    _strDescription = strReturn;
                return strReturn;
            }
        }

        /// <summary>
        /// Creates the improvements necessary to 'activate' a given drug.
        /// TODO: I'm really not happy with the lack of extensibility on this.
        /// TODO: Refactor drug effects to just use XML nodes, which can then be passed to Improvement Manager?
        /// TODO: Refactor Improvement Manager to automatically collapse improvements of the same type into a single improvement?
        /// </summary>
        public async Task GenerateImprovement(CancellationToken token = default)
        {
            if (await _objCharacter.Improvements.AnyAsync(ig => ig.SourceName == InternalId, token: token)
                    .ConfigureAwait(false))
                return;
            await _objCharacter.ImprovementGroups.AddAsync(Name, token).ConfigureAwait(false);
            string strSpace = await LanguageManager.GetStringAsync("String_Space", token: token).ConfigureAwait(false);
            string strNamePrefix = await GetCurrentDisplayNameShortAsync(token).ConfigureAwait(false) + strSpace + '-' +
                                   strSpace;
            List<Improvement> lstImprovements = new List<Improvement>();
            foreach (KeyValuePair<string, decimal> kvpAttribute in await GetAttributesAsync(token).ConfigureAwait(false))
            {
                if (kvpAttribute.Value != 0)
                {
                    lstImprovements.Add(new Improvement(_objCharacter)
                    {
                        ImproveSource = Improvement.ImprovementSource.Drug,
                        ImproveType = Improvement.ImprovementType.Attribute,
                        SourceName = InternalId,
                        Augmented = kvpAttribute.Value,
                        ImprovedName = kvpAttribute.Key,
                        CustomName =
                        strNamePrefix + LanguageManager.GetString("String_Attribute" + kvpAttribute.Key + "Short", token: token)
                                      + strSpace +
                                      kvpAttribute.Value.ToString("+#,0;-#,0;0", GlobalSettings.CultureInfo)
                    });
                }
            }

            foreach (KeyValuePair<string, int> objLimit in await GetLimitsAsync(token).ConfigureAwait(false))
            {
                if (objLimit.Value == 0) continue;
                Improvement i = new Improvement(_objCharacter)
                {
                    ImproveSource = Improvement.ImprovementSource.Drug,
                    SourceName = InternalId,
                    CustomName = strNamePrefix + await LanguageManager
                                                   .GetStringAsync("Node_" + objLimit.Key, token: token)
                                                   .ConfigureAwait(false)
                                               + strSpace + objLimit.Value.ToString("+#,0;-#,0;0",
                                                   GlobalSettings.CultureInfo)
                };
                await i.SetValueAsync(objLimit.Value, token).ConfigureAwait(false);
                switch (objLimit.Key)
                {
                    case "Physical":
                        i.ImproveType = Improvement.ImprovementType.PhysicalLimit;
                        break;

                    case "Mental":
                        i.ImproveType = Improvement.ImprovementType.MentalLimit;
                        break;

                    case "Social":
                        i.ImproveType = Improvement.ImprovementType.SocialLimit;
                        break;
                }

                lstImprovements.Add(i);
            }

            int intInitiative = await GetInitiativeAsync(token).ConfigureAwait(false);
            if (intInitiative != 0)
            {
                Improvement i = new Improvement(_objCharacter)
                {
                    ImproveSource = Improvement.ImprovementSource.Drug,
                    SourceName = InternalId,
                    ImproveType = Improvement.ImprovementType.Initiative,
                    CustomName = strNamePrefix + await LanguageManager.GetStringAsync("String_Initiative", token: token)
                                                   .ConfigureAwait(false)
                                               + strSpace + intInitiative.ToString("+#,0;-#,0;0",
                                                   GlobalSettings.CultureInfo)
                };
                await i.SetValueAsync(intInitiative, token).ConfigureAwait(false);
                lstImprovements.Add(i);
            }

            int intInitiativeDice = await GetInitiativeDiceAsync(token).ConfigureAwait(false);
            if (intInitiativeDice != 0)
            {
                Improvement i = new Improvement(_objCharacter)
                {
                    ImproveSource = Improvement.ImprovementSource.Drug,
                    SourceName = InternalId,
                    ImproveType = Improvement.ImprovementType.InitiativeDice,
                    CustomName = strNamePrefix + await LanguageManager
                                                   .GetStringAsync("String_InitiativeDice", token: token)
                                                   .ConfigureAwait(false)
                                               + strSpace + intInitiativeDice.ToString("+#,0;-#,0;0",
                                                   GlobalSettings.CultureInfo)
                };
                await i.SetValueAsync(intInitiativeDice, token).ConfigureAwait(false);
                lstImprovements.Add(i);
            }

            List<XmlNode> lstQualities = await GetQualitiesAsync(token).ConfigureAwait(false);
            if (lstQualities.Count > 0)
            {
                XmlDocument objXmlDocument =
                    await _objCharacter.LoadDataAsync("qualities.xml", token: token).ConfigureAwait(false);
                foreach (XmlNode objXmlAddQuality in lstQualities)
                {
                    XmlNode objXmlSelectedQuality =
                        objXmlDocument.TryGetNodeByNameOrId("/chummer/qualities/quality", objXmlAddQuality.InnerText);
                    if (objXmlSelectedQuality == null)
                        continue;
                    XPathNavigator xpnSelectedQuality = objXmlSelectedQuality.CreateNavigator();
                    string strForceValue = objXmlAddQuality.Attributes?["select"]?.InnerText ?? string.Empty;

                    string strRating = objXmlAddQuality.Attributes?["rating"]?.InnerText;
                    int intCount = string.IsNullOrEmpty(strRating)
                        ? 1
                        : await ImprovementManager.ValueToIntAsync(_objCharacter, strRating, 1, token)
                            .ConfigureAwait(false);
                    bool blnDoesNotContributeToBP =
                        !string.Equals(objXmlAddQuality.Attributes?["contributetobp"]?.InnerText, bool.TrueString,
                            StringComparison.OrdinalIgnoreCase);

                    for (int i = 0; i < intCount; ++i)
                    {
                        // Makes sure we aren't over our limits for this particular quality from this overall source
                        if (objXmlAddQuality.Attributes?["forced"]?.InnerText == bool.TrueString ||
                            await xpnSelectedQuality.RequirementsMetAsync(_objCharacter,
                                strLocalName: await LanguageManager.GetStringAsync("String_Quality", token: token)
                                    .ConfigureAwait(false), strIgnoreQuality: Name, token: token).ConfigureAwait(false))
                        {
                            List<Weapon> lstWeapons = new List<Weapon>(1);
                            Quality objAddQuality = new Quality(_objCharacter);
                            try
                            {
                                await objAddQuality.CreateAsync(objXmlSelectedQuality, QualitySource.Improvement,
                                    lstWeapons,
                                    strForceValue, Name, token).ConfigureAwait(false);

                                if (blnDoesNotContributeToBP)
                                {
                                    await objAddQuality.SetBPAsync(0, token).ConfigureAwait(false);
                                    await objAddQuality.SetContributeToLimitAsync(false, token).ConfigureAwait(false);
                                }

                                await _objCharacter.Qualities.AddAsync(objAddQuality, token).ConfigureAwait(false);
                                foreach (Weapon objWeapon in lstWeapons)
                                    await _objCharacter.Weapons.AddAsync(objWeapon, token).ConfigureAwait(false);
                                Improvement objImprovement = new Improvement(_objCharacter)
                                {
                                    ImprovedName = objAddQuality.InternalId,
                                    ImproveSource = Improvement.ImprovementSource.Drug,
                                    SourceName = InternalId,
                                    ImproveType = Improvement.ImprovementType.SpecificQuality,
                                    CustomName =
                                        strNamePrefix + await LanguageManager
                                                          .GetStringAsync("String_InitiativeDice", token: token)
                                                          .ConfigureAwait(false)
                                                      + strSpace + await objAddQuality.GetNameAsync(token)
                                                          .ConfigureAwait(false)
                                };
                                lstImprovements.Add(objImprovement);
                            }
                            catch
                            {
                                await objAddQuality.DisposeAsync().ConfigureAwait(false);
                                throw;
                            }
                        }
                        else
                        {
                            throw new AbortedException();
                        }
                    }
                }
            }

            foreach (Improvement i in lstImprovements)
            {
                i.CustomGroup = Name;
                i.Custom = true;
                await i.SetEnabledAsync(false, token).ConfigureAwait(false);
                // This is initially set to false make sure no property changers are triggered
                i.SetupComplete = true;
            }

            _objCharacter.Improvements.AddRange(lstImprovements);
        }

        public async Task<XmlNode> GetNodeCoreAsync(bool blnSync, string strLanguage, CancellationToken token = default)
        {
            XmlNode objReturn = _objCachedMyXmlNode;
            if (objReturn != null && strLanguage == _strCachedXmlNodeLanguage
                                  && !GlobalSettings.LiveCustomData)
                return objReturn;
            XmlDocument objDoc = blnSync
                // ReSharper disable once MethodHasAsyncOverload
                ? _objCharacter.LoadData("drugcomponents.xml", strLanguage, token: token)
                : await _objCharacter.LoadDataAsync("drugcomponents.xml", strLanguage, token: token).ConfigureAwait(false);
            if (SourceID != Guid.Empty)
                objReturn = objDoc.TryGetNodeById("/chummer/drugcomponents/drugcomponent", SourceID);
            if (objReturn == null)
            {
                objReturn = objDoc.TryGetNodeByNameOrId("/chummer/drugcomponents/drugcomponent", Name);
                objReturn?.TryGetGuidFieldQuickly("id", ref _guiSourceID);
            }
            _objCachedMyXmlNode = objReturn;
            _strCachedXmlNodeLanguage = strLanguage;
            return objReturn;
        }

        private XPathNavigator _objCachedMyXPathNode;
        private string _strCachedXPathNodeLanguage = string.Empty;
        private readonly ThreadSafeObservableCollection<DrugComponent> _lstComponents;

        public async Task<XPathNavigator> GetNodeXPathCoreAsync(bool blnSync, string strLanguage, CancellationToken token = default)
        {
            XPathNavigator objReturn = _objCachedMyXPathNode;
            if (objReturn != null && strLanguage == _strCachedXPathNodeLanguage
                                  && !GlobalSettings.LiveCustomData)
                return objReturn;
            XPathNavigator objDoc = blnSync
                // ReSharper disable once MethodHasAsyncOverload
                ? _objCharacter.LoadDataXPath("drugcomponents.xml", strLanguage, token: token)
                : await _objCharacter.LoadDataXPathAsync("drugcomponents.xml", strLanguage, token: token).ConfigureAwait(false);
            if (SourceID != Guid.Empty)
                objReturn = objDoc.TryGetNodeById("/chummer/drugcomponents/drugcomponent", SourceID);
            if (objReturn == null)
            {
                objReturn = objDoc.TryGetNodeByNameOrId("/chummer/drugcomponents/drugcomponent", Name);
                objReturn?.TryGetGuidFieldQuickly("id", ref _guiSourceID);
            }
            _objCachedMyXPathNode = objReturn;
            _strCachedXPathNodeLanguage = strLanguage;
            return objReturn;
        }

        public bool Remove(bool blnConfirmDelete = true)
        {
            if (blnConfirmDelete && !CommonFunctions.ConfirmDelete(LanguageManager.GetString("Message_DeleteDrug")))
            {
                return false;
            }
            _objCharacter.Drugs.Remove(this);
            ImprovementManager.RemoveImprovements(_objCharacter, Improvement.ImprovementSource.Drug, InternalId);

            Dispose();

            return true;
        }

        public async Task<bool> RemoveAsync(bool blnConfirmDelete = true, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (blnConfirmDelete && !await CommonFunctions
                    .ConfirmDeleteAsync(
                        await LanguageManager.GetStringAsync("Message_DeleteDrug", token: token).ConfigureAwait(false),
                        token).ConfigureAwait(false))
            {
                return false;
            }

            await _objCharacter.Drugs.RemoveAsync(this, token).ConfigureAwait(false);
            await ImprovementManager
                .RemoveImprovementsAsync(_objCharacter, Improvement.ImprovementSource.Drug, InternalId, token)
                .ConfigureAwait(false);

            await DisposeAsync().ConfigureAwait(false);

            return true;
        }

        #endregion Methods

        /// <inheritdoc />
        public void Dispose()
        {
            _lstComponents.Dispose();
        }

        /// <inheritdoc />
        public ValueTask DisposeAsync()
        {
            return _lstComponents.DisposeAsync();
        }
    }

    /// <summary>
    /// Drug Component.
    /// </summary>
    public class DrugComponent : IHasName, IHasInternalId, IHasXmlDataNode, IHasCharacterObject
    {
        private static readonly Lazy<Logger> s_ObjLogger = new Lazy<Logger>(LogManager.GetCurrentClassLogger);
        private static Logger Log => s_ObjLogger.Value;
        private Guid _guidId;
        private Guid _guiSourceID;
        private string _strName;
        private string _strCategory;
        private string _strAvailability = "0";
        private int _intLevel;
        private int _intLimit = 1;
        private string _strSource;
        private string _strPage;
        private string _strCost;
        private int _intAddictionThreshold;
        private int _intAddictionRating;
        private XmlNode _objCachedMyXmlNode;
        private string _strCachedXmlNodeLanguage;
        private readonly Character _objCharacter;

        public DrugComponent(Character objCharacter)
        {
            _guidId = Guid.NewGuid();
            _objCharacter = objCharacter;
        }

        public Character CharacterObject => _objCharacter;

        #region Constructor, Create, Save, Load, and Print Methods

        public void Load(XmlNode objXmlData)
        {
            objXmlData.TryGetStringFieldQuickly("name", ref _strName);
            _objCachedMyXmlNode = null;
            _objCachedMyXPathNode = null;
            if (!objXmlData.TryGetGuidFieldQuickly("sourceid", ref _guiSourceID))
            {
                this.GetNodeXPath()?.TryGetGuidFieldQuickly("id", ref _guiSourceID);
            }
            objXmlData.TryGetField("internalid", Guid.TryParse, out _guidId);
            objXmlData.TryGetStringFieldQuickly("category", ref _strCategory);
            XmlNodeList xmlEffectsList = objXmlData.SelectNodes("effects/effect");
            if (xmlEffectsList?.Count > 0)
            {
                foreach (XmlNode objXmlLevel in xmlEffectsList)
                {
                    DrugEffect objDrugEffect = new DrugEffect();
                    objXmlLevel.TryGetField("level", out int effectLevel);
                    objDrugEffect.Level = effectLevel;
                    XmlNodeList xmlEffectChildNodeList = objXmlLevel.SelectNodes("*");
                    if (xmlEffectChildNodeList?.Count > 0)
                    {
                        foreach (XmlNode objXmlEffect in xmlEffectChildNodeList)
                        {
                            string strEffectName = string.Empty;
                            objXmlEffect.TryGetStringFieldQuickly("name", ref strEffectName);
                            switch (objXmlEffect.Name)
                            {
                                case "attribute":
                                    {
                                        int intEffectValue = 0;
                                        if (!string.IsNullOrEmpty(strEffectName) && objXmlEffect.TryGetInt32FieldQuickly("value", ref intEffectValue))
                                            objDrugEffect.Attributes[strEffectName] = intEffectValue;
                                    }
                                    break;

                                case "limit":
                                    {
                                        int intEffectValue = 0;
                                        if (!string.IsNullOrEmpty(strEffectName) && objXmlEffect.TryGetInt32FieldQuickly("value", ref intEffectValue))
                                            objDrugEffect.Limits[strEffectName] = intEffectValue;
                                        break;
                                    }
                                case "quality":
                                    objDrugEffect.Qualities.Add(objXmlEffect);
                                    break;

                                case "info":
                                    objDrugEffect.Infos.Add(objXmlEffect.InnerText);
                                    break;

                                case "initiative":
                                    {
                                        if (int.TryParse(objXmlEffect.InnerText, out int intInnerText))
                                            objDrugEffect.Initiative = intInnerText;
                                        break;
                                    }
                                case "initiativedice":
                                    {
                                        if (int.TryParse(objXmlEffect.InnerText, out int intInnerText))
                                            objDrugEffect.InitiativeDice = intInnerText;
                                        break;
                                    }
                                case "crashdamage":
                                    {
                                        if (int.TryParse(objXmlEffect.InnerText, out int intInnerText))
                                            objDrugEffect.CrashDamage = intInnerText;
                                        break;
                                    }
                                case "speed":
                                    {
                                        if (int.TryParse(objXmlEffect.InnerText, out int intInnerText))
                                            objDrugEffect.Speed = intInnerText;
                                        break;
                                    }
                                case "duration":
                                    {
                                        if (int.TryParse(objXmlEffect.InnerText, out int intInnerText))
                                            objDrugEffect.Duration = intInnerText;
                                        break;
                                    }
                                default:
                                    Log.Warn("Unknown drug effect " + objXmlEffect.Name + " in component " + strEffectName);
                                    break;
                            }
                        }
                    }

                    DrugEffects.Add(objDrugEffect);
                }
            }

            objXmlData.TryGetStringFieldQuickly("availability", ref _strAvailability);
            objXmlData.TryGetStringFieldQuickly("cost", ref _strCost);
            objXmlData.TryGetInt32FieldQuickly("level", ref _intLevel);
            objXmlData.TryGetInt32FieldQuickly("limit", ref _intLimit);
            objXmlData.TryGetInt32FieldQuickly("rating", ref _intAddictionRating);
            objXmlData.TryGetInt32FieldQuickly("threshold", ref _intAddictionThreshold);
            objXmlData.TryGetStringFieldQuickly("source", ref _strSource);
            objXmlData.TryGetStringFieldQuickly("page", ref _strPage);
        }

        public void Save(XmlWriter objXmlWriter)
        {
            if (objXmlWriter == null)
                return;
            objXmlWriter.WriteElementString("sourceid", SourceIDString);
            objXmlWriter.WriteElementString("guid", InternalId);
            objXmlWriter.WriteElementString("name", _strName);
            objXmlWriter.WriteElementString("category", _strCategory);

            objXmlWriter.WriteStartElement("effects");
            foreach (DrugEffect objDrugEffect in DrugEffects)
            {
                objXmlWriter.WriteStartElement("effect");
                foreach (KeyValuePair<string, decimal> objAttribute in objDrugEffect.Attributes)
                {
                    objXmlWriter.WriteStartElement("attribute");
                    objXmlWriter.WriteElementString("name", objAttribute.Key);
                    objXmlWriter.WriteElementString("value", objAttribute.Value.ToString(GlobalSettings.InvariantCultureInfo));
                    objXmlWriter.WriteEndElement();
                }
                foreach (KeyValuePair<string, int> objLimit in objDrugEffect.Limits)
                {
                    objXmlWriter.WriteStartElement("limit");
                    objXmlWriter.WriteElementString("name", objLimit.Key);
                    objXmlWriter.WriteElementString("value", objLimit.Value.ToString(GlobalSettings.InvariantCultureInfo));
                    objXmlWriter.WriteEndElement();
                }
                foreach (XmlNode nodQuality in objDrugEffect.Qualities)
                {
                    objXmlWriter.WriteRaw("<quality>" + nodQuality.InnerXml + "</quality>");
                }
                foreach (string strInfo in objDrugEffect.Infos)
                {
                    objXmlWriter.WriteElementString("info", strInfo);
                }
                if (objDrugEffect.Initiative != 0)
                    objXmlWriter.WriteElementString("initiative", objDrugEffect.Initiative.ToString(GlobalSettings.InvariantCultureInfo));
                if (objDrugEffect.InitiativeDice != 0)
                    objXmlWriter.WriteElementString("initiativedice", objDrugEffect.InitiativeDice.ToString(GlobalSettings.InvariantCultureInfo));
                if (objDrugEffect.Duration != 0)
                    objXmlWriter.WriteElementString("duration", objDrugEffect.Duration.ToString(GlobalSettings.InvariantCultureInfo));
                if (objDrugEffect.Speed != 0)
                    objXmlWriter.WriteElementString("speed", objDrugEffect.Speed.ToString(GlobalSettings.InvariantCultureInfo));
                if (objDrugEffect.CrashDamage != 0)
                    objXmlWriter.WriteElementString("crashdamage", objDrugEffect.CrashDamage.ToString(GlobalSettings.InvariantCultureInfo));
                objXmlWriter.WriteEndElement();
            }
            objXmlWriter.WriteEndElement();

            objXmlWriter.WriteElementString("availability", _strAvailability);
            objXmlWriter.WriteElementString("cost", _strCost);
            objXmlWriter.WriteElementString("level", _intLevel.ToString(GlobalSettings.InvariantCultureInfo));
            objXmlWriter.WriteElementString("limit", _intLimit.ToString(GlobalSettings.InvariantCultureInfo));
            if (_intAddictionRating != 0)
                objXmlWriter.WriteElementString("rating", _intAddictionRating.ToString(GlobalSettings.InvariantCultureInfo));
            if (_intAddictionThreshold != 0)
                objXmlWriter.WriteElementString("threshold", _intAddictionThreshold.ToString(GlobalSettings.InvariantCultureInfo));
            objXmlWriter.WriteElementString("source", _strSource);
            objXmlWriter.WriteElementString("page", _strPage);
        }

        #endregion Constructor, Create, Save, Load, and Print Methods

        #region Properties

        /// <summary>
        /// Drug Component's English Name
        /// </summary>
        public string Name
        {
            get => _strName;
            set => _strName = value;
        }

        /// <summary>
        /// The name of the object as it should appear on printouts (translated name only).
        /// </summary>
        public string DisplayNameShort(string strLanguage)
        {
            if (strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                return Name;

            XPathNavigator xmlGearDataNode = this.GetNodeXPath(strLanguage);
            if (xmlGearDataNode?.SelectSingleNodeAndCacheExpression("name")?.Value == "Custom Item")
            {
                return _objCharacter.TranslateExtra(Name, strLanguage);
            }

            return xmlGearDataNode?.SelectSingleNodeAndCacheExpression("translate")?.Value ?? Name;
        }

        /// <summary>
        /// The name of the object as it should be displayed in lists. Name (Level X).
        /// </summary>
        public string DisplayName(CultureInfo objCulture, string strLanguage)
        {
            string strReturn = DisplayNameShort(strLanguage);
            if (Level != 0)
            {
                string strSpace = LanguageManager.GetString("String_Space", strLanguage);
                strReturn += strSpace + '(' + LanguageManager.GetString("String_Level", strLanguage) + strSpace + Level.ToString(objCulture) + ')';
            }
            return strReturn;
        }

        public string CurrentDisplayName => DisplayName(GlobalSettings.CultureInfo, GlobalSettings.Language);

        public string CurrentDisplayNameShort => DisplayNameShort(GlobalSettings.Language);

        /// <summary>
        /// The name of the object as it should appear on printouts (translated name only).
        /// </summary>
        public async Task<string> DisplayNameShortAsync(string strLanguage, CancellationToken token = default)
        {
            if (strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                return Name;

            XPathNavigator xmlGearDataNode = await this.GetNodeXPathAsync(strLanguage, token: token).ConfigureAwait(false);
            if (xmlGearDataNode?.SelectSingleNodeAndCacheExpression("name", token)?.Value == "Custom Item")
            {
                return await _objCharacter.TranslateExtraAsync(Name, strLanguage, token: token).ConfigureAwait(false);
            }

            return xmlGearDataNode?.SelectSingleNodeAndCacheExpression("translate", token)?.Value ?? Name;
        }

        /// <summary>
        /// The name of the object as it should be displayed in lists. Name (Level X).
        /// </summary>
        public async Task<string> DisplayNameAsync(CultureInfo objCulture, string strLanguage, CancellationToken token = default)
        {
            string strReturn = await DisplayNameShortAsync(strLanguage, token).ConfigureAwait(false);
            if (Level != 0)
            {
                string strSpace = await LanguageManager.GetStringAsync("String_Space", strLanguage, token: token).ConfigureAwait(false);
                strReturn += strSpace + '(' + await LanguageManager.GetStringAsync("String_Level", strLanguage, token: token).ConfigureAwait(false) + strSpace + Level.ToString(objCulture) + ')';
            }
            return strReturn;
        }

        public Task<string> GetCurrentDisplayNameAsync(CancellationToken token = default) => DisplayNameAsync(GlobalSettings.CultureInfo, GlobalSettings.Language, token);

        public Task<string> GetCurrentDisplayNameShortAsync(CancellationToken token = default) => DisplayNameShortAsync(GlobalSettings.Language, token);

        /// <summary>
        /// Translated Category.
        /// </summary>
        public string DisplayCategory(string strLanguage)
        {
            if (strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                return Category;

            return _objCharacter.LoadDataXPath("drugcomponents.xml", strLanguage)
                                .SelectSingleNodeAndCacheExpression(
                                    "/chummer/categories/category[. = " + Category.CleanXPath() + "]/@translate")?.Value
                   ?? Category;
        }

        /// <summary>
        /// Category
        /// </summary>
        public string Category
        {
            get => _strCategory;
            set => _strCategory = value;
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
            string s = this.GetNodeXPath(strLanguage)?.SelectSingleNodeAndCacheExpression("altpage")?.Value ?? Page;
            return !string.IsNullOrWhiteSpace(s) ? s : Page;
        }

        public List<DrugEffect> DrugEffects { get; } = new List<DrugEffect>();

        public DrugEffect ActiveDrugEffect => DrugEffects.Find(effect => effect.Level == Level);

        public string Cost
        {
            get => _strCost;
            set => _strCost = value;
        }

        /// <summary>
        /// Cost of the drug component per level
        /// </summary>
        public decimal CostPerLevel
        {
            get
            {
                string strCostExpression = Cost;
                if (string.IsNullOrEmpty(strCostExpression))
                    return 0;

                strCostExpression = strCostExpression.ProcessFixedValuesString(Level);

                if (strCostExpression.DoesNeedXPathProcessingToBeConvertedToNumber(out decimal decReturn))
                {
                    using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdCost))
                    {
                        sbdCost.Append(strCostExpression.TrimStart('+'));
                        sbdCost.Replace("Level", Level.ToString(GlobalSettings.InvariantCultureInfo));
                        _objCharacter.ProcessAttributesInXPath(sbdCost, strCostExpression);
                        (bool blnIsSuccess, object objProcess)
                            = CommonFunctions.EvaluateInvariantXPath(sbdCost.ToString());
                        if (blnIsSuccess)
                            decReturn = Convert.ToDecimal((double)objProcess);
                    }
                }

                return decReturn;
            }
        }

        /// <summary>
        /// Cost of the drug component per level
        /// </summary>
        public async Task<decimal> GetCostPerLevelAsync(CancellationToken token = default)
        {
            string strCostExpression = Cost;
            if (string.IsNullOrEmpty(strCostExpression))
                return 0;

            strCostExpression = strCostExpression.ProcessFixedValuesString(Level);

            if (strCostExpression.DoesNeedXPathProcessingToBeConvertedToNumber(out decimal decReturn))
            {
                using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdCost))
                {
                    sbdCost.Append(strCostExpression.TrimStart('+'));
                    sbdCost.Replace("Level", Level.ToString(GlobalSettings.InvariantCultureInfo));
                    await _objCharacter.ProcessAttributesInXPathAsync(sbdCost, strCostExpression, token: token).ConfigureAwait(false);
                    (bool blnIsSuccess, object objProcess)
                        = await CommonFunctions.EvaluateInvariantXPathAsync(sbdCost.ToString(), token).ConfigureAwait(false);
                    if (blnIsSuccess)
                        decReturn = Convert.ToDecimal((double)objProcess);
                }
            }

            return decReturn;
        }

        public string Availability
        {
            get => _strAvailability;
            set => _strAvailability = value;
        }

        /// <summary>
        /// Total Availability in the program's current language.
        /// </summary>
        public string DisplayTotalAvail => TotalAvail(GlobalSettings.CultureInfo, GlobalSettings.Language);

        /// <summary>
        /// Total Availability in the program's current language.
        /// </summary>
        public Task<string> GetDisplayTotalAvailAsync(CancellationToken token = default) => TotalAvailAsync(GlobalSettings.CultureInfo, GlobalSettings.Language, token);

        /// <summary>
        /// Total Availability.
        /// </summary>
        public string TotalAvail(CultureInfo objCulture, string strLanguage)
        {
            return TotalAvailTuple.ToString(objCulture, strLanguage);
        }

        /// <summary>
        /// Calculated Availability of the Vehicle.
        /// </summary>
        public async Task<string> TotalAvailAsync(CultureInfo objCulture, string strLanguage, CancellationToken token = default)
        {
            return await (await GetTotalAvailTupleAsync(token: token).ConfigureAwait(false)).ToStringAsync(objCulture, strLanguage, token).ConfigureAwait(false);
        }

        /// <summary>
        /// Total Availability as a triple.
        /// </summary>
        public AvailabilityValue TotalAvailTuple
        {
            get
            {
                bool blnModifyParentAvail = false;
                string strAvail = Availability;
                char chrLastAvailChar = ' ';
                int intAvail = 0;
                if (strAvail.Length > 0)
                {
                    chrLastAvailChar = strAvail[strAvail.Length - 1];
                    if (chrLastAvailChar == 'F' || chrLastAvailChar == 'R')
                    {
                        strAvail = strAvail.Substring(0, strAvail.Length - 1);
                    }

                    blnModifyParentAvail = strAvail.StartsWith('+', '-');
                    if (strAvail.DoesNeedXPathProcessingToBeConvertedToNumber(out decimal decValue))
                    {
                        using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdAvail))
                        {
                            sbdAvail.Append(strAvail.TrimStart('+'));
                            _objCharacter.ProcessAttributesInXPath(sbdAvail, strAvail);
                            (bool blnIsSuccess, object objProcess)
                                = CommonFunctions.EvaluateInvariantXPath(sbdAvail.ToString());
                            if (blnIsSuccess)
                                intAvail += ((double)objProcess).StandardRound();
                        }
                    }
                    else
                        intAvail += decValue.StandardRound();
                }

                if (intAvail < 0)
                    intAvail = 0;

                return new AvailabilityValue(intAvail, chrLastAvailChar, blnModifyParentAvail);
            }
        }

        /// <summary>
        /// Total Availability as a triple.
        /// </summary>
        public async Task<AvailabilityValue> GetTotalAvailTupleAsync(CancellationToken token = default)
        {
            bool blnModifyParentAvail = false;
            string strAvail = Availability;
            char chrLastAvailChar = ' ';
            int intAvail = 0;
            if (strAvail.Length > 0)
            {
                chrLastAvailChar = strAvail[strAvail.Length - 1];
                if (chrLastAvailChar == 'F' || chrLastAvailChar == 'R')
                {
                    strAvail = strAvail.Substring(0, strAvail.Length - 1);
                }

                blnModifyParentAvail = strAvail.StartsWith('+', '-');
                if (strAvail.DoesNeedXPathProcessingToBeConvertedToNumber(out decimal decValue))
                {
                    using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdAvail))
                    {
                        sbdAvail.Append(strAvail.TrimStart('+'));
                        await _objCharacter.ProcessAttributesInXPathAsync(sbdAvail, strAvail, token: token).ConfigureAwait(false);
                        (bool blnIsSuccess, object objProcess)
                            = await CommonFunctions.EvaluateInvariantXPathAsync(sbdAvail.ToString(), token).ConfigureAwait(false);
                        if (blnIsSuccess)
                            intAvail += ((double)objProcess).StandardRound();
                    }
                }
                else
                    intAvail += decValue.StandardRound();
            }

            if (intAvail < 0)
                intAvail = 0;

            return new AvailabilityValue(intAvail, chrLastAvailChar, blnModifyParentAvail);
        }

        public int AddictionThreshold
        {
            get => _intAddictionThreshold;
            set => _intAddictionThreshold = value;
        }

        public int AddictionRating
        {
            get => _intAddictionRating;
            set => _intAddictionRating = value;
        }

        public int Level
        {
            get => _intLevel;
            set => _intLevel = value;
        }

        /// <summary>
        /// Amount of this drug component that is allowed to be in a complete drug recipe. If 0, assume unlimited.
        /// </summary>
        public int Limit
        {
            get => _intLimit;
            set => _intLimit = value;
        }

        /// <summary>
        /// Identifier of the object within data files.
        /// </summary>
        public Guid SourceID => _guiSourceID;

        /// <summary>
        /// String-formatted identifier of the <inheritdoc cref="SourceID"/> from the data files.
        /// </summary>
        public string SourceIDString => _guiSourceID.ToString("D", GlobalSettings.InvariantCultureInfo);

        public string InternalId => _guidId.ToString("D", GlobalSettings.InvariantCultureInfo);

        #endregion Properties

        #region Methods

        public string GenerateDescription(int intLevel = -1)
        {
            if (intLevel >= DrugEffects.Count)
                return null;

            using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool,
                                                          out StringBuilder sbdDescription))
            {
                string strSpace = LanguageManager.GetString("String_Space");
                string strColon = LanguageManager.GetString("String_Colon");
                sbdDescription.Append(DisplayCategory(GlobalSettings.Language)).Append(strColon).Append(strSpace)
                              .AppendLine(CurrentDisplayName);

                if (intLevel != -1)
                {
                    DrugEffect objDrugEffect = DrugEffects[intLevel];
                    bool blnNewLineFlag = false;
                    foreach (KeyValuePair<string, decimal> objAttribute in objDrugEffect.Attributes)
                    {
                        if (objAttribute.Value != 0)
                        {
                            if (blnNewLineFlag)
                            {
                                sbdDescription.Append(',').Append(strSpace);
                            }

                            sbdDescription
                                .Append(LanguageManager.GetString("String_Attribute" + objAttribute.Key + "Short"))
                                .Append(strSpace)
                                .Append(objAttribute.Value.ToString("+#;-#", GlobalSettings.CultureInfo));
                            blnNewLineFlag = true;
                        }
                    }

                    if (blnNewLineFlag)
                    {
                        blnNewLineFlag = false;
                        sbdDescription.AppendLine();
                    }

                    foreach (KeyValuePair<string, int> objLimit in objDrugEffect.Limits)
                    {
                        if (objLimit.Value != 0)
                        {
                            if (blnNewLineFlag)
                            {
                                sbdDescription.Append(',').Append(strSpace);
                            }

                            sbdDescription.Append(LanguageManager.GetString("Node_" + objLimit.Key)).Append(strSpace)
                                          .Append(LanguageManager.GetString("String_Limit")).Append(strSpace)
                                          .Append(objLimit.Value.ToString("+#;-#", GlobalSettings.CultureInfo));
                            blnNewLineFlag = true;
                        }
                    }

                    if (blnNewLineFlag)
                    {
                        sbdDescription.AppendLine();
                    }

                    if (objDrugEffect.Initiative != 0 || objDrugEffect.InitiativeDice != 0)
                    {
                        sbdDescription.Append(LanguageManager.GetString("String_AttributeINILong")).Append(strSpace);
                        if (objDrugEffect.Initiative != 0)
                        {
                            sbdDescription.Append(
                                objDrugEffect.Initiative.ToString("+#;-#", GlobalSettings.CultureInfo));
                            if (objDrugEffect.InitiativeDice != 0)
                                sbdDescription
                                    .Append(objDrugEffect.InitiativeDice.ToString("+#;-#", GlobalSettings.CultureInfo))
                                    .Append(LanguageManager.GetString("String_D6"));
                        }
                        else if (objDrugEffect.InitiativeDice != 0)
                            sbdDescription
                                .Append(objDrugEffect.InitiativeDice.ToString("+#;-#", GlobalSettings.CultureInfo))
                                .Append(LanguageManager.GetString("String_D6"));

                        sbdDescription.AppendLine();
                    }

                    foreach (XmlNode strQuality in objDrugEffect.Qualities)
                        sbdDescription.Append(_objCharacter.TranslateExtra(strQuality.InnerText)).Append(strSpace)
                                      .AppendLine(LanguageManager.GetString("String_Quality"));
                    foreach (string strInfo in objDrugEffect.Infos)
                        sbdDescription.AppendLine(_objCharacter.TranslateExtra(strInfo));

                    if (Category == "Custom Drug" || objDrugEffect.Duration != 0)
                        sbdDescription.Append(LanguageManager.GetString("Label_Duration")).Append(strColon)
                                      .Append(strSpace)
                                      .Append("10 ⨯ ")
                                      .Append((objDrugEffect.Duration + 1).ToString(GlobalSettings.CultureInfo))
                                      .Append(LanguageManager.GetString("String_D6")).Append(strSpace)
                                      .AppendLine(LanguageManager.GetString("String_Minutes"));

                    if (Category == "Custom Drug" || objDrugEffect.Speed != 0)
                    {
                        sbdDescription.Append(LanguageManager.GetString("Label_Speed")).Append(strColon)
                                      .Append(strSpace);
                        if (objDrugEffect.Speed <= 0)
                            sbdDescription.AppendLine(LanguageManager.GetString("String_Immediate"));
                        else if (objDrugEffect.Speed <= 60)
                            sbdDescription.Append((objDrugEffect.Speed / 3).ToString(GlobalSettings.CultureInfo))
                                          .Append(strSpace).AppendLine(LanguageManager.GetString("String_CombatTurns"));
                        else
                            sbdDescription.Append(objDrugEffect.Speed.ToString(GlobalSettings.CultureInfo))
                                          .AppendLine(LanguageManager.GetString("String_Seconds"));
                    }

                    if (objDrugEffect.CrashDamage != 0)
                        sbdDescription.Append(LanguageManager.GetString("Label_CrashEffect")).Append(strSpace)
                                      .Append(objDrugEffect.CrashDamage.ToString(GlobalSettings.CultureInfo))
                                      .Append(LanguageManager.GetString("String_DamageStun")).Append(strSpace)
                                      .AppendLine(LanguageManager.GetString("String_DamageUnresisted"));

                    sbdDescription.Append(LanguageManager.GetString("Label_AddictionRating")).Append(strSpace)
                                  .AppendLine((AddictionRating * (intLevel + 1)).ToString(GlobalSettings.CultureInfo));
                    sbdDescription.Append(LanguageManager.GetString("Label_AddictionThreshold")).Append(strSpace)
                                  .AppendLine(
                                      (AddictionThreshold * (intLevel + 1)).ToString(GlobalSettings.CultureInfo));
                    sbdDescription.Append(LanguageManager.GetString("Label_Cost")).Append(strSpace)
                                  .Append((CostPerLevel * (intLevel + 1)).ToString(
                                              _objCharacter.Settings.NuyenFormat, GlobalSettings.CultureInfo))
                                  .AppendLine(LanguageManager.GetString("String_NuyenSymbol"));
                    sbdDescription.Append(LanguageManager.GetString("Label_Avail")).Append(strSpace)
                                  .AppendLine(DisplayTotalAvail);
                }
                else
                {
                    string strPerLevel = LanguageManager.GetString("String_PerLevel");
                    sbdDescription.Append(LanguageManager.GetString("Label_AddictionRating")).Append(strSpace)
                                  .Append(0.ToString(GlobalSettings.CultureInfo)).Append(strSpace)
                                  .AppendLine(strPerLevel);
                    sbdDescription.Append(LanguageManager.GetString("Label_AddictionThreshold")).Append(strSpace)
                                  .Append(0.ToString(GlobalSettings.CultureInfo)).Append(strSpace)
                                  .AppendLine(strPerLevel);
                    sbdDescription.Append(LanguageManager.GetString("Label_Cost")).Append(strSpace)
                                  .Append((CostPerLevel * (intLevel + 1)).ToString(
                                              _objCharacter.Settings.NuyenFormat, GlobalSettings.CultureInfo))
                                  .Append(LanguageManager.GetString("String_NuyenSymbol"))
                                  .Append(strSpace).AppendLine(strPerLevel);
                    sbdDescription.Append(LanguageManager.GetString("Label_Avail")).Append(strSpace)
                                  .AppendLine(DisplayTotalAvail);
                }

                return sbdDescription.ToString();
            }
        }

        public async Task<string> GenerateDescriptionAsync(int intLevel = -1, CancellationToken token = default)
        {
            if (intLevel >= DrugEffects.Count)
                return null;

            using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool,
                                                          out StringBuilder sbdDescription))
            {
                string strSpace = await LanguageManager.GetStringAsync("String_Space", token: token).ConfigureAwait(false);
                string strColon = await LanguageManager.GetStringAsync("String_Colon", token: token).ConfigureAwait(false);
                sbdDescription.Append(DisplayCategory(GlobalSettings.Language)).Append(strColon).Append(strSpace)
                              .AppendLine(CurrentDisplayName);

                if (intLevel != -1)
                {
                    DrugEffect objDrugEffect = DrugEffects[intLevel];
                    bool blnNewLineFlag = false;
                    foreach (KeyValuePair<string, decimal> objAttribute in objDrugEffect.Attributes)
                    {
                        if (objAttribute.Value != 0)
                        {
                            if (blnNewLineFlag)
                            {
                                sbdDescription.Append(',').Append(strSpace);
                            }

                            sbdDescription
                                .Append(await LanguageManager.GetStringAsync("String_Attribute" + objAttribute.Key + "Short", token: token).ConfigureAwait(false))
                                .Append(strSpace)
                                .Append(objAttribute.Value.ToString("+#;-#", GlobalSettings.CultureInfo));
                            blnNewLineFlag = true;
                        }
                    }

                    if (blnNewLineFlag)
                    {
                        blnNewLineFlag = false;
                        sbdDescription.AppendLine();
                    }

                    foreach (KeyValuePair<string, int> objLimit in objDrugEffect.Limits)
                    {
                        if (objLimit.Value != 0)
                        {
                            if (blnNewLineFlag)
                            {
                                sbdDescription.Append(',').Append(strSpace);
                            }

                            sbdDescription.Append(await LanguageManager.GetStringAsync("Node_" + objLimit.Key, token: token).ConfigureAwait(false)).Append(strSpace)
                                          .Append(await LanguageManager.GetStringAsync("String_Limit", token: token).ConfigureAwait(false)).Append(strSpace)
                                          .Append(objLimit.Value.ToString("+#;-#", GlobalSettings.CultureInfo));
                            blnNewLineFlag = true;
                        }
                    }

                    if (blnNewLineFlag)
                    {
                        sbdDescription.AppendLine();
                    }

                    if (objDrugEffect.Initiative != 0 || objDrugEffect.InitiativeDice != 0)
                    {
                        sbdDescription.Append(await LanguageManager.GetStringAsync("String_AttributeINILong", token: token).ConfigureAwait(false)).Append(strSpace);
                        if (objDrugEffect.Initiative != 0)
                        {
                            sbdDescription.Append(
                                objDrugEffect.Initiative.ToString("+#;-#", GlobalSettings.CultureInfo));
                            if (objDrugEffect.InitiativeDice != 0)
                                sbdDescription
                                    .Append(objDrugEffect.InitiativeDice.ToString("+#;-#", GlobalSettings.CultureInfo))
                                    .Append(await LanguageManager.GetStringAsync("String_D6", token: token).ConfigureAwait(false));
                        }
                        else if (objDrugEffect.InitiativeDice != 0)
                            sbdDescription
                                .Append(objDrugEffect.InitiativeDice.ToString("+#;-#", GlobalSettings.CultureInfo))
                                .Append(await LanguageManager.GetStringAsync("String_D6", token: token).ConfigureAwait(false));

                        sbdDescription.AppendLine();
                    }

                    foreach (XmlNode strQuality in objDrugEffect.Qualities)
                        sbdDescription.Append(await _objCharacter.TranslateExtraAsync(strQuality.InnerText, token: token).ConfigureAwait(false)).Append(strSpace)
                                      .AppendLine(await LanguageManager.GetStringAsync("String_Quality", token: token).ConfigureAwait(false));
                    foreach (string strInfo in objDrugEffect.Infos)
                        sbdDescription.AppendLine(await _objCharacter.TranslateExtraAsync(strInfo, token: token).ConfigureAwait(false));

                    if (Category == "Custom Drug" || objDrugEffect.Duration != 0)
                        sbdDescription.Append(await LanguageManager.GetStringAsync("Label_Duration", token: token).ConfigureAwait(false)).Append(strColon)
                                      .Append(strSpace)
                                      .Append("10 ⨯ ")
                                      .Append((objDrugEffect.Duration + 1).ToString(GlobalSettings.CultureInfo))
                                      .Append(await LanguageManager.GetStringAsync("String_D6", token: token).ConfigureAwait(false)).Append(strSpace)
                                      .AppendLine(await LanguageManager.GetStringAsync("String_Minutes", token: token).ConfigureAwait(false));

                    if (Category == "Custom Drug" || objDrugEffect.Speed != 0)
                    {
                        sbdDescription.Append(await LanguageManager.GetStringAsync("Label_Speed", token: token).ConfigureAwait(false)).Append(strColon)
                                      .Append(strSpace);
                        if (objDrugEffect.Speed <= 0)
                            sbdDescription.AppendLine(await LanguageManager.GetStringAsync("String_Immediate", token: token).ConfigureAwait(false));
                        else if (objDrugEffect.Speed <= 60)
                            sbdDescription.Append((objDrugEffect.Speed / 3).ToString(GlobalSettings.CultureInfo))
                                          .Append(strSpace).AppendLine(await LanguageManager.GetStringAsync("String_CombatTurns", token: token).ConfigureAwait(false));
                        else
                            sbdDescription.Append(objDrugEffect.Speed.ToString(GlobalSettings.CultureInfo))
                                          .AppendLine(await LanguageManager.GetStringAsync("String_Seconds", token: token).ConfigureAwait(false));
                    }

                    if (objDrugEffect.CrashDamage != 0)
                        sbdDescription.Append(await LanguageManager.GetStringAsync("Label_CrashEffect", token: token).ConfigureAwait(false)).Append(strSpace)
                                      .Append(objDrugEffect.CrashDamage.ToString(GlobalSettings.CultureInfo))
                                      .Append(await LanguageManager.GetStringAsync("String_DamageStun", token: token).ConfigureAwait(false)).Append(strSpace)
                                      .AppendLine(await LanguageManager.GetStringAsync("String_DamageUnresisted", token: token).ConfigureAwait(false));

                    sbdDescription.Append(await LanguageManager.GetStringAsync("Label_AddictionRating", token: token).ConfigureAwait(false)).Append(strSpace)
                                  .AppendLine((AddictionRating * (intLevel + 1)).ToString(GlobalSettings.CultureInfo));
                    sbdDescription.Append(await LanguageManager.GetStringAsync("Label_AddictionThreshold", token: token).ConfigureAwait(false)).Append(strSpace)
                                  .AppendLine(
                                      (AddictionThreshold * (intLevel + 1)).ToString(GlobalSettings.CultureInfo));
                    string strNuyenFormat = await (await _objCharacter.GetSettingsAsync(token).ConfigureAwait(false)).GetNuyenFormatAsync(token).ConfigureAwait(false);
                    sbdDescription.Append(await LanguageManager.GetStringAsync("Label_Cost", token: token).ConfigureAwait(false)).Append(strSpace)
                                  .Append((await GetCostPerLevelAsync(token).ConfigureAwait(false) * (intLevel + 1)).ToString(
                                             strNuyenFormat, GlobalSettings.CultureInfo))
                                  .AppendLine(await LanguageManager.GetStringAsync("String_NuyenSymbol", token: token).ConfigureAwait(false));
                    sbdDescription.Append(await LanguageManager.GetStringAsync("Label_Avail", token: token).ConfigureAwait(false)).Append(strSpace)
                                  .AppendLine(DisplayTotalAvail);
                }
                else
                {
                    string strPerLevel = await LanguageManager.GetStringAsync("String_PerLevel", token: token).ConfigureAwait(false);
                    sbdDescription.Append(await LanguageManager.GetStringAsync("Label_AddictionRating", token: token).ConfigureAwait(false)).Append(strSpace)
                                  .Append(0.ToString(GlobalSettings.CultureInfo)).Append(strSpace)
                                  .AppendLine(strPerLevel);
                    sbdDescription.Append(await LanguageManager.GetStringAsync("Label_AddictionThreshold", token: token).ConfigureAwait(false)).Append(strSpace)
                                  .Append(0.ToString(GlobalSettings.CultureInfo)).Append(strSpace)
                                  .AppendLine(strPerLevel);
                    string strNuyenFormat = await (await _objCharacter.GetSettingsAsync(token).ConfigureAwait(false)).GetNuyenFormatAsync(token).ConfigureAwait(false);
                    sbdDescription.Append(await LanguageManager.GetStringAsync("Label_Cost", token: token).ConfigureAwait(false)).Append(strSpace)
                                  .Append((await GetCostPerLevelAsync(token).ConfigureAwait(false) * (intLevel + 1)).ToString(
                                              strNuyenFormat, GlobalSettings.CultureInfo))
                                  .Append(await LanguageManager.GetStringAsync("String_NuyenSymbol", token: token).ConfigureAwait(false))
                                  .Append(strSpace).AppendLine(strPerLevel);
                    sbdDescription.Append(await LanguageManager.GetStringAsync("Label_Avail", token: token).ConfigureAwait(false)).Append(strSpace)
                                  .AppendLine(DisplayTotalAvail);
                }

                return sbdDescription.ToString();
            }
        }

        public async Task<XmlNode> GetNodeCoreAsync(bool blnSync, string strLanguage, CancellationToken token = default)
        {
            XmlNode objReturn = _objCachedMyXmlNode;
            if (objReturn != null && strLanguage == _strCachedXmlNodeLanguage
                                  && !GlobalSettings.LiveCustomData)
                return objReturn;
            XmlDocument objDoc = blnSync
                // ReSharper disable once MethodHasAsyncOverload
                ? _objCharacter.LoadData("drugcomponents.xml", strLanguage, token: token)
                : await _objCharacter.LoadDataAsync("drugcomponents.xml", strLanguage, token: token).ConfigureAwait(false);
            if (SourceID != Guid.Empty)
                objReturn = objDoc.TryGetNodeById("/chummer/drugcomponents/drugcomponent", SourceID);
            if (objReturn == null)
            {
                objReturn = objDoc.TryGetNodeByNameOrId("/chummer/drugcomponents/drugcomponent", Name);
                objReturn?.TryGetGuidFieldQuickly("id", ref _guiSourceID);
            }
            _objCachedMyXmlNode = objReturn;
            _strCachedXmlNodeLanguage = strLanguage;
            return objReturn;
        }

        private XPathNavigator _objCachedMyXPathNode;
        private string _strCachedXPathNodeLanguage = string.Empty;

        public async Task<XPathNavigator> GetNodeXPathCoreAsync(bool blnSync, string strLanguage, CancellationToken token = default)
        {
            XPathNavigator objReturn = _objCachedMyXPathNode;
            if (objReturn != null && strLanguage == _strCachedXPathNodeLanguage
                                  && !GlobalSettings.LiveCustomData)
                return objReturn;
            XPathNavigator objDoc = blnSync
                // ReSharper disable once MethodHasAsyncOverload
                ? _objCharacter.LoadDataXPath("drugcomponents.xml", strLanguage, token: token)
                : await _objCharacter.LoadDataXPathAsync("drugcomponents.xml", strLanguage, token: token).ConfigureAwait(false);
            if (SourceID != Guid.Empty)
                objReturn = objDoc.TryGetNodeById("/chummer/drugcomponents/drugcomponent", SourceID);
            if (objReturn == null)
            {
                objReturn = objDoc.TryGetNodeByNameOrId("/chummer/drugcomponents/drugcomponent", Name);
                objReturn?.TryGetGuidFieldQuickly("id", ref _guiSourceID);
            }
            _objCachedMyXPathNode = objReturn;
            _strCachedXPathNodeLanguage = strLanguage;
            return objReturn;
        }

        #endregion Methods
    }

    /// <summary>
    /// Drug Effect
    /// </summary>
    public class DrugEffect
    {
        public Dictionary<string, decimal> Attributes { get; } = new Dictionary<string, decimal>();

        public Dictionary<string, int> Limits { get; } = new Dictionary<string, int>();

        public List<XmlNode> Qualities { get; } = new List<XmlNode>();

        public List<string> Infos { get; } = new List<string>();

        public int Initiative { get; set; }

        public int InitiativeDice { get; set; }

        public int CrashDamage { get; set; }

        public int Speed { get; set; }

        public int Duration { get; set; }

        public int Level { get; set; }
    }
}
