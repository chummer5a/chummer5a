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
            XmlNode namenode = xmlNodeMentor?.SelectSingleNode("name");
            if (namenode != null)
                Name = namenode.InnerText;
            XmlNode typenode = xmlNodeMentor?.SelectSingleNode("mentortype");
            if (typenode != null && Enum.TryParse(typenode.InnerText, true, out Improvement.ImprovementType outEnum))
            {
                _eMentorType = outEnum;
            }
        }

        /// <summary>
        /// Create a Mentor Spirit from an XmlNode.
        /// </summary>
        /// <param name="xmlMentor">XmlNode to create the object from.</param>
        /// <param name="eMentorType">Whether this is a Mentor or a Paragon.</param>
        /// <param name="strForceValue">Force a value to be selected for the Mentor Spirit.</param>
        /// <param name="strForceValueChoice1">Name/Text for Choice 1.</param>
        /// <param name="strForceValueChoice2">Name/Text for Choice 2.</param>
        public void Create(XmlNode xmlMentor, Improvement.ImprovementType eMentorType, string strForceValue = "", string strForceValueChoice1 = "", string strForceValueChoice2 = "")
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

                if (GlobalSettings.InsertPdfNotesIfAvailable && string.IsNullOrEmpty(Notes))
                {
                    Notes = CommonFunctions.GetBookNotes(xmlMentor, Name, CurrentDisplayNameShort, Source, Page,
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
                    ImprovementManager.ForcedValue = strForceValue;
                    if (!ImprovementManager.CreateImprovements(_objCharacter,
                                                               Improvement.ImprovementSource.MentorSpirit,
                                                               _guiID.ToString(
                                                                   "D", GlobalSettings.InvariantCultureInfo), _nodBonus,
                                                               1, CurrentDisplayNameShort))
                    {
                        _guiID = Guid.Empty;
                        return;
                    }

                    _strExtra = ImprovementManager.SelectedValue;
                    ImprovementManager.ForcedValue = strOldForce;
                    ImprovementManager.SelectedValue = strOldSelected;
                }
                else if (!string.IsNullOrEmpty(strForceValue))
                {
                    _strExtra = strForceValue;
                }

                _nodChoice1 = xmlMentor.SelectSingleNode("choices/choice[name = " + strForceValueChoice1.CleanXPath()
                                                         + "]/bonus");
                if (_nodChoice1 != null)
                {
                    string strOldForce = ImprovementManager.ForcedValue;
                    string strOldSelected = ImprovementManager.SelectedValue;
                    //ImprovementManager.ForcedValue = strForceValueChoice1;
                    if (!ImprovementManager.CreateImprovements(_objCharacter,
                                                               Improvement.ImprovementSource.MentorSpirit,
                                                               _guiID.ToString(
                                                                   "D", GlobalSettings.InvariantCultureInfo),
                                                               _nodChoice1, 1, CurrentDisplayNameShort))
                    {
                        _guiID = Guid.Empty;
                        return;
                    }

                    if (string.IsNullOrEmpty(_strExtra))
                    {
                        _strExtra = ImprovementManager.SelectedValue;
                    }

                    ImprovementManager.ForcedValue = strOldForce;
                    ImprovementManager.SelectedValue = strOldSelected;
                }
                else if (string.IsNullOrEmpty(_strExtra) && !string.IsNullOrEmpty(strForceValueChoice1))
                {
                    _strExtra = strForceValueChoice1;
                }

                _nodChoice2 = xmlMentor.SelectSingleNode("choices/choice[name = " + strForceValueChoice2.CleanXPath()
                                                         + "]/bonus");
                if (_nodChoice2 != null)
                {
                    string strOldForce = ImprovementManager.ForcedValue;
                    string strOldSelected = ImprovementManager.SelectedValue;
                    //ImprovementManager.ForcedValue = strForceValueChoice2;
                    if (!ImprovementManager.CreateImprovements(_objCharacter,
                                                               Improvement.ImprovementSource.MentorSpirit,
                                                               _guiID.ToString(
                                                                   "D", GlobalSettings.InvariantCultureInfo),
                                                               _nodChoice2, 1, CurrentDisplayNameShort))
                    {
                        _guiID = Guid.Empty;
                        return;
                    }

                    if (string.IsNullOrEmpty(_strExtra))
                    {
                        _strExtra = ImprovementManager.SelectedValue;
                    }

                    ImprovementManager.ForcedValue = strOldForce;
                    ImprovementManager.SelectedValue = strOldSelected;
                }
                else if (string.IsNullOrEmpty(_strExtra) && !string.IsNullOrEmpty(strForceValueChoice2))
                {
                    _strExtra = strForceValueChoice2;
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
        /// Save the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        public void Save(XmlWriter objWriter)
        {
            if (objWriter == null)
                return;
            using (EnterReadLock.Enter(LockObject))
            {
                objWriter.WriteStartElement("mentorspirit");
                objWriter.WriteElementString("sourceid", SourceIDString);
                objWriter.WriteElementString("guid", InternalId);
                objWriter.WriteElementString("name", _strName);
                objWriter.WriteElementString("mentortype", _eMentorType.ToString());
                objWriter.WriteElementString("extra", _strExtra);
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
                                 .SelectSingleNode("/chummer/mentors/mentor[name = " + Name.CleanXPath() + ']')
                                 ?.TryGetGuidFieldQuickly("id", ref _guiSourceID);
                }

                objNode.TryGetStringFieldQuickly("extra", ref _strExtra);
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
            using (await EnterReadLock.EnterAsync(LockObject, token).ConfigureAwait(false))
            {
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
                using (EnterReadLock.Enter(LockObject))
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
                using (EnterReadLock.Enter(LockObject))
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
        /// Extra string related to improvements selected for the Mentor Spirit or Paragon.
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
                    if (Interlocked.Exchange(ref _strExtra, value) != value && _objCharacter.MentorSpirits.Count > 0
                                                                            && _objCharacter.MentorSpirits[0] == this)
                        _objCharacter.OnPropertyChanged(nameof(Character.FirstMentorSpiritDisplayName));
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
                using (EnterReadLock.Enter(LockObject))
                    return _blnMentorMask;
            }
            set
            {
                using (EnterReadLock.Enter(LockObject))
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
                using (EnterReadLock.Enter(LockObject))
                    return _strAdvantage;
            }
        }

        /// <summary>
        /// Advantage of the mentor as it should be displayed in the UI. Advantage (Extra).
        /// </summary>
        public string DisplayAdvantage(string strLanguage)
        {
            using (EnterReadLock.Enter(LockObject))
            {
                string strReturn = Advantage;
                if (strLanguage != GlobalSettings.DefaultLanguage)
                {
                    string strTemp = string.Empty;
                    if (this.GetNodeXPath(strLanguage)?.TryGetMultiLineStringFieldQuickly("altadvantage", ref strTemp)
                        == true)
                        strReturn = strTemp;
                }

                if (!string.IsNullOrEmpty(Extra))
                {
                    // Attempt to retrieve the CharacterAttribute name.
                    strReturn += LanguageManager.GetString("String_Space", strLanguage) + '('
                        + _objCharacter.TranslateExtra(Extra, strLanguage) + ')';
                }

                return strReturn;
            }
        }

        /// <summary>
        /// Advantage of the mentor as it should be displayed in the UI. Advantage (Extra).
        /// </summary>
        public async ValueTask<string> DisplayAdvantageAsync(string strLanguage, CancellationToken token = default)
        {
            using (await EnterReadLock.EnterAsync(LockObject, token).ConfigureAwait(false))
            {
                string strReturn = Advantage;
                if (strLanguage != GlobalSettings.DefaultLanguage)
                {
                    string strTemp = string.Empty;
                    if ((await this.GetNodeXPathAsync(strLanguage, token: token).ConfigureAwait(false))
                        ?.TryGetMultiLineStringFieldQuickly("altadvantage", ref strTemp) == true)
                        strReturn = strTemp;
                }

                if (!string.IsNullOrEmpty(Extra))
                {
                    // Attempt to retrieve the CharacterAttribute name.
                    strReturn
                        += await LanguageManager.GetStringAsync("String_Space", strLanguage, token: token)
                                                .ConfigureAwait(false) + '(' + await _objCharacter
                            .TranslateExtraAsync(Extra, strLanguage, token: token).ConfigureAwait(false) + ')';
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
                using (EnterReadLock.Enter(LockObject))
                    return _strDisadvantage;
            }
        }

        /// <summary>
        /// Disadvantage of the mentor as it should be displayed in the UI. Disadvantage (Extra).
        /// </summary>
        public string DisplayDisadvantage(string strLanguage)
        {
            using (EnterReadLock.Enter(LockObject))
            {
                string strReturn = Disadvantage;
                if (strLanguage != GlobalSettings.DefaultLanguage)
                {
                    string strTemp = string.Empty;
                    if (this.GetNodeXPath(strLanguage)
                            ?.TryGetMultiLineStringFieldQuickly("altdisadvantage", ref strTemp) == true)
                        strReturn = strTemp;
                }

                if (!string.IsNullOrEmpty(Extra))
                {
                    // Attempt to retrieve the CharacterAttribute name.
                    strReturn += LanguageManager.GetString("String_Space", strLanguage) + '('
                        + _objCharacter.TranslateExtra(Extra, strLanguage) + ')';
                }

                return strReturn;
            }
        }

        /// <summary>
        /// Disadvantage of the mentor as it should be displayed in the UI. Disadvantage (Extra).
        /// </summary>
        public async ValueTask<string> DisplayDisadvantageAsync(string strLanguage, CancellationToken token = default)
        {
            using (await EnterReadLock.EnterAsync(LockObject, token).ConfigureAwait(false))
            {
                string strReturn = Disadvantage;
                if (strLanguage != GlobalSettings.DefaultLanguage)
                {
                    string strTemp = string.Empty;
                    if ((await this.GetNodeXPathAsync(strLanguage, token: token).ConfigureAwait(false))
                        ?.TryGetMultiLineStringFieldQuickly("altdisadvantage", ref strTemp) == true)
                        strReturn = strTemp;
                }

                if (!string.IsNullOrEmpty(Extra))
                {
                    // Attempt to retrieve the CharacterAttribute name.
                    strReturn
                        += await LanguageManager.GetStringAsync("String_Space", strLanguage, token: token)
                                                .ConfigureAwait(false) + '(' + await _objCharacter
                            .TranslateExtraAsync(Extra, strLanguage, token: token).ConfigureAwait(false) + ')';
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

            using (EnterReadLock.Enter(LockObject))
                return this.GetNodeXPath(strLanguage)?.SelectSingleNodeAndCacheExpression("translate")?.Value ?? Name;
        }

        /// <summary>
        /// The name of the object as it should be displayed on printouts (translated name only).
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
                using (EnterReadLock.Enter(LockObject))
                    return _strSource;
            }
            set
            {
                using (EnterReadLock.Enter(LockObject))
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
                using (EnterReadLock.Enter(LockObject))
                    return _strPage;
            }
            set
            {
                using (EnterReadLock.Enter(LockObject))
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
        /// Notes.
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
                using (EnterReadLock.Enter(LockObject))
                    return _colNotes;
            }
            set
            {
                using (EnterReadLock.Enter(LockObject))
                    _colNotes = value;
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
            using (blnSync
                       // ReSharper disable once MethodHasAsyncOverload
                       ? EnterReadLock.Enter(LockObject, token)
                       : await EnterReadLock.EnterAsync(LockObject, token).ConfigureAwait(false))
            {
                XmlNode objReturn = _objCachedMyXmlNode;
                if (objReturn != null && strLanguage == _strCachedXmlNodeLanguage
                                      && !GlobalSettings.LiveCustomData)
                    return objReturn;
                objReturn = (blnSync
                        // ReSharper disable once MethodHasAsyncOverload
                        ? _objCharacter.LoadData(
                            _eMentorType == Improvement.ImprovementType.MentorSpirit
                                ? "mentors.xml"
                                : "paragons.xml", strLanguage, token: token)
                        : await _objCharacter.LoadDataAsync(
                            _eMentorType == Improvement.ImprovementType.MentorSpirit
                                ? "mentors.xml"
                                : "paragons.xml", strLanguage, token: token).ConfigureAwait(false))
                    .SelectSingleNode(SourceID == Guid.Empty
                                          ? "/chummer/mentors/mentor[name = " + Name.CleanXPath()
                                                                              + ']'
                                          : "/chummer/mentors/mentor[id = "
                                            + SourceIDString.CleanXPath()
                                            + " or id = " + SourceIDString.ToUpperInvariant()
                                                                          .CleanXPath()
                                            + ']');
                _objCachedMyXmlNode = objReturn;
                _strCachedXmlNodeLanguage = strLanguage;
                return objReturn;
            }
        }

        private XPathNavigator _objCachedMyXPathNode;
        private string _strCachedXPathNodeLanguage = string.Empty;

        public async Task<XPathNavigator> GetNodeXPathCoreAsync(bool blnSync, string strLanguage, CancellationToken token = default)
        {
            using (blnSync
                       // ReSharper disable once MethodHasAsyncOverload
                       ? EnterReadLock.Enter(LockObject, token)
                       : await EnterReadLock.EnterAsync(LockObject, token).ConfigureAwait(false))
            {
                XPathNavigator objReturn = _objCachedMyXPathNode;
                if (objReturn != null && strLanguage == _strCachedXPathNodeLanguage
                                      && !GlobalSettings.LiveCustomData)
                    return objReturn;
                objReturn = (blnSync
                        ? _objCharacter
                            // ReSharper disable once MethodHasAsyncOverload
                            .LoadDataXPath(
                                _eMentorType == Improvement.ImprovementType.MentorSpirit
                                    ? "mentors.xml"
                                    : "paragons.xml", strLanguage, token: token)
                        : await _objCharacter
                                .LoadDataXPathAsync(
                                    _eMentorType == Improvement.ImprovementType.MentorSpirit
                                        ? "mentors.xml"
                                        : "paragons.xml", strLanguage, token: token).ConfigureAwait(false))
                    .SelectSingleNode(SourceID == Guid.Empty
                                          ? "/chummer/mentors/mentor[name = " + Name.CleanXPath()
                                                                              + ']'
                                          : "/chummer/mentors/mentor[id = "
                                            + SourceIDString.CleanXPath()
                                            + " or id = " + SourceIDString.ToUpperInvariant()
                                                                          .CleanXPath()
                                            + ']');
                _objCachedMyXPathNode = objReturn;
                _strCachedXPathNodeLanguage = strLanguage;
                return objReturn;
            }
        }

        public string InternalId
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
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
