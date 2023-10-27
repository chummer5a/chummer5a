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
    [DebuggerDisplay("{DisplayNameShort(GlobalSettings.DefaultLanguage)}")]
    public sealed class MentorSpirit : IHasInternalId, IHasName, IHasSourceId, IHasXmlDataNode, IHasSource, IHasNotes, IHasLockObject
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
        private XmlNode _nodBonus;
        private XmlNode _nodChoice1;
        private XmlNode _nodChoice2;
        private Improvement.ImprovementType _eMentorType;
        private Guid _guiSourceID;
        private readonly Character _objCharacter;
        private bool _blnMentorMask;

        /// <inheritdoc />
        public AsyncFriendlyReaderWriterLock LockObject { get; } = new AsyncFriendlyReaderWriterLock();

        #region Constructor

        public MentorSpirit(Character objCharacter, XmlNode xmlNodeMentor = null)
        {
            // Create the GUID for the new Mentor Spirit.
            _guiID = Guid.NewGuid();
            _objCharacter = objCharacter;
            if (xmlNodeMentor != null)
            {
                string strName = xmlNodeMentor["name"]?.InnerText;
                if (!string.IsNullOrEmpty(strName))
                    Name = strName;
                string strType = xmlNodeMentor["mentortype"]?.InnerText;
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
        public void Create(XmlNode xmlMentor, Improvement.ImprovementType eMentorType, string strForceValue = "", string strChoice1 = "", string strChoice2 = "")
        {
            using (LockObject.EnterWriteLock())
            {
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
                                                             DisplayPage(GlobalSettings.Language), _objCharacter);
                }

                // Cache the English list of advantages gained through the Mentor Spirit.
                xmlMentor.TryGetMultiLineStringFieldQuickly("advantage", ref _strAdvantage);
                xmlMentor.TryGetMultiLineStringFieldQuickly("disadvantage", ref _strDisadvantage);

                _nodBonus = xmlMentor["bonus"];
                if (_nodBonus != null)
                {
                    string strOldForce = ImprovementManager.ForcedValue;
                    string strOldSelected = ImprovementManager.SelectedValue;
                    try
                    {
                        ImprovementManager.ForcedValue = strForceValue;
                        ImprovementManager.SelectedValue = string.Empty;
                        if (!ImprovementManager.CreateImprovements(_objCharacter,
                                Improvement.ImprovementSource.MentorSpirit,
                                _guiID.ToString(
                                    "D", GlobalSettings.InvariantCultureInfo), _nodBonus,
                                1, strDisplayName))
                        {
                            _guiID = Guid.Empty;
                            return;
                        }

                        _strExtra = ImprovementManager.SelectedValue;
                    }
                    finally
                    {
                        ImprovementManager.ForcedValue = strOldForce;
                        ImprovementManager.SelectedValue = strOldSelected;
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
                        string strOldForce = ImprovementManager.ForcedValue;
                        string strOldSelected = ImprovementManager.SelectedValue;
                        try
                        {
                            ImprovementManager.ForcedValue = string.Empty;
                            ImprovementManager.SelectedValue = string.Empty;
                            if (!ImprovementManager.CreateImprovements(_objCharacter,
                                    Improvement.ImprovementSource.MentorSpirit,
                                    _guiID.ToString(
                                        "D", GlobalSettings.InvariantCultureInfo),
                                    _nodChoice1, 1, strDisplayName))
                            {
                                _guiID = Guid.Empty;
                                return;
                            }

                            _strExtraChoice1 = ImprovementManager.SelectedValue;
                        }
                        finally
                        {
                            ImprovementManager.ForcedValue = strOldForce;
                            ImprovementManager.SelectedValue = strOldSelected;
                        }

                        if (string.IsNullOrWhiteSpace(_strExtraChoice1))
                            _strExtraChoice1 = strChoice1;
                    }
                    else
                    {
                        _strExtraChoice1 = strChoice1;
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
                        string strOldForce = ImprovementManager.ForcedValue;
                        string strOldSelected = ImprovementManager.SelectedValue;
                        try
                        {
                            ImprovementManager.ForcedValue = string.Empty;
                            ImprovementManager.SelectedValue = string.Empty;
                            if (!ImprovementManager.CreateImprovements(_objCharacter,
                                    Improvement.ImprovementSource.MentorSpirit,
                                    _guiID.ToString(
                                        "D", GlobalSettings.InvariantCultureInfo),
                                    _nodChoice2, 1, strDisplayName))
                            {
                                _guiID = Guid.Empty;
                                return;
                            }

                            _strExtraChoice2 = ImprovementManager.SelectedValue;
                        }
                        finally
                        {
                            ImprovementManager.ForcedValue = strOldForce;
                            ImprovementManager.SelectedValue = strOldSelected;
                        }

                        if (string.IsNullOrWhiteSpace(_strExtraChoice2))
                            _strExtraChoice2 = strChoice2;
                    }
                    else
                    {
                        _strExtraChoice2 = strChoice2;
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
                    _strNotes = CommonFunctions.GetTextFromPdf(_strSource + ' ' + _strPage, _strName);
                    if (string.IsNullOrEmpty(_strNotes))
                    {
                        _strNotes = CommonFunctions.GetTextFromPdf(Source + ' ' + DisplayPage(GlobalSettings.Language), CurrentDisplayName);
                    }
                }
                */
            }
        }

        private SourceString _objCachedSourceDetail;

        public SourceString SourceDetail
        {
            get
            {
                using (LockObject.EnterReadLock())
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
                if (_nodBonus != null)
                    objWriter.WriteRaw("<bonus>" + _nodBonus.InnerXml + "</bonus>");
                else
                    objWriter.WriteElementString("bonus", string.Empty);
                if (_nodChoice1 != null)
                    objWriter.WriteRaw("<choice1>" + _nodChoice1.InnerXml + "</choice1>");
                else
                    objWriter.WriteElementString("choice1", string.Empty);
                if (_nodChoice2 != null)
                    objWriter.WriteRaw("<choice2>" + _nodChoice2.InnerXml + "</choice2>");
                else
                    objWriter.WriteElementString("choice2", string.Empty);

                objWriter.WriteElementString("notesColor", ColorTranslator.ToHtml(_colNotes));
                objWriter.WriteElementString("notes", _strNotes.CleanOfInvalidUnicodeChars());

                if (SourceID != Guid.Empty && !string.IsNullOrEmpty(SourceIDString))
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
            if (objNode == null)
                return;
            using (LockObject.EnterWriteLock())
            {
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
                    _eMentorType = Improvement.ConvertToImprovementType(objNode["mentortype"].InnerText);
                    _objCachedMyXmlNode = null;
                    _objCachedMyXPathNode = null;
                }

                Lazy<XPathNavigator> objMyNode = new Lazy<XPathNavigator>(() => this.GetNodeXPath());
                if (!objNode.TryGetGuidFieldQuickly("sourceid", ref _guiSourceID)
                    && objMyNode.Value?.TryGetGuidFieldQuickly("id", ref _guiSourceID) == false)
                {
                    _objCharacter.LoadDataXPath("qualities.xml")
                                 .TryGetNodeByNameOrId("/chummer/mentors/mentor", Name)
                                 ?.TryGetGuidFieldQuickly("id", ref _guiSourceID);
                }

                objNode.TryGetStringFieldQuickly("extra", ref _strExtra);
                objNode.TryGetStringFieldQuickly("extrachoice1", ref _strExtraChoice1);
                objNode.TryGetStringFieldQuickly("extrachoice2", ref _strExtraChoice2);
                objNode.TryGetStringFieldQuickly("source", ref _strSource);
                objNode.TryGetStringFieldQuickly("page", ref _strPage);
                if (_objCharacter.LastSavedVersion <= new Version(5, 217, 31))
                {
                    // Cache advantages from data file because localized version used to be cached directly.
                    XPathNavigator node = objMyNode.Value;
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
        }

        /// <summary>
        /// Print the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        /// <param name="strLanguageToPrint">Language in which to print</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public async ValueTask Print(XmlWriter objWriter, string strLanguageToPrint, CancellationToken token = default)
        {
            if (objWriter == null)
                return;
            using (await LockObject.EnterReadLockAsync(token).ConfigureAwait(false))
            {
                token.ThrowIfCancellationRequested();
                // <mentorspirit>
                XmlElementWriteHelper objBaseElement
                    = await objWriter.StartElementAsync("mentorspirit", token).ConfigureAwait(false);
                try
                {
                    await objWriter.WriteElementStringAsync("guid", InternalId, token).ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("sourceid", SourceIDString, token).ConfigureAwait(false);
                    await objWriter
                          .WriteElementStringAsync(
                              "name", await DisplayNameShortAsync(strLanguageToPrint, token).ConfigureAwait(false),
                              token).ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("mentortype", _eMentorType.ToString(), token)
                                   .ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("name_english", Name, token).ConfigureAwait(false);
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
                    await objWriter
                          .WriteElementStringAsync(
                              "extra",
                              await _objCharacter.TranslateExtraAsync(Extra, strLanguageToPrint, token: token)
                                                 .ConfigureAwait(false), token).ConfigureAwait(false);
                    await objWriter
                        .WriteElementStringAsync(
                            "extrachoice1",
                            await _objCharacter.TranslateExtraAsync(ExtraChoice1, strLanguageToPrint, token: token)
                                .ConfigureAwait(false), token).ConfigureAwait(false);
                    await objWriter
                        .WriteElementStringAsync(
                            "extrachoice2",
                            await _objCharacter.TranslateExtraAsync(ExtraChoice2, strLanguageToPrint, token: token)
                                .ConfigureAwait(false), token).ConfigureAwait(false);
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
                        await objWriter.WriteElementStringAsync("notes", _strNotes.CleanOfInvalidUnicodeChars(), token)
                                       .ConfigureAwait(false);
                }
                finally
                {
                    // </mentorspirit>
                    await objBaseElement.DisposeAsync().ConfigureAwait(false);
                }
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
                {
                    if (string.IsNullOrEmpty(_strName) && _objCharacter.MentorSpirits.Count > 0
                                                       && _objCharacter.MentorSpirits[0] == this)
                    {
                        _strName = _objCharacter.MentorSpirits[0].Name;
                    }

                    return _strName;
                }
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (Interlocked.Exchange(ref _strName, value) != value)
                    {
                        if (SourceID == Guid.Empty)
                        {
                            _objCachedMyXmlNode = null;
                            _objCachedMyXPathNode = null;
                        }

                        if (_objCharacter.MentorSpirits.Count > 0 && _objCharacter.MentorSpirits[0] == this)
                            _objCharacter.OnPropertyChanged(nameof(Character.FirstMentorSpiritDisplayName));
                    }
                }
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
        public async ValueTask<string> DisplayExtrasAsync(string strLanguage, CancellationToken token = default)
        {
            using (await LockObject.EnterReadLockAsync(token).ConfigureAwait(false))
            {
                token.ThrowIfCancellationRequested();
                string strReturn;
                string strReturn1 = await LanguageManager
                    .TranslateExtraAsync(Extra, strLanguage, _objCharacter, token: token)
                    .ConfigureAwait(false);
                string strReturn2 = await LanguageManager
                    .TranslateExtraAsync(ExtraChoice1, strLanguage, _objCharacter, token: token)
                    .ConfigureAwait(false);
                string strReturn3 = await LanguageManager
                    .TranslateExtraAsync(ExtraChoice2, strLanguage, _objCharacter, token: token)
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
                    if (Interlocked.Exchange(ref _strExtra, value) != value && _objCharacter.MentorSpirits.Count > 0
                                                                            && _objCharacter.MentorSpirits[0] == this)
                        _objCharacter.OnPropertyChanged(nameof(Character.FirstMentorSpiritDisplayInformation));
                }
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
                    if (Interlocked.Exchange(ref _strExtraChoice1, value) != value && _objCharacter.MentorSpirits.Count > 0
                                                                            && _objCharacter.MentorSpirits[0] == this)
                        _objCharacter.OnPropertyChanged(nameof(Character.FirstMentorSpiritDisplayInformation));
                }
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
                    if (Interlocked.Exchange(ref _strExtraChoice2, value) != value && _objCharacter.MentorSpirits.Count > 0
                                                                            && _objCharacter.MentorSpirits[0] == this)
                        _objCharacter.OnPropertyChanged(nameof(Character.FirstMentorSpiritDisplayInformation));
                }
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
        public async ValueTask<string> DisplayAdvantageAsync(string strLanguage, CancellationToken token = default)
        {
            using (await LockObject.EnterReadLockAsync(token).ConfigureAwait(false))
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
        public async ValueTask<string> DisplayDisadvantageAsync(string strLanguage, CancellationToken token = default)
        {
            using (await LockObject.EnterReadLockAsync(token).ConfigureAwait(false))
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
        public async ValueTask<string> DisplayNameShortAsync(string strLanguage, CancellationToken token = default)
        {
            if (strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                return Name;

            using (await LockObject.EnterReadLockAsync(token).ConfigureAwait(false))
            {
                token.ThrowIfCancellationRequested();
                XPathNavigator objNode = await this.GetNodeXPathAsync(strLanguage, token: token).ConfigureAwait(false);
                return objNode != null
                    ? (await objNode.SelectSingleNodeAndCacheExpressionAsync("translate", token: token)
                                    .ConfigureAwait(false))?.Value ?? Name
                    : Name;
            }
        }

        public string CurrentDisplayNameShort => DisplayNameShort(GlobalSettings.Language);

        public string CurrentDisplayName => CurrentDisplayNameShort;

        public ValueTask<string> GetCurrentDisplayNameShortAsync(CancellationToken token = default)
        {
            return DisplayNameShortAsync(GlobalSettings.Language, token);
        }

        public ValueTask<string> GetCurrentDisplayNameAsync(CancellationToken token = default)
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
        public async ValueTask<string> DisplayPageAsync(string strLanguage, CancellationToken token = default)
        {
            if (strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                return Page;
            using (await LockObject.EnterReadLockAsync(token).ConfigureAwait(false))
            {
                token.ThrowIfCancellationRequested();
                XPathNavigator objNode = await this.GetNodeXPathAsync(strLanguage, token: token).ConfigureAwait(false);
                string s = objNode != null
                    ? (await objNode.SelectSingleNodeAndCacheExpressionAsync("altpage", token: token)
                                    .ConfigureAwait(false))?.Value ?? Page
                    : Page;
                return !string.IsNullOrWhiteSpace(s) ? s : Page;
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
                    _strNotes = value;
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
                using (LockObject.EnterUpgradeableReadLock())
                    _colNotes = value;
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

        private XmlNode _objCachedMyXmlNode;
        private string _strCachedXmlNodeLanguage = string.Empty;

        public async Task<XmlNode> GetNodeCoreAsync(bool blnSync, string strLanguage, CancellationToken token = default)
        {
            // ReSharper disable once MethodHasAsyncOverload
            using (blnSync ? LockObject.EnterReadLock(token) : await LockObject.EnterReadLockAsync(token).ConfigureAwait(false))
            {
                token.ThrowIfCancellationRequested();
                XmlNode objReturn = _objCachedMyXmlNode;
                if (objReturn != null && strLanguage == _strCachedXmlNodeLanguage
                                      && !GlobalSettings.LiveCustomData)
                    return objReturn;
                XmlNode objDoc = blnSync
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
                    objReturn = objDoc.TryGetNodeByNameOrId("/chummer/mentors/mentor", Name);
                    objReturn?.TryGetGuidFieldQuickly("id", ref _guiSourceID);
                }

                _objCachedMyXmlNode = objReturn;
                _strCachedXmlNodeLanguage = strLanguage;
                return objReturn;
            }
        }

        private XPathNavigator _objCachedMyXPathNode;
        private string _strCachedXPathNodeLanguage = string.Empty;

        public async Task<XPathNavigator> GetNodeXPathCoreAsync(bool blnSync, string strLanguage, CancellationToken token = default)
        {
            // ReSharper disable once MethodHasAsyncOverload
            using (blnSync ? LockObject.EnterReadLock(token) : await LockObject.EnterReadLockAsync(token).ConfigureAwait(false))
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
                    objReturn = objDoc.TryGetNodeByNameOrId("/chummer/mentors/mentor", Name);
                    objReturn?.TryGetGuidFieldQuickly("id", ref _guiSourceID);
                }
                _objCachedMyXPathNode = objReturn;
                _strCachedXPathNodeLanguage = strLanguage;
                return objReturn;
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

        public Task SetSourceDetailAsync(Control sourceControl, CancellationToken token = default)
        {
            if (_objCachedSourceDetail.Language != GlobalSettings.Language)
                _objCachedSourceDetail = default;
            return SourceDetail.SetControlAsync(sourceControl, token);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            LockObject.Dispose();
        }

        /// <inheritdoc />
        public ValueTask DisposeAsync()
        {
            return LockObject.DisposeAsync();
        }
    }
}
