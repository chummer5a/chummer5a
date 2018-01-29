using System;
using System.Collections.Generic;
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
    /// A Metamagic or Echo.
    /// </summary>
    public class Metamagic : IHasInternalId, IHasName, IHasXmlNode
    {
        private Guid _guiID;
        private string _strName = string.Empty;
        private string _strSource = string.Empty;
        private string _strPage = string.Empty;
        private bool _blnPaidWithKarma;
        private int _intGrade;
        private XmlNode _nodBonus;
        private Improvement.ImprovementSource _objImprovementSource = Improvement.ImprovementSource.Metamagic;
        private string _strNotes = string.Empty;

        private readonly Character _objCharacter;

        #region Constructor, Create, Save, Load, and Print Methods
        public Metamagic(Character objCharacter)
        {
            // Create the GUID for the new piece of Cyberware.
            _guiID = Guid.NewGuid();
            _objCharacter = objCharacter;
        }

        /// Create a Metamagic from an XmlNode.
        /// <param name="objXmlMetamagicNode">XmlNode to create the object from.</param>
        /// <param name="objCharacter">Character the Gear is being added to.</param>
        /// <param name="objSource">Source of the Improvement.</param>
        public void Create(XmlNode objXmlMetamagicNode, Improvement.ImprovementSource objSource, string strForcedValue = "")
        {
            if (objXmlMetamagicNode.TryGetStringFieldQuickly("name", ref _strName))
                _objCachedMyXmlNode = null;
            objXmlMetamagicNode.TryGetStringFieldQuickly("source", ref _strSource);
            objXmlMetamagicNode.TryGetStringFieldQuickly("page", ref _strPage);
            _objImprovementSource = objSource;
            objXmlMetamagicNode.TryGetInt32FieldQuickly("grade", ref _intGrade);
            if (!objXmlMetamagicNode.TryGetStringFieldQuickly("altnotes", ref _strNotes))
                objXmlMetamagicNode.TryGetStringFieldQuickly("notes", ref _strNotes);
            _nodBonus = objXmlMetamagicNode["bonus"];
            if (_nodBonus != null)
            {
                int intRating = 1;
                if (_objCharacter.SubmersionGrade > 0)
                    intRating = _objCharacter.SubmersionGrade;
                else
                    intRating = _objCharacter.InitiateGrade;

                string strOldFocedValue = ImprovementManager.ForcedValue;
                string strOldSelectedValue = ImprovementManager.SelectedValue;
                ImprovementManager.ForcedValue = strOldFocedValue;
                if (!ImprovementManager.CreateImprovements(_objCharacter, objSource, _guiID.ToString("D"), _nodBonus, true, intRating, DisplayNameShort(GlobalOptions.Language)))
                {
                    _guiID = Guid.Empty;
                    ImprovementManager.ForcedValue = strOldFocedValue;
                    return;
                }
                if (!string.IsNullOrEmpty(ImprovementManager.SelectedValue))
                {
                    _strName += " (" + ImprovementManager.SelectedValue + ')';
                    _objCachedMyXmlNode = null;
                }
                ImprovementManager.ForcedValue = strOldFocedValue;
                ImprovementManager.SelectedValue = strOldSelectedValue;
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
            objWriter.WriteStartElement("metamagic");
            objWriter.WriteElementString("guid", _guiID.ToString("D"));
            objWriter.WriteElementString("name", _strName);
            objWriter.WriteElementString("source", _strSource);
            objWriter.WriteElementString("paidwithkarma", _blnPaidWithKarma.ToString());
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
        /// Load the Metamagic from the XmlNode.
        /// </summary>
        /// <param name="objNode">XmlNode to load.</param>
        public void Load(XmlNode objNode)
        {
            objNode.TryGetField("guid", Guid.TryParse, out _guiID);
            if (objNode.TryGetStringFieldQuickly("name", ref _strName))
                _objCachedMyXmlNode = null;
            objNode.TryGetStringFieldQuickly("source", ref _strSource);
            objNode.TryGetStringFieldQuickly("page", ref _strPage);
            objNode.TryGetBoolFieldQuickly("paidwithkarma", ref _blnPaidWithKarma);
            objNode.TryGetInt32FieldQuickly("grade", ref _intGrade);

            _nodBonus = objNode["bonus"];
            if (objNode["improvementsource"] != null)
                SourceType = Improvement.ConvertToImprovementSource(objNode["improvementsource"].InnerText);

            objNode.TryGetStringFieldQuickly("notes", ref _strNotes);
        }

        /// <summary>
        /// Print the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        public void Print(XmlTextWriter objWriter, CultureInfo objCulture, string strLanguageToPrint)
        {
            objWriter.WriteStartElement("metamagic");
            objWriter.WriteElementString("name", DisplayNameShort(strLanguageToPrint));
            objWriter.WriteElementString("name_english", Name);
            objWriter.WriteElementString("source", CommonFunctions.LanguageBookShort(Source, strLanguageToPrint));
            objWriter.WriteElementString("page", Page(strLanguageToPrint));
            objWriter.WriteElementString("grade", Grade.ToString(objCulture));
            objWriter.WriteElementString("improvementsource", _objImprovementSource.ToString());
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
            set
            {
                if (_objImprovementSource != value)
                {
                    _objCachedMyXmlNode = null;
                    _objImprovementSource = value;
                }
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
                if (_strName != value)
                {
                    _objCachedMyXmlNode = null;
                    _strName = value;
                }
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
        public string Page(string strLanguage)
        {
            // Get the translated name if applicable.
            if (strLanguage == GlobalOptions.DefaultLanguage)
                return _strPage;

            return GetNode(strLanguage)?["altpage"]?.InnerText ?? _strPage;
        }

        /// <summary>
        /// Whether or not the Metamagic was paid for with Karma.
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
                if (_objImprovementSource == Improvement.ImprovementSource.Metamagic)
                    _objCachedMyXmlNode = XmlManager.Load("metamagic.xml", strLanguage).SelectSingleNode("/chummer/metamagics/metamagic[name = \"" + Name + "\"]");
                else
                    _objCachedMyXmlNode = XmlManager.Load("echoes.xml", strLanguage).SelectSingleNode("/chummer/echoes/echo[name = \"" + Name + "\"]");
                _strCachedXmlNodeLanguage = strLanguage;
            }
            return _objCachedMyXmlNode;
        }
        #endregion

        #region Methods
        public TreeNode CreateTreeNode(ContextMenuStrip cmsMetamagic, bool blnAddCategory = false)
        {
            string strText = DisplayName(GlobalOptions.Language);
            if (blnAddCategory)
                strText = LanguageManager.GetString(SourceType == Improvement.ImprovementSource.Metamagic ? "Label_Metamagic" : "Label_Echo", GlobalOptions.Language) + ' ' + strText;
            TreeNode objNode = new TreeNode
            {
                Name = InternalId,
                Text = strText,
                Tag = InternalId,
                ContextMenuStrip = cmsMetamagic
            };
            if (!string.IsNullOrEmpty(Notes))
            {
                objNode.ForeColor = Color.SaddleBrown;
            }
            else if (Grade == -1)
            {
                objNode.ForeColor = SystemColors.GrayText;
            }
            objNode.ToolTipText = Notes.WordWrap(100);

            return objNode;
        }
        #endregion
    }
}
