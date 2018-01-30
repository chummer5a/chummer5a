using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace Chummer
{
    /// <summary>
    /// A Martial Art.
    /// </summary>
    public class MartialArt : IHasChildren<MartialArtTechnique>, IHasName, IHasInternalId, IHasXmlNode
    {
        private string _strName = string.Empty;
        private string _strSource = string.Empty;
        private string _strPage = string.Empty;
        private int _intKarmaCost = 7;
        private int _intRating = 1;
        private Guid _guiID;
        private ObservableCollection<MartialArtTechnique> _lstTechniques = new ObservableCollection<MartialArtTechnique>();
        private string _strNotes = string.Empty;
        private Character _objCharacter;
        private bool _blnIsQuality;

        #region Create, Save, Load, and Print Methods
        public MartialArt(Character objCharacter)
        {
            _objCharacter = objCharacter;
            _guiID = Guid.NewGuid();
        }

        /// Create a Martial Art from an XmlNode.
        /// <param name="objXmlArtNode">XmlNode to create the object from.</param>
        public void Create(XmlNode objXmlArtNode)
        {
            if (objXmlArtNode.TryGetStringFieldQuickly("name", ref _strName))
                _objCachedMyXmlNode = null;
            objXmlArtNode.TryGetStringFieldQuickly("source", ref _strSource);
            objXmlArtNode.TryGetStringFieldQuickly("page", ref _strPage);
            objXmlArtNode.TryGetInt32FieldQuickly("cost", ref _intKarmaCost);
            if (!objXmlArtNode.TryGetStringFieldQuickly("altnotes", ref _strNotes))
                objXmlArtNode.TryGetStringFieldQuickly("notes", ref _strNotes);
            _blnIsQuality = objXmlArtNode["isquality"]?.InnerText == System.Boolean.TrueString;

            if (objXmlArtNode["bonus"] != null)
            {
                ImprovementManager.CreateImprovements(_objCharacter, Improvement.ImprovementSource.MartialArt, InternalId,
                    objXmlArtNode["bonus"], false, 1, DisplayNameShort(GlobalOptions.Language));
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
            objWriter.WriteStartElement("martialart");
            objWriter.WriteElementString("name", _strName);
            objWriter.WriteElementString("guid", InternalId);
            objWriter.WriteElementString("source", _strSource);
            objWriter.WriteElementString("page", _strPage);
            objWriter.WriteElementString("rating", _intRating.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("cost", _intKarmaCost.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("isquality", _blnIsQuality.ToString());
            objWriter.WriteStartElement("martialarttechniques");
            foreach (MartialArtTechnique objTechnique in _lstTechniques)
            {
                objTechnique.Save(objWriter);
            }
            objWriter.WriteEndElement();
            objWriter.WriteElementString("notes", _strNotes);
            objWriter.WriteEndElement();
            _objCharacter.SourceProcess(_strSource);
        }

        /// <summary>
        /// Load the Martial Art from the XmlNode.
        /// </summary>
        /// <param name="objNode">XmlNode to load.</param>
        public void Load(XmlNode objNode)
        {
            if (!objNode.TryGetField("guid", Guid.TryParse, out _guiID))
                _guiID = Guid.NewGuid();
            if (objNode.TryGetStringFieldQuickly("name", ref _strName))
                _objCachedMyXmlNode = null;
            objNode.TryGetStringFieldQuickly("source", ref _strSource);
            objNode.TryGetStringFieldQuickly("page", ref _strPage);
            objNode.TryGetInt32FieldQuickly("rating", ref _intRating);
            objNode.TryGetInt32FieldQuickly("cost", ref _intKarmaCost);
            objNode.TryGetBoolFieldQuickly("isquality", ref _blnIsQuality);

            foreach (XmlNode nodTechnique in objNode.SelectNodes("martialartadvantages/martialartadvantage"))
            {
                MartialArtTechnique objTechnique = new MartialArtTechnique(_objCharacter);
                objTechnique.Load(nodTechnique);
                _lstTechniques.Add(objTechnique);
            }
            foreach (XmlNode nodTechnique in objNode.SelectNodes("martialarttechniques/martialarttechnique"))
            {
                MartialArtTechnique objTechnique = new MartialArtTechnique(_objCharacter);
                objTechnique.Load(nodTechnique);
                _lstTechniques.Add(objTechnique);
            }

            objNode.TryGetStringFieldQuickly("notes", ref _strNotes);
        }

        /// <summary>
        /// Print the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        public void Print(XmlTextWriter objWriter, CultureInfo objCulture, string strLanguageToPrint)
        {
            objWriter.WriteStartElement("martialart");
            objWriter.WriteElementString("name", DisplayNameShort(strLanguageToPrint));
            objWriter.WriteElementString("name_english", Name);
            objWriter.WriteElementString("source", CommonFunctions.LanguageBookShort(Source, strLanguageToPrint));
            objWriter.WriteElementString("page", Page(strLanguageToPrint));
            objWriter.WriteElementString("rating", Rating.ToString(objCulture));
            objWriter.WriteElementString("cost", Cost.ToString(objCulture));
            objWriter.WriteStartElement("martialarttechniques");
            foreach (MartialArtTechnique objAdvantage in Techniques)
            {
                objAdvantage.Print(objWriter, strLanguageToPrint);
            }
            objWriter.WriteEndElement();
            if (_objCharacter.Options.PrintNotes)
                objWriter.WriteElementString("notes", Notes);
            objWriter.WriteEndElement();
        }
        #endregion

        #region Properties
        /// <summary>
        /// Name.
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

        public string InternalId => _guiID.ToString("D");

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
            if (strLanguage == GlobalOptions.DefaultLanguage)
                return _strPage;

            return GetNode(strLanguage)?["altpage"]?.InnerText ?? _strPage;
        }

        /// <summary>
        /// Rating.
        /// </summary>
        public int Rating
        {
            get => _intRating;
            set => _intRating = value;
        }

        /// <summary>
        /// Karma Cost (usually 7).
        /// </summary>
        public int Cost
        {
            get => _intKarmaCost;
            set => _intKarmaCost = value;
        }

        /// <summary>
        /// Is from a quality.
        /// </summary>
        public bool IsQuality
        {
            get => _blnIsQuality;
            set => _blnIsQuality = value;
        }

        /// <summary>
        /// Selected Martial Arts Advantages.
        /// </summary>
        public ObservableCollection<MartialArtTechnique> Techniques => _lstTechniques;
        public IList<MartialArtTechnique> Children => Techniques;

        /// <summary>
        /// Notes.
        /// </summary>
        public string Notes
        {
            get => _strNotes;
            set => _strNotes = value;
        }

        private XmlNode _objCachedMyXmlNode = null;
        private string _strCachedXmlNodeLanguage = string.Empty;

        public XmlNode GetNode()
        {
            return GetNode(GlobalOptions.Language);
        }

        public XmlNode GetNode(string strLanguage)
        {
            if (_objCachedMyXmlNode == null || strLanguage != _strCachedXmlNodeLanguage || GlobalOptions.LiveCustomData)
            {
                _objCachedMyXmlNode = XmlManager.Load("martialarts.xml", strLanguage).SelectSingleNode("/chummer/martialarts/martialart[name = \"" + Name + "\"]");
                _strCachedXmlNodeLanguage = strLanguage;
            }
            return _objCachedMyXmlNode;
        }
        #endregion

        #region Methods
        public TreeNode CreateTreeNode(ContextMenuStrip cmsMartialArt, ContextMenuStrip cmsMartialArtTechnique)
        {
            TreeNode objNode = new TreeNode
            {
                Name = InternalId,
                Text = DisplayName(GlobalOptions.Language),
                Tag = InternalId,
                ContextMenuStrip = cmsMartialArt
            };
            if (!string.IsNullOrEmpty(Notes))
            {
                objNode.ForeColor = Color.SaddleBrown;
            }
            else if (IsQuality)
            {
                objNode.ForeColor = SystemColors.GrayText;
            }
            objNode.ToolTipText = Notes.WordWrap(100);

            foreach (MartialArtTechnique objAdvantage in Techniques)
            {
                objNode.Nodes.Add(objAdvantage.CreateTreeNode(cmsMartialArtTechnique));
                objNode.Expand();
            }

            return objNode;
        }
        #endregion
    }
}
