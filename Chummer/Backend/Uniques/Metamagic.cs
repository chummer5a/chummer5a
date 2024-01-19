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
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;
using NLog;

namespace Chummer
{
    /// <summary>
    /// A Metamagic or Echo.
    /// </summary>
    [HubClassTag("SourceID", true, "Name", null)]
    [DebuggerDisplay("{DisplayName(GlobalSettings.DefaultLanguage)}")]
    public class Metamagic : IHasInternalId, IHasName, IHasSourceId, IHasXmlDataNode, IHasNotes, ICanRemove, IHasSource
    {
        private static readonly Lazy<Logger> s_ObjLogger = new Lazy<Logger>(LogManager.GetCurrentClassLogger);
        private static Logger Log => s_ObjLogger.Value;

        private Guid _guiID;
        private Guid _guiSourceID;
        private string _strName = string.Empty;
        private string _strSource = string.Empty;
        private string _strPage = string.Empty;
        private bool _blnPaidWithKarma;
        private int _intGrade;
        private XmlNode _nodBonus;
        private Improvement.ImprovementSource _eImprovementSource = Improvement.ImprovementSource.Metamagic;
        private string _strNotes = string.Empty;
        private Color _colNotes = ColorManager.HasNotesColor;

        private readonly Character _objCharacter;

        #region Constructor, Create, Save, Load, and Print Methods

        public Metamagic(Character objCharacter)
        {
            // Create the GUID for the new piece of Cyberware.
            _guiID = Guid.NewGuid();
            _objCharacter = objCharacter;
        }

        /// <summary>
        /// Create a Metamagic from an XmlNode.
        /// </summary>
        /// <param name="objXmlMetamagicNode">XmlNode to create the object from.</param>
        /// <param name="objSource">Source of the Improvement.</param>
        /// <param name="strForcedValue">Value to forcefully select for any ImprovementManager prompts.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public void Create(XmlNode objXmlMetamagicNode, Improvement.ImprovementSource objSource,
            string strForcedValue = "", CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (!objXmlMetamagicNode.TryGetField("id", Guid.TryParse, out _guiSourceID))
            {
                Log.Warn(new object[] { "Missing id field for xmlnode", objXmlMetamagicNode });
                Utils.BreakIfDebug();
            }

            if (objXmlMetamagicNode.TryGetStringFieldQuickly("name", ref _strName))
            {
                _objCachedMyXmlNode = null;
                _objCachedMyXPathNode = null;
            }

            objXmlMetamagicNode.TryGetStringFieldQuickly("source", ref _strSource);
            objXmlMetamagicNode.TryGetStringFieldQuickly("page", ref _strPage);
            _eImprovementSource = objSource;
            objXmlMetamagicNode.TryGetInt32FieldQuickly("grade", ref _intGrade);
            if (!objXmlMetamagicNode.TryGetMultiLineStringFieldQuickly("altnotes", ref _strNotes))
                objXmlMetamagicNode.TryGetMultiLineStringFieldQuickly("notes", ref _strNotes);

            string sNotesColor = ColorTranslator.ToHtml(ColorManager.HasNotesColor);
            objXmlMetamagicNode.TryGetStringFieldQuickly("notesColor", ref sNotesColor);
            _colNotes = ColorTranslator.FromHtml(sNotesColor);

            if (GlobalSettings.InsertPdfNotesIfAvailable && string.IsNullOrEmpty(Notes))
            {
                Notes = CommonFunctions.GetBookNotes(objXmlMetamagicNode, Name, CurrentDisplayName, Source, Page,
                    DisplayPage(GlobalSettings.Language), _objCharacter, token);
            }

            _nodBonus = objXmlMetamagicNode["bonus"];
            if (_nodBonus != null)
            {
                int intRating = _objCharacter.SubmersionGrade > 0
                    ? _objCharacter.SubmersionGrade
                    : _objCharacter.InitiateGrade;

                string strOldFocedValue = ImprovementManager.ForcedValue;
                string strOldSelectedValue = ImprovementManager.SelectedValue;
                ImprovementManager.ForcedValue = strForcedValue;
                if (!ImprovementManager.CreateImprovements(_objCharacter, objSource,
                        _guiID.ToString("D", GlobalSettings.InvariantCultureInfo), _nodBonus, intRating,
                        CurrentDisplayNameShort, token: token))
                {
                    _guiID = Guid.Empty;
                    ImprovementManager.ForcedValue = strOldFocedValue;
                    return;
                }

                if (!string.IsNullOrEmpty(ImprovementManager.SelectedValue))
                {
                    _strName += LanguageManager.GetString("String_Space", token: token) + '(' +
                                ImprovementManager.SelectedValue + ')';
                    _objCachedMyXmlNode = null;
                    _objCachedMyXPathNode = null;
                }

                ImprovementManager.ForcedValue = strOldFocedValue;
                ImprovementManager.SelectedValue = strOldSelectedValue;
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

        /// <summary>
        /// Create a Metamagic from an XmlNode.
        /// </summary>
        /// <param name="objXmlMetamagicNode">XmlNode to create the object from.</param>
        /// <param name="objSource">Source of the Improvement.</param>
        /// <param name="strForcedValue">Value to forcefully select for any ImprovementManager prompts.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public async Task CreateAsync(XmlNode objXmlMetamagicNode, Improvement.ImprovementSource objSource,
            string strForcedValue = "", CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (!objXmlMetamagicNode.TryGetField("id", Guid.TryParse, out _guiSourceID))
            {
                Log.Warn(new object[] { "Missing id field for xmlnode", objXmlMetamagicNode });
                Utils.BreakIfDebug();
            }

            if (objXmlMetamagicNode.TryGetStringFieldQuickly("name", ref _strName))
            {
                _objCachedMyXmlNode = null;
                _objCachedMyXPathNode = null;
            }

            objXmlMetamagicNode.TryGetStringFieldQuickly("source", ref _strSource);
            objXmlMetamagicNode.TryGetStringFieldQuickly("page", ref _strPage);
            _eImprovementSource = objSource;
            objXmlMetamagicNode.TryGetInt32FieldQuickly("grade", ref _intGrade);
            if (!objXmlMetamagicNode.TryGetMultiLineStringFieldQuickly("altnotes", ref _strNotes))
                objXmlMetamagicNode.TryGetMultiLineStringFieldQuickly("notes", ref _strNotes);

            string sNotesColor = ColorTranslator.ToHtml(ColorManager.HasNotesColor);
            objXmlMetamagicNode.TryGetStringFieldQuickly("notesColor", ref sNotesColor);
            _colNotes = ColorTranslator.FromHtml(sNotesColor);

            if (GlobalSettings.InsertPdfNotesIfAvailable && string.IsNullOrEmpty(Notes))
            {
                Notes = await CommonFunctions.GetBookNotesAsync(objXmlMetamagicNode, Name,
                        await GetCurrentDisplayNameAsync(token).ConfigureAwait(false), Source, Page,
                        await DisplayPageAsync(GlobalSettings.Language, token).ConfigureAwait(false), _objCharacter,
                        token)
                    .ConfigureAwait(false);
            }

            _nodBonus = objXmlMetamagicNode["bonus"];
            if (_nodBonus != null)
            {
                int intSubmersionGrade = await _objCharacter.GetSubmersionGradeAsync(token).ConfigureAwait(false);
                int intRating = intSubmersionGrade > 0
                    ? intSubmersionGrade
                    : await _objCharacter.GetInitiateGradeAsync(token).ConfigureAwait(false);

                string strOldFocedValue = ImprovementManager.ForcedValue;
                string strOldSelectedValue = ImprovementManager.SelectedValue;
                ImprovementManager.ForcedValue = strForcedValue;
                if (!await ImprovementManager.CreateImprovementsAsync(_objCharacter, objSource,
                            _guiID.ToString("D", GlobalSettings.InvariantCultureInfo), _nodBonus, intRating,
                            await GetCurrentDisplayNameShortAsync(token).ConfigureAwait(false), token: token)
                        .ConfigureAwait(false))
                {
                    _guiID = Guid.Empty;
                    ImprovementManager.ForcedValue = strOldFocedValue;
                    return;
                }

                if (!string.IsNullOrEmpty(ImprovementManager.SelectedValue))
                {
                    _strName +=
                        await LanguageManager.GetStringAsync("String_Space", token: token).ConfigureAwait(false) + '(' +
                        ImprovementManager.SelectedValue + ')';
                    _objCachedMyXmlNode = null;
                    _objCachedMyXPathNode = null;
                }

                ImprovementManager.ForcedValue = strOldFocedValue;
                ImprovementManager.SelectedValue = strOldSelectedValue;
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

        private SourceString _objCachedSourceDetail;

        public SourceString SourceDetail =>
            _objCachedSourceDetail == default
                ? _objCachedSourceDetail = SourceString.GetSourceString(Source,
                    DisplayPage(GlobalSettings.Language),
                    GlobalSettings.Language,
                    GlobalSettings.CultureInfo,
                    _objCharacter)
                : _objCachedSourceDetail;

        public async Task<SourceString> GetSourceDetailAsync(CancellationToken token = default)
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

        /// <summary>
        /// Save the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        public void Save(XmlWriter objWriter)
        {
            if (objWriter == null)
                return;
            objWriter.WriteStartElement("metamagic");
            objWriter.WriteElementString("sourceid", SourceIDString);
            objWriter.WriteElementString("guid", InternalId);
            objWriter.WriteElementString("name", _strName);
            objWriter.WriteElementString("source", _strSource);
            objWriter.WriteElementString("paidwithkarma", _blnPaidWithKarma.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("page", _strPage);
            objWriter.WriteElementString("grade", _intGrade.ToString(GlobalSettings.InvariantCultureInfo));
            if (_nodBonus != null)
                objWriter.WriteRaw(_nodBonus.OuterXml);
            else
                objWriter.WriteElementString("bonus", string.Empty);
            objWriter.WriteElementString("improvementsource", _eImprovementSource.ToString());
            objWriter.WriteElementString("notes", _strNotes.CleanOfInvalidUnicodeChars());
            objWriter.WriteElementString("notesColor", ColorTranslator.ToHtml(_colNotes));
            objWriter.WriteEndElement();
        }

        /// <summary>
        /// Load the Metamagic from the XmlNode.
        /// </summary>
        /// <param name="objNode">XmlNode to load.</param>
        public void Load(XmlNode objNode)
        {
            if (!objNode.TryGetField("guid", Guid.TryParse, out _guiID))
            {
                _guiID = Guid.NewGuid();
            }

            objNode.TryGetStringFieldQuickly("name", ref _strName);
            _objCachedMyXmlNode = null;
            _objCachedMyXPathNode = null;

            if (!objNode.TryGetGuidFieldQuickly("sourceid", ref _guiSourceID))
            {
                this.GetNodeXPath()?.TryGetGuidFieldQuickly("id", ref _guiSourceID);
            }
            objNode.TryGetStringFieldQuickly("source", ref _strSource);
            objNode.TryGetStringFieldQuickly("page", ref _strPage);
            objNode.TryGetBoolFieldQuickly("paidwithkarma", ref _blnPaidWithKarma);
            objNode.TryGetInt32FieldQuickly("grade", ref _intGrade);

            _nodBonus = objNode["bonus"];
            if (objNode["improvementsource"] != null)
                SourceType = Improvement.ConvertToImprovementSource(objNode["improvementsource"].InnerText);

            objNode.TryGetMultiLineStringFieldQuickly("notes", ref _strNotes);

            string sNotesColor = ColorTranslator.ToHtml(ColorManager.HasNotesColor);
            objNode.TryGetStringFieldQuickly("notesColor", ref sNotesColor);
            _colNotes = ColorTranslator.FromHtml(sNotesColor);
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
            // <metamagic>
            XmlElementWriteHelper objBaseElement = await objWriter.StartElementAsync("metamagic", token).ConfigureAwait(false);
            try
            {
                await objWriter.WriteElementStringAsync("guid", InternalId, token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("sourceid", SourceIDString, token).ConfigureAwait(false);
                await objWriter
                      .WriteElementStringAsync(
                          "name", await DisplayNameShortAsync(strLanguageToPrint, token).ConfigureAwait(false), token)
                      .ConfigureAwait(false);
                await objWriter
                      .WriteElementStringAsync(
                          "fullname", await DisplayNameAsync(strLanguageToPrint, token).ConfigureAwait(false), token)
                      .ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("name_english", Name, token).ConfigureAwait(false);
                await objWriter
                      .WriteElementStringAsync(
                          "source",
                          await _objCharacter.LanguageBookShortAsync(Source, strLanguageToPrint, token)
                                             .ConfigureAwait(false), token).ConfigureAwait(false);
                await objWriter
                      .WriteElementStringAsync(
                          "page", await DisplayPageAsync(strLanguageToPrint, token).ConfigureAwait(false), token)
                      .ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("grade", Grade.ToString(objCulture), token)
                               .ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("improvementsource", _eImprovementSource.ToString(), token)
                               .ConfigureAwait(false);
                if (GlobalSettings.PrintNotes)
                    await objWriter.WriteElementStringAsync("notes", Notes, token).ConfigureAwait(false);
            }
            finally
            {
                // </metamagic>
                await objBaseElement.DisposeAsync().ConfigureAwait(false);
            }
        }

        #endregion Constructor, Create, Save, Load, and Print Methods

        #region Properties

        /// <summary>
        /// Identifier of the object within data files.
        /// </summary>
        public Guid SourceID => _guiSourceID;

        /// <summary>
        /// String-formatted identifier of the <inheritdoc cref="SourceID"/> from the data files.
        /// </summary>
        public string SourceIDString => _guiSourceID.ToString("D", GlobalSettings.InvariantCultureInfo);

        /// <summary>
        /// Internal identifier which will be used to identify this Metamagic in the Improvement system.
        /// </summary>
        public string InternalId => _guiID.ToString("D", GlobalSettings.InvariantCultureInfo);

        /// <summary>
        /// Bonus node from the XML file.
        /// </summary>
        public XmlNode Bonus
        {
            get => _nodBonus;
            set => _nodBonus = value;
        }

        /// <summary>
        /// ImprovementSource Type.
        /// </summary>
        public Improvement.ImprovementSource SourceType
        {
            get => _eImprovementSource;
            set
            {
                if (InterlockedExtensions.Exchange(ref _eImprovementSource, value) == value)
                    return;
                _objCachedMyXmlNode = null;
                _objCachedMyXPathNode = null;
            }
        }

        /// <summary>
        /// Metamagic name.
        /// </summary>
        public string Name
        {
            get => _strName;
            set
            {
                if (Interlocked.Exchange(ref _strName, value) == value)
                    return;
                if (SourceID == Guid.Empty)
                {
                    _objCachedMyXmlNode = null;
                    _objCachedMyXPathNode = null;
                }
            }
        }

        /// <summary>
        /// The name of the object as it should be displayed on printouts (translated name only).
        /// </summary>
        public string DisplayNameShort(string strLanguage)
        {
            // Get the translated name if applicable.
            if (strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                return Name;

            return this.GetNodeXPath(strLanguage)?.SelectSingleNodeAndCacheExpression("translate")?.Value ?? Name;
        }

        /// <summary>
        /// The name of the object as it should be displayed on printouts (translated name only).
        /// </summary>
        public async Task<string> DisplayNameShortAsync(string strLanguage, CancellationToken token = default)
        {
            // Get the translated name if applicable.
            if (strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                return Name;

            XPathNavigator objNode = await this.GetNodeXPathAsync(strLanguage, token: token).ConfigureAwait(false);
            return objNode?.SelectSingleNodeAndCacheExpression("translate", token: token)?.Value ?? Name;
        }

        /// <summary>
        /// The name of the object as it should be displayed on printouts in the program's current language.
        /// </summary>
        public string CurrentDisplayNameShort => DisplayNameShort(GlobalSettings.Language);

        public Task<string> GetCurrentDisplayNameShortAsync(CancellationToken token = default) => DisplayNameShortAsync(GlobalSettings.Language, token);

        /// <summary>
        /// The name of the object as it should be displayed in lists. Name (Extra).
        /// </summary>
        public string DisplayName(string strLanguage)
        {
            string strReturn = DisplayNameShort(strLanguage);

            return strReturn;
        }

        /// <summary>
        /// The name of the object as it should be displayed in lists. Name (Extra).
        /// </summary>
        public Task<string> DisplayNameAsync(string strLanguage, CancellationToken token = default)
        {
            return DisplayNameShortAsync(strLanguage, token);
        }

        /// <summary>
        /// The name of the object as it should be displayed in lists in the program's current language.
        /// </summary>
        public string CurrentDisplayName => DisplayName(GlobalSettings.Language);

        public Task<string> GetCurrentDisplayNameAsync(CancellationToken token = default) => DisplayNameAsync(GlobalSettings.Language, token);

        /// <summary>
        /// Grade to which the Metamagic is tied. Negative if the Metamagic was added by an Improvement and not by an Initiation/Submersion.
        /// </summary>
        public int Grade
        {
            get => _intGrade;
            set => _intGrade = value;
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
            XPathNavigator objNode = await this.GetNodeXPathAsync(strLanguage, token: token).ConfigureAwait(false);
            string strReturn = objNode?.SelectSingleNodeAndCacheExpression("altpage", token: token)?.Value ?? Page;
            return !string.IsNullOrWhiteSpace(strReturn) ? strReturn : Page;
        }

        /// <summary>
        /// Whether the Metamagic was paid for with Karma.
        /// </summary>
        public bool PaidWithKarma
        {
            get => _blnPaidWithKarma;
            set => _blnPaidWithKarma = value;
        }

        /// <summary>
        /// Notes.
        /// </summary>
        public string Notes
        {
            get => _strNotes;
            set => _strNotes = value;
        }

        /// <summary>
        /// Forecolor to use for Notes in treeviews.
        /// </summary>
        public Color NotesColor
        {
            get => _colNotes;
            set => _colNotes = value;
        }

        private XmlNode _objCachedMyXmlNode;
        private string _strCachedXmlNodeLanguage = string.Empty;

        public async Task<XmlNode> GetNodeCoreAsync(bool blnSync, string strLanguage, CancellationToken token = default)
        {
            XmlNode objReturn = _objCachedMyXmlNode;
            if (objReturn != null && strLanguage == _strCachedXmlNodeLanguage
                                  && !GlobalSettings.LiveCustomData)
                return objReturn;
            string strDoc = "metamagic.xml";
            string strPath = "/chummer/metamagics/metamagic";
            if (_eImprovementSource == Improvement.ImprovementSource.Echo)
            {
                strDoc = "echoes.xml";
                strPath = "/chummer/echoes/echo";
            }

            XmlDocument objDoc = blnSync
                // ReSharper disable once MethodHasAsyncOverload
                ? _objCharacter.LoadData(strDoc, strLanguage, token: token)
                : await _objCharacter.LoadDataAsync(strDoc, strLanguage, token: token)
                                     .ConfigureAwait(false);
            if (SourceID != Guid.Empty)
                objReturn = objDoc.TryGetNodeById(strPath, SourceID);
            if (objReturn == null)
            {
                objReturn = objDoc.TryGetNodeByNameOrId(strPath, Name);
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
            string strDoc = "metamagic.xml";
            string strPath = "/chummer/metamagics/metamagic";
            if (_eImprovementSource == Improvement.ImprovementSource.Echo)
            {
                strDoc = "echoes.xml";
                strPath = "/chummer/echoes/echo";
            }

            XPathNavigator objDoc = blnSync
                // ReSharper disable once MethodHasAsyncOverload
                ? _objCharacter.LoadDataXPath(strDoc, strLanguage, token: token)
                : await _objCharacter.LoadDataXPathAsync(strDoc, strLanguage, token: token)
                                     .ConfigureAwait(false);
            if (SourceID != Guid.Empty)
                objReturn = objDoc.TryGetNodeById(strPath, SourceID);
            if (objReturn == null)
            {
                objReturn = objDoc.TryGetNodeByNameOrId(strPath, Name);
                objReturn?.TryGetGuidFieldQuickly("id", ref _guiSourceID);
            }
            _objCachedMyXPathNode = objReturn;
            _strCachedXPathNodeLanguage = strLanguage;
            return objReturn;
        }

        #endregion Properties

        #region UI Methods

        public TreeNode CreateTreeNode(ContextMenuStrip cmsMetamagic, bool blnAddCategory = false)
        {
            if (Grade == -1 && !string.IsNullOrEmpty(Source) && !_objCharacter.Settings.BookEnabled(Source))
                return null;

            string strText = CurrentDisplayName;
            if (blnAddCategory)
                strText = LanguageManager.GetString(SourceType == Improvement.ImprovementSource.Metamagic ? "Label_Metamagic" : "Label_Echo") + LanguageManager.GetString("String_Space") + strText;
            TreeNode objNode = new TreeNode
            {
                Name = InternalId,
                Text = strText,
                Tag = this,
                ContextMenuStrip = cmsMetamagic,
                ForeColor = PreferredColor,
                ToolTipText = Notes.WordWrap()
            };

            return objNode;
        }

        public Color PreferredColor
        {
            get
            {
                if (!string.IsNullOrEmpty(Notes))
                {
                    return Grade == -1
                        ? ColorManager.GenerateCurrentModeDimmedColor(NotesColor)
                        : ColorManager.GenerateCurrentModeColor(NotesColor);
                }
                return Grade == -1
                    ? ColorManager.GrayText
                    : ColorManager.WindowText;
            }
        }

        #endregion UI Methods

        public bool Remove(bool blnConfirmDelete = true)
        {
            if (blnConfirmDelete)
            {
                if (Grade != 0) // If we are prompting, we are not removing this by removing the initiation/submersion that granted it
                    return false;
                string strMessage;
                if (_objCharacter.MAGEnabled)
                    strMessage = LanguageManager.GetString("Message_DeleteMetamagic");
                else if (_objCharacter.RESEnabled)
                    strMessage = LanguageManager.GetString("Message_DeleteEcho");
                else
                    return false;
                if (!CommonFunctions.ConfirmDelete(strMessage))
                    return false;
            }

            _objCharacter.Metamagics.Remove(this);
            ImprovementManager.RemoveImprovements(_objCharacter, SourceType, InternalId);
            return true;
        }

        public async Task<bool> RemoveAsync(bool blnConfirmDelete = true, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (blnConfirmDelete)
            {
                if (Grade != 0) // If we are prompting, we are not removing this by removing the initiation/submersion that granted it
                    return false;
                string strMessage;
                if (await _objCharacter.GetMAGEnabledAsync(token).ConfigureAwait(false))
                    strMessage = await LanguageManager.GetStringAsync("Message_DeleteMetamagic", token: token)
                                                      .ConfigureAwait(false);
                else if (await _objCharacter.GetRESEnabledAsync(token).ConfigureAwait(false))
                    strMessage = await LanguageManager.GetStringAsync("Message_DeleteEcho", token: token)
                                                      .ConfigureAwait(false);
                else
                    return false;
                if (!await CommonFunctions.ConfirmDeleteAsync(strMessage, token).ConfigureAwait(false))
                    return false;
            }

            await _objCharacter.Metamagics.RemoveAsync(this, token).ConfigureAwait(false);
            await ImprovementManager.RemoveImprovementsAsync(_objCharacter, SourceType, InternalId, token)
                                    .ConfigureAwait(false);
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
    }
}
