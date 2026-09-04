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
    public sealed class Drug : IHasName, IHasSourceId, IHasXmlDataNode, ICanSort, IHasStolenProperty, ICanBlackMarketDiscount, ICanRemove, IDisposable, IAsyncDisposable, IHasCharacterObject, IHasNotes, IHasInternalId
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
        private bool _blnDiscountCost;
        private bool _blnCachedAttributeFlag;
        private XmlNode _objCachedMyXmlNode;
        private string _strCachedXmlNodeLanguage;
        private string _strSource;
        private string _strPage;
        private int _intDurationDice;
        private string _strNotes = string.Empty;
        private Color _colNotes = ColorManager.HasNotesColor;
        private Cyberware _objParentCyberware;

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
            NormalizeCustomDrugsCategory(ref _strCategory);
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
            objXmlData.TryGetBoolFieldQuickly("discountedcost", ref _blnDiscountCost);
            objXmlData.TryGetStringFieldQuickly("duration", ref _strDuration);
            objXmlData.TryGetInt32FieldQuickly("durationdice", ref _intDurationDice);
            DurationTimescale = CommonFunctions.ConvertStringToTimescale(objXmlData["timescale"]?.InnerTextViaPool());

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
            Utils.SafelyRunSynchronously(() => LoadCoreAsync(true, objXmlData));
        }

        public Task LoadAsync(XmlNode objXmlData, CancellationToken token = default)
        {
            return LoadCoreAsync(false, objXmlData, token);
        }

        private async Task LoadCoreAsync(bool blnSync, XmlNode objXmlData, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (!objXmlData.TryGetField("guid", Guid.TryParse, out _guiID))
                _guiID = Guid.NewGuid();
            objXmlData.TryGetStringFieldQuickly("name", ref _strName);
            _objCachedMyXmlNode = null;
            _objCachedMyXPathNode = null;
            if (!objXmlData.TryGetGuidFieldQuickly("sourceid", ref _guiSourceID))
            {
                // ReSharper disable once MethodHasAsyncOverload
                (blnSync ? this.GetNodeXPath(token) : await this.GetNodeXPathAsync(token).ConfigureAwait(false))?.TryGetGuidFieldQuickly("id", ref _guiSourceID);
            }
            objXmlData.TryGetStringFieldQuickly("category", ref _strCategory);
            NormalizeCustomDrugsCategory(ref _strCategory);
            Grade = blnSync
                // ReSharper disable once MethodHasAsyncOverload
                ? Grade.ConvertToCyberwareGrade(objXmlData["grade"]?.InnerTextViaPool(token), Improvement.ImprovementSource.Drug, _objCharacter, token)
                : await Grade.ConvertToCyberwareGradeAsync(objXmlData["grade"]?.InnerTextViaPool(token), Improvement.ImprovementSource.Drug, _objCharacter, token).ConfigureAwait(false);

            XmlNodeList xmlComponentsNodeList = objXmlData.SelectNodes("drugcomponents/drugcomponent");
            if (xmlComponentsNodeList?.Count > 0)
            {
                if (blnSync)
                {
                    foreach (XmlNode objXmlLevel in xmlComponentsNodeList)
                    {
                        DrugComponent c = new DrugComponent(_objCharacter);
                        // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                        c.Load(objXmlLevel);
                        // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                        Components.Add(c);
                    }
                }
                else
                {
                    foreach (XmlNode objXmlLevel in xmlComponentsNodeList)
                    {
                        DrugComponent c = new DrugComponent(_objCharacter);
                        await c.LoadAsync(objXmlLevel, token).ConfigureAwait(false);
                        await Components.AddAsync(c, token).ConfigureAwait(false);
                    }
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
            objXmlData.TryGetBoolFieldQuickly("discountedcost", ref _blnDiscountCost);
            objXmlData.TryGetField("source", out _strSource);
            objXmlData.TryGetField("page", out _strPage);
            objXmlData.TryGetMultiLineStringFieldQuickly("notes", ref _strNotes);

            string sNotesColor = ColorTranslator.ToHtml(ColorManager.HasNotesColor);
            objXmlData.TryGetStringFieldQuickly("notesColor", ref sNotesColor);
            _colNotes = ColorTranslator.FromHtml(sNotesColor);
        }

        /// <summary>
        /// Creates a character drug from a premade catalog entry in drugs.xml.
        /// Catalog &lt;bonus&gt; is the source of effects. A matching drugcomponent is attached only when the catalog entry has no bonus.
        /// </summary>
        /// <param name="objCharacter">Character receiving the drug.</param>
        /// <param name="strCatalogId">Catalog id from <c>/chummer/drugs/drug</c>.</param>
        /// <param name="objGrade">Selected drug grade.</param>
        /// <param name="intRating">Selected rating when applicable.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        /// <returns>The new drug, or null when the catalog id is unknown.</returns>
        public static async Task<Drug> CreateFromCatalogAsync(Character objCharacter, string strCatalogId,
            Grade objGrade, int intRating = 0, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (objCharacter == null)
                throw new ArgumentNullException(nameof(objCharacter));
            if (string.IsNullOrEmpty(strCatalogId))
                throw new ArgumentException("Catalog id is required.", nameof(strCatalogId));

            XmlNode objCatalogNode =
                await DrugsData.GetCatalogDrugNodeByNameOrIdAsync(objCharacter, strCatalogId, token)
                    .ConfigureAwait(false);
            if (objCatalogNode == null)
                return null;

            Drug objDrug = new Drug(objCharacter);
            objCatalogNode.TryGetStringFieldQuickly("name", ref objDrug._strName);
            objCatalogNode.TryGetStringFieldQuickly("category", ref objDrug._strCategory);
            NormalizeCustomDrugsCategory(ref objDrug._strCategory);
            if (!objCatalogNode.TryGetStringFieldQuickly("availability", ref objDrug._strAvailability))
                objCatalogNode.TryGetStringFieldQuickly("avail", ref objDrug._strAvailability);
            objCatalogNode.TryGetDecFieldQuickly("cost", ref objDrug._decCost);
            objCatalogNode.TryGetInt32FieldQuickly("rating", ref objDrug._intAddictionRating);
            objCatalogNode.TryGetInt32FieldQuickly("threshold", ref objDrug._intAddictionThreshold);
            objCatalogNode.TryGetField("source", out objDrug._strSource);
            objCatalogNode.TryGetField("page", out objDrug._strPage);
            if (objCatalogNode.TryGetField("id", Guid.TryParse, out Guid guiCatalogId))
                objDrug._guiSourceID = guiCatalogId;

            if (objGrade != null)
                objDrug.Grade = objGrade;

            if (intRating > 0)
                objDrug._intAddictionRating = intRating;

            objCatalogNode.TryGetStringFieldQuickly("duration", ref objDrug._strDuration);
            int intCatalogSpeed = 0;
            if (objCatalogNode.TryGetInt32FieldQuickly("speed", ref intCatalogSpeed))
                objDrug._intCachedSpeed = intCatalogSpeed;

            if (objDrug._guiSourceID != Guid.Empty)
            {
                XmlDocument objComponentDoc =
                    await objCharacter.LoadDataAsync(DrugsData.ComponentsFileName, token: token).ConfigureAwait(false);
                XmlNode objComponentNode =
                    objComponentDoc.TryGetNodeById(DrugsData.ComponentXPath, objDrug._guiSourceID);
                if (objComponentNode != null && objCatalogNode["bonus"] == null)
                {
                    DrugComponent objComponent = new DrugComponent(objCharacter);
                    await objComponent.LoadAsync(objComponentNode, token).ConfigureAwait(false);
                    await objDrug.Components.AddAsync(objComponent, token: token).ConfigureAwait(false);
                }
            }

            objDrug._decQty = 1;
            return objDrug;
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
            objXmlWriter.WriteElementString("discountedcost", _blnDiscountCost.ToString(GlobalSettings.InvariantCultureInfo));
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
                    foreach (string strQualityText in (await GetQualitiesAsync(token).ConfigureAwait(false)).Select(x => x.InnerTextViaPool(token)))
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
            if (string.IsNullOrEmpty(_strEffectDescription))
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

        public async Task SetNameAsync(string value, CancellationToken token = default)
        {
            _strName = await _objCharacter.ReverseTranslateExtraAsync(value, token: token).ConfigureAwait(false);
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
            set
            {
                _strCategory = value;
                NormalizeCustomDrugsCategory(ref _strCategory);
            }
        }

        /// <summary>
        /// Whether <paramref name="strCategory"/> is Custom Drugs, including the legacy singular name.
        /// </summary>
        /// <param name="strCategory">Category to test.</param>
        /// <returns>True if the category is Custom Drugs or the legacy Custom Drug name.</returns>
        public static bool IsCustomDrugsCategory(string strCategory) =>
            string.Equals(strCategory, "Custom Drugs", StringComparison.OrdinalIgnoreCase)
            || string.Equals(strCategory, "Custom Drug", StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Rewrites the legacy singular Custom Drug category to Custom Drugs.
        /// </summary>
        /// <param name="strCategory">Category string to normalize in place.</param>
        private static void NormalizeCustomDrugsCategory(ref string strCategory)
        {
            if (string.Equals(strCategory, "Custom Drug", StringComparison.OrdinalIgnoreCase))
                strCategory = "Custom Drugs";
        }

        /// <summary>
        /// Drug category for improvements whose <see cref="Improvement.SourceName"/> is the granting drug's internal id.
        /// </summary>
        /// <param name="objCharacter">Character owning the drug.</param>
        /// <param name="strDrugInternalId">Internal id stored on the drug improvement's <see cref="Improvement.SourceName"/>.</param>
        /// <param name="dicDrugCategoryByInternalId">Optional lookup built from <see cref="Character.Drugs"/> and nested cyberware drugs.</param>
        /// <returns>The drug's category, or an empty string if not found.</returns>
        public static string GetCategoryForDrugSource(Character objCharacter, string strDrugInternalId,
            IReadOnlyDictionary<string, string> dicDrugCategoryByInternalId = null)
        {
            if (string.IsNullOrEmpty(strDrugInternalId))
                return string.Empty;
            if (dicDrugCategoryByInternalId != null
                && dicDrugCategoryByInternalId.TryGetValue(strDrugInternalId, out string strCachedCategory))
                return strCachedCategory ?? string.Empty;
            if (objCharacter == null)
                return string.Empty;
            Drug objDrug = objCharacter.Drugs.FirstOrDefault(x => x.InternalId == strDrugInternalId);
            if (objDrug != null)
                return objDrug.Category ?? string.Empty;
            return FindNestedDrug(objCharacter, strDrugInternalId)?.Category ?? string.Empty;
        }

        /// <summary>
        /// Finds a drug nested under cyberware/bioware <see cref="Cyberware.DrugChildren"/>.
        /// </summary>
        /// <param name="objCharacter">Character to search.</param>
        /// <param name="strDrugInternalId">Internal id of the nested drug.</param>
        /// <returns>The matching drug, or null if not found.</returns>
        public static Drug FindNestedDrug(Character objCharacter, string strDrugInternalId)
        {
            if (objCharacter == null || string.IsNullOrEmpty(strDrugInternalId))
                return null;
            return FindNestedDrug(objCharacter.Cyberware, strDrugInternalId);
        }

        private static Drug FindNestedDrug(IEnumerable<Cyberware> lstWare, string strDrugInternalId)
        {
            foreach (Cyberware objWare in lstWare)
            {
                Drug objFound = objWare.DrugChildren.FirstOrDefault(x => x.InternalId == strDrugInternalId);
                if (objFound != null)
                    return objFound;
                objFound = FindNestedDrug(objWare.Children, strDrugInternalId);
                if (objFound != null)
                    return objFound;
            }

            return null;
        }

        /// <summary>
        /// Adds categories for all drugs nested under cyberware/bioware into a lookup dictionary.
        /// </summary>
        /// <param name="objCharacter">Character whose ware to scan.</param>
        /// <param name="dicDrugCategoryByInternalId">Dictionary to populate.</param>
        public static void AddNestedDrugCategoriesToLookup(Character objCharacter,
            IDictionary<string, string> dicDrugCategoryByInternalId)
        {
            if (objCharacter == null || dicDrugCategoryByInternalId == null)
                return;
            AddNestedDrugCategoriesToLookup(objCharacter.Cyberware, dicDrugCategoryByInternalId);
        }

        private static void AddNestedDrugCategoriesToLookup(IEnumerable<Cyberware> lstWare,
            IDictionary<string, string> dicDrugCategoryByInternalId)
        {
            foreach (Cyberware objWare in lstWare)
            {
                foreach (Drug objDrug in objWare.DrugChildren)
                    dicDrugCategoryByInternalId[objDrug.InternalId] = objDrug.Category ?? string.Empty;
                AddNestedDrugCategoriesToLookup(objWare.Children, dicDrugCategoryByInternalId);
            }
        }

        /// <summary>
        /// Adds categories for all drugs nested under cyberware/bioware into a lookup dictionary.
        /// </summary>
        /// <param name="objCharacter">Character whose ware to scan.</param>
        /// <param name="dicDrugCategoryByInternalId">Dictionary to populate.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public static async Task AddNestedDrugCategoriesToLookupAsync(Character objCharacter,
            IDictionary<string, string> dicDrugCategoryByInternalId, CancellationToken token = default)
        {
            if (objCharacter == null || dicDrugCategoryByInternalId == null)
                return;
            await AddNestedDrugCategoriesToLookupAsync(
                await objCharacter.GetCyberwareAsync(token).ConfigureAwait(false),
                dicDrugCategoryByInternalId, token).ConfigureAwait(false);
        }

        private static async Task AddNestedDrugCategoriesToLookupAsync(
            ThreadSafeObservableCollection<Cyberware> lstWare,
            IDictionary<string, string> dicDrugCategoryByInternalId, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            await lstWare.ForEachAsync(async objWare =>
            {
                await (await objWare.GetDrugChildrenAsync(token).ConfigureAwait(false)).ForEachAsync(objDrug =>
                {
                    dicDrugCategoryByInternalId[objDrug.InternalId] = objDrug.Category ?? string.Empty;
                }, token).ConfigureAwait(false);
                await AddNestedDrugCategoriesToLookupAsync(
                    await objWare.GetChildrenAsync(token).ConfigureAwait(false),
                    dicDrugCategoryByInternalId, token).ConfigureAwait(false);
            }, token).ConfigureAwait(false);
        }

        /// <summary>
        /// Whether a <see cref="Improvement.ImprovementType.DrugPositiveAttributeModifier"/> filter applies to a drug category.
        /// "All" or "*" matches every category. An empty filter matches only drugs with no category. Otherwise the filter must equal the drug category.
        /// Singular and plural Custom Drug(s) are treated as the same category for legacy saves.
        /// </summary>
        /// <param name="strFilter">
        /// ImprovedName of the DrugPositiveAttributeModifier improvement.
        /// </param>
        /// <param name="strDrugCategory">
        /// Category of the drug that granted the attribute bonus.
        /// </param>
        /// <returns>
        /// True if the modifier should increase positive attribute bonuses from that category.
        /// </returns>
        public static bool PositiveAttributeModifierAppliesToCategory(string strFilter, string strDrugCategory)
        {
            if (strFilter == "*" || strFilter.Equals("All", StringComparison.OrdinalIgnoreCase))
                return true;
            if (string.IsNullOrEmpty(strFilter))
                return string.IsNullOrEmpty(strDrugCategory);
            if (string.Equals(strFilter, strDrugCategory, StringComparison.OrdinalIgnoreCase))
                return true;
            return IsCustomDrugsCategory(strFilter) && IsCustomDrugsCategory(strDrugCategory);
        }

        private bool IncludeDefaultDurationAndSpeed =>
            IsCustomDrugsCategory(Category)
            || string.Equals(Category, "BTLs", StringComparison.OrdinalIgnoreCase);

        private decimal _decCachedCost = decimal.MinValue;

        /// <summary>
        /// Base cost of the Drug.
        /// </summary>
        public decimal Cost
        {
            get
            {
                if (_decCachedCost != decimal.MinValue)
                    return _decCachedCost;
                decimal decReturn = Components.Count > 0
                    ? Components.Sum(d => d.ActiveDrugEffect != null, d => d.CostPerLevel)
                    : _decCost;
                if (DiscountCost)
                    decReturn *= 0.9m;
                return _decCachedCost = decReturn;
            }
        }

        /// <summary>
        /// Base cost of the Drug.
        /// </summary>
        public async Task<decimal> GetCostAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (_decCachedCost != decimal.MinValue)
                return _decCachedCost;
            decimal decReturn = Components.Count > 0
                ? await Components.SumAsync(d => d.ActiveDrugEffect != null,
                    d => d.GetCostPerLevelAsync(token), token).ConfigureAwait(false)
                : _decCost;
            if (DiscountCost)
                decReturn *= 0.9m;
            return _decCachedCost = decReturn;
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
        /// Cyberware that contains this drug (e.g. a Chemical Gland), if any.
        /// </summary>
        public Cyberware ParentCyberware
        {
            get => _objParentCyberware;
            set => _objParentCyberware = value;
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
                strAvail = strAvail.TrimStart('+');
                if (strAvail.DoesNeedXPathProcessingToBeConvertedToNumber(out decimal decValue))
                {
                    strAvail = _objCharacter.ProcessAttributesInXPath(strAvail);
                    (bool blnIsSuccess, object objProcess)
                        = CommonFunctions.EvaluateInvariantXPath(strAvail);
                    if (blnIsSuccess)
                        intAvail += ((double)objProcess).StandardRound();
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

            intAvail += ImprovementManager.ValueOf(_objCharacter, Improvement.ImprovementType.Availability, strImprovedName: SourceIDString, blnIncludeNonImproved: true).StandardRound();

            if (intAvail < 0)
                intAvail = 0;

            return new AvailabilityValue(intAvail, chrLastAvailChar, blnModifyParentAvail);
        }

        /// <summary>
        /// Total Availability as a triple.
        /// </summary>
        public async Task<AvailabilityValue> TotalAvailTupleAsync(bool blnCheckChildren = true, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
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
                strAvail = strAvail.TrimStart('+');
                if (strAvail.DoesNeedXPathProcessingToBeConvertedToNumber(out decimal decValue))
                {
                    strAvail = await _objCharacter.ProcessAttributesInXPathAsync(strAvail, token: token).ConfigureAwait(false);
                    (bool blnIsSuccess, object objProcess)
                        = await CommonFunctions.EvaluateInvariantXPathAsync(strAvail, token).ConfigureAwait(false);
                    if (blnIsSuccess)
                        intAvail += ((double)objProcess).StandardRound();
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

            intAvail += (await ImprovementManager.ValueOfAsync(_objCharacter, Improvement.ImprovementType.Availability, strImprovedName: SourceIDString, blnIncludeNonImproved: true, token: token).ConfigureAwait(false)).StandardRound();

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
            return _intCachedAddictionThreshold != int.MinValue
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
                    strDuration = _objCharacter.ProcessAttributesInXPath(strDuration);
                    (bool blnIsSuccess, object objProcess) = CommonFunctions.EvaluateInvariantXPath(strDuration);
                    if (blnIsSuccess)
                        decDuration = Convert.ToDecimal((double)objProcess);
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
                strDuration = await _objCharacter
                        .ProcessAttributesInXPathAsync(strDuration, token: token).ConfigureAwait(false);
                (bool blnIsSuccess, object objProcess) = await CommonFunctions.EvaluateInvariantXPathAsync(strDuration, token).ConfigureAwait(false);
                if (blnIsSuccess)
                    decDuration = Convert.ToDecimal((double)objProcess);
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
            bool blnDoCache = strLanguage.Equals(GlobalSettings.Language, StringComparison.OrdinalIgnoreCase) && ReferenceEquals(objCulture, GlobalSettings.CultureInfo);
            if (!string.IsNullOrWhiteSpace(_strCachedDisplayDuration) && blnDoCache)
                return _strCachedDisplayDuration;
            int intDuration = Duration;
            if (intDuration > 0)
            {
                string strSpace = LanguageManager.GetString("String_Space", strLanguage);
                string strDisplayDuration = intDuration.ToString(objCulture) + strSpace;
                if (DurationDice > 0)
                {
                    strDisplayDuration += "×" + strSpace + DurationDice.ToString(objCulture) +
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
            bool blnDoCache = strLanguage.Equals(GlobalSettings.Language, StringComparison.OrdinalIgnoreCase) && ReferenceEquals(objCulture, GlobalSettings.CultureInfo);
            if (!string.IsNullOrWhiteSpace(_strCachedDisplayDuration) && blnDoCache)
                return _strCachedDisplayDuration;
            int intDuration = await GetDurationAsync(token).ConfigureAwait(false);
            if (intDuration > 0)
            {
                string strSpace = await LanguageManager.GetStringAsync("String_Space", strLanguage, token: token).ConfigureAwait(false);
                string strDisplayDuration = intDuration.ToString(objCulture) + strSpace;
                if (DurationDice > 0)
                {
                    strDisplayDuration += "×" + strSpace + DurationDice.ToString(objCulture) +
                                          await LanguageManager.GetStringAsync("String_D6", strLanguage, token: token).ConfigureAwait(false) + strSpace;
                }
                return _strCachedDisplayDuration = strDisplayDuration + await CommonFunctions.GetTimescaleStringAsync(DurationTimescale, intDuration > 1, token: token).ConfigureAwait(false);
            }
            return _strCachedDisplayDuration = await CommonFunctions.GetTimescaleStringAsync(DurationTimescale, false, token: token).ConfigureAwait(false);
        }

        public int DurationDice { get => _intDurationDice; set => _intDurationDice = value; }

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
            return _intCachedCrashDamage != int.MinValue
                    ? _intCachedCrashDamage
                    : _intCachedCrashDamage = await Components.SumAsync(d => d.ActiveDrugEffect != null, d => d.ActiveDrugEffect.CrashDamage, token).ConfigureAwait(false);
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

        private decimal GetDisplayAttributeModifier(decimal decValue)
        {
            if (decValue <= 0)
                return decValue;
            return decValue + GetPositiveAttributeModifierBonus();
        }

        private async Task<decimal> GetDisplayAttributeModifierAsync(decimal decValue, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            if (decValue <= 0)
                return decValue;
            return decValue + await GetPositiveAttributeModifierBonusAsync(token).ConfigureAwait(false);
        }

        private decimal GetPositiveAttributeModifierBonus()
        {
            decimal decBonus = 0;
            foreach (Improvement objBonus in ImprovementManager.GetCachedImprovementListForValueOf(
                         _objCharacter, Improvement.ImprovementType.DrugPositiveAttributeModifier))
            {
                if (!PositiveAttributeModifierAppliesToCategory(objBonus.ImprovedName, Category))
                    continue;
                decBonus += (objBonus.Value != 0 ? objBonus.Value : objBonus.Augmented) * objBonus.Rating;
            }
            return decBonus;
        }

        private async Task<decimal> GetPositiveAttributeModifierBonusAsync(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            decimal decBonus = 0;
            foreach (Improvement objBonus in await ImprovementManager.GetCachedImprovementListForValueOfAsync(
                         _objCharacter, Improvement.ImprovementType.DrugPositiveAttributeModifier, token: token)
                         .ConfigureAwait(false))
            {
                token.ThrowIfCancellationRequested();
                if (!PositiveAttributeModifierAppliesToCategory(objBonus.ImprovedName, Category))
                    continue;
                decBonus += (objBonus.Value != 0 ? objBonus.Value : objBonus.Augmented) * objBonus.Rating;
            }
            return decBonus;
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

        /// <summary>
        /// Whether the Drug's cost should be discounted by 10% through the Black Market Pipeline Quality.
        /// </summary>
        public bool DiscountCost
        {
            get => _blnDiscountCost;
            set
            {
                if (_blnDiscountCost == value)
                    return;
                _blnDiscountCost = value;
                _decCachedCost = decimal.MinValue;
            }
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
                                sbdDescription.Append(',', strSpace);
                            }

                            sbdDescription.Append(LanguageManager.GetString("String_Attribute" + objAttribute.Key + "Short", strLanguage),
                                strSpace, GetDisplayAttributeModifier(objAttribute.Value).ToString("+#.#;-#.#", GlobalSettings.CultureInfo));
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
                                sbdDescription.Append(',', strSpace);
                            }

                            sbdDescription.Append(LanguageManager.GetString("Node_" + objLimit.Key, strLanguage),
                                strSpace, LanguageManager.GetString("String_Limit", strLanguage),
                                strSpace, objLimit.Value.ToString(" +#;-#", GlobalSettings.CultureInfo));
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
                        sbdDescription.Append(_objCharacter.TranslateExtra(nodQuality.InnerTextViaPool(), strLanguage))
                                      .Append(strSpace)
                                      .AppendLine(LanguageManager.GetString("String_Quality", strLanguage));
                    }

                    foreach (string strInfo in Infos)
                        sbdDescription.AppendLine(_objCharacter.TranslateExtra(strInfo, strLanguage));

                    if (IncludeDefaultDurationAndSpeed || Duration != 0)
                        sbdDescription.Append(LanguageManager.GetString("Label_Duration", strLanguage))
                                      .AppendLine(GetDisplayDuration(objCulture, strLanguage));

                    if (IncludeDefaultDurationAndSpeed || Speed != 0)
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
                                sbdDescription.Append(',', strSpace);
                            }

                            sbdDescription.Append(await LanguageManager.GetStringAsync("String_Attribute" + objAttribute.Key + "Short", strLanguage, token: token).ConfigureAwait(false),
                                strSpace, (await GetDisplayAttributeModifierAsync(objAttribute.Value, token).ConfigureAwait(false)).ToString("+#.#;-#.#", GlobalSettings.CultureInfo));
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
                                sbdDescription.Append(',', strSpace);
                            }

                            sbdDescription.Append(await LanguageManager.GetStringAsync("Node_" + objLimit.Key, strLanguage, token: token).ConfigureAwait(false),
                                strSpace, await LanguageManager.GetStringAsync("String_Limit", strLanguage, token: token).ConfigureAwait(false),
                                strSpace, objLimit.Value.ToString(" +#;-#", GlobalSettings.CultureInfo));
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
                        sbdDescription.Append(await _objCharacter.TranslateExtraAsync(nodQuality.InnerTextViaPool(token), strLanguage, token: token).ConfigureAwait(false))
                                      .Append(strSpace)
                                      .AppendLine(await LanguageManager.GetStringAsync("String_Quality", strLanguage, token: token).ConfigureAwait(false));
                    }

                    foreach (string strInfo in await GetInfosAsync(token).ConfigureAwait(false))
                        sbdDescription.AppendLine(await _objCharacter.TranslateExtraAsync(strInfo, strLanguage, token: token).ConfigureAwait(false));

                    if (IncludeDefaultDurationAndSpeed || await GetDurationAsync(token).ConfigureAwait(false) != 0)
                        sbdDescription.Append(await LanguageManager.GetStringAsync("Label_Duration", strLanguage, token: token).ConfigureAwait(false))
                                      .AppendLine(await GetDisplayDurationAsync(objCulture, strLanguage, token).ConfigureAwait(false));

                    int intSpeed = await GetSpeedAsync(token).ConfigureAwait(false);
                    if (IncludeDefaultDurationAndSpeed || intSpeed != 0)
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
        /// Improvement group name for this dose. Nested gland drugs append the parent ware name so they
        /// do not share a CustomGroup with a Drugs-tab dose of the same catalog name.
        /// </summary>
        /// <returns>The custom improvement group name for this drug instance.</returns>
        public string GetImprovementGroupName()
        {
            if (_objParentCyberware == null)
                return Name;
            string strParent = _objParentCyberware.CurrentDisplayNameShort;
            if (string.IsNullOrEmpty(strParent))
                strParent = _objParentCyberware.Name;
            return string.IsNullOrEmpty(strParent) ? Name : Name + " (" + strParent + ")";
        }

        /// <summary>
        /// Improvement group name for this dose. Nested gland drugs append the parent ware name so they
        /// do not share a CustomGroup with a Drugs-tab dose of the same catalog name.
        /// </summary>
        /// <param name="token">Cancellation token to listen to.</param>
        /// <returns>The custom improvement group name for this drug instance.</returns>
        public async Task<string> GetImprovementGroupNameAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (_objParentCyberware == null)
                return Name;
            string strParent = await _objParentCyberware.GetCurrentDisplayNameShortAsync(token).ConfigureAwait(false);
            if (string.IsNullOrEmpty(strParent))
                strParent = _objParentCyberware.Name;
            return string.IsNullOrEmpty(strParent) ? Name : Name + " (" + strParent + ")";
        }

        /// <summary>
        /// Creates disabled improvements for this drug dose (improvement group is enabled when the dose is taken).
        /// </summary>
        public async Task GenerateImprovement(CancellationToken token = default)
        {
            if (await _objCharacter.Improvements.AnyAsync(ig => ig.SourceName == InternalId, token: token)
                    .ConfigureAwait(false))
                return;

            string strGroupName = await GetImprovementGroupNameAsync(token).ConfigureAwait(false);
            await (await _objCharacter.GetImprovementGroupsAsync(token).ConfigureAwait(false))
                .AddAsync(strGroupName, token).ConfigureAwait(false);

            DrugBonusCompiler.CompileResult objCompileResult =
                await DrugBonusCompiler.CompileAsync(_objCharacter, this, token).ConfigureAwait(false);
            if (objCompileResult.BonusNode == null && objCompileResult.QualityNodes.Count == 0)
                return;

            string strDisplayName = await GetCurrentDisplayNameShortAsync(token).ConfigureAwait(false);

            if (objCompileResult.BonusNode?.HasChildNodes == true
                && !await ImprovementManager.CreateImprovementsAsync(
                    _objCharacter,
                    Improvement.ImprovementSource.Drug,
                    InternalId,
                    objCompileResult.BonusNode,
                    intRating: 1,
                    strDisplayName,
                    token: token).ConfigureAwait(false))
            {
                return;
            }

            await AddDrugQualityImprovementsAsync(objCompileResult.QualityNodes, strDisplayName, token)
                .ConfigureAwait(false);
            await FinalizeDrugImprovementsAsync(strGroupName, token).ConfigureAwait(false);
        }

        private async Task AddDrugQualityImprovementsAsync(IReadOnlyList<XmlNode> lstQualities, string strDisplayName,
            CancellationToken token = default)
        {
            if (lstQualities == null || lstQualities.Count == 0)
                return;

            string strSpace = await LanguageManager.GetStringAsync("String_Space", token: token).ConfigureAwait(false);
            string strNamePrefix = strDisplayName + strSpace + "-" + strSpace;
            XmlDocument objXmlDocument =
                await _objCharacter.LoadDataAsync("qualities.xml", token: token).ConfigureAwait(false);
            List<Improvement> lstImprovements = new List<Improvement>(lstQualities.Count);

            foreach (XmlNode objXmlAddQuality in lstQualities)
            {
                XmlNode objXmlSelectedQuality =
                    objXmlDocument.TryGetNodeByNameOrId("/chummer/qualities/quality",
                        objXmlAddQuality.InnerTextViaPool(token));
                if (objXmlSelectedQuality == null)
                    continue;
                XPathNavigator xpnSelectedQuality = objXmlSelectedQuality.CreateNavigator();
                string strForceValue = objXmlAddQuality.Attributes?["select"]?.InnerTextViaPool(token) ?? string.Empty;

                string strRating = objXmlAddQuality.Attributes?["rating"]?.InnerTextViaPool(token);
                int intCount = string.IsNullOrEmpty(strRating)
                    ? 1
                    : await ImprovementManager.ValueToIntAsync(_objCharacter, strRating, 1, token)
                        .ConfigureAwait(false);

                string strQualityName = objXmlAddQuality.InnerTextViaPool(token);
                string strQualityLabel =
                    await LanguageManager.GetStringAsync("String_Quality", token: token).ConfigureAwait(false);
                if (objXmlAddQuality.Attributes?["forced"]?.InnerTextIsTrueString() != true &&
                    !await xpnSelectedQuality.RequirementsMetAsync(_objCharacter,
                        strLocalName: strQualityLabel, strIgnoreQuality: Name, token: token).ConfigureAwait(false))
                {
                    throw new AbortedException();
                }

                string strCustomName = strNamePrefix + strQualityLabel + strSpace + strQualityName;
                if (intCount > 1)
                    strCustomName += strSpace + intCount.ToString(GlobalSettings.CultureInfo);
                Improvement objImprovement = new Improvement(_objCharacter)
                {
                    ImprovedName = strQualityName,
                    UniqueName = strQualityName,
                    Target = strForceValue,
                    ImproveSource = Improvement.ImprovementSource.Drug,
                    SourceName = InternalId,
                    ImproveType = Improvement.ImprovementType.SpecificQuality,
                    CustomName = strCustomName,
                    SetupComplete = true
                };
                await objImprovement.SetRatingAsync(Math.Max(1, intCount), token).ConfigureAwait(false);
                lstImprovements.Add(objImprovement);
            }

            _objCharacter.Improvements.AddRange(lstImprovements);
        }

        private async Task FinalizeDrugImprovementsAsync(string strGroupName, CancellationToken token = default)
        {
            List<Improvement> lstImprovements = await _objCharacter.Improvements
                .ToListAsync(x => x.SourceName == InternalId, token: token).ConfigureAwait(false);
            if (lstImprovements.Count == 0)
                return;

            string strDisplayName = await GetCurrentDisplayNameShortAsync(token).ConfigureAwait(false);
            string strSpace = await LanguageManager.GetStringAsync("String_Space", token: token).ConfigureAwait(false);

            // CreateImprovements adds entries before CustomGroup/CustomName are set, so the Improvements
            // tab nests them under Selected Improvements with blank labels. Remove and re-add after
            // filling those fields so they land under the drug's group with readable names.
            foreach (Improvement objImprovement in lstImprovements)
            {
                await _objCharacter.Improvements.RemoveAsync(objImprovement, token).ConfigureAwait(false);
            }

            foreach (Improvement objImprovement in lstImprovements)
            {
                if (string.IsNullOrEmpty(objImprovement.CustomName))
                {
                    objImprovement.CustomName = await BuildDrugImprovementCustomNameAsync(
                        objImprovement, strDisplayName, strSpace, token).ConfigureAwait(false);
                }

                objImprovement.CustomGroup = strGroupName;
                objImprovement.Custom = true;
                objImprovement.SetupComplete = true;
                await objImprovement.SetEnabledAsync(false, token).ConfigureAwait(false);
                await _objCharacter.Improvements.AddAsync(objImprovement, token).ConfigureAwait(false);
            }
        }

        private static async Task<string> BuildDrugImprovementCustomNameAsync(Improvement objImprovement,
            string strDisplayName, string strSpace, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            string strPrefix = strDisplayName + strSpace + "-" + strSpace;
            decimal decBonus = objImprovement.Augmented != 0 ? objImprovement.Augmented : objImprovement.Value;
            string strBonus = decBonus.ToString("+0;-0;0", GlobalSettings.CultureInfo);

            switch (objImprovement.ImproveType)
            {
                case Improvement.ImprovementType.Attribute:
                {
                    string strAttrKey = "String_Attribute" + objImprovement.ImprovedName + "Short";
                    string strAttr = await LanguageManager.GetStringAsync(strAttrKey, token: token)
                        .ConfigureAwait(false);
                    if (string.IsNullOrEmpty(strAttr) || string.Equals(strAttr, strAttrKey, StringComparison.Ordinal))
                        strAttr = objImprovement.ImprovedName;
                    return strPrefix + strAttr + strSpace + strBonus;
                }
                case Improvement.ImprovementType.PhysicalLimit:
                    return strPrefix
                           + await LanguageManager.GetStringAsync("Node_Physical", token: token).ConfigureAwait(false)
                           + strSpace + strBonus;
                case Improvement.ImprovementType.MentalLimit:
                    return strPrefix
                           + await LanguageManager.GetStringAsync("Node_Mental", token: token).ConfigureAwait(false)
                           + strSpace + strBonus;
                case Improvement.ImprovementType.SocialLimit:
                    return strPrefix
                           + await LanguageManager.GetStringAsync("Node_Social", token: token).ConfigureAwait(false)
                           + strSpace + strBonus;
                case Improvement.ImprovementType.Skill:
                case Improvement.ImprovementType.SkillBase:
                case Improvement.ImprovementType.SkillLevel:
                    return strPrefix + objImprovement.ImprovedName + strSpace + strBonus;
                case Improvement.ImprovementType.Initiative:
                    return strPrefix
                           + await LanguageManager.GetStringAsync("String_Initiative", token: token)
                               .ConfigureAwait(false)
                           + strSpace + strBonus;
                case Improvement.ImprovementType.InitiativeDice:
                    return strPrefix
                           + await LanguageManager.GetStringAsync("String_InitiativeDice", token: token)
                               .ConfigureAwait(false)
                           + strSpace + strBonus;
                case Improvement.ImprovementType.SpecificQuality:
                    return strPrefix
                           + await LanguageManager.GetStringAsync("String_Quality", token: token).ConfigureAwait(false)
                           + strSpace + objImprovement.ImprovedName;
                default:
                    return strPrefix + objImprovement.ImproveType + strSpace + strBonus;
            }
        }

        public async Task<XmlNode> GetNodeCoreAsync(bool blnSync, string strLanguage, CancellationToken token = default)
        {
            XmlNode objReturn = _objCachedMyXmlNode;
            if (objReturn != null && strLanguage == _strCachedXmlNodeLanguage
                                  && !GlobalSettings.LiveCustomData)
                return objReturn;
            if (SourceID != Guid.Empty)
            {
                objReturn = blnSync
                    ? DrugsData.GetCatalogDrugNode(_objCharacter, SourceID, strLanguage, token)
                    : await DrugsData.GetCatalogDrugNodeAsync(_objCharacter, SourceID, strLanguage, token)
                        .ConfigureAwait(false);
            }
            if (objReturn == null)
            {
                XmlDocument objDoc = blnSync
                    // ReSharper disable once MethodHasAsyncOverload
                    ? _objCharacter.LoadData(DrugsData.ComponentsFileName, strLanguage, token: token)
                    : await _objCharacter.LoadDataAsync(DrugsData.ComponentsFileName, strLanguage, token: token)
                        .ConfigureAwait(false);
                if (SourceID != Guid.Empty)
                    objReturn = objDoc.TryGetNodeById(DrugsData.ComponentXPath, SourceID);
                if (objReturn == null)
                {
                    objReturn = objDoc.TryGetNodeByNameOrId(DrugsData.ComponentXPath, Name);
                    objReturn?.TryGetGuidFieldQuickly("id", ref _guiSourceID);
                }
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
            if (SourceID != Guid.Empty)
            {
                XPathNavigator objCatalogDoc = blnSync
                    ? _objCharacter.LoadDataXPath(DrugsData.CatalogFileName, strLanguage, token: token)
                    : await _objCharacter.LoadDataXPathAsync(DrugsData.CatalogFileName, strLanguage, token: token)
                        .ConfigureAwait(false);
                objReturn = objCatalogDoc.TryGetNodeById(DrugsData.CatalogDrugXPath, SourceID);
                if (objReturn == null)
                {
                    XPathNavigator objLegacyDoc = blnSync
                        ? _objCharacter.LoadDataXPath(DrugsData.ComponentsFileName, strLanguage, token: token)
                        : await _objCharacter.LoadDataXPathAsync(DrugsData.ComponentsFileName, strLanguage, token: token)
                            .ConfigureAwait(false);
                    objReturn = objLegacyDoc.TryGetNodeById(DrugsData.CatalogDrugXPath, SourceID);
                }
            }
            if (objReturn == null)
            {
                XPathNavigator objDoc = blnSync
                    // ReSharper disable once MethodHasAsyncOverload
                    ? _objCharacter.LoadDataXPath(DrugsData.ComponentsFileName, strLanguage, token: token)
                    : await _objCharacter.LoadDataXPathAsync(DrugsData.ComponentsFileName, strLanguage, token: token)
                        .ConfigureAwait(false);
                if (SourceID != Guid.Empty)
                    objReturn = objDoc.TryGetNodeById(DrugsData.ComponentXPath, SourceID);
                if (objReturn == null)
                {
                    objReturn = objDoc.TryGetNodeByNameOrId(DrugsData.ComponentXPath, Name);
                    objReturn?.TryGetGuidFieldQuickly("id", ref _guiSourceID);
                }
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

            if (_objParentCyberware != null)
            {
                Cyberware objParent = _objParentCyberware;
                _objParentCyberware = null;
                objParent.DrugChildren.Remove(this);
            }
            else
            {
                _objCharacter.Drugs.Remove(this);
            }

            RemoveDrugImprovementsAndQualities();
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

            if (_objParentCyberware != null)
            {
                Cyberware objParent = _objParentCyberware;
                _objParentCyberware = null;
                await (await objParent.GetDrugChildrenAsync(token).ConfigureAwait(false))
                    .RemoveAsync(this, token).ConfigureAwait(false);
            }
            else
            {
                await _objCharacter.Drugs.RemoveAsync(this, token).ConfigureAwait(false);
            }

            await RemoveDrugImprovementsAndQualitiesAsync(token).ConfigureAwait(false);
            await DisposeAsync().ConfigureAwait(false);

            return true;
        }

        /// <summary>
        /// Removes Drug-sourced improvements and any leftover qualities granted by this drug.
        /// </summary>
        private void RemoveDrugImprovementsAndQualities()
        {
            string strGroupName = GetImprovementGroupName();
            ImprovementManager.RemoveImprovements(_objCharacter, Improvement.ImprovementSource.Drug, InternalId);
            // Fallback for character files loaded before Drug.guid was restored on Load (mismatched SourceName).
            // Scoped to this dose's group name so Drugs-tab and gland doses of the same drug do not wipe each other.
            List<Improvement> lstByGroup = _objCharacter.Improvements
                .Where(x => x.ImproveSource == Improvement.ImprovementSource.Drug && x.CustomGroup == strGroupName)
                .ToList();
            if (lstByGroup.Count > 0)
                ImprovementManager.RemoveImprovements(_objCharacter, lstByGroup);
            RemoveOrphanedDrugQualities();
            if (_objCharacter.ImprovementGroups.Contains(strGroupName)
                && !_objCharacter.Improvements.Any(x => x.CustomGroup == strGroupName))
                _objCharacter.ImprovementGroups.Remove(strGroupName);
        }

        /// <summary>
        /// Removes Drug-sourced improvements and any leftover qualities granted by this drug.
        /// </summary>
        private async Task RemoveDrugImprovementsAndQualitiesAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            string strGroupName = await GetImprovementGroupNameAsync(token).ConfigureAwait(false);
            await ImprovementManager
                .RemoveImprovementsAsync(_objCharacter, Improvement.ImprovementSource.Drug, InternalId, token)
                .ConfigureAwait(false);
            // Fallback for character files loaded before Drug.guid was restored on Load (mismatched SourceName).
            // Scoped to this dose's group name so Drugs-tab and gland doses of the same drug do not wipe each other.
            List<Improvement> lstByGroup = await _objCharacter.Improvements.ToListAsync(
                x => x.ImproveSource == Improvement.ImprovementSource.Drug && x.CustomGroup == strGroupName,
                token: token).ConfigureAwait(false);
            if (lstByGroup.Count > 0)
                await ImprovementManager.RemoveImprovementsAsync(_objCharacter, lstByGroup, token: token)
                    .ConfigureAwait(false);
            await RemoveOrphanedDrugQualitiesAsync(token).ConfigureAwait(false);
            ThreadSafeObservableCollection<string> lstGroups =
                await _objCharacter.GetImprovementGroupsAsync(token).ConfigureAwait(false);
            if (await lstGroups.ContainsAsync(strGroupName, token).ConfigureAwait(false)
                && !await _objCharacter.Improvements.AnyAsync(x => x.CustomGroup == strGroupName, token: token)
                    .ConfigureAwait(false))
                await lstGroups.RemoveAsync(strGroupName, token).ConfigureAwait(false);
        }

        private void RemoveOrphanedDrugQualities()
        {
            string strDrugName = Name;
            for (int i = _objCharacter.Qualities.Count - 1; i >= 0; --i)
            {
                if (i >= _objCharacter.Qualities.Count)
                    continue;
                Quality objQuality = _objCharacter.Qualities[i];
                if (objQuality.OriginSource != QualitySource.Improvement
                    || !string.Equals(objQuality.SourceName, strDrugName, StringComparison.Ordinal))
                    continue;
                // Skip if another SpecificQuality improvement still references this quality (shared source name).
                if (_objCharacter.Improvements.Any(x =>
                        x.ImproveType == Improvement.ImprovementType.SpecificQuality
                        && x.ImprovedName == objQuality.InternalId))
                    continue;
                objQuality.DeleteQuality();
            }
        }

        private async Task RemoveOrphanedDrugQualitiesAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            string strDrugName = Name;
            ThreadSafeObservableCollection<Quality> lstQualities =
                await _objCharacter.GetQualitiesAsync(token).ConfigureAwait(false);
            for (int i = await lstQualities.GetCountAsync(token).ConfigureAwait(false) - 1; i >= 0; --i)
            {
                if (i >= await lstQualities.GetCountAsync(token).ConfigureAwait(false))
                    continue;
                Quality objQuality = await lstQualities.GetValueAtAsync(i, token).ConfigureAwait(false);
                if (await objQuality.GetOriginSourceAsync(token).ConfigureAwait(false) != QualitySource.Improvement
                    || !string.Equals(await objQuality.GetSourceNameAsync(token).ConfigureAwait(false), strDrugName,
                        StringComparison.Ordinal))
                    continue;
                string strQualityId = objQuality.InternalId;
                if (await _objCharacter.Improvements.AnyAsync(x =>
                            x.ImproveType == Improvement.ImprovementType.SpecificQuality
                            && x.ImprovedName == strQualityId, token)
                        .ConfigureAwait(false))
                    continue;
                await objQuality.DeleteQualityAsync(token: token).ConfigureAwait(false);
            }
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

        /// <summary>
        /// Independent copy for a custom-drug recipe so Level changes do not leak across recipes.
        /// </summary>
        /// <returns>A new component with a new instance id and copied effects.</returns>
        public DrugComponent Clone()
        {
            DrugComponent objCopy = new DrugComponent(_objCharacter);
            objCopy._guiSourceID = _guiSourceID;
            objCopy._strName = _strName;
            objCopy._strCategory = _strCategory;
            objCopy._strAvailability = _strAvailability;
            objCopy._intLevel = _intLevel;
            objCopy._intLimit = _intLimit;
            objCopy._strSource = _strSource;
            objCopy._strPage = _strPage;
            objCopy._strCost = _strCost;
            objCopy._intAddictionThreshold = _intAddictionThreshold;
            objCopy._intAddictionRating = _intAddictionRating;
            foreach (DrugEffect objEffect in DrugEffects)
                objCopy.DrugEffects.Add(objEffect.Clone());
            return objCopy;
        }

        #region Constructor, Create, Save, Load, and Print Methods

        public void Load(XmlNode objXmlData)
        {
            Utils.SafelyRunSynchronously(() => LoadCoreAsync(true, objXmlData));
        }

        public Task LoadAsync(XmlNode objXmlData, CancellationToken token = default)
        {
            return LoadCoreAsync(false, objXmlData, token);
        }

        private async Task LoadCoreAsync(bool blnSync, XmlNode objXmlData, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            objXmlData.TryGetStringFieldQuickly("name", ref _strName);
            _objCachedMyXmlNode = null;
            _objCachedMyXPathNode = null;
            if (!objXmlData.TryGetGuidFieldQuickly("sourceid", ref _guiSourceID))
            {
                // ReSharper disable once MethodHasAsyncOverload
                (blnSync ? this.GetNodeXPath(token) : await this.GetNodeXPathAsync(token).ConfigureAwait(false))?.TryGetGuidFieldQuickly("id", ref _guiSourceID);
            }
            objXmlData.TryGetField("internalid", Guid.TryParse, out _guidId);
            objXmlData.TryGetStringFieldQuickly("category", ref _strCategory);
            // Legacy saves used singular "Custom Drug"
            if (string.Equals(_strCategory, "Custom Drug", StringComparison.OrdinalIgnoreCase))
                _strCategory = "Custom Drugs";
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
                            switch (objXmlEffect.Name.ToUpperInvariant())
                            {
                                case "ATTRIBUTE":
                                    {
                                        int intEffectValue = 0;
                                        if (!string.IsNullOrEmpty(strEffectName) && objXmlEffect.TryGetInt32FieldQuickly("value", ref intEffectValue))
                                            objDrugEffect.Attributes[strEffectName] = intEffectValue;
                                    }
                                    break;

                                case "LIMIT":
                                    {
                                        int intEffectValue = 0;
                                        if (!string.IsNullOrEmpty(strEffectName) && objXmlEffect.TryGetInt32FieldQuickly("value", ref intEffectValue))
                                            objDrugEffect.Limits[strEffectName] = intEffectValue;
                                        break;
                                    }
                                case "QUALITY":
                                    objDrugEffect.Qualities.Add(objXmlEffect);
                                    break;

                                case "SPECIFICSKILL":
                                    objDrugEffect.SpecificSkills.Add(objXmlEffect);
                                    break;

                                case "INFO":
                                    objDrugEffect.Infos.Add(objXmlEffect.InnerTextViaPool(token));
                                    break;

                                case "INITIATIVE":
                                    {
                                        if (int.TryParse(objXmlEffect.InnerTextViaPool(token), out int intInnerText))
                                            objDrugEffect.Initiative = intInnerText;
                                        break;
                                    }
                                case "INITIATIVEDICE":
                                    {
                                        if (int.TryParse(objXmlEffect.InnerTextViaPool(token), out int intInnerText))
                                            objDrugEffect.InitiativeDice = intInnerText;
                                        break;
                                    }
                                case "CRASHDAMAGE":
                                    {
                                        if (int.TryParse(objXmlEffect.InnerTextViaPool(token), out int intInnerText))
                                            objDrugEffect.CrashDamage = intInnerText;
                                        break;
                                    }
                                case "SPEED":
                                    {
                                        if (int.TryParse(objXmlEffect.InnerTextViaPool(token), out int intInnerText))
                                            objDrugEffect.Speed = intInnerText;
                                        break;
                                    }
                                case "DURATION":
                                    {
                                        if (int.TryParse(objXmlEffect.InnerTextViaPool(token), out int intInnerText))
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
                    if (!nodQuality.IsNullOrInnerTextIsEmpty())
                        objXmlWriter.WriteRaw("<quality>" + nodQuality.InnerXmlViaPool() + "</quality>");
                }
                foreach (XmlNode nodSkill in objDrugEffect.SpecificSkills)
                {
                    if (nodSkill != null)
                        objXmlWriter.WriteRaw(nodSkill.OuterXmlViaPool());
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
                strReturn += strSpace + "(" + LanguageManager.GetString("String_Level", strLanguage) + strSpace + Level.ToString(objCulture) + ")";
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
                strReturn += strSpace + "(" + await LanguageManager.GetStringAsync("String_Level", strLanguage, token: token).ConfigureAwait(false) + strSpace + Level.ToString(objCulture) + ")";
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

            return _objCharacter.LoadDataXPath(DrugsData.ComponentsFileName, strLanguage)
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
            set
            {
                _strCategory = value;
                if (string.Equals(_strCategory, "Custom Drug", StringComparison.OrdinalIgnoreCase))
                    _strCategory = "Custom Drugs";
            }
        }

        private bool IncludeDefaultDurationAndSpeed =>
            Drug.IsCustomDrugsCategory(Category)
            || string.Equals(Category, "BTLs", StringComparison.OrdinalIgnoreCase);

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

                strCostExpression = strCostExpression.ProcessFixedValuesString(Level).TrimStart('+')
                    .Replace("{Level}", Level.ToString(GlobalSettings.InvariantCultureInfo))
                    .Replace("Level", Level.ToString(GlobalSettings.InvariantCultureInfo));

                if (strCostExpression.DoesNeedXPathProcessingToBeConvertedToNumber(out decimal decReturn))
                {
                    strCostExpression = _objCharacter.ProcessAttributesInXPath(strCostExpression);
                    (bool blnIsSuccess, object objProcess)
                        = CommonFunctions.EvaluateInvariantXPath(strCostExpression);
                    if (blnIsSuccess)
                        decReturn = Convert.ToDecimal((double)objProcess);
                }

                return decReturn;
            }
        }

        /// <summary>
        /// Cost of the drug component per level
        /// </summary>
        public async Task<decimal> GetCostPerLevelAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            string strCostExpression = Cost;
            if (string.IsNullOrEmpty(strCostExpression))
                return 0;

            strCostExpression = strCostExpression.ProcessFixedValuesString(Level, token).TrimStart('+')
                .Replace("{Level}", Level.ToString(GlobalSettings.InvariantCultureInfo))
                .Replace("Level", Level.ToString(GlobalSettings.InvariantCultureInfo));

            if (strCostExpression.DoesNeedXPathProcessingToBeConvertedToNumber(out decimal decReturn))
            {
                strCostExpression = await _objCharacter.ProcessAttributesInXPathAsync(strCostExpression, token: token).ConfigureAwait(false);
                (bool blnIsSuccess, object objProcess)
                    = await CommonFunctions.EvaluateInvariantXPathAsync(strCostExpression, token).ConfigureAwait(false);
                if (blnIsSuccess)
                    decReturn = Convert.ToDecimal((double)objProcess);
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
                    strAvail = strAvail.TrimStart('+');
                    if (strAvail.DoesNeedXPathProcessingToBeConvertedToNumber(out decimal decValue))
                    {
                        strAvail = _objCharacter.ProcessAttributesInXPath(strAvail);
                        (bool blnIsSuccess, object objProcess)
                            = CommonFunctions.EvaluateInvariantXPath(strAvail);
                        if (blnIsSuccess)
                            intAvail += ((double)objProcess).StandardRound();
                    }
                    else
                        intAvail += decValue.StandardRound();
                }

                intAvail += ImprovementManager.ValueOf(_objCharacter, Improvement.ImprovementType.Availability, strImprovedName: SourceIDString, blnIncludeNonImproved: true).StandardRound();

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
            token.ThrowIfCancellationRequested();
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
                strAvail = strAvail.TrimStart('+');
                if (strAvail.DoesNeedXPathProcessingToBeConvertedToNumber(out decimal decValue))
                {
                    strAvail = await _objCharacter.ProcessAttributesInXPathAsync(strAvail, token: token).ConfigureAwait(false);
                    (bool blnIsSuccess, object objProcess)
                        = await CommonFunctions.EvaluateInvariantXPathAsync(strAvail, token).ConfigureAwait(false);
                    if (blnIsSuccess)
                        intAvail += ((double)objProcess).StandardRound();
                }
                else
                    intAvail += decValue.StandardRound();
            }

            intAvail += (await ImprovementManager.ValueOfAsync(_objCharacter, Improvement.ImprovementType.Availability, strImprovedName: SourceIDString, blnIncludeNonImproved: true, token: token).ConfigureAwait(false)).StandardRound();

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
                                sbdDescription.Append(',', strSpace);
                            }

                            sbdDescription.Append(LanguageManager.GetString("String_Attribute" + objAttribute.Key + "Short"),
                                strSpace, objAttribute.Value.ToString("+#;-#", GlobalSettings.CultureInfo));
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
                                sbdDescription.Append(',', strSpace);
                            }

                            sbdDescription.Append(LanguageManager.GetString("Node_" + objLimit.Key),
                                strSpace, LanguageManager.GetString("String_Limit"),
                                strSpace, objLimit.Value.ToString("+#;-#", GlobalSettings.CultureInfo));
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
                        sbdDescription.Append(_objCharacter.TranslateExtra(strQuality.InnerTextViaPool())).Append(strSpace)
                                      .AppendLine(LanguageManager.GetString("String_Quality"));
                    foreach (string strInfo in objDrugEffect.Infos)
                        sbdDescription.AppendLine(_objCharacter.TranslateExtra(strInfo));

                    if (IncludeDefaultDurationAndSpeed || objDrugEffect.Duration != 0)
                        sbdDescription.Append(LanguageManager.GetString("Label_Duration")).Append(strColon)
                                      .Append(strSpace)
                                      .Append("10 ⨯ ")
                                      .Append((objDrugEffect.Duration + 1).ToString(GlobalSettings.CultureInfo))
                                      .Append(LanguageManager.GetString("String_D6")).Append(strSpace)
                                      .AppendLine(LanguageManager.GetString("String_Minutes"));

                    if (IncludeDefaultDurationAndSpeed || objDrugEffect.Speed != 0)
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
                                sbdDescription.Append(',', strSpace);
                            }

                            sbdDescription.Append(await LanguageManager.GetStringAsync("String_Attribute" + objAttribute.Key + "Short", token: token).ConfigureAwait(false),
                                strSpace, objAttribute.Value.ToString("+#;-#", GlobalSettings.CultureInfo));
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
                                sbdDescription.Append(',', strSpace);
                            }

                            sbdDescription.Append(await LanguageManager.GetStringAsync("Node_" + objLimit.Key, token: token).ConfigureAwait(false),
                                strSpace, await LanguageManager.GetStringAsync("String_Limit", token: token).ConfigureAwait(false),
                                strSpace, objLimit.Value.ToString("+#;-#", GlobalSettings.CultureInfo));
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
                        sbdDescription.Append(await _objCharacter.TranslateExtraAsync(strQuality.InnerTextViaPool(token), token: token).ConfigureAwait(false)).Append(strSpace)
                                      .AppendLine(await LanguageManager.GetStringAsync("String_Quality", token: token).ConfigureAwait(false));
                    foreach (string strInfo in objDrugEffect.Infos)
                        sbdDescription.AppendLine(await _objCharacter.TranslateExtraAsync(strInfo, token: token).ConfigureAwait(false));

                    if (IncludeDefaultDurationAndSpeed || objDrugEffect.Duration != 0)
                        sbdDescription.Append(await LanguageManager.GetStringAsync("Label_Duration", token: token).ConfigureAwait(false)).Append(strColon)
                                      .Append(strSpace)
                                      .Append("10 ⨯ ")
                                      .Append((objDrugEffect.Duration + 1).ToString(GlobalSettings.CultureInfo))
                                      .Append(await LanguageManager.GetStringAsync("String_D6", token: token).ConfigureAwait(false)).Append(strSpace)
                                      .AppendLine(await LanguageManager.GetStringAsync("String_Minutes", token: token).ConfigureAwait(false));

                    if (IncludeDefaultDurationAndSpeed || objDrugEffect.Speed != 0)
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
                ? _objCharacter.LoadData(DrugsData.ComponentsFileName, strLanguage, token: token)
                : await _objCharacter.LoadDataAsync(DrugsData.ComponentsFileName, strLanguage, token: token)
                    .ConfigureAwait(false);
            if (SourceID != Guid.Empty)
                objReturn = objDoc.TryGetNodeById(DrugsData.ComponentXPath, SourceID);
            if (objReturn == null)
            {
                objReturn = objDoc.TryGetNodeByNameOrId(DrugsData.ComponentXPath, Name);
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
                ? _objCharacter.LoadDataXPath(DrugsData.ComponentsFileName, strLanguage, token: token)
                : await _objCharacter.LoadDataXPathAsync(DrugsData.ComponentsFileName, strLanguage, token: token)
                    .ConfigureAwait(false);
            if (SourceID != Guid.Empty)
                objReturn = objDoc.TryGetNodeById(DrugsData.ComponentXPath, SourceID);
            if (objReturn == null)
            {
                objReturn = objDoc.TryGetNodeByNameOrId(DrugsData.ComponentXPath, Name);
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

        /// <summary>
        /// Skill bonus nodes from the component effect (name + bonus).
        /// </summary>
        public List<XmlNode> SpecificSkills { get; } = new List<XmlNode>();

        public List<string> Infos { get; } = new List<string>();

        public int Initiative { get; set; }

        public int InitiativeDice { get; set; }

        public int CrashDamage { get; set; }

        public int Speed { get; set; }

        public int Duration { get; set; }

        public int Level { get; set; }

        /// <summary>
        /// Shallow copy of this effect (XML nodes are shared read-only catalog data).
        /// </summary>
        /// <returns>A new effect with copied numeric fields and the same quality/skill nodes.</returns>
        public DrugEffect Clone()
        {
            DrugEffect objCopy = new DrugEffect
            {
                Initiative = Initiative,
                InitiativeDice = InitiativeDice,
                CrashDamage = CrashDamage,
                Speed = Speed,
                Duration = Duration,
                Level = Level
            };
            foreach (KeyValuePair<string, decimal> kvp in Attributes)
                objCopy.Attributes.Add(kvp.Key, kvp.Value);
            foreach (KeyValuePair<string, int> kvp in Limits)
                objCopy.Limits.Add(kvp.Key, kvp.Value);
            objCopy.Qualities.AddRange(Qualities);
            objCopy.SpecificSkills.AddRange(SpecificSkills);
            objCopy.Infos.AddRange(Infos);
            return objCopy;
        }
    }
}
