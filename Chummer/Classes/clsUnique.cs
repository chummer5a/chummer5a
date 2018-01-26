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
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;
using Chummer.Backend.Equipment;
using Chummer.Backend.Skills;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.ComponentModel;
using Chummer.Backend;

namespace Chummer
{
    public class TextEventArgs : EventArgs
    {
        private readonly string _strText;

        public TextEventArgs(string strText)
        {
            _strText = strText;
        }

        public string Text => _strText;
    }

    /// <summary>
    /// Type of Quality.
    /// </summary>
    public enum QualityType
    {
        Positive = 0,
        Negative = 1,
        LifeModule = 2,
        Entertainment = 3,
        Contracts = 4
    }

    /// <summary>
    /// Source of the Quality.
    /// </summary>
    public enum QualitySource
    {
        Selected = 0,
        Metatype = 1,
        MetatypeRemovable = 2,
        BuiltIn = 3,
        LifeModule = 4,
        Improvement = 5,
    }

    /// <summary>
    /// Reason a quality is not valid
    /// </summary>
    [Flags]
    public enum QualityFailureReason
    {
        Allowed = 0x0,
        LimitExceeded = 0x1,
        RequiredSingle = 0x2,
        RequiredMultiple = 0x4,
        ForbiddenSingle = 0x8,
        MetatypeRequired = 0x10,
    }

    /// <summary>
    /// A Quality.
    /// </summary>
    public class Quality : IHasInternalId, IHasName, IHasXmlNode
    {
        private Guid _guiID;
        private string _strName = string.Empty;
        private bool _blnMetagenetic = false;
        private string _strExtra = string.Empty;
        private string _strSource = string.Empty;
        private string _strPage = string.Empty;
        private bool _blnMutant = false;
        private string _strNotes = string.Empty;
        private bool _blnImplemented = true;
        private bool _blnContributeToLimit = true;
        private bool _blnPrint = true;
        private bool _blnDoubleCostCareer = true;
        private bool _blnCanBuyWithSpellPoints = false;
        private int _intBP = 0;
        private QualityType _eQualityType = QualityType.Positive;
        private QualitySource _eQualitySource = QualitySource.Selected;
        private string _strSourceName = string.Empty;
        private XmlNode _nodBonus;
        private XmlNode _nodFirstLevelBonus;
        private XmlNode _nodDiscounts;
        private readonly Character _objCharacter;
        private Guid _guiWeaponID;
        private Guid _qualiyGuid;
        private string _stage;

        public String Stage
        {
            get => _stage;
            private set => _stage = value;
        }

        #region Helper Methods
        /// <summary>
        /// Convert a string to a QualityType.
        /// </summary>
        /// <param name="strValue">String value to convert.</param>
        public static QualityType ConvertToQualityType(string strValue)
        {
            if (string.IsNullOrEmpty(strValue))
                return default(QualityType);
            switch (strValue)
            {
                case "Negative":
                    return QualityType.Negative;
                case "LifeModule":
                    return QualityType.LifeModule;
                default:
                    return QualityType.Positive;
            }
        }

        /// <summary>
        /// Convert a string to a QualitySource.
        /// </summary>
        /// <param name="strValue">String value to convert.</param>
        public static QualitySource ConvertToQualitySource(string strValue)
        {
            if (string.IsNullOrEmpty(strValue))
                return default(QualitySource);
            switch (strValue)
            {
                case "Metatype":
                    return QualitySource.Metatype;
                case "MetatypeRemovable":
                    return QualitySource.MetatypeRemovable;
                case "LifeModule":
                    return QualitySource.LifeModule;
                case "Built-In":
                    return QualitySource.BuiltIn;
                case "Improvement":
                    return QualitySource.Improvement;
                default:
                    return QualitySource.Selected;
            }
        }
        #endregion

        #region Constructor, Create, Save, Load, and Print Methods
        public Quality(Character objCharacter)
        {
            // Create the GUID for the new Quality.
            _guiID = Guid.NewGuid();
            _objCharacter = objCharacter;
        }
        public void SetGUID(Guid guidExisting)
        {
            _guiID = guidExisting;
        }

        /// <summary>
        /// Create a Quality from an XmlNode.
        /// </summary>
        /// <param name="objXmlQuality">XmlNode to create the object from.</param>
        /// <param name="objQualitySource">Source of the Quality.</param>
        /// <param name="lstWeapons">List of Weapons that should be added to the Character.</param>
        /// <param name="strForceValue">Force a value to be selected for the Quality.</param>
        public void Create(XmlNode objXmlQuality, QualitySource objQualitySource, List<Weapon> lstWeapons, string strForceValue = "", string strSourceName = "")
        {
            _strSourceName = strSourceName;
            objXmlQuality.TryGetStringFieldQuickly("name", ref _strName);
            objXmlQuality.TryGetBoolFieldQuickly("metagenetic", ref _blnMetagenetic);
            if (!objXmlQuality.TryGetStringFieldQuickly("altnotes", ref _strNotes))
                objXmlQuality.TryGetStringFieldQuickly("notes", ref _strNotes);
            // Check for a Variable Cost.
            XmlNode objKarmaNode = objXmlQuality["karma"];
            if (objKarmaNode != null)
            {
                string strKarmaNodeTest = objKarmaNode.InnerText;
                if (strKarmaNodeTest.StartsWith("Variable("))
                {
                    decimal decMin = 0.0m;
                    decimal decMax = decimal.MaxValue;
                    string strCost = strKarmaNodeTest.TrimStart("Variable(", true).TrimEnd(')');
                    if (strCost.Contains('-'))
                    {
                        string[] strValues = strCost.Split('-');
                        decMin = Convert.ToDecimal(strValues[0], GlobalOptions.InvariantCultureInfo);
                        decMax = Convert.ToDecimal(strValues[1], GlobalOptions.InvariantCultureInfo);
                    }
                    else
                        decMin = Convert.ToDecimal(strCost.FastEscape('+'), GlobalOptions.InvariantCultureInfo);

                    if (decMin != 0 || decMax != decimal.MaxValue)
                    {
                        frmSelectNumber frmPickNumber = new frmSelectNumber(0);
                        if (decMax > 1000000)
                            decMax = 1000000;
                        frmPickNumber.Minimum = decMin;
                        frmPickNumber.Maximum = decMax;
                        frmPickNumber.Description = LanguageManager.GetString("String_SelectVariableCost", GlobalOptions.Language).Replace("{0}", DisplayNameShort(GlobalOptions.Language));
                        frmPickNumber.AllowCancel = false;
                        frmPickNumber.ShowDialog();
                        _intBP = decimal.ToInt32(frmPickNumber.SelectedValue);
                    }
                }
                else
                {
                    _intBP = Convert.ToInt32(strKarmaNodeTest);
                }
            }
            _eQualityType = ConvertToQualityType(objXmlQuality["category"]?.InnerText);
            _eQualitySource = objQualitySource;
            objXmlQuality.TryGetBoolFieldQuickly("doublecareer", ref _blnDoubleCostCareer);
            objXmlQuality.TryGetBoolFieldQuickly("canbuywithspellpoints", ref _blnCanBuyWithSpellPoints);
            objXmlQuality.TryGetBoolFieldQuickly("print", ref _blnPrint);
            objXmlQuality.TryGetBoolFieldQuickly("implemented", ref _blnImplemented);
            objXmlQuality.TryGetBoolFieldQuickly("contributetolimit", ref _blnContributeToLimit);
            objXmlQuality.TryGetStringFieldQuickly("source", ref _strSource);
            objXmlQuality.TryGetStringFieldQuickly("page", ref _strPage);
            _blnMutant = objXmlQuality["mutant"] != null;

            if (_eQualityType == QualityType.LifeModule)
            {
                objXmlQuality.TryGetStringFieldQuickly("stage", ref _stage);
            }

            if (objXmlQuality["id"] != null)
            {
                _qualiyGuid = Guid.Parse(objXmlQuality["id"].InnerText);
                _objCachedMyXmlNode = null;
            }

            // Add Weapons if applicable.
            if (objXmlQuality.InnerXml.Contains("<addweapon>"))
            {
                XmlDocument objXmlWeaponDocument = XmlManager.Load("weapons.xml");

                // More than one Weapon can be added, so loop through all occurrences.
                if (objXmlWeaponDocument != null)
                {
                    foreach (XmlNode objXmlAddWeapon in objXmlQuality.SelectNodes("addweapon"))
                    {
                        string strLoopID = objXmlAddWeapon.InnerText;
                        XmlNode objXmlWeapon = strLoopID.IsGuid()
                            ? objXmlWeaponDocument.SelectSingleNode("/chummer/weapons/weapon[id = \"" + strLoopID + "\"]")
                            : objXmlWeaponDocument.SelectSingleNode("/chummer/weapons/weapon[name = \"" + strLoopID + "\"]");

                        Weapon objGearWeapon = new Weapon(_objCharacter);
                        objGearWeapon.Create(objXmlWeapon, lstWeapons);
                        objGearWeapon.ParentID = InternalId;
                        lstWeapons.Add(objGearWeapon);

                        _guiWeaponID = Guid.Parse(objGearWeapon.InternalId);
                    }
                }
            }

            if (objXmlQuality.InnerXml.Contains("<naturalweapons>"))
            {
                foreach (XmlNode objXmlNaturalWeapon in objXmlQuality["naturalweapons"].SelectNodes("naturalweapon"))
                {
                    Weapon objWeapon = new Weapon(_objCharacter);
                    if (objXmlNaturalWeapon["name"] != null)
                        objWeapon.Name = objXmlNaturalWeapon["name"].InnerText;
                    objWeapon.Category = LanguageManager.GetString("Tab_Critter", GlobalOptions.Language);
                    objWeapon.WeaponType = "Melee";
                    if (objXmlNaturalWeapon["reach"] != null)
                        objWeapon.Reach = Convert.ToInt32(objXmlNaturalWeapon["reach"].InnerText);
                    if (objXmlNaturalWeapon["accuracy"] != null)
                        objWeapon.Accuracy = objXmlNaturalWeapon["accuracy"].InnerText;
                    if (objXmlNaturalWeapon["damage"] != null)
                        objWeapon.Damage = objXmlNaturalWeapon["damage"].InnerText;
                    if (objXmlNaturalWeapon["ap"] != null)
                        objWeapon.AP = objXmlNaturalWeapon["ap"].InnerText; ;
                    objWeapon.Mode = "0";
                    objWeapon.RC = "0";
                    objWeapon.Concealability = 0;
                    objWeapon.Avail = "0";
                    objWeapon.Cost = "0";
                    if (objXmlNaturalWeapon["useskill"] != null)
                        objWeapon.UseSkill = objXmlNaturalWeapon["useskill"].InnerText;
                    if (objXmlNaturalWeapon["source"] != null)
                        objWeapon.Source = objXmlNaturalWeapon["source"].InnerText;
                    if (objXmlNaturalWeapon["page"] != null)
                        objWeapon.Page = objXmlNaturalWeapon["page"].InnerText;

                    _objCharacter.Weapons.Add(objWeapon);
                }
            }
            _nodDiscounts = objXmlQuality["costdiscount"];
            // If the item grants a bonus, pass the information to the Improvement Manager.
            _nodBonus = objXmlQuality["bonus"];
            if (_nodBonus?.ChildNodes.Count > 0)
            {
                ImprovementManager.ForcedValue = strForceValue;
                if (!ImprovementManager.CreateImprovements(_objCharacter, Improvement.ImprovementSource.Quality, _guiID.ToString("D"), objXmlQuality["bonus"], false, 1, DisplayNameShort(GlobalOptions.Language)))
                {
                    _guiID = Guid.Empty;
                    return;
                }
                if (!string.IsNullOrEmpty(ImprovementManager.SelectedValue))
                {
                    _strExtra = ImprovementManager.SelectedValue;
                }
            }
            else if (!string.IsNullOrEmpty(strForceValue))
            {
                _strExtra = strForceValue;
            }
            _nodFirstLevelBonus = objXmlQuality["firstlevelbonus"];
            if (Levels == 0 && _nodFirstLevelBonus?.ChildNodes.Count > 0)
            {
                ImprovementManager.ForcedValue = strForceValue;
                if (!ImprovementManager.CreateImprovements(_objCharacter, Improvement.ImprovementSource.Quality, _guiID.ToString("D"), objXmlQuality["firstlevelbonus"], false, 1, DisplayNameShort(GlobalOptions.Language)))
                {
                    _guiID = Guid.Empty;
                    return;
                }
            }

            if (string.IsNullOrEmpty(_strNotes))
            {
                string strEnglishNameOnSource = _strName;
                objXmlQuality.TryGetStringFieldQuickly("nameonpage", ref strEnglishNameOnSource);
                _strNotes = CommonFunctions.GetTextFromPDF($"{_strSource} {_strPage}", strEnglishNameOnSource);

                if (string.IsNullOrEmpty(_strNotes))
                {
                    string strTranslatedNameOnSource = DisplayName(GlobalOptions.Language);

                    // don't check if again it is not translated
                    if (strTranslatedNameOnSource != strEnglishNameOnSource)
                    {
                        // if we found <altnameonpage> but it contains the same english name already searched,
                        // so we reset the variable to use DisplayName instead
                        if (objXmlQuality.TryGetStringFieldQuickly("altnameonpage", ref strTranslatedNameOnSource)
                            && strTranslatedNameOnSource == strEnglishNameOnSource)
                            strTranslatedNameOnSource = DisplayName(GlobalOptions.Language);

                        _strNotes = CommonFunctions.GetTextFromPDF($"{Source} {Page(GlobalOptions.Language)}", strTranslatedNameOnSource);
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
            objWriter.WriteStartElement("quality");
            objWriter.WriteElementString("guid", _guiID.ToString("D"));
            objWriter.WriteElementString("name", _strName);
            objWriter.WriteElementString("extra", _strExtra);
            objWriter.WriteElementString("bp", _intBP.ToString(CultureInfo.InvariantCulture));
            objWriter.WriteElementString("implemented", _blnImplemented.ToString());
            objWriter.WriteElementString("contributetolimit", _blnContributeToLimit.ToString());
            objWriter.WriteElementString("doublecareer", _blnDoubleCostCareer.ToString());
            objWriter.WriteElementString("canbuywithspellpoints", _blnCanBuyWithSpellPoints.ToString());
            objWriter.WriteElementString("metagenetic", _blnMetagenetic.ToString());
            objWriter.WriteElementString("print", _blnPrint.ToString());
            objWriter.WriteElementString("qualitytype", _eQualityType.ToString());
            objWriter.WriteElementString("qualitysource", _eQualitySource.ToString());
            objWriter.WriteElementString("mutant", _blnMutant.ToString());
            objWriter.WriteElementString("source", _strSource);
            objWriter.WriteElementString("page", _strPage);
            objWriter.WriteElementString("sourcename", _strSourceName);
            if (_nodBonus != null)
                objWriter.WriteRaw("<bonus>" + _nodBonus.InnerXml + "</bonus>");
            else
                objWriter.WriteElementString("bonus", string.Empty);
            if (_nodFirstLevelBonus != null)
                objWriter.WriteRaw("<firstlevelbonus>" + _nodFirstLevelBonus.InnerXml + "</firstlevelbonus>");
            else
                objWriter.WriteElementString("firstlevelbonus", string.Empty);
            if (_guiWeaponID != Guid.Empty)
                objWriter.WriteElementString("weaponguid", _guiWeaponID.ToString("D"));
            if (_nodDiscounts != null)
                objWriter.WriteRaw("<costdiscount>" + _nodDiscounts.InnerXml + "</costdiscount>");
            objWriter.WriteElementString("notes", _strNotes);
            if (_eQualityType == QualityType.LifeModule)
            {
                objWriter.WriteElementString("stage", _stage);
            }

            if (!_qualiyGuid.Equals(Guid.Empty))
            {
                objWriter.WriteElementString("id", _qualiyGuid.ToString("D"));
            }

            objWriter.WriteEndElement();
            _objCharacter.SourceProcess(_strSource);
        }

        /// <summary>
        /// Load the CharacterAttribute from the XmlNode.
        /// </summary>
        /// <param name="objNode">XmlNode to load.</param>
        public void Load(XmlNode objNode)
        {
            objNode.TryGetField("guid", Guid.TryParse, out _guiID);
            objNode.TryGetStringFieldQuickly("name", ref _strName);
            objNode.TryGetStringFieldQuickly("extra", ref _strExtra);
            objNode.TryGetInt32FieldQuickly("bp", ref _intBP);
            objNode.TryGetBoolFieldQuickly("implemented", ref _blnImplemented);
            objNode.TryGetBoolFieldQuickly("contributetolimit", ref _blnContributeToLimit);
            objNode.TryGetBoolFieldQuickly("print", ref _blnPrint);
            objNode.TryGetBoolFieldQuickly("doublecareer", ref _blnDoubleCostCareer);
            objNode.TryGetBoolFieldQuickly("canbuywithspellpoints", ref _blnCanBuyWithSpellPoints);
            _eQualityType = ConvertToQualityType(objNode["qualitytype"]?.InnerText);
            _eQualitySource = ConvertToQualitySource(objNode["qualitysource"]?.InnerText);
            string strTemp = string.Empty;
            if (objNode.TryGetStringFieldQuickly("metagenetic", ref strTemp))
            {
                _blnMetagenetic = strTemp == bool.TrueString || strTemp == "yes";
            }
            if (objNode.TryGetStringFieldQuickly("mutant", ref strTemp))
            {
                _blnMutant = strTemp == bool.TrueString || strTemp == "yes";
            }
            objNode.TryGetStringFieldQuickly("source", ref _strSource);
            objNode.TryGetStringFieldQuickly("page", ref _strPage);
            objNode.TryGetStringFieldQuickly("sourcename", ref _strSourceName);
            _nodBonus = objNode["bonus"];
            _nodFirstLevelBonus = objNode["firstlevelbonus"] ?? GetNode()?["firstlevelbonus"];
            _nodDiscounts = objNode["costdiscount"];
            objNode.TryGetField("weaponguid", Guid.TryParse, out _guiWeaponID);
            objNode.TryGetStringFieldQuickly("notes", ref _strNotes);

            if (_eQualityType == QualityType.LifeModule)
            {
                objNode.TryGetStringFieldQuickly("stage", ref _stage);
            }
            if (!objNode.TryGetField("id", Guid.TryParse, out _qualiyGuid))
            {
                XmlNode objNewNode = XmlManager.Load("qualities.xml").SelectSingleNode("/chummer/qualities/quality[name = \"" + Name + "\"]");
                if (objNewNode?.TryGetField("id", Guid.TryParse, out _qualiyGuid) == true)
                    _objCachedMyXmlNode = null;
            }
            else
                _objCachedMyXmlNode = null;
        }

        /// <summary>
        /// Print the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        public void Print(XmlTextWriter objWriter, int intRating, CultureInfo objCulture, string strLanguageToPrint)
        {
            if (_blnPrint)
            {
                string strRatingString = string.Empty;
                if (intRating > 1)
                    strRatingString = ' ' + intRating.ToString(objCulture);
                string strSourceName = string.Empty;
                if (!string.IsNullOrWhiteSpace(SourceName))
                    strSourceName = " (" + GetSourceName(strLanguageToPrint) + ')';
                objWriter.WriteStartElement("quality");
                objWriter.WriteElementString("name", DisplayNameShort(strLanguageToPrint));
                objWriter.WriteElementString("name_english", Name + strRatingString);
                objWriter.WriteElementString("extra", LanguageManager.TranslateExtra(Extra, strLanguageToPrint) + strRatingString + strSourceName);
                objWriter.WriteElementString("bp", BP.ToString(objCulture));
                string strQualityType = Type.ToString();
                if (strLanguageToPrint != GlobalOptions.DefaultLanguage)
                {
                    XmlNode objNode = XmlManager.Load("qualities.xml", strLanguageToPrint).SelectSingleNode("/chummer/categories/category[. = \"" + strQualityType + "\"]/@translate");
                    strQualityType = objNode?.InnerText ?? strQualityType;
                }
                objWriter.WriteElementString("qualitytype", strQualityType);
                objWriter.WriteElementString("qualitytype_english", Type.ToString());
                objWriter.WriteElementString("qualitysource", OriginSource.ToString());
                objWriter.WriteElementString("source", CommonFunctions.LanguageBookShort(Source, strLanguageToPrint));
                objWriter.WriteElementString("page", Page(strLanguageToPrint));
                if (_objCharacter.Options.PrintNotes)
                    objWriter.WriteElementString("notes", Notes);
                objWriter.WriteEndElement();
            }
        }
        #endregion

        #region Properties
        /// <summary>
        /// Internal identifier which will be used to identify this Quality in the Improvement system.
        /// </summary>
        public string InternalId => _guiID.ToString("D");

        /// <summary>
        /// Internal identifier for the quality type
        /// </summary>
        public string QualityId => _qualiyGuid.Equals(Guid.Empty) ? string.Empty : _qualiyGuid.ToString("D");

        /// <summary>
        /// Guid of a Weapon.
        /// </summary>
        public string WeaponID
        {
            get => _guiWeaponID.ToString("D");
            set => _guiWeaponID = Guid.Parse(value);
        }

        /// <summary>
        /// Quality's name.
        /// </summary>
        public string Name
        {
            get => _strName;
            set => _strName = value;
        }

        /// <summary>
        /// Does the quality come from being a Changeling?
        /// </summary>
        public bool Metagenetic => _blnMetagenetic;

        /// <summary>
        /// Extra information that should be applied to the name, like a linked CharacterAttribute.
        /// </summary>
        public string Extra
        {
            get => _strExtra;
            set => _strExtra = LanguageManager.ReverseTranslateExtra(value, GlobalOptions.Language);
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
            if (strLanguage == GlobalOptions.DefaultLanguage)
                return _strPage;

            return GetNode(strLanguage)?["altpage"]?.InnerText ?? _strPage;
        }

        /// <summary>
        /// Name of the Improvement that added this quality.
        /// </summary>
        public string SourceName
        {
            get => _strSourceName;
            set => _strSourceName = value;
        }

        /// <summary>
        /// Name of the Improvement that added this quality.
        /// </summary>
        public string GetSourceName(string strLanguage)
        {
            return LanguageManager.TranslateExtra(_strSourceName, strLanguage);
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
        /// Bonus node from the XML file only awarded for the first instance the character has the quality.
        /// </summary>
        public XmlNode FirstLevelBonus
        {
            get => _nodFirstLevelBonus;
            set => _nodFirstLevelBonus = value;
        }

        /// <summary>
        /// Quality Type.
        /// </summary>
        public QualityType Type
        {
            get => _eQualityType;
            set => _eQualityType = value;
        }

        /// <summary>
        /// Source of the Quality.
        /// </summary>
        public QualitySource OriginSource
        {
            get => _eQualitySource;
            set => _eQualitySource = value;
        }

        /// <summary>
        /// Number of Build Points the Quality costs.
        /// </summary>
        public int BP
        {
            get
            {
                string strValue = _nodDiscounts?["value"]?.InnerText;
                if (string.IsNullOrEmpty(strValue))
                    return _intBP;
                int intReturn = _intBP;
                if (_nodDiscounts.RequirementsMet(_objCharacter))
                {
                    if (Type == QualityType.Positive)
                    {
                        intReturn += Convert.ToInt32(strValue);
                    }
                    else if (Type == QualityType.Negative)
                    {
                        intReturn -= Convert.ToInt32(strValue);
                    }
                }
                return intReturn;
            }
            set => _intBP = value;
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
        /// If there is more than one instance of the same quality, it's: Name (Extra) Number
        /// </summary>
        public string DisplayName(string strLanguage)
        {
            string strReturn = DisplayNameShort(strLanguage);

            if (!string.IsNullOrEmpty(_strExtra))
            {
                // Attempt to retrieve the CharacterAttribute name.
                strReturn += " (" + LanguageManager.TranslateExtra(_strExtra, strLanguage) + ')';
            }

            int intLevels = Levels;
            if (intLevels > 1)
                strReturn += ' ' + intLevels.ToString(GlobalOptions.CultureInfo);

            return strReturn;
        }

        /// <summary>
        /// Returns how many instances of this quality there are in the character's quality list
        /// TODO: Actually implement a proper rating system for qualities that plays nice with the Improvements Manager.
        /// </summary>
        public int Levels
        {
            get
            {
                return _objCharacter.Qualities.Count(objExistingQuality => objExistingQuality.QualityId == QualityId && objExistingQuality.Extra == Extra && objExistingQuality.SourceName == SourceName);
            }
        }

        /// <summary>
        /// Whether or not the Quality appears on the printouts.
        /// </summary>
        public bool AllowPrint
        {
            get => _blnPrint;
            set => _blnPrint = value;
        }

        /// <summary>
        /// Whether or not the Qualitie's cost is doubled in Career Mode.
        /// </summary>
        public bool DoubleCost
        {
            get => _blnDoubleCostCareer;
            set => _blnDoubleCostCareer = value;
        }

        /// <summary>
        /// Whether or not the quality can be bought with free spell points instead
        /// </summary>
        public bool CanBuyWithSpellPoints
        {
            get => _blnCanBuyWithSpellPoints;
            set => _blnCanBuyWithSpellPoints = value;
        }

        /// <summary>
        /// Whether or not the Quality has been implemented completely, or needs additional code support.
        /// </summary>
        public bool Implemented
        {
            get => _blnImplemented;
            set => _blnImplemented = value;
        }
        /// <summary>
        /// Whether or not the Quality contributes towards the character's Quality BP limits.
        /// </summary>
        public bool ContributeToLimit
        {
            get
            {
                if (_eQualitySource == QualitySource.Metatype || _eQualitySource == QualitySource.MetatypeRemovable)
                    return false;

                // Positive Metagenetic Qualities are free if you're a Changeling.
                if (Metagenetic && _objCharacter.MetageneticLimit > 0)
                    return false;

                // The Beast's Way and the Spiritual Way get the Mentor Spirit for free.
                if (_strName == "Mentor Spirit" && _objCharacter.Qualities.Any(objQuality => objQuality.Name == "The Beast's Way" || objQuality.Name == "The Spiritual Way"))
                    return false;

                return _blnContributeToLimit;
            }
            set => _blnContributeToLimit = value;
        }

        /// <summary>
        /// Whether or not the Quality contributes towards the character's Total BP.
        /// </summary>
        public bool ContributeToBP
        {
            get
            {
                if (_eQualitySource == QualitySource.Metatype || _eQualitySource == QualitySource.MetatypeRemovable)
                    return false;

                // Positive Metagenetic Qualities are free if you're a Changeling.
                if (Metagenetic && _objCharacter.MetageneticLimit > 0)
                    return false;

                // The Beast's Way and the Spiritual Way get the Mentor Spirit for free.
                if (_strName == "Mentor Spirit" && _objCharacter.Qualities.Any(objQuality => objQuality.Name == "The Beast's Way" || objQuality.Name == "The Spiritual Way"))
                    return false;

                return true;
            }
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
                _objCachedMyXmlNode = XmlManager.Load("qualities.xml", strLanguage).SelectSingleNode("/chummer/qualities/quality[id = \"" + QualityId + "\"]");
                _strCachedXmlNodeLanguage = strLanguage;
            }
            return _objCachedMyXmlNode;
        }
        #endregion

        #region Methods
        public TreeNode CreateTreeNode(ContextMenuStrip cmsQuality)
        {
            TreeNode objNode = new TreeNode
            {
                Name = InternalId,
                Text = DisplayName(GlobalOptions.Language),
                Tag = this,
                ContextMenuStrip = cmsQuality
            };
            if (!Implemented)
            {
                objNode.ForeColor = Color.Red;
            }
            else if (!string.IsNullOrEmpty(Notes))
            {
                objNode.ForeColor = Color.SaddleBrown;
            }
            else if (OriginSource == QualitySource.BuiltIn ||
                OriginSource == QualitySource.Improvement ||
                OriginSource == QualitySource.LifeModule ||
                OriginSource == QualitySource.Metatype)
            {
                objNode.ForeColor = SystemColors.GrayText;
            }
            objNode.ToolTipText = Notes.WordWrap(100);

            return objNode;
        }
        #endregion

        #region Static Methods

        /// <summary>
        /// Retuns weither a quality is valid on said Character
        /// THIS IS A WIP AND ONLY CHECKS QUALITIES. REQUIRED POWERS, METATYPES AND OTHERS WON'T BE CHECKED
        /// </summary>
        /// <param name="objCharacter">The Character</param>
        /// <param name="XmlQuality">The XmlNode describing the quality</param>
        /// <returns>Is the Quality valid on said Character</returns>
        public static bool IsValid(Character objCharacter, XmlNode objXmlQuality)
        {
            return IsValid(objCharacter, objXmlQuality, out QualityFailureReason q, out List<Quality> q2);
        }

        /// <summary>
        /// Retuns weither a quality is valid on said Character
        /// THIS IS A WIP AND ONLY CHECKS QUALITIES. REQUIRED POWERS, METATYPES AND OTHERS WON'T BE CHECKED
        /// ConflictingQualities will only contain existing Qualities and won't contain required but missing Qualities
        /// </summary>
        /// <param name="objCharacter">The Character</param>
        /// <param name="objXmlQuality">The XmlNode describing the quality</param>
        /// <param name="reason">The reason the quality is not valid</param>
        /// <param name="conflictingQualities">List of Qualities that conflicts with this Quality</param>
        /// <returns>Is the Quality valid on said Character</returns>
        public static bool IsValid(Character objCharacter, XmlNode objXmlQuality, out QualityFailureReason reason, out List<Quality> conflictingQualities)
        {
            conflictingQualities = new List<Quality>();
            reason = QualityFailureReason.Allowed;
            //If limit are not present or no, check if same quality exists
            string strTemp = string.Empty;
            if (!(objXmlQuality.TryGetStringFieldQuickly("limit", ref strTemp) && strTemp == bool.FalseString))
            {
                foreach (Quality objQuality in objCharacter.Qualities)
                {
                    if (objQuality.QualityId == objXmlQuality["id"]?.InnerText)
                    {
                        reason |= QualityFailureReason.LimitExceeded; //QualityFailureReason is a flag enum, meaning each bit represents a different thing
                        //So instead of changing it, |= adds rhs to list of reasons on lhs, if it is not present
                        conflictingQualities.Add(objQuality);
                    }
                }
            }

            XmlNode xmlRequiredNode = objXmlQuality["required"];
            if (xmlRequiredNode != null)
            {
                XmlNode xmlOneOfNode = xmlRequiredNode["oneof"];
                if (xmlOneOfNode != null)
                {
                    //Add to set for O(N log M) runtime instead of O(N * M)
                    HashSet<String> lstRequired = new HashSet<String>();
                    foreach (XmlNode node in xmlOneOfNode.SelectNodes("quality"))
                    {
                        lstRequired.Add(node.InnerText);
                    }

                    if (!objCharacter.Qualities.Any(quality => lstRequired.Contains(quality.Name)))
                    {
                        reason |= QualityFailureReason.RequiredSingle;
                    }

                    reason |= QualityFailureReason.MetatypeRequired;
                    foreach (XmlNode objNode in xmlOneOfNode.SelectNodes("metatype"))
                    {
                        if (objNode.InnerText == objCharacter.Metatype)
                        {
                            reason &= ~QualityFailureReason.MetatypeRequired;
                            break;
                        }
                    }
                }
                XmlNode xmlAllOfNode = xmlRequiredNode["allof"];
                if (xmlAllOfNode != null)
                {
                    //Add to set for O(N log M) runtime instead of O(N * M)
                    HashSet<String> lstRequired = new HashSet<String>();
                    foreach (Quality objQuality in objCharacter.Qualities)
                    {
                        lstRequired.Add(objQuality.Name);
                    }
                    foreach (XmlNode node in xmlAllOfNode.SelectNodes("quality"))
                    {
                        if (!lstRequired.Contains(node.InnerText))
                        {
                            reason |= QualityFailureReason.RequiredMultiple;
                            break;
                        }
                    }
                }
            }

            XmlNode xmlForbiddenNode = objXmlQuality["forbidden"];
            if (xmlForbiddenNode != null)
            {
                XmlNode xmlOneOfNode = xmlForbiddenNode["oneof"];
                if (xmlOneOfNode != null)
                {
                    //Add to set for O(N log M) runtime instead of O(N * M)
                    HashSet<String> qualityForbidden = new HashSet<String>();
                    foreach (XmlNode node in xmlOneOfNode.SelectNodes("quality"))
                    {
                        qualityForbidden.Add(node.InnerText);
                    }

                    foreach (Quality quality in objCharacter.Qualities)
                    {
                        if (qualityForbidden.Contains(quality.Name))
                        {
                            reason |= QualityFailureReason.ForbiddenSingle;
                            conflictingQualities.Add(quality);
                        }
                    }
                }
            }

            return conflictingQualities.Count <= 0 & reason == QualityFailureReason.Allowed;
        }

        /// <summary>
        /// This method builds a xmlNode upwards adding/overriding elements
        /// </summary>
        /// <param name="id">ID of the node</param>
        /// <returns>A XmlNode containing the id and all nodes of its parrents</returns>
        public static XmlNode GetNodeOverrideable(string id, XmlDocument xmlDoc)
        {
            return GetNodeOverrideable(xmlDoc.SelectSingleNode("//*[id = \"" + id + "\"]"));
        }

        private static XmlNode GetNodeOverrideable(XmlNode n)
        {
            XmlNode workNode = n.Clone();  //clone as to not mess up the acctual xml document
            if (workNode != null)
            {
                XmlNode parentNode = n.SelectSingleNode("../..");
                if (parentNode?["id"] != null)
                {
                    XmlNode sourceNode = GetNodeOverrideable(parentNode);
                    if (sourceNode != null)
                    {
                        foreach (XmlNode node in sourceNode.ChildNodes)
                        {
                            if (workNode[node.LocalName] == null && node.LocalName != "versions")
                            {
                                workNode.AppendChild(node.Clone());
                            }
                            else if (node.LocalName == "bonus")
                            {
                                XmlNode xmlBonusNode = workNode["bonus"];
                                if (xmlBonusNode != null)
                                {
                                    foreach (XmlNode childNode in node.ChildNodes)
                                    {
                                        xmlBonusNode.AppendChild(childNode.Clone());
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return workNode;
        }
        #endregion
    }

    /// <summary>
    /// Type of Spirit.
    /// </summary>
    public enum SpiritType
    {
        Spirit = 0,
        Sprite = 1,
    }

    /// <summary>
    /// A Magician's Spirit or Technomancer's Sprite.
    /// </summary>
    public class Spirit : IHasInternalId, IHasName, IHasXmlNode, IHasMugshots, INotifyPropertyChanged
    {
        private Guid _guiId;
        private string _strName = string.Empty;
        private string _strCritterName = string.Empty;
        private int _intServicesOwed;
        private SpiritType _eEntityType = SpiritType.Spirit;
        private bool _blnBound = true;
        private int _intForce = 1;
        private string _strFileName = string.Empty;
        private string _strRelativeName = string.Empty;
        private string _strNotes = string.Empty;
        private readonly Character _objCharacter;
        private Character _objLinkedCharacter;

        private readonly List<Image> _lstMugshots = new List<Image>();
        private int _intMainMugshotIndex = -1;

        #region Helper Methods
        /// <summary>
        /// Convert a string to a SpiritType.
        /// </summary>
        /// <param name="strValue">String value to convert.</param>
        public static SpiritType ConvertToSpiritType(string strValue)
        {
            if (string.IsNullOrEmpty(strValue))
                return default(SpiritType);
            switch (strValue)
            {
                case "Spirit":
                    return SpiritType.Spirit;
                default:
                    return SpiritType.Sprite;
            }
        }
        #endregion

        #region Constructor, Save, Load, and Print Methods
        public Spirit(Character objCharacter)
        {
            // Create the GUID for the new Spirit.
            _objCharacter = objCharacter;
        }

        /// <summary>
        /// Save the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        public void Save(XmlTextWriter objWriter)
        {
            objWriter.WriteStartElement("spirit");
            objWriter.WriteElementString("guid", _guiId.ToString("D"));
            objWriter.WriteElementString("name", _strName);
            objWriter.WriteElementString("crittername", _strCritterName);
            objWriter.WriteElementString("services", _intServicesOwed.ToString(CultureInfo.InvariantCulture));
            objWriter.WriteElementString("force", _intForce.ToString(CultureInfo.InvariantCulture));
            objWriter.WriteElementString("bound", _blnBound.ToString());
            objWriter.WriteElementString("fettered", _blnFettered.ToString());
            objWriter.WriteElementString("type", _eEntityType.ToString());
            objWriter.WriteElementString("file", _strFileName);
            objWriter.WriteElementString("relative", _strRelativeName);
            objWriter.WriteElementString("notes", _strNotes);
            SaveMugshots(objWriter);
            objWriter.WriteEndElement();

            /* Disabled for now because we cannot change any properties in the linked character anyway
            if (LinkedCharacter?.IsSaving == false && !Program.MainForm.OpenCharacterForms.Any(x => x.CharacterObject == LinkedCharacter))
                LinkedCharacter.Save();
                */
        }

        /// <summary>
        /// Load the Spirit from the XmlNode.
        /// </summary>
        /// <param name="objNode">XmlNode to load.</param>
        public void Load(XmlNode objNode)
        {
            if (objNode == null)
                return;
            objNode.TryGetField("guid", Guid.TryParse, out _guiId);
            if (objNode.TryGetStringFieldQuickly("name", ref _strName))
                _objCachedMyXmlNode = null;
            objNode.TryGetStringFieldQuickly("crittername", ref _strCritterName);
            objNode.TryGetInt32FieldQuickly("services", ref _intServicesOwed);
            objNode.TryGetInt32FieldQuickly("force", ref _intForce);
            objNode.TryGetBoolFieldQuickly("bound", ref _blnBound);
            objNode.TryGetBoolFieldQuickly("fettered", ref _blnFettered);
            _eEntityType = ConvertToSpiritType(objNode["type"]?.InnerText);
            objNode.TryGetStringFieldQuickly("file", ref _strFileName);
            objNode.TryGetStringFieldQuickly("relative", ref _strRelativeName);
            objNode.TryGetStringFieldQuickly("notes", ref _strNotes);

            RefreshLinkedCharacter(false);

            LoadMugshots(objNode);
        }

        /// <summary>
        /// Print the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        public void Print(XmlTextWriter objWriter, CultureInfo objCulture, string strLanguageToPrint)
        {
            // Translate the Critter name if applicable.
            string strName = Name;
            XmlNode objXmlCritterNode = GetNode(strLanguageToPrint);
            if (strLanguageToPrint != GlobalOptions.DefaultLanguage)
            {
                strName = objXmlCritterNode?["translate"]?.InnerText;
            }

            objWriter.WriteStartElement("spirit");
            objWriter.WriteElementString("name", strName);
            objWriter.WriteElementString("name_english", Name);
            objWriter.WriteElementString("crittername", CritterName);
            objWriter.WriteElementString("fettered", Fettered.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("bound", Bound.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("services", ServicesOwed.ToString(objCulture));
            objWriter.WriteElementString("force", Force.ToString(objCulture));

            if (objXmlCritterNode != null)
            {
                //Attributes for spirits, named differently as to not confuse <attribtue>

                Dictionary<String, int> attributes = new Dictionary<string, int>();
                objWriter.WriteStartElement("spiritattributes");
                foreach (string attribute in new String[] { "bod", "agi", "rea", "str", "cha", "int", "wil", "log", "ini" })
                {
                    String strInner = string.Empty;
                    if (objXmlCritterNode.TryGetStringFieldQuickly(attribute, ref strInner))
                    {
                        int value = 1;
                        try
                        {
                            value = Convert.ToInt32(CommonFunctions.EvaluateInvariantXPath(strInner.Replace("F", _intForce.ToString())));
                        }
                        catch (XPathException)
                        {
                            if (!int.TryParse(strInner, out value))
                            {
                                value = _intForce; //if failed to parse, default to force
                            }
                        }
                        value = Math.Max(value, 1); //Min value is 1
                        objWriter.WriteElementString(attribute, value.ToString(objCulture));

                        attributes[attribute] = value;
                    }
                }

                objWriter.WriteEndElement();

                //Dump skills, (optional)powers if present to output

                XmlDocument objXmlPowersDocument = XmlManager.Load("spiritpowers.xml", strLanguageToPrint);
                XmlNode xmlPowersNode = objXmlCritterNode["powers"];
                if (xmlPowersNode != null)
                {
                    //objWriter.WriteRaw(objXmlCritterNode["powers"].OuterXml);
                    objWriter.WriteStartElement("powers");
                    foreach (XmlNode objXmlPowerNode in xmlPowersNode.ChildNodes)
                    {
                        PrintPowerInfo(objWriter, objXmlPowersDocument, objXmlPowerNode.InnerText);
                    }
                    objWriter.WriteEndElement();
                }
                xmlPowersNode = objXmlCritterNode["optionalpowers"];
                if (xmlPowersNode != null)
                {
                    //objWriter.WriteRaw(objXmlCritterNode["optionalpowers"].OuterXml);
                    objWriter.WriteStartElement("optionalpowers");
                    foreach (XmlNode objXmlPowerNode in xmlPowersNode.ChildNodes)
                    {
                        PrintPowerInfo(objWriter, objXmlPowersDocument, objXmlPowerNode.InnerText);
                    }
                    objWriter.WriteEndElement();
                }

                if (objXmlCritterNode["skills"] != null)
                {
                    //objWriter.WriteRaw(objXmlCritterNode["skills"].OuterXml);
                    objWriter.WriteStartElement("skills");
                    foreach (XmlNode objXmlSkillNode in objXmlCritterNode["skills"].ChildNodes)
                    {
                        string attrName = objXmlSkillNode.Attributes?["attr"]?.Value;
                        if (!attributes.TryGetValue(attrName, out int attr))
                            attr = _intForce;
                        int dicepool = attr + _intForce;

                        objWriter.WriteStartElement("skill");
                        objWriter.WriteElementString("name", objXmlSkillNode.InnerText);
                        objWriter.WriteElementString("attr", attrName);
                        objWriter.WriteElementString("pool", dicepool.ToString(objCulture));
                        objWriter.WriteEndElement();
                    }
                    objWriter.WriteEndElement();
                }

                if (objXmlCritterNode["weaknesses"] != null)
                {
                    objWriter.WriteRaw(objXmlCritterNode["weaknesses"].OuterXml);
                }

                //Page in book for reference
                string source = string.Empty;
                string page = string.Empty;

                if (objXmlCritterNode.TryGetStringFieldQuickly("source", ref source))
                    objWriter.WriteElementString("source", source);
                if (objXmlCritterNode.TryGetStringFieldQuickly("page", ref page))
                    objWriter.WriteElementString("page", page);
            }

            objWriter.WriteElementString("bound", _blnBound.ToString());
            objWriter.WriteElementString("type", _eEntityType.ToString());

            if (_objCharacter.Options.PrintNotes)
                objWriter.WriteElementString("notes", Notes);
            PrintMugshots(objWriter);
            objWriter.WriteEndElement();
        }

        private static void PrintPowerInfo(XmlTextWriter objWriter, XmlDocument objXmlDocument, string strPowerName)
        {
            string strSource = string.Empty;
            string strPage = string.Empty;
            XmlNode objXmlPowerNode = objXmlDocument.SelectSingleNode("/chummer/powers/power[name = \"" + strPowerName + "\"]") ??
                objXmlDocument.SelectSingleNode("/chummer/powers/power[starts-with(\"" + strPowerName + "\", name)]");
            if (objXmlPowerNode != null)
            {
                objXmlPowerNode.TryGetStringFieldQuickly("source", ref strSource);
                objXmlPowerNode.TryGetStringFieldQuickly("page", ref strPage);
                objXmlPowerNode.TryGetStringFieldQuickly("translate", ref strPowerName);
            }

            objWriter.WriteStartElement("power");
            objWriter.WriteElementString("name", strPowerName);
            objWriter.WriteElementString("source", strSource);
            objWriter.WriteElementString("page", strPage);
            objWriter.WriteEndElement();
        }
        #endregion

        #region Properties
        /// <summary>
        /// The Character object being used by the Spirit.
        /// </summary>
        public Character CharacterObject => _objCharacter;

        /// <summary>
        /// Name of the Spirit's Metatype.
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
        /// Name of the Spirit.
        /// </summary>
        public string CritterName
        {
            get
            {
                if (LinkedCharacter != null)
                    return LinkedCharacter.CharacterName;
                return _strCritterName;
            }
            set => _strCritterName = value;
        }

        /// <summary>
        /// Number of Services the Spirit owes.
        /// </summary>
        public int ServicesOwed
        {
            get => _intServicesOwed;
            set
            {
                if (_intServicesOwed != value)
                {
                    _intServicesOwed = value;
                    if (!CharacterObject.Created)
                    {
                        // Retrieve the character's Summoning Skill Rating.
                        int intSkillValue = CharacterObject.SkillsSection.GetActiveSkill(EntityType == SpiritType.Spirit ? "Summoning" : "Compiling")?.Rating ?? 0;

                        if (_intServicesOwed > intSkillValue && !CharacterObject.IgnoreRules)
                        {
                            MessageBox.Show(LanguageManager.GetString(EntityType == SpiritType.Spirit ? "Message_SpiritServices" : "Message_SpriteServices", GlobalOptions.Language),
                                LanguageManager.GetString(EntityType == SpiritType.Spirit ? "MessageTitle_SpiritServices" : "MessageTitle_SpriteServices", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                            _intServicesOwed = intSkillValue;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// The Spirit's Force.
        /// </summary>
        public int Force
        {
            get => _intForce;
            set => _intForce = value;
        }

        /// <summary>
        /// Whether or not the Spirit is Bound.
        /// </summary>
        public bool Bound
        {
            get => _blnBound;
            set => _blnBound = value;
        }

        /// <summary>
        /// The Spirit's type, either Spirit or Sprite.
        /// </summary>
        public SpiritType EntityType
        {
            get => _eEntityType;
            set
            {
                if (_eEntityType != value)
                {
                    _objCachedMyXmlNode = null;
                    _eEntityType = value;
                }
            }
        }

        /// <summary>
        /// Name of the save file for this Spirit/Sprite.
        /// </summary>
        public string FileName
        {
            get => _strFileName;
            set
            {
                if (_strFileName != value)
                {
                    _strFileName = value;
                    RefreshLinkedCharacter(!string.IsNullOrEmpty(value));
                }
            }
        }

        /// <summary>
        /// Relative path to the save file.
        /// </summary>
        public string RelativeFileName
        {
            get => _strRelativeName;
            set
            {
                if (_strRelativeName != value)
                {
                    _strRelativeName = value;
                    RefreshLinkedCharacter(!string.IsNullOrEmpty(value));
                }
            }
        }

        /// <summary>
        /// Notes.
        /// </summary>
        public string Notes
        {
            get => _strNotes;
            set => _strNotes = value;
        }

        private bool _blnFettered = false;
        public bool Fettered
        {
            get => _blnFettered;
            set
            {
                if (_blnFettered != value)
                {
                    if (value)
                    {
                        //Only one Fettered spirit is permitted. 
                        if (CharacterObject.Spirits.Any(objSpirit => objSpirit.Fettered))
                        {
                            return;
                        }
                        if (CharacterObject.Created)
                        {
                            int FetteringCost = Force * 3;
                            if (!CharacterObject.ConfirmKarmaExpense(LanguageManager.GetString("Message_ConfirmKarmaExpenseSpend", GlobalOptions.Language)
                                        .Replace("{0}", Name)
                                        .Replace("{1}", FetteringCost.ToString())))
                            {
                                return;
                            }

                            // Create the Expense Log Entry.
                            ExpenseLogEntry objExpense = new ExpenseLogEntry(CharacterObject);
                            objExpense.Create(FetteringCost * -1,
                                LanguageManager.GetString("String_ExpenseFetteredSpirit", GlobalOptions.Language) + ' ' + Name,
                                ExpenseType.Karma, DateTime.Now);
                            CharacterObject.ExpenseEntries.Add(objExpense);
                            CharacterObject.Karma -= FetteringCost;

                            ExpenseUndo objUndo = new ExpenseUndo();
                            objUndo.CreateKarma(KarmaExpenseType.SpiritFettering, InternalId);
                            objExpense.Undo = objUndo;
                        }
                        ImprovementManager.CreateImprovement(CharacterObject, EntityType == SpiritType.Spirit ? "MAG" : "RES", Improvement.ImprovementSource.SpiritFettering, string.Empty, Improvement.ImprovementType.Attribute, string.Empty, 0, 1, 0, 0, -1);
                        ImprovementManager.Commit(CharacterObject);
                    }
                    else
                    {
                        ImprovementManager.RemoveImprovements(CharacterObject, Improvement.ImprovementSource.SpiritFettering);
                    }
                    _blnFettered = value;
                }
            }
        }

        public string InternalId
        {
            get => _guiId.ToString("D");
            set => _guiId = Guid.Parse(value);
        }

        public event PropertyChangedEventHandler PropertyChanged;

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
                _objCachedMyXmlNode = XmlManager.Load(_eEntityType == SpiritType.Spirit ? "traditions.xml" : "streams.xml", strLanguage).SelectSingleNode("/chummer/spirits/spirit[name = \"" + Name + "\"]");
                _strCachedXmlNodeLanguage = strLanguage;
            }
            return _objCachedMyXmlNode;
        }

        public Character LinkedCharacter
        {
            get => _objLinkedCharacter;
        }

        public bool NoLinkedCharacter
        {
            get => _objLinkedCharacter == null;
        }

        public void RefreshLinkedCharacter(bool blnShowError)
        {
            Character _objOldLinkedCharacter = _objLinkedCharacter;
            _objCharacter.LinkedCharacters.Remove(_objLinkedCharacter);
            _objLinkedCharacter = null;
            bool blnError = false;
            bool blnUseRelative = false;

            // Make sure the file still exists before attempting to load it.
            if (!File.Exists(FileName))
            {
                // If the file doesn't exist, use the relative path if one is available.
                if (string.IsNullOrEmpty(RelativeFileName))
                    blnError = true;
                else if (!File.Exists(Path.GetFullPath(RelativeFileName)))
                    blnError = true;
                else
                    blnUseRelative = true;

                if (blnError && blnShowError)
                {
                    MessageBox.Show(LanguageManager.GetString("Message_FileNotFound", GlobalOptions.Language).Replace("{0}", FileName), LanguageManager.GetString("MessageTitle_FileNotFound", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            if (!blnError)
            {
                string strFile = blnUseRelative ? Path.GetFullPath(RelativeFileName) : FileName;
                if (strFile.EndsWith(".chum5"))
                {
                    Character objOpenCharacter = Program.MainForm.OpenCharacters.FirstOrDefault(x => x.FileName == strFile);
                    if (objOpenCharacter != null)
                        _objLinkedCharacter = objOpenCharacter;
                    else
                        _objLinkedCharacter = Program.MainForm.LoadCharacter(strFile, string.Empty, false, false);
                    if (_objLinkedCharacter != null)
                        _objCharacter.LinkedCharacters.Add(_objLinkedCharacter);
                }
            }
            if (_objLinkedCharacter != _objOldLinkedCharacter)
            {
                if (_objOldLinkedCharacter != null)
                {
                    if (!Program.MainForm.OpenCharacters.Any(x => x.LinkedCharacters.Contains(_objOldLinkedCharacter) && x != _objOldLinkedCharacter))
                    {
                        Program.MainForm.OpenCharacters.Remove(_objOldLinkedCharacter);
                        _objOldLinkedCharacter.Dispose();
                    }
                }
                if (_objLinkedCharacter != null)
                {
                    if (string.IsNullOrEmpty(_strCritterName) && CritterName != LanguageManager.GetString("String_UnnamedCharacter", GlobalOptions.Language))
                        _strCritterName = CritterName;
                }
                PropertyChangedEventHandler objPropertyChanged = PropertyChanged;
                if (objPropertyChanged != null)
                {
                    objPropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(CritterName)));
                    objPropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(NoLinkedCharacter)));
                }
            }
        }
        #endregion

        #region IHasMugshots
        /// <summary>
		/// Character's portraits encoded using Base64.
		/// </summary>
		public IList<Image> Mugshots
        {
            get
            {
                if (LinkedCharacter != null)
                    return LinkedCharacter.Mugshots;
                else
                    return _lstMugshots;
            }
        }

        /// <summary>
        /// Character's main portrait encoded using Base64.
        /// </summary>
        public Image MainMugshot
        {
            get
            {
                if (LinkedCharacter != null)
                    return LinkedCharacter.MainMugshot;
                else if (MainMugshotIndex >= Mugshots.Count || MainMugshotIndex < 0)
                    return null;
                else
                    return Mugshots[MainMugshotIndex];
            }
            set
            {
                if (LinkedCharacter != null)
                    LinkedCharacter.MainMugshot = value;
                else
                {
                    if (value == null)
                    {
                        MainMugshotIndex = -1;
                        return;
                    }
                    int intNewMainMugshotIndex = Mugshots.IndexOf(value);
                    if (intNewMainMugshotIndex != -1)
                    {
                        MainMugshotIndex = intNewMainMugshotIndex;
                    }
                    else
                    {
                        Mugshots.Add(value);
                        MainMugshotIndex = Mugshots.Count - 1;
                    }
                }
            }
        }

        /// <summary>
        /// Index of Character's main portrait. -1 if set to none.
        /// </summary>
        public int MainMugshotIndex
        {
            get
            {
                if (LinkedCharacter != null)
                    return LinkedCharacter.MainMugshotIndex;
                else
                    return _intMainMugshotIndex;
            }
            set
            {
                if (LinkedCharacter != null)
                    LinkedCharacter.MainMugshotIndex = value;
                else if (value >= _lstMugshots.Count || value < -1)
                    _intMainMugshotIndex = -1;
                else
                    _intMainMugshotIndex = value;
            }
        }

        public void SaveMugshots(XmlTextWriter objWriter)
        {
            objWriter.WriteElementString("mainmugshotindex", MainMugshotIndex.ToString());
            // <mugshot>
            objWriter.WriteStartElement("mugshots");
            foreach (Image imgMugshot in Mugshots)
            {
                objWriter.WriteElementString("mugshot", imgMugshot.ToBase64String());
            }
            // </mugshot>
            objWriter.WriteEndElement();
        }

        public void LoadMugshots(XmlNode xmlSavedNode)
        {
            xmlSavedNode.TryGetInt32FieldQuickly("mainmugshotindex", ref _intMainMugshotIndex);
            XmlNodeList objXmlMugshotsList = xmlSavedNode.SelectNodes("mugshots/mugshot");
            if (objXmlMugshotsList != null)
            {
                List<string> lstMugshotsBase64 = new List<string>(objXmlMugshotsList.Count);
                foreach (XmlNode objXmlMugshot in objXmlMugshotsList)
                {
                    string strMugshot = objXmlMugshot.InnerText;
                    if (!string.IsNullOrWhiteSpace(strMugshot))
                    {
                        lstMugshotsBase64.Add(strMugshot);
                    }
                }
                if (lstMugshotsBase64.Count > 1)
                {
                    Image[] objMugshotImages = new Image[lstMugshotsBase64.Count];
                    Parallel.For(0, lstMugshotsBase64.Count, i =>
                    {
                        objMugshotImages[i] = lstMugshotsBase64[i].ToImage(System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
                    });
                    _lstMugshots.AddRange(objMugshotImages);
                }
                else if (lstMugshotsBase64.Count == 1)
                {
                    _lstMugshots.Add(lstMugshotsBase64[0].ToImage(System.Drawing.Imaging.PixelFormat.Format32bppPArgb));
                }
            }
        }

        public void PrintMugshots(XmlTextWriter objWriter)
        {
            if (LinkedCharacter != null)
                LinkedCharacter.PrintMugshots(objWriter);
            else if (Mugshots.Count > 0)
            {
                // Since IE is retarded and can't handle base64 images before IE9, we need to dump the image to a temporary directory and re-write the information.
                // If you give it an extension of jpg, gif, or png, it expects the file to be in that format and won't render the image unless it was originally that type.
                // But if you give it the extension img, it will render whatever you give it (which doesn't make any damn sense, but that's IE for you).
                string strMugshotsDirectoryPath = Path.Combine(Application.StartupPath, "mugshots");
                if (!Directory.Exists(strMugshotsDirectoryPath))
                {
                    try
                    {
                        Directory.CreateDirectory(strMugshotsDirectoryPath);
                    }
                    catch (UnauthorizedAccessException)
                    {
                        MessageBox.Show(LanguageManager.GetString("Message_Insufficient_Permissions_Warning", GlobalOptions.Language));
                    }
                }
                Guid guiImage = Guid.NewGuid();
                string imgMugshotPath = Path.Combine(strMugshotsDirectoryPath, guiImage.ToString("N") + ".img");
                Image imgMainMugshot = MainMugshot;
                if (imgMainMugshot != null)
                {
                    imgMainMugshot.Save(imgMugshotPath);
                    // <mainmugshotpath />
                    objWriter.WriteElementString("mainmugshotpath", "file://" + imgMugshotPath.Replace(Path.DirectorySeparatorChar, '/'));
                    // <mainmugshotbase64 />
                    objWriter.WriteElementString("mainmugshotbase64", imgMainMugshot.ToBase64String());
                }
                // <othermugshots>
                objWriter.WriteElementString("hasothermugshots", (imgMainMugshot == null || Mugshots.Count > 1).ToString());
                objWriter.WriteStartElement("othermugshots");
                for (int i = 0; i < Mugshots.Count; ++i)
                {
                    if (i == MainMugshotIndex)
                        continue;
                    Image imgMugshot = Mugshots[i];
                    objWriter.WriteStartElement("mugshot");

                    objWriter.WriteElementString("stringbase64", imgMugshot.ToBase64String());

                    imgMugshotPath = Path.Combine(strMugshotsDirectoryPath, guiImage.ToString("N") + i.ToString() + ".img");
                    imgMugshot.Save(imgMugshotPath);
                    objWriter.WriteElementString("temppath", "file://" + imgMugshotPath.Replace(Path.DirectorySeparatorChar, '/'));

                    objWriter.WriteEndElement();
                }
                // </mugshots>
                objWriter.WriteEndElement();
            }
        }
        #endregion
    }

    /// <summary>
    /// A Magician Spell.
    /// </summary>
    public class Spell : IHasInternalId, IHasName, IHasXmlNode
    {
        private Guid _guiID;
        private string _strName = string.Empty;
        private string _strDescriptors = string.Empty;
        private string _strCategory = string.Empty;
        private string _strType = string.Empty;
        private string _strRange = string.Empty;
        private string _strDamage = string.Empty;
        private string _strDuration = string.Empty;
        private string _strDV = string.Empty;
        private string _strSource = string.Empty;
        private string _strPage = string.Empty;
        private string _strExtra = string.Empty;
        private bool _blnLimited;
        private bool _blnExtended;
        private string _strNotes = string.Empty;
        private readonly Character _objCharacter;
        private bool _blnAlchemical;
        private bool _blnFreeBonus;
        private bool _blnUsesUnarmed = false;
        private int _intGrade;
        private Improvement.ImprovementSource _objImprovementSource = Improvement.ImprovementSource.Spell;

        #region Constructor, Create, Save, Load, and Print Methods
        public Spell(Character objCharacter)
        {
            // Create the GUID for the new Spell.
            _guiID = Guid.NewGuid();
            _objCharacter = objCharacter;
        }

        /// Create a Spell from an XmlNode.
        /// <param name="objXmlSpellNode">XmlNode to create the object from.</param>
        /// <param name="strForcedValue">Value to forcefully select for any ImprovementManager prompts.</param>
        /// <param name="blnLimited">Whether or not the Spell should be marked as Limited.</param>
        /// <param name="blnExtended">Whether or not the Spell should be marked as Extended.</param>
        public void Create(XmlNode objXmlSpellNode, string strForcedValue = "", bool blnLimited = false, bool blnExtended = false, bool blnAlchemical = false, Improvement.ImprovementSource objSource = Improvement.ImprovementSource.Spell)
        {
            if (objXmlSpellNode.TryGetStringFieldQuickly("name", ref _strName))
                _objCachedMyXmlNode = null;
            _blnExtended = blnExtended;

            ImprovementManager.ForcedValue = strForcedValue;
            if (objXmlSpellNode["bonus"] != null)
            {
                if (!ImprovementManager.CreateImprovements(_objCharacter, Improvement.ImprovementSource.Spell, _guiID.ToString("D"), objXmlSpellNode["bonus"], false, 1, DisplayNameShort(GlobalOptions.Language)))
                {
                    _guiID = Guid.Empty;
                    return;
                }
                if (!string.IsNullOrEmpty(ImprovementManager.SelectedValue))
                {
                    _strExtra = ImprovementManager.SelectedValue;
                }
            }
            if (!objXmlSpellNode.TryGetStringFieldQuickly("altnotes", ref _strNotes))
                objXmlSpellNode.TryGetStringFieldQuickly("notes", ref _strNotes);
            objXmlSpellNode.TryGetStringFieldQuickly("descriptor", ref _strDescriptors);
            objXmlSpellNode.TryGetStringFieldQuickly("category", ref _strCategory);
            objXmlSpellNode.TryGetStringFieldQuickly("type", ref _strType);
            objXmlSpellNode.TryGetStringFieldQuickly("range", ref _strRange);
            objXmlSpellNode.TryGetStringFieldQuickly("damage", ref _strDamage);
            objXmlSpellNode.TryGetStringFieldQuickly("duration", ref _strDuration);
            objXmlSpellNode.TryGetStringFieldQuickly("dv", ref _strDV);
            _blnLimited = blnLimited;
            _blnAlchemical = blnAlchemical;
            objXmlSpellNode.TryGetStringFieldQuickly("source", ref _strSource);
            objXmlSpellNode.TryGetStringFieldQuickly("page", ref _strPage);
            _objImprovementSource = objSource;

            if (_blnLimited && _strDV.StartsWith('F'))
            {
                string strDV = _strDV;
                int intPos = 0;
                if (strDV.Contains('-'))
                {
                    intPos = strDV.IndexOf('-') + 1;
                    string strAfter = strDV.Substring(intPos, strDV.Length - intPos);
                    strDV = strDV.Substring(0, intPos);
                    int.TryParse(strAfter, out int intAfter);
                    intAfter += 2;
                    strDV += intAfter.ToString();
                }
                else if (strDV.Contains('+'))
                {
                    intPos = strDV.IndexOf('+');
                    string strAfter = strDV.Substring(intPos, strDV.Length - intPos);
                    strDV = strDV.Substring(0, intPos);
                    int.TryParse(strAfter, out int intAfter);
                    intAfter -= 2;
                    if (intAfter > 0)
                        strDV += '+' + intAfter.ToString();
                    else if (intAfter < 0)
                        strDV += intAfter.ToString();
                }
                else
                {
                    strDV += "-2";
                }
                _strDV = strDV;
            }
            /*
            if (_strNotes == string.Empty)
            {
                _strNotes = CommonFunctions.GetText($"{_strSource} {_strPage}", Name);
            }*/
        }

        /// <summary>
        /// Save the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        public void Save(XmlTextWriter objWriter)
        {
            objWriter.WriteStartElement("spell");
            objWriter.WriteElementString("guid", _guiID.ToString("D"));
            objWriter.WriteElementString("name", _strName);
            objWriter.WriteElementString("descriptors", _strDescriptors);
            objWriter.WriteElementString("category", _strCategory);
            objWriter.WriteElementString("type", _strType);
            objWriter.WriteElementString("range", _strRange);
            objWriter.WriteElementString("damage", _strDamage);
            objWriter.WriteElementString("duration", _strDuration);
            objWriter.WriteElementString("dv", _strDV);
            objWriter.WriteElementString("limited", _blnLimited.ToString());
            objWriter.WriteElementString("extended", _blnExtended.ToString());
            objWriter.WriteElementString("alchemical", _blnAlchemical.ToString());
            objWriter.WriteElementString("source", _strSource);
            objWriter.WriteElementString("page", _strPage);
            objWriter.WriteElementString("extra", _strExtra);
            objWriter.WriteElementString("notes", _strNotes);
            objWriter.WriteElementString("freebonus", _blnFreeBonus.ToString());
            objWriter.WriteElementString("usesunarmed", _blnUsesUnarmed.ToString());
            objWriter.WriteElementString("improvementsource", _objImprovementSource.ToString());
            objWriter.WriteElementString("grade", _intGrade.ToString(CultureInfo.InvariantCulture));
            objWriter.WriteEndElement();
            _objCharacter.SourceProcess(_strSource);
        }

        /// <summary>
        /// Load the Spell from the XmlNode.
        /// </summary>
        /// <param name="objNode">XmlNode to load.</param>
        public void Load(XmlNode objNode)
        {
            objNode.TryGetField("guid", Guid.TryParse, out _guiID);
            if (objNode.TryGetStringFieldQuickly("name", ref _strName))
                _objCachedMyXmlNode = null;
            objNode.TryGetStringFieldQuickly("descriptors", ref _strDescriptors);
            objNode.TryGetStringFieldQuickly("category", ref _strCategory);
            objNode.TryGetStringFieldQuickly("type", ref _strType);
            objNode.TryGetStringFieldQuickly("range", ref _strRange);
            objNode.TryGetStringFieldQuickly("damage", ref _strDamage);
            objNode.TryGetStringFieldQuickly("duration", ref _strDuration);
            if (objNode["improvementsource"] != null)
            {
                _objImprovementSource = Improvement.ConvertToImprovementSource(objNode["improvementsource"].InnerText);
            }
            objNode.TryGetInt32FieldQuickly("grade", ref _intGrade);
            objNode.TryGetStringFieldQuickly("dv", ref _strDV);
            objNode.TryGetBoolFieldQuickly("limited", ref _blnLimited);
            objNode.TryGetBoolFieldQuickly("extended", ref _blnExtended);
            objNode.TryGetBoolFieldQuickly("freebonus", ref _blnFreeBonus);
            objNode.TryGetBoolFieldQuickly("usesunarmed", ref _blnUsesUnarmed);
            objNode.TryGetBoolFieldQuickly("alchemical", ref _blnAlchemical);
            objNode.TryGetStringFieldQuickly("source", ref _strSource);
            objNode.TryGetStringFieldQuickly("page", ref _strPage);

            objNode.TryGetStringFieldQuickly("extra", ref _strExtra);
            objNode.TryGetStringFieldQuickly("notes", ref _strNotes);
        }

        /// <summary>
        /// Print the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        public void Print(XmlTextWriter objWriter, CultureInfo objCulture, string strLanguageToPrint)
        {
            objWriter.WriteStartElement("spell");
            if (Limited)
                objWriter.WriteElementString("name", DisplayNameShort(strLanguageToPrint) + " (" + LanguageManager.GetString("String_SpellLimited", strLanguageToPrint) + ')');
            else if (Alchemical)
                objWriter.WriteElementString("name", DisplayNameShort(strLanguageToPrint) + " (" + LanguageManager.GetString("String_SpellAlchemical", strLanguageToPrint) + ')');
            else
                objWriter.WriteElementString("name", DisplayNameShort(strLanguageToPrint));
            objWriter.WriteElementString("name_english", Name);
            objWriter.WriteElementString("descriptors", DisplayDescriptors(strLanguageToPrint));
            objWriter.WriteElementString("category", DisplayCategory(strLanguageToPrint));
            objWriter.WriteElementString("category_english", Category);
            objWriter.WriteElementString("type", DisplayType(strLanguageToPrint));
            objWriter.WriteElementString("range", DisplayRange(strLanguageToPrint));
            objWriter.WriteElementString("damage", DisplayDamage(strLanguageToPrint));
            objWriter.WriteElementString("duration", DisplayDuration(strLanguageToPrint));
            objWriter.WriteElementString("dv", DisplayDV(strLanguageToPrint));
            objWriter.WriteElementString("alchemy", Alchemical.ToString());
            objWriter.WriteElementString("dicepool", DicePool.ToString(objCulture));
            objWriter.WriteElementString("source", CommonFunctions.LanguageBookShort(Source, strLanguageToPrint));
            objWriter.WriteElementString("page", DisplayPage(strLanguageToPrint));
            objWriter.WriteElementString("extra", LanguageManager.TranslateExtra(Extra, strLanguageToPrint));
            if (_objCharacter.Options.PrintNotes)
                objWriter.WriteElementString("notes", Notes);
            objWriter.WriteEndElement();
        }
        #endregion

        #region Properties
        /// <summary>
        /// Internal identifier which will be used to identify this Spell in the Improvement system.
        /// </summary>
        public string InternalId => _guiID.ToString("D");

        /// <summary>
        /// Spell's name.
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
        /// Spell's grade.
        /// </summary>
        public int Grade
        {
            get => _intGrade;
            set => _intGrade = value;
        }

        /// <summary>
        /// Spell's descriptors.
        /// </summary>
        public string Descriptors
        {
            get => _strDescriptors;
            set => _strDescriptors = value;
        }

        /// <summary>
        /// Translated Descriptors.
        /// </summary>
        public string DisplayDescriptors(string strLanguage)
        {
            StringBuilder objReturn = new StringBuilder();

            string[] strDescriptorsIn = Descriptors.Split(',');
            foreach (string strDescriptor in strDescriptorsIn)
            {
                switch (strDescriptor.Trim())
                {
                    case "Active":
                        objReturn.Append(LanguageManager.GetString("String_DescActive", strLanguage));
                        break;
                    case "Adept":
                        objReturn.Append(LanguageManager.GetString("String_DescAdept", strLanguage));
                        break;
                    case "Alchemical Preparation":
                        objReturn.Append(LanguageManager.GetString("String_DescAlchemicalPreparation", strLanguage));
                        break;
                    case "Anchored":
                        objReturn.Append(LanguageManager.GetString("String_DescAnchored", strLanguage));
                        break;
                    case "Area":
                        objReturn.Append(LanguageManager.GetString("String_DescArea", strLanguage));
                        break;
                    case "Blood":
                        objReturn.Append(LanguageManager.GetString("String_DescBlood", strLanguage));
                        break;
                    case "Contractual":
                        objReturn.Append(LanguageManager.GetString("String_DescContractual", strLanguage));
                        break;
                    case "Direct":
                        objReturn.Append(LanguageManager.GetString("String_DescDirect", strLanguage));
                        break;
                    case "Directional":
                        objReturn.Append(LanguageManager.GetString("String_DescDirectional", strLanguage));
                        break;
                    case "Elemental":
                        objReturn.Append(LanguageManager.GetString("String_DescElemental", strLanguage));
                        break;
                    case "Environmental":
                        objReturn.Append(LanguageManager.GetString("String_DescEnvironmental", strLanguage));
                        break;
                    case "Geomancy":
                        objReturn.Append(LanguageManager.GetString("String_DescGeomancy", strLanguage));
                        break;
                    case "Indirect":
                        objReturn.Append(LanguageManager.GetString("String_DescIndirect", strLanguage));
                        break;
                    case "Mana":
                        objReturn.Append(LanguageManager.GetString("String_DescMana", strLanguage));
                        break;
                    case "Material Link":
                        objReturn.Append(LanguageManager.GetString("String_DescMaterialLink", strLanguage));
                        break;
                    case "Mental":
                        objReturn.Append(LanguageManager.GetString("String_DescMental", strLanguage));
                        break;
                    case "Minion":
                        objReturn.Append(LanguageManager.GetString("String_DescMinion", strLanguage));
                        break;
                    case "Multi-Sense":
                        objReturn.Append(LanguageManager.GetString("String_DescMultiSense", strLanguage));
                        break;
                    case "Negative":
                        objReturn.Append(LanguageManager.GetString("String_DescNegative", strLanguage));
                        break;
                    case "Obvious":
                        objReturn.Append(LanguageManager.GetString("String_DescObvious", strLanguage));
                        break;
                    case "Organic Link":
                        objReturn.Append(LanguageManager.GetString("String_DescOrganicLink", strLanguage));
                        break;
                    case "Passive":
                        objReturn.Append(LanguageManager.GetString("String_DescPassive", strLanguage));
                        break;
                    case "Physical":
                        objReturn.Append(LanguageManager.GetString("String_DescPhysical", strLanguage));
                        break;
                    case "Psychic":
                        objReturn.Append(LanguageManager.GetString("String_DescPsychic", strLanguage));
                        break;
                    case "Realistic":
                        objReturn.Append(LanguageManager.GetString("String_DescRealistic", strLanguage));
                        break;
                    case "Single-Sense":
                        objReturn.Append(LanguageManager.GetString("String_DescSingleSense", strLanguage));
                        break;
                    case "Touch":
                        objReturn.Append(LanguageManager.GetString("String_DescTouch", strLanguage));
                        break;
                    case "Spell":
                        objReturn.Append(LanguageManager.GetString("String_DescSpell", strLanguage));
                        break;
                    case "Spotter":
                        objReturn.Append(LanguageManager.GetString("String_DescSpotter", strLanguage));
                        break;
                }
                objReturn.Append(", ");
            }

            // If Extended Area was not found and the Extended flag is enabled, add Extended Area to the list of Descriptors.
            if (Extended)
                objReturn.Append(LanguageManager.GetString("String_DescExtendedArea", strLanguage) + ", ");

            // Remove the trailing comma.
            if (objReturn.Length >= 2)
                objReturn.Length -= 2;

            return objReturn.ToString();
        }

        /// <summary>
        /// Translated Category.
        /// </summary>
        public string DisplayCategory(string strLanguage)
        {
            if (strLanguage == GlobalOptions.DefaultLanguage)
                return Category;

            return XmlManager.Load("spells.xml", strLanguage).SelectSingleNode("/chummer/categories/category[. = \"" + Category + "\"]/@translate")?.InnerText ?? Category;
        }

        /// <summary>
        /// Spell's category.
        /// </summary>
        public string Category
        {
            get => _strCategory;
            set => _strCategory = value;
        }

        /// <summary>
        /// Spell's type.
        /// </summary>
        public string Type
        {
            get => _strType;
            set => _strType = value;
        }

        /// <summary>
        /// Translated Type.
        /// </summary>
        public string DisplayType(string strLanguage)
        {
            string strReturn = string.Empty;

            switch (Type)
            {
                case "M":
                    strReturn = LanguageManager.GetString("String_SpellTypeMana", strLanguage);
                    break;
                default:
                    strReturn = LanguageManager.GetString("String_SpellTypePhysical", strLanguage);
                    break;
            }

            return strReturn;
        }

        /// <summary>
        /// Translated Drain Value.
        /// </summary>
        public string DisplayDV(string strLanguage)
        {
            string strReturn = DV.Replace('/', '÷');
            strReturn = strReturn.CheapReplace("F", () => LanguageManager.GetString("String_SpellForce", strLanguage));
            strReturn = strReturn.CheapReplace("Overflow damage", () => LanguageManager.GetString("String_SpellOverflowDamage", strLanguage));
            strReturn = strReturn.CheapReplace("Damage Value", () => LanguageManager.GetString("String_SpellDamageValue", strLanguage));
            strReturn = strReturn.CheapReplace("Toxin DV", () => LanguageManager.GetString("String_SpellToxinDV", strLanguage));
            strReturn = strReturn.CheapReplace("Disease DV", () => LanguageManager.GetString("String_SpellDiseaseDV", strLanguage));
            strReturn = strReturn.CheapReplace("Radiation Power", () => LanguageManager.GetString("String_SpellRadiationPower", strLanguage));

            return strReturn;
        }

        /// <summary>
        /// Drain Tooltip.
        /// </summary>
        public string DVTooltip
        {
            get
            {
                string strTip = LanguageManager.GetString("Tip_SpellDrainBase", GlobalOptions.Language);
                int intMAG = _objCharacter.MAG.TotalValue;

                if (_objCharacter.AdeptEnabled && _objCharacter.MagicianEnabled)
                {
                    intMAG = _objCharacter.Options.SpiritForceBasedOnTotalMAG ? _objCharacter.MAG.TotalValue : _objCharacter.MAG.Value;
                }

                for (int i = 1; i <= intMAG * 2; i++)
                {
                    // Calculate the Spell's Drain for the current Force.
                    object xprResult = null;
                    try
                    {
                        xprResult = CommonFunctions.EvaluateInvariantXPath(DV.Replace("F", i.ToString()).Replace("/", " div "));
                    }
                    catch (XPathException)
                    {
                        xprResult = null;
                    }

                    if (xprResult != null && DV != "Special")
                    {
                        int intDV = Convert.ToInt32(Math.Floor(Convert.ToDouble(xprResult.ToString(), GlobalOptions.InvariantCultureInfo)));

                        // Drain cannot be lower than 2.
                        if (intDV < 2)
                            intDV = 2;
                        strTip += "\n" + LanguageManager.GetString("String_Force", GlobalOptions.Language) + ' ' + i.ToString() + ": " + intDV.ToString();
                    }
                    else
                    {
                        strTip = LanguageManager.GetString("Tip_SpellDrainSeeDescription", GlobalOptions.Language);
                        break;
                    }
                }
                if (_objCharacter.Improvements.Any(o => (o.ImproveType == Improvement.ImprovementType.DrainValue || o.ImproveType == Improvement.ImprovementType.SpellCategoryDrain) && (string.IsNullOrEmpty(o.ImprovedName) || o.ImprovedName == Category)))
                {
                    strTip += $"\n {LanguageManager.GetString("Label_Bonus", GlobalOptions.Language)}";
                    strTip = _objCharacter.Improvements
                        .Where(o => (o.ImproveType == Improvement.ImprovementType.DrainValue || o.ImproveType == Improvement.ImprovementType.SpellCategoryDrain) && (string.IsNullOrEmpty(o.ImprovedName) || o.ImprovedName == Category))
                        .Aggregate(strTip, (current, imp) => current + $"\n {_objCharacter.GetObjectName(imp, GlobalOptions.Language)} ({imp.Value:0;-0;0})");
                }

                return strTip;
            }
        }

        /// <summary>
        /// Spell's range.
        /// </summary>
        public string Range
        {
            get => _strRange;
            set => _strRange = value;
        }

        /// <summary>
        /// Translated Range.
        /// </summary>
        public string DisplayRange(string strLanguage)
        {
            string strReturn = Range;
            strReturn = strReturn.CheapReplace("Self", () => LanguageManager.GetString("String_SpellRangeSelf", strLanguage));
            strReturn = strReturn.CheapReplace("LOS", () => LanguageManager.GetString("String_SpellRangeLineOfSight", strLanguage));
            strReturn = strReturn.CheapReplace("LOI", () => LanguageManager.GetString("String_SpellRangeLineOfInfluence", strLanguage));
            strReturn = strReturn.CheapReplace("T", () => LanguageManager.GetString("String_SpellRangeTouch", strLanguage));
            strReturn = strReturn.CheapReplace("(A)", () => "(" + LanguageManager.GetString("String_SpellRangeArea", strLanguage) + ')');
            strReturn = strReturn.CheapReplace("MAG", () => LanguageManager.GetString("String_AttributeMAGShort", strLanguage));

            return strReturn;
        }

        /// <summary>
        /// Spell's damage.
        /// </summary>
        public string Damage
        {
            get => _strDamage;
            set => _strDamage = value;
        }

        /// <summary>
        /// Translated Damage.
        /// </summary>
        public string DisplayDamage(string strLanguage)
        {
            string strReturn = string.Empty;

            switch (Damage)
            {
                case "P":
                    strReturn = LanguageManager.GetString("String_DamagePhysical", strLanguage);
                    break;
                case "S":
                    strReturn = LanguageManager.GetString("String_DamageStun", strLanguage);
                    break;
                default:
                    strReturn = LanguageManager.GetString("String_None", strLanguage);
                    break;
            }

            return strReturn;
        }

        /// <summary>
        /// Spell's duration.
        /// </summary>
        public string Duration
        {
            get => _strDuration;
            set => _strDuration = value;
        }

        /// <summary>
        /// Translated Duration.
        /// </summary>
        public string DisplayDuration(string strLanguage)
        {
            string strReturn = string.Empty;

            switch (Duration)
            {
                case "P":
                    strReturn = LanguageManager.GetString("String_SpellDurationPermanent", strLanguage);
                    break;
                case "S":
                    strReturn = LanguageManager.GetString("String_SpellDurationSustained", strLanguage);
                    break;
                case "I":
                    strReturn = LanguageManager.GetString("String_SpellDurationInstant", strLanguage);
                    break;
                case "Special":
                    strReturn = LanguageManager.GetString("String_SpellDurationSpecial", strLanguage);
                    break;
                default:
                    strReturn = LanguageManager.GetString("String_None", strLanguage);
                    break;
            }

            return strReturn;
        }

        /// <summary>
        /// Spell's drain value.
        /// </summary>
        public string DV
        {
            get
            {
                string strReturn = _strDV;
                bool force = strReturn.StartsWith('F');
                if (_objCharacter.Improvements.Any(o => (o.ImproveType == Improvement.ImprovementType.DrainValue || o.ImproveType == Improvement.ImprovementType.SpellCategoryDrain) && (string.IsNullOrEmpty(o.ImprovedName) || o.ImprovedName == Category)) || Limited)
                {
                    string dv = strReturn.TrimStart('F');
                    //Navigator can't do math on a single value, so inject a mathable value.
                    if (string.IsNullOrEmpty(dv))
                    {
                        dv = "0";
                    }
                    else if (strReturn.Contains('-'))
                    {
                        dv = strReturn.Substring(strReturn.IndexOf('-'));
                    }
                    else if (strReturn.Contains('+'))
                    {
                        dv = strReturn.Substring(strReturn.IndexOf('+'));
                    }
                    foreach (
                        Improvement imp in
                        _objCharacter.Improvements.Where(
                            i =>
                                (i.ImproveType == Improvement.ImprovementType.DrainValue || i.ImproveType == Improvement.ImprovementType.SpellCategoryDrain) &&
                                (string.IsNullOrEmpty(i.ImprovedName) || i.ImprovedName == Category) && i.Enabled))
                    {
                        dv += $" + {imp.Value:0;-0;0}";
                    }
                    if (Limited)
                    {
                        dv += " + -2";
                    }
                    object xprResult = CommonFunctions.EvaluateInvariantXPath(dv.TrimStart('+'));
                    if (force)
                    {
                        strReturn = $"F{xprResult:+0;-0;0}";
                    }
                    else
                    {
                        strReturn += xprResult;
                    }
                }
                return strReturn;
            }
            set => _strDV = value;
        }

        /// <summary>
        /// Spell's Source.
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
            if (strLanguage == GlobalOptions.DefaultLanguage)
                return Page;

            return GetNode(strLanguage)?["altpage"]?.InnerText ?? Page;
        }

        /// <summary>
        /// Extra information from Improvement dialogues.
        /// </summary>
        public string Extra
        {
            get => _strExtra;
            set => _strExtra = LanguageManager.ReverseTranslateExtra(value, GlobalOptions.Language);
        }

        /// <summary>
        /// Whether or not the Spell is Limited.
        /// </summary>
        public bool Limited
        {
            get => _blnLimited;
            set => _blnLimited = value;
        }

        /// <summary>
        /// Whether or not the Spell is Extended.
        /// </summary>
        public bool Extended
        {
            get => _blnExtended;
            set => _blnExtended = value;
        }

        /// <summary>
        /// Whether or not the Spell is Alchemical.
        /// </summary>
        public bool Alchemical
        {
            get => _blnAlchemical;
            set => _blnAlchemical = value;
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
        /// The name of the object as it should be displayed on printouts (translated name only).
        /// </summary>
        public string DisplayNameShort(string strLanguage)
        {
            string strReturn = strLanguage != GlobalOptions.DefaultLanguage ? GetNode(strLanguage)?["translate"]?.InnerText ?? Name : Name;
            if (Extended)
                strReturn += ", " + LanguageManager.GetString("String_SpellExtended", strLanguage);

            return strReturn;
        }

        /// <summary>
        /// The name of the object as it should be displayed in lists.
        /// </summary>
        public string DisplayName(string strLanguage)
        {
            string strReturn = DisplayNameShort(strLanguage);

            if (Limited)
                strReturn += " (" + LanguageManager.GetString("String_SpellLimited", strLanguage) + ')';
            if (Alchemical)
                strReturn += " (" + LanguageManager.GetString("String_SpellAlchemical", strLanguage) + ')';
            if (!string.IsNullOrEmpty(Extra))
            {
                // Attempt to retrieve the CharacterAttribute name.
                strReturn += " (" + LanguageManager.TranslateExtra(Extra, strLanguage) + ')';
            }
            return strReturn;
        }

        /// <summary>
        /// Does the spell cost Karma? Typically provided by improvements.
        /// </summary>
        public bool FreeBonus
        {
            get => _blnFreeBonus;
            set => _blnFreeBonus = value;
        }

        /// <summary>
        /// Does the spell use Unarmed in place of Spellcasting for its casting test?
        /// </summary>
        public bool UsesUnarmed
        {
            get => _blnUsesUnarmed;
            set => _blnUsesUnarmed = value;
        }
        #endregion

        #region ComplexProperties
        /// <summary>
        /// Skill used by this spell
        /// </summary>
        public Skill Skill
        {
            get
            {
                if (Alchemical)
                {
                    return _objCharacter.SkillsSection.GetActiveSkill("Alchemy");
                }
                else if (Category == "Enchantments")
                {
                    return _objCharacter.SkillsSection.GetActiveSkill("Artificing");
                }
                else if (Category == "Rituals")
                {
                    return _objCharacter.SkillsSection.GetActiveSkill("Ritual Spellcasting");
                }
                else
                {
                    return _objCharacter.SkillsSection.GetActiveSkill(UsesUnarmed ? "Unarmed Combat" : "Spellcasting");
                }
            }
        }

        /// <summary>
        /// The Dice Pool size for the Active Skill required to cast the Spell.
        /// </summary>
        public int DicePool
        {
            get
            {
                int intReturn = 0;
                Skill objSkill = Skill;
                if (objSkill != null)
                {
                    if (UsesUnarmed)
                        intReturn = objSkill.PoolOtherAttribute(_objCharacter.MAG.TotalValue);
                    else
                        intReturn = objSkill.Pool;
                    // Add any Specialization bonus if applicable.
                    if (objSkill.HasSpecialization(Category))
                        intReturn += 2;
                }

                // Include any Improvements to the Spell Category.
                intReturn += ImprovementManager.ValueOf(_objCharacter, Improvement.ImprovementType.SpellCategory, false, Category);

                return intReturn;
            }
        }

        /// <summary>
        /// Tooltip information for the Dice Pool.
        /// </summary>
        public string DicePoolTooltip
        {
            get
            {
                string strReturn = string.Empty;
                Skill objSkill = Skill;
                if (objSkill != null)
                {
                    int intPool = 0;
                    if (UsesUnarmed)
                        intPool = objSkill.PoolOtherAttribute(_objCharacter.MAG.TotalValue);
                    else
                        intPool = objSkill.Pool;
                    strReturn = objSkill.DisplayNameMethod(GlobalOptions.Language) + " (" + intPool.ToString() + ')';
                    // Add any Specialization bonus if applicable.
                    if (objSkill.HasSpecialization(Category))
                        strReturn += " + " + LanguageManager.GetString("String_ExpenseSpecialization", GlobalOptions.Language) + ": " + DisplayCategory(GlobalOptions.Language) + " (2)";
                }

                // Include any Improvements to the Spell Category.
                int intSpellImprovements = ImprovementManager.ValueOf(_objCharacter, Improvement.ImprovementType.SpellCategory, false, Category);
                if (intSpellImprovements != 0)
                    strReturn += " + " + DisplayCategory(GlobalOptions.Language) + " (" + intSpellImprovements.ToString() + ')';

                return strReturn;
            }
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
                _objCachedMyXmlNode = XmlManager.Load("spells.xml", strLanguage).SelectSingleNode("/chummer/spells/spell[name = \"" + Name + "\" and category = \"" + Category + "\"]");
                _strCachedXmlNodeLanguage = strLanguage;
            }
            return _objCachedMyXmlNode;
        }
        #endregion

        #region Methods
        public TreeNode CreateTreeNode(ContextMenuStrip cmsSpell, bool blnAddCategory = false)
        {
            string strText = DisplayName(GlobalOptions.Language);
            if (blnAddCategory)
            {
                if (Category == "Rituals")
                    strText = LanguageManager.GetString("Label_Ritual", GlobalOptions.Language) + ' ' + strText;
                if (Category == "Enchantments")
                    strText = LanguageManager.GetString("Label_Enchantment", GlobalOptions.Language) + ' ' + strText;
            }
            TreeNode objNode = new TreeNode
            {
                Name = InternalId,
                Text = strText,
                Tag = InternalId,
                ContextMenuStrip = cmsSpell
            };
            if (!string.IsNullOrEmpty(Notes))
            {
                objNode.ForeColor = Color.SaddleBrown;
            }
            else if (Grade != 0)
            {
                objNode.ForeColor = SystemColors.GrayText;
            }
            objNode.ToolTipText = Notes.WordWrap(100);

            return objNode;
        }
        #endregion
    }

    /// <summary>
    /// A Focus.
    /// </summary>
    public class Focus : IHasInternalId, IHasName
    {
        private Guid _guiID;
        private readonly Character _objCharacter;
        private string _strName = string.Empty;
        private Guid _guiGearId;
        private int _intRating;

        #region Constructor, Create, Save, and Load Methods
        public Focus(Character objCharacter)
        {
            // Create the GUID for the new Focus.
            _guiID = Guid.NewGuid();
            _objCharacter = objCharacter;
        }

        /// <summary>
        /// Save the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        public void Save(XmlTextWriter objWriter)
        {
            objWriter.WriteStartElement("focus");
            objWriter.WriteElementString("guid", _guiID.ToString("D"));
            objWriter.WriteElementString("name", _strName);
            objWriter.WriteElementString("gearid", _guiGearId.ToString("D"));
            objWriter.WriteElementString("rating", _intRating.ToString());
            objWriter.WriteEndElement();
        }

        /// <summary>
        /// Load the Focus from the XmlNode.
        /// </summary>
        /// <param name="objNode">XmlNode to load.</param>
        public void Load(XmlNode objNode)
        {
            _guiID = Guid.Parse(objNode["guid"].InnerText);
            _strName = objNode["name"].InnerText;
            _intRating = Convert.ToInt32(objNode["rating"].InnerText);
            _guiGearId = Guid.Parse(objNode["gearid"].InnerText);
        }
        #endregion

        #region Properties
        /// <summary>
        /// Internal identifier which will be used to identify this Focus in the Improvement system.
        /// </summary>
        public string InternalId => _guiID.ToString("D");

        public string DisplayName { get; set; }

        /// <summary>
        /// Foci's name.
        /// </summary>
        public string Name
        {
            get => _strName;
            set => _strName = value;
        }

        /// <summary>
        /// GUID of the linked Gear.
        /// TODO: Replace this with a pointer to the Gear instead of having to do lookups.
        /// </summary>
        public string GearId
        {
            get => _guiGearId.ToString("D");
            set => _guiGearId = Guid.Parse(value);
        }

        /// <summary>
        /// Rating of the Foci.
        /// </summary>
        public int Rating
        {
            get => _intRating;
            set => _intRating = value;
        }
        #endregion
    }

    /// <summary>
    /// A Stacked Focus.
    /// </summary>
    public class StackedFocus
    {
        private Guid _guiID;
        private bool _blnBonded;
        private Guid _guiGearId;
        private readonly List<Gear> _lstGear = new List<Gear>();
        private readonly Character _objCharacter;

        #region Constructor, Create, Save, and Load Methods
        public StackedFocus(Character objCharacter)
        {
            // Create the GUID for the new Focus.
            _guiID = Guid.NewGuid();
            _objCharacter = objCharacter;
        }

        /// <summary>
        /// Save the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        public void Save(XmlTextWriter objWriter)
        {
            objWriter.WriteStartElement("stackedfocus");
            objWriter.WriteElementString("guid", _guiID.ToString("D"));
            objWriter.WriteElementString("gearid", _guiGearId.ToString("D"));
            objWriter.WriteElementString("bonded", _blnBonded.ToString());
            objWriter.WriteStartElement("gears");
            foreach (Gear objGear in _lstGear)
                objGear.Save(objWriter);
            objWriter.WriteEndElement();
            objWriter.WriteEndElement();
        }

        /// <summary>
        /// Load the Stacked Focus from the XmlNode.
        /// </summary>
        /// <param name="objNode">XmlNode to load.</param>
        public void Load(XmlNode objNode)
        {
            _guiID = Guid.Parse(objNode["guid"].InnerText);
            _guiGearId = Guid.Parse(objNode["gearid"].InnerText);
            _blnBonded = objNode["bonded"]?.InnerText == System.Boolean.TrueString;
            foreach (XmlNode nodGear in objNode.SelectNodes("gears/gear"))
            {
                Gear objGear = new Gear(_objCharacter);
                objGear.Load(nodGear);
                _lstGear.Add(objGear);
            }
        }
        #endregion

        #region Properties
        /// <summary>
        /// Internal identifier which will be used to identify this Stacked Focus in the Improvement system.
        /// </summary>
        public string InternalId => _guiID.ToString("D");

        /// <summary>
        /// GUID of the linked Gear.
        /// </summary>
        public string GearId
        {
            get => _guiGearId.ToString("D");
            set => _guiGearId = Guid.Parse(value);
        }

        /// <summary>
        /// Whether or not the Stacked Focus in Bonded.
        /// </summary>
        public bool Bonded
        {
            get => _blnBonded;
            set => _blnBonded = value;
        }

        /// <summary>
        /// The Stacked Focus' total Force.
        /// </summary>
        public int TotalForce
        {
            get
            {
                int intReturn = 0;
                foreach (Gear objGear in Gear)
                    intReturn += objGear.Rating;

                return intReturn;
            }
        }

        /// <summary>
        /// The cost in Karma to bind this Stacked Focus.
        /// </summary>
        public int BindingCost
        {
            get
            {
                int intCost = 0;
                foreach (Gear objFocus in Gear)
                {
                    // Each Focus costs an amount of Karma equal to their Force x speicific Karma cost.
                    string strFocusName = objFocus.Name;
                    string strFocusExtra = objFocus.Extra;
                    int intPosition = strFocusName.IndexOf('(');
                    if (intPosition > -1)
                        strFocusName = strFocusName.Substring(0, intPosition - 1);
                    intPosition = strFocusName.IndexOf(',');
                    if (intPosition > -1)
                        strFocusName = strFocusName.Substring(0, intPosition);
                    int intKarmaMultiplier = 1;
                    int intExtraKarmaCost = 0;
                    switch (strFocusName)
                    {
                        case "Qi Focus":
                            intKarmaMultiplier = _objCharacter.Options.KarmaQiFocus;
                            break;
                        case "Sustaining Focus":
                            intKarmaMultiplier = _objCharacter.Options.KarmaSustainingFocus;
                            break;
                        case "Counterspelling Focus":
                            intKarmaMultiplier = _objCharacter.Options.KarmaCounterspellingFocus;
                            break;
                        case "Banishing Focus":
                            intKarmaMultiplier = _objCharacter.Options.KarmaBanishingFocus;
                            break;
                        case "Binding Focus":
                            intKarmaMultiplier = _objCharacter.Options.KarmaBindingFocus;
                            break;
                        case "Weapon Focus":
                            intKarmaMultiplier = _objCharacter.Options.KarmaWeaponFocus;
                            break;
                        case "Spellcasting Focus":
                            intKarmaMultiplier = _objCharacter.Options.KarmaSpellcastingFocus;
                            break;
                        case "Summoning Focus":
                            intKarmaMultiplier = _objCharacter.Options.KarmaSummoningFocus;
                            break;
                        case "Alchemical Focus":
                            intKarmaMultiplier = _objCharacter.Options.KarmaAlchemicalFocus;
                            break;
                        case "Centering Focus":
                            intKarmaMultiplier = _objCharacter.Options.KarmaCenteringFocus;
                            break;
                        case "Masking Focus":
                            intKarmaMultiplier = _objCharacter.Options.KarmaMaskingFocus;
                            break;
                        case "Disenchanting Focus":
                            intKarmaMultiplier = _objCharacter.Options.KarmaDisenchantingFocus;
                            break;
                        case "Power Focus":
                            intKarmaMultiplier = _objCharacter.Options.KarmaPowerFocus;
                            break;
                        case "Flexible Signature Focus":
                            intKarmaMultiplier = _objCharacter.Options.KarmaFlexibleSignatureFocus;
                            break;
                        case "Ritual Spellcasting Focus":
                            intKarmaMultiplier = _objCharacter.Options.KarmaRitualSpellcastingFocus;
                            break;
                        case "Spell Shaping Focus":
                            intKarmaMultiplier = _objCharacter.Options.KarmaSpellShapingFocus;
                            break;
                        default:
                            intKarmaMultiplier = 1;
                            break;
                    }
                    foreach (Improvement objLoopImprovement in _objCharacter.Improvements.Where(x => x.ImprovedName == strFocusName && (string.IsNullOrEmpty(x.Target) || strFocusExtra.Contains(x.Target)) && x.Enabled))
                    {
                        if (objLoopImprovement.ImproveType == Improvement.ImprovementType.FocusBindingKarmaCost)
                            intExtraKarmaCost += objLoopImprovement.Value;
                        else if (objLoopImprovement.ImproveType == Improvement.ImprovementType.FocusBindingKarmaMultiplier)
                            intKarmaMultiplier += objLoopImprovement.Value;
                    }
                    intCost += (objFocus.Rating * intKarmaMultiplier) + intExtraKarmaCost;
                }
                return intCost;
            }
        }

        /// <summary>
        /// Stacked Focus Name.
        /// </summary>
        public string Name(string strLanguage)
        {
            StringBuilder strbldReturn = new StringBuilder();
            foreach (Gear objGear in Gear)
            {
                strbldReturn.Append(objGear.DisplayName(strLanguage));
                strbldReturn.Append(", ");
            }

            // Remove the trailing comma.
            if (strbldReturn.Length > 0)
                strbldReturn.Length -= 2;

            return strbldReturn.ToString();
        }

        /// <summary>
        /// List of Gear that make up the Stacked Focus.
        /// </summary>
        public IList<Gear> Gear
        {
            get => _lstGear;
        }
        #endregion

        #region Methods
        public TreeNode CreateTreeNode(Gear objGear, ContextMenuStrip cmsStackedFocus)
        {
            TreeNode objNode = objGear.CreateTreeNode(cmsStackedFocus);

            objNode.Name = InternalId;
            objNode.Text = LanguageManager.GetString("String_StackedFocus", GlobalOptions.Language) + ": " + Name(GlobalOptions.Language);
            objNode.Tag = InternalId;
            objNode.Checked = Bonded;

            return objNode;
        }
        #endregion
    }

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
            objWriter.WriteElementString("grade", _intGrade.ToString(CultureInfo.InvariantCulture));
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

    /// <summary>
    /// An Art.
    /// </summary>
    public class Art : IHasInternalId, IHasName, IHasXmlNode
    {
        private Guid _guiID;
        private string _strName = string.Empty;
        private string _strSource = string.Empty;
        private string _strPage = string.Empty;
        private XmlNode _nodBonus;
        private int _intGrade;
        private Improvement.ImprovementSource _objImprovementSource = Improvement.ImprovementSource.Art;
        private string _strNotes = string.Empty;

        private readonly Character _objCharacter;

        #region Constructor, Create, Save, Load, and Print Methods
        public Art(Character objCharacter)
        {
            // Create the GUID for the new art.
            _guiID = Guid.NewGuid();
            _objCharacter = objCharacter;
        }

        /// Create an Art from an XmlNode.
        /// <param name="objXmlArtNode">XmlNode to create the object from.</param>
        /// <param name="objCharacter">Character the Gear is being added to.</param>
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
                    _strName += " (" + ImprovementManager.SelectedValue + ')';
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
            objWriter.WriteStartElement("art");
            objWriter.WriteElementString("guid", _guiID.ToString("D"));
            objWriter.WriteElementString("name", _strName);
            objWriter.WriteElementString("source", _strSource);
            objWriter.WriteElementString("page", _strPage);
            objWriter.WriteElementString("grade", _intGrade.ToString(CultureInfo.InvariantCulture));
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
        public void Print(XmlTextWriter objWriter, string strLanguageToPrint)
        {
            objWriter.WriteStartElement("art");
            objWriter.WriteElementString("name", DisplayNameShort(strLanguageToPrint));
            objWriter.WriteElementString("name_english", Name);
            objWriter.WriteElementString("source", CommonFunctions.LanguageBookShort(Source, strLanguageToPrint));
            objWriter.WriteElementString("page", Page(strLanguageToPrint));
            objWriter.WriteElementString("improvementsource", _objImprovementSource.ToString());
            if (_objCharacter.Options.PrintNotes)
                objWriter.WriteElementString("notes", _strNotes);
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
        /// The initiate grade where the art was learned.
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
                _objCachedMyXmlNode = XmlManager.Load("metamagic.xml", strLanguage).SelectSingleNode("/chummer/arts/art[name = \"" + Name + "\"]");
                _strCachedXmlNodeLanguage = strLanguage;
            }
            return _objCachedMyXmlNode;
        }
        #endregion

        #region Methods
        public TreeNode CreateTreeNode(ContextMenuStrip cmsArt, bool blnAddCategory = false)
        {
            string strText = DisplayName(GlobalOptions.Language);
            if (blnAddCategory)
                strText = LanguageManager.GetString("Label_Art", GlobalOptions.Language) + ' ' + strText;
            TreeNode objNode = new TreeNode
            {
                Name = InternalId,
                Text = strText,
                Tag = InternalId,
                ContextMenuStrip = cmsArt
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

    /// <summary>
    /// An Enhancement.
    /// </summary>
    public class Enhancement : IHasInternalId, IHasName, IHasXmlNode
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
        /// <param name="objXmlEnhancementNode">XmlNode to create the object from.</param>
        /// <param name="objCharacter">Character the Enhancement is being added to.</param>
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
                    _strName += " (" + ImprovementManager.SelectedValue + ')';
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
            objWriter.WriteElementString("grade", _intGrade.ToString(CultureInfo.InvariantCulture));
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
        public void Print(XmlTextWriter objWriter, string strLanguageToPrint)
        {
            objWriter.WriteStartElement("enhancement");
            objWriter.WriteElementString("name", DisplayNameShort(strLanguageToPrint));
            objWriter.WriteElementString("name_english", Name);
            objWriter.WriteElementString("source", CommonFunctions.LanguageBookShort(Source, strLanguageToPrint));
            objWriter.WriteElementString("page", Page(strLanguageToPrint));
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
                _objCachedMyXmlNode = XmlManager.Load("powers.xml", strLanguage).SelectSingleNode("/chummer/enhancements/enhancement[name = \"" + Name + "\"]");
                _strCachedXmlNodeLanguage = strLanguage;
            }
            return _objCachedMyXmlNode;
        }
        #endregion

        #region Methods
        public TreeNode CreateTreeNode(ContextMenuStrip cmsEnhancement, bool blnAddCategory = false)
        {
            string strText = DisplayName(GlobalOptions.Language);
            if (blnAddCategory)
                strText = LanguageManager.GetString("Label_Enhancement", GlobalOptions.Language) + ' ' + strText;
            TreeNode objNode = new TreeNode
            {
                Name = InternalId,
                Text = strText,
                Tag = InternalId,
                ContextMenuStrip = cmsEnhancement
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

    /// <summary>
    /// A Technomancer Program or Complex Form.
    /// </summary>
    public class ComplexForm : IHasInternalId, IHasName, IHasXmlNode
    {
        private Guid _guiID;
        private string _strName = string.Empty;
        private string _strTarget = string.Empty;
        private string _strDuration = string.Empty;
        private string _strFV = string.Empty;
        private string _strSource = string.Empty;
        private string _strPage = string.Empty;
        private string _strNotes = string.Empty;
        private string _strExtra = string.Empty;
        private int _intGrade = 0;
        private readonly Character _objCharacter;

        #region Constructor, Create, Save, Load, and Print Methods
        public ComplexForm(Character objCharacter)
        {
            // Create the GUID for the new Complex Form.
            _guiID = Guid.NewGuid();
            _objCharacter = objCharacter;
        }

        /// Create a Complex Form from an XmlNode.
        /// <param name="objXmlComplexFormNode">XmlNode to create the object from.</param>
        /// <param name="strForcedValue">Value to forcefully select for any ImprovementManager prompts.</param>
        public void Create(XmlNode objXmlComplexFormNode, string strExtra = "")
        {
            if (objXmlComplexFormNode.TryGetStringFieldQuickly("name", ref _strName))
                _objCachedMyXmlNode = null;
            objXmlComplexFormNode.TryGetStringFieldQuickly("target", ref _strTarget);
            objXmlComplexFormNode.TryGetStringFieldQuickly("source", ref _strSource);
            objXmlComplexFormNode.TryGetStringFieldQuickly("page", ref _strPage);
            objXmlComplexFormNode.TryGetStringFieldQuickly("duration", ref _strDuration);
            objXmlComplexFormNode.TryGetStringFieldQuickly("fv", ref _strFV);
            if (!objXmlComplexFormNode.TryGetStringFieldQuickly("altnotes", ref _strNotes))
                objXmlComplexFormNode.TryGetStringFieldQuickly("notes", ref _strNotes);
            _strExtra = strExtra;

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
            objWriter.WriteStartElement("complexform");
            objWriter.WriteElementString("guid", _guiID.ToString("D"));
            objWriter.WriteElementString("name", _strName);
            objWriter.WriteElementString("target", _strTarget);
            objWriter.WriteElementString("duration", _strDuration);
            objWriter.WriteElementString("fv", _strFV);
            objWriter.WriteElementString("extra", _strExtra);
            objWriter.WriteElementString("source", _strSource);
            objWriter.WriteElementString("page", _strPage);
            objWriter.WriteElementString("notes", _strNotes);
            objWriter.WriteElementString("grade", _intGrade.ToString(CultureInfo.InvariantCulture));
            objWriter.WriteEndElement();
            _objCharacter.SourceProcess(_strSource);
        }

        /// <summary>
        /// Load the Complex Form from the XmlNode.
        /// </summary>
        /// <param name="objNode">XmlNode to load.</param>
        public void Load(XmlNode objNode)
        {
            objNode.TryGetField("guid", Guid.TryParse, out _guiID);
            if (objNode.TryGetStringFieldQuickly("name", ref _strName))
                _objCachedMyXmlNode = null;
            objNode.TryGetStringFieldQuickly("target", ref _strTarget);
            objNode.TryGetStringFieldQuickly("source", ref _strSource);
            objNode.TryGetStringFieldQuickly("page", ref _strPage);
            objNode.TryGetStringFieldQuickly("duration", ref _strDuration);
            objNode.TryGetStringFieldQuickly("extra", ref _strExtra);
            objNode.TryGetStringFieldQuickly("fv", ref _strFV);
            objNode.TryGetStringFieldQuickly("notes", ref _strNotes);
            objNode.TryGetInt32FieldQuickly("grade", ref _intGrade);
        }

        /// <summary>
        /// Print the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        public void Print(XmlTextWriter objWriter, string strLanguageToPrint)
        {
            objWriter.WriteStartElement("complexform");
            objWriter.WriteElementString("name", DisplayNameShort(strLanguageToPrint));
            objWriter.WriteElementString("name_english", Name);
            objWriter.WriteElementString("duration", Duration);
            objWriter.WriteElementString("fv", FV);
            objWriter.WriteElementString("target", Target);
            objWriter.WriteElementString("source", CommonFunctions.LanguageBookShort(Source, strLanguageToPrint));
            objWriter.WriteElementString("page", Page(strLanguageToPrint));
            if (_objCharacter.Options.PrintNotes)
                objWriter.WriteElementString("notes", Notes);
            objWriter.WriteEndElement();
        }
        #endregion

        #region Properties
        /// <summary>
        /// Internal identifier which will be used to identify this Complex Form in the Improvement system.
        /// </summary>
        public string InternalId => _guiID.ToString("D");

        /// <summary>
        /// Complex Form's name.
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
        /// Complex Form's extra info.
        /// </summary>
        public string Extra
        {
            get => _strExtra;
            set => _strExtra = LanguageManager.ReverseTranslateExtra(value, GlobalOptions.Language);
        }

        /// <summary>
        /// Complex Form's grade.
        /// </summary>
        public int Grade
        {
            get => _intGrade;
            set => _intGrade = value;
        }

        /// <summary>
        /// The name of the object as it should be displayed on printouts (translated name only).
        /// </summary>
        public string DisplayNameShort(string strLanguage)
        {
            string strReturn = _strName;
            // Get the translated name if applicable.
            if (strLanguage != GlobalOptions.DefaultLanguage)
                strReturn = GetNode(strLanguage)?["translate"]?.InnerText ?? _strName;

            if (!string.IsNullOrEmpty(_strExtra))
            {
                string strExtra = _strExtra;
                if (strLanguage != GlobalOptions.DefaultLanguage)
                    strExtra = LanguageManager.TranslateExtra(_strExtra, strLanguage);
                strReturn += " (" + strExtra + ')';
            }
            return strReturn;
        }

        /// <summary>
        /// The name of the object as it should be displayed in lists. Name (Extra).
        /// </summary>
        public string DisplayName
        {
            get
            {
                return DisplayNameShort(GlobalOptions.Language);
            }
        }

        /// <summary>
        /// Complex Form's Duration.
        /// </summary>
        public string Duration
        {
            get => _strDuration;
            set => _strDuration = value;
        }

        /// <summary>
        /// The Complex Form's FV.
        /// </summary>
        public string FV
        {
            get => _strFV;
            set => _strFV = value;
        }

        /// <summary>
        /// The Complex Form's Target.
        /// </summary>
        public string Target
        {
            get => _strTarget;
            set => _strTarget = value;
        }

        /// <summary>
        /// Complex Form's Source.
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
                _objCachedMyXmlNode = XmlManager.Load("complexforms.xml", strLanguage).SelectSingleNode("/chummer/complexforms/complexform[name = \"" + Name + "\"]");
                _strCachedXmlNodeLanguage = strLanguage;
            }
            return _objCachedMyXmlNode;
        }
        #endregion

        #region Methods
        public TreeNode CreateTreeNode(ContextMenuStrip cmsComplexForm)
        {
            TreeNode objNode = new TreeNode
            {
                Name = InternalId,
                Text = DisplayName,
                Tag = InternalId,
                ContextMenuStrip = cmsComplexForm
            };
            if (!string.IsNullOrEmpty(Notes))
            {
                objNode.ForeColor = Color.SaddleBrown;
            }
            else if (Grade != 0)
            {
                objNode.ForeColor = SystemColors.GrayText;
            }
            objNode.ToolTipText = Notes.WordWrap(100);
            return objNode;
        }
        #endregion
    }

    /// <summary>
    /// An AI Program or Advanced Program.
    /// </summary>
    public class AIProgram : IHasInternalId, IHasName, IHasXmlNode
    {
        private Guid _guiID;
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
        /// <param name="strForcedValue">Value to forcefully select for any ImprovementManager prompts.</param>
        public void Create(XmlNode objXmlProgramNode, bool boolIsAdvancedProgram, string strExtra = "", bool boolCanDelete = true)
        {
            if (objXmlProgramNode.TryGetStringFieldQuickly("name", ref _strName))
                _objCachedMyXmlNode = null;
            _strRequiresProgram = LanguageManager.GetString("String_None", GlobalOptions.Language);
            _boolCanDelete = boolCanDelete;
            objXmlProgramNode.TryGetStringFieldQuickly("require", ref _strRequiresProgram);
            objXmlProgramNode.TryGetStringFieldQuickly("source", ref _strSource);
            objXmlProgramNode.TryGetStringFieldQuickly("page", ref _strPage);
            if (!objXmlProgramNode.TryGetStringFieldQuickly("altnotes", ref _strNotes))
                objXmlProgramNode.TryGetStringFieldQuickly("notes", ref _strNotes);
            _strExtra = strExtra;
            _boolIsAdvancedProgram = boolIsAdvancedProgram;
        }

        /// <summary>
        /// Save the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        public void Save(XmlTextWriter objWriter)
        {
            objWriter.WriteStartElement("aiprogram");
            objWriter.WriteElementString("guid", _guiID.ToString("D"));
            objWriter.WriteElementString("name", _strName);
            objWriter.WriteElementString("requiresprogram", _strRequiresProgram);
            objWriter.WriteElementString("extra", _strExtra);
            objWriter.WriteElementString("source", _strSource);
            objWriter.WriteElementString("page", _strPage);
            objWriter.WriteElementString("notes", _strNotes);
            objWriter.WriteElementString("isadvancedprogram", _boolIsAdvancedProgram.ToString());
            objWriter.WriteEndElement();
            _objCharacter.SourceProcess(_strSource);
        }

        /// <summary>
        /// Load the Program from the XmlNode.
        /// </summary>
        /// <param name="objNode">XmlNode to load.</param>
        public void Load(XmlNode objNode)
        {
            objNode.TryGetField("guid", Guid.TryParse, out _guiID);
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
        public void Print(XmlTextWriter objWriter, string strLanguageToPrint)
        {
            objWriter.WriteStartElement("aiprogram");
            objWriter.WriteElementString("name", DisplayNameShort(strLanguageToPrint));
            objWriter.WriteElementString("name_english", Name);
            if (string.IsNullOrEmpty(_strRequiresProgram) || _strRequiresProgram == LanguageManager.GetString("String_None", strLanguageToPrint))
                objWriter.WriteElementString("requiresprogram", LanguageManager.GetString("String_None", strLanguageToPrint));
            else
                objWriter.WriteElementString("requiresprogram", DisplayRequiresProgram(strLanguageToPrint));
            objWriter.WriteElementString("source", CommonFunctions.LanguageBookShort(Source, strLanguageToPrint));
            objWriter.WriteElementString("page", Page(strLanguageToPrint));
            if (_objCharacter.Options.PrintNotes)
                objWriter.WriteElementString("notes", Notes);
            objWriter.WriteEndElement();
        }
        #endregion

        #region Properties
        /// <summary>
        /// Internal identifier which will be used to identify this AI Program in the Improvement system.
        /// </summary>
        public string InternalId => _guiID.ToString("D");

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
            set => _strExtra = LanguageManager.ReverseTranslateExtra(value, GlobalOptions.Language);
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
                strReturn += " (" + strExtra + ')';
            }
            return strReturn;
        }

        /// <summary>
        /// The name of the object as it should be displayed in lists. Name (Extra).
        /// </summary>
        public string DisplayName
        {
            get
            {
                return DisplayNameShort(GlobalOptions.Language);
            }
        }

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
            XmlNode objNode = XmlManager.Load("programs.xml", strLanguage).SelectSingleNode("/chummer/programs/program[name = \"" + RequiresProgram + "\"]");
            return objNode?["translate"]?.InnerText ?? objNode?["name"]?.InnerText ?? LanguageManager.GetString("String_None", strLanguage);
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
        /// If the AI Program is an Advanced Program.
        /// </summary>
        public bool IsAdvancedProgram => _boolIsAdvancedProgram;

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
                _objCachedMyXmlNode = XmlManager.Load("programs.xml", strLanguage).SelectSingleNode("/chummer/programs/program[name = \"" + Name + "\"]");
                _strCachedXmlNodeLanguage = strLanguage;
            }
            return _objCachedMyXmlNode;
        }
        #endregion

        #region Methods
        public TreeNode CreateTreeNode(ContextMenuStrip cmsAIProgram)
        {
            TreeNode objNode = new TreeNode
            {
                Name = InternalId,
                Text = DisplayName,
                Tag = InternalId,
                ContextMenuStrip = cmsAIProgram
            };
            if (!string.IsNullOrEmpty(Notes))
            {
                objNode.ForeColor = Color.SaddleBrown;
            }
            else if (!CanDelete)
            {
                objNode.ForeColor = SystemColors.GrayText;
            }
            objNode.ToolTipText = Notes.WordWrap(100);
            return objNode;
        }
        #endregion
    }

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
        private List<MartialArtTechnique> _lstTechniques = new List<MartialArtTechnique>();
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
            objWriter.WriteElementString("rating", _intRating.ToString(CultureInfo.InvariantCulture));
            objWriter.WriteElementString("cost", _intKarmaCost.ToString(CultureInfo.InvariantCulture));
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
        public IList<MartialArtTechnique> Techniques => _lstTechniques;
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

    /// <summary>
    /// A Martial Arts Technique.
    /// </summary>
    public class MartialArtTechnique : IHasInternalId, IHasName, IHasXmlNode
    {
        private Guid _guiID;
        private string _strName = string.Empty;
        private string _strNotes = string.Empty;
        private string _strSource = string.Empty;
        private string _strPage = string.Empty;
        private string _strSourceId = string.Empty;
        private Character _objCharacter;

        #region Constructor, Create, Save, Load, and Print Methods
        public MartialArtTechnique(Character objCharacter)
        {
            // Create the GUID for the new Martial Art Technique.
            _guiID = Guid.NewGuid();
            _objCharacter = objCharacter;
        }

        /// Create a Martial Art Technique from an XmlNode.
        /// <param name="xmlTechniqueDataNode">XmlNode to create the object from.</param>
        /// <param name="objCharacter">Character the Gear is being added to.</param>
        public void Create(XmlNode xmlTechniqueDataNode)
        {
            if (xmlTechniqueDataNode.TryGetStringFieldQuickly("id", ref _strSourceId))
                _objCachedMyXmlNode = null;
            if (xmlTechniqueDataNode.TryGetStringFieldQuickly("name", ref _strName))
                if (!xmlTechniqueDataNode.TryGetStringFieldQuickly("altnotes", ref _strNotes))
                    xmlTechniqueDataNode.TryGetStringFieldQuickly("notes", ref _strNotes);
            xmlTechniqueDataNode.TryGetStringFieldQuickly("source", ref _strSource);
            xmlTechniqueDataNode.TryGetStringFieldQuickly("page", ref _strPage);

            if (xmlTechniqueDataNode["bonus"] != null)
            {
                if (!ImprovementManager.CreateImprovements(_objCharacter, Improvement.ImprovementSource.MartialArtTechnique, _guiID.ToString("D"), xmlTechniqueDataNode["bonus"], false, 1, DisplayName(GlobalOptions.Language)))
                {
                    _guiID = Guid.Empty;
                    return;
                }
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
            set
            {
                _strName = value;
            }
        }

        /// <summary>
        /// The name of the object as it should be displayed on printouts (translated name only).
        /// </summary>
        public string DisplayName(string strLanguage)
        {
            // Get the translated name if applicable.
            if (strLanguage == GlobalOptions.DefaultLanguage)
                return Name;

            return GetNode(strLanguage)?.Attributes?["translate"]?.InnerText ?? Name;
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
                _objCachedMyXmlNode = XmlManager.Load("martialarts.xml", strLanguage).SelectSingleNode("/chummer/techniques/technique[id = \"" + _strSourceId + "\"]");
                _strCachedXmlNodeLanguage = strLanguage;
            }
            return _objCachedMyXmlNode;
        }
        #endregion

        #region Methods
        public TreeNode CreateTreeNode(ContextMenuStrip cmsMartialArtTechnique)
        {
            TreeNode objNode = new TreeNode
            {
                Name = InternalId,
                Text = DisplayName(GlobalOptions.Language),
                Tag = InternalId,
                ContextMenuStrip = cmsMartialArtTechnique
            };
            if (!string.IsNullOrEmpty(Notes))
            {
                objNode.ForeColor = Color.SaddleBrown;
            }
            objNode.ToolTipText = Notes.WordWrap(100);

            return objNode;
        }
        #endregion
    }

    /// <summary>
    /// A Martial Art Maneuver.
    /// </summary>
    public class MartialArtManeuver : IHasInternalId, IHasName, IHasXmlNode
    {
        private Guid _guiID;
        private string _strName = string.Empty;
        private string _strSource = string.Empty;
        private string _strPage = string.Empty;
        private string _strNotes = string.Empty;
        private readonly Character _objCharacter;

        #region Constructor, Create, Save, Load, and Print Methods
        public MartialArtManeuver(Character objCharacter)
        {
            // Create the GUID for the new Martial Art Maneuver.
            _guiID = Guid.NewGuid();
            _objCharacter = objCharacter;
        }

        /// Create a Martial Art Maneuver from an XmlNode.
        /// <param name="objXmlManeuverNode">XmlNode to create the object from.</param>
        public void Create(XmlNode objXmlManeuverNode)
        {
            if (objXmlManeuverNode.TryGetStringFieldQuickly("name", ref _strName))
                _objCachedMyXmlNode = null;
            objXmlManeuverNode.TryGetStringFieldQuickly("source", ref _strSource);
            objXmlManeuverNode.TryGetStringFieldQuickly("page", ref _strPage);
            if (!objXmlManeuverNode.TryGetStringFieldQuickly("altnotes", ref _strNotes))
                objXmlManeuverNode.TryGetStringFieldQuickly("notes", ref _strNotes);
        }

        /// <summary>
        /// Save the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        public void Save(XmlTextWriter objWriter)
        {
            objWriter.WriteStartElement("martialartmaneuver");
            objWriter.WriteElementString("guid", _guiID.ToString("D"));
            objWriter.WriteElementString("name", _strName);
            objWriter.WriteElementString("source", _strSource);
            objWriter.WriteElementString("page", _strPage);
            objWriter.WriteElementString("notes", _strNotes);
            objWriter.WriteEndElement();
            _objCharacter.SourceProcess(_strSource);
        }

        /// <summary>
        /// Load the Martial Art Maneuver from the XmlNode.
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
            objNode.TryGetStringFieldQuickly("notes", ref _strNotes);
        }

        /// <summary>
        /// Print the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        public void Print(XmlTextWriter objWriter, string strLanguageToPrint)
        {
            objWriter.WriteStartElement("martialartmaneuver");
            objWriter.WriteElementString("name", DisplayNameShort(strLanguageToPrint));
            objWriter.WriteElementString("name_english", Name);
            objWriter.WriteElementString("source", CommonFunctions.LanguageBookShort(Source, strLanguageToPrint));
            objWriter.WriteElementString("page", Page(strLanguageToPrint));
            if (_objCharacter.Options.PrintNotes)
                objWriter.WriteElementString("notes", Notes);
            objWriter.WriteEndElement();
        }
        #endregion

        #region Properties
        /// <summary>
        /// Internal identifier which will be used to identify this Martial Art Maneuver in the Improvement system.
        /// </summary>
        public string InternalId => _guiID.ToString("D");

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
        /// Page.
        /// </summary>
        public string Page(string strLanguage)
        {
            // Get the translated name if applicable.
            if (strLanguage != GlobalOptions.DefaultLanguage)
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
                _objCachedMyXmlNode = XmlManager.Load("martialarts.xml", strLanguage).SelectSingleNode("/chummer/maneuvers/maneuver[name = \"" + Name + "\"]");
                _strCachedXmlNodeLanguage = strLanguage;
            }
            return _objCachedMyXmlNode;
        }
        #endregion

        #region Methods
        public TreeNode CreateTreeNode(ContextMenuStrip cmsMartialArtTechnique)
        {
            TreeNode objNode = new TreeNode
            {
                Name = InternalId,
                Text = DisplayName(GlobalOptions.Language),
                Tag = InternalId,
                ContextMenuStrip = cmsMartialArtTechnique
            };
            if (!string.IsNullOrEmpty(Notes))
            {
                objNode.ForeColor = Color.SaddleBrown;
            }
            objNode.ToolTipText = Notes.WordWrap(100);

            return objNode;
        }
        #endregion
    }

    /// <summary>
    /// Type of Contact.
    /// </summary>
    public enum LimitType
    {
        Physical = 0,
        Mental,
        Social,
        Astral,
        NumLimitTypes // 🡐 This one should always be the last defined enum
    }

    /// <summary>
    /// A Skill Limit Modifier.
    /// </summary>
    public class LimitModifier : IHasInternalId, IHasName
    {
        private Guid _guiID;
        private string _strName = string.Empty;
        private string _strNotes = string.Empty;
        private string _strLimit = string.Empty;
        private string _strCondition = string.Empty;
        private int _intBonus;
        private Character _objCharacter;

        #region Constructor, Create, Save, Load, and Print Methods
        public LimitModifier(Character objCharacter)
        {
            // Create the GUID for the new Skill Limit Modifier.
            _guiID = Guid.NewGuid();
            _objCharacter = objCharacter;
        }

        /// Create a Skill Limit Modifier from an XmlNode.
        /// <param name="objXmlAdvantageNode">XmlNode to create the object from.</param>
        /// <param name="objCharacter">Character the Gear is being added to.</param>
        public void Create(XmlNode objXmlLimitModifierNode)
        {
            _strName = objXmlLimitModifierNode["name"].InnerText;

            if (objXmlLimitModifierNode["bonus"] != null)
            {
                if (!ImprovementManager.CreateImprovements(_objCharacter, Improvement.ImprovementSource.MartialArtTechnique, _guiID.ToString("D"), objXmlLimitModifierNode["bonus"], false, 1, DisplayNameShort))
                {
                    _guiID = Guid.Empty;
                    return;
                }
            }
            if (!objXmlLimitModifierNode.TryGetStringFieldQuickly("altnotes", ref _strNotes))
                objXmlLimitModifierNode.TryGetStringFieldQuickly("notes", ref _strNotes);
        }

        /// Create a Skill Limit Modifier from properties.
        /// <param name="strName">The name of the modifier.</param>
        /// <param name="intBonus">The bonus amount.</param>
        /// <param name="strLimit">The limit this modifies.</param>
        public void Create(string strName, int intBonus, string strLimit, string strCondition)
        {
            _strName = strName;
            _strLimit = strLimit;
            _intBonus = intBonus;
            _strCondition = strCondition;
        }

        /// <summary>
        /// Save the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        public void Save(XmlTextWriter objWriter)
        {
            objWriter.WriteStartElement("limitmodifier");
            objWriter.WriteElementString("guid", _guiID.ToString("D"));
            objWriter.WriteElementString("name", _strName);
            objWriter.WriteElementString("limit", _strLimit);
            objWriter.WriteElementString("condition", _strCondition);
            objWriter.WriteElementString("bonus", _intBonus.ToString(CultureInfo.InvariantCulture));
            objWriter.WriteElementString("notes", _strNotes);
            objWriter.WriteEndElement();
        }

        /// <summary>
        /// Load the Skill Limit Modifier from the XmlNode.
        /// </summary>
        /// <param name="objNode">XmlNode to load.</param>
        public void Load(XmlNode objNode)
        {
            if (objNode.TryGetField("guid", Guid.TryParse, out _guiID))
                _guiID = Guid.NewGuid();
            objNode.TryGetStringFieldQuickly("name", ref _strName);
            objNode.TryGetStringFieldQuickly("limit", ref _strLimit);
            objNode.TryGetInt32FieldQuickly("bonus", ref _intBonus);
            objNode.TryGetStringFieldQuickly("condition", ref _strCondition);
            objNode.TryGetStringFieldQuickly("notes", ref _strNotes);
        }

        /// <summary>
        /// Print the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        public void Print(XmlTextWriter objWriter, string strLanguageToPrint)
        {
            objWriter.WriteStartElement("limitmodifier");
            objWriter.WriteElementString("name", DisplayName);
            objWriter.WriteElementString("name_english", Name);
            objWriter.WriteElementString("condition", Condition);
            if (_objCharacter.Options.PrintNotes)
                objWriter.WriteElementString("notes", Notes);
            objWriter.WriteEndElement();
        }
        #endregion

        #region Properties
        /// <summary>
        /// Internal identifier which will be used to identify this Skill Limit Modifier in the Improvement system.
        /// </summary>
        public string InternalId => _guiID.ToString("D");

        /// <summary>
        /// Internal identifier which will be used to identify this Skill Limit Modifier in the Improvement system.
        /// </summary>
        public Guid Guid
        {
            get => _guiID;
            set => _guiID = value;
        }

        /// <summary>
        /// Name.
        /// </summary>
        public string Name
        {
            get => _strName;
            set => _strName = value;
        }

        /// <summary>
        /// Name.
        /// </summary>
        public string Notes
        {
            get => _strNotes;
            set => _strNotes = value;
        }

        /// <summary>
        /// Limit.
        /// </summary>
        public string Limit
        {
            get => _strLimit;
            set => _strLimit = value;
        }

        /// <summary>
        /// Condition.
        /// </summary>
        public string Condition
        {
            get => _strCondition;
            set => _strCondition = value;
        }

        /// <summary>
        /// Bonus.
        /// </summary>
        public int Bonus
        {
            get => _intBonus;
            set => _intBonus = value;
        }

        /// <summary>
        /// The name of the object as it should be displayed on printouts (translated name only).
        /// </summary>
        public string DisplayNameShort
        {
            get
            {
                string strReturn = _strName;
                return strReturn;
            }
        }

        /// <summary>
        /// The name of the object as it should be displayed in lists. Name (Extra).
        /// </summary>
        public string DisplayName
        {
            get
            {
                string strBonus = string.Empty;
                if (_intBonus > 0)
                    strBonus = '+' + _intBonus.ToString();
                else
                    strBonus = _intBonus.ToString();

                string strReturn = DisplayNameShort + " [" + strBonus + ']';
                if (!string.IsNullOrEmpty(_strCondition))
                    strReturn += " (" + _strCondition + ')';
                return strReturn;
            }
        }
        #endregion

        #region Methods
        public TreeNode CreateTreeNode(ContextMenuStrip cmsLimitModifier)
        {
            TreeNode objNode = new TreeNode
            {
                Name = InternalId,
                ContextMenuStrip = cmsLimitModifier,
                Text = DisplayName,
                Tag = InternalId
            };
            if (!string.IsNullOrEmpty(Notes))
            {
                objNode.ForeColor = Color.SaddleBrown;
            }
            objNode.ToolTipText = Notes.WordWrap(100);
            return objNode;
        }
        #endregion
    }

    /// <summary>
    /// Type of Contact.
    /// </summary>
    public enum ContactType
    {
        Contact = 0,
        Enemy = 1,
        Pet = 2,
    }

    /// <summary>
    /// A Contact or Enemy.
    /// </summary>
    public class Contact : INotifyPropertyChanged, IHasName, IHasMugshots
    {
        private string _strName = string.Empty;
        private string _strRole = string.Empty;
        private string _strLocation = string.Empty;
        private string _strUnique = Guid.NewGuid().ToString("D");

        private int _intConnection = 1;
        private int _intLoyalty = 1;
        private string _strMetatype = string.Empty;
        private string _strSex = string.Empty;
        private string _strAge = string.Empty;
        private string _strType = string.Empty;
        private string _strPreferredPayment = string.Empty;
        private string _strHobbiesVice = string.Empty;
        private string _strPersonalLife = string.Empty;

        private string _strGroupName = string.Empty;
        private ContactType _eContactType = ContactType.Contact;
        private string _strFileName = string.Empty;
        private string _strRelativeName = string.Empty;
        private Character _objLinkedCharacter;
        private string _strNotes = string.Empty;
        private Color _objColour;
        private bool _blnFree;
        private bool _blnIsGroup;
        private readonly Character _objCharacter;
        private bool _blnMadeMan;
        private bool _blnBlackmail;
        private bool _blnFamily;
        private bool _readonly;
        private bool _blnForceLoyalty;

        private readonly List<Image> _lstMugshots = new List<Image>();
        private int _intMainMugshotIndex = -1;

        public event PropertyChangedEventHandler PropertyChanged;

        #region Helper Methods
        /// <summary>
        /// Convert a string to a ContactType.
        /// </summary>
        /// <param name="strValue">String value to convert.</param>
        public static ContactType ConvertToContactType(string strValue)
        {
            if (string.IsNullOrEmpty(strValue))
                return default(ContactType);
            switch (strValue)
            {
                case "Contact":
                    return ContactType.Contact;
                case "Pet":
                    return ContactType.Pet;
                default:
                    return ContactType.Enemy;
            }
        }
        #endregion

        #region Constructor, Save, Load, and Print Methods
        public Contact(Character objCharacter)
        {
            _objCharacter = objCharacter;
        }

        /// <summary>
        /// Save the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        public void Save(XmlTextWriter objWriter)
        {
            objWriter.WriteStartElement("contact");
            objWriter.WriteElementString("name", _strName);
            objWriter.WriteElementString("role", _strRole);
            objWriter.WriteElementString("location", _strLocation);
            objWriter.WriteElementString("connection", _intConnection.ToString(CultureInfo.InvariantCulture));
            objWriter.WriteElementString("loyalty", _intLoyalty.ToString(CultureInfo.InvariantCulture));
            objWriter.WriteElementString("metatype", _strMetatype);
            objWriter.WriteElementString("sex", _strSex);
            objWriter.WriteElementString("age", _strAge);
            objWriter.WriteElementString("contacttype", _strType);
            objWriter.WriteElementString("preferredpayment", _strPreferredPayment);
            objWriter.WriteElementString("hobbiesvice", _strHobbiesVice);
            objWriter.WriteElementString("personallife", _strPersonalLife);
            objWriter.WriteElementString("type", _eContactType.ToString());
            objWriter.WriteElementString("file", _strFileName);
            objWriter.WriteElementString("relative", _strRelativeName);
            objWriter.WriteElementString("notes", _strNotes);
            objWriter.WriteElementString("groupname", _strGroupName);
            objWriter.WriteElementString("colour", _objColour.ToArgb().ToString());
            objWriter.WriteElementString("free", _blnFree.ToString());
            objWriter.WriteElementString("group", _blnIsGroup.ToString());
            objWriter.WriteElementString("forceloyalty", _blnForceLoyalty.ToString());
            objWriter.WriteElementString("family", _blnFamily.ToString());
            objWriter.WriteElementString("blackmail", _blnBlackmail.ToString());

            if (ReadOnly) objWriter.WriteElementString("readonly", string.Empty);

            if (_strUnique != null)
            {
                objWriter.WriteElementString("guid", _strUnique);
            }

            SaveMugshots(objWriter);

            /* Disabled for now because we cannot change any properties in the linked character anyway
            if (LinkedCharacter?.IsSaving == false && !Program.MainForm.OpenCharacterForms.Any(x => x.CharacterObject == LinkedCharacter))
                LinkedCharacter.Save();
                */

            objWriter.WriteEndElement();
        }

        /// <summary>
        /// Load the Contact from the XmlNode.
        /// </summary>
        /// <param name="objNode">XmlNode to load.</param>
        public void Load(XmlNode objNode)
        {
            objNode.TryGetStringFieldQuickly("name", ref _strName);
            objNode.TryGetStringFieldQuickly("role", ref _strRole);
            objNode.TryGetStringFieldQuickly("location", ref _strLocation);
            objNode.TryGetInt32FieldQuickly("connection", ref _intConnection);
            objNode.TryGetInt32FieldQuickly("loyalty", ref _intLoyalty);
            objNode.TryGetStringFieldQuickly("metatype", ref _strMetatype);
            objNode.TryGetStringFieldQuickly("sex", ref _strSex);
            objNode.TryGetStringFieldQuickly("age", ref _strAge);
            objNode.TryGetStringFieldQuickly("contacttype", ref _strType);
            objNode.TryGetStringFieldQuickly("preferredpayment", ref _strPreferredPayment);
            objNode.TryGetStringFieldQuickly("hobbiesvice", ref _strHobbiesVice);
            objNode.TryGetStringFieldQuickly("personallife", ref _strPersonalLife);
            _eContactType = ConvertToContactType(objNode["type"]?.InnerText);
            objNode.TryGetStringFieldQuickly("file", ref _strFileName);
            objNode.TryGetStringFieldQuickly("notes", ref _strNotes);
            objNode.TryGetStringFieldQuickly("groupname", ref _strGroupName);
            objNode.TryGetBoolFieldQuickly("free", ref _blnFree);
            objNode.TryGetBoolFieldQuickly("group", ref _blnIsGroup);
            objNode.TryGetStringFieldQuickly("guid", ref _strUnique);
            objNode.TryGetBoolFieldQuickly("family", ref _blnFamily);
            objNode.TryGetBoolFieldQuickly("blackmail", ref _blnBlackmail);
            if (objNode["colour"] != null)
            {
                int intTmp = _objColour.ToArgb();
                if (objNode.TryGetInt32FieldQuickly("colour", ref intTmp))
                    _objColour = Color.FromArgb(intTmp);
            }

            if (objNode["readonly"] != null)
                _readonly = true;
            if (objNode["forceloyalty"] != null)
            {
                objNode.TryGetBoolFieldQuickly("forceloyalty", ref _blnForceLoyalty);
            }
            else if (objNode["mademan"] != null)
            {
                objNode.TryGetBoolFieldQuickly("mademan", ref _blnForceLoyalty);
            }

            RefreshLinkedCharacter(false);

            // Mugshots
            LoadMugshots(objNode);
        }

        /// <summary>
        /// Print the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        public void Print(XmlTextWriter objWriter, CultureInfo objCulture, string strLanguageToPrint)
        {
            objWriter.WriteStartElement("contact");
            objWriter.WriteElementString("name", Name);
            objWriter.WriteElementString("role", DisplayRoleMethod(strLanguageToPrint));
            objWriter.WriteElementString("location", Location);
            if (!IsGroup)
                objWriter.WriteElementString("connection", Connection.ToString(objCulture));
            else
                objWriter.WriteElementString("connection", LanguageManager.GetString("String_Group", strLanguageToPrint) + "(" + Connection.ToString(objCulture) + ')');
            objWriter.WriteElementString("loyalty", Loyalty.ToString(objCulture));
            objWriter.WriteElementString("metatype", DisplayMetatypeMethod(strLanguageToPrint));
            objWriter.WriteElementString("sex", DisplaySexMethod(strLanguageToPrint));
            objWriter.WriteElementString("age", DisplayAgeMethod(strLanguageToPrint));
            objWriter.WriteElementString("contacttype", DisplayTypeMethod(strLanguageToPrint));
            objWriter.WriteElementString("preferredpayment", DisplayPreferredPaymentMethod(strLanguageToPrint));
            objWriter.WriteElementString("hobbiesvice", DisplayHobbiesViceMethod(strLanguageToPrint));
            objWriter.WriteElementString("personallife", DisplayPersonalLifeMethod(strLanguageToPrint));
            objWriter.WriteElementString("type", LanguageManager.GetString("String_" + EntityType.ToString(), strLanguageToPrint));
            objWriter.WriteElementString("forceloyalty", ForceLoyalty.ToString());
            objWriter.WriteElementString("blackmail", Blackmail.ToString());
            objWriter.WriteElementString("family", Family.ToString());
            if (_objCharacter.Options.PrintNotes)
                objWriter.WriteElementString("notes", Notes);

            PrintMugshots(objWriter);

            objWriter.WriteEndElement();
        }
        #endregion

        #region Properties

        public bool ReadOnly
        {
            get => _readonly;
            set => _readonly = value;
        }


        /// <summary>
        /// Total points used for this contact.
        /// </summary>
        public int ContactPoints
        {
            get
            {
                if (Free) return 0;
                int intReturn = 0;
                if (Family) intReturn += 1;
                if (Blackmail) intReturn += 2;
                intReturn += Connection + Loyalty;
                return intReturn;
            }
        }

        /// <summary>
        /// Name of the Contact.
        /// </summary>
        public string Name
        {
            get
            {
                if (LinkedCharacter != null)
                    return LinkedCharacter.CharacterName;
                return _strName;
            }
            set => _strName = value;
        }

        public string DisplayRoleMethod(string strLanguage)
        {
            if (strLanguage == GlobalOptions.DefaultLanguage)
                return Role;

            return XmlManager.Load("contacts.xml", strLanguage).SelectSingleNode("/chummer/contacts/contact[text() = \"" + Role + "\"]/@translate")?.InnerText ?? Role;
        }

        public string DisplayRole
        {
            get
            {
                return DisplayRoleMethod(GlobalOptions.Language);
            }
            set
            {
                _strRole = LanguageManager.ReverseTranslateExtra(value, GlobalOptions.Language);
            }
        }

        /// <summary>
        /// Role of the Contact.
        /// </summary>
        public string Role
        {
            get => _strRole;
            set => _strRole = value;
        }

        /// <summary>
        /// Location of the Contact.
        /// </summary>
        public string Location
        {
            get => _strLocation;
            set => _strLocation = value;
        }

        /// <summary>
        /// Contact's Connection Rating.
        /// </summary>
        public int Connection
        {
            get => _intConnection;
            set => _intConnection = value;
        }

        /// <summary>
        /// Contact's Loyalty Rating (or Enemy's Incidence Rating).
        /// </summary>
        public int Loyalty
        {
            get => _intLoyalty;
            set => _intLoyalty = value;
        }

        public string DisplayMetatypeMethod(string strLanguage)
        {
            string strReturn = Metatype;
            if (LinkedCharacter != null)
            {
                // Update character information fields.
                XmlDocument objMetatypeDoc = XmlManager.Load("metatypes.xml", strLanguage);
                XmlNode objMetatypeNode = objMetatypeDoc.SelectSingleNode("/chummer/metatypes/metatype[name = \"" + LinkedCharacter.Metatype + "\"]");
                if (objMetatypeNode == null)
                {
                    objMetatypeDoc = XmlManager.Load("critters.xml", strLanguage);
                    objMetatypeNode = objMetatypeDoc.SelectSingleNode("/chummer/metatypes/metatype[name = \"" + LinkedCharacter.Metatype + "\"]");
                }

                strReturn = objMetatypeNode["translate"]?.InnerText ?? LanguageManager.TranslateExtra(LinkedCharacter.Metatype, strLanguage);

                if (!string.IsNullOrEmpty(LinkedCharacter.Metavariant))
                {
                    objMetatypeNode = objMetatypeNode.SelectSingleNode("metavariants/metavariant[name = \"" + LinkedCharacter.Metavariant + "\"]");

                    string strMetatypeTranslate = objMetatypeNode["translate"]?.InnerText;
                    strReturn += !string.IsNullOrEmpty(strMetatypeTranslate) ? " (" + strMetatypeTranslate + ')' : " (" + LanguageManager.TranslateExtra(LinkedCharacter.Metavariant, strLanguage) + ')';
                }
            }
            else
                strReturn = LanguageManager.TranslateExtra(strReturn, strLanguage);
            return strReturn;
        }

        public string DisplayMetatype
        {
            get
            {
                return DisplayMetatypeMethod(GlobalOptions.Language);
            }
            set
            {
                _strMetatype = LanguageManager.ReverseTranslateExtra(value, GlobalOptions.Language);
            }
        }

        /// <summary>
        /// Metatype of this Contact.
        /// </summary>
        public string Metatype
        {
            get
            {
                if (LinkedCharacter != null)
                {
                    string strMetatype = LinkedCharacter.Metatype;

                    if (!string.IsNullOrEmpty(LinkedCharacter.Metavariant))
                    {
                        strMetatype += " (" + LinkedCharacter.Metavariant + ')';
                    }
                    return strMetatype;
                }
                return _strMetatype;
            }
            set
            {
                _strMetatype = value;
            }
        }

        public string DisplaySexMethod(string strLanguage)
        {
            if (strLanguage == GlobalOptions.DefaultLanguage)
                return Sex;

            return XmlManager.Load("contacts.xml", strLanguage).SelectSingleNode("/chummer/sexes/sex[text() = \"" + Sex + "\"]/@translate")?.InnerText ?? Sex;
        }

        public string DisplaySex
        {
            get
            {
                return DisplaySexMethod(GlobalOptions.Language);
            }
            set
            {
                _strSex = LanguageManager.ReverseTranslateExtra(value, GlobalOptions.Language);
            }
        }

        /// <summary>
        /// Gender of this Contact.
        /// </summary>
        public string Sex
        {
            get
            {
                if (LinkedCharacter != null)
                    return LinkedCharacter.Sex;
                return _strSex;
            }
            set
            {
                _strSex = value;
            }
        }

        public string DisplayAgeMethod(string strLanguage)
        {
            if (strLanguage == GlobalOptions.DefaultLanguage)
                return Age;

            return XmlManager.Load("contacts.xml", strLanguage).SelectSingleNode("/chummer/ages/age[text() = \"" + Age + "\"]/@translate")?.InnerText ?? Age;
        }

        public string DisplayAge
        {
            get
            {
                return DisplayAgeMethod(GlobalOptions.Language);
            }
            set
            {
                _strAge = LanguageManager.ReverseTranslateExtra(value, GlobalOptions.Language);
            }
        }

        /// <summary>
        /// How old is this Contact.
        /// </summary>
        public string Age
        {
            get
            {
                if (LinkedCharacter != null)
                    return LinkedCharacter.Age;
                return _strAge;
            }
            set
            {
                _strAge = value;
            }
        }

        public string DisplayTypeMethod(string strLanguage)
        {
            if (strLanguage == GlobalOptions.DefaultLanguage)
                return Type;

            return XmlManager.Load("contacts.xml", strLanguage).SelectSingleNode("/chummer/types/type[text() = \"" + Type + "\"]/@translate")?.InnerText ?? Type;
        }

        public string DisplayType
        {
            get
            {
                return DisplayTypeMethod(GlobalOptions.Language);
            }
            set
            {
                _strType = LanguageManager.ReverseTranslateExtra(value, GlobalOptions.Language);
            }
        }

        /// <summary>
        /// What type of Contact is this.
        /// </summary>
        public string Type
        {
            get
            {
                return _strType;
            }
            set
            {
                _strType = value;
            }
        }

        public string DisplayPreferredPaymentMethod(string strLanguage)
        {
            if (strLanguage == GlobalOptions.DefaultLanguage)
                return PreferredPayment;

            return XmlManager.Load("contacts.xml", strLanguage).SelectSingleNode("/chummer/preferredpayments/preferredpayment[text() = \"" + PreferredPayment + "\"]/@translate")?.InnerText ?? PreferredPayment;
        }

        public string DisplayPreferredPayment
        {
            get
            {
                return DisplayPreferredPaymentMethod(GlobalOptions.Language);
            }
            set
            {
                _strPreferredPayment = LanguageManager.ReverseTranslateExtra(value, GlobalOptions.Language);
            }
        }

        /// <summary>
        /// Preferred payment method of this Contact.
        /// </summary>
        public string PreferredPayment
        {
            get
            {
                return _strPreferredPayment;
            }
            set
            {
                _strPreferredPayment = value;
            }
        }

        public string DisplayHobbiesViceMethod(string strLanguage)
        {
            if (strLanguage == GlobalOptions.DefaultLanguage)
                return HobbiesVice;

            return XmlManager.Load("contacts.xml", strLanguage).SelectSingleNode("/chummer/hobbiesvices/hobbyvice[text() = \"" + HobbiesVice + "\"]/@translate")?.InnerText ?? HobbiesVice;
        }

        public string DisplayHobbiesVice
        {
            get
            {
                return DisplayHobbiesViceMethod(GlobalOptions.Language);
            }
            set
            {
                _strHobbiesVice = LanguageManager.ReverseTranslateExtra(value, GlobalOptions.Language);
            }
        }

        /// <summary>
        /// Hobbies/Vice of this Contact.
        /// </summary>
        public string HobbiesVice
        {
            get
            {
                return _strHobbiesVice;
            }
            set
            {
                _strHobbiesVice = value;
            }
        }

        public string DisplayPersonalLifeMethod(string strLanguage)
        {
            if (strLanguage == GlobalOptions.DefaultLanguage)
                return PersonalLife;

            return XmlManager.Load("contacts.xml", strLanguage).SelectSingleNode("/chummer/personallives/personallife[text() = \"" + PersonalLife + "\"]/@translate")?.InnerText ?? PersonalLife;
        }

        public string DisplayPersonalLife
        {
            get
            {
                return DisplayPersonalLifeMethod(GlobalOptions.Language);
            }
            set
            {
                _strPersonalLife = LanguageManager.ReverseTranslateExtra(value, GlobalOptions.Language);
            }
        }

        /// <summary>
        /// Personal Life of this Contact.
        /// </summary>
        public string PersonalLife
        {
            get
            {
                return _strPersonalLife;
            }
            set
            {
                _strPersonalLife = value;
            }
        }

        /// <summary>
        /// Is this contact a group contact?
        /// </summary>
        public bool IsGroup
        {
            get => _blnIsGroup;
            set
            {
                _blnIsGroup = value;

                if (value && !ForceLoyalty)
                {
                    _intLoyalty = 1;
                }
            }
        }

        public bool IsGroupOrMadeMan
        {
            get => IsGroup || MadeMan;
            set => IsGroup = value;
        }

        public bool LoyaltyEnabled => !IsGroup && !ForceLoyalty;

        public int ConnectionMaximum => !_objCharacter.Created ? (_objCharacter.FriendsInHighPlaces ? 12 : 6) : 12;

        public string QuickText => $"({Connection}/{(IsGroup ? $"{Loyalty}G" : Loyalty.ToString())})";

        /// <summary>
        /// The Contact's type, either Contact or Enemy.
        /// </summary>
        public ContactType EntityType
        {
            get => _eContactType;
            set => _eContactType = value;
        }

        public bool IsNotEnemy
        {
            get
            {
                return EntityType != ContactType.Enemy;
            }
        }

        /// <summary>
        /// Name of the save file for this Contact.
        /// </summary>
        public string FileName
        {
            get => _strFileName;
            set
            {
                if (_strFileName != value)
                {
                    _strFileName = value;
                    RefreshLinkedCharacter(!string.IsNullOrEmpty(value));
                }
            }
        }

        /// <summary>
        /// Relative path to the save file.
        /// </summary>
        public string RelativeFileName
        {
            get => _strRelativeName;
            set
            {
                if (_strRelativeName != value)
                {
                    _strRelativeName = value;
                    RefreshLinkedCharacter(!string.IsNullOrEmpty(value));
                }
            }
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
        /// Group Name.
        /// </summary>
        public string GroupName
        {
            get => _strGroupName;
            set => _strGroupName = value;
        }

        /// <summary>
        /// Contact Colour.
        /// </summary>
        public Color Colour
        {
            get => _objColour;
            set => _objColour = value;
        }

        /// <summary>
        /// Whether or not this is a free contact.
        /// </summary>
        public bool Free
        {
            get => _blnFree;
            set => _blnFree = value;
        }
        /// <summary>
        /// Unique ID for this contact
        /// </summary>
        public string GUID
        {
            get
            {
                return _strUnique;
            }
        }

        /// <summary>
        /// Is this contact a made man
        /// </summary>
        public bool MadeMan
        {
            get => _blnMadeMan;
            set
            {
                _blnMadeMan = value;
                if (value)
                {
                    _intLoyalty = 3;
                }
            }
        }

        public bool NotMadeMan
        {
            get => !MadeMan;
        }

        public bool Blackmail
        {
            get => _blnBlackmail;
            set => _blnBlackmail = value;
        }

        public bool Family
        {
            get => _blnFamily;
            set => _blnFamily = value;
        }

        public bool ForceLoyalty
        {
            get => _blnForceLoyalty;
            set => _blnForceLoyalty = value;
        }

        public Character CharacterObject
        {
            get => _objCharacter;
        }

        public Character LinkedCharacter
        {
            get => _objLinkedCharacter;
        }

        public bool NoLinkedCharacter
        {
            get => _objLinkedCharacter == null;
        }

        public void RefreshForControl()
        {
            PropertyChangedEventHandler objPropertyChanged = PropertyChanged;
            if (objPropertyChanged != null)
            {
                objPropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(Loyalty)));
                objPropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(Connection)));
            }
            RefreshLinkedCharacter(false);
        }

        public void RefreshLinkedCharacter(bool blnShowError)
        {
            Character _objOldLinkedCharacter = _objLinkedCharacter;
            _objCharacter.LinkedCharacters.Remove(_objLinkedCharacter);
            _objLinkedCharacter = null;
            bool blnError = false;
            bool blnUseRelative = false;

            // Make sure the file still exists before attempting to load it.
            if (!File.Exists(FileName))
            {
                // If the file doesn't exist, use the relative path if one is available.
                if (string.IsNullOrEmpty(RelativeFileName))
                    blnError = true;
                else if (!File.Exists(Path.GetFullPath(RelativeFileName)))
                    blnError = true;
                else
                    blnUseRelative = true;

                if (blnError && blnShowError)
                {
                    MessageBox.Show(LanguageManager.GetString("Message_FileNotFound", GlobalOptions.Language).Replace("{0}", FileName), LanguageManager.GetString("MessageTitle_FileNotFound", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            if (!blnError)
            {
                string strFile = blnUseRelative ? Path.GetFullPath(RelativeFileName) : FileName;
                if (strFile.EndsWith(".chum5"))
                {
                    Character objOpenCharacter = Program.MainForm.OpenCharacters.FirstOrDefault(x => x.FileName == strFile);
                    if (objOpenCharacter != null)
                        _objLinkedCharacter = objOpenCharacter;
                    else
                        _objLinkedCharacter = Program.MainForm.LoadCharacter(strFile, string.Empty, false, false);
                    if (_objLinkedCharacter != null)
                        _objCharacter.LinkedCharacters.Add(_objLinkedCharacter);
                }
            }
            if (_objLinkedCharacter != _objOldLinkedCharacter)
            {
                if (_objOldLinkedCharacter != null)
                {
                    if (!Program.MainForm.OpenCharacters.Any(x => x.LinkedCharacters.Contains(_objOldLinkedCharacter) && x != _objOldLinkedCharacter))
                    {
                        Program.MainForm.OpenCharacters.Remove(_objOldLinkedCharacter);
                        _objOldLinkedCharacter.Dispose();
                    }
                }
                if (_objLinkedCharacter != null)
                {
                    if (string.IsNullOrEmpty(_strName) && Name != LanguageManager.GetString("String_UnnamedCharacter", GlobalOptions.Language))
                        _strName = Name;
                    if (string.IsNullOrEmpty(_strAge) && !string.IsNullOrEmpty(Age))
                        _strAge = Age;
                    if (string.IsNullOrEmpty(_strSex) && !string.IsNullOrEmpty(Sex))
                        _strSex = Sex;
                    if (string.IsNullOrEmpty(_strMetatype) && !string.IsNullOrEmpty(Metatype))
                        _strMetatype = Metatype;
                }
                PropertyChangedEventHandler objPropertyChanged = PropertyChanged;
                if (objPropertyChanged != null)
                {
                    objPropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
                    objPropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(Age)));
                    objPropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(Sex)));
                    objPropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(Metatype)));
                    objPropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(NoLinkedCharacter)));
                }
            }
        }
        #endregion

        #region IHasMugshots
        /// <summary>
		/// Character's portraits encoded using Base64.
		/// </summary>
		public IList<Image> Mugshots
        {
            get
            {
                if (LinkedCharacter != null)
                    return LinkedCharacter.Mugshots;
                else
                    return _lstMugshots;
            }
        }

        /// <summary>
        /// Character's main portrait encoded using Base64.
        /// </summary>
        public Image MainMugshot
        {
            get
            {
                if (LinkedCharacter != null)
                    return LinkedCharacter.MainMugshot;
                else if (MainMugshotIndex >= Mugshots.Count || MainMugshotIndex < 0)
                    return null;
                else
                    return Mugshots[MainMugshotIndex];
            }
            set
            {
                if (LinkedCharacter != null)
                    LinkedCharacter.MainMugshot = value;
                else
                {
                    if (value == null)
                    {
                        MainMugshotIndex = -1;
                        return;
                    }
                    int intNewMainMugshotIndex = Mugshots.IndexOf(value);
                    if (intNewMainMugshotIndex != -1)
                    {
                        MainMugshotIndex = intNewMainMugshotIndex;
                    }
                    else
                    {
                        Mugshots.Add(value);
                        MainMugshotIndex = Mugshots.Count - 1;
                    }
                }
            }
        }

        /// <summary>
        /// Index of Character's main portrait. -1 if set to none.
        /// </summary>
        public int MainMugshotIndex
        {
            get
            {
                if (LinkedCharacter != null)
                    return LinkedCharacter.MainMugshotIndex;
                else
                    return _intMainMugshotIndex;
            }
            set
            {
                if (LinkedCharacter != null)
                    LinkedCharacter.MainMugshotIndex = value;
                else if (value >= _lstMugshots.Count || value < -1)
                    _intMainMugshotIndex = -1;
                else
                    _intMainMugshotIndex = value;
            }
        }

        public void SaveMugshots(XmlTextWriter objWriter)
        {
            objWriter.WriteElementString("mainmugshotindex", MainMugshotIndex.ToString());
            // <mugshot>
            objWriter.WriteStartElement("mugshots");
            foreach (Image imgMugshot in Mugshots)
            {
                objWriter.WriteElementString("mugshot", imgMugshot.ToBase64String());
            }
            // </mugshot>
            objWriter.WriteEndElement();
        }

        public void LoadMugshots(XmlNode xmlSavedNode)
        {
            xmlSavedNode.TryGetInt32FieldQuickly("mainmugshotindex", ref _intMainMugshotIndex);
            XmlNodeList objXmlMugshotsList = xmlSavedNode.SelectNodes("mugshots/mugshot");
            if (objXmlMugshotsList != null)
            {
                List<string> lstMugshotsBase64 = new List<string>(objXmlMugshotsList.Count);
                foreach (XmlNode objXmlMugshot in objXmlMugshotsList)
                {
                    string strMugshot = objXmlMugshot.InnerText;
                    if (!string.IsNullOrWhiteSpace(strMugshot))
                    {
                        lstMugshotsBase64.Add(strMugshot);
                    }
                }
                if (lstMugshotsBase64.Count > 1)
                {
                    Image[] objMugshotImages = new Image[lstMugshotsBase64.Count];
                    Parallel.For(0, lstMugshotsBase64.Count, i =>
                    {
                        objMugshotImages[i] = lstMugshotsBase64[i].ToImage(System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
                    });
                    _lstMugshots.AddRange(objMugshotImages);
                }
                else if (lstMugshotsBase64.Count == 1)
                {
                    _lstMugshots.Add(lstMugshotsBase64[0].ToImage(System.Drawing.Imaging.PixelFormat.Format32bppPArgb));
                }
            }
        }

        public void PrintMugshots(XmlTextWriter objWriter)
        {
            if (LinkedCharacter != null)
                LinkedCharacter.PrintMugshots(objWriter);
            else if (Mugshots.Count > 0)
            {
                // Since IE is retarded and can't handle base64 images before IE9, we need to dump the image to a temporary directory and re-write the information.
                // If you give it an extension of jpg, gif, or png, it expects the file to be in that format and won't render the image unless it was originally that type.
                // But if you give it the extension img, it will render whatever you give it (which doesn't make any damn sense, but that's IE for you).
                string strMugshotsDirectoryPath = Path.Combine(Application.StartupPath, "mugshots");
                if (!Directory.Exists(strMugshotsDirectoryPath))
                {
                    try
                    {
                        Directory.CreateDirectory(strMugshotsDirectoryPath);
                    }
                    catch (UnauthorizedAccessException)
                    {
                        MessageBox.Show(LanguageManager.GetString("Message_Insufficient_Permissions_Warning", GlobalOptions.Language));
                    }
                }
                Guid guiImage = Guid.NewGuid();
                string imgMugshotPath = Path.Combine(strMugshotsDirectoryPath, guiImage.ToString("N") + ".img");
                Image imgMainMugshot = MainMugshot;
                if (imgMainMugshot != null)
                {
                    imgMainMugshot.Save(imgMugshotPath);
                    // <mainmugshotpath />
                    objWriter.WriteElementString("mainmugshotpath", "file://" + imgMugshotPath.Replace(Path.DirectorySeparatorChar, '/'));
                    // <mainmugshotbase64 />
                    objWriter.WriteElementString("mainmugshotbase64", imgMainMugshot.ToBase64String());
                }
                // <othermugshots>
                objWriter.WriteElementString("hasothermugshots", (imgMainMugshot == null || Mugshots.Count > 1).ToString());
                objWriter.WriteStartElement("othermugshots");
                for (int i = 0; i < Mugshots.Count; ++i)
                {
                    if (i == MainMugshotIndex)
                        continue;
                    Image imgMugshot = Mugshots[i];
                    objWriter.WriteStartElement("mugshot");

                    objWriter.WriteElementString("stringbase64", imgMugshot.ToBase64String());

                    imgMugshotPath = Path.Combine(strMugshotsDirectoryPath, guiImage.ToString("N") + i.ToString() + ".img");
                    imgMugshot.Save(imgMugshotPath);
                    objWriter.WriteElementString("temppath", "file://" + imgMugshotPath.Replace(Path.DirectorySeparatorChar, '/'));

                    objWriter.WriteEndElement();
                }
                // </mugshots>
                objWriter.WriteEndElement();
            }
        }
        #endregion
    }

    /// <summary>
    /// A Critter Power.
    /// </summary>
    public class CritterPower : IHasInternalId, IHasName, IHasXmlNode
    {
        private Guid _guiID;
        private string _strName = string.Empty;
        private string _strCategory = string.Empty;
        private string _strType = string.Empty;
        private string _strAction = string.Empty;
        private string _strRange = string.Empty;
        private string _strDuration = string.Empty;
        private string _strExtra = string.Empty;
        private string _strSource = string.Empty;
        private string _strPage = string.Empty;
        private int _intKarma;
        private decimal _decPowerPoints = 0.0m;
        private XmlNode _nodBonus;
        private string _strNotes = string.Empty;
        private readonly Character _objCharacter;
        private bool _blnCountTowardsLimit = true;
        private int _intRating;
        private int _intGrade;

        #region Constructor, Create, Save, Load, and Print Methods
        public CritterPower(Character objCharacter)
        {
            // Create the GUID for the new Power.
            _guiID = Guid.NewGuid();
            _objCharacter = objCharacter;
        }

        /// Create a Critter Power from an XmlNode.
        /// <param name="objXmlPowerNode">XmlNode to create the object from.</param>
        /// <param name="intRating">Selected Rating for the Gear.</param>
        /// <param name="strForcedValue">Value to forcefully select for any ImprovementManager prompts.</param>
        public void Create(XmlNode objXmlPowerNode, int intRating = 0, string strForcedValue = "")
        {
            if (objXmlPowerNode.TryGetStringFieldQuickly("name", ref _strName))
                _objCachedMyXmlNode = null;
            _intRating = intRating;
            _nodBonus = objXmlPowerNode["bonus"];
            if (!objXmlPowerNode.TryGetStringFieldQuickly("altnotes", ref _strNotes))
                objXmlPowerNode.TryGetStringFieldQuickly("notes", ref _strNotes);
            // If the piece grants a bonus, pass the information to the Improvement Manager.
            if (_nodBonus != null)
            {
                ImprovementManager.ForcedValue = strForcedValue;
                if (!ImprovementManager.CreateImprovements(_objCharacter, Improvement.ImprovementSource.CritterPower, _guiID.ToString("D"), _nodBonus, true, intRating, DisplayNameShort(GlobalOptions.Language)))
                {
                    _guiID = Guid.Empty;
                    return;
                }
                if (!string.IsNullOrEmpty(ImprovementManager.SelectedValue))
                {
                    _strExtra = ImprovementManager.SelectedValue;
                }
                else if (intRating != 0)
                    _strExtra = intRating.ToString();
            }
            else if (intRating != 0)
                _strExtra = intRating.ToString();
            else
                _strExtra = strForcedValue;
            objXmlPowerNode.TryGetStringFieldQuickly("category", ref _strCategory);
            objXmlPowerNode.TryGetStringFieldQuickly("type", ref _strType);
            objXmlPowerNode.TryGetStringFieldQuickly("action", ref _strAction);
            objXmlPowerNode.TryGetStringFieldQuickly("range", ref _strRange);
            objXmlPowerNode.TryGetStringFieldQuickly("duration", ref _strDuration);
            objXmlPowerNode.TryGetStringFieldQuickly("source", ref _strSource);
            objXmlPowerNode.TryGetStringFieldQuickly("page", ref _strPage);
            objXmlPowerNode.TryGetInt32FieldQuickly("karma", ref _intKarma);

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
            objWriter.WriteStartElement("critterpower");
            objWriter.WriteElementString("guid", _guiID.ToString("D"));
            objWriter.WriteElementString("name", _strName);
            objWriter.WriteElementString("extra", _strExtra);
            objWriter.WriteElementString("rating", _intRating.ToString(CultureInfo.InvariantCulture));
            objWriter.WriteElementString("category", _strCategory);
            objWriter.WriteElementString("type", _strType);
            objWriter.WriteElementString("action", _strAction);
            objWriter.WriteElementString("range", _strRange);
            objWriter.WriteElementString("duration", _strDuration);
            objWriter.WriteElementString("grade", _intGrade.ToString(CultureInfo.InvariantCulture));
            objWriter.WriteElementString("source", _strSource);
            objWriter.WriteElementString("page", _strPage);
            objWriter.WriteElementString("karma", _intKarma.ToString(CultureInfo.InvariantCulture));
            objWriter.WriteElementString("points", _decPowerPoints.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("counttowardslimit", _blnCountTowardsLimit.ToString());
            if (_nodBonus != null)
                objWriter.WriteRaw("<bonus>" + _nodBonus.InnerXml + "</bonus>");
            else
                objWriter.WriteElementString("bonus", string.Empty);
            objWriter.WriteElementString("notes", _strNotes);
            objWriter.WriteEndElement();
            _objCharacter.SourceProcess(_strSource);
        }

        /// <summary>
        /// Load the Critter Power from the XmlNode.
        /// </summary>
        /// <param name="objNode">XmlNode to load.</param>
        public void Load(XmlNode objNode)
        {
            objNode.TryGetField("guid", Guid.TryParse, out _guiID);
            if (objNode.TryGetStringFieldQuickly("name", ref _strName))
                _objCachedMyXmlNode = null;
            objNode.TryGetStringFieldQuickly("extra", ref _strExtra);
            objNode.TryGetStringFieldQuickly("category", ref _strCategory);
            objNode.TryGetStringFieldQuickly("type", ref _strType);
            objNode.TryGetStringFieldQuickly("action", ref _strAction);
            objNode.TryGetStringFieldQuickly("range", ref _strRange);
            objNode.TryGetStringFieldQuickly("duration", ref _strDuration);
            objNode.TryGetStringFieldQuickly("source", ref _strSource);
            objNode.TryGetStringFieldQuickly("page", ref _strPage);
            objNode.TryGetDecFieldQuickly("points", ref _decPowerPoints);
            objNode.TryGetBoolFieldQuickly("counttowardslimit", ref _blnCountTowardsLimit);
            objNode.TryGetInt32FieldQuickly("grade", ref _intGrade);
            _nodBonus = objNode["bonus"];
            objNode.TryGetStringFieldQuickly("notes", ref _strNotes);
        }

        /// <summary>
        /// Print the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        public void Print(XmlTextWriter objWriter, string strLanguageToPrint)
        {
            objWriter.WriteStartElement("critterpower");
            objWriter.WriteElementString("name", DisplayNameShort(strLanguageToPrint));
            objWriter.WriteElementString("name_english", Name);
            objWriter.WriteElementString("extra", LanguageManager.TranslateExtra(_strExtra, strLanguageToPrint));
            objWriter.WriteElementString("category", DisplayCategory(strLanguageToPrint));
            objWriter.WriteElementString("category_english", Category);
            objWriter.WriteElementString("type", DisplayType(strLanguageToPrint));
            objWriter.WriteElementString("action", DisplayAction(strLanguageToPrint));
            objWriter.WriteElementString("range", DisplayRange(strLanguageToPrint));
            objWriter.WriteElementString("duration", DisplayDuration(strLanguageToPrint));
            objWriter.WriteElementString("source", CommonFunctions.LanguageBookShort(Source, strLanguageToPrint));
            objWriter.WriteElementString("page", Page(strLanguageToPrint));
            if (_objCharacter.Options.PrintNotes)
                objWriter.WriteElementString("notes", Notes);
            objWriter.WriteEndElement();
        }
        #endregion

        #region Properties
        /// <summary>
        /// Internal identifier which will be used to identify this Critter Power in the Improvement system.
        /// </summary>
        public string InternalId => _guiID.ToString("D");

        /// <summary>
        /// Paid levels of the power.
        /// </summary>
        public int Rating
        {
            get => _intRating;
            set
            {
                if (Extra == Rating.ToString())
                {
                    Extra = value.ToString();
                }
                _intRating = value;
            }
        }

        /// <summary>
        /// Total rating of the power, including any bonus levels from Improvements.
        /// </summary>
        public int TotalRating
        {
            get
            {
                return Rating + _objCharacter.Improvements.Where(objImprovement => objImprovement.ImprovedName == Name && objImprovement.ImproveType == Improvement.ImprovementType.CritterPowerLevel && objImprovement.Enabled).Sum(objImprovement => objImprovement.Rating);
            }
        }

        /// <summary>
        /// Power's name.
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
                strReturn += " (" + LanguageManager.TranslateExtra(_strExtra, strLanguage) + ')';
            }

            return strReturn;
        }

        /// <summary>
        /// Extra information that should be applied to the name, like a linked CharacterAttribute.
        /// </summary>
        public string Extra
        {
            get => _strExtra;
            set => _strExtra = LanguageManager.ReverseTranslateExtra(value, GlobalOptions.Language);
        }

        /// <summary>
        /// Grade of the Critter power
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
        /// Page Number.
        /// </summary>
        public string Page(string strLanguage)
        {
            // Get the translated name if applicable.
            if (strLanguage != GlobalOptions.DefaultLanguage)
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
        /// Translated Category.
        /// </summary>
        public string DisplayCategory(string strLanguage)
        {
            // Get the translated name if applicable.
            if (strLanguage == GlobalOptions.DefaultLanguage)
                return Category;

            return XmlManager.Load("critterpowers.xml", strLanguage).SelectSingleNode("/chummer/categories/category[. = \"" + Category + "\"]/@translate")?.InnerText ?? Category;
        }

        /// <summary>
        /// Category.
        /// </summary>
        public string Category
        {
            get => _strCategory;
            set => _strCategory = value;
        }

        /// <summary>
        /// Type.
        /// </summary>
        public string Type
        {
            get => _strType;
            set => _strType = value;
        }

        /// <summary>
        /// Translated Type.
        /// </summary>
        public string DisplayType(string strLanguage)
        {
            string strReturn = Type;

            switch (strReturn)
            {
                case "M":
                    strReturn = LanguageManager.GetString("String_SpellTypeMana", strLanguage);
                    break;
                case "P":
                    strReturn = LanguageManager.GetString("String_SpellTypePhysical", strLanguage);
                    break;
                default:
                    strReturn = LanguageManager.GetString("String_None", strLanguage);
                    break;
            }

            return strReturn;
        }

        /// <summary>
        /// Action.
        /// </summary>
        public string Action
        {
            get => _strAction;
            set => _strAction = value;
        }

        /// <summary>
        /// Translated Action.
        /// </summary>
        public string DisplayAction(string strLanguage)
        {
            string strReturn = Action;

            switch (strReturn)
            {
                case "Auto":
                    strReturn = LanguageManager.GetString("String_ActionAutomatic", strLanguage);
                    break;
                case "Free":
                    strReturn = LanguageManager.GetString("String_ActionFree", strLanguage);
                    break;
                case "Simple":
                    strReturn = LanguageManager.GetString("String_ActionSimple", strLanguage);
                    break;
                case "Complex":
                    strReturn = LanguageManager.GetString("String_ActionComplex", strLanguage);
                    break;
                case "Special":
                    strReturn = LanguageManager.GetString("String_SpellDurationSpecial", strLanguage);
                    break;
                default:
                    strReturn = LanguageManager.GetString("String_None", strLanguage);
                    break;
            }

            return strReturn;
        }

        /// <summary>
        /// Range.
        /// </summary>
        public string Range
        {
            get => _strRange;
            set => _strRange = value;
        }

        /// <summary>
        /// Translated Range.
        /// </summary>
        public string DisplayRange(string strLanguage)
        {
            string strReturn = Range;

            strReturn = strReturn.CheapReplace("Self", () => LanguageManager.GetString("String_SpellRangeSelf", strLanguage));
            strReturn = strReturn.CheapReplace("Special", () => LanguageManager.GetString("String_SpellDurationSpecial", strLanguage));
            strReturn = strReturn.CheapReplace("LOS", () => LanguageManager.GetString("String_SpellRangeLineOfSight", strLanguage));
            strReturn = strReturn.CheapReplace("LOI", () => LanguageManager.GetString("String_SpellRangeLineOfInfluence", strLanguage));
            strReturn = strReturn.CheapReplace("T", () => LanguageManager.GetString("String_SpellRangeTouch", strLanguage));
            strReturn = strReturn.CheapReplace("(A)", () => "(" + LanguageManager.GetString("String_SpellRangeArea", strLanguage) + ')');
            strReturn = strReturn.CheapReplace("MAG", () => LanguageManager.GetString("String_AttributeMAGShort", strLanguage));

            return strReturn;
        }

        /// <summary>
        /// Duration.
        /// </summary>
        public string Duration
        {
            get => _strDuration;
            set => _strDuration = value;
        }

        /// <summary>
        /// Translated Duration.
        /// </summary>
        public string DisplayDuration(string strLanguage)
        {
            string strReturn = Duration;

            switch (strReturn)
            {
                case "Instant":
                    strReturn = LanguageManager.GetString("String_SpellDurationInstantLong", strLanguage);
                    break;
                case "Sustained":
                    strReturn = LanguageManager.GetString("String_SpellDurationSustained", strLanguage);
                    break;
                case "Always":
                    strReturn = LanguageManager.GetString("String_SpellDurationAlways", strLanguage);
                    break;
                case "Special":
                    strReturn = LanguageManager.GetString("String_SpellDurationSpecial", strLanguage);
                    break;
            }

            return strReturn;
        }

        /// <summary>
        /// Power Points used by the Critter Power (Free Spirits only).
        /// </summary>
        public decimal PowerPoints
        {
            get => _decPowerPoints;
            set => _decPowerPoints = value;
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
        /// Whether or not the Critter Power counts towards their total number of Critter Powers.
        /// </summary>
        public bool CountTowardsLimit
        {
            get => _blnCountTowardsLimit;
            set => _blnCountTowardsLimit = value;
        }

        /// <summary>
        /// Karma that the Critter must pay to take the power.
        /// </summary>
        public int Karma
        {
            get => _intKarma;
            set => _intKarma = value;
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
                _objCachedMyXmlNode = XmlManager.Load("critterpowers.xml", strLanguage).SelectSingleNode("/chummer/powers/power[name = \"" + Name + "\"]");
                _strCachedXmlNodeLanguage = strLanguage;
            }
            return _objCachedMyXmlNode;
        }
        #endregion

        #region Methods
        public TreeNode CreateTreeNode(ContextMenuStrip cmsCritterPower)
        {
            TreeNode objNode = new TreeNode
            {
                Name = InternalId,
                ContextMenuStrip = cmsCritterPower,
                Text = DisplayName(GlobalOptions.Language),
                Tag = InternalId
            };
            if (!string.IsNullOrEmpty(Notes))
            {
                objNode.ForeColor = Color.SaddleBrown;
            }
            else if (Grade != 0)
            {
                objNode.ForeColor = SystemColors.GrayText;
            }
            objNode.ToolTipText = Notes.WordWrap(100);
            return objNode;
        }
        #endregion
    }

    /// <summary>
    /// An Initiation Grade.
    /// </summary>
    public class InitiationGrade : IHasInternalId
    {
        private Guid _guiID;
        private bool _blnGroup;
        private bool _blnOrdeal;
        private bool _blnSchooling;
        private bool _blnTechnomancer;
        private int _intGrade;
        private string _strNotes = string.Empty;

        private readonly CharacterOptions _objOptions;

        #region Constructor, Create, Save, and Load Methods
        public InitiationGrade(Character objCharacter)
        {
            // Create the GUID for the new InitiationGrade.
            _guiID = Guid.NewGuid();
            _objOptions = objCharacter.Options;
        }

        /// Create an Intiation Grade from an XmlNode.
        /// <param name="intGrade">Grade number.</param>
        /// <param name="blnTechnomancer">Whether or not the character is a Technomancer.</param>
        /// <param name="blnGroup">Whether or not a Group was used.</param>
        /// <param name="blnOrdeal">Whether or not an Ordeal was used.</param>
        /// <param name="blnSchooling">Whether or not Schooling was used.</param>
        public void Create(int intGrade, bool blnTechnomancer, bool blnGroup, bool blnOrdeal, bool blnSchooling)
        {
            _intGrade = intGrade;
            _blnTechnomancer = blnTechnomancer;
            _blnGroup = blnGroup;
            _blnOrdeal = blnOrdeal;
            _blnSchooling = blnSchooling;
        }

        /// <summary>
        /// Save the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        public void Save(XmlTextWriter objWriter)
        {
            objWriter.WriteStartElement("initiationgrade");
            objWriter.WriteElementString("guid", _guiID.ToString("D"));
            objWriter.WriteElementString("res", _blnTechnomancer.ToString());
            objWriter.WriteElementString("grade", _intGrade.ToString(CultureInfo.InvariantCulture));
            objWriter.WriteElementString("group", _blnGroup.ToString());
            objWriter.WriteElementString("ordeal", _blnOrdeal.ToString());
            objWriter.WriteElementString("schooling", _blnSchooling.ToString());
            objWriter.WriteElementString("notes", _strNotes);
            objWriter.WriteEndElement();
        }

        /// <summary>
        /// Load the Initiation Grade from the XmlNode.
        /// </summary>
        /// <param name="objNode">XmlNode to load.</param>
        public void Load(XmlNode objNode)
        {
            if (objNode.TryGetField("guid", Guid.TryParse, out _guiID))
                _guiID = Guid.NewGuid();
            objNode.TryGetBoolFieldQuickly("res", ref _blnTechnomancer);
            objNode.TryGetInt32FieldQuickly("grade", ref _intGrade);
            objNode.TryGetBoolFieldQuickly("group", ref _blnGroup);
            objNode.TryGetBoolFieldQuickly("ordeal", ref _blnOrdeal);
            objNode.TryGetBoolFieldQuickly("schooling", ref _blnSchooling);
            objNode.TryGetStringFieldQuickly("notes", ref _strNotes);
        }
        #endregion

        #region Properties
        /// <summary>
        /// Internal identifier which will be used to identify this Initiation Grade in the Improvement system.
        /// </summary>
        public string InternalId => _guiID.ToString("D");

        /// <summary>
        /// Initiate Grade.
        /// </summary>
        public int Grade
        {
            get => _intGrade;
            set => _intGrade = value;
        }

        /// <summary>
        /// Whether or not a Group was used.
        /// </summary>
        public bool Group
        {
            get => _blnGroup;
            set => _blnGroup = value;
        }

        /// <summary>
        /// Whether or not an Ordeal was used.
        /// </summary>
        public bool Ordeal
        {
            get => _blnOrdeal;
            set => _blnOrdeal = value;
        }

        /// <summary>
        /// Whether or not Schooling was used.
        /// </summary>
        public bool Schooling
        {
            get => _blnSchooling;
            set => _blnSchooling = value;
        }

        /// <summary>
        /// Whether or not the Initiation Grade is for a Technomancer.
        /// </summary>
        public bool Technomancer
        {
            get => _blnTechnomancer;
            set => _blnTechnomancer = value;
        }
        #endregion

        #region Complex Properties
        /// <summary>
        /// The Initiation Grade's Karma cost.
        /// </summary>
        public int KarmaCost
        {
            get
            {
                decimal decCost = _objOptions.KarmaInititationFlat + (Grade * _objOptions.KarmaInitiation);
                decimal decMultiplier = 1.0m;

                // Discount for Group.
                if (Group)
                    decMultiplier -= 0.1m;

                // Discount for Ordeal.
                if (Ordeal)
                    decMultiplier -= Technomancer ? 0.2m : 0.1m;

                // Discount for Schooling.
                if (Schooling)
                    decMultiplier -= 0.1m;

                return decimal.ToInt32(decimal.Ceiling(decCost * decMultiplier));
            }
        }

        /// <summary>
        /// Text to display in the Initiation Grade list.
        /// </summary>
        public string Text(string strLanguage)
        {
            StringBuilder strReturn = new StringBuilder(LanguageManager.GetString("String_Grade", strLanguage));
            strReturn.Append(' ');
            strReturn.Append(_intGrade.ToString());
            if (Group || Ordeal)
            {
                strReturn.Append(" (");
                if (Group)
                {
                    if (_blnTechnomancer)
                        strReturn.Append(LanguageManager.GetString("String_Network", strLanguage));
                    else
                        strReturn.Append(LanguageManager.GetString("String_Group", strLanguage));
                    if (Ordeal || Schooling)
                        strReturn.Append(", ");
                }
                if (Ordeal)
                {
                    if (Technomancer)
                        strReturn.Append(LanguageManager.GetString("String_Task", strLanguage));
                    else
                        strReturn.Append(LanguageManager.GetString("String_Ordeal", strLanguage));
                    if (Schooling)
                        strReturn.Append(", ");
                }
                if (Schooling)
                {
                    strReturn.Append(LanguageManager.GetString("String_Schooling", strLanguage));
                }
                strReturn.Append(')');
            }

            return strReturn.ToString();
        }

        /// <summary>
        /// Notes.
        /// </summary>
        public string Notes
        {
            get => _strNotes;
            set => _strNotes = value;
        }
        #endregion

        #region Methods
        public TreeNode CreateTreeNode(ContextMenuStrip cmsInitiationGrade)
        {
            TreeNode objNode = new TreeNode
            {
                ContextMenuStrip = cmsInitiationGrade,
                Name = InternalId,
                Text = Text(GlobalOptions.Language),
                Tag = InternalId
            };
            if (!string.IsNullOrEmpty(Notes))
            {
                objNode.ForeColor = Color.SaddleBrown;
            }
            objNode.ToolTipText = Notes.WordWrap(100);
            return objNode;
        }
        #endregion
    }

    public class CalendarWeek : IHasInternalId
    {
        private Guid _guiID;
        private int _intYear = 2072;
        private int _intWeek = 1;
        private string _strNotes = string.Empty;

        #region Constructor, Save, Load, and Print Methods
        public CalendarWeek()
        {
            // Create the GUID for the new CalendarWeek.
            _guiID = Guid.NewGuid();
        }

        public CalendarWeek(int intYear, int intWeek)
        {
            // Create the GUID for the new CalendarWeek.
            _guiID = Guid.NewGuid();
            _intYear = intYear;
            _intWeek = intWeek;
        }

        /// <summary>
        /// Save the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        public void Save(XmlTextWriter objWriter)
        {
            objWriter.WriteStartElement("week");
            objWriter.WriteElementString("guid", _guiID.ToString("D"));
            objWriter.WriteElementString("year", _intYear.ToString(CultureInfo.InvariantCulture));
            objWriter.WriteElementString("week", _intWeek.ToString(CultureInfo.InvariantCulture));
            objWriter.WriteElementString("notes", _strNotes);
            objWriter.WriteEndElement();
        }

        /// <summary>
        /// Load the Calendar Week from the XmlNode.
        /// </summary>
        /// <param name="objNode">XmlNode to load.</param>
        public void Load(XmlNode objNode)
        {
            _guiID = Guid.Parse(objNode["guid"].InnerText);
            _intYear = Convert.ToInt32(objNode["year"].InnerText);
            _intWeek = Convert.ToInt32(objNode["week"].InnerText);
            _strNotes = objNode["notes"].InnerText;
        }

        /// <summary>
        /// Print the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        public void Print(XmlTextWriter objWriter, CultureInfo objCulture, bool blnPrintNotes = true)
        {
            objWriter.WriteStartElement("week");
            objWriter.WriteElementString("year", Year.ToString(objCulture));
            objWriter.WriteElementString("month", Month.ToString(objCulture));
            objWriter.WriteElementString("week", MonthWeek.ToString(objCulture));
            if (blnPrintNotes)
                objWriter.WriteElementString("notes", Notes);
            objWriter.WriteEndElement();
        }
        #endregion

        #region Properties
        /// <summary>
        /// Internal identifier which will be used to identify this Calendar Week in the Improvement system.
        /// </summary>
        public string InternalId => _guiID.ToString("D");

        /// <summary>
        /// Year.
        /// </summary>
        public int Year
        {
            get => _intYear;
            set => _intYear = value;
        }

        /// <summary>
        /// Month.
        /// </summary>
        public int Month
        {
            get
            {
                switch (_intWeek)
                {
                    case 1:
                    case 2:
                    case 3:
                    case 4:
                        return 1;
                    case 5:
                    case 6:
                    case 7:
                    case 8:
                        return 2;
                    case 9:
                    case 10:
                    case 11:
                    case 12:
                    case 13:
                        return 3;
                    case 14:
                    case 15:
                    case 16:
                    case 17:
                        return 4;
                    case 18:
                    case 19:
                    case 20:
                    case 21:
                        return 5;
                    case 22:
                    case 23:
                    case 24:
                    case 25:
                    case 26:
                        return 6;
                    case 27:
                    case 28:
                    case 29:
                    case 30:
                        return 7;
                    case 31:
                    case 32:
                    case 33:
                    case 34:
                        return 8;
                    case 35:
                    case 36:
                    case 37:
                    case 38:
                    case 39:
                        return 9;
                    case 40:
                    case 41:
                    case 42:
                    case 43:
                        return 10;
                    case 44:
                    case 45:
                    case 46:
                    case 47:
                        return 11;
                    default:
                        return 12;
                }
            }
        }

        /// <summary>
        /// Week of the month.
        /// </summary>
        public int MonthWeek
        {
            get
            {
                switch (_intWeek)
                {
                    case 1:
                    case 5:
                    case 9:
                    case 14:
                    case 18:
                    case 22:
                    case 27:
                    case 31:
                    case 35:
                    case 40:
                    case 44:
                    case 48:
                        return 1;
                    case 2:
                    case 6:
                    case 10:
                    case 15:
                    case 19:
                    case 23:
                    case 28:
                    case 32:
                    case 36:
                    case 41:
                    case 45:
                    case 49:
                        return 2;
                    case 3:
                    case 7:
                    case 11:
                    case 16:
                    case 20:
                    case 24:
                    case 29:
                    case 33:
                    case 37:
                    case 42:
                    case 46:
                    case 50:
                        return 3;
                    case 4:
                    case 8:
                    case 12:
                    case 17:
                    case 21:
                    case 25:
                    case 30:
                    case 34:
                    case 38:
                    case 43:
                    case 47:
                    case 51:
                        return 4;
                    default:
                        return 5;
                }
            }
        }

        /// <summary>
        /// Month and Week to display.
        /// </summary>
        public string DisplayName(string strLanguage)
        {
            string strReturn = LanguageManager.GetString("String_WeekDisplay", strLanguage).Replace("{0}", _intYear.ToString()).Replace("{1}", Month.ToString()).Replace("{2}", MonthWeek.ToString());
            return strReturn;
        }

        /// <summary>
        /// Week.
        /// </summary>
        public int Week
        {
            get => _intWeek;
            set => _intWeek = value;
        }

        /// <summary>
        /// Notes.
        /// </summary>
        public string Notes
        {
            get => _strNotes;
            set => _strNotes = value;
        }
        #endregion
    }

    public class MentorSpirit : IHasInternalId, IHasName, IHasXmlNode
    {
        private Guid _guiID;
        private string _strName = string.Empty;
        private string _strAdvantage = string.Empty;
        private string _strDisadvantage = string.Empty;
        private string _strExtra = string.Empty;
        private string _strSource = string.Empty;
        private string _strPage = string.Empty;
        private string _strNotes = string.Empty;
        private XmlNode _nodBonus = null;
        private XmlNode _nodChoice1 = null;
        private XmlNode _nodChoice2 = null;
        private Improvement.ImprovementType _eMentorType;
        private Guid _sourceID;
        private readonly Character _objCharacter;
        private bool _blnMentorMask = false;

        #region Constructor
        public MentorSpirit(Character objCharacter)
        {
            // Create the GUID for the new Mentor Spirit.
            _guiID = Guid.NewGuid();
            _objCharacter = objCharacter;
        }

        /// <summary>
        /// Create a Mentor Spirit from an XmlNode.
        /// </summary>
        /// <param name="objXmlMentor">XmlNode to create the object from.</param>
        /// <param name="eMentorType">Whether this is a Mentor or a Paragon.</param>
        /// <param name="objXmlChoice1">Bonus node from Choice 1.</param>
        /// <param name="objXmlChoice2">Bonus node from Choice 2.</param>
        /// <param name="strForceValueChoice1">Name/Text for Choice 1.</param>
        /// <param name="strForceValueChoice2">Name/Text for Choice 2.</param>
        /// <param name="strForceValue">Force a value to be selected for the Mentor Spirit.</param>
        /// <param name="blnMentorMask">Whether the Mentor's Mask is enabled.</param>
        public void Create(XmlNode objXmlMentor, Improvement.ImprovementType eMentorType, XmlNode objXmlChoice1, XmlNode objXmlChoice2, string strForceValue = "", string strForceValueChoice1 = "", string strForceValueChoice2 = "", bool blnMentorMask = false)
        {
            _blnMentorMask = blnMentorMask;
            _eMentorType = eMentorType;
            _objCachedMyXmlNode = null;
            objXmlMentor.TryGetStringFieldQuickly("name", ref _strName);
            objXmlMentor.TryGetStringFieldQuickly("source", ref _strSource);
            objXmlMentor.TryGetStringFieldQuickly("page", ref _strPage);
            if (!objXmlMentor.TryGetStringFieldQuickly("altnotes", ref _strNotes))
                objXmlMentor.TryGetStringFieldQuickly("notes", ref _strNotes);
            if (objXmlMentor["id"] != null)
                _sourceID = Guid.Parse(objXmlMentor["id"].InnerText);

            // Build the list of advantages gained through the Mentor Spirit.
            if (!objXmlMentor.TryGetStringFieldQuickly("altadvantage", ref _strAdvantage))
            {
                objXmlMentor.TryGetStringFieldQuickly("advantage", ref _strAdvantage);
            }
            if (!objXmlMentor.TryGetStringFieldQuickly("altdisadvantage", ref _strDisadvantage))
            {
                objXmlMentor.TryGetStringFieldQuickly("disadvantage", ref _strDisadvantage);
            }

            _nodBonus = objXmlMentor["bonus"];
            if (_nodBonus != null)
            {
                string strOldForce = ImprovementManager.ForcedValue;
                string strOldSelected = ImprovementManager.SelectedValue;
                ImprovementManager.ForcedValue = strForceValue;
                if (!ImprovementManager.CreateImprovements(_objCharacter, Improvement.ImprovementSource.MentorSpirit, _guiID.ToString("D"), _nodBonus, false, 1, DisplayNameShort(GlobalOptions.Language)))
                {
                    _guiID = Guid.Empty;
                    return;
                }
                _strExtra = ImprovementManager.SelectedValue;
                ImprovementManager.ForcedValue = strOldForce;
                ImprovementManager.SelectedValue = strOldSelected;
            }
            else if (!string.IsNullOrEmpty(strForceValue))
            {
                _strExtra = strForceValue;
            }
            _nodChoice1 = objXmlChoice1;
            if (_nodChoice1 != null)
            {
                string strOldForce = ImprovementManager.ForcedValue;
                string strOldSelected = ImprovementManager.SelectedValue;
                ImprovementManager.ForcedValue = strForceValueChoice1;
                if (!ImprovementManager.CreateImprovements(_objCharacter, Improvement.ImprovementSource.MentorSpirit, _guiID.ToString("D"), _nodChoice1, false, 1, DisplayNameShort(GlobalOptions.Language)))
                {
                    _guiID = Guid.Empty;
                    return;
                }
                if (string.IsNullOrEmpty(_strExtra))
                {
                    _strExtra = ImprovementManager.SelectedValue;
                }
                ImprovementManager.ForcedValue = strOldForce;
                ImprovementManager.SelectedValue = strOldSelected;
            }
            else if (string.IsNullOrEmpty(_strExtra) && !string.IsNullOrEmpty(strForceValueChoice1))
            {
                _strExtra = strForceValueChoice1;
            }
            _nodChoice2 = objXmlChoice2;
            if (_nodChoice2 != null)
            {
                string strOldForce = ImprovementManager.ForcedValue;
                string strOldSelected = ImprovementManager.SelectedValue;
                ImprovementManager.ForcedValue = strForceValueChoice2;
                if (!ImprovementManager.CreateImprovements(_objCharacter, Improvement.ImprovementSource.MentorSpirit, _guiID.ToString("D"), _nodChoice2, false, 1, DisplayNameShort(GlobalOptions.Language)))
                {
                    _guiID = Guid.Empty;
                    return;
                }
                if (string.IsNullOrEmpty(_strExtra))
                {
                    _strExtra = ImprovementManager.SelectedValue;
                }
                ImprovementManager.ForcedValue = strOldForce;
                ImprovementManager.SelectedValue = strOldSelected;
            }
            else if (string.IsNullOrEmpty(_strExtra) && !string.IsNullOrEmpty(strForceValueChoice2))
            {
                _strExtra = strForceValueChoice2;
            }
            if (_blnMentorMask)
            {
                ImprovementManager.CreateImprovement(_objCharacter, string.Empty, Improvement.ImprovementSource.MentorSpirit, _guiID.ToString("D"), Improvement.ImprovementType.AdeptPowerPoints, string.Empty, 1);
                ImprovementManager.CreateImprovement(_objCharacter, string.Empty, Improvement.ImprovementSource.MentorSpirit, _guiID.ToString("D"), Improvement.ImprovementType.DrainValue, string.Empty, -1);
                ImprovementManager.Commit(_objCharacter);
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
            objWriter.WriteStartElement("mentorspirit");
            objWriter.WriteElementString("guid", _guiID.ToString("D"));
            objWriter.WriteElementString("name", _strName);
            objWriter.WriteElementString("mentortype", _eMentorType.ToString());
            objWriter.WriteElementString("extra", _strExtra);
            objWriter.WriteElementString("source", _strSource);
            objWriter.WriteElementString("page", _strPage);
            objWriter.WriteElementString("advantage", _strAdvantage);
            objWriter.WriteElementString("disadvantage", _strDisadvantage);
            objWriter.WriteElementString("mentormask", _blnMentorMask.ToString());
            if (_nodBonus != null)
                objWriter.WriteRaw("<bonus>" + _nodBonus.InnerXml + "</bonus>");
            else
                objWriter.WriteElementString("bonus", string.Empty);
            if (_nodChoice1 != null)
                objWriter.WriteRaw("<choice1>" + _nodChoice1.InnerXml + "</choice1>");
            else
                objWriter.WriteElementString("choice1", string.Empty);
            if (_nodChoice2 != null)
                objWriter.WriteRaw("<choice2>" + _nodChoice2.InnerXml + "</choice2>");
            else
                objWriter.WriteElementString("choice2", string.Empty);
            objWriter.WriteElementString("notes", _strNotes);

            if (!string.IsNullOrEmpty(SourceID))
            {
                objWriter.WriteElementString("id", SourceID);
            }

            objWriter.WriteEndElement();
            _objCharacter.SourceProcess(_strSource);
        }

        /// <summary>
        /// Load the Mentor Spirit from the XmlNode.
        /// </summary>
        /// <param name="objNode">XmlNode to load.</param>
        public void Load(XmlNode objNode)
        {
            objNode.TryGetField("guid", Guid.TryParse, out _guiID);
            if (objNode.TryGetStringFieldQuickly("name", ref _strName))
                _objCachedMyXmlNode = null;
            if (objNode["mentortype"] != null)
            {
                _eMentorType = Improvement.ConvertToImprovementType(objNode["mentortype"].InnerText);
                _objCachedMyXmlNode = null;
            }
            objNode.TryGetStringFieldQuickly("extra", ref _strExtra);
            objNode.TryGetStringFieldQuickly("source", ref _strSource);
            objNode.TryGetStringFieldQuickly("page", ref _strPage);
            objNode.TryGetStringFieldQuickly("advantage", ref _strAdvantage);
            objNode.TryGetStringFieldQuickly("disadvantage", ref _strDisadvantage);
            objNode.TryGetBoolFieldQuickly("mentormask", ref _blnMentorMask);
            _nodBonus = objNode["bonus"];
            _nodChoice1 = objNode["choice1"];
            _nodChoice2 = objNode["choice2"];
            objNode.TryGetStringFieldQuickly("notes", ref _strNotes);

            if (!objNode.TryGetField("id", Guid.TryParse, out _sourceID))
            {
                XmlNode objNewNode = XmlManager.Load("qualities.xml").SelectSingleNode("/chummer/mentors/mentor[name = \"" + Name + "\"]");
                if (objNewNode != null)
                    objNewNode.TryGetField("id", Guid.TryParse, out _sourceID);
            }
        }

        /// <summary>
        /// Print the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        public void Print(XmlTextWriter objWriter, int intRating, string strLanguageToPrint)
        {
            objWriter.WriteStartElement("mentorspirit");
            objWriter.WriteElementString("name", DisplayNameShort(strLanguageToPrint));
            objWriter.WriteElementString("mentortype", _eMentorType.ToString());
            objWriter.WriteElementString("name_english", Name);
            objWriter.WriteElementString("advantage", Advantage);
            objWriter.WriteElementString("disadvantage", Disadvantage);
            objWriter.WriteElementString("extra", LanguageManager.TranslateExtra(_strExtra, strLanguageToPrint));
            objWriter.WriteElementString("source", CommonFunctions.LanguageBookShort(Source, strLanguageToPrint));
            objWriter.WriteElementString("page", Page(strLanguageToPrint));
            objWriter.WriteElementString("mentormask", MentorMask.ToString());
            if (_objCharacter.Options.PrintNotes)
                objWriter.WriteElementString("notes", _strNotes);
            objWriter.WriteEndElement();
        }
        #endregion

        #region Properties
        /// <summary>
        /// Name of the Mentor Spirit or Paragon.
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
        /// Whether the Mentor Spirit is taken with a Mentor Mask.
        /// </summary>
        public bool MentorMask
        {
            get => _blnMentorMask;
            set => _blnMentorMask = value;
        }

        /// <summary>
        /// Advantage of the Mentor Spirit or Paragon.
        /// </summary>
        public string Advantage
        {
            get => _strAdvantage;
            set => _strAdvantage = value;
        }

        /// <summary>
        /// Advantage of the mentor as it should be displayed in the UI. Advantage (Extra).
        /// </summary>
        public string DisplayAdvantage(string strLanguage)
        {
            string strReturn = _strAdvantage;

            if (!string.IsNullOrEmpty(_strExtra))
            {
                // Attempt to retrieve the CharacterAttribute name.
                strReturn += " (" + LanguageManager.TranslateExtra(_strExtra, strLanguage) + ')';
            }

            return strReturn;
        }

        /// <summary>
        /// Disadvantage of the Mentor Spirit or Paragon.
        /// </summary>
        public string Disadvantage
        {
            get => _strDisadvantage;
            set => _strDisadvantage = value;
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
            if (strLanguage == GlobalOptions.DefaultLanguage)
                return _strPage;

            return GetNode(strLanguage)?["altpage"]?.InnerText ?? _strPage;
        }

        /// <summary>
        /// Guid of the Xml Node containing data on this Mentor Spirit or Paragon.
        /// </summary>
        public string SourceID
        {
            get
            {
                return _sourceID.Equals(Guid.Empty) ? string.Empty : _sourceID.ToString("D");
            }
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
                _objCachedMyXmlNode = XmlManager.Load(_eMentorType == Improvement.ImprovementType.MentorSpirit ? "mentors.xml" : "paragons.xml", strLanguage).SelectSingleNode("/chummer/mentors/mentor[id = \"" + _sourceID.ToString("D") + "\"]");
                _strCachedXmlNodeLanguage = strLanguage;
            }
            return _objCachedMyXmlNode;
        }

        public string InternalId
        {
            get
            {
                return _guiID.ToString("D");
            }
        }
        #endregion
    }
}
