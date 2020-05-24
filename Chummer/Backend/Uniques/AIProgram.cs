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
    /// An AI Program or Advanced Program.
    /// </summary>
    [HubClassTag("SourceID", true, "Name", "Extra")]
    [DebuggerDisplay("{DisplayNameShort(GlobalOptions.DefaultLanguage)}")]
    public class AIProgram : IHasInternalId, IHasName, IHasXmlNode, IHasNotes, ICanRemove, IHasSource
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private Guid _guiID;
        private Guid _guiSourceID;
        private string _strName = string.Empty;
        private string _strRequiresProgram = string.Empty;
        private string _strSource = string.Empty;
        private string _strPage = string.Empty;
        private string _strNotes = string.Empty;
        private string _strExtra = string.Empty;
        private bool _boolIsAdvancedProgram;
        private bool _boolCanDelete = true;
        private readonly Character _objCharacter;

        #region Constructor, Create, Save, Load, and Print Methods
        public AIProgram(Character objCharacter)
        {
            // Create the GUID for the new Program.
            _guiID = Guid.NewGuid();
            _objCharacter = objCharacter;
        }

        /// Create a Program from an XmlNode.
        /// <param name="objXmlProgramNode">XmlNode to create the object from.</param>
        /// <param name="strExtra">Value to forcefully select for any ImprovementManager prompts.</param>
        /// <param name="boolCanDelete">Can this AI program be deleted on its own (set to false for Improvement-granted programs).</param>
        public void Create(XmlNode objXmlProgramNode, string strExtra = "", bool boolCanDelete = true)
        {
            if (!objXmlProgramNode.TryGetField("id", Guid.TryParse, out _guiSourceID))
            {
                Log.Warn(new object[] { "Missing id field for program xmlnode", objXmlProgramNode });
                Utils.BreakIfDebug();
            }
            if (objXmlProgramNode.TryGetStringFieldQuickly("name", ref _strName))
                _objCachedMyXmlNode = null;
            _strRequiresProgram = LanguageManager.GetString("String_None");
            _boolCanDelete = boolCanDelete;
            objXmlProgramNode.TryGetStringFieldQuickly("require", ref _strRequiresProgram);
            objXmlProgramNode.TryGetStringFieldQuickly("source", ref _strSource);
            objXmlProgramNode.TryGetStringFieldQuickly("page", ref _strPage);
            if (!objXmlProgramNode.TryGetStringFieldQuickly("altnotes", ref _strNotes))
                objXmlProgramNode.TryGetStringFieldQuickly("notes", ref _strNotes);
            _strExtra = strExtra;
            string strCategory = string.Empty;
            if (objXmlProgramNode.TryGetStringFieldQuickly("category", ref strCategory))
                _boolIsAdvancedProgram = strCategory == "Advanced Programs";
        }

        private SourceString _objCachedSourceDetail;
        public SourceString SourceDetail => _objCachedSourceDetail = _objCachedSourceDetail ?? new SourceString(Source, DisplayPage(GlobalOptions.Language), GlobalOptions.Language);

        /// <summary>
        /// Save the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        public void Save(XmlTextWriter objWriter)
        {
            if (objWriter == null)
                return;
            objWriter.WriteStartElement("aiprogram");
            objWriter.WriteElementString("sourceid", SourceIDString);
            objWriter.WriteElementString("guid", InternalId);
            objWriter.WriteElementString("name", _strName);
            objWriter.WriteElementString("requiresprogram", _strRequiresProgram);
            objWriter.WriteElementString("extra", _strExtra);
            objWriter.WriteElementString("source", _strSource);
            objWriter.WriteElementString("page", _strPage);
            objWriter.WriteElementString("notes", _strNotes);
            objWriter.WriteElementString("isadvancedprogram", _boolIsAdvancedProgram.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteEndElement();
            _objCharacter.SourceProcess(_strSource);
        }

        /// <summary>
        /// Load the Program from the XmlNode.
        /// </summary>
        /// <param name="objNode">XmlNode to load.</param>
        public void Load(XmlNode objNode)
        {
            if (objNode == null)
                return;
            if (!objNode.TryGetField("guid", Guid.TryParse, out _guiID))
            {
                _guiID = Guid.NewGuid();
            }
            if(!objNode.TryGetGuidFieldQuickly("sourceid", ref _guiSourceID))
            {
                XmlNode node = GetNode(GlobalOptions.Language);
                node?.TryGetGuidFieldQuickly("id", ref _guiSourceID);
            }

            if (objNode.TryGetStringFieldQuickly("name", ref _strName))
                _objCachedMyXmlNode = null;
            objNode.TryGetStringFieldQuickly("requiresprogram", ref _strRequiresProgram);
            objNode.TryGetStringFieldQuickly("source", ref _strSource);
            objNode.TryGetStringFieldQuickly("page", ref _strPage);
            objNode.TryGetStringFieldQuickly("extra", ref _strExtra);
            objNode.TryGetStringFieldQuickly("notes", ref _strNotes);
            _boolIsAdvancedProgram = objNode["isadvancedprogram"]?.InnerText == bool.TrueString;
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
            objWriter.WriteStartElement("aiprogram");
            objWriter.WriteElementString("name", DisplayNameShort(strLanguageToPrint));
            objWriter.WriteElementString("fullname", DisplayName);
            objWriter.WriteElementString("name_english", Name);
            if (string.IsNullOrEmpty(_strRequiresProgram) || _strRequiresProgram == LanguageManager.GetString("String_None", strLanguageToPrint))
                objWriter.WriteElementString("requiresprogram", LanguageManager.GetString("String_None", strLanguageToPrint));
            else
                objWriter.WriteElementString("requiresprogram", DisplayRequiresProgram(strLanguageToPrint));
            objWriter.WriteElementString("source", CommonFunctions.LanguageBookShort(Source, strLanguageToPrint));
            objWriter.WriteElementString("page", DisplayPage(strLanguageToPrint));
            if (_objCharacter.Options.PrintNotes)
                objWriter.WriteElementString("notes", Notes);
            objWriter.WriteEndElement();
        }
        #endregion

        #region Properties


        /// <summary>
        /// Identifier of the object within data files.
        /// </summary>
        public Guid SourceID => _guiSourceID;

        /// <summary>
        /// String-formatted identifier of the <inheritdoc cref="SourceID"/> from the data files.
        /// </summary>
        public string SourceIDString => _guiSourceID.ToString("D", GlobalOptions.InvariantCultureInfo);

        /// <summary>
        /// Internal identifier which will be used to identify this AI Program in the Improvement system.
        /// </summary>
        public string InternalId => _guiID.ToString("D", GlobalOptions.InvariantCultureInfo);

        /// <summary>
        /// AI Program's name.
        /// </summary>
        public string Name
        {
            get => _strName;
            set
            {
                if (_strName != value)
                    _objCachedMyXmlNode = null;
                _strName = value;
            }
        }

        /// <summary>
        /// AI Program's extra info.
        /// </summary>
        public string Extra
        {
            get => _strExtra;
            set => _strExtra = LanguageManager.ReverseTranslateExtra(value);
        }

        /// <summary>
        /// The name of the object as it should be displayed on printouts (translated name only).
        /// </summary>
        public string DisplayNameShort(string strLanguage)
        {
            string strReturn = Name;
            // Get the translated name if applicable.
            if (strLanguage != GlobalOptions.DefaultLanguage)
                strReturn = GetNode(strLanguage)?["translate"]?.InnerText ?? Name;

            if (!string.IsNullOrEmpty(Extra))
            {
                string strExtra = Extra;
                if (strLanguage != GlobalOptions.DefaultLanguage)
                    strExtra = LanguageManager.TranslateExtra(Extra, strLanguage);
                strReturn += LanguageManager.GetString("String_Space", strLanguage) + '(' + strExtra + ')';
            }
            return strReturn;
        }

        /// <summary>
        /// The name of the object as it should be displayed in lists. Name (Extra).
        /// </summary>
        public string DisplayName => DisplayNameShort(GlobalOptions.Language);

        /// <summary>
        /// AI Advanced Program's requirement program.
        /// </summary>
        public string RequiresProgram
        {
            get => _strRequiresProgram;
            set => _strRequiresProgram = value;
        }

        /// <summary>
        /// AI Advanced Program's requirement program.
        /// </summary>
        public string DisplayRequiresProgram(string strLanguage)
        {
            if (string.IsNullOrEmpty(RequiresProgram))
                return LanguageManager.GetString("String_None", strLanguage);
            if (strLanguage == GlobalOptions.Language)
                return RequiresProgram;

            return XmlManager.Load("programs.xml", strLanguage).SelectSingleNode("/chummer/programs/program[name = \"" + RequiresProgram + "\"]/translate")?.InnerText ?? RequiresProgram;
        }

        /// <summary>
        /// If the AI Advanced Program is added from a quality.
        /// </summary>
        public bool CanDelete
        {
            get => _boolCanDelete;
            set => _boolCanDelete = value;
        }

        /// <summary>
        /// AI Program's Source.
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
            if (strLanguage == GlobalOptions.DefaultLanguage)
                return Page;
            string s = GetNode(strLanguage)?["altpage"]?.InnerText ?? Page;
            return !string.IsNullOrWhiteSpace(s) ? s : Page;
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
        /// If the AI Program is an Advanced Program.
        /// </summary>
        public bool IsAdvancedProgram => _boolIsAdvancedProgram;

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
                _objCachedMyXmlNode = SourceID == Guid.Empty
                    ? XmlManager.Load("programs.xml", strLanguage)
                        .SelectSingleNode($"/chummer/programs/program[name = \"{Name}\"]")
                    : XmlManager.Load("programs.xml", strLanguage)
                        .SelectSingleNode($"/chummer/programs/program[id = \"{SourceIDString}\" or id = \"{SourceIDString.ToUpperInvariant()}\"]");
                _strCachedXmlNodeLanguage = strLanguage;
            }
            return _objCachedMyXmlNode;
        }
        #endregion

        #region UI Methods
        public TreeNode CreateTreeNode(ContextMenuStrip cmsAIProgram)
        {
            if (!CanDelete && !string.IsNullOrEmpty(Source) && !_objCharacter.Options.BookEnabled(Source))
                return null;

            TreeNode objNode = new TreeNode
            {
                Name = InternalId,
                Text = DisplayName,
                Tag = this,
                ContextMenuStrip = cmsAIProgram,
                ForeColor = PreferredColor,
                ToolTipText = Notes.WordWrap(100)
            };
            return objNode;
        }

        public Color PreferredColor
        {
            get
            {
                if (!string.IsNullOrEmpty(Notes))
                {
                    return Color.SaddleBrown;
                }
                if (!CanDelete)
                {
                    return SystemColors.GrayText;
                }

                return SystemColors.WindowText;
            }
        }
        #endregion

        public bool Remove(bool blnConfirmDelete = true)
        {
            if (!CanDelete) return false;
            if (blnConfirmDelete)
            {
                if (!_objCharacter.ConfirmDelete(LanguageManager.GetString("Message_DeleteAIProgram",
                    GlobalOptions.Language)))
                    return false;
            }

            ImprovementManager.RemoveImprovements(_objCharacter, Improvement.ImprovementSource.AIProgram,
                InternalId);

            return _objCharacter.AIPrograms.Remove(this);
        }

        public void SetSourceDetail(Control sourceControl)
        {
            if (_objCachedSourceDetail?.Language != GlobalOptions.Language)
                _objCachedSourceDetail = null;
            SourceDetail.SetControl(sourceControl);
        }
    }
}
