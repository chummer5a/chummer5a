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
using Chummer.Backend.Attributes;

namespace Chummer.Backend.Uniques
{
    public enum TraditionType
    {
        None,
        MAG,
        RES
    }

    /// <summary>
    /// A Tradition
    /// </summary>
    [HubClassTag("SourceID", true, "Name", "Extra")]
    public sealed class Tradition : IHasInternalId, IHasName, IHasSourceId, IHasXmlDataNode, IHasSource, INotifyMultiplePropertyChanged, IHasLockObject
    {
        private Guid _guiID;
        private Guid _guiSourceID;
        private string _strName = string.Empty;
        private string _strExtra = string.Empty;
        private string _strSource = string.Empty;
        private string _strPage = string.Empty;
        private string _strDrainExpression = string.Empty;
        private string _strSpiritForm = "Materialization";
        private string _strSpiritCombat = string.Empty;
        private string _strSpiritDetection = string.Empty;
        private string _strSpiritHealth = string.Empty;
        private string _strSpiritIllusion = string.Empty;
        private string _strSpiritManipulation = string.Empty;
        private string _strNotes = string.Empty;
        private readonly List<string> _lstAvailableSpirits = new List<string>(5);
        private XmlNode _nodBonus;
        private TraditionType _eTraditionType = TraditionType.None;

        private readonly Character _objCharacter;

        #region Constructor, Create, Save, Load, and Print Methods

        public Tradition(Character objCharacter)
        {
            // Create the GUID for the new piece of Cyberware.
            _guiID = Guid.NewGuid();
            _objCharacter = objCharacter;
            if (objCharacter != null)
            {
                using (objCharacter.LockObject.EnterWriteLock())
                    objCharacter.PropertyChanged += RefreshDrainExpression;
            }
        }

        public override string ToString()
        {
            using (EnterReadLock.Enter(LockObject))
                return !string.IsNullOrEmpty(_strName) ? _strName : base.ToString();
        }

        /// <inheritdoc />
        public async ValueTask DisposeAsync()
        {
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync().ConfigureAwait(false);
            try
            {
                if (_objCharacter != null)
                {
                    try
                    {
                        IAsyncDisposable objLocker2
                            = await _objCharacter.LockObject.EnterWriteLockAsync().ConfigureAwait(false);
                        try
                        {
                            _objCharacter.PropertyChanged -= RefreshDrainExpression;
                        }
                        finally
                        {
                            await objLocker2.DisposeAsync().ConfigureAwait(false);
                        }
                    }
                    catch (ObjectDisposedException)
                    {
                        //swallow this
                    }
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }

            await LockObject.DisposeAsync().ConfigureAwait(false);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            using (LockObject.EnterWriteLock())
            {
                if (_objCharacter != null)
                {
                    try
                    {
                        using (_objCharacter.LockObject.EnterWriteLock())
                            _objCharacter.PropertyChanged -= RefreshDrainExpression;
                    }
                    catch (ObjectDisposedException)
                    {
                        //swallow this
                    }
                }
            }

            LockObject.Dispose();
        }

        public void ResetTradition()
        {
            using (LockObject.EnterWriteLock())
            {
                ImprovementManager.RemoveImprovements(_objCharacter, Improvement.ImprovementSource.Tradition,
                                                      InternalId);
                Bonus = null;
                Name = string.Empty;
                Extra = string.Empty;
                Source = string.Empty;
                _strPage = string.Empty;
                DrainExpression = string.Empty;
                SpiritForm = "Materialization";
                _lstAvailableSpirits.Clear();
                Type = TraditionType.None;
                _objCachedSourceDetail = default;
            }
        }

        public async ValueTask ResetTraditionAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                await ImprovementManager
                      .RemoveImprovementsAsync(_objCharacter, Improvement.ImprovementSource.Tradition, InternalId,
                                               token).ConfigureAwait(false);
                Bonus = null;
                Name = string.Empty;
                Extra = string.Empty;
                Source = string.Empty;
                _strPage = string.Empty;
                DrainExpression = string.Empty;
                SpiritForm = "Materialization";
                _lstAvailableSpirits.Clear();
                Type = TraditionType.None;
                _objCachedSourceDetail = default;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Create a Tradition from an XmlNode.
        /// </summary>
        /// <param name="xmlTraditionNode">XmlNode to create the object from.</param>
        /// <param name="blnIsTechnomancerTradition">Whether or not this tradition is for a technomancer.</param>
        /// <param name="strForcedValue">Value to forcefully select for any ImprovementManager prompts.</param>
        public bool Create(XmlNode xmlTraditionNode, bool blnIsTechnomancerTradition = false, string strForcedValue = "")
        {
            using (LockObject.EnterWriteLock())
            {
                ResetTradition();
                Type = blnIsTechnomancerTradition ? TraditionType.RES : TraditionType.MAG;
                if (xmlTraditionNode.TryGetField("id", Guid.TryParse, out _guiSourceID))
                {
                    _xmlCachedMyXmlNode = null;
                    _objCachedMyXPathNode = null;
                }

                xmlTraditionNode.TryGetStringFieldQuickly("name", ref _strName);
                xmlTraditionNode.TryGetStringFieldQuickly("source", ref _strSource);
                xmlTraditionNode.TryGetStringFieldQuickly("page", ref _strPage);
                string strTemp = string.Empty;
                if (xmlTraditionNode.TryGetStringFieldQuickly("drain", ref strTemp))
                    DrainExpression = strTemp;
                if (xmlTraditionNode.TryGetStringFieldQuickly("spiritform", ref strTemp))
                    SpiritForm = strTemp;
                _nodBonus = xmlTraditionNode["bonus"];
                if (_nodBonus != null)
                {
                    string strOldFocedValue = ImprovementManager.ForcedValue;
                    string strOldSelectedValue = ImprovementManager.SelectedValue;
                    ImprovementManager.ForcedValue = strForcedValue;
                    if (!ImprovementManager.CreateImprovements(_objCharacter, Improvement.ImprovementSource.Tradition,
                                                               InternalId, _nodBonus,
                                                               strFriendlyName: CurrentDisplayNameShort))
                    {
                        ImprovementManager.ForcedValue = strOldFocedValue;
                        return false;
                    }

                    if (!string.IsNullOrEmpty(ImprovementManager.SelectedValue))
                    {
                        _strExtra = ImprovementManager.SelectedValue;
                    }

                    ImprovementManager.ForcedValue = strOldFocedValue;
                    ImprovementManager.SelectedValue = strOldSelectedValue;
                }

                if (GlobalSettings.InsertPdfNotesIfAvailable && string.IsNullOrEmpty(Notes))
                {
                    Notes = CommonFunctions.GetBookNotes(xmlTraditionNode, Name, CurrentDisplayName, Source, Page,
                                                         DisplayPage(GlobalSettings.Language), _objCharacter);
                }

                RebuildSpiritList();
                this.OnMultiplePropertyChanged(nameof(Name), nameof(Extra), nameof(Source), nameof(Page));
                return true;
            }
        }

        public void RebuildSpiritList(bool blnDoOnPropertyChanged = true)
        {
            using (LockObject.EnterWriteLock())
            {
                _lstAvailableSpirits.Clear();
                _strSpiritCombat = string.Empty;
                _strSpiritDetection = string.Empty;
                _strSpiritHealth = string.Empty;
                _strSpiritIllusion = string.Empty;
                _strSpiritManipulation = string.Empty;
                if (Type != TraditionType.None)
                {
                    XPathNavigator xmlSpiritListNode
                        = this.GetNodeXPath()?.SelectSingleNodeAndCacheExpression("spirits");
                    if (xmlSpiritListNode != null)
                    {
                        foreach (XPathNavigator xmlSpiritNode in xmlSpiritListNode.SelectAndCacheExpression("spirit"))
                        {
                            _lstAvailableSpirits.Add(xmlSpiritNode.Value);
                        }

                        XPathNavigator xmlCombatSpiritNode
                            = xmlSpiritListNode.SelectSingleNodeAndCacheExpression("spiritcombat");
                        if (xmlCombatSpiritNode != null)
                            _strSpiritCombat = xmlCombatSpiritNode.Value;
                        XPathNavigator xmlDetectionSpiritNode
                            = xmlSpiritListNode.SelectSingleNodeAndCacheExpression("spiritdetection");
                        if (xmlDetectionSpiritNode != null)
                            _strSpiritDetection = xmlDetectionSpiritNode.Value;
                        XPathNavigator xmlHealthSpiritNode
                            = xmlSpiritListNode.SelectSingleNodeAndCacheExpression("spirithealth");
                        if (xmlHealthSpiritNode != null)
                            _strSpiritHealth = xmlHealthSpiritNode.Value;
                        XPathNavigator xmlIllusionSpiritNode
                            = xmlSpiritListNode.SelectSingleNodeAndCacheExpression("spiritillusion");
                        if (xmlIllusionSpiritNode != null)
                            _strSpiritIllusion = xmlIllusionSpiritNode.Value;
                        XPathNavigator xmlManipulationSpiritNode
                            = xmlSpiritListNode.SelectSingleNodeAndCacheExpression("spiritmanipulation");
                        if (xmlManipulationSpiritNode != null)
                            _strSpiritManipulation = xmlManipulationSpiritNode.Value;
                    }
                }

                if (blnDoOnPropertyChanged)
                {
                    this.OnMultiplePropertyChanged(nameof(AvailableSpirits), nameof(SpiritCombat),
                                                   nameof(SpiritDetection), nameof(SpiritHealth),
                                                   nameof(SpiritIllusion), nameof(SpiritManipulation));
                }
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
            using (EnterReadLock.Enter(LockObject))
            {
                if (_eTraditionType == TraditionType.None)
                    return;
                objWriter.WriteStartElement("tradition");
                objWriter.WriteElementString("sourceid", SourceIDString);
                objWriter.WriteElementString("guid", InternalId);
                objWriter.WriteElementString("traditiontype", _eTraditionType.ToString());
                objWriter.WriteElementString("name", _strName);
                objWriter.WriteElementString("extra", _strExtra);
                objWriter.WriteElementString("spiritform", _strSpiritForm);
                objWriter.WriteElementString("drain", _strDrainExpression);
                objWriter.WriteElementString("source", _strSource);
                objWriter.WriteElementString("page", _strPage);
                objWriter.WriteElementString("spiritcombat", _strSpiritCombat);
                objWriter.WriteElementString("spiritdetection", _strSpiritDetection);
                objWriter.WriteElementString("spirithealth", _strSpiritHealth);
                objWriter.WriteElementString("spiritillusion", _strSpiritIllusion);
                objWriter.WriteElementString("spiritmanipulation", _strSpiritManipulation);
                objWriter.WriteStartElement("spirits");
                foreach (string strSpirit in _lstAvailableSpirits)
                {
                    objWriter.WriteElementString("spirit", strSpirit);
                }

                objWriter.WriteEndElement();
                if (_nodBonus != null)
                    objWriter.WriteRaw(_nodBonus.OuterXml);
                else
                    objWriter.WriteElementString("bonus", string.Empty);
                objWriter.WriteEndElement();
            }
        }

        /// <summary>
        /// Load the Tradition from the XmlNode.
        /// </summary>
        /// <param name="xmlNode">XmlNode to load.</param>
        public void Load(XmlNode xmlNode)
        {
            using (LockObject.EnterWriteLock())
            {
                string strTemp = string.Empty;
                if (!xmlNode.TryGetStringFieldQuickly("traditiontype", ref strTemp)
                    || !Enum.TryParse(strTemp, out _eTraditionType))
                {
                    _eTraditionType = TraditionType.None;
                    return;
                }

                if (!xmlNode.TryGetField("guid", Guid.TryParse, out _guiID))
                {
                    _guiID = Guid.NewGuid();
                }

                xmlNode.TryGetStringFieldQuickly("name", ref _strName);
                Lazy<XPathNavigator> objMyNode = new Lazy<XPathNavigator>(() => this.GetNodeXPath());
                if (!xmlNode.TryGetGuidFieldQuickly("sourceid", ref _guiSourceID)
                    && !xmlNode.TryGetGuidFieldQuickly("id", ref _guiSourceID))
                {
                    objMyNode.Value?.TryGetGuidFieldQuickly("id", ref _guiSourceID);
                }

                xmlNode.TryGetStringFieldQuickly("extra", ref _strExtra);
                xmlNode.TryGetStringFieldQuickly("spiritform", ref _strSpiritForm);
                if (!xmlNode.TryGetStringFieldQuickly("drain", ref _strDrainExpression))
                    objMyNode.Value?.TryGetStringFieldQuickly("drain", ref _strDrainExpression);
                // Legacy catch for if a drain expression is not empty but has no attributes associated with it.
                if (_objCharacter.LastSavedVersion < new Version(5, 214, 77) &&
                    !string.IsNullOrEmpty(_strDrainExpression) && !_strDrainExpression.Contains('{') &&
                    AttributeSection.AttributeStrings.Any(x => _strDrainExpression.Contains(x)))
                {
                    if (IsCustomTradition)
                    {
                        foreach (string strAttribute in AttributeSection.AttributeStrings)
                            _strDrainExpression = _strDrainExpression.Replace(strAttribute, '{' + strAttribute + '}');
                        _strDrainExpression = _strDrainExpression.Replace("{MAG}Adept", "{MAGAdept}");
                    }
                    else
                        objMyNode.Value?.TryGetStringFieldQuickly("drain", ref _strDrainExpression);
                }

                xmlNode.TryGetStringFieldQuickly("source", ref _strSource);
                xmlNode.TryGetStringFieldQuickly("page", ref _strPage);
                xmlNode.TryGetStringFieldQuickly("spiritcombat", ref _strSpiritCombat);
                xmlNode.TryGetStringFieldQuickly("spiritdetection", ref _strSpiritDetection);
                xmlNode.TryGetStringFieldQuickly("spirithealth", ref _strSpiritHealth);
                xmlNode.TryGetStringFieldQuickly("spiritillusion", ref _strSpiritIllusion);
                xmlNode.TryGetStringFieldQuickly("spiritmanipulation", ref _strSpiritManipulation);
                using (XmlNodeList xmlSpiritList = xmlNode.SelectNodes("spirits/spirit"))
                {
                    if (xmlSpiritList?.Count > 0)
                    {
                        foreach (XmlNode xmlSpiritNode in xmlSpiritList)
                        {
                            _lstAvailableSpirits.Add(xmlSpiritNode.InnerText);
                        }
                    }
                }

                _nodBonus = xmlNode["bonus"];
            }
        }

        /// <summary>
        /// Load the Tradition from the XmlNode using old data saved before traditions had their own class.
        /// </summary>
        /// <param name="xpathCharacterNode">XPathNavigator of the Character from which to load.</param>
        public void LegacyLoad(XPathNavigator xpathCharacterNode)
        {
            using (LockObject.EnterWriteLock())
            {
                bool blnDoDrainSweep;
                if (_eTraditionType == TraditionType.RES)
                {
                    xpathCharacterNode.TryGetStringFieldQuickly("stream", ref _strName);
                    blnDoDrainSweep
                        = xpathCharacterNode.TryGetStringFieldQuickly("streamfading", ref _strDrainExpression);
                }
                else
                {
                    if (IsCustomTradition)
                    {
                        xpathCharacterNode.TryGetStringFieldQuickly("traditionname", ref _strName);
                        xpathCharacterNode.TryGetStringFieldQuickly("spiritcombat", ref _strSpiritCombat);
                        xpathCharacterNode.TryGetStringFieldQuickly("spiritdetection", ref _strSpiritDetection);
                        xpathCharacterNode.TryGetStringFieldQuickly("spirithealth", ref _strSpiritHealth);
                        xpathCharacterNode.TryGetStringFieldQuickly("spiritillusion", ref _strSpiritIllusion);
                        xpathCharacterNode.TryGetStringFieldQuickly("spiritmanipulation", ref _strSpiritManipulation);
                    }
                    else
                        xpathCharacterNode.TryGetStringFieldQuickly("tradition", ref _strName);

                    blnDoDrainSweep
                        = xpathCharacterNode.TryGetStringFieldQuickly("traditiondrain", ref _strDrainExpression);
                }

                if (blnDoDrainSweep)
                {
                    foreach (string strAttribute in AttributeSection.AttributeStrings)
                        _strDrainExpression = _strDrainExpression.Replace(strAttribute, '{' + strAttribute + '}');
                    _strDrainExpression = _strDrainExpression.Replace("{MAG}Adept", "{MAGAdept}");
                }
            }
        }

        public void LoadFromHeroLab(XPathNavigator xmlHeroLabNode)
        {
            if (xmlHeroLabNode == null)
                return;
            using (LockObject.EnterWriteLock())
            {
                _eTraditionType = TraditionType.MAG;
                _strName = xmlHeroLabNode.SelectSingleNodeAndCacheExpression("@name")?.Value;
                XmlNode xmlTraditionDataNode = !string.IsNullOrEmpty(_strName)
                    ? _objCharacter.LoadData("traditions.xml")
                                   .TryGetNodeByNameOrId("/chummer/traditions/tradition", _strName)
                    : null;
                if (xmlTraditionDataNode?.TryGetField("id", Guid.TryParse, out _guiSourceID) != true)
                {
                    _guiSourceID = new Guid(CustomMagicalTraditionGuid);
                    xmlTraditionDataNode = this.GetNode();
                }

                Create(xmlTraditionDataNode);
                if (IsCustomTradition)
                {
                    _strSpiritCombat = xmlHeroLabNode.SelectSingleNodeAndCacheExpression("@combatspirits")?.Value;
                    _strSpiritDetection = xmlHeroLabNode.SelectSingleNodeAndCacheExpression("@detectionspirits")?.Value;
                    _strSpiritHealth = xmlHeroLabNode.SelectSingleNodeAndCacheExpression("@healthspirits")?.Value;
                    _strSpiritIllusion = xmlHeroLabNode.SelectSingleNodeAndCacheExpression("@illusionspirits")?.Value;
                    _strSpiritManipulation = xmlHeroLabNode.SelectSingleNodeAndCacheExpression("@manipulationspirits")
                                                           ?.Value;
                }
            }
        }

        /// <summary>
        /// Print the object's XML to the XmlWriter.
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
                // <tradition>
                XmlElementWriteHelper objBaseElement
                    = await objWriter.StartElementAsync("tradition", token).ConfigureAwait(false);
                try
                {
                    await objWriter.WriteElementStringAsync("guid", InternalId, token).ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("sourceid", SourceIDString, token).ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("istechnomancertradition",
                                                            (Type == TraditionType.RES).ToString(
                                                                GlobalSettings.InvariantCultureInfo), token)
                                   .ConfigureAwait(false);
                    await objWriter
                          .WriteElementStringAsync(
                              "name", await DisplayNameShortAsync(strLanguageToPrint, token).ConfigureAwait(false),
                              token)
                          .ConfigureAwait(false);
                    await objWriter
                          .WriteElementStringAsync(
                              "fullname", await DisplayNameAsync(strLanguageToPrint, token).ConfigureAwait(false),
                              token)
                          .ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("name_english", Name, token).ConfigureAwait(false);
                    await objWriter
                          .WriteElementStringAsync(
                              "extra",
                              await _objCharacter.TranslateExtraAsync(Extra, strLanguageToPrint, token: token)
                                                 .ConfigureAwait(false), token).ConfigureAwait(false);
                    if (Type == TraditionType.MAG)
                    {
                        await objWriter
                              .WriteElementStringAsync("spiritcombat",
                                                       await DisplaySpiritCombatMethodAsync(strLanguageToPrint, token)
                                                           .ConfigureAwait(false), token).ConfigureAwait(false);
                        await objWriter
                              .WriteElementStringAsync("spiritdetection",
                                                       await DisplaySpiritDetectionMethodAsync(
                                                               strLanguageToPrint, token)
                                                           .ConfigureAwait(false), token).ConfigureAwait(false);
                        await objWriter
                              .WriteElementStringAsync("spirithealth",
                                                       await DisplaySpiritHealthMethodAsync(strLanguageToPrint, token)
                                                           .ConfigureAwait(false), token).ConfigureAwait(false);
                        await objWriter
                              .WriteElementStringAsync("spiritillusion",
                                                       await DisplaySpiritIllusionMethodAsync(strLanguageToPrint, token)
                                                           .ConfigureAwait(false), token).ConfigureAwait(false);
                        await objWriter
                              .WriteElementStringAsync("spiritmanipulation",
                                                       await DisplaySpiritManipulationMethodAsync(
                                                               strLanguageToPrint, token)
                                                           .ConfigureAwait(false), token).ConfigureAwait(false);
                        await objWriter
                              .WriteElementStringAsync("spiritform",
                                                       await DisplaySpiritFormAsync(strLanguageToPrint, token)
                                                           .ConfigureAwait(false), token).ConfigureAwait(false);
                    }

                    await objWriter
                          .WriteElementStringAsync("drainattributes",
                                                   await DisplayDrainExpressionMethodAsync(
                                                       objCulture, strLanguageToPrint, token).ConfigureAwait(false),
                                                   token)
                          .ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("drainvalue", DrainValue.ToString(objCulture), token)
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
                }
                finally
                {
                    // </tradition>
                    await objBaseElement.DisposeAsync().ConfigureAwait(false);
                }
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
                using (EnterReadLock.Enter(LockObject))
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
                using (EnterReadLock.Enter(LockObject))
                    return Type == TraditionType.None
                        ? string.Empty
                        : _guiSourceID.ToString("D", GlobalSettings.InvariantCultureInfo);
            }
        }

        /// <summary>
        /// Internal identifier which will be used to identify this Tradition in the Improvement system.
        /// </summary>
        public string InternalId
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                    return _guiID.ToString("D", GlobalSettings.InvariantCultureInfo);
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
        /// Bonus node from the XML file.
        /// </summary>
        public XmlNode Bonus
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                    return _nodBonus;
            }
            set
            {
                using (EnterReadLock.Enter(LockObject))
                {
                    if (Interlocked.Exchange(ref _nodBonus, value) == value)
                        return;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// ImprovementSource Type.
        /// </summary>
        public TraditionType Type
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                    return _eTraditionType;
            }
            set
            {
                using (EnterReadLock.Enter(LockObject))
                {
                    if (InterlockedExtensions.Exchange(ref _eTraditionType, value) == value)
                        return;
                    using (LockObject.EnterWriteLock())
                    {
                        _xmlCachedMyXmlNode = null;
                        _objCachedMyXPathNode = null;
                    }
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// The GUID of the Custom entry in the Magical Tradition file
        /// </summary>
        public const string CustomMagicalTraditionGuid = "616ba093-306c-45fc-8f41-0b98c8cccb46";

        /// <summary>
        /// Whether or not a Tradition is a custom one (i.e. it has a custom name and custom spirit settings)
        /// </summary>
        public bool IsCustomTradition
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                    return string.Equals(SourceIDString, CustomMagicalTraditionGuid, StringComparison.OrdinalIgnoreCase);
                // TODO: If Custom Technomancer Tradition added to streams.xml, check for that GUID as well
            }
        }

        public bool CanChooseDrainAttribute
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                    return IsCustomTradition || string.IsNullOrEmpty(_strDrainExpression);
            }
        }

        /// <summary>
        /// Tradition name.
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
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// The name of the object as it should be displayed on printouts (translated name only).
        /// </summary>
        public string DisplayNameShort(string strLanguage)
        {
            using (EnterReadLock.Enter(LockObject))
            {
                if (IsCustomTradition)
                {
                    if (GlobalSettings.Language != strLanguage)
                    {
                        string strFile = string.Empty;
                        switch (Type)
                        {
                            case TraditionType.MAG:
                                strFile = "traditions.xml";
                                break;

                            case TraditionType.RES:
                                strFile = "streams.xml";
                                break;
                        }

                        string strReturnEnglish
                            = strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase)
                                ? Name
                                : _objCharacter.ReverseTranslateExtra(Name, GlobalSettings.DefaultLanguage, strFile);
                        return _objCharacter.TranslateExtra(strReturnEnglish, strLanguage);
                    }

                    return _objCharacter.TranslateExtra(Name, strLanguage);
                }

                // Get the translated name if applicable.
                if (strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                    return Name;

                return this.GetNodeXPath(strLanguage)?.SelectSingleNodeAndCacheExpression("translate")?.Value ?? Name;
            }
        }

        /// <summary>
        /// The name of the object as it should be displayed on printouts (translated name only).
        /// </summary>
        public async ValueTask<string> DisplayNameShortAsync(string strLanguage, CancellationToken token = default)
        {
            using (await EnterReadLock.EnterAsync(LockObject, token).ConfigureAwait(false))
            {
                if (IsCustomTradition)
                {
                    if (GlobalSettings.Language != strLanguage)
                    {
                        string strFile = string.Empty;
                        switch (Type)
                        {
                            case TraditionType.MAG:
                                strFile = "traditions.xml";
                                break;

                            case TraditionType.RES:
                                strFile = "streams.xml";
                                break;
                        }

                        string strReturnEnglish
                            = strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase)
                                ? Name
                                : await _objCharacter.ReverseTranslateExtraAsync(
                                    Name, GlobalSettings.DefaultLanguage, strFile, token).ConfigureAwait(false);
                        return await _objCharacter.TranslateExtraAsync(strReturnEnglish, strLanguage, token: token)
                                                  .ConfigureAwait(false);
                    }

                    return await _objCharacter.TranslateExtraAsync(Name, strLanguage, token: token)
                                              .ConfigureAwait(false);
                }

                // Get the translated name if applicable.
                if (strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                    return Name;

                XPathNavigator objNode = await this.GetNodeXPathAsync(strLanguage, token: token).ConfigureAwait(false);
                return objNode != null
                    ? (await objNode.SelectSingleNodeAndCacheExpressionAsync("translate", token: token)
                                    .ConfigureAwait(false))?.Value ?? Name
                    : Name;
            }
        }

        /// <summary>
        /// The name of the object as it should be displayed in lists. Name (Extra).
        /// </summary>
        public string DisplayName(string strLanguage)
        {
            using (EnterReadLock.Enter(LockObject))
            {
                string strReturn = DisplayNameShort(strLanguage);

                if (!string.IsNullOrEmpty(Extra))
                    strReturn += LanguageManager.GetString("String_Space", strLanguage) + '('
                        + _objCharacter.TranslateExtra(Extra, strLanguage) + ')';

                return strReturn;
            }
        }

        /// <summary>
        /// The name of the object as it should be displayed in lists. Name (Extra).
        /// </summary>
        public async ValueTask<string> DisplayNameAsync(string strLanguage, CancellationToken token = default)
        {
            using (await EnterReadLock.EnterAsync(LockObject, token).ConfigureAwait(false))
            {
                string strReturn = await DisplayNameShortAsync(strLanguage, token).ConfigureAwait(false);

                if (!string.IsNullOrEmpty(Extra))
                    strReturn
                        += await LanguageManager.GetStringAsync("String_Space", strLanguage, token: token)
                                                .ConfigureAwait(false) + '(' + await _objCharacter
                            .TranslateExtraAsync(Extra, strLanguage, token: token).ConfigureAwait(false) + ')';

                return strReturn;
            }
        }

        public string CurrentDisplayName => DisplayName(GlobalSettings.Language);

        public ValueTask<string> GetCurrentDisplayNameAsync(CancellationToken token = default) => DisplayNameAsync(GlobalSettings.Language, token);

        public string CurrentDisplayNameShort => DisplayNameShort(GlobalSettings.Language);

        public ValueTask<string> GetCurrentDisplayNameShortAsync(CancellationToken token = default) => DisplayNameShortAsync(GlobalSettings.Language, token);

        /// <summary>
        /// What type of forms do spirits of these traditions come in? Defaults to Materialization.
        /// </summary>
        public string SpiritForm
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                    return _strSpiritForm;
            }
            set
            {
                value = _objCharacter.ReverseTranslateExtra(value);
                using (EnterReadLock.Enter(LockObject))
                {
                    if (Interlocked.Exchange(ref _strSpiritForm, value) == value)
                        return;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// The spirit form of the tradition as it should be displayed in printouts and the UI.
        /// </summary>
        public string DisplaySpiritForm(string strLanguage)
        {
            return _objCharacter.TranslateExtra(SpiritForm, strLanguage, "critterpowers.xml");
        }

        /// <summary>
        /// The spirit form of the tradition as it should be displayed in printouts and the UI.
        /// </summary>
        public Task<string> DisplaySpiritFormAsync(string strLanguage, CancellationToken token = default)
        {
            return _objCharacter.TranslateExtraAsync(SpiritForm, strLanguage, "critterpowers.xml", token);
        }

        /// <summary>
        /// Value that was selected during an ImprovementManager dialogue.
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
                value = _objCharacter.ReverseTranslateExtra(value);
                using (EnterReadLock.Enter(LockObject))
                {
                    if (Interlocked.Exchange(ref _strExtra, value) == value)
                        return;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Magician's Tradition Drain Attributes.
        /// </summary>
        public string DrainExpression
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                {
                    if (_objCharacter.AdeptEnabled && !_objCharacter.MagicianEnabled)
                    {
                        return "{BOD} + {WIL}";
                    }

                    return _strDrainExpression;
                }
            }
            set
            {
                using (EnterReadLock.Enter(LockObject))
                {
                    using (EnterReadLock.Enter(_objCharacter))
                    using (EnterReadLock.Enter(_objCharacter.AttributeSection))
                    using (EnterReadLock.Enter(_objCharacter.AttributeSection.Attributes))
                    {
                        _objCharacter.AttributeSection.Attributes.ForEach(x => x.LockObject.EnterReadLock());
                        try
                        {
                            string strOldExpression = Interlocked.Exchange(ref _strDrainExpression, value);
                            if (strOldExpression == value)
                                return;
                            foreach (string strAttribute in AttributeSection.AttributeStrings)
                            {
                                if (strOldExpression.Contains(strAttribute))
                                {
                                    if (!value.Contains(strAttribute))
                                    {
                                        CharacterAttrib objAttrib = _objCharacter.GetAttribute(strAttribute);
                                        using (objAttrib.LockObject.EnterWriteLock())
                                            objAttrib.PropertyChanged -= RefreshDrainValue;
                                    }
                                }
                                else if (value.Contains(strAttribute))
                                {
                                    CharacterAttrib objAttrib = _objCharacter.GetAttribute(strAttribute);
                                    using (objAttrib.LockObject.EnterWriteLock())
                                        objAttrib.PropertyChanged += RefreshDrainValue;
                                }
                            }
                        }
                        finally
                        {
                            _objCharacter.AttributeSection.Attributes.ForEach(x => x.LockObject.ExitReadLock());
                        }
                    }

                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Magician's Tradition Drain Attributes for display purposes.
        /// </summary>
        public string DisplayDrainExpression => DisplayDrainExpressionMethod(GlobalSettings.CultureInfo, GlobalSettings.Language);

        /// <summary>
        /// Magician's Tradition Drain Attributes for display purposes.
        /// </summary>
        public string DisplayDrainExpressionMethod(CultureInfo objCultureInfo, string strLanguage)
        {
            return _objCharacter.AttributeSection.ProcessAttributesInXPathForTooltip(DrainExpression, objCultureInfo, strLanguage, false);
        }

        /// <summary>
        /// Magician's Tradition Drain Attributes for display purposes.
        /// </summary>
        public ValueTask<string> DisplayDrainExpressionMethodAsync(CultureInfo objCultureInfo, string strLanguage, CancellationToken token = default)
        {
            return _objCharacter.AttributeSection.ProcessAttributesInXPathForTooltipAsync(DrainExpression, objCultureInfo, strLanguage, false, token: token);
        }

        /// <summary>
        /// Magician's total amount of dice for resisting drain.
        /// </summary>
        public int DrainValue
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                {
                    if (Type == TraditionType.None)
                        return 0;
                    string strDrainAttributes = DrainExpression;
                    string strDrain;
                    using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool,
                                                                  out StringBuilder sbdDrain))
                    {
                        sbdDrain.Append(strDrainAttributes);
                        _objCharacter.AttributeSection.ProcessAttributesInXPath(sbdDrain, strDrainAttributes);
                        strDrain = sbdDrain.ToString();
                    }

                    if (!decimal.TryParse(strDrain, out decimal decDrain))
                    {
                        (bool blnIsSuccess, object objProcess) = CommonFunctions.EvaluateInvariantXPath(strDrain);
                        if (blnIsSuccess)
                            decDrain = Convert.ToDecimal(objProcess, GlobalSettings.InvariantCultureInfo);
                    }

                    // Add any Improvements for Drain Resistance.
                    if (Type == TraditionType.RES)
                        decDrain += ImprovementManager.ValueOf(_objCharacter,
                                                               Improvement.ImprovementType.FadingResistance);
                    else
                        decDrain += ImprovementManager.ValueOf(_objCharacter,
                                                               Improvement.ImprovementType.DrainResistance);

                    return decDrain.StandardRound();
                }
            }
        }

        public string DrainValueToolTip
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                {
                    if (Type == TraditionType.None)
                        return string.Empty;
                    string strSpace = LanguageManager.GetString("String_Space");
                    using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool,
                                                                  out StringBuilder sbdToolTip))
                    {
                        sbdToolTip.Append(DrainExpression);
                        // Update the Fading CharacterAttribute Value.
                        _objCharacter.AttributeSection.ProcessAttributesInXPathForTooltip(sbdToolTip, DrainExpression);

                        List<Improvement> lstUsedImprovements
                            = ImprovementManager.GetCachedImprovementListForValueOf(
                                _objCharacter,
                                Type == TraditionType.RES
                                    ? Improvement.ImprovementType.FadingResistance
                                    : Improvement.ImprovementType.DrainResistance);
                        foreach (Improvement objLoopImprovement in lstUsedImprovements)
                        {
                            sbdToolTip.Append(strSpace).Append('+').Append(strSpace)
                                      .Append(_objCharacter.GetObjectName(objLoopImprovement)).Append(strSpace)
                                      .Append('(')
                                      .Append(objLoopImprovement.Value.ToString(GlobalSettings.CultureInfo))
                                      .Append(')');
                        }

                        return sbdToolTip.ToString();
                    }
                }
            }
        }

        public void RefreshDrainExpression(object sender, PropertyChangedEventArgs e)
        {
            if (Type == TraditionType.MAG && (e?.PropertyName == nameof(Character.AdeptEnabled) || e?.PropertyName == nameof(Character.MagicianEnabled)))
                OnPropertyChanged(nameof(DrainExpression));
        }

        public void RefreshDrainValue(object sender, PropertyChangedEventArgs e)
        {
            if (Type != TraditionType.None && e?.PropertyName == nameof(CharacterAttrib.TotalValue))
                OnPropertyChanged(nameof(DrainValue));
        }

        public IReadOnlyList<string> AvailableSpirits => _lstAvailableSpirits;

        /// <summary>
        /// Magician's Combat Spirit (for Custom Traditions) in English.
        /// </summary>
        public string SpiritCombat
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                    return Type == TraditionType.None ? string.Empty : _strSpiritCombat;
            }
            set
            {
                using (EnterReadLock.Enter(LockObject))
                {
                    if (Type != TraditionType.None && Interlocked.Exchange(ref _strSpiritCombat, value) != value)
                        OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Method to get Magician's Combat Spirit (for Custom Traditions) in a language.
        /// </summary>
        public string DisplaySpiritCombatMethod(string strLanguage)
        {
            string strSpirit = SpiritCombat;
            return string.IsNullOrEmpty(strSpirit)
                ? LanguageManager.GetString("String_None", strLanguage)
                : _objCharacter.TranslateExtra(strSpirit, strLanguage, "critters.xml");
        }

        /// <summary>
        /// Method to get Magician's Combat Spirit (for Custom Traditions) in a language.
        /// </summary>
        public Task<string> DisplaySpiritCombatMethodAsync(string strLanguage, CancellationToken token = default)
        {
            string strSpirit = SpiritCombat;
            return string.IsNullOrEmpty(strSpirit)
                ? LanguageManager.GetStringAsync("String_None", strLanguage, token: token)
                : _objCharacter.TranslateExtraAsync(strSpirit, strLanguage, "critters.xml", token);
        }

        /// <summary>
        /// Magician's Combat Spirit (for Custom Traditions) in the language of the current UI.
        /// </summary>
        public string DisplaySpiritCombat
        {
            get => DisplaySpiritCombatMethod(GlobalSettings.Language);
            set
            {
                using (EnterReadLock.Enter(LockObject))
                {
                    if (Type != TraditionType.None)
                    {
                        SpiritCombat = _objCharacter.ReverseTranslateExtra(value, GlobalSettings.Language, "critters.xml");
                    }
                }
            }
        }

        /// <summary>
        /// Magician's Detection Spirit (for Custom Traditions).
        /// </summary>
        public string SpiritDetection
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                    return Type == TraditionType.None ? string.Empty : _strSpiritDetection;
            }
            set
            {
                using (EnterReadLock.Enter(LockObject))
                {
                    if (Type != TraditionType.None && Interlocked.Exchange(ref _strSpiritDetection, value) != value)
                        OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Method to get Magician's Detection Spirit (for Custom Traditions) in a language.
        /// </summary>
        public string DisplaySpiritDetectionMethod(string strLanguage)
        {
            string strSpirit = SpiritDetection;
            return string.IsNullOrEmpty(strSpirit)
                ? LanguageManager.GetString("String_None", strLanguage)
                : _objCharacter.TranslateExtra(strSpirit, strLanguage, "critters.xml");
        }

        /// <summary>
        /// Method to get Magician's Detection Spirit (for Custom Traditions) in a language.
        /// </summary>
        public Task<string> DisplaySpiritDetectionMethodAsync(string strLanguage, CancellationToken token = default)
        {
            string strSpirit = SpiritDetection;
            return string.IsNullOrEmpty(strSpirit)
                ? LanguageManager.GetStringAsync("String_None", strLanguage, token: token)
                : _objCharacter.TranslateExtraAsync(strSpirit, strLanguage, "critters.xml", token);
        }

        /// <summary>
        /// Magician's Detection Spirit (for Custom Traditions) in the language of the current UI.
        /// </summary>
        public string DisplaySpiritDetection
        {
            get => DisplaySpiritDetectionMethod(GlobalSettings.Language);
            set
            {
                using (EnterReadLock.Enter(LockObject))
                {
                    if (Type != TraditionType.None)
                        SpiritDetection
                            = _objCharacter.ReverseTranslateExtra(value, GlobalSettings.Language, "critters.xml");
                }
            }
        }

        /// <summary>
        /// Magician's Health Spirit (for Custom Traditions).
        /// </summary>
        public string SpiritHealth
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                    return Type == TraditionType.None ? string.Empty : _strSpiritHealth;
            }
            set
            {
                using (EnterReadLock.Enter(LockObject))
                {
                    if (Type != TraditionType.None && Interlocked.Exchange(ref _strSpiritHealth, value) != value)
                        OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Method to get Magician's Health Spirit (for Custom Traditions) in a language.
        /// </summary>
        public string DisplaySpiritHealthMethod(string strLanguage)
        {
            string strSpirit = SpiritHealth;
            return string.IsNullOrEmpty(strSpirit)
                ? LanguageManager.GetString("String_None", strLanguage)
                : _objCharacter.TranslateExtra(strSpirit, strLanguage, "critters.xml");
        }

        /// <summary>
        /// Method to get Magician's Health Spirit (for Custom Traditions) in a language.
        /// </summary>
        public Task<string> DisplaySpiritHealthMethodAsync(string strLanguage, CancellationToken token = default)
        {
            string strSpirit = SpiritHealth;
            return string.IsNullOrEmpty(strSpirit)
                ? LanguageManager.GetStringAsync("String_None", strLanguage, token: token)
                : _objCharacter.TranslateExtraAsync(strSpirit, strLanguage, "critters.xml", token);
        }

        /// <summary>
        /// Magician's Health Spirit (for Custom Traditions) in the language of the current UI.
        /// </summary>
        public string DisplaySpiritHealth
        {
            get => DisplaySpiritHealthMethod(GlobalSettings.Language);
            set
            {
                using (EnterReadLock.Enter(LockObject))
                {
                    if (Type != TraditionType.None)
                        SpiritHealth
                            = _objCharacter.ReverseTranslateExtra(value, GlobalSettings.Language, "critters.xml");
                }
            }
        }

        /// <summary>
        /// Magician's Illusion Spirit (for Custom Traditions).
        /// </summary>
        public string SpiritIllusion
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                    return Type == TraditionType.None ? string.Empty : _strSpiritIllusion;
            }
            set
            {
                using (EnterReadLock.Enter(LockObject))
                {
                    if (Type != TraditionType.None && Interlocked.Exchange(ref _strSpiritIllusion, value) != value)
                        OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Method to get Magician's Illusion Spirit (for Custom Traditions) in a language.
        /// </summary>
        public string DisplaySpiritIllusionMethod(string strLanguage)
        {
            string strSpirit = SpiritIllusion;
            return string.IsNullOrEmpty(strSpirit)
                ? LanguageManager.GetString("String_None", strLanguage)
                : _objCharacter.TranslateExtra(strSpirit, strLanguage, "critters.xml");
        }

        /// <summary>
        /// Method to get Magician's Illusion Spirit (for Custom Traditions) in a language.
        /// </summary>
        public Task<string> DisplaySpiritIllusionMethodAsync(string strLanguage, CancellationToken token = default)
        {
            string strSpirit = SpiritIllusion;
            return string.IsNullOrEmpty(strSpirit)
                ? LanguageManager.GetStringAsync("String_None", strLanguage, token: token)
                : _objCharacter.TranslateExtraAsync(strSpirit, strLanguage, "critters.xml", token);
        }

        /// <summary>
        /// Magician's Illusion Spirit (for Custom Traditions) in the language of the current UI.
        /// </summary>
        public string DisplaySpiritIllusion
        {
            get => DisplaySpiritIllusionMethod(GlobalSettings.Language);
            set
            {
                using (EnterReadLock.Enter(LockObject))
                {
                    if (Type != TraditionType.None)
                        SpiritIllusion
                            = _objCharacter.ReverseTranslateExtra(value, GlobalSettings.Language, "critters.xml");
                }
            }
        }

        /// <summary>
        /// Magician's Manipulation Spirit (for Custom Traditions).
        /// </summary>
        public string SpiritManipulation
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                    return Type == TraditionType.None ? string.Empty : _strSpiritManipulation;
            }
            set
            {
                using (EnterReadLock.Enter(LockObject))
                {
                    if (Type != TraditionType.None && Interlocked.Exchange(ref _strSpiritManipulation, value) != value)
                        OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Method to get Magician's Manipulation Spirit (for Custom Traditions) in a language.
        /// </summary>
        public string DisplaySpiritManipulationMethod(string strLanguage)
        {
            string strSpirit = SpiritManipulation;
            return string.IsNullOrEmpty(strSpirit)
                ? LanguageManager.GetString("String_None", strLanguage)
                : _objCharacter.TranslateExtra(strSpirit, strLanguage, "critters.xml");
        }

        /// <summary>
        /// Method to get Magician's Manipulation Spirit (for Custom Traditions) in a language.
        /// </summary>
        public Task<string> DisplaySpiritManipulationMethodAsync(string strLanguage, CancellationToken token = default)
        {
            string strSpirit = SpiritManipulation;
            return string.IsNullOrEmpty(strSpirit)
                ? LanguageManager.GetStringAsync("String_None", strLanguage, token: token)
                : _objCharacter.TranslateExtraAsync(strSpirit, strLanguage, "critters.xml", token);
        }

        /// <summary>
        /// Magician's Manipulation Spirit (for Custom Traditions) in the language of the current UI.
        /// </summary>
        public string DisplaySpiritManipulation
        {
            get => DisplaySpiritManipulationMethod(GlobalSettings.Language);
            set
            {
                using (EnterReadLock.Enter(LockObject))
                {
                    if (Type != TraditionType.None)
                        SpiritManipulation
                            = _objCharacter.ReverseTranslateExtra(value, GlobalSettings.Language, "critters.xml");
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
                using (EnterReadLock.Enter(LockObject))
                    return _strSource;
            }
            set
            {
                using (EnterReadLock.Enter(LockObject))
                {
                    if (Interlocked.Exchange(ref _strSource, value) == value)
                        return;
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
                    if (Interlocked.Exchange(ref _strPage, value) == value)
                        return;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Description of the object.
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
                    if (Interlocked.Exchange(ref _strNotes, value) == value)
                        return;
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
            using (EnterReadLock.Enter(LockObject))
            {
                if (strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                    return Page;
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
            using (await EnterReadLock.EnterAsync(LockObject, token).ConfigureAwait(false))
            {
                if (strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                    return Page;
                XPathNavigator objNode = await this.GetNodeXPathAsync(strLanguage, token: token).ConfigureAwait(false);
                string s = objNode != null
                    ? (await objNode.SelectSingleNodeAndCacheExpressionAsync("altpage", token: token)
                                    .ConfigureAwait(false))?.Value ?? Page
                    : Page;
                return !string.IsNullOrWhiteSpace(s) ? s : Page;
            }
        }

        private XmlNode _xmlCachedMyXmlNode;
        private string _strCachedXmlNodeLanguage = string.Empty;

        public event PropertyChangedEventHandler PropertyChanged;

        public async Task<XmlNode> GetNodeCoreAsync(bool blnSync, string strLanguage, CancellationToken token = default)
        {
            // ReSharper disable once MethodHasAsyncOverload
            using (blnSync ? EnterReadLock.Enter(LockObject, token) : await EnterReadLock.EnterAsync(LockObject, token).ConfigureAwait(false))
            {
                if (Type == TraditionType.None)
                    return null;
                XmlNode objReturn = _xmlCachedMyXmlNode;
                if (objReturn != null && strLanguage == _strCachedXmlNodeLanguage
                                      && !GlobalSettings.LiveCustomData)
                    return objReturn;
                XmlDocument objDoc = null;
                switch (Type)
                {
                    case TraditionType.MAG:
                        objDoc = blnSync
                            // ReSharper disable once MethodHasAsyncOverload
                            ? _objCharacter.LoadData("traditions.xml", strLanguage, token: token)
                            : await _objCharacter.LoadDataAsync("traditions.xml", strLanguage, token: token)
                                                 .ConfigureAwait(false);
                        break;

                    case TraditionType.RES:
                        objDoc = blnSync
                            // ReSharper disable once MethodHasAsyncOverload
                            ? _objCharacter.LoadData("traditions.xml", strLanguage, token: token)
                            : await _objCharacter.LoadDataAsync("streams.xml", strLanguage, token: token)
                                                 .ConfigureAwait(false);
                        break;
                }

                objReturn = objDoc?.TryGetNodeById("/chummer/traditions/tradition", SourceID);
                _xmlCachedMyXmlNode = objReturn;
                _strCachedXmlNodeLanguage = strLanguage;
                return objReturn;
            }
        }

        private XPathNavigator _objCachedMyXPathNode;
        private string _strCachedXPathNodeLanguage = string.Empty;

        public async Task<XPathNavigator> GetNodeXPathCoreAsync(bool blnSync, string strLanguage, CancellationToken token = default)
        {
            // ReSharper disable once MethodHasAsyncOverload
            using (blnSync ? EnterReadLock.Enter(LockObject, token) : await EnterReadLock.EnterAsync(LockObject, token).ConfigureAwait(false))
            {
                XPathNavigator objReturn = _objCachedMyXPathNode;
                if (objReturn != null && strLanguage == _strCachedXPathNodeLanguage
                                      && !GlobalSettings.LiveCustomData)
                    return objReturn;
                XPathNavigator objDoc = null;
                switch (Type)
                {
                    case TraditionType.MAG:
                        objDoc = blnSync
                            // ReSharper disable once MethodHasAsyncOverload
                            ? _objCharacter.LoadDataXPath("traditions.xml", strLanguage, token: token)
                            : await _objCharacter.LoadDataXPathAsync("traditions.xml", strLanguage, token: token)
                                                 .ConfigureAwait(false);
                        break;

                    case TraditionType.RES:
                        objDoc = blnSync
                            // ReSharper disable once MethodHasAsyncOverload
                            ? _objCharacter.LoadDataXPath("streams.xml", strLanguage, token: token)
                            : await _objCharacter.LoadDataXPathAsync("streams.xml", strLanguage, token: token)
                                                 .ConfigureAwait(false);
                        break;
                }

                objReturn = objDoc?.TryGetNodeById("/chummer/traditions/tradition", SourceID);
                _objCachedMyXPathNode = objReturn;
                _strCachedXPathNodeLanguage = strLanguage;
                return objReturn;
            }
        }

        #endregion Properties

        #region static

        //A tree of dependencies. Once some of the properties are changed,
        //anything they depend on, also needs to raise OnChanged
        //This tree keeps track of dependencies
        private static readonly PropertyDependencyGraph<Tradition> s_AttributeDependencyGraph =
            new PropertyDependencyGraph<Tradition>(
                new DependencyGraphNode<string, Tradition>(nameof(CurrentDisplayName),
                    new DependencyGraphNode<string, Tradition>(nameof(DisplayName),
                        new DependencyGraphNode<string, Tradition>(nameof(DisplayNameShort),
                            new DependencyGraphNode<string, Tradition>(nameof(Name))
                        ),
                        new DependencyGraphNode<string, Tradition>(nameof(Extra))
                    )
                ),
                new DependencyGraphNode<string, Tradition>(nameof(DrainValueToolTip),
                    new DependencyGraphNode<string, Tradition>(nameof(DrainValue),
                        new DependencyGraphNode<string, Tradition>(nameof(DrainExpression))
                    )
                ),
                new DependencyGraphNode<string, Tradition>(nameof(DisplayDrainExpression),
                    new DependencyGraphNode<string, Tradition>(nameof(DrainExpression))
                ),
                new DependencyGraphNode<string, Tradition>(nameof(AvailableSpirits),
                    new DependencyGraphNode<string, Tradition>(nameof(SpiritCombat)),
                    new DependencyGraphNode<string, Tradition>(nameof(SpiritDetection)),
                    new DependencyGraphNode<string, Tradition>(nameof(SpiritHealth)),
                    new DependencyGraphNode<string, Tradition>(nameof(SpiritIllusion)),
                    new DependencyGraphNode<string, Tradition>(nameof(SpiritManipulation))
                ),
                new DependencyGraphNode<string, Tradition>(nameof(DisplaySpiritCombat),
                    new DependencyGraphNode<string, Tradition>(nameof(SpiritCombat))
                ),
                new DependencyGraphNode<string, Tradition>(nameof(DisplaySpiritDetection),
                    new DependencyGraphNode<string, Tradition>(nameof(SpiritDetection))
                ),
                new DependencyGraphNode<string, Tradition>(nameof(DisplaySpiritHealth),
                    new DependencyGraphNode<string, Tradition>(nameof(SpiritHealth))
                ),
                new DependencyGraphNode<string, Tradition>(nameof(DisplaySpiritIllusion),
                    new DependencyGraphNode<string, Tradition>(nameof(SpiritIllusion))
                ),
                new DependencyGraphNode<string, Tradition>(nameof(DisplaySpiritManipulation),
                    new DependencyGraphNode<string, Tradition>(nameof(SpiritManipulation))
                )
            );

        public static IEnumerable<Tradition> GetTraditions(Character character)
        {
            using (XmlNodeList xmlTraditions = character.LoadData("traditions.xml").SelectNodes("/chummer/traditions/tradition[" + character.Settings.BookXPath() + ']'))
            {
                if (xmlTraditions?.Count > 0)
                {
                    foreach (XmlNode node in xmlTraditions)
                    {
                        Tradition tradition = new Tradition(character);
                        tradition.Create(node);
                        yield return tradition;
                    }
                }
            }
        }

        #endregion static

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

        [NotifyPropertyChangedInvocator]
        public void OnPropertyChanged([CallerMemberName] string strPropertyName = null)
        {
            this.OnMultiplePropertyChanged(strPropertyName);
        }

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
                                = s_AttributeDependencyGraph.GetWithAllDependents(this, strPropertyName, true);
                        else
                        {
                            foreach (string strLoopChangedProperty in s_AttributeDependencyGraph
                                         .GetWithAllDependentsEnumerable(this, strPropertyName))
                                setNamesOfChangedProperties.Add(strLoopChangedProperty);
                        }
                    }

                    if (setNamesOfChangedProperties == null || setNamesOfChangedProperties.Count == 0)
                        return;

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

                _objCharacter?.OnPropertyChanged(nameof(Character.MagicTradition));
            }
        }

        /// <inheritdoc />
        public AsyncFriendlyReaderWriterLock LockObject { get; } = new AsyncFriendlyReaderWriterLock();
    }
}
