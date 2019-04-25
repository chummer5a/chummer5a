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
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Xml;
// ReSharper disable StringLiteralTypo

namespace Chummer
{
    public class CharacterOptions : INotifyPropertyChanged
    {
        private readonly Character _character;
        private string _strFileName = "default.xml";
        private string _strName = "Default Settings";
        private string _strImageFolder = string.Empty;

        // Settings.
        // ReSharper disable once InconsistentNaming
        private bool _blnAllow2ndMaxAttribute;
        private bool _blnAllowBiowareSuites;
        private bool _blnAllowCyberwareESSDiscounts;
        private bool _blnAllowEditPartOfBaseWeapon;
        private bool _blnAllowHigherStackedFoci;
        private bool _blnAllowInitiationInCreateMode;
        private bool _blnAllowObsolescentUpgrade;
        private bool _blnAllowSkillDiceRolling;
        private bool _blnDontUseCyberlimbCalculation;
        private bool _blnAllowSkillRegrouping = true;
        private bool _blnAlternateMetatypeAttributeKarma;
        private bool _blnArmorDegradation;
        private bool _blnStrictSkillGroupsInCreateMode;
        private bool _blnAllowPointBuySpecializationsOnKarmaSkills;
        private bool _blnCalculateCommlinkResponse = true;
        private bool _blnConfirmDelete = true;
        private bool _blnConfirmKarmaExpense = true;
        private bool _blnCyberlegMovement;
        private bool _blnDontDoubleQualityPurchaseCost;
        private bool _blnDontDoubleQualityRefundCost;
        private bool _blnEnforceCapacity = true;
        private bool _blnESSLossReducesMaximumOnly;
        private bool _blnExceedNegativeQualities;
        private bool _blnExceedNegativeQualitiesLimit;
        private bool _blnExceedPositiveQualities;
        private bool _blnExceedPositiveQualitiesCostDoubled;
        private bool _blnExtendAnyDetectionSpell;
        private bool _blnFreeContactsMultiplierEnabled;
        private bool _blnDroneArmorMultiplierEnabled;
        private bool _blnFreeKnowledgeMultiplierEnabled;
        private bool _blnFreeSpiritPowerPointsMAG;
        private bool _blnNoArmorEncumbrance;
        private bool _blnIgnoreArt;
        private bool _blnIgnoreComplexFormLimit;
        private bool _blnUnarmedImprovementsApplyToWeapons;
        private bool _blnLicenseRestrictedItems;
        private bool _blnMaximumArmorModifications;
        private bool _blnMetatypeCostsKarma = true;
        private bool _blnMoreLethalGameplay;
        private bool _blnMultiplyForbiddenCost;
        private bool _blnMultiplyRestrictedCost;
        private bool _blnNoSingleArmorEncumbrance;
        private bool _blnPrintExpenses;
        private bool _blnPrintFreeExpenses = true;
        private bool _blnPrintNotes;
        private bool _blnPrintSkillsWithZeroRating = true;
        private bool _blnRestrictRecoil = true;
        private bool _blnSpecialKarmaCostBasedOnShownValue;
        private bool _blnSpiritForceBasedOnTotalMAG;
        private bool _blnUnrestrictedNuyen;
        private bool _blnUseCalculatedPublicAwareness;
        private bool _blnUsePointsOnBrokenGroups;
        private bool _blnUseTotalValueForFreeContacts;
        private bool _blnUseTotalValueForFreeKnowledge;
        private bool _blnDoNotRoundEssenceInternally;
        private bool _blnEnemyKarmaQualityLimit = true;
        private string _strEssenceFormat = "#,0.00";
        private int _intForbiddenCostMultiplier = 1;
        private readonly int _intFreeContactsFlatNumber = 0;
        private int _intFreeContactsMultiplier = 3;
        private int _intDroneArmorMultiplier = 2;
        private int _intFreeKnowledgeMultiplier = 2;
        private int _intLimbCount = 6;
        private int _intMetatypeCostMultiplier = 1;
        private decimal _decNuyenPerBP = 2000.0m;
        private int _intRestrictedCostMultiplier = 1;
        private bool _blnAutomaticBackstory = true;
        private bool _blnFreeMartialArtSpecialization;
        private bool _blnPrioritySpellsAsAdeptPowers;
        private bool _blnMysAdeptAllowPPCareer;
        private bool _blnMysAdeptSecondMAGAttribute;
        private bool _blnReverseAttributePriorityOrder;
        private bool _blnHideItemsOverAvailLimit = true;
        private bool _blnAllowHoverIncrement;
        private bool _blnSearchInCategoryOnly = true;
        private string _strNuyenFormat = "#,0.##";
        private bool _blnCompensateSkillGroupKarmaDifference;
        private bool _cyberwareRounding;
        private bool _increasedImprovedAbilityMultiplier;
        private bool _allowFreeGrids;
        private bool _blnAllowTechnomancerSchooling;
        private string _strBookXPath = string.Empty;
        private string _strExcludeLimbSlot = string.Empty;
        private int _intCyberlimbAttributeBonusCap = 4;

        // Karma variables.
        private int _intKarmaAttribute = 5;
        private int _intKarmaCarryover = 7;
        private int _intKarmaComplexFormOption = 2;
        private int _intKarmaComplexFormSkillfot = 1;
        private int _intKarmaContact = 1;
        private int _intKarmaEnemy = 1;
        private int _intKarmaEnhancement = 2;
        private int _intKarmaImproveActiveSkill = 2;
        private int _intKarmaImproveComplexForm = 1;
        private int _intKarmaImproveKnowledgeSkill = 1;
        private int _intKarmaImproveSkillGroup = 5;
        private int _intKarmaInitiation = 3;
        private int _intKarmaInitiationFlat = 10;
        private int _intKarmaJoinGroup = 5;
        private int _intKarmaLeaveGroup = 1;
        private int _intKarmaManeuver = 5;
        private int _intKarmaMetamagic = 15;
        private int _intKarmaNewActiveSkill = 2;
        private int _intKarmaNewComplexForm = 4;
        private int _intKarmaNewKnowledgeSkill = 1;
        private int _intKarmaNewSkillGroup = 5;
        private int _intKarmaNuyenPer = 2000;
        private int _intKarmaQuality = 1;
        private int _intKarmaSpecialization = 7;
        private int _intKarmaKnoSpecialization = 7;
        private int _intKarmaSpell = 5;
        private int _intKarmaSpirit = 1;
        private int _intKarmaNewAIProgram = 5;
        private int _intKarmaNewAIAdvancedProgram = 8;
        private int _intKarmaMysticAdeptPowerPoint = 5;

        // Karma Foci variables.
        // Enchanting
        private int _intKarmaAlchemicalFocus = 3;
        private int _intKarmaDisenchantingFocus = 3;
        // Metamagic
        private int _intKarmaCenteringFocus = 3;
        private int _intKarmaFlexibleSignatureFocus = 3;
        private int _intKarmaMaskingFocus = 3;
        private int _intKarmaSpellShapingFocus = 3;
        // Power
        private int _intKarmaPowerFocus = 6;
        // Qi
        private int _intKarmaQiFocus = 2;
        // Spell
        private int _intKarmaCounterspellingFocus = 2;
        private int _intKarmaRitualSpellcastingFocus = 2;
        private int _intKarmaSpellcastingFocus = 2;
        private int _intKarmaSustainingFocus = 2;
        // Spirit
        private int _intKarmaBanishingFocus = 2;
        private int _intKarmaBindingFocus = 2;
        private int _intKarmaSummoningFocus = 2;
        // Weapon
        private int _intKarmaWeaponFocus = 3;

        // Default build settings.
        private string _strBuildMethod = "Karma";
        private int _intBuildPoints = 800;
        private int _intAvailability = 12;

        // List of names of custom data directories
        private readonly Dictionary<string,bool> _dicCustomDataDirectoryNames = new Dictionary<string, bool>();

        // Sourcebook list.
        private readonly HashSet<string> _lstBooks = new HashSet<string>();

        public event PropertyChangedEventHandler PropertyChanged;

        #region Initialization, Save, and Load Methods
        public CharacterOptions(Character character)
        {
            _character = character;

            if (Utils.IsRunningInVisualStudio)
                return;

            // Create the settings directory if it does not exist.
            string settingsDirectoryPath = Path.Combine(Utils.GetStartupPath, "settings");
            if (!Directory.Exists(settingsDirectoryPath))
            {
                try
                {
                    Directory.CreateDirectory(settingsDirectoryPath);
                }
                catch (UnauthorizedAccessException)
                {
                    Program.MainForm.ShowMessageBox(LanguageManager.GetString("Message_Insufficient_Permissions_Warning", GlobalOptions.Language));
                }
            }

            // If the default.xml settings file does not exist, attempt to read the settings from the Registry (old storage format), then save them to the default.xml file.
            string strFilePath = Path.Combine(settingsDirectoryPath, "default.xml");
            if (!File.Exists(strFilePath) || !Load("default.xml"))
            {
                _strFileName = "default.xml";
                LoadFromRegistry();
                Save();
            }
        }

        /// <summary>
        /// Save the current settings to the settings file.
        /// </summary>
        public void Save()
        {
            string strFilePath = Path.Combine(Utils.GetStartupPath, "settings", _strFileName);
            FileStream objStream = new FileStream(strFilePath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
            XmlTextWriter objWriter = new XmlTextWriter(objStream, Encoding.UTF8)
            {
                Formatting = Formatting.Indented,
                Indentation = 1,
                IndentChar = '\t'
            };
            objWriter.WriteStartDocument();

            // <settings>
            objWriter.WriteStartElement("settings");

            // <name />
            objWriter.WriteElementString("name", _strName);
            // <recentimagefolder />
            if (!string.IsNullOrEmpty(_strImageFolder))
            {
                objWriter.WriteElementString("recentimagefolder", _strImageFolder);
            }
            // <confirmdelete />
            objWriter.WriteElementString("confirmdelete", _blnConfirmDelete.ToString());
            // <licenserestricted />
            objWriter.WriteElementString("licenserestricted", _blnLicenseRestrictedItems.ToString());
            // <confirmkarmaexpense />
            objWriter.WriteElementString("confirmkarmaexpense", _blnConfirmKarmaExpense.ToString());
            // <printzeroratingskills />
            objWriter.WriteElementString("printzeroratingskills", _blnPrintSkillsWithZeroRating.ToString());
            // <morelethalgameplay />
            objWriter.WriteElementString("morelethalgameplay", _blnMoreLethalGameplay.ToString());
            // <spiritforcebasedontotalmag />
            objWriter.WriteElementString("spiritforcebasedontotalmag", _blnSpiritForceBasedOnTotalMAG.ToString());
            // <printexpenses />
            objWriter.WriteElementString("printexpenses", _blnPrintExpenses.ToString());
            // <printfreeexpenses />
            objWriter.WriteElementString("printfreeexpenses", _blnPrintFreeExpenses.ToString());
            // <nuyenperbp />
            objWriter.WriteElementString("nuyenperbp", _decNuyenPerBP.ToString(GlobalOptions.InvariantCultureInfo));
            // <hideitemsoveravaillimit />
            objWriter.WriteElementString("hideitemsoveravaillimit", _blnHideItemsOverAvailLimit.ToString());
            // <UnarmedImprovementsApplyToWeapons />
            objWriter.WriteElementString("unarmedimprovementsapplytoweapons", _blnUnarmedImprovementsApplyToWeapons.ToString());
            // <allowinitiationincreatemode />
            objWriter.WriteElementString("allowinitiationincreatemode", _blnAllowInitiationInCreateMode.ToString());
            // <usepointsonbrokengroups />
            objWriter.WriteElementString("usepointsonbrokengroups", _blnUsePointsOnBrokenGroups.ToString());
            // <dontdoublequalities />
            objWriter.WriteElementString("dontdoublequalities", _blnDontDoubleQualityPurchaseCost.ToString());
            // <dontdoublequalities />
            objWriter.WriteElementString("dontdoublequalityrefunds", _blnDontDoubleQualityRefundCost.ToString());
            // <ignoreart />
            objWriter.WriteElementString("ignoreart", _blnIgnoreArt.ToString());
            // <cyberlegmovement />
            objWriter.WriteElementString("cyberlegmovement", _blnCyberlegMovement.ToString());
            // <allow2ndmaxattribute />
            objWriter.WriteElementString("allow2ndmaxattribute", _blnAllow2ndMaxAttribute.ToString());
            // <freekarmacontactsmultiplier />
            objWriter.WriteElementString("freekarmacontactsmultiplier", _intFreeContactsMultiplier.ToString());
            // <freekarmaknowledgemultiplier />
            objWriter.WriteElementString("freekarmaknowledgemultiplier", _intFreeKnowledgeMultiplier.ToString());
            // <freekarmaknowledgemultiplierenabled />
            objWriter.WriteElementString("freekarmaknowledgemultiplierenabled", _blnFreeKnowledgeMultiplierEnabled.ToString());
            // <freecontactsmultiplierenabled />
            objWriter.WriteElementString("freecontactsmultiplierenabled", _blnFreeContactsMultiplierEnabled.ToString());
            // <freecontactsflatnumber />
            objWriter.WriteElementString("freecontactsflatnumber", _intFreeContactsFlatNumber.ToString());
            // <dronearmormultiplierenabled />
            objWriter.WriteElementString("dronearmormultiplierenabled", _blnDroneArmorMultiplierEnabled.ToString());
            // <dronearmorflatnumber />
            objWriter.WriteElementString("dronearmorflatnumber", _intDroneArmorMultiplier.ToString());
            // <usetotalvalueforknowledge />
            objWriter.WriteElementString("usetotalvalueforknowledge", _blnUseTotalValueForFreeKnowledge.ToString());
            // <usetotalvalueforcontacts />
            objWriter.WriteElementString("usetotalvalueforcontacts", _blnUseTotalValueForFreeContacts.ToString());
            // <nosinglearmorencumbrance />
            objWriter.WriteElementString("nosinglearmorencumbrance", _blnNoSingleArmorEncumbrance.ToString());
            // <ignorecomplexformlimit />
            objWriter.WriteElementString("ignorecomplexformlimit", _blnIgnoreComplexFormLimit.ToString());
            // <NoArmorEncumbrance />
            objWriter.WriteElementString("noarmorencumbrance", _blnNoArmorEncumbrance.ToString());
            // <esslossreducesmaximumonly />
            objWriter.WriteElementString("esslossreducesmaximumonly", _blnESSLossReducesMaximumOnly.ToString());
            // <allowskillregrouping />
            objWriter.WriteElementString("allowskillregrouping", _blnAllowSkillRegrouping.ToString());
            // <metatypecostskarma />
            objWriter.WriteElementString("metatypecostskarma", _blnMetatypeCostsKarma.ToString());
            // <metatypecostskarmamultiplier />
            objWriter.WriteElementString("metatypecostskarmamultiplier", _intMetatypeCostMultiplier.ToString());
            // <limbcount />
            objWriter.WriteElementString("limbcount", _intLimbCount.ToString());
            // <excludelimbslot />
            objWriter.WriteElementString("excludelimbslot", _strExcludeLimbSlot);
            // <allowcyberwareessdiscounts />
            objWriter.WriteElementString("allowcyberwareessdiscounts", _blnAllowCyberwareESSDiscounts.ToString());
            // <maximumarmormodifications />
            objWriter.WriteElementString("maximumarmormodifications", _blnMaximumArmorModifications.ToString());
            // <armordegredation />
            objWriter.WriteElementString("armordegredation", _blnArmorDegradation.ToString());
            // <specialkarmacostbasedonshownvalue />
            objWriter.WriteElementString("specialkarmacostbasedonshownvalue", _blnSpecialKarmaCostBasedOnShownValue.ToString());
            // <exceedpositivequalities />
            objWriter.WriteElementString("exceedpositivequalities", _blnExceedPositiveQualities.ToString());
            // <exceedpositivequalitiescostdoubled />
            objWriter.WriteElementString("exceedpositivequalitiescostdoubled", _blnExceedPositiveQualitiesCostDoubled.ToString());

            objWriter.WriteElementString("mysaddppcareer", MysAdeptAllowPPCareer.ToString());

            // <mysadeptsecondmagattribute />
            objWriter.WriteElementString("mysadeptsecondmagattribute", MysAdeptSecondMAGAttribute.ToString());

            // <exceednegativequalities />
            objWriter.WriteElementString("exceednegativequalities", _blnExceedNegativeQualities.ToString());
            // <exceednegativequalitieslimit />
            objWriter.WriteElementString("exceednegativequalitieslimit", _blnExceedNegativeQualitiesLimit.ToString());
            // <multiplyrestrictedcost />
            objWriter.WriteElementString("multiplyrestrictedcost", _blnMultiplyRestrictedCost.ToString());
            // <multiplyforbiddencost />
            objWriter.WriteElementString("multiplyforbiddencost", _blnMultiplyForbiddenCost.ToString());
            // <restrictedcostmultiplier />
            objWriter.WriteElementString("restrictedcostmultiplier", _intRestrictedCostMultiplier.ToString());
            // <forbiddencostmultiplier />
            objWriter.WriteElementString("forbiddencostmultiplier", _intForbiddenCostMultiplier.ToString());
            // <donotroundessenceinternally />
            objWriter.WriteElementString("donotroundessenceinternally", _blnDoNotRoundEssenceInternally.ToString());
            // <donotroundessenceinternally />
            objWriter.WriteElementString("enemykarmaqualitylimit", _blnEnemyKarmaQualityLimit.ToString());
            // <nuyenformat />
            objWriter.WriteElementString("nuyenformat", _strNuyenFormat);
            // <essencedecimals />
            objWriter.WriteElementString("essenceformat", _strEssenceFormat);
            // <enforcecapacity />
            objWriter.WriteElementString("enforcecapacity", _blnEnforceCapacity.ToString());
            // <restrictrecoil />
            objWriter.WriteElementString("restrictrecoil", _blnRestrictRecoil.ToString());
            // <unrestrictednuyen />
            objWriter.WriteElementString("unrestrictednuyen", _blnUnrestrictedNuyen.ToString());
            // <calculatecommlinkresponse />
            objWriter.WriteElementString("calculatecommlinkresponse", _blnCalculateCommlinkResponse.ToString());
            // <allowhigherstackedfoci />
            objWriter.WriteElementString("allowhigherstackedfoci", _blnAllowHigherStackedFoci.ToString());
            // <alloweditpartofbaseweapon />
            objWriter.WriteElementString("alloweditpartofbaseweapon", _blnAllowEditPartOfBaseWeapon.ToString());
            // <breakskillgroupsincreatemode />
            objWriter.WriteElementString("breakskillgroupsincreatemode", _blnStrictSkillGroupsInCreateMode.ToString());
            // <allowpointbuyspecializationsonkarmaskills />
            objWriter.WriteElementString("allowpointbuyspecializationsonkarmaskills", _blnAllowPointBuySpecializationsOnKarmaSkills.ToString());
            // <extendanydetectionspell />
            objWriter.WriteElementString("extendanydetectionspell", _blnExtendAnyDetectionSpell.ToString());
            // <allowskilldicerolling />
            objWriter.WriteElementString("allowskilldicerolling", _blnAllowSkillDiceRolling.ToString());
            //<dontusecyberlimbcalculation />
            objWriter.WriteElementString("dontusecyberlimbcalculation", _blnDontUseCyberlimbCalculation.ToString());
            // <alternatemetatypeattributekarma />
            objWriter.WriteElementString("alternatemetatypeattributekarma", _blnAlternateMetatypeAttributeKarma.ToString());
            // <reversekarmapriorityorder />
            objWriter.WriteElementString("reverseattributepriorityorder", ReverseAttributePriorityOrder.ToString());
            // <printnotes />
            objWriter.WriteElementString("printnotes", _blnPrintNotes.ToString());
            // <allowobsolescentupgrade />
            objWriter.WriteElementString("allowobsolescentupgrade", _blnAllowObsolescentUpgrade.ToString());
            // <allowbiowaresuites />
            objWriter.WriteElementString("allowbiowaresuites", _blnAllowBiowareSuites.ToString());
            // <freespiritpowerpointsmag />
            objWriter.WriteElementString("freespiritpowerpointsmag", _blnFreeSpiritPowerPointsMAG.ToString());
            // <allowhoverincrement />
            objWriter.WriteElementString("allowhoverincrement", AllowHoverIncrement.ToString());
            // <searchincategoryonly />
            objWriter.WriteElementString("searchincategoryonly", SearchInCategoryOnly.ToString());
            // <compensateskillgroupkarmadifference />
            objWriter.WriteElementString("compensateskillgroupkarmadifference", _blnCompensateSkillGroupKarmaDifference.ToString());
            // <autobackstory />
            objWriter.WriteElementString("autobackstory", _blnAutomaticBackstory.ToString());
            // <freemartialartspecialization />
            objWriter.WriteElementString("freemartialartspecialization", _blnFreeMartialArtSpecialization.ToString());
            // <priorityspellsasadeptpowers />
            objWriter.WriteElementString("priorityspellsasadeptpowers", _blnPrioritySpellsAsAdeptPowers.ToString());
            // <usecalculatedpublicawareness />
            objWriter.WriteElementString("usecalculatedpublicawareness", _blnUseCalculatedPublicAwareness.ToString());
            // <increasedimprovedabilitymodifier />
            objWriter.WriteElementString("increasedimprovedabilitymodifier", _increasedImprovedAbilityMultiplier.ToString());
            // <allowfreegrids />
            objWriter.WriteElementString("allowfreegrids", _allowFreeGrids.ToString());
            // <allowtechnomancerschooling />
            objWriter.WriteElementString("allowtechnomancerschooling", _blnAllowTechnomancerSchooling.ToString());
            // <cyberlimbattributebonuscap />
            objWriter.WriteElementString("cyberlimbattributebonuscap", _intCyberlimbAttributeBonusCap.ToString());

            // <karmacost>
            objWriter.WriteStartElement("karmacost");
            // <karmaattribute />
            objWriter.WriteElementString("karmaattribute", _intKarmaAttribute.ToString());
            // <karmaquality />
            objWriter.WriteElementString("karmaquality", _intKarmaQuality.ToString());
            // <karmaspecialization />
            objWriter.WriteElementString("karmaspecialization", _intKarmaSpecialization.ToString());
            // <karmaknospecialization />
            objWriter.WriteElementString("karmaknospecialization", _intKarmaKnoSpecialization.ToString());
            // <karmanewknowledgeskill />
            objWriter.WriteElementString("karmanewknowledgeskill", _intKarmaNewKnowledgeSkill.ToString());
            // <karmanewactiveskill />
            objWriter.WriteElementString("karmanewactiveskill", _intKarmaNewActiveSkill.ToString());
            // <karmanewskillgroup />
            objWriter.WriteElementString("karmanewskillgroup", _intKarmaNewSkillGroup.ToString());
            // <karmaimproveknowledgeskill />
            objWriter.WriteElementString("karmaimproveknowledgeskill", _intKarmaImproveKnowledgeSkill.ToString());
            // <karmaimproveactiveskill />
            objWriter.WriteElementString("karmaimproveactiveskill", _intKarmaImproveActiveSkill.ToString());
            // <karmaimproveskillgroup />
            objWriter.WriteElementString("karmaimproveskillgroup", _intKarmaImproveSkillGroup.ToString());
            // <karmaspell />
            objWriter.WriteElementString("karmaspell", _intKarmaSpell.ToString());
            // <karmaenhancement />
            objWriter.WriteElementString("karmaenhancement", _intKarmaEnhancement.ToString());
            // <karmanewcomplexform />
            objWriter.WriteElementString("karmanewcomplexform", _intKarmaNewComplexForm.ToString());
            // <karmaimprovecomplexform />
            objWriter.WriteElementString("karmaimprovecomplexform", _intKarmaImproveComplexForm.ToString());
            // <karmanewaiprogram />
            objWriter.WriteElementString("karmanewaiprogram", _intKarmaNewAIProgram.ToString());
            // <karmanewaiadvancedprogram />
            objWriter.WriteElementString("karmanewaiadvancedprogram", _intKarmaNewAIAdvancedProgram.ToString());
            // <karmanuyenper />
            objWriter.WriteElementString("karmanuyenper", _intKarmaNuyenPer.ToString());
            // <karmacontact />
            objWriter.WriteElementString("karmacontact", _intKarmaContact.ToString());
            // <karmaenemy />
            objWriter.WriteElementString("karmaenemy", _intKarmaEnemy.ToString());
            // <karmacarryover />
            objWriter.WriteElementString("karmacarryover", _intKarmaCarryover.ToString());
            // <karmaspirit />
            objWriter.WriteElementString("karmaspirit", _intKarmaSpirit.ToString());
            // <karmamaneuver />
            objWriter.WriteElementString("karmamaneuver", _intKarmaManeuver.ToString());
            // <karmainitiation />
            objWriter.WriteElementString("karmainitiation", _intKarmaInitiation.ToString());
            // <karmainitiationflat />
            objWriter.WriteElementString("karmainitiationflat", _intKarmaInitiationFlat.ToString());
            // <karmametamagic />
            objWriter.WriteElementString("karmametamagic", _intKarmaMetamagic.ToString());
            // <karmacomplexformoption />
            objWriter.WriteElementString("karmacomplexformoption", _intKarmaComplexFormOption.ToString());
            // <karmacomplexformskillsoft />
            objWriter.WriteElementString("karmacomplexformskillsoft", _intKarmaComplexFormSkillfot.ToString());
            // <karmajoingroup />
            objWriter.WriteElementString("karmajoingroup", _intKarmaJoinGroup.ToString());
            // <karmaleavegroup />
            objWriter.WriteElementString("karmaleavegroup", _intKarmaLeaveGroup.ToString());
            // <karmaalchemicalfocus />
            objWriter.WriteElementString("karmaalchemicalfocus", _intKarmaAlchemicalFocus.ToString());
            // <karmabanishingfocus />
            objWriter.WriteElementString("karmabanishingfocus", _intKarmaBanishingFocus.ToString());
            // <karmabindingfocus />
            objWriter.WriteElementString("karmabindingfocus", _intKarmaBindingFocus.ToString());
            // <karmacenteringfocus />
            objWriter.WriteElementString("karmacenteringfocus", _intKarmaCenteringFocus.ToString());
            // <karmacounterspellingfocus />
            objWriter.WriteElementString("karmacounterspellingfocus", _intKarmaCounterspellingFocus.ToString());
            // <karmadisenchantingfocus />
            objWriter.WriteElementString("karmadisenchantingfocus", _intKarmaDisenchantingFocus.ToString());
            // <karmaflexiblesignaturefocus />
            objWriter.WriteElementString("karmaflexiblesignaturefocus", _intKarmaFlexibleSignatureFocus.ToString());
            // <karmamaskingfocus />
            objWriter.WriteElementString("karmamaskingfocus", _intKarmaMaskingFocus.ToString());
            // <karmapowerfocus />
            objWriter.WriteElementString("karmapowerfocus", _intKarmaPowerFocus.ToString());
            // <karmaqifocus />
            objWriter.WriteElementString("karmaqifocus", _intKarmaQiFocus.ToString());
            // <karmaritualspellcastingfocus />
            objWriter.WriteElementString("karmaritualspellcastingfocus", _intKarmaRitualSpellcastingFocus.ToString());
            // <karmaspellcastingfocus />
            objWriter.WriteElementString("karmaspellcastingfocus", _intKarmaSpellcastingFocus.ToString());
            // <karmaspellshapingfocus />
            objWriter.WriteElementString("karmaspellshapingfocus", _intKarmaSpellShapingFocus.ToString());
            // <karmasummoningfocus />
            objWriter.WriteElementString("karmasummoningfocus", _intKarmaSummoningFocus.ToString());
            // <karmasustainingfocus />
            objWriter.WriteElementString("karmasustainingfocus", _intKarmaSustainingFocus.ToString());
            // <karmaweaponfocus />
            objWriter.WriteElementString("karmaweaponfocus", _intKarmaWeaponFocus.ToString());
            // <karmaweaponfocus />
            objWriter.WriteElementString("karmamysadpp", _intKarmaMysticAdeptPowerPoint.ToString());
            // </karmacost>
            objWriter.WriteEndElement();

            // <books>
            objWriter.WriteStartElement("books");
            foreach (string strBook in _lstBooks)
                objWriter.WriteElementString("book", strBook);
            // </books>
            objWriter.WriteEndElement();


            // <customdatadirectorynames>
            objWriter.WriteStartElement("customdatadirectorynames");
            foreach (KeyValuePair<string, bool> dicDirectoryName in _dicCustomDataDirectoryNames)
            {
                objWriter.WriteStartElement("customdatadirectoryname");
                objWriter.WriteElementString("directoryname", dicDirectoryName.Key);
                objWriter.WriteElementString("enabled", dicDirectoryName.Value.ToString(GlobalOptions.InvariantCultureInfo));
                objWriter.WriteEndElement();
            }
            // </customdatadirectorynames>
            objWriter.WriteEndElement();
            // <hascustomdirectories>

            // <defaultbuild>
            objWriter.WriteStartElement("defaultbuild");
            // <buildmethod />
            objWriter.WriteElementString("buildmethod", _strBuildMethod);
            // <buildpoints />
            objWriter.WriteElementString("buildpoints", _intBuildPoints.ToString());
            // <availability />
            objWriter.WriteElementString("availability", _intAvailability.ToString());
            // </defaultbuild>
            objWriter.WriteEndElement();

            // </settings>
            objWriter.WriteEndElement();

            objWriter.WriteEndDocument();
            objWriter.Close();
        }

        /// <summary>
        /// Load the settings from the settings file.
        /// </summary>
        /// <param name="strFileName">Settings file to load from.</param>
        public bool Load(string strFileName)
        {
            _strFileName = strFileName;
            string strFilePath = Path.Combine(Utils.GetStartupPath, "settings", _strFileName);
            XmlDocument objXmlDocument = new XmlDocument();
            // Make sure the settings file exists. If not, ask the user if they would like to use the default settings file instead. A character cannot be loaded without a settings file.
            if (File.Exists(strFilePath))
            {
                try
                {
                    using (StreamReader objStreamReader = new StreamReader(strFilePath, Encoding.UTF8, true))
                    {
                        objXmlDocument.Load(objStreamReader);
                    }
                }
                catch (IOException)
                {
                    Program.MainForm.ShowMessageBox(LanguageManager.GetString("Message_CharacterOptions_CannotLoadCharacter", GlobalOptions.Language), LanguageManager.GetString("MessageText_CharacterOptions_CannotLoadCharacter", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
                catch (XmlException)
                {
                    Program.MainForm.ShowMessageBox(LanguageManager.GetString("Message_CharacterOptions_CannotLoadCharacter", GlobalOptions.Language), LanguageManager.GetString("MessageText_CharacterOptions_CannotLoadCharacter", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }
            else
            {
                if (MessageBox.Show(string.Format(LanguageManager.GetString("Message_CharacterOptions_CannotLoadSetting", GlobalOptions.Language), _strFileName), LanguageManager.GetString("MessageTitle_CharacterOptions_CannotLoadSetting", GlobalOptions.Language), MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                {
                    Program.MainForm.ShowMessageBox(LanguageManager.GetString("Message_CharacterOptions_CannotLoadCharacter", GlobalOptions.Language), LanguageManager.GetString("MessageText_CharacterOptions_CannotLoadCharacter", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
                else
                {
                    _strFileName = "default.xml";
                    strFilePath = Path.Combine(Utils.GetStartupPath, "settings", _strFileName);
                    try
                    {
                        using (StreamReader objStreamReader = new StreamReader(strFilePath, Encoding.UTF8, true))
                        {
                            objXmlDocument.Load(objStreamReader);
                        }
                    }
                    catch (IOException)
                    {
                        Program.MainForm.ShowMessageBox(LanguageManager.GetString("Message_CharacterOptions_CannotLoadCharacter", GlobalOptions.Language), LanguageManager.GetString("MessageText_CharacterOptions_CannotLoadCharacter", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }
                    catch (XmlException)
                    {
                        Program.MainForm.ShowMessageBox(LanguageManager.GetString("Message_CharacterOptions_CannotLoadCharacter", GlobalOptions.Language), LanguageManager.GetString("MessageText_CharacterOptions_CannotLoadCharacter", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }
                }
            }

            XmlNode objXmlNode = objXmlDocument.SelectSingleNode("//settings");
            // Setting name.
            _strName = objXmlDocument.SelectSingleNode("/settings/name")?.InnerText;
            // Most recent image folder location used.
            objXmlNode.TryGetStringFieldQuickly("recentimagefolder", ref _strImageFolder);
            // Confirm delete.
            objXmlNode.TryGetBoolFieldQuickly("confirmdelete", ref _blnConfirmDelete);
            // License Restricted items.
            objXmlNode.TryGetBoolFieldQuickly("licenserestricted", ref _blnLicenseRestrictedItems);
            // Confirm Karama ExpenseTryGetField
            objXmlNode.TryGetBoolFieldQuickly("confirmkarmaexpense", ref _blnConfirmKarmaExpense);
            // Print all Active Skills with a total value greater than 0 (as opposed to only printing those with a Rating higher than 0).
            objXmlNode.TryGetBoolFieldQuickly("printzeroratingskills", ref _blnPrintSkillsWithZeroRating);
            // More Lethal Gameplay.
            objXmlNode.TryGetBoolFieldQuickly("morelethalgameplay", ref _blnMoreLethalGameplay);
            // Spirit Force Based on Total MAG.
            objXmlNode.TryGetBoolFieldQuickly("spiritforcebasedontotalmag", ref _blnSpiritForceBasedOnTotalMAG);
            // Print Expenses.
            objXmlNode.TryGetBoolFieldQuickly("printexpenses", ref _blnPrintExpenses);
            // Print Free Expenses.
            objXmlNode.TryGetBoolFieldQuickly("printfreeexpenses", ref _blnPrintFreeExpenses);
            // Nuyen per Build Point
            objXmlNode.TryGetDecFieldQuickly("nuyenperbp", ref _decNuyenPerBP);
            // Hide Items Over Avail Limit in Create Mode
            objXmlNode.TryGetBoolFieldQuickly("hideitemsoveravaillimit", ref _blnHideItemsOverAvailLimit);
            // Knucks use Unarmed
            objXmlNode.TryGetBoolFieldQuickly("unarmedimprovementsapplytoweapons", ref _blnUnarmedImprovementsApplyToWeapons);
            // Allow Initiation in Create Mode
            objXmlNode.TryGetBoolFieldQuickly("allowinitiationincreatemode", ref _blnAllowInitiationInCreateMode);
            // Use Points on Broken Groups
            objXmlNode.TryGetBoolFieldQuickly("usepointsonbrokengroups", ref _blnUsePointsOnBrokenGroups);
            // Don't Double the Cost of purchasing Qualities in Career Mode
            objXmlNode.TryGetBoolFieldQuickly("dontdoublequalities", ref _blnDontDoubleQualityPurchaseCost);
            // Don't Double the Cost of removing Qualities in Career Mode
            objXmlNode.TryGetBoolFieldQuickly("dontdoublequalityrefunds", ref _blnDontDoubleQualityRefundCost);
            // Ignore Art Requirements from Street Grimoire
            objXmlNode.TryGetBoolFieldQuickly("ignoreart", ref _blnIgnoreArt);
            // Use Cyberleg Stats for Movement
            objXmlNode.TryGetBoolFieldQuickly("cyberlegmovement", ref _blnCyberlegMovement);
            // Allow a 2nd Max Attribute
            objXmlNode.TryGetBoolFieldQuickly("allow2ndmaxattribute", ref _blnAllow2ndMaxAttribute);
            // Free Contacts Multiplier
            objXmlNode.TryGetInt32FieldQuickly("freekarmacontactsmultiplier", ref _intFreeContactsMultiplier);
            // Free Contacts use Total Value instead of Value
            objXmlNode.TryGetBoolFieldQuickly("usetotalvalueforcontacts", ref _blnUseTotalValueForFreeContacts);
            // Free Contacts Multiplier Enabled
            objXmlNode.TryGetBoolFieldQuickly("freecontactsmultiplierenabled", ref _blnFreeContactsMultiplierEnabled);
            // Drone Armor Multiplier Enabled
            objXmlNode.TryGetBoolFieldQuickly("dronearmormultiplierenabled", ref _blnDroneArmorMultiplierEnabled);
            // Drone Armor Multiplier Value
            objXmlNode.TryGetInt32FieldQuickly("dronearmorflatnumber", ref _intDroneArmorMultiplier);
            // Free Knowledge Multiplier Enabled
            objXmlNode.TryGetBoolFieldQuickly("freekarmaknowledgemultiplierenabled", ref _blnFreeKnowledgeMultiplierEnabled);
            objXmlNode.TryGetInt32FieldQuickly("freekarmacontactsmultiplier", ref _intFreeContactsMultiplier);
            // Free Knowledge uses Total Value instead of Value
            objXmlNode.TryGetBoolFieldQuickly("usetotalvalueforknowledge", ref _blnUseTotalValueForFreeKnowledge);
            // Free Contacts Multiplier
            objXmlNode.TryGetInt32FieldQuickly("freekarmaknowledgemultiplier", ref _intFreeKnowledgeMultiplier);
            // No Single Armor Encumbrance
            objXmlNode.TryGetBoolFieldQuickly("nosinglearmorencumbrance", ref _blnNoSingleArmorEncumbrance);
            // Ignore Armor Encumbrance
            objXmlNode.TryGetBoolFieldQuickly("noarmorencumbrance", ref _blnNoArmorEncumbrance);
            // Ignore Complex Form Limit
            objXmlNode.TryGetBoolFieldQuickly("ignorecomplexformlimit", ref _blnIgnoreComplexFormLimit);
            // Essence Loss Reduces Maximum Only.
            objXmlNode.TryGetBoolFieldQuickly("esslossreducesmaximumonly", ref _blnESSLossReducesMaximumOnly);
            // Allow Skill Regrouping.
            objXmlNode.TryGetBoolFieldQuickly("allowskillregrouping", ref _blnAllowSkillRegrouping);
            // Metatype Costs Karma.
            objXmlNode.TryGetBoolFieldQuickly("metatypecostskarma", ref _blnMetatypeCostsKarma);
            // Metatype Costs Karma.
            objXmlNode.TryGetBoolFieldQuickly("reverseattributepriorityorder", ref _blnReverseAttributePriorityOrder);
            // Metatype Costs Karma Multiplier.
            objXmlNode.TryGetInt32FieldQuickly("metatypecostskarmamultiplier", ref _intMetatypeCostMultiplier);
            // Limb Count.
            objXmlNode.TryGetInt32FieldQuickly("limbcount", ref _intLimbCount);
            // Exclude Limb Slot.
            objXmlNode.TryGetStringFieldQuickly("excludelimbslot", ref _strExcludeLimbSlot);
            // Allow Cyberware Essence Cost Discounts.
            objXmlNode.TryGetBoolFieldQuickly("allowcyberwareessdiscounts", ref _blnAllowCyberwareESSDiscounts);
            // Use Maximum Armor Modifications.
            objXmlNode.TryGetBoolFieldQuickly("maximumarmormodifications", ref _blnMaximumArmorModifications);
            // Allow Armor Degredation.
            objXmlNode.TryGetBoolFieldQuickly("armordegredation", ref _blnArmorDegradation);
            // Whether or not Karma costs for increasing Special Attributes is based on the shown value instead of actual value.
            objXmlNode.TryGetBoolFieldQuickly("specialkarmacostbasedonshownvalue", ref _blnSpecialKarmaCostBasedOnShownValue);
            // Allow more than 35 BP in Positive Qualities.
            objXmlNode.TryGetBoolFieldQuickly("exceedpositivequalities", ref _blnExceedPositiveQualities);
            // Double all positive qualities in excess of the limit
            objXmlNode.TryGetBoolFieldQuickly("exceedpositivequalitiescostdoubled", ref _blnExceedPositiveQualitiesCostDoubled);

            objXmlNode.TryGetBoolFieldQuickly("mysaddppcareer", ref _blnMysAdeptAllowPPCareer);

            // Split MAG for Mystic Adepts so that they have a separate MAG rating for Adept Powers instead of using the special PP rules for mystic adepts
            objXmlNode.TryGetBoolFieldQuickly("mysadeptsecondmagattribute", ref _blnMysAdeptSecondMAGAttribute);

            // Grant a free specialization when taking a martial art.
            objXmlNode.TryGetBoolFieldQuickly("freemartialartspecialization", ref _blnFreeMartialArtSpecialization);
            // Can spend spells from Magic priority as power points
            objXmlNode.TryGetBoolFieldQuickly("priorityspellsasadeptpowers", ref _blnPrioritySpellsAsAdeptPowers);
            // Allow more than 35 BP in Negative Qualities.
            objXmlNode.TryGetBoolFieldQuickly("exceednegativequalities", ref _blnExceedNegativeQualities);
            // Character can still only receive 35 BP from Negative Qualities (though they can still add as many as they'd like).
            objXmlNode.TryGetBoolFieldQuickly("exceednegativequalitieslimit", ref _blnExceedNegativeQualitiesLimit);
            // Whether or not Restricted items have their cost multiplied.
            objXmlNode.TryGetBoolFieldQuickly("multiplyrestrictedcost", ref _blnMultiplyRestrictedCost);
            // Whether or not Forbidden items have their cost multiplied.
            objXmlNode.TryGetBoolFieldQuickly("multiplyforbiddencost", ref _blnMultiplyForbiddenCost);
            // Restricted cost multiplier.
            objXmlNode.TryGetInt32FieldQuickly("restrictedcostmultiplier", ref _intRestrictedCostMultiplier);
            // Forbidden cost multiplier.
            objXmlNode.TryGetInt32FieldQuickly("forbiddencostmultiplier", ref _intForbiddenCostMultiplier);
            // Only round essence when its value is displayed
            objXmlNode.TryGetBoolFieldQuickly("donotroundessenceinternally", ref _blnDoNotRoundEssenceInternally);
            // Only round essence when its value is displayed
            objXmlNode.TryGetBoolFieldQuickly("enemykarmaqualitylimit", ref _blnEnemyKarmaQualityLimit);
            // Format in which nuyen values are displayed
            objXmlNode.TryGetStringFieldQuickly("nuyenformat", ref _strNuyenFormat);
            // Format in which essence values should be displayed (and to which they should be rounded)
            if (!objXmlNode.TryGetStringFieldQuickly("essenceformat", ref _strEssenceFormat))
            {
                int intTemp = 2;
                // Number of decimal places to round to when calculating Essence.
                objXmlNode.TryGetInt32FieldQuickly("essencedecimals", ref intTemp);
                EssenceDecimals = intTemp;
            }
            // Whether or not Capacity limits should be enforced.
            objXmlNode.TryGetBoolFieldQuickly("enforcecapacity", ref _blnEnforceCapacity);
            // Whether or not Recoil modifiers are restricted (AR 148).
            objXmlNode.TryGetBoolFieldQuickly("restrictrecoil", ref _blnRestrictRecoil);
            // Whether or not character are not restricted to the number of points they can invest in Nuyen.
            objXmlNode.TryGetBoolFieldQuickly("unrestrictednuyen", ref _blnUnrestrictedNuyen);
            // Whether or not a Commlink's Response should be calculated based on the number of programms it has running.
            objXmlNode.TryGetBoolFieldQuickly("calculatecommlinkresponse", ref _blnCalculateCommlinkResponse);
            // Whether or not Stacked Foci can go a combined Force higher than 6.
            objXmlNode.TryGetBoolFieldQuickly("allowhigherstackedfoci", ref _blnAllowHigherStackedFoci);
            // Whether or not the user can change the status of a Weapon Mod or Accessory being part of the base Weapon.
            objXmlNode.TryGetBoolFieldQuickly("alloweditpartofbaseweapon", ref _blnAllowEditPartOfBaseWeapon);
            // Whether or not the user can break Skill Groups while in Create Mode.
            objXmlNode.TryGetBoolFieldQuickly("breakskillgroupsincreatemode", ref _blnStrictSkillGroupsInCreateMode);
            // Whether or not the user is allowed to buy specializations with skill points for skills only bought with karma.
            objXmlNode.TryGetBoolFieldQuickly("allowpointbuyspecializationsonkarmaskills", ref _blnAllowPointBuySpecializationsOnKarmaSkills);
            // Whether or not any Detection Spell can be taken as Extended range version.
            objXmlNode.TryGetBoolFieldQuickly("extendanydetectionspell", ref _blnExtendAnyDetectionSpell);
            // Whether or not dice rolling id allowed for Skills.
            objXmlNode.TryGetBoolFieldQuickly("allowskilldicerolling", ref _blnAllowSkillDiceRolling);
            // Whether or not cyberlimbs are used for augmeneted attribute calculation.
            objXmlNode.TryGetBoolFieldQuickly("dontusecyberlimbcalculation", ref _blnDontUseCyberlimbCalculation);
            // House rule: Treat the Metatype Attribute Minimum as 1 for the purpose of calculating Karma costs.
            objXmlNode.TryGetBoolFieldQuickly("alternatemetatypeattributekarma", ref _blnAlternateMetatypeAttributeKarma);
            // Whether or not Notes should be printed.
            objXmlNode.TryGetBoolFieldQuickly("printnotes", ref _blnPrintNotes);
            // Whether or not Obsolescent can be removed/upgrade in the same manner as Obsolete.
            objXmlNode.TryGetBoolFieldQuickly("allowobsolescentupgrade", ref _blnAllowObsolescentUpgrade);
            // Whether or not Bioware Suites can be created and added.
            objXmlNode.TryGetBoolFieldQuickly("allowbiowaresuites", ref _blnAllowBiowareSuites);
            // House rule: Free Spirits calculate their Power Points based on their MAG instead of EDG.
            objXmlNode.TryGetBoolFieldQuickly("freespiritpowerpointsmag", ref _blnFreeSpiritPowerPointsMAG);
            // House rule: Whether or not Technomancers can select Autosofts as Complex Forms.
            objXmlNode.TryGetBoolFieldQuickly("allowhoverincrement", ref _blnAllowHoverIncrement);
            // Optional Rule: Whether searching in a selection form will limit itself to the current Category that's selected.
            objXmlNode.TryGetBoolFieldQuickly("searchincategoryonly", ref _blnSearchInCategoryOnly);
            // House rule: Whether to compensate for the karma cost difference between raising skill ratings and skill groups when increasing the rating of the last skill in the group
            objXmlNode.TryGetBoolFieldQuickly("compensateskillgroupkarmadifference", ref _blnCompensateSkillGroupKarmaDifference);
            // Optional Rule: Whether Life Modules should automatically create a character back story.
            objXmlNode.TryGetBoolFieldQuickly("autobackstory", ref _blnAutomaticBackstory);
            // House Rule: Whether Public Awareness should be a calculated attribute based on Street Cred and Notoriety.
            objXmlNode.TryGetBoolFieldQuickly("usecalculatedpublicawareness", ref _blnUseCalculatedPublicAwareness);
            // House Rule: Whether Improved Ability should be capped at 0.5 (false) or 1.5 (true) of the target skill's Learned Rating.
            objXmlNode.TryGetBoolFieldQuickly("increasedimprovedabilitymodifier", ref _increasedImprovedAbilityMultiplier);
            // House Rule: Whether lifestyles will give free grid subscriptions found in HT to players. 
            objXmlNode.TryGetBoolFieldQuickly("allowfreegrids", ref _allowFreeGrids);
            // House Rule: Whether Technomancers should be allowed to receive Schooling discounts in the same manner as Awakened. 
            objXmlNode.TryGetBoolFieldQuickly("allowtechnomancerschooling", ref _blnAllowTechnomancerSchooling);
            // House Rule: Maximum value that cyberlimbs can have as a bonus on top of their Customisation. 
            objXmlNode.TryGetInt32FieldQuickly("cyberlimbattributebonuscap", ref _intCyberlimbAttributeBonusCap);

            objXmlNode = objXmlDocument.SelectSingleNode("//settings/karmacost");
            // Attempt to populate the Karma values.
            if (objXmlNode != null)
            {
                objXmlNode.TryGetInt32FieldQuickly("karmaattribute", ref _intKarmaAttribute);
                objXmlNode.TryGetInt32FieldQuickly("karmaquality", ref _intKarmaQuality);
                objXmlNode.TryGetInt32FieldQuickly("karmaspecialization", ref _intKarmaSpecialization);
                objXmlNode.TryGetInt32FieldQuickly("karmaknospecialization", ref _intKarmaKnoSpecialization);
                objXmlNode.TryGetInt32FieldQuickly("karmanewknowledgeskill", ref _intKarmaNewKnowledgeSkill);
                objXmlNode.TryGetInt32FieldQuickly("karmanewactiveskill", ref _intKarmaNewActiveSkill);
                objXmlNode.TryGetInt32FieldQuickly("karmanewskillgroup", ref _intKarmaNewSkillGroup);
                objXmlNode.TryGetInt32FieldQuickly("karmaimproveknowledgeskill", ref _intKarmaImproveKnowledgeSkill);
                objXmlNode.TryGetInt32FieldQuickly("karmaimproveactiveskill", ref _intKarmaImproveActiveSkill);
                objXmlNode.TryGetInt32FieldQuickly("karmaimproveskillgroup", ref _intKarmaImproveSkillGroup);
                objXmlNode.TryGetInt32FieldQuickly("karmaspell", ref _intKarmaSpell);
                objXmlNode.TryGetInt32FieldQuickly("karmanewcomplexform", ref _intKarmaNewComplexForm);
                objXmlNode.TryGetInt32FieldQuickly("karmaimprovecomplexform", ref _intKarmaImproveComplexForm);
                objXmlNode.TryGetInt32FieldQuickly("karmanewaiprogram", ref _intKarmaNewAIProgram);
                objXmlNode.TryGetInt32FieldQuickly("karmanewaiadvancedprogram", ref _intKarmaNewAIAdvancedProgram);
                objXmlNode.TryGetInt32FieldQuickly("karmanuyenper", ref _intKarmaNuyenPer);
                objXmlNode.TryGetInt32FieldQuickly("karmacontact", ref _intKarmaContact);
                objXmlNode.TryGetInt32FieldQuickly("karmaenemy", ref _intKarmaEnemy);
                objXmlNode.TryGetInt32FieldQuickly("karmacarryover", ref _intKarmaCarryover);
                objXmlNode.TryGetInt32FieldQuickly("karmaspirit", ref _intKarmaSpirit);
                objXmlNode.TryGetInt32FieldQuickly("karmamaneuver", ref _intKarmaManeuver);
                objXmlNode.TryGetInt32FieldQuickly("karmainitiation", ref _intKarmaInitiation);
                objXmlNode.TryGetInt32FieldQuickly("karmainitiationflat", ref _intKarmaInitiationFlat);
                objXmlNode.TryGetInt32FieldQuickly("karmametamagic", ref _intKarmaMetamagic);
                objXmlNode.TryGetInt32FieldQuickly("karmacomplexformoption", ref _intKarmaComplexFormOption);
                objXmlNode.TryGetInt32FieldQuickly("karmajoingroup", ref _intKarmaJoinGroup);
                objXmlNode.TryGetInt32FieldQuickly("karmaleavegroup", ref _intKarmaLeaveGroup);
                objXmlNode.TryGetInt32FieldQuickly("karmacomplexformskillsoft", ref _intKarmaComplexFormSkillfot);
                objXmlNode.TryGetInt32FieldQuickly("karmaenhancement", ref _intKarmaEnhancement);
                objXmlNode.TryGetInt32FieldQuickly("karmamysadpp", ref _intKarmaMysticAdeptPowerPoint);

                // Attempt to load the Karma costs for Foci.
                objXmlNode.TryGetInt32FieldQuickly("karmaalchemicalfocus", ref _intKarmaAlchemicalFocus);
                objXmlNode.TryGetInt32FieldQuickly("karmabanishingfocus", ref _intKarmaBanishingFocus);
                objXmlNode.TryGetInt32FieldQuickly("karmabindingfocus", ref _intKarmaBindingFocus);
                objXmlNode.TryGetInt32FieldQuickly("karmacenteringfocus", ref _intKarmaCenteringFocus);
                objXmlNode.TryGetInt32FieldQuickly("karmacounterspellingfocus", ref _intKarmaCounterspellingFocus);
                objXmlNode.TryGetInt32FieldQuickly("karmadisenchantingfocus", ref _intKarmaDisenchantingFocus);
                objXmlNode.TryGetInt32FieldQuickly("karmaflexiblesignaturefocus", ref _intKarmaFlexibleSignatureFocus);
                objXmlNode.TryGetInt32FieldQuickly("karmamaskingfocus", ref _intKarmaMaskingFocus);
                objXmlNode.TryGetInt32FieldQuickly("karmapowerfocus", ref _intKarmaPowerFocus);
                objXmlNode.TryGetInt32FieldQuickly("karmaqifocus", ref _intKarmaQiFocus);
                objXmlNode.TryGetInt32FieldQuickly("karmaritualspellcastingfocus", ref _intKarmaRitualSpellcastingFocus);
                objXmlNode.TryGetInt32FieldQuickly("karmaspellcastingfocus", ref _intKarmaSpellcastingFocus);
                objXmlNode.TryGetInt32FieldQuickly("karmaspellshapingfocus", ref _intKarmaSpellShapingFocus);
                objXmlNode.TryGetInt32FieldQuickly("karmasummoningfocus", ref _intKarmaSummoningFocus);
                objXmlNode.TryGetInt32FieldQuickly("karmasustainingfocus", ref _intKarmaSustainingFocus);
                objXmlNode.TryGetInt32FieldQuickly("karmaweaponfocus", ref _intKarmaWeaponFocus);
            }

            // Load Books.
            _lstBooks.Clear();
            using (XmlNodeList xmlBookList = objXmlDocument.SelectNodes("/settings/books/book"))
                if (xmlBookList != null)
                    foreach (XmlNode objXmlBook in xmlBookList)
                        _lstBooks.Add(objXmlBook.InnerText);
            RecalculateBookXPath();

            // Load Custom Data Directory names.
            _dicCustomDataDirectoryNames.Clear();
            using (XmlNodeList xmlDirectoryList =
                objXmlDocument.SelectNodes("/settings/customdatadirectorynames/customdatadirectoryname"))
            {
                if (xmlDirectoryList != null)
                    foreach (XmlNode objXmlDirectoryName in xmlDirectoryList)
                    {
                        _dicCustomDataDirectoryNames.Add(objXmlDirectoryName["directoryname"].InnerText,
                            Convert.ToBoolean(objXmlDirectoryName["enabled"].InnerText));
                    }
            }

            if (_dicCustomDataDirectoryNames.Count == 0)
            {
                using (XmlNodeList xmlDirectoryList =
                    objXmlDocument.SelectNodes("/settings/customdatadirectorynames/directoryname"))
                    if (xmlDirectoryList != null)
                        foreach (XmlNode objXmlDirectoryName in xmlDirectoryList)
                            _dicCustomDataDirectoryNames.Add(objXmlDirectoryName.InnerText, true);
            }

            // Load default build settings.
            objXmlNode = objXmlDocument.SelectSingleNode("//settings/defaultbuild");
            if (objXmlNode != null)
            {
                objXmlNode.TryGetStringFieldQuickly("buildmethod", ref _strBuildMethod);
                objXmlNode.TryGetInt32FieldQuickly("buildpoints", ref _intBuildPoints);
                objXmlNode.TryGetInt32FieldQuickly("availability", ref _intAvailability);
            }

            return true;
        }
        #endregion

        #region Properties and Methods
        /// <summary>
        /// Load the Options from the Registry (which will subsequently be converted to the XML Settings File format). Registry keys are deleted once they are read since they will no longer be used.
        /// </summary>
        public void LoadFromRegistry()
        {
            if (GlobalOptions.ChummerRegistryKey == null)
                return;
            // Confirm delete.
            GlobalOptions.LoadBoolFromRegistry(ref _blnConfirmDelete, "confirmdelete", string.Empty, true);

            // Confirm Karama Expense.
            GlobalOptions.LoadBoolFromRegistry(ref _blnConfirmKarmaExpense, "confirmkarmaexpense", string.Empty, true);

            // Print all Active Skills with a total value greater than 0 (as opposed to only printing those with a Rating higher than 0).
            GlobalOptions.LoadBoolFromRegistry(ref _blnPrintSkillsWithZeroRating, "printzeroratingskills", string.Empty, true);

            // More Lethal Gameplay.
            GlobalOptions.LoadBoolFromRegistry(ref _blnMoreLethalGameplay, "morelethalgameplay", string.Empty, true);

            // Spirit Force Based on Total MAG.
            GlobalOptions.LoadBoolFromRegistry(ref _blnSpiritForceBasedOnTotalMAG, "spiritforcebasedontotalmag", string.Empty, true);

            // Skill Defaulting Includes modifiers.
            bool blnTemp = false;
            GlobalOptions.LoadBoolFromRegistry(ref blnTemp, "skilldefaultingincludesmodifiers", string.Empty, true);

            // Print Expenses.
            GlobalOptions.LoadBoolFromRegistry(ref _blnPrintExpenses, "printexpenses", string.Empty, true);

            // Print Free Expenses.
            GlobalOptions.LoadBoolFromRegistry(ref _blnPrintFreeExpenses, "printfreeexpenses", string.Empty, true);

            // Nuyen per Build Point
            GlobalOptions.LoadDecFromRegistry(ref _decNuyenPerBP, "nuyenperbp", string.Empty, true);

            // Free Contacts Multiplier Enabled
            GlobalOptions.LoadBoolFromRegistry(ref _blnFreeContactsMultiplierEnabled, "freekarmacontactsmultiplierenabled", string.Empty, true);

            // Free Contacts Multiplier Value
            GlobalOptions.LoadInt32FromRegistry(ref _intFreeContactsMultiplier, "freekarmacontactsmultiplier", string.Empty, true);

            // Free Knowledge Multiplier Enabled
            GlobalOptions.LoadBoolFromRegistry(ref _blnFreeKnowledgeMultiplierEnabled, "freekarmaknowledgemultiplierenabled", string.Empty, true);

            // Free Knowledge Multiplier Value
            GlobalOptions.LoadInt32FromRegistry(ref _intFreeKnowledgeMultiplier, "freekarmaknowledgemultiplier", string.Empty, true);

            // Karma Free Knowledge Multiplier Enabled
            GlobalOptions.LoadBoolFromRegistry(ref _blnFreeKnowledgeMultiplierEnabled, "freeknowledgemultiplierenabled", string.Empty, true);

            // No Single Armor Encumbrance
            GlobalOptions.LoadBoolFromRegistry(ref _blnNoSingleArmorEncumbrance, "nosinglearmorencumbrance", string.Empty, true);

            // Essence Loss Reduces Maximum Only.
            GlobalOptions.LoadBoolFromRegistry(ref _blnESSLossReducesMaximumOnly, "esslossreducesmaximumonly", string.Empty, true);

            // Allow Skill Regrouping.
            GlobalOptions.LoadBoolFromRegistry(ref _blnAllowSkillRegrouping, "allowskillregrouping", string.Empty, true);

            // Attempt to populate the Karma values.
            GlobalOptions.LoadInt32FromRegistry(ref _intKarmaAttribute, "karmaattribute", string.Empty, true);
            GlobalOptions.LoadInt32FromRegistry(ref _intKarmaQuality, "karmaquality", string.Empty, true);
            GlobalOptions.LoadInt32FromRegistry(ref _intKarmaSpecialization, "karmaspecialization", string.Empty, true);
            GlobalOptions.LoadInt32FromRegistry(ref _intKarmaKnoSpecialization, "karmaknospecialization", string.Empty, true);
            GlobalOptions.LoadInt32FromRegistry(ref _intKarmaNewKnowledgeSkill, "karmanewknowledgeskill", string.Empty, true);
            GlobalOptions.LoadInt32FromRegistry(ref _intKarmaNewActiveSkill, "karmanewactiveskill", string.Empty, true);
            GlobalOptions.LoadInt32FromRegistry(ref _intKarmaNewSkillGroup, "karmanewskillgroup", string.Empty, true);
            GlobalOptions.LoadInt32FromRegistry(ref _intKarmaImproveKnowledgeSkill, "karmaimproveknowledgeskill", string.Empty, true);
            GlobalOptions.LoadInt32FromRegistry(ref _intKarmaImproveActiveSkill, "karmaimproveactiveskill", string.Empty, true);
            GlobalOptions.LoadInt32FromRegistry(ref _intKarmaImproveSkillGroup, "karmaimproveskillgroup", string.Empty, true);
            GlobalOptions.LoadInt32FromRegistry(ref _intKarmaSpell, "karmaspell", string.Empty, true);
            GlobalOptions.LoadInt32FromRegistry(ref _intKarmaEnhancement, "karmaenhancement", string.Empty, true);
            GlobalOptions.LoadInt32FromRegistry(ref _intKarmaNewComplexForm, "karmanewcomplexform", string.Empty, true);
            GlobalOptions.LoadInt32FromRegistry(ref _intKarmaImproveComplexForm, "karmaimprovecomplexform", string.Empty, true);
            GlobalOptions.LoadInt32FromRegistry(ref _intKarmaNewAIProgram, "karmanewaiprogram", string.Empty, true);
            GlobalOptions.LoadInt32FromRegistry(ref _intKarmaNewAIAdvancedProgram, "karmanewaiadvancedprogram", string.Empty, true);
            GlobalOptions.LoadInt32FromRegistry(ref _intKarmaNuyenPer, "karmanuyenper", string.Empty, true);
            GlobalOptions.LoadInt32FromRegistry(ref _intKarmaContact, "karmacontact", string.Empty, true);
            GlobalOptions.LoadInt32FromRegistry(ref _intKarmaEnemy, "karmaenemy", string.Empty, true);
            GlobalOptions.LoadInt32FromRegistry(ref _intKarmaCarryover, "karmacarryover", string.Empty, true);
            GlobalOptions.LoadInt32FromRegistry(ref _intKarmaSpirit, "karmaspirit", string.Empty, true);
            GlobalOptions.LoadInt32FromRegistry(ref _intKarmaManeuver, "karmamaneuver", string.Empty, true);
            GlobalOptions.LoadInt32FromRegistry(ref _intKarmaInitiation, "karmainitiation", string.Empty, true);
            GlobalOptions.LoadInt32FromRegistry(ref _intKarmaInitiationFlat, "karmainitiationflat", string.Empty, true);
            GlobalOptions.LoadInt32FromRegistry(ref _intKarmaMetamagic, "karmametamagic", string.Empty, true);
            GlobalOptions.LoadInt32FromRegistry(ref _intKarmaComplexFormOption, "karmacomplexformoption", string.Empty, true);

            // Retrieve the sourcebooks that are in the Registry.
            string strBookList;
            object objBooksKeyValue = GlobalOptions.ChummerRegistryKey.GetValue("books");
            if (objBooksKeyValue != null)
            {
                strBookList = objBooksKeyValue.ToString();
            }
            else
            {
                // We were unable to get the Registry key which means the book options have not been saved yet, so create the default values.
                strBookList = "Shadowrun 5th Edition";
                GlobalOptions.ChummerRegistryKey.SetValue("books", strBookList);
            }
            string[] strBooks = strBookList.Split(',');

            XmlDocument objXmlDocument = XmlManager.Load("books.xml");

            foreach (string strBookName in strBooks)
            {
                string strCode = objXmlDocument.SelectSingleNode("/chummer/books/book[name = " + strBookName.CleanXPath() + " and not(hide)]/code")?.InnerText;
                if (!string.IsNullOrEmpty(strCode))
                {
                    _lstBooks.Add(strCode);
                }
            }
            RecalculateBookXPath();

            // Delete the Registry keys ones the values have been retrieve since they will no longer be used.
            GlobalOptions.ChummerRegistryKey.DeleteValue("books");
        }

        /// <summary>
        /// Determine whether or not a given book is in use.
        /// </summary>
        /// <param name="strCode">Book code to search for.</param>
        public bool BookEnabled(string strCode)
        {
            return _lstBooks.Contains(strCode);
        }

        /// <summary>
        /// XPath query used to filter items based on the user's selected source books and optional rules.
        /// </summary>
        public string BookXPath(bool excludeHidden = true)
        {
            string strPath = string.Empty;

            if (excludeHidden)
            {
                strPath = "not(hide)";
            }
            if (string.IsNullOrWhiteSpace(_strBookXPath))
            {
                RecalculateBookXPath();
            }
            if (string.IsNullOrWhiteSpace(strPath))
            {
                strPath = _strBookXPath;
            }
            else
            {
                strPath += $" and {_strBookXPath}";
            }
            if (!GlobalOptions.Dronemods)
            {
                strPath += " and not(optionaldrone)";
            }
            return strPath;
        }

        /// <summary>
        /// XPath query used to filter items based on the user's selected source books.
        /// </summary>
        public void RecalculateBookXPath()
        {
            StringBuilder strBookXPath = new StringBuilder("(");
            _strBookXPath = string.Empty;

            foreach (string strBook in _lstBooks)
            {
                if (!string.IsNullOrWhiteSpace(strBook))
                {
                    strBookXPath.Append("source = \"");
                    strBookXPath.Append(strBook);
                    strBookXPath.Append("\" or ");
                }
            }
            if (strBookXPath.Length >= 4)
            {
                strBookXPath.Length -= 4;
                strBookXPath.Append(')');
                _strBookXPath = strBookXPath.ToString();
            }
            else
                _strBookXPath = string.Empty;
        }

        /// <summary>
        /// Whether or not all Active Skills with a total score higher than 0 should be printed.
        /// </summary>
        public bool PrintSkillsWithZeroRating
        {
            get => _blnPrintSkillsWithZeroRating;
            set => _blnPrintSkillsWithZeroRating = value;
        }

        /// <summary>
        /// Whether or not the More Lethal Gameplay optional rule is enabled.
        /// </summary>
        public bool MoreLethalGameplay
        {
            get => _blnMoreLethalGameplay;
            set => _blnMoreLethalGameplay = value;
        }

        /// <summary>
        /// Whether or not to require licensing restricted items.
        /// </summary>
        public bool LicenseRestricted
        {
            get => _blnLicenseRestrictedItems;
            set => _blnLicenseRestrictedItems = value;
        }

        /// <summary>
        /// Whether or not a Spirit's Maximum Force is based on the character's total MAG.
        /// </summary>
        public bool SpiritForceBasedOnTotalMAG
        {
            get => _blnSpiritForceBasedOnTotalMAG;
            set => _blnSpiritForceBasedOnTotalMAG = value;
        }

        /// <summary>
        /// Whether or not the Karma and Nuyen Expenses should be printed on the character sheet.
        /// </summary>
        public bool PrintExpenses
        {
            get => _blnPrintExpenses;
            set => _blnPrintExpenses = value;
        }

        /// <summary>
        /// Whether or not the Karma and Nuyen Expenses that have a cost of 0 should be printed on the character sheet.
        /// </summary>
        public bool PrintFreeExpenses
        {
            get => _blnPrintFreeExpenses;
            set => _blnPrintFreeExpenses = value;
        }

        /// <summary>
        /// Amount of Nuyen gained per BP spent.
        /// </summary>
        public decimal NuyenPerBP
        {
            get => _decNuyenPerBP;
            set => _decNuyenPerBP = value;
        }

        /// <summary>
        /// Whether or not UnarmedAP, UnarmedReach and UnarmedDV Improvements apply to weapons that use the Unarmed Combat skill.
        /// </summary>
        public bool UnarmedImprovementsApplyToWeapons
        {
            get => _blnUnarmedImprovementsApplyToWeapons;
            set => _blnUnarmedImprovementsApplyToWeapons = value;
        }

        /// <summary>
        /// Whether or not characters may use Initiation/Submersion in Create mode.
        /// </summary>
        public bool AllowInitiationInCreateMode
        {
            get => _blnAllowInitiationInCreateMode;
            set
            {
                if (_blnAllowInitiationInCreateMode != value)
                {
                    _blnAllowInitiationInCreateMode = value;
                    _character?.OnPropertyChanged(nameof(Character.AddInitiationsAllowed));
                }
            }
        }

        /// <summary>
        /// Whether or not characters can spend skill points on broken groups.
        /// </summary>
        public bool UsePointsOnBrokenGroups
        {
            get => _blnUsePointsOnBrokenGroups;
            set => _blnUsePointsOnBrokenGroups = value;
        }

        /// <summary>
        /// Whether or not characters in Career Mode should pay double for qualities.
        /// </summary>
        public bool DontDoubleQualityPurchases
        {
            get => _blnDontDoubleQualityPurchaseCost;
            set => _blnDontDoubleQualityPurchaseCost = value;
        }

        /// <summary>
        /// Whether or not characters in Career Mode should pay double for removing Negative Qualities.
        /// </summary>
        public bool DontDoubleQualityRefunds
        {
            get => _blnDontDoubleQualityRefundCost;
            set => _blnDontDoubleQualityRefundCost = value;
        }

        /// <summary>
        /// Whether or not to ignore the art requirements from street grimoire.
        /// </summary>
        public bool IgnoreArt
        {
            get => _blnIgnoreArt;
            set => _blnIgnoreArt = value;
        }

        /// <summary>
        /// Whether or not to ignore the limit on Complex Forms in Career mode.
        /// </summary>
        public bool IgnoreComplexFormLimit
        {
            get => _blnIgnoreComplexFormLimit;
            set => _blnIgnoreComplexFormLimit = value;
        }

        /// <summary>
        /// Whether or not to use stats from Cyberlegs when calculating movement rates
        /// </summary>
        public bool CyberlegMovement
        {
            get => _blnCyberlegMovement;
            set => _blnCyberlegMovement = value;
        }

        /// <summary>
        /// Allow Mystic Adepts to increase their power points during career mode
        /// </summary>
        public bool MysAdeptAllowPPCareer
        {
            get => _blnMysAdeptAllowPPCareer;
            set
            {
                if (_blnMysAdeptAllowPPCareer != value)
                {
                    _blnMysAdeptAllowPPCareer = value;
                    _character?.OnPropertyChanged(nameof(Character.MysAdeptAllowPPCareer));
                }
            }
        }

        /// <summary>
        /// Split MAG for Mystic Adepts so that they have a separate MAG rating for Adept Powers instead of using the special PP rules for mystic adepts
        /// </summary>
        public bool MysAdeptSecondMAGAttribute
        {
            get => _blnMysAdeptSecondMAGAttribute;
            set
            {
                if (_blnMysAdeptSecondMAGAttribute != value)
                {
                    _blnMysAdeptSecondMAGAttribute = value;
                    _character?.OnPropertyChanged(nameof(Character.UseMysticAdeptPPs));
                }
            }
        }

        /// <summary>
        /// Whether or not to allow a 2nd max attribute with Exceptional Attribute
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public bool Allow2ndMaxAttribute
        {
            get => _blnAllow2ndMaxAttribute;
            set => _blnAllow2ndMaxAttribute = value;
        }

        /// <summary>
        /// The CHA multiplier to be used with the Free Contacts Option.
        /// </summary>
        public int FreeContactsMultiplier
        {
            get
            {
                if (_character?.GameplayOption == "Prime Runner") return _intFreeContactsMultiplier*2;  //HACK. Should really read from gameplayoptions
                return _intFreeContactsMultiplier;
            }
            set
            {
                //As this is a hack, not quite sure how this is glued together.
                //If i understand it right (COMUNICATING TROUGHT FUCKING FILES?) this should never happen.
                //Keyword should
                if (_character == null)
                {
                    _intFreeContactsMultiplier = value;
                }
                else
                {
                    Utils.BreakIfDebug();
                }
            }
        }

        /// <summary>
        /// Whether or not characters get a flat number of BP for free Contacts.
        /// </summary>
        public bool FreeContactsMultiplierEnabled
        {
            get => _blnFreeContactsMultiplierEnabled;
            set => _blnFreeContactsMultiplierEnabled = value;
        }

        /// <summary>
        /// The Drone Body multiplier for maximal Armor
        /// </summary>
        public int DroneArmorMultiplier
        {
            get => _intDroneArmorMultiplier;
            set => _intDroneArmorMultiplier = value;
        }

        /// <summary>
        /// Whether or not Armor
        /// </summary>
        public bool DroneArmorMultiplierEnabled
        {
            get => _blnDroneArmorMultiplierEnabled;
            set => _blnDroneArmorMultiplierEnabled = value;
        }

        /// <summary>
        /// Whether or not the multiplier for Free Knowledge points are used.
        /// </summary>
        public bool FreeKnowledgeMultiplierEnabled
        {
            get => _blnFreeKnowledgeMultiplierEnabled;
            set => _blnFreeKnowledgeMultiplierEnabled = value;
        }

        /// <summary>
        /// The INT+LOG multiplier to be used with the Free Knowledge Option.
        /// </summary>
        public int FreeKnowledgeMultiplier
        {
            get => _intFreeKnowledgeMultiplier;
            set => _intFreeKnowledgeMultiplier = value;
        }

        /// <summary>
        /// House Rule: Ignore Armor Encumbrance entirely.
        /// </summary>
        public bool NoArmorEncumbrance
        {
            get => _blnNoArmorEncumbrance;
            set => _blnNoArmorEncumbrance = value;
        }

        /// <summary>
        /// Whether or not Essence loss only reduces MAG/RES maximum value, not the current value.
        /// </summary>
        public bool ESSLossReducesMaximumOnly
        {
            get => _blnESSLossReducesMaximumOnly;
            set => _blnESSLossReducesMaximumOnly = value;
        }

        /// <summary>
        /// Whether or not characters are allowed to put points into a Skill Group again once it is broken and all Ratings are the same.
        /// </summary>
        public bool AllowSkillRegrouping
        {
            get => _blnAllowSkillRegrouping;
            set => _blnAllowSkillRegrouping = value;
        }

        /// <summary>
        /// Whether or not confirmation messages are shown when deleting an object.
        /// </summary>
        public bool ConfirmDelete
        {
            get => _blnConfirmDelete;
            set => _blnConfirmDelete = value;
        }

        /// <summary>
        /// Wehther or not confirmation messages are shown for Karma Expenses.
        /// </summary>
        public bool ConfirmKarmaExpense
        {
            get => _blnConfirmKarmaExpense;
            set => _blnConfirmKarmaExpense = value;
        }

        /// <summary>
        /// Sourcebooks.
        /// </summary>
        public ICollection<string> Books => _lstBooks;

        /// <summary>
        /// Names of custom data directories
        /// </summary>
        public Dictionary<string,bool> CustomDataDirectoryNames => _dicCustomDataDirectoryNames;

        /// <summary>
        /// Setting name.
        /// </summary>
        public string Name
        {
            get => _strName;
            set => _strName = value;
        }

        /// <summary>
        /// Whether or not Metatypes cost Karma equal to their BP when creating a character with Karma.
        /// </summary>
        public bool MetatypeCostsKarma
        {
            get => _blnMetatypeCostsKarma;
            set => _blnMetatypeCostsKarma = value;
        }

        /// <summary>
        /// Mutiplier for Metatype Karma Costs when converting from BP to Karma.
        /// </summary>
        public int MetatypeCostsKarmaMultiplier
        {
            get => _intMetatypeCostMultiplier;
            set => _intMetatypeCostMultiplier = value;
        }

        /// <summary>
        /// Number of Limbs a standard character has.
        /// </summary>
        public int LimbCount
        {
            get => _intLimbCount;
            set => _intLimbCount = value;
        }

        /// <summary>
        /// Exclude a particular Limb Slot from count towards the Limb Count.
        /// </summary>
        public string ExcludeLimbSlot
        {
            get => _strExcludeLimbSlot;
            set => _strExcludeLimbSlot = value;
        }

        /// <summary>
        /// Allow Cyberware Essence cost discounts.
        /// </summary>
        public bool AllowCyberwareESSDiscounts
        {
            get => _blnAllowCyberwareESSDiscounts;
            set => _blnAllowCyberwareESSDiscounts = value;
        }
        
        /// <summary>
        /// Whether or not Armor Degredation is allowed.
        /// </summary>
        public bool ArmorDegradation
        {
            get => _blnArmorDegradation;
            set
            {
                if (_blnArmorDegradation != value)
                {
                    _blnArmorDegradation = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ArmorDegradation)));
                }
            }
        }
        
        /// <summary>
        /// If true, karma costs will not decrease from reductions due to essence loss. Effectively, essence loss becomes an augmented modifier, not one that alters minima and maxima.
        /// </summary>
        public bool SpecialKarmaCostBasedOnShownValue
        {
            get => _blnSpecialKarmaCostBasedOnShownValue;
            set
            {
                if (_blnSpecialKarmaCostBasedOnShownValue != value)
                {
                    _blnSpecialKarmaCostBasedOnShownValue = value;
                    _character?.RefreshEssenceLossImprovements();
                }
            }
        }

        /// <summary>
        /// Whether or not characters can have more than 25 BP in Positive Qualities.
        /// </summary>
        public bool ExceedPositiveQualities
        {
            get => _blnExceedPositiveQualities;
            set => _blnExceedPositiveQualities = value;
        }

        /// <summary>
        /// If true, the karma cost of qualities is doubled after the initial 25.
        /// </summary>
        public bool ExceedPositiveQualitiesCostDoubled
        {
            get => _blnExceedPositiveQualitiesCostDoubled;
            set => _blnExceedPositiveQualitiesCostDoubled = value;
        }

        /// <summary>
        /// Whether or not characters can have more than 25 BP in Negative Qualities.
        /// </summary>
        public bool ExceedNegativeQualities
        {
            get => _blnExceedNegativeQualities;
            set => _blnExceedNegativeQualities = value;
        }

        /// <summary>
        /// If true, the character will not receive additional BP from Negative Qualities past the initial 25
        /// </summary>
        public bool ExceedNegativeQualitiesLimit
        {
            get => _blnExceedNegativeQualitiesLimit;
            set => _blnExceedNegativeQualitiesLimit = value;
        }
        
        /// <summary>
        /// Whether or not Restricted items have their cost multiplied.
        /// </summary>
        public bool MultiplyRestrictedCost
        {
            get => _blnMultiplyRestrictedCost;
            set => _blnMultiplyRestrictedCost = value;
        }

        /// <summary>
        /// Whether or not Forbidden items have their cost multiplied.
        /// </summary>
        public bool MultiplyForbiddenCost
        {
            get => _blnMultiplyForbiddenCost;
            set => _blnMultiplyForbiddenCost = value;
        }

        /// <summary>
        /// Cost multiplier for Restricted items.
        /// </summary>
        public int RestrictedCostMultiplier
        {
            get => _intRestrictedCostMultiplier;
            set => _intRestrictedCostMultiplier = value;
        }

        /// <summary>
        /// Cost multiplier for Forbidden items.
        /// </summary>
        public int ForbiddenCostMultiplier
        {
            get => _intForbiddenCostMultiplier;
            set => _intForbiddenCostMultiplier = value;
        }

        private int _intCachedNuyenDecimals = -1;
        /// <summary>
        /// Number of decimal places to round to when diplaying nuyen values.
        /// </summary>
        public int NuyenDecimals
        {
            get
            {
                if (_intCachedNuyenDecimals >= 0)
                    return _intCachedNuyenDecimals;
                string strNuyenFormat = NuyenFormat;
                int intDecimalPlaces = strNuyenFormat.IndexOf('.');
                if (intDecimalPlaces == -1)
                    intDecimalPlaces = 0;
                else
                    intDecimalPlaces = strNuyenFormat.Length - intDecimalPlaces - 1;

                return _intCachedNuyenDecimals = intDecimalPlaces;
            }
            set
            {
                int intCurrentNuyenDecimals = NuyenDecimals;
                int intNewNuyenDecimals = Math.Max(value, 0);
                if (intNewNuyenDecimals < intCurrentNuyenDecimals)
                {
                    if (intNewNuyenDecimals > 0)
                        NuyenFormat = NuyenFormat.Substring(0, NuyenFormat.Length - (intNewNuyenDecimals - intCurrentNuyenDecimals));
                    else
                    {
                        int intDecimalPlaces = NuyenFormat.IndexOf('.');
                        if (intDecimalPlaces != -1)
                            NuyenFormat = NuyenFormat.Substring(0, intDecimalPlaces);
                    }
                }
                else if (intNewNuyenDecimals > intCurrentNuyenDecimals)
                {
                    StringBuilder objNuyenFormat = string.IsNullOrEmpty(NuyenFormat) ? new StringBuilder("#,0") : new StringBuilder(NuyenFormat);
                    if (intCurrentNuyenDecimals == 0)
                    {
                        objNuyenFormat.Append(".");
                        for (int i = 0; i < intNewNuyenDecimals; ++i)
                        {
                            objNuyenFormat.Append("0");
                        }
                    }
                    else
                    {
                        string strDecimalTypeToAdd = string.IsNullOrEmpty(NuyenFormat) ? "0" : NuyenFormat[NuyenFormat.Length - 1].ToString();
                        intNewNuyenDecimals -= intCurrentNuyenDecimals;
                        for (int i = 0; i < intNewNuyenDecimals; ++i)
                        {
                            objNuyenFormat.Append(strDecimalTypeToAdd);
                        }
                    }
                    NuyenFormat = objNuyenFormat.ToString();
                }
            }
        }

        /// <summary>
        /// Format in which nuyen values should be displayed (does not include nuyen symbol).
        /// </summary>
        public string NuyenFormat
        {
            get => _strNuyenFormat;
            set
            {
                if (_strNuyenFormat != value)
                {
                    _strNuyenFormat = value;
                    _intCachedNuyenDecimals = -1;
                    _character?.OnMultiplePropertyChanged(nameof(Character.DisplayNuyen), nameof(Character.DisplayCareerNuyen));
                }
            }
        }

        private int _intCachedEssenceDecimals = -1;

        /// <summary>
        /// Number of decimal places to round to when calculating Essence.
        /// </summary>
        public int EssenceDecimals
        {
            get
            {
                if (_intCachedEssenceDecimals >= 0)
                    return _intCachedEssenceDecimals;
                string strEssenceFormat = EssenceFormat;
                int intDecimalPlaces = strEssenceFormat.IndexOf('.');
                if (intDecimalPlaces == -1)
                    intDecimalPlaces = 0;
                else
                    intDecimalPlaces = strEssenceFormat.Length - intDecimalPlaces - 1;

                return _intCachedEssenceDecimals = intDecimalPlaces;
            }
            set
            {
                int intCurrentEssenceDecimals = EssenceDecimals;
                int intNewEssenceDecimals = Math.Max(value, 0);
                if (intNewEssenceDecimals < intCurrentEssenceDecimals)
                {
                    if (intNewEssenceDecimals > 0)
                    {
                        int length = EssenceFormat.Length - (intCurrentEssenceDecimals - intNewEssenceDecimals);
                        if (length < 3) length = 3;
                        EssenceFormat = EssenceFormat.Substring(0, length);
                    }
                    else
                    {
                        int intDecimalPlaces = EssenceFormat.IndexOf('.');
                        if (intDecimalPlaces != -1)
                            EssenceFormat = EssenceFormat.Substring(0, intDecimalPlaces);
                    }
                }
                else if (intNewEssenceDecimals > intCurrentEssenceDecimals)
                {
                    StringBuilder objEssenceFormat = string.IsNullOrEmpty(EssenceFormat) ? new StringBuilder("#,0") : new StringBuilder(EssenceFormat);
                    if (intCurrentEssenceDecimals == 0)
                    {
                        objEssenceFormat.Append(".");
                    }
                    intNewEssenceDecimals -= intCurrentEssenceDecimals;
                    for (int i = 0; i < intNewEssenceDecimals; ++i)
                    {
                        objEssenceFormat.Append("0");
                    }
                    EssenceFormat = objEssenceFormat.ToString();
                }
            }
        }

        /// <summary>
        /// Display format for Essence.
        /// </summary>
        public string EssenceFormat
        {
            get => _strEssenceFormat;
            set
            {
                if (_strEssenceFormat != value)
                {
                    _strEssenceFormat = value;
                    _intCachedEssenceDecimals = -1;
                    _character?.OnMultiplePropertyChanged(nameof(Character.PrototypeTranshumanEssenceUsed), nameof(Character.Essence));
                }
            }
        }

        /// <summary>
        /// Only round essence when its value is displayed
        /// </summary>
        public bool DontRoundEssenceInternally
        {
            get => _blnDoNotRoundEssenceInternally;
            set
            {
                if (_blnDoNotRoundEssenceInternally != value)
                {
                    _blnDoNotRoundEssenceInternally = value;
                    _character?.OnMultiplePropertyChanged(nameof(Character.PrototypeTranshumanEssenceUsed), nameof(Character.Essence));
                }
            }
        }

        /// <summary>
        /// Do Enemies count towards Negative Quality Karma limit in create mode?
        /// </summary>
        public bool EnemyKarmaQualityLimit
        {
            get => _blnEnemyKarmaQualityLimit;
            set => _blnEnemyKarmaQualityLimit = value;
        }

        /// <summary>
        /// Whether or not Capacity limits should be enforced.
        /// </summary>
        public bool EnforceCapacity
        {
            get => _blnEnforceCapacity;
            set => _blnEnforceCapacity = value;
        }

        /// <summary>
        /// Whether or not Recoil modifiers are restricted (AR 148).
        /// </summary>
        public bool RestrictRecoil
        {
            get => _blnRestrictRecoil;
            set => _blnRestrictRecoil = value;
        }
        
        /// <summary>
        /// Whether or not characters are unresicted in the number of points they can invest in Nuyen.
        /// </summary>
        public bool UnrestrictedNuyen
        {
            get => _blnUnrestrictedNuyen;
            set
            {
                if (_blnUnrestrictedNuyen != value)
                {
                    _blnUnrestrictedNuyen = value;
                    _character?.OnPropertyChanged(nameof(Character.TotalNuyenMaximumBP));
                }
            }
        }

        /// <summary>
        /// Whether or not Stacked Foci can have a combined Force higher than 6.
        /// </summary>
        public bool AllowHigherStackedFoci
        {
            get => _blnAllowHigherStackedFoci;
            set => _blnAllowHigherStackedFoci = value;
        }

        /// <summary>
        /// Whether or not the user can change the Part of Base Weapon flag for a Weapon Accessory or Mod.
        /// </summary>
        public bool AllowEditPartOfBaseWeapon
        {
            get => _blnAllowEditPartOfBaseWeapon;
            set => _blnAllowEditPartOfBaseWeapon = value;
        }

        /// <summary>
        /// Whether or not the user is allowed to break Skill Groups while in Create Mode.
        /// </summary>
        public bool StrictSkillGroupsInCreateMode
        {
            get => _blnStrictSkillGroupsInCreateMode;
            set => _blnStrictSkillGroupsInCreateMode = value;
        }

        /// <summary>
        /// Whether or not the user is allowed to buy specializations with skill points for skills only bought with karma.
        /// </summary>
        public bool AllowPointBuySpecializationsOnKarmaSkills
        {
            get => _blnAllowPointBuySpecializationsOnKarmaSkills;
            set => _blnAllowPointBuySpecializationsOnKarmaSkills = value;
        }

        /// <summary>
        /// Whether or not any Detection Spell can be taken as Extended range version.
        /// </summary>
        public bool ExtendAnyDetectionSpell
        {
            get => _blnExtendAnyDetectionSpell;
            set => _blnExtendAnyDetectionSpell = value;
        }

        /// <summary>
        /// Whether or not dice rolling is allowed for Skills.
        /// </summary>
        public bool AllowSkillDiceRolling
        {
            get => _blnAllowSkillDiceRolling;
            set => _blnAllowSkillDiceRolling = value;
        }

        /// <summary>
        /// Whether or not cyberlimbs stats are used in attribute calculation
        /// </summary>
        public bool DontUseCyberlimbCalculation
        {
            get => _blnDontUseCyberlimbCalculation;
            set => _blnDontUseCyberlimbCalculation = value;
        }

        /// <summary>
        /// House rule: Treat the Metatype Attribute Minimum as 1 for the purpose of calculating Karma costs.
        /// </summary>
        public bool AlternateMetatypeAttributeKarma
        {
            get => _blnAlternateMetatypeAttributeKarma;
            set => _blnAlternateMetatypeAttributeKarma = value;
        }

        /// <summary>
        /// House rule: Whether to compensate for the karma cost difference between raising skill ratings and skill groups when increasing the rating of the last skill in the group
        /// </summary>
        public bool CompensateSkillGroupKarmaDifference
        {
            get => _blnCompensateSkillGroupKarmaDifference;
            set => _blnCompensateSkillGroupKarmaDifference = value;
        }

        /// <summary>
        /// Whether or not Notes should be printed.
        /// </summary>
        public bool PrintNotes
        {
            get => _blnPrintNotes;
            set => _blnPrintNotes = value;
        }

        /// <summary>
        /// Whether or not Obsolescent can be removed/upgraded in the same way as Obsolete.
        /// </summary>
        public bool AllowObsolescentUpgrade
        {
            get => _blnAllowObsolescentUpgrade;
            set => _blnAllowObsolescentUpgrade = value;
        }

        /// <summary>
        /// Whether or not Bioware Suites can be added and created.
        /// </summary>
        public bool AllowBiowareSuites
        {
            get => _blnAllowBiowareSuites;
            set => _blnAllowBiowareSuites = value;
        }

        /// <summary>
        /// House rule: Free Spirits calculate their Power Points based on their MAG instead of EDG.
        /// </summary>
        public bool FreeSpiritPowerPointsMAG
        {
            get => _blnFreeSpiritPowerPointsMAG;
            set => _blnFreeSpiritPowerPointsMAG = value;
        }
        #endregion
        
        #region Karma
        /// <summary>
        /// Karma cost to improve an Attribute = New Rating X this value.
        /// </summary>
        public int KarmaAttribute
        {
            get => _intKarmaAttribute;
            set => _intKarmaAttribute = value;
        }

        /// <summary>
        /// Karma cost to purchase a Quality = BP Cost x this value.
        /// </summary>
        public int KarmaQuality
        {
            get => _intKarmaQuality;
            set => _intKarmaQuality = value;
        }

        /// <summary>
        /// Karma cost to purchase a Specialization for an active skill = this value.
        /// </summary>
        public int KarmaSpecialization
        {
            get => _intKarmaSpecialization;
            set => _intKarmaSpecialization = value;
        }

        /// <summary>
        /// Karma cost to purchase a Specialization for a knowledge skill = this value.
        /// </summary>
        public int KarmaKnowledgeSpecialization
        {
            get => _intKarmaKnoSpecialization;
            set => _intKarmaKnoSpecialization = value;
        }

        /// <summary>
        /// Karma cost to purchase a new Knowledge Skill = this value.
        /// </summary>
        public int KarmaNewKnowledgeSkill
        {
            get => _intKarmaNewKnowledgeSkill;
            set => _intKarmaNewKnowledgeSkill = value;
        }

        /// <summary>
        /// Karma cost to purchase a new Active Skill = this value.
        /// </summary>
        public int KarmaNewActiveSkill
        {
            get => _intKarmaNewActiveSkill;
            set => _intKarmaNewActiveSkill = value;
        }

        /// <summary>
        /// Karma cost to purchase a new Skill Group = this value.
        /// </summary>
        public int KarmaNewSkillGroup
        {
            get => _intKarmaNewSkillGroup;
            set => _intKarmaNewSkillGroup = value;
        }

        /// <summary>
        /// Karma cost to improve a Knowledge Skill = New Rating x this value.
        /// </summary>
        public int KarmaImproveKnowledgeSkill
        {
            get => _intKarmaImproveKnowledgeSkill;
            set => _intKarmaImproveKnowledgeSkill = value;
        }

        /// <summary>
        /// Karma cost to improve an Active Skill = New Rating x this value.
        /// </summary>
        public int KarmaImproveActiveSkill
        {
            get => _intKarmaImproveActiveSkill;
            set => _intKarmaImproveActiveSkill = value;
        }

        /// <summary>
        /// Karma cost to improve a Skill Group = New Rating x this value.
        /// </summary>
        public int KarmaImproveSkillGroup
        {
            get => _intKarmaImproveSkillGroup;
            set => _intKarmaImproveSkillGroup = value;
        }

        /// <summary>
        /// Karma cost for each Spell = this value.
        /// </summary>
        public int KarmaSpell
        {
            get => _intKarmaSpell;
            set => _intKarmaSpell = value;
        }

        /// <summary>
        /// Karma cost for each Enhancement = this value.
        /// </summary>
        public int KarmaEnhancement
        {
            get => _intKarmaEnhancement;
            set => _intKarmaEnhancement = value;
        }

        /// <summary>
        /// Karma cost for a new Complex Form = this value.
        /// </summary>
        public int KarmaNewComplexForm
        {
            get => _intKarmaNewComplexForm;
            set => _intKarmaNewComplexForm = value;
        }

        /// <summary>
        /// Karma cost to improve a Complex Form = New Rating x this value.
        /// </summary>
        public int KarmaImproveComplexForm
        {
            get => _intKarmaImproveComplexForm;
            set => _intKarmaImproveComplexForm = value;
        }

        /// <summary>
        /// Karma cost for Complex Form Options = Rating x this value.
        /// </summary>
        public int KarmaComplexFormOption
        {
            get => _intKarmaComplexFormOption;
            set => _intKarmaComplexFormOption = value;
        }

        /// <summary>
        /// Karma cost for Complex Form Skillsofts = Rating x this value.
        /// </summary>
        public int KarmaComplexFormSkillsoft
        {
            get => _intKarmaComplexFormSkillfot;
            set => _intKarmaComplexFormSkillfot = value;
        }

        /// <summary>
        /// Karma cost for a new AI Program
        /// </summary>
        public int KarmaNewAIProgram
        {
            get => _intKarmaNewAIProgram;
            set => _intKarmaNewAIProgram = value;
        }

        /// <summary>
        /// Karma cost for a new AI Advanced Program
        /// </summary>
        public int KarmaNewAIAdvancedProgram
        {
            get => _intKarmaNewAIAdvancedProgram;
            set => _intKarmaNewAIAdvancedProgram = value;
        }

        /// <summary>
        /// Amount of Nueyn objtained per Karma point.
        /// </summary>
        public int KarmaNuyenPer
        {
            get => _intKarmaNuyenPer;
            set => _intKarmaNuyenPer = value;
        }

        /// <summary>
        /// Karma cost for a Contact = (Connection + Loyalty) x this value.
        /// </summary>
        public int KarmaContact
        {
            get => _intKarmaContact;
            set => _intKarmaContact = value;
        }

        /// <summary>
        /// Karma cost for an Enemy = (Connection + Loyalty) x this value.
        /// </summary>
        public int KarmaEnemy
        {
            get => _intKarmaEnemy;
            set => _intKarmaEnemy = value;
        }

        /// <summary>
        /// Maximum amount of remaining Karma that is carried over to the character once they are created.
        /// </summary>
        public int KarmaCarryover
        {
            get => _intKarmaCarryover;
            set => _intKarmaCarryover = value;
        }

        /// <summary>
        /// Karma cost for a Spirit = this value.regis
        /// </summary>
        public int KarmaSpirit
        {
            get => _intKarmaSpirit;
            set => _intKarmaSpirit = value;
        }

        /// <summary>
        /// Karma cost for a Combat Maneuver = this value.
        /// </summary>
        public int KarmaManeuver
        {
            get => _intKarmaManeuver;
            set => _intKarmaManeuver = value;
        }

        /// <summary>
        /// Karma cost for an Initiation = KarmaInititationFlat + (New Rating x this value).
        /// </summary>
        public int KarmaInitiation
        {
            get => _intKarmaInitiation;
            set => _intKarmaInitiation = value;
        }

        /// <summary>
        /// Karma cost for an Initiation = this value + (New Rating x KarmaInititation).
        /// </summary>
        public int KarmaInititationFlat
        {
            get => _intKarmaInitiationFlat;
            set => _intKarmaInitiationFlat = value;
        }

        /// <summary>
        /// Karma cost for a Metamagic = this value.
        /// </summary>
        public int KarmaMetamagic
        {
            get => _intKarmaMetamagic;
            set => _intKarmaMetamagic = value;
        }

        /// <summary>
        /// Karma cost to join a Group = this value.
        /// </summary>
        public int KarmaJoinGroup
        {
            get => _intKarmaJoinGroup;
            set => _intKarmaJoinGroup = value;
        }

        /// <summary>
        /// Karma cost to leave a Group = this value.
        /// </summary>
        public int KarmaLeaveGroup
        {
            get => _intKarmaLeaveGroup;
            set => _intKarmaLeaveGroup = value;
        }

        /// <summary>
        /// Karma cost for Alchemical Foci.
        /// </summary>
        public int KarmaAlchemicalFocus
        {
            get => _intKarmaAlchemicalFocus;
            set => _intKarmaAlchemicalFocus = value;
        }

        /// <summary>
        /// Karma cost for Banishing Foci.
        /// </summary>
        public int KarmaBanishingFocus
        {
            get => _intKarmaBanishingFocus;
            set => _intKarmaBanishingFocus = value;
        }

        /// <summary>
        /// Karma cost for Binding Foci.
        /// </summary>
        public int KarmaBindingFocus
        {
            get => _intKarmaBindingFocus;
            set => _intKarmaBindingFocus = value;
        }

        /// <summary>
        /// Karma cost for Centering Foci.
        /// </summary>
        public int KarmaCenteringFocus
        {
            get => _intKarmaCenteringFocus;
            set => _intKarmaCenteringFocus = value;
        }

        /// <summary>
        /// Karma cost for Counterspelling Foci.
        /// </summary>
        public int KarmaCounterspellingFocus
        {
            get => _intKarmaCounterspellingFocus;
            set => _intKarmaCounterspellingFocus = value;
        }

        /// <summary>
        /// Karma cost for Disenchanting Foci.
        /// </summary>
        public int KarmaDisenchantingFocus
        {
            get => _intKarmaDisenchantingFocus;
            set => _intKarmaDisenchantingFocus = value;
        }

        /// <summary>
        /// Karma cost for Flexible Signature Foci.
        /// </summary>
        public int KarmaFlexibleSignatureFocus
        {
            get => _intKarmaFlexibleSignatureFocus;
            set => _intKarmaFlexibleSignatureFocus = value;
        }

        /// <summary>
        /// Karma cost for Masking Foci.
        /// </summary>
        public int KarmaMaskingFocus
        {
            get => _intKarmaMaskingFocus;
            set => _intKarmaMaskingFocus = value;
        }

        /// <summary>
        /// Karma cost for Power Foci.
        /// </summary>
        public int KarmaPowerFocus
        {
            get => _intKarmaPowerFocus;
            set => _intKarmaPowerFocus = value;
        }

        /// <summary>
        /// Karma cost for Qi Foci.
        /// </summary>
        public int KarmaQiFocus
        {
            get => _intKarmaQiFocus;
            set => _intKarmaQiFocus = value;
        }

        /// <summary>
        /// Karma cost for Ritual Spellcasting Foci.
        /// </summary>
        public int KarmaRitualSpellcastingFocus
        {
            get => _intKarmaRitualSpellcastingFocus;
            set => _intKarmaRitualSpellcastingFocus = value;
        }

        /// <summary>
        /// Karma cost for Spellcasting Foci.
        /// </summary>
        public int KarmaSpellcastingFocus
        {
            get => _intKarmaSpellcastingFocus;
            set => _intKarmaSpellcastingFocus = value;
        }

        /// <summary>
        /// Karma cost for Spell Shaping Foci.
        /// </summary>
        public int KarmaSpellShapingFocus
        {
            get => _intKarmaSpellShapingFocus;
            set => _intKarmaSpellShapingFocus = value;
        }

        /// <summary>
        /// Karma cost for Summoning Foci.
        /// </summary>
        public int KarmaSummoningFocus
        {
            get => _intKarmaSummoningFocus;
            set => _intKarmaSummoningFocus = value;
        }

        /// <summary>
        /// Karma cost for Sustaining Foci.
        /// </summary>
        public int KarmaSustainingFocus
        {
            get => _intKarmaSustainingFocus;
            set => _intKarmaSustainingFocus = value;
        }

        /// <summary>
        /// Karma cost for Weapon Foci.
        /// </summary>
        public int KarmaWeaponFocus
        {
            get => _intKarmaWeaponFocus;
            set => _intKarmaWeaponFocus = value;
        }

        /// <summary>
        /// How much Karma a single Power Point costs for a Mystic Adept.
        /// </summary>
        public int KarmaMysticAdeptPowerPoint
        {
            get => _intKarmaMysticAdeptPowerPoint;
            set
            {
                if (_intKarmaMysticAdeptPowerPoint != value)
                {
                    _intKarmaMysticAdeptPowerPoint = value;
                    _character?.OnPropertyChanged(nameof(Character.CanAffordCareerPP));
                }
            }
        }

        #endregion

        #region Default Build
        /// <summary>
        /// Whether Life Modules should automatically generate a character background.
        /// </summary>
        public bool AutomaticBackstory
        {
            get => _blnAutomaticBackstory;
            set => _blnAutomaticBackstory = value;
        }

        /// <summary>
        /// Whether to use the rules from SR4 to calculate Public Awareness.
        /// </summary>
        public bool UseCalculatedPublicAwareness
        {
            get => _blnUseCalculatedPublicAwareness;
            set => _blnUseCalculatedPublicAwareness = value;
        }

        /// <summary>
        /// Whether you benefit from augmented values for contact points.
        /// </summary>
        public bool UseTotalValueForFreeContacts
        {
            get => _blnUseTotalValueForFreeContacts;
            set
            {
                if (_blnUseTotalValueForFreeContacts != value)
                {
                    _blnUseTotalValueForFreeContacts = value;
                    _character?.OnPropertyChanged(nameof(Character.ContactPoints));
                }
            }
        }

        /// <summary>
        /// Whether you benefit from augmented values for free knowledge points.
        /// </summary>
        public bool UseTotalValueForFreeKnowledge
        {
            get => _blnUseTotalValueForFreeKnowledge;
            set => _blnUseTotalValueForFreeKnowledge = value;
        }

        /// <summary>
        /// Whether Martial Arts grant a free specialisation in a skill.
        /// </summary>
        public bool FreeMartialArtSpecialization
        {
            get => _blnFreeMartialArtSpecialization;
            set => _blnFreeMartialArtSpecialization = value;
        }

        /// <summary>
        /// Whether Spells from Magic Priority can also be spent on power points.
        /// </summary>
        public bool PrioritySpellsAsAdeptPowers
        {
            get => _blnPrioritySpellsAsAdeptPowers;
            set => _blnPrioritySpellsAsAdeptPowers = value;
        }

        /// <summary>
        /// Last folder from which a mugshot was added
        /// </summary>
        public string RecentImageFolder
        {
            get => _strImageFolder;
            set => _strImageFolder = value;
        }

        /// <summary>
        /// Allows characters to spend their Karma before Priority Points.
        /// </summary>
        public bool ReverseAttributePriorityOrder
        {
            get => _blnReverseAttributePriorityOrder;
            internal set => _blnReverseAttributePriorityOrder = value;
        }

        /// <summary>
        /// Whether items that exceed the Availability Limit should be shown in Create Mode.
        /// </summary>
        public bool HideItemsOverAvailLimit
        {
            get => _blnHideItemsOverAvailLimit;
            set => _blnHideItemsOverAvailLimit = value;
        }

        /// <summary>
        /// Whether or not numeric updowns can increment values of numericupdown controls by hovering over the control.
        /// </summary>
        public bool AllowHoverIncrement
        {
            get => _blnAllowHoverIncrement;
            internal set => _blnAllowHoverIncrement = value;
        }
        /// <summary>
        /// Whether searching in a selection form will limit itself to the current Category that's selected.
        /// </summary>
        public bool SearchInCategoryOnly
        {
            get => _blnSearchInCategoryOnly;
            internal set => _blnSearchInCategoryOnly = value;
        }

        public NumericUpDownEx.InterceptMouseWheelMode InterceptMode => AllowHoverIncrement ? NumericUpDownEx.InterceptMouseWheelMode.WhenMouseOver : NumericUpDownEx.InterceptMouseWheelMode.WhenFocus;

        /// <summary>
        /// Whether to use floor-based rounding for Cyberware. If enabled, 
        /// </summary>
        public bool CyberwareRounding
        {
            get => _cyberwareRounding;
            set => _cyberwareRounding = value;
        }

        /// <summary>
        /// Whether the Improved Ability power (SR5 309) should be capped at 0.5 of current Rating or 1.5 of current Rating. 
        /// </summary>
        public bool IncreasedImprovedAbilityMultiplier
        {
            get => _increasedImprovedAbilityMultiplier;
            set => _increasedImprovedAbilityMultiplier = value;
        }
        /// <summary>
        /// Whether lifestyles will automatically give free grid subscriptions found in (HT)
        /// </summary>
        public bool AllowFreeGrids
        {
            get => _allowFreeGrids;
            set => _allowFreeGrids = value;
        }

        /// <summary>
        /// Whether Technomancers are allowed to use the Schooling discount on their initiations in the same manner as awakened. 
        /// </summary>
        public bool AllowTechnomancerSchooling
        {
            get => _blnAllowTechnomancerSchooling;
            set => _blnAllowTechnomancerSchooling = value;
        }
        /// <summary>
        /// Maximum value of bonuses that can affect cyberlimbs.
        /// </summary>
        public int CyberlimbAttributeBonusCap
        {
            get => _intCyberlimbAttributeBonusCap;
            set => _intCyberlimbAttributeBonusCap = value;
        }

        #endregion

        #region Constant Values
        /// <summary>
        /// The value by which Specializations add to dicepool. 
        /// </summary>
        public int SpecializationBonus = 2;

        #endregion
    }



    public class CustomDataDirectoryInfo : IComparable
    {
        #region Properties

        public string Name { get; set; } = string.Empty;

        public string Path { get; set; } = string.Empty;

        public bool Enabled { get; set; }

        #endregion

        public int CompareTo(object obj)
        {
            if (obj == null)
                return 1;
            if (obj is CustomDataDirectoryInfo objOtherDirectoryInfo)
            {
                int intReturn = string.Compare(Name, objOtherDirectoryInfo.Name, StringComparison.Ordinal);
                if (intReturn == 0)
                {
                    intReturn = string.Compare(Path, objOtherDirectoryInfo.Path, StringComparison.Ordinal);
                    if (intReturn == 0)
                    {
                        intReturn = Enabled == objOtherDirectoryInfo.Enabled ? 0 : (Enabled ? -1 : 1);
                    }
                }

                return intReturn;
            }

            return string.Compare(Name, obj.ToString(), StringComparison.Ordinal);
        }
    }
}
