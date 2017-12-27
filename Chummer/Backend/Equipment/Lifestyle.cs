using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
// ReSharper disable ConvertToAutoProperty

namespace Chummer.Backend.Equipment
{
    /// <summary>
    /// Lifestyle.
    /// </summary>
    public class Lifestyle : IHasInternalId, IHasName, IHasXmlNode
    {
        // ReSharper disable once InconsistentNaming
        private Guid _guiID;
        // ReSharper disable once InconsistentNaming
        private Guid _sourceID;
        private string _strName = string.Empty;
        private decimal _decCost;
        private int _intDice;
        private decimal _decMultiplier;
        private int _intMonths = 1;
        private int _intRoommates;
        private decimal _decPercentage = 100.0m;
        private bool _blnPurchased;
        private int _intEntertainment;
        private int _intComforts;
        private int _intArea;
        private int _intSecurity;
        private int _intBaseComforts;
        private int _intBaseArea;
        private int _intBaseSecurity;
        private bool _primaryTenant;
        private int _costForSecurity;
        private int _costForArea;
        private int _costForComforts;
        private string _strBaseLifestyle = string.Empty;
        private string _strSource = string.Empty;
        private string _strPage = string.Empty;
        private bool _blnTrustFund;
        private LifestyleType _objType = LifestyleType.Standard;
        private readonly List<LifestyleQuality> _lstLifestyleQualities = new List<LifestyleQuality>();
        private string _strNotes = string.Empty;
        private readonly Character _objCharacter;

        #region Helper Methods
        /// <summary>
        /// Convert a string to a LifestyleType.
        /// </summary>
        /// <param name="strValue">String value to convert.</param>
        public static LifestyleType ConverToLifestyleType(string strValue)
        {
            switch (strValue)
            {
                case "BoltHole":
                    return LifestyleType.BoltHole;
                case "Safehouse":
                    return LifestyleType.Safehouse;
                case "Advanced":
                    return LifestyleType.Advanced;
                default:
                    return LifestyleType.Standard;
            }
        }
        #endregion

        #region Constructor, Create, Save, Load, and Print Methods
        public Lifestyle(Character objCharacter)
        {
            // Create the GUID for the new Lifestyle.
            _guiID = Guid.NewGuid();
            _objCharacter = objCharacter;
        }

        /// Create a Lifestyle from an XmlNode and return the TreeNodes for it.
        /// <param name="objXmlLifestyle">XmlNode to create the object from.</param>
        /// <param name="objNode">TreeNode to populate a TreeView.</param>
        public void Create(XmlNode objXmlLifestyle, TreeNode objNode)
        {
            objXmlLifestyle.TryGetStringFieldQuickly("name", ref _strBaseLifestyle);
            objXmlLifestyle.TryGetDecFieldQuickly("cost", ref _decCost);
            objXmlLifestyle.TryGetInt32FieldQuickly("dice", ref _intDice);
            objXmlLifestyle.TryGetDecFieldQuickly("multiplier", ref _decMultiplier);
            objXmlLifestyle.TryGetStringFieldQuickly("source", ref _strSource);
            objXmlLifestyle.TryGetStringFieldQuickly("page", ref _strPage);
            objXmlLifestyle.TryGetStringFieldQuickly("notes", ref _strNotes);
            if (!objXmlLifestyle.TryGetField("id", Guid.TryParse, out _sourceID))
            {
                Log.Warning(new object[] { "Missing id field for lifestyle xmlnode", objXmlLifestyle});

                if (System.Diagnostics.Debugger.IsAttached)
                    System.Diagnostics.Debugger.Break();
            }
            else
                _objCachedMyXmlNode = null;

            objNode.Text = DisplayName(GlobalOptions.Language);
            objNode.Tag = _guiID;
        }

        /// <summary>
        /// Save the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        public void Save(XmlTextWriter objWriter)
        {
            objWriter.WriteStartElement("lifestyle");
            objWriter.WriteElementString("guid", _guiID.ToString());
            objWriter.WriteElementString("name", _strName);
            objWriter.WriteElementString("cost", _decCost.ToString(CultureInfo.InvariantCulture));
            objWriter.WriteElementString("dice", _intDice.ToString(CultureInfo.InvariantCulture));
            objWriter.WriteElementString("baselifestyle", _strBaseLifestyle);
            objWriter.WriteElementString("multiplier", _decMultiplier.ToString(CultureInfo.InvariantCulture));
            objWriter.WriteElementString("months", _intMonths.ToString(CultureInfo.InvariantCulture));
            objWriter.WriteElementString("roommates", _intRoommates.ToString(CultureInfo.InvariantCulture));
            objWriter.WriteElementString("percentage", _decPercentage.ToString(CultureInfo.InvariantCulture));
            objWriter.WriteElementString("purchased", _blnPurchased.ToString());
            objWriter.WriteElementString("area", _intArea.ToString(CultureInfo.InvariantCulture));
            objWriter.WriteElementString("comforts", _intComforts.ToString(CultureInfo.InvariantCulture));
            objWriter.WriteElementString("security", _intSecurity.ToString(CultureInfo.InvariantCulture));
            objWriter.WriteElementString("basearea", _intBaseArea.ToString(CultureInfo.InvariantCulture));
            objWriter.WriteElementString("basecomforts", _intBaseComforts.ToString(CultureInfo.InvariantCulture));
            objWriter.WriteElementString("basesecurity", _intBaseSecurity.ToString(CultureInfo.InvariantCulture));
            objWriter.WriteElementString("entertainment", _intEntertainment.ToString(CultureInfo.InvariantCulture));
            objWriter.WriteElementString("costforearea", _costForArea.ToString(CultureInfo.InvariantCulture));
            objWriter.WriteElementString("costforcomforts", _costForComforts.ToString(CultureInfo.InvariantCulture));
            objWriter.WriteElementString("costforsecurity", _costForSecurity.ToString(CultureInfo.InvariantCulture));
            objWriter.WriteElementString("source", _strSource);
            objWriter.WriteElementString("page", _strPage);
            objWriter.WriteElementString("trustfund", _blnTrustFund.ToString());
            objWriter.WriteElementString("primarytenant", _primaryTenant.ToString());
            objWriter.WriteElementString("type", _objType.ToString());
            objWriter.WriteElementString("sourceid", SourceID.ToString());
            objWriter.WriteStartElement("lifestylequalities");
            foreach (var objQuality in _lstLifestyleQualities)
            {
                objQuality.Save(objWriter);
            }
            objWriter.WriteEndElement();
            objWriter.WriteStartElement("freegrids");
            foreach (var objQuality in FreeGrids)
            {
                objQuality.Save(objWriter);
            }
            objWriter.WriteEndElement();
            objWriter.WriteElementString("notes", _strNotes);
            objWriter.WriteEndElement();
            _objCharacter.SourceProcess(_strSource);
        }

        /// <summary>
        /// Load the CharacterAttribute from the XmlNode.
        /// </summary>
        /// <param name="objNode">XmlNode to load.</param>
        /// <param name="blnCopy"></param>
        public void Load(XmlNode objNode, bool blnCopy = false)
        {
            //Can't out property and no backing field
            if (objNode.TryGetField("sourceid", Guid.TryParse, out Guid source))
            {
                SourceID = source;
            }

            if (blnCopy)
            {
                _guiID = Guid.NewGuid();
                _intMonths = 0;
            }
            else
            {
                objNode.TryGetInt32FieldQuickly("months", ref _intMonths);
                objNode.TryGetField("guid", Guid.TryParse, out _guiID);
            }

            objNode.TryGetStringFieldQuickly("name", ref _strName);
            objNode.TryGetDecFieldQuickly("cost", ref _decCost);
            objNode.TryGetInt32FieldQuickly("dice", ref _intDice);
            objNode.TryGetDecFieldQuickly("multiplier", ref _decMultiplier);

            objNode.TryGetInt32FieldQuickly("area", ref _intArea);
            objNode.TryGetInt32FieldQuickly("comforts", ref _intComforts);
            objNode.TryGetInt32FieldQuickly("security", ref _intSecurity);
            objNode.TryGetInt32FieldQuickly("basearea", ref _intBaseArea);
            objNode.TryGetInt32FieldQuickly("basecomforts", ref _intBaseComforts);
            objNode.TryGetInt32FieldQuickly("basesecurity", ref _intBaseSecurity);
            objNode.TryGetInt32FieldQuickly("costforarea", ref _costForArea);
            objNode.TryGetInt32FieldQuickly("costforcomforts", ref _costForComforts);
            objNode.TryGetInt32FieldQuickly("costforsecurity", ref _costForSecurity);
            objNode.TryGetInt32FieldQuickly("roommates", ref _intRoommates);
            objNode.TryGetDecFieldQuickly("percentage", ref _decPercentage);
            objNode.TryGetBoolFieldQuickly("purchased", ref _blnPurchased);
            objNode.TryGetStringFieldQuickly("baselifestyle", ref _strBaseLifestyle);
            if (string.IsNullOrWhiteSpace(_strBaseLifestyle))
            {
                objNode.TryGetStringFieldQuickly("lifestylename", ref _strBaseLifestyle);
                if (string.IsNullOrWhiteSpace(_strBaseLifestyle))
                {
                    XmlDocument objXmlDocument = XmlManager.Load("lifestyles.xml");
                    var lstQualities = new List<ListItem>();
                    foreach (XmlNode n in objXmlDocument.SelectNodes("/chummer/lifestyles/lifestyle"))
                    {
                        string strName = n["name"].InnerText;
                        lstQualities.Add(new ListItem(strName, n["translate"]?.InnerText ?? strName));
                    }
                    var frmSelect = new frmSelectItem
                    {
                        GeneralItems = lstQualities,
                        Description = LanguageManager.GetString("String_CannotFindLifestyle", GlobalOptions.Language).Replace("{0}", _strName)
                    };
                    frmSelect.ShowDialog();
                    if (frmSelect.DialogResult == DialogResult.Cancel)
                        return;
                    _strBaseLifestyle = frmSelect.SelectedItem;
                }
            }
            if (_strBaseLifestyle == "Middle")
                _strBaseLifestyle = "Medium";
            objNode.TryGetStringFieldQuickly("source", ref _strSource);
            objNode.TryGetBoolFieldQuickly("trustfund", ref _blnTrustFund);
            if (objNode["primarytenant"] == null)
            {
                _primaryTenant = _intRoommates == 0;
            }
            else
            {
                objNode.TryGetBoolFieldQuickly("primarytenant", ref _primaryTenant);
            }
            objNode.TryGetStringFieldQuickly("page", ref _strPage);

            // Lifestyle Qualities
            var objXmlNodeList = objNode.SelectNodes("lifestylequalities/lifestylequality");
            if (objXmlNodeList != null)
                foreach (XmlNode objXmlQuality in objXmlNodeList)
                {
                    var objQuality = new LifestyleQuality(_objCharacter);
                    objQuality.Load(objXmlQuality, this);
                    _lstLifestyleQualities.Add(objQuality);
                }

            // Free Grids provided by the Lifestyle
            objXmlNodeList = objNode.SelectNodes("freegrids/lifestylequality");
            if (objXmlNodeList != null)
                foreach (XmlNode objXmlQuality in objXmlNodeList)
                {
                    var objQuality = new LifestyleQuality(_objCharacter);
                    objQuality.Load(objXmlQuality, this);
                    FreeGrids.Add(objQuality);
                }

            objNode.TryGetStringFieldQuickly("notes", ref _strNotes);

            var strtemp = string.Empty;
            if (objNode.TryGetStringFieldQuickly("type", ref strtemp))
            {
                _objType = ConverToLifestyleType(strtemp);
            }
            LegacyShim(objNode);
        }

        /// <summary>
        /// Converts old lifestyle structures to new standards. 
        /// </summary>
        private void LegacyShim(XmlNode n)
        {
            //Lifestyles would previously store the entire calculated value of their Cost, Area, Comforts and Security. Better to have it be a volatile Complex Property. 
            if (_objCharacter.LastSavedVersion <= Version.Parse("5.197.0") && n["costforarea"] == null)
            {
                var objXmlDocument = XmlManager.Load("lifestyles.xml");
                var objLifestyleQualityNode = objXmlDocument.SelectSingleNode("/chummer/lifestyles/lifestyle[name = \"" + _strBaseLifestyle + "\"]");
                if (objLifestyleQualityNode?["costforarea"] != null)
                {
                    Cost = Convert.ToInt32(objLifestyleQualityNode["cost"]?.InnerText);
                    CostForArea = Convert.ToInt32(objLifestyleQualityNode["costforarea"]?.InnerText);
                    CostForComforts = Convert.ToInt32(objLifestyleQualityNode["costforcomforts"]?.InnerText);
                    CostForSecurity = Convert.ToInt32(objLifestyleQualityNode["costforsecurity"]?.InnerText);
                }

                int intMinArea = 0;
                int intMinComfort = 0;
                int intMinSec = 0;

                // Calculate the limits of the 3 aspects.
                // Area.
                XmlNode objXmlNode = objXmlDocument.SelectSingleNode("/chummer/neighborhoods/neighborhood[name = \"" + _strBaseLifestyle + "\"]");
                objXmlNode.TryGetInt32FieldQuickly("minimum", ref intMinArea);
                BaseArea = intMinArea;
                // Comforts.
                objXmlNode = objXmlDocument.SelectSingleNode("/chummer/comforts/comfort[name = \"" + _strBaseLifestyle + "\"]");
                objXmlNode.TryGetInt32FieldQuickly("minimum", ref intMinComfort);
                BaseComforts = intMinComfort;
                // Security.
                objXmlNode = objXmlDocument.SelectSingleNode("/chummer/securities/security[name = \"" + _strBaseLifestyle + "\"]");
                objXmlNode.TryGetInt32FieldQuickly("minimum", ref intMinSec);
                BaseSecurity = intMinSec;

                n.TryGetInt32FieldQuickly("area", ref intMinArea);
                n.TryGetInt32FieldQuickly("comforts", ref intMinComfort);
                n.TryGetInt32FieldQuickly("security", ref intMinSec);

                // Calculate the cost of Positive Qualities.
                foreach (LifestyleQuality objQuality in LifestyleQualities)
                {
                    intMinArea -= objQuality.Area;
                    intMinComfort -= objQuality.Comfort;
                    intMinSec -= objQuality.Security;
                }
                Area = Math.Max(intMinArea - BaseArea, 0);
                Comforts = Math.Max(intMinComfort - BaseComforts, 0);
                Security = Math.Max(intMinSec - BaseSecurity, 0);
            }
        }

        /// <summary>
        /// Print the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        /// <param name="objCulture">Culture info that is used for conversion of decimals.</param>
        public void Print(XmlTextWriter objWriter, CultureInfo objCulture, string strLanguageToPrint)
        {
            objWriter.WriteStartElement("lifestyle");
            objWriter.WriteElementString("name", Name);
            objWriter.WriteElementString("cost", _decCost.ToString(_objCharacter.Options.NuyenFormat, objCulture));
            objWriter.WriteElementString("totalmonthlycost", TotalMonthlyCost.ToString(_objCharacter.Options.NuyenFormat, objCulture));
            objWriter.WriteElementString("totalcost", TotalCost.ToString(_objCharacter.Options.NuyenFormat, objCulture));
            objWriter.WriteElementString("dice", _intDice.ToString(objCulture));
            objWriter.WriteElementString("multiplier", _decMultiplier.ToString(_objCharacter.Options.NuyenFormat, objCulture));
            objWriter.WriteElementString("months", _intMonths.ToString(objCulture));
            objWriter.WriteElementString("purchased", _blnPurchased.ToString());
            objWriter.WriteElementString("type", _objType.ToString());
            objWriter.WriteElementString("sourceid", SourceID.ToString());
            var strBaseLifestyle = string.Empty;

            // Retrieve the Advanced Lifestyle information if applicable.
            if (!string.IsNullOrEmpty(_strBaseLifestyle))
            {
                XmlNode objXmlAspect = GetNode();
                if (objXmlAspect != null)
                {
                    if (objXmlAspect["translate"] != null)
                        strBaseLifestyle = objXmlAspect["translate"].InnerText;
                    else if (objXmlAspect["name"] != null)
                        strBaseLifestyle = objXmlAspect["name"].InnerText;
                }
            }

            objWriter.WriteElementString("baselifestyle", strBaseLifestyle);
            objWriter.WriteElementString("trustfund", _blnTrustFund.ToString());
            objWriter.WriteElementString("source", CommonFunctions.LanguageBookShort(Source, strLanguageToPrint));
            objWriter.WriteElementString("page", DisplayPage(strLanguageToPrint));
            objWriter.WriteStartElement("qualities");

            // Retrieve the Qualities for the Advanced Lifestyle if applicable.
            if (_lstLifestyleQualities.Count > 0)
            {
                foreach (var objQuality in _lstLifestyleQualities)
                {
                    objQuality.Print(objWriter, objCulture, strLanguageToPrint);
                }
            }
            // Retrieve the free Grids for the Advanced Lifestyle if applicable.
            if (FreeGrids.Count > 0)
            {
                foreach (var objQuality in FreeGrids)
                {
                    var strThisQuality = objQuality.DisplayName(strLanguageToPrint);
                    objWriter.WriteElementString("quality", strThisQuality);
                }
            }
            objWriter.WriteEndElement();
            if (_objCharacter.Options.PrintNotes)
                objWriter.WriteElementString("notes", _strNotes);
            objWriter.WriteEndElement();
        }
        #endregion

        #region Properties
        /// <summary>
        /// Internal identifier which will be used to identify this Lifestyle in the Improvement system.
        /// </summary>
        public string InternalId => _guiID.ToString();

        public IList<LifestyleQuality> FreeGrids { get; } = new List<LifestyleQuality>();

        // ReSharper disable once InconsistentNaming
        public Guid SourceID
        {
            get => _sourceID;
            set
            {
                if (_sourceID != Guid.Empty)
                {
                    throw new InvalidOperationException("Source ID can only be set once");
                }

                if (_sourceID != value)
                    _objCachedMyXmlNode = null;
                _sourceID = value;
            }
        }

        /// <summary>
        /// A custom name for the Lifestyle assigned by the player.
        /// </summary>
        public string Name
        {
            get => _strName;
            set => _strName = value;
        }

        /// <summary>
        /// The name of the object as it should be displayed on printouts (translated name only).
        /// </summary>
        public string DisplayNameShort(string strLanguage)
        {
            // Get the translated name if applicable.
            if (strLanguage == GlobalOptions.DefaultLanguage)
                return BaseLifestyle;

            return GetNode(strLanguage)?["translate"]?.InnerText ?? BaseLifestyle;
        }

        /// <summary>
        /// The name of the object as it should be displayed in lists. Name (Extra).
        /// </summary>
        public string DisplayName(string strLanguage)
        {
            string strReturn = DisplayNameShort(strLanguage);

            if (!string.IsNullOrEmpty(Name))
                strReturn += " (\"" + Name + "\")";

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
        /// Sourcebook Page Number.
        /// </summary>
        public string Page
        {
            get => _strPage;
            set => _strPage = value;
        }

        public string DisplayPage(string strLanguage)
        {
            // Get the translated name if applicable.
            if (strLanguage == GlobalOptions.DefaultLanguage)
                return _strPage;

            return GetNode(strLanguage)?["altpage"]?.InnerText ?? _strPage;
        }

        /// <summary>
        /// Cost.
        /// </summary>
        public decimal Cost
        {
            get => _decCost;
            set => _decCost = value;
        }

        /// <summary>
        /// Number of dice the character rolls to determine their statring Nuyen.
        /// </summary>
        public int Dice
        {
            get => _intDice;
            set => _intDice = value;
        }

        /// <summary>
        /// Number the character multiplies the dice roll with to determine their starting Nuyen.
        /// </summary>
        public decimal Multiplier
        {
            get => _decMultiplier;
            set => _decMultiplier = value;
        }

        /// <summary>
        /// Months purchased.
        /// </summary>
        public int Months
        {
            get => _intMonths;
            set => _intMonths = value;
        }

        /// <summary>
        /// Whether or not the Lifestyle has been Purchased and no longer rented.
        /// </summary>
        public bool Purchased
        {
            get => _blnPurchased;
            set => _blnPurchased = value;
        }

        /// <summary>
        /// Base Lifestyle.
        /// </summary>
        public string BaseLifestyle
        {
            get => _strBaseLifestyle;
            set => _strBaseLifestyle = value;
        }
        /// <summary>
        /// Advance Lifestyle Comforts.
        /// </summary>
        public int Comforts
        {
            get => _intComforts;
            set => _intComforts = value;
        }
        /// <summary>
        /// Base level of Comforts.
        /// </summary>
        public int BaseComforts
        {
            get => _intBaseComforts;
            set => _intBaseComforts = value;
        }

        /// <summary>
        /// Advance Lifestyle Neighborhood Entertainment.
        /// </summary>
        public int BaseArea
        {
            get => _intBaseArea;
            set => _intBaseArea = value;
        }

        /// <summary>
        /// Advance Lifestyle Security Entertainment.
        /// </summary>
        public int BaseSecurity
        {
            get => _intBaseSecurity;
            set => _intBaseSecurity = value;
        }
        /// <summary>
        /// Advance Lifestyle Comforts.
        /// </summary>
        public int Entertainment
        {
            get => _intEntertainment;
            set => _intEntertainment = value;
        }

        /// <summary>
        /// Advance Lifestyle Neighborhood.
        /// </summary>
        public int Area
        {
            get => _intArea;
            set => _intArea = value;
        }

        /// <summary>
        /// Advance Lifestyle Security.
        /// </summary>
        public int Security
        {
            get => _intSecurity;
            set => _intSecurity = value;
        }
        /// <summary>
        /// Advanced Lifestyle Qualities.
        /// </summary>
        public IList<LifestyleQuality> LifestyleQualities => _lstLifestyleQualities;

        /// <summary>
        /// Notes.
        /// </summary>
        public string Notes
        {
            get => _strNotes;
            set => _strNotes = value;
        }

        /// <summary>
        /// Type of the Lifestyle.
        /// </summary>
        public LifestyleType StyleType
        {
            get => _objType;
            set => _objType = value;
        }

        /// <summary>
        /// Number of Roommates this Lifestyle is shared with.
        /// </summary>
        public int Roommates
        {
            get => _intRoommates;
            set => _intRoommates = value;
        }

        /// <summary>
        /// Percentage of the total cost the character pays per month.
        /// </summary>
        public decimal Percentage
        {
            get => _decPercentage;
            set => _decPercentage = value;
        }

        /// <summary>
        /// Whether the lifestyle is currently covered by the Trust Fund Quality.
        /// </summary>
        public bool TrustFund
        {
            get => _blnTrustFund;
            set => _blnTrustFund = value;
        }

        /// <summary>
        /// Whether the character is the primary tenant for the Lifestyle. 
        /// </summary>
        public bool PrimaryTenant
        {
            get => _primaryTenant;
            set => _primaryTenant = value;
        }

        /// <summary>
        /// Nuyen cost for each point of upgraded Security. Expected to be zero for lifestyles other than Street.
        /// </summary>
        public int CostForArea
        {
            get => _costForArea;
            set => _costForArea = value;
        }

        /// <summary>
        /// Nuyen cost for each point of upgraded Security. Expected to be zero for lifestyles other than Street.
        /// </summary>
        public int CostForComforts
        {
            get => _costForComforts;
            set => _costForComforts = value;
        }

        /// <summary>
        /// Nuyen cost for each point of upgraded Security. Expected to be zero for lifestyles other than Street.
        /// </summary>
        public int CostForSecurity
        {
            get => _costForSecurity;
            set => _costForSecurity = value;
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
                _objCachedMyXmlNode = XmlManager.Load("lifestyles.xml", strLanguage)?.SelectSingleNode("/chummer/lifestyles/lifestyle[id = \"" + SourceID.ToString().TrimStart('{').TrimEnd('}') + "\"]");
                _strCachedXmlNodeLanguage = strLanguage;
            }
            return _objCachedMyXmlNode;
        }
        #endregion

        #region Complex Properties
        /// <summary>
        /// Total cost of the Lifestyle, counting all purchased months.
        /// </summary>
        public decimal TotalCost => TotalMonthlyCost * _intMonths;

        public decimal CostMultiplier
        {
            get
            {
                decimal d = (Roommates + Area + Comforts + Security) * 10;
                d += Convert.ToDecimal(ImprovementManager.ValueOf(_objCharacter, Improvement.ImprovementType.LifestyleCost), GlobalOptions.InvariantCultureInfo);
                if (_objType == LifestyleType.Standard)
                   d += Convert.ToDecimal(ImprovementManager.ValueOf(_objCharacter, Improvement.ImprovementType.BasicLifestyleCost), GlobalOptions.InvariantCultureInfo);
                d += _lstLifestyleQualities.Sum(lq => lq.Multiplier);
                return d / 100;
            }
        }

        /// <summary>
        /// Total Area of the Lifestyle, including all Lifestyle qualities.
        /// </summary>
        public int TotalArea => BaseArea + Area + LifestyleQualities.Sum(lq => lq.Area);

        /// <summary>
        /// Total Comfort of the Lifestyle, including all Lifestyle qualities.
        /// </summary>
        public int TotalComforts => BaseComforts + Comforts + LifestyleQualities.Sum(lq => lq.Comfort);

        /// <summary>
        /// Total Security of the Lifestyle, including all Lifestyle qualities.
        /// </summary>
        public int TotalSecurity => BaseSecurity + Security + LifestyleQualities.Sum(lq => lq.Security);

        /// <summary>
        /// Base cost of the Lifestyle itself, including all multipliers from Improvements, qualities and upgraded attributes.
        /// </summary>
        public decimal BaseCost => Cost * Math.Max(CostMultiplier + BaseCostMultiplier, 1);

        /// <summary>
        /// Base Cost Multiplier from any Lifestyle Qualities the Lifestyle has. 
        /// </summary>
        public decimal BaseCostMultiplier => Convert.ToDecimal(_lstLifestyleQualities.Sum(lq => lq.BaseMultiplier) / 100, GlobalOptions.InvariantCultureInfo);

        /// <summary>
        /// Total monthly cost of the Lifestyle.
        /// </summary>
        public decimal TotalMonthlyCost
        {
            get
            {
                decimal decReturn = 0;
                decReturn += Area * CostForArea;
                decReturn += Comforts * CostForComforts;
                decReturn += Security * CostForSecurity;

                decimal decExtraAssetCost = 0;
                decimal decContractCost = 0;
                foreach (var objQuality in _lstLifestyleQualities)
                {
                    //Add the flat cost from Qualities.
                    if (objQuality.Category == "Contracts")
                        decContractCost += objQuality.Cost;
                    else
                        decExtraAssetCost += objQuality.Cost;
                }

                if (!_blnTrustFund)
                {
                    decReturn += BaseCost;
                }
                decReturn += decExtraAssetCost * CostMultiplier;
                if (!PrimaryTenant)
                {
                    decReturn /= _intRoommates + 1.0m;
                }
                decReturn *= Percentage /100;
                decReturn += decContractCost;
                return decReturn;
            }
        }

        public static string GetEquivalentLifestyle(string strLifestyle)
        {
            switch (strLifestyle)
            {
                case "Bolt Hole":
                    return "Squatter";
                case "Traveler":
                    return "Low";
                case "Commercial":
                    return "Medium";
            }
            return strLifestyle.StartsWith("Hospitalized") ? "High" : strLifestyle;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Set the InternalId for the Lifestyle. Used when editing an Advanced Lifestyle.
        /// </summary>
        /// <param name="strInternalId">InternalId to set.</param>
        public void SetInternalId(string strInternalId)
        {
            _guiID = Guid.Parse(strInternalId);
        }
        #endregion
    }
}
