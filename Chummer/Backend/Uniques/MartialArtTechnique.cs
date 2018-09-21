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

namespace Chummer
{
    /// <summary>
    /// A Martial Arts Technique.
    /// </summary>
    [DebuggerDisplay("{DisplayName(GlobalOptions.DefaultLanguage)}")]
    public class MartialArtTechnique : IHasInternalId, IHasName, IHasXmlNode, IHasNotes, ICanRemove, IHasSource
    {
        private Guid _guiID;
        private string _strName = string.Empty;
        private string _strNotes = string.Empty;
        private string _strSource = string.Empty;
        private string _strPage = string.Empty;
        private string _strSourceId = string.Empty;
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
            if (xmlTechniqueDataNode.TryGetStringFieldQuickly("id", ref _strSourceId))
                _objCachedMyXmlNode = null;
            if (xmlTechniqueDataNode.TryGetStringFieldQuickly("name", ref _strName))
                if (!xmlTechniqueDataNode.TryGetStringFieldQuickly("altnotes", ref _strNotes))
                    xmlTechniqueDataNode.TryGetStringFieldQuickly("notes", ref _strNotes);
            xmlTechniqueDataNode.TryGetStringFieldQuickly("source", ref _strSource);
            xmlTechniqueDataNode.TryGetStringFieldQuickly("page", ref _strPage);
            
            if (xmlTechniqueDataNode["bonus"] == null) return;
            if (!ImprovementManager.CreateImprovements(_objCharacter, Improvement.ImprovementSource.MartialArtTechnique, _guiID.ToString("D"), xmlTechniqueDataNode["bonus"], false, 1, DisplayName(GlobalOptions.Language)))
            {
                _guiID = Guid.Empty;
            }
        }

        private SourceString _objCachedSourceDetail;
        public SourceString SourceDetail
        {
            get
            {
                if (_objCachedSourceDetail == null)
                {
                    string strSource = Source;
                    string strPage = Page(GlobalOptions.Language);
                    if (!string.IsNullOrEmpty(strSource) && !string.IsNullOrEmpty(strPage))
                    {
                        _objCachedSourceDetail = new SourceString(strSource, strPage, GlobalOptions.Language);
                    }
                    else
                    {
                        Utils.BreakIfDebug();
                    }
                }

                return _objCachedSourceDetail;
            }
        }

        /// <summary>
        /// Save the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        public void Save(XmlTextWriter objWriter)
        {
            objWriter.WriteStartElement("martialarttechnique");
            objWriter.WriteElementString("guid", _guiID.ToString("D"));
            objWriter.WriteElementString("sourceid", _strSourceId);
            objWriter.WriteElementString("name", _strName);
            objWriter.WriteElementString("notes", _strNotes);
            objWriter.WriteElementString("source", _strSource);
            objWriter.WriteElementString("page", _strSource);
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
                _guiID = Guid.NewGuid();
            objNode.TryGetStringFieldQuickly("name", ref _strName);
            if (objNode.TryGetStringFieldQuickly("sourceid", ref _strSourceId))
                _objCachedMyXmlNode = null;
            else
            {
                if (XmlManager.Load("martialarts.xml").SelectSingleNode("/chummer/techniques/technique[name = \"" + _strName + "\"]").TryGetStringFieldQuickly("sourceid", ref _strSourceId))
                    _objCachedMyXmlNode = null;
            }
            objNode.TryGetStringFieldQuickly("source", ref _strSource);
            objNode.TryGetStringFieldQuickly("page", ref _strPage);
            objNode.TryGetStringFieldQuickly("notes", ref _strNotes);
        }

        /// <summary>
        /// Print the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        /// <param name="strLanguageToPrint">Language in which to print</param>
        public void Print(XmlTextWriter objWriter, string strLanguageToPrint)
        {
            objWriter.WriteStartElement("martialarttechnique");
            objWriter.WriteElementString("name", DisplayName(strLanguageToPrint));
            objWriter.WriteElementString("name_english", Name);
            if (_objCharacter.Options.PrintNotes)
                objWriter.WriteElementString("notes", Notes);
            objWriter.WriteElementString("source", Source);
            objWriter.WriteElementString("page", Page(strLanguageToPrint));
            objWriter.WriteEndElement();
        }
        #endregion

        #region Properties
        /// <summary>
        /// Internal identifier which will be used to identify this Martial Art Technique in the Improvement system.
        /// </summary>
        public string InternalId => _guiID.ToString("D");

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
            if (strLanguage == GlobalOptions.DefaultLanguage)
                return Name;

            return GetNode(strLanguage)?["translate"]?.InnerText ?? Name;
        }

        /// <summary>
        /// Notes attached to this technique.
        /// </summary>
        public string Notes
        {
            get => _strNotes;
            set => _strNotes = value;
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
        /// Page Number.
        /// </summary>
        public string Page(string strLanguage)
        {
            // Get the translated name if applicable.
            if (strLanguage != GlobalOptions.DefaultLanguage)
                return _strPage;

            return GetNode(strLanguage)?["altpage"]?.InnerText ?? _strPage;
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
                _objCachedMyXmlNode = XmlManager.Load("martialarts.xml", strLanguage).SelectSingleNode("/chummer/techniques/technique[id = \"" + _strSourceId + "\"]");
                _strCachedXmlNodeLanguage = strLanguage;
            }
            return _objCachedMyXmlNode;
        }
        #endregion

        #region Methods

        public bool Remove(Character objCharacter, bool blnConfirmDelete)
        {
            if (blnConfirmDelete)
            {
                if (!objCharacter.ConfirmDelete(LanguageManager.GetString("Message_DeleteMartialArt",
                    GlobalOptions.Language)))
                    return false;
            }
            // Find the selected Advantage object.
            //TODO: Advantages should know what their parent is. 
            objCharacter.MartialArts.FindMartialArtTechnique(InternalId, out MartialArt objMartialArt);

            ImprovementManager.RemoveImprovements(objCharacter,
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
                Text = DisplayName(GlobalOptions.Language),
                Tag = this,
                ContextMenuStrip = cmsMartialArtTechnique,
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

                return SystemColors.WindowText;
            }
        }
        #endregion

        public void SetSourceDetail(Control sourceControl)
        {
            if (_objCachedSourceDetail?.Language != GlobalOptions.Language)
                _objCachedSourceDetail = null;
            SourceDetail.SetControl(sourceControl);
        }
    }
}
