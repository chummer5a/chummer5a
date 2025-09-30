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
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;
using NLog;

namespace Chummer
{
    [HubClassTag("SourceID", true, "Name", "Extra")]
    [DebuggerDisplay("{DisplayNameShort(\"en-us\")}")]
    public sealed class MentorSpirit : IHasInternalId, IHasName, IHasSourceId, IHasXmlDataNode, IHasSource, IHasNotes, IHasLockObject, IHasCharacterObject
    {
        private static readonly Lazy<Logger> s_ObjLogger = new Lazy<Logger>(LogManager.GetCurrentClassLogger);
        private static Logger Log => s_ObjLogger.Value;

        private Guid _guiID;
        private string _strName = string.Empty;
        private string _strAdvantage = string.Empty;
        private string _strDisadvantage = string.Empty;
        private string _strExtra = string.Empty;
        private string _strExtraChoice1 = string.Empty;
        private string _strExtraChoice2 = string.Empty;
        private string _strSource = string.Empty;
        private string _strPage = string.Empty;
        private string _strNotes = string.Empty;
        private Color _colNotes = ColorManager.HasNotesColor;
        private XmlElement _nodBonus;
        private XmlNode _nodChoice1;
        private XmlNode _nodChoice2;
        private Improvement.ImprovementType _eMentorType;
        private Guid _guiSourceID;
        private readonly Character _objCharacter;
        private bool _blnMentorMask;

        public Character CharacterObject => _objCharacter; // readonly member, no locking needed

        /// <inheritdoc />
        public AsyncFriendlyReaderWriterLock LockObject { get; }

        #region Constructor

        public MentorSpirit(Character objCharacter, XmlNode xmlNodeMentor = null)
        {
            // Create the GUID for the new Mentor Spirit.
            _guiID = Guid.NewGuid();
            _objCharacter = objCharacter ?? throw new ArgumentNullException(nameof(objCharacter));
            LockObject = objCharacter.LockObject;
            if (xmlNodeMentor != null)
            {
                string strName = xmlNodeMentor["name"]?.InnerTextViaPool();
                if (!string.IsNullOrEmpty(strName))
                    Name = strName;
                string strType = xmlNodeMentor["mentortype"]?.InnerTextViaPool();
                if (!string.IsNullOrEmpty(strType)
                    && Enum.TryParse(strType, true, out Improvement.ImprovementType outEnum))
                {
                    _eMentorType = outEnum;
                }
            }
        }

        /// <summary>
        /// Create a Mentor Spirit from an XmlNode.
        /// </summary>
        /// <param name="xmlMentor">XmlNode to create the object from.</param>
        /// <param name="eMentorType">Whether this is a Mentor or a Paragon.</param>
        /// <param name="strForceValue">Force a value to be selected for the Mentor Spirit.</param>
        /// <param name="strChoice1">Name/Text for Choice 1.</param>
        /// <param name="strChoice2">Name/Text for Choice 2.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public void Create(XmlNode xmlMentor, Improvement.ImprovementType eMentorType, string strForceValue = "", string strChoice1 = "", string strChoice2 = "", CancellationToken token = default)
        {
            using (LockObject.EnterWriteLock(token))
            {
                token.ThrowIfCancellationRequested();
                _eMentorType = eMentorType;
                _objCachedMyXmlNode = null;
                _objCachedMyXPathNode = null;
                if (!xmlMentor.TryGetField("id", Guid.TryParse, out _guiSourceID))
                {
                    Log.Warn(new object[] {"Missing id field for xmlnode", xmlMentor});
                    Utils.BreakIfDebug();
                }

                xmlMentor.TryGetStringFieldQuickly("name", ref _strName);
                xmlMentor.TryGetStringFieldQuickly("source", ref _strSource);
                xmlMentor.TryGetStringFieldQuickly("page", ref _strPage);

                string sNotesColor = ColorTranslator.ToHtml(ColorManager.HasNotesColor);
                xmlMentor.TryGetStringFieldQuickly("notesColor", ref sNotesColor);
                _colNotes = ColorTranslator.FromHtml(sNotesColor);

                if (!xmlMentor.TryGetMultiLineStringFieldQuickly("altnotes", ref _strNotes))
                    xmlMentor.TryGetMultiLineStringFieldQuickly("notes", ref _strNotes);

                string strDisplayName = CurrentDisplayNameShort;
                if (GlobalSettings.InsertPdfNotesIfAvailable && string.IsNullOrEmpty(Notes))
                {
                    Notes = CommonFunctions.GetBookNotes(xmlMentor, Name, strDisplayName, Source, Page,
                                                             DisplayPage(GlobalSettings.Language), _objCharacter, token);
                }

                // Cache the English list of advantages gained through the Mentor Spirit.
                xmlMentor.TryGetMultiLineStringFieldQuickly("advantage", ref _strAdvantage);
                xmlMentor.TryGetMultiLineStringFieldQuickly("disadvantage", ref _strDisadvantage);

                _nodBonus = xmlMentor["bonus"];
                if (_nodBonus != null)
                {
                    string strOldForcedValue = ImprovementManager.GetForcedValue(_objCharacter);
                    string strOldSelectedValue = ImprovementManager.GetSelectedValue(_objCharacter);
                    try
                    {
                        ImprovementManager.SetForcedValue(strForceValue, _objCharacter);
                        if (!ImprovementManager.CreateImprovements(_objCharacter,
                                Improvement.ImprovementSource.MentorSpirit,
                                _guiID.ToString(
                                    "D", GlobalSettings.InvariantCultureInfo), _nodBonus,
                                1, strDisplayName, token: token))
                        {
                            _guiID = Guid.Empty;
                            return;
                        }

                        _strExtra = ImprovementManager.GetSelectedValue(_objCharacter);
                    }
                    finally
                    {
                        ImprovementManager.SetSelectedValue(strOldSelectedValue, _objCharacter);
                        ImprovementManager.SetForcedValue(strOldForcedValue, _objCharacter);
                    }

                    if (string.IsNullOrWhiteSpace(_strExtra))
                        _strExtra = strForceValue;
                }
                else if (!string.IsNullOrWhiteSpace(strForceValue))
                {
                    _strExtra = strForceValue;
                }
                else
                    _strExtra = string.Empty;

                if (!string.IsNullOrEmpty(strChoice1))
                {
                    _nodChoice1 = xmlMentor.SelectSingleNode("choices/choice[name = " + strChoice1.CleanXPath()
                        + "]/bonus");
                    if (_nodChoice1 != null)
                    {
                        string strOldForcedValue = ImprovementManager.GetForcedValue(_objCharacter);
                        string strOldSelectedValue = ImprovementManager.GetSelectedValue(_objCharacter);
                        try
                        {
                            ImprovementManager.SetForcedValue(strForceValue, _objCharacter);
                            if (!ImprovementManager.CreateImprovements(_objCharacter,
                                    Improvement.ImprovementSource.MentorSpirit,
                                    _guiID.ToString(
                                        "D", GlobalSettings.InvariantCultureInfo),
                                    _nodChoice1, 1, strDisplayName, token: token))
                            {
                                _guiID = Guid.Empty;
                                return;
                            }

                            _strExtraChoice1 = ImprovementManager.GetSelectedValue(_objCharacter);
                        }
                        finally
                        {
                            ImprovementManager.SetSelectedValue(strOldSelectedValue, _objCharacter);
                            ImprovementManager.SetForcedValue(strOldForcedValue, _objCharacter);
                        }

                        if (string.IsNullOrWhiteSpace(_strExtraChoice1))
                            _strExtraChoice1 = string.IsNullOrEmpty(strForceValue) ? strChoice1 : strForceValue;
                    }
                    else
                    {
                        _strExtraChoice1 = string.IsNullOrEmpty(strForceValue) ? strChoice1 : strForceValue;
                    }
                }
                else
                {
                    _nodChoice1 = null;
                    _strExtraChoice1 = string.Empty;
                }

                if (!string.IsNullOrEmpty(strChoice2))
                {
                    _nodChoice2 = xmlMentor.SelectSingleNode("choices/choice[name = " + strChoice2.CleanXPath()
                        + "]/bonus");
                    if (_nodChoice2 != null)
                    {
                        string strOldForcedValue = ImprovementManager.GetForcedValue(_objCharacter);
                        string strOldSelectedValue = ImprovementManager.GetSelectedValue(_objCharacter);
                        try
                        {
                            ImprovementManager.SetForcedValue(strForceValue, _objCharacter);
                            if (!ImprovementManager.CreateImprovements(_objCharacter,
                                    Improvement.ImprovementSource.MentorSpirit,
                                    _guiID.ToString(
                                        "D", GlobalSettings.InvariantCultureInfo),
                                    _nodChoice2, 1, strDisplayName, token: token))
                            {
                                _guiID = Guid.Empty;
                                return;
                            }

                            _strExtraChoice2 = ImprovementManager.GetSelectedValue(_objCharacter);
                        }
                        finally
                        {
                            ImprovementManager.SetSelectedValue(strOldSelectedValue, _objCharacter);
                            ImprovementManager.SetForcedValue(strOldForcedValue, _objCharacter);
                        }

                        if (string.IsNullOrWhiteSpace(_strExtraChoice2))
                            _strExtraChoice2 = string.IsNullOrEmpty(strForceValue) ? strChoice2 : strForceValue;
                    }
                    else
                    {
                        _strExtraChoice2 = string.IsNullOrEmpty(strForceValue) ? strChoice2 : strForceValue;
                    }
                }
                else
                {
                    _nodChoice2 = null;
                    _strExtraChoice2 = string.Empty;
                }

                /*
                if (string.IsNullOrEmpty(_strNotes))
                {
                    _strNotes = CommonFunctions.GetTextFromPdf(_strSource + " " + _strPage, _strName);
                    if (string.IsNullOrEmpty(_strNotes))
                    {
                        _strNotes = CommonFunctions.GetTextFromPdf(Source + " " + DisplayPage(GlobalSettings.Language), CurrentDisplayName);
                    }
                }
                */
            }
        }

        /// <summary>
        /// Create a Mentor Spirit from an XmlNode.
        /// </summary>
        /// <param name="xmlMentor">XmlNode to create the object from.</param>
        /// <param name="eMentorType">Whether this is a Mentor or a Paragon.</param>
        /// <param name="strForceValue">Force a value to be selected for the Mentor Spirit.</param>
        /// <param name="strChoice1">Name/Text for Choice 1.</param>
        /// <param name="strChoice2">Name/Text for Choice 2.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public async Task CreateAsync(XmlNode xmlMentor, Improvement.ImprovementType eMentorType, string strForceValue = "", string strChoice1 = "", string strChoice2 = "", CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                _eMentorType = eMentorType;
                _objCachedMyXmlNode = null;
                _objCachedMyXPathNode = null;
                if (!xmlMentor.TryGetField("id", Guid.TryParse, out _guiSourceID))
                {
                    Log.Warn(new object[] { "Missing id field for xmlnode", xmlMentor });
                    Utils.BreakIfDebug();
                }

                xmlMentor.TryGetStringFieldQuickly("name", ref _strName);
                xmlMentor.TryGetStringFieldQuickly("source", ref _strSource);
                xmlMentor.TryGetStringFieldQuickly("page", ref _strPage);

                string sNotesColor = ColorTranslator.ToHtml(ColorManager.HasNotesColor);
                xmlMentor.TryGetStringFieldQuickly("notesColor", ref sNotesColor);
                _colNotes = ColorTranslator.FromHtml(sNotesColor);

                if (!xmlMentor.TryGetMultiLineStringFieldQuickly("altnotes", ref _strNotes))
                    xmlMentor.TryGetMultiLineStringFieldQuickly("notes", ref _strNotes);

                string strDisplayName = await GetCurrentDisplayNameShortAsync(token).ConfigureAwait(false);
                if (GlobalSettings.InsertPdfNotesIfAvailable && string.IsNullOrEmpty(await GetNotesAsync(token).ConfigureAwait(false)))
                {
                    await SetNotesAsync(await CommonFunctions.GetBookNotesAsync(xmlMentor, await GetNameAsync(token).ConfigureAwait(false), strDisplayName, Source, Page,
                        await DisplayPageAsync(GlobalSettings.Language, token).ConfigureAwait(false), _objCharacter, token).ConfigureAwait(false), token).ConfigureAwait(false);
                }

                // Cache the English list of advantages gained through the Mentor Spirit.
                xmlMentor.TryGetMultiLineStringFieldQuickly("advantage", ref _strAdvantage);
                xmlMentor.TryGetMultiLineStringFieldQuickly("disadvantage", ref _strDisadvantage);

                _nodBonus = xmlMentor["bonus"];
                if (_nodBonus != null)
                {
                    string strOldForcedValue = ImprovementManager.GetForcedValue(_objCharacter);
                    string strOldSelectedValue = ImprovementManager.GetSelectedValue(_objCharacter);
                    try
                    {
                        ImprovementManager.SetForcedValue(strForceValue, _objCharacter);
                        if (!await ImprovementManager.CreateImprovementsAsync(_objCharacter,
                                Improvement.ImprovementSource.MentorSpirit,
                                _guiID.ToString(
                                    "D", GlobalSettings.InvariantCultureInfo), _nodBonus,
                                1, strDisplayName, token: token).ConfigureAwait(false))
                        {
                            _guiID = Guid.Empty;
                            return;
                        }

                        _strExtra = ImprovementManager.GetSelectedValue(_objCharacter);
                    }
                    finally
                    {
                        ImprovementManager.SetSelectedValue(strOldSelectedValue, _objCharacter);
                        ImprovementManager.SetForcedValue(strOldForcedValue, _objCharacter);
                    }

                    if (string.IsNullOrWhiteSpace(_strExtra))
                        _strExtra = strForceValue;
                }
                else if (!string.IsNullOrWhiteSpace(strForceValue))
                {
                    _strExtra = strForceValue;
                }
                else
                    _strExtra = string.Empty;

                if (!string.IsNullOrEmpty(strChoice1))
                {
                    _nodChoice1 = xmlMentor.SelectSingleNode("choices/choice[name = " + strChoice1.CleanXPath()
                        + "]/bonus");
                    if (_nodChoice1 != null)
                    {
                        string strOldForcedValue = ImprovementManager.GetForcedValue(_objCharacter);
                        string strOldSelectedValue = ImprovementManager.GetSelectedValue(_objCharacter);
                        try
                        {
                            ImprovementManager.SetForcedValue(strForceValue, _objCharacter);
                            if (!await ImprovementManager.CreateImprovementsAsync(_objCharacter,
                                    Improvement.ImprovementSource.MentorSpirit,
                                    _guiID.ToString(
                                        "D", GlobalSettings.InvariantCultureInfo),
                                    _nodChoice1, 1, strDisplayName, token: token).ConfigureAwait(false))
                            {
                                _guiID = Guid.Empty;
                                return;
                            }

                            _strExtraChoice1 = ImprovementManager.GetSelectedValue(_objCharacter);
                        }
                        finally
                        {
                            ImprovementManager.SetSelectedValue(strOldSelectedValue, _objCharacter);
                            ImprovementManager.SetForcedValue(strOldForcedValue, _objCharacter);
                        }

                        if (string.IsNullOrWhiteSpace(_strExtraChoice1))
                            _strExtraChoice1 = string.IsNullOrEmpty(strForceValue) ? strChoice1 : strForceValue;
                    }
                    else
                    {
                        _strExtraChoice1 = string.IsNullOrEmpty(strForceValue) ? strChoice1 : strForceValue;
                    }
                }
                else
                {
                    _nodChoice1 = null;
                    _strExtraChoice1 = string.Empty;
                }

                if (!string.IsNullOrEmpty(strChoice2))
                {
                    _nodChoice2 = xmlMentor.SelectSingleNode("choices/choice[name = " + strChoice2.CleanXPath()
                        + "]/bonus");
                    if (_nodChoice2 != null)
                    {
                        string strOldForcedValue = ImprovementManager.GetForcedValue(_objCharacter);
                        string strOldSelectedValue = ImprovementManager.GetSelectedValue(_objCharacter);
                        try
                        {
                            ImprovementManager.SetForcedValue(strForceValue, _objCharacter);
                            if (!await ImprovementManager.CreateImprovementsAsync(_objCharacter,
                                    Improvement.ImprovementSource.MentorSpirit,
                                    _guiID.ToString(
                                        "D", GlobalSettings.InvariantCultureInfo),
                                    _nodChoice2, 1, strDisplayName, token: token).ConfigureAwait(false))
                            {
                                _guiID = Guid.Empty;
                                return;
                            }

                            _strExtraChoice2 = ImprovementManager.GetSelectedValue(_objCharacter);
                        }
                        finally
                        {
                            ImprovementManager.SetSelectedValue(strOldSelectedValue, _objCharacter);
                            ImprovementManager.SetForcedValue(strOldForcedValue, _objCharacter);
                        }

                        if (string.IsNullOrWhiteSpace(_strExtraChoice2))
                            _strExtraChoice2 = string.IsNullOrEmpty(strForceValue) ? strChoice2 : strForceValue;
                    }
                    else
                    {
                        _strExtraChoice2 = string.IsNullOrEmpty(strForceValue) ? strChoice2 : strForceValue;
                    }
                }
                else
                {
                    _nodChoice2 = null;
                    _strExtraChoice2 = string.Empty;
                }

                /*
                if (string.IsNullOrEmpty(_strNotes))
                {
                    _strNotes = CommonFunctions.GetTextFromPdf(_strSource + " " + _strPage, _strName);
                    if (string.IsNullOrEmpty(_strNotes))
                    {
                        _strNotes = CommonFunctions.GetTextFromPdf(Source + " " + DisplayPage(GlobalSettings.Language), CurrentDisplayName);
                    }
                }
                */
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
                objWriter.WriteStartElement("mentorspirit");
                objWriter.WriteElementString("sourceid", SourceIDString);
                objWriter.WriteElementString("guid", InternalId);
                objWriter.WriteElementString("name", _strName);
                objWriter.WriteElementString("mentortype", _eMentorType.ToString());
                objWriter.WriteElementString("extra", _strExtra);
                objWriter.WriteElementString("extrachoice1", _strExtraChoice1);
                objWriter.WriteElementString("extrachoice2", _strExtraChoice2);
                objWriter.WriteElementString("source", _strSource);
                objWriter.WriteElementString("page", _strPage);
                objWriter.WriteElementString("advantage", _strAdvantage);
                objWriter.WriteElementString("disadvantage", _strDisadvantage);
                objWriter.WriteElementString("mentormask",
                                             _blnMentorMask.ToString(GlobalSettings.InvariantCultureInfo));
                if (!_nodBonus.IsNullOrInnerTextIsEmpty())
                    objWriter.WriteRaw("<bonus>" + _nodBonus.InnerXmlViaPool() + "</bonus>");
                else
                    objWriter.WriteElementString("bonus", string.Empty);
                if (!_nodChoice1.IsNullOrInnerTextIsEmpty())
                    objWriter.WriteRaw("<choice1>" + _nodChoice1.InnerXmlViaPool() + "</choice1>");
                else
                    objWriter.WriteElementString("choice1", string.Empty);
                if (!_nodChoice2.IsNullOrInnerTextIsEmpty())
                    objWriter.WriteRaw("<choice2>" + _nodChoice2.InnerXmlViaPool() + "</choice2>");
                else
                    objWriter.WriteElementString("choice2", string.Empty);

                objWriter.WriteElementString("notesColor", ColorTranslator.ToHtml(_colNotes));
                objWriter.WriteElementString("notes", _strNotes.CleanOfXmlInvalidUnicodeChars());

                if (SourceID != Guid.Empty)
                {
                    objWriter.WriteElementString("id", SourceIDString);
                }

                objWriter.WriteEndElement();
            }
        }

        /// <summary>
        /// Load the Mentor Spirit from the XmlNode.
        /// </summary>
        /// <param name="objNode">XmlNode to load.</param>
        public void Load(XmlNode objNode)
        {
            Utils.SafelyRunSynchronously(() => LoadCoreAsync(true, objNode));
        }

        /// <summary>
        /// Load the Mentor Spirit from the XmlNode.
        /// </summary>
        /// <param name="objNode">XmlNode to load.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public Task LoadAsync(XmlNode objNode, CancellationToken token = default)
        {
            return LoadCoreAsync(false, objNode, token);
        }

        private async Task LoadCoreAsync(bool blnSync, XmlNode objNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (objNode == null)
                return;
            IDisposable objLocker = null;
            IAsyncDisposable objLockerAsync = null;
            if (blnSync)
                objLocker = LockObject.EnterWriteLock(token);
            else
                objLockerAsync = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (!objNode.TryGetField("guid", Guid.TryParse, out _guiID))
                {
                    _guiID = Guid.NewGuid();
                }

                if (objNode.TryGetStringFieldQuickly("name", ref _strName))
                {
                    _objCachedMyXmlNode = null;
                    _objCachedMyXPathNode = null;
                }

                if (objNode["mentortype"] != null)
                {
                    _eMentorType = Improvement.ConvertToImprovementType(objNode["mentortype"].InnerTextViaPool(token));
                    _objCachedMyXmlNode = null;
                    _objCachedMyXPathNode = null;
                }

                Lazy<XPathNavigator> objMyNode = null;
                Microsoft.VisualStudio.Threading.AsyncLazy<XPathNavigator> objMyNodeAsync = null;
                if (blnSync)
                    objMyNode = new Lazy<XPathNavigator>(() => this.GetNodeXPath(token));
                else
                    objMyNodeAsync = new Microsoft.VisualStudio.Threading.AsyncLazy<XPathNavigator>(() => this.GetNodeXPathAsync(token), Utils.JoinableTaskFactory);
                if (!objNode.TryGetGuidFieldQuickly("sourceid", ref _guiSourceID)
                    && (blnSync ? objMyNode.Value : await objMyNodeAsync.GetValueAsync(token).ConfigureAwait(false))?.TryGetGuidFieldQuickly("id", ref _guiSourceID) == false)
                {
                    (blnSync
                            // ReSharper disable once MethodHasAsyncOverload
                            ? _objCharacter.LoadDataXPath("qualities.xml", token: token)
                        : await _objCharacter.LoadDataXPathAsync("qualities.xml", token: token).ConfigureAwait(false))
                                 .TryGetNodeByNameOrId("/chummer/mentors/mentor", Name)
                                 ?.TryGetGuidFieldQuickly("id", ref _guiSourceID);
                }

                objNode.TryGetStringFieldQuickly("extra", ref _strExtra);
                objNode.TryGetStringFieldQuickly("extrachoice1", ref _strExtraChoice1);
                objNode.TryGetStringFieldQuickly("extrachoice2", ref _strExtraChoice2);
                objNode.TryGetStringFieldQuickly("source", ref _strSource);
                objNode.TryGetStringFieldQuickly("page", ref _strPage);
                if (_objCharacter.LastSavedVersion <= new ValueVersion(5, 217, 31))
                {
                    // Cache advantages from data file because localized version used to be cached directly.
                    XPathNavigator node = blnSync ? objMyNode.Value : await objMyNodeAsync.GetValueAsync(token).ConfigureAwait(false);
                    if (node != null)
                    {
                        if (!node.TryGetMultiLineStringFieldQuickly("advantage", ref _strAdvantage))
                            objNode.TryGetMultiLineStringFieldQuickly("advantage", ref _strAdvantage);
                        if (!node.TryGetMultiLineStringFieldQuickly("disadvantage", ref _strDisadvantage))
                            objNode.TryGetMultiLineStringFieldQuickly("disadvantage", ref _strDisadvantage);
                    }
                    else
                    {
                        objNode.TryGetMultiLineStringFieldQuickly("advantage", ref _strAdvantage);
                        objNode.TryGetMultiLineStringFieldQuickly("disadvantage", ref _strDisadvantage);
                    }
                }
                else
                {
                    objNode.TryGetMultiLineStringFieldQuickly("advantage", ref _strAdvantage);
                    objNode.TryGetMultiLineStringFieldQuickly("disadvantage", ref _strDisadvantage);
                }

                objNode.TryGetBoolFieldQuickly("mentormask", ref _blnMentorMask);
                _nodBonus = objNode["bonus"];
                _nodChoice1 = objNode["choice1"];
                _nodChoice2 = objNode["choice2"];

                objNode.TryGetMultiLineStringFieldQuickly("notes", ref _strNotes);
                string sNotesColor = ColorTranslator.ToHtml(ColorManager.HasNotesColor);
                objNode.TryGetStringFieldQuickly("notesColor", ref sNotesColor);
                _colNotes = ColorTranslator.FromHtml(sNotesColor);
            }
            finally
            {
                if (blnSync)
                    objLocker.Dispose();
                else
                    await objLockerAsync.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Print the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        /// <param name="strLanguageToPrint">Language in which to print</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public async Task Print(XmlWriter objWriter, string strLanguageToPrint, CancellationToken token = default)
        {
            if (objWriter == null)
                return;
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                // <mentorspirit>
                XmlElementWriteHelper objBaseElement
                    = await objWriter.StartElementAsync("mentorspirit", token).ConfigureAwait(false);
                try
                {
                    await objWriter.WriteElementStringAsync("guid", InternalId, token).ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("sourceid", SourceIDString, token).ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("mentortype", _eMentorType.ToString(), token)
                        .ConfigureAwait(false);
                    await objWriter
                        .WriteElementStringAsync(
                            "name", await DisplayNameShortAsync(strLanguageToPrint, token).ConfigureAwait(false),
                            token).ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("name_english", await GetNameAsync(token).ConfigureAwait(false), token).ConfigureAwait(false);
                    await objWriter
                        .WriteElementStringAsync("advantage",
                            await DisplayAdvantageAsync(strLanguageToPrint, token)
                                .ConfigureAwait(false), token).ConfigureAwait(false);
                    await objWriter
                        .WriteElementStringAsync("disadvantage",
                            await DisplayDisadvantageAsync(strLanguageToPrint, token)
                                .ConfigureAwait(false), token).ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("advantage_english", Advantage, token)
                        .ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("disadvantage_english", Disadvantage, token)
                        .ConfigureAwait(false);
                    string strExtra = await GetExtraAsync(token).ConfigureAwait(false);
                    string strExtraChoice1 = await GetExtraChoice1Async(token).ConfigureAwait(false);
                    string strExtraChoice2 = await GetExtraChoice2Async(token).ConfigureAwait(false);
                    await objWriter
                        .WriteElementStringAsync(
                            "extra",
                            await _objCharacter.TranslateExtraAsync(strExtra, strLanguageToPrint, token: token)
                                .ConfigureAwait(false), token).ConfigureAwait(false);
                    await objWriter
                        .WriteElementStringAsync(
                            "extrachoice1",
                            await _objCharacter.TranslateExtraAsync(strExtraChoice1, strLanguageToPrint, token: token)
                                .ConfigureAwait(false), token).ConfigureAwait(false);
                    await objWriter
                        .WriteElementStringAsync(
                            "extrachoice2",
                            await _objCharacter.TranslateExtraAsync(strExtraChoice2, strLanguageToPrint, token: token)
                                .ConfigureAwait(false), token).ConfigureAwait(false);
                    await objWriter
                        .WriteElementStringAsync(
                            "extra_english", strExtra, token).ConfigureAwait(false);
                    await objWriter
                        .WriteElementStringAsync(
                            "extrachoice1_english", strExtraChoice1, token).ConfigureAwait(false);
                    await objWriter
                        .WriteElementStringAsync(
                            "extrachoice2_english", strExtraChoice2, token).ConfigureAwait(false);
                    await objWriter
                        .WriteElementStringAsync(
                            "source",
                            await _objCharacter.LanguageBookShortAsync(Source, strLanguageToPrint, token)
                                .ConfigureAwait(false), token).ConfigureAwait(false);
                    await objWriter
                        .WriteElementStringAsync(
                            "page", await DisplayPageAsync(strLanguageToPrint, token).ConfigureAwait(false), token)
                        .ConfigureAwait(false);
                    await objWriter
                        .WriteElementStringAsync("mentormask",
                            MentorMask.ToString(GlobalSettings.InvariantCultureInfo), token)
                        .ConfigureAwait(false);
                    if (GlobalSettings.PrintNotes)
                        await objWriter.WriteElementStringAsync("notes", _strNotes.CleanOfXmlInvalidUnicodeChars(), token)
                            .ConfigureAwait(false);
                }
                finally
                {
                    // </mentorspirit>
                    await objBaseElement.DisposeAsync().ConfigureAwait(false);
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        #endregion Constructor

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
        /// Name of the Mentor Spirit or Paragon.
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
                    if (SourceID == Guid.Empty)
                    {
                        using (LockObject.EnterWriteLock())
                        {
                            _objCachedMyXmlNode = null;
                            _objCachedMyXPathNode = null;
                        }
                    }

                    using (_objCharacter.LockObject.EnterUpgradeableReadLock())
                    {
                        if (_objCharacter.MentorSpirits.Count > 0 && _objCharacter.MentorSpirits[0] == this)
                            _objCharacter.OnPropertyChanged(nameof(Character.FirstMentorSpiritDisplayInformation));
                    }
                }
            }
        }

        /// <summary>
        /// Name of the Mentor Spirit or Paragon.
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
        /// Name of the Mentor Spirit or Paragon.
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
                if (SourceID == Guid.Empty)
                {
                    token.ThrowIfCancellationRequested();
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
                }

                token.ThrowIfCancellationRequested();
                IAsyncDisposable objLocker3 = await _objCharacter.LockObject.EnterUpgradeableReadLockAsync(token)
                    .ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    if (await _objCharacter.MentorSpirits.GetCountAsync(token).ConfigureAwait(false) > 0 &&
                        await _objCharacter.MentorSpirits.GetValueAtAsync(0, token).ConfigureAwait(false) == this)
                        await _objCharacter
                            .OnPropertyChangedAsync(nameof(Character.FirstMentorSpiritDisplayInformation), token)
                            .ConfigureAwait(false);
                }
                finally
                {
                    await objLocker3.DisposeAsync().ConfigureAwait(false);
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Choices related to the mentor as it should be displayed in the UI.
        /// </summary>
        public string DisplayExtras(string strLanguage)
        {
            using (LockObject.EnterReadLock())
            {
                string strReturn;
                string strReturn1 = LanguageManager.TranslateExtra(Extra, strLanguage, _objCharacter);
                string strReturn2 = LanguageManager.TranslateExtra(ExtraChoice1, strLanguage, _objCharacter);
                string strReturn3 = LanguageManager.TranslateExtra(ExtraChoice2, strLanguage, _objCharacter);

                if (!string.IsNullOrWhiteSpace(strReturn1))
                {
                    strReturn = strReturn1;
                    if (!string.IsNullOrWhiteSpace(strReturn2))
                    {
                        strReturn += Environment.NewLine + strReturn2;
                    }
                    if (!string.IsNullOrWhiteSpace(strReturn3))
                    {
                        strReturn += Environment.NewLine + strReturn3;
                    }
                }
                else if (!string.IsNullOrWhiteSpace(strReturn2))
                {
                    strReturn = strReturn2;
                    if (!string.IsNullOrWhiteSpace(strReturn3))
                    {
                        strReturn += Environment.NewLine + strReturn3;
                    }
                }
                else
                {
                    strReturn = strReturn3;
                }

                return strReturn;
            }
        }

        /// <summary>
        /// Choices related to the mentor as it should be displayed in the UI.
        /// </summary>
        public async Task<string> DisplayExtrasAsync(string strLanguage, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                string strReturn;
                string strReturn1 = await LanguageManager
                    .TranslateExtraAsync(await GetExtraAsync(token).ConfigureAwait(false), strLanguage, _objCharacter, token: token)
                    .ConfigureAwait(false);
                string strReturn2 = await LanguageManager
                    .TranslateExtraAsync(await GetExtraChoice1Async(token).ConfigureAwait(false), strLanguage, _objCharacter, token: token)
                    .ConfigureAwait(false);
                string strReturn3 = await LanguageManager
                    .TranslateExtraAsync(await GetExtraChoice2Async(token).ConfigureAwait(false), strLanguage, _objCharacter, token: token)
                    .ConfigureAwait(false);

                if (!string.IsNullOrWhiteSpace(strReturn1))
                {
                    strReturn = strReturn1;
                    if (!string.IsNullOrWhiteSpace(strReturn2))
                    {
                        strReturn += Environment.NewLine + strReturn2;
                    }
                    if (!string.IsNullOrWhiteSpace(strReturn3))
                    {
                        strReturn += Environment.NewLine + strReturn3;
                    }
                }
                else if (!string.IsNullOrWhiteSpace(strReturn2))
                {
                    strReturn = strReturn2;
                    if (!string.IsNullOrWhiteSpace(strReturn3))
                    {
                        strReturn += Environment.NewLine + strReturn3;
                    }
                }
                else
                {
                    strReturn = strReturn3;
                }

                return strReturn;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Extra string related to improvements selected for the Mentor Spirit or Paragon.
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
                    {
                        using (_objCharacter.LockObject.EnterUpgradeableReadLock())
                        {
                            if (_objCharacter.MentorSpirits.Count > 0 && _objCharacter.MentorSpirits[0] == this)
                                _objCharacter.OnPropertyChanged(nameof(Character.FirstMentorSpiritDisplayInformation));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Extra string related to improvements selected for the Mentor Spirit or Paragon.
        /// </summary>
        public async Task<string> GetExtraAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _strExtra;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Extra string related to improvements selected for the Mentor Spirit or Paragon.
        /// </summary>
        public async Task SetExtraAsync(string value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (Interlocked.Exchange(ref _strExtra, value) == value)
                    return;

                token.ThrowIfCancellationRequested();
                IAsyncDisposable objLocker3 = await _objCharacter.LockObject.EnterUpgradeableReadLockAsync(token)
                    .ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    if (await _objCharacter.MentorSpirits.GetCountAsync(token).ConfigureAwait(false) > 0 &&
                        await _objCharacter.MentorSpirits.GetValueAtAsync(0, token).ConfigureAwait(false) == this)
                        await _objCharacter
                            .OnPropertyChangedAsync(nameof(Character.FirstMentorSpiritDisplayInformation), token)
                            .ConfigureAwait(false);
                }
                finally
                {
                    await objLocker3.DisposeAsync().ConfigureAwait(false);
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Extra string related to the improvements selected for the first choice of the Mentor Spirit or Paragon.
        /// </summary>
        public string ExtraChoice1
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _strExtraChoice1;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (Interlocked.Exchange(ref _strExtraChoice1, value) != value)
                    {
                        using (_objCharacter.LockObject.EnterUpgradeableReadLock())
                        {
                            if (_objCharacter.MentorSpirits.Count > 0 && _objCharacter.MentorSpirits[0] == this)
                                _objCharacter.OnPropertyChanged(nameof(Character.FirstMentorSpiritDisplayInformation));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Extra string related to the improvements selected for the first choice of the Mentor Spirit or Paragon.
        /// </summary>
        public async Task<string> GetExtraChoice1Async(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _strExtraChoice1;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Extra string related to the improvements selected for the first choice of the Mentor Spirit or Paragon.
        /// </summary>
        public async Task SetExtraChoice1Async(string value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (Interlocked.Exchange(ref _strExtraChoice1, value) == value)
                    return;

                token.ThrowIfCancellationRequested();
                IAsyncDisposable objLocker3 = await _objCharacter.LockObject.EnterUpgradeableReadLockAsync(token)
                    .ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    if (await _objCharacter.MentorSpirits.GetCountAsync(token).ConfigureAwait(false) > 0 &&
                        await _objCharacter.MentorSpirits.GetValueAtAsync(0, token).ConfigureAwait(false) == this)
                        await _objCharacter
                            .OnPropertyChangedAsync(nameof(Character.FirstMentorSpiritDisplayInformation), token)
                            .ConfigureAwait(false);
                }
                finally
                {
                    await objLocker3.DisposeAsync().ConfigureAwait(false);
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Extra string related to improvements selected for the second choice of the Mentor Spirit or Paragon.
        /// </summary>
        public string ExtraChoice2
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _strExtraChoice2;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (Interlocked.Exchange(ref _strExtraChoice2, value) != value)
                    {
                        using (_objCharacter.LockObject.EnterUpgradeableReadLock())
                        {
                            if (_objCharacter.MentorSpirits.Count > 0 && _objCharacter.MentorSpirits[0] == this)
                                _objCharacter.OnPropertyChanged(nameof(Character.FirstMentorSpiritDisplayInformation));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Extra string related to the improvements selected for the second choice of the Mentor Spirit or Paragon.
        /// </summary>
        public async Task<string> GetExtraChoice2Async(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _strExtraChoice2;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Extra string related to the improvements selected for the second choice of the Mentor Spirit or Paragon.
        /// </summary>
        public async Task SetExtraChoice2Async(string value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (Interlocked.Exchange(ref _strExtraChoice2, value) == value)
                    return;

                token.ThrowIfCancellationRequested();
                IAsyncDisposable objLocker3 = await _objCharacter.LockObject.EnterUpgradeableReadLockAsync(token)
                    .ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    if (await _objCharacter.MentorSpirits.GetCountAsync(token).ConfigureAwait(false) > 0 &&
                        await _objCharacter.MentorSpirits.GetValueAtAsync(0, token).ConfigureAwait(false) == this)
                        await _objCharacter
                            .OnPropertyChangedAsync(nameof(Character.FirstMentorSpiritDisplayInformation), token)
                            .ConfigureAwait(false);
                }
                finally
                {
                    await objLocker3.DisposeAsync().ConfigureAwait(false);
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Whether the Mentor Spirit is taken with a Mentor Mask.
        /// </summary>
        public bool MentorMask
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _blnMentorMask;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                    _blnMentorMask = value;
            }
        }

        /// <summary>
        /// Advantage of the Mentor Spirit or Paragon (in English).
        /// </summary>
        public string Advantage
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _strAdvantage;
            }
        }

        /// <summary>
        /// Advantage of the mentor as it should be displayed in the UI.
        /// </summary>
        public string DisplayAdvantage(string strLanguage)
        {
            using (LockObject.EnterReadLock())
            {
                string strReturn = Advantage;
                if (strLanguage != GlobalSettings.DefaultLanguage)
                {
                    string strTemp = string.Empty;
                    if (this.GetNodeXPath(strLanguage)?.TryGetMultiLineStringFieldQuickly("altadvantage", ref strTemp)
                        == true)
                        strReturn = strTemp;
                }

                return strReturn;
            }
        }

        /// <summary>
        /// Advantage of the mentor as it should be displayed in the UI.
        /// </summary>
        public async Task<string> DisplayAdvantageAsync(string strLanguage, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                string strReturn = Advantage;
                if (strLanguage != GlobalSettings.DefaultLanguage)
                {
                    string strTemp = string.Empty;
                    if ((await this.GetNodeXPathAsync(strLanguage, token: token).ConfigureAwait(false))
                        ?.TryGetMultiLineStringFieldQuickly("altadvantage", ref strTemp) == true)
                        strReturn = strTemp;
                }

                return strReturn;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Disadvantage of the Mentor Spirit or Paragon (in English).
        /// </summary>
        public string Disadvantage
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _strDisadvantage;
            }
        }

        /// <summary>
        /// Disadvantage of the mentor as it should be displayed in the UI.
        /// </summary>
        public string DisplayDisadvantage(string strLanguage)
        {
            using (LockObject.EnterReadLock())
            {
                string strReturn = Disadvantage;
                if (strLanguage != GlobalSettings.DefaultLanguage)
                {
                    string strTemp = string.Empty;
                    if (this.GetNodeXPath(strLanguage)
                            ?.TryGetMultiLineStringFieldQuickly("altdisadvantage", ref strTemp) == true)
                        strReturn = strTemp;
                }

                return strReturn;
            }
        }

        /// <summary>
        /// Disadvantage of the mentor as it should be displayed in the UI.
        /// </summary>
        public async Task<string> DisplayDisadvantageAsync(string strLanguage, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                string strReturn = Disadvantage;
                if (strLanguage != GlobalSettings.DefaultLanguage)
                {
                    string strTemp = string.Empty;
                    if ((await this.GetNodeXPathAsync(strLanguage, token: token).ConfigureAwait(false))
                        ?.TryGetMultiLineStringFieldQuickly("altdisadvantage", ref strTemp) == true)
                        strReturn = strTemp;
                }

                return strReturn;
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
                return await GetNameAsync(token).ConfigureAwait(false);

            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                XPathNavigator objNode = await this.GetNodeXPathAsync(strLanguage, token: token).ConfigureAwait(false);
                return objNode?.SelectSingleNodeAndCacheExpression("translate", token)?.Value ?? await GetNameAsync(token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public string CurrentDisplayNameShort => DisplayNameShort(GlobalSettings.Language);

        public string CurrentDisplayName => CurrentDisplayNameShort;

        public Task<string> GetCurrentDisplayNameShortAsync(CancellationToken token = default)
        {
            return DisplayNameShortAsync(GlobalSettings.Language, token);
        }

        public Task<string> GetCurrentDisplayNameAsync(CancellationToken token = default)
        {
            return GetCurrentDisplayNameShortAsync(token);
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
                    _strSource = value;
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
                    _strPage = value;
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
                using (LockObject.EnterReadLock())
                {
                    if (_strNotes == value)
                        return;
                }
                using (LockObject.EnterUpgradeableReadLock())
                    _strNotes = value;
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
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_strNotes == value)
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
                _strNotes = value;
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

        public Color PreferredColor
        {
            get
            {
                using (LockObject.EnterReadLock())
                {
                    if (!string.IsNullOrEmpty(Notes))
                    {
                        return ColorManager.GenerateCurrentModeColor(NotesColor);
                    }

                    return ColorManager.WindowText;
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
                    return ColorManager.GenerateCurrentModeColor(await GetNotesColorAsync(token).ConfigureAwait(false));
                }

                return ColorManager.WindowText;
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
                    ? _objCharacter.LoadData(_eMentorType == Improvement.ImprovementType.MentorSpirit
                                                 ? "mentors.xml"
                                                 : "paragons.xml", strLanguage, token: token)
                    : await _objCharacter.LoadDataAsync(_eMentorType == Improvement.ImprovementType.MentorSpirit
                                                            ? "mentors.xml"
                                                            : "paragons.xml", strLanguage, token: token)
                                         .ConfigureAwait(false);
                if (SourceID != Guid.Empty)
                    objReturn = objDoc.TryGetNodeById("/chummer/mentors/mentor", SourceID);
                if (objReturn == null)
                {
                    objReturn = objDoc.TryGetNodeByNameOrId("/chummer/mentors/mentor", blnSync ? Name : await GetNameAsync(token).ConfigureAwait(false));
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
                    ? _objCharacter.LoadDataXPath(_eMentorType == Improvement.ImprovementType.MentorSpirit
                                                 ? "mentors.xml"
                                                 : "paragons.xml", strLanguage, token: token)
                    : await _objCharacter.LoadDataXPathAsync(_eMentorType == Improvement.ImprovementType.MentorSpirit
                                                            ? "mentors.xml"
                                                            : "paragons.xml", strLanguage, token: token)
                                         .ConfigureAwait(false);
                if (SourceID != Guid.Empty)
                    objReturn = objDoc.TryGetNodeById("/chummer/mentors/mentor", SourceID);
                if (objReturn == null)
                {
                    objReturn = objDoc.TryGetNodeByNameOrId("/chummer/mentors/mentor", blnSync ? Name : await GetNameAsync(token).ConfigureAwait(false));
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

        public string InternalId
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _guiID.ToString("D", GlobalSettings.InvariantCultureInfo);
            }
        }

        #endregion Properties

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
    }
}
