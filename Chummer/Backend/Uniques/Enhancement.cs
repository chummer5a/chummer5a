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
    /// An Enhancement.
    /// </summary>
    [DebuggerDisplay("{DisplayName(GlobalOptions.DefaultLanguage)}")]
    public class Enhancement : IHasInternalId, IHasName, IHasXmlNode, IHasNotes, ICanRemove
    {
        private Guid _guiID;
        private string _strName = string.Empty;
        private string _strSource = string.Empty;
        private string _strPage = string.Empty;
        private XmlNode _nodBonus;
        private int _intGrade;
        private Improvement.ImprovementSource _objImprovementSource = Improvement.ImprovementSource.Enhancement;
        private string _strNotes = string.Empty;
        private Power _objParent;

        private readonly Character _objCharacter;

        #region Constructor, Create, Save, Load, and Print Methods
        public Enhancement(Character objCharacter)
        {
            // Create the GUID for the new art.
            _guiID = Guid.NewGuid();
            _objCharacter = objCharacter;
        }

        /// Create an Enhancement from an XmlNode.
        /// <param name="objXmlArtNode">XmlNode to create the object from.</param>
        /// <param name="objSource">Source of the Improvement.</param>
        public void Create(XmlNode objXmlArtNode, Improvement.ImprovementSource objSource)
        {
            if (objXmlArtNode.TryGetStringFieldQuickly("name", ref _strName))
                _objCachedMyXmlNode = null;
            objXmlArtNode.TryGetStringFieldQuickly("source", ref _strSource);
            objXmlArtNode.TryGetStringFieldQuickly("page", ref _strPage);
            _objImprovementSource = objSource;
            if (!objXmlArtNode.TryGetStringFieldQuickly("altnotes", ref _strNotes))
                objXmlArtNode.TryGetStringFieldQuickly("notes", ref _strNotes);
            objXmlArtNode.TryGetInt32FieldQuickly("grade", ref _intGrade);
            _nodBonus = objXmlArtNode["bonus"];
            if (_nodBonus != null)
            {
                if (!ImprovementManager.CreateImprovements(_objCharacter, objSource, _guiID.ToString("D"), _nodBonus, true, 1, DisplayNameShort(GlobalOptions.Language)))
                {
                    _guiID = Guid.Empty;
                    return;
                }
                if (!string.IsNullOrEmpty(ImprovementManager.SelectedValue))
                {
                    _strName += LanguageManager.GetString("String_Space", GlobalOptions.Language) + '(' + ImprovementManager.SelectedValue + ')';
                    _objCachedMyXmlNode = null;
                }
            }
            /*
            if (string.IsNullOrEmpty(_strNotes))
            {
                _strNotes = CommonFunctions.GetTextFromPDF($"{_strSource} {_strPage}", _strName);
                if (string.IsNullOrEmpty(_strNotes))
                {
                    _strNotes = CommonFunctions.GetTextFromPDF($"{Source} {Page(GlobalOptions.Language)}", DisplayName(GlobalOptions.Language));
                }
            }*/
        }

        /// <summary>
        /// Save the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        public void Save(XmlTextWriter objWriter)
        {
            objWriter.WriteStartElement("enhancement");
            objWriter.WriteElementString("guid", _guiID.ToString("D"));
            objWriter.WriteElementString("name", _strName);
            objWriter.WriteElementString("source", _strSource);
            objWriter.WriteElementString("page", _strPage);
            objWriter.WriteElementString("grade", _intGrade.ToString(GlobalOptions.InvariantCultureInfo));
            if (_nodBonus != null)
                objWriter.WriteRaw(_nodBonus.OuterXml);
            else
                objWriter.WriteElementString("bonus", string.Empty);
            objWriter.WriteElementString("improvementsource", _objImprovementSource.ToString());
            objWriter.WriteElementString("notes", _strNotes);
            objWriter.WriteEndElement();
            _objCharacter.SourceProcess(_strSource);
        }

        /// <summary>
        /// Load the Enhancement from the XmlNode.
        /// </summary>
        /// <param name="objNode">XmlNode to load.</param>
        public void Load(XmlNode objNode)
        {
            objNode.TryGetField("guid", Guid.TryParse, out _guiID);
            if (objNode.TryGetStringFieldQuickly("name", ref _strName))
                _objCachedMyXmlNode = null;
            objNode.TryGetStringFieldQuickly("source", ref _strSource);
            objNode.TryGetStringFieldQuickly("page", ref _strPage);
            _nodBonus = objNode["bonus"];
            if (objNode["improvementsource"] != null)
                _objImprovementSource = Improvement.ConvertToImprovementSource(objNode["improvementsource"].InnerText);

            objNode.TryGetInt32FieldQuickly("grade", ref _intGrade);
            objNode.TryGetStringFieldQuickly("notes", ref _strNotes);
        }

        /// <summary>
        /// Print the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        /// <param name="strLanguageToPrint">Language in which to print</param>
        public void Print(XmlTextWriter objWriter, string strLanguageToPrint)
        {
            objWriter.WriteStartElement("enhancement");
            objWriter.WriteElementString("name", DisplayNameShort(strLanguageToPrint));
            objWriter.WriteElementString("name_english", Name);
            objWriter.WriteElementString("source", CommonFunctions.LanguageBookShort(Source, strLanguageToPrint));
            objWriter.WriteElementString("page", Page(strLanguageToPrint));
            objWriter.WriteElementString("improvementsource", SourceType.ToString());
            if (_objCharacter.Options.PrintNotes)
                objWriter.WriteElementString("notes", Notes);
            objWriter.WriteEndElement();
        }
        #endregion

        #region Properties
        /// <summary>
        /// Internal identifier which will be used to identify this Metamagic in the Improvement system.
        /// </summary>
        public string InternalId => _guiID.ToString("D");

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
            get => _objImprovementSource;
            set => _objImprovementSource = value;
        }

        /// <summary>
        /// Metamagic name.
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
        /// The name of the object as it should be displayed on printouts (translated name only).
        /// </summary>
        public string DisplayNameShort(string strLanguage)
        {
            // Get the translated name if applicable.
            if (strLanguage == GlobalOptions.DefaultLanguage)
                return Name;

            return GetNode(strLanguage)?["translate"]?.InnerText ?? Name;
        }

        /// <summary>
        /// The name of the object as it should be displayed in lists. Name (Extra).
        /// </summary>
        public string DisplayName(string strLanguage)
        {
            string strReturn = DisplayNameShort(strLanguage);

            return strReturn;
        }

        /// <summary>
        /// The initiate grade where the enhancement was learned.
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
        public string Page(string strLanguage)
        {
            // Get the translated name if applicable.
            if (strLanguage == GlobalOptions.DefaultLanguage)
                return _strPage;

            return GetNode(strLanguage)?["altpage"]?.InnerText ?? _strPage;
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
        /// Parent Power.
        /// </summary>
        public Power Parent
        {
            get => _objParent;
            set => _objParent = value;
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
                _objCachedMyXmlNode = XmlManager.Load("powers.xml", strLanguage).SelectSingleNode("/chummer/enhancements/enhancement[name = \"" + Name + "\"]");
                _strCachedXmlNodeLanguage = strLanguage;
            }
            return _objCachedMyXmlNode;
        }
        #endregion

        #region UI Methods
        public TreeNode CreateTreeNode(ContextMenuStrip cmsEnhancement, bool blnAddCategory = false)
        {
            if (Grade == -1 && !string.IsNullOrEmpty(Source) && !_objCharacter.Options.BookEnabled(Source))
                return null;

            string strText = DisplayName(GlobalOptions.Language);
            if (blnAddCategory)
                strText = LanguageManager.GetString("Label_Enhancement", GlobalOptions.Language) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + strText;
            TreeNode objNode = new TreeNode
            {
                Name = InternalId,
                Text = strText,
                Tag = this,
                ContextMenuStrip = cmsEnhancement,
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
                if (Grade == -1)
                {
                    return SystemColors.GrayText;
                }

                return SystemColors.WindowText;
            }
        }
        #endregion

        public bool Remove(Character characterObject)
        {
            if (Grade <= 0)
                return false;
            string strMessage = LanguageManager.GetString("Message_DeleteEnhancement", GlobalOptions.Language);
            if (!characterObject.ConfirmDelete(strMessage))
                return false;

            characterObject.Enhancements.Remove(this);
            foreach (Power objPower in characterObject.Powers)
            {
                if (objPower.Enhancements.Contains(this))
                    objPower.Enhancements.Remove(this);
            }

            return true;
        }
    }
}
