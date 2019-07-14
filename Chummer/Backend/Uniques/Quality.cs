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

using Chummer.Backend.Equipment;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using NLog;

namespace Chummer
{
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
    [HubClassTag("SourceID", true, "Name", "Extra;Type")]
    [DebuggerDisplay("{DisplayName(GlobalOptions.DefaultLanguage)}")]
    public class Quality : IHasInternalId, IHasName, IHasXmlNode, IHasNotes, IHasSource
    {
        private Logger Log = NLog.LogManager.GetCurrentClassLogger();
        private Guid _guiSourceID = Guid.Empty;
        private Guid _guiID;
        private string _strName = string.Empty;
        private bool _blnMetagenetic;
        private string _strExtra = string.Empty;
        private string _strSource = string.Empty;
        private string _strPage = string.Empty;
        private bool _blnMutant;
        private string _strNotes = string.Empty;
        private bool _blnImplemented = true;
        private bool _blnContributeToLimit = true;
        private bool _blnPrint = true;
        private bool _blnDoubleCostCareer = true;
        private bool _blnCanBuyWithSpellPoints;
        private int _intBP;
        private QualityType _eQualityType = QualityType.Positive;
        private QualitySource _eQualitySource = QualitySource.Selected;
        private string _strSourceName = string.Empty;
        private XmlNode _nodBonus;
        private XmlNode _nodFirstLevelBonus;
        private XmlNode _nodDiscounts;
        private readonly Character _objCharacter;
        private Guid _guiWeaponID;
        private string _strStage;
        private bool _blnStagedPurchase;

        public string Stage => _strStage;

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
        /// <param name="strSourceName">Friendly name for the improvement that added this quality.</param>
        public void Create(XmlNode objXmlQuality, QualitySource objQualitySource, IList<Weapon> lstWeapons, string strForceValue = "", string strSourceName = "")
        {
            if (!objXmlQuality.TryGetField("id", Guid.TryParse, out _guiSourceID))
            {
                Log.Warn(new object[] { "Missing id field for xmlnode", objXmlQuality });
                Utils.BreakIfDebug();
            }
            _strSourceName = strSourceName;
            objXmlQuality.TryGetStringFieldQuickly("name", ref _strName);
            objXmlQuality.TryGetBoolFieldQuickly("metagenetic", ref _blnMetagenetic);
            if (!objXmlQuality.TryGetStringFieldQuickly("altnotes", ref _strNotes))
                objXmlQuality.TryGetStringFieldQuickly("notes", ref _strNotes);
            objXmlQuality.TryGetInt32FieldQuickly("karma", ref _intBP);
            _eQualityType = ConvertToQualityType(objXmlQuality["category"]?.InnerText);
            _eQualitySource = objQualitySource;
            objXmlQuality.TryGetBoolFieldQuickly("doublecareer", ref _blnDoubleCostCareer);
            objXmlQuality.TryGetBoolFieldQuickly("canbuywithspellpoints", ref _blnCanBuyWithSpellPoints);
            objXmlQuality.TryGetBoolFieldQuickly("print", ref _blnPrint);
            objXmlQuality.TryGetBoolFieldQuickly("implemented", ref _blnImplemented);
            objXmlQuality.TryGetBoolFieldQuickly("contributetolimit", ref _blnContributeToLimit);
            objXmlQuality.TryGetBoolFieldQuickly("stagedpurchase", ref _blnStagedPurchase);
            objXmlQuality.TryGetStringFieldQuickly("source", ref _strSource);
            objXmlQuality.TryGetStringFieldQuickly("page", ref _strPage);
            _blnMutant = objXmlQuality["mutant"] != null;

            if (_eQualityType == QualityType.LifeModule)
            {
                objXmlQuality.TryGetStringFieldQuickly("stage", ref _strStage);
            }

            // Add Weapons if applicable.
            // More than one Weapon can be added, so loop through all occurrences.
            using (XmlNodeList xmlAddWeaponList = objXmlQuality.SelectNodes("addweapon"))
                if (xmlAddWeaponList?.Count > 0)
                {
                    XmlDocument objXmlWeaponDocument = XmlManager.Load("weapons.xml");
                    foreach (XmlNode objXmlAddWeapon in xmlAddWeaponList)
                    {
                        string strLoopID = objXmlAddWeapon.InnerText;
                        XmlNode objXmlWeapon = strLoopID.IsGuid()
                            ? objXmlWeaponDocument.SelectSingleNode("/chummer/weapons/weapon[id = \"" + strLoopID + "\"]")
                            : objXmlWeaponDocument.SelectSingleNode("/chummer/weapons/weapon[name = \"" + strLoopID + "\"]");

                        Weapon objGearWeapon = new Weapon(_objCharacter);
                        objGearWeapon.Create(objXmlWeapon, lstWeapons);
                        objGearWeapon.ParentID = InternalId;
                        objGearWeapon.Cost = "0";
                        lstWeapons.Add(objGearWeapon);

                        Guid.TryParse(objGearWeapon.InternalId, out _guiWeaponID);
                    }
                }

            using (XmlNodeList xmlNaturalWeaponList = objXmlQuality.SelectNodes("naturalweapons/naturalweapon"))
                if (xmlNaturalWeaponList?.Count > 0)
                    foreach (XmlNode objXmlNaturalWeapon in xmlNaturalWeaponList)
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
                            objWeapon.AP = objXmlNaturalWeapon["ap"].InnerText;
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

            _nodDiscounts = objXmlQuality["costdiscount"];
            // If the item grants a bonus, pass the information to the Improvement Manager.
            _nodBonus = objXmlQuality["bonus"];
            if (_nodBonus?.ChildNodes.Count > 0)
            {
                ImprovementManager.ForcedValue = strForceValue;
                if (!ImprovementManager.CreateImprovements(_objCharacter, Improvement.ImprovementSource.Quality, InternalId, _nodBonus, false, 1, DisplayNameShort(GlobalOptions.Language)))
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
            if (_nodFirstLevelBonus?.ChildNodes.Count > 0 && Levels == 0)
            {
                ImprovementManager.ForcedValue = string.IsNullOrEmpty(strForceValue) ? Extra : strForceValue;
                if (!ImprovementManager.CreateImprovements(_objCharacter, Improvement.ImprovementSource.Quality, InternalId, _nodFirstLevelBonus, false, 1, DisplayNameShort(GlobalOptions.Language)))
                {
                    _guiID = Guid.Empty;
                    return;
                }
            }

            if (string.IsNullOrEmpty(Notes))
            {
                string strEnglishNameOnPage = Name;
                string strNameOnPage = string.Empty;
                // make sure we have something and not just an empty tag
                if (objXmlQuality.TryGetStringFieldQuickly("nameonpage", ref strNameOnPage) && !string.IsNullOrEmpty(strNameOnPage))
                    strEnglishNameOnPage = strNameOnPage;

                string strQualityNotes = CommonFunctions.GetTextFromPDF($"{Source} {Page}", strEnglishNameOnPage);

                if (string.IsNullOrEmpty(strQualityNotes) && GlobalOptions.Language != GlobalOptions.DefaultLanguage)
                {
                    string strTranslatedNameOnPage = DisplayName(GlobalOptions.CultureInfo, GlobalOptions.Language);

                    // don't check again it is not translated
                    if (strTranslatedNameOnPage != _strName)
                    {
                        // if we found <altnameonpage>, and is not empty and not the same as english we must use that instead
                        if (objXmlQuality.TryGetStringFieldQuickly("altnameonpage", ref strNameOnPage)
                            && !string.IsNullOrEmpty(strNameOnPage) && strNameOnPage != strEnglishNameOnPage)
                            strTranslatedNameOnPage = strNameOnPage;

                        Notes = CommonFunctions.GetTextFromPDF($"{Source} {DisplayPage(GlobalOptions.Language)}", strTranslatedNameOnPage);
                    }
                }
                else
                    Notes = strQualityNotes;
            }
        }

        private SourceString _objCachedSourceDetail;
        public SourceString SourceDetail => _objCachedSourceDetail ?? (_objCachedSourceDetail =
                                                new SourceString(Source, DisplayPage(GlobalOptions.Language), GlobalOptions.Language));

        /// <summary>
        /// Save the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        public void Save(XmlTextWriter objWriter)
        {
            objWriter.WriteStartElement("quality");
            objWriter.WriteElementString("sourceid", SourceIDString);
            objWriter.WriteElementString("guid", InternalId);
            objWriter.WriteElementString("name", _strName);
            objWriter.WriteElementString("extra", _strExtra);
            objWriter.WriteElementString("bp", _intBP.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("implemented", _blnImplemented.ToString());
            objWriter.WriteElementString("contributetolimit", _blnContributeToLimit.ToString());
            objWriter.WriteElementString("stagedpurchase", _blnStagedPurchase.ToString());
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
                objWriter.WriteElementString("stage", _strStage);
            }

            objWriter.WriteEndElement();

            if (OriginSource != QualitySource.BuiltIn &&
                OriginSource != QualitySource.Improvement &&
                OriginSource != QualitySource.LifeModule &&
                OriginSource != QualitySource.Metatype &&
                OriginSource != QualitySource.MetatypeRemovable)
            _objCharacter.SourceProcess(_strSource);
        }

        /// <summary>
        /// Load the CharacterAttribute from the XmlNode.
        /// </summary>
        /// <param name="objNode">XmlNode to load.</param>
        public void Load(XmlNode objNode)
        {
            if (!objNode.TryGetField("guid", Guid.TryParse, out _guiID))
            {
                _guiID = Guid.NewGuid();
            }
            objNode.TryGetStringFieldQuickly("name", ref _strName);
            if(!objNode.TryGetGuidFieldQuickly("sourceid", ref _guiSourceID))
            {
                XmlNode node = GetNode(GlobalOptions.Language);
                node?.TryGetGuidFieldQuickly("id", ref _guiSourceID);
            }
            objNode.TryGetStringFieldQuickly("extra", ref _strExtra);
            objNode.TryGetInt32FieldQuickly("bp", ref _intBP);
            objNode.TryGetBoolFieldQuickly("implemented", ref _blnImplemented);
            objNode.TryGetBoolFieldQuickly("contributetolimit", ref _blnContributeToLimit);
            objNode.TryGetBoolFieldQuickly("stagedpurchase", ref _blnStagedPurchase);
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
                objNode.TryGetStringFieldQuickly("stage", ref _strStage);
            }
        }

        /// <summary>
        /// Print the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        /// <param name="intRating">Pre-calculated rating of the quality for printing.</param>
        /// <param name="objCulture">Culture in which to print.</param>
        /// <param name="strLanguageToPrint">Language in which to print</param>
        public void Print(XmlTextWriter objWriter, int intRating, CultureInfo objCulture, string strLanguageToPrint)
        {
            if (AllowPrint)
            {
                string strSpaceCharacter = LanguageManager.GetString("String_Space", strLanguageToPrint);
                string strRatingString = string.Empty;
                if (intRating > 1)
                    strRatingString = strSpaceCharacter + intRating.ToString(objCulture);
                string strSourceName = string.Empty;
                if (!string.IsNullOrWhiteSpace(SourceName))
                    strSourceName = strSpaceCharacter + '(' + GetSourceName(strLanguageToPrint) + ')';
                objWriter.WriteStartElement("quality");
                objWriter.WriteElementString("name", DisplayNameShort(strLanguageToPrint));
                objWriter.WriteElementString("name_english", Name + strRatingString);
                objWriter.WriteElementString("extra", LanguageManager.TranslateExtra(Extra, strLanguageToPrint) + strRatingString + strSourceName);
                objWriter.WriteElementString("bp", BP.ToString(objCulture));
                string strQualityType = Type.ToString();
                if (strLanguageToPrint != GlobalOptions.DefaultLanguage)
                {
                    strQualityType = XmlManager.Load("qualities.xml", strLanguageToPrint).SelectSingleNode("/chummer/categories/category[. = \"" + strQualityType + "\"]/@translate")?.InnerText ?? strQualityType;
                }
                objWriter.WriteElementString("qualitytype", strQualityType);
                objWriter.WriteElementString("qualitytype_english", Type.ToString());
                objWriter.WriteElementString("qualitysource", OriginSource.ToString());
                objWriter.WriteElementString("source", CommonFunctions.LanguageBookShort(Source, strLanguageToPrint));
                objWriter.WriteElementString("page", DisplayPage(strLanguageToPrint));
                if (_objCharacter.Options.PrintNotes)
                    objWriter.WriteElementString("notes", Notes);
                objWriter.WriteEndElement();
            }
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
        public string SourceIDString => _guiSourceID.ToString("D");

        /// <summary>
        /// Internal identifier which will be used to identify this Quality in the Improvement system.
        /// </summary>
        public string InternalId => _guiID.ToString("D");

        /// <summary>
        /// Guid of a Weapon.
        /// </summary>
        public string WeaponID
        {
            get => _guiWeaponID.ToString("D");
            set
            {
                if (Guid.TryParse(value, out Guid guiTemp))
                    _guiWeaponID = guiTemp;
            }
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
        /// 
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
        public string DisplayName(CultureInfo objCulture, string strLanguage)
        {
            string strReturn = DisplayNameShort(strLanguage);
            string strSpaceCharacter = LanguageManager.GetString("String_Space", strLanguage);

            if (!string.IsNullOrEmpty(Extra))
            {
                // Attempt to retrieve the CharacterAttribute name.
                strReturn += strSpaceCharacter + '(' + LanguageManager.TranslateExtra(Extra, strLanguage) + ')';
            }

            int intLevels = Levels;
            if (intLevels > 1)
                strReturn += strSpaceCharacter + intLevels.ToString(objCulture);

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
                return _objCharacter.Qualities.Count(objExistingQuality => objExistingQuality.SourceIDString == SourceIDString && objExistingQuality.Extra == Extra && objExistingQuality.SourceName == SourceName && objExistingQuality.Type == Type);
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
        /// Whether this quality can be purchased in stages, i.e. allowing the character to go into karmic debt
        /// </summary>
        public bool StagedPurchase
        {
            get => _blnStagedPurchase;
            set => _blnStagedPurchase = value;
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
                    ? XmlManager.Load("qualities.xml", strLanguage)
                        .SelectSingleNode($"/chummer/qualities/quality[name = \"{Name}\"]")
                    : XmlManager.Load("qualities.xml", strLanguage)
                        .SelectSingleNode($"/chummer/qualities/quality[id = \"{SourceIDString}\" or id = \"{SourceIDString.ToUpperInvariant()}\"]");
                _strCachedXmlNodeLanguage = strLanguage;
            }
            return _objCachedMyXmlNode;
        }
        #endregion

        #region UI Methods
        public TreeNode CreateTreeNode(ContextMenuStrip cmsQuality)
        {
            if ((OriginSource == QualitySource.BuiltIn ||
                 OriginSource == QualitySource.Improvement ||
                 OriginSource == QualitySource.LifeModule ||
                 OriginSource == QualitySource.Metatype ||
                 OriginSource == QualitySource.MetatypeRemovable) && !string.IsNullOrEmpty(Source) && !_objCharacter.Options.BookEnabled(Source))
                return null;

            TreeNode objNode = new TreeNode
            {
                Name = InternalId,
                Text = DisplayName(GlobalOptions.CultureInfo, GlobalOptions.Language),
                Tag = this,
                ContextMenuStrip = cmsQuality,
                ForeColor = PreferredColor,
                ToolTipText = Notes.WordWrap(100)
            };

            return objNode;
        }

        public Color PreferredColor
        {
            get
            {
                if (!Implemented)
                {
                    return Color.Red;
                }
                if (!string.IsNullOrEmpty(Notes))
                {
                    return Color.SaddleBrown;
                }
                if (OriginSource == QualitySource.BuiltIn ||
                    OriginSource == QualitySource.Improvement ||
                    OriginSource == QualitySource.LifeModule ||
                    OriginSource == QualitySource.Metatype)
                {
                    return SystemColors.GrayText;
                }

                return SystemColors.WindowText;
            }
        }
        #endregion

        #region Static Methods

        /// <summary>
        /// Retuns weither a quality is valid on said Character
        /// THIS IS A WIP AND ONLY CHECKS QUALITIES. REQUIRED POWERS, METATYPES AND OTHERS WON'T BE CHECKED
        /// </summary>
        /// <param name="objCharacter">The Character</param>
        /// <param name="xmlQuality">The XmlNode describing the quality</param>
        /// <returns>Is the Quality valid on said Character</returns>
        public static bool IsValid(Character objCharacter, XmlNode xmlQuality)
        {
            return IsValid(objCharacter, xmlQuality, out QualityFailureReason _, out List<Quality> _);
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
                    if (objQuality.SourceIDString == objXmlQuality["id"]?.InnerText)
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
                    HashSet<string> lstRequired = new HashSet<string>();
                    using (XmlNodeList xmlNodeList = xmlOneOfNode.SelectNodes("quality"))
                        if (xmlNodeList != null)
                            foreach (XmlNode node in xmlNodeList)
                            {
                                lstRequired.Add(node.InnerText);
                            }

                    if (!objCharacter.Qualities.Any(quality => lstRequired.Contains(quality.Name)))
                    {
                        reason |= QualityFailureReason.RequiredSingle;
                    }

                    reason |= QualityFailureReason.MetatypeRequired;
                    using (XmlNodeList xmlNodeList = xmlOneOfNode.SelectNodes("metatype"))
                        if (xmlNodeList != null)
                            foreach (XmlNode objNode in xmlNodeList)
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
                    HashSet<string> lstRequired = new HashSet<string>();
                    foreach (Quality objQuality in objCharacter.Qualities)
                    {
                        lstRequired.Add(objQuality.Name);
                    }
                    using (XmlNodeList xmlNodeList = xmlAllOfNode.SelectNodes("quality"))
                        if (xmlNodeList != null)
                            foreach (XmlNode node in xmlNodeList)
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
                    HashSet<string> qualityForbidden = new HashSet<string>();
                    using (XmlNodeList xmlNodeList = xmlOneOfNode.SelectNodes("quality"))
                        if (xmlNodeList != null)
                            foreach (XmlNode node in xmlNodeList)
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

            return conflictingQualities.Count <= 0 && reason == QualityFailureReason.Allowed;
        }

        /// <summary>
        /// This method builds a xmlNode upwards adding/overriding elements
        /// </summary>
        /// <param name="id">ID of the node</param>
        /// <param name="xmlDoc">XmlDocument containing the object with which to override this quality.</param>
        /// <returns>A XmlNode containing the id and all nodes of its parrents</returns>
        public static XmlNode GetNodeOverrideable(string id, XmlDocument xmlDoc)
        {
            return GetNodeOverrideable(xmlDoc.SelectSingleNode("//*[id = \"" + id + "\"]"));
        }

        private static XmlNode GetNodeOverrideable(XmlNode n)
        {
            XmlNode workNode = n.Clone();  //clone as to not mess up the acctual xml document

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

            return workNode;
        }
        #endregion

        /// <summary>
        /// Swaps an old quality for a new one. 
        /// </summary>
        /// <param name="objOldQuality">Old quality that's being removed.</param>
        /// <param name="objCharacter">Character object that the quality will be removed from.</param>
        /// <param name="objXmlQuality">XML entry for the new quality.</param>
        /// <param name="source">QualitySource type. Expected to be QualitySource.Selected in most cases.</param>
        /// <returns></returns>
        public bool Swap(Quality objOldQuality, Character objCharacter, XmlNode objXmlQuality, QualitySource source = QualitySource.Selected)
        {
            List<Weapon> lstWeapons = new List<Weapon>();
            Create(objXmlQuality, source, lstWeapons);

            bool blnAddItem = true;
            int intKarmaCost = (BP - objOldQuality.BP) * objCharacter.Options.KarmaQuality;
            // Make sure the character has enough Karma to pay for the Quality.
            if (Type == QualityType.Positive)
            {
                if (objCharacter.Created && !objCharacter.Options.DontDoubleQualityPurchases)
                {
                    intKarmaCost *= 2;
                }
                if (intKarmaCost > objCharacter.Karma)
                {
                    MessageBox.Show(LanguageManager.GetString("Message_NotEnoughKarma", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_NotEnoughKarma", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                    blnAddItem = false;
                }

                if (blnAddItem)
                {
                    if (!objCharacter.ConfirmKarmaExpense(string.Format(LanguageManager.GetString("Message_QualitySwap", GlobalOptions.Language)
                        , objOldQuality.DisplayNameShort(GlobalOptions.Language)
                        , DisplayNameShort(GlobalOptions.Language))))
                        blnAddItem = false;
                }

                if (!blnAddItem) return false;
                if (objCharacter.Created)
                {
                    // Create the Karma expense.
                    ExpenseLogEntry objExpense = new ExpenseLogEntry(objCharacter);
                    objExpense.Create(intKarmaCost * -1,
                        string.Format(LanguageManager.GetString("String_ExpenseSwapPositiveQuality", GlobalOptions.Language)
                            , DisplayNameShort(GlobalOptions.Language)
                            , DisplayNameShort(GlobalOptions.Language)), ExpenseType.Karma, DateTime.Now);
                    objCharacter.ExpenseEntries.AddWithSort(objExpense);
                    objCharacter.Karma -= intKarmaCost;
                }
            }
            else
            {
                if (!objCharacter.Options.DontDoubleQualityRefunds)
                {
                    intKarmaCost *= 2;
                }
                // This should only happen when a character is trading up to a less-costly Quality.
                if (intKarmaCost > 0)
                {
                    if (intKarmaCost > objCharacter.Karma)
                    {
                        MessageBox.Show(LanguageManager.GetString("Message_NotEnoughKarma", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_NotEnoughKarma", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                        blnAddItem = false;
                    }

                    if (blnAddItem)
                    {
                        if (!objCharacter.ConfirmKarmaExpense(string.Format(LanguageManager.GetString("Message_QualitySwap", GlobalOptions.Language), objOldQuality.DisplayNameShort(GlobalOptions.Language), DisplayNameShort(GlobalOptions.Language))))
                            blnAddItem = false;
                    }
                }
                else
                {
                    // Trading a more expensive quality for a less expensive quality shouldn't give you karma. TODO: Optional rule to govern this behaviour.
                    intKarmaCost = 0;
                }

                if (!blnAddItem) return false;
                if (objCharacter.Created)
                {
                    // Create the Karma expense.
                    ExpenseLogEntry objExpense = new ExpenseLogEntry(objCharacter);
                    objExpense.Create(intKarmaCost * -1,
                        string.Format(LanguageManager.GetString("String_ExpenseSwapNegativeQuality", GlobalOptions.Language)
                            , DisplayNameShort(GlobalOptions.Language)
                            , DisplayNameShort(GlobalOptions.Language)), ExpenseType.Karma, DateTime.Now);
                    objCharacter.ExpenseEntries.AddWithSort(objExpense);
                    objCharacter.Karma -= intKarmaCost;
                }
            }

            // Add any created Weapons to the character.
            foreach (Weapon objWeapon in lstWeapons)
            {
                objCharacter.Weapons.Add(objWeapon);
            }

            // Remove any Improvements for the old Quality.
            ImprovementManager.RemoveImprovements(objCharacter, Improvement.ImprovementSource.Quality, objOldQuality.InternalId);
            objCharacter.Qualities.Remove(objOldQuality);

            // Remove any Weapons created by the old Quality if applicable.
            if (!objOldQuality.WeaponID.IsEmptyGuid())
            {
                List<Weapon> lstOldWeapons = objCharacter.Weapons.DeepWhere(x => x.Children, x => x.ParentID == objOldQuality.InternalId).ToList();
                foreach (Weapon objWeapon in lstOldWeapons)
                {
                    objWeapon.DeleteWeapon();
                    // We can remove here because lstWeapons is separate from the Weapons that were yielded through DeepWhere
                    if (objWeapon.Parent != null)
                        objWeapon.Parent.Children.Remove(objWeapon);
                    else
                        objCharacter.Weapons.Remove(objWeapon);
                }
            }

            // Add the new Quality to the character.
            objCharacter.Qualities.Add(this);


            return true;
        }

        public void SetSourceDetail(Control sourceControl)
        {
            if (_objCachedSourceDetail?.Language != GlobalOptions.Language)
                _objCachedSourceDetail = null;
            SourceDetail.SetControl(sourceControl);
        }
    }
}
