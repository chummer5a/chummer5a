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
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;
using Chummer.Annotations;
using Chummer.Backend.Equipment;
using NLog;

namespace Chummer
{
    /// <summary>
    /// Type of Quality.
    /// </summary>
    public enum QualityType
    {
        Positive = 0,
        Negative = 1,
        LifeModule = 2,
        Entertainment = 3,
        Contracts = 4
    }

    /// <summary>
    /// Source of the Quality.
    /// </summary>
    public enum QualitySource
    {
        Selected = 0,
        Metatype = 1,
        MetatypeRemovable = 2,
        BuiltIn = 3,
        LifeModule = 4,
        Improvement = 5,
        MetatypeRemovedAtChargen = 6,
        Heritage = 7
    }

    /// <summary>
    /// Reason a quality is not valid
    /// </summary>
    [Flags]
    public enum QualityFailureReasons
    {
        None = 0x0,
        LimitExceeded = 0x1,
        RequiredSingle = 0x2,
        RequiredMultiple = 0x4,
        ForbiddenSingle = 0x8,
        MetatypeRequired = 0x10
    }

    /// <summary>
    /// A Quality.
    /// </summary>
    [HubClassTag("SourceID", true, "Name", "Extra;Type")]
    [DebuggerDisplay("{CurrentDisplayName}")]
    public sealed class Quality : IHasInternalId, IHasName, IHasSourceId, IHasXmlDataNode, IHasNotes, IHasSource, INotifyMultiplePropertiesChangedAsync, IHasLockObject, IHasCharacterObject
    {
        private static readonly Lazy<Logger> s_ObjLogger = new Lazy<Logger>(LogManager.GetCurrentClassLogger);
        private static Logger Log => s_ObjLogger.Value;
        private Guid _guiSourceID = Guid.Empty;
        private Guid _guiID;
        private string _strName = string.Empty;
        private bool _blnMetagenic;
        private string _strExtra = string.Empty;
        private string _strSource = string.Empty;
        private string _strPage = string.Empty;
        private bool _blnMutant;
        private string _strNotes = string.Empty;
        private Color _colNotes = ColorManager.HasNotesColor;
        private bool _blnImplemented = true;
        private bool _blnContributeToBP = true;
        private bool _blnContributeToLimit = true;
        private bool _blnPrint = true;
        private bool _blnDoubleCostCareer = true;
        private bool _blnCanBuyWithSpellPoints;
        private int _intBP;
        private QualityType _eQualityType = QualityType.Positive;
        private QualitySource _eQualitySource = QualitySource.Selected;
        private string _strSourceName = string.Empty;
        private XmlNode _nodBonus;
        private XmlNode _nodFirstLevelBonus;
        private XmlNode _nodNaturalWeaponsNode;
        private XPathNavigator _nodDiscounts;
        private readonly Character _objCharacter;
        private Guid _guiWeaponID;
        private string _strStage;
        private bool _blnStagedPurchase;

        public Character CharacterObject => _objCharacter; // readonly member, no locking needed

        public string Stage
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _strStage;
            }
        }

        #region Helper Methods

        /// <summary>
        /// Convert a string to a QualityType.
        /// </summary>
        /// <param name="strValue">String value to convert.</param>
        public static QualityType ConvertToQualityType(string strValue)
        {
            if (string.IsNullOrEmpty(strValue))
                return default;
            switch (strValue)
            {
                case "Negative":
                    return QualityType.Negative;

                case "LifeModule":
                    return QualityType.LifeModule;

                default:
                    return QualityType.Positive;
            }
        }

        /// <summary>
        /// Convert a string to a QualitySource.
        /// </summary>
        /// <param name="strValue">String value to convert.</param>
        public static QualitySource ConvertToQualitySource(string strValue)
        {
            if (string.IsNullOrEmpty(strValue))
                return default;
            switch (strValue)
            {
                case "Metatype":
                    return QualitySource.Metatype;

                case "MetatypeRemovable":
                    return QualitySource.MetatypeRemovable;

                case "LifeModule":
                    return QualitySource.LifeModule;

                case "Built-In":
                    return QualitySource.BuiltIn;

                case "Improvement":
                    return QualitySource.Improvement;

                case "MetatypeRemovedAtChargen":
                    return QualitySource.MetatypeRemovedAtChargen;

                case "Heritage":
                    return QualitySource.Heritage;

                default:
                    return QualitySource.Selected;
            }
        }

        #endregion Helper Methods

        #region Constructor, Create, Save, Load, and Print Methods

        public Quality(Character objCharacter)
        {
            // Create the GUID for the new Quality.
            _guiID = Guid.NewGuid();
            _objCharacter = objCharacter ?? throw new ArgumentNullException(nameof(objCharacter));
            LockObject = objCharacter.LockObject;
        }

        public void SetGUID(Guid guidExisting)
        {
            using (LockObject.EnterWriteLock())
                _guiID = guidExisting;
        }

        /// <summary>
        /// Create a Quality from an XmlNode.
        /// </summary>
        /// <param name="objXmlQuality">XmlNode to create the object from.</param>
        /// <param name="objQualitySource">Source of the Quality.</param>
        /// <param name="lstWeapons">List of Weapons that should be added to the Character.</param>
        /// <param name="strForceValue">Force a value to be selected for the Quality.</param>
        /// <param name="strSourceName">Friendly name for the improvement that added this quality.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public void Create(XmlNode objXmlQuality, QualitySource objQualitySource, IList<Weapon> lstWeapons, string strForceValue = "", string strSourceName = "", CancellationToken token = default)
        {
            Utils.SafelyRunSynchronously(() => CreateCoreAsync(true, objXmlQuality, objQualitySource, lstWeapons, strForceValue, strSourceName, token), token);
        }

        /// <summary>
        /// Create a Quality from an XmlNode.
        /// </summary>
        /// <param name="objXmlQuality">XmlNode to create the object from.</param>
        /// <param name="objQualitySource">Source of the Quality.</param>
        /// <param name="lstWeapons">List of Weapons that should be added to the Character.</param>
        /// <param name="strForceValue">Force a value to be selected for the Quality.</param>
        /// <param name="strSourceName">Friendly name for the improvement that added this quality.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public Task CreateAsync(XmlNode objXmlQuality, QualitySource objQualitySource, IList<Weapon> lstWeapons, string strForceValue = "", string strSourceName = "", CancellationToken token = default)
        {
            return CreateCoreAsync(false, objXmlQuality, objQualitySource, lstWeapons, strForceValue, strSourceName, token);
        }

        private async Task CreateCoreAsync(bool blnSync, XmlNode objXmlQuality, QualitySource objQualitySource, IList<Weapon> lstWeapons,
            string strForceValue = "", string strSourceName = "", CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (!objXmlQuality.TryGetField("id", Guid.TryParse, out _guiSourceID))
            {
                Log.Warn(new object[] { "Missing id field for xmlnode", objXmlQuality });
                Utils.BreakIfDebug();
            }

            IDisposable objSyncLocker = null;
            IAsyncDisposable objAsyncLocker = null;
            if (blnSync)
                objSyncLocker = LockObject.EnterWriteLock(token);
            else
                objAsyncLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                _strSourceName = strSourceName;
                objXmlQuality.TryGetStringFieldQuickly("name", ref _strName);
                if (!objXmlQuality.TryGetBoolFieldQuickly("metagenic", ref _blnMetagenic))
                {
                    //Shim for customdata files that have the old name for the metagenic flag.
                    objXmlQuality.TryGetBoolFieldQuickly("metagenetic", ref _blnMetagenic);
                }

                if (!objXmlQuality.TryGetMultiLineStringFieldQuickly("altnotes", ref _strNotes))
                    objXmlQuality.TryGetMultiLineStringFieldQuickly("notes", ref _strNotes);

                string sNotesColor = ColorTranslator.ToHtml(ColorManager.HasNotesColor);
                objXmlQuality.TryGetStringFieldQuickly("notesColor", ref sNotesColor);
                _colNotes = ColorTranslator.FromHtml(sNotesColor);

                objXmlQuality.TryGetInt32FieldQuickly("karma", ref _intBP);
                _eQualityType = ConvertToQualityType(objXmlQuality["category"]?.InnerText);
                _eQualitySource = objQualitySource;
                objXmlQuality.TryGetBoolFieldQuickly("doublecareer", ref _blnDoubleCostCareer);
                objXmlQuality.TryGetBoolFieldQuickly("canbuywithspellpoints", ref _blnCanBuyWithSpellPoints);
                objXmlQuality.TryGetBoolFieldQuickly("print", ref _blnPrint);
                objXmlQuality.TryGetBoolFieldQuickly("implemented", ref _blnImplemented);
                objXmlQuality.TryGetBoolFieldQuickly("contributetobp", ref _blnContributeToBP);
                objXmlQuality.TryGetBoolFieldQuickly("contributetolimit", ref _blnContributeToLimit);
                objXmlQuality.TryGetBoolFieldQuickly("stagedpurchase", ref _blnStagedPurchase);
                objXmlQuality.TryGetStringFieldQuickly("source", ref _strSource);
                objXmlQuality.TryGetStringFieldQuickly("page", ref _strPage);
                _blnMutant = objXmlQuality["mutant"] != null;

                if (_eQualityType == QualityType.LifeModule)
                {
                    objXmlQuality.TryGetStringFieldQuickly("stage", ref _strStage);
                }

                // Add Weapons if applicable.
                // More than one Weapon can be added, so loop through all occurrences.
                using (XmlNodeList xmlAddWeaponList = objXmlQuality.SelectNodes("addweapon"))
                {
                    if (xmlAddWeaponList?.Count > 0 && lstWeapons != null)
                    {
                        XmlDocument objXmlWeaponDocument = blnSync
                            // ReSharper disable once MethodHasAsyncOverload
                            ? _objCharacter.LoadData("weapons.xml", token: token)
                            : await _objCharacter.LoadDataAsync("weapons.xml", token: token).ConfigureAwait(false);
                        foreach (XmlNode objXmlAddWeapon in xmlAddWeaponList)
                        {
                            string strLoopID = objXmlAddWeapon.InnerText;
                            XmlNode objXmlWeapon = objXmlWeaponDocument.TryGetNodeByNameOrId(
                                "/chummer/weapons/weapon", strLoopID);
                            if (objXmlWeapon != null)
                            {
                                int intAddWeaponRating = 0;
                                string strWeaponRating = objXmlAddWeapon.Attributes?["rating"]?.InnerText;
                                if (!string.IsNullOrEmpty(strWeaponRating) && int.TryParse(
                                        strWeaponRating, NumberStyles.Any, GlobalSettings.InvariantCultureInfo,
                                        out int intWeaponRating))
                                {
                                    intAddWeaponRating = intWeaponRating;
                                }

                                Weapon objGearWeapon = new Weapon(_objCharacter);
                                if (blnSync)
                                    // ReSharper disable once MethodHasAsyncOverload
                                    objGearWeapon.Create(objXmlWeapon, lstWeapons, true, true, true, intAddWeaponRating, token: token);
                                else
                                    await objGearWeapon.CreateAsync(objXmlWeapon, lstWeapons, true, true, true, intAddWeaponRating, token: token).ConfigureAwait(false);
                                objGearWeapon.ParentID = InternalId;
                                objGearWeapon.Cost = "0";

                                if (Guid.TryParse(objGearWeapon.InternalId, out _guiWeaponID))
                                    lstWeapons.Add(objGearWeapon);
                                else
                                    _guiWeaponID = Guid.Empty;
                            }
                            else
                            {
                                Utils.BreakIfDebug();
                            }
                        }
                    }
                }

                _nodDiscounts = objXmlQuality.SelectSingleNodeAndCacheExpressionAsNavigator("costdiscount", token);
                // If the item grants a bonus, pass the information to the Improvement Manager.
                _nodBonus = objXmlQuality["bonus"];
                if (_nodBonus?.ChildNodes.Count > 0)
                {
                    ImprovementManager.SetForcedValue(strForceValue, _objCharacter);
                    if (blnSync)
                    {
                        // ReSharper disable once MethodHasAsyncOverload
                        if (!ImprovementManager.CreateImprovements(_objCharacter, Improvement.ImprovementSource.Quality,
                                InternalId, _nodBonus, 1, CurrentDisplayNameShort, token: token))
                        {
                            _guiID = Guid.Empty;
                            return;
                        }
                    }
                    else if (!await ImprovementManager.CreateImprovementsAsync(_objCharacter, Improvement.ImprovementSource.Quality,
                                 InternalId, _nodBonus, 1, await GetCurrentDisplayNameShortAsync(token).ConfigureAwait(false), token: token).ConfigureAwait(false))
                    {
                        _guiID = Guid.Empty;
                        return;
                    }

                    string strSelectedValue = ImprovementManager.GetSelectedValue(_objCharacter);
                    if (!string.IsNullOrEmpty(strSelectedValue))
                    {
                        _strExtra = strSelectedValue;
                    }
                }
                else if (!string.IsNullOrEmpty(strForceValue))
                {
                    _strExtra = strForceValue;
                }

                _nodFirstLevelBonus = objXmlQuality["firstlevelbonus"];
                if (_nodFirstLevelBonus?.ChildNodes.Count > 0 && (blnSync ? Levels : await GetLevelsAsync(token).ConfigureAwait(false)) == 0)
                {
                    ImprovementManager.SetForcedValue(string.IsNullOrEmpty(strForceValue) ? Extra : strForceValue, _objCharacter);
                    if (blnSync)
                    {
                        // ReSharper disable once MethodHasAsyncOverload
                        if (!ImprovementManager.CreateImprovements(_objCharacter, Improvement.ImprovementSource.Quality,
                                InternalId, _nodFirstLevelBonus, 1,
                                CurrentDisplayNameShort, token: token))
                        {
                            _guiID = Guid.Empty;
                            return;
                        }
                    }
                    else if (!await ImprovementManager.CreateImprovementsAsync(_objCharacter, Improvement.ImprovementSource.Quality,
                                 InternalId, _nodFirstLevelBonus, 1,
                                 await GetCurrentDisplayNameShortAsync(token).ConfigureAwait(false), token: token).ConfigureAwait(false))
                    {
                        _guiID = Guid.Empty;
                        return;
                    }
                }

                _nodNaturalWeaponsNode = objXmlQuality["naturalweapons"];
                // Hacky to handle the naturalweapons node as another bonus node, but it will suffice because bonus nodes can have naturalweapon nodes
                if (_nodNaturalWeaponsNode?.ChildNodes.Count > 0)
                {
                    ImprovementManager.SetForcedValue(string.IsNullOrEmpty(strForceValue) ? Extra : strForceValue, _objCharacter);
                    if (blnSync)
                    {
                        // ReSharper disable once MethodHasAsyncOverload
                        if (!ImprovementManager.CreateImprovements(_objCharacter, Improvement.ImprovementSource.Quality,
                                InternalId, _nodNaturalWeaponsNode, 1,
                                CurrentDisplayNameShort, token: token))
                        {
                            _guiID = Guid.Empty;
                            return;
                        }
                    }
                    else if (!await ImprovementManager.CreateImprovementsAsync(_objCharacter, Improvement.ImprovementSource.Quality,
                                 InternalId, _nodNaturalWeaponsNode, 1,
                                 await GetCurrentDisplayNameShortAsync(token).ConfigureAwait(false), token: token).ConfigureAwait(false))
                    {
                        _guiID = Guid.Empty;
                        return;
                    }
                }

                // Check if the quality is already suppressed by something
                if (blnSync)
                    // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                    RefreshSuppressed();
                else
                    await RefreshSuppressedAsync(token).ConfigureAwait(false);

                if (GlobalSettings.InsertPdfNotesIfAvailable)
                {
                    if (blnSync)
                    {
                        if (string.IsNullOrEmpty(Notes))
                        {
                            // ReSharper disable once MethodHasAsyncOverload
                            Notes = CommonFunctions.GetBookNotes(objXmlQuality, Name, CurrentDisplayName, Source, Page,
                                // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                                DisplayPage(GlobalSettings.Language), _objCharacter, token);
                        }
                    }
                    else if (string.IsNullOrEmpty(await GetNotesAsync(token).ConfigureAwait(false)))
                    {
                        await SetNotesAsync(await CommonFunctions.GetBookNotesAsync(objXmlQuality,
                            await GetNameAsync(token).ConfigureAwait(false),
                            await GetCurrentDisplayNameAsync(token).ConfigureAwait(false), Source, Page,
                            await DisplayPageAsync(GlobalSettings.Language, token).ConfigureAwait(false), _objCharacter,
                            token).ConfigureAwait(false), token).ConfigureAwait(false);
                    }
                }
            }
            finally
            {
                objSyncLocker?.Dispose();
                if (objAsyncLocker != null)
                    await objAsyncLocker.DisposeAsync().ConfigureAwait(false);
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
                objWriter.WriteStartElement("quality");
                objWriter.WriteElementString("sourceid", SourceIDString);
                objWriter.WriteElementString("guid", InternalId);
                objWriter.WriteElementString("name", _strName);
                objWriter.WriteElementString("extra", _strExtra);
                objWriter.WriteElementString("bp", _intBP.ToString(GlobalSettings.InvariantCultureInfo));
                objWriter.WriteElementString("implemented",
                                             _blnImplemented.ToString(GlobalSettings.InvariantCultureInfo));
                objWriter.WriteElementString("contributetobp",
                                             _blnContributeToBP.ToString(GlobalSettings.InvariantCultureInfo));
                objWriter.WriteElementString("contributetolimit",
                                             _blnContributeToLimit.ToString(GlobalSettings.InvariantCultureInfo));
                objWriter.WriteElementString("stagedpurchase",
                                             _blnStagedPurchase.ToString(GlobalSettings.InvariantCultureInfo));
                objWriter.WriteElementString("doublecareer",
                                             _blnDoubleCostCareer.ToString(GlobalSettings.InvariantCultureInfo));
                objWriter.WriteElementString("canbuywithspellpoints",
                                             _blnCanBuyWithSpellPoints.ToString(GlobalSettings.InvariantCultureInfo));
                objWriter.WriteElementString("metagenic", _blnMetagenic.ToString(GlobalSettings.InvariantCultureInfo));
                objWriter.WriteElementString("print", _blnPrint.ToString(GlobalSettings.InvariantCultureInfo));
                objWriter.WriteElementString("qualitytype", _eQualityType.ToString());
                objWriter.WriteElementString("qualitysource", _eQualitySource.ToString());
                objWriter.WriteElementString("mutant", _blnMutant.ToString(GlobalSettings.InvariantCultureInfo));
                objWriter.WriteElementString("source", _strSource);
                objWriter.WriteElementString("page", _strPage);
                objWriter.WriteElementString("sourcename", _strSourceName);
                if (!string.IsNullOrEmpty(_nodBonus?.InnerXml))
                    objWriter.WriteRaw("<bonus>" + _nodBonus.InnerXml + "</bonus>");
                else
                    objWriter.WriteElementString("bonus", string.Empty);
                if (!string.IsNullOrEmpty(_nodFirstLevelBonus?.InnerXml))
                    objWriter.WriteRaw("<firstlevelbonus>" + _nodFirstLevelBonus.InnerXml + "</firstlevelbonus>");
                else
                    objWriter.WriteElementString("firstlevelbonus", string.Empty);
                if (!string.IsNullOrEmpty(_nodNaturalWeaponsNode?.InnerXml))
                    objWriter.WriteRaw("<naturalweapons>" + _nodNaturalWeaponsNode.InnerXml + "</naturalweapons>");
                else
                    objWriter.WriteElementString("naturalweapons", string.Empty);
                if (_guiWeaponID != Guid.Empty)
                    objWriter.WriteElementString("weaponguid",
                                                 _guiWeaponID.ToString("D", GlobalSettings.InvariantCultureInfo));
                if (_nodDiscounts != null)
                    objWriter.WriteRaw("<costdiscount>" + _nodDiscounts.InnerXml + "</costdiscount>");
                objWriter.WriteElementString("notes", _strNotes.CleanOfInvalidUnicodeChars());
                objWriter.WriteElementString("notesColor", ColorTranslator.ToHtml(_colNotes));
                if (_eQualityType == QualityType.LifeModule)
                {
                    objWriter.WriteElementString("stage", _strStage);
                }

                objWriter.WriteEndElement();
            }
        }

        /// <summary>
        /// Load the CharacterAttribute from the XmlNode.
        /// </summary>
        /// <param name="objNode">XmlNode to load.</param>
        public void Load(XmlNode objNode)
        {
            if (objNode == null)
                return;
            using (LockObject.EnterWriteLock())
            {
                if (!objNode.TryGetField("guid", Guid.TryParse, out _guiID))
                {
                    _guiID = Guid.NewGuid();
                }

                objNode.TryGetStringFieldQuickly("name", ref _strName);
                _objCachedMyXmlNode = null;
                _objCachedMyXPathNode = null;
                Lazy<XmlNode> objMyNode = new Lazy<XmlNode>(() => this.GetNode());
                if (!objNode.TryGetGuidFieldQuickly("sourceid", ref _guiSourceID))
                {
                    objMyNode.Value?.TryGetGuidFieldQuickly("id", ref _guiSourceID);
                }

                objNode.TryGetStringFieldQuickly("extra", ref _strExtra);
                objNode.TryGetInt32FieldQuickly("bp", ref _intBP);
                objNode.TryGetBoolFieldQuickly("implemented", ref _blnImplemented);
                objNode.TryGetBoolFieldQuickly("contributetobp", ref _blnContributeToBP);
                objNode.TryGetBoolFieldQuickly("contributetolimit", ref _blnContributeToLimit);
                objNode.TryGetBoolFieldQuickly("stagedpurchase", ref _blnStagedPurchase);
                objNode.TryGetBoolFieldQuickly("print", ref _blnPrint);
                objNode.TryGetBoolFieldQuickly("doublecareer", ref _blnDoubleCostCareer);
                objNode.TryGetBoolFieldQuickly("canbuywithspellpoints", ref _blnCanBuyWithSpellPoints);
                _eQualityType = ConvertToQualityType(objNode["qualitytype"]?.InnerText);
                _eQualitySource = ConvertToQualitySource(objNode["qualitysource"]?.InnerText);
                string strTemp = string.Empty;
                if (objNode.TryGetStringFieldQuickly("metagenic", ref strTemp)
                    //Shim for characters files that have the old name for the metagenic flag.
                    || objNode.TryGetStringFieldQuickly("metagenetic", ref strTemp))
                {
                    _blnMetagenic = strTemp == bool.TrueString || strTemp == "yes";
                }

                if (objNode.TryGetStringFieldQuickly("mutant", ref strTemp))
                {
                    _blnMutant = strTemp == bool.TrueString || strTemp == "yes";
                }

                objNode.TryGetStringFieldQuickly("source", ref _strSource);
                objNode.TryGetStringFieldQuickly("page", ref _strPage);
                objNode.TryGetStringFieldQuickly("sourcename", ref _strSourceName);
                _nodBonus = objNode["bonus"];
                _nodFirstLevelBonus = objNode["firstlevelbonus"] ?? objMyNode.Value?["firstlevelbonus"];
                _nodNaturalWeaponsNode = objNode["naturalweapons"] ?? objMyNode.Value?["naturalweapons"];
                _nodDiscounts = objNode.SelectSingleNodeAndCacheExpressionAsNavigator("costdiscount");
                objNode.TryGetField("weaponguid", Guid.TryParse, out _guiWeaponID);
                objNode.TryGetMultiLineStringFieldQuickly("notes", ref _strNotes);

                string sNotesColor = ColorTranslator.ToHtml(ColorManager.HasNotesColor);
                objNode.TryGetStringFieldQuickly("notesColor", ref sNotesColor);
                _colNotes = ColorTranslator.FromHtml(sNotesColor);

                if (_eQualityType == QualityType.LifeModule)
                {
                    objNode.TryGetStringFieldQuickly("stage", ref _strStage);
                }

                switch (_eQualitySource)
                {
                    case QualitySource.Selected when string.IsNullOrEmpty(_nodBonus?.InnerText)
                                                     && string.IsNullOrEmpty(_nodFirstLevelBonus?.InnerText)
                                                     && string.IsNullOrEmpty(_nodNaturalWeaponsNode?.InnerText)
                                                     && (_eQualityType == QualityType.Positive
                                                         || _eQualityType == QualityType.Negative)
                                                     && objMyNode.Value != null
                                                     && ConvertToQualityType(objMyNode.Value["category"]?.InnerText)
                                                     != _eQualityType:
                        _eQualitySource = QualitySource.MetatypeRemovedAtChargen;
                        break;
                    // Legacy shim for priority-given qualities
                    case QualitySource.Metatype when _objCharacter.LastSavedVersion <= new ValueVersion(5, 212, 71)
                                                     && _objCharacter.EffectiveBuildMethodUsesPriorityTables
                                                     && objMyNode.Value != null
                                                     && objMyNode.Value["onlyprioritygiven"] != null:
                        _eQualitySource = QualitySource.Heritage;
                        break;
                }
            }
        }

        /// <summary>
        /// Print the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        /// <param name="intRating">Pre-calculated rating of the quality for printing.</param>
        /// <param name="objCulture">Culture in which to print.</param>
        /// <param name="strLanguageToPrint">Language in which to print</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public async Task Print(XmlWriter objWriter, int intRating, CultureInfo objCulture, string strLanguageToPrint, CancellationToken token = default)
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
                    = await objWriter.StartElementAsync("quality", token: token).ConfigureAwait(false);
                try
                {
                    await objWriter.WriteElementStringAsync("guid", InternalId, token: token).ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("sourceid", SourceIDString, token: token)
                        .ConfigureAwait(false);
                    await objWriter
                        .WriteElementStringAsync(
                            "name", await DisplayNameShortAsync(strLanguageToPrint, token).ConfigureAwait(false),
                            token: token).ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("name_english", Name, token: token).ConfigureAwait(false);
                    string strSpace = await LanguageManager
                        .GetStringAsync("String_Space", strLanguageToPrint, token: token)
                        .ConfigureAwait(false);
                    string strRatingString = string.Empty;
                    if (intRating > 1)
                        strRatingString = strSpace + intRating.ToString(objCulture);
                    string strSourceName = string.Empty;
                    if (!string.IsNullOrWhiteSpace(SourceName))
                        strSourceName = strSpace + '('
                                                 + await GetSourceNameAsync(strLanguageToPrint, token)
                                                     .ConfigureAwait(false) + ')';
                    await objWriter.WriteElementStringAsync(
                            "extra",
                            await _objCharacter.TranslateExtraAsync(Extra, strLanguageToPrint, token: token)
                                .ConfigureAwait(false) + strRatingString + strSourceName,
                            token: token)
                        .ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("bp", BP.ToString(objCulture), token: token)
                        .ConfigureAwait(false);
                    string strQualityType = Type.ToString();
                    if (!strLanguageToPrint.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                    {
                        strQualityType =
                            (await _objCharacter
                                .LoadDataXPathAsync("qualities.xml", strLanguageToPrint, token: token)
                                .ConfigureAwait(false))
                            .SelectSingleNodeAndCacheExpression(
                                "/chummer/categories/category[. = " + strQualityType.CleanXPath()
                                                                    + "]/@translate", token: token)
                            ?.Value ?? strQualityType;
                    }

                    await objWriter.WriteElementStringAsync("qualitytype", strQualityType, token: token)
                        .ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("qualitytype_english", Type.ToString(), token: token)
                        .ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("qualitysource", OriginSource.ToString(), token: token)
                        .ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("metagenic", Metagenic.ToString(), token: token)
                        .ConfigureAwait(false);
                    await objWriter
                        .WriteElementStringAsync(
                            "source",
                            await _objCharacter.LanguageBookShortAsync(Source, strLanguageToPrint, token)
                                .ConfigureAwait(false), token: token).ConfigureAwait(false);
                    await objWriter
                        .WriteElementStringAsync(
                            "page", await DisplayPageAsync(strLanguageToPrint, token).ConfigureAwait(false),
                            token: token).ConfigureAwait(false);
                    if (GlobalSettings.PrintNotes)
                        await objWriter
                            .WriteElementStringAsync("notes", await GetNotesAsync(token).ConfigureAwait(false),
                                token: token).ConfigureAwait(false);
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
        /// Identifier of the object within data files.
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
        /// String-formatted identifier of the <inheritdoc cref="SourceID"/> from the data files.
        /// </summary>
        public string SourceIDString
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _guiSourceID.ToString("D", GlobalSettings.InvariantCultureInfo);
            }
        }

        /// <summary>
        /// String-formatted identifier of the <inheritdoc cref="SourceID"/> from the data files.
        /// </summary>
        public async Task<string> GetSourceIDStringAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _guiSourceID.ToString("D", GlobalSettings.InvariantCultureInfo);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Internal identifier which will be used to identify this Quality in the Improvement system.
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
        /// Guid of a Weapon.
        /// </summary>
        public string WeaponID
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _guiWeaponID.ToString("D", GlobalSettings.InvariantCultureInfo);
            }
            set
            {
                using (LockObject.EnterReadLock())
                {
                    if (!Guid.TryParse(value, out Guid guiTemp) || _guiWeaponID == guiTemp)
                        return;
                }

                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (!Guid.TryParse(value, out Guid guiTemp) || _guiWeaponID == guiTemp)
                        return;
                    using (LockObject.EnterWriteLock())
                        _guiWeaponID = guiTemp;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Quality's name.
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
        /// Quality's name.
        /// </summary>
        public async Task<string> GetNameAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
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
        /// Does the quality come from being a Changeling?
        /// </summary>
        public bool Metagenic
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _blnMetagenic;
            }
        }

        /// <summary>
        /// Does the quality come from being a Changeling?
        /// </summary>
        public async Task<bool> GetMetagenicAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _blnMetagenic;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Extra information that should be applied to the name, like a linked CharacterAttribute.
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
                value = _objCharacter.ReverseTranslateExtra(value);
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (Interlocked.Exchange(ref _strExtra, value) == value)
                        return;
                    OnPropertyChanged();
                }
            }
        }

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
        /// Page Number.
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
        /// Name of the Improvement that added this quality.
        /// </summary>
        public string SourceName
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _strSourceName;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (Interlocked.Exchange(ref _strSourceName, value) != value)
                        OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Name of the Improvement that added this quality.
        /// </summary>
        public string GetSourceName(string strLanguage)
        {
            using (LockObject.EnterReadLock())
                return _objCharacter.TranslateExtra(_strSourceName, strLanguage);
        }

        /// <summary>
        /// Name of the Improvement that added this quality.
        /// </summary>
        public async Task<string> GetSourceNameAsync(string strLanguage, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return await _objCharacter.TranslateExtraAsync(_strSourceName, strLanguage, token: token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Bonus node from the XML file.
        /// </summary>
        public XmlNode Bonus
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _nodBonus;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (Interlocked.Exchange(ref _nodBonus, value) != value)
                        OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Bonus node from the XML file only awarded for the first instance the character has the quality.
        /// </summary>
        public XmlNode FirstLevelBonus
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _nodFirstLevelBonus;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (Interlocked.Exchange(ref _nodFirstLevelBonus, value) != value)
                        OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Bonus node from the XML file that exists purely for legacy reasons to support the older way of adding natural weapons.
        /// </summary>
        public XmlNode NaturalWeaponsNode
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _nodNaturalWeaponsNode;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (Interlocked.Exchange(ref _nodNaturalWeaponsNode, value) != value)
                        OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Quality Type.
        /// </summary>
        public QualityType Type
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _eQualityType;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (InterlockedExtensions.Exchange(ref _eQualityType, value) != value)
                        OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Quality Type.
        /// </summary>
        public async Task<QualityType> GetTypeAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _eQualityType;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Source of the Quality.
        /// </summary>
        public QualitySource OriginSource
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _eQualitySource;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (InterlockedExtensions.Exchange(ref _eQualitySource, value) != value)
                        OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Source of the Quality.
        /// </summary>
        public async Task<QualitySource> GetOriginSourceAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _eQualitySource;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Number of Build Points the Quality costs.
        /// </summary>
        public int BP
        {
            get
            {
                using (LockObject.EnterReadLock())
                {
                    int intValue = 0;
                    if (_nodDiscounts?.TryGetInt32FieldQuickly("value", ref intValue) != true)
                        return _intBP;
                    int intReturn = _intBP;
                    if (_nodDiscounts.RequirementsMet(_objCharacter))
                    {
                        switch (Type)
                        {
                            case QualityType.Positive:
                                intReturn += intValue;
                                break;

                            case QualityType.Negative:
                                intReturn -= intValue;
                                break;
                        }
                    }

                    return intReturn;
                }
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (Interlocked.Exchange(ref _intBP, value) != value)
                        OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Number of Build Points the Quality costs.
        /// </summary>
        public async Task<int> GetBPAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                int intValue = 0;
                if (_nodDiscounts?.TryGetInt32FieldQuickly("value", ref intValue) != true)
                    return _intBP;
                int intReturn = _intBP;
                if (await _nodDiscounts.RequirementsMetAsync(_objCharacter, token: token).ConfigureAwait(false))
                {
                    switch (await GetTypeAsync(token).ConfigureAwait(false))
                    {
                        case QualityType.Positive:
                            intReturn += intValue;
                            break;

                        case QualityType.Negative:
                            intReturn -= intValue;
                            break;
                    }
                }

                return intReturn;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Number of Build Points the Quality costs.
        /// </summary>
        public async Task SetBPAsync(int value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (Interlocked.Exchange(ref _intBP, value) != value)
                    await OnPropertyChangedAsync(nameof(BP), token).ConfigureAwait(false);
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

        /// <summary>
        /// The name of the object as it should be displayed in lists. Name (Extra).
        /// If there is more than one instance of the same quality, it's: Name (Extra) Number
        /// </summary>
        public string DisplayName(CultureInfo objCulture, string strLanguage)
        {
            using (LockObject.EnterReadLock())
            {
                string strReturn = DisplayNameShort(strLanguage);
                string strSpace = LanguageManager.GetString("String_Space", strLanguage);

                if (!string.IsNullOrEmpty(Extra))
                {
                    // Attempt to retrieve the CharacterAttribute name.
                    strReturn += strSpace + '(' + _objCharacter.TranslateExtra(Extra, strLanguage) + ')';
                }

                int intLevels = Levels;
                if (intLevels > 1)
                {
                    strReturn += strSpace + intLevels.ToString(objCulture);
                }
                else
                {
                    // Add a "1" to qualities that have levels, but for which we are only at level 1
                    XPathNavigator xmlDataNode = this.GetNodeXPath(strLanguage);
                    if (xmlDataNode != null && xmlDataNode.SelectSingleNodeAndCacheExpression("nolevels") == null)
                    {
                        XPathNavigator xmlMyLimitNode = null;
                        if (!_objCharacter.Created)
                            xmlMyLimitNode = xmlDataNode.SelectSingleNodeAndCacheExpression("chargenlimit");
                        if (xmlMyLimitNode == null)
                            xmlMyLimitNode = xmlDataNode.SelectSingleNodeAndCacheExpression("limit");
                        if (xmlMyLimitNode != null && int.TryParse(xmlMyLimitNode.Value, out int _))
                        {
                            strReturn += strSpace + intLevels.ToString(objCulture);
                        }
                    }
                }

                return strReturn;
            }
        }

        /// <summary>
        /// The name of the object as it should be displayed in lists. Name (Extra).
        /// If there is more than one instance of the same quality, it's: Name (Extra) Number
        /// </summary>
        public async Task<string> DisplayNameAsync(CultureInfo objCulture, string strLanguage, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                string strReturn = await DisplayNameShortAsync(strLanguage, token).ConfigureAwait(false);
                string strSpace = await LanguageManager.GetStringAsync("String_Space", strLanguage, token: token)
                                                       .ConfigureAwait(false);

                if (!string.IsNullOrEmpty(Extra))
                {
                    // Attempt to retrieve the CharacterAttribute name.
                    strReturn += strSpace + '(' + await _objCharacter
                                                        .TranslateExtraAsync(Extra, strLanguage, token: token)
                                                        .ConfigureAwait(false) + ')';
                }

                int intLevels = await GetLevelsAsync(token).ConfigureAwait(false);
                if (intLevels > 1)
                {
                    strReturn += strSpace + intLevels.ToString(objCulture);
                }
                else
                {
                    // Add a "1" to qualities that have levels, but for which we are only at level 1
                    XPathNavigator xmlDataNode
                        = await this.GetNodeXPathAsync(strLanguage, token: token).ConfigureAwait(false);
                    if (xmlDataNode != null && xmlDataNode.SelectSingleNodeAndCacheExpression("nolevels", token) == null)
                    {
                        XPathNavigator xmlMyLimitNode = null;
                        if (!await _objCharacter.GetCreatedAsync(token).ConfigureAwait(false))
                            xmlMyLimitNode = xmlDataNode.SelectSingleNodeAndCacheExpression("chargenlimit", token);
                        if (xmlMyLimitNode == null)
                            xmlMyLimitNode = xmlDataNode.SelectSingleNodeAndCacheExpression("limit", token);
                        if (xmlMyLimitNode != null && int.TryParse(xmlMyLimitNode.Value, out int _))
                        {
                            strReturn += strSpace + intLevels.ToString(objCulture);
                        }
                    }
                }

                return strReturn;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public string CurrentDisplayName => DisplayName(GlobalSettings.CultureInfo, GlobalSettings.Language);

        public Task<string> GetCurrentDisplayNameAsync(CancellationToken token = default) =>
            DisplayNameAsync(GlobalSettings.CultureInfo, GlobalSettings.Language, token);

        public string CurrentDisplayNameShort => DisplayNameShort(GlobalSettings.Language);

        public Task<string> GetCurrentDisplayNameShortAsync(CancellationToken token = default) =>
            DisplayNameShortAsync(GlobalSettings.Language, token);

        /// <summary>
        /// Returns how many instances of this quality there are in the character's quality list
        /// TODO: Actually implement a proper rating system for qualities that plays nice with the Improvements Manager.
        /// </summary>
        public int Levels
        {
            get
            {
                using (LockObject.EnterReadLock())
                {
                    return _objCharacter.Qualities.Count(objExistingQuality =>
                                                             objExistingQuality.SourceID == SourceID
                                                             && objExistingQuality.Extra == Extra &&
                                                             objExistingQuality.SourceName == SourceName
                                                             && objExistingQuality.Type == Type);
                }
            }
        }

        /// <summary>
        /// Returns how many instances of this quality there are in the character's quality list
        /// TODO: Actually implement a proper rating system for qualities that plays nice with the Improvements Manager.
        /// </summary>
        public async Task<int> GetLevelsAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                Guid guiMyId = SourceID;
                string strMyExtra = Extra;
                string strMySourceName = SourceName;
                QualityType eMyType = await GetTypeAsync(token).ConfigureAwait(false);
                return await _objCharacter.Qualities.CountAsync(async objExistingQuality =>
                    objExistingQuality.SourceID == guiMyId
                    && objExistingQuality.Extra == strMyExtra &&
                    objExistingQuality.SourceName == strMySourceName
                    && await objExistingQuality.GetTypeAsync(token).ConfigureAwait(false) == eMyType, token: token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Whether the Quality appears on the printouts.
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
        /// Whether the Qualitie's cost is doubled in Career Mode.
        /// </summary>
        public bool DoubleCost
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _blnDoubleCostCareer;
            }
            set
            {
                using (LockObject.EnterReadLock())
                {
                    if (_blnDoubleCostCareer == value)
                        return;
                }

                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (_blnDoubleCostCareer == value)
                        return;
                    using (LockObject.EnterWriteLock())
                        _blnDoubleCostCareer = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Whether the quality can be bought with free spell points instead
        /// </summary>
        public bool CanBuyWithSpellPoints
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _blnCanBuyWithSpellPoints;
            }
            set
            {
                using (LockObject.EnterReadLock())
                {
                    if (_blnCanBuyWithSpellPoints == value)
                        return;
                }

                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (_blnCanBuyWithSpellPoints == value)
                        return;
                    using (LockObject.EnterWriteLock())
                        _blnCanBuyWithSpellPoints = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Whether the Quality has been implemented completely, or needs additional code support.
        /// </summary>
        public bool Implemented
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _blnImplemented;
            }
            set
            {
                using (LockObject.EnterReadLock())
                {
                    if (_blnImplemented == value)
                        return;
                }

                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (_blnImplemented == value)
                        return;
                    using (LockObject.EnterWriteLock())
                        _blnImplemented = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Whether the Quality contributes towards the character's Quality BP limits.
        /// </summary>
        public bool ContributeToLimit
        {
            get
            {
                using (LockObject.EnterReadLock())
                {
                    if (OriginSource == QualitySource.Metatype
                        || OriginSource == QualitySource.MetatypeRemovable
                        || OriginSource == QualitySource.MetatypeRemovedAtChargen
                        || OriginSource == QualitySource.Heritage)
                        return false;

                    // Positive Metagenic Qualities are free if you're a Changeling.
                    if (Metagenic && _objCharacter.MetagenicLimit > 0)
                        return false;

                    // The Beast's Way and the Spiritual Way get the Mentor Spirit for free.
                    if (Name == "Mentor Spirit" && _objCharacter.Qualities.Any(
                            objQuality =>
                                objQuality.Name == "The Beast's Way" || objQuality.Name == "The Spiritual Way"))
                        return false;

                    return _blnContributeToLimit
                           && ImprovementManager
                              .GetCachedImprovementListForValueOf(_objCharacter,
                                                                  Improvement.ImprovementType.FreeQuality,
                                                                  SourceIDString).Count == 0
                           && ImprovementManager
                              .GetCachedImprovementListForValueOf(_objCharacter,
                                                                  Improvement.ImprovementType.FreeQuality, Name).Count
                           == 0;
                }
            }
            set
            {
                using (LockObject.EnterReadLock())
                {
                    if (_blnContributeToLimit == value)
                        return;
                }

                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (_blnContributeToLimit == value)
                        return;
                    using (LockObject.EnterWriteLock())
                        _blnContributeToLimit = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Whether the Quality contributes towards the character's Quality BP limits.
        /// </summary>
        public async Task<bool> GetContributeToLimitAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                QualitySource eQualitySource = await GetOriginSourceAsync(token).ConfigureAwait(false);
                if (eQualitySource == QualitySource.Metatype
                    || eQualitySource == QualitySource.MetatypeRemovable
                    || eQualitySource == QualitySource.MetatypeRemovedAtChargen
                    || eQualitySource == QualitySource.Heritage)
                    return false;

                // Positive Metagenic Qualities are free if you're a Changeling.
                if (await GetMetagenicAsync(token).ConfigureAwait(false) && await _objCharacter.GetMetagenicLimitAsync(token).ConfigureAwait(false) > 0)
                    return false;

                // The Beast's Way and the Spiritual Way get the Mentor Spirit for free.
                string strName = await GetNameAsync(token).ConfigureAwait(false);
                if (strName == "Mentor Spirit" && await _objCharacter.Qualities.AnyAsync(
                        async objQuality =>
                        {
                            string strInnerName = await objQuality.GetNameAsync(token).ConfigureAwait(false);
                            return strInnerName == "The Beast's Way" || strInnerName == "The Spiritual Way";
                        }, token: token).ConfigureAwait(false))
                    return false;

                return _blnContributeToLimit
                       && (await ImprovementManager
                           .GetCachedImprovementListForValueOfAsync(_objCharacter,
                               Improvement.ImprovementType.FreeQuality,
                               await GetSourceIDStringAsync(token).ConfigureAwait(false), token: token).ConfigureAwait(false)).Count == 0
                       && (await ImprovementManager
                           .GetCachedImprovementListForValueOfAsync(_objCharacter,
                               Improvement.ImprovementType.FreeQuality, strName, token: token).ConfigureAwait(false)).Count
                       == 0;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Whether the Quality contributes towards the character's Quality BP limits.
        /// </summary>
        public async Task SetContributeToLimitAsync(bool value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_blnContributeToLimit == value)
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
                if (_blnContributeToLimit == value)
                    return;
                IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    _blnContributeToLimit = value;
                }
                finally
                {
                    await objLocker2.DisposeAsync().ConfigureAwait(false);
                }

                await OnPropertyChangedAsync(nameof(ContributeToLimit), token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Whether the Quality contributes towards the character's Quality BP limits.
        /// </summary>
        public bool ContributeToMetagenicLimit
        {
            get
            {
                using (LockObject.EnterReadLock())
                {
                    if (OriginSource == QualitySource.Metatype || OriginSource == QualitySource.MetatypeRemovable
                                                               || OriginSource == QualitySource.MetatypeRemovedAtChargen
                                                               || OriginSource == QualitySource.Heritage)
                        return false;

                    return Metagenic && _objCharacter.MetagenicLimit > 0;
                }
            }
        }

        /// <summary>
        /// Whether the Quality contributes towards the character's Quality BP limits.
        /// </summary>
        public async Task<bool> GetContributeToMetagenicLimitAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                QualitySource eQualitySource = await GetOriginSourceAsync(token).ConfigureAwait(false);
                if (eQualitySource == QualitySource.Metatype
                    || eQualitySource == QualitySource.MetatypeRemovable
                    || eQualitySource == QualitySource.MetatypeRemovedAtChargen
                    || eQualitySource == QualitySource.Heritage)
                    return false;

                return await GetMetagenicAsync(token).ConfigureAwait(false) &&
                       await _objCharacter.GetMetagenicLimitAsync(token).ConfigureAwait(false) > 0;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Whether this quality can be purchased in stages, i.e. allowing the character to go into karmic debt
        /// </summary>
        public bool StagedPurchase
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _blnStagedPurchase;
            }
            set
            {
                using (LockObject.EnterReadLock())
                {
                    if (_blnStagedPurchase == value)
                        return;
                }

                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (_blnStagedPurchase == value)
                        return;
                    using (LockObject.EnterWriteLock())
                        _blnStagedPurchase = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Whether the Quality contributes towards the character's Total BP.
        /// </summary>
        public bool ContributeToBP
        {
            get
            {
                using (LockObject.EnterReadLock())
                {
                    if (OriginSource == QualitySource.Metatype || OriginSource == QualitySource.MetatypeRemovable
                                                               || OriginSource == QualitySource.Heritage)
                        return false;

                    // Positive Metagenic Qualities are free if you're a Changeling.
                    if (Metagenic && _objCharacter.MetagenicLimit > 0)
                        return false;

                    // The Beast's Way and the Spiritual Way get the Mentor Spirit for free.
                    if (Name == "Mentor Spirit" && _objCharacter.Qualities.Any(
                            objQuality =>
                                objQuality.Name == "The Beast's Way" || objQuality.Name == "The Spiritual Way"))
                        return false;

                    return _blnContributeToBP
                           && ImprovementManager
                              .GetCachedImprovementListForValueOf(_objCharacter,
                                                                  Improvement.ImprovementType.FreeQuality,
                                                                  SourceIDString).Count == 0
                           && ImprovementManager
                              .GetCachedImprovementListForValueOf(_objCharacter,
                                                                  Improvement.ImprovementType.FreeQuality, Name).Count
                           == 0;
                }
            }
        }

        /// <summary>
        /// Whether the Quality contributes towards the character's Total BP.
        /// </summary>
        public async Task<bool> GetContributeToBPAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                QualitySource eQualitySource = await GetOriginSourceAsync(token).ConfigureAwait(false);
                if (eQualitySource == QualitySource.Metatype
                    || eQualitySource == QualitySource.MetatypeRemovable
                    || eQualitySource == QualitySource.Heritage)
                    return false;

                if (await GetMetagenicAsync(token).ConfigureAwait(false) &&
                    await _objCharacter.GetMetagenicLimitAsync(token).ConfigureAwait(false) > 0)
                    return false;

                // The Beast's Way and the Spiritual Way get the Mentor Spirit for free.
                string strName = await GetNameAsync(token).ConfigureAwait(false);
                if (strName == "Mentor Spirit" && await _objCharacter.Qualities.AnyAsync(
                        async objQuality =>
                        {
                            string strInnerName = await objQuality.GetNameAsync(token).ConfigureAwait(false);
                            return strInnerName == "The Beast's Way" || strInnerName == "The Spiritual Way";
                        }, token: token).ConfigureAwait(false))
                    return false;

                return _blnContributeToBP
                       && (await ImprovementManager
                           .GetCachedImprovementListForValueOfAsync(_objCharacter,
                               Improvement.ImprovementType.FreeQuality,
                               await GetSourceIDStringAsync(token).ConfigureAwait(false), token: token).ConfigureAwait(false)).Count == 0
                       && (await ImprovementManager
                           .GetCachedImprovementListForValueOfAsync(_objCharacter,
                               Improvement.ImprovementType.FreeQuality, strName, token: token).ConfigureAwait(false)).Count
                       == 0;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        private string _strCachedNotes = string.Empty;

        /// <summary>
        /// Notes.
        /// </summary>
        public string Notes
        {
            get
            {
                using (LockObject.EnterReadLock())
                {
                    if (!string.IsNullOrEmpty(_strCachedNotes))
                        return _strCachedNotes;
                    string strCachedNotes = string.Empty;
                    if (Suppressed)
                    {
                        Improvement objDisablingImprovement
                            = ImprovementManager
                              .GetCachedImprovementListForValueOf(_objCharacter,
                                                                  Improvement.ImprovementType.DisableQuality,
                                                                  SourceIDString).FirstOrDefault()
                              ?? ImprovementManager
                                 .GetCachedImprovementListForValueOf(_objCharacter,
                                                                     Improvement.ImprovementType.DisableQuality, Name)
                                 .FirstOrDefault();
                        strCachedNotes += string.Format(GlobalSettings.CultureInfo,
                                                        LanguageManager.GetString("String_SuppressedBy"),
                                                        _objCharacter.GetObjectName(objDisablingImprovement)
                                                        ?? LanguageManager.GetString("String_Unknown"))
                                          + Environment.NewLine;
                    }

                    strCachedNotes += _strNotes;
                    return _strCachedNotes = strCachedNotes;
                }
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (Interlocked.Exchange(ref _strNotes, value) == value)
                        return;
                    _strCachedNotes = string.Empty;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Notes.
        /// </summary>
        public async Task<string> GetNotesAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (!string.IsNullOrEmpty(_strCachedNotes))
                    return _strCachedNotes;
                string strCachedNotes = string.Empty;
                if (Suppressed)
                {
                    Improvement objDisablingImprovement
                        = (await ImprovementManager
                              .GetCachedImprovementListForValueOfAsync(_objCharacter,
                                  Improvement.ImprovementType.DisableQuality,
                                  SourceIDString, token: token).ConfigureAwait(false)).FirstOrDefault()
                          ?? (await ImprovementManager
                              .GetCachedImprovementListForValueOfAsync(_objCharacter,
                                  Improvement.ImprovementType.DisableQuality, Name, token: token).ConfigureAwait(false))
                          .FirstOrDefault();
                    strCachedNotes += string.Format(GlobalSettings.CultureInfo,
                                          await LanguageManager.GetStringAsync("String_SuppressedBy", token: token)
                                              .ConfigureAwait(false),
                                          await _objCharacter.GetObjectNameAsync(objDisablingImprovement, token: token)
                                              .ConfigureAwait(false)
                                          ?? await LanguageManager.GetStringAsync("String_Unknown", token: token)
                                              .ConfigureAwait(false))
                                      + Environment.NewLine;
                }

                strCachedNotes += _strNotes;
                return _strCachedNotes = strCachedNotes;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Notes.
        /// </summary>
        public async Task SetNotesAsync(string value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                if (Interlocked.Exchange(ref _strNotes, value) == value)
                    return;
                _strCachedNotes = string.Empty;
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
                        _colNotes = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _intCachedSuppressed = -1;

        public bool Suppressed
        {
            get
            {
                using (LockObject.EnterReadLock())
                {
                    if (_intCachedSuppressed < 0)
                    {
                        _intCachedSuppressed = ImprovementManager
                                               .GetCachedImprovementListForValueOf(
                                                   _objCharacter, Improvement.ImprovementType.DisableQuality, Name)
                                               .Count
                                               + ImprovementManager
                                                 .GetCachedImprovementListForValueOf(
                                                     _objCharacter, Improvement.ImprovementType.DisableQuality,
                                                     SourceIDString).Count;
                    }

                    return _intCachedSuppressed > 0;
                }
            }
        }

        public async Task<bool> GetSuppressedAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_intCachedSuppressed < 0)
                {
                    _intCachedSuppressed = (await ImprovementManager
                                               .GetCachedImprovementListForValueOfAsync(
                                                   _objCharacter, Improvement.ImprovementType.DisableQuality,
                                                   await GetNameAsync(token).ConfigureAwait(false), token: token)
                                               .ConfigureAwait(false))
                                           .Count
                                           + (await ImprovementManager
                                               .GetCachedImprovementListForValueOfAsync(
                                                   _objCharacter, Improvement.ImprovementType.DisableQuality,
                                                   await GetSourceIDStringAsync(token).ConfigureAwait(false),
                                                   token: token).ConfigureAwait(false)).Count;
                }

                return _intCachedSuppressed > 0;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        private void RefreshSuppressed()
        {
            using (LockObject.EnterUpgradeableReadLock())
            {
                if (Suppressed)
                {
                    ImprovementManager.DisableImprovements(_objCharacter, _objCharacter.Improvements.Where(imp =>
                                                               imp.SourceName == SourceIDString && imp.Enabled));
                }
                else
                {
                    ImprovementManager.EnableImprovements(_objCharacter, _objCharacter.Improvements.Where(imp =>
                                                              imp.SourceName == SourceIDString && !imp.Enabled));
                }
            }
        }

        private async Task RefreshSuppressedAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                string strSourceIdString = await GetSourceIDStringAsync(token).ConfigureAwait(false);
                if (await GetSuppressedAsync(token).ConfigureAwait(false))
                {
                    await ImprovementManager.DisableImprovementsAsync(_objCharacter,
                        (await _objCharacter.GetImprovementsAsync(token).ConfigureAwait(false)).Where(imp =>
                            imp.SourceName == strSourceIdString && imp.Enabled), token).ConfigureAwait(false);
                }
                else
                {
                    await ImprovementManager.EnableImprovementsAsync(_objCharacter,
                        (await _objCharacter.GetImprovementsAsync(token).ConfigureAwait(false)).Where(imp =>
                            imp.SourceName == strSourceIdString && !imp.Enabled), token).ConfigureAwait(false);
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        private XmlNode _objCachedMyXmlNode;
        private string _strCachedXmlNodeLanguage = string.Empty;

        public async Task<XmlNode> GetNodeCoreAsync(bool blnSync, string strLanguage, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                XmlNode objReturn = _objCachedMyXmlNode;
                if (objReturn != null && strLanguage == _strCachedXmlNodeLanguage
                                      && !GlobalSettings.LiveCustomData)
                    return objReturn;
                XmlDocument objDoc = blnSync
                    // ReSharper disable once MethodHasAsyncOverload
                    ? _objCharacter.LoadData("qualities.xml", strLanguage, token: token)
                    : await _objCharacter.LoadDataAsync("qualities.xml", strLanguage, token: token)
                                         .ConfigureAwait(false);
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
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        private XPathNavigator _objCachedMyXPathNode;
        private string _strCachedXPathNodeLanguage = string.Empty;

        public async Task<XPathNavigator> GetNodeXPathCoreAsync(bool blnSync, string strLanguage, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                XPathNavigator objReturn = _objCachedMyXPathNode;
                if (objReturn != null && strLanguage == _strCachedXPathNodeLanguage
                                      && !GlobalSettings.LiveCustomData)
                    return objReturn;
                XPathNavigator objDoc = blnSync
                    // ReSharper disable once MethodHasAsyncOverload
                    ? _objCharacter.LoadDataXPath("qualities.xml", strLanguage, token: token)
                    : await _objCharacter.LoadDataXPathAsync("qualities.xml", strLanguage, token: token)
                                         .ConfigureAwait(false);
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
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        #endregion Properties

        #region UI Methods

        public TreeNode CreateTreeNode(ContextMenuStrip cmsQuality, TreeView treQualities)
        {
            using (LockObject.EnterReadLock())
            {
                if ((OriginSource == QualitySource.BuiltIn ||
                     OriginSource == QualitySource.Improvement ||
                     OriginSource == QualitySource.LifeModule ||
                     OriginSource == QualitySource.Metatype ||
                     OriginSource == QualitySource.MetatypeRemovable ||
                     OriginSource == QualitySource.MetatypeRemovedAtChargen ||
                     OriginSource == QualitySource.Heritage) && !string.IsNullOrEmpty(Source)
                                                             && !_objCharacter.Settings.BookEnabled(Source))
                    return null;

                TreeNode objNode = new TreeNode
                {
                    Name = InternalId,
                    Text = CurrentDisplayName,
                    Tag = this,
                    ContextMenuStrip = cmsQuality,
                    ForeColor = PreferredColor,
                    ToolTipText = Notes.WordWrap()
                };
                if (Suppressed)
                {
                    //Treenodes store their font as null when inheriting from the treeview; have to pull it from the treeview directly to set the fontstyle.
                    objNode.NodeFont = new Font(treQualities.Font, FontStyle.Strikeout);
                }

                return objNode;
            }
        }

        public Color PreferredColor
        {
            get
            {
                if (!Implemented)
                {
                    return ColorManager.ErrorColor;
                }
                if (!string.IsNullOrEmpty(Notes))
                {
                    return OriginSource == QualitySource.BuiltIn
                           || OriginSource == QualitySource.Improvement
                           || OriginSource == QualitySource.LifeModule
                           || OriginSource == QualitySource.Metatype
                           || OriginSource == QualitySource.Heritage
                        ? ColorManager.GenerateCurrentModeDimmedColor(NotesColor)
                        : ColorManager.GenerateCurrentModeColor(NotesColor);
                }
                return OriginSource == QualitySource.BuiltIn
                       || OriginSource == QualitySource.Improvement
                       || OriginSource == QualitySource.LifeModule
                       || OriginSource == QualitySource.Metatype
                       || OriginSource == QualitySource.Heritage
                    ? ColorManager.GrayText
                    : ColorManager.WindowText;
            }
        }

        #endregion UI Methods

        #region Static Methods

        /// <summary>
        /// Retuns weither a quality is valid on said Character
        /// THIS IS A WIP AND ONLY CHECKS QUALITIES. REQUIRED POWERS, METATYPES AND OTHERS WON'T BE CHECKED
        /// </summary>
        /// <param name="objCharacter">The Character</param>
        /// <param name="xmlQuality">The XmlNode describing the quality</param>
        /// <param name="token">Cancellation token to listen to.</param>
        /// <returns>Is the Quality valid on said Character</returns>
        public static bool IsValid(Character objCharacter, XmlNode xmlQuality, CancellationToken token = default)
        {
            return IsValid(objCharacter, xmlQuality, out QualityFailureReasons _, out List<Quality> _, token);
        }

        /// <summary>
        /// Returns whether a quality is valid on said Character
        /// THIS IS A WIP AND ONLY CHECKS QUALITIES. REQUIRED POWERS, METATYPES AND OTHERS WON'T BE CHECKED
        /// ConflictingQualities will only contain existing Qualities and won't contain required but missing Qualities
        /// </summary>
        /// <param name="objCharacter">The Character</param>
        /// <param name="objXmlQuality">The XmlNode describing the quality</param>
        /// <param name="reason">The reason the quality is not valid</param>
        /// <param name="conflictingQualities">List of Qualities that conflicts with this Quality</param>
        /// <param name="token">Cancellation token to listen to.</param>
        /// <returns>Is the Quality valid on said Character</returns>
        public static bool IsValid(Character objCharacter, XmlNode objXmlQuality, out QualityFailureReasons reason, out List<Quality> conflictingQualities, CancellationToken token = default)
        {
            if (objCharacter == null)
                throw new ArgumentNullException(nameof(objCharacter));
            using (objCharacter.LockObject.EnterReadLock(token))
            {
                conflictingQualities = new List<Quality>(objCharacter.Qualities.Count);
                reason = QualityFailureReasons.None;
                //If limit are not present or no, check if same quality exists
                string strTemp = string.Empty;
                if (!(objXmlQuality.TryGetStringFieldQuickly("limit", ref strTemp) && strTemp == bool.FalseString))
                {
                    foreach (Quality objQuality in objCharacter.Qualities)
                    {
                        token.ThrowIfCancellationRequested();
                        if (string.Equals(objQuality.SourceIDString, objXmlQuality["id"]?.InnerText, StringComparison.OrdinalIgnoreCase))
                        {
                            reason |= QualityFailureReasons
                                .LimitExceeded; //QualityFailureReason is a flag enum, meaning each bit represents a different thing
                            //So instead of changing it, |= adds rhs to list of reasons on lhs, if it is not present
                            conflictingQualities.Add(objQuality);
                        }
                    }
                }

                XmlElement xmlRequiredNode = objXmlQuality["required"];
                if (xmlRequiredNode != null)
                {
                    XmlElement xmlOneOfNode = xmlRequiredNode["oneof"];
                    if (xmlOneOfNode != null)
                    {
                        token.ThrowIfCancellationRequested();
                        //Add to set for O(N log M) runtime instead of O(N * M)
                        using (new FetchSafelyFromPool<HashSet<string>>(Utils.StringHashSetPool,
                                                                        out HashSet<string>
                                                                            lstRequired))
                        {
                            using (XmlNodeList xmlNodeList = xmlOneOfNode.SelectNodes("quality"))
                            {
                                if (xmlNodeList?.Count > 0)
                                {
                                    foreach (XmlNode node in xmlNodeList)
                                    {
                                        token.ThrowIfCancellationRequested();
                                        lstRequired.Add(node.InnerText);
                                    }
                                }
                            }

                            if (!objCharacter.Qualities.Any(quality => lstRequired.Contains(quality.Name), token))
                            {
                                reason |= QualityFailureReasons.RequiredSingle;
                            }
                        }

                        token.ThrowIfCancellationRequested();

                        reason |= QualityFailureReasons.MetatypeRequired;
                        using (XmlNodeList xmlNodeList = xmlOneOfNode.SelectNodes("metatype"))
                        {
                            if (xmlNodeList?.Count > 0)
                            {
                                foreach (XmlNode objNode in xmlNodeList)
                                {
                                    token.ThrowIfCancellationRequested();
                                    if (objNode.InnerText == objCharacter.Metatype)
                                    {
                                        reason &= ~QualityFailureReasons.MetatypeRequired;
                                        break;
                                    }
                                }
                            }
                        }
                    }

                    token.ThrowIfCancellationRequested();

                    XmlElement xmlAllOfNode = xmlRequiredNode["allof"];
                    if (xmlAllOfNode != null)
                    {
                        //Add to set for O(N log M) runtime instead of O(N * M)
                        using (new FetchSafelyFromPool<HashSet<string>>(Utils.StringHashSetPool,
                                                                        out HashSet<string>
                                                                            lstRequired))
                        {
                            foreach (Quality objQuality in objCharacter.Qualities)
                            {
                                token.ThrowIfCancellationRequested();
                                lstRequired.Add(objQuality.Name);
                            }

                            using (XmlNodeList xmlNodeList = xmlAllOfNode.SelectNodes("quality"))
                            {
                                if (xmlNodeList?.Count > 0)
                                {
                                    foreach (XmlNode node in xmlNodeList)
                                    {
                                        token.ThrowIfCancellationRequested();
                                        if (!lstRequired.Contains(node.InnerText))
                                        {
                                            reason |= QualityFailureReasons.RequiredMultiple;
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                token.ThrowIfCancellationRequested();

                XmlElement xmlForbiddenNode = objXmlQuality["forbidden"];
                if (xmlForbiddenNode != null)
                {
                    XmlElement xmlOneOfNode = xmlForbiddenNode["oneof"];
                    if (xmlOneOfNode != null)
                    {
                        //Add to set for O(N log M) runtime instead of O(N * M)
                        using (new FetchSafelyFromPool<HashSet<string>>(Utils.StringHashSetPool,
                                                                        out HashSet<string>
                                                                            setQualityForbidden))
                        {
                            using (XmlNodeList xmlNodeList = xmlOneOfNode.SelectNodes("quality"))
                            {
                                if (xmlNodeList != null)
                                {
                                    foreach (XmlNode node in xmlNodeList)
                                    {
                                        token.ThrowIfCancellationRequested();
                                        setQualityForbidden.Add(node.InnerText);
                                    }
                                }
                            }

                            foreach (Quality quality in objCharacter.Qualities)
                            {
                                token.ThrowIfCancellationRequested();
                                if (setQualityForbidden.Contains(quality.Name))
                                {
                                    reason |= QualityFailureReasons.ForbiddenSingle;
                                    conflictingQualities.Add(quality);
                                }
                            }
                        }
                    }
                }
            }

            return conflictingQualities.Count == 0 && reason == QualityFailureReasons.None;
        }

        /// <summary>
        /// This method builds a xmlNode upwards adding/overriding elements
        /// </summary>
        /// <param name="id">ID of the node</param>
        /// <param name="xmlDoc">XmlDocument containing the object with which to override this quality.</param>
        /// <returns>A XmlNode containing the id and all nodes of its parrents</returns>
        public static XmlNode GetNodeOverrideable(string id, XmlDocument xmlDoc)
        {
            if (xmlDoc == null)
                throw new ArgumentNullException(nameof(xmlDoc));
            XmlNode node = xmlDoc.TryGetNodeByNameOrId(".//*", id)
                           ?? throw new ArgumentException("Could not find node " + id + " in xmlDoc " + xmlDoc.Name
                                                          + '.');
            return GetNodeOverrideable(node);
        }

        private static XmlNode GetNodeOverrideable(XmlNode n)
        {
            XmlNode workNode = n.Clone();  //clone as to not mess up the acctual xml document

            XmlNode parentNode = n.SelectSingleNode("../..");
            if (parentNode?["id"] == null)
                return workNode;
            XmlNode sourceNode = GetNodeOverrideable(parentNode);
            if (sourceNode == null)
                return workNode;
            foreach (XmlNode node in sourceNode.ChildNodes)
            {
                if (workNode[node.LocalName] == null && node.LocalName != "versions")
                {
                    workNode.AppendChild(node.Clone());
                }
                else if (node.LocalName == "bonus")
                {
                    XmlElement xmlBonusNode = workNode["bonus"];
                    if (xmlBonusNode == null)
                        continue;
                    foreach (XmlNode childNode in node.ChildNodes)
                    {
                        xmlBonusNode.AppendChild(childNode.Clone());
                    }
                }
            }

            return workNode;
        }

        //A tree of dependencies. Once some of the properties are changed,
        //anything they depend on, also needs to raise OnChanged
        //This tree keeps track of dependencies
        private static readonly PropertyDependencyGraph<Quality> s_QualityDependencyGraph =
            new PropertyDependencyGraph<Quality>(
                new DependencyGraphNode<string, Quality>(nameof(Notes),
                    new DependencyGraphNode<string, Quality>(nameof(Suppressed))
                ),
                new DependencyGraphNode<string, Quality>(nameof(BP),
                    new DependencyGraphNode<string, Quality>(nameof(Type))
                ),
                new DependencyGraphNode<string, Quality>(nameof(ContributeToMetagenicLimit),
                    new DependencyGraphNode<string, Quality>(nameof(OriginSource)),
                    new DependencyGraphNode<string, Quality>(nameof(Metagenic))
                ),
                new DependencyGraphNode<string, Quality>(nameof(PreferredColor),
                    new DependencyGraphNode<string, Quality>(nameof(OriginSource))
                ),
                new DependencyGraphNode<string, Quality>(nameof(ContributeToBP),
                    new DependencyGraphNode<string, Quality>(nameof(OriginSource)),
                    new DependencyGraphNode<string, Quality>(nameof(Metagenic)),
                    new DependencyGraphNode<string, Quality>(nameof(Name))
                )
            );

        #endregion Static Methods

        /// <summary>
        /// Swaps an old quality for a new one.
        /// </summary>
        /// <param name="objOldQuality">Old quality that's being removed.</param>
        /// <param name="objXmlQuality">XML entry for the new quality.</param>
        /// <param name="intNewQualityRating">Rating of the new quality to add. All of the old quality's ratings will be removed</param>
        /// <param name="token">Cancellation token to listen to.</param>
        /// <returns></returns>
        public async Task<bool> Swap(Quality objOldQuality, XmlNode objXmlQuality, int intNewQualityRating,
                                          CancellationToken token = default)
        {
            if (objOldQuality == null)
                throw new ArgumentNullException(nameof(objOldQuality));
            IAsyncDisposable objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                // Helps to capture a write lock here for performance purposes
                IAsyncDisposable objLocker2 = await _objCharacter.LockObject.EnterWriteLockAsync(token)
                    .ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    List<Weapon> lstWeapons = new List<Weapon>(1);
                    await CreateAsync(objXmlQuality, QualitySource.Selected, lstWeapons, token: token).ConfigureAwait(false);

                    int intKarmaCost;
                    IAsyncDisposable objLocker3 = await objOldQuality.LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
                    try
                    {
                        token.ThrowIfCancellationRequested();
                        bool blnAddItem = true;
                        intKarmaCost = (await GetBPAsync(token).ConfigureAwait(false) * intNewQualityRating -
                                        await objOldQuality.GetBPAsync(token).ConfigureAwait(false) *
                                        await objOldQuality.GetLevelsAsync(token).ConfigureAwait(false))
                                       * await _objCharacter.Settings.GetKarmaQualityAsync(token).ConfigureAwait(false);

                        string strOldQualityName = await objOldQuality.GetCurrentDisplayNameShortAsync(token)
                            .ConfigureAwait(false);

                        // Make sure the character has enough Karma to pay for the Quality.
                        if (Type == QualityType.Positive)
                        {
                            if (await _objCharacter.GetCreatedAsync(token).ConfigureAwait(false)
                                && !_objCharacter.Settings.DontDoubleQualityPurchases)
                            {
                                intKarmaCost *= 2;
                            }

                            if (intKarmaCost > await _objCharacter.GetKarmaAsync(token).ConfigureAwait(false))
                            {
                                await Program.ShowScrollableMessageBoxAsync(
                                    await LanguageManager.GetStringAsync("Message_NotEnoughKarma", token: token)
                                        .ConfigureAwait(false),
                                    await LanguageManager.GetStringAsync(
                                        "MessageTitle_NotEnoughKarma", token: token).ConfigureAwait(false),
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Information, token: token).ConfigureAwait(false);
                                blnAddItem = false;
                            }

                            if (blnAddItem && !await CommonFunctions.ConfirmKarmaExpenseAsync(
                                        string.Format(GlobalSettings.CultureInfo,
                                            await LanguageManager
                                                .GetStringAsync("Message_QualitySwap",
                                                    token: token)
                                                .ConfigureAwait(false)
                                            , strOldQualityName
                                            , await GetCurrentDisplayNameShortAsync(token).ConfigureAwait(false)),
                                        token)
                                    .ConfigureAwait(false))
                            {
                                blnAddItem = false;
                            }
                        }
                        else
                        {
                            if (!_objCharacter.Settings.DontDoubleQualityRefunds)
                            {
                                intKarmaCost *= 2;
                            }

                            // This should only happen when a character is trading up to a less-costly Quality.
                            if (intKarmaCost > 0)
                            {
                                if (intKarmaCost > await _objCharacter.GetKarmaAsync(token).ConfigureAwait(false))
                                {
                                    await Program.ShowScrollableMessageBoxAsync(
                                        await LanguageManager.GetStringAsync("Message_NotEnoughKarma", token: token)
                                            .ConfigureAwait(false),
                                        await LanguageManager
                                            .GetStringAsync("MessageTitle_NotEnoughKarma", token: token)
                                            .ConfigureAwait(false),
                                        MessageBoxButtons.OK, MessageBoxIcon.Information, token: token).ConfigureAwait(false);
                                    blnAddItem = false;
                                }

                                if (blnAddItem && !await CommonFunctions.ConfirmKarmaExpenseAsync(
                                        string.Format(GlobalSettings.CultureInfo,
                                            await LanguageManager
                                                .GetStringAsync("Message_QualitySwap", token: token)
                                                .ConfigureAwait(false),
                                            strOldQualityName,
                                            await GetCurrentDisplayNameShortAsync(token)
                                                .ConfigureAwait(false)),
                                        token).ConfigureAwait(false))
                                {
                                    blnAddItem = false;
                                }
                            }
                            else
                            {
                                // Trading a more expensive quality for a less expensive quality shouldn't give you karma. TODO: Optional rule to govern this behaviour.
                                intKarmaCost = 0;
                            }
                        }

                        if (!blnAddItem)
                            return false;
                    }
                    finally
                    {
                        await objLocker3.DisposeAsync().ConfigureAwait(false);
                    }

                    // Removing the old quality from the character
                    await objOldQuality.DeleteQualityAsync(true, token).ConfigureAwait(false);

                    // Add the new Quality to the character.
                    await _objCharacter.Qualities.AddAsync(this, token).ConfigureAwait(false);

                    for (int i = 2; i <= intNewQualityRating; ++i)
                    {
                        Quality objNewQualityLevel = new Quality(_objCharacter);
                        try
                        {
                            await objNewQualityLevel.CreateAsync(objXmlQuality, QualitySource.Selected, lstWeapons, _strExtra,
                                _strSourceName, token).ConfigureAwait(false);
                            await _objCharacter.Qualities.AddAsync(objNewQualityLevel, token).ConfigureAwait(false);
                        }
                        catch
                        {
                            await objNewQualityLevel.DeleteQualityAsync(token: token).ConfigureAwait(false);
                            throw;
                        }
                    }

                    // Add any created Weapons to the character.
                    foreach (Weapon objWeapon in lstWeapons)
                    {
                        await _objCharacter.Weapons.AddAsync(objWeapon, token).ConfigureAwait(false);
                    }

                    if (await _objCharacter.GetCreatedAsync(token).ConfigureAwait(false))
                    {
                        // Create the Karma expense.
                        ExpenseLogEntry objExpense = new ExpenseLogEntry(_objCharacter);
                        objExpense.Create(intKarmaCost * -1,
                            string.Format(GlobalSettings.CultureInfo,
                                await LanguageManager.GetStringAsync(
                                        Type == QualityType.Positive
                                            ? "String_ExpenseSwapPositiveQuality"
                                            : "String_ExpenseSwapNegativeQuality",
                                        token: token)
                                    .ConfigureAwait(false)
                                , await objOldQuality.GetCurrentDisplayNameAsync(token)
                                    .ConfigureAwait(false)
                                , await GetCurrentDisplayNameAsync(token).ConfigureAwait(false)),
                            ExpenseType.Karma,
                            DateTime.Now);
                        await _objCharacter.ExpenseEntries.AddWithSortAsync(objExpense, token: token)
                            .ConfigureAwait(false);
                        await _objCharacter.ModifyKarmaAsync(-intKarmaCost, token).ConfigureAwait(false);
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

            await objOldQuality.DisposeAsync().ConfigureAwait(false);

            return true;
        }

        public void SetSourceDetail(Control sourceControl)
        {
            if (_objCachedSourceDetail.Language != GlobalSettings.Language)
                _objCachedSourceDetail = default;
            SourceDetail.SetControl(sourceControl);
        }

        public async Task SetSourceDetailAsync(Control sourceControl, CancellationToken token = default)
        {
            if (_objCachedSourceDetail.Language != GlobalSettings.Language)
                _objCachedSourceDetail = default;
            await (await GetSourceDetailAsync(token).ConfigureAwait(false)).SetControlAsync(sourceControl, token).ConfigureAwait(false);
        }

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
                                = s_QualityDependencyGraph.GetWithAllDependents(this, strPropertyName, true);
                        else
                        {
                            foreach (string strLoopChangedProperty in s_QualityDependencyGraph
                                         .GetWithAllDependentsEnumerable(this, strPropertyName))
                                setNamesOfChangedProperties.Add(strLoopChangedProperty);
                        }
                    }

                    if (setNamesOfChangedProperties == null || setNamesOfChangedProperties.Count == 0)
                        return;

                    if (lstPropertyNames.Contains(nameof(Suppressed)))
                    {
                        using (LockObject.EnterWriteLock())
                        {
                            _intCachedSuppressed = -1;
                            RefreshSuppressed();
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

        public async Task OnMultiplePropertiesChangedAsync(IReadOnlyCollection<string> lstPropertyNames, CancellationToken token = default)
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
                                = await s_QualityDependencyGraph.GetWithAllDependentsAsync(this, strPropertyName, true, token).ConfigureAwait(false);
                        else
                        {
                            foreach (string strLoopChangedProperty in await s_QualityDependencyGraph
                                         .GetWithAllDependentsEnumerableAsync(this, strPropertyName, token).ConfigureAwait(false))
                                setNamesOfChangedProperties.Add(strLoopChangedProperty);
                        }
                    }

                    if (setNamesOfChangedProperties == null || setNamesOfChangedProperties.Count == 0)
                        return;

                    if (lstPropertyNames.Contains(nameof(Suppressed)))
                    {
                        IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                        try
                        {
                            token.ThrowIfCancellationRequested();
                            _intCachedSuppressed = -1;
                            await RefreshSuppressedAsync(token).ConfigureAwait(false);
                        }
                        finally
                        {
                            await objLocker2.DisposeAsync().ConfigureAwait(false);
                        }
                    }

                    if (_setMultiplePropertiesChangedAsync.Count > 0)
                    {
                        MultiplePropertiesChangedEventArgs objArgs =
                            new MultiplePropertiesChangedEventArgs(setNamesOfChangedProperties.ToArray());
                        List<Task> lstTasks = new List<Task>(Utils.MaxParallelBatchSize);
                        int i = 0;
                        foreach (MultiplePropertiesChangedAsyncEventHandler objEvent in _setMultiplePropertiesChangedAsync)
                        {
                            lstTasks.Add(objEvent.Invoke(this, objArgs, token));
                            if (++i < Utils.MaxParallelBatchSize)
                                continue;
                            await Task.WhenAll(lstTasks).ConfigureAwait(false);
                            lstTasks.Clear();
                            i = 0;
                        }

                        await Task.WhenAll(lstTasks).ConfigureAwait(false);
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
                        List<Task> lstTasks =
                            new List<Task>(Math.Min(lstArgsList.Count * _setPropertyChangedAsync.Count,
                                Utils.MaxParallelBatchSize));
                        int i = 0;
                        foreach (PropertyChangedAsyncEventHandler objEvent in _setPropertyChangedAsync)
                        {
                            foreach (PropertyChangedEventArgs objArg in lstArgsList)
                            {
                                lstTasks.Add(objEvent.Invoke(this, objArg, token));
                                if (++i < Utils.MaxParallelBatchSize)
                                    continue;
                                await Task.WhenAll(lstTasks).ConfigureAwait(false);
                                lstTasks.Clear();
                                i = 0;
                            }
                        }

                        await Task.WhenAll(lstTasks).ConfigureAwait(false);
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

        /// <summary>
        /// Removes a quality from the character, assuming we have already gone through all the necessary UI prompts.
        /// TODO: make Quality properly inherit from ICanRemove by also putting the UI stuff in here as well
        /// </summary>
        /// <returns>Nuyen cost of the actual removal (necessary for removing some stuff that adds qualities as part of their effects).</returns>
        public decimal DeleteQuality(bool blnFullRemoval = false, CancellationToken token = default)
        {
            try
            {
                using (LockObject.EnterWriteLock(token))
                {
                    decimal decReturn = 0;
                    if (blnFullRemoval)
                    {
                        for (int i = _objCharacter.Qualities.Count - 1; i >= 0; --i)
                        {
                            token.ThrowIfCancellationRequested();
                            if (i >= _objCharacter.Qualities.Count)
                                continue;
                            Quality objLoopQuality = _objCharacter.Qualities[i];
                            if (objLoopQuality.SourceID == SourceID
                                && objLoopQuality.Extra == Extra
                                && objLoopQuality.SourceName == SourceName
                                && objLoopQuality.Type == Type
                                && !ReferenceEquals(objLoopQuality, this))
                                decReturn += objLoopQuality.DeleteQuality(token: token);
                        }
                    }

                    _objCharacter.Qualities.Remove(this);

                    // Remove the Improvements that were created by the Quality.
                    decReturn += ImprovementManager.RemoveImprovements(_objCharacter, Improvement.ImprovementSource.Quality,
                                                                      InternalId, token: token);

                    // Remove any Weapons created by the Quality if applicable.
                    if (!WeaponID.IsEmptyGuid())
                    {
                        token.ThrowIfCancellationRequested();
                        List<Weapon> lstWeapons = _objCharacter.Weapons.DeepWhere(x => x.Children, x => x.ParentID == InternalId, token).ToList();
                        foreach (Vehicle objVehicle in _objCharacter.Vehicles)
                        {
                            lstWeapons.AddRange(objVehicle.Weapons.DeepWhere(x => x.Children, x => x.ParentID == InternalId, token));
                            foreach (VehicleMod objMod in objVehicle.Mods)
                            {
                                lstWeapons.AddRange(objMod.Weapons.DeepWhere(x => x.Children, x => x.ParentID == InternalId, token));
                            }
                            foreach (WeaponMount objMount in objVehicle.WeaponMounts)
                            {
                                lstWeapons.AddRange(objMount.Weapons.DeepWhere(x => x.Children, x => x.ParentID == InternalId, token));
                                foreach (VehicleMod objMod in objMount.Mods)
                                {
                                    lstWeapons.AddRange(objMod.Weapons.DeepWhere(x => x.Children, x => x.ParentID == InternalId, token));
                                }
                            }
                        }

                        decReturn += lstWeapons.Sum(objDeleteWeapon => objDeleteWeapon.TotalCost + objDeleteWeapon.DeleteWeapon());
                    }

                    return decReturn;
                }
            }
            finally
            {
                Dispose();
            }
        }

        /// <summary>
        /// Removes a quality from the character, assuming we have already gone through all the necessary UI prompts.
        /// TODO: make Quality properly inherit from ICanRemove by also putting the UI stuff in here as well
        /// </summary>
        /// <returns>Nuyen cost of the actual removal (necessary for removing some stuff that adds qualities as part of their effects).</returns>
        public async Task<decimal> DeleteQualityAsync(bool blnFullRemoval = false,
                                                           CancellationToken token = default)
        {
            try
            {
                IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    decimal decReturn = 0;
                    if (blnFullRemoval)
                    {
                        for (int i = await _objCharacter.Qualities.GetCountAsync(token).ConfigureAwait(false) - 1; i >= 0; --i)
                        {
                            token.ThrowIfCancellationRequested();
                            if (i >= await _objCharacter.Qualities.GetCountAsync(token).ConfigureAwait(false))
                                continue;
                            Quality objLoopQuality = await _objCharacter.Qualities.GetValueAtAsync(i, token).ConfigureAwait(false);
                            if (objLoopQuality.SourceID == SourceID
                                && objLoopQuality.Extra == Extra
                                && objLoopQuality.SourceName == SourceName
                                && await objLoopQuality.GetTypeAsync(token).ConfigureAwait(false) == Type
                                && !ReferenceEquals(this, objLoopQuality))
                                decReturn += await objLoopQuality.DeleteQualityAsync(token: token).ConfigureAwait(false);
                        }
                    }

                    await _objCharacter.Qualities.RemoveAsync(this, token).ConfigureAwait(false);

                    // Remove the Improvements that were created by the Quality.
                    decReturn += await ImprovementManager
                                              .RemoveImprovementsAsync(_objCharacter,
                                                                       Improvement.ImprovementSource.Quality,
                                                                       InternalId, token).ConfigureAwait(false);

                    // Remove any Weapons created by the Quality if applicable.
                    if (!WeaponID.IsEmptyGuid())
                    {
                        List<Weapon> lstWeapons = await _objCharacter.Weapons
                            .DeepWhereAsync(x => x.Children, x => x.ParentID == InternalId, token).ConfigureAwait(false);
                        await _objCharacter.Vehicles.ForEachAsync(async objVehicle =>
                        {
                            lstWeapons.AddRange(await objVehicle.Weapons
                                .DeepWhereAsync(x => x.Children, x => x.ParentID == InternalId, token)
                                .ConfigureAwait(false));
                            await objVehicle.Mods.ForEachAsync(async objMod =>
                            {
                                lstWeapons.AddRange(await objMod.Weapons
                                    .DeepWhereAsync(x => x.Children, x => x.ParentID == InternalId, token)
                                    .ConfigureAwait(false));
                            }, token).ConfigureAwait(false);

                            await objVehicle.WeaponMounts.ForEachAsync(async objMount =>
                            {
                                lstWeapons.AddRange(await objMount.Weapons
                                    .DeepWhereAsync(x => x.Children, x => x.ParentID == InternalId, token)
                                    .ConfigureAwait(false));
                                await objMount.Mods.ForEachAsync(async objMod =>
                                {
                                    lstWeapons.AddRange(await objMod.Weapons
                                        .DeepWhereAsync(x => x.Children, x => x.ParentID == InternalId, token)
                                        .ConfigureAwait(false));
                                }, token).ConfigureAwait(false);
                            }, token).ConfigureAwait(false);
                        }, token).ConfigureAwait(false);

                        decReturn += await lstWeapons.SumAsync(async objDeleteWeapon =>
                                await objDeleteWeapon.GetTotalCostAsync(token).ConfigureAwait(false)
                                + await objDeleteWeapon.DeleteWeaponAsync(token: token).ConfigureAwait(false), token)
                            .ConfigureAwait(false);
                    }

                    return decReturn;
                }
                finally
                {
                    await objLocker.DisposeAsync().ConfigureAwait(false);
                }
            }
            finally
            {
                await DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            // No disposal necessary because our LockObject is our character owner's LockObject
        }

        /// <inheritdoc />
        public ValueTask DisposeAsync()
        {
            // No disposal necessary because our LockObject is our character owner's LockObject
            return default;
        }

        /// <inheritdoc />
        public AsyncFriendlyReaderWriterLock LockObject { get; }
    }
}
