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
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;
using Chummer.Annotations;
using Chummer.Backend.Enums;
using NLog;

// ReSharper disable ConvertToAutoProperty

namespace Chummer.Backend.Equipment
{
    /// <summary>
    /// Lifestyle.
    /// </summary>
    [DebuggerDisplay("{DisplayName(\"en-us\")}")]
    public sealed class Lifestyle : IHasInternalId, IHasXmlDataNode, IHasNotes, ICanRemove, IHasCustomName, IHasSourceId, IHasSource, ICanSort, INotifyMultiplePropertiesChangedAsync, IHasLockObject, IHasCost, IHasCharacterObject
    {
        private static readonly Lazy<Logger> s_ObjLogger = new Lazy<Logger>(LogManager.GetCurrentClassLogger);
        private static Logger Log => s_ObjLogger.Value;

        // ReSharper disable once InconsistentNaming
        private Guid _guiID;

        // ReSharper disable once InconsistentNaming
        private Guid _guiSourceID;

        private string _strName = string.Empty;
        private decimal _decCost;
        private int _intDice;
        private decimal _decMultiplier;
        private int _intIncrements = 1;
        private int _intRoommates;
        private decimal _decPercentage = 100.0m;
        private int _intComforts;
        private int _intArea;
        private int _intSecurity;
        private int _intBaseComforts;
        private int _intBaseArea;
        private int _intBaseSecurity;
        private int _intComfortsMaximum;
        private int _intSecurityMaximum;
        private int _intAreaMaximum;
        private int _intBonusLP;
        private int _intLP;
        private bool _blnAllowBonusLP;
        private bool _blnSplitCostWithRoommates;
        private decimal _decCostForSecurity;
        private decimal _decCostForArea;
        private decimal _decCostForComforts;
        private string _strBaseLifestyle = string.Empty;
        private string _strSource = string.Empty;
        private string _strPage = string.Empty;
        private bool _blnTrustFund;
        private LifestyleType _eType = LifestyleType.Standard;
        private LifestyleIncrement _eIncrement = LifestyleIncrement.Month;
        private string _strNotes = string.Empty;
        private Color _colNotes = ColorManager.HasNotesColor;
        private int _intSortOrder;
        private readonly Character _objCharacter;

        private string _strCity;
        private string _strDistrict;
        private string _strBorough;
        private bool _blnAllowsNameOverride;

        #region Helper Methods

        /// <summary>
        /// Convert a string to a LifestyleType.
        /// </summary>
        /// <param name="strValue">String value to convert.</param>
        public static LifestyleType ConvertToLifestyleType(string strValue)
        {
            switch (strValue.ToUpperInvariant())
            {
                case "BOLTHOLE":
                    return LifestyleType.BoltHole;

                case "SAFEHOUSE":
                    return LifestyleType.Safehouse;

                case "ADVANCED":
                    return LifestyleType.Advanced;

                default:
                    return LifestyleType.Standard;
            }
        }

        /// <summary>
        /// Convert a string to a LifestyleType.
        /// </summary>
        /// <param name="strValue">String value to convert.</param>
        public static LifestyleIncrement ConvertToLifestyleIncrement(string strValue)
        {
            switch (strValue.ToUpperInvariant())
            {
                case "DAY":
                    return LifestyleIncrement.Day;

                case "WEEK":
                    return LifestyleIncrement.Week;

                default:
                    return LifestyleIncrement.Month;
            }
        }

        #endregion Helper Methods

        #region Constructor, Create, Save, Load, and Print Methods

        public Lifestyle(Character objCharacter)
        {
            // Create the GUID for the new Lifestyle.
            _guiID = Guid.NewGuid();
            _objCharacter = objCharacter ?? throw new ArgumentNullException(nameof(objCharacter));
            LockObject = objCharacter.LockObject;
            _lstLifestyleQualities = new ThreadSafeObservableCollection<LifestyleQuality>(LockObject);
            LifestyleQualities.CollectionChangedAsync += LifestyleQualitiesCollectionChanged;
            LifestyleQualities.BeforeClearCollectionChangedAsync += LifestyleQualitiesOnBeforeClearCollectionChanged;
        }

        /// <summary>
        /// Create a Lifestyle from an XmlNode and return the TreeNodes for it.
        /// </summary>
        /// <param name="objXmlLifestyle">XmlNode to create the object from.</param>
        public void Create(XmlNode objXmlLifestyle)
        {
            using (LockObject.EnterWriteLock())
            {
                if (!objXmlLifestyle.TryGetField("id", Guid.TryParse, out _guiSourceID))
                {
                    Log.Warn(new object[] {"Missing id field for xmlnode", objXmlLifestyle});
                    Utils.BreakIfDebug();
                }
                else
                {
                    _objCachedMyXmlNode = null;
                    _objCachedMyXPathNode = null;
                }

                objXmlLifestyle.TryGetStringFieldQuickly("name", ref _strBaseLifestyle);
                objXmlLifestyle.TryGetDecFieldQuickly("cost", ref _decCost);
                objXmlLifestyle.TryGetInt32FieldQuickly("dice", ref _intDice);
                objXmlLifestyle.TryGetDecFieldQuickly("multiplier", ref _decMultiplier);
                objXmlLifestyle.TryGetStringFieldQuickly("source", ref _strSource);
                objXmlLifestyle.TryGetStringFieldQuickly("page", ref _strPage);
                objXmlLifestyle.TryGetInt32FieldQuickly("lp", ref _intLP);
                objXmlLifestyle.TryGetDecFieldQuickly("costforarea", ref _decCostForArea);
                objXmlLifestyle.TryGetDecFieldQuickly("costforcomforts", ref _decCostForComforts);
                objXmlLifestyle.TryGetDecFieldQuickly("costforsecurity", ref _decCostForSecurity);
                objXmlLifestyle.TryGetBoolFieldQuickly("allowbonuslp", ref _blnAllowBonusLP);
                if (!objXmlLifestyle.TryGetMultiLineStringFieldQuickly("altnotes", ref _strNotes))
                    objXmlLifestyle.TryGetMultiLineStringFieldQuickly("notes", ref _strNotes);

                if (GlobalSettings.InsertPdfNotesIfAvailable && string.IsNullOrEmpty(Notes))
                {
                    Notes = CommonFunctions.GetBookNotes(objXmlLifestyle, Name, CurrentDisplayName, Source, Page,
                                                         DisplayPage(GlobalSettings.Language), _objCharacter);
                }

                string sNotesColor = ColorTranslator.ToHtml(ColorManager.HasNotesColor);
                objXmlLifestyle.TryGetStringFieldQuickly("notesColor", ref sNotesColor);
                _colNotes = ColorTranslator.FromHtml(sNotesColor);

                string strTemp = string.Empty;
                if (objXmlLifestyle.TryGetStringFieldQuickly("increment", ref strTemp))
                    _eIncrement = ConvertToLifestyleIncrement(strTemp);

                XPathNavigator xmlLifestyleXPathDocument = _objCharacter.LoadDataXPath("lifestyles.xml");
                XPathNavigator xmlLifestyleNode =
                    xmlLifestyleXPathDocument.SelectSingleNode(
                        "/chummer/comforts/comfort[name = " + BaseLifestyle.CleanXPath() + "]");
                xmlLifestyleNode.TryGetInt32FieldQuickly("minimum", ref _intBaseComforts);
                xmlLifestyleNode.TryGetInt32FieldQuickly("limit", ref _intComfortsMaximum);

                // Area.
                xmlLifestyleNode =
                    xmlLifestyleXPathDocument.SelectSingleNode(
                        "/chummer/neighborhoods/neighborhood[name = " + BaseLifestyle.CleanXPath() + "]");
                xmlLifestyleNode.TryGetInt32FieldQuickly("minimum", ref _intBaseArea);
                xmlLifestyleNode.TryGetInt32FieldQuickly("limit", ref _intAreaMaximum);

                // Security.
                xmlLifestyleNode =
                    xmlLifestyleXPathDocument.SelectSingleNode(
                        "/chummer/securities/security[name = " + BaseLifestyle.CleanXPath() + "]");
                xmlLifestyleNode.TryGetInt32FieldQuickly("minimum", ref _intBaseSecurity);
                xmlLifestyleNode.TryGetInt32FieldQuickly("limit", ref _intSecurityMaximum);
                if (_objCharacter.Settings.BookEnabled("HT") || _objCharacter.Settings.AllowFreeGrids)
                {
                    using (XmlNodeList lstGridNodes = objXmlLifestyle.SelectNodes("freegrids/freegrid"))
                    {
                        if (lstGridNodes == null || lstGridNodes.Count <= 0)
                            return;

                        foreach (LifestyleQuality objFreeGrid in LifestyleQualities.Where(x => x.IsFreeGrid).ToList())
                        {
                            objFreeGrid.Remove(false);
                        }

                        XmlDocument xmlLifestyleDocument = _objCharacter.LoadData("lifestyles.xml");
                        foreach (XmlNode xmlNode in lstGridNodes)
                        {
                            XmlNode xmlQuality
                                = xmlLifestyleDocument.TryGetNodeByNameOrId(
                                    "/chummer/qualities/quality", xmlNode.InnerTextViaPool());
                            string strPush = xmlNode.SelectSingleNodeAndCacheExpressionAsNavigator("@select")?.Value;
                            if (!string.IsNullOrWhiteSpace(strPush))
                            {
                                _objCharacter.PushText.Push(strPush);
                            }

                            LifestyleQuality objQuality = new LifestyleQuality(_objCharacter);
                            try
                            {
                                objQuality.Create(xmlQuality, this, _objCharacter, QualitySource.BuiltIn);
                                objQuality.IsFreeGrid = true;
                                LifestyleQualities.Add(objQuality);
                            }
                            catch
                            {
                                objQuality.Remove(false);
                                throw;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Create a Lifestyle from an XmlNode and return the TreeNodes for it.
        /// </summary>
        /// <param name="objXmlLifestyle">XmlNode to create the object from.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public async Task CreateAsync(XmlNode objXmlLifestyle, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (!objXmlLifestyle.TryGetField("id", Guid.TryParse, out _guiSourceID))
                {
                    Log.Warn(new object[] { "Missing id field for xmlnode", objXmlLifestyle });
                    Utils.BreakIfDebug();
                }
                else
                {
                    _objCachedMyXmlNode = null;
                    _objCachedMyXPathNode = null;
                }

                objXmlLifestyle.TryGetStringFieldQuickly("name", ref _strBaseLifestyle);
                objXmlLifestyle.TryGetDecFieldQuickly("cost", ref _decCost);
                objXmlLifestyle.TryGetInt32FieldQuickly("dice", ref _intDice);
                objXmlLifestyle.TryGetDecFieldQuickly("multiplier", ref _decMultiplier);
                objXmlLifestyle.TryGetStringFieldQuickly("source", ref _strSource);
                objXmlLifestyle.TryGetStringFieldQuickly("page", ref _strPage);
                objXmlLifestyle.TryGetInt32FieldQuickly("lp", ref _intLP);
                objXmlLifestyle.TryGetDecFieldQuickly("costforarea", ref _decCostForArea);
                objXmlLifestyle.TryGetDecFieldQuickly("costforcomforts", ref _decCostForComforts);
                objXmlLifestyle.TryGetDecFieldQuickly("costforsecurity", ref _decCostForSecurity);
                objXmlLifestyle.TryGetBoolFieldQuickly("allowbonuslp", ref _blnAllowBonusLP);
                if (!objXmlLifestyle.TryGetMultiLineStringFieldQuickly("altnotes", ref _strNotes))
                    objXmlLifestyle.TryGetMultiLineStringFieldQuickly("notes", ref _strNotes);

                if (GlobalSettings.InsertPdfNotesIfAvailable && string.IsNullOrEmpty(await GetNotesAsync(token).ConfigureAwait(false)))
                {
                    await SetNotesAsync(await CommonFunctions.GetBookNotesAsync(objXmlLifestyle, _strName,
                        await GetCurrentDisplayNameAsync(token).ConfigureAwait(false), _strSource, _strPage,
                        await DisplayPageAsync(GlobalSettings.Language, token).ConfigureAwait(false), _objCharacter,
                        token).ConfigureAwait(false), token).ConfigureAwait(false);
                }

                string sNotesColor = ColorTranslator.ToHtml(ColorManager.HasNotesColor);
                objXmlLifestyle.TryGetStringFieldQuickly("notesColor", ref sNotesColor);
                _colNotes = ColorTranslator.FromHtml(sNotesColor);

                string strTemp = string.Empty;
                if (objXmlLifestyle.TryGetStringFieldQuickly("increment", ref strTemp))
                    _eIncrement = ConvertToLifestyleIncrement(strTemp);

                XPathNavigator xmlLifestyleXPathDocument =
                    await _objCharacter.LoadDataXPathAsync("lifestyles.xml", token: token).ConfigureAwait(false);
                XPathNavigator xmlLifestyleNode =
                    xmlLifestyleXPathDocument.SelectSingleNode(
                        "/chummer/comforts/comfort[name = " + _strBaseLifestyle.CleanXPath() + "]");
                xmlLifestyleNode.TryGetInt32FieldQuickly("minimum", ref _intBaseComforts);
                xmlLifestyleNode.TryGetInt32FieldQuickly("limit", ref _intComfortsMaximum);

                // Area.
                xmlLifestyleNode =
                    xmlLifestyleXPathDocument.SelectSingleNode(
                        "/chummer/neighborhoods/neighborhood[name = " + _strBaseLifestyle.CleanXPath() + "]");
                xmlLifestyleNode.TryGetInt32FieldQuickly("minimum", ref _intBaseArea);
                xmlLifestyleNode.TryGetInt32FieldQuickly("limit", ref _intAreaMaximum);

                // Security.
                xmlLifestyleNode =
                    xmlLifestyleXPathDocument.SelectSingleNode(
                        "/chummer/securities/security[name = " + _strBaseLifestyle.CleanXPath() + "]");
                xmlLifestyleNode.TryGetInt32FieldQuickly("minimum", ref _intBaseSecurity);
                xmlLifestyleNode.TryGetInt32FieldQuickly("limit", ref _intSecurityMaximum);
                CharacterSettings objSettings = await _objCharacter.GetSettingsAsync(token).ConfigureAwait(false);
                if (await objSettings.BookEnabledAsync("HT", token).ConfigureAwait(false) || await objSettings.GetAllowFreeGridsAsync(token).ConfigureAwait(false))
                {
                    using (XmlNodeList lstGridNodes = objXmlLifestyle.SelectNodes("freegrids/freegrid"))
                    {
                        if (lstGridNodes == null || lstGridNodes.Count <= 0)
                            return;

                        foreach (LifestyleQuality objFreeGrid in await LifestyleQualities.ToListAsync(x => x.GetIsFreeGridAsync(token),
                                     token: token).ConfigureAwait(false))
                        {
                            await objFreeGrid.RemoveAsync(false, token).ConfigureAwait(false);
                        }

                        XmlDocument xmlLifestyleDocument =
                            await _objCharacter.LoadDataAsync("lifestyles.xml", token: token).ConfigureAwait(false);
                        foreach (XmlNode xmlNode in lstGridNodes)
                        {
                            XmlNode xmlQuality
                                = xmlLifestyleDocument.TryGetNodeByNameOrId(
                                    "/chummer/qualities/quality", xmlNode.InnerTextViaPool(token));
                            string strPush = xmlNode.SelectSingleNodeAndCacheExpressionAsNavigator("@select", token)
                                ?.Value;
                            if (!string.IsNullOrWhiteSpace(strPush))
                            {
                                (await _objCharacter.GetPushTextAsync(token).ConfigureAwait(false)).Push(strPush);
                            }

                            LifestyleQuality objQuality = new LifestyleQuality(_objCharacter);
                            try
                            {
                                await objQuality.CreateAsync(xmlQuality, this, _objCharacter, QualitySource.BuiltIn,
                                    token: token).ConfigureAwait(false);
                                await objQuality.SetIsFreeGridAsync(true, token).ConfigureAwait(false);
                                await _lstLifestyleQualities.AddAsync(objQuality, token).ConfigureAwait(false);
                            }
                            catch
                            {
                                await objQuality.RemoveAsync(false, CancellationToken.None).ConfigureAwait(false);
                                throw;
                            }
                        }
                    }
                }

                List<LifestyleQuality> lstToRemove = new List<LifestyleQuality>(await _lstLifestyleQualities.GetCountAsync(token).ConfigureAwait(false));
                await _lstLifestyleQualities.ForEachAsync(async objQuality =>
                {
                    if (await objQuality.GetOriginSourceAsync(token).ConfigureAwait(false) == QualitySource.Selected)
                    {
                        XPathNavigator xmlQuality = await objQuality.GetNodeXPathAsync(token).ConfigureAwait(false);
                        if (!await xmlQuality.RequirementsMetAsync(_objCharacter, this, token: token)
                                .ConfigureAwait(false))
                        {
                            lstToRemove.Add(objQuality);
                        }
                    }
                }, token).ConfigureAwait(false);
                foreach (LifestyleQuality objQuality in lstToRemove)
                    await objQuality.RemoveAsync(false, token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        private SourceString _objCachedSourceDetail;

        public SourceString SourceDetail
        {
            get
            {
                using (LockObject.EnterReadLock())
                {
                    return _objCachedSourceDetail == default
                        ? _objCachedSourceDetail = SourceString.GetSourceString(Source,
                            DisplayPage(GlobalSettings.Language),
                            GlobalSettings.Language,
                            GlobalSettings.CultureInfo,
                            _objCharacter)
                        : _objCachedSourceDetail;
                }
            }
        }

        public async Task<SourceString> GetSourceDetailAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _objCachedSourceDetail == default
                    ? _objCachedSourceDetail = await SourceString.GetSourceStringAsync(Source,
                        await DisplayPageAsync(GlobalSettings.Language, token).ConfigureAwait(false),
                        GlobalSettings.Language,
                        GlobalSettings.CultureInfo,
                        _objCharacter, token).ConfigureAwait(false)
                    : _objCachedSourceDetail;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Save the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        public void Save(XmlWriter objWriter)
        {
            if (objWriter == null)
                return;
            using (LockObject.EnterReadLock())
            {
                objWriter.WriteStartElement("lifestyle");
                objWriter.WriteElementString("sourceid", SourceIDString);
                objWriter.WriteElementString("guid", InternalId);
                objWriter.WriteElementString("name", _strName);
                objWriter.WriteElementString("allowsnameoverride", AllowsNameOverride.ToString(GlobalSettings.InvariantCultureInfo));
                objWriter.WriteElementString("cost", _decCost.ToString(GlobalSettings.InvariantCultureInfo));
                objWriter.WriteElementString("dice", _intDice.ToString(GlobalSettings.InvariantCultureInfo));
                objWriter.WriteElementString("lp", _intLP.ToString(GlobalSettings.InvariantCultureInfo));
                objWriter.WriteElementString("baselifestyle", _strBaseLifestyle);
                objWriter.WriteElementString("multiplier",
                                             _decMultiplier.ToString(GlobalSettings.InvariantCultureInfo));
                objWriter.WriteElementString("months", _intIncrements.ToString(GlobalSettings.InvariantCultureInfo));
                objWriter.WriteElementString("roommates", _intRoommates.ToString(GlobalSettings.InvariantCultureInfo));
                objWriter.WriteElementString("percentage",
                                             _decPercentage.ToString(GlobalSettings.InvariantCultureInfo));
                objWriter.WriteElementString("area", _intArea.ToString(GlobalSettings.InvariantCultureInfo));
                objWriter.WriteElementString("comforts", _intComforts.ToString(GlobalSettings.InvariantCultureInfo));
                objWriter.WriteElementString("security", _intSecurity.ToString(GlobalSettings.InvariantCultureInfo));
                objWriter.WriteElementString("basearea", _intBaseArea.ToString(GlobalSettings.InvariantCultureInfo));
                objWriter.WriteElementString("basecomforts",
                                             _intBaseComforts.ToString(GlobalSettings.InvariantCultureInfo));
                objWriter.WriteElementString("basesecurity",
                                             _intBaseSecurity.ToString(GlobalSettings.InvariantCultureInfo));
                objWriter.WriteElementString("maxarea", _intAreaMaximum.ToString(GlobalSettings.InvariantCultureInfo));
                objWriter.WriteElementString("maxcomforts",
                                             _intComfortsMaximum.ToString(GlobalSettings.InvariantCultureInfo));
                objWriter.WriteElementString("maxsecurity",
                                             _intSecurityMaximum.ToString(GlobalSettings.InvariantCultureInfo));
                objWriter.WriteElementString("costforearea",
                                             _decCostForArea.ToString(GlobalSettings.InvariantCultureInfo));
                objWriter.WriteElementString("costforcomforts",
                                             _decCostForComforts.ToString(GlobalSettings.InvariantCultureInfo));
                objWriter.WriteElementString("costforsecurity",
                                             _decCostForSecurity.ToString(GlobalSettings.InvariantCultureInfo));
                objWriter.WriteElementString("allowbonuslp",
                                             _blnAllowBonusLP.ToString(GlobalSettings.InvariantCultureInfo));
                objWriter.WriteElementString("bonuslp", _intBonusLP.ToString(GlobalSettings.InvariantCultureInfo));
                objWriter.WriteElementString("source", _strSource);
                objWriter.WriteElementString("page", _strPage);
                objWriter.WriteElementString("trustfund", _blnTrustFund.ToString(GlobalSettings.InvariantCultureInfo));
                objWriter.WriteElementString("splitcostwithroommates",
                    _blnSplitCostWithRoommates.ToString(GlobalSettings.InvariantCultureInfo));
                objWriter.WriteElementString("type", _eType.ToString());
                objWriter.WriteElementString("increment", _eIncrement.ToString());
                objWriter.WriteElementString("sourceid", SourceIDString);

                objWriter.WriteElementString("city", _strCity);
                objWriter.WriteElementString("district", _strDistrict);
                objWriter.WriteElementString("borough", _strBorough);

                objWriter.WriteStartElement("lifestylequalities");
                foreach (LifestyleQuality objQuality in LifestyleQualities)
                {
                    objQuality.Save(objWriter);
                }

                objWriter.WriteEndElement();
                objWriter.WriteElementString("notes", _strNotes.CleanOfXmlInvalidUnicodeChars());
                objWriter.WriteElementString("notesColor", ColorTranslator.ToHtml(_colNotes));
                objWriter.WriteElementString("sortorder", _intSortOrder.ToString(GlobalSettings.InvariantCultureInfo));
                objWriter.WriteEndElement();
            }
        }

        /// <summary>
        /// Load the Lifestyle from the XmlNode.
        /// </summary>
        /// <param name="objNode">XmlNode to load.</param>
        /// <param name="blnCopy">Whether we are loading a copy of an existing lifestyle.</param>
        public void Load(XmlNode objNode, bool blnCopy = false)
        {
            Utils.SafelyRunSynchronously(() => LoadCoreAsync(true, objNode, blnCopy));
        }

        /// <summary>
        /// Load the Lifestyle from the XmlNode.
        /// </summary>
        /// <param name="objNode">XmlNode to load.</param>
        /// <param name="blnCopy">Whether we are loading a copy of an existing lifestyle.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public Task LoadAsync(XmlNode objNode, bool blnCopy = false, CancellationToken token = default)
        {
            return LoadCoreAsync(false, objNode, blnCopy, token);
        }

        private async Task LoadCoreAsync(bool blnSync, XmlNode objNode, bool blnCopy = false, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IDisposable objLocker = null;
            IAsyncDisposable objLockerAsync = null;
            if (blnSync)
                // ReSharper disable once MethodHasAsyncOverload
                objLocker = LockObject.EnterWriteLock(token);
            else
                objLockerAsync = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (blnCopy || !objNode.TryGetField("guid", Guid.TryParse, out _guiID))
                {
                    _guiID = Guid.NewGuid();
                }

                objNode.TryGetStringFieldQuickly("name", ref _strName);
                objNode.TryGetBoolFieldQuickly("allowsnameoverride", ref _blnAllowsNameOverride);
                _objCachedMyXmlNode = null;
                _objCachedMyXPathNode = null;
                Lazy<XmlNode> objMyNode = null;
                Microsoft.VisualStudio.Threading.AsyncLazy<XmlNode> objMyNodeAsync = null;
                if (blnSync)
                    objMyNode = new Lazy<XmlNode>(() => this.GetNode(token));
                else
                    objMyNodeAsync = new Microsoft.VisualStudio.Threading.AsyncLazy<XmlNode>(() => this.GetNodeAsync(token), Utils.JoinableTaskFactory);
                if (!objNode.TryGetGuidFieldQuickly("sourceid", ref _guiSourceID))
                {
                    (blnSync ? objMyNode.Value : await objMyNodeAsync.GetValueAsync(token).ConfigureAwait(false))?.TryGetGuidFieldQuickly("id", ref _guiSourceID);
                }

                if (blnCopy)
                {
                    _intIncrements = 0;
                }
                else
                {
                    objNode.TryGetInt32FieldQuickly("months", ref _intIncrements);
                    objNode.TryGetField("guid", Guid.TryParse, out _guiID);
                }

                objNode.TryGetDecFieldQuickly("cost", ref _decCost);
                objNode.TryGetInt32FieldQuickly("dice", ref _intDice);
                objNode.TryGetDecFieldQuickly("multiplier", ref _decMultiplier);

                objNode.TryGetStringFieldQuickly("city", ref _strCity);
                objNode.TryGetStringFieldQuickly("district", ref _strDistrict);
                objNode.TryGetStringFieldQuickly("borough", ref _strBorough);

                objNode.TryGetInt32FieldQuickly("area", ref _intArea);
                objNode.TryGetInt32FieldQuickly("comforts", ref _intComforts);
                objNode.TryGetInt32FieldQuickly("security", ref _intSecurity);
                objNode.TryGetInt32FieldQuickly("basearea", ref _intBaseArea);
                objNode.TryGetInt32FieldQuickly("basecomforts", ref _intBaseComforts);
                objNode.TryGetInt32FieldQuickly("basesecurity", ref _intBaseSecurity);
                objNode.TryGetDecFieldQuickly("costforarea", ref _decCostForArea);
                objNode.TryGetDecFieldQuickly("costforcomforts", ref _decCostForComforts);
                objNode.TryGetDecFieldQuickly("costforsecurity", ref _decCostForSecurity);
                objNode.TryGetInt32FieldQuickly("roommates", ref _intRoommates);
                objNode.TryGetDecFieldQuickly("percentage", ref _decPercentage);
                objNode.TryGetStringFieldQuickly("baselifestyle", ref _strBaseLifestyle);
                objNode.TryGetInt32FieldQuickly("sortorder", ref _intSortOrder);
                XPathNavigator xmlLifestyles = blnSync
                    // ReSharper disable once MethodHasAsyncOverload
                    ? _objCharacter.LoadDataXPath("lifestyles.xml", token: token)
                    : await _objCharacter.LoadDataXPathAsync("lifestyles.xml", token: token).ConfigureAwait(false);
                if (xmlLifestyles.TryGetNodeByNameOrId("/chummer/lifestyles/lifestyle", BaseLifestyle) == null
                    && xmlLifestyles.TryGetNodeByNameOrId("/chummer/lifestyles/lifestyle", Name) != null)
                {
                    (_strName, _strBaseLifestyle) = (_strBaseLifestyle, _strName);
                }

                if (string.IsNullOrWhiteSpace(_strBaseLifestyle))
                {
                    objNode.TryGetStringFieldQuickly("lifestylename", ref _strBaseLifestyle);
                    if (string.IsNullOrWhiteSpace(_strBaseLifestyle))
                    {
                        using (new FetchSafelyFromSafeObjectPool<List<ListItem>>(Utils.ListItemListPool,
                                                                       out List<ListItem> lstQualities))
                        {
                            foreach (XPathNavigator xmlLifestyle in xmlLifestyles.SelectAndCacheExpression(
                                         "/chummer/lifestyles/lifestyle", token))
                            {
                                string strName = xmlLifestyle.SelectSingleNodeAndCacheExpression("name", token)?.Value
                                                 ?? (blnSync
                                                     // ReSharper disable once MethodHasAsyncOverload
                                                     ? LanguageManager.GetString("String_Error", token: token)
                                                    : await LanguageManager.GetStringAsync("String_Error", token: token).ConfigureAwait(false));
                                lstQualities.Add(
                                    new ListItem(
                                        strName,
                                        xmlLifestyle.SelectSingleNodeAndCacheExpression("translate", token)?.Value
                                        ?? strName));
                            }

                            string strDescription = string.Format(GlobalSettings.CultureInfo, blnSync
                                // ReSharper disable once MethodHasAsyncOverload
                                ? LanguageManager.GetString("String_CannotFindLifestyle", token: token)
                                : await LanguageManager.GetStringAsync("String_CannotFindLifestyle", token: token).ConfigureAwait(false), _strName);
                            using (ThreadSafeForm<SelectItem> frmSelect = blnSync
                                // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                                ? ThreadSafeForm<SelectItem>.Get(
                                       () => new SelectItem
                                       {
                                           Description = strDescription
                                       })
                                : await ThreadSafeForm<SelectItem>.GetAsync(
                                       () => new SelectItem
                                       {
                                           Description = strDescription
                                       }, token).ConfigureAwait(false))
                            {
                                frmSelect.MyForm.SetGeneralItemsMode(lstQualities);
                                // ReSharper disable once MethodHasAsyncOverload
                                if ((blnSync ? frmSelect.ShowDialogSafe(_objCharacter, token) : await frmSelect.ShowDialogSafeAsync(_objCharacter, token).ConfigureAwait(false))
                                    == DialogResult.Cancel)
                                {
                                    _guiID = Guid.Empty;
                                    return;
                                }

                                _strBaseLifestyle = frmSelect.MyForm.SelectedItem;
                            }
                        }
                    }
                }

                if (_strBaseLifestyle == "Middle")
                    _strBaseLifestyle = "Medium";
                // Legacy sweep for issues with Advanced Lifestyle selector not properly resetting values upon changes to the Base Lifestyle
                if (_objCharacter.LastSavedVersion <= new ValueVersion(5, 212, 73)
                    && _strBaseLifestyle != "Street"
                    && (_decCostForArea != 0 || _decCostForComforts != 0 || _decCostForSecurity != 0))
                {
                    XmlNode xmlDataNode = blnSync ? objMyNode.Value : await objMyNodeAsync.GetValueAsync(token).ConfigureAwait(false);
                    if (xmlDataNode != null)
                    {
                        xmlDataNode.TryGetDecFieldQuickly("costforarea", ref _decCostForArea);
                        xmlDataNode.TryGetDecFieldQuickly("costforcomforts", ref _decCostForComforts);
                        xmlDataNode.TryGetDecFieldQuickly("costforsecurity", ref _decCostForSecurity);
                    }
                }

                if (!objNode.TryGetBoolFieldQuickly("allowbonuslp", ref _blnAllowBonusLP))
                    (blnSync ? objMyNode.Value : await objMyNodeAsync.GetValueAsync(token).ConfigureAwait(false))?.TryGetBoolFieldQuickly("allowbonuslp", ref _blnAllowBonusLP);
                if (!objNode.TryGetInt32FieldQuickly("bonuslp", ref _intBonusLP) && _strBaseLifestyle == "Traveler")
                    // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                    _intBonusLP = blnSync ? Utils.GlobalRandom.NextD6ModuloBiasRemoved() : await Utils.GlobalRandom.NextD6ModuloBiasRemovedAsync(token).ConfigureAwait(false);

                if (!objNode.TryGetInt32FieldQuickly("lp", ref _intLP))
                {
                    XPathNavigator xmlLifestyleNode =
                        xmlLifestyles.TryGetNodeByNameOrId("/chummer/lifestyles/lifestyle", BaseLifestyle);
                    xmlLifestyleNode.TryGetInt32FieldQuickly("lp", ref _intLP);
                }

                if (!objNode.TryGetInt32FieldQuickly("maxarea", ref _intAreaMaximum))
                {
                    XPathNavigator xmlLifestyleNode =
                        xmlLifestyles.TryGetNodeByNameOrId("/chummer/comforts/comfort", BaseLifestyle);
                    xmlLifestyleNode.TryGetInt32FieldQuickly("minimum", ref _intBaseComforts);
                    xmlLifestyleNode.TryGetInt32FieldQuickly("limit", ref _intComfortsMaximum);

                    // Area.
                    xmlLifestyleNode =
                        xmlLifestyles.TryGetNodeByNameOrId("/chummer/neighborhoods/neighborhood", BaseLifestyle);
                    xmlLifestyleNode.TryGetInt32FieldQuickly("minimum", ref _intBaseArea);
                    xmlLifestyleNode.TryGetInt32FieldQuickly("limit", ref _intAreaMaximum);

                    // Security.
                    xmlLifestyleNode =
                        xmlLifestyles.TryGetNodeByNameOrId("/chummer/securities/security", BaseLifestyle);
                    xmlLifestyleNode.TryGetInt32FieldQuickly("minimum", ref _intBaseSecurity);
                    xmlLifestyleNode.TryGetInt32FieldQuickly("limit", ref _intSecurityMaximum);
                }
                else
                {
                    objNode.TryGetInt32FieldQuickly("maxarea", ref _intAreaMaximum);
                    objNode.TryGetInt32FieldQuickly("maxcomforts", ref _intComfortsMaximum);
                    objNode.TryGetInt32FieldQuickly("maxsecurity", ref _intSecurityMaximum);
                }

                objNode.TryGetStringFieldQuickly("source", ref _strSource);
                objNode.TryGetBoolFieldQuickly("trustfund", ref _blnTrustFund);
                if (!objNode.TryGetBoolFieldQuickly("splitcostwithroommates", ref _blnSplitCostWithRoommates))
                {
                    if (objNode.TryGetBoolFieldQuickly("primarytenant", ref _blnSplitCostWithRoommates))
                        _blnSplitCostWithRoommates = !_blnSplitCostWithRoommates;
                    else
                        _blnSplitCostWithRoommates = _intRoommates > 0;
                }

                objNode.TryGetStringFieldQuickly("page", ref _strPage);

                // Lifestyle Qualities
                using (XmlNodeList xmlQualityList = objNode.SelectNodes("lifestylequalities/lifestylequality"))
                {
                    if (xmlQualityList != null)
                    {
                        if (blnSync)
                        {
                            foreach (XmlNode xmlQuality in xmlQualityList)
                            {
                                LifestyleQuality objQuality = new LifestyleQuality(_objCharacter);
                                try
                                {
                                    // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                                    objQuality.Load(xmlQuality, this);
                                    // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                                    LifestyleQualities.Add(objQuality);
                                }
                                catch
                                {
                                    // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                                    objQuality.Remove(false);
                                    throw;
                                }
                            }
                        }
                        else
                        {
                            foreach (XmlNode xmlQuality in xmlQualityList)
                            {
                                LifestyleQuality objQuality = new LifestyleQuality(_objCharacter);
                                try
                                {
                                    await objQuality.LoadAsync(xmlQuality, this, token).ConfigureAwait(false);
                                    await LifestyleQualities.AddAsync(objQuality, token).ConfigureAwait(false);
                                }
                                catch
                                {
                                    await objQuality.RemoveAsync(false, CancellationToken.None).ConfigureAwait(false);
                                    throw;
                                }
                            }
                        }
                    }
                }

                // Legacy sweep:
                // Free Grids provided by the Lifestyle saved to a separate node
                using (XmlNodeList xmlQualityList = objNode.SelectNodes("freegrids/lifestylequality"))
                {
                    if (xmlQualityList != null)
                    {
                        if (blnSync)
                        {
                            foreach (XmlNode xmlQuality in xmlQualityList)
                            {
                                LifestyleQuality objQuality = new LifestyleQuality(_objCharacter);
                                try
                                {
                                    // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                                    objQuality.Load(xmlQuality, this);
                                    objQuality.IsFreeGrid = true;
                                    // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                                    LifestyleQualities.Add(objQuality);
                                }
                                catch
                                {
                                    // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                                    objQuality.Remove(false);
                                    throw;
                                }
                            }
                        }
                        else
                        {
                            foreach (XmlNode xmlQuality in xmlQualityList)
                            {
                                LifestyleQuality objQuality = new LifestyleQuality(_objCharacter);
                                try
                                {
                                    await objQuality.LoadAsync(xmlQuality, this, token).ConfigureAwait(false);
                                    await objQuality.SetIsFreeGridAsync(true, token).ConfigureAwait(false);
                                    await LifestyleQualities.AddAsync(objQuality, token).ConfigureAwait(false);
                                }
                                catch
                                {
                                    await objQuality.RemoveAsync(false, CancellationToken.None).ConfigureAwait(false);
                                    throw;
                                }
                            }
                        }
                    }
                }

                objNode.TryGetMultiLineStringFieldQuickly("notes", ref _strNotes);

                string sNotesColor = ColorTranslator.ToHtml(ColorManager.HasNotesColor);
                objNode.TryGetStringFieldQuickly("notesColor", ref sNotesColor);
                _colNotes = ColorTranslator.FromHtml(sNotesColor);

                string strTemp = string.Empty;
                if (objNode.TryGetStringFieldQuickly("type", ref strTemp))
                {
                    _eType = ConvertToLifestyleType(strTemp);
                }

                if (objNode.TryGetStringFieldQuickly("increment", ref strTemp))
                {
                    _eIncrement = ConvertToLifestyleIncrement(strTemp);
                }
                else if (_eType == LifestyleType.Safehouse)
                    _eIncrement = LifestyleIncrement.Week;
                else if ((blnSync ? objMyNode.Value : await objMyNodeAsync.GetValueAsync(token).ConfigureAwait(false))?.TryGetStringFieldQuickly("increment", ref strTemp) == true)
                    _eIncrement = ConvertToLifestyleIncrement(strTemp);

                if (blnSync)
                    // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                    LegacyShim(objNode);
                else
                    await LegacyShimAsync(objNode, token).ConfigureAwait(false);
            }
            finally
            {
                if (blnSync)
                    // ReSharper disable once MethodHasAsyncOverload
                    objLocker.Dispose();
                else
                    await objLockerAsync.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Converts old lifestyle structures to new standards.
        /// </summary>
        private void LegacyShim(XmlNode xmlLifestyleNode)
        {
            //Lifestyles would previously store the entire calculated value of their Cost, Area, Comforts and Security. Better to have it be a volatile Complex Property.
            if (_objCharacter.LastSavedVersion > new ValueVersion(5, 197, 0) || xmlLifestyleNode["costforarea"] != null)
                return;
            XPathNavigator objXmlDocument = _objCharacter.LoadDataXPath("lifestyles.xml");
            using (LockObject.EnterWriteLock())
            {
                XPathNavigator objLifestyleQualityNode
                    = objXmlDocument.TryGetNodeByNameOrId("/chummer/lifestyles/lifestyle", BaseLifestyle);
                if (objLifestyleQualityNode != null)
                {
                    decimal decTemp = 0.0m;
                    if (objLifestyleQualityNode.TryGetDecFieldQuickly("cost", ref decTemp))
                        Cost = decTemp;
                    if (objLifestyleQualityNode.TryGetDecFieldQuickly("costforarea", ref decTemp))
                        CostForArea = decTemp;
                    if (objLifestyleQualityNode.TryGetDecFieldQuickly("costforcomforts", ref decTemp))
                        CostForComforts = decTemp;
                    if (objLifestyleQualityNode.TryGetDecFieldQuickly("costforsecurity", ref decTemp))
                        CostForSecurity = decTemp;
                }

                int intMinArea = 0;
                int intMinComfort = 0;
                int intMinSec = 0;
                int intMaxArea = 0;
                int intMaxComfort = 0;
                int intMaxSec = 0;

                // Calculate the limits of the 3 aspects.
                // Area.
                XPathNavigator objXmlNode
                    = objXmlDocument.TryGetNodeByNameOrId("/chummer/neighborhoods/neighborhood", BaseLifestyle);
                objXmlNode.TryGetInt32FieldQuickly("minimum", ref intMinArea);
                objXmlNode.TryGetInt32FieldQuickly("limit", ref intMaxArea);
                BaseArea = intMinArea;
                AreaMaximum = Math.Max(intMaxArea, intMinArea);
                // Comforts.
                objXmlNode = objXmlDocument.TryGetNodeByNameOrId("/chummer/comforts/comfort", BaseLifestyle);
                objXmlNode.TryGetInt32FieldQuickly("minimum", ref intMinComfort);
                objXmlNode.TryGetInt32FieldQuickly("limit", ref intMaxComfort);
                BaseComforts = intMinComfort;
                ComfortsMaximum = Math.Max(intMaxComfort, intMinComfort);
                // Security.
                objXmlNode = objXmlDocument.TryGetNodeByNameOrId("/chummer/securities/security", BaseLifestyle);
                objXmlNode.TryGetInt32FieldQuickly("minimum", ref intMinSec);
                objXmlNode.TryGetInt32FieldQuickly("limit", ref intMaxSec);
                BaseSecurity = intMinSec;
                SecurityMaximum = Math.Max(intMaxSec, intMinSec);

                xmlLifestyleNode.TryGetInt32FieldQuickly("area", ref intMinArea);
                xmlLifestyleNode.TryGetInt32FieldQuickly("comforts", ref intMinComfort);
                xmlLifestyleNode.TryGetInt32FieldQuickly("security", ref intMinSec);

                // Calculate the cost of Positive Qualities.
                foreach (LifestyleQuality objQuality in LifestyleQualities.Where(
                             x => x.OriginSource != QualitySource.BuiltIn))
                {
                    intMinArea -= objQuality.Area;
                    intMinComfort -= objQuality.Comforts;
                    intMinSec -= objQuality.Security;
                }

                Area = Math.Max(intMinArea - BaseArea, 0);
                Comforts = Math.Max(intMinComfort - BaseComforts, 0);
                Security = Math.Max(intMinSec - BaseSecurity, 0);
            }
        }

        /// <summary>
        /// Converts old lifestyle structures to new standards.
        /// </summary>
        private async Task LegacyShimAsync(XmlNode xmlLifestyleNode, CancellationToken token = default)
        {
            //Lifestyles would previously store the entire calculated value of their Cost, Area, Comforts and Security. Better to have it be a volatile Complex Property.
            if (_objCharacter.LastSavedVersion > new ValueVersion(5, 197, 0) || xmlLifestyleNode["costforarea"] != null)
                return;
            XPathNavigator objXmlDocument = await _objCharacter.LoadDataXPathAsync("lifestyles.xml", token: token).ConfigureAwait(false);
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                string strBaseLifestyle = await GetBaseLifestyleAsync(token).ConfigureAwait(false);
                XPathNavigator objLifestyleQualityNode
                    = objXmlDocument.TryGetNodeByNameOrId("/chummer/lifestyles/lifestyle", strBaseLifestyle);
                if (objLifestyleQualityNode != null)
                {
                    decimal decTemp = 0.0m;
                    if (objLifestyleQualityNode.TryGetDecFieldQuickly("cost", ref decTemp))
                        await SetCostAsync(decTemp, token).ConfigureAwait(false);
                    if (objLifestyleQualityNode.TryGetDecFieldQuickly("costforarea", ref decTemp))
                        CostForArea = decTemp;
                    if (objLifestyleQualityNode.TryGetDecFieldQuickly("costforcomforts", ref decTemp))
                        CostForComforts = decTemp;
                    if (objLifestyleQualityNode.TryGetDecFieldQuickly("costforsecurity", ref decTemp))
                        CostForSecurity = decTemp;
                }

                int intMinArea = 0;
                int intMinComfort = 0;
                int intMinSec = 0;
                int intMaxArea = 0;
                int intMaxComfort = 0;
                int intMaxSec = 0;

                // Calculate the limits of the 3 aspects.
                // Area.
                XPathNavigator objXmlNode
                    = objXmlDocument.TryGetNodeByNameOrId("/chummer/neighborhoods/neighborhood", strBaseLifestyle);
                objXmlNode.TryGetInt32FieldQuickly("minimum", ref intMinArea);
                objXmlNode.TryGetInt32FieldQuickly("limit", ref intMaxArea);
                BaseArea = intMinArea;
                AreaMaximum = Math.Max(intMaxArea, intMinArea);
                // Comforts.
                objXmlNode = objXmlDocument.TryGetNodeByNameOrId("/chummer/comforts/comfort", strBaseLifestyle);
                objXmlNode.TryGetInt32FieldQuickly("minimum", ref intMinComfort);
                objXmlNode.TryGetInt32FieldQuickly("limit", ref intMaxComfort);
                BaseComforts = intMinComfort;
                ComfortsMaximum = Math.Max(intMaxComfort, intMinComfort);
                // Security.
                objXmlNode = objXmlDocument.TryGetNodeByNameOrId("/chummer/securities/security", strBaseLifestyle);
                objXmlNode.TryGetInt32FieldQuickly("minimum", ref intMinSec);
                objXmlNode.TryGetInt32FieldQuickly("limit", ref intMaxSec);
                BaseSecurity = intMinSec;
                SecurityMaximum = Math.Max(intMaxSec, intMinSec);

                xmlLifestyleNode.TryGetInt32FieldQuickly("area", ref intMinArea);
                xmlLifestyleNode.TryGetInt32FieldQuickly("comforts", ref intMinComfort);
                xmlLifestyleNode.TryGetInt32FieldQuickly("security", ref intMinSec);

                // Calculate the cost of Positive Qualities.
                await LifestyleQualities.ForEachAsync(async objQuality =>
                {
                    if (await objQuality.GetOriginSourceAsync(token).ConfigureAwait(false) != QualitySource.BuiltIn)
                    {
                        intMinArea -= objQuality.Area;
                        intMinComfort -= objQuality.Comforts;
                        intMinSec -= objQuality.Security;
                    }
                }, token).ConfigureAwait(false);

                await SetAreaAsync(Math.Max(intMinArea - BaseArea, 0), token).ConfigureAwait(false);
                await SetComfortsAsync(Math.Max(intMinComfort - BaseComforts, 0), token).ConfigureAwait(false);
                await SetSecurityAsync(Math.Max(intMinSec - BaseSecurity, 0), token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
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
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                // <lifestyle>
                XmlElementWriteHelper objBaseElement
                    = await objWriter.StartElementAsync("lifestyle", token).ConfigureAwait(false);
                try
                {
                    await objWriter.WriteElementStringAsync("guid", InternalId, token).ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("sourceid", SourceIDString, token).ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("name", CustomName, token).ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("allowsnameoverride", AllowsNameOverride.ToString(GlobalSettings.InvariantCultureInfo), token).ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("city", await GetCityAsync(token).ConfigureAwait(false), token).ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("district", await GetDistrictAsync(token).ConfigureAwait(false), token).ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("borough", await GetBoroughAsync(token).ConfigureAwait(false), token).ConfigureAwait(false);
                    string strNuyenFormat = await (await _objCharacter.GetSettingsAsync(token).ConfigureAwait(false)).GetNuyenFormatAsync(token).ConfigureAwait(false);
                    await objWriter
                        .WriteElementStringAsync(
                            "cost", (await GetCostAsync(token).ConfigureAwait(false)).ToString(strNuyenFormat, objCulture),
                            token).ConfigureAwait(false);
                    await objWriter
                        .WriteElementStringAsync("totalmonthlycost",
                            (await GetTotalMonthlyCostAsync(token: token).ConfigureAwait(false))
                            .ToString(
                                strNuyenFormat, objCulture), token)
                        .ConfigureAwait(false);
                    await objWriter
                        .WriteElementStringAsync("totalcost",
                            (await GetTotalCostAsync(token).ConfigureAwait(false)).ToString(
                                strNuyenFormat, objCulture),
                            token).ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("dice", (await GetDiceAsync(token).ConfigureAwait(false)).ToString(objCulture), token)
                        .ConfigureAwait(false);
                    await objWriter
                        .WriteElementStringAsync("multiplier",
                            (await GetMultiplierAsync(token).ConfigureAwait(false)).ToString(strNuyenFormat, objCulture),
                            token).ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("months", (await GetIncrementsAsync(token).ConfigureAwait(false)).ToString(objCulture), token)
                        .ConfigureAwait(false);
                    await objWriter
                        .WriteElementStringAsync("purchased", (await GetPurchasedAsync(token).ConfigureAwait(false)).ToString(GlobalSettings.InvariantCultureInfo),
                            token).ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("type", (await GetStyleTypeAsync(token).ConfigureAwait(false)).ToString(), token).ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("increment", (await GetIncrementTypeAsync(token).ConfigureAwait(false)).ToString(), token)
                        .ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("bonuslp", (await GetBonusLPAsync(token).ConfigureAwait(false)).ToString(objCulture), token)
                        .ConfigureAwait(false);
                    string strBaseLifestyle = await GetBaseLifestyleAsync(token).ConfigureAwait(false);
                    string strBaseLifestyleLocalized = strBaseLifestyle;
                    // Retrieve the Advanced Lifestyle information if applicable.
                    if (!string.IsNullOrEmpty(strBaseLifestyle))
                    {
                        XPathNavigator objXmlAspect = await this.GetNodeXPathAsync(token: token).ConfigureAwait(false);
                        if (objXmlAspect != null)
                        {
                            strBaseLifestyle = objXmlAspect.SelectSingleNodeAndCacheExpression("name", token)?.Value ?? strBaseLifestyle;
                            strBaseLifestyleLocalized
                                = objXmlAspect.SelectSingleNodeAndCacheExpression("translate", token)?.Value
                                  ?? strBaseLifestyle;
                        }
                    }

                    await objWriter.WriteElementStringAsync("baselifestyle", strBaseLifestyleLocalized, token)
                        .ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("baselifestyle_english", strBaseLifestyle, token)
                        .ConfigureAwait(false);
                    await objWriter
                        .WriteElementStringAsync("trustfund", TrustFund.ToString(GlobalSettings.InvariantCultureInfo),
                            token).ConfigureAwait(false);
                    await objWriter
                        .WriteElementStringAsync(
                            "source",
                            await _objCharacter.LanguageBookShortAsync(Source, strLanguageToPrint, token)
                                .ConfigureAwait(false), token).ConfigureAwait(false);
                    await objWriter
                        .WriteElementStringAsync(
                            "page", await DisplayPageAsync(strLanguageToPrint, token).ConfigureAwait(false), token)
                        .ConfigureAwait(false);

                    // <qualities>
                    XmlElementWriteHelper objQualitiesElement
                        = await objWriter.StartElementAsync("qualities", token).ConfigureAwait(false);
                    try
                    {
                        // Retrieve the Qualities for the Advanced Lifestyle if applicable.
                        await LifestyleQualities.ForEachAsync(objQuality => objQuality.Print(objWriter, objCulture, strLanguageToPrint, token), token).ConfigureAwait(false);
                    }
                    finally
                    {
                        // </qualities>
                        await objQualitiesElement.DisposeAsync().ConfigureAwait(false);
                    }

                    if (GlobalSettings.PrintNotes)
                        await objWriter.WriteElementStringAsync("notes", await GetNotesAsync(token).ConfigureAwait(false), token).ConfigureAwait(false);
                }
                finally
                {
                    // </lifestyle>
                    await objBaseElement.DisposeAsync().ConfigureAwait(false);
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        #endregion Constructor, Create, Save, Load, and Print Methods

        #region Properties

        /// <summary>
        /// Internal identifier which will be used to identify this Lifestyle in the Improvement system.
        /// </summary>
        public string InternalId
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _guiID.ToString("D", GlobalSettings.InvariantCultureInfo);
            }
        }

        /// <summary>
        /// Identifier of the object within data files.
        /// </summary>
        public Guid SourceID
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _guiSourceID;
            }
            set
            {
                using (LockObject.EnterReadLock())
                {
                    if (_guiSourceID == value)
                        return;
                }

                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (_guiSourceID == value)
                        return;
                    using (LockObject.EnterWriteLock())
                    {
                        _guiSourceID = value;
                        OnPropertyChanged();
                    }
                }
            }
        }

        /// <summary>
        /// String-formatted identifier of the <inheritdoc cref="SourceID"/> from the data files.
        /// </summary>
        public string SourceIDString => SourceID.ToString("D", GlobalSettings.InvariantCultureInfo);

        /// <summary>
        /// A custom name for the Lifestyle assigned by the player.
        /// </summary>
        public string Name
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _strName;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (Interlocked.Exchange(ref _strName, value) != value)
                        OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// A custom name for the Lifestyle assigned by the player.
        /// </summary>
        public async Task<string> GetNameAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _strName;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// A custom name for the Lifestyle assigned by the player.
        /// </summary>
        public async Task SetNameAsync(string value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (Interlocked.Exchange(ref _strName, value) != value)
                    await OnPropertyChangedAsync(nameof(Name), token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public string CustomName
        {
            get => Name;
            set => Name = value;
        }

        /// <summary>
        /// Whether the XML data allows name override for this item.
        /// For lifestyles, this is typically false as they use Name directly.
        /// </summary>
        public bool AllowsNameOverride
        {
            get => _blnAllowsNameOverride;
            set => _blnAllowsNameOverride = value;
        }

        /// <summary>
        /// The name of the object as it should be displayed on printouts (translated name only).
        /// </summary>
        public string DisplayNameShort(string strLanguage)
        {
            // Get the translated name if applicable.
            if (strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                return BaseLifestyle;

            using (LockObject.EnterReadLock())
            {
                return this.GetNodeXPath(strLanguage)?.SelectSingleNodeAndCacheExpression("translate")?.Value
                       ?? BaseLifestyle;
            }
        }

        /// <summary>
        /// The name of the object as it should be displayed on printouts (translated name only).
        /// </summary>
        public async Task<string> DisplayNameShortAsync(string strLanguage, CancellationToken token = default)
        {
            // Get the translated name if applicable.
            if (strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                return BaseLifestyle;

            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                XPathNavigator objNode = await this.GetNodeXPathAsync(strLanguage, token: token).ConfigureAwait(false);
                return objNode?.SelectSingleNodeAndCacheExpression("translate", token)?.Value ?? BaseLifestyle;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// The name of the object as it should be displayed in lists. Name (Extra).
        /// </summary>
        public string DisplayName(string strLanguage)
        {
            using (LockObject.EnterReadLock())
            {
                string strReturn = DisplayNameShort(strLanguage);

                if (!string.IsNullOrEmpty(CustomName))
                    strReturn += LanguageManager.GetString("String_Space") + "(\"" + CustomName + "\")";

                return strReturn;
            }
        }

        /// <summary>
        /// The name of the object as it should be displayed in lists. Name (Extra).
        /// </summary>
        public async Task<string> DisplayNameAsync(string strLanguage, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                string strReturn = await DisplayNameShortAsync(strLanguage, token).ConfigureAwait(false);

                if (!string.IsNullOrEmpty(CustomName))
                    strReturn += await LanguageManager.GetStringAsync("String_Space", token: token)
                                                      .ConfigureAwait(false) + "(\"" + CustomName + "\")";

                return strReturn;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public string CurrentDisplayName => DisplayName(GlobalSettings.Language);

        public string CurrentDisplayNameShort => DisplayNameShort(GlobalSettings.Language);

        public Task<string> GetCurrentDisplayNameAsync(CancellationToken token = default) => DisplayNameAsync(GlobalSettings.Language, token);

        public Task<string> GetCurrentDisplayNameShortAsync(CancellationToken token = default) => DisplayNameShortAsync(GlobalSettings.Language, token);

        /// <summary>
        /// Sourcebook.
        /// </summary>
        public string Source
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _strSource;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
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
                using (LockObject.EnterReadLock())
                    return _strPage;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
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
            using (LockObject.EnterReadLock())
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
        public async Task<string> DisplayPageAsync(string strLanguage, CancellationToken token = default)
        {
            if (strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                return Page;
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                XPathNavigator objNode = await this.GetNodeXPathAsync(strLanguage, token: token).ConfigureAwait(false);
                string strReturn = objNode?.SelectSingleNodeAndCacheExpression("altpage", token: token)?.Value ?? Page;
                return !string.IsNullOrWhiteSpace(strReturn) ? strReturn : Page;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Cost.
        /// </summary>
        public decimal Cost
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _decCost;
            }
            set
            {
                using (LockObject.EnterReadLock())
                {
                    if (_decCost == value)
                        return;
                }

                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (_decCost == value)
                        return;
                    using (LockObject.EnterWriteLock())
                    {
                        _decCost = value;
                        OnPropertyChanged();
                    }
                }
            }
        }

        /// <summary>
        /// Free Lifestyle points from Traveler lifestyle.
        /// </summary>
        public async Task<decimal> GetCostAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _decCost;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Cost
        /// </summary>
        public async Task SetCostAsync(decimal value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_decCost == value)
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            token.ThrowIfCancellationRequested();
            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_decCost == value)
                    return;
                IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    _decCost = value;
                    await OnPropertyChangedAsync(nameof(Cost), token).ConfigureAwait(false);
                }
                finally
                {
                    await objLocker2.DisposeAsync().ConfigureAwait(false);
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Number of dice the character rolls to determine their starting Nuyen.
        /// </summary>
        public int Dice
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _intDice;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (Interlocked.Exchange(ref _intDice, value) != value)
                        OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Number of dice the character rolls to determine their starting Nuyen.
        /// </summary>
        public async Task<int> GetDiceAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _intDice;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Number of dice the character rolls to determine their starting Nuyen.
        /// </summary>
        public async Task SetDiceAsync(int value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (Interlocked.Exchange(ref _intDice, value) != value)
                    await OnPropertyChangedAsync(nameof(Dice), token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Number the character multiplies the dice roll with to determine their starting Nuyen.
        /// </summary>
        public decimal Multiplier
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _decMultiplier;
            }
            set
            {
                using (LockObject.EnterReadLock())
                {
                    if (_decMultiplier == value)
                        return;
                }

                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (_decMultiplier == value)
                        return;
                    using (LockObject.EnterWriteLock())
                    {
                        _decMultiplier = value;
                        OnPropertyChanged();
                    }
                }
            }
        }

        /// <summary>
        /// Number the character multiplies the dice roll with to determine their starting Nuyen.
        /// </summary>
        public async Task<decimal> GetMultiplierAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _decMultiplier;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Number the character multiplies the dice roll with to determine their starting Nuyen.
        /// </summary>
        public async Task SetMultiplierAsync(decimal value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_decMultiplier == value)
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }

            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_decMultiplier == value)
                    return;
                IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    _decMultiplier = value;
                    await OnPropertyChangedAsync(nameof(Multiplier), token).ConfigureAwait(false);
                }
                finally
                {
                    await objLocker2.DisposeAsync().ConfigureAwait(false);
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Months/Weeks/Days purchased.
        /// </summary>
        public int Increments
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _intIncrements;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (Interlocked.Exchange(ref _intIncrements, value) != value)
                        OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Months/Weeks/Days purchased.
        /// </summary>
        public async Task<int> GetIncrementsAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _intIncrements;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Months/Weeks/Days purchased.
        /// </summary>
        public async Task SetIncrementsAsync(int value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (Interlocked.Exchange(ref _intIncrements, value) != value)
                    await OnPropertyChangedAsync(nameof(Increments), token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Months/Weeks/Days purchased.
        /// </summary>
        public async Task ModifyIncrementsAsync(int value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (value == 0)
                return;
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                Interlocked.Add(ref _intIncrements, value);
                await OnPropertyChangedAsync(nameof(Increments), token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Whether the Lifestyle has been Purchased and no longer rented.
        /// </summary>
        public bool Purchased
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return Increments >= IncrementsRequiredForPermanent;
            }
        }

        /// <summary>
        /// Whether the Lifestyle has been Purchased and no longer rented.
        /// </summary>
        public async Task<bool> GetPurchasedAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return await GetIncrementsAsync(token).ConfigureAwait(false) >= await GetIncrementsRequiredForPermanentAsync(token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public int IncrementsRequiredForPermanent
        {
            get
            {
                switch (IncrementType)
                {
                    case LifestyleIncrement.Day:
                        return 3044; // 30.436875 days per month on average * 100 months, rounded up
                    case LifestyleIncrement.Week:
                        return 435; // 4.348125 weeks per month on average * 100 months, rounded up
                    default:
                        return 100;
                }
            }
        }

        public async Task<int> GetIncrementsRequiredForPermanentAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            switch (await GetIncrementTypeAsync(token).ConfigureAwait(false))
            {
                case LifestyleIncrement.Day:
                    return 3044; // 30.436875 days per month on average * 100 months, rounded up
                case LifestyleIncrement.Week:
                    return 435; // 4.348125 weeks per month on average * 100 months, rounded up
                default:
                    return 100;
            }
        }

        /// <summary>
        /// Base Lifestyle.
        /// </summary>
        public string BaseLifestyle
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _strBaseLifestyle;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (Interlocked.Exchange(ref _strBaseLifestyle, value) == value)
                        return;
                    XmlDocument xmlLifestyleDocument = _objCharacter.LoadData("lifestyles.xml");
                    using (LockObject.EnterWriteLock())
                    {
                        // This needs a handler for translations, will fix later.
                        LifestyleQuality objNotAHomeQuality = LifestyleQualities.FirstOrDefault(
                                                                  x => x.Name == "Not a Home"
                                                                       && x.OriginSource == QualitySource.BuiltIn)
                                                              ?? LifestyleQualities.FirstOrDefault(x =>
                                                                  x.Name == "Not a Home");
                        if (value == "Bolt Hole")
                        {
                            if (objNotAHomeQuality == null)
                            {
                                XmlNode xmlQuality
                                    = xmlLifestyleDocument.SelectSingleNode(
                                        "/chummer/qualities/quality[name = \"Not a Home\"]");
                                LifestyleQuality objQuality = new LifestyleQuality(_objCharacter);
                                try
                                {
                                    objQuality.Create(xmlQuality, this, _objCharacter, QualitySource.BuiltIn);
                                    LifestyleQualities.Add(objQuality);
                                }
                                catch
                                {
                                    objQuality.Remove(false);
                                    throw;
                                }
                            }
                            else if (objNotAHomeQuality.OriginSource != QualitySource.BuiltIn)
                                objNotAHomeQuality.OriginSource = QualitySource.BuiltIn;
                        }
                        else if (objNotAHomeQuality?.OriginSource == QualitySource.BuiltIn)
                        {
                            objNotAHomeQuality.OriginSource = QualitySource.Selected;
                        }

                        XmlNode xmlLifestyle
                            = xmlLifestyleDocument.TryGetNodeByNameOrId("/chummer/lifestyles/lifestyle", value);
                        if (xmlLifestyle != null)
                        {
                            _strBaseLifestyle = string.Empty;
                            _decCost = 0;
                            _intDice = 0;
                            _decMultiplier = 0;
                            _strSource = string.Empty;
                            _strPage = string.Empty;
                            _intLP = 0;
                            _decCostForArea = 0;
                            _decCostForComforts = 0;
                            _decCostForSecurity = 0;
                            _blnAllowBonusLP = false;
                            _eIncrement = LifestyleIncrement.Month;
                            _intBaseComforts = 0;
                            _intComfortsMaximum = 0;
                            _intBaseArea = 0;
                            _intAreaMaximum = 0;
                            _intBaseSecurity = 0;
                            _intSecurityMaximum = 0;
                            Create(xmlLifestyle);
                            this.OnMultiplePropertyChanged(nameof(BaseLifestyle), nameof(Cost), nameof(Dice),
                                nameof(Multiplier), nameof(SourceID), nameof(Source),
                                nameof(Page), nameof(LP), nameof(CostForArea),
                                nameof(CostForComforts), nameof(CostForSecurity),
                                nameof(AllowBonusLP), nameof(IncrementType),
                                nameof(BaseComforts), nameof(ComfortsMaximum),
                                nameof(BaseArea), nameof(AreaMaximum), nameof(BaseSecurity),
                                nameof(SecurityMaximum), nameof(LifestyleQualities));
                            return;
                        }
                        OnPropertyChanged();
                    }
                }
            }
        }

        /// <summary>
        /// Base Lifestyle.
        /// </summary>
        public async Task<string> GetBaseLifestyleAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _strBaseLifestyle;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Base Lifestyle.
        /// </summary>
        public async Task SetBaseLifestyleAsync(string value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_strBaseLifestyle == value)
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            token.ThrowIfCancellationRequested();
            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (Interlocked.Exchange(ref _strBaseLifestyle, value) == value)
                    return;
                XmlDocument xmlLifestyleDocument = await _objCharacter.LoadDataAsync("lifestyles.xml", token: token).ConfigureAwait(false);
                IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    // This needs a handler for translations, will fix later.
                    LifestyleQuality objNotAHomeQuality = await LifestyleQualities.FirstOrDefaultAsync(
                                                              async x =>
                                                                  await x.GetNameAsync(token).ConfigureAwait(false) ==
                                                                  "Not a Home" &&
                                                                  await x.GetOriginSourceAsync(token)
                                                                      .ConfigureAwait(false) == QualitySource.BuiltIn,
                                                              token: token).ConfigureAwait(false)
                                                          ?? await LifestyleQualities.FirstOrDefaultAsync(
                                                              async x => await x.GetNameAsync(token)
                                                                  .ConfigureAwait(false) == "Not a Home",
                                                              token: token).ConfigureAwait(false);
                    if (value == "Bolt Hole")
                    {
                        if (objNotAHomeQuality == null)
                        {
                            XmlNode xmlQuality
                                = xmlLifestyleDocument.SelectSingleNode(
                                    "/chummer/qualities/quality[name = \"Not a Home\"]");
                            LifestyleQuality objQuality = new LifestyleQuality(_objCharacter);
                            try
                            {
                                await objQuality
                                    .CreateAsync(xmlQuality, this, _objCharacter, QualitySource.BuiltIn, token: token)
                                    .ConfigureAwait(false);
                                await LifestyleQualities.AddAsync(objQuality, token).ConfigureAwait(false);
                            }
                            catch
                            {
                                await objQuality.RemoveAsync(false, CancellationToken.None).ConfigureAwait(false);
                                throw;
                            }
                        }
                        else if (await objNotAHomeQuality.GetOriginSourceAsync(token).ConfigureAwait(false) !=
                                 QualitySource.BuiltIn)
                        {
                            await objNotAHomeQuality.SetOriginSourceAsync(QualitySource.BuiltIn, token)
                                .ConfigureAwait(false);
                        }
                    }
                    else if (objNotAHomeQuality != null
                             && await objNotAHomeQuality.GetOriginSourceAsync(token).ConfigureAwait(false) == QualitySource.BuiltIn)
                    {
                        await objNotAHomeQuality.SetOriginSourceAsync(QualitySource.Selected, token).ConfigureAwait(false);
                    }

                    XmlNode xmlLifestyle
                        = xmlLifestyleDocument.TryGetNodeByNameOrId("/chummer/lifestyles/lifestyle", value);
                    if (xmlLifestyle != null)
                    {
                        _strBaseLifestyle = string.Empty;
                        _decCost = 0;
                        _intDice = 0;
                        _decMultiplier = 0;
                        _strSource = string.Empty;
                        _strPage = string.Empty;
                        _intLP = 0;
                        _decCostForArea = 0;
                        _decCostForComforts = 0;
                        _decCostForSecurity = 0;
                        _blnAllowBonusLP = false;
                        _eIncrement = LifestyleIncrement.Month;
                        _intBaseComforts = 0;
                        _intComfortsMaximum = 0;
                        _intBaseArea = 0;
                        _intAreaMaximum = 0;
                        _intBaseSecurity = 0;
                        _intSecurityMaximum = 0;
                        await CreateAsync(xmlLifestyle, token).ConfigureAwait(false);
                        await this.OnMultiplePropertyChangedAsync(token, nameof(BaseLifestyle), nameof(Cost), nameof(Dice),
                            nameof(Multiplier), nameof(SourceID), nameof(Source),
                            nameof(Page), nameof(LP), nameof(CostForArea),
                            nameof(CostForComforts), nameof(CostForSecurity),
                            nameof(AllowBonusLP), nameof(IncrementType),
                            nameof(BaseComforts), nameof(ComfortsMaximum),
                            nameof(BaseArea), nameof(AreaMaximum), nameof(BaseSecurity),
                            nameof(SecurityMaximum), nameof(LifestyleQualities)).ConfigureAwait(false);
                        return;
                    }

                    await OnPropertyChangedAsync(nameof(BaseLifestyle), token).ConfigureAwait(false);
                }
                finally
                {
                    await objLocker2.DisposeAsync().ConfigureAwait(false);
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Base Lifestyle Points awarded by the lifestyle.
        /// </summary>
        public int LP
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _intLP;
            }
        }

        /// <summary>
        /// Total LP cost of the Lifestyle, including all qualities, roommates, bonus LP, etc.
        /// </summary>
        public int TotalLP
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return LP - Comforts - Area - Security +
                           // HT 140: Bonus LP cannot exceed 2x base LP
                           Math.Min(Roommates + BonusLP - LifestyleQualities.Sum(x => x.LPCost < 0, x => x.LPCost),
                               2*LP) -
                           LifestyleQualities.Sum(x => x.LPCost > 0, x => x.LPCost);
            }
        }

        public async Task<int> GetTotalLPAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return LP - await GetComfortsAsync(token).ConfigureAwait(false)
                          - await GetAreaAsync(token).ConfigureAwait(false)
                          - await GetSecurityAsync(token).ConfigureAwait(false)
                       // HT 140: Bonus LP cannot exceed 2x base LP
                       + Math.Min(await GetRoommatesAsync(token).ConfigureAwait(false) +
                                  await GetBonusLPAsync(token).ConfigureAwait(false) -
                                  await LifestyleQualities
                                      .SumAsync(x => x.LPCost < 0, x => x.GetLPCostAsync(token), token)
                                      .ConfigureAwait(false), 2*LP) -
                       await LifestyleQualities.SumAsync(x => x.LPCost > 0, x => x.GetLPCostAsync(token), token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Free Lifestyle points from Traveler lifestyle.
        /// </summary>
        public int BonusLP
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _intBonusLP;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (Interlocked.Exchange(ref _intBonusLP, value) != value)
                        OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Free Lifestyle points from Traveler lifestyle.
        /// </summary>
        public async Task<int> GetBonusLPAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _intBonusLP;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Free Lifestyle points from Traveler lifestyle.
        /// </summary>
        public async Task SetBonusLPAsync(int value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (Interlocked.Exchange(ref _intBonusLP, value) != value)
                    await OnPropertyChangedAsync(nameof(BonusLP), token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public bool AllowBonusLP
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _blnAllowBonusLP;
            }
            set
            {
                using (LockObject.EnterReadLock())
                {
                    if (_blnAllowBonusLP == value)
                        return;
                }

                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (_blnAllowBonusLP == value)
                        return;
                    using (LockObject.EnterWriteLock())
                    {
                        _blnAllowBonusLP = value;
                        OnPropertyChanged();
                    }
                }
            }
        }

        public async Task<bool> GetAllowBonusLPAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _blnAllowBonusLP;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public async Task SetAllowBonusLPAsync(bool value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_blnAllowBonusLP == value)
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            token.ThrowIfCancellationRequested();
            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_blnAllowBonusLP == value)
                    return;
                IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    _blnAllowBonusLP = value;
                    await OnPropertyChangedAsync(nameof(AllowBonusLP), token).ConfigureAwait(false);
                }
                finally
                {
                    await objLocker2.DisposeAsync().ConfigureAwait(false);
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Base level of Comforts.
        /// </summary>
        public int BaseComforts
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _intBaseComforts;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (Interlocked.Exchange(ref _intBaseComforts, value) != value)
                        OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Advance Lifestyle Neighborhood Entertainment.
        /// </summary>
        public int BaseArea
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _intBaseArea;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (Interlocked.Exchange(ref _intBaseArea, value) != value)
                        OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Advance Lifestyle Security Entertainment.
        /// </summary>
        public int BaseSecurity
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _intBaseSecurity;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (Interlocked.Exchange(ref _intBaseSecurity, value) != value)
                        OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Advance Lifestyle Comforts.
        /// </summary>
        public int Comforts
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _intComforts;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (Interlocked.Exchange(ref _intComforts, value) != value)
                        OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Advance Lifestyle Comforts.
        /// </summary>
        public async Task<int> GetComfortsAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _intComforts;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Advance Lifestyle Comforts.
        /// </summary>
        public async Task SetComfortsAsync(int value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (Interlocked.Exchange(ref _intComforts, value) != value)
                    await OnPropertyChangedAsync(nameof(Comforts), token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Advance Lifestyle Neighborhood.
        /// </summary>
        public int Area
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _intArea;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (Interlocked.Exchange(ref _intArea, value) != value)
                        OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Advance Lifestyle Neighborhood.
        /// </summary>
        public async Task<int> GetAreaAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _intArea;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Advance Lifestyle Neighborhood.
        /// </summary>
        public async Task SetAreaAsync(int value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (Interlocked.Exchange(ref _intArea, value) != value)
                    await OnPropertyChangedAsync(nameof(Area), token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Advance Lifestyle Security.
        /// </summary>
        public int Security
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _intSecurity;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (Interlocked.Exchange(ref _intSecurity, value) != value)
                        OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Advance Lifestyle Security.
        /// </summary>
        public async Task<int> GetSecurityAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _intSecurity;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Advance Lifestyle Security.
        /// </summary>
        public async Task SetSecurityAsync(int value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (Interlocked.Exchange(ref _intSecurity, value) != value)
                    await OnPropertyChangedAsync(nameof(Security), token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public int AreaMaximum
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _intAreaMaximum;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (Interlocked.Exchange(ref _intAreaMaximum, value) != value)
                        OnPropertyChanged();
                }
            }
        }

        public int ComfortsMaximum
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _intComfortsMaximum;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (Interlocked.Exchange(ref _intComfortsMaximum, value) != value)
                        OnPropertyChanged();
                }
            }
        }

        public int SecurityMaximum
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _intSecurityMaximum;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (Interlocked.Exchange(ref _intSecurityMaximum, value) != value)
                        OnPropertyChanged();
                }
            }
        }

        public int TotalComfortsMaximum
        {
            get
            {
                using (LockObject.EnterReadLock())
                {
                    return ComfortsMaximum
                           + LifestyleQualities.Sum(x => x.OriginSource != QualitySource.BuiltIn,
                                                    lq => lq.ComfortsMaximum);
                }
            }
        }

        public async Task<int> GetTotalComfortsMaximumAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return ComfortsMaximum
                       + await LifestyleQualities
                               .SumAsync(x => x.OriginSource != QualitySource.BuiltIn, lq => lq.ComfortsMaximum, token)
                               .ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public int TotalSecurityMaximum
        {
            get
            {
                using (LockObject.EnterReadLock())
                {
                    return SecurityMaximum
                           + LifestyleQualities.Sum(x => x.OriginSource != QualitySource.BuiltIn,
                                                    lq => lq.SecurityMaximum);
                }
            }
        }

        public async Task<int> GetTotalSecurityMaximumAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return SecurityMaximum
                       + await LifestyleQualities
                               .SumAsync(x => x.OriginSource != QualitySource.BuiltIn, lq => lq.SecurityMaximum, token)
                               .ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public int TotalAreaMaximum
        {
            get
            {
                using (LockObject.EnterReadLock())
                {
                    return AreaMaximum
                           + LifestyleQualities.Sum(x => x.OriginSource != QualitySource.BuiltIn,
                                                    lq => lq.AreaMaximum);
                }
            }
        }

        public async Task<int> GetTotalAreaMaximumAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return AreaMaximum
                       + await LifestyleQualities
                               .SumAsync(x => x.OriginSource != QualitySource.BuiltIn, lq => lq.AreaMaximum, token)
                               .ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        private readonly ThreadSafeObservableCollection<LifestyleQuality> _lstLifestyleQualities;

        /// <summary>
        /// Advanced Lifestyle Qualities.
        /// </summary>
        public ThreadSafeObservableCollection<LifestyleQuality> LifestyleQualities
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _lstLifestyleQualities;
            }
        }

        /// <summary>
        /// Notes.
        /// </summary>
        public string Notes
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _strNotes;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (Interlocked.Exchange(ref _strNotes, value) != value)
                        OnPropertyChanged();
                }
            }
        }

        public async Task<string> GetNotesAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _strNotes;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public async Task SetNotesAsync(string value, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                // No need to write lock because interlocked guarantees safety
                if (Interlocked.Exchange(ref _strNotes, value) == value)
                    return;
                await OnPropertyChangedAsync(nameof(Notes), token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Forecolor to use for Notes in treeviews.
        /// </summary>
        public Color NotesColor
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _colNotes;
            }
            set
            {
                using (LockObject.EnterReadLock())
                {
                    if (_colNotes == value)
                        return;
                }

                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (_colNotes == value)
                        return;
                    using (LockObject.EnterWriteLock())
                    {
                        _colNotes = value;
                        OnPropertyChanged();
                    }
                }
            }
        }

        public async Task<Color> GetNotesColorAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _colNotes;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public async Task SetNotesColorAsync(Color value, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (value == _colNotes)
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }

            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_colNotes == value)
                    return;
                IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    _colNotes = value;
                    await OnPropertyChangedAsync(nameof(NotesColor), token).ConfigureAwait(false);
                }
                finally
                {
                    await objLocker2.DisposeAsync().ConfigureAwait(false);
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Type of the Lifestyle.
        /// </summary>
        public LifestyleType StyleType
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _eType;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (InterlockedExtensions.Exchange(ref _eType, value) != value)
                        OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Type of the Lifestyle.
        /// </summary>
        public async Task<LifestyleType> GetStyleTypeAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _eType;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Type of the Lifestyle.
        /// </summary>
        public async Task SetStyleTypeAsync(LifestyleType value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (InterlockedExtensions.Exchange(ref _eType, value) != value)
                    await OnPropertyChangedAsync(nameof(StyleType), token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Interval of payments required for the Lifestyle.
        /// </summary>
        public LifestyleIncrement IncrementType
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _eIncrement;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (InterlockedExtensions.Exchange(ref _eIncrement, value) != value)
                        OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Interval of payments required for the Lifestyle.
        /// </summary>
        public async Task<LifestyleIncrement> GetIncrementTypeAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _eIncrement;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Interval of payments required for the Lifestyle.
        /// </summary>
        public async Task SetIncrementTypeAsync(LifestyleIncrement value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (InterlockedExtensions.Exchange(ref _eIncrement, value) != value)
                    await OnPropertyChangedAsync(nameof(IncrementType), token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Number of Roommates this Lifestyle is shared with.
        /// </summary>
        public int Roommates
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _intRoommates;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (Interlocked.Exchange(ref _intRoommates, value) != value)
                        OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Number of Roommates this Lifestyle is shared with.
        /// </summary>
        public async Task<int> GetRoommatesAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _intRoommates;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Number of Roommates this Lifestyle is shared with.
        /// </summary>
        public async Task SetRoommatesAsync(int value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (Interlocked.Exchange(ref _intRoommates, value) != value)
                    await OnPropertyChangedAsync(nameof(Roommates), token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Percentage of the total cost the character pays per month.
        /// </summary>
        public decimal Percentage
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _decPercentage;
            }
            set
            {
                using (LockObject.EnterReadLock())
                {
                    if (_decPercentage == value)
                        return;
                }

                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (_decPercentage == value)
                        return;
                    using (LockObject.EnterWriteLock())
                    {
                        _decPercentage = value;
                        OnPropertyChanged();
                    }
                }
            }
        }

        public async Task<decimal> GetPercentageAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _decPercentage;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public async Task SetPercentageAsync(decimal value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_decPercentage == value)
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            token.ThrowIfCancellationRequested();
            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_decPercentage == value)
                    return;
                IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    _decPercentage = value;
                    await OnPropertyChangedAsync(nameof(Percentage), token).ConfigureAwait(false);
                }
                finally
                {
                    await objLocker2.DisposeAsync().ConfigureAwait(false);
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Whether the lifestyle is currently covered by the Trust Fund Quality.
        /// </summary>
        public bool TrustFund
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _blnTrustFund && IsTrustFundEligible;
            }
            set
            {
                using (LockObject.EnterReadLock())
                {
                    if (_blnTrustFund == value)
                        return;
                }

                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (_blnTrustFund == value)
                        return;
                    using (LockObject.EnterWriteLock())
                    {
                        _blnTrustFund = value;
                        OnPropertyChanged();
                    }
                }
            }
        }

        /// <summary>
        /// Whether the lifestyle is currently covered by the Trust Fund Quality.
        /// </summary>
        public async Task<bool> GetTrustFundAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _blnTrustFund && await GetIsTrustFundEligibleAsync(token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Whether the lifestyle is currently covered by the Trust Fund Quality.
        /// </summary>
        public async Task SetTrustFundAsync(bool value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_blnTrustFund == value)
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            token.ThrowIfCancellationRequested();
            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_blnTrustFund == value)
                    return;
                IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    _blnTrustFund = value;
                    await OnPropertyChangedAsync(nameof(TrustFund), token).ConfigureAwait(false);
                }
                finally
                {
                    await objLocker2.DisposeAsync().ConfigureAwait(false);
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public bool IsTrustFundEligible => StaticIsTrustFundEligible(_objCharacter, BaseLifestyle);

        public async Task<bool> GetIsTrustFundEligibleAsync(CancellationToken token = default)
        {
            return await StaticIsTrustFundEligibleAsync(_objCharacter,
                await GetBaseLifestyleAsync(token).ConfigureAwait(false), token).ConfigureAwait(false);
        }

        public static bool StaticIsTrustFundEligible(Character objCharacter, string strBaseLifestyle)
        {
            switch (objCharacter.TrustFund)
            {
                case 1:
                case 4:
                    return strBaseLifestyle == "Medium";

                case 2:
                    return strBaseLifestyle == "Low";

                case 3:
                    return strBaseLifestyle == "High";
            }
            return false;
        }

        public static async Task<bool> StaticIsTrustFundEligibleAsync(Character objCharacter, string strBaseLifestyle, CancellationToken token = default)
        {
            switch (await objCharacter.GetTrustFundAsync(token).ConfigureAwait(false))
            {
                case 1:
                case 4:
                    return strBaseLifestyle == "Medium";

                case 2:
                    return strBaseLifestyle == "Low";

                case 3:
                    return strBaseLifestyle == "High";
            }
            return false;
        }

        /// <summary>
        /// Whether the character shares the costs of the Lifestyle with their roommates.
        /// </summary>
        public bool SplitCostWithRoommates
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _blnSplitCostWithRoommates && Roommates > 0 && !TrustFund;
            }
            set
            {
                using (LockObject.EnterReadLock())
                {
                    if (_blnSplitCostWithRoommates == value)
                        return;
                }

                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (_blnSplitCostWithRoommates == value)
                        return;
                    using (LockObject.EnterWriteLock())
                    {
                        _blnSplitCostWithRoommates = value;
                        OnPropertyChanged();
                    }
                }
            }
        }

        /// <summary>
        /// Whether the character shares the costs of the Lifestyle with their roommates.
        /// </summary>
        public async Task<bool> GetSplitCostWithRoommatesAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _blnSplitCostWithRoommates && await GetRoommatesAsync(token).ConfigureAwait(false) > 0 &&
                       !await GetTrustFundAsync(token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Whether the character shares the costs of the Lifestyle with their roommates.
        /// </summary>
        public async Task SetSplitCostWithRoommatesAsync(bool value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_blnSplitCostWithRoommates == value)
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            token.ThrowIfCancellationRequested();
            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_blnSplitCostWithRoommates == value)
                    return;
                IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    _blnSplitCostWithRoommates = value;
                    await OnPropertyChangedAsync(nameof(SplitCostWithRoommates), token).ConfigureAwait(false);
                }
                finally
                {
                    await objLocker2.DisposeAsync().ConfigureAwait(false);
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Nuyen cost for each point of upgraded Security. Expected to be zero for lifestyles other than Street.
        /// </summary>
        public decimal CostForArea
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _decCostForArea;
            }
            set
            {
                using (LockObject.EnterReadLock())
                {
                    if (_decCostForArea == value)
                        return;
                }

                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (_decCostForArea == value)
                        return;
                    using (LockObject.EnterWriteLock())
                    {
                        _decCostForArea = value;
                        OnPropertyChanged();
                    }
                }
            }
        }

        /// <summary>
        /// Nuyen cost for each point of upgraded Security. Expected to be zero for lifestyles other than Street.
        /// </summary>
        public decimal CostForComforts
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _decCostForComforts;
            }
            set
            {
                using (LockObject.EnterReadLock())
                {
                    if (_decCostForComforts == value)
                        return;
                }

                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (_decCostForComforts == value)
                        return;
                    using (LockObject.EnterWriteLock())
                    {
                        _decCostForComforts = value;
                        OnPropertyChanged();
                    }
                }
            }
        }

        /// <summary>
        /// Nuyen cost for each point of upgraded Security. Expected to be zero for lifestyles other than Street.
        /// </summary>
        public decimal CostForSecurity
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _decCostForSecurity;
            }
            set
            {
                using (LockObject.EnterReadLock())
                {
                    if (_decCostForSecurity == value)
                        return;
                }

                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (_decCostForSecurity == value)
                        return;
                    using (LockObject.EnterWriteLock())
                    {
                        _decCostForSecurity = value;
                        OnPropertyChanged();
                    }
                }
            }
        }

        /// <summary>
        /// Used by our sorting algorithm to remember which order the user moves things to
        /// </summary>
        public int SortOrder
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _intSortOrder;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (Interlocked.Exchange(ref _intSortOrder, value) != value)
                        OnPropertyChanged();
                }
            }
        }

        private XmlNode _objCachedMyXmlNode;
        private string _strCachedXmlNodeLanguage = string.Empty;

        public async Task<XmlNode> GetNodeCoreAsync(bool blnSync, string strLanguage, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IDisposable objLocker = null;
            IAsyncDisposable objLockerAsync = null;
            if (blnSync)
                // ReSharper disable once MethodHasAsyncOverload
                objLocker = LockObject.EnterReadLock(token);
            else
                objLockerAsync = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                XmlNode objReturn = _objCachedMyXmlNode;
                if (objReturn != null && strLanguage == _strCachedXmlNodeLanguage
                                      && !GlobalSettings.LiveCustomData)
                    return objReturn;
                XmlDocument objDoc = blnSync
                    // ReSharper disable once MethodHasAsyncOverload
                    ? _objCharacter.LoadData("lifestyles.xml", strLanguage, token: token)
                    : await _objCharacter.LoadDataAsync("lifestyles.xml", strLanguage, token: token).ConfigureAwait(false);
                objReturn = objDoc.TryGetNodeById("/chummer/lifestyles/lifestyle", SourceID);
                if (objReturn == null && SourceID != Guid.Empty)
                {
                    objReturn = objDoc.TryGetNodeByNameOrId("/chummer/lifestyles/gear", Name);
                    objReturn?.TryGetGuidFieldQuickly("id", ref _guiSourceID);
                }
                _objCachedMyXmlNode = objReturn;
                _strCachedXmlNodeLanguage = strLanguage;
                return objReturn;
            }
            finally
            {
                objLocker?.Dispose();
                if (objLockerAsync != null)
                    await objLockerAsync.DisposeAsync().ConfigureAwait(false);
            }
        }

        private XPathNavigator _objCachedMyXPathNode;
        private string _strCachedXPathNodeLanguage = string.Empty;

        public async Task<XPathNavigator> GetNodeXPathCoreAsync(bool blnSync, string strLanguage, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IDisposable objLocker = null;
            IAsyncDisposable objLockerAsync = null;
            if (blnSync)
                // ReSharper disable once MethodHasAsyncOverload
                objLocker = LockObject.EnterReadLock(token);
            else
                objLockerAsync = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                XPathNavigator objReturn = _objCachedMyXPathNode;
                if (objReturn != null && strLanguage == _strCachedXPathNodeLanguage
                                      && !GlobalSettings.LiveCustomData)
                    return objReturn;
                XPathNavigator objDoc = blnSync
                    // ReSharper disable once MethodHasAsyncOverload
                    ? _objCharacter.LoadDataXPath("lifestyles.xml", strLanguage, token: token)
                    : await _objCharacter.LoadDataXPathAsync("lifestyles.xml", strLanguage, token: token).ConfigureAwait(false);
                if (SourceID != Guid.Empty)
                    objReturn = objDoc.TryGetNodeById("/chummer/lifestyles/lifestyle", SourceID);
                if (objReturn == null)
                {
                    objReturn = objDoc.TryGetNodeByNameOrId("/chummer/lifestyles/lifestyle", Name);
                    objReturn?.TryGetGuidFieldQuickly("id", ref _guiSourceID);
                }
                _objCachedMyXPathNode = objReturn;
                _strCachedXPathNodeLanguage = strLanguage;
                return objReturn;
            }
            finally
            {
                objLocker?.Dispose();
                if (objLockerAsync != null)
                    await objLockerAsync.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Calculates the Expected Value of an Lifestyle at chargen under the assumption that the average value was rolled
        /// </summary>
        public decimal ExpectedValue
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return 3.5m * Dice * Multiplier;
            }
        }

        /// <summary>
        /// Calculates the Expected Value of an Lifestyle at chargen under the assumption that the average value was rolled
        /// </summary>
        public async Task<decimal> GetExpectedValueAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return 3.5m * await GetDiceAsync(token).ConfigureAwait(false) * await GetMultiplierAsync(token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public string City
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _strCity;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (Interlocked.Exchange(ref _strCity, value) != value)
                        OnPropertyChanged();
                }
            }
        }

        public async Task<string> GetCityAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _strCity;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public async Task SetCityAsync(string value, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (Interlocked.Exchange(ref _strCity, value) != value)
                    await OnPropertyChangedAsync(nameof(City), token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public string District
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _strDistrict;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (Interlocked.Exchange(ref _strDistrict, value) != value)
                        OnPropertyChanged();
                }
            }
        }

        public async Task<string> GetDistrictAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _strDistrict;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public async Task SetDistrictAsync(string value, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (Interlocked.Exchange(ref _strDistrict, value) != value)
                    await OnPropertyChangedAsync(nameof(District), token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public string Borough
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _strBorough;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (Interlocked.Exchange(ref _strBorough, value) != value)
                        OnPropertyChanged();
                }
            }
        }

        public async Task<string> GetBoroughAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _strBorough;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public async Task SetBoroughAsync(string value, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (Interlocked.Exchange(ref _strBorough, value) != value)
                    await OnPropertyChangedAsync(nameof(Borough), token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        #endregion Properties

        #region Complex Properties

        /// <summary>
        /// Total cost of the Lifestyle, counting all purchased months.
        /// </summary>
        public decimal TotalCost
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return GetTotalMonthlyCost() * Increments;
            }
        }

        /// <summary>
        /// Total cost of the Lifestyle, counting all purchased months.
        /// </summary>
        public async Task<decimal> GetTotalCostAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return await GetTotalMonthlyCostAsync(token: token).ConfigureAwait(false) * await GetIncrementsAsync(token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Total Area of the Lifestyle, including all Lifestyle qualities.
        /// </summary>
        public int TotalArea
        {
            get
            {
                using (LockObject.EnterReadLock())
                {
                    return BaseArea + Area
                                    + LifestyleQualities.Sum(x => x.OriginSource != QualitySource.BuiltIn,
                                                             lq => lq.Area);
                }
            }
        }

        /// <summary>
        /// Total Area of the Lifestyle, including all Lifestyle qualities.
        /// </summary>
        public async Task<int> GetTotalAreaAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return BaseArea + await GetAreaAsync(token).ConfigureAwait(false)
                                + await LifestyleQualities.SumAsync(
                                    async x => await x.GetOriginSourceAsync(token).ConfigureAwait(false) != QualitySource.BuiltIn,
                                    lq => lq.Area, token: token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Total Comforts of the Lifestyle, including all Lifestyle qualities.
        /// </summary>
        public int TotalComforts
        {
            get
            {
                using (LockObject.EnterReadLock())
                {
                    return BaseComforts + Comforts
                                        + LifestyleQualities.Sum(
                                            x => x.OriginSource != QualitySource.BuiltIn, lq => lq.Comforts);
                }
            }
        }

        /// <summary>
        /// Total Comforts of the Lifestyle, including all Lifestyle qualities.
        /// </summary>
        public async Task<int> GetTotalComfortsAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return BaseComforts + await GetComfortsAsync(token).ConfigureAwait(false)
                                    + await LifestyleQualities.SumAsync(
                                            async x => await x.GetOriginSourceAsync(token).ConfigureAwait(false) != QualitySource.BuiltIn,
                                            lq => lq.Comforts, token: token)
                                        .ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Total Security of the Lifestyle, including all Lifestyle qualities.
        /// </summary>
        public int TotalSecurity
        {
            get
            {
                using (LockObject.EnterReadLock())
                {
                    return BaseSecurity + Security
                                        + LifestyleQualities.Sum(
                                            x => x.OriginSource != QualitySource.BuiltIn, lq => lq.Security);
                }
            }
        }

        /// <summary>
        /// Total Security of the Lifestyle, including all Lifestyle qualities.
        /// </summary>
        public async Task<int> GetTotalSecurityAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return BaseSecurity + await GetSecurityAsync(token).ConfigureAwait(false)
                                    + await LifestyleQualities.SumAsync(
                                            async x => await x.GetOriginSourceAsync(token).ConfigureAwait(false) != QualitySource.BuiltIn,
                                            lq => lq.Security, token: token)
                                        .ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public decimal AreaDelta
        {
            get
            {
                using (LockObject.EnterReadLock())
                {
                    return Math.Max(
                        TotalAreaMaximum - (BaseArea
                                            + LifestyleQualities.Sum(x => x.OriginSource != QualitySource.BuiltIn,
                                                                     lq => lq.Area)), 0);
                }
            }
        }

        public async Task<decimal> GetAreaDeltaAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return Math.Max(
                    await GetTotalAreaMaximumAsync(token).ConfigureAwait(false) - (BaseArea + await LifestyleQualities
                        .SumAsync(async x => await x.GetOriginSourceAsync(token).ConfigureAwait(false) != QualitySource.BuiltIn,
                            lq => lq.Area, token: token)
                        .ConfigureAwait(false)), 0);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public decimal ComfortsDelta
        {
            get
            {
                using (LockObject.EnterReadLock())
                {
                    return Math.Max(
                        TotalComfortsMaximum - (BaseComforts
                                                + LifestyleQualities.Sum(x => x.OriginSource != QualitySource.BuiltIn,
                                                    lq => lq.Comforts)), 0);
                }
            }
        }

        public async Task<decimal> GetComfortsDeltaAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return Math.Max(
                    await GetTotalComfortsMaximumAsync(token).ConfigureAwait(false) - (BaseComforts
                        + await LifestyleQualities
                            .SumAsync(async x => await x.GetOriginSourceAsync(token).ConfigureAwait(false) != QualitySource.BuiltIn,
                                lq => lq.Comforts, token: token)
                            .ConfigureAwait(false)), 0);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public decimal SecurityDelta
        {
            get
            {
                using (LockObject.EnterReadLock())
                {
                    return Math.Max(
                        TotalSecurityMaximum - (BaseSecurity
                                                + LifestyleQualities.Sum(x => x.OriginSource != QualitySource.BuiltIn,
                                                    lq => lq.Security)), 0);
                }
            }
        }

        public async Task<decimal> GetSecurityDeltaAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return Math.Max(
                    await GetTotalSecurityMaximumAsync(token).ConfigureAwait(false) - (BaseSecurity
                        + await LifestyleQualities
                            .SumAsync(async x => await x.GetOriginSourceAsync(token).ConfigureAwait(false) != QualitySource.BuiltIn,
                                lq => lq.Security, token: token)
                            .ConfigureAwait(false)), 0);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public string FormattedArea
        {
            get
            {
                using (LockObject.EnterReadLock())
                {
                    return string.Format(GlobalSettings.CultureInfo,
                                         LanguageManager.GetString("Label_SelectAdvancedLifestyle_Base"),
                                         BaseArea.ToString(GlobalSettings.CultureInfo),
                                         TotalAreaMaximum.ToString(GlobalSettings.CultureInfo));
                }
            }
        }

        public async Task<string> GetFormattedAreaAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return string.Format(
                    GlobalSettings.CultureInfo,
                    await LanguageManager.GetStringAsync("Label_SelectAdvancedLifestyle_Base", token: token)
                                         .ConfigureAwait(false), BaseArea.ToString(GlobalSettings.CultureInfo),
                    (await GetTotalAreaMaximumAsync(token).ConfigureAwait(false)).ToString(GlobalSettings.CultureInfo));
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public string FormattedComforts
        {
            get
            {
                using (LockObject.EnterReadLock())
                {
                    return string.Format(GlobalSettings.CultureInfo,
                                         LanguageManager.GetString("Label_SelectAdvancedLifestyle_Base"),
                                         BaseComforts.ToString(GlobalSettings.CultureInfo),
                                         TotalComfortsMaximum.ToString(GlobalSettings.CultureInfo));
                }
            }
        }

        public async Task<string> GetFormattedComfortsAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return string.Format(
                    GlobalSettings.CultureInfo,
                    await LanguageManager.GetStringAsync("Label_SelectAdvancedLifestyle_Base", token: token)
                                         .ConfigureAwait(false), BaseComforts.ToString(GlobalSettings.CultureInfo),
                    (await GetTotalComfortsMaximumAsync(token).ConfigureAwait(false)).ToString(
                        GlobalSettings.CultureInfo));
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public string FormattedSecurity
        {
            get
            {
                using (LockObject.EnterReadLock())
                {
                    return string.Format(GlobalSettings.CultureInfo,
                                         LanguageManager.GetString("Label_SelectAdvancedLifestyle_Base"),
                                         BaseSecurity.ToString(GlobalSettings.CultureInfo),
                                         TotalSecurityMaximum.ToString(GlobalSettings.CultureInfo));
                }
            }
        }

        public async Task<string> GetFormattedSecurityAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return string.Format(
                    GlobalSettings.CultureInfo,
                    await LanguageManager.GetStringAsync("Label_SelectAdvancedLifestyle_Base", token: token)
                                         .ConfigureAwait(false), BaseSecurity.ToString(GlobalSettings.CultureInfo),
                    (await GetTotalSecurityMaximumAsync(token).ConfigureAwait(false)).ToString(
                        GlobalSettings.CultureInfo));
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Cost of the Lifestyle itself that is to be split among roommates if there are any. Does not include outing, service, and contract costs or character-based lifestyle cost adjustments.
        /// </summary>
        public decimal CostPreSplit
        {
            get
            {
                using (LockObject.EnterReadLock())
                {
                    // Follow steps on HT 139 before split.
                    // Base cost
                    decimal decCost = Cost;

                    // Base cost multiplier
                    decimal decMultiplier = 1.0m;
                    foreach (LifestyleQuality lq in LifestyleQualities)
                    {
                        if (lq.OriginSource != QualitySource.BuiltIn)
                        {
                            decimal decInnerMultiplier = lq.BaseMultiplier;
                            if (decInnerMultiplier != 0)
                                decMultiplier *= 1.0m + decInnerMultiplier / 100.0m;
                        }
                    }
                    if (decMultiplier != 1.0m)
                        decCost *= decMultiplier;

                    // Add in costs from lifestyle categories.
                    decMultiplier = Area + Comforts + Security;
                    if (decMultiplier != 0)
                        decCost *= 1.0m + 0.1m * decMultiplier;
                    decCost += Area * CostForArea + Comforts * CostForComforts + Security * CostForSecurity;

                    // Add costs from Entertainment Assets.
                    decMultiplier = 1.0m;
                    decimal decFlatCosts = LifestyleQualities.Sum(
                        x => x.OriginSource != QualitySource.BuiltIn && x.Type == QualityType.Entertainment &&
                                 x.Category.Contains("Asset"),
                        lq =>
                        {
                            decimal decInnerMultiplier = lq.Multiplier;
                            if (decInnerMultiplier != 0)
                                decMultiplier *= 1.0m + decInnerMultiplier / 100.0m;
                            return lq.Cost;
                        });
                    // Flat costs added after multipliers, as per HT 139
                    if (decMultiplier != 1.0m)
                        decCost *= decMultiplier;
                    decCost += decFlatCosts;

                    // Add costs from qualities that are not Entertainment Assets (or Contracts)
                    decMultiplier = 1.0m;
                    decFlatCosts = LifestyleQualities.Sum(
                        x => x.OriginSource != QualitySource.BuiltIn && x.Type != QualityType.Entertainment && x.Type != QualityType.Contracts,
                        lq =>
                        {
                            decimal decInnerMultiplier = lq.Multiplier;
                            if (decInnerMultiplier != 0)
                                decMultiplier *= 1.0m + decInnerMultiplier / 100.0m;
                            return lq.Cost;
                        });
                    // Flat costs added after multipliers, as per HT 139
                    if (decMultiplier != 1.0m)
                        decCost *= decMultiplier;
                    decCost += decFlatCosts;

                    // Shared lifestyle adjustment
                    if (Roommates > 0)
                        decCost *= 1.0m + 0.1m * Roommates;

                    //Qualities may have reduced the cost below zero. No spooky mansion payouts here, so clamp it to zero or higher.
                    return Math.Max(decCost, 0);
                }
            }
        }

        /// <summary>
        /// Cost of the Lifestyle itself that is to be split among roommates if there are any. Does not include outing, service, and contract costs or character-based lifestyle cost adjustments.
        /// </summary>
        public async Task<decimal> GetCostPreSplitAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                // Follow steps on HT 139 before split.
                // Base cost
                decimal decCost = await GetCostAsync(token).ConfigureAwait(false);

                // Base cost multiplier
                decimal decMultiplier = 1.0m;
                await LifestyleQualities.ForEachAsync(async x =>
                {
                    if (await x.GetOriginSourceAsync(token).ConfigureAwait(false) != QualitySource.BuiltIn)
                    {
                        decimal decInnerMultiplier = await x.GetBaseMultiplierAsync(token).ConfigureAwait(false);
                        if (decInnerMultiplier != 0)
                            decMultiplier *= 1.0m + decInnerMultiplier / 100.0m;
                    }
                }, token).ConfigureAwait(false);
                if (decMultiplier != 1.0m)
                    decCost *= decMultiplier;

                // Add in costs from lifestyle categories.
                int intArea = await GetAreaAsync(token).ConfigureAwait(false);
                int intComforts = await GetComfortsAsync(token).ConfigureAwait(false);
                int intSecurity = await GetSecurityAsync(token).ConfigureAwait(false);
                decMultiplier = (intArea + intComforts + intSecurity) * 0.1m;
                if (decMultiplier != 0)
                    decCost *= 1.0m + decMultiplier;
                decCost += intArea * CostForArea + intComforts * CostForComforts + intSecurity * CostForSecurity;

                // Add costs from Entertainment Assets.
                decMultiplier = 1.0m;
                decimal decFlatCosts = await LifestyleQualities.SumAsync(
                    async x => await x.GetOriginSourceAsync(token).ConfigureAwait(false) != QualitySource.BuiltIn && x.Type == QualityType.Entertainment &&
                               x.Category.Contains("Asset"),
                    async lq =>
                    {
                        decimal decInnerMultiplier = await lq.GetMultiplierAsync(token).ConfigureAwait(false);
                        if (decInnerMultiplier != 0)
                            decMultiplier *= 1.0m + decInnerMultiplier / 100.0m;
                        return await lq.GetCostAsync(token).ConfigureAwait(false);
                    }, token).ConfigureAwait(false);
                // Flat costs added after multipliers, as per HT 139
                if (decMultiplier != 1.0m)
                    decCost *= decMultiplier;
                decCost += decFlatCosts;

                // Add costs from qualities that are not Entertainment Assets (or Contracts)
                decMultiplier = 1.0m;
                decFlatCosts = await LifestyleQualities.SumAsync(
                    async x => await x.GetOriginSourceAsync(token).ConfigureAwait(false) != QualitySource.BuiltIn && x.Type != QualityType.Entertainment &&
                               x.Type != QualityType.Contracts,
                    async lq =>
                    {
                        decimal decInnerMultiplier = await lq.GetMultiplierAsync(token).ConfigureAwait(false);
                        if (decInnerMultiplier != 0)
                            decMultiplier *= 1.0m + decInnerMultiplier / 100.0m;
                        return await lq.GetCostAsync(token).ConfigureAwait(false);
                    }, token).ConfigureAwait(false);
                // Flat costs added after multipliers, as per HT 139
                if (decMultiplier != 1.0m)
                    decCost *= decMultiplier;
                decCost += decFlatCosts;

                // Shared lifestyle adjustment
                int intRoommates = await GetRoommatesAsync(token).ConfigureAwait(false);
                if (intRoommates > 0)
                    decCost *= 1.0m + 0.1m * intRoommates;

                //Qualities may have reduced the cost below zero. No spooky mansion payouts here, so clamp it to zero or higher.
                return Math.Max(decCost, 0);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Total monthly cost of the Lifestyle.
        /// </summary>
        public decimal GetTotalMonthlyCost(bool blnIncorporateOnceOffAdjustments = true)
        {
            decimal decReturn = 0;

            using (LockObject.EnterReadLock())
            {
                if (!TrustFund)
                {
                    decReturn += CostPreSplit;

                    // Follow HT 139 from Split step onwards
                    if (SplitCostWithRoommates)
                    {
                        decReturn /= Roommates + 1.0m;
                    }
                }

                string strBaseLifestyle = BaseLifestyle;
                // Factor in character-based lifestyle cost adjustments (metatype, dependents, augmentations, etc.)
                List<Improvement> lstImprovements = ImprovementManager.GetCachedImprovementListForValueOf(
                    _objCharacter,
                    Improvement.ImprovementType.LifestyleCost,
                    strImprovedName: strBaseLifestyle, blnIncludeNonImproved: true);
                if (StyleType == LifestyleType.Standard)
                {
                    lstImprovements.AddRange(ImprovementManager.GetCachedImprovementListForValueOf(
                        _objCharacter,
                        Improvement.ImprovementType.BasicLifestyleCost,
                        strImprovedName: strBaseLifestyle, blnIncludeNonImproved: true));
                }
                if (blnIncorporateOnceOffAdjustments)
                {
                    List<Improvement> lstOnceOffImprovements = StyleType == LifestyleType.Standard
                        ? _objCharacter.Improvements.FindAll(x =>
                            x.Enabled && (x.ImproveType == Improvement.ImprovementType.LifestyleCost || x.ImproveType == Improvement.ImprovementType.BasicLifestyleCost)
                            && x.Condition == "once"
                            && (string.IsNullOrEmpty(x.ImprovedName) || x.ImprovedName == strBaseLifestyle))
                        : _objCharacter.Improvements.FindAll(x =>
                            x.Enabled && x.ImproveType == Improvement.ImprovementType.LifestyleCost
                            && x.Condition == "once"
                            && (string.IsNullOrEmpty(x.ImprovedName) || x.ImprovedName == strBaseLifestyle));
                    if (lstOnceOffImprovements.Count > 0)
                    {
                        Dictionary<string, List<Improvement>> dicGenericOnceOffImprovementsByUnique = new Dictionary<string, List<Improvement>>(lstOnceOffImprovements.Count);
                        Dictionary<string, List<Improvement>> dicSpecificOnceOffImprovementsByUnique = new Dictionary<string, List<Improvement>>(lstOnceOffImprovements.Count);
                        foreach (Improvement objImprovement in lstOnceOffImprovements)
                        {
                            string strUnique = objImprovement.UniqueName;
                            Dictionary<string, List<Improvement>> dicToUse = string.IsNullOrEmpty(objImprovement.ImprovedName)
                                ? dicGenericOnceOffImprovementsByUnique
                                : dicSpecificOnceOffImprovementsByUnique;
                            if (dicToUse.TryGetValue(strUnique, out List<Improvement> lstLoop))
                                lstLoop.Add(objImprovement);
                            else
                            {
                                lstLoop = new List<Improvement>(lstOnceOffImprovements.Count)
                                    {
                                        objImprovement
                                    };
                                dicToUse.Add(strUnique, lstLoop);
                            }
                        }
                        if (dicGenericOnceOffImprovementsByUnique.Count > 0 || dicSpecificOnceOffImprovementsByUnique.Count > 0)
                        {
                            // We want to make sure that the largest discounts affect the costliest lifestyle first and the largest markups affect the smallest first
                            List<Lifestyle> lstLifestyles = dicGenericOnceOffImprovementsByUnique.Count > 0
                                ? _objCharacter.Lifestyles.ToList()
                                : _objCharacter.Lifestyles.FindAll(x => x.BaseLifestyle == strBaseLifestyle);
                            Dictionary<Lifestyle, decimal> dicLifestyleCosts = new Dictionary<Lifestyle, decimal>(lstLifestyles.Count);
                            foreach (Lifestyle objLoop in lstLifestyles)
                            {
                                decimal decCost = objLoop.GetTotalMonthlyCost(false);
                                switch (objLoop.IncrementType)
                                {
                                    case LifestyleIncrement.Day:
                                        decCost *= 4.34812m / 7;
                                        break;

                                    case LifestyleIncrement.Week:
                                        decCost *= 4.34812m;
                                        break;
                                }
                                dicLifestyleCosts.Add(objLoop, decCost);
                            }
                            lstLifestyles.Sort((x, y) =>
                            {
                                dicLifestyleCosts.TryGetValue(x, out decimal decLeftCost);
                                dicLifestyleCosts.TryGetValue(y, out decimal decRightCost);
                                return decRightCost.CompareTo(decLeftCost);
                            });
                            int intMyIndex = lstLifestyles.IndexOf(this);
                            foreach (List<Improvement> lstLoop in dicGenericOnceOffImprovementsByUnique.Values)
                            {
                                if (lstLoop.Count <= intMyIndex)
                                    continue; // We have so few improvements in this unique group that this lifestyle would not be affected, so skip
                                lstLoop.Sort((x, y) => x.Value.CompareTo(y.Value));
                                lstImprovements.Add(lstLoop[intMyIndex]);
                            }
                            if (dicGenericOnceOffImprovementsByUnique.Count > 0)
                            {
                                intMyIndex = 0;
                                foreach (Lifestyle objLoop in lstLifestyles)
                                {
                                    if (ReferenceEquals(objLoop, this))
                                        break;
                                    if (objLoop.BaseLifestyle == strBaseLifestyle)
                                        ++intMyIndex;
                                }
                            }
                            foreach (List<Improvement> lstLoop in dicSpecificOnceOffImprovementsByUnique.Values)
                            {
                                if (lstLoop.Count <= intMyIndex)
                                    continue; // We have so few improvements in this unique group that this lifestyle would not be affected, so skip
                                lstLoop.Sort((x, y) => x.Value.CompareTo(y.Value));
                                lstImprovements.Add(lstLoop[intMyIndex]);
                            }
                        }
                    }
                }
                // Dependents and metatype adjustments are handled separately
                List<Quality> lstDependentsQualities =
                    _objCharacter.Qualities.Where(x => x.Name.Contains("Dependent")).ToList();
                decimal decDependents = 0;
                decimal decMetatype = 0;
                decimal decOther = 1.0m;
                foreach (Improvement objImprovement in lstImprovements)
                {
                    if (objImprovement.ImproveSource == Improvement.ImprovementSource.Quality &&
                        lstDependentsQualities.Any(x => x.InternalId == objImprovement.SourceName))
                        decDependents += objImprovement.Value;
                    else if (objImprovement.ImproveSource == Improvement.ImprovementSource.Heritage
                                || objImprovement.ImproveSource == Improvement.ImprovementSource.Metatype
                                || objImprovement.ImproveSource == Improvement.ImprovementSource.Metavariant)
                        decMetatype += objImprovement.Value;
                    else
                        decOther *= 1.0m + objImprovement.Value / 100.0m;
                }
                // Dependents first
                if (decDependents != 0)
                    decReturn *= 1.0m + decDependents / 100.0m;
                // Metatype next
                if (decMetatype != 0)
                    decReturn *= 1.0m + decMetatype / 100.0m;
                // Finally, everything else
                if (decOther != 1.0m)
                    decReturn *= decOther;

                // Add in Outings and Services costs
                decimal decContractCost = 0;
                decimal decOutingsAndServicesCost = 0;
                decimal decMultiplier = 1.0m;
                decimal decBaseMultiplier = 1.0m;
                foreach (LifestyleQuality objQuality in LifestyleQualities)
                {
                    if (objQuality.OriginSource == QualitySource.BuiltIn)
                        continue;
                    if (objQuality.Type == QualityType.Contracts)
                        decContractCost += objQuality.Cost;
                    else if (objQuality.Type == QualityType.Entertainment && !objQuality.Category.Contains("Asset"))
                    {
                        decOutingsAndServicesCost += objQuality.Cost;
                        decMultiplier *= 1.0m + objQuality.Multiplier / 100.0m;
                        decBaseMultiplier *= 1.0m + objQuality.BaseMultiplier / 100.0m;
                    }
                }
                // Flat costs added after multipliers, as per HT 139
                if (decMultiplier != 1.0m)
                    decReturn *= decMultiplier;
                decReturn += decOutingsAndServicesCost;
                if (decBaseMultiplier != 1.0m)
                    decReturn += Cost * decBaseMultiplier;

                // Apply final percentage modifier
                decReturn *= Percentage / 100;

                // Add contract costs, which are technically not a part of the lifestyle and are just used as a more automated way to deal with recurring costs
                switch (IncrementType)
                {
                    case LifestyleIncrement.Day:
                        decContractCost /= 4.34812m * 7;
                        break;

                    case LifestyleIncrement.Week:
                        decContractCost /= 4.34812m;
                        break;
                }

                decReturn += decContractCost;
            }

            return decReturn;
        }

        /// <summary>
        /// Total monthly cost of the Lifestyle.
        /// </summary>
        public async Task<decimal> GetTotalMonthlyCostAsync(bool blnIncorporateOnceOffAdjustments = true, CancellationToken token = default)
        {
            decimal decReturn = 0;
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (!await GetTrustFundAsync(token).ConfigureAwait(false))
                {
                    decReturn += await GetCostPreSplitAsync(token).ConfigureAwait(false);

                    // Follow HT 139 from Split step onwards
                    if (await GetSplitCostWithRoommatesAsync(token).ConfigureAwait(false))
                    {
                        decReturn /= await GetRoommatesAsync(token).ConfigureAwait(false) + 1.0m;
                    }
                }

                string strBaseLifestyle = await GetBaseLifestyleAsync(token).ConfigureAwait(false);
                // Factor in character-based lifestyle cost adjustments (metatype, dependents, augmentations, etc.)
                List<Improvement> lstImprovements = await ImprovementManager.GetCachedImprovementListForValueOfAsync(
                    _objCharacter,
                    Improvement.ImprovementType.LifestyleCost,
                    strImprovedName: strBaseLifestyle, blnIncludeNonImproved: true, token: token).ConfigureAwait(false);
                if (StyleType == LifestyleType.Standard)
                {
                    lstImprovements.AddRange(await ImprovementManager.GetCachedImprovementListForValueOfAsync(
                        _objCharacter,
                        Improvement.ImprovementType.BasicLifestyleCost,
                        strImprovedName: strBaseLifestyle, blnIncludeNonImproved: true, token: token).ConfigureAwait(false));
                }
                if (blnIncorporateOnceOffAdjustments)
                {
                    List<Improvement> lstOnceOffImprovements = new List<Improvement>(await _objCharacter.Improvements.GetCountAsync(token).ConfigureAwait(false));
                    if (StyleType == LifestyleType.Standard)
                    {
                        await _objCharacter.Improvements.ForEachAsync(x =>
                        {
                            if (x.Enabled && (x.ImproveType == Improvement.ImprovementType.LifestyleCost || x.ImproveType == Improvement.ImprovementType.BasicLifestyleCost)
                                && x.Condition == "once"
                                && (string.IsNullOrEmpty(x.ImprovedName) || x.ImprovedName == strBaseLifestyle))
                                lstOnceOffImprovements.Add(x);
                        }, token).ConfigureAwait(false);
                    }
                    else
                    {
                        await _objCharacter.Improvements.ForEachAsync(x =>
                        {
                            if (x.Enabled && x.ImproveType == Improvement.ImprovementType.LifestyleCost
                                && x.Condition == "once"
                                && (string.IsNullOrEmpty(x.ImprovedName) || x.ImprovedName == strBaseLifestyle))
                                lstOnceOffImprovements.Add(x);
                        }, token).ConfigureAwait(false);
                    }
                    if (lstOnceOffImprovements.Count > 0)
                    {
                        Dictionary<string, List<Improvement>> dicGenericOnceOffImprovementsByUnique = new Dictionary<string, List<Improvement>>(lstOnceOffImprovements.Count);
                        Dictionary<string, List<Improvement>> dicSpecificOnceOffImprovementsByUnique = new Dictionary<string, List<Improvement>>(lstOnceOffImprovements.Count);
                        foreach (Improvement objImprovement in lstOnceOffImprovements)
                        {
                            string strUnique = objImprovement.UniqueName;
                            Dictionary<string, List<Improvement>> dicToUse = string.IsNullOrEmpty(objImprovement.ImprovedName)
                                ? dicGenericOnceOffImprovementsByUnique
                                : dicSpecificOnceOffImprovementsByUnique;
                            if (dicToUse.TryGetValue(strUnique, out List<Improvement> lstLoop))
                                lstLoop.Add(objImprovement);
                            else
                            {
                                lstLoop = new List<Improvement>(lstOnceOffImprovements.Count)
                                    {
                                        objImprovement
                                    };
                                dicToUse.Add(strUnique, lstLoop);
                            }
                        }
                        if (dicGenericOnceOffImprovementsByUnique.Count > 0 || dicSpecificOnceOffImprovementsByUnique.Count > 0)
                        {
                            // We want to make sure that the largest discounts affect the costliest lifestyle first and the largest markups affect the smallest first
                            List<Lifestyle> lstLifestyles = dicGenericOnceOffImprovementsByUnique.Count > 0
                                ? await _objCharacter.Lifestyles.ToListAsync(token)
                                : await _objCharacter.Lifestyles.ToListAsync(async x => await x.GetBaseLifestyleAsync(token).ConfigureAwait(false) == strBaseLifestyle, token);
                            Dictionary<Lifestyle, decimal> dicLifestyleCosts = new Dictionary<Lifestyle, decimal>(lstLifestyles.Count);
                            foreach (Lifestyle objLoop in lstLifestyles)
                            {
                                decimal decCost = await objLoop.GetTotalMonthlyCostAsync(false, token).ConfigureAwait(false);
                                switch (objLoop.IncrementType)
                                {
                                    case LifestyleIncrement.Day:
                                        decCost *= 4.34812m / 7;
                                        break;

                                    case LifestyleIncrement.Week:
                                        decCost *= 4.34812m;
                                        break;
                                }
                                dicLifestyleCosts.Add(objLoop, decCost);
                            }
                            lstLifestyles.Sort((x, y) =>
                            {
                                dicLifestyleCosts.TryGetValue(x, out decimal decLeftCost);
                                dicLifestyleCosts.TryGetValue(y, out decimal decRightCost);
                                return decRightCost.CompareTo(decLeftCost);
                            });
                            int intMyIndex = lstLifestyles.IndexOf(this);
                            foreach (List<Improvement> lstLoop in dicGenericOnceOffImprovementsByUnique.Values)
                            {
                                if (lstLoop.Count <= intMyIndex)
                                    continue; // We have so few improvements in this unique group that this lifestyle would not be affected, so skip
                                lstLoop.Sort((x, y) => x.Value.CompareTo(y.Value));
                                lstImprovements.Add(lstLoop[intMyIndex]);
                            }
                            if (dicGenericOnceOffImprovementsByUnique.Count > 0)
                            {
                                intMyIndex = 0;
                                foreach (Lifestyle objLoop in lstLifestyles)
                                {
                                    if (ReferenceEquals(objLoop, this))
                                        break;
                                    if (await objLoop.GetBaseLifestyleAsync(token).ConfigureAwait(false) == strBaseLifestyle)
                                        ++intMyIndex;
                                }
                            }
                            foreach (List<Improvement> lstLoop in dicSpecificOnceOffImprovementsByUnique.Values)
                            {
                                if (lstLoop.Count <= intMyIndex)
                                    continue; // We have so few improvements in this unique group that this lifestyle would not be affected, so skip
                                lstLoop.Sort((x, y) => x.Value.CompareTo(y.Value));
                                lstImprovements.Add(lstLoop[intMyIndex]);
                            }
                        }
                    }
                }
                // Dependents and metatype adjustments are handled separately
                List<Quality> lstDependentsQualities =
                    await _objCharacter.Qualities.ToListAsync(
                        async x => (await x.GetNameAsync(token).ConfigureAwait(false)).Contains("Dependent"), token).ConfigureAwait(false);
                decimal decDependents = 0;
                decimal decMetatype = 0;
                decimal decOther = 1.0m;
                foreach (Improvement objImprovement in lstImprovements)
                {
                    if (objImprovement.ImproveSource == Improvement.ImprovementSource.Quality &&
                        lstDependentsQualities.Any(x => x.InternalId == objImprovement.SourceName))
                        decDependents += objImprovement.Value;
                    else if (objImprovement.ImproveSource == Improvement.ImprovementSource.Heritage
                             || objImprovement.ImproveSource == Improvement.ImprovementSource.Metatype
                             || objImprovement.ImproveSource == Improvement.ImprovementSource.Metavariant)
                        decMetatype += objImprovement.Value;
                    else
                        decOther *= 1.0m + objImprovement.Value / 100.0m;
                }
                // Dependents first
                if (decDependents != 0)
                    decReturn *= 1.0m + decDependents / 100.0m;
                // Metatype next
                if (decMetatype != 0)
                    decReturn *= 1.0m + decMetatype / 100.0m;
                // Finally, everything else
                if (decOther != 1.0m)
                    decReturn *= decOther;

                // Add in Outings and Services costs
                decimal decContractCost = 0;
                decimal decMultiplier = 1.0m;
                decimal decBaseMultiplier = 1.0m;
                decimal decOutingsAndServicesCost = await LifestyleQualities.SumAsync(
                    async x => await x.GetOriginSourceAsync(token).ConfigureAwait(false) != QualitySource.BuiltIn, async x =>
                    {
                        switch (x.Type)
                        {
                            case QualityType.Contracts:
                                decContractCost += await x.GetCostAsync(token).ConfigureAwait(false);
                                break;
                            case QualityType.Entertainment when !x.Category.Contains("Asset"):
                            {
                                decMultiplier *= 1.0m + (await x.GetMultiplierAsync(token).ConfigureAwait(false)) / 100.0m;
                                decBaseMultiplier *= 1.0m + (await x.GetBaseMultiplierAsync(token).ConfigureAwait(false)) / 100.0m;
                                return await x.GetCostAsync(token).ConfigureAwait(false);
                            }
                        }
                        return 0;
                    }, token).ConfigureAwait(false);
                // Flat costs added after multipliers, as per HT 139
                if (decMultiplier != 1.0m)
                    decReturn *= decMultiplier;
                decReturn += decOutingsAndServicesCost;
                if (decBaseMultiplier != 1.0m)
                    decReturn += await GetCostAsync(token).ConfigureAwait(false) * decBaseMultiplier;

                // Apply final percentage modifier
                decReturn *= await GetPercentageAsync(token).ConfigureAwait(false) / 100;

                // Add contract costs, which are technically not a part of the lifestyle and are just used as a more automated way to deal with recurring costs
                switch (IncrementType)
                {
                    case LifestyleIncrement.Day:
                        decContractCost /= 4.34812m * 7;
                        break;

                    case LifestyleIncrement.Week:
                        decContractCost /= 4.34812m;
                        break;
                }

                decReturn += decContractCost;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }

            return decReturn;
        }

        public string DisplayTotalMonthlyCost
        {
            get
            {
                using (LockObject.EnterReadLock())
                {
                    return GetTotalMonthlyCost().ToString(_objCharacter.Settings.NuyenFormat, GlobalSettings.CultureInfo)
                           + LanguageManager.GetString("String_NuyenSymbol");
                }
            }
        }

        public async Task<string> GetDisplayTotalMonthlyCostAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return (await GetTotalMonthlyCostAsync(token: token).ConfigureAwait(false)).ToString(
                           await (await _objCharacter.GetSettingsAsync(token).ConfigureAwait(false)).GetNuyenFormatAsync(token).ConfigureAwait(false),
                           GlobalSettings.CultureInfo)
                       + await LanguageManager.GetStringAsync("String_NuyenSymbol", token: token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public static string GetEquivalentLifestyle(string strLifestyle)
        {
            if (string.IsNullOrEmpty(strLifestyle))
                return string.Empty;
            switch (strLifestyle)
            {
                case "BOLTHOLE":
                case "BOLT HOLE":
                    return "Squatter";

                case "TRAVELER":
                    return "Low";

                case "COMMERCIAL":
                    return "Medium";

                default:
                    return strLifestyle.StartsWith("Hospitalized", StringComparison.OrdinalIgnoreCase) ? "High" : strLifestyle;
            }
        }

        #endregion Complex Properties

        #region Methods

        /// <summary>
        /// Set the InternalId for the Lifestyle. Used when editing an Advanced Lifestyle.
        /// </summary>
        /// <param name="strInternalId">InternalId to set.</param>
        public void SetInternalId(string strInternalId)
        {
            if (Guid.TryParse(strInternalId, out Guid guiTemp))
            {
                using (LockObject.EnterWriteLock())
                    _guiID = guiTemp;
            }
        }

        /// <summary>
        /// Purchases an additional month of the selected lifestyle.
        /// </summary>
        public async Task BuyExtraMonth(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                // Create the Expense Log Entry.
                decimal decAmount = await GetTotalMonthlyCostAsync(token: token).ConfigureAwait(false);
                if (decAmount > await _objCharacter.GetNuyenAsync(token).ConfigureAwait(false))
                {
                    await Program.ShowScrollableMessageBoxAsync(
                        await LanguageManager.GetStringAsync("Message_NotEnoughNuyen", token: token).ConfigureAwait(false),
                        await LanguageManager.GetStringAsync("MessageTitle_NotEnoughNuyen", token: token).ConfigureAwait(false),
                        MessageBoxButtons.OK, MessageBoxIcon.Information, token: token).ConfigureAwait(false);
                    return;
                }

                IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    ExpenseLogEntry objExpense = new ExpenseLogEntry(_objCharacter);
                    objExpense.Create(decAmount * -1,
                        await LanguageManager.GetStringAsync("String_ExpenseLifestyle", token: token).ConfigureAwait(false) + " " +
                        await GetCurrentDisplayNameShortAsync(token).ConfigureAwait(false),
                        ExpenseType.Nuyen, DateTime.Now);
                    await (await _objCharacter.GetExpenseEntriesAsync(token).ConfigureAwait(false))
                        .AddWithSortAsync(objExpense, token: token).ConfigureAwait(false);
                    await _objCharacter.ModifyNuyenAsync(-decAmount, token).ConfigureAwait(false);

                    ExpenseUndo objUndo = new ExpenseUndo();
                    objUndo.CreateNuyen(NuyenExpenseType.IncreaseLifestyle, InternalId);
                    objExpense.Undo = objUndo;

                    await ModifyIncrementsAsync(1, token).ConfigureAwait(false);
                }
                finally
                {
                    await objLocker2.DisposeAsync().ConfigureAwait(false);
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        private async Task LifestyleQualitiesOnBeforeClearCollectionChanged(object sender, NotifyCollectionChangedEventArgs e, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            foreach (LifestyleQuality objQuality in e.OldItems)
            {
                await objQuality.DisposeAsync().ConfigureAwait(false);
            }
        }

        private async Task LifestyleQualitiesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Replace:
                    HashSet<LifestyleQuality> setNewLifestyleQualities = e.NewItems.OfType<LifestyleQuality>().ToHashSet();
                    foreach (LifestyleQuality objQuality in e.OldItems)
                    {
                        if (!setNewLifestyleQualities.Contains(objQuality))
                            await objQuality.DisposeAsync().ConfigureAwait(false);
                    }
                    break;

                case NotifyCollectionChangedAction.Move:
                    return;
            }
            await OnPropertyChangedAsync(nameof(LifestyleQualities), token).ConfigureAwait(false);
        }

        #region UI Methods

        public async Task<TreeNode> CreateTreeNode(ContextMenuStrip cmsBasicLifestyle, ContextMenuStrip cmsAdvancedLifestyle, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                //if (!string.IsNullOrEmpty(ParentID) && !string.IsNullOrEmpty(Source) && !_objCharacter.Settings.BookEnabled(Source))
                //return null;
                TreeNode objNode = new TreeNode
                {
                    Name = InternalId,
                    Text = await GetCurrentDisplayNameAsync(token).ConfigureAwait(false),
                    Tag = this,
                    ContextMenuStrip = await GetStyleTypeAsync(token).ConfigureAwait(false) == LifestyleType.Standard
                        ? cmsBasicLifestyle
                        : cmsAdvancedLifestyle,
                    ForeColor = await GetPreferredColorAsync(token).ConfigureAwait(false),
                    ToolTipText = (await GetNotesAsync(token).ConfigureAwait(false)).WordWrap()
                };
                return objNode;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public Color PreferredColor =>
            !string.IsNullOrEmpty(Notes)
                ? ColorManager.GenerateCurrentModeColor(NotesColor)
                : ColorManager.WindowText;

        public async Task<Color> GetPreferredColorAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            return !string.IsNullOrEmpty(await GetNotesAsync(token).ConfigureAwait(false))
                ? ColorManager.GenerateCurrentModeColor(await GetNotesColorAsync(token).ConfigureAwait(false))
                : ColorManager.WindowText;
        }

        #endregion UI Methods

        #endregion Methods

        private static readonly PropertyDependencyGraph<Lifestyle> s_LifestyleDependencyGraph =
            new PropertyDependencyGraph<Lifestyle>(
                new DependencyGraphNode<string, Lifestyle>(nameof(AreaDelta),
                    new DependencyGraphNode<string, Lifestyle>(nameof(TotalAreaMaximum),
                        new DependencyGraphNode<string, Lifestyle>(nameof(LifestyleQualities)),
                        new DependencyGraphNode<string, Lifestyle>(nameof(AreaMaximum))
                    ),
                    new DependencyGraphNode<string, Lifestyle>(nameof(LifestyleQualities)),
                    new DependencyGraphNode<string, Lifestyle>(nameof(BaseArea))
                ),
                new DependencyGraphNode<string, Lifestyle>(nameof(TotalArea),
                    new DependencyGraphNode<string, Lifestyle>(nameof(BaseArea)),
                    new DependencyGraphNode<string, Lifestyle>(nameof(Area)),
                    new DependencyGraphNode<string, Lifestyle>(nameof(LifestyleQualities))
                ),
                new DependencyGraphNode<string, Lifestyle>(nameof(FormattedArea),
                    new DependencyGraphNode<string, Lifestyle>(nameof(BaseArea)),
                    new DependencyGraphNode<string, Lifestyle>(nameof(TotalAreaMaximum))
                ),
                new DependencyGraphNode<string, Lifestyle>(nameof(ComfortsDelta),
                    new DependencyGraphNode<string, Lifestyle>(nameof(TotalComfortsMaximum),
                        new DependencyGraphNode<string, Lifestyle>(nameof(LifestyleQualities)),
                        new DependencyGraphNode<string, Lifestyle>(nameof(ComfortsMaximum))
                    ),
                    new DependencyGraphNode<string, Lifestyle>(nameof(LifestyleQualities)),
                    new DependencyGraphNode<string, Lifestyle>(nameof(BaseComforts))
                ),
                new DependencyGraphNode<string, Lifestyle>(nameof(TotalComforts),
                    new DependencyGraphNode<string, Lifestyle>(nameof(BaseComforts)),
                    new DependencyGraphNode<string, Lifestyle>(nameof(Comforts)),
                    new DependencyGraphNode<string, Lifestyle>(nameof(LifestyleQualities))
                ),
                new DependencyGraphNode<string, Lifestyle>(nameof(FormattedComforts),
                    new DependencyGraphNode<string, Lifestyle>(nameof(BaseComforts)),
                    new DependencyGraphNode<string, Lifestyle>(nameof(TotalComfortsMaximum))
                ),
                new DependencyGraphNode<string, Lifestyle>(nameof(SecurityDelta),
                    new DependencyGraphNode<string, Lifestyle>(nameof(TotalSecurityMaximum),
                        new DependencyGraphNode<string, Lifestyle>(nameof(LifestyleQualities)),
                        new DependencyGraphNode<string, Lifestyle>(nameof(SecurityMaximum))
                    ),
                    new DependencyGraphNode<string, Lifestyle>(nameof(LifestyleQualities)),
                    new DependencyGraphNode<string, Lifestyle>(nameof(BaseSecurity))
                ),
                new DependencyGraphNode<string, Lifestyle>(nameof(TotalSecurity),
                    new DependencyGraphNode<string, Lifestyle>(nameof(BaseSecurity)),
                    new DependencyGraphNode<string, Lifestyle>(nameof(Security)),
                    new DependencyGraphNode<string, Lifestyle>(nameof(LifestyleQualities))
                ),
                new DependencyGraphNode<string, Lifestyle>(nameof(FormattedSecurity),
                    new DependencyGraphNode<string, Lifestyle>(nameof(BaseSecurity)),
                    new DependencyGraphNode<string, Lifestyle>(nameof(TotalAreaMaximum))
                ),
                new DependencyGraphNode<string, Lifestyle>(nameof(DisplayTotalMonthlyCost),
                    new DependencyGraphNode<string, Lifestyle>(nameof(GetTotalMonthlyCost),
                        new DependencyGraphNode<string, Lifestyle>(nameof(TrustFund),
                            new DependencyGraphNode<string, Lifestyle>(nameof(IsTrustFundEligible),
                                new DependencyGraphNode<string, Lifestyle>(nameof(BaseLifestyle))
                            )
                        ),
                        new DependencyGraphNode<string, Lifestyle>(nameof(CostPreSplit), x => !x.TrustFund, async (x, t) => !await x.GetTrustFundAsync(t).ConfigureAwait(false),
                            new DependencyGraphNode<string, Lifestyle>(nameof(TrustFund))
                        ),
                        new DependencyGraphNode<string, Lifestyle>(nameof(LifestyleQualities)),
                        new DependencyGraphNode<string, Lifestyle>(nameof(StyleType)),
                        new DependencyGraphNode<string, Lifestyle>(nameof(SplitCostWithRoommates), x => !x.TrustFund, async (x, t) => !await x.GetTrustFundAsync(t).ConfigureAwait(false),
                            new DependencyGraphNode<string, Lifestyle>(nameof(Roommates)),
                            new DependencyGraphNode<string, Lifestyle>(nameof(TrustFund))
                        ),
                        new DependencyGraphNode<string, Lifestyle>(nameof(Roommates), x => x.SplitCostWithRoommates, async (x, t) => await x.GetSplitCostWithRoommatesAsync(t).ConfigureAwait(false),
                            new DependencyGraphNode<string, Lifestyle>(nameof(SplitCostWithRoommates))
                        ),
                        new DependencyGraphNode<string, Lifestyle>(nameof(IncrementType)),
                        new DependencyGraphNode<string, Lifestyle>(nameof(Percentage))
                    )
                ),
                new DependencyGraphNode<string, Lifestyle>(nameof(CostPreSplit),
                    new DependencyGraphNode<string, Lifestyle>(nameof(Cost)),
                    new DependencyGraphNode<string, Lifestyle>(nameof(LifestyleQualities)),
                    new DependencyGraphNode<string, Lifestyle>(nameof(Roommates)),
                    new DependencyGraphNode<string, Lifestyle>(nameof(Area)),
                    new DependencyGraphNode<string, Lifestyle>(nameof(Comforts)),
                    new DependencyGraphNode<string, Lifestyle>(nameof(Security)),
                    new DependencyGraphNode<string, Lifestyle>(nameof(CostForArea)),
                    new DependencyGraphNode<string, Lifestyle>(nameof(CostForComforts)),
                    new DependencyGraphNode<string, Lifestyle>(nameof(CostForSecurity))
                ),
                new DependencyGraphNode<string, Lifestyle>(nameof(TotalCost),
                    new DependencyGraphNode<string, Lifestyle>(nameof(Increments)),
                    new DependencyGraphNode<string, Lifestyle>(nameof(GetTotalMonthlyCost))
                ),
                new DependencyGraphNode<string, Lifestyle>(nameof(TotalLP),
                    new DependencyGraphNode<string, Lifestyle>(nameof(LP)),
                    new DependencyGraphNode<string, Lifestyle>(nameof(Comforts)),
                    new DependencyGraphNode<string, Lifestyle>(nameof(Area)),
                    new DependencyGraphNode<string, Lifestyle>(nameof(Security)),
                    new DependencyGraphNode<string, Lifestyle>(nameof(Roommates)),
                    new DependencyGraphNode<string, Lifestyle>(nameof(BonusLP)),
                    new DependencyGraphNode<string, Lifestyle>(nameof(LifestyleQualities))
                ),
                new DependencyGraphNode<string, Lifestyle>(nameof(ExpectedValue),
                    new DependencyGraphNode<string, Lifestyle>(nameof(Dice)),
                    new DependencyGraphNode<string, Lifestyle>(nameof(Multiplier))
                ),
                new DependencyGraphNode<string, Lifestyle>(nameof(Purchased),
                    new DependencyGraphNode<string, Lifestyle>(nameof(Increments)),
                    new DependencyGraphNode<string, Lifestyle>(nameof(IncrementsRequiredForPermanent),
                        new DependencyGraphNode<string, Lifestyle>(nameof(IncrementType))
                    )
                ),
                new DependencyGraphNode<string, Lifestyle>(nameof(CurrentDisplayName),
                    new DependencyGraphNode<string, Lifestyle>(nameof(DisplayName),
                        new DependencyGraphNode<string, Lifestyle>(nameof(CustomName),
                            new DependencyGraphNode<string, Lifestyle>(nameof(Name))
                        ),
                        new DependencyGraphNode<string, Lifestyle>(nameof(DisplayNameShort),
                            new DependencyGraphNode<string, Lifestyle>(nameof(BaseLifestyle))
                        )
                    )
                ),
                new DependencyGraphNode<string, Lifestyle>(nameof(SourceIDString),
                    new DependencyGraphNode<string, Lifestyle>(nameof(SourceID))
                ),
                new DependencyGraphNode<string, Lifestyle>(nameof(SourceDetail),
                    new DependencyGraphNode<string, Lifestyle>(nameof(Source)),
                    new DependencyGraphNode<string, Lifestyle>(nameof(DisplayPage),
                        new DependencyGraphNode<string, Lifestyle>(nameof(Page))
                    )
                )
            );

        public event PropertyChangedEventHandler PropertyChanged;

        private readonly ConcurrentHashSet<PropertyChangedAsyncEventHandler> _setPropertyChangedAsync =
            new ConcurrentHashSet<PropertyChangedAsyncEventHandler>();

        public event PropertyChangedAsyncEventHandler PropertyChangedAsync
        {
            add => _setPropertyChangedAsync.TryAdd(value);
            remove => _setPropertyChangedAsync.Remove(value);
        }

        public event MultiplePropertiesChangedEventHandler MultiplePropertiesChanged;

        private readonly ConcurrentHashSet<MultiplePropertiesChangedAsyncEventHandler> _setMultiplePropertiesChangedAsync =
            new ConcurrentHashSet<MultiplePropertiesChangedAsyncEventHandler>();

        public event MultiplePropertiesChangedAsyncEventHandler MultiplePropertiesChangedAsync
        {
            add => _setMultiplePropertiesChangedAsync.TryAdd(value);
            remove => _setMultiplePropertiesChangedAsync.Remove(value);
        }

        [NotifyPropertyChangedInvocator]
        public void OnPropertyChanged([CallerMemberName] string strPropertyName = null)
        {
            this.OnMultiplePropertyChanged(strPropertyName);
        }

        public Task OnPropertyChangedAsync(string strPropertyName, CancellationToken token = default)
        {
            return this.OnMultiplePropertyChangedAsync(token, strPropertyName);
        }

        public void OnMultiplePropertiesChanged(IReadOnlyCollection<string> lstPropertyNames)
        {
            using (LockObject.EnterUpgradeableReadLock())
            {
                HashSet<string> setNamesOfChangedProperties = null;
                try
                {
                    foreach (string strPropertyName in lstPropertyNames)
                    {
                        if (setNamesOfChangedProperties == null)
                            setNamesOfChangedProperties
                                = s_LifestyleDependencyGraph.GetWithAllDependents(this, strPropertyName, true);
                        else
                        {
                            foreach (string strLoopChangedProperty in s_LifestyleDependencyGraph
                                         .GetWithAllDependentsEnumerable(this, strPropertyName))
                                setNamesOfChangedProperties.Add(strLoopChangedProperty);
                        }
                    }

                    if (setNamesOfChangedProperties == null || setNamesOfChangedProperties.Count == 0)
                        return;

                    if (setNamesOfChangedProperties.Contains(nameof(BaseLifestyle)))
                    {
                        foreach (LifestyleQuality objQuality in LifestyleQualities)
                        {
                            objQuality.OnPropertyChanged(nameof(LifestyleQuality.CanBeFreeByLifestyle));
                        }
                    }

                    if (_setMultiplePropertiesChangedAsync.Count > 0)
                    {
                        MultiplePropertiesChangedEventArgs objArgs =
                            new MultiplePropertiesChangedEventArgs(setNamesOfChangedProperties.ToArray());
                        List<Func<Task>> lstFuncs = new List<Func<Task>>(_setMultiplePropertiesChangedAsync.Count);
                        foreach (MultiplePropertiesChangedAsyncEventHandler objEvent in _setMultiplePropertiesChangedAsync)
                        {
                            lstFuncs.Add(() => objEvent.Invoke(this, objArgs));
                        }

                        Utils.RunWithoutThreadLock(lstFuncs);
                        if (MultiplePropertiesChanged != null)
                        {
                            Utils.RunOnMainThread(() =>
                            {
                                // ReSharper disable once AccessToModifiedClosure
                                MultiplePropertiesChanged?.Invoke(this, objArgs);
                            });
                        }
                    }
                    else if (MultiplePropertiesChanged != null)
                    {
                        MultiplePropertiesChangedEventArgs objArgs =
                            new MultiplePropertiesChangedEventArgs(setNamesOfChangedProperties.ToArray());
                        Utils.RunOnMainThread(() =>
                        {
                            // ReSharper disable once AccessToModifiedClosure
                            MultiplePropertiesChanged?.Invoke(this, objArgs);
                        });
                    }

                    if (_setPropertyChangedAsync.Count > 0)
                    {
                        List<PropertyChangedEventArgs> lstArgsList = setNamesOfChangedProperties.Select(x => new PropertyChangedEventArgs(x)).ToList();
                        List<Func<Task>> lstFuncs = new List<Func<Task>>(lstArgsList.Count * _setPropertyChangedAsync.Count);
                        foreach (PropertyChangedAsyncEventHandler objEvent in _setPropertyChangedAsync)
                        {
                            foreach (PropertyChangedEventArgs objArg in lstArgsList)
                                lstFuncs.Add(() => objEvent.Invoke(this, objArg));
                        }

                        Utils.RunWithoutThreadLock(lstFuncs);
                        if (PropertyChanged != null)
                        {
                            Utils.RunOnMainThread(() =>
                            {
                                if (PropertyChanged != null)
                                {
                                    // ReSharper disable once AccessToModifiedClosure
                                    foreach (PropertyChangedEventArgs objArgs in lstArgsList)
                                    {
                                        PropertyChanged.Invoke(this, objArgs);
                                    }
                                }
                            });
                        }
                    }
                    else if (PropertyChanged != null)
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

        public async Task OnMultiplePropertiesChangedAsync(IReadOnlyCollection<string> lstPropertyNames,
            CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                HashSet<string> setNamesOfChangedProperties = null;
                try
                {
                    foreach (string strPropertyName in lstPropertyNames)
                    {
                        if (setNamesOfChangedProperties == null)
                            setNamesOfChangedProperties
                                = await s_LifestyleDependencyGraph.GetWithAllDependentsAsync(this, strPropertyName, true, token).ConfigureAwait(false);
                        else
                        {
                            foreach (string strLoopChangedProperty in await s_LifestyleDependencyGraph
                                         .GetWithAllDependentsEnumerableAsync(this, strPropertyName, token).ConfigureAwait(false))
                                setNamesOfChangedProperties.Add(strLoopChangedProperty);
                        }
                    }

                    if (setNamesOfChangedProperties == null || setNamesOfChangedProperties.Count == 0)
                        return;

                    if (setNamesOfChangedProperties.Contains(nameof(BaseLifestyle)))
                    {
                        await LifestyleQualities.ForEachAsync(x =>
                                x.OnPropertyChanged(nameof(LifestyleQuality.CanBeFreeByLifestyle)), token)
                            .ConfigureAwait(false);
                    }

                    if (_setMultiplePropertiesChangedAsync.Count > 0)
                    {
                        MultiplePropertiesChangedEventArgs objArgs =
                            new MultiplePropertiesChangedEventArgs(setNamesOfChangedProperties.ToArray());
                        await ParallelExtensions.ForEachAsync(_setMultiplePropertiesChangedAsync, objEvent => objEvent.Invoke(this, objArgs, token), token).ConfigureAwait(false);
                        if (MultiplePropertiesChanged != null)
                        {
                            await Utils.RunOnMainThreadAsync(() =>
                            {
                                // ReSharper disable once AccessToModifiedClosure
                                MultiplePropertiesChanged?.Invoke(this, objArgs);
                            }, token: token).ConfigureAwait(false);
                        }
                    }
                    else if (MultiplePropertiesChanged != null)
                    {
                        MultiplePropertiesChangedEventArgs objArgs =
                            new MultiplePropertiesChangedEventArgs(setNamesOfChangedProperties.ToArray());
                        await Utils.RunOnMainThreadAsync(() =>
                        {
                            // ReSharper disable once AccessToModifiedClosure
                            MultiplePropertiesChanged?.Invoke(this, objArgs);
                        }, token: token).ConfigureAwait(false);
                    }

                    if (_setPropertyChangedAsync.Count > 0)
                    {
                        List<PropertyChangedEventArgs> lstArgsList = setNamesOfChangedProperties
                            .Select(x => new PropertyChangedEventArgs(x)).ToList();
                        List<ValueTuple<PropertyChangedAsyncEventHandler, PropertyChangedEventArgs>> lstAsyncEventsList
                            = new List<ValueTuple<PropertyChangedAsyncEventHandler, PropertyChangedEventArgs>>(lstArgsList.Count * _setPropertyChangedAsync.Count);
                        foreach (PropertyChangedAsyncEventHandler objEvent in _setPropertyChangedAsync)
                        {
                            foreach (PropertyChangedEventArgs objArg in lstArgsList)
                            {
                                lstAsyncEventsList.Add(new ValueTuple<PropertyChangedAsyncEventHandler, PropertyChangedEventArgs>(objEvent, objArg));
                            }
                        }
                        await ParallelExtensions.ForEachAsync(lstAsyncEventsList, tupEvent => tupEvent.Item1.Invoke(this, tupEvent.Item2, token), token).ConfigureAwait(false);

                        if (PropertyChanged != null)
                        {
                            await Utils.RunOnMainThreadAsync(() =>
                            {
                                if (PropertyChanged != null)
                                {
                                    // ReSharper disable once AccessToModifiedClosure
                                    foreach (PropertyChangedEventArgs objArgs in lstArgsList)
                                    {
                                        PropertyChanged.Invoke(this, objArgs);
                                    }
                                }
                            }, token).ConfigureAwait(false);
                        }
                    }
                    else if (PropertyChanged != null)
                    {
                        await Utils.RunOnMainThreadAsync(() =>
                        {
                            if (PropertyChanged != null)
                            {
                                // ReSharper disable once AccessToModifiedClosure
                                foreach (string strPropertyToChange in setNamesOfChangedProperties)
                                {
                                    PropertyChanged.Invoke(this, new PropertyChangedEventArgs(strPropertyToChange));
                                }
                            }
                        }, token).ConfigureAwait(false);
                    }
                }
                finally
                {
                    if (setNamesOfChangedProperties != null)
                        Utils.StringHashSetPool.Return(ref setNamesOfChangedProperties);
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public bool Remove(bool blnConfirmDelete = true)
        {
            using (LockObject.EnterUpgradeableReadLock())
            {
                if (blnConfirmDelete
                    && !CommonFunctions.ConfirmDelete(LanguageManager.GetString("Message_DeleteLifestyle")))
                    return false;
                using (_objCharacter.Lifestyles.LockObject.EnterUpgradeableReadLock())
                {
                    if (_objCharacter.Lifestyles.Contains(this) && !_objCharacter.Lifestyles.Remove(this))
                        return false;
                    int intQualitiesCount = LifestyleQualities.Count;
                    if (intQualitiesCount > 0)
                    {
                        List<string> lstIds = new List<string>(intQualitiesCount);
                        LifestyleQualities.ForEach(x => lstIds.Add(x.InternalId));
                        ImprovementManager.RemoveImprovements(CharacterObject, Improvement.ImprovementSource.Quality,
                            lstIds);
                    }
                }
            }

            Dispose();
            return true;
        }

        public async Task<bool> RemoveAsync(bool blnConfirmDelete = true, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (blnConfirmDelete && !await CommonFunctions.ConfirmDeleteAsync(
                        await LanguageManager.GetStringAsync("Message_DeleteLifestyle", token: token)
                            .ConfigureAwait(false), token).ConfigureAwait(false))
                    return false;
                IAsyncDisposable objLocker2 = await _objCharacter.Lifestyles.LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    if (await _objCharacter.Lifestyles.ContainsAsync(this, token).ConfigureAwait(false)
                        && !await _objCharacter.Lifestyles.RemoveAsync(this, token).ConfigureAwait(false))
                        return false;
                    int intQualitiesCount = await LifestyleQualities.GetCountAsync(token).ConfigureAwait(false);
                    if (intQualitiesCount > 0)
                    {
                        List<string> lstIds = new List<string>(intQualitiesCount);
                        await LifestyleQualities.ForEachAsync(x => lstIds.Add(x.InternalId), token).ConfigureAwait(false);
                        await ImprovementManager.RemoveImprovementsAsync(CharacterObject, Improvement.ImprovementSource.Quality, lstIds, token).ConfigureAwait(false);
                    }
                }
                finally
                {
                    await objLocker2.DisposeAsync().ConfigureAwait(false);
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }

            await DisposeAsync().ConfigureAwait(false);
            return true;
        }

        public void SetSourceDetail(Control sourceControl)
        {
            using (LockObject.EnterReadLock())
            {
                if (_objCachedSourceDetail.Language != GlobalSettings.Language)
                    _objCachedSourceDetail = default;
                SourceDetail.SetControl(sourceControl);
            }
        }

        public async Task SetSourceDetailAsync(Control sourceControl, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_objCachedSourceDetail.Language != GlobalSettings.Language)
                    _objCachedSourceDetail = default;
                await (await GetSourceDetailAsync(token).ConfigureAwait(false)).SetControlAsync(sourceControl, token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            using (LockObject.EnterWriteLock())
            {
                _lstLifestyleQualities.EnumerateWithSideEffects().ForEach(x => x.Dispose());
                _lstLifestyleQualities.CollectionChangedAsync -= LifestyleQualitiesCollectionChanged;
                _lstLifestyleQualities.BeforeClearCollectionChangedAsync -= LifestyleQualitiesOnBeforeClearCollectionChanged;
                _lstLifestyleQualities.Dispose();
                // to help the GC
                PropertyChanged = null;
                MultiplePropertiesChanged = null;
                _setPropertyChangedAsync.Clear();
                _setMultiplePropertiesChangedAsync.Clear();
            }
        }

        /// <inheritdoc />
        public async ValueTask DisposeAsync()
        {
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync().ConfigureAwait(false);
            try
            {
                await _lstLifestyleQualities.ForEachWithSideEffectsAsync(async x => await x.DisposeAsync().ConfigureAwait(false)).ConfigureAwait(false);
                _lstLifestyleQualities.CollectionChangedAsync -= LifestyleQualitiesCollectionChanged;
                _lstLifestyleQualities.BeforeClearCollectionChangedAsync -= LifestyleQualitiesOnBeforeClearCollectionChanged;
                await _lstLifestyleQualities.DisposeAsync().ConfigureAwait(false);
                // to help the GC
                PropertyChanged = null;
                MultiplePropertiesChanged = null;
                _setPropertyChangedAsync.Clear();
                _setMultiplePropertiesChangedAsync.Clear();
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <inheritdoc />
        public AsyncFriendlyReaderWriterLock LockObject { get; }

        public Character CharacterObject => _objCharacter; // readonly member, no locking required
    }
}
