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
using System.Collections.Specialized;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;
using Chummer.Backend.Attributes;
using NLog;

namespace Chummer.Backend.Equipment
{
    /// <summary>
    /// A specific piece of Armor.
    /// </summary>
    [HubClassTag("SourceID", true, "TotalArmor", "Extra")]
    [DebuggerDisplay("{DisplayName(GlobalSettings.InvariantCultureInfo, GlobalSettings.DefaultLanguage)}")]
    public sealed class Armor : IHasInternalId, IHasName, IHasXmlDataNode, IHasNotes, ICanSell, IHasChildrenAndCost<Gear>, IHasCustomName, IHasLocation, ICanEquip, IHasSource, IHasRating, ICanSort, IHasWirelessBonus, IHasStolenProperty, ICanPaste, IHasGear, IHasMatrixAttributes, ICanBlackMarketDiscount, IDisposable
    {
        private static Logger Log { get; } = LogManager.GetCurrentClassLogger();
        private Guid _guiSourceID = Guid.Empty;
        private Guid _guiID;
        private Guid _guiWeaponID = Guid.Empty;
        private string _strName = string.Empty;
        private string _strCategory = string.Empty;
        private string _strArmorValue = "0";
        private string _strArmorOverrideValue = string.Empty;
        private string _strArmorCapacity = "0";
        private string _strAvail = string.Empty;
        private string _strCost = string.Empty;
        private string _strWeight = string.Empty;
        private int _intRating;
        private int _intMaxRating;
        private string _strSource = string.Empty;
        private string _strPage = string.Empty;
        private string _strArmorName = string.Empty;
        private string _strExtra = string.Empty;
        private string _strRatingLabel = "String_Rating";
        private int _intDamage;
        private bool _blnEquipped = true;
        private readonly Character _objCharacter;
        private readonly TaggedObservableCollection<ArmorMod> _lstArmorMods = new TaggedObservableCollection<ArmorMod>();
        private readonly TaggedObservableCollection<Gear> _lstGear = new TaggedObservableCollection<Gear>();
        private string _strNotes = string.Empty;
        private Color _colNotes = ColorManager.HasNotesColor;
        private Location _objLocation;
        private XmlNode _nodBonus;
        private XmlNode _nodWirelessBonus;
        private bool _blnDiscountCost;
        private int _intSortOrder;
        private bool _blnStolen;
        private bool _blnEncumbrance = true;

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
        private string _strCanFormPersona = string.Empty;
        private bool _blnCanSwapAttributes;
        private bool _blnWirelessOn = true;
        private int _intMatrixCMBonus;
        private int _intMatrixCMFilled;

        #region Constructor, Create, Save, Load, and Print Methods

        public Armor(Character objCharacter)
        {
            // Create the GUID for the new piece of Armor.
            _guiID = Guid.NewGuid();
            _objCharacter = objCharacter;

            _lstArmorMods.AddTaggedCollectionChanged(this, ArmorModsOnCollectionChanged);
            _lstGear.AddTaggedCollectionChanged(this, GearOnCollectionChanged);
        }

        private void ArmorModsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Move)
                return;
            bool blnDoEncumbranceRefresh = false;
            bool blnDoArmorEncumbranceRefresh = false;
            List<ArmorMod> lstImprovementSourcesToProcess = new List<ArmorMod>(e.NewItems?.Count ?? 0);
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    // ReSharper disable once PossibleNullReferenceException
                    foreach (ArmorMod objNewItem in e.NewItems)
                    {
                        objNewItem.Parent = this;
                        if (objNewItem.Equipped)
                        {
                            blnDoEncumbranceRefresh = Equipped;
                            blnDoArmorEncumbranceRefresh = Equipped && objNewItem.Encumbrance;
                            lstImprovementSourcesToProcess.Add(objNewItem);
                        }
                    }

                    break;

                case NotifyCollectionChangedAction.Remove:
                    foreach (ArmorMod objOldItem in e.OldItems)
                    {
                        objOldItem.Parent = null;
                        if (objOldItem.Equipped)
                        {
                            blnDoEncumbranceRefresh = Equipped;
                            blnDoArmorEncumbranceRefresh = Equipped && objOldItem.Encumbrance;
                        }
                    }

                    break;

                case NotifyCollectionChangedAction.Replace:
                    // ReSharper disable once AssignNullToNotNullAttribute
                    HashSet<ArmorMod> setNewItems = e.NewItems.OfType<ArmorMod>().ToHashSet();
                    foreach (ArmorMod objOldItem in e.OldItems)
                    {
                        if (setNewItems.Contains(objOldItem))
                            continue;
                        objOldItem.Parent = null;
                        if (objOldItem.Equipped)
                        {
                            blnDoEncumbranceRefresh = Equipped;
                            blnDoArmorEncumbranceRefresh = Equipped && objOldItem.Encumbrance;
                        }
                    }
                    
                    foreach (ArmorMod objNewItem in setNewItems)
                    {
                        objNewItem.Parent = this;
                        if (objNewItem.Equipped)
                        {
                            blnDoEncumbranceRefresh = Equipped;
                            blnDoArmorEncumbranceRefresh = Equipped && objNewItem.Encumbrance;
                            lstImprovementSourcesToProcess.Add(objNewItem);
                        }
                    }

                    break;

                case NotifyCollectionChangedAction.Reset:
                    blnDoArmorEncumbranceRefresh = Equipped;
                    break;
            }

            using (new FetchSafelyFromPool<Dictionary<INotifyMultiplePropertyChanged, HashSet<string>>>(
                       Utils.DictionaryForMultiplePropertyChangedPool,
                       out Dictionary<INotifyMultiplePropertyChanged, HashSet<string>> dicChangedProperties))
            {
                try
                {
                    if (_objCharacter != null)
                    {
                        if (blnDoEncumbranceRefresh)
                        {
                            if (!dicChangedProperties.TryGetValue(_objCharacter,
                                                                  out HashSet<string> setChangedProperties))
                            {
                                setChangedProperties = Utils.StringHashSetPool.Get();
                                dicChangedProperties.Add(_objCharacter, setChangedProperties);
                            }

                            setChangedProperties.Add(nameof(Character.TotalCarriedWeight));
                        }
                        if (blnDoArmorEncumbranceRefresh)
                        {
                            if (!dicChangedProperties.TryGetValue(_objCharacter,
                                                                  out HashSet<string> setChangedProperties))
                            {
                                setChangedProperties = Utils.StringHashSetPool.Get();
                                dicChangedProperties.Add(_objCharacter, setChangedProperties);
                            }

                            setChangedProperties.Add(nameof(Character.ArmorEncumbrance));
                        }
                    }

                    if (lstImprovementSourcesToProcess.Count > 0 && _objCharacter?.IsLoading == false)
                    {
                        foreach (ArmorMod objItem in lstImprovementSourcesToProcess)
                        {
                            // Needed in order to properly process named sources where
                            // the tooltip was built before the object was added to the character
                            foreach (Improvement objImprovement in _objCharacter.Improvements)
                            {
                                if (objImprovement.SourceName.TrimEndOnce("Wireless")
                                    == objItem.InternalId
                                    && objImprovement.Enabled)
                                {
                                    foreach ((INotifyMultiplePropertyChanged objItemToUpdate,
                                                 string strPropertyToUpdate) in objImprovement
                                                 .GetRelevantPropertyChangers())
                                    {
                                        if (dicChangedProperties.TryGetValue(
                                                objItemToUpdate, out HashSet<string> setChangedProperties))
                                            setChangedProperties.Add(strPropertyToUpdate);
                                        else
                                        {
                                            HashSet<string> setTemp = Utils.StringHashSetPool.Get();
                                            setTemp.Add(strPropertyToUpdate);
                                            dicChangedProperties.Add(objItemToUpdate, setTemp);
                                        }
                                    }
                                }
                            }
                        }
                    }

                    foreach (KeyValuePair<INotifyMultiplePropertyChanged, HashSet<string>> kvpToProcess in dicChangedProperties)
                    {
                        kvpToProcess.Key.OnMultiplePropertyChanged(kvpToProcess.Value.ToList());
                    }
                }
                finally
                {
                    foreach (HashSet<string> setToReturn in dicChangedProperties.Values)
                        Utils.StringHashSetPool.Return(setToReturn);
                }
            }
        }

        private void GearOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (Gear objNewItem in e.NewItems)
                    {
                        objNewItem.Parent = this;
                        objNewItem.ChangeEquippedStatus(Equipped);
                    }
                    break;

                case NotifyCollectionChangedAction.Remove:
                    foreach (Gear objOldItem in e.OldItems)
                    {
                        objOldItem.Parent = null;
                        objOldItem.ChangeEquippedStatus(false);
                    }
                    break;

                case NotifyCollectionChangedAction.Replace:
                    foreach (Gear objOldItem in e.OldItems)
                    {
                        objOldItem.Parent = null;
                        objOldItem.ChangeEquippedStatus(false);
                    }
                    foreach (Gear objNewItem in e.NewItems)
                    {
                        objNewItem.Parent = this;
                        objNewItem.ChangeEquippedStatus(Equipped);
                    }
                    break;
            }
        }

        /// Create an Armor from an XmlNode.
        /// <param name="objXmlArmorNode">XmlNode to create the object from.</param>
        /// <param name="blnSkipCost">Whether or not creating the Armor should skip the Variable price dialogue (should only be used by SelectArmor form).</param>
        /// <param name="blnCreateChildren">Whether or not child items should be created.</param>
        /// <param name="intRating">Rating of the item.</param>
        /// <param name="lstWeapons">List of Weapons that added to the character's weapons.</param>
        /// <param name="blnSkipSelectForms">Whether or not to skip forms that are created for bonuses like Custom Fit (Stack).</param>
        public void Create(XmlNode objXmlArmorNode, int intRating, IList<Weapon> lstWeapons, bool blnSkipCost = false, bool blnCreateChildren = true, bool blnSkipSelectForms = false)
        {
            if (!objXmlArmorNode.TryGetField("id", Guid.TryParse, out _guiSourceID))
            {
                Log.Warn(new object[] { "Missing id field for armor xmlnode", objXmlArmorNode });
                Utils.BreakIfDebug();
            }
            else
            {
                _objCachedMyXmlNode = null;
                _objCachedMyXPathNode = null;
            }

            objXmlArmorNode.TryGetStringFieldQuickly("name", ref _strName);
            objXmlArmorNode.TryGetStringFieldQuickly("category", ref _strCategory);
            objXmlArmorNode.TryGetStringFieldQuickly("armor", ref _strArmorValue);
            if (objXmlArmorNode.TryGetStringFieldQuickly("armoroverride", ref _strArmorOverrideValue) && _strArmorOverrideValue == "0")
                _strArmorOverrideValue = string.Empty;
            _intRating = intRating;
            objXmlArmorNode.TryGetInt32FieldQuickly("rating", ref _intMaxRating);
            objXmlArmorNode.TryGetStringFieldQuickly("ratinglabel", ref _strRatingLabel);
            objXmlArmorNode.TryGetStringFieldQuickly("armorcapacity", ref _strArmorCapacity);
            objXmlArmorNode.TryGetStringFieldQuickly("avail", ref _strAvail);
            objXmlArmorNode.TryGetStringFieldQuickly("source", ref _strSource);
            objXmlArmorNode.TryGetStringFieldQuickly("page", ref _strPage);
            if (!objXmlArmorNode.TryGetMultiLineStringFieldQuickly("altnotes", ref _strNotes))
                objXmlArmorNode.TryGetMultiLineStringFieldQuickly("notes", ref _strNotes);

            string sNotesColor = ColorTranslator.ToHtml(ColorManager.HasNotesColor);
            objXmlArmorNode.TryGetStringFieldQuickly("notesColor", ref sNotesColor);
            _colNotes = ColorTranslator.FromHtml(sNotesColor);

            if (string.IsNullOrEmpty(Notes))
            {
                Notes = CommonFunctions.GetBookNotes(objXmlArmorNode, Name, CurrentDisplayName, Source, Page,
                    DisplayPage(GlobalSettings.Language), _objCharacter);
            }

            objXmlArmorNode.TryGetBoolFieldQuickly("encumbrance", ref _blnEncumbrance);
            _nodBonus = objXmlArmorNode["bonus"];
            _nodWirelessBonus = objXmlArmorNode["wirelessbonus"];
            _blnWirelessOn = false;

            objXmlArmorNode.TryGetStringFieldQuickly("cost", ref _strCost);
            objXmlArmorNode.TryGetStringFieldQuickly("weight", ref _strWeight);

            // Check for a Variable Cost.
            if (!blnSkipCost && _strCost.StartsWith("Variable(", StringComparison.Ordinal))
            {
                string strFirstHalf = _strCost.TrimStartOnce("Variable(", true).TrimEndOnce(')');
                string strSecondHalf = string.Empty;
                int intHyphenIndex = strFirstHalf.IndexOf('-');
                if (intHyphenIndex != -1)
                {
                    if (intHyphenIndex + 1 < strFirstHalf.Length)
                        strSecondHalf = strFirstHalf.Substring(intHyphenIndex + 1);
                    strFirstHalf = strFirstHalf.Substring(0, intHyphenIndex);
                }

                if (!blnSkipSelectForms)
                {
                    decimal decMin;
                    decimal decMax = decimal.MaxValue;
                    if (intHyphenIndex != -1)
                    {
                        decMin = Convert.ToDecimal(strFirstHalf, GlobalSettings.InvariantCultureInfo);
                        decMax = Convert.ToDecimal(strSecondHalf, GlobalSettings.InvariantCultureInfo);
                    }
                    else
                        decMin = Convert.ToDecimal(strFirstHalf.FastEscape('+'), GlobalSettings.InvariantCultureInfo);

                    if (decMin != decimal.MinValue || decMax != decimal.MaxValue)
                    {
                        if (decMax > 1000000)
                            decMax = 1000000;

                        Form frmToUse = Program.GetFormForDialog(_objCharacter);

                        DialogResult eResult = frmToUse.DoThreadSafeFunc(() =>
                        {
                            using (SelectNumber frmPickNumber
                                   = new SelectNumber(_objCharacter.Settings.MaxNuyenDecimals)
                                   {
                                       Minimum = decMin,
                                       Maximum = decMax,
                                       Description = string.Format(
                                           GlobalSettings.CultureInfo,
                                           LanguageManager.GetString("String_SelectVariableCost"),
                                           DisplayNameShort(GlobalSettings.Language)),
                                       AllowCancel = false
                                   })
                            {
                                if (frmPickNumber.ShowDialogSafe(frmToUse) != DialogResult.Cancel)
                                    _strCost = frmPickNumber.SelectedValue.ToString(GlobalSettings.InvariantCultureInfo);
                                return frmPickNumber.DialogResult;
                            }
                        });
                        if (eResult == DialogResult.Cancel)
                        {
                            _guiID = Guid.Empty;
                            return;
                        }
                    }
                    else
                        _strCost = strFirstHalf;
                }
                else
                    _strCost = strFirstHalf;
            }

            if (!blnSkipSelectForms)
            {
                if (Bonus != null)
                {
                    if (!ImprovementManager.CreateImprovements(_objCharacter, Improvement.ImprovementSource.Armor, _guiID.ToString("D", GlobalSettings.InvariantCultureInfo), Bonus, 1, DisplayNameShort(GlobalSettings.Language)))
                    {
                        _guiID = Guid.Empty;
                        return;
                    }

                    if (!string.IsNullOrEmpty(ImprovementManager.SelectedValue))
                    {
                        _strExtra = ImprovementManager.SelectedValue;
                    }
                }

                XmlNode xmlSelectModesFromCategory = objXmlArmorNode["selectmodsfromcategory"];
                if (xmlSelectModesFromCategory != null)
                {
                    XmlDocument objXmlDocument = _objCharacter.LoadData("armor.xml");

                    // More than one Weapon can be added, so loop through all occurrences.
                    foreach (XmlNode objXmlCategoryNode in xmlSelectModesFromCategory)
                    {
                        Form frmToUse = Program.GetFormForDialog(_objCharacter);

                        DialogResult eResult = frmToUse.DoThreadSafeFunc(() =>
                        {
                            using (SelectArmorMod frmPickArmorMod = new SelectArmorMod(_objCharacter, this)
                                   {
                                       AllowedCategories = objXmlCategoryNode.InnerText,
                                       ExcludeGeneralCategory = true
                                   })
                            {
                                frmPickArmorMod.ShowDialogSafe(_objCharacter);

                                if (frmPickArmorMod.DialogResult == DialogResult.Cancel)
                                    return DialogResult.Cancel;

                                // Locate the selected piece.
                                XmlNode objXmlMod = objXmlDocument.SelectSingleNode(
                                    "/chummer/mods/mod[id = " + frmPickArmorMod.SelectedArmorMod.CleanXPath() + ']');

                                if (objXmlMod != null)
                                {
                                    ArmorMod objMod = new ArmorMod(_objCharacter);

                                    // ReSharper disable once AccessToModifiedClosure
                                    objMod.Create(objXmlMod, intRating, lstWeapons, blnSkipCost);
                                    objMod.IncludedInArmor = true;
                                    objMod.ArmorCapacity = "[0]";
                                    objMod.Cost = "0";
                                    objMod.MaximumRating = objMod.Rating;
                                    _lstArmorMods.Add(objMod);
                                }
                                else
                                {
                                    ArmorMod objMod = new ArmorMod(_objCharacter)
                                    {
                                        Name = _strName,
                                        Category = "Features",
                                        Avail = "0",
                                        Source = _strSource,
                                        Page = _strPage,
                                        IncludedInArmor = true,
                                        ArmorCapacity = "[0]",
                                        Cost = "0",
                                        Rating = 0,
                                        MaximumRating = 0
                                    };
                                    _lstArmorMods.Add(objMod);
                                }

                                return frmPickArmorMod.DialogResult;
                            }
                        });
                        if (eResult == DialogResult.Cancel)
                        {
                            _guiID = Guid.Empty;
                            return;
                        }
                    }
                }
            }

            // Add any Armor Mods that come with the Armor.
            if (objXmlArmorNode["mods"] != null && blnCreateChildren)
            {
                XmlNodeList xmlArmorList = objXmlArmorNode.SelectNodes("mods/name");
                if (xmlArmorList != null)
                {
                    XmlDocument objXmlArmorDocument = _objCharacter.LoadData("armor.xml");
                    foreach (XmlNode objXmlArmorMod in xmlArmorList)
                    {
                        XmlAttributeCollection objXmlAttributes = objXmlArmorMod.Attributes;
                        string strForceValue = string.Empty;
                        if (objXmlAttributes != null)
                        {
                            int.TryParse(objXmlAttributes["rating"]?.InnerText, NumberStyles.Any,
                                GlobalSettings.InvariantCultureInfo, out intRating);
                            strForceValue = objXmlAttributes["select"]?.InnerText ?? string.Empty;
                        }

                        XmlNode objXmlMod = objXmlArmorDocument.SelectSingleNode("/chummer/mods/mod[name = " + objXmlArmorMod.InnerText.CleanXPath() + ']');
                        if (objXmlMod != null)
                        {
                            ArmorMod objMod = new ArmorMod(_objCharacter);

                            objMod.Create(objXmlMod, intRating, lstWeapons, blnSkipCost, blnSkipSelectForms);
                            if (string.IsNullOrWhiteSpace(objMod.Extra))
                            {
                                objMod.Extra = strForceValue;
                            }

                            objMod.IncludedInArmor = true;
                            objMod.ArmorCapacity = "[0]";
                            objMod.Cost = "0";
                            string strMaxRating = objXmlAttributes?["maxrating"]?.InnerText;
                            //If maxrating is being specified, we're intentionally bypassing the normal maximum rating. Set the maxrating first, then the rating again.
                            if (!string.IsNullOrEmpty(strMaxRating))
                            {
                                int.TryParse(strMaxRating, NumberStyles.Any, GlobalSettings.InvariantCultureInfo,
                                    out int intDummy);
                                objMod.MaximumRating = intDummy;
                                int.TryParse(objXmlAttributes["rating"]?.InnerText, NumberStyles.Any, GlobalSettings.InvariantCultureInfo,
                                    out intDummy);
                                objMod.Rating = intDummy;
                            }
                            else
                            {
                                objMod.MaximumRating = objMod.Rating;
                            }

                            _lstArmorMods.Add(objMod);
                        }
                        else
                        {
                            int intLoopRating = 0;
                            int intLoopMaximumRating = 0;
                            if (objXmlAttributes != null)
                            {
                                int.TryParse(objXmlAttributes["rating"]?.InnerText, NumberStyles.Any,
                                    GlobalSettings.InvariantCultureInfo, out intLoopRating);
                                int.TryParse(objXmlAttributes["maxrating"]?.InnerText, NumberStyles.Any,
                                    GlobalSettings.InvariantCultureInfo, out intLoopMaximumRating);
                            }
                            ArmorMod objMod = new ArmorMod(_objCharacter)
                            {
                                Name = _strName,
                                Category = "Features",
                                Avail = "0",
                                Source = _strSource,
                                Page = _strPage,
                                IncludedInArmor = true,
                                ArmorCapacity = "[0]",
                                Cost = "0",
                                Rating = intLoopRating,
                                MaximumRating = intLoopMaximumRating,
                                Extra = strForceValue
                            };
                            _lstArmorMods.Add(objMod);
                        }
                    }
                }
            }

            // Add any Gear that comes with the Armor.
            if (objXmlArmorNode["gears"] != null && blnCreateChildren)
            {
                XmlDocument objXmlGearDocument = _objCharacter.LoadData("gear.xml");

                XmlNodeList objXmlGearList = objXmlArmorNode["gears"].SelectNodes("usegear");
                List<Weapon> lstChildWeapons = new List<Weapon>(1);
                foreach (XmlNode objXmlArmorGear in objXmlGearList)
                {
                    Gear objGear = new Gear(_objCharacter);
                    if (!objGear.CreateFromNode(objXmlGearDocument, objXmlArmorGear, lstChildWeapons, !blnSkipSelectForms))
                        continue;
                    foreach (Weapon objWeapon in lstChildWeapons)
                    {
                        objWeapon.ParentID = InternalId;
                    }
                    objGear.Parent = this;
                    objGear.ParentID = InternalId;
                    GearChildren.Add(objGear);
                }
                lstWeapons?.AddRange(lstChildWeapons);
            }

            objXmlArmorNode.TryGetStringFieldQuickly("devicerating", ref _strDeviceRating);
            if (!objXmlArmorNode.TryGetStringFieldQuickly("attributearray", ref _strAttributeArray))
            {
                objXmlArmorNode.TryGetStringFieldQuickly("attack", ref _strAttack);
                objXmlArmorNode.TryGetStringFieldQuickly("sleaze", ref _strSleaze);
                objXmlArmorNode.TryGetStringFieldQuickly("dataprocessing", ref _strDataProcessing);
                objXmlArmorNode.TryGetStringFieldQuickly("firewall", ref _strFirewall);
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
            objXmlArmorNode.TryGetStringFieldQuickly("modattack", ref _strModAttack);
            objXmlArmorNode.TryGetStringFieldQuickly("modsleaze", ref _strModSleaze);
            objXmlArmorNode.TryGetStringFieldQuickly("moddataprocessing", ref _strModDataProcessing);
            objXmlArmorNode.TryGetStringFieldQuickly("modfirewall", ref _strModFirewall);
            objXmlArmorNode.TryGetStringFieldQuickly("modattributearray", ref _strModAttributeArray);

            objXmlArmorNode.TryGetStringFieldQuickly("programs", ref _strProgramLimit);
            objXmlArmorNode.TryGetStringFieldQuickly("canformpersona", ref _strCanFormPersona);
            objXmlArmorNode.TryGetInt32FieldQuickly("matrixcmbonus", ref _intMatrixCMBonus);

            if (!blnSkipSelectForms)
                RefreshWirelessBonuses();

            XmlDocument objXmlWeaponDocument = _objCharacter.LoadData("weapons.xml");

            // More than one Weapon can be added, so loop through all occurrences.
            foreach (XmlNode objXmlAddWeapon in objXmlArmorNode.SelectNodes("addweapon"))
            {
                string strLoopID = objXmlAddWeapon.InnerText;
                XmlNode objXmlWeapon = strLoopID.IsGuid()
                    ? objXmlWeaponDocument.SelectSingleNode("/chummer/weapons/weapon[id = " + strLoopID.CleanXPath() + ']')
                    : objXmlWeaponDocument.SelectSingleNode("/chummer/weapons/weapon[name = " + strLoopID.CleanXPath() + ']');

                int intAddWeaponRating = 0;
                string strLoopRating = objXmlAddWeapon.SelectSingleNode("@rating")?.Value;
                if (!string.IsNullOrEmpty(strLoopRating))
                {
                    strLoopRating = strLoopRating.CheapReplace("{Rating}",
                        () => Rating.ToString(GlobalSettings.InvariantCultureInfo));
                    int.TryParse(strLoopRating, NumberStyles.Any, GlobalSettings.InvariantCultureInfo, out intAddWeaponRating);
                }
                Weapon objGearWeapon = new Weapon(_objCharacter);
                objGearWeapon.Create(objXmlWeapon, lstWeapons, true, !blnSkipSelectForms, blnSkipCost, intAddWeaponRating);
                objGearWeapon.ParentID = InternalId;
                objGearWeapon.Cost = "0";
                if (Guid.TryParse(objGearWeapon.InternalId, out _guiWeaponID))
                    lstWeapons.Add(objGearWeapon);
                else
                    _guiWeaponID = Guid.Empty;
            }
        }

        /// <summary>
        /// Save the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        public void Save(XmlWriter objWriter)
        {
            if (objWriter == null)
                return;
            objWriter.WriteStartElement("armor");
            objWriter.WriteElementString("sourceid", SourceIDString);
            objWriter.WriteElementString("guid", InternalId);
            objWriter.WriteElementString("name", _strName);
            objWriter.WriteElementString("category", _strCategory);
            objWriter.WriteElementString("armor", _strArmorValue);
            objWriter.WriteElementString("armoroverride", _strArmorOverrideValue);
            objWriter.WriteElementString("armorcapacity", _strArmorCapacity);
            objWriter.WriteElementString("avail", _strAvail);
            objWriter.WriteElementString("cost", _strCost);
            objWriter.WriteElementString("weight", _strWeight);
            objWriter.WriteElementString("source", _strSource);
            objWriter.WriteElementString("page", _strPage);
            objWriter.WriteElementString("armorname", _strArmorName);
            objWriter.WriteElementString("equipped", _blnEquipped.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("active", this.IsActiveCommlink(_objCharacter).ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("homenode", this.IsHomeNode(_objCharacter).ToString(GlobalSettings.InvariantCultureInfo));
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
            objWriter.WriteElementString("canswapattributes", _blnCanSwapAttributes.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("matrixcmfilled", _intMatrixCMFilled.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("matrixcmbonus",
                _intMatrixCMBonus.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("wirelesson", _blnWirelessOn.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("canformpersona", _strCanFormPersona);
            objWriter.WriteElementString("extra", _strExtra);
            objWriter.WriteElementString("damage", _intDamage.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("rating", _intRating.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("maxrating", _intMaxRating.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("ratinglabel", _strRatingLabel);
            objWriter.WriteElementString("stolen", _blnStolen.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("emcumbrance", _blnEncumbrance.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteStartElement("armormods");
            foreach (ArmorMod objMod in _lstArmorMods)
            {
                objMod.Save(objWriter);
            }
            objWriter.WriteEndElement();
            if (_lstGear.Count > 0)
            {
                objWriter.WriteStartElement("gears");
                foreach (Gear objGear in _lstGear)
                {
                    objGear.Save(objWriter);
                }
                objWriter.WriteEndElement();
            }
            if (_nodBonus != null)
                objWriter.WriteRaw(_nodBonus.OuterXml);
            else
                objWriter.WriteElementString("bonus", string.Empty);
            if (_nodWirelessBonus != null)
                objWriter.WriteRaw(_nodWirelessBonus.OuterXml);
            else
                objWriter.WriteElementString("wirelessbonus", string.Empty);
            objWriter.WriteElementString("location", Location?.InternalId ?? string.Empty);
            objWriter.WriteElementString("notes", System.Text.RegularExpressions.Regex.Replace(_strNotes, @"[\u0000-\u0008\u000B\u000C\u000E-\u001F]", ""));
            objWriter.WriteElementString("notesColor", ColorTranslator.ToHtml(_colNotes));
            objWriter.WriteElementString("discountedcost", _blnDiscountCost.ToString(GlobalSettings.InvariantCultureInfo));
            if (_guiWeaponID != Guid.Empty)
                objWriter.WriteElementString("weaponguid", _guiWeaponID.ToString("D", GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("sortorder", _intSortOrder.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteEndElement();
        }

        /// <summary>
        /// Load the CharacterAttribute from the XmlNode.
        /// </summary>
        /// <param name="objNode">XmlNode to load.</param>
        /// <param name="blnCopy">Check if we are copying an existing item.</param>
        public void Load(XmlNode objNode, bool blnCopy = false)
        {
            if (objNode == null)
                return;
            objNode.TryGetStringFieldQuickly("name", ref _strName);
            _objCachedMyXmlNode = null;
            _objCachedMyXPathNode = null;
            Lazy<XPathNavigator> objMyNode = new Lazy<XPathNavigator>(this.GetNodeXPath);
            if (blnCopy)
            {
                _guiID = Guid.NewGuid();
                _objLocation = null;
            }
            else
            {
                if (!objNode.TryGetField("guid", Guid.TryParse, out _guiID))
                {
                    _guiID = Guid.NewGuid();
                }
                string strLocation = objNode["location"]?.InnerText;
                if (!string.IsNullOrEmpty(strLocation))
                {
                    if (Guid.TryParse(strLocation, out Guid temp))
                    {
                        string strLocationId = temp.ToString();
                        // Location is an object. Look for it based on the InternalId. Requires that locations have been loaded already!
                        Location =
                            _objCharacter.ArmorLocations.FirstOrDefault(location =>
                                location.InternalId == strLocationId);
                    }
                    else
                    {
                        //Legacy. Location is a string.
                        Location =
                            _objCharacter.ArmorLocations.FirstOrDefault(location =>
                                location.Name == strLocation);
                    }
                    _objLocation?.Children.Add(this);
                }
            }

            if (!objNode.TryGetGuidFieldQuickly("sourceid", ref _guiSourceID))
            {
                objMyNode.Value?.TryGetGuidFieldQuickly("id", ref _guiSourceID);
            }

            if (!objNode.TryGetBoolFieldQuickly("encumbrance", ref _blnEncumbrance))
                _blnEncumbrance = true;

            objNode.TryGetStringFieldQuickly("category", ref _strCategory);
            objNode.TryGetStringFieldQuickly("armor", ref _strArmorValue);
            objNode.TryGetStringFieldQuickly("avail", ref _strAvail);
            objNode.TryGetStringFieldQuickly("cost", ref _strCost);
            if (!objNode.TryGetStringFieldQuickly("weight", ref _strWeight))
                objMyNode.Value?.TryGetStringFieldQuickly("weight", ref _strWeight);
            objNode.TryGetStringFieldQuickly("source", ref _strSource);
            if (objNode.TryGetStringFieldQuickly("armoroverride", ref _strArmorOverrideValue) && _strArmorOverrideValue == "0")
                _strArmorOverrideValue = string.Empty;
            objNode.TryGetStringFieldQuickly("armorcapacity", ref _strArmorCapacity);
            objNode.TryGetInt32FieldQuickly("rating", ref _intRating);
            objNode.TryGetInt32FieldQuickly("maxrating", ref _intMaxRating);
            objNode.TryGetStringFieldQuickly("ratinglabel", ref _strRatingLabel);
            objNode.TryGetStringFieldQuickly("page", ref _strPage);
            objNode.TryGetStringFieldQuickly("armorname", ref _strArmorName);
            objNode.TryGetBoolFieldQuickly("equipped", ref _blnEquipped);
            objNode.TryGetStringFieldQuickly("extra", ref _strExtra);
            objNode.TryGetInt32FieldQuickly("damage", ref _intDamage);
            objNode.TryGetMultiLineStringFieldQuickly("notes", ref _strNotes);

            string sNotesColor = ColorTranslator.ToHtml(ColorManager.HasNotesColor);
            objNode.TryGetStringFieldQuickly("notesColor", ref sNotesColor);
            _colNotes = ColorTranslator.FromHtml(sNotesColor);

            objNode.TryGetBoolFieldQuickly("discountedcost", ref _blnDiscountCost);
            objNode.TryGetBoolFieldQuickly("stolen", ref _blnStolen);
            objNode.TryGetInt32FieldQuickly("sortorder", ref _intSortOrder);
            _nodBonus = objNode["bonus"];
            _nodWirelessBonus = objNode["wirelessbonus"];
            if (!objNode.TryGetBoolFieldQuickly("wirelesson", ref _blnWirelessOn))
                _blnWirelessOn = false;
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
                objMyNode.Value?.TryGetStringFieldQuickly("devicerating", ref _strDeviceRating);
            if (!objNode.TryGetStringFieldQuickly("programlimit", ref _strProgramLimit))
                objMyNode.Value?.TryGetStringFieldQuickly("programs", ref _strProgramLimit);
            objNode.TryGetStringFieldQuickly("overclocked", ref _strOverclocked);
            if (!objNode.TryGetStringFieldQuickly("attack", ref _strAttack))
                objMyNode.Value?.TryGetStringFieldQuickly("attack", ref _strAttack);
            if (!objNode.TryGetStringFieldQuickly("sleaze", ref _strSleaze))
                objMyNode.Value?.TryGetStringFieldQuickly("sleaze", ref _strSleaze);
            if (!objNode.TryGetStringFieldQuickly("dataprocessing", ref _strDataProcessing))
                objMyNode.Value?.TryGetStringFieldQuickly("dataprocessing", ref _strDataProcessing);
            if (!objNode.TryGetStringFieldQuickly("firewall", ref _strFirewall))
                objMyNode.Value?.TryGetStringFieldQuickly("firewall", ref _strFirewall);
            if (!objNode.TryGetStringFieldQuickly("attributearray", ref _strAttributeArray))
                objMyNode.Value?.TryGetStringFieldQuickly("attributearray", ref _strAttributeArray);
            if (!objNode.TryGetStringFieldQuickly("modattack", ref _strModAttack))
                objMyNode.Value?.TryGetStringFieldQuickly("modattack", ref _strModAttack);
            if (!objNode.TryGetStringFieldQuickly("modsleaze", ref _strModSleaze))
                objMyNode.Value?.TryGetStringFieldQuickly("modsleaze", ref _strModSleaze);
            if (!objNode.TryGetStringFieldQuickly("moddataprocessing", ref _strModDataProcessing))
                objMyNode.Value?.TryGetStringFieldQuickly("moddataprocessing", ref _strModDataProcessing);
            if (!objNode.TryGetStringFieldQuickly("modfirewall", ref _strModFirewall))
                objMyNode.Value?.TryGetStringFieldQuickly("modfirewall", ref _strModFirewall);
            if (!objNode.TryGetStringFieldQuickly("modattributearray", ref _strModAttributeArray))
                objMyNode.Value?.TryGetStringFieldQuickly("modattributearray", ref _strModAttributeArray);
            objNode.TryGetInt32FieldQuickly("matrixcmfilled", ref _intMatrixCMFilled);
            objNode.TryGetInt32FieldQuickly("matrixcmbonus", ref _intMatrixCMBonus);
            if (!objNode.TryGetStringFieldQuickly("canformpersona", ref _strCanFormPersona))
                objMyNode.Value?.TryGetStringFieldQuickly("canformpersona", ref _strCanFormPersona);
            XmlNode xmlChildrenNode = objNode["armormods"];
            if (xmlChildrenNode != null)
            {
                using (XmlNodeList nodMods = xmlChildrenNode.SelectNodes("armormod"))
                {
                    if (nodMods != null)
                    {
                        foreach (XmlNode nodMod in nodMods)
                        {
                            ArmorMod objMod = new ArmorMod(_objCharacter);
                            objMod.Load(nodMod, blnCopy);
                            _lstArmorMods.Add(objMod);
                        }
                    }
                }
            }
            xmlChildrenNode = objNode["gears"];
            if (xmlChildrenNode != null)
            {
                using (XmlNodeList nodGears = xmlChildrenNode.SelectNodes("gear"))
                {
                    if (nodGears != null)
                    {
                        foreach (XmlNode nodGear in nodGears)
                        {
                            Gear objGear = new Gear(_objCharacter);
                            objGear.Load(nodGear, blnCopy);
                            _lstGear.Add(objGear);
                        }
                    }
                }
            }

            if (blnCopy)
            {
                if (Bonus != null)
                {
                    if (!string.IsNullOrEmpty(Extra))
                        ImprovementManager.ForcedValue = Extra;
                    ImprovementManager.CreateImprovements(_objCharacter, Improvement.ImprovementSource.Armor, _guiID.ToString("D", GlobalSettings.InvariantCultureInfo), Bonus, 1, DisplayNameShort(GlobalSettings.Language));
                    if (!string.IsNullOrEmpty(ImprovementManager.SelectedValue))
                    {
                        Extra = ImprovementManager.SelectedValue;
                    }
                }

                if (WirelessOn && WirelessBonus != null)
                {
                    ImprovementManager.ForcedValue = Extra;

                    if (!ImprovementManager.CreateImprovements(_objCharacter, Improvement.ImprovementSource.Armor, _guiID.ToString("D", GlobalSettings.InvariantCultureInfo), WirelessBonus, 1, DisplayNameShort(GlobalSettings.Language)))
                    {
                        _guiID = Guid.Empty;
                        return;
                    }
                }

                if (!_blnEquipped)
                {
                    _blnEquipped = true;
                    Equipped = false;
                }

                RefreshWirelessBonuses();
            }
        }

        /// <summary>
        /// Print the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        /// <param name="objCulture">Culture in which to print.</param>
        /// <param name="strLanguageToPrint">Language in which to print</param>
        public async ValueTask Print(XmlWriter objWriter, CultureInfo objCulture, string strLanguageToPrint)
        {
            if (objWriter == null)
                return;
            await objWriter.WriteStartElementAsync("armor");
            await objWriter.WriteElementStringAsync("guid", InternalId);
            await objWriter.WriteElementStringAsync("sourceid", SourceIDString);
            await objWriter.WriteElementStringAsync("name", await DisplayNameShortAsync(strLanguageToPrint));
            await objWriter.WriteElementStringAsync("fullname", await DisplayNameAsync(objCulture, strLanguageToPrint));
            await objWriter.WriteElementStringAsync("name_english", Name);
            await objWriter.WriteElementStringAsync("category", DisplayCategory(strLanguageToPrint));
            await objWriter.WriteElementStringAsync("category_english", Category);
            await objWriter.WriteElementStringAsync("armor", DisplayArmorValue);
            await objWriter.WriteElementStringAsync("totalarmorcapacity", TotalArmorCapacity(objCulture));
            await objWriter.WriteElementStringAsync("calculatedcapacity", CalculatedCapacity(objCulture));
            await objWriter.WriteElementStringAsync("capacityremaining", CapacityRemaining.ToString(objCulture));
            await objWriter.WriteElementStringAsync("avail", TotalAvail(objCulture, strLanguageToPrint));
            await objWriter.WriteElementStringAsync("cost", TotalCost.ToString(_objCharacter.Settings.NuyenFormat, objCulture));
            await objWriter.WriteElementStringAsync("owncost", OwnCost.ToString(_objCharacter.Settings.NuyenFormat, objCulture));
            await objWriter.WriteElementStringAsync("weight", TotalWeight.ToString(_objCharacter.Settings.WeightFormat, objCulture));
            await objWriter.WriteElementStringAsync("ownweight", OwnWeight.ToString(_objCharacter.Settings.WeightFormat, objCulture));
            await objWriter.WriteElementStringAsync("source", await _objCharacter.LanguageBookShortAsync(Source, strLanguageToPrint));
            await objWriter.WriteElementStringAsync("page", await DisplayPageAsync(strLanguageToPrint));
            await objWriter.WriteElementStringAsync("armorname", CustomName);
            await objWriter.WriteElementStringAsync("equipped", Equipped.ToString(GlobalSettings.InvariantCultureInfo));
            await objWriter.WriteElementStringAsync("ratinglabel", RatingLabel);
            await objWriter.WriteElementStringAsync("wirelesson", WirelessOn.ToString(GlobalSettings.InvariantCultureInfo));
            await objWriter.WriteStartElementAsync("armormods");
            foreach (ArmorMod objMod in ArmorMods)
            {
                await objMod.Print(objWriter, objCulture, strLanguageToPrint);
            }
            await objWriter.WriteEndElementAsync();
            await objWriter.WriteStartElementAsync("gears");
            foreach (Gear objGear in GearChildren)
            {
                await objGear.Print(objWriter, objCulture, strLanguageToPrint);
            }
            await objWriter.WriteEndElementAsync();
            await objWriter.WriteElementStringAsync("extra", await _objCharacter.TranslateExtraAsync(Extra, strLanguageToPrint));
            await objWriter.WriteElementStringAsync("location", Location?.DisplayNameShort(strLanguageToPrint));
            await objWriter.WriteElementStringAsync("attack", this.GetTotalMatrixAttribute("Attack").ToString(objCulture));
            await objWriter.WriteElementStringAsync("sleaze", this.GetTotalMatrixAttribute("Sleaze").ToString(objCulture));
            await objWriter.WriteElementStringAsync("dataprocessing",
                                                    this.GetTotalMatrixAttribute("Data Processing").ToString(objCulture));
            await objWriter.WriteElementStringAsync("firewall", this.GetTotalMatrixAttribute("Firewall").ToString(objCulture));
            await objWriter.WriteElementStringAsync("devicerating",
                                                    this.GetTotalMatrixAttribute("Device Rating").ToString(objCulture));
            await objWriter.WriteElementStringAsync("programlimit",
                                                    this.GetTotalMatrixAttribute("Program Limit").ToString(objCulture));
            await objWriter.WriteElementStringAsync("iscommlink", IsCommlink.ToString(GlobalSettings.InvariantCultureInfo));
            await objWriter.WriteElementStringAsync("isprogram", IsProgram.ToString(GlobalSettings.InvariantCultureInfo));
            await objWriter.WriteElementStringAsync("active",
                                                    this.IsActiveCommlink(_objCharacter).ToString(GlobalSettings.InvariantCultureInfo));
            await objWriter.WriteElementStringAsync("homenode",
                                                    this.IsHomeNode(_objCharacter).ToString(GlobalSettings.InvariantCultureInfo));
            await objWriter.WriteElementStringAsync("conditionmonitor", MatrixCM.ToString(objCulture));
            await objWriter.WriteElementStringAsync("matrixcmfilled", MatrixCMFilled.ToString(objCulture));
            if (GlobalSettings.PrintNotes)
                await objWriter.WriteElementStringAsync("notes", Notes);
            await objWriter.WriteEndElementAsync();
        }

        #endregion Constructor, Create, Save, Load, and Print Methods

        #region Properties

        /// <summary>
        /// Internal identifier which will be used to identify this piece of Armor in the Improvement system.
        /// </summary>
        public string InternalId => _guiID.ToString("D", GlobalSettings.InvariantCultureInfo);

        /// <summary>
        /// Name of the Armor.
        /// </summary>
        public string Name
        {
            get => _strName;
            set
            {
                if (_strName == value)
                    return;
                _objCachedMyXmlNode = null;
                _objCachedMyXPathNode = null;
                _strName = value;
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
        /// Wireless Bonus node from the XML file.
        /// </summary>
        public XmlNode WirelessBonus
        {
            get => _nodWirelessBonus;
            set => _nodWirelessBonus = value;
        }

        /// <summary>
        /// Value that was selected during an ImprovementManager dialogue.
        /// </summary>
        public string Extra
        {
            get => _strExtra;
            set => _strExtra = _objCharacter.ReverseTranslateExtra(value);
        }

        /// <summary>
        /// Translated Category.
        /// </summary>
        public string DisplayCategory(string strLanguage)
        {
            if (strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                return Category;

            return _objCharacter.LoadDataXPath("armor.xml", strLanguage).SelectSingleNode("/chummer/categories/category[. = " + Category.CleanXPath() + "]/@translate")?.Value ?? Category;
        }

        /// <summary>
        /// Armor's Category.
        /// </summary>
        public string Category
        {
            get => _strCategory;
            set => _strCategory = value;
        }

        /// <summary>
        /// Whether or not the Armor contributes to Encumbrance.
        /// </summary>
        public bool Encumbrance => _blnEncumbrance;

        /// <summary>
        /// Armor's Armor value.
        /// </summary>
        public string ArmorValue
        {
            get => _strArmorValue;
            set => _strArmorValue = value;
        }

        /// <summary>
        /// Armor's Armor Override value.
        /// </summary>
        public string ArmorOverrideValue
        {
            get => _strArmorOverrideValue;
            set => _strArmorOverrideValue = value == "0" ? string.Empty : value;
        }

        /// <summary>
        /// Damage done to the Armor's Armor Rating.
        /// </summary>
        public int ArmorDamage
        {
            get => _intDamage;
            set
            {
                int intOldValue = _intDamage;
                _intDamage = value;

                int.TryParse(ArmorValue, NumberStyles.Any, GlobalSettings.InvariantCultureInfo,
                    out int intTotalArmor);

                // Go through all of the Mods for this piece of Armor and add the Armor value.
                foreach (ArmorMod objMod in ArmorMods)
                {
                    if (objMod.Equipped)
                        intTotalArmor += objMod.Armor;
                }

                if (_intDamage < 0)
                    _intDamage = 0;
                if (_intDamage > intTotalArmor)
                    _intDamage = intTotalArmor;

                if (_intDamage != intOldValue && Equipped && _objCharacter != null)
                {
                    _objCharacter.OnPropertyChanged(nameof(Character.GetArmorRating));
                    _objCharacter.RefreshArmorEncumbrance();
                }
            }
        }

        /// <summary>
        /// Rating.
        /// </summary>
        public int Rating
        {
            get => Math.Min(_intRating, MaxRating);
            set
            {
                int intNewValue = Math.Min(value, MaxRating);
                if (_intRating == intNewValue)
                    return;
                _intRating = intNewValue;
                if (Equipped && _objCharacter != null && (ArmorValue.Contains("Rating") || ArmorOverrideValue.Contains("Rating")))
                {
                    _objCharacter.OnPropertyChanged(nameof(Character.GetArmorRating));
                    _objCharacter.RefreshArmorEncumbrance();
                }
                if (GearChildren.Count > 0)
                {
                    foreach (Gear objChild in GearChildren.Where(x => x.MaxRating.Contains("Parent") || x.MinRating.Contains("Parent")))
                    {
                        // This will update a child's rating if it would become out of bounds due to its parent's rating changing
                        int intCurrentRating = objChild.Rating;
                        objChild.Rating = intCurrentRating;
                    }
                }
            }
        }

        /// <summary>
        /// Rating.
        /// </summary>
        public int MaxRating
        {
            get => _intMaxRating;
            set => _intMaxRating = value;
        }

        /// <summary>
        /// How the rating should be referred to in UI.
        /// </summary>
        public string RatingLabel
        {
            get => _strRatingLabel;
            set => _strRatingLabel = value;
        }

        /// <summary>
        /// Armor's Capacity string.
        /// </summary>
        public string ArmorCapacity
        {
            get => _strArmorCapacity;
            set => _strArmorCapacity = value;
        }

        /// <summary>
        /// Armor's Capacity.
        /// </summary>
        public string TotalArmorCapacity(CultureInfo objCultureInfo)
        {
            string strArmorCapacity = ArmorCapacity;
            if (strArmorCapacity.Contains("Rating"))
            {
                // If the Capacity is determined by the Rating, evaluate the expression.
                // XPathExpression cannot evaluate while there are square brackets, so remove them if necessary.
                bool blnSquareBrackets = strArmorCapacity.StartsWith('[');
                string strCapacity = strArmorCapacity;
                if (blnSquareBrackets)
                    strCapacity = strCapacity.Substring(1, strCapacity.Length - 2);

                object objProcess = CommonFunctions.EvaluateInvariantXPath(strCapacity.Replace("Rating", Rating.ToString(GlobalSettings.InvariantCultureInfo)), out bool blnIsSuccess);
                string strReturn = blnIsSuccess ? ((double)objProcess).ToString("#,0.##", objCultureInfo) : objProcess.ToString();
                if (blnSquareBrackets)
                    strReturn = '[' + strReturn + ']';

                return strReturn;
            }

            return decimal.TryParse(strArmorCapacity, NumberStyles.Any, GlobalSettings.InvariantCultureInfo,
                                    out decimal decReturn)
                ? decReturn.ToString("#,0.##", objCultureInfo)
                : strArmorCapacity;
        }

        public string CurrentTotalArmorCapacity => TotalArmorCapacity(GlobalSettings.CultureInfo);

        /// <summary>
        /// Armor's Availability.
        /// </summary>
        public string Avail
        {
            get => _strAvail;
            set => _strAvail = value;
        }

        /// <summary>
        /// Armor's Cost.
        /// </summary>
        public string Cost
        {
            get => _strCost;
            set => _strCost = value;
        }

        /// <summary>
        /// Armor's Weight.
        /// </summary>
        public string Weight
        {
            get => _strWeight;
            set => _strWeight = value;
        }

        public string DisplayCost(out decimal decItemCost, bool blnUseRating = true, decimal decMarkup = 0.0m)
        {
            decItemCost = 0;
            string strReturn = Cost;
            if (strReturn.StartsWith("Variable(", StringComparison.Ordinal))
            {
                strReturn = strReturn.TrimStartOnce("Variable(", true).TrimEndOnce(')');
                decimal decMin;
                decimal decMax = decimal.MaxValue;
                if (strReturn.Contains('-'))
                {
                    string[] strValues = strReturn.Split('-');
                    decMin = Convert.ToDecimal(strValues[0], GlobalSettings.InvariantCultureInfo);
                    decMax = Convert.ToDecimal(strValues[1], GlobalSettings.InvariantCultureInfo);
                }
                else
                    decMin = Convert.ToDecimal(strReturn.FastEscape('+'), GlobalSettings.InvariantCultureInfo);

                if (decMax == decimal.MaxValue)
                    strReturn = decMin.ToString(_objCharacter.Settings.NuyenFormat, GlobalSettings.CultureInfo) + "¥+";
                else
                    strReturn = decMin.ToString(_objCharacter.Settings.NuyenFormat, GlobalSettings.CultureInfo) + " - " + decMax.ToString(_objCharacter.Settings.NuyenFormat, GlobalSettings.CultureInfo) + '¥';

                decItemCost = decMin;
                return strReturn;
            }

            if (blnUseRating)
            {
                if (strReturn.StartsWith("FixedValues(", StringComparison.Ordinal))
                {
                    string[] strValues = strReturn.TrimStartOnce("FixedValues(", true).TrimEndOnce(')').Split(',', StringSplitOptions.RemoveEmptyEntries);
                    strReturn = strValues[Math.Max(Math.Min(Rating, strValues.Length) - 1, 0)];
                }
                decimal decTotalCost;
                // If the cost is determined by the Rating, evaluate the expression.
                if (strReturn.Contains("Rating"))
                {
                    string strCost = strReturn.Replace("Rating", Rating.ToString(GlobalSettings.InvariantCultureInfo));
                    object objProcess = CommonFunctions.EvaluateInvariantXPath(strCost, out bool blnIsSuccess);
                    decTotalCost = blnIsSuccess ? Convert.ToDecimal(objProcess.ToString(), GlobalSettings.InvariantCultureInfo) : 0;
                }
                else
                {
                    decTotalCost = Convert.ToDecimal(strReturn, GlobalSettings.InvariantCultureInfo);
                }

                decTotalCost *= 1.0m + decMarkup;

                if (DiscountCost)
                    decTotalCost *= 0.9m;

                decItemCost = decTotalCost;

                return decTotalCost.ToString(_objCharacter.Settings.NuyenFormat, GlobalSettings.CultureInfo) + '¥';
            }

            if (strReturn.StartsWith("FixedValues(", StringComparison.Ordinal))
            {
                string[] strValues = strReturn.TrimStartOnce("FixedValues(", true).TrimEndOnce(')').Split(',', StringSplitOptions.RemoveEmptyEntries);
                strReturn = strValues[0] + '-' + strValues[Math.Max(Math.Min(MaxRating, strValues.Length) - 1, 0)];
            }

            return strReturn.CheapReplace("Rating", () => LanguageManager.GetString(RatingLabel)) + '¥';
        }

        private SourceString _objCachedSourceDetail;

        public SourceString SourceDetail
        {
            get
            {
                if (_objCachedSourceDetail == default)
                    _objCachedSourceDetail = new SourceString(Source,
                        DisplayPage(GlobalSettings.Language), GlobalSettings.Language, GlobalSettings.CultureInfo,
                        _objCharacter);
                return _objCachedSourceDetail;
            }
        }

        /// <summary>
        /// Armor's Sourcebook.
        /// </summary>
        public string Source
        {
            get => _strSource;
            set => _strSource = value;
        }

        /// <summary>
        /// Guid of a Weapon created from the Armour.
        /// </summary>
        public string WeaponID
        {
            get => _guiWeaponID.ToString("D", GlobalSettings.InvariantCultureInfo);
            set
            {
                if (Guid.TryParse(value, out Guid guiTemp))
                    _guiWeaponID = guiTemp;
            }
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
            if (strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                return Page;
            string s = this.GetNodeXPath(strLanguage)?.SelectSingleNodeAndCacheExpression("altpage")?.Value ?? Page;
            return !string.IsNullOrWhiteSpace(s) ? s : Page;
        }

        /// <summary>
        /// Sourcebook Page Number using a given language file.
        /// Returns Page if not found or the string is empty.
        /// </summary>
        /// <param name="strLanguage">Language file keyword to use.</param>
        /// <returns></returns>
        public async ValueTask<string> DisplayPageAsync(string strLanguage)
        {
            if (strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                return Page;
            string s = (await this.GetNodeXPathAsync(strLanguage))?.SelectSingleNodeAndCacheExpression("altpage")?.Value ?? Page;
            return !string.IsNullOrWhiteSpace(s) ? s : Page;
        }

        /// <summary>
        /// Whether or not the Armor is equipped and should be considered for highest Armor Rating or Armor Encumbrance.
        /// </summary>
        public bool Equipped
        {
            get => _blnEquipped;
            set
            {
                if (_blnEquipped == value)
                    return;
                _blnEquipped = value;
                if (value)
                {
                    // Add the Armor's Improvements to the character.
                    ImprovementManager.EnableImprovements(_objCharacter, _objCharacter.Improvements.Where(x => x.ImproveSource == Improvement.ImprovementSource.Armor && x.SourceName == InternalId));
                    // Add the Improvements from any Armor Mods in the Armor.
                    foreach (ArmorMod objMod in ArmorMods)
                    {
                        if (objMod.Equipped)
                        {
                            ImprovementManager.EnableImprovements(_objCharacter, _objCharacter.Improvements.Where(x => x.ImproveSource == Improvement.ImprovementSource.ArmorMod && x.SourceName == InternalId));
                            // Add the Improvements from any Gear in the Armor.
                            foreach (Gear objGear in objMod.GearChildren)
                            {
                                if (objGear.Equipped)
                                {
                                    objGear.ChangeEquippedStatus(true, true);
                                }
                            }
                        }
                    }
                    // Add the Improvements from any Gear in the Armor.
                    foreach (Gear objGear in GearChildren)
                    {
                        if (objGear.Equipped)
                        {
                            objGear.ChangeEquippedStatus(true, true);
                        }
                    }
                }
                else
                {
                    // Add the Armor's Improvements to the character.
                    ImprovementManager.DisableImprovements(_objCharacter,
                                                           _objCharacter.Improvements.Where(
                                                               x => x.ImproveSource
                                                                    == Improvement.ImprovementSource.Armor
                                                                    && x.SourceName == InternalId));
                    // Add the Improvements from any Armor Mods in the Armor.
                    foreach (ArmorMod objMod in ArmorMods)
                    {
                        ImprovementManager.DisableImprovements(_objCharacter,
                                                               _objCharacter.Improvements.Where(
                                                                   x => x.ImproveSource
                                                                        == Improvement.ImprovementSource.ArmorMod
                                                                        && x.SourceName == InternalId));
                        // Add the Improvements from any Gear in the Armor.
                        foreach (Gear objGear in objMod.GearChildren)
                        {
                            objGear.ChangeEquippedStatus(false, true);
                        }
                    }
                    // Add the Improvements from any Gear in the Armor.
                    foreach (Gear objGear in GearChildren)
                    {
                        objGear.ChangeEquippedStatus(false, true);
                    }
                }

                _objCharacter?.OnMultiplePropertyChanged(nameof(Character.ArmorEncumbrance), nameof(Character.TotalCarriedWeight), nameof(Character.TotalArmorRating));
            }
        }

        /// <summary>
        /// Whether or not Wireless is turned on for this armor
        /// </summary>
        public bool WirelessOn
        {
            get => _blnWirelessOn;
            set
            {
                if (value == _blnWirelessOn)
                    return;
                _blnWirelessOn = value;
                RefreshWirelessBonuses();
            }
        }

        /// <summary>
        /// The Armor's total Armor value including Modifications.
        /// </summary>
        public int TotalArmor
        {
            get
            {
                int.TryParse(ArmorValue.Replace("Rating", Rating.ToString(GlobalSettings.InvariantCultureInfo)), NumberStyles.Any, GlobalSettings.InvariantCultureInfo, out int intTotalArmor);
                // Go through all of the Mods for this piece of Armor and add the Armor value.
                intTotalArmor += ArmorMods.Where(o => o.Equipped).Sum(o => o.Armor);
                intTotalArmor -= ArmorDamage;

                return Math.Max(intTotalArmor, 0);
            }
        }

        /// <summary>
        /// The Armor's total bonus Armor value including Modifications.
        /// </summary>
        public int TotalOverrideArmor
        {
            get
            {
                int.TryParse(ArmorOverrideValue.Replace("Rating", Rating.ToString(GlobalSettings.InvariantCultureInfo)), NumberStyles.Any, GlobalSettings.InvariantCultureInfo, out int intTotalArmor);
                // Go through all of the Mods for this piece of Armor and add the Armor value.
                intTotalArmor += ArmorMods.Where(o => o.Equipped).Sum(o => o.Armor);
                intTotalArmor -= ArmorDamage;

                return Math.Max(intTotalArmor, 0);
            }
        }

        public string DisplayArmorValue
        {
            get
            {
                string strArmorOverrideValue = ArmorOverrideValue;
                if (!string.IsNullOrWhiteSpace(strArmorOverrideValue))
                {
                    return TotalArmor.ToString(GlobalSettings.CultureInfo) + '/' + strArmorOverrideValue;
                }

                string strArmor = ArmorValue;
                char chrFirstArmorChar = strArmor.Length > 0 ? strArmor[0] : ' ';
                if (chrFirstArmorChar == '+' || chrFirstArmorChar == '-')
                {
                    return TotalArmor.ToString("+0;-0;0", GlobalSettings.CultureInfo);
                }
                return TotalArmor.ToString(GlobalSettings.CultureInfo);
            }
        }

        /// <summary>
        /// The Armor's total Cost including Modifications.
        /// </summary>
        public decimal StolenTotalCost
        {
            get
            {
                decimal decTotalCost = 0;
                if (Stolen) decTotalCost += OwnCost;

                // Go through all of the Mods for this piece of Armor and add the Cost value.
                decTotalCost += ArmorMods.Where(mod => mod.Stolen).Sum(mod => mod.StolenTotalCost);

                // Go through all of the Gear for this piece of Armor and add the Cost value.
                decTotalCost += GearChildren.Where(g => g.Stolen).Sum(g => g.StolenTotalCost);

                return decTotalCost;
            }
        }

        /// <summary>
        /// The Armor's total Cost including Modifications.
        /// </summary>
        public decimal TotalCost
        {
            get
            {
                decimal decTotalCost = OwnCost;

                // Go through all of the Mods for this piece of Armor and add the Cost value.
                foreach (ArmorMod objMod in ArmorMods)
                    decTotalCost += objMod.TotalCost;

                // Go through all of the Gear for this piece of Armor and add the Cost value.
                foreach (Gear objGear in GearChildren)
                    decTotalCost += objGear.TotalCost;

                return decTotalCost;
            }
        }

        /// <summary>
        /// Cost multiplier for Gear Children attached to this Armor.
        /// </summary>
        public int ChildCostMultiplier => 1;

        /// <summary>
        /// Cost for just the Armor.
        /// </summary>
        public decimal OwnCost
        {
            get
            {
                // If the cost is determined by the Rating, evaluate the expression.
                string strCostExpression = Cost;

                decimal decReturn = 0;
                using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdCost))
                {
                    sbdCost.Append(strCostExpression.TrimStart('+'));
                    sbdCost.Replace("Rating", Rating.ToString(GlobalSettings.InvariantCultureInfo));

                    foreach (CharacterAttrib objLoopAttribute in _objCharacter.AttributeSection.AttributeList.Concat(
                                 _objCharacter.AttributeSection.SpecialAttributeList))
                    {
                        sbdCost.CheapReplace(strCostExpression, objLoopAttribute.Abbrev,
                                             () => objLoopAttribute.TotalValue.ToString(
                                                 GlobalSettings.InvariantCultureInfo));
                        sbdCost.CheapReplace(strCostExpression, objLoopAttribute.Abbrev + "Base",
                                             () => objLoopAttribute.TotalBase.ToString(
                                                 GlobalSettings.InvariantCultureInfo));
                    }

                    object objProcess
                        = CommonFunctions.EvaluateInvariantXPath(sbdCost.ToString(), out bool blnIsSuccess);
                    if (blnIsSuccess)
                        decReturn = Convert.ToDecimal(objProcess, GlobalSettings.InvariantCultureInfo);
                }

                if (DiscountCost)
                    decReturn *= 0.9m;

                return decReturn;
            }
        }

        /// <summary>
        /// The Armor's total Weight including Modifications.
        /// </summary>
        public decimal TotalWeight => OwnWeight + ArmorMods.Where(x => x.Equipped).Sum(x => x.TotalWeight)
                                                + GearChildren.Where(x => x.Equipped).Sum(x => x.TotalWeight);

        /// <summary>
        /// Weight for just the Armor.
        /// </summary>
        public decimal OwnWeight
        {
            get
            {
                string strWeightExpression = Weight;
                if (string.IsNullOrEmpty(strWeightExpression))
                    return 0;
                decimal decReturn = 0;
                using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdWeight))
                {
                    sbdWeight.Append(strWeightExpression.TrimStart('+'));
                    sbdWeight.Replace("Rating", Rating.ToString(GlobalSettings.InvariantCultureInfo));

                    foreach (CharacterAttrib objLoopAttribute in _objCharacter.AttributeSection.AttributeList.Concat(
                                 _objCharacter.AttributeSection.SpecialAttributeList))
                    {
                        sbdWeight.CheapReplace(strWeightExpression, objLoopAttribute.Abbrev,
                                               () => objLoopAttribute.TotalValue.ToString(
                                                   GlobalSettings.InvariantCultureInfo));
                        sbdWeight.CheapReplace(strWeightExpression, objLoopAttribute.Abbrev + "Base",
                                               () => objLoopAttribute.TotalBase.ToString(
                                                   GlobalSettings.InvariantCultureInfo));
                    }

                    object objProcess
                        = CommonFunctions.EvaluateInvariantXPath(sbdWeight.ToString(), out bool blnIsSuccess);
                    if (blnIsSuccess)
                        decReturn = Convert.ToDecimal(objProcess, GlobalSettings.InvariantCultureInfo);
                }

                return decReturn;
            }
        }

        /// <summary>
        /// The Modifications currently applied to the Armor.
        /// </summary>
        public TaggedObservableCollection<ArmorMod> ArmorMods
        {
            get
            {
                using (new EnterReadLock(_objCharacter.LockObject))
                    return _lstArmorMods;
            }
        }

        /// <summary>
        /// The Gear currently applied to the Armor.
        /// </summary>
        public TaggedObservableCollection<Gear> GearChildren
        {
            get
            {
                using (new EnterReadLock(_objCharacter.LockObject))
                    return _lstGear;
            }
        }

        /// <summary>
        /// Location.
        /// </summary>
        public Location Location
        {
            get => _objLocation;
            set => _objLocation = value;
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
        /// Forecolor to use for Notes in treeviews.
        /// </summary>
        public Color NotesColor
        {
            get => _colNotes;
            set => _colNotes = value;
        }

        /// <summary>
        /// Whether or not the Armor's cost should be discounted by 10% through the Black Market Pipeline Quality.
        /// </summary>
        public bool DiscountCost
        {
            get => _blnDiscountCost;
            set => _blnDiscountCost = value;
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
        /// Identifier of the object within data files.
        /// </summary>
        public Guid SourceID => _guiSourceID;

        /// <summary>
        /// String-formatted identifier of the <inheritdoc cref="SourceID"/> from the data files.
        /// </summary>
        public string SourceIDString => _guiSourceID.ToString("D", GlobalSettings.InvariantCultureInfo);

        public TaggedObservableCollection<Gear> Children => GearChildren;

        public bool Stolen
        {
            get => _blnStolen;
            set => _blnStolen = value;
        }

        /// <summary>
        /// Device Rating string.
        /// </summary>
        public string DeviceRating
        {
            get => _strDeviceRating;
            set => _strDeviceRating = value;
        }

        /// <summary>
        /// Attack.
        /// </summary>
        public string Attack
        {
            get => _strAttack;
            set => _strAttack = value;
        }

        /// <summary>
        /// Sleaze.
        /// </summary>
        public string Sleaze
        {
            get => _strSleaze;
            set => _strSleaze = value;
        }

        /// <summary>
        /// Data Processing.
        /// </summary>
        public string DataProcessing
        {
            get => _strDataProcessing;
            set => _strDataProcessing = value;
        }

        /// <summary>
        /// Firewall.
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

        public IEnumerable<IHasMatrixAttributes> ChildrenWithMatrixAttributes => Children;

        /// <summary>
        /// Commlink's Limit for how many Programs they can run.
        /// </summary>
        public string ProgramLimit
        {
            get => _strProgramLimit;
            set => _strProgramLimit = value;
        }

        /// <summary>
        /// ASDF attribute boosted by Overclocker.
        /// </summary>
        public string Overclocked
        {
            get => !_objCharacter.Overclocker ? string.Empty : _strOverclocked;
            set => _strOverclocked = value;
        }

        /// <summary>
        /// Returns true if this is a cyberdeck whose attributes we could swap around.
        /// </summary>
        public bool CanSwapAttributes
        {
            get => _blnCanSwapAttributes;
            set => _blnCanSwapAttributes = value;
        }

        /// <summary>
        /// Whether or not the Gear qualifies as a Program in the printout XML.
        /// </summary>
        public bool IsProgram => false;

        /// <summary>
        /// String to determine if gear can form persona or grants persona forming to its parent.
        /// </summary>
        public string CanFormPersona
        {
            get => _strCanFormPersona;
            set => _strCanFormPersona = value;
        }

        public bool IsCommlink
        {
            get { return CanFormPersona.Contains("Self") || Children.Any(x => x.CanFormPersona.Contains("Parent")); }
        }

        /// <summary>
        /// Base Matrix Boxes.
        /// </summary>
        public int BaseMatrixBoxes => 8;

        /// <summary>
        /// Bonus Matrix Boxes.
        /// </summary>
        public int BonusMatrixBoxes
        {
            get => _intMatrixCMBonus;
            set => _intMatrixCMBonus = value;
        }

        /// <summary>
        /// Total Bonus Matrix Boxes (including all children).
        /// </summary>
        public int TotalBonusMatrixBoxes
        {
            get
            {
                int intReturn = BonusMatrixBoxes;
                intReturn += Children.Where(g => g.Equipped).Sum(loopGear => loopGear.TotalBonusMatrixBoxes);
                return intReturn;
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

        #endregion Properties

        #region Complex Properties

        /// <summary>
        /// Total Availability in the program's current language.
        /// </summary>
        public string DisplayTotalAvail => TotalAvail(GlobalSettings.CultureInfo, GlobalSettings.Language);

        /// <summary>
        /// Total Availability.
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
            int intAvail = 0;
            if (strAvail.Length > 0)
            {
                if (strAvail.StartsWith("FixedValues(", StringComparison.Ordinal))
                {
                    string[] strValues = strAvail.TrimStartOnce("FixedValues(", true).TrimEndOnce(')').Split(',', StringSplitOptions.RemoveEmptyEntries);
                    strAvail = strValues[Math.Max(Math.Min(Rating, strValues.Length) - 1, 0)];
                }

                chrLastAvailChar = strAvail[strAvail.Length - 1];
                if (chrLastAvailChar == 'F' || chrLastAvailChar == 'R')
                {
                    strAvail = strAvail.Substring(0, strAvail.Length - 1);
                }

                blnModifyParentAvail = strAvail.StartsWith('+', '-');

                using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdAvail))
                {
                    sbdAvail.Append(strAvail.TrimStart('+'));
                    sbdAvail.Replace("Rating", Rating.ToString(GlobalSettings.InvariantCultureInfo));

                    foreach (CharacterAttrib objLoopAttribute in _objCharacter.AttributeSection.AttributeList.Concat(
                                 _objCharacter.AttributeSection.SpecialAttributeList))
                    {
                        sbdAvail.CheapReplace(strAvail, objLoopAttribute.Abbrev,
                                              () => objLoopAttribute.TotalValue.ToString(
                                                  GlobalSettings.InvariantCultureInfo));
                        sbdAvail.CheapReplace(strAvail, objLoopAttribute.Abbrev + "Base",
                                              () => objLoopAttribute.TotalBase.ToString(
                                                  GlobalSettings.InvariantCultureInfo));
                    }

                    object objProcess
                        = CommonFunctions.EvaluateInvariantXPath(sbdAvail.ToString(), out bool blnIsSuccess);
                    if (blnIsSuccess)
                        intAvail = ((double) objProcess).StandardRound();
                }
            }

            if (blnCheckChildren)
            {
                // Run through armor mod children and increase the Avail by any Mod whose Avail starts with "+" or "-".
                foreach (ArmorMod objChild in ArmorMods)
                {
                    if (!objChild.IncludedInArmor)
                    {
                        AvailabilityValue objLoopAvailTuple = objChild.TotalAvailTuple();
                        if (objLoopAvailTuple.AddToParent)
                            intAvail += objLoopAvailTuple.Value;
                        if (objLoopAvailTuple.Suffix == 'F')
                            chrLastAvailChar = 'F';
                        else if (chrLastAvailChar != 'F' && objLoopAvailTuple.Suffix == 'R')
                            chrLastAvailChar = 'R';
                    }
                }

                // Run through gear children and increase the Avail by any Mod whose Avail starts with "+" or "-".
                foreach (Gear objChild in GearChildren)
                {
                    if (objChild.ParentID != InternalId)
                    {
                        AvailabilityValue objLoopAvailTuple = objChild.TotalAvailTuple();
                        if (objLoopAvailTuple.AddToParent)
                            intAvail += objLoopAvailTuple.Value;
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

            return new AvailabilityValue(intAvail, chrLastAvailChar, blnModifyParentAvail);
        }

        /// <summary>
        /// Calculated Capacity of the Armor.
        /// </summary>
        public string CalculatedCapacity(CultureInfo objCultureInfo)
        {
            string strReturn = TotalArmorCapacity(objCultureInfo);

            // If an Armor Capacity is specified for the Armor, use that value.
            if (string.IsNullOrEmpty(strReturn) || strReturn == "0")
                strReturn = (0.0m).ToString("#,0.##", objCultureInfo);
            else if (strReturn == "Rating")
                strReturn = Rating.ToString(objCultureInfo);
            else if (decimal.TryParse(strReturn, NumberStyles.Any, objCultureInfo, out decimal decReturn))
                strReturn = decReturn.ToString("#,0.##", objCultureInfo);

            foreach (string strArmorModCapacity in ArmorMods.Select(x => x.ArmorCapacity))
            {
                if (!strArmorModCapacity.StartsWith('-')
                    && !strArmorModCapacity.StartsWith("[-", StringComparison.Ordinal))
                    continue;
                // If the Capacity is determined by the Capacity of the parent, evaluate the expression. Generally used for providing a percentage of armour capacity as bonus, ie YNT Softweave.
                // XPathExpression cannot evaluate while there are square brackets, so remove them if necessary.
                string strCapacity = strArmorModCapacity
                                     .FastEscape('[', ']')
                                     .CheapReplace("Capacity", () => TotalArmorCapacity(GlobalSettings.InvariantCultureInfo))
                                     .Replace("Rating", Rating.ToString(GlobalSettings.InvariantCultureInfo));

                object objProcess = CommonFunctions.EvaluateInvariantXPath(strCapacity, out bool blnIsSuccess);
                if (blnIsSuccess)
                {
                    strCapacity = (Convert.ToDecimal(strReturn, objCultureInfo)
                                   - Convert.ToDecimal(objProcess, GlobalSettings.InvariantCultureInfo))
                        .ToString("#,0.##", objCultureInfo);
                }

                strReturn = strCapacity;
            }

            return strReturn;
        }

        public string CurrentCalculatedCapacity => CalculatedCapacity(GlobalSettings.CultureInfo);

        /// <summary>
        /// The amount of Capacity remaining in the Armor.
        /// </summary>
        public decimal CapacityRemaining
        {
            get
            {
                // Get the Armor base Capacity.
                decimal decCapacity = Convert.ToDecimal(CalculatedCapacity(GlobalSettings.InvariantCultureInfo), GlobalSettings.InvariantCultureInfo);

                // If there is no Capacity (meaning that the Armor Suit Capacity or Maximum Armor Modification rule is turned off depending on the type of Armor), don't bother to calculate the remaining
                // Capacity since it's disabled and return 0 instead.
                if (decCapacity == 0)
                    return 0;

                // Calculate the remaining Capacity for a Suit of Armor.
                string strArmorCapacity = TotalArmorCapacity(GlobalSettings.InvariantCultureInfo);
                if (strArmorCapacity != "0" && !string.IsNullOrEmpty(strArmorCapacity)) // && _objCharacter.Settings.ArmorSuitCapacity)
                {
                    // Run through its Armor Mods and deduct the Capacity costs. Mods that confer capacity (ie negative values) are excluded, as they're processed in TotalArmorCapacity.
                    if (ArmorMods.Count > 0)
                        decCapacity -= ArmorMods.Where(x => !x.IncludedInArmor).Sum(x => Math.Max(x.TotalCapacity, 0));
                    // Run through its Gear and deduct the Armor Capacity costs.
                    if (GearChildren.Count > 0)
                        decCapacity -= GearChildren.Where(x => !x.IncludedInParent).Sum(x => x.PluginArmorCapacity * x.Quantity);
                }
                // Calculate the remaining Capacity for a standard piece of Armor using the Maximum Armor Modifications rules.
                else // if (_objCharacter.Settings.MaximumArmorModifications)
                {
                    // Run through its Armor Mods and deduct the Rating (or 1 if it has no Rating).
                    foreach (ArmorMod objMod in ArmorMods)
                    {
                        if (!objMod.IncludedInArmor)
                            decCapacity -= objMod.Rating > 0 ? objMod.Rating : 1;
                    }

                    // Run through its Gear and deduct the Rating (or 1 if it has no Rating).
                    foreach (Gear objGear in GearChildren)
                    {
                        if (!objGear.IncludedInParent)
                            decCapacity -= objGear.Rating > 0 ? objGear.Rating : 1;
                    }
                }

                return decCapacity;
            }
        }

        public string DisplayCapacity
        {
            get
            {
                string strCalculatedCapacity = CurrentCalculatedCapacity;
                if (strCalculatedCapacity.Contains('[') && !strCalculatedCapacity.Contains("/["))
                    return strCalculatedCapacity;
                return string.Format(GlobalSettings.CultureInfo, LanguageManager.GetString("String_CapacityRemaining"),
                                     strCalculatedCapacity, CapacityRemaining.ToString("#,0.##", GlobalSettings.CultureInfo));
            }
        }

        /// <summary>
        /// Capacity display style.
        /// </summary>
        public CapacityStyle CapacityDisplayStyle
        {
            get
            {
                string strArmorCapacity = ArmorCapacity;
                if (!string.IsNullOrEmpty(strArmorCapacity) && strArmorCapacity != "0")
                {
                    return CapacityStyle.Standard;
                }

                return CapacityStyle.Zero;
            }
        }

        /// <summary>
        /// The name of the object as it should appear on printouts (translated name only).
        /// </summary>
        public string DisplayNameShort(string strLanguage)
        {
            if (strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                return Name;

            return this.GetNodeXPath(strLanguage)?.SelectSingleNodeAndCacheExpression("translate")?.Value ?? Name;
        }

        /// <summary>
        /// The name of the object as it should appear on printouts (translated name only).
        /// </summary>
        public async ValueTask<string> DisplayNameShortAsync(string strLanguage)
        {
            if (strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                return Name;

            return (await this.GetNodeXPathAsync(strLanguage))?.SelectSingleNodeAndCacheExpression("translate")?.Value ?? Name;
        }

        /// <summary>
        /// The name of the object as it should be displayed in lists. Qty Name (Rating) (Extra).
        /// </summary>
        public string DisplayName(CultureInfo objCulture, string strLanguage)
        {
            string strReturn = DisplayNameShort(strLanguage);
            string strSpace = LanguageManager.GetString("String_Space", strLanguage);
            if (!string.IsNullOrEmpty(CustomName))
                strReturn += strSpace + "(\"" + CustomName + "\")";
            if (Rating > 0)
                strReturn += strSpace + '(' + LanguageManager.GetString(RatingLabel, strLanguage) + strSpace + Rating.ToString(objCulture) + ')';
            if (!string.IsNullOrEmpty(Extra))
                strReturn += strSpace + '(' + _objCharacter.TranslateExtra(Extra, strLanguage) + ')';
            return strReturn;
        }

        /// <summary>
        /// The name of the object as it should be displayed in lists. Qty Name (Rating) (Extra).
        /// </summary>
        public async ValueTask<string> DisplayNameAsync(CultureInfo objCulture, string strLanguage)
        {
            string strReturn = await DisplayNameShortAsync(strLanguage);
            string strSpace = await LanguageManager.GetStringAsync("String_Space", strLanguage);
            if (!string.IsNullOrEmpty(CustomName))
                strReturn += strSpace + "(\"" + CustomName + "\")";
            if (Rating > 0)
                strReturn += strSpace + '(' + await LanguageManager.GetStringAsync(RatingLabel, strLanguage) + strSpace + Rating.ToString(objCulture) + ')';
            if (!string.IsNullOrEmpty(Extra))
                strReturn += strSpace + '(' + await _objCharacter.TranslateExtraAsync(Extra, strLanguage) + ')';
            return strReturn;
        }

        public string CurrentDisplayName => DisplayName(GlobalSettings.CultureInfo, GlobalSettings.Language);

        /// <summary>
        /// A custom name for the Armor assigned by the player.
        /// </summary>
        public string CustomName
        {
            get => _strArmorName;
            set => _strArmorName = value;
        }

        private XmlNode _objCachedMyXmlNode;
        private string _strCachedXmlNodeLanguage = string.Empty;

        public async Task<XmlNode> GetNodeCoreAsync(bool blnSync, string strLanguage)
        {
            if (_objCachedMyXmlNode != null && strLanguage == _strCachedXmlNodeLanguage && !GlobalSettings.LiveCustomData)
                return _objCachedMyXmlNode;
            _objCachedMyXmlNode = (blnSync
                    // ReSharper disable once MethodHasAsyncOverload
                    ? _objCharacter.LoadData("armor.xml", strLanguage)
                    : await _objCharacter.LoadDataAsync("armor.xml", strLanguage))
                .SelectSingleNode(SourceID == Guid.Empty
                                      ? "/chummer/armors/armor[name = "
                                        + Name.CleanXPath() + ']'
                                      : "/chummer/armors/armor[id = "
                                        + SourceIDString.CleanXPath() + " or id = "
                                        + SourceIDString.ToUpperInvariant().CleanXPath()
                                        + ']');

            _strCachedXmlNodeLanguage = strLanguage;
            return _objCachedMyXmlNode;
        }

        private XPathNavigator _objCachedMyXPathNode;
        private string _strCachedXPathNodeLanguage = string.Empty;

        public async Task<XPathNavigator> GetNodeXPathCoreAsync(bool blnSync, string strLanguage)
        {
            if (_objCachedMyXPathNode != null && strLanguage == _strCachedXPathNodeLanguage
                                              && !GlobalSettings.LiveCustomData)
                return _objCachedMyXPathNode;
            _objCachedMyXPathNode = (blnSync
                    // ReSharper disable once MethodHasAsyncOverload
                    ? _objCharacter.LoadDataXPath("armor.xml", strLanguage)
                    : await _objCharacter.LoadDataXPathAsync("armor.xml", strLanguage))
                .SelectSingleNode(SourceID == Guid.Empty
                                      ? "/chummer/armors/armor[name = "
                                        + Name.CleanXPath() + ']'
                                      : "/chummer/armors/armor[id = "
                                        + SourceIDString.CleanXPath() + " or id = "
                                        + SourceIDString.ToUpperInvariant().CleanXPath()
                                        + ']');
            _strCachedXPathNodeLanguage = strLanguage;
            return _objCachedMyXPathNode;
        }

        public int GetBaseMatrixAttribute(string strAttributeName)
        {
            string strExpression = this.GetMatrixAttributeString(strAttributeName);
            if (string.IsNullOrEmpty(strExpression))
            {
                switch (strAttributeName)
                {
                    case "Device Rating":
                        strExpression = "2";
                        break;

                    case "Program Limit":
                        if (IsCommlink)
                        {
                            strExpression = this.GetMatrixAttributeString("Device Rating");
                            if (string.IsNullOrEmpty(strExpression))
                                strExpression = "2";
                        }
                        else
                            strExpression = "0";
                        break;

                    case "Data Processing":
                    case "Firewall":
                        strExpression = this.GetMatrixAttributeString("Device Rating");
                        if (string.IsNullOrEmpty(strExpression))
                            strExpression = "2";
                        break;

                    default:
                        strExpression = "0";
                        break;
                }
            }

            if (strExpression.IndexOfAny('{', '+', '-', '*', ',') != -1 || strExpression.Contains("div"))
            {
                using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdValue))
                {
                    sbdValue.Append(strExpression);
                    foreach (string strMatrixAttribute in MatrixAttributes.MatrixAttributeStrings)
                    {
                        sbdValue.CheapReplace(strExpression, "{Gear " + strMatrixAttribute + '}', () => "0");
                        sbdValue.CheapReplace(strExpression, "{Parent " + strMatrixAttribute + '}', () => "0");
                        if (Children.Count > 0 && strExpression.Contains("{Children " + strMatrixAttribute + '}'))
                        {
                            int intTotalChildrenValue = 0;
                            foreach (Gear objLoopGear in Children)
                            {
                                if (objLoopGear.Equipped)
                                {
                                    intTotalChildrenValue += objLoopGear.GetBaseMatrixAttribute(strMatrixAttribute);
                                }
                            }

                            sbdValue.Replace("{Children " + strMatrixAttribute + '}',
                                             intTotalChildrenValue.ToString(GlobalSettings.InvariantCultureInfo));
                        }
                    }

                    _objCharacter.AttributeSection.ProcessAttributesInXPath(sbdValue, strExpression);
                    // Replace the division sign with "div" since we're using XPath.
                    sbdValue.Replace("/", " div ");
                    // This is first converted to a decimal and rounded up since some items have a multiplier that is not a whole number, such as 2.5.
                    object objProcess
                        = CommonFunctions.EvaluateInvariantXPath(sbdValue.ToString(), out bool blnIsSuccess);
                    return blnIsSuccess ? ((double) objProcess).StandardRound() : 0;
                }
            }
            int.TryParse(strExpression, NumberStyles.Any, GlobalSettings.InvariantCultureInfo, out int intReturn);
            return intReturn;
        }

        public int GetBonusMatrixAttribute(string strAttributeName)
        {
            if (string.IsNullOrEmpty(strAttributeName))
                return 0;
            int intReturn = 0;

            if (Overclocked == strAttributeName)
            {
                ++intReturn;
            }

            if (!strAttributeName.StartsWith("Mod ", StringComparison.Ordinal))
                strAttributeName = "Mod " + strAttributeName;

            foreach (Gear objGear in Children)
            {
                if (objGear.Equipped && objGear.ParentID != InternalId)
                {
                    intReturn += objGear.GetTotalMatrixAttribute(strAttributeName);
                }
            }

            return intReturn;
        }

        #endregion Complex Properties

        #region Methods

        /// <summary>
        /// Method to delete an Armor object. Returns total extra cost removed unrelated to children.
        /// </summary>
        public decimal DeleteArmor()
        {
            _objCharacter.Armor.Remove(this);

            decimal decReturn = 0.0m;
            // Remove any Improvements created by the Armor and its children.
            foreach (ArmorMod objMod in ArmorMods)
                decReturn += objMod.DeleteArmorMod(false);
            // Remove any Improvements created by the Armor's Gear.
            foreach (Gear objGear in GearChildren)
                decReturn += objGear.DeleteGear(false);

            decReturn += ImprovementManager.RemoveImprovements(_objCharacter, Improvement.ImprovementSource.Armor, InternalId);

            // Remove the Cyberweapon created by the Mod if applicable.
            if (!WeaponID.IsEmptyGuid())
            {
                foreach (Weapon objDeleteWeapon in _objCharacter.Weapons.DeepWhere(x => x.Children, x => x.ParentID == InternalId).ToList())
                {
                    decReturn += objDeleteWeapon.TotalCost + objDeleteWeapon.DeleteWeapon();
                }
                foreach (Vehicle objVehicle in _objCharacter.Vehicles)
                {
                    foreach (Weapon objDeleteWeapon in objVehicle.Weapons.DeepWhere(x => x.Children, x => x.ParentID == InternalId).ToList())
                    {
                        decReturn += objDeleteWeapon.TotalCost + objDeleteWeapon.DeleteWeapon();
                    }

                    foreach (VehicleMod objMod in objVehicle.Mods)
                    {
                        foreach (Weapon objDeleteWeapon in objMod.Weapons.DeepWhere(x => x.Children, x => x.ParentID == InternalId).ToList())
                        {
                            decReturn += objDeleteWeapon.TotalCost + objDeleteWeapon.DeleteWeapon();
                        }
                    }

                    foreach (WeaponMount objMount in objVehicle.WeaponMounts)
                    {
                        foreach (Weapon objDeleteWeapon in objMount.Weapons.DeepWhere(x => x.Children, x => x.ParentID == InternalId).ToList())
                        {
                            decReturn += objDeleteWeapon.TotalCost + objDeleteWeapon.DeleteWeapon();
                        }
                    }
                }
            }

            DisposeSelf();

            return decReturn;
        }

        /// <summary>
        /// Toggle the Wireless Bonus for this armor.
        /// </summary>
        public void RefreshWirelessBonuses()
        {
            if (!string.IsNullOrEmpty(WirelessBonus?.InnerText))
            {
                if (WirelessOn && Equipped)
                {
                    if (WirelessBonus.SelectSingleNode("@mode")?.Value == "replace")
                    {
                        ImprovementManager.DisableImprovements(_objCharacter,
                                                               _objCharacter.Improvements.Where(x =>
                                                                   x.ImproveSource == Improvement.ImprovementSource
                                                                       .Armor &&
                                                                   x.SourceName == InternalId));
                    }

                    ImprovementManager.CreateImprovements(_objCharacter, Improvement.ImprovementSource.Armor, InternalId + "Wireless", WirelessBonus, Rating, DisplayNameShort(GlobalSettings.Language));

                    if (!string.IsNullOrEmpty(ImprovementManager.SelectedValue) && string.IsNullOrEmpty(_strExtra))
                        _strExtra = ImprovementManager.SelectedValue;
                }
                else
                {
                    if (WirelessBonus.SelectSingleNode("@mode")?.Value == "replace")
                    {
                        ImprovementManager.EnableImprovements(_objCharacter,
                                                              _objCharacter.Improvements.Where(x =>
                                                                  x.ImproveSource == Improvement.ImprovementSource
                                                                      .Armor &&
                                                                  x.SourceName == InternalId));
                    }

                    string strSourceNameToRemove = InternalId + "Wireless";
                    ImprovementManager.RemoveImprovements(_objCharacter,
                                                          _objCharacter.Improvements.Where(x =>
                                                              x.ImproveSource == Improvement.ImprovementSource.Armor
                                                              &&
                                                              x.SourceName == strSourceNameToRemove).ToList());
                }
            }

            foreach (ArmorMod objArmorMod in ArmorMods)
                objArmorMod.RefreshWirelessBonuses();
            foreach (Gear objGear in GearChildren)
                objGear.RefreshWirelessBonuses();
        }

        #region UI Methods

        /// <summary>
        /// Add a piece of Armor to the Armor TreeView.
        /// </summary>
        /// <param name="cmsArmor">ContextMenuStrip for the Armor Node.</param>
        /// <param name="cmsArmorMod">ContextMenuStrip for Armor Mod Nodes.</param>
        /// <param name="cmsArmorGear">ContextMenuStrip for Armor Gear Nodes.</param>
        public TreeNode CreateTreeNode(ContextMenuStrip cmsArmor, ContextMenuStrip cmsArmorMod, ContextMenuStrip cmsArmorGear)
        {
            //if (!string.IsNullOrEmpty(ParentID) && !string.IsNullOrEmpty(Source) && !_objCharacter.Settings.BookEnabled(Source))
            //return null;

            TreeNode objNode = new TreeNode
            {
                Name = InternalId,
                Text = CurrentDisplayName,
                Tag = this,
                ContextMenuStrip = cmsArmor,
                ForeColor = PreferredColor,
                ToolTipText = Notes.WordWrap()
            };

            TreeNodeCollection lstChildNodes = objNode.Nodes;
            foreach (ArmorMod objMod in ArmorMods)
            {
                TreeNode objLoopNode = objMod.CreateTreeNode(cmsArmorMod, cmsArmorGear);
                if (objLoopNode != null)
                    lstChildNodes.Add(objLoopNode);
            }
            foreach (Gear objGear in GearChildren)
            {
                TreeNode objLoopNode = objGear.CreateTreeNode(cmsArmorGear);
                if (objLoopNode != null)
                    lstChildNodes.Add(objLoopNode);
            }
            if (lstChildNodes.Count > 0)
                objNode.Expand();

            return objNode;
        }

        public Color PreferredColor =>
            !string.IsNullOrEmpty(Notes)
                ? ColorManager.GenerateCurrentModeColor(NotesColor)
                : ColorManager.WindowText;

        #endregion UI Methods

        #endregion Methods

        public bool Remove(bool blnConfirmDelete = true)
        {
            if (blnConfirmDelete && !CommonFunctions.ConfirmDelete(LanguageManager.GetString("Message_DeleteArmor")))
                return false;
            DeleteArmor();
            return true;
        }

        public bool Sell(decimal percentage, bool blnConfirmDelete)
        {
            if (blnConfirmDelete && !CommonFunctions.ConfirmDelete(LanguageManager.GetString("Message_DeleteArmor")))
                return false;

            if (!_objCharacter.Created)
            {
                DeleteArmor();
                return true;
            }
            // Create the Expense Log Entry for the sale.
            decimal decAmount = TotalCost * percentage;
            decAmount += DeleteArmor() * percentage;
            ExpenseLogEntry objExpense = new ExpenseLogEntry(_objCharacter);
            objExpense.Create(decAmount, LanguageManager.GetString("String_ExpenseSoldArmor") + ' ' + DisplayNameShort(GlobalSettings.Language), ExpenseType.Nuyen, DateTime.Now);
            _objCharacter.ExpenseEntries.AddWithSort(objExpense);
            _objCharacter.Nuyen += decAmount;
            return true;
        }

        /// <summary>
        /// Alias map for SourceDetail control text and tooltip assignation.
        /// </summary>
        /// <param name="sourceControl"></param>
        public void SetSourceDetail(Control sourceControl)
        {
            if (_objCachedSourceDetail.Language != GlobalSettings.Language)
                _objCachedSourceDetail = default;
            SourceDetail.SetControl(sourceControl);
        }

        /// <summary>
        /// Checks a nominated piece of gear for Availability requirements.
        /// </summary>
        /// <param name="dicRestrictedGearLimits">Dictionary of Restricted Gear availabilities still available with the amount of items that can still use that availability.</param>
        /// <param name="sbdAvailItems">StringBuilder used to list names of gear that are currently over the availability limit.</param>
        /// <param name="sbdRestrictedItems">StringBuilder used to list names of gear that are being used for Restricted Gear.</param>
        /// <param name="intRestrictedCount">Number of items that are above availability limits.</param>
        public void CheckRestrictedGear(IDictionary<int, int> dicRestrictedGearLimits, StringBuilder sbdAvailItems, StringBuilder sbdRestrictedItems, ref int intRestrictedCount)
        {
            AvailabilityValue objTotalAvail = TotalAvailTuple();
            int intAvailInt = objTotalAvail.Value;
            if (intAvailInt > _objCharacter.Settings.MaximumAvailability)
            {
                int intLowestValidRestrictedGearAvail = -1;
                foreach (int intValidAvail in dicRestrictedGearLimits.Keys)
                {
                    if (intValidAvail >= intAvailInt && (intLowestValidRestrictedGearAvail < 0
                                                         || intValidAvail < intLowestValidRestrictedGearAvail))
                        intLowestValidRestrictedGearAvail = intValidAvail;
                }

                if (intLowestValidRestrictedGearAvail >= 0 && dicRestrictedGearLimits[intLowestValidRestrictedGearAvail] > 0)
                {
                    --dicRestrictedGearLimits[intLowestValidRestrictedGearAvail];
                    sbdRestrictedItems.AppendLine().Append("\t\t").Append(CurrentDisplayName);
                }
                else
                {
                    dicRestrictedGearLimits.Remove(intLowestValidRestrictedGearAvail);
                    ++intRestrictedCount;
                    sbdAvailItems.AppendLine().Append("\t\t").Append(CurrentDisplayName);
                }
            }

            foreach (Gear objChild in Children)
            {
                objChild.CheckRestrictedGear(dicRestrictedGearLimits, sbdAvailItems, sbdRestrictedItems, ref intRestrictedCount);
            }

            foreach (ArmorMod objChild in ArmorMods)
            {
                objChild.CheckRestrictedGear(dicRestrictedGearLimits, sbdAvailItems, sbdRestrictedItems, ref intRestrictedCount);
            }
        }

        public bool AllowPasteXml
        {
            get
            {
                string strCapacity = CalculatedCapacity(GlobalSettings.InvariantCultureInfo);
                if (string.IsNullOrEmpty(strCapacity) || strCapacity == "0")
                    return false;
                string strPasteCategory = GlobalSettings.Clipboard.SelectSingleNode("category")?.Value ?? string.Empty;
                switch (GlobalSettings.ClipboardContentType)
                {
                    case ClipboardContentType.ArmorMod:
                        {
                            XPathNavigator xmlNode = this.GetNodeXPath();
                            if (xmlNode == null)
                                return strPasteCategory == "General";
                            XPathNavigator xmlForceModCategory = xmlNode.SelectSingleNode("forcemodcategory");
                            if (xmlForceModCategory != null)
                                return xmlForceModCategory.Value == strPasteCategory;
                            if (strPasteCategory == "General")
                                return true;
                            XPathNodeIterator xmlAddonCategoryList = xmlNode.Select("addoncategory");
                            return xmlAddonCategoryList.Count <= 0 || xmlAddonCategoryList.Cast<XPathNavigator>().Any(xmlCategory => xmlCategory.Value == strPasteCategory);
                        }
                    case ClipboardContentType.Gear:
                        {
                            XPathNavigator xmlNode = this.GetNodeXPath();
                            if (xmlNode == null)
                                return false;
                            XPathNodeIterator xmlAddonCategoryList = xmlNode.Select("addoncategory");
                            return xmlAddonCategoryList.Count <= 0 || xmlAddonCategoryList.Cast<XPathNavigator>().Any(xmlCategory => xmlCategory.Value == strPasteCategory);
                        }
                    default:
                        return false;
                }
            }
        }

        public bool AllowPasteObject(object input)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            foreach (ArmorMod objChild in _lstArmorMods)
                objChild.Dispose();
            foreach (Gear objChild in _lstGear)
                objChild.Dispose();
            DisposeSelf();
        }

        private void DisposeSelf()
        {
            _lstArmorMods.Dispose();
            _lstGear.Dispose();
        }
    }
}
