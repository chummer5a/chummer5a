using System;
using Chummer.UI.Powers;
using Chummer.UI.Attributes;
using Chummer.UI.Shared;

namespace Chummer
{
    partial class frmCreate
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                components?.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.TreeNode treeNode1 = new System.Windows.Forms.TreeNode("Selected Positive Qualities");
            System.Windows.Forms.TreeNode treeNode2 = new System.Windows.Forms.TreeNode("Selected Negative Qualities");
            System.Windows.Forms.TreeNode treeNode3 = new System.Windows.Forms.TreeNode("Selected Martial Arts");
            System.Windows.Forms.TreeNode treeNode4 = new System.Windows.Forms.TreeNode("Selected Qualities");
            System.Windows.Forms.TreeNode treeNode5 = new System.Windows.Forms.TreeNode("Selected Combat Spells");
            System.Windows.Forms.TreeNode treeNode6 = new System.Windows.Forms.TreeNode("Selected Detection Spells");
            System.Windows.Forms.TreeNode treeNode7 = new System.Windows.Forms.TreeNode("Selected Health Spells");
            System.Windows.Forms.TreeNode treeNode8 = new System.Windows.Forms.TreeNode("Selected Illusion Spells");
            System.Windows.Forms.TreeNode treeNode9 = new System.Windows.Forms.TreeNode("Selected Manipulation Spells");
            System.Windows.Forms.TreeNode treeNode10 = new System.Windows.Forms.TreeNode("Selected Rituals");
            System.Windows.Forms.TreeNode treeNode11 = new System.Windows.Forms.TreeNode("Selected Enchantments");
            System.Windows.Forms.TreeNode treeNode12 = new System.Windows.Forms.TreeNode("Selected Complex Forms");
            System.Windows.Forms.TreeNode treeNode13 = new System.Windows.Forms.TreeNode("Selected AI Programs and Advanced Programs");
            System.Windows.Forms.TreeNode treeNode14 = new System.Windows.Forms.TreeNode("Critter Powers");
            System.Windows.Forms.TreeNode treeNode15 = new System.Windows.Forms.TreeNode("Weaknesses");
            System.Windows.Forms.TreeNode treeNode16 = new System.Windows.Forms.TreeNode("Selected Cyberware");
            System.Windows.Forms.TreeNode treeNode17 = new System.Windows.Forms.TreeNode("Selected Bioware");
            System.Windows.Forms.TreeNode treeNode18 = new System.Windows.Forms.TreeNode("Unequipped Modular Cyberware");
            System.Windows.Forms.TreeNode treeNode19 = new System.Windows.Forms.TreeNode("Selected Gear");
            System.Windows.Forms.TreeNode treeNode20 = new System.Windows.Forms.TreeNode("Selected Armor");
            System.Windows.Forms.TreeNode treeNode21 = new System.Windows.Forms.TreeNode("Selected Weapons");
            System.Windows.Forms.TreeNode treeNode22 = new System.Windows.Forms.TreeNode("Selected Drugs");
            System.Windows.Forms.TreeNode treeNode23 = new System.Windows.Forms.TreeNode("Selected Lifestyles");
            System.Windows.Forms.TreeNode treeNode24 = new System.Windows.Forms.TreeNode("Selected Vehicles");
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmCreate));
            this.StatusStrip = new System.Windows.Forms.StatusStrip();
            this.tslKarmaLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.tslKarma = new System.Windows.Forms.ToolStripStatusLabel();
            this.tslKarmaRemainingLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.tslKarmaRemaining = new System.Windows.Forms.ToolStripStatusLabel();
            this.tslEssenceLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.tslEssence = new System.Windows.Forms.ToolStripStatusLabel();
            this.tslNuyenRemainingLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.tslNuyenRemaining = new System.Windows.Forms.ToolStripStatusLabel();
            this.pgbProgress = new System.Windows.Forms.ToolStripProgressBar();
            this.dlgSaveFile = new System.Windows.Forms.SaveFileDialog();
            this.lblAttributesAug = new System.Windows.Forms.Label();
            this.lblAttributesBase = new System.Windows.Forms.Label();
            this.lblAttributesMetatype = new System.Windows.Forms.Label();
            this.lblAttributes = new System.Windows.Forms.Label();
            this.chkArmorEquipped = new System.Windows.Forms.CheckBox();
            this.chkWeaponAccessoryInstalled = new System.Windows.Forms.CheckBox();
            this.lblNotoriety = new System.Windows.Forms.Label();
            this.lblStreetCred = new System.Windows.Forms.Label();
            this.chkCharacterCreated = new System.Windows.Forms.CheckBox();
            this.lblBuildFoci = new System.Windows.Forms.Label();
            this.lblBuildMartialArts = new System.Windows.Forms.Label();
            this.lblBuildNuyen = new System.Windows.Forms.Label();
            this.lblBuildEnemies = new System.Windows.Forms.Label();
            this.lblBuildComplexForms = new System.Windows.Forms.Label();
            this.lblBuildSprites = new System.Windows.Forms.Label();
            this.lblBuildSpirits = new System.Windows.Forms.Label();
            this.lblBuildSpells = new System.Windows.Forms.Label();
            this.lblBuildKnowledgeSkills = new System.Windows.Forms.Label();
            this.lblBuildActiveSkills = new System.Windows.Forms.Label();
            this.lblBuildSkillGroups = new System.Windows.Forms.Label();
            this.lblBuildContacts = new System.Windows.Forms.Label();
            this.lblBuildPrimaryAttributes = new System.Windows.Forms.Label();
            this.lblBuildNegativeQualities = new System.Windows.Forms.Label();
            this.lblBuildPositiveQualities = new System.Windows.Forms.Label();
            this.lblRiggingINILabel = new System.Windows.Forms.Label();
            this.lblMatrixINIHotLabel = new System.Windows.Forms.Label();
            this.lblMatrixINIColdLabel = new System.Windows.Forms.Label();
            this.lblMemoryLabel = new System.Windows.Forms.Label();
            this.lblLiftCarryLabel = new System.Windows.Forms.Label();
            this.lblJudgeIntentionsLabel = new System.Windows.Forms.Label();
            this.lblComposureLabel = new System.Windows.Forms.Label();
            this.lblRemainingNuyenLabel = new System.Windows.Forms.Label();
            this.lblESS = new System.Windows.Forms.Label();
            this.lblArmorLabel = new System.Windows.Forms.Label();
            this.lblAstralINILabel = new System.Windows.Forms.Label();
            this.lblMatrixINILabel = new System.Windows.Forms.Label();
            this.lblINILabel = new System.Windows.Forms.Label();
            this.lblCMStunLabel = new System.Windows.Forms.Label();
            this.lblCMPhysicalLabel = new System.Windows.Forms.Label();
            this.lblKarma = new System.Windows.Forms.Label();
            this.lblSpellDefenseIndirectDodgeLabel = new System.Windows.Forms.Label();
            this.lblSpellDefenseIndirectSoakLabel = new System.Windows.Forms.Label();
            this.lblSpellDefenseDirectSoakManaLabel = new System.Windows.Forms.Label();
            this.lblSpellDefenseDirectSoakPhysicalLabel = new System.Windows.Forms.Label();
            this.lblSpellDefenseDecAttBODLabel = new System.Windows.Forms.Label();
            this.lblSpellDefenseDetectionLabel = new System.Windows.Forms.Label();
            this.lblSpellDefenseDecAttAGILabel = new System.Windows.Forms.Label();
            this.lblSpellDefenseDecAttSTRLabel = new System.Windows.Forms.Label();
            this.lblSpellDefenseDecAttREALabel = new System.Windows.Forms.Label();
            this.lblSpellDefenseDecAttWILLabel = new System.Windows.Forms.Label();
            this.lblSpellDefenseDecAttLOGLabel = new System.Windows.Forms.Label();
            this.lblSpellDefenseDecAttINTLabel = new System.Windows.Forms.Label();
            this.lblSpellDefenseDecAttCHALabel = new System.Windows.Forms.Label();
            this.lblSpellDefenseManipPhysicalLabel = new System.Windows.Forms.Label();
            this.lblSpellDefenseManipMentalLabel = new System.Windows.Forms.Label();
            this.lblSpellDefenseIllusionPhysicalLabel = new System.Windows.Forms.Label();
            this.lblSpellDefenseIllusionManaLabel = new System.Windows.Forms.Label();
            this.lblCounterspellingDiceLabel = new System.Windows.Forms.Label();
            this.lblBuildAIAdvancedPrograms = new System.Windows.Forms.Label();
            this.lblBuildRitualsBPLabel = new System.Windows.Forms.Label();
            this.lblBuildPrepsBPLabel = new System.Windows.Forms.Label();
            this.lblPublicAware = new System.Windows.Forms.Label();
            this.cmsMartialArts = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsMartialArtsAddAdvantage = new System.Windows.Forms.ToolStripMenuItem();
            this.tsMartialArtsNotes = new System.Windows.Forms.ToolStripMenuItem();
            this.cmsSpellButton = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsCreateSpell = new System.Windows.Forms.ToolStripMenuItem();
            this.cmsComplexForm = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsAddComplexFormOption = new System.Windows.Forms.ToolStripMenuItem();
            this.tsComplexFormNotes = new System.Windows.Forms.ToolStripMenuItem();
            this.cmsCyberware = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsCyberwareAddAsPlugin = new System.Windows.Forms.ToolStripMenuItem();
            this.tsCyberwareAddGear = new System.Windows.Forms.ToolStripMenuItem();
            this.tsCyberwareNotes = new System.Windows.Forms.ToolStripMenuItem();
            this.cmsVehicleCyberware = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsVehicleCyberwareAddAsPlugin = new System.Windows.Forms.ToolStripMenuItem();
            this.tsVehicleCyberwareAddGear = new System.Windows.Forms.ToolStripMenuItem();
            this.tsVehicleCyberwareNotes = new System.Windows.Forms.ToolStripMenuItem();
            this.cmsLifestyle = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsAdvancedLifestyle = new System.Windows.Forms.ToolStripMenuItem();
            this.cmsArmor = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsAddArmorMod = new System.Windows.Forms.ToolStripMenuItem();
            this.tsAddArmorGear = new System.Windows.Forms.ToolStripMenuItem();
            this.tsArmorName = new System.Windows.Forms.ToolStripMenuItem();
            this.tsArmorNotes = new System.Windows.Forms.ToolStripMenuItem();
            this.cmsWeapon = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsCreateNaturalWeapon = new System.Windows.Forms.ToolStripMenuItem();
            this.tsWeaponAddAccessory = new System.Windows.Forms.ToolStripMenuItem();
            this.tsWeaponAddUnderbarrel = new System.Windows.Forms.ToolStripMenuItem();
            this.tsWeaponName = new System.Windows.Forms.ToolStripMenuItem();
            this.tsWeaponNotes = new System.Windows.Forms.ToolStripMenuItem();
            this.tsWeaponMountLocation = new System.Windows.Forms.ToolStripMenuItem();
            this.lmtControl = new Chummer.UI.Shared.LimitTabUserControl();
            this.tsWeaponAddModification = new System.Windows.Forms.ToolStripMenuItem();
            this.cmsGearButton = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsGearButtonAddAccessory = new System.Windows.Forms.ToolStripMenuItem();
            this.cmsVehicle = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsVehicleAddWeapon = new System.Windows.Forms.ToolStripMenuItem();
            this.tsVehicleAddWeaponWeapon = new System.Windows.Forms.ToolStripMenuItem();
            this.tsVehicleAddWeaponAccessory = new System.Windows.Forms.ToolStripMenuItem();
            this.tsVehicleAddUnderbarrelWeapon = new System.Windows.Forms.ToolStripMenuItem();
            this.tsVehicleAddWeaponMount = new System.Windows.Forms.ToolStripMenuItem();
            this.tsVehicleAddMod = new System.Windows.Forms.ToolStripMenuItem();
            this.tsVehicleAddCyberware = new System.Windows.Forms.ToolStripMenuItem();
            this.tsVehicleAddSensor = new System.Windows.Forms.ToolStripMenuItem();
            this.tsVehicleAddGear = new System.Windows.Forms.ToolStripMenuItem();
            this.tsVehicleSensorAddAsPlugin = new System.Windows.Forms.ToolStripMenuItem();
            this.tsVehicleName = new System.Windows.Forms.ToolStripMenuItem();
            this.tsVehicleNotes = new System.Windows.Forms.ToolStripMenuItem();
            this.tsVehicleWeaponMountNotes = new System.Windows.Forms.ToolStripMenuItem();
            this.cmsWeaponMount = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsVehicleMountWeapon = new System.Windows.Forms.ToolStripMenuItem();
            this.tsVehicleMountWeaponAdd = new System.Windows.Forms.ToolStripMenuItem();
            this.tsVehicleMountWeaponAccessory = new System.Windows.Forms.ToolStripMenuItem();
            this.tsVehicleMountWeaponUnderbarrel = new System.Windows.Forms.ToolStripMenuItem();
            this.tsEditWeaponMount = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuCreateMenu = new System.Windows.Forms.MenuStrip();
            this.mnuCreateFile = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuFileSave = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuFileSaveAs = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuFileSaveAsCreated = new System.Windows.Forms.ToolStripMenuItem();
            this.tssFileMenu1 = new System.Windows.Forms.ToolStripSeparator();
            this.mnuFileClose = new System.Windows.Forms.ToolStripMenuItem();
            this.tssFileMenu2 = new System.Windows.Forms.ToolStripSeparator();
            this.mnuFilePrint = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuCreateEdit = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuEditCopy = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuEditPaste = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuCreateSpecial = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuSpecialAddCyberwareSuite = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuSpecialAddBiowareSuite = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuSpecialCreateCyberwareSuite = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuSpecialCreateBiowareSuite = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuSpecialAddPACKSKit = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuSpecialCreatePACKSKit = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuSpecialChangeMetatype = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuSpecialChangeOptions = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuSpecialMutantCritter = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuSpecialToxicCritter = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuSpecialCyberzombie = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuSpecialConvertToFreeSprite = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuSpecialReapplyImprovements = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuSpecialBPAvailLimit = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuSpecialConfirmValidity = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuSpecialKarmaValue = new System.Windows.Forms.ToolStripMenuItem();
            this.tsMain = new System.Windows.Forms.ToolStrip();
            this.tsbSave = new System.Windows.Forms.ToolStripButton();
            this.tsbPrint = new System.Windows.Forms.ToolStripButton();
            this.tssSeparator = new System.Windows.Forms.ToolStripSeparator();
            this.tsbCopy = new System.Windows.Forms.ToolStripButton();
            this.tsbPaste = new System.Windows.Forms.ToolStripButton();
            this.cmsGear = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsGearAddAsPlugin = new System.Windows.Forms.ToolStripMenuItem();
            this.tsGearName = new System.Windows.Forms.ToolStripMenuItem();
            this.tsGearNotes = new System.Windows.Forms.ToolStripMenuItem();
            this.tsGearAllowRenameAddAsPlugin = new System.Windows.Forms.ToolStripMenuItem();
            this.tsGearAllowRenameName = new System.Windows.Forms.ToolStripMenuItem();
            this.tsGearAllowRenameNotes = new System.Windows.Forms.ToolStripMenuItem();
            this.cmsVehicleWeapon = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsVehicleAddWeaponAccessoryAlt = new System.Windows.Forms.ToolStripMenuItem();
            this.tsVehicleAddUnderbarrelWeaponAlt = new System.Windows.Forms.ToolStripMenuItem();
            this.tsVehicleWeaponNotes = new System.Windows.Forms.ToolStripMenuItem();
            this.cmsVehicleGear = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsVehicleGearAddAsPlugin = new System.Windows.Forms.ToolStripMenuItem();
            this.tsVehicleGearNotes = new System.Windows.Forms.ToolStripMenuItem();
            this.cmsArmorGear = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsArmorGearAddAsPlugin = new System.Windows.Forms.ToolStripMenuItem();
            this.tsArmorGearNotes = new System.Windows.Forms.ToolStripMenuItem();
            this.cmsArmorMod = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsArmorModNotes = new System.Windows.Forms.ToolStripMenuItem();
            this.tsArmorModAddAsPlugin = new System.Windows.Forms.ToolStripMenuItem();
            this.cmsQuality = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsQualityNotes = new System.Windows.Forms.ToolStripMenuItem();
            this.cmsSpell = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsmSpellNotes = new System.Windows.Forms.ToolStripMenuItem();
            this.cmsCritterPowers = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsCritterPowersNotes = new System.Windows.Forms.ToolStripMenuItem();
            this.cmsLifestyleNotes = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsEditLifestyle = new System.Windows.Forms.ToolStripMenuItem();
            this.tsLifestyleName = new System.Windows.Forms.ToolStripMenuItem();
            this.tsLifestyleNotes = new System.Windows.Forms.ToolStripMenuItem();
            this.cmsWeaponAccessory = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsWeaponAccessoryAddGear = new System.Windows.Forms.ToolStripMenuItem();
            this.tsWeaponAccessoryNotes = new System.Windows.Forms.ToolStripMenuItem();
            this.cmsGearPlugin = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsGearPluginNotes = new System.Windows.Forms.ToolStripMenuItem();
            this.cmsComplexFormPlugin = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsComplexFormPluginNotes = new System.Windows.Forms.ToolStripMenuItem();
            this.cmsBioware = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsBiowareNotes = new System.Windows.Forms.ToolStripMenuItem();
            this.cmsAdvancedLifestyle = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsEditAdvancedLifestyle = new System.Windows.Forms.ToolStripMenuItem();
            this.tsAdvancedLifestyleNotes = new System.Windows.Forms.ToolStripMenuItem();
            this.cmsGearLocation = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsGearRenameLocation = new System.Windows.Forms.ToolStripMenuItem();
            this.tsGearLocationAddGear = new System.Windows.Forms.ToolStripMenuItem();
            this.cmsArmorLocation = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsArmorLocationAddArmor = new System.Windows.Forms.ToolStripMenuItem();
            this.tsArmorRenameLocation = new System.Windows.Forms.ToolStripMenuItem();
            this.cmsCyberwareGear = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsCyberwareGearMenuAddAsPlugin = new System.Windows.Forms.ToolStripMenuItem();
            this.cmsVehicleCyberwareGear = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsVehicleCyberwareGearMenuAddAsPlugin = new System.Windows.Forms.ToolStripMenuItem();
            this.cmsWeaponAccessoryGear = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsWeaponAccessoryGearMenuAddAsPlugin = new System.Windows.Forms.ToolStripMenuItem();
            this.cmsVehicleLocation = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsVehicleRenameLocation = new System.Windows.Forms.ToolStripMenuItem();
            this.tsVehicleLocationAddVehicle = new System.Windows.Forms.ToolStripMenuItem();
            this.cmsVehicleWeaponAccessory = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsVehicleWeaponAccessoryAddGear = new System.Windows.Forms.ToolStripMenuItem();
            this.tsVehicleWeaponAccessoryNotes = new System.Windows.Forms.ToolStripMenuItem();
            this.cmsVehicleWeaponAccessoryGear = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsVehicleWeaponAccessoryGearMenuAddAsPlugin = new System.Windows.Forms.ToolStripMenuItem();
            this.cmsWeaponLocation = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsWeaponRenameLocation = new System.Windows.Forms.ToolStripMenuItem();
            this.tsWeaponLocationAddWeapon = new System.Windows.Forms.ToolStripMenuItem();
            this.splitMain = new System.Windows.Forms.SplitContainer();
            this.tabCharacterTabs = new System.Windows.Forms.TabControl();
            this.tabCommon = new System.Windows.Forms.TabPage();
            this.tlpCommon = new Chummer.BufferedTableLayoutPanel(this.components);
            this.tlpCommonLeftSide = new Chummer.BufferedTableLayoutPanel(this.components);
            this.treQualities = new System.Windows.Forms.TreeView();
            this.lblQualityLevelLabel = new System.Windows.Forms.Label();
            this.tlpQualityButtons = new Chummer.BufferedTableLayoutPanel(this.components);
            this.cmdAddQuality = new System.Windows.Forms.Button();
            this.cmdLifeModule = new System.Windows.Forms.Button();
            this.cmdDeleteQuality = new System.Windows.Forms.Button();
            this.nudQualityLevel = new System.Windows.Forms.NumericUpDown();
            this.tlpCommonLeftSideBottom = new Chummer.BufferedTableLayoutPanel(this.components);
            this.lblQualitySource = new System.Windows.Forms.Label();
            this.lblQualityBP = new System.Windows.Forms.Label();
            this.lblQualityBPLabel = new System.Windows.Forms.Label();
            this.lblQualitySourceLabel = new System.Windows.Forms.Label();
            this.pnlAttributes = new System.Windows.Forms.FlowLayoutPanel();
            this.flpAttributesLabels = new System.Windows.Forms.FlowLayoutPanel();
            this.tlpAlias = new Chummer.BufferedTableLayoutPanel(this.components);
            this.lblAlias = new System.Windows.Forms.Label();
            this.txtAlias = new System.Windows.Forms.TextBox();
            this.tlpCommonRightSide = new Chummer.BufferedTableLayoutPanel(this.components);
            this.lblMetatypeLabel = new System.Windows.Forms.Label();
            this.lblStolenNuyen = new System.Windows.Forms.Label();
            this.lblMysticAdeptAssignment = new System.Windows.Forms.Label();
            this.nudMysticAdeptMAGMagician = new System.Windows.Forms.NumericUpDown();
            this.lblMetatype = new System.Windows.Forms.Label();
            this.lblStolenNuyenLabel = new System.Windows.Forms.Label();
            this.lblMetatypeSourceLabel = new System.Windows.Forms.Label();
            this.lblMetatypeSource = new System.Windows.Forms.Label();
            this.lblNuyen = new System.Windows.Forms.Label();
            this.tlpNuyen = new Chummer.BufferedTableLayoutPanel(this.components);
            this.lblNuyenTotal = new System.Windows.Forms.Label();
            this.nudNuyen = new System.Windows.Forms.NumericUpDown();
            this.tabSkills = new System.Windows.Forms.TabPage();
            this.tabSkillUc = new Chummer.UI.Skills.SkillsTabUserControl();
            this.tabLimits = new System.Windows.Forms.TabPage();
            this.tabMartialArts = new System.Windows.Forms.TabPage();
            this.tlpMartialArts = new Chummer.BufferedTableLayoutPanel(this.components);
            this.treMartialArts = new System.Windows.Forms.TreeView();
            this.lblMartialArtSource = new System.Windows.Forms.Label();
            this.lblMartialArtSourceLabel = new System.Windows.Forms.Label();
            this.tlpMartialArtsButtons = new Chummer.BufferedTableLayoutPanel(this.components);
            this.cmdAddMartialArt = new SplitButton();
            this.cmdDeleteMartialArt = new System.Windows.Forms.Button();
            this.tabMagician = new System.Windows.Forms.TabPage();
            this.tlpMagician = new Chummer.BufferedTableLayoutPanel(this.components);
            this.cmdAddSpirit = new System.Windows.Forms.Button();
            this.panSpirits = new System.Windows.Forms.Panel();
            this.treSpells = new System.Windows.Forms.TreeView();
            this.flpMagician = new System.Windows.Forms.FlowLayoutPanel();
            this.gpbMagicianSpell = new System.Windows.Forms.GroupBox();
            this.tlpMagicianSpell = new System.Windows.Forms.TableLayoutPanel();
            this.lblSpellDescriptorsLabel = new System.Windows.Forms.Label();
            this.lblSpellDescriptors = new System.Windows.Forms.Label();
            this.lblSpellTypeLabel = new System.Windows.Forms.Label();
            this.lblSpellDV = new System.Windows.Forms.Label();
            this.lblSpellType = new System.Windows.Forms.Label();
            this.lblSpellDamageLabel = new System.Windows.Forms.Label();
            this.lblSpellDamage = new System.Windows.Forms.Label();
            this.lblSpellCategoryLabel = new System.Windows.Forms.Label();
            this.lblSpellCategory = new System.Windows.Forms.Label();
            this.lblSpellRangeLabel = new System.Windows.Forms.Label();
            this.lblSpellRange = new System.Windows.Forms.Label();
            this.lblSpellDVLabel = new System.Windows.Forms.Label();
            this.lblSpellDuration = new System.Windows.Forms.Label();
            this.lblSpellDurationLabel = new System.Windows.Forms.Label();
            this.lblSpellDicePoolLabel = new System.Windows.Forms.Label();
            this.lblSpellDicePool = new System.Windows.Forms.Label();
            this.lblSpellSource = new System.Windows.Forms.Label();
            this.lblSpellSourceLabel = new System.Windows.Forms.Label();
            this.gpbMagicianTradition = new System.Windows.Forms.GroupBox();
            this.tlpMagicianTradition = new System.Windows.Forms.TableLayoutPanel();
            this.lblTraditionLabel = new System.Windows.Forms.Label();
            this.cboTradition = new Chummer.ElasticComboBox();
            this.lblDrainAttributesLabel = new System.Windows.Forms.Label();
            this.lblTraditionName = new System.Windows.Forms.Label();
            this.lblTraditionSource = new System.Windows.Forms.Label();
            this.cboSpiritManipulation = new Chummer.ElasticComboBox();
            this.lblTraditionSourceLabel = new System.Windows.Forms.Label();
            this.txtTraditionName = new System.Windows.Forms.TextBox();
            this.lblSpiritCombat = new System.Windows.Forms.Label();
            this.lblSpiritManipulation = new System.Windows.Forms.Label();
            this.cboSpiritCombat = new Chummer.ElasticComboBox();
            this.cboSpiritIllusion = new Chummer.ElasticComboBox();
            this.lblSpiritDetection = new System.Windows.Forms.Label();
            this.cboSpiritDetection = new Chummer.ElasticComboBox();
            this.lblSpiritIllusion = new System.Windows.Forms.Label();
            this.cboSpiritHealth = new Chummer.ElasticComboBox();
            this.lblSpiritHealth = new System.Windows.Forms.Label();
            this.tlpDrainAttributes = new Chummer.BufferedTableLayoutPanel(this.components);
            this.lblDrainAttributesValue = new Chummer.LabelWithToolTip();
            this.lblDrainAttributes = new System.Windows.Forms.Label();
            this.cboDrain = new Chummer.ElasticComboBox();
            this.gpbMagicianMentorSpirit = new System.Windows.Forms.GroupBox();
            this.tlpMagicianMentorSpirit = new System.Windows.Forms.TableLayoutPanel();
            this.lblMentorSpiritLabel = new System.Windows.Forms.Label();
            this.lblMentorSpirit = new System.Windows.Forms.Label();
            this.lblMentorSpiritInformation = new System.Windows.Forms.Label();
            this.lblMentorSpiritSourceLabel = new System.Windows.Forms.Label();
            this.lblMentorSpiritSource = new System.Windows.Forms.Label();
            this.tlpMagicianButtons = new Chummer.BufferedTableLayoutPanel(this.components);
            this.cmdAddSpell = new SplitButton();
            this.cmdDeleteSpell = new System.Windows.Forms.Button();
            this.tabAdept = new System.Windows.Forms.TabPage();
            this.tabPowerUc = new Chummer.UI.Powers.PowersTabUserControl();
            this.tabTechnomancer = new System.Windows.Forms.TabPage();
            this.tlpTechnomancer = new Chummer.BufferedTableLayoutPanel(this.components);
            this.cmdAddSprite = new System.Windows.Forms.Button();
            this.panSprites = new System.Windows.Forms.Panel();
            this.treComplexForms = new System.Windows.Forms.TreeView();
            this.flpTechnomancer = new System.Windows.Forms.FlowLayoutPanel();
            this.gpbTechnomancerComplexForm = new System.Windows.Forms.GroupBox();
            this.tlpTechnomancerComplexForm = new System.Windows.Forms.TableLayoutPanel();
            this.lblComplexFormDicePool = new System.Windows.Forms.Label();
            this.lblComplexFormDicePoolLabel = new System.Windows.Forms.Label();
            this.lblTargetLabel = new System.Windows.Forms.Label();
            this.lblTarget = new System.Windows.Forms.Label();
            this.lblDurationLabel = new System.Windows.Forms.Label();
            this.lblDuration = new System.Windows.Forms.Label();
            this.lblFVLabel = new System.Windows.Forms.Label();
            this.lblFV = new System.Windows.Forms.Label();
            this.lblComplexFormSourceLabel = new System.Windows.Forms.Label();
            this.lblComplexFormSource = new System.Windows.Forms.Label();
            this.gpbTechnomancerStream = new System.Windows.Forms.GroupBox();
            this.tlpTechnomancerStream = new System.Windows.Forms.TableLayoutPanel();
            this.lblStreamLabel = new System.Windows.Forms.Label();
            this.cboStream = new Chummer.ElasticComboBox();
            this.lblFadingAttributesLabel = new System.Windows.Forms.Label();
            this.flpFadingAttributesValue = new System.Windows.Forms.FlowLayoutPanel();
            this.lblFadingAttributes = new System.Windows.Forms.Label();
            this.lblFadingAttributesValue = new Chummer.LabelWithToolTip();
            this.gpbTechnomancerParagon = new System.Windows.Forms.GroupBox();
            this.tlpTechnomancerParagon = new System.Windows.Forms.TableLayoutPanel();
            this.lblParagonInformation = new System.Windows.Forms.Label();
            this.tlpTechnomancerButtons = new Chummer.BufferedTableLayoutPanel(this.components);
            this.cmdAddComplexForm = new System.Windows.Forms.Button();
            this.cmdDeleteComplexForm = new System.Windows.Forms.Button();
            this.tabAdvancedPrograms = new System.Windows.Forms.TabPage();
            this.tlpAdvancedPrograms = new Chummer.BufferedTableLayoutPanel(this.components);
            this.treAIPrograms = new System.Windows.Forms.TreeView();
            this.lblAIProgramsSource = new System.Windows.Forms.Label();
            this.lblAIProgramsRequires = new System.Windows.Forms.Label();
            this.lblAIProgramsRequiresLabel = new System.Windows.Forms.Label();
            this.lblAIProgramsSourceLabel = new System.Windows.Forms.Label();
            this.tlpAdvancedProgramsButtons = new Chummer.BufferedTableLayoutPanel(this.components);
            this.cmdAddAIProgram = new System.Windows.Forms.Button();
            this.cmdDeleteAIProgram = new System.Windows.Forms.Button();
            this.tabCritter = new System.Windows.Forms.TabPage();
            this.tlpCritter = new Chummer.BufferedTableLayoutPanel(this.components);
            this.treCritterPowers = new System.Windows.Forms.TreeView();
            this.chkCritterPowerCount = new System.Windows.Forms.CheckBox();
            this.lblCritterPowerPointCostLabel = new System.Windows.Forms.Label();
            this.lblCritterPowerPointCost = new System.Windows.Forms.Label();
            this.lblCritterPowerSourceLabel = new System.Windows.Forms.Label();
            this.lblCritterPowerSource = new System.Windows.Forms.Label();
            this.lblCritterPowerDurationLabel = new System.Windows.Forms.Label();
            this.lblCritterPowerDuration = new System.Windows.Forms.Label();
            this.lblCritterPowerRangeLabel = new System.Windows.Forms.Label();
            this.lblCritterPowerRange = new System.Windows.Forms.Label();
            this.lblCritterPowerActionLabel = new System.Windows.Forms.Label();
            this.lblCritterPowerAction = new System.Windows.Forms.Label();
            this.lblCritterPowerTypeLabel = new System.Windows.Forms.Label();
            this.lblCritterPowerType = new System.Windows.Forms.Label();
            this.lblCritterPowerCategoryLabel = new System.Windows.Forms.Label();
            this.lblCritterPowerCategory = new System.Windows.Forms.Label();
            this.lblCritterPowerNameLabel = new System.Windows.Forms.Label();
            this.lblCritterPowerName = new System.Windows.Forms.Label();
            this.lblCritterPowerPointsLabel = new System.Windows.Forms.Label();
            this.lblCritterPowerPoints = new System.Windows.Forms.Label();
            this.tlpCritterButtons = new Chummer.BufferedTableLayoutPanel(this.components);
            this.cmdAddCritterPower = new System.Windows.Forms.Button();
            this.cmdDeleteCritterPower = new System.Windows.Forms.Button();
            this.tabInitiation = new System.Windows.Forms.TabPage();
            this.tlpInitiation = new Chummer.BufferedTableLayoutPanel(this.components);
            this.treMetamagic = new System.Windows.Forms.TreeView();
            this.lblMetamagicSourceLabel = new System.Windows.Forms.Label();
            this.lblMetamagicSource = new System.Windows.Forms.Label();
            this.flpInitiation = new System.Windows.Forms.FlowLayoutPanel();
            this.gpbInitiationType = new System.Windows.Forms.GroupBox();
            this.flpInitiationCheckBoxes = new System.Windows.Forms.FlowLayoutPanel();
            this.chkInitiationOrdeal = new System.Windows.Forms.CheckBox();
            this.chkInitiationSchooling = new System.Windows.Forms.CheckBox();
            this.gpbInitiationGroup = new System.Windows.Forms.GroupBox();
            this.tlpInitiationGroup = new System.Windows.Forms.TableLayoutPanel();
            this.txtGroupNotes = new System.Windows.Forms.TextBox();
            this.lblGroupName = new System.Windows.Forms.Label();
            this.chkInitiationGroup = new System.Windows.Forms.CheckBox();
            this.lblGroupNotes = new System.Windows.Forms.Label();
            this.txtGroupName = new System.Windows.Forms.TextBox();
            this.chkJoinGroup = new System.Windows.Forms.CheckBox();
            this.tlpInitiationButtons = new Chummer.BufferedTableLayoutPanel(this.components);
            this.cmdAddMetamagic = new SplitButton();
            this.cmsMetamagic = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsMetamagicAddArt = new System.Windows.Forms.ToolStripMenuItem();
            this.tsMetamagicAddEnchantment = new System.Windows.Forms.ToolStripMenuItem();
            this.tsMetamagicAddEnhancement = new System.Windows.Forms.ToolStripMenuItem();
            this.tsMetamagicAddMetamagic = new System.Windows.Forms.ToolStripMenuItem();
            this.tsMetamagicAddRitual = new System.Windows.Forms.ToolStripMenuItem();
            this.tsMetamagicNotes = new System.Windows.Forms.ToolStripMenuItem();
            this.cmdDeleteMetamagic = new System.Windows.Forms.Button();
            this.tabCyberware = new System.Windows.Forms.TabPage();
            this.tlpCyberware = new Chummer.BufferedTableLayoutPanel(this.components);
            this.treCyberware = new System.Windows.Forms.TreeView();
            this.flpCyberware = new System.Windows.Forms.FlowLayoutPanel();
            this.gpbEssenceConsumption = new System.Windows.Forms.GroupBox();
            this.tlpCyberwareEssenceConsumption = new System.Windows.Forms.TableLayoutPanel();
            this.lblCyberwareESSLabel = new System.Windows.Forms.Label();
            this.lblBiowareESSLabel = new System.Windows.Forms.Label();
            this.lblEssenceHoleESSLabel = new System.Windows.Forms.Label();
            this.lblPrototypeTranshumanESSLabel = new System.Windows.Forms.Label();
            this.lblCyberwareESS = new System.Windows.Forms.Label();
            this.lblBiowareESS = new System.Windows.Forms.Label();
            this.lblEssenceHoleESS = new System.Windows.Forms.Label();
            this.lblPrototypeTranshumanESS = new System.Windows.Forms.Label();
            this.gpbCyberwareCommon = new System.Windows.Forms.GroupBox();
            this.tlpCyberwareCommon = new System.Windows.Forms.TableLayoutPanel();
            this.lblCyberwareNameLabel = new System.Windows.Forms.Label();
            this.nudCyberwareRating = new System.Windows.Forms.NumericUpDown();
            this.lblCyberwareRatingLabel = new System.Windows.Forms.Label();
            this.lblCyberwareCapacity = new System.Windows.Forms.Label();
            this.lblCyberwareCost = new System.Windows.Forms.Label();
            this.lblCyberwareCostLabel = new System.Windows.Forms.Label();
            this.lblCyberlimbAGI = new System.Windows.Forms.Label();
            this.lblCyberlimbAGILabel = new System.Windows.Forms.Label();
            this.lblCyberlimbSTR = new System.Windows.Forms.Label();
            this.lblCyberlimbSTRLabel = new System.Windows.Forms.Label();
            this.lblCyberwareCapacityLabel = new System.Windows.Forms.Label();
            this.lblCyberwareName = new System.Windows.Forms.Label();
            this.cmdCyberwareChangeMount = new System.Windows.Forms.Button();
            this.lblCyberwareCategoryLabel = new System.Windows.Forms.Label();
            this.lblCyberwareSource = new System.Windows.Forms.Label();
            this.lblCyberwareSourceLabel = new System.Windows.Forms.Label();
            this.lblCyberwareAvail = new System.Windows.Forms.Label();
            this.lblCyberwareAvailLabel = new System.Windows.Forms.Label();
            this.lblCyberwareEssence = new System.Windows.Forms.Label();
            this.lblCyberwareEssenceLabel = new System.Windows.Forms.Label();
            this.cboCyberwareGrade = new Chummer.ElasticComboBox();
            this.lblCyberwareGradeLabel = new System.Windows.Forms.Label();
            this.lblCyberwareCategory = new System.Windows.Forms.Label();
            this.flpCyberwareCommonCheckBoxes = new System.Windows.Forms.FlowLayoutPanel();
            this.chkCyberwareStolen = new System.Windows.Forms.CheckBox();
            this.chkPrototypeTranshuman = new System.Windows.Forms.CheckBox();
            this.gpbCyberwareMatrix = new System.Windows.Forms.GroupBox();
            this.tlpCyberwareMatrix = new Chummer.BufferedTableLayoutPanel(this.components);
            this.lblCyberFirewallLabel = new System.Windows.Forms.Label();
            this.lblCyberSleazeLabel = new System.Windows.Forms.Label();
            this.lblCyberDeviceRatingLabel = new System.Windows.Forms.Label();
            this.lblCyberDeviceRating = new System.Windows.Forms.Label();
            this.lblCyberAttackLabel = new System.Windows.Forms.Label();
            this.lblCyberDataProcessingLabel = new System.Windows.Forms.Label();
            this.cboCyberwareAttack = new System.Windows.Forms.ComboBox();
            this.cboCyberwareSleaze = new System.Windows.Forms.ComboBox();
            this.cboCyberwareDataProcessing = new System.Windows.Forms.ComboBox();
            this.cboCyberwareFirewall = new System.Windows.Forms.ComboBox();
            this.flpCyberwareMatrixCheckBoxes = new System.Windows.Forms.FlowLayoutPanel();
            this.chkCyberwareHomeNode = new System.Windows.Forms.CheckBox();
            this.chkCyberwareActiveCommlink = new System.Windows.Forms.CheckBox();
            this.tlpCyberwareButtons = new Chummer.BufferedTableLayoutPanel(this.components);
            this.cmdAddCyberware = new SplitButton();
            this.cmdDeleteCyberware = new System.Windows.Forms.Button();
            this.cmdAddBioware = new System.Windows.Forms.Button();
            this.tabStreetGear = new System.Windows.Forms.TabPage();
            this.tabStreetGearTabs = new System.Windows.Forms.TabControl();
            this.tabGear = new System.Windows.Forms.TabPage();
            this.tlpGear = new Chummer.BufferedTableLayoutPanel(this.components);
            this.treGear = new System.Windows.Forms.TreeView();
            this.flpGear = new System.Windows.Forms.FlowLayoutPanel();
            this.gpbGearCommon = new System.Windows.Forms.GroupBox();
            this.tlpGearCommon = new System.Windows.Forms.TableLayoutPanel();
            this.lblGearNameLabel = new System.Windows.Forms.Label();
            this.lblGearName = new System.Windows.Forms.Label();
            this.lblGearCategoryLabel = new System.Windows.Forms.Label();
            this.lblGearCategory = new System.Windows.Forms.Label();
            this.lblGearAvailLabel = new System.Windows.Forms.Label();
            this.lblGearAvail = new System.Windows.Forms.Label();
            this.lblGearRatingLabel = new System.Windows.Forms.Label();
            this.lblGearQtyLabel = new System.Windows.Forms.Label();
            this.nudGearRating = new System.Windows.Forms.NumericUpDown();
            this.nudGearQty = new System.Windows.Forms.NumericUpDown();
            this.lblGearCostLabel = new System.Windows.Forms.Label();
            this.lblGearCost = new System.Windows.Forms.Label();
            this.lblGearCapacityLabel = new System.Windows.Forms.Label();
            this.lblGearCapacity = new System.Windows.Forms.Label();
            this.lblGearSourceLabel = new System.Windows.Forms.Label();
            this.lblGearSource = new System.Windows.Forms.Label();
            this.flpGearCommonCheckBoxes = new System.Windows.Forms.FlowLayoutPanel();
            this.chkGearEquipped = new System.Windows.Forms.CheckBox();
            this.chkGearStolen = new System.Windows.Forms.CheckBox();
            this.gpbGearMatrix = new System.Windows.Forms.GroupBox();
            this.tlpGearMatrix = new System.Windows.Forms.TableLayoutPanel();
            this.lblGearDeviceRatingLabel = new System.Windows.Forms.Label();
            this.lblGearDeviceRating = new System.Windows.Forms.Label();
            this.lblGearAttackLabel = new System.Windows.Forms.Label();
            this.lblGearSleazeLabel = new System.Windows.Forms.Label();
            this.cboGearFirewall = new Chummer.ElasticComboBox();
            this.lblGearFirewallLabel = new System.Windows.Forms.Label();
            this.cboGearDataProcessing = new Chummer.ElasticComboBox();
            this.lblGearDataProcessingLabel = new System.Windows.Forms.Label();
            this.cboGearAttack = new Chummer.ElasticComboBox();
            this.cboGearSleaze = new Chummer.ElasticComboBox();
            this.flpGearMatrixCheckBoxes = new System.Windows.Forms.FlowLayoutPanel();
            this.chkGearHomeNode = new System.Windows.Forms.CheckBox();
            this.chkGearActiveCommlink = new System.Windows.Forms.CheckBox();
            this.gpbGearBondedFoci = new System.Windows.Forms.GroupBox();
            this.tlpGearBondedFoci = new System.Windows.Forms.TableLayoutPanel();
            this.treFoci = new System.Windows.Forms.TreeView();
            this.cmdCreateStackedFocus = new System.Windows.Forms.Button();
            this.tlpGearButtons = new Chummer.BufferedTableLayoutPanel(this.components);
            this.cmdAddGear = new SplitButton();
            this.chkCommlinks = new System.Windows.Forms.CheckBox();
            this.cmdDeleteGear = new System.Windows.Forms.Button();
            this.cmdAddLocation = new System.Windows.Forms.Button();
            this.tabArmor = new System.Windows.Forms.TabPage();
            this.tlpArmor = new Chummer.BufferedTableLayoutPanel(this.components);
            this.treArmor = new System.Windows.Forms.TreeView();
            this.flpArmor = new System.Windows.Forms.FlowLayoutPanel();
            this.gpbArmorCommon = new System.Windows.Forms.GroupBox();
            this.tlpArmorCommon = new System.Windows.Forms.TableLayoutPanel();
            this.lblArmorValueLabel = new System.Windows.Forms.Label();
            this.lblArmorValue = new System.Windows.Forms.Label();
            this.lblArmorRatingLabel = new System.Windows.Forms.Label();
            this.nudArmorRating = new System.Windows.Forms.NumericUpDown();
            this.lblArmorAvailLabel = new System.Windows.Forms.Label();
            this.lblArmorAvail = new System.Windows.Forms.Label();
            this.lblArmorCostLabel = new System.Windows.Forms.Label();
            this.lblArmorCost = new System.Windows.Forms.Label();
            this.lblArmorCapacityLabel = new System.Windows.Forms.Label();
            this.lblArmorSource = new System.Windows.Forms.Label();
            this.lblArmorCapacity = new System.Windows.Forms.Label();
            this.lblArmorSourceLabel = new System.Windows.Forms.Label();
            this.flpArmorCommonCheckBoxes = new System.Windows.Forms.FlowLayoutPanel();
            this.chkIncludedInArmor = new System.Windows.Forms.CheckBox();
            this.chkArmorStolen = new System.Windows.Forms.CheckBox();
            this.gpbArmorMatrix = new System.Windows.Forms.GroupBox();
            this.tlpArmorMatrix = new System.Windows.Forms.TableLayoutPanel();
            this.lblArmorAttack = new System.Windows.Forms.Label();
            this.lblArmorAttackLabel = new System.Windows.Forms.Label();
            this.lblArmorFirewall = new System.Windows.Forms.Label();
            this.lblArmorDeviceRating = new System.Windows.Forms.Label();
            this.lblArmorFirewallLabel = new System.Windows.Forms.Label();
            this.lblArmorDeviceRatingLabel = new System.Windows.Forms.Label();
            this.lblArmorSleaze = new System.Windows.Forms.Label();
            this.lblArmorSleazeLabel = new System.Windows.Forms.Label();
            this.lblArmorDataProcessing = new System.Windows.Forms.Label();
            this.lblArmorDataProcessingLabel = new System.Windows.Forms.Label();
            this.gpbArmorLocation = new System.Windows.Forms.GroupBox();
            this.tlpArmorLocation = new System.Windows.Forms.TableLayoutPanel();
            this.lblArmorEquippedLabel = new System.Windows.Forms.Label();
            this.lblArmorEquipped = new System.Windows.Forms.Label();
            this.flpArmorLocationButtons = new System.Windows.Forms.FlowLayoutPanel();
            this.cmdArmorEquipAll = new System.Windows.Forms.Button();
            this.cmdArmorUnEquipAll = new System.Windows.Forms.Button();
            this.tlpArmorButtons = new Chummer.BufferedTableLayoutPanel(this.components);
            this.cmdAddArmor = new SplitButton();
            this.cmdAddArmorBundle = new System.Windows.Forms.Button();
            this.cmdDeleteArmor = new System.Windows.Forms.Button();
            this.tabWeapons = new System.Windows.Forms.TabPage();
            this.tlpWeapons = new Chummer.BufferedTableLayoutPanel(this.components);
            this.treWeapons = new System.Windows.Forms.TreeView();
            this.flpWeapons = new System.Windows.Forms.FlowLayoutPanel();
            this.gpbWeaponsCommon = new System.Windows.Forms.GroupBox();
            this.tlpWeaponsCommon = new System.Windows.Forms.TableLayoutPanel();
            this.lblWeaponCapacity = new System.Windows.Forms.Label();
            this.lblWeaponNameLabel = new System.Windows.Forms.Label();
            this.lblWeaponName = new System.Windows.Forms.Label();
            this.lblWeaponCapacityLabel = new System.Windows.Forms.Label();
            this.lblWeaponConceal = new System.Windows.Forms.Label();
            this.lblWeaponCategoryLabel = new System.Windows.Forms.Label();
            this.lblWeaponConcealLabel = new System.Windows.Forms.Label();
            this.lblWeaponSourceLabel = new System.Windows.Forms.Label();
            this.lblWeaponSource = new System.Windows.Forms.Label();
            this.lblWeaponCategory = new System.Windows.Forms.Label();
            this.lblWeaponAvailLabel = new System.Windows.Forms.Label();
            this.lblWeaponAvail = new System.Windows.Forms.Label();
            this.lblWeaponSlots = new System.Windows.Forms.Label();
            this.lblWeaponSlotsLabel = new System.Windows.Forms.Label();
            this.lblWeaponRating = new System.Windows.Forms.Label();
            this.lblWeaponCostLabel = new System.Windows.Forms.Label();
            this.lblWeaponRatingLabel = new System.Windows.Forms.Label();
            this.lblWeaponCost = new System.Windows.Forms.Label();
            this.flpWeaponsCommonCheckBoxes = new System.Windows.Forms.FlowLayoutPanel();
            this.chkIncludedInWeapon = new System.Windows.Forms.CheckBox();
            this.chkWeaponStolen = new System.Windows.Forms.CheckBox();
            this.gpbWeaponsWeapon = new System.Windows.Forms.GroupBox();
            this.flpWeaponsWeapon = new System.Windows.Forms.FlowLayoutPanel();
            this.tlpWeaponsWeapon = new System.Windows.Forms.TableLayoutPanel();
            this.lblWeaponDamageLabel = new System.Windows.Forms.Label();
            this.lblWeaponDamage = new System.Windows.Forms.Label();
            this.lblWeaponAmmo = new System.Windows.Forms.Label();
            this.lblWeaponAmmoLabel = new System.Windows.Forms.Label();
            this.lblWeaponReach = new System.Windows.Forms.Label();
            this.lblWeaponReachLabel = new System.Windows.Forms.Label();
            this.lblWeaponRC = new System.Windows.Forms.Label();
            this.lblWeaponRCLabel = new System.Windows.Forms.Label();
            this.lblWeaponDicePool = new System.Windows.Forms.Label();
            this.lblWeaponAccuracy = new System.Windows.Forms.Label();
            this.lblWeaponMode = new System.Windows.Forms.Label();
            this.lblWeaponModeLabel = new System.Windows.Forms.Label();
            this.lblWeaponDicePoolLabel = new System.Windows.Forms.Label();
            this.lblWeaponAccuracyLabel = new System.Windows.Forms.Label();
            this.lblWeaponAPLabel = new System.Windows.Forms.Label();
            this.lblWeaponAP = new System.Windows.Forms.Label();
            this.tlpWeaponsRanges = new Chummer.BufferedTableLayoutPanel(this.components);
            this.lblWeaponAlternateRangeExtreme = new System.Windows.Forms.Label();
            this.lblWeaponRangeAlternate = new System.Windows.Forms.Label();
            this.lblWeaponAlternateRangeLong = new System.Windows.Forms.Label();
            this.lblWeaponRangeLabel = new System.Windows.Forms.Label();
            this.lblWeaponAlternateRangeMedium = new System.Windows.Forms.Label();
            this.lblWeaponRangeMain = new System.Windows.Forms.Label();
            this.lblWeaponAlternateRangeShort = new System.Windows.Forms.Label();
            this.lblWeaponRangeShortLabel = new System.Windows.Forms.Label();
            this.lblWeaponRangeShort = new System.Windows.Forms.Label();
            this.lblWeaponRangeMediumLabel = new System.Windows.Forms.Label();
            this.lblWeaponRangeLongLabel = new System.Windows.Forms.Label();
            this.lblWeaponRangeExtremeLabel = new System.Windows.Forms.Label();
            this.lblWeaponRangeMedium = new System.Windows.Forms.Label();
            this.lblWeaponRangeLong = new System.Windows.Forms.Label();
            this.lblWeaponRangeExtreme = new System.Windows.Forms.Label();
            this.gpbWeaponsMatrix = new System.Windows.Forms.GroupBox();
            this.tlpWeaponsMatrix = new System.Windows.Forms.TableLayoutPanel();
            this.lblWeaponDeviceRating = new System.Windows.Forms.Label();
            this.lblWeaponAttack = new System.Windows.Forms.Label();
            this.lblWeaponSleaze = new System.Windows.Forms.Label();
            this.lblWeaponDataProcessing = new System.Windows.Forms.Label();
            this.lblWeaponFirewall = new System.Windows.Forms.Label();
            this.lblWeaponDeviceRatingLabel = new System.Windows.Forms.Label();
            this.lblWeaponAttackLabel = new System.Windows.Forms.Label();
            this.lblWeaponFirewallLabel = new System.Windows.Forms.Label();
            this.lblWeaponSleazeLabel = new System.Windows.Forms.Label();
            this.lblWeaponDataProcessingLabel = new System.Windows.Forms.Label();
            this.tlpWeaponsButtons = new Chummer.BufferedTableLayoutPanel(this.components);
            this.cmdAddWeapon = new SplitButton();
            this.cmdAddWeaponLocation = new System.Windows.Forms.Button();
            this.cmdDeleteWeapon = new System.Windows.Forms.Button();
            this.tabDrugs = new System.Windows.Forms.TabPage();
            this.tlpDrugInfo = new Chummer.BufferedTableLayoutPanel(this.components);
            this.treCustomDrugs = new System.Windows.Forms.TreeView();
            this.flpDrugs = new System.Windows.Forms.FlowLayoutPanel();
            this.gpbDrugsCommon = new System.Windows.Forms.GroupBox();
            this.tlpDrugsCommon = new System.Windows.Forms.TableLayoutPanel();
            this.chkDrugStolen = new System.Windows.Forms.CheckBox();
            this.lblDrugNameLabel = new System.Windows.Forms.Label();
            this.lblDrugEffectLabel = new System.Windows.Forms.Label();
            this.lblDrugName = new System.Windows.Forms.Label();
            this.lblDrugComponentsLabel = new System.Windows.Forms.Label();
            this.lblDrugCategoryLabel = new System.Windows.Forms.Label();
            this.lblDrugAddictionThresholdLabel = new System.Windows.Forms.Label();
            this.lblDrugAddictionRatingLabel = new System.Windows.Forms.Label();
            this.lblDrugCategory = new System.Windows.Forms.Label();
            this.lblDrugQtyLabel = new System.Windows.Forms.Label();
            this.nudDrugQty = new System.Windows.Forms.NumericUpDown();
            this.lblDrugGrade = new System.Windows.Forms.Label();
            this.lblDrugCostLabel = new System.Windows.Forms.Label();
            this.lblDrugGradeLabel = new System.Windows.Forms.Label();
            this.lblDrugAvailabel = new System.Windows.Forms.Label();
            this.lblDrugAvail = new System.Windows.Forms.Label();
            this.lblDrugCost = new System.Windows.Forms.Label();
            this.lblDrugAddictionThreshold = new System.Windows.Forms.Label();
            this.lblDrugAddictionRating = new System.Windows.Forms.Label();
            this.lblDrugComponents = new System.Windows.Forms.Label();
            this.lblDrugEffect = new System.Windows.Forms.Label();
            this.tlpDrugButtons = new Chummer.BufferedTableLayoutPanel(this.components);
            this.btnCreateCustomDrug = new System.Windows.Forms.Button();
            this.btnDeleteCustomDrug = new System.Windows.Forms.Button();
            this.tabLifestyle = new System.Windows.Forms.TabPage();
            this.tlpLifestyleDetails = new Chummer.BufferedTableLayoutPanel(this.components);
            this.treLifestyles = new System.Windows.Forms.TreeView();
            this.flpLifestyleDetails = new System.Windows.Forms.FlowLayoutPanel();
            this.gpbLifestyleCommon = new System.Windows.Forms.GroupBox();
            this.tlpLifestyleCommon = new System.Windows.Forms.TableLayoutPanel();
            this.lblLifestyleCostLabel = new System.Windows.Forms.Label();
            this.lblLifestyleQualities = new System.Windows.Forms.Label();
            this.lblLifestyleStartingNuyen = new System.Windows.Forms.Label();
            this.flpLifestyleIntervals = new System.Windows.Forms.FlowLayoutPanel();
            this.nudLifestyleMonths = new System.Windows.Forms.NumericUpDown();
            this.lblLifestyleMonthsLabel = new System.Windows.Forms.Label();
            this.lblLifestyleTotalCost = new System.Windows.Forms.Label();
            this.lblLifestyleCost = new System.Windows.Forms.Label();
            this.lblBaseLifestyle = new System.Windows.Forms.Label();
            this.lblLifestyleSourceLabel = new System.Windows.Forms.Label();
            this.lblLifestyleSource = new System.Windows.Forms.Label();
            this.lblLifestyleComfortsLabel = new System.Windows.Forms.Label();
            this.lblLifestyleQualitiesLabel = new System.Windows.Forms.Label();
            this.lblLifestyleStartingNuyenLabel = new System.Windows.Forms.Label();
            this.tlpLifestyleButtons = new Chummer.BufferedTableLayoutPanel(this.components);
            this.cmdAddLifestyle = new SplitButton();
            this.cmdDeleteLifestyle = new System.Windows.Forms.Button();
            this.tabVehicles = new System.Windows.Forms.TabPage();
            this.tlpVehicles = new Chummer.BufferedTableLayoutPanel(this.components);
            this.flpVehiclesButtons = new System.Windows.Forms.FlowLayoutPanel();
            this.cmdAddVehicle = new SplitButton();
            this.cmdDeleteVehicle = new System.Windows.Forms.Button();
            this.cmdAddVehicleLocation = new System.Windows.Forms.Button();
            this.flpVehicles = new System.Windows.Forms.FlowLayoutPanel();
            this.gpbVehiclesCommon = new System.Windows.Forms.GroupBox();
            this.tlpVehiclesCommon = new System.Windows.Forms.TableLayoutPanel();
            this.lblVehicleNameLabel = new System.Windows.Forms.Label();
            this.lblVehicleAvailLabel = new System.Windows.Forms.Label();
            this.lblVehicleCostLabel = new System.Windows.Forms.Label();
            this.lblVehicleAvail = new System.Windows.Forms.Label();
            this.lblVehicleSlots = new System.Windows.Forms.Label();
            this.lblVehicleCost = new System.Windows.Forms.Label();
            this.lblVehicleSource = new System.Windows.Forms.Label();
            this.lblVehicleSlotsLabel = new System.Windows.Forms.Label();
            this.lblVehicleName = new System.Windows.Forms.Label();
            this.lblVehicleSourceLabel = new System.Windows.Forms.Label();
            this.lblVehicleCategory = new System.Windows.Forms.Label();
            this.nudVehicleGearQty = new System.Windows.Forms.NumericUpDown();
            this.lblVehicleCategoryLabel = new System.Windows.Forms.Label();
            this.lblVehicleRatingLabel = new System.Windows.Forms.Label();
            this.lblVehicleGearQtyLabel = new System.Windows.Forms.Label();
            this.nudVehicleRating = new System.Windows.Forms.NumericUpDown();
            this.cmdVehicleCyberwareChangeMount = new System.Windows.Forms.Button();
            this.flpVehiclesCommonCheckBoxes = new System.Windows.Forms.FlowLayoutPanel();
            this.chkVehicleWeaponAccessoryInstalled = new System.Windows.Forms.CheckBox();
            this.chkVehicleIncludedInWeapon = new System.Windows.Forms.CheckBox();
            this.chkVehicleStolen = new System.Windows.Forms.CheckBox();
            this.gpbVehiclesVehicle = new System.Windows.Forms.GroupBox();
            this.tlpVehiclesVehicle = new System.Windows.Forms.TableLayoutPanel();
            this.lblVehicleHandlingLabel = new System.Windows.Forms.Label();
            this.lblVehicleHandling = new System.Windows.Forms.Label();
            this.lblVehicleAccelLabel = new System.Windows.Forms.Label();
            this.lblVehicleAccel = new System.Windows.Forms.Label();
            this.lblVehicleSpeedLabel = new System.Windows.Forms.Label();
            this.lblVehicleSpeed = new System.Windows.Forms.Label();
            this.lblVehicleCosmetic = new System.Windows.Forms.Label();
            this.lblVehicleCosmeticLabel = new System.Windows.Forms.Label();
            this.lblVehiclePilotLabel = new System.Windows.Forms.Label();
            this.lblVehiclePilot = new System.Windows.Forms.Label();
            this.lblVehicleDroneModSlots = new System.Windows.Forms.Label();
            this.lblVehicleBodyLabel = new System.Windows.Forms.Label();
            this.lblVehicleBody = new System.Windows.Forms.Label();
            this.lblVehicleWeaponsmodLabel = new System.Windows.Forms.Label();
            this.lblVehicleDroneModSlotsLabel = new System.Windows.Forms.Label();
            this.lblVehicleSeats = new System.Windows.Forms.Label();
            this.lblVehicleBodymodLabel = new System.Windows.Forms.Label();
            this.lblVehicleArmorLabel = new System.Windows.Forms.Label();
            this.lblVehicleArmor = new System.Windows.Forms.Label();
            this.lblVehicleSeatsLabel = new System.Windows.Forms.Label();
            this.lblVehicleSensorLabel = new System.Windows.Forms.Label();
            this.lblVehicleSensor = new System.Windows.Forms.Label();
            this.lblVehiclePowertrainLabel = new System.Windows.Forms.Label();
            this.lblVehiclePowertrain = new System.Windows.Forms.Label();
            this.lblVehicleWeaponsmod = new System.Windows.Forms.Label();
            this.lblVehicleElectromagnetic = new System.Windows.Forms.Label();
            this.lblVehicleElectromagneticLabel = new System.Windows.Forms.Label();
            this.lblVehicleBodymod = new System.Windows.Forms.Label();
            this.lblVehicleProtectionLabel = new System.Windows.Forms.Label();
            this.lblVehicleProtection = new System.Windows.Forms.Label();
            this.gpbVehiclesWeapon = new System.Windows.Forms.GroupBox();
            this.flpVehiclesWeapon = new System.Windows.Forms.FlowLayoutPanel();
            this.tlpVehiclesWeaponCommon = new System.Windows.Forms.TableLayoutPanel();
            this.lblVehicleWeaponDamageLabel = new System.Windows.Forms.Label();
            this.lblVehicleWeaponDamage = new System.Windows.Forms.Label();
            this.lblVehicleWeaponAPLabel = new System.Windows.Forms.Label();
            this.lblVehicleWeaponAP = new System.Windows.Forms.Label();
            this.lblVehicleWeaponAccuracyLabel = new System.Windows.Forms.Label();
            this.lblVehicleWeaponAccuracy = new System.Windows.Forms.Label();
            this.lblVehicleWeaponModeLabel = new System.Windows.Forms.Label();
            this.lblVehicleWeaponDicePoolLabel = new System.Windows.Forms.Label();
            this.lblVehicleWeaponDicePool = new System.Windows.Forms.Label();
            this.lblVehicleWeaponAmmoLabel = new System.Windows.Forms.Label();
            this.lblVehicleWeaponAmmo = new System.Windows.Forms.Label();
            this.cboVehicleWeaponFiringMode = new Chummer.ElasticComboBox();
            this.lblFiringModeLabel = new System.Windows.Forms.Label();
            this.lblVehicleWeaponMode = new System.Windows.Forms.Label();
            this.tlpVehiclesWeaponRanges = new Chummer.BufferedTableLayoutPanel(this.components);
            this.lblVehicleWeaponAlternateRangeExtreme = new System.Windows.Forms.Label();
            this.lblVehicleWeaponRangeExtreme = new System.Windows.Forms.Label();
            this.lblVehicleWeaponAlternateRangeLong = new System.Windows.Forms.Label();
            this.lblVehicleWeaponRangeExtremeLabel = new System.Windows.Forms.Label();
            this.lblVehicleWeaponRangeLabel = new System.Windows.Forms.Label();
            this.lblVehicleWeaponAlternateRangeMedium = new System.Windows.Forms.Label();
            this.lblVehicleWeaponRangeLong = new System.Windows.Forms.Label();
            this.lblVehicleWeaponAlternateRangeShort = new System.Windows.Forms.Label();
            this.lblVehicleWeaponRangeLongLabel = new System.Windows.Forms.Label();
            this.lblVehicleWeaponRangeShortLabel = new System.Windows.Forms.Label();
            this.lblVehicleWeaponRangeMedium = new System.Windows.Forms.Label();
            this.lblVehicleWeaponRangeAlternate = new System.Windows.Forms.Label();
            this.lblVehicleWeaponRangeMain = new System.Windows.Forms.Label();
            this.lblVehicleWeaponRangeMediumLabel = new System.Windows.Forms.Label();
            this.lblVehicleWeaponRangeShort = new System.Windows.Forms.Label();
            this.gpbVehiclesMatrix = new System.Windows.Forms.GroupBox();
            this.tlpVehiclesMatrix = new System.Windows.Forms.TableLayoutPanel();
            this.lblVehicleDeviceLabel = new System.Windows.Forms.Label();
            this.lblVehicleDevice = new System.Windows.Forms.Label();
            this.cboVehicleAttack = new Chummer.ElasticComboBox();
            this.lblVehicleAttackLabel = new System.Windows.Forms.Label();
            this.lblVehicleSleazeLabel = new System.Windows.Forms.Label();
            this.cboVehicleSleaze = new Chummer.ElasticComboBox();
            this.lblVehicleDataProcessingLabel = new System.Windows.Forms.Label();
            this.cboVehicleDataProcessing = new Chummer.ElasticComboBox();
            this.cboVehicleFirewall = new Chummer.ElasticComboBox();
            this.lblVehicleFirewallLabel = new System.Windows.Forms.Label();
            this.flpVehiclesMatrixCheckBoxes = new System.Windows.Forms.FlowLayoutPanel();
            this.chkVehicleHomeNode = new System.Windows.Forms.CheckBox();
            this.chkVehicleActiveCommlink = new System.Windows.Forms.CheckBox();
            this.treVehicles = new System.Windows.Forms.TreeView();
            this.tabCharacterInfo = new System.Windows.Forms.TabPage();
            this.tlpCharacterInfo = new Chummer.BufferedTableLayoutPanel(this.components);
            this.tlpLongTexts = new System.Windows.Forms.TableLayoutPanel();
            this.gpbDescription = new System.Windows.Forms.GroupBox();
            this.rtfDescription = new Chummer.UI.Editors.RtfEditor();
            this.gpbBackground = new System.Windows.Forms.GroupBox();
            this.rtfBackground = new Chummer.UI.Editors.RtfEditor();
            this.gpbConcept = new System.Windows.Forms.GroupBox();
            this.rtfConcept = new Chummer.UI.Editors.RtfEditor();
            this.gpbNotes = new System.Windows.Forms.GroupBox();
            this.rtfNotes = new Chummer.UI.Editors.RtfEditor();
            this.picMugshot = new System.Windows.Forms.PictureBox();
            this.chkIsMainMugshot = new System.Windows.Forms.CheckBox();
            this.cboPrimaryArm = new Chummer.ElasticComboBox();
            this.lblNumMugshots = new System.Windows.Forms.Label();
            this.lblSex = new System.Windows.Forms.Label();
            this.nudMugshotIndex = new System.Windows.Forms.NumericUpDown();
            this.lblHandedness = new System.Windows.Forms.Label();
            this.btnCreateBackstory = new System.Windows.Forms.Button();
            this.txtSex = new System.Windows.Forms.TextBox();
            this.lblPublicAwareTotal = new Chummer.LabelWithToolTip();
            this.lblAge = new System.Windows.Forms.Label();
            this.lblNotorietyTotal = new Chummer.LabelWithToolTip();
            this.txtAge = new System.Windows.Forms.TextBox();
            this.lblStreetCredTotal = new Chummer.LabelWithToolTip();
            this.lblEyes = new System.Windows.Forms.Label();
            this.txtEyes = new System.Windows.Forms.TextBox();
            this.lblHair = new System.Windows.Forms.Label();
            this.txtHair = new System.Windows.Forms.TextBox();
            this.lblHeight = new System.Windows.Forms.Label();
            this.txtHeight = new System.Windows.Forms.TextBox();
            this.lblWeight = new System.Windows.Forms.Label();
            this.txtPlayerName = new System.Windows.Forms.TextBox();
            this.txtCharacterName = new System.Windows.Forms.TextBox();
            this.lblPlayerName = new System.Windows.Forms.Label();
            this.lblCharacterName = new System.Windows.Forms.Label();
            this.txtWeight = new System.Windows.Forms.TextBox();
            this.lblSkin = new System.Windows.Forms.Label();
            this.txtSkin = new System.Windows.Forms.TextBox();
            this.lblMugshot = new System.Windows.Forms.Label();
            this.lblAstralReputation = new System.Windows.Forms.Label();
            this.lblWildReputation = new System.Windows.Forms.Label();
            this.lblAstralReputationTotal = new Chummer.LabelWithToolTip();
            this.lblWildReputationTotal = new Chummer.LabelWithToolTip();
            this.tlpMugshotButtons = new Chummer.BufferedTableLayoutPanel(this.components);
            this.cmdAddMugshot = new System.Windows.Forms.Button();
            this.cmdDeleteMugshot = new System.Windows.Forms.Button();
            this.tabRelationships = new System.Windows.Forms.TabPage();
            this.tabPeople = new System.Windows.Forms.TabControl();
            this.tabContacts = new System.Windows.Forms.TabPage();
            this.tlpContacts = new System.Windows.Forms.TableLayoutPanel();
            this.panContacts = new System.Windows.Forms.FlowLayoutPanel();
            this.lblContactArchtypeLabel = new System.Windows.Forms.Label();
            this.lblContactLocationLabel = new System.Windows.Forms.Label();
            this.lblContactNameLabel = new System.Windows.Forms.Label();
            this.tlpContactsButtons = new Chummer.BufferedTableLayoutPanel(this.components);
            this.cmdAddContact = new SplitButton();
            this.cmsAddContact = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsAddFromFile = new System.Windows.Forms.ToolStripMenuItem();
            this.lblContactPoints = new System.Windows.Forms.Label();
            this.cmdContactsExpansionToggle = new System.Windows.Forms.Button();
            this.lblContactPoints_Label = new System.Windows.Forms.Label();
            this.cmdSwapContactOrder = new System.Windows.Forms.Button();
            this.tabEnemies = new System.Windows.Forms.TabPage();
            this.tlpEnemies = new System.Windows.Forms.TableLayoutPanel();
            this.panEnemies = new System.Windows.Forms.FlowLayoutPanel();
            this.flpEnemiesButtons = new System.Windows.Forms.FlowLayoutPanel();
            this.cmdAddEnemy = new SplitButton();
            this.lblEnemyArchetypeLabel = new System.Windows.Forms.Label();
            this.lblEnemyLocationLabel = new System.Windows.Forms.Label();
            this.lblEnemyNameLabel = new System.Windows.Forms.Label();
            this.tabPets = new System.Windows.Forms.TabPage();
            this.tlpPets = new System.Windows.Forms.TableLayoutPanel();
            this.panPets = new System.Windows.Forms.FlowLayoutPanel();
            this.flpPetsButtons = new System.Windows.Forms.FlowLayoutPanel();
            this.cmdAddPet = new SplitButton();
            this.tabInfo = new System.Windows.Forms.TabControl();
            this.tabBPSummary = new System.Windows.Forms.TabPage();
            this.tlpKarmaSummary = new Chummer.BufferedTableLayoutPanel(this.components);
            this.lblMetagenicQualities = new System.Windows.Forms.Label();
            this.lblMetagenicQualitiesLabel = new System.Windows.Forms.Label();
            this.lblAINormalProgramsBP = new System.Windows.Forms.Label();
            this.lblAIAdvancedProgramsBP = new System.Windows.Forms.Label();
            this.lblBuildRitualsBP = new System.Windows.Forms.Label();
            this.lblBuildAINormalPrograms = new System.Windows.Forms.Label();
            this.lblPBuildSpecial = new System.Windows.Forms.Label();
            this.lblSummaryMetatype = new System.Windows.Forms.Label();
            this.lblInitiationBP = new System.Windows.Forms.Label();
            this.lblMartialArtsBP = new System.Windows.Forms.Label();
            this.lblBuildPrepsBP = new System.Windows.Forms.Label();
            this.lblBuildInitiation = new System.Windows.Forms.Label();
            this.lblFociBP = new System.Windows.Forms.Label();
            this.lblComplexFormsBP = new System.Windows.Forms.Label();
            this.lblPBuildSpecialLabel = new System.Windows.Forms.Label();
            this.lblKarmaMetatypeBP = new System.Windows.Forms.Label();
            this.lblSpritesBP = new System.Windows.Forms.Label();
            this.lblSpiritsBP = new System.Windows.Forms.Label();
            this.lblAttributesBP = new System.Windows.Forms.Label();
            this.lblPositiveQualitiesBP = new System.Windows.Forms.Label();
            this.lblNegativeQualitiesBP = new System.Windows.Forms.Label();
            this.lblContactsBP = new System.Windows.Forms.Label();
            this.lblEnemiesBP = new System.Windows.Forms.Label();
            this.lblNuyenBP = new System.Windows.Forms.Label();
            this.lblSkillGroupsBP = new System.Windows.Forms.Label();
            this.lblActiveSkillsBP = new System.Windows.Forms.Label();
            this.lblSpellsBP = new System.Windows.Forms.Label();
            this.lblKnowledgeSkillsBP = new System.Windows.Forms.Label();
            this.tabOtherInfo = new System.Windows.Forms.TabPage();
            this.tlpOtherInfo = new Chummer.BufferedTableLayoutPanel(this.components);
            this.lblFly = new System.Windows.Forms.Label();
            this.lblRiggingINI = new Chummer.LabelWithToolTip();
            this.lblFlyLabel = new System.Windows.Forms.Label();
            this.lblSwim = new System.Windows.Forms.Label();
            this.lblSwimLabel = new System.Windows.Forms.Label();
            this.lblCMPhysical = new Chummer.LabelWithToolTip();
            this.lblMovement = new System.Windows.Forms.Label();
            this.lblMemory = new Chummer.LabelWithToolTip();
            this.lblMovementLabel = new System.Windows.Forms.Label();
            this.lblMatrixINIHot = new Chummer.LabelWithToolTip();
            this.lblLiftCarry = new Chummer.LabelWithToolTip();
            this.lblMatrixINICold = new Chummer.LabelWithToolTip();
            this.lblJudgeIntentions = new Chummer.LabelWithToolTip();
            this.lblCMStun = new Chummer.LabelWithToolTip();
            this.lblComposure = new Chummer.LabelWithToolTip();
            this.lblINI = new Chummer.LabelWithToolTip();
            this.lblRemainingNuyen = new System.Windows.Forms.Label();
            this.lblAstralINI = new Chummer.LabelWithToolTip();
            this.lblMatrixINI = new Chummer.LabelWithToolTip();
            this.lblESSMax = new System.Windows.Forms.Label();
            this.lblArmor = new Chummer.LabelWithToolTip();
            this.lblSurprise = new Chummer.LabelWithToolTip();
            this.lblSurpriseLabel = new Chummer.LabelWithToolTip();
            this.lblDodgeLabel = new Chummer.LabelWithToolTip();
            this.lblDodge = new Chummer.LabelWithToolTip();
            this.tabDefenses = new System.Windows.Forms.TabPage();
            this.tlpSpellDefense = new Chummer.BufferedTableLayoutPanel(this.components);
            this.lblSpellDefenseManipPhysical = new Chummer.LabelWithToolTip();
            this.nudCounterspellingDice = new System.Windows.Forms.NumericUpDown();
            this.lblSpellDefenseManipMental = new Chummer.LabelWithToolTip();
            this.lblSpellDefenseIllusionPhysical = new Chummer.LabelWithToolTip();
            this.lblSpellDefenseIndirectDodge = new Chummer.LabelWithToolTip();
            this.lblSpellDefenseIllusionMana = new Chummer.LabelWithToolTip();
            this.lblSpellDefenseIndirectSoak = new Chummer.LabelWithToolTip();
            this.lblSpellDefenseDecAttWIL = new Chummer.LabelWithToolTip();
            this.lblSpellDefenseDecAttLOG = new Chummer.LabelWithToolTip();
            this.lblSpellDefenseDirectSoakMana = new Chummer.LabelWithToolTip();
            this.lblSpellDefenseDecAttINT = new Chummer.LabelWithToolTip();
            this.lblSpellDefenseDirectSoakPhysical = new Chummer.LabelWithToolTip();
            this.lblSpellDefenseDecAttCHA = new Chummer.LabelWithToolTip();
            this.lblSpellDefenseDetection = new Chummer.LabelWithToolTip();
            this.lblSpellDefenseDecAttSTR = new Chummer.LabelWithToolTip();
            this.lblSpellDefenseDecAttBOD = new Chummer.LabelWithToolTip();
            this.lblSpellDefenseDecAttAGI = new Chummer.LabelWithToolTip();
            this.lblSpellDefenseDecAttREA = new Chummer.LabelWithToolTip();
            this.cmsInitiationNotes = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsInitiationNotes = new System.Windows.Forms.ToolStripMenuItem();
            this.cmsTechnique = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsAddTechniqueNotes = new System.Windows.Forms.ToolStripMenuItem();
            this.cmsAdvancedProgram = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsAddAdvancedProgramOption = new System.Windows.Forms.ToolStripMenuItem();
            this.tsAIProgramNotes = new System.Windows.Forms.ToolStripMenuItem();
            this.cmsGearAllowRename = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsGearAllowRenameExtra = new System.Windows.Forms.ToolStripMenuItem();
            this.tlpMagicianMentorSpiritHeader = new Chummer.BufferedTableLayoutPanel(this.components);
            this.lblParagonSource = new System.Windows.Forms.Label();
            this.lblParagon = new System.Windows.Forms.Label();
            this.lblParagonSourceLabel = new System.Windows.Forms.Label();
            this.lblParagonLabel = new System.Windows.Forms.Label();
            this.tlpTechnomancerParagonHeader = new Chummer.BufferedTableLayoutPanel(this.components);
            this.StatusStrip.SuspendLayout();
            this.cmsMartialArts.SuspendLayout();
            this.cmsSpellButton.SuspendLayout();
            this.cmsComplexForm.SuspendLayout();
            this.cmsCyberware.SuspendLayout();
            this.cmsVehicleCyberware.SuspendLayout();
            this.cmsLifestyle.SuspendLayout();
            this.cmsArmor.SuspendLayout();
            this.cmsWeapon.SuspendLayout();
            this.cmsGearButton.SuspendLayout();
            this.cmsVehicle.SuspendLayout();
            this.cmsWeaponMount.SuspendLayout();
            this.mnuCreateMenu.SuspendLayout();
            this.tsMain.SuspendLayout();
            this.cmsGear.SuspendLayout();
            this.cmsVehicleWeapon.SuspendLayout();
            this.cmsVehicleGear.SuspendLayout();
            this.cmsArmorGear.SuspendLayout();
            this.cmsArmorMod.SuspendLayout();
            this.cmsQuality.SuspendLayout();
            this.cmsSpell.SuspendLayout();
            this.cmsCritterPowers.SuspendLayout();
            this.cmsLifestyleNotes.SuspendLayout();
            this.cmsWeaponAccessory.SuspendLayout();
            this.cmsGearPlugin.SuspendLayout();
            this.cmsComplexFormPlugin.SuspendLayout();
            this.cmsBioware.SuspendLayout();
            this.cmsAdvancedLifestyle.SuspendLayout();
            this.cmsGearLocation.SuspendLayout();
            this.cmsArmorLocation.SuspendLayout();
            this.cmsCyberwareGear.SuspendLayout();
            this.cmsVehicleCyberwareGear.SuspendLayout();
            this.cmsWeaponAccessoryGear.SuspendLayout();
            this.cmsVehicleLocation.SuspendLayout();
            this.cmsVehicleWeaponAccessory.SuspendLayout();
            this.cmsVehicleWeaponAccessoryGear.SuspendLayout();
            this.cmsWeaponLocation.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitMain)).BeginInit();
            this.splitMain.Panel1.SuspendLayout();
            this.splitMain.Panel2.SuspendLayout();
            this.splitMain.SuspendLayout();
            this.tabCharacterTabs.SuspendLayout();
            this.tabCommon.SuspendLayout();
            this.tlpCommon.SuspendLayout();
            this.tlpCommonLeftSide.SuspendLayout();
            this.tlpQualityButtons.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudQualityLevel)).BeginInit();
            this.tlpCommonLeftSideBottom.SuspendLayout();
            this.flpAttributesLabels.SuspendLayout();
            this.tlpAlias.SuspendLayout();
            this.tlpCommonRightSide.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudMysticAdeptMAGMagician)).BeginInit();
            this.tlpNuyen.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudNuyen)).BeginInit();
            this.tabSkills.SuspendLayout();
            this.tabLimits.SuspendLayout();
            this.tabMartialArts.SuspendLayout();
            this.tlpMartialArts.SuspendLayout();
            this.tlpMartialArtsButtons.SuspendLayout();
            this.tabMagician.SuspendLayout();
            this.tlpMagician.SuspendLayout();
            this.flpMagician.SuspendLayout();
            this.gpbMagicianSpell.SuspendLayout();
            this.tlpMagicianSpell.SuspendLayout();
            this.gpbMagicianTradition.SuspendLayout();
            this.tlpMagicianTradition.SuspendLayout();
            this.tlpDrainAttributes.SuspendLayout();
            this.gpbMagicianMentorSpirit.SuspendLayout();
            this.tlpMagicianMentorSpirit.SuspendLayout();
            this.tlpMagicianButtons.SuspendLayout();
            this.tabAdept.SuspendLayout();
            this.tabTechnomancer.SuspendLayout();
            this.tlpTechnomancer.SuspendLayout();
            this.flpTechnomancer.SuspendLayout();
            this.gpbTechnomancerComplexForm.SuspendLayout();
            this.tlpTechnomancerComplexForm.SuspendLayout();
            this.gpbTechnomancerStream.SuspendLayout();
            this.tlpTechnomancerStream.SuspendLayout();
            this.flpFadingAttributesValue.SuspendLayout();
            this.gpbTechnomancerParagon.SuspendLayout();
            this.tlpTechnomancerParagon.SuspendLayout();
            this.tlpTechnomancerButtons.SuspendLayout();
            this.tabAdvancedPrograms.SuspendLayout();
            this.tlpAdvancedPrograms.SuspendLayout();
            this.tlpAdvancedProgramsButtons.SuspendLayout();
            this.tabCritter.SuspendLayout();
            this.tlpCritter.SuspendLayout();
            this.tlpCritterButtons.SuspendLayout();
            this.tabInitiation.SuspendLayout();
            this.tlpInitiation.SuspendLayout();
            this.flpInitiation.SuspendLayout();
            this.gpbInitiationType.SuspendLayout();
            this.flpInitiationCheckBoxes.SuspendLayout();
            this.gpbInitiationGroup.SuspendLayout();
            this.tlpInitiationGroup.SuspendLayout();
            this.tlpInitiationButtons.SuspendLayout();
            this.cmsMetamagic.SuspendLayout();
            this.tabCyberware.SuspendLayout();
            this.tlpCyberware.SuspendLayout();
            this.flpCyberware.SuspendLayout();
            this.gpbEssenceConsumption.SuspendLayout();
            this.tlpCyberwareEssenceConsumption.SuspendLayout();
            this.gpbCyberwareCommon.SuspendLayout();
            this.tlpCyberwareCommon.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudCyberwareRating)).BeginInit();
            this.flpCyberwareCommonCheckBoxes.SuspendLayout();
            this.gpbCyberwareMatrix.SuspendLayout();
            this.tlpCyberwareMatrix.SuspendLayout();
            this.flpCyberwareMatrixCheckBoxes.SuspendLayout();
            this.tlpCyberwareButtons.SuspendLayout();
            this.tabStreetGear.SuspendLayout();
            this.tabStreetGearTabs.SuspendLayout();
            this.tabGear.SuspendLayout();
            this.tlpGear.SuspendLayout();
            this.flpGear.SuspendLayout();
            this.gpbGearCommon.SuspendLayout();
            this.tlpGearCommon.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudGearRating)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudGearQty)).BeginInit();
            this.flpGearCommonCheckBoxes.SuspendLayout();
            this.gpbGearMatrix.SuspendLayout();
            this.tlpGearMatrix.SuspendLayout();
            this.flpGearMatrixCheckBoxes.SuspendLayout();
            this.gpbGearBondedFoci.SuspendLayout();
            this.tlpGearBondedFoci.SuspendLayout();
            this.tlpGearButtons.SuspendLayout();
            this.tabArmor.SuspendLayout();
            this.tlpArmor.SuspendLayout();
            this.flpArmor.SuspendLayout();
            this.gpbArmorCommon.SuspendLayout();
            this.tlpArmorCommon.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudArmorRating)).BeginInit();
            this.flpArmorCommonCheckBoxes.SuspendLayout();
            this.gpbArmorMatrix.SuspendLayout();
            this.tlpArmorMatrix.SuspendLayout();
            this.gpbArmorLocation.SuspendLayout();
            this.tlpArmorLocation.SuspendLayout();
            this.flpArmorLocationButtons.SuspendLayout();
            this.tlpArmorButtons.SuspendLayout();
            this.tabWeapons.SuspendLayout();
            this.tlpWeapons.SuspendLayout();
            this.flpWeapons.SuspendLayout();
            this.gpbWeaponsCommon.SuspendLayout();
            this.tlpWeaponsCommon.SuspendLayout();
            this.flpWeaponsCommonCheckBoxes.SuspendLayout();
            this.gpbWeaponsWeapon.SuspendLayout();
            this.flpWeaponsWeapon.SuspendLayout();
            this.tlpWeaponsWeapon.SuspendLayout();
            this.tlpWeaponsRanges.SuspendLayout();
            this.gpbWeaponsMatrix.SuspendLayout();
            this.tlpWeaponsMatrix.SuspendLayout();
            this.tlpWeaponsButtons.SuspendLayout();
            this.tabDrugs.SuspendLayout();
            this.tlpDrugInfo.SuspendLayout();
            this.flpDrugs.SuspendLayout();
            this.gpbDrugsCommon.SuspendLayout();
            this.tlpDrugsCommon.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudDrugQty)).BeginInit();
            this.tlpDrugButtons.SuspendLayout();
            this.tabLifestyle.SuspendLayout();
            this.tlpLifestyleDetails.SuspendLayout();
            this.flpLifestyleDetails.SuspendLayout();
            this.gpbLifestyleCommon.SuspendLayout();
            this.tlpLifestyleCommon.SuspendLayout();
            this.flpLifestyleIntervals.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudLifestyleMonths)).BeginInit();
            this.tlpLifestyleButtons.SuspendLayout();
            this.tabVehicles.SuspendLayout();
            this.tlpVehicles.SuspendLayout();
            this.flpVehiclesButtons.SuspendLayout();
            this.flpVehicles.SuspendLayout();
            this.gpbVehiclesCommon.SuspendLayout();
            this.tlpVehiclesCommon.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudVehicleGearQty)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudVehicleRating)).BeginInit();
            this.flpVehiclesCommonCheckBoxes.SuspendLayout();
            this.gpbVehiclesVehicle.SuspendLayout();
            this.tlpVehiclesVehicle.SuspendLayout();
            this.gpbVehiclesWeapon.SuspendLayout();
            this.flpVehiclesWeapon.SuspendLayout();
            this.tlpVehiclesWeaponCommon.SuspendLayout();
            this.tlpVehiclesWeaponRanges.SuspendLayout();
            this.gpbVehiclesMatrix.SuspendLayout();
            this.tlpVehiclesMatrix.SuspendLayout();
            this.flpVehiclesMatrixCheckBoxes.SuspendLayout();
            this.tabCharacterInfo.SuspendLayout();
            this.tlpCharacterInfo.SuspendLayout();
            this.tlpLongTexts.SuspendLayout();
            this.gpbDescription.SuspendLayout();
            this.gpbBackground.SuspendLayout();
            this.gpbConcept.SuspendLayout();
            this.gpbNotes.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picMugshot)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudMugshotIndex)).BeginInit();
            this.tlpMugshotButtons.SuspendLayout();
            this.tabRelationships.SuspendLayout();
            this.tabPeople.SuspendLayout();
            this.tabContacts.SuspendLayout();
            this.tlpContacts.SuspendLayout();
            this.tlpContactsButtons.SuspendLayout();
            this.cmsAddContact.SuspendLayout();
            this.tabEnemies.SuspendLayout();
            this.tlpEnemies.SuspendLayout();
            this.flpEnemiesButtons.SuspendLayout();
            this.tabPets.SuspendLayout();
            this.tlpPets.SuspendLayout();
            this.flpPetsButtons.SuspendLayout();
            this.tabInfo.SuspendLayout();
            this.tabBPSummary.SuspendLayout();
            this.tlpKarmaSummary.SuspendLayout();
            this.tabOtherInfo.SuspendLayout();
            this.tlpOtherInfo.SuspendLayout();
            this.tabDefenses.SuspendLayout();
            this.tlpSpellDefense.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudCounterspellingDice)).BeginInit();
            this.cmsInitiationNotes.SuspendLayout();
            this.cmsTechnique.SuspendLayout();
            this.cmsAdvancedProgram.SuspendLayout();
            this.cmsGearAllowRename.SuspendLayout();
            this.tlpMagicianMentorSpiritHeader.SuspendLayout();
            this.tlpTechnomancerParagonHeader.SuspendLayout();
            this.SuspendLayout();
            // 
            // StatusStrip
            // 
            this.StatusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tslKarmaLabel,
            this.tslKarma,
            this.tslKarmaRemainingLabel,
            this.tslKarmaRemaining,
            this.tslEssenceLabel,
            this.tslEssence,
            this.tslNuyenRemainingLabel,
            this.tslNuyenRemaining,
            this.pgbProgress});
            this.StatusStrip.Location = new System.Drawing.Point(0, 657);
            this.StatusStrip.Name = "StatusStrip";
            this.StatusStrip.Size = new System.Drawing.Size(1264, 24);
            this.StatusStrip.TabIndex = 24;
            this.StatusStrip.Text = "StatusStrip1";
            // 
            // tslKarmaLabel
            // 
            this.tslKarmaLabel.Name = "tslKarmaLabel";
            this.tslKarmaLabel.Size = new System.Drawing.Size(44, 19);
            this.tslKarmaLabel.Tag = "Label_Karma";
            this.tslKarmaLabel.Text = "Karma:";
            // 
            // tslKarma
            // 
            this.tslKarma.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Right;
            this.tslKarma.Name = "tslKarma";
            this.tslKarma.Size = new System.Drawing.Size(29, 19);
            this.tslKarma.Text = "400";
            // 
            // tslKarmaRemainingLabel
            // 
            this.tslKarmaRemainingLabel.Name = "tslKarmaRemainingLabel";
            this.tslKarmaRemainingLabel.Size = new System.Drawing.Size(104, 19);
            this.tslKarmaRemainingLabel.Tag = "Label_KarmaRemaining";
            this.tslKarmaRemainingLabel.Text = "Karma Remaining:";
            // 
            // tslKarmaRemaining
            // 
            this.tslKarmaRemaining.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Right;
            this.tslKarmaRemaining.Name = "tslKarmaRemaining";
            this.tslKarmaRemaining.Size = new System.Drawing.Size(29, 19);
            this.tslKarmaRemaining.Text = "400";
            // 
            // tslEssenceLabel
            // 
            this.tslEssenceLabel.Name = "tslEssenceLabel";
            this.tslEssenceLabel.Size = new System.Drawing.Size(51, 19);
            this.tslEssenceLabel.Tag = "Label_OtherEssence";
            this.tslEssenceLabel.Text = "Essence:";
            // 
            // tslEssence
            // 
            this.tslEssence.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Right;
            this.tslEssence.Name = "tslEssence";
            this.tslEssence.Size = new System.Drawing.Size(32, 19);
            this.tslEssence.Text = "6.00";
            // 
            // tslNuyenRemainingLabel
            // 
            this.tslNuyenRemainingLabel.Name = "tslNuyenRemainingLabel";
            this.tslNuyenRemainingLabel.Size = new System.Drawing.Size(105, 19);
            this.tslNuyenRemainingLabel.Tag = "Label_OtherNuyenRemain";
            this.tslNuyenRemainingLabel.Text = "Nuyen Remaining:";
            // 
            // tslNuyenRemaining
            // 
            this.tslNuyenRemaining.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Right;
            this.tslNuyenRemaining.Name = "tslNuyenRemaining";
            this.tslNuyenRemaining.Size = new System.Drawing.Size(23, 19);
            this.tslNuyenRemaining.Text = "0¥";
            // 
            // pgbProgress
            // 
            this.pgbProgress.Name = "pgbProgress";
            this.pgbProgress.Size = new System.Drawing.Size(400, 18);
            this.pgbProgress.Visible = false;
            // 
            // dlgSaveFile
            // 
            this.dlgSaveFile.DefaultExt = "sr5";
            this.dlgSaveFile.Filter = "Chummer Character|*.sr5";
            this.dlgSaveFile.Title = "Save Character";
            // 
            // lblAttributesAug
            // 
            this.lblAttributesAug.AutoSize = true;
            this.lblAttributesAug.Dock = System.Windows.Forms.DockStyle.Right;
            this.lblAttributesAug.Location = new System.Drawing.Point(177, 0);
            this.lblAttributesAug.MinimumSize = new System.Drawing.Size(0, 24);
            this.lblAttributesAug.Name = "lblAttributesAug";
            this.lblAttributesAug.Size = new System.Drawing.Size(50, 24);
            this.lblAttributesAug.TabIndex = 55;
            this.lblAttributesAug.Tag = "Label_ValAugmented";
            this.lblAttributesAug.Text = "Val (Aug)";
            this.lblAttributesAug.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            // 
            // lblAttributesBase
            // 
            this.lblAttributesBase.AutoSize = true;
            this.lblAttributesBase.Dock = System.Windows.Forms.DockStyle.Right;
            this.lblAttributesBase.Location = new System.Drawing.Point(92, 0);
            this.lblAttributesBase.MinimumSize = new System.Drawing.Size(0, 24);
            this.lblAttributesBase.Name = "lblAttributesBase";
            this.lblAttributesBase.Size = new System.Drawing.Size(36, 24);
            this.lblAttributesBase.TabIndex = 54;
            this.lblAttributesBase.Tag = "String_Points";
            this.lblAttributesBase.Text = "Points";
            this.lblAttributesBase.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            // 
            // lblAttributesMetatype
            // 
            this.lblAttributesMetatype.AutoSize = true;
            this.lblAttributesMetatype.Dock = System.Windows.Forms.DockStyle.Right;
            this.lblAttributesMetatype.Location = new System.Drawing.Point(233, 0);
            this.lblAttributesMetatype.MinimumSize = new System.Drawing.Size(0, 24);
            this.lblAttributesMetatype.Name = "lblAttributesMetatype";
            this.lblAttributesMetatype.Size = new System.Drawing.Size(80, 24);
            this.lblAttributesMetatype.TabIndex = 53;
            this.lblAttributesMetatype.Tag = "Label_MetatypeLimits";
            this.lblAttributesMetatype.Text = "Metatype Limits";
            this.lblAttributesMetatype.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            // 
            // lblAttributes
            // 
            this.lblAttributes.AutoSize = true;
            this.lblAttributes.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.lblAttributes.Location = new System.Drawing.Point(304, 26);
            this.lblAttributes.MinimumSize = new System.Drawing.Size(0, 24);
            this.lblAttributes.Name = "lblAttributes";
            this.lblAttributes.Size = new System.Drawing.Size(51, 24);
            this.lblAttributes.TabIndex = 5;
            this.lblAttributes.Tag = "Label_Attributes";
            this.lblAttributes.Text = "Attributes";
            this.lblAttributes.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            // 
            // chkArmorEquipped
            // 
            this.chkArmorEquipped.AutoSize = true;
            this.chkArmorEquipped.Enabled = false;
            this.chkArmorEquipped.Location = new System.Drawing.Point(3, 3);
            this.chkArmorEquipped.Name = "chkArmorEquipped";
            this.chkArmorEquipped.Size = new System.Drawing.Size(71, 17);
            this.chkArmorEquipped.TabIndex = 78;
            this.chkArmorEquipped.Tag = "Checkbox_Equipped";
            this.chkArmorEquipped.Text = "Equipped";
            this.chkArmorEquipped.UseVisualStyleBackColor = true;
            this.chkArmorEquipped.CheckedChanged += new System.EventHandler(this.chkArmorEquipped_CheckedChanged);
            // 
            // chkWeaponAccessoryInstalled
            // 
            this.chkWeaponAccessoryInstalled.AutoSize = true;
            this.chkWeaponAccessoryInstalled.Enabled = false;
            this.chkWeaponAccessoryInstalled.Location = new System.Drawing.Point(3, 3);
            this.chkWeaponAccessoryInstalled.Name = "chkWeaponAccessoryInstalled";
            this.chkWeaponAccessoryInstalled.Size = new System.Drawing.Size(65, 17);
            this.chkWeaponAccessoryInstalled.TabIndex = 72;
            this.chkWeaponAccessoryInstalled.Tag = "Checkbox_Installed";
            this.chkWeaponAccessoryInstalled.Text = "Installed";
            this.chkWeaponAccessoryInstalled.UseVisualStyleBackColor = true;
            this.chkWeaponAccessoryInstalled.CheckedChanged += new System.EventHandler(this.chkWeaponAccessoryInstalled_CheckedChanged);
            // 
            // lblNotoriety
            // 
            this.lblNotoriety.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblNotoriety.AutoSize = true;
            this.tlpCharacterInfo.SetColumnSpan(this.lblNotoriety, 3);
            this.lblNotoriety.Location = new System.Drawing.Point(839, 110);
            this.lblNotoriety.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblNotoriety.Name = "lblNotoriety";
            this.lblNotoriety.Size = new System.Drawing.Size(52, 13);
            this.lblNotoriety.TabIndex = 84;
            this.lblNotoriety.Tag = "Label_Notoriety";
            this.lblNotoriety.Text = "Notoriety:";
            this.lblNotoriety.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblStreetCred
            // 
            this.lblStreetCred.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblStreetCred.AutoSize = true;
            this.tlpCharacterInfo.SetColumnSpan(this.lblStreetCred, 3);
            this.lblStreetCred.Location = new System.Drawing.Point(828, 85);
            this.lblStreetCred.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblStreetCred.Name = "lblStreetCred";
            this.lblStreetCred.Size = new System.Drawing.Size(63, 13);
            this.lblStreetCred.TabIndex = 82;
            this.lblStreetCred.Tag = "Label_StreetCred";
            this.lblStreetCred.Text = "Street Cred:";
            this.lblStreetCred.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // chkCharacterCreated
            // 
            this.chkCharacterCreated.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkCharacterCreated.AutoSize = true;
            this.tlpCharacterInfo.SetColumnSpan(this.chkCharacterCreated, 3);
            this.chkCharacterCreated.Location = new System.Drawing.Point(778, 4);
            this.chkCharacterCreated.Name = "chkCharacterCreated";
            this.chkCharacterCreated.Size = new System.Drawing.Size(152, 17);
            this.chkCharacterCreated.TabIndex = 62;
            this.chkCharacterCreated.Tag = "Checkbox_Created";
            this.chkCharacterCreated.Text = "Mark character as Created";
            this.chkCharacterCreated.UseVisualStyleBackColor = true;
            // 
            // lblBuildFoci
            // 
            this.lblBuildFoci.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblBuildFoci.AutoSize = true;
            this.lblBuildFoci.Location = new System.Drawing.Point(113, 381);
            this.lblBuildFoci.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblBuildFoci.Name = "lblBuildFoci";
            this.lblBuildFoci.Size = new System.Drawing.Size(27, 13);
            this.lblBuildFoci.TabIndex = 81;
            this.lblBuildFoci.Tag = "Label_SummaryFoci";
            this.lblBuildFoci.Text = "Foci";
            // 
            // lblBuildMartialArts
            // 
            this.lblBuildMartialArts.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblBuildMartialArts.AutoSize = true;
            this.lblBuildMartialArts.Location = new System.Drawing.Point(81, 506);
            this.lblBuildMartialArts.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblBuildMartialArts.Name = "lblBuildMartialArts";
            this.lblBuildMartialArts.Size = new System.Drawing.Size(59, 13);
            this.lblBuildMartialArts.TabIndex = 79;
            this.lblBuildMartialArts.Tag = "Tab_MartialArts";
            this.lblBuildMartialArts.Text = "Martial Arts";
            // 
            // lblBuildNuyen
            // 
            this.lblBuildNuyen.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblBuildNuyen.AutoSize = true;
            this.lblBuildNuyen.Location = new System.Drawing.Point(102, 206);
            this.lblBuildNuyen.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblBuildNuyen.Name = "lblBuildNuyen";
            this.lblBuildNuyen.Size = new System.Drawing.Size(38, 13);
            this.lblBuildNuyen.TabIndex = 76;
            this.lblBuildNuyen.Tag = "Label_SummaryNuyen";
            this.lblBuildNuyen.Text = "Nuyen";
            // 
            // lblBuildEnemies
            // 
            this.lblBuildEnemies.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblBuildEnemies.AutoSize = true;
            this.lblBuildEnemies.Location = new System.Drawing.Point(93, 181);
            this.lblBuildEnemies.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblBuildEnemies.Name = "lblBuildEnemies";
            this.lblBuildEnemies.Size = new System.Drawing.Size(47, 13);
            this.lblBuildEnemies.TabIndex = 75;
            this.lblBuildEnemies.Tag = "Label_SummaryEnemies";
            this.lblBuildEnemies.Text = "Enemies";
            // 
            // lblBuildComplexForms
            // 
            this.lblBuildComplexForms.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblBuildComplexForms.AutoSize = true;
            this.lblBuildComplexForms.Location = new System.Drawing.Point(62, 456);
            this.lblBuildComplexForms.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblBuildComplexForms.Name = "lblBuildComplexForms";
            this.lblBuildComplexForms.Size = new System.Drawing.Size(78, 13);
            this.lblBuildComplexForms.TabIndex = 72;
            this.lblBuildComplexForms.Tag = "Label_SummaryComplexForms";
            this.lblBuildComplexForms.Text = "Complex Forms";
            // 
            // lblBuildSprites
            // 
            this.lblBuildSprites.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblBuildSprites.AutoSize = true;
            this.lblBuildSprites.Location = new System.Drawing.Point(101, 431);
            this.lblBuildSprites.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblBuildSprites.Name = "lblBuildSprites";
            this.lblBuildSprites.Size = new System.Drawing.Size(39, 13);
            this.lblBuildSprites.TabIndex = 71;
            this.lblBuildSprites.Tag = "Label_SummarySprites";
            this.lblBuildSprites.Text = "Sprites";
            // 
            // lblBuildSpirits
            // 
            this.lblBuildSpirits.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblBuildSpirits.AutoSize = true;
            this.lblBuildSpirits.Location = new System.Drawing.Point(105, 406);
            this.lblBuildSpirits.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblBuildSpirits.Name = "lblBuildSpirits";
            this.lblBuildSpirits.Size = new System.Drawing.Size(35, 13);
            this.lblBuildSpirits.TabIndex = 70;
            this.lblBuildSpirits.Tag = "Label_SummarySpirits";
            this.lblBuildSpirits.Text = "Spirits";
            // 
            // lblBuildSpells
            // 
            this.lblBuildSpells.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblBuildSpells.AutoSize = true;
            this.lblBuildSpells.Location = new System.Drawing.Point(105, 306);
            this.lblBuildSpells.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblBuildSpells.Name = "lblBuildSpells";
            this.lblBuildSpells.Size = new System.Drawing.Size(35, 13);
            this.lblBuildSpells.TabIndex = 69;
            this.lblBuildSpells.Tag = "Label_SummarySpells";
            this.lblBuildSpells.Text = "Spells";
            // 
            // lblBuildKnowledgeSkills
            // 
            this.lblBuildKnowledgeSkills.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblBuildKnowledgeSkills.AutoSize = true;
            this.lblBuildKnowledgeSkills.Location = new System.Drawing.Point(53, 281);
            this.lblBuildKnowledgeSkills.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblBuildKnowledgeSkills.Name = "lblBuildKnowledgeSkills";
            this.lblBuildKnowledgeSkills.Size = new System.Drawing.Size(87, 13);
            this.lblBuildKnowledgeSkills.TabIndex = 64;
            this.lblBuildKnowledgeSkills.Tag = "Label_SummaryKnowledgeSkills";
            this.lblBuildKnowledgeSkills.Text = "Knowledge Skills";
            // 
            // lblBuildActiveSkills
            // 
            this.lblBuildActiveSkills.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblBuildActiveSkills.AutoSize = true;
            this.lblBuildActiveSkills.Location = new System.Drawing.Point(76, 256);
            this.lblBuildActiveSkills.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblBuildActiveSkills.Name = "lblBuildActiveSkills";
            this.lblBuildActiveSkills.Size = new System.Drawing.Size(64, 13);
            this.lblBuildActiveSkills.TabIndex = 63;
            this.lblBuildActiveSkills.Tag = "Label_SummaryActiveSkills";
            this.lblBuildActiveSkills.Text = "Active Skills";
            // 
            // lblBuildSkillGroups
            // 
            this.lblBuildSkillGroups.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblBuildSkillGroups.AutoSize = true;
            this.lblBuildSkillGroups.Location = new System.Drawing.Point(77, 231);
            this.lblBuildSkillGroups.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblBuildSkillGroups.Name = "lblBuildSkillGroups";
            this.lblBuildSkillGroups.Size = new System.Drawing.Size(63, 13);
            this.lblBuildSkillGroups.TabIndex = 62;
            this.lblBuildSkillGroups.Tag = "Label_SummarySkillGroups";
            this.lblBuildSkillGroups.Text = "Skill Groups";
            // 
            // lblBuildContacts
            // 
            this.lblBuildContacts.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblBuildContacts.AutoSize = true;
            this.lblBuildContacts.Location = new System.Drawing.Point(91, 156);
            this.lblBuildContacts.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblBuildContacts.Name = "lblBuildContacts";
            this.lblBuildContacts.Size = new System.Drawing.Size(49, 13);
            this.lblBuildContacts.TabIndex = 59;
            this.lblBuildContacts.Tag = "Label_SummaryContacts";
            this.lblBuildContacts.Text = "Contacts";
            // 
            // lblBuildPrimaryAttributes
            // 
            this.lblBuildPrimaryAttributes.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblBuildPrimaryAttributes.AutoSize = true;
            this.lblBuildPrimaryAttributes.Location = new System.Drawing.Point(89, 31);
            this.lblBuildPrimaryAttributes.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblBuildPrimaryAttributes.Name = "lblBuildPrimaryAttributes";
            this.lblBuildPrimaryAttributes.Size = new System.Drawing.Size(51, 13);
            this.lblBuildPrimaryAttributes.TabIndex = 57;
            this.lblBuildPrimaryAttributes.Tag = "Label_Attributes";
            this.lblBuildPrimaryAttributes.Text = "Attributes";
            // 
            // lblBuildNegativeQualities
            // 
            this.lblBuildNegativeQualities.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblBuildNegativeQualities.AutoSize = true;
            this.lblBuildNegativeQualities.Location = new System.Drawing.Point(47, 106);
            this.lblBuildNegativeQualities.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblBuildNegativeQualities.Name = "lblBuildNegativeQualities";
            this.lblBuildNegativeQualities.Size = new System.Drawing.Size(93, 13);
            this.lblBuildNegativeQualities.TabIndex = 54;
            this.lblBuildNegativeQualities.Tag = "Label_SummaryNegativeQualities";
            this.lblBuildNegativeQualities.Text = "Negative Qualities";
            // 
            // lblBuildPositiveQualities
            // 
            this.lblBuildPositiveQualities.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblBuildPositiveQualities.AutoSize = true;
            this.lblBuildPositiveQualities.Location = new System.Drawing.Point(53, 81);
            this.lblBuildPositiveQualities.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblBuildPositiveQualities.Name = "lblBuildPositiveQualities";
            this.lblBuildPositiveQualities.Size = new System.Drawing.Size(87, 13);
            this.lblBuildPositiveQualities.TabIndex = 50;
            this.lblBuildPositiveQualities.Tag = "Label_SummaryPositiveQualities";
            this.lblBuildPositiveQualities.Text = "Positive Qualities";
            // 
            // lblRiggingINILabel
            // 
            this.lblRiggingINILabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblRiggingINILabel.AutoSize = true;
            this.lblRiggingINILabel.Location = new System.Drawing.Point(54, 181);
            this.lblRiggingINILabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblRiggingINILabel.Name = "lblRiggingINILabel";
            this.lblRiggingINILabel.Size = new System.Drawing.Size(112, 13);
            this.lblRiggingINILabel.TabIndex = 75;
            this.lblRiggingINILabel.Tag = "Label_OtherRiggingInit";
            this.lblRiggingINILabel.Text = "Rigging Initiative (AR):";
            // 
            // lblMatrixINIHotLabel
            // 
            this.lblMatrixINIHotLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblMatrixINIHotLabel.AutoSize = true;
            this.lblMatrixINIHotLabel.Location = new System.Drawing.Point(60, 156);
            this.lblMatrixINIHotLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblMatrixINIHotLabel.Name = "lblMatrixINIHotLabel";
            this.lblMatrixINIHotLabel.Size = new System.Drawing.Size(106, 13);
            this.lblMatrixINIHotLabel.TabIndex = 73;
            this.lblMatrixINIHotLabel.Tag = "Label_OtherMatrixInitVRHot";
            this.lblMatrixINIHotLabel.Text = "Matrix Initiative (Hot):";
            // 
            // lblMatrixINIColdLabel
            // 
            this.lblMatrixINIColdLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblMatrixINIColdLabel.AutoSize = true;
            this.lblMatrixINIColdLabel.Location = new System.Drawing.Point(56, 131);
            this.lblMatrixINIColdLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblMatrixINIColdLabel.Name = "lblMatrixINIColdLabel";
            this.lblMatrixINIColdLabel.Size = new System.Drawing.Size(110, 13);
            this.lblMatrixINIColdLabel.TabIndex = 71;
            this.lblMatrixINIColdLabel.Tag = "Label_OtherMatrixInitVRCold";
            this.lblMatrixINIColdLabel.Text = "Matrix Initiative (Cold):";
            // 
            // lblMemoryLabel
            // 
            this.lblMemoryLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblMemoryLabel.AutoSize = true;
            this.lblMemoryLabel.Location = new System.Drawing.Point(119, 406);
            this.lblMemoryLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblMemoryLabel.Name = "lblMemoryLabel";
            this.lblMemoryLabel.Size = new System.Drawing.Size(47, 13);
            this.lblMemoryLabel.TabIndex = 50;
            this.lblMemoryLabel.Tag = "Label_OtherMemory";
            this.lblMemoryLabel.Text = "Memory:";
            // 
            // lblLiftCarryLabel
            // 
            this.lblLiftCarryLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblLiftCarryLabel.AutoSize = true;
            this.lblLiftCarryLabel.Location = new System.Drawing.Point(94, 381);
            this.lblLiftCarryLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblLiftCarryLabel.Name = "lblLiftCarryLabel";
            this.lblLiftCarryLabel.Size = new System.Drawing.Size(72, 13);
            this.lblLiftCarryLabel.TabIndex = 48;
            this.lblLiftCarryLabel.Tag = "Label_OtherLiftAndCarry";
            this.lblLiftCarryLabel.Text = "Lift and Carry:";
            // 
            // lblJudgeIntentionsLabel
            // 
            this.lblJudgeIntentionsLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblJudgeIntentionsLabel.AutoSize = true;
            this.lblJudgeIntentionsLabel.Location = new System.Drawing.Point(78, 331);
            this.lblJudgeIntentionsLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblJudgeIntentionsLabel.Name = "lblJudgeIntentionsLabel";
            this.lblJudgeIntentionsLabel.Size = new System.Drawing.Size(88, 13);
            this.lblJudgeIntentionsLabel.TabIndex = 46;
            this.lblJudgeIntentionsLabel.Tag = "Label_OtherJudgeIntention";
            this.lblJudgeIntentionsLabel.Text = "Judge Intentions:";
            // 
            // lblComposureLabel
            // 
            this.lblComposureLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblComposureLabel.AutoSize = true;
            this.lblComposureLabel.Location = new System.Drawing.Point(103, 306);
            this.lblComposureLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblComposureLabel.Name = "lblComposureLabel";
            this.lblComposureLabel.Size = new System.Drawing.Size(63, 13);
            this.lblComposureLabel.TabIndex = 44;
            this.lblComposureLabel.Tag = "Label_OtherComposure";
            this.lblComposureLabel.Text = "Composure:";
            // 
            // lblRemainingNuyenLabel
            // 
            this.lblRemainingNuyenLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblRemainingNuyenLabel.AutoSize = true;
            this.lblRemainingNuyenLabel.Location = new System.Drawing.Point(72, 281);
            this.lblRemainingNuyenLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblRemainingNuyenLabel.Name = "lblRemainingNuyenLabel";
            this.lblRemainingNuyenLabel.Size = new System.Drawing.Size(94, 13);
            this.lblRemainingNuyenLabel.TabIndex = 36;
            this.lblRemainingNuyenLabel.Tag = "Label_OtherNuyenRemain";
            this.lblRemainingNuyenLabel.Text = "Nuyen Remaining:";
            // 
            // lblESS
            // 
            this.lblESS.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblESS.AutoSize = true;
            this.lblESS.Location = new System.Drawing.Point(115, 256);
            this.lblESS.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblESS.Name = "lblESS";
            this.lblESS.Size = new System.Drawing.Size(51, 13);
            this.lblESS.TabIndex = 34;
            this.lblESS.Tag = "Label_OtherEssence";
            this.lblESS.Text = "Essence:";
            // 
            // lblArmorLabel
            // 
            this.lblArmorLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblArmorLabel.AutoSize = true;
            this.lblArmorLabel.Location = new System.Drawing.Point(129, 206);
            this.lblArmorLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblArmorLabel.Name = "lblArmorLabel";
            this.lblArmorLabel.Size = new System.Drawing.Size(37, 13);
            this.lblArmorLabel.TabIndex = 70;
            this.lblArmorLabel.Tag = "Label_ArmorValueShort";
            this.lblArmorLabel.Text = "Armor:";
            // 
            // lblAstralINILabel
            // 
            this.lblAstralINILabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblAstralINILabel.AutoSize = true;
            this.lblAstralINILabel.Location = new System.Drawing.Point(88, 81);
            this.lblAstralINILabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblAstralINILabel.Name = "lblAstralINILabel";
            this.lblAstralINILabel.Size = new System.Drawing.Size(78, 13);
            this.lblAstralINILabel.TabIndex = 23;
            this.lblAstralINILabel.Tag = "Label_OtherAstralInit";
            this.lblAstralINILabel.Text = "Astral Initiative:";
            // 
            // lblMatrixINILabel
            // 
            this.lblMatrixINILabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblMatrixINILabel.AutoSize = true;
            this.lblMatrixINILabel.Location = new System.Drawing.Point(62, 106);
            this.lblMatrixINILabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblMatrixINILabel.Name = "lblMatrixINILabel";
            this.lblMatrixINILabel.Size = new System.Drawing.Size(104, 13);
            this.lblMatrixINILabel.TabIndex = 22;
            this.lblMatrixINILabel.Tag = "Label_OtherMatrixInit";
            this.lblMatrixINILabel.Text = "Matrix Initiative (AR):";
            // 
            // lblINILabel
            // 
            this.lblINILabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblINILabel.AutoSize = true;
            this.lblINILabel.Location = new System.Drawing.Point(117, 56);
            this.lblINILabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblINILabel.Name = "lblINILabel";
            this.lblINILabel.Size = new System.Drawing.Size(49, 13);
            this.lblINILabel.TabIndex = 20;
            this.lblINILabel.Tag = "Label_OtherInit";
            this.lblINILabel.Text = "Initiative:";
            // 
            // lblCMStunLabel
            // 
            this.lblCMStunLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblCMStunLabel.AutoSize = true;
            this.lblCMStunLabel.Location = new System.Drawing.Point(56, 31);
            this.lblCMStunLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblCMStunLabel.Name = "lblCMStunLabel";
            this.lblCMStunLabel.Size = new System.Drawing.Size(110, 13);
            this.lblCMStunLabel.TabIndex = 19;
            this.lblCMStunLabel.Tag = "Label_OtherStunCM";
            this.lblCMStunLabel.Text = "Stun Condition Track:";
            // 
            // lblCMPhysicalLabel
            // 
            this.lblCMPhysicalLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblCMPhysicalLabel.AutoSize = true;
            this.lblCMPhysicalLabel.Location = new System.Drawing.Point(39, 6);
            this.lblCMPhysicalLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblCMPhysicalLabel.Name = "lblCMPhysicalLabel";
            this.lblCMPhysicalLabel.Size = new System.Drawing.Size(127, 13);
            this.lblCMPhysicalLabel.TabIndex = 18;
            this.lblCMPhysicalLabel.Tag = "Label_OtherPhysicalCM";
            this.lblCMPhysicalLabel.Text = "Physical Condition Track:";
            // 
            // lblKarma
            // 
            this.lblKarma.AutoSize = true;
            this.lblKarma.Dock = System.Windows.Forms.DockStyle.Right;
            this.lblKarma.Location = new System.Drawing.Point(134, 0);
            this.lblKarma.MinimumSize = new System.Drawing.Size(0, 24);
            this.lblKarma.Name = "lblKarma";
            this.lblKarma.Size = new System.Drawing.Size(37, 24);
            this.lblKarma.TabIndex = 92;
            this.lblKarma.Tag = "String_Karma";
            this.lblKarma.Text = "Karma";
            this.lblKarma.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            // 
            // lblSpellDefenseIndirectDodgeLabel
            // 
            this.lblSpellDefenseIndirectDodgeLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblSpellDefenseIndirectDodgeLabel.AutoSize = true;
            this.lblSpellDefenseIndirectDodgeLabel.Location = new System.Drawing.Point(89, 32);
            this.lblSpellDefenseIndirectDodgeLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSpellDefenseIndirectDodgeLabel.Name = "lblSpellDefenseIndirectDodgeLabel";
            this.lblSpellDefenseIndirectDodgeLabel.Size = new System.Drawing.Size(77, 13);
            this.lblSpellDefenseIndirectDodgeLabel.TabIndex = 25;
            this.lblSpellDefenseIndirectDodgeLabel.Tag = "Label_SpellDefenseIndirectDodge";
            this.lblSpellDefenseIndirectDodgeLabel.Text = "Indirect Dodge";
            // 
            // lblSpellDefenseIndirectSoakLabel
            // 
            this.lblSpellDefenseIndirectSoakLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblSpellDefenseIndirectSoakLabel.AutoSize = true;
            this.lblSpellDefenseIndirectSoakLabel.Location = new System.Drawing.Point(96, 57);
            this.lblSpellDefenseIndirectSoakLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSpellDefenseIndirectSoakLabel.Name = "lblSpellDefenseIndirectSoakLabel";
            this.lblSpellDefenseIndirectSoakLabel.Size = new System.Drawing.Size(70, 13);
            this.lblSpellDefenseIndirectSoakLabel.TabIndex = 27;
            this.lblSpellDefenseIndirectSoakLabel.Tag = "Label_SpellDefenseIndirect";
            this.lblSpellDefenseIndirectSoakLabel.Text = "Indirect Soak";
            // 
            // lblSpellDefenseDirectSoakManaLabel
            // 
            this.lblSpellDefenseDirectSoakManaLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblSpellDefenseDirectSoakManaLabel.AutoSize = true;
            this.lblSpellDefenseDirectSoakManaLabel.Location = new System.Drawing.Point(67, 82);
            this.lblSpellDefenseDirectSoakManaLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSpellDefenseDirectSoakManaLabel.Name = "lblSpellDefenseDirectSoakManaLabel";
            this.lblSpellDefenseDirectSoakManaLabel.Size = new System.Drawing.Size(99, 13);
            this.lblSpellDefenseDirectSoakManaLabel.TabIndex = 29;
            this.lblSpellDefenseDirectSoakManaLabel.Tag = "Label_SpellDefenseDirectSoakMana";
            this.lblSpellDefenseDirectSoakManaLabel.Text = "Direct Soak - Mana";
            // 
            // lblSpellDefenseDirectSoakPhysicalLabel
            // 
            this.lblSpellDefenseDirectSoakPhysicalLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblSpellDefenseDirectSoakPhysicalLabel.AutoSize = true;
            this.lblSpellDefenseDirectSoakPhysicalLabel.Location = new System.Drawing.Point(55, 107);
            this.lblSpellDefenseDirectSoakPhysicalLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSpellDefenseDirectSoakPhysicalLabel.Name = "lblSpellDefenseDirectSoakPhysicalLabel";
            this.lblSpellDefenseDirectSoakPhysicalLabel.Size = new System.Drawing.Size(111, 13);
            this.lblSpellDefenseDirectSoakPhysicalLabel.TabIndex = 31;
            this.lblSpellDefenseDirectSoakPhysicalLabel.Tag = "Label_SpellDefenseDirectSoakPhysical";
            this.lblSpellDefenseDirectSoakPhysicalLabel.Text = "Direct Soak - Physical";
            // 
            // lblSpellDefenseDecAttBODLabel
            // 
            this.lblSpellDefenseDecAttBODLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblSpellDefenseDecAttBODLabel.AutoSize = true;
            this.lblSpellDefenseDecAttBODLabel.Location = new System.Drawing.Point(39, 157);
            this.lblSpellDefenseDecAttBODLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSpellDefenseDecAttBODLabel.Name = "lblSpellDefenseDecAttBODLabel";
            this.lblSpellDefenseDecAttBODLabel.Size = new System.Drawing.Size(127, 13);
            this.lblSpellDefenseDecAttBODLabel.TabIndex = 35;
            this.lblSpellDefenseDecAttBODLabel.Tag = "Label_SpellDefenseDecAttBOD";
            this.lblSpellDefenseDecAttBODLabel.Text = "Decrease Attribute (BOD)";
            // 
            // lblSpellDefenseDetectionLabel
            // 
            this.lblSpellDefenseDetectionLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblSpellDefenseDetectionLabel.AutoSize = true;
            this.lblSpellDefenseDetectionLabel.Location = new System.Drawing.Point(82, 132);
            this.lblSpellDefenseDetectionLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSpellDefenseDetectionLabel.Name = "lblSpellDefenseDetectionLabel";
            this.lblSpellDefenseDetectionLabel.Size = new System.Drawing.Size(84, 13);
            this.lblSpellDefenseDetectionLabel.TabIndex = 33;
            this.lblSpellDefenseDetectionLabel.Tag = "Label_SpellDefenseDetection";
            this.lblSpellDefenseDetectionLabel.Text = "Detection Spells";
            // 
            // lblSpellDefenseDecAttAGILabel
            // 
            this.lblSpellDefenseDecAttAGILabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblSpellDefenseDecAttAGILabel.AutoSize = true;
            this.lblSpellDefenseDecAttAGILabel.Location = new System.Drawing.Point(44, 182);
            this.lblSpellDefenseDecAttAGILabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSpellDefenseDecAttAGILabel.Name = "lblSpellDefenseDecAttAGILabel";
            this.lblSpellDefenseDecAttAGILabel.Size = new System.Drawing.Size(122, 13);
            this.lblSpellDefenseDecAttAGILabel.TabIndex = 41;
            this.lblSpellDefenseDecAttAGILabel.Tag = "Label_SpellDefenseDecAttAGI";
            this.lblSpellDefenseDecAttAGILabel.Text = "Decrease Attribute (AGI)";
            // 
            // lblSpellDefenseDecAttSTRLabel
            // 
            this.lblSpellDefenseDecAttSTRLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblSpellDefenseDecAttSTRLabel.AutoSize = true;
            this.lblSpellDefenseDecAttSTRLabel.Location = new System.Drawing.Point(40, 232);
            this.lblSpellDefenseDecAttSTRLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSpellDefenseDecAttSTRLabel.Name = "lblSpellDefenseDecAttSTRLabel";
            this.lblSpellDefenseDecAttSTRLabel.Size = new System.Drawing.Size(126, 13);
            this.lblSpellDefenseDecAttSTRLabel.TabIndex = 43;
            this.lblSpellDefenseDecAttSTRLabel.Tag = "Label_SpellDefenseDecAttSTR";
            this.lblSpellDefenseDecAttSTRLabel.Text = "Decrease Attribute (STR)";
            // 
            // lblSpellDefenseDecAttREALabel
            // 
            this.lblSpellDefenseDecAttREALabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblSpellDefenseDecAttREALabel.AutoSize = true;
            this.lblSpellDefenseDecAttREALabel.Location = new System.Drawing.Point(40, 207);
            this.lblSpellDefenseDecAttREALabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSpellDefenseDecAttREALabel.Name = "lblSpellDefenseDecAttREALabel";
            this.lblSpellDefenseDecAttREALabel.Size = new System.Drawing.Size(126, 13);
            this.lblSpellDefenseDecAttREALabel.TabIndex = 42;
            this.lblSpellDefenseDecAttREALabel.Tag = "Label_SpellDefenseDecAttREA";
            this.lblSpellDefenseDecAttREALabel.Text = "Decrease Attribute (REA)";
            // 
            // lblSpellDefenseDecAttWILLabel
            // 
            this.lblSpellDefenseDecAttWILLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblSpellDefenseDecAttWILLabel.AutoSize = true;
            this.lblSpellDefenseDecAttWILLabel.Location = new System.Drawing.Point(42, 332);
            this.lblSpellDefenseDecAttWILLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSpellDefenseDecAttWILLabel.Name = "lblSpellDefenseDecAttWILLabel";
            this.lblSpellDefenseDecAttWILLabel.Size = new System.Drawing.Size(124, 13);
            this.lblSpellDefenseDecAttWILLabel.TabIndex = 47;
            this.lblSpellDefenseDecAttWILLabel.Tag = "Label_SpellDefenseDecAttWIL";
            this.lblSpellDefenseDecAttWILLabel.Text = "Decrease Attribute (WIL)";
            // 
            // lblSpellDefenseDecAttLOGLabel
            // 
            this.lblSpellDefenseDecAttLOGLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblSpellDefenseDecAttLOGLabel.AutoSize = true;
            this.lblSpellDefenseDecAttLOGLabel.Location = new System.Drawing.Point(40, 307);
            this.lblSpellDefenseDecAttLOGLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSpellDefenseDecAttLOGLabel.Name = "lblSpellDefenseDecAttLOGLabel";
            this.lblSpellDefenseDecAttLOGLabel.Size = new System.Drawing.Size(126, 13);
            this.lblSpellDefenseDecAttLOGLabel.TabIndex = 46;
            this.lblSpellDefenseDecAttLOGLabel.Tag = "Label_SpellDefenseDecAttLOG";
            this.lblSpellDefenseDecAttLOGLabel.Text = "Decrease Attribute (LOG)";
            // 
            // lblSpellDefenseDecAttINTLabel
            // 
            this.lblSpellDefenseDecAttINTLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblSpellDefenseDecAttINTLabel.AutoSize = true;
            this.lblSpellDefenseDecAttINTLabel.Location = new System.Drawing.Point(44, 282);
            this.lblSpellDefenseDecAttINTLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSpellDefenseDecAttINTLabel.Name = "lblSpellDefenseDecAttINTLabel";
            this.lblSpellDefenseDecAttINTLabel.Size = new System.Drawing.Size(122, 13);
            this.lblSpellDefenseDecAttINTLabel.TabIndex = 45;
            this.lblSpellDefenseDecAttINTLabel.Tag = "Label_SpellDefenseDecAttINT";
            this.lblSpellDefenseDecAttINTLabel.Text = "Decrease Attribute (INT)";
            // 
            // lblSpellDefenseDecAttCHALabel
            // 
            this.lblSpellDefenseDecAttCHALabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblSpellDefenseDecAttCHALabel.AutoSize = true;
            this.lblSpellDefenseDecAttCHALabel.Location = new System.Drawing.Point(40, 257);
            this.lblSpellDefenseDecAttCHALabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSpellDefenseDecAttCHALabel.Name = "lblSpellDefenseDecAttCHALabel";
            this.lblSpellDefenseDecAttCHALabel.Size = new System.Drawing.Size(126, 13);
            this.lblSpellDefenseDecAttCHALabel.TabIndex = 44;
            this.lblSpellDefenseDecAttCHALabel.Tag = "Label_SpellDefenseDecAttCHA";
            this.lblSpellDefenseDecAttCHALabel.Text = "Decrease Attribute (CHA)";
            // 
            // lblSpellDefenseManipPhysicalLabel
            // 
            this.lblSpellDefenseManipPhysicalLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblSpellDefenseManipPhysicalLabel.AutoSize = true;
            this.lblSpellDefenseManipPhysicalLabel.Location = new System.Drawing.Point(51, 432);
            this.lblSpellDefenseManipPhysicalLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSpellDefenseManipPhysicalLabel.Name = "lblSpellDefenseManipPhysicalLabel";
            this.lblSpellDefenseManipPhysicalLabel.Size = new System.Drawing.Size(115, 13);
            this.lblSpellDefenseManipPhysicalLabel.TabIndex = 59;
            this.lblSpellDefenseManipPhysicalLabel.Tag = "Label_SpellDefenseManipPhysical";
            this.lblSpellDefenseManipPhysicalLabel.Text = "Manipulation - Physical";
            // 
            // lblSpellDefenseManipMentalLabel
            // 
            this.lblSpellDefenseManipMentalLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblSpellDefenseManipMentalLabel.AutoSize = true;
            this.lblSpellDefenseManipMentalLabel.Location = new System.Drawing.Point(58, 407);
            this.lblSpellDefenseManipMentalLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSpellDefenseManipMentalLabel.Name = "lblSpellDefenseManipMentalLabel";
            this.lblSpellDefenseManipMentalLabel.Size = new System.Drawing.Size(108, 13);
            this.lblSpellDefenseManipMentalLabel.TabIndex = 57;
            this.lblSpellDefenseManipMentalLabel.Tag = "Label_SpellDefenseManipMental";
            this.lblSpellDefenseManipMentalLabel.Text = "Manipulation - Mental";
            // 
            // lblSpellDefenseIllusionPhysicalLabel
            // 
            this.lblSpellDefenseIllusionPhysicalLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblSpellDefenseIllusionPhysicalLabel.AutoSize = true;
            this.lblSpellDefenseIllusionPhysicalLabel.Location = new System.Drawing.Point(79, 382);
            this.lblSpellDefenseIllusionPhysicalLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSpellDefenseIllusionPhysicalLabel.Name = "lblSpellDefenseIllusionPhysicalLabel";
            this.lblSpellDefenseIllusionPhysicalLabel.Size = new System.Drawing.Size(87, 13);
            this.lblSpellDefenseIllusionPhysicalLabel.TabIndex = 55;
            this.lblSpellDefenseIllusionPhysicalLabel.Tag = "Label_SpellDefenseIllusionPhysical";
            this.lblSpellDefenseIllusionPhysicalLabel.Text = "Illusion - Physical";
            // 
            // lblSpellDefenseIllusionManaLabel
            // 
            this.lblSpellDefenseIllusionManaLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblSpellDefenseIllusionManaLabel.AutoSize = true;
            this.lblSpellDefenseIllusionManaLabel.Location = new System.Drawing.Point(91, 357);
            this.lblSpellDefenseIllusionManaLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSpellDefenseIllusionManaLabel.Name = "lblSpellDefenseIllusionManaLabel";
            this.lblSpellDefenseIllusionManaLabel.Size = new System.Drawing.Size(75, 13);
            this.lblSpellDefenseIllusionManaLabel.TabIndex = 53;
            this.lblSpellDefenseIllusionManaLabel.Tag = "Label_SpellDefenseIllusionMana";
            this.lblSpellDefenseIllusionManaLabel.Text = "Illusion - Mana";
            // 
            // lblCounterspellingDiceLabel
            // 
            this.lblCounterspellingDiceLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblCounterspellingDiceLabel.AutoSize = true;
            this.lblCounterspellingDiceLabel.Location = new System.Drawing.Point(62, 6);
            this.lblCounterspellingDiceLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblCounterspellingDiceLabel.Name = "lblCounterspellingDiceLabel";
            this.lblCounterspellingDiceLabel.Size = new System.Drawing.Size(104, 13);
            this.lblCounterspellingDiceLabel.TabIndex = 62;
            this.lblCounterspellingDiceLabel.Tag = "Label_CounterspellingDice";
            this.lblCounterspellingDiceLabel.Text = "Counterspelling Dice";
            // 
            // lblBuildAIAdvancedPrograms
            // 
            this.lblBuildAIAdvancedPrograms.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblBuildAIAdvancedPrograms.AutoSize = true;
            this.lblBuildAIAdvancedPrograms.Location = new System.Drawing.Point(37, 556);
            this.lblBuildAIAdvancedPrograms.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblBuildAIAdvancedPrograms.Name = "lblBuildAIAdvancedPrograms";
            this.lblBuildAIAdvancedPrograms.Size = new System.Drawing.Size(103, 13);
            this.lblBuildAIAdvancedPrograms.TabIndex = 87;
            this.lblBuildAIAdvancedPrograms.Tag = "Label_SummaryAIAdvancedPrograms";
            this.lblBuildAIAdvancedPrograms.Text = "Advanced Programs";
            // 
            // lblBuildRitualsBPLabel
            // 
            this.lblBuildRitualsBPLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblBuildRitualsBPLabel.AutoSize = true;
            this.lblBuildRitualsBPLabel.Location = new System.Drawing.Point(101, 356);
            this.lblBuildRitualsBPLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblBuildRitualsBPLabel.Name = "lblBuildRitualsBPLabel";
            this.lblBuildRitualsBPLabel.Size = new System.Drawing.Size(39, 13);
            this.lblBuildRitualsBPLabel.TabIndex = 133;
            this.lblBuildRitualsBPLabel.Tag = "Label_SummaryRituals";
            this.lblBuildRitualsBPLabel.Text = "Rituals";
            // 
            // lblBuildPrepsBPLabel
            // 
            this.lblBuildPrepsBPLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblBuildPrepsBPLabel.AutoSize = true;
            this.lblBuildPrepsBPLabel.Location = new System.Drawing.Point(74, 331);
            this.lblBuildPrepsBPLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblBuildPrepsBPLabel.Name = "lblBuildPrepsBPLabel";
            this.lblBuildPrepsBPLabel.Size = new System.Drawing.Size(66, 13);
            this.lblBuildPrepsBPLabel.TabIndex = 131;
            this.lblBuildPrepsBPLabel.Tag = "Label_SummaryPreparations";
            this.lblBuildPrepsBPLabel.Text = "Preparations";
            // 
            // lblPublicAware
            // 
            this.lblPublicAware.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblPublicAware.AutoSize = true;
            this.tlpCharacterInfo.SetColumnSpan(this.lblPublicAware, 3);
            this.lblPublicAware.Location = new System.Drawing.Point(797, 135);
            this.lblPublicAware.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblPublicAware.Name = "lblPublicAware";
            this.lblPublicAware.Size = new System.Drawing.Size(94, 13);
            this.lblPublicAware.TabIndex = 86;
            this.lblPublicAware.Tag = "Label_PublicAwareness";
            this.lblPublicAware.Text = "Public Awareness:";
            this.lblPublicAware.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // cmsMartialArts
            // 
            this.cmsMartialArts.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsMartialArtsAddAdvantage,
            this.tsMartialArtsNotes});
            this.cmsMartialArts.Name = "cmsWeapon";
            this.cmsMartialArts.Size = new System.Drawing.Size(154, 48);
            // 
            // tsMartialArtsAddAdvantage
            // 
            this.tsMartialArtsAddAdvantage.Image = global::Chummer.Properties.Resources.medal_gold_add;
            this.tsMartialArtsAddAdvantage.Name = "tsMartialArtsAddAdvantage";
            this.tsMartialArtsAddAdvantage.Size = new System.Drawing.Size(153, 22);
            this.tsMartialArtsAddAdvantage.Tag = "Menu_AddAdvantage";
            this.tsMartialArtsAddAdvantage.Text = "&Add Technique";
            this.tsMartialArtsAddAdvantage.Click += new System.EventHandler(this.tsMartialArtsAddAdvantage_Click);
            // 
            // tsMartialArtsNotes
            // 
            this.tsMartialArtsNotes.Image = global::Chummer.Properties.Resources.note_edit;
            this.tsMartialArtsNotes.Name = "tsMartialArtsNotes";
            this.tsMartialArtsNotes.Size = new System.Drawing.Size(153, 22);
            this.tsMartialArtsNotes.Tag = "Menu_Notes";
            this.tsMartialArtsNotes.Text = "&Notes";
            this.tsMartialArtsNotes.Click += new System.EventHandler(this.tsMartialArtsNotes_Click);
            // 
            // cmsSpellButton
            // 
            this.cmsSpellButton.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsCreateSpell});
            this.cmsSpellButton.Name = "cmsSpellButton";
            this.cmsSpellButton.Size = new System.Drawing.Size(137, 26);
            // 
            // tsCreateSpell
            // 
            this.tsCreateSpell.Name = "tsCreateSpell";
            this.tsCreateSpell.Size = new System.Drawing.Size(136, 22);
            this.tsCreateSpell.Tag = "Menu_CreateSpell";
            this.tsCreateSpell.Text = "&Create Spell";
            this.tsCreateSpell.Visible = false;
            this.tsCreateSpell.Click += new System.EventHandler(this.tsCreateSpell_Click);
            // 
            // cmsComplexForm
            // 
            this.cmsComplexForm.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsAddComplexFormOption,
            this.tsComplexFormNotes});
            this.cmsComplexForm.Name = "cmsComplexForm";
            this.cmsComplexForm.Size = new System.Drawing.Size(137, 48);
            // 
            // tsAddComplexFormOption
            // 
            this.tsAddComplexFormOption.Enabled = false;
            this.tsAddComplexFormOption.Image = global::Chummer.Properties.Resources.plugin_add;
            this.tsAddComplexFormOption.Name = "tsAddComplexFormOption";
            this.tsAddComplexFormOption.Size = new System.Drawing.Size(136, 22);
            this.tsAddComplexFormOption.Tag = "Menu_AddOption";
            this.tsAddComplexFormOption.Text = "&Add Option";
            // 
            // tsComplexFormNotes
            // 
            this.tsComplexFormNotes.Image = global::Chummer.Properties.Resources.note_edit;
            this.tsComplexFormNotes.Name = "tsComplexFormNotes";
            this.tsComplexFormNotes.Size = new System.Drawing.Size(136, 22);
            this.tsComplexFormNotes.Tag = "Menu_Notes";
            this.tsComplexFormNotes.Text = "&Notes";
            this.tsComplexFormNotes.Click += new System.EventHandler(this.tsComplexFormNotes_Click);
            // 
            // cmsCyberware
            // 
            this.cmsCyberware.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsCyberwareAddAsPlugin,
            this.tsCyberwareAddGear,
            this.tsCyberwareNotes});
            this.cmsCyberware.Name = "cmsCyberware";
            this.cmsCyberware.Size = new System.Drawing.Size(148, 70);
            // 
            // tsCyberwareAddAsPlugin
            // 
            this.tsCyberwareAddAsPlugin.Image = global::Chummer.Properties.Resources.brick_add;
            this.tsCyberwareAddAsPlugin.Name = "tsCyberwareAddAsPlugin";
            this.tsCyberwareAddAsPlugin.Size = new System.Drawing.Size(147, 22);
            this.tsCyberwareAddAsPlugin.Tag = "Menu_AddAsPlugin";
            this.tsCyberwareAddAsPlugin.Text = "&Add as Plugin";
            this.tsCyberwareAddAsPlugin.Click += new System.EventHandler(this.tsCyberwareAddAsPlugin_Click);
            // 
            // tsCyberwareAddGear
            // 
            this.tsCyberwareAddGear.Image = global::Chummer.Properties.Resources.camera_add;
            this.tsCyberwareAddGear.Name = "tsCyberwareAddGear";
            this.tsCyberwareAddGear.Size = new System.Drawing.Size(147, 22);
            this.tsCyberwareAddGear.Tag = "Menu_AddGear";
            this.tsCyberwareAddGear.Text = "Add &Gear";
            this.tsCyberwareAddGear.Click += new System.EventHandler(this.tsCyberwareAddGear_Click);
            // 
            // tsCyberwareNotes
            // 
            this.tsCyberwareNotes.Image = global::Chummer.Properties.Resources.note_edit;
            this.tsCyberwareNotes.Name = "tsCyberwareNotes";
            this.tsCyberwareNotes.Size = new System.Drawing.Size(147, 22);
            this.tsCyberwareNotes.Tag = "Menu_Notes";
            this.tsCyberwareNotes.Text = "&Notes";
            this.tsCyberwareNotes.Click += new System.EventHandler(this.tsCyberwareNotes_Click);
            // 
            // cmsVehicleCyberware
            // 
            this.cmsVehicleCyberware.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsVehicleCyberwareAddAsPlugin,
            this.tsVehicleCyberwareAddGear,
            this.tsVehicleCyberwareNotes});
            this.cmsVehicleCyberware.Name = "cmsVehicleCyberware";
            this.cmsVehicleCyberware.Size = new System.Drawing.Size(148, 70);
            // 
            // tsVehicleCyberwareAddAsPlugin
            // 
            this.tsVehicleCyberwareAddAsPlugin.Image = global::Chummer.Properties.Resources.brick_add;
            this.tsVehicleCyberwareAddAsPlugin.Name = "tsVehicleCyberwareAddAsPlugin";
            this.tsVehicleCyberwareAddAsPlugin.Size = new System.Drawing.Size(147, 22);
            this.tsVehicleCyberwareAddAsPlugin.Tag = "Menu_AddAsPlugin";
            this.tsVehicleCyberwareAddAsPlugin.Text = "&Add as Plugin";
            this.tsVehicleCyberwareAddAsPlugin.Click += new System.EventHandler(this.tsVehicleCyberwareAddAsPlugin_Click);
            // 
            // tsVehicleCyberwareAddGear
            // 
            this.tsVehicleCyberwareAddGear.Image = global::Chummer.Properties.Resources.camera_add;
            this.tsVehicleCyberwareAddGear.Name = "tsVehicleCyberwareAddGear";
            this.tsVehicleCyberwareAddGear.Size = new System.Drawing.Size(147, 22);
            this.tsVehicleCyberwareAddGear.Tag = "Menu_AddGear";
            this.tsVehicleCyberwareAddGear.Text = "Add &Gear";
            this.tsVehicleCyberwareAddGear.Click += new System.EventHandler(this.tsVehicleCyberwareAddGear_Click);
            // 
            // tsVehicleCyberwareNotes
            // 
            this.tsVehicleCyberwareNotes.Image = global::Chummer.Properties.Resources.note_edit;
            this.tsVehicleCyberwareNotes.Name = "tsVehicleCyberwareNotes";
            this.tsVehicleCyberwareNotes.Size = new System.Drawing.Size(147, 22);
            this.tsVehicleCyberwareNotes.Tag = "Menu_Notes";
            this.tsVehicleCyberwareNotes.Text = "&Notes";
            this.tsVehicleCyberwareNotes.Click += new System.EventHandler(this.tsVehicleNotes_Click);
            // 
            // cmsLifestyle
            // 
            this.cmsLifestyle.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsAdvancedLifestyle});
            this.cmsLifestyle.Name = "cmsLifestyle";
            this.cmsLifestyle.Size = new System.Drawing.Size(174, 26);
            // 
            // tsAdvancedLifestyle
            // 
            this.tsAdvancedLifestyle.Image = global::Chummer.Properties.Resources.house;
            this.tsAdvancedLifestyle.Name = "tsAdvancedLifestyle";
            this.tsAdvancedLifestyle.Size = new System.Drawing.Size(173, 22);
            this.tsAdvancedLifestyle.Tag = "Menu_AdvancedLifestyle";
            this.tsAdvancedLifestyle.Text = "&Advanced Lifestyle";
            this.tsAdvancedLifestyle.Click += new System.EventHandler(this.tsAdvancedLifestyle_Click);
            // 
            // cmsArmor
            // 
            this.cmsArmor.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsAddArmorMod,
            this.tsAddArmorGear,
            this.tsArmorName,
            this.tsArmorNotes});
            this.cmsArmor.Name = "cmsWeapon";
            this.cmsArmor.Size = new System.Drawing.Size(162, 92);
            // 
            // tsAddArmorMod
            // 
            this.tsAddArmorMod.Image = global::Chummer.Properties.Resources.brick_add;
            this.tsAddArmorMod.Name = "tsAddArmorMod";
            this.tsAddArmorMod.Size = new System.Drawing.Size(161, 22);
            this.tsAddArmorMod.Tag = "Menu_AddArmorMod";
            this.tsAddArmorMod.Text = "&Add Armor Mod";
            this.tsAddArmorMod.Click += new System.EventHandler(this.tsAddArmorMod_Click);
            // 
            // tsAddArmorGear
            // 
            this.tsAddArmorGear.Image = global::Chummer.Properties.Resources.camera_add;
            this.tsAddArmorGear.Name = "tsAddArmorGear";
            this.tsAddArmorGear.Size = new System.Drawing.Size(161, 22);
            this.tsAddArmorGear.Tag = "Menu_AddGear";
            this.tsAddArmorGear.Text = "A&dd Gear";
            this.tsAddArmorGear.Click += new System.EventHandler(this.tsAddArmorGear_Click);
            // 
            // tsArmorName
            // 
            this.tsArmorName.Image = global::Chummer.Properties.Resources.tag_red;
            this.tsArmorName.Name = "tsArmorName";
            this.tsArmorName.Size = new System.Drawing.Size(161, 22);
            this.tsArmorName.Tag = "Menu_NameArmor";
            this.tsArmorName.Text = "Name Armor";
            this.tsArmorName.Click += new System.EventHandler(this.tsArmorName_Click);
            // 
            // tsArmorNotes
            // 
            this.tsArmorNotes.Image = global::Chummer.Properties.Resources.note_edit;
            this.tsArmorNotes.Name = "tsArmorNotes";
            this.tsArmorNotes.Size = new System.Drawing.Size(161, 22);
            this.tsArmorNotes.Tag = "Menu_Notes";
            this.tsArmorNotes.Text = "&Notes";
            this.tsArmorNotes.Click += new System.EventHandler(this.tsArmorNotes_Click);
            // 
            // cmsWeapon
            // 
            this.cmsWeapon.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsCreateNaturalWeapon,
            this.tsWeaponAddAccessory,
            this.tsWeaponAddUnderbarrel,
            this.tsWeaponName,
            this.tsWeaponNotes});
            this.cmsWeapon.Name = "cmsWeapon";
            this.cmsWeapon.Size = new System.Drawing.Size(209, 114);
            // 
            // tsCreateNaturalWeapon
            // 
            this.tsCreateNaturalWeapon.Image = global::Chummer.Properties.Resources.bomb;
            this.tsCreateNaturalWeapon.Name = "tsCreateNaturalWeapon";
            this.tsCreateNaturalWeapon.Size = new System.Drawing.Size(208, 22);
            this.tsCreateNaturalWeapon.Tag = "Menu_AddNaturalWeapon";
            this.tsCreateNaturalWeapon.Text = "Create Natural Weapon";
            this.tsCreateNaturalWeapon.Click += new System.EventHandler(this.tsCreateNaturalWeapon_Click);
            // 
            // tsWeaponAddAccessory
            // 
            this.tsWeaponAddAccessory.Image = global::Chummer.Properties.Resources.brick_add;
            this.tsWeaponAddAccessory.Name = "tsWeaponAddAccessory";
            this.tsWeaponAddAccessory.Size = new System.Drawing.Size(208, 22);
            this.tsWeaponAddAccessory.Tag = "Menu_AddAccessory";
            this.tsWeaponAddAccessory.Text = "&Add Accessory";
            this.tsWeaponAddAccessory.Click += new System.EventHandler(this.tsWeaponAddAccessory_Click);
            // 
            // tsWeaponAddUnderbarrel
            // 
            this.tsWeaponAddUnderbarrel.Image = global::Chummer.Properties.Resources.award_star2_add;
            this.tsWeaponAddUnderbarrel.Name = "tsWeaponAddUnderbarrel";
            this.tsWeaponAddUnderbarrel.Size = new System.Drawing.Size(208, 22);
            this.tsWeaponAddUnderbarrel.Tag = "Menu_AddUnderbarrelWeapon";
            this.tsWeaponAddUnderbarrel.Text = "Add Underbarrel Weapon";
            this.tsWeaponAddUnderbarrel.Click += new System.EventHandler(this.tsWeaponAddUnderbarrel_Click);
            // 
            // tsWeaponName
            // 
            this.tsWeaponName.Image = global::Chummer.Properties.Resources.tag_red;
            this.tsWeaponName.Name = "tsWeaponName";
            this.tsWeaponName.Size = new System.Drawing.Size(208, 22);
            this.tsWeaponName.Tag = "Menu_NameWeapon";
            this.tsWeaponName.Text = "Name &Weapon";
            this.tsWeaponName.Click += new System.EventHandler(this.tsWeaponName_Click);
            // 
            // tsWeaponNotes
            // 
            this.tsWeaponNotes.Image = global::Chummer.Properties.Resources.note_edit;
            this.tsWeaponNotes.Name = "tsWeaponNotes";
            this.tsWeaponNotes.Size = new System.Drawing.Size(208, 22);
            this.tsWeaponNotes.Tag = "Menu_Notes";
            this.tsWeaponNotes.Text = "&Notes";
            this.tsWeaponNotes.Click += new System.EventHandler(this.tsWeaponNotes_Click);
            // 
            // tsWeaponMountLocation
            // 
            this.tsWeaponMountLocation.Image = global::Chummer.Properties.Resources.tag_red;
            this.tsWeaponMountLocation.Name = "tsWeaponMountLocation";
            this.tsWeaponMountLocation.Size = new System.Drawing.Size(180, 22);
            this.tsWeaponMountLocation.Tag = "Menu_RenameLocation";
            this.tsWeaponMountLocation.Text = "Rename &Location";
            this.tsWeaponMountLocation.Click += new System.EventHandler(this.tsWeaponMountLocation_Click);
            // 
            // lmtControl
            // 
            this.lmtControl.AutoSize = true;
            this.lmtControl.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.lmtControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lmtControl.Location = new System.Drawing.Point(3, 3);
            this.lmtControl.MinimumSize = new System.Drawing.Size(480, 0);
            this.lmtControl.Name = "lmtControl";
            this.lmtControl.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lmtControl.Size = new System.Drawing.Size(971, 625);
            this.lmtControl.TabIndex = 0;
            // 
            // tsWeaponAddModification
            // 
            this.tsWeaponAddModification.Name = "tsWeaponAddModification";
            this.tsWeaponAddModification.Size = new System.Drawing.Size(32, 19);
            // 
            // cmsGearButton
            // 
            this.cmsGearButton.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsGearButtonAddAccessory});
            this.cmsGearButton.Name = "cmsGearButton";
            this.cmsGearButton.Size = new System.Drawing.Size(153, 26);
            // 
            // tsGearButtonAddAccessory
            // 
            this.tsGearButtonAddAccessory.Image = global::Chummer.Properties.Resources.brick_add;
            this.tsGearButtonAddAccessory.Name = "tsGearButtonAddAccessory";
            this.tsGearButtonAddAccessory.Size = new System.Drawing.Size(152, 22);
            this.tsGearButtonAddAccessory.Tag = "Menu_AddAccessory";
            this.tsGearButtonAddAccessory.Text = "&Add Accessory";
            this.tsGearButtonAddAccessory.Click += new System.EventHandler(this.tsGearButtonAddAccessory_Click);
            // 
            // cmsVehicle
            // 
            this.cmsVehicle.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsVehicleAddWeapon,
            this.tsVehicleAddWeaponMount,
            this.tsVehicleAddMod,
            this.tsVehicleAddCyberware,
            this.tsVehicleAddSensor,
            this.tsVehicleName,
            this.tsVehicleNotes});
            this.cmsVehicle.Name = "cmsWeapon";
            this.cmsVehicle.Size = new System.Drawing.Size(193, 158);
            // 
            // tsVehicleAddWeapon
            // 
            this.tsVehicleAddWeapon.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsVehicleAddWeaponWeapon,
            this.tsVehicleAddWeaponAccessory,
            this.tsVehicleAddUnderbarrelWeapon});
            this.tsVehicleAddWeapon.Image = global::Chummer.Properties.Resources.award_star_add;
            this.tsVehicleAddWeapon.Name = "tsVehicleAddWeapon";
            this.tsVehicleAddWeapon.Size = new System.Drawing.Size(192, 22);
            this.tsVehicleAddWeapon.Tag = "Menu_Weapons";
            this.tsVehicleAddWeapon.Text = "&Weapons";
            // 
            // tsVehicleAddWeaponWeapon
            // 
            this.tsVehicleAddWeaponWeapon.Image = global::Chummer.Properties.Resources.award_star_add;
            this.tsVehicleAddWeaponWeapon.Name = "tsVehicleAddWeaponWeapon";
            this.tsVehicleAddWeaponWeapon.Size = new System.Drawing.Size(208, 22);
            this.tsVehicleAddWeaponWeapon.Tag = "Menu_AddWeapon";
            this.tsVehicleAddWeaponWeapon.Text = "Add &Weapon";
            this.tsVehicleAddWeaponWeapon.Click += new System.EventHandler(this.tsVehicleAddWeaponWeapon_Click);
            // 
            // tsVehicleAddWeaponAccessory
            // 
            this.tsVehicleAddWeaponAccessory.Image = global::Chummer.Properties.Resources.brick_add;
            this.tsVehicleAddWeaponAccessory.Name = "tsVehicleAddWeaponAccessory";
            this.tsVehicleAddWeaponAccessory.Size = new System.Drawing.Size(208, 22);
            this.tsVehicleAddWeaponAccessory.Tag = "Menu_AddAccessory";
            this.tsVehicleAddWeaponAccessory.Text = "Add &Accessory";
            this.tsVehicleAddWeaponAccessory.Click += new System.EventHandler(this.tsVehicleAddWeaponAccessory_Click);
            // 
            // tsVehicleAddUnderbarrelWeapon
            // 
            this.tsVehicleAddUnderbarrelWeapon.Image = global::Chummer.Properties.Resources.award_star2_add;
            this.tsVehicleAddUnderbarrelWeapon.Name = "tsVehicleAddUnderbarrelWeapon";
            this.tsVehicleAddUnderbarrelWeapon.Size = new System.Drawing.Size(208, 22);
            this.tsVehicleAddUnderbarrelWeapon.Tag = "Menu_AddUnderbarrelWeapon";
            this.tsVehicleAddUnderbarrelWeapon.Text = "Add Underbarrel Weapon";
            this.tsVehicleAddUnderbarrelWeapon.Click += new System.EventHandler(this.tsVehicleAddUnderbarrelWeapon_Click);
            // 
            // tsVehicleAddWeaponMount
            // 
            this.tsVehicleAddWeaponMount.Image = global::Chummer.Properties.Resources.car_add;
            this.tsVehicleAddWeaponMount.Name = "tsVehicleAddWeaponMount";
            this.tsVehicleAddWeaponMount.Size = new System.Drawing.Size(192, 22);
            this.tsVehicleAddWeaponMount.Tag = "Menu_AddWeaponMount";
            this.tsVehicleAddWeaponMount.Text = "Add Weapon Mount";
            this.tsVehicleAddWeaponMount.Click += new System.EventHandler(this.tsVehicleAddWeaponMount_Click);
            // 
            // tsVehicleAddMod
            // 
            this.tsVehicleAddMod.Image = global::Chummer.Properties.Resources.car_add;
            this.tsVehicleAddMod.Name = "tsVehicleAddMod";
            this.tsVehicleAddMod.Size = new System.Drawing.Size(192, 22);
            this.tsVehicleAddMod.Tag = "Menu_AddModification";
            this.tsVehicleAddMod.Text = "Add &Modification";
            this.tsVehicleAddMod.Click += new System.EventHandler(this.tsVehicleAddMod_Click);
            // 
            // tsVehicleAddCyberware
            // 
            this.tsVehicleAddCyberware.Image = global::Chummer.Properties.Resources.brick_add;
            this.tsVehicleAddCyberware.Name = "tsVehicleAddCyberware";
            this.tsVehicleAddCyberware.Size = new System.Drawing.Size(192, 22);
            this.tsVehicleAddCyberware.Tag = "Menu_AddCyberwarePlugin";
            this.tsVehicleAddCyberware.Text = "Add Cyberware Plugin";
            this.tsVehicleAddCyberware.Click += new System.EventHandler(this.tsVehicleAddCyberware_Click);
            // 
            // tsVehicleAddSensor
            // 
            this.tsVehicleAddSensor.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsVehicleAddGear,
            this.tsVehicleSensorAddAsPlugin});
            this.tsVehicleAddSensor.Image = global::Chummer.Properties.Resources.camera_add;
            this.tsVehicleAddSensor.Name = "tsVehicleAddSensor";
            this.tsVehicleAddSensor.Size = new System.Drawing.Size(192, 22);
            this.tsVehicleAddSensor.Tag = "Menu_Gear";
            this.tsVehicleAddSensor.Text = "&Gear";
            // 
            // tsVehicleAddGear
            // 
            this.tsVehicleAddGear.Image = global::Chummer.Properties.Resources.camera_add;
            this.tsVehicleAddGear.Name = "tsVehicleAddGear";
            this.tsVehicleAddGear.Size = new System.Drawing.Size(147, 22);
            this.tsVehicleAddGear.Tag = "Menu_AddGear";
            this.tsVehicleAddGear.Text = "Add &Gear";
            this.tsVehicleAddGear.Click += new System.EventHandler(this.tsVehicleAddGear_Click);
            // 
            // tsVehicleSensorAddAsPlugin
            // 
            this.tsVehicleSensorAddAsPlugin.Image = global::Chummer.Properties.Resources.brick_add;
            this.tsVehicleSensorAddAsPlugin.Name = "tsVehicleSensorAddAsPlugin";
            this.tsVehicleSensorAddAsPlugin.Size = new System.Drawing.Size(147, 22);
            this.tsVehicleSensorAddAsPlugin.Tag = "Menu_AddAsPlugin";
            this.tsVehicleSensorAddAsPlugin.Text = "&Add as Plugin";
            this.tsVehicleSensorAddAsPlugin.Click += new System.EventHandler(this.tsVehicleSensorAddAsPlugin_Click);
            // 
            // tsVehicleName
            // 
            this.tsVehicleName.Image = global::Chummer.Properties.Resources.tag_red;
            this.tsVehicleName.Name = "tsVehicleName";
            this.tsVehicleName.Size = new System.Drawing.Size(192, 22);
            this.tsVehicleName.Tag = "Menu_NameVehicle";
            this.tsVehicleName.Text = "Name Vehicle";
            this.tsVehicleName.Click += new System.EventHandler(this.tsVehicleName_Click);
            // 
            // tsVehicleNotes
            // 
            this.tsVehicleNotes.Image = global::Chummer.Properties.Resources.note_edit;
            this.tsVehicleNotes.Name = "tsVehicleNotes";
            this.tsVehicleNotes.Size = new System.Drawing.Size(192, 22);
            this.tsVehicleNotes.Tag = "Menu_Notes";
            this.tsVehicleNotes.Text = "&Notes";
            this.tsVehicleNotes.Click += new System.EventHandler(this.tsVehicleNotes_Click);
            // 
            // tsVehicleWeaponMountNotes
            // 
            this.tsVehicleWeaponMountNotes.Image = global::Chummer.Properties.Resources.note_edit;
            this.tsVehicleWeaponMountNotes.Name = "tsVehicleWeaponMountNotes";
            this.tsVehicleWeaponMountNotes.Size = new System.Drawing.Size(180, 22);
            this.tsVehicleWeaponMountNotes.Tag = "Menu_Notes";
            this.tsVehicleWeaponMountNotes.Text = "&Notes";
            this.tsVehicleWeaponMountNotes.Click += new System.EventHandler(this.tsVehicleNotes_Click);
            // 
            // cmsWeaponMount
            // 
            this.cmsWeaponMount.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsWeaponMountLocation,
            this.tsVehicleMountWeapon,
            this.tsVehicleWeaponMountNotes,
            this.tsEditWeaponMount});
            this.cmsWeaponMount.Name = "cmsWeaponMount";
            this.cmsWeaponMount.Size = new System.Drawing.Size(181, 92);
            // 
            // tsVehicleMountWeapon
            // 
            this.tsVehicleMountWeapon.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsVehicleMountWeaponAdd,
            this.tsVehicleMountWeaponAccessory,
            this.tsVehicleMountWeaponUnderbarrel});
            this.tsVehicleMountWeapon.Image = global::Chummer.Properties.Resources.award_star_add;
            this.tsVehicleMountWeapon.Name = "tsVehicleMountWeapon";
            this.tsVehicleMountWeapon.Size = new System.Drawing.Size(180, 22);
            this.tsVehicleMountWeapon.Tag = "Menu_Weapons";
            this.tsVehicleMountWeapon.Text = "&Weapons";
            // 
            // tsVehicleMountWeaponAdd
            // 
            this.tsVehicleMountWeaponAdd.Image = global::Chummer.Properties.Resources.award_star_add;
            this.tsVehicleMountWeaponAdd.Name = "tsVehicleMountWeaponAdd";
            this.tsVehicleMountWeaponAdd.Size = new System.Drawing.Size(208, 22);
            this.tsVehicleMountWeaponAdd.Tag = "Menu_AddWeapon";
            this.tsVehicleMountWeaponAdd.Text = "Add &Weapon";
            this.tsVehicleMountWeaponAdd.Click += new System.EventHandler(this.tsVehicleAddWeaponWeapon_Click);
            // 
            // tsVehicleMountWeaponAccessory
            // 
            this.tsVehicleMountWeaponAccessory.Image = global::Chummer.Properties.Resources.brick_add;
            this.tsVehicleMountWeaponAccessory.Name = "tsVehicleMountWeaponAccessory";
            this.tsVehicleMountWeaponAccessory.Size = new System.Drawing.Size(208, 22);
            this.tsVehicleMountWeaponAccessory.Tag = "Menu_AddAccessory";
            this.tsVehicleMountWeaponAccessory.Text = "Add &Accessory";
            this.tsVehicleMountWeaponAccessory.Click += new System.EventHandler(this.tsVehicleAddWeaponAccessory_Click);
            // 
            // tsVehicleMountWeaponUnderbarrel
            // 
            this.tsVehicleMountWeaponUnderbarrel.Image = global::Chummer.Properties.Resources.award_star2_add;
            this.tsVehicleMountWeaponUnderbarrel.Name = "tsVehicleMountWeaponUnderbarrel";
            this.tsVehicleMountWeaponUnderbarrel.Size = new System.Drawing.Size(208, 22);
            this.tsVehicleMountWeaponUnderbarrel.Tag = "Menu_AddUnderbarrelWeapon";
            this.tsVehicleMountWeaponUnderbarrel.Text = "Add Underbarrel Weapon";
            this.tsVehicleMountWeaponUnderbarrel.Click += new System.EventHandler(this.tsVehicleAddUnderbarrelWeapon_Click);
            // 
            // tsEditWeaponMount
            // 
            this.tsEditWeaponMount.Image = global::Chummer.Properties.Resources.cog_edit;
            this.tsEditWeaponMount.Name = "tsEditWeaponMount";
            this.tsEditWeaponMount.Size = new System.Drawing.Size(180, 22);
            this.tsEditWeaponMount.Tag = "Menu_EditWeaponMount";
            this.tsEditWeaponMount.Text = "&Edit Weapon Mount";
            this.tsEditWeaponMount.Click += new System.EventHandler(this.tsEditWeaponMount_Click);
            // 
            // mnuCreateMenu
            // 
            this.mnuCreateMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuCreateFile,
            this.mnuCreateEdit,
            this.mnuCreateSpecial});
            this.mnuCreateMenu.Location = new System.Drawing.Point(0, 0);
            this.mnuCreateMenu.Name = "mnuCreateMenu";
            this.mnuCreateMenu.Size = new System.Drawing.Size(1085, 24);
            this.mnuCreateMenu.TabIndex = 51;
            this.mnuCreateMenu.Text = "Top Level Menu";
            this.mnuCreateMenu.Visible = false;
            // 
            // mnuCreateFile
            // 
            this.mnuCreateFile.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuFileSave,
            this.mnuFileSaveAs,
            this.mnuFileSaveAsCreated,
            this.tssFileMenu1,
            this.mnuFileClose,
            this.tssFileMenu2,
            this.mnuFilePrint});
            this.mnuCreateFile.MergeAction = System.Windows.Forms.MergeAction.MatchOnly;
            this.mnuCreateFile.Name = "mnuCreateFile";
            this.mnuCreateFile.Size = new System.Drawing.Size(37, 20);
            this.mnuCreateFile.Tag = "Menu_Main_File";
            this.mnuCreateFile.Text = "&File";
            // 
            // mnuFileSave
            // 
            this.mnuFileSave.Image = global::Chummer.Properties.Resources.disk;
            this.mnuFileSave.ImageTransparentColor = System.Drawing.Color.Black;
            this.mnuFileSave.MergeAction = System.Windows.Forms.MergeAction.Insert;
            this.mnuFileSave.MergeIndex = 3;
            this.mnuFileSave.Name = "mnuFileSave";
            this.mnuFileSave.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
            this.mnuFileSave.Size = new System.Drawing.Size(158, 22);
            this.mnuFileSave.Tag = "Menu_FileSave";
            this.mnuFileSave.Text = "&Save";
            this.mnuFileSave.Click += new System.EventHandler(this.mnuFileSave_Click);
            // 
            // mnuFileSaveAs
            // 
            this.mnuFileSaveAs.Image = global::Chummer.Properties.Resources.disk;
            this.mnuFileSaveAs.MergeAction = System.Windows.Forms.MergeAction.Insert;
            this.mnuFileSaveAs.MergeIndex = 4;
            this.mnuFileSaveAs.Name = "mnuFileSaveAs";
            this.mnuFileSaveAs.Size = new System.Drawing.Size(158, 22);
            this.mnuFileSaveAs.Tag = "Menu_FileSaveAs";
            this.mnuFileSaveAs.Text = "Save &As";
            this.mnuFileSaveAs.Click += new System.EventHandler(this.mnuFileSaveAs_Click);
            // 
            // mnuFileSaveAsCreated
            // 
            this.mnuFileSaveAsCreated.Image = global::Chummer.Properties.Resources.accept;
            this.mnuFileSaveAsCreated.MergeAction = System.Windows.Forms.MergeAction.Insert;
            this.mnuFileSaveAsCreated.MergeIndex = 5;
            this.mnuFileSaveAsCreated.Name = "mnuFileSaveAsCreated";
            this.mnuFileSaveAsCreated.Size = new System.Drawing.Size(158, 22);
            this.mnuFileSaveAsCreated.Tag = "Menu_FileSaveAsCreated";
            this.mnuFileSaveAsCreated.Text = "Save As &Created";
            this.mnuFileSaveAsCreated.Click += new System.EventHandler(this.mnuFileSaveAsCreated_Click);
            // 
            // tssFileMenu1
            // 
            this.tssFileMenu1.MergeAction = System.Windows.Forms.MergeAction.Insert;
            this.tssFileMenu1.MergeIndex = 6;
            this.tssFileMenu1.Name = "tssFileMenu1";
            this.tssFileMenu1.Size = new System.Drawing.Size(155, 6);
            // 
            // mnuFileClose
            // 
            this.mnuFileClose.Image = global::Chummer.Properties.Resources.cancel;
            this.mnuFileClose.MergeAction = System.Windows.Forms.MergeAction.Insert;
            this.mnuFileClose.MergeIndex = 7;
            this.mnuFileClose.Name = "mnuFileClose";
            this.mnuFileClose.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.W)));
            this.mnuFileClose.Size = new System.Drawing.Size(158, 22);
            this.mnuFileClose.Tag = "Menu_FileClose";
            this.mnuFileClose.Text = "&Close";
            this.mnuFileClose.Click += new System.EventHandler(this.mnuFileClose_Click);
            // 
            // tssFileMenu2
            // 
            this.tssFileMenu2.MergeIndex = 8;
            this.tssFileMenu2.Name = "tssFileMenu2";
            this.tssFileMenu2.Size = new System.Drawing.Size(155, 6);
            // 
            // mnuFilePrint
            // 
            this.mnuFilePrint.Image = global::Chummer.Properties.Resources.printer;
            this.mnuFilePrint.MergeAction = System.Windows.Forms.MergeAction.Insert;
            this.mnuFilePrint.MergeIndex = 9;
            this.mnuFilePrint.Name = "mnuFilePrint";
            this.mnuFilePrint.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.P)));
            this.mnuFilePrint.Size = new System.Drawing.Size(158, 22);
            this.mnuFilePrint.Tag = "Menu_FilePrint";
            this.mnuFilePrint.Text = "&Print";
            this.mnuFilePrint.Click += new System.EventHandler(this.mnuFilePrint_Click);
            // 
            // mnuCreateEdit
            // 
            this.mnuCreateEdit.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuEditCopy,
            this.mnuEditPaste});
            this.mnuCreateEdit.MergeAction = System.Windows.Forms.MergeAction.Insert;
            this.mnuCreateEdit.MergeIndex = 1;
            this.mnuCreateEdit.Name = "mnuCreateEdit";
            this.mnuCreateEdit.Size = new System.Drawing.Size(39, 20);
            this.mnuCreateEdit.Tag = "Menu_Main_Edit";
            this.mnuCreateEdit.Text = "&Edit";
            // 
            // mnuEditCopy
            // 
            this.mnuEditCopy.Image = global::Chummer.Properties.Resources.page_copy;
            this.mnuEditCopy.Name = "mnuEditCopy";
            this.mnuEditCopy.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.C)));
            this.mnuEditCopy.Size = new System.Drawing.Size(144, 22);
            this.mnuEditCopy.Tag = "Menu_EditCopy";
            this.mnuEditCopy.Text = "&Copy";
            this.mnuEditCopy.Click += new System.EventHandler(this.mnuEditCopy_Click);
            // 
            // mnuEditPaste
            // 
            this.mnuEditPaste.Image = global::Chummer.Properties.Resources.page_paste;
            this.mnuEditPaste.Name = "mnuEditPaste";
            this.mnuEditPaste.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.V)));
            this.mnuEditPaste.Size = new System.Drawing.Size(144, 22);
            this.mnuEditPaste.Tag = "Menu_EditPaste";
            this.mnuEditPaste.Text = "&Paste";
            this.mnuEditPaste.Click += new System.EventHandler(this.mnuEditPaste_Click);
            // 
            // mnuCreateSpecial
            // 
            this.mnuCreateSpecial.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuSpecialAddCyberwareSuite,
            this.mnuSpecialAddBiowareSuite,
            this.mnuSpecialCreateCyberwareSuite,
            this.mnuSpecialCreateBiowareSuite,
            this.mnuSpecialAddPACKSKit,
            this.mnuSpecialCreatePACKSKit,
            this.mnuSpecialChangeMetatype,
            this.mnuSpecialChangeOptions,
            this.mnuSpecialMutantCritter,
            this.mnuSpecialToxicCritter,
            this.mnuSpecialCyberzombie,
            this.mnuSpecialConvertToFreeSprite,
            this.mnuSpecialReapplyImprovements,
            this.mnuSpecialBPAvailLimit,
            this.mnuSpecialConfirmValidity,
            this.mnuSpecialKarmaValue});
            this.mnuCreateSpecial.MergeAction = System.Windows.Forms.MergeAction.Insert;
            this.mnuCreateSpecial.MergeIndex = 3;
            this.mnuCreateSpecial.Name = "mnuCreateSpecial";
            this.mnuCreateSpecial.Size = new System.Drawing.Size(56, 20);
            this.mnuCreateSpecial.Tag = "Menu_Special";
            this.mnuCreateSpecial.Text = "&Special";
            // 
            // mnuSpecialAddCyberwareSuite
            // 
            this.mnuSpecialAddCyberwareSuite.Image = global::Chummer.Properties.Resources.briefcase_add;
            this.mnuSpecialAddCyberwareSuite.Name = "mnuSpecialAddCyberwareSuite";
            this.mnuSpecialAddCyberwareSuite.Size = new System.Drawing.Size(213, 22);
            this.mnuSpecialAddCyberwareSuite.Tag = "Menu_SpecialAddCyberwareSuite";
            this.mnuSpecialAddCyberwareSuite.Text = "Add &Cyberware Suite";
            this.mnuSpecialAddCyberwareSuite.Click += new System.EventHandler(this.mnuSpecialAddCyberwareSuite_Click);
            // 
            // mnuSpecialAddBiowareSuite
            // 
            this.mnuSpecialAddBiowareSuite.Image = global::Chummer.Properties.Resources.briefcase_add;
            this.mnuSpecialAddBiowareSuite.Name = "mnuSpecialAddBiowareSuite";
            this.mnuSpecialAddBiowareSuite.Size = new System.Drawing.Size(213, 22);
            this.mnuSpecialAddBiowareSuite.Tag = "Menu_SpecialAddBiowareSuite";
            this.mnuSpecialAddBiowareSuite.Text = "Add &Bioware Suite";
            this.mnuSpecialAddBiowareSuite.Click += new System.EventHandler(this.mnuSpecialAddBiowareSuite_Click);
            // 
            // mnuSpecialCreateCyberwareSuite
            // 
            this.mnuSpecialCreateCyberwareSuite.Image = global::Chummer.Properties.Resources.briefcase_edit;
            this.mnuSpecialCreateCyberwareSuite.Name = "mnuSpecialCreateCyberwareSuite";
            this.mnuSpecialCreateCyberwareSuite.Size = new System.Drawing.Size(213, 22);
            this.mnuSpecialCreateCyberwareSuite.Tag = "Menu_SpecialCreateCyberwareSuite";
            this.mnuSpecialCreateCyberwareSuite.Text = "Create Cyberware Suite";
            this.mnuSpecialCreateCyberwareSuite.Click += new System.EventHandler(this.mnuSpecialCreateCyberwareSuite_Click);
            // 
            // mnuSpecialCreateBiowareSuite
            // 
            this.mnuSpecialCreateBiowareSuite.Image = global::Chummer.Properties.Resources.briefcase_edit;
            this.mnuSpecialCreateBiowareSuite.Name = "mnuSpecialCreateBiowareSuite";
            this.mnuSpecialCreateBiowareSuite.Size = new System.Drawing.Size(213, 22);
            this.mnuSpecialCreateBiowareSuite.Tag = "Menu_SpecialCreateBiowareSuite";
            this.mnuSpecialCreateBiowareSuite.Text = "Create Bioware Suite";
            this.mnuSpecialCreateBiowareSuite.Click += new System.EventHandler(this.mnuSpecialCreateBiowareSuite_Click);
            // 
            // mnuSpecialAddPACKSKit
            // 
            this.mnuSpecialAddPACKSKit.Image = global::Chummer.Properties.Resources.basket_add;
            this.mnuSpecialAddPACKSKit.Name = "mnuSpecialAddPACKSKit";
            this.mnuSpecialAddPACKSKit.Size = new System.Drawing.Size(213, 22);
            this.mnuSpecialAddPACKSKit.Tag = "Menu_SpecialAddPACKSKit";
            this.mnuSpecialAddPACKSKit.Text = "Add &PACKS Kit";
            this.mnuSpecialAddPACKSKit.Click += new System.EventHandler(this.mnuSpecialAddPACKSKit_Click);
            // 
            // mnuSpecialCreatePACKSKit
            // 
            this.mnuSpecialCreatePACKSKit.Image = global::Chummer.Properties.Resources.basket_edit;
            this.mnuSpecialCreatePACKSKit.Name = "mnuSpecialCreatePACKSKit";
            this.mnuSpecialCreatePACKSKit.Size = new System.Drawing.Size(213, 22);
            this.mnuSpecialCreatePACKSKit.Tag = "Menu_SpecialCreatePACKSKit";
            this.mnuSpecialCreatePACKSKit.Text = "Create PACKS Kit";
            this.mnuSpecialCreatePACKSKit.Click += new System.EventHandler(this.mnuSpecialCreatePACKSKit_Click);
            // 
            // mnuSpecialChangeMetatype
            // 
            this.mnuSpecialChangeMetatype.Image = global::Chummer.Properties.Resources.user_go;
            this.mnuSpecialChangeMetatype.Name = "mnuSpecialChangeMetatype";
            this.mnuSpecialChangeMetatype.Size = new System.Drawing.Size(213, 22);
            this.mnuSpecialChangeMetatype.Tag = "Menu_SpecialChangeMetatype";
            this.mnuSpecialChangeMetatype.Text = "Change &Metatype";
            this.mnuSpecialChangeMetatype.Click += new System.EventHandler(this.mnuSpecialChangeMetatype_Click);
            // 
            // mnuSpecialChangeOptions
            // 
            this.mnuSpecialChangeOptions.Image = global::Chummer.Properties.Resources.user_go;
            this.mnuSpecialChangeOptions.Name = "mnuSpecialChangeOptions";
            this.mnuSpecialChangeOptions.Size = new System.Drawing.Size(213, 22);
            this.mnuSpecialChangeOptions.Tag = "Menu_SpecialChangeOptions";
            this.mnuSpecialChangeOptions.Text = "Change &Options File";
            this.mnuSpecialChangeOptions.Click += new System.EventHandler(this.mnuSpecialChangeOptions_Click);
            // 
            // mnuSpecialMutantCritter
            // 
            this.mnuSpecialMutantCritter.Name = "mnuSpecialMutantCritter";
            this.mnuSpecialMutantCritter.Size = new System.Drawing.Size(213, 22);
            // 
            // mnuSpecialToxicCritter
            // 
            this.mnuSpecialToxicCritter.Name = "mnuSpecialToxicCritter";
            this.mnuSpecialToxicCritter.Size = new System.Drawing.Size(213, 22);
            // 
            // mnuSpecialCyberzombie
            // 
            this.mnuSpecialCyberzombie.Image = global::Chummer.Properties.Resources.transmit_go;
            this.mnuSpecialCyberzombie.Name = "mnuSpecialCyberzombie";
            this.mnuSpecialCyberzombie.Size = new System.Drawing.Size(213, 22);
            this.mnuSpecialCyberzombie.Tag = "Menu_SpecialConvertToCyberzombie";
            this.mnuSpecialCyberzombie.Text = "Convert to Cyberzombie";
            this.mnuSpecialCyberzombie.Click += new System.EventHandler(this.mnuSpecialCyberzombie_Click);
            // 
            // mnuSpecialConvertToFreeSprite
            // 
            this.mnuSpecialConvertToFreeSprite.Image = global::Chummer.Properties.Resources.emoticon_waii;
            this.mnuSpecialConvertToFreeSprite.Name = "mnuSpecialConvertToFreeSprite";
            this.mnuSpecialConvertToFreeSprite.Size = new System.Drawing.Size(213, 22);
            this.mnuSpecialConvertToFreeSprite.Tag = "Menu_SpecialConvertToFreeSprite";
            this.mnuSpecialConvertToFreeSprite.Text = "Convert to Free Sprite";
            this.mnuSpecialConvertToFreeSprite.Visible = false;
            this.mnuSpecialConvertToFreeSprite.Click += new System.EventHandler(this.mnuSpecialConvertToFreeSprite_Click);
            // 
            // mnuSpecialReapplyImprovements
            // 
            this.mnuSpecialReapplyImprovements.Image = global::Chummer.Properties.Resources.page_refresh;
            this.mnuSpecialReapplyImprovements.Name = "mnuSpecialReapplyImprovements";
            this.mnuSpecialReapplyImprovements.Size = new System.Drawing.Size(213, 22);
            this.mnuSpecialReapplyImprovements.Tag = "Menu_SpecialReapplyImprovements";
            this.mnuSpecialReapplyImprovements.Text = "Re-apply Improvements";
            this.mnuSpecialReapplyImprovements.Click += new System.EventHandler(this.mnuSpecialReapplyImprovements_Click);
            // 
            // mnuSpecialBPAvailLimit
            // 
            this.mnuSpecialBPAvailLimit.Image = global::Chummer.Properties.Resources.table_edit;
            this.mnuSpecialBPAvailLimit.Name = "mnuSpecialBPAvailLimit";
            this.mnuSpecialBPAvailLimit.Size = new System.Drawing.Size(213, 22);
            this.mnuSpecialBPAvailLimit.Tag = "Menu_SpecialBPAvailLimit";
            this.mnuSpecialBPAvailLimit.Text = "Change BP/Avail Limit";
            this.mnuSpecialBPAvailLimit.Visible = false;
            this.mnuSpecialBPAvailLimit.Click += new System.EventHandler(this.mnuSpecialBPAvailLimit_Click);
            // 
            // mnuSpecialConfirmValidity
            // 
            this.mnuSpecialConfirmValidity.Image = global::Chummer.Properties.Resources.accept;
            this.mnuSpecialConfirmValidity.Name = "mnuSpecialConfirmValidity";
            this.mnuSpecialConfirmValidity.Size = new System.Drawing.Size(213, 22);
            this.mnuSpecialConfirmValidity.Tag = "Menu_ValidCharacter";
            this.mnuSpecialConfirmValidity.Text = "Confirm Character Validity";
            this.mnuSpecialConfirmValidity.Click += new System.EventHandler(this.mnuSpecialConfirmValidity_Click);
            // 
            // mnuSpecialKarmaValue
            // 
            this.mnuSpecialKarmaValue.Image = global::Chummer.Properties.Resources.calculator;
            this.mnuSpecialKarmaValue.Name = "mnuSpecialKarmaValue";
            this.mnuSpecialKarmaValue.Size = new System.Drawing.Size(213, 22);
            this.mnuSpecialKarmaValue.Tag = "Menu_KarmaValue";
            this.mnuSpecialKarmaValue.Text = "Calculate Karma Value";
            this.mnuSpecialKarmaValue.Click += new System.EventHandler(this.mnuSpecialKarmaValue_Click);
            // 
            // tsMain
            // 
            this.tsMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsbSave,
            this.tsbPrint,
            this.tssSeparator,
            this.tsbCopy,
            this.tsbPaste});
            this.tsMain.Location = new System.Drawing.Point(0, 0);
            this.tsMain.Name = "tsMain";
            this.tsMain.Size = new System.Drawing.Size(1085, 25);
            this.tsMain.TabIndex = 53;
            this.tsMain.Text = "ToolStrip";
            this.tsMain.Visible = false;
            // 
            // tsbSave
            // 
            this.tsbSave.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbSave.Image = global::Chummer.Properties.Resources.disk;
            this.tsbSave.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbSave.MergeAction = System.Windows.Forms.MergeAction.Replace;
            this.tsbSave.MergeIndex = 2;
            this.tsbSave.Name = "tsbSave";
            this.tsbSave.Size = new System.Drawing.Size(23, 22);
            this.tsbSave.Tag = "Menu_FileSave";
            this.tsbSave.Text = "Save Character";
            this.tsbSave.Click += new System.EventHandler(this.tsbSave_Click);
            // 
            // tsbPrint
            // 
            this.tsbPrint.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbPrint.Image = global::Chummer.Properties.Resources.printer;
            this.tsbPrint.ImageTransparentColor = System.Drawing.Color.Black;
            this.tsbPrint.MergeAction = System.Windows.Forms.MergeAction.Replace;
            this.tsbPrint.MergeIndex = 4;
            this.tsbPrint.Name = "tsbPrint";
            this.tsbPrint.Size = new System.Drawing.Size(23, 22);
            this.tsbPrint.Tag = "Menu_FilePrint";
            this.tsbPrint.Text = "Print Character";
            this.tsbPrint.Click += new System.EventHandler(this.tsbPrint_Click);
            // 
            // tssSeparator
            // 
            this.tssSeparator.MergeIndex = 5;
            this.tssSeparator.Name = "tssSeparator";
            this.tssSeparator.Size = new System.Drawing.Size(6, 25);
            // 
            // tsbCopy
            // 
            this.tsbCopy.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbCopy.Image = global::Chummer.Properties.Resources.page_copy;
            this.tsbCopy.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbCopy.MergeIndex = 6;
            this.tsbCopy.Name = "tsbCopy";
            this.tsbCopy.Size = new System.Drawing.Size(23, 22);
            this.tsbCopy.Tag = "Menu_EditCopy";
            this.tsbCopy.Text = "Copy";
            this.tsbCopy.Click += new System.EventHandler(this.tsbCopy_Click);
            // 
            // tsbPaste
            // 
            this.tsbPaste.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbPaste.Image = global::Chummer.Properties.Resources.page_paste;
            this.tsbPaste.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbPaste.MergeIndex = 7;
            this.tsbPaste.Name = "tsbPaste";
            this.tsbPaste.Size = new System.Drawing.Size(23, 22);
            this.tsbPaste.Tag = "Menu_EditPaste";
            this.tsbPaste.Text = "Paste";
            this.tsbPaste.Click += new System.EventHandler(this.tsbPaste_Click);
            // 
            // cmsGear
            // 
            this.cmsGear.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsGearAddAsPlugin,
            this.tsGearName,
            this.tsGearNotes});
            this.cmsGear.Name = "cmsGear";
            this.cmsGear.Size = new System.Drawing.Size(148, 70);
            // 
            // tsGearAddAsPlugin
            // 
            this.tsGearAddAsPlugin.Image = global::Chummer.Properties.Resources.brick_add;
            this.tsGearAddAsPlugin.Name = "tsGearAddAsPlugin";
            this.tsGearAddAsPlugin.Size = new System.Drawing.Size(147, 22);
            this.tsGearAddAsPlugin.Tag = "Menu_AddAsPlugin";
            this.tsGearAddAsPlugin.Text = "&Add as Plugin";
            this.tsGearAddAsPlugin.Click += new System.EventHandler(this.tsGearAddAsPlugin_Click);
            // 
            // tsGearName
            // 
            this.tsGearName.Image = global::Chummer.Properties.Resources.tag_red;
            this.tsGearName.Name = "tsGearName";
            this.tsGearName.Size = new System.Drawing.Size(147, 22);
            this.tsGearName.Tag = "Menu_NameGear";
            this.tsGearName.Text = "Name Gear";
            this.tsGearName.Click += new System.EventHandler(this.tsGearName_Click);
            // 
            // tsGearNotes
            // 
            this.tsGearNotes.Image = global::Chummer.Properties.Resources.note_edit;
            this.tsGearNotes.Name = "tsGearNotes";
            this.tsGearNotes.Size = new System.Drawing.Size(147, 22);
            this.tsGearNotes.Tag = "Menu_Notes";
            this.tsGearNotes.Text = "&Notes";
            this.tsGearNotes.Click += new System.EventHandler(this.tsGearNotes_Click);
            // 
            // tsGearAllowRenameAddAsPlugin
            // 
            this.tsGearAllowRenameAddAsPlugin.Image = global::Chummer.Properties.Resources.brick_add;
            this.tsGearAllowRenameAddAsPlugin.Name = "tsGearAllowRenameAddAsPlugin";
            this.tsGearAllowRenameAddAsPlugin.Size = new System.Drawing.Size(170, 22);
            this.tsGearAllowRenameAddAsPlugin.Tag = "Menu_AddAsPlugin";
            this.tsGearAllowRenameAddAsPlugin.Text = "&Add as Plugin";
            this.tsGearAllowRenameAddAsPlugin.Click += new System.EventHandler(this.tsGearAddAsPlugin_Click);
            // 
            // tsGearAllowRenameName
            // 
            this.tsGearAllowRenameName.Image = global::Chummer.Properties.Resources.tag_red;
            this.tsGearAllowRenameName.Name = "tsGearAllowRenameName";
            this.tsGearAllowRenameName.Size = new System.Drawing.Size(170, 22);
            this.tsGearAllowRenameName.Tag = "Menu_NameGear";
            this.tsGearAllowRenameName.Text = "Name Gear";
            this.tsGearAllowRenameName.Click += new System.EventHandler(this.tsGearName_Click);
            // 
            // tsGearAllowRenameNotes
            // 
            this.tsGearAllowRenameNotes.Image = global::Chummer.Properties.Resources.note_edit;
            this.tsGearAllowRenameNotes.Name = "tsGearAllowRenameNotes";
            this.tsGearAllowRenameNotes.Size = new System.Drawing.Size(170, 22);
            this.tsGearAllowRenameNotes.Tag = "Menu_Notes";
            this.tsGearAllowRenameNotes.Text = "&Notes";
            this.tsGearAllowRenameNotes.Click += new System.EventHandler(this.tsGearNotes_Click);
            // 
            // cmsVehicleWeapon
            // 
            this.cmsVehicleWeapon.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsVehicleAddWeaponAccessoryAlt,
            this.tsVehicleAddUnderbarrelWeaponAlt,
            this.tsVehicleWeaponNotes});
            this.cmsVehicleWeapon.Name = "cmsVehicleWeapon";
            this.cmsVehicleWeapon.Size = new System.Drawing.Size(209, 70);
            // 
            // tsVehicleAddWeaponAccessoryAlt
            // 
            this.tsVehicleAddWeaponAccessoryAlt.Image = global::Chummer.Properties.Resources.brick_add;
            this.tsVehicleAddWeaponAccessoryAlt.Name = "tsVehicleAddWeaponAccessoryAlt";
            this.tsVehicleAddWeaponAccessoryAlt.Size = new System.Drawing.Size(208, 22);
            this.tsVehicleAddWeaponAccessoryAlt.Tag = "Menu_AddAccessory";
            this.tsVehicleAddWeaponAccessoryAlt.Text = "Add &Accessory";
            this.tsVehicleAddWeaponAccessoryAlt.Click += new System.EventHandler(this.tsVehicleAddWeaponAccessoryAlt_Click);
            // 
            // tsVehicleAddUnderbarrelWeaponAlt
            // 
            this.tsVehicleAddUnderbarrelWeaponAlt.Image = global::Chummer.Properties.Resources.award_star2_add;
            this.tsVehicleAddUnderbarrelWeaponAlt.Name = "tsVehicleAddUnderbarrelWeaponAlt";
            this.tsVehicleAddUnderbarrelWeaponAlt.Size = new System.Drawing.Size(208, 22);
            this.tsVehicleAddUnderbarrelWeaponAlt.Tag = "Menu_AddUnderbarrelWeapon";
            this.tsVehicleAddUnderbarrelWeaponAlt.Text = "Add Underbarrel Weapon";
            this.tsVehicleAddUnderbarrelWeaponAlt.Click += new System.EventHandler(this.tsVehicleAddUnderbarrelWeaponAlt_Click);
            // 
            // tsVehicleWeaponNotes
            // 
            this.tsVehicleWeaponNotes.Image = global::Chummer.Properties.Resources.note_edit;
            this.tsVehicleWeaponNotes.Name = "tsVehicleWeaponNotes";
            this.tsVehicleWeaponNotes.Size = new System.Drawing.Size(208, 22);
            this.tsVehicleWeaponNotes.Tag = "Menu_Notes";
            this.tsVehicleWeaponNotes.Text = "&Notes";
            this.tsVehicleWeaponNotes.Click += new System.EventHandler(this.tsVehicleNotes_Click);
            // 
            // cmsVehicleGear
            // 
            this.cmsVehicleGear.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsVehicleGearAddAsPlugin,
            this.tsVehicleGearNotes});
            this.cmsVehicleGear.Name = "cmsWeapon";
            this.cmsVehicleGear.Size = new System.Drawing.Size(148, 48);
            // 
            // tsVehicleGearAddAsPlugin
            // 
            this.tsVehicleGearAddAsPlugin.Image = global::Chummer.Properties.Resources.brick_add;
            this.tsVehicleGearAddAsPlugin.Name = "tsVehicleGearAddAsPlugin";
            this.tsVehicleGearAddAsPlugin.Size = new System.Drawing.Size(147, 22);
            this.tsVehicleGearAddAsPlugin.Tag = "Menu_AddAsPlugin";
            this.tsVehicleGearAddAsPlugin.Text = "&Add as Plugin";
            this.tsVehicleGearAddAsPlugin.Click += new System.EventHandler(this.tsVehicleGearAddAsPlugin_Click);
            // 
            // tsVehicleGearNotes
            // 
            this.tsVehicleGearNotes.Image = global::Chummer.Properties.Resources.note_edit;
            this.tsVehicleGearNotes.Name = "tsVehicleGearNotes";
            this.tsVehicleGearNotes.Size = new System.Drawing.Size(147, 22);
            this.tsVehicleGearNotes.Tag = "Menu_Notes";
            this.tsVehicleGearNotes.Text = "&Notes";
            this.tsVehicleGearNotes.Click += new System.EventHandler(this.tsVehicleGearNotes_Click);
            // 
            // cmsArmorGear
            // 
            this.cmsArmorGear.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsArmorGearAddAsPlugin,
            this.tsArmorGearNotes});
            this.cmsArmorGear.Name = "cmsWeapon";
            this.cmsArmorGear.Size = new System.Drawing.Size(148, 48);
            // 
            // tsArmorGearAddAsPlugin
            // 
            this.tsArmorGearAddAsPlugin.Image = global::Chummer.Properties.Resources.brick_add;
            this.tsArmorGearAddAsPlugin.Name = "tsArmorGearAddAsPlugin";
            this.tsArmorGearAddAsPlugin.Size = new System.Drawing.Size(147, 22);
            this.tsArmorGearAddAsPlugin.Tag = "Menu_AddAsPlugin";
            this.tsArmorGearAddAsPlugin.Text = "&Add as Plugin";
            this.tsArmorGearAddAsPlugin.Click += new System.EventHandler(this.tsArmorGearAddAsPlugin_Click);
            // 
            // tsArmorGearNotes
            // 
            this.tsArmorGearNotes.Image = global::Chummer.Properties.Resources.note_edit;
            this.tsArmorGearNotes.Name = "tsArmorGearNotes";
            this.tsArmorGearNotes.Size = new System.Drawing.Size(147, 22);
            this.tsArmorGearNotes.Tag = "Menu_Notes";
            this.tsArmorGearNotes.Text = "&Notes";
            this.tsArmorGearNotes.Click += new System.EventHandler(this.tsArmorNotes_Click);
            // 
            // cmsArmorMod
            // 
            this.cmsArmorMod.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsArmorModNotes,
            this.tsArmorModAddAsPlugin});
            this.cmsArmorMod.Name = "cmsArmorMod";
            this.cmsArmorMod.Size = new System.Drawing.Size(148, 48);
            // 
            // tsArmorModNotes
            // 
            this.tsArmorModNotes.Image = global::Chummer.Properties.Resources.note_edit;
            this.tsArmorModNotes.Name = "tsArmorModNotes";
            this.tsArmorModNotes.Size = new System.Drawing.Size(147, 22);
            this.tsArmorModNotes.Tag = "Menu_Notes";
            this.tsArmorModNotes.Text = "&Notes";
            this.tsArmorModNotes.Click += new System.EventHandler(this.tsArmorNotes_Click);
            // 
            // tsArmorModAddAsPlugin
            // 
            this.tsArmorModAddAsPlugin.Image = global::Chummer.Properties.Resources.brick_add;
            this.tsArmorModAddAsPlugin.Name = "tsArmorModAddAsPlugin";
            this.tsArmorModAddAsPlugin.Size = new System.Drawing.Size(147, 22);
            this.tsArmorModAddAsPlugin.Tag = "Menu_AddAsPlugin";
            this.tsArmorModAddAsPlugin.Text = "&Add as Plugin";
            this.tsArmorModAddAsPlugin.Click += new System.EventHandler(this.tsArmorGearAddAsPlugin_Click);
            // 
            // cmsQuality
            // 
            this.cmsQuality.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsQualityNotes});
            this.cmsQuality.Name = "cmsQuality";
            this.cmsQuality.Size = new System.Drawing.Size(106, 26);
            // 
            // tsQualityNotes
            // 
            this.tsQualityNotes.Image = global::Chummer.Properties.Resources.note_edit;
            this.tsQualityNotes.Name = "tsQualityNotes";
            this.tsQualityNotes.Size = new System.Drawing.Size(105, 22);
            this.tsQualityNotes.Tag = "Menu_Notes";
            this.tsQualityNotes.Text = "&Notes";
            this.tsQualityNotes.Click += new System.EventHandler(this.tsQualityNotes_Click);
            // 
            // cmsSpell
            // 
            this.cmsSpell.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmSpellNotes});
            this.cmsSpell.Name = "cmsSpell";
            this.cmsSpell.Size = new System.Drawing.Size(106, 26);
            // 
            // tsmSpellNotes
            // 
            this.tsmSpellNotes.Image = global::Chummer.Properties.Resources.note_edit;
            this.tsmSpellNotes.Name = "tsmSpellNotes";
            this.tsmSpellNotes.Size = new System.Drawing.Size(105, 22);
            this.tsmSpellNotes.Tag = "Menu_Notes";
            this.tsmSpellNotes.Text = "&Notes";
            this.tsmSpellNotes.Click += new System.EventHandler(this.tsSpellNotes_Click);
            // 
            // cmsCritterPowers
            // 
            this.cmsCritterPowers.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsCritterPowersNotes});
            this.cmsCritterPowers.Name = "cmsCritterPowers";
            this.cmsCritterPowers.Size = new System.Drawing.Size(106, 26);
            // 
            // tsCritterPowersNotes
            // 
            this.tsCritterPowersNotes.Image = global::Chummer.Properties.Resources.note_edit;
            this.tsCritterPowersNotes.Name = "tsCritterPowersNotes";
            this.tsCritterPowersNotes.Size = new System.Drawing.Size(105, 22);
            this.tsCritterPowersNotes.Tag = "Menu_Notes";
            this.tsCritterPowersNotes.Text = "&Notes";
            this.tsCritterPowersNotes.Click += new System.EventHandler(this.tsCritterPowersNotes_Click);
            // 
            // cmsLifestyleNotes
            // 
            this.cmsLifestyleNotes.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsEditLifestyle,
            this.tsLifestyleName,
            this.tsLifestyleNotes});
            this.cmsLifestyleNotes.Name = "cmsLifestyleNotes";
            this.cmsLifestyleNotes.Size = new System.Drawing.Size(153, 70);
            // 
            // tsEditLifestyle
            // 
            this.tsEditLifestyle.Image = global::Chummer.Properties.Resources.house_edit;
            this.tsEditLifestyle.Name = "tsEditLifestyle";
            this.tsEditLifestyle.Size = new System.Drawing.Size(152, 22);
            this.tsEditLifestyle.Tag = "Menu_EditLifestyle";
            this.tsEditLifestyle.Text = "&Edit Lifestyle";
            this.tsEditLifestyle.Click += new System.EventHandler(this.tsEditLifestyle_Click);
            // 
            // tsLifestyleName
            // 
            this.tsLifestyleName.Image = global::Chummer.Properties.Resources.tag_red;
            this.tsLifestyleName.Name = "tsLifestyleName";
            this.tsLifestyleName.Size = new System.Drawing.Size(152, 22);
            this.tsLifestyleName.Tag = "Menu_NameLifestyle";
            this.tsLifestyleName.Text = "Name Lifestyle";
            this.tsLifestyleName.Click += new System.EventHandler(this.tsLifestyleName_Click);
            // 
            // tsLifestyleNotes
            // 
            this.tsLifestyleNotes.Image = global::Chummer.Properties.Resources.note_edit;
            this.tsLifestyleNotes.Name = "tsLifestyleNotes";
            this.tsLifestyleNotes.Size = new System.Drawing.Size(152, 22);
            this.tsLifestyleNotes.Tag = "Menu_Notes";
            this.tsLifestyleNotes.Text = "&Notes";
            this.tsLifestyleNotes.Click += new System.EventHandler(this.tsLifestyleNotes_Click);
            // 
            // cmsWeaponAccessory
            // 
            this.cmsWeaponAccessory.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsWeaponAccessoryAddGear,
            this.tsWeaponAccessoryNotes});
            this.cmsWeaponAccessory.Name = "cmsWeaponAccessory";
            this.cmsWeaponAccessory.Size = new System.Drawing.Size(124, 48);
            // 
            // tsWeaponAccessoryAddGear
            // 
            this.tsWeaponAccessoryAddGear.Image = global::Chummer.Properties.Resources.camera_add;
            this.tsWeaponAccessoryAddGear.Name = "tsWeaponAccessoryAddGear";
            this.tsWeaponAccessoryAddGear.Size = new System.Drawing.Size(123, 22);
            this.tsWeaponAccessoryAddGear.Tag = "Menu_AddGear";
            this.tsWeaponAccessoryAddGear.Text = "Add &Gear";
            this.tsWeaponAccessoryAddGear.Click += new System.EventHandler(this.tsWeaponAccessoryAddGear_Click);
            // 
            // tsWeaponAccessoryNotes
            // 
            this.tsWeaponAccessoryNotes.Image = global::Chummer.Properties.Resources.note_edit;
            this.tsWeaponAccessoryNotes.Name = "tsWeaponAccessoryNotes";
            this.tsWeaponAccessoryNotes.Size = new System.Drawing.Size(123, 22);
            this.tsWeaponAccessoryNotes.Tag = "Menu_Notes";
            this.tsWeaponAccessoryNotes.Text = "&Notes";
            this.tsWeaponAccessoryNotes.Click += new System.EventHandler(this.tsWeaponNotes_Click);
            // 
            // cmsGearPlugin
            // 
            this.cmsGearPlugin.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsGearPluginNotes});
            this.cmsGearPlugin.Name = "cmsGearPlugin";
            this.cmsGearPlugin.Size = new System.Drawing.Size(106, 26);
            // 
            // tsGearPluginNotes
            // 
            this.tsGearPluginNotes.Image = global::Chummer.Properties.Resources.note_edit;
            this.tsGearPluginNotes.Name = "tsGearPluginNotes";
            this.tsGearPluginNotes.Size = new System.Drawing.Size(105, 22);
            this.tsGearPluginNotes.Tag = "Menu_Notes";
            this.tsGearPluginNotes.Text = "&Notes";
            this.tsGearPluginNotes.Click += new System.EventHandler(this.tsGearNotes_Click);
            // 
            // cmsComplexFormPlugin
            // 
            this.cmsComplexFormPlugin.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsComplexFormPluginNotes});
            this.cmsComplexFormPlugin.Name = "cmsComplexFormPlugin";
            this.cmsComplexFormPlugin.Size = new System.Drawing.Size(106, 26);
            // 
            // tsComplexFormPluginNotes
            // 
            this.tsComplexFormPluginNotes.Image = global::Chummer.Properties.Resources.note_edit;
            this.tsComplexFormPluginNotes.Name = "tsComplexFormPluginNotes";
            this.tsComplexFormPluginNotes.Size = new System.Drawing.Size(105, 22);
            this.tsComplexFormPluginNotes.Tag = "Menu_Notes";
            this.tsComplexFormPluginNotes.Text = "&Notes";
            // 
            // cmsBioware
            // 
            this.cmsBioware.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsBiowareNotes});
            this.cmsBioware.Name = "cmsBioware";
            this.cmsBioware.Size = new System.Drawing.Size(106, 26);
            // 
            // tsBiowareNotes
            // 
            this.tsBiowareNotes.Image = global::Chummer.Properties.Resources.note_edit;
            this.tsBiowareNotes.Name = "tsBiowareNotes";
            this.tsBiowareNotes.Size = new System.Drawing.Size(105, 22);
            this.tsBiowareNotes.Tag = "Menu_Notes";
            this.tsBiowareNotes.Text = "&Notes";
            this.tsBiowareNotes.Click += new System.EventHandler(this.tsCyberwareNotes_Click);
            // 
            // cmsAdvancedLifestyle
            // 
            this.cmsAdvancedLifestyle.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsEditAdvancedLifestyle,
            this.tsAdvancedLifestyleNotes});
            this.cmsAdvancedLifestyle.Name = "cmsAdvancedLifestyle";
            this.cmsAdvancedLifestyle.Size = new System.Drawing.Size(197, 48);
            // 
            // tsEditAdvancedLifestyle
            // 
            this.tsEditAdvancedLifestyle.Image = global::Chummer.Properties.Resources.house_edit;
            this.tsEditAdvancedLifestyle.Name = "tsEditAdvancedLifestyle";
            this.tsEditAdvancedLifestyle.Size = new System.Drawing.Size(196, 22);
            this.tsEditAdvancedLifestyle.Tag = "Menu_EditAdvancedLifestyle";
            this.tsEditAdvancedLifestyle.Text = "&Edit Advanced Lifestyle";
            this.tsEditAdvancedLifestyle.Click += new System.EventHandler(this.tsEditAdvancedLifestyle_Click);
            // 
            // tsAdvancedLifestyleNotes
            // 
            this.tsAdvancedLifestyleNotes.Image = global::Chummer.Properties.Resources.note_edit;
            this.tsAdvancedLifestyleNotes.Name = "tsAdvancedLifestyleNotes";
            this.tsAdvancedLifestyleNotes.Size = new System.Drawing.Size(196, 22);
            this.tsAdvancedLifestyleNotes.Tag = "Menu_Notes";
            this.tsAdvancedLifestyleNotes.Text = "&Notes";
            this.tsAdvancedLifestyleNotes.Click += new System.EventHandler(this.tsAdvancedLifestyleNotes_Click);
            // 
            // cmsGearLocation
            // 
            this.cmsGearLocation.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsGearRenameLocation,
            this.tsGearLocationAddGear});
            this.cmsGearLocation.Name = "cmsGearLocation";
            this.cmsGearLocation.Size = new System.Drawing.Size(167, 48);
            // 
            // tsGearRenameLocation
            // 
            this.tsGearRenameLocation.Image = global::Chummer.Properties.Resources.building_edit;
            this.tsGearRenameLocation.Name = "tsGearRenameLocation";
            this.tsGearRenameLocation.Size = new System.Drawing.Size(166, 22);
            this.tsGearRenameLocation.Tag = "Menu_RenameLocation";
            this.tsGearRenameLocation.Text = "&Rename Location";
            this.tsGearRenameLocation.Click += new System.EventHandler(this.tsGearRenameLocation_Click);
            // 
            // tsGearLocationAddGear
            // 
            this.tsGearLocationAddGear.Image = global::Chummer.Properties.Resources.camera_add;
            this.tsGearLocationAddGear.Name = "tsGearLocationAddGear";
            this.tsGearLocationAddGear.Size = new System.Drawing.Size(166, 22);
            this.tsGearLocationAddGear.Tag = "Menu_AddGear";
            this.tsGearLocationAddGear.Text = "Add &Gear";
            this.tsGearLocationAddGear.Click += new System.EventHandler(this.cmdAddGear_Click);
            // 
            // cmsArmorLocation
            // 
            this.cmsArmorLocation.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsArmorLocationAddArmor,
            this.tsArmorRenameLocation});
            this.cmsArmorLocation.Name = "cmsGearLocation";
            this.cmsArmorLocation.Size = new System.Drawing.Size(167, 48);
            // 
            // tsArmorLocationAddArmor
            // 
            this.tsArmorLocationAddArmor.Image = global::Chummer.Properties.Resources.building_edit;
            this.tsArmorLocationAddArmor.Name = "tsArmorLocationAddArmor";
            this.tsArmorLocationAddArmor.Size = new System.Drawing.Size(166, 22);
            this.tsArmorLocationAddArmor.Tag = "Button_AddArmor";
            this.tsArmorLocationAddArmor.Text = "&Add Armor";
            this.tsArmorLocationAddArmor.Click += new System.EventHandler(this.tsArmorLocationAddArmor_Click);
            // 
            // tsArmorRenameLocation
            // 
            this.tsArmorRenameLocation.Image = global::Chummer.Properties.Resources.building_edit;
            this.tsArmorRenameLocation.Name = "tsArmorRenameLocation";
            this.tsArmorRenameLocation.Size = new System.Drawing.Size(166, 22);
            this.tsArmorRenameLocation.Tag = "Menu_RenameLocation";
            this.tsArmorRenameLocation.Text = "&Rename Location";
            this.tsArmorRenameLocation.Click += new System.EventHandler(this.tsArmorRenameLocation_Click);
            // 
            // cmsCyberwareGear
            // 
            this.cmsCyberwareGear.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsCyberwareGearMenuAddAsPlugin});
            this.cmsCyberwareGear.Name = "cmsCyberwareGear";
            this.cmsCyberwareGear.Size = new System.Drawing.Size(148, 26);
            // 
            // tsCyberwareGearMenuAddAsPlugin
            // 
            this.tsCyberwareGearMenuAddAsPlugin.Image = global::Chummer.Properties.Resources.brick_add;
            this.tsCyberwareGearMenuAddAsPlugin.Name = "tsCyberwareGearMenuAddAsPlugin";
            this.tsCyberwareGearMenuAddAsPlugin.Size = new System.Drawing.Size(147, 22);
            this.tsCyberwareGearMenuAddAsPlugin.Tag = "Menu_AddAsPlugin";
            this.tsCyberwareGearMenuAddAsPlugin.Text = "&Add as Plugin";
            this.tsCyberwareGearMenuAddAsPlugin.Click += new System.EventHandler(this.tsCyberwareGearMenuAddAsPlugin_Click);
            // 
            // cmsVehicleCyberwareGear
            // 
            this.cmsVehicleCyberwareGear.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsVehicleCyberwareGearMenuAddAsPlugin});
            this.cmsVehicleCyberwareGear.Name = "cmsVehicleCyberwareGear";
            this.cmsVehicleCyberwareGear.Size = new System.Drawing.Size(148, 26);
            // 
            // tsVehicleCyberwareGearMenuAddAsPlugin
            // 
            this.tsVehicleCyberwareGearMenuAddAsPlugin.Image = global::Chummer.Properties.Resources.brick_add;
            this.tsVehicleCyberwareGearMenuAddAsPlugin.Name = "tsVehicleCyberwareGearMenuAddAsPlugin";
            this.tsVehicleCyberwareGearMenuAddAsPlugin.Size = new System.Drawing.Size(147, 22);
            this.tsVehicleCyberwareGearMenuAddAsPlugin.Tag = "Menu_AddAsPlugin";
            this.tsVehicleCyberwareGearMenuAddAsPlugin.Text = "&Add as Plugin";
            this.tsVehicleCyberwareGearMenuAddAsPlugin.Click += new System.EventHandler(this.tsVehicleCyberwareGearMenuAddAsPlugin_Click);
            // 
            // cmsWeaponAccessoryGear
            // 
            this.cmsWeaponAccessoryGear.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsWeaponAccessoryGearMenuAddAsPlugin});
            this.cmsWeaponAccessoryGear.Name = "cmsWeaponAccessoryGear";
            this.cmsWeaponAccessoryGear.Size = new System.Drawing.Size(148, 26);
            // 
            // tsWeaponAccessoryGearMenuAddAsPlugin
            // 
            this.tsWeaponAccessoryGearMenuAddAsPlugin.Image = global::Chummer.Properties.Resources.brick_add;
            this.tsWeaponAccessoryGearMenuAddAsPlugin.Name = "tsWeaponAccessoryGearMenuAddAsPlugin";
            this.tsWeaponAccessoryGearMenuAddAsPlugin.Size = new System.Drawing.Size(147, 22);
            this.tsWeaponAccessoryGearMenuAddAsPlugin.Tag = "Menu_AddAsPlugin";
            this.tsWeaponAccessoryGearMenuAddAsPlugin.Text = "&Add as Plugin";
            this.tsWeaponAccessoryGearMenuAddAsPlugin.Click += new System.EventHandler(this.tsWeaponAccessoryGearMenuAddAsPlugin_Click);
            // 
            // cmsVehicleLocation
            // 
            this.cmsVehicleLocation.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsVehicleRenameLocation,
            this.tsVehicleLocationAddVehicle});
            this.cmsVehicleLocation.Name = "cmsGearLocation";
            this.cmsVehicleLocation.Size = new System.Drawing.Size(167, 48);
            // 
            // tsVehicleRenameLocation
            // 
            this.tsVehicleRenameLocation.Image = global::Chummer.Properties.Resources.building_edit;
            this.tsVehicleRenameLocation.Name = "tsVehicleRenameLocation";
            this.tsVehicleRenameLocation.Size = new System.Drawing.Size(166, 22);
            this.tsVehicleRenameLocation.Tag = "Menu_RenameLocation";
            this.tsVehicleRenameLocation.Text = "&Rename Location";
            this.tsVehicleRenameLocation.Click += new System.EventHandler(this.tsVehicleRenameLocation_Click);
            // 
            // tsVehicleLocationAddVehicle
            // 
            this.tsVehicleLocationAddVehicle.Image = global::Chummer.Properties.Resources.car_add;
            this.tsVehicleLocationAddVehicle.Name = "tsVehicleLocationAddVehicle";
            this.tsVehicleLocationAddVehicle.Size = new System.Drawing.Size(166, 22);
            this.tsVehicleLocationAddVehicle.Tag = "Menu_AddVehicle";
            this.tsVehicleLocationAddVehicle.Text = "Add &Vehicle";
            this.tsVehicleLocationAddVehicle.Click += new System.EventHandler(this.tsVehicleLocationAddVehicle_Click);
            // 
            // cmsVehicleWeaponAccessory
            // 
            this.cmsVehicleWeaponAccessory.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsVehicleWeaponAccessoryAddGear,
            this.tsVehicleWeaponAccessoryNotes});
            this.cmsVehicleWeaponAccessory.Name = "cmsWeaponAccessory";
            this.cmsVehicleWeaponAccessory.Size = new System.Drawing.Size(124, 48);
            // 
            // tsVehicleWeaponAccessoryAddGear
            // 
            this.tsVehicleWeaponAccessoryAddGear.Image = global::Chummer.Properties.Resources.camera_add;
            this.tsVehicleWeaponAccessoryAddGear.Name = "tsVehicleWeaponAccessoryAddGear";
            this.tsVehicleWeaponAccessoryAddGear.Size = new System.Drawing.Size(123, 22);
            this.tsVehicleWeaponAccessoryAddGear.Tag = "Menu_AddGear";
            this.tsVehicleWeaponAccessoryAddGear.Text = "Add &Gear";
            this.tsVehicleWeaponAccessoryAddGear.Click += new System.EventHandler(this.tsVehicleWeaponAccessoryAddGear_Click);
            // 
            // tsVehicleWeaponAccessoryNotes
            // 
            this.tsVehicleWeaponAccessoryNotes.Image = global::Chummer.Properties.Resources.note_edit;
            this.tsVehicleWeaponAccessoryNotes.Name = "tsVehicleWeaponAccessoryNotes";
            this.tsVehicleWeaponAccessoryNotes.Size = new System.Drawing.Size(123, 22);
            this.tsVehicleWeaponAccessoryNotes.Tag = "Menu_Notes";
            this.tsVehicleWeaponAccessoryNotes.Text = "&Notes";
            this.tsVehicleWeaponAccessoryNotes.Click += new System.EventHandler(this.tsVehicleNotes_Click);
            // 
            // cmsVehicleWeaponAccessoryGear
            // 
            this.cmsVehicleWeaponAccessoryGear.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsVehicleWeaponAccessoryGearMenuAddAsPlugin});
            this.cmsVehicleWeaponAccessoryGear.Name = "cmsVehicleWeaponAccessoryGear";
            this.cmsVehicleWeaponAccessoryGear.Size = new System.Drawing.Size(148, 26);
            // 
            // tsVehicleWeaponAccessoryGearMenuAddAsPlugin
            // 
            this.tsVehicleWeaponAccessoryGearMenuAddAsPlugin.Image = global::Chummer.Properties.Resources.brick_add;
            this.tsVehicleWeaponAccessoryGearMenuAddAsPlugin.Name = "tsVehicleWeaponAccessoryGearMenuAddAsPlugin";
            this.tsVehicleWeaponAccessoryGearMenuAddAsPlugin.Size = new System.Drawing.Size(147, 22);
            this.tsVehicleWeaponAccessoryGearMenuAddAsPlugin.Tag = "Menu_AddAsPlugin";
            this.tsVehicleWeaponAccessoryGearMenuAddAsPlugin.Text = "&Add as Plugin";
            this.tsVehicleWeaponAccessoryGearMenuAddAsPlugin.Click += new System.EventHandler(this.tsVehicleWeaponAccessoryGearMenuAddAsPlugin_Click);
            // 
            // cmsWeaponLocation
            // 
            this.cmsWeaponLocation.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsWeaponRenameLocation,
            this.tsWeaponLocationAddWeapon});
            this.cmsWeaponLocation.Name = "cmsGearLocation";
            this.cmsWeaponLocation.Size = new System.Drawing.Size(167, 48);
            // 
            // tsWeaponRenameLocation
            // 
            this.tsWeaponRenameLocation.Image = global::Chummer.Properties.Resources.building_edit;
            this.tsWeaponRenameLocation.Name = "tsWeaponRenameLocation";
            this.tsWeaponRenameLocation.Size = new System.Drawing.Size(166, 22);
            this.tsWeaponRenameLocation.Tag = "Menu_RenameLocation";
            this.tsWeaponRenameLocation.Text = "&Rename Location";
            this.tsWeaponRenameLocation.Click += new System.EventHandler(this.tsWeaponRenameLocation_Click);
            // 
            // tsWeaponLocationAddWeapon
            // 
            this.tsWeaponLocationAddWeapon.Image = global::Chummer.Properties.Resources.award_star_add;
            this.tsWeaponLocationAddWeapon.Name = "tsWeaponLocationAddWeapon";
            this.tsWeaponLocationAddWeapon.Size = new System.Drawing.Size(166, 22);
            this.tsWeaponLocationAddWeapon.Tag = "Menu_AddWeapon";
            this.tsWeaponLocationAddWeapon.Text = "Add &Weapon";
            this.tsWeaponLocationAddWeapon.Click += new System.EventHandler(this.tsWeaponLocationAddWeapon_Click);
            // 
            // splitMain
            // 
            this.splitMain.BackColor = System.Drawing.SystemColors.InactiveCaption;
            this.splitMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitMain.Location = new System.Drawing.Point(0, 0);
            this.splitMain.Name = "splitMain";
            // 
            // splitMain.Panel1
            // 
            this.splitMain.Panel1.BackColor = System.Drawing.SystemColors.Control;
            this.splitMain.Panel1.Controls.Add(this.tabCharacterTabs);
            this.splitMain.Panel1MinSize = 849;
            // 
            // splitMain.Panel2
            // 
            this.splitMain.Panel2.BackColor = System.Drawing.SystemColors.Control;
            this.splitMain.Panel2.Controls.Add(this.tabInfo);
            this.splitMain.Size = new System.Drawing.Size(1264, 657);
            this.splitMain.SplitterDistance = 985;
            this.splitMain.TabIndex = 54;
            // 
            // tabCharacterTabs
            // 
            this.tabCharacterTabs.Controls.Add(this.tabCommon);
            this.tabCharacterTabs.Controls.Add(this.tabSkills);
            this.tabCharacterTabs.Controls.Add(this.tabLimits);
            this.tabCharacterTabs.Controls.Add(this.tabMartialArts);
            this.tabCharacterTabs.Controls.Add(this.tabMagician);
            this.tabCharacterTabs.Controls.Add(this.tabAdept);
            this.tabCharacterTabs.Controls.Add(this.tabTechnomancer);
            this.tabCharacterTabs.Controls.Add(this.tabAdvancedPrograms);
            this.tabCharacterTabs.Controls.Add(this.tabCritter);
            this.tabCharacterTabs.Controls.Add(this.tabInitiation);
            this.tabCharacterTabs.Controls.Add(this.tabCyberware);
            this.tabCharacterTabs.Controls.Add(this.tabStreetGear);
            this.tabCharacterTabs.Controls.Add(this.tabVehicles);
            this.tabCharacterTabs.Controls.Add(this.tabCharacterInfo);
            this.tabCharacterTabs.Controls.Add(this.tabRelationships);
            this.tabCharacterTabs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabCharacterTabs.Location = new System.Drawing.Point(0, 0);
            this.tabCharacterTabs.Name = "tabCharacterTabs";
            this.tabCharacterTabs.SelectedIndex = 0;
            this.tabCharacterTabs.Size = new System.Drawing.Size(985, 657);
            this.tabCharacterTabs.TabIndex = 33;
            this.tabCharacterTabs.SelectedIndexChanged += new System.EventHandler(this.tabCharacterTabs_SelectedIndexChanged);
            // 
            // tabCommon
            // 
            this.tabCommon.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.tabCommon.Controls.Add(this.tlpCommon);
            this.tabCommon.Location = new System.Drawing.Point(4, 22);
            this.tabCommon.Name = "tabCommon";
            this.tabCommon.Padding = new System.Windows.Forms.Padding(3);
            this.tabCommon.Size = new System.Drawing.Size(977, 631);
            this.tabCommon.TabIndex = 0;
            this.tabCommon.Tag = "Tab_Common";
            this.tabCommon.Text = "Common";
            // 
            // tlpCommon
            // 
            this.tlpCommon.AutoSize = true;
            this.tlpCommon.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpCommon.ColumnCount = 4;
            this.tlpCommon.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpCommon.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpCommon.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpCommon.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpCommon.Controls.Add(this.tlpCommonLeftSide, 0, 0);
            this.tlpCommon.Controls.Add(this.pnlAttributes, 1, 2);
            this.tlpCommon.Controls.Add(this.lblAttributes, 1, 1);
            this.tlpCommon.Controls.Add(this.flpAttributesLabels, 2, 1);
            this.tlpCommon.Controls.Add(this.tlpAlias, 1, 0);
            this.tlpCommon.Controls.Add(this.tlpCommonRightSide, 3, 0);
            this.tlpCommon.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpCommon.Location = new System.Drawing.Point(3, 3);
            this.tlpCommon.Name = "tlpCommon";
            this.tlpCommon.RowCount = 3;
            this.tlpCommon.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpCommon.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpCommon.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpCommon.Size = new System.Drawing.Size(971, 625);
            this.tlpCommon.TabIndex = 99;
            // 
            // tlpCommonLeftSide
            // 
            this.tlpCommonLeftSide.AutoSize = true;
            this.tlpCommonLeftSide.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpCommonLeftSide.ColumnCount = 2;
            this.tlpCommonLeftSide.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpCommonLeftSide.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpCommonLeftSide.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpCommonLeftSide.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpCommonLeftSide.Controls.Add(this.treQualities, 0, 2);
            this.tlpCommonLeftSide.Controls.Add(this.lblQualityLevelLabel, 0, 1);
            this.tlpCommonLeftSide.Controls.Add(this.tlpQualityButtons, 0, 0);
            this.tlpCommonLeftSide.Controls.Add(this.nudQualityLevel, 1, 1);
            this.tlpCommonLeftSide.Controls.Add(this.tlpCommonLeftSideBottom, 0, 3);
            this.tlpCommonLeftSide.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpCommonLeftSide.Location = new System.Drawing.Point(0, 0);
            this.tlpCommonLeftSide.Margin = new System.Windows.Forms.Padding(0);
            this.tlpCommonLeftSide.Name = "tlpCommonLeftSide";
            this.tlpCommonLeftSide.RowCount = 4;
            this.tlpCommon.SetRowSpan(this.tlpCommonLeftSide, 3);
            this.tlpCommonLeftSide.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpCommonLeftSide.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpCommonLeftSide.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpCommonLeftSide.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpCommonLeftSide.Size = new System.Drawing.Size(301, 625);
            this.tlpCommonLeftSide.TabIndex = 100;
            // 
            // treQualities
            // 
            this.tlpCommonLeftSide.SetColumnSpan(this.treQualities, 2);
            this.treQualities.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treQualities.HideSelection = false;
            this.treQualities.Indent = 15;
            this.treQualities.Location = new System.Drawing.Point(3, 58);
            this.treQualities.Name = "treQualities";
            treeNode1.Name = "nodPositiveQualityRoot";
            treeNode1.Tag = "Node_SelectedPositiveQualities";
            treeNode1.Text = "Selected Positive Qualities";
            treeNode2.Name = "nodNegativeQualityRoot";
            treeNode2.Tag = "Node_SelectedNegativeQualities";
            treeNode2.Text = "Selected Negative Qualities";
            this.treQualities.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            treeNode1,
            treeNode2});
            this.treQualities.ShowNodeToolTips = true;
            this.treQualities.ShowPlusMinus = false;
            this.treQualities.ShowRootLines = false;
            this.treQualities.Size = new System.Drawing.Size(295, 539);
            this.treQualities.TabIndex = 33;
            this.treQualities.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treQualities_AfterSelect);
            this.treQualities.KeyDown += new System.Windows.Forms.KeyEventHandler(this.treQualities_KeyDown);
            this.treQualities.MouseDown += new System.Windows.Forms.MouseEventHandler(this.TreeView_MouseDown);
            // 
            // lblQualityLevelLabel
            // 
            this.lblQualityLevelLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblQualityLevelLabel.AutoSize = true;
            this.lblQualityLevelLabel.Location = new System.Drawing.Point(3, 35);
            this.lblQualityLevelLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblQualityLevelLabel.Name = "lblQualityLevelLabel";
            this.lblQualityLevelLabel.Size = new System.Drawing.Size(71, 13);
            this.lblQualityLevelLabel.TabIndex = 98;
            this.lblQualityLevelLabel.Tag = "Label_QualityLevel";
            this.lblQualityLevelLabel.Text = "Quality Level:";
            // 
            // tlpQualityButtons
            // 
            this.tlpQualityButtons.AutoSize = true;
            this.tlpQualityButtons.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpQualityButtons.ColumnCount = 3;
            this.tlpCommonLeftSide.SetColumnSpan(this.tlpQualityButtons, 2);
            this.tlpQualityButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpQualityButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpQualityButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpQualityButtons.Controls.Add(this.cmdAddQuality, 0, 0);
            this.tlpQualityButtons.Controls.Add(this.cmdLifeModule, 2, 0);
            this.tlpQualityButtons.Controls.Add(this.cmdDeleteQuality, 1, 0);
            this.tlpQualityButtons.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpQualityButtons.Location = new System.Drawing.Point(0, 0);
            this.tlpQualityButtons.Margin = new System.Windows.Forms.Padding(0);
            this.tlpQualityButtons.Name = "tlpQualityButtons";
            this.tlpQualityButtons.RowCount = 1;
            this.tlpQualityButtons.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpQualityButtons.Size = new System.Drawing.Size(301, 29);
            this.tlpQualityButtons.TabIndex = 102;
            // 
            // cmdAddQuality
            // 
            this.cmdAddQuality.AutoSize = true;
            this.cmdAddQuality.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdAddQuality.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmdAddQuality.Location = new System.Drawing.Point(3, 3);
            this.cmdAddQuality.Name = "cmdAddQuality";
            this.cmdAddQuality.Size = new System.Drawing.Size(105, 23);
            this.cmdAddQuality.TabIndex = 62;
            this.cmdAddQuality.Tag = "Button_AddQuality";
            this.cmdAddQuality.Text = "Add &Quality";
            this.cmdAddQuality.UseVisualStyleBackColor = true;
            this.cmdAddQuality.Click += new System.EventHandler(this.cmdAddQuality_Click);
            // 
            // cmdLifeModule
            // 
            this.cmdLifeModule.AutoSize = true;
            this.cmdLifeModule.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdLifeModule.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmdLifeModule.Location = new System.Drawing.Point(225, 3);
            this.cmdLifeModule.Name = "cmdLifeModule";
            this.cmdLifeModule.Size = new System.Drawing.Size(73, 23);
            this.cmdLifeModule.TabIndex = 95;
            this.cmdLifeModule.Tag = "String_LifeModule";
            this.cmdLifeModule.Text = "Life Module";
            this.cmdLifeModule.UseVisualStyleBackColor = true;
            this.cmdLifeModule.Visible = false;
            this.cmdLifeModule.Click += new System.EventHandler(this.cmdLifeModule_Click);
            // 
            // cmdDeleteQuality
            // 
            this.cmdDeleteQuality.AutoSize = true;
            this.cmdDeleteQuality.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdDeleteQuality.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmdDeleteQuality.Location = new System.Drawing.Point(114, 3);
            this.cmdDeleteQuality.Name = "cmdDeleteQuality";
            this.cmdDeleteQuality.Size = new System.Drawing.Size(105, 23);
            this.cmdDeleteQuality.TabIndex = 63;
            this.cmdDeleteQuality.Tag = "String_Delete";
            this.cmdDeleteQuality.Text = "Delete";
            this.cmdDeleteQuality.UseVisualStyleBackColor = true;
            this.cmdDeleteQuality.Click += new System.EventHandler(this.cmdDeleteQuality_Click);
            // 
            // nudQualityLevel
            // 
            this.nudQualityLevel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.nudQualityLevel.AutoSize = true;
            this.nudQualityLevel.Location = new System.Drawing.Point(80, 32);
            this.nudQualityLevel.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudQualityLevel.Name = "nudQualityLevel";
            this.nudQualityLevel.Size = new System.Drawing.Size(41, 20);
            this.nudQualityLevel.TabIndex = 97;
            this.nudQualityLevel.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudQualityLevel.ValueChanged += new System.EventHandler(this.nudQualityLevel_ValueChanged);
            // 
            // tlpCommonLeftSideBottom
            // 
            this.tlpCommonLeftSideBottom.AutoSize = true;
            this.tlpCommonLeftSideBottom.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpCommonLeftSideBottom.ColumnCount = 4;
            this.tlpCommonLeftSide.SetColumnSpan(this.tlpCommonLeftSideBottom, 2);
            this.tlpCommonLeftSideBottom.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tlpCommonLeftSideBottom.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 30F));
            this.tlpCommonLeftSideBottom.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tlpCommonLeftSideBottom.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 30F));
            this.tlpCommonLeftSideBottom.Controls.Add(this.lblQualitySource, 3, 0);
            this.tlpCommonLeftSideBottom.Controls.Add(this.lblQualityBP, 1, 0);
            this.tlpCommonLeftSideBottom.Controls.Add(this.lblQualityBPLabel, 0, 0);
            this.tlpCommonLeftSideBottom.Controls.Add(this.lblQualitySourceLabel, 2, 0);
            this.tlpCommonLeftSideBottom.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpCommonLeftSideBottom.Location = new System.Drawing.Point(0, 600);
            this.tlpCommonLeftSideBottom.Margin = new System.Windows.Forms.Padding(0);
            this.tlpCommonLeftSideBottom.Name = "tlpCommonLeftSideBottom";
            this.tlpCommonLeftSideBottom.RowCount = 1;
            this.tlpCommonLeftSideBottom.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpCommonLeftSideBottom.Size = new System.Drawing.Size(301, 25);
            this.tlpCommonLeftSideBottom.TabIndex = 103;
            // 
            // lblQualitySource
            // 
            this.lblQualitySource.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblQualitySource.AutoSize = true;
            this.lblQualitySource.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblQualitySource.Location = new System.Drawing.Point(213, 6);
            this.lblQualitySource.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblQualitySource.Name = "lblQualitySource";
            this.lblQualitySource.Size = new System.Drawing.Size(47, 13);
            this.lblQualitySource.TabIndex = 65;
            this.lblQualitySource.Text = "[Source]";
            this.lblQualitySource.Visible = false;
            this.lblQualitySource.Click += new System.EventHandler(this.OpenSourceFromLabel);
            // 
            // lblQualityBP
            // 
            this.lblQualityBP.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblQualityBP.AutoSize = true;
            this.lblQualityBP.Location = new System.Drawing.Point(63, 6);
            this.lblQualityBP.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblQualityBP.Name = "lblQualityBP";
            this.lblQualityBP.Size = new System.Drawing.Size(27, 13);
            this.lblQualityBP.TabIndex = 67;
            this.lblQualityBP.Text = "[BP]";
            this.lblQualityBP.Visible = false;
            // 
            // lblQualityBPLabel
            // 
            this.lblQualityBPLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblQualityBPLabel.AutoSize = true;
            this.lblQualityBPLabel.Location = new System.Drawing.Point(17, 6);
            this.lblQualityBPLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblQualityBPLabel.Name = "lblQualityBPLabel";
            this.lblQualityBPLabel.Size = new System.Drawing.Size(40, 13);
            this.lblQualityBPLabel.TabIndex = 66;
            this.lblQualityBPLabel.Tag = "Label_Karma";
            this.lblQualityBPLabel.Text = "Karma:";
            this.lblQualityBPLabel.Visible = false;
            // 
            // lblQualitySourceLabel
            // 
            this.lblQualitySourceLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblQualitySourceLabel.AutoSize = true;
            this.lblQualitySourceLabel.Location = new System.Drawing.Point(163, 6);
            this.lblQualitySourceLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblQualitySourceLabel.Name = "lblQualitySourceLabel";
            this.lblQualitySourceLabel.Size = new System.Drawing.Size(44, 13);
            this.lblQualitySourceLabel.TabIndex = 64;
            this.lblQualitySourceLabel.Tag = "Label_Source";
            this.lblQualitySourceLabel.Text = "Source:";
            this.lblQualitySourceLabel.Visible = false;
            // 
            // pnlAttributes
            // 
            this.pnlAttributes.AutoScroll = true;
            this.pnlAttributes.AutoSize = true;
            this.pnlAttributes.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpCommon.SetColumnSpan(this.pnlAttributes, 2);
            this.pnlAttributes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlAttributes.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.pnlAttributes.Location = new System.Drawing.Point(304, 53);
            this.pnlAttributes.Name = "pnlAttributes";
            this.pnlAttributes.Size = new System.Drawing.Size(370, 569);
            this.pnlAttributes.TabIndex = 96;
            this.pnlAttributes.WrapContents = false;
            this.pnlAttributes.Layout += new System.Windows.Forms.LayoutEventHandler(this.pnlAttributes_Layout);
            // 
            // flpAttributesLabels
            // 
            this.flpAttributesLabels.AutoSize = true;
            this.flpAttributesLabels.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flpAttributesLabels.Controls.Add(this.lblAttributesMetatype);
            this.flpAttributesLabels.Controls.Add(this.lblAttributesAug);
            this.flpAttributesLabels.Controls.Add(this.lblKarma);
            this.flpAttributesLabels.Controls.Add(this.lblAttributesBase);
            this.flpAttributesLabels.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.flpAttributesLabels.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.flpAttributesLabels.Location = new System.Drawing.Point(358, 26);
            this.flpAttributesLabels.Margin = new System.Windows.Forms.Padding(0, 0, 3, 0);
            this.flpAttributesLabels.Name = "flpAttributesLabels";
            this.flpAttributesLabels.Size = new System.Drawing.Size(316, 24);
            this.flpAttributesLabels.TabIndex = 102;
            this.flpAttributesLabels.WrapContents = false;
            // 
            // tlpAlias
            // 
            this.tlpAlias.AutoSize = true;
            this.tlpAlias.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpAlias.ColumnCount = 2;
            this.tlpCommon.SetColumnSpan(this.tlpAlias, 2);
            this.tlpAlias.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpAlias.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpAlias.Controls.Add(this.lblAlias, 0, 0);
            this.tlpAlias.Controls.Add(this.txtAlias, 1, 0);
            this.tlpAlias.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpAlias.Location = new System.Drawing.Point(301, 0);
            this.tlpAlias.Margin = new System.Windows.Forms.Padding(0);
            this.tlpAlias.Name = "tlpAlias";
            this.tlpAlias.RowCount = 1;
            this.tlpAlias.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpAlias.Size = new System.Drawing.Size(376, 26);
            this.tlpAlias.TabIndex = 103;
            // 
            // lblAlias
            // 
            this.lblAlias.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblAlias.AutoSize = true;
            this.lblAlias.Location = new System.Drawing.Point(3, 6);
            this.lblAlias.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblAlias.Name = "lblAlias";
            this.lblAlias.Size = new System.Drawing.Size(32, 13);
            this.lblAlias.TabIndex = 90;
            this.lblAlias.Tag = "Label_Alias";
            this.lblAlias.Text = "Alias:";
            this.lblAlias.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtAlias
            // 
            this.txtAlias.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.txtAlias.Location = new System.Drawing.Point(41, 3);
            this.txtAlias.Name = "txtAlias";
            this.txtAlias.Size = new System.Drawing.Size(332, 20);
            this.txtAlias.TabIndex = 91;
            // 
            // tlpCommonRightSide
            // 
            this.tlpCommonRightSide.AutoSize = true;
            this.tlpCommonRightSide.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpCommonRightSide.ColumnCount = 2;
            this.tlpCommonRightSide.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpCommonRightSide.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpCommonRightSide.Controls.Add(this.lblMetatypeLabel, 0, 0);
            this.tlpCommonRightSide.Controls.Add(this.lblStolenNuyen, 1, 3);
            this.tlpCommonRightSide.Controls.Add(this.lblMysticAdeptAssignment, 0, 5);
            this.tlpCommonRightSide.Controls.Add(this.nudMysticAdeptMAGMagician, 1, 5);
            this.tlpCommonRightSide.Controls.Add(this.lblMetatype, 1, 0);
            this.tlpCommonRightSide.Controls.Add(this.lblStolenNuyenLabel, 0, 3);
            this.tlpCommonRightSide.Controls.Add(this.lblMetatypeSourceLabel, 0, 1);
            this.tlpCommonRightSide.Controls.Add(this.lblMetatypeSource, 1, 1);
            this.tlpCommonRightSide.Controls.Add(this.lblNuyen, 0, 2);
            this.tlpCommonRightSide.Controls.Add(this.tlpNuyen, 1, 2);
            this.tlpCommonRightSide.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpCommonRightSide.Location = new System.Drawing.Point(677, 0);
            this.tlpCommonRightSide.Margin = new System.Windows.Forms.Padding(0);
            this.tlpCommonRightSide.Name = "tlpCommonRightSide";
            this.tlpCommonRightSide.RowCount = 7;
            this.tlpCommon.SetRowSpan(this.tlpCommonRightSide, 3);
            this.tlpCommonRightSide.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpCommonRightSide.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpCommonRightSide.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpCommonRightSide.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpCommonRightSide.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.tlpCommonRightSide.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpCommonRightSide.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpCommonRightSide.Size = new System.Drawing.Size(294, 625);
            this.tlpCommonRightSide.TabIndex = 104;
            // 
            // lblMetatypeLabel
            // 
            this.lblMetatypeLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblMetatypeLabel.AutoSize = true;
            this.lblMetatypeLabel.Location = new System.Drawing.Point(90, 6);
            this.lblMetatypeLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblMetatypeLabel.Name = "lblMetatypeLabel";
            this.lblMetatypeLabel.Size = new System.Drawing.Size(54, 13);
            this.lblMetatypeLabel.TabIndex = 19;
            this.lblMetatypeLabel.Tag = "Label_Metatype";
            this.lblMetatypeLabel.Text = "Metatype:";
            this.lblMetatypeLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblStolenNuyen
            // 
            this.lblStolenNuyen.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblStolenNuyen.AutoSize = true;
            this.lblStolenNuyen.Location = new System.Drawing.Point(150, 82);
            this.lblStolenNuyen.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblStolenNuyen.Name = "lblStolenNuyen";
            this.lblStolenNuyen.Size = new System.Drawing.Size(19, 13);
            this.lblStolenNuyen.TabIndex = 19;
            this.lblStolenNuyen.Tag = "Label_Nuyen";
            this.lblStolenNuyen.Text = "0¥";
            this.lblStolenNuyen.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblMysticAdeptAssignment
            // 
            this.lblMysticAdeptAssignment.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblMysticAdeptAssignment.AutoSize = true;
            this.lblMysticAdeptAssignment.Location = new System.Drawing.Point(11, 207);
            this.lblMysticAdeptAssignment.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblMysticAdeptAssignment.Name = "lblMysticAdeptAssignment";
            this.lblMysticAdeptAssignment.Size = new System.Drawing.Size(133, 13);
            this.lblMysticAdeptAssignment.TabIndex = 56;
            this.lblMysticAdeptAssignment.Tag = "Label_MysticAdeptAssignment";
            this.lblMysticAdeptAssignment.Text = "Mystic Adept Power Points";
            this.lblMysticAdeptAssignment.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // nudMysticAdeptMAGMagician
            // 
            this.nudMysticAdeptMAGMagician.AutoSize = true;
            this.nudMysticAdeptMAGMagician.Dock = System.Windows.Forms.DockStyle.Left;
            this.nudMysticAdeptMAGMagician.Location = new System.Drawing.Point(150, 204);
            this.nudMysticAdeptMAGMagician.MinimumSize = new System.Drawing.Size(41, 0);
            this.nudMysticAdeptMAGMagician.Name = "nudMysticAdeptMAGMagician";
            this.nudMysticAdeptMAGMagician.Size = new System.Drawing.Size(41, 20);
            this.nudMysticAdeptMAGMagician.TabIndex = 60;
            // 
            // lblMetatype
            // 
            this.lblMetatype.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblMetatype.AutoSize = true;
            this.lblMetatype.Location = new System.Drawing.Point(150, 6);
            this.lblMetatype.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblMetatype.Name = "lblMetatype";
            this.lblMetatype.Size = new System.Drawing.Size(39, 13);
            this.lblMetatype.TabIndex = 20;
            this.lblMetatype.Text = "[None]";
            this.lblMetatype.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblStolenNuyenLabel
            // 
            this.lblStolenNuyenLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblStolenNuyenLabel.AutoSize = true;
            this.lblStolenNuyenLabel.Location = new System.Drawing.Point(64, 82);
            this.lblStolenNuyenLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblStolenNuyenLabel.Name = "lblStolenNuyenLabel";
            this.lblStolenNuyenLabel.Size = new System.Drawing.Size(80, 13);
            this.lblStolenNuyenLabel.TabIndex = 19;
            this.lblStolenNuyenLabel.Tag = "Label_StolenGearNuyen";
            this.lblStolenNuyenLabel.Text = "Nuyen (Stolen):";
            this.lblStolenNuyenLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblMetatypeSourceLabel
            // 
            this.lblMetatypeSourceLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblMetatypeSourceLabel.AutoSize = true;
            this.lblMetatypeSourceLabel.Location = new System.Drawing.Point(100, 31);
            this.lblMetatypeSourceLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblMetatypeSourceLabel.Name = "lblMetatypeSourceLabel";
            this.lblMetatypeSourceLabel.Size = new System.Drawing.Size(44, 13);
            this.lblMetatypeSourceLabel.TabIndex = 88;
            this.lblMetatypeSourceLabel.Tag = "Label_Source";
            this.lblMetatypeSourceLabel.Text = "Source:";
            this.lblMetatypeSourceLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblMetatypeSource
            // 
            this.lblMetatypeSource.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblMetatypeSource.AutoSize = true;
            this.lblMetatypeSource.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblMetatypeSource.Location = new System.Drawing.Point(150, 31);
            this.lblMetatypeSource.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblMetatypeSource.Name = "lblMetatypeSource";
            this.lblMetatypeSource.Size = new System.Drawing.Size(47, 13);
            this.lblMetatypeSource.TabIndex = 89;
            this.lblMetatypeSource.Text = "[Source]";
            this.lblMetatypeSource.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblMetatypeSource.Click += new System.EventHandler(this.OpenSourceFromLabel);
            // 
            // lblNuyen
            // 
            this.lblNuyen.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblNuyen.AutoSize = true;
            this.lblNuyen.Location = new System.Drawing.Point(103, 56);
            this.lblNuyen.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblNuyen.Name = "lblNuyen";
            this.lblNuyen.Size = new System.Drawing.Size(41, 13);
            this.lblNuyen.TabIndex = 16;
            this.lblNuyen.Tag = "Label_Nuyen";
            this.lblNuyen.Text = "Nuyen:";
            this.lblNuyen.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // tlpNuyen
            // 
            this.tlpNuyen.AutoSize = true;
            this.tlpNuyen.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpNuyen.ColumnCount = 2;
            this.tlpNuyen.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpNuyen.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpNuyen.Controls.Add(this.lblNuyenTotal, 1, 0);
            this.tlpNuyen.Controls.Add(this.nudNuyen, 0, 0);
            this.tlpNuyen.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpNuyen.Location = new System.Drawing.Point(147, 50);
            this.tlpNuyen.Margin = new System.Windows.Forms.Padding(0);
            this.tlpNuyen.Name = "tlpNuyen";
            this.tlpNuyen.RowCount = 1;
            this.tlpNuyen.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpNuyen.Size = new System.Drawing.Size(147, 26);
            this.tlpNuyen.TabIndex = 90;
            // 
            // lblNuyenTotal
            // 
            this.lblNuyenTotal.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblNuyenTotal.AutoSize = true;
            this.lblNuyenTotal.Location = new System.Drawing.Point(74, 6);
            this.lblNuyenTotal.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblNuyenTotal.Name = "lblNuyenTotal";
            this.lblNuyenTotal.Size = new System.Drawing.Size(70, 13);
            this.lblNuyenTotal.TabIndex = 18;
            this.lblNuyenTotal.Text = "= 1,000,000¥";
            // 
            // nudNuyen
            // 
            this.nudNuyen.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.nudNuyen.AutoSize = true;
            this.nudNuyen.Location = new System.Drawing.Point(3, 3);
            this.nudNuyen.Maximum = new decimal(new int[] {
            1073742,
            0,
            0,
            0});
            this.nudNuyen.Name = "nudNuyen";
            this.nudNuyen.Size = new System.Drawing.Size(65, 20);
            this.nudNuyen.TabIndex = 17;
            this.nudNuyen.Value = new decimal(new int[] {
            20,
            0,
            0,
            0});
            // 
            // tabSkills
            // 
            this.tabSkills.Controls.Add(this.tabSkillUc);
            this.tabSkills.Location = new System.Drawing.Point(4, 22);
            this.tabSkills.Name = "tabSkills";
            this.tabSkills.Size = new System.Drawing.Size(977, 631);
            this.tabSkills.TabIndex = 14;
            this.tabSkills.Tag = "Tab_Skills";
            this.tabSkills.Text = "Skills";
            this.tabSkills.UseVisualStyleBackColor = true;
            // 
            // tabSkillUc
            // 
            this.tabSkillUc.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabSkillUc.Location = new System.Drawing.Point(0, 0);
            this.tabSkillUc.Name = "tabSkillUc";
            this.tabSkillUc.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.tabSkillUc.Size = new System.Drawing.Size(977, 631);
            this.tabSkillUc.TabIndex = 0;
            this.tabSkillUc.Tag = "";
            // 
            // tabLimits
            // 
            this.tabLimits.Controls.Add(this.lmtControl);
            this.tabLimits.Location = new System.Drawing.Point(4, 22);
            this.tabLimits.Name = "tabLimits";
            this.tabLimits.Padding = new System.Windows.Forms.Padding(3);
            this.tabLimits.Size = new System.Drawing.Size(977, 631);
            this.tabLimits.TabIndex = 13;
            this.tabLimits.Tag = "Tab_Limits";
            this.tabLimits.Text = "Limits";
            // 
            // tabMartialArts
            // 
            this.tabMartialArts.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.tabMartialArts.Controls.Add(this.tlpMartialArts);
            this.tabMartialArts.Location = new System.Drawing.Point(4, 22);
            this.tabMartialArts.Name = "tabMartialArts";
            this.tabMartialArts.Padding = new System.Windows.Forms.Padding(3);
            this.tabMartialArts.Size = new System.Drawing.Size(977, 631);
            this.tabMartialArts.TabIndex = 8;
            this.tabMartialArts.Tag = "Tab_MartialArts";
            this.tabMartialArts.Text = "Martial Arts";
            // 
            // tlpMartialArts
            // 
            this.tlpMartialArts.AutoSize = true;
            this.tlpMartialArts.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpMartialArts.ColumnCount = 3;
            this.tlpMartialArts.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMartialArts.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMartialArts.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMartialArts.Controls.Add(this.treMartialArts, 0, 1);
            this.tlpMartialArts.Controls.Add(this.lblMartialArtSource, 2, 1);
            this.tlpMartialArts.Controls.Add(this.lblMartialArtSourceLabel, 1, 1);
            this.tlpMartialArts.Controls.Add(this.tlpMartialArtsButtons, 0, 0);
            this.tlpMartialArts.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpMartialArts.Location = new System.Drawing.Point(3, 3);
            this.tlpMartialArts.Name = "tlpMartialArts";
            this.tlpMartialArts.RowCount = 3;
            this.tlpMartialArts.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMartialArts.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMartialArts.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMartialArts.Size = new System.Drawing.Size(971, 625);
            this.tlpMartialArts.TabIndex = 58;
            // 
            // treMartialArts
            // 
            this.treMartialArts.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treMartialArts.HideSelection = false;
            this.treMartialArts.Location = new System.Drawing.Point(3, 32);
            this.treMartialArts.Name = "treMartialArts";
            treeNode3.Name = "treMartialArtsRoot";
            treeNode3.Tag = "Node_SelectedMartialArts";
            treeNode3.Text = "Selected Martial Arts";
            treeNode4.Name = "treQualitiesRoot";
            treeNode4.Text = "Selected Qualities";
            this.treMartialArts.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            treeNode3,
            treeNode4});
            this.tlpMartialArts.SetRowSpan(this.treMartialArts, 2);
            this.treMartialArts.ShowNodeToolTips = true;
            this.treMartialArts.ShowPlusMinus = false;
            this.treMartialArts.ShowRootLines = false;
            this.treMartialArts.Size = new System.Drawing.Size(295, 590);
            this.treMartialArts.TabIndex = 2;
            this.treMartialArts.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treMartialArts_AfterSelect);
            this.treMartialArts.KeyDown += new System.Windows.Forms.KeyEventHandler(this.treMartialArts_KeyDown);
            this.treMartialArts.MouseDown += new System.Windows.Forms.MouseEventHandler(this.TreeView_MouseDown);
            // 
            // lblMartialArtSource
            // 
            this.lblMartialArtSource.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblMartialArtSource.AutoSize = true;
            this.lblMartialArtSource.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblMartialArtSource.Location = new System.Drawing.Point(354, 35);
            this.lblMartialArtSource.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblMartialArtSource.Name = "lblMartialArtSource";
            this.lblMartialArtSource.Size = new System.Drawing.Size(47, 13);
            this.lblMartialArtSource.TabIndex = 25;
            this.lblMartialArtSource.Text = "[Source]";
            this.lblMartialArtSource.Click += new System.EventHandler(this.OpenSourceFromLabel);
            // 
            // lblMartialArtSourceLabel
            // 
            this.lblMartialArtSourceLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblMartialArtSourceLabel.AutoSize = true;
            this.lblMartialArtSourceLabel.Location = new System.Drawing.Point(304, 35);
            this.lblMartialArtSourceLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblMartialArtSourceLabel.Name = "lblMartialArtSourceLabel";
            this.lblMartialArtSourceLabel.Size = new System.Drawing.Size(44, 13);
            this.lblMartialArtSourceLabel.TabIndex = 24;
            this.lblMartialArtSourceLabel.Tag = "Label_Source";
            this.lblMartialArtSourceLabel.Text = "Source:";
            // 
            // tlpMartialArtsButtons
            // 
            this.tlpMartialArtsButtons.AutoSize = true;
            this.tlpMartialArtsButtons.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpMartialArtsButtons.ColumnCount = 2;
            this.tlpMartialArts.SetColumnSpan(this.tlpMartialArtsButtons, 3);
            this.tlpMartialArtsButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpMartialArtsButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpMartialArtsButtons.Controls.Add(this.cmdAddMartialArt, 0, 0);
            this.tlpMartialArtsButtons.Controls.Add(this.cmdDeleteMartialArt, 1, 0);
            this.tlpMartialArtsButtons.Location = new System.Drawing.Point(0, 0);
            this.tlpMartialArtsButtons.Margin = new System.Windows.Forms.Padding(0);
            this.tlpMartialArtsButtons.Name = "tlpMartialArtsButtons";
            this.tlpMartialArtsButtons.RowCount = 1;
            this.tlpMartialArtsButtons.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpMartialArtsButtons.Size = new System.Drawing.Size(220, 29);
            this.tlpMartialArtsButtons.TabIndex = 57;
            // 
            // cmdAddMartialArt
            // 
            this.cmdAddMartialArt.AutoSize = true;
            this.cmdAddMartialArt.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdAddMartialArt.ContextMenuStrip = this.cmsMartialArts;
            this.cmdAddMartialArt.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmdAddMartialArt.Location = new System.Drawing.Point(3, 3);
            this.cmdAddMartialArt.Name = "cmdAddMartialArt";
            this.cmdAddMartialArt.Size = new System.Drawing.Size(104, 23);
            this.cmdAddMartialArt.SplitMenuStrip = this.cmsMartialArts;
            this.cmdAddMartialArt.TabIndex = 55;
            this.cmdAddMartialArt.Tag = "Button_AddMartialArt";
            this.cmdAddMartialArt.Text = "&Add Martial Art";
            this.cmdAddMartialArt.UseVisualStyleBackColor = true;
            this.cmdAddMartialArt.Click += new System.EventHandler(this.cmdAddMartialArt_Click);
            // 
            // cmdDeleteMartialArt
            // 
            this.cmdDeleteMartialArt.AutoSize = true;
            this.cmdDeleteMartialArt.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdDeleteMartialArt.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmdDeleteMartialArt.Location = new System.Drawing.Point(113, 3);
            this.cmdDeleteMartialArt.Name = "cmdDeleteMartialArt";
            this.cmdDeleteMartialArt.Size = new System.Drawing.Size(104, 23);
            this.cmdDeleteMartialArt.TabIndex = 1;
            this.cmdDeleteMartialArt.Tag = "String_Delete";
            this.cmdDeleteMartialArt.Text = "Delete";
            this.cmdDeleteMartialArt.UseVisualStyleBackColor = true;
            this.cmdDeleteMartialArt.Click += new System.EventHandler(this.cmdDeleteMartialArt_Click);
            // 
            // tabMagician
            // 
            this.tabMagician.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.tabMagician.Controls.Add(this.tlpMagician);
            this.tabMagician.Location = new System.Drawing.Point(4, 22);
            this.tabMagician.Name = "tabMagician";
            this.tabMagician.Padding = new System.Windows.Forms.Padding(3);
            this.tabMagician.Size = new System.Drawing.Size(977, 631);
            this.tabMagician.TabIndex = 1;
            this.tabMagician.Tag = "Tab_Magician";
            this.tabMagician.Text = "Spells and Spirits";
            // 
            // tlpMagician
            // 
            this.tlpMagician.ColumnCount = 2;
            this.tlpMagician.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMagician.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMagician.Controls.Add(this.cmdAddSpirit, 0, 2);
            this.tlpMagician.Controls.Add(this.panSpirits, 0, 3);
            this.tlpMagician.Controls.Add(this.treSpells, 0, 1);
            this.tlpMagician.Controls.Add(this.flpMagician, 1, 1);
            this.tlpMagician.Controls.Add(this.tlpMagicianButtons, 0, 0);
            this.tlpMagician.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpMagician.Location = new System.Drawing.Point(3, 3);
            this.tlpMagician.Name = "tlpMagician";
            this.tlpMagician.RowCount = 4;
            this.tlpMagician.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMagician.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMagician.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMagician.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMagician.Size = new System.Drawing.Size(971, 625);
            this.tlpMagician.TabIndex = 157;
            // 
            // cmdAddSpirit
            // 
            this.cmdAddSpirit.AutoSize = true;
            this.cmdAddSpirit.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdAddSpirit.Location = new System.Drawing.Point(3, 417);
            this.cmdAddSpirit.Name = "cmdAddSpirit";
            this.cmdAddSpirit.Size = new System.Drawing.Size(62, 23);
            this.cmdAddSpirit.TabIndex = 68;
            this.cmdAddSpirit.Tag = "Button_AddSpirit";
            this.cmdAddSpirit.Text = "A&dd Spirit";
            this.cmdAddSpirit.UseVisualStyleBackColor = true;
            this.cmdAddSpirit.Click += new System.EventHandler(this.cmdAddSpirit_Click);
            // 
            // panSpirits
            // 
            this.panSpirits.AutoScroll = true;
            this.panSpirits.AutoSize = true;
            this.panSpirits.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpMagician.SetColumnSpan(this.panSpirits, 2);
            this.panSpirits.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panSpirits.Location = new System.Drawing.Point(3, 446);
            this.panSpirits.Name = "panSpirits";
            this.panSpirits.Size = new System.Drawing.Size(965, 176);
            this.panSpirits.TabIndex = 4;
            // 
            // treSpells
            // 
            this.treSpells.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treSpells.HideSelection = false;
            this.treSpells.Location = new System.Drawing.Point(3, 32);
            this.treSpells.Name = "treSpells";
            treeNode5.Name = "nodSpellCombatRoot";
            treeNode5.Tag = "Node_SelectedCombatSpells";
            treeNode5.Text = "Selected Combat Spells";
            treeNode6.Name = "nodSpellDetectionRoot";
            treeNode6.Tag = "Node_SelectedDetectionSpells";
            treeNode6.Text = "Selected Detection Spells";
            treeNode7.Name = "nodSpellHealthRoot";
            treeNode7.Tag = "Node_SelectedHealthSpells";
            treeNode7.Text = "Selected Health Spells";
            treeNode8.Name = "nodSpellIllusionRoot";
            treeNode8.Tag = "Node_SelectedIllusionSpells";
            treeNode8.Text = "Selected Illusion Spells";
            treeNode9.Name = "nodSpellManipulationRoot";
            treeNode9.Tag = "Node_SelectedManipulationSpells";
            treeNode9.Text = "Selected Manipulation Spells";
            treeNode10.Name = "nodSpellGeomancyRoot";
            treeNode10.Tag = "Node_SelectedGeomancyRituals";
            treeNode10.Text = "Selected Rituals";
            treeNode11.Name = "nodSpellEnchantmentRoot";
            treeNode11.Tag = "Node_SelectedEnchantments";
            treeNode11.Text = "Selected Enchantments";
            this.treSpells.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            treeNode5,
            treeNode6,
            treeNode7,
            treeNode8,
            treeNode9,
            treeNode10,
            treeNode11});
            this.treSpells.ShowNodeToolTips = true;
            this.treSpells.ShowRootLines = false;
            this.treSpells.Size = new System.Drawing.Size(295, 379);
            this.treSpells.TabIndex = 70;
            this.treSpells.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treSpells_AfterSelect);
            this.treSpells.KeyDown += new System.Windows.Forms.KeyEventHandler(this.treSpells_KeyDown);
            this.treSpells.MouseDown += new System.Windows.Forms.MouseEventHandler(this.TreeView_MouseDown);
            // 
            // flpMagician
            // 
            this.flpMagician.AutoScroll = true;
            this.flpMagician.Controls.Add(this.gpbMagicianSpell);
            this.flpMagician.Controls.Add(this.gpbMagicianTradition);
            this.flpMagician.Controls.Add(this.gpbMagicianMentorSpirit);
            this.flpMagician.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flpMagician.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flpMagician.Location = new System.Drawing.Point(301, 29);
            this.flpMagician.Margin = new System.Windows.Forms.Padding(0);
            this.flpMagician.Name = "flpMagician";
            this.flpMagician.Size = new System.Drawing.Size(670, 385);
            this.flpMagician.TabIndex = 157;
            this.flpMagician.WrapContents = false;
            // 
            // gpbMagicianSpell
            // 
            this.gpbMagicianSpell.AutoSize = true;
            this.gpbMagicianSpell.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.gpbMagicianSpell.Controls.Add(this.tlpMagicianSpell);
            this.gpbMagicianSpell.Dock = System.Windows.Forms.DockStyle.Top;
            this.gpbMagicianSpell.Location = new System.Drawing.Point(3, 3);
            this.gpbMagicianSpell.Name = "gpbMagicianSpell";
            this.gpbMagicianSpell.Size = new System.Drawing.Size(561, 144);
            this.gpbMagicianSpell.TabIndex = 0;
            this.gpbMagicianSpell.TabStop = false;
            this.gpbMagicianSpell.Text = "Spell";
            this.gpbMagicianSpell.Visible = false;
            // 
            // tlpMagicianSpell
            // 
            this.tlpMagicianSpell.AutoSize = true;
            this.tlpMagicianSpell.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpMagicianSpell.ColumnCount = 4;
            this.tlpMagicianSpell.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMagicianSpell.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpMagicianSpell.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMagicianSpell.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpMagicianSpell.Controls.Add(this.lblSpellDescriptorsLabel, 0, 0);
            this.tlpMagicianSpell.Controls.Add(this.lblSpellDescriptors, 1, 0);
            this.tlpMagicianSpell.Controls.Add(this.lblSpellTypeLabel, 2, 0);
            this.tlpMagicianSpell.Controls.Add(this.lblSpellDV, 3, 2);
            this.tlpMagicianSpell.Controls.Add(this.lblSpellType, 3, 0);
            this.tlpMagicianSpell.Controls.Add(this.lblSpellDamageLabel, 2, 1);
            this.tlpMagicianSpell.Controls.Add(this.lblSpellDamage, 3, 1);
            this.tlpMagicianSpell.Controls.Add(this.lblSpellCategoryLabel, 0, 1);
            this.tlpMagicianSpell.Controls.Add(this.lblSpellCategory, 1, 1);
            this.tlpMagicianSpell.Controls.Add(this.lblSpellRangeLabel, 0, 2);
            this.tlpMagicianSpell.Controls.Add(this.lblSpellRange, 1, 2);
            this.tlpMagicianSpell.Controls.Add(this.lblSpellDVLabel, 2, 2);
            this.tlpMagicianSpell.Controls.Add(this.lblSpellDuration, 3, 3);
            this.tlpMagicianSpell.Controls.Add(this.lblSpellDurationLabel, 2, 3);
            this.tlpMagicianSpell.Controls.Add(this.lblSpellDicePoolLabel, 0, 3);
            this.tlpMagicianSpell.Controls.Add(this.lblSpellDicePool, 1, 3);
            this.tlpMagicianSpell.Controls.Add(this.lblSpellSource, 1, 4);
            this.tlpMagicianSpell.Controls.Add(this.lblSpellSourceLabel, 0, 4);
            this.tlpMagicianSpell.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpMagicianSpell.Location = new System.Drawing.Point(3, 16);
            this.tlpMagicianSpell.Name = "tlpMagicianSpell";
            this.tlpMagicianSpell.RowCount = 5;
            this.tlpMagicianSpell.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMagicianSpell.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMagicianSpell.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMagicianSpell.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMagicianSpell.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMagicianSpell.Size = new System.Drawing.Size(555, 125);
            this.tlpMagicianSpell.TabIndex = 0;
            // 
            // lblSpellDescriptorsLabel
            // 
            this.lblSpellDescriptorsLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblSpellDescriptorsLabel.AutoSize = true;
            this.lblSpellDescriptorsLabel.Location = new System.Drawing.Point(3, 6);
            this.lblSpellDescriptorsLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSpellDescriptorsLabel.Name = "lblSpellDescriptorsLabel";
            this.lblSpellDescriptorsLabel.Size = new System.Drawing.Size(63, 13);
            this.lblSpellDescriptorsLabel.TabIndex = 71;
            this.lblSpellDescriptorsLabel.Tag = "Label_Descriptors";
            this.lblSpellDescriptorsLabel.Text = "Descriptors:";
            // 
            // lblSpellDescriptors
            // 
            this.lblSpellDescriptors.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblSpellDescriptors.AutoSize = true;
            this.lblSpellDescriptors.Location = new System.Drawing.Point(72, 6);
            this.lblSpellDescriptors.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSpellDescriptors.Name = "lblSpellDescriptors";
            this.lblSpellDescriptors.Size = new System.Drawing.Size(66, 13);
            this.lblSpellDescriptors.TabIndex = 72;
            this.lblSpellDescriptors.Text = "[Descriptors]";
            // 
            // lblSpellTypeLabel
            // 
            this.lblSpellTypeLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblSpellTypeLabel.AutoSize = true;
            this.lblSpellTypeLabel.Location = new System.Drawing.Point(303, 6);
            this.lblSpellTypeLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSpellTypeLabel.Name = "lblSpellTypeLabel";
            this.lblSpellTypeLabel.Size = new System.Drawing.Size(34, 13);
            this.lblSpellTypeLabel.TabIndex = 83;
            this.lblSpellTypeLabel.Tag = "Label_Type";
            this.lblSpellTypeLabel.Text = "Type:";
            // 
            // lblSpellDV
            // 
            this.lblSpellDV.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblSpellDV.AutoSize = true;
            this.lblSpellDV.Location = new System.Drawing.Point(343, 56);
            this.lblSpellDV.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSpellDV.Name = "lblSpellDV";
            this.lblSpellDV.Size = new System.Drawing.Size(28, 13);
            this.lblSpellDV.TabIndex = 82;
            this.lblSpellDV.Text = "[DV]";
            // 
            // lblSpellType
            // 
            this.lblSpellType.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblSpellType.AutoSize = true;
            this.lblSpellType.Location = new System.Drawing.Point(343, 6);
            this.lblSpellType.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSpellType.Name = "lblSpellType";
            this.lblSpellType.Size = new System.Drawing.Size(37, 13);
            this.lblSpellType.TabIndex = 84;
            this.lblSpellType.Text = "[Type]";
            // 
            // lblSpellDamageLabel
            // 
            this.lblSpellDamageLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblSpellDamageLabel.AutoSize = true;
            this.lblSpellDamageLabel.Location = new System.Drawing.Point(287, 31);
            this.lblSpellDamageLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSpellDamageLabel.Name = "lblSpellDamageLabel";
            this.lblSpellDamageLabel.Size = new System.Drawing.Size(50, 13);
            this.lblSpellDamageLabel.TabIndex = 77;
            this.lblSpellDamageLabel.Tag = "Label_Damage";
            this.lblSpellDamageLabel.Text = "Damage:";
            // 
            // lblSpellDamage
            // 
            this.lblSpellDamage.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblSpellDamage.AutoSize = true;
            this.lblSpellDamage.Location = new System.Drawing.Point(343, 31);
            this.lblSpellDamage.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSpellDamage.Name = "lblSpellDamage";
            this.lblSpellDamage.Size = new System.Drawing.Size(53, 13);
            this.lblSpellDamage.TabIndex = 78;
            this.lblSpellDamage.Text = "[Damage]";
            // 
            // lblSpellCategoryLabel
            // 
            this.lblSpellCategoryLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblSpellCategoryLabel.AutoSize = true;
            this.lblSpellCategoryLabel.Location = new System.Drawing.Point(14, 31);
            this.lblSpellCategoryLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSpellCategoryLabel.Name = "lblSpellCategoryLabel";
            this.lblSpellCategoryLabel.Size = new System.Drawing.Size(52, 13);
            this.lblSpellCategoryLabel.TabIndex = 73;
            this.lblSpellCategoryLabel.Tag = "Label_Category";
            this.lblSpellCategoryLabel.Text = "Category:";
            // 
            // lblSpellCategory
            // 
            this.lblSpellCategory.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblSpellCategory.AutoSize = true;
            this.lblSpellCategory.Location = new System.Drawing.Point(72, 31);
            this.lblSpellCategory.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSpellCategory.Name = "lblSpellCategory";
            this.lblSpellCategory.Size = new System.Drawing.Size(55, 13);
            this.lblSpellCategory.TabIndex = 74;
            this.lblSpellCategory.Text = "[Category]";
            // 
            // lblSpellRangeLabel
            // 
            this.lblSpellRangeLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblSpellRangeLabel.AutoSize = true;
            this.lblSpellRangeLabel.Location = new System.Drawing.Point(24, 56);
            this.lblSpellRangeLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSpellRangeLabel.Name = "lblSpellRangeLabel";
            this.lblSpellRangeLabel.Size = new System.Drawing.Size(42, 13);
            this.lblSpellRangeLabel.TabIndex = 75;
            this.lblSpellRangeLabel.Tag = "Label_Range";
            this.lblSpellRangeLabel.Text = "Range:";
            // 
            // lblSpellRange
            // 
            this.lblSpellRange.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblSpellRange.AutoSize = true;
            this.lblSpellRange.Location = new System.Drawing.Point(72, 56);
            this.lblSpellRange.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSpellRange.Name = "lblSpellRange";
            this.lblSpellRange.Size = new System.Drawing.Size(45, 13);
            this.lblSpellRange.TabIndex = 76;
            this.lblSpellRange.Text = "[Range]";
            // 
            // lblSpellDVLabel
            // 
            this.lblSpellDVLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblSpellDVLabel.AutoSize = true;
            this.lblSpellDVLabel.Location = new System.Drawing.Point(312, 56);
            this.lblSpellDVLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSpellDVLabel.Name = "lblSpellDVLabel";
            this.lblSpellDVLabel.Size = new System.Drawing.Size(25, 13);
            this.lblSpellDVLabel.TabIndex = 81;
            this.lblSpellDVLabel.Tag = "Label_DV";
            this.lblSpellDVLabel.Text = "DV:";
            // 
            // lblSpellDuration
            // 
            this.lblSpellDuration.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblSpellDuration.AutoSize = true;
            this.lblSpellDuration.Location = new System.Drawing.Point(343, 81);
            this.lblSpellDuration.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSpellDuration.Name = "lblSpellDuration";
            this.lblSpellDuration.Size = new System.Drawing.Size(53, 13);
            this.lblSpellDuration.TabIndex = 80;
            this.lblSpellDuration.Text = "[Duration]";
            // 
            // lblSpellDurationLabel
            // 
            this.lblSpellDurationLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblSpellDurationLabel.AutoSize = true;
            this.lblSpellDurationLabel.Location = new System.Drawing.Point(287, 81);
            this.lblSpellDurationLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSpellDurationLabel.Name = "lblSpellDurationLabel";
            this.lblSpellDurationLabel.Size = new System.Drawing.Size(50, 13);
            this.lblSpellDurationLabel.TabIndex = 79;
            this.lblSpellDurationLabel.Tag = "Label_Duration";
            this.lblSpellDurationLabel.Text = "Duration:";
            // 
            // lblSpellDicePoolLabel
            // 
            this.lblSpellDicePoolLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblSpellDicePoolLabel.AutoSize = true;
            this.lblSpellDicePoolLabel.Location = new System.Drawing.Point(10, 81);
            this.lblSpellDicePoolLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSpellDicePoolLabel.Name = "lblSpellDicePoolLabel";
            this.lblSpellDicePoolLabel.Size = new System.Drawing.Size(56, 13);
            this.lblSpellDicePoolLabel.TabIndex = 106;
            this.lblSpellDicePoolLabel.Tag = "Label_DicePool";
            this.lblSpellDicePoolLabel.Text = "Dice Pool:";
            // 
            // lblSpellDicePool
            // 
            this.lblSpellDicePool.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblSpellDicePool.AutoSize = true;
            this.lblSpellDicePool.Location = new System.Drawing.Point(72, 81);
            this.lblSpellDicePool.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSpellDicePool.Name = "lblSpellDicePool";
            this.lblSpellDicePool.Size = new System.Drawing.Size(59, 13);
            this.lblSpellDicePool.TabIndex = 107;
            this.lblSpellDicePool.Text = "[Dice Pool]";
            // 
            // lblSpellSource
            // 
            this.lblSpellSource.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblSpellSource.AutoSize = true;
            this.lblSpellSource.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblSpellSource.Location = new System.Drawing.Point(72, 106);
            this.lblSpellSource.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSpellSource.Name = "lblSpellSource";
            this.lblSpellSource.Size = new System.Drawing.Size(47, 13);
            this.lblSpellSource.TabIndex = 88;
            this.lblSpellSource.Text = "[Source]";
            this.lblSpellSource.Click += new System.EventHandler(this.OpenSourceFromLabel);
            // 
            // lblSpellSourceLabel
            // 
            this.lblSpellSourceLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblSpellSourceLabel.AutoSize = true;
            this.lblSpellSourceLabel.Location = new System.Drawing.Point(22, 106);
            this.lblSpellSourceLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSpellSourceLabel.Name = "lblSpellSourceLabel";
            this.lblSpellSourceLabel.Size = new System.Drawing.Size(44, 13);
            this.lblSpellSourceLabel.TabIndex = 87;
            this.lblSpellSourceLabel.Tag = "Label_Source";
            this.lblSpellSourceLabel.Text = "Source:";
            // 
            // gpbMagicianTradition
            // 
            this.gpbMagicianTradition.AutoSize = true;
            this.gpbMagicianTradition.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.gpbMagicianTradition.Controls.Add(this.tlpMagicianTradition);
            this.gpbMagicianTradition.Dock = System.Windows.Forms.DockStyle.Top;
            this.gpbMagicianTradition.Location = new System.Drawing.Point(3, 153);
            this.gpbMagicianTradition.Name = "gpbMagicianTradition";
            this.gpbMagicianTradition.Size = new System.Drawing.Size(561, 181);
            this.gpbMagicianTradition.TabIndex = 1;
            this.gpbMagicianTradition.TabStop = false;
            this.gpbMagicianTradition.Tag = "String_Tradition";
            this.gpbMagicianTradition.Text = "Tradition";
            // 
            // tlpMagicianTradition
            // 
            this.tlpMagicianTradition.AutoSize = true;
            this.tlpMagicianTradition.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpMagicianTradition.ColumnCount = 4;
            this.tlpMagicianTradition.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMagicianTradition.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpMagicianTradition.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMagicianTradition.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpMagicianTradition.Controls.Add(this.lblTraditionLabel, 0, 0);
            this.tlpMagicianTradition.Controls.Add(this.cboTradition, 1, 0);
            this.tlpMagicianTradition.Controls.Add(this.lblDrainAttributesLabel, 0, 1);
            this.tlpMagicianTradition.Controls.Add(this.lblTraditionName, 2, 0);
            this.tlpMagicianTradition.Controls.Add(this.lblTraditionSource, 1, 5);
            this.tlpMagicianTradition.Controls.Add(this.cboSpiritManipulation, 3, 5);
            this.tlpMagicianTradition.Controls.Add(this.lblTraditionSourceLabel, 0, 5);
            this.tlpMagicianTradition.Controls.Add(this.txtTraditionName, 3, 0);
            this.tlpMagicianTradition.Controls.Add(this.lblSpiritCombat, 2, 1);
            this.tlpMagicianTradition.Controls.Add(this.lblSpiritManipulation, 2, 5);
            this.tlpMagicianTradition.Controls.Add(this.cboSpiritCombat, 3, 1);
            this.tlpMagicianTradition.Controls.Add(this.cboSpiritIllusion, 3, 4);
            this.tlpMagicianTradition.Controls.Add(this.lblSpiritDetection, 2, 2);
            this.tlpMagicianTradition.Controls.Add(this.cboSpiritDetection, 3, 2);
            this.tlpMagicianTradition.Controls.Add(this.lblSpiritIllusion, 2, 4);
            this.tlpMagicianTradition.Controls.Add(this.cboSpiritHealth, 3, 3);
            this.tlpMagicianTradition.Controls.Add(this.lblSpiritHealth, 2, 3);
            this.tlpMagicianTradition.Controls.Add(this.tlpDrainAttributes, 1, 1);
            this.tlpMagicianTradition.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpMagicianTradition.Location = new System.Drawing.Point(3, 16);
            this.tlpMagicianTradition.Name = "tlpMagicianTradition";
            this.tlpMagicianTradition.RowCount = 6;
            this.tlpMagicianTradition.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMagicianTradition.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMagicianTradition.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMagicianTradition.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMagicianTradition.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMagicianTradition.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMagicianTradition.Size = new System.Drawing.Size(555, 162);
            this.tlpMagicianTradition.TabIndex = 0;
            // 
            // lblTraditionLabel
            // 
            this.lblTraditionLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblTraditionLabel.AutoSize = true;
            this.lblTraditionLabel.Location = new System.Drawing.Point(41, 7);
            this.lblTraditionLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblTraditionLabel.Name = "lblTraditionLabel";
            this.lblTraditionLabel.Size = new System.Drawing.Size(51, 13);
            this.lblTraditionLabel.TabIndex = 89;
            this.lblTraditionLabel.Tag = "Label_Tradition";
            this.lblTraditionLabel.Text = "Tradition:";
            // 
            // cboTradition
            // 
            this.cboTradition.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.cboTradition.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboTradition.FormattingEnabled = true;
            this.cboTradition.Location = new System.Drawing.Point(98, 3);
            this.cboTradition.Name = "cboTradition";
            this.cboTradition.Size = new System.Drawing.Size(186, 21);
            this.cboTradition.TabIndex = 90;
            this.cboTradition.TooltipText = "";
            this.cboTradition.SelectedIndexChanged += new System.EventHandler(this.cboTradition_SelectedIndexChanged);
            // 
            // lblDrainAttributesLabel
            // 
            this.lblDrainAttributesLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblDrainAttributesLabel.AutoSize = true;
            this.lblDrainAttributesLabel.Location = new System.Drawing.Point(3, 34);
            this.lblDrainAttributesLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblDrainAttributesLabel.Name = "lblDrainAttributesLabel";
            this.lblDrainAttributesLabel.Size = new System.Drawing.Size(89, 13);
            this.lblDrainAttributesLabel.TabIndex = 91;
            this.lblDrainAttributesLabel.Tag = "Label_ResistDrain";
            this.lblDrainAttributesLabel.Text = "Resist Drain with:";
            // 
            // lblTraditionName
            // 
            this.lblTraditionName.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblTraditionName.AutoSize = true;
            this.lblTraditionName.Location = new System.Drawing.Point(322, 7);
            this.lblTraditionName.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblTraditionName.Name = "lblTraditionName";
            this.lblTraditionName.Size = new System.Drawing.Size(38, 13);
            this.lblTraditionName.TabIndex = 141;
            this.lblTraditionName.Tag = "Label_TraditionName";
            this.lblTraditionName.Text = "Name:";
            this.lblTraditionName.Visible = false;
            // 
            // lblTraditionSource
            // 
            this.lblTraditionSource.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblTraditionSource.AutoSize = true;
            this.lblTraditionSource.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblTraditionSource.Location = new System.Drawing.Point(98, 142);
            this.lblTraditionSource.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblTraditionSource.Name = "lblTraditionSource";
            this.lblTraditionSource.Size = new System.Drawing.Size(47, 13);
            this.lblTraditionSource.TabIndex = 155;
            this.lblTraditionSource.Text = "[Source]";
            this.lblTraditionSource.Click += new System.EventHandler(this.OpenSourceFromLabel);
            // 
            // cboSpiritManipulation
            // 
            this.cboSpiritManipulation.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.cboSpiritManipulation.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboSpiritManipulation.FormattingEnabled = true;
            this.cboSpiritManipulation.Location = new System.Drawing.Point(366, 138);
            this.cboSpiritManipulation.Name = "cboSpiritManipulation";
            this.cboSpiritManipulation.Size = new System.Drawing.Size(186, 21);
            this.cboSpiritManipulation.TabIndex = 153;
            this.cboSpiritManipulation.TooltipText = "";
            this.cboSpiritManipulation.Visible = false;
            // 
            // lblTraditionSourceLabel
            // 
            this.lblTraditionSourceLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblTraditionSourceLabel.AutoSize = true;
            this.lblTraditionSourceLabel.Location = new System.Drawing.Point(48, 142);
            this.lblTraditionSourceLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblTraditionSourceLabel.Name = "lblTraditionSourceLabel";
            this.lblTraditionSourceLabel.Size = new System.Drawing.Size(44, 13);
            this.lblTraditionSourceLabel.TabIndex = 154;
            this.lblTraditionSourceLabel.Tag = "Label_Source";
            this.lblTraditionSourceLabel.Text = "Source:";
            // 
            // txtTraditionName
            // 
            this.txtTraditionName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.txtTraditionName.Location = new System.Drawing.Point(366, 3);
            this.txtTraditionName.Name = "txtTraditionName";
            this.txtTraditionName.Size = new System.Drawing.Size(186, 20);
            this.txtTraditionName.TabIndex = 142;
            this.txtTraditionName.Visible = false;
            // 
            // lblSpiritCombat
            // 
            this.lblSpiritCombat.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblSpiritCombat.AutoSize = true;
            this.lblSpiritCombat.Location = new System.Drawing.Point(314, 34);
            this.lblSpiritCombat.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSpiritCombat.Name = "lblSpiritCombat";
            this.lblSpiritCombat.Size = new System.Drawing.Size(46, 13);
            this.lblSpiritCombat.TabIndex = 144;
            this.lblSpiritCombat.Tag = "Label_SpiritCombat";
            this.lblSpiritCombat.Text = "Combat:";
            this.lblSpiritCombat.Visible = false;
            // 
            // lblSpiritManipulation
            // 
            this.lblSpiritManipulation.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblSpiritManipulation.AutoSize = true;
            this.lblSpiritManipulation.Location = new System.Drawing.Point(290, 142);
            this.lblSpiritManipulation.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSpiritManipulation.Name = "lblSpiritManipulation";
            this.lblSpiritManipulation.Size = new System.Drawing.Size(70, 13);
            this.lblSpiritManipulation.TabIndex = 152;
            this.lblSpiritManipulation.Tag = "Label_SpiritManipulation";
            this.lblSpiritManipulation.Text = "Manipulation:";
            this.lblSpiritManipulation.Visible = false;
            // 
            // cboSpiritCombat
            // 
            this.cboSpiritCombat.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.cboSpiritCombat.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboSpiritCombat.FormattingEnabled = true;
            this.cboSpiritCombat.Location = new System.Drawing.Point(366, 30);
            this.cboSpiritCombat.Name = "cboSpiritCombat";
            this.cboSpiritCombat.Size = new System.Drawing.Size(186, 21);
            this.cboSpiritCombat.TabIndex = 145;
            this.cboSpiritCombat.TooltipText = "";
            this.cboSpiritCombat.Visible = false;
            // 
            // cboSpiritIllusion
            // 
            this.cboSpiritIllusion.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.cboSpiritIllusion.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboSpiritIllusion.FormattingEnabled = true;
            this.cboSpiritIllusion.Location = new System.Drawing.Point(366, 111);
            this.cboSpiritIllusion.Name = "cboSpiritIllusion";
            this.cboSpiritIllusion.Size = new System.Drawing.Size(186, 21);
            this.cboSpiritIllusion.TabIndex = 151;
            this.cboSpiritIllusion.TooltipText = "";
            this.cboSpiritIllusion.Visible = false;
            // 
            // lblSpiritDetection
            // 
            this.lblSpiritDetection.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblSpiritDetection.AutoSize = true;
            this.lblSpiritDetection.Location = new System.Drawing.Point(304, 61);
            this.lblSpiritDetection.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSpiritDetection.Name = "lblSpiritDetection";
            this.lblSpiritDetection.Size = new System.Drawing.Size(56, 13);
            this.lblSpiritDetection.TabIndex = 146;
            this.lblSpiritDetection.Tag = "Label_SpiritDetection";
            this.lblSpiritDetection.Text = "Detection:";
            this.lblSpiritDetection.Visible = false;
            // 
            // cboSpiritDetection
            // 
            this.cboSpiritDetection.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.cboSpiritDetection.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboSpiritDetection.FormattingEnabled = true;
            this.cboSpiritDetection.Location = new System.Drawing.Point(366, 57);
            this.cboSpiritDetection.Name = "cboSpiritDetection";
            this.cboSpiritDetection.Size = new System.Drawing.Size(186, 21);
            this.cboSpiritDetection.TabIndex = 147;
            this.cboSpiritDetection.TooltipText = "";
            this.cboSpiritDetection.Visible = false;
            // 
            // lblSpiritIllusion
            // 
            this.lblSpiritIllusion.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblSpiritIllusion.AutoSize = true;
            this.lblSpiritIllusion.Location = new System.Drawing.Point(318, 115);
            this.lblSpiritIllusion.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSpiritIllusion.Name = "lblSpiritIllusion";
            this.lblSpiritIllusion.Size = new System.Drawing.Size(42, 13);
            this.lblSpiritIllusion.TabIndex = 150;
            this.lblSpiritIllusion.Tag = "Label_SpiritIllusion";
            this.lblSpiritIllusion.Text = "Illusion:";
            this.lblSpiritIllusion.Visible = false;
            // 
            // cboSpiritHealth
            // 
            this.cboSpiritHealth.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.cboSpiritHealth.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboSpiritHealth.FormattingEnabled = true;
            this.cboSpiritHealth.Location = new System.Drawing.Point(366, 84);
            this.cboSpiritHealth.Name = "cboSpiritHealth";
            this.cboSpiritHealth.Size = new System.Drawing.Size(186, 21);
            this.cboSpiritHealth.TabIndex = 149;
            this.cboSpiritHealth.TooltipText = "";
            this.cboSpiritHealth.Visible = false;
            // 
            // lblSpiritHealth
            // 
            this.lblSpiritHealth.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblSpiritHealth.AutoSize = true;
            this.lblSpiritHealth.Location = new System.Drawing.Point(319, 88);
            this.lblSpiritHealth.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSpiritHealth.Name = "lblSpiritHealth";
            this.lblSpiritHealth.Size = new System.Drawing.Size(41, 13);
            this.lblSpiritHealth.TabIndex = 148;
            this.lblSpiritHealth.Tag = "Label_SpiritHealth";
            this.lblSpiritHealth.Text = "Health:";
            this.lblSpiritHealth.Visible = false;
            // 
            // tlpDrainAttributes
            // 
            this.tlpDrainAttributes.AutoSize = true;
            this.tlpDrainAttributes.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpDrainAttributes.ColumnCount = 3;
            this.tlpDrainAttributes.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpDrainAttributes.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpDrainAttributes.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpDrainAttributes.Controls.Add(this.lblDrainAttributesValue, 2, 0);
            this.tlpDrainAttributes.Controls.Add(this.lblDrainAttributes, 1, 0);
            this.tlpDrainAttributes.Controls.Add(this.cboDrain, 0, 0);
            this.tlpDrainAttributes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpDrainAttributes.Location = new System.Drawing.Point(95, 27);
            this.tlpDrainAttributes.Margin = new System.Windows.Forms.Padding(0);
            this.tlpDrainAttributes.Name = "tlpDrainAttributes";
            this.tlpDrainAttributes.RowCount = 1;
            this.tlpDrainAttributes.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpDrainAttributes.Size = new System.Drawing.Size(192, 27);
            this.tlpDrainAttributes.TabIndex = 156;
            // 
            // lblDrainAttributesValue
            // 
            this.lblDrainAttributesValue.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblDrainAttributesValue.AutoSize = true;
            this.lblDrainAttributesValue.Location = new System.Drawing.Point(152, 7);
            this.lblDrainAttributesValue.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblDrainAttributesValue.Name = "lblDrainAttributesValue";
            this.lblDrainAttributesValue.Size = new System.Drawing.Size(37, 13);
            this.lblDrainAttributesValue.TabIndex = 93;
            this.lblDrainAttributesValue.Text = "[Total]";
            this.lblDrainAttributesValue.ToolTipText = "";
            // 
            // lblDrainAttributes
            // 
            this.lblDrainAttributes.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblDrainAttributes.AutoSize = true;
            this.lblDrainAttributes.Location = new System.Drawing.Point(89, 7);
            this.lblDrainAttributes.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblDrainAttributes.Name = "lblDrainAttributes";
            this.lblDrainAttributes.Size = new System.Drawing.Size(57, 13);
            this.lblDrainAttributes.TabIndex = 92;
            this.lblDrainAttributes.Text = "[Attributes]";
            // 
            // cboDrain
            // 
            this.cboDrain.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.cboDrain.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboDrain.FormattingEnabled = true;
            this.cboDrain.Location = new System.Drawing.Point(3, 3);
            this.cboDrain.Name = "cboDrain";
            this.cboDrain.Size = new System.Drawing.Size(80, 21);
            this.cboDrain.TabIndex = 143;
            this.cboDrain.TooltipText = "";
            this.cboDrain.Visible = false;
            // 
            // gpbMagicianMentorSpirit
            // 
            this.gpbMagicianMentorSpirit.AutoSize = true;
            this.gpbMagicianMentorSpirit.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.gpbMagicianMentorSpirit.Controls.Add(this.tlpMagicianMentorSpirit);
            this.gpbMagicianMentorSpirit.Dock = System.Windows.Forms.DockStyle.Top;
            this.gpbMagicianMentorSpirit.Location = new System.Drawing.Point(3, 340);
            this.gpbMagicianMentorSpirit.Name = "gpbMagicianMentorSpirit";
            this.gpbMagicianMentorSpirit.Size = new System.Drawing.Size(561, 69);
            this.gpbMagicianMentorSpirit.TabIndex = 2;
            this.gpbMagicianMentorSpirit.TabStop = false;
            this.gpbMagicianMentorSpirit.Tag = "String_MentorSpirit";
            this.gpbMagicianMentorSpirit.Text = "Mentor Spirit";
            // 
            // tlpMagicianMentorSpirit
            // 
            this.tlpMagicianMentorSpirit.AutoSize = true;
            this.tlpMagicianMentorSpirit.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpMagicianMentorSpirit.ColumnCount = 1;
            this.tlpMagicianMentorSpirit.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMagicianMentorSpirit.Controls.Add(this.lblMentorSpiritInformation, 0, 1);
            this.tlpMagicianMentorSpirit.Controls.Add(this.tlpMagicianMentorSpiritHeader, 0, 0);
            this.tlpMagicianMentorSpirit.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpMagicianMentorSpirit.Location = new System.Drawing.Point(3, 16);
            this.tlpMagicianMentorSpirit.Name = "tlpMagicianMentorSpirit";
            this.tlpMagicianMentorSpirit.RowCount = 2;
            this.tlpMagicianMentorSpirit.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMagicianMentorSpirit.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMagicianMentorSpirit.Size = new System.Drawing.Size(555, 50);
            this.tlpMagicianMentorSpirit.TabIndex = 0;
            // 
            // lblMentorSpiritLabel
            // 
            this.lblMentorSpiritLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblMentorSpiritLabel.AutoSize = true;
            this.lblMentorSpiritLabel.Location = new System.Drawing.Point(3, 6);
            this.lblMentorSpiritLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblMentorSpiritLabel.Name = "lblMentorSpiritLabel";
            this.lblMentorSpiritLabel.Size = new System.Drawing.Size(69, 13);
            this.lblMentorSpiritLabel.TabIndex = 95;
            this.lblMentorSpiritLabel.Tag = "Label_MentorSpirit";
            this.lblMentorSpiritLabel.Text = "Mentor Spirit:";
            // 
            // lblMentorSpirit
            // 
            this.lblMentorSpirit.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblMentorSpirit.AutoSize = true;
            this.lblMentorSpirit.Location = new System.Drawing.Point(78, 6);
            this.lblMentorSpirit.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblMentorSpirit.Name = "lblMentorSpirit";
            this.lblMentorSpirit.Size = new System.Drawing.Size(72, 13);
            this.lblMentorSpirit.TabIndex = 96;
            this.lblMentorSpirit.Text = "[Mentor Spirit]";
            // 
            // lblMentorSpiritInformation
            // 
            this.lblMentorSpiritInformation.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblMentorSpiritInformation.AutoSize = true;
            this.lblMentorSpiritInformation.Location = new System.Drawing.Point(3, 31);
            this.lblMentorSpiritInformation.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblMentorSpiritInformation.Name = "lblMentorSpiritInformation";
            this.lblMentorSpiritInformation.Size = new System.Drawing.Size(127, 13);
            this.lblMentorSpiritInformation.TabIndex = 94;
            this.lblMentorSpiritInformation.Text = "[Mentor Spirit Information]";
            // 
            // lblMentorSpiritSourceLabel
            // 
            this.lblMentorSpiritSourceLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblMentorSpiritSourceLabel.AutoSize = true;
            this.lblMentorSpiritSourceLabel.Location = new System.Drawing.Point(293, 6);
            this.lblMentorSpiritSourceLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblMentorSpiritSourceLabel.Name = "lblMentorSpiritSourceLabel";
            this.lblMentorSpiritSourceLabel.Size = new System.Drawing.Size(44, 13);
            this.lblMentorSpiritSourceLabel.TabIndex = 97;
            this.lblMentorSpiritSourceLabel.Tag = "Label_Source";
            this.lblMentorSpiritSourceLabel.Text = "Source:";
            // 
            // lblMentorSpiritSource
            // 
            this.lblMentorSpiritSource.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblMentorSpiritSource.AutoSize = true;
            this.lblMentorSpiritSource.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblMentorSpiritSource.Location = new System.Drawing.Point(343, 6);
            this.lblMentorSpiritSource.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblMentorSpiritSource.Name = "lblMentorSpiritSource";
            this.lblMentorSpiritSource.Size = new System.Drawing.Size(47, 13);
            this.lblMentorSpiritSource.TabIndex = 98;
            this.lblMentorSpiritSource.Text = "[Source]";
            this.lblMentorSpiritSource.Click += new System.EventHandler(this.OpenSourceFromLabel);
            // 
            // tlpMagicianButtons
            // 
            this.tlpMagicianButtons.AutoSize = true;
            this.tlpMagicianButtons.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpMagicianButtons.ColumnCount = 2;
            this.tlpMagician.SetColumnSpan(this.tlpMagicianButtons, 2);
            this.tlpMagicianButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpMagicianButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpMagicianButtons.Controls.Add(this.cmdAddSpell, 0, 0);
            this.tlpMagicianButtons.Controls.Add(this.cmdDeleteSpell, 1, 0);
            this.tlpMagicianButtons.Location = new System.Drawing.Point(0, 0);
            this.tlpMagicianButtons.Margin = new System.Windows.Forms.Padding(0);
            this.tlpMagicianButtons.Name = "tlpMagicianButtons";
            this.tlpMagicianButtons.RowCount = 1;
            this.tlpMagicianButtons.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpMagicianButtons.Size = new System.Drawing.Size(136, 29);
            this.tlpMagicianButtons.TabIndex = 158;
            // 
            // cmdAddSpell
            // 
            this.cmdAddSpell.AutoSize = true;
            this.cmdAddSpell.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdAddSpell.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmdAddSpell.Location = new System.Drawing.Point(3, 3);
            this.cmdAddSpell.Name = "cmdAddSpell";
            this.cmdAddSpell.Size = new System.Drawing.Size(62, 23);
            this.cmdAddSpell.TabIndex = 140;
            this.cmdAddSpell.Tag = "Button_AddSpell";
            this.cmdAddSpell.Text = "&Add Spell";
            this.cmdAddSpell.UseVisualStyleBackColor = true;
            this.cmdAddSpell.Click += new System.EventHandler(this.cmdAddSpell_Click);
            // 
            // cmdDeleteSpell
            // 
            this.cmdDeleteSpell.AutoSize = true;
            this.cmdDeleteSpell.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdDeleteSpell.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmdDeleteSpell.Location = new System.Drawing.Point(71, 3);
            this.cmdDeleteSpell.Name = "cmdDeleteSpell";
            this.cmdDeleteSpell.Size = new System.Drawing.Size(62, 23);
            this.cmdDeleteSpell.TabIndex = 69;
            this.cmdDeleteSpell.Tag = "String_Delete";
            this.cmdDeleteSpell.Text = "Delete";
            this.cmdDeleteSpell.UseVisualStyleBackColor = true;
            this.cmdDeleteSpell.Click += new System.EventHandler(this.cmdDeleteSpell_Click);
            // 
            // tabAdept
            // 
            this.tabAdept.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.tabAdept.Controls.Add(this.tabPowerUc);
            this.tabAdept.Location = new System.Drawing.Point(4, 22);
            this.tabAdept.Name = "tabAdept";
            this.tabAdept.Size = new System.Drawing.Size(977, 631);
            this.tabAdept.TabIndex = 2;
            this.tabAdept.Tag = "Tab_Adept";
            this.tabAdept.Text = "Adept Powers";
            // 
            // tabPowerUc
            // 
            this.tabPowerUc.AutoSize = true;
            this.tabPowerUc.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tabPowerUc.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabPowerUc.Location = new System.Drawing.Point(0, 0);
            this.tabPowerUc.MinimumSize = new System.Drawing.Size(480, 80);
            this.tabPowerUc.Name = "tabPowerUc";
            this.tabPowerUc.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.tabPowerUc.Size = new System.Drawing.Size(977, 631);
            this.tabPowerUc.TabIndex = 0;
            // 
            // tabTechnomancer
            // 
            this.tabTechnomancer.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.tabTechnomancer.Controls.Add(this.tlpTechnomancer);
            this.tabTechnomancer.Location = new System.Drawing.Point(4, 22);
            this.tabTechnomancer.Name = "tabTechnomancer";
            this.tabTechnomancer.Padding = new System.Windows.Forms.Padding(3);
            this.tabTechnomancer.Size = new System.Drawing.Size(977, 631);
            this.tabTechnomancer.TabIndex = 3;
            this.tabTechnomancer.Tag = "Tab_Technomancer";
            this.tabTechnomancer.Text = "Sprites and Complex Forms";
            // 
            // tlpTechnomancer
            // 
            this.tlpTechnomancer.ColumnCount = 2;
            this.tlpTechnomancer.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpTechnomancer.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpTechnomancer.Controls.Add(this.cmdAddSprite, 0, 2);
            this.tlpTechnomancer.Controls.Add(this.panSprites, 0, 3);
            this.tlpTechnomancer.Controls.Add(this.treComplexForms, 0, 1);
            this.tlpTechnomancer.Controls.Add(this.flpTechnomancer, 1, 1);
            this.tlpTechnomancer.Controls.Add(this.tlpTechnomancerButtons, 0, 0);
            this.tlpTechnomancer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpTechnomancer.Location = new System.Drawing.Point(3, 3);
            this.tlpTechnomancer.Name = "tlpTechnomancer";
            this.tlpTechnomancer.RowCount = 4;
            this.tlpTechnomancer.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpTechnomancer.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpTechnomancer.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpTechnomancer.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpTechnomancer.Size = new System.Drawing.Size(971, 625);
            this.tlpTechnomancer.TabIndex = 155;
            // 
            // cmdAddSprite
            // 
            this.cmdAddSprite.AutoSize = true;
            this.cmdAddSprite.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdAddSprite.Location = new System.Drawing.Point(3, 417);
            this.cmdAddSprite.Name = "cmdAddSprite";
            this.cmdAddSprite.Size = new System.Drawing.Size(66, 23);
            this.cmdAddSprite.TabIndex = 26;
            this.cmdAddSprite.Tag = "Button_AddSprite";
            this.cmdAddSprite.Text = "&Add Sprite";
            this.cmdAddSprite.UseVisualStyleBackColor = true;
            this.cmdAddSprite.Click += new System.EventHandler(this.cmdAddSprite_Click);
            // 
            // panSprites
            // 
            this.panSprites.AutoScroll = true;
            this.panSprites.AutoSize = true;
            this.panSprites.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpTechnomancer.SetColumnSpan(this.panSprites, 2);
            this.panSprites.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panSprites.Location = new System.Drawing.Point(3, 446);
            this.panSprites.Name = "panSprites";
            this.panSprites.Size = new System.Drawing.Size(965, 176);
            this.panSprites.TabIndex = 25;
            // 
            // treComplexForms
            // 
            this.treComplexForms.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treComplexForms.HideSelection = false;
            this.treComplexForms.Location = new System.Drawing.Point(3, 32);
            this.treComplexForms.Name = "treComplexForms";
            treeNode12.Name = "nodProgramAdvancedRoot";
            treeNode12.Tag = "Node_SelectedAdvancedComplexForms";
            treeNode12.Text = "Selected Complex Forms";
            this.treComplexForms.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            treeNode12});
            this.treComplexForms.ShowNodeToolTips = true;
            this.treComplexForms.ShowRootLines = false;
            this.treComplexForms.Size = new System.Drawing.Size(295, 379);
            this.treComplexForms.TabIndex = 71;
            this.treComplexForms.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treComplexForms_AfterSelect);
            this.treComplexForms.KeyDown += new System.Windows.Forms.KeyEventHandler(this.treComplexForms_KeyDown);
            this.treComplexForms.MouseDown += new System.Windows.Forms.MouseEventHandler(this.TreeView_MouseDown);
            // 
            // flpTechnomancer
            // 
            this.flpTechnomancer.AutoScroll = true;
            this.flpTechnomancer.Controls.Add(this.gpbTechnomancerComplexForm);
            this.flpTechnomancer.Controls.Add(this.gpbTechnomancerStream);
            this.flpTechnomancer.Controls.Add(this.gpbTechnomancerParagon);
            this.flpTechnomancer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flpTechnomancer.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flpTechnomancer.Location = new System.Drawing.Point(301, 29);
            this.flpTechnomancer.Margin = new System.Windows.Forms.Padding(0);
            this.flpTechnomancer.Name = "flpTechnomancer";
            this.flpTechnomancer.Size = new System.Drawing.Size(670, 385);
            this.flpTechnomancer.TabIndex = 188;
            this.flpTechnomancer.WrapContents = false;
            // 
            // gpbTechnomancerComplexForm
            // 
            this.gpbTechnomancerComplexForm.AutoSize = true;
            this.gpbTechnomancerComplexForm.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.gpbTechnomancerComplexForm.Controls.Add(this.tlpTechnomancerComplexForm);
            this.gpbTechnomancerComplexForm.Dock = System.Windows.Forms.DockStyle.Top;
            this.gpbTechnomancerComplexForm.Location = new System.Drawing.Point(3, 3);
            this.gpbTechnomancerComplexForm.Name = "gpbTechnomancerComplexForm";
            this.gpbTechnomancerComplexForm.Size = new System.Drawing.Size(500, 144);
            this.gpbTechnomancerComplexForm.TabIndex = 0;
            this.gpbTechnomancerComplexForm.TabStop = false;
            this.gpbTechnomancerComplexForm.Tag = "String_ExpenseComplexForm";
            this.gpbTechnomancerComplexForm.Text = "Complex Form";
            this.gpbTechnomancerComplexForm.Visible = false;
            // 
            // tlpTechnomancerComplexForm
            // 
            this.tlpTechnomancerComplexForm.AutoSize = true;
            this.tlpTechnomancerComplexForm.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpTechnomancerComplexForm.ColumnCount = 2;
            this.tlpTechnomancerComplexForm.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpTechnomancerComplexForm.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpTechnomancerComplexForm.Controls.Add(this.lblComplexFormDicePool, 1, 3);
            this.tlpTechnomancerComplexForm.Controls.Add(this.lblComplexFormDicePoolLabel, 0, 3);
            this.tlpTechnomancerComplexForm.Controls.Add(this.lblTargetLabel, 0, 0);
            this.tlpTechnomancerComplexForm.Controls.Add(this.lblTarget, 0, 0);
            this.tlpTechnomancerComplexForm.Controls.Add(this.lblDurationLabel, 0, 1);
            this.tlpTechnomancerComplexForm.Controls.Add(this.lblDuration, 1, 1);
            this.tlpTechnomancerComplexForm.Controls.Add(this.lblFVLabel, 0, 2);
            this.tlpTechnomancerComplexForm.Controls.Add(this.lblFV, 1, 2);
            this.tlpTechnomancerComplexForm.Controls.Add(this.lblComplexFormSourceLabel, 0, 4);
            this.tlpTechnomancerComplexForm.Controls.Add(this.lblComplexFormSource, 1, 4);
            this.tlpTechnomancerComplexForm.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpTechnomancerComplexForm.Location = new System.Drawing.Point(3, 16);
            this.tlpTechnomancerComplexForm.Name = "tlpTechnomancerComplexForm";
            this.tlpTechnomancerComplexForm.RowCount = 5;
            this.tlpTechnomancerComplexForm.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpTechnomancerComplexForm.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpTechnomancerComplexForm.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpTechnomancerComplexForm.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpTechnomancerComplexForm.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpTechnomancerComplexForm.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpTechnomancerComplexForm.Size = new System.Drawing.Size(494, 125);
            this.tlpTechnomancerComplexForm.TabIndex = 0;
            // 
            // lblComplexFormDicePool
            // 
            this.lblComplexFormDicePool.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblComplexFormDicePool.AutoSize = true;
            this.lblComplexFormDicePool.Location = new System.Drawing.Point(65, 81);
            this.lblComplexFormDicePool.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblComplexFormDicePool.Name = "lblComplexFormDicePool";
            this.lblComplexFormDicePool.Size = new System.Drawing.Size(59, 13);
            this.lblComplexFormDicePool.TabIndex = 183;
            this.lblComplexFormDicePool.Text = "[Dice Pool]";
            // 
            // lblComplexFormDicePoolLabel
            // 
            this.lblComplexFormDicePoolLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblComplexFormDicePoolLabel.AutoSize = true;
            this.lblComplexFormDicePoolLabel.Location = new System.Drawing.Point(3, 81);
            this.lblComplexFormDicePoolLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblComplexFormDicePoolLabel.Name = "lblComplexFormDicePoolLabel";
            this.lblComplexFormDicePoolLabel.Size = new System.Drawing.Size(56, 13);
            this.lblComplexFormDicePoolLabel.TabIndex = 182;
            this.lblComplexFormDicePoolLabel.Tag = "Label_DicePool";
            this.lblComplexFormDicePoolLabel.Text = "Dice Pool:";
            // 
            // lblTargetLabel
            // 
            this.lblTargetLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblTargetLabel.AutoSize = true;
            this.lblTargetLabel.Location = new System.Drawing.Point(18, 6);
            this.lblTargetLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblTargetLabel.Name = "lblTargetLabel";
            this.lblTargetLabel.Size = new System.Drawing.Size(41, 13);
            this.lblTargetLabel.TabIndex = 148;
            this.lblTargetLabel.Tag = "Label_SelectComplexForm_Target";
            this.lblTargetLabel.Text = "Target:";
            // 
            // lblTarget
            // 
            this.lblTarget.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblTarget.AutoSize = true;
            this.lblTarget.Location = new System.Drawing.Point(65, 6);
            this.lblTarget.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblTarget.Name = "lblTarget";
            this.lblTarget.Size = new System.Drawing.Size(39, 13);
            this.lblTarget.TabIndex = 149;
            this.lblTarget.Text = "[None]";
            // 
            // lblDurationLabel
            // 
            this.lblDurationLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblDurationLabel.AutoSize = true;
            this.lblDurationLabel.Location = new System.Drawing.Point(9, 31);
            this.lblDurationLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblDurationLabel.Name = "lblDurationLabel";
            this.lblDurationLabel.Size = new System.Drawing.Size(50, 13);
            this.lblDurationLabel.TabIndex = 150;
            this.lblDurationLabel.Tag = "Label_SelectComplexForm_Duration";
            this.lblDurationLabel.Text = "Duration:";
            // 
            // lblDuration
            // 
            this.lblDuration.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblDuration.AutoSize = true;
            this.lblDuration.Location = new System.Drawing.Point(65, 31);
            this.lblDuration.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblDuration.Name = "lblDuration";
            this.lblDuration.Size = new System.Drawing.Size(39, 13);
            this.lblDuration.TabIndex = 151;
            this.lblDuration.Text = "[None]";
            // 
            // lblFVLabel
            // 
            this.lblFVLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblFVLabel.AutoSize = true;
            this.lblFVLabel.Location = new System.Drawing.Point(36, 56);
            this.lblFVLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblFVLabel.Name = "lblFVLabel";
            this.lblFVLabel.Size = new System.Drawing.Size(23, 13);
            this.lblFVLabel.TabIndex = 152;
            this.lblFVLabel.Tag = "Label_SelectComplexForm_FV";
            this.lblFVLabel.Text = "FV:";
            // 
            // lblFV
            // 
            this.lblFV.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblFV.AutoSize = true;
            this.lblFV.Location = new System.Drawing.Point(65, 56);
            this.lblFV.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblFV.Name = "lblFV";
            this.lblFV.Size = new System.Drawing.Size(39, 13);
            this.lblFV.TabIndex = 153;
            this.lblFV.Text = "[None]";
            // 
            // lblComplexFormSourceLabel
            // 
            this.lblComplexFormSourceLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblComplexFormSourceLabel.AutoSize = true;
            this.lblComplexFormSourceLabel.Location = new System.Drawing.Point(15, 106);
            this.lblComplexFormSourceLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblComplexFormSourceLabel.Name = "lblComplexFormSourceLabel";
            this.lblComplexFormSourceLabel.Size = new System.Drawing.Size(44, 13);
            this.lblComplexFormSourceLabel.TabIndex = 89;
            this.lblComplexFormSourceLabel.Tag = "Label_Source";
            this.lblComplexFormSourceLabel.Text = "Source:";
            // 
            // lblComplexFormSource
            // 
            this.lblComplexFormSource.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblComplexFormSource.AutoSize = true;
            this.lblComplexFormSource.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblComplexFormSource.Location = new System.Drawing.Point(65, 106);
            this.lblComplexFormSource.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblComplexFormSource.Name = "lblComplexFormSource";
            this.lblComplexFormSource.Size = new System.Drawing.Size(47, 13);
            this.lblComplexFormSource.TabIndex = 90;
            this.lblComplexFormSource.Text = "[Source]";
            this.lblComplexFormSource.Click += new System.EventHandler(this.OpenSourceFromLabel);
            // 
            // gpbTechnomancerStream
            // 
            this.gpbTechnomancerStream.AutoSize = true;
            this.gpbTechnomancerStream.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.gpbTechnomancerStream.Controls.Add(this.tlpTechnomancerStream);
            this.gpbTechnomancerStream.Dock = System.Windows.Forms.DockStyle.Top;
            this.gpbTechnomancerStream.Location = new System.Drawing.Point(3, 153);
            this.gpbTechnomancerStream.Name = "gpbTechnomancerStream";
            this.gpbTechnomancerStream.Size = new System.Drawing.Size(500, 71);
            this.gpbTechnomancerStream.TabIndex = 1;
            this.gpbTechnomancerStream.TabStop = false;
            this.gpbTechnomancerStream.Tag = "String_Stream";
            this.gpbTechnomancerStream.Text = "Stream";
            // 
            // tlpTechnomancerStream
            // 
            this.tlpTechnomancerStream.AutoSize = true;
            this.tlpTechnomancerStream.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpTechnomancerStream.ColumnCount = 2;
            this.tlpTechnomancerStream.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpTechnomancerStream.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpTechnomancerStream.Controls.Add(this.lblStreamLabel, 0, 0);
            this.tlpTechnomancerStream.Controls.Add(this.cboStream, 1, 0);
            this.tlpTechnomancerStream.Controls.Add(this.lblFadingAttributesLabel, 0, 1);
            this.tlpTechnomancerStream.Controls.Add(this.flpFadingAttributesValue, 1, 1);
            this.tlpTechnomancerStream.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpTechnomancerStream.Location = new System.Drawing.Point(3, 16);
            this.tlpTechnomancerStream.Name = "tlpTechnomancerStream";
            this.tlpTechnomancerStream.RowCount = 2;
            this.tlpTechnomancerStream.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpTechnomancerStream.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpTechnomancerStream.Size = new System.Drawing.Size(494, 52);
            this.tlpTechnomancerStream.TabIndex = 0;
            // 
            // lblStreamLabel
            // 
            this.lblStreamLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblStreamLabel.AutoSize = true;
            this.lblStreamLabel.Location = new System.Drawing.Point(56, 7);
            this.lblStreamLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblStreamLabel.Name = "lblStreamLabel";
            this.lblStreamLabel.Size = new System.Drawing.Size(43, 13);
            this.lblStreamLabel.TabIndex = 100;
            this.lblStreamLabel.Tag = "Label_Stream";
            this.lblStreamLabel.Text = "Stream:";
            // 
            // cboStream
            // 
            this.cboStream.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.cboStream.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboStream.FormattingEnabled = true;
            this.cboStream.Location = new System.Drawing.Point(105, 3);
            this.cboStream.Name = "cboStream";
            this.cboStream.Size = new System.Drawing.Size(386, 21);
            this.cboStream.TabIndex = 101;
            this.cboStream.TooltipText = "";
            this.cboStream.SelectedIndexChanged += new System.EventHandler(this.cboStream_SelectedIndexChanged);
            // 
            // lblFadingAttributesLabel
            // 
            this.lblFadingAttributesLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblFadingAttributesLabel.AutoSize = true;
            this.lblFadingAttributesLabel.Location = new System.Drawing.Point(3, 33);
            this.lblFadingAttributesLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblFadingAttributesLabel.Name = "lblFadingAttributesLabel";
            this.lblFadingAttributesLabel.Size = new System.Drawing.Size(96, 13);
            this.lblFadingAttributesLabel.TabIndex = 96;
            this.lblFadingAttributesLabel.Tag = "Label_ResistFading";
            this.lblFadingAttributesLabel.Text = "Resist Fading with:";
            // 
            // flpFadingAttributesValue
            // 
            this.flpFadingAttributesValue.AutoSize = true;
            this.flpFadingAttributesValue.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flpFadingAttributesValue.Controls.Add(this.lblFadingAttributes);
            this.flpFadingAttributesValue.Controls.Add(this.lblFadingAttributesValue);
            this.flpFadingAttributesValue.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flpFadingAttributesValue.Location = new System.Drawing.Point(102, 27);
            this.flpFadingAttributesValue.Margin = new System.Windows.Forms.Padding(0);
            this.flpFadingAttributesValue.Name = "flpFadingAttributesValue";
            this.flpFadingAttributesValue.Size = new System.Drawing.Size(392, 25);
            this.flpFadingAttributesValue.TabIndex = 187;
            // 
            // lblFadingAttributes
            // 
            this.lblFadingAttributes.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblFadingAttributes.AutoSize = true;
            this.lblFadingAttributes.Location = new System.Drawing.Point(3, 6);
            this.lblFadingAttributes.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblFadingAttributes.Name = "lblFadingAttributes";
            this.lblFadingAttributes.Size = new System.Drawing.Size(57, 13);
            this.lblFadingAttributes.TabIndex = 97;
            this.lblFadingAttributes.Text = "[Attributes]";
            // 
            // lblFadingAttributesValue
            // 
            this.lblFadingAttributesValue.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblFadingAttributesValue.AutoSize = true;
            this.lblFadingAttributesValue.Location = new System.Drawing.Point(66, 6);
            this.lblFadingAttributesValue.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblFadingAttributesValue.Name = "lblFadingAttributesValue";
            this.lblFadingAttributesValue.Size = new System.Drawing.Size(37, 13);
            this.lblFadingAttributesValue.TabIndex = 98;
            this.lblFadingAttributesValue.Text = "[Total]";
            this.lblFadingAttributesValue.ToolTipText = "";
            // 
            // gpbTechnomancerParagon
            // 
            this.gpbTechnomancerParagon.AutoSize = true;
            this.gpbTechnomancerParagon.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.gpbTechnomancerParagon.Controls.Add(this.tlpTechnomancerParagon);
            this.gpbTechnomancerParagon.Dock = System.Windows.Forms.DockStyle.Top;
            this.gpbTechnomancerParagon.Location = new System.Drawing.Point(3, 230);
            this.gpbTechnomancerParagon.MinimumSize = new System.Drawing.Size(500, 0);
            this.gpbTechnomancerParagon.Name = "gpbTechnomancerParagon";
            this.gpbTechnomancerParagon.Size = new System.Drawing.Size(500, 69);
            this.gpbTechnomancerParagon.TabIndex = 2;
            this.gpbTechnomancerParagon.TabStop = false;
            this.gpbTechnomancerParagon.Tag = "String_Paragon";
            this.gpbTechnomancerParagon.Text = "Paragon";
            // 
            // tlpTechnomancerParagon
            // 
            this.tlpTechnomancerParagon.AutoSize = true;
            this.tlpTechnomancerParagon.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpTechnomancerParagon.ColumnCount = 1;
            this.tlpTechnomancerParagon.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpTechnomancerParagon.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpTechnomancerParagon.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpTechnomancerParagon.Controls.Add(this.lblParagonInformation, 0, 1);
            this.tlpTechnomancerParagon.Controls.Add(this.tlpTechnomancerParagonHeader, 0, 0);
            this.tlpTechnomancerParagon.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpTechnomancerParagon.Location = new System.Drawing.Point(3, 16);
            this.tlpTechnomancerParagon.Name = "tlpTechnomancerParagon";
            this.tlpTechnomancerParagon.RowCount = 2;
            this.tlpTechnomancerParagon.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpTechnomancerParagon.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpTechnomancerParagon.Size = new System.Drawing.Size(494, 50);
            this.tlpTechnomancerParagon.TabIndex = 0;
            // 
            // lblParagonInformation
            // 
            this.lblParagonInformation.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblParagonInformation.AutoSize = true;
            this.lblParagonInformation.Location = new System.Drawing.Point(3, 31);
            this.lblParagonInformation.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblParagonInformation.Name = "lblParagonInformation";
            this.lblParagonInformation.Size = new System.Drawing.Size(108, 13);
            this.lblParagonInformation.TabIndex = 184;
            this.lblParagonInformation.Text = "[Paragon Information]";
            // 
            // tlpTechnomancerButtons
            // 
            this.tlpTechnomancerButtons.AutoSize = true;
            this.tlpTechnomancerButtons.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpTechnomancerButtons.ColumnCount = 2;
            this.tlpTechnomancer.SetColumnSpan(this.tlpTechnomancerButtons, 2);
            this.tlpTechnomancerButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpTechnomancerButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpTechnomancerButtons.Controls.Add(this.cmdAddComplexForm, 0, 0);
            this.tlpTechnomancerButtons.Controls.Add(this.cmdDeleteComplexForm, 1, 0);
            this.tlpTechnomancerButtons.Location = new System.Drawing.Point(0, 0);
            this.tlpTechnomancerButtons.Margin = new System.Windows.Forms.Padding(0);
            this.tlpTechnomancerButtons.Name = "tlpTechnomancerButtons";
            this.tlpTechnomancerButtons.RowCount = 1;
            this.tlpTechnomancerButtons.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpTechnomancerButtons.Size = new System.Drawing.Size(222, 29);
            this.tlpTechnomancerButtons.TabIndex = 189;
            // 
            // cmdAddComplexForm
            // 
            this.cmdAddComplexForm.AutoSize = true;
            this.cmdAddComplexForm.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdAddComplexForm.ContextMenuStrip = this.cmsComplexForm;
            this.cmdAddComplexForm.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmdAddComplexForm.Location = new System.Drawing.Point(3, 3);
            this.cmdAddComplexForm.Name = "cmdAddComplexForm";
            this.cmdAddComplexForm.Size = new System.Drawing.Size(105, 23);
            this.cmdAddComplexForm.TabIndex = 147;
            this.cmdAddComplexForm.Tag = "Button_AddComplexForm";
            this.cmdAddComplexForm.Text = "Add Complex Form";
            this.cmdAddComplexForm.UseVisualStyleBackColor = true;
            this.cmdAddComplexForm.Click += new System.EventHandler(this.cmdAddComplexForm_Click);
            // 
            // cmdDeleteComplexForm
            // 
            this.cmdDeleteComplexForm.AutoSize = true;
            this.cmdDeleteComplexForm.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdDeleteComplexForm.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmdDeleteComplexForm.Location = new System.Drawing.Point(114, 3);
            this.cmdDeleteComplexForm.Name = "cmdDeleteComplexForm";
            this.cmdDeleteComplexForm.Size = new System.Drawing.Size(105, 23);
            this.cmdDeleteComplexForm.TabIndex = 31;
            this.cmdDeleteComplexForm.Tag = "String_Delete";
            this.cmdDeleteComplexForm.Text = "Delete";
            this.cmdDeleteComplexForm.UseVisualStyleBackColor = true;
            this.cmdDeleteComplexForm.Click += new System.EventHandler(this.cmdDeleteComplexForm_Click);
            // 
            // tabAdvancedPrograms
            // 
            this.tabAdvancedPrograms.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.tabAdvancedPrograms.Controls.Add(this.tlpAdvancedPrograms);
            this.tabAdvancedPrograms.Location = new System.Drawing.Point(4, 22);
            this.tabAdvancedPrograms.Name = "tabAdvancedPrograms";
            this.tabAdvancedPrograms.Padding = new System.Windows.Forms.Padding(3);
            this.tabAdvancedPrograms.Size = new System.Drawing.Size(977, 631);
            this.tabAdvancedPrograms.TabIndex = 15;
            this.tabAdvancedPrograms.Tag = "Tab_AdvancedPrograms";
            this.tabAdvancedPrograms.Text = "Advanced Programs";
            // 
            // tlpAdvancedPrograms
            // 
            this.tlpAdvancedPrograms.AutoSize = true;
            this.tlpAdvancedPrograms.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpAdvancedPrograms.ColumnCount = 3;
            this.tlpAdvancedPrograms.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpAdvancedPrograms.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpAdvancedPrograms.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpAdvancedPrograms.Controls.Add(this.treAIPrograms, 0, 1);
            this.tlpAdvancedPrograms.Controls.Add(this.lblAIProgramsSource, 2, 2);
            this.tlpAdvancedPrograms.Controls.Add(this.lblAIProgramsRequires, 2, 1);
            this.tlpAdvancedPrograms.Controls.Add(this.lblAIProgramsRequiresLabel, 1, 1);
            this.tlpAdvancedPrograms.Controls.Add(this.lblAIProgramsSourceLabel, 1, 2);
            this.tlpAdvancedPrograms.Controls.Add(this.tlpAdvancedProgramsButtons, 0, 0);
            this.tlpAdvancedPrograms.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpAdvancedPrograms.Location = new System.Drawing.Point(3, 3);
            this.tlpAdvancedPrograms.Name = "tlpAdvancedPrograms";
            this.tlpAdvancedPrograms.RowCount = 4;
            this.tlpAdvancedPrograms.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpAdvancedPrograms.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpAdvancedPrograms.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpAdvancedPrograms.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpAdvancedPrograms.Size = new System.Drawing.Size(971, 625);
            this.tlpAdvancedPrograms.TabIndex = 152;
            // 
            // treAIPrograms
            // 
            this.treAIPrograms.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treAIPrograms.HideSelection = false;
            this.treAIPrograms.Location = new System.Drawing.Point(3, 32);
            this.treAIPrograms.Name = "treAIPrograms";
            treeNode13.Name = "nodAIProgramsRoot";
            treeNode13.Tag = "Node_SelectedAIPrograms";
            treeNode13.Text = "Selected AI Programs and Advanced Programs";
            this.treAIPrograms.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            treeNode13});
            this.tlpAdvancedPrograms.SetRowSpan(this.treAIPrograms, 3);
            this.treAIPrograms.ShowNodeToolTips = true;
            this.treAIPrograms.ShowRootLines = false;
            this.treAIPrograms.Size = new System.Drawing.Size(295, 590);
            this.treAIPrograms.TabIndex = 71;
            this.treAIPrograms.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treAIPrograms_AfterSelect);
            this.treAIPrograms.KeyDown += new System.Windows.Forms.KeyEventHandler(this.treAIPrograms_KeyDown);
            // 
            // lblAIProgramsSource
            // 
            this.lblAIProgramsSource.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblAIProgramsSource.AutoSize = true;
            this.lblAIProgramsSource.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblAIProgramsSource.Location = new System.Drawing.Point(362, 60);
            this.lblAIProgramsSource.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblAIProgramsSource.Name = "lblAIProgramsSource";
            this.lblAIProgramsSource.Size = new System.Drawing.Size(47, 13);
            this.lblAIProgramsSource.TabIndex = 90;
            this.lblAIProgramsSource.Text = "[Source]";
            this.lblAIProgramsSource.Click += new System.EventHandler(this.OpenSourceFromLabel);
            // 
            // lblAIProgramsRequires
            // 
            this.lblAIProgramsRequires.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblAIProgramsRequires.AutoSize = true;
            this.lblAIProgramsRequires.Location = new System.Drawing.Point(362, 35);
            this.lblAIProgramsRequires.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblAIProgramsRequires.Name = "lblAIProgramsRequires";
            this.lblAIProgramsRequires.Size = new System.Drawing.Size(39, 13);
            this.lblAIProgramsRequires.TabIndex = 149;
            this.lblAIProgramsRequires.Tag = "";
            this.lblAIProgramsRequires.Text = "[None]";
            // 
            // lblAIProgramsRequiresLabel
            // 
            this.lblAIProgramsRequiresLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblAIProgramsRequiresLabel.AutoSize = true;
            this.lblAIProgramsRequiresLabel.Location = new System.Drawing.Point(304, 35);
            this.lblAIProgramsRequiresLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblAIProgramsRequiresLabel.Name = "lblAIProgramsRequiresLabel";
            this.lblAIProgramsRequiresLabel.Size = new System.Drawing.Size(52, 13);
            this.lblAIProgramsRequiresLabel.TabIndex = 148;
            this.lblAIProgramsRequiresLabel.Tag = "String_Requires";
            this.lblAIProgramsRequiresLabel.Text = "Requires:";
            // 
            // lblAIProgramsSourceLabel
            // 
            this.lblAIProgramsSourceLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblAIProgramsSourceLabel.AutoSize = true;
            this.lblAIProgramsSourceLabel.Location = new System.Drawing.Point(312, 60);
            this.lblAIProgramsSourceLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblAIProgramsSourceLabel.Name = "lblAIProgramsSourceLabel";
            this.lblAIProgramsSourceLabel.Size = new System.Drawing.Size(44, 13);
            this.lblAIProgramsSourceLabel.TabIndex = 89;
            this.lblAIProgramsSourceLabel.Tag = "Label_Source";
            this.lblAIProgramsSourceLabel.Text = "Source:";
            // 
            // tlpAdvancedProgramsButtons
            // 
            this.tlpAdvancedProgramsButtons.AutoSize = true;
            this.tlpAdvancedProgramsButtons.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpAdvancedProgramsButtons.ColumnCount = 2;
            this.tlpAdvancedPrograms.SetColumnSpan(this.tlpAdvancedProgramsButtons, 3);
            this.tlpAdvancedProgramsButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpAdvancedProgramsButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpAdvancedProgramsButtons.Controls.Add(this.cmdAddAIProgram, 0, 0);
            this.tlpAdvancedProgramsButtons.Controls.Add(this.cmdDeleteAIProgram, 1, 0);
            this.tlpAdvancedProgramsButtons.Location = new System.Drawing.Point(0, 0);
            this.tlpAdvancedProgramsButtons.Margin = new System.Windows.Forms.Padding(0);
            this.tlpAdvancedProgramsButtons.Name = "tlpAdvancedProgramsButtons";
            this.tlpAdvancedProgramsButtons.RowCount = 1;
            this.tlpAdvancedProgramsButtons.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpAdvancedProgramsButtons.Size = new System.Drawing.Size(168, 29);
            this.tlpAdvancedProgramsButtons.TabIndex = 152;
            // 
            // cmdAddAIProgram
            // 
            this.cmdAddAIProgram.AutoSize = true;
            this.cmdAddAIProgram.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdAddAIProgram.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmdAddAIProgram.Location = new System.Drawing.Point(3, 3);
            this.cmdAddAIProgram.Name = "cmdAddAIProgram";
            this.cmdAddAIProgram.Size = new System.Drawing.Size(78, 23);
            this.cmdAddAIProgram.TabIndex = 150;
            this.cmdAddAIProgram.Tag = "Button_AddProgram";
            this.cmdAddAIProgram.Text = "Add Program";
            this.cmdAddAIProgram.UseVisualStyleBackColor = true;
            this.cmdAddAIProgram.Click += new System.EventHandler(this.cmdAddAIProgram_Click);
            // 
            // cmdDeleteAIProgram
            // 
            this.cmdDeleteAIProgram.AutoSize = true;
            this.cmdDeleteAIProgram.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdDeleteAIProgram.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmdDeleteAIProgram.Location = new System.Drawing.Point(87, 3);
            this.cmdDeleteAIProgram.Name = "cmdDeleteAIProgram";
            this.cmdDeleteAIProgram.Size = new System.Drawing.Size(78, 23);
            this.cmdDeleteAIProgram.TabIndex = 31;
            this.cmdDeleteAIProgram.Tag = "String_Delete";
            this.cmdDeleteAIProgram.Text = "Delete";
            this.cmdDeleteAIProgram.UseVisualStyleBackColor = true;
            this.cmdDeleteAIProgram.Click += new System.EventHandler(this.cmdDeleteAIProgram_Click);
            // 
            // tabCritter
            // 
            this.tabCritter.BackColor = System.Drawing.SystemColors.Control;
            this.tabCritter.Controls.Add(this.tlpCritter);
            this.tabCritter.Location = new System.Drawing.Point(4, 22);
            this.tabCritter.Name = "tabCritter";
            this.tabCritter.Padding = new System.Windows.Forms.Padding(3);
            this.tabCritter.Size = new System.Drawing.Size(977, 631);
            this.tabCritter.TabIndex = 11;
            this.tabCritter.Tag = "Tab_Critter";
            this.tabCritter.Text = "Critter Powers";
            // 
            // tlpCritter
            // 
            this.tlpCritter.ColumnCount = 3;
            this.tlpCritter.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpCritter.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpCritter.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpCritter.Controls.Add(this.treCritterPowers, 0, 1);
            this.tlpCritter.Controls.Add(this.chkCritterPowerCount, 1, 10);
            this.tlpCritter.Controls.Add(this.lblCritterPowerPointCostLabel, 1, 9);
            this.tlpCritter.Controls.Add(this.lblCritterPowerPointCost, 2, 9);
            this.tlpCritter.Controls.Add(this.lblCritterPowerSourceLabel, 1, 8);
            this.tlpCritter.Controls.Add(this.lblCritterPowerSource, 2, 8);
            this.tlpCritter.Controls.Add(this.lblCritterPowerDurationLabel, 1, 7);
            this.tlpCritter.Controls.Add(this.lblCritterPowerDuration, 2, 7);
            this.tlpCritter.Controls.Add(this.lblCritterPowerRangeLabel, 1, 6);
            this.tlpCritter.Controls.Add(this.lblCritterPowerRange, 2, 6);
            this.tlpCritter.Controls.Add(this.lblCritterPowerActionLabel, 1, 5);
            this.tlpCritter.Controls.Add(this.lblCritterPowerAction, 2, 5);
            this.tlpCritter.Controls.Add(this.lblCritterPowerTypeLabel, 1, 4);
            this.tlpCritter.Controls.Add(this.lblCritterPowerType, 2, 4);
            this.tlpCritter.Controls.Add(this.lblCritterPowerCategoryLabel, 1, 3);
            this.tlpCritter.Controls.Add(this.lblCritterPowerCategory, 2, 3);
            this.tlpCritter.Controls.Add(this.lblCritterPowerNameLabel, 1, 2);
            this.tlpCritter.Controls.Add(this.lblCritterPowerName, 2, 2);
            this.tlpCritter.Controls.Add(this.lblCritterPowerPointsLabel, 1, 1);
            this.tlpCritter.Controls.Add(this.lblCritterPowerPoints, 2, 1);
            this.tlpCritter.Controls.Add(this.tlpCritterButtons, 0, 0);
            this.tlpCritter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpCritter.Location = new System.Drawing.Point(3, 3);
            this.tlpCritter.Name = "tlpCritter";
            this.tlpCritter.RowCount = 12;
            this.tlpCritter.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpCritter.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpCritter.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpCritter.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpCritter.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpCritter.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpCritter.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpCritter.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpCritter.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpCritter.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpCritter.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpCritter.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpCritter.Size = new System.Drawing.Size(971, 625);
            this.tlpCritter.TabIndex = 22;
            // 
            // treCritterPowers
            // 
            this.treCritterPowers.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treCritterPowers.HideSelection = false;
            this.treCritterPowers.Location = new System.Drawing.Point(3, 32);
            this.treCritterPowers.Name = "treCritterPowers";
            treeNode14.Name = "nodCritterPowerRoot";
            treeNode14.Tag = "Node_CritterPowers";
            treeNode14.Text = "Critter Powers";
            treeNode15.Name = "nodCritterWeaknessRoot";
            treeNode15.Tag = "Node_CritterWeaknesses";
            treeNode15.Text = "Weaknesses";
            this.treCritterPowers.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            treeNode14,
            treeNode15});
            this.tlpCritter.SetRowSpan(this.treCritterPowers, 11);
            this.treCritterPowers.ShowNodeToolTips = true;
            this.treCritterPowers.ShowPlusMinus = false;
            this.treCritterPowers.ShowRootLines = false;
            this.treCritterPowers.Size = new System.Drawing.Size(295, 590);
            this.treCritterPowers.TabIndex = 0;
            this.treCritterPowers.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treCritterPowers_AfterSelect);
            this.treCritterPowers.KeyDown += new System.Windows.Forms.KeyEventHandler(this.treCritterPowers_KeyDown);
            this.treCritterPowers.MouseDown += new System.Windows.Forms.MouseEventHandler(this.TreeView_MouseDown);
            // 
            // chkCritterPowerCount
            // 
            this.chkCritterPowerCount.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkCritterPowerCount.AutoSize = true;
            this.tlpCritter.SetColumnSpan(this.chkCritterPowerCount, 2);
            this.chkCritterPowerCount.Location = new System.Drawing.Point(304, 257);
            this.chkCritterPowerCount.Name = "chkCritterPowerCount";
            this.chkCritterPowerCount.Size = new System.Drawing.Size(182, 17);
            this.chkCritterPowerCount.TabIndex = 21;
            this.chkCritterPowerCount.Tag = "Checkbox_CritterPowerCount";
            this.chkCritterPowerCount.Text = "Counts towards Critter Power limit";
            this.chkCritterPowerCount.UseVisualStyleBackColor = true;
            this.chkCritterPowerCount.CheckedChanged += new System.EventHandler(this.chkCritterPowerCount_CheckedChanged);
            // 
            // lblCritterPowerPointCostLabel
            // 
            this.lblCritterPowerPointCostLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblCritterPowerPointCostLabel.AutoSize = true;
            this.lblCritterPowerPointCostLabel.Location = new System.Drawing.Point(337, 235);
            this.lblCritterPowerPointCostLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblCritterPowerPointCostLabel.Name = "lblCritterPowerPointCostLabel";
            this.lblCritterPowerPointCostLabel.Size = new System.Drawing.Size(39, 13);
            this.lblCritterPowerPointCostLabel.TabIndex = 19;
            this.lblCritterPowerPointCostLabel.Tag = "Label_Points";
            this.lblCritterPowerPointCostLabel.Text = "Points:";
            this.lblCritterPowerPointCostLabel.Visible = false;
            // 
            // lblCritterPowerPointCost
            // 
            this.lblCritterPowerPointCost.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblCritterPowerPointCost.AutoSize = true;
            this.lblCritterPowerPointCost.Location = new System.Drawing.Point(382, 235);
            this.lblCritterPowerPointCost.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblCritterPowerPointCost.Name = "lblCritterPowerPointCost";
            this.lblCritterPowerPointCost.Size = new System.Drawing.Size(75, 13);
            this.lblCritterPowerPointCost.TabIndex = 20;
            this.lblCritterPowerPointCost.Text = "[Power Points]";
            this.lblCritterPowerPointCost.Visible = false;
            // 
            // lblCritterPowerSourceLabel
            // 
            this.lblCritterPowerSourceLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblCritterPowerSourceLabel.AutoSize = true;
            this.lblCritterPowerSourceLabel.Location = new System.Drawing.Point(332, 210);
            this.lblCritterPowerSourceLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblCritterPowerSourceLabel.Name = "lblCritterPowerSourceLabel";
            this.lblCritterPowerSourceLabel.Size = new System.Drawing.Size(44, 13);
            this.lblCritterPowerSourceLabel.TabIndex = 13;
            this.lblCritterPowerSourceLabel.Tag = "Label_Source";
            this.lblCritterPowerSourceLabel.Text = "Source:";
            // 
            // lblCritterPowerSource
            // 
            this.lblCritterPowerSource.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblCritterPowerSource.AutoSize = true;
            this.lblCritterPowerSource.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblCritterPowerSource.Location = new System.Drawing.Point(382, 210);
            this.lblCritterPowerSource.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblCritterPowerSource.Name = "lblCritterPowerSource";
            this.lblCritterPowerSource.Size = new System.Drawing.Size(47, 13);
            this.lblCritterPowerSource.TabIndex = 14;
            this.lblCritterPowerSource.Text = "[Source]";
            this.lblCritterPowerSource.Click += new System.EventHandler(this.OpenSourceFromLabel);
            // 
            // lblCritterPowerDurationLabel
            // 
            this.lblCritterPowerDurationLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblCritterPowerDurationLabel.AutoSize = true;
            this.lblCritterPowerDurationLabel.Location = new System.Drawing.Point(326, 185);
            this.lblCritterPowerDurationLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblCritterPowerDurationLabel.Name = "lblCritterPowerDurationLabel";
            this.lblCritterPowerDurationLabel.Size = new System.Drawing.Size(50, 13);
            this.lblCritterPowerDurationLabel.TabIndex = 11;
            this.lblCritterPowerDurationLabel.Tag = "Label_Duration";
            this.lblCritterPowerDurationLabel.Text = "Duration:";
            // 
            // lblCritterPowerDuration
            // 
            this.lblCritterPowerDuration.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblCritterPowerDuration.AutoSize = true;
            this.lblCritterPowerDuration.Location = new System.Drawing.Point(382, 185);
            this.lblCritterPowerDuration.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblCritterPowerDuration.Name = "lblCritterPowerDuration";
            this.lblCritterPowerDuration.Size = new System.Drawing.Size(53, 13);
            this.lblCritterPowerDuration.TabIndex = 12;
            this.lblCritterPowerDuration.Text = "[Duration]";
            // 
            // lblCritterPowerRangeLabel
            // 
            this.lblCritterPowerRangeLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblCritterPowerRangeLabel.AutoSize = true;
            this.lblCritterPowerRangeLabel.Location = new System.Drawing.Point(334, 160);
            this.lblCritterPowerRangeLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblCritterPowerRangeLabel.Name = "lblCritterPowerRangeLabel";
            this.lblCritterPowerRangeLabel.Size = new System.Drawing.Size(42, 13);
            this.lblCritterPowerRangeLabel.TabIndex = 9;
            this.lblCritterPowerRangeLabel.Tag = "Label_Range";
            this.lblCritterPowerRangeLabel.Text = "Range:";
            // 
            // lblCritterPowerRange
            // 
            this.lblCritterPowerRange.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblCritterPowerRange.AutoSize = true;
            this.lblCritterPowerRange.Location = new System.Drawing.Point(382, 160);
            this.lblCritterPowerRange.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblCritterPowerRange.Name = "lblCritterPowerRange";
            this.lblCritterPowerRange.Size = new System.Drawing.Size(45, 13);
            this.lblCritterPowerRange.TabIndex = 10;
            this.lblCritterPowerRange.Text = "[Range]";
            // 
            // lblCritterPowerActionLabel
            // 
            this.lblCritterPowerActionLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblCritterPowerActionLabel.AutoSize = true;
            this.lblCritterPowerActionLabel.Location = new System.Drawing.Point(336, 135);
            this.lblCritterPowerActionLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblCritterPowerActionLabel.Name = "lblCritterPowerActionLabel";
            this.lblCritterPowerActionLabel.Size = new System.Drawing.Size(40, 13);
            this.lblCritterPowerActionLabel.TabIndex = 7;
            this.lblCritterPowerActionLabel.Tag = "Label_Action";
            this.lblCritterPowerActionLabel.Text = "Action:";
            // 
            // lblCritterPowerAction
            // 
            this.lblCritterPowerAction.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblCritterPowerAction.AutoSize = true;
            this.lblCritterPowerAction.Location = new System.Drawing.Point(382, 135);
            this.lblCritterPowerAction.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblCritterPowerAction.Name = "lblCritterPowerAction";
            this.lblCritterPowerAction.Size = new System.Drawing.Size(43, 13);
            this.lblCritterPowerAction.TabIndex = 8;
            this.lblCritterPowerAction.Text = "[Action]";
            // 
            // lblCritterPowerTypeLabel
            // 
            this.lblCritterPowerTypeLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblCritterPowerTypeLabel.AutoSize = true;
            this.lblCritterPowerTypeLabel.Location = new System.Drawing.Point(342, 110);
            this.lblCritterPowerTypeLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblCritterPowerTypeLabel.Name = "lblCritterPowerTypeLabel";
            this.lblCritterPowerTypeLabel.Size = new System.Drawing.Size(34, 13);
            this.lblCritterPowerTypeLabel.TabIndex = 5;
            this.lblCritterPowerTypeLabel.Tag = "Label_Type";
            this.lblCritterPowerTypeLabel.Text = "Type:";
            // 
            // lblCritterPowerType
            // 
            this.lblCritterPowerType.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblCritterPowerType.AutoSize = true;
            this.lblCritterPowerType.Location = new System.Drawing.Point(382, 110);
            this.lblCritterPowerType.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblCritterPowerType.Name = "lblCritterPowerType";
            this.lblCritterPowerType.Size = new System.Drawing.Size(37, 13);
            this.lblCritterPowerType.TabIndex = 6;
            this.lblCritterPowerType.Text = "[Type]";
            // 
            // lblCritterPowerCategoryLabel
            // 
            this.lblCritterPowerCategoryLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblCritterPowerCategoryLabel.AutoSize = true;
            this.lblCritterPowerCategoryLabel.Location = new System.Drawing.Point(324, 85);
            this.lblCritterPowerCategoryLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblCritterPowerCategoryLabel.Name = "lblCritterPowerCategoryLabel";
            this.lblCritterPowerCategoryLabel.Size = new System.Drawing.Size(52, 13);
            this.lblCritterPowerCategoryLabel.TabIndex = 3;
            this.lblCritterPowerCategoryLabel.Tag = "Label_Category";
            this.lblCritterPowerCategoryLabel.Text = "Category:";
            // 
            // lblCritterPowerCategory
            // 
            this.lblCritterPowerCategory.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblCritterPowerCategory.AutoSize = true;
            this.lblCritterPowerCategory.Location = new System.Drawing.Point(382, 85);
            this.lblCritterPowerCategory.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblCritterPowerCategory.Name = "lblCritterPowerCategory";
            this.lblCritterPowerCategory.Size = new System.Drawing.Size(55, 13);
            this.lblCritterPowerCategory.TabIndex = 4;
            this.lblCritterPowerCategory.Text = "[Category]";
            // 
            // lblCritterPowerNameLabel
            // 
            this.lblCritterPowerNameLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblCritterPowerNameLabel.AutoSize = true;
            this.lblCritterPowerNameLabel.Location = new System.Drawing.Point(338, 60);
            this.lblCritterPowerNameLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblCritterPowerNameLabel.Name = "lblCritterPowerNameLabel";
            this.lblCritterPowerNameLabel.Size = new System.Drawing.Size(38, 13);
            this.lblCritterPowerNameLabel.TabIndex = 1;
            this.lblCritterPowerNameLabel.Tag = "Label_Name";
            this.lblCritterPowerNameLabel.Text = "Name:";
            // 
            // lblCritterPowerName
            // 
            this.lblCritterPowerName.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblCritterPowerName.AutoSize = true;
            this.lblCritterPowerName.Location = new System.Drawing.Point(382, 60);
            this.lblCritterPowerName.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblCritterPowerName.Name = "lblCritterPowerName";
            this.lblCritterPowerName.Size = new System.Drawing.Size(41, 13);
            this.lblCritterPowerName.TabIndex = 2;
            this.lblCritterPowerName.Text = "[Name]";
            // 
            // lblCritterPowerPointsLabel
            // 
            this.lblCritterPowerPointsLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblCritterPowerPointsLabel.AutoSize = true;
            this.lblCritterPowerPointsLabel.Location = new System.Drawing.Point(304, 35);
            this.lblCritterPowerPointsLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblCritterPowerPointsLabel.Name = "lblCritterPowerPointsLabel";
            this.lblCritterPowerPointsLabel.Size = new System.Drawing.Size(72, 13);
            this.lblCritterPowerPointsLabel.TabIndex = 17;
            this.lblCritterPowerPointsLabel.Tag = "Label_PowerPoints";
            this.lblCritterPowerPointsLabel.Text = "Power Points:";
            this.lblCritterPowerPointsLabel.Visible = false;
            // 
            // lblCritterPowerPoints
            // 
            this.lblCritterPowerPoints.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblCritterPowerPoints.AutoSize = true;
            this.lblCritterPowerPoints.Location = new System.Drawing.Point(382, 35);
            this.lblCritterPowerPoints.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblCritterPowerPoints.Name = "lblCritterPowerPoints";
            this.lblCritterPowerPoints.Size = new System.Drawing.Size(76, 13);
            this.lblCritterPowerPoints.TabIndex = 18;
            this.lblCritterPowerPoints.Text = "0 (0 remaining)";
            this.lblCritterPowerPoints.Visible = false;
            // 
            // tlpCritterButtons
            // 
            this.tlpCritterButtons.AutoSize = true;
            this.tlpCritterButtons.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpCritterButtons.ColumnCount = 2;
            this.tlpCritter.SetColumnSpan(this.tlpCritterButtons, 3);
            this.tlpCritterButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpCritterButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpCritterButtons.Controls.Add(this.cmdAddCritterPower, 0, 0);
            this.tlpCritterButtons.Controls.Add(this.cmdDeleteCritterPower, 1, 0);
            this.tlpCritterButtons.Location = new System.Drawing.Point(0, 0);
            this.tlpCritterButtons.Margin = new System.Windows.Forms.Padding(0);
            this.tlpCritterButtons.Name = "tlpCritterButtons";
            this.tlpCritterButtons.RowCount = 1;
            this.tlpCritterButtons.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpCritterButtons.Size = new System.Drawing.Size(150, 29);
            this.tlpCritterButtons.TabIndex = 23;
            // 
            // cmdAddCritterPower
            // 
            this.cmdAddCritterPower.AutoSize = true;
            this.cmdAddCritterPower.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdAddCritterPower.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmdAddCritterPower.Location = new System.Drawing.Point(3, 3);
            this.cmdAddCritterPower.Name = "cmdAddCritterPower";
            this.cmdAddCritterPower.Size = new System.Drawing.Size(69, 23);
            this.cmdAddCritterPower.TabIndex = 15;
            this.cmdAddCritterPower.Tag = "Button_AddCritterPower";
            this.cmdAddCritterPower.Text = "&Add Power";
            this.cmdAddCritterPower.UseVisualStyleBackColor = true;
            this.cmdAddCritterPower.Click += new System.EventHandler(this.cmdAddCritterPower_Click);
            // 
            // cmdDeleteCritterPower
            // 
            this.cmdDeleteCritterPower.AutoSize = true;
            this.cmdDeleteCritterPower.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdDeleteCritterPower.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmdDeleteCritterPower.Location = new System.Drawing.Point(78, 3);
            this.cmdDeleteCritterPower.Name = "cmdDeleteCritterPower";
            this.cmdDeleteCritterPower.Size = new System.Drawing.Size(69, 23);
            this.cmdDeleteCritterPower.TabIndex = 16;
            this.cmdDeleteCritterPower.Tag = "String_Delete";
            this.cmdDeleteCritterPower.Text = "Delete";
            this.cmdDeleteCritterPower.UseVisualStyleBackColor = true;
            this.cmdDeleteCritterPower.Click += new System.EventHandler(this.cmdDeleteCritterPower_Click);
            // 
            // tabInitiation
            // 
            this.tabInitiation.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.tabInitiation.Controls.Add(this.tlpInitiation);
            this.tabInitiation.Location = new System.Drawing.Point(4, 22);
            this.tabInitiation.Name = "tabInitiation";
            this.tabInitiation.Padding = new System.Windows.Forms.Padding(3);
            this.tabInitiation.Size = new System.Drawing.Size(977, 631);
            this.tabInitiation.TabIndex = 10;
            this.tabInitiation.Tag = "Tab_Initiation";
            this.tabInitiation.Text = "Initiation & Submersion";
            // 
            // tlpInitiation
            // 
            this.tlpInitiation.ColumnCount = 3;
            this.tlpInitiation.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpInitiation.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpInitiation.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpInitiation.Controls.Add(this.treMetamagic, 0, 1);
            this.tlpInitiation.Controls.Add(this.lblMetamagicSourceLabel, 1, 1);
            this.tlpInitiation.Controls.Add(this.lblMetamagicSource, 2, 1);
            this.tlpInitiation.Controls.Add(this.flpInitiation, 1, 2);
            this.tlpInitiation.Controls.Add(this.tlpInitiationButtons, 0, 0);
            this.tlpInitiation.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpInitiation.Location = new System.Drawing.Point(3, 3);
            this.tlpInitiation.Name = "tlpInitiation";
            this.tlpInitiation.RowCount = 3;
            this.tlpInitiation.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpInitiation.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpInitiation.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpInitiation.Size = new System.Drawing.Size(971, 625);
            this.tlpInitiation.TabIndex = 130;
            // 
            // treMetamagic
            // 
            this.treMetamagic.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treMetamagic.HideSelection = false;
            this.treMetamagic.Location = new System.Drawing.Point(3, 32);
            this.treMetamagic.Name = "treMetamagic";
            this.tlpInitiation.SetRowSpan(this.treMetamagic, 2);
            this.treMetamagic.ShowLines = false;
            this.treMetamagic.ShowNodeToolTips = true;
            this.treMetamagic.ShowPlusMinus = false;
            this.treMetamagic.ShowRootLines = false;
            this.treMetamagic.Size = new System.Drawing.Size(295, 590);
            this.treMetamagic.TabIndex = 96;
            this.treMetamagic.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treMetamagic_AfterSelect);
            this.treMetamagic.KeyDown += new System.Windows.Forms.KeyEventHandler(this.treMetamagic_KeyDown);
            this.treMetamagic.MouseDown += new System.Windows.Forms.MouseEventHandler(this.TreeView_MouseDown);
            // 
            // lblMetamagicSourceLabel
            // 
            this.lblMetamagicSourceLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblMetamagicSourceLabel.AutoSize = true;
            this.lblMetamagicSourceLabel.Location = new System.Drawing.Point(304, 35);
            this.lblMetamagicSourceLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblMetamagicSourceLabel.Name = "lblMetamagicSourceLabel";
            this.lblMetamagicSourceLabel.Size = new System.Drawing.Size(44, 13);
            this.lblMetamagicSourceLabel.TabIndex = 108;
            this.lblMetamagicSourceLabel.Tag = "Label_Source";
            this.lblMetamagicSourceLabel.Text = "Source:";
            this.lblMetamagicSourceLabel.Visible = false;
            // 
            // lblMetamagicSource
            // 
            this.lblMetamagicSource.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblMetamagicSource.AutoSize = true;
            this.lblMetamagicSource.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblMetamagicSource.Location = new System.Drawing.Point(354, 35);
            this.lblMetamagicSource.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblMetamagicSource.Name = "lblMetamagicSource";
            this.lblMetamagicSource.Size = new System.Drawing.Size(47, 13);
            this.lblMetamagicSource.TabIndex = 109;
            this.lblMetamagicSource.Text = "[Source]";
            this.lblMetamagicSource.Visible = false;
            this.lblMetamagicSource.Click += new System.EventHandler(this.OpenSourceFromLabel);
            // 
            // flpInitiation
            // 
            this.tlpInitiation.SetColumnSpan(this.flpInitiation, 2);
            this.flpInitiation.Controls.Add(this.gpbInitiationType);
            this.flpInitiation.Controls.Add(this.gpbInitiationGroup);
            this.flpInitiation.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flpInitiation.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flpInitiation.Location = new System.Drawing.Point(301, 54);
            this.flpInitiation.Margin = new System.Windows.Forms.Padding(0);
            this.flpInitiation.Name = "flpInitiation";
            this.flpInitiation.Size = new System.Drawing.Size(670, 571);
            this.flpInitiation.TabIndex = 132;
            this.flpInitiation.WrapContents = false;
            // 
            // gpbInitiationType
            // 
            this.gpbInitiationType.AutoSize = true;
            this.gpbInitiationType.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.gpbInitiationType.Controls.Add(this.flpInitiationCheckBoxes);
            this.gpbInitiationType.Dock = System.Windows.Forms.DockStyle.Top;
            this.gpbInitiationType.Location = new System.Drawing.Point(3, 3);
            this.gpbInitiationType.Name = "gpbInitiationType";
            this.gpbInitiationType.Size = new System.Drawing.Size(503, 65);
            this.gpbInitiationType.TabIndex = 1;
            this.gpbInitiationType.TabStop = false;
            this.gpbInitiationType.Tag = "String_InitiationType";
            this.gpbInitiationType.Text = "Initiation Type";
            // 
            // flpInitiationCheckBoxes
            // 
            this.flpInitiationCheckBoxes.AutoSize = true;
            this.flpInitiationCheckBoxes.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flpInitiationCheckBoxes.Controls.Add(this.chkInitiationOrdeal);
            this.flpInitiationCheckBoxes.Controls.Add(this.chkInitiationSchooling);
            this.flpInitiationCheckBoxes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flpInitiationCheckBoxes.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flpInitiationCheckBoxes.Location = new System.Drawing.Point(3, 16);
            this.flpInitiationCheckBoxes.Margin = new System.Windows.Forms.Padding(0);
            this.flpInitiationCheckBoxes.Name = "flpInitiationCheckBoxes";
            this.flpInitiationCheckBoxes.Size = new System.Drawing.Size(497, 46);
            this.flpInitiationCheckBoxes.TabIndex = 130;
            this.flpInitiationCheckBoxes.WrapContents = false;
            // 
            // chkInitiationOrdeal
            // 
            this.chkInitiationOrdeal.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkInitiationOrdeal.AutoSize = true;
            this.chkInitiationOrdeal.Location = new System.Drawing.Point(3, 3);
            this.chkInitiationOrdeal.Name = "chkInitiationOrdeal";
            this.chkInitiationOrdeal.Size = new System.Drawing.Size(131, 17);
            this.chkInitiationOrdeal.TabIndex = 102;
            this.chkInitiationOrdeal.Tag = "Checkbox_InitiationOrdeal";
            this.chkInitiationOrdeal.Text = "Initiatory Ordeal (-10%)";
            this.chkInitiationOrdeal.UseVisualStyleBackColor = true;
            this.chkInitiationOrdeal.CheckedChanged += new System.EventHandler(this.chkInitiationOrdeal_CheckedChanged);
            // 
            // chkInitiationSchooling
            // 
            this.chkInitiationSchooling.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkInitiationSchooling.AutoSize = true;
            this.chkInitiationSchooling.Location = new System.Drawing.Point(3, 26);
            this.chkInitiationSchooling.Name = "chkInitiationSchooling";
            this.chkInitiationSchooling.Size = new System.Drawing.Size(105, 17);
            this.chkInitiationSchooling.TabIndex = 129;
            this.chkInitiationSchooling.Tag = "Checkbox_InitiationSchooling";
            this.chkInitiationSchooling.Text = "Schooling (-10%)";
            this.chkInitiationSchooling.UseVisualStyleBackColor = true;
            this.chkInitiationSchooling.CheckedChanged += new System.EventHandler(this.chkInitiationSchooling_CheckedChanged);
            this.chkInitiationSchooling.EnabledChanged += new System.EventHandler(this.chkInitiationSchooling_EnabledChanged);
            // 
            // gpbInitiationGroup
            // 
            this.gpbInitiationGroup.AutoSize = true;
            this.gpbInitiationGroup.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.gpbInitiationGroup.Controls.Add(this.tlpInitiationGroup);
            this.gpbInitiationGroup.Dock = System.Windows.Forms.DockStyle.Top;
            this.gpbInitiationGroup.Location = new System.Drawing.Point(3, 74);
            this.gpbInitiationGroup.Name = "gpbInitiationGroup";
            this.gpbInitiationGroup.Size = new System.Drawing.Size(503, 197);
            this.gpbInitiationGroup.TabIndex = 0;
            this.gpbInitiationGroup.TabStop = false;
            this.gpbInitiationGroup.Tag = "String_InitiationGroup";
            this.gpbInitiationGroup.Text = "Initiation Group";
            // 
            // tlpInitiationGroup
            // 
            this.tlpInitiationGroup.AutoSize = true;
            this.tlpInitiationGroup.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpInitiationGroup.ColumnCount = 2;
            this.tlpInitiationGroup.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpInitiationGroup.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpInitiationGroup.Controls.Add(this.txtGroupNotes, 1, 3);
            this.tlpInitiationGroup.Controls.Add(this.lblGroupName, 0, 2);
            this.tlpInitiationGroup.Controls.Add(this.chkInitiationGroup, 1, 1);
            this.tlpInitiationGroup.Controls.Add(this.lblGroupNotes, 0, 3);
            this.tlpInitiationGroup.Controls.Add(this.txtGroupName, 1, 2);
            this.tlpInitiationGroup.Controls.Add(this.chkJoinGroup, 1, 0);
            this.tlpInitiationGroup.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpInitiationGroup.Location = new System.Drawing.Point(3, 16);
            this.tlpInitiationGroup.Name = "tlpInitiationGroup";
            this.tlpInitiationGroup.RowCount = 4;
            this.tlpInitiationGroup.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpInitiationGroup.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpInitiationGroup.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpInitiationGroup.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpInitiationGroup.Size = new System.Drawing.Size(497, 178);
            this.tlpInitiationGroup.TabIndex = 1;
            // 
            // txtGroupNotes
            // 
            this.txtGroupNotes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtGroupNotes.Location = new System.Drawing.Point(48, 75);
            this.txtGroupNotes.Multiline = true;
            this.txtGroupNotes.Name = "txtGroupNotes";
            this.txtGroupNotes.Size = new System.Drawing.Size(446, 100);
            this.txtGroupNotes.TabIndex = 117;
            this.txtGroupNotes.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtGroupNotes_KeyDown);
            // 
            // lblGroupName
            // 
            this.lblGroupName.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblGroupName.AutoSize = true;
            this.lblGroupName.Location = new System.Drawing.Point(3, 52);
            this.lblGroupName.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblGroupName.Name = "lblGroupName";
            this.lblGroupName.Size = new System.Drawing.Size(39, 13);
            this.lblGroupName.TabIndex = 114;
            this.lblGroupName.Tag = "Label_Group";
            this.lblGroupName.Text = "Group:";
            // 
            // chkInitiationGroup
            // 
            this.chkInitiationGroup.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkInitiationGroup.AutoSize = true;
            this.chkInitiationGroup.Enabled = false;
            this.chkInitiationGroup.Location = new System.Drawing.Point(48, 26);
            this.chkInitiationGroup.Name = "chkInitiationGroup";
            this.chkInitiationGroup.Size = new System.Drawing.Size(129, 17);
            this.chkInitiationGroup.TabIndex = 126;
            this.chkInitiationGroup.Tag = "Checkbox_InitiationGroup";
            this.chkInitiationGroup.Text = "Group Initiation (-10%)";
            this.chkInitiationGroup.UseVisualStyleBackColor = true;
            this.chkInitiationGroup.CheckedChanged += new System.EventHandler(this.chkInitiationGroup_CheckedChanged);
            this.chkInitiationGroup.EnabledChanged += new System.EventHandler(this.chkInitiationGroup_EnabledChanged);
            // 
            // lblGroupNotes
            // 
            this.lblGroupNotes.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblGroupNotes.AutoSize = true;
            this.lblGroupNotes.Location = new System.Drawing.Point(4, 78);
            this.lblGroupNotes.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblGroupNotes.Name = "lblGroupNotes";
            this.lblGroupNotes.Size = new System.Drawing.Size(38, 13);
            this.lblGroupNotes.TabIndex = 116;
            this.lblGroupNotes.Tag = "Label_Notes";
            this.lblGroupNotes.Text = "Notes:";
            // 
            // txtGroupName
            // 
            this.txtGroupName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.txtGroupName.Location = new System.Drawing.Point(48, 49);
            this.txtGroupName.Name = "txtGroupName";
            this.txtGroupName.Size = new System.Drawing.Size(446, 20);
            this.txtGroupName.TabIndex = 115;
            // 
            // chkJoinGroup
            // 
            this.chkJoinGroup.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkJoinGroup.AutoSize = true;
            this.chkJoinGroup.Location = new System.Drawing.Point(48, 3);
            this.chkJoinGroup.Name = "chkJoinGroup";
            this.chkJoinGroup.Size = new System.Drawing.Size(77, 17);
            this.chkJoinGroup.TabIndex = 125;
            this.chkJoinGroup.Tag = "Checkbox_JoinedGroup";
            this.chkJoinGroup.Text = "Join Group";
            this.chkJoinGroup.UseVisualStyleBackColor = true;
            this.chkJoinGroup.CheckedChanged += new System.EventHandler(this.chkJoinGroup_CheckedChanged);
            // 
            // tlpInitiationButtons
            // 
            this.tlpInitiationButtons.AutoSize = true;
            this.tlpInitiationButtons.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpInitiationButtons.ColumnCount = 2;
            this.tlpInitiation.SetColumnSpan(this.tlpInitiationButtons, 3);
            this.tlpInitiationButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpInitiationButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpInitiationButtons.Controls.Add(this.cmdAddMetamagic, 0, 0);
            this.tlpInitiationButtons.Controls.Add(this.cmdDeleteMetamagic, 1, 0);
            this.tlpInitiationButtons.Location = new System.Drawing.Point(0, 0);
            this.tlpInitiationButtons.Margin = new System.Windows.Forms.Padding(0);
            this.tlpInitiationButtons.Name = "tlpInitiationButtons";
            this.tlpInitiationButtons.RowCount = 1;
            this.tlpInitiationButtons.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpInitiationButtons.Size = new System.Drawing.Size(258, 29);
            this.tlpInitiationButtons.TabIndex = 133;
            // 
            // cmdAddMetamagic
            // 
            this.cmdAddMetamagic.AutoSize = true;
            this.cmdAddMetamagic.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdAddMetamagic.ContextMenuStrip = this.cmsMetamagic;
            this.cmdAddMetamagic.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmdAddMetamagic.Location = new System.Drawing.Point(3, 3);
            this.cmdAddMetamagic.Name = "cmdAddMetamagic";
            this.cmdAddMetamagic.Size = new System.Drawing.Size(123, 23);
            this.cmdAddMetamagic.SplitMenuStrip = this.cmsMetamagic;
            this.cmdAddMetamagic.TabIndex = 93;
            this.cmdAddMetamagic.Tag = "Button_AddInitiateGrade";
            this.cmdAddMetamagic.Text = "&Add Initiate Grade";
            this.cmdAddMetamagic.UseVisualStyleBackColor = true;
            this.cmdAddMetamagic.Click += new System.EventHandler(this.cmdAddMetamagic_Click);
            // 
            // cmsMetamagic
            // 
            this.cmsMetamagic.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsMetamagicAddArt,
            this.tsMetamagicAddEnchantment,
            this.tsMetamagicAddEnhancement,
            this.tsMetamagicAddMetamagic,
            this.tsMetamagicAddRitual,
            this.tsMetamagicNotes});
            this.cmsMetamagic.Name = "cmsMetamagic";
            this.cmsMetamagic.Size = new System.Drawing.Size(173, 136);
            // 
            // tsMetamagicAddArt
            // 
            this.tsMetamagicAddArt.Name = "tsMetamagicAddArt";
            this.tsMetamagicAddArt.Size = new System.Drawing.Size(172, 22);
            this.tsMetamagicAddArt.Text = "Add Art";
            this.tsMetamagicAddArt.Click += new System.EventHandler(this.tsMetamagicAddArt_Click);
            // 
            // tsMetamagicAddEnchantment
            // 
            this.tsMetamagicAddEnchantment.Name = "tsMetamagicAddEnchantment";
            this.tsMetamagicAddEnchantment.Size = new System.Drawing.Size(172, 22);
            this.tsMetamagicAddEnchantment.Text = "Add Enchantment";
            this.tsMetamagicAddEnchantment.Click += new System.EventHandler(this.tsMetamagicAddEnchantment_Click);
            // 
            // tsMetamagicAddEnhancement
            // 
            this.tsMetamagicAddEnhancement.Name = "tsMetamagicAddEnhancement";
            this.tsMetamagicAddEnhancement.Size = new System.Drawing.Size(172, 22);
            this.tsMetamagicAddEnhancement.Text = "Add Enhancement";
            this.tsMetamagicAddEnhancement.Click += new System.EventHandler(this.tsMetamagicAddEnhancement_Click);
            // 
            // tsMetamagicAddMetamagic
            // 
            this.tsMetamagicAddMetamagic.Name = "tsMetamagicAddMetamagic";
            this.tsMetamagicAddMetamagic.Size = new System.Drawing.Size(172, 22);
            this.tsMetamagicAddMetamagic.Text = "Add Metamagic";
            this.tsMetamagicAddMetamagic.Click += new System.EventHandler(this.tsMetamagicAddMetamagic_Click);
            // 
            // tsMetamagicAddRitual
            // 
            this.tsMetamagicAddRitual.Name = "tsMetamagicAddRitual";
            this.tsMetamagicAddRitual.Size = new System.Drawing.Size(172, 22);
            this.tsMetamagicAddRitual.Text = "Add Ritual";
            this.tsMetamagicAddRitual.Click += new System.EventHandler(this.tsMetamagicAddRitual_Click);
            // 
            // tsMetamagicNotes
            // 
            this.tsMetamagicNotes.Image = global::Chummer.Properties.Resources.note_edit;
            this.tsMetamagicNotes.Name = "tsMetamagicNotes";
            this.tsMetamagicNotes.Size = new System.Drawing.Size(172, 22);
            this.tsMetamagicNotes.Tag = "Menu_Notes";
            this.tsMetamagicNotes.Text = "&Notes";
            this.tsMetamagicNotes.Click += new System.EventHandler(this.tsMetamagicNotes_Click);
            // 
            // cmdDeleteMetamagic
            // 
            this.cmdDeleteMetamagic.AutoSize = true;
            this.cmdDeleteMetamagic.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdDeleteMetamagic.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmdDeleteMetamagic.Location = new System.Drawing.Point(132, 3);
            this.cmdDeleteMetamagic.Name = "cmdDeleteMetamagic";
            this.cmdDeleteMetamagic.Size = new System.Drawing.Size(123, 23);
            this.cmdDeleteMetamagic.TabIndex = 94;
            this.cmdDeleteMetamagic.Tag = "Button_RemoveInitiateGrade";
            this.cmdDeleteMetamagic.Text = "&Remove Initiate Grade";
            this.cmdDeleteMetamagic.UseVisualStyleBackColor = true;
            this.cmdDeleteMetamagic.Click += new System.EventHandler(this.cmdDeleteMetamagic_Click);
            // 
            // tabCyberware
            // 
            this.tabCyberware.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.tabCyberware.Controls.Add(this.tlpCyberware);
            this.tabCyberware.Location = new System.Drawing.Point(4, 22);
            this.tabCyberware.Name = "tabCyberware";
            this.tabCyberware.Padding = new System.Windows.Forms.Padding(3);
            this.tabCyberware.Size = new System.Drawing.Size(977, 631);
            this.tabCyberware.TabIndex = 4;
            this.tabCyberware.Tag = "Tab_Cyberware";
            this.tabCyberware.Text = "Cyberware and Bioware";
            // 
            // tlpCyberware
            // 
            this.tlpCyberware.AutoSize = true;
            this.tlpCyberware.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpCyberware.ColumnCount = 2;
            this.tlpCyberware.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpCyberware.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpCyberware.Controls.Add(this.treCyberware, 0, 1);
            this.tlpCyberware.Controls.Add(this.flpCyberware, 1, 1);
            this.tlpCyberware.Controls.Add(this.tlpCyberwareButtons, 0, 0);
            this.tlpCyberware.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpCyberware.Location = new System.Drawing.Point(3, 3);
            this.tlpCyberware.Name = "tlpCyberware";
            this.tlpCyberware.RowCount = 2;
            this.tlpCyberware.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpCyberware.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpCyberware.Size = new System.Drawing.Size(971, 625);
            this.tlpCyberware.TabIndex = 250;
            // 
            // treCyberware
            // 
            this.treCyberware.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treCyberware.HideSelection = false;
            this.treCyberware.Location = new System.Drawing.Point(3, 32);
            this.treCyberware.Name = "treCyberware";
            treeNode16.Name = "nodCyberwareRoot";
            treeNode16.Tag = "Node_SelectedCyberware";
            treeNode16.Text = "Selected Cyberware";
            treeNode17.Name = "nodBioware";
            treeNode17.Tag = "Node_SelectedBioware";
            treeNode17.Text = "Selected Bioware";
            treeNode18.Name = "nodUnequippedModularCyberware";
            treeNode18.Tag = "Node_UnequippedModularCyberware";
            treeNode18.Text = "Unequipped Modular Cyberware";
            this.treCyberware.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            treeNode16,
            treeNode17,
            treeNode18});
            this.treCyberware.ShowNodeToolTips = true;
            this.treCyberware.ShowRootLines = false;
            this.treCyberware.Size = new System.Drawing.Size(295, 590);
            this.treCyberware.TabIndex = 28;
            this.treCyberware.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treCyberware_AfterSelect);
            this.treCyberware.KeyDown += new System.Windows.Forms.KeyEventHandler(this.treCyberware_KeyDown);
            this.treCyberware.MouseDown += new System.Windows.Forms.MouseEventHandler(this.TreeView_MouseDown);
            // 
            // flpCyberware
            // 
            this.flpCyberware.AutoScroll = true;
            this.flpCyberware.Controls.Add(this.gpbEssenceConsumption);
            this.flpCyberware.Controls.Add(this.gpbCyberwareCommon);
            this.flpCyberware.Controls.Add(this.gpbCyberwareMatrix);
            this.flpCyberware.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flpCyberware.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flpCyberware.Location = new System.Drawing.Point(301, 29);
            this.flpCyberware.Margin = new System.Windows.Forms.Padding(0);
            this.flpCyberware.Name = "flpCyberware";
            this.flpCyberware.Size = new System.Drawing.Size(670, 596);
            this.flpCyberware.TabIndex = 252;
            this.flpCyberware.WrapContents = false;
            // 
            // gpbEssenceConsumption
            // 
            this.gpbEssenceConsumption.AutoSize = true;
            this.gpbEssenceConsumption.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.gpbEssenceConsumption.Controls.Add(this.tlpCyberwareEssenceConsumption);
            this.gpbEssenceConsumption.Dock = System.Windows.Forms.DockStyle.Left;
            this.gpbEssenceConsumption.Location = new System.Drawing.Point(3, 3);
            this.gpbEssenceConsumption.MinimumSize = new System.Drawing.Size(150, 0);
            this.gpbEssenceConsumption.Name = "gpbEssenceConsumption";
            this.gpbEssenceConsumption.Size = new System.Drawing.Size(169, 119);
            this.gpbEssenceConsumption.TabIndex = 252;
            this.gpbEssenceConsumption.TabStop = false;
            this.gpbEssenceConsumption.Tag = "Label_EssenceConsumption";
            this.gpbEssenceConsumption.Text = "Essence Consumption";
            // 
            // tlpCyberwareEssenceConsumption
            // 
            this.tlpCyberwareEssenceConsumption.AutoSize = true;
            this.tlpCyberwareEssenceConsumption.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpCyberwareEssenceConsumption.ColumnCount = 2;
            this.tlpCyberwareEssenceConsumption.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpCyberwareEssenceConsumption.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpCyberwareEssenceConsumption.Controls.Add(this.lblCyberwareESSLabel, 0, 0);
            this.tlpCyberwareEssenceConsumption.Controls.Add(this.lblBiowareESSLabel, 0, 1);
            this.tlpCyberwareEssenceConsumption.Controls.Add(this.lblEssenceHoleESSLabel, 0, 2);
            this.tlpCyberwareEssenceConsumption.Controls.Add(this.lblPrototypeTranshumanESSLabel, 0, 3);
            this.tlpCyberwareEssenceConsumption.Controls.Add(this.lblCyberwareESS, 1, 0);
            this.tlpCyberwareEssenceConsumption.Controls.Add(this.lblBiowareESS, 1, 1);
            this.tlpCyberwareEssenceConsumption.Controls.Add(this.lblEssenceHoleESS, 1, 2);
            this.tlpCyberwareEssenceConsumption.Controls.Add(this.lblPrototypeTranshumanESS, 1, 3);
            this.tlpCyberwareEssenceConsumption.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpCyberwareEssenceConsumption.Location = new System.Drawing.Point(3, 16);
            this.tlpCyberwareEssenceConsumption.Name = "tlpCyberwareEssenceConsumption";
            this.tlpCyberwareEssenceConsumption.RowCount = 4;
            this.tlpCyberwareEssenceConsumption.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpCyberwareEssenceConsumption.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpCyberwareEssenceConsumption.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpCyberwareEssenceConsumption.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpCyberwareEssenceConsumption.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpCyberwareEssenceConsumption.Size = new System.Drawing.Size(163, 100);
            this.tlpCyberwareEssenceConsumption.TabIndex = 0;
            // 
            // lblCyberwareESSLabel
            // 
            this.lblCyberwareESSLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblCyberwareESSLabel.AutoSize = true;
            this.lblCyberwareESSLabel.Location = new System.Drawing.Point(60, 6);
            this.lblCyberwareESSLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblCyberwareESSLabel.Name = "lblCyberwareESSLabel";
            this.lblCyberwareESSLabel.Size = new System.Drawing.Size(60, 13);
            this.lblCyberwareESSLabel.TabIndex = 55;
            this.lblCyberwareESSLabel.Tag = "Label_Cyberware";
            this.lblCyberwareESSLabel.Text = "Cyberware:";
            // 
            // lblBiowareESSLabel
            // 
            this.lblBiowareESSLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblBiowareESSLabel.AutoSize = true;
            this.lblBiowareESSLabel.Location = new System.Drawing.Point(72, 31);
            this.lblBiowareESSLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblBiowareESSLabel.Name = "lblBiowareESSLabel";
            this.lblBiowareESSLabel.Size = new System.Drawing.Size(48, 13);
            this.lblBiowareESSLabel.TabIndex = 56;
            this.lblBiowareESSLabel.Tag = "Label_Bioware";
            this.lblBiowareESSLabel.Text = "Bioware:";
            // 
            // lblEssenceHoleESSLabel
            // 
            this.lblEssenceHoleESSLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblEssenceHoleESSLabel.AutoSize = true;
            this.lblEssenceHoleESSLabel.Location = new System.Drawing.Point(44, 56);
            this.lblEssenceHoleESSLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblEssenceHoleESSLabel.Name = "lblEssenceHoleESSLabel";
            this.lblEssenceHoleESSLabel.Size = new System.Drawing.Size(76, 13);
            this.lblEssenceHoleESSLabel.TabIndex = 60;
            this.lblEssenceHoleESSLabel.Tag = "Label_EssenceHole";
            this.lblEssenceHoleESSLabel.Text = "Essence Hole:";
            // 
            // lblPrototypeTranshumanESSLabel
            // 
            this.lblPrototypeTranshumanESSLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblPrototypeTranshumanESSLabel.AutoSize = true;
            this.lblPrototypeTranshumanESSLabel.Location = new System.Drawing.Point(3, 81);
            this.lblPrototypeTranshumanESSLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblPrototypeTranshumanESSLabel.Name = "lblPrototypeTranshumanESSLabel";
            this.lblPrototypeTranshumanESSLabel.Size = new System.Drawing.Size(117, 13);
            this.lblPrototypeTranshumanESSLabel.TabIndex = 248;
            this.lblPrototypeTranshumanESSLabel.Tag = "Label_PrototypeTranshuman";
            this.lblPrototypeTranshumanESSLabel.Text = "Prototype Transhuman:";
            // 
            // lblCyberwareESS
            // 
            this.lblCyberwareESS.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblCyberwareESS.AutoSize = true;
            this.lblCyberwareESS.Location = new System.Drawing.Point(126, 6);
            this.lblCyberwareESS.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblCyberwareESS.Name = "lblCyberwareESS";
            this.lblCyberwareESS.Size = new System.Drawing.Size(34, 13);
            this.lblCyberwareESS.TabIndex = 57;
            this.lblCyberwareESS.Text = "[0.00]";
            // 
            // lblBiowareESS
            // 
            this.lblBiowareESS.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblBiowareESS.AutoSize = true;
            this.lblBiowareESS.Location = new System.Drawing.Point(126, 31);
            this.lblBiowareESS.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblBiowareESS.Name = "lblBiowareESS";
            this.lblBiowareESS.Size = new System.Drawing.Size(34, 13);
            this.lblBiowareESS.TabIndex = 58;
            this.lblBiowareESS.Text = "[0.00]";
            // 
            // lblEssenceHoleESS
            // 
            this.lblEssenceHoleESS.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblEssenceHoleESS.AutoSize = true;
            this.lblEssenceHoleESS.Location = new System.Drawing.Point(126, 56);
            this.lblEssenceHoleESS.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblEssenceHoleESS.Name = "lblEssenceHoleESS";
            this.lblEssenceHoleESS.Size = new System.Drawing.Size(34, 13);
            this.lblEssenceHoleESS.TabIndex = 61;
            this.lblEssenceHoleESS.Text = "[0.00]";
            // 
            // lblPrototypeTranshumanESS
            // 
            this.lblPrototypeTranshumanESS.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblPrototypeTranshumanESS.AutoSize = true;
            this.lblPrototypeTranshumanESS.Location = new System.Drawing.Point(126, 81);
            this.lblPrototypeTranshumanESS.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblPrototypeTranshumanESS.Name = "lblPrototypeTranshumanESS";
            this.lblPrototypeTranshumanESS.Size = new System.Drawing.Size(34, 13);
            this.lblPrototypeTranshumanESS.TabIndex = 249;
            this.lblPrototypeTranshumanESS.Text = "[0.00]";
            // 
            // gpbCyberwareCommon
            // 
            this.gpbCyberwareCommon.AutoSize = true;
            this.gpbCyberwareCommon.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.gpbCyberwareCommon.Controls.Add(this.tlpCyberwareCommon);
            this.gpbCyberwareCommon.Dock = System.Windows.Forms.DockStyle.Top;
            this.gpbCyberwareCommon.Location = new System.Drawing.Point(3, 128);
            this.gpbCyberwareCommon.Name = "gpbCyberwareCommon";
            this.gpbCyberwareCommon.Size = new System.Drawing.Size(511, 201);
            this.gpbCyberwareCommon.TabIndex = 255;
            this.gpbCyberwareCommon.TabStop = false;
            this.gpbCyberwareCommon.Tag = "String_Info";
            this.gpbCyberwareCommon.Text = "Info";
            // 
            // tlpCyberwareCommon
            // 
            this.tlpCyberwareCommon.AutoSize = true;
            this.tlpCyberwareCommon.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpCyberwareCommon.ColumnCount = 4;
            this.tlpCyberwareCommon.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpCyberwareCommon.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpCyberwareCommon.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpCyberwareCommon.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpCyberwareCommon.Controls.Add(this.lblCyberwareNameLabel, 0, 0);
            this.tlpCyberwareCommon.Controls.Add(this.nudCyberwareRating, 3, 1);
            this.tlpCyberwareCommon.Controls.Add(this.lblCyberwareRatingLabel, 2, 1);
            this.tlpCyberwareCommon.Controls.Add(this.lblCyberwareCapacity, 3, 2);
            this.tlpCyberwareCommon.Controls.Add(this.lblCyberwareCost, 3, 3);
            this.tlpCyberwareCommon.Controls.Add(this.lblCyberwareCostLabel, 2, 3);
            this.tlpCyberwareCommon.Controls.Add(this.lblCyberlimbAGI, 3, 4);
            this.tlpCyberwareCommon.Controls.Add(this.lblCyberlimbAGILabel, 2, 4);
            this.tlpCyberwareCommon.Controls.Add(this.lblCyberlimbSTR, 3, 5);
            this.tlpCyberwareCommon.Controls.Add(this.lblCyberlimbSTRLabel, 2, 5);
            this.tlpCyberwareCommon.Controls.Add(this.lblCyberwareCapacityLabel, 2, 2);
            this.tlpCyberwareCommon.Controls.Add(this.lblCyberwareName, 1, 0);
            this.tlpCyberwareCommon.Controls.Add(this.cmdCyberwareChangeMount, 0, 6);
            this.tlpCyberwareCommon.Controls.Add(this.lblCyberwareCategoryLabel, 0, 1);
            this.tlpCyberwareCommon.Controls.Add(this.lblCyberwareSource, 1, 5);
            this.tlpCyberwareCommon.Controls.Add(this.lblCyberwareSourceLabel, 0, 5);
            this.tlpCyberwareCommon.Controls.Add(this.lblCyberwareAvail, 1, 4);
            this.tlpCyberwareCommon.Controls.Add(this.lblCyberwareAvailLabel, 0, 4);
            this.tlpCyberwareCommon.Controls.Add(this.lblCyberwareEssence, 1, 3);
            this.tlpCyberwareCommon.Controls.Add(this.lblCyberwareEssenceLabel, 0, 3);
            this.tlpCyberwareCommon.Controls.Add(this.cboCyberwareGrade, 1, 2);
            this.tlpCyberwareCommon.Controls.Add(this.lblCyberwareGradeLabel, 0, 2);
            this.tlpCyberwareCommon.Controls.Add(this.lblCyberwareCategory, 1, 1);
            this.tlpCyberwareCommon.Controls.Add(this.flpCyberwareCommonCheckBoxes, 2, 6);
            this.tlpCyberwareCommon.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpCyberwareCommon.Location = new System.Drawing.Point(3, 16);
            this.tlpCyberwareCommon.Margin = new System.Windows.Forms.Padding(0);
            this.tlpCyberwareCommon.Name = "tlpCyberwareCommon";
            this.tlpCyberwareCommon.RowCount = 7;
            this.tlpCyberwareCommon.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpCyberwareCommon.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpCyberwareCommon.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpCyberwareCommon.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpCyberwareCommon.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpCyberwareCommon.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpCyberwareCommon.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpCyberwareCommon.Size = new System.Drawing.Size(505, 182);
            this.tlpCyberwareCommon.TabIndex = 253;
            // 
            // lblCyberwareNameLabel
            // 
            this.lblCyberwareNameLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblCyberwareNameLabel.AutoSize = true;
            this.lblCyberwareNameLabel.Location = new System.Drawing.Point(17, 6);
            this.lblCyberwareNameLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblCyberwareNameLabel.Name = "lblCyberwareNameLabel";
            this.lblCyberwareNameLabel.Size = new System.Drawing.Size(38, 13);
            this.lblCyberwareNameLabel.TabIndex = 29;
            this.lblCyberwareNameLabel.Tag = "Label_Name";
            this.lblCyberwareNameLabel.Text = "Name:";
            // 
            // nudCyberwareRating
            // 
            this.nudCyberwareRating.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.nudCyberwareRating.AutoSize = true;
            this.nudCyberwareRating.Enabled = false;
            this.nudCyberwareRating.Location = new System.Drawing.Point(328, 28);
            this.nudCyberwareRating.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.nudCyberwareRating.Name = "nudCyberwareRating";
            this.nudCyberwareRating.Size = new System.Drawing.Size(59, 20);
            this.nudCyberwareRating.TabIndex = 44;
            this.nudCyberwareRating.ValueChanged += new System.EventHandler(this.nudCyberwareRating_ValueChanged);
            // 
            // lblCyberwareRatingLabel
            // 
            this.lblCyberwareRatingLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblCyberwareRatingLabel.AutoSize = true;
            this.lblCyberwareRatingLabel.Location = new System.Drawing.Point(281, 31);
            this.lblCyberwareRatingLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblCyberwareRatingLabel.Name = "lblCyberwareRatingLabel";
            this.lblCyberwareRatingLabel.Size = new System.Drawing.Size(41, 13);
            this.lblCyberwareRatingLabel.TabIndex = 42;
            this.lblCyberwareRatingLabel.Tag = "Label_Rating";
            this.lblCyberwareRatingLabel.Text = "Rating:";
            // 
            // lblCyberwareCapacity
            // 
            this.lblCyberwareCapacity.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblCyberwareCapacity.AutoSize = true;
            this.lblCyberwareCapacity.Location = new System.Drawing.Point(328, 58);
            this.lblCyberwareCapacity.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblCyberwareCapacity.Name = "lblCyberwareCapacity";
            this.lblCyberwareCapacity.Size = new System.Drawing.Size(54, 13);
            this.lblCyberwareCapacity.TabIndex = 36;
            this.lblCyberwareCapacity.Text = "[Capacity]";
            // 
            // lblCyberwareCost
            // 
            this.lblCyberwareCost.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblCyberwareCost.AutoSize = true;
            this.lblCyberwareCost.Location = new System.Drawing.Point(328, 84);
            this.lblCyberwareCost.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblCyberwareCost.Name = "lblCyberwareCost";
            this.lblCyberwareCost.Size = new System.Drawing.Size(34, 13);
            this.lblCyberwareCost.TabIndex = 41;
            this.lblCyberwareCost.Text = "[Cost]";
            // 
            // lblCyberwareCostLabel
            // 
            this.lblCyberwareCostLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblCyberwareCostLabel.AutoSize = true;
            this.lblCyberwareCostLabel.Location = new System.Drawing.Point(291, 84);
            this.lblCyberwareCostLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblCyberwareCostLabel.Name = "lblCyberwareCostLabel";
            this.lblCyberwareCostLabel.Size = new System.Drawing.Size(31, 13);
            this.lblCyberwareCostLabel.TabIndex = 40;
            this.lblCyberwareCostLabel.Tag = "Label_Cost";
            this.lblCyberwareCostLabel.Text = "Cost:";
            // 
            // lblCyberlimbAGI
            // 
            this.lblCyberlimbAGI.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblCyberlimbAGI.AutoSize = true;
            this.lblCyberlimbAGI.Location = new System.Drawing.Point(328, 109);
            this.lblCyberlimbAGI.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblCyberlimbAGI.Name = "lblCyberlimbAGI";
            this.lblCyberlimbAGI.Size = new System.Drawing.Size(19, 13);
            this.lblCyberlimbAGI.TabIndex = 224;
            this.lblCyberlimbAGI.Text = "[0]";
            this.lblCyberlimbAGI.Visible = false;
            // 
            // lblCyberlimbAGILabel
            // 
            this.lblCyberlimbAGILabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblCyberlimbAGILabel.AutoSize = true;
            this.lblCyberlimbAGILabel.Location = new System.Drawing.Point(258, 109);
            this.lblCyberlimbAGILabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblCyberlimbAGILabel.Name = "lblCyberlimbAGILabel";
            this.lblCyberlimbAGILabel.Size = new System.Drawing.Size(64, 13);
            this.lblCyberlimbAGILabel.TabIndex = 222;
            this.lblCyberlimbAGILabel.Tag = "Label_CyberlimbAGI";
            this.lblCyberlimbAGILabel.Text = "Agility (AGI):";
            this.lblCyberlimbAGILabel.Visible = false;
            // 
            // lblCyberlimbSTR
            // 
            this.lblCyberlimbSTR.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblCyberlimbSTR.AutoSize = true;
            this.lblCyberlimbSTR.Location = new System.Drawing.Point(328, 134);
            this.lblCyberlimbSTR.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblCyberlimbSTR.Name = "lblCyberlimbSTR";
            this.lblCyberlimbSTR.Size = new System.Drawing.Size(19, 13);
            this.lblCyberlimbSTR.TabIndex = 225;
            this.lblCyberlimbSTR.Text = "[0]";
            this.lblCyberlimbSTR.Visible = false;
            // 
            // lblCyberlimbSTRLabel
            // 
            this.lblCyberlimbSTRLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblCyberlimbSTRLabel.AutoSize = true;
            this.lblCyberlimbSTRLabel.Location = new System.Drawing.Point(241, 134);
            this.lblCyberlimbSTRLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblCyberlimbSTRLabel.Name = "lblCyberlimbSTRLabel";
            this.lblCyberlimbSTRLabel.Size = new System.Drawing.Size(81, 13);
            this.lblCyberlimbSTRLabel.TabIndex = 223;
            this.lblCyberlimbSTRLabel.Tag = "Label_CyberlimbSTR";
            this.lblCyberlimbSTRLabel.Text = "Strength (STR):";
            this.lblCyberlimbSTRLabel.Visible = false;
            // 
            // lblCyberwareCapacityLabel
            // 
            this.lblCyberwareCapacityLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblCyberwareCapacityLabel.AutoSize = true;
            this.lblCyberwareCapacityLabel.Location = new System.Drawing.Point(271, 58);
            this.lblCyberwareCapacityLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblCyberwareCapacityLabel.Name = "lblCyberwareCapacityLabel";
            this.lblCyberwareCapacityLabel.Size = new System.Drawing.Size(51, 13);
            this.lblCyberwareCapacityLabel.TabIndex = 35;
            this.lblCyberwareCapacityLabel.Tag = "Label_Capacity";
            this.lblCyberwareCapacityLabel.Text = "Capacity:";
            // 
            // lblCyberwareName
            // 
            this.lblCyberwareName.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblCyberwareName.AutoSize = true;
            this.tlpCyberwareCommon.SetColumnSpan(this.lblCyberwareName, 3);
            this.lblCyberwareName.Location = new System.Drawing.Point(61, 6);
            this.lblCyberwareName.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblCyberwareName.Name = "lblCyberwareName";
            this.lblCyberwareName.Size = new System.Drawing.Size(41, 13);
            this.lblCyberwareName.TabIndex = 30;
            this.lblCyberwareName.Text = "[Name]";
            // 
            // cmdCyberwareChangeMount
            // 
            this.cmdCyberwareChangeMount.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.cmdCyberwareChangeMount.AutoSize = true;
            this.cmdCyberwareChangeMount.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpCyberwareCommon.SetColumnSpan(this.cmdCyberwareChangeMount, 2);
            this.cmdCyberwareChangeMount.Location = new System.Drawing.Point(3, 156);
            this.cmdCyberwareChangeMount.Name = "cmdCyberwareChangeMount";
            this.cmdCyberwareChangeMount.Size = new System.Drawing.Size(143, 23);
            this.cmdCyberwareChangeMount.TabIndex = 227;
            this.cmdCyberwareChangeMount.Tag = "Button_ChangeMountedLocation";
            this.cmdCyberwareChangeMount.Text = "Change Mounted Location";
            this.cmdCyberwareChangeMount.UseVisualStyleBackColor = true;
            this.cmdCyberwareChangeMount.Visible = false;
            this.cmdCyberwareChangeMount.Click += new System.EventHandler(this.cmdCyberwareChangeMount_Click);
            // 
            // lblCyberwareCategoryLabel
            // 
            this.lblCyberwareCategoryLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblCyberwareCategoryLabel.AutoSize = true;
            this.lblCyberwareCategoryLabel.Location = new System.Drawing.Point(3, 31);
            this.lblCyberwareCategoryLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblCyberwareCategoryLabel.Name = "lblCyberwareCategoryLabel";
            this.lblCyberwareCategoryLabel.Size = new System.Drawing.Size(52, 13);
            this.lblCyberwareCategoryLabel.TabIndex = 31;
            this.lblCyberwareCategoryLabel.Tag = "Label_Category";
            this.lblCyberwareCategoryLabel.Text = "Category:";
            // 
            // lblCyberwareSource
            // 
            this.lblCyberwareSource.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblCyberwareSource.AutoSize = true;
            this.lblCyberwareSource.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblCyberwareSource.Location = new System.Drawing.Point(61, 134);
            this.lblCyberwareSource.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblCyberwareSource.Name = "lblCyberwareSource";
            this.lblCyberwareSource.Size = new System.Drawing.Size(47, 13);
            this.lblCyberwareSource.TabIndex = 48;
            this.lblCyberwareSource.Text = "[Source]";
            this.lblCyberwareSource.Click += new System.EventHandler(this.OpenSourceFromLabel);
            // 
            // lblCyberwareSourceLabel
            // 
            this.lblCyberwareSourceLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblCyberwareSourceLabel.AutoSize = true;
            this.lblCyberwareSourceLabel.Location = new System.Drawing.Point(11, 134);
            this.lblCyberwareSourceLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblCyberwareSourceLabel.Name = "lblCyberwareSourceLabel";
            this.lblCyberwareSourceLabel.Size = new System.Drawing.Size(44, 13);
            this.lblCyberwareSourceLabel.TabIndex = 47;
            this.lblCyberwareSourceLabel.Tag = "Label_Source";
            this.lblCyberwareSourceLabel.Text = "Source:";
            // 
            // lblCyberwareAvail
            // 
            this.lblCyberwareAvail.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblCyberwareAvail.AutoSize = true;
            this.lblCyberwareAvail.Location = new System.Drawing.Point(61, 109);
            this.lblCyberwareAvail.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblCyberwareAvail.Name = "lblCyberwareAvail";
            this.lblCyberwareAvail.Size = new System.Drawing.Size(36, 13);
            this.lblCyberwareAvail.TabIndex = 39;
            this.lblCyberwareAvail.Text = "[Avail]";
            // 
            // lblCyberwareAvailLabel
            // 
            this.lblCyberwareAvailLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblCyberwareAvailLabel.AutoSize = true;
            this.lblCyberwareAvailLabel.Location = new System.Drawing.Point(22, 109);
            this.lblCyberwareAvailLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblCyberwareAvailLabel.Name = "lblCyberwareAvailLabel";
            this.lblCyberwareAvailLabel.Size = new System.Drawing.Size(33, 13);
            this.lblCyberwareAvailLabel.TabIndex = 38;
            this.lblCyberwareAvailLabel.Tag = "Label_Avail";
            this.lblCyberwareAvailLabel.Text = "Avail:";
            // 
            // lblCyberwareEssence
            // 
            this.lblCyberwareEssence.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblCyberwareEssence.AutoSize = true;
            this.lblCyberwareEssence.Location = new System.Drawing.Point(61, 84);
            this.lblCyberwareEssence.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblCyberwareEssence.Name = "lblCyberwareEssence";
            this.lblCyberwareEssence.Size = new System.Drawing.Size(34, 13);
            this.lblCyberwareEssence.TabIndex = 34;
            this.lblCyberwareEssence.Text = "[ESS]";
            // 
            // lblCyberwareEssenceLabel
            // 
            this.lblCyberwareEssenceLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblCyberwareEssenceLabel.AutoSize = true;
            this.lblCyberwareEssenceLabel.Location = new System.Drawing.Point(4, 84);
            this.lblCyberwareEssenceLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblCyberwareEssenceLabel.Name = "lblCyberwareEssenceLabel";
            this.lblCyberwareEssenceLabel.Size = new System.Drawing.Size(51, 13);
            this.lblCyberwareEssenceLabel.TabIndex = 33;
            this.lblCyberwareEssenceLabel.Tag = "Label_Essence";
            this.lblCyberwareEssenceLabel.Text = "Essence:";
            // 
            // cboCyberwareGrade
            // 
            this.cboCyberwareGrade.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.cboCyberwareGrade.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboCyberwareGrade.FormattingEnabled = true;
            this.cboCyberwareGrade.Location = new System.Drawing.Point(61, 54);
            this.cboCyberwareGrade.Name = "cboCyberwareGrade";
            this.cboCyberwareGrade.Size = new System.Drawing.Size(174, 21);
            this.cboCyberwareGrade.TabIndex = 43;
            this.cboCyberwareGrade.TooltipText = "";
            this.cboCyberwareGrade.Visible = false;
            this.cboCyberwareGrade.SelectedIndexChanged += new System.EventHandler(this.cboCyberwareGrade_SelectedIndexChanged);
            // 
            // lblCyberwareGradeLabel
            // 
            this.lblCyberwareGradeLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblCyberwareGradeLabel.AutoSize = true;
            this.lblCyberwareGradeLabel.Location = new System.Drawing.Point(16, 58);
            this.lblCyberwareGradeLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblCyberwareGradeLabel.Name = "lblCyberwareGradeLabel";
            this.lblCyberwareGradeLabel.Size = new System.Drawing.Size(39, 13);
            this.lblCyberwareGradeLabel.TabIndex = 37;
            this.lblCyberwareGradeLabel.Tag = "Label_Grade";
            this.lblCyberwareGradeLabel.Text = "Grade:";
            // 
            // lblCyberwareCategory
            // 
            this.lblCyberwareCategory.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblCyberwareCategory.AutoSize = true;
            this.lblCyberwareCategory.Location = new System.Drawing.Point(61, 31);
            this.lblCyberwareCategory.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblCyberwareCategory.Name = "lblCyberwareCategory";
            this.lblCyberwareCategory.Size = new System.Drawing.Size(55, 13);
            this.lblCyberwareCategory.TabIndex = 32;
            this.lblCyberwareCategory.Text = "[Category]";
            // 
            // flpCyberwareCommonCheckBoxes
            // 
            this.flpCyberwareCommonCheckBoxes.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.flpCyberwareCommonCheckBoxes.AutoSize = true;
            this.flpCyberwareCommonCheckBoxes.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpCyberwareCommon.SetColumnSpan(this.flpCyberwareCommonCheckBoxes, 2);
            this.flpCyberwareCommonCheckBoxes.Controls.Add(this.chkCyberwareStolen);
            this.flpCyberwareCommonCheckBoxes.Controls.Add(this.chkPrototypeTranshuman);
            this.flpCyberwareCommonCheckBoxes.Location = new System.Drawing.Point(238, 156);
            this.flpCyberwareCommonCheckBoxes.Margin = new System.Windows.Forms.Padding(0);
            this.flpCyberwareCommonCheckBoxes.Name = "flpCyberwareCommonCheckBoxes";
            this.flpCyberwareCommonCheckBoxes.Size = new System.Drawing.Size(201, 23);
            this.flpCyberwareCommonCheckBoxes.TabIndex = 229;
            this.flpCyberwareCommonCheckBoxes.WrapContents = false;
            // 
            // chkCyberwareStolen
            // 
            this.chkCyberwareStolen.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkCyberwareStolen.AutoSize = true;
            this.chkCyberwareStolen.Location = new System.Drawing.Point(3, 3);
            this.chkCyberwareStolen.Name = "chkCyberwareStolen";
            this.chkCyberwareStolen.Size = new System.Drawing.Size(56, 17);
            this.chkCyberwareStolen.TabIndex = 228;
            this.chkCyberwareStolen.Tag = "Checkbox_Stolen";
            this.chkCyberwareStolen.Text = "Stolen";
            this.chkCyberwareStolen.UseVisualStyleBackColor = true;
            this.chkCyberwareStolen.Visible = false;
            this.chkCyberwareStolen.CheckedChanged += new System.EventHandler(this.chkCyberwareStolen_CheckedChanged);
            // 
            // chkPrototypeTranshuman
            // 
            this.chkPrototypeTranshuman.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkPrototypeTranshuman.AutoSize = true;
            this.chkPrototypeTranshuman.Location = new System.Drawing.Point(65, 3);
            this.chkPrototypeTranshuman.Name = "chkPrototypeTranshuman";
            this.chkPrototypeTranshuman.Size = new System.Drawing.Size(133, 17);
            this.chkPrototypeTranshuman.TabIndex = 226;
            this.chkPrototypeTranshuman.Tag = "Checkbox_PrototypeTranshuman";
            this.chkPrototypeTranshuman.Text = "Prototype Transhuman";
            this.chkPrototypeTranshuman.UseVisualStyleBackColor = true;
            this.chkPrototypeTranshuman.Visible = false;
            this.chkPrototypeTranshuman.CheckedChanged += new System.EventHandler(this.chkPrototypeTranshuman_CheckedChanged);
            // 
            // gpbCyberwareMatrix
            // 
            this.gpbCyberwareMatrix.AutoSize = true;
            this.gpbCyberwareMatrix.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.gpbCyberwareMatrix.Controls.Add(this.tlpCyberwareMatrix);
            this.gpbCyberwareMatrix.Dock = System.Windows.Forms.DockStyle.Top;
            this.gpbCyberwareMatrix.Location = new System.Drawing.Point(3, 335);
            this.gpbCyberwareMatrix.MinimumSize = new System.Drawing.Size(501, 0);
            this.gpbCyberwareMatrix.Name = "gpbCyberwareMatrix";
            this.gpbCyberwareMatrix.Size = new System.Drawing.Size(511, 94);
            this.gpbCyberwareMatrix.TabIndex = 254;
            this.gpbCyberwareMatrix.TabStop = false;
            this.gpbCyberwareMatrix.Tag = "String_Matrix";
            this.gpbCyberwareMatrix.Text = "Matrix";
            // 
            // tlpCyberwareMatrix
            // 
            this.tlpCyberwareMatrix.AutoSize = true;
            this.tlpCyberwareMatrix.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpCyberwareMatrix.ColumnCount = 5;
            this.tlpCyberwareMatrix.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tlpCyberwareMatrix.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tlpCyberwareMatrix.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tlpCyberwareMatrix.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tlpCyberwareMatrix.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tlpCyberwareMatrix.Controls.Add(this.lblCyberFirewallLabel, 4, 1);
            this.tlpCyberwareMatrix.Controls.Add(this.lblCyberSleazeLabel, 2, 1);
            this.tlpCyberwareMatrix.Controls.Add(this.lblCyberDeviceRatingLabel, 0, 1);
            this.tlpCyberwareMatrix.Controls.Add(this.lblCyberDeviceRating, 0, 2);
            this.tlpCyberwareMatrix.Controls.Add(this.lblCyberAttackLabel, 1, 1);
            this.tlpCyberwareMatrix.Controls.Add(this.lblCyberDataProcessingLabel, 3, 1);
            this.tlpCyberwareMatrix.Controls.Add(this.cboCyberwareAttack, 1, 2);
            this.tlpCyberwareMatrix.Controls.Add(this.cboCyberwareSleaze, 2, 2);
            this.tlpCyberwareMatrix.Controls.Add(this.cboCyberwareDataProcessing, 3, 2);
            this.tlpCyberwareMatrix.Controls.Add(this.cboCyberwareFirewall, 4, 2);
            this.tlpCyberwareMatrix.Controls.Add(this.flpCyberwareMatrixCheckBoxes, 2, 0);
            this.tlpCyberwareMatrix.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpCyberwareMatrix.Location = new System.Drawing.Point(3, 16);
            this.tlpCyberwareMatrix.Margin = new System.Windows.Forms.Padding(0);
            this.tlpCyberwareMatrix.Name = "tlpCyberwareMatrix";
            this.tlpCyberwareMatrix.RowCount = 3;
            this.tlpCyberwareMatrix.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpCyberwareMatrix.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpCyberwareMatrix.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpCyberwareMatrix.Size = new System.Drawing.Size(505, 75);
            this.tlpCyberwareMatrix.TabIndex = 251;
            // 
            // lblCyberFirewallLabel
            // 
            this.lblCyberFirewallLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblCyberFirewallLabel.AutoSize = true;
            this.lblCyberFirewallLabel.Location = new System.Drawing.Point(407, 29);
            this.lblCyberFirewallLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblCyberFirewallLabel.Name = "lblCyberFirewallLabel";
            this.lblCyberFirewallLabel.Size = new System.Drawing.Size(45, 13);
            this.lblCyberFirewallLabel.TabIndex = 174;
            this.lblCyberFirewallLabel.Tag = "Label_Firewall";
            this.lblCyberFirewallLabel.Text = "Firewall:";
            // 
            // lblCyberSleazeLabel
            // 
            this.lblCyberSleazeLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblCyberSleazeLabel.AutoSize = true;
            this.lblCyberSleazeLabel.Location = new System.Drawing.Point(205, 29);
            this.lblCyberSleazeLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblCyberSleazeLabel.Name = "lblCyberSleazeLabel";
            this.lblCyberSleazeLabel.Size = new System.Drawing.Size(42, 13);
            this.lblCyberSleazeLabel.TabIndex = 170;
            this.lblCyberSleazeLabel.Tag = "Label_Sleaze";
            this.lblCyberSleazeLabel.Text = "Sleaze:";
            // 
            // lblCyberDeviceRatingLabel
            // 
            this.lblCyberDeviceRatingLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblCyberDeviceRatingLabel.AutoSize = true;
            this.lblCyberDeviceRatingLabel.Location = new System.Drawing.Point(3, 29);
            this.lblCyberDeviceRatingLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblCyberDeviceRatingLabel.Name = "lblCyberDeviceRatingLabel";
            this.lblCyberDeviceRatingLabel.Size = new System.Drawing.Size(78, 13);
            this.lblCyberDeviceRatingLabel.TabIndex = 166;
            this.lblCyberDeviceRatingLabel.Tag = "Label_DeviceRating";
            this.lblCyberDeviceRatingLabel.Text = "Device Rating:";
            // 
            // lblCyberDeviceRating
            // 
            this.lblCyberDeviceRating.AutoSize = true;
            this.lblCyberDeviceRating.Location = new System.Drawing.Point(3, 54);
            this.lblCyberDeviceRating.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblCyberDeviceRating.Name = "lblCyberDeviceRating";
            this.lblCyberDeviceRating.Size = new System.Drawing.Size(19, 13);
            this.lblCyberDeviceRating.TabIndex = 167;
            this.lblCyberDeviceRating.Text = "[0]";
            // 
            // lblCyberAttackLabel
            // 
            this.lblCyberAttackLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblCyberAttackLabel.AutoSize = true;
            this.lblCyberAttackLabel.Location = new System.Drawing.Point(104, 29);
            this.lblCyberAttackLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblCyberAttackLabel.Name = "lblCyberAttackLabel";
            this.lblCyberAttackLabel.Size = new System.Drawing.Size(41, 13);
            this.lblCyberAttackLabel.TabIndex = 168;
            this.lblCyberAttackLabel.Tag = "Label_Attack";
            this.lblCyberAttackLabel.Text = "Attack:";
            // 
            // lblCyberDataProcessingLabel
            // 
            this.lblCyberDataProcessingLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblCyberDataProcessingLabel.AutoSize = true;
            this.lblCyberDataProcessingLabel.Location = new System.Drawing.Point(306, 29);
            this.lblCyberDataProcessingLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblCyberDataProcessingLabel.Name = "lblCyberDataProcessingLabel";
            this.lblCyberDataProcessingLabel.Size = new System.Drawing.Size(88, 13);
            this.lblCyberDataProcessingLabel.TabIndex = 172;
            this.lblCyberDataProcessingLabel.Tag = "Label_DataProcessing";
            this.lblCyberDataProcessingLabel.Text = "Data Processing:";
            // 
            // cboCyberwareAttack
            // 
            this.cboCyberwareAttack.Dock = System.Windows.Forms.DockStyle.Top;
            this.cboCyberwareAttack.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboCyberwareAttack.FormattingEnabled = true;
            this.cboCyberwareAttack.Location = new System.Drawing.Point(104, 51);
            this.cboCyberwareAttack.Name = "cboCyberwareAttack";
            this.cboCyberwareAttack.Size = new System.Drawing.Size(95, 21);
            this.cboCyberwareAttack.TabIndex = 248;
            this.cboCyberwareAttack.SelectedIndexChanged += new System.EventHandler(this.cboCyberwareAttack_SelectedIndexChanged);
            // 
            // cboCyberwareSleaze
            // 
            this.cboCyberwareSleaze.Dock = System.Windows.Forms.DockStyle.Top;
            this.cboCyberwareSleaze.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboCyberwareSleaze.FormattingEnabled = true;
            this.cboCyberwareSleaze.Location = new System.Drawing.Point(205, 51);
            this.cboCyberwareSleaze.Name = "cboCyberwareSleaze";
            this.cboCyberwareSleaze.Size = new System.Drawing.Size(95, 21);
            this.cboCyberwareSleaze.TabIndex = 249;
            this.cboCyberwareSleaze.SelectedIndexChanged += new System.EventHandler(this.cboCyberwareSleaze_SelectedIndexChanged);
            // 
            // cboCyberwareDataProcessing
            // 
            this.cboCyberwareDataProcessing.Dock = System.Windows.Forms.DockStyle.Top;
            this.cboCyberwareDataProcessing.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboCyberwareDataProcessing.FormattingEnabled = true;
            this.cboCyberwareDataProcessing.Location = new System.Drawing.Point(306, 51);
            this.cboCyberwareDataProcessing.Name = "cboCyberwareDataProcessing";
            this.cboCyberwareDataProcessing.Size = new System.Drawing.Size(95, 21);
            this.cboCyberwareDataProcessing.TabIndex = 250;
            this.cboCyberwareDataProcessing.SelectedIndexChanged += new System.EventHandler(this.cboCyberwareDataProcessing_SelectedIndexChanged);
            // 
            // cboCyberwareFirewall
            // 
            this.cboCyberwareFirewall.Dock = System.Windows.Forms.DockStyle.Top;
            this.cboCyberwareFirewall.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboCyberwareFirewall.FormattingEnabled = true;
            this.cboCyberwareFirewall.Location = new System.Drawing.Point(407, 51);
            this.cboCyberwareFirewall.Name = "cboCyberwareFirewall";
            this.cboCyberwareFirewall.Size = new System.Drawing.Size(95, 21);
            this.cboCyberwareFirewall.TabIndex = 251;
            this.cboCyberwareFirewall.SelectedIndexChanged += new System.EventHandler(this.cboCyberwareFirewall_SelectedIndexChanged);
            // 
            // flpCyberwareMatrixCheckBoxes
            // 
            this.flpCyberwareMatrixCheckBoxes.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.flpCyberwareMatrixCheckBoxes.AutoSize = true;
            this.flpCyberwareMatrixCheckBoxes.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpCyberwareMatrix.SetColumnSpan(this.flpCyberwareMatrixCheckBoxes, 3);
            this.flpCyberwareMatrixCheckBoxes.Controls.Add(this.chkCyberwareHomeNode);
            this.flpCyberwareMatrixCheckBoxes.Controls.Add(this.chkCyberwareActiveCommlink);
            this.flpCyberwareMatrixCheckBoxes.Location = new System.Drawing.Point(202, 0);
            this.flpCyberwareMatrixCheckBoxes.Margin = new System.Windows.Forms.Padding(0);
            this.flpCyberwareMatrixCheckBoxes.Name = "flpCyberwareMatrixCheckBoxes";
            this.flpCyberwareMatrixCheckBoxes.Size = new System.Drawing.Size(199, 23);
            this.flpCyberwareMatrixCheckBoxes.TabIndex = 252;
            this.flpCyberwareMatrixCheckBoxes.WrapContents = false;
            // 
            // chkCyberwareHomeNode
            // 
            this.chkCyberwareHomeNode.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkCyberwareHomeNode.AutoSize = true;
            this.chkCyberwareHomeNode.Location = new System.Drawing.Point(3, 3);
            this.chkCyberwareHomeNode.Name = "chkCyberwareHomeNode";
            this.chkCyberwareHomeNode.Size = new System.Drawing.Size(83, 17);
            this.chkCyberwareHomeNode.TabIndex = 246;
            this.chkCyberwareHomeNode.Tag = "Checkbox_HomeNode";
            this.chkCyberwareHomeNode.Text = "Home Node";
            this.chkCyberwareHomeNode.UseVisualStyleBackColor = true;
            this.chkCyberwareHomeNode.CheckedChanged += new System.EventHandler(this.chkCyberwareHomeNode_CheckedChanged);
            // 
            // chkCyberwareActiveCommlink
            // 
            this.chkCyberwareActiveCommlink.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkCyberwareActiveCommlink.AutoSize = true;
            this.chkCyberwareActiveCommlink.Location = new System.Drawing.Point(92, 3);
            this.chkCyberwareActiveCommlink.Name = "chkCyberwareActiveCommlink";
            this.chkCyberwareActiveCommlink.Size = new System.Drawing.Size(104, 17);
            this.chkCyberwareActiveCommlink.TabIndex = 247;
            this.chkCyberwareActiveCommlink.Tag = "Checkbox_ActiveCommlink";
            this.chkCyberwareActiveCommlink.Text = "Active Commlink";
            this.chkCyberwareActiveCommlink.UseVisualStyleBackColor = true;
            this.chkCyberwareActiveCommlink.CheckedChanged += new System.EventHandler(this.chkCyberwareActiveCommlink_CheckedChanged);
            // 
            // tlpCyberwareButtons
            // 
            this.tlpCyberwareButtons.AutoSize = true;
            this.tlpCyberwareButtons.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpCyberwareButtons.ColumnCount = 3;
            this.tlpCyberware.SetColumnSpan(this.tlpCyberwareButtons, 2);
            this.tlpCyberwareButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tlpCyberwareButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tlpCyberwareButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tlpCyberwareButtons.Controls.Add(this.cmdAddCyberware, 0, 0);
            this.tlpCyberwareButtons.Controls.Add(this.cmdDeleteCyberware, 2, 0);
            this.tlpCyberwareButtons.Controls.Add(this.cmdAddBioware, 1, 0);
            this.tlpCyberwareButtons.Location = new System.Drawing.Point(0, 0);
            this.tlpCyberwareButtons.Margin = new System.Windows.Forms.Padding(0);
            this.tlpCyberwareButtons.Name = "tlpCyberwareButtons";
            this.tlpCyberwareButtons.RowCount = 1;
            this.tlpCyberwareButtons.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpCyberwareButtons.Size = new System.Drawing.Size(339, 29);
            this.tlpCyberwareButtons.TabIndex = 253;
            // 
            // cmdAddCyberware
            // 
            this.cmdAddCyberware.AutoSize = true;
            this.cmdAddCyberware.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdAddCyberware.ContextMenuStrip = this.cmsCyberware;
            this.cmdAddCyberware.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmdAddCyberware.Location = new System.Drawing.Point(3, 3);
            this.cmdAddCyberware.Name = "cmdAddCyberware";
            this.cmdAddCyberware.Size = new System.Drawing.Size(107, 23);
            this.cmdAddCyberware.SplitMenuStrip = this.cmsCyberware;
            this.cmdAddCyberware.TabIndex = 91;
            this.cmdAddCyberware.Tag = "Button_AddCyberware";
            this.cmdAddCyberware.Text = "&Add Cyberware";
            this.cmdAddCyberware.UseVisualStyleBackColor = true;
            this.cmdAddCyberware.Click += new System.EventHandler(this.cmdAddCyberware_Click);
            // 
            // cmdDeleteCyberware
            // 
            this.cmdDeleteCyberware.AutoSize = true;
            this.cmdDeleteCyberware.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdDeleteCyberware.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmdDeleteCyberware.Location = new System.Drawing.Point(229, 3);
            this.cmdDeleteCyberware.Name = "cmdDeleteCyberware";
            this.cmdDeleteCyberware.Size = new System.Drawing.Size(107, 23);
            this.cmdDeleteCyberware.TabIndex = 45;
            this.cmdDeleteCyberware.Tag = "String_Delete";
            this.cmdDeleteCyberware.Text = "Delete";
            this.cmdDeleteCyberware.UseVisualStyleBackColor = true;
            this.cmdDeleteCyberware.Click += new System.EventHandler(this.cmdDeleteCyberware_Click);
            // 
            // cmdAddBioware
            // 
            this.cmdAddBioware.AutoSize = true;
            this.cmdAddBioware.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdAddBioware.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmdAddBioware.Location = new System.Drawing.Point(116, 3);
            this.cmdAddBioware.Name = "cmdAddBioware";
            this.cmdAddBioware.Size = new System.Drawing.Size(107, 23);
            this.cmdAddBioware.TabIndex = 46;
            this.cmdAddBioware.Tag = "Button_AddBioware";
            this.cmdAddBioware.Text = "A&dd Bioware";
            this.cmdAddBioware.UseVisualStyleBackColor = true;
            this.cmdAddBioware.Click += new System.EventHandler(this.cmdAddBioware_Click);
            // 
            // tabStreetGear
            // 
            this.tabStreetGear.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.tabStreetGear.Controls.Add(this.tabStreetGearTabs);
            this.tabStreetGear.Location = new System.Drawing.Point(4, 22);
            this.tabStreetGear.Name = "tabStreetGear";
            this.tabStreetGear.Size = new System.Drawing.Size(977, 631);
            this.tabStreetGear.TabIndex = 5;
            this.tabStreetGear.Tag = "Tab_StreetGear";
            this.tabStreetGear.Text = "Street Gear";
            // 
            // tabStreetGearTabs
            // 
            this.tabStreetGearTabs.Controls.Add(this.tabGear);
            this.tabStreetGearTabs.Controls.Add(this.tabArmor);
            this.tabStreetGearTabs.Controls.Add(this.tabWeapons);
            this.tabStreetGearTabs.Controls.Add(this.tabDrugs);
            this.tabStreetGearTabs.Controls.Add(this.tabLifestyle);
            this.tabStreetGearTabs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabStreetGearTabs.Location = new System.Drawing.Point(0, 0);
            this.tabStreetGearTabs.Name = "tabStreetGearTabs";
            this.tabStreetGearTabs.SelectedIndex = 0;
            this.tabStreetGearTabs.Size = new System.Drawing.Size(977, 631);
            this.tabStreetGearTabs.TabIndex = 87;
            this.tabStreetGearTabs.SelectedIndexChanged += new System.EventHandler(this.tabStreetGearTabs_SelectedIndexChanged);
            // 
            // tabGear
            // 
            this.tabGear.BackColor = System.Drawing.SystemColors.Control;
            this.tabGear.Controls.Add(this.tlpGear);
            this.tabGear.Location = new System.Drawing.Point(4, 22);
            this.tabGear.Name = "tabGear";
            this.tabGear.Padding = new System.Windows.Forms.Padding(3);
            this.tabGear.Size = new System.Drawing.Size(969, 605);
            this.tabGear.TabIndex = 3;
            this.tabGear.Tag = "Tab_Gear";
            this.tabGear.Text = "Gear";
            // 
            // tlpGear
            // 
            this.tlpGear.AutoSize = true;
            this.tlpGear.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpGear.ColumnCount = 2;
            this.tlpGear.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpGear.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpGear.Controls.Add(this.treGear, 0, 1);
            this.tlpGear.Controls.Add(this.flpGear, 1, 1);
            this.tlpGear.Controls.Add(this.tlpGearButtons, 0, 0);
            this.tlpGear.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpGear.Location = new System.Drawing.Point(3, 3);
            this.tlpGear.Name = "tlpGear";
            this.tlpGear.RowCount = 2;
            this.tlpGear.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpGear.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpGear.Size = new System.Drawing.Size(963, 599);
            this.tlpGear.TabIndex = 160;
            // 
            // treGear
            // 
            this.treGear.AllowDrop = true;
            this.treGear.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treGear.HideSelection = false;
            this.treGear.Location = new System.Drawing.Point(3, 32);
            this.treGear.Name = "treGear";
            treeNode19.Name = "nodGearRoot";
            treeNode19.Tag = "Node_SelectedGear";
            treeNode19.Text = "Selected Gear";
            this.treGear.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            treeNode19});
            this.treGear.ShowNodeToolTips = true;
            this.treGear.Size = new System.Drawing.Size(295, 564);
            this.treGear.TabIndex = 49;
            this.treGear.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treGear_AfterSelect);
            this.treGear.DragOver += new System.Windows.Forms.DragEventHandler(this.treGear_DragOver);
            this.treGear.KeyDown += new System.Windows.Forms.KeyEventHandler(this.treGear_KeyDown);
            this.treGear.MouseDown += new System.Windows.Forms.MouseEventHandler(this.TreeView_MouseDown);
            // 
            // flpGear
            // 
            this.flpGear.AutoScroll = true;
            this.flpGear.Controls.Add(this.gpbGearCommon);
            this.flpGear.Controls.Add(this.gpbGearMatrix);
            this.flpGear.Controls.Add(this.gpbGearBondedFoci);
            this.flpGear.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flpGear.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flpGear.Location = new System.Drawing.Point(301, 29);
            this.flpGear.Margin = new System.Windows.Forms.Padding(0);
            this.flpGear.Name = "flpGear";
            this.flpGear.Size = new System.Drawing.Size(662, 570);
            this.flpGear.TabIndex = 160;
            this.flpGear.WrapContents = false;
            // 
            // gpbGearCommon
            // 
            this.gpbGearCommon.AutoSize = true;
            this.gpbGearCommon.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.gpbGearCommon.Controls.Add(this.tlpGearCommon);
            this.gpbGearCommon.Dock = System.Windows.Forms.DockStyle.Top;
            this.gpbGearCommon.Location = new System.Drawing.Point(3, 3);
            this.gpbGearCommon.Name = "gpbGearCommon";
            this.gpbGearCommon.Size = new System.Drawing.Size(506, 146);
            this.gpbGearCommon.TabIndex = 0;
            this.gpbGearCommon.TabStop = false;
            this.gpbGearCommon.Tag = "String_Info";
            this.gpbGearCommon.Text = "Info";
            // 
            // tlpGearCommon
            // 
            this.tlpGearCommon.AutoSize = true;
            this.tlpGearCommon.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpGearCommon.ColumnCount = 4;
            this.tlpGearCommon.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpGearCommon.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpGearCommon.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpGearCommon.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpGearCommon.Controls.Add(this.lblGearNameLabel, 0, 0);
            this.tlpGearCommon.Controls.Add(this.lblGearName, 1, 0);
            this.tlpGearCommon.Controls.Add(this.lblGearCategoryLabel, 0, 1);
            this.tlpGearCommon.Controls.Add(this.lblGearCategory, 1, 1);
            this.tlpGearCommon.Controls.Add(this.lblGearAvailLabel, 2, 1);
            this.tlpGearCommon.Controls.Add(this.lblGearAvail, 3, 1);
            this.tlpGearCommon.Controls.Add(this.lblGearRatingLabel, 0, 2);
            this.tlpGearCommon.Controls.Add(this.lblGearQtyLabel, 0, 3);
            this.tlpGearCommon.Controls.Add(this.nudGearRating, 1, 2);
            this.tlpGearCommon.Controls.Add(this.nudGearQty, 1, 3);
            this.tlpGearCommon.Controls.Add(this.lblGearCostLabel, 2, 2);
            this.tlpGearCommon.Controls.Add(this.lblGearCost, 3, 2);
            this.tlpGearCommon.Controls.Add(this.lblGearCapacityLabel, 2, 3);
            this.tlpGearCommon.Controls.Add(this.lblGearCapacity, 3, 3);
            this.tlpGearCommon.Controls.Add(this.lblGearSourceLabel, 0, 4);
            this.tlpGearCommon.Controls.Add(this.lblGearSource, 1, 4);
            this.tlpGearCommon.Controls.Add(this.flpGearCommonCheckBoxes, 2, 4);
            this.tlpGearCommon.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpGearCommon.Location = new System.Drawing.Point(3, 16);
            this.tlpGearCommon.Name = "tlpGearCommon";
            this.tlpGearCommon.RowCount = 5;
            this.tlpGearCommon.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpGearCommon.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpGearCommon.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpGearCommon.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpGearCommon.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpGearCommon.Size = new System.Drawing.Size(500, 127);
            this.tlpGearCommon.TabIndex = 0;
            // 
            // lblGearNameLabel
            // 
            this.lblGearNameLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblGearNameLabel.AutoSize = true;
            this.lblGearNameLabel.Location = new System.Drawing.Point(17, 6);
            this.lblGearNameLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblGearNameLabel.Name = "lblGearNameLabel";
            this.lblGearNameLabel.Size = new System.Drawing.Size(38, 13);
            this.lblGearNameLabel.TabIndex = 53;
            this.lblGearNameLabel.Tag = "Label_Name";
            this.lblGearNameLabel.Text = "Name:";
            // 
            // lblGearName
            // 
            this.lblGearName.AutoSize = true;
            this.lblGearName.Location = new System.Drawing.Point(61, 6);
            this.lblGearName.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblGearName.Name = "lblGearName";
            this.lblGearName.Size = new System.Drawing.Size(41, 13);
            this.lblGearName.TabIndex = 54;
            this.lblGearName.Text = "[Name]";
            // 
            // lblGearCategoryLabel
            // 
            this.lblGearCategoryLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblGearCategoryLabel.AutoSize = true;
            this.lblGearCategoryLabel.Location = new System.Drawing.Point(3, 31);
            this.lblGearCategoryLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblGearCategoryLabel.Name = "lblGearCategoryLabel";
            this.lblGearCategoryLabel.Size = new System.Drawing.Size(52, 13);
            this.lblGearCategoryLabel.TabIndex = 55;
            this.lblGearCategoryLabel.Tag = "Label_Category";
            this.lblGearCategoryLabel.Text = "Category:";
            // 
            // lblGearCategory
            // 
            this.lblGearCategory.AutoSize = true;
            this.lblGearCategory.Location = new System.Drawing.Point(61, 31);
            this.lblGearCategory.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblGearCategory.Name = "lblGearCategory";
            this.lblGearCategory.Size = new System.Drawing.Size(55, 13);
            this.lblGearCategory.TabIndex = 56;
            this.lblGearCategory.Text = "[Category]";
            // 
            // lblGearAvailLabel
            // 
            this.lblGearAvailLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblGearAvailLabel.AutoSize = true;
            this.lblGearAvailLabel.Location = new System.Drawing.Point(271, 31);
            this.lblGearAvailLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblGearAvailLabel.Name = "lblGearAvailLabel";
            this.lblGearAvailLabel.Size = new System.Drawing.Size(33, 13);
            this.lblGearAvailLabel.TabIndex = 59;
            this.lblGearAvailLabel.Tag = "Label_Avail";
            this.lblGearAvailLabel.Text = "Avail:";
            // 
            // lblGearAvail
            // 
            this.lblGearAvail.AutoSize = true;
            this.lblGearAvail.Location = new System.Drawing.Point(310, 31);
            this.lblGearAvail.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblGearAvail.Name = "lblGearAvail";
            this.lblGearAvail.Size = new System.Drawing.Size(36, 13);
            this.lblGearAvail.TabIndex = 60;
            this.lblGearAvail.Text = "[Avail]";
            // 
            // lblGearRatingLabel
            // 
            this.lblGearRatingLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblGearRatingLabel.AutoSize = true;
            this.lblGearRatingLabel.Location = new System.Drawing.Point(14, 56);
            this.lblGearRatingLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblGearRatingLabel.Name = "lblGearRatingLabel";
            this.lblGearRatingLabel.Size = new System.Drawing.Size(41, 13);
            this.lblGearRatingLabel.TabIndex = 51;
            this.lblGearRatingLabel.Tag = "Label_Rating";
            this.lblGearRatingLabel.Text = "Rating:";
            // 
            // lblGearQtyLabel
            // 
            this.lblGearQtyLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblGearQtyLabel.AutoSize = true;
            this.lblGearQtyLabel.Location = new System.Drawing.Point(29, 82);
            this.lblGearQtyLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblGearQtyLabel.Name = "lblGearQtyLabel";
            this.lblGearQtyLabel.Size = new System.Drawing.Size(26, 13);
            this.lblGearQtyLabel.TabIndex = 63;
            this.lblGearQtyLabel.Tag = "Label_Qty";
            this.lblGearQtyLabel.Text = "Qty:";
            // 
            // nudGearRating
            // 
            this.nudGearRating.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.nudGearRating.AutoSize = true;
            this.nudGearRating.Enabled = false;
            this.nudGearRating.Location = new System.Drawing.Point(61, 53);
            this.nudGearRating.Maximum = new decimal(new int[] {
            6,
            0,
            0,
            0});
            this.nudGearRating.Name = "nudGearRating";
            this.nudGearRating.Size = new System.Drawing.Size(29, 20);
            this.nudGearRating.TabIndex = 52;
            this.nudGearRating.ValueChanged += new System.EventHandler(this.nudGearRating_ValueChanged);
            // 
            // nudGearQty
            // 
            this.nudGearQty.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.nudGearQty.AutoSize = true;
            this.nudGearQty.Enabled = false;
            this.nudGearQty.Location = new System.Drawing.Point(61, 79);
            this.nudGearQty.Maximum = new decimal(new int[] {
            1000000000,
            0,
            0,
            0});
            this.nudGearQty.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudGearQty.Name = "nudGearQty";
            this.nudGearQty.Size = new System.Drawing.Size(77, 20);
            this.nudGearQty.TabIndex = 64;
            this.nudGearQty.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudGearQty.ValueChanged += new System.EventHandler(this.nudGearQty_ValueChanged);
            // 
            // lblGearCostLabel
            // 
            this.lblGearCostLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblGearCostLabel.AutoSize = true;
            this.lblGearCostLabel.Location = new System.Drawing.Point(273, 56);
            this.lblGearCostLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblGearCostLabel.Name = "lblGearCostLabel";
            this.lblGearCostLabel.Size = new System.Drawing.Size(31, 13);
            this.lblGearCostLabel.TabIndex = 61;
            this.lblGearCostLabel.Tag = "Label_Cost";
            this.lblGearCostLabel.Text = "Cost:";
            // 
            // lblGearCost
            // 
            this.lblGearCost.AutoSize = true;
            this.lblGearCost.Location = new System.Drawing.Point(310, 56);
            this.lblGearCost.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblGearCost.Name = "lblGearCost";
            this.lblGearCost.Size = new System.Drawing.Size(34, 13);
            this.lblGearCost.TabIndex = 62;
            this.lblGearCost.Text = "[Cost]";
            // 
            // lblGearCapacityLabel
            // 
            this.lblGearCapacityLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblGearCapacityLabel.AutoSize = true;
            this.lblGearCapacityLabel.Location = new System.Drawing.Point(253, 82);
            this.lblGearCapacityLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblGearCapacityLabel.Name = "lblGearCapacityLabel";
            this.lblGearCapacityLabel.Size = new System.Drawing.Size(51, 13);
            this.lblGearCapacityLabel.TabIndex = 57;
            this.lblGearCapacityLabel.Tag = "Label_Capacity";
            this.lblGearCapacityLabel.Text = "Capacity:";
            // 
            // lblGearCapacity
            // 
            this.lblGearCapacity.AutoSize = true;
            this.lblGearCapacity.Location = new System.Drawing.Point(310, 82);
            this.lblGearCapacity.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblGearCapacity.Name = "lblGearCapacity";
            this.lblGearCapacity.Size = new System.Drawing.Size(54, 13);
            this.lblGearCapacity.TabIndex = 58;
            this.lblGearCapacity.Text = "[Capacity]";
            // 
            // lblGearSourceLabel
            // 
            this.lblGearSourceLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblGearSourceLabel.AutoSize = true;
            this.lblGearSourceLabel.Location = new System.Drawing.Point(11, 108);
            this.lblGearSourceLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblGearSourceLabel.Name = "lblGearSourceLabel";
            this.lblGearSourceLabel.Size = new System.Drawing.Size(44, 13);
            this.lblGearSourceLabel.TabIndex = 73;
            this.lblGearSourceLabel.Tag = "Label_Source";
            this.lblGearSourceLabel.Text = "Source:";
            // 
            // lblGearSource
            // 
            this.lblGearSource.AutoSize = true;
            this.lblGearSource.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblGearSource.Location = new System.Drawing.Point(61, 108);
            this.lblGearSource.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblGearSource.Name = "lblGearSource";
            this.lblGearSource.Size = new System.Drawing.Size(47, 13);
            this.lblGearSource.TabIndex = 74;
            this.lblGearSource.Text = "[Source]";
            this.lblGearSource.Click += new System.EventHandler(this.OpenSourceFromLabel);
            // 
            // flpGearCommonCheckBoxes
            // 
            this.flpGearCommonCheckBoxes.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.flpGearCommonCheckBoxes.AutoSize = true;
            this.flpGearCommonCheckBoxes.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpGearCommon.SetColumnSpan(this.flpGearCommonCheckBoxes, 2);
            this.flpGearCommonCheckBoxes.Controls.Add(this.chkGearEquipped);
            this.flpGearCommonCheckBoxes.Controls.Add(this.chkGearStolen);
            this.flpGearCommonCheckBoxes.Location = new System.Drawing.Point(250, 103);
            this.flpGearCommonCheckBoxes.Margin = new System.Windows.Forms.Padding(0);
            this.flpGearCommonCheckBoxes.Name = "flpGearCommonCheckBoxes";
            this.flpGearCommonCheckBoxes.Size = new System.Drawing.Size(139, 23);
            this.flpGearCommonCheckBoxes.TabIndex = 230;
            this.flpGearCommonCheckBoxes.WrapContents = false;
            // 
            // chkGearEquipped
            // 
            this.chkGearEquipped.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkGearEquipped.AutoSize = true;
            this.chkGearEquipped.Location = new System.Drawing.Point(3, 3);
            this.chkGearEquipped.Name = "chkGearEquipped";
            this.chkGearEquipped.Size = new System.Drawing.Size(71, 17);
            this.chkGearEquipped.TabIndex = 93;
            this.chkGearEquipped.Tag = "Checkbox_Equipped";
            this.chkGearEquipped.Text = "Equipped";
            this.chkGearEquipped.UseVisualStyleBackColor = true;
            this.chkGearEquipped.Visible = false;
            this.chkGearEquipped.CheckedChanged += new System.EventHandler(this.chkGearEquipped_CheckedChanged);
            // 
            // chkGearStolen
            // 
            this.chkGearStolen.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkGearStolen.AutoSize = true;
            this.chkGearStolen.Location = new System.Drawing.Point(80, 3);
            this.chkGearStolen.Name = "chkGearStolen";
            this.chkGearStolen.Size = new System.Drawing.Size(56, 17);
            this.chkGearStolen.TabIndex = 229;
            this.chkGearStolen.Tag = "Checkbox_Stolen";
            this.chkGearStolen.Text = "Stolen";
            this.chkGearStolen.UseVisualStyleBackColor = true;
            this.chkGearStolen.Visible = false;
            this.chkGearStolen.CheckedChanged += new System.EventHandler(this.chkGearStolen_CheckedChanged);
            // 
            // gpbGearMatrix
            // 
            this.gpbGearMatrix.AutoSize = true;
            this.gpbGearMatrix.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.gpbGearMatrix.Controls.Add(this.tlpGearMatrix);
            this.gpbGearMatrix.Dock = System.Windows.Forms.DockStyle.Top;
            this.gpbGearMatrix.Location = new System.Drawing.Point(3, 155);
            this.gpbGearMatrix.Name = "gpbGearMatrix";
            this.gpbGearMatrix.Size = new System.Drawing.Size(506, 94);
            this.gpbGearMatrix.TabIndex = 1;
            this.gpbGearMatrix.TabStop = false;
            this.gpbGearMatrix.Tag = "String_Matrix";
            this.gpbGearMatrix.Text = "Matrix";
            // 
            // tlpGearMatrix
            // 
            this.tlpGearMatrix.AutoSize = true;
            this.tlpGearMatrix.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpGearMatrix.ColumnCount = 5;
            this.tlpGearMatrix.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tlpGearMatrix.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tlpGearMatrix.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tlpGearMatrix.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tlpGearMatrix.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tlpGearMatrix.Controls.Add(this.lblGearDeviceRatingLabel, 0, 1);
            this.tlpGearMatrix.Controls.Add(this.lblGearDeviceRating, 0, 2);
            this.tlpGearMatrix.Controls.Add(this.lblGearAttackLabel, 1, 1);
            this.tlpGearMatrix.Controls.Add(this.lblGearSleazeLabel, 2, 1);
            this.tlpGearMatrix.Controls.Add(this.cboGearFirewall, 4, 2);
            this.tlpGearMatrix.Controls.Add(this.lblGearFirewallLabel, 4, 1);
            this.tlpGearMatrix.Controls.Add(this.cboGearDataProcessing, 3, 2);
            this.tlpGearMatrix.Controls.Add(this.lblGearDataProcessingLabel, 3, 1);
            this.tlpGearMatrix.Controls.Add(this.cboGearAttack, 1, 2);
            this.tlpGearMatrix.Controls.Add(this.cboGearSleaze, 2, 2);
            this.tlpGearMatrix.Controls.Add(this.flpGearMatrixCheckBoxes, 2, 0);
            this.tlpGearMatrix.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpGearMatrix.Location = new System.Drawing.Point(3, 16);
            this.tlpGearMatrix.Name = "tlpGearMatrix";
            this.tlpGearMatrix.RowCount = 3;
            this.tlpGearMatrix.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpGearMatrix.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpGearMatrix.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpGearMatrix.Size = new System.Drawing.Size(500, 75);
            this.tlpGearMatrix.TabIndex = 0;
            // 
            // lblGearDeviceRatingLabel
            // 
            this.lblGearDeviceRatingLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblGearDeviceRatingLabel.AutoSize = true;
            this.lblGearDeviceRatingLabel.Location = new System.Drawing.Point(3, 29);
            this.lblGearDeviceRatingLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblGearDeviceRatingLabel.Name = "lblGearDeviceRatingLabel";
            this.lblGearDeviceRatingLabel.Size = new System.Drawing.Size(78, 13);
            this.lblGearDeviceRatingLabel.TabIndex = 65;
            this.lblGearDeviceRatingLabel.Tag = "Label_DeviceRating";
            this.lblGearDeviceRatingLabel.Text = "Device Rating:";
            // 
            // lblGearDeviceRating
            // 
            this.lblGearDeviceRating.AutoSize = true;
            this.lblGearDeviceRating.Location = new System.Drawing.Point(3, 54);
            this.lblGearDeviceRating.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblGearDeviceRating.Name = "lblGearDeviceRating";
            this.lblGearDeviceRating.Size = new System.Drawing.Size(19, 13);
            this.lblGearDeviceRating.TabIndex = 66;
            this.lblGearDeviceRating.Text = "[0]";
            // 
            // lblGearAttackLabel
            // 
            this.lblGearAttackLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblGearAttackLabel.AutoSize = true;
            this.lblGearAttackLabel.Location = new System.Drawing.Point(103, 29);
            this.lblGearAttackLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblGearAttackLabel.Name = "lblGearAttackLabel";
            this.lblGearAttackLabel.Size = new System.Drawing.Size(41, 13);
            this.lblGearAttackLabel.TabIndex = 148;
            this.lblGearAttackLabel.Tag = "Label_Attack";
            this.lblGearAttackLabel.Text = "Attack:";
            // 
            // lblGearSleazeLabel
            // 
            this.lblGearSleazeLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblGearSleazeLabel.AutoSize = true;
            this.lblGearSleazeLabel.Location = new System.Drawing.Point(203, 29);
            this.lblGearSleazeLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblGearSleazeLabel.Name = "lblGearSleazeLabel";
            this.lblGearSleazeLabel.Size = new System.Drawing.Size(42, 13);
            this.lblGearSleazeLabel.TabIndex = 150;
            this.lblGearSleazeLabel.Tag = "Label_Sleaze";
            this.lblGearSleazeLabel.Text = "Sleaze:";
            // 
            // cboGearFirewall
            // 
            this.cboGearFirewall.Dock = System.Windows.Forms.DockStyle.Top;
            this.cboGearFirewall.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboGearFirewall.FormattingEnabled = true;
            this.cboGearFirewall.Location = new System.Drawing.Point(403, 51);
            this.cboGearFirewall.Name = "cboGearFirewall";
            this.cboGearFirewall.Size = new System.Drawing.Size(94, 21);
            this.cboGearFirewall.TabIndex = 158;
            this.cboGearFirewall.TooltipText = "";
            this.cboGearFirewall.Visible = false;
            this.cboGearFirewall.SelectedIndexChanged += new System.EventHandler(this.cboGearFirewall_SelectedIndexChanged);
            // 
            // lblGearFirewallLabel
            // 
            this.lblGearFirewallLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblGearFirewallLabel.AutoSize = true;
            this.lblGearFirewallLabel.Location = new System.Drawing.Point(403, 29);
            this.lblGearFirewallLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblGearFirewallLabel.Name = "lblGearFirewallLabel";
            this.lblGearFirewallLabel.Size = new System.Drawing.Size(45, 13);
            this.lblGearFirewallLabel.TabIndex = 154;
            this.lblGearFirewallLabel.Tag = "Label_Firewall";
            this.lblGearFirewallLabel.Text = "Firewall:";
            // 
            // cboGearDataProcessing
            // 
            this.cboGearDataProcessing.Dock = System.Windows.Forms.DockStyle.Top;
            this.cboGearDataProcessing.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboGearDataProcessing.FormattingEnabled = true;
            this.cboGearDataProcessing.Location = new System.Drawing.Point(303, 51);
            this.cboGearDataProcessing.Name = "cboGearDataProcessing";
            this.cboGearDataProcessing.Size = new System.Drawing.Size(94, 21);
            this.cboGearDataProcessing.TabIndex = 159;
            this.cboGearDataProcessing.TooltipText = "";
            this.cboGearDataProcessing.Visible = false;
            this.cboGearDataProcessing.SelectedIndexChanged += new System.EventHandler(this.cboGearDataProcessing_SelectedIndexChanged);
            // 
            // lblGearDataProcessingLabel
            // 
            this.lblGearDataProcessingLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblGearDataProcessingLabel.AutoSize = true;
            this.lblGearDataProcessingLabel.Location = new System.Drawing.Point(303, 29);
            this.lblGearDataProcessingLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblGearDataProcessingLabel.Name = "lblGearDataProcessingLabel";
            this.lblGearDataProcessingLabel.Size = new System.Drawing.Size(58, 13);
            this.lblGearDataProcessingLabel.TabIndex = 152;
            this.lblGearDataProcessingLabel.Tag = "Label_DataProcessing";
            this.lblGearDataProcessingLabel.Text = "Data Proc:";
            // 
            // cboGearAttack
            // 
            this.cboGearAttack.Dock = System.Windows.Forms.DockStyle.Top;
            this.cboGearAttack.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboGearAttack.FormattingEnabled = true;
            this.cboGearAttack.Location = new System.Drawing.Point(103, 51);
            this.cboGearAttack.Name = "cboGearAttack";
            this.cboGearAttack.Size = new System.Drawing.Size(94, 21);
            this.cboGearAttack.TabIndex = 156;
            this.cboGearAttack.TooltipText = "";
            this.cboGearAttack.Visible = false;
            this.cboGearAttack.SelectedIndexChanged += new System.EventHandler(this.cboGearAttack_SelectedIndexChanged);
            // 
            // cboGearSleaze
            // 
            this.cboGearSleaze.Dock = System.Windows.Forms.DockStyle.Top;
            this.cboGearSleaze.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboGearSleaze.FormattingEnabled = true;
            this.cboGearSleaze.Location = new System.Drawing.Point(203, 51);
            this.cboGearSleaze.Name = "cboGearSleaze";
            this.cboGearSleaze.Size = new System.Drawing.Size(94, 21);
            this.cboGearSleaze.TabIndex = 157;
            this.cboGearSleaze.TooltipText = "";
            this.cboGearSleaze.Visible = false;
            this.cboGearSleaze.SelectedIndexChanged += new System.EventHandler(this.cboGearSleaze_SelectedIndexChanged);
            // 
            // flpGearMatrixCheckBoxes
            // 
            this.flpGearMatrixCheckBoxes.AutoSize = true;
            this.flpGearMatrixCheckBoxes.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpGearMatrix.SetColumnSpan(this.flpGearMatrixCheckBoxes, 3);
            this.flpGearMatrixCheckBoxes.Controls.Add(this.chkGearHomeNode);
            this.flpGearMatrixCheckBoxes.Controls.Add(this.chkGearActiveCommlink);
            this.flpGearMatrixCheckBoxes.Location = new System.Drawing.Point(200, 0);
            this.flpGearMatrixCheckBoxes.Margin = new System.Windows.Forms.Padding(0);
            this.flpGearMatrixCheckBoxes.Name = "flpGearMatrixCheckBoxes";
            this.flpGearMatrixCheckBoxes.Size = new System.Drawing.Size(199, 23);
            this.flpGearMatrixCheckBoxes.TabIndex = 160;
            // 
            // chkGearHomeNode
            // 
            this.chkGearHomeNode.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkGearHomeNode.AutoSize = true;
            this.chkGearHomeNode.Location = new System.Drawing.Point(3, 3);
            this.chkGearHomeNode.Name = "chkGearHomeNode";
            this.chkGearHomeNode.Size = new System.Drawing.Size(83, 17);
            this.chkGearHomeNode.TabIndex = 108;
            this.chkGearHomeNode.Tag = "Checkbox_HomeNode";
            this.chkGearHomeNode.Text = "Home Node";
            this.chkGearHomeNode.UseVisualStyleBackColor = true;
            this.chkGearHomeNode.Visible = false;
            this.chkGearHomeNode.CheckedChanged += new System.EventHandler(this.chkGearHomeNode_CheckedChanged);
            // 
            // chkGearActiveCommlink
            // 
            this.chkGearActiveCommlink.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkGearActiveCommlink.AutoSize = true;
            this.chkGearActiveCommlink.Location = new System.Drawing.Point(92, 3);
            this.chkGearActiveCommlink.Name = "chkGearActiveCommlink";
            this.chkGearActiveCommlink.Size = new System.Drawing.Size(104, 17);
            this.chkGearActiveCommlink.TabIndex = 115;
            this.chkGearActiveCommlink.Tag = "Checkbox_ActiveCommlink";
            this.chkGearActiveCommlink.Text = "Active Commlink";
            this.chkGearActiveCommlink.UseVisualStyleBackColor = true;
            this.chkGearActiveCommlink.CheckedChanged += new System.EventHandler(this.chkGearActiveCommlink_CheckedChanged);
            // 
            // gpbGearBondedFoci
            // 
            this.gpbGearBondedFoci.AutoSize = true;
            this.gpbGearBondedFoci.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.gpbGearBondedFoci.Controls.Add(this.tlpGearBondedFoci);
            this.gpbGearBondedFoci.Dock = System.Windows.Forms.DockStyle.Top;
            this.gpbGearBondedFoci.Location = new System.Drawing.Point(3, 255);
            this.gpbGearBondedFoci.Name = "gpbGearBondedFoci";
            this.gpbGearBondedFoci.Size = new System.Drawing.Size(506, 254);
            this.gpbGearBondedFoci.TabIndex = 2;
            this.gpbGearBondedFoci.TabStop = false;
            this.gpbGearBondedFoci.Tag = "Label_BondedFoci";
            this.gpbGearBondedFoci.Text = "Bonded Foci";
            // 
            // tlpGearBondedFoci
            // 
            this.tlpGearBondedFoci.AutoSize = true;
            this.tlpGearBondedFoci.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpGearBondedFoci.ColumnCount = 1;
            this.tlpGearBondedFoci.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpGearBondedFoci.Controls.Add(this.treFoci, 0, 1);
            this.tlpGearBondedFoci.Controls.Add(this.cmdCreateStackedFocus, 0, 0);
            this.tlpGearBondedFoci.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpGearBondedFoci.Location = new System.Drawing.Point(3, 16);
            this.tlpGearBondedFoci.Name = "tlpGearBondedFoci";
            this.tlpGearBondedFoci.RowCount = 2;
            this.tlpGearBondedFoci.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpGearBondedFoci.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpGearBondedFoci.Size = new System.Drawing.Size(500, 235);
            this.tlpGearBondedFoci.TabIndex = 0;
            // 
            // treFoci
            // 
            this.treFoci.CheckBoxes = true;
            this.treFoci.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treFoci.Location = new System.Drawing.Point(3, 32);
            this.treFoci.Name = "treFoci";
            this.treFoci.ShowLines = false;
            this.treFoci.ShowPlusMinus = false;
            this.treFoci.ShowRootLines = false;
            this.treFoci.Size = new System.Drawing.Size(494, 200);
            this.treFoci.TabIndex = 91;
            this.treFoci.BeforeCheck += new System.Windows.Forms.TreeViewCancelEventHandler(this.treFoci_BeforeCheck);
            this.treFoci.AfterCheck += new System.Windows.Forms.TreeViewEventHandler(this.treFoci_AfterCheck);
            // 
            // cmdCreateStackedFocus
            // 
            this.cmdCreateStackedFocus.AutoSize = true;
            this.cmdCreateStackedFocus.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdCreateStackedFocus.Location = new System.Drawing.Point(3, 3);
            this.cmdCreateStackedFocus.Name = "cmdCreateStackedFocus";
            this.cmdCreateStackedFocus.Size = new System.Drawing.Size(123, 23);
            this.cmdCreateStackedFocus.TabIndex = 109;
            this.cmdCreateStackedFocus.Tag = "Button_CreateStackedFocus";
            this.cmdCreateStackedFocus.Text = "Create Stacked Focus";
            this.cmdCreateStackedFocus.UseVisualStyleBackColor = true;
            this.cmdCreateStackedFocus.Visible = false;
            this.cmdCreateStackedFocus.Click += new System.EventHandler(this.cmdCreateStackedFocus_Click);
            // 
            // tlpGearButtons
            // 
            this.tlpGearButtons.AutoSize = true;
            this.tlpGearButtons.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpGearButtons.ColumnCount = 4;
            this.tlpGear.SetColumnSpan(this.tlpGearButtons, 2);
            this.tlpGearButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tlpGearButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tlpGearButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tlpGearButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpGearButtons.Controls.Add(this.cmdAddGear, 0, 0);
            this.tlpGearButtons.Controls.Add(this.chkCommlinks, 3, 0);
            this.tlpGearButtons.Controls.Add(this.cmdDeleteGear, 1, 0);
            this.tlpGearButtons.Controls.Add(this.cmdAddLocation, 2, 0);
            this.tlpGearButtons.Location = new System.Drawing.Point(0, 0);
            this.tlpGearButtons.Margin = new System.Windows.Forms.Padding(0);
            this.tlpGearButtons.Name = "tlpGearButtons";
            this.tlpGearButtons.RowCount = 1;
            this.tlpGearButtons.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpGearButtons.Size = new System.Drawing.Size(392, 29);
            this.tlpGearButtons.TabIndex = 161;
            // 
            // cmdAddGear
            // 
            this.cmdAddGear.AutoSize = true;
            this.cmdAddGear.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdAddGear.ContextMenuStrip = this.cmsGearButton;
            this.cmdAddGear.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmdAddGear.Location = new System.Drawing.Point(3, 3);
            this.cmdAddGear.Name = "cmdAddGear";
            this.cmdAddGear.Size = new System.Drawing.Size(80, 23);
            this.cmdAddGear.SplitMenuStrip = this.cmsGearButton;
            this.cmdAddGear.TabIndex = 145;
            this.cmdAddGear.Tag = "Button_AddGear";
            this.cmdAddGear.Text = "&Add Gear";
            this.cmdAddGear.UseVisualStyleBackColor = true;
            this.cmdAddGear.Click += new System.EventHandler(this.cmdAddGear_Click);
            // 
            // chkCommlinks
            // 
            this.chkCommlinks.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkCommlinks.AutoSize = true;
            this.chkCommlinks.Location = new System.Drawing.Point(261, 6);
            this.chkCommlinks.Name = "chkCommlinks";
            this.chkCommlinks.Size = new System.Drawing.Size(128, 17);
            this.chkCommlinks.TabIndex = 114;
            this.chkCommlinks.Tag = "Checkbox_Commlinks";
            this.chkCommlinks.Text = "Only show Commlinks";
            this.chkCommlinks.UseVisualStyleBackColor = true;
            this.chkCommlinks.CheckedChanged += new System.EventHandler(this.chkCommlinks_CheckedChanged);
            // 
            // cmdDeleteGear
            // 
            this.cmdDeleteGear.AutoSize = true;
            this.cmdDeleteGear.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdDeleteGear.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmdDeleteGear.Location = new System.Drawing.Point(89, 3);
            this.cmdDeleteGear.Name = "cmdDeleteGear";
            this.cmdDeleteGear.Size = new System.Drawing.Size(80, 23);
            this.cmdDeleteGear.TabIndex = 50;
            this.cmdDeleteGear.Tag = "String_Delete";
            this.cmdDeleteGear.Text = "Delete";
            this.cmdDeleteGear.UseVisualStyleBackColor = true;
            this.cmdDeleteGear.Click += new System.EventHandler(this.cmdDeleteGear_Click);
            // 
            // cmdAddLocation
            // 
            this.cmdAddLocation.AutoSize = true;
            this.cmdAddLocation.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdAddLocation.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmdAddLocation.Location = new System.Drawing.Point(175, 3);
            this.cmdAddLocation.Name = "cmdAddLocation";
            this.cmdAddLocation.Size = new System.Drawing.Size(80, 23);
            this.cmdAddLocation.TabIndex = 103;
            this.cmdAddLocation.Tag = "Button_AddLocation";
            this.cmdAddLocation.Text = "Add Location";
            this.cmdAddLocation.UseVisualStyleBackColor = true;
            this.cmdAddLocation.Click += new System.EventHandler(this.cmdAddLocation_Click);
            // 
            // tabArmor
            // 
            this.tabArmor.BackColor = System.Drawing.SystemColors.Control;
            this.tabArmor.Controls.Add(this.tlpArmor);
            this.tabArmor.Location = new System.Drawing.Point(4, 22);
            this.tabArmor.Name = "tabArmor";
            this.tabArmor.Padding = new System.Windows.Forms.Padding(3);
            this.tabArmor.Size = new System.Drawing.Size(184, 48);
            this.tabArmor.TabIndex = 1;
            this.tabArmor.Tag = "Tab_Armor";
            this.tabArmor.Text = "Armor";
            // 
            // tlpArmor
            // 
            this.tlpArmor.AutoSize = true;
            this.tlpArmor.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpArmor.ColumnCount = 2;
            this.tlpArmor.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpArmor.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpArmor.Controls.Add(this.treArmor, 0, 1);
            this.tlpArmor.Controls.Add(this.flpArmor, 1, 1);
            this.tlpArmor.Controls.Add(this.tlpArmorButtons, 0, 0);
            this.tlpArmor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpArmor.Location = new System.Drawing.Point(3, 3);
            this.tlpArmor.Name = "tlpArmor";
            this.tlpArmor.RowCount = 2;
            this.tlpArmor.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpArmor.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpArmor.Size = new System.Drawing.Size(178, 42);
            this.tlpArmor.TabIndex = 176;
            // 
            // treArmor
            // 
            this.treArmor.AllowDrop = true;
            this.treArmor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treArmor.HideSelection = false;
            this.treArmor.Location = new System.Drawing.Point(3, 32);
            this.treArmor.Name = "treArmor";
            treeNode20.Name = "nodArmorRoot";
            treeNode20.Tag = "Node_SelectedArmor";
            treeNode20.Text = "Selected Armor";
            this.treArmor.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            treeNode20});
            this.treArmor.ShowNodeToolTips = true;
            this.treArmor.Size = new System.Drawing.Size(295, 7);
            this.treArmor.TabIndex = 69;
            this.treArmor.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treArmor_AfterSelect);
            this.treArmor.DragOver += new System.Windows.Forms.DragEventHandler(this.treArmor_DragOver);
            this.treArmor.KeyDown += new System.Windows.Forms.KeyEventHandler(this.treArmor_KeyDown);
            this.treArmor.MouseDown += new System.Windows.Forms.MouseEventHandler(this.TreeView_MouseDown);
            // 
            // flpArmor
            // 
            this.flpArmor.AutoScroll = true;
            this.flpArmor.Controls.Add(this.gpbArmorCommon);
            this.flpArmor.Controls.Add(this.gpbArmorMatrix);
            this.flpArmor.Controls.Add(this.gpbArmorLocation);
            this.flpArmor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flpArmor.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flpArmor.Location = new System.Drawing.Point(301, 29);
            this.flpArmor.Margin = new System.Windows.Forms.Padding(0);
            this.flpArmor.Name = "flpArmor";
            this.flpArmor.Size = new System.Drawing.Size(1, 13);
            this.flpArmor.TabIndex = 177;
            this.flpArmor.WrapContents = false;
            // 
            // gpbArmorCommon
            // 
            this.gpbArmorCommon.AutoSize = true;
            this.gpbArmorCommon.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.gpbArmorCommon.Controls.Add(this.tlpArmorCommon);
            this.gpbArmorCommon.Dock = System.Windows.Forms.DockStyle.Top;
            this.gpbArmorCommon.Location = new System.Drawing.Point(3, 3);
            this.gpbArmorCommon.Name = "gpbArmorCommon";
            this.gpbArmorCommon.Size = new System.Drawing.Size(426, 164);
            this.gpbArmorCommon.TabIndex = 0;
            this.gpbArmorCommon.TabStop = false;
            this.gpbArmorCommon.Tag = "String_Info";
            this.gpbArmorCommon.Text = "Info";
            // 
            // tlpArmorCommon
            // 
            this.tlpArmorCommon.AutoSize = true;
            this.tlpArmorCommon.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpArmorCommon.ColumnCount = 4;
            this.tlpArmorCommon.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tlpArmorCommon.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tlpArmorCommon.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tlpArmorCommon.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tlpArmorCommon.Controls.Add(this.lblArmorValueLabel, 0, 0);
            this.tlpArmorCommon.Controls.Add(this.lblArmorValue, 1, 0);
            this.tlpArmorCommon.Controls.Add(this.lblArmorRatingLabel, 0, 1);
            this.tlpArmorCommon.Controls.Add(this.nudArmorRating, 1, 1);
            this.tlpArmorCommon.Controls.Add(this.lblArmorAvailLabel, 2, 1);
            this.tlpArmorCommon.Controls.Add(this.lblArmorAvail, 3, 1);
            this.tlpArmorCommon.Controls.Add(this.lblArmorCostLabel, 2, 2);
            this.tlpArmorCommon.Controls.Add(this.lblArmorCost, 3, 2);
            this.tlpArmorCommon.Controls.Add(this.lblArmorCapacityLabel, 0, 2);
            this.tlpArmorCommon.Controls.Add(this.lblArmorSource, 1, 3);
            this.tlpArmorCommon.Controls.Add(this.lblArmorCapacity, 1, 2);
            this.tlpArmorCommon.Controls.Add(this.lblArmorSourceLabel, 0, 3);
            this.tlpArmorCommon.Controls.Add(this.flpArmorCommonCheckBoxes, 2, 3);
            this.tlpArmorCommon.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpArmorCommon.Location = new System.Drawing.Point(3, 16);
            this.tlpArmorCommon.Name = "tlpArmorCommon";
            this.tlpArmorCommon.RowCount = 4;
            this.tlpArmorCommon.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpArmorCommon.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpArmorCommon.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpArmorCommon.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpArmorCommon.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpArmorCommon.Size = new System.Drawing.Size(420, 145);
            this.tlpArmorCommon.TabIndex = 0;
            // 
            // lblArmorValueLabel
            // 
            this.lblArmorValueLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblArmorValueLabel.AutoSize = true;
            this.lblArmorValueLabel.Location = new System.Drawing.Point(65, 6);
            this.lblArmorValueLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblArmorValueLabel.Name = "lblArmorValueLabel";
            this.lblArmorValueLabel.Size = new System.Drawing.Size(37, 13);
            this.lblArmorValueLabel.TabIndex = 132;
            this.lblArmorValueLabel.Tag = "Label_Armor";
            this.lblArmorValueLabel.Text = "Armor:";
            // 
            // lblArmorValue
            // 
            this.lblArmorValue.AutoSize = true;
            this.lblArmorValue.Location = new System.Drawing.Point(108, 6);
            this.lblArmorValue.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblArmorValue.Name = "lblArmorValue";
            this.lblArmorValue.Size = new System.Drawing.Size(20, 13);
            this.lblArmorValue.TabIndex = 133;
            this.lblArmorValue.Text = "[A]";
            // 
            // lblArmorRatingLabel
            // 
            this.lblArmorRatingLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblArmorRatingLabel.AutoSize = true;
            this.lblArmorRatingLabel.Location = new System.Drawing.Point(61, 31);
            this.lblArmorRatingLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblArmorRatingLabel.Name = "lblArmorRatingLabel";
            this.lblArmorRatingLabel.Size = new System.Drawing.Size(41, 13);
            this.lblArmorRatingLabel.TabIndex = 81;
            this.lblArmorRatingLabel.Tag = "Label_Rating";
            this.lblArmorRatingLabel.Text = "Rating:";
            // 
            // nudArmorRating
            // 
            this.nudArmorRating.AutoSize = true;
            this.nudArmorRating.Enabled = false;
            this.nudArmorRating.Location = new System.Drawing.Point(108, 28);
            this.nudArmorRating.Maximum = new decimal(new int[] {
            6,
            0,
            0,
            0});
            this.nudArmorRating.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudArmorRating.Name = "nudArmorRating";
            this.nudArmorRating.Size = new System.Drawing.Size(29, 20);
            this.nudArmorRating.TabIndex = 82;
            this.nudArmorRating.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudArmorRating.ValueChanged += new System.EventHandler(this.nudArmorRating_ValueChanged);
            // 
            // lblArmorAvailLabel
            // 
            this.lblArmorAvailLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblArmorAvailLabel.AutoSize = true;
            this.lblArmorAvailLabel.Location = new System.Drawing.Point(279, 31);
            this.lblArmorAvailLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblArmorAvailLabel.Name = "lblArmorAvailLabel";
            this.lblArmorAvailLabel.Size = new System.Drawing.Size(33, 13);
            this.lblArmorAvailLabel.TabIndex = 74;
            this.lblArmorAvailLabel.Tag = "Label_Avail";
            this.lblArmorAvailLabel.Text = "Avail:";
            // 
            // lblArmorAvail
            // 
            this.lblArmorAvail.AutoSize = true;
            this.lblArmorAvail.Location = new System.Drawing.Point(318, 31);
            this.lblArmorAvail.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblArmorAvail.Name = "lblArmorAvail";
            this.lblArmorAvail.Size = new System.Drawing.Size(36, 13);
            this.lblArmorAvail.TabIndex = 75;
            this.lblArmorAvail.Text = "[Avail]";
            // 
            // lblArmorCostLabel
            // 
            this.lblArmorCostLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblArmorCostLabel.AutoSize = true;
            this.lblArmorCostLabel.Location = new System.Drawing.Point(281, 57);
            this.lblArmorCostLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblArmorCostLabel.Name = "lblArmorCostLabel";
            this.lblArmorCostLabel.Size = new System.Drawing.Size(31, 13);
            this.lblArmorCostLabel.TabIndex = 76;
            this.lblArmorCostLabel.Tag = "Label_Cost";
            this.lblArmorCostLabel.Text = "Cost:";
            // 
            // lblArmorCost
            // 
            this.lblArmorCost.AutoSize = true;
            this.lblArmorCost.Location = new System.Drawing.Point(318, 57);
            this.lblArmorCost.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblArmorCost.Name = "lblArmorCost";
            this.lblArmorCost.Size = new System.Drawing.Size(34, 13);
            this.lblArmorCost.TabIndex = 77;
            this.lblArmorCost.Text = "[Cost]";
            // 
            // lblArmorCapacityLabel
            // 
            this.lblArmorCapacityLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblArmorCapacityLabel.AutoSize = true;
            this.lblArmorCapacityLabel.Location = new System.Drawing.Point(51, 57);
            this.lblArmorCapacityLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblArmorCapacityLabel.Name = "lblArmorCapacityLabel";
            this.lblArmorCapacityLabel.Size = new System.Drawing.Size(51, 13);
            this.lblArmorCapacityLabel.TabIndex = 83;
            this.lblArmorCapacityLabel.Tag = "Label_Capacity";
            this.lblArmorCapacityLabel.Text = "Capacity:";
            // 
            // lblArmorSource
            // 
            this.lblArmorSource.AutoSize = true;
            this.lblArmorSource.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblArmorSource.Location = new System.Drawing.Point(108, 82);
            this.lblArmorSource.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblArmorSource.Name = "lblArmorSource";
            this.lblArmorSource.Size = new System.Drawing.Size(47, 13);
            this.lblArmorSource.TabIndex = 80;
            this.lblArmorSource.Text = "[Source]";
            this.lblArmorSource.Click += new System.EventHandler(this.OpenSourceFromLabel);
            // 
            // lblArmorCapacity
            // 
            this.lblArmorCapacity.AutoSize = true;
            this.lblArmorCapacity.Location = new System.Drawing.Point(108, 57);
            this.lblArmorCapacity.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblArmorCapacity.Name = "lblArmorCapacity";
            this.lblArmorCapacity.Size = new System.Drawing.Size(54, 13);
            this.lblArmorCapacity.TabIndex = 84;
            this.lblArmorCapacity.Text = "[Capacity]";
            // 
            // lblArmorSourceLabel
            // 
            this.lblArmorSourceLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblArmorSourceLabel.AutoSize = true;
            this.lblArmorSourceLabel.Location = new System.Drawing.Point(58, 82);
            this.lblArmorSourceLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblArmorSourceLabel.Name = "lblArmorSourceLabel";
            this.lblArmorSourceLabel.Size = new System.Drawing.Size(44, 13);
            this.lblArmorSourceLabel.TabIndex = 79;
            this.lblArmorSourceLabel.Tag = "Label_Source";
            this.lblArmorSourceLabel.Text = "Source:";
            // 
            // flpArmorCommonCheckBoxes
            // 
            this.flpArmorCommonCheckBoxes.AutoSize = true;
            this.flpArmorCommonCheckBoxes.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpArmorCommon.SetColumnSpan(this.flpArmorCommonCheckBoxes, 2);
            this.flpArmorCommonCheckBoxes.Controls.Add(this.chkArmorEquipped);
            this.flpArmorCommonCheckBoxes.Controls.Add(this.chkIncludedInArmor);
            this.flpArmorCommonCheckBoxes.Controls.Add(this.chkArmorStolen);
            this.flpArmorCommonCheckBoxes.Dock = System.Windows.Forms.DockStyle.Top;
            this.flpArmorCommonCheckBoxes.Location = new System.Drawing.Point(210, 76);
            this.flpArmorCommonCheckBoxes.Margin = new System.Windows.Forms.Padding(0);
            this.flpArmorCommonCheckBoxes.Name = "flpArmorCommonCheckBoxes";
            this.flpArmorCommonCheckBoxes.Size = new System.Drawing.Size(210, 46);
            this.flpArmorCommonCheckBoxes.TabIndex = 134;
            // 
            // chkIncludedInArmor
            // 
            this.chkIncludedInArmor.AutoSize = true;
            this.chkIncludedInArmor.Enabled = false;
            this.chkIncludedInArmor.Location = new System.Drawing.Point(80, 3);
            this.chkIncludedInArmor.Name = "chkIncludedInArmor";
            this.chkIncludedInArmor.Size = new System.Drawing.Size(113, 17);
            this.chkIncludedInArmor.TabIndex = 113;
            this.chkIncludedInArmor.Tag = "Checkbox_BaseArmor";
            this.chkIncludedInArmor.Text = "Part of base Armor";
            this.chkIncludedInArmor.UseVisualStyleBackColor = true;
            // 
            // chkArmorStolen
            // 
            this.chkArmorStolen.AutoSize = true;
            this.chkArmorStolen.Location = new System.Drawing.Point(3, 26);
            this.chkArmorStolen.Name = "chkArmorStolen";
            this.chkArmorStolen.Size = new System.Drawing.Size(56, 17);
            this.chkArmorStolen.TabIndex = 230;
            this.chkArmorStolen.Tag = "Checkbox_Stolen";
            this.chkArmorStolen.Text = "Stolen";
            this.chkArmorStolen.UseVisualStyleBackColor = true;
            this.chkArmorStolen.Visible = false;
            this.chkArmorStolen.CheckedChanged += new System.EventHandler(this.chkArmorStolen_CheckedChanged);
            // 
            // gpbArmorMatrix
            // 
            this.gpbArmorMatrix.AutoSize = true;
            this.gpbArmorMatrix.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.gpbArmorMatrix.Controls.Add(this.tlpArmorMatrix);
            this.gpbArmorMatrix.Dock = System.Windows.Forms.DockStyle.Top;
            this.gpbArmorMatrix.Location = new System.Drawing.Point(3, 173);
            this.gpbArmorMatrix.Name = "gpbArmorMatrix";
            this.gpbArmorMatrix.Size = new System.Drawing.Size(426, 94);
            this.gpbArmorMatrix.TabIndex = 1;
            this.gpbArmorMatrix.TabStop = false;
            this.gpbArmorMatrix.Tag = "String_Matrix";
            this.gpbArmorMatrix.Text = "Matrix";
            // 
            // tlpArmorMatrix
            // 
            this.tlpArmorMatrix.AutoSize = true;
            this.tlpArmorMatrix.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpArmorMatrix.ColumnCount = 5;
            this.tlpArmorMatrix.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tlpArmorMatrix.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tlpArmorMatrix.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tlpArmorMatrix.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tlpArmorMatrix.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tlpArmorMatrix.Controls.Add(this.lblArmorAttack, 1, 2);
            this.tlpArmorMatrix.Controls.Add(this.lblArmorAttackLabel, 1, 1);
            this.tlpArmorMatrix.Controls.Add(this.lblArmorFirewall, 4, 2);
            this.tlpArmorMatrix.Controls.Add(this.lblArmorDeviceRating, 0, 2);
            this.tlpArmorMatrix.Controls.Add(this.lblArmorFirewallLabel, 4, 1);
            this.tlpArmorMatrix.Controls.Add(this.lblArmorDeviceRatingLabel, 0, 1);
            this.tlpArmorMatrix.Controls.Add(this.lblArmorSleaze, 2, 2);
            this.tlpArmorMatrix.Controls.Add(this.lblArmorSleazeLabel, 2, 1);
            this.tlpArmorMatrix.Controls.Add(this.lblArmorDataProcessing, 3, 2);
            this.tlpArmorMatrix.Controls.Add(this.lblArmorDataProcessingLabel, 3, 1);
            this.tlpArmorMatrix.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpArmorMatrix.Location = new System.Drawing.Point(3, 16);
            this.tlpArmorMatrix.Name = "tlpArmorMatrix";
            this.tlpArmorMatrix.RowCount = 3;
            this.tlpArmorMatrix.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tlpArmorMatrix.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpArmorMatrix.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpArmorMatrix.Size = new System.Drawing.Size(420, 75);
            this.tlpArmorMatrix.TabIndex = 0;
            // 
            // lblArmorAttack
            // 
            this.lblArmorAttack.AutoSize = true;
            this.lblArmorAttack.Location = new System.Drawing.Point(87, 56);
            this.lblArmorAttack.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblArmorAttack.Name = "lblArmorAttack";
            this.lblArmorAttack.Size = new System.Drawing.Size(19, 13);
            this.lblArmorAttack.TabIndex = 169;
            this.lblArmorAttack.Text = "[0]";
            // 
            // lblArmorAttackLabel
            // 
            this.lblArmorAttackLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblArmorAttackLabel.AutoSize = true;
            this.lblArmorAttackLabel.Location = new System.Drawing.Point(87, 31);
            this.lblArmorAttackLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblArmorAttackLabel.Name = "lblArmorAttackLabel";
            this.lblArmorAttackLabel.Size = new System.Drawing.Size(41, 13);
            this.lblArmorAttackLabel.TabIndex = 168;
            this.lblArmorAttackLabel.Tag = "Label_Attack";
            this.lblArmorAttackLabel.Text = "Attack:";
            // 
            // lblArmorFirewall
            // 
            this.lblArmorFirewall.AutoSize = true;
            this.lblArmorFirewall.Location = new System.Drawing.Point(339, 56);
            this.lblArmorFirewall.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblArmorFirewall.Name = "lblArmorFirewall";
            this.lblArmorFirewall.Size = new System.Drawing.Size(19, 13);
            this.lblArmorFirewall.TabIndex = 175;
            this.lblArmorFirewall.Text = "[0]";
            // 
            // lblArmorDeviceRating
            // 
            this.lblArmorDeviceRating.AutoSize = true;
            this.lblArmorDeviceRating.Location = new System.Drawing.Point(3, 56);
            this.lblArmorDeviceRating.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblArmorDeviceRating.Name = "lblArmorDeviceRating";
            this.lblArmorDeviceRating.Size = new System.Drawing.Size(19, 13);
            this.lblArmorDeviceRating.TabIndex = 167;
            this.lblArmorDeviceRating.Text = "[0]";
            // 
            // lblArmorFirewallLabel
            // 
            this.lblArmorFirewallLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblArmorFirewallLabel.AutoSize = true;
            this.lblArmorFirewallLabel.Location = new System.Drawing.Point(339, 31);
            this.lblArmorFirewallLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblArmorFirewallLabel.Name = "lblArmorFirewallLabel";
            this.lblArmorFirewallLabel.Size = new System.Drawing.Size(45, 13);
            this.lblArmorFirewallLabel.TabIndex = 174;
            this.lblArmorFirewallLabel.Tag = "Label_Firewall";
            this.lblArmorFirewallLabel.Text = "Firewall:";
            // 
            // lblArmorDeviceRatingLabel
            // 
            this.lblArmorDeviceRatingLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblArmorDeviceRatingLabel.AutoSize = true;
            this.lblArmorDeviceRatingLabel.Location = new System.Drawing.Point(3, 31);
            this.lblArmorDeviceRatingLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblArmorDeviceRatingLabel.Name = "lblArmorDeviceRatingLabel";
            this.lblArmorDeviceRatingLabel.Size = new System.Drawing.Size(78, 13);
            this.lblArmorDeviceRatingLabel.TabIndex = 166;
            this.lblArmorDeviceRatingLabel.Tag = "Label_DeviceRating";
            this.lblArmorDeviceRatingLabel.Text = "Device Rating:";
            // 
            // lblArmorSleaze
            // 
            this.lblArmorSleaze.AutoSize = true;
            this.lblArmorSleaze.Location = new System.Drawing.Point(171, 56);
            this.lblArmorSleaze.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblArmorSleaze.Name = "lblArmorSleaze";
            this.lblArmorSleaze.Size = new System.Drawing.Size(19, 13);
            this.lblArmorSleaze.TabIndex = 171;
            this.lblArmorSleaze.Text = "[0]";
            // 
            // lblArmorSleazeLabel
            // 
            this.lblArmorSleazeLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblArmorSleazeLabel.AutoSize = true;
            this.lblArmorSleazeLabel.Location = new System.Drawing.Point(171, 31);
            this.lblArmorSleazeLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblArmorSleazeLabel.Name = "lblArmorSleazeLabel";
            this.lblArmorSleazeLabel.Size = new System.Drawing.Size(42, 13);
            this.lblArmorSleazeLabel.TabIndex = 170;
            this.lblArmorSleazeLabel.Tag = "Label_Sleaze";
            this.lblArmorSleazeLabel.Text = "Sleaze:";
            // 
            // lblArmorDataProcessing
            // 
            this.lblArmorDataProcessing.AutoSize = true;
            this.lblArmorDataProcessing.Location = new System.Drawing.Point(255, 56);
            this.lblArmorDataProcessing.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblArmorDataProcessing.Name = "lblArmorDataProcessing";
            this.lblArmorDataProcessing.Size = new System.Drawing.Size(19, 13);
            this.lblArmorDataProcessing.TabIndex = 173;
            this.lblArmorDataProcessing.Text = "[0]";
            // 
            // lblArmorDataProcessingLabel
            // 
            this.lblArmorDataProcessingLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblArmorDataProcessingLabel.AutoSize = true;
            this.lblArmorDataProcessingLabel.Location = new System.Drawing.Point(255, 31);
            this.lblArmorDataProcessingLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblArmorDataProcessingLabel.Name = "lblArmorDataProcessingLabel";
            this.lblArmorDataProcessingLabel.Size = new System.Drawing.Size(61, 13);
            this.lblArmorDataProcessingLabel.TabIndex = 172;
            this.lblArmorDataProcessingLabel.Tag = "Label_DataProcessing";
            this.lblArmorDataProcessingLabel.Text = "Data Proc.:";
            // 
            // gpbArmorLocation
            // 
            this.gpbArmorLocation.AutoSize = true;
            this.gpbArmorLocation.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.gpbArmorLocation.Controls.Add(this.tlpArmorLocation);
            this.gpbArmorLocation.Dock = System.Windows.Forms.DockStyle.Top;
            this.gpbArmorLocation.Location = new System.Drawing.Point(3, 273);
            this.gpbArmorLocation.Name = "gpbArmorLocation";
            this.gpbArmorLocation.Size = new System.Drawing.Size(426, 73);
            this.gpbArmorLocation.TabIndex = 2;
            this.gpbArmorLocation.TabStop = false;
            this.gpbArmorLocation.Tag = "String_Armor";
            this.gpbArmorLocation.Text = "Armor";
            // 
            // tlpArmorLocation
            // 
            this.tlpArmorLocation.AutoSize = true;
            this.tlpArmorLocation.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpArmorLocation.ColumnCount = 2;
            this.tlpArmorLocation.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpArmorLocation.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpArmorLocation.Controls.Add(this.lblArmorEquippedLabel, 0, 0);
            this.tlpArmorLocation.Controls.Add(this.lblArmorEquipped, 0, 1);
            this.tlpArmorLocation.Controls.Add(this.flpArmorLocationButtons, 1, 0);
            this.tlpArmorLocation.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpArmorLocation.Location = new System.Drawing.Point(3, 16);
            this.tlpArmorLocation.Name = "tlpArmorLocation";
            this.tlpArmorLocation.RowCount = 2;
            this.tlpArmorLocation.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpArmorLocation.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpArmorLocation.Size = new System.Drawing.Size(420, 54);
            this.tlpArmorLocation.TabIndex = 0;
            // 
            // lblArmorEquippedLabel
            // 
            this.lblArmorEquippedLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblArmorEquippedLabel.AutoSize = true;
            this.lblArmorEquippedLabel.Location = new System.Drawing.Point(3, 10);
            this.lblArmorEquippedLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblArmorEquippedLabel.Name = "lblArmorEquippedLabel";
            this.lblArmorEquippedLabel.Size = new System.Drawing.Size(52, 13);
            this.lblArmorEquippedLabel.TabIndex = 110;
            this.lblArmorEquippedLabel.Tag = "Checkbox_Equipped";
            this.lblArmorEquippedLabel.Text = "Equipped";
            this.lblArmorEquippedLabel.Visible = false;
            // 
            // lblArmorEquipped
            // 
            this.lblArmorEquipped.AutoSize = true;
            this.tlpArmorLocation.SetColumnSpan(this.lblArmorEquipped, 2);
            this.lblArmorEquipped.Location = new System.Drawing.Point(3, 35);
            this.lblArmorEquipped.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblArmorEquipped.Name = "lblArmorEquipped";
            this.lblArmorEquipped.Size = new System.Drawing.Size(152, 13);
            this.lblArmorEquipped.TabIndex = 111;
            this.lblArmorEquipped.Text = "[Armor Bundle Equipped Items]";
            this.lblArmorEquipped.Visible = false;
            // 
            // flpArmorLocationButtons
            // 
            this.flpArmorLocationButtons.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.flpArmorLocationButtons.AutoSize = true;
            this.flpArmorLocationButtons.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flpArmorLocationButtons.Controls.Add(this.cmdArmorEquipAll);
            this.flpArmorLocationButtons.Controls.Add(this.cmdArmorUnEquipAll);
            this.flpArmorLocationButtons.Location = new System.Drawing.Point(275, 0);
            this.flpArmorLocationButtons.Margin = new System.Windows.Forms.Padding(0);
            this.flpArmorLocationButtons.Name = "flpArmorLocationButtons";
            this.flpArmorLocationButtons.Size = new System.Drawing.Size(145, 29);
            this.flpArmorLocationButtons.TabIndex = 112;
            this.flpArmorLocationButtons.WrapContents = false;
            // 
            // cmdArmorEquipAll
            // 
            this.cmdArmorEquipAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdArmorEquipAll.AutoSize = true;
            this.cmdArmorEquipAll.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdArmorEquipAll.Location = new System.Drawing.Point(3, 3);
            this.cmdArmorEquipAll.Name = "cmdArmorEquipAll";
            this.cmdArmorEquipAll.Size = new System.Drawing.Size(58, 23);
            this.cmdArmorEquipAll.TabIndex = 108;
            this.cmdArmorEquipAll.Tag = "Button_EquipAll";
            this.cmdArmorEquipAll.Text = "Equip All";
            this.cmdArmorEquipAll.UseVisualStyleBackColor = true;
            this.cmdArmorEquipAll.Visible = false;
            this.cmdArmorEquipAll.Click += new System.EventHandler(this.cmdArmorEquipAll_Click);
            // 
            // cmdArmorUnEquipAll
            // 
            this.cmdArmorUnEquipAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdArmorUnEquipAll.AutoSize = true;
            this.cmdArmorUnEquipAll.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdArmorUnEquipAll.Location = new System.Drawing.Point(67, 3);
            this.cmdArmorUnEquipAll.Name = "cmdArmorUnEquipAll";
            this.cmdArmorUnEquipAll.Size = new System.Drawing.Size(75, 23);
            this.cmdArmorUnEquipAll.TabIndex = 109;
            this.cmdArmorUnEquipAll.Tag = "Button_UnEquipAll";
            this.cmdArmorUnEquipAll.Text = "Un-Equip All";
            this.cmdArmorUnEquipAll.UseVisualStyleBackColor = true;
            this.cmdArmorUnEquipAll.Visible = false;
            this.cmdArmorUnEquipAll.Click += new System.EventHandler(this.cmdArmorUnEquipAll_Click);
            // 
            // tlpArmorButtons
            // 
            this.tlpArmorButtons.AutoSize = true;
            this.tlpArmorButtons.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpArmorButtons.ColumnCount = 3;
            this.tlpArmor.SetColumnSpan(this.tlpArmorButtons, 2);
            this.tlpArmorButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tlpArmorButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tlpArmorButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tlpArmorButtons.Controls.Add(this.cmdAddArmor, 0, 0);
            this.tlpArmorButtons.Controls.Add(this.cmdAddArmorBundle, 2, 0);
            this.tlpArmorButtons.Controls.Add(this.cmdDeleteArmor, 1, 0);
            this.tlpArmorButtons.Location = new System.Drawing.Point(0, 0);
            this.tlpArmorButtons.Margin = new System.Windows.Forms.Padding(0);
            this.tlpArmorButtons.Name = "tlpArmorButtons";
            this.tlpArmorButtons.RowCount = 1;
            this.tlpArmorButtons.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpArmorButtons.Size = new System.Drawing.Size(178, 29);
            this.tlpArmorButtons.TabIndex = 178;
            // 
            // cmdAddArmor
            // 
            this.cmdAddArmor.AutoSize = true;
            this.cmdAddArmor.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdAddArmor.ContextMenuStrip = this.cmsArmor;
            this.cmdAddArmor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmdAddArmor.Location = new System.Drawing.Point(3, 3);
            this.cmdAddArmor.Name = "cmdAddArmor";
            this.cmdAddArmor.Size = new System.Drawing.Size(53, 23);
            this.cmdAddArmor.SplitMenuStrip = this.cmsArmor;
            this.cmdAddArmor.TabIndex = 131;
            this.cmdAddArmor.Tag = "Button_AddArmor";
            this.cmdAddArmor.Text = "&Add Armor";
            this.cmdAddArmor.UseVisualStyleBackColor = true;
            this.cmdAddArmor.Click += new System.EventHandler(this.cmdAddArmor_Click);
            // 
            // cmdAddArmorBundle
            // 
            this.cmdAddArmorBundle.AutoSize = true;
            this.cmdAddArmorBundle.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdAddArmorBundle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmdAddArmorBundle.Location = new System.Drawing.Point(121, 3);
            this.cmdAddArmorBundle.Name = "cmdAddArmorBundle";
            this.cmdAddArmorBundle.Size = new System.Drawing.Size(54, 23);
            this.cmdAddArmorBundle.TabIndex = 104;
            this.cmdAddArmorBundle.Tag = "Button_AddBundle";
            this.cmdAddArmorBundle.Text = "Add Armor Bundle";
            this.cmdAddArmorBundle.UseVisualStyleBackColor = true;
            this.cmdAddArmorBundle.Click += new System.EventHandler(this.cmdAddArmorBundle_Click);
            // 
            // cmdDeleteArmor
            // 
            this.cmdDeleteArmor.AutoSize = true;
            this.cmdDeleteArmor.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdDeleteArmor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmdDeleteArmor.Location = new System.Drawing.Point(62, 3);
            this.cmdDeleteArmor.Name = "cmdDeleteArmor";
            this.cmdDeleteArmor.Size = new System.Drawing.Size(53, 23);
            this.cmdDeleteArmor.TabIndex = 68;
            this.cmdDeleteArmor.Tag = "String_Delete";
            this.cmdDeleteArmor.Text = "Delete";
            this.cmdDeleteArmor.UseVisualStyleBackColor = true;
            this.cmdDeleteArmor.Click += new System.EventHandler(this.cmdDeleteArmor_Click);
            // 
            // tabWeapons
            // 
            this.tabWeapons.BackColor = System.Drawing.SystemColors.Control;
            this.tabWeapons.Controls.Add(this.tlpWeapons);
            this.tabWeapons.Location = new System.Drawing.Point(4, 22);
            this.tabWeapons.Name = "tabWeapons";
            this.tabWeapons.Padding = new System.Windows.Forms.Padding(3);
            this.tabWeapons.Size = new System.Drawing.Size(184, 48);
            this.tabWeapons.TabIndex = 2;
            this.tabWeapons.Tag = "Tab_Weapons";
            this.tabWeapons.Text = "Weapons";
            // 
            // tlpWeapons
            // 
            this.tlpWeapons.AutoSize = true;
            this.tlpWeapons.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpWeapons.ColumnCount = 2;
            this.tlpWeapons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpWeapons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpWeapons.Controls.Add(this.treWeapons, 0, 1);
            this.tlpWeapons.Controls.Add(this.flpWeapons, 1, 1);
            this.tlpWeapons.Controls.Add(this.tlpWeaponsButtons, 0, 0);
            this.tlpWeapons.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpWeapons.Location = new System.Drawing.Point(3, 3);
            this.tlpWeapons.Name = "tlpWeapons";
            this.tlpWeapons.RowCount = 1;
            this.tlpWeapons.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpWeapons.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpWeapons.Size = new System.Drawing.Size(178, 42);
            this.tlpWeapons.TabIndex = 225;
            // 
            // treWeapons
            // 
            this.treWeapons.AllowDrop = true;
            this.treWeapons.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treWeapons.HideSelection = false;
            this.treWeapons.Location = new System.Drawing.Point(3, 32);
            this.treWeapons.Name = "treWeapons";
            treeNode21.Name = "nodWeaponsRoot";
            treeNode21.Tag = "Node_SelectedWeapons";
            treeNode21.Text = "Selected Weapons";
            this.treWeapons.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            treeNode21});
            this.treWeapons.ShowNodeToolTips = true;
            this.treWeapons.Size = new System.Drawing.Size(295, 7);
            this.treWeapons.TabIndex = 29;
            this.treWeapons.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treWeapons_AfterSelect);
            this.treWeapons.DragOver += new System.Windows.Forms.DragEventHandler(this.treWeapons_DragOver);
            this.treWeapons.KeyDown += new System.Windows.Forms.KeyEventHandler(this.treWeapons_KeyDown);
            this.treWeapons.MouseDown += new System.Windows.Forms.MouseEventHandler(this.TreeView_MouseDown);
            // 
            // flpWeapons
            // 
            this.flpWeapons.AutoScroll = true;
            this.flpWeapons.Controls.Add(this.gpbWeaponsCommon);
            this.flpWeapons.Controls.Add(this.gpbWeaponsWeapon);
            this.flpWeapons.Controls.Add(this.gpbWeaponsMatrix);
            this.flpWeapons.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flpWeapons.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flpWeapons.Location = new System.Drawing.Point(301, 29);
            this.flpWeapons.Margin = new System.Windows.Forms.Padding(0);
            this.flpWeapons.Name = "flpWeapons";
            this.flpWeapons.Size = new System.Drawing.Size(1, 13);
            this.flpWeapons.TabIndex = 227;
            this.flpWeapons.WrapContents = false;
            // 
            // gpbWeaponsCommon
            // 
            this.gpbWeaponsCommon.AutoSize = true;
            this.gpbWeaponsCommon.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.gpbWeaponsCommon.Controls.Add(this.tlpWeaponsCommon);
            this.gpbWeaponsCommon.Dock = System.Windows.Forms.DockStyle.Top;
            this.gpbWeaponsCommon.Location = new System.Drawing.Point(3, 3);
            this.gpbWeaponsCommon.Name = "gpbWeaponsCommon";
            this.gpbWeaponsCommon.Size = new System.Drawing.Size(500, 188);
            this.gpbWeaponsCommon.TabIndex = 0;
            this.gpbWeaponsCommon.TabStop = false;
            this.gpbWeaponsCommon.Tag = "String_Info";
            this.gpbWeaponsCommon.Text = "Info";
            // 
            // tlpWeaponsCommon
            // 
            this.tlpWeaponsCommon.AutoSize = true;
            this.tlpWeaponsCommon.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpWeaponsCommon.ColumnCount = 4;
            this.tlpWeaponsCommon.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpWeaponsCommon.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpWeaponsCommon.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpWeaponsCommon.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpWeaponsCommon.Controls.Add(this.lblWeaponCapacity, 1, 3);
            this.tlpWeaponsCommon.Controls.Add(this.lblWeaponNameLabel, 0, 0);
            this.tlpWeaponsCommon.Controls.Add(this.lblWeaponName, 1, 0);
            this.tlpWeaponsCommon.Controls.Add(this.lblWeaponCapacityLabel, 0, 3);
            this.tlpWeaponsCommon.Controls.Add(this.lblWeaponConceal, 3, 3);
            this.tlpWeaponsCommon.Controls.Add(this.lblWeaponCategoryLabel, 0, 1);
            this.tlpWeaponsCommon.Controls.Add(this.lblWeaponConcealLabel, 2, 3);
            this.tlpWeaponsCommon.Controls.Add(this.lblWeaponSourceLabel, 0, 4);
            this.tlpWeaponsCommon.Controls.Add(this.lblWeaponSource, 1, 4);
            this.tlpWeaponsCommon.Controls.Add(this.lblWeaponCategory, 1, 1);
            this.tlpWeaponsCommon.Controls.Add(this.lblWeaponAvailLabel, 2, 0);
            this.tlpWeaponsCommon.Controls.Add(this.lblWeaponAvail, 3, 0);
            this.tlpWeaponsCommon.Controls.Add(this.lblWeaponSlots, 3, 2);
            this.tlpWeaponsCommon.Controls.Add(this.lblWeaponSlotsLabel, 2, 2);
            this.tlpWeaponsCommon.Controls.Add(this.lblWeaponRating, 1, 2);
            this.tlpWeaponsCommon.Controls.Add(this.lblWeaponCostLabel, 2, 1);
            this.tlpWeaponsCommon.Controls.Add(this.lblWeaponRatingLabel, 0, 2);
            this.tlpWeaponsCommon.Controls.Add(this.lblWeaponCost, 3, 1);
            this.tlpWeaponsCommon.Controls.Add(this.flpWeaponsCommonCheckBoxes, 2, 4);
            this.tlpWeaponsCommon.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpWeaponsCommon.Location = new System.Drawing.Point(3, 16);
            this.tlpWeaponsCommon.Name = "tlpWeaponsCommon";
            this.tlpWeaponsCommon.RowCount = 5;
            this.tlpWeaponsCommon.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpWeaponsCommon.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpWeaponsCommon.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpWeaponsCommon.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpWeaponsCommon.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpWeaponsCommon.Size = new System.Drawing.Size(494, 169);
            this.tlpWeaponsCommon.TabIndex = 0;
            // 
            // lblWeaponCapacity
            // 
            this.lblWeaponCapacity.AutoSize = true;
            this.lblWeaponCapacity.Location = new System.Drawing.Point(61, 81);
            this.lblWeaponCapacity.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblWeaponCapacity.Name = "lblWeaponCapacity";
            this.lblWeaponCapacity.Size = new System.Drawing.Size(54, 13);
            this.lblWeaponCapacity.TabIndex = 224;
            this.lblWeaponCapacity.Text = "[Capacity]";
            // 
            // lblWeaponNameLabel
            // 
            this.lblWeaponNameLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblWeaponNameLabel.AutoSize = true;
            this.lblWeaponNameLabel.Location = new System.Drawing.Point(17, 6);
            this.lblWeaponNameLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblWeaponNameLabel.Name = "lblWeaponNameLabel";
            this.lblWeaponNameLabel.Size = new System.Drawing.Size(38, 13);
            this.lblWeaponNameLabel.TabIndex = 47;
            this.lblWeaponNameLabel.Tag = "Label_Name";
            this.lblWeaponNameLabel.Text = "Name:";
            // 
            // lblWeaponName
            // 
            this.lblWeaponName.AutoSize = true;
            this.lblWeaponName.Location = new System.Drawing.Point(61, 6);
            this.lblWeaponName.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblWeaponName.Name = "lblWeaponName";
            this.lblWeaponName.Size = new System.Drawing.Size(41, 13);
            this.lblWeaponName.TabIndex = 48;
            this.lblWeaponName.Text = "[Name]";
            // 
            // lblWeaponCapacityLabel
            // 
            this.lblWeaponCapacityLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblWeaponCapacityLabel.AutoSize = true;
            this.lblWeaponCapacityLabel.Location = new System.Drawing.Point(4, 81);
            this.lblWeaponCapacityLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblWeaponCapacityLabel.Name = "lblWeaponCapacityLabel";
            this.lblWeaponCapacityLabel.Size = new System.Drawing.Size(51, 13);
            this.lblWeaponCapacityLabel.TabIndex = 223;
            this.lblWeaponCapacityLabel.Tag = "Label_Capacity";
            this.lblWeaponCapacityLabel.Text = "Capacity:";
            // 
            // lblWeaponConceal
            // 
            this.lblWeaponConceal.AutoSize = true;
            this.lblWeaponConceal.Location = new System.Drawing.Point(310, 81);
            this.lblWeaponConceal.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblWeaponConceal.Name = "lblWeaponConceal";
            this.lblWeaponConceal.Size = new System.Drawing.Size(52, 13);
            this.lblWeaponConceal.TabIndex = 100;
            this.lblWeaponConceal.Text = "[Conceal]";
            // 
            // lblWeaponCategoryLabel
            // 
            this.lblWeaponCategoryLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblWeaponCategoryLabel.AutoSize = true;
            this.lblWeaponCategoryLabel.Location = new System.Drawing.Point(3, 31);
            this.lblWeaponCategoryLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblWeaponCategoryLabel.Name = "lblWeaponCategoryLabel";
            this.lblWeaponCategoryLabel.Size = new System.Drawing.Size(52, 13);
            this.lblWeaponCategoryLabel.TabIndex = 49;
            this.lblWeaponCategoryLabel.Tag = "Label_Category";
            this.lblWeaponCategoryLabel.Text = "Category:";
            // 
            // lblWeaponConcealLabel
            // 
            this.lblWeaponConcealLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblWeaponConcealLabel.AutoSize = true;
            this.lblWeaponConcealLabel.Location = new System.Drawing.Point(255, 81);
            this.lblWeaponConcealLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblWeaponConcealLabel.Name = "lblWeaponConcealLabel";
            this.lblWeaponConcealLabel.Size = new System.Drawing.Size(49, 13);
            this.lblWeaponConcealLabel.TabIndex = 99;
            this.lblWeaponConcealLabel.Tag = "Label_Conceal";
            this.lblWeaponConcealLabel.Text = "Conceal:";
            // 
            // lblWeaponSourceLabel
            // 
            this.lblWeaponSourceLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblWeaponSourceLabel.AutoSize = true;
            this.lblWeaponSourceLabel.Location = new System.Drawing.Point(11, 106);
            this.lblWeaponSourceLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblWeaponSourceLabel.Name = "lblWeaponSourceLabel";
            this.lblWeaponSourceLabel.Size = new System.Drawing.Size(44, 13);
            this.lblWeaponSourceLabel.TabIndex = 68;
            this.lblWeaponSourceLabel.Tag = "Label_Source";
            this.lblWeaponSourceLabel.Text = "Source:";
            // 
            // lblWeaponSource
            // 
            this.lblWeaponSource.AutoSize = true;
            this.lblWeaponSource.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblWeaponSource.Location = new System.Drawing.Point(61, 106);
            this.lblWeaponSource.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblWeaponSource.Name = "lblWeaponSource";
            this.lblWeaponSource.Size = new System.Drawing.Size(47, 13);
            this.lblWeaponSource.TabIndex = 69;
            this.lblWeaponSource.Text = "[Source]";
            this.lblWeaponSource.Click += new System.EventHandler(this.OpenSourceFromLabel);
            // 
            // lblWeaponCategory
            // 
            this.lblWeaponCategory.AutoSize = true;
            this.lblWeaponCategory.Location = new System.Drawing.Point(61, 31);
            this.lblWeaponCategory.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblWeaponCategory.Name = "lblWeaponCategory";
            this.lblWeaponCategory.Size = new System.Drawing.Size(55, 13);
            this.lblWeaponCategory.TabIndex = 50;
            this.lblWeaponCategory.Text = "[Category]";
            // 
            // lblWeaponAvailLabel
            // 
            this.lblWeaponAvailLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblWeaponAvailLabel.AutoSize = true;
            this.lblWeaponAvailLabel.Location = new System.Drawing.Point(271, 6);
            this.lblWeaponAvailLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblWeaponAvailLabel.Name = "lblWeaponAvailLabel";
            this.lblWeaponAvailLabel.Size = new System.Drawing.Size(33, 13);
            this.lblWeaponAvailLabel.TabIndex = 56;
            this.lblWeaponAvailLabel.Tag = "Label_Avail";
            this.lblWeaponAvailLabel.Text = "Avail:";
            // 
            // lblWeaponAvail
            // 
            this.lblWeaponAvail.AutoSize = true;
            this.lblWeaponAvail.Location = new System.Drawing.Point(310, 6);
            this.lblWeaponAvail.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblWeaponAvail.Name = "lblWeaponAvail";
            this.lblWeaponAvail.Size = new System.Drawing.Size(36, 13);
            this.lblWeaponAvail.TabIndex = 57;
            this.lblWeaponAvail.Text = "[Avail]";
            // 
            // lblWeaponSlots
            // 
            this.lblWeaponSlots.AutoSize = true;
            this.lblWeaponSlots.Location = new System.Drawing.Point(310, 56);
            this.lblWeaponSlots.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblWeaponSlots.Name = "lblWeaponSlots";
            this.lblWeaponSlots.Size = new System.Drawing.Size(36, 13);
            this.lblWeaponSlots.TabIndex = 71;
            this.lblWeaponSlots.Text = "[Slots]";
            // 
            // lblWeaponSlotsLabel
            // 
            this.lblWeaponSlotsLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblWeaponSlotsLabel.AutoSize = true;
            this.lblWeaponSlotsLabel.Location = new System.Drawing.Point(247, 56);
            this.lblWeaponSlotsLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblWeaponSlotsLabel.Name = "lblWeaponSlotsLabel";
            this.lblWeaponSlotsLabel.Size = new System.Drawing.Size(57, 13);
            this.lblWeaponSlotsLabel.TabIndex = 70;
            this.lblWeaponSlotsLabel.Tag = "Label_ModSlots";
            this.lblWeaponSlotsLabel.Text = "Mod Slots:";
            // 
            // lblWeaponRating
            // 
            this.lblWeaponRating.AutoSize = true;
            this.lblWeaponRating.Location = new System.Drawing.Point(61, 56);
            this.lblWeaponRating.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblWeaponRating.Name = "lblWeaponRating";
            this.lblWeaponRating.Size = new System.Drawing.Size(44, 13);
            this.lblWeaponRating.TabIndex = 167;
            this.lblWeaponRating.Text = "[Rating]";
            // 
            // lblWeaponCostLabel
            // 
            this.lblWeaponCostLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblWeaponCostLabel.AutoSize = true;
            this.lblWeaponCostLabel.Location = new System.Drawing.Point(273, 31);
            this.lblWeaponCostLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblWeaponCostLabel.Name = "lblWeaponCostLabel";
            this.lblWeaponCostLabel.Size = new System.Drawing.Size(31, 13);
            this.lblWeaponCostLabel.TabIndex = 58;
            this.lblWeaponCostLabel.Tag = "Label_Cost";
            this.lblWeaponCostLabel.Text = "Cost:";
            // 
            // lblWeaponRatingLabel
            // 
            this.lblWeaponRatingLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblWeaponRatingLabel.AutoSize = true;
            this.lblWeaponRatingLabel.Location = new System.Drawing.Point(14, 56);
            this.lblWeaponRatingLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblWeaponRatingLabel.Name = "lblWeaponRatingLabel";
            this.lblWeaponRatingLabel.Size = new System.Drawing.Size(41, 13);
            this.lblWeaponRatingLabel.TabIndex = 166;
            this.lblWeaponRatingLabel.Tag = "Label_Rating";
            this.lblWeaponRatingLabel.Text = "Rating:";
            // 
            // lblWeaponCost
            // 
            this.lblWeaponCost.AutoSize = true;
            this.lblWeaponCost.Location = new System.Drawing.Point(310, 31);
            this.lblWeaponCost.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblWeaponCost.Name = "lblWeaponCost";
            this.lblWeaponCost.Size = new System.Drawing.Size(34, 13);
            this.lblWeaponCost.TabIndex = 59;
            this.lblWeaponCost.Text = "[Cost]";
            // 
            // flpWeaponsCommonCheckBoxes
            // 
            this.flpWeaponsCommonCheckBoxes.AutoSize = true;
            this.flpWeaponsCommonCheckBoxes.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpWeaponsCommon.SetColumnSpan(this.flpWeaponsCommonCheckBoxes, 2);
            this.flpWeaponsCommonCheckBoxes.Controls.Add(this.chkWeaponAccessoryInstalled);
            this.flpWeaponsCommonCheckBoxes.Controls.Add(this.chkIncludedInWeapon);
            this.flpWeaponsCommonCheckBoxes.Controls.Add(this.chkWeaponStolen);
            this.flpWeaponsCommonCheckBoxes.Dock = System.Windows.Forms.DockStyle.Top;
            this.flpWeaponsCommonCheckBoxes.Location = new System.Drawing.Point(244, 100);
            this.flpWeaponsCommonCheckBoxes.Margin = new System.Windows.Forms.Padding(0);
            this.flpWeaponsCommonCheckBoxes.Name = "flpWeaponsCommonCheckBoxes";
            this.flpWeaponsCommonCheckBoxes.Size = new System.Drawing.Size(250, 46);
            this.flpWeaponsCommonCheckBoxes.TabIndex = 225;
            // 
            // chkIncludedInWeapon
            // 
            this.chkIncludedInWeapon.AutoSize = true;
            this.chkIncludedInWeapon.Enabled = false;
            this.chkIncludedInWeapon.Location = new System.Drawing.Point(74, 3);
            this.chkIncludedInWeapon.Name = "chkIncludedInWeapon";
            this.chkIncludedInWeapon.Size = new System.Drawing.Size(127, 17);
            this.chkIncludedInWeapon.TabIndex = 73;
            this.chkIncludedInWeapon.Tag = "Checkbox_BaseWeapon";
            this.chkIncludedInWeapon.Text = "Part of base Weapon";
            this.chkIncludedInWeapon.UseVisualStyleBackColor = true;
            this.chkIncludedInWeapon.CheckedChanged += new System.EventHandler(this.chkIncludedInWeapon_CheckedChanged);
            // 
            // chkWeaponStolen
            // 
            this.chkWeaponStolen.AutoSize = true;
            this.chkWeaponStolen.Location = new System.Drawing.Point(3, 26);
            this.chkWeaponStolen.Name = "chkWeaponStolen";
            this.chkWeaponStolen.Size = new System.Drawing.Size(56, 17);
            this.chkWeaponStolen.TabIndex = 229;
            this.chkWeaponStolen.Tag = "Checkbox_Stolen";
            this.chkWeaponStolen.Text = "Stolen";
            this.chkWeaponStolen.UseVisualStyleBackColor = true;
            this.chkWeaponStolen.Visible = false;
            this.chkWeaponStolen.CheckedChanged += new System.EventHandler(this.chkWeaponStolen_CheckedChanged);
            // 
            // gpbWeaponsWeapon
            // 
            this.gpbWeaponsWeapon.AutoSize = true;
            this.gpbWeaponsWeapon.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.gpbWeaponsWeapon.Controls.Add(this.flpWeaponsWeapon);
            this.gpbWeaponsWeapon.Dock = System.Windows.Forms.DockStyle.Top;
            this.gpbWeaponsWeapon.Location = new System.Drawing.Point(3, 197);
            this.gpbWeaponsWeapon.Name = "gpbWeaponsWeapon";
            this.gpbWeaponsWeapon.Size = new System.Drawing.Size(500, 150);
            this.gpbWeaponsWeapon.TabIndex = 2;
            this.gpbWeaponsWeapon.TabStop = false;
            this.gpbWeaponsWeapon.Tag = "String_Weapon";
            this.gpbWeaponsWeapon.Text = "Weapon";
            // 
            // flpWeaponsWeapon
            // 
            this.flpWeaponsWeapon.AutoSize = true;
            this.flpWeaponsWeapon.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flpWeaponsWeapon.Controls.Add(this.tlpWeaponsWeapon);
            this.flpWeaponsWeapon.Controls.Add(this.tlpWeaponsRanges);
            this.flpWeaponsWeapon.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flpWeaponsWeapon.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flpWeaponsWeapon.Location = new System.Drawing.Point(3, 16);
            this.flpWeaponsWeapon.Margin = new System.Windows.Forms.Padding(0);
            this.flpWeaponsWeapon.Name = "flpWeaponsWeapon";
            this.flpWeaponsWeapon.Size = new System.Drawing.Size(494, 131);
            this.flpWeaponsWeapon.TabIndex = 0;
            this.flpWeaponsWeapon.WrapContents = false;
            // 
            // tlpWeaponsWeapon
            // 
            this.tlpWeaponsWeapon.AutoSize = true;
            this.tlpWeaponsWeapon.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpWeaponsWeapon.ColumnCount = 8;
            this.tlpWeaponsWeapon.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpWeaponsWeapon.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tlpWeaponsWeapon.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpWeaponsWeapon.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tlpWeaponsWeapon.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpWeaponsWeapon.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tlpWeaponsWeapon.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpWeaponsWeapon.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tlpWeaponsWeapon.Controls.Add(this.lblWeaponDamageLabel, 0, 0);
            this.tlpWeaponsWeapon.Controls.Add(this.lblWeaponDamage, 1, 0);
            this.tlpWeaponsWeapon.Controls.Add(this.lblWeaponAmmo, 5, 1);
            this.tlpWeaponsWeapon.Controls.Add(this.lblWeaponAmmoLabel, 4, 1);
            this.tlpWeaponsWeapon.Controls.Add(this.lblWeaponReach, 7, 0);
            this.tlpWeaponsWeapon.Controls.Add(this.lblWeaponReachLabel, 6, 0);
            this.tlpWeaponsWeapon.Controls.Add(this.lblWeaponRC, 7, 1);
            this.tlpWeaponsWeapon.Controls.Add(this.lblWeaponRCLabel, 6, 1);
            this.tlpWeaponsWeapon.Controls.Add(this.lblWeaponDicePool, 1, 1);
            this.tlpWeaponsWeapon.Controls.Add(this.lblWeaponAccuracy, 5, 0);
            this.tlpWeaponsWeapon.Controls.Add(this.lblWeaponMode, 3, 1);
            this.tlpWeaponsWeapon.Controls.Add(this.lblWeaponModeLabel, 2, 1);
            this.tlpWeaponsWeapon.Controls.Add(this.lblWeaponDicePoolLabel, 0, 1);
            this.tlpWeaponsWeapon.Controls.Add(this.lblWeaponAccuracyLabel, 4, 0);
            this.tlpWeaponsWeapon.Controls.Add(this.lblWeaponAPLabel, 2, 0);
            this.tlpWeaponsWeapon.Controls.Add(this.lblWeaponAP, 3, 0);
            this.tlpWeaponsWeapon.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpWeaponsWeapon.Location = new System.Drawing.Point(0, 0);
            this.tlpWeaponsWeapon.Margin = new System.Windows.Forms.Padding(0, 0, 0, 3);
            this.tlpWeaponsWeapon.Name = "tlpWeaponsWeapon";
            this.tlpWeaponsWeapon.RowCount = 2;
            this.tlpWeaponsWeapon.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpWeaponsWeapon.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpWeaponsWeapon.Size = new System.Drawing.Size(494, 50);
            this.tlpWeaponsWeapon.TabIndex = 0;
            // 
            // lblWeaponDamageLabel
            // 
            this.lblWeaponDamageLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblWeaponDamageLabel.AutoSize = true;
            this.lblWeaponDamageLabel.Location = new System.Drawing.Point(9, 6);
            this.lblWeaponDamageLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblWeaponDamageLabel.Name = "lblWeaponDamageLabel";
            this.lblWeaponDamageLabel.Size = new System.Drawing.Size(50, 13);
            this.lblWeaponDamageLabel.TabIndex = 51;
            this.lblWeaponDamageLabel.Tag = "Label_Damage";
            this.lblWeaponDamageLabel.Text = "Damage:";
            // 
            // lblWeaponDamage
            // 
            this.lblWeaponDamage.AutoSize = true;
            this.lblWeaponDamage.Location = new System.Drawing.Point(65, 6);
            this.lblWeaponDamage.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblWeaponDamage.Name = "lblWeaponDamage";
            this.lblWeaponDamage.Size = new System.Drawing.Size(35, 13);
            this.lblWeaponDamage.TabIndex = 52;
            this.lblWeaponDamage.Text = "[Dmg]";
            // 
            // lblWeaponAmmo
            // 
            this.lblWeaponAmmo.AutoSize = true;
            this.lblWeaponAmmo.Location = new System.Drawing.Point(309, 31);
            this.lblWeaponAmmo.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblWeaponAmmo.Name = "lblWeaponAmmo";
            this.lblWeaponAmmo.Size = new System.Drawing.Size(42, 13);
            this.lblWeaponAmmo.TabIndex = 67;
            this.lblWeaponAmmo.Text = "[Ammo]";
            // 
            // lblWeaponAmmoLabel
            // 
            this.lblWeaponAmmoLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblWeaponAmmoLabel.AutoSize = true;
            this.lblWeaponAmmoLabel.Location = new System.Drawing.Point(264, 31);
            this.lblWeaponAmmoLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblWeaponAmmoLabel.Name = "lblWeaponAmmoLabel";
            this.lblWeaponAmmoLabel.Size = new System.Drawing.Size(39, 13);
            this.lblWeaponAmmoLabel.TabIndex = 66;
            this.lblWeaponAmmoLabel.Tag = "Label_Ammo";
            this.lblWeaponAmmoLabel.Text = "Ammo:";
            // 
            // lblWeaponReach
            // 
            this.lblWeaponReach.AutoSize = true;
            this.lblWeaponReach.Location = new System.Drawing.Point(427, 6);
            this.lblWeaponReach.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblWeaponReach.Name = "lblWeaponReach";
            this.lblWeaponReach.Size = new System.Drawing.Size(45, 13);
            this.lblWeaponReach.TabIndex = 63;
            this.lblWeaponReach.Text = "[Reach]";
            // 
            // lblWeaponReachLabel
            // 
            this.lblWeaponReachLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblWeaponReachLabel.AutoSize = true;
            this.lblWeaponReachLabel.Location = new System.Drawing.Point(379, 6);
            this.lblWeaponReachLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblWeaponReachLabel.Name = "lblWeaponReachLabel";
            this.lblWeaponReachLabel.Size = new System.Drawing.Size(42, 13);
            this.lblWeaponReachLabel.TabIndex = 62;
            this.lblWeaponReachLabel.Tag = "Label_Reach";
            this.lblWeaponReachLabel.Text = "Reach:";
            // 
            // lblWeaponRC
            // 
            this.lblWeaponRC.AutoSize = true;
            this.lblWeaponRC.Location = new System.Drawing.Point(427, 31);
            this.lblWeaponRC.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblWeaponRC.Name = "lblWeaponRC";
            this.lblWeaponRC.Size = new System.Drawing.Size(28, 13);
            this.lblWeaponRC.TabIndex = 54;
            this.lblWeaponRC.Text = "[RC]";
            // 
            // lblWeaponRCLabel
            // 
            this.lblWeaponRCLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblWeaponRCLabel.AutoSize = true;
            this.lblWeaponRCLabel.Location = new System.Drawing.Point(396, 31);
            this.lblWeaponRCLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblWeaponRCLabel.Name = "lblWeaponRCLabel";
            this.lblWeaponRCLabel.Size = new System.Drawing.Size(25, 13);
            this.lblWeaponRCLabel.TabIndex = 53;
            this.lblWeaponRCLabel.Tag = "Label_RC";
            this.lblWeaponRCLabel.Text = "RC:";
            // 
            // lblWeaponDicePool
            // 
            this.lblWeaponDicePool.AutoSize = true;
            this.lblWeaponDicePool.Location = new System.Drawing.Point(65, 31);
            this.lblWeaponDicePool.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblWeaponDicePool.Name = "lblWeaponDicePool";
            this.lblWeaponDicePool.Size = new System.Drawing.Size(34, 13);
            this.lblWeaponDicePool.TabIndex = 107;
            this.lblWeaponDicePool.Text = "[Pool]";
            // 
            // lblWeaponAccuracy
            // 
            this.lblWeaponAccuracy.AutoSize = true;
            this.lblWeaponAccuracy.Location = new System.Drawing.Point(309, 6);
            this.lblWeaponAccuracy.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblWeaponAccuracy.Name = "lblWeaponAccuracy";
            this.lblWeaponAccuracy.Size = new System.Drawing.Size(32, 13);
            this.lblWeaponAccuracy.TabIndex = 116;
            this.lblWeaponAccuracy.Text = "[Acc]";
            // 
            // lblWeaponMode
            // 
            this.lblWeaponMode.AutoSize = true;
            this.lblWeaponMode.Location = new System.Drawing.Point(178, 31);
            this.lblWeaponMode.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblWeaponMode.Name = "lblWeaponMode";
            this.lblWeaponMode.Size = new System.Drawing.Size(40, 13);
            this.lblWeaponMode.TabIndex = 65;
            this.lblWeaponMode.Text = "[Mode]";
            // 
            // lblWeaponModeLabel
            // 
            this.lblWeaponModeLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblWeaponModeLabel.AutoSize = true;
            this.lblWeaponModeLabel.Location = new System.Drawing.Point(135, 31);
            this.lblWeaponModeLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblWeaponModeLabel.Name = "lblWeaponModeLabel";
            this.lblWeaponModeLabel.Size = new System.Drawing.Size(37, 13);
            this.lblWeaponModeLabel.TabIndex = 64;
            this.lblWeaponModeLabel.Tag = "Label_Mode";
            this.lblWeaponModeLabel.Text = "Mode:";
            // 
            // lblWeaponDicePoolLabel
            // 
            this.lblWeaponDicePoolLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblWeaponDicePoolLabel.AutoSize = true;
            this.lblWeaponDicePoolLabel.Location = new System.Drawing.Point(3, 31);
            this.lblWeaponDicePoolLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblWeaponDicePoolLabel.Name = "lblWeaponDicePoolLabel";
            this.lblWeaponDicePoolLabel.Size = new System.Drawing.Size(56, 13);
            this.lblWeaponDicePoolLabel.TabIndex = 106;
            this.lblWeaponDicePoolLabel.Tag = "Label_DicePool";
            this.lblWeaponDicePoolLabel.Text = "Dice Pool:";
            // 
            // lblWeaponAccuracyLabel
            // 
            this.lblWeaponAccuracyLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblWeaponAccuracyLabel.AutoSize = true;
            this.lblWeaponAccuracyLabel.Location = new System.Drawing.Point(248, 6);
            this.lblWeaponAccuracyLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblWeaponAccuracyLabel.Name = "lblWeaponAccuracyLabel";
            this.lblWeaponAccuracyLabel.Size = new System.Drawing.Size(55, 13);
            this.lblWeaponAccuracyLabel.TabIndex = 115;
            this.lblWeaponAccuracyLabel.Tag = "Label_Accuracy";
            this.lblWeaponAccuracyLabel.Text = "Accuracy:";
            // 
            // lblWeaponAPLabel
            // 
            this.lblWeaponAPLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblWeaponAPLabel.AutoSize = true;
            this.lblWeaponAPLabel.Location = new System.Drawing.Point(148, 6);
            this.lblWeaponAPLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblWeaponAPLabel.Name = "lblWeaponAPLabel";
            this.lblWeaponAPLabel.Size = new System.Drawing.Size(24, 13);
            this.lblWeaponAPLabel.TabIndex = 60;
            this.lblWeaponAPLabel.Tag = "Label_AP";
            this.lblWeaponAPLabel.Text = "AP:";
            // 
            // lblWeaponAP
            // 
            this.lblWeaponAP.AutoSize = true;
            this.lblWeaponAP.Location = new System.Drawing.Point(178, 6);
            this.lblWeaponAP.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblWeaponAP.Name = "lblWeaponAP";
            this.lblWeaponAP.Size = new System.Drawing.Size(27, 13);
            this.lblWeaponAP.TabIndex = 61;
            this.lblWeaponAP.Text = "[AP]";
            // 
            // tlpWeaponsRanges
            // 
            this.tlpWeaponsRanges.AutoSize = true;
            this.tlpWeaponsRanges.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpWeaponsRanges.ColumnCount = 5;
            this.tlpWeaponsRanges.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tlpWeaponsRanges.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tlpWeaponsRanges.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tlpWeaponsRanges.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tlpWeaponsRanges.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tlpWeaponsRanges.Controls.Add(this.lblWeaponAlternateRangeExtreme, 4, 2);
            this.tlpWeaponsRanges.Controls.Add(this.lblWeaponRangeAlternate, 0, 2);
            this.tlpWeaponsRanges.Controls.Add(this.lblWeaponAlternateRangeLong, 3, 2);
            this.tlpWeaponsRanges.Controls.Add(this.lblWeaponRangeLabel, 0, 0);
            this.tlpWeaponsRanges.Controls.Add(this.lblWeaponAlternateRangeMedium, 2, 2);
            this.tlpWeaponsRanges.Controls.Add(this.lblWeaponRangeMain, 0, 1);
            this.tlpWeaponsRanges.Controls.Add(this.lblWeaponAlternateRangeShort, 1, 2);
            this.tlpWeaponsRanges.Controls.Add(this.lblWeaponRangeShortLabel, 1, 0);
            this.tlpWeaponsRanges.Controls.Add(this.lblWeaponRangeShort, 1, 1);
            this.tlpWeaponsRanges.Controls.Add(this.lblWeaponRangeMediumLabel, 2, 0);
            this.tlpWeaponsRanges.Controls.Add(this.lblWeaponRangeLongLabel, 3, 0);
            this.tlpWeaponsRanges.Controls.Add(this.lblWeaponRangeExtremeLabel, 4, 0);
            this.tlpWeaponsRanges.Controls.Add(this.lblWeaponRangeMedium, 2, 1);
            this.tlpWeaponsRanges.Controls.Add(this.lblWeaponRangeLong, 3, 1);
            this.tlpWeaponsRanges.Controls.Add(this.lblWeaponRangeExtreme, 4, 1);
            this.tlpWeaponsRanges.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpWeaponsRanges.Location = new System.Drawing.Point(0, 56);
            this.tlpWeaponsRanges.Margin = new System.Windows.Forms.Padding(0, 3, 0, 0);
            this.tlpWeaponsRanges.MinimumSize = new System.Drawing.Size(494, 0);
            this.tlpWeaponsRanges.Name = "tlpWeaponsRanges";
            this.tlpWeaponsRanges.RowCount = 3;
            this.tlpWeaponsRanges.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpWeaponsRanges.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpWeaponsRanges.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpWeaponsRanges.Size = new System.Drawing.Size(494, 75);
            this.tlpWeaponsRanges.TabIndex = 225;
            // 
            // lblWeaponAlternateRangeExtreme
            // 
            this.lblWeaponAlternateRangeExtreme.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblWeaponAlternateRangeExtreme.AutoSize = true;
            this.lblWeaponAlternateRangeExtreme.Location = new System.Drawing.Point(395, 56);
            this.lblWeaponAlternateRangeExtreme.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblWeaponAlternateRangeExtreme.Name = "lblWeaponAlternateRangeExtreme";
            this.lblWeaponAlternateRangeExtreme.Size = new System.Drawing.Size(96, 13);
            this.lblWeaponAlternateRangeExtreme.TabIndex = 220;
            this.lblWeaponAlternateRangeExtreme.Text = "[0]";
            this.lblWeaponAlternateRangeExtreme.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lblWeaponRangeAlternate
            // 
            this.lblWeaponRangeAlternate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblWeaponRangeAlternate.AutoSize = true;
            this.lblWeaponRangeAlternate.Location = new System.Drawing.Point(5, 56);
            this.lblWeaponRangeAlternate.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblWeaponRangeAlternate.Name = "lblWeaponRangeAlternate";
            this.lblWeaponRangeAlternate.Size = new System.Drawing.Size(90, 13);
            this.lblWeaponRangeAlternate.TabIndex = 222;
            this.lblWeaponRangeAlternate.Tag = "";
            this.lblWeaponRangeAlternate.Text = "[Alternate Range]";
            // 
            // lblWeaponAlternateRangeLong
            // 
            this.lblWeaponAlternateRangeLong.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblWeaponAlternateRangeLong.AutoSize = true;
            this.lblWeaponAlternateRangeLong.Location = new System.Drawing.Point(297, 56);
            this.lblWeaponAlternateRangeLong.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblWeaponAlternateRangeLong.Name = "lblWeaponAlternateRangeLong";
            this.lblWeaponAlternateRangeLong.Size = new System.Drawing.Size(92, 13);
            this.lblWeaponAlternateRangeLong.TabIndex = 219;
            this.lblWeaponAlternateRangeLong.Text = "[0]";
            this.lblWeaponAlternateRangeLong.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lblWeaponRangeLabel
            // 
            this.lblWeaponRangeLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblWeaponRangeLabel.AutoSize = true;
            this.lblWeaponRangeLabel.Location = new System.Drawing.Point(3, 6);
            this.lblWeaponRangeLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblWeaponRangeLabel.Name = "lblWeaponRangeLabel";
            this.lblWeaponRangeLabel.Size = new System.Drawing.Size(92, 13);
            this.lblWeaponRangeLabel.TabIndex = 90;
            this.lblWeaponRangeLabel.Tag = "Label_RangeHeading";
            this.lblWeaponRangeLabel.Text = "Range";
            this.lblWeaponRangeLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lblWeaponAlternateRangeMedium
            // 
            this.lblWeaponAlternateRangeMedium.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblWeaponAlternateRangeMedium.AutoSize = true;
            this.lblWeaponAlternateRangeMedium.Location = new System.Drawing.Point(199, 56);
            this.lblWeaponAlternateRangeMedium.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblWeaponAlternateRangeMedium.Name = "lblWeaponAlternateRangeMedium";
            this.lblWeaponAlternateRangeMedium.Size = new System.Drawing.Size(92, 13);
            this.lblWeaponAlternateRangeMedium.TabIndex = 218;
            this.lblWeaponAlternateRangeMedium.Text = "[0]";
            this.lblWeaponAlternateRangeMedium.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lblWeaponRangeMain
            // 
            this.lblWeaponRangeMain.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblWeaponRangeMain.AutoSize = true;
            this.lblWeaponRangeMain.Location = new System.Drawing.Point(24, 31);
            this.lblWeaponRangeMain.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblWeaponRangeMain.Name = "lblWeaponRangeMain";
            this.lblWeaponRangeMain.Size = new System.Drawing.Size(71, 13);
            this.lblWeaponRangeMain.TabIndex = 221;
            this.lblWeaponRangeMain.Tag = "";
            this.lblWeaponRangeMain.Text = "[Main Range]";
            // 
            // lblWeaponAlternateRangeShort
            // 
            this.lblWeaponAlternateRangeShort.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblWeaponAlternateRangeShort.AutoSize = true;
            this.lblWeaponAlternateRangeShort.Location = new System.Drawing.Point(101, 56);
            this.lblWeaponAlternateRangeShort.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblWeaponAlternateRangeShort.Name = "lblWeaponAlternateRangeShort";
            this.lblWeaponAlternateRangeShort.Size = new System.Drawing.Size(92, 13);
            this.lblWeaponAlternateRangeShort.TabIndex = 217;
            this.lblWeaponAlternateRangeShort.Text = "[0]";
            this.lblWeaponAlternateRangeShort.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lblWeaponRangeShortLabel
            // 
            this.lblWeaponRangeShortLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblWeaponRangeShortLabel.AutoSize = true;
            this.lblWeaponRangeShortLabel.Location = new System.Drawing.Point(101, 6);
            this.lblWeaponRangeShortLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblWeaponRangeShortLabel.Name = "lblWeaponRangeShortLabel";
            this.lblWeaponRangeShortLabel.Size = new System.Drawing.Size(92, 13);
            this.lblWeaponRangeShortLabel.TabIndex = 91;
            this.lblWeaponRangeShortLabel.Tag = "Label_RangeShort";
            this.lblWeaponRangeShortLabel.Text = "Short (-0)";
            this.lblWeaponRangeShortLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lblWeaponRangeShort
            // 
            this.lblWeaponRangeShort.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblWeaponRangeShort.AutoSize = true;
            this.lblWeaponRangeShort.Location = new System.Drawing.Point(101, 31);
            this.lblWeaponRangeShort.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblWeaponRangeShort.Name = "lblWeaponRangeShort";
            this.lblWeaponRangeShort.Size = new System.Drawing.Size(92, 13);
            this.lblWeaponRangeShort.TabIndex = 95;
            this.lblWeaponRangeShort.Text = "[0]";
            this.lblWeaponRangeShort.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lblWeaponRangeMediumLabel
            // 
            this.lblWeaponRangeMediumLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblWeaponRangeMediumLabel.AutoSize = true;
            this.lblWeaponRangeMediumLabel.Location = new System.Drawing.Point(199, 6);
            this.lblWeaponRangeMediumLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblWeaponRangeMediumLabel.Name = "lblWeaponRangeMediumLabel";
            this.lblWeaponRangeMediumLabel.Size = new System.Drawing.Size(92, 13);
            this.lblWeaponRangeMediumLabel.TabIndex = 92;
            this.lblWeaponRangeMediumLabel.Tag = "Label_RangeMedium";
            this.lblWeaponRangeMediumLabel.Text = "Medium (-1)";
            this.lblWeaponRangeMediumLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lblWeaponRangeLongLabel
            // 
            this.lblWeaponRangeLongLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblWeaponRangeLongLabel.AutoSize = true;
            this.lblWeaponRangeLongLabel.Location = new System.Drawing.Point(297, 6);
            this.lblWeaponRangeLongLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblWeaponRangeLongLabel.Name = "lblWeaponRangeLongLabel";
            this.lblWeaponRangeLongLabel.Size = new System.Drawing.Size(92, 13);
            this.lblWeaponRangeLongLabel.TabIndex = 93;
            this.lblWeaponRangeLongLabel.Tag = "Label_RangeLong";
            this.lblWeaponRangeLongLabel.Text = "Long (-3)";
            this.lblWeaponRangeLongLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lblWeaponRangeExtremeLabel
            // 
            this.lblWeaponRangeExtremeLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblWeaponRangeExtremeLabel.AutoSize = true;
            this.lblWeaponRangeExtremeLabel.Location = new System.Drawing.Point(395, 6);
            this.lblWeaponRangeExtremeLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblWeaponRangeExtremeLabel.Name = "lblWeaponRangeExtremeLabel";
            this.lblWeaponRangeExtremeLabel.Size = new System.Drawing.Size(96, 13);
            this.lblWeaponRangeExtremeLabel.TabIndex = 94;
            this.lblWeaponRangeExtremeLabel.Tag = "Label_RangeExtreme";
            this.lblWeaponRangeExtremeLabel.Text = "Extreme (-6)";
            this.lblWeaponRangeExtremeLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lblWeaponRangeMedium
            // 
            this.lblWeaponRangeMedium.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblWeaponRangeMedium.AutoSize = true;
            this.lblWeaponRangeMedium.Location = new System.Drawing.Point(199, 31);
            this.lblWeaponRangeMedium.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblWeaponRangeMedium.Name = "lblWeaponRangeMedium";
            this.lblWeaponRangeMedium.Size = new System.Drawing.Size(92, 13);
            this.lblWeaponRangeMedium.TabIndex = 96;
            this.lblWeaponRangeMedium.Text = "[0]";
            this.lblWeaponRangeMedium.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lblWeaponRangeLong
            // 
            this.lblWeaponRangeLong.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblWeaponRangeLong.AutoSize = true;
            this.lblWeaponRangeLong.Location = new System.Drawing.Point(297, 31);
            this.lblWeaponRangeLong.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblWeaponRangeLong.Name = "lblWeaponRangeLong";
            this.lblWeaponRangeLong.Size = new System.Drawing.Size(92, 13);
            this.lblWeaponRangeLong.TabIndex = 97;
            this.lblWeaponRangeLong.Text = "[0]";
            this.lblWeaponRangeLong.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lblWeaponRangeExtreme
            // 
            this.lblWeaponRangeExtreme.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblWeaponRangeExtreme.AutoSize = true;
            this.lblWeaponRangeExtreme.Location = new System.Drawing.Point(395, 31);
            this.lblWeaponRangeExtreme.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblWeaponRangeExtreme.Name = "lblWeaponRangeExtreme";
            this.lblWeaponRangeExtreme.Size = new System.Drawing.Size(96, 13);
            this.lblWeaponRangeExtreme.TabIndex = 98;
            this.lblWeaponRangeExtreme.Text = "[0]";
            this.lblWeaponRangeExtreme.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // gpbWeaponsMatrix
            // 
            this.gpbWeaponsMatrix.AutoSize = true;
            this.gpbWeaponsMatrix.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.gpbWeaponsMatrix.Controls.Add(this.tlpWeaponsMatrix);
            this.gpbWeaponsMatrix.Dock = System.Windows.Forms.DockStyle.Top;
            this.gpbWeaponsMatrix.Location = new System.Drawing.Point(3, 353);
            this.gpbWeaponsMatrix.Name = "gpbWeaponsMatrix";
            this.gpbWeaponsMatrix.Size = new System.Drawing.Size(500, 94);
            this.gpbWeaponsMatrix.TabIndex = 1;
            this.gpbWeaponsMatrix.TabStop = false;
            this.gpbWeaponsMatrix.Tag = "String_Matrix";
            this.gpbWeaponsMatrix.Text = "Matrix";
            // 
            // tlpWeaponsMatrix
            // 
            this.tlpWeaponsMatrix.AutoSize = true;
            this.tlpWeaponsMatrix.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpWeaponsMatrix.ColumnCount = 5;
            this.tlpWeaponsMatrix.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tlpWeaponsMatrix.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tlpWeaponsMatrix.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tlpWeaponsMatrix.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tlpWeaponsMatrix.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tlpWeaponsMatrix.Controls.Add(this.lblWeaponDeviceRating, 0, 2);
            this.tlpWeaponsMatrix.Controls.Add(this.lblWeaponAttack, 1, 2);
            this.tlpWeaponsMatrix.Controls.Add(this.lblWeaponSleaze, 2, 2);
            this.tlpWeaponsMatrix.Controls.Add(this.lblWeaponDataProcessing, 3, 2);
            this.tlpWeaponsMatrix.Controls.Add(this.lblWeaponFirewall, 4, 2);
            this.tlpWeaponsMatrix.Controls.Add(this.lblWeaponDeviceRatingLabel, 0, 1);
            this.tlpWeaponsMatrix.Controls.Add(this.lblWeaponAttackLabel, 1, 1);
            this.tlpWeaponsMatrix.Controls.Add(this.lblWeaponFirewallLabel, 4, 1);
            this.tlpWeaponsMatrix.Controls.Add(this.lblWeaponSleazeLabel, 2, 1);
            this.tlpWeaponsMatrix.Controls.Add(this.lblWeaponDataProcessingLabel, 3, 1);
            this.tlpWeaponsMatrix.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpWeaponsMatrix.Location = new System.Drawing.Point(3, 16);
            this.tlpWeaponsMatrix.Name = "tlpWeaponsMatrix";
            this.tlpWeaponsMatrix.RowCount = 3;
            this.tlpWeaponsMatrix.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tlpWeaponsMatrix.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpWeaponsMatrix.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpWeaponsMatrix.Size = new System.Drawing.Size(494, 75);
            this.tlpWeaponsMatrix.TabIndex = 0;
            // 
            // lblWeaponDeviceRating
            // 
            this.lblWeaponDeviceRating.AutoSize = true;
            this.lblWeaponDeviceRating.Location = new System.Drawing.Point(3, 56);
            this.lblWeaponDeviceRating.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblWeaponDeviceRating.Name = "lblWeaponDeviceRating";
            this.lblWeaponDeviceRating.Size = new System.Drawing.Size(19, 13);
            this.lblWeaponDeviceRating.TabIndex = 157;
            this.lblWeaponDeviceRating.Text = "[0]";
            // 
            // lblWeaponAttack
            // 
            this.lblWeaponAttack.AutoSize = true;
            this.lblWeaponAttack.Location = new System.Drawing.Point(101, 56);
            this.lblWeaponAttack.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblWeaponAttack.Name = "lblWeaponAttack";
            this.lblWeaponAttack.Size = new System.Drawing.Size(19, 13);
            this.lblWeaponAttack.TabIndex = 159;
            this.lblWeaponAttack.Text = "[0]";
            // 
            // lblWeaponSleaze
            // 
            this.lblWeaponSleaze.AutoSize = true;
            this.lblWeaponSleaze.Location = new System.Drawing.Point(199, 56);
            this.lblWeaponSleaze.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblWeaponSleaze.Name = "lblWeaponSleaze";
            this.lblWeaponSleaze.Size = new System.Drawing.Size(19, 13);
            this.lblWeaponSleaze.TabIndex = 161;
            this.lblWeaponSleaze.Text = "[0]";
            // 
            // lblWeaponDataProcessing
            // 
            this.lblWeaponDataProcessing.AutoSize = true;
            this.lblWeaponDataProcessing.Location = new System.Drawing.Point(297, 56);
            this.lblWeaponDataProcessing.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblWeaponDataProcessing.Name = "lblWeaponDataProcessing";
            this.lblWeaponDataProcessing.Size = new System.Drawing.Size(19, 13);
            this.lblWeaponDataProcessing.TabIndex = 163;
            this.lblWeaponDataProcessing.Text = "[0]";
            // 
            // lblWeaponFirewall
            // 
            this.lblWeaponFirewall.AutoSize = true;
            this.lblWeaponFirewall.Location = new System.Drawing.Point(395, 56);
            this.lblWeaponFirewall.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblWeaponFirewall.Name = "lblWeaponFirewall";
            this.lblWeaponFirewall.Size = new System.Drawing.Size(19, 13);
            this.lblWeaponFirewall.TabIndex = 165;
            this.lblWeaponFirewall.Text = "[0]";
            // 
            // lblWeaponDeviceRatingLabel
            // 
            this.lblWeaponDeviceRatingLabel.AutoSize = true;
            this.lblWeaponDeviceRatingLabel.Location = new System.Drawing.Point(3, 31);
            this.lblWeaponDeviceRatingLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblWeaponDeviceRatingLabel.Name = "lblWeaponDeviceRatingLabel";
            this.lblWeaponDeviceRatingLabel.Size = new System.Drawing.Size(78, 13);
            this.lblWeaponDeviceRatingLabel.TabIndex = 156;
            this.lblWeaponDeviceRatingLabel.Tag = "Label_DeviceRating";
            this.lblWeaponDeviceRatingLabel.Text = "Device Rating:";
            // 
            // lblWeaponAttackLabel
            // 
            this.lblWeaponAttackLabel.AutoSize = true;
            this.lblWeaponAttackLabel.Location = new System.Drawing.Point(101, 31);
            this.lblWeaponAttackLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblWeaponAttackLabel.Name = "lblWeaponAttackLabel";
            this.lblWeaponAttackLabel.Size = new System.Drawing.Size(41, 13);
            this.lblWeaponAttackLabel.TabIndex = 158;
            this.lblWeaponAttackLabel.Tag = "Label_Attack";
            this.lblWeaponAttackLabel.Text = "Attack:";
            // 
            // lblWeaponFirewallLabel
            // 
            this.lblWeaponFirewallLabel.AutoSize = true;
            this.lblWeaponFirewallLabel.Location = new System.Drawing.Point(395, 31);
            this.lblWeaponFirewallLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblWeaponFirewallLabel.Name = "lblWeaponFirewallLabel";
            this.lblWeaponFirewallLabel.Size = new System.Drawing.Size(45, 13);
            this.lblWeaponFirewallLabel.TabIndex = 164;
            this.lblWeaponFirewallLabel.Tag = "Label_Firewall";
            this.lblWeaponFirewallLabel.Text = "Firewall:";
            // 
            // lblWeaponSleazeLabel
            // 
            this.lblWeaponSleazeLabel.AutoSize = true;
            this.lblWeaponSleazeLabel.Location = new System.Drawing.Point(199, 31);
            this.lblWeaponSleazeLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblWeaponSleazeLabel.Name = "lblWeaponSleazeLabel";
            this.lblWeaponSleazeLabel.Size = new System.Drawing.Size(42, 13);
            this.lblWeaponSleazeLabel.TabIndex = 160;
            this.lblWeaponSleazeLabel.Tag = "Label_Sleaze";
            this.lblWeaponSleazeLabel.Text = "Sleaze:";
            // 
            // lblWeaponDataProcessingLabel
            // 
            this.lblWeaponDataProcessingLabel.AutoSize = true;
            this.lblWeaponDataProcessingLabel.Location = new System.Drawing.Point(297, 31);
            this.lblWeaponDataProcessingLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblWeaponDataProcessingLabel.Name = "lblWeaponDataProcessingLabel";
            this.lblWeaponDataProcessingLabel.Size = new System.Drawing.Size(88, 13);
            this.lblWeaponDataProcessingLabel.TabIndex = 162;
            this.lblWeaponDataProcessingLabel.Tag = "Label_DataProcessing";
            this.lblWeaponDataProcessingLabel.Text = "Data Processing:";
            // 
            // tlpWeaponsButtons
            // 
            this.tlpWeaponsButtons.AutoSize = true;
            this.tlpWeaponsButtons.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpWeaponsButtons.ColumnCount = 3;
            this.tlpWeapons.SetColumnSpan(this.tlpWeaponsButtons, 2);
            this.tlpWeaponsButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tlpWeaponsButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tlpWeaponsButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tlpWeaponsButtons.Controls.Add(this.cmdAddWeapon, 0, 0);
            this.tlpWeaponsButtons.Controls.Add(this.cmdAddWeaponLocation, 2, 0);
            this.tlpWeaponsButtons.Controls.Add(this.cmdDeleteWeapon, 1, 0);
            this.tlpWeaponsButtons.Location = new System.Drawing.Point(0, 0);
            this.tlpWeaponsButtons.Margin = new System.Windows.Forms.Padding(0);
            this.tlpWeaponsButtons.Name = "tlpWeaponsButtons";
            this.tlpWeaponsButtons.RowCount = 1;
            this.tlpWeaponsButtons.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpWeaponsButtons.Size = new System.Drawing.Size(178, 29);
            this.tlpWeaponsButtons.TabIndex = 228;
            // 
            // cmdAddWeapon
            // 
            this.cmdAddWeapon.AutoSize = true;
            this.cmdAddWeapon.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdAddWeapon.ContextMenuStrip = this.cmsWeapon;
            this.cmdAddWeapon.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmdAddWeapon.Location = new System.Drawing.Point(3, 3);
            this.cmdAddWeapon.Name = "cmdAddWeapon";
            this.cmdAddWeapon.Size = new System.Drawing.Size(53, 23);
            this.cmdAddWeapon.SplitMenuStrip = this.cmsWeapon;
            this.cmdAddWeapon.TabIndex = 154;
            this.cmdAddWeapon.Tag = "Button_AddWeapon";
            this.cmdAddWeapon.Text = "&Add Weapon";
            this.cmdAddWeapon.UseVisualStyleBackColor = true;
            this.cmdAddWeapon.Click += new System.EventHandler(this.cmdAddWeapon_Click);
            // 
            // cmdAddWeaponLocation
            // 
            this.cmdAddWeaponLocation.AutoSize = true;
            this.cmdAddWeaponLocation.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdAddWeaponLocation.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmdAddWeaponLocation.Location = new System.Drawing.Point(121, 3);
            this.cmdAddWeaponLocation.Name = "cmdAddWeaponLocation";
            this.cmdAddWeaponLocation.Size = new System.Drawing.Size(54, 23);
            this.cmdAddWeaponLocation.TabIndex = 114;
            this.cmdAddWeaponLocation.Tag = "Button_AddLocation";
            this.cmdAddWeaponLocation.Text = "Add Location";
            this.cmdAddWeaponLocation.UseVisualStyleBackColor = true;
            this.cmdAddWeaponLocation.Click += new System.EventHandler(this.cmdAddWeaponLocation_Click);
            // 
            // cmdDeleteWeapon
            // 
            this.cmdDeleteWeapon.AutoSize = true;
            this.cmdDeleteWeapon.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdDeleteWeapon.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmdDeleteWeapon.Location = new System.Drawing.Point(62, 3);
            this.cmdDeleteWeapon.Name = "cmdDeleteWeapon";
            this.cmdDeleteWeapon.Size = new System.Drawing.Size(53, 23);
            this.cmdDeleteWeapon.TabIndex = 46;
            this.cmdDeleteWeapon.Tag = "String_Delete";
            this.cmdDeleteWeapon.Text = "Delete";
            this.cmdDeleteWeapon.UseVisualStyleBackColor = true;
            this.cmdDeleteWeapon.Click += new System.EventHandler(this.cmdDeleteWeapon_Click);
            // 
            // tabDrugs
            // 
            this.tabDrugs.BackColor = System.Drawing.SystemColors.Control;
            this.tabDrugs.Controls.Add(this.tlpDrugInfo);
            this.tabDrugs.Location = new System.Drawing.Point(4, 22);
            this.tabDrugs.Name = "tabDrugs";
            this.tabDrugs.Padding = new System.Windows.Forms.Padding(3);
            this.tabDrugs.Size = new System.Drawing.Size(184, 48);
            this.tabDrugs.TabIndex = 6;
            this.tabDrugs.Tag = "Tab_Drugs";
            this.tabDrugs.Text = "Drugs";
            // 
            // tlpDrugInfo
            // 
            this.tlpDrugInfo.AutoSize = true;
            this.tlpDrugInfo.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpDrugInfo.ColumnCount = 2;
            this.tlpDrugInfo.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpDrugInfo.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpDrugInfo.Controls.Add(this.treCustomDrugs, 0, 1);
            this.tlpDrugInfo.Controls.Add(this.flpDrugs, 1, 1);
            this.tlpDrugInfo.Controls.Add(this.tlpDrugButtons, 0, 0);
            this.tlpDrugInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpDrugInfo.Location = new System.Drawing.Point(3, 3);
            this.tlpDrugInfo.MinimumSize = new System.Drawing.Size(815, 587);
            this.tlpDrugInfo.Name = "tlpDrugInfo";
            this.tlpDrugInfo.RowCount = 2;
            this.tlpDrugInfo.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpDrugInfo.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpDrugInfo.Size = new System.Drawing.Size(815, 587);
            this.tlpDrugInfo.TabIndex = 97;
            // 
            // treCustomDrugs
            // 
            this.treCustomDrugs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treCustomDrugs.Location = new System.Drawing.Point(3, 32);
            this.treCustomDrugs.Name = "treCustomDrugs";
            treeNode22.Name = "treeNode24";
            treeNode22.Tag = "Node_SelectedDrugs";
            treeNode22.Text = "Selected Drugs";
            this.treCustomDrugs.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            treeNode22});
            this.treCustomDrugs.Size = new System.Drawing.Size(295, 552);
            this.treCustomDrugs.TabIndex = 2;
            this.treCustomDrugs.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treCustomDrugs_AfterSelect);
            // 
            // flpDrugs
            // 
            this.flpDrugs.AutoScroll = true;
            this.flpDrugs.Controls.Add(this.gpbDrugsCommon);
            this.flpDrugs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flpDrugs.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flpDrugs.Location = new System.Drawing.Point(301, 29);
            this.flpDrugs.Margin = new System.Windows.Forms.Padding(0);
            this.flpDrugs.Name = "flpDrugs";
            this.flpDrugs.Size = new System.Drawing.Size(514, 558);
            this.flpDrugs.TabIndex = 100;
            this.flpDrugs.WrapContents = false;
            // 
            // gpbDrugsCommon
            // 
            this.gpbDrugsCommon.AutoSize = true;
            this.gpbDrugsCommon.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.gpbDrugsCommon.Controls.Add(this.tlpDrugsCommon);
            this.gpbDrugsCommon.Dock = System.Windows.Forms.DockStyle.Top;
            this.gpbDrugsCommon.Location = new System.Drawing.Point(3, 3);
            this.gpbDrugsCommon.Name = "gpbDrugsCommon";
            this.gpbDrugsCommon.Size = new System.Drawing.Size(159, 293);
            this.gpbDrugsCommon.TabIndex = 0;
            this.gpbDrugsCommon.TabStop = false;
            this.gpbDrugsCommon.Tag = "String_Info";
            this.gpbDrugsCommon.Text = "Info";
            // 
            // tlpDrugsCommon
            // 
            this.tlpDrugsCommon.AutoSize = true;
            this.tlpDrugsCommon.ColumnCount = 2;
            this.tlpDrugsCommon.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpDrugsCommon.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpDrugsCommon.Controls.Add(this.chkDrugStolen, 1, 10);
            this.tlpDrugsCommon.Controls.Add(this.lblDrugNameLabel, 0, 0);
            this.tlpDrugsCommon.Controls.Add(this.lblDrugEffectLabel, 0, 9);
            this.tlpDrugsCommon.Controls.Add(this.lblDrugName, 1, 0);
            this.tlpDrugsCommon.Controls.Add(this.lblDrugComponentsLabel, 0, 8);
            this.tlpDrugsCommon.Controls.Add(this.lblDrugCategoryLabel, 0, 1);
            this.tlpDrugsCommon.Controls.Add(this.lblDrugAddictionThresholdLabel, 0, 6);
            this.tlpDrugsCommon.Controls.Add(this.lblDrugAddictionRatingLabel, 0, 7);
            this.tlpDrugsCommon.Controls.Add(this.lblDrugCategory, 1, 1);
            this.tlpDrugsCommon.Controls.Add(this.lblDrugQtyLabel, 0, 2);
            this.tlpDrugsCommon.Controls.Add(this.nudDrugQty, 1, 2);
            this.tlpDrugsCommon.Controls.Add(this.lblDrugGrade, 1, 3);
            this.tlpDrugsCommon.Controls.Add(this.lblDrugCostLabel, 0, 5);
            this.tlpDrugsCommon.Controls.Add(this.lblDrugGradeLabel, 0, 3);
            this.tlpDrugsCommon.Controls.Add(this.lblDrugAvailabel, 0, 4);
            this.tlpDrugsCommon.Controls.Add(this.lblDrugAvail, 1, 4);
            this.tlpDrugsCommon.Controls.Add(this.lblDrugCost, 1, 5);
            this.tlpDrugsCommon.Controls.Add(this.lblDrugAddictionThreshold, 1, 6);
            this.tlpDrugsCommon.Controls.Add(this.lblDrugAddictionRating, 1, 7);
            this.tlpDrugsCommon.Controls.Add(this.lblDrugComponents, 1, 8);
            this.tlpDrugsCommon.Controls.Add(this.lblDrugEffect, 1, 9);
            this.tlpDrugsCommon.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpDrugsCommon.Location = new System.Drawing.Point(3, 16);
            this.tlpDrugsCommon.Name = "tlpDrugsCommon";
            this.tlpDrugsCommon.RowCount = 11;
            this.tlpDrugsCommon.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpDrugsCommon.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpDrugsCommon.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpDrugsCommon.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpDrugsCommon.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpDrugsCommon.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpDrugsCommon.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpDrugsCommon.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpDrugsCommon.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpDrugsCommon.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpDrugsCommon.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpDrugsCommon.Size = new System.Drawing.Size(153, 274);
            this.tlpDrugsCommon.TabIndex = 0;
            // 
            // chkDrugStolen
            // 
            this.chkDrugStolen.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkDrugStolen.AutoSize = true;
            this.chkDrugStolen.Location = new System.Drawing.Point(78, 254);
            this.chkDrugStolen.Name = "chkDrugStolen";
            this.chkDrugStolen.Size = new System.Drawing.Size(56, 17);
            this.chkDrugStolen.TabIndex = 230;
            this.chkDrugStolen.Tag = "Checkbox_Stolen";
            this.chkDrugStolen.Text = "Stolen";
            this.chkDrugStolen.UseVisualStyleBackColor = true;
            this.chkDrugStolen.Visible = false;
            this.chkDrugStolen.CheckedChanged += new System.EventHandler(this.chkDrugStolen_CheckedChanged);
            // 
            // lblDrugNameLabel
            // 
            this.lblDrugNameLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblDrugNameLabel.AutoSize = true;
            this.lblDrugNameLabel.Location = new System.Drawing.Point(34, 6);
            this.lblDrugNameLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblDrugNameLabel.Name = "lblDrugNameLabel";
            this.lblDrugNameLabel.Size = new System.Drawing.Size(38, 13);
            this.lblDrugNameLabel.TabIndex = 73;
            this.lblDrugNameLabel.Tag = "Label_Name";
            this.lblDrugNameLabel.Text = "Name:";
            // 
            // lblDrugEffectLabel
            // 
            this.lblDrugEffectLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblDrugEffectLabel.AutoSize = true;
            this.lblDrugEffectLabel.Location = new System.Drawing.Point(29, 232);
            this.lblDrugEffectLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblDrugEffectLabel.Name = "lblDrugEffectLabel";
            this.lblDrugEffectLabel.Size = new System.Drawing.Size(43, 13);
            this.lblDrugEffectLabel.TabIndex = 97;
            this.lblDrugEffectLabel.Text = "Effects:";
            // 
            // lblDrugName
            // 
            this.lblDrugName.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblDrugName.AutoSize = true;
            this.lblDrugName.Location = new System.Drawing.Point(78, 6);
            this.lblDrugName.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblDrugName.Name = "lblDrugName";
            this.lblDrugName.Size = new System.Drawing.Size(41, 13);
            this.lblDrugName.TabIndex = 74;
            this.lblDrugName.Text = "[Name]";
            // 
            // lblDrugComponentsLabel
            // 
            this.lblDrugComponentsLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblDrugComponentsLabel.AutoSize = true;
            this.lblDrugComponentsLabel.Location = new System.Drawing.Point(3, 207);
            this.lblDrugComponentsLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblDrugComponentsLabel.Name = "lblDrugComponentsLabel";
            this.lblDrugComponentsLabel.Size = new System.Drawing.Size(69, 13);
            this.lblDrugComponentsLabel.TabIndex = 91;
            this.lblDrugComponentsLabel.Tag = "Label_DrugComponents";
            this.lblDrugComponentsLabel.Text = "Components:";
            // 
            // lblDrugCategoryLabel
            // 
            this.lblDrugCategoryLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblDrugCategoryLabel.AutoSize = true;
            this.lblDrugCategoryLabel.Location = new System.Drawing.Point(20, 31);
            this.lblDrugCategoryLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblDrugCategoryLabel.Name = "lblDrugCategoryLabel";
            this.lblDrugCategoryLabel.Size = new System.Drawing.Size(52, 13);
            this.lblDrugCategoryLabel.TabIndex = 75;
            this.lblDrugCategoryLabel.Tag = "Label_Category";
            this.lblDrugCategoryLabel.Text = "Category:";
            // 
            // lblDrugAddictionThresholdLabel
            // 
            this.lblDrugAddictionThresholdLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblDrugAddictionThresholdLabel.AutoSize = true;
            this.lblDrugAddictionThresholdLabel.Location = new System.Drawing.Point(15, 157);
            this.lblDrugAddictionThresholdLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblDrugAddictionThresholdLabel.Name = "lblDrugAddictionThresholdLabel";
            this.lblDrugAddictionThresholdLabel.Size = new System.Drawing.Size(57, 13);
            this.lblDrugAddictionThresholdLabel.TabIndex = 92;
            this.lblDrugAddictionThresholdLabel.Tag = "Label_AddictionThreshold";
            this.lblDrugAddictionThresholdLabel.Text = "Threshold:";
            // 
            // lblDrugAddictionRatingLabel
            // 
            this.lblDrugAddictionRatingLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblDrugAddictionRatingLabel.AutoSize = true;
            this.lblDrugAddictionRatingLabel.Location = new System.Drawing.Point(31, 182);
            this.lblDrugAddictionRatingLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblDrugAddictionRatingLabel.Name = "lblDrugAddictionRatingLabel";
            this.lblDrugAddictionRatingLabel.Size = new System.Drawing.Size(41, 13);
            this.lblDrugAddictionRatingLabel.TabIndex = 90;
            this.lblDrugAddictionRatingLabel.Tag = "Label_Rating";
            this.lblDrugAddictionRatingLabel.Text = "Rating:";
            // 
            // lblDrugCategory
            // 
            this.lblDrugCategory.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblDrugCategory.AutoSize = true;
            this.lblDrugCategory.Location = new System.Drawing.Point(78, 31);
            this.lblDrugCategory.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblDrugCategory.Name = "lblDrugCategory";
            this.lblDrugCategory.Size = new System.Drawing.Size(55, 13);
            this.lblDrugCategory.TabIndex = 76;
            this.lblDrugCategory.Text = "[Category]";
            // 
            // lblDrugQtyLabel
            // 
            this.lblDrugQtyLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblDrugQtyLabel.AutoSize = true;
            this.lblDrugQtyLabel.Location = new System.Drawing.Point(46, 56);
            this.lblDrugQtyLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblDrugQtyLabel.Name = "lblDrugQtyLabel";
            this.lblDrugQtyLabel.Size = new System.Drawing.Size(26, 13);
            this.lblDrugQtyLabel.TabIndex = 95;
            this.lblDrugQtyLabel.Tag = "Label_Qty";
            this.lblDrugQtyLabel.Text = "Qty:";
            // 
            // nudDrugQty
            // 
            this.nudDrugQty.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.nudDrugQty.AutoSize = true;
            this.nudDrugQty.Enabled = false;
            this.nudDrugQty.Location = new System.Drawing.Point(78, 53);
            this.nudDrugQty.Maximum = new decimal(new int[] {
            9000,
            0,
            0,
            0});
            this.nudDrugQty.Name = "nudDrugQty";
            this.nudDrugQty.Size = new System.Drawing.Size(47, 20);
            this.nudDrugQty.TabIndex = 96;
            this.nudDrugQty.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudDrugQty.Visible = false;
            this.nudDrugQty.ValueChanged += new System.EventHandler(this.nudDrugQty_ValueChanged);
            // 
            // lblDrugGrade
            // 
            this.lblDrugGrade.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblDrugGrade.AutoSize = true;
            this.lblDrugGrade.Location = new System.Drawing.Point(78, 82);
            this.lblDrugGrade.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblDrugGrade.Name = "lblDrugGrade";
            this.lblDrugGrade.Size = new System.Drawing.Size(42, 13);
            this.lblDrugGrade.TabIndex = 94;
            this.lblDrugGrade.Text = "[Grade]";
            // 
            // lblDrugCostLabel
            // 
            this.lblDrugCostLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblDrugCostLabel.AutoSize = true;
            this.lblDrugCostLabel.Location = new System.Drawing.Point(41, 132);
            this.lblDrugCostLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblDrugCostLabel.Name = "lblDrugCostLabel";
            this.lblDrugCostLabel.Size = new System.Drawing.Size(31, 13);
            this.lblDrugCostLabel.TabIndex = 79;
            this.lblDrugCostLabel.Tag = "Label_Cost";
            this.lblDrugCostLabel.Text = "Cost:";
            // 
            // lblDrugGradeLabel
            // 
            this.lblDrugGradeLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblDrugGradeLabel.AutoSize = true;
            this.lblDrugGradeLabel.Location = new System.Drawing.Point(33, 82);
            this.lblDrugGradeLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblDrugGradeLabel.Name = "lblDrugGradeLabel";
            this.lblDrugGradeLabel.Size = new System.Drawing.Size(39, 13);
            this.lblDrugGradeLabel.TabIndex = 93;
            this.lblDrugGradeLabel.Tag = "Label_Grade";
            this.lblDrugGradeLabel.Text = "Grade:";
            // 
            // lblDrugAvailabel
            // 
            this.lblDrugAvailabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblDrugAvailabel.AutoSize = true;
            this.lblDrugAvailabel.Location = new System.Drawing.Point(39, 107);
            this.lblDrugAvailabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblDrugAvailabel.Name = "lblDrugAvailabel";
            this.lblDrugAvailabel.Size = new System.Drawing.Size(33, 13);
            this.lblDrugAvailabel.TabIndex = 77;
            this.lblDrugAvailabel.Tag = "Label_Avail";
            this.lblDrugAvailabel.Text = "Avail:";
            // 
            // lblDrugAvail
            // 
            this.lblDrugAvail.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblDrugAvail.AutoSize = true;
            this.lblDrugAvail.Location = new System.Drawing.Point(78, 107);
            this.lblDrugAvail.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblDrugAvail.Name = "lblDrugAvail";
            this.lblDrugAvail.Size = new System.Drawing.Size(36, 13);
            this.lblDrugAvail.TabIndex = 78;
            this.lblDrugAvail.Text = "[Avail]";
            // 
            // lblDrugCost
            // 
            this.lblDrugCost.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblDrugCost.AutoSize = true;
            this.lblDrugCost.Location = new System.Drawing.Point(78, 132);
            this.lblDrugCost.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblDrugCost.Name = "lblDrugCost";
            this.lblDrugCost.Size = new System.Drawing.Size(34, 13);
            this.lblDrugCost.TabIndex = 80;
            this.lblDrugCost.Text = "[Cost]";
            // 
            // lblDrugAddictionThreshold
            // 
            this.lblDrugAddictionThreshold.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblDrugAddictionThreshold.AutoSize = true;
            this.lblDrugAddictionThreshold.Location = new System.Drawing.Point(78, 157);
            this.lblDrugAddictionThreshold.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblDrugAddictionThreshold.Name = "lblDrugAddictionThreshold";
            this.lblDrugAddictionThreshold.Size = new System.Drawing.Size(60, 13);
            this.lblDrugAddictionThreshold.TabIndex = 86;
            this.lblDrugAddictionThreshold.Text = "[Threshold]";
            // 
            // lblDrugAddictionRating
            // 
            this.lblDrugAddictionRating.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblDrugAddictionRating.AutoSize = true;
            this.lblDrugAddictionRating.Location = new System.Drawing.Point(78, 182);
            this.lblDrugAddictionRating.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblDrugAddictionRating.Name = "lblDrugAddictionRating";
            this.lblDrugAddictionRating.Size = new System.Drawing.Size(44, 13);
            this.lblDrugAddictionRating.TabIndex = 88;
            this.lblDrugAddictionRating.Text = "[Rating]";
            // 
            // lblDrugComponents
            // 
            this.lblDrugComponents.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblDrugComponents.AutoSize = true;
            this.lblDrugComponents.Location = new System.Drawing.Point(78, 207);
            this.lblDrugComponents.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblDrugComponents.Name = "lblDrugComponents";
            this.lblDrugComponents.Size = new System.Drawing.Size(72, 13);
            this.lblDrugComponents.TabIndex = 84;
            this.lblDrugComponents.Text = "[Components]";
            // 
            // lblDrugEffect
            // 
            this.lblDrugEffect.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblDrugEffect.AutoSize = true;
            this.lblDrugEffect.Location = new System.Drawing.Point(78, 232);
            this.lblDrugEffect.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblDrugEffect.Name = "lblDrugEffect";
            this.lblDrugEffect.Size = new System.Drawing.Size(19, 13);
            this.lblDrugEffect.TabIndex = 98;
            this.lblDrugEffect.Text = "[0]";
            // 
            // tlpDrugButtons
            // 
            this.tlpDrugButtons.AutoSize = true;
            this.tlpDrugButtons.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpDrugButtons.ColumnCount = 2;
            this.tlpDrugInfo.SetColumnSpan(this.tlpDrugButtons, 2);
            this.tlpDrugButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpDrugButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpDrugButtons.Controls.Add(this.btnCreateCustomDrug, 0, 0);
            this.tlpDrugButtons.Controls.Add(this.btnDeleteCustomDrug, 1, 0);
            this.tlpDrugButtons.Location = new System.Drawing.Point(0, 0);
            this.tlpDrugButtons.Margin = new System.Windows.Forms.Padding(0);
            this.tlpDrugButtons.Name = "tlpDrugButtons";
            this.tlpDrugButtons.RowCount = 1;
            this.tlpDrugButtons.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpDrugButtons.Size = new System.Drawing.Size(236, 29);
            this.tlpDrugButtons.TabIndex = 101;
            // 
            // btnCreateCustomDrug
            // 
            this.btnCreateCustomDrug.AutoSize = true;
            this.btnCreateCustomDrug.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnCreateCustomDrug.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnCreateCustomDrug.Location = new System.Drawing.Point(3, 3);
            this.btnCreateCustomDrug.Name = "btnCreateCustomDrug";
            this.btnCreateCustomDrug.Size = new System.Drawing.Size(112, 23);
            this.btnCreateCustomDrug.TabIndex = 1;
            this.btnCreateCustomDrug.Tag = "Button_CreateCustomDrug";
            this.btnCreateCustomDrug.Text = "Create Custom Drug";
            this.btnCreateCustomDrug.UseVisualStyleBackColor = true;
            this.btnCreateCustomDrug.Click += new System.EventHandler(this.btnCreateCustomDrug_Click_1);
            // 
            // btnDeleteCustomDrug
            // 
            this.btnDeleteCustomDrug.AutoSize = true;
            this.btnDeleteCustomDrug.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnDeleteCustomDrug.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnDeleteCustomDrug.Location = new System.Drawing.Point(121, 3);
            this.btnDeleteCustomDrug.Name = "btnDeleteCustomDrug";
            this.btnDeleteCustomDrug.Size = new System.Drawing.Size(112, 23);
            this.btnDeleteCustomDrug.TabIndex = 3;
            this.btnDeleteCustomDrug.Tag = "String_Delete";
            this.btnDeleteCustomDrug.Text = "Delete";
            this.btnDeleteCustomDrug.UseVisualStyleBackColor = true;
            this.btnDeleteCustomDrug.Click += new System.EventHandler(this.btnDeleteCustomDrug_Click);
            // 
            // tabLifestyle
            // 
            this.tabLifestyle.BackColor = System.Drawing.SystemColors.Control;
            this.tabLifestyle.Controls.Add(this.tlpLifestyleDetails);
            this.tabLifestyle.Location = new System.Drawing.Point(4, 22);
            this.tabLifestyle.Name = "tabLifestyle";
            this.tabLifestyle.Padding = new System.Windows.Forms.Padding(3);
            this.tabLifestyle.Size = new System.Drawing.Size(184, 48);
            this.tabLifestyle.TabIndex = 0;
            this.tabLifestyle.Tag = "Tab_Lifestyle";
            this.tabLifestyle.Text = "Lifestyle";
            // 
            // tlpLifestyleDetails
            // 
            this.tlpLifestyleDetails.AutoSize = true;
            this.tlpLifestyleDetails.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpLifestyleDetails.ColumnCount = 2;
            this.tlpLifestyleDetails.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpLifestyleDetails.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpLifestyleDetails.Controls.Add(this.treLifestyles, 0, 1);
            this.tlpLifestyleDetails.Controls.Add(this.flpLifestyleDetails, 1, 1);
            this.tlpLifestyleDetails.Controls.Add(this.tlpLifestyleButtons, 0, 0);
            this.tlpLifestyleDetails.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpLifestyleDetails.Location = new System.Drawing.Point(3, 3);
            this.tlpLifestyleDetails.MinimumSize = new System.Drawing.Size(815, 587);
            this.tlpLifestyleDetails.Name = "tlpLifestyleDetails";
            this.tlpLifestyleDetails.RowCount = 2;
            this.tlpLifestyleDetails.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpLifestyleDetails.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpLifestyleDetails.Size = new System.Drawing.Size(815, 587);
            this.tlpLifestyleDetails.TabIndex = 104;
            // 
            // treLifestyles
            // 
            this.treLifestyles.AllowDrop = true;
            this.treLifestyles.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treLifestyles.HideSelection = false;
            this.treLifestyles.Location = new System.Drawing.Point(3, 32);
            this.treLifestyles.Name = "treLifestyles";
            treeNode23.Name = "nodLifestylesRoot";
            treeNode23.Tag = "Node_SelectedLifestyles";
            treeNode23.Text = "Selected Lifestyles";
            this.treLifestyles.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            treeNode23});
            this.treLifestyles.ShowNodeToolTips = true;
            this.treLifestyles.ShowRootLines = false;
            this.treLifestyles.Size = new System.Drawing.Size(295, 552);
            this.treLifestyles.TabIndex = 80;
            this.treLifestyles.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treLifestyles_AfterSelect);
            this.treLifestyles.DragOver += new System.Windows.Forms.DragEventHandler(this.treLifestyles_DragOver);
            this.treLifestyles.DoubleClick += new System.EventHandler(this.treLifestyles_DoubleClick);
            this.treLifestyles.KeyDown += new System.Windows.Forms.KeyEventHandler(this.treLifestyles_KeyDown);
            this.treLifestyles.MouseDown += new System.Windows.Forms.MouseEventHandler(this.TreeView_MouseDown);
            // 
            // flpLifestyleDetails
            // 
            this.flpLifestyleDetails.AutoScroll = true;
            this.flpLifestyleDetails.Controls.Add(this.gpbLifestyleCommon);
            this.flpLifestyleDetails.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flpLifestyleDetails.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flpLifestyleDetails.Location = new System.Drawing.Point(301, 29);
            this.flpLifestyleDetails.Margin = new System.Windows.Forms.Padding(0);
            this.flpLifestyleDetails.Name = "flpLifestyleDetails";
            this.flpLifestyleDetails.Size = new System.Drawing.Size(514, 558);
            this.flpLifestyleDetails.TabIndex = 105;
            this.flpLifestyleDetails.WrapContents = false;
            // 
            // gpbLifestyleCommon
            // 
            this.gpbLifestyleCommon.AutoSize = true;
            this.gpbLifestyleCommon.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.gpbLifestyleCommon.Controls.Add(this.tlpLifestyleCommon);
            this.gpbLifestyleCommon.Dock = System.Windows.Forms.DockStyle.Top;
            this.gpbLifestyleCommon.Location = new System.Drawing.Point(3, 3);
            this.gpbLifestyleCommon.Name = "gpbLifestyleCommon";
            this.gpbLifestyleCommon.Size = new System.Drawing.Size(244, 170);
            this.gpbLifestyleCommon.TabIndex = 0;
            this.gpbLifestyleCommon.TabStop = false;
            this.gpbLifestyleCommon.Tag = "String_Info";
            this.gpbLifestyleCommon.Text = "Info";
            // 
            // tlpLifestyleCommon
            // 
            this.tlpLifestyleCommon.AutoSize = true;
            this.tlpLifestyleCommon.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpLifestyleCommon.ColumnCount = 2;
            this.tlpLifestyleCommon.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpLifestyleCommon.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpLifestyleCommon.Controls.Add(this.lblLifestyleCostLabel, 0, 0);
            this.tlpLifestyleCommon.Controls.Add(this.lblLifestyleQualities, 1, 5);
            this.tlpLifestyleCommon.Controls.Add(this.lblLifestyleStartingNuyen, 1, 4);
            this.tlpLifestyleCommon.Controls.Add(this.flpLifestyleIntervals, 0, 1);
            this.tlpLifestyleCommon.Controls.Add(this.lblLifestyleCost, 1, 0);
            this.tlpLifestyleCommon.Controls.Add(this.lblBaseLifestyle, 1, 3);
            this.tlpLifestyleCommon.Controls.Add(this.lblLifestyleSourceLabel, 0, 2);
            this.tlpLifestyleCommon.Controls.Add(this.lblLifestyleSource, 1, 2);
            this.tlpLifestyleCommon.Controls.Add(this.lblLifestyleComfortsLabel, 0, 3);
            this.tlpLifestyleCommon.Controls.Add(this.lblLifestyleQualitiesLabel, 0, 5);
            this.tlpLifestyleCommon.Controls.Add(this.lblLifestyleStartingNuyenLabel, 0, 4);
            this.tlpLifestyleCommon.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpLifestyleCommon.Location = new System.Drawing.Point(3, 16);
            this.tlpLifestyleCommon.Name = "tlpLifestyleCommon";
            this.tlpLifestyleCommon.RowCount = 6;
            this.tlpLifestyleCommon.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpLifestyleCommon.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpLifestyleCommon.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpLifestyleCommon.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpLifestyleCommon.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpLifestyleCommon.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpLifestyleCommon.Size = new System.Drawing.Size(238, 151);
            this.tlpLifestyleCommon.TabIndex = 0;
            // 
            // lblLifestyleCostLabel
            // 
            this.lblLifestyleCostLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblLifestyleCostLabel.AutoSize = true;
            this.lblLifestyleCostLabel.Location = new System.Drawing.Point(17, 6);
            this.lblLifestyleCostLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblLifestyleCostLabel.Name = "lblLifestyleCostLabel";
            this.lblLifestyleCostLabel.Size = new System.Drawing.Size(66, 13);
            this.lblLifestyleCostLabel.TabIndex = 85;
            this.lblLifestyleCostLabel.Tag = "Label_SelectLifestyle_CostPerMonth";
            this.lblLifestyleCostLabel.Text = "Cost/Month:";
            // 
            // lblLifestyleQualities
            // 
            this.lblLifestyleQualities.AutoSize = true;
            this.lblLifestyleQualities.Location = new System.Drawing.Point(89, 132);
            this.lblLifestyleQualities.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblLifestyleQualities.MaximumSize = new System.Drawing.Size(427, 0);
            this.lblLifestyleQualities.Name = "lblLifestyleQualities";
            this.lblLifestyleQualities.Size = new System.Drawing.Size(19, 13);
            this.lblLifestyleQualities.TabIndex = 103;
            this.lblLifestyleQualities.Text = "[0]";
            // 
            // lblLifestyleStartingNuyen
            // 
            this.lblLifestyleStartingNuyen.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblLifestyleStartingNuyen.AutoSize = true;
            this.lblLifestyleStartingNuyen.Location = new System.Drawing.Point(89, 107);
            this.lblLifestyleStartingNuyen.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblLifestyleStartingNuyen.MaximumSize = new System.Drawing.Size(427, 0);
            this.lblLifestyleStartingNuyen.Name = "lblLifestyleStartingNuyen";
            this.lblLifestyleStartingNuyen.Size = new System.Drawing.Size(83, 13);
            this.lblLifestyleStartingNuyen.TabIndex = 89;
            this.lblLifestyleStartingNuyen.Text = "[Starting Nuyen]";
            // 
            // flpLifestyleIntervals
            // 
            this.flpLifestyleIntervals.AutoSize = true;
            this.flpLifestyleIntervals.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpLifestyleCommon.SetColumnSpan(this.flpLifestyleIntervals, 2);
            this.flpLifestyleIntervals.Controls.Add(this.nudLifestyleMonths);
            this.flpLifestyleIntervals.Controls.Add(this.lblLifestyleMonthsLabel);
            this.flpLifestyleIntervals.Controls.Add(this.lblLifestyleTotalCost);
            this.flpLifestyleIntervals.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flpLifestyleIntervals.Location = new System.Drawing.Point(0, 25);
            this.flpLifestyleIntervals.Margin = new System.Windows.Forms.Padding(0);
            this.flpLifestyleIntervals.Name = "flpLifestyleIntervals";
            this.flpLifestyleIntervals.Size = new System.Drawing.Size(238, 26);
            this.flpLifestyleIntervals.TabIndex = 106;
            this.flpLifestyleIntervals.WrapContents = false;
            // 
            // nudLifestyleMonths
            // 
            this.nudLifestyleMonths.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.nudLifestyleMonths.AutoSize = true;
            this.nudLifestyleMonths.Location = new System.Drawing.Point(3, 3);
            this.nudLifestyleMonths.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudLifestyleMonths.Name = "nudLifestyleMonths";
            this.nudLifestyleMonths.Size = new System.Drawing.Size(41, 20);
            this.nudLifestyleMonths.TabIndex = 82;
            this.nudLifestyleMonths.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudLifestyleMonths.ValueChanged += new System.EventHandler(this.nudLifestyleMonths_ValueChanged);
            // 
            // lblLifestyleMonthsLabel
            // 
            this.lblLifestyleMonthsLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblLifestyleMonthsLabel.AutoSize = true;
            this.lblLifestyleMonthsLabel.Location = new System.Drawing.Point(50, 6);
            this.lblLifestyleMonthsLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblLifestyleMonthsLabel.Name = "lblLifestyleMonthsLabel";
            this.lblLifestyleMonthsLabel.Size = new System.Drawing.Size(142, 13);
            this.lblLifestyleMonthsLabel.TabIndex = 83;
            this.lblLifestyleMonthsLabel.Text = "[Intrvl] ([Num] for Permanent)";
            this.lblLifestyleMonthsLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblLifestyleTotalCost
            // 
            this.lblLifestyleTotalCost.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblLifestyleTotalCost.AutoSize = true;
            this.lblLifestyleTotalCost.Location = new System.Drawing.Point(198, 6);
            this.lblLifestyleTotalCost.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblLifestyleTotalCost.Name = "lblLifestyleTotalCost";
            this.lblLifestyleTotalCost.Size = new System.Drawing.Size(37, 13);
            this.lblLifestyleTotalCost.TabIndex = 86;
            this.lblLifestyleTotalCost.Text = "[Total]";
            this.lblLifestyleTotalCost.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblLifestyleCost
            // 
            this.lblLifestyleCost.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblLifestyleCost.AutoSize = true;
            this.lblLifestyleCost.Location = new System.Drawing.Point(89, 6);
            this.lblLifestyleCost.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblLifestyleCost.MaximumSize = new System.Drawing.Size(427, 0);
            this.lblLifestyleCost.Name = "lblLifestyleCost";
            this.lblLifestyleCost.Size = new System.Drawing.Size(34, 13);
            this.lblLifestyleCost.TabIndex = 84;
            this.lblLifestyleCost.Text = "[Cost]";
            // 
            // lblBaseLifestyle
            // 
            this.lblBaseLifestyle.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblBaseLifestyle.AutoSize = true;
            this.lblBaseLifestyle.Location = new System.Drawing.Point(89, 82);
            this.lblBaseLifestyle.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblBaseLifestyle.Name = "lblBaseLifestyle";
            this.lblBaseLifestyle.Size = new System.Drawing.Size(19, 13);
            this.lblBaseLifestyle.TabIndex = 93;
            this.lblBaseLifestyle.Text = "[0]";
            // 
            // lblLifestyleSourceLabel
            // 
            this.lblLifestyleSourceLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblLifestyleSourceLabel.AutoSize = true;
            this.lblLifestyleSourceLabel.Location = new System.Drawing.Point(39, 57);
            this.lblLifestyleSourceLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblLifestyleSourceLabel.Name = "lblLifestyleSourceLabel";
            this.lblLifestyleSourceLabel.Size = new System.Drawing.Size(44, 13);
            this.lblLifestyleSourceLabel.TabIndex = 87;
            this.lblLifestyleSourceLabel.Tag = "Label_Source";
            this.lblLifestyleSourceLabel.Text = "Source:";
            // 
            // lblLifestyleSource
            // 
            this.lblLifestyleSource.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblLifestyleSource.AutoSize = true;
            this.lblLifestyleSource.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblLifestyleSource.Location = new System.Drawing.Point(89, 57);
            this.lblLifestyleSource.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblLifestyleSource.Name = "lblLifestyleSource";
            this.lblLifestyleSource.Size = new System.Drawing.Size(47, 13);
            this.lblLifestyleSource.TabIndex = 88;
            this.lblLifestyleSource.Text = "[Source]";
            this.lblLifestyleSource.Click += new System.EventHandler(this.OpenSourceFromLabel);
            // 
            // lblLifestyleComfortsLabel
            // 
            this.lblLifestyleComfortsLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblLifestyleComfortsLabel.AutoSize = true;
            this.lblLifestyleComfortsLabel.Location = new System.Drawing.Point(35, 82);
            this.lblLifestyleComfortsLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblLifestyleComfortsLabel.Name = "lblLifestyleComfortsLabel";
            this.lblLifestyleComfortsLabel.Size = new System.Drawing.Size(48, 13);
            this.lblLifestyleComfortsLabel.TabIndex = 92;
            this.lblLifestyleComfortsLabel.Tag = "Label_SelectAdvancedLifestyle_Lifestyle";
            this.lblLifestyleComfortsLabel.Text = "Lifestyle:";
            // 
            // lblLifestyleQualitiesLabel
            // 
            this.lblLifestyleQualitiesLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblLifestyleQualitiesLabel.AutoSize = true;
            this.lblLifestyleQualitiesLabel.Location = new System.Drawing.Point(33, 132);
            this.lblLifestyleQualitiesLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblLifestyleQualitiesLabel.Name = "lblLifestyleQualitiesLabel";
            this.lblLifestyleQualitiesLabel.Size = new System.Drawing.Size(50, 13);
            this.lblLifestyleQualitiesLabel.TabIndex = 102;
            this.lblLifestyleQualitiesLabel.Tag = "Label_LifestyleQualities";
            this.lblLifestyleQualitiesLabel.Text = "Qualities:";
            // 
            // lblLifestyleStartingNuyenLabel
            // 
            this.lblLifestyleStartingNuyenLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblLifestyleStartingNuyenLabel.AutoSize = true;
            this.lblLifestyleStartingNuyenLabel.Location = new System.Drawing.Point(3, 107);
            this.lblLifestyleStartingNuyenLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblLifestyleStartingNuyenLabel.Name = "lblLifestyleStartingNuyenLabel";
            this.lblLifestyleStartingNuyenLabel.Size = new System.Drawing.Size(80, 13);
            this.lblLifestyleStartingNuyenLabel.TabIndex = 90;
            this.lblLifestyleStartingNuyenLabel.Tag = "Label_SelectLifestyle_StartingNuyen";
            this.lblLifestyleStartingNuyenLabel.Text = "Starting Nuyen:";
            // 
            // tlpLifestyleButtons
            // 
            this.tlpLifestyleButtons.AutoSize = true;
            this.tlpLifestyleButtons.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpLifestyleButtons.ColumnCount = 2;
            this.tlpLifestyleDetails.SetColumnSpan(this.tlpLifestyleButtons, 2);
            this.tlpLifestyleButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpLifestyleButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpLifestyleButtons.Controls.Add(this.cmdAddLifestyle, 0, 0);
            this.tlpLifestyleButtons.Controls.Add(this.cmdDeleteLifestyle, 1, 0);
            this.tlpLifestyleButtons.Location = new System.Drawing.Point(0, 0);
            this.tlpLifestyleButtons.Margin = new System.Windows.Forms.Padding(0);
            this.tlpLifestyleButtons.Name = "tlpLifestyleButtons";
            this.tlpLifestyleButtons.RowCount = 1;
            this.tlpLifestyleButtons.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpLifestyleButtons.Size = new System.Drawing.Size(202, 29);
            this.tlpLifestyleButtons.TabIndex = 106;
            // 
            // cmdAddLifestyle
            // 
            this.cmdAddLifestyle.AutoSize = true;
            this.cmdAddLifestyle.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdAddLifestyle.ContextMenuStrip = this.cmsLifestyle;
            this.cmdAddLifestyle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmdAddLifestyle.Location = new System.Drawing.Point(3, 3);
            this.cmdAddLifestyle.Name = "cmdAddLifestyle";
            this.cmdAddLifestyle.Size = new System.Drawing.Size(95, 23);
            this.cmdAddLifestyle.SplitMenuStrip = this.cmsLifestyle;
            this.cmdAddLifestyle.TabIndex = 91;
            this.cmdAddLifestyle.Tag = "Button_AddLifestyle";
            this.cmdAddLifestyle.Text = "&Add Lifestyle";
            this.cmdAddLifestyle.UseVisualStyleBackColor = true;
            this.cmdAddLifestyle.Click += new System.EventHandler(this.cmdAddLifestyle_Click);
            // 
            // cmdDeleteLifestyle
            // 
            this.cmdDeleteLifestyle.AutoSize = true;
            this.cmdDeleteLifestyle.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdDeleteLifestyle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmdDeleteLifestyle.Location = new System.Drawing.Point(104, 3);
            this.cmdDeleteLifestyle.Name = "cmdDeleteLifestyle";
            this.cmdDeleteLifestyle.Size = new System.Drawing.Size(95, 23);
            this.cmdDeleteLifestyle.TabIndex = 81;
            this.cmdDeleteLifestyle.Tag = "String_Delete";
            this.cmdDeleteLifestyle.Text = "Delete";
            this.cmdDeleteLifestyle.UseVisualStyleBackColor = true;
            this.cmdDeleteLifestyle.Click += new System.EventHandler(this.cmdDeleteLifestyle_Click);
            // 
            // tabVehicles
            // 
            this.tabVehicles.BackColor = System.Drawing.SystemColors.Control;
            this.tabVehicles.Controls.Add(this.tlpVehicles);
            this.tabVehicles.Location = new System.Drawing.Point(4, 22);
            this.tabVehicles.Name = "tabVehicles";
            this.tabVehicles.Padding = new System.Windows.Forms.Padding(3);
            this.tabVehicles.Size = new System.Drawing.Size(977, 631);
            this.tabVehicles.TabIndex = 7;
            this.tabVehicles.Tag = "Tab_Vehicles";
            this.tabVehicles.Text = "Vehicles & Drones";
            // 
            // tlpVehicles
            // 
            this.tlpVehicles.AutoSize = true;
            this.tlpVehicles.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpVehicles.ColumnCount = 2;
            this.tlpVehicles.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpVehicles.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpVehicles.Controls.Add(this.flpVehiclesButtons, 0, 0);
            this.tlpVehicles.Controls.Add(this.flpVehicles, 1, 1);
            this.tlpVehicles.Controls.Add(this.treVehicles, 0, 1);
            this.tlpVehicles.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpVehicles.Location = new System.Drawing.Point(3, 3);
            this.tlpVehicles.Name = "tlpVehicles";
            this.tlpVehicles.RowCount = 2;
            this.tlpVehicles.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpVehicles.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpVehicles.Size = new System.Drawing.Size(971, 625);
            this.tlpVehicles.TabIndex = 245;
            // 
            // flpVehiclesButtons
            // 
            this.flpVehiclesButtons.AutoSize = true;
            this.flpVehiclesButtons.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpVehicles.SetColumnSpan(this.flpVehiclesButtons, 2);
            this.flpVehiclesButtons.Controls.Add(this.cmdAddVehicle);
            this.flpVehiclesButtons.Controls.Add(this.cmdDeleteVehicle);
            this.flpVehiclesButtons.Controls.Add(this.cmdAddVehicleLocation);
            this.flpVehiclesButtons.Location = new System.Drawing.Point(0, 0);
            this.flpVehiclesButtons.Margin = new System.Windows.Forms.Padding(0);
            this.flpVehiclesButtons.Name = "flpVehiclesButtons";
            this.flpVehiclesButtons.Size = new System.Drawing.Size(270, 29);
            this.flpVehiclesButtons.TabIndex = 0;
            this.flpVehiclesButtons.WrapContents = false;
            // 
            // cmdAddVehicle
            // 
            this.cmdAddVehicle.AutoSize = true;
            this.cmdAddVehicle.ContextMenuStrip = this.cmsVehicle;
            this.cmdAddVehicle.Location = new System.Drawing.Point(3, 3);
            this.cmdAddVehicle.Name = "cmdAddVehicle";
            this.cmdAddVehicle.Size = new System.Drawing.Size(92, 23);
            this.cmdAddVehicle.SplitMenuStrip = this.cmsVehicle;
            this.cmdAddVehicle.TabIndex = 185;
            this.cmdAddVehicle.Tag = "Button_AddVehicle";
            this.cmdAddVehicle.Text = "&Add Vehicle";
            this.cmdAddVehicle.UseVisualStyleBackColor = true;
            this.cmdAddVehicle.Click += new System.EventHandler(this.cmdAddVehicle_Click);
            // 
            // cmdDeleteVehicle
            // 
            this.cmdDeleteVehicle.AutoSize = true;
            this.cmdDeleteVehicle.Location = new System.Drawing.Point(101, 3);
            this.cmdDeleteVehicle.Name = "cmdDeleteVehicle";
            this.cmdDeleteVehicle.Size = new System.Drawing.Size(80, 23);
            this.cmdDeleteVehicle.TabIndex = 32;
            this.cmdDeleteVehicle.Tag = "String_Delete";
            this.cmdDeleteVehicle.Text = "Delete";
            this.cmdDeleteVehicle.UseVisualStyleBackColor = true;
            this.cmdDeleteVehicle.Click += new System.EventHandler(this.cmdDeleteVehicle_Click);
            // 
            // cmdAddVehicleLocation
            // 
            this.cmdAddVehicleLocation.AutoSize = true;
            this.cmdAddVehicleLocation.Location = new System.Drawing.Point(187, 3);
            this.cmdAddVehicleLocation.Name = "cmdAddVehicleLocation";
            this.cmdAddVehicleLocation.Size = new System.Drawing.Size(80, 23);
            this.cmdAddVehicleLocation.TabIndex = 128;
            this.cmdAddVehicleLocation.Tag = "Button_AddLocation";
            this.cmdAddVehicleLocation.Text = "Add Location";
            this.cmdAddVehicleLocation.UseVisualStyleBackColor = true;
            this.cmdAddVehicleLocation.Click += new System.EventHandler(this.cmdAddVehicleLocation_Click);
            // 
            // flpVehicles
            // 
            this.flpVehicles.AutoScroll = true;
            this.flpVehicles.Controls.Add(this.gpbVehiclesCommon);
            this.flpVehicles.Controls.Add(this.gpbVehiclesVehicle);
            this.flpVehicles.Controls.Add(this.gpbVehiclesWeapon);
            this.flpVehicles.Controls.Add(this.gpbVehiclesMatrix);
            this.flpVehicles.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flpVehicles.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flpVehicles.Location = new System.Drawing.Point(301, 29);
            this.flpVehicles.Margin = new System.Windows.Forms.Padding(0);
            this.flpVehicles.Name = "flpVehicles";
            this.flpVehicles.Size = new System.Drawing.Size(670, 596);
            this.flpVehicles.TabIndex = 246;
            this.flpVehicles.WrapContents = false;
            // 
            // gpbVehiclesCommon
            // 
            this.gpbVehiclesCommon.AutoSize = true;
            this.gpbVehiclesCommon.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.gpbVehiclesCommon.Controls.Add(this.tlpVehiclesCommon);
            this.gpbVehiclesCommon.Dock = System.Windows.Forms.DockStyle.Top;
            this.gpbVehiclesCommon.Location = new System.Drawing.Point(3, 3);
            this.gpbVehiclesCommon.Name = "gpbVehiclesCommon";
            this.gpbVehiclesCommon.Size = new System.Drawing.Size(501, 199);
            this.gpbVehiclesCommon.TabIndex = 3;
            this.gpbVehiclesCommon.TabStop = false;
            this.gpbVehiclesCommon.Tag = "String_Info";
            this.gpbVehiclesCommon.Text = "Info";
            // 
            // tlpVehiclesCommon
            // 
            this.tlpVehiclesCommon.AutoSize = true;
            this.tlpVehiclesCommon.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpVehiclesCommon.ColumnCount = 4;
            this.tlpVehiclesCommon.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpVehiclesCommon.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpVehiclesCommon.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpVehiclesCommon.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpVehiclesCommon.Controls.Add(this.lblVehicleNameLabel, 0, 0);
            this.tlpVehiclesCommon.Controls.Add(this.lblVehicleAvailLabel, 2, 0);
            this.tlpVehiclesCommon.Controls.Add(this.lblVehicleCostLabel, 2, 1);
            this.tlpVehiclesCommon.Controls.Add(this.lblVehicleAvail, 3, 0);
            this.tlpVehiclesCommon.Controls.Add(this.lblVehicleSlots, 3, 2);
            this.tlpVehiclesCommon.Controls.Add(this.lblVehicleCost, 3, 1);
            this.tlpVehiclesCommon.Controls.Add(this.lblVehicleSource, 1, 4);
            this.tlpVehiclesCommon.Controls.Add(this.lblVehicleSlotsLabel, 2, 2);
            this.tlpVehiclesCommon.Controls.Add(this.lblVehicleName, 1, 0);
            this.tlpVehiclesCommon.Controls.Add(this.lblVehicleSourceLabel, 0, 4);
            this.tlpVehiclesCommon.Controls.Add(this.lblVehicleCategory, 1, 1);
            this.tlpVehiclesCommon.Controls.Add(this.nudVehicleGearQty, 1, 3);
            this.tlpVehiclesCommon.Controls.Add(this.lblVehicleCategoryLabel, 0, 1);
            this.tlpVehiclesCommon.Controls.Add(this.lblVehicleRatingLabel, 0, 2);
            this.tlpVehiclesCommon.Controls.Add(this.lblVehicleGearQtyLabel, 0, 3);
            this.tlpVehiclesCommon.Controls.Add(this.nudVehicleRating, 1, 2);
            this.tlpVehiclesCommon.Controls.Add(this.cmdVehicleCyberwareChangeMount, 2, 3);
            this.tlpVehiclesCommon.Controls.Add(this.flpVehiclesCommonCheckBoxes, 2, 4);
            this.tlpVehiclesCommon.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpVehiclesCommon.Location = new System.Drawing.Point(3, 16);
            this.tlpVehiclesCommon.Name = "tlpVehiclesCommon";
            this.tlpVehiclesCommon.RowCount = 5;
            this.tlpVehiclesCommon.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpVehiclesCommon.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpVehiclesCommon.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpVehiclesCommon.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpVehiclesCommon.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpVehiclesCommon.Size = new System.Drawing.Size(495, 180);
            this.tlpVehiclesCommon.TabIndex = 0;
            // 
            // lblVehicleNameLabel
            // 
            this.lblVehicleNameLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblVehicleNameLabel.AutoSize = true;
            this.lblVehicleNameLabel.Location = new System.Drawing.Point(17, 6);
            this.lblVehicleNameLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblVehicleNameLabel.Name = "lblVehicleNameLabel";
            this.lblVehicleNameLabel.Size = new System.Drawing.Size(38, 13);
            this.lblVehicleNameLabel.TabIndex = 51;
            this.lblVehicleNameLabel.Tag = "Label_Name";
            this.lblVehicleNameLabel.Text = "Name:";
            // 
            // lblVehicleAvailLabel
            // 
            this.lblVehicleAvailLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblVehicleAvailLabel.AutoSize = true;
            this.lblVehicleAvailLabel.Location = new System.Drawing.Point(260, 6);
            this.lblVehicleAvailLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblVehicleAvailLabel.Name = "lblVehicleAvailLabel";
            this.lblVehicleAvailLabel.Size = new System.Drawing.Size(33, 13);
            this.lblVehicleAvailLabel.TabIndex = 47;
            this.lblVehicleAvailLabel.Tag = "Label_Avail";
            this.lblVehicleAvailLabel.Text = "Avail:";
            // 
            // lblVehicleCostLabel
            // 
            this.lblVehicleCostLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblVehicleCostLabel.AutoSize = true;
            this.lblVehicleCostLabel.Location = new System.Drawing.Point(262, 31);
            this.lblVehicleCostLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblVehicleCostLabel.Name = "lblVehicleCostLabel";
            this.lblVehicleCostLabel.Size = new System.Drawing.Size(31, 13);
            this.lblVehicleCostLabel.TabIndex = 49;
            this.lblVehicleCostLabel.Tag = "Label_Cost";
            this.lblVehicleCostLabel.Text = "Cost:";
            // 
            // lblVehicleAvail
            // 
            this.lblVehicleAvail.AutoSize = true;
            this.lblVehicleAvail.Location = new System.Drawing.Point(299, 6);
            this.lblVehicleAvail.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblVehicleAvail.Name = "lblVehicleAvail";
            this.lblVehicleAvail.Size = new System.Drawing.Size(36, 13);
            this.lblVehicleAvail.TabIndex = 48;
            this.lblVehicleAvail.Text = "[Avail]";
            // 
            // lblVehicleSlots
            // 
            this.lblVehicleSlots.AutoSize = true;
            this.lblVehicleSlots.Location = new System.Drawing.Point(299, 56);
            this.lblVehicleSlots.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblVehicleSlots.Name = "lblVehicleSlots";
            this.lblVehicleSlots.Size = new System.Drawing.Size(36, 13);
            this.lblVehicleSlots.TabIndex = 58;
            this.lblVehicleSlots.Text = "[Slots]";
            this.lblVehicleSlots.Visible = false;
            // 
            // lblVehicleCost
            // 
            this.lblVehicleCost.AutoSize = true;
            this.lblVehicleCost.Location = new System.Drawing.Point(299, 31);
            this.lblVehicleCost.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblVehicleCost.Name = "lblVehicleCost";
            this.lblVehicleCost.Size = new System.Drawing.Size(34, 13);
            this.lblVehicleCost.TabIndex = 50;
            this.lblVehicleCost.Text = "[Cost]";
            // 
            // lblVehicleSource
            // 
            this.lblVehicleSource.AutoSize = true;
            this.lblVehicleSource.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblVehicleSource.Location = new System.Drawing.Point(61, 111);
            this.lblVehicleSource.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblVehicleSource.Name = "lblVehicleSource";
            this.lblVehicleSource.Size = new System.Drawing.Size(47, 13);
            this.lblVehicleSource.TabIndex = 60;
            this.lblVehicleSource.Text = "[Source]";
            this.lblVehicleSource.Click += new System.EventHandler(this.OpenSourceFromLabel);
            // 
            // lblVehicleSlotsLabel
            // 
            this.lblVehicleSlotsLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblVehicleSlotsLabel.AutoSize = true;
            this.lblVehicleSlotsLabel.Location = new System.Drawing.Point(260, 56);
            this.lblVehicleSlotsLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblVehicleSlotsLabel.Name = "lblVehicleSlotsLabel";
            this.lblVehicleSlotsLabel.Size = new System.Drawing.Size(33, 13);
            this.lblVehicleSlotsLabel.TabIndex = 57;
            this.lblVehicleSlotsLabel.Tag = "Label_Slots";
            this.lblVehicleSlotsLabel.Text = "Slots:";
            this.lblVehicleSlotsLabel.Visible = false;
            // 
            // lblVehicleName
            // 
            this.lblVehicleName.AutoSize = true;
            this.lblVehicleName.Location = new System.Drawing.Point(61, 6);
            this.lblVehicleName.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblVehicleName.Name = "lblVehicleName";
            this.lblVehicleName.Size = new System.Drawing.Size(41, 13);
            this.lblVehicleName.TabIndex = 52;
            this.lblVehicleName.Text = "[Name]";
            // 
            // lblVehicleSourceLabel
            // 
            this.lblVehicleSourceLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblVehicleSourceLabel.AutoSize = true;
            this.lblVehicleSourceLabel.Location = new System.Drawing.Point(11, 111);
            this.lblVehicleSourceLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblVehicleSourceLabel.Name = "lblVehicleSourceLabel";
            this.lblVehicleSourceLabel.Size = new System.Drawing.Size(44, 13);
            this.lblVehicleSourceLabel.TabIndex = 59;
            this.lblVehicleSourceLabel.Tag = "Label_Source";
            this.lblVehicleSourceLabel.Text = "Source:";
            // 
            // lblVehicleCategory
            // 
            this.lblVehicleCategory.AutoSize = true;
            this.lblVehicleCategory.Location = new System.Drawing.Point(61, 31);
            this.lblVehicleCategory.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblVehicleCategory.Name = "lblVehicleCategory";
            this.lblVehicleCategory.Size = new System.Drawing.Size(55, 13);
            this.lblVehicleCategory.TabIndex = 54;
            this.lblVehicleCategory.Text = "[Category]";
            // 
            // nudVehicleGearQty
            // 
            this.nudVehicleGearQty.AutoSize = true;
            this.nudVehicleGearQty.Enabled = false;
            this.nudVehicleGearQty.Location = new System.Drawing.Point(61, 79);
            this.nudVehicleGearQty.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.nudVehicleGearQty.Name = "nudVehicleGearQty";
            this.nudVehicleGearQty.Size = new System.Drawing.Size(59, 20);
            this.nudVehicleGearQty.TabIndex = 79;
            this.nudVehicleGearQty.ValueChanged += new System.EventHandler(this.nudVehicleGearQty_ValueChanged);
            // 
            // lblVehicleCategoryLabel
            // 
            this.lblVehicleCategoryLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblVehicleCategoryLabel.AutoSize = true;
            this.lblVehicleCategoryLabel.Location = new System.Drawing.Point(3, 31);
            this.lblVehicleCategoryLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblVehicleCategoryLabel.Name = "lblVehicleCategoryLabel";
            this.lblVehicleCategoryLabel.Size = new System.Drawing.Size(52, 13);
            this.lblVehicleCategoryLabel.TabIndex = 53;
            this.lblVehicleCategoryLabel.Tag = "Label_Category";
            this.lblVehicleCategoryLabel.Text = "Category:";
            // 
            // lblVehicleRatingLabel
            // 
            this.lblVehicleRatingLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblVehicleRatingLabel.AutoSize = true;
            this.lblVehicleRatingLabel.Location = new System.Drawing.Point(14, 56);
            this.lblVehicleRatingLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblVehicleRatingLabel.Name = "lblVehicleRatingLabel";
            this.lblVehicleRatingLabel.Size = new System.Drawing.Size(41, 13);
            this.lblVehicleRatingLabel.TabIndex = 55;
            this.lblVehicleRatingLabel.Tag = "Label_Rating";
            this.lblVehicleRatingLabel.Text = "Rating:";
            // 
            // lblVehicleGearQtyLabel
            // 
            this.lblVehicleGearQtyLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblVehicleGearQtyLabel.AutoSize = true;
            this.lblVehicleGearQtyLabel.Location = new System.Drawing.Point(3, 82);
            this.lblVehicleGearQtyLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblVehicleGearQtyLabel.Name = "lblVehicleGearQtyLabel";
            this.lblVehicleGearQtyLabel.Size = new System.Drawing.Size(52, 13);
            this.lblVehicleGearQtyLabel.TabIndex = 78;
            this.lblVehicleGearQtyLabel.Tag = "Label_GearQty";
            this.lblVehicleGearQtyLabel.Text = "Gear Qty:";
            // 
            // nudVehicleRating
            // 
            this.nudVehicleRating.AutoSize = true;
            this.nudVehicleRating.Enabled = false;
            this.nudVehicleRating.Location = new System.Drawing.Point(61, 53);
            this.nudVehicleRating.Maximum = new decimal(new int[] {
            6,
            0,
            0,
            0});
            this.nudVehicleRating.Name = "nudVehicleRating";
            this.nudVehicleRating.Size = new System.Drawing.Size(29, 20);
            this.nudVehicleRating.TabIndex = 56;
            this.nudVehicleRating.ValueChanged += new System.EventHandler(this.nudVehicleRating_ValueChanged);
            // 
            // cmdVehicleCyberwareChangeMount
            // 
            this.cmdVehicleCyberwareChangeMount.AutoSize = true;
            this.cmdVehicleCyberwareChangeMount.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpVehiclesCommon.SetColumnSpan(this.cmdVehicleCyberwareChangeMount, 2);
            this.cmdVehicleCyberwareChangeMount.Location = new System.Drawing.Point(260, 79);
            this.cmdVehicleCyberwareChangeMount.Name = "cmdVehicleCyberwareChangeMount";
            this.cmdVehicleCyberwareChangeMount.Size = new System.Drawing.Size(143, 23);
            this.cmdVehicleCyberwareChangeMount.TabIndex = 241;
            this.cmdVehicleCyberwareChangeMount.Tag = "Button_ChangeMountedLocation";
            this.cmdVehicleCyberwareChangeMount.Text = "Change Mounted Location";
            this.cmdVehicleCyberwareChangeMount.UseVisualStyleBackColor = true;
            this.cmdVehicleCyberwareChangeMount.Visible = false;
            this.cmdVehicleCyberwareChangeMount.Click += new System.EventHandler(this.cmdVehicleCyberwareChangeMount_Click);
            // 
            // flpVehiclesCommonCheckBoxes
            // 
            this.flpVehiclesCommonCheckBoxes.AutoSize = true;
            this.flpVehiclesCommonCheckBoxes.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpVehiclesCommon.SetColumnSpan(this.flpVehiclesCommonCheckBoxes, 2);
            this.flpVehiclesCommonCheckBoxes.Controls.Add(this.chkVehicleWeaponAccessoryInstalled);
            this.flpVehiclesCommonCheckBoxes.Controls.Add(this.chkVehicleIncludedInWeapon);
            this.flpVehiclesCommonCheckBoxes.Controls.Add(this.chkVehicleStolen);
            this.flpVehiclesCommonCheckBoxes.Dock = System.Windows.Forms.DockStyle.Top;
            this.flpVehiclesCommonCheckBoxes.Location = new System.Drawing.Point(257, 105);
            this.flpVehiclesCommonCheckBoxes.Margin = new System.Windows.Forms.Padding(0);
            this.flpVehiclesCommonCheckBoxes.Name = "flpVehiclesCommonCheckBoxes";
            this.flpVehiclesCommonCheckBoxes.Size = new System.Drawing.Size(238, 50);
            this.flpVehiclesCommonCheckBoxes.TabIndex = 242;
            // 
            // chkVehicleWeaponAccessoryInstalled
            // 
            this.chkVehicleWeaponAccessoryInstalled.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkVehicleWeaponAccessoryInstalled.AutoSize = true;
            this.chkVehicleWeaponAccessoryInstalled.Enabled = false;
            this.chkVehicleWeaponAccessoryInstalled.Location = new System.Drawing.Point(3, 4);
            this.chkVehicleWeaponAccessoryInstalled.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkVehicleWeaponAccessoryInstalled.Name = "chkVehicleWeaponAccessoryInstalled";
            this.chkVehicleWeaponAccessoryInstalled.Size = new System.Drawing.Size(65, 17);
            this.chkVehicleWeaponAccessoryInstalled.TabIndex = 74;
            this.chkVehicleWeaponAccessoryInstalled.Tag = "Checkbox_Installed";
            this.chkVehicleWeaponAccessoryInstalled.Text = "Installed";
            this.chkVehicleWeaponAccessoryInstalled.UseVisualStyleBackColor = true;
            this.chkVehicleWeaponAccessoryInstalled.CheckedChanged += new System.EventHandler(this.chkVehicleWeaponAccessoryInstalled_CheckedChanged);
            // 
            // chkVehicleIncludedInWeapon
            // 
            this.chkVehicleIncludedInWeapon.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkVehicleIncludedInWeapon.AutoSize = true;
            this.chkVehicleIncludedInWeapon.CheckAlign = System.Drawing.ContentAlignment.TopLeft;
            this.chkVehicleIncludedInWeapon.Enabled = false;
            this.chkVehicleIncludedInWeapon.Location = new System.Drawing.Point(74, 4);
            this.chkVehicleIncludedInWeapon.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkVehicleIncludedInWeapon.Name = "chkVehicleIncludedInWeapon";
            this.chkVehicleIncludedInWeapon.Size = new System.Drawing.Size(127, 17);
            this.chkVehicleIncludedInWeapon.TabIndex = 75;
            this.chkVehicleIncludedInWeapon.Tag = "Checkbox_BaseWeapon";
            this.chkVehicleIncludedInWeapon.Text = "Part of base Weapon";
            this.chkVehicleIncludedInWeapon.UseVisualStyleBackColor = true;
            // 
            // chkVehicleStolen
            // 
            this.chkVehicleStolen.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkVehicleStolen.AutoSize = true;
            this.chkVehicleStolen.Enabled = false;
            this.chkVehicleStolen.Location = new System.Drawing.Point(3, 29);
            this.chkVehicleStolen.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkVehicleStolen.Name = "chkVehicleStolen";
            this.chkVehicleStolen.Size = new System.Drawing.Size(56, 17);
            this.chkVehicleStolen.TabIndex = 76;
            this.chkVehicleStolen.Tag = "Checkbox_Stolen";
            this.chkVehicleStolen.Text = "Stolen";
            this.chkVehicleStolen.UseVisualStyleBackColor = true;
            this.chkVehicleStolen.CheckedChanged += new System.EventHandler(this.chkVehicleStolen_CheckedChanged);
            // 
            // gpbVehiclesVehicle
            // 
            this.gpbVehiclesVehicle.AutoSize = true;
            this.gpbVehiclesVehicle.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.gpbVehiclesVehicle.Controls.Add(this.tlpVehiclesVehicle);
            this.gpbVehiclesVehicle.Dock = System.Windows.Forms.DockStyle.Top;
            this.gpbVehiclesVehicle.Location = new System.Drawing.Point(3, 208);
            this.gpbVehiclesVehicle.Name = "gpbVehiclesVehicle";
            this.gpbVehiclesVehicle.Size = new System.Drawing.Size(501, 119);
            this.gpbVehiclesVehicle.TabIndex = 2;
            this.gpbVehiclesVehicle.TabStop = false;
            this.gpbVehiclesVehicle.Tag = "String_Vehicle";
            this.gpbVehiclesVehicle.Text = "Vehicle";
            // 
            // tlpVehiclesVehicle
            // 
            this.tlpVehiclesVehicle.AutoSize = true;
            this.tlpVehiclesVehicle.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpVehiclesVehicle.ColumnCount = 8;
            this.tlpVehiclesVehicle.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpVehiclesVehicle.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tlpVehiclesVehicle.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpVehiclesVehicle.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tlpVehiclesVehicle.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpVehiclesVehicle.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tlpVehiclesVehicle.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpVehiclesVehicle.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tlpVehiclesVehicle.Controls.Add(this.lblVehicleHandlingLabel, 0, 0);
            this.tlpVehiclesVehicle.Controls.Add(this.lblVehicleHandling, 1, 0);
            this.tlpVehiclesVehicle.Controls.Add(this.lblVehicleAccelLabel, 2, 0);
            this.tlpVehiclesVehicle.Controls.Add(this.lblVehicleAccel, 3, 0);
            this.tlpVehiclesVehicle.Controls.Add(this.lblVehicleSpeedLabel, 4, 0);
            this.tlpVehiclesVehicle.Controls.Add(this.lblVehicleSpeed, 5, 0);
            this.tlpVehiclesVehicle.Controls.Add(this.lblVehicleCosmetic, 7, 3);
            this.tlpVehiclesVehicle.Controls.Add(this.lblVehicleCosmeticLabel, 6, 3);
            this.tlpVehiclesVehicle.Controls.Add(this.lblVehiclePilotLabel, 6, 0);
            this.tlpVehiclesVehicle.Controls.Add(this.lblVehiclePilot, 7, 0);
            this.tlpVehiclesVehicle.Controls.Add(this.lblVehicleDroneModSlots, 1, 2);
            this.tlpVehiclesVehicle.Controls.Add(this.lblVehicleBodyLabel, 0, 1);
            this.tlpVehiclesVehicle.Controls.Add(this.lblVehicleBody, 1, 1);
            this.tlpVehiclesVehicle.Controls.Add(this.lblVehicleWeaponsmodLabel, 6, 2);
            this.tlpVehiclesVehicle.Controls.Add(this.lblVehicleDroneModSlotsLabel, 0, 2);
            this.tlpVehiclesVehicle.Controls.Add(this.lblVehicleSeats, 5, 1);
            this.tlpVehiclesVehicle.Controls.Add(this.lblVehicleBodymodLabel, 0, 3);
            this.tlpVehiclesVehicle.Controls.Add(this.lblVehicleArmorLabel, 2, 1);
            this.tlpVehiclesVehicle.Controls.Add(this.lblVehicleArmor, 3, 1);
            this.tlpVehiclesVehicle.Controls.Add(this.lblVehicleSeatsLabel, 4, 1);
            this.tlpVehiclesVehicle.Controls.Add(this.lblVehicleSensorLabel, 6, 1);
            this.tlpVehiclesVehicle.Controls.Add(this.lblVehicleSensor, 7, 1);
            this.tlpVehiclesVehicle.Controls.Add(this.lblVehiclePowertrainLabel, 2, 2);
            this.tlpVehiclesVehicle.Controls.Add(this.lblVehiclePowertrain, 3, 2);
            this.tlpVehiclesVehicle.Controls.Add(this.lblVehicleWeaponsmod, 7, 2);
            this.tlpVehiclesVehicle.Controls.Add(this.lblVehicleElectromagnetic, 5, 3);
            this.tlpVehiclesVehicle.Controls.Add(this.lblVehicleElectromagneticLabel, 4, 3);
            this.tlpVehiclesVehicle.Controls.Add(this.lblVehicleBodymod, 3, 3);
            this.tlpVehiclesVehicle.Controls.Add(this.lblVehicleProtectionLabel, 4, 2);
            this.tlpVehiclesVehicle.Controls.Add(this.lblVehicleProtection, 5, 2);
            this.tlpVehiclesVehicle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpVehiclesVehicle.Location = new System.Drawing.Point(3, 16);
            this.tlpVehiclesVehicle.Name = "tlpVehiclesVehicle";
            this.tlpVehiclesVehicle.RowCount = 4;
            this.tlpVehiclesVehicle.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpVehiclesVehicle.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpVehiclesVehicle.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpVehiclesVehicle.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpVehiclesVehicle.Size = new System.Drawing.Size(495, 100);
            this.tlpVehiclesVehicle.TabIndex = 0;
            // 
            // lblVehicleHandlingLabel
            // 
            this.lblVehicleHandlingLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblVehicleHandlingLabel.AutoSize = true;
            this.lblVehicleHandlingLabel.Location = new System.Drawing.Point(8, 6);
            this.lblVehicleHandlingLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblVehicleHandlingLabel.Name = "lblVehicleHandlingLabel";
            this.lblVehicleHandlingLabel.Size = new System.Drawing.Size(52, 13);
            this.lblVehicleHandlingLabel.TabIndex = 33;
            this.lblVehicleHandlingLabel.Tag = "Label_Handling";
            this.lblVehicleHandlingLabel.Text = "Handling:";
            // 
            // lblVehicleHandling
            // 
            this.lblVehicleHandling.AutoSize = true;
            this.lblVehicleHandling.Location = new System.Drawing.Point(66, 6);
            this.lblVehicleHandling.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblVehicleHandling.Name = "lblVehicleHandling";
            this.lblVehicleHandling.Size = new System.Drawing.Size(55, 13);
            this.lblVehicleHandling.TabIndex = 34;
            this.lblVehicleHandling.Text = "[Handling]";
            // 
            // lblVehicleAccelLabel
            // 
            this.lblVehicleAccelLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblVehicleAccelLabel.AutoSize = true;
            this.lblVehicleAccelLabel.Location = new System.Drawing.Point(138, 6);
            this.lblVehicleAccelLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblVehicleAccelLabel.Name = "lblVehicleAccelLabel";
            this.lblVehicleAccelLabel.Size = new System.Drawing.Size(37, 13);
            this.lblVehicleAccelLabel.TabIndex = 35;
            this.lblVehicleAccelLabel.Tag = "Label_Accel";
            this.lblVehicleAccelLabel.Text = "Accel:";
            // 
            // lblVehicleAccel
            // 
            this.lblVehicleAccel.AutoSize = true;
            this.lblVehicleAccel.Location = new System.Drawing.Point(181, 6);
            this.lblVehicleAccel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblVehicleAccel.Name = "lblVehicleAccel";
            this.lblVehicleAccel.Size = new System.Drawing.Size(40, 13);
            this.lblVehicleAccel.TabIndex = 36;
            this.lblVehicleAccel.Text = "[Accel]";
            // 
            // lblVehicleSpeedLabel
            // 
            this.lblVehicleSpeedLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblVehicleSpeedLabel.AutoSize = true;
            this.lblVehicleSpeedLabel.Location = new System.Drawing.Point(250, 6);
            this.lblVehicleSpeedLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblVehicleSpeedLabel.Name = "lblVehicleSpeedLabel";
            this.lblVehicleSpeedLabel.Size = new System.Drawing.Size(41, 13);
            this.lblVehicleSpeedLabel.TabIndex = 37;
            this.lblVehicleSpeedLabel.Tag = "Label_Speed";
            this.lblVehicleSpeedLabel.Text = "Speed:";
            // 
            // lblVehicleSpeed
            // 
            this.lblVehicleSpeed.AutoSize = true;
            this.lblVehicleSpeed.Location = new System.Drawing.Point(297, 6);
            this.lblVehicleSpeed.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblVehicleSpeed.Name = "lblVehicleSpeed";
            this.lblVehicleSpeed.Size = new System.Drawing.Size(44, 13);
            this.lblVehicleSpeed.TabIndex = 38;
            this.lblVehicleSpeed.Text = "[Speed]";
            // 
            // lblVehicleCosmetic
            // 
            this.lblVehicleCosmetic.AutoSize = true;
            this.lblVehicleCosmetic.Location = new System.Drawing.Point(428, 81);
            this.lblVehicleCosmetic.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblVehicleCosmetic.Name = "lblVehicleCosmetic";
            this.lblVehicleCosmetic.Size = new System.Drawing.Size(56, 13);
            this.lblVehicleCosmetic.TabIndex = 208;
            this.lblVehicleCosmetic.Text = "[Cosmetic]";
            // 
            // lblVehicleCosmeticLabel
            // 
            this.lblVehicleCosmeticLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblVehicleCosmeticLabel.AutoSize = true;
            this.lblVehicleCosmeticLabel.Location = new System.Drawing.Point(369, 81);
            this.lblVehicleCosmeticLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblVehicleCosmeticLabel.Name = "lblVehicleCosmeticLabel";
            this.lblVehicleCosmeticLabel.Size = new System.Drawing.Size(53, 13);
            this.lblVehicleCosmeticLabel.TabIndex = 202;
            this.lblVehicleCosmeticLabel.Tag = "Label_Cosmetic";
            this.lblVehicleCosmeticLabel.Text = "Cosmetic:";
            // 
            // lblVehiclePilotLabel
            // 
            this.lblVehiclePilotLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblVehiclePilotLabel.AutoSize = true;
            this.lblVehiclePilotLabel.Location = new System.Drawing.Point(392, 6);
            this.lblVehiclePilotLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblVehiclePilotLabel.Name = "lblVehiclePilotLabel";
            this.lblVehiclePilotLabel.Size = new System.Drawing.Size(30, 13);
            this.lblVehiclePilotLabel.TabIndex = 39;
            this.lblVehiclePilotLabel.Tag = "Label_Pilot";
            this.lblVehiclePilotLabel.Text = "Pilot:";
            // 
            // lblVehiclePilot
            // 
            this.lblVehiclePilot.AutoSize = true;
            this.lblVehiclePilot.Location = new System.Drawing.Point(428, 6);
            this.lblVehiclePilot.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblVehiclePilot.Name = "lblVehiclePilot";
            this.lblVehiclePilot.Size = new System.Drawing.Size(33, 13);
            this.lblVehiclePilot.TabIndex = 40;
            this.lblVehiclePilot.Text = "[Pilot]";
            // 
            // lblVehicleDroneModSlots
            // 
            this.lblVehicleDroneModSlots.AutoSize = true;
            this.lblVehicleDroneModSlots.Location = new System.Drawing.Point(66, 56);
            this.lblVehicleDroneModSlots.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblVehicleDroneModSlots.Name = "lblVehicleDroneModSlots";
            this.lblVehicleDroneModSlots.Size = new System.Drawing.Size(45, 13);
            this.lblVehicleDroneModSlots.TabIndex = 210;
            this.lblVehicleDroneModSlots.Text = "[MSlots]";
            this.lblVehicleDroneModSlots.Visible = false;
            // 
            // lblVehicleBodyLabel
            // 
            this.lblVehicleBodyLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblVehicleBodyLabel.AutoSize = true;
            this.lblVehicleBodyLabel.Location = new System.Drawing.Point(26, 31);
            this.lblVehicleBodyLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblVehicleBodyLabel.Name = "lblVehicleBodyLabel";
            this.lblVehicleBodyLabel.Size = new System.Drawing.Size(34, 13);
            this.lblVehicleBodyLabel.TabIndex = 41;
            this.lblVehicleBodyLabel.Tag = "Label_Body";
            this.lblVehicleBodyLabel.Text = "Body:";
            // 
            // lblVehicleBody
            // 
            this.lblVehicleBody.AutoSize = true;
            this.lblVehicleBody.Location = new System.Drawing.Point(66, 31);
            this.lblVehicleBody.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblVehicleBody.Name = "lblVehicleBody";
            this.lblVehicleBody.Size = new System.Drawing.Size(37, 13);
            this.lblVehicleBody.TabIndex = 42;
            this.lblVehicleBody.Text = "[Body]";
            // 
            // lblVehicleWeaponsmodLabel
            // 
            this.lblVehicleWeaponsmodLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblVehicleWeaponsmodLabel.AutoSize = true;
            this.lblVehicleWeaponsmodLabel.Location = new System.Drawing.Point(366, 56);
            this.lblVehicleWeaponsmodLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblVehicleWeaponsmodLabel.Name = "lblVehicleWeaponsmodLabel";
            this.lblVehicleWeaponsmodLabel.Size = new System.Drawing.Size(56, 13);
            this.lblVehicleWeaponsmodLabel.TabIndex = 199;
            this.lblVehicleWeaponsmodLabel.Tag = "Label_Weapons";
            this.lblVehicleWeaponsmodLabel.Text = "Weapons:";
            // 
            // lblVehicleDroneModSlotsLabel
            // 
            this.lblVehicleDroneModSlotsLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblVehicleDroneModSlotsLabel.AutoSize = true;
            this.lblVehicleDroneModSlotsLabel.Location = new System.Drawing.Point(3, 56);
            this.lblVehicleDroneModSlotsLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblVehicleDroneModSlotsLabel.Name = "lblVehicleDroneModSlotsLabel";
            this.lblVehicleDroneModSlotsLabel.Size = new System.Drawing.Size(57, 13);
            this.lblVehicleDroneModSlotsLabel.TabIndex = 209;
            this.lblVehicleDroneModSlotsLabel.Tag = "Label_DroneModSlots";
            this.lblVehicleDroneModSlotsLabel.Text = "Mod Slots:";
            this.lblVehicleDroneModSlotsLabel.Visible = false;
            // 
            // lblVehicleSeats
            // 
            this.lblVehicleSeats.AutoSize = true;
            this.lblVehicleSeats.Location = new System.Drawing.Point(297, 31);
            this.lblVehicleSeats.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblVehicleSeats.Name = "lblVehicleSeats";
            this.lblVehicleSeats.Size = new System.Drawing.Size(40, 13);
            this.lblVehicleSeats.TabIndex = 212;
            this.lblVehicleSeats.Text = "[Seats]";
            // 
            // lblVehicleBodymodLabel
            // 
            this.lblVehicleBodymodLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblVehicleBodymodLabel.AutoSize = true;
            this.tlpVehiclesVehicle.SetColumnSpan(this.lblVehicleBodymodLabel, 3);
            this.lblVehicleBodymodLabel.Location = new System.Drawing.Point(112, 81);
            this.lblVehicleBodymodLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblVehicleBodymodLabel.Name = "lblVehicleBodymodLabel";
            this.lblVehicleBodymodLabel.Size = new System.Drawing.Size(63, 13);
            this.lblVehicleBodymodLabel.TabIndex = 200;
            this.lblVehicleBodymodLabel.Tag = "Label_Bodymod";
            this.lblVehicleBodymodLabel.Text = "Body Mods:";
            // 
            // lblVehicleArmorLabel
            // 
            this.lblVehicleArmorLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblVehicleArmorLabel.AutoSize = true;
            this.lblVehicleArmorLabel.Location = new System.Drawing.Point(138, 31);
            this.lblVehicleArmorLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblVehicleArmorLabel.Name = "lblVehicleArmorLabel";
            this.lblVehicleArmorLabel.Size = new System.Drawing.Size(37, 13);
            this.lblVehicleArmorLabel.TabIndex = 43;
            this.lblVehicleArmorLabel.Tag = "Label_Armor";
            this.lblVehicleArmorLabel.Text = "Armor:";
            // 
            // lblVehicleArmor
            // 
            this.lblVehicleArmor.AutoSize = true;
            this.lblVehicleArmor.Location = new System.Drawing.Point(181, 31);
            this.lblVehicleArmor.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblVehicleArmor.Name = "lblVehicleArmor";
            this.lblVehicleArmor.Size = new System.Drawing.Size(40, 13);
            this.lblVehicleArmor.TabIndex = 44;
            this.lblVehicleArmor.Text = "[Armor]";
            // 
            // lblVehicleSeatsLabel
            // 
            this.lblVehicleSeatsLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblVehicleSeatsLabel.AutoSize = true;
            this.lblVehicleSeatsLabel.Location = new System.Drawing.Point(254, 31);
            this.lblVehicleSeatsLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblVehicleSeatsLabel.Name = "lblVehicleSeatsLabel";
            this.lblVehicleSeatsLabel.Size = new System.Drawing.Size(37, 13);
            this.lblVehicleSeatsLabel.TabIndex = 211;
            this.lblVehicleSeatsLabel.Tag = "Label_VehicleSeats";
            this.lblVehicleSeatsLabel.Text = "Seats:";
            // 
            // lblVehicleSensorLabel
            // 
            this.lblVehicleSensorLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblVehicleSensorLabel.AutoSize = true;
            this.lblVehicleSensorLabel.Location = new System.Drawing.Point(379, 31);
            this.lblVehicleSensorLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblVehicleSensorLabel.Name = "lblVehicleSensorLabel";
            this.lblVehicleSensorLabel.Size = new System.Drawing.Size(43, 13);
            this.lblVehicleSensorLabel.TabIndex = 45;
            this.lblVehicleSensorLabel.Tag = "Label_Sensor";
            this.lblVehicleSensorLabel.Text = "Sensor:";
            // 
            // lblVehicleSensor
            // 
            this.lblVehicleSensor.AutoSize = true;
            this.lblVehicleSensor.Location = new System.Drawing.Point(428, 31);
            this.lblVehicleSensor.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblVehicleSensor.Name = "lblVehicleSensor";
            this.lblVehicleSensor.Size = new System.Drawing.Size(46, 13);
            this.lblVehicleSensor.TabIndex = 46;
            this.lblVehicleSensor.Text = "[Sensor]";
            // 
            // lblVehiclePowertrainLabel
            // 
            this.lblVehiclePowertrainLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblVehiclePowertrainLabel.AutoSize = true;
            this.lblVehiclePowertrainLabel.Location = new System.Drawing.Point(135, 56);
            this.lblVehiclePowertrainLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblVehiclePowertrainLabel.Name = "lblVehiclePowertrainLabel";
            this.lblVehiclePowertrainLabel.Size = new System.Drawing.Size(40, 13);
            this.lblVehiclePowertrainLabel.TabIndex = 197;
            this.lblVehiclePowertrainLabel.Tag = "Label_Powertrain";
            this.lblVehiclePowertrainLabel.Text = "Power:";
            // 
            // lblVehiclePowertrain
            // 
            this.lblVehiclePowertrain.AutoSize = true;
            this.lblVehiclePowertrain.Location = new System.Drawing.Point(181, 56);
            this.lblVehiclePowertrain.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblVehiclePowertrain.Name = "lblVehiclePowertrain";
            this.lblVehiclePowertrain.Size = new System.Drawing.Size(43, 13);
            this.lblVehiclePowertrain.TabIndex = 203;
            this.lblVehiclePowertrain.Text = "[Power]";
            // 
            // lblVehicleWeaponsmod
            // 
            this.lblVehicleWeaponsmod.AutoSize = true;
            this.lblVehicleWeaponsmod.Location = new System.Drawing.Point(428, 56);
            this.lblVehicleWeaponsmod.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblVehicleWeaponsmod.Name = "lblVehicleWeaponsmod";
            this.lblVehicleWeaponsmod.Size = new System.Drawing.Size(42, 13);
            this.lblVehicleWeaponsmod.TabIndex = 205;
            this.lblVehicleWeaponsmod.Text = "[Weap]";
            // 
            // lblVehicleElectromagnetic
            // 
            this.lblVehicleElectromagnetic.AutoSize = true;
            this.lblVehicleElectromagnetic.Location = new System.Drawing.Point(297, 81);
            this.lblVehicleElectromagnetic.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblVehicleElectromagnetic.Name = "lblVehicleElectromagnetic";
            this.lblVehicleElectromagnetic.Size = new System.Drawing.Size(34, 13);
            this.lblVehicleElectromagnetic.TabIndex = 207;
            this.lblVehicleElectromagnetic.Text = "[Elec]";
            // 
            // lblVehicleElectromagneticLabel
            // 
            this.lblVehicleElectromagneticLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblVehicleElectromagneticLabel.AutoSize = true;
            this.lblVehicleElectromagneticLabel.Location = new System.Drawing.Point(260, 81);
            this.lblVehicleElectromagneticLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblVehicleElectromagneticLabel.Name = "lblVehicleElectromagneticLabel";
            this.lblVehicleElectromagneticLabel.Size = new System.Drawing.Size(31, 13);
            this.lblVehicleElectromagneticLabel.TabIndex = 201;
            this.lblVehicleElectromagneticLabel.Tag = "Label_Electromagnetic";
            this.lblVehicleElectromagneticLabel.Text = "Elec:";
            // 
            // lblVehicleBodymod
            // 
            this.lblVehicleBodymod.AutoSize = true;
            this.lblVehicleBodymod.Location = new System.Drawing.Point(181, 81);
            this.lblVehicleBodymod.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblVehicleBodymod.Name = "lblVehicleBodymod";
            this.lblVehicleBodymod.Size = new System.Drawing.Size(37, 13);
            this.lblVehicleBodymod.TabIndex = 206;
            this.lblVehicleBodymod.Text = "[Body]";
            // 
            // lblVehicleProtectionLabel
            // 
            this.lblVehicleProtectionLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblVehicleProtectionLabel.AutoSize = true;
            this.lblVehicleProtectionLabel.Location = new System.Drawing.Point(262, 56);
            this.lblVehicleProtectionLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblVehicleProtectionLabel.Name = "lblVehicleProtectionLabel";
            this.lblVehicleProtectionLabel.Size = new System.Drawing.Size(29, 13);
            this.lblVehicleProtectionLabel.TabIndex = 198;
            this.lblVehicleProtectionLabel.Tag = "Label_Protection";
            this.lblVehicleProtectionLabel.Text = "Prot:";
            // 
            // lblVehicleProtection
            // 
            this.lblVehicleProtection.AutoSize = true;
            this.lblVehicleProtection.Location = new System.Drawing.Point(297, 56);
            this.lblVehicleProtection.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblVehicleProtection.Name = "lblVehicleProtection";
            this.lblVehicleProtection.Size = new System.Drawing.Size(32, 13);
            this.lblVehicleProtection.TabIndex = 204;
            this.lblVehicleProtection.Text = "[Prot]";
            // 
            // gpbVehiclesWeapon
            // 
            this.gpbVehiclesWeapon.AutoSize = true;
            this.gpbVehiclesWeapon.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.gpbVehiclesWeapon.Controls.Add(this.flpVehiclesWeapon);
            this.gpbVehiclesWeapon.Dock = System.Windows.Forms.DockStyle.Top;
            this.gpbVehiclesWeapon.Location = new System.Drawing.Point(3, 333);
            this.gpbVehiclesWeapon.Name = "gpbVehiclesWeapon";
            this.gpbVehiclesWeapon.Size = new System.Drawing.Size(501, 152);
            this.gpbVehiclesWeapon.TabIndex = 0;
            this.gpbVehiclesWeapon.TabStop = false;
            this.gpbVehiclesWeapon.Tag = "String_Weapon";
            this.gpbVehiclesWeapon.Text = "Weapon";
            // 
            // flpVehiclesWeapon
            // 
            this.flpVehiclesWeapon.AutoSize = true;
            this.flpVehiclesWeapon.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flpVehiclesWeapon.Controls.Add(this.tlpVehiclesWeaponCommon);
            this.flpVehiclesWeapon.Controls.Add(this.tlpVehiclesWeaponRanges);
            this.flpVehiclesWeapon.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flpVehiclesWeapon.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flpVehiclesWeapon.Location = new System.Drawing.Point(3, 16);
            this.flpVehiclesWeapon.Margin = new System.Windows.Forms.Padding(0);
            this.flpVehiclesWeapon.Name = "flpVehiclesWeapon";
            this.flpVehiclesWeapon.Size = new System.Drawing.Size(495, 133);
            this.flpVehiclesWeapon.TabIndex = 0;
            this.flpVehiclesWeapon.WrapContents = false;
            // 
            // tlpVehiclesWeaponCommon
            // 
            this.tlpVehiclesWeaponCommon.AutoSize = true;
            this.tlpVehiclesWeaponCommon.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpVehiclesWeaponCommon.ColumnCount = 9;
            this.tlpVehiclesWeaponCommon.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpVehiclesWeaponCommon.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tlpVehiclesWeaponCommon.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpVehiclesWeaponCommon.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tlpVehiclesWeaponCommon.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpVehiclesWeaponCommon.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tlpVehiclesWeaponCommon.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpVehiclesWeaponCommon.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tlpVehiclesWeaponCommon.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpVehiclesWeaponCommon.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpVehiclesWeaponCommon.Controls.Add(this.lblVehicleWeaponDamageLabel, 0, 0);
            this.tlpVehiclesWeaponCommon.Controls.Add(this.lblVehicleWeaponDamage, 1, 0);
            this.tlpVehiclesWeaponCommon.Controls.Add(this.lblVehicleWeaponAPLabel, 3, 0);
            this.tlpVehiclesWeaponCommon.Controls.Add(this.lblVehicleWeaponAP, 4, 0);
            this.tlpVehiclesWeaponCommon.Controls.Add(this.lblVehicleWeaponAccuracyLabel, 5, 0);
            this.tlpVehiclesWeaponCommon.Controls.Add(this.lblVehicleWeaponAccuracy, 6, 0);
            this.tlpVehiclesWeaponCommon.Controls.Add(this.lblVehicleWeaponModeLabel, 7, 0);
            this.tlpVehiclesWeaponCommon.Controls.Add(this.lblVehicleWeaponDicePoolLabel, 0, 1);
            this.tlpVehiclesWeaponCommon.Controls.Add(this.lblVehicleWeaponDicePool, 1, 1);
            this.tlpVehiclesWeaponCommon.Controls.Add(this.lblVehicleWeaponAmmoLabel, 3, 1);
            this.tlpVehiclesWeaponCommon.Controls.Add(this.lblVehicleWeaponAmmo, 4, 1);
            this.tlpVehiclesWeaponCommon.Controls.Add(this.cboVehicleWeaponFiringMode, 6, 1);
            this.tlpVehiclesWeaponCommon.Controls.Add(this.lblFiringModeLabel, 5, 1);
            this.tlpVehiclesWeaponCommon.Controls.Add(this.lblVehicleWeaponMode, 8, 0);
            this.tlpVehiclesWeaponCommon.Dock = System.Windows.Forms.DockStyle.Top;
            this.tlpVehiclesWeaponCommon.Location = new System.Drawing.Point(0, 0);
            this.tlpVehiclesWeaponCommon.Margin = new System.Windows.Forms.Padding(0, 0, 0, 3);
            this.tlpVehiclesWeaponCommon.Name = "tlpVehiclesWeaponCommon";
            this.tlpVehiclesWeaponCommon.RowCount = 2;
            this.tlpVehiclesWeaponCommon.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpVehiclesWeaponCommon.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpVehiclesWeaponCommon.Size = new System.Drawing.Size(494, 52);
            this.tlpVehiclesWeaponCommon.TabIndex = 0;
            // 
            // lblVehicleWeaponDamageLabel
            // 
            this.lblVehicleWeaponDamageLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblVehicleWeaponDamageLabel.AutoSize = true;
            this.lblVehicleWeaponDamageLabel.Location = new System.Drawing.Point(9, 6);
            this.lblVehicleWeaponDamageLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblVehicleWeaponDamageLabel.Name = "lblVehicleWeaponDamageLabel";
            this.lblVehicleWeaponDamageLabel.Size = new System.Drawing.Size(50, 13);
            this.lblVehicleWeaponDamageLabel.TabIndex = 134;
            this.lblVehicleWeaponDamageLabel.Tag = "Label_Damage";
            this.lblVehicleWeaponDamageLabel.Text = "Damage:";
            // 
            // lblVehicleWeaponDamage
            // 
            this.lblVehicleWeaponDamage.AutoSize = true;
            this.lblVehicleWeaponDamage.Location = new System.Drawing.Point(65, 6);
            this.lblVehicleWeaponDamage.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblVehicleWeaponDamage.Name = "lblVehicleWeaponDamage";
            this.lblVehicleWeaponDamage.Size = new System.Drawing.Size(53, 13);
            this.lblVehicleWeaponDamage.TabIndex = 135;
            this.lblVehicleWeaponDamage.Text = "[Damage]";
            // 
            // lblVehicleWeaponAPLabel
            // 
            this.lblVehicleWeaponAPLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblVehicleWeaponAPLabel.AutoSize = true;
            this.lblVehicleWeaponAPLabel.Location = new System.Drawing.Point(185, 6);
            this.lblVehicleWeaponAPLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblVehicleWeaponAPLabel.Name = "lblVehicleWeaponAPLabel";
            this.lblVehicleWeaponAPLabel.Size = new System.Drawing.Size(24, 13);
            this.lblVehicleWeaponAPLabel.TabIndex = 136;
            this.lblVehicleWeaponAPLabel.Tag = "Label_AP";
            this.lblVehicleWeaponAPLabel.Text = "AP:";
            // 
            // lblVehicleWeaponAP
            // 
            this.lblVehicleWeaponAP.AutoSize = true;
            this.lblVehicleWeaponAP.Location = new System.Drawing.Point(215, 6);
            this.lblVehicleWeaponAP.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblVehicleWeaponAP.Name = "lblVehicleWeaponAP";
            this.lblVehicleWeaponAP.Size = new System.Drawing.Size(27, 13);
            this.lblVehicleWeaponAP.TabIndex = 137;
            this.lblVehicleWeaponAP.Text = "[AP]";
            // 
            // lblVehicleWeaponAccuracyLabel
            // 
            this.lblVehicleWeaponAccuracyLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblVehicleWeaponAccuracyLabel.AutoSize = true;
            this.lblVehicleWeaponAccuracyLabel.Location = new System.Drawing.Point(277, 6);
            this.lblVehicleWeaponAccuracyLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblVehicleWeaponAccuracyLabel.Name = "lblVehicleWeaponAccuracyLabel";
            this.lblVehicleWeaponAccuracyLabel.Size = new System.Drawing.Size(55, 13);
            this.lblVehicleWeaponAccuracyLabel.TabIndex = 243;
            this.lblVehicleWeaponAccuracyLabel.Tag = "Label_Accuracy";
            this.lblVehicleWeaponAccuracyLabel.Text = "Accuracy:";
            // 
            // lblVehicleWeaponAccuracy
            // 
            this.lblVehicleWeaponAccuracy.AutoSize = true;
            this.lblVehicleWeaponAccuracy.Location = new System.Drawing.Point(338, 6);
            this.lblVehicleWeaponAccuracy.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblVehicleWeaponAccuracy.Name = "lblVehicleWeaponAccuracy";
            this.lblVehicleWeaponAccuracy.Size = new System.Drawing.Size(32, 13);
            this.lblVehicleWeaponAccuracy.TabIndex = 244;
            this.lblVehicleWeaponAccuracy.Text = "[Acc]";
            // 
            // lblVehicleWeaponModeLabel
            // 
            this.lblVehicleWeaponModeLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblVehicleWeaponModeLabel.AutoSize = true;
            this.lblVehicleWeaponModeLabel.Location = new System.Drawing.Point(408, 6);
            this.lblVehicleWeaponModeLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblVehicleWeaponModeLabel.Name = "lblVehicleWeaponModeLabel";
            this.lblVehicleWeaponModeLabel.Size = new System.Drawing.Size(37, 13);
            this.lblVehicleWeaponModeLabel.TabIndex = 138;
            this.lblVehicleWeaponModeLabel.Tag = "Label_Mode";
            this.lblVehicleWeaponModeLabel.Text = "Mode:";
            // 
            // lblVehicleWeaponDicePoolLabel
            // 
            this.lblVehicleWeaponDicePoolLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblVehicleWeaponDicePoolLabel.AutoSize = true;
            this.lblVehicleWeaponDicePoolLabel.Location = new System.Drawing.Point(3, 31);
            this.lblVehicleWeaponDicePoolLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblVehicleWeaponDicePoolLabel.Name = "lblVehicleWeaponDicePoolLabel";
            this.lblVehicleWeaponDicePoolLabel.Size = new System.Drawing.Size(56, 13);
            this.lblVehicleWeaponDicePoolLabel.TabIndex = 239;
            this.lblVehicleWeaponDicePoolLabel.Tag = "Label_DicePool";
            this.lblVehicleWeaponDicePoolLabel.Text = "Dice Pool:";
            // 
            // lblVehicleWeaponDicePool
            // 
            this.lblVehicleWeaponDicePool.AutoSize = true;
            this.lblVehicleWeaponDicePool.Location = new System.Drawing.Point(65, 31);
            this.lblVehicleWeaponDicePool.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblVehicleWeaponDicePool.Name = "lblVehicleWeaponDicePool";
            this.lblVehicleWeaponDicePool.Size = new System.Drawing.Size(34, 13);
            this.lblVehicleWeaponDicePool.TabIndex = 240;
            this.lblVehicleWeaponDicePool.Text = "[Pool]";
            // 
            // lblVehicleWeaponAmmoLabel
            // 
            this.lblVehicleWeaponAmmoLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblVehicleWeaponAmmoLabel.AutoSize = true;
            this.lblVehicleWeaponAmmoLabel.Location = new System.Drawing.Point(170, 31);
            this.lblVehicleWeaponAmmoLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblVehicleWeaponAmmoLabel.Name = "lblVehicleWeaponAmmoLabel";
            this.lblVehicleWeaponAmmoLabel.Size = new System.Drawing.Size(39, 13);
            this.lblVehicleWeaponAmmoLabel.TabIndex = 140;
            this.lblVehicleWeaponAmmoLabel.Tag = "Label_Ammo";
            this.lblVehicleWeaponAmmoLabel.Text = "Ammo:";
            // 
            // lblVehicleWeaponAmmo
            // 
            this.lblVehicleWeaponAmmo.AutoSize = true;
            this.lblVehicleWeaponAmmo.Location = new System.Drawing.Point(215, 31);
            this.lblVehicleWeaponAmmo.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblVehicleWeaponAmmo.Name = "lblVehicleWeaponAmmo";
            this.lblVehicleWeaponAmmo.Size = new System.Drawing.Size(42, 13);
            this.lblVehicleWeaponAmmo.TabIndex = 141;
            this.lblVehicleWeaponAmmo.Text = "[Ammo]";
            // 
            // cboVehicleWeaponFiringMode
            // 
            this.cboVehicleWeaponFiringMode.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tlpVehiclesWeaponCommon.SetColumnSpan(this.cboVehicleWeaponFiringMode, 3);
            this.cboVehicleWeaponFiringMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboVehicleWeaponFiringMode.FormattingEnabled = true;
            this.cboVehicleWeaponFiringMode.Location = new System.Drawing.Point(338, 28);
            this.cboVehicleWeaponFiringMode.Name = "cboVehicleWeaponFiringMode";
            this.cboVehicleWeaponFiringMode.Size = new System.Drawing.Size(153, 21);
            this.cboVehicleWeaponFiringMode.TabIndex = 250;
            this.cboVehicleWeaponFiringMode.TooltipText = "";
            this.cboVehicleWeaponFiringMode.SelectedIndexChanged += new System.EventHandler(this.cboVehicleWeaponFiringMode_SelectedIndexChanged);
            // 
            // lblFiringModeLabel
            // 
            this.lblFiringModeLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblFiringModeLabel.AutoSize = true;
            this.lblFiringModeLabel.Location = new System.Drawing.Point(267, 31);
            this.lblFiringModeLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblFiringModeLabel.Name = "lblFiringModeLabel";
            this.lblFiringModeLabel.Size = new System.Drawing.Size(65, 13);
            this.lblFiringModeLabel.TabIndex = 249;
            this.lblFiringModeLabel.Tag = "Label_FiringMode";
            this.lblFiringModeLabel.Text = "Firing Mode:";
            // 
            // lblVehicleWeaponMode
            // 
            this.lblVehicleWeaponMode.AutoSize = true;
            this.lblVehicleWeaponMode.Location = new System.Drawing.Point(451, 6);
            this.lblVehicleWeaponMode.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblVehicleWeaponMode.Name = "lblVehicleWeaponMode";
            this.lblVehicleWeaponMode.Size = new System.Drawing.Size(40, 13);
            this.lblVehicleWeaponMode.TabIndex = 139;
            this.lblVehicleWeaponMode.Text = "[Mode]";
            // 
            // tlpVehiclesWeaponRanges
            // 
            this.tlpVehiclesWeaponRanges.AutoSize = true;
            this.tlpVehiclesWeaponRanges.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpVehiclesWeaponRanges.ColumnCount = 5;
            this.tlpVehiclesWeaponRanges.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tlpVehiclesWeaponRanges.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tlpVehiclesWeaponRanges.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tlpVehiclesWeaponRanges.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tlpVehiclesWeaponRanges.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tlpVehiclesWeaponRanges.Controls.Add(this.lblVehicleWeaponAlternateRangeExtreme, 4, 2);
            this.tlpVehiclesWeaponRanges.Controls.Add(this.lblVehicleWeaponRangeExtreme, 4, 1);
            this.tlpVehiclesWeaponRanges.Controls.Add(this.lblVehicleWeaponAlternateRangeLong, 3, 2);
            this.tlpVehiclesWeaponRanges.Controls.Add(this.lblVehicleWeaponRangeExtremeLabel, 4, 0);
            this.tlpVehiclesWeaponRanges.Controls.Add(this.lblVehicleWeaponRangeLabel, 0, 0);
            this.tlpVehiclesWeaponRanges.Controls.Add(this.lblVehicleWeaponAlternateRangeMedium, 2, 2);
            this.tlpVehiclesWeaponRanges.Controls.Add(this.lblVehicleWeaponRangeLong, 3, 1);
            this.tlpVehiclesWeaponRanges.Controls.Add(this.lblVehicleWeaponAlternateRangeShort, 1, 2);
            this.tlpVehiclesWeaponRanges.Controls.Add(this.lblVehicleWeaponRangeLongLabel, 3, 0);
            this.tlpVehiclesWeaponRanges.Controls.Add(this.lblVehicleWeaponRangeShortLabel, 1, 0);
            this.tlpVehiclesWeaponRanges.Controls.Add(this.lblVehicleWeaponRangeMedium, 2, 1);
            this.tlpVehiclesWeaponRanges.Controls.Add(this.lblVehicleWeaponRangeAlternate, 0, 2);
            this.tlpVehiclesWeaponRanges.Controls.Add(this.lblVehicleWeaponRangeMain, 0, 1);
            this.tlpVehiclesWeaponRanges.Controls.Add(this.lblVehicleWeaponRangeMediumLabel, 2, 0);
            this.tlpVehiclesWeaponRanges.Controls.Add(this.lblVehicleWeaponRangeShort, 1, 1);
            this.tlpVehiclesWeaponRanges.Dock = System.Windows.Forms.DockStyle.Top;
            this.tlpVehiclesWeaponRanges.Location = new System.Drawing.Point(0, 58);
            this.tlpVehiclesWeaponRanges.Margin = new System.Windows.Forms.Padding(0, 3, 0, 0);
            this.tlpVehiclesWeaponRanges.MinimumSize = new System.Drawing.Size(494, 0);
            this.tlpVehiclesWeaponRanges.Name = "tlpVehiclesWeaponRanges";
            this.tlpVehiclesWeaponRanges.RowCount = 3;
            this.tlpVehiclesWeaponRanges.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpVehiclesWeaponRanges.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpVehiclesWeaponRanges.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpVehiclesWeaponRanges.Size = new System.Drawing.Size(494, 75);
            this.tlpVehiclesWeaponRanges.TabIndex = 245;
            // 
            // lblVehicleWeaponAlternateRangeExtreme
            // 
            this.lblVehicleWeaponAlternateRangeExtreme.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblVehicleWeaponAlternateRangeExtreme.AutoSize = true;
            this.lblVehicleWeaponAlternateRangeExtreme.Location = new System.Drawing.Point(395, 56);
            this.lblVehicleWeaponAlternateRangeExtreme.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblVehicleWeaponAlternateRangeExtreme.Name = "lblVehicleWeaponAlternateRangeExtreme";
            this.lblVehicleWeaponAlternateRangeExtreme.Size = new System.Drawing.Size(96, 13);
            this.lblVehicleWeaponAlternateRangeExtreme.TabIndex = 236;
            this.lblVehicleWeaponAlternateRangeExtreme.Text = "[0]";
            this.lblVehicleWeaponAlternateRangeExtreme.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lblVehicleWeaponRangeExtreme
            // 
            this.lblVehicleWeaponRangeExtreme.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblVehicleWeaponRangeExtreme.AutoSize = true;
            this.lblVehicleWeaponRangeExtreme.Location = new System.Drawing.Point(395, 31);
            this.lblVehicleWeaponRangeExtreme.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblVehicleWeaponRangeExtreme.Name = "lblVehicleWeaponRangeExtreme";
            this.lblVehicleWeaponRangeExtreme.Size = new System.Drawing.Size(96, 13);
            this.lblVehicleWeaponRangeExtreme.TabIndex = 150;
            this.lblVehicleWeaponRangeExtreme.Text = "[0]";
            this.lblVehicleWeaponRangeExtreme.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lblVehicleWeaponAlternateRangeLong
            // 
            this.lblVehicleWeaponAlternateRangeLong.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblVehicleWeaponAlternateRangeLong.AutoSize = true;
            this.lblVehicleWeaponAlternateRangeLong.Location = new System.Drawing.Point(297, 56);
            this.lblVehicleWeaponAlternateRangeLong.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblVehicleWeaponAlternateRangeLong.Name = "lblVehicleWeaponAlternateRangeLong";
            this.lblVehicleWeaponAlternateRangeLong.Size = new System.Drawing.Size(92, 13);
            this.lblVehicleWeaponAlternateRangeLong.TabIndex = 235;
            this.lblVehicleWeaponAlternateRangeLong.Text = "[0]";
            this.lblVehicleWeaponAlternateRangeLong.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lblVehicleWeaponRangeExtremeLabel
            // 
            this.lblVehicleWeaponRangeExtremeLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblVehicleWeaponRangeExtremeLabel.AutoSize = true;
            this.lblVehicleWeaponRangeExtremeLabel.Location = new System.Drawing.Point(395, 6);
            this.lblVehicleWeaponRangeExtremeLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblVehicleWeaponRangeExtremeLabel.Name = "lblVehicleWeaponRangeExtremeLabel";
            this.lblVehicleWeaponRangeExtremeLabel.Size = new System.Drawing.Size(96, 13);
            this.lblVehicleWeaponRangeExtremeLabel.TabIndex = 146;
            this.lblVehicleWeaponRangeExtremeLabel.Tag = "Label_RangeExtreme";
            this.lblVehicleWeaponRangeExtremeLabel.Text = "Extreme (-6)";
            this.lblVehicleWeaponRangeExtremeLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lblVehicleWeaponRangeLabel
            // 
            this.lblVehicleWeaponRangeLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblVehicleWeaponRangeLabel.AutoSize = true;
            this.lblVehicleWeaponRangeLabel.Location = new System.Drawing.Point(3, 6);
            this.lblVehicleWeaponRangeLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblVehicleWeaponRangeLabel.Name = "lblVehicleWeaponRangeLabel";
            this.lblVehicleWeaponRangeLabel.Size = new System.Drawing.Size(92, 13);
            this.lblVehicleWeaponRangeLabel.TabIndex = 142;
            this.lblVehicleWeaponRangeLabel.Tag = "Label_RangeHeading";
            this.lblVehicleWeaponRangeLabel.Text = "Range";
            this.lblVehicleWeaponRangeLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lblVehicleWeaponAlternateRangeMedium
            // 
            this.lblVehicleWeaponAlternateRangeMedium.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblVehicleWeaponAlternateRangeMedium.AutoSize = true;
            this.lblVehicleWeaponAlternateRangeMedium.Location = new System.Drawing.Point(199, 56);
            this.lblVehicleWeaponAlternateRangeMedium.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblVehicleWeaponAlternateRangeMedium.Name = "lblVehicleWeaponAlternateRangeMedium";
            this.lblVehicleWeaponAlternateRangeMedium.Size = new System.Drawing.Size(92, 13);
            this.lblVehicleWeaponAlternateRangeMedium.TabIndex = 234;
            this.lblVehicleWeaponAlternateRangeMedium.Text = "[0]";
            this.lblVehicleWeaponAlternateRangeMedium.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lblVehicleWeaponRangeLong
            // 
            this.lblVehicleWeaponRangeLong.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblVehicleWeaponRangeLong.AutoSize = true;
            this.lblVehicleWeaponRangeLong.Location = new System.Drawing.Point(297, 31);
            this.lblVehicleWeaponRangeLong.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblVehicleWeaponRangeLong.Name = "lblVehicleWeaponRangeLong";
            this.lblVehicleWeaponRangeLong.Size = new System.Drawing.Size(92, 13);
            this.lblVehicleWeaponRangeLong.TabIndex = 149;
            this.lblVehicleWeaponRangeLong.Text = "[0]";
            this.lblVehicleWeaponRangeLong.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lblVehicleWeaponAlternateRangeShort
            // 
            this.lblVehicleWeaponAlternateRangeShort.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblVehicleWeaponAlternateRangeShort.AutoSize = true;
            this.lblVehicleWeaponAlternateRangeShort.Location = new System.Drawing.Point(101, 56);
            this.lblVehicleWeaponAlternateRangeShort.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblVehicleWeaponAlternateRangeShort.Name = "lblVehicleWeaponAlternateRangeShort";
            this.lblVehicleWeaponAlternateRangeShort.Size = new System.Drawing.Size(92, 13);
            this.lblVehicleWeaponAlternateRangeShort.TabIndex = 233;
            this.lblVehicleWeaponAlternateRangeShort.Text = "[0]";
            this.lblVehicleWeaponAlternateRangeShort.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lblVehicleWeaponRangeLongLabel
            // 
            this.lblVehicleWeaponRangeLongLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblVehicleWeaponRangeLongLabel.AutoSize = true;
            this.lblVehicleWeaponRangeLongLabel.Location = new System.Drawing.Point(297, 6);
            this.lblVehicleWeaponRangeLongLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblVehicleWeaponRangeLongLabel.Name = "lblVehicleWeaponRangeLongLabel";
            this.lblVehicleWeaponRangeLongLabel.Size = new System.Drawing.Size(92, 13);
            this.lblVehicleWeaponRangeLongLabel.TabIndex = 145;
            this.lblVehicleWeaponRangeLongLabel.Tag = "Label_RangeLong";
            this.lblVehicleWeaponRangeLongLabel.Text = "Long (-3)";
            this.lblVehicleWeaponRangeLongLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lblVehicleWeaponRangeShortLabel
            // 
            this.lblVehicleWeaponRangeShortLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblVehicleWeaponRangeShortLabel.AutoSize = true;
            this.lblVehicleWeaponRangeShortLabel.Location = new System.Drawing.Point(101, 6);
            this.lblVehicleWeaponRangeShortLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblVehicleWeaponRangeShortLabel.Name = "lblVehicleWeaponRangeShortLabel";
            this.lblVehicleWeaponRangeShortLabel.Size = new System.Drawing.Size(92, 13);
            this.lblVehicleWeaponRangeShortLabel.TabIndex = 143;
            this.lblVehicleWeaponRangeShortLabel.Tag = "Label_RangeShort";
            this.lblVehicleWeaponRangeShortLabel.Text = "Short (-0)";
            this.lblVehicleWeaponRangeShortLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lblVehicleWeaponRangeMedium
            // 
            this.lblVehicleWeaponRangeMedium.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblVehicleWeaponRangeMedium.AutoSize = true;
            this.lblVehicleWeaponRangeMedium.Location = new System.Drawing.Point(199, 31);
            this.lblVehicleWeaponRangeMedium.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblVehicleWeaponRangeMedium.Name = "lblVehicleWeaponRangeMedium";
            this.lblVehicleWeaponRangeMedium.Size = new System.Drawing.Size(92, 13);
            this.lblVehicleWeaponRangeMedium.TabIndex = 148;
            this.lblVehicleWeaponRangeMedium.Text = "[0]";
            this.lblVehicleWeaponRangeMedium.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lblVehicleWeaponRangeAlternate
            // 
            this.lblVehicleWeaponRangeAlternate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblVehicleWeaponRangeAlternate.AutoSize = true;
            this.lblVehicleWeaponRangeAlternate.Location = new System.Drawing.Point(5, 56);
            this.lblVehicleWeaponRangeAlternate.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblVehicleWeaponRangeAlternate.Name = "lblVehicleWeaponRangeAlternate";
            this.lblVehicleWeaponRangeAlternate.Size = new System.Drawing.Size(90, 13);
            this.lblVehicleWeaponRangeAlternate.TabIndex = 238;
            this.lblVehicleWeaponRangeAlternate.Tag = "";
            this.lblVehicleWeaponRangeAlternate.Text = "[Alternate Range]";
            // 
            // lblVehicleWeaponRangeMain
            // 
            this.lblVehicleWeaponRangeMain.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblVehicleWeaponRangeMain.AutoSize = true;
            this.lblVehicleWeaponRangeMain.Location = new System.Drawing.Point(24, 31);
            this.lblVehicleWeaponRangeMain.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblVehicleWeaponRangeMain.Name = "lblVehicleWeaponRangeMain";
            this.lblVehicleWeaponRangeMain.Size = new System.Drawing.Size(71, 13);
            this.lblVehicleWeaponRangeMain.TabIndex = 237;
            this.lblVehicleWeaponRangeMain.Tag = "";
            this.lblVehicleWeaponRangeMain.Text = "[Main Range]";
            // 
            // lblVehicleWeaponRangeMediumLabel
            // 
            this.lblVehicleWeaponRangeMediumLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblVehicleWeaponRangeMediumLabel.AutoSize = true;
            this.lblVehicleWeaponRangeMediumLabel.Location = new System.Drawing.Point(199, 6);
            this.lblVehicleWeaponRangeMediumLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblVehicleWeaponRangeMediumLabel.Name = "lblVehicleWeaponRangeMediumLabel";
            this.lblVehicleWeaponRangeMediumLabel.Size = new System.Drawing.Size(92, 13);
            this.lblVehicleWeaponRangeMediumLabel.TabIndex = 144;
            this.lblVehicleWeaponRangeMediumLabel.Tag = "Label_RangeMedium";
            this.lblVehicleWeaponRangeMediumLabel.Text = "Medium (-1)";
            this.lblVehicleWeaponRangeMediumLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lblVehicleWeaponRangeShort
            // 
            this.lblVehicleWeaponRangeShort.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblVehicleWeaponRangeShort.AutoSize = true;
            this.lblVehicleWeaponRangeShort.Location = new System.Drawing.Point(101, 31);
            this.lblVehicleWeaponRangeShort.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblVehicleWeaponRangeShort.Name = "lblVehicleWeaponRangeShort";
            this.lblVehicleWeaponRangeShort.Size = new System.Drawing.Size(92, 13);
            this.lblVehicleWeaponRangeShort.TabIndex = 147;
            this.lblVehicleWeaponRangeShort.Text = "[0]";
            this.lblVehicleWeaponRangeShort.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // gpbVehiclesMatrix
            // 
            this.gpbVehiclesMatrix.AutoSize = true;
            this.gpbVehiclesMatrix.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.gpbVehiclesMatrix.Controls.Add(this.tlpVehiclesMatrix);
            this.gpbVehiclesMatrix.Dock = System.Windows.Forms.DockStyle.Top;
            this.gpbVehiclesMatrix.Location = new System.Drawing.Point(3, 491);
            this.gpbVehiclesMatrix.Name = "gpbVehiclesMatrix";
            this.gpbVehiclesMatrix.Size = new System.Drawing.Size(501, 94);
            this.gpbVehiclesMatrix.TabIndex = 1;
            this.gpbVehiclesMatrix.TabStop = false;
            this.gpbVehiclesMatrix.Tag = "String_Matrix";
            this.gpbVehiclesMatrix.Text = "Matrix";
            // 
            // tlpVehiclesMatrix
            // 
            this.tlpVehiclesMatrix.AutoSize = true;
            this.tlpVehiclesMatrix.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpVehiclesMatrix.ColumnCount = 5;
            this.tlpVehiclesMatrix.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tlpVehiclesMatrix.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tlpVehiclesMatrix.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tlpVehiclesMatrix.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tlpVehiclesMatrix.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tlpVehiclesMatrix.Controls.Add(this.lblVehicleDeviceLabel, 0, 1);
            this.tlpVehiclesMatrix.Controls.Add(this.lblVehicleDevice, 0, 2);
            this.tlpVehiclesMatrix.Controls.Add(this.cboVehicleAttack, 1, 2);
            this.tlpVehiclesMatrix.Controls.Add(this.lblVehicleAttackLabel, 1, 1);
            this.tlpVehiclesMatrix.Controls.Add(this.lblVehicleSleazeLabel, 2, 1);
            this.tlpVehiclesMatrix.Controls.Add(this.cboVehicleSleaze, 2, 2);
            this.tlpVehiclesMatrix.Controls.Add(this.lblVehicleDataProcessingLabel, 3, 1);
            this.tlpVehiclesMatrix.Controls.Add(this.cboVehicleDataProcessing, 3, 2);
            this.tlpVehiclesMatrix.Controls.Add(this.cboVehicleFirewall, 4, 2);
            this.tlpVehiclesMatrix.Controls.Add(this.lblVehicleFirewallLabel, 4, 1);
            this.tlpVehiclesMatrix.Controls.Add(this.flpVehiclesMatrixCheckBoxes, 2, 0);
            this.tlpVehiclesMatrix.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpVehiclesMatrix.Location = new System.Drawing.Point(3, 16);
            this.tlpVehiclesMatrix.Name = "tlpVehiclesMatrix";
            this.tlpVehiclesMatrix.RowCount = 3;
            this.tlpVehiclesMatrix.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpVehiclesMatrix.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpVehiclesMatrix.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpVehiclesMatrix.Size = new System.Drawing.Size(495, 75);
            this.tlpVehiclesMatrix.TabIndex = 0;
            // 
            // lblVehicleDeviceLabel
            // 
            this.lblVehicleDeviceLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblVehicleDeviceLabel.AutoSize = true;
            this.lblVehicleDeviceLabel.Location = new System.Drawing.Point(3, 29);
            this.lblVehicleDeviceLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblVehicleDeviceLabel.Name = "lblVehicleDeviceLabel";
            this.lblVehicleDeviceLabel.Size = new System.Drawing.Size(78, 13);
            this.lblVehicleDeviceLabel.TabIndex = 117;
            this.lblVehicleDeviceLabel.Tag = "Label_DeviceRating";
            this.lblVehicleDeviceLabel.Text = "Device Rating:";
            // 
            // lblVehicleDevice
            // 
            this.lblVehicleDevice.AutoSize = true;
            this.lblVehicleDevice.Location = new System.Drawing.Point(3, 54);
            this.lblVehicleDevice.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblVehicleDevice.Name = "lblVehicleDevice";
            this.lblVehicleDevice.Size = new System.Drawing.Size(47, 13);
            this.lblVehicleDevice.TabIndex = 118;
            this.lblVehicleDevice.Text = "[Device]";
            // 
            // cboVehicleAttack
            // 
            this.cboVehicleAttack.Dock = System.Windows.Forms.DockStyle.Top;
            this.cboVehicleAttack.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboVehicleAttack.FormattingEnabled = true;
            this.cboVehicleAttack.Location = new System.Drawing.Point(102, 51);
            this.cboVehicleAttack.Name = "cboVehicleAttack";
            this.cboVehicleAttack.Size = new System.Drawing.Size(93, 21);
            this.cboVehicleAttack.TabIndex = 193;
            this.cboVehicleAttack.TooltipText = "";
            this.cboVehicleAttack.Visible = false;
            this.cboVehicleAttack.SelectedIndexChanged += new System.EventHandler(this.cboVehicleAttack_SelectedIndexChanged);
            // 
            // lblVehicleAttackLabel
            // 
            this.lblVehicleAttackLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblVehicleAttackLabel.AutoSize = true;
            this.lblVehicleAttackLabel.Location = new System.Drawing.Point(102, 29);
            this.lblVehicleAttackLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblVehicleAttackLabel.Name = "lblVehicleAttackLabel";
            this.lblVehicleAttackLabel.Size = new System.Drawing.Size(41, 13);
            this.lblVehicleAttackLabel.TabIndex = 186;
            this.lblVehicleAttackLabel.Tag = "Label_Attack";
            this.lblVehicleAttackLabel.Text = "Attack:";
            // 
            // lblVehicleSleazeLabel
            // 
            this.lblVehicleSleazeLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblVehicleSleazeLabel.AutoSize = true;
            this.lblVehicleSleazeLabel.Location = new System.Drawing.Point(201, 29);
            this.lblVehicleSleazeLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblVehicleSleazeLabel.Name = "lblVehicleSleazeLabel";
            this.lblVehicleSleazeLabel.Size = new System.Drawing.Size(42, 13);
            this.lblVehicleSleazeLabel.TabIndex = 188;
            this.lblVehicleSleazeLabel.Tag = "Label_Sleaze";
            this.lblVehicleSleazeLabel.Text = "Sleaze:";
            // 
            // cboVehicleSleaze
            // 
            this.cboVehicleSleaze.Dock = System.Windows.Forms.DockStyle.Top;
            this.cboVehicleSleaze.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboVehicleSleaze.FormattingEnabled = true;
            this.cboVehicleSleaze.Location = new System.Drawing.Point(201, 51);
            this.cboVehicleSleaze.Name = "cboVehicleSleaze";
            this.cboVehicleSleaze.Size = new System.Drawing.Size(93, 21);
            this.cboVehicleSleaze.TabIndex = 194;
            this.cboVehicleSleaze.TooltipText = "";
            this.cboVehicleSleaze.Visible = false;
            this.cboVehicleSleaze.SelectedIndexChanged += new System.EventHandler(this.cboVehicleSleaze_SelectedIndexChanged);
            // 
            // lblVehicleDataProcessingLabel
            // 
            this.lblVehicleDataProcessingLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblVehicleDataProcessingLabel.AutoSize = true;
            this.lblVehicleDataProcessingLabel.Location = new System.Drawing.Point(300, 29);
            this.lblVehicleDataProcessingLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblVehicleDataProcessingLabel.Name = "lblVehicleDataProcessingLabel";
            this.lblVehicleDataProcessingLabel.Size = new System.Drawing.Size(58, 13);
            this.lblVehicleDataProcessingLabel.TabIndex = 190;
            this.lblVehicleDataProcessingLabel.Tag = "Label_DataProcessing";
            this.lblVehicleDataProcessingLabel.Text = "Data Proc:";
            // 
            // cboVehicleDataProcessing
            // 
            this.cboVehicleDataProcessing.Dock = System.Windows.Forms.DockStyle.Top;
            this.cboVehicleDataProcessing.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboVehicleDataProcessing.FormattingEnabled = true;
            this.cboVehicleDataProcessing.Location = new System.Drawing.Point(300, 51);
            this.cboVehicleDataProcessing.Name = "cboVehicleDataProcessing";
            this.cboVehicleDataProcessing.Size = new System.Drawing.Size(93, 21);
            this.cboVehicleDataProcessing.TabIndex = 196;
            this.cboVehicleDataProcessing.TooltipText = "";
            this.cboVehicleDataProcessing.Visible = false;
            this.cboVehicleDataProcessing.SelectedIndexChanged += new System.EventHandler(this.cboVehicleDataProcessing_SelectedIndexChanged);
            // 
            // cboVehicleFirewall
            // 
            this.cboVehicleFirewall.Dock = System.Windows.Forms.DockStyle.Top;
            this.cboVehicleFirewall.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboVehicleFirewall.FormattingEnabled = true;
            this.cboVehicleFirewall.Location = new System.Drawing.Point(399, 51);
            this.cboVehicleFirewall.Name = "cboVehicleFirewall";
            this.cboVehicleFirewall.Size = new System.Drawing.Size(93, 21);
            this.cboVehicleFirewall.TabIndex = 195;
            this.cboVehicleFirewall.TooltipText = "";
            this.cboVehicleFirewall.Visible = false;
            this.cboVehicleFirewall.SelectedIndexChanged += new System.EventHandler(this.cboVehicleFirewall_SelectedIndexChanged);
            // 
            // lblVehicleFirewallLabel
            // 
            this.lblVehicleFirewallLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblVehicleFirewallLabel.AutoSize = true;
            this.lblVehicleFirewallLabel.Location = new System.Drawing.Point(399, 29);
            this.lblVehicleFirewallLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblVehicleFirewallLabel.Name = "lblVehicleFirewallLabel";
            this.lblVehicleFirewallLabel.Size = new System.Drawing.Size(45, 13);
            this.lblVehicleFirewallLabel.TabIndex = 192;
            this.lblVehicleFirewallLabel.Tag = "Label_Firewall";
            this.lblVehicleFirewallLabel.Text = "Firewall:";
            // 
            // flpVehiclesMatrixCheckBoxes
            // 
            this.flpVehiclesMatrixCheckBoxes.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.flpVehiclesMatrixCheckBoxes.AutoSize = true;
            this.flpVehiclesMatrixCheckBoxes.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpVehiclesMatrix.SetColumnSpan(this.flpVehiclesMatrixCheckBoxes, 3);
            this.flpVehiclesMatrixCheckBoxes.Controls.Add(this.chkVehicleHomeNode);
            this.flpVehiclesMatrixCheckBoxes.Controls.Add(this.chkVehicleActiveCommlink);
            this.flpVehiclesMatrixCheckBoxes.Location = new System.Drawing.Point(198, 0);
            this.flpVehiclesMatrixCheckBoxes.Margin = new System.Windows.Forms.Padding(0);
            this.flpVehiclesMatrixCheckBoxes.Name = "flpVehiclesMatrixCheckBoxes";
            this.flpVehiclesMatrixCheckBoxes.Size = new System.Drawing.Size(199, 23);
            this.flpVehiclesMatrixCheckBoxes.TabIndex = 243;
            this.flpVehiclesMatrixCheckBoxes.WrapContents = false;
            // 
            // chkVehicleHomeNode
            // 
            this.chkVehicleHomeNode.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkVehicleHomeNode.AutoSize = true;
            this.chkVehicleHomeNode.Location = new System.Drawing.Point(3, 3);
            this.chkVehicleHomeNode.Name = "chkVehicleHomeNode";
            this.chkVehicleHomeNode.Size = new System.Drawing.Size(83, 17);
            this.chkVehicleHomeNode.TabIndex = 127;
            this.chkVehicleHomeNode.Tag = "Checkbox_HomeNode";
            this.chkVehicleHomeNode.Text = "Home Node";
            this.chkVehicleHomeNode.UseVisualStyleBackColor = true;
            this.chkVehicleHomeNode.Visible = false;
            this.chkVehicleHomeNode.CheckedChanged += new System.EventHandler(this.chkVehicleHomeNode_CheckedChanged);
            // 
            // chkVehicleActiveCommlink
            // 
            this.chkVehicleActiveCommlink.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkVehicleActiveCommlink.AutoSize = true;
            this.chkVehicleActiveCommlink.Location = new System.Drawing.Point(92, 3);
            this.chkVehicleActiveCommlink.Name = "chkVehicleActiveCommlink";
            this.chkVehicleActiveCommlink.Size = new System.Drawing.Size(104, 17);
            this.chkVehicleActiveCommlink.TabIndex = 242;
            this.chkVehicleActiveCommlink.Tag = "Checkbox_ActiveCommlink";
            this.chkVehicleActiveCommlink.Text = "Active Commlink";
            this.chkVehicleActiveCommlink.UseVisualStyleBackColor = true;
            this.chkVehicleActiveCommlink.Visible = false;
            this.chkVehicleActiveCommlink.CheckedChanged += new System.EventHandler(this.chkVehicleActiveCommlink_CheckedChanged);
            // 
            // treVehicles
            // 
            this.treVehicles.AllowDrop = true;
            this.treVehicles.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treVehicles.HideSelection = false;
            this.treVehicles.Location = new System.Drawing.Point(3, 32);
            this.treVehicles.Name = "treVehicles";
            treeNode24.Name = "nodVehiclesRoot";
            treeNode24.Tag = "Node_SelectedVehicles";
            treeNode24.Text = "Selected Vehicles";
            this.treVehicles.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            treeNode24});
            this.treVehicles.ShowNodeToolTips = true;
            this.treVehicles.Size = new System.Drawing.Size(295, 590);
            this.treVehicles.TabIndex = 30;
            this.treVehicles.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treVehicles_AfterSelect);
            this.treVehicles.DragOver += new System.Windows.Forms.DragEventHandler(this.treVehicles_DragOver);
            this.treVehicles.KeyDown += new System.Windows.Forms.KeyEventHandler(this.treVehicles_KeyDown);
            this.treVehicles.MouseDown += new System.Windows.Forms.MouseEventHandler(this.TreeView_MouseDown);
            // 
            // tabCharacterInfo
            // 
            this.tabCharacterInfo.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.tabCharacterInfo.Controls.Add(this.tlpCharacterInfo);
            this.tabCharacterInfo.Location = new System.Drawing.Point(4, 22);
            this.tabCharacterInfo.Name = "tabCharacterInfo";
            this.tabCharacterInfo.Padding = new System.Windows.Forms.Padding(3);
            this.tabCharacterInfo.Size = new System.Drawing.Size(977, 631);
            this.tabCharacterInfo.TabIndex = 9;
            this.tabCharacterInfo.Tag = "Tab_CharacterInfo";
            this.tabCharacterInfo.Text = "Character Info";
            // 
            // tlpCharacterInfo
            // 
            this.tlpCharacterInfo.ColumnCount = 12;
            this.tlpCharacterInfo.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpCharacterInfo.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tlpCharacterInfo.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpCharacterInfo.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tlpCharacterInfo.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpCharacterInfo.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tlpCharacterInfo.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpCharacterInfo.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 13F));
            this.tlpCharacterInfo.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 7F));
            this.tlpCharacterInfo.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpCharacterInfo.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.tlpCharacterInfo.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.tlpCharacterInfo.Controls.Add(this.tlpLongTexts, 0, 2);
            this.tlpCharacterInfo.Controls.Add(this.picMugshot, 8, 12);
            this.tlpCharacterInfo.Controls.Add(this.chkIsMainMugshot, 8, 11);
            this.tlpCharacterInfo.Controls.Add(this.cboPrimaryArm, 10, 2);
            this.tlpCharacterInfo.Controls.Add(this.lblNumMugshots, 11, 9);
            this.tlpCharacterInfo.Controls.Add(this.lblSex, 0, 0);
            this.tlpCharacterInfo.Controls.Add(this.nudMugshotIndex, 10, 9);
            this.tlpCharacterInfo.Controls.Add(this.lblHandedness, 8, 2);
            this.tlpCharacterInfo.Controls.Add(this.btnCreateBackstory, 9, 8);
            this.tlpCharacterInfo.Controls.Add(this.txtSex, 1, 0);
            this.tlpCharacterInfo.Controls.Add(this.lblPublicAwareTotal, 11, 5);
            this.tlpCharacterInfo.Controls.Add(this.lblAge, 2, 0);
            this.tlpCharacterInfo.Controls.Add(this.lblNotorietyTotal, 11, 4);
            this.tlpCharacterInfo.Controls.Add(this.txtAge, 3, 0);
            this.tlpCharacterInfo.Controls.Add(this.lblStreetCredTotal, 11, 3);
            this.tlpCharacterInfo.Controls.Add(this.lblEyes, 4, 0);
            this.tlpCharacterInfo.Controls.Add(this.lblPublicAware, 8, 5);
            this.tlpCharacterInfo.Controls.Add(this.txtEyes, 5, 0);
            this.tlpCharacterInfo.Controls.Add(this.lblNotoriety, 8, 4);
            this.tlpCharacterInfo.Controls.Add(this.lblHair, 6, 0);
            this.tlpCharacterInfo.Controls.Add(this.lblStreetCred, 8, 3);
            this.tlpCharacterInfo.Controls.Add(this.txtHair, 7, 0);
            this.tlpCharacterInfo.Controls.Add(this.chkCharacterCreated, 9, 0);
            this.tlpCharacterInfo.Controls.Add(this.lblHeight, 0, 1);
            this.tlpCharacterInfo.Controls.Add(this.txtHeight, 1, 1);
            this.tlpCharacterInfo.Controls.Add(this.lblWeight, 2, 1);
            this.tlpCharacterInfo.Controls.Add(this.txtPlayerName, 10, 1);
            this.tlpCharacterInfo.Controls.Add(this.txtCharacterName, 7, 1);
            this.tlpCharacterInfo.Controls.Add(this.lblPlayerName, 9, 1);
            this.tlpCharacterInfo.Controls.Add(this.lblCharacterName, 6, 1);
            this.tlpCharacterInfo.Controls.Add(this.txtWeight, 3, 1);
            this.tlpCharacterInfo.Controls.Add(this.lblSkin, 4, 1);
            this.tlpCharacterInfo.Controls.Add(this.txtSkin, 5, 1);
            this.tlpCharacterInfo.Controls.Add(this.lblMugshot, 8, 9);
            this.tlpCharacterInfo.Controls.Add(this.lblAstralReputation, 8, 6);
            this.tlpCharacterInfo.Controls.Add(this.lblWildReputation, 8, 7);
            this.tlpCharacterInfo.Controls.Add(this.lblAstralReputationTotal, 11, 6);
            this.tlpCharacterInfo.Controls.Add(this.lblWildReputationTotal, 11, 7);
            this.tlpCharacterInfo.Controls.Add(this.tlpMugshotButtons, 8, 10);
            this.tlpCharacterInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpCharacterInfo.Location = new System.Drawing.Point(3, 3);
            this.tlpCharacterInfo.Name = "tlpCharacterInfo";
            this.tlpCharacterInfo.RowCount = 13;
            this.tlpCharacterInfo.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpCharacterInfo.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpCharacterInfo.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpCharacterInfo.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpCharacterInfo.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpCharacterInfo.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpCharacterInfo.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpCharacterInfo.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpCharacterInfo.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpCharacterInfo.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpCharacterInfo.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpCharacterInfo.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpCharacterInfo.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpCharacterInfo.Size = new System.Drawing.Size(971, 625);
            this.tlpCharacterInfo.TabIndex = 96;
            // 
            // tlpLongTexts
            // 
            this.tlpLongTexts.AutoScroll = true;
            this.tlpLongTexts.ColumnCount = 1;
            this.tlpCharacterInfo.SetColumnSpan(this.tlpLongTexts, 8);
            this.tlpLongTexts.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpLongTexts.Controls.Add(this.gpbDescription, 0, 0);
            this.tlpLongTexts.Controls.Add(this.gpbBackground, 0, 1);
            this.tlpLongTexts.Controls.Add(this.gpbConcept, 0, 2);
            this.tlpLongTexts.Controls.Add(this.gpbNotes, 0, 3);
            this.tlpLongTexts.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpLongTexts.Location = new System.Drawing.Point(0, 52);
            this.tlpLongTexts.Margin = new System.Windows.Forms.Padding(0);
            this.tlpLongTexts.Name = "tlpLongTexts";
            this.tlpLongTexts.RowCount = 4;
            this.tlpCharacterInfo.SetRowSpan(this.tlpLongTexts, 11);
            this.tlpLongTexts.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tlpLongTexts.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tlpLongTexts.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tlpLongTexts.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tlpLongTexts.Size = new System.Drawing.Size(723, 573);
            this.tlpLongTexts.TabIndex = 100;
            // 
            // gpbDescription
            // 
            this.gpbDescription.AutoSize = true;
            this.gpbDescription.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.gpbDescription.Controls.Add(this.rtfDescription);
            this.gpbDescription.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gpbDescription.Location = new System.Drawing.Point(3, 3);
            this.gpbDescription.Name = "gpbDescription";
            this.gpbDescription.Size = new System.Drawing.Size(717, 137);
            this.gpbDescription.TabIndex = 96;
            this.gpbDescription.TabStop = false;
            this.gpbDescription.Tag = "Label_Description";
            this.gpbDescription.Text = "Description:";
            // 
            // rtfDescription
            // 
            this.rtfDescription.AllowFormatting = true;
            this.rtfDescription.AutoSize = true;
            this.rtfDescription.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.rtfDescription.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rtfDescription.Location = new System.Drawing.Point(3, 16);
            this.rtfDescription.MinimumSize = new System.Drawing.Size(0, 60);
            this.rtfDescription.Name = "rtfDescription";
            this.rtfDescription.Rtf = "{\\rtf1\\ansi\\ansicpg1252\\deff0\\nouicompat\\deflang1033{\\fonttbl{\\f0\\fnil\\fcharset0 " +
    "Microsoft Sans Serif;}}\r\n{\\*\\generator Riched20 10.0.19041}\\viewkind4\\uc1 \r\n\\par" +
    "d\\f0\\fs17\\par\r\n}\r\n";
            this.rtfDescription.Size = new System.Drawing.Size(711, 118);
            this.rtfDescription.TabIndex = 0;
            // 
            // gpbBackground
            // 
            this.gpbBackground.AutoSize = true;
            this.gpbBackground.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.gpbBackground.Controls.Add(this.rtfBackground);
            this.gpbBackground.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gpbBackground.Location = new System.Drawing.Point(3, 146);
            this.gpbBackground.Name = "gpbBackground";
            this.gpbBackground.Size = new System.Drawing.Size(717, 137);
            this.gpbBackground.TabIndex = 97;
            this.gpbBackground.TabStop = false;
            this.gpbBackground.Tag = "Label_Background";
            this.gpbBackground.Text = "Background:";
            // 
            // rtfBackground
            // 
            this.rtfBackground.AllowFormatting = true;
            this.rtfBackground.AutoSize = true;
            this.rtfBackground.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.rtfBackground.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rtfBackground.Location = new System.Drawing.Point(3, 16);
            this.rtfBackground.MinimumSize = new System.Drawing.Size(0, 60);
            this.rtfBackground.Name = "rtfBackground";
            this.rtfBackground.Rtf = "{\\rtf1\\ansi\\ansicpg1252\\deff0\\nouicompat\\deflang1033{\\fonttbl{\\f0\\fnil\\fcharset0 " +
    "Microsoft Sans Serif;}}\r\n{\\*\\generator Riched20 10.0.19041}\\viewkind4\\uc1 \r\n\\par" +
    "d\\f0\\fs17\\par\r\n}\r\n";
            this.rtfBackground.Size = new System.Drawing.Size(711, 118);
            this.rtfBackground.TabIndex = 0;
            // 
            // gpbConcept
            // 
            this.gpbConcept.AutoSize = true;
            this.gpbConcept.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.gpbConcept.Controls.Add(this.rtfConcept);
            this.gpbConcept.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gpbConcept.Location = new System.Drawing.Point(3, 289);
            this.gpbConcept.Name = "gpbConcept";
            this.gpbConcept.Size = new System.Drawing.Size(717, 137);
            this.gpbConcept.TabIndex = 98;
            this.gpbConcept.TabStop = false;
            this.gpbConcept.Tag = "Label_Concept";
            this.gpbConcept.Text = "Concept:";
            // 
            // rtfConcept
            // 
            this.rtfConcept.AllowFormatting = true;
            this.rtfConcept.AutoSize = true;
            this.rtfConcept.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.rtfConcept.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rtfConcept.Location = new System.Drawing.Point(3, 16);
            this.rtfConcept.MinimumSize = new System.Drawing.Size(0, 60);
            this.rtfConcept.Name = "rtfConcept";
            this.rtfConcept.Rtf = "{\\rtf1\\ansi\\ansicpg1252\\deff0\\nouicompat\\deflang1033{\\fonttbl{\\f0\\fnil\\fcharset0 " +
    "Microsoft Sans Serif;}}\r\n{\\*\\generator Riched20 10.0.19041}\\viewkind4\\uc1 \r\n\\par" +
    "d\\f0\\fs17\\par\r\n}\r\n";
            this.rtfConcept.Size = new System.Drawing.Size(711, 118);
            this.rtfConcept.TabIndex = 0;
            // 
            // gpbNotes
            // 
            this.gpbNotes.AutoSize = true;
            this.gpbNotes.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.gpbNotes.Controls.Add(this.rtfNotes);
            this.gpbNotes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gpbNotes.Location = new System.Drawing.Point(3, 432);
            this.gpbNotes.Name = "gpbNotes";
            this.gpbNotes.Size = new System.Drawing.Size(717, 138);
            this.gpbNotes.TabIndex = 99;
            this.gpbNotes.TabStop = false;
            this.gpbNotes.Tag = "Label_Notes";
            this.gpbNotes.Text = "Notes:";
            // 
            // rtfNotes
            // 
            this.rtfNotes.AllowFormatting = true;
            this.rtfNotes.AutoSize = true;
            this.rtfNotes.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.rtfNotes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rtfNotes.Location = new System.Drawing.Point(3, 16);
            this.rtfNotes.MinimumSize = new System.Drawing.Size(0, 60);
            this.rtfNotes.Name = "rtfNotes";
            this.rtfNotes.Rtf = "{\\rtf1\\ansi\\ansicpg1252\\deff0\\nouicompat\\deflang1033{\\fonttbl{\\f0\\fnil\\fcharset0 " +
    "Microsoft Sans Serif;}}\r\n{\\*\\generator Riched20 10.0.19041}\\viewkind4\\uc1 \r\n\\par" +
    "d\\f0\\fs17\\par\r\n}\r\n";
            this.rtfNotes.Size = new System.Drawing.Size(711, 119);
            this.rtfNotes.TabIndex = 0;
            // 
            // picMugshot
            // 
            this.tlpCharacterInfo.SetColumnSpan(this.picMugshot, 4);
            this.picMugshot.Dock = System.Windows.Forms.DockStyle.Fill;
            this.picMugshot.Location = new System.Drawing.Point(726, 314);
            this.picMugshot.MinimumSize = new System.Drawing.Size(21, 31);
            this.picMugshot.Name = "picMugshot";
            this.picMugshot.Size = new System.Drawing.Size(242, 308);
            this.picMugshot.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picMugshot.TabIndex = 20;
            this.picMugshot.TabStop = false;
            this.picMugshot.SizeChanged += new System.EventHandler(this.picMugshot_SizeChanged);
            // 
            // chkIsMainMugshot
            // 
            this.chkIsMainMugshot.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkIsMainMugshot.AutoSize = true;
            this.tlpCharacterInfo.SetColumnSpan(this.chkIsMainMugshot, 4);
            this.chkIsMainMugshot.Location = new System.Drawing.Point(726, 291);
            this.chkIsMainMugshot.Name = "chkIsMainMugshot";
            this.chkIsMainMugshot.Size = new System.Drawing.Size(104, 17);
            this.chkIsMainMugshot.TabIndex = 95;
            this.chkIsMainMugshot.Tag = "Checkbox_IsMainMugshot";
            this.chkIsMainMugshot.Text = "Is Main Mugshot";
            this.chkIsMainMugshot.UseVisualStyleBackColor = true;
            this.chkIsMainMugshot.CheckedChanged += new System.EventHandler(this.chkIsMainMugshot_CheckedChanged);
            // 
            // cboPrimaryArm
            // 
            this.cboPrimaryArm.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tlpCharacterInfo.SetColumnSpan(this.cboPrimaryArm, 2);
            this.cboPrimaryArm.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboPrimaryArm.FormattingEnabled = true;
            this.cboPrimaryArm.Location = new System.Drawing.Point(823, 55);
            this.cboPrimaryArm.Name = "cboPrimaryArm";
            this.cboPrimaryArm.Size = new System.Drawing.Size(145, 21);
            this.cboPrimaryArm.TabIndex = 93;
            this.cboPrimaryArm.TooltipText = "";
            this.cboPrimaryArm.SelectedIndexChanged += new System.EventHandler(this.cboPrimaryArm_SelectedIndexChanged);
            // 
            // lblNumMugshots
            // 
            this.lblNumMugshots.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblNumMugshots.AutoSize = true;
            this.lblNumMugshots.Location = new System.Drawing.Point(897, 239);
            this.lblNumMugshots.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblNumMugshots.Name = "lblNumMugshots";
            this.lblNumMugshots.Size = new System.Drawing.Size(21, 13);
            this.lblNumMugshots.TabIndex = 94;
            this.lblNumMugshots.Tag = "";
            this.lblNumMugshots.Text = "/ 0";
            this.lblNumMugshots.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblSex
            // 
            this.lblSex.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblSex.AutoSize = true;
            this.lblSex.Location = new System.Drawing.Point(16, 6);
            this.lblSex.Name = "lblSex";
            this.lblSex.Size = new System.Drawing.Size(28, 13);
            this.lblSex.TabIndex = 0;
            this.lblSex.Tag = "Label_Sex";
            this.lblSex.Text = "Sex:";
            this.lblSex.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // nudMugshotIndex
            // 
            this.nudMugshotIndex.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.nudMugshotIndex.AutoSize = true;
            this.nudMugshotIndex.Location = new System.Drawing.Point(823, 236);
            this.nudMugshotIndex.Name = "nudMugshotIndex";
            this.nudMugshotIndex.Size = new System.Drawing.Size(68, 20);
            this.nudMugshotIndex.TabIndex = 93;
            this.nudMugshotIndex.ValueChanged += new System.EventHandler(this.nudMugshotIndex_ValueChanged);
            // 
            // lblHandedness
            // 
            this.lblHandedness.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblHandedness.AutoSize = true;
            this.tlpCharacterInfo.SetColumnSpan(this.lblHandedness, 2);
            this.lblHandedness.Location = new System.Drawing.Point(747, 59);
            this.lblHandedness.Name = "lblHandedness";
            this.lblHandedness.Size = new System.Drawing.Size(70, 13);
            this.lblHandedness.TabIndex = 92;
            this.lblHandedness.Tag = "Label_Handedness";
            this.lblHandedness.Text = "Handedness:";
            this.lblHandedness.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // btnCreateBackstory
            // 
            this.btnCreateBackstory.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.btnCreateBackstory.AutoSize = true;
            this.btnCreateBackstory.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpCharacterInfo.SetColumnSpan(this.btnCreateBackstory, 3);
            this.btnCreateBackstory.Location = new System.Drawing.Point(778, 207);
            this.btnCreateBackstory.Name = "btnCreateBackstory";
            this.btnCreateBackstory.Size = new System.Drawing.Size(98, 23);
            this.btnCreateBackstory.TabIndex = 91;
            this.btnCreateBackstory.Text = "Create Backstory";
            this.btnCreateBackstory.UseVisualStyleBackColor = true;
            this.btnCreateBackstory.Visible = false;
            this.btnCreateBackstory.Click += new System.EventHandler(this.btnCreateBackstory_Click);
            // 
            // txtSex
            // 
            this.txtSex.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.txtSex.Location = new System.Drawing.Point(50, 3);
            this.txtSex.Name = "txtSex";
            this.txtSex.Size = new System.Drawing.Size(143, 20);
            this.txtSex.TabIndex = 1;
            // 
            // lblPublicAwareTotal
            // 
            this.lblPublicAwareTotal.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblPublicAwareTotal.AutoSize = true;
            this.lblPublicAwareTotal.Location = new System.Drawing.Point(897, 135);
            this.lblPublicAwareTotal.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblPublicAwareTotal.Name = "lblPublicAwareTotal";
            this.lblPublicAwareTotal.Size = new System.Drawing.Size(19, 13);
            this.lblPublicAwareTotal.TabIndex = 90;
            this.lblPublicAwareTotal.Tag = "Label_StreetCred";
            this.lblPublicAwareTotal.Text = "[0]";
            this.lblPublicAwareTotal.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblPublicAwareTotal.ToolTipText = "";
            // 
            // lblAge
            // 
            this.lblAge.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblAge.AutoSize = true;
            this.lblAge.Location = new System.Drawing.Point(214, 6);
            this.lblAge.Name = "lblAge";
            this.lblAge.Size = new System.Drawing.Size(29, 13);
            this.lblAge.TabIndex = 2;
            this.lblAge.Tag = "Label_Age";
            this.lblAge.Text = "Age:";
            this.lblAge.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblNotorietyTotal
            // 
            this.lblNotorietyTotal.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblNotorietyTotal.AutoSize = true;
            this.lblNotorietyTotal.Location = new System.Drawing.Point(897, 110);
            this.lblNotorietyTotal.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblNotorietyTotal.Name = "lblNotorietyTotal";
            this.lblNotorietyTotal.Size = new System.Drawing.Size(19, 13);
            this.lblNotorietyTotal.TabIndex = 89;
            this.lblNotorietyTotal.Tag = "Label_StreetCred";
            this.lblNotorietyTotal.Text = "[0]";
            this.lblNotorietyTotal.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblNotorietyTotal.ToolTipText = "";
            // 
            // txtAge
            // 
            this.txtAge.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.txtAge.Location = new System.Drawing.Point(249, 3);
            this.txtAge.Name = "txtAge";
            this.txtAge.Size = new System.Drawing.Size(143, 20);
            this.txtAge.TabIndex = 3;
            // 
            // lblStreetCredTotal
            // 
            this.lblStreetCredTotal.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblStreetCredTotal.AutoSize = true;
            this.lblStreetCredTotal.Location = new System.Drawing.Point(897, 85);
            this.lblStreetCredTotal.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblStreetCredTotal.Name = "lblStreetCredTotal";
            this.lblStreetCredTotal.Size = new System.Drawing.Size(19, 13);
            this.lblStreetCredTotal.TabIndex = 88;
            this.lblStreetCredTotal.Tag = "";
            this.lblStreetCredTotal.Text = "[0]";
            this.lblStreetCredTotal.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblStreetCredTotal.ToolTipText = "";
            // 
            // lblEyes
            // 
            this.lblEyes.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblEyes.AutoSize = true;
            this.lblEyes.Location = new System.Drawing.Point(398, 6);
            this.lblEyes.Name = "lblEyes";
            this.lblEyes.Size = new System.Drawing.Size(33, 13);
            this.lblEyes.TabIndex = 4;
            this.lblEyes.Tag = "Label_Eyes";
            this.lblEyes.Text = "Eyes:";
            this.lblEyes.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtEyes
            // 
            this.txtEyes.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.txtEyes.Location = new System.Drawing.Point(437, 3);
            this.txtEyes.Name = "txtEyes";
            this.txtEyes.Size = new System.Drawing.Size(143, 20);
            this.txtEyes.TabIndex = 5;
            // 
            // lblHair
            // 
            this.lblHair.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblHair.AutoSize = true;
            this.lblHair.Location = new System.Drawing.Point(595, 6);
            this.lblHair.Name = "lblHair";
            this.lblHair.Size = new System.Drawing.Size(29, 13);
            this.lblHair.TabIndex = 6;
            this.lblHair.Tag = "Label_Hair";
            this.lblHair.Text = "Hair:";
            this.lblHair.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtHair
            // 
            this.txtHair.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tlpCharacterInfo.SetColumnSpan(this.txtHair, 2);
            this.txtHair.Location = new System.Drawing.Point(630, 3);
            this.txtHair.Name = "txtHair";
            this.txtHair.Size = new System.Drawing.Size(142, 20);
            this.txtHair.TabIndex = 7;
            // 
            // lblHeight
            // 
            this.lblHeight.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblHeight.AutoSize = true;
            this.lblHeight.Location = new System.Drawing.Point(3, 32);
            this.lblHeight.Name = "lblHeight";
            this.lblHeight.Size = new System.Drawing.Size(41, 13);
            this.lblHeight.TabIndex = 8;
            this.lblHeight.Tag = "Label_Height";
            this.lblHeight.Text = "Height:";
            this.lblHeight.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtHeight
            // 
            this.txtHeight.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.txtHeight.Location = new System.Drawing.Point(50, 29);
            this.txtHeight.Name = "txtHeight";
            this.txtHeight.Size = new System.Drawing.Size(143, 20);
            this.txtHeight.TabIndex = 9;
            // 
            // lblWeight
            // 
            this.lblWeight.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblWeight.AutoSize = true;
            this.lblWeight.Location = new System.Drawing.Point(199, 32);
            this.lblWeight.Name = "lblWeight";
            this.lblWeight.Size = new System.Drawing.Size(44, 13);
            this.lblWeight.TabIndex = 10;
            this.lblWeight.Tag = "Label_Weight";
            this.lblWeight.Text = "Weight:";
            this.lblWeight.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtPlayerName
            // 
            this.txtPlayerName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tlpCharacterInfo.SetColumnSpan(this.txtPlayerName, 2);
            this.txtPlayerName.Location = new System.Drawing.Point(823, 29);
            this.txtPlayerName.Name = "txtPlayerName";
            this.txtPlayerName.Size = new System.Drawing.Size(145, 20);
            this.txtPlayerName.TabIndex = 64;
            // 
            // txtCharacterName
            // 
            this.txtCharacterName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tlpCharacterInfo.SetColumnSpan(this.txtCharacterName, 2);
            this.txtCharacterName.Location = new System.Drawing.Point(630, 29);
            this.txtCharacterName.Name = "txtCharacterName";
            this.txtCharacterName.Size = new System.Drawing.Size(142, 20);
            this.txtCharacterName.TabIndex = 66;
            // 
            // lblPlayerName
            // 
            this.lblPlayerName.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblPlayerName.AutoSize = true;
            this.lblPlayerName.Location = new System.Drawing.Point(778, 32);
            this.lblPlayerName.Name = "lblPlayerName";
            this.lblPlayerName.Size = new System.Drawing.Size(39, 13);
            this.lblPlayerName.TabIndex = 63;
            this.lblPlayerName.Tag = "Label_Player";
            this.lblPlayerName.Text = "Player:";
            this.lblPlayerName.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblCharacterName
            // 
            this.lblCharacterName.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblCharacterName.AutoSize = true;
            this.lblCharacterName.Location = new System.Drawing.Point(586, 32);
            this.lblCharacterName.Name = "lblCharacterName";
            this.lblCharacterName.Size = new System.Drawing.Size(38, 13);
            this.lblCharacterName.TabIndex = 65;
            this.lblCharacterName.Tag = "Label_CharacterName";
            this.lblCharacterName.Text = "Name:";
            this.lblCharacterName.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtWeight
            // 
            this.txtWeight.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.txtWeight.Location = new System.Drawing.Point(249, 29);
            this.txtWeight.Name = "txtWeight";
            this.txtWeight.Size = new System.Drawing.Size(143, 20);
            this.txtWeight.TabIndex = 11;
            // 
            // lblSkin
            // 
            this.lblSkin.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblSkin.AutoSize = true;
            this.lblSkin.Location = new System.Drawing.Point(400, 32);
            this.lblSkin.Name = "lblSkin";
            this.lblSkin.Size = new System.Drawing.Size(31, 13);
            this.lblSkin.TabIndex = 12;
            this.lblSkin.Tag = "Label_Skin";
            this.lblSkin.Text = "Skin:";
            this.lblSkin.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtSkin
            // 
            this.txtSkin.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.txtSkin.Location = new System.Drawing.Point(437, 29);
            this.txtSkin.Name = "txtSkin";
            this.txtSkin.Size = new System.Drawing.Size(143, 20);
            this.txtSkin.TabIndex = 13;
            // 
            // lblMugshot
            // 
            this.lblMugshot.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblMugshot.AutoSize = true;
            this.tlpCharacterInfo.SetColumnSpan(this.lblMugshot, 2);
            this.lblMugshot.Location = new System.Drawing.Point(766, 239);
            this.lblMugshot.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblMugshot.Name = "lblMugshot";
            this.lblMugshot.Size = new System.Drawing.Size(51, 13);
            this.lblMugshot.TabIndex = 21;
            this.lblMugshot.Tag = "Label_Mugshot";
            this.lblMugshot.Text = "Mugshot:";
            this.lblMugshot.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblAstralReputation
            // 
            this.lblAstralReputation.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblAstralReputation.AutoSize = true;
            this.tlpCharacterInfo.SetColumnSpan(this.lblAstralReputation, 3);
            this.lblAstralReputation.Location = new System.Drawing.Point(800, 160);
            this.lblAstralReputation.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblAstralReputation.Name = "lblAstralReputation";
            this.lblAstralReputation.Size = new System.Drawing.Size(91, 13);
            this.lblAstralReputation.TabIndex = 102;
            this.lblAstralReputation.Tag = "Label_AstralReputation";
            this.lblAstralReputation.Text = "Astral Reputation:";
            this.lblAstralReputation.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblWildReputation
            // 
            this.lblWildReputation.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblWildReputation.AutoSize = true;
            this.tlpCharacterInfo.SetColumnSpan(this.lblWildReputation, 3);
            this.lblWildReputation.Location = new System.Drawing.Point(805, 185);
            this.lblWildReputation.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblWildReputation.Name = "lblWildReputation";
            this.lblWildReputation.Size = new System.Drawing.Size(86, 13);
            this.lblWildReputation.TabIndex = 103;
            this.lblWildReputation.Tag = "Label_PublicAwareness";
            this.lblWildReputation.Text = "Wild Reputation:";
            this.lblWildReputation.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblAstralReputationTotal
            // 
            this.lblAstralReputationTotal.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblAstralReputationTotal.AutoSize = true;
            this.lblAstralReputationTotal.Location = new System.Drawing.Point(897, 160);
            this.lblAstralReputationTotal.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblAstralReputationTotal.Name = "lblAstralReputationTotal";
            this.lblAstralReputationTotal.Size = new System.Drawing.Size(19, 13);
            this.lblAstralReputationTotal.TabIndex = 104;
            this.lblAstralReputationTotal.Text = "[0]";
            this.lblAstralReputationTotal.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblAstralReputationTotal.ToolTipText = "";
            // 
            // lblWildReputationTotal
            // 
            this.lblWildReputationTotal.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblWildReputationTotal.AutoSize = true;
            this.lblWildReputationTotal.Location = new System.Drawing.Point(897, 185);
            this.lblWildReputationTotal.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblWildReputationTotal.Name = "lblWildReputationTotal";
            this.lblWildReputationTotal.Size = new System.Drawing.Size(19, 13);
            this.lblWildReputationTotal.TabIndex = 105;
            this.lblWildReputationTotal.Text = "[0]";
            this.lblWildReputationTotal.ToolTipText = "";
            // 
            // tlpMugshotButtons
            // 
            this.tlpMugshotButtons.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.tlpMugshotButtons.AutoSize = true;
            this.tlpMugshotButtons.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpMugshotButtons.ColumnCount = 2;
            this.tlpCharacterInfo.SetColumnSpan(this.tlpMugshotButtons, 4);
            this.tlpMugshotButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpMugshotButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpMugshotButtons.Controls.Add(this.cmdAddMugshot, 0, 0);
            this.tlpMugshotButtons.Controls.Add(this.cmdDeleteMugshot, 1, 0);
            this.tlpMugshotButtons.Location = new System.Drawing.Point(793, 259);
            this.tlpMugshotButtons.Margin = new System.Windows.Forms.Padding(0);
            this.tlpMugshotButtons.Name = "tlpMugshotButtons";
            this.tlpMugshotButtons.RowCount = 1;
            this.tlpMugshotButtons.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpMugshotButtons.Size = new System.Drawing.Size(108, 29);
            this.tlpMugshotButtons.TabIndex = 106;
            // 
            // cmdAddMugshot
            // 
            this.cmdAddMugshot.AutoSize = true;
            this.cmdAddMugshot.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdAddMugshot.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmdAddMugshot.Location = new System.Drawing.Point(3, 3);
            this.cmdAddMugshot.Name = "cmdAddMugshot";
            this.cmdAddMugshot.Size = new System.Drawing.Size(48, 23);
            this.cmdAddMugshot.TabIndex = 22;
            this.cmdAddMugshot.Tag = "Button_AddMugshot";
            this.cmdAddMugshot.Text = "Add";
            this.cmdAddMugshot.UseVisualStyleBackColor = true;
            this.cmdAddMugshot.Click += new System.EventHandler(this.cmdAddMugshot_Click);
            // 
            // cmdDeleteMugshot
            // 
            this.cmdDeleteMugshot.AutoSize = true;
            this.cmdDeleteMugshot.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdDeleteMugshot.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmdDeleteMugshot.Location = new System.Drawing.Point(57, 3);
            this.cmdDeleteMugshot.Name = "cmdDeleteMugshot";
            this.cmdDeleteMugshot.Size = new System.Drawing.Size(48, 23);
            this.cmdDeleteMugshot.TabIndex = 23;
            this.cmdDeleteMugshot.Tag = "String_Delete";
            this.cmdDeleteMugshot.Text = "Delete";
            this.cmdDeleteMugshot.UseVisualStyleBackColor = true;
            this.cmdDeleteMugshot.Click += new System.EventHandler(this.cmdDeleteMugshot_Click);
            // 
            // tabRelationships
            // 
            this.tabRelationships.Controls.Add(this.tabPeople);
            this.tabRelationships.Location = new System.Drawing.Point(4, 22);
            this.tabRelationships.Name = "tabRelationships";
            this.tabRelationships.Size = new System.Drawing.Size(977, 631);
            this.tabRelationships.TabIndex = 16;
            this.tabRelationships.Tag = "String_Relationships";
            this.tabRelationships.Text = "Relationships";
            this.tabRelationships.UseVisualStyleBackColor = true;
            // 
            // tabPeople
            // 
            this.tabPeople.Controls.Add(this.tabContacts);
            this.tabPeople.Controls.Add(this.tabEnemies);
            this.tabPeople.Controls.Add(this.tabPets);
            this.tabPeople.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabPeople.Location = new System.Drawing.Point(0, 0);
            this.tabPeople.Name = "tabPeople";
            this.tabPeople.SelectedIndex = 0;
            this.tabPeople.Size = new System.Drawing.Size(977, 631);
            this.tabPeople.TabIndex = 94;
            // 
            // tabContacts
            // 
            this.tabContacts.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.tabContacts.Controls.Add(this.tlpContacts);
            this.tabContacts.Location = new System.Drawing.Point(4, 22);
            this.tabContacts.Name = "tabContacts";
            this.tabContacts.Padding = new System.Windows.Forms.Padding(3);
            this.tabContacts.Size = new System.Drawing.Size(969, 605);
            this.tabContacts.TabIndex = 0;
            this.tabContacts.Tag = "Label_Contacts";
            this.tabContacts.Text = "Contacts";
            // 
            // tlpContacts
            // 
            this.tlpContacts.AutoSize = true;
            this.tlpContacts.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpContacts.ColumnCount = 4;
            this.tlpContacts.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 28F));
            this.tlpContacts.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 128F));
            this.tlpContacts.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 128F));
            this.tlpContacts.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpContacts.Controls.Add(this.panContacts, 0, 2);
            this.tlpContacts.Controls.Add(this.lblContactArchtypeLabel, 3, 1);
            this.tlpContacts.Controls.Add(this.lblContactLocationLabel, 2, 1);
            this.tlpContacts.Controls.Add(this.lblContactNameLabel, 1, 1);
            this.tlpContacts.Controls.Add(this.tlpContactsButtons, 0, 0);
            this.tlpContacts.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpContacts.Location = new System.Drawing.Point(3, 3);
            this.tlpContacts.Name = "tlpContacts";
            this.tlpContacts.RowCount = 3;
            this.tlpContacts.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpContacts.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpContacts.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpContacts.Size = new System.Drawing.Size(963, 599);
            this.tlpContacts.TabIndex = 52;
            // 
            // panContacts
            // 
            this.panContacts.AllowDrop = true;
            this.panContacts.AutoScroll = true;
            this.panContacts.AutoSize = true;
            this.panContacts.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpContacts.SetColumnSpan(this.panContacts, 4);
            this.panContacts.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panContacts.Location = new System.Drawing.Point(3, 51);
            this.panContacts.Name = "panContacts";
            this.panContacts.Size = new System.Drawing.Size(957, 545);
            this.panContacts.TabIndex = 25;
            this.panContacts.Click += new System.EventHandler(this.panContacts_Click);
            this.panContacts.DragDrop += new System.Windows.Forms.DragEventHandler(this.panContacts_DragDrop);
            this.panContacts.DragEnter += new System.Windows.Forms.DragEventHandler(this.panContacts_DragEnter);
            this.panContacts.DragOver += new System.Windows.Forms.DragEventHandler(this.panContacts_DragOver);
            // 
            // lblContactArchtypeLabel
            // 
            this.lblContactArchtypeLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblContactArchtypeLabel.AutoSize = true;
            this.lblContactArchtypeLabel.Location = new System.Drawing.Point(287, 32);
            this.lblContactArchtypeLabel.Margin = new System.Windows.Forms.Padding(3);
            this.lblContactArchtypeLabel.Name = "lblContactArchtypeLabel";
            this.lblContactArchtypeLabel.Size = new System.Drawing.Size(52, 13);
            this.lblContactArchtypeLabel.TabIndex = 44;
            this.lblContactArchtypeLabel.Tag = "Label_Archetype";
            this.lblContactArchtypeLabel.Text = "Archtype:";
            // 
            // lblContactLocationLabel
            // 
            this.lblContactLocationLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblContactLocationLabel.AutoSize = true;
            this.lblContactLocationLabel.Location = new System.Drawing.Point(159, 32);
            this.lblContactLocationLabel.Margin = new System.Windows.Forms.Padding(3);
            this.lblContactLocationLabel.Name = "lblContactLocationLabel";
            this.lblContactLocationLabel.Size = new System.Drawing.Size(51, 13);
            this.lblContactLocationLabel.TabIndex = 43;
            this.lblContactLocationLabel.Tag = "Label_Location";
            this.lblContactLocationLabel.Text = "Location:";
            // 
            // lblContactNameLabel
            // 
            this.lblContactNameLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblContactNameLabel.AutoSize = true;
            this.lblContactNameLabel.Location = new System.Drawing.Point(31, 32);
            this.lblContactNameLabel.Margin = new System.Windows.Forms.Padding(3);
            this.lblContactNameLabel.Name = "lblContactNameLabel";
            this.lblContactNameLabel.Size = new System.Drawing.Size(38, 13);
            this.lblContactNameLabel.TabIndex = 42;
            this.lblContactNameLabel.Tag = "Label_Name";
            this.lblContactNameLabel.Text = "Name:";
            // 
            // tlpContactsButtons
            // 
            this.tlpContactsButtons.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.tlpContactsButtons.AutoSize = true;
            this.tlpContactsButtons.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpContactsButtons.ColumnCount = 5;
            this.tlpContacts.SetColumnSpan(this.tlpContactsButtons, 4);
            this.tlpContactsButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tlpContactsButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tlpContactsButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tlpContactsButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpContactsButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpContactsButtons.Controls.Add(this.cmdAddContact, 0, 0);
            this.tlpContactsButtons.Controls.Add(this.lblContactPoints, 4, 0);
            this.tlpContactsButtons.Controls.Add(this.cmdContactsExpansionToggle, 1, 0);
            this.tlpContactsButtons.Controls.Add(this.lblContactPoints_Label, 3, 0);
            this.tlpContactsButtons.Controls.Add(this.cmdSwapContactOrder, 2, 0);
            this.tlpContactsButtons.Location = new System.Drawing.Point(0, 0);
            this.tlpContactsButtons.Margin = new System.Windows.Forms.Padding(0);
            this.tlpContactsButtons.Name = "tlpContactsButtons";
            this.tlpContactsButtons.RowCount = 1;
            this.tlpContactsButtons.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpContactsButtons.Size = new System.Drawing.Size(541, 29);
            this.tlpContactsButtons.TabIndex = 52;
            // 
            // cmdAddContact
            // 
            this.cmdAddContact.AutoSize = true;
            this.cmdAddContact.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdAddContact.ContextMenuStrip = this.cmsAddContact;
            this.cmdAddContact.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmdAddContact.Location = new System.Drawing.Point(3, 3);
            this.cmdAddContact.Name = "cmdAddContact";
            this.cmdAddContact.Size = new System.Drawing.Size(112, 23);
            this.cmdAddContact.SplitMenuStrip = this.cmsAddContact;
            this.cmdAddContact.TabIndex = 24;
            this.cmdAddContact.Tag = "Button_AddContact";
            this.cmdAddContact.Text = "&Add Contact";
            this.cmdAddContact.UseVisualStyleBackColor = true;
            this.cmdAddContact.Click += new System.EventHandler(this.cmdAddContact_Click);
            // 
            // cmsAddContact
            // 
            this.cmsAddContact.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsAddFromFile});
            this.cmsAddContact.Name = "cmsAddContact";
            this.cmsAddContact.Size = new System.Drawing.Size(149, 26);
            // 
            // tsAddFromFile
            // 
            this.tsAddFromFile.Name = "tsAddFromFile";
            this.tsAddFromFile.Size = new System.Drawing.Size(148, 22);
            this.tsAddFromFile.Tag = "Menu_AddFromFile";
            this.tsAddFromFile.Text = "&Add From File";
            this.tsAddFromFile.Click += new System.EventHandler(this.tsAddFromFile_Click);
            // 
            // lblContactPoints
            // 
            this.lblContactPoints.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblContactPoints.AutoSize = true;
            this.lblContactPoints.Location = new System.Drawing.Point(519, 8);
            this.lblContactPoints.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblContactPoints.Name = "lblContactPoints";
            this.lblContactPoints.Size = new System.Drawing.Size(19, 13);
            this.lblContactPoints.TabIndex = 48;
            this.lblContactPoints.Text = "[0]";
            // 
            // cmdContactsExpansionToggle
            // 
            this.cmdContactsExpansionToggle.AutoSize = true;
            this.cmdContactsExpansionToggle.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdContactsExpansionToggle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmdContactsExpansionToggle.Location = new System.Drawing.Point(121, 3);
            this.cmdContactsExpansionToggle.Name = "cmdContactsExpansionToggle";
            this.cmdContactsExpansionToggle.Size = new System.Drawing.Size(112, 23);
            this.cmdContactsExpansionToggle.TabIndex = 49;
            this.cmdContactsExpansionToggle.Tag = "Button_ContactsExpansionToggle";
            this.cmdContactsExpansionToggle.Text = "&Expand/Collapse All";
            this.cmdContactsExpansionToggle.UseVisualStyleBackColor = true;
            this.cmdContactsExpansionToggle.Click += new System.EventHandler(this.cmdContactsExpansionToggle_Click);
            // 
            // lblContactPoints_Label
            // 
            this.lblContactPoints_Label.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblContactPoints_Label.AutoSize = true;
            this.lblContactPoints_Label.Location = new System.Drawing.Point(357, 8);
            this.lblContactPoints_Label.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblContactPoints_Label.Name = "lblContactPoints_Label";
            this.lblContactPoints_Label.Size = new System.Drawing.Size(156, 13);
            this.lblContactPoints_Label.TabIndex = 47;
            this.lblContactPoints_Label.Tag = "Label_FreeContactPoints";
            this.lblContactPoints_Label.Text = "Free Contact Points Remaining:";
            // 
            // cmdSwapContactOrder
            // 
            this.cmdSwapContactOrder.AutoSize = true;
            this.cmdSwapContactOrder.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdSwapContactOrder.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmdSwapContactOrder.Location = new System.Drawing.Point(239, 3);
            this.cmdSwapContactOrder.Name = "cmdSwapContactOrder";
            this.cmdSwapContactOrder.Size = new System.Drawing.Size(112, 23);
            this.cmdSwapContactOrder.TabIndex = 50;
            this.cmdSwapContactOrder.Tag = "Button_SwapOrdering";
            this.cmdSwapContactOrder.Text = "&Swap Ordering";
            this.cmdSwapContactOrder.UseVisualStyleBackColor = true;
            this.cmdSwapContactOrder.Click += new System.EventHandler(this.cmdSwapContactOrder_Click);
            // 
            // tabEnemies
            // 
            this.tabEnemies.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.tabEnemies.Controls.Add(this.tlpEnemies);
            this.tabEnemies.Location = new System.Drawing.Point(4, 22);
            this.tabEnemies.Name = "tabEnemies";
            this.tabEnemies.Padding = new System.Windows.Forms.Padding(3);
            this.tabEnemies.Size = new System.Drawing.Size(184, 48);
            this.tabEnemies.TabIndex = 1;
            this.tabEnemies.Tag = "Label_Enemies";
            this.tabEnemies.Text = "Enemies";
            // 
            // tlpEnemies
            // 
            this.tlpEnemies.AutoSize = true;
            this.tlpEnemies.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpEnemies.ColumnCount = 4;
            this.tlpEnemies.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 28F));
            this.tlpEnemies.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 128F));
            this.tlpEnemies.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 128F));
            this.tlpEnemies.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpEnemies.Controls.Add(this.panEnemies, 0, 2);
            this.tlpEnemies.Controls.Add(this.flpEnemiesButtons, 0, 0);
            this.tlpEnemies.Controls.Add(this.lblEnemyArchetypeLabel, 3, 1);
            this.tlpEnemies.Controls.Add(this.lblEnemyLocationLabel, 2, 1);
            this.tlpEnemies.Controls.Add(this.lblEnemyNameLabel, 1, 1);
            this.tlpEnemies.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpEnemies.Location = new System.Drawing.Point(3, 3);
            this.tlpEnemies.Name = "tlpEnemies";
            this.tlpEnemies.RowCount = 3;
            this.tlpEnemies.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpEnemies.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpEnemies.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpEnemies.Size = new System.Drawing.Size(178, 42);
            this.tlpEnemies.TabIndex = 50;
            // 
            // panEnemies
            // 
            this.panEnemies.AutoScroll = true;
            this.panEnemies.AutoSize = true;
            this.panEnemies.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpEnemies.SetColumnSpan(this.panEnemies, 4);
            this.panEnemies.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panEnemies.Location = new System.Drawing.Point(3, 51);
            this.panEnemies.Name = "panEnemies";
            this.panEnemies.Size = new System.Drawing.Size(172, 1);
            this.panEnemies.TabIndex = 41;
            this.panEnemies.Click += new System.EventHandler(this.panEnemies_Click);
            // 
            // flpEnemiesButtons
            // 
            this.flpEnemiesButtons.AutoSize = true;
            this.flpEnemiesButtons.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpEnemies.SetColumnSpan(this.flpEnemiesButtons, 4);
            this.flpEnemiesButtons.Controls.Add(this.cmdAddEnemy);
            this.flpEnemiesButtons.Location = new System.Drawing.Point(0, 0);
            this.flpEnemiesButtons.Margin = new System.Windows.Forms.Padding(0);
            this.flpEnemiesButtons.Name = "flpEnemiesButtons";
            this.flpEnemiesButtons.Size = new System.Drawing.Size(95, 29);
            this.flpEnemiesButtons.TabIndex = 50;
            this.flpEnemiesButtons.WrapContents = false;
            // 
            // cmdAddEnemy
            // 
            this.cmdAddEnemy.AutoSize = true;
            this.cmdAddEnemy.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdAddEnemy.ContextMenuStrip = this.cmsAddContact;
            this.cmdAddEnemy.Location = new System.Drawing.Point(3, 3);
            this.cmdAddEnemy.Name = "cmdAddEnemy";
            this.cmdAddEnemy.Size = new System.Drawing.Size(89, 23);
            this.cmdAddEnemy.SplitMenuStrip = this.cmsAddContact;
            this.cmdAddEnemy.TabIndex = 40;
            this.cmdAddEnemy.Tag = "Button_AddEnemy";
            this.cmdAddEnemy.Text = "A&dd Enemy";
            this.cmdAddEnemy.UseVisualStyleBackColor = true;
            this.cmdAddEnemy.Click += new System.EventHandler(this.cmdAddEnemy_Click);
            // 
            // lblEnemyArchetypeLabel
            // 
            this.lblEnemyArchetypeLabel.AutoSize = true;
            this.lblEnemyArchetypeLabel.Location = new System.Drawing.Point(287, 32);
            this.lblEnemyArchetypeLabel.Margin = new System.Windows.Forms.Padding(3);
            this.lblEnemyArchetypeLabel.Name = "lblEnemyArchetypeLabel";
            this.lblEnemyArchetypeLabel.Size = new System.Drawing.Size(1, 13);
            this.lblEnemyArchetypeLabel.TabIndex = 49;
            this.lblEnemyArchetypeLabel.Tag = "Label_Archetype";
            this.lblEnemyArchetypeLabel.Text = "Archtype:";
            // 
            // lblEnemyLocationLabel
            // 
            this.lblEnemyLocationLabel.AutoSize = true;
            this.lblEnemyLocationLabel.Location = new System.Drawing.Point(159, 32);
            this.lblEnemyLocationLabel.Margin = new System.Windows.Forms.Padding(3);
            this.lblEnemyLocationLabel.Name = "lblEnemyLocationLabel";
            this.lblEnemyLocationLabel.Size = new System.Drawing.Size(51, 13);
            this.lblEnemyLocationLabel.TabIndex = 48;
            this.lblEnemyLocationLabel.Tag = "Label_Location";
            this.lblEnemyLocationLabel.Text = "Location:";
            // 
            // lblEnemyNameLabel
            // 
            this.lblEnemyNameLabel.AutoSize = true;
            this.lblEnemyNameLabel.Location = new System.Drawing.Point(31, 32);
            this.lblEnemyNameLabel.Margin = new System.Windows.Forms.Padding(3);
            this.lblEnemyNameLabel.Name = "lblEnemyNameLabel";
            this.lblEnemyNameLabel.Size = new System.Drawing.Size(38, 13);
            this.lblEnemyNameLabel.TabIndex = 47;
            this.lblEnemyNameLabel.Tag = "Label_Name";
            this.lblEnemyNameLabel.Text = "Name:";
            // 
            // tabPets
            // 
            this.tabPets.BackColor = System.Drawing.SystemColors.Control;
            this.tabPets.Controls.Add(this.tlpPets);
            this.tabPets.Location = new System.Drawing.Point(4, 22);
            this.tabPets.Name = "tabPets";
            this.tabPets.Padding = new System.Windows.Forms.Padding(3);
            this.tabPets.Size = new System.Drawing.Size(184, 48);
            this.tabPets.TabIndex = 4;
            this.tabPets.Tag = "Tab_Pets";
            this.tabPets.Text = "Pets and Cohorts";
            // 
            // tlpPets
            // 
            this.tlpPets.AutoSize = true;
            this.tlpPets.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpPets.ColumnCount = 1;
            this.tlpPets.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpPets.Controls.Add(this.panPets, 0, 1);
            this.tlpPets.Controls.Add(this.flpPetsButtons, 0, 0);
            this.tlpPets.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpPets.Location = new System.Drawing.Point(3, 3);
            this.tlpPets.Name = "tlpPets";
            this.tlpPets.RowCount = 2;
            this.tlpPets.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpPets.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpPets.Size = new System.Drawing.Size(178, 42);
            this.tlpPets.TabIndex = 25;
            // 
            // panPets
            // 
            this.panPets.AutoScroll = true;
            this.panPets.AutoSize = true;
            this.panPets.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.panPets.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panPets.Location = new System.Drawing.Point(0, 29);
            this.panPets.Margin = new System.Windows.Forms.Padding(0);
            this.panPets.Name = "panPets";
            this.panPets.Size = new System.Drawing.Size(178, 13);
            this.panPets.TabIndex = 24;
            // 
            // flpPetsButtons
            // 
            this.flpPetsButtons.AutoSize = true;
            this.flpPetsButtons.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flpPetsButtons.Controls.Add(this.cmdAddPet);
            this.flpPetsButtons.Location = new System.Drawing.Point(0, 0);
            this.flpPetsButtons.Margin = new System.Windows.Forms.Padding(0);
            this.flpPetsButtons.Name = "flpPetsButtons";
            this.flpPetsButtons.Size = new System.Drawing.Size(79, 29);
            this.flpPetsButtons.TabIndex = 25;
            this.flpPetsButtons.WrapContents = false;
            // 
            // cmdAddPet
            // 
            this.cmdAddPet.AutoSize = true;
            this.cmdAddPet.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdAddPet.ContextMenuStrip = this.cmsAddContact;
            this.cmdAddPet.Location = new System.Drawing.Point(3, 3);
            this.cmdAddPet.Name = "cmdAddPet";
            this.cmdAddPet.Size = new System.Drawing.Size(73, 23);
            this.cmdAddPet.SplitMenuStrip = this.cmsAddContact;
            this.cmdAddPet.TabIndex = 23;
            this.cmdAddPet.Tag = "Button_AddPet";
            this.cmdAddPet.Text = "&Add Pet";
            this.cmdAddPet.UseVisualStyleBackColor = true;
            this.cmdAddPet.Click += new System.EventHandler(this.cmdAddPet_Click);
            // 
            // tabInfo
            // 
            this.tabInfo.Controls.Add(this.tabBPSummary);
            this.tabInfo.Controls.Add(this.tabOtherInfo);
            this.tabInfo.Controls.Add(this.tabDefenses);
            this.tabInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabInfo.Location = new System.Drawing.Point(0, 0);
            this.tabInfo.Name = "tabInfo";
            this.tabInfo.SelectedIndex = 0;
            this.tabInfo.Size = new System.Drawing.Size(275, 657);
            this.tabInfo.TabIndex = 50;
            // 
            // tabBPSummary
            // 
            this.tabBPSummary.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.tabBPSummary.Controls.Add(this.tlpKarmaSummary);
            this.tabBPSummary.Location = new System.Drawing.Point(4, 22);
            this.tabBPSummary.Name = "tabBPSummary";
            this.tabBPSummary.Padding = new System.Windows.Forms.Padding(3);
            this.tabBPSummary.Size = new System.Drawing.Size(267, 631);
            this.tabBPSummary.TabIndex = 0;
            this.tabBPSummary.Tag = "Tab_BPSummary";
            this.tabBPSummary.Text = "Karma Summary";
            // 
            // tlpKarmaSummary
            // 
            this.tlpKarmaSummary.AutoScroll = true;
            this.tlpKarmaSummary.AutoSize = true;
            this.tlpKarmaSummary.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpKarmaSummary.ColumnCount = 2;
            this.tlpKarmaSummary.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 55F));
            this.tlpKarmaSummary.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 45F));
            this.tlpKarmaSummary.Controls.Add(this.lblMetagenicQualities, 1, 5);
            this.tlpKarmaSummary.Controls.Add(this.lblMetagenicQualitiesLabel, 0, 5);
            this.tlpKarmaSummary.Controls.Add(this.lblAINormalProgramsBP, 1, 21);
            this.tlpKarmaSummary.Controls.Add(this.lblAIAdvancedProgramsBP, 1, 22);
            this.tlpKarmaSummary.Controls.Add(this.lblBuildAIAdvancedPrograms, 0, 22);
            this.tlpKarmaSummary.Controls.Add(this.lblBuildRitualsBP, 1, 14);
            this.tlpKarmaSummary.Controls.Add(this.lblBuildAINormalPrograms, 0, 21);
            this.tlpKarmaSummary.Controls.Add(this.lblBuildRitualsBPLabel, 0, 14);
            this.tlpKarmaSummary.Controls.Add(this.lblPBuildSpecial, 1, 2);
            this.tlpKarmaSummary.Controls.Add(this.lblSummaryMetatype, 0, 0);
            this.tlpKarmaSummary.Controls.Add(this.lblInitiationBP, 1, 19);
            this.tlpKarmaSummary.Controls.Add(this.lblBuildMartialArts, 0, 20);
            this.tlpKarmaSummary.Controls.Add(this.lblMartialArtsBP, 1, 20);
            this.tlpKarmaSummary.Controls.Add(this.lblBuildPrepsBP, 1, 13);
            this.tlpKarmaSummary.Controls.Add(this.lblBuildInitiation, 0, 19);
            this.tlpKarmaSummary.Controls.Add(this.lblBuildPrepsBPLabel, 0, 13);
            this.tlpKarmaSummary.Controls.Add(this.lblFociBP, 1, 15);
            this.tlpKarmaSummary.Controls.Add(this.lblBuildFoci, 0, 15);
            this.tlpKarmaSummary.Controls.Add(this.lblComplexFormsBP, 1, 18);
            this.tlpKarmaSummary.Controls.Add(this.lblPBuildSpecialLabel, 0, 2);
            this.tlpKarmaSummary.Controls.Add(this.lblBuildComplexForms, 0, 18);
            this.tlpKarmaSummary.Controls.Add(this.lblKarmaMetatypeBP, 1, 0);
            this.tlpKarmaSummary.Controls.Add(this.lblSpritesBP, 1, 17);
            this.tlpKarmaSummary.Controls.Add(this.lblBuildSprites, 0, 17);
            this.tlpKarmaSummary.Controls.Add(this.lblBuildPrimaryAttributes, 0, 1);
            this.tlpKarmaSummary.Controls.Add(this.lblSpiritsBP, 1, 16);
            this.tlpKarmaSummary.Controls.Add(this.lblBuildSpirits, 0, 16);
            this.tlpKarmaSummary.Controls.Add(this.lblAttributesBP, 1, 1);
            this.tlpKarmaSummary.Controls.Add(this.lblBuildPositiveQualities, 0, 3);
            this.tlpKarmaSummary.Controls.Add(this.lblBuildNegativeQualities, 0, 4);
            this.tlpKarmaSummary.Controls.Add(this.lblPositiveQualitiesBP, 1, 3);
            this.tlpKarmaSummary.Controls.Add(this.lblNegativeQualitiesBP, 1, 4);
            this.tlpKarmaSummary.Controls.Add(this.lblBuildContacts, 0, 6);
            this.tlpKarmaSummary.Controls.Add(this.lblContactsBP, 1, 6);
            this.tlpKarmaSummary.Controls.Add(this.lblBuildEnemies, 0, 7);
            this.tlpKarmaSummary.Controls.Add(this.lblEnemiesBP, 1, 7);
            this.tlpKarmaSummary.Controls.Add(this.lblNuyenBP, 1, 8);
            this.tlpKarmaSummary.Controls.Add(this.lblBuildNuyen, 0, 8);
            this.tlpKarmaSummary.Controls.Add(this.lblBuildSkillGroups, 0, 9);
            this.tlpKarmaSummary.Controls.Add(this.lblSkillGroupsBP, 1, 9);
            this.tlpKarmaSummary.Controls.Add(this.lblBuildActiveSkills, 0, 10);
            this.tlpKarmaSummary.Controls.Add(this.lblActiveSkillsBP, 1, 10);
            this.tlpKarmaSummary.Controls.Add(this.lblSpellsBP, 1, 12);
            this.tlpKarmaSummary.Controls.Add(this.lblBuildSpells, 0, 12);
            this.tlpKarmaSummary.Controls.Add(this.lblBuildKnowledgeSkills, 0, 11);
            this.tlpKarmaSummary.Controls.Add(this.lblKnowledgeSkillsBP, 1, 11);
            this.tlpKarmaSummary.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpKarmaSummary.Location = new System.Drawing.Point(3, 3);
            this.tlpKarmaSummary.Margin = new System.Windows.Forms.Padding(0);
            this.tlpKarmaSummary.Name = "tlpKarmaSummary";
            this.tlpKarmaSummary.RowCount = 23;
            this.tlpKarmaSummary.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpKarmaSummary.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpKarmaSummary.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpKarmaSummary.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpKarmaSummary.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpKarmaSummary.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpKarmaSummary.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpKarmaSummary.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpKarmaSummary.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpKarmaSummary.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpKarmaSummary.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpKarmaSummary.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpKarmaSummary.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpKarmaSummary.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpKarmaSummary.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpKarmaSummary.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpKarmaSummary.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpKarmaSummary.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpKarmaSummary.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpKarmaSummary.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpKarmaSummary.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpKarmaSummary.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpKarmaSummary.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpKarmaSummary.Size = new System.Drawing.Size(261, 625);
            this.tlpKarmaSummary.TabIndex = 99;
            // 
            // lblMetagenicQualities
            // 
            this.lblMetagenicQualities.AutoSize = true;
            this.lblMetagenicQualities.Location = new System.Drawing.Point(146, 131);
            this.lblMetagenicQualities.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblMetagenicQualities.Name = "lblMetagenicQualities";
            this.lblMetagenicQualities.Size = new System.Drawing.Size(88, 13);
            this.lblMetagenicQualities.TabIndex = 137;
            this.lblMetagenicQualities.Text = "[P]/[N] (L) (0-1 K)";
            // 
            // lblMetagenicQualitiesLabel
            // 
            this.lblMetagenicQualitiesLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblMetagenicQualitiesLabel.AutoSize = true;
            this.lblMetagenicQualitiesLabel.Location = new System.Drawing.Point(40, 131);
            this.lblMetagenicQualitiesLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblMetagenicQualitiesLabel.Name = "lblMetagenicQualitiesLabel";
            this.lblMetagenicQualitiesLabel.Size = new System.Drawing.Size(100, 13);
            this.lblMetagenicQualitiesLabel.TabIndex = 136;
            this.lblMetagenicQualitiesLabel.Tag = "Label_SummaryMetagenicQualities";
            this.lblMetagenicQualitiesLabel.Text = "Metagenic Qualities";
            // 
            // lblAINormalProgramsBP
            // 
            this.lblAINormalProgramsBP.AutoSize = true;
            this.lblAINormalProgramsBP.Location = new System.Drawing.Point(146, 531);
            this.lblAINormalProgramsBP.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblAINormalProgramsBP.Name = "lblAINormalProgramsBP";
            this.lblAINormalProgramsBP.Size = new System.Drawing.Size(30, 13);
            this.lblAINormalProgramsBP.TabIndex = 88;
            this.lblAINormalProgramsBP.Text = "0 BP";
            // 
            // lblAIAdvancedProgramsBP
            // 
            this.lblAIAdvancedProgramsBP.AutoSize = true;
            this.lblAIAdvancedProgramsBP.Location = new System.Drawing.Point(146, 556);
            this.lblAIAdvancedProgramsBP.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblAIAdvancedProgramsBP.Name = "lblAIAdvancedProgramsBP";
            this.lblAIAdvancedProgramsBP.Size = new System.Drawing.Size(30, 13);
            this.lblAIAdvancedProgramsBP.TabIndex = 86;
            this.lblAIAdvancedProgramsBP.Text = "0 BP";
            // 
            // lblBuildRitualsBP
            // 
            this.lblBuildRitualsBP.AutoSize = true;
            this.lblBuildRitualsBP.Location = new System.Drawing.Point(146, 356);
            this.lblBuildRitualsBP.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblBuildRitualsBP.Name = "lblBuildRitualsBP";
            this.lblBuildRitualsBP.Size = new System.Drawing.Size(30, 13);
            this.lblBuildRitualsBP.TabIndex = 132;
            this.lblBuildRitualsBP.Text = "0 BP";
            // 
            // lblBuildAINormalPrograms
            // 
            this.lblBuildAINormalPrograms.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblBuildAINormalPrograms.AutoSize = true;
            this.lblBuildAINormalPrograms.Location = new System.Drawing.Point(53, 531);
            this.lblBuildAINormalPrograms.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblBuildAINormalPrograms.Name = "lblBuildAINormalPrograms";
            this.lblBuildAINormalPrograms.Size = new System.Drawing.Size(87, 13);
            this.lblBuildAINormalPrograms.TabIndex = 89;
            this.lblBuildAINormalPrograms.Tag = "Label_SummaryAINormalPrograms";
            this.lblBuildAINormalPrograms.Text = "Normal Programs";
            // 
            // lblPBuildSpecial
            // 
            this.lblPBuildSpecial.AutoSize = true;
            this.lblPBuildSpecial.Location = new System.Drawing.Point(146, 56);
            this.lblPBuildSpecial.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblPBuildSpecial.Name = "lblPBuildSpecial";
            this.lblPBuildSpecial.Size = new System.Drawing.Size(34, 13);
            this.lblPBuildSpecial.TabIndex = 135;
            this.lblPBuildSpecial.Text = "0 of 0";
            // 
            // lblSummaryMetatype
            // 
            this.lblSummaryMetatype.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblSummaryMetatype.AutoSize = true;
            this.lblSummaryMetatype.Location = new System.Drawing.Point(89, 6);
            this.lblSummaryMetatype.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSummaryMetatype.Name = "lblSummaryMetatype";
            this.lblSummaryMetatype.Size = new System.Drawing.Size(51, 13);
            this.lblSummaryMetatype.TabIndex = 73;
            this.lblSummaryMetatype.Tag = "Label_SummaryMetatype";
            this.lblSummaryMetatype.Text = "Metatype";
            // 
            // lblInitiationBP
            // 
            this.lblInitiationBP.AutoSize = true;
            this.lblInitiationBP.Location = new System.Drawing.Point(146, 481);
            this.lblInitiationBP.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblInitiationBP.Name = "lblInitiationBP";
            this.lblInitiationBP.Size = new System.Drawing.Size(30, 13);
            this.lblInitiationBP.TabIndex = 82;
            this.lblInitiationBP.Text = "0 BP";
            // 
            // lblMartialArtsBP
            // 
            this.lblMartialArtsBP.AutoSize = true;
            this.lblMartialArtsBP.Location = new System.Drawing.Point(146, 506);
            this.lblMartialArtsBP.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblMartialArtsBP.Name = "lblMartialArtsBP";
            this.lblMartialArtsBP.Size = new System.Drawing.Size(30, 13);
            this.lblMartialArtsBP.TabIndex = 82;
            this.lblMartialArtsBP.Text = "0 BP";
            // 
            // lblBuildPrepsBP
            // 
            this.lblBuildPrepsBP.AutoSize = true;
            this.lblBuildPrepsBP.Location = new System.Drawing.Point(146, 331);
            this.lblBuildPrepsBP.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblBuildPrepsBP.Name = "lblBuildPrepsBP";
            this.lblBuildPrepsBP.Size = new System.Drawing.Size(30, 13);
            this.lblBuildPrepsBP.TabIndex = 130;
            this.lblBuildPrepsBP.Text = "0 BP";
            // 
            // lblBuildInitiation
            // 
            this.lblBuildInitiation.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblBuildInitiation.AutoSize = true;
            this.lblBuildInitiation.Location = new System.Drawing.Point(34, 481);
            this.lblBuildInitiation.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblBuildInitiation.Name = "lblBuildInitiation";
            this.lblBuildInitiation.Size = new System.Drawing.Size(106, 13);
            this.lblBuildInitiation.TabIndex = 83;
            this.lblBuildInitiation.Tag = "Label_SummaryInitiation";
            this.lblBuildInitiation.Text = "Initiation/Submersion";
            // 
            // lblFociBP
            // 
            this.lblFociBP.AutoSize = true;
            this.lblFociBP.Location = new System.Drawing.Point(146, 381);
            this.lblFociBP.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblFociBP.Name = "lblFociBP";
            this.lblFociBP.Size = new System.Drawing.Size(30, 13);
            this.lblFociBP.TabIndex = 80;
            this.lblFociBP.Text = "0 BP";
            // 
            // lblComplexFormsBP
            // 
            this.lblComplexFormsBP.AutoSize = true;
            this.lblComplexFormsBP.Location = new System.Drawing.Point(146, 456);
            this.lblComplexFormsBP.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblComplexFormsBP.Name = "lblComplexFormsBP";
            this.lblComplexFormsBP.Size = new System.Drawing.Size(30, 13);
            this.lblComplexFormsBP.TabIndex = 61;
            this.lblComplexFormsBP.Text = "0 BP";
            // 
            // lblPBuildSpecialLabel
            // 
            this.lblPBuildSpecialLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblPBuildSpecialLabel.AutoSize = true;
            this.lblPBuildSpecialLabel.Location = new System.Drawing.Point(51, 56);
            this.lblPBuildSpecialLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblPBuildSpecialLabel.Name = "lblPBuildSpecialLabel";
            this.lblPBuildSpecialLabel.Size = new System.Drawing.Size(89, 13);
            this.lblPBuildSpecialLabel.TabIndex = 134;
            this.lblPBuildSpecialLabel.Tag = "String_Special";
            this.lblPBuildSpecialLabel.Text = "Special Attributes";
            // 
            // lblKarmaMetatypeBP
            // 
            this.lblKarmaMetatypeBP.AutoSize = true;
            this.lblKarmaMetatypeBP.Location = new System.Drawing.Point(146, 6);
            this.lblKarmaMetatypeBP.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKarmaMetatypeBP.Name = "lblKarmaMetatypeBP";
            this.lblKarmaMetatypeBP.Size = new System.Drawing.Size(30, 13);
            this.lblKarmaMetatypeBP.TabIndex = 74;
            this.lblKarmaMetatypeBP.Text = "0 BP";
            // 
            // lblSpritesBP
            // 
            this.lblSpritesBP.AutoSize = true;
            this.lblSpritesBP.Location = new System.Drawing.Point(146, 431);
            this.lblSpritesBP.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSpritesBP.Name = "lblSpritesBP";
            this.lblSpritesBP.Size = new System.Drawing.Size(30, 13);
            this.lblSpritesBP.TabIndex = 58;
            this.lblSpritesBP.Text = "0 BP";
            // 
            // lblSpiritsBP
            // 
            this.lblSpiritsBP.AutoSize = true;
            this.lblSpiritsBP.Location = new System.Drawing.Point(146, 406);
            this.lblSpiritsBP.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSpiritsBP.Name = "lblSpiritsBP";
            this.lblSpiritsBP.Size = new System.Drawing.Size(30, 13);
            this.lblSpiritsBP.TabIndex = 52;
            this.lblSpiritsBP.Text = "0 BP";
            // 
            // lblAttributesBP
            // 
            this.lblAttributesBP.AutoSize = true;
            this.lblAttributesBP.Location = new System.Drawing.Point(146, 31);
            this.lblAttributesBP.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblAttributesBP.Name = "lblAttributesBP";
            this.lblAttributesBP.Size = new System.Drawing.Size(30, 13);
            this.lblAttributesBP.TabIndex = 56;
            this.lblAttributesBP.Text = "0 BP";
            // 
            // lblPositiveQualitiesBP
            // 
            this.lblPositiveQualitiesBP.AutoSize = true;
            this.lblPositiveQualitiesBP.Location = new System.Drawing.Point(146, 81);
            this.lblPositiveQualitiesBP.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblPositiveQualitiesBP.Name = "lblPositiveQualitiesBP";
            this.lblPositiveQualitiesBP.Size = new System.Drawing.Size(30, 13);
            this.lblPositiveQualitiesBP.TabIndex = 53;
            this.lblPositiveQualitiesBP.Text = "0 BP";
            // 
            // lblNegativeQualitiesBP
            // 
            this.lblNegativeQualitiesBP.AutoSize = true;
            this.lblNegativeQualitiesBP.Location = new System.Drawing.Point(146, 106);
            this.lblNegativeQualitiesBP.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblNegativeQualitiesBP.Name = "lblNegativeQualitiesBP";
            this.lblNegativeQualitiesBP.Size = new System.Drawing.Size(30, 13);
            this.lblNegativeQualitiesBP.TabIndex = 55;
            this.lblNegativeQualitiesBP.Text = "0 BP";
            // 
            // lblContactsBP
            // 
            this.lblContactsBP.AutoSize = true;
            this.lblContactsBP.Location = new System.Drawing.Point(146, 156);
            this.lblContactsBP.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblContactsBP.Name = "lblContactsBP";
            this.lblContactsBP.Size = new System.Drawing.Size(46, 13);
            this.lblContactsBP.TabIndex = 60;
            this.lblContactsBP.Text = "0 Karma";
            // 
            // lblEnemiesBP
            // 
            this.lblEnemiesBP.AutoSize = true;
            this.lblEnemiesBP.Location = new System.Drawing.Point(146, 181);
            this.lblEnemiesBP.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblEnemiesBP.Name = "lblEnemiesBP";
            this.lblEnemiesBP.Size = new System.Drawing.Size(30, 13);
            this.lblEnemiesBP.TabIndex = 66;
            this.lblEnemiesBP.Text = "0 BP";
            // 
            // lblNuyenBP
            // 
            this.lblNuyenBP.AutoSize = true;
            this.lblNuyenBP.Location = new System.Drawing.Point(146, 206);
            this.lblNuyenBP.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblNuyenBP.Name = "lblNuyenBP";
            this.lblNuyenBP.Size = new System.Drawing.Size(30, 13);
            this.lblNuyenBP.TabIndex = 77;
            this.lblNuyenBP.Text = "0 BP";
            // 
            // lblSkillGroupsBP
            // 
            this.lblSkillGroupsBP.AutoSize = true;
            this.lblSkillGroupsBP.Location = new System.Drawing.Point(146, 231);
            this.lblSkillGroupsBP.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSkillGroupsBP.Name = "lblSkillGroupsBP";
            this.lblSkillGroupsBP.Size = new System.Drawing.Size(30, 13);
            this.lblSkillGroupsBP.TabIndex = 65;
            this.lblSkillGroupsBP.Text = "0 BP";
            // 
            // lblActiveSkillsBP
            // 
            this.lblActiveSkillsBP.AutoSize = true;
            this.lblActiveSkillsBP.Location = new System.Drawing.Point(146, 256);
            this.lblActiveSkillsBP.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblActiveSkillsBP.Name = "lblActiveSkillsBP";
            this.lblActiveSkillsBP.Size = new System.Drawing.Size(30, 13);
            this.lblActiveSkillsBP.TabIndex = 67;
            this.lblActiveSkillsBP.Text = "0 BP";
            // 
            // lblSpellsBP
            // 
            this.lblSpellsBP.AutoSize = true;
            this.lblSpellsBP.Location = new System.Drawing.Point(146, 306);
            this.lblSpellsBP.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSpellsBP.Name = "lblSpellsBP";
            this.lblSpellsBP.Size = new System.Drawing.Size(30, 13);
            this.lblSpellsBP.TabIndex = 51;
            this.lblSpellsBP.Text = "0 BP";
            // 
            // lblKnowledgeSkillsBP
            // 
            this.lblKnowledgeSkillsBP.AutoSize = true;
            this.lblKnowledgeSkillsBP.Location = new System.Drawing.Point(146, 281);
            this.lblKnowledgeSkillsBP.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblKnowledgeSkillsBP.Name = "lblKnowledgeSkillsBP";
            this.lblKnowledgeSkillsBP.Size = new System.Drawing.Size(30, 13);
            this.lblKnowledgeSkillsBP.TabIndex = 68;
            this.lblKnowledgeSkillsBP.Text = "0 BP";
            // 
            // tabOtherInfo
            // 
            this.tabOtherInfo.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.tabOtherInfo.Controls.Add(this.tlpOtherInfo);
            this.tabOtherInfo.Location = new System.Drawing.Point(4, 22);
            this.tabOtherInfo.Name = "tabOtherInfo";
            this.tabOtherInfo.Padding = new System.Windows.Forms.Padding(3);
            this.tabOtherInfo.Size = new System.Drawing.Size(267, 631);
            this.tabOtherInfo.TabIndex = 1;
            this.tabOtherInfo.Tag = "Tab_OtherInfo";
            this.tabOtherInfo.Text = "Other Info";
            // 
            // tlpOtherInfo
            // 
            this.tlpOtherInfo.AutoScroll = true;
            this.tlpOtherInfo.AutoSize = true;
            this.tlpOtherInfo.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpOtherInfo.ColumnCount = 2;
            this.tlpOtherInfo.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 65F));
            this.tlpOtherInfo.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 35F));
            this.tlpOtherInfo.Controls.Add(this.lblFly, 1, 19);
            this.tlpOtherInfo.Controls.Add(this.lblRiggingINI, 1, 7);
            this.tlpOtherInfo.Controls.Add(this.lblFlyLabel, 0, 19);
            this.tlpOtherInfo.Controls.Add(this.lblCMPhysicalLabel, 0, 0);
            this.tlpOtherInfo.Controls.Add(this.lblSwim, 1, 18);
            this.tlpOtherInfo.Controls.Add(this.lblRiggingINILabel, 0, 7);
            this.tlpOtherInfo.Controls.Add(this.lblSwimLabel, 0, 18);
            this.tlpOtherInfo.Controls.Add(this.lblCMPhysical, 1, 0);
            this.tlpOtherInfo.Controls.Add(this.lblMovement, 1, 17);
            this.tlpOtherInfo.Controls.Add(this.lblMemory, 1, 16);
            this.tlpOtherInfo.Controls.Add(this.lblMovementLabel, 0, 17);
            this.tlpOtherInfo.Controls.Add(this.lblMatrixINIHot, 1, 6);
            this.tlpOtherInfo.Controls.Add(this.lblMemoryLabel, 0, 16);
            this.tlpOtherInfo.Controls.Add(this.lblCMStunLabel, 0, 1);
            this.tlpOtherInfo.Controls.Add(this.lblLiftCarry, 1, 15);
            this.tlpOtherInfo.Controls.Add(this.lblMatrixINICold, 1, 5);
            this.tlpOtherInfo.Controls.Add(this.lblLiftCarryLabel, 0, 15);
            this.tlpOtherInfo.Controls.Add(this.lblMatrixINIHotLabel, 0, 6);
            this.tlpOtherInfo.Controls.Add(this.lblJudgeIntentions, 1, 13);
            this.tlpOtherInfo.Controls.Add(this.lblCMStun, 1, 1);
            this.tlpOtherInfo.Controls.Add(this.lblJudgeIntentionsLabel, 0, 13);
            this.tlpOtherInfo.Controls.Add(this.lblINILabel, 0, 2);
            this.tlpOtherInfo.Controls.Add(this.lblComposure, 1, 12);
            this.tlpOtherInfo.Controls.Add(this.lblMatrixINIColdLabel, 0, 5);
            this.tlpOtherInfo.Controls.Add(this.lblComposureLabel, 0, 12);
            this.tlpOtherInfo.Controls.Add(this.lblINI, 1, 2);
            this.tlpOtherInfo.Controls.Add(this.lblAstralINILabel, 0, 3);
            this.tlpOtherInfo.Controls.Add(this.lblMatrixINILabel, 0, 4);
            this.tlpOtherInfo.Controls.Add(this.lblRemainingNuyen, 1, 11);
            this.tlpOtherInfo.Controls.Add(this.lblAstralINI, 1, 3);
            this.tlpOtherInfo.Controls.Add(this.lblRemainingNuyenLabel, 0, 11);
            this.tlpOtherInfo.Controls.Add(this.lblMatrixINI, 1, 4);
            this.tlpOtherInfo.Controls.Add(this.lblESSMax, 1, 10);
            this.tlpOtherInfo.Controls.Add(this.lblArmorLabel, 0, 8);
            this.tlpOtherInfo.Controls.Add(this.lblArmor, 1, 8);
            this.tlpOtherInfo.Controls.Add(this.lblESS, 0, 10);
            this.tlpOtherInfo.Controls.Add(this.lblSurprise, 1, 14);
            this.tlpOtherInfo.Controls.Add(this.lblSurpriseLabel, 0, 14);
            this.tlpOtherInfo.Controls.Add(this.lblDodgeLabel, 0, 9);
            this.tlpOtherInfo.Controls.Add(this.lblDodge, 1, 9);
            this.tlpOtherInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpOtherInfo.Location = new System.Drawing.Point(3, 3);
            this.tlpOtherInfo.Margin = new System.Windows.Forms.Padding(0);
            this.tlpOtherInfo.Name = "tlpOtherInfo";
            this.tlpOtherInfo.RowCount = 20;
            this.tlpOtherInfo.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpOtherInfo.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpOtherInfo.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpOtherInfo.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpOtherInfo.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpOtherInfo.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpOtherInfo.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpOtherInfo.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpOtherInfo.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpOtherInfo.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpOtherInfo.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpOtherInfo.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpOtherInfo.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpOtherInfo.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpOtherInfo.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpOtherInfo.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpOtherInfo.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpOtherInfo.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpOtherInfo.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpOtherInfo.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpOtherInfo.Size = new System.Drawing.Size(261, 625);
            this.tlpOtherInfo.TabIndex = 99;
            // 
            // lblFly
            // 
            this.lblFly.AutoSize = true;
            this.lblFly.Location = new System.Drawing.Point(172, 481);
            this.lblFly.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblFly.Name = "lblFly";
            this.lblFly.Size = new System.Drawing.Size(13, 13);
            this.lblFly.TabIndex = 55;
            this.lblFly.Text = "0";
            // 
            // lblRiggingINI
            // 
            this.lblRiggingINI.AutoSize = true;
            this.lblRiggingINI.Location = new System.Drawing.Point(172, 181);
            this.lblRiggingINI.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblRiggingINI.Name = "lblRiggingINI";
            this.lblRiggingINI.Size = new System.Drawing.Size(13, 13);
            this.lblRiggingINI.TabIndex = 76;
            this.lblRiggingINI.Text = "0";
            this.lblRiggingINI.ToolTipText = "";
            // 
            // lblFlyLabel
            // 
            this.lblFlyLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblFlyLabel.AutoSize = true;
            this.lblFlyLabel.Location = new System.Drawing.Point(143, 481);
            this.lblFlyLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblFlyLabel.Name = "lblFlyLabel";
            this.lblFlyLabel.Size = new System.Drawing.Size(23, 13);
            this.lblFlyLabel.TabIndex = 54;
            this.lblFlyLabel.Tag = "Label_OtherFly";
            this.lblFlyLabel.Text = "Fly:";
            // 
            // lblSwim
            // 
            this.lblSwim.AutoSize = true;
            this.lblSwim.Location = new System.Drawing.Point(172, 456);
            this.lblSwim.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSwim.Name = "lblSwim";
            this.lblSwim.Size = new System.Drawing.Size(13, 13);
            this.lblSwim.TabIndex = 53;
            this.lblSwim.Text = "0";
            // 
            // lblSwimLabel
            // 
            this.lblSwimLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblSwimLabel.AutoSize = true;
            this.lblSwimLabel.Location = new System.Drawing.Point(131, 456);
            this.lblSwimLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSwimLabel.Name = "lblSwimLabel";
            this.lblSwimLabel.Size = new System.Drawing.Size(35, 13);
            this.lblSwimLabel.TabIndex = 52;
            this.lblSwimLabel.Tag = "Label_OtherSwim";
            this.lblSwimLabel.Text = "Swim:";
            // 
            // lblCMPhysical
            // 
            this.lblCMPhysical.AutoSize = true;
            this.lblCMPhysical.Location = new System.Drawing.Point(172, 6);
            this.lblCMPhysical.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblCMPhysical.Name = "lblCMPhysical";
            this.lblCMPhysical.Size = new System.Drawing.Size(13, 13);
            this.lblCMPhysical.TabIndex = 24;
            this.lblCMPhysical.Text = "0";
            this.lblCMPhysical.ToolTipText = "";
            // 
            // lblMovement
            // 
            this.lblMovement.AutoSize = true;
            this.lblMovement.Location = new System.Drawing.Point(172, 431);
            this.lblMovement.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblMovement.Name = "lblMovement";
            this.lblMovement.Size = new System.Drawing.Size(13, 13);
            this.lblMovement.TabIndex = 43;
            this.lblMovement.Text = "0";
            // 
            // lblMemory
            // 
            this.lblMemory.AutoSize = true;
            this.lblMemory.Location = new System.Drawing.Point(172, 406);
            this.lblMemory.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblMemory.Name = "lblMemory";
            this.lblMemory.Size = new System.Drawing.Size(13, 13);
            this.lblMemory.TabIndex = 51;
            this.lblMemory.Text = "0";
            this.lblMemory.ToolTipText = "";
            // 
            // lblMovementLabel
            // 
            this.lblMovementLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblMovementLabel.AutoSize = true;
            this.lblMovementLabel.Location = new System.Drawing.Point(106, 431);
            this.lblMovementLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblMovementLabel.Name = "lblMovementLabel";
            this.lblMovementLabel.Size = new System.Drawing.Size(60, 13);
            this.lblMovementLabel.TabIndex = 42;
            this.lblMovementLabel.Tag = "Label_OtherMovement";
            this.lblMovementLabel.Text = "Movement:";
            // 
            // lblMatrixINIHot
            // 
            this.lblMatrixINIHot.AutoSize = true;
            this.lblMatrixINIHot.Location = new System.Drawing.Point(172, 156);
            this.lblMatrixINIHot.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblMatrixINIHot.Name = "lblMatrixINIHot";
            this.lblMatrixINIHot.Size = new System.Drawing.Size(13, 13);
            this.lblMatrixINIHot.TabIndex = 74;
            this.lblMatrixINIHot.Text = "0";
            this.lblMatrixINIHot.ToolTipText = "";
            // 
            // lblLiftCarry
            // 
            this.lblLiftCarry.AutoSize = true;
            this.lblLiftCarry.Location = new System.Drawing.Point(172, 381);
            this.lblLiftCarry.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblLiftCarry.Name = "lblLiftCarry";
            this.lblLiftCarry.Size = new System.Drawing.Size(13, 13);
            this.lblLiftCarry.TabIndex = 49;
            this.lblLiftCarry.Text = "0";
            this.lblLiftCarry.ToolTipText = "";
            // 
            // lblMatrixINICold
            // 
            this.lblMatrixINICold.AutoSize = true;
            this.lblMatrixINICold.Location = new System.Drawing.Point(172, 131);
            this.lblMatrixINICold.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblMatrixINICold.Name = "lblMatrixINICold";
            this.lblMatrixINICold.Size = new System.Drawing.Size(13, 13);
            this.lblMatrixINICold.TabIndex = 72;
            this.lblMatrixINICold.Text = "0";
            this.lblMatrixINICold.ToolTipText = "";
            // 
            // lblJudgeIntentions
            // 
            this.lblJudgeIntentions.AutoSize = true;
            this.lblJudgeIntentions.Location = new System.Drawing.Point(172, 331);
            this.lblJudgeIntentions.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblJudgeIntentions.Name = "lblJudgeIntentions";
            this.lblJudgeIntentions.Size = new System.Drawing.Size(13, 13);
            this.lblJudgeIntentions.TabIndex = 47;
            this.lblJudgeIntentions.Text = "0";
            this.lblJudgeIntentions.ToolTipText = "";
            // 
            // lblCMStun
            // 
            this.lblCMStun.AutoSize = true;
            this.lblCMStun.Location = new System.Drawing.Point(172, 31);
            this.lblCMStun.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblCMStun.Name = "lblCMStun";
            this.lblCMStun.Size = new System.Drawing.Size(13, 13);
            this.lblCMStun.TabIndex = 25;
            this.lblCMStun.Text = "0";
            this.lblCMStun.ToolTipText = "";
            // 
            // lblComposure
            // 
            this.lblComposure.AutoSize = true;
            this.lblComposure.Location = new System.Drawing.Point(172, 306);
            this.lblComposure.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblComposure.Name = "lblComposure";
            this.lblComposure.Size = new System.Drawing.Size(13, 13);
            this.lblComposure.TabIndex = 45;
            this.lblComposure.Text = "0";
            this.lblComposure.ToolTipText = "";
            // 
            // lblINI
            // 
            this.lblINI.AutoSize = true;
            this.lblINI.Location = new System.Drawing.Point(172, 56);
            this.lblINI.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblINI.Name = "lblINI";
            this.lblINI.Size = new System.Drawing.Size(13, 13);
            this.lblINI.TabIndex = 26;
            this.lblINI.Text = "0";
            this.lblINI.ToolTipText = "";
            // 
            // lblRemainingNuyen
            // 
            this.lblRemainingNuyen.AutoSize = true;
            this.lblRemainingNuyen.Location = new System.Drawing.Point(172, 281);
            this.lblRemainingNuyen.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblRemainingNuyen.Name = "lblRemainingNuyen";
            this.lblRemainingNuyen.Size = new System.Drawing.Size(13, 13);
            this.lblRemainingNuyen.TabIndex = 37;
            this.lblRemainingNuyen.Text = "0";
            // 
            // lblAstralINI
            // 
            this.lblAstralINI.AutoSize = true;
            this.lblAstralINI.Location = new System.Drawing.Point(172, 81);
            this.lblAstralINI.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblAstralINI.Name = "lblAstralINI";
            this.lblAstralINI.Size = new System.Drawing.Size(13, 13);
            this.lblAstralINI.TabIndex = 29;
            this.lblAstralINI.Text = "0";
            this.lblAstralINI.ToolTipText = "";
            // 
            // lblMatrixINI
            // 
            this.lblMatrixINI.AutoSize = true;
            this.lblMatrixINI.Location = new System.Drawing.Point(172, 106);
            this.lblMatrixINI.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblMatrixINI.Name = "lblMatrixINI";
            this.lblMatrixINI.Size = new System.Drawing.Size(13, 13);
            this.lblMatrixINI.TabIndex = 28;
            this.lblMatrixINI.Text = "0";
            this.lblMatrixINI.ToolTipText = "";
            // 
            // lblESSMax
            // 
            this.lblESSMax.AutoSize = true;
            this.lblESSMax.Location = new System.Drawing.Point(172, 256);
            this.lblESSMax.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblESSMax.Name = "lblESSMax";
            this.lblESSMax.Size = new System.Drawing.Size(13, 13);
            this.lblESSMax.TabIndex = 35;
            this.lblESSMax.Text = "0";
            // 
            // lblArmor
            // 
            this.lblArmor.AutoSize = true;
            this.lblArmor.Location = new System.Drawing.Point(172, 206);
            this.lblArmor.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblArmor.Name = "lblArmor";
            this.lblArmor.Size = new System.Drawing.Size(13, 13);
            this.lblArmor.TabIndex = 31;
            this.lblArmor.Text = "0";
            this.lblArmor.ToolTipText = "";
            // 
            // lblSurprise
            // 
            this.lblSurprise.AutoSize = true;
            this.lblSurprise.Location = new System.Drawing.Point(172, 356);
            this.lblSurprise.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSurprise.Name = "lblSurprise";
            this.lblSurprise.Size = new System.Drawing.Size(13, 13);
            this.lblSurprise.TabIndex = 77;
            this.lblSurprise.Text = "0";
            this.lblSurprise.ToolTipText = "";
            // 
            // lblSurpriseLabel
            // 
            this.lblSurpriseLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblSurpriseLabel.AutoSize = true;
            this.lblSurpriseLabel.Location = new System.Drawing.Point(118, 356);
            this.lblSurpriseLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSurpriseLabel.Name = "lblSurpriseLabel";
            this.lblSurpriseLabel.Size = new System.Drawing.Size(48, 13);
            this.lblSurpriseLabel.TabIndex = 78;
            this.lblSurpriseLabel.Tag = "Label_OtherSurprise";
            this.lblSurpriseLabel.Text = "Surprise:";
            this.lblSurpriseLabel.ToolTipText = "";
            // 
            // lblDodgeLabel
            // 
            this.lblDodgeLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblDodgeLabel.AutoSize = true;
            this.lblDodgeLabel.Location = new System.Drawing.Point(124, 231);
            this.lblDodgeLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblDodgeLabel.Name = "lblDodgeLabel";
            this.lblDodgeLabel.Size = new System.Drawing.Size(42, 13);
            this.lblDodgeLabel.TabIndex = 79;
            this.lblDodgeLabel.Text = "Dodge:";
            this.lblDodgeLabel.ToolTipText = "";
            // 
            // lblDodge
            // 
            this.lblDodge.AutoSize = true;
            this.lblDodge.Location = new System.Drawing.Point(172, 231);
            this.lblDodge.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblDodge.Name = "lblDodge";
            this.lblDodge.Size = new System.Drawing.Size(13, 13);
            this.lblDodge.TabIndex = 80;
            this.lblDodge.Text = "0";
            this.lblDodge.ToolTipText = "";
            // 
            // tabDefenses
            // 
            this.tabDefenses.BackColor = System.Drawing.SystemColors.Control;
            this.tabDefenses.Controls.Add(this.tlpSpellDefense);
            this.tabDefenses.Location = new System.Drawing.Point(4, 22);
            this.tabDefenses.Name = "tabDefenses";
            this.tabDefenses.Padding = new System.Windows.Forms.Padding(3);
            this.tabDefenses.Size = new System.Drawing.Size(267, 631);
            this.tabDefenses.TabIndex = 3;
            this.tabDefenses.Tag = "String_SpellDefense";
            this.tabDefenses.Text = "Spell Defense";
            // 
            // tlpSpellDefense
            // 
            this.tlpSpellDefense.AutoScroll = true;
            this.tlpSpellDefense.AutoSize = true;
            this.tlpSpellDefense.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpSpellDefense.ColumnCount = 2;
            this.tlpSpellDefense.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 65F));
            this.tlpSpellDefense.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 35F));
            this.tlpSpellDefense.Controls.Add(this.lblSpellDefenseManipPhysical, 1, 17);
            this.tlpSpellDefense.Controls.Add(this.nudCounterspellingDice, 1, 0);
            this.tlpSpellDefense.Controls.Add(this.lblSpellDefenseManipMental, 1, 16);
            this.tlpSpellDefense.Controls.Add(this.lblSpellDefenseManipPhysicalLabel, 0, 17);
            this.tlpSpellDefense.Controls.Add(this.lblCounterspellingDiceLabel, 0, 0);
            this.tlpSpellDefense.Controls.Add(this.lblSpellDefenseIndirectDodgeLabel, 0, 1);
            this.tlpSpellDefense.Controls.Add(this.lblSpellDefenseManipMentalLabel, 0, 16);
            this.tlpSpellDefense.Controls.Add(this.lblSpellDefenseIndirectSoakLabel, 0, 2);
            this.tlpSpellDefense.Controls.Add(this.lblSpellDefenseIllusionPhysical, 1, 15);
            this.tlpSpellDefense.Controls.Add(this.lblSpellDefenseIndirectDodge, 1, 1);
            this.tlpSpellDefense.Controls.Add(this.lblSpellDefenseIllusionMana, 1, 14);
            this.tlpSpellDefense.Controls.Add(this.lblSpellDefenseIllusionPhysicalLabel, 0, 15);
            this.tlpSpellDefense.Controls.Add(this.lblSpellDefenseIndirectSoak, 1, 2);
            this.tlpSpellDefense.Controls.Add(this.lblSpellDefenseDirectSoakManaLabel, 0, 3);
            this.tlpSpellDefense.Controls.Add(this.lblSpellDefenseIllusionManaLabel, 0, 14);
            this.tlpSpellDefense.Controls.Add(this.lblSpellDefenseDirectSoakPhysicalLabel, 0, 4);
            this.tlpSpellDefense.Controls.Add(this.lblSpellDefenseDecAttWIL, 1, 13);
            this.tlpSpellDefense.Controls.Add(this.lblSpellDefenseDetectionLabel, 0, 5);
            this.tlpSpellDefense.Controls.Add(this.lblSpellDefenseDecAttLOG, 1, 12);
            this.tlpSpellDefense.Controls.Add(this.lblSpellDefenseDirectSoakMana, 1, 3);
            this.tlpSpellDefense.Controls.Add(this.lblSpellDefenseDecAttWILLabel, 0, 13);
            this.tlpSpellDefense.Controls.Add(this.lblSpellDefenseDecAttINT, 1, 11);
            this.tlpSpellDefense.Controls.Add(this.lblSpellDefenseDecAttLOGLabel, 0, 12);
            this.tlpSpellDefense.Controls.Add(this.lblSpellDefenseDirectSoakPhysical, 1, 4);
            this.tlpSpellDefense.Controls.Add(this.lblSpellDefenseDecAttCHA, 1, 10);
            this.tlpSpellDefense.Controls.Add(this.lblSpellDefenseDetection, 1, 5);
            this.tlpSpellDefense.Controls.Add(this.lblSpellDefenseDecAttSTR, 1, 9);
            this.tlpSpellDefense.Controls.Add(this.lblSpellDefenseDecAttBOD, 1, 6);
            this.tlpSpellDefense.Controls.Add(this.lblSpellDefenseDecAttINTLabel, 0, 11);
            this.tlpSpellDefense.Controls.Add(this.lblSpellDefenseDecAttBODLabel, 0, 6);
            this.tlpSpellDefense.Controls.Add(this.lblSpellDefenseDecAttCHALabel, 0, 10);
            this.tlpSpellDefense.Controls.Add(this.lblSpellDefenseDecAttAGILabel, 0, 7);
            this.tlpSpellDefense.Controls.Add(this.lblSpellDefenseDecAttAGI, 1, 7);
            this.tlpSpellDefense.Controls.Add(this.lblSpellDefenseDecAttREALabel, 0, 8);
            this.tlpSpellDefense.Controls.Add(this.lblSpellDefenseDecAttSTRLabel, 0, 9);
            this.tlpSpellDefense.Controls.Add(this.lblSpellDefenseDecAttREA, 1, 8);
            this.tlpSpellDefense.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpSpellDefense.Location = new System.Drawing.Point(3, 3);
            this.tlpSpellDefense.Margin = new System.Windows.Forms.Padding(0);
            this.tlpSpellDefense.Name = "tlpSpellDefense";
            this.tlpSpellDefense.RowCount = 18;
            this.tlpSpellDefense.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpSpellDefense.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpSpellDefense.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpSpellDefense.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpSpellDefense.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpSpellDefense.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpSpellDefense.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpSpellDefense.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpSpellDefense.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpSpellDefense.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpSpellDefense.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpSpellDefense.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpSpellDefense.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpSpellDefense.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpSpellDefense.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpSpellDefense.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpSpellDefense.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpSpellDefense.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpSpellDefense.Size = new System.Drawing.Size(261, 625);
            this.tlpSpellDefense.TabIndex = 99;
            // 
            // lblSpellDefenseManipPhysical
            // 
            this.lblSpellDefenseManipPhysical.AutoSize = true;
            this.lblSpellDefenseManipPhysical.Location = new System.Drawing.Point(172, 432);
            this.lblSpellDefenseManipPhysical.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSpellDefenseManipPhysical.Name = "lblSpellDefenseManipPhysical";
            this.lblSpellDefenseManipPhysical.Size = new System.Drawing.Size(13, 13);
            this.lblSpellDefenseManipPhysical.TabIndex = 60;
            this.lblSpellDefenseManipPhysical.Text = "0";
            this.lblSpellDefenseManipPhysical.ToolTipText = "";
            // 
            // nudCounterspellingDice
            // 
            this.nudCounterspellingDice.Dock = System.Windows.Forms.DockStyle.Fill;
            this.nudCounterspellingDice.Location = new System.Drawing.Point(172, 3);
            this.nudCounterspellingDice.Name = "nudCounterspellingDice";
            this.nudCounterspellingDice.Size = new System.Drawing.Size(86, 20);
            this.nudCounterspellingDice.TabIndex = 61;
            // 
            // lblSpellDefenseManipMental
            // 
            this.lblSpellDefenseManipMental.AutoSize = true;
            this.lblSpellDefenseManipMental.Location = new System.Drawing.Point(172, 407);
            this.lblSpellDefenseManipMental.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSpellDefenseManipMental.Name = "lblSpellDefenseManipMental";
            this.lblSpellDefenseManipMental.Size = new System.Drawing.Size(13, 13);
            this.lblSpellDefenseManipMental.TabIndex = 58;
            this.lblSpellDefenseManipMental.Text = "0";
            this.lblSpellDefenseManipMental.ToolTipText = "";
            // 
            // lblSpellDefenseIllusionPhysical
            // 
            this.lblSpellDefenseIllusionPhysical.AutoSize = true;
            this.lblSpellDefenseIllusionPhysical.Location = new System.Drawing.Point(172, 382);
            this.lblSpellDefenseIllusionPhysical.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSpellDefenseIllusionPhysical.Name = "lblSpellDefenseIllusionPhysical";
            this.lblSpellDefenseIllusionPhysical.Size = new System.Drawing.Size(13, 13);
            this.lblSpellDefenseIllusionPhysical.TabIndex = 56;
            this.lblSpellDefenseIllusionPhysical.Text = "0";
            this.lblSpellDefenseIllusionPhysical.ToolTipText = "";
            // 
            // lblSpellDefenseIndirectDodge
            // 
            this.lblSpellDefenseIndirectDodge.AutoSize = true;
            this.lblSpellDefenseIndirectDodge.Location = new System.Drawing.Point(172, 32);
            this.lblSpellDefenseIndirectDodge.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSpellDefenseIndirectDodge.Name = "lblSpellDefenseIndirectDodge";
            this.lblSpellDefenseIndirectDodge.Size = new System.Drawing.Size(13, 13);
            this.lblSpellDefenseIndirectDodge.TabIndex = 26;
            this.lblSpellDefenseIndirectDodge.Text = "0";
            this.lblSpellDefenseIndirectDodge.ToolTipText = "";
            // 
            // lblSpellDefenseIllusionMana
            // 
            this.lblSpellDefenseIllusionMana.AutoSize = true;
            this.lblSpellDefenseIllusionMana.Location = new System.Drawing.Point(172, 357);
            this.lblSpellDefenseIllusionMana.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSpellDefenseIllusionMana.Name = "lblSpellDefenseIllusionMana";
            this.lblSpellDefenseIllusionMana.Size = new System.Drawing.Size(13, 13);
            this.lblSpellDefenseIllusionMana.TabIndex = 54;
            this.lblSpellDefenseIllusionMana.Text = "0";
            this.lblSpellDefenseIllusionMana.ToolTipText = "";
            // 
            // lblSpellDefenseIndirectSoak
            // 
            this.lblSpellDefenseIndirectSoak.AutoSize = true;
            this.lblSpellDefenseIndirectSoak.Location = new System.Drawing.Point(172, 57);
            this.lblSpellDefenseIndirectSoak.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSpellDefenseIndirectSoak.Name = "lblSpellDefenseIndirectSoak";
            this.lblSpellDefenseIndirectSoak.Size = new System.Drawing.Size(13, 13);
            this.lblSpellDefenseIndirectSoak.TabIndex = 28;
            this.lblSpellDefenseIndirectSoak.Text = "0";
            this.lblSpellDefenseIndirectSoak.ToolTipText = "";
            // 
            // lblSpellDefenseDecAttWIL
            // 
            this.lblSpellDefenseDecAttWIL.AutoSize = true;
            this.lblSpellDefenseDecAttWIL.Location = new System.Drawing.Point(172, 332);
            this.lblSpellDefenseDecAttWIL.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSpellDefenseDecAttWIL.Name = "lblSpellDefenseDecAttWIL";
            this.lblSpellDefenseDecAttWIL.Size = new System.Drawing.Size(13, 13);
            this.lblSpellDefenseDecAttWIL.TabIndex = 52;
            this.lblSpellDefenseDecAttWIL.Text = "0";
            this.lblSpellDefenseDecAttWIL.ToolTipText = "";
            // 
            // lblSpellDefenseDecAttLOG
            // 
            this.lblSpellDefenseDecAttLOG.AutoSize = true;
            this.lblSpellDefenseDecAttLOG.Location = new System.Drawing.Point(172, 307);
            this.lblSpellDefenseDecAttLOG.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSpellDefenseDecAttLOG.Name = "lblSpellDefenseDecAttLOG";
            this.lblSpellDefenseDecAttLOG.Size = new System.Drawing.Size(13, 13);
            this.lblSpellDefenseDecAttLOG.TabIndex = 51;
            this.lblSpellDefenseDecAttLOG.Text = "0";
            this.lblSpellDefenseDecAttLOG.ToolTipText = "";
            // 
            // lblSpellDefenseDirectSoakMana
            // 
            this.lblSpellDefenseDirectSoakMana.AutoSize = true;
            this.lblSpellDefenseDirectSoakMana.Location = new System.Drawing.Point(172, 82);
            this.lblSpellDefenseDirectSoakMana.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSpellDefenseDirectSoakMana.Name = "lblSpellDefenseDirectSoakMana";
            this.lblSpellDefenseDirectSoakMana.Size = new System.Drawing.Size(13, 13);
            this.lblSpellDefenseDirectSoakMana.TabIndex = 30;
            this.lblSpellDefenseDirectSoakMana.Text = "0";
            this.lblSpellDefenseDirectSoakMana.ToolTipText = "";
            // 
            // lblSpellDefenseDecAttINT
            // 
            this.lblSpellDefenseDecAttINT.AutoSize = true;
            this.lblSpellDefenseDecAttINT.Location = new System.Drawing.Point(172, 282);
            this.lblSpellDefenseDecAttINT.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSpellDefenseDecAttINT.Name = "lblSpellDefenseDecAttINT";
            this.lblSpellDefenseDecAttINT.Size = new System.Drawing.Size(13, 13);
            this.lblSpellDefenseDecAttINT.TabIndex = 50;
            this.lblSpellDefenseDecAttINT.Text = "0";
            this.lblSpellDefenseDecAttINT.ToolTipText = "";
            // 
            // lblSpellDefenseDirectSoakPhysical
            // 
            this.lblSpellDefenseDirectSoakPhysical.AutoSize = true;
            this.lblSpellDefenseDirectSoakPhysical.Location = new System.Drawing.Point(172, 107);
            this.lblSpellDefenseDirectSoakPhysical.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSpellDefenseDirectSoakPhysical.Name = "lblSpellDefenseDirectSoakPhysical";
            this.lblSpellDefenseDirectSoakPhysical.Size = new System.Drawing.Size(13, 13);
            this.lblSpellDefenseDirectSoakPhysical.TabIndex = 32;
            this.lblSpellDefenseDirectSoakPhysical.Text = "0";
            this.lblSpellDefenseDirectSoakPhysical.ToolTipText = "";
            // 
            // lblSpellDefenseDecAttCHA
            // 
            this.lblSpellDefenseDecAttCHA.AutoSize = true;
            this.lblSpellDefenseDecAttCHA.Location = new System.Drawing.Point(172, 257);
            this.lblSpellDefenseDecAttCHA.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSpellDefenseDecAttCHA.Name = "lblSpellDefenseDecAttCHA";
            this.lblSpellDefenseDecAttCHA.Size = new System.Drawing.Size(13, 13);
            this.lblSpellDefenseDecAttCHA.TabIndex = 49;
            this.lblSpellDefenseDecAttCHA.Text = "0";
            this.lblSpellDefenseDecAttCHA.ToolTipText = "";
            // 
            // lblSpellDefenseDetection
            // 
            this.lblSpellDefenseDetection.AutoSize = true;
            this.lblSpellDefenseDetection.Location = new System.Drawing.Point(172, 132);
            this.lblSpellDefenseDetection.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSpellDefenseDetection.Name = "lblSpellDefenseDetection";
            this.lblSpellDefenseDetection.Size = new System.Drawing.Size(13, 13);
            this.lblSpellDefenseDetection.TabIndex = 34;
            this.lblSpellDefenseDetection.Text = "0";
            this.lblSpellDefenseDetection.ToolTipText = "";
            // 
            // lblSpellDefenseDecAttSTR
            // 
            this.lblSpellDefenseDecAttSTR.AutoSize = true;
            this.lblSpellDefenseDecAttSTR.Location = new System.Drawing.Point(172, 232);
            this.lblSpellDefenseDecAttSTR.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSpellDefenseDecAttSTR.Name = "lblSpellDefenseDecAttSTR";
            this.lblSpellDefenseDecAttSTR.Size = new System.Drawing.Size(13, 13);
            this.lblSpellDefenseDecAttSTR.TabIndex = 48;
            this.lblSpellDefenseDecAttSTR.Text = "0";
            this.lblSpellDefenseDecAttSTR.ToolTipText = "";
            // 
            // lblSpellDefenseDecAttBOD
            // 
            this.lblSpellDefenseDecAttBOD.AutoSize = true;
            this.lblSpellDefenseDecAttBOD.Location = new System.Drawing.Point(172, 157);
            this.lblSpellDefenseDecAttBOD.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSpellDefenseDecAttBOD.Name = "lblSpellDefenseDecAttBOD";
            this.lblSpellDefenseDecAttBOD.Size = new System.Drawing.Size(13, 13);
            this.lblSpellDefenseDecAttBOD.TabIndex = 36;
            this.lblSpellDefenseDecAttBOD.Text = "0";
            this.lblSpellDefenseDecAttBOD.ToolTipText = "";
            // 
            // lblSpellDefenseDecAttAGI
            // 
            this.lblSpellDefenseDecAttAGI.AutoSize = true;
            this.lblSpellDefenseDecAttAGI.Location = new System.Drawing.Point(172, 182);
            this.lblSpellDefenseDecAttAGI.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSpellDefenseDecAttAGI.Name = "lblSpellDefenseDecAttAGI";
            this.lblSpellDefenseDecAttAGI.Size = new System.Drawing.Size(13, 13);
            this.lblSpellDefenseDecAttAGI.TabIndex = 38;
            this.lblSpellDefenseDecAttAGI.Text = "0";
            this.lblSpellDefenseDecAttAGI.ToolTipText = "";
            // 
            // lblSpellDefenseDecAttREA
            // 
            this.lblSpellDefenseDecAttREA.AutoSize = true;
            this.lblSpellDefenseDecAttREA.Location = new System.Drawing.Point(172, 207);
            this.lblSpellDefenseDecAttREA.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSpellDefenseDecAttREA.Name = "lblSpellDefenseDecAttREA";
            this.lblSpellDefenseDecAttREA.Size = new System.Drawing.Size(13, 13);
            this.lblSpellDefenseDecAttREA.TabIndex = 40;
            this.lblSpellDefenseDecAttREA.Text = "0";
            this.lblSpellDefenseDecAttREA.ToolTipText = "";
            // 
            // cmsInitiationNotes
            // 
            this.cmsInitiationNotes.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsInitiationNotes});
            this.cmsInitiationNotes.Name = "cmsMetamagic";
            this.cmsInitiationNotes.Size = new System.Drawing.Size(106, 26);
            // 
            // tsInitiationNotes
            // 
            this.tsInitiationNotes.Image = global::Chummer.Properties.Resources.note_edit;
            this.tsInitiationNotes.Name = "tsInitiationNotes";
            this.tsInitiationNotes.Size = new System.Drawing.Size(105, 22);
            this.tsInitiationNotes.Tag = "Menu_Notes";
            this.tsInitiationNotes.Text = "&Notes";
            this.tsInitiationNotes.Click += new System.EventHandler(this.tsInitiationNotes_Click);
            // 
            // cmsTechnique
            // 
            this.cmsTechnique.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsAddTechniqueNotes});
            this.cmsTechnique.Name = "cmsWeapon";
            this.cmsTechnique.Size = new System.Drawing.Size(106, 26);
            // 
            // tsAddTechniqueNotes
            // 
            this.tsAddTechniqueNotes.Image = global::Chummer.Properties.Resources.note_edit;
            this.tsAddTechniqueNotes.Name = "tsAddTechniqueNotes";
            this.tsAddTechniqueNotes.Size = new System.Drawing.Size(105, 22);
            this.tsAddTechniqueNotes.Tag = "Menu_Notes";
            this.tsAddTechniqueNotes.Text = "&Notes";
            this.tsAddTechniqueNotes.Click += new System.EventHandler(this.tsAddTechniqueNotes_Click);
            // 
            // cmsAdvancedProgram
            // 
            this.cmsAdvancedProgram.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsAddAdvancedProgramOption,
            this.tsAIProgramNotes});
            this.cmsAdvancedProgram.Name = "cmsAdvancedProgram";
            this.cmsAdvancedProgram.Size = new System.Drawing.Size(137, 48);
            // 
            // tsAddAdvancedProgramOption
            // 
            this.tsAddAdvancedProgramOption.Image = global::Chummer.Properties.Resources.plugin_add;
            this.tsAddAdvancedProgramOption.Name = "tsAddAdvancedProgramOption";
            this.tsAddAdvancedProgramOption.Size = new System.Drawing.Size(136, 22);
            this.tsAddAdvancedProgramOption.Tag = "Menu_AddOption";
            this.tsAddAdvancedProgramOption.Text = "&Add Option";
            // 
            // tsAIProgramNotes
            // 
            this.tsAIProgramNotes.Image = global::Chummer.Properties.Resources.note_edit;
            this.tsAIProgramNotes.Name = "tsAIProgramNotes";
            this.tsAIProgramNotes.Size = new System.Drawing.Size(136, 22);
            this.tsAIProgramNotes.Tag = "Menu_Notes";
            this.tsAIProgramNotes.Text = "&Notes";
            this.tsAIProgramNotes.Click += new System.EventHandler(this.tsAIProgramNotes_Click);
            // 
            // cmsGearAllowRename
            // 
            this.cmsGearAllowRename.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsGearAllowRenameAddAsPlugin,
            this.tsGearAllowRenameName,
            this.tsGearAllowRenameNotes,
            this.tsGearAllowRenameExtra});
            this.cmsGearAllowRename.Name = "cmsGearAllowRename";
            this.cmsGearAllowRename.Size = new System.Drawing.Size(171, 92);
            // 
            // tsGearAllowRenameExtra
            // 
            this.tsGearAllowRenameExtra.Image = global::Chummer.Properties.Resources.note_edit;
            this.tsGearAllowRenameExtra.Name = "tsGearAllowRenameExtra";
            this.tsGearAllowRenameExtra.Size = new System.Drawing.Size(170, 22);
            this.tsGearAllowRenameExtra.Tag = "Menu_RenameExtraText";
            this.tsGearAllowRenameExtra.Text = "&Rename Extra Text";
            this.tsGearAllowRenameExtra.Click += new System.EventHandler(this.tsGearRename_Click);
            // 
            // tlpMagicianMentorSpiritHeader
            // 
            this.tlpMagicianMentorSpiritHeader.AutoSize = true;
            this.tlpMagicianMentorSpiritHeader.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpMagicianMentorSpiritHeader.ColumnCount = 4;
            this.tlpMagicianMentorSpiritHeader.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMagicianMentorSpiritHeader.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpMagicianMentorSpiritHeader.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMagicianMentorSpiritHeader.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpMagicianMentorSpiritHeader.Controls.Add(this.lblMentorSpiritLabel, 0, 0);
            this.tlpMagicianMentorSpiritHeader.Controls.Add(this.lblMentorSpiritSourceLabel, 2, 0);
            this.tlpMagicianMentorSpiritHeader.Controls.Add(this.lblMentorSpirit, 1, 0);
            this.tlpMagicianMentorSpiritHeader.Controls.Add(this.lblMentorSpiritSource, 3, 0);
            this.tlpMagicianMentorSpiritHeader.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpMagicianMentorSpiritHeader.Location = new System.Drawing.Point(0, 0);
            this.tlpMagicianMentorSpiritHeader.Margin = new System.Windows.Forms.Padding(0);
            this.tlpMagicianMentorSpiritHeader.Name = "tlpMagicianMentorSpiritHeader";
            this.tlpMagicianMentorSpiritHeader.RowCount = 1;
            this.tlpMagicianMentorSpiritHeader.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMagicianMentorSpiritHeader.Size = new System.Drawing.Size(555, 25);
            this.tlpMagicianMentorSpiritHeader.TabIndex = 99;
            // 
            // lblParagonSource
            // 
            this.lblParagonSource.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblParagonSource.AutoSize = true;
            this.lblParagonSource.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblParagonSource.Location = new System.Drawing.Point(303, 6);
            this.lblParagonSource.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblParagonSource.Name = "lblParagonSource";
            this.lblParagonSource.Size = new System.Drawing.Size(47, 13);
            this.lblParagonSource.TabIndex = 188;
            this.lblParagonSource.Text = "[Source]";
            this.lblParagonSource.Click += new System.EventHandler(this.OpenSourceFromLabel);
            // 
            // lblParagon
            // 
            this.lblParagon.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblParagon.AutoSize = true;
            this.lblParagon.Location = new System.Drawing.Point(59, 6);
            this.lblParagon.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblParagon.Name = "lblParagon";
            this.lblParagon.Size = new System.Drawing.Size(53, 13);
            this.lblParagon.TabIndex = 186;
            this.lblParagon.Text = "[Paragon]";
            // 
            // lblParagonSourceLabel
            // 
            this.lblParagonSourceLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblParagonSourceLabel.AutoSize = true;
            this.lblParagonSourceLabel.Location = new System.Drawing.Point(253, 6);
            this.lblParagonSourceLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblParagonSourceLabel.Name = "lblParagonSourceLabel";
            this.lblParagonSourceLabel.Size = new System.Drawing.Size(44, 13);
            this.lblParagonSourceLabel.TabIndex = 187;
            this.lblParagonSourceLabel.Tag = "Label_Source";
            this.lblParagonSourceLabel.Text = "Source:";
            // 
            // lblParagonLabel
            // 
            this.lblParagonLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblParagonLabel.AutoSize = true;
            this.lblParagonLabel.Location = new System.Drawing.Point(3, 6);
            this.lblParagonLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblParagonLabel.Name = "lblParagonLabel";
            this.lblParagonLabel.Size = new System.Drawing.Size(50, 13);
            this.lblParagonLabel.TabIndex = 185;
            this.lblParagonLabel.Tag = "Label_Paragon";
            this.lblParagonLabel.Text = "Paragon:";
            // 
            // tlpTechnomancerParagonHeader
            // 
            this.tlpTechnomancerParagonHeader.AutoSize = true;
            this.tlpTechnomancerParagonHeader.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpTechnomancerParagonHeader.ColumnCount = 4;
            this.tlpTechnomancerParagonHeader.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpTechnomancerParagonHeader.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpTechnomancerParagonHeader.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpTechnomancerParagonHeader.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpTechnomancerParagonHeader.Controls.Add(this.lblParagonLabel, 0, 0);
            this.tlpTechnomancerParagonHeader.Controls.Add(this.lblParagonSourceLabel, 2, 0);
            this.tlpTechnomancerParagonHeader.Controls.Add(this.lblParagon, 1, 0);
            this.tlpTechnomancerParagonHeader.Controls.Add(this.lblParagonSource, 3, 0);
            this.tlpTechnomancerParagonHeader.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpTechnomancerParagonHeader.Location = new System.Drawing.Point(0, 0);
            this.tlpTechnomancerParagonHeader.Margin = new System.Windows.Forms.Padding(0);
            this.tlpTechnomancerParagonHeader.Name = "tlpTechnomancerParagonHeader";
            this.tlpTechnomancerParagonHeader.RowCount = 1;
            this.tlpTechnomancerParagonHeader.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpTechnomancerParagonHeader.Size = new System.Drawing.Size(494, 25);
            this.tlpTechnomancerParagonHeader.TabIndex = 189;
            // 
            // frmCreate
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.ClientSize = new System.Drawing.Size(1264, 681);
            this.Controls.Add(this.splitMain);
            this.Controls.Add(this.StatusStrip);
            this.Controls.Add(this.tsMain);
            this.Controls.Add(this.mnuCreateMenu);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.mnuCreateMenu;
            this.Name = "frmCreate";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Chummer - Create New Character";
            this.Activated += new System.EventHandler(this.frmCreate_Activated);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmCreate_FormClosing);
            this.Load += new System.EventHandler(this.frmCreate_Load);
            this.StatusStrip.ResumeLayout(false);
            this.StatusStrip.PerformLayout();
            this.cmsMartialArts.ResumeLayout(false);
            this.cmsSpellButton.ResumeLayout(false);
            this.cmsComplexForm.ResumeLayout(false);
            this.cmsCyberware.ResumeLayout(false);
            this.cmsVehicleCyberware.ResumeLayout(false);
            this.cmsLifestyle.ResumeLayout(false);
            this.cmsArmor.ResumeLayout(false);
            this.cmsWeapon.ResumeLayout(false);
            this.cmsGearButton.ResumeLayout(false);
            this.cmsVehicle.ResumeLayout(false);
            this.cmsWeaponMount.ResumeLayout(false);
            this.mnuCreateMenu.ResumeLayout(false);
            this.mnuCreateMenu.PerformLayout();
            this.tsMain.ResumeLayout(false);
            this.tsMain.PerformLayout();
            this.cmsGear.ResumeLayout(false);
            this.cmsVehicleWeapon.ResumeLayout(false);
            this.cmsVehicleGear.ResumeLayout(false);
            this.cmsArmorGear.ResumeLayout(false);
            this.cmsArmorMod.ResumeLayout(false);
            this.cmsQuality.ResumeLayout(false);
            this.cmsSpell.ResumeLayout(false);
            this.cmsCritterPowers.ResumeLayout(false);
            this.cmsLifestyleNotes.ResumeLayout(false);
            this.cmsWeaponAccessory.ResumeLayout(false);
            this.cmsGearPlugin.ResumeLayout(false);
            this.cmsComplexFormPlugin.ResumeLayout(false);
            this.cmsBioware.ResumeLayout(false);
            this.cmsAdvancedLifestyle.ResumeLayout(false);
            this.cmsGearLocation.ResumeLayout(false);
            this.cmsArmorLocation.ResumeLayout(false);
            this.cmsCyberwareGear.ResumeLayout(false);
            this.cmsVehicleCyberwareGear.ResumeLayout(false);
            this.cmsWeaponAccessoryGear.ResumeLayout(false);
            this.cmsVehicleLocation.ResumeLayout(false);
            this.cmsVehicleWeaponAccessory.ResumeLayout(false);
            this.cmsVehicleWeaponAccessoryGear.ResumeLayout(false);
            this.cmsWeaponLocation.ResumeLayout(false);
            this.splitMain.Panel1.ResumeLayout(false);
            this.splitMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitMain)).EndInit();
            this.splitMain.ResumeLayout(false);
            this.tabCharacterTabs.ResumeLayout(false);
            this.tabCommon.ResumeLayout(false);
            this.tabCommon.PerformLayout();
            this.tlpCommon.ResumeLayout(false);
            this.tlpCommon.PerformLayout();
            this.tlpCommonLeftSide.ResumeLayout(false);
            this.tlpCommonLeftSide.PerformLayout();
            this.tlpQualityButtons.ResumeLayout(false);
            this.tlpQualityButtons.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudQualityLevel)).EndInit();
            this.tlpCommonLeftSideBottom.ResumeLayout(false);
            this.tlpCommonLeftSideBottom.PerformLayout();
            this.flpAttributesLabels.ResumeLayout(false);
            this.flpAttributesLabels.PerformLayout();
            this.tlpAlias.ResumeLayout(false);
            this.tlpAlias.PerformLayout();
            this.tlpCommonRightSide.ResumeLayout(false);
            this.tlpCommonRightSide.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudMysticAdeptMAGMagician)).EndInit();
            this.tlpNuyen.ResumeLayout(false);
            this.tlpNuyen.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudNuyen)).EndInit();
            this.tabSkills.ResumeLayout(false);
            this.tabLimits.ResumeLayout(false);
            this.tabLimits.PerformLayout();
            this.tabMartialArts.ResumeLayout(false);
            this.tabMartialArts.PerformLayout();
            this.tlpMartialArts.ResumeLayout(false);
            this.tlpMartialArts.PerformLayout();
            this.tlpMartialArtsButtons.ResumeLayout(false);
            this.tlpMartialArtsButtons.PerformLayout();
            this.tabMagician.ResumeLayout(false);
            this.tlpMagician.ResumeLayout(false);
            this.tlpMagician.PerformLayout();
            this.flpMagician.ResumeLayout(false);
            this.flpMagician.PerformLayout();
            this.gpbMagicianSpell.ResumeLayout(false);
            this.gpbMagicianSpell.PerformLayout();
            this.tlpMagicianSpell.ResumeLayout(false);
            this.tlpMagicianSpell.PerformLayout();
            this.gpbMagicianTradition.ResumeLayout(false);
            this.gpbMagicianTradition.PerformLayout();
            this.tlpMagicianTradition.ResumeLayout(false);
            this.tlpMagicianTradition.PerformLayout();
            this.tlpDrainAttributes.ResumeLayout(false);
            this.tlpDrainAttributes.PerformLayout();
            this.gpbMagicianMentorSpirit.ResumeLayout(false);
            this.gpbMagicianMentorSpirit.PerformLayout();
            this.tlpMagicianMentorSpirit.ResumeLayout(false);
            this.tlpMagicianMentorSpirit.PerformLayout();
            this.tlpMagicianButtons.ResumeLayout(false);
            this.tlpMagicianButtons.PerformLayout();
            this.tabAdept.ResumeLayout(false);
            this.tabAdept.PerformLayout();
            this.tabTechnomancer.ResumeLayout(false);
            this.tlpTechnomancer.ResumeLayout(false);
            this.tlpTechnomancer.PerformLayout();
            this.flpTechnomancer.ResumeLayout(false);
            this.flpTechnomancer.PerformLayout();
            this.gpbTechnomancerComplexForm.ResumeLayout(false);
            this.gpbTechnomancerComplexForm.PerformLayout();
            this.tlpTechnomancerComplexForm.ResumeLayout(false);
            this.tlpTechnomancerComplexForm.PerformLayout();
            this.gpbTechnomancerStream.ResumeLayout(false);
            this.gpbTechnomancerStream.PerformLayout();
            this.tlpTechnomancerStream.ResumeLayout(false);
            this.tlpTechnomancerStream.PerformLayout();
            this.flpFadingAttributesValue.ResumeLayout(false);
            this.flpFadingAttributesValue.PerformLayout();
            this.gpbTechnomancerParagon.ResumeLayout(false);
            this.gpbTechnomancerParagon.PerformLayout();
            this.tlpTechnomancerParagon.ResumeLayout(false);
            this.tlpTechnomancerParagon.PerformLayout();
            this.tlpTechnomancerButtons.ResumeLayout(false);
            this.tlpTechnomancerButtons.PerformLayout();
            this.tabAdvancedPrograms.ResumeLayout(false);
            this.tabAdvancedPrograms.PerformLayout();
            this.tlpAdvancedPrograms.ResumeLayout(false);
            this.tlpAdvancedPrograms.PerformLayout();
            this.tlpAdvancedProgramsButtons.ResumeLayout(false);
            this.tlpAdvancedProgramsButtons.PerformLayout();
            this.tabCritter.ResumeLayout(false);
            this.tlpCritter.ResumeLayout(false);
            this.tlpCritter.PerformLayout();
            this.tlpCritterButtons.ResumeLayout(false);
            this.tlpCritterButtons.PerformLayout();
            this.tabInitiation.ResumeLayout(false);
            this.tlpInitiation.ResumeLayout(false);
            this.tlpInitiation.PerformLayout();
            this.flpInitiation.ResumeLayout(false);
            this.flpInitiation.PerformLayout();
            this.gpbInitiationType.ResumeLayout(false);
            this.gpbInitiationType.PerformLayout();
            this.flpInitiationCheckBoxes.ResumeLayout(false);
            this.flpInitiationCheckBoxes.PerformLayout();
            this.gpbInitiationGroup.ResumeLayout(false);
            this.gpbInitiationGroup.PerformLayout();
            this.tlpInitiationGroup.ResumeLayout(false);
            this.tlpInitiationGroup.PerformLayout();
            this.tlpInitiationButtons.ResumeLayout(false);
            this.tlpInitiationButtons.PerformLayout();
            this.cmsMetamagic.ResumeLayout(false);
            this.tabCyberware.ResumeLayout(false);
            this.tabCyberware.PerformLayout();
            this.tlpCyberware.ResumeLayout(false);
            this.tlpCyberware.PerformLayout();
            this.flpCyberware.ResumeLayout(false);
            this.flpCyberware.PerformLayout();
            this.gpbEssenceConsumption.ResumeLayout(false);
            this.gpbEssenceConsumption.PerformLayout();
            this.tlpCyberwareEssenceConsumption.ResumeLayout(false);
            this.tlpCyberwareEssenceConsumption.PerformLayout();
            this.gpbCyberwareCommon.ResumeLayout(false);
            this.gpbCyberwareCommon.PerformLayout();
            this.tlpCyberwareCommon.ResumeLayout(false);
            this.tlpCyberwareCommon.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudCyberwareRating)).EndInit();
            this.flpCyberwareCommonCheckBoxes.ResumeLayout(false);
            this.flpCyberwareCommonCheckBoxes.PerformLayout();
            this.gpbCyberwareMatrix.ResumeLayout(false);
            this.gpbCyberwareMatrix.PerformLayout();
            this.tlpCyberwareMatrix.ResumeLayout(false);
            this.tlpCyberwareMatrix.PerformLayout();
            this.flpCyberwareMatrixCheckBoxes.ResumeLayout(false);
            this.flpCyberwareMatrixCheckBoxes.PerformLayout();
            this.tlpCyberwareButtons.ResumeLayout(false);
            this.tlpCyberwareButtons.PerformLayout();
            this.tabStreetGear.ResumeLayout(false);
            this.tabStreetGearTabs.ResumeLayout(false);
            this.tabGear.ResumeLayout(false);
            this.tabGear.PerformLayout();
            this.tlpGear.ResumeLayout(false);
            this.tlpGear.PerformLayout();
            this.flpGear.ResumeLayout(false);
            this.flpGear.PerformLayout();
            this.gpbGearCommon.ResumeLayout(false);
            this.gpbGearCommon.PerformLayout();
            this.tlpGearCommon.ResumeLayout(false);
            this.tlpGearCommon.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudGearRating)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudGearQty)).EndInit();
            this.flpGearCommonCheckBoxes.ResumeLayout(false);
            this.flpGearCommonCheckBoxes.PerformLayout();
            this.gpbGearMatrix.ResumeLayout(false);
            this.gpbGearMatrix.PerformLayout();
            this.tlpGearMatrix.ResumeLayout(false);
            this.tlpGearMatrix.PerformLayout();
            this.flpGearMatrixCheckBoxes.ResumeLayout(false);
            this.flpGearMatrixCheckBoxes.PerformLayout();
            this.gpbGearBondedFoci.ResumeLayout(false);
            this.gpbGearBondedFoci.PerformLayout();
            this.tlpGearBondedFoci.ResumeLayout(false);
            this.tlpGearBondedFoci.PerformLayout();
            this.tlpGearButtons.ResumeLayout(false);
            this.tlpGearButtons.PerformLayout();
            this.tabArmor.ResumeLayout(false);
            this.tabArmor.PerformLayout();
            this.tlpArmor.ResumeLayout(false);
            this.tlpArmor.PerformLayout();
            this.flpArmor.ResumeLayout(false);
            this.flpArmor.PerformLayout();
            this.gpbArmorCommon.ResumeLayout(false);
            this.gpbArmorCommon.PerformLayout();
            this.tlpArmorCommon.ResumeLayout(false);
            this.tlpArmorCommon.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudArmorRating)).EndInit();
            this.flpArmorCommonCheckBoxes.ResumeLayout(false);
            this.flpArmorCommonCheckBoxes.PerformLayout();
            this.gpbArmorMatrix.ResumeLayout(false);
            this.gpbArmorMatrix.PerformLayout();
            this.tlpArmorMatrix.ResumeLayout(false);
            this.tlpArmorMatrix.PerformLayout();
            this.gpbArmorLocation.ResumeLayout(false);
            this.gpbArmorLocation.PerformLayout();
            this.tlpArmorLocation.ResumeLayout(false);
            this.tlpArmorLocation.PerformLayout();
            this.flpArmorLocationButtons.ResumeLayout(false);
            this.flpArmorLocationButtons.PerformLayout();
            this.tlpArmorButtons.ResumeLayout(false);
            this.tlpArmorButtons.PerformLayout();
            this.tabWeapons.ResumeLayout(false);
            this.tabWeapons.PerformLayout();
            this.tlpWeapons.ResumeLayout(false);
            this.tlpWeapons.PerformLayout();
            this.flpWeapons.ResumeLayout(false);
            this.flpWeapons.PerformLayout();
            this.gpbWeaponsCommon.ResumeLayout(false);
            this.gpbWeaponsCommon.PerformLayout();
            this.tlpWeaponsCommon.ResumeLayout(false);
            this.tlpWeaponsCommon.PerformLayout();
            this.flpWeaponsCommonCheckBoxes.ResumeLayout(false);
            this.flpWeaponsCommonCheckBoxes.PerformLayout();
            this.gpbWeaponsWeapon.ResumeLayout(false);
            this.gpbWeaponsWeapon.PerformLayout();
            this.flpWeaponsWeapon.ResumeLayout(false);
            this.flpWeaponsWeapon.PerformLayout();
            this.tlpWeaponsWeapon.ResumeLayout(false);
            this.tlpWeaponsWeapon.PerformLayout();
            this.tlpWeaponsRanges.ResumeLayout(false);
            this.tlpWeaponsRanges.PerformLayout();
            this.gpbWeaponsMatrix.ResumeLayout(false);
            this.gpbWeaponsMatrix.PerformLayout();
            this.tlpWeaponsMatrix.ResumeLayout(false);
            this.tlpWeaponsMatrix.PerformLayout();
            this.tlpWeaponsButtons.ResumeLayout(false);
            this.tlpWeaponsButtons.PerformLayout();
            this.tabDrugs.ResumeLayout(false);
            this.tabDrugs.PerformLayout();
            this.tlpDrugInfo.ResumeLayout(false);
            this.tlpDrugInfo.PerformLayout();
            this.flpDrugs.ResumeLayout(false);
            this.flpDrugs.PerformLayout();
            this.gpbDrugsCommon.ResumeLayout(false);
            this.gpbDrugsCommon.PerformLayout();
            this.tlpDrugsCommon.ResumeLayout(false);
            this.tlpDrugsCommon.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudDrugQty)).EndInit();
            this.tlpDrugButtons.ResumeLayout(false);
            this.tlpDrugButtons.PerformLayout();
            this.tabLifestyle.ResumeLayout(false);
            this.tabLifestyle.PerformLayout();
            this.tlpLifestyleDetails.ResumeLayout(false);
            this.tlpLifestyleDetails.PerformLayout();
            this.flpLifestyleDetails.ResumeLayout(false);
            this.flpLifestyleDetails.PerformLayout();
            this.gpbLifestyleCommon.ResumeLayout(false);
            this.gpbLifestyleCommon.PerformLayout();
            this.tlpLifestyleCommon.ResumeLayout(false);
            this.tlpLifestyleCommon.PerformLayout();
            this.flpLifestyleIntervals.ResumeLayout(false);
            this.flpLifestyleIntervals.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudLifestyleMonths)).EndInit();
            this.tlpLifestyleButtons.ResumeLayout(false);
            this.tlpLifestyleButtons.PerformLayout();
            this.tabVehicles.ResumeLayout(false);
            this.tabVehicles.PerformLayout();
            this.tlpVehicles.ResumeLayout(false);
            this.tlpVehicles.PerformLayout();
            this.flpVehiclesButtons.ResumeLayout(false);
            this.flpVehiclesButtons.PerformLayout();
            this.flpVehicles.ResumeLayout(false);
            this.flpVehicles.PerformLayout();
            this.gpbVehiclesCommon.ResumeLayout(false);
            this.gpbVehiclesCommon.PerformLayout();
            this.tlpVehiclesCommon.ResumeLayout(false);
            this.tlpVehiclesCommon.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudVehicleGearQty)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudVehicleRating)).EndInit();
            this.flpVehiclesCommonCheckBoxes.ResumeLayout(false);
            this.flpVehiclesCommonCheckBoxes.PerformLayout();
            this.gpbVehiclesVehicle.ResumeLayout(false);
            this.gpbVehiclesVehicle.PerformLayout();
            this.tlpVehiclesVehicle.ResumeLayout(false);
            this.tlpVehiclesVehicle.PerformLayout();
            this.gpbVehiclesWeapon.ResumeLayout(false);
            this.gpbVehiclesWeapon.PerformLayout();
            this.flpVehiclesWeapon.ResumeLayout(false);
            this.flpVehiclesWeapon.PerformLayout();
            this.tlpVehiclesWeaponCommon.ResumeLayout(false);
            this.tlpVehiclesWeaponCommon.PerformLayout();
            this.tlpVehiclesWeaponRanges.ResumeLayout(false);
            this.tlpVehiclesWeaponRanges.PerformLayout();
            this.gpbVehiclesMatrix.ResumeLayout(false);
            this.gpbVehiclesMatrix.PerformLayout();
            this.tlpVehiclesMatrix.ResumeLayout(false);
            this.tlpVehiclesMatrix.PerformLayout();
            this.flpVehiclesMatrixCheckBoxes.ResumeLayout(false);
            this.flpVehiclesMatrixCheckBoxes.PerformLayout();
            this.tabCharacterInfo.ResumeLayout(false);
            this.tlpCharacterInfo.ResumeLayout(false);
            this.tlpCharacterInfo.PerformLayout();
            this.tlpLongTexts.ResumeLayout(false);
            this.tlpLongTexts.PerformLayout();
            this.gpbDescription.ResumeLayout(false);
            this.gpbDescription.PerformLayout();
            this.gpbBackground.ResumeLayout(false);
            this.gpbBackground.PerformLayout();
            this.gpbConcept.ResumeLayout(false);
            this.gpbConcept.PerformLayout();
            this.gpbNotes.ResumeLayout(false);
            this.gpbNotes.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picMugshot)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudMugshotIndex)).EndInit();
            this.tlpMugshotButtons.ResumeLayout(false);
            this.tlpMugshotButtons.PerformLayout();
            this.tabRelationships.ResumeLayout(false);
            this.tabPeople.ResumeLayout(false);
            this.tabContacts.ResumeLayout(false);
            this.tabContacts.PerformLayout();
            this.tlpContacts.ResumeLayout(false);
            this.tlpContacts.PerformLayout();
            this.tlpContactsButtons.ResumeLayout(false);
            this.tlpContactsButtons.PerformLayout();
            this.cmsAddContact.ResumeLayout(false);
            this.tabEnemies.ResumeLayout(false);
            this.tabEnemies.PerformLayout();
            this.tlpEnemies.ResumeLayout(false);
            this.tlpEnemies.PerformLayout();
            this.flpEnemiesButtons.ResumeLayout(false);
            this.flpEnemiesButtons.PerformLayout();
            this.tabPets.ResumeLayout(false);
            this.tabPets.PerformLayout();
            this.tlpPets.ResumeLayout(false);
            this.tlpPets.PerformLayout();
            this.flpPetsButtons.ResumeLayout(false);
            this.flpPetsButtons.PerformLayout();
            this.tabInfo.ResumeLayout(false);
            this.tabBPSummary.ResumeLayout(false);
            this.tabBPSummary.PerformLayout();
            this.tlpKarmaSummary.ResumeLayout(false);
            this.tlpKarmaSummary.PerformLayout();
            this.tabOtherInfo.ResumeLayout(false);
            this.tabOtherInfo.PerformLayout();
            this.tlpOtherInfo.ResumeLayout(false);
            this.tlpOtherInfo.PerformLayout();
            this.tabDefenses.ResumeLayout(false);
            this.tabDefenses.PerformLayout();
            this.tlpSpellDefense.ResumeLayout(false);
            this.tlpSpellDefense.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudCounterspellingDice)).EndInit();
            this.cmsInitiationNotes.ResumeLayout(false);
            this.cmsTechnique.ResumeLayout(false);
            this.cmsAdvancedProgram.ResumeLayout(false);
            this.cmsGearAllowRename.ResumeLayout(false);
            this.tlpMagicianMentorSpiritHeader.ResumeLayout(false);
            this.tlpMagicianMentorSpiritHeader.PerformLayout();
            this.tlpTechnomancerParagonHeader.ResumeLayout(false);
            this.tlpTechnomancerParagonHeader.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion

        private System.Windows.Forms.StatusStrip StatusStrip;
        private System.Windows.Forms.ToolStripStatusLabel tslKarmaLabel;
        private System.Windows.Forms.ToolStripStatusLabel tslKarma;
        private System.Windows.Forms.ToolStripStatusLabel tslKarmaRemainingLabel;
        private System.Windows.Forms.ToolStripStatusLabel tslKarmaRemaining;
        private System.Windows.Forms.Label lblAttributes;
        private System.Windows.Forms.SaveFileDialog dlgSaveFile;
        private System.Windows.Forms.TabControl tabCharacterTabs;
        private System.Windows.Forms.TabPage tabCommon;
        private System.Windows.Forms.TabPage tabMagician;
        private System.Windows.Forms.TabPage tabAdept;
        private System.Windows.Forms.TabPage tabTechnomancer;
        private System.Windows.Forms.TabPage tabCyberware;
        private System.Windows.Forms.TabPage tabStreetGear;
        private System.Windows.Forms.TreeView treQualities;
        private System.Windows.Forms.Button cmdAddSpirit;
        private System.Windows.Forms.Button cmdAddSprite;
        private System.Windows.Forms.Panel panSprites;
        private System.Windows.Forms.Label lblCyberwareRatingLabel;
        private System.Windows.Forms.Label lblCyberwareCost;
        private System.Windows.Forms.Label lblCyberwareCostLabel;
        private System.Windows.Forms.Label lblCyberwareAvail;
        private System.Windows.Forms.Label lblCyberwareAvailLabel;
        private System.Windows.Forms.Label lblCyberwareGradeLabel;
        private System.Windows.Forms.Label lblCyberwareCapacity;
        private System.Windows.Forms.Label lblCyberwareCapacityLabel;
        private System.Windows.Forms.Label lblCyberwareEssence;
        private System.Windows.Forms.Label lblCyberwareEssenceLabel;
        private System.Windows.Forms.Label lblCyberwareCategory;
        private System.Windows.Forms.Label lblCyberwareCategoryLabel;
        private System.Windows.Forms.Label lblCyberwareName;
        private System.Windows.Forms.Label lblCyberwareNameLabel;
        private System.Windows.Forms.TreeView treCyberware;
        private ElasticComboBox cboCyberwareGrade;
        private System.Windows.Forms.NumericUpDown nudCyberwareRating;
        private System.Windows.Forms.Button cmdDeleteCyberware;
        private System.Windows.Forms.Button cmdAddBioware;
        private System.Windows.Forms.TabControl tabInfo;
        private System.Windows.Forms.TabPage tabBPSummary;
        private System.Windows.Forms.Label lblEnemiesBP;
        private System.Windows.Forms.Label lblBuildEnemies;
        private System.Windows.Forms.Label lblKarmaMetatypeBP;
        private System.Windows.Forms.Label lblSummaryMetatype;
        private System.Windows.Forms.Label lblComplexFormsBP;
        private System.Windows.Forms.Label lblBuildComplexForms;
        private System.Windows.Forms.Label lblBuildSprites;
        private System.Windows.Forms.Label lblBuildSpirits;
        private System.Windows.Forms.Label lblSpiritsBP;
        private System.Windows.Forms.Label lblSpritesBP;
        private System.Windows.Forms.Label lblBuildSpells;
        private System.Windows.Forms.Label lblKnowledgeSkillsBP;
        private System.Windows.Forms.Label lblBuildKnowledgeSkills;
        private System.Windows.Forms.Label lblActiveSkillsBP;
        private System.Windows.Forms.Label lblSpellsBP;
        private System.Windows.Forms.Label lblBuildActiveSkills;
        private System.Windows.Forms.Label lblSkillGroupsBP;
        private System.Windows.Forms.Label lblBuildSkillGroups;
        private System.Windows.Forms.Label lblBuildContacts;
        private System.Windows.Forms.Label lblBuildPrimaryAttributes;
        private System.Windows.Forms.Label lblBuildNegativeQualities;
        private System.Windows.Forms.Label lblBuildPositiveQualities;
        private System.Windows.Forms.Label lblPositiveQualitiesBP;
        private System.Windows.Forms.Label lblNegativeQualitiesBP;
        private System.Windows.Forms.Label lblAttributesBP;
        private System.Windows.Forms.Label lblContactsBP;
        private System.Windows.Forms.TabPage tabOtherInfo;
        private System.Windows.Forms.Label lblESSMax;
        private System.Windows.Forms.Label lblESS;
        private LabelWithToolTip lblArmor;
        private System.Windows.Forms.Label lblArmorLabel;
        private LabelWithToolTip lblAstralINI;
        private LabelWithToolTip lblMatrixINI;
        private LabelWithToolTip lblINI;
        private LabelWithToolTip lblCMStun;
        private LabelWithToolTip lblCMPhysical;
        private System.Windows.Forms.Label lblAstralINILabel;
        private System.Windows.Forms.Label lblMatrixINILabel;
        private System.Windows.Forms.Label lblINILabel;
        private System.Windows.Forms.Label lblCMStunLabel;
        private System.Windows.Forms.Label lblCMPhysicalLabel;
        private System.Windows.Forms.Button cmdDeleteWeapon;
        private System.Windows.Forms.TreeView treWeapons;
        private System.Windows.Forms.Label lblWeaponAP;
        private System.Windows.Forms.Label lblWeaponAPLabel;
        private System.Windows.Forms.Label lblWeaponCost;
        private System.Windows.Forms.Label lblWeaponCostLabel;
        private System.Windows.Forms.Label lblWeaponAvail;
        private System.Windows.Forms.Label lblWeaponAvailLabel;
        private System.Windows.Forms.Label lblWeaponRC;
        private System.Windows.Forms.Label lblWeaponRCLabel;
        private System.Windows.Forms.Label lblWeaponDamage;
        private System.Windows.Forms.Label lblWeaponDamageLabel;
        private System.Windows.Forms.Label lblWeaponCategory;
        private System.Windows.Forms.Label lblWeaponCategoryLabel;
        private System.Windows.Forms.Label lblWeaponMode;
        private System.Windows.Forms.Label lblWeaponModeLabel;
        private System.Windows.Forms.Label lblWeaponReach;
        private System.Windows.Forms.Label lblWeaponReachLabel;
        private System.Windows.Forms.Label lblWeaponAmmo;
        private System.Windows.Forms.Label lblWeaponAmmoLabel;
        private System.Windows.Forms.Button cmdDeleteArmor;
        private System.Windows.Forms.TreeView treArmor;
        private System.Windows.Forms.Label lblArmorCost;
        private System.Windows.Forms.Label lblArmorCostLabel;
        private System.Windows.Forms.Label lblArmorAvail;
        private System.Windows.Forms.Label lblArmorAvailLabel;
        private System.Windows.Forms.Label lblRemainingNuyen;
        private System.Windows.Forms.Label lblRemainingNuyenLabel;
        private System.Windows.Forms.MenuStrip mnuCreateMenu;
        private System.Windows.Forms.ToolStripMenuItem mnuCreateFile;
        private System.Windows.Forms.ToolStripMenuItem mnuFileSaveAs;
        private System.Windows.Forms.ToolStripMenuItem mnuFileSave;
        private System.Windows.Forms.Label lblNuyenBP;
        private System.Windows.Forms.Label lblBuildNuyen;
        private System.Windows.Forms.Label lblWeaponName;
        private System.Windows.Forms.Label lblWeaponNameLabel;
        private System.Windows.Forms.ContextMenuStrip cmsCyberware;
        private System.Windows.Forms.ContextMenuStrip cmsVehicleCyberware;
        private System.Windows.Forms.ToolStripMenuItem tsCyberwareAddAsPlugin;
        private System.Windows.Forms.ToolStripMenuItem tsVehicleCyberwareAddAsPlugin;
        private System.Windows.Forms.Button cmdDeleteLifestyle;
        private System.Windows.Forms.TreeView treLifestyles;
        private System.Windows.Forms.Label lblLifestyleTotalCost;
        private System.Windows.Forms.Label lblLifestyleCostLabel;
        private System.Windows.Forms.Label lblLifestyleCost;
        private System.Windows.Forms.Label lblLifestyleMonthsLabel;
        private System.Windows.Forms.NumericUpDown nudLifestyleMonths;
        private System.Windows.Forms.TabControl tabStreetGearTabs;
        private System.Windows.Forms.TabPage tabLifestyle;
        private System.Windows.Forms.TabPage tabArmor;
        private System.Windows.Forms.TabPage tabWeapons;
        private System.Windows.Forms.TabPage tabGear;
        private System.Windows.Forms.TreeView treGear;
        private System.Windows.Forms.Button cmdDeleteGear;
        private System.Windows.Forms.NumericUpDown nudGearRating;
        private System.Windows.Forms.Label lblGearRatingLabel;
        private System.Windows.Forms.ContextMenuStrip cmsWeapon;
        private System.Windows.Forms.ToolStripMenuItem tsWeaponAddAccessory;
        private System.Windows.Forms.TabPage tabVehicles;
        private System.Windows.Forms.ContextMenuStrip cmsArmor;
        private System.Windows.Forms.ToolStripMenuItem tsAddArmorMod;
        private System.Windows.Forms.ToolStrip tsMain;
        private System.Windows.Forms.ToolStripButton tsbSave;
        private System.Windows.Forms.ContextMenuStrip cmsGear;
        private System.Windows.Forms.ToolStripMenuItem tsGearAddAsPlugin;
        private System.Windows.Forms.Label lblGearCost;
        private System.Windows.Forms.Label lblGearCostLabel;
        private System.Windows.Forms.Label lblGearAvail;
        private System.Windows.Forms.Label lblGearAvailLabel;
        private System.Windows.Forms.Label lblGearCapacity;
        private System.Windows.Forms.Label lblGearCapacityLabel;
        private System.Windows.Forms.Label lblGearCategory;
        private System.Windows.Forms.Label lblGearCategoryLabel;
        private System.Windows.Forms.Label lblGearName;
        private System.Windows.Forms.Label lblGearNameLabel;
        private System.Windows.Forms.NumericUpDown nudGearQty;
        private System.Windows.Forms.Label lblGearQtyLabel;
        private System.Windows.Forms.Button cmdDeleteVehicle;
        private System.Windows.Forms.ToolStripButton tsbPrint;
        private System.Windows.Forms.ContextMenuStrip cmsWeaponMount;
        private System.Windows.Forms.ContextMenuStrip cmsVehicle;
        private System.Windows.Forms.ToolStripMenuItem tsVehicleAddMod;
        private System.Windows.Forms.ToolStripMenuItem tsVehicleAddWeaponMount;
        private System.Windows.Forms.ToolStripMenuItem tsVehicleAddWeapon;
        private System.Windows.Forms.ToolStripMenuItem tsVehicleAddWeaponAccessory;
        private System.Windows.Forms.ContextMenuStrip cmsVehicleWeapon;
        private System.Windows.Forms.ToolStripMenuItem tsVehicleAddWeaponAccessoryAlt;
        private System.Windows.Forms.CheckBox chkArmorEquipped;
        private System.Windows.Forms.TabPage tabMartialArts;
        private System.Windows.Forms.TreeView treMartialArts;
        private System.Windows.Forms.Button cmdDeleteMartialArt;
        private System.Windows.Forms.ContextMenuStrip cmsMartialArts;
        private System.Windows.Forms.ToolStripMenuItem tsMartialArtsAddAdvantage;
        private System.Windows.Forms.TabPage tabCharacterInfo;
        private System.Windows.Forms.TextBox txtSkin;
        private System.Windows.Forms.Label lblSkin;
        private System.Windows.Forms.TextBox txtWeight;
        private System.Windows.Forms.Label lblWeight;
        private System.Windows.Forms.TextBox txtHeight;
        private System.Windows.Forms.Label lblHeight;
        private System.Windows.Forms.TextBox txtHair;
        private System.Windows.Forms.Label lblHair;
        private System.Windows.Forms.TextBox txtEyes;
        private System.Windows.Forms.Label lblEyes;
        private System.Windows.Forms.TextBox txtAge;
        private System.Windows.Forms.Label lblAge;
        private System.Windows.Forms.TextBox txtSex;
        private System.Windows.Forms.Label lblSex;
        private System.Windows.Forms.ToolStripStatusLabel tslEssenceLabel;
        private System.Windows.Forms.ToolStripStatusLabel tslEssence;
        private System.Windows.Forms.ToolStripStatusLabel tslNuyenRemainingLabel;
        private System.Windows.Forms.ToolStripStatusLabel tslNuyenRemaining;
        private System.Windows.Forms.Label lblGearDeviceRating;
        private System.Windows.Forms.Label lblGearDeviceRatingLabel;
        private System.Windows.Forms.Label lblBuildMartialArts;
        private System.Windows.Forms.ToolStripMenuItem tsWeaponAddModification;
        private System.Windows.Forms.Label lblBuildFoci;
        private System.Windows.Forms.Label lblFociBP;
        private System.Windows.Forms.Label lblMartialArtSource;
        private System.Windows.Forms.Label lblMartialArtSourceLabel;
        private System.Windows.Forms.Label lblCyberwareSource;
        private System.Windows.Forms.Label lblCyberwareSourceLabel;
        private System.Windows.Forms.Label lblLifestyleSource;
        private System.Windows.Forms.Label lblLifestyleSourceLabel;
        private System.Windows.Forms.Label lblArmorSource;
        private System.Windows.Forms.Label lblArmorSourceLabel;
        private System.Windows.Forms.Label lblWeaponSource;
        private System.Windows.Forms.Label lblWeaponSourceLabel;
        private System.Windows.Forms.Label lblGearSource;
        private System.Windows.Forms.Label lblGearSourceLabel;
        private System.Windows.Forms.Label lblMugshot;
        private System.Windows.Forms.PictureBox picMugshot;
        private System.Windows.Forms.Button cmdDeleteMugshot;
        private System.Windows.Forms.Button cmdAddMugshot;
        private System.Windows.Forms.Label lblAttributesAug;
        private System.Windows.Forms.Label lblAttributesBase;
        private System.Windows.Forms.Label lblAttributesMetatype;
        private System.Windows.Forms.ToolStripMenuItem mnuFilePrint;
        private System.Windows.Forms.ToolStripSeparator tssFileMenu1;
        private System.Windows.Forms.TabPage tabInitiation;
        private SplitButton cmdAddMetamagic;
        private System.Windows.Forms.Button cmdDeleteMetamagic;
        private System.Windows.Forms.Label lblInitiationBP;
        private System.Windows.Forms.Label lblMartialArtsBP;
        private System.Windows.Forms.Label lblBuildInitiation;
        private System.Windows.Forms.Label lblWeaponSlots;
        private System.Windows.Forms.Label lblWeaponSlotsLabel;
        private System.Windows.Forms.ToolStripMenuItem tsVehicleAddSensor;
        private System.Windows.Forms.ContextMenuStrip cmsVehicleGear;
        private System.Windows.Forms.ToolStripMenuItem tsVehicleGearAddAsPlugin;
        private System.Windows.Forms.ToolStripMenuItem tsVehicleSensorAddAsPlugin;
        private System.Windows.Forms.ToolStripMenuItem mnuCreateSpecial;
        private System.Windows.Forms.ToolStripMenuItem mnuSpecialAddPACKSKit;
        private System.Windows.Forms.ToolStripMenuItem mnuSpecialAddCyberwareSuite;
        private System.Windows.Forms.ToolStripMenuItem mnuSpecialConfirmValidity;
        private System.Windows.Forms.NumericUpDown nudArmorRating;
        private System.Windows.Forms.Label lblArmorRatingLabel;
        private System.Windows.Forms.Label lblLifestyleStartingNuyenLabel;
        private System.Windows.Forms.Label lblLifestyleStartingNuyen;
        private System.Windows.Forms.ToolStripProgressBar pgbProgress;
        private System.Windows.Forms.CheckBox chkCharacterCreated;
        private System.Windows.Forms.Label lblBaseLifestyle;
        private System.Windows.Forms.Label lblLifestyleComfortsLabel;
        private System.Windows.Forms.Label lblLifestyleQualities;
        private System.Windows.Forms.Label lblLifestyleQualitiesLabel;
        private System.Windows.Forms.TextBox txtPlayerName;
        private System.Windows.Forms.Label lblPlayerName;
        private System.Windows.Forms.CheckBox chkWeaponAccessoryInstalled;
        private System.Windows.Forms.CheckBox chkIncludedInWeapon;
        private System.Windows.Forms.Label lblWeaponRangeExtreme;
        private System.Windows.Forms.Label lblWeaponRangeLong;
        private System.Windows.Forms.Label lblWeaponRangeMedium;
        private System.Windows.Forms.Label lblWeaponRangeShort;
        private System.Windows.Forms.Label lblWeaponRangeExtremeLabel;
        private System.Windows.Forms.Label lblWeaponRangeLongLabel;
        private System.Windows.Forms.Label lblWeaponRangeMediumLabel;
        private System.Windows.Forms.Label lblWeaponRangeShortLabel;
        private System.Windows.Forms.Label lblWeaponRangeLabel;
        private System.Windows.Forms.TabPage tabCritter;
        private System.Windows.Forms.Label lblCritterPowerSource;
        private System.Windows.Forms.Label lblCritterPowerSourceLabel;
        private System.Windows.Forms.Label lblCritterPowerDuration;
        private System.Windows.Forms.Label lblCritterPowerDurationLabel;
        private System.Windows.Forms.Label lblCritterPowerRange;
        private System.Windows.Forms.Label lblCritterPowerRangeLabel;
        private System.Windows.Forms.Label lblCritterPowerAction;
        private System.Windows.Forms.Label lblCritterPowerActionLabel;
        private System.Windows.Forms.Label lblCritterPowerType;
        private System.Windows.Forms.Label lblCritterPowerTypeLabel;
        private System.Windows.Forms.Label lblCritterPowerCategory;
        private System.Windows.Forms.Label lblCritterPowerCategoryLabel;
        private System.Windows.Forms.Label lblCritterPowerName;
        private System.Windows.Forms.Label lblCritterPowerNameLabel;
        private System.Windows.Forms.Button cmdDeleteCritterPower;
        private System.Windows.Forms.Button cmdAddCritterPower;
        private System.Windows.Forms.ToolStripMenuItem tsWeaponName;
        private System.Windows.Forms.ToolStripMenuItem tsWeaponMountLocation;
        private System.Windows.Forms.ToolStripMenuItem tsWeaponAddUnderbarrel;
        private System.Windows.Forms.Button cmdDeleteComplexForm;
        private System.Windows.Forms.Label lblComplexFormSource;
        private System.Windows.Forms.Label lblComplexFormSourceLabel;
        private System.Windows.Forms.TreeView treComplexForms;
        private ElasticComboBox cboStream;
        private LabelWithToolTip lblFadingAttributesValue;
        private System.Windows.Forms.Label lblFadingAttributes;
        private System.Windows.Forms.Label lblFadingAttributesLabel;
        private System.Windows.Forms.Label lblStreamLabel;
        private System.Windows.Forms.ContextMenuStrip cmsComplexForm;
        private System.Windows.Forms.ToolStripMenuItem tsAddComplexFormOption;
        private System.Windows.Forms.Label lblWeaponConceal;
        private System.Windows.Forms.Label lblWeaponConcealLabel;
        private System.Windows.Forms.ContextMenuStrip cmsGearButton;
        private System.Windows.Forms.ToolStripMenuItem tsGearButtonAddAccessory;
        private System.Windows.Forms.Button cmdDeleteQuality;
        private System.Windows.Forms.Button cmdAddQuality;
        private System.Windows.Forms.Label lblQualitySource;
        private System.Windows.Forms.Label lblQualitySourceLabel;
        private System.Windows.Forms.Label lblQualityBP;
        private System.Windows.Forms.Label lblQualityBPLabel;
        private System.Windows.Forms.CheckBox chkInitiationOrdeal;
        private System.Windows.Forms.Label lblMovement;
        private System.Windows.Forms.Label lblMovementLabel;
        private System.Windows.Forms.Label lblCritterPowerPoints;
        private System.Windows.Forms.Label lblCritterPowerPointsLabel;
        private System.Windows.Forms.Label lblCritterPowerPointCost;
        private System.Windows.Forms.Label lblCritterPowerPointCostLabel;
        private System.Windows.Forms.ToolStripMenuItem mnuSpecialChangeMetatype;
        private System.Windows.Forms.ToolStripMenuItem mnuSpecialChangeOptions;
        private System.Windows.Forms.ToolStripMenuItem mnuSpecialCreatePACKSKit;
        private LabelWithToolTip lblMemory;
        private System.Windows.Forms.Label lblMemoryLabel;
        private LabelWithToolTip lblLiftCarry;
        private System.Windows.Forms.Label lblLiftCarryLabel;
        private LabelWithToolTip lblJudgeIntentions;
        private System.Windows.Forms.Label lblJudgeIntentionsLabel;
        private LabelWithToolTip lblComposure;
        private System.Windows.Forms.Label lblComposureLabel;
        private System.Windows.Forms.CheckBox chkGearEquipped;
        private System.Windows.Forms.ToolStripMenuItem mnuSpecialMutantCritter;
        private System.Windows.Forms.ToolStripMenuItem mnuSpecialToxicCritter;
        private System.Windows.Forms.Label lblArmorCapacity;
        private System.Windows.Forms.Label lblArmorCapacityLabel;
        private System.Windows.Forms.ToolStripMenuItem tsAddArmorGear;
        private System.Windows.Forms.ContextMenuStrip cmsArmorGear;
        private System.Windows.Forms.ToolStripMenuItem tsArmorGearAddAsPlugin;
        private System.Windows.Forms.ToolStripMenuItem tsArmorNotes;
        private System.Windows.Forms.ToolStripMenuItem tsWeaponNotes;
        private System.Windows.Forms.ToolStripMenuItem tsCyberwareNotes;
        private System.Windows.Forms.ToolStripMenuItem tsVehicleCyberwareNotes;
        private System.Windows.Forms.ContextMenuStrip cmsArmorMod;
        private System.Windows.Forms.ToolStripMenuItem tsArmorModNotes;
        private System.Windows.Forms.ToolStripMenuItem tsArmorModAddAsPlugin;
        private System.Windows.Forms.ContextMenuStrip cmsQuality;
        private System.Windows.Forms.ToolStripMenuItem tsQualityNotes;
        private System.Windows.Forms.ToolStripMenuItem tsMartialArtsNotes;
        private System.Windows.Forms.ContextMenuStrip cmsSpell;
        private System.Windows.Forms.ToolStripMenuItem tsmSpellNotes;
        private System.Windows.Forms.ToolStripMenuItem tsComplexFormNotes;
        private System.Windows.Forms.ContextMenuStrip cmsCritterPowers;
        private System.Windows.Forms.ToolStripMenuItem tsCritterPowersNotes;
        private System.Windows.Forms.ToolStripMenuItem tsGearNotes;
        private System.Windows.Forms.ToolStripMenuItem tsVehicleNotes;
        private System.Windows.Forms.ToolStripMenuItem tsVehicleWeaponMountNotes;
        private System.Windows.Forms.ContextMenuStrip cmsLifestyleNotes;
        private System.Windows.Forms.ToolStripMenuItem tsLifestyleNotes;
        private System.Windows.Forms.ToolStripMenuItem tsArmorGearNotes;
        private System.Windows.Forms.ContextMenuStrip cmsWeaponAccessory;
        private System.Windows.Forms.ToolStripMenuItem tsWeaponAccessoryNotes;
        private System.Windows.Forms.ToolStripMenuItem tsVehicleWeaponNotes;
        private System.Windows.Forms.ContextMenuStrip cmsGearPlugin;
        private System.Windows.Forms.ToolStripMenuItem tsGearPluginNotes;
        private System.Windows.Forms.ContextMenuStrip cmsComplexFormPlugin;
        private System.Windows.Forms.ToolStripMenuItem tsComplexFormPluginNotes;
        private System.Windows.Forms.ToolStripMenuItem tsVehicleAddWeaponWeapon;
        private System.Windows.Forms.ToolStripMenuItem tsVehicleAddGear;
        private System.Windows.Forms.ToolStripMenuItem mnuSpecialCyberzombie;
        private System.Windows.Forms.ToolStripMenuItem tsVehicleAddUnderbarrelWeapon;
        private System.Windows.Forms.ToolStripMenuItem tsVehicleAddUnderbarrelWeaponAlt;
        private System.Windows.Forms.ToolStripMenuItem tsVehicleName;
        private System.Windows.Forms.ToolStripMenuItem mnuSpecialCreateCyberwareSuite;
        private System.Windows.Forms.ToolStripMenuItem tsVehicleAddCyberware;
        private System.Windows.Forms.Label lblFly;
        private System.Windows.Forms.Label lblFlyLabel;
        private System.Windows.Forms.Label lblSwim;
        private System.Windows.Forms.Label lblSwimLabel;
        private System.Windows.Forms.Button cmdAddLocation;
        private System.Windows.Forms.ToolStripMenuItem tsArmorName;
        private System.Windows.Forms.SplitContainer splitMain;
        private System.Windows.Forms.Label lblAlias;
        private System.Windows.Forms.Label lblCharacterName;
        private System.Windows.Forms.TextBox txtCharacterName;
        private System.Windows.Forms.ContextMenuStrip cmsBioware;
        private System.Windows.Forms.ToolStripMenuItem tsBiowareNotes;
        private System.Windows.Forms.ContextMenuStrip cmsAdvancedLifestyle;
        private System.Windows.Forms.ToolStripMenuItem tsEditAdvancedLifestyle;
        private System.Windows.Forms.ContextMenuStrip cmsLifestyle;
        private System.Windows.Forms.ToolStripMenuItem tsAdvancedLifestyle;
        private System.Windows.Forms.ToolStripMenuItem tsAdvancedLifestyleNotes;
        private System.Windows.Forms.CheckBox chkGearHomeNode;
        private System.Windows.Forms.ToolStripMenuItem tsLifestyleName;
        private System.Windows.Forms.ToolStripMenuItem mnuFileClose;
        private System.Windows.Forms.ToolStripSeparator tssFileMenu2;
        private System.Windows.Forms.ContextMenuStrip cmsGearLocation;
        private System.Windows.Forms.ToolStripMenuItem tsGearRenameLocation;
        private System.Windows.Forms.ToolStripMenuItem mnuSpecialReapplyImprovements;
        private System.Windows.Forms.ContextMenuStrip cmsSpellButton;
        private System.Windows.Forms.ToolStripMenuItem tsCreateSpell;
        private LabelWithToolTip lblPublicAwareTotal;
        private LabelWithToolTip lblNotorietyTotal;
        private LabelWithToolTip lblStreetCredTotal;
        private System.Windows.Forms.Label lblPublicAware;
        private System.Windows.Forms.Label lblNotoriety;
        private System.Windows.Forms.Label lblStreetCred;
        private System.Windows.Forms.Label lblWeaponDicePool;
        private System.Windows.Forms.Label lblWeaponDicePoolLabel;
        private System.Windows.Forms.ContextMenuStrip cmsArmorLocation;
        private System.Windows.Forms.ToolStripMenuItem tsArmorLocationAddArmor;
        private System.Windows.Forms.ToolStripMenuItem tsArmorRenameLocation;
        private System.Windows.Forms.Button cmdAddArmorBundle;
        private System.Windows.Forms.Button cmdArmorUnEquipAll;
        private System.Windows.Forms.Button cmdArmorEquipAll;
        private System.Windows.Forms.Label lblArmorEquipped;
        private System.Windows.Forms.Label lblArmorEquippedLabel;
        private System.Windows.Forms.ToolStripMenuItem tsEditLifestyle;
        private System.Windows.Forms.ContextMenuStrip cmsCyberwareGear;
        private System.Windows.Forms.ContextMenuStrip cmsVehicleCyberwareGear;
        private System.Windows.Forms.ToolStripMenuItem tsCyberwareGearMenuAddAsPlugin;
        private System.Windows.Forms.ToolStripMenuItem tsVehicleCyberwareGearMenuAddAsPlugin;
        private System.Windows.Forms.ContextMenuStrip cmsWeaponAccessoryGear;
        private System.Windows.Forms.ToolStripMenuItem tsWeaponAccessoryGearMenuAddAsPlugin;
        private System.Windows.Forms.ToolStripMenuItem tsWeaponAccessoryAddGear;
        private System.Windows.Forms.ToolStripMenuItem tsCyberwareAddGear;
        private System.Windows.Forms.ToolStripMenuItem tsVehicleCyberwareAddGear;
        private System.Windows.Forms.Button cmdAddVehicleLocation;
        private System.Windows.Forms.ContextMenuStrip cmsVehicleLocation;
        private System.Windows.Forms.ToolStripMenuItem tsVehicleRenameLocation;
        private System.Windows.Forms.ToolStripSeparator tssSeparator;
        private System.Windows.Forms.ToolStripButton tsbCopy;
        private System.Windows.Forms.ToolStripButton tsbPaste;
        private System.Windows.Forms.ToolStripMenuItem mnuCreateEdit;
        private System.Windows.Forms.ToolStripMenuItem mnuEditCopy;
        private System.Windows.Forms.ToolStripMenuItem mnuEditPaste;
        private System.Windows.Forms.ToolStripMenuItem tsCreateNaturalWeapon;
        private System.Windows.Forms.ContextMenuStrip cmsVehicleWeaponAccessory;
        private System.Windows.Forms.ToolStripMenuItem tsVehicleWeaponAccessoryAddGear;
        private System.Windows.Forms.ToolStripMenuItem tsVehicleWeaponAccessoryNotes;
        private System.Windows.Forms.ContextMenuStrip cmsVehicleWeaponAccessoryGear;
        private System.Windows.Forms.ToolStripMenuItem tsVehicleWeaponAccessoryGearMenuAddAsPlugin;
        private System.Windows.Forms.TabPage tabPets;
        private SplitButton cmdAddPet;
        private System.Windows.Forms.FlowLayoutPanel panPets;
        private System.Windows.Forms.CheckBox chkCritterPowerCount;
        private System.Windows.Forms.CheckBox chkIncludedInArmor;
        private System.Windows.Forms.ToolStripMenuItem mnuSpecialAddBiowareSuite;
        private System.Windows.Forms.ToolStripMenuItem mnuSpecialCreateBiowareSuite;
        private System.Windows.Forms.Button cmdAddWeaponLocation;
        private System.Windows.Forms.ContextMenuStrip cmsWeaponLocation;
        private System.Windows.Forms.ToolStripMenuItem tsWeaponRenameLocation;
        private System.Windows.Forms.ToolStripMenuItem tsGearName;
        private System.Windows.Forms.ToolStripMenuItem tsVehicleGearNotes;
        private System.Windows.Forms.CheckBox chkCommlinks;
        private System.Windows.Forms.CheckBox chkGearActiveCommlink;
        private System.Windows.Forms.ToolStripMenuItem mnuSpecialBPAvailLimit;
        private System.Windows.Forms.ToolStripMenuItem mnuSpecialConvertToFreeSprite;
        private System.Windows.Forms.Label lblBiowareESS;
        private System.Windows.Forms.Label lblCyberwareESS;
        private System.Windows.Forms.Label lblBiowareESSLabel;
        private System.Windows.Forms.Label lblCyberwareESSLabel;
        private System.Windows.Forms.Label lblEssenceHoleESS;
        private System.Windows.Forms.Label lblEssenceHoleESSLabel;
        private LabelWithToolTip lblRiggingINI;
        private System.Windows.Forms.Label lblRiggingINILabel;
        private LabelWithToolTip lblMatrixINIHot;
        private System.Windows.Forms.Label lblMatrixINIHotLabel;
        private LabelWithToolTip lblMatrixINICold;
        private System.Windows.Forms.Label lblMatrixINIColdLabel;
        private SplitButton cmdAddLifestyle;
        private System.Windows.Forms.TabPage tabLimits;
        private System.Windows.Forms.Label lblWeaponAccuracy;
        private System.Windows.Forms.Label lblWeaponAccuracyLabel;
        private SplitButton cmdAddMartialArt;
        private SplitButton cmdAddSpell;
        private System.Windows.Forms.Button cmdAddComplexForm;
        private SplitButton cmdAddCyberware;
        private SplitButton cmdAddArmor;
        private SplitButton cmdAddWeapon;
        private SplitButton cmdAddGear;
        private SplitButton cmdAddVehicle;
        private System.Windows.Forms.Label lblSpellDicePool;
        private System.Windows.Forms.Label lblSpellDicePoolLabel;
        private System.Windows.Forms.Label lblMentorSpirit;
        private System.Windows.Forms.Label lblMentorSpiritLabel;
        private System.Windows.Forms.Label lblMentorSpiritInformation;
        private LabelWithToolTip lblDrainAttributesValue;
        private System.Windows.Forms.Label lblDrainAttributes;
        private System.Windows.Forms.Label lblDrainAttributesLabel;
        private ElasticComboBox cboTradition;
        private System.Windows.Forms.Label lblTraditionLabel;
        private System.Windows.Forms.Label lblSpellSource;
        private System.Windows.Forms.Label lblSpellSourceLabel;
        private System.Windows.Forms.Label lblSpellType;
        private System.Windows.Forms.Label lblSpellTypeLabel;
        private System.Windows.Forms.Label lblSpellDV;
        private System.Windows.Forms.Label lblSpellDVLabel;
        private System.Windows.Forms.Label lblSpellDuration;
        private System.Windows.Forms.Label lblSpellDurationLabel;
        private System.Windows.Forms.Label lblSpellDamage;
        private System.Windows.Forms.Label lblSpellDamageLabel;
        private System.Windows.Forms.Label lblSpellRange;
        private System.Windows.Forms.Label lblSpellRangeLabel;
        private System.Windows.Forms.Label lblSpellCategory;
        private System.Windows.Forms.Label lblSpellCategoryLabel;
        private System.Windows.Forms.Label lblSpellDescriptors;
        private System.Windows.Forms.Label lblSpellDescriptorsLabel;
        private System.Windows.Forms.TreeView treSpells;
        private System.Windows.Forms.Button cmdDeleteSpell;
        private System.Windows.Forms.Label lblFV;
        private System.Windows.Forms.Label lblFVLabel;
        private System.Windows.Forms.Label lblDuration;
        private System.Windows.Forms.Label lblDurationLabel;
        private System.Windows.Forms.Label lblTarget;
        private System.Windows.Forms.Label lblTargetLabel;
        private System.Windows.Forms.Label lblArmorValueLabel;
        private System.Windows.Forms.Label lblArmorValue;
        private System.Windows.Forms.Label lblGearFirewallLabel;
        private System.Windows.Forms.Label lblGearDataProcessingLabel;
        private System.Windows.Forms.Label lblGearSleazeLabel;
        private System.Windows.Forms.Label lblGearAttackLabel;
        private System.Windows.Forms.Label lblCyberFirewallLabel;
        private System.Windows.Forms.Label lblCyberDataProcessingLabel;
        private System.Windows.Forms.Label lblCyberSleazeLabel;
        private System.Windows.Forms.Label lblCyberAttackLabel;
        private System.Windows.Forms.Label lblCyberDeviceRating;
        private System.Windows.Forms.Label lblCyberDeviceRatingLabel;
        private System.Windows.Forms.Label lblWeaponFirewall;
        private System.Windows.Forms.Label lblWeaponFirewallLabel;
        private System.Windows.Forms.Label lblWeaponDataProcessing;
        private System.Windows.Forms.Label lblWeaponDataProcessingLabel;
        private System.Windows.Forms.Label lblWeaponSleaze;
        private System.Windows.Forms.Label lblWeaponSleazeLabel;
        private System.Windows.Forms.Label lblWeaponAttack;
        private System.Windows.Forms.Label lblWeaponAttackLabel;
        private System.Windows.Forms.Label lblWeaponDeviceRating;
        private System.Windows.Forms.Label lblWeaponDeviceRatingLabel;
        private System.Windows.Forms.Label lblArmorFirewall;
        private System.Windows.Forms.Label lblArmorFirewallLabel;
        private System.Windows.Forms.Label lblArmorDataProcessing;
        private System.Windows.Forms.Label lblArmorDataProcessingLabel;
        private System.Windows.Forms.Label lblArmorSleaze;
        private System.Windows.Forms.Label lblArmorSleazeLabel;
        private System.Windows.Forms.Label lblArmorAttack;
        private System.Windows.Forms.Label lblArmorAttackLabel;
        private System.Windows.Forms.Label lblArmorDeviceRating;
        private System.Windows.Forms.Label lblArmorDeviceRatingLabel;
	    private ElasticComboBox cboDrain;
		private System.Windows.Forms.TextBox txtTraditionName;
        private System.Windows.Forms.Label lblTraditionName;
        private ElasticComboBox cboSpiritCombat;
        private System.Windows.Forms.Label lblSpiritCombat;
        private ElasticComboBox cboSpiritManipulation;
        private System.Windows.Forms.Label lblSpiritManipulation;
        private ElasticComboBox cboSpiritIllusion;
        private System.Windows.Forms.Label lblSpiritIllusion;
        private ElasticComboBox cboSpiritHealth;
        private System.Windows.Forms.Label lblSpiritHealth;
        private ElasticComboBox cboSpiritDetection;
        private System.Windows.Forms.Label lblSpiritDetection;
        private System.Windows.Forms.Label lblKarma;
        private System.Windows.Forms.TabControl tabPeople;
        private System.Windows.Forms.TabPage tabContacts;
        private SplitButton cmdAddContact;
        private System.Windows.Forms.FlowLayoutPanel panContacts;
        private System.Windows.Forms.TabPage tabEnemies;
        private System.Windows.Forms.FlowLayoutPanel panEnemies;
        private SplitButton cmdAddEnemy;
        private System.Windows.Forms.CheckBox chkInitiationSchooling;
        private System.Windows.Forms.ContextMenuStrip cmsInitiationNotes;
        private System.Windows.Forms.ToolStripMenuItem tsInitiationNotes;
        private System.Windows.Forms.ContextMenuStrip cmsMetamagic;
        private System.Windows.Forms.ToolStripMenuItem tsMetamagicAddArt;
        private System.Windows.Forms.ToolStripMenuItem tsMetamagicAddEnchantment;
        private System.Windows.Forms.ToolStripMenuItem tsMetamagicAddEnhancement;
        private System.Windows.Forms.ToolStripMenuItem tsMetamagicAddMetamagic;
        private System.Windows.Forms.ToolStripMenuItem tsMetamagicAddRitual;
        private System.Windows.Forms.ToolStripMenuItem tsMetamagicNotes;
        private System.Windows.Forms.Label lblContactArchtypeLabel;
        private System.Windows.Forms.Label lblContactLocationLabel;
        private System.Windows.Forms.Label lblContactNameLabel;
        private System.Windows.Forms.Label lblEnemyArchetypeLabel;
        private System.Windows.Forms.Label lblEnemyLocationLabel;
        private System.Windows.Forms.Label lblEnemyNameLabel;
        private System.Windows.Forms.ContextMenuStrip cmsTechnique;
        private System.Windows.Forms.ToolStripMenuItem tsAddTechniqueNotes;
        private System.Windows.Forms.Label lblContactPoints;
        private System.Windows.Forms.Label lblContactPoints_Label;
        private System.Windows.Forms.Button cmdLifeModule;
        private System.Windows.Forms.TreeView treCritterPowers;
        private System.Windows.Forms.Label lblWeaponRating;
        private System.Windows.Forms.Label lblWeaponRatingLabel;
        private System.Windows.Forms.Button btnCreateBackstory;
        private ElasticComboBox cboGearDataProcessing;
        private ElasticComboBox cboGearFirewall;
        private ElasticComboBox cboGearSleaze;
        private ElasticComboBox cboGearAttack;
        private System.Windows.Forms.TabPage tabDefenses;
        private System.Windows.Forms.Label lblSpellDefenseDirectSoakPhysicalLabel;
        private LabelWithToolTip lblSpellDefenseDirectSoakMana;
        private System.Windows.Forms.Label lblSpellDefenseDirectSoakManaLabel;
        private LabelWithToolTip lblSpellDefenseIndirectSoak;
        private System.Windows.Forms.Label lblSpellDefenseIndirectSoakLabel;
        private LabelWithToolTip lblSpellDefenseIndirectDodge;
        private System.Windows.Forms.Label lblSpellDefenseIndirectDodgeLabel;
        private System.Windows.Forms.Label lblCounterspellingDiceLabel;
        private System.Windows.Forms.NumericUpDown nudCounterspellingDice;
        private LabelWithToolTip lblSpellDefenseManipPhysical;
        private System.Windows.Forms.Label lblSpellDefenseManipPhysicalLabel;
        private LabelWithToolTip lblSpellDefenseManipMental;
        private System.Windows.Forms.Label lblSpellDefenseManipMentalLabel;
        private LabelWithToolTip lblSpellDefenseIllusionPhysical;
        private System.Windows.Forms.Label lblSpellDefenseIllusionPhysicalLabel;
        private LabelWithToolTip lblSpellDefenseIllusionMana;
        private System.Windows.Forms.Label lblSpellDefenseIllusionManaLabel;
        private LabelWithToolTip lblSpellDefenseDecAttWIL;
        private LabelWithToolTip lblSpellDefenseDecAttLOG;
        private LabelWithToolTip lblSpellDefenseDecAttINT;
        private LabelWithToolTip lblSpellDefenseDecAttCHA;
        private LabelWithToolTip lblSpellDefenseDecAttSTR;
        private System.Windows.Forms.Label lblSpellDefenseDecAttWILLabel;
        private System.Windows.Forms.Label lblSpellDefenseDecAttLOGLabel;
        private System.Windows.Forms.Label lblSpellDefenseDecAttINTLabel;
        private System.Windows.Forms.Label lblSpellDefenseDecAttCHALabel;
        private System.Windows.Forms.Label lblSpellDefenseDecAttSTRLabel;
        private System.Windows.Forms.Label lblSpellDefenseDecAttREALabel;
        private System.Windows.Forms.Label lblSpellDefenseDecAttAGILabel;
        private LabelWithToolTip lblSpellDefenseDecAttREA;
        private LabelWithToolTip lblSpellDefenseDecAttAGI;
        private LabelWithToolTip lblSpellDefenseDecAttBOD;
        private System.Windows.Forms.Label lblSpellDefenseDecAttBODLabel;
        private LabelWithToolTip lblSpellDefenseDetection;
        private System.Windows.Forms.Label lblSpellDefenseDetectionLabel;
        private LabelWithToolTip lblSpellDefenseDirectSoakPhysical;
        private System.Windows.Forms.Label lblCyberlimbSTR;
        private System.Windows.Forms.Label lblCyberlimbAGI;
        private System.Windows.Forms.Label lblCyberlimbSTRLabel;
        private System.Windows.Forms.Label lblCyberlimbAGILabel;
        private System.Windows.Forms.TabPage tabSkills;
        private UI.Skills.SkillsTabUserControl tabSkillUc;
        private System.Windows.Forms.Button cmdCreateStackedFocus;
        private System.Windows.Forms.TreeView treFoci;
        private System.Windows.Forms.Label lblAINormalProgramsBP;
        private System.Windows.Forms.Label lblBuildAINormalPrograms;
        private System.Windows.Forms.Label lblAIAdvancedProgramsBP;
        private System.Windows.Forms.Label lblBuildAIAdvancedPrograms;
        private System.Windows.Forms.TabPage tabAdvancedPrograms;
        private System.Windows.Forms.Button cmdAddAIProgram;
        private System.Windows.Forms.Label lblAIProgramsRequires;
        private System.Windows.Forms.Label lblAIProgramsRequiresLabel;
        private System.Windows.Forms.Label lblAIProgramsSource;
        private System.Windows.Forms.Label lblAIProgramsSourceLabel;
        private System.Windows.Forms.TreeView treAIPrograms;
        private System.Windows.Forms.Button cmdDeleteAIProgram;
        private System.Windows.Forms.ContextMenuStrip cmsAdvancedProgram;
        private System.Windows.Forms.ToolStripMenuItem tsAddAdvancedProgramOption;
        private System.Windows.Forms.ToolStripMenuItem tsAIProgramNotes;
        private System.Windows.Forms.Label lblHandedness;
        private ElasticComboBox cboPrimaryArm;
        private System.Windows.Forms.CheckBox chkIsMainMugshot;
        private System.Windows.Forms.Label lblNumMugshots;
        private System.Windows.Forms.NumericUpDown nudMugshotIndex;
        private System.Windows.Forms.Label lblTraditionSource;
        private System.Windows.Forms.Label lblTraditionSourceLabel;
        private PowersTabUserControl tabPowerUc;
        private System.Windows.Forms.Label lblBuildRitualsBPLabel;
        private System.Windows.Forms.Label lblBuildRitualsBP;
        private System.Windows.Forms.Label lblBuildPrepsBPLabel;
        private System.Windows.Forms.Label lblBuildPrepsBP;
        private System.Windows.Forms.FlowLayoutPanel pnlAttributes;
        private System.Windows.Forms.Label lblPBuildSpecial;
        private System.Windows.Forms.Label lblPBuildSpecialLabel;
        private System.Windows.Forms.CheckBox chkPrototypeTranshuman;
        private System.Windows.Forms.Label lblQualityLevelLabel;
        private System.Windows.Forms.NumericUpDown nudQualityLevel;
        private System.Windows.Forms.Label lblWeaponRangeAlternate;
        private System.Windows.Forms.Label lblWeaponRangeMain;
        private System.Windows.Forms.Label lblWeaponAlternateRangeExtreme;
        private System.Windows.Forms.Label lblWeaponAlternateRangeLong;
        private System.Windows.Forms.Label lblWeaponAlternateRangeMedium;
        private System.Windows.Forms.Label lblWeaponAlternateRangeShort;
        private System.Windows.Forms.ToolStripMenuItem mnuSpecialKarmaValue;
        private System.Windows.Forms.Button cmdCyberwareChangeMount;
        private System.Windows.Forms.ContextMenuStrip cmsGearAllowRename;
        private System.Windows.Forms.ToolStripMenuItem tsGearAllowRenameExtra;
        private System.Windows.Forms.ToolStripMenuItem tsGearAllowRenameAddAsPlugin;
        private System.Windows.Forms.ToolStripMenuItem tsGearAllowRenameName;
        private System.Windows.Forms.ToolStripMenuItem tsGearAllowRenameNotes;
        private System.Windows.Forms.ToolStripMenuItem tsVehicleMountWeapon;
        private System.Windows.Forms.ToolStripMenuItem tsVehicleMountWeaponAdd;
        private System.Windows.Forms.ToolStripMenuItem tsVehicleMountWeaponAccessory;
        private System.Windows.Forms.ToolStripMenuItem tsVehicleMountWeaponUnderbarrel;
        private System.Windows.Forms.TabPage tabRelationships;
        private System.Windows.Forms.Button cmdContactsExpansionToggle;
        private System.Windows.Forms.Button cmdSwapContactOrder;
        private System.Windows.Forms.ToolStripMenuItem tsWeaponLocationAddWeapon;
        private System.Windows.Forms.ToolStripMenuItem tsGearLocationAddGear;
        private System.Windows.Forms.ToolStripMenuItem tsVehicleLocationAddVehicle;
        private System.Windows.Forms.Label lblPrototypeTranshumanESSLabel;
        private System.Windows.Forms.Label lblPrototypeTranshumanESS;
        private System.Windows.Forms.ToolStripMenuItem tsEditWeaponMount;
        private System.Windows.Forms.ContextMenuStrip cmsAddContact;
        private System.Windows.Forms.ToolStripMenuItem tsAddFromFile;
        private Chummer.BufferedTableLayoutPanel tlpKarmaSummary;
        private Chummer.BufferedTableLayoutPanel tlpOtherInfo;
        private Chummer.BufferedTableLayoutPanel tlpSpellDefense;
        private System.Windows.Forms.Label lblWeaponCapacity;
        private System.Windows.Forms.Label lblWeaponCapacityLabel;
        private Chummer.BufferedTableLayoutPanel tlpLifestyleDetails;
	    private System.Windows.Forms.TabPage tabDrugs;
	    private System.Windows.Forms.Button btnDeleteCustomDrug;
	    private System.Windows.Forms.TreeView treCustomDrugs;
	    private System.Windows.Forms.Button btnCreateCustomDrug;
	    private System.Windows.Forms.Label lblDrugComponents;
	    private System.Windows.Forms.Label lblDrugCost;
	    private System.Windows.Forms.Label lblDrugCostLabel;
	    private System.Windows.Forms.Label lblDrugAvail;
	    private System.Windows.Forms.Label lblDrugAvailabel;
	    private System.Windows.Forms.Label lblDrugCategory;
	    private System.Windows.Forms.Label lblDrugCategoryLabel;
	    private System.Windows.Forms.Label lblDrugName;
	    private System.Windows.Forms.Label lblDrugNameLabel;
	    private System.Windows.Forms.Label lblDrugAddictionRating;
	    private System.Windows.Forms.Label lblDrugAddictionThreshold;
	    private System.Windows.Forms.Label lblDrugAddictionThresholdLabel;
	    private System.Windows.Forms.Label lblDrugComponentsLabel;
	    private System.Windows.Forms.Label lblDrugAddictionRatingLabel;
	    private System.Windows.Forms.Label lblDrugGrade;
	    private System.Windows.Forms.Label lblDrugGradeLabel;
	    private System.Windows.Forms.NumericUpDown nudDrugQty;
	    private System.Windows.Forms.Label lblDrugQtyLabel;
        private Chummer.BufferedTableLayoutPanel tlpDrugInfo;
        private System.Windows.Forms.Label lblDrugEffectLabel;
        private System.Windows.Forms.Label lblDrugEffect;
        private Chummer.BufferedTableLayoutPanel tlpMagician;
        private Chummer.BufferedTableLayoutPanel tlpTechnomancer;
        private System.Windows.Forms.Label lblParagonInformation;
        private Chummer.BufferedTableLayoutPanel tlpAdvancedPrograms;
        private Chummer.BufferedTableLayoutPanel tlpCommon;
        private Chummer.BufferedTableLayoutPanel tlpCommonLeftSide;
        private System.Windows.Forms.TextBox txtAlias;
        private UI.Shared.LimitTabUserControl lmtControl;
        private Chummer.BufferedTableLayoutPanel tlpMartialArts;
        private Chummer.BufferedTableLayoutPanel tlpInitiation;
        private Chummer.BufferedTableLayoutPanel tlpCritter;
        private Chummer.BufferedTableLayoutPanel tlpCyberware;
        private Chummer.BufferedTableLayoutPanel tlpCyberwareMatrix;
        private Chummer.BufferedTableLayoutPanel tlpVehicles;
        private System.Windows.Forms.FlowLayoutPanel flpVehiclesButtons;
        private Chummer.BufferedTableLayoutPanel tlpCharacterInfo;
        private Chummer.BufferedTableLayoutPanel tlpGear;
        private Chummer.BufferedTableLayoutPanel tlpArmor;
        private Chummer.BufferedTableLayoutPanel tlpWeapons;
        private Chummer.BufferedTableLayoutPanel tlpWeaponsRanges;
        private System.Windows.Forms.TableLayoutPanel tlpContacts;
        private System.Windows.Forms.TableLayoutPanel tlpEnemies;
        private System.Windows.Forms.FlowLayoutPanel flpEnemiesButtons;
        private System.Windows.Forms.TableLayoutPanel tlpPets;
        private System.Windows.Forms.FlowLayoutPanel flpPetsButtons;
        private System.Windows.Forms.FlowLayoutPanel flpInitiationCheckBoxes;
        private System.Windows.Forms.CheckBox chkCyberwareHomeNode;
        private System.Windows.Forms.CheckBox chkCyberwareActiveCommlink;
        private System.Windows.Forms.FlowLayoutPanel flpCyberware;
        private System.Windows.Forms.GroupBox gpbEssenceConsumption;
        private System.Windows.Forms.TableLayoutPanel tlpCyberwareEssenceConsumption;
        private System.Windows.Forms.TableLayoutPanel tlpCyberwareCommon;
        private System.Windows.Forms.GroupBox gpbCyberwareMatrix;
        private System.Windows.Forms.GroupBox gpbCyberwareCommon;
        private System.Windows.Forms.ComboBox cboCyberwareAttack;
        private System.Windows.Forms.ComboBox cboCyberwareSleaze;
        private System.Windows.Forms.ComboBox cboCyberwareDataProcessing;
        private System.Windows.Forms.ComboBox cboCyberwareFirewall;
        private System.Windows.Forms.FlowLayoutPanel flpLifestyleDetails;
        private System.Windows.Forms.GroupBox gpbLifestyleCommon;
        private System.Windows.Forms.TableLayoutPanel tlpLifestyleCommon;
        private System.Windows.Forms.FlowLayoutPanel flpLifestyleIntervals;
        private System.Windows.Forms.FlowLayoutPanel flpDrugs;
        private System.Windows.Forms.GroupBox gpbDrugsCommon;
        private System.Windows.Forms.TableLayoutPanel tlpDrugsCommon;
        private System.Windows.Forms.FlowLayoutPanel flpArmor;
        private System.Windows.Forms.GroupBox gpbArmorCommon;
        private System.Windows.Forms.TableLayoutPanel tlpArmorCommon;
        private System.Windows.Forms.FlowLayoutPanel flpArmorCommonCheckBoxes;
        private System.Windows.Forms.GroupBox gpbArmorMatrix;
        private System.Windows.Forms.TableLayoutPanel tlpArmorMatrix;
        private System.Windows.Forms.GroupBox gpbArmorLocation;
        private System.Windows.Forms.TableLayoutPanel tlpArmorLocation;
        private System.Windows.Forms.FlowLayoutPanel flpArmorLocationButtons;
        private System.Windows.Forms.FlowLayoutPanel flpWeapons;
        private System.Windows.Forms.GroupBox gpbWeaponsCommon;
        private System.Windows.Forms.TableLayoutPanel tlpWeaponsCommon;
        private System.Windows.Forms.FlowLayoutPanel flpWeaponsCommonCheckBoxes;
        private System.Windows.Forms.GroupBox gpbWeaponsMatrix;
        private System.Windows.Forms.TableLayoutPanel tlpWeaponsMatrix;
        private System.Windows.Forms.GroupBox gpbWeaponsWeapon;
        private System.Windows.Forms.FlowLayoutPanel flpWeaponsWeapon;
        private System.Windows.Forms.TableLayoutPanel tlpWeaponsWeapon;
        private System.Windows.Forms.FlowLayoutPanel flpGear;
        private System.Windows.Forms.GroupBox gpbGearCommon;
        private System.Windows.Forms.TableLayoutPanel tlpGearCommon;
        private System.Windows.Forms.GroupBox gpbGearMatrix;
        private System.Windows.Forms.TableLayoutPanel tlpGearMatrix;
        private System.Windows.Forms.GroupBox gpbGearBondedFoci;
        private System.Windows.Forms.TableLayoutPanel tlpGearBondedFoci;
        private System.Windows.Forms.TreeView treMetamagic;
        private System.Windows.Forms.Label lblMetamagicSourceLabel;
        private System.Windows.Forms.Label lblMetamagicSource;
        private System.Windows.Forms.FlowLayoutPanel flpInitiation;
        private System.Windows.Forms.GroupBox gpbInitiationGroup;
        private System.Windows.Forms.TableLayoutPanel tlpInitiationGroup;
        private System.Windows.Forms.TextBox txtGroupNotes;
        private System.Windows.Forms.Label lblGroupName;
        private System.Windows.Forms.CheckBox chkInitiationGroup;
        private System.Windows.Forms.Label lblGroupNotes;
        private System.Windows.Forms.TextBox txtGroupName;
        private System.Windows.Forms.CheckBox chkJoinGroup;
        private System.Windows.Forms.GroupBox gpbInitiationType;
        private System.Windows.Forms.FlowLayoutPanel flpMagician;
        private System.Windows.Forms.GroupBox gpbMagicianSpell;
        private System.Windows.Forms.TableLayoutPanel tlpMagicianSpell;
        private System.Windows.Forms.GroupBox gpbMagicianTradition;
        private System.Windows.Forms.TableLayoutPanel tlpMagicianTradition;
        private System.Windows.Forms.GroupBox gpbMagicianMentorSpirit;
        private System.Windows.Forms.TableLayoutPanel tlpMagicianMentorSpirit;
        private System.Windows.Forms.Panel panSpirits;
        private System.Windows.Forms.Label lblMentorSpiritSourceLabel;
        private System.Windows.Forms.Label lblMentorSpiritSource;
        private System.Windows.Forms.FlowLayoutPanel flpFadingAttributesValue;
        private System.Windows.Forms.FlowLayoutPanel flpTechnomancer;
        private System.Windows.Forms.GroupBox gpbTechnomancerComplexForm;
        private System.Windows.Forms.TableLayoutPanel tlpTechnomancerComplexForm;
        private System.Windows.Forms.Label lblComplexFormDicePoolLabel;
        private System.Windows.Forms.GroupBox gpbTechnomancerStream;
        private System.Windows.Forms.TableLayoutPanel tlpTechnomancerStream;
        private System.Windows.Forms.GroupBox gpbTechnomancerParagon;
        private System.Windows.Forms.TableLayoutPanel tlpTechnomancerParagon;
        private System.Windows.Forms.Label lblComplexFormDicePool;
        private System.Windows.Forms.CheckBox chkCyberwareStolen;
        private System.Windows.Forms.CheckBox chkGearStolen;
        private System.Windows.Forms.CheckBox chkArmorStolen;
        private System.Windows.Forms.CheckBox chkWeaponStolen;
        private System.Windows.Forms.CheckBox chkDrugStolen;
        private System.Windows.Forms.ToolStripMenuItem mnuFileSaveAsCreated;
        private System.Windows.Forms.Label lblMetagenicQualities;
        private System.Windows.Forms.Label lblMetagenicQualitiesLabel;
        private LabelWithToolTip lblSurprise;
        private LabelWithToolTip lblSurpriseLabel;
        private LabelWithToolTip lblDodgeLabel;
        private LabelWithToolTip lblDodge;
        private System.Windows.Forms.GroupBox gpbDescription;
        private UI.Editors.RtfEditor rtfDescription;
        private System.Windows.Forms.GroupBox gpbBackground;
        private UI.Editors.RtfEditor rtfBackground;
        private System.Windows.Forms.TableLayoutPanel tlpLongTexts;
        private System.Windows.Forms.GroupBox gpbConcept;
        private UI.Editors.RtfEditor rtfConcept;
        private System.Windows.Forms.GroupBox gpbNotes;
        private UI.Editors.RtfEditor rtfNotes;
        private System.Windows.Forms.FlowLayoutPanel flpGearCommonCheckBoxes;
        private System.Windows.Forms.FlowLayoutPanel flpGearMatrixCheckBoxes;
        private System.Windows.Forms.FlowLayoutPanel flpCyberwareMatrixCheckBoxes;
        private BufferedTableLayoutPanel tlpQualityButtons;
        private System.Windows.Forms.FlowLayoutPanel flpAttributesLabels;
        private BufferedTableLayoutPanel tlpAlias;
        private BufferedTableLayoutPanel tlpCommonRightSide;
        private System.Windows.Forms.Label lblMetatypeLabel;
        private System.Windows.Forms.Label lblStolenNuyen;
        private System.Windows.Forms.Label lblMysticAdeptAssignment;
        private System.Windows.Forms.NumericUpDown nudMysticAdeptMAGMagician;
        private System.Windows.Forms.Label lblMetatype;
        private System.Windows.Forms.Label lblStolenNuyenLabel;
        private System.Windows.Forms.Label lblMetatypeSourceLabel;
        private System.Windows.Forms.Label lblMetatypeSource;
        private System.Windows.Forms.Label lblNuyen;
        private System.Windows.Forms.NumericUpDown nudNuyen;
        private System.Windows.Forms.Label lblNuyenTotal;
        private System.Windows.Forms.Label lblAstralReputation;
        private System.Windows.Forms.Label lblWildReputation;
        private LabelWithToolTip lblAstralReputationTotal;
        private LabelWithToolTip lblWildReputationTotal;
        private BufferedTableLayoutPanel tlpCommonLeftSideBottom;
        private BufferedTableLayoutPanel tlpMartialArtsButtons;
        private BufferedTableLayoutPanel tlpMagicianButtons;
        private BufferedTableLayoutPanel tlpDrainAttributes;
        private BufferedTableLayoutPanel tlpMugshotButtons;
        private BufferedTableLayoutPanel tlpTechnomancerButtons;
        private BufferedTableLayoutPanel tlpAdvancedProgramsButtons;
        private BufferedTableLayoutPanel tlpCritterButtons;
        private BufferedTableLayoutPanel tlpInitiationButtons;
        private System.Windows.Forms.FlowLayoutPanel flpCyberwareCommonCheckBoxes;
        private BufferedTableLayoutPanel tlpCyberwareButtons;
        private BufferedTableLayoutPanel tlpContactsButtons;
        private BufferedTableLayoutPanel tlpGearButtons;
        private BufferedTableLayoutPanel tlpArmorButtons;
        private BufferedTableLayoutPanel tlpWeaponsButtons;
        private BufferedTableLayoutPanel tlpDrugButtons;
        private BufferedTableLayoutPanel tlpLifestyleButtons;
        private BufferedTableLayoutPanel tlpNuyen;
        private System.Windows.Forms.FlowLayoutPanel flpVehicles;
        private System.Windows.Forms.GroupBox gpbVehiclesCommon;
        private System.Windows.Forms.TableLayoutPanel tlpVehiclesCommon;
        private System.Windows.Forms.Label lblVehicleNameLabel;
        private System.Windows.Forms.Label lblVehicleAvailLabel;
        private System.Windows.Forms.Label lblVehicleCostLabel;
        private System.Windows.Forms.Label lblVehicleAvail;
        private System.Windows.Forms.Label lblVehicleSlots;
        private System.Windows.Forms.Label lblVehicleCost;
        private System.Windows.Forms.Label lblVehicleSource;
        private System.Windows.Forms.Label lblVehicleSlotsLabel;
        private System.Windows.Forms.Label lblVehicleName;
        private System.Windows.Forms.Label lblVehicleSourceLabel;
        private System.Windows.Forms.Label lblVehicleCategory;
        private System.Windows.Forms.NumericUpDown nudVehicleGearQty;
        private System.Windows.Forms.Label lblVehicleCategoryLabel;
        private System.Windows.Forms.Label lblVehicleRatingLabel;
        private System.Windows.Forms.Label lblVehicleGearQtyLabel;
        private System.Windows.Forms.NumericUpDown nudVehicleRating;
        private System.Windows.Forms.Button cmdVehicleCyberwareChangeMount;
        private System.Windows.Forms.FlowLayoutPanel flpVehiclesCommonCheckBoxes;
        private System.Windows.Forms.CheckBox chkVehicleWeaponAccessoryInstalled;
        private System.Windows.Forms.CheckBox chkVehicleIncludedInWeapon;
        private System.Windows.Forms.CheckBox chkVehicleStolen;
        private System.Windows.Forms.GroupBox gpbVehiclesVehicle;
        private System.Windows.Forms.TableLayoutPanel tlpVehiclesVehicle;
        private System.Windows.Forms.Label lblVehicleHandlingLabel;
        private System.Windows.Forms.Label lblVehicleHandling;
        private System.Windows.Forms.Label lblVehicleAccelLabel;
        private System.Windows.Forms.Label lblVehicleAccel;
        private System.Windows.Forms.Label lblVehicleSpeedLabel;
        private System.Windows.Forms.Label lblVehicleSpeed;
        private System.Windows.Forms.Label lblVehicleCosmetic;
        private System.Windows.Forms.Label lblVehicleCosmeticLabel;
        private System.Windows.Forms.Label lblVehiclePilotLabel;
        private System.Windows.Forms.Label lblVehiclePilot;
        private System.Windows.Forms.Label lblVehicleDroneModSlots;
        private System.Windows.Forms.Label lblVehicleBodyLabel;
        private System.Windows.Forms.Label lblVehicleBody;
        private System.Windows.Forms.Label lblVehicleWeaponsmodLabel;
        private System.Windows.Forms.Label lblVehicleDroneModSlotsLabel;
        private System.Windows.Forms.Label lblVehicleSeats;
        private System.Windows.Forms.Label lblVehicleBodymodLabel;
        private System.Windows.Forms.Label lblVehicleArmorLabel;
        private System.Windows.Forms.Label lblVehicleArmor;
        private System.Windows.Forms.Label lblVehicleSeatsLabel;
        private System.Windows.Forms.Label lblVehicleSensorLabel;
        private System.Windows.Forms.Label lblVehicleSensor;
        private System.Windows.Forms.Label lblVehiclePowertrainLabel;
        private System.Windows.Forms.Label lblVehiclePowertrain;
        private System.Windows.Forms.Label lblVehicleWeaponsmod;
        private System.Windows.Forms.Label lblVehicleElectromagnetic;
        private System.Windows.Forms.Label lblVehicleElectromagneticLabel;
        private System.Windows.Forms.Label lblVehicleBodymod;
        private System.Windows.Forms.Label lblVehicleProtectionLabel;
        private System.Windows.Forms.Label lblVehicleProtection;
        private System.Windows.Forms.GroupBox gpbVehiclesWeapon;
        private System.Windows.Forms.FlowLayoutPanel flpVehiclesWeapon;
        private System.Windows.Forms.TableLayoutPanel tlpVehiclesWeaponCommon;
        private System.Windows.Forms.Label lblVehicleWeaponDamageLabel;
        private System.Windows.Forms.Label lblVehicleWeaponDamage;
        private System.Windows.Forms.Label lblVehicleWeaponAPLabel;
        private System.Windows.Forms.Label lblVehicleWeaponAP;
        private System.Windows.Forms.Label lblVehicleWeaponAccuracyLabel;
        private System.Windows.Forms.Label lblVehicleWeaponAccuracy;
        private System.Windows.Forms.Label lblVehicleWeaponModeLabel;
        private System.Windows.Forms.Label lblVehicleWeaponDicePoolLabel;
        private System.Windows.Forms.Label lblVehicleWeaponDicePool;
        private System.Windows.Forms.Label lblVehicleWeaponAmmoLabel;
        private System.Windows.Forms.Label lblVehicleWeaponAmmo;
        private ElasticComboBox cboVehicleWeaponFiringMode;
        private System.Windows.Forms.Label lblFiringModeLabel;
        private System.Windows.Forms.Label lblVehicleWeaponMode;
        private BufferedTableLayoutPanel tlpVehiclesWeaponRanges;
        private System.Windows.Forms.Label lblVehicleWeaponAlternateRangeExtreme;
        private System.Windows.Forms.Label lblVehicleWeaponRangeExtreme;
        private System.Windows.Forms.Label lblVehicleWeaponAlternateRangeLong;
        private System.Windows.Forms.Label lblVehicleWeaponRangeExtremeLabel;
        private System.Windows.Forms.Label lblVehicleWeaponRangeLabel;
        private System.Windows.Forms.Label lblVehicleWeaponAlternateRangeMedium;
        private System.Windows.Forms.Label lblVehicleWeaponRangeLong;
        private System.Windows.Forms.Label lblVehicleWeaponAlternateRangeShort;
        private System.Windows.Forms.Label lblVehicleWeaponRangeLongLabel;
        private System.Windows.Forms.Label lblVehicleWeaponRangeShortLabel;
        private System.Windows.Forms.Label lblVehicleWeaponRangeMedium;
        private System.Windows.Forms.Label lblVehicleWeaponRangeAlternate;
        private System.Windows.Forms.Label lblVehicleWeaponRangeMain;
        private System.Windows.Forms.Label lblVehicleWeaponRangeMediumLabel;
        private System.Windows.Forms.Label lblVehicleWeaponRangeShort;
        private System.Windows.Forms.GroupBox gpbVehiclesMatrix;
        private System.Windows.Forms.TableLayoutPanel tlpVehiclesMatrix;
        private System.Windows.Forms.Label lblVehicleDeviceLabel;
        private System.Windows.Forms.Label lblVehicleDevice;
        private ElasticComboBox cboVehicleAttack;
        private System.Windows.Forms.Label lblVehicleAttackLabel;
        private System.Windows.Forms.Label lblVehicleSleazeLabel;
        private ElasticComboBox cboVehicleSleaze;
        private System.Windows.Forms.Label lblVehicleDataProcessingLabel;
        private ElasticComboBox cboVehicleDataProcessing;
        private ElasticComboBox cboVehicleFirewall;
        private System.Windows.Forms.Label lblVehicleFirewallLabel;
        private System.Windows.Forms.FlowLayoutPanel flpVehiclesMatrixCheckBoxes;
        private System.Windows.Forms.CheckBox chkVehicleHomeNode;
        private System.Windows.Forms.CheckBox chkVehicleActiveCommlink;
        private System.Windows.Forms.TreeView treVehicles;
        private BufferedTableLayoutPanel tlpMagicianMentorSpiritHeader;
        private BufferedTableLayoutPanel tlpTechnomancerParagonHeader;
        private System.Windows.Forms.Label lblParagonLabel;
        private System.Windows.Forms.Label lblParagonSourceLabel;
        private System.Windows.Forms.Label lblParagon;
        private System.Windows.Forms.Label lblParagonSource;
    }
}
