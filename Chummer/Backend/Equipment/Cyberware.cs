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
using System.Collections.Specialized;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;
using Chummer.Backend.Attributes;
using NLog;

namespace Chummer.Backend.Equipment
{
    /// <summary>
    /// A piece of Cyberware.
    /// </summary>
    [HubClassTag("SourceID", true, "Name", "Extra")]
    [DebuggerDisplay("{DisplayName(GlobalOptions.InvariantCultureInfo, GlobalOptions.DefaultLanguage)}")]
    public class Cyberware : ICanPaste, IHasChildren<Cyberware>, IHasGear, IHasName, IHasInternalId, IHasXmlNode,
        IHasMatrixAttributes, IHasNotes, ICanSell, IHasRating, IHasSource, ICanSort, IHasStolenProperty,
        IHasWirelessBonus
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private Guid _guiSourceID = Guid.Empty;
        private Guid _guiID;
        private string _strName = string.Empty;
        private string _strCategory = string.Empty;
        private string _strLimbSlot = string.Empty;
        private string _strLimbSlotCount = "1";
        private bool _blnInheritAttributes;
        private string _strESS = string.Empty;
        private decimal _decExtraESSAdditiveMultiplier;
        private decimal _decExtraESSMultiplicativeMultiplier = 1.0m;
        private string _strCapacity = string.Empty;
        private string _strAvail = string.Empty;
        private string _strCost = string.Empty;
        private string _strSource = string.Empty;
        private string _strPage = string.Empty;
        private int _intMatrixCMFilled;
        private int _intRating;
        private string _strMinRating = string.Empty;
        private string _strMaxRating = string.Empty;
        private string _strRatingLabel = "String_Rating";
        private string _strAllowSubsystems = string.Empty;
        private bool _blnSuite;
        private bool _blnStolen;
        private string _strLocation = string.Empty;
        private string _strExtra = string.Empty;
        private Guid _guiWeaponID = Guid.Empty;
        private Guid _guiVehicleID = Guid.Empty;
        private Grade _objGrade;

        private readonly TaggedObservableCollection<Cyberware> _lstChildren =
            new TaggedObservableCollection<Cyberware>();

        private readonly TaggedObservableCollection<Gear> _lstGear = new TaggedObservableCollection<Gear>();
        private XmlNode _nodBonus;
        private XmlNode _nodPairBonus;
        private XmlNode _nodWirelessBonus;
        private XmlNode _nodWirelessPairBonus;
        private readonly HashSet<string> _lstIncludeInPairBonus = new HashSet<string>();
        private readonly HashSet<string> _lstIncludeInWirelessPairBonus = new HashSet<string>();
        private bool _blnWirelessOn;
        private XmlNode _nodAllowGear;
        private Improvement.ImprovementSource _objImprovementSource = Improvement.ImprovementSource.Cyberware;
        private string _strNotes = string.Empty;
        private int _intEssenceDiscount;
        private string _strForceGrade = string.Empty;
        private bool _blnDiscountCost;
        private Vehicle _objParentVehicle;
        private bool _blnPrototypeTranshuman;
        private Cyberware _objParent;
        private bool _blnAddToParentESS;
        private bool _blnAddToParentCapacity;
        private string _strParentID = string.Empty;
        private string _strHasModularMount = string.Empty;
        private string _strPlugsIntoModularMount = string.Empty;
        private string _strBlocksMounts = string.Empty;
        private string _strForced = string.Empty;

        private string _strDeviceRating = string.Empty;
        private string _strAttack = string.Empty;
        private string _strSleaze = string.Empty;
        private string _strDataProcessing = string.Empty;
        private string _strFirewall = string.Empty;
        private string _strAttributeArray = string.Empty;
        private string _strModAttack = string.Empty;
        private string _strModSleaze = string.Empty;
        private string _strModDataProcessing = string.Empty;
        private string _strModFirewall = string.Empty;
        private string _strModAttributeArray = string.Empty;
        private string _strProgramLimit = string.Empty;
        private string _strOverclocked = "None";
        private bool _blnCanSwapAttributes;
        private int _intSortOrder;

        private readonly Character _objCharacter;
        private static readonly char[] s_MathOperators = new char[] {'"', '*', '/', '+', '-'};

        // I don't like this, but it's easier than making it a specific property of the cyberware.
        private static readonly HashSet<string> s_AgilityCustomizationStrings = new HashSet<string>
            {"Customized Agility", "Cyberlimb Customization, Agility (2050)"};

        private static readonly HashSet<string> s_AgilityEnhancementStrings = new HashSet<string>
            {"Enhanced Agility", "Cyberlimb Augmentation, Agility (2050)"};

        private static readonly HashSet<string> s_AgilityCombinedStrings =
            new HashSet<string>(s_AgilityEnhancementStrings.Union(s_AgilityEnhancementStrings));

        private static readonly HashSet<string> s_StrengthCustomizationStrings = new HashSet<string>
            {"Customized Strength", "Cyberlimb Customization, Strength (2050)"};

        private static readonly HashSet<string> s_StrengthEnhancementStrings = new HashSet<string>
            {"Enhanced Strength", "Cyberlimb Augmentation, Strength (2050)"};

        private static readonly HashSet<string> s_StrengthCombinedStrings =
            new HashSet<string>(s_StrengthEnhancementStrings.Union(s_StrengthEnhancementStrings));

        #region Helper Methods

        /// <summary>
        /// Convert a string to a Grade.
        /// </summary>
        /// <param name="strValue">String value to convert.</param>
        /// <param name="objSource">Source representing whether this is a cyberware or bioware grade.</param>
        /// <param name="objCharacter">Character from which to fetch a grade list</param>
        public static Grade ConvertToCyberwareGrade(string strValue, Improvement.ImprovementSource objSource,
            Character objCharacter)
        {
            if (objCharacter == null)
                throw new ArgumentNullException(nameof(objCharacter));
            IList<Grade> lstGrades = objCharacter.GetGradeList(objSource, true);
            foreach (Grade objGrade in lstGrades)
            {
                if (objGrade.Name == strValue)
                    return objGrade;
            }

            return lstGrades.FirstOrDefault(x => x.Name == "Standard");
        }

        #endregion

        #region Constructor, Create, Save, Load, and Print Methods

        public Cyberware(Character objCharacter)
        {
            // Create the GUID for the new piece of Cyberware.
            _guiID = Guid.NewGuid();
            _objCharacter = objCharacter;

            _lstChildren.CollectionChanged += CyberwareChildrenOnCollectionChanged;
            _lstGear.CollectionChanged += GearChildrenOnCollectionChanged;
        }

        private void CyberwareChildrenOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            bool blnDoCyberlimbAGIRefresh = false;
            bool blnDoCyberlimbSTRRefresh = false;
            bool blnDoEssenceImprovementsRefresh = false;
            bool blnDoRedlinerRefresh = false;
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (Cyberware objNewItem in e.NewItems)
                    {
                        objNewItem.Parent = this;
                        if ((!blnDoCyberlimbAGIRefresh || !blnDoCyberlimbSTRRefresh) &&
                            Category == "Cyberlimb" && Parent?.InheritAttributes != false && ParentVehicle == null &&
                            !_objCharacter.Options.DontUseCyberlimbCalculation &&
                            !string.IsNullOrWhiteSpace(LimbSlot) &&
                            !_objCharacter.Options.ExcludeLimbSlot.Contains(LimbSlot))
                        {
                            if (InheritAttributes)
                            {
                                blnDoCyberlimbAGIRefresh = true;
                                blnDoCyberlimbSTRRefresh = true;
                            }
                            else
                            {
                                if (!blnDoCyberlimbAGIRefresh && s_AgilityCombinedStrings.Contains(objNewItem.Name))
                                {
                                    blnDoCyberlimbAGIRefresh = true;
                                }

                                if (!blnDoCyberlimbSTRRefresh && s_StrengthCombinedStrings.Contains(objNewItem.Name))
                                {
                                    blnDoCyberlimbSTRRefresh = true;
                                }
                            }
                        }

                        if (!blnDoEssenceImprovementsRefresh && (Parent == null || AddToParentESS) &&
                            string.IsNullOrEmpty(PlugsIntoModularMount) && ParentVehicle == null)
                            blnDoEssenceImprovementsRefresh = true;
                    }

                    this.RefreshMatrixAttributeArray();
                    blnDoRedlinerRefresh = true;
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (Cyberware objOldItem in e.OldItems)
                    {
                        objOldItem.Parent = null;
                        if ((!blnDoCyberlimbAGIRefresh || !blnDoCyberlimbSTRRefresh) &&
                            Category == "Cyberlimb" && Parent?.InheritAttributes != false && ParentVehicle == null &&
                            !_objCharacter.Options.DontUseCyberlimbCalculation &&
                            !string.IsNullOrWhiteSpace(LimbSlot) &&
                            !_objCharacter.Options.ExcludeLimbSlot.Contains(LimbSlot))
                        {
                            if (InheritAttributes)
                            {
                                blnDoCyberlimbAGIRefresh = true;
                                blnDoCyberlimbSTRRefresh = true;
                            }
                            else
                            {
                                if (!blnDoCyberlimbAGIRefresh && s_AgilityCombinedStrings.Contains(objOldItem.Name))
                                {
                                    blnDoCyberlimbAGIRefresh = true;
                                }

                                if (!blnDoCyberlimbSTRRefresh && s_StrengthCombinedStrings.Contains(objOldItem.Name))
                                {
                                    blnDoCyberlimbSTRRefresh = true;
                                }
                            }
                        }

                        if (!blnDoEssenceImprovementsRefresh && (Parent == null || AddToParentESS) &&
                            string.IsNullOrEmpty(PlugsIntoModularMount) && ParentVehicle == null)
                            blnDoEssenceImprovementsRefresh = true;
                    }

                    this.RefreshMatrixAttributeArray();
                    blnDoRedlinerRefresh = true;
                    break;
                case NotifyCollectionChangedAction.Replace:
                    foreach (Cyberware objOldItem in e.OldItems)
                    {
                        objOldItem.Parent = null;
                        if ((!blnDoCyberlimbAGIRefresh || !blnDoCyberlimbSTRRefresh) &&
                            Category == "Cyberlimb" && Parent?.InheritAttributes != false && ParentVehicle == null &&
                            !_objCharacter.Options.DontUseCyberlimbCalculation &&
                            !string.IsNullOrWhiteSpace(LimbSlot) &&
                            !_objCharacter.Options.ExcludeLimbSlot.Contains(LimbSlot))
                        {
                            if (InheritAttributes)
                            {
                                blnDoCyberlimbAGIRefresh = true;
                                blnDoCyberlimbSTRRefresh = true;
                            }
                            else
                            {
                                if (!blnDoCyberlimbAGIRefresh && s_AgilityCombinedStrings.Contains(objOldItem.Name))
                                {
                                    blnDoCyberlimbAGIRefresh = true;
                                }

                                if (!blnDoCyberlimbSTRRefresh && s_StrengthCombinedStrings.Contains(objOldItem.Name))
                                {
                                    blnDoCyberlimbSTRRefresh = true;
                                }
                            }
                        }

                        if (!blnDoEssenceImprovementsRefresh && (Parent == null || AddToParentESS) &&
                            string.IsNullOrEmpty(PlugsIntoModularMount) && ParentVehicle == null)
                            blnDoEssenceImprovementsRefresh = true;
                    }

                    foreach (Cyberware objNewItem in e.NewItems)
                    {
                        objNewItem.Parent = this;
                        if ((!blnDoCyberlimbAGIRefresh || !blnDoCyberlimbSTRRefresh) &&
                            Category == "Cyberlimb" && Parent?.InheritAttributes != false && ParentVehicle == null &&
                            !_objCharacter.Options.DontUseCyberlimbCalculation &&
                            !string.IsNullOrWhiteSpace(LimbSlot) &&
                            !_objCharacter.Options.ExcludeLimbSlot.Contains(LimbSlot))
                        {
                            if (InheritAttributes)
                            {
                                blnDoCyberlimbAGIRefresh = true;
                                blnDoCyberlimbSTRRefresh = true;
                            }
                            else
                            {
                                if (!blnDoCyberlimbAGIRefresh && s_AgilityCombinedStrings.Contains(objNewItem.Name))
                                {
                                    blnDoCyberlimbAGIRefresh = true;
                                }

                                if (!blnDoCyberlimbSTRRefresh && s_StrengthCombinedStrings.Contains(objNewItem.Name))
                                {
                                    blnDoCyberlimbSTRRefresh = true;
                                }
                            }
                        }

                        if (!blnDoEssenceImprovementsRefresh && (Parent == null || AddToParentESS) &&
                            string.IsNullOrEmpty(PlugsIntoModularMount) && ParentVehicle == null)
                            blnDoEssenceImprovementsRefresh = true;
                    }

                    this.RefreshMatrixAttributeArray();
                    blnDoRedlinerRefresh = true;
                    break;
                case NotifyCollectionChangedAction.Reset:
                    blnDoEssenceImprovementsRefresh = true;
                    if (Category == "Cyberlimb" && Parent?.InheritAttributes != false && ParentVehicle == null &&
                        !_objCharacter.Options.DontUseCyberlimbCalculation &&
                        !string.IsNullOrWhiteSpace(LimbSlot) &&
                        !_objCharacter.Options.ExcludeLimbSlot.Contains(LimbSlot))
                    {
                        blnDoCyberlimbAGIRefresh = true;
                        blnDoCyberlimbSTRRefresh = true;
                    }

                    this.RefreshMatrixAttributeArray();
                    blnDoRedlinerRefresh = true;
                    break;
            }

            bool blnDoMovementUpdate = false;
            if (blnDoCyberlimbAGIRefresh || blnDoCyberlimbSTRRefresh)
            {
                foreach (CharacterAttrib objCharacterAttrib in _objCharacter.AttributeSection.AttributeList.Concat(
                    _objCharacter.AttributeSection.SpecialAttributeList))
                {
                    if ((blnDoCyberlimbAGIRefresh && objCharacterAttrib.Abbrev == "AGI") ||
                        (blnDoCyberlimbSTRRefresh && objCharacterAttrib.Abbrev == "STR"))
                    {
                        objCharacterAttrib.OnPropertyChanged(nameof(CharacterAttrib.TotalValue));
                    }
                }

                blnDoMovementUpdate = _objCharacter.Options.CyberlegMovement && LimbSlot == "leg";
            }

            if (_objCharacter != null)
            {
                List<string> lstPropertiesToChange = new List<string>();
                if (blnDoRedlinerRefresh)
                    lstPropertiesToChange.Add(nameof(Character.RedlinerBonus));
                if (blnDoEssenceImprovementsRefresh)
                    lstPropertiesToChange.Add(EssencePropertyName);
                if (blnDoMovementUpdate)
                    lstPropertiesToChange.Add(nameof(Character.GetMovement));
                if (lstPropertiesToChange.Count > 0)
                    _objCharacter.OnMultiplePropertyChanged(lstPropertiesToChange.ToArray());
            }
        }

        private void GearChildrenOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (Gear objNewItem in e.NewItems)
                    {
                        objNewItem.Parent = this;
                        objNewItem.ChangeEquippedStatus(IsModularCurrentlyEquipped);
                    }

                    this.RefreshMatrixAttributeArray();
                    break;
                case NotifyCollectionChangedAction.Replace:
                    foreach (Gear objOldItem in e.OldItems)
                    {
                        objOldItem.Parent = null;
                    }
                    foreach (Gear objNewItem in e.NewItems)
                    {
                        objNewItem.Parent = this;
                        objNewItem.ChangeEquippedStatus(IsModularCurrentlyEquipped);
                    }

                    this.RefreshMatrixAttributeArray();
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (Gear objOldItem in e.OldItems)
                    {
                        objOldItem.Parent = null;
                    }

                    this.RefreshMatrixAttributeArray();
                    break;
                case NotifyCollectionChangedAction.Reset:
                    this.RefreshMatrixAttributeArray();
                    break;
            }
        }

        /// Create a Cyberware from an XmlNode.
        /// <param name="objXmlCyberware">XmlNode to create the object from.</param>
        /// <param name="objGrade">Grade of the selected piece.</param>
        /// <param name="objSource">Source of the piece.</param>
        /// <param name="intRating">Selected Rating of the piece of Cyberware.</param>
        /// <param name="lstWeapons">List of Weapons that should be added to the Character.</param>
        /// <param name="lstVehicles">List of Vehicles that should be added to the Character.</param>
        /// <param name="blnCreateImprovements">Whether or not Improvements should be created.</param>
        /// <param name="blnCreateChildren">Whether or not child items should be created.</param>
        /// <param name="strForced">Force a particular value to be selected by an Improvement prompts.</param>
        /// <param name="objParent">Cyberware to which this new cyberware should be added (needed in creation method for selecting a side).</param>
        /// <param name="objParentVehicle">Vehicle to which this new cyberware will be added (needed in creation method for selecting a side and improvements).</param>
        public void Create(XmlNode objXmlCyberware, Grade objGrade, Improvement.ImprovementSource objSource,
            int intRating, IList<Weapon> lstWeapons, IList<Vehicle> lstVehicles, bool blnCreateImprovements = true,
            bool blnCreateChildren = true, string strForced = "", Cyberware objParent = null,
            Vehicle objParentVehicle = null)
        {
            Parent = objParent;
            _strForced = strForced;
            _objParentVehicle = objParentVehicle;
            if (!objXmlCyberware.TryGetField("id", Guid.TryParse, out _guiSourceID))
            {
                Log.Warn(new object[] {"Missing id field for cyberware xmlnode", objXmlCyberware});
                Utils.BreakIfDebug();
            }
            else
                _objCachedMyXmlNode = null;

            objXmlCyberware.TryGetStringFieldQuickly("name", ref _strName);
            objXmlCyberware.TryGetStringFieldQuickly("category", ref _strCategory);
            objXmlCyberware.TryGetStringFieldQuickly("limbslot", ref _strLimbSlot);
            objXmlCyberware.TryGetStringFieldQuickly("limbslotcount", ref _strLimbSlotCount);
            if (!objXmlCyberware.TryGetStringFieldQuickly("altnotes", ref _strNotes))
                objXmlCyberware.TryGetStringFieldQuickly("notes", ref _strNotes);

            if (string.IsNullOrEmpty(Notes))
            {
                string strEnglishNameOnPage = Name;
                string strNameOnPage = string.Empty;
                // make sure we have something and not just an empty tag
                if (objXmlCyberware.TryGetStringFieldQuickly("nameonpage", ref strNameOnPage) &&
                    !string.IsNullOrEmpty(strNameOnPage))
                    strEnglishNameOnPage = strNameOnPage;

                string strGearNotes = CommonFunctions.GetTextFromPDF(Source + ' ' + Page, strEnglishNameOnPage);

                if (string.IsNullOrEmpty(strGearNotes) && GlobalOptions.Language != GlobalOptions.DefaultLanguage)
                {
                    string strTranslatedNameOnPage = CurrentDisplayName;

                    // don't check again it is not translated
                    if (strTranslatedNameOnPage != _strName)
                    {
                        // if we found <altnameonpage>, and is not empty and not the same as english we must use that instead
                        if (objXmlCyberware.TryGetStringFieldQuickly("altnameonpage", ref strNameOnPage)
                            && !string.IsNullOrEmpty(strNameOnPage) && strNameOnPage != strEnglishNameOnPage)
                            strTranslatedNameOnPage = strNameOnPage;

                        Notes = CommonFunctions.GetTextFromPDF(Source + ' ' + DisplayPage(GlobalOptions.Language),
                            strTranslatedNameOnPage);
                    }
                }
                else
                    Notes = strGearNotes;
            }

            _blnInheritAttributes = objXmlCyberware["inheritattributes"] != null;
            _objGrade = objGrade;
            objXmlCyberware.TryGetStringFieldQuickly("ess", ref _strESS);
            objXmlCyberware.TryGetStringFieldQuickly("capacity", ref _strCapacity);
            objXmlCyberware.TryGetStringFieldQuickly("avail", ref _strAvail);
            objXmlCyberware.TryGetStringFieldQuickly("source", ref _strSource);
            objXmlCyberware.TryGetStringFieldQuickly("page", ref _strPage);
            _blnAddToParentESS = objXmlCyberware["addtoparentess"] != null;
            _blnAddToParentCapacity = objXmlCyberware["addtoparentcapacity"] != null;
            _nodBonus = objXmlCyberware["bonus"];
            _nodPairBonus = objXmlCyberware["pairbonus"];
            _nodWirelessBonus = objXmlCyberware["wirelessbonus"];
            _nodWirelessPairBonus = objXmlCyberware["wirelesspairbonus"];
            _blnWirelessOn = _nodWirelessPairBonus != null;
            _nodAllowGear = objXmlCyberware["allowgear"];
            objXmlCyberware.TryGetStringFieldQuickly("mountsto", ref _strPlugsIntoModularMount);
            objXmlCyberware.TryGetStringFieldQuickly("modularmount", ref _strHasModularMount);
            objXmlCyberware.TryGetStringFieldQuickly("blocksmounts", ref _strBlocksMounts);

            _objImprovementSource = objSource;
            _objCachedMyXmlNode = null;
            objXmlCyberware.TryGetStringFieldQuickly("rating", ref _strMaxRating);
            objXmlCyberware.TryGetStringFieldQuickly("minrating", ref _strMinRating);
            objXmlCyberware.TryGetStringFieldQuickly("ratinglabel", ref _strRatingLabel);

            _intRating = Math.Min(Math.Max(intRating, MinRating), MaxRating);

            objXmlCyberware.TryGetStringFieldQuickly("devicerating", ref _strDeviceRating);
            if (!objXmlCyberware.TryGetStringFieldQuickly("attributearray", ref _strAttributeArray))
            {
                objXmlCyberware.TryGetStringFieldQuickly("attack", ref _strAttack);
                objXmlCyberware.TryGetStringFieldQuickly("sleaze", ref _strSleaze);
                objXmlCyberware.TryGetStringFieldQuickly("dataprocessing", ref _strDataProcessing);
                objXmlCyberware.TryGetStringFieldQuickly("firewall", ref _strFirewall);
            }
            else
            {
                _blnCanSwapAttributes = true;
                string[] strArray = _strAttributeArray.Split(',');
                _strAttack = strArray[0];
                _strSleaze = strArray[1];
                _strDataProcessing = strArray[2];
                _strFirewall = strArray[3];
            }

            objXmlCyberware.TryGetStringFieldQuickly("modattack", ref _strModAttack);
            objXmlCyberware.TryGetStringFieldQuickly("modsleaze", ref _strModSleaze);
            objXmlCyberware.TryGetStringFieldQuickly("moddataprocessing", ref _strModDataProcessing);
            objXmlCyberware.TryGetStringFieldQuickly("modfirewall", ref _strModFirewall);
            objXmlCyberware.TryGetStringFieldQuickly("modattributearray", ref _strModAttributeArray);

            objXmlCyberware.TryGetStringFieldQuickly("programs", ref _strProgramLimit);

            objXmlCyberware.TryGetStringFieldQuickly("forcegrade", ref _strForceGrade);

            // Add Subsytem information if applicable.
            if (objXmlCyberware.InnerXml.Contains("allowsubsystems"))
            {
                string strSubsystem = string.Empty;
                XmlNodeList lstSubSystems = objXmlCyberware.SelectNodes("allowsubsystems/category");
                for (int i = 0; i < lstSubSystems?.Count; i++)
                {
                    strSubsystem += lstSubSystems[i].InnerText;
                    if (i != lstSubSystems.Count - 1)
                    {
                        strSubsystem += ",";
                    }
                }

                _strAllowSubsystems = strSubsystem;
            }

            if (objXmlCyberware.SelectSingleNode("pairinclude")?.Attributes?["includeself"]?.InnerText !=
                bool.FalseString)
            {
                _lstIncludeInPairBonus.Add(Name);
            }
            foreach (XmlNode objPairNameNode in objXmlCyberware.SelectNodes("pairinclude/name"))
            {
                _lstIncludeInPairBonus.Add(objPairNameNode.InnerText);
            }

            if (objXmlCyberware.SelectSingleNode("wirelesspairinclude")?.Attributes?["includeself"]?.InnerText !=
                bool.FalseString)
            {
                _lstIncludeInWirelessPairBonus.Add(Name);
            }
            foreach (XmlNode objPairNameNode in objXmlCyberware.SelectNodes("wirelesspairinclude/name"))
            {
                _lstIncludeInWirelessPairBonus.Add(objPairNameNode.InnerText);
            }

            _strCost = objXmlCyberware["cost"]?.InnerText ?? "0";
            // Check for a Variable Cost.
            if (_strCost.StartsWith("Variable(", StringComparison.Ordinal))
            {
                decimal decMin;
                decimal decMax = decimal.MaxValue;
                string strCost = _strCost.TrimStartOnce("Variable(", true).TrimEndOnce(')');
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
                    if (decMax > 1000000)
                        decMax = 1000000;
                    using (frmSelectNumber frmPickNumber = new frmSelectNumber(_objCharacter.Options.NuyenDecimals)
                    {
                        Minimum = decMin,
                        Maximum = decMax,
                        Description = string.Format(GlobalOptions.CultureInfo, LanguageManager.GetString("String_SelectVariableCost"),
                            DisplayNameShort(GlobalOptions.Language)),
                        AllowCancel = false
                    })
                    {
                        frmPickNumber.ShowDialog();
                        _strCost = frmPickNumber.SelectedValue.ToString(GlobalOptions.InvariantCultureInfo);
                    }
                }
            }

            // Add Cyberweapons if applicable.
            XmlDocument objXmlWeaponDocument = XmlManager.Load("weapons.xml");

            // More than one Weapon can be added, so loop through all occurrences.
            foreach (XmlNode objXmlAddWeapon in objXmlCyberware.SelectNodes("addweapon"))
            {
                string strLoopID = objXmlAddWeapon.InnerText;
                XmlNode objXmlWeapon = strLoopID.IsGuid()
                    ? objXmlWeaponDocument.SelectSingleNode("/chummer/weapons/weapon[id = \"" + strLoopID + "\"]")
                    : objXmlWeaponDocument.SelectSingleNode("/chummer/weapons/weapon[name = \"" + strLoopID + "\"]");

                if (objXmlWeapon != null)
                {
                    Weapon objGearWeapon = new Weapon(_objCharacter)
                    {
                        ParentVehicle = ParentVehicle
                    };
                    int intAddWeaponRating = 0;
                    if (objXmlAddWeapon.Attributes["rating"]?.InnerText != null)
                    {
                        intAddWeaponRating = Convert.ToInt32(objXmlAddWeapon.Attributes["rating"]?.InnerText
                            .CheapReplace("{Rating}", () => Rating.ToString(GlobalOptions.InvariantCultureInfo)), GlobalOptions.InvariantCultureInfo);
                    }
                    objGearWeapon.Create(objXmlWeapon, lstWeapons, blnCreateChildren, blnCreateImprovements,false, intAddWeaponRating);
                    objGearWeapon.ParentID = InternalId;
                    objGearWeapon.Cost = "0";

                    if (Guid.TryParse(objGearWeapon.InternalId, out _guiWeaponID))
                        lstWeapons.Add(objGearWeapon);
                }
            }

            // Add Drone Bodyparts if applicable.
            XmlDocument objXmlVehicleDocument = XmlManager.Load("vehicles.xml");

            // More than one Weapon can be added, so loop through all occurrences.
            foreach (XmlNode xmlAddVehicle in objXmlCyberware.SelectNodes("addvehicle"))
            {
                string strLoopID = xmlAddVehicle.InnerText;
                XmlNode xmlVehicle = strLoopID.IsGuid()
                    ? objXmlVehicleDocument.SelectSingleNode("/chummer/vehicles/vehicle[id = \"" + strLoopID + "\"]")
                    : objXmlVehicleDocument.SelectSingleNode("/chummer/vehicles/vehicle[name = \"" + strLoopID + "\"]");

                if (xmlVehicle != null)
                {
                    Vehicle objVehicle = new Vehicle(_objCharacter);
                    objVehicle.Create(xmlVehicle);
                    objVehicle.ParentID = InternalId;

                    if (Guid.TryParse(objVehicle.InternalId, out _guiVehicleID))
                        lstVehicles.Add(objVehicle);
                }
            }

            /*
             * This needs to be handled separately from usual bonus nodes because:
             * - Children must always inherit the side of their parent(s)
             * - In case of numerical limits, we must be able to apply them separately to each side
             * - Modular cyberlimbs need a constant side regardless of their equip status
             * - In cases where modular mounts might get blocked, we must force the 'ware to the unblocked side
             */
            if (objXmlCyberware["selectside"] != null)
            {
                string strParentSide = Parent?.Location;
                if (!string.IsNullOrEmpty(strParentSide))
                {
                    _strLocation = strParentSide;
                }
                else
                {
                    using (frmSelectSide frmPickSide = new frmSelectSide
                    {
                        Description =
                            string.Format(GlobalOptions.CultureInfo, LanguageManager.GetString("Label_SelectSide"),
                                DisplayNameShort(GlobalOptions.Language))
                    })
                    {
                        string strForcedSide = string.Empty;
                        if (_strForced == "Right" || _strForced == "Left")
                            strForcedSide = _strForced;
                        // TODO: Fix for modular mounts / banned mounts if someone has an amount of limbs different from the default amount
                        if (string.IsNullOrEmpty(strForcedSide) && ParentVehicle == null)
                        {
                            XPathNavigator xpnCyberware = objXmlCyberware.CreateNavigator();
                            ObservableCollection<Cyberware> lstCyberwareToCheck =
                                Parent == null ? _objCharacter.Cyberware : Parent.Children;
                            if (!xpnCyberware.RequirementsMet(_objCharacter, Parent, string.Empty, string.Empty,
                                    string.Empty, "Left") ||
                                (!string.IsNullOrEmpty(BlocksMounts) && lstCyberwareToCheck.Any(x =>
                                    !string.IsNullOrEmpty(x.HasModularMount) && x.Location == "Left" &&
                                    BlocksMounts.Split(',').Contains(x.HasModularMount))) ||
                                (!string.IsNullOrEmpty(HasModularMount) && lstCyberwareToCheck.Any(x =>
                                    !string.IsNullOrEmpty(x.BlocksMounts) && x.Location == "Left" &&
                                    x.BlocksMounts.Split(',').Contains(HasModularMount))))
                                strForcedSide = "Right";
                            else if (!xpnCyberware.RequirementsMet(_objCharacter, Parent, string.Empty, string.Empty,
                                         string.Empty, "Right") ||
                                     (!string.IsNullOrEmpty(BlocksMounts) && lstCyberwareToCheck.Any(x =>
                                         !string.IsNullOrEmpty(x.HasModularMount) && x.Location == "Right" &&
                                         BlocksMounts.Split(',').Contains(x.HasModularMount))) ||
                                     (!string.IsNullOrEmpty(HasModularMount) && lstCyberwareToCheck.Any(x =>
                                         !string.IsNullOrEmpty(x.BlocksMounts) && x.Location == "Right" &&
                                         x.BlocksMounts.Split(',').Contains(HasModularMount))))
                                strForcedSide = "Left";
                        }

                        if (!string.IsNullOrEmpty(strForcedSide))
                            frmPickSide.ForceValue(strForcedSide);
                        else
                            frmPickSide.ShowDialog();

                        // Make sure the dialogue window was not canceled.
                        if (frmPickSide.DialogResult == DialogResult.Cancel)
                        {
                            _guiID = Guid.Empty;
                            return;
                        }

                        _strLocation = frmPickSide.SelectedSide;
                    }
                }
            }

            // If the piece grants a bonus, pass the information to the Improvement Manager.
            // Modular cyberlimbs only get their bonuses applied when they are equipped onto a limb, so we're skipping those here
            if (blnCreateImprovements)
            {
                if (Bonus != null || PairBonus != null)
                {
                    if (!string.IsNullOrEmpty(_strForced) && _strForced != "Left" && _strForced != "Right")
                        ImprovementManager.ForcedValue = _strForced;

                    if (Bonus != null && !ImprovementManager.CreateImprovements(_objCharacter, objSource,
                        _guiID.ToString("D", GlobalOptions.InvariantCultureInfo), Bonus, Rating, DisplayNameShort(GlobalOptions.Language)))
                    {
                        _guiID = Guid.Empty;
                        return;
                    }

                    if (!string.IsNullOrEmpty(ImprovementManager.SelectedValue) && string.IsNullOrEmpty(_strExtra))
                        _strExtra = ImprovementManager.SelectedValue;

                    if (!string.IsNullOrEmpty(ImprovementManager.SelectedValue) && string.IsNullOrEmpty(_strExtra))
                        _strExtra = ImprovementManager.SelectedValue;

                    if (PairBonus != null)
                    {
                        // This cyberware should not be included in the count to make things easier.
                        List<Cyberware> lstPairableCyberwares = _objCharacter.Cyberware.DeepWhere(x => x.Children,
                            x => x != this && IncludePair.Contains(x.Name) && x.Extra == Extra &&
                                 x.IsModularCurrentlyEquipped).ToList();
                        int intCount = lstPairableCyberwares.Count;
                        // Need to use slightly different logic if this cyberware has a location (Left or Right) and only pairs with itself because Lefts can only be paired with Rights and Rights only with Lefts
                        if (!string.IsNullOrEmpty(Location) && IncludePair.All(x => x == Name))
                        {
                            intCount = 0;
                            foreach (Cyberware objPairableCyberware in lstPairableCyberwares)
                            {
                                if (objPairableCyberware.Location != Location)
                                    // We have found a cyberware with which this one could be paired, so increase count by 1
                                    intCount += 1;
                                else
                                    // We have found a cyberware that would serve as a pair to another cyberware instead of this one, so decrease count by 1
                                    intCount -= 1;
                            }

                            // If we have at least one cyberware with which we could pair, set count to 1 so that it passes the modulus to add the PairBonus. Otherwise, set to 0 so it doesn't pass.
                            intCount = intCount > 0 ? 1 : 0;
                        }

                        if ((intCount & 1) == 1 && !ImprovementManager.CreateImprovements(_objCharacter, objSource,
                            _guiID.ToString("D", GlobalOptions.InvariantCultureInfo) + "Pair", PairBonus, Rating, DisplayNameShort(GlobalOptions.Language)))
                        {
                            _guiID = Guid.Empty;
                            return;
                        }
                    }
                }

                if (WirelessBonus != null || WirelessPairBonus != null)
                {
                    ToggleWirelessBonuses(_blnWirelessOn);
                }
            }

            // Retrieve the Bioware or Cyberware ESS Cost Multiplier. Bioware Modifiers do not apply to Genetech.
            if (GetNode()?["forcegrade"]?.InnerText != "None")
            {
                // Apply the character's Cyberware Essence cost multiplier if applicable.
                if (_objImprovementSource == Improvement.ImprovementSource.Cyberware)
                {
                    if (ImprovementManager.ValueOf(_objCharacter,
                            Improvement.ImprovementType.CyberwareEssCostNonRetroactive) != 0)
                    {
                        decimal decMultiplier = 1;
                        decMultiplier = _objCharacter.Improvements
                            .Where(objImprovement =>
                                objImprovement.ImproveType ==
                                Improvement.ImprovementType.CyberwareEssCostNonRetroactive && objImprovement.Enabled)
                            .Aggregate(decMultiplier,
                                (current, objImprovement) =>
                                    current - (1m - objImprovement.Value / 100m));
                        _decExtraESSAdditiveMultiplier -= 1.0m - decMultiplier;
                    }

                    if (ImprovementManager.ValueOf(_objCharacter,
                            Improvement.ImprovementType.CyberwareTotalEssMultiplierNonRetroactive) != 0)
                    {
                        foreach (Improvement objImprovement in _objCharacter.Improvements.Where(x =>
                            x.Enabled && x.ImproveType ==
                            Improvement.ImprovementType.CyberwareTotalEssMultiplierNonRetroactive))
                        {
                            _decExtraESSMultiplicativeMultiplier *=
                                objImprovement.Value / 100m;
                        }
                    }
                }

                // Apply the character's Bioware Essence cost multiplier if applicable.
                else if (_objImprovementSource == Improvement.ImprovementSource.Bioware)
                {
                    if (ImprovementManager.ValueOf(_objCharacter,
                            Improvement.ImprovementType.BiowareEssCostNonRetroactive) != 0)
                    {
                        decimal decMultiplier = 1;
                        decMultiplier = _objCharacter.Improvements
                            .Where(objImprovement =>
                                objImprovement.ImproveType ==
                                Improvement.ImprovementType.BiowareEssCostNonRetroactive && objImprovement.Enabled)
                            .Aggregate(decMultiplier,
                                (current, objImprovement) =>
                                    current - (1m - objImprovement.Value / 100m));
                        _decExtraESSAdditiveMultiplier -= 1.0m - decMultiplier;
                    }

                    if (ImprovementManager.ValueOf(_objCharacter,
                            Improvement.ImprovementType.BiowareTotalEssMultiplierNonRetroactive) != 0)
                    {
                        foreach (Improvement objImprovement in _objCharacter.Improvements.Where(x =>
                            x.Enabled && x.ImproveType ==
                            Improvement.ImprovementType.BiowareTotalEssMultiplierNonRetroactive))
                        {
                            _decExtraESSMultiplicativeMultiplier *=
                                objImprovement.Value / 100m;
                        }
                    }
                }
            }

            if (blnCreateChildren)
                CreateChildren(objXmlCyberware, objGrade, lstWeapons, lstVehicles, blnCreateImprovements);

            if (!string.IsNullOrEmpty(_strPlugsIntoModularMount))
                ChangeModularEquip(false);
        }

        private void CreateChildren(XmlNode objParentNode, Grade objGrade, IList<Weapon> lstWeapons,
            IList<Vehicle> objVehicles, bool blnCreateImprovements = true)
        {
            // If we've just added a new base item, see if there are any subsystems that should automatically be added.
            XmlNode xmlSubsystemsNode = objParentNode["subsystems"];
            if (xmlSubsystemsNode != null)
            {
                // Load Cyberware subsystems first
                using (XmlNodeList objXmlSubSystemNameList = xmlSubsystemsNode.SelectNodes("cyberware"))
                    if (objXmlSubSystemNameList?.Count > 0)
                    {
                        XmlDocument objXmlDocument = XmlManager.Load("cyberware.xml");
                        foreach (XmlNode objXmlSubsystemNode in objXmlSubSystemNameList)
                        {
                            XmlNode objXmlSubsystem = objXmlDocument.SelectSingleNode(
                                "/chummer/cyberwares/cyberware[name = \"" + objXmlSubsystemNode["name"]?.InnerText +
                                "\"]");

                            if (objXmlSubsystem != null)
                            {
                                Cyberware objSubsystem = new Cyberware(_objCharacter);
                                int intSubSystemRating = Convert.ToInt32(objXmlSubsystemNode["rating"]?.InnerText, GlobalOptions.InvariantCultureInfo);
                                objSubsystem.Create(objXmlSubsystem, objGrade, Improvement.ImprovementSource.Cyberware,
                                    intSubSystemRating, lstWeapons, objVehicles, blnCreateImprovements, true,
                                    objXmlSubsystemNode["forced"]?.InnerText ?? string.Empty, this);
                                objSubsystem.ParentID = InternalId;
                                objSubsystem.Cost = "0";
                                // If the <subsystem> tag itself contains extra children, add those, too
                                objSubsystem.CreateChildren(objXmlSubsystemNode, objGrade, lstWeapons, objVehicles,
                                    blnCreateImprovements);

                                _lstChildren.Add(objSubsystem);
                            }
                        }
                    }

                // Load bioware subsystems next
                using (XmlNodeList objXmlSubSystemNameList = xmlSubsystemsNode.SelectNodes("bioware"))
                    if (objXmlSubSystemNameList?.Count > 0)
                    {
                        XmlDocument objXmlDocument = XmlManager.Load("bioware.xml");
                        foreach (XmlNode objXmlSubsystemNode in objXmlSubSystemNameList)
                        {
                            XmlNode objXmlSubsystem = objXmlDocument.SelectSingleNode(
                                "/chummer/biowares/bioware[name = \"" + objXmlSubsystemNode["name"]?.InnerText + "\"]");

                            if (objXmlSubsystem != null)
                            {
                                Cyberware objSubsystem = new Cyberware(_objCharacter);
                                int intSubSystemRating = Convert.ToInt32(objXmlSubsystemNode["rating"]?.InnerText, GlobalOptions.InvariantCultureInfo);
                                objSubsystem.Create(objXmlSubsystem, objGrade, Improvement.ImprovementSource.Bioware,
                                    intSubSystemRating, lstWeapons, objVehicles, blnCreateImprovements, true,
                                    objXmlSubsystemNode["forced"]?.InnerText ?? string.Empty, this);
                                objSubsystem.ParentID = InternalId;
                                objSubsystem.Cost = "0";
                                // If the <subsystem> tag itself contains extra children, add those, too
                                objSubsystem.CreateChildren(objXmlSubsystemNode, objGrade, lstWeapons, objVehicles,
                                    blnCreateImprovements);

                                _lstChildren.Add(objSubsystem);
                            }
                        }
                    }
            }

            // Check to see if there are any child elements.
            if (objParentNode["gears"] != null)
            {
                XmlDocument objXmlGearDocument = XmlManager.Load("gear.xml");

                XmlNodeList objXmlGearList = objParentNode["gears"].SelectNodes("usegear");
                if (objXmlGearList?.Count > 0)
                {
                    IList<Weapon> lstChildWeapons = new List<Weapon>();
                    foreach (XmlNode objXmlVehicleGear in objXmlGearList)
                    {
                        Gear objGear = new Gear(_objCharacter);
                        if (!objGear.CreateFromNode(objXmlGearDocument, objXmlVehicleGear, lstChildWeapons,
                            blnCreateImprovements))
                            continue;
                        foreach (Weapon objWeapon in lstChildWeapons)
                        {
                            objWeapon.ParentID = InternalId;
                        }

                        objGear.Parent = this;
                        objGear.ParentID = InternalId;
                        Gear.Add(objGear);
                        lstChildWeapons.AddRange(lstWeapons);
                    }

                    lstWeapons.AddRange(lstChildWeapons);
                }
            }
        }

        /// <summary>
        /// Save the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        public void Save(XmlTextWriter objWriter)
        {
            if (objWriter == null)
                return;
            objWriter.WriteStartElement("cyberware");
            objWriter.WriteElementString("guid", InternalId);
            objWriter.WriteElementString("sourceid", SourceIDString);
            objWriter.WriteElementString("name", _strName);
            objWriter.WriteElementString("category", _strCategory);
            objWriter.WriteElementString("limbslot", _strLimbSlot);
            objWriter.WriteElementString("limbslotcount", _strLimbSlotCount);
            objWriter.WriteElementString("inheritattributes", _blnInheritAttributes.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("ess", _strESS);
            objWriter.WriteElementString("capacity", _strCapacity);
            objWriter.WriteElementString("avail", _strAvail);
            objWriter.WriteElementString("cost", _strCost);
            objWriter.WriteElementString("source", _strSource);
            objWriter.WriteElementString("page", _strPage);
            objWriter.WriteElementString("parentid", _strParentID);
            objWriter.WriteElementString("hasmodularmount", _strHasModularMount);
            objWriter.WriteElementString("plugsintomodularmount", _strPlugsIntoModularMount);
            objWriter.WriteElementString("blocksmounts", _strBlocksMounts);
            objWriter.WriteElementString("forced", _strForced);
            objWriter.WriteElementString("rating", _intRating.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("minrating", _strMinRating);
            objWriter.WriteElementString("maxrating", _strMaxRating);
            objWriter.WriteElementString("ratinglabel", _strRatingLabel);
            objWriter.WriteElementString("subsystems", _strAllowSubsystems);
            objWriter.WriteElementString("wirelesson", WirelessOn.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("grade", _objGrade.Name);
            objWriter.WriteElementString("location", _strLocation);
            objWriter.WriteElementString("extra", _strExtra);
            objWriter.WriteElementString("suite", _blnSuite.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("stolen", _blnStolen.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("essdiscount",
                _intEssenceDiscount.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("extraessadditivemultiplier",
                _decExtraESSAdditiveMultiplier.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("extraessmultiplicativemultiplier",
                _decExtraESSMultiplicativeMultiplier.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("forcegrade", _strForceGrade);
            objWriter.WriteElementString("matrixcmfilled",
                _intMatrixCMFilled.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("prototypetranshuman", _blnPrototypeTranshuman.ToString(GlobalOptions.InvariantCultureInfo));
            if (_nodBonus != null)
                objWriter.WriteRaw(_nodBonus.OuterXml);
            else
                objWriter.WriteElementString("bonus", string.Empty);
            if (_nodPairBonus != null)
                objWriter.WriteRaw(_nodPairBonus.OuterXml);
            else
                objWriter.WriteElementString("pairbonus", string.Empty);
            if (_nodWirelessBonus != null)
                objWriter.WriteRaw(_nodWirelessBonus.OuterXml);
            else
                objWriter.WriteElementString("wirelessbonus", string.Empty);
            if (_nodWirelessPairBonus != null)
                objWriter.WriteRaw(_nodWirelessPairBonus.OuterXml);
            else
                objWriter.WriteElementString("wirelesspairbonus", string.Empty);
            if (_nodAllowGear != null)
                objWriter.WriteRaw(_nodAllowGear.OuterXml);
            objWriter.WriteElementString("improvementsource", _objImprovementSource.ToString());
            if (_guiWeaponID != Guid.Empty)
                objWriter.WriteElementString("weaponguid", _guiWeaponID.ToString("D", GlobalOptions.InvariantCultureInfo));
            if (_guiVehicleID != Guid.Empty)
                objWriter.WriteElementString("vehicleguid", _guiVehicleID.ToString("D", GlobalOptions.InvariantCultureInfo));

            #region PairInclude

            objWriter.WriteStartElement("pairinclude");
            foreach (string strName in _lstIncludeInPairBonus)
                objWriter.WriteElementString("name", strName);
            objWriter.WriteEndElement();

            #endregion

            #region WirelessPairInclude

            objWriter.WriteStartElement("wirelesspairinclude");
            foreach (string strName in _lstIncludeInWirelessPairBonus)
                objWriter.WriteElementString("name", strName);
            objWriter.WriteEndElement();

            #endregion

            #region Children

            objWriter.WriteStartElement("children");
            foreach (Cyberware objChild in _lstChildren)
            {
                objChild.Save(objWriter);
            }

            objWriter.WriteEndElement();

            #endregion

            #region Gear

            if (_lstGear.Count > 0)
            {
                objWriter.WriteStartElement("gears");
                foreach (Gear objGear in _lstGear)
                {
                    objGear.Save(objWriter);
                }

                objWriter.WriteEndElement();
            }

            #endregion

            objWriter.WriteElementString("notes", _strNotes);
            objWriter.WriteElementString("discountedcost", _blnDiscountCost.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("addtoparentess", _blnAddToParentESS.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("addtoparentcapacity", _blnAddToParentCapacity.ToString(GlobalOptions.InvariantCultureInfo));

            objWriter.WriteElementString("active", this.IsActiveCommlink(_objCharacter).ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("homenode", this.IsHomeNode(_objCharacter).ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("devicerating", _strDeviceRating);
            objWriter.WriteElementString("programlimit", _strProgramLimit);
            objWriter.WriteElementString("overclocked", _strOverclocked);
            objWriter.WriteElementString("attack", _strAttack);
            objWriter.WriteElementString("sleaze", _strSleaze);
            objWriter.WriteElementString("dataprocessing", _strDataProcessing);
            objWriter.WriteElementString("firewall", _strFirewall);
            objWriter.WriteElementString("attributearray", _strAttributeArray);
            objWriter.WriteElementString("modattack", _strModAttack);
            objWriter.WriteElementString("modsleaze", _strModSleaze);
            objWriter.WriteElementString("moddataprocessing", _strModDataProcessing);
            objWriter.WriteElementString("modfirewall", _strModFirewall);
            objWriter.WriteElementString("modattributearray", _strModAttributeArray);
            objWriter.WriteElementString("canswapattributes", _blnCanSwapAttributes.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("sortorder", _intSortOrder.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteEndElement();

            if (string.IsNullOrEmpty(ParentID))
                _objCharacter.SourceProcess(_strSource);
        }

        /// <summary>
        /// Load the CharacterAttribute from the XmlNode.
        /// </summary>
        /// <param name="objNode">XmlNode to load.</param>
        /// <param name="blnCopy">Whether this is a copy of an existing cyberware being loaded.</param>
        public void Load(XmlNode objNode, bool blnCopy = false)
        {
            objNode.TryGetStringFieldQuickly("name", ref _strName);
            if (!objNode.TryGetGuidFieldQuickly("sourceid", ref _guiSourceID))
            {
                XmlNode node = GetNode(GlobalOptions.Language);
                node?.TryGetGuidFieldQuickly("id", ref _guiSourceID);
            }

            if (blnCopy)
            {
                _guiID = Guid.NewGuid();
            }
            else
                objNode.TryGetField("guid", Guid.TryParse, out _guiID);

            objNode.TryGetStringFieldQuickly("category", ref _strCategory);
            if (objNode["improvementsource"] != null)
            {
                _objImprovementSource = Improvement.ConvertToImprovementSource(objNode["improvementsource"].InnerText);
                _objCachedMyXmlNode = null;
            }

            // Legacy shim for mis-formatted name of Reflex Recorder
            if (_strName == "Reflex Recorder (Skill)" && _objCharacter.LastSavedVersion <= new Version(5, 198, 31))
            {
                // This step is needed in case there's a custom data file that has the name "Reflex Recorder (Skill)", in which case we wouldn't want to rename the 'ware
                XmlNode xmlReflexRecorderNode = _objImprovementSource == Improvement.ImprovementSource.Bioware
                    ? XmlManager.Load("bioware.xml")
                        .SelectSingleNode("/chummer/biowares/bioware[name = \"Reflex Recorder (Skill)\"]")
                    : XmlManager.Load("cyberware.xml")
                        .SelectSingleNode("/chummer/cyberwares/cyberware[name = \"Reflex Recorder (Skill)\"]");
                if (xmlReflexRecorderNode == null)
                    _strName = "Reflex Recorder";
            }

            objNode.TryGetInt32FieldQuickly("matrixcmfilled", ref _intMatrixCMFilled);
            objNode.TryGetStringFieldQuickly("limbslot", ref _strLimbSlot);
            objNode.TryGetStringFieldQuickly("limbslotcount", ref _strLimbSlotCount);
            objNode.TryGetBoolFieldQuickly("inheritattributes", ref _blnInheritAttributes);
            objNode.TryGetStringFieldQuickly("ess", ref _strESS);
            objNode.TryGetStringFieldQuickly("capacity", ref _strCapacity);
            objNode.TryGetStringFieldQuickly("avail", ref _strAvail);
            objNode.TryGetStringFieldQuickly("cost", ref _strCost);
            objNode.TryGetStringFieldQuickly("source", ref _strSource);
            objNode.TryGetStringFieldQuickly("page", ref _strPage);
            objNode.TryGetStringFieldQuickly("parentid", ref _strParentID);
            if (!objNode.TryGetStringFieldQuickly("hasmodularmount", ref _strHasModularMount))
                _strHasModularMount = GetNode()?["hasmodularmount"]?.InnerText ?? string.Empty;
            if (!objNode.TryGetStringFieldQuickly("plugsintomodularmount", ref _strPlugsIntoModularMount))
                _strPlugsIntoModularMount = GetNode()?["plugsintomodularmount"]?.InnerText ?? string.Empty;
            if (!objNode.TryGetStringFieldQuickly("blocksmounts", ref _strBlocksMounts))
                _strBlocksMounts = GetNode()?["blocksmounts"]?.InnerText ?? string.Empty;
            objNode.TryGetStringFieldQuickly("forced", ref _strForced);
            objNode.TryGetInt32FieldQuickly("rating", ref _intRating);
            objNode.TryGetStringFieldQuickly("minrating", ref _strMinRating);
            objNode.TryGetStringFieldQuickly("maxrating", ref _strMaxRating);
            objNode.TryGetStringFieldQuickly("ratinglabel", ref _strRatingLabel);
            objNode.TryGetInt32FieldQuickly("sortorder", ref _intSortOrder);
            // Legacy shim for old-form customized attribute
            if ((s_StrengthCombinedStrings.Contains(Name) || s_AgilityCombinedStrings.Contains(Name)) &&
                int.TryParse(MaxRatingString, out int _))
            {
                XmlNode objMyXmlNode = GetNode();
                if (objMyXmlNode != null)
                {
                    objMyXmlNode.TryGetStringFieldQuickly("minrating", ref _strMinRating);
                    objMyXmlNode.TryGetStringFieldQuickly("rating", ref _strMaxRating);
                    objMyXmlNode.TryGetStringFieldQuickly("avail", ref _strAvail);
                    objMyXmlNode.TryGetStringFieldQuickly("cost", ref _strCost);
                }
            }

            objNode.TryGetStringFieldQuickly("subsystems", ref _strAllowSubsystems);
            if (objNode["grade"] != null)
                _objGrade = Grade.ConvertToCyberwareGrade(objNode["grade"].InnerText, _objImprovementSource, _objCharacter);
            objNode.TryGetStringFieldQuickly("location", ref _strLocation);
            if (!objNode.TryGetStringFieldQuickly("extra", ref _strExtra) && _strLocation != "Left" &&
                _strLocation != "Right")
            {
                _strExtra = _strLocation;
                _strLocation = string.Empty;
            }

            objNode.TryGetBoolFieldQuickly("suite", ref _blnSuite);
            objNode.TryGetBoolFieldQuickly("stolen", ref _blnStolen);
            objNode.TryGetInt32FieldQuickly("essdiscount", ref _intEssenceDiscount);
            objNode.TryGetDecFieldQuickly("extraessadditivemultiplier", ref _decExtraESSAdditiveMultiplier);
            objNode.TryGetDecFieldQuickly("extraessmultiplicativemultiplier", ref _decExtraESSMultiplicativeMultiplier);
            objNode.TryGetStringFieldQuickly("forcegrade", ref _strForceGrade);
            if (_objCharacter.PrototypeTranshuman > 0 &&  SourceType == Improvement.ImprovementSource.Bioware)
                objNode.TryGetBoolFieldQuickly("prototypetranshuman", ref _blnPrototypeTranshuman);
            _nodBonus = objNode["bonus"];
            _nodPairBonus = objNode["pairbonus"];
            XmlNode xmlPairIncludeNode = objNode["pairinclude"];
            if (xmlPairIncludeNode == null)
            {
                xmlPairIncludeNode = GetNode()?["pairinclude"];
                _lstIncludeInPairBonus.Add(Name);
            }

            if (xmlPairIncludeNode != null)
            {
                using (XmlNodeList xmlNameList = xmlPairIncludeNode.SelectNodes("name"))
                    if (xmlNameList != null)
                        foreach (XmlNode xmlNameNode in xmlNameList)
                            _lstIncludeInPairBonus.Add(xmlNameNode.InnerText);
            }

            _nodWirelessPairBonus = objNode["wirelesspairbonus"];
            xmlPairIncludeNode = objNode["wirelesspairinclude"];
            if (xmlPairIncludeNode == null)
            {
                xmlPairIncludeNode = GetNode()?["wirelesspairinclude"];
                _lstIncludeInWirelessPairBonus.Add(Name);
            }

            if (xmlPairIncludeNode != null)
            {
                using (XmlNodeList xmlNameList = xmlPairIncludeNode.SelectNodes("name"))
                    if (xmlNameList != null)
                        foreach (XmlNode xmlNameNode in xmlNameList)
                            _lstIncludeInWirelessPairBonus.Add(xmlNameNode.InnerText);
            }

            _nodWirelessBonus = objNode["wirelessbonus"];
            if (!objNode.TryGetBoolFieldQuickly("wirelesson", ref _blnWirelessOn))
            {
                _blnWirelessOn = false;
            }

            _nodAllowGear = objNode["allowgear"];
            // Legacy Sweep
            if (_strForceGrade != "None" && IsGeneware)
            {
                _strForceGrade = GetNode()?["forcegrade"]?.InnerText;
                if (!string.IsNullOrEmpty(_strForceGrade))
                    _objGrade = Grade.ConvertToCyberwareGrade(_strForceGrade, _objImprovementSource, _objCharacter);
            }

            if (objNode["weaponguid"] != null)
            {
                if (!Guid.TryParse(objNode["weaponguid"].InnerText, out _guiWeaponID))
                    _guiWeaponID = Guid.Empty;
            }

            if (objNode["vehicleguid"] != null)
            {
                if (!Guid.TryParse(objNode["vehicleguid"].InnerText, out _guiVehicleID))
                    _guiVehicleID = Guid.Empty;
            }

            if (objNode.InnerXml.Contains("<cyberware>"))
            {
                XmlNodeList nodChildren = objNode.SelectNodes("children/cyberware");
                foreach (XmlNode nodChild in nodChildren)
                {
                    Cyberware objChild = new Cyberware(_objCharacter);
                    objChild.Load(nodChild, blnCopy);
                    _lstChildren.Add(objChild);
                }
            }

            if (objNode.InnerXml.Contains("<gears>"))
            {
                XmlNodeList nodChildren = objNode.SelectNodes("gears/gear");
                foreach (XmlNode nodChild in nodChildren)
                {
                    Gear objGear = new Gear(_objCharacter);
                    objGear.Load(nodChild, blnCopy);
                    _lstGear.Add(objGear);
                }
            }

            objNode.TryGetStringFieldQuickly("notes", ref _strNotes);
            objNode.TryGetBoolFieldQuickly("discountedcost", ref _blnDiscountCost);
            if (objNode["addtoparentess"] != null)
            {
                if (bool.TryParse(objNode["addtoparentess"].InnerText, out bool blnTmp))
                {
                    _blnAddToParentESS = blnTmp;
                }
            }
            else
                _blnAddToParentESS = GetNode()?["addtoparentess"] != null;

            if (objNode["addtoparentcapacity"] != null)
            {
                if (bool.TryParse(objNode["addtoparentcapacity"].InnerText, out bool blnTmp))
                {
                    _blnAddToParentCapacity = blnTmp;
                }
            }
            else
                _blnAddToParentCapacity = GetNode()?["addtoparentcapacity"] != null;

            bool blnIsActive = false;
            if (objNode.TryGetBoolFieldQuickly("active", ref blnIsActive) && blnIsActive)
                this.SetActiveCommlink(_objCharacter, true);
            if (blnCopy)
            {
                this.SetHomeNode(_objCharacter, false);
            }
            else
            {
                bool blnIsHomeNode = false;
                if (objNode.TryGetBoolFieldQuickly("homenode", ref blnIsHomeNode) && blnIsHomeNode)
                {
                    this.SetHomeNode(_objCharacter, true);
                }
            }

            if (!objNode.TryGetStringFieldQuickly("devicerating", ref _strDeviceRating))
                GetNode()?.TryGetStringFieldQuickly("devicerating", ref _strDeviceRating);
            if (!objNode.TryGetStringFieldQuickly("programlimit", ref _strProgramLimit))
                GetNode()?.TryGetStringFieldQuickly("programs", ref _strProgramLimit);
            objNode.TryGetStringFieldQuickly("overclocked", ref _strOverclocked);
            if (!objNode.TryGetStringFieldQuickly("attack", ref _strAttack))
                GetNode()?.TryGetStringFieldQuickly("attack", ref _strAttack);
            if (!objNode.TryGetStringFieldQuickly("sleaze", ref _strSleaze))
                GetNode()?.TryGetStringFieldQuickly("sleaze", ref _strSleaze);
            if (!objNode.TryGetStringFieldQuickly("dataprocessing", ref _strDataProcessing))
                GetNode()?.TryGetStringFieldQuickly("dataprocessing", ref _strDataProcessing);
            if (!objNode.TryGetStringFieldQuickly("firewall", ref _strFirewall))
                GetNode()?.TryGetStringFieldQuickly("firewall", ref _strFirewall);
            if (!objNode.TryGetStringFieldQuickly("attributearray", ref _strAttributeArray))
                GetNode()?.TryGetStringFieldQuickly("attributearray", ref _strAttributeArray);
            if (!objNode.TryGetStringFieldQuickly("modattack", ref _strModAttack))
                GetNode()?.TryGetStringFieldQuickly("modattack", ref _strModAttack);
            if (!objNode.TryGetStringFieldQuickly("modsleaze", ref _strModSleaze))
                GetNode()?.TryGetStringFieldQuickly("modsleaze", ref _strModSleaze);
            if (!objNode.TryGetStringFieldQuickly("moddataprocessing", ref _strModDataProcessing))
                GetNode()?.TryGetStringFieldQuickly("moddataprocessing", ref _strModDataProcessing);
            if (!objNode.TryGetStringFieldQuickly("modfirewall", ref _strModFirewall))
                GetNode()?.TryGetStringFieldQuickly("modfirewall", ref _strModFirewall);
            if (!objNode.TryGetStringFieldQuickly("modattributearray", ref _strModAttributeArray))
                GetNode()?.TryGetStringFieldQuickly("modattributearray", ref _strModAttributeArray);

            if (blnCopy)
            {
                if (Bonus != null || WirelessBonus != null || PairBonus != null || WirelessPairBonus != null)
                {
                    if (!string.IsNullOrEmpty(_strForced) && _strForced != "Left" && _strForced != "Right")
                        ImprovementManager.ForcedValue = _strForced;

                    if (Bonus != null)
                    {
                        ImprovementManager.CreateImprovements(_objCharacter, _objImprovementSource,
                            _guiID.ToString("D", GlobalOptions.InvariantCultureInfo), Bonus, Rating, DisplayNameShort(GlobalOptions.Language));
                    }

                    if (!string.IsNullOrEmpty(ImprovementManager.SelectedValue) && string.IsNullOrEmpty(_strExtra))
                        _strExtra = ImprovementManager.SelectedValue;

                    if (WirelessBonus != null)
                    {
                        ImprovementManager.CreateImprovements(_objCharacter, _objImprovementSource,
                            _guiID.ToString("D", GlobalOptions.InvariantCultureInfo), WirelessBonus, Rating, DisplayNameShort(GlobalOptions.Language));
                    }

                    if (!string.IsNullOrEmpty(ImprovementManager.SelectedValue) && string.IsNullOrEmpty(_strExtra))
                        _strExtra = ImprovementManager.SelectedValue;

                    if (WirelessPairBonus != null && WirelessOn)
                    {
                        // This cyberware should not be included in the count to make things easier.
                        List<Cyberware> lstPairableCyberwares = _objCharacter.Cyberware.DeepWhere(x => x.Children,
                            x => x != this && IncludeWirelessPair.Contains(x.Name) && x.Extra == Extra &&
                                 x.IsModularCurrentlyEquipped).ToList();
                        int intCount = lstPairableCyberwares.Count;
                        // Need to use slightly different logic if this cyberware has a location (Left or Right) and only pairs with itself because Lefts can only be paired with Rights and Rights only with Lefts
                        if (!string.IsNullOrEmpty(Location) && IncludeWirelessPair.All(x => x == Name))
                        {
                            intCount = 0;
                            foreach (Cyberware objPairableCyberware in lstPairableCyberwares)
                            {
                                if (objPairableCyberware.Location != Location)
                                    // We have found a cyberware with which this one could be paired, so increase count by 1
                                    intCount += 1;
                                else
                                    // We have found a cyberware that would serve as a pair to another cyberware instead of this one, so decrease count by 1
                                    intCount -= 1;
                            }

                            // If we have at least one cyberware with which we could pair, set count to 1 so that it passes the modulus to add the PairBonus. Otherwise, set to 0 so it doesn't pass.
                            intCount = intCount > 0 ? 1 : 0;
                        }

                        if (intCount % 2 == 1)
                        {
                            ImprovementManager.CreateImprovements(_objCharacter, SourceType,
                                InternalId + "WirelessPair", WirelessPairBonus, Rating, DisplayNameShort(GlobalOptions.Language));
                        }
                    }
                    else if (PairBonus != null)
                    {
                        // This cyberware should not be included in the count to make things easier.
                        List<Cyberware> lstPairableCyberwares = _objCharacter.Cyberware.DeepWhere(x => x.Children,
                            x => x != this && IncludePair.Contains(x.Name) && x.Extra == Extra &&
                                 x.IsModularCurrentlyEquipped).ToList();
                        int intCount = lstPairableCyberwares.Count;
                        // Need to use slightly different logic if this cyberware has a location (Left or Right) and only pairs with itself because Lefts can only be paired with Rights and Rights only with Lefts
                        if (!string.IsNullOrEmpty(Location) && IncludePair.All(x => x == Name))
                        {
                            intCount = 0;
                            foreach (Cyberware objPairableCyberware in lstPairableCyberwares)
                            {
                                if (objPairableCyberware.Location != Location)
                                    // We have found a cyberware with which this one could be paired, so increase count by 1
                                    intCount += 1;
                                else
                                    // We have found a cyberware that would serve as a pair to another cyberware instead of this one, so decrease count by 1
                                    intCount -= 1;
                            }

                            // If we have at least one cyberware with which we could pair, set count to 1 so that it passes the modulus to add the PairBonus. Otherwise, set to 0 so it doesn't pass.
                            intCount = intCount > 0 ? 1 : 0;
                        }

                        if ((intCount & 1) == 1)
                        {
                            ImprovementManager.CreateImprovements(_objCharacter, SourceType, InternalId + "Pair",
                                PairBonus, Rating, DisplayNameShort(GlobalOptions.Language));
                        }
                    }
                }

                if (!IsModularCurrentlyEquipped)
                {
                    ChangeModularEquip(false);
                }
            }
        }

        /// <summary>
        /// Print the object's XML to the XmlWriter.
        /// </summary>obv
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        /// <param name="objCulture">Culture in which to print.</param>
        /// <param name="strLanguageToPrint">Language in which to print</param>
        public void Print(XmlTextWriter objWriter, CultureInfo objCulture, string strLanguageToPrint)
        {
            if (objWriter == null)
                return;
            objWriter.WriteStartElement("cyberware");
            if (string.IsNullOrWhiteSpace(LimbSlot) && _strCategory != "Cyberlimb")
                objWriter.WriteElementString("name", DisplayNameShort(strLanguageToPrint));
            else
            {
                string strSpace = LanguageManager.GetString("String_Space", strLanguageToPrint);
                int intLimit = (TotalStrength * 2 + _objCharacter.BOD.TotalValue + _objCharacter.REA.TotalValue + 2) /
                               3;
                objWriter.WriteElementString("name",
                    DisplayNameShort(strLanguageToPrint) + strSpace + '(' +
                    _objCharacter.AGI.GetDisplayAbbrev(strLanguageToPrint) + strSpace +
                    TotalAgility.ToString(objCulture) + ',' + strSpace +
                    _objCharacter.STR.GetDisplayAbbrev(strLanguageToPrint) + strSpace +
                    TotalStrength.ToString(objCulture) + ',' + strSpace +
                    LanguageManager.GetString("String_LimitPhysicalShort", strLanguageToPrint) + strSpace +
                    intLimit.ToString(objCulture) + ')');
            }

            objWriter.WriteElementString("category", DisplayCategory(strLanguageToPrint));

            objWriter.WriteElementString("ess",
                CalculatedESS.ToString(_objCharacter.Options.EssenceFormat, objCulture));
            objWriter.WriteElementString("capacity", Capacity);
            objWriter.WriteElementString("avail", TotalAvail(objCulture, strLanguageToPrint));
            objWriter.WriteElementString("cost", TotalCost.ToString(_objCharacter.Options.NuyenFormat, objCulture));
            objWriter.WriteElementString("owncost", OwnCost.ToString(_objCharacter.Options.NuyenFormat, objCulture));
            objWriter.WriteElementString("source", CommonFunctions.LanguageBookShort(Source, strLanguageToPrint));
            objWriter.WriteElementString("page", DisplayPage(strLanguageToPrint));
            objWriter.WriteElementString("rating", Rating.ToString(objCulture));
            objWriter.WriteElementString("minrating", MinRating.ToString(objCulture));
            objWriter.WriteElementString("maxrating", MaxRating.ToString(objCulture));
            objWriter.WriteElementString("ratinglabel", RatingLabel);
            objWriter.WriteElementString("allowsubsystems", AllowedSubsystems);
            objWriter.WriteElementString("wirelesson", WirelessOn.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("grade", Grade.DisplayName(strLanguageToPrint));
            objWriter.WriteElementString("location", Location);
            objWriter.WriteElementString("extra", LanguageManager.TranslateExtra(Extra, strLanguageToPrint));
            objWriter.WriteElementString("improvementsource", SourceType.ToString());
            if (Gear.Count > 0)
            {
                objWriter.WriteStartElement("gears");
                foreach (Gear objGear in Gear)
                {
                    objGear.Print(objWriter, objCulture, strLanguageToPrint);
                }

                objWriter.WriteEndElement();
            }

            objWriter.WriteStartElement("children");
            foreach (Cyberware objChild in Children)
            {
                objChild.Print(objWriter, objCulture, strLanguageToPrint);
            }

            objWriter.WriteEndElement();
            if (_objCharacter.Options.PrintNotes)
                objWriter.WriteElementString("notes", Notes);
            objWriter.WriteElementString("iscommlink", IsCommlink.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("active", this.IsActiveCommlink(_objCharacter).ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("homenode", this.IsHomeNode(_objCharacter).ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("attack", this.GetTotalMatrixAttribute("Attack").ToString(objCulture));
            objWriter.WriteElementString("sleaze", this.GetTotalMatrixAttribute("Sleaze").ToString(objCulture));
            objWriter.WriteElementString("dataprocessing",
                this.GetTotalMatrixAttribute("Data Processing").ToString(objCulture));
            objWriter.WriteElementString("firewall", this.GetTotalMatrixAttribute("Firewall").ToString(objCulture));
            objWriter.WriteElementString("devicerating",
                this.GetTotalMatrixAttribute("Device Rating").ToString(objCulture));
            objWriter.WriteElementString("programlimit",
                this.GetTotalMatrixAttribute("Program Limit").ToString(objCulture));
            objWriter.WriteEndElement();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Internal identifier which will be used to identify this piece of Cyberware in the Improvement system.
        /// </summary>
        public string InternalId => _guiID.ToString("D", GlobalOptions.InvariantCultureInfo);

        /// <summary>
        /// Guid of a Cyberware Weapon.
        /// </summary>
        public string WeaponID
        {
            get => _guiWeaponID.ToString("D", GlobalOptions.InvariantCultureInfo);
            set
            {
                if (Guid.TryParse(value, out Guid guiTemp))
                    _guiWeaponID = guiTemp;
            }
        }

        /// <summary>
        /// Guid of a Cyberware Drone/Vehicle.
        /// </summary>
        public string VehicleID
        {
            get => _guiVehicleID.ToString("D", GlobalOptions.InvariantCultureInfo);
            set
            {
                if (Guid.TryParse(value, out Guid guiTemp))
                    _guiVehicleID = guiTemp;
            }
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
        /// Bonus node from the XML file that only activates for each pair of 'ware.
        /// </summary>
        public XmlNode PairBonus
        {
            get => _nodPairBonus;
            set => _nodPairBonus = value;
        }

        /// <summary>
        /// Bonus node from the XML file that only activates for each pair of 'ware.
        /// </summary>
        public XmlNode WirelessPairBonus
        {
            get => _nodWirelessPairBonus;
            set => _nodWirelessPairBonus = value;
        }

        /// <summary>
        /// Wireless bonus node from the XML file.
        /// </summary>
        public XmlNode WirelessBonus
        {
            get => _nodWirelessBonus;
            set => _nodWirelessBonus = value;
        }

        /// <summary>
        /// Whether the Cyberware's Wireless is enabled
        /// </summary>
        public bool WirelessOn
        {
            get => _blnWirelessOn;
            set
            {
                if (_blnWirelessOn == value) return;
                _blnWirelessOn = value;
                ToggleWirelessBonuses(value);
            }
        }

        /// <summary>
        /// AllowGear node from the XML file.
        /// </summary>
        public XmlNode AllowGear
        {
            get => _nodAllowGear;
            set => _nodAllowGear = value;
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
                    _objCachedMyXmlNode = null;
                _objImprovementSource = value;
            }
        }

        /// <summary>
        /// Cyberware name.
        /// </summary>
        public string Name
        {
            get => _strName;
            set
            {
                if (_strName == value) return;
                string strOldValue = _strName;
                _lstIncludeInPairBonus.Remove(_strName);
                _lstIncludeInPairBonus.Add(value);
                _lstIncludeInWirelessPairBonus.Remove(_strName);
                _lstIncludeInWirelessPairBonus.Add(value);
                _strName = value;
                if (_objParent?.Category != "Cyberlimb" || _objParent.Parent?.InheritAttributes == false ||
                    _objParent.ParentVehicle != null || _objCharacter.Options.DontUseCyberlimbCalculation ||
                    string.IsNullOrWhiteSpace(_objParent.LimbSlot) ||
                    _objCharacter.Options.ExcludeLimbSlot.Contains(_objParent.LimbSlot)) return;
                bool blnDoMovementUpdate = false;
                if (s_AgilityCombinedStrings.Contains(value) || s_AgilityCombinedStrings.Contains(strOldValue))
                {
                    blnDoMovementUpdate = true;
                    foreach (CharacterAttrib objCharacterAttrib in _objCharacter.AttributeSection.AttributeList
                        .Concat(_objCharacter.AttributeSection.SpecialAttributeList)
                        .Where(abbrev => abbrev.Abbrev == "AGI"))
                    {
                        objCharacterAttrib.OnPropertyChanged(nameof(CharacterAttrib.TotalValue));
                    }
                }

                if (s_StrengthCombinedStrings.Contains(value) || s_StrengthCombinedStrings.Contains(strOldValue))
                {
                    blnDoMovementUpdate = true;
                    foreach (CharacterAttrib objCharacterAttrib in _objCharacter.AttributeSection.AttributeList
                        .Concat(_objCharacter.AttributeSection.SpecialAttributeList)
                        .Where(abbrev => abbrev.Abbrev == "STR"))
                    {
                        objCharacterAttrib.OnPropertyChanged(nameof(CharacterAttrib.TotalValue));
                    }
                }

                if (blnDoMovementUpdate && _objCharacter.Options.CyberlegMovement && LimbSlot == "leg")
                    _objCharacter.OnPropertyChanged(nameof(Character.GetMovement));
            }
        }

        public bool InheritAttributes => _blnInheritAttributes;


        /// <summary>
        /// Identifier of the object within data files.
        /// </summary>
        public Guid SourceID => _guiSourceID;

        /// <summary>
        /// String-formatted identifier of the <inheritdoc cref="SourceID"/> from the data files.
        /// </summary>
        public string SourceIDString => _guiSourceID.ToString("D", GlobalOptions.InvariantCultureInfo);

        /// <summary>
        /// The name of the object as it should be displayed on printouts (translated name only).
        /// </summary>
        public string DisplayNameShort(string strLanguage)
        {
            if (strLanguage == GlobalOptions.DefaultLanguage)
                return Name;

            return GetNode(strLanguage)?["translate"]?.InnerText ?? Name;
        }

        public string CurrentDisplayNameShort => DisplayNameShort(GlobalOptions.Language);

        public static Guid EssenceHoleGUID { get; } = new Guid("b57eadaa-7c3b-4b80-8d79-cbbd922c1196");
        public static Guid EssenceAntiHoleGUID { get; } = new Guid("961eac53-0c43-4b19-8741-2872177a3a4c");

        /// <summary>
        /// The name of the object as it should be displayed in lists. Qty Name (Rating) (Extra).
        /// </summary>
        public string DisplayName(CultureInfo objCulture, string strLanguage)
        {
            string strReturn = DisplayNameShort(strLanguage);
            string strSpace = LanguageManager.GetString("String_Space", strLanguage);
            if (Rating > 0 && SourceID != EssenceHoleGUID && SourceID != EssenceAntiHoleGUID)
            {
                strReturn += strSpace + '(' + LanguageManager.GetString(RatingLabel, strLanguage) +
                             strSpace + Rating.ToString(objCulture) + ')';
            }

            if (!string.IsNullOrEmpty(Extra))
            {
                // Attempt to retrieve the CharacterAttribute name.
                strReturn += strSpace + '(' + LanguageManager.TranslateExtra(Extra, strLanguage) + ')';
            }

            if (!string.IsNullOrEmpty(Location))
            {
                string strSide = string.Empty;
                if (Location == "Left")
                    strSide = LanguageManager.GetString("String_Improvement_SideLeft", strLanguage);
                else if (Location == "Right")
                    strSide = LanguageManager.GetString("String_Improvement_SideRight", strLanguage);
                if (!string.IsNullOrEmpty(strSide))
                    strReturn += strSpace + '(' + strSide + ')';
            }

            return strReturn;
        }

        public string CurrentDisplayName => DisplayName(GlobalOptions.CultureInfo, GlobalOptions.Language);

        /// <summary>
        /// Translated Category.
        /// </summary>
        public string DisplayCategory(string strLanguage)
        {
            if (strLanguage == GlobalOptions.DefaultLanguage)
                return Category;

            return XmlManager
                       .Load(SourceType == Improvement.ImprovementSource.Cyberware ? "cyberware.xml" : "bioware.xml",
                           strLanguage)
                       .SelectSingleNode("/chummer/categories/category[. = \"" + Category + "\"]/@translate")
                       ?.InnerText ?? Category;
        }

        /// <summary>
        /// Cyberware category.
        /// </summary>
        public string Category
        {
            get => _strCategory;
            set
            {
                if (_strCategory != value)
                {
                    string strOldValue = _strCategory;
                    _strCategory = value;
                    if ((value == "Cyberlimb" || strOldValue == "Cyberlimb") && Parent?.InheritAttributes != false &&
                        ParentVehicle == null && !_objCharacter.Options.DontUseCyberlimbCalculation &&
                        !string.IsNullOrWhiteSpace(LimbSlot) &&
                        !_objCharacter.Options.ExcludeLimbSlot.Contains(LimbSlot))
                    {
                        foreach (CharacterAttrib objCharacterAttrib in _objCharacter.AttributeSection.AttributeList
                            .Concat(_objCharacter.AttributeSection.SpecialAttributeList))
                        {
                            if (objCharacterAttrib.Abbrev == "AGI" || objCharacterAttrib.Abbrev == "STR")
                            {
                                objCharacterAttrib.OnPropertyChanged(nameof(CharacterAttrib.TotalValue));
                            }
                        }

                        if (_objCharacter.Options.CyberlegMovement && LimbSlot == "leg")
                            _objCharacter.OnPropertyChanged(nameof(Character.GetMovement));
                    }
                }
            }
        }

        /// <summary>
        /// The type of body "slot" a Cyberlimb occupies.
        /// </summary>
        public string LimbSlot
        {
            get => _strLimbSlot;
            set
            {
                if (_strLimbSlot != value)
                {
                    string strOldValue = _strLimbSlot;
                    _strLimbSlot = value;
                    if (Category == "Cyberlimb" && Parent?.InheritAttributes != false && ParentVehicle == null &&
                        !_objCharacter.Options.DontUseCyberlimbCalculation &&
                        (!string.IsNullOrWhiteSpace(value) && !_objCharacter.Options.ExcludeLimbSlot.Contains(value)) ||
                        (!string.IsNullOrWhiteSpace(strOldValue) &&
                         !_objCharacter.Options.ExcludeLimbSlot.Contains(strOldValue)))
                    {
                        foreach (CharacterAttrib objCharacterAttrib in _objCharacter.AttributeSection.AttributeList
                            .Concat(_objCharacter.AttributeSection.SpecialAttributeList))
                        {
                            if (objCharacterAttrib.Abbrev == "AGI" || objCharacterAttrib.Abbrev == "STR")
                            {
                                objCharacterAttrib.OnPropertyChanged(nameof(CharacterAttrib.TotalValue));
                            }
                        }

                        if (_objCharacter.Options.CyberlegMovement && (value == "leg" || strOldValue == "leg"))
                            _objCharacter.OnPropertyChanged(nameof(Character.GetMovement));
                    }
                }
            }
        }

        /// <summary>
        /// The amount of body "slots" a Cyberlimb occupies.
        /// </summary>
        public int LimbSlotCount
        {
            get
            {
                if (_strLimbSlotCount == "all")
                {
                    return _objCharacter.LimbCount(LimbSlot);
                }

                int.TryParse(_strLimbSlotCount, NumberStyles.Any, GlobalOptions.InvariantCultureInfo, out int intReturn);
                return intReturn;
            }
            set
            {
                string strNewValue = value.ToString(GlobalOptions.InvariantCultureInfo);
                if (_strLimbSlotCount != strNewValue)
                {
                    _strLimbSlotCount = strNewValue;
                    if (Category == "Cyberlimb" && Parent?.InheritAttributes != false && ParentVehicle == null &&
                        !_objCharacter.Options.DontUseCyberlimbCalculation &&
                        !string.IsNullOrWhiteSpace(LimbSlot) &&
                        !_objCharacter.Options.ExcludeLimbSlot.Contains(LimbSlot))
                    {
                        foreach (CharacterAttrib objCharacterAttrib in _objCharacter.AttributeSection.AttributeList
                            .Concat(_objCharacter.AttributeSection.SpecialAttributeList))
                        {
                            if (objCharacterAttrib.Abbrev == "AGI" || objCharacterAttrib.Abbrev == "STR")
                            {
                                objCharacterAttrib.OnPropertyChanged(nameof(CharacterAttrib.TotalValue));
                            }
                        }

                        if (_objCharacter.Options.CyberlegMovement && LimbSlot == "leg")
                            _objCharacter.OnPropertyChanged(nameof(Character.GetMovement));
                    }
                }
            }
        }

        /// <summary>
        /// How many limbs does this cyberware have?
        /// </summary>
        public int GetCyberlimbCount(IReadOnlyCollection<string> lstExcludeLimbs = null)
        {
            int intCount = 0;
            if (!string.IsNullOrEmpty(LimbSlot) && lstExcludeLimbs?.All(l => l != LimbSlot) != false)
            {
                intCount += LimbSlotCount;
            }
            else
            {
                foreach (Cyberware objCyberwareChild in Children)
                {
                    intCount += objCyberwareChild.GetCyberlimbCount(lstExcludeLimbs);
                }
            }

            return intCount;
        }

        /// <summary>
        /// The location of a Cyberlimb (Left or Right).
        /// </summary>
        public string Location
        {
            get => _strLocation;
            set => _strLocation = value;
        }

        /// <summary>
        /// Original Forced Extra string associated with the 'ware.
        /// </summary>
        public string Forced
        {
            get => _strForced;
            set => _strForced = value;
        }

        /// <summary>
        /// Extra string associated with the 'ware.
        /// </summary>
        public string Extra
        {
            get => _strExtra;
            set => _strExtra = value;
        }

        /// <summary>
        /// Essence cost of the Cyberware.
        /// </summary>
        public string ESS
        {
            get => _strESS;
            set => _strESS = value;
        }

        /// <summary>
        /// Cyberware capacity.
        /// </summary>
        public string Capacity
        {
            get => _strCapacity;
            set => _strCapacity = value;
        }

        /// <summary>
        /// Availability.
        /// </summary>
        public string Avail
        {
            get => _strAvail;
            set => _strAvail = value;
        }

        /// <summary>
        /// Cost.
        /// </summary>
        public string Cost
        {
            get => _strCost;
            set => _strCost = value;
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

        private SourceString _objCachedSourceDetail;

        public SourceString SourceDetail => _objCachedSourceDetail ?? (_objCachedSourceDetail =
                                                new SourceString(Source, DisplayPage(GlobalOptions.Language),
                                                    GlobalOptions.Language));

        /// <summary>
        /// ID of the object that added this cyberware (if any).
        /// </summary>
        public string ParentID
        {
            get => _strParentID;
            set => _strParentID = value;
        }

        /// <summary>
        /// The modular mount this cyberware contains. Returns string.Empty if it contains no mount.
        /// </summary>
        public string HasModularMount
        {
            get => _strHasModularMount;
            set => _strHasModularMount = value;
        }

        /// <summary>
        /// What modular mount this cyberware plugs into. Returns string.Empty if it doesn't plug into a modular mount.
        /// </summary>
        public string PlugsIntoModularMount
        {
            get => _strPlugsIntoModularMount;
            set => _strPlugsIntoModularMount = value;
        }

        /// <summary>
        /// Returns whether the 'ware is currently equipped (with improvements applied) or not.
        /// </summary>
        public bool IsModularCurrentlyEquipped
        {
            get
            {
                // Cyberware always equipped if it's not a modular one
                bool blnReturn = string.IsNullOrEmpty(PlugsIntoModularMount);
                Cyberware objCurrentParent = Parent;
                // If top-level parent is one that has a modular mount but also does not plug into another modular mount itself, then return true, otherwise return false
                while (objCurrentParent != null)
                {
                    if (!string.IsNullOrEmpty(objCurrentParent.HasModularMount))
                        blnReturn = true;
                    if (!string.IsNullOrEmpty(objCurrentParent.PlugsIntoModularMount))
                        blnReturn = false;
                    objCurrentParent = objCurrentParent.Parent;
                }

                return blnReturn;
            }
        }

        public bool Stolen
        {
            get => _blnStolen;
            set => _blnStolen = value;
        }

        public void ToggleWirelessBonuses(bool enable)
        {
            if (enable)
            {
                if (WirelessBonus?.Attributes?.Count > 0)
                {
                    if (WirelessBonus.Attributes["mode"].InnerText == "replace")
                    {
                        ImprovementManager.DisableImprovements(_objCharacter,
                            _objCharacter.Improvements
                                .Where(x => x.ImproveSource == SourceType && x.SourceName == InternalId).ToList());
                    }
                }

                if (WirelessBonus?.InnerText != null)
                {
                    ImprovementManager.CreateImprovements(_objCharacter, _objImprovementSource,
                        _guiID.ToString("D", GlobalOptions.InvariantCultureInfo) + "Wireless", WirelessBonus, Rating, DisplayNameShort(GlobalOptions.Language));
                }

                if (!string.IsNullOrEmpty(ImprovementManager.SelectedValue) && string.IsNullOrEmpty(_strExtra))
                    _strExtra = ImprovementManager.SelectedValue;

                if (WirelessPairBonus == null) return;
                // This cyberware should not be included in the count to make things easier.
                List<Cyberware> lstPairableCyberwares = _objCharacter.Cyberware.DeepWhere(x => x.Children,
                    x => x != this && IncludeWirelessPair.Contains(x.Name) && x.Extra == Extra &&
                         x.IsModularCurrentlyEquipped && x.WirelessOn).ToList();
                int intCount = lstPairableCyberwares.Count;
                // Need to use slightly different logic if this cyberware has a location (Left or Right) and only pairs with itself because Lefts can only be paired with Rights and Rights only with Lefts
                if (!string.IsNullOrEmpty(Location) && IncludeWirelessPair.All(x => x == Name))
                {
                    intCount = 0;
                    foreach (Cyberware objPairableCyberware in lstPairableCyberwares)
                    {
                        if (objPairableCyberware.Location != Location)
                            // We have found a cyberware with which this one could be paired, so increase count by 1
                            intCount += 1;
                        else
                            // We have found a cyberware that would serve as a pair to another cyberware instead of this one, so decrease count by 1
                            intCount -= 1;
                    }

                    // If we have at least one cyberware with which we could pair, set count to 1 so that it passes the modulus to add the PairBonus. Otherwise, set to 0 so it doesn't pass.
                    intCount = intCount > 0 ? 1 : 0;
                }

                if (WirelessPairBonus?.Attributes?.Count > 0 && intCount % 2 == 1)
                {
                    if (WirelessPairBonus.Attributes["mode"].InnerText == "replace")
                    {
                        ImprovementManager.RemoveImprovements(_objCharacter,
                            _objCharacter.Improvements
                                .Where(x => x.ImproveSource == SourceType && x.SourceName == InternalId).ToList());
                    }

                    ImprovementManager.CreateImprovements(_objCharacter, SourceType,
                        _guiID.ToString("D", GlobalOptions.InvariantCultureInfo) + "WirelessPair", WirelessPairBonus, Rating, DisplayNameShort(GlobalOptions.Language));
                }

                foreach (Cyberware objLoopCyberware in lstPairableCyberwares)
                {
                    ImprovementManager.RemoveImprovements(_objCharacter, objLoopCyberware.SourceType,
                        objLoopCyberware.InternalId + "WirelessPair");
                    if (objLoopCyberware.WirelessPairBonus?.Attributes?["mode"]?.InnerText == "replace")
                    {
                        ImprovementManager.RemoveImprovements(_objCharacter, objLoopCyberware.SourceType,
                            objLoopCyberware.InternalId);
                    }
                    // Go down the list and create pair bonuses for every second item
                    if (intCount > 0 && intCount % 2 == 1)
                    {
                        ImprovementManager.CreateImprovements(_objCharacter, objLoopCyberware.SourceType,
                            objLoopCyberware.InternalId + "WirelessPair",
                            objLoopCyberware.WirelessPairBonus, objLoopCyberware.Rating, objLoopCyberware.DisplayNameShort(GlobalOptions.Language));
                    }

                    intCount -= 1;
                }
            }
            else
            {
                if (WirelessBonus?.Attributes?.Count > 0)
                {
                    if (WirelessBonus.Attributes?["mode"].InnerText == "replace")
                    {
                        ImprovementManager.EnableImprovements(_objCharacter,
                            _objCharacter.Improvements
                                .Where(x => x.ImproveSource == SourceType && x.SourceName == InternalId).ToList());
                    }
                }

                ImprovementManager.DisableImprovements(_objCharacter,
                    _objCharacter.Improvements
                        .Where(x => x.ImproveSource == SourceType && x.SourceName == InternalId + "Wireless").ToList());

                if (WirelessPairBonus == null) return;

                ImprovementManager.RemoveImprovements(_objCharacter, SourceType, InternalId + "WirelessPair");
                // This cyberware should not be included in the count to make things easier (we want to get the same number regardless of whether we call this before or after the actual equipping).
                List<Cyberware> lstPairableCyberwares = _objCharacter.Cyberware.DeepWhere(x => x.Children,
                    x => x != this && IncludeWirelessPair.Contains(x.Name) && x.Extra == Extra &&
                         x.IsModularCurrentlyEquipped && WirelessOn).ToList();
                int intCount = lstPairableCyberwares.Count;
                // Need to use slightly different logic if this cyberware has a location (Left or Right) and only pairs with itself because Lefts can only be paired with Rights and Rights only with Lefts
                if (!string.IsNullOrEmpty(Location) && IncludeWirelessPair.All(x => x == Name))
                {
                    int intMatchLocationCount = 0;
                    int intNotMatchLocationCount = 0;
                    foreach (Cyberware objPairableCyberware in lstPairableCyberwares)
                    {
                        if (objPairableCyberware.Location != Location)
                            intNotMatchLocationCount += 1;
                        else
                            intMatchLocationCount += 1;
                    }

                    // Set the count to the total number of cyberwares in matching pairs, which would mean 2x the number of whichever location contains the fewest members (since every single one of theirs would have a pair)
                    intCount = Math.Min(intMatchLocationCount, intNotMatchLocationCount) * 2;
                }

                if (WirelessPairBonus?.Attributes?.Count > 0)
                {
                    if (WirelessPairBonus.Attributes["mode"].InnerText == "replace")
                    {
                        ImprovementManager.EnableImprovements(_objCharacter,
                            _objCharacter.Improvements
                                .Where(x => x.ImproveSource == SourceType && x.SourceName == InternalId).ToList());
                    }
                }

                foreach (Cyberware objLoopCyberware in lstPairableCyberwares)
                {
                    ImprovementManager.RemoveImprovements(_objCharacter, objLoopCyberware.SourceType,
                        objLoopCyberware.InternalId + "WirelessPair");
                    // Go down the list and create pair bonuses for every second item
                    if (intCount > 0 && intCount % 2 == 0)
                    {
                        ImprovementManager.CreateImprovements(_objCharacter, objLoopCyberware.SourceType,
                            objLoopCyberware.InternalId + "WirelessPair",
                            objLoopCyberware.WirelessPairBonus, objLoopCyberware.Rating, objLoopCyberware.DisplayNameShort(GlobalOptions.Language));
                    }

                    intCount -= 1;
                }
            }
        }

        /// <summary>
        /// Used by our sorting algorithm to remember which order the user moves things to
        /// </summary>
        public int SortOrder
        {
            get => _intSortOrder;
            set => _intSortOrder = value;
        }

        /// <summary>
        /// Equips a piece of modular cyberware, activating the improvements of it and its children. Call after attaching onto objCharacter.Cyberware or a parent
        /// </summary>
        public void ChangeModularEquip(bool blnEquip)
        {

            ToggleWirelessBonuses(blnEquip);
            if (blnEquip)
            {
                ImprovementManager.EnableImprovements(_objCharacter,
                    _objCharacter.Improvements.Where(x => x.ImproveSource == SourceType && x.SourceName == InternalId)
                        .ToList());

                /*
                // If the piece grants a bonus, pass the information to the Improvement Manager.
                if (Bonus != null || WirelessBonus != null || PairBonus != null)
                {
                    if (!string.IsNullOrEmpty(_strForced) && _strForced != "Right" && _strForced != "Left")
                        ImprovementManager.ForcedValue = _strForced;

                    if (Bonus != null)
                        ImprovementManager.CreateImprovements(_objCharacter, SourceType, InternalId, Bonus, false, Rating, DisplayNameShort(GlobalOptions.Language));
                    if (!string.IsNullOrEmpty(ImprovementManager.SelectedValue) && string.IsNullOrEmpty(_strExtra))
                        _strExtra = ImprovementManager.SelectedValue;

                    if (WirelessBonus != null && WirelessOn)
                        ImprovementManager.CreateImprovements(_objCharacter, SourceType, InternalId, WirelessBonus, false, Rating, DisplayNameShort(GlobalOptions.Language));
                    if (!string.IsNullOrEmpty(ImprovementManager.SelectedValue) && string.IsNullOrEmpty(_strExtra))
                        _strExtra = ImprovementManager.SelectedValue;
                }
                */

                if (PairBonus != null)
                {
                    // This cyberware should not be included in the count to make things easier.
                    List<Cyberware> lstPairableCyberwares = _objCharacter.Cyberware.DeepWhere(x => x.Children,
                        x => x != this && IncludePair.Contains(x.Name) && x.Extra == Extra &&
                             x.IsModularCurrentlyEquipped).ToList();
                    int intCount = lstPairableCyberwares.Count;
                    // Need to use slightly different logic if this cyberware has a location (Left or Right) and only pairs with itself because Lefts can only be paired with Rights and Rights only with Lefts
                    if (!string.IsNullOrEmpty(Location) && IncludePair.All(x => x == Name))
                    {
                        intCount = 0;
                        foreach (Cyberware objPairableCyberware in lstPairableCyberwares)
                        {
                            if (objPairableCyberware.Location != Location)
                                // We have found a cyberware with which this one could be paired, so increase count by 1
                                intCount += 1;
                            else
                                // We have found a cyberware that would serve as a pair to another cyberware instead of this one, so decrease count by 1
                                intCount -= 1;
                        }

                        // If we have at least one cyberware with which we could pair, set count to 1 so that it passes the modulus to add the PairBonus. Otherwise, set to 0 so it doesn't pass.
                        intCount = intCount > 0 ? 1 : 0;
                    }

                    if ((intCount & 1) == 1)
                    {
                        ImprovementManager.CreateImprovements(_objCharacter, SourceType, InternalId + "Pair", PairBonus,
                            Rating, DisplayNameShort(GlobalOptions.Language));
                    }
                }
            }
            else
            {
                ImprovementManager.DisableImprovements(_objCharacter,
                    _objCharacter.Improvements.Where(x => x.ImproveSource == SourceType && x.SourceName == InternalId)
                        .ToList());

                if (PairBonus != null)
                {
                    ImprovementManager.RemoveImprovements(_objCharacter, SourceType, InternalId + "Pair");
                    // This cyberware should not be included in the count to make things easier (we want to get the same number regardless of whether we call this before or after the actual equipping).
                    List<Cyberware> lstPairableCyberwares = _objCharacter.Cyberware.DeepWhere(x => x.Children,
                        x => x != this && IncludePair.Contains(x.Name) && x.Extra == Extra &&
                             x.IsModularCurrentlyEquipped).ToList();
                    int intCount = lstPairableCyberwares.Count;
                    // Need to use slightly different logic if this cyberware has a location (Left or Right) and only pairs with itself because Lefts can only be paired with Rights and Rights only with Lefts
                    if (!string.IsNullOrEmpty(Location) && IncludePair.All(x => x == Name))
                    {
                        int intMatchLocationCount = 0;
                        int intNotMatchLocationCount = 0;
                        foreach (Cyberware objPairableCyberware in lstPairableCyberwares)
                        {
                            if (objPairableCyberware.Location != Location)
                                intNotMatchLocationCount += 1;
                            else
                                intMatchLocationCount += 1;
                        }

                        // Set the count to the total number of cyberwares in matching pairs, which would mean 2x the number of whichever location contains the fewest members (since every single one of theirs would have a pair)
                        intCount = Math.Min(intMatchLocationCount, intNotMatchLocationCount) * 2;
                    }

                    foreach (Cyberware objLoopCyberware in lstPairableCyberwares)
                    {
                        ImprovementManager.RemoveImprovements(_objCharacter, objLoopCyberware.SourceType,
                            objLoopCyberware.InternalId + "Pair");
                        // Go down the list and create pair bonuses for every second item
                        if (intCount > 0 && (intCount & 1) == 0)
                        {
                            ImprovementManager.CreateImprovements(_objCharacter, objLoopCyberware.SourceType,
                                objLoopCyberware.InternalId + "Pair", objLoopCyberware.PairBonus, objLoopCyberware.Rating,
                                objLoopCyberware.DisplayNameShort(GlobalOptions.Language));
                        }

                        intCount -= 1;
                    }
                }
            }

            foreach (Gear objChildGear in Gear)
                objChildGear.ChangeEquippedStatus(blnEquip);

            foreach (Cyberware objChild in Children)
                objChild.ChangeModularEquip(blnEquip);
        }

        public bool CanRemoveThroughImprovements
        {
            get
            {
                Cyberware objParent = this;
                bool blnNoParentIsModular = string.IsNullOrEmpty(objParent.PlugsIntoModularMount);
                while (objParent.Parent != null && blnNoParentIsModular)
                {
                    objParent = objParent.Parent;
                    blnNoParentIsModular = string.IsNullOrEmpty(objParent.PlugsIntoModularMount);
                }

                return blnNoParentIsModular;
            }
        }

        /// <summary>
        /// Comma-separated list of mount locations with which this 'ware is mutually exclusive.
        /// </summary>
        public string BlocksMounts
        {
            get => _strBlocksMounts;
            set => _strBlocksMounts = value;
        }

        /// <summary>
        /// Rating.
        /// </summary>
        public int Rating
        {
            get => Math.Max(Math.Min(_intRating, MaxRating), MinRating);
            set
            {
                int intNewValue = Math.Max(Math.Min(value, MaxRating), MinRating);
                if (_intRating == intNewValue) return;
                _intRating = intNewValue;
                bool blnDoMovementUpdate = false;
                if (_objParent?.Category == "Cyberlimb" && _objParent.Parent?.InheritAttributes != false &&
                    _objParent.ParentVehicle == null && !_objCharacter.Options.DontUseCyberlimbCalculation &&
                    !string.IsNullOrWhiteSpace(_objParent.LimbSlot) &&
                    !_objCharacter.Options.ExcludeLimbSlot.Contains(_objParent.LimbSlot))
                {
                    if (s_AgilityCombinedStrings.Contains(Name))
                    {
                        foreach (CharacterAttrib objCharacterAttrib in _objCharacter.AttributeSection.AttributeList
                            .Concat(_objCharacter.AttributeSection.SpecialAttributeList)
                            .Where(abbrev => abbrev.Abbrev == "AGI"))
                        {
                            objCharacterAttrib.OnPropertyChanged(nameof(CharacterAttrib.TotalValue));
                        }

                        blnDoMovementUpdate = true;
                    }
                    else if (s_StrengthCombinedStrings.Contains(Name))
                    {
                        foreach (CharacterAttrib objCharacterAttrib in _objCharacter.AttributeSection.AttributeList
                            .Concat(_objCharacter.AttributeSection.SpecialAttributeList)
                            .Where(abbrev => abbrev.Abbrev == "STR"))
                        {
                            objCharacterAttrib.OnPropertyChanged(nameof(CharacterAttrib.TotalValue));
                        }

                        blnDoMovementUpdate = true;
                    }
                }

                blnDoMovementUpdate =
                    blnDoMovementUpdate && _objCharacter.Options.CyberlegMovement && LimbSlot == "leg";
                bool blnDoEssenceUpdate = (ESS.Contains("Rating") || ESS.Contains("FixedValues")) && (Parent == null || AddToParentESS) &&
                                          string.IsNullOrEmpty(PlugsIntoModularMount) && ParentVehicle == null;
                if (blnDoMovementUpdate && blnDoEssenceUpdate)
                    _objCharacter.OnMultiplePropertyChanged(nameof(Character.GetMovement), EssencePropertyName);
                else if (blnDoMovementUpdate)
                    _objCharacter.OnPropertyChanged(nameof(Character.GetMovement));
                else if (blnDoEssenceUpdate)
                    _objCharacter.OnPropertyChanged(EssencePropertyName);

                if (Gear.Count > 0)
                {
                    foreach (Gear objChild in Gear.Where(x =>
                        x.MaxRating.Contains("Parent") || x.MinRating.Contains("Parent")))
                    {
                        // This will update a child's rating if it would become out of bounds due to its parent's rating changing
                        objChild.Rating = objChild.Rating;
                    }
                }
            }
        }

        /// <summary>
        /// Total Minimum Rating.
        /// </summary>
        public int MinRating
        {
            get
            {
                int intReturn = 0;
                string strRating = MinRatingString;

                // Not a simple integer, so we need to start mucking around with strings
                if (!string.IsNullOrEmpty(strRating) && !int.TryParse(strRating, NumberStyles.Any, GlobalOptions.InvariantCultureInfo, out intReturn))
                {
                    strRating = strRating.CheapReplace("MaximumSTR",
                            () => (ParentVehicle != null
                                ? Math.Max(1, ParentVehicle.TotalBody * 2)
                                : _objCharacter.STR.TotalMaximum).ToString(GlobalOptions.InvariantCultureInfo))
                        .CheapReplace("MaximumAGI",
                            () => (ParentVehicle != null
                                ? Math.Max(1, ParentVehicle.Pilot * 2)
                                : _objCharacter.AGI.TotalMaximum).ToString(GlobalOptions.InvariantCultureInfo))
                        .CheapReplace("MinimumSTR", () => (ParentVehicle?.TotalBody ?? 3).ToString(GlobalOptions.InvariantCultureInfo))
                        .CheapReplace("MinimumAGI", () => (ParentVehicle?.Pilot ?? 3).ToString(GlobalOptions.InvariantCultureInfo));

                    object objProcess = CommonFunctions.EvaluateInvariantXPath(strRating, out bool blnIsSuccess);
                    if (blnIsSuccess)
                        intReturn = Convert.ToInt32(objProcess, GlobalOptions.InvariantCultureInfo);
                }

                return intReturn;
            }
        }

        /// <summary>
        /// String representing minimum rating before it would be computed.
        /// </summary>
        public string MinRatingString
        {
            get => _strMinRating;
            set => _strMinRating = value;
        }

        /// <summary>
        /// Total Maximum Rating.
        /// </summary>
        public int MaxRating
        {
            get
            {
                int intReturn = 0;
                string strRating = MaxRatingString;

                // Not a simple integer, so we need to start mucking around with strings
                if (!string.IsNullOrEmpty(strRating) && !int.TryParse(strRating, NumberStyles.Any, GlobalOptions.InvariantCultureInfo, out intReturn))
                {
                    strRating = strRating.CheapReplace("MaximumSTR",
                            () => (ParentVehicle != null
                                ? Math.Max(1, ParentVehicle.TotalBody * 2)
                                : _objCharacter.STR.TotalMaximum).ToString(GlobalOptions.InvariantCultureInfo))
                        .CheapReplace("MaximumAGI",
                            () => (ParentVehicle != null
                                ? Math.Max(1, ParentVehicle.Pilot * 2)
                                : _objCharacter.AGI.TotalMaximum).ToString(GlobalOptions.InvariantCultureInfo))
                        .CheapReplace("MinimumSTR", () => (ParentVehicle?.TotalBody ?? 3).ToString(GlobalOptions.InvariantCultureInfo))
                        .CheapReplace("MinimumAGI", () => (ParentVehicle?.Pilot ?? 3).ToString(GlobalOptions.InvariantCultureInfo));

                    object objProcess = CommonFunctions.EvaluateInvariantXPath(strRating, out bool blnIsSuccess);
                    if (blnIsSuccess)
                        intReturn = Convert.ToInt32(objProcess, GlobalOptions.InvariantCultureInfo);
                }

                return intReturn;
            }
        }

        /// <summary>
        /// String representing maximum rating before it would be computed.
        /// </summary>
        public string MaxRatingString
        {
            get => _strMaxRating;
            set => _strMaxRating = value;
        }

        public string RatingLabel
        {
            get => _strRatingLabel;
            set => _strRatingLabel = value;
        }

        /// <summary>
        /// Grade level of the Cyberware.
        /// </summary>
        public Grade Grade
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(ForceGrade) && ForceGrade != _objGrade.Name)
                {
                    return Grade.ConvertToCyberwareGrade(ForceGrade, SourceType, _objCharacter);
                }

                return _objGrade;
            }
            set
            {
                if (_objGrade != value && value != null)
                {
                    bool blnGradeEssenceChanged = _objGrade.Essence != value.Essence;
                    _objGrade = value;
                    if (blnGradeEssenceChanged && (Parent == null || AddToParentESS) &&
                        string.IsNullOrEmpty(PlugsIntoModularMount) && ParentVehicle == null)
                        _objCharacter.OnPropertyChanged(EssencePropertyName);
                    // Run through all of the child pieces and make sure their Grade matches.
                    foreach (Cyberware objChild in Children)
                    {
                        objChild.Grade = value;
                    }
                }
            }
        }

        /// <summary>
        /// The Categories of allowable Subsystems.
        /// </summary>
        public string AllowedSubsystems
        {
            get => _strAllowSubsystems;
            set => _strAllowSubsystems = value;
        }

        /// <summary>
        /// Whether or not the piece of Cyberware is part of a Cyberware Suite.
        /// </summary>
        public bool Suite
        {
            get => _blnSuite;
            set => _blnSuite = value;
        }

        /// <summary>
        /// Essence cost discount.
        /// </summary>
        public int ESSDiscount
        {
            get => _intEssenceDiscount;
            set
            {
                if (_intEssenceDiscount != value)
                {
                    _intEssenceDiscount = value;
                    if ((Parent == null || AddToParentESS) && string.IsNullOrEmpty(PlugsIntoModularMount) &&
                        ParentVehicle == null)
                        _objCharacter.OnPropertyChanged(EssencePropertyName);
                }
            }
        }

        /// <summary>
        /// Extra Essence cost multiplier (additively stacking, starts at 0).
        /// </summary>
        public decimal ExtraESSAdditiveMultiplier
        {
            get => _decExtraESSAdditiveMultiplier;
            set
            {
                if (_decExtraESSAdditiveMultiplier != value)
                {
                    _decExtraESSAdditiveMultiplier = value;
                    if ((Parent == null || AddToParentESS) && string.IsNullOrEmpty(PlugsIntoModularMount) &&
                        ParentVehicle == null)
                        _objCharacter.OnPropertyChanged(EssencePropertyName);
                }
            }
        }

        /// <summary>
        /// Extra Essence cost multiplier (multiplicative stacking, starts at 1).
        /// </summary>
        public decimal ExtraESSMultiplicativeMultiplier
        {
            get => _decExtraESSMultiplicativeMultiplier;
            set
            {
                if (_decExtraESSMultiplicativeMultiplier != value)
                {
                    _decExtraESSMultiplicativeMultiplier = value;
                    if ((Parent == null || AddToParentESS) && string.IsNullOrEmpty(PlugsIntoModularMount) &&
                        ParentVehicle == null)
                        _objCharacter.OnPropertyChanged(EssencePropertyName);
                }
            }
        }

        /// <summary>
        /// Base Physical Boxes. 12 for vehicles, 6 for Drones.
        /// </summary>
        public int BaseMatrixBoxes
        {
            get
            {
                int baseMatrixBoxes = 8;
                return baseMatrixBoxes;
            }
        }

        /// <summary>
        /// Matrix Condition Monitor boxes.
        /// </summary>
        public int MatrixCM => BaseMatrixBoxes + (this.GetTotalMatrixAttribute("Device Rating") + 1) / 2 +
                               TotalBonusMatrixBoxes;

        /// <summary>
        /// Matrix Condition Monitor boxes filled.
        /// </summary>
        public int MatrixCMFilled
        {
            get => _intMatrixCMFilled;
            set => _intMatrixCMFilled = value;
        }

        /// <summary>
        /// A List of child pieces of Cyberware.
        /// </summary>
        public TaggedObservableCollection<Cyberware> Children => _lstChildren;

        /// <summary>
        /// A List of the Gear attached to the Cyberware.
        /// </summary>
        public TaggedObservableCollection<Gear> Gear => _lstGear;

        /// <summary>
        /// List of names to include in pair bonus
        /// </summary>
        public ICollection<string> IncludePair => _lstIncludeInPairBonus;

        /// <summary>
        /// List of names to include in pair bonus
        /// </summary>
        public ICollection<string> IncludeWirelessPair => _lstIncludeInWirelessPairBonus;

        /// <summary>
        /// Notes.
        /// </summary>
        public string Notes
        {
            get => _strNotes;
            set => _strNotes = value;
        }

        /// <summary>
        /// Whether or not the Cyberware's cost should be discounted by 10% through the Black Market Pipeline Quality.
        /// </summary>
        public bool DiscountCost
        {
            get => _blnDiscountCost && _objCharacter.BlackMarketDiscount;
            set => _blnDiscountCost = value;
        }

        /// <summary>
        /// Whether or not the Cyberware's ESS cost increases that of its parent when added as a subsystem (usually no).
        /// </summary>
        public bool AddToParentESS
        {
            get => _blnAddToParentESS;
            set
            {
                if (_blnAddToParentESS != value)
                {
                    bool blnOldValue = _blnAddToParentESS;
                    _blnAddToParentESS = value;
                    if ((Parent == null || AddToParentESS || blnOldValue) &&
                        string.IsNullOrEmpty(PlugsIntoModularMount) && ParentVehicle == null)
                        _objCharacter.OnPropertyChanged(EssencePropertyName);
                }
            }
        }

        /// <summary>
        /// Whether or not the Cyberware's ESS cost increases that of its parent when added as a subsystem (usually no).
        /// </summary>
        public bool AddToParentCapacity
        {
            get => _blnAddToParentCapacity;
            set
            {
                if (_blnAddToParentCapacity != value)
                {
                    bool blnOldValue = _blnAddToParentCapacity;
                    _blnAddToParentCapacity = value;
                    if ((Parent == null || AddToParentCapacity || blnOldValue) &&
                        string.IsNullOrEmpty(PlugsIntoModularMount) && ParentVehicle == null)
                        _objCharacter.OnPropertyChanged(Capacity);
                }
            }
        }

        /// <summary>
        /// Parent Cyberware.
        /// </summary>
        public Cyberware Parent
        {
            get => _objParent;
            set
            {
                if (_objParent != value)
                {
                    bool blnOldEquipped = IsModularCurrentlyEquipped;
                    _objParent = value;
                    ParentVehicle = value?.ParentVehicle;
                    if (IsModularCurrentlyEquipped != blnOldEquipped)
                    {
                        foreach (Gear objGear in Gear)
                        {
                            if (blnOldEquipped)
                                objGear.ChangeEquippedStatus(false);
                            else if (objGear.Equipped)
                                objGear.ChangeEquippedStatus(true);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Topmost Parent Cyberware.
        /// </summary>
        public Cyberware TopMostParent
        {
            get
            {
                Cyberware objReturn = this;
                while (objReturn.Parent != null)
                    objReturn = objReturn.Parent;
                return objReturn;
            }
        }

        /// <summary>
        /// Vehicle to which this cyberware is attached (if any)
        /// </summary>
        public Vehicle ParentVehicle
        {
            get => _objParentVehicle;
            set
            {
                if (_objParentVehicle != value)
                {
                    _objParentVehicle = value;
                    bool blnEquipped = IsModularCurrentlyEquipped;
                    foreach (Gear objGear in Gear)
                    {
                        if (value != null)
                            objGear.ChangeEquippedStatus(false);
                        else if (objGear.Equipped && blnEquipped)
                            objGear.ChangeEquippedStatus(true);
                    }
                }

                foreach (Cyberware objChild in Children)
                    objChild.ParentVehicle = value;
            }
        }

        /// <summary>
        /// Grade that the Cyberware should be forced to use, if applicable.
        /// </summary>
        public string ForceGrade => _strForceGrade;

        /// <summary>
        /// Is the Bioware's cost affected by Prototype Transhuman?
        /// </summary>
        public bool PrototypeTranshuman
        {
            get => _blnPrototypeTranshuman && SourceType == Improvement.ImprovementSource.Bioware;
            set
            {
                if (_blnPrototypeTranshuman != value)
                {
                    string strOldEssencePropertyName = EssencePropertyName;
                    _blnPrototypeTranshuman = value;
                    if ((Parent == null || AddToParentESS) && string.IsNullOrEmpty(PlugsIntoModularMount) &&
                        ParentVehicle == null)
                        _objCharacter.OnMultiplePropertyChanged(strOldEssencePropertyName, EssencePropertyName);
                }

                foreach (Cyberware objCyberware in Children)
                    objCyberware.PrototypeTranshuman = value;
            }
        }

        public string EssencePropertyName
        {
            get
            {
                if (PrototypeTranshuman)
                    return nameof(Character.PrototypeTranshumanEssenceUsed);
                if (SourceID.Equals(EssenceHoleGUID) || SourceID.Equals(EssenceAntiHoleGUID))
                    return nameof(Character.EssenceHole);
                if (SourceType == Improvement.ImprovementSource.Bioware)
                    return nameof(Character.BiowareEssence);
                if (SourceType == Improvement.ImprovementSource.Cyberware)
                    return nameof(Character.CyberwareEssence);
                return nameof(Character.Essence);
            }
        }

        private XmlNode _objCachedMyXmlNode;
        private string _strCachedXmlNodeLanguage = string.Empty;

        public XmlNode GetNode()
        {
            return GetNode(GlobalOptions.Language);
        }

        public XmlNode GetNode(string strLanguage)
        {
            if (_objCachedMyXmlNode != null && strLanguage == _strCachedXmlNodeLanguage &&
                !GlobalOptions.LiveCustomData)
                return _objCachedMyXmlNode;
            string strGuid = SourceIDString;
            XmlDocument objDoc;
            if (_objImprovementSource == Improvement.ImprovementSource.Bioware)
            {
                objDoc = XmlManager.Load("bioware.xml", strLanguage);
                _objCachedMyXmlNode = objDoc.SelectSingleNode("/chummer/biowares/bioware[id = \"" + strGuid +
                                                              "\" or id = \"" + strGuid.ToUpperInvariant() + "\"]");
                if (_objCachedMyXmlNode == null)
                {
                    _objCachedMyXmlNode = objDoc.SelectSingleNode("/chummer/biowares/bioware[name = \"" + Name + "\"]");
                    _objCachedMyXmlNode?.TryGetGuidFieldQuickly("id", ref _guiSourceID);
                }
            }
            else
            {
                objDoc = XmlManager.Load("cyberware.xml", strLanguage);
                _objCachedMyXmlNode = objDoc.SelectSingleNode("/chummer/cyberwares/cyberware[id = \"" + strGuid +
                                                              "\" or id = \"" + strGuid.ToUpperInvariant() + "\"]");
                if (_objCachedMyXmlNode == null)
                {
                    _objCachedMyXmlNode =
                        objDoc.SelectSingleNode("/chummer/cyberwares/cyberware[name = \"" + Name + "\"]");
                    _objCachedMyXmlNode?.TryGetGuidFieldQuickly("id", ref _guiSourceID);
                }
            }

            _strCachedXmlNodeLanguage = strLanguage;
            return _objCachedMyXmlNode;
        }

        #endregion

        #region Complex Properties

        /// <summary>
        /// Ghetto workaround for flagging an object as geneware.
        /// </summary>
        public bool IsGeneware =>
            (SourceType == Improvement.ImprovementSource.Bioware &&
             (Category.StartsWith("Genetech", StringComparison.Ordinal) || Category.StartsWith("Genetic Infusions", StringComparison.Ordinal) ||
              Category.StartsWith("Genemods", StringComparison.Ordinal)));

        /// <summary>
        /// Total Availability in the program's current language.
        /// </summary>
        public string DisplayTotalAvail => TotalAvail(GlobalOptions.CultureInfo, GlobalOptions.Language);

        /// <summary>
        /// Total Availability of the Cyberware and its plugins.
        /// </summary>
        public string TotalAvail(CultureInfo objCulture, string strLanguage)
        {
            return TotalAvailTuple().ToString(objCulture, strLanguage);
        }

        /// <summary>
        /// Total Availability as a triple.
        /// </summary>
        public AvailabilityValue TotalAvailTuple(bool blnCheckChildren = true)
        {
            bool blnModifyParentAvail = false;
            string strAvail = Avail;
            char chrLastAvailChar = ' ';
            int intAvail = Grade.Avail;
            bool blnOrGear = false;
            if (strAvail.Length > 0)
            {
                if (strAvail.StartsWith("FixedValues(", StringComparison.Ordinal))
                {
                    string[] strValues = strAvail.TrimStartOnce("FixedValues(", true).TrimEndOnce(')').Split(',');
                    strAvail = strValues[Math.Max(Math.Min(Rating, strValues.Length) - 1, 0)];
                }

                blnOrGear = strAvail.EndsWith(" or Gear", StringComparison.Ordinal);
                if (blnOrGear)
                    strAvail = strAvail.TrimEndOnce(" or Gear", true);

                chrLastAvailChar = strAvail[strAvail.Length - 1];
                if (chrLastAvailChar == 'F' || chrLastAvailChar == 'R')
                {
                    strAvail = strAvail.Substring(0, strAvail.Length - 1);
                }

                blnModifyParentAvail = strAvail.StartsWith('+', '-');
                if (blnModifyParentAvail)
                    intAvail = 0;
                StringBuilder objAvail = new StringBuilder(strAvail.TrimStart('+'));
                objAvail.CheapReplace(strAvail, "MinRating", () => MinRating.ToString(GlobalOptions.InvariantCultureInfo));
                objAvail.CheapReplace(strAvail, "Rating", () => Rating.ToString(GlobalOptions.InvariantCultureInfo));

                foreach (CharacterAttrib objLoopAttribute in _objCharacter.AttributeSection.AttributeList.Concat(
                    _objCharacter.AttributeSection.SpecialAttributeList))
                {
                    objAvail.CheapReplace(strAvail, objLoopAttribute.Abbrev,
                        () => objLoopAttribute.TotalValue.ToString(GlobalOptions.InvariantCultureInfo));
                    objAvail.CheapReplace(strAvail, objLoopAttribute.Abbrev + "Base",
                        () => objLoopAttribute.TotalBase.ToString(GlobalOptions.InvariantCultureInfo));
                }

                object objProcess = CommonFunctions.EvaluateInvariantXPath(objAvail.ToString(), out bool blnIsSuccess);
                if (blnIsSuccess)
                    intAvail += Convert.ToInt32(objProcess, GlobalOptions.InvariantCultureInfo);
            }

            if (blnCheckChildren)
            {
                // Run through cyberware children and increase the Avail by any installed Mod whose Avail starts with "+" or "-".
                foreach (Cyberware objChild in Children)
                {
                    if (objChild.ParentID == InternalId ||
                        !objChild.IsModularCurrentlyEquipped &&
                        !string.IsNullOrEmpty(objChild.PlugsIntoModularMount))
                        continue;
                    AvailabilityValue objLoopAvailTuple = objChild.TotalAvailTuple();
                    if (objLoopAvailTuple.AddToParent)
                        intAvail += objLoopAvailTuple.Value;
                    if (objLoopAvailTuple.Suffix == 'F')
                        chrLastAvailChar = 'F';
                    else if (chrLastAvailChar != 'F' && objLoopAvailTuple.Suffix == 'R')
                        chrLastAvailChar = 'R';
                }
            }

            int intLoopAvail = 0;
            // Run through gear children and increase the Avail by any Mod whose Avail starts with "+" or "-".
            foreach (Gear objChild in Gear)
            {
                if (objChild.ParentID != InternalId)
                {
                    AvailabilityValue objLoopAvailTuple = objChild.TotalAvailTuple();
                    if (!objLoopAvailTuple.AddToParent)
                        intLoopAvail = Math.Max(intLoopAvail, objLoopAvailTuple.Value);
                    if (blnCheckChildren)
                    {
                        if (objLoopAvailTuple.AddToParent)
                            intAvail += objLoopAvailTuple.Value;
                        if (objLoopAvailTuple.Suffix == 'F')
                            chrLastAvailChar = 'F';
                        else if (chrLastAvailChar != 'F' && objLoopAvailTuple.Suffix == 'R')
                            chrLastAvailChar = 'R';
                    }
                    else if (blnOrGear)
                    {
                        if (objLoopAvailTuple.Suffix == 'F')
                            chrLastAvailChar = 'F';
                        else if (chrLastAvailChar != 'F' && objLoopAvailTuple.Suffix == 'R')
                            chrLastAvailChar = 'R';
                    }
                }
            }

            // Avail cannot go below 0. This typically happens when an item with Avail 0 is given the Second Hand category.
            if (intAvail < 0)
                intAvail = 0;

            if (blnOrGear && intLoopAvail > intAvail)
                intAvail = intLoopAvail;

            return new AvailabilityValue(intAvail, chrLastAvailChar, blnModifyParentAvail);
        }

        /// <summary>
        /// Calculated Capacity of the Cyberware.
        /// </summary>
        public string CalculatedCapacity
        {
            get
            {
                string strCapacity = Capacity;
                if (strCapacity.StartsWith("FixedValues(", StringComparison.Ordinal))
                {
                    string[] strValues = strCapacity.TrimStartOnce("FixedValues(", true).TrimEndOnce(')').Split(',');
                    strCapacity = strValues[Math.Max(Math.Min(Rating, strValues.Length) - 1, 0)];
                }

                if (string.IsNullOrEmpty(strCapacity))
                    return (0.0m).ToString("#,0.##", GlobalOptions.CultureInfo);
                if (strCapacity == "[*]")
                    return "*";
                string strReturn;
                int intPos = strCapacity.IndexOf("/[", StringComparison.Ordinal);
                if (intPos != -1)
                {
                    string strFirstHalf = strCapacity.Substring(0, intPos);
                    string strSecondHalf = strCapacity.Substring(intPos + 1, strCapacity.Length - intPos - 1);
                    bool blnSquareBrackets = strFirstHalf.StartsWith('[');

                    if (blnSquareBrackets && strFirstHalf.Length > 2)
                        strFirstHalf = strFirstHalf.Substring(1, strFirstHalf.Length - 2);

                    try
                    {
                        object objProcess =
                            CommonFunctions.EvaluateInvariantXPath(strFirstHalf.Replace("Rating", Rating.ToString(GlobalOptions.InvariantCultureInfo)),
                                out bool blnIsSuccess);
                        strReturn = blnIsSuccess
                            ? ((double) objProcess).ToString("#,0.##", GlobalOptions.CultureInfo)
                            : strFirstHalf;
                    }
                    catch (OverflowException) // Result is text and not a double
                    {
                        strReturn = strFirstHalf;
                    }
                    catch (InvalidCastException) // Result is text and not a double
                    {
                        strReturn = strFirstHalf;
                    }

                    if (blnSquareBrackets)
                        strReturn = '[' + strCapacity + ']';

                    strSecondHalf = strSecondHalf.Trim('[', ']');
                    if (Children.Any(x => x.AddToParentCapacity))
                    {
                        // Run through its Children and deduct the Capacity costs.
                        foreach (Cyberware objChildCyberware in Children.Where(objChild => objChild.AddToParentCapacity)
                        )
                        {
                            if (objChildCyberware.ParentID == InternalId)
                            {
                                continue;
                            }

                            string strLoopCapacity = objChildCyberware.CalculatedCapacity;
                            int intLoopPos = strLoopCapacity.IndexOf("/[", StringComparison.Ordinal);
                            if (intLoopPos != -1)
                                strLoopCapacity = strLoopCapacity.Substring(intLoopPos + 2,
                                    strLoopCapacity.LastIndexOf(']') - intLoopPos - 2);
                            else if (strLoopCapacity.StartsWith('['))
                                strLoopCapacity = strLoopCapacity.Substring(1, strLoopCapacity.Length - 2);
                            if (strLoopCapacity == "*")
                                strLoopCapacity = "0";
                            strSecondHalf += "+(" + strLoopCapacity + ')';
                        }
                    }

                    try
                    {
                        object objProcess =
                            CommonFunctions.EvaluateInvariantXPath(strSecondHalf.Replace("Rating", Rating.ToString(GlobalOptions.InvariantCultureInfo)),
                                out bool blnIsSuccess);
                        strSecondHalf =
                            '[' + (blnIsSuccess
                                ? ((double) objProcess).ToString("#,0.##", GlobalOptions.CultureInfo)
                                : strSecondHalf) + ']';
                    }
                    catch (OverflowException) // Result is text and not a double
                    {
                        strSecondHalf = '[' + strSecondHalf + ']';
                    }
                    catch (InvalidCastException) // Result is text and not a double
                    {
                        strSecondHalf = '[' + strSecondHalf + ']';
                    }

                    strReturn += "/" + strSecondHalf;
                }
                else if (strCapacity.Contains("Rating") ||
                         (strCapacity.StartsWith('[') && Children.Any(x => x.AddToParentCapacity)))
                {
                    // If the Capacity is determined by the Rating, evaluate the expression.
                    // XPathExpression cannot evaluate while there are square brackets, so remove them if necessary.
                    bool blnSquareBrackets = strCapacity.StartsWith('[');
                    if (blnSquareBrackets)
                    {
                        strCapacity = strCapacity.Substring(1, strCapacity.Length - 2);
                        if (Children.Any(x => x.AddToParentCapacity))
                        {
                            // Run through its Children and deduct the Capacity costs.
                            foreach (Cyberware objChildCyberware in Children.Where(objChild =>
                                objChild.AddToParentCapacity))
                            {
                                if (objChildCyberware.ParentID == InternalId)
                                {
                                    continue;
                                }

                                string strLoopCapacity = objChildCyberware.CalculatedCapacity;
                                int intLoopPos = strLoopCapacity.IndexOf("/[", StringComparison.Ordinal);
                                if (intLoopPos != -1)
                                    strLoopCapacity = strLoopCapacity.Substring(intLoopPos + 2,
                                        strLoopCapacity.LastIndexOf(']') - intLoopPos - 2);
                                else if (strLoopCapacity.StartsWith('['))
                                    strLoopCapacity = strLoopCapacity.Substring(1, strLoopCapacity.Length - 2);
                                if (strLoopCapacity == "*")
                                    strLoopCapacity = "0";
                                strCapacity += "+(" + strLoopCapacity + ')';
                            }
                        }
                    }

                    object objProcess =
                        CommonFunctions.EvaluateInvariantXPath(strCapacity.Replace("Rating", Rating.ToString(GlobalOptions.InvariantCultureInfo)),
                            out bool blnIsSuccess);
                    strReturn = blnIsSuccess
                        ? ((double) objProcess).ToString("#,0.##", GlobalOptions.CultureInfo)
                        : strCapacity;
                    if (blnSquareBrackets)
                        strReturn = '[' + strReturn + ']';
                }
                else if (decimal.TryParse(strCapacity, NumberStyles.Any, GlobalOptions.InvariantCultureInfo,
                    out decimal decReturn))
                    return decReturn.ToString("#,0.##", GlobalOptions.CultureInfo);
                else
                {
                    // Just a straight Capacity, so return the value.
                    return strCapacity;
                }

                return strReturn;
            }
        }

        /// <summary>
        /// Calculated Essence cost of the Cyberware.
        /// </summary>
        public decimal CalculatedESS
        {
            get
            {
                if (PrototypeTranshuman)
                    return 0;
                return CalculatedESS;
            }
        }

        /// <summary>
        /// Calculated Essence cost of the Cyberware if Prototype Transhuman is ignored.
        /// </summary>
        public decimal CalculatedESSPrototypeInvariant
        {
            get
            {
                if (Parent != null && !AddToParentESS)
                    return 0;
                if (SourceID == EssenceHoleGUID) // Essence hole
                {
                    return Rating / 100m;
                }

                if (SourceID == EssenceAntiHoleGUID) // Essence anti-hole
                {
                    return Rating / -100m;
                }

                decimal decReturn;

                string strESS = ESS;
                if (strESS.StartsWith("FixedValues(", StringComparison.Ordinal))
                {
                    string strSuffix = string.Empty;
                    if (!strESS.EndsWith(')'))
                    {
                        strSuffix = strESS.Substring(strESS.LastIndexOf(')') + 1);
                        strESS = strESS.TrimEndOnce(strSuffix);
                    }

                    string[] strValues = strESS.TrimStartOnce("FixedValues(", true).TrimEndOnce(')').Split(',');
                    strESS = strValues[Math.Max(Math.Min(Rating, strValues.Length) - 1, 0)];
                    strESS += strSuffix;
                }

                if (strESS.Contains("Rating") || strESS.IndexOfAny(s_MathOperators) >= 0)
                {
                    // If the cost is determined by the Rating or there's a math operation in play, evaluate the expression.
                    object objProcess = CommonFunctions.EvaluateInvariantXPath(strESS.Replace("Rating", Rating.ToString(GlobalOptions.InvariantCultureInfo)),
                        out bool blnIsSuccess);
                    decReturn = blnIsSuccess ? Convert.ToDecimal(objProcess, GlobalOptions.InvariantCultureInfo) : 0;
                }
                else
                {
                    // Just a straight cost, so return the value.
                    decimal.TryParse(strESS, NumberStyles.Any, GlobalOptions.InvariantCultureInfo, out decReturn);
                }

                // Factor in the Essence multiplier of the selected CyberwareGrade.
                decimal decESSMultiplier = Grade.Essence + ExtraESSAdditiveMultiplier;
                decimal decTotalESSMultiplier = 1.0m * ExtraESSMultiplicativeMultiplier;

                if (_blnSuite)
                    decESSMultiplier -= 0.1m;

                if (ESSDiscount != 0)
                {
                    decimal decDiscount = ESSDiscount * 0.01m;
                    decTotalESSMultiplier *= 1.0m - decDiscount;
                }

                void UpdateMultipliers(Improvement.ImprovementType baseMultiplier, Improvement.ImprovementType totalMultiplier, ref decimal decMultiplier, ref decimal decTotalMultiplier)
                {
                    if (ImprovementManager.ValueOf(_objCharacter, baseMultiplier) != 0)
                    {
                        decMultiplier = _objCharacter.Improvements
                            .Where(objImprovement =>
                                objImprovement.ImproveType == baseMultiplier &&
                                objImprovement.Enabled)
                            .Aggregate(decMultiplier,
                                (current, objImprovement) =>
                                    current - (1m - objImprovement.Value / 100m));
                        decESSMultiplier = Math.Floor((decESSMultiplier - 1.0m + decMultiplier) * 10.0m) / 10;
                    }

                    if (totalMultiplier == Improvement.ImprovementType.None) return;
                    if (ImprovementManager.ValueOf(_objCharacter, totalMultiplier) != 0)
                    {
                        decTotalMultiplier = _objCharacter.Improvements
                            .Where(x => x.Enabled && x.ImproveType == totalMultiplier)
                            .Aggregate(decTotalESSMultiplier,
                                (current, objImprovement) =>
                                    current * (objImprovement.Value / 100m));
                    }
                }

                // Retrieve the Bioware, Geneware or Cyberware ESS Cost Multiplier.
                if (ForceGrade == "None" && !IsGeneware)
                {
                    decESSMultiplier = 1.0m;
                    decTotalESSMultiplier = 1.0m;
                }
                else
                {
                    decimal decMultiplier = 1;
                    switch (_objImprovementSource)
                    {
                        // Apply the character's Cyberware Essence cost multiplier if applicable.
                        case Improvement.ImprovementSource.Cyberware:
                            UpdateMultipliers(Improvement.ImprovementType.CyberwareEssCost, Improvement.ImprovementType.CyberwareTotalEssMultiplier, ref decMultiplier, ref decTotalESSMultiplier);
                            break;
                        // Apply the character's Bioware Essence cost multiplier if applicable.
                        case Improvement.ImprovementSource.Bioware when !IsGeneware:
                            UpdateMultipliers(Improvement.ImprovementType.BiowareEssCost, Improvement.ImprovementType.BiowareTotalEssMultiplier, ref decMultiplier, ref decTotalESSMultiplier);
                            break;
                        // Apply the character's Geneware Essence cost multiplier if applicable. Since Geneware does not use Grades, we only check the genetechessmultiplier improvement.
                        case Improvement.ImprovementSource.Bioware when IsGeneware:
                            UpdateMultipliers(Improvement.ImprovementType.GenetechEssMultiplier, Improvement.ImprovementType.None, ref decMultiplier, ref decTotalESSMultiplier);
                            break;
                    }

                    // Apply the character's Basic Bioware Essence cost multiplier if applicable.
                    if (_strCategory == "Basic" && _objImprovementSource == Improvement.ImprovementSource.Bioware &&
                        ImprovementManager.ValueOf(_objCharacter, Improvement.ImprovementType.BasicBiowareEssCost) != 0)
                    {
                        decimal decBasicMultiplier = _objCharacter.Improvements
                            .Where(objImprovement =>
                                objImprovement.ImproveType == Improvement.ImprovementType.BasicBiowareEssCost &&
                                objImprovement.Enabled)
                            .Aggregate<Improvement, decimal>(1,
                                (current, objImprovement) =>
                                    current - (1m - objImprovement.Value / 100m));
                        decESSMultiplier -= 1.0m - decBasicMultiplier;
                    }
                }

                decReturn = decReturn * decESSMultiplier * decTotalESSMultiplier;

                if (_objCharacter != null && !_objCharacter.Options.DontRoundEssenceInternally)
                    decReturn = decimal.Round(decReturn, _objCharacter.Options.EssenceDecimals,
                        MidpointRounding.AwayFromZero);
                decReturn += Children.Where(objChild => objChild.AddToParentESS).AsParallel()
                    .Sum(objChild => objChild.CalculatedESS);
                return decReturn;
            }
        }

        public int GetBaseMatrixAttribute(string strAttributeName)
        {
            string strExpression = this.GetMatrixAttributeString(strAttributeName);
            if (string.IsNullOrEmpty(strExpression))
            {
                switch (strAttributeName)
                {
                    case "Device Rating":
                        return _objGrade.DeviceRating;
                    case "Program Limit":
                        if (IsCommlink)
                        {
                            strExpression = this.GetMatrixAttributeString("Device Rating");
                            if (string.IsNullOrEmpty(strExpression))
                                return _objGrade.DeviceRating;
                        }
                        else
                            return _objGrade.DeviceRating;

                        break;
                    case "Data Processing":
                    case "Firewall":
                        strExpression = this.GetMatrixAttributeString("Device Rating");
                        if (string.IsNullOrEmpty(strExpression))
                            return _objGrade.DeviceRating;
                        break;
                    default:
                        return 0;
                }
            }

            if (strExpression.StartsWith("FixedValues(", StringComparison.Ordinal))
            {
                string[] strValues = strExpression.TrimStartOnce("FixedValues(", true).TrimEndOnce(')').Split(',');
                strExpression = strValues[Math.Max(0, Math.Min(Rating, strValues.Length) - 1)].Trim('[', ']');
            }

            if (strExpression.IndexOfAny('{', '+', '-', '*', ',') != -1 || strExpression.Contains("div"))
            {
                StringBuilder objValue = new StringBuilder(strExpression);
                objValue.Replace("{Rating}", Rating.ToString(GlobalOptions.InvariantCultureInfo));
                foreach (string strMatrixAttribute in MatrixAttributes.MatrixAttributeStrings)
                {
                    objValue.CheapReplace(strExpression, "{Gear " + strMatrixAttribute + "}",
                        () => (Parent?.GetBaseMatrixAttribute(strMatrixAttribute) ?? 0).ToString(GlobalOptions
                            .InvariantCultureInfo));
                    objValue.CheapReplace(strExpression, "{Parent " + strMatrixAttribute + "}",
                        () => (Parent?.GetMatrixAttributeString(strMatrixAttribute) ?? "0"));
                    if (Children.Count + Gear.Count > 0 &&
                        strExpression.Contains("{Children " + strMatrixAttribute + "}"))
                    {
                        int intTotalChildrenValue = 0;
                        foreach (Cyberware objLoopCyberware in Children)
                        {
                            if (objLoopCyberware.IsModularCurrentlyEquipped)
                            {
                                intTotalChildrenValue += objLoopCyberware.GetBaseMatrixAttribute(strMatrixAttribute);
                            }
                        }

                        foreach (Gear objLoopGear in Gear)
                        {
                            if (objLoopGear.Equipped)
                            {
                                intTotalChildrenValue += objLoopGear.GetBaseMatrixAttribute(strMatrixAttribute);
                            }
                        }

                        objValue.Replace("{Children " + strMatrixAttribute + "}",
                            intTotalChildrenValue.ToString(GlobalOptions.InvariantCultureInfo));
                    }
                }

                foreach (string strCharAttributeName in AttributeSection.AttributeStrings)
                {
                    objValue.CheapReplace(strExpression, "{" + strCharAttributeName + "}",
                        () => _objCharacter.GetAttribute(strCharAttributeName).TotalValue.ToString(GlobalOptions.InvariantCultureInfo));
                    objValue.CheapReplace(strExpression, "{" + strCharAttributeName + "Base}",
                        () => _objCharacter.GetAttribute(strCharAttributeName).TotalBase.ToString(GlobalOptions.InvariantCultureInfo));
                }

                // This is first converted to a decimal and rounded up since some items have a multiplier that is not a whole number, such as 2.5.
                object objProcess = CommonFunctions.EvaluateInvariantXPath(objValue.ToString(), out bool blnIsSuccess);
                return blnIsSuccess ? Convert.ToInt32(Math.Ceiling((double) objProcess)) : 0;
            }

            int.TryParse(strExpression, NumberStyles.Any, GlobalOptions.InvariantCultureInfo, out int intReturn);
            return intReturn;
        }

        public int GetBonusMatrixAttribute(string strAttributeName)
        {
            if (string.IsNullOrEmpty(strAttributeName))
                return 0;
            int intReturn = 0;

            if (Overclocked == strAttributeName)
            {
                intReturn += 1;
            }

            if (!strAttributeName.StartsWith("Mod ", StringComparison.Ordinal))
                strAttributeName = "Mod " + strAttributeName;

            foreach (Cyberware objLoopCyberware in Children)
            {
                if (objLoopCyberware.IsModularCurrentlyEquipped)
                {
                    intReturn += objLoopCyberware.GetTotalMatrixAttribute(strAttributeName);
                }
            }

            foreach (Gear objLoopGear in Gear)
            {
                if (objLoopGear.Equipped)
                {
                    intReturn += objLoopGear.GetTotalMatrixAttribute(strAttributeName);
                }
            }

            return intReturn;
        }

        /// <summary>
        /// Total cost of the just the Cyberware itself before we factor in any multipliers.
        /// </summary>
        public decimal OwnCostPreMultipliers
        {
            get
            {
                string strCostExpression = Cost;

                if (strCostExpression.StartsWith("FixedValues(", StringComparison.Ordinal))
                {
                    string strSuffix = string.Empty;
                    if (!strCostExpression.EndsWith(')'))
                    {
                        strSuffix = strCostExpression.Substring(strCostExpression.LastIndexOf(')') + 1);
                        strCostExpression = strCostExpression.TrimEndOnce(strSuffix);
                    }

                    string[] strValues = strCostExpression.TrimStartOnce("FixedValues(", true).TrimEndOnce(')')
                        .Split(',');
                    strCostExpression = strValues[Math.Max(Math.Min(Rating, strValues.Length) - 1, 0)].Trim('[', ']');
                    strCostExpression += strSuffix;
                }

                string strParentCost = "0";
                decimal decTotalParentGearCost = 0;
                if (_objParent != null)
                {
                    if (strCostExpression.Contains("Parent Cost"))
                        strParentCost = _objParent.Cost;
                    if (strCostExpression.Contains("Parent Gear Cost"))
                        foreach (Gear loopGear in _objParent.Gear)
                        {
                            decTotalParentGearCost += loopGear.CalculatedCost;
                        }
                }

                decimal decTotalGearCost = 0;
                if (Gear.Count > 0 && strCostExpression.Contains("Gear Cost"))
                {
                    foreach (Gear loopGear in Gear)
                    {
                        decTotalGearCost += loopGear.CalculatedCost;
                    }
                }

                decimal decTotalChildrenCost = 0;
                if (Children.Count > 0 && strCostExpression.Contains("Children Cost"))
                {
                    foreach (Cyberware loopWare in Children)
                    {
                        decTotalChildrenCost += loopWare.TotalCost;
                    }
                }

                if (string.IsNullOrEmpty(strCostExpression))
                    return 0;

                StringBuilder objCost = new StringBuilder(strCostExpression.TrimStart('+'));
                objCost.Replace("Parent Cost", strParentCost);
                objCost.Replace("Parent Gear Cost",
                    decTotalParentGearCost.ToString(GlobalOptions.InvariantCultureInfo));
                objCost.Replace("Gear Cost", decTotalGearCost.ToString(GlobalOptions.InvariantCultureInfo));
                objCost.Replace("Children Cost", decTotalChildrenCost.ToString(GlobalOptions.InvariantCultureInfo));
                objCost.CheapReplace(strCostExpression, "MinRating", () => MinRating.ToString(GlobalOptions.InvariantCultureInfo));
                objCost.Replace("Rating", Rating.ToString(GlobalOptions.InvariantCultureInfo));

                foreach (CharacterAttrib objLoopAttribute in _objCharacter.AttributeSection.AttributeList.Concat(
                    _objCharacter.AttributeSection.SpecialAttributeList))
                {
                    objCost.CheapReplace(strCostExpression, objLoopAttribute.Abbrev,
                        () => objLoopAttribute.TotalValue.ToString(GlobalOptions.InvariantCultureInfo));
                    objCost.CheapReplace(strCostExpression, objLoopAttribute.Abbrev + "Base",
                        () => objLoopAttribute.TotalBase.ToString(GlobalOptions.InvariantCultureInfo));
                }

                object objProcess = CommonFunctions.EvaluateInvariantXPath(objCost.ToString(), out bool blnIsSuccess);
                return blnIsSuccess ? Convert.ToDecimal(objProcess, GlobalOptions.InvariantCultureInfo) : 0;
            }
        }

        /// <summary>
        /// Total cost of the Cyberware and its plugins.
        /// </summary>
        public decimal TotalCost
        {
            get
            {
                decimal decReturn = TotalCostWithoutModifiers;

                if (_blnSuite)
                    decReturn *= 0.9m;

                return decReturn;
            }
        }

        /// <summary>
        /// Identical to TotalCost, but without the Improvement and Suite multipliers which would otherwise be doubled.
        /// </summary>
        private decimal TotalCostWithoutModifiers
        {
            get
            {
                decimal decCost = OwnCostPreMultipliers;
                decimal decReturn = decCost;

                // Factor in the Cost multiplier of the selected CyberwareGrade.
                decReturn *= Grade.Cost;

                if (DiscountCost)
                    decReturn *= 0.9m;

                // Genetech Cost multiplier.
                if (IsGeneware && ImprovementManager.ValueOf(_objCharacter, Improvement.ImprovementType.GenetechCostMultiplier) != 0)
                {
                    decimal decMultiplier = 1.0m;
                    foreach (Improvement objImprovement in _objCharacter.Improvements)
                    {
                        if (objImprovement.ImproveType == Improvement.ImprovementType.GenetechCostMultiplier &&
                            objImprovement.Enabled)
                            decMultiplier -=
                                1.0m - (objImprovement.Value / 100.0m);
                    }

                    decReturn *= decMultiplier;
                }

                // Add in the cost of all child components.
                foreach (Cyberware objChild in Children)
                {
                    if (objChild.Capacity != "[*]")
                    {
                        // If the child cost starts with "*", multiply the item's base cost.
                        if (objChild.Cost.StartsWith('*'))
                        {
                            decimal decPluginCost =
                                decCost * (Convert.ToDecimal(objChild.Cost.TrimStart('*'),
                                               GlobalOptions.InvariantCultureInfo) - 1);

                            if (objChild.DiscountCost)
                                decPluginCost *= 0.9m;

                            decReturn += decPluginCost;
                        }
                        else
                            decReturn += objChild.TotalCostWithoutModifiers;
                    }
                }

                // Add in the cost of all Gear plugins.
                foreach (Gear objGear in Gear)
                {
                    decReturn += objGear.TotalCost;
                }

                return decReturn;
            }
        }


        /// <summary>
        /// Identical to TotalCost, including the modifiers from Suite improvements.
        /// </summary>
        public decimal StolenTotalCost
        {
            get
            {
                decimal decCost = OwnCostPreMultipliers;
                decimal decReturn = decCost;

                // Factor in the Cost multiplier of the selected CyberwareGrade.
                decReturn *= Grade.Cost;

                if (DiscountCost)
                    decReturn *= 0.9m;

                // Add in the cost of all child components.
                foreach (Cyberware objChild in Children.Where(child => child.Stolen).AsParallel())
                {
                    if (objChild.Capacity == "[*]") continue;
                    // If the child cost starts with "*", multiply the item's base cost.
                    if (objChild.Cost.StartsWith('*'))
                    {
                        decimal decPluginCost =
                            decCost * (Convert.ToDecimal(objChild.Cost.TrimStart('*'),
                                           GlobalOptions.InvariantCultureInfo) - 1);

                        if (objChild.DiscountCost)
                            decPluginCost *= 0.9m;

                        decReturn += decPluginCost;
                    }
                    else
                        decReturn += objChild.TotalCostWithoutModifiers;
                }

                // Add in the cost of all Gear plugins.
                decReturn += Gear.Where(g => g.Stolen).AsParallel().Sum(objGear => objGear.StolenTotalCost);

                if (_blnSuite)
                    decReturn *= 0.9m;

                return decReturn;
            }
        }

        /// <summary>
        /// Cost of just the Cyberware itself.
        /// </summary>
        public decimal OwnCost
        {
            get
            {
                decimal decReturn = OwnCostPreMultipliers;

                // Factor in the Cost multiplier of the selected CyberwareGrade.
                decReturn *= Grade.Cost;

                if (DiscountCost)
                    decReturn *= 0.9m;

                if (_blnSuite)
                    decReturn *= 0.9m;

                return decReturn;
            }
        }

        /// <summary>
        /// The amount of Capacity remaining in the Cyberware.
        /// </summary>
        public decimal CapacityRemaining
        {
            get
            {
                decimal decCapacity = 0;
                if (Capacity.Contains("/["))
                {
                    // Get the Cyberware base Capacity.
                    string strBaseCapacity = CalculatedCapacity;
                    strBaseCapacity = strBaseCapacity.Substring(0, strBaseCapacity.IndexOf('/'));
                    decCapacity = Convert.ToDecimal(strBaseCapacity, GlobalOptions.CultureInfo);

                    // Run through its Children and deduct the Capacity costs.
                    foreach (Cyberware objChildCyberware in Children)
                    {
                        // Children that are built into the parent
                        if (objChildCyberware.PlugsIntoModularMount == HasModularMount &&
                            !string.IsNullOrWhiteSpace(HasModularMount) ||
                            objChildCyberware.ParentID == InternalId) continue;
                        string strCapacity = objChildCyberware.CalculatedCapacity;
                        int intPos = strCapacity.IndexOf("/[", StringComparison.Ordinal);
                        if (intPos != -1)
                            strCapacity = strCapacity.Substring(intPos + 2,
                                strCapacity.LastIndexOf(']') - intPos - 2);
                        else if (strCapacity.StartsWith('['))
                            strCapacity = strCapacity.Substring(1, strCapacity.Length - 2);
                        if (strCapacity == "*")
                            strCapacity = "0";
                        decCapacity -= Convert.ToDecimal(strCapacity, GlobalOptions.CultureInfo);
                    }

                    // Run through its Children and deduct the Capacity costs.
                    foreach (Gear objChildGear in Gear)
                    {
                        if (objChildGear.IncludedInParent)
                        {
                            continue;
                        }

                        string strCapacity = objChildGear.CalculatedCapacity;
                        int intPos = strCapacity.IndexOf("/[", StringComparison.Ordinal);
                        if (intPos != -1)
                            strCapacity = strCapacity.Substring(intPos + 2, strCapacity.LastIndexOf(']') - intPos - 2);
                        else if (strCapacity.StartsWith('['))
                            strCapacity = strCapacity.Substring(1, strCapacity.Length - 2);
                        if (strCapacity == "*")
                            strCapacity = "0";
                        decCapacity -= Convert.ToDecimal(strCapacity, GlobalOptions.CultureInfo);
                    }

                }
                else if (!Capacity.Contains('['))
                {
                    // Get the Cyberware base Capacity.
                    decCapacity = Convert.ToDecimal(CalculatedCapacity, GlobalOptions.CultureInfo);

                    // Run through its Children and deduct the Capacity costs.
                    foreach (Cyberware objChildCyberware in Children)
                    {
                        if (objChildCyberware.PlugsIntoModularMount == HasModularMount &&
                            !string.IsNullOrWhiteSpace(HasModularMount) ||
                            objChildCyberware.ParentID == InternalId) continue;
                        string strCapacity = objChildCyberware.CalculatedCapacity;
                        int intPos = strCapacity.IndexOf("/[", StringComparison.Ordinal);
                        if (intPos != -1)
                            strCapacity = strCapacity.Substring(intPos + 2, strCapacity.LastIndexOf(']') - intPos - 2);
                        else if (strCapacity.StartsWith('['))
                            strCapacity = strCapacity.Substring(1, strCapacity.Length - 2);
                        if (strCapacity == "*")
                            strCapacity = "0";
                        decCapacity -= Convert.ToDecimal(strCapacity, GlobalOptions.CultureInfo);
                    }

                    // Run through its Children and deduct the Capacity costs.
                    foreach (Gear objChildGear in Gear)
                    {
                        if (objChildGear.IncludedInParent)
                        {
                            continue;
                        }

                        string strCapacity = objChildGear.CalculatedCapacity;
                        int intPos = strCapacity.IndexOf("/[", StringComparison.Ordinal);
                        if (intPos != -1)
                            strCapacity = strCapacity.Substring(intPos + 2, strCapacity.LastIndexOf(']') - intPos - 2);
                        else if (strCapacity.StartsWith('['))
                            strCapacity = strCapacity.Substring(1, strCapacity.Length - 2);
                        if (strCapacity == "*")
                            strCapacity = "0";
                        decCapacity -= Convert.ToDecimal(strCapacity, GlobalOptions.CultureInfo);
                    }
                }

                return decCapacity;
            }
        }

        /// <summary>
        /// Base Cyberlimb Strength (before modifiers and customization).
        /// </summary>
        public int BaseStrength
        {
            get
            {
                if (_strCategory != "Cyberlimb")
                    return 0;
                if (ParentVehicle != null)
                    return Math.Max(ParentVehicle.TotalBody, 0);
                // Base Strength for any limb is 3.
                return 3;
            }
        }

        /// <summary>
        /// Cyberlimb Strength.
        /// </summary>
        public int TotalStrength
        {
            get
            {
                if (InheritAttributes)
                {
                    int intAverageAttribute = 0;
                    int intCyberlimbChildrenNumber = 0;
                    foreach (Cyberware objChild in Children)
                    {
                        if (objChild.TotalStrength <= 0) continue;
                        intCyberlimbChildrenNumber += 1;
                        intAverageAttribute += objChild.TotalStrength;
                    }

                    if (intCyberlimbChildrenNumber == 0)
                        intCyberlimbChildrenNumber = 1;

                    return intAverageAttribute / intCyberlimbChildrenNumber;
                }

                if (_strCategory != "Cyberlimb")
                {
                    return 0;
                }

                int intAttribute = BaseStrength;
                int intBonus = 0;

                if (Children.Count > 0)
                {
                    List<Cyberware> lstCustomizationWare = new List<Cyberware>(Children.Count);
                    List<Cyberware> lstEnhancementWare = new List<Cyberware>(Children.Count);
                    foreach (Cyberware objChild in Children)
                    {
                        if (s_StrengthCustomizationStrings.Contains(objChild.Name))
                            lstCustomizationWare.Add(objChild);
                        if (s_StrengthEnhancementStrings.Contains(objChild.Name))
                            lstEnhancementWare.Add(objChild);
                    }
                    if (lstCustomizationWare.Count > 0)
                    {
                        intAttribute = lstCustomizationWare.Count > 1
                            ? lstCustomizationWare.Max(s => s.Rating)
                            : lstCustomizationWare.First().Rating;
                    }
                    if (lstEnhancementWare.Count > 0)
                    {
                        intBonus = lstEnhancementWare.Count > 1
                            ? lstEnhancementWare.Max(s => s.Rating)
                            : lstEnhancementWare.First().Rating;
                    }
                }
                if (ParentVehicle == null)
                {
                    intBonus += _objCharacter.RedlinerBonus;
                }
                intBonus = Math.Min(intBonus, _objCharacter.Options.CyberlimbAttributeBonusCap);

                return ParentVehicle == null
                    ? Math.Min(intAttribute + intBonus, _objCharacter.STR.TotalAugmentedMaximum)
                    : Math.Min(intAttribute + intBonus, Math.Max(ParentVehicle.TotalBody * 2, 1));
            }
        }

        /// <summary>
        /// Base Cyberlimb Agility (before modifiers and customization).
        /// </summary>
        public int BaseAgility
        {
            get
            {
                if (_strCategory != "Cyberlimb")
                    return 0;
                if (ParentVehicle != null)
                    return Math.Max(ParentVehicle.Pilot, 0);
                // Base Agility for any limb is 3.
                return 3;
            }
        }

        /// <summary>
        /// Cyberlimb Agility.
        /// </summary>
        public int TotalAgility
        {
            get
            {
                if (InheritAttributes)
                {
                    int intAverageAttribute = 0;
                    int intCyberlimbChildrenNumber = 0;
                    foreach (Cyberware objChild in Children)
                    {
                        if (objChild.TotalAgility <= 0) continue;
                        intCyberlimbChildrenNumber += 1;
                        intAverageAttribute += objChild.TotalAgility;
                    }

                    if (intCyberlimbChildrenNumber == 0)
                        intCyberlimbChildrenNumber = 1;

                    return intAverageAttribute / intCyberlimbChildrenNumber;
                }

                if (_strCategory != "Cyberlimb")
                {
                    return 0;
                }

                int intAttribute = BaseAgility;
                int intBonus = 0;

                if (Children.Count > 0)
                {
                    List<Cyberware> lstCustomizationWare = new List<Cyberware>(Children.Count);
                    List<Cyberware> lstEnhancementWare = new List<Cyberware>(Children.Count);
                    foreach (Cyberware objChild in Children)
                    {
                        if (s_AgilityCustomizationStrings.Contains(objChild.Name))
                            lstCustomizationWare.Add(objChild);
                        if (s_AgilityEnhancementStrings.Contains(objChild.Name))
                            lstEnhancementWare.Add(objChild);
                    }
                    if (lstCustomizationWare.Count > 0)
                    {
                        intAttribute = lstCustomizationWare.Count > 1
                            ? lstCustomizationWare.Max(s => s.Rating)
                            : lstCustomizationWare.First().Rating;
                    }
                    if (lstEnhancementWare.Count > 0)
                    {
                        intBonus = lstEnhancementWare.Count > 1
                            ? lstEnhancementWare.Max(s => s.Rating)
                            : lstEnhancementWare.First().Rating;
                    }
                }
                if (ParentVehicle == null)
                {
                    intBonus += _objCharacter.RedlinerBonus;
                }
                intBonus = Math.Min(intBonus, _objCharacter.Options.CyberlimbAttributeBonusCap);

                return ParentVehicle == null
                    ? Math.Min(intAttribute + intBonus, _objCharacter.AGI.TotalAugmentedMaximum)
                    : Math.Min(intAttribute + intBonus, Math.Max(ParentVehicle.Pilot * 2, 1));
            }
        }

        public bool IsProgram => false;

        /// <summary>
        /// Device rating string for Cyberware. If it's empty, then GetBaseMatrixAttribute for Device Rating will fetch the grade's DR.
        /// </summary>
        public string DeviceRating
        {
            get => _strDeviceRating;
            set => _strDeviceRating = value;
        }

        /// <summary>
        /// Attack string (if one is explicitly specified for this 'ware).
        /// </summary>
        public string Attack
        {
            get => _strAttack;
            set => _strAttack = value;
        }

        /// <summary>
        /// Sleaze string (if one is explicitly specified for this 'ware).
        /// </summary>
        public string Sleaze
        {
            get => _strSleaze;
            set => _strSleaze = value;
        }

        /// <summary>
        /// Data Processing string (if one is explicitly specified for this 'ware).
        /// </summary>
        public string DataProcessing
        {
            get => _strDataProcessing;
            set => _strDataProcessing = value;
        }

        /// <summary>
        /// Firewall string (if one is explicitly specified for this 'ware).
        /// </summary>
        public string Firewall
        {
            get => _strFirewall;
            set => _strFirewall = value;
        }

        /// <summary>
        /// Modify Parent's Attack by this.
        /// </summary>
        public string ModAttack
        {
            get => _strModAttack;
            set => _strModAttack = value;
        }

        /// <summary>
        /// Modify Parent's Sleaze by this.
        /// </summary>
        public string ModSleaze
        {
            get => _strModSleaze;
            set => _strModSleaze = value;
        }

        /// <summary>
        /// Modify Parent's Data Processing by this.
        /// </summary>
        public string ModDataProcessing
        {
            get => _strModDataProcessing;
            set => _strModDataProcessing = value;
        }

        /// <summary>
        /// Modify Parent's Firewall by this.
        /// </summary>
        public string ModFirewall
        {
            get => _strModFirewall;
            set => _strModFirewall = value;
        }

        /// <summary>
        /// Cyberdeck's Attribute Array string.
        /// </summary>
        public string AttributeArray
        {
            get => _strAttributeArray;
            set => _strAttributeArray = value;
        }

        /// <summary>
        /// Modify Parent's Attribute Array by this.
        /// </summary>
        public string ModAttributeArray
        {
            get => _strModAttributeArray;
            set => _strModAttributeArray = value;
        }

        /// <summary>
        /// ASDF attribute boosted by Overclocker.
        /// </summary>
        public string Overclocked
        {
            get => _strOverclocked;
            set => _strOverclocked = value;
        }

        /// <summary>
        /// Empty for Cyberware.
        /// </summary>
        public string CanFormPersona
        {
            get => string.Empty;
            set { }
        }

        public bool IsCommlink => Gear.Any(x => x.CanFormPersona.Contains("Parent")) &&
                                  this.GetTotalMatrixAttribute("Device Rating") > 0;

        /// <summary>
        /// 0 for Cyberware.
        /// </summary>
        public int BonusMatrixBoxes
        {
            get => 0;
            set { }
        }

        public int TotalBonusMatrixBoxes
        {
            get
            {
                int intBonusBoxes = 0;
                foreach (Gear objGear in Gear)
                {
                    if (objGear.Equipped)
                    {
                        intBonusBoxes += objGear.TotalBonusMatrixBoxes;
                    }
                }

                return intBonusBoxes;
            }
        }

        /// <summary>
        /// Commlink's Limit for how many Programs they can run.
        /// </summary>
        public string ProgramLimit
        {
            get => _strProgramLimit;
            set => _strProgramLimit = value;
        }

        /// <summary>
        /// Returns true if this is a Cyberdeck whose attributes we could swap around.
        /// </summary>
        public bool CanSwapAttributes
        {
            get => _blnCanSwapAttributes;
            set => _blnCanSwapAttributes = value;
        }

        public IList<IHasMatrixAttributes> ChildrenWithMatrixAttributes =>
            Gear.Concat(Children.Cast<IHasMatrixAttributes>()).ToList();

        #endregion

        #region Methods

        /// <summary>
        /// Recursive method to delete a piece of 'ware and its Improvements from the character. Returns total extra cost removed unrelated to children.
        /// </summary>
        public decimal DeleteCyberware()
        {
            decimal decReturn = 0;
            // Remove any children the Gear may have.
            foreach (Cyberware objChild in Children)
                decReturn += objChild.DeleteCyberware();

            // Remove the Gear Weapon created by the Gear if applicable.
            if (!WeaponID.IsEmptyGuid())
            {
                List<Tuple<Weapon, Vehicle, VehicleMod, WeaponMount>> lstWeaponsToDelete =
                    new List<Tuple<Weapon, Vehicle, VehicleMod, WeaponMount>>();
                foreach (Weapon objWeapon in _objCharacter.Weapons.DeepWhere(x => x.Children,
                    x => x.ParentID == InternalId))
                {
                    lstWeaponsToDelete.Add(
                        new Tuple<Weapon, Vehicle, VehicleMod, WeaponMount>(objWeapon, null, null, null));
                }

                foreach (Vehicle objVehicle in _objCharacter.Vehicles)
                {
                    foreach (Weapon objWeapon in objVehicle.Weapons.DeepWhere(x => x.Children,
                        x => x.ParentID == InternalId))
                    {
                        lstWeaponsToDelete.Add(
                            new Tuple<Weapon, Vehicle, VehicleMod, WeaponMount>(objWeapon, objVehicle, null, null));
                    }

                    foreach (VehicleMod objMod in objVehicle.Mods)
                    {
                        foreach (Weapon objWeapon in objMod.Weapons.DeepWhere(x => x.Children,
                            x => x.ParentID == InternalId))
                        {
                            lstWeaponsToDelete.Add(
                                new Tuple<Weapon, Vehicle, VehicleMod, WeaponMount>(objWeapon, objVehicle, objMod,
                                    null));
                        }
                    }

                    foreach (WeaponMount objMount in objVehicle.WeaponMounts)
                    {
                        foreach (Weapon objWeapon in objMount.Weapons.DeepWhere(x => x.Children,
                            x => x.ParentID == InternalId))
                        {
                            lstWeaponsToDelete.Add(
                                new Tuple<Weapon, Vehicle, VehicleMod, WeaponMount>(objWeapon, objVehicle, null,
                                    objMount));
                        }
                    }
                }

                // We need this list separate because weapons to remove can contain gear that add more weapons in need of removing
                foreach (Tuple<Weapon, Vehicle, VehicleMod, WeaponMount> objLoopTuple in lstWeaponsToDelete)
                {
                    Weapon objWeapon = objLoopTuple.Item1;
                    decReturn += objWeapon.TotalCost + objWeapon.DeleteWeapon();
                    if (objWeapon.Parent != null)
                        objWeapon.Parent.Children.Remove(objWeapon);
                    else if (objLoopTuple.Item4 != null)
                        objLoopTuple.Item4.Weapons.Remove(objWeapon);
                    else if (objLoopTuple.Item3 != null)
                        objLoopTuple.Item3.Weapons.Remove(objWeapon);
                    else if (objLoopTuple.Item2 != null)
                        objLoopTuple.Item2.Weapons.Remove(objWeapon);
                    else
                        _objCharacter.Weapons.Remove(objWeapon);
                }
            }

            // Remove any Vehicle that the Cyberware created.
            if (!VehicleID.IsEmptyGuid())
            {
                List<Vehicle> lstVehiclesToRemove = new List<Vehicle>();
                foreach (Vehicle objLoopVehicle in _objCharacter.Vehicles)
                {
                    if (objLoopVehicle.ParentID == InternalId)
                    {
                        lstVehiclesToRemove.Add(objLoopVehicle);
                    }
                }

                foreach (Vehicle objLoopVehicle in lstVehiclesToRemove)
                {
                    decReturn += objLoopVehicle.TotalCost;
                    _objCharacter.Vehicles.Remove(objLoopVehicle);
                    foreach (Gear objLoopGear in objLoopVehicle.Gear)
                    {
                        decReturn += objLoopGear.DeleteGear();
                    }

                    foreach (Weapon objLoopWeapon in objLoopVehicle.Weapons)
                    {
                        decReturn += objLoopWeapon.DeleteWeapon();
                    }

                    foreach (VehicleMod objLoopMod in objLoopVehicle.Mods)
                    {
                        foreach (Weapon objLoopWeapon in objLoopMod.Weapons)
                        {
                            decReturn += objLoopWeapon.DeleteWeapon();
                        }

                        foreach (Cyberware objLoopCyberware in objLoopMod.Cyberware)
                        {
                            decReturn += objLoopCyberware.DeleteCyberware();
                        }
                    }

                    foreach (WeaponMount objLoopWeaponMount in objLoopVehicle.WeaponMounts)
                    {
                        foreach (Weapon objLoopWeapon in objLoopWeaponMount.Weapons)
                        {
                            decReturn += objLoopWeapon.DeleteWeapon();
                        }
                    }
                }
            }

            decReturn += ImprovementManager.RemoveImprovements(_objCharacter, SourceType, InternalId);
            decReturn += ImprovementManager.RemoveImprovements(_objCharacter, SourceType, InternalId + "Pair");
            if (PairBonus != null && !WirelessOn)
            {
                ImprovementManager.RemoveImprovements(_objCharacter, SourceType, InternalId + "Pair");
                // This cyberware should not be included in the count to make things easier.
                List<Cyberware> lstPairableCyberwares = _objCharacter.Cyberware.DeepWhere(x => x.Children,
                        x => x != this && IncludePair.Contains(x.Name) && x.Extra == Extra &&
                             x.IsModularCurrentlyEquipped)
                    .ToList();
                int intCount = lstPairableCyberwares.Count;
                // Need to use slightly different logic if this cyberware has a location (Left or Right) and only pairs with itself because Lefts can only be paired with Rights and Rights only with Lefts
                if (!string.IsNullOrEmpty(Location) && IncludePair.All(x => x == Name))
                {
                    int intMatchLocationCount = 0;
                    int intNotMatchLocationCount = 0;
                    foreach (Cyberware objPairableCyberware in lstPairableCyberwares)
                    {
                        if (objPairableCyberware.Location != Location)
                            intNotMatchLocationCount += 1;
                        else
                            intMatchLocationCount += 1;
                    }

                    // Set the count to the total number of cyberwares in matching pairs, which would mean 2x the number of whichever location contains the fewest members (since every single one of theirs would have a pair)
                    intCount = Math.Min(intMatchLocationCount, intNotMatchLocationCount) * 2;
                }

                foreach (Cyberware objLoopCyberware in lstPairableCyberwares)
                {
                    ImprovementManager.RemoveImprovements(_objCharacter, objLoopCyberware.SourceType,
                        objLoopCyberware.InternalId + "Pair");
                    // Go down the list and create pair bonuses for every second item
                    if (intCount > 0 && (intCount & 1) == 0)
                    {
                        ImprovementManager.CreateImprovements(_objCharacter, objLoopCyberware.SourceType,
                            objLoopCyberware.InternalId + "Pair", objLoopCyberware.PairBonus, objLoopCyberware.Rating,
                            objLoopCyberware.DisplayNameShort(GlobalOptions.Language));
                    }

                    intCount -= 1;
                }
            }

            decReturn += ImprovementManager.RemoveImprovements(_objCharacter, SourceType, InternalId + "WirelessPair");
            if (WirelessPairBonus != null)
            {
                ImprovementManager.RemoveImprovements(_objCharacter, SourceType, InternalId + "WirelessPair");
                // This cyberware should not be included in the count to make things easier.
                List<Cyberware> lstPairableCyberwares = _objCharacter.Cyberware.DeepWhere(x => x.Children,
                    x => x != this && IncludeWirelessPair.Contains(x.Name) && x.Extra == Extra &&
                         x.IsModularCurrentlyEquipped).ToList();
                int intCount = lstPairableCyberwares.Count;
                // Need to use slightly different logic if this cyberware has a location (Left or Right) and only pairs with itself because Lefts can only be paired with Rights and Rights only with Lefts
                if (!string.IsNullOrEmpty(Location) && IncludeWirelessPair.All(x => x == Name))
                {
                    int intMatchLocationCount = 0;
                    int intNotMatchLocationCount = 0;
                    foreach (Cyberware objPairableCyberware in lstPairableCyberwares)
                    {
                        if (objPairableCyberware.Location != Location)
                            intNotMatchLocationCount += 1;
                        else
                            intMatchLocationCount += 1;
                    }

                    // Set the count to the total number of cyberwares in matching pairs, which would mean 2x the number of whichever location contains the fewest members (since every single one of theirs would have a pair)
                    intCount = Math.Min(intMatchLocationCount, intNotMatchLocationCount) * 2;
                }

                foreach (Cyberware objLoopCyberware in lstPairableCyberwares)
                {
                    ImprovementManager.RemoveImprovements(_objCharacter, objLoopCyberware.SourceType,
                        objLoopCyberware.InternalId + "WirelessPair");
                    if (objLoopCyberware.WirelessPairBonus?.Attributes?["mode"]?.InnerText == "replace")
                    {
                        ImprovementManager.RemoveImprovements(_objCharacter, objLoopCyberware.SourceType,
                            objLoopCyberware.InternalId);
                    }
                    // Go down the list and create pair bonuses for every second item
                    if (intCount > 0 && intCount % 2 == 0)
                    {
                        ImprovementManager.CreateImprovements(_objCharacter, objLoopCyberware.SourceType,
                            objLoopCyberware.InternalId + "WirelessPair", objLoopCyberware.WirelessPairBonus, objLoopCyberware.Rating,
                            objLoopCyberware.DisplayNameShort(GlobalOptions.Language));
                    }

                    intCount -= 1;
                }
            }

            foreach (Gear objLoopGear in Gear)
            {
                decReturn += objLoopGear.DeleteGear();
            }

            // Fix for legacy characters with old addqualities improvements.
            XmlNodeList xmlOldAddQualitiesList = GetNode()?.SelectNodes("addqualities/addquality");
            if (xmlOldAddQualitiesList != null)
            {
                foreach (XmlNode objNode in xmlOldAddQualitiesList)
                {
                    Quality objQuality = _objCharacter.Qualities.FirstOrDefault(x => x.Name == objNode.InnerText);
                    if (objQuality == null) continue;
                    _objCharacter.Qualities.Remove(objQuality);
                    decReturn += ImprovementManager.RemoveImprovements(_objCharacter,
                        Improvement.ImprovementSource.CritterPower, objQuality.InternalId);
                }
            }

            return decReturn;
        }

        /// <summary>
        /// Checks a nominated piece of gear for Availability requirements.
        /// </summary>
        /// <param name="blnRestrictedGearUsed">Whether Restricted Gear is already being used.</param>
        /// <param name="intRestrictedCount">Amount of gear that is currently over the availability limit.</param>
        /// <param name="strAvailItems">String used to list names of gear that are currently over the availability limit.</param>
        /// <param name="strRestrictedItem">Item that is being used for Restricted Gear.</param>
        /// <param name="strCyberwareGrade">String used to list names of cyberware that have a banned cyberware grade.</param>
        /// <param name="blnOutRestrictedGearUsed">Whether Restricted Gear is already being used (tracked across gear children).</param>
        /// <param name="intOutRestrictedCount">Amount of gear that is currently over the availability limit (tracked across gear children).</param>
        /// <param name="strOutAvailItems">String used to list names of gear that are currently over the availability limit (tracked across gear children).</param>
        /// <param name="strOutRestrictedItem">Item that is being used for Restricted Gear (tracked across gear children).</param>
        /// <param name="strOutCyberwareGrade">String used to return list names of cyberware that have a banned cyberware grade</param>
        public void CheckRestrictedGear(bool blnRestrictedGearUsed, int intRestrictedCount, string strAvailItems,
            string strRestrictedItem, string strCyberwareGrade, out bool blnOutRestrictedGearUsed,
            out int intOutRestrictedCount, out string strOutAvailItems, out string strOutRestrictedItem,
            out string strOutCyberwareGrade)
        {
            if (string.IsNullOrEmpty(ParentID))
            {
                if (_objCharacter.BannedWareGrades.Any(s => Grade.Name.Contains(s)))
                    strCyberwareGrade += Environment.NewLine + "\t\t" + DisplayNameShort(GlobalOptions.Language);

                AvailabilityValue objTotalAvail = TotalAvailTuple();
                if (!objTotalAvail.AddToParent)
                {
                    int intAvailInt = objTotalAvail.Value;
                    if (intAvailInt > _objCharacter.MaximumAvailability)
                    {
                        if (intAvailInt <= _objCharacter.RestrictedGear && !blnRestrictedGearUsed)
                        {
                            blnRestrictedGearUsed = true;
                            strRestrictedItem = CurrentDisplayName;
                        }
                        else
                        {
                            intRestrictedCount++;
                            strAvailItems += Environment.NewLine + "\t\t" + DisplayNameShort(GlobalOptions.Language);
                        }
                    }
                }
            }

            foreach (Cyberware objChild in Children)
            {
                objChild.CheckRestrictedGear(blnRestrictedGearUsed, intRestrictedCount, strAvailItems,
                    strRestrictedItem, strCyberwareGrade, out blnRestrictedGearUsed, out intRestrictedCount,
                    out strAvailItems, out strRestrictedItem, out strCyberwareGrade);
            }

            foreach (Gear objChild in Gear)
            {
                objChild.CheckRestrictedGear(blnRestrictedGearUsed, intRestrictedCount, strAvailItems,
                    strRestrictedItem, out blnRestrictedGearUsed, out intRestrictedCount, out strAvailItems,
                    out strRestrictedItem);
            }

            strOutAvailItems = strAvailItems;
            intOutRestrictedCount = intRestrictedCount;
            blnOutRestrictedGearUsed = blnRestrictedGearUsed;
            strOutRestrictedItem = strRestrictedItem;
            strOutCyberwareGrade = strCyberwareGrade;
        }

        #region UI Methods

        /// <summary>
        /// Build up the Tree for the current piece of Cyberware and all of its children.
        /// </summary>
        /// <param name="cmsCyberware">ContextMenuStrip that the new Cyberware TreeNodes should use.</param>
        /// <param name="cmsGear">ContextMenuStrip that the new Gear TreeNodes should use.</param>
        public TreeNode CreateTreeNode(ContextMenuStrip cmsCyberware, ContextMenuStrip cmsGear)
        {
            if (!string.IsNullOrEmpty(ParentID) && !string.IsNullOrEmpty(Source) &&
                !_objCharacter.Options.BookEnabled(Source))
                return null;

            TreeNode objNode = new TreeNode
            {
                Name = InternalId,
                Text = CurrentDisplayName,
                Tag = this,
                ContextMenuStrip = cmsCyberware,
                ForeColor = PreferredColor,
                ToolTipText = Notes.WordWrap(100)
            };

            TreeNodeCollection lstChildNodes = objNode.Nodes;
            foreach (Cyberware objChild in Children)
            {
                TreeNode objLoopNode = objChild.CreateTreeNode(cmsCyberware, cmsGear);
                if (objLoopNode != null)
                    lstChildNodes.Add(objLoopNode);
            }

            foreach (Gear objGear in Gear)
            {
                TreeNode objLoopNode = objGear.CreateTreeNode(cmsGear);
                if (objLoopNode != null)
                    lstChildNodes.Add(objLoopNode);
            }

            if (lstChildNodes.Count > 0)
                objNode.Expand();

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

                if (!string.IsNullOrEmpty(ParentID))
                {
                    return SystemColors.GrayText;
                }

                return SystemColors.WindowText;
            }
        }

        public void SetupChildrenCyberwareCollectionChanged(bool blnAdd, TreeView treCyberware,
            ContextMenuStrip cmsCyberware = null, ContextMenuStrip cmsCyberwareGear = null)
        {
            if (blnAdd)
            {
                Children.AddTaggedCollectionChanged(treCyberware,
                    (x, y) => this.RefreshChildrenCyberware(treCyberware, cmsCyberware, cmsCyberwareGear, null, y));
                Gear.AddTaggedCollectionChanged(treCyberware,
                    (x, y) => this.RefreshChildrenGears(treCyberware, cmsCyberwareGear, () => Children.Count, y));

                foreach (Cyberware objChild in Children)
                    objChild.SetupChildrenCyberwareCollectionChanged(true, treCyberware, cmsCyberware,
                        cmsCyberwareGear);
                foreach (Gear objGear in Gear)
                    objGear.SetupChildrenGearsCollectionChanged(true, treCyberware, cmsCyberwareGear);
            }
            else
            {
                Children.RemoveTaggedCollectionChanged(treCyberware);
                Gear.RemoveTaggedCollectionChanged(treCyberware);
                foreach (Cyberware objChild in Children)
                    objChild.SetupChildrenCyberwareCollectionChanged(false, treCyberware);
                foreach (Gear objGear in Gear)
                    objGear.SetupChildrenGearsCollectionChanged(false, treCyberware);
            }
        }

        #endregion

        #region Hero Lab Importing

        public bool ImportHeroLabCyberware(XmlNode xmlCyberwareImportNode, XmlNode xmlParentCyberwareNode,
            IList<Weapon> lstWeapons, IList<Vehicle> lstVehicles, Grade objSelectedGrade = null)
        {
            if (xmlCyberwareImportNode == null)
                return false;
            bool blnCyberware = true;
            string strGradeName = objSelectedGrade?.Name ?? "Standard";
            string strOriginalName = xmlCyberwareImportNode.Attributes?["name"]?.InnerText ?? string.Empty;
            if (!string.IsNullOrEmpty(strOriginalName))
            {
                IList<Grade> objCyberwareGradeList =
                    _objCharacter.GetGradeList(Improvement.ImprovementSource.Cyberware);
                IList<Grade> objBiowareGradeList = _objCharacter.GetGradeList(Improvement.ImprovementSource.Bioware);
                if (objSelectedGrade == null)
                {
                    foreach (Grade objCyberwareGrade in objCyberwareGradeList)
                    {
                        if (strOriginalName.EndsWith(" (" + objCyberwareGrade.Name + ')', StringComparison.Ordinal))
                        {
                            strGradeName = objCyberwareGrade.Name;
                            strOriginalName = strOriginalName.TrimEndOnce(" (" + objCyberwareGrade.Name + ')');
                            goto EndGradeCheck;
                        }
                    }

                    foreach (Grade objCyberwareGrade in objBiowareGradeList)
                    {
                        if (strOriginalName.EndsWith(" (" + objCyberwareGrade.Name + ')', StringComparison.Ordinal))
                        {
                            strGradeName = objCyberwareGrade.Name;
                            strOriginalName = strOriginalName.TrimEndOnce(" (" + objCyberwareGrade.Name + ')');
                            goto EndGradeCheck;
                        }
                    }

                    EndGradeCheck: ;
                }

                XmlDocument xmlCyberwareDocument = XmlManager.Load("cyberware.xml");
                XmlDocument xmlBiowareDocument = XmlManager.Load("bioware.xml");
                string strForceValue = string.Empty;
                XmlNode xmlCyberwareDataNode = null;
                XmlNodeList xmlCyberwareNodeList =
                    xmlCyberwareDocument.SelectNodes("/chummer/cyberwares/cyberware[contains(name, \"" +
                                                     strOriginalName + "\")]");
                if (xmlCyberwareNodeList != null)
                {
                    foreach (XmlNode xmlLoopNode in xmlCyberwareNodeList)
                    {
                        XmlNode xmlTestNode = xmlLoopNode.SelectSingleNode("forbidden/parentdetails");
                        if (xmlTestNode != null)
                        {
                            // Assumes topmost parent is an AND node
                            if (xmlParentCyberwareNode.ProcessFilterOperationNode(xmlTestNode, false))
                            {
                                continue;
                            }
                        }

                        xmlTestNode = xmlLoopNode.SelectSingleNode("required/parentdetails");
                        if (xmlTestNode != null)
                        {
                            // Assumes topmost parent is an AND node
                            if (!xmlParentCyberwareNode.ProcessFilterOperationNode(xmlTestNode, false))
                            {
                                continue;
                            }
                        }

                        xmlCyberwareDataNode = xmlLoopNode;
                        break;
                    }
                }

                if (xmlCyberwareDataNode == null)
                {
                    blnCyberware = false;
                    xmlCyberwareNodeList =
                        xmlBiowareDocument.SelectNodes("/chummer/biowares/bioware[contains(name, \"" + strOriginalName +
                                                       "\")]");
                    if (xmlCyberwareNodeList != null)
                    {
                        foreach (XmlNode xmlLoopNode in xmlCyberwareNodeList)
                        {
                            XmlNode xmlTestNode = xmlLoopNode.SelectSingleNode("forbidden/parentdetails");
                            if (xmlTestNode != null)
                            {
                                // Assumes topmost parent is an AND node
                                if (xmlParentCyberwareNode.ProcessFilterOperationNode(xmlTestNode, false))
                                {
                                    continue;
                                }
                            }

                            xmlTestNode = xmlLoopNode.SelectSingleNode("required/parentdetails");
                            if (xmlTestNode != null)
                            {
                                // Assumes topmost parent is an AND node
                                if (!xmlParentCyberwareNode.ProcessFilterOperationNode(xmlTestNode, false))
                                {
                                    continue;
                                }
                            }

                            xmlCyberwareDataNode = xmlLoopNode;
                            break;
                        }
                    }
                }

                if (xmlCyberwareDataNode == null)
                {
                    string[] astrOriginalNameSplit = strOriginalName.Split(':');
                    if (astrOriginalNameSplit.Length > 1)
                    {
                        string strName = astrOriginalNameSplit[0].Trim();
                        blnCyberware = true;
                        xmlCyberwareNodeList =
                            xmlCyberwareDocument.SelectNodes(
                                "/chummer/cyberwares/cyberware[contains(name, \"" + strName + "\")]");
                        if (xmlCyberwareNodeList != null)
                        {
                            foreach (XmlNode xmlLoopNode in xmlCyberwareNodeList)
                            {
                                XmlNode xmlTestNode = xmlLoopNode.SelectSingleNode("forbidden/parentdetails");
                                if (xmlTestNode != null)
                                {
                                    // Assumes topmost parent is an AND node
                                    if (xmlParentCyberwareNode.ProcessFilterOperationNode(xmlTestNode, false))
                                    {
                                        continue;
                                    }
                                }

                                xmlTestNode = xmlLoopNode.SelectSingleNode("required/parentdetails");
                                if (xmlTestNode != null)
                                {
                                    // Assumes topmost parent is an AND node
                                    if (!xmlParentCyberwareNode.ProcessFilterOperationNode(xmlTestNode, false))
                                    {
                                        continue;
                                    }
                                }

                                xmlCyberwareDataNode = xmlLoopNode;
                                break;
                            }

                            if (xmlCyberwareDataNode != null)
                                strForceValue = astrOriginalNameSplit[1].Trim();
                            else
                            {
                                blnCyberware = false;
                                xmlCyberwareNodeList =
                                    xmlBiowareDocument.SelectNodes(
                                        "/chummer/biowares/bioware[contains(name, \"" + strName + "\")]");
                                if (xmlCyberwareNodeList != null)
                                    foreach (XmlNode xmlLoopNode in xmlCyberwareNodeList)
                                    {
                                        XmlNode xmlTestNode = xmlLoopNode.SelectSingleNode("forbidden/parentdetails");
                                        if (xmlTestNode != null)
                                        {
                                            // Assumes topmost parent is an AND node
                                            if (xmlParentCyberwareNode.ProcessFilterOperationNode(xmlTestNode, false))
                                            {
                                                continue;
                                            }
                                        }

                                        xmlTestNode = xmlLoopNode.SelectSingleNode("required/parentdetails");
                                        if (xmlTestNode != null)
                                        {
                                            // Assumes topmost parent is an AND node
                                            if (!xmlParentCyberwareNode.ProcessFilterOperationNode(xmlTestNode, false))
                                            {
                                                continue;
                                            }
                                        }

                                        xmlCyberwareDataNode = xmlLoopNode;
                                        break;
                                    }

                                if (xmlCyberwareDataNode != null)
                                    strForceValue = astrOriginalNameSplit[1].Trim();
                            }
                        }
                    }

                    if (xmlCyberwareDataNode == null)
                    {
                        astrOriginalNameSplit = strOriginalName.Split(',');
                        if (astrOriginalNameSplit.Length > 1)
                        {
                            string strName = astrOriginalNameSplit[0].Trim();
                            blnCyberware = true;
                            xmlCyberwareNodeList =
                                xmlCyberwareDocument.SelectNodes(
                                    "/chummer/cyberwares/cyberware[contains(name, \"" + strName + "\")]");
                            if (xmlCyberwareNodeList != null)
                            {
                                foreach (XmlNode xmlLoopNode in xmlCyberwareNodeList)
                                {
                                    XmlNode xmlTestNode = xmlLoopNode.SelectSingleNode("forbidden/parentdetails");
                                    if (xmlTestNode != null)
                                    {
                                        // Assumes topmost parent is an AND node
                                        if (xmlParentCyberwareNode.ProcessFilterOperationNode(xmlTestNode, false))
                                        {
                                            continue;
                                        }
                                    }

                                    xmlTestNode = xmlLoopNode.SelectSingleNode("required/parentdetails");
                                    if (xmlTestNode != null)
                                    {
                                        // Assumes topmost parent is an AND node
                                        if (!xmlParentCyberwareNode.ProcessFilterOperationNode(xmlTestNode, false))
                                        {
                                            continue;
                                        }
                                    }

                                    xmlCyberwareDataNode = xmlLoopNode;
                                    break;
                                }

                                if (xmlCyberwareDataNode != null)
                                    strForceValue = astrOriginalNameSplit[1].Trim();
                                else
                                {
                                    blnCyberware = false;
                                    xmlCyberwareNodeList =
                                        xmlBiowareDocument.SelectNodes(
                                            "/chummer/biowares/bioware[contains(name, \"" + strName + "\")]");
                                    if (xmlCyberwareNodeList != null)
                                        foreach (XmlNode xmlLoopNode in xmlCyberwareNodeList)
                                        {
                                            XmlNode xmlTestNode =
                                                xmlLoopNode.SelectSingleNode("forbidden/parentdetails");
                                            if (xmlTestNode != null)
                                            {
                                                // Assumes topmost parent is an AND node
                                                if (xmlParentCyberwareNode.ProcessFilterOperationNode(xmlTestNode,
                                                    false))
                                                {
                                                    continue;
                                                }
                                            }

                                            xmlTestNode = xmlLoopNode.SelectSingleNode("required/parentdetails");
                                            if (xmlTestNode != null)
                                            {
                                                // Assumes topmost parent is an AND node
                                                if (!xmlParentCyberwareNode.ProcessFilterOperationNode(xmlTestNode,
                                                    false))
                                                {
                                                    continue;
                                                }
                                            }

                                            xmlCyberwareDataNode = xmlLoopNode;
                                            break;
                                        }
                                }
                            }
                        }
                    }
                }

                if (xmlCyberwareDataNode != null)
                {
                    if (objSelectedGrade == null)
                    {
                        objSelectedGrade =
                            (blnCyberware ? objCyberwareGradeList : objBiowareGradeList).FirstOrDefault(x =>
                                x.Name == strGradeName);
                    }

                    Create(xmlCyberwareDataNode, objSelectedGrade,
                        blnCyberware ? Improvement.ImprovementSource.Cyberware : Improvement.ImprovementSource.Bioware,
                        Convert.ToInt32(xmlCyberwareImportNode.Attributes?["rating"]?.InnerText, GlobalOptions.InvariantCultureInfo), lstWeapons,
                        lstVehicles, true, true, strForceValue);
                    Notes = xmlCyberwareImportNode["description"]?.InnerText;

                    ProcessHeroLabCyberwarePlugins(xmlCyberwareImportNode, objSelectedGrade, lstWeapons, lstVehicles);

                    return true;
                }
            }

            return false;
        }

        public void ProcessHeroLabCyberwarePlugins(XmlNode xmlGearImportNode, Grade objParentGrade,
            IList<Weapon> lstWeapons, IList<Vehicle> lstVehicles)
        {
            if (xmlGearImportNode == null)
                return;
            foreach (string strPluginNodeName in Character.HeroLabPluginNodeNames)
            {
                XmlNodeList xmlPluginList = xmlGearImportNode.SelectNodes(strPluginNodeName + "/item[@useradded != \"no\"]");
                if (xmlPluginList != null)
                {
                    foreach (XmlNode xmlPluginToAdd in xmlPluginList)
                    {
                        Cyberware objPlugin = new Cyberware(_objCharacter);
                        if (objPlugin.ImportHeroLabCyberware(xmlPluginToAdd, GetNode(), lstWeapons, lstVehicles,
                            objParentGrade))
                        {
                            objPlugin.Parent = this;
                            Children.Add(objPlugin);
                        }
                        else
                        {
                            Gear objPluginGear = new Gear(_objCharacter);
                            if (objPluginGear.ImportHeroLabGear(xmlPluginToAdd, GetNode(), lstWeapons))
                            {
                                objPluginGear.Parent = this;
                                Gear.Add(objPluginGear);
                            }
                        }
                    }
                }

                xmlPluginList = xmlGearImportNode.SelectNodes(strPluginNodeName + "/item[@useradded = \"no\"]");
                if (xmlPluginList != null)
                {
                    foreach (XmlNode xmlPluginToAdd in xmlPluginList)
                    {
                        string strName = xmlPluginToAdd.Attributes?["name"]?.InnerText ?? string.Empty;
                        if (!string.IsNullOrEmpty(strName))
                        {
                            Cyberware objPlugin = Children.FirstOrDefault(x =>
                                x.ParentID == InternalId && (x.Name.Contains(strName) || strName.Contains(x.Name)));
                            if (objPlugin != null)
                            {
                                objPlugin.Notes = xmlPluginToAdd["description"]?.InnerText;
                                objPlugin.ProcessHeroLabCyberwarePlugins(xmlPluginToAdd, objParentGrade, lstWeapons,
                                    lstVehicles);
                            }
                            else
                            {
                                Gear objPluginGear = Gear.FirstOrDefault(x =>
                                    x.IncludedInParent && (x.Name.Contains(strName) || strName.Contains(x.Name)));
                                if (objPluginGear != null)
                                {
                                    objPluginGear.Quantity =
                                        Convert.ToDecimal(xmlPluginToAdd.Attributes?["quantity"]?.InnerText ?? "1",
                                            GlobalOptions.InvariantCultureInfo);
                                    objPluginGear.Notes = xmlPluginToAdd["description"]?.InnerText;
                                    objPluginGear.ProcessHeroLabGearPlugins(xmlPluginToAdd, lstWeapons);
                                }
                            }
                        }
                    }
                }
            }

            this.RefreshMatrixAttributeArray();
        }

        #endregion

        #endregion

        public bool Remove(bool blnConfirmDelete = true)
        {
            if (Capacity == "[*]" && Parent != null && (!_objCharacter.IgnoreRules || _objCharacter.Created))
            {
                Program.MainForm.ShowMessageBox(
                    LanguageManager.GetString("Message_CannotRemoveCyberware"),
                    LanguageManager.GetString("MessageTitle_CannotRemoveCyberware"),
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }

            if (blnConfirmDelete)
            {
                if (SourceType == Improvement.ImprovementSource.Bioware)
                {
                    if (!_objCharacter.ConfirmDelete(LanguageManager.GetString("Message_DeleteBioware",
                        GlobalOptions.Language)))
                        return false;
                }
                else
                {
                    if (!_objCharacter.ConfirmDelete(LanguageManager.GetString("Message_DeleteCyberware",
                        GlobalOptions.Language)))
                        return false;
                }
            }

            if (ParentVehicle != null)
            {
                _objCharacter.Vehicles.FindVehicleCyberware(x => x.InternalId == InternalId,
                    out VehicleMod objMod);
                objMod.Cyberware.Remove(this);
            }
            else if (Parent != null)
                Parent.Children.Remove(this);
            else
            {
                _objCharacter.Cyberware.Remove(this);
            }

            DeleteCyberware();
            return true;
        }

        public void Sell(decimal percentage)
        {
            // Create the Expense Log Entry for the sale.
            decimal decAmount = TotalCost * percentage;
            ExpenseLogEntry objExpense = new ExpenseLogEntry(_objCharacter);
            string strEntry = LanguageManager.GetString(
                SourceType == Improvement.ImprovementSource.Cyberware
                    ? "String_ExpenseSoldCyberware"
                    : "String_ExpenseSoldBioware");
            decAmount += DeleteCyberware() * percentage;
            objExpense.Create(decAmount, strEntry + ' ' + DisplayNameShort(GlobalOptions.Language), ExpenseType.Nuyen,
                DateTime.Now);
            _objCharacter.ExpenseEntries.AddWithSort(objExpense);
            _objCharacter.Nuyen += decAmount;

            if (Parent != null)
                Parent.Children.Remove(this);
            else
                _objCharacter.Cyberware.Remove(this);
        }

        /// <summary>
        /// Purchases a selected piece of Cyberware with a given Grade and Rating.
        /// </summary>
        /// <param name="objNode"></param>
        /// <param name="objGrade"></param>
        /// <param name="objImprovementSource"></param>
        /// <param name="intRating"></param>
        /// <param name="objVehicle"></param>
        /// <param name="lstCyberwareCollection"></param>
        /// <param name="lstVehicleCollection"></param>
        /// <param name="lstWeaponCollection"></param>
        /// <param name="decMarkup"></param>
        /// <param name="blnFree"></param>
        /// <param name="blnBlackMarket"></param>
        /// <param name="blnForVehicle"></param>
        /// <param name="strExpenseString"></param>
        /// <returns></returns>
        public bool Purchase(XmlNode objNode, Improvement.ImprovementSource objImprovementSource, Grade objGrade,
            int intRating, Vehicle objVehicle, TaggedObservableCollection<Cyberware> lstCyberwareCollection,
            ObservableCollection<Vehicle> lstVehicleCollection, TaggedObservableCollection<Weapon> lstWeaponCollection,
            decimal decMarkup = 0, bool blnFree = false, bool blnBlackMarket = false, bool blnForVehicle = false,
            string strExpenseString = "String_ExpensePurchaseCyberware")
        {
            // Create the Cyberware object.
            List<Weapon> lstWeapons = new List<Weapon>();
            List<Vehicle> lstVehicles = new List<Vehicle>();
            Create(objNode, objGrade, objImprovementSource, intRating, lstWeapons, lstVehicles, true, true,
                string.Empty, null, objVehicle);
            if (InternalId.IsEmptyGuid())
            {
                return false;
            }

            if (blnFree)
                Cost = "0";
            DiscountCost = blnBlackMarket;

            if (_objCharacter.Created)
            {
                decimal decCost = 0;
                // Check the item's Cost and make sure the character can afford it.
                if (!blnFree)
                {
                    decCost = TotalCost;

                    // Multiply the cost if applicable.
                    char chrAvail = TotalAvailTuple().Suffix;
                    if (chrAvail == 'R' && _objCharacter.Options.MultiplyRestrictedCost)
                        decCost *= _objCharacter.Options.RestrictedCostMultiplier;
                    if (chrAvail == 'F' && _objCharacter.Options.MultiplyForbiddenCost)
                        decCost *= _objCharacter.Options.ForbiddenCostMultiplier;

                    // Apply a markup if applicable.
                    if (decMarkup != 0)
                    {
                        decCost *= 1 + (decMarkup / 100.0m);
                    }

                    if (decCost > _objCharacter.Nuyen)
                    {
                        Program.MainForm.ShowMessageBox(
                            LanguageManager.GetString("Message_NotEnoughNuyen"),
                            LanguageManager.GetString("MessageTitle_NotEnoughNuyen"),
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                        return false;
                    }
                }

                // Create the Expense Log Entry.
                ExpenseLogEntry objExpense = new ExpenseLogEntry(_objCharacter);
                string strEntry = LanguageManager.GetString(strExpenseString);
                string strName = DisplayNameShort(GlobalOptions.Language);
                if (SourceID == EssenceHoleGUID || SourceID == EssenceAntiHoleGUID)
                {
                    strName += LanguageManager.GetString("String_Space") + '(' +
                               Rating.ToString(GlobalOptions.CultureInfo) + ')';
                }

                objExpense.Create(decCost * -1,
                    strEntry + LanguageManager.GetString("String_Space") +
                    strName, ExpenseType.Nuyen, DateTime.Now);
                _objCharacter.ExpenseEntries.AddWithSort(objExpense);
                _objCharacter.Nuyen -= decCost;

                if (SourceID != EssenceHoleGUID && SourceID != EssenceAntiHoleGUID)
                {
                    ExpenseUndo objUndo = new ExpenseUndo();
                    objUndo.CreateNuyen(
                        blnForVehicle ? NuyenExpenseType.AddVehicleModCyberware : NuyenExpenseType.AddCyberware,
                        InternalId);
                    objExpense.Undo = objUndo;
                }
            }

            if (SourceID == EssenceAntiHoleGUID)
            {
                _objCharacter.DecreaseEssenceHole(Rating);
            }
            else if (SourceID == EssenceHoleGUID)
            {
                _objCharacter.IncreaseEssenceHole(Rating);
            }
            else
            {
                if (_objCharacter.Created && objVehicle == null && _objParent == null)
                {
                    _objCharacter.DecreaseEssenceHole((int) (CalculatedESS * 100),
                        SourceID == Cyberware.EssenceAntiHoleGUID);
                }

                lstCyberwareCollection?.Add(this);

                foreach (Weapon objWeapon in lstWeapons)
                {
                    objWeapon.ParentVehicle = objVehicle;
                    lstWeaponCollection?.Add(objWeapon);
                }

                foreach (Vehicle objLoopVehicle in lstVehicles)
                {
                    lstVehicleCollection?.Add(objLoopVehicle);
                }
            }

            return true;
        }

        public void Upgrade(Grade objGrade, int intRating, decimal refundPercentage, bool blnFree)
        {
            decimal saleCost = TotalCost * refundPercentage;
            int oldRating = Rating;
            decimal oldEssence = CalculatedESS;
            Grade oldGrade = Grade;

            Rating = intRating;
            Grade = objGrade;
            decimal newCost = TotalCost - saleCost;
            if (blnFree)
            {
                newCost = 0;
            }
            if (newCost > _objCharacter.Nuyen)
            {
                Program.MainForm.ShowMessageBox(
                    LanguageManager.GetString("Message_NotEnoughNuyen"),
                    LanguageManager.GetString("MessageTitle_NotEnoughNuyen"),
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                Rating = oldRating;
                Grade = oldGrade;
                return;
            }

            string strSpace = LanguageManager.GetString("String_Space");
            StringBuilder expenseBuilder = new StringBuilder();
            expenseBuilder.Append(LanguageManager.GetString("String_ExpenseUpgradedCyberware") +
                                  strSpace + CurrentDisplayNameShort);
            if (oldGrade != Grade || oldRating != intRating)
            {
                expenseBuilder.Append('(' + LanguageManager.GetString("String_Grade") + strSpace +
                                      Grade.CurrentDisplayName + strSpace + "->" + oldGrade.CurrentDisplayName +
                                      strSpace + LanguageManager.GetString(RatingLabel) +
                                      oldRating + strSpace + "->" + strSpace + Rating.ToString(GlobalOptions.CultureInfo) + ')');
            }

            // Create the Expense Log Entry.
            ExpenseLogEntry objExpense = new ExpenseLogEntry(_objCharacter);
            objExpense.Create(newCost * -1, expenseBuilder.ToString(), ExpenseType.Nuyen, DateTime.Now);
            _objCharacter.ExpenseEntries.AddWithSort(objExpense);
            _objCharacter.Nuyen -= newCost;

            ExpenseUndo objUndo = new ExpenseUndo();
            objUndo.CreateNuyen(NuyenExpenseType.AddGear, InternalId);
            objExpense.Undo = objUndo;
            decimal decEssDelta = oldEssence - CalculatedESS;
            if (decEssDelta > 0)
            {
                //The new Essence cost is greater than the old one.
                _objCharacter.IncreaseEssenceHole((int) (decEssDelta * 100));
            }
            else if (decEssDelta < 0)
            {
                _objCharacter.DecreaseEssenceHole((int)(decEssDelta * 100)*-1);
            }
        }

        /// <summary>
        /// Alias map for SourceDetail control text and tooltip assignation.
        /// </summary>
        /// <param name="sourceControl"></param>
        public void SetSourceDetail(Control sourceControl)
        {
            if (_objCachedSourceDetail?.Language != GlobalOptions.Language)
                _objCachedSourceDetail = null;
            SourceDetail.SetControl(sourceControl);
        }

        public bool AllowPasteXml
        {
            get
            {
                switch (GlobalOptions.ClipboardContentType)
                {
                    case ClipboardContentType.Gear:
                        XmlNode objXmlCategoryNode =
                            GlobalOptions.Clipboard.SelectSingleNode("/character/gear/category");
                        XmlNode objXmlNameNode =
                            GlobalOptions.Clipboard.SelectSingleNode("/character/gear/name");
                        if (AllowGear.ChildNodes.Cast<XmlNode>().Any(objAllowed => (objAllowed.Name == "gearcategory" &&
                                                                                    objAllowed.InnerText ==
                                                                                    objXmlCategoryNode?.InnerText) ||
                                                                                   objAllowed.Name == "gearname" &&
                                                                                   objAllowed.InnerText ==
                                                                                   objXmlNameNode?.InnerText))
                        {
                            return true;
                        }

                        break;
                    case ClipboardContentType.Cyberware:
                        Utils.BreakIfDebug(); //Currently unimplemented.
                        break;
                    default:
                        return false;
                }

                return false;
            }
        }

        public bool AllowPasteObject(object input)
        {
            if (input is Cyberware objCyberware)
            {

                if (!string.IsNullOrEmpty(objCyberware.PlugsIntoModularMount))
                {
                    if (objCyberware.PlugsIntoModularMount != HasModularMount ||
                        Children.Any(x =>
                            x.PlugsIntoModularMount == objCyberware.HasModularMount))
                    {
                        return false;
                    }
                    else
                    {
                        objCyberware.Location = Location;
                    }
                }

                if (objCyberware.SourceType != SourceType) return true;
                {
                    string strAllowedSubsystems = AllowedSubsystems;
                    if (!string.IsNullOrEmpty(strAllowedSubsystems))
                    {
                        return strAllowedSubsystems.Split(',')
                            .All(strSubsystem => objCyberware.Category != strSubsystem);
                    }

                    if (string.IsNullOrEmpty(objCyberware.HasModularMount) &&
                        string.IsNullOrEmpty(objCyberware.BlocksMounts)) return true;
                    HashSet<string> setDisallowedMounts = new HashSet<string>();
                    HashSet<string> setHasMounts = new HashSet<string>();
                    string[] strLoopDisallowedMounts = BlocksMounts.Split(',');
                    foreach (string strLoop in strLoopDisallowedMounts)
                        setDisallowedMounts.Add(strLoop + Location);
                    string strLoopHasModularMount = HasModularMount;
                    if (!string.IsNullOrEmpty(strLoopHasModularMount))
                        setHasMounts.Add(strLoopHasModularMount);
                    foreach (Cyberware objLoopCyberware in Children.DeepWhere(
                        x => x.Children, x => string.IsNullOrEmpty(x.PlugsIntoModularMount)))
                    {
                        strLoopDisallowedMounts = objLoopCyberware.BlocksMounts.Split(',');
                        foreach (string strLoop in strLoopDisallowedMounts)
                            if (!setDisallowedMounts.Contains(strLoop + objLoopCyberware.Location))
                                setDisallowedMounts.Add(strLoop + objLoopCyberware.Location);
                        strLoopHasModularMount = objLoopCyberware.HasModularMount;
                        if (string.IsNullOrEmpty(strLoopHasModularMount)) continue;
                        if (!setHasMounts.Contains(strLoopHasModularMount))
                            setHasMounts.Add(strLoopHasModularMount);
                    }

                    if (!string.IsNullOrEmpty(objCyberware.HasModularMount) &&
                        setDisallowedMounts.Count > 0)
                    {
                        foreach (string strLoop in setDisallowedMounts)
                        {
                            if (strLoop.EndsWith("Right", StringComparison.Ordinal))
                                continue;
                            string strCheck = strLoop;
                            if (strCheck.EndsWith("Left", StringComparison.Ordinal))
                            {
                                strCheck = strCheck.TrimEndOnce("Left", true);
                                if (!setDisallowedMounts.Contains(strCheck + "Right"))
                                    continue;
                            }

                            if (strCheck == objCyberware.HasModularMount)
                            {
                                return false;
                            }
                        }
                    }

                    if (string.IsNullOrEmpty(objCyberware.BlocksMounts)) return true;
                    {
                        if (string.IsNullOrEmpty(objCyberware.Location) && string.IsNullOrEmpty(Location) &&
                            (Children.All(x => x.Location != "Left") || Children.All(x => x.Location != "Right")))
                            return true;
                        string[] astrBlockedMounts = objCyberware.BlocksMounts.Split(',');
                        return astrBlockedMounts.All(strLoop => !setHasMounts.Contains(strLoop));
                    }
                }
            }

            return true;
        }
    }
}
