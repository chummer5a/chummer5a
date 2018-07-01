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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
// ReSharper disable ConvertToAutoProperty

namespace Chummer.Backend.Equipment
{
    /// <summary>
    /// Type of Lifestyle.
    /// </summary>
    public enum LifestyleIncrement
    {
        Month = 0,
        Week = 1,
        Day = 2,
    }

    /// <summary>
    /// Lifestyle.
    /// </summary>
    [DebuggerDisplay("{DisplayName(GlobalOptions.DefaultLanguage)}")]
    public class Lifestyle : IHasInternalId, IHasXmlNode, IHasNotes, ICanRemove, IHasCustomName
    {
        // ReSharper disable once InconsistentNaming
        private Guid _guiID;
        // ReSharper disable once InconsistentNaming
        private Guid _sourceID;
        private string _strName = string.Empty;
        private decimal _decCost;
        private int _intDice;
        private decimal _decMultiplier;
        private int _intIncrements = 1;
        private int _intRoommates;
        private decimal _decPercentage = 100.0m;
        private int _intComforts;
        private int _intArea;
        private int _intSecurity;
        private int _intBaseComforts;
        private int _intBaseArea;
        private int _intBaseSecurity;
        private int _intBonusLP;
        private bool _blnAllowBonusLP;
        private bool _blnIsPrimaryTenant;
        private decimal _decCostForSecurity;
        private decimal _decCostForArea;
        private decimal _decCostForComforts;
        private string _strBaseLifestyle = string.Empty;
        private string _strSource = string.Empty;
        private string _strPage = string.Empty;
        private bool _blnTrustFund;
        private LifestyleType _eType = LifestyleType.Standard;
        private LifestyleIncrement _eIncrement = LifestyleIncrement.Month;
        private readonly ObservableCollection<LifestyleQuality> _lstLifestyleQualities = new ObservableCollection<LifestyleQuality>();
        private readonly ObservableCollection<LifestyleQuality> _lstFreeGrids = new ObservableCollection<LifestyleQuality>();
        private string _strNotes = string.Empty;
        private readonly Character _objCharacter;

        #region Helper Methods
        /// <summary>
        /// Convert a string to a LifestyleType.
        /// </summary>
        /// <param name="strValue">String value to convert.</param>
        public static LifestyleType ConvertToLifestyleType(string strValue)
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

        /// <summary>
        /// Convert a string to a LifestyleType.
        /// </summary>
        /// <param name="strValue">String value to convert.</param>
        public static LifestyleIncrement ConvertToLifestyleIncrement(string strValue)
        {
            switch (strValue)
            {
                case "day":
                case "Day":
                    return LifestyleIncrement.Day;
                case "week":
                case "Week":
                    return LifestyleIncrement.Week;
                default:
                    return LifestyleIncrement.Month;
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
        public void Create(XmlNode objXmlLifestyle)
        {
            objXmlLifestyle.TryGetStringFieldQuickly("name", ref _strBaseLifestyle);
            objXmlLifestyle.TryGetDecFieldQuickly("cost", ref _decCost);
            objXmlLifestyle.TryGetInt32FieldQuickly("dice", ref _intDice);
            objXmlLifestyle.TryGetDecFieldQuickly("multiplier", ref _decMultiplier);
            objXmlLifestyle.TryGetStringFieldQuickly("source", ref _strSource);
            objXmlLifestyle.TryGetStringFieldQuickly("page", ref _strPage);
            objXmlLifestyle.TryGetDecFieldQuickly("costforarea", ref _decCostForArea);
            objXmlLifestyle.TryGetDecFieldQuickly("costforcomforts", ref _decCostForComforts);
            objXmlLifestyle.TryGetDecFieldQuickly("costforsecurity", ref _decCostForSecurity);
            objXmlLifestyle.TryGetBoolFieldQuickly("allowbonuslp", ref _blnAllowBonusLP);
            if (!objXmlLifestyle.TryGetStringFieldQuickly("altnotes", ref _strNotes))
                objXmlLifestyle.TryGetStringFieldQuickly("notes", ref _strNotes);
            if (!objXmlLifestyle.TryGetField("id", Guid.TryParse, out _sourceID))
            {
                Log.Warning(new object[] { "Missing id field for lifestyle xmlnode", objXmlLifestyle});

                Utils.BreakIfDebug();
            }
            else
                _objCachedMyXmlNode = null;
            string strTemp = string.Empty;
            if (objXmlLifestyle.TryGetStringFieldQuickly("increment", ref strTemp))
                _eIncrement = ConvertToLifestyleIncrement(strTemp);

            using (XmlNodeList lstGridNodes = objXmlLifestyle.SelectNodes("freegrids/freegrid"))
            {
                if (lstGridNodes?.Count > 0)
                {
                    FreeGrids.Clear();
                    XmlDocument xmlLifestyleDocument = XmlManager.Load("lifestyles.xml");
                    foreach (XmlNode xmlNode in lstGridNodes)
                    {
                        XmlNode xmlQuality = xmlLifestyleDocument.SelectSingleNode("/chummer/qualities/quality[name = \"" + xmlNode.InnerText + "\"]");
                        LifestyleQuality objQuality = new LifestyleQuality(_objCharacter);
                        string strPush = xmlNode.Attributes?["select"]?.InnerText;
                        if (!string.IsNullOrWhiteSpace(strPush))
                        {
                            _objCharacter.Pushtext.Push(strPush);
                        }

                        objQuality.Create(xmlQuality, this, _objCharacter, QualitySource.BuiltIn);

                        FreeGrids.Add(objQuality);
                    }
                }
            }
        }

        /// <summary>
        /// Save the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        public void Save(XmlTextWriter objWriter)
        {
            objWriter.WriteStartElement("lifestyle");
            objWriter.WriteElementString("guid", _guiID.ToString("D"));
            objWriter.WriteElementString("name", _strName);
            objWriter.WriteElementString("cost", _decCost.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("dice", _intDice.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("baselifestyle", _strBaseLifestyle);
            objWriter.WriteElementString("multiplier", _decMultiplier.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("months", _intIncrements.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("roommates", _intRoommates.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("percentage", _decPercentage.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("area", _intArea.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("comforts", _intComforts.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("security", _intSecurity.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("basearea", _intBaseArea.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("basecomforts", _intBaseComforts.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("basesecurity", _intBaseSecurity.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("costforearea", _decCostForArea.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("costforcomforts", _decCostForComforts.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("costforsecurity", _decCostForSecurity.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("allowbonuslp", _blnAllowBonusLP.ToString());
            objWriter.WriteElementString("bonuslp", _intBonusLP.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("source", _strSource);
            objWriter.WriteElementString("page", _strPage);
            objWriter.WriteElementString("trustfund", _blnTrustFund.ToString());
            objWriter.WriteElementString("primarytenant", _blnIsPrimaryTenant.ToString());
            objWriter.WriteElementString("type", _eType.ToString());
            objWriter.WriteElementString("increment", _eIncrement.ToString());
            objWriter.WriteElementString("sourceid", SourceID.ToString("D"));
            objWriter.WriteStartElement("lifestylequalities");
            foreach (LifestyleQuality objQuality in _lstLifestyleQualities)
            {
                objQuality.Save(objWriter);
            }
            objWriter.WriteEndElement();
            objWriter.WriteStartElement("freegrids");
            foreach (LifestyleQuality objQuality in _lstFreeGrids)
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
                _intIncrements = 0;
            }
            else
            {
                objNode.TryGetInt32FieldQuickly("months", ref _intIncrements);
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
            objNode.TryGetDecFieldQuickly("costforarea", ref _decCostForArea);
            objNode.TryGetDecFieldQuickly("costforcomforts", ref _decCostForComforts);
            objNode.TryGetDecFieldQuickly("costforsecurity", ref _decCostForSecurity);
            objNode.TryGetInt32FieldQuickly("roommates", ref _intRoommates);
            objNode.TryGetDecFieldQuickly("percentage", ref _decPercentage);
            objNode.TryGetStringFieldQuickly("baselifestyle", ref _strBaseLifestyle);
            if (XmlManager.Load("lifestyles.xml").SelectSingleNode($"/chummer/lifestyles/lifestyle[name =\"{_strBaseLifestyle}\"]") == null && XmlManager.Load("lifestyles.xml").SelectSingleNode($"/chummer/lifestyles/lifestyle[name =\"{_strName}\"]") != null)
            {
                string baselifestyle = _strName;
                _strName = _strBaseLifestyle;
                _strBaseLifestyle = baselifestyle;
            }
            if (string.IsNullOrWhiteSpace(_strBaseLifestyle))
            {
                objNode.TryGetStringFieldQuickly("lifestylename", ref _strBaseLifestyle);
                if (string.IsNullOrWhiteSpace(_strBaseLifestyle))
                    {
                        List<ListItem> lstQualities = new List<ListItem>();
                        using (XmlNodeList xmlLifestyleList = XmlManager.Load("lifestyles.xml").SelectNodes("/chummer/lifestyles/lifestyle"))
                            if (xmlLifestyleList != null)
                                foreach (XmlNode xmlLifestyle in xmlLifestyleList)
                                {
                                    string strName = xmlLifestyle["name"]?.InnerText ?? LanguageManager.GetString("String_Error", GlobalOptions.Language);
                                    lstQualities.Add(new ListItem(strName, xmlLifestyle["translate"]?.InnerText ?? strName));
                                }
                        frmSelectItem frmSelect = new frmSelectItem
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
            if (!objNode.TryGetBoolFieldQuickly("allowbonuslp", ref _blnAllowBonusLP))
                GetNode()?.TryGetBoolFieldQuickly("allowbonuslp", ref _blnAllowBonusLP);
            if (!objNode.TryGetInt32FieldQuickly("bonuslp", ref _intBonusLP) && _strBaseLifestyle == "Traveler")
                _intBonusLP = 1 + GlobalOptions.RandomGenerator.NextD6ModuloBiasRemoved();
            objNode.TryGetStringFieldQuickly("source", ref _strSource);
            objNode.TryGetBoolFieldQuickly("trustfund", ref _blnTrustFund);
            if (objNode["primarytenant"] == null)
            {
                _blnIsPrimaryTenant = _intRoommates == 0;
            }
            else
            {
                objNode.TryGetBoolFieldQuickly("primarytenant", ref _blnIsPrimaryTenant);
            }
            objNode.TryGetStringFieldQuickly("page", ref _strPage);

            // Lifestyle Qualities
            using (XmlNodeList xmlQualityList = objNode.SelectNodes("lifestylequalities/lifestylequality"))
                if (xmlQualityList != null)
                    foreach (XmlNode xmlQuality in xmlQualityList)
                    {
                        LifestyleQuality objQuality = new LifestyleQuality(_objCharacter);
                        objQuality.Load(xmlQuality, this);
                        _lstLifestyleQualities.Add(objQuality);
                    }

            // Free Grids provided by the Lifestyle
            using (XmlNodeList xmlQualityList = objNode.SelectNodes("freegrids/lifestylequality"))
                if (xmlQualityList != null)
                    foreach (XmlNode xmlQuality in xmlQualityList)
                    {
                        LifestyleQuality objQuality = new LifestyleQuality(_objCharacter);
                        objQuality.Load(xmlQuality, this);
                        _lstFreeGrids.Add(objQuality);
                    }

            objNode.TryGetStringFieldQuickly("notes", ref _strNotes);

            string strTemp = string.Empty;
            if (objNode.TryGetStringFieldQuickly("type", ref strTemp))
            {
                _eType = ConvertToLifestyleType(strTemp);
            }
            if (objNode.TryGetStringFieldQuickly("increment", ref strTemp))
            {
                _eIncrement = ConvertToLifestyleIncrement(strTemp);
            }
            else if (_eType == LifestyleType.Safehouse)
                _eIncrement = LifestyleIncrement.Week;
            else
            {
                XmlNode xmlLifestyleNode = GetNode();
                if (xmlLifestyleNode != null && xmlLifestyleNode.TryGetStringFieldQuickly("increment", ref strTemp))
                {
                    _eIncrement = ConvertToLifestyleIncrement(strTemp);
                }
            }
            LegacyShim(objNode);
        }

        /// <summary>
        /// Converts old lifestyle structures to new standards.
        /// </summary>
        private void LegacyShim(XmlNode xmlLifestyleNode)
        {
            //Lifestyles would previously store the entire calculated value of their Cost, Area, Comforts and Security. Better to have it be a volatile Complex Property.
            if (_objCharacter.LastSavedVersion <= new Version("5.197.0") && xmlLifestyleNode["costforarea"] == null)
            {
                XmlDocument objXmlDocument = XmlManager.Load("lifestyles.xml");
                XmlNode objLifestyleQualityNode = objXmlDocument.SelectSingleNode("/chummer/lifestyles/lifestyle[name = \"" + _strBaseLifestyle + "\"]");
                if (objLifestyleQualityNode != null)
                {
                    decimal decTemp = 0.0m;
                    if (objLifestyleQualityNode.TryGetDecFieldQuickly("cost", ref decTemp))
                        Cost = decTemp;
                    if (objLifestyleQualityNode.TryGetDecFieldQuickly("costforarea", ref decTemp))
                        CostForArea = decTemp;
                    if (objLifestyleQualityNode.TryGetDecFieldQuickly("costforcomforts", ref decTemp))
                        CostForComforts = decTemp;
                    if (objLifestyleQualityNode.TryGetDecFieldQuickly("costforsecurity", ref decTemp))
                        CostForSecurity = decTemp;
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

                xmlLifestyleNode.TryGetInt32FieldQuickly("area", ref intMinArea);
                xmlLifestyleNode.TryGetInt32FieldQuickly("comforts", ref intMinComfort);
                xmlLifestyleNode.TryGetInt32FieldQuickly("security", ref intMinSec);

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
        /// <param name="objCulture">Culture in which to print.</param>
        /// <param name="strLanguageToPrint">Language in which to print</param>
        public void Print(XmlTextWriter objWriter, CultureInfo objCulture, string strLanguageToPrint)
        {
            objWriter.WriteStartElement("lifestyle");
            objWriter.WriteElementString("name", CustomName);
            objWriter.WriteElementString("cost", Cost.ToString(_objCharacter.Options.NuyenFormat, objCulture));
            objWriter.WriteElementString("totalmonthlycost", TotalMonthlyCost.ToString(_objCharacter.Options.NuyenFormat, objCulture));
            objWriter.WriteElementString("totalcost", TotalCost.ToString(_objCharacter.Options.NuyenFormat, objCulture));
            objWriter.WriteElementString("dice", Dice.ToString(objCulture));
            objWriter.WriteElementString("multiplier", Multiplier.ToString(_objCharacter.Options.NuyenFormat, objCulture));
            objWriter.WriteElementString("months", Increments.ToString(objCulture));
            objWriter.WriteElementString("purchased", Purchased.ToString());
            objWriter.WriteElementString("type", StyleType.ToString());
            objWriter.WriteElementString("increment", IncrementType.ToString());
            objWriter.WriteElementString("sourceid", SourceID.ToString("D"));
            objWriter.WriteElementString("bonuslp", BonusLP.ToString(objCulture));
            string strBaseLifestyle = string.Empty;

            // Retrieve the Advanced Lifestyle information if applicable.
            if (!string.IsNullOrEmpty(BaseLifestyle))
            {
                XmlNode objXmlAspect = GetNode();
                if (objXmlAspect != null)
                {
                    strBaseLifestyle = objXmlAspect["translate"]?.InnerText ?? objXmlAspect["name"]?.InnerText ?? strBaseLifestyle;
                }
            }

            objWriter.WriteElementString("baselifestyle", strBaseLifestyle);
            objWriter.WriteElementString("trustfund", TrustFund.ToString());
            objWriter.WriteElementString("source", CommonFunctions.LanguageBookShort(Source, strLanguageToPrint));
            objWriter.WriteElementString("page", DisplayPage(strLanguageToPrint));
            objWriter.WriteStartElement("qualities");

            // Retrieve the Qualities for the Advanced Lifestyle if applicable.
            foreach (LifestyleQuality objQuality in LifestyleQualities)
            {
                objQuality.Print(objWriter, objCulture, strLanguageToPrint);
            }
            // Retrieve the free Grids for the Advanced Lifestyle if applicable.
            foreach (LifestyleQuality objQuality in FreeGrids)
            {
                objQuality.Print(objWriter, objCulture, strLanguageToPrint);
            }
            objWriter.WriteEndElement();
            if (_objCharacter.Options.PrintNotes)
                objWriter.WriteElementString("notes", Notes);
            objWriter.WriteEndElement();
        }
        #endregion

        #region Properties
        /// <summary>
        /// Internal identifier which will be used to identify this Lifestyle in the Improvement system.
        /// </summary>
        public string InternalId => _guiID.ToString("D");

        public ObservableCollection<LifestyleQuality> FreeGrids => _lstFreeGrids;

        // ReSharper disable once InconsistentNaming
        public Guid SourceID
        {
            get => _sourceID;
            set
            {
                if (_sourceID != value)
                {
                    _objCachedMyXmlNode = null;
                    _sourceID = value;
                }
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

        public string CustomName
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

            if (!string.IsNullOrEmpty(CustomName))
                strReturn += " (\"" + CustomName + "\")";

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
                return Page;

            return GetNode(strLanguage)?["altpage"]?.InnerText ?? Page;
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
        /// Number of dice the character rolls to determine their starting Nuyen.
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
        /// Months/Weeks/Days purchased.
        /// </summary>
        public int Increments
        {
            get => _intIncrements;
            set => _intIncrements = value;
        }

        /// <summary>
        /// Whether or not the Lifestyle has been Purchased and no longer rented.
        /// </summary>
        public bool Purchased => Increments >= IncrementsRequiredForPermanent;

        public int IncrementsRequiredForPermanent
        {
            get
            {
                switch (IncrementType)
                {
                    case LifestyleIncrement.Day:
                        return 3044; // 30.436875 days per month on average * 100 months, rounded up
                    case LifestyleIncrement.Week:
                        return 435; // 4.348125 weeks per month on average * 100 months, rounded up
                    default:
                        return 100;
                }
            }
        }

        /// <summary>
        /// Base Lifestyle.
        /// </summary>
        public string BaseLifestyle
        {
            get => _strBaseLifestyle;
            set
            {
                if (_strBaseLifestyle != value)
                {
                    _strBaseLifestyle = value;
                    XmlDocument xmlLifestyleDocument = XmlManager.Load("lifestyles.xml");
                    // This needs a handler for translations, will fix later.
                    if (value == "Bolt Hole")
                    {
                        if (LifestyleQualities.All(x => x.Name != "Not a Home"))
                        {
                            XmlNode xmlQuality = xmlLifestyleDocument.SelectSingleNode("/chummer/qualities/quality[name = \"Not a Home\"]");
                            LifestyleQuality objQuality = new LifestyleQuality(_objCharacter);
                            objQuality.Create(xmlQuality, this, _objCharacter, QualitySource.BuiltIn);

                            LifestyleQualities.Add(objQuality);
                        }
                    }
                    else
                    {
                        foreach (LifestyleQuality objQuality in LifestyleQualities.ToList())
                        {
                            //Bolt Holes automatically come with the Not a Home quality.
                            if (objQuality.Name == "Not a Home" || objQuality.Name == "Dug a Hole")
                            {
                                LifestyleQualities.Remove(objQuality);
                            }
                        }
                    }

                    XmlNode xmlLifestyle = xmlLifestyleDocument.SelectSingleNode("/chummer/lifestyles/lifestyle[name = \"" + value + "\"]");
                    if (xmlLifestyle != null)
                    {
                        Create(xmlLifestyle);
                    }
                }
            }
        }

        /// <summary>
        /// Free Lifestyle points from Traveler lifestyle.
        /// </summary>
        public int BonusLP
        {
            get => _intBonusLP;
            set => _intBonusLP = value;
        }

        public bool AllowBonusLP => _blnAllowBonusLP;

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
        public ObservableCollection<LifestyleQuality> LifestyleQualities => _lstLifestyleQualities;

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
            get => _eType;
            set => _eType = value;
        }

        /// <summary>
        /// Interval of payments required for the Lifestyle.
        /// </summary>
        public LifestyleIncrement IncrementType
        {
            get => _eIncrement;
            set => _eIncrement = value;
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
            get => _blnTrustFund && IsTrustFundEligible;
            set => _blnTrustFund = value;
        }

        public bool IsTrustFundEligible
        {
            get
            {
                switch (_objCharacter.TrustFund)
                {
                    case 1:
                    case 4:
                        return BaseLifestyle == "Medium";
                    case 2:
                        return BaseLifestyle == "Low";
                    case 3:
                        return BaseLifestyle == "High";
                }

                return false;
            }
        }

        /// <summary>
        /// Whether the character is the primary tenant for the Lifestyle.
        /// </summary>
        public bool PrimaryTenant
        {
            get => _blnIsPrimaryTenant;
            set => _blnIsPrimaryTenant = value;
        }

        /// <summary>
        /// Nuyen cost for each point of upgraded Security. Expected to be zero for lifestyles other than Street.
        /// </summary>
        public decimal CostForArea
        {
            get => _decCostForArea;
            set => _decCostForArea = value;
        }

        /// <summary>
        /// Nuyen cost for each point of upgraded Security. Expected to be zero for lifestyles other than Street.
        /// </summary>
        public decimal CostForComforts
        {
            get => _decCostForComforts;
            set => _decCostForComforts = value;
        }

        /// <summary>
        /// Nuyen cost for each point of upgraded Security. Expected to be zero for lifestyles other than Street.
        /// </summary>
        public decimal CostForSecurity
        {
            get => _decCostForSecurity;
            set => _decCostForSecurity = value;
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
                _objCachedMyXmlNode = XmlManager.Load("lifestyles.xml", strLanguage).SelectSingleNode("/chummer/lifestyles/lifestyle[id = \"" + SourceID.ToString("D") + "\"]");
                _strCachedXmlNodeLanguage = strLanguage;
            }
            return _objCachedMyXmlNode;
        }
        #endregion

        #region Complex Properties
        /// <summary>
        /// Total cost of the Lifestyle, counting all purchased months.
        /// </summary>
        public decimal TotalCost => TotalMonthlyCost * Increments;

        public decimal CostMultiplier
        {
            get
            {
                decimal d = (Roommates + Area + Comforts + Security) * 10;
                d += Convert.ToDecimal(ImprovementManager.ValueOf(_objCharacter, Improvement.ImprovementType.LifestyleCost), GlobalOptions.InvariantCultureInfo);
                if (_eType == LifestyleType.Standard)
                   d += Convert.ToDecimal(ImprovementManager.ValueOf(_objCharacter, Improvement.ImprovementType.BasicLifestyleCost), GlobalOptions.InvariantCultureInfo);
                d += LifestyleQualities.Sum(lq => lq.Multiplier);
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
        public decimal BaseCostMultiplier => Convert.ToDecimal(LifestyleQualities.Sum(lq => lq.BaseMultiplier) / 100, GlobalOptions.InvariantCultureInfo);

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
                foreach (LifestyleQuality objQuality in LifestyleQualities)
                {
                    //Add the flat cost from Qualities.
                    if (objQuality.Type == QualityType.Contracts)
                        decContractCost += objQuality.Cost;
                    else
                        decExtraAssetCost += objQuality.Cost;
                }

                if (!TrustFund)
                {
                    decReturn += BaseCost;
                }
                decReturn += decExtraAssetCost;
                decReturn *= CostMultiplier + 1.0m;
                if (!PrimaryTenant)
                {
                    decReturn /= _intRoommates + 1.0m;
                }
                decReturn *= Percentage /100;

                switch (IncrementType)
                {
                    case LifestyleIncrement.Day:
                        decContractCost /= (4.34812m * 7);
                        break;
                    case LifestyleIncrement.Week:
                        decContractCost /= (4.34812m);
                        break;
                }

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
            if (Guid.TryParse(strInternalId, out Guid guiTemp))
                _guiID = guiTemp;
        }

        /// <summary>
        /// Purchases an additional month of the selected lifestyle. 
        /// </summary>
        /// <param name="CharacterObject">Character to use.</param>
        public void IncrementMonths(Character CharacterObject)
        {
            // Create the Expense Log Entry.
            decimal decAmount = TotalMonthlyCost;
                if (decAmount > CharacterObject.Nuyen)
            {
                MessageBox.Show(LanguageManager.GetString("Message_NotEnoughNuyen", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_NotEnoughNuyen", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            ExpenseLogEntry objExpense = new ExpenseLogEntry(CharacterObject);
            objExpense.Create(decAmount* -1, LanguageManager.GetString("String_ExpenseLifestyle", GlobalOptions.Language) + ' ' + DisplayNameShort(GlobalOptions.Language), ExpenseType.Nuyen, DateTime.Now);
            CharacterObject.ExpenseEntries.AddWithSort(objExpense);
            CharacterObject.Nuyen -= decAmount;

            ExpenseUndo objUndo = new ExpenseUndo();
            objUndo.CreateNuyen(NuyenExpenseType.IncreaseLifestyle, InternalId);
            objExpense.Undo = objUndo;

            Increments += 1;
        }
        #region UI Methods
        public TreeNode CreateTreeNode(ContextMenuStrip cmsBasicLifestyle, ContextMenuStrip cmsAdvancedLifestyle)
        {
            //if (!string.IsNullOrEmpty(ParentID) && !string.IsNullOrEmpty(Source) && !_objCharacter.Options.BookEnabled(Source))
            //return null;

            TreeNode objNode = new TreeNode
            {
                Name = InternalId,
                Text = DisplayName(GlobalOptions.Language),
                Tag = this,
                ContextMenuStrip = StyleType == LifestyleType.Standard ? cmsBasicLifestyle : cmsAdvancedLifestyle,
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
        #endregion

        public bool Remove(Character characterObject)
        {
            return characterObject.ConfirmDelete(LanguageManager.GetString("Message_DeleteLifestyle", GlobalOptions.Language)) && characterObject.Lifestyles.Remove(this);
        }
    }
}
