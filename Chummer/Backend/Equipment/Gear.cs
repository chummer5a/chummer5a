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
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;
using Chummer.Annotations;
using Chummer.Backend.Attributes;
using NLog;

namespace Chummer.Backend.Equipment
{
    /// <summary>
    /// Standard Character Gear.
    /// </summary>
    [HubClassTag("SourceID", true, "Name", "Extra")]
    [DebuggerDisplay("{DisplayName(GlobalSettings.InvariantCultureInfo, GlobalSettings.DefaultLanguage)}")]
    public sealed class Gear : IHasChildrenAndCost<Gear>, IHasName, IHasSourceId, IHasInternalId, IHasXmlDataNode, IHasMatrixAttributes,
        IHasNotes, ICanSell, IHasLocation, ICanEquip, IHasSource, IHasRating, INotifyMultiplePropertyChangedAsync, ICanSort,
        IHasStolenProperty, ICanPaste, IHasWirelessBonus, IHasGear, ICanBlackMarketDiscount, IDisposable, IAsyncDisposable
    {
        private static readonly Lazy<Logger> s_ObjLogger = new Lazy<Logger>(LogManager.GetCurrentClassLogger);
        private static Logger Log => s_ObjLogger.Value;
        private Guid _guiID;
        private Guid _guiSourceID;
        private string _strName = string.Empty;
        private string _strCategory = string.Empty;
        private string _strMaxRating = string.Empty;
        private string _strMinRating = string.Empty;
        private string _strRatingLabel = "String_Rating";
        private int _intRating;
        private decimal _decQty = 1.0m;
        private string _strCapacity = string.Empty;
        private string _strArmorCapacity = string.Empty;
        private string _strAvail = string.Empty;
        private decimal _decCostFor = 1.0m;
        private string _strDeviceRating = string.Empty;
        private string _strCost = string.Empty;
        private string _strWeight = string.Empty;
        private string _strSource = string.Empty;
        private string _strPage = string.Empty;
        private string _strExtra = string.Empty;
        private string _strCanFormPersona = string.Empty;
        private string _strAmmoForWeaponType = string.Empty;
        private bool _blnBonded;
        private bool _blnEquipped = true;
        private bool _blnWirelessOn = true;
        private XmlNode _nodBonus;
        private XmlNode _nodWirelessBonus;
        private XmlNode _nodWeaponBonus;
        private XmlNode _nodFlechetteWeaponBonus;
        private Guid _guiWeaponID = Guid.Empty;
        private readonly TaggedObservableCollection<Gear> _lstChildren = new TaggedObservableCollection<Gear>();
        private string _strNotes = string.Empty;
        private Color _colNotes = ColorManager.HasNotesColor;
        private Location _objLocation;
        private readonly Character _objCharacter;
        private int _intChildCostMultiplier = 1;
        private int _intChildAvailModifier;
        private bool _blnDiscountCost;
        private string _strGearName = string.Empty;
        private string _strParentID = string.Empty;
        private int _intMatrixCMBonus;
        private int _intMatrixCMFilled;
        private string _strForcedValue = string.Empty;
        private bool _blnAllowRename;
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
        private bool _blnStolen;
        private bool _blnIsFlechetteAmmo;
        private Clip _objLoadedIntoClip; // Set on loading in weapon clips

        #region Constructor, Create, Save, Load, and Print Methods

        public Gear(Character objCharacter)
        {
            // Create the GUID for the new piece of Gear.
            _guiID = Guid.NewGuid();
            _objCharacter = objCharacter;

            _lstChildren.AddTaggedCollectionChanged(this, ChildrenOnCollectionChanged);
        }

        private async Task ChildrenOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (e.Action == NotifyCollectionChangedAction.Move)
                return;
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (Gear objNewItem in e.NewItems)
                    {
                        objNewItem.Parent = this;
                        if (Equipped)
                            await objNewItem.ChangeEquippedStatusAsync(true, token: token).ConfigureAwait(false);
                    }
                    break;

                case NotifyCollectionChangedAction.Remove:
                    foreach (Gear objOldItem in e.OldItems)
                    {
                        objOldItem.Parent = null;
                        if (Equipped)
                            await objOldItem.ChangeEquippedStatusAsync(false, token: token).ConfigureAwait(false);
                    }
                    break;

                case NotifyCollectionChangedAction.Replace:
                    foreach (Gear objOldItem in e.OldItems)
                    {
                        objOldItem.Parent = null;
                        if (Equipped)
                            await objOldItem.ChangeEquippedStatusAsync(false, token: token).ConfigureAwait(false);
                    }
                    foreach (Gear objNewItem in e.NewItems)
                    {
                        objNewItem.Parent = this;
                        if (Equipped)
                            await objNewItem.ChangeEquippedStatusAsync(true, token: token).ConfigureAwait(false);
                    }
                    break;
            }
            await this.RefreshMatrixAttributeArrayAsync(_objCharacter, token).ConfigureAwait(false);
        }

        /// <summary>
        /// Create a Gear from an XmlNode and return the TreeNodes for it.
        /// </summary>
        /// <param name="objXmlGear">XmlNode to create the object from.</param>
        /// <param name="intRating">Selected Rating for the Gear.</param>
        /// <param name="lstWeapons">List of Weapons that should be added to the character.</param>
        /// <param name="strForceValue">Value to forcefully select for any ImprovementManager prompts.</param>
        /// <param name="blnAddImprovements">Whether or not Improvements should be added to the character.</param>
        /// <param name="blnCreateChildren">Whether or not child Gear should be created.</param>
        /// <param name="blnSkipSelectForms">Whether or not to skip forms that are created for bonuses. Use only when creating Gear for previews in selection forms.</param>
        public void Create(XmlNode objXmlGear, int intRating, ICollection<Weapon> lstWeapons, string strForceValue = "",
            bool blnAddImprovements = true, bool blnCreateChildren = true, bool blnSkipSelectForms = false)
        {
            if (objXmlGear == null)
                return;
            _blnEquipped = !blnSkipSelectForms;
            _strForcedValue = strForceValue;
            if (!objXmlGear.TryGetField("id", Guid.TryParse, out _guiSourceID))
            {
                Log.Warn(new object[] { "Missing id field for armor xmlnode", objXmlGear });
                Utils.BreakIfDebug();
            }
            else
            {
                _objCachedMyXmlNode = null;
                _objCachedMyXPathNode = null;
            }

            if (objXmlGear.TryGetStringFieldQuickly("name", ref _strName))
            {
                _objCachedMyXmlNode = null;
                _objCachedMyXPathNode = null;
            }

            if (objXmlGear.TryGetStringFieldQuickly("category", ref _strCategory))
            {
                _objCachedMyXmlNode = null;
                _objCachedMyXPathNode = null;
            }

            objXmlGear.TryGetStringFieldQuickly("avail", ref _strAvail);
            objXmlGear.TryGetStringFieldQuickly("capacity", ref _strCapacity);
            objXmlGear.TryGetStringFieldQuickly("armorcapacity", ref _strArmorCapacity);
            objXmlGear.TryGetDecFieldQuickly("costfor", ref _decCostFor);
            _decQty = _decCostFor;
            if (!objXmlGear.TryGetStringFieldQuickly("cost", ref _strCost))
                _strCost = string.Empty;
            if (!objXmlGear.TryGetStringFieldQuickly("weight", ref _strWeight))
                _strWeight = string.Empty;
            _nodBonus = objXmlGear["bonus"];
            _nodWirelessBonus = objXmlGear["wirelessbonus"];
            _blnWirelessOn = false;
            objXmlGear.TryGetStringFieldQuickly("ratinglabel", ref _strRatingLabel);
            objXmlGear.TryGetStringFieldQuickly("rating", ref _strMaxRating);
            if (_strMaxRating == "0")
                _strMaxRating = string.Empty;
            objXmlGear.TryGetStringFieldQuickly("minrating", ref _strMinRating);
            if (!objXmlGear.TryGetMultiLineStringFieldQuickly("altnotes", ref _strNotes))
                objXmlGear.TryGetMultiLineStringFieldQuickly("notes", ref _strNotes);

            string sNotesColor = ColorTranslator.ToHtml(ColorManager.HasNotesColor);
            objXmlGear.TryGetStringFieldQuickly("notesColor", ref sNotesColor);
            _colNotes = ColorTranslator.FromHtml(sNotesColor);
            _intRating = Math.Max(Math.Min(intRating, MaxRatingValue), MinRatingValue);
            objXmlGear.TryGetStringFieldQuickly("devicerating", ref _strDeviceRating);
            objXmlGear.TryGetInt32FieldQuickly("matrixcmbonus", ref _intMatrixCMBonus);
            objXmlGear.TryGetStringFieldQuickly("source", ref _strSource);
            objXmlGear.TryGetStringFieldQuickly("page", ref _strPage);
            objXmlGear.TryGetStringFieldQuickly("canformpersona", ref _strCanFormPersona);
            objXmlGear.TryGetStringFieldQuickly("ammoforweapontype", ref _strAmmoForWeaponType);
            objXmlGear.TryGetInt32FieldQuickly("childcostmultiplier", ref _intChildCostMultiplier);
            objXmlGear.TryGetInt32FieldQuickly("childavailmodifier", ref _intChildAvailModifier);
            objXmlGear.TryGetBoolFieldQuickly("allowrename", ref _blnAllowRename);
            objXmlGear.TryGetBoolFieldQuickly("stolen", ref _blnStolen);
            objXmlGear.TryGetBoolFieldQuickly("isflechetteammo", ref _blnIsFlechetteAmmo);

            if (GlobalSettings.InsertPdfNotesIfAvailable && string.IsNullOrEmpty(Notes))
            {
                Notes = CommonFunctions.GetBookNotes(objXmlGear, Name, CurrentDisplayName, Source, Page,
                    DisplayPage(GlobalSettings.Language), _objCharacter);
            }

            // Check for a Custom name
            if (_strName == "Custom Item")
            {
                if (!string.IsNullOrEmpty(_strForcedValue))
                {
                    string strCustomName =
                        LanguageManager.GetString(_strForcedValue, GlobalSettings.DefaultLanguage, false);
                    if (string.IsNullOrEmpty(strCustomName))
                        strCustomName = _strForcedValue;
                    _strName = strCustomName;
                    _objCachedMyXmlNode = null;
                    _objCachedMyXPathNode = null;
                }
                else if (!blnSkipSelectForms)
                {
                    using (ThreadSafeForm<SelectText> frmPickText = ThreadSafeForm<SelectText>.Get(() => new SelectText
                    {
                        PreventXPathErrors = true,
                        Description = LanguageManager.GetString("String_CustomItem_SelectText")
                    }))
                    {
                        // Make sure the dialogue window was not canceled.
                        if (frmPickText.ShowDialogSafe(_objCharacter) == DialogResult.Cancel)
                        {
                            _guiID = Guid.Empty;
                            return;
                        }

                        string strCustomName = LanguageManager.GetString(frmPickText.MyForm.SelectedValue,
                                                                         GlobalSettings.DefaultLanguage, false);
                        if (string.IsNullOrEmpty(strCustomName))
                            strCustomName =
                                _objCharacter.ReverseTranslateExtra(frmPickText.MyForm.SelectedValue);
                        _strName = strCustomName;
                        _objCachedMyXmlNode = null;
                        _objCachedMyXPathNode = null;
                    }
                }
            }

            // Check for a Variable Cost.
            if (!string.IsNullOrEmpty(_strCost) && _strCost.StartsWith("Variable(", StringComparison.Ordinal) &&
                string.IsNullOrEmpty(_strForcedValue))
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

                    if (decMin != 0 || decMax != decimal.MaxValue)
                    {
                        if (decMax > 1000000)
                            decMax = 1000000;
                        using (ThreadSafeForm<SelectNumber> frmPickNumber
                               = ThreadSafeForm<SelectNumber>.Get(() => new SelectNumber(_objCharacter.Settings.MaxNuyenDecimals)
                               {
                                   Minimum = decMin,
                                   Maximum = decMax,
                                   Description = string.Format(
                                       GlobalSettings.CultureInfo,
                                       LanguageManager.GetString("String_SelectVariableCost"),
                                       CurrentDisplayNameShort),
                                   AllowCancel = false
                               }))
                        {
                            if (frmPickNumber.ShowDialogSafe(_objCharacter) == DialogResult.Cancel)
                            {
                                _guiID = Guid.Empty;
                                return;
                            }
                            _strCost = frmPickNumber.MyForm.SelectedValue.ToString(GlobalSettings.InvariantCultureInfo);
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
                // If the Gear is Ammunition, ask the user to select a Weapon Category for it to be limited to.
                string strAmmoWeaponType = string.Empty;
                bool blnDoExtra = false;
                if (objXmlGear.TryGetStringFieldQuickly("ammoforweapontype", ref strAmmoWeaponType))
                    blnDoExtra = objXmlGear.SelectSingleNodeAndCacheExpressionAsNavigator("ammoforweapontype/@noextra")?.Value != bool.TrueString;
                if (!string.IsNullOrEmpty(strAmmoWeaponType) && blnDoExtra)
                {
                    using (ThreadSafeForm<SelectWeaponCategory> frmPickWeaponCategory
                           = ThreadSafeForm<SelectWeaponCategory>.Get(() => new SelectWeaponCategory(_objCharacter)
                           {
                               Description = LanguageManager.GetString("String_SelectWeaponCategoryAmmo"),
                               WeaponType = strAmmoWeaponType
                           }))
                    {
                        if (!string.IsNullOrEmpty(_strForcedValue) &&
                            !_strForcedValue.Equals(_strName, StringComparison.Ordinal))
                            frmPickWeaponCategory.MyForm.OnlyCategory = _strForcedValue;

                        if (frmPickWeaponCategory.ShowDialogSafe(_objCharacter)
                            != DialogResult.OK)
                        {
                            _guiID = Guid.Empty;
                            return;
                        }

                        if (!string.IsNullOrEmpty(frmPickWeaponCategory.MyForm.SelectedCategory))
                            _strExtra = frmPickWeaponCategory.MyForm.SelectedCategory;
                    }
                }
            }

            // Add Gear Weapons if applicable.
            using (XmlNodeList xmlWeaponList = objXmlGear.SelectNodes("addweapon"))
            {
                if (xmlWeaponList != null)
                {
                    XmlDocument objXmlWeaponDocument = _objCharacter.LoadData("weapons.xml");

                    // More than one Weapon can be added, so loop through all occurrences.
                    foreach (XmlNode objXmlAddWeapon in xmlWeaponList)
                    {
                        string strLoopID = objXmlAddWeapon.InnerText;
                        XmlNode objXmlWeapon =
                            objXmlWeaponDocument.TryGetNodeByNameOrId("/chummer/weapons/weapon",
                                strLoopID);

                        if (objXmlWeapon != null)
                        {
                            int intAddWeaponRating = 0;
                            string strLoopRating = objXmlAddWeapon.Attributes["rating"]?.InnerText;
                            if (!string.IsNullOrEmpty(strLoopRating))
                            {
                                strLoopRating = strLoopRating.CheapReplace("{Rating}",
                                    () => Rating.ToString(GlobalSettings.InvariantCultureInfo));
                                int.TryParse(strLoopRating, NumberStyles.Any, GlobalSettings.InvariantCultureInfo, out intAddWeaponRating);
                            }

                            Weapon objGearWeapon = new Weapon(_objCharacter);
                            objGearWeapon.Create(objXmlWeapon, lstWeapons, true, blnAddImprovements,
                                                 blnSkipSelectForms, intAddWeaponRating);
                            objGearWeapon.ParentID = InternalId;
                            objGearWeapon.Cost = "0";
                            if (Guid.TryParse(objGearWeapon.InternalId, out _guiWeaponID))
                                lstWeapons.Add(objGearWeapon);
                            else
                                _guiWeaponID = Guid.Empty;
                        }
                    }
                }
            }

            // If the item grants a bonus, pass the information to the Improvement Manager.
            if (Bonus != null && !blnSkipSelectForms)
            {
                // Do not apply the Improvements if this is a Focus, unless we're specifically creating a Weapon Focus. This is to avoid creating the Foci's Improvements twice (once when it's first added
                // to the character which is incorrect, and once when the Focus is actually Bonded).
                bool blnApply = !((_strCategory == "Foci" || _strCategory == "Metamagic Foci") &&
                                  !_nodBonus.InnerXml.Contains("selectweapon"));

                if (blnApply)
                {
                    string strSource = _guiID.ToString("D", GlobalSettings.InvariantCultureInfo);
                    ImprovementManager.ForcedValue = _strForcedValue;
                    if (!ImprovementManager.CreateImprovements(_objCharacter, Improvement.ImprovementSource.Gear,
                        strSource, Bonus, intRating, CurrentDisplayNameShort, blnAddImprovements))
                    {
                        _guiID = Guid.Empty;
                        return;
                    }

                    if (!string.IsNullOrEmpty(ImprovementManager.SelectedValue))
                    {
                        _strExtra = ImprovementManager.SelectedValue;
                    }
                }
            }
            else if (!string.IsNullOrEmpty(_strForcedValue) && string.IsNullOrEmpty(_strExtra))
            {
                _strExtra = _strForcedValue;
            }

            // Add the Copy Protection and Registration plugins to the Matrix program. This does not apply if Unwired is not enabled, Hacked is selected, or this is a Suite being added (individual programs will add it to themselves).
            if (blnCreateChildren)
            {
                // Check to see if there are any child elements.
                CreateChildren(objXmlGear, blnAddImprovements, blnSkipSelectForms);
            }

            // If the item grants a Weapon bonus (Ammunition), just fill the WeaponBonus XmlNode.
            _nodWeaponBonus = objXmlGear["weaponbonus"];
            _nodFlechetteWeaponBonus = objXmlGear["flechetteweaponbonus"];

            if (!objXmlGear.TryGetStringFieldQuickly("attributearray", ref _strAttributeArray))
            {
                objXmlGear.TryGetStringFieldQuickly("attack", ref _strAttack);
                objXmlGear.TryGetStringFieldQuickly("sleaze", ref _strSleaze);
                objXmlGear.TryGetStringFieldQuickly("dataprocessing", ref _strDataProcessing);
                objXmlGear.TryGetStringFieldQuickly("firewall", ref _strFirewall);
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

            objXmlGear.TryGetStringFieldQuickly("modattack", ref _strModAttack);
            objXmlGear.TryGetStringFieldQuickly("modsleaze", ref _strModSleaze);
            objXmlGear.TryGetStringFieldQuickly("moddataprocessing", ref _strModDataProcessing);
            objXmlGear.TryGetStringFieldQuickly("modfirewall", ref _strModFirewall);
            objXmlGear.TryGetStringFieldQuickly("modattributearray", ref _strModAttributeArray);

            objXmlGear.TryGetStringFieldQuickly("programs", ref _strProgramLimit);

            if (blnAddImprovements && !blnSkipSelectForms)
                RefreshWirelessBonuses();
        }

        private SourceString _objCachedSourceDetail;

        public SourceString SourceDetail
        {
            get
            {
                if (_objCachedSourceDetail == default)
                    _objCachedSourceDetail = SourceString.GetSourceString(Source, DisplayPage(GlobalSettings.Language),
                        GlobalSettings.Language, GlobalSettings.CultureInfo, _objCharacter);
                return _objCachedSourceDetail;
            }
        }

        private void CreateChildren(XmlNode xmlParentGearNode, bool blnAddImprovements, bool blnSkipSelectForms)
        {
            XmlNode objGearsNode = xmlParentGearNode?["gears"];
            if (objGearsNode != null)
            {
                // Create Gear by looking up the name of the item we're provided with.
                using (XmlNodeList xmlUseGearList = objGearsNode.SelectNodes("usegear"))
                {
                    if (xmlUseGearList?.Count > 0)
                    {
                        foreach (XmlNode objXmlChild in xmlUseGearList)
                        {
                            CreateChild(objXmlChild, blnAddImprovements, blnSkipSelectForms);
                        }
                    }
                }

                // Create Gear by choosing from pre-determined lists.
                using (XmlNodeList xmlChooseGearList = objGearsNode.SelectNodes("choosegear"))
                {
                    if (xmlChooseGearList?.Count > 0)
                    {
                        XmlDocument xmlDocument = xmlParentGearNode.OwnerDocument ?? _objCharacter.LoadData("gear.xml");
                        bool blnCancelledDialog = false;
                        List<XmlNode> lstChildrenToCreate = new List<XmlNode>(xmlChooseGearList.Count);
                        foreach (XmlNode objXmlChooseGearNode in xmlChooseGearList)
                        {
                            // Each list is processed on its own and has usegear members
                            XmlNodeList objXmlNodeList = objXmlChooseGearNode.SelectNodes("usegear");
                            if (objXmlNodeList == null)
                                continue;
                            using (new FetchSafelyFromPool<List<ListItem>>(
                                       Utils.ListItemListPool, out List<ListItem> lstGears))
                            {
                                foreach (XmlNode objChoiceNode in objXmlNodeList)
                                {
                                    XmlNode xmlChoiceName = objChoiceNode["name"];
                                    XmlNode xmlChoiceCategory = objChoiceNode["category"];
                                    string strFilter = "/chummer/gears/gear";
                                    if (xmlChoiceName != null || xmlChoiceCategory != null)
                                    {
                                        strFilter += '[';
                                        if (xmlChoiceName != null)
                                        {
                                            strFilter += "name = " + xmlChoiceName.InnerText.CleanXPath();
                                            if (xmlChoiceCategory != null)
                                                strFilter += " and category = "
                                                             + xmlChoiceCategory.InnerText.CleanXPath();
                                        }
                                        else
                                            strFilter += "category = " + xmlChoiceCategory.InnerText.CleanXPath();

                                        strFilter += ']';
                                    }

                                    XmlNode objXmlLoopGear = xmlDocument.SelectSingleNode(strFilter);
                                    if (objXmlLoopGear == null)
                                        continue;
                                    XPathNavigator xmlTestNode = objXmlLoopGear.SelectSingleNodeAndCacheExpressionAsNavigator("forbidden/geardetails");
                                    if (xmlTestNode != null &&
                                        xmlParentGearNode.ProcessFilterOperationNode(xmlTestNode, false))
                                    {
                                        // Assumes topmost parent is an AND node
                                        continue;
                                    }

                                    xmlTestNode = objXmlLoopGear.SelectSingleNodeAndCacheExpressionAsNavigator("required/geardetails");
                                    if (xmlTestNode != null &&
                                        !xmlParentGearNode.ProcessFilterOperationNode(xmlTestNode, false))
                                    {
                                        // Assumes topmost parent is an AND node
                                        continue;
                                    }

                                    string strName = objChoiceNode["name"]?.InnerText ?? string.Empty;
                                    string strDisplayName = LanguageManager.GetString(strName, false);
                                    if (string.IsNullOrEmpty(strDisplayName))
                                        strDisplayName = _objCharacter.TranslateExtra(strName);
                                    lstGears.Add(new ListItem(strName, strDisplayName));
                                }

                                if (lstGears.Count == 0)
                                {
                                    if (objXmlChooseGearNode["required"]?.InnerText == bool.TrueString)
                                    {
                                        blnCancelledDialog = true;
                                        break;
                                    }

                                    continue;
                                }

                                string strChooseGearNodeName = objXmlChooseGearNode["name"]?.InnerText ?? string.Empty;
                                string strFriendlyName = LanguageManager.GetString(strChooseGearNodeName, false);
                                if (string.IsNullOrEmpty(strFriendlyName))
                                    strFriendlyName = _objCharacter.TranslateExtra(strChooseGearNodeName);
                                using (ThreadSafeForm<SelectItem> frmPickItem = ThreadSafeForm<SelectItem>.Get(
                                           () => new SelectItem
                                           {
                                               Description = string.Format(GlobalSettings.CultureInfo,
                                                                           LanguageManager.GetString(
                                                                               "String_Improvement_SelectText"),
                                                                           strFriendlyName)
                                           }))
                                {
                                    frmPickItem.MyForm.SetGeneralItemsMode(lstGears);

                                    // Make sure the dialogue window was not canceled.
                                    if (frmPickItem.ShowDialogSafe(_objCharacter) == DialogResult.Cancel)
                                    {
                                        if (objXmlChooseGearNode["required"]?.InnerText == bool.TrueString)
                                        {
                                            blnCancelledDialog = true;
                                            break;
                                        }

                                        continue;
                                    }

                                    XmlNode objXmlChosenGear =
                                        objXmlChooseGearNode.TryGetNodeByNameOrId("usegear", frmPickItem.MyForm.SelectedItem);

                                    if (objXmlChosenGear == null)
                                    {
                                        if (objXmlChooseGearNode["required"]?.InnerText == bool.TrueString)
                                        {
                                            blnCancelledDialog = true;
                                            break;
                                        }

                                        continue;
                                    }

                                    lstChildrenToCreate.Add(objXmlChosenGear);
                                }
                            }
                        }

                        if (!blnCancelledDialog)
                        {
                            foreach (XmlNode objXmlChild in lstChildrenToCreate)
                            {
                                CreateChild(objXmlChild, blnAddImprovements, blnSkipSelectForms);
                            }
                        }
                    }
                }
            }
        }

        private void CreateChild(XmlNode xmlChildNode, bool blnAddImprovements, bool blnSkipSelectForms)
        {
            if (xmlChildNode == null)
                return;
            XmlNode xmlChildName = xmlChildNode["name"];
            XmlNode xmlChildCategory = xmlChildNode["category"];
            string strFilter = "/chummer/gears/gear";
            if (xmlChildName != null || xmlChildCategory != null)
            {
                strFilter += '[';
                if (xmlChildName != null)
                {
                    strFilter += "name = " + xmlChildName.InnerText.CleanXPath();
                    if (xmlChildCategory != null)
                        strFilter += " and category = " + xmlChildCategory.InnerText.CleanXPath();
                }
                else
                    strFilter += "category = " + xmlChildCategory.InnerText.CleanXPath();
                strFilter += ']';
            }
            XmlDocument xmlDocument = xmlChildNode.OwnerDocument ?? _objCharacter.LoadData("gear.xml");
            XmlNode xmlChildDataNode = xmlDocument.SelectSingleNode(strFilter);
            if (xmlChildDataNode == null)
                return;
            int intChildRating = 0;
            xmlChildNode.TryGetInt32FieldQuickly("rating", ref intChildRating);
            decimal decChildQty = 1;
            string strChildForceSource = xmlChildNode["source"]?.InnerText ?? string.Empty;
            string strChildForcePage = xmlChildNode["page"]?.InnerText ?? string.Empty;
            XmlAttributeCollection xmlChildNameAttributes = xmlChildName?.Attributes;
            string strChildForceValue = xmlChildNameAttributes?["select"]?.InnerText ?? string.Empty;
            bool blnCreateChildren = xmlChildNameAttributes?["createchildren"]?.InnerText != bool.FalseString;
            bool blnAddChildImprovements = xmlChildNameAttributes?["addimprovements"]?.InnerText != bool.FalseString &&
                                           blnAddImprovements;
            if (xmlChildNameAttributes?["qty"] != null)
                decChildQty = Convert.ToDecimal(xmlChildNameAttributes["qty"].InnerText,
                    GlobalSettings.InvariantCultureInfo);

            Gear objChild = new Gear(_objCharacter);
            List<Weapon> lstChildWeapons = new List<Weapon>(1);
            objChild.Create(xmlChildDataNode, intChildRating, lstChildWeapons, strChildForceValue,
                blnAddChildImprovements, blnCreateChildren, blnSkipSelectForms);
            objChild.Quantity = decChildQty;
            objChild.Cost = "0";
            objChild.ParentID = InternalId;
            if (!string.IsNullOrEmpty(strChildForceSource))
                objChild.Source = strChildForceSource;
            if (!string.IsNullOrEmpty(strChildForcePage))
                objChild.Page = strChildForcePage;
            Children.Add(objChild);
            this.RefreshMatrixAttributeArray(_objCharacter);

            // Change the Capacity of the child if necessary.
            if (xmlChildNode["capacity"] != null)
                objChild.Capacity = xmlChildNode["capacity"].InnerText;

            objChild.CreateChildren(xmlChildNode, blnAddChildImprovements, blnSkipSelectForms);
        }

        /// <summary>
        /// Create a gear from an XmlNode attached to another object type.
        /// </summary>
        /// <param name="xmlGearsDocument">XmlDocument containing information about all possible gear items.</param>
        /// <param name="xmlGearNode">XmlNode containing information about the child gear that needs to be created.</param>
        /// <param name="lstWeapons">List of weapons that this (and other children) gear creates.</param>
        /// <param name="blnAddImprovements">Whether to create improvements for the gear or not (for Selection Windows, set to False).</param>
        /// <param name="blnSkipSelectForms">Whether or not to skip forms that are created for bonuses. Use only when creating Gear for previews in selection forms.</param>
        public bool CreateFromNode(XmlDocument xmlGearsDocument, XmlNode xmlGearNode, ICollection<Weapon> lstWeapons,
            bool blnAddImprovements = true, bool blnSkipSelectForms = false)
        {
            if (xmlGearsDocument == null)
                throw new ArgumentNullException(nameof(xmlGearsDocument));
            if (xmlGearNode == null)
                throw new ArgumentNullException(nameof(xmlGearNode));
            XmlNode xmlGearDataNode;
            List<Gear> lstChildGears = new List<Gear>(1);
            XmlAttributeCollection lstGearAttributes = xmlGearNode.Attributes;
            if (!int.TryParse(lstGearAttributes?["rating"]?.InnerText, NumberStyles.Any,
                    GlobalSettings.InvariantCultureInfo, out int intRating))
            {
                int.TryParse(xmlGearNode["rating"]?.InnerText, NumberStyles.Any,
                    GlobalSettings.InvariantCultureInfo, out intRating);
            }
            string strName = xmlGearNode["id"]?.InnerText ?? xmlGearNode["name"]?.InnerText;
            if (!string.IsNullOrEmpty(strName))
            {
                xmlGearDataNode = xmlGearsDocument.TryGetNodeByNameOrId("/chummer/gears/gear", strName);
                XmlNodeList xmlInnerGears = xmlGearNode.SelectNodes("gears/gear");
                if (xmlInnerGears?.Count > 0)
                {
                    foreach (XmlNode xmlChildGearNode in xmlInnerGears)
                    {
                        Gear objChildGear = new Gear(_objCharacter);
                        if (objChildGear.CreateFromNode(xmlGearsDocument, xmlChildGearNode, lstWeapons,
                            blnAddImprovements, blnSkipSelectForms))
                        {
                            objChildGear.ParentID = InternalId;
                            objChildGear.Parent = this;
                            lstChildGears.Add(objChildGear);
                        }
                        else
                            Utils.BreakIfDebug();
                    }
                }
            }
            else
            {
                xmlGearDataNode =
                    xmlGearsDocument.TryGetNodeByNameOrId("/chummer/gears/gear", xmlGearNode.InnerText);
            }

            if (xmlGearDataNode != null)
            {
                bool blnConsumeCapacity = lstGearAttributes?["consumecapacity"]?.InnerText == bool.TrueString;

                string strForceValue = lstGearAttributes?["select"]?.InnerText ?? string.Empty;
                decimal decQty = Convert.ToDecimal(lstGearAttributes?["qty"]?.InnerText ?? "1",
                                                   GlobalSettings.InvariantCultureInfo);
                string strMaxRating = lstGearAttributes?["maxrating"]?.InnerText ?? string.Empty;
                Create(xmlGearDataNode, intRating, lstWeapons, strForceValue, blnAddImprovements, true, blnSkipSelectForms);

                // Change the Capacity of the child if necessary.
                if (xmlGearNode["capacity"] != null)
                    Capacity = xmlGearNode["capacity"].InnerText;

                if (!blnConsumeCapacity)
                {
                    string strOldCapacity = Capacity;
                    int intSlashIndex = strOldCapacity?.IndexOf("/[", StringComparison.Ordinal) ?? -1;
                    if (intSlashIndex == -1)
                        Capacity = "[0]";
                    else
                        Capacity = (strOldCapacity?.Substring(0, intSlashIndex) ?? "0") + "/[0]";
                    strOldCapacity = ArmorCapacity;
                    intSlashIndex = strOldCapacity?.IndexOf("/[", StringComparison.Ordinal) ?? -1;
                    if (intSlashIndex == -1)
                        ArmorCapacity = "[0]";
                    else
                        ArmorCapacity = (strOldCapacity?.Substring(0, intSlashIndex) ?? "0") + "/[0]";
                }

                Cost = "0";
                Quantity = decQty;
                if (!string.IsNullOrEmpty(strMaxRating))
                    MaxRating = strMaxRating;

                foreach (Gear objGearChild in lstChildGears)
                {
                    objGearChild.ParentID = InternalId;
                    Children.Add(objGearChild);
                }
            }
            else
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Copy a piece of Gear.
        /// </summary>
        /// <param name="objGear">Gear object to copy.</param>
        public void Copy(Gear objGear)
        {
            if (objGear == null)
                return;
            _objCachedMyXmlNode = objGear.GetNode();
            _objCachedMyXPathNode = objGear.GetNodeXPath();
            SourceID = objGear.SourceID;
            _blnAllowRename = objGear.AllowRename;
            _strName = objGear.Name;
            _strCategory = objGear.Category;
            _strMaxRating = objGear.MaxRating;
            _strMinRating = objGear.MinRating;
            Rating = objGear.Rating;
            _decQty = objGear.Quantity;
            _strCapacity = objGear.Capacity;
            _strArmorCapacity = objGear.ArmorCapacity;
            _strAvail = objGear.Avail;
            _decCostFor = objGear.CostFor;
            _strDeviceRating = objGear.DeviceRating;
            _strCost = objGear.Cost;
            _strWeight = objGear.Weight;
            _strSource = objGear.Source;
            _strPage = objGear.Page;
            _strCanFormPersona = objGear.CanFormPersona;
            _strAmmoForWeaponType = objGear.AmmoForWeaponType;
            _strExtra = objGear.Extra;
            _blnBonded = objGear.Bonded;
            _blnEquipped = objGear.Equipped;
            _blnWirelessOn = objGear.WirelessOn;
            _nodBonus = objGear.Bonus;
            _nodWirelessBonus = objGear.WirelessBonus;
            _nodWeaponBonus = objGear.WeaponBonus;
            _nodFlechetteWeaponBonus = objGear.FlechetteWeaponBonus;
            if (!Guid.TryParse(objGear.WeaponID, out _guiWeaponID))
                _guiWeaponID = Guid.Empty;
            _strNotes = objGear.Notes;
            _objLocation = objGear.Location;
            _intChildAvailModifier = objGear.ChildAvailModifier;
            _intChildCostMultiplier = objGear.ChildCostMultiplier;
            _strGearName = objGear.GearName;
            _strForcedValue = objGear._strForcedValue;
            _blnIsFlechetteAmmo = objGear._blnIsFlechetteAmmo;
            _objLoadedIntoClip = null;

            foreach (Gear objGearChild in objGear.Children)
            {
                Gear objChild = new Gear(_objCharacter);
                objChild.Copy(objGearChild);
                _lstChildren.Add(objChild);
            }

            _strOverclocked = objGear.Overclocked;
            _strAttack = objGear.Attack;
            _strSleaze = objGear.Sleaze;
            _strDataProcessing = objGear.DataProcessing;
            _strFirewall = objGear.Firewall;
            _strAttributeArray = objGear.AttributeArray;
            _strModAttack = objGear.ModAttack;
            _strModSleaze = objGear.ModSleaze;
            _strModDataProcessing = objGear.ModDataProcessing;
            _strModFirewall = objGear.ModFirewall;
            _strModAttributeArray = objGear.ModAttributeArray;
        }

        /// <summary>
        /// Save the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        public void Save(XmlWriter objWriter)
        {
            if (objWriter == null)
                return;
            objWriter.WriteStartElement("gear");

            objWriter.WriteElementString("sourceid", SourceIDString);
            objWriter.WriteElementString("guid", InternalId);
            objWriter.WriteElementString("name", _strName);
            objWriter.WriteElementString("category", _strCategory);
            objWriter.WriteElementString("capacity", _strCapacity);
            objWriter.WriteElementString("armorcapacity", _strArmorCapacity);
            objWriter.WriteElementString("minrating", _strMinRating);
            objWriter.WriteElementString("maxrating", _strMaxRating);
            objWriter.WriteElementString("rating", Rating.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("qty", _decQty.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("avail", _strAvail);
            if (_decCostFor > 1)
                objWriter.WriteElementString("costfor", _decCostFor.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("cost", _strCost);
            objWriter.WriteElementString("weight", _strWeight);
            objWriter.WriteElementString("extra", _strExtra);
            objWriter.WriteElementString("bonded", _blnBonded.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("equipped", _blnEquipped.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("wirelesson", _blnWirelessOn.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("stolen", _blnStolen.ToString(GlobalSettings.InvariantCultureInfo));
            if (_guiWeaponID != Guid.Empty)
                objWriter.WriteElementString("weaponguid",
                    _guiWeaponID.ToString("D", GlobalSettings.InvariantCultureInfo));
            if (!string.IsNullOrEmpty(_nodBonus?.InnerXml))
                objWriter.WriteRaw("<bonus>" + _nodBonus.InnerXml + "</bonus>");
            else
                objWriter.WriteElementString("bonus", string.Empty);
            if (!string.IsNullOrEmpty(_nodWirelessBonus?.InnerXml))
                objWriter.WriteRaw("<wirelessbonus>" + _nodWirelessBonus.InnerXml + "</wirelessbonus>");
            else
                objWriter.WriteElementString("wirelessbonus", string.Empty);
            if (!string.IsNullOrEmpty(_nodWeaponBonus?.InnerXml))
                objWriter.WriteRaw("<weaponbonus>" + _nodWeaponBonus.InnerXml + "</weaponbonus>");
            else
                objWriter.WriteElementString("weaponbonus", string.Empty);
            if (!string.IsNullOrEmpty(_nodFlechetteWeaponBonus?.InnerXml))
                objWriter.WriteRaw("<flechetteweaponbonus>" + _nodFlechetteWeaponBonus.InnerXml +
                                   "</flechetteweaponbonus>");
            else
                objWriter.WriteElementString("flechetteweaponbonus", string.Empty);
            objWriter.WriteElementString("source", _strSource);
            objWriter.WriteElementString("page", _strPage);
            objWriter.WriteElementString("isflechetteammo",
                _blnIsFlechetteAmmo.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("ammoforweapontype", _strAmmoForWeaponType);
            objWriter.WriteElementString("canformpersona", _strCanFormPersona);
            objWriter.WriteElementString("devicerating", _strDeviceRating);
            objWriter.WriteElementString("gearname", _strGearName);
            objWriter.WriteElementString("forcedvalue", _strForcedValue);
            objWriter.WriteElementString("matrixcmfilled",
                _intMatrixCMFilled.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("matrixcmbonus",
                _intMatrixCMBonus.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("parentid", _strParentID);
            objWriter.WriteElementString("allowrename", _blnAllowRename.ToString(GlobalSettings.InvariantCultureInfo));
            if (_intChildCostMultiplier != 1)
                objWriter.WriteElementString("childcostmultiplier",
                    _intChildCostMultiplier.ToString(GlobalSettings.InvariantCultureInfo));
            if (_intChildAvailModifier != 0)
                objWriter.WriteElementString("childavailmodifier",
                    _intChildAvailModifier.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteStartElement("children");
            foreach (Gear objGear in _lstChildren)
            {
                objGear.Save(objWriter);
            }

            objWriter.WriteEndElement();
            objWriter.WriteElementString("location", Location?.InternalId ?? string.Empty);
            objWriter.WriteElementString("notes", _strNotes.CleanOfInvalidUnicodeChars());
            objWriter.WriteElementString("notesColor", ColorTranslator.ToHtml(_colNotes));
            objWriter.WriteElementString("discountedcost",
                _blnDiscountCost.ToString(GlobalSettings.InvariantCultureInfo));

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
            objWriter.WriteElementString("canswapattributes",
                _blnCanSwapAttributes.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("active",
                this.IsActiveCommlink(_objCharacter).ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("homenode",
                this.IsHomeNode(_objCharacter).ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("sortorder", _intSortOrder.ToString(GlobalSettings.InvariantCultureInfo));

            objWriter.WriteEndElement();
        }

        /// <summary>
        /// Load the Gear from the XmlNode.
        /// </summary>
        /// <param name="objNode">XmlNode to load.</param>
        /// <param name="blnCopy">Whether or not we are loading a copy of an existing gear.</param>
        public void Load(XmlNode objNode, bool blnCopy = false)
        {
            if (blnCopy || !objNode.TryGetField("guid", Guid.TryParse, out _guiID))
            {
                _guiID = Guid.NewGuid();
            }

            objNode.TryGetStringFieldQuickly("name", ref _strName);
            objNode.TryGetStringFieldQuickly("category", ref _strCategory);
            _objCachedMyXmlNode = null;
            _objCachedMyXPathNode = null;
            Lazy<XmlNode> objMyNode = new Lazy<XmlNode>(() => this.GetNode());
            if (!objNode.TryGetGuidFieldQuickly("sourceid", ref _guiSourceID) &&
                !objNode.TryGetGuidFieldQuickly("id", ref _guiSourceID))
            {
                _guiSourceID = Guid.Empty;
            }

            objNode.TryGetInt32FieldQuickly("matrixcmfilled", ref _intMatrixCMFilled);
            objNode.TryGetInt32FieldQuickly("matrixcmbonus", ref _intMatrixCMBonus);
            objNode.TryGetStringFieldQuickly("capacity", ref _strCapacity);
            objNode.TryGetStringFieldQuickly("armorcapacity", ref _strArmorCapacity);
            objNode.TryGetStringFieldQuickly("minrating", ref _strMinRating);
            objNode.TryGetStringFieldQuickly("maxrating", ref _strMaxRating);
            if (_strMaxRating == "0")
                _strMaxRating = string.Empty;
            objNode.TryGetInt32FieldQuickly("rating", ref _intRating);
            objNode.TryGetDecFieldQuickly("qty", ref _decQty);
            objNode.TryGetStringFieldQuickly("avail", ref _strAvail);

            // Legacy shim
            if (string.IsNullOrEmpty(_strAvail) &&
                (objNode["avail3"] != null || objNode["avail6"] != null || objNode["avail10"] != null))
            {
                objMyNode.Value?.TryGetStringFieldQuickly("avail", ref _strAvail);
            }

            objNode.TryGetDecFieldQuickly("costfor", ref _decCostFor);
            objNode.TryGetStringFieldQuickly("cost", ref _strCost);
            // Legacy shim
            if (string.IsNullOrEmpty(_strCost) &&
                (objNode["cost3"] != null || objNode["cost6"] != null || objNode["cost10"] != null))
            {
                objMyNode.Value?.TryGetStringFieldQuickly("cost", ref _strCost);
            }
            if (!objNode.TryGetStringFieldQuickly("weight", ref _strWeight))
                objMyNode.Value?.TryGetStringFieldQuickly("weight", ref _strWeight);

            objNode.TryGetStringFieldQuickly("extra", ref _strExtra);
            if (_strExtra == "Hold-Outs")
                _strExtra = "Holdouts";
            objNode.TryGetBoolFieldQuickly("bonded", ref _blnBonded);
            objNode.TryGetBoolFieldQuickly("equipped", ref _blnEquipped);
            _nodBonus = objNode["bonus"];
            _nodWirelessBonus = objNode["wirelessbonus"];
            if (!objNode.TryGetBoolFieldQuickly("wirelesson", ref _blnWirelessOn))
                _blnWirelessOn = false;
            _nodWeaponBonus = objNode["weaponbonus"];
            _nodFlechetteWeaponBonus = objNode["flechetteweaponbonus"];
            objNode.TryGetStringFieldQuickly("source", ref _strSource);
            objNode.TryGetStringFieldQuickly("page", ref _strPage);
            objNode.TryGetBoolFieldQuickly("stolen", ref _blnStolen);
            if (!objNode.TryGetBoolFieldQuickly("isflechetteammmo", ref _blnIsFlechetteAmmo))
            {
                objMyNode.Value?.TryGetBoolFieldQuickly("isflechetteammmo", ref _blnIsFlechetteAmmo);
                if (_nodFlechetteWeaponBonus == null && _blnIsFlechetteAmmo)
                    _nodFlechetteWeaponBonus = objMyNode.Value?["flechetteweaponbonus"];
            }

            if (!objNode.TryGetStringFieldQuickly("ammoforweapontype", ref _strAmmoForWeaponType))
            {
                objMyNode.Value?.TryGetStringFieldQuickly("ammoforweapontype", ref _strAmmoForWeaponType);
            }

            bool blnNeedCommlinkLegacyShim =
                !objNode.TryGetStringFieldQuickly("canformpersona", ref _strCanFormPersona);
            if (!objNode.TryGetStringFieldQuickly("devicerating", ref _strDeviceRating))
                objMyNode.Value?.TryGetStringFieldQuickly("devicerating", ref _strDeviceRating);
            string strWeaponID = string.Empty;
            if (objNode.TryGetStringFieldQuickly("weaponguid", ref strWeaponID) &&
                !Guid.TryParse(strWeaponID, out _guiWeaponID))
            {
                _guiWeaponID = Guid.Empty;
            }

            objNode.TryGetInt32FieldQuickly("childcostmultiplier", ref _intChildCostMultiplier);
            objNode.TryGetInt32FieldQuickly("childavailmodifier", ref _intChildAvailModifier);

            objNode.TryGetStringFieldQuickly("gearname", ref _strGearName);
            objNode.TryGetStringFieldQuickly("forcedvalue", ref _strForcedValue);
            objNode.TryGetBoolFieldQuickly("allowrename", ref _blnAllowRename);
            if (!objNode.TryGetStringFieldQuickly("parentid", ref _strParentID))
            {
                // Legacy Shim
                bool blnIncludedInParent = false;
                if (objNode.TryGetBoolFieldQuickly("includedinparent", ref blnIncludedInParent) && blnIncludedInParent)
                {
                    // ParentIDs were only added when improvements were added that could allow for the adding of gear by something that would not become the gear's parent...
                    // ... so all we care about is that this string is not empty and does not match the internal IDs of any sources for adding gear via improvements.
                    _strParentID = Guid.NewGuid().ToString("D", GlobalSettings.InvariantCultureInfo);
                }
            }

            using (XmlNodeList nodChildren = objNode.SelectNodes("children/gear"))
                if (nodChildren != null)
                    foreach (XmlNode nodChild in nodChildren)
                    {
                        Gear objGear = new Gear(_objCharacter);
                        objGear.Load(nodChild, blnCopy);
                        objGear.Parent = this;
                        _lstChildren.Add(objGear);
                    }

            // Legacy Shim
            if (!string.IsNullOrEmpty(_strMaxRating) && _strName.Contains("Certified Credstick"))
            {
                XmlNode objNuyenNode = _objCharacter.LoadData("gear.xml")
                    .SelectSingleNode("/chummer/gears/gear[contains(name, \"Nuyen\") and category = \"Currency\"]");
                if (objNuyenNode != null)
                {
                    if (Rating > 0)
                    {
                        Gear objNuyenGear = new Gear(_objCharacter);
                        objNuyenGear.Create(objNuyenNode, 0, new List<Weapon>(1));
                        objNuyenGear.Quantity = Rating;
                        _lstChildren.Add(objNuyenGear);
                    }

                    objMyNode.Value?.TryGetStringFieldQuickly("rating", ref _strMaxRating);
                    if (_strMaxRating == "0")
                        _strMaxRating = string.Empty;
                    objMyNode.Value?.TryGetStringFieldQuickly("minrating", ref _strMinRating);
                    Rating = 0;
                    objMyNode.Value?.TryGetStringFieldQuickly("capacity", ref _strCapacity);
                }
            }

            string strLocation = objNode["location"]?.InnerText;
            if (!string.IsNullOrEmpty(strLocation))
            {
                if (Guid.TryParse(strLocation, out Guid temp))
                {
                    // Location is an object. Look for it based on the InternalId. Requires that locations have been loaded already!
                    _objLocation =
                        CharacterObject.GearLocations.FirstOrDefault(location =>
                            location.InternalId == temp.ToString());
                }
                else
                {
                    //Legacy. Location is a string.
                    _objLocation =
                        CharacterObject.GearLocations.FirstOrDefault(location =>
                            location.Name == strLocation);
                }

                _objLocation?.Children.Add(this);
            }

            objNode.TryGetStringFieldQuickly("notes", ref _strNotes);

            string sNotesColor = ColorTranslator.ToHtml(ColorManager.HasNotesColor);
            objNode.TryGetStringFieldQuickly("notesColor", ref sNotesColor);
            _colNotes = ColorTranslator.FromHtml(sNotesColor);

            objNode.TryGetBoolFieldQuickly("discountedcost", ref _blnDiscountCost);
            objNode.TryGetInt32FieldQuickly("sortorder", ref _intSortOrder);

            // Convert old qi foci to the new bonus. In order to force the user to update their powers, unequip the focus and remove all improvements.
            if (_strName == "Qi Focus")
            {
                int intResult = _objCharacter.LastSavedVersion.CompareTo(new Version(5, 193, 5));
                if (intResult == -1)
                {
                    XmlNode gear = _objCharacter.LoadData("gear.xml")
                        .TryGetNodeByNameOrId("/chummer/gears/gear", _strName);
                    if (gear != null)
                    {
                        Equipped = false;
                        ImprovementManager.RemoveImprovements(_objCharacter, Improvement.ImprovementSource.Gear,
                            InternalId);
                        Bonus = gear["bonus"];
                        WirelessBonus = gear["wirelessbonus"];
                    }
                }
            }

            if (blnCopy)
            {
                _objLocation = null;

                if (Bonus != null || WirelessBonus != null)
                {
                    // If this is a Focus which is not bonded, don't do anything.
                    if (Category != "Stacked Focus")
                    {
                        bool blnAddImprovement = true;
                        if (Category.EndsWith("Foci", StringComparison.Ordinal))
                            blnAddImprovement = Bonded;

                        if (blnAddImprovement)
                        {
                            if (!string.IsNullOrEmpty(Extra))
                                ImprovementManager.ForcedValue = Extra;
                            if (Bonus != null && ImprovementManager.CreateImprovements(_objCharacter, Improvement.ImprovementSource.Gear,
                                    InternalId, Bonus, Rating, CurrentDisplayNameShort))
                            {
                                Extra = ImprovementManager.SelectedValue;
                            }

                            if (WirelessOn && WirelessBonus != null)
                            {
                                ImprovementManager.CreateImprovements(_objCharacter, Improvement.ImprovementSource.Gear,
                                    InternalId, WirelessBonus, Rating, CurrentDisplayNameShort);
                            }
                        }
                    }
                    else
                    {
                        // Stacked Foci need to be handled a little differently.
                        _objCharacter.StackedFoci.ForEach(objStack =>
                        {
                            if (objStack.GearId != InternalId || !objStack.Bonded)
                                return;
                            objStack.Gear.ForEach(objFociGear =>
                            {
                                if (!string.IsNullOrEmpty(objFociGear.Extra))
                                    ImprovementManager.ForcedValue = objFociGear.Extra;
                                if (objFociGear.Bonus != null && ImprovementManager.CreateImprovements(_objCharacter,
                                        Improvement.ImprovementSource.StackedFocus, objStack.InternalId,
                                        objFociGear.Bonus, objFociGear.Rating,
                                        objFociGear.CurrentDisplayNameShort))
                                {
                                    objFociGear.Extra = ImprovementManager.SelectedValue;
                                }

                                if (objFociGear.WirelessOn && objFociGear.WirelessBonus != null)
                                {
                                    ImprovementManager.CreateImprovements(_objCharacter,
                                                                          Improvement.ImprovementSource.StackedFocus,
                                                                          objStack.InternalId,
                                                                          objFociGear.WirelessBonus, Rating,
                                                                          objFociGear.CurrentDisplayNameShort);
                                }
                            });
                        });
                    }
                }

                if (!Equipped)
                    ChangeEquippedStatus(false);
            }
            else if (!Equipped && (Bonus != null || WirelessBonus != null) && !_objCharacter.Improvements.Any(x =>
                x.ImproveSource == Improvement.ImprovementSource.Gear && x.SourceName == InternalId))
            {
                // If this is a Focus which is not bonded, don't do anything.
                if (Category != "Stacked Focus")
                {
                    bool blnAddImprovement = true;
                    if (Category.EndsWith("Foci", StringComparison.Ordinal))
                        blnAddImprovement = Bonded;

                    if (blnAddImprovement)
                    {
                        if (!string.IsNullOrEmpty(Extra))
                            ImprovementManager.ForcedValue = Extra;
                        if (Bonus != null && ImprovementManager.CreateImprovements(_objCharacter, Improvement.ImprovementSource.Gear,
                                InternalId, Bonus, Rating, CurrentDisplayNameShort))
                        {
                            Extra = ImprovementManager.SelectedValue;
                        }

                        if (WirelessOn && WirelessBonus != null)
                        {
                            ImprovementManager.CreateImprovements(_objCharacter, Improvement.ImprovementSource.Gear,
                                InternalId, WirelessBonus, Rating, CurrentDisplayNameShort);
                        }
                    }
                }
                else
                {
                    // Stacked Foci need to be handled a little differently.
                    _objCharacter.StackedFoci.ForEach(objStack =>
                    {
                        if (objStack.GearId != InternalId || !objStack.Bonded)
                            return;
                        objStack.Gear.ForEach(objFociGear =>
                        {
                            if (!string.IsNullOrEmpty(objFociGear.Extra))
                                ImprovementManager.ForcedValue = objFociGear.Extra;
                            if (objFociGear.Bonus != null && ImprovementManager.CreateImprovements(_objCharacter,
                                    Improvement.ImprovementSource
                                               .StackedFocus, objStack.InternalId,
                                    objFociGear.Bonus, objFociGear.Rating,
                                    objFociGear.CurrentDisplayNameShort))
                            {
                                objFociGear.Extra = ImprovementManager.SelectedValue;
                            }

                            if (objFociGear.WirelessOn && objFociGear.WirelessBonus != null)
                            {
                                ImprovementManager.CreateImprovements(_objCharacter,
                                                                      Improvement.ImprovementSource.StackedFocus,
                                                                      objStack.InternalId,
                                                                      objFociGear.WirelessBonus, Rating,
                                                                      objFociGear.CurrentDisplayNameShort);
                            }
                        });
                    });
                }

                ChangeEquippedStatus(false);
            }

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

            if (!objNode.TryGetBoolFieldQuickly("canswapattributes", ref _blnCanSwapAttributes) &&
                Category == "Cyberdecks")
            {
                // Legacy shim
                _blnCanSwapAttributes = Name != "MCT Trainee" && Name != "C-K Analyst" &&
                                        Name != "Aztechnology Emissary" &&
                                        Name != "Yak Killer" && Name != "Ring of Light Special" &&
                                        Name != "Ares Echo Unlimited";
            }

            if (blnNeedCommlinkLegacyShim)
            {
                if (_strDeviceRating == "0")
                {
                    _strModAttack = _strAttack;
                    _strModSleaze = _strSleaze;
                    _strModDataProcessing = _strDataProcessing;
                    _strModFirewall = _strFirewall;
                    if (objMyNode.Value != null)
                    {
                        _strAttack = string.Empty;
                        objMyNode.Value.TryGetStringFieldQuickly("attack", ref _strAttack);
                        _strSleaze = string.Empty;
                        objMyNode.Value.TryGetStringFieldQuickly("sleaze", ref _strSleaze);
                        _strDataProcessing = string.Empty;
                        objMyNode.Value.TryGetStringFieldQuickly("dataprocessing", ref _strDataProcessing);
                        _strFirewall = string.Empty;
                        objMyNode.Value.TryGetStringFieldQuickly("firewall", ref _strFirewall);
                    }
                }

                objMyNode.Value?.TryGetStringFieldQuickly("canformpersona", ref _strCanFormPersona);
                bool blnIsCommlinkLegacy = false;
                objNode.TryGetBoolFieldQuickly("iscommlink", ref blnIsCommlinkLegacy);
                // This is Commlink Functionality, which originally had Persona Firmware that would now make the Commlink Functionality item count as a commlink
                if (blnIsCommlinkLegacy != IsCommlink)
                {
                    for (int i = Children.Count - 1; i >= 0; --i)
                    {
                        Gear objLoopChild = Children[i];
                        if (objLoopChild.ParentID == InternalId && objLoopChild.CanFormPersona == "Parent")
                            Children.RemoveAt(i);
                    }
                }
            }
        }

        /// <summary>
        /// Print the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        /// <param name="objCulture">Culture in which to print.</param>
        /// <param name="strLanguageToPrint">Language in which to print</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public async Task Print(XmlWriter objWriter, CultureInfo objCulture, string strLanguageToPrint, CancellationToken token = default)
        {
            if (objWriter == null)
                return;

            // <gear>
            XmlElementWriteHelper objBaseElement = await objWriter.StartElementAsync("gear", token).ConfigureAwait(false);
            try
            {
                await objWriter.WriteElementStringAsync("guid", InternalId, token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("sourceid", SourceIDString, token).ConfigureAwait(false);
                if ((Category == "Foci" || Category == "Metamagic Foci") && Bonded)
                    await objWriter.WriteElementStringAsync(
                                       "name",
                                       await DisplayNameShortAsync(strLanguageToPrint, token).ConfigureAwait(false)
                                       + await LanguageManager
                                               .GetStringAsync("String_Space", strLanguageToPrint, token: token)
                                               .ConfigureAwait(false) + await LanguageManager
                                                                              .GetStringAsync(
                                                                                  "Label_BondedFoci",
                                                                                  strLanguageToPrint, token: token)
                                                                              .ConfigureAwait(false), token)
                                   .ConfigureAwait(false);
                else
                    await objWriter
                          .WriteElementStringAsync(
                              "name", await DisplayNameShortAsync(strLanguageToPrint, token).ConfigureAwait(false),
                              token).ConfigureAwait(false);

                await objWriter.WriteElementStringAsync("name_english", Name, token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("category", DisplayCategory(strLanguageToPrint), token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("category_english", Category, token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("ispersona",
                                                        (Name == "Living Persona").ToString(GlobalSettings.InvariantCultureInfo), token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("isammo",
                                                        (Category == "Ammunition" || !string.IsNullOrEmpty(AmmoForWeaponType)).ToString(GlobalSettings
                                                            .InvariantCultureInfo), token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("issin",
                                                        (Name == "Fake SIN" || Name == "Credstick, Fake (2050)" || Name == "Fake SIN").ToString(GlobalSettings
                                                            .InvariantCultureInfo), token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("capacity", Capacity, token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("armorcapacity", ArmorCapacity, token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("maxrating", MaxRating, token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("rating", Rating.ToString(objCulture), token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("qty", DisplayQuantity(objCulture), token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("avail", await TotalAvailAsync(objCulture, strLanguageToPrint, token).ConfigureAwait(false), token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("avail_english",
                                                        await TotalAvailAsync(GlobalSettings.InvariantCultureInfo, GlobalSettings.DefaultLanguage, token).ConfigureAwait(false), token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("cost", (await GetTotalCostAsync(token).ConfigureAwait(false)).ToString(_objCharacter.Settings.NuyenFormat, objCulture), token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("owncost", (await GetOwnCostAsync(token).ConfigureAwait(false)).ToString(_objCharacter.Settings.NuyenFormat, objCulture), token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("weight", TotalWeight.ToString(_objCharacter.Settings.WeightFormat, objCulture), token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("ownweight", OwnWeight.ToString(_objCharacter.Settings.WeightFormat, objCulture), token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("extra", await _objCharacter.TranslateExtraAsync(Extra, strLanguageToPrint, token: token).ConfigureAwait(false), token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("bonded", Bonded.ToString(GlobalSettings.InvariantCultureInfo), token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("equipped", Equipped.ToString(GlobalSettings.InvariantCultureInfo), token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("wirelesson", WirelessOn.ToString(GlobalSettings.InvariantCultureInfo), token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("location", Location != null ? await Location.DisplayNameAsync(strLanguageToPrint, token).ConfigureAwait(false) : string.Empty, token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("gearname", GearName, token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("source", await _objCharacter.LanguageBookShortAsync(Source, strLanguageToPrint, token).ConfigureAwait(false), token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("page", await DisplayPageAsync(strLanguageToPrint, token).ConfigureAwait(false), token).ConfigureAwait(false);

                await objWriter.WriteElementStringAsync("attack", (await this.GetTotalMatrixAttributeAsync("Attack", token).ConfigureAwait(false)).ToString(objCulture), token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("sleaze", (await this.GetTotalMatrixAttributeAsync("Sleaze", token).ConfigureAwait(false)).ToString(objCulture), token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("dataprocessing",
                                                        (await this.GetTotalMatrixAttributeAsync("Data Processing", token).ConfigureAwait(false)).ToString(objCulture), token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("firewall", (await this.GetTotalMatrixAttributeAsync("Firewall", token).ConfigureAwait(false)).ToString(objCulture), token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("devicerating",
                                                        (await this.GetTotalMatrixAttributeAsync("Device Rating", token).ConfigureAwait(false)).ToString(objCulture), token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("programlimit",
                                                        (await this.GetTotalMatrixAttributeAsync("Program Limit", token).ConfigureAwait(false)).ToString(objCulture), token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("iscommlink", IsCommlink.ToString(GlobalSettings.InvariantCultureInfo), token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("isprogram", IsProgram.ToString(GlobalSettings.InvariantCultureInfo), token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("active",
                                                        (await this.IsActiveCommlinkAsync(_objCharacter, token).ConfigureAwait(false)).ToString(GlobalSettings.InvariantCultureInfo), token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("homenode",
                                                        (await this.IsHomeNodeAsync(_objCharacter, token).ConfigureAwait(false)).ToString(GlobalSettings.InvariantCultureInfo), token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("conditionmonitor", MatrixCM.ToString(objCulture), token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("matrixcmfilled", MatrixCMFilled.ToString(objCulture), token).ConfigureAwait(false);

                // <children>
                XmlElementWriteHelper objChildrenElement = await objWriter.StartElementAsync("children", token).ConfigureAwait(false);
                try
                {
                    foreach (Gear objGear in Children)
                    {
                        await objGear.Print(objWriter, objCulture, strLanguageToPrint, token).ConfigureAwait(false);
                    }
                }
                finally
                {
                    // </children>
                    await objChildrenElement.DisposeAsync().ConfigureAwait(false);
                }

                await PrintWeaponBonusEntries(objWriter, objCulture, strLanguageToPrint, token: token).ConfigureAwait(false);

                if (GlobalSettings.PrintNotes)
                    await objWriter.WriteElementStringAsync("notes", Notes, token).ConfigureAwait(false);
            }
            finally
            {
                // </gear>
                await objBaseElement.DisposeAsync().ConfigureAwait(false);
            }
        }

        public async Task PrintWeaponBonusEntries(XmlWriter objWriter, CultureInfo objCulture, string strLanguageToPrint, bool blnForcePrintAllBlocks = false, CancellationToken token = default)
        {
            if (objWriter == null)
                return;

            if (WeaponBonus != null || blnForcePrintAllBlocks)
            {
                await objWriter.WriteElementStringAsync("weaponbonusdamage", await WeaponBonusDamageAsync(strLanguageToPrint, token).ConfigureAwait(false), token: token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("weaponbonusdamage_english",
                                                        await WeaponBonusDamageAsync(GlobalSettings.DefaultLanguage, token).ConfigureAwait(false), token: token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("weaponbonusap", WeaponBonusAP, token: token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("weaponbonusacc", WeaponBonusAcc, token: token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("weaponbonusrange", WeaponBonusRange.ToString(objCulture), token: token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("weaponbonuspool", WeaponBonusPool.ToString(objCulture), token: token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("weaponbonussmartlinkpool", WeaponBonusSmartlinkPool.ToString(objCulture), token: token).ConfigureAwait(false);
            }

            if (FlechetteWeaponBonus != null || blnForcePrintAllBlocks)
            {
                await objWriter.WriteElementStringAsync("flechetteweaponbonusdamage",
                                                        await FlechetteWeaponBonusDamageAsync(strLanguageToPrint, token).ConfigureAwait(false), token: token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("flechetteweaponbonusdamage_english",
                                                        await FlechetteWeaponBonusDamageAsync(GlobalSettings.DefaultLanguage, token).ConfigureAwait(false), token: token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("flechetteweaponbonusap", FlechetteWeaponBonusAP, token: token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("flechetteweaponbonusacc", FlechetteWeaponBonusAcc, token: token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("flechetteweaponbonusrange", FlechetteWeaponBonusRange.ToString(objCulture), token: token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("flechetteweaponbonuspool", FlechetteWeaponBonusPool.ToString(objCulture), token: token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("flechetteweaponbonussmartlinkpool", FlechetteWeaponBonusSmartlinkPool.ToString(objCulture), token: token).ConfigureAwait(false);
            }
        }

        #endregion Constructor, Create, Save, Load, and Print Methods

        #region Properties

        /// <summary>
        /// Guid of the object from the data. You probably want to use SourceIDString instead.
        /// </summary>
        public Guid SourceID
        {
            get => _guiSourceID;
            set => _guiSourceID = value;
        }

        /// <summary>
        /// String-formatted Guid of the <inheritdoc cref="SourceID"/> from the data files.
        /// </summary>
        public string SourceIDString => _guiSourceID.ToString("D", GlobalSettings.InvariantCultureInfo);

        /// <summary>
        /// Internal identifier which will be used to identify this piece of Gear in the Character.
        /// </summary>
        public string InternalId => _guiID.ToString("D", GlobalSettings.InvariantCultureInfo);

        /// <summary>
        /// Guid of a Cyberware Weapon.
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
        /// Bonus node from the XML file.
        /// </summary>
        public XmlNode Bonus
        {
            get => _nodBonus;
            set => _nodBonus = value;
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
        /// WeaponBonus node from the XML file.
        /// </summary>
        public XmlNode WeaponBonus
        {
            get => _nodWeaponBonus;
            set => _nodWeaponBonus = value;
        }

        /// <summary>
        /// WeaponBonus node from the XML file that is used only by weapons that have flechette codes built in.
        /// </summary>
        public XmlNode FlechetteWeaponBonus
        {
            get => _nodFlechetteWeaponBonus;
            set => _nodFlechetteWeaponBonus = value;
        }

        /// <summary>
        /// Character to which the gear is assigned.
        /// </summary>
        public Character CharacterObject => _objCharacter;

        /// <summary>
        /// Name.
        /// </summary>
        public string Name
        {
            get => _strName;
            set
            {
                if (Interlocked.Exchange(ref _strName, value) == value)
                    return;
                _objCachedMyXmlNode = null;
                _objCachedMyXPathNode = null;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// A custom name for the Gear assigned by the player.
        /// </summary>
        public string GearName
        {
            get => _strGearName;
            set
            {
                if (Interlocked.Exchange(ref _strGearName, value) != value)
                    OnPropertyChanged();
            }
        }

        /// <summary>
        /// Translated Category.
        /// </summary>
        public string DisplayCategory(string strLanguage)
        {
            if (strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                return Category;

            return _objCharacter.LoadDataXPath("gear.xml", strLanguage)
                                .SelectSingleNodeAndCacheExpression(
                                    "/chummer/categories/category[. = " + Category.CleanXPath() + "]/@translate")
                                ?.Value ?? Category;
        }

        /// <summary>
        /// Category.
        /// </summary>
        public string Category
        {
            get => _strCategory;
            set
            {
                if (Interlocked.Exchange(ref _strCategory, value) == value)
                    return;
                _objCachedMyXmlNode = null;
                _objCachedMyXPathNode = null;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gear capacity.
        /// </summary>
        public string Capacity
        {
            get => _strCapacity;
            set => _strCapacity = value;
        }

        /// <summary>
        /// Armor capacity.
        /// </summary>
        public string ArmorCapacity
        {
            get => _strArmorCapacity;
            set => _strArmorCapacity = value;
        }

        /// <summary>
        /// Minimum Rating (string form).
        /// </summary>
        public string MinRating
        {
            get => _strMinRating;
            set => _strMinRating = value;
        }

        /// <summary>
        /// Maximum Rating (string form).
        /// </summary>
        public string MaxRating
        {
            get => _strMaxRating;
            set => _strMaxRating = value;
        }

        /// <summary>
        /// Minimum Rating (value form).
        /// </summary>
        public int MinRatingValue
        {
            get
            {
                string strExpression = MinRating;
                return string.IsNullOrEmpty(strExpression) ? 0 : ProcessRatingString(strExpression);
            }
            set => MinRating = value.ToString(GlobalSettings.InvariantCultureInfo);
        }

        /// <summary>
        /// Maximum Rating (string form).
        /// </summary>
        public int MaxRatingValue
        {
            get
            {
                string strExpression = MaxRating;
                return string.IsNullOrEmpty(strExpression) ? int.MaxValue : ProcessRatingString(strExpression);
            }
            set => MaxRating = value.ToString(GlobalSettings.InvariantCultureInfo);
        }

        /// <summary>
        /// Processes a string into an int based on logical processing.
        /// </summary>
        private int ProcessRatingString(string strExpression)
        {
            if (strExpression.StartsWith("FixedValues(", StringComparison.Ordinal))
            {
                string[] strValues = strExpression.TrimStartOnce("FixedValues(", true).TrimEndOnce(')')
                    .Split(',', StringSplitOptions.RemoveEmptyEntries);
                strExpression = strValues[Math.Max(Math.Min(Rating, strValues.Length) - 1, 0)].Trim('[', ']');
            }

            if (strExpression.IndexOfAny('{', '+', '-', '*', ',') != -1 || strExpression.Contains("div"))
            {
                using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdValue))
                {
                    sbdValue.Append(strExpression);
                    sbdValue.Replace("{Rating}", Rating.ToString(GlobalSettings.InvariantCultureInfo));
                    sbdValue.CheapReplace(strExpression, "{Parent Rating}",
                                          () => (Parent as IHasRating)?.Rating.ToString(
                                                    GlobalSettings.InvariantCultureInfo) ??
                                                int.MaxValue.ToString(GlobalSettings.InvariantCultureInfo));
                    _objCharacter.AttributeSection.ProcessAttributesInXPath(sbdValue, strExpression);

                    foreach (string strMatrixAttribute in MatrixAttributes.MatrixAttributeStrings)
                    {
                        sbdValue.CheapReplace(strExpression, "{Gear " + strMatrixAttribute + '}',
                                              () => ((Parent as IHasMatrixAttributes)?.GetBaseMatrixAttribute(
                                                      strMatrixAttribute) ?? 0)
                                                  .ToString(
                                                      GlobalSettings.InvariantCultureInfo));
                        sbdValue.CheapReplace(strExpression, "{Parent " + strMatrixAttribute + '}',
                                              () => (Parent as IHasMatrixAttributes).GetMatrixAttributeString(
                                                  strMatrixAttribute) ?? "0");
                        if (Children.Count == 0 || !strExpression.Contains("{Children " + strMatrixAttribute + '}'))
                            continue;
                        int intTotalChildrenValue = Children.Sum(g => g.Equipped, loopGear =>
                                                                     loopGear.GetBaseMatrixAttribute(
                                                                         strMatrixAttribute));

                        sbdValue.Replace("{Children " + strMatrixAttribute + '}',
                                         intTotalChildrenValue.ToString(GlobalSettings.InvariantCultureInfo));
                    }

                    // This is first converted to a decimal and rounded up since some items have a multiplier that is not a whole number, such as 2.5.
                    (bool blnIsSuccess, object objProcess)
                        = CommonFunctions.EvaluateInvariantXPath(sbdValue.ToString());
                    return blnIsSuccess ? ((double)objProcess).StandardRound() : 0;
                }
            }

            int.TryParse(strExpression, NumberStyles.Any, GlobalSettings.InvariantCultureInfo, out int intReturn);

            return intReturn;
        }

        /// <summary>
        /// Processes a string into an int based on logical processing.
        /// </summary>
        private async Task<int> ProcessRatingStringAsync(string strExpression, CancellationToken token = default)
        {
            if (strExpression.StartsWith("FixedValues(", StringComparison.Ordinal))
            {
                string[] strValues = strExpression.TrimStartOnce("FixedValues(", true).TrimEndOnce(')')
                    .Split(',', StringSplitOptions.RemoveEmptyEntries);
                strExpression = strValues[Math.Max(Math.Min(Rating, strValues.Length) - 1, 0)].Trim('[', ']');
            }

            if (strExpression.IndexOfAny('{', '+', '-', '*', ',') != -1 || strExpression.Contains("div"))
            {
                using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdValue))
                {
                    sbdValue.Append(strExpression);
                    sbdValue.Replace("{Rating}", Rating.ToString(GlobalSettings.InvariantCultureInfo));
                    await sbdValue.CheapReplaceAsync(strExpression, "{Parent Rating}",
                        () => (Parent as IHasRating)?.Rating.ToString(
                                  GlobalSettings.InvariantCultureInfo) ??
                              int.MaxValue.ToString(GlobalSettings.InvariantCultureInfo), token: token).ConfigureAwait(false);
                    await _objCharacter.AttributeSection.ProcessAttributesInXPathAsync(sbdValue, strExpression, token: token).ConfigureAwait(false);

                    foreach (string strMatrixAttribute in MatrixAttributes.MatrixAttributeStrings)
                    {
                        await sbdValue.CheapReplaceAsync(strExpression, "{Gear " + strMatrixAttribute + '}',
                            () => ((Parent as IHasMatrixAttributes)?.GetBaseMatrixAttribute(
                                    strMatrixAttribute) ?? 0)
                                .ToString(
                                    GlobalSettings.InvariantCultureInfo), token: token).ConfigureAwait(false);
                        await sbdValue.CheapReplaceAsync(strExpression, "{Parent " + strMatrixAttribute + '}',
                            () => (Parent as IHasMatrixAttributes).GetMatrixAttributeString(
                                strMatrixAttribute) ?? "0", token: token).ConfigureAwait(false);
                        if (Children.Count == 0 || !strExpression.Contains("{Children " + strMatrixAttribute + '}'))
                            continue;
                        int intTotalChildrenValue = await Children.SumAsync(g => g.Equipped, loopGear =>
                            loopGear.GetBaseMatrixAttributeAsync(
                                strMatrixAttribute, token), token: token).ConfigureAwait(false);

                        sbdValue.Replace("{Children " + strMatrixAttribute + '}',
                                         intTotalChildrenValue.ToString(GlobalSettings.InvariantCultureInfo));
                    }

                    // This is first converted to a decimal and rounded up since some items have a multiplier that is not a whole number, such as 2.5.
                    (bool blnIsSuccess, object objProcess)
                        = await CommonFunctions.EvaluateInvariantXPathAsync(sbdValue.ToString(), token).ConfigureAwait(false);
                    return blnIsSuccess ? ((double)objProcess).StandardRound() : 0;
                }
            }

            int.TryParse(strExpression, NumberStyles.Any, GlobalSettings.InvariantCultureInfo, out int intReturn);

            return intReturn;
        }

        /// <summary>
        /// Rating.
        /// </summary>
        public int Rating
        {
            get => _intRating;
            set
            {
                value = Math.Max(Math.Min(value, MaxRatingValue), MinRatingValue);
                if (Interlocked.Exchange(ref _intRating, value) != value)
                {
                    if (Children.Count > 0)
                    {
                        foreach (Gear objChild in Children)
                        {
                            if (objChild.MaxRating.Contains("Parent") || objChild.MinRating.Contains("Parent"))
                            {
                                // This will update a child's rating if it would become out of bounds due to its parent's rating changing
                                int intCurrentRating = objChild.Rating;
                                objChild.Rating = intCurrentRating;
                            }
                        }
                    }

                    OnPropertyChanged();
                }
            }
        }

        public string RatingLabel
        {
            get => _strRatingLabel;
            set => _strRatingLabel = value;
        }

        /// <summary>
        /// Quantity.
        /// </summary>
        public decimal Quantity
        {
            get => _decQty;
            set
            {
                if (_decQty == value)
                    return;
                _decQty = value;
                OnPropertyChanged();
            }
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
        /// Use for ammo. The number of rounds that the nuyen amount buys.
        /// </summary>
        public decimal CostFor
        {
            get => _decCostFor;
            set => _decCostFor = value;
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
        /// Weight.
        /// </summary>
        public string Weight
        {
            get => _strWeight;
            set => _strWeight = value;
        }

        /// <summary>
        /// Value that was selected during an ImprovementManager dialogue.
        /// </summary>
        public string Extra
        {
            get => _strExtra;
            set
            {
                value = _objCharacter.ReverseTranslateExtra(value);
                if (Interlocked.Exchange(ref _strExtra, value) != value)
                    OnPropertyChanged();
            }
        }

        /// <summary>
        /// Whether or not the Foci is bonded.
        /// </summary>
        public bool Bonded
        {
            get => _blnBonded;
            set => _blnBonded = value;
        }

        /// <summary>
        /// Whether or not the Gear is equipped.
        /// </summary>
        public bool Equipped
        {
            get => _blnEquipped;
            set => _blnEquipped = value;
        }

        /// <summary>
        /// Whether or not the Gear's wireless bonus is enabled.
        /// </summary>
        public bool WirelessOn
        {
            get => _blnWirelessOn;
            set
            {
                if (_blnWirelessOn == value)
                    return;
                _blnWirelessOn = value;
                RefreshWirelessBonuses();
            }
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
        /// <param name="token">Cancellation token to listen to.</param>
        /// <returns></returns>
        public async Task<string> DisplayPageAsync(string strLanguage, CancellationToken token = default)
        {
            if (strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                return Page;
            XPathNavigator objNode = await this.GetNodeXPathAsync(strLanguage, token: token).ConfigureAwait(false);
            string strReturn = objNode?.SelectSingleNodeAndCacheExpression("altpage", token: token)?.Value ?? Page;
            return !string.IsNullOrWhiteSpace(strReturn) ? strReturn : Page;
        }

        public bool IsFlechetteAmmo
        {
            get => _blnIsFlechetteAmmo;
            set => _blnIsFlechetteAmmo = value;
        }

        public string AmmoForWeaponType
        {
            get => _strAmmoForWeaponType;
            set => _strAmmoForWeaponType = value;
        }

        public Clip LoadedIntoClip
        {
            get => _objLoadedIntoClip;
            set
            {
                Clip objOldValue = Interlocked.Exchange(ref _objLoadedIntoClip, value);
                if (objOldValue == value)
                    return;
                if (objOldValue != null)
                    objOldValue.AmmoGear = null;
                if (value != null)
                {
                    value.AmmoGear = this;
                    if (objOldValue == null)
                    {
                        Equipped = true;
                        ChangeEquippedStatus(true);
                    }
                }
            }
        }

        /// <inheritdoc />
        public string CanFormPersona
        {
            get => _strCanFormPersona;
            set => _strCanFormPersona = value;
        }

        /// <inheritdoc />
        public Task<string> GetCanFormPersonaAsync(CancellationToken token = default) => token.IsCancellationRequested
            ? Task.FromCanceled<string>(token)
            : Task.FromResult(_strCanFormPersona);

        /// <inheritdoc />
        public bool IsCommlink =>
            CanFormPersona.Contains("Self") || Children.Any(x => x.CanFormPersona.Contains("Parent"));

        /// <inheritdoc />
        public async Task<bool> GetIsCommlinkAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            return (await GetCanFormPersonaAsync(token).ConfigureAwait(false)).Contains("Self") || await Children
                .AnyAsync(async x => (await x.GetCanFormPersonaAsync(token).ConfigureAwait(false)).Contains("Parent"),
                    token: token).ConfigureAwait(false);
        }

        /// <summary>
        /// A List of child pieces of Gear.
        /// </summary>
        public TaggedObservableCollection<Gear> Children
        {
            get
            {
                using (_objCharacter.LockObject.EnterReadLock())
                    return _lstChildren;
            }
        }

        /// <summary>
        /// Notes.
        /// </summary>
        public string Notes
        {
            get => _strNotes;
            set
            {
                if (Interlocked.Exchange(ref _strNotes, value) != value)
                    OnPropertyChanged();
            }
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
        /// Device Rating string.
        /// </summary>
        public string DeviceRating
        {
            get => _strDeviceRating;
            set => _strDeviceRating = value;
        }

        /// <summary>
        /// Allow Renaming the Gear in Create Mode
        /// </summary>
        public bool AllowRename => _blnAllowRename && _objCharacter?.Created != true;

        /// <summary>
        /// Get the base value of a Matrix attribute of this gear (without children or Overclocker)
        /// </summary>
        /// <param name="strAttributeName">Matrix attribute name.</param>
        public int GetBaseMatrixAttribute(string strAttributeName)
        {
            string strExpression = this.GetMatrixAttributeString(strAttributeName);
            if (string.IsNullOrEmpty(strExpression))
            {
                switch (strAttributeName)
                {
                    case "Device Rating":
                        strExpression = IsCommlink ? "2" : "0";
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
                            strExpression = "0";
                        break;

                    default:
                        strExpression = "0";
                        break;
                }
            }

            if (strExpression.StartsWith("FixedValues(", StringComparison.Ordinal))
            {
                string[] strValues = strExpression.TrimStartOnce("FixedValues(", true).TrimEndOnce(')')
                    .Split(',', StringSplitOptions.RemoveEmptyEntries);
                strExpression = strValues[Math.Max(Math.Min(Rating, strValues.Length) - 1, 0)].Trim('[', ']');
            }

            if (Name == "Living Persona")
            {
                string strExtraExpression = string.Empty;
                switch (strAttributeName)
                {
                    case "Device Rating":
                        strExtraExpression
                            = string.Concat(ImprovementManager
                                            .GetCachedImprovementListForValueOf(
                                                CharacterObject, Improvement.ImprovementType.LivingPersonaDeviceRating)
                                            .Select(x => x.ImprovedName));
                        break;

                    case "Program Limit":
                        strExtraExpression
                            = string.Concat(ImprovementManager
                                            .GetCachedImprovementListForValueOf(
                                                CharacterObject, Improvement.ImprovementType.LivingPersonaProgramLimit)
                                            .Select(x => x.ImprovedName));
                        break;

                    case "Attack":
                        strExtraExpression
                            = string.Concat(ImprovementManager
                                            .GetCachedImprovementListForValueOf(
                                                CharacterObject, Improvement.ImprovementType.LivingPersonaAttack)
                                            .Select(x => x.ImprovedName));
                        break;

                    case "Sleaze":
                        strExtraExpression
                            = string.Concat(ImprovementManager
                                            .GetCachedImprovementListForValueOf(
                                                CharacterObject, Improvement.ImprovementType.LivingPersonaSleaze)
                                            .Select(x => x.ImprovedName));
                        break;

                    case "Data Processing":
                        strExtraExpression
                            = string.Concat(ImprovementManager
                                            .GetCachedImprovementListForValueOf(
                                                CharacterObject, Improvement.ImprovementType.LivingPersonaDataProcessing)
                                            .Select(x => x.ImprovedName));
                        break;

                    case "Firewall":
                        strExtraExpression
                            = string.Concat(ImprovementManager
                                            .GetCachedImprovementListForValueOf(
                                                CharacterObject, Improvement.ImprovementType.LivingPersonaFirewall)
                                            .Select(x => x.ImprovedName));
                        break;
                }

                if (!string.IsNullOrEmpty(strExtraExpression))
                    strExpression += strExtraExpression;
            }

            return string.IsNullOrEmpty(strExpression) ? 0 : ProcessRatingString(strExpression);
        }

        public async Task<int> GetBaseMatrixAttributeAsync(string strAttributeName, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            string strExpression = this.GetMatrixAttributeString(strAttributeName);
            if (string.IsNullOrEmpty(strExpression))
            {
                switch (strAttributeName)
                {
                    case "Device Rating":
                        strExpression = await GetIsCommlinkAsync(token).ConfigureAwait(false) ? "2" : "0";
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
                            strExpression = "0";
                        break;

                    default:
                        strExpression = "0";
                        break;
                }
            }

            if (strExpression.StartsWith("FixedValues(", StringComparison.Ordinal))
            {
                string[] strValues = strExpression.TrimStartOnce("FixedValues(", true).TrimEndOnce(')')
                    .Split(',', StringSplitOptions.RemoveEmptyEntries);
                strExpression = strValues[Math.Max(Math.Min(Rating, strValues.Length) - 1, 0)].Trim('[', ']');
            }

            if (Name == "Living Persona")
            {
                string strExtraExpression = string.Empty;
                switch (strAttributeName)
                {
                    case "Device Rating":
                        strExtraExpression
                            = string.Concat((await ImprovementManager
                                    .GetCachedImprovementListForValueOfAsync(
                                        CharacterObject, Improvement.ImprovementType.LivingPersonaDeviceRating, token: token).ConfigureAwait(false))
                                            .Select(x => x.ImprovedName));
                        break;

                    case "Program Limit":
                        strExtraExpression
                            = string.Concat((await ImprovementManager
                                    .GetCachedImprovementListForValueOfAsync(
                                        CharacterObject, Improvement.ImprovementType.LivingPersonaProgramLimit, token: token).ConfigureAwait(false))
                                            .Select(x => x.ImprovedName));
                        break;

                    case "Attack":
                        strExtraExpression
                            = string.Concat((await ImprovementManager
                                    .GetCachedImprovementListForValueOfAsync(
                                        CharacterObject, Improvement.ImprovementType.LivingPersonaAttack, token: token).ConfigureAwait(false))
                                            .Select(x => x.ImprovedName));
                        break;

                    case "Sleaze":
                        strExtraExpression
                            = string.Concat((await ImprovementManager
                                    .GetCachedImprovementListForValueOfAsync(
                                        CharacterObject, Improvement.ImprovementType.LivingPersonaSleaze, token: token).ConfigureAwait(false))
                                            .Select(x => x.ImprovedName));
                        break;

                    case "Data Processing":
                        strExtraExpression
                            = string.Concat((await ImprovementManager
                                    .GetCachedImprovementListForValueOfAsync(
                                        CharacterObject, Improvement.ImprovementType.LivingPersonaDataProcessing, token: token).ConfigureAwait(false))
                                            .Select(x => x.ImprovedName));
                        break;

                    case "Firewall":
                        strExtraExpression
                            = string.Concat((await ImprovementManager
                                    .GetCachedImprovementListForValueOfAsync(
                                        CharacterObject, Improvement.ImprovementType.LivingPersonaFirewall, token: token).ConfigureAwait(false))
                                            .Select(x => x.ImprovedName));
                        break;
                }

                if (!string.IsNullOrEmpty(strExtraExpression))
                    strExpression += strExtraExpression;
            }

            return string.IsNullOrEmpty(strExpression) ? 0 : await ProcessRatingStringAsync(strExpression, token).ConfigureAwait(false);
        }

        /// <summary>
        /// Get the bonus value of a Matrix attribute of this gear from children and Overclocker
        /// </summary>
        public int GetBonusMatrixAttribute(string strAttributeName)
        {
            if (string.IsNullOrEmpty(strAttributeName))
                return 0;
            int intReturn = Overclocked == strAttributeName ? 1 : 0;

            if (!strAttributeName.StartsWith("Mod ", StringComparison.Ordinal))
                strAttributeName = "Mod " + strAttributeName;

            foreach (Gear loopGear in Children)
            {
                if (loopGear.Equipped)
                {
                    intReturn += loopGear.GetTotalMatrixAttribute(strAttributeName);
                }
            }

            return intReturn;
        }

        /// <summary>
        /// Get the bonus value of a Matrix attribute of this gear from children and Overclocker
        /// </summary>
        public async Task<int> GetBonusMatrixAttributeAsync(string strAttributeName, CancellationToken token = default)
        {
            if (string.IsNullOrEmpty(strAttributeName))
                return 0;
            int intReturn = await GetOverclockedAsync(token).ConfigureAwait(false) == strAttributeName ? 1 : 0;

            if (!strAttributeName.StartsWith("Mod ", StringComparison.Ordinal))
                strAttributeName = "Mod " + strAttributeName;

            intReturn += await Children.SumAsync(x => x.Equipped, x => x.GetTotalMatrixAttributeAsync(strAttributeName, token), token).ConfigureAwait(false);

            return intReturn;
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
        /// Whether or not the Gear qualifies as a Program in the printout XML.
        /// </summary>
        public bool IsProgram => Category.EndsWith("Programs", StringComparison.OrdinalIgnoreCase) ||
                                 Category.EndsWith("Apps", StringComparison.OrdinalIgnoreCase) ||
                                 Category.StartsWith("Autosofts", StringComparison.OrdinalIgnoreCase) ||
                                 Category.StartsWith("Software", StringComparison.OrdinalIgnoreCase) ||
                                 Category.StartsWith("Skillsofts", StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Cost multiplier for Children attached to this Gear.
        /// </summary>
        public int ChildCostMultiplier
        {
            get => _intChildCostMultiplier;
            set => _intChildCostMultiplier = value;
        }

        /// <summary>
        /// Avail modifier for Children attached to this Gear.
        /// </summary>
        public int ChildAvailModifier
        {
            get => _intChildAvailModifier;
            set => _intChildAvailModifier = value;
        }

        private object _objParent;

        /// <summary>
        /// Parent Gear.
        /// </summary>
        public object Parent
        {
            get => _objParent;
            set
            {
                if (Interlocked.Exchange(ref _objParent, value) == value)
                    return;
                Rating = Math.Max(MinRatingValue, Math.Min(MaxRatingValue, Rating));
            }
        }

        /// <summary>
        /// Whether or not the Gear's cost should be discounted by 10% through the Black Market Pipeline Quality.
        /// </summary>
        public bool DiscountCost
        {
            get => _blnDiscountCost;
            set => _blnDiscountCost = value;
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

        /// <inheritdoc />
        public string Overclocked
        {
            get => CharacterObject.Overclocker ? _strOverclocked : string.Empty;
            set => _strOverclocked = value;
        }

        /// <inheritdoc />
        public async Task<string> GetOverclockedAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            return await CharacterObject.GetOverclockerAsync(token).ConfigureAwait(false) ? _strOverclocked : string.Empty;
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
        /// Used by our sorting algorithm to remember which order the user moves things to
        /// </summary>
        public int SortOrder
        {
            get => _intSortOrder;
            set => _intSortOrder = value;
        }

        /// <summary>
        /// Whether or not the Gear is included in its parent item when purchased (currently applies to Armor only).
        /// </summary>
        public bool IncludedInParent => !string.IsNullOrEmpty(ParentID);

        /// <summary>
        /// Whether or not the Gear's equipped status can be changed.
        /// </summary>
        public bool CanChangeEquip => !IncludedInParent && LoadedIntoClip == null;

        /// <summary>
        /// ID of the object that added this cyberware (if any).
        /// </summary>
        public string ParentID
        {
            get => _strParentID;
            set
            {
                if (Interlocked.Exchange(ref _strParentID, value) != value)
                    OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private readonly ConcurrentHashSet<PropertyChangedAsyncEventHandler> _setPropertyChangedAsync =
            new ConcurrentHashSet<PropertyChangedAsyncEventHandler>();

        public event PropertyChangedAsyncEventHandler PropertyChangedAsync
        {
            add => _setPropertyChangedAsync.TryAdd(value);
            remove => _setPropertyChangedAsync.Remove(value);
        }

        private XmlNode _objCachedMyXmlNode;
        private string _strCachedXmlNodeLanguage = string.Empty;

        public async Task<XmlNode> GetNodeCoreAsync(bool blnSync, string strLanguage, CancellationToken token = default)
        {
            XmlNode objReturn = _objCachedMyXmlNode;
            if (objReturn != null && strLanguage == _strCachedXmlNodeLanguage
                                  && !GlobalSettings.LiveCustomData)
                return objReturn;
            XmlDocument objDoc = blnSync
                // ReSharper disable once MethodHasAsyncOverload
                ? _objCharacter.LoadData("gear.xml", strLanguage, token: token)
                : await _objCharacter.LoadDataAsync("gear.xml", strLanguage, token: token).ConfigureAwait(false);
            if (SourceID != Guid.Empty)
                objReturn = objDoc.TryGetNodeById("/chummer/gears/gear", SourceID);
            if (objReturn == null)
            {
                string strCategoryFilter = "category = " + Category.CleanXPath();
                objReturn = objDoc.TryGetNodeByNameOrId("/chummer/gears/gear", Name, strCategoryFilter);
                if (objReturn == null)
                {
                    string strNameCleaned = Name.CleanXPath();
                    objReturn = objDoc.SelectSingleNode("/chummer/gears/gear[contains(name, " + strNameCleaned
                                                        + ") and " + strCategoryFilter + "]")
                                ?? objDoc.TryGetNodeByNameOrId("/chummer/gears/gear", Name)
                                ?? objDoc.SelectSingleNode("/chummer/gears/gear[contains(name, " + strNameCleaned
                                                           + ")]");
                }

                objReturn?.TryGetGuidFieldQuickly("id", ref _guiSourceID);
            }

            _objCachedMyXmlNode = objReturn;
            _strCachedXmlNodeLanguage = strLanguage;
            return objReturn;
        }

        private XPathNavigator _objCachedMyXPathNode;
        private string _strCachedXPathNodeLanguage = string.Empty;

        public async Task<XPathNavigator> GetNodeXPathCoreAsync(bool blnSync, string strLanguage, CancellationToken token = default)
        {
            XPathNavigator objReturn = _objCachedMyXPathNode;
            if (objReturn != null && strLanguage == _strCachedXPathNodeLanguage
                                  && !GlobalSettings.LiveCustomData)
                return objReturn;
            XPathNavigator objDoc = blnSync
                // ReSharper disable once MethodHasAsyncOverload
                ? _objCharacter.LoadDataXPath("gear.xml", strLanguage, token: token)
                : await _objCharacter.LoadDataXPathAsync("gear.xml", strLanguage, token: token).ConfigureAwait(false);
            if (SourceID != Guid.Empty)
                objReturn = objDoc.TryGetNodeById("/chummer/gears/gear", SourceID);
            if (objReturn == null)
            {
                string strCategoryFilter = "category = " + Category.CleanXPath();
                objReturn = objDoc.TryGetNodeByNameOrId("/chummer/gears/gear", Name, strCategoryFilter);
                if (objReturn == null)
                {
                    string strNameCleaned = Name.CleanXPath();
                    objReturn = objDoc.SelectSingleNode("/chummer/gears/gear[contains(name, " + strNameCleaned
                                                        + ") and " + strCategoryFilter + "]")
                                ?? objDoc.TryGetNodeByNameOrId("/chummer/gears/gear", Name)
                                ?? objDoc.SelectSingleNode("/chummer/gears/gear[contains(name, " + strNameCleaned
                                                           + ")]");
                }
                objReturn?.TryGetGuidFieldQuickly("id", ref _guiSourceID);
            }

            _objCachedMyXPathNode = objReturn;
            _strCachedXPathNodeLanguage = strLanguage;
            return objReturn;
        }

        #endregion Properties

        #region Complex Properties

        /// <summary>
        /// Total Availability in the program's current language.
        /// </summary>
        public string DisplayTotalAvail => TotalAvail(GlobalSettings.CultureInfo, GlobalSettings.Language);

        /// <summary>
        /// Total Availability in the program's current language.
        /// </summary>
        public Task<string> GetDisplayTotalAvailAsync(CancellationToken token = default) => TotalAvailAsync(GlobalSettings.CultureInfo, GlobalSettings.Language, token);

        /// <summary>
        /// Total Availability of the Gear and its accessories.
        /// </summary>
        public string TotalAvail(CultureInfo objCulture, string strLanguage)
        {
            return TotalAvailTuple().ToString(objCulture, strLanguage);
        }

        /// <summary>
        /// Total Availability of the Gear and its accessories.
        /// </summary>
        public async Task<string> TotalAvailAsync(CultureInfo objCulture, string strLanguage, CancellationToken token = default)
        {
            return await (await TotalAvailTupleAsync(token: token).ConfigureAwait(false)).ToStringAsync(objCulture, strLanguage, token).ConfigureAwait(false);
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
                    string[] strValues = strAvail.TrimStartOnce("FixedValues(", true).TrimEndOnce(')')
                        .Split(',', StringSplitOptions.RemoveEmptyEntries);
                    strAvail = strValues[Math.Max(Math.Min(Rating, strValues.Length) - 1, 0)];
                }

                chrLastAvailChar = strAvail[strAvail.Length - 1];
                if (chrLastAvailChar == 'F' || chrLastAvailChar == 'R')
                {
                    strAvail = strAvail.Substring(0, strAvail.Length - 1);
                }

                blnModifyParentAvail = strAvail.StartsWith('+', '-') && !IncludedInParent;
                using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdAvail))
                {
                    sbdAvail.Append(strAvail.TrimStart('+'));
                    sbdAvail.CheapReplace(strAvail, "MinRating",
                                          () => MinRatingValue.ToString(GlobalSettings.InvariantCultureInfo));
                    sbdAvail.CheapReplace(strAvail, "Parent Rating",
                                          () => (Parent as IHasRating)?.Rating.ToString(
                                              GlobalSettings.InvariantCultureInfo));
                    sbdAvail.Replace("Rating", Rating.ToString(GlobalSettings.InvariantCultureInfo));
                    foreach (CharacterAttrib objLoopAttribute in _objCharacter.GetAllAttributes())
                    {
                        sbdAvail.CheapReplace(strAvail, objLoopAttribute.Abbrev,
                                              () => objLoopAttribute.TotalValue.ToString(
                                                  GlobalSettings.InvariantCultureInfo));
                        sbdAvail.CheapReplace(strAvail, objLoopAttribute.Abbrev + "Base",
                                              () => objLoopAttribute.TotalBase.ToString(
                                                  GlobalSettings.InvariantCultureInfo));
                    }

                    (bool blnIsSuccess, object objProcess)
                        = CommonFunctions.EvaluateInvariantXPath(sbdAvail.ToString());
                    if (blnIsSuccess)
                        intAvail += ((double)objProcess).StandardRound();
                }
            }

            if (blnCheckChildren)
            {
                // Run through the child items and increase the Avail by any Mod whose Avail contains "+".
                foreach (Gear objChild in Children)
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

            return new AvailabilityValue(intAvail, chrLastAvailChar, blnModifyParentAvail, IncludedInParent);
        }

        /// <summary>
        /// Total Availability as a triple.
        /// </summary>
        public async Task<AvailabilityValue> TotalAvailTupleAsync(bool blnCheckChildren = true, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            bool blnModifyParentAvail = false;
            string strAvail = Avail;
            char chrLastAvailChar = ' ';
            int intAvail = 0;
            if (strAvail.Length > 0)
            {
                if (strAvail.StartsWith("FixedValues(", StringComparison.Ordinal))
                {
                    string[] strValues = strAvail.TrimStartOnce("FixedValues(", true).TrimEndOnce(')')
                        .Split(',', StringSplitOptions.RemoveEmptyEntries);
                    strAvail = strValues[Math.Max(Math.Min(Rating, strValues.Length) - 1, 0)];
                }

                chrLastAvailChar = strAvail[strAvail.Length - 1];
                if (chrLastAvailChar == 'F' || chrLastAvailChar == 'R')
                {
                    strAvail = strAvail.Substring(0, strAvail.Length - 1);
                }

                blnModifyParentAvail = strAvail.StartsWith('+', '-') && !IncludedInParent;
                using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdAvail))
                {
                    sbdAvail.Append(strAvail.TrimStart('+'));
                    await sbdAvail.CheapReplaceAsync(strAvail, "MinRating",
                                                     () => MinRatingValue.ToString(GlobalSettings.InvariantCultureInfo), token: token).ConfigureAwait(false);
                    await sbdAvail.CheapReplaceAsync(strAvail, "Parent Rating",
                                                     () => (Parent as IHasRating)?.Rating.ToString(
                                                         GlobalSettings.InvariantCultureInfo), token: token).ConfigureAwait(false);
                    sbdAvail.Replace("Rating", Rating.ToString(GlobalSettings.InvariantCultureInfo));
                    // Keeping enumerations separate reduces heap allocations
                    AttributeSection objAttributeSection = await _objCharacter.GetAttributeSectionAsync(token).ConfigureAwait(false);
                    await (await objAttributeSection.GetAttributeListAsync(token).ConfigureAwait(false)).ForEachAsync(async objLoopAttribute =>
                    {
                        await sbdAvail.CheapReplaceAsync(strAvail, objLoopAttribute.Abbrev,
                                                         async () => (await objLoopAttribute.GetTotalValueAsync(token)
                                                             .ConfigureAwait(false)).ToString(
                                                             GlobalSettings.InvariantCultureInfo), token: token)
                                      .ConfigureAwait(false);
                        await sbdAvail.CheapReplaceAsync(strAvail, objLoopAttribute.Abbrev + "Base",
                                                         async () => (await objLoopAttribute.GetTotalBaseAsync(token)
                                                             .ConfigureAwait(false)).ToString(
                                                             GlobalSettings.InvariantCultureInfo), token: token)
                                      .ConfigureAwait(false);
                    }, token).ConfigureAwait(false);
                    await (await objAttributeSection.GetSpecialAttributeListAsync(token).ConfigureAwait(false)).ForEachAsync(async objLoopAttribute =>
                    {
                        await sbdAvail.CheapReplaceAsync(strAvail, objLoopAttribute.Abbrev,
                                                         async () => (await objLoopAttribute.GetTotalValueAsync(token)
                                                             .ConfigureAwait(false)).ToString(
                                                             GlobalSettings.InvariantCultureInfo), token: token)
                                      .ConfigureAwait(false);
                        await sbdAvail.CheapReplaceAsync(strAvail, objLoopAttribute.Abbrev + "Base",
                                                         async () => (await objLoopAttribute.GetTotalBaseAsync(token)
                                                             .ConfigureAwait(false)).ToString(
                                                             GlobalSettings.InvariantCultureInfo), token: token)
                                      .ConfigureAwait(false);
                    }, token).ConfigureAwait(false);

                    (bool blnIsSuccess, object objProcess)
                        = await CommonFunctions.EvaluateInvariantXPathAsync(sbdAvail.ToString(), token).ConfigureAwait(false);
                    if (blnIsSuccess)
                        intAvail += ((double)objProcess).StandardRound();
                }
            }

            if (blnCheckChildren)
            {
                // Run through the child items and increase the Avail by any Mod whose Avail contains "+".
                intAvail += await Children.SumAsync(async objChild =>
                {
                    if (objChild.ParentID == InternalId)
                        return 0;
                    AvailabilityValue objLoopAvailTuple
                        = await objChild.TotalAvailTupleAsync(token: token).ConfigureAwait(false);
                    if (objLoopAvailTuple.Suffix == 'F')
                        chrLastAvailChar = 'F';
                    else if (chrLastAvailChar != 'F' && objLoopAvailTuple.Suffix == 'R')
                        chrLastAvailChar = 'R';
                    return objLoopAvailTuple.AddToParent ? objLoopAvailTuple.Value : 0;
                }, token).ConfigureAwait(false);
            }

            // Avail cannot go below 0. This typically happens when an item with Avail 0 is given the Second Hand category.
            if (intAvail < 0)
                intAvail = 0;

            return new AvailabilityValue(intAvail, chrLastAvailChar, blnModifyParentAvail, IncludedInParent);
        }

        /// <summary>
        /// Calculated Capacity of the Gear.
        /// </summary>
        public string CalculatedCapacity
        {
            get
            {
                string strReturn = _strCapacity;
                if (string.IsNullOrEmpty(strReturn))
                    return 0.0m.ToString("#,0.##", GlobalSettings.CultureInfo);
                if (strReturn.StartsWith("FixedValues(", StringComparison.Ordinal))
                {
                    string[] strValues = strReturn.TrimStartOnce("FixedValues(", true).TrimEndOnce(')')
                        .Split(',', StringSplitOptions.RemoveEmptyEntries);
                    strReturn = strValues[Math.Max(Math.Min(Rating, strValues.Length) - 1, 0)];
                }

                int intPos = strReturn.IndexOf("/[", StringComparison.Ordinal);
                if (intPos != -1)
                {
                    string strFirstHalf = strReturn.Substring(0, intPos);
                    string strSecondHalf = strReturn.Substring(intPos + 1, strReturn.Length - intPos - 1);
                    bool blnSquareBrackets = strFirstHalf.StartsWith('[');
                    if (blnSquareBrackets && strFirstHalf.Length > 2)
                        strFirstHalf = strFirstHalf.Substring(1, strFirstHalf.Length - 2);

                    if (strFirstHalf == "[*]")
                        strReturn = "*";
                    else
                    {
                        if (strFirstHalf.StartsWith("FixedValues(", StringComparison.Ordinal))
                        {
                            string[] strValues = strFirstHalf.TrimStartOnce("FixedValues(", true).TrimEndOnce(')')
                                .Split(',', StringSplitOptions.RemoveEmptyEntries);
                            strFirstHalf = strValues[Math.Max(Math.Min(Rating, strValues.Length) - 1, 0)];
                        }

                        (bool blnIsSuccess, object objProcess) = CommonFunctions.EvaluateInvariantXPath(
                            strFirstHalf.Replace("Rating", Rating.ToString(GlobalSettings.InvariantCultureInfo)));
                        strReturn = blnIsSuccess
                            ? ((double)objProcess).ToString("#,0.##", GlobalSettings.CultureInfo)
                            : strFirstHalf;
                    }

                    if (blnSquareBrackets)
                        strReturn = '[' + strReturn + ']';
                    if (!string.IsNullOrEmpty(strSecondHalf))
                        strReturn += '/' + strSecondHalf;
                }
                else
                {
                    // If the Capacity is determined by the Rating, evaluate the expression.
                    // XPathExpression cannot evaluate while there are square brackets, so remove them if necessary.
                    bool blnSquareBrackets = strReturn.StartsWith('[');
                    if (blnSquareBrackets)
                        strReturn = strReturn.Substring(1, strReturn.Length - 2);

                    if (strReturn.Contains("Rating"))
                    {
                        // This has resulted in a non-whole number, so round it (minimum of 1).
                        (bool blnIsSuccess, object objProcess) = CommonFunctions.EvaluateInvariantXPath(strReturn
                                .CheapReplace("Parent Rating",
                                    () => (Parent as IHasRating)?.Rating.ToString(GlobalSettings.InvariantCultureInfo))
                                .Replace("Rating", Rating.ToString(GlobalSettings.InvariantCultureInfo)));
                        double dblNumber = blnIsSuccess ? (double)objProcess : 1;
                        if (dblNumber < 1)
                            dblNumber = 1;
                        strReturn = dblNumber.ToString("#,0.##", GlobalSettings.CultureInfo);
                    }
                    else if (decimal.TryParse(strReturn, NumberStyles.Any, GlobalSettings.InvariantCultureInfo,
                        out decimal decReturn))
                    {
                        // Just a straight Capacity, so return the value.
                        strReturn = decReturn.ToString("#,0.##", GlobalSettings.CultureInfo);
                    }

                    if (blnSquareBrackets)
                        strReturn = '[' + strReturn + ']';
                }

                return strReturn;
            }
        }

        /// <summary>
        /// Calculated Capacity of the Gear when attached to Armor.
        /// </summary>
        public string CalculatedArmorCapacity
        {
            get
            {
                string strReturn = ArmorCapacity;
                if (string.IsNullOrEmpty(strReturn))
                    return 0.ToString(GlobalSettings.CultureInfo);
                int intPos = strReturn.IndexOf("/[", StringComparison.Ordinal);
                if (intPos != -1)
                {
                    string strFirstHalf = strReturn.Substring(0, intPos);
                    string strSecondHalf = strReturn.Substring(intPos + 1, strReturn.Length - intPos - 1);
                    bool blnSquareBrackets = strFirstHalf.StartsWith('[');
                    if (blnSquareBrackets && strFirstHalf.Length > 2)
                        strFirstHalf = strFirstHalf.Substring(1, strFirstHalf.Length - 2);

                    if (strFirstHalf == "[*]")
                        strReturn = "*";
                    else
                    {
                        if (strFirstHalf.StartsWith("FixedValues(", StringComparison.Ordinal))
                        {
                            string[] strValues = strFirstHalf.TrimStartOnce("FixedValues(", true).TrimEndOnce(')')
                                .Split(',', StringSplitOptions.RemoveEmptyEntries);
                            strFirstHalf = strValues[Math.Max(Math.Min(Rating, strValues.Length) - 1, 0)];
                        }

                        (bool blnIsSuccess, object objProcess) = CommonFunctions.EvaluateInvariantXPath(
                            strFirstHalf.Replace("Rating", Rating.ToString(GlobalSettings.InvariantCultureInfo)));
                        strReturn = blnIsSuccess
                            ? ((double)objProcess).ToString("#,0.##", GlobalSettings.CultureInfo)
                            : strFirstHalf;
                    }

                    if (blnSquareBrackets)
                        strReturn = '[' + strReturn + ']';
                    strReturn += '/' + strSecondHalf;
                }
                else if (strReturn.Contains("Rating"))
                {
                    // If the Capacity is determined by the Rating, evaluate the expression.
                    // XPathExpression cannot evaluate while there are square brackets, so remove them if necessary.
                    bool blnSquareBrackets = strReturn.StartsWith('[');
                    if (blnSquareBrackets)
                        strReturn = strReturn.Substring(1, strReturn.Length - 2);

                    (bool blnIsSuccess, object objProcess) = CommonFunctions.EvaluateInvariantXPath(
                        strReturn.Replace("Rating", Rating.ToString(GlobalSettings.InvariantCultureInfo)));
                    if (blnIsSuccess)
                        strReturn = ((double)objProcess).ToString("#,0.##", GlobalSettings.CultureInfo);
                    if (blnSquareBrackets)
                        strReturn = '[' + strReturn + ']';
                }
                else if (decimal.TryParse(strReturn, NumberStyles.Any, GlobalSettings.InvariantCultureInfo,
                    out decimal decReturn))
                {
                    // Just a straight Capacity, so return the value.
                    strReturn = decReturn.ToString("#,0.##", GlobalSettings.CultureInfo);
                }

                return strReturn;
            }
        }

        /// <summary>
        /// Total cost of the just the Gear itself before we factor in any multipliers.
        /// </summary>
        public decimal OwnCostPreMultipliers
        {
            get
            {
                string strCostExpression = Cost;

                if (strCostExpression.StartsWith("FixedValues(", StringComparison.Ordinal))
                {
                    string[] strValues = strCostExpression.TrimStartOnce("FixedValues(", true).TrimEndOnce(')')
                        .Split(',', StringSplitOptions.RemoveEmptyEntries);
                    strCostExpression = strValues[Math.Max(Math.Min(Rating, strValues.Length) - 1, 0)].Trim('[', ']');
                }

                decimal decGearCost = 0;
                decimal decParentCost = 0;
                if (Parent != null)
                {
                    if (strCostExpression.Contains("Gear Cost"))
                        decGearCost = ((Gear)Parent).CalculatedCost;
                    if (strCostExpression.Contains("Parent Cost"))
                        decParentCost = ((Gear)Parent).OwnCostPreMultipliers;
                }

                decimal decTotalChildrenCost = 0;
                if (Children.Count > 0 && strCostExpression.Contains("Children Cost"))
                {
                    decTotalChildrenCost += Children.Sum(x => x.CalculatedCost);
                }

                if (string.IsNullOrEmpty(strCostExpression))
                    return 0;

                decimal decReturn = 0;
                using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdCost))
                {
                    sbdCost.Append(strCostExpression.TrimStart('+'));
                    sbdCost.Replace("Gear Cost", decGearCost.ToString(GlobalSettings.InvariantCultureInfo));
                    sbdCost.Replace("Children Cost",
                                    decTotalChildrenCost.ToString(GlobalSettings.InvariantCultureInfo));
                    sbdCost.Replace("Parent Rating",
                                    (Parent as IHasRating)?.Rating.ToString(GlobalSettings.InvariantCultureInfo)
                                    ?? "0");
                    sbdCost.Replace("Rating", Rating.ToString(GlobalSettings.InvariantCultureInfo));
                    sbdCost.Replace("Parent Cost", decParentCost.ToString(GlobalSettings.InvariantCultureInfo));
                    foreach (CharacterAttrib objLoopAttribute in _objCharacter.GetAllAttributes())
                    {
                        sbdCost.CheapReplace(strCostExpression, objLoopAttribute.Abbrev,
                                             () => objLoopAttribute.TotalValue.ToString(
                                                 GlobalSettings.InvariantCultureInfo));
                        sbdCost.CheapReplace(strCostExpression, objLoopAttribute.Abbrev + "Base",
                                             () => objLoopAttribute.TotalBase.ToString(
                                                 GlobalSettings.InvariantCultureInfo));
                    }

                    // This is first converted to a decimal and rounded up since some items have a multiplier that is not a whole number, such as 2.5.
                    (bool blnIsSuccess, object objProcess)
                        = CommonFunctions.EvaluateInvariantXPath(sbdCost.ToString());
                    if (blnIsSuccess)
                        decReturn = Convert.ToDecimal(objProcess, GlobalSettings.InvariantCultureInfo);
                }

                if (DiscountCost)
                    decReturn *= 0.9m;

                return decReturn;
            }
        }

        /// <summary>
        /// Total cost of the just the Gear itself before we factor in any multipliers.
        /// </summary>
        public async Task<decimal> GetOwnCostPreMultipliersAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            string strCostExpression = Cost;

            if (strCostExpression.StartsWith("FixedValues(", StringComparison.Ordinal))
            {
                string[] strValues = strCostExpression.TrimStartOnce("FixedValues(", true).TrimEndOnce(')')
                                                      .Split(',', StringSplitOptions.RemoveEmptyEntries);
                strCostExpression = strValues[Math.Max(Math.Min(Rating, strValues.Length) - 1, 0)].Trim('[', ']');
            }

            decimal decGearCost = 0;
            decimal decParentCost = 0;
            if (Parent != null)
            {
                if (strCostExpression.Contains("Gear Cost"))
                    decGearCost = await ((Gear) Parent).GetCalculatedCostAsync(token).ConfigureAwait(false);
                if (strCostExpression.Contains("Parent Cost"))
                    decParentCost = await ((Gear) Parent).GetOwnCostPreMultipliersAsync(token).ConfigureAwait(false);
            }

            decimal decTotalChildrenCost = 0;
            if (await Children.GetCountAsync(token).ConfigureAwait(false) > 0 && strCostExpression.Contains("Children Cost"))
            {
                decTotalChildrenCost += await Children.SumAsync(x => x.GetCalculatedCostAsync(token), token).ConfigureAwait(false);
            }

            if (string.IsNullOrEmpty(strCostExpression))
                return 0;

            decimal decReturn = 0;
            using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdCost))
            {
                sbdCost.Append(strCostExpression.TrimStart('+'));
                sbdCost.Replace("Gear Cost", decGearCost.ToString(GlobalSettings.InvariantCultureInfo));
                sbdCost.Replace("Children Cost",
                                decTotalChildrenCost.ToString(GlobalSettings.InvariantCultureInfo));
                sbdCost.Replace("Parent Rating",
                                (Parent as IHasRating)?.Rating.ToString(GlobalSettings.InvariantCultureInfo)
                                ?? "0");
                sbdCost.Replace("Rating", Rating.ToString(GlobalSettings.InvariantCultureInfo));
                sbdCost.Replace("Parent Cost", decParentCost.ToString(GlobalSettings.InvariantCultureInfo));

                // Keeping enumerations separate reduces heap allocations
                AttributeSection objAttributeSection = await _objCharacter.GetAttributeSectionAsync(token).ConfigureAwait(false);
                await (await objAttributeSection.GetAttributeListAsync(token).ConfigureAwait(false)).ForEachAsync(async objLoopAttribute =>
                {
                    await sbdCost.CheapReplaceAsync(strCostExpression, objLoopAttribute.Abbrev,
                                                    async () => (await objLoopAttribute.GetTotalValueAsync(token)
                                                        .ConfigureAwait(false)).ToString(
                                                        GlobalSettings.InvariantCultureInfo), token: token)
                                 .ConfigureAwait(false);
                    await sbdCost.CheapReplaceAsync(strCostExpression, objLoopAttribute.Abbrev + "Base",
                                                    async () => (await objLoopAttribute.GetTotalBaseAsync(token)
                                                        .ConfigureAwait(false)).ToString(
                                                        GlobalSettings.InvariantCultureInfo), token: token)
                                 .ConfigureAwait(false);
                }, token).ConfigureAwait(false);
                await (await objAttributeSection.GetSpecialAttributeListAsync(token).ConfigureAwait(false)).ForEachAsync(async objLoopAttribute =>
                {
                    await sbdCost.CheapReplaceAsync(strCostExpression, objLoopAttribute.Abbrev,
                                                    async () => (await objLoopAttribute.GetTotalValueAsync(token)
                                                        .ConfigureAwait(false)).ToString(
                                                        GlobalSettings.InvariantCultureInfo), token: token)
                                 .ConfigureAwait(false);
                    await sbdCost.CheapReplaceAsync(strCostExpression, objLoopAttribute.Abbrev + "Base",
                                                    async () => (await objLoopAttribute.GetTotalBaseAsync(token)
                                                        .ConfigureAwait(false)).ToString(
                                                        GlobalSettings.InvariantCultureInfo), token: token)
                                 .ConfigureAwait(false);
                }, token).ConfigureAwait(false);

                // This is first converted to a decimal and rounded up since some items have a multiplier that is not a whole number, such as 2.5.
                (bool blnIsSuccess, object objProcess)
                    = await CommonFunctions.EvaluateInvariantXPathAsync(sbdCost.ToString(), token).ConfigureAwait(false);
                if (blnIsSuccess)
                    decReturn = Convert.ToDecimal(objProcess, GlobalSettings.InvariantCultureInfo);
            }

            if (DiscountCost)
                decReturn *= 0.9m;

            return decReturn;
        }

        /// <summary>
        /// Total cost of the just the Gear itself.
        /// </summary>
        public decimal CalculatedCost => OwnCostPreMultipliers * Quantity / CostFor;

        public async Task<decimal> GetCalculatedCostAsync(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            return await GetOwnCostPreMultipliersAsync(token).ConfigureAwait(false) * Quantity / CostFor;
        }

        /// <summary>
        /// Total cost of the Gear and its accessories.
        /// </summary>
        public decimal TotalCost
        {
            get
            {
                // Add in the cost of all child components.
                decimal decPlugin = Children.Sum(x => x.TotalCost);

                // The number is divided at the end for ammo purposes. This is done since the cost is per "costfor" but is being multiplied by the actual number of rounds.
                int intParentMultiplier = (Parent as IHasChildrenAndCost<Gear>)?.ChildCostMultiplier ?? 1;
                
                // Add in the cost of the plugins separate since their value is not based on the Cost For number (it is always cost x qty).
                return OwnCostPreMultipliers * Quantity * intParentMultiplier / CostFor + decPlugin * Quantity;
            }
        }

        /// <summary>
        /// Total cost of the Gear and its accessories.
        /// </summary>
        public async Task<decimal> GetTotalCostAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();

            // Add in the cost of all child components.
            decimal decPlugin = await Children.SumAsync(x => x.GetTotalCostAsync(token), token).ConfigureAwait(false);

            // The number is divided at the end for ammo purposes. This is done since the cost is per "costfor" but is being multiplied by the actual number of rounds.
            int intParentMultiplier = (Parent as IHasChildrenAndCost<Gear>)?.ChildCostMultiplier ?? 1;

            // Add in the cost of the plugins separate since their value is not based on the Cost For number (it is always cost x qty).
            return await GetOwnCostPreMultipliersAsync(token).ConfigureAwait(false) * Quantity * intParentMultiplier / CostFor + decPlugin * Quantity;
        }

        public decimal StolenTotalCost => CalculatedStolenTotalCost(true);

        public decimal NonStolenTotalCost => CalculatedStolenTotalCost(false);

        public decimal CalculatedStolenTotalCost(bool blnStolen)
        {
            // Add in the cost of all child components.
            decimal decPlugin = Children.Sum(x => x.CalculatedStolenTotalCost(blnStolen));
            if (Stolen != blnStolen)
                return decPlugin * Quantity;

            // The number is divided at the end for ammo purposes. This is done since the cost is per "costfor" but is being multiplied by the actual number of rounds.
            int intParentMultiplier = (Parent as IHasChildrenAndCost<Gear>)?.ChildCostMultiplier ?? 1;
            
            // Add in the cost of the plugins separate since their value is not based on the Cost For number (it is always cost x qty).
            return OwnCostPreMultipliers * Quantity * intParentMultiplier / CostFor + decPlugin * Quantity;
        }

        public Task<decimal> GetStolenTotalCostAsync(CancellationToken token = default) => CalculatedStolenTotalCostAsync(true, token);

        public Task<decimal> GetNonStolenTotalCostAsync(CancellationToken token = default) => CalculatedStolenTotalCostAsync(false, token);

        public async Task<decimal> CalculatedStolenTotalCostAsync(bool blnStolen, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();

            // Add in the cost of all child components.
            decimal decPlugin = await Children.SumAsync(x => x.CalculatedStolenTotalCostAsync(blnStolen, token), token).ConfigureAwait(false);
            if (Stolen != blnStolen)
                return decPlugin * Quantity;

            // The number is divided at the end for ammo purposes. This is done since the cost is per "costfor" but is being multiplied by the actual number of rounds.
            int intParentMultiplier = (Parent as IHasChildrenAndCost<Gear>)?.ChildCostMultiplier ?? 1;

            // Add in the cost of the plugins separate since their value is not based on the Cost For number (it is always cost x qty).
            return await GetOwnCostPreMultipliersAsync(token).ConfigureAwait(false) * Quantity * intParentMultiplier / CostFor + decPlugin * Quantity;
        }

        /// <summary>
        /// The cost of just the Gear itself.
        /// </summary>
        public decimal OwnCost =>
            (OwnCostPreMultipliers * (Parent as IHasChildrenAndCost<Gear>)?.ChildCostMultiplier ?? 1) / CostFor;

        /// <summary>
        /// The cost of just the Gear itself.
        /// </summary>
        public async Task<decimal> GetOwnCostAsync(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            return (await GetOwnCostPreMultipliersAsync(token).ConfigureAwait(false)
                * (Parent as IHasChildrenAndCost<Gear>)?.ChildCostMultiplier ?? 1) / CostFor;
        }

        /// <summary>
        /// Total weight of the just the Gear itself
        /// </summary>
        public decimal OwnWeight
        {
            get
            {
                if (IncludedInParent)
                    return 0;
                string strWeightExpression = Weight;
                if (string.IsNullOrEmpty(strWeightExpression))
                    return 0;

                if (strWeightExpression.StartsWith("FixedValues(", StringComparison.Ordinal))
                {
                    string[] strValues = strWeightExpression.TrimStartOnce("FixedValues(", true).TrimEndOnce(')')
                                                            .Split(',', StringSplitOptions.RemoveEmptyEntries);
                    strWeightExpression = strValues[Math.Max(Math.Min(Rating, strValues.Length) - 1, 0)].Trim('[', ']');
                }

                decimal decGearWeight = 0;
                decimal decParentWeight = 0;
                if (Parent != null && strWeightExpression.ContainsAny("Parent Weight", "Gear Weight"))
                {
                    decParentWeight = ((Gear)Parent).OwnWeight;
                    decGearWeight = decParentWeight * ((Gear)Parent).Quantity;
                }

                decimal decTotalChildrenWeight = 0;
                if (Children.Count > 0 && strWeightExpression.Contains("Children Weight"))
                {
                    decTotalChildrenWeight += Children.Sum(x => x.OwnWeight * x.Quantity);
                }

                decimal decReturn = 0;
                using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdWeight))
                {
                    sbdWeight.Append(strWeightExpression.TrimStart('+'));
                    sbdWeight.Replace("Gear Weight", decGearWeight.ToString(GlobalSettings.InvariantCultureInfo));
                    sbdWeight.Replace("Children Weight",
                                      decTotalChildrenWeight.ToString(GlobalSettings.InvariantCultureInfo));
                    sbdWeight.Replace("Parent Rating",
                                      (Parent as IHasRating)?.Rating.ToString(GlobalSettings.InvariantCultureInfo)
                                      ?? "0");
                    sbdWeight.Replace("Rating", Rating.ToString(GlobalSettings.InvariantCultureInfo));
                    sbdWeight.Replace("Parent Weight", decParentWeight.ToString(GlobalSettings.InvariantCultureInfo));
                    foreach (CharacterAttrib objLoopAttribute in _objCharacter.GetAllAttributes())
                    {
                        sbdWeight.CheapReplace(strWeightExpression, objLoopAttribute.Abbrev,
                                               () => objLoopAttribute.TotalValue.ToString(
                                                   GlobalSettings.InvariantCultureInfo));
                        sbdWeight.CheapReplace(strWeightExpression, objLoopAttribute.Abbrev + "Base",
                                               () => objLoopAttribute.TotalBase.ToString(
                                                   GlobalSettings.InvariantCultureInfo));
                    }

                    (bool blnIsSuccess, object objProcess)
                        = CommonFunctions.EvaluateInvariantXPath(sbdWeight.ToString());
                    if (blnIsSuccess)
                        decReturn = Convert.ToDecimal(objProcess, GlobalSettings.InvariantCultureInfo);
                }

                return decReturn;
            }
        }

        /// <summary>
        /// Total weight of the Gear and its accessories.
        /// </summary>
        public decimal TotalWeight => (OwnWeight + Children.Sum(x => x.Equipped, x => x.TotalWeight)) * Quantity;

        /// <summary>
        /// The Gear's Capacity cost if used as a plugin.
        /// </summary>
        public decimal PluginCapacity
        {
            get
            {
                string strCapacity = CalculatedCapacity;
                // If this is a multiple-capacity item, use only the second half.
                int intPos = strCapacity.IndexOf("/[", StringComparison.Ordinal);
                if (intPos != -1)
                {
                    strCapacity = strCapacity.Substring(intPos + 1);
                }

                // Only items that contain square brackets should consume Capacity. Everything else is treated as [0].
                strCapacity = strCapacity.StartsWith('[') ? strCapacity.Substring(1, strCapacity.Length - 2) : "0";
                return decimal.TryParse(strCapacity, NumberStyles.Any, GlobalSettings.CultureInfo, out decimal decReturn)
                    ? decReturn
                    : 0;
            }
        }

        /// <summary>
        /// The Gear's Capacity cost if used as an Armor plugin.
        /// </summary>
        public decimal PluginArmorCapacity
        {
            get
            {
                string strCapacity = CalculatedArmorCapacity;
                // If this is a multiple-capacity item, use only the second half.
                int intPos = strCapacity.IndexOf("/[", StringComparison.Ordinal);
                if (intPos != -1)
                {
                    strCapacity = strCapacity.Substring(intPos + 1);
                }

                // Only items that contain square brackets should consume Capacity. Everything else is treated as [0].
                strCapacity = strCapacity.StartsWith('[') ? strCapacity.Substring(1, strCapacity.Length - 2) : "0";
                return strCapacity == "*" ? 0 : Convert.ToDecimal(strCapacity, GlobalSettings.CultureInfo);
            }
        }

        /// <summary>
        /// The amount of Capacity remaining in the Gear.
        /// </summary>
        public decimal CapacityRemaining
        {
            get
            {
                decimal decCapacity = 0;
                string strMyCapacity = CalculatedCapacity;
                int intPos = strMyCapacity.IndexOf("/[", StringComparison.Ordinal);
                if (intPos != -1 || !strMyCapacity.Contains('['))
                {
                    // Get the Gear base Capacity.
                    if (intPos != -1)
                    {
                        // If this is a multiple-capacity item, use only the first half.
                        strMyCapacity = strMyCapacity.Substring(0, intPos);
                        if (!decimal.TryParse(strMyCapacity, NumberStyles.Any, GlobalSettings.CultureInfo,
                            out decCapacity))
                            decCapacity = 0;
                    }
                    else if (!decimal.TryParse(strMyCapacity, NumberStyles.Any, GlobalSettings.CultureInfo,
                        out decCapacity))
                        decCapacity = 0;

                    if (Children.Count > 0)
                    {
                        // Run through its Children and deduct the Capacity costs.
                        decCapacity -= Children.Sum(x => x.PluginCapacity * x.Quantity);
                    }
                }

                return decCapacity;
            }
        }

        public string DisplayCapacity
        {
            get
            {
                if (Capacity.Contains('[') && !Capacity.Contains("/["))
                    return CalculatedCapacity;
                return string.Format(GlobalSettings.CultureInfo, LanguageManager.GetString("String_CapacityRemaining"),
                    CalculatedCapacity, CapacityRemaining.ToString("#,0.##", GlobalSettings.CultureInfo));
            }
        }

        /// <summary>
        /// The name of the object as it should appear on printouts (translated name only).
        /// </summary>
        public string DisplayNameShort(string strLanguage)
        {
            if (strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                return Name;

            XPathNavigator xmlGearDataNode = this.GetNodeXPath(strLanguage);
            if (xmlGearDataNode?.SelectSingleNodeAndCacheExpression("name")?.Value == "Custom Item")
            {
                return _objCharacter.TranslateExtra(Name, strLanguage);
            }

            return xmlGearDataNode?.SelectSingleNodeAndCacheExpression("translate")?.Value ?? Name;
        }

        /// <summary>
        /// The name of the object as it should appear on printouts (translated name only).
        /// </summary>
        public async Task<string> DisplayNameShortAsync(string strLanguage, CancellationToken token = default)
        {
            if (strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                return Name;

            XPathNavigator xmlGearDataNode = await this.GetNodeXPathAsync(strLanguage, token: token).ConfigureAwait(false);
            if (xmlGearDataNode?.SelectSingleNodeAndCacheExpression("name", token)?.Value == "Custom Item")
            {
                return await _objCharacter.TranslateExtraAsync(Name, strLanguage, token: token).ConfigureAwait(false);
            }

            return xmlGearDataNode?.SelectSingleNodeAndCacheExpression("translate", token: token)?.Value ?? Name;
        }

        /// <summary>
        /// Quantity to show, formatted for display purposes.
        /// </summary>
        /// <param name="objCulture">CultureInfo in which number formatting should be done.</param>
        /// <param name="blnBlankOnOneQuantity">Whether to return an empty string if there is only one of an item.</param>
        /// <param name="blnOverrideQuantity">Whether or not to override the quantity to display.</param>
        /// <param name="decQuantityToUse">If <paramref name="blnOverrideQuantity"/> is true, the override value to use for it.</param>
        /// <returns>A formatted string for the quantity of this piece of gear.</returns>
        public string DisplayQuantity(CultureInfo objCulture, bool blnBlankOnOneQuantity = false, bool blnOverrideQuantity = false,
                                      decimal decQuantityToUse = 0.0m)
        {
            if (!blnOverrideQuantity)
                decQuantityToUse = Quantity;

            if (Name.StartsWith("Nuyen", StringComparison.Ordinal))
                return decQuantityToUse.ToString(_objCharacter.Settings.NuyenFormat, objCulture);
            if (Category == "Currency")
                return decQuantityToUse.ToString("#,0.00", objCulture);
            return !blnBlankOnOneQuantity || Quantity != 1.0m ? decQuantityToUse.ToString("#,0.##", objCulture) : string.Empty;
        }

        /// <summary>
        /// The name of the object as it should be displayed in lists. Qty Name (Rating) (Extra).
        /// </summary>
        public string DisplayName(CultureInfo objCulture, string strLanguage, bool blnOverrideQuantity = false, decimal decQuantityToUse = 0.0m)
        {
            string strQuantity = DisplayQuantity(objCulture, true, blnOverrideQuantity, decQuantityToUse);
            string strReturn = DisplayNameShort(strLanguage);
            string strSpace = LanguageManager.GetString("String_Space", strLanguage);
            if (!string.IsNullOrEmpty(strQuantity))
                strReturn = strQuantity + strSpace + strReturn;

            if (Rating > 0)
                strReturn += strSpace + '('
                                      + string.Format(
                                          objCulture, LanguageManager.GetString("Label_RatingFormat", strLanguage),
                                          LanguageManager.GetString(RatingLabel, strLanguage)) + strSpace
                                      + Rating.ToString(objCulture) + ')';
            if (!string.IsNullOrEmpty(Extra))
                strReturn += strSpace + '(' + _objCharacter.TranslateExtra(Extra, strLanguage) + ')';
            if (!string.IsNullOrEmpty(GearName))
                strReturn += strSpace + "(\"" + GearName + "\")";
            if (LoadedIntoClip != null)
                strReturn += strSpace + '(' + string.Format(objCulture, LanguageManager.GetString("Label_Loaded"), LoadedIntoClip.DisplayWeaponName(objCulture, strLanguage)) + ')';
            return strReturn;
        }

        /// <summary>
        /// The name of the object as it should be displayed in lists. Qty Name (Rating) (Extra).
        /// </summary>
        public async Task<string> DisplayNameAsync(CultureInfo objCulture, string strLanguage, bool blnOverrideQuantity = false, decimal decQuantityToUse = 0.0m, CancellationToken token = default)
        {
            string strQuantity = DisplayQuantity(objCulture, true, blnOverrideQuantity, decQuantityToUse);
            string strReturn = await DisplayNameShortAsync(strLanguage, token).ConfigureAwait(false);
            string strSpace = await LanguageManager.GetStringAsync("String_Space", strLanguage, token: token).ConfigureAwait(false);
            if (!string.IsNullOrEmpty(strQuantity))
                strReturn = strQuantity + strSpace + strReturn;

            if (Rating > 0)
                strReturn += strSpace + '('
                                      + string.Format(
                                          objCulture,
                                          await LanguageManager
                                                .GetStringAsync("Label_RatingFormat", strLanguage, token: token)
                                                .ConfigureAwait(false),
                                          await LanguageManager.GetStringAsync(RatingLabel, strLanguage, token: token)
                                                               .ConfigureAwait(false)) + strSpace
                                      + Rating.ToString(objCulture) + ')';
            if (!string.IsNullOrEmpty(Extra))
                strReturn += strSpace + '(' + await _objCharacter.TranslateExtraAsync(Extra, strLanguage, token: token).ConfigureAwait(false) + ')';
            if (!string.IsNullOrEmpty(GearName))
                strReturn += strSpace + "(\"" + GearName + "\")";
            if (LoadedIntoClip != null)
                strReturn += strSpace + '('
                                      + string.Format(
                                          objCulture,
                                          await LanguageManager.GetStringAsync("Label_Loaded", token: token).ConfigureAwait(false),
                                          await LoadedIntoClip.DisplayWeaponNameAsync(objCulture, strLanguage, token).ConfigureAwait(false))
                                      + ')';
            return strReturn;
        }

        public string CurrentDisplayNameShort => DisplayNameShort(GlobalSettings.Language);

        public Task<string> GetCurrentDisplayNameShortAsync(CancellationToken token = default) => DisplayNameShortAsync(GlobalSettings.Language, token: token);

        public string CurrentDisplayName => DisplayName(GlobalSettings.CultureInfo, GlobalSettings.Language);

        public Task<string> GetCurrentDisplayNameAsync(CancellationToken token = default) => DisplayNameAsync(GlobalSettings.CultureInfo, GlobalSettings.Language, token: token);

        /// <summary>
        /// Weapon Bonus Damage.
        /// </summary>
        public string WeaponBonusDamage(string strLanguage)
        {
            if (WeaponBonus == null)
                return string.Empty;
            string strReturn = WeaponBonus["damagereplace"]?.InnerText ?? "0";
            // Use the damagereplace value if applicable.
            if (strReturn == "0")
            {
                // Use the damage bonus if available, otherwise use 0.
                strReturn = WeaponBonus["damage"]?.InnerText ?? "0";

                // Attach the type if applicable.
                strReturn += WeaponBonus["damagetype"]?.InnerText ?? string.Empty;

                // If this does not start with "-", add a "+" to the string.
                if (!strReturn.StartsWith('-', '+'))
                    strReturn = '+' + strReturn;
            }

            // Translate the Avail string.
            if (!strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
            {
                strReturn = strReturn.CheapReplace("P",
                        () => LanguageManager.GetString("String_DamagePhysical", strLanguage))
                    .CheapReplace("S", () => LanguageManager.GetString("String_DamageStun", strLanguage));
            }

            return strReturn;
        }

        /// <summary>
        /// Weapon Bonus Damage.
        /// </summary>
        public async Task<string> WeaponBonusDamageAsync(string strLanguage, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (WeaponBonus == null)
                return string.Empty;
            string strReturn = WeaponBonus["damagereplace"]?.InnerText ?? "0";
            // Use the damagereplace value if applicable.
            if (strReturn == "0")
            {
                // Use the damage bonus if available, otherwise use 0.
                strReturn = WeaponBonus["damage"]?.InnerText ?? "0";

                // Attach the type if applicable.
                strReturn += WeaponBonus["damagetype"]?.InnerText ?? string.Empty;

                // If this does not start with "-", add a "+" to the string.
                if (!strReturn.StartsWith('-', '+'))
                    strReturn = '+' + strReturn;
            }

            // Translate the Avail string.
            if (!strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
            {
                strReturn = await strReturn
                                  .CheapReplaceAsync(
                                      "P", () => LanguageManager.GetStringAsync("String_DamagePhysical", strLanguage, token: token), token: token)
                                  .CheapReplaceAsync(
                                      "S", () => LanguageManager.GetStringAsync("String_DamageStun", strLanguage, token: token), token: token).ConfigureAwait(false);
            }

            return strReturn;
        }

        /// <summary>
        /// Weapon Bonus AP.
        /// </summary>
        public string WeaponBonusAP
        {
            get
            {
                if (WeaponBonus == null)
                    return string.Empty;
                // Use the apreplace value if applicable.
                // Use the ap bonus if available, otherwise use 0.
                string strReturn = WeaponBonus["apreplace"]?.InnerText ?? WeaponBonus["ap"]?.InnerText ?? "0";

                // If this does not start with "-", add a "+" to the string.
                if (!strReturn.StartsWith('-', '+'))
                    strReturn = '+' + strReturn;

                return strReturn;
            }
        }

        /// <summary>
        /// Weapon Bonus Accuracy.
        /// </summary>
        public string WeaponBonusAcc
        {
            get
            {
                if (WeaponBonus == null)
                    return string.Empty;
                // Use the apreplace value if applicable.
                // Use the ap bonus if available, otherwise use 0.
                string strReturn = WeaponBonus["accuracyreplace"]?.InnerText
                                   ?? WeaponBonus["accuracy"]?.InnerText ?? "0";

                // If this does not start with "-", add a "+" to the string.
                if (!strReturn.StartsWith('-', '+'))
                    strReturn = '+' + strReturn;

                return strReturn;
            }
        }

        /// <summary>
        /// Weapon Bonus Range.
        /// </summary>
        public int WeaponBonusRange
        {
            get
            {
                int intReturn = 0;
                WeaponBonus?.TryGetInt32FieldQuickly("rangebonus", ref intReturn);
                return intReturn;
            }
        }

        /// <summary>
        /// Weapon Bonus Dice pool.
        /// </summary>
        public int WeaponBonusPool
        {
            get
            {
                int intReturn = 0;
                WeaponBonus?.TryGetInt32FieldQuickly("pool", ref intReturn);
                return intReturn;
            }
        }

        /// <summary>
        /// Weapon Bonus Dice pool for smartlink only.
        /// </summary>
        public int WeaponBonusSmartlinkPool
        {
            get
            {
                int intReturn = 0;
                WeaponBonus?.TryGetInt32FieldQuickly("smartlinkpool", ref intReturn);
                return intReturn;
            }
        }

        /// <summary>
        /// Weapon Bonus Damage.
        /// </summary>
        public string FlechetteWeaponBonusDamage(string strLanguage)
        {
            if (FlechetteWeaponBonus == null)
                return string.Empty;
            string strReturn = FlechetteWeaponBonus["damagereplace"]?.InnerText ?? "0";
            // Use the damagereplace value if applicable.
            if (strReturn == "0")
            {
                // Use the damage bonus if available, otherwise use 0.
                strReturn = FlechetteWeaponBonus["damage"]?.InnerText ?? "0";

                // Attach the type if applicable.
                strReturn += FlechetteWeaponBonus["damagetype"]?.InnerText ?? string.Empty;

                // If this does not start with "-", add a "+" to the string.
                if (!strReturn.StartsWith('-'))
                    strReturn = '+' + strReturn;
            }

            // Translate the Avail string.
            if (!strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
            {
                strReturn = strReturn.CheapReplace("P",
                        () => LanguageManager.GetString("String_DamagePhysical", strLanguage))
                    .CheapReplace("S", () => LanguageManager.GetString("String_DamageStun", strLanguage));
            }

            return strReturn;
        }

        /// <summary>
        /// Weapon Bonus Damage.
        /// </summary>
        public async Task<string> FlechetteWeaponBonusDamageAsync(string strLanguage, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (FlechetteWeaponBonus == null)
                return string.Empty;
            string strReturn = FlechetteWeaponBonus["damagereplace"]?.InnerText ?? "0";
            // Use the damagereplace value if applicable.
            if (strReturn == "0")
            {
                // Use the damage bonus if available, otherwise use 0.
                strReturn = FlechetteWeaponBonus["damage"]?.InnerText ?? "0";

                // Attach the type if applicable.
                strReturn += FlechetteWeaponBonus["damagetype"]?.InnerText ?? string.Empty;

                // If this does not start with "-", add a "+" to the string.
                if (!strReturn.StartsWith('-'))
                    strReturn = '+' + strReturn;
            }

            // Translate the Avail string.
            if (!strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
            {
                strReturn = await strReturn.CheapReplaceAsync("P", () => LanguageManager.GetStringAsync("String_DamagePhysical", strLanguage, token: token), token: token)
                                           .CheapReplaceAsync("S", () => LanguageManager.GetStringAsync("String_DamageStun", strLanguage, token: token), token: token).ConfigureAwait(false);
            }

            return strReturn;
        }

        /// <summary>
        /// Weapon Bonus AP.
        /// </summary>
        public string FlechetteWeaponBonusAP
        {
            get
            {
                if (FlechetteWeaponBonus == null)
                    return string.Empty;
                // Use the apreplace value if applicable.
                // Use the ap bonus if available, otherwise use 0.
                string strReturn = FlechetteWeaponBonus["apreplace"]?.InnerText ??
                                   FlechetteWeaponBonus["ap"]?.InnerText ?? "0";

                // If this does not start with "-", add a "+" to the string.
                if (!strReturn.StartsWith('-'))
                    strReturn = '+' + strReturn;

                return strReturn;
            }
        }

        /// <summary>
        /// Weapon Bonus Accuracy.
        /// </summary>
        public string FlechetteWeaponBonusAcc
        {
            get
            {
                if (FlechetteWeaponBonus == null)
                    return string.Empty;
                // Use the apreplace value if applicable.
                // Use the ap bonus if available, otherwise use 0.
                string strReturn = FlechetteWeaponBonus["accuracyreplace"]?.InnerText ??
                                   FlechetteWeaponBonus["accuracy"]?.InnerText ?? "0";

                // If this does not start with "-", add a "+" to the string.
                if (!strReturn.StartsWith('-', '+'))
                    strReturn = '+' + strReturn;

                return strReturn;
            }
        }

        /// <summary>
        /// Weapon Bonus Range.
        /// </summary>
        public int FlechetteWeaponBonusRange
        {
            get
            {
                int intReturn = 0;
                FlechetteWeaponBonus?.TryGetInt32FieldQuickly("rangebonus", ref intReturn);
                return intReturn;
            }
        }

        /// <summary>
        /// Weapon Bonus Dice pool.
        /// </summary>
        public int FlechetteWeaponBonusPool
        {
            get
            {
                int intReturn = 0;
                FlechetteWeaponBonus?.TryGetInt32FieldQuickly("pool", ref intReturn);
                return intReturn;
            }
        }

        /// <summary>
        /// Weapon Bonus Dice pool for smartlink only.
        /// </summary>
        public int FlechetteWeaponBonusSmartlinkPool
        {
            get
            {
                int intReturn = 0;
                FlechetteWeaponBonus?.TryGetInt32FieldQuickly("smartlinkpool", ref intReturn);
                return intReturn;
            }
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
                if (Name == "Living Persona")
                {
                    string strExpression
                        = string.Concat(ImprovementManager
                                        .GetCachedImprovementListForValueOf(
                                            CharacterObject, Improvement.ImprovementType.LivingPersonaMatrixCM)
                                        .Select(x => x.ImprovedName));
                    intReturn += string.IsNullOrEmpty(strExpression) ? 0 : ProcessRatingString(strExpression);
                }

                intReturn += Children.Sum(g => g.Equipped, loopGear => loopGear.TotalBonusMatrixBoxes);
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

        #endregion Complex Properties

        #region Methods

        public bool IsIdenticalToOtherGear(Gear objOtherGear, bool blnIgnoreSuperficials = false)
        {
            if (objOtherGear == null)
                return false;
            return Name == objOtherGear.Name
                   && Category == objOtherGear.Category
                   && Rating == objOtherGear.Rating
                   && Extra == objOtherGear.Extra
                   && (blnIgnoreSuperficials
                       || (GearName == objOtherGear.GearName
                           && Notes == objOtherGear.Notes))
                   && Children.DeepMatch(objOtherGear.Children, x => x.Children, (x, y) => x.Quantity == y.Quantity
                       && x.IsIdenticalToOtherGear(y, blnIgnoreSuperficials));
        }

        /// <summary>
        /// Change the Equipped status of a piece of Gear and all of its children.
        /// </summary>
        /// <param name="blnEquipped">Whether or not the Gear should be marked as Equipped.</param>
        /// <param name="blnSkipEncumbranceOnPropertyChanged">Whether we should skip notifying our character that they should re-check their encumbrance. Set to `true` if this is a batch operation and there is going to be a refresh later anyway.</param>
        public void ChangeEquippedStatus(bool blnEquipped, bool blnSkipEncumbranceOnPropertyChanged = false)
        {
            if (_objCharacter?.IsLoading != false)
                return;
            if (blnEquipped)
            {
                // Add any Improvements from the Gear.
                if (Category != "Stacked Focus")
                {
                    if (!Category.EndsWith("Foci", StringComparison.Ordinal) || Bonded)
                    {
                        ImprovementManager.EnableImprovements(_objCharacter,
                                                              _objCharacter.Improvements.Where(x =>
                                                                  x.ImproveSource == Improvement.ImprovementSource
                                                                      .Gear && x.SourceName == InternalId));
                    }
                }
                else
                {
                    // Stacked Foci need to be handled a little differently.
                    _objCharacter.StackedFoci.ForEach(objStack =>
                    {
                        if (objStack.GearId == InternalId && objStack.Bonded)
                        {
                            string strStackInternalId = objStack.InternalId;
                            ImprovementManager.EnableImprovements(_objCharacter,
                                                                  _objCharacter.Improvements.Where(x =>
                                                                          x.ImproveSource
                                                                          == Improvement.ImprovementSource.StackedFocus
                                                                          &&
                                                                          x.SourceName == strStackInternalId));
                        }
                    });
                }
            }
            else
            {
                // Remove any Improvements from the Gear.
                if (Category != "Stacked Focus")
                    ImprovementManager.DisableImprovements(_objCharacter,
                                                           _objCharacter.Improvements.Where(x =>
                                                               x.ImproveSource == Improvement.ImprovementSource.Gear
                                                               && x.SourceName == InternalId));
                else
                {
                    // Stacked Foci need to be handled a little differently.
                    _objCharacter.StackedFoci.ForEach(objStack =>
                    {
                        if (objStack.GearId == InternalId)
                        {
                            string strStackInternalId = objStack.InternalId;
                            ImprovementManager.DisableImprovements(_objCharacter,
                                                                   _objCharacter.Improvements.Where(x =>
                                                                           x.ImproveSource
                                                                           == Improvement.ImprovementSource.StackedFocus
                                                                           &&
                                                                           x.SourceName == strStackInternalId));
                        }
                    });
                }
            }

            foreach (Gear objGear in Children)
                objGear.ChangeEquippedStatus(blnEquipped, true);

            if (!blnSkipEncumbranceOnPropertyChanged && (!string.IsNullOrEmpty(Weight)
                                                         || Children.DeepAny(
                                                             x => x.Children.Where(y => y.Equipped), x => !string.IsNullOrEmpty(x.Weight))))
                _objCharacter.OnPropertyChanged(nameof(Character.TotalCarriedWeight));
        }

        /// <summary>
        /// Change the Equipped status of a piece of Gear and all of its children.
        /// </summary>
        /// <param name="blnEquipped">Whether or not the Gear should be marked as Equipped.</param>
        /// <param name="blnSkipEncumbranceOnPropertyChanged">Whether we should skip notifying our character that they should re-check their encumbrance. Set to `true` if this is a batch operation and there is going to be a refresh later anyway.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public async Task ChangeEquippedStatusAsync(bool blnEquipped,
                                                         bool blnSkipEncumbranceOnPropertyChanged = false,
                                                         CancellationToken token = default)
        {
            if (_objCharacter?.IsLoading != false)
                return;
            if (blnEquipped)
            {
                // Add any Improvements from the Gear.
                if (Category != "Stacked Focus")
                {
                    if (!Category.EndsWith("Foci", StringComparison.Ordinal) || Bonded)
                    {
                        await ImprovementManager.EnableImprovementsAsync(_objCharacter,
                                                                         await _objCharacter.Improvements.ToListAsync(x =>
                                                                                 x.ImproveSource == Improvement.ImprovementSource
                                                                                     .Gear && x.SourceName == InternalId,
                                                                             token: token).ConfigureAwait(false), token).ConfigureAwait(false);
                    }
                }
                else
                {
                    // Stacked Foci need to be handled a little differently.
                    await _objCharacter.StackedFoci.ForEachAsync(async objStack =>
                    {
                        if (objStack.GearId == InternalId && objStack.Bonded)
                        {
                            string strStackInternalId = objStack.InternalId;
                            await ImprovementManager.EnableImprovementsAsync(_objCharacter,
                                                                             await _objCharacter.Improvements.ToListAsync(x =>
                                                                                     x.ImproveSource
                                                                                     == Improvement.ImprovementSource.StackedFocus
                                                                                     &&
                                                                                     x.SourceName == strStackInternalId,
                                                                                 token: token).ConfigureAwait(false), token).ConfigureAwait(false);
                        }
                    }, token).ConfigureAwait(false);
                }
            }
            else
            {
                // Remove any Improvements from the Gear.
                if (Category != "Stacked Focus")
                    await ImprovementManager.DisableImprovementsAsync(_objCharacter,
                                                                      await _objCharacter.Improvements.ToListAsync(x =>
                                                                              x.ImproveSource
                                                                              == Improvement.ImprovementSource.Gear
                                                                              && x.SourceName == InternalId,
                                                                          token: token).ConfigureAwait(false), token)
                                            .ConfigureAwait(false);
                else
                {
                    // Stacked Foci need to be handled a little differently.
                    await _objCharacter.StackedFoci.ForEachAsync(async objStack =>
                    {
                        if (objStack.GearId == InternalId)
                        {
                            string strStackInternalId = objStack.InternalId;
                            await ImprovementManager.DisableImprovementsAsync(_objCharacter,
                                                                              await _objCharacter.Improvements
                                                                                  .ToListAsync(x =>
                                                                                          x.ImproveSource
                                                                                          == Improvement
                                                                                              .ImprovementSource
                                                                                              .StackedFocus
                                                                                          &&
                                                                                          x.SourceName
                                                                                          == strStackInternalId,
                                                                                      token: token)
                                                                                  .ConfigureAwait(false), token)
                                                    .ConfigureAwait(false);
                        }
                    }, token).ConfigureAwait(false);
                }
            }

            foreach (Gear objGear in Children)
                await objGear.ChangeEquippedStatusAsync(blnEquipped, true, token).ConfigureAwait(false);

            if (!blnSkipEncumbranceOnPropertyChanged && (!string.IsNullOrEmpty(Weight)
                                                         || Children
                                                             .DeepAny(
                                                                 x => x.Children.Where(y => y.Equipped),
                                                                 x => x.Equipped && !string.IsNullOrEmpty(x.Weight))))
                _objCharacter.OnPropertyChanged(nameof(Character.TotalCarriedWeight));
        }

        /// <summary>
        /// Toggle the Wireless Bonus for this gear.
        /// </summary>
        public void RefreshWirelessBonuses()
        {
            if (_objCharacter?.IsLoading != false)
                return;
            if (!string.IsNullOrEmpty(WirelessBonus?.InnerText))
            {
                if (WirelessOn && Equipped && (Parent as IHasWirelessBonus)?.WirelessOn != false)
                {
                    if (WirelessBonus.SelectSingleNodeAndCacheExpressionAsNavigator("@mode")?.Value == "replace")
                    {
                        ImprovementManager.DisableImprovements(_objCharacter,
                                                               _objCharacter.Improvements.Where(x =>
                                                                   x.ImproveSource
                                                                   == Improvement.ImprovementSource
                                                                       .Gear &&
                                                                   x.SourceName == InternalId));
                    }

                    if (ImprovementManager.CreateImprovements(_objCharacter, Improvement.ImprovementSource.Gear,
                                                              InternalId + "Wireless", WirelessBonus, Rating,
                                                              CurrentDisplayNameShort)
                        && !string.IsNullOrEmpty(ImprovementManager.SelectedValue) && string.IsNullOrEmpty(_strExtra))
                        _strExtra = ImprovementManager.SelectedValue;
                }
                else
                {
                    if (WirelessBonus.SelectSingleNodeAndCacheExpressionAsNavigator("@mode")?.Value == "replace")
                    {
                        ImprovementManager.EnableImprovements(_objCharacter,
                                                              _objCharacter.Improvements.Where(x =>
                                                                  x.ImproveSource == Improvement.ImprovementSource
                                                                      .Gear &&
                                                                  x.SourceName == InternalId));
                    }

                    string strSourceNameToRemove = InternalId + "Wireless";
                    ImprovementManager.RemoveImprovements(_objCharacter,
                        _objCharacter.Improvements.Where(x =>
                            x.ImproveSource == Improvement.ImprovementSource.Gear &&
                            x.SourceName == strSourceNameToRemove).ToList());
                }
            }

            foreach (Gear objGear in Children)
                objGear.RefreshWirelessBonuses();
        }

        /// <summary>
        /// Toggle the Wireless Bonus for this gear.
        /// </summary>
        public async Task RefreshWirelessBonusesAsync(CancellationToken token = default)
        {
            if (_objCharacter?.IsLoading != false)
                return;
            if (!string.IsNullOrEmpty(WirelessBonus?.InnerText))
            {
                if (WirelessOn && Equipped && (Parent as IHasWirelessBonus)?.WirelessOn != false)
                {
                    if (WirelessBonus.SelectSingleNodeAndCacheExpressionAsNavigator("@mode", token)?.Value == "replace")
                    {
                        await ImprovementManager.DisableImprovementsAsync(_objCharacter,
                                                                          await _objCharacter.Improvements.ToListAsync(x =>
                                                                              x.ImproveSource
                                                                              == Improvement.ImprovementSource
                                                                                  .Gear &&
                                                                              x.SourceName == InternalId, token: token).ConfigureAwait(false), token).ConfigureAwait(false);
                    }

                    if (await ImprovementManager.CreateImprovementsAsync(
                            _objCharacter, Improvement.ImprovementSource.Gear,
                            InternalId + "Wireless", WirelessBonus, Rating,
                            await GetCurrentDisplayNameShortAsync(token).ConfigureAwait(false),
                            token: token).ConfigureAwait(false)
                        && !string.IsNullOrEmpty(ImprovementManager.SelectedValue) && string.IsNullOrEmpty(_strExtra))
                        _strExtra = ImprovementManager.SelectedValue;
                }
                else
                {
                    if (WirelessBonus.SelectSingleNodeAndCacheExpressionAsNavigator("@mode", token)?.Value == "replace")
                    {
                        await ImprovementManager.EnableImprovementsAsync(_objCharacter,
                                                                         await _objCharacter.Improvements.ToListAsync(x =>
                                                                             x.ImproveSource == Improvement.ImprovementSource
                                                                                 .Gear &&
                                                                             x.SourceName == InternalId, token: token).ConfigureAwait(false), token).ConfigureAwait(false);
                    }

                    string strSourceNameToRemove = InternalId + "Wireless";
                    await ImprovementManager.RemoveImprovementsAsync(_objCharacter,
                                                                     await _objCharacter.Improvements.ToListAsync(x =>
                                                                             x.ImproveSource
                                                                             == Improvement.ImprovementSource.Gear &&
                                                                             x.SourceName == strSourceNameToRemove,
                                                                         token).ConfigureAwait(false), token: token)
                                            .ConfigureAwait(false);
                }
            }

            foreach (Gear objGear in Children)
                await objGear.RefreshWirelessBonusesAsync(token).ConfigureAwait(false);
        }

        /// <summary>
        /// Recursive method to delete a piece of Gear and its Improvements from the character. Returns total extra cost removed unrelated to children.
        /// </summary>
        public decimal DeleteGear(bool blnDoRemoval = true)
        {
            if (blnDoRemoval)
            {
                switch (Parent)
                {
                    case IHasGear objHasChildren:
                        objHasChildren.GearChildren.Remove(this);
                        break;

                    case IHasChildren<Gear> objHasChildren:
                        objHasChildren.Children.Remove(this);
                        break;

                    default:
                        CharacterObject.Gear.Remove(this);
                        break;
                }
            }

            LoadedIntoClip = null;

            decimal decReturn = 0;
            // Remove any children the Gear may have.
            foreach (Gear objChild in Children)
                decReturn += objChild.DeleteGear(false);

            // Remove the Gear Weapon created by the Gear if applicable.
            if (!WeaponID.IsEmptyGuid())
            {
                foreach (Weapon objDeleteWeapon in _objCharacter.Weapons.DeepWhere(x => x.Children, x => x.ParentID == InternalId).ToList())
                {
                    decReturn += objDeleteWeapon.TotalCost + objDeleteWeapon.DeleteWeapon();
                }

                decReturn += _objCharacter.Vehicles.Sum(objVehicle =>
                {
                    decimal decInnerReturn = 0;
                    foreach (Weapon objDeleteWeapon in objVehicle.Weapons
                                                                 .DeepWhere(x => x.Children,
                                                                            x => x.ParentID == InternalId).ToList())
                    {
                        decInnerReturn += objDeleteWeapon.TotalCost + objDeleteWeapon.DeleteWeapon();
                    }

                    decInnerReturn += objVehicle.Mods.Sum(objMod =>
                    {
                        decimal decInnerReturn2 = 0;
                        foreach (Weapon objDeleteWeapon in objMod.Weapons
                                                                 .DeepWhere(x => x.Children,
                                                                            x => x.ParentID == InternalId).ToList())
                        {
                            decInnerReturn2 += objDeleteWeapon.TotalCost + objDeleteWeapon.DeleteWeapon();
                        }

                        return decInnerReturn2;
                    });

                    decInnerReturn += objVehicle.WeaponMounts.Sum(objMount =>
                    {
                        decimal decInnerReturn2 = 0;
                        foreach (Weapon objDeleteWeapon in objMount.Weapons
                                                                   .DeepWhere(x => x.Children,
                                                                              x => x.ParentID == InternalId).ToList())
                        {
                            decInnerReturn2 += objDeleteWeapon.TotalCost + objDeleteWeapon.DeleteWeapon();
                        }

                        decInnerReturn2 += objMount.Mods.Sum(objMod =>
                        {
                            decimal decInnerReturn3 = 0;
                            foreach (Weapon objDeleteWeapon in objMod.Weapons
                                                                     .DeepWhere(x => x.Children,
                                                                         x => x.ParentID == InternalId).ToList())
                            {
                                decInnerReturn3 += objDeleteWeapon.TotalCost + objDeleteWeapon.DeleteWeapon();
                            }

                            return decInnerReturn3;
                        });

                        return decInnerReturn2;
                    });

                    return decInnerReturn;
                });
            }

            decReturn +=
                ImprovementManager.RemoveImprovements(_objCharacter, Improvement.ImprovementSource.Gear, InternalId);

            switch (Category)
            {
                // If a Focus is being removed, make sure the actual Focus is being removed from the character as well.
                case "Foci":
                case "Metamagic Foci":
                    {
                        HashSet<Focus> lstRemoveFoci = new HashSet<Focus>();
                        _objCharacter.Foci.ForEach(objFocus =>
                        {
                            if (objFocus.GearObject == this)
                                lstRemoveFoci.Add(objFocus);
                        });

                        foreach (Focus objFocus in lstRemoveFoci)
                        {
                            /*
                            foreach (Power objPower in objCharacter.Powers)
                            {
                                if (objPower.BonusSource == objFocus.GearId)
                                {
                                    //objPower.FreeLevels -= (objFocus.Rating / 4);
                                }
                            }
                            */
                            _objCharacter.Foci.Remove(objFocus);
                        }

                        break;
                    }
                // If a Stacked Focus is being removed, make sure the Stacked Foci and its bonuses are being removed.
                case "Stacked Focus":
                    {
                        StackedFocus objStack = _objCharacter.StackedFoci.Find(x => x.GearId == InternalId);
                        if (objStack != null)
                        {
                            decReturn += ImprovementManager.RemoveImprovements(_objCharacter,
                                Improvement.ImprovementSource.StackedFocus, objStack.InternalId);
                            _objCharacter.StackedFoci.Remove(objStack);
                            objStack.Dispose();
                        }

                        break;
                    }
            }

            this.SetActiveCommlink(_objCharacter, false);

            DisposeSelf();

            return decReturn;
        }

        /// <summary>
        /// Recursive method to delete a piece of Gear and its Improvements from the character. Returns total extra cost removed unrelated to children.
        /// </summary>
        public async Task<decimal> DeleteGearAsync(bool blnDoRemoval = true, CancellationToken token = default)
        {
            if (blnDoRemoval)
            {
                switch (Parent)
                {
                    case IHasGear objHasChildren:
                        await objHasChildren.GearChildren.RemoveAsync(this, token).ConfigureAwait(false);
                        break;

                    case IHasChildren<Gear> objHasChildren:
                        await objHasChildren.Children.RemoveAsync(this, token).ConfigureAwait(false);
                        break;

                    default:
                        await CharacterObject.Gear.RemoveAsync(this, token).ConfigureAwait(false);
                        break;
                }
            }

            LoadedIntoClip = null;

            // Remove any children the Gear may have.
            decimal decReturn = await Children.SumAsync(x => x.DeleteGearAsync(false, token), token)
                                              .ConfigureAwait(false);

            // Remove the Gear Weapon created by the Gear if applicable.
            if (!WeaponID.IsEmptyGuid())
            {
                foreach (Weapon objDeleteWeapon in await _objCharacter.Weapons
                                                                      .DeepWhereAsync(
                                                                          x => x.Children,
                                                                          x => x.ParentID == InternalId, token)
                                                                      .ConfigureAwait(false))
                {
                    decReturn += await objDeleteWeapon.GetTotalCostAsync(token).ConfigureAwait(false)
                                 + await objDeleteWeapon.DeleteWeaponAsync(token: token).ConfigureAwait(false);
                }

                decReturn += await _objCharacter.Vehicles.SumAsync(async objVehicle =>
                {
                    decimal decInner = 0;
                    foreach (Weapon objDeleteWeapon in await objVehicle.Weapons
                                                                       .DeepWhereAsync(
                                                                           x => x.Children,
                                                                           x => x.ParentID == InternalId, token)
                                                                       .ConfigureAwait(false))
                    {
                        decInner += await objDeleteWeapon.GetTotalCostAsync(token).ConfigureAwait(false)
                                    + await objDeleteWeapon.DeleteWeaponAsync(token: token).ConfigureAwait(false);
                    }

                    decInner += await objVehicle.Mods.SumAsync(async objMod =>
                    {
                        decimal decInner2 = 0;
                        foreach (Weapon objDeleteWeapon in await objMod.Weapons
                                                                       .DeepWhereAsync(
                                                                           x => x.Children,
                                                                           x => x.ParentID == InternalId, token)
                                                                       .ConfigureAwait(false))
                        {
                            decInner2 += await objDeleteWeapon.GetTotalCostAsync(token).ConfigureAwait(false)
                                         + await objDeleteWeapon.DeleteWeaponAsync(token: token).ConfigureAwait(false);
                        }

                        return decInner2;
                    }, token).ConfigureAwait(false);

                    decInner += await objVehicle.WeaponMounts.SumAsync(async objMount =>
                    {
                        decimal decInner2 = 0;
                        foreach (Weapon objDeleteWeapon in await objMount.Weapons
                                                                         .DeepWhereAsync(
                                                                             x => x.Children,
                                                                             x => x.ParentID == InternalId, token)
                                                                         .ConfigureAwait(false))
                        {
                            decInner2 += await objDeleteWeapon.GetTotalCostAsync(token).ConfigureAwait(false)
                                         + await objDeleteWeapon.DeleteWeaponAsync(token: token).ConfigureAwait(false);
                        }

                        decInner2 += await objMount.Mods.SumAsync(async objMod =>
                        {
                            decimal decInner3 = 0;
                            foreach (Weapon objDeleteWeapon in await objMod.Weapons
                                                                           .DeepWhereAsync(
                                                                               x => x.Children,
                                                                               x => x.ParentID == InternalId, token)
                                                                           .ConfigureAwait(false))
                            {
                                decInner3 += await objDeleteWeapon.GetTotalCostAsync(token).ConfigureAwait(false)
                                             + await objDeleteWeapon.DeleteWeaponAsync(token: token)
                                                                    .ConfigureAwait(false);
                            }

                            return decInner3;
                        }, token).ConfigureAwait(false);

                        return decInner2;
                    }, token).ConfigureAwait(false);

                    return decInner;
                }, token).ConfigureAwait(false);
            }

            decReturn +=
                await ImprovementManager
                      .RemoveImprovementsAsync(_objCharacter, Improvement.ImprovementSource.Gear, InternalId, token)
                      .ConfigureAwait(false);

            switch (Category)
            {
                // If a Focus is being removed, make sure the actual Focus is being removed from the character as well.
                case "Foci":
                case "Metamagic Foci":
                {
                    HashSet<Focus> lstRemoveFoci = new HashSet<Focus>();
                    await _objCharacter.Foci.ForEachAsync(objFocus =>
                    {
                        if (objFocus.GearObject == this)
                            lstRemoveFoci.Add(objFocus);
                    }, token).ConfigureAwait(false);

                    foreach (Focus objFocus in lstRemoveFoci)
                    {
                        /*
                        foreach (Power objPower in objCharacter.Powers)
                        {
                            if (objPower.BonusSource == objFocus.GearId)
                            {
                                //objPower.FreeLevels -= (objFocus.Rating / 4);
                            }
                        }
                        */
                        await _objCharacter.Foci.RemoveAsync(objFocus, token).ConfigureAwait(false);
                    }

                    break;
                }
                // If a Stacked Focus is being removed, make sure the Stacked Foci and its bonuses are being removed.
                case "Stacked Focus":
                {
                    StackedFocus objStack = await _objCharacter.StackedFoci
                                                               .FindAsync(x => x.GearId == InternalId, token)
                                                               .ConfigureAwait(false);
                    if (objStack != null)
                    {
                        decReturn += await ImprovementManager.RemoveImprovementsAsync(_objCharacter,
                                                                 Improvement.ImprovementSource.StackedFocus,
                                                                 objStack.InternalId, token)
                                                             .ConfigureAwait(false);
                        await _objCharacter.StackedFoci.RemoveAsync(objStack, token).ConfigureAwait(false);
                        await objStack.DisposeAsync().ConfigureAwait(false);
                    }

                    break;
                }
            }

            await this.SetActiveCommlinkAsync(_objCharacter, false, token).ConfigureAwait(false);

            await DisposeSelfAsync().ConfigureAwait(false);

            return decReturn;
        }

        public async Task ReaddImprovements(TreeView treGears, StringBuilder sbdOutdatedItems,
                                                 IReadOnlyCollection<string> lstInternalIdFilter,
                                                 Improvement.ImprovementSource eSource
                                                     = Improvement.ImprovementSource.Gear, bool blnStackEquipped = true,
                                                 CancellationToken token = default)
        {
            // We're only re-apply improvements a list of items, not all of them
            if (lstInternalIdFilter?.Contains(InternalId) != false)
            {
                XmlNode objNode = await this.GetNodeAsync(token: token).ConfigureAwait(false);
                if (objNode != null)
                {
                    if (Category == "Stacked Focus")
                    {
                        StackedFocus objStack = await _objCharacter.StackedFoci
                                                                   .FindAsync(x => x.GearId == InternalId, token)
                                                                   .ConfigureAwait(false);
                        if (objStack != null)
                        {
                            foreach (Gear objFociGear in objStack.Gear)
                            {
                                await objFociGear.ReaddImprovements(treGears, sbdOutdatedItems, lstInternalIdFilter,
                                                                    Improvement.ImprovementSource.StackedFocus,
                                                                    blnStackEquipped, token).ConfigureAwait(false);
                            }
                        }
                    }

                    Bonus = objNode["bonus"];
                    WirelessBonus = objNode["wirelessbonus"];
                    if (blnStackEquipped && Equipped)
                    {
                        if (Bonus != null)
                        {
                            ImprovementManager.ForcedValue = Extra;
                            await ImprovementManager.CreateImprovementsAsync(
                                                        _objCharacter, eSource, InternalId, Bonus, Rating,
                                                        await GetCurrentDisplayNameShortAsync(token)
                                                            .ConfigureAwait(false), token: token)
                                                    .ConfigureAwait(false);
                            if (!string.IsNullOrEmpty(ImprovementManager.SelectedValue))
                            {
                                Extra = ImprovementManager.SelectedValue;
                                string strText = await GetCurrentDisplayNameShortAsync(token).ConfigureAwait(false);
                                await treGears.DoThreadSafeAsync(x =>
                                {
                                    TreeNode objGearNode = x.FindNode(InternalId);
                                    if (objGearNode != null)
                                        objGearNode.Text = strText;
                                }, token: token).ConfigureAwait(false);
                            }
                        }

                        if (WirelessOn && WirelessBonus != null)
                        {
                            ImprovementManager.ForcedValue = Extra;
                            if (await ImprovementManager.CreateImprovementsAsync(
                                    _objCharacter, eSource, InternalId, WirelessBonus,
                                    Rating, await GetCurrentDisplayNameShortAsync(token).ConfigureAwait(false),
                                    token: token).ConfigureAwait(false)
                                && !string.IsNullOrEmpty(ImprovementManager.SelectedValue))
                            {
                                Extra = ImprovementManager.SelectedValue;
                                string strText = await GetCurrentDisplayNameShortAsync(token).ConfigureAwait(false);
                                await treGears.DoThreadSafeAsync(x =>
                                {
                                    TreeNode objGearNode = x.FindNode(InternalId);
                                    if (objGearNode != null)
                                        objGearNode.Text = strText;
                                }, token: token).ConfigureAwait(false);
                            }
                        }
                    }
                }
                else
                {
                    sbdOutdatedItems?.AppendLine(await GetCurrentDisplayNameShortAsync(token).ConfigureAwait(false));
                }
            }

            foreach (Gear objChild in Children)
                await objChild
                      .ReaddImprovements(treGears, sbdOutdatedItems, lstInternalIdFilter, eSource, blnStackEquipped,
                                         token).ConfigureAwait(false);
        }

        /// <summary>
        /// Checks a nominated piece of gear for Availability requirements.
        /// </summary>
        /// <param name="dicRestrictedGearLimits">Dictionary of Restricted Gear availabilities still available with the amount of items that can still use that availability.</param>
        /// <param name="sbdAvailItems">StringBuilder used to list names of gear that are currently over the availability limit.</param>
        /// <param name="sbdRestrictedItems">StringBuilder used to list names of gear that are being used for Restricted Gear.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public async Task<int> CheckRestrictedGear(IDictionary<int, int> dicRestrictedGearLimits, StringBuilder sbdAvailItems, StringBuilder sbdRestrictedItems, CancellationToken token = default)
        {
            int intRestrictedCount = 0;
            if (!IncludedInParent)
            {
                AvailabilityValue objTotalAvail = await TotalAvailTupleAsync(token: token).ConfigureAwait(false);
                if (!objTotalAvail.AddToParent)
                {
                    int intAvailInt = objTotalAvail.Value;
                    if (intAvailInt > CharacterObject.Settings.MaximumAvailability)
                    {
                        int intLowestValidRestrictedGearAvail = -1;
                        foreach (int intValidAvail in dicRestrictedGearLimits.Keys)
                        {
                            if (intValidAvail >= intAvailInt && (intLowestValidRestrictedGearAvail < 0
                                                                 || intValidAvail < intLowestValidRestrictedGearAvail))
                                intLowestValidRestrictedGearAvail = intValidAvail;
                        }

                        decimal decIllegalQuantity = Quantity;
                        int intIllegalQuantityRounded = decIllegalQuantity.StandardRound();
                        int intRestrictedGearQuantityUsed = intIllegalQuantityRounded;
                        if (intLowestValidRestrictedGearAvail >= 0)
                        {
                            if (dicRestrictedGearLimits[intLowestValidRestrictedGearAvail] >= intIllegalQuantityRounded)
                            {
                                dicRestrictedGearLimits[intLowestValidRestrictedGearAvail] -= intIllegalQuantityRounded;
                                decIllegalQuantity -= intRestrictedGearQuantityUsed;
                            }
                            else
                            {
                                intRestrictedGearQuantityUsed = dicRestrictedGearLimits[intLowestValidRestrictedGearAvail];
                                decIllegalQuantity -= intRestrictedGearQuantityUsed;
                                dicRestrictedGearLimits.Remove(intLowestValidRestrictedGearAvail);
                            }
                        }
                        if (decIllegalQuantity > 0)
                        {
                            ++intRestrictedCount;
                            sbdAvailItems.AppendLine().Append("\t\t").Append(await DisplayNameAsync(GlobalSettings.CultureInfo, GlobalSettings.Language, true, decIllegalQuantity, token).ConfigureAwait(false));
                        }
                        if (intRestrictedGearQuantityUsed > 0)
                        {
                            string strNameToUse = Parent == null
                                ? await DisplayNameAsync(GlobalSettings.CultureInfo, GlobalSettings.Language, true,
                                                         intRestrictedGearQuantityUsed, token).ConfigureAwait(false)
                                : string.Format(GlobalSettings.CultureInfo, "{0}{1}({2})",
                                                await DisplayNameAsync(GlobalSettings.CultureInfo, GlobalSettings.Language, true,
                                                                       intRestrictedGearQuantityUsed, token).ConfigureAwait(false),
                                                await LanguageManager.GetStringAsync("String_Space", token: token).ConfigureAwait(false),
                                                Parent);
                            sbdRestrictedItems.AppendLine().Append("\t\t").Append(strNameToUse);
                        }
                    }
                }
            }

            intRestrictedCount += await Children
                                        .SumAsync(objChild =>
                                                objChild
                                                    .CheckRestrictedGear(
                                                        dicRestrictedGearLimits, sbdAvailItems, sbdRestrictedItems,
                                                        token), token: token)
                                        .ConfigureAwait(false);

            return intRestrictedCount;
        }

        #region UI Methods

        /// <summary>
        /// Collection of TreeNodes to update when a relevant property is changed
        /// </summary>
        public HashSet<TreeNode> LinkedTreeNodes { get; } = new HashSet<TreeNode>();

        /// <summary>
        /// Build up the Tree for the current piece of Gear and all of its children.
        /// </summary>
        /// <param name="cmsGear">ContextMenuStrip for the Gear to use.</param>
        /// <param name="cmsCustomGear">ContextMenuStrip for the Gear to use if it can be renamed the way Custom Gear can (in Create mode).</param>
        public TreeNode CreateTreeNode(ContextMenuStrip cmsGear, ContextMenuStrip cmsCustomGear)
        {
            if (!string.IsNullOrEmpty(ParentID) && !string.IsNullOrEmpty(Source) &&
                !_objCharacter.Settings.BookEnabled(Source))
                return null;

            TreeNode objNode = new TreeNode
            {
                Name = InternalId,
                Text = CurrentDisplayName,
                Tag = this,
                ContextMenuStrip = cmsCustomGear != null && AllowRename ? cmsCustomGear : cmsGear,
                ForeColor = PreferredColor,
                ToolTipText = Notes.WordWrap()
            };

            BuildChildrenGearTree(objNode, cmsGear, cmsCustomGear);

            return objNode;
        }

        public Color PreferredColor
        {
            get
            {
                if (!string.IsNullOrEmpty(Notes))
                {
                    return !string.IsNullOrEmpty(ParentID)
                        ? ColorManager.GenerateCurrentModeDimmedColor(NotesColor)
                        : ColorManager.GenerateCurrentModeColor(NotesColor);
                }

                return !string.IsNullOrEmpty(ParentID)
                    ? ColorManager.GrayText
                    : ColorManager.WindowText;
            }
        }

        public bool Stolen
        {
            get => _blnStolen;
            set => _blnStolen = value;
        }

        /// <summary>
        /// Build up the Tree for the current piece of Gear's children.
        /// </summary>
        /// <param name="objParentNode">Parent node to which to append children gear.</param>
        /// <param name="cmsGear">ContextMenuStrip for the Gear's children to use to use.</param>
        /// <param name="cmsCustomGear">ContextMenuStrip for the Gear's children to use if they can be renamed the way Custom Gear can (in Create mode).</param>
        public void BuildChildrenGearTree(TreeNode objParentNode, ContextMenuStrip cmsGear, ContextMenuStrip cmsCustomGear)
        {
            if (objParentNode == null)
                return;
            bool blnExpandNode = false;
            foreach (Gear objChild in Children)
            {
                TreeNode objChildNode = objChild.CreateTreeNode(cmsGear, cmsCustomGear);
                if (objChildNode != null)
                {
                    objParentNode.Nodes.Add(objChildNode);
                    if (objChild.ParentID != InternalId ||
                        this.GetNodeXPath()?.SelectSingleNodeAndCacheExpression("gears/@startcollapsed")?.Value != bool.TrueString)
                        blnExpandNode = true;
                }
            }

            if (blnExpandNode)
                objParentNode.Expand();
        }

        public void SetupChildrenGearsCollectionChanged(bool blnAdd, TreeView treGear, ContextMenuStrip cmsGear = null, ContextMenuStrip cmsCustomGear = null, NotifyCollectionChangedEventHandler funcMakeDirty = null)
        {
            if (blnAdd)
            {
                Task FuncDelegateBeforeClearToAdd(object x, NotifyCollectionChangedEventArgs y,
                    CancellationToken innerToken = default) =>
                    this.RefreshChildrenGearsClearBindings(treGear, y, innerToken);

                Task FuncDelegateToAdd(object x, NotifyCollectionChangedEventArgs y, CancellationToken innerToken = default) =>
                    this.RefreshChildrenGears(treGear, cmsGear, cmsCustomGear, null, y, funcMakeDirty, token: innerToken);

                Children.AddTaggedBeforeClearCollectionChanged(treGear, FuncDelegateBeforeClearToAdd);
                Children.AddTaggedCollectionChanged(treGear, FuncDelegateToAdd);
                if (funcMakeDirty != null)
                    Children.AddTaggedCollectionChanged(treGear, funcMakeDirty);
                foreach (Gear objChild in Children)
                {
                    objChild.SetupChildrenGearsCollectionChanged(true, treGear, cmsGear, cmsCustomGear, funcMakeDirty);
                }
            }
            else
            {
                Children.RemoveTaggedAsyncBeforeClearCollectionChanged(treGear);
                Children.RemoveTaggedAsyncCollectionChanged(treGear);
                foreach (Gear objChild in Children)
                {
                    objChild.SetupChildrenGearsCollectionChanged(false, treGear);
                }
            }
        }

        public async Task SetupChildrenGearsCollectionChangedAsync(bool blnAdd, TreeView treGear, ContextMenuStrip cmsGear = null, ContextMenuStrip cmsCustomGear = null, NotifyCollectionChangedEventHandler funcMakeDirty = null, CancellationToken token = default)
        {
            if (blnAdd)
            {
                Task FuncDelegateBeforeClearToAdd(object x, NotifyCollectionChangedEventArgs y,
                    CancellationToken innerToken = default) =>
                    this.RefreshChildrenGearsClearBindings(treGear, y, innerToken);

                Task FuncDelegateToAdd(object x, NotifyCollectionChangedEventArgs y, CancellationToken innerToken = default) =>
                    this.RefreshChildrenGears(treGear, cmsGear, cmsCustomGear, null, y, funcMakeDirty, token: innerToken);

                Children.AddTaggedBeforeClearCollectionChanged(treGear, FuncDelegateBeforeClearToAdd);
                Children.AddTaggedCollectionChanged(treGear, FuncDelegateToAdd);
                if (funcMakeDirty != null)
                    Children.AddTaggedCollectionChanged(treGear, funcMakeDirty);
                await Children.ForEachAsync(
                    objChild => objChild.SetupChildrenGearsCollectionChangedAsync(true, treGear, cmsGear, cmsCustomGear,
                        funcMakeDirty, token), token).ConfigureAwait(false);
            }
            else
            {
                await Children.RemoveTaggedAsyncBeforeClearCollectionChangedAsync(treGear, token).ConfigureAwait(false);
                await Children.RemoveTaggedAsyncCollectionChangedAsync(treGear, token).ConfigureAwait(false);
                await Children.ForEachAsync(
                    objChild => objChild.SetupChildrenGearsCollectionChangedAsync(false, treGear, token: token), token).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Refreshes a single focus' rating (for changing ratings in create mode)
        /// </summary>
        /// <param name="treFoci">TreeView of foci.</param>
        /// <param name="intNewRating">New rating that the focus is supposed to have.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        /// <returns>True if the new rating complies by focus limits or the gear is not bonded, false otherwise</returns>
        public async Task<bool> RefreshSingleFocusRating(TreeView treFoci, int intNewRating, CancellationToken token = default)
        {
            if (Bonded)
            {
                int intMaxFocusTotal = await (await _objCharacter.GetAttributeAsync("MAG", token: token).ConfigureAwait(false)).GetTotalValueAsync(token).ConfigureAwait(false) * 5;
                if (await (await _objCharacter.GetSettingsAsync(token).ConfigureAwait(false)).GetMysAdeptSecondMAGAttributeAsync(token).ConfigureAwait(false) && await _objCharacter.GetIsMysticAdeptAsync(token).ConfigureAwait(false))
                    intMaxFocusTotal = Math.Min(intMaxFocusTotal, await (await _objCharacter.GetAttributeAsync("MAGAdept", token: token).ConfigureAwait(false)).GetTotalValueAsync(token).ConfigureAwait(false) * 5);

                int intFociTotal = await (await _objCharacter.GetFociAsync(token).ConfigureAwait(false)).SumAsync(x => x.GearObject != this, x => x.Rating, token).ConfigureAwait(false);

                if (intFociTotal + intNewRating > intMaxFocusTotal && !await _objCharacter.GetIgnoreRulesAsync(token).ConfigureAwait(false))
                {
                    Program.ShowScrollableMessageBox(await LanguageManager.GetStringAsync("Message_FocusMaximumForce", token: token).ConfigureAwait(false),
                                                     await LanguageManager.GetStringAsync("MessageTitle_FocusMaximum", token: token).ConfigureAwait(false), MessageBoxButtons.OK,
                                                     MessageBoxIcon.Information);
                    return false;
                }
            }

            Rating = intNewRating;

            switch (Category)
            {
                case "Foci":
                case "Metamagic Foci":
                    {
                        TreeNode nodFocus = await treFoci.DoThreadSafeFuncAsync(x => x.FindNodeByTag(this), token: token).ConfigureAwait(false);
                        if (nodFocus != null)
                        {
                            string strText = (await GetCurrentDisplayNameAsync(token).ConfigureAwait(false)).Replace(
                                await LanguageManager.GetStringAsync(RatingLabel, token: token).ConfigureAwait(false),
                                await LanguageManager.GetStringAsync("String_Force", token: token).ConfigureAwait(false));
                            await treFoci.DoThreadSafeFuncAsync(() => nodFocus.Text = strText, token: token)
                                         .ConfigureAwait(false);
                        }
                    }
                    break;

                case "Stacked Focus":
                    {
                        ThreadSafeList<StackedFocus> lstStackedFoci
                            = await _objCharacter.GetStackedFociAsync(token).ConfigureAwait(false);
                        for (int i = await lstStackedFoci.GetCountAsync(token).ConfigureAwait(false) - 1; i >= 0; --i)
                        {
                            if (i >= await lstStackedFoci.GetCountAsync(token).ConfigureAwait(false))
                                continue;
                            StackedFocus objStack = await lstStackedFoci.GetValueAtAsync(i, token)
                                                                        .ConfigureAwait(false);
                            if (objStack.GearId != InternalId)
                                continue;
                            TreeNode nodFocus = await treFoci
                                                      .DoThreadSafeFuncAsync(
                                                          x => x.FindNode(objStack.InternalId), token: token)
                                                      .ConfigureAwait(false);
                            if (nodFocus != null)
                            {
                                string strText = (await GetCurrentDisplayNameAsync(token).ConfigureAwait(false)).Replace(
                                    await LanguageManager.GetStringAsync(RatingLabel, token: token).ConfigureAwait(false),
                                    await LanguageManager.GetStringAsync("String_Force", token: token)
                                                         .ConfigureAwait(false));
                                await treFoci.DoThreadSafeFuncAsync(() => nodFocus.Text = strText, token: token)
                                             .ConfigureAwait(false);
                            }

                            break;
                        }
                    }
                    break;
            }

            return true;
        }

        #endregion UI Methods

        #region Hero Lab Importing Methods

        public bool ImportHeroLabGear(XPathNavigator xmlGearImportNode, XmlNode xmlParentGearNode,
            IList<Weapon> lstWeapons, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (xmlGearImportNode == null)
                return false;
            string strOriginalName = xmlGearImportNode.SelectSingleNodeAndCacheExpression("@name", token)?.Value ?? string.Empty;
            if (!string.IsNullOrEmpty(strOriginalName))
            {
                XmlDocument xmlGearDocument = _objCharacter.LoadData("gear.xml", token: token);
                string strForceValue = string.Empty;
                XmlNode xmlGearDataNode = null;
                using (XmlNodeList xmlGearDataList =
                    xmlGearDocument.SelectNodes("/chummer/gears/gear[contains(name, " + strOriginalName.CleanXPath() +
                                                ")]"))
                {
                    if (xmlGearDataList?.Count > 0)
                    {
                        foreach (XmlNode xmlLoopNode in xmlGearDataList)
                        {
                            token.ThrowIfCancellationRequested();
                            XPathNavigator xmlTestNode = xmlLoopNode.SelectSingleNodeAndCacheExpressionAsNavigator("forbidden/parentdetails", token);
                            if (xmlTestNode != null && xmlParentGearNode.ProcessFilterOperationNode(xmlTestNode, false))
                            {
                                // Assumes topmost parent is an AND node
                                continue;
                            }

                            xmlTestNode = xmlLoopNode.SelectSingleNodeAndCacheExpressionAsNavigator("required/parentdetails", token);
                            if (xmlTestNode != null &&
                                !xmlParentGearNode.ProcessFilterOperationNode(xmlTestNode, false))
                            {
                                // Assumes topmost parent is an AND node
                                continue;
                            }

                            xmlTestNode = xmlLoopNode.SelectSingleNodeAndCacheExpressionAsNavigator("forbidden/geardetails", token);
                            if (xmlTestNode != null && xmlParentGearNode.ProcessFilterOperationNode(xmlTestNode, false))
                            {
                                // Assumes topmost parent is an AND node
                                continue;
                            }

                            xmlTestNode = xmlLoopNode.SelectSingleNodeAndCacheExpressionAsNavigator("required/geardetails", token);
                            if (xmlTestNode != null &&
                                !xmlParentGearNode.ProcessFilterOperationNode(xmlTestNode, false))
                            {
                                // Assumes topmost parent is an AND node
                                continue;
                            }

                            xmlGearDataNode = xmlLoopNode;
                            break;
                        }
                    }
                }

                if (xmlGearDataNode == null)
                {
                    string[] astrOriginalNameSplit = strOriginalName.Split(':', StringSplitOptions.RemoveEmptyEntries);
                    if (astrOriginalNameSplit.Length > 1)
                    {
                        string strName = astrOriginalNameSplit[0].Trim();
                        using (XmlNodeList xmlGearDataList =
                            xmlGearDocument.SelectNodes("/chummer/gears/gear[contains(name, " + strName.CleanXPath() +
                                                        ")]"))
                        {
                            if (xmlGearDataList?.Count > 0)
                            {
                                foreach (XmlNode xmlLoopNode in xmlGearDataList)
                                {
                                    token.ThrowIfCancellationRequested();
                                    XPathNavigator xmlTestNode = xmlLoopNode.SelectSingleNodeAndCacheExpressionAsNavigator("forbidden/parentdetails", token);
                                    if (xmlTestNode != null &&
                                        xmlParentGearNode.ProcessFilterOperationNode(xmlTestNode, false))
                                    {
                                        // Assumes topmost parent is an AND node
                                        continue;
                                    }

                                    xmlTestNode = xmlLoopNode.SelectSingleNodeAndCacheExpressionAsNavigator("required/parentdetails", token);
                                    if (xmlTestNode != null &&
                                        !xmlParentGearNode.ProcessFilterOperationNode(xmlTestNode, false))
                                    {
                                        // Assumes topmost parent is an AND node
                                        continue;
                                    }

                                    xmlTestNode = xmlLoopNode.SelectSingleNodeAndCacheExpressionAsNavigator("forbidden/geardetails", token);
                                    if (xmlTestNode != null &&
                                        xmlParentGearNode.ProcessFilterOperationNode(xmlTestNode, false))
                                    {
                                        // Assumes topmost parent is an AND node
                                        continue;
                                    }

                                    xmlTestNode = xmlLoopNode.SelectSingleNodeAndCacheExpressionAsNavigator("required/geardetails", token);
                                    if (xmlTestNode != null &&
                                        !xmlParentGearNode.ProcessFilterOperationNode(xmlTestNode, false))
                                    {
                                        // Assumes topmost parent is an AND node
                                        continue;
                                    }

                                    xmlGearDataNode = xmlLoopNode;
                                    break;
                                }
                            }
                        }

                        if (xmlGearDataNode != null)
                            strForceValue = astrOriginalNameSplit[1].Trim();
                    }

                    if (xmlGearDataNode == null)
                    {
                        astrOriginalNameSplit = strOriginalName.Split(',', StringSplitOptions.RemoveEmptyEntries);
                        if (astrOriginalNameSplit.Length > 1)
                        {
                            string strName = astrOriginalNameSplit[0].Trim();
                            using (XmlNodeList xmlGearDataList =
                                xmlGearDocument.SelectNodes("/chummer/gears/gear[contains(name, " +
                                                            strName.CleanXPath() + ")]"))
                            {
                                if (xmlGearDataList?.Count > 0)
                                {
                                    foreach (XmlNode xmlLoopNode in xmlGearDataList)
                                    {
                                        token.ThrowIfCancellationRequested();
                                        XPathNavigator xmlTestNode = xmlLoopNode.SelectSingleNodeAndCacheExpressionAsNavigator("forbidden/parentdetails", token);
                                        if (xmlTestNode != null &&
                                            xmlParentGearNode.ProcessFilterOperationNode(xmlTestNode, false))
                                        {
                                            // Assumes topmost parent is an AND node
                                            continue;
                                        }

                                        xmlTestNode = xmlLoopNode.SelectSingleNodeAndCacheExpressionAsNavigator("required/parentdetails", token);
                                        if (xmlTestNode != null &&
                                            !xmlParentGearNode.ProcessFilterOperationNode(xmlTestNode, false))
                                        {
                                            // Assumes topmost parent is an AND node
                                            continue;
                                        }

                                        xmlTestNode = xmlLoopNode.SelectSingleNodeAndCacheExpressionAsNavigator("forbidden/geardetails", token);
                                        if (xmlTestNode != null &&
                                            xmlParentGearNode.ProcessFilterOperationNode(xmlTestNode, false))
                                        {
                                            // Assumes topmost parent is an AND node
                                            continue;
                                        }

                                        xmlTestNode = xmlLoopNode.SelectSingleNodeAndCacheExpressionAsNavigator("required/geardetails", token);
                                        if (xmlTestNode != null &&
                                            !xmlParentGearNode.ProcessFilterOperationNode(xmlTestNode, false))
                                        {
                                            // Assumes topmost parent is an AND node
                                            continue;
                                        }

                                        xmlGearDataNode = xmlLoopNode;
                                        break;
                                    }
                                }
                            }

                            if (xmlGearDataNode != null)
                                strForceValue = astrOriginalNameSplit[1].Trim();
                        }
                    }
                }

                if (xmlGearDataNode != null)
                {
                    Create(xmlGearDataNode, xmlGearImportNode.SelectSingleNodeAndCacheExpression("@rating", token)?.ValueAsInt ?? 0, lstWeapons,
                        strForceValue);
                }
                else
                {
                    XmlNode xmlCustomGearDataNode =
                        xmlGearDocument.SelectSingleNode("/chummer/gears/gear[name = 'Custom Item']");
                    if (xmlCustomGearDataNode != null)
                    {
                        Create(xmlCustomGearDataNode, xmlGearImportNode.SelectSingleNodeAndCacheExpression("@rating", token)?.ValueAsInt ?? 0,
                            lstWeapons, strOriginalName);
                        Cost = xmlGearImportNode.SelectSingleNodeAndCacheExpression("gearcost/@value", token)?.Value;
                    }
                    else
                        return false;
                }

                if (InternalId.IsEmptyGuid())
                    return false;

                Quantity = xmlGearImportNode.SelectSingleNodeAndCacheExpression("@quantity", token)?.ValueAsInt ?? 1;
                Notes = xmlGearImportNode.SelectSingleNodeAndCacheExpression("description", token)?.Value;

                ProcessHeroLabGearPlugins(xmlGearImportNode, lstWeapons, token);

                return true;
            }

            return false;
        }

        public async Task<bool> ImportHeroLabGearAsync(XPathNavigator xmlGearImportNode, XmlNode xmlParentGearNode,
                                                       IList<Weapon> lstWeapons, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (xmlGearImportNode == null)
                return false;
            string strOriginalName
                = xmlGearImportNode.SelectSingleNodeAndCacheExpression("@name", token)?.Value ?? string.Empty;
            if (!string.IsNullOrEmpty(strOriginalName))
            {
                XmlDocument xmlGearDocument
                    = await _objCharacter.LoadDataAsync("gear.xml", token: token).ConfigureAwait(false);
                string strForceValue = string.Empty;
                XmlNode xmlGearDataNode = null;
                using (XmlNodeList xmlGearDataList =
                       xmlGearDocument.SelectNodes("/chummer/gears/gear[contains(name, " + strOriginalName.CleanXPath()
                                                   +
                                                   ")]"))
                {
                    if (xmlGearDataList?.Count > 0)
                    {
                        foreach (XmlNode xmlLoopNode in xmlGearDataList)
                        {
                            token.ThrowIfCancellationRequested();
                            XPathNavigator xmlTestNode = xmlLoopNode.SelectSingleNodeAndCacheExpressionAsNavigator("forbidden/parentdetails", token);
                            if (xmlTestNode != null && await xmlParentGearNode.ProcessFilterOperationNodeAsync(xmlTestNode, false, token).ConfigureAwait(false))
                            {
                                // Assumes topmost parent is an AND node
                                continue;
                            }

                            xmlTestNode = xmlLoopNode.SelectSingleNodeAndCacheExpressionAsNavigator("required/parentdetails", token);
                            if (xmlTestNode != null &&
                                !await xmlParentGearNode.ProcessFilterOperationNodeAsync(xmlTestNode, false, token).ConfigureAwait(false))
                            {
                                // Assumes topmost parent is an AND node
                                continue;
                            }

                            xmlTestNode = xmlLoopNode.SelectSingleNodeAndCacheExpressionAsNavigator("forbidden/geardetails", token);
                            if (xmlTestNode != null && await xmlParentGearNode.ProcessFilterOperationNodeAsync(xmlTestNode, false, token).ConfigureAwait(false))
                            {
                                // Assumes topmost parent is an AND node
                                continue;
                            }

                            xmlTestNode = xmlLoopNode.SelectSingleNodeAndCacheExpressionAsNavigator("required/geardetails", token);
                            if (xmlTestNode != null &&
                                !await xmlParentGearNode.ProcessFilterOperationNodeAsync(xmlTestNode, false, token).ConfigureAwait(false))
                            {
                                // Assumes topmost parent is an AND node
                                continue;
                            }

                            xmlGearDataNode = xmlLoopNode;
                            break;
                        }
                    }
                }

                if (xmlGearDataNode == null)
                {
                    string[] astrOriginalNameSplit = strOriginalName.Split(':', StringSplitOptions.RemoveEmptyEntries);
                    if (astrOriginalNameSplit.Length > 1)
                    {
                        string strName = astrOriginalNameSplit[0].Trim();
                        using (XmlNodeList xmlGearDataList =
                               xmlGearDocument.SelectNodes("/chummer/gears/gear[contains(name, " + strName.CleanXPath()
                                                           +
                                                           ")]"))
                        {
                            if (xmlGearDataList?.Count > 0)
                            {
                                foreach (XmlNode xmlLoopNode in xmlGearDataList)
                                {
                                    token.ThrowIfCancellationRequested();
                                    XPathNavigator xmlTestNode = xmlLoopNode.SelectSingleNodeAndCacheExpressionAsNavigator("forbidden/parentdetails", token);
                                    if (xmlTestNode != null &&
                                        await xmlParentGearNode.ProcessFilterOperationNodeAsync(xmlTestNode, false, token).ConfigureAwait(false))
                                    {
                                        // Assumes topmost parent is an AND node
                                        continue;
                                    }

                                    xmlTestNode = xmlLoopNode.SelectSingleNodeAndCacheExpressionAsNavigator("required/parentdetails", token);
                                    if (xmlTestNode != null &&
                                        !await xmlParentGearNode.ProcessFilterOperationNodeAsync(xmlTestNode, false, token).ConfigureAwait(false))
                                    {
                                        // Assumes topmost parent is an AND node
                                        continue;
                                    }

                                    xmlTestNode = xmlLoopNode.SelectSingleNodeAndCacheExpressionAsNavigator("forbidden/geardetails", token);
                                    if (xmlTestNode != null &&
                                        await xmlParentGearNode.ProcessFilterOperationNodeAsync(xmlTestNode, false, token).ConfigureAwait(false))
                                    {
                                        // Assumes topmost parent is an AND node
                                        continue;
                                    }

                                    xmlTestNode = xmlLoopNode.SelectSingleNodeAndCacheExpressionAsNavigator("required/geardetails", token);
                                    if (xmlTestNode != null &&
                                        !await xmlParentGearNode.ProcessFilterOperationNodeAsync(xmlTestNode, false, token).ConfigureAwait(false))
                                    {
                                        // Assumes topmost parent is an AND node
                                        continue;
                                    }

                                    xmlGearDataNode = xmlLoopNode;
                                    break;
                                }
                            }
                        }

                        if (xmlGearDataNode != null)
                            strForceValue = astrOriginalNameSplit[1].Trim();
                    }

                    if (xmlGearDataNode == null)
                    {
                        astrOriginalNameSplit = strOriginalName.Split(',', StringSplitOptions.RemoveEmptyEntries);
                        if (astrOriginalNameSplit.Length > 1)
                        {
                            string strName = astrOriginalNameSplit[0].Trim();
                            using (XmlNodeList xmlGearDataList =
                                   xmlGearDocument.SelectNodes("/chummer/gears/gear[contains(name, " +
                                                               strName.CleanXPath() + ")]"))
                            {
                                if (xmlGearDataList?.Count > 0)
                                {
                                    foreach (XmlNode xmlLoopNode in xmlGearDataList)
                                    {
                                        token.ThrowIfCancellationRequested();
                                        XPathNavigator xmlTestNode = xmlLoopNode.SelectSingleNodeAndCacheExpressionAsNavigator("forbidden/parentdetails", token);
                                        if (xmlTestNode != null &&
                                            await xmlParentGearNode.ProcessFilterOperationNodeAsync(xmlTestNode, false, token).ConfigureAwait(false))
                                        {
                                            // Assumes topmost parent is an AND node
                                            continue;
                                        }

                                        xmlTestNode = xmlLoopNode.SelectSingleNodeAndCacheExpressionAsNavigator("required/parentdetails", token);
                                        if (xmlTestNode != null &&
                                            !await xmlParentGearNode.ProcessFilterOperationNodeAsync(xmlTestNode, false, token).ConfigureAwait(false))
                                        {
                                            // Assumes topmost parent is an AND node
                                            continue;
                                        }

                                        xmlTestNode = xmlLoopNode.SelectSingleNodeAndCacheExpressionAsNavigator("forbidden/geardetails", token);
                                        if (xmlTestNode != null &&
                                            await xmlParentGearNode.ProcessFilterOperationNodeAsync(xmlTestNode, false, token).ConfigureAwait(false))
                                        {
                                            // Assumes topmost parent is an AND node
                                            continue;
                                        }

                                        xmlTestNode = xmlLoopNode.SelectSingleNodeAndCacheExpressionAsNavigator("required/geardetails", token);
                                        if (xmlTestNode != null &&
                                            !await xmlParentGearNode.ProcessFilterOperationNodeAsync(xmlTestNode, false, token).ConfigureAwait(false))
                                        {
                                            // Assumes topmost parent is an AND node
                                            continue;
                                        }

                                        xmlGearDataNode = xmlLoopNode;
                                        break;
                                    }
                                }
                            }

                            if (xmlGearDataNode != null)
                                strForceValue = astrOriginalNameSplit[1].Trim();
                        }
                    }
                }

                if (xmlGearDataNode != null)
                {
                    Create(xmlGearDataNode,
                           xmlGearImportNode.SelectSingleNodeAndCacheExpression("@rating", token)?.ValueAsInt ?? 0, lstWeapons,
                           strForceValue);
                }
                else
                {
                    XmlNode xmlCustomGearDataNode =
                        xmlGearDocument.SelectSingleNode("/chummer/gears/gear[name = 'Custom Item']");
                    if (xmlCustomGearDataNode != null)
                    {
                        Create(xmlCustomGearDataNode,
                               xmlGearImportNode.SelectSingleNodeAndCacheExpression("@rating", token)?.ValueAsInt ?? 0,
                               lstWeapons, strOriginalName);
                        Cost = xmlGearImportNode
                            .SelectSingleNodeAndCacheExpression("gearcost/@value", token)?.Value;
                    }
                    else
                        return false;
                }

                if (InternalId.IsEmptyGuid())
                    return false;

                Quantity = xmlGearImportNode.SelectSingleNodeAndCacheExpression("@quantity", token)?.ValueAsInt ?? 1;
                Notes = xmlGearImportNode.SelectSingleNodeAndCacheExpression("description", token)?.Value;

                await ProcessHeroLabGearPluginsAsync(xmlGearImportNode, lstWeapons, token).ConfigureAwait(false);

                return true;
            }

            return false;
        }

        public void ProcessHeroLabGearPlugins(XPathNavigator xmlGearImportNode, IList<Weapon> lstWeapons, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (xmlGearImportNode == null)
                return;
            foreach (string strPluginNodeName in Character.HeroLabPluginNodeNames)
            {
                token.ThrowIfCancellationRequested();
                foreach (XPathNavigator xmlPluginToAdd in xmlGearImportNode.Select(strPluginNodeName +
                    "/item[@useradded != \"no\"]"))
                {
                    token.ThrowIfCancellationRequested();
                    Gear objPlugin = new Gear(_objCharacter);
                    if (objPlugin.ImportHeroLabGear(xmlPluginToAdd, this.GetNode(token), lstWeapons, token))
                    {
                        objPlugin.Parent = this;
                        Children.Add(objPlugin);
                    }
                }

                foreach (XPathNavigator xmlPluginToAdd in xmlGearImportNode.Select(strPluginNodeName +
                    "/item[@useradded = \"no\"]"))
                {
                    token.ThrowIfCancellationRequested();
                    string strName = xmlPluginToAdd.SelectSingleNodeAndCacheExpression("@name", token)?.Value ?? string.Empty;
                    if (!string.IsNullOrEmpty(strName))
                    {
                        Gear objPlugin = Children.FirstOrDefault(x =>
                            x.IncludedInParent && (x.Name.Contains(strName) || strName.Contains(x.Name)));
                        if (objPlugin != null)
                        {
                            objPlugin.Quantity = xmlPluginToAdd.SelectSingleNodeAndCacheExpression("@quantity", token)?.ValueAsInt ?? 1;
                            objPlugin.Notes = xmlPluginToAdd.SelectSingleNodeAndCacheExpression("description", token)?.Value;
                            objPlugin.ProcessHeroLabGearPlugins(xmlPluginToAdd, lstWeapons, token);
                        }
                    }
                }
            }

            this.RefreshMatrixAttributeArray(_objCharacter);
        }

        public async Task ProcessHeroLabGearPluginsAsync(XPathNavigator xmlGearImportNode, IList<Weapon> lstWeapons, CancellationToken token = default)
        {
            if (xmlGearImportNode == null)
                return;
            foreach (string strPluginNodeName in Character.HeroLabPluginNodeNames)
            {
                foreach (XPathNavigator xmlPluginToAdd in xmlGearImportNode.Select(strPluginNodeName +
                             "/item[@useradded != \"no\"]"))
                {
                    Gear objPlugin = new Gear(_objCharacter);
                    if (await objPlugin.ImportHeroLabGearAsync(xmlPluginToAdd, await this.GetNodeAsync(token: token).ConfigureAwait(false), lstWeapons, token).ConfigureAwait(false))
                    {
                        objPlugin.Parent = this;
                        await Children.AddAsync(objPlugin, token).ConfigureAwait(false);
                    }
                }

                foreach (XPathNavigator xmlPluginToAdd in xmlGearImportNode.Select(strPluginNodeName +
                             "/item[@useradded = \"no\"]"))
                {
                    string strName = xmlPluginToAdd.SelectSingleNodeAndCacheExpression("@name", token)?.Value ?? string.Empty;
                    if (!string.IsNullOrEmpty(strName))
                    {
                        Gear objPlugin = await Children.FirstOrDefaultAsync(x => x.IncludedInParent && (x.Name.Contains(strName) || strName.Contains(x.Name)), token: token).ConfigureAwait(false);
                        if (objPlugin != null)
                        {
                            objPlugin.Quantity = xmlPluginToAdd.SelectSingleNodeAndCacheExpression("@quantity", token)?.ValueAsInt ?? 1;
                            objPlugin.Notes = xmlPluginToAdd.SelectSingleNodeAndCacheExpression("description", token)?.Value;
                            await objPlugin.ProcessHeroLabGearPluginsAsync(xmlPluginToAdd, lstWeapons, token).ConfigureAwait(false);
                        }
                    }
                }
            }

            await this.RefreshMatrixAttributeArrayAsync(_objCharacter, token).ConfigureAwait(false);
        }

        #endregion Hero Lab Importing Methods

        #endregion Methods

        #region static

        //A tree of dependencies. Once some of the properties are changed,
        //anything they depend on, also needs to raise OnChanged
        //This tree keeps track of dependencies
        private static readonly PropertyDependencyGraph<Gear> s_GearDependencyGraph =
            new PropertyDependencyGraph<Gear>(
                new DependencyGraphNode<string, Gear>(nameof(CurrentDisplayName),
                    new DependencyGraphNode<string, Gear>(nameof(DisplayName),
                        new DependencyGraphNode<string, Gear>(nameof(DisplayNameShort),
                            new DependencyGraphNode<string, Gear>(nameof(Name))
                        ),
                        new DependencyGraphNode<string, Gear>(nameof(Quantity)),
                        new DependencyGraphNode<string, Gear>(nameof(Rating)),
                        new DependencyGraphNode<string, Gear>(nameof(Extra)),
                        new DependencyGraphNode<string, Gear>(nameof(GearName))
                    )
                ),
                new DependencyGraphNode<string, Gear>(nameof(CurrentDisplayNameShort),
                    new DependencyGraphNode<string, Gear>(nameof(DisplayNameShort))
                ),
                new DependencyGraphNode<string, Gear>(nameof(PreferredColor),
                    new DependencyGraphNode<string, Gear>(nameof(Notes)),
                    new DependencyGraphNode<string, Gear>(nameof(ParentID))
                ),
                new DependencyGraphNode<string, Gear>(nameof(TotalWeight),
                    new DependencyGraphNode<string, Gear>(nameof(OwnWeight),
                        new DependencyGraphNode<string, Gear>(nameof(Rating), x => x.Equipped && x.Weight.ContainsAny("FixedValues", "Rating"))
                    ),
                    new DependencyGraphNode<string, Gear>(nameof(Quantity))
                )
            );

        #endregion static

        /// <summary>
        /// Recursive method to add a Gear's Improvements to a character when moving them from a Vehicle.
        /// </summary>
        public void AddGearImprovements()
        {
            if (Bonus != null || (WirelessOn && WirelessBonus != null))
            {
                string strForce = string.Empty;
                if (!string.IsNullOrEmpty(Extra))
                    strForce = Extra;
                ImprovementManager.ForcedValue = strForce;
                if (Bonus != null)
                    ImprovementManager.CreateImprovements(CharacterObject, Improvement.ImprovementSource.Gear,
                        InternalId, Bonus, Rating, CurrentDisplayNameShort);
                if (WirelessOn && WirelessBonus != null)
                    ImprovementManager.CreateImprovements(CharacterObject, Improvement.ImprovementSource.Gear,
                        InternalId, WirelessBonus, Rating, CurrentDisplayNameShort);
            }

            foreach (Gear objChild in Children)
                objChild.AddGearImprovements();
        }

        public bool Remove(bool blnConfirmDelete = true)
        {
            if (blnConfirmDelete && !CommonFunctions.ConfirmDelete(LanguageManager.GetString("Message_DeleteGear")))
                return false;

            DeleteGear();
            return true;
        }

        public async Task<bool> RemoveAsync(bool blnConfirmDelete = true, CancellationToken token = default)
        {
            if (blnConfirmDelete && !await CommonFunctions
                    .ConfirmDeleteAsync(
                        await LanguageManager
                            .GetStringAsync("Message_DeleteGear", token: token)
                            .ConfigureAwait(false), token).ConfigureAwait(false))
                return false;

            await DeleteGearAsync(token: token).ConfigureAwait(false);
            return true;
        }

        public bool Sell(decimal decPercentage, bool blnConfirmDelete)
        {
            if (!_objCharacter.Created)
                return Remove(blnConfirmDelete);

            if (blnConfirmDelete && !CommonFunctions.ConfirmDelete(LanguageManager.GetString("Message_DeleteGear")))
                return false;

            string strExpense = "String_ExpenseSoldGear";

            IHasCost objParent = null;
            if (Parent != null)
            {
                objParent = Parent as IHasCost;
                switch (Parent)
                {
                    case Armor _:
                    case ArmorMod _:
                        strExpense = "String_ExpenseSoldArmorGear";
                        break;
                    case Weapon _:
                    case WeaponAccessory _:
                        strExpense = "String_ExpenseSoldWeaponGear";
                        break;
                    case Cyberware _:
                        strExpense = "String_ExpenseSoldCyberwareGear";
                        break;
                    case Vehicle _:
                    case VehicleMod _:
                    case WeaponMount _:
                        strExpense = "String_ExpenseSoldVehicleGear";
                        break;
                }
            }

            decimal decAmount;
            if (objParent != null)
            {
                decimal decOriginal = objParent.TotalCost;
                decAmount = DeleteGear() * decPercentage;
                decAmount += (decOriginal - objParent.TotalCost) * decPercentage;
            }
            else
            {
                decimal decOriginal = TotalCost;
                decAmount = (DeleteGear() + decOriginal) * decPercentage;
            }
            // Create the Expense Log Entry for the sale.
            ExpenseLogEntry objExpense = new ExpenseLogEntry(CharacterObject);
            string strEntry = LanguageManager.GetString(strExpense);
            objExpense.Create(decAmount, strEntry + ' ' + CurrentDisplayNameShort, ExpenseType.Nuyen,
                DateTime.Now);
            CharacterObject.ExpenseEntries.AddWithSort(objExpense);
            CharacterObject.Nuyen += decAmount;
            return true;
        }

        public async Task<bool> SellAsync(decimal decPercentage, bool blnConfirmDelete,
            CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (!await _objCharacter.GetCreatedAsync(token).ConfigureAwait(false))
                return await RemoveAsync(blnConfirmDelete, token).ConfigureAwait(false);

            if (blnConfirmDelete && !await CommonFunctions
                    .ConfirmDeleteAsync(
                        await LanguageManager.GetStringAsync("Message_DeleteArmorMod", token: token).ConfigureAwait(false),
                        token).ConfigureAwait(false))
                return false;

            string strExpense = "String_ExpenseSoldGear";

            IHasCost objParent = null;
            if (Parent != null)
            {
                objParent = Parent as IHasCost;
                switch (Parent)
                {
                    case Armor _:
                    case ArmorMod _:
                        strExpense = "String_ExpenseSoldArmorGear";
                        break;
                    case Weapon _:
                    case WeaponAccessory _:
                        strExpense = "String_ExpenseSoldWeaponGear";
                        break;
                    case Cyberware _:
                        strExpense = "String_ExpenseSoldCyberwareGear";
                        break;
                    case Vehicle _:
                    case VehicleMod _:
                    case WeaponMount _:
                        strExpense = "String_ExpenseSoldVehicleGear";
                        break;
                }
            }

            decimal decAmount;
            if (objParent != null)
            {
                decimal decOriginal = await objParent.GetTotalCostAsync(token).ConfigureAwait(false);
                decAmount = await DeleteGearAsync(token: token).ConfigureAwait(false) * decPercentage;
                decAmount += (decOriginal - await objParent.GetTotalCostAsync(token).ConfigureAwait(false)) * decPercentage;
            }
            else
            {
                decimal decOriginal = await GetTotalCostAsync(token).ConfigureAwait(false);
                decAmount = (await DeleteGearAsync(token: token).ConfigureAwait(false) + decOriginal) * decPercentage;
            }

            // Create the Expense Log Entry for the sale.
            ExpenseLogEntry objExpense = new ExpenseLogEntry(_objCharacter);
            objExpense.Create(decAmount,
                await LanguageManager.GetStringAsync(strExpense, token: token).ConfigureAwait(false) +
                ' ' + await GetCurrentDisplayNameShortAsync(token).ConfigureAwait(false), ExpenseType.Nuyen,
                DateTime.Now);
            await _objCharacter.ExpenseEntries.AddWithSortAsync(objExpense, token: token).ConfigureAwait(false);
            await _objCharacter.ModifyNuyenAsync(decAmount, token).ConfigureAwait(false);
            return true;
        }

        public void SetSourceDetail(Control sourceControl)
        {
            if (_objCachedSourceDetail.Language != GlobalSettings.Language)
                _objCachedSourceDetail = default;
            SourceDetail.SetControl(sourceControl);
        }

        public Task SetSourceDetailAsync(Control sourceControl, CancellationToken token = default)
        {
            if (_objCachedSourceDetail.Language != GlobalSettings.Language)
                _objCachedSourceDetail = default;
            return SourceDetail.SetControlAsync(sourceControl, token);
        }

        [NotifyPropertyChangedInvocator]
        public void OnPropertyChanged([CallerMemberName] string strPropertyName = null)
        {
            this.OnMultiplePropertyChanged(strPropertyName);
        }

        public Task OnPropertyChangedAsync(string strPropertyName, CancellationToken token = default)
        {
            return this.OnMultiplePropertyChangedAsync(token, strPropertyName);
        }

        public void OnMultiplePropertyChanged(IReadOnlyCollection<string> lstPropertyNames)
        {
            HashSet<string> setNamesOfChangedProperties = null;
            try
            {
                foreach (string strPropertyName in lstPropertyNames)
                {
                    if (setNamesOfChangedProperties == null)
                        setNamesOfChangedProperties = s_GearDependencyGraph.GetWithAllDependents(this, strPropertyName, true);
                    else
                    {
                        foreach (string strLoopChangedProperty in s_GearDependencyGraph.GetWithAllDependentsEnumerable(
                                     this,
                                     strPropertyName))
                            setNamesOfChangedProperties.Add(strLoopChangedProperty);
                    }
                }

                if (setNamesOfChangedProperties == null || setNamesOfChangedProperties.Count == 0)
                    return;

                if (_setPropertyChangedAsync.Count > 0)
                {
                    List<PropertyChangedEventArgs> lstArgsList = setNamesOfChangedProperties.Select(x => new PropertyChangedEventArgs(x)).ToList();
                    List<Func<Task>> lstFuncs = new List<Func<Task>>(lstArgsList.Count * _setPropertyChangedAsync.Count);
                    foreach (PropertyChangedAsyncEventHandler objEvent in _setPropertyChangedAsync)
                    {
                        foreach (PropertyChangedEventArgs objArg in lstArgsList)
                            lstFuncs.Add(() => objEvent.Invoke(this, objArg));
                    }

                    Utils.RunWithoutThreadLock(lstFuncs);
                    if (PropertyChanged != null)
                    {
                        Utils.RunOnMainThread(() =>
                        {
                            if (PropertyChanged != null)
                            {
                                // ReSharper disable once AccessToModifiedClosure
                                foreach (PropertyChangedEventArgs objArgs in lstArgsList)
                                {
                                    PropertyChanged.Invoke(this, objArgs);
                                }
                            }
                        });
                    }
                }
                else if (PropertyChanged != null)
                {
                    Utils.RunOnMainThread(() =>
                    {
                        if (PropertyChanged != null)
                        {
                            // ReSharper disable once AccessToModifiedClosure
                            foreach (string strPropertyToChange in setNamesOfChangedProperties)
                            {
                                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(strPropertyToChange));
                            }
                        }
                    });
                }

                if (Equipped && ((setNamesOfChangedProperties.Contains(nameof(TotalWeight))
                                  && (!string.IsNullOrEmpty(Weight)
                                      || Children.DeepAny(x => x.Children.Where(y => y.Equipped), x => x.Equipped && !string.IsNullOrEmpty(x.Weight))))
                                 || (setNamesOfChangedProperties.Contains(nameof(Rating))
                                     && Children.Any(x => x.Equipped && x.Weight.Contains("Parent Rating")))))
                {
                    _objCharacter.OnPropertyChanged(nameof(Character.TotalCarriedWeight));
                }
            }
            finally
            {
                if (setNamesOfChangedProperties != null)
                    Utils.StringHashSetPool.Return(ref setNamesOfChangedProperties);
            }
        }

        public async Task OnMultiplePropertyChangedAsync(IReadOnlyCollection<string> lstPropertyNames,
            CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            HashSet<string> setNamesOfChangedProperties = null;
            try
            {
                foreach (string strPropertyName in lstPropertyNames)
                {
                    if (setNamesOfChangedProperties == null)
                        setNamesOfChangedProperties =
                            s_GearDependencyGraph.GetWithAllDependents(this, strPropertyName, true);
                    else
                    {
                        foreach (string strLoopChangedProperty in s_GearDependencyGraph.GetWithAllDependentsEnumerable(
                                     this,
                                     strPropertyName))
                            setNamesOfChangedProperties.Add(strLoopChangedProperty);
                    }
                }

                if (setNamesOfChangedProperties == null || setNamesOfChangedProperties.Count == 0)
                    return;

                if (_setPropertyChangedAsync.Count > 0)
                {
                    List<PropertyChangedEventArgs> lstArgsList = setNamesOfChangedProperties
                        .Select(x => new PropertyChangedEventArgs(x)).ToList();
                    List<Task> lstTasks = new List<Task>(Math.Min(lstArgsList.Count * _setPropertyChangedAsync.Count,
                        Utils.MaxParallelBatchSize));
                    int i = 0;
                    foreach (PropertyChangedAsyncEventHandler objEvent in _setPropertyChangedAsync)
                    {
                        foreach (PropertyChangedEventArgs objArg in lstArgsList)
                        {
                            lstTasks.Add(objEvent.Invoke(this, objArg, token));
                            if (++i < Utils.MaxParallelBatchSize)
                                continue;
                            await Task.WhenAll(lstTasks).ConfigureAwait(false);
                            lstTasks.Clear();
                            i = 0;
                        }
                    }

                    await Task.WhenAll(lstTasks).ConfigureAwait(false);

                    if (PropertyChanged != null)
                    {
                        await Utils.RunOnMainThreadAsync(() =>
                        {
                            if (PropertyChanged != null)
                            {
                                // ReSharper disable once AccessToModifiedClosure
                                foreach (PropertyChangedEventArgs objArgs in lstArgsList)
                                {
                                    token.ThrowIfCancellationRequested();
                                    PropertyChanged.Invoke(this, objArgs);
                                }
                            }
                        }, token).ConfigureAwait(false);
                    }
                }
                else if (PropertyChanged != null)
                {
                    await Utils.RunOnMainThreadAsync(() =>
                    {
                        if (PropertyChanged != null)
                        {
                            // ReSharper disable once AccessToModifiedClosure
                            foreach (string strPropertyToChange in setNamesOfChangedProperties)
                            {
                                token.ThrowIfCancellationRequested();
                                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(strPropertyToChange));
                            }
                        }
                    }, token).ConfigureAwait(false);
                }

                if (Equipped && ((setNamesOfChangedProperties.Contains(nameof(TotalWeight))
                                  && (!string.IsNullOrEmpty(Weight)
                                      || Children.DeepAny(x => x.Children.Where(y => y.Equipped),
                                          x => x.Equipped && !string.IsNullOrEmpty(x.Weight))))
                                 || (setNamesOfChangedProperties.Contains(nameof(Rating))
                                     && await Children.AnyAsync(x => x.Equipped && x.Weight.Contains("Parent Rating"),
                                         token: token).ConfigureAwait(false))))
                {
                    await _objCharacter.OnPropertyChangedAsync(nameof(Character.TotalCarriedWeight), token).ConfigureAwait(false);
                }
            }
            finally
            {
                if (setNamesOfChangedProperties != null)
                    Utils.StringHashSetPool.Return(ref setNamesOfChangedProperties);
            }
        }

        public bool AllowPasteXml
        {
            get
            {
                switch (GlobalSettings.ClipboardContentType)
                {
                    case ClipboardContentType.Gear:
                        {
                            XPathNodeIterator xmlAddonCategoryList = this.GetNodeXPath()?.SelectAndCacheExpression("addoncategory");
                            if (!(xmlAddonCategoryList?.Count > 0))
                                return false;
                            string strCategory = GlobalSettings.Clipboard["category"]?.InnerText;
                            return xmlAddonCategoryList.Cast<XPathNavigator>().Any(xmlCategory => xmlCategory.Value == strCategory);
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

        public TaggedObservableCollection<Gear> GearChildren => Children;

        /// <inheritdoc />
        public void Dispose()
        {
            foreach (Gear objChild in _lstChildren)
                objChild.Dispose();
            DisposeSelf();
        }

        private void DisposeSelf()
        {
            _lstChildren.Dispose();
        }

        /// <inheritdoc />
        public async ValueTask DisposeAsync()
        {
            foreach (Gear objChild in _lstChildren)
                await objChild.DisposeAsync().ConfigureAwait(false);
            await DisposeSelfAsync().ConfigureAwait(false);
        }

        private ValueTask DisposeSelfAsync()
        {
            return _lstChildren.DisposeAsync();
        }
    }
}
