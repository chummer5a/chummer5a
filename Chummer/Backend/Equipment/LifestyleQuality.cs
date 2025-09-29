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
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;
using Chummer.Annotations;
using NLog;

namespace Chummer.Backend.Equipment
{
    [DebuggerDisplay("{DisplayName(\"en-us\")}")]
    public sealed class LifestyleQuality : IHasInternalId, IHasName, IHasSourceId, IHasXmlDataNode, IHasNotes, IHasSource, ICanRemove, INotifyMultiplePropertiesChangedAsync, IHasLockObject, IHasCharacterObject
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
        private int _intLPCost;
        private string _strCost = string.Empty;
        private decimal _decMultiplier;
        private decimal _decBaseMultiplier;
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
            switch (strValue.ToUpperInvariant())
            {
                case "NEGATIVE":
                    return QualityType.Negative;

                case "POSITIVE":
                    return QualityType.Positive;

                case "CONTRACTS":
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
            switch (strValue.ToUpperInvariant())
            {
                case "BUILTIN":
                    return QualitySource.BuiltIn;

                case "HERITAGE":
                    return QualitySource.Heritage;

                case "IMPROVEMENT":
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
            _objCharacter = objCharacter ?? throw new ArgumentNullException(nameof(objCharacter));
            LockObject = objCharacter.LockObject;
        }

        /// <summary>
        ///     Create a LifestyleQuality from an XmlNode.
        /// </summary>
        /// <param name="objXmlLifestyleQuality">XmlNode to create the object from.</param>
        /// <param name="objParentLifestyle">Lifestyle object to which the LifestyleQuality will be added.</param>
        /// <param name="objCharacter">Character object the LifestyleQuality will be added to.</param>
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

                objXmlLifestyleQuality.TryGetInt32FieldQuickly("lp", ref _intLPCost);
                objXmlLifestyleQuality.TryGetStringFieldQuickly("cost", ref _strCost);
                objXmlLifestyleQuality.TryGetDecFieldQuickly("multiplier", ref _decMultiplier);
                objXmlLifestyleQuality.TryGetDecFieldQuickly("multiplierbaseonly", ref _decBaseMultiplier);
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

                if (GlobalSettings.InsertPdfNotesIfAvailable && string.IsNullOrEmpty(Notes))
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

                // If the item grants a bonus, pass the information to the Improvement Manager.
                XmlElement xmlBonus = objXmlLifestyleQuality["bonus"];
                if (xmlBonus != null)
                {
                    string strOldForcedValue = ImprovementManager.GetForcedValue(_objCharacter);
                    try
                    {
                        ImprovementManager.SetForcedValue(_strExtra, _objCharacter);
                        if (!ImprovementManager.CreateImprovements(objCharacter, Improvement.ImprovementSource.Quality,
                                InternalId, xmlBonus, 1, CurrentDisplayNameShort))
                        {
                            _guiID = Guid.Empty;
                            return;
                        }

                        string strSelectedValue = ImprovementManager.GetSelectedValue(_objCharacter);
                        if (!string.IsNullOrEmpty(strSelectedValue))
                            _strExtra = strSelectedValue;
                    }
                    finally
                    {
                        ImprovementManager.SetForcedValue(strOldForcedValue, _objCharacter);
                    }
                }

                // Built-In Qualities appear as grey text to show that they cannot be removed.
                if (objLifestyleQualitySource == QualitySource.BuiltIn)
                    _blnFree = true;
            }
        }

        /// <summary>
        ///     Create a LifestyleQuality from an XmlNode.
        /// </summary>
        /// <param name="objXmlLifestyleQuality">XmlNode to create the object from.</param>
        /// <param name="objParentLifestyle">Lifestyle object to which the LifestyleQuality will be added.</param>
        /// <param name="objCharacter">Character object the LifestyleQuality will be added to.</param>
        /// <param name="objLifestyleQualitySource">Source of the LifestyleQuality.</param>
        /// <param name="strExtra">Forced value for the LifestyleQuality's Extra string (also used by its bonus node).</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public async Task CreateAsync(XmlNode objXmlLifestyleQuality, Lifestyle objParentLifestyle, Character objCharacter,
            QualitySource objLifestyleQualitySource, string strExtra = "", CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                _objParentLifestyle = objParentLifestyle;
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

                objXmlLifestyleQuality.TryGetInt32FieldQuickly("lp", ref _intLPCost);
                objXmlLifestyleQuality.TryGetStringFieldQuickly("cost", ref _strCost);
                objXmlLifestyleQuality.TryGetDecFieldQuickly("multiplier", ref _decMultiplier);
                objXmlLifestyleQuality.TryGetDecFieldQuickly("multiplierbaseonly", ref _decBaseMultiplier);
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

                if (GlobalSettings.InsertPdfNotesIfAvailable && string.IsNullOrEmpty(await GetNotesAsync(token).ConfigureAwait(false)))
                {
                    await SetNotesAsync(await CommonFunctions.GetBookNotesAsync(objXmlLifestyleQuality, Name, await GetCurrentDisplayNameAsync(token).ConfigureAwait(false), Source, Page,
                        await DisplayPageAsync(GlobalSettings.Language, token).ConfigureAwait(false), _objCharacter, token).ConfigureAwait(false), token).ConfigureAwait(false);
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

                // If the item grants a bonus, pass the information to the Improvement Manager.
                XmlElement xmlBonus = objXmlLifestyleQuality["bonus"];
                if (xmlBonus != null)
                {
                    string strOldForcedValue = ImprovementManager.GetForcedValue(_objCharacter);
                    try
                    {
                        ImprovementManager.SetForcedValue(_strExtra, _objCharacter);
                        if (!await ImprovementManager.CreateImprovementsAsync(objCharacter,
                                    Improvement.ImprovementSource.Quality,
                                    InternalId, xmlBonus, 1,
                                    await GetCurrentDisplayNameShortAsync(token).ConfigureAwait(false), token: token)
                                .ConfigureAwait(false))
                        {
                            _guiID = Guid.Empty;
                            return;
                        }

                        string strSelectedValue = ImprovementManager.GetSelectedValue(_objCharacter);
                        if (!string.IsNullOrEmpty(strSelectedValue))
                            _strExtra = strSelectedValue;
                    }
                    finally
                    {
                        ImprovementManager.SetForcedValue(strOldForcedValue, _objCharacter);
                    }
                }

                // Built-In Qualities appear as grey text to show that they cannot be removed.
                if (objLifestyleQualitySource == QualitySource.BuiltIn)
                    _blnFree = true;
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
        ///     Save the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        public void Save(XmlWriter objWriter)
        {
            if (objWriter == null)
                return;
            using (LockObject.EnterReadLock())
            {
                objWriter.WriteStartElement("lifestylequality");
                objWriter.WriteElementString("sourceid", SourceIDString);
                objWriter.WriteElementString("guid", InternalId);
                objWriter.WriteElementString("name", _strName);
                objWriter.WriteElementString("category", _strCategory);
                objWriter.WriteElementString("extra", _strExtra);
                objWriter.WriteElementString("cost", _strCost);
                objWriter.WriteElementString("multiplier",
                                             _decMultiplier.ToString(GlobalSettings.InvariantCultureInfo));
                objWriter.WriteElementString("basemultiplier",
                                             _decBaseMultiplier.ToString(GlobalSettings.InvariantCultureInfo));
                objWriter.WriteElementString("lp", _intLPCost.ToString(GlobalSettings.InvariantCultureInfo));
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
                                                 ? StringExtensions.JoinFast(",", _setAllowedFreeLifestyles)
                                                 : string.Empty);
                if (!Bonus.IsNullOrInnerTextIsEmpty())
                    objWriter.WriteRaw("<bonus>" + Bonus.InnerXmlViaPool() + "</bonus>");
                else
                    objWriter.WriteElementString("bonus", string.Empty);
                objWriter.WriteElementString("notes", _strNotes.CleanOfXmlInvalidUnicodeChars());
                objWriter.WriteElementString("notesColor", ColorTranslator.ToHtml(_colNotes));
                objWriter.WriteEndElement();
            }
        }

        /// <summary>
        /// Load the Lifestyle quality from the XmlNode.
        /// </summary>
        /// <param name="objNode">XmlNode to load.</param>
        /// <param name="objParentLifestyle">Lifestyle object to which this LifestyleQuality belongs.</param>
        public void Load(XmlNode objNode, Lifestyle objParentLifestyle)
        {
            Utils.SafelyRunSynchronously(() => LoadCoreAsync(true, objNode, objParentLifestyle));
        }

        /// <summary>
        /// Load the Lifestyle quality from the XmlNode.
        /// </summary>
        /// <param name="objNode">XmlNode to load.</param>
        /// <param name="objParentLifestyle">Lifestyle object to which this LifestyleQuality belongs.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public Task LoadAsync(XmlNode objNode, Lifestyle objParentLifestyle, CancellationToken token = default)
        {
            return LoadCoreAsync(false, objNode, objParentLifestyle, token);
        }

        /// <summary>
        /// Load the Lifestyle quality from the XmlNode.
        /// </summary>
        private async Task LoadCoreAsync(bool blnSync, XmlNode objNode, Lifestyle objParentLifestyle, CancellationToken token = default)
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
                _objParentLifestyle = objParentLifestyle;
                if (!objNode.TryGetField("guid", Guid.TryParse, out _guiID))
                    _guiID = Guid.NewGuid();
                objNode.TryGetStringFieldQuickly("name", ref _strName);
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

                objNode.TryGetStringFieldQuickly("extra", ref _strExtra);
                objNode.TryGetInt32FieldQuickly("lp", ref _intLPCost);
                objNode.TryGetStringFieldQuickly("cost", ref _strCost);
                objNode.TryGetDecFieldQuickly("multiplier", ref _decMultiplier);
                objNode.TryGetDecFieldQuickly("basemultiplier", ref _decBaseMultiplier);
                if (!objNode.TryGetBoolFieldQuickly("uselpcost", ref _blnUseLPCost))
                    objNode.TryGetBoolFieldQuickly("contributetolimit", ref _blnUseLPCost);
                if (!objNode.TryGetInt32FieldQuickly("areamaximum", ref _intAreaMaximum))
                    (blnSync ? objMyNode.Value : await objMyNodeAsync.GetValueAsync(token).ConfigureAwait(false))?.TryGetInt32FieldQuickly("areamaximum", ref _intAreaMaximum);
                if (!objNode.TryGetInt32FieldQuickly("area", ref _intArea))
                    (blnSync ? objMyNode.Value : await objMyNodeAsync.GetValueAsync(token).ConfigureAwait(false))?.TryGetInt32FieldQuickly("area", ref _intArea);
                if (!objNode.TryGetInt32FieldQuickly("securitymaximum", ref _intSecurityMaximum))
                    (blnSync ? objMyNode.Value : await objMyNodeAsync.GetValueAsync(token).ConfigureAwait(false))?.TryGetInt32FieldQuickly("securitymaximum", ref _intSecurityMaximum);
                if (!objNode.TryGetInt32FieldQuickly("security", ref _intSecurity))
                    (blnSync ? objMyNode.Value : await objMyNodeAsync.GetValueAsync(token).ConfigureAwait(false))?.TryGetInt32FieldQuickly("security", ref _intSecurity);
                if (!objNode.TryGetInt32FieldQuickly("comforts", ref _intComforts))
                    (blnSync ? objMyNode.Value : await objMyNodeAsync.GetValueAsync(token).ConfigureAwait(false))?.TryGetInt32FieldQuickly("comforts", ref _intComforts);
                if (!objNode.TryGetInt32FieldQuickly("comfortsmaximum", ref _intComfortsMaximum))
                    (blnSync ? objMyNode.Value : await objMyNodeAsync.GetValueAsync(token).ConfigureAwait(false))?.TryGetInt32FieldQuickly("comfortsmaximum", ref _intComfortsMaximum);
                objNode.TryGetBoolFieldQuickly("print", ref _blnPrint);
                if (objNode["lifestylequalitytype"] != null)
                    _eType = ConvertToLifestyleQualityType(objNode["lifestylequalitytype"].InnerTextViaPool(token));
                if (objNode["lifestylequalitysource"] != null)
                    OriginSource = ConvertToLifestyleQualitySource(objNode["lifestylequalitysource"].InnerTextViaPool(token));
                if (!objNode.TryGetStringFieldQuickly("category", ref _strCategory)
                    && (blnSync ? objMyNode.Value : await objMyNodeAsync.GetValueAsync(token).ConfigureAwait(false))?.TryGetStringFieldQuickly("category", ref _strCategory) != true)
                    _strCategory = string.Empty;
                objNode.TryGetBoolFieldQuickly("free", ref _blnFree);
                objNode.TryGetBoolFieldQuickly("isfreegrid", ref _blnIsFreeGrid);
                objNode.TryGetStringFieldQuickly("source", ref _strSource);
                objNode.TryGetStringFieldQuickly("page", ref _strPage);
                string strAllowedFreeLifestyles = string.Empty;
                if (!objNode.TryGetStringFieldQuickly("allowed", ref strAllowedFreeLifestyles)
                    && (blnSync ? objMyNode.Value : await objMyNodeAsync.GetValueAsync(token).ConfigureAwait(false))?.TryGetStringFieldQuickly("allowed", ref strAllowedFreeLifestyles) != true)
                    strAllowedFreeLifestyles = string.Empty;
                _setAllowedFreeLifestyles.Clear();
                foreach (string strLoopLifestyle in strAllowedFreeLifestyles.SplitNoAlloc(
                             ',', StringSplitOptions.RemoveEmptyEntries))
                    _setAllowedFreeLifestyles.Add(strLoopLifestyle);
                if (blnSync)
                    Bonus = objNode["bonus"];
                else
                    await SetBonusAsync(objNode["bonus"], token).ConfigureAwait(false);
                objNode.TryGetMultiLineStringFieldQuickly("notes", ref _strNotes);

                string sNotesColor = ColorTranslator.ToHtml(ColorManager.HasNotesColor);
                objNode.TryGetStringFieldQuickly("notesColor", ref sNotesColor);
                _colNotes = ColorTranslator.FromHtml(sNotesColor);

                if (blnSync)
                    // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                    LegacyShim();
                else
                    await LegacyShimAsync(token).ConfigureAwait(false);
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
        ///     Performs actions based on the character's last loaded AppVersion attribute.
        /// </summary>
        private void LegacyShim()
        {
            if (Utils.IsUnitTest)
                return;
            //Unstored Cost and LP values prior to 5.190.2 nightlies.
            if (_objCharacter.LastSavedVersion > new ValueVersion(5, 190, 0))
                return;
            XPathNavigator objXmlDocument = _objCharacter.LoadDataXPath("lifestyles.xml");
            using (LockObject.EnterWriteLock())
            {
                XPathNavigator objLifestyleQualityNode = this.GetNodeXPath()
                                                         ?? objXmlDocument.SelectSingleNode(
                                                             "/chummer/qualities/quality[name = " + Name.CleanXPath()
                                                             + "]");
                if (objLifestyleQualityNode == null)
                {
                    using (new FetchSafelyFromSafeObjectPool<List<ListItem>>(Utils.ListItemListPool,
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
                                objXmlDocument.TryGetNodeByNameOrId("/chummer/qualities/quality",
                                    frmSelect.MyForm.SelectedItem);
                        }
                    }
                }

                int intTemp = 0;
                string strTemp = string.Empty;
                if (objLifestyleQualityNode.TryGetStringFieldQuickly("cost", ref strTemp))
                    CostString = strTemp;
                if (objLifestyleQualityNode.TryGetInt32FieldQuickly("lp", ref intTemp))
                    LPCost = intTemp;
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
                decimal decTemp = 0.0m;
                if (objLifestyleQualityNode.TryGetDecFieldQuickly("multiplier", ref decTemp))
                    Multiplier = decTemp;
                if (objLifestyleQualityNode.TryGetDecFieldQuickly("multiplierbaseonly", ref decTemp))
                    BaseMultiplier = decTemp;
            }
        }

        /// <summary>
        ///     Performs actions based on the character's last loaded AppVersion attribute.
        /// </summary>
        private async Task LegacyShimAsync(CancellationToken token = default)
        {
            if (Utils.IsUnitTest)
                return;
            //Unstored Cost and LP values prior to 5.190.2 nightlies.
            if (_objCharacter.LastSavedVersion > new ValueVersion(5, 190, 0))
                return;
            XPathNavigator objXmlDocument = await _objCharacter.LoadDataXPathAsync("lifestyles.xml", token: token).ConfigureAwait(false);
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                XPathNavigator objLifestyleQualityNode = await this.GetNodeXPathAsync(token).ConfigureAwait(false)
                                                         ?? objXmlDocument.SelectSingleNode(
                                                             "/chummer/qualities/quality[name = " + (await GetNameAsync(token).ConfigureAwait(false)).CleanXPath()
                                                             + "]");
                if (objLifestyleQualityNode == null)
                {
                    using (new FetchSafelyFromSafeObjectPool<List<ListItem>>(Utils.ListItemListPool,
                                                                   out List<ListItem> lstQualities))
                    {
                        foreach (XPathNavigator xmlNode in objXmlDocument.SelectAndCacheExpression(
                                     "/chummer/qualities/quality", token))
                        {
                            lstQualities.Add(new ListItem(xmlNode.SelectSingleNodeAndCacheExpression("id", token)?.Value,
                                                          xmlNode.SelectSingleNodeAndCacheExpression("translate", token)?.Value
                                                          ?? xmlNode.SelectSingleNodeAndCacheExpression("name", token)
                                                                    ?.Value));
                        }

                        string strDescription = string.Format(GlobalSettings.CultureInfo,
                                                                   await LanguageManager.GetStringAsync(
                                                                       "String_intCannotFindLifestyleQuality", token: token).ConfigureAwait(false),
                                                                   _strName);
                        using (ThreadSafeForm<SelectItem> frmSelect = await ThreadSafeForm<SelectItem>.GetAsync(
                                   () => new SelectItem
                                   {
                                       Description = strDescription
                                   }, token).ConfigureAwait(false))
                        {
                            frmSelect.MyForm.SetGeneralItemsMode(lstQualities);
                            if (await frmSelect.ShowDialogSafeAsync(_objCharacter, token).ConfigureAwait(false) == DialogResult.Cancel)
                            {
                                _guiID = Guid.Empty;
                                return;
                            }

                            objLifestyleQualityNode =
                                objXmlDocument.TryGetNodeByNameOrId("/chummer/qualities/quality",
                                    frmSelect.MyForm.SelectedItem);
                        }
                    }
                }

                int intTemp = 0;
                string strTemp = string.Empty;
                if (objLifestyleQualityNode.TryGetStringFieldQuickly("cost", ref strTemp))
                    CostString = strTemp;
                if (objLifestyleQualityNode.TryGetInt32FieldQuickly("lp", ref intTemp))
                    LPCost = intTemp;
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
                decimal decTemp = 0.0m;
                if (objLifestyleQualityNode.TryGetDecFieldQuickly("multiplier", ref decTemp))
                    await SetMultiplierAsync(decTemp, token).ConfigureAwait(false);
                if (objLifestyleQualityNode.TryGetDecFieldQuickly("multiplierbaseonly", ref decTemp))
                    await SetBaseMultiplierAsync(decTemp, token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        ///     Print the object's XML to the XmlWriter.
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
                            "name_english", Name,
                            token).ConfigureAwait(false);
                    await objWriter
                        .WriteElementStringAsync(
                            "fullname", await DisplayNameAsync(strLanguageToPrint, token).ConfigureAwait(false),
                            token).ConfigureAwait(false);
                    await objWriter
                        .WriteElementStringAsync(
                            "fullname_english", await DisplayNameAsync(GlobalSettings.DefaultLanguage, token).ConfigureAwait(false),
                            token).ConfigureAwait(false);
                    await objWriter
                        .WriteElementStringAsync("formattedname",
                            await FormattedDisplayNameAsync(
                                objCulture, strLanguageToPrint, token).ConfigureAwait(false),
                            token).ConfigureAwait(false);
                    await objWriter
                        .WriteElementStringAsync("formattedname_english",
                            await FormattedDisplayNameAsync(
                                GlobalSettings.InvariantCultureInfo, GlobalSettings.DefaultLanguage, token).ConfigureAwait(false),
                            token).ConfigureAwait(false);
                    await objWriter
                        .WriteElementStringAsync(
                            "extra",
                            await _objCharacter.TranslateExtraAsync(Extra, strLanguageToPrint, token: token)
                                .ConfigureAwait(false), token).ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("lp", (await GetLPCostAsync(token).ConfigureAwait(false)).ToString(objCulture), token).ConfigureAwait(false);
                    await objWriter
                        .WriteElementStringAsync(
                            "cost", (await GetCostAsync(token).ConfigureAwait(false)).ToString(await (await _objCharacter.GetSettingsAsync(token).ConfigureAwait(false)).GetNuyenFormatAsync(token).ConfigureAwait(false), objCulture), token)
                        .ConfigureAwait(false);
                    string strLifestyleQualityType = Type.ToString();
                    if (!strLanguageToPrint.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                    {
                        XPathNavigator objNode
                            = (await _objCharacter
                                    .LoadDataXPathAsync("lifestyles.xml", strLanguageToPrint, token: token)
                                    .ConfigureAwait(false))
                                .SelectSingleNodeAndCacheExpression("/chummer/categories/category[. = " +
                                                                    strLifestyleQualityType.CleanXPath()
                                                                    + "]", token: token);
                        if (objNode != null)
                            strLifestyleQualityType
                                = objNode.SelectSingleNodeAndCacheExpression("@translate", token)?.Value ?? strLifestyleQualityType;
                    }

                    await objWriter.WriteElementStringAsync("lifestylequalitytype", strLifestyleQualityType, token)
                        .ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("lifestylequalitytype_english", Type.ToString(), token)
                        .ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("lifestylequalitysource", OriginSource.ToString(), token)
                        .ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("free", (await GetFreeAsync(token).ConfigureAwait(false)).ToString(), token).ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("freebylifestyle", CanBeFreeByLifestyle.ToString(), token)
                        .ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("isfreegrid", (await GetIsFreeGridAsync(token).ConfigureAwait(false)).ToString(), token)
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
                        await objWriter.WriteElementStringAsync("notes", await GetNotesAsync(token).ConfigureAwait(false), token).ConfigureAwait(false);
                }
                finally
                {
                    // </quality>
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
        ///     Internal identifier which will be used to identify this LifestyleQuality in the Improvement system.
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
        ///     Identifier of the object within data files.
        /// </summary>
        public Guid SourceID
        {
            get
            {
                using (LockObject.EnterReadLock())
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
                using (LockObject.EnterReadLock())
                    return _strName;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
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
        /// Tradition name.
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
        /// Tradition name.
        /// </summary>
        public async Task SetNameAsync(string value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (Interlocked.Exchange(ref _strName, value) == value)
                    return;

                IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    _objCachedMyXmlNode = null;
                    _objCachedMyXPathNode = null;
                }
                finally
                {
                    await objLocker2.DisposeAsync().ConfigureAwait(false);
                }
                await OnPropertyChangedAsync(nameof(Name), token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        ///     LifestyleQuality's parent lifestyle.
        /// </summary>
        public Lifestyle ParentLifestyle
        {
            get
            {
                using (LockObject.EnterReadLock())
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
                using (LockObject.EnterReadLock())
                    return _strExtra;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
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
        ///     Bonus node from the XML file.
        /// </summary>
        public XmlNode Bonus
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _xmlBonus;
            }
            private set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (Interlocked.Exchange(ref _xmlBonus, value) != value)
                        OnPropertyChanged();
                }
            }
        }

        private async Task SetBonusAsync(XmlNode value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (Interlocked.Exchange(ref _xmlBonus, value) != value)
                    await OnPropertyChangedAsync(nameof(Bonus), token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        ///     LifestyleQuality Type.
        /// </summary>
        public QualityType Type
        {
            get
            {
                using (LockObject.EnterReadLock())
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
                using (LockObject.EnterReadLock())
                    return _eOriginSource;
            }
            set
            {
                using (LockObject.EnterReadLock())
                {
                    if (_eOriginSource == value)
                        return;
                }

                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (InterlockedExtensions.Exchange(ref _eOriginSource, value) != value)
                        OnPropertyChanged();
                }
            }
        }

        public async Task<QualitySource> GetOriginSourceAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _eOriginSource;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public async Task SetOriginSourceAsync(QualitySource value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_eOriginSource == value)
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
                if (InterlockedExtensions.Exchange(ref _eOriginSource, value) != value)
                    await OnPropertyChangedAsync(nameof(OriginSource), token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Number of Build Points the LifestyleQuality costs.
        /// </summary>
        public int LPCost
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return LPFree ? 0 : _intLPCost;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (Interlocked.Exchange(ref _intLPCost, value) != value)
                        OnPropertyChanged();
                }
            }
        }

        public async Task<int> GetLPCostAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return await GetLPFreeAsync(token).ConfigureAwait(false) ? 0 : _intLPCost;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public bool LPFree
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return Free || (!UseLPCost && CanBeFreeByLifestyle);
            }
        }

        public async Task<bool> GetLPFreeAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return await GetFreeAsync(token).ConfigureAwait(false)
                    || (!await GetUseLPCostAsync(token).ConfigureAwait(false)
                        && await GetCanBeFreeByLifestyleAsync(token).ConfigureAwait(false));
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// The name of the object as it should be displayed on printouts (translated name only).
        /// </summary>
        public string DisplayNameShort(string strLanguage)
        {
            if (strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                return Name;

            using (LockObject.EnterReadLock())
                return this.GetNodeXPath(strLanguage)?.SelectSingleNodeAndCacheExpression("translate")?.Value ?? Name;
        }

        /// <summary>
        /// The name of the object as it should be displayed on printouts (translated name only).
        /// </summary>
        public async Task<string> DisplayNameShortAsync(string strLanguage, CancellationToken token = default)
        {
            if (strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                return Name;

            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                XPathNavigator objNode = await this.GetNodeXPathAsync(strLanguage, token: token).ConfigureAwait(false);
                return objNode != null
                    ? objNode.SelectSingleNodeAndCacheExpression("translate", token: token)?.Value ?? Name
                    : Name;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public string CurrentDisplayNameShort => DisplayNameShort(GlobalSettings.Language);

        public Task<string> GetCurrentDisplayNameShortAsync(CancellationToken token = default) =>
            DisplayNameShortAsync(GlobalSettings.Language, token);

        /// <summary>
        ///     The name of the object as it should be displayed in lists. Name (Extra).
        /// </summary>
        public string DisplayName(string strLanguage)
        {
            using (LockObject.EnterReadLock())
            {
                string strReturn = DisplayNameShort(strLanguage);

                if (!string.IsNullOrEmpty(Extra))
                    // Attempt to retrieve the CharacterAttribute name.
                    strReturn += LanguageManager.GetString("String_Space", strLanguage) + "(" +
                                 _objCharacter.TranslateExtra(Extra, strLanguage) + ")";
                return strReturn;
            }
        }

        /// <summary>
        ///     The name of the object as it should be displayed in lists. Name (Extra).
        /// </summary>
        public async Task<string> DisplayNameAsync(string strLanguage, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                string strReturn = await DisplayNameShortAsync(strLanguage, token).ConfigureAwait(false);

                if (!string.IsNullOrEmpty(Extra))
                    // Attempt to retrieve the CharacterAttribute name.
                    strReturn += await LanguageManager.GetStringAsync("String_Space", strLanguage, token: token)
                                                      .ConfigureAwait(false) + "(" +
                                 await _objCharacter.TranslateExtraAsync(Extra, strLanguage, token: token)
                                                    .ConfigureAwait(false) + ")";
                return strReturn;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public string CurrentDisplayName => DisplayName(GlobalSettings.Language);

        public Task<string> GetCurrentDisplayNameAsync(CancellationToken token = default) => DisplayNameAsync(GlobalSettings.Language, token);

        public string FormattedDisplayName(CultureInfo objCulture, string strLanguage)
        {
            using (LockObject.EnterReadLock())
            {
                return DisplayName(strLanguage)
                    + LanguageManager.GetString("String_Space", strLanguage)
                    + "[" + DisplayCost(objCulture, strLanguage) + "]";
            }
        }

        public async Task<string> FormattedDisplayNameAsync(CultureInfo objCulture, string strLanguage, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return await DisplayNameAsync(strLanguage, token).ConfigureAwait(false)
                       + await LanguageManager.GetStringAsync("String_Space", strLanguage, token: token).ConfigureAwait(false)
                       + "[" + await DisplayCostAsync(objCulture, strLanguage, token).ConfigureAwait(false) + "]";
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public string CurrentFormattedDisplayName => FormattedDisplayName(GlobalSettings.CultureInfo, GlobalSettings.Language);

        public Task<string> GetCurrentFormattedDisplayNameAsync(CancellationToken token = default) =>
            FormattedDisplayNameAsync(GlobalSettings.CultureInfo, GlobalSettings.Language, token);

        /// <summary>
        ///     Whether the LifestyleQuality appears on the printouts.
        /// </summary>
        public bool AllowPrint
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _blnPrint;
            }
            set
            {
                using (LockObject.EnterReadLock())
                {
                    if (_blnPrint == value)
                        return;
                }

                using (LockObject.EnterUpgradeableReadLock())
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
        ///     Nuyen cost of the Quality.
        /// </summary>
        public decimal Cost
        {
            get
            {
                using (LockObject.EnterReadLock())
                {
                    if (Free)
                        return 0;
                    string strCost = CostString;
                    if (strCost.DoesNeedXPathProcessingToBeConvertedToNumber(out decimal decReturn))
                    {
                        strCost = _objCharacter.ProcessAttributesInXPath(strCost);
                        (bool blnIsSuccess, object objProcess) = CommonFunctions.EvaluateInvariantXPath(strCost);
                        if (blnIsSuccess)
                            return Convert.ToDecimal((double)objProcess);
                    }

                    return decReturn;
                }
            }
        }

        public async Task<decimal> GetCostAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (await GetCostFreeAsync(token).ConfigureAwait(false))
                    return 0;
                string strCost = CostString;
                if (strCost.DoesNeedXPathProcessingToBeConvertedToNumber(out decimal decReturn))
                {
                    strCost = await _objCharacter.ProcessAttributesInXPathAsync(strCost, token: token).ConfigureAwait(false);
                    (bool blnIsSuccess, object objProcess) = await CommonFunctions.EvaluateInvariantXPathAsync(strCost, token).ConfigureAwait(false);
                    if (blnIsSuccess)
                        return Convert.ToDecimal((double)objProcess);
                }

                return decReturn;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public string CurrentDisplayCost => DisplayCost(GlobalSettings.CultureInfo, GlobalSettings.Language);

        public Task<string> GetCurrentDisplayCostAsync(CancellationToken token = default) => DisplayCostAsync(GlobalSettings.CultureInfo, GlobalSettings.Language, token);

        public string DisplayCost(CultureInfo objCulture, string strLanguage)
        {
            using (LockObject.EnterReadLock())
            {
                if (CostFree)
                    return LanguageManager.GetString("Checkbox_Free", strLanguage);
                if (objCulture == null)
                    objCulture = GlobalSettings.CultureInfo;
                string strReturn;
                using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdReturn))
                {
                    decimal decBaseMultiplier = BaseMultiplier;
                    if (decBaseMultiplier != 0)
                    {
                        if (decBaseMultiplier > 0)
                            sbdReturn.Append('+');
                        sbdReturn.Append((_objParentLifestyle.Cost * (1.0m + decBaseMultiplier)).ToString(objCulture));
                    }

                    decimal decMultiplier = Multiplier;
                    if (decMultiplier != 0)
                    {
                        if (sbdReturn.Length > 0)
                            sbdReturn.Append(',').Append(LanguageManager.GetString("String_Space", strLanguage));
                        if (decMultiplier > 0)
                            sbdReturn.Append('+');
                        sbdReturn.Append(decMultiplier.ToString(objCulture)).Append('%');
                    }

                    decimal decCost = Cost;
                    if (decCost != 0)
                    {
                        if (sbdReturn.Length > 0)
                            sbdReturn.Append(',').Append(LanguageManager.GetString("String_Space", strLanguage));
                        if (decCost > 0)
                            sbdReturn.Append('+');
                        sbdReturn.Append(decCost.ToString(_objCharacter.Settings.NuyenFormat, objCulture))
                            .Append(LanguageManager.GetString("String_NuyenSymbol", strLanguage));
                    }
                    strReturn = sbdReturn.ToString();
                }

                return string.IsNullOrEmpty(strReturn) ? LanguageManager.GetString("String_None") : strReturn;
            }
        }

        public async Task<string> DisplayCostAsync(CultureInfo objCulture, string strLanguage, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (await GetCostFreeAsync(token).ConfigureAwait(false))
                    return await LanguageManager.GetStringAsync("Checkbox_Free", strLanguage, token: token).ConfigureAwait(false);
                if (objCulture == null)
                    objCulture = GlobalSettings.CultureInfo;
                string strReturn;
                using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdReturn))
                {
                    decimal decBaseMultiplier = await GetBaseMultiplierAsync(token).ConfigureAwait(false);
                    if (decBaseMultiplier != 0)
                    {
                        if (decBaseMultiplier > 0)
                            sbdReturn.Append('+');
                        sbdReturn.Append((await _objParentLifestyle.GetCostAsync(token).ConfigureAwait(false) * (1.0m + decBaseMultiplier)).ToString(objCulture));
                    }

                    decimal decMultiplier = await GetMultiplierAsync(token).ConfigureAwait(false);
                    if (decMultiplier != 0)
                    {
                        if (sbdReturn.Length > 0)
                            sbdReturn.Append(',').Append(await LanguageManager.GetStringAsync("String_Space", strLanguage, token: token).ConfigureAwait(false));
                        if (decMultiplier > 0)
                            sbdReturn.Append('+');
                        sbdReturn.Append(decMultiplier.ToString(objCulture)).Append('%');
                    }

                    decimal decCost = await GetCostAsync(token).ConfigureAwait(false);
                    if (decCost != 0)
                    {
                        if (sbdReturn.Length > 0)
                            sbdReturn.Append(',').Append(await LanguageManager.GetStringAsync("String_Space", strLanguage, token: token).ConfigureAwait(false));
                        if (decCost > 0)
                            sbdReturn.Append('+');
                        sbdReturn.Append(decCost.ToString(await (await _objCharacter.GetSettingsAsync(token).ConfigureAwait(false)).GetNuyenFormatAsync(token: token).ConfigureAwait(false), objCulture))
                            .Append(await LanguageManager.GetStringAsync("String_NuyenSymbol", strLanguage, token: token).ConfigureAwait(false));
                    }
                    strReturn = sbdReturn.ToString();
                }

                return string.IsNullOrEmpty(strReturn)
                    ? await LanguageManager.GetStringAsync("String_None", strLanguage, token: token).ConfigureAwait(false)
                    : strReturn;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        ///     String for the nuyen cost of the Quality.
        /// </summary>
        public string CostString
        {
            get
            {
                using (LockObject.EnterReadLock())
                {
                    return string.IsNullOrWhiteSpace(_strCost) ? "0" : _strCost;
                }
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (Interlocked.Exchange(ref _strCost, value) != value)
                        OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Does the Quality have a Nuyen cost?
        /// </summary>
        public bool CostFree
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return Free || IsFreeByLifestyle;
            }
        }

        /// <summary>
        /// Does the Quality have a Nuyen cost?
        /// </summary>
        public async Task<bool> GetCostFreeAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return await GetFreeAsync(token).ConfigureAwait(false) || await GetIsFreeByLifestyleAsync(token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Does the Quality have any cost?
        /// </summary>
        public bool Free
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _blnFree;
            }
            set
            {
                using (LockObject.EnterReadLock())
                {
                    if (_blnFree == value)
                        return;
                }

                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (_blnFree == value)
                        return;
                    using (LockObject.EnterWriteLock())
                        _blnFree = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Does the Quality have any cost?
        /// </summary>
        public async Task<bool> GetFreeAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _blnFree;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Does the Quality have any cost?
        /// </summary>
        public async Task SetFreeAsync(bool value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_blnFree == value)
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
                if (_blnFree == value)
                    return;
                IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    _blnFree = value;
                }
                finally
                {
                    await objLocker2.DisposeAsync().ConfigureAwait(false);
                }

                await OnPropertyChangedAsync(nameof(Free), token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public bool IsFreeGrid
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _blnIsFreeGrid;
            }
            set
            {
                using (LockObject.EnterReadLock())
                {
                    if (_blnIsFreeGrid == value)
                        return;
                }

                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (_blnIsFreeGrid == value)
                        return;
                    using (LockObject.EnterWriteLock())
                    {
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
                    }
                    OnPropertyChanged();
                }
            }
        }

        public async Task<bool> GetIsFreeGridAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _blnIsFreeGrid;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public async Task SetIsFreeGridAsync(bool value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_blnIsFreeGrid == value)
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
                if (_blnIsFreeGrid == value)
                    return;
                IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    _blnIsFreeGrid = value;
                    QualitySource eOriginSource = await GetOriginSourceAsync(token).ConfigureAwait(false);
                    switch (value)
                    {
                        case true when eOriginSource == QualitySource.Selected:
                            await SetOriginSourceAsync(QualitySource.BuiltIn, token).ConfigureAwait(false);
                            break;

                        case false when eOriginSource == QualitySource.BuiltIn:
                            await SetOriginSourceAsync(QualitySource.Selected, token).ConfigureAwait(false);
                            break;
                    }
                }
                finally
                {
                    await objLocker2.DisposeAsync().ConfigureAwait(false);
                }

                await OnPropertyChangedAsync(nameof(IsFreeGrid), token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Whether this Quality should cost LP if it can be made to not cost LP.
        /// </summary>
        public bool UseLPCost
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _blnUseLPCost;
            }
            set
            {
                using (LockObject.EnterReadLock())
                {
                    if (_blnUseLPCost == value)
                        return;
                }

                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (_blnUseLPCost == value)
                        return;
                    using (LockObject.EnterWriteLock())
                        _blnUseLPCost = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Whether this Quality should cost LP if it can be made to not cost LP.
        /// </summary>
        public async Task<bool> GetUseLPCostAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _blnUseLPCost;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Whether this Quality should cost LP if it can be made to not cost LP.
        /// </summary>
        public async Task SetUseLPCostAsync(bool value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_blnUseLPCost == value)
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
                if (_blnUseLPCost == value)
                    return;
                IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    _blnUseLPCost = value;
                }
                finally
                {
                    await objLocker2.DisposeAsync().ConfigureAwait(false);
                }

                await OnPropertyChangedAsync(nameof(UseLPCost), token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Whether this Quality costs no money because of costing LP instead
        /// </summary>
        public bool IsFreeByLifestyle
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return OriginSource == QualitySource.BuiltIn || (UseLPCost && CanBeFreeByLifestyle);
            }
        }

        /// <summary>
        /// Whether this Quality costs no money because of costing LP instead
        /// </summary>
        public async Task<bool> GetIsFreeByLifestyleAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return await GetOriginSourceAsync(token).ConfigureAwait(false) == QualitySource.BuiltIn
                    || (await GetUseLPCostAsync(token).ConfigureAwait(false) && await GetCanBeFreeByLifestyleAsync(token).ConfigureAwait(false));
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Can this Quality have no nuyen costs based on the base lifestyle?
        /// </summary>
        public bool CanBeFreeByLifestyle
        {
            get
            {
                using (LockObject.EnterReadLock())
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
        /// Can this Quality have no nuyen costs based on the base lifestyle?
        /// </summary>
        public async Task<bool> GetCanBeFreeByLifestyleAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
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
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        ///     Comforts LP is increased/reduced by this Quality.
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
        ///     Comforts LP maximum is increased/reduced by this Quality.
        /// </summary>
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

        /// <summary>
        ///     Security LP value is increased/reduced by this Quality.
        /// </summary>
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

        /// <summary>
        ///     Security LP value is increased/reduced by this Quality.
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
        /// Percentage by which the quality increases the overall Lifestyle Cost.
        /// </summary>
        public decimal Multiplier
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return CostFree ? 0 : _decMultiplier;
            }
            private set
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
        /// Percentage by which the quality increases the overall Lifestyle Cost.
        /// </summary>
        public async Task<decimal> GetMultiplierAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return await GetCostFreeAsync(token).ConfigureAwait(false) ? 0 : _decMultiplier;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Percentage by which the quality increases the overall Lifestyle Cost.
        /// </summary>
        private async Task SetMultiplierAsync(decimal value, CancellationToken token = default)
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
        /// Percentage by which the quality increases the Lifestyle Cost ONLY, without affecting other qualities.
        /// </summary>
        public decimal BaseMultiplier
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return CostFree ? 0 : _decBaseMultiplier;
            }
            private set
            {
                using (LockObject.EnterReadLock())
                {
                    if (_decBaseMultiplier == value)
                        return;
                }

                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (_decBaseMultiplier == value)
                        return;
                    using (LockObject.EnterWriteLock())
                    {
                        _decBaseMultiplier = value;
                        OnPropertyChanged();
                    }
                }
            }
        }

        /// <summary>
        /// Percentage by which the quality increases the Lifestyle Cost ONLY, without affecting other qualities.
        /// </summary>
        public async Task<decimal> GetBaseMultiplierAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return await GetCostFreeAsync(token).ConfigureAwait(false) ? 0 : _decBaseMultiplier;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Percentage by which the quality increases the Lifestyle Cost ONLY, without affecting other qualities.
        /// </summary>
        private async Task SetBaseMultiplierAsync(decimal value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_decBaseMultiplier == value)
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
                if (_decBaseMultiplier == value)
                    return;
                IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    _decBaseMultiplier = value;
                    await OnPropertyChangedAsync(nameof(BaseMultiplier), token).ConfigureAwait(false);
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
        ///     Category of the Quality.
        /// </summary>
        public string Category
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _strCategory;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
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

        /// <summary>
        ///     Area/Neighborhood minimum is increased/reduced by this Quality.
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
                if (SourceID != Guid.Empty)
                    objReturn = objDoc.TryGetNodeById("/chummer/qualities/quality", SourceID);
                if (objReturn == null)
                {
                    objReturn = objDoc.TryGetNodeByNameOrId("/chummer/qualities/quality", Name);
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
        private QualitySource _eOriginSource = QualitySource.Selected;
        private XmlNode _xmlBonus;
        private QualityType _eType = QualityType.Positive;
        private Lifestyle _objParentLifestyle;

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
                    objReturn = objDoc.TryGetNodeById("/chummer/qualities/quality", SourceID);
                if (objReturn == null)
                {
                    objReturn = objDoc.TryGetNodeByNameOrId("/chummer/qualities/quality", Name);
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

        #endregion Properties

        #region UI Methods

        public async Task<TreeNode> CreateTreeNode(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (await GetOriginSourceAsync(token).ConfigureAwait(false) == QualitySource.BuiltIn
                    && !string.IsNullOrEmpty(Source)
                    && !await (await _objCharacter.GetSettingsAsync(token).ConfigureAwait(false)).BookEnabledAsync(Source, token).ConfigureAwait(false))
                    return null;

                TreeNode objNode = new TreeNode
                {
                    Name = InternalId,
                    Text = await GetCurrentFormattedDisplayNameAsync(token).ConfigureAwait(false),
                    Tag = this,
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

        public Color PreferredColor
        {
            get
            {
                using (LockObject.EnterReadLock())
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

        public async Task<Color> GetPreferredColorAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (!string.IsNullOrEmpty(await GetNotesAsync(token).ConfigureAwait(false)))
                {
                    return OriginSource == QualitySource.BuiltIn
                        ? ColorManager.GenerateCurrentModeDimmedColor(await GetNotesColorAsync(token).ConfigureAwait(false))
                        : ColorManager.GenerateCurrentModeColor(await GetNotesColorAsync(token).ConfigureAwait(false));
                }
                return OriginSource == QualitySource.BuiltIn
                    ? ColorManager.GrayText
                    : ColorManager.WindowText;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        #endregion UI Methods

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

        private int _intIsDisposed;

        public bool IsDisposed => _intIsDisposed > 0;

        /// <inheritdoc />
        public void Dispose()
        {
            if (Interlocked.CompareExchange(ref _intIsDisposed, 1, 0) > 0)
                return;
            using (LockObject.EnterWriteLock())
            {
                Utils.StringHashSetPool.Return(ref _setAllowedFreeLifestyles);
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
            if (Interlocked.CompareExchange(ref _intIsDisposed, 1, 0) > 0)
                return;
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync().ConfigureAwait(false);
            try
            {
                Utils.StringHashSetPool.Return(ref _setAllowedFreeLifestyles);
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

        public bool Remove(bool blnConfirmDelete = true)
        {
            bool blnReturn;
            using (LockObject.EnterUpgradeableReadLock())
            {
                if (blnConfirmDelete &&
                    !CommonFunctions.ConfirmDelete(LanguageManager.GetString("Message_DeleteQuality")))
                    return false;

                ImprovementManager.RemoveImprovements(_objCharacter, Improvement.ImprovementSource.Quality, InternalId);

                blnReturn = ParentLifestyle.LifestyleQualities.Remove(this);
            }

            Dispose();
            return blnReturn;
        }

        public async Task<bool> RemoveAsync(bool blnConfirmDelete = true, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            bool blnReturn;
            IAsyncDisposable objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (blnConfirmDelete && !await CommonFunctions
                        .ConfirmDeleteAsync(
                            await LanguageManager
                                .GetStringAsync("Message_DeleteQuality", token: token)
                                .ConfigureAwait(false), token).ConfigureAwait(false))
                    return false;

                await ImprovementManager
                    .RemoveImprovementsAsync(_objCharacter, Improvement.ImprovementSource.Quality, InternalId, token)
                    .ConfigureAwait(false);

                blnReturn = await ParentLifestyle.LifestyleQualities.RemoveAsync(this, token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }

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
                        new DependencyGraphNode<string, LifestyleQuality>(nameof(DisplayCost),
                            new DependencyGraphNode<string, LifestyleQuality>(nameof(Cost),
                                new DependencyGraphNode<string, LifestyleQuality>(nameof(CostFree),
                                    new DependencyGraphNode<string, LifestyleQuality>(nameof(Free)),
                                    new DependencyGraphNode<string, LifestyleQuality>(nameof(IsFreeByLifestyle),
                                        new DependencyGraphNode<string, LifestyleQuality>(nameof(OriginSource)),
                                        new DependencyGraphNode<string, LifestyleQuality>(nameof(UseLPCost), x => x.OriginSource != QualitySource.BuiltIn, async (x, t) => await x.GetOriginSourceAsync(t).ConfigureAwait(false) != QualitySource.BuiltIn),
                                        new DependencyGraphNode<string, LifestyleQuality>(nameof(CanBeFreeByLifestyle), x => x.OriginSource != QualitySource.BuiltIn, async (x, t) => await x.GetOriginSourceAsync(t).ConfigureAwait(false) != QualitySource.BuiltIn)
                                    )
                                ),
                                new DependencyGraphNode<string, LifestyleQuality>(nameof(CostString), x => !x.CostFree, async (x, t) => !await x.GetCostFreeAsync(t).ConfigureAwait(false))
                            ),
                            new DependencyGraphNode<string, LifestyleQuality>(nameof(Multiplier),
                                new DependencyGraphNode<string, LifestyleQuality>(nameof(CostFree))
                            )
                        )
                    )
                ),
                new DependencyGraphNode<string, LifestyleQuality>(nameof(LPCost),
                    new DependencyGraphNode<string, LifestyleQuality>(nameof(LPFree),
                        new DependencyGraphNode<string, LifestyleQuality>(nameof(Free)),
                        new DependencyGraphNode<string, LifestyleQuality>(nameof(UseLPCost), x => !x.Free, async (x, t) => !await x.GetFreeAsync(t).ConfigureAwait(false)),
                        new DependencyGraphNode<string, LifestyleQuality>(nameof(CanBeFreeByLifestyle), x => !x.Free, async (x, t) => !await x.GetFreeAsync(t).ConfigureAwait(false))
                    )
                ),
                new DependencyGraphNode<string, LifestyleQuality>(nameof(BaseMultiplier),
                    new DependencyGraphNode<string, LifestyleQuality>(nameof(CostFree))
                )
            );

        /// <inheritdoc />
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

        /// <inheritdoc />
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
                        using (new FetchSafelyFromSafeObjectPool<HashSet<string>>(Utils.StringHashSetPool,
                                                                        out HashSet<string>
                                                                            setParentLifestyleNamesOfChangedProperties))
                        {
                            if (setNamesOfChangedProperties.Contains(nameof(LPCost)))
                                setParentLifestyleNamesOfChangedProperties.Add(nameof(Lifestyle.TotalLP));

                            if ((setNamesOfChangedProperties.Contains(nameof(Cost))
                                 || setNamesOfChangedProperties.Contains(nameof(BaseMultiplier))
                                 || setNamesOfChangedProperties.Contains(nameof(Multiplier)))
                                && OriginSource != QualitySource.BuiltIn)
                            {
                                if (Type == QualityType.Contracts || (Type == QualityType.Entertainment &&
                                                                      !Category.Contains("Asset")))
                                    setParentLifestyleNamesOfChangedProperties.Add(
                                        nameof(Lifestyle.GetTotalMonthlyCost));
                                else
                                    setParentLifestyleNamesOfChangedProperties.Add(nameof(Lifestyle.CostPreSplit));
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
                                ParentLifestyle.OnMultiplePropertiesChanged(setParentLifestyleNamesOfChangedProperties);
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
                                = await s_LifestyleQualityDependencyGraph.GetWithAllDependentsAsync(this, strPropertyName, true, token).ConfigureAwait(false);
                        else
                        {
                            foreach (string strLoopChangedProperty in await s_LifestyleQualityDependencyGraph
                                         .GetWithAllDependentsEnumerableAsync(this, strPropertyName, token).ConfigureAwait(false))
                                setNamesOfChangedProperties.Add(strLoopChangedProperty);
                        }
                    }

                    if (setNamesOfChangedProperties == null || setNamesOfChangedProperties.Count == 0)
                        return;

                    if (ParentLifestyle != null)
                    {
                        using (new FetchSafelyFromSafeObjectPool<HashSet<string>>(Utils.StringHashSetPool,
                                   out HashSet<string>
                                       setParentLifestyleNamesOfChangedProperties))
                        {
                            if (setNamesOfChangedProperties.Contains(nameof(LPCost)))
                                setParentLifestyleNamesOfChangedProperties.Add(nameof(Lifestyle.TotalLP));

                            if ((setNamesOfChangedProperties.Contains(nameof(Cost))
                                 || setNamesOfChangedProperties.Contains(nameof(BaseMultiplier))
                                 || setNamesOfChangedProperties.Contains(nameof(Multiplier)))
                                && await GetOriginSourceAsync(token).ConfigureAwait(false) != QualitySource.BuiltIn)
                            {
                                if (Type == QualityType.Contracts || (Type == QualityType.Entertainment &&
                                                                      !Category.Contains("Asset")))
                                    setParentLifestyleNamesOfChangedProperties.Add(
                                        nameof(Lifestyle.GetTotalMonthlyCost));
                                else
                                    setParentLifestyleNamesOfChangedProperties.Add(nameof(Lifestyle.CostPreSplit));
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
                                await ParentLifestyle
                                    .OnMultiplePropertiesChangedAsync(setParentLifestyleNamesOfChangedProperties, token)
                                    .ConfigureAwait(false);
                        }
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
                                    foreach (string strPropertyToChange in setNamesOfChangedProperties)
                                    {
                                        token.ThrowIfCancellationRequested();
                                        PropertyChanged.Invoke(this, new PropertyChangedEventArgs(strPropertyToChange));
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
                                foreach (string strPropertyToChange in lstPropertyNames)
                                {
                                    token.ThrowIfCancellationRequested();
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

        /// <inheritdoc />
        public AsyncFriendlyReaderWriterLock LockObject { get; }

        public Character CharacterObject => _objCharacter; // readonly member, no locking required
    }
}
