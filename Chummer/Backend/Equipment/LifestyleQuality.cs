using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;

namespace Chummer.Backend.Equipment
{
    public class LifestyleQuality : IHasInternalId, IHasName, IHasXmlNode
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
        private string _strCost = string.Empty;
        private int _intMultiplier = 0;
        private int _intBaseMultiplier = 0;
        private List<string> _lstAllowedFreeLifestyles = new List<string>();
        private Lifestyle _objParentLifestyle = null;
        private QualityType _objLifestyleQualityType = QualityType.Positive;
        private QualitySource _objLifestyleQualitySource = QualitySource.Selected;
        private XmlNode _nodBonus;
        private readonly Character _objCharacter;
        private bool _blnFree;
        private string _strTooltipSource;

        #region Helper Methods
        /// <summary>
        /// Convert a string to a LifestyleQualityType.
        /// </summary>
        /// <param name="strValue">String value to convert.</param>
        public static QualityType ConvertToLifestyleQualityType(string strValue)
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
#if DEBUG
        public static QualitySource ConvertToLifestyleQualitySource(string strValue)
        {
            switch (strValue)
            {
                default:
                    return QualitySource.Selected;
            }
#else
        public QualitySource ConvertToLifestyleQualitySource()
        {
            return QualitySource.Selected;
#endif
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
            if (objXmlLifestyleQuality.TryGetStringFieldQuickly("name", ref _strName))
                _objCachedMyXmlNode = null;
            objXmlLifestyleQuality.TryGetInt32FieldQuickly("lp", ref _intLP);
            objXmlLifestyleQuality.TryGetStringFieldQuickly("cost", ref _strCost);
            objXmlLifestyleQuality.TryGetInt32FieldQuickly("multiplier", ref _intMultiplier);
            objXmlLifestyleQuality.TryGetInt32FieldQuickly("multiplierbaseonly", ref _intBaseMultiplier);
            if (objXmlLifestyleQuality["category"] != null)
                _objLifestyleQualityType = ConvertToLifestyleQualityType(objXmlLifestyleQuality["category"].InnerText);
            _objLifestyleQualitySource = objLifestyleQualitySource;
            if (objXmlLifestyleQuality["print"]?.InnerText == "no")
                _blnPrint = false;
            if (objXmlLifestyleQuality["contributetolimit"]?.InnerText == "no")
                _blnContributeToLimit = false;
            objXmlLifestyleQuality.TryGetStringFieldQuickly("notes", ref _strNotes);
            objXmlLifestyleQuality.TryGetStringFieldQuickly("source", ref _strSource);
            objXmlLifestyleQuality.TryGetStringFieldQuickly("page", ref _strPage);
            string strAllowedFreeLifestyles = string.Empty;
            if (objXmlLifestyleQuality.TryGetStringFieldQuickly("allowed", ref strAllowedFreeLifestyles))
                _lstAllowedFreeLifestyles = strAllowedFreeLifestyles.Split(',').ToList();
            if (objNode.Text.Contains('('))
            {
                _strExtra = objNode.Text.Split('(')[1].TrimEnd(')');
            }

            // If the item grants a bonus, pass the information to the Improvement Manager.
            if (objXmlLifestyleQuality.InnerXml.Contains("<bonus>"))
            {
                if (!ImprovementManager.CreateImprovements(objCharacter, Improvement.ImprovementSource.Quality, _guiID.ToString(), objXmlLifestyleQuality["bonus"], false, 1, DisplayNameShort(GlobalOptions.Language)))
                {
                    _guiID = Guid.Empty;
                    return;
                }
                if (!string.IsNullOrEmpty(ImprovementManager.SelectedValue))
                {
                    _strExtra = ImprovementManager.SelectedValue;
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
            objNode.Text = FormattedDisplayName(GlobalOptions.CultureInfo, GlobalOptions.Language);
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
            objWriter.WriteElementString("cost", _strCost);
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
            if (objNode.TryGetStringFieldQuickly("name", ref _strName))
                _objCachedMyXmlNode = null;
            if (!objNode.TryGetField("id", Guid.TryParse, out _SourceGuid))
            {
                GetNode()?.TryGetField("id", Guid.TryParse, out _SourceGuid);
            }
            objNode.TryGetStringFieldQuickly("extra", ref _strExtra);
            objNode.TryGetInt32FieldQuickly("lp", ref _intLP);
            objNode.TryGetStringFieldQuickly("cost", ref _strCost);
            objNode.TryGetInt32FieldQuickly("multiplier", ref _intMultiplier);
            objNode.TryGetInt32FieldQuickly("basemultiplier", ref _intBaseMultiplier);
            objNode.TryGetBoolFieldQuickly("contributetolimit", ref _blnContributeToLimit);
            objNode.TryGetBoolFieldQuickly("print", ref _blnPrint);
            if (objNode["lifestylequalitytype"] != null)
                _objLifestyleQualityType = ConvertToLifestyleQualityType(objNode["lifestylequalitytype"].InnerText);
#if DEBUG
            if (objNode["lifestylequalitysource"] != null)
                _objLifestyleQualitySource = ConvertToLifestyleQualitySource(objNode["lifestylequalitysource"].InnerText);
#else
            _objLifestyleQualitySource = QualitySource.Selected;
#endif
            objNode.TryGetStringFieldQuickly("source", ref _strSource);
            objNode.TryGetStringFieldQuickly("page", ref _strPage);
            string strAllowedFreeLifestyles = string.Empty;
            if (!objNode.TryGetStringFieldQuickly("allowed", ref strAllowedFreeLifestyles))
            {
                strAllowedFreeLifestyles = GetNode()?["allowed"]?.InnerText ?? string.Empty;
            }
            _lstAllowedFreeLifestyles = strAllowedFreeLifestyles.Split(',').ToList();
            _nodBonus = objNode["bonus"];
            objNode.TryGetStringFieldQuickly("notes", ref _strNotes);
            
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
                XmlDocument objXmlDocument = XmlManager.Load("lifestyles.xml");
                XmlNode objLifestyleQualityNode = objXmlDocument.SelectSingleNode("/chummer/qualities/quality[id = \"" + _guiID + "\"]") ??
                                                  objXmlDocument.SelectSingleNode("/chummer/qualities/quality[name = \"" + _strName + "\"]");
                if (objLifestyleQualityNode == null)
                {
                    var lstQualities = new List<ListItem>();
                    foreach (XmlNode objNode in objXmlDocument.SelectNodes("/chummer/qualities/quality"))
                    {
                        string strName = objNode["name"].InnerText;
                        lstQualities.Add(new ListItem(strName, objNode["translate"]?.InnerText ?? strName));
                    }
                    var frmSelect = new frmSelectItem
                    {
                        GeneralItems = lstQualities,
                        Description = LanguageManager.GetString("String_CannotFindLifestyleQuality", GlobalOptions.Language).Replace("{0}", _strName)
                    };
                    frmSelect.ShowDialog();
                    if (frmSelect.DialogResult == DialogResult.Cancel)
                        return;

                    objLifestyleQualityNode = objXmlDocument.SelectSingleNode("/chummer/qualities/quality[name = \"" + frmSelect.SelectedItem + "\"]");
                }
                int intTemp = 0;
                string strTemp = string.Empty;
                if (objLifestyleQualityNode.TryGetStringFieldQuickly("cost", ref strTemp))
                    CostString = strTemp;
                if (objLifestyleQualityNode.TryGetInt32FieldQuickly("lp", ref intTemp))
                    LP = intTemp;
                if (objLifestyleQualityNode.TryGetInt32FieldQuickly("areamaximum", ref intTemp))
                    AreaMaximum = intTemp;
                if (objLifestyleQualityNode.TryGetInt32FieldQuickly("comfortsmaximum", ref intTemp))
                    ComfortMaximum = intTemp;
                if (objLifestyleQualityNode.TryGetInt32FieldQuickly("securitymaximum", ref intTemp))
                    SecurityMaximum = intTemp;
                if (objLifestyleQualityNode.TryGetInt32FieldQuickly("area", ref intTemp))
                    Area = intTemp;
                if (objLifestyleQualityNode.TryGetInt32FieldQuickly("comforts", ref intTemp))
                    Comfort = intTemp;
                if (objLifestyleQualityNode.TryGetInt32FieldQuickly("security", ref intTemp))
                    Security = intTemp;
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
        public void Print(XmlTextWriter objWriter, CultureInfo objCulture, string strLanguageToPrint)
        {
            if (!_blnPrint) return;
            objWriter.WriteStartElement("quality");
            objWriter.WriteElementString("name", DisplayNameShort(strLanguageToPrint));
            objWriter.WriteElementString("formattedname", FormattedDisplayName(objCulture, strLanguageToPrint));
            objWriter.WriteElementString("extra", LanguageManager.TranslateExtra(_strExtra, strLanguageToPrint));
            objWriter.WriteElementString("lp", _intLP.ToString(objCulture));
            objWriter.WriteElementString("cost", Cost.ToString(_objCharacter.Options.NuyenFormat, objCulture));
            string strLifestyleQualityType = _objLifestyleQualityType.ToString();
            if (strLanguageToPrint != GlobalOptions.DefaultLanguage)
            {
                XmlDocument objXmlDocument = XmlManager.Load("lifestyles.xml", strLanguageToPrint);

                XmlNode objNode = objXmlDocument?.SelectSingleNode("/chummer/categories/category[. = \"" + strLifestyleQualityType + "\"]");
                strLifestyleQualityType = objNode?.Attributes?["translate"]?.InnerText ?? strLifestyleQualityType;
            }
            objWriter.WriteElementString("lifestylequalitytype", strLifestyleQualityType);
            objWriter.WriteElementString("lifestylequalitytype_english", _objLifestyleQualityType.ToString());
            objWriter.WriteElementString("lifestylequalitysource", _objLifestyleQualitySource.ToString());
            objWriter.WriteElementString("source", CommonFunctions.LanguageBookShort(Source, strLanguageToPrint));
            objWriter.WriteElementString("page", Page(strLanguageToPrint));
            if (_objCharacter.Options.PrintNotes)
                objWriter.WriteElementString("notes", _strNotes);
            objWriter.WriteEndElement();
        }
#endregion

#region Properties
        /// <summary>
        /// Internal identifier which will be used to identify this LifestyleQuality in the Improvement system.
        /// </summary>
        public string InternalId => _guiID.ToString();

        /// <summary>
        /// Source identifier that will be used to identify this Lifestyle Quality in data.
        /// </summary>
        public string SourceID => _SourceGuid.ToString();

        /// <summary>
        /// LifestyleQuality's name.
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
        /// LifestyleQuality's parent lifestyle.
        /// </summary>
        public Lifestyle ParentLifestyle
        {
            get => _objParentLifestyle;
            set => _objParentLifestyle = value;
        }

        /// <summary>
        /// Extra information that should be applied to the name, like a linked CharacterAttribute.
        /// </summary>
        public string Extra
        {
            get => _strExtra;
            set => _strExtra = value;
        }

        /// <summary>
        /// Sourcebook.
        /// </summary>
        public string Source
        {
            get => _strSource;
            set => _strSource = value;
        }

        public string SourceTooltip
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(_strTooltipSource)) return _strTooltipSource;
                XmlDocument objBookDocument = XmlManager.Load("books.xml");
                XmlNode objXmlBook = objBookDocument.SelectSingleNode("/chummer/books/book[code = \"" + _strSource + "\"]");
                _strTooltipSource = $"{objXmlBook["name"].InnerText} {LanguageManager.GetString("String_Page", GlobalOptions.Language)} {Page(GlobalOptions.Language)}";
                return _strTooltipSource;
            }
        }

        /// <summary>
        /// Page Number.
        /// </summary>
        public string Page(string strLanguage)
        {
            if (strLanguage == GlobalOptions.DefaultLanguage)
                return _strPage;

            return GetNode(strLanguage)?["altpage"]?.InnerText ?? _strPage;
        }

        /// <summary>
        /// Bonus node from the XML file.
        /// </summary>
        public XmlNode Bonus
        {
            get => _nodBonus;
            set => _nodBonus = value;
        }

        /// <summary>
        /// LifestyleQuality Type.
        /// </summary>
        public QualityType Type => _objLifestyleQualityType;

        /// <summary>
        /// Source of the LifestyleQuality.
        /// </summary>
        public QualitySource OriginSource
        {
            get => _objLifestyleQualitySource;
            set => _objLifestyleQualitySource = value;
        }

        /// <summary>
        /// Number of Build Points the LifestyleQuality costs.
        /// </summary>
        public int LP
        {
            get => Free ? 0 : _intLP;
            set => _intLP = value;
        }

        /// <summary>
        /// The name of the object as it should be displayed on printouts (translated name only).
        /// </summary>
        public string DisplayNameShort(string strLanguage)
        {
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

            if (!string.IsNullOrEmpty(_strExtra))
            {
                // Attempt to retrieve the CharacterAttribute name.
                strReturn += " (" + LanguageManager.TranslateExtra(_strExtra, strLanguage) + ")";
            }
            return strReturn;
        }

        public string FormattedDisplayName(CultureInfo objCulture, string strLanguage)
        {
            string strReturn = DisplayName(strLanguage);

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
                strReturn += " [+" + Cost.ToString(_objCharacter.Options.NuyenFormat, objCulture) + "¥]";
            }
            return strReturn;
        }

        /// <summary>
        /// Whether or not the LifestyleQuality appears on the printouts.
        /// </summary>
        public bool AllowPrint
        {
            get => _blnPrint;
            set => _blnPrint = value;
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
        /// Nuyen cost of the Quality.
        /// </summary>
        public decimal Cost
        {
            get
            {
                if (Free || FreeByLifestyle)
                    return 0;
                if (!decimal.TryParse(_strCost, NumberStyles.Any, GlobalOptions.InvariantCultureInfo, out decimal decReturn))
                {
                    try
                    {
                        decReturn = Convert.ToDecimal(CommonFunctions.EvaluateInvariantXPath(_strCost));
                    }
                    catch (XPathException)
                    {
                        decReturn = 0.0m;
                    }
                }
                return decReturn;
            }
        }

        /// <summary>
        /// String for the nuyen cost of the Quality.
        /// </summary>
        public string CostString
        {
            get => _strCost;
            set => _strCost = value;
        }

        /// <summary>
        /// Does the Quality have a Nuyen or LP cost?
        /// </summary>
        public bool Free
        {
            get => _blnFree || OriginSource == QualitySource.BuiltIn;
            set => _blnFree = value;
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
        /// Comfort LP is increased/reduced by this Quality. 
        /// </summary>
        public int Comfort
        {
            get => _comfort;
            set => _comfort = value;
        }

        /// <summary>
        /// Comfort LP maximum is increased/reduced by this Quality. 
        /// </summary>
        public int ComfortMaximum
        {
            get => _comfortMaximum;
            set => _comfortMaximum = value;
        }

        /// <summary>
        /// Security LP value is increased/reduced by this Quality. 
        /// </summary>
        public int SecurityMaximum
        {
            get => _securityMaximum;
            set => _securityMaximum = value;
        }

        /// <summary>
        /// Security LP value is increased/reduced by this Quality. 
        /// </summary>
        public int Security
        {
            get => _security;
            set => _security= value;
        }

        /// <summary>
        /// Percentage by which the quality increases the overall Lifestyle Cost.
        /// </summary>
        public int Multiplier
        {
            get => (Free || FreeByLifestyle) ? 0 : _intMultiplier;
            set => _intMultiplier = value;
        }

        /// <summary>
        /// Percentage by which the quality increases the Lifestyle Cost ONLY, without affecting other qualities.
        /// </summary>
        public int BaseMultiplier
        {
            get => (Free || FreeByLifestyle) ? 0 : _intBaseMultiplier;
            set => _intBaseMultiplier = value;
        }

        /// <summary>
        /// Category of the Quality. 
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// Area/Neighborhood LP Cost/Benefit of the Quality.
        /// </summary>
        public int AreaMaximum
        {
            get => _areaMaximum;
            set => _areaMaximum = value;
        }

        /// <summary>
        /// Area/Neighborhood minimum is increased/reduced by this Quality. 
        /// </summary>
        public int Area
        {
            get => _area;
            set => _area = value;
        }
        
        private int _area;
        private int _comfort;
        private int _security;
        private int _areaMaximum;
        private int _comfortMaximum;
        private int _securityMaximum;

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
                _objCachedMyXmlNode = XmlManager.Load("lifestyles.xml", strLanguage)?.SelectSingleNode("/chummer/qualities/quality[name = \"" + Name + "\"]");
                _strCachedXmlNodeLanguage = strLanguage;
            }
            return _objCachedMyXmlNode;
        }
        #endregion
    }
}
