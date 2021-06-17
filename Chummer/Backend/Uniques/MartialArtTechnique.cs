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
using System.Windows.Forms;
using System.Xml;
using NLog;

namespace Chummer
{
    /// <summary>
    /// A Martial Arts Technique.
    /// </summary>
    [DebuggerDisplay("{DisplayName(GlobalOptions.DefaultLanguage)}")]
    public class MartialArtTechnique : IHasInternalId, IHasName, IHasXmlNode, IHasNotes, ICanRemove, IHasSource
    {
        private readonly Logger Log = LogManager.GetCurrentClassLogger();
        private Guid _guiID;
        private Guid _guiSourceID;
        private string _strName = string.Empty;
        private string _strNotes = string.Empty;
        private Color _colNotes = ColorManager.HasNotesColor;
        private string _strSource = string.Empty;
        private string _strPage = string.Empty;
        private readonly Character _objCharacter;

        #region Constructor, Create, Save, Load, and Print Methods
        public MartialArtTechnique(Character objCharacter)
        {
            // Create the GUID for the new Martial Art Technique.
            _guiID = Guid.NewGuid();
            _objCharacter = objCharacter;
        }

        /// Create a Martial Art Technique from an XmlNode.
        /// <param name="xmlTechniqueDataNode">XmlNode to create the object from.</param>
        public void Create(XmlNode xmlTechniqueDataNode)
        {
            if (!xmlTechniqueDataNode.TryGetField("id", Guid.TryParse, out _guiSourceID))
            {
                Log.Warn(new object[] {"Missing id field for xmlnode", xmlTechniqueDataNode});
                Utils.BreakIfDebug();
            }

            if (xmlTechniqueDataNode.TryGetStringFieldQuickly("name", ref _strName) && !xmlTechniqueDataNode.TryGetMultiLineStringFieldQuickly("altnotes", ref _strNotes))
                xmlTechniqueDataNode.TryGetMultiLineStringFieldQuickly("notes", ref _strNotes);

            String sNotesColor = ColorTranslator.ToHtml(ColorManager.HasNotesColor);
            xmlTechniqueDataNode.TryGetStringFieldQuickly("notesColor", ref sNotesColor);
            _colNotes = ColorTranslator.FromHtml(sNotesColor);

            xmlTechniqueDataNode.TryGetStringFieldQuickly("source", ref _strSource);
            xmlTechniqueDataNode.TryGetStringFieldQuickly("page", ref _strPage);

            if (string.IsNullOrEmpty(Notes))
            {
                string strEnglishNameOnPage = Name;
                string strNameOnPage = string.Empty;
                // make sure we have something and not just an empty tag
                if (xmlTechniqueDataNode.TryGetStringFieldQuickly("nameonpage", ref strNameOnPage) &&
                    !string.IsNullOrEmpty(strNameOnPage))
                    strEnglishNameOnPage = strNameOnPage;

                string strQualityNotes = CommonFunctions.GetTextFromPdf(Source + ' ' + Page, strEnglishNameOnPage, _objCharacter);

                if (string.IsNullOrEmpty(strQualityNotes) && !GlobalOptions.Language.Equals(GlobalOptions.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                {
                    string strTranslatedNameOnPage = CurrentDisplayName;

                    // don't check again it is not translated
                    if (strTranslatedNameOnPage != _strName)
                    {
                        // if we found <altnameonpage>, and is not empty and not the same as english we must use that instead
                        if (xmlTechniqueDataNode.TryGetStringFieldQuickly("altnameonpage", ref strNameOnPage)
                            && !string.IsNullOrEmpty(strNameOnPage) && strNameOnPage != strEnglishNameOnPage)
                            strTranslatedNameOnPage = strNameOnPage;

                        Notes = CommonFunctions.GetTextFromPdf(Source + ' ' + DisplayPage(GlobalOptions.Language),
                            strTranslatedNameOnPage, _objCharacter);
                    }
                }
                else
                    Notes = strQualityNotes;
            }

            if (xmlTechniqueDataNode["bonus"] == null) return;
            if (!ImprovementManager.CreateImprovements(_objCharacter, Improvement.ImprovementSource.MartialArtTechnique,
                _guiID.ToString("D", GlobalOptions.InvariantCultureInfo), xmlTechniqueDataNode["bonus"], 1, CurrentDisplayName))
            {
                _guiID = Guid.Empty;
            }
        }

        private SourceString _objCachedSourceDetail;
        public SourceString SourceDetail
        {
            get
            {
                if (_objCachedSourceDetail == default)
                    _objCachedSourceDetail = new SourceString(Source, DisplayPage(GlobalOptions.Language),
                        GlobalOptions.Language, GlobalOptions.CultureInfo, _objCharacter);
                return _objCachedSourceDetail;
            }
        }

        /// <summary>
        /// Save the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        public void Save(XmlTextWriter objWriter)
        {
            if (objWriter == null)
                return;
            objWriter.WriteStartElement("martialarttechnique");
            objWriter.WriteElementString("sourceid", SourceIDString);
            objWriter.WriteElementString("guid", InternalId);
            objWriter.WriteElementString("name", _strName);
            objWriter.WriteElementString("notes", System.Text.RegularExpressions.Regex.Replace(_strNotes, @"[\u0000-\u0008\u000B\u000C\u000E-\u001F]", ""));
            objWriter.WriteElementString("notesColor", ColorTranslator.ToHtml(_colNotes));
            objWriter.WriteElementString("source", _strSource);
            objWriter.WriteElementString("page", _strPage);
            objWriter.WriteEndElement();

            _objCharacter.SourceProcess(_strSource);
        }

        /// <summary>
        /// Load the Martial Art Technique from the XmlNode.
        /// </summary>
        /// <param name="objNode">XmlNode to load.</param>
        public void Load(XmlNode objNode)
        {
            if (!objNode.TryGetField("guid", Guid.TryParse, out _guiID))
            {
                _guiID = Guid.NewGuid();
            }
            objNode.TryGetStringFieldQuickly("name", ref _strName);
            if(!objNode.TryGetGuidFieldQuickly("sourceid", ref _guiSourceID))
            {
                XmlNode node = GetNode(GlobalOptions.Language);
                node?.TryGetGuidFieldQuickly("id", ref _guiSourceID);
            }
            objNode.TryGetStringFieldQuickly("source", ref _strSource);
            objNode.TryGetStringFieldQuickly("page", ref _strPage);
            objNode.TryGetMultiLineStringFieldQuickly("notes", ref _strNotes);

            String sNotesColor = ColorTranslator.ToHtml(ColorManager.HasNotesColor);
            objNode.TryGetStringFieldQuickly("notesColor", ref sNotesColor);
            _colNotes = ColorTranslator.FromHtml(sNotesColor);
        }

        /// <summary>
        /// Print the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        /// <param name="strLanguageToPrint">Language in which to print</param>
        public void Print(XmlTextWriter objWriter, string strLanguageToPrint)
        {
            if (objWriter == null)
                return;
            objWriter.WriteStartElement("martialarttechnique");
            objWriter.WriteElementString("guid", InternalId);
            objWriter.WriteElementString("sourceid", SourceIDString);
            objWriter.WriteElementString("name", DisplayName(strLanguageToPrint));
            objWriter.WriteElementString("name_english", Name);
            if (GlobalOptions.PrintNotes)
                objWriter.WriteElementString("notes", Notes);
            objWriter.WriteElementString("source", Source);
            objWriter.WriteElementString("page", DisplayPage(strLanguageToPrint));
            objWriter.WriteEndElement();
        }
        #endregion

        #region Properties
        /// <summary>
        /// Internal identifier which will be used to identify this Martial Art Technique in the Improvement system.
        /// </summary>
        public string InternalId => _guiID.ToString("D", GlobalOptions.InvariantCultureInfo);
        /// <summary>
        /// Identifier of the object within data files.
        /// </summary>
        public Guid SourceID
        {
            get => _guiSourceID;
            set
            {
                if (_guiSourceID == value) return;
                _guiSourceID = value;
                _objCachedMyXmlNode = null;
            }
        }

        /// <summary>
        /// String-formatted identifier of the <inheritdoc cref="SourceID"/> from the data files.
        /// </summary>
        public string SourceIDString => _guiSourceID.ToString("D", GlobalOptions.InvariantCultureInfo);

        /// <summary>
        /// Name.
        /// </summary>
        public string Name
        {
            get => _strName;
            set => _strName = value;
        }

        /// <summary>
        /// The name of the object as it should be displayed on printouts (translated name only).
        /// </summary>
        public string DisplayName(string strLanguage)
        {
            // Get the translated name if applicable.
            if (strLanguage.Equals(GlobalOptions.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                return Name;

            return GetNode(strLanguage)?["translate"]?.InnerText ?? Name;
        }

        public string CurrentDisplayName => DisplayName(GlobalOptions.Language);

        /// <summary>
        /// Notes attached to this technique.
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
            if (strLanguage.Equals(GlobalOptions.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                return Page;
            string s = GetNode(strLanguage)?["altpage"]?.InnerText ?? Page;
            return !string.IsNullOrWhiteSpace(s) ? s : Page;
        }

        private XmlNode _objCachedMyXmlNode;
        private string _strCachedXmlNodeLanguage = string.Empty;

        public XmlNode GetNode()
        {
            return GetNode(GlobalOptions.Language);
        }

        public XmlNode GetNode(string strLanguage)
        {
            if (_objCachedMyXmlNode == null || strLanguage != _strCachedXmlNodeLanguage || GlobalOptions.LiveCustomData)
            {
                _objCachedMyXmlNode = _objCharacter.LoadData("martialarts.xml", strLanguage)
                    .SelectSingleNode(SourceID == Guid.Empty
                        ? "/chummer/techniques/technique[name = " + Name.CleanXPath() + ']'
                        : string.Format(GlobalOptions.InvariantCultureInfo,
                            "/chummer/techniques/technique[id = {0} or id = {1}]",
                            SourceIDString.CleanXPath(), SourceIDString.ToUpperInvariant().CleanXPath()));
                _strCachedXmlNodeLanguage = strLanguage;
            }
            return _objCachedMyXmlNode;
        }
        #endregion

        #region Methods

        public bool Remove(bool blnConfirmDelete = true)
        {
            if (blnConfirmDelete && !CommonFunctions.ConfirmDelete(LanguageManager.GetString("Message_DeleteMartialArt")))
                return false;
            // Find the selected Technique object.
            //TODO: Techniques should know what their parent is.
            _objCharacter.MartialArts.FindMartialArtTechnique(InternalId, out MartialArt objMartialArt);

            ImprovementManager.RemoveImprovements(_objCharacter,
                Improvement.ImprovementSource.MartialArtTechnique, InternalId);

            objMartialArt.Techniques.Remove(this);
            return true;
        }
        #endregion

        #region UI Methods
        public TreeNode CreateTreeNode(ContextMenuStrip cmsMartialArtTechnique)
        {
            //if (!string.IsNullOrEmpty(ParentID) && !string.IsNullOrEmpty(Source) && !_objCharacter.Options.BookEnabled(Source))
            //return null;

            TreeNode objNode = new TreeNode
            {
                Name = InternalId,
                Text = CurrentDisplayName,
                Tag = this,
                ContextMenuStrip = cmsMartialArtTechnique,
                ForeColor = PreferredColor,
                ToolTipText = Notes.WordWrap()
            };

            return objNode;
        }

        public Color PreferredColor =>
            !string.IsNullOrEmpty(Notes)
                ? ColorManager.GenerateCurrentModeColor(NotesColor)
                : ColorManager.WindowText;
        #endregion

        public void SetSourceDetail(Control sourceControl)
        {
            if (_objCachedSourceDetail.Language != GlobalOptions.Language)
                _objCachedSourceDetail = default;
            SourceDetail.SetControl(sourceControl);
        }
    }
}
