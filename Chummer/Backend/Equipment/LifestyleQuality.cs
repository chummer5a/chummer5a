using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Xml;

namespace Chummer.Backend.Equipment
{
    public class LifestyleQuality : INamedItemWithGuid
    {
        private Guid _guiID;
        private Guid _SourceGuid;
        private string _strName = string.Empty;
        private string _strExtra = string.Empty;
        private string _strSource = string.Empty;
        private string _strPage = string.Empty;
        private string _strNotes = string.Empty;
        private bool _blnContributeToLimit = true;
        private bool _blnPrint = true;
        private int _intLP = 0;
        private int _intCost = 0;
        private int _intMultiplier = 0;
        private int _intBaseMultiplier = 0;
        private List<string> _lstAllowedFreeLifestyles = new List<string>();
        private Lifestyle _objParentLifestyle = null;
        private QualityType _objLifestyleQualityType = QualityType.Positive;
        private QualitySource _objLifestyleQualitySource = QualitySource.Selected;
        private XmlNode _nodBonus;
        private readonly Character _objCharacter;
        private string _strAltName = string.Empty;
        private string _strAltPage = string.Empty;
        private bool _blnFree;
        private string _strTooltipSource;

        #region Helper Methods
        /// <summary>
        /// Convert a string to a LifestyleQualityType.
        /// </summary>
        /// <param name="strValue">String value to convert.</param>
        public QualityType ConvertToLifestyleQualityType(string strValue)
        {
            switch (strValue)
            {
                case "Negative":
                    return QualityType.Negative;
                case "Positive":
                    return QualityType.Positive;
                case "Contracts":
                    return QualityType.Contracts;
                default:
                    return QualityType.Entertainment;
            }
        }

        /// <summary>
        /// Convert a string to a LifestyleQualitySource.
        /// </summary>
        /// <param name="strValue">String value to convert.</param>
        public QualitySource ConvertToLifestyleQualitySource(string strValue)
        {
            switch (strValue)
            {
                default:
                    return QualitySource.Selected;
            }
        }
        #endregion

        #region Constructor, Create, Save, Load, and Print Methods
        public LifestyleQuality(Character objCharacter)
        {
            // Create the GUID for the new LifestyleQuality.
            _guiID = Guid.NewGuid();
            _objCharacter = objCharacter;
        }

        /// <summary>
        /// Create a LifestyleQuality from an XmlNode and return the TreeNodes for it.
        /// </summary>
        /// <param name="objXmlLifestyleQuality">XmlNode to create the object from.</param>
        /// <param name="objCharacter">Character object the LifestyleQuality will be added to.</param>
        /// <param name="objLifestyleQualitySource">Source of the LifestyleQuality.</param>
        /// <param name="objNode">TreeNode to populate a TreeView.</param>
        public void Create(XmlNode objXmlLifestyleQuality, Lifestyle objParentLifestyle, Character objCharacter, QualitySource objLifestyleQualitySource, TreeNode objNode)
        {
            _objParentLifestyle = objParentLifestyle;
            _SourceGuid = Guid.Parse(objXmlLifestyleQuality["id"].InnerText);
            objXmlLifestyleQuality.TryGetStringFieldQuickly("name", ref _strName);
            objXmlLifestyleQuality.TryGetInt32FieldQuickly("lp", ref _intLP);
            objXmlLifestyleQuality.TryGetInt32FieldQuickly("cost", ref _intCost);
            objXmlLifestyleQuality.TryGetInt32FieldQuickly("multiplier", ref _intMultiplier);
            objXmlLifestyleQuality.TryGetInt32FieldQuickly("multiplierbaseonly", ref _intBaseMultiplier);
            if (objXmlLifestyleQuality["category"] != null)
                _objLifestyleQualityType = ConvertToLifestyleQualityType(objXmlLifestyleQuality["category"].InnerText);
            _objLifestyleQualitySource = objLifestyleQualitySource;
            if (objXmlLifestyleQuality["print"]?.InnerText == "no")
                _blnPrint = false;
            if (objXmlLifestyleQuality["contributetolimit"]?.InnerText == "no")
                _blnContributeToLimit = false;
            objXmlLifestyleQuality.TryGetStringFieldQuickly("source", ref _strSource);
            objXmlLifestyleQuality.TryGetStringFieldQuickly("page", ref _strPage);
            string strAllowedFreeLifestyles = string.Empty;
            if (objXmlLifestyleQuality.TryGetStringFieldQuickly("allowed", ref strAllowedFreeLifestyles))
                _lstAllowedFreeLifestyles = strAllowedFreeLifestyles.Split(',').ToList();
            if (objNode.Text.Contains('('))
            {
                _strExtra = objNode.Text.Split('(')[1].TrimEnd(')');
            }
            if (GlobalOptions.Instance.Language != "en-us")
            {
                XmlDocument objXmlDocument = XmlManager.Instance.Load("lifestyles.xml");
                XmlNode objLifestyleQualityNode = objXmlDocument.SelectSingleNode("/chummer/qualities/quality[name = \"" + _strName + "\"]");
                if (objLifestyleQualityNode != null)
                {
                    objXmlLifestyleQuality.TryGetStringFieldQuickly("translate", ref _strAltName);
                    objXmlLifestyleQuality.TryGetStringFieldQuickly("altpage", ref _strAltPage);
                }
            }

            // If the item grants a bonus, pass the information to the Improvement Manager.
            if (objXmlLifestyleQuality.InnerXml.Contains("<bonus>"))
            {
                ImprovementManager objImprovementManager = new ImprovementManager(objCharacter);
                if (!objImprovementManager.CreateImprovements(Improvement.ImprovementSource.Quality, _guiID.ToString(), objXmlLifestyleQuality["bonus"], false, 1, DisplayNameShort))
                {
                    _guiID = Guid.Empty;
                    return;
                }
                if (!string.IsNullOrEmpty(objImprovementManager.SelectedValue))
                {
                    _strExtra = objImprovementManager.SelectedValue;
                    //objNode.Text += " (" + objImprovementManager.SelectedValue + ")";
                }
            }

            // Built-In Qualities appear as grey text to show that they cannot be removed.
            if (objLifestyleQualitySource == QualitySource.BuiltIn)
            {
                objNode.ForeColor = SystemColors.GrayText;
                Free = true;
            }
            objNode.Name = Name;
            objNode.Text = DisplayName;
            objNode.Tag = InternalId;
        }

        /// <summary>
        /// Save the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        public void Save(XmlTextWriter objWriter)
        {
            objWriter.WriteStartElement("lifestylequality");
            objWriter.WriteElementString("id", _SourceGuid.ToString());
            objWriter.WriteElementString("guid", _guiID.ToString());
            objWriter.WriteElementString("name", _strName);
            objWriter.WriteElementString("extra", _strExtra);
            objWriter.WriteElementString("cost", _intCost.ToString(CultureInfo.InvariantCulture));
            objWriter.WriteElementString("multiplier", _intMultiplier.ToString(CultureInfo.InvariantCulture));
            objWriter.WriteElementString("basemultiplier", _intBaseMultiplier.ToString(CultureInfo.InvariantCulture));
            objWriter.WriteElementString("lp", _intLP.ToString());
            objWriter.WriteElementString("contributetolimit", _blnContributeToLimit.ToString());
            objWriter.WriteElementString("print", _blnPrint.ToString());
            objWriter.WriteElementString("lifestylequalitytype", _objLifestyleQualityType.ToString());
            objWriter.WriteElementString("lifestylequalitysource", _objLifestyleQualitySource.ToString());
            objWriter.WriteElementString("source", _strSource);
            objWriter.WriteElementString("page", _strPage);
            objWriter.WriteElementString("allowed", string.Join(",", _lstAllowedFreeLifestyles));
            if (_nodBonus != null)
                objWriter.WriteRaw("<bonus>" + _nodBonus.InnerXml + "</bonus>");
            else
                objWriter.WriteElementString("bonus", string.Empty);
            objWriter.WriteElementString("notes", _strNotes);
            objWriter.WriteEndElement();
            _objCharacter.SourceProcess(_strSource);
        }

        /// <summary>
        /// Load the CharacterAttribute from the XmlNode.
        /// </summary>
        /// <param name="objNode">XmlNode to load.</param>
        public void Load(XmlNode objNode, Lifestyle objParentLifestyle)
        {
            ParentLifestyle = objParentLifestyle;
            objNode.TryGetField("guid", Guid.TryParse, out _guiID);
            objNode.TryGetStringFieldQuickly("name", ref _strName);
            if (!objNode.TryGetField("id", Guid.TryParse, out _SourceGuid))
            {
                var doc = XmlManager.Instance.Load("lifestyles.xml");
                var q = doc.SelectSingleNode("/chummer/qualities/quality[name = \"" + Name + "\"]");
                q.TryGetField("id", Guid.TryParse, out _SourceGuid);
            }
            objNode.TryGetStringFieldQuickly("extra", ref _strExtra);
            objNode.TryGetInt32FieldQuickly("lp", ref _intLP);
            objNode.TryGetInt32FieldQuickly("cost", ref _intCost);
            objNode.TryGetInt32FieldQuickly("multiplier", ref _intMultiplier);
            objNode.TryGetInt32FieldQuickly("basemultiplier", ref _intBaseMultiplier);
            objNode.TryGetBoolFieldQuickly("contributetolimit", ref _blnContributeToLimit);
            objNode.TryGetBoolFieldQuickly("print", ref _blnPrint);
            if (objNode["lifestylequalitytype"] != null)
                _objLifestyleQualityType = ConvertToLifestyleQualityType(objNode["lifestylequalitytype"].InnerText);
            if (objNode["lifestylequalitysource"] != null)
                _objLifestyleQualitySource = ConvertToLifestyleQualitySource(objNode["lifestylequalitysource"].InnerText);
            objNode.TryGetStringFieldQuickly("source", ref _strSource);
            objNode.TryGetStringFieldQuickly("page", ref _strPage);
            string strAllowedFreeLifestyles = string.Empty;
            if (!objNode.TryGetStringFieldQuickly("allowed", ref strAllowedFreeLifestyles))
            {
                XmlDocument objXmlDocument = XmlManager.Instance.Load("lifestyles.xml");
                XmlNode objXmlQuality = objXmlDocument.SelectSingleNode("/chummer/qualities/quality[name = \"" + Name + "\"]");
                strAllowedFreeLifestyles = objXmlQuality?["allowed"]?.InnerText ?? string.Empty;
            }
            _lstAllowedFreeLifestyles = strAllowedFreeLifestyles.Split(',').ToList();
            _nodBonus = objNode["bonus"];
            objNode.TryGetStringFieldQuickly("notes", ref _strNotes);

            if (GlobalOptions.Instance.Language != "en-us")
            {
                XmlDocument objXmlDocument = XmlManager.Instance.Load("lifestyles.xml");
                XmlNode objLifestyleQualityNode = objXmlDocument.SelectSingleNode("/chummer/lifestylequalities/lifestylequality[name = \"" + _strName + "\"]");
                if (objLifestyleQualityNode != null)
                {
                    objLifestyleQualityNode.TryGetStringFieldQuickly("translate", ref _strAltName);
                    objLifestyleQualityNode.TryGetStringFieldQuickly("altpage", ref _strAltPage);
                }
            }
            LegacyShim();
        }

        /// <summary>
        /// Performs actions based on the character's last loaded AppVersion attribute. 
        /// </summary>
        private void LegacyShim()
        {
            //Unstored Cost and LP values prior to 5.190.2 nightlies.
            if (_objCharacter.LastSavedVersion <= Version.Parse("5.190.0"))
            {
                XmlDocument objXmlDocument = XmlManager.Instance.Load("lifestyles.xml");
                XmlNode objLifestyleQualityNode = objXmlDocument.SelectSingleNode("/chummer/qualities/quality[id = \"" + _guiID + "\"]") ??
                                                  objXmlDocument.SelectSingleNode("/chummer/qualities/quality[name = \"" + _strName + "\"]");
                if (objLifestyleQualityNode == null)
                {
                    var lstQualities = new List<ListItem>();
                    lstQualities.AddRange(
                             from XmlNode objNode in
                             objXmlDocument.SelectNodes("/chummer/qualities/quality")
                             select new ListItem
                             {
                                 Value = objNode["name"].InnerText,
                                 Name = objNode["translate"]?.InnerText ?? objNode["name"].InnerText
                             });
                    var frmSelect = new frmSelectItem
                    {
                        DropdownItems = lstQualities,
                        Description =
                        LanguageManager.Instance.GetString("String_CannotFindLifestyleQuality").Replace("{0}", _strName)
                    };
                    frmSelect.ShowDialog();
                    if (frmSelect.DialogResult == DialogResult.Cancel)
                        return;

                    objLifestyleQualityNode = objXmlDocument.SelectSingleNode("/chummer/qualities/quality[name = \"" + frmSelect.SelectedItem + "\"]");
                }
                int intTemp = 0;
                if (objLifestyleQualityNode.TryGetInt32FieldQuickly("cost", ref intTemp))
                    Cost = intTemp;
                if (objLifestyleQualityNode.TryGetInt32FieldQuickly("lp", ref intTemp))
                    LP = intTemp;
                if (objLifestyleQualityNode.TryGetInt32FieldQuickly("area", ref intTemp))
                    AreaCost = intTemp;
                if (objLifestyleQualityNode.TryGetInt32FieldQuickly("comforts", ref intTemp))
                    ComfortCost = intTemp;
                if (objLifestyleQualityNode.TryGetInt32FieldQuickly("security", ref intTemp))
                    SecurityCost = intTemp;
                if (objLifestyleQualityNode.TryGetInt32FieldQuickly("areaminimum", ref intTemp))
                    AreaMinimum = intTemp;
                if (objLifestyleQualityNode.TryGetInt32FieldQuickly("comfortsminimum", ref intTemp))
                    ComfortMinimum = intTemp;
                if (objLifestyleQualityNode.TryGetInt32FieldQuickly("securityminimum", ref intTemp))
                    SecurityMinimum = intTemp;
                if (objLifestyleQualityNode.TryGetInt32FieldQuickly("multiplier", ref intTemp))
                    Multiplier = intTemp;
                if (objLifestyleQualityNode.TryGetInt32FieldQuickly("multiplierbaseonly", ref intTemp))
                    BaseMultiplier = intTemp;
            }
        }

        /// <summary>
        /// Print the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        public void Print(XmlTextWriter objWriter)
        {
            if (!_blnPrint) return;
            objWriter.WriteStartElement("quality");
            objWriter.WriteElementString("name", DisplayNameShort);
            objWriter.WriteElementString("formattedname", FormattedDisplayName);
            objWriter.WriteElementString("extra", LanguageManager.Instance.TranslateExtra(_strExtra));
            objWriter.WriteElementString("lp", _intLP.ToString());
            objWriter.WriteElementString("cost", _intCost.ToString());
            string strLifestyleQualityType = _objLifestyleQualityType.ToString();
            if (GlobalOptions.Instance.Language != "en-us")
            {
                XmlDocument objXmlDocument = XmlManager.Instance.Load("lifestyles.xml");

                XmlNode objNode = objXmlDocument.SelectSingleNode("/chummer/categories/category[. = \"" + strLifestyleQualityType + "\"]");
                strLifestyleQualityType = objNode?.Attributes?["translate"].InnerText ?? strLifestyleQualityType;
            }
            objWriter.WriteElementString("lifestylequalitytype", strLifestyleQualityType);
            objWriter.WriteElementString("lifestylequalitytype_english", _objLifestyleQualityType.ToString());
            objWriter.WriteElementString("lifestylequalitysource", _objLifestyleQualitySource.ToString());
            objWriter.WriteElementString("source", _objCharacter.Options.LanguageBookShort(_strSource));
            objWriter.WriteElementString("page", Page);
            if (_objCharacter.Options.PrintNotes)
                objWriter.WriteElementString("notes", _strNotes);
            objWriter.WriteEndElement();
        }
        #endregion

        #region Properties
        /// <summary>
        /// Internal identifier which will be used to identify this LifestyleQuality in the Improvement system.
        /// </summary>
        public string InternalId
        {
            get
            {
                return _guiID.ToString();
            }
        }
        /// <summary>
        /// Source identifier that will be used to identify this Lifestyle Quality in data.
        /// </summary>
        public string SourceID
        {
            get
            {
                return _SourceGuid.ToString();
            }
        }
        /// <summary>
        /// LifestyleQuality's name.
        /// </summary>
        public string Name
        {
            get
            {
                return _strName;
            }
            set
            {
                _strName = value;
            }
        }

        /// <summary>
        /// LifestyleQuality's parent lifestyle.
        /// </summary>
        public Lifestyle ParentLifestyle
        {
            get
            {
                return _objParentLifestyle;
            }
            set
            {
                _objParentLifestyle = value;
            }
        }

        /// <summary>
        /// Extra information that should be applied to the name, like a linked CharacterAttribute.
        /// </summary>
        public string Extra
        {
            get
            {
                return _strExtra;
            }
            set
            {
                _strExtra = value;
            }
        }

        /// <summary>
        /// Sourcebook.
        /// </summary>
        public string Source
        {
            get
            {
                return _strSource;
            }
            set
            {
                _strSource = value;
            }
        }

        public string SourceTooltip
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(_strTooltipSource)) return _strTooltipSource;
                XmlDocument objBookDocument = XmlManager.Instance.Load("books.xml");
                XmlNode objXmlBook = objBookDocument.SelectSingleNode("/chummer/books/book[code = \"" + _strSource + "\"]");
                _strTooltipSource = $"{objXmlBook["name"].InnerText} {LanguageManager.Instance.GetString("String_Page")} {Page}";
                return _strTooltipSource;
            }
        }

        /// <summary>
        /// Page Number.
        /// </summary>
        public string Page
        {
            get
            {
                if (!string.IsNullOrEmpty(_strAltPage))
                    return _strAltPage;

                return _strPage;
            }
            set
            {
                _strPage = value;
            }
        }

        /// <summary>
        /// Bonus node from the XML file.
        /// </summary>
        public XmlNode Bonus
        {
            get
            {
                return _nodBonus;
            }
            set
            {
                _nodBonus = value;
            }
        }

        /// <summary>
        /// LifestyleQuality Type.
        /// </summary>
        public QualityType Type
        {
            get
            {
                return _objLifestyleQualityType;
            }
        }

        /// <summary>
        /// Source of the LifestyleQuality.
        /// </summary>
        public QualitySource OriginSource
        {
            get
            {
                return _objLifestyleQualitySource;
            }
            set
            {
                _objLifestyleQualitySource = value;
            }
        }

        /// <summary>
        /// Number of Build Points the LifestyleQuality costs.
        /// </summary>
        public int LP
        {
            get
            {
                return Free ? 0 : _intLP;
            }
            set
            {
                _intLP = value;
            }
        }

        /// <summary>
        /// The name of the object as it should be displayed on printouts (translated name only).
        /// </summary>
        public string DisplayNameShort
        {
            get
            {
                if (!string.IsNullOrEmpty(_strAltName))
                    return _strAltName;

                return _strName;
            }
        }

        /// <summary>
        /// The name of the object as it should be displayed in lists. Name (Extra).
        /// </summary>
        public string DisplayName
        {
            get
            {
                string strReturn = DisplayNameShort;

                if (!string.IsNullOrEmpty(_strExtra))
                {
                    LanguageManager.Instance.Load(GlobalOptions.Instance.Language, this);
                    // Attempt to retrieve the CharacterAttribute name.
                    strReturn += " (" + LanguageManager.Instance.TranslateExtra(_strExtra) + ")";
                }
                return strReturn;
            }
        }

        public string FormattedDisplayName
        {
            get
            {
                string strReturn = DisplayName;

                if (Multiplier > 0)
                {
                    strReturn += $" [+{Multiplier}%]";
                }
                else if (Multiplier < 0)
                {
                    strReturn += $" [-{Multiplier}%]";
                }

                if (Cost > 0)
                {
                    strReturn += $" [+{Cost}¥]";
                }
                return strReturn;
            }
        }

        /// <summary>
        /// Whether or not the LifestyleQuality appears on the printouts.
        /// </summary>
        public bool AllowPrint
        {
            get
            {
                return _blnPrint;
            }
            set
            {
                _blnPrint = value;
            }
        }

        /// <summary>
        /// Notes.
        /// </summary>
        public string Notes
        {
            get
            {
                return _strNotes;
            }
            set
            {
                _strNotes = value;
            }
        }

        /// <summary>
        /// Nuyen cost of the Quality.
        /// </summary>
        public int Cost
        {
            get { return Free || FreeByLifestyle ? 0 : _intCost; }
            set { _intCost = value; }
        }

        /// <summary>
        /// Does the Quality have a Nuyen or LP cost?
        /// </summary>
        public bool Free
        {
            get { return _blnFree || OriginSource == QualitySource.BuiltIn; }
            set { _blnFree = value; }
        }

        /// <summary>
        /// Are the costs of this Quality included in base lifestyle costs?
        /// </summary>
        public bool FreeByLifestyle
        {
            get
            {
                if (Type == QualityType.Entertainment || Type == QualityType.Contracts)
                {
                    string strLifestyleEquivalent = Lifestyle.GetEquivalentLifestyle(_objParentLifestyle.BaseLifestyle);
                    if (!string.IsNullOrEmpty(_objParentLifestyle?.BaseLifestyle) &&
                        _lstAllowedFreeLifestyles.Any(strLifestyle => strLifestyle == strLifestyleEquivalent || strLifestyle == _objParentLifestyle.BaseLifestyle))
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        /// <summary>
        /// Minimum level of Comfort that's necessary for the Quality to not cost Nuyen.
        /// </summary>
        public int ComfortMinimum { get; set; }

        /// <summary>
        /// Comfort LP Cost/Benefit of the Quality.
        /// </summary>
        public int ComfortCost { get; set; }

        /// <summary>
        /// Security LP Cost/Benefit of the Quality.
        /// </summary>
        public int SecurityCost { get; set; }
        
        /// <summary>
        /// Minimum level of Security that's necessary for the Quality to not cost Nuyen.
        /// </summary>
        public int SecurityMinimum { get; set; }

        /// <summary>
        /// Percentage by which the quality increases the overall Lifestyle Cost.
        /// </summary>
        public int Multiplier
        {
            get { return (Free || FreeByLifestyle) ? 0 : _intMultiplier; }
            set { _intMultiplier = value; }
        }

        /// <summary>
        /// Percentage by which the quality increases the Lifestyle Cost ONLY, without affecting other qualities.
        /// </summary>
        public int BaseMultiplier
        {
            get { return (Free || FreeByLifestyle) ? 0 : _intBaseMultiplier; }
            set { _intBaseMultiplier = value; }
        }

        /// <summary>
        /// Category of the Quality. 
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// Area/Neighborhood LP Cost/Benefit of the Quality.
        /// </summary>
        public int AreaCost { get; set; }

        /// <summary>
        /// Minimum level of Area/Neighborhood that's necessary for the Quality to not cost Nuyen.
        /// </summary>
        public int AreaMinimum { get; set; }

        #endregion
    }
}