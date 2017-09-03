using Chummer.helpers;
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
            if (disposing && (components != null))
            {
                components.Dispose();
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmCreate));
            System.Windows.Forms.TreeNode treeNode1 = new System.Windows.Forms.TreeNode("Selected Positive Qualities");
            System.Windows.Forms.TreeNode treeNode2 = new System.Windows.Forms.TreeNode("Selected Negative Qualities");
            System.Windows.Forms.TreeNode treeNode3 = new System.Windows.Forms.TreeNode("Physical");
            System.Windows.Forms.TreeNode treeNode4 = new System.Windows.Forms.TreeNode("Mental");
            System.Windows.Forms.TreeNode treeNode5 = new System.Windows.Forms.TreeNode("Social");
            System.Windows.Forms.TreeNode treeNode6 = new System.Windows.Forms.TreeNode("Selected Martial Arts");
            System.Windows.Forms.TreeNode treeNode7 = new System.Windows.Forms.TreeNode("Selected Qualities");
            System.Windows.Forms.TreeNode treeNode8 = new System.Windows.Forms.TreeNode("Selected Combat Spells");
            System.Windows.Forms.TreeNode treeNode9 = new System.Windows.Forms.TreeNode("Selected Detection Spells");
            System.Windows.Forms.TreeNode treeNode10 = new System.Windows.Forms.TreeNode("Selected Health Spells");
            System.Windows.Forms.TreeNode treeNode11 = new System.Windows.Forms.TreeNode("Selected Illusion Spells");
            System.Windows.Forms.TreeNode treeNode12 = new System.Windows.Forms.TreeNode("Selected Manipulation Spells");
            System.Windows.Forms.TreeNode treeNode13 = new System.Windows.Forms.TreeNode("Selected Rituals");
            System.Windows.Forms.TreeNode treeNode14 = new System.Windows.Forms.TreeNode("Selected Enchantments");
            System.Windows.Forms.TreeNode treeNode15 = new System.Windows.Forms.TreeNode("Selected Complex Forms");
            System.Windows.Forms.TreeNode treeNode16 = new System.Windows.Forms.TreeNode("Selected AI Programs and Advanced Programs");
            System.Windows.Forms.TreeNode treeNode17 = new System.Windows.Forms.TreeNode("Critter Powers");
            System.Windows.Forms.TreeNode treeNode18 = new System.Windows.Forms.TreeNode("Weaknesses");
            System.Windows.Forms.TreeNode treeNode19 = new System.Windows.Forms.TreeNode("Selected Cyberware");
            System.Windows.Forms.TreeNode treeNode20 = new System.Windows.Forms.TreeNode("Selected Bioware");
            System.Windows.Forms.TreeNode treeNode21 = new System.Windows.Forms.TreeNode("Selected Lifestyles");
            System.Windows.Forms.TreeNode treeNode22 = new System.Windows.Forms.TreeNode("Selected Armor");
            System.Windows.Forms.TreeNode treeNode23 = new System.Windows.Forms.TreeNode("Selected Weapons");
            System.Windows.Forms.TreeNode treeNode24 = new System.Windows.Forms.TreeNode("Selected Gear");
            System.Windows.Forms.TreeNode treeNode25 = new System.Windows.Forms.TreeNode("Selected Vehicles");
            this.StatusStrip = new System.Windows.Forms.StatusStrip();
            this.tssBPLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.tssBP = new System.Windows.Forms.ToolStripStatusLabel();
            this.tssBPRemainLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.tssBPRemain = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel3 = new System.Windows.Forms.ToolStripStatusLabel();
            this.tssEssence = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel4 = new System.Windows.Forms.ToolStripStatusLabel();
            this.tssNuyenRemaining = new System.Windows.Forms.ToolStripStatusLabel();
            this.pgbProgress = new System.Windows.Forms.ToolStripProgressBar();
            this.dlgSaveFile = new System.Windows.Forms.SaveFileDialog();
            this.tipTooltip = new TheArtOfDev.HtmlRenderer.WinForms.HtmlToolTip();
            this.lblAttributesAug = new System.Windows.Forms.Label();
            this.lblAttributesBase = new System.Windows.Forms.Label();
            this.lblAttributesMetatype = new System.Windows.Forms.Label();
            this.lblAttributes = new System.Windows.Forms.Label();
            this.lblNuyen = new System.Windows.Forms.Label();
            this.lblSpirits = new System.Windows.Forms.Label();
            this.lblComplexForms = new System.Windows.Forms.Label();
            this.lblSprites = new System.Windows.Forms.Label();
            this.chkArmorEquipped = new System.Windows.Forms.CheckBox();
            this.chkWeaponAccessoryInstalled = new System.Windows.Forms.CheckBox();
            this.chkVehicleWeaponAccessoryInstalled = new System.Windows.Forms.CheckBox();
            this.lblNotoriety = new System.Windows.Forms.Label();
            this.lblStreetCred = new System.Windows.Forms.Label();
            this.chkCharacterCreated = new System.Windows.Forms.CheckBox();
            this.lblBuildFoci = new System.Windows.Forms.Label();
            this.lblBuildManeuvers = new System.Windows.Forms.Label();
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
            this.label33 = new System.Windows.Forms.Label();
            this.lblSelectedSpells = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.lblSpellDefenceIndirectDodgeLabel = new System.Windows.Forms.Label();
            this.lblSpellDefenceIndirectSoakLabel = new System.Windows.Forms.Label();
            this.lblSpellDefenceDirectSoakManaLabel = new System.Windows.Forms.Label();
            this.lblSpellDefenceDirectSoakPhysicalLabel = new System.Windows.Forms.Label();
            this.lblSpellDefenceDecAttBODLabel = new System.Windows.Forms.Label();
            this.lblSpellDefenceDetectionLabel = new System.Windows.Forms.Label();
            this.lblSpellDefenceDecAttAGILabel = new System.Windows.Forms.Label();
            this.lblSpellDefenceDecAttSTRLabel = new System.Windows.Forms.Label();
            this.lblSpellDefenceDecAttREALabel = new System.Windows.Forms.Label();
            this.lblSpellDefenceDecAttWILLabel = new System.Windows.Forms.Label();
            this.lblSpellDefenceDecAttLOGLabel = new System.Windows.Forms.Label();
            this.lblSpellDefenceDecAttINTLabel = new System.Windows.Forms.Label();
            this.lblSpellDefenceDecAttCHALabel = new System.Windows.Forms.Label();
            this.lbllSpellDefenceManipPhysicalLabel = new System.Windows.Forms.Label();
            this.lblSpellDefenceManipMentalLabel = new System.Windows.Forms.Label();
            this.lblSpellDefenceIllusionPhysicalLabel = new System.Windows.Forms.Label();
            this.lblSpellDefenceIllusionManaLabel = new System.Windows.Forms.Label();
            this.lblCounterspellingDiceLabel = new System.Windows.Forms.Label();
            this.lblFoci = new System.Windows.Forms.Label();
            this.lblBuildAIAdvancedPrograms = new System.Windows.Forms.Label();
            this.lblBuildRitualsBPLabel = new System.Windows.Forms.Label();
            this.lblBuildPrepsBPLabel = new System.Windows.Forms.Label();
            this.lblAIProgramsAdvancedPrograms = new System.Windows.Forms.Label();
            this.lblPublicAware = new System.Windows.Forms.Label();
            this.lblSocialLabel = new System.Windows.Forms.Label();
            this.lblMentalLabel = new System.Windows.Forms.Label();
            this.lblPhysicalLabel = new System.Windows.Forms.Label();
            this.lblAstralLabel = new System.Windows.Forms.Label();
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
            this.tsWeaponAddModification = new System.Windows.Forms.ToolStripMenuItem();
            this.cmsGearButton = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsGearButtonAddAccessory = new System.Windows.Forms.ToolStripMenuItem();
            this.tsGearAddNexus = new System.Windows.Forms.ToolStripMenuItem();
            this.cmsVehicle = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsVehicleAddWeaponMount = new System.Windows.Forms.ToolStripMenuItem();
            this.tsVehicleAddMod = new System.Windows.Forms.ToolStripMenuItem();
            this.tsVehicleAddCyberware = new System.Windows.Forms.ToolStripMenuItem();
            this.tsVehicleAddSensor = new System.Windows.Forms.ToolStripMenuItem();
            this.tsVehicleAddGear = new System.Windows.Forms.ToolStripMenuItem();
            this.tsVehicleSensorAddAsPlugin = new System.Windows.Forms.ToolStripMenuItem();
            this.tsVehicleAddNexus = new System.Windows.Forms.ToolStripMenuItem();
            this.tsVehicleAddWeapon = new System.Windows.Forms.ToolStripMenuItem();
            this.tsVehicleAddWeaponWeapon = new System.Windows.Forms.ToolStripMenuItem();
            this.tsVehicleAddWeaponAccessory = new System.Windows.Forms.ToolStripMenuItem();
            this.tsVehicleAddUnderbarrelWeapon = new System.Windows.Forms.ToolStripMenuItem();
            this.tsVehicleName = new System.Windows.Forms.ToolStripMenuItem();
            this.tsVehicleNotes = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuCreateMenu = new System.Windows.Forms.MenuStrip();
            this.mnuCreateFile = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuFileSave = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuFileSaveAs = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.mnuFileClose = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
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
            this.mnuSpecialMutantCritter = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuSpecialToxicCritter = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuSpecialCyberzombie = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuSpecialConvertToFreeSprite = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuSpecialReapplyImprovements = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuSpecialBPAvailLimit = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuSpecialConfirmValidity = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStrip = new System.Windows.Forms.ToolStrip();
            this.tsbSave = new System.Windows.Forms.ToolStripButton();
            this.tsbPrint = new System.Windows.Forms.ToolStripButton();
            this.tsbSeparator = new System.Windows.Forms.ToolStripSeparator();
            this.tsbCopy = new System.Windows.Forms.ToolStripButton();
            this.tsbPaste = new System.Windows.Forms.ToolStripButton();
            this.cmsGear = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsGearAddAsPlugin = new System.Windows.Forms.ToolStripMenuItem();
            this.tsGearName = new System.Windows.Forms.ToolStripMenuItem();
            this.tsGearNotes = new System.Windows.Forms.ToolStripMenuItem();
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
            this.cmsQuality = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsQualityNotes = new System.Windows.Forms.ToolStripMenuItem();
            this.cmsSpell = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsSpellNotes = new System.Windows.Forms.ToolStripMenuItem();
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
            this.cmsArmorLocation = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsArmorRenameLocation = new System.Windows.Forms.ToolStripMenuItem();
            this.cmsCyberwareGear = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsCyberwareGearMenuAddAsPlugin = new System.Windows.Forms.ToolStripMenuItem();
            this.cmsWeaponAccessoryGear = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsWeaponAccessoryGearMenuAddAsPlugin = new System.Windows.Forms.ToolStripMenuItem();
            this.cmsVehicleLocation = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsVehicleRenameLocation = new System.Windows.Forms.ToolStripMenuItem();
            this.cmsVehicleWeaponAccessory = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsVehicleWeaponAccessoryAddGear = new System.Windows.Forms.ToolStripMenuItem();
            this.tsVehicleWeaponAccessoryNotes = new System.Windows.Forms.ToolStripMenuItem();
            this.cmsVehicleWeaponAccessoryGear = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsVehicleWeaponAccessoryGearMenuAddAsPlugin = new System.Windows.Forms.ToolStripMenuItem();
            this.cmsWeaponLocation = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsWeaponRenameLocation = new System.Windows.Forms.ToolStripMenuItem();
            this.splitMain = new System.Windows.Forms.SplitContainer();
            this.tabCharacterTabs = new System.Windows.Forms.TabControl();
            this.tabCommon = new System.Windows.Forms.TabPage();
            this.pnlAttributes = new System.Windows.Forms.FlowLayoutPanel();
            this.cmdLifeModule = new System.Windows.Forms.Button();
            this.tabPeople = new System.Windows.Forms.TabControl();
            this.tabContacts = new System.Windows.Forms.TabPage();
            this.lblContactPoints = new System.Windows.Forms.Label();
            this.lblContactPoints_Label = new System.Windows.Forms.Label();
            this.lblContactArchtypeLabel = new System.Windows.Forms.Label();
            this.lblContactLocationLabel = new System.Windows.Forms.Label();
            this.lblContactNameLabel = new System.Windows.Forms.Label();
            this.cmdAddContact = new System.Windows.Forms.Button();
            this.panContacts = new System.Windows.Forms.FlowLayoutPanel();
            this.tabEnemies = new System.Windows.Forms.TabPage();
            this.label6 = new System.Windows.Forms.Label();
            this.label15 = new System.Windows.Forms.Label();
            this.label16 = new System.Windows.Forms.Label();
            this.panEnemies = new System.Windows.Forms.FlowLayoutPanel();
            this.cmdAddEnemy = new System.Windows.Forms.Button();
            this.txtAlias = new System.Windows.Forms.TextBox();
            this.lblAlias = new System.Windows.Forms.Label();
            this.lblMetatypeSource = new System.Windows.Forms.Label();
            this.lblMetatypeSourceLabel = new System.Windows.Forms.Label();
            this.lblQualityBP = new System.Windows.Forms.Label();
            this.lblQualityBPLabel = new System.Windows.Forms.Label();
            this.lblQualitySource = new System.Windows.Forms.Label();
            this.lblQualitySourceLabel = new System.Windows.Forms.Label();
            this.cmdDeleteQuality = new System.Windows.Forms.Button();
            this.cmdAddQuality = new System.Windows.Forms.Button();
            this.nudMysticAdeptMAGMagician = new System.Windows.Forms.NumericUpDown();
            this.lblMysticAdeptAssignment = new System.Windows.Forms.Label();
            this.treQualities = new Chummer.helpers.TreeView();
            this.lblMetatype = new System.Windows.Forms.Label();
            this.lblMetatypeLabel = new System.Windows.Forms.Label();
            this.nudNuyen = new System.Windows.Forms.NumericUpDown();
            this.lblNuyenTotal = new System.Windows.Forms.Label();
            this.tabSkills = new System.Windows.Forms.TabPage();
            this.tabSkillUc = new Chummer.UI.Skills.SkillsTabUserControl();
            this.tabLimits = new System.Windows.Forms.TabPage();
            this.lblAstral = new System.Windows.Forms.Label();
            this.lblSocial = new System.Windows.Forms.Label();
            this.lblMental = new System.Windows.Forms.Label();
            this.lblPhysical = new System.Windows.Forms.Label();
            this.cmdAddLimitModifier = new System.Windows.Forms.Button();
            this.treLimit = new Chummer.helpers.TreeView();
            this.cmdDeleteLimitModifier = new System.Windows.Forms.Button();
            this.tabMartialArts = new System.Windows.Forms.TabPage();
            this.cmdAddMartialArt = new SplitButton();
            this.lblMartialArtSource = new System.Windows.Forms.Label();
            this.lblMartialArtSourceLabel = new System.Windows.Forms.Label();
            this.treMartialArts = new Chummer.helpers.TreeView();
            this.cmdDeleteMartialArt = new System.Windows.Forms.Button();
            this.tabMagician = new System.Windows.Forms.TabPage();
            this.lblTraditionSource = new System.Windows.Forms.Label();
            this.lblTraditionSourceLabel = new System.Windows.Forms.Label();
            this.cboSpiritManipulation = new Chummer.helpers.ComboBox();
            this.lblSpiritManipulation = new System.Windows.Forms.Label();
            this.cboSpiritIllusion = new Chummer.helpers.ComboBox();
            this.lblSpiritIllusion = new System.Windows.Forms.Label();
            this.cboSpiritHealth = new Chummer.helpers.ComboBox();
            this.lblSpiritHealth = new System.Windows.Forms.Label();
            this.cboSpiritDetection = new Chummer.helpers.ComboBox();
            this.lblSpiritDetection = new System.Windows.Forms.Label();
            this.cboSpiritCombat = new Chummer.helpers.ComboBox();
            this.lblSpiritCombat = new System.Windows.Forms.Label();
            this.cboDrain = new Chummer.helpers.ComboBox();
            this.txtTraditionName = new System.Windows.Forms.TextBox();
            this.lblTraditionName = new System.Windows.Forms.Label();
            this.lblSpellDicePool = new System.Windows.Forms.Label();
            this.lblSpellDicePoolLabel = new System.Windows.Forms.Label();
            this.lblMentorSpirit = new System.Windows.Forms.Label();
            this.lblMentorSpiritLabel = new System.Windows.Forms.Label();
            this.lblMentorSpiritInformation = new System.Windows.Forms.Label();
            this.lblDrainAttributesValue = new System.Windows.Forms.Label();
            this.lblDrainAttributes = new System.Windows.Forms.Label();
            this.lblDrainAttributesLabel = new System.Windows.Forms.Label();
            this.cboTradition = new Chummer.helpers.ComboBox();
            this.lblTraditionLabel = new System.Windows.Forms.Label();
            this.lblSpellSource = new System.Windows.Forms.Label();
            this.lblSpellSourceLabel = new System.Windows.Forms.Label();
            this.lblSpellType = new System.Windows.Forms.Label();
            this.lblSpellTypeLabel = new System.Windows.Forms.Label();
            this.lblSpellDV = new System.Windows.Forms.Label();
            this.lblSpellDVLabel = new System.Windows.Forms.Label();
            this.lblSpellDuration = new System.Windows.Forms.Label();
            this.lblSpellDurationLabel = new System.Windows.Forms.Label();
            this.lblSpellDamage = new System.Windows.Forms.Label();
            this.lblSpellDamageLabel = new System.Windows.Forms.Label();
            this.lblSpellRange = new System.Windows.Forms.Label();
            this.lblSpellRangeLabel = new System.Windows.Forms.Label();
            this.lblSpellCategory = new System.Windows.Forms.Label();
            this.lblSpellCategoryLabel = new System.Windows.Forms.Label();
            this.lblSpellDescriptors = new System.Windows.Forms.Label();
            this.lblSpellDescriptorsLabel = new System.Windows.Forms.Label();
            this.treSpells = new Chummer.helpers.TreeView();
            this.cmdDeleteSpell = new System.Windows.Forms.Button();
            this.cmdAddSpirit = new System.Windows.Forms.Button();
            this.panSpirits = new System.Windows.Forms.Panel();
            this.cmdAddSpell = new SplitButton();
            this.tabAdept = new System.Windows.Forms.TabPage();
            this.tabPowerUc = new Chummer.UI.Powers.PowersTabUserControl();
            this.tabTechnomancer = new System.Windows.Forms.TabPage();
            this.lblLivingPersonaFirewall = new System.Windows.Forms.Label();
            this.lblLivingPersonaFirewallLabel = new System.Windows.Forms.Label();
            this.lblLivingPersonaDataProcessing = new System.Windows.Forms.Label();
            this.lblLivingPersonaDataProcessingLabel = new System.Windows.Forms.Label();
            this.lblLivingPersonaSleaze = new System.Windows.Forms.Label();
            this.lblLivingPersonaSleazeLabel = new System.Windows.Forms.Label();
            this.lblLivingPersonaAttack = new System.Windows.Forms.Label();
            this.lblLivingPersonaAttackLabel = new System.Windows.Forms.Label();
            this.lblFV = new System.Windows.Forms.Label();
            this.lblFVLabel = new System.Windows.Forms.Label();
            this.lblDuration = new System.Windows.Forms.Label();
            this.lblDurationLabel = new System.Windows.Forms.Label();
            this.lblTarget = new System.Windows.Forms.Label();
            this.lblTargetLabel = new System.Windows.Forms.Label();
            this.lblLivingPersonaLabel = new System.Windows.Forms.Label();
            this.lblLivingPersonaDeviceRating = new System.Windows.Forms.Label();
            this.lblLivingPersonaDeviceRatingLabel = new System.Windows.Forms.Label();
            this.lblFadingAttributesValue = new System.Windows.Forms.Label();
            this.lblFadingAttributes = new System.Windows.Forms.Label();
            this.lblFadingAttributesLabel = new System.Windows.Forms.Label();
            this.lblComplexFormSource = new System.Windows.Forms.Label();
            this.lblComplexFormSourceLabel = new System.Windows.Forms.Label();
            this.treComplexForms = new Chummer.helpers.TreeView();
            this.cmdDeleteComplexForm = new System.Windows.Forms.Button();
            this.cmdAddSprite = new System.Windows.Forms.Button();
            this.panSprites = new System.Windows.Forms.Panel();
            this.cmdAddComplexForm = new SplitButton();
            this.tabAdvancedPrograms = new System.Windows.Forms.TabPage();
            this.cmdAddAIProgram = new System.Windows.Forms.Button();
            this.lblAIProgramsRequires = new System.Windows.Forms.Label();
            this.lblAIProgramsRequiresLabel = new System.Windows.Forms.Label();
            this.lblAIProgramsSource = new System.Windows.Forms.Label();
            this.lblAIProgramsSourceLabel = new System.Windows.Forms.Label();
            this.treAIPrograms = new Chummer.helpers.TreeView();
            this.cmdDeleteAIProgram = new System.Windows.Forms.Button();
            this.tabCritter = new System.Windows.Forms.TabPage();
            this.chkCritterPowerCount = new System.Windows.Forms.CheckBox();
            this.lblCritterPowerPointCost = new System.Windows.Forms.Label();
            this.lblCritterPowerPointCostLabel = new System.Windows.Forms.Label();
            this.lblCritterPowerPoints = new System.Windows.Forms.Label();
            this.lblCritterPowerPointsLabel = new System.Windows.Forms.Label();
            this.cmdDeleteCritterPower = new System.Windows.Forms.Button();
            this.cmdAddCritterPower = new System.Windows.Forms.Button();
            this.lblCritterPowerSource = new System.Windows.Forms.Label();
            this.lblCritterPowerSourceLabel = new System.Windows.Forms.Label();
            this.lblCritterPowerDuration = new System.Windows.Forms.Label();
            this.lblCritterPowerDurationLabel = new System.Windows.Forms.Label();
            this.lblCritterPowerRange = new System.Windows.Forms.Label();
            this.lblCritterPowerRangeLabel = new System.Windows.Forms.Label();
            this.lblCritterPowerAction = new System.Windows.Forms.Label();
            this.lblCritterPowerActionLabel = new System.Windows.Forms.Label();
            this.lblCritterPowerType = new System.Windows.Forms.Label();
            this.lblCritterPowerTypeLabel = new System.Windows.Forms.Label();
            this.lblCritterPowerCategory = new System.Windows.Forms.Label();
            this.lblCritterPowerCategoryLabel = new System.Windows.Forms.Label();
            this.lblCritterPowerName = new System.Windows.Forms.Label();
            this.lblCritterPowerNameLabel = new System.Windows.Forms.Label();
            this.treCritterPowers = new Chummer.helpers.TreeView();
            this.tabInitiation = new System.Windows.Forms.TabPage();
            this.chkInitiationSchooling = new System.Windows.Forms.CheckBox();
            this.lblMetamagicSource = new System.Windows.Forms.Label();
            this.lblMetamagicSourceLabel = new System.Windows.Forms.Label();
            this.chkInitiationOrdeal = new System.Windows.Forms.CheckBox();
            this.chkInitiationGroup = new System.Windows.Forms.CheckBox();
            this.treMetamagic = new Chummer.helpers.TreeView();
            this.cmdAddMetamagic = new System.Windows.Forms.Button();
            this.tabCyberware = new System.Windows.Forms.TabPage();
            this.chkPrototypeTranshuman = new System.Windows.Forms.CheckBox();
            this.lblCyberlimbSTR = new System.Windows.Forms.Label();
            this.lblCyberlimbAGI = new System.Windows.Forms.Label();
            this.lblCyberlimbSTRLabel = new System.Windows.Forms.Label();
            this.lblCyberlimbAGILabel = new System.Windows.Forms.Label();
            this.lblCyberFirewall = new System.Windows.Forms.Label();
            this.lblCyberFirewallLabel = new System.Windows.Forms.Label();
            this.lblCyberDataProcessing = new System.Windows.Forms.Label();
            this.lblCyberDataProcessingLabel = new System.Windows.Forms.Label();
            this.lblCyberSleaze = new System.Windows.Forms.Label();
            this.lblCyberSleazeLabel = new System.Windows.Forms.Label();
            this.lblCyberAttack = new System.Windows.Forms.Label();
            this.lblCyberAttackLabel = new System.Windows.Forms.Label();
            this.lblCyberDeviceRating = new System.Windows.Forms.Label();
            this.lblCyberDeviceRatingLabel = new System.Windows.Forms.Label();
            this.lblEssenceHoleESS = new System.Windows.Forms.Label();
            this.lblEssenceHoleESSLabel = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.lblBiowareESS = new System.Windows.Forms.Label();
            this.lblCyberwareESS = new System.Windows.Forms.Label();
            this.lblBiowareESSLabel = new System.Windows.Forms.Label();
            this.lblCyberwareESSLabel = new System.Windows.Forms.Label();
            this.lblCyberwareSource = new System.Windows.Forms.Label();
            this.lblCyberwareSourceLabel = new System.Windows.Forms.Label();
            this.cmdAddBioware = new System.Windows.Forms.Button();
            this.cmdDeleteCyberware = new System.Windows.Forms.Button();
            this.nudCyberwareRating = new System.Windows.Forms.NumericUpDown();
            this.cboCyberwareGrade = new System.Windows.Forms.ComboBox();
            this.lblCyberwareRatingLabel = new System.Windows.Forms.Label();
            this.lblCyberwareCost = new System.Windows.Forms.Label();
            this.lblCyberwareCostLabel = new System.Windows.Forms.Label();
            this.lblCyberwareAvail = new System.Windows.Forms.Label();
            this.lblCyberwareAvailLabel = new System.Windows.Forms.Label();
            this.lblCyberwareGradeLabel = new System.Windows.Forms.Label();
            this.lblCyberwareCapacity = new System.Windows.Forms.Label();
            this.lblCyberwareCapacityLabel = new System.Windows.Forms.Label();
            this.lblCyberwareEssence = new System.Windows.Forms.Label();
            this.lblCyberwareEssenceLabel = new System.Windows.Forms.Label();
            this.lblCyberwareCategory = new System.Windows.Forms.Label();
            this.lblCyberwareCategoryLabel = new System.Windows.Forms.Label();
            this.lblCyberwareName = new System.Windows.Forms.Label();
            this.lblCyberwareNameLabel = new System.Windows.Forms.Label();
            this.treCyberware = new Chummer.helpers.TreeView();
            this.cmdAddCyberware = new SplitButton();
            this.tabStreetGear = new System.Windows.Forms.TabPage();
            this.tabStreetGearTabs = new System.Windows.Forms.TabControl();
            this.tabLifestyle = new System.Windows.Forms.TabPage();
            this.cmdAddLifestyle = new SplitButton();
            this.lblLifestyleQualities = new System.Windows.Forms.Label();
            this.lblLifestyleQualitiesLabel = new System.Windows.Forms.Label();
            this.lblBaseLifestyle = new System.Windows.Forms.Label();
            this.lblLifestyleComfortsLabel = new System.Windows.Forms.Label();
            this.lblLifestyleStartingNuyenLabel = new System.Windows.Forms.Label();
            this.lblLifestyleStartingNuyen = new System.Windows.Forms.Label();
            this.lblLifestyleSource = new System.Windows.Forms.Label();
            this.lblLifestyleSourceLabel = new System.Windows.Forms.Label();
            this.lblLifestyleTotalCost = new System.Windows.Forms.Label();
            this.lblLifestyleCostLabel = new System.Windows.Forms.Label();
            this.treLifestyles = new Chummer.helpers.TreeView();
            this.lblLifestyleCost = new System.Windows.Forms.Label();
            this.cmdDeleteLifestyle = new System.Windows.Forms.Button();
            this.lblLifestyleMonthsLabel = new System.Windows.Forms.Label();
            this.nudLifestyleMonths = new System.Windows.Forms.NumericUpDown();
            this.tabArmor = new System.Windows.Forms.TabPage();
            this.lblArmorFirewall = new System.Windows.Forms.Label();
            this.lblArmorFirewallLabel = new System.Windows.Forms.Label();
            this.lblArmorDataProcessing = new System.Windows.Forms.Label();
            this.lblArmorDataProcessingLabel = new System.Windows.Forms.Label();
            this.lblArmorSleaze = new System.Windows.Forms.Label();
            this.lblArmorSleazeLabel = new System.Windows.Forms.Label();
            this.lblArmorAttack = new System.Windows.Forms.Label();
            this.lblArmorAttackLabel = new System.Windows.Forms.Label();
            this.lblArmorDeviceRating = new System.Windows.Forms.Label();
            this.lblArmorDeviceRatingLabel = new System.Windows.Forms.Label();
            this.lblArmorValueLabel = new System.Windows.Forms.Label();
            this.lblArmorValue = new System.Windows.Forms.Label();
            this.chkIncludedInArmor = new System.Windows.Forms.CheckBox();
            this.lblArmorEquipped = new System.Windows.Forms.Label();
            this.lblArmorEquippedLabel = new System.Windows.Forms.Label();
            this.cmdArmorUnEquipAll = new System.Windows.Forms.Button();
            this.cmdArmorEquipAll = new System.Windows.Forms.Button();
            this.cmdAddArmorBundle = new System.Windows.Forms.Button();
            this.lblArmorCapacity = new System.Windows.Forms.Label();
            this.lblArmorCapacityLabel = new System.Windows.Forms.Label();
            this.nudArmorRating = new System.Windows.Forms.NumericUpDown();
            this.lblArmorRatingLabel = new System.Windows.Forms.Label();
            this.lblArmorSource = new System.Windows.Forms.Label();
            this.lblArmorSourceLabel = new System.Windows.Forms.Label();
            this.lblArmorCost = new System.Windows.Forms.Label();
            this.lblArmorCostLabel = new System.Windows.Forms.Label();
            this.cmdDeleteArmor = new System.Windows.Forms.Button();
            this.lblArmorAvail = new System.Windows.Forms.Label();
            this.treArmor = new Chummer.helpers.TreeView();
            this.lblArmorAvailLabel = new System.Windows.Forms.Label();
            this.cmdAddArmor = new SplitButton();
            this.tabWeapons = new System.Windows.Forms.TabPage();
            this.lblWeaponRating = new System.Windows.Forms.Label();
            this.lblWeaponRatingLabel = new System.Windows.Forms.Label();
            this.lblWeaponFirewall = new System.Windows.Forms.Label();
            this.lblWeaponFirewallLabel = new System.Windows.Forms.Label();
            this.lblWeaponDataProcessing = new System.Windows.Forms.Label();
            this.lblWeaponDataProcessingLabel = new System.Windows.Forms.Label();
            this.lblWeaponSleaze = new System.Windows.Forms.Label();
            this.lblWeaponSleazeLabel = new System.Windows.Forms.Label();
            this.lblWeaponAttack = new System.Windows.Forms.Label();
            this.lblWeaponAttackLabel = new System.Windows.Forms.Label();
            this.lblWeaponDeviceRating = new System.Windows.Forms.Label();
            this.lblWeaponDeviceRatingLabel = new System.Windows.Forms.Label();
            this.lblWeaponAccuracyLabel = new System.Windows.Forms.Label();
            this.lblWeaponAccuracy = new System.Windows.Forms.Label();
            this.cmdAddWeaponLocation = new System.Windows.Forms.Button();
            this.lblWeaponDicePool = new System.Windows.Forms.Label();
            this.lblWeaponDicePoolLabel = new System.Windows.Forms.Label();
            this.lblWeaponConceal = new System.Windows.Forms.Label();
            this.lblWeaponConcealLabel = new System.Windows.Forms.Label();
            this.lblWeaponRangeExtreme = new System.Windows.Forms.Label();
            this.lblWeaponRangeLong = new System.Windows.Forms.Label();
            this.lblWeaponRangeMedium = new System.Windows.Forms.Label();
            this.lblWeaponRangeShort = new System.Windows.Forms.Label();
            this.lblWeaponRangeExtremeLabel = new System.Windows.Forms.Label();
            this.lblWeaponRangeLongLabel = new System.Windows.Forms.Label();
            this.lblWeaponRangeMediumLabel = new System.Windows.Forms.Label();
            this.lblWeaponRangeShortLabel = new System.Windows.Forms.Label();
            this.lblWeaponRangeLabel = new System.Windows.Forms.Label();
            this.chkIncludedInWeapon = new System.Windows.Forms.CheckBox();
            this.lblWeaponSlots = new System.Windows.Forms.Label();
            this.lblWeaponSlotsLabel = new System.Windows.Forms.Label();
            this.lblWeaponSource = new System.Windows.Forms.Label();
            this.lblWeaponSourceLabel = new System.Windows.Forms.Label();
            this.lblWeaponAmmo = new System.Windows.Forms.Label();
            this.lblWeaponAmmoLabel = new System.Windows.Forms.Label();
            this.treWeapons = new Chummer.helpers.TreeView();
            this.lblWeaponMode = new System.Windows.Forms.Label();
            this.cmdDeleteWeapon = new System.Windows.Forms.Button();
            this.lblWeaponModeLabel = new System.Windows.Forms.Label();
            this.lblWeaponNameLabel = new System.Windows.Forms.Label();
            this.lblWeaponReach = new System.Windows.Forms.Label();
            this.lblWeaponName = new System.Windows.Forms.Label();
            this.lblWeaponReachLabel = new System.Windows.Forms.Label();
            this.lblWeaponCategoryLabel = new System.Windows.Forms.Label();
            this.lblWeaponAP = new System.Windows.Forms.Label();
            this.lblWeaponCategory = new System.Windows.Forms.Label();
            this.lblWeaponAPLabel = new System.Windows.Forms.Label();
            this.lblWeaponDamageLabel = new System.Windows.Forms.Label();
            this.lblWeaponCost = new System.Windows.Forms.Label();
            this.lblWeaponDamage = new System.Windows.Forms.Label();
            this.lblWeaponCostLabel = new System.Windows.Forms.Label();
            this.lblWeaponRCLabel = new System.Windows.Forms.Label();
            this.lblWeaponAvail = new System.Windows.Forms.Label();
            this.lblWeaponRC = new System.Windows.Forms.Label();
            this.lblWeaponAvailLabel = new System.Windows.Forms.Label();
            this.cmdAddWeapon = new SplitButton();
            this.tabGear = new System.Windows.Forms.TabPage();
            this.cboGearDataProcessing = new System.Windows.Forms.ComboBox();
            this.cboGearFirewall = new System.Windows.Forms.ComboBox();
            this.cboGearSleaze = new System.Windows.Forms.ComboBox();
            this.cboGearAttack = new System.Windows.Forms.ComboBox();
            this.lblGearFirewallLabel = new System.Windows.Forms.Label();
            this.lblGearDataProcessingLabel = new System.Windows.Forms.Label();
            this.lblGearSleazeLabel = new System.Windows.Forms.Label();
            this.lblGearAttackLabel = new System.Windows.Forms.Label();
            this.nudAdeptWayDiscount = new System.Windows.Forms.NumericUpDown();
            this.lblAdeptWayDiscount = new System.Windows.Forms.Label();
            this.chkActiveCommlink = new System.Windows.Forms.CheckBox();
            this.chkCommlinks = new System.Windows.Forms.CheckBox();
            this.cmdCreateStackedFocus = new System.Windows.Forms.Button();
            this.chkGearHomeNode = new System.Windows.Forms.CheckBox();
            this.lblGearAP = new System.Windows.Forms.Label();
            this.lblGearAPLabel = new System.Windows.Forms.Label();
            this.lblGearDamage = new System.Windows.Forms.Label();
            this.lblGearDamageLabel = new System.Windows.Forms.Label();
            this.cmdAddLocation = new System.Windows.Forms.Button();
            this.chkGearEquipped = new System.Windows.Forms.CheckBox();
            this.treFoci = new System.Windows.Forms.TreeView();
            this.lblGearSource = new System.Windows.Forms.Label();
            this.lblGearSourceLabel = new System.Windows.Forms.Label();
            this.lblGearDeviceRating = new System.Windows.Forms.Label();
            this.lblGearDeviceRatingLabel = new System.Windows.Forms.Label();
            this.nudGearQty = new System.Windows.Forms.NumericUpDown();
            this.lblGearQtyLabel = new System.Windows.Forms.Label();
            this.lblGearCost = new System.Windows.Forms.Label();
            this.lblGearCostLabel = new System.Windows.Forms.Label();
            this.lblGearAvail = new System.Windows.Forms.Label();
            this.lblGearAvailLabel = new System.Windows.Forms.Label();
            this.lblGearCapacity = new System.Windows.Forms.Label();
            this.lblGearCapacityLabel = new System.Windows.Forms.Label();
            this.lblGearCategory = new System.Windows.Forms.Label();
            this.lblGearCategoryLabel = new System.Windows.Forms.Label();
            this.lblGearName = new System.Windows.Forms.Label();
            this.lblGearNameLabel = new System.Windows.Forms.Label();
            this.nudGearRating = new System.Windows.Forms.NumericUpDown();
            this.lblGearRatingLabel = new System.Windows.Forms.Label();
            this.treGear = new Chummer.helpers.TreeView();
            this.cmdDeleteGear = new System.Windows.Forms.Button();
            this.cmdAddGear = new SplitButton();
            this.tabPets = new System.Windows.Forms.TabPage();
            this.panPets = new System.Windows.Forms.FlowLayoutPanel();
            this.cmdAddPet = new System.Windows.Forms.Button();
            this.tabVehicles = new System.Windows.Forms.TabPage();
            this.lblVehicleSeats = new System.Windows.Forms.Label();
            this.lblVehicleSeatsLabel = new System.Windows.Forms.Label();
            this.lblVehicleDroneModSlots = new System.Windows.Forms.Label();
            this.lblVehicleDroneModSlotsLabel = new System.Windows.Forms.Label();
            this.lblVehicleCosmetic = new System.Windows.Forms.Label();
            this.lblVehicleElectromagnetic = new System.Windows.Forms.Label();
            this.lblVehicleBodymod = new System.Windows.Forms.Label();
            this.lblVehicleWeaponsmod = new System.Windows.Forms.Label();
            this.lblVehicleProtection = new System.Windows.Forms.Label();
            this.lblVehiclePowertrain = new System.Windows.Forms.Label();
            this.lblVehicleCosmeticLabel = new System.Windows.Forms.Label();
            this.lblVehicleElectromagneticLabel = new System.Windows.Forms.Label();
            this.lblVehicleBodymodLabel = new System.Windows.Forms.Label();
            this.lblVehicleWeaponsmodLabel = new System.Windows.Forms.Label();
            this.lblVehicleProtectionLabel = new System.Windows.Forms.Label();
            this.lblVehiclePowertrainLabel = new System.Windows.Forms.Label();
            this.cboVehicleGearDataProcessing = new System.Windows.Forms.ComboBox();
            this.cboVehicleGearFirewall = new System.Windows.Forms.ComboBox();
            this.cboVehicleGearSleaze = new System.Windows.Forms.ComboBox();
            this.cboVehicleGearAttack = new System.Windows.Forms.ComboBox();
            this.lblVehicleFirewallLabel = new System.Windows.Forms.Label();
            this.lblVehicleDataProcessingLabel = new System.Windows.Forms.Label();
            this.lblVehicleSleazeLabel = new System.Windows.Forms.Label();
            this.lblVehicleAttackLabel = new System.Windows.Forms.Label();
            this.lblVehicleWeaponRangeExtreme = new System.Windows.Forms.Label();
            this.lblVehicleWeaponRangeLong = new System.Windows.Forms.Label();
            this.lblVehicleWeaponRangeMedium = new System.Windows.Forms.Label();
            this.lblVehicleWeaponRangeShort = new System.Windows.Forms.Label();
            this.lblVehicleWeaponRangeExtremeLabel = new System.Windows.Forms.Label();
            this.lblVehicleWeaponRangeLongLabel = new System.Windows.Forms.Label();
            this.lblVehicleWeaponRangeMediumLabel = new System.Windows.Forms.Label();
            this.lblVehicleWeaponRangeShortLabel = new System.Windows.Forms.Label();
            this.lblVehicleWeaponRangeLabel = new System.Windows.Forms.Label();
            this.lblVehicleWeaponAmmo = new System.Windows.Forms.Label();
            this.lblVehicleWeaponAmmoLabel = new System.Windows.Forms.Label();
            this.lblVehicleWeaponMode = new System.Windows.Forms.Label();
            this.lblVehicleWeaponModeLabel = new System.Windows.Forms.Label();
            this.lblVehicleWeaponNameLabel = new System.Windows.Forms.Label();
            this.lblVehicleWeaponName = new System.Windows.Forms.Label();
            this.lblVehicleWeaponCategoryLabel = new System.Windows.Forms.Label();
            this.lblVehicleWeaponAP = new System.Windows.Forms.Label();
            this.lblVehicleWeaponCategory = new System.Windows.Forms.Label();
            this.lblVehicleWeaponAPLabel = new System.Windows.Forms.Label();
            this.lblVehicleWeaponDamageLabel = new System.Windows.Forms.Label();
            this.lblVehicleWeaponDamage = new System.Windows.Forms.Label();
            this.cmdAddVehicleLocation = new System.Windows.Forms.Button();
            this.chkVehicleHomeNode = new System.Windows.Forms.CheckBox();
            this.lblVehicleDevice = new System.Windows.Forms.Label();
            this.lblVehicleDeviceLabel = new System.Windows.Forms.Label();
            this.nudVehicleGearQty = new System.Windows.Forms.NumericUpDown();
            this.lblVehicleGearQtyLabel = new System.Windows.Forms.Label();
            this.chkVehicleIncludedInWeapon = new System.Windows.Forms.CheckBox();
            this.lblVehicleSource = new System.Windows.Forms.Label();
            this.lblVehicleSourceLabel = new System.Windows.Forms.Label();
            this.lblVehicleSlots = new System.Windows.Forms.Label();
            this.lblVehicleSlotsLabel = new System.Windows.Forms.Label();
            this.nudVehicleRating = new System.Windows.Forms.NumericUpDown();
            this.lblVehicleRatingLabel = new System.Windows.Forms.Label();
            this.lblVehicleNameLabel = new System.Windows.Forms.Label();
            this.lblVehicleName = new System.Windows.Forms.Label();
            this.lblVehicleCategoryLabel = new System.Windows.Forms.Label();
            this.lblVehicleCategory = new System.Windows.Forms.Label();
            this.lblVehicleSensor = new System.Windows.Forms.Label();
            this.lblVehicleSensorLabel = new System.Windows.Forms.Label();
            this.lblVehiclePilot = new System.Windows.Forms.Label();
            this.lblVehiclePilotLabel = new System.Windows.Forms.Label();
            this.lblVehicleArmor = new System.Windows.Forms.Label();
            this.lblVehicleArmorLabel = new System.Windows.Forms.Label();
            this.lblVehicleBody = new System.Windows.Forms.Label();
            this.lblVehicleBodyLabel = new System.Windows.Forms.Label();
            this.lblVehicleSpeed = new System.Windows.Forms.Label();
            this.lblVehicleSpeedLabel = new System.Windows.Forms.Label();
            this.lblVehicleCost = new System.Windows.Forms.Label();
            this.lblVehicleCostLabel = new System.Windows.Forms.Label();
            this.lblVehicleAvail = new System.Windows.Forms.Label();
            this.lblVehicleAvailLabel = new System.Windows.Forms.Label();
            this.lblVehicleAccel = new System.Windows.Forms.Label();
            this.lblVehicleAccelLabel = new System.Windows.Forms.Label();
            this.lblVehicleHandling = new System.Windows.Forms.Label();
            this.lblVehicleHandlingLabel = new System.Windows.Forms.Label();
            this.cmdDeleteVehicle = new System.Windows.Forms.Button();
            this.treVehicles = new Chummer.helpers.TreeView();
            this.cmdAddVehicle = new SplitButton();
            this.tabCharacterInfo = new System.Windows.Forms.TabPage();
            this.cboPrimaryArm = new Chummer.helpers.ComboBox();
            this.lblHandedness = new System.Windows.Forms.Label();
            this.chkIsMainMugshot = new System.Windows.Forms.CheckBox();
            this.lblNumMugshots = new System.Windows.Forms.Label();
            this.nudMugshotIndex = new System.Windows.Forms.NumericUpDown();
            this.lblMugshotDimensions = new System.Windows.Forms.Label();
            this.btnCreateBackstory = new System.Windows.Forms.Button();
            this.lblPublicAwareTotal = new System.Windows.Forms.Label();
            this.lblNotorietyTotal = new System.Windows.Forms.Label();
            this.lblStreetCredTotal = new System.Windows.Forms.Label();
            this.lblCharacterName = new System.Windows.Forms.Label();
            this.txtCharacterName = new System.Windows.Forms.TextBox();
            this.txtPlayerName = new System.Windows.Forms.TextBox();
            this.lblPlayerName = new System.Windows.Forms.Label();
            this.txtNotes = new System.Windows.Forms.TextBox();
            this.lblNotes = new System.Windows.Forms.Label();
            this.cmdDeleteMugshot = new System.Windows.Forms.Button();
            this.cmdAddMugshot = new System.Windows.Forms.Button();
            this.lblMugshot = new System.Windows.Forms.Label();
            this.txtConcept = new System.Windows.Forms.TextBox();
            this.lblConcept = new System.Windows.Forms.Label();
            this.txtBackground = new System.Windows.Forms.TextBox();
            this.lblBackground = new System.Windows.Forms.Label();
            this.txtDescription = new System.Windows.Forms.TextBox();
            this.lblDescription = new System.Windows.Forms.Label();
            this.txtSkin = new System.Windows.Forms.TextBox();
            this.lblSkin = new System.Windows.Forms.Label();
            this.txtWeight = new System.Windows.Forms.TextBox();
            this.lblWeight = new System.Windows.Forms.Label();
            this.txtHeight = new System.Windows.Forms.TextBox();
            this.lblHeight = new System.Windows.Forms.Label();
            this.txtHair = new System.Windows.Forms.TextBox();
            this.lblHair = new System.Windows.Forms.Label();
            this.txtEyes = new System.Windows.Forms.TextBox();
            this.lblEyes = new System.Windows.Forms.Label();
            this.txtAge = new System.Windows.Forms.TextBox();
            this.lblAge = new System.Windows.Forms.Label();
            this.txtSex = new System.Windows.Forms.TextBox();
            this.lblSex = new System.Windows.Forms.Label();
            this.picMugshot = new System.Windows.Forms.PictureBox();
            this.tabImprovements = new System.Windows.Forms.TabPage();
            this.panImprovements = new System.Windows.Forms.FlowLayoutPanel();
            this.tabInfo = new System.Windows.Forms.TabControl();
            this.tabBPSummary = new System.Windows.Forms.TabPage();
            this.lblPBuildSpecial = new System.Windows.Forms.Label();
            this.lblPBuildSpecialLabel = new System.Windows.Forms.Label();
            this.lblBuildRitualsBP = new System.Windows.Forms.Label();
            this.lblBuildPrepsBP = new System.Windows.Forms.Label();
            this.lblAINormalProgramsBP = new System.Windows.Forms.Label();
            this.lblBuildAINormalPrograms = new System.Windows.Forms.Label();
            this.lblAIAdvancedProgramsBP = new System.Windows.Forms.Label();
            this.lblInitiationBP = new System.Windows.Forms.Label();
            this.lblBuildInitiation = new System.Windows.Forms.Label();
            this.lblFociBP = new System.Windows.Forms.Label();
            this.lblManeuversBP = new System.Windows.Forms.Label();
            this.lblNuyenBP = new System.Windows.Forms.Label();
            this.lblEnemiesBP = new System.Windows.Forms.Label();
            this.lblKarmaMetatypeBP = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.lblComplexFormsBP = new System.Windows.Forms.Label();
            this.lblSpiritsBP = new System.Windows.Forms.Label();
            this.lblSpritesBP = new System.Windows.Forms.Label();
            this.lblKnowledgeSkillsBP = new System.Windows.Forms.Label();
            this.lblActiveSkillsBP = new System.Windows.Forms.Label();
            this.lblSpellsBP = new System.Windows.Forms.Label();
            this.lblSkillGroupsBP = new System.Windows.Forms.Label();
            this.lblPositiveQualitiesBP = new System.Windows.Forms.Label();
            this.lblNegativeQualitiesBP = new System.Windows.Forms.Label();
            this.lblAttributesBP = new System.Windows.Forms.Label();
            this.lblContactsBP = new System.Windows.Forms.Label();
            this.tabOtherInfo = new System.Windows.Forms.TabPage();
            this.lblRiggingINI = new System.Windows.Forms.Label();
            this.lblMatrixINIHot = new System.Windows.Forms.Label();
            this.lblMatrixINICold = new System.Windows.Forms.Label();
            this.lblFly = new System.Windows.Forms.Label();
            this.lblFlyLabel = new System.Windows.Forms.Label();
            this.lblSwim = new System.Windows.Forms.Label();
            this.lblSwimLabel = new System.Windows.Forms.Label();
            this.lblMemory = new System.Windows.Forms.Label();
            this.lblLiftCarry = new System.Windows.Forms.Label();
            this.lblJudgeIntentions = new System.Windows.Forms.Label();
            this.lblComposure = new System.Windows.Forms.Label();
            this.lblMovement = new System.Windows.Forms.Label();
            this.lblMovementLabel = new System.Windows.Forms.Label();
            this.lblRemainingNuyen = new System.Windows.Forms.Label();
            this.lblESSMax = new System.Windows.Forms.Label();
            this.lblArmor = new System.Windows.Forms.Label();
            this.lblAstralINI = new System.Windows.Forms.Label();
            this.lblMatrixINI = new System.Windows.Forms.Label();
            this.lblINI = new System.Windows.Forms.Label();
            this.lblCMStun = new System.Windows.Forms.Label();
            this.lblCMPhysical = new System.Windows.Forms.Label();
            this.tabDefences = new System.Windows.Forms.TabPage();
            this.nudCounterspellingDice = new System.Windows.Forms.NumericUpDown();
            this.lbllSpellDefenceManipPhysical = new System.Windows.Forms.Label();
            this.lblSpellDefenceManipMental = new System.Windows.Forms.Label();
            this.lblSpellDefenceIllusionPhysical = new System.Windows.Forms.Label();
            this.lblSpellDefenceIllusionMana = new System.Windows.Forms.Label();
            this.lblSpellDefenceDecAttWIL = new System.Windows.Forms.Label();
            this.lblSpellDefenceDecAttLOG = new System.Windows.Forms.Label();
            this.lblSpellDefenceDecAttINT = new System.Windows.Forms.Label();
            this.lblSpellDefenceDecAttCHA = new System.Windows.Forms.Label();
            this.lblSpellDefenceDecAttSTR = new System.Windows.Forms.Label();
            this.lblSpellDefenceDecAttREA = new System.Windows.Forms.Label();
            this.lblSpellDefenceDecAttAGI = new System.Windows.Forms.Label();
            this.lblSpellDefenceDecAttBOD = new System.Windows.Forms.Label();
            this.lblSpellDefenceDetection = new System.Windows.Forms.Label();
            this.lblSpellDefenceDirectSoakPhysical = new System.Windows.Forms.Label();
            this.lblSpellDefenceDirectSoakMana = new System.Windows.Forms.Label();
            this.lblSpellDefenceIndirectSoak = new System.Windows.Forms.Label();
            this.lblSpellDefenceIndirectDodge = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.cmsLimitModifier = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tssLimitModifierNotes = new System.Windows.Forms.ToolStripMenuItem();
            this.tssLimitModifierEdit = new System.Windows.Forms.ToolStripMenuItem();
            this.cmsCustomLimitModifier = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.cmsInitiationNotes = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsInitiationNotes = new System.Windows.Forms.ToolStripMenuItem();
            this.cmsMetamagic = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsMetamagicAddArt = new System.Windows.Forms.ToolStripMenuItem();
            this.tsMetamagicAddEnchantment = new System.Windows.Forms.ToolStripMenuItem();
            this.tsMetamagicAddEnhancement = new System.Windows.Forms.ToolStripMenuItem();
            this.tsMetamagicAddMetamagic = new System.Windows.Forms.ToolStripMenuItem();
            this.tsMetamagicAddRitual = new System.Windows.Forms.ToolStripMenuItem();
            this.tsMetamagicNotes = new System.Windows.Forms.ToolStripMenuItem();
            this.cmsTechnique = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsAddTechniqueNotes = new System.Windows.Forms.ToolStripMenuItem();
            this.cmsAdvancedProgram = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsAddAdvancedProgramOption = new System.Windows.Forms.ToolStripMenuItem();
            this.tsAIProgramNotes = new System.Windows.Forms.ToolStripMenuItem();
            this.StatusStrip.SuspendLayout();
            this.cmsMartialArts.SuspendLayout();
            this.cmsSpellButton.SuspendLayout();
            this.cmsComplexForm.SuspendLayout();
            this.cmsCyberware.SuspendLayout();
            this.cmsLifestyle.SuspendLayout();
            this.cmsArmor.SuspendLayout();
            this.cmsWeapon.SuspendLayout();
            this.cmsGearButton.SuspendLayout();
            this.cmsVehicle.SuspendLayout();
            this.mnuCreateMenu.SuspendLayout();
            this.toolStrip.SuspendLayout();
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
            this.tabPeople.SuspendLayout();
            this.tabContacts.SuspendLayout();
            this.tabEnemies.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudMysticAdeptMAGMagician)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudNuyen)).BeginInit();
            this.tabSkills.SuspendLayout();
            this.tabLimits.SuspendLayout();
            this.tabMartialArts.SuspendLayout();
            this.tabMagician.SuspendLayout();
            this.tabAdept.SuspendLayout();
            this.tabTechnomancer.SuspendLayout();
            this.tabAdvancedPrograms.SuspendLayout();
            this.tabCritter.SuspendLayout();
            this.tabInitiation.SuspendLayout();
            this.tabCyberware.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudCyberwareRating)).BeginInit();
            this.tabStreetGear.SuspendLayout();
            this.tabStreetGearTabs.SuspendLayout();
            this.tabLifestyle.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudLifestyleMonths)).BeginInit();
            this.tabArmor.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudArmorRating)).BeginInit();
            this.tabWeapons.SuspendLayout();
            this.tabGear.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudAdeptWayDiscount)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudGearQty)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudGearRating)).BeginInit();
            this.tabPets.SuspendLayout();
            this.tabVehicles.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudVehicleGearQty)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudVehicleRating)).BeginInit();
            this.tabCharacterInfo.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudMugshotIndex)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picMugshot)).BeginInit();
            this.tabImprovements.SuspendLayout();
            this.tabInfo.SuspendLayout();
            this.tabBPSummary.SuspendLayout();
            this.tabOtherInfo.SuspendLayout();
            this.tabDefences.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudCounterspellingDice)).BeginInit();
            this.cmsLimitModifier.SuspendLayout();
            this.cmsInitiationNotes.SuspendLayout();
            this.cmsMetamagic.SuspendLayout();
            this.cmsTechnique.SuspendLayout();
            this.cmsAdvancedProgram.SuspendLayout();
            this.SuspendLayout();
            //
            // StatusStrip
            //
            this.StatusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tssBPLabel,
            this.tssBP,
            this.tssBPRemainLabel,
            this.tssBPRemain,
            this.toolStripStatusLabel3,
            this.tssEssence,
            this.toolStripStatusLabel4,
            this.tssNuyenRemaining,
            this.pgbProgress});
            this.StatusStrip.Location = new System.Drawing.Point(0, 640);
            this.StatusStrip.Name = "StatusStrip";
            this.StatusStrip.Size = new System.Drawing.Size(1040, 24);
            this.StatusStrip.TabIndex = 24;
            this.StatusStrip.Text = "StatusStrip1";
            //
            // tssBPLabel
            //
            this.tssBPLabel.Name = "tssBPLabel";
            this.tssBPLabel.Size = new System.Drawing.Size(44, 19);
            this.tssBPLabel.Tag = "Label_Karma";
            this.tssBPLabel.Text = "Karma:";
            //
            // tssBP
            //
            this.tssBP.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Right;
            this.tssBP.Name = "tssBP";
            this.tssBP.Size = new System.Drawing.Size(29, 19);
            this.tssBP.Text = "400";
            //
            // tssBPRemainLabel
            //
            this.tssBPRemainLabel.Name = "tssBPRemainLabel";
            this.tssBPRemainLabel.Size = new System.Drawing.Size(104, 19);
            this.tssBPRemainLabel.Tag = "Label_KarmaRemaining";
            this.tssBPRemainLabel.Text = "Karma Remaining:";
            //
            // tssBPRemain
            //
            this.tssBPRemain.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Right;
            this.tssBPRemain.Name = "tssBPRemain";
            this.tssBPRemain.Size = new System.Drawing.Size(29, 19);
            this.tssBPRemain.Text = "400";
            //
            // toolStripStatusLabel3
            //
            this.toolStripStatusLabel3.Name = "toolStripStatusLabel3";
            this.toolStripStatusLabel3.Size = new System.Drawing.Size(51, 19);
            this.toolStripStatusLabel3.Tag = "Label_OtherEssence";
            this.toolStripStatusLabel3.Text = "Essence:";
            //
            // tssEssence
            //
            this.tssEssence.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Right;
            this.tssEssence.Name = "tssEssence";
            this.tssEssence.Size = new System.Drawing.Size(32, 19);
            this.tssEssence.Text = "6.00";
            //
            // toolStripStatusLabel4
            //
            this.toolStripStatusLabel4.Name = "toolStripStatusLabel4";
            this.toolStripStatusLabel4.Size = new System.Drawing.Size(105, 19);
            this.toolStripStatusLabel4.Tag = "Label_OtherNuyenRemain";
            this.toolStripStatusLabel4.Text = "Nuyen Remaining:";
            //
            // tssNuyenRemaining
            //
            this.tssNuyenRemaining.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Right;
            this.tssNuyenRemaining.Name = "tssNuyenRemaining";
            this.tssNuyenRemaining.Size = new System.Drawing.Size(23, 19);
            this.tssNuyenRemaining.Text = "0¥";
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
            // tipTooltip
            //
            this.tipTooltip.AllowLinksHandling = true;
            this.tipTooltip.AutoPopDelay = 10000;
            this.tipTooltip.BaseStylesheet = null;
            this.tipTooltip.InitialDelay = 250;
            this.tipTooltip.IsBalloon = true;
            this.tipTooltip.MaximumSize = new System.Drawing.Size(0, 0);
            this.tipTooltip.OwnerDraw = true;
            this.tipTooltip.ReshowDelay = 100;
            this.tipTooltip.TooltipCssClass = "htmltooltip";
            this.tipTooltip.ToolTipIcon = System.Windows.Forms.ToolTipIcon.Info;
            this.tipTooltip.ToolTipTitle = "Chummer Help";
            //
            // lblAttributesAug
            //
            this.lblAttributesAug.AutoSize = true;
            this.lblAttributesAug.Location = new System.Drawing.Point(508, 32);
            this.lblAttributesAug.Name = "lblAttributesAug";
            this.lblAttributesAug.Size = new System.Drawing.Size(50, 13);
            this.lblAttributesAug.TabIndex = 55;
            this.lblAttributesAug.Tag = "Label_ValAugmented";
            this.lblAttributesAug.Text = "Val (Aug)";
            this.tipTooltip.SetToolTip(this.lblAttributesAug, "Augmented Attribute value.");
            //
            // lblAttributesBase
            //
            this.lblAttributesBase.AutoSize = true;
            this.lblAttributesBase.Location = new System.Drawing.Point(413, 32);
            this.lblAttributesBase.Name = "lblAttributesBase";
            this.lblAttributesBase.Size = new System.Drawing.Size(31, 13);
            this.lblAttributesBase.TabIndex = 54;
            this.lblAttributesBase.Tag = "Label_Base";
            this.lblAttributesBase.Text = "Base";
            this.tipTooltip.SetToolTip(this.lblAttributesBase, "Base Attribute value.");
            //
            // lblAttributesMetatype
            //
            this.lblAttributesMetatype.AutoSize = true;
            this.lblAttributesMetatype.Location = new System.Drawing.Point(564, 32);
            this.lblAttributesMetatype.Name = "lblAttributesMetatype";
            this.lblAttributesMetatype.Size = new System.Drawing.Size(80, 13);
            this.lblAttributesMetatype.TabIndex = 53;
            this.lblAttributesMetatype.Tag = "Label_MetatypeLimits";
            this.lblAttributesMetatype.Text = "Metatype Limits";
            this.tipTooltip.SetToolTip(this.lblAttributesMetatype, "Metatype Minimum / Maximum (Augmented Maximum) values.");
            //
            // lblAttributes
            //
            this.lblAttributes.AutoSize = true;
            this.lblAttributes.Location = new System.Drawing.Point(285, 32);
            this.lblAttributes.Name = "lblAttributes";
            this.lblAttributes.Size = new System.Drawing.Size(51, 13);
            this.lblAttributes.TabIndex = 5;
            this.lblAttributes.Tag = "Label_Attributes";
            this.lblAttributes.Text = "Attributes";
            this.tipTooltip.SetToolTip(this.lblAttributes, "Characters cannot spend more than 50% of their BP on Primary Attributes (not incl" +
        "uding EDG, MAG, and RES).\r\nOnly one attribute may be at its Maximum value during" +
        " character creation.");
            //
            // lblNuyen
            //
            this.lblNuyen.AutoSize = true;
            this.lblNuyen.Location = new System.Drawing.Point(650, 65);
            this.lblNuyen.Name = "lblNuyen";
            this.lblNuyen.Size = new System.Drawing.Size(38, 13);
            this.lblNuyen.TabIndex = 16;
            this.lblNuyen.Tag = "Label_Nuyen";
            this.lblNuyen.Text = "Nuyen";
            this.tipTooltip.SetToolTip(this.lblNuyen, "The amount of Nuyen your character has to purchase equipment. 1 BP = 5,000¥.");
            //
            // lblSpirits
            //
            this.lblSpirits.AutoSize = true;
            this.lblSpirits.Location = new System.Drawing.Point(10, 404);
            this.lblSpirits.Name = "lblSpirits";
            this.lblSpirits.Size = new System.Drawing.Size(35, 13);
            this.lblSpirits.TabIndex = 0;
            this.lblSpirits.Tag = "Label_Spirits";
            this.lblSpirits.Text = "Spirits";
            this.tipTooltip.SetToolTip(this.lblSpirits, "Each Service a Spirit owes costs 1 BP.");
            //
            // lblComplexForms
            //
            this.lblComplexForms.AutoSize = true;
            this.lblComplexForms.Location = new System.Drawing.Point(8, 9);
            this.lblComplexForms.Name = "lblComplexForms";
            this.lblComplexForms.Size = new System.Drawing.Size(78, 13);
            this.lblComplexForms.TabIndex = 28;
            this.lblComplexForms.Tag = "Label_ComplexForms";
            this.lblComplexForms.Text = "Complex Forms";
            this.tipTooltip.SetToolTip(this.lblComplexForms, "Complex Forms cost a number of BP equal to their Rating.");
            //
            // lblSprites
            //
            this.lblSprites.AutoSize = true;
            this.lblSprites.Location = new System.Drawing.Point(8, 400);
            this.lblSprites.Name = "lblSprites";
            this.lblSprites.Size = new System.Drawing.Size(39, 13);
            this.lblSprites.TabIndex = 24;
            this.lblSprites.Tag = "Label_Sprites";
            this.lblSprites.Text = "Sprites";
            this.tipTooltip.SetToolTip(this.lblSprites, "Each Service a Sprite owes costs 1 BP.");
            //
            // chkArmorEquipped
            //
            this.chkArmorEquipped.AutoSize = true;
            this.chkArmorEquipped.Enabled = false;
            this.chkArmorEquipped.Location = new System.Drawing.Point(310, 157);
            this.chkArmorEquipped.Name = "chkArmorEquipped";
            this.chkArmorEquipped.Size = new System.Drawing.Size(71, 17);
            this.chkArmorEquipped.TabIndex = 78;
            this.chkArmorEquipped.Tag = "Checkbox_Equipped";
            this.chkArmorEquipped.Text = "Equipped";
            this.tipTooltip.SetToolTip(this.chkArmorEquipped, "Equipped Armor and Armor Mods are factored into Armor Encumbrance and a character" +
        "\'s highest Armor Ratings.");
            this.chkArmorEquipped.UseVisualStyleBackColor = true;
            this.chkArmorEquipped.CheckedChanged += new System.EventHandler(this.chkArmorEquipped_CheckedChanged);
            //
            // chkWeaponAccessoryInstalled
            //
            this.chkWeaponAccessoryInstalled.AutoSize = true;
            this.chkWeaponAccessoryInstalled.Enabled = false;
            this.chkWeaponAccessoryInstalled.Location = new System.Drawing.Point(586, 150);
            this.chkWeaponAccessoryInstalled.Name = "chkWeaponAccessoryInstalled";
            this.chkWeaponAccessoryInstalled.Size = new System.Drawing.Size(65, 17);
            this.chkWeaponAccessoryInstalled.TabIndex = 72;
            this.chkWeaponAccessoryInstalled.Tag = "Checkbox_Installed";
            this.chkWeaponAccessoryInstalled.Text = "Installed";
            this.tipTooltip.SetToolTip(this.chkWeaponAccessoryInstalled, "Installed Weapon Accessories and Mods count towards a Weapon\'s stats.");
            this.chkWeaponAccessoryInstalled.UseVisualStyleBackColor = true;
            this.chkWeaponAccessoryInstalled.CheckedChanged += new System.EventHandler(this.chkWeaponAccessoryInstalled_CheckedChanged);
            //
            // chkVehicleWeaponAccessoryInstalled
            //
            this.chkVehicleWeaponAccessoryInstalled.AutoSize = true;
            this.chkVehicleWeaponAccessoryInstalled.Enabled = false;
            this.chkVehicleWeaponAccessoryInstalled.Location = new System.Drawing.Point(727, 244);
            this.chkVehicleWeaponAccessoryInstalled.Name = "chkVehicleWeaponAccessoryInstalled";
            this.chkVehicleWeaponAccessoryInstalled.Size = new System.Drawing.Size(65, 17);
            this.chkVehicleWeaponAccessoryInstalled.TabIndex = 74;
            this.chkVehicleWeaponAccessoryInstalled.Tag = "Checkbox_Installed";
            this.chkVehicleWeaponAccessoryInstalled.Text = "Installed";
            this.tipTooltip.SetToolTip(this.chkVehicleWeaponAccessoryInstalled, "Installed Weapon Accessories and Mods count towards a Weapon\'s stats.");
            this.chkVehicleWeaponAccessoryInstalled.UseVisualStyleBackColor = true;
            this.chkVehicleWeaponAccessoryInstalled.CheckedChanged += new System.EventHandler(this.chkVehicleWeaponAccessoryInstalled_CheckedChanged);
            //
            // lblNotoriety
            //
            this.lblNotoriety.AutoSize = true;
            this.lblNotoriety.Location = new System.Drawing.Point(654, 116);
            this.lblNotoriety.Name = "lblNotoriety";
            this.lblNotoriety.Size = new System.Drawing.Size(52, 13);
            this.lblNotoriety.TabIndex = 84;
            this.lblNotoriety.Tag = "Label_Notoriety";
            this.lblNotoriety.Text = "Notoriety:";
            this.tipTooltip.SetToolTip(this.lblNotoriety, "Notoriety is typically gained through Qualities and can be further adjusted by Ga" +
        "me Masters. Notoriety can be reduced by 1 point by burning 2 points of Street Cr" +
        "ed.");
            //
            // lblStreetCred
            //
            this.lblStreetCred.AutoSize = true;
            this.lblStreetCred.Location = new System.Drawing.Point(654, 90);
            this.lblStreetCred.Name = "lblStreetCred";
            this.lblStreetCred.Size = new System.Drawing.Size(63, 13);
            this.lblStreetCred.TabIndex = 82;
            this.lblStreetCred.Tag = "Label_StreetCred";
            this.lblStreetCred.Text = "Street Cred:";
            this.tipTooltip.SetToolTip(this.lblStreetCred, "Street Cred is calculated as Career Karma ÷ 10, rounded up, and can be further ad" +
        "justed by Game Masters.");
            //
            // chkCharacterCreated
            //
            this.chkCharacterCreated.AutoSize = true;
            this.chkCharacterCreated.Location = new System.Drawing.Point(657, 8);
            this.chkCharacterCreated.Name = "chkCharacterCreated";
            this.chkCharacterCreated.Size = new System.Drawing.Size(152, 17);
            this.chkCharacterCreated.TabIndex = 62;
            this.chkCharacterCreated.Tag = "Checkbox_Created";
            this.chkCharacterCreated.Text = "Mark character as Created";
            this.tipTooltip.SetToolTip(this.chkCharacterCreated, "Once a character has been saved as Created it cannot be further modified using th" +
        "e Build system. The character is put into Career mode where it can be improved w" +
        "ith Karma and used for play.");
            this.chkCharacterCreated.UseVisualStyleBackColor = true;
            //
            // lblBuildFoci
            //
            this.lblBuildFoci.AutoSize = true;
            this.lblBuildFoci.Location = new System.Drawing.Point(10, 303);
            this.lblBuildFoci.Name = "lblBuildFoci";
            this.lblBuildFoci.Size = new System.Drawing.Size(27, 13);
            this.lblBuildFoci.TabIndex = 81;
            this.lblBuildFoci.Tag = "Label_SummaryFoci";
            this.lblBuildFoci.Text = "Foci";
            this.tipTooltip.SetToolTip(this.lblBuildFoci, "Each bonded Focus costs a number of BP equal to its Force.");
            //
            // lblBuildManeuvers
            //
            this.lblBuildManeuvers.AutoSize = true;
            this.lblBuildManeuvers.Location = new System.Drawing.Point(10, 408);
            this.lblBuildManeuvers.Name = "lblBuildManeuvers";
            this.lblBuildManeuvers.Size = new System.Drawing.Size(85, 13);
            this.lblBuildManeuvers.TabIndex = 79;
            this.lblBuildManeuvers.Tag = "Label_SummaryManeuvers";
            this.lblBuildManeuvers.Text = "M.A. Maneuvers";
            this.tipTooltip.SetToolTip(this.lblBuildManeuvers, "Each Martial Art Maneuver costs 2 BP.");
            //
            // lblBuildNuyen
            //
            this.lblBuildNuyen.AutoSize = true;
            this.lblBuildNuyen.Location = new System.Drawing.Point(10, 156);
            this.lblBuildNuyen.Name = "lblBuildNuyen";
            this.lblBuildNuyen.Size = new System.Drawing.Size(38, 13);
            this.lblBuildNuyen.TabIndex = 76;
            this.lblBuildNuyen.Tag = "Label_SummaryNuyen";
            this.lblBuildNuyen.Text = "Nuyen";
            this.tipTooltip.SetToolTip(this.lblBuildNuyen, "The amount of Nuyen your character has to purchase equipment. 1 BP = 5,000¥.");
            //
            // lblBuildEnemies
            //
            this.lblBuildEnemies.AutoSize = true;
            this.lblBuildEnemies.Location = new System.Drawing.Point(10, 135);
            this.lblBuildEnemies.Name = "lblBuildEnemies";
            this.lblBuildEnemies.Size = new System.Drawing.Size(47, 13);
            this.lblBuildEnemies.TabIndex = 75;
            this.lblBuildEnemies.Tag = "Label_SummaryEnemies";
            this.lblBuildEnemies.Text = "Enemies";
            this.tipTooltip.SetToolTip(this.lblBuildEnemies, "Enemies are the opposite of Contacts. Each Enemy counts towards your Negative Qua" +
        "lities BP amount.");
            //
            // lblBuildComplexForms
            //
            this.lblBuildComplexForms.AutoSize = true;
            this.lblBuildComplexForms.Location = new System.Drawing.Point(10, 366);
            this.lblBuildComplexForms.Name = "lblBuildComplexForms";
            this.lblBuildComplexForms.Size = new System.Drawing.Size(78, 13);
            this.lblBuildComplexForms.TabIndex = 72;
            this.lblBuildComplexForms.Tag = "Label_SummaryComplexForms";
            this.lblBuildComplexForms.Text = "Complex Forms";
            this.tipTooltip.SetToolTip(this.lblBuildComplexForms, "Complex Forms cost a number of BP equal to their Rating.");
            //
            // lblBuildSprites
            //
            this.lblBuildSprites.AutoSize = true;
            this.lblBuildSprites.Location = new System.Drawing.Point(10, 345);
            this.lblBuildSprites.Name = "lblBuildSprites";
            this.lblBuildSprites.Size = new System.Drawing.Size(39, 13);
            this.lblBuildSprites.TabIndex = 71;
            this.lblBuildSprites.Tag = "Label_SummarySprites";
            this.lblBuildSprites.Text = "Sprites";
            this.tipTooltip.SetToolTip(this.lblBuildSprites, "Each Service a Sprite owes costs 1 BP.");
            //
            // lblBuildSpirits
            //
            this.lblBuildSpirits.AutoSize = true;
            this.lblBuildSpirits.Location = new System.Drawing.Point(10, 324);
            this.lblBuildSpirits.Name = "lblBuildSpirits";
            this.lblBuildSpirits.Size = new System.Drawing.Size(35, 13);
            this.lblBuildSpirits.TabIndex = 70;
            this.lblBuildSpirits.Tag = "Label_SummarySpirits";
            this.lblBuildSpirits.Text = "Spirits";
            this.tipTooltip.SetToolTip(this.lblBuildSpirits, "Each Service a Spirit owes costs 1 BP.");
            //
            // lblBuildSpells
            //
            this.lblBuildSpells.AutoSize = true;
            this.lblBuildSpells.Location = new System.Drawing.Point(10, 240);
            this.lblBuildSpells.Name = "lblBuildSpells";
            this.lblBuildSpells.Size = new System.Drawing.Size(35, 13);
            this.lblBuildSpells.TabIndex = 69;
            this.lblBuildSpells.Tag = "Label_SummarySpells";
            this.lblBuildSpells.Text = "Spells";
            this.tipTooltip.SetToolTip(this.lblBuildSpells, "Each Spell costs 3 BP.");
            //
            // lblBuildKnowledgeSkills
            //
            this.lblBuildKnowledgeSkills.AutoSize = true;
            this.lblBuildKnowledgeSkills.Location = new System.Drawing.Point(10, 219);
            this.lblBuildKnowledgeSkills.Name = "lblBuildKnowledgeSkills";
            this.lblBuildKnowledgeSkills.Size = new System.Drawing.Size(87, 13);
            this.lblBuildKnowledgeSkills.TabIndex = 64;
            this.lblBuildKnowledgeSkills.Tag = "Label_SummaryKnowledgeSkills";
            this.lblBuildKnowledgeSkills.Text = "Knowledge Skills";
            this.tipTooltip.SetToolTip(this.lblBuildKnowledgeSkills, "Characters receive (INT + LOG) x 3 free points to spend on general Knowledge Skil" +
        "ls including Languages. Additional Knowledge Skill points cost 2 BP each.");
            //
            // lblBuildActiveSkills
            //
            this.lblBuildActiveSkills.AutoSize = true;
            this.lblBuildActiveSkills.Location = new System.Drawing.Point(10, 198);
            this.lblBuildActiveSkills.Name = "lblBuildActiveSkills";
            this.lblBuildActiveSkills.Size = new System.Drawing.Size(64, 13);
            this.lblBuildActiveSkills.TabIndex = 63;
            this.lblBuildActiveSkills.Tag = "Label_SummaryActiveSkills";
            this.lblBuildActiveSkills.Text = "Active Skills";
            this.tipTooltip.SetToolTip(this.lblBuildActiveSkills, "Skills cost 4 BP per point and 2 BP for a Specialization.");
            //
            // lblBuildSkillGroups
            //
            this.lblBuildSkillGroups.AutoSize = true;
            this.lblBuildSkillGroups.Location = new System.Drawing.Point(10, 177);
            this.lblBuildSkillGroups.Name = "lblBuildSkillGroups";
            this.lblBuildSkillGroups.Size = new System.Drawing.Size(63, 13);
            this.lblBuildSkillGroups.TabIndex = 62;
            this.lblBuildSkillGroups.Tag = "Label_SummarySkillGroups";
            this.lblBuildSkillGroups.Text = "Skill Groups";
            this.tipTooltip.SetToolTip(this.lblBuildSkillGroups, "Skill Groups cost 10 BP per point.");
            //
            // lblBuildContacts
            //
            this.lblBuildContacts.AutoSize = true;
            this.lblBuildContacts.Location = new System.Drawing.Point(10, 114);
            this.lblBuildContacts.Name = "lblBuildContacts";
            this.lblBuildContacts.Size = new System.Drawing.Size(49, 13);
            this.lblBuildContacts.TabIndex = 59;
            this.lblBuildContacts.Tag = "Label_SummaryContacts";
            this.lblBuildContacts.Text = "Contacts";
            this.tipTooltip.SetToolTip(this.lblBuildContacts, "Each point of Connection or Loyalty costs 1 BP.");
            //
            // lblBuildPrimaryAttributes
            //
            this.lblBuildPrimaryAttributes.AutoSize = true;
            this.lblBuildPrimaryAttributes.Location = new System.Drawing.Point(10, 30);
            this.lblBuildPrimaryAttributes.Name = "lblBuildPrimaryAttributes";
            this.lblBuildPrimaryAttributes.Size = new System.Drawing.Size(51, 13);
            this.lblBuildPrimaryAttributes.TabIndex = 57;
            this.lblBuildPrimaryAttributes.Tag = "Label_Attributes";
            this.lblBuildPrimaryAttributes.Text = "Attributes";
            this.tipTooltip.SetToolTip(this.lblBuildPrimaryAttributes, "Characters cannot spend more than 50% of their BP on Primary Attributes (not incl" +
        "uding EDG, MAG, and RES).\r\nOnly one attribute may be at its Maximum value during" +
        " character creation.");
            //
            // lblBuildNegativeQualities
            //
            this.lblBuildNegativeQualities.AutoSize = true;
            this.lblBuildNegativeQualities.Location = new System.Drawing.Point(10, 93);
            this.lblBuildNegativeQualities.Name = "lblBuildNegativeQualities";
            this.lblBuildNegativeQualities.Size = new System.Drawing.Size(93, 13);
            this.lblBuildNegativeQualities.TabIndex = 54;
            this.lblBuildNegativeQualities.Tag = "Label_SummaryNegativeQualities";
            this.lblBuildNegativeQualities.Text = "Negative Qualities";
            this.tipTooltip.SetToolTip(this.lblBuildNegativeQualities, "Characters may select up to -35 BP worth of Negative Qualities. Qualities in dark" +
        " red do not count towards the -35 BP limit.");
            //
            // lblBuildPositiveQualities
            //
            this.lblBuildPositiveQualities.AutoSize = true;
            this.lblBuildPositiveQualities.Location = new System.Drawing.Point(10, 72);
            this.lblBuildPositiveQualities.Name = "lblBuildPositiveQualities";
            this.lblBuildPositiveQualities.Size = new System.Drawing.Size(87, 13);
            this.lblBuildPositiveQualities.TabIndex = 50;
            this.lblBuildPositiveQualities.Tag = "Label_SummaryPositiveQualities";
            this.lblBuildPositiveQualities.Text = "Positive Qualities";
            this.tipTooltip.SetToolTip(this.lblBuildPositiveQualities, "Characters may select up to 35 BP worth of Positive Qualities. Qualities in dark " +
        "red do not count towards the 35 BP limit.");
            //
            // lblRiggingINILabel
            //
            this.lblRiggingINILabel.AutoSize = true;
            this.lblRiggingINILabel.Location = new System.Drawing.Point(6, 167);
            this.lblRiggingINILabel.Name = "lblRiggingINILabel";
            this.lblRiggingINILabel.Size = new System.Drawing.Size(112, 13);
            this.lblRiggingINILabel.TabIndex = 75;
            this.lblRiggingINILabel.Tag = "Label_OtherRiggingInit";
            this.lblRiggingINILabel.Text = "Rigging Initiative (AR):";
            this.tipTooltip.SetToolTip(this.lblRiggingINILabel, "Matrix Initiative is calculated as Commlink Response + INT.");
            //
            // lblMatrixINIHotLabel
            //
            this.lblMatrixINIHotLabel.AutoSize = true;
            this.lblMatrixINIHotLabel.Location = new System.Drawing.Point(6, 144);
            this.lblMatrixINIHotLabel.Name = "lblMatrixINIHotLabel";
            this.lblMatrixINIHotLabel.Size = new System.Drawing.Size(106, 13);
            this.lblMatrixINIHotLabel.TabIndex = 73;
            this.lblMatrixINIHotLabel.Tag = "Label_OtherMatrixInitVRHot";
            this.lblMatrixINIHotLabel.Text = "Matrix Initiative (Hot):";
            this.tipTooltip.SetToolTip(this.lblMatrixINIHotLabel, "Matrix Initiative is calculated as Commlink Response + INT.");
            //
            // lblMatrixINIColdLabel
            //
            this.lblMatrixINIColdLabel.AutoSize = true;
            this.lblMatrixINIColdLabel.Location = new System.Drawing.Point(6, 121);
            this.lblMatrixINIColdLabel.Name = "lblMatrixINIColdLabel";
            this.lblMatrixINIColdLabel.Size = new System.Drawing.Size(110, 13);
            this.lblMatrixINIColdLabel.TabIndex = 71;
            this.lblMatrixINIColdLabel.Tag = "Label_OtherMatrixInitVRCold";
            this.lblMatrixINIColdLabel.Text = "Matrix Initiative (Cold):";
            this.tipTooltip.SetToolTip(this.lblMatrixINIColdLabel, "Matrix Initiative is calculated as Commlink Response + INT.");
            //
            // lblMemoryLabel
            //
            this.lblMemoryLabel.AutoSize = true;
            this.lblMemoryLabel.Location = new System.Drawing.Point(6, 324);
            this.lblMemoryLabel.Name = "lblMemoryLabel";
            this.lblMemoryLabel.Size = new System.Drawing.Size(47, 13);
            this.lblMemoryLabel.TabIndex = 50;
            this.lblMemoryLabel.Tag = "Label_OtherMemory";
            this.lblMemoryLabel.Text = "Memory:";
            this.tipTooltip.SetToolTip(this.lblMemoryLabel, "Memory is calculated as LOG + WIL.");
            //
            // lblLiftCarryLabel
            //
            this.lblLiftCarryLabel.AutoSize = true;
            this.lblLiftCarryLabel.Location = new System.Drawing.Point(6, 301);
            this.lblLiftCarryLabel.Name = "lblLiftCarryLabel";
            this.lblLiftCarryLabel.Size = new System.Drawing.Size(72, 13);
            this.lblLiftCarryLabel.TabIndex = 48;
            this.lblLiftCarryLabel.Tag = "Label_OtherLiftAndCarry";
            this.lblLiftCarryLabel.Text = "Lift and Carry:";
            this.tipTooltip.SetToolTip(this.lblLiftCarryLabel, "Lift and Carry is calculated as STR + BOD.");
            //
            // lblJudgeIntentionsLabel
            //
            this.lblJudgeIntentionsLabel.AutoSize = true;
            this.lblJudgeIntentionsLabel.Location = new System.Drawing.Point(6, 278);
            this.lblJudgeIntentionsLabel.Name = "lblJudgeIntentionsLabel";
            this.lblJudgeIntentionsLabel.Size = new System.Drawing.Size(88, 13);
            this.lblJudgeIntentionsLabel.TabIndex = 46;
            this.lblJudgeIntentionsLabel.Tag = "Label_OtherJudgeIntention";
            this.lblJudgeIntentionsLabel.Text = "Judge Intentions:";
            this.tipTooltip.SetToolTip(this.lblJudgeIntentionsLabel, "Judge Intentions is calculated as INT + CHA.");
            //
            // lblComposureLabel
            //
            this.lblComposureLabel.AutoSize = true;
            this.lblComposureLabel.Location = new System.Drawing.Point(6, 255);
            this.lblComposureLabel.Name = "lblComposureLabel";
            this.lblComposureLabel.Size = new System.Drawing.Size(63, 13);
            this.lblComposureLabel.TabIndex = 44;
            this.lblComposureLabel.Tag = "Label_OtherCmposure";
            this.lblComposureLabel.Text = "Composure:";
            this.tipTooltip.SetToolTip(this.lblComposureLabel, "Composure is calculated as WIL + CHA.");
            //
            // lblRemainingNuyenLabel
            //
            this.lblRemainingNuyenLabel.AutoSize = true;
            this.lblRemainingNuyenLabel.Location = new System.Drawing.Point(6, 232);
            this.lblRemainingNuyenLabel.Name = "lblRemainingNuyenLabel";
            this.lblRemainingNuyenLabel.Size = new System.Drawing.Size(94, 13);
            this.lblRemainingNuyenLabel.TabIndex = 36;
            this.lblRemainingNuyenLabel.Tag = "Label_OtherNuyenRemain";
            this.lblRemainingNuyenLabel.Text = "Nuyen Remaining:";
            this.tipTooltip.SetToolTip(this.lblRemainingNuyenLabel, "The amount of Nuyen you have left to purchase gear.");
            //
            // lblESS
            //
            this.lblESS.AutoSize = true;
            this.lblESS.Location = new System.Drawing.Point(6, 209);
            this.lblESS.Name = "lblESS";
            this.lblESS.Size = new System.Drawing.Size(51, 13);
            this.lblESS.TabIndex = 34;
            this.lblESS.Tag = "Label_OtherEssence";
            this.lblESS.Text = "Essence:";
            this.tipTooltip.SetToolTip(this.lblESS, "Characters start with 6 Essence which is decreased by adding Cyberware and Biowar" +
        "e.");
            //
            // lblArmorLabel
            //
            this.lblArmorLabel.AutoSize = true;
            this.lblArmorLabel.Location = new System.Drawing.Point(6, 188);
            this.lblArmorLabel.Name = "lblArmorLabel";
            this.lblArmorLabel.Size = new System.Drawing.Size(37, 13);
            this.lblArmorLabel.TabIndex = 70;
            this.lblArmorLabel.Tag = "Label_ArmorValueShort";
            this.lblArmorLabel.Text = "Armor:";
            this.tipTooltip.SetToolTip(this.lblArmorLabel, "Only the highest single Armor value is used to determine your Armor Rating.");
            //
            // lblAstralINILabel
            //
            this.lblAstralINILabel.AutoSize = true;
            this.lblAstralINILabel.Location = new System.Drawing.Point(6, 75);
            this.lblAstralINILabel.Name = "lblAstralINILabel";
            this.lblAstralINILabel.Size = new System.Drawing.Size(78, 13);
            this.lblAstralINILabel.TabIndex = 23;
            this.lblAstralINILabel.Tag = "Label_OtherAstralInit";
            this.lblAstralINILabel.Text = "Astral Initiative:";
            this.tipTooltip.SetToolTip(this.lblAstralINILabel, "Astral Initiative is calculated as INT x 2.");
            //
            // lblMatrixINILabel
            //
            this.lblMatrixINILabel.AutoSize = true;
            this.lblMatrixINILabel.Location = new System.Drawing.Point(6, 98);
            this.lblMatrixINILabel.Name = "lblMatrixINILabel";
            this.lblMatrixINILabel.Size = new System.Drawing.Size(104, 13);
            this.lblMatrixINILabel.TabIndex = 22;
            this.lblMatrixINILabel.Tag = "Label_OtherMatrixInit";
            this.lblMatrixINILabel.Text = "Matrix Initiative (AR):";
            this.tipTooltip.SetToolTip(this.lblMatrixINILabel, "Matrix Initiative is calculated as Commlink Response + INT.");
            //
            // lblINILabel
            //
            this.lblINILabel.AutoSize = true;
            this.lblINILabel.Location = new System.Drawing.Point(6, 53);
            this.lblINILabel.Name = "lblINILabel";
            this.lblINILabel.Size = new System.Drawing.Size(49, 13);
            this.lblINILabel.TabIndex = 20;
            this.lblINILabel.Tag = "Label_OtherInit";
            this.lblINILabel.Text = "Initiative:";
            this.tipTooltip.SetToolTip(this.lblINILabel, "Initiative is calculated as REA + INT.");
            //
            // lblCMStunLabel
            //
            this.lblCMStunLabel.AutoSize = true;
            this.lblCMStunLabel.Location = new System.Drawing.Point(6, 31);
            this.lblCMStunLabel.Name = "lblCMStunLabel";
            this.lblCMStunLabel.Size = new System.Drawing.Size(110, 13);
            this.lblCMStunLabel.TabIndex = 19;
            this.lblCMStunLabel.Tag = "Label_OtherStunCM";
            this.lblCMStunLabel.Text = "Stun Condition Track:";
            this.tipTooltip.SetToolTip(this.lblCMStunLabel, "Stun CM is calculated as 8 + (WIL / 2).");
            //
            // lblCMPhysicalLabel
            //
            this.lblCMPhysicalLabel.AutoSize = true;
            this.lblCMPhysicalLabel.Location = new System.Drawing.Point(6, 9);
            this.lblCMPhysicalLabel.Name = "lblCMPhysicalLabel";
            this.lblCMPhysicalLabel.Size = new System.Drawing.Size(127, 13);
            this.lblCMPhysicalLabel.TabIndex = 18;
            this.lblCMPhysicalLabel.Tag = "Label_OtherPhysicalCM";
            this.lblCMPhysicalLabel.Text = "Physical Condition Track:";
            this.tipTooltip.SetToolTip(this.lblCMPhysicalLabel, "Physical CM is calculated as 8 + (BOD / 2).");
            //
            // label33
            //
            this.label33.AutoSize = true;
            this.label33.Location = new System.Drawing.Point(8, 402);
            this.label33.Name = "label33";
            this.label33.Size = new System.Drawing.Size(35, 13);
            this.label33.TabIndex = 108;
            this.label33.Tag = "Label_Spirits";
            this.label33.Text = "Spirits";
            this.tipTooltip.SetToolTip(this.label33, "Each Service a Spirit owes costs 1 BP.");
            //
            // lblSelectedSpells
            //
            this.lblSelectedSpells.AutoSize = true;
            this.lblSelectedSpells.Location = new System.Drawing.Point(8, 10);
            this.lblSelectedSpells.Name = "lblSelectedSpells";
            this.lblSelectedSpells.Size = new System.Drawing.Size(80, 13);
            this.lblSelectedSpells.TabIndex = 2;
            this.lblSelectedSpells.Tag = "Label_SelectedSpells";
            this.lblSelectedSpells.Text = "Selected Spells";
            this.tipTooltip.SetToolTip(this.lblSelectedSpells, "Each Spell costs 3 BP.");
            //
            // label7
            //
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(459, 32);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(37, 13);
            this.label7.TabIndex = 92;
            this.label7.Tag = "String_Karma";
            this.label7.Text = "Karma";
            this.tipTooltip.SetToolTip(this.label7, "Base Attribute value.");
            //
            // lblSpellDefenceIndirectDodgeLabel
            //
            this.lblSpellDefenceIndirectDodgeLabel.AutoSize = true;
            this.lblSpellDefenceIndirectDodgeLabel.Location = new System.Drawing.Point(3, 32);
            this.lblSpellDefenceIndirectDodgeLabel.Name = "lblSpellDefenceIndirectDodgeLabel";
            this.lblSpellDefenceIndirectDodgeLabel.Size = new System.Drawing.Size(77, 13);
            this.lblSpellDefenceIndirectDodgeLabel.TabIndex = 25;
            this.lblSpellDefenceIndirectDodgeLabel.Tag = "Label_SpellDefenceIndirectDodge";
            this.lblSpellDefenceIndirectDodgeLabel.Text = "Indirect Dodge";
            this.tipTooltip.SetToolTip(this.lblSpellDefenceIndirectDodgeLabel, "Physical CM is calculated as 8 + (BOD / 2).");
            //
            // lblSpellDefenceIndirectSoakLabel
            //
            this.lblSpellDefenceIndirectSoakLabel.AutoSize = true;
            this.lblSpellDefenceIndirectSoakLabel.Location = new System.Drawing.Point(3, 52);
            this.lblSpellDefenceIndirectSoakLabel.Name = "lblSpellDefenceIndirectSoakLabel";
            this.lblSpellDefenceIndirectSoakLabel.Size = new System.Drawing.Size(70, 13);
            this.lblSpellDefenceIndirectSoakLabel.TabIndex = 27;
            this.lblSpellDefenceIndirectSoakLabel.Tag = "Label_SpellDefenceIndirect";
            this.lblSpellDefenceIndirectSoakLabel.Text = "Indirect Soak";
            this.tipTooltip.SetToolTip(this.lblSpellDefenceIndirectSoakLabel, "Physical CM is calculated as 8 + (BOD / 2).");
            //
            // lblSpellDefenceDirectSoakManaLabel
            //
            this.lblSpellDefenceDirectSoakManaLabel.AutoSize = true;
            this.lblSpellDefenceDirectSoakManaLabel.Location = new System.Drawing.Point(3, 72);
            this.lblSpellDefenceDirectSoakManaLabel.Name = "lblSpellDefenceDirectSoakManaLabel";
            this.lblSpellDefenceDirectSoakManaLabel.Size = new System.Drawing.Size(99, 13);
            this.lblSpellDefenceDirectSoakManaLabel.TabIndex = 29;
            this.lblSpellDefenceDirectSoakManaLabel.Tag = "Label_SpellDefenceDirectSoakMana";
            this.lblSpellDefenceDirectSoakManaLabel.Text = "Direct Soak - Mana";
            this.tipTooltip.SetToolTip(this.lblSpellDefenceDirectSoakManaLabel, "Physical CM is calculated as 8 + (BOD / 2).");
            //
            // lblSpellDefenceDirectSoakPhysicalLabel
            //
            this.lblSpellDefenceDirectSoakPhysicalLabel.AutoSize = true;
            this.lblSpellDefenceDirectSoakPhysicalLabel.Location = new System.Drawing.Point(3, 92);
            this.lblSpellDefenceDirectSoakPhysicalLabel.Name = "lblSpellDefenceDirectSoakPhysicalLabel";
            this.lblSpellDefenceDirectSoakPhysicalLabel.Size = new System.Drawing.Size(111, 13);
            this.lblSpellDefenceDirectSoakPhysicalLabel.TabIndex = 31;
            this.lblSpellDefenceDirectSoakPhysicalLabel.Tag = "Label_SpellDefenceDirectSoakPhysical";
            this.lblSpellDefenceDirectSoakPhysicalLabel.Text = "Direct Soak - Physical";
            this.tipTooltip.SetToolTip(this.lblSpellDefenceDirectSoakPhysicalLabel, "Physical CM is calculated as 8 + (BOD / 2).");
            //
            // lblSpellDefenceDecAttBODLabel
            //
            this.lblSpellDefenceDecAttBODLabel.AutoSize = true;
            this.lblSpellDefenceDecAttBODLabel.Location = new System.Drawing.Point(3, 132);
            this.lblSpellDefenceDecAttBODLabel.Name = "lblSpellDefenceDecAttBODLabel";
            this.lblSpellDefenceDecAttBODLabel.Size = new System.Drawing.Size(127, 13);
            this.lblSpellDefenceDecAttBODLabel.TabIndex = 35;
            this.lblSpellDefenceDecAttBODLabel.Tag = "Label_SpellDefenceDecAttBOD";
            this.lblSpellDefenceDecAttBODLabel.Text = "Decrease Attribute (BOD)";
            this.tipTooltip.SetToolTip(this.lblSpellDefenceDecAttBODLabel, "Physical CM is calculated as 8 + (BOD / 2).");
            //
            // lblSpellDefenceDetectionLabel
            //
            this.lblSpellDefenceDetectionLabel.AutoSize = true;
            this.lblSpellDefenceDetectionLabel.Location = new System.Drawing.Point(3, 112);
            this.lblSpellDefenceDetectionLabel.Name = "lblSpellDefenceDetectionLabel";
            this.lblSpellDefenceDetectionLabel.Size = new System.Drawing.Size(84, 13);
            this.lblSpellDefenceDetectionLabel.TabIndex = 33;
            this.lblSpellDefenceDetectionLabel.Tag = "Label_SpellDefenceDetection";
            this.lblSpellDefenceDetectionLabel.Text = "Detection Spells";
            this.tipTooltip.SetToolTip(this.lblSpellDefenceDetectionLabel, "Physical CM is calculated as 8 + (BOD / 2).");
            //
            // lblSpellDefenceDecAttAGILabel
            //
            this.lblSpellDefenceDecAttAGILabel.AutoSize = true;
            this.lblSpellDefenceDecAttAGILabel.Location = new System.Drawing.Point(3, 152);
            this.lblSpellDefenceDecAttAGILabel.Name = "lblSpellDefenceDecAttAGILabel";
            this.lblSpellDefenceDecAttAGILabel.Size = new System.Drawing.Size(122, 13);
            this.lblSpellDefenceDecAttAGILabel.TabIndex = 41;
            this.lblSpellDefenceDecAttAGILabel.Tag = "Label_SpellDefenceDecAttAGI";
            this.lblSpellDefenceDecAttAGILabel.Text = "Decrease Attribute (AGI)";
            this.tipTooltip.SetToolTip(this.lblSpellDefenceDecAttAGILabel, "Physical CM is calculated as 8 + (BOD / 2).");
            //
            // lblSpellDefenceDecAttSTRLabel
            //
            this.lblSpellDefenceDecAttSTRLabel.AutoSize = true;
            this.lblSpellDefenceDecAttSTRLabel.Location = new System.Drawing.Point(3, 192);
            this.lblSpellDefenceDecAttSTRLabel.Name = "lblSpellDefenceDecAttSTRLabel";
            this.lblSpellDefenceDecAttSTRLabel.Size = new System.Drawing.Size(126, 13);
            this.lblSpellDefenceDecAttSTRLabel.TabIndex = 43;
            this.lblSpellDefenceDecAttSTRLabel.Tag = "Label_SpellDefenceDecAttSTR";
            this.lblSpellDefenceDecAttSTRLabel.Text = "Decrease Attribute (STR)";
            this.tipTooltip.SetToolTip(this.lblSpellDefenceDecAttSTRLabel, "Physical CM is calculated as 8 + (BOD / 2).");
            //
            // lblSpellDefenceDecAttREALabel
            //
            this.lblSpellDefenceDecAttREALabel.AutoSize = true;
            this.lblSpellDefenceDecAttREALabel.Location = new System.Drawing.Point(3, 172);
            this.lblSpellDefenceDecAttREALabel.Name = "lblSpellDefenceDecAttREALabel";
            this.lblSpellDefenceDecAttREALabel.Size = new System.Drawing.Size(126, 13);
            this.lblSpellDefenceDecAttREALabel.TabIndex = 42;
            this.lblSpellDefenceDecAttREALabel.Tag = "Label_SpellDefenceDecAttREA";
            this.lblSpellDefenceDecAttREALabel.Text = "Decrease Attribute (REA)";
            this.tipTooltip.SetToolTip(this.lblSpellDefenceDecAttREALabel, "Physical CM is calculated as 8 + (BOD / 2).");
            //
            // lblSpellDefenceDecAttWILLabel
            //
            this.lblSpellDefenceDecAttWILLabel.AutoSize = true;
            this.lblSpellDefenceDecAttWILLabel.Location = new System.Drawing.Point(3, 272);
            this.lblSpellDefenceDecAttWILLabel.Name = "lblSpellDefenceDecAttWILLabel";
            this.lblSpellDefenceDecAttWILLabel.Size = new System.Drawing.Size(124, 13);
            this.lblSpellDefenceDecAttWILLabel.TabIndex = 47;
            this.lblSpellDefenceDecAttWILLabel.Tag = "Label_SpellDefenceDecAttWIL";
            this.lblSpellDefenceDecAttWILLabel.Text = "Decrease Attribute (WIL)";
            this.tipTooltip.SetToolTip(this.lblSpellDefenceDecAttWILLabel, "Physical CM is calculated as 8 + (BOD / 2).");
            //
            // lblSpellDefenceDecAttLOGLabel
            //
            this.lblSpellDefenceDecAttLOGLabel.AutoSize = true;
            this.lblSpellDefenceDecAttLOGLabel.Location = new System.Drawing.Point(3, 252);
            this.lblSpellDefenceDecAttLOGLabel.Name = "lblSpellDefenceDecAttLOGLabel";
            this.lblSpellDefenceDecAttLOGLabel.Size = new System.Drawing.Size(126, 13);
            this.lblSpellDefenceDecAttLOGLabel.TabIndex = 46;
            this.lblSpellDefenceDecAttLOGLabel.Tag = "Label_SpellDefenceDecAttLOG";
            this.lblSpellDefenceDecAttLOGLabel.Text = "Decrease Attribute (LOG)";
            this.tipTooltip.SetToolTip(this.lblSpellDefenceDecAttLOGLabel, "Physical CM is calculated as 8 + (BOD / 2).");
            //
            // lblSpellDefenceDecAttINTLabel
            //
            this.lblSpellDefenceDecAttINTLabel.AutoSize = true;
            this.lblSpellDefenceDecAttINTLabel.Location = new System.Drawing.Point(3, 232);
            this.lblSpellDefenceDecAttINTLabel.Name = "lblSpellDefenceDecAttINTLabel";
            this.lblSpellDefenceDecAttINTLabel.Size = new System.Drawing.Size(122, 13);
            this.lblSpellDefenceDecAttINTLabel.TabIndex = 45;
            this.lblSpellDefenceDecAttINTLabel.Tag = "Label_SpellDefenceDecAttINT";
            this.lblSpellDefenceDecAttINTLabel.Text = "Decrease Attribute (INT)";
            this.tipTooltip.SetToolTip(this.lblSpellDefenceDecAttINTLabel, "Physical CM is calculated as 8 + (BOD / 2).");
            //
            // lblSpellDefenceDecAttCHALabel
            //
            this.lblSpellDefenceDecAttCHALabel.AutoSize = true;
            this.lblSpellDefenceDecAttCHALabel.Location = new System.Drawing.Point(3, 212);
            this.lblSpellDefenceDecAttCHALabel.Name = "lblSpellDefenceDecAttCHALabel";
            this.lblSpellDefenceDecAttCHALabel.Size = new System.Drawing.Size(126, 13);
            this.lblSpellDefenceDecAttCHALabel.TabIndex = 44;
            this.lblSpellDefenceDecAttCHALabel.Tag = "Label_SpellDefenceDecAttCHA";
            this.lblSpellDefenceDecAttCHALabel.Text = "Decrease Attribute (CHA)";
            this.tipTooltip.SetToolTip(this.lblSpellDefenceDecAttCHALabel, "Physical CM is calculated as 8 + (BOD / 2).");
            //
            // lbllSpellDefenceManipPhysicalLabel
            //
            this.lbllSpellDefenceManipPhysicalLabel.AutoSize = true;
            this.lbllSpellDefenceManipPhysicalLabel.Location = new System.Drawing.Point(3, 351);
            this.lbllSpellDefenceManipPhysicalLabel.Name = "lbllSpellDefenceManipPhysicalLabel";
            this.lbllSpellDefenceManipPhysicalLabel.Size = new System.Drawing.Size(115, 13);
            this.lbllSpellDefenceManipPhysicalLabel.TabIndex = 59;
            this.lbllSpellDefenceManipPhysicalLabel.Tag = "Label_SpellDefenceManipPhysical";
            this.lbllSpellDefenceManipPhysicalLabel.Text = "Manipulation - Physical";
            this.tipTooltip.SetToolTip(this.lbllSpellDefenceManipPhysicalLabel, "Physical CM is calculated as 8 + (BOD / 2).");
            //
            // lblSpellDefenceManipMentalLabel
            //
            this.lblSpellDefenceManipMentalLabel.AutoSize = true;
            this.lblSpellDefenceManipMentalLabel.Location = new System.Drawing.Point(3, 331);
            this.lblSpellDefenceManipMentalLabel.Name = "lblSpellDefenceManipMentalLabel";
            this.lblSpellDefenceManipMentalLabel.Size = new System.Drawing.Size(108, 13);
            this.lblSpellDefenceManipMentalLabel.TabIndex = 57;
            this.lblSpellDefenceManipMentalLabel.Tag = "Label_SpellDefenceManipMental";
            this.lblSpellDefenceManipMentalLabel.Text = "Manipulation - Mental";
            this.tipTooltip.SetToolTip(this.lblSpellDefenceManipMentalLabel, "Physical CM is calculated as 8 + (BOD / 2).");
            //
            // lblSpellDefenceIllusionPhysicalLabel
            //
            this.lblSpellDefenceIllusionPhysicalLabel.AutoSize = true;
            this.lblSpellDefenceIllusionPhysicalLabel.Location = new System.Drawing.Point(3, 311);
            this.lblSpellDefenceIllusionPhysicalLabel.Name = "lblSpellDefenceIllusionPhysicalLabel";
            this.lblSpellDefenceIllusionPhysicalLabel.Size = new System.Drawing.Size(87, 13);
            this.lblSpellDefenceIllusionPhysicalLabel.TabIndex = 55;
            this.lblSpellDefenceIllusionPhysicalLabel.Tag = "Label_SpellDefenceIllusionPhysical";
            this.lblSpellDefenceIllusionPhysicalLabel.Text = "Illusion - Physical";
            this.tipTooltip.SetToolTip(this.lblSpellDefenceIllusionPhysicalLabel, "Physical CM is calculated as 8 + (BOD / 2).");
            //
            // lblSpellDefenceIllusionManaLabel
            //
            this.lblSpellDefenceIllusionManaLabel.AutoSize = true;
            this.lblSpellDefenceIllusionManaLabel.Location = new System.Drawing.Point(3, 291);
            this.lblSpellDefenceIllusionManaLabel.Name = "lblSpellDefenceIllusionManaLabel";
            this.lblSpellDefenceIllusionManaLabel.Size = new System.Drawing.Size(75, 13);
            this.lblSpellDefenceIllusionManaLabel.TabIndex = 53;
            this.lblSpellDefenceIllusionManaLabel.Tag = "Label_SpellDefenceIllusionMana";
            this.lblSpellDefenceIllusionManaLabel.Text = "Illusion - Mana";
            this.tipTooltip.SetToolTip(this.lblSpellDefenceIllusionManaLabel, "Physical CM is calculated as 8 + (BOD / 2).");
            //
            // lblCounterspellingDiceLabel
            //
            this.lblCounterspellingDiceLabel.AutoSize = true;
            this.lblCounterspellingDiceLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblCounterspellingDiceLabel.Location = new System.Drawing.Point(3, 11);
            this.lblCounterspellingDiceLabel.Name = "lblCounterspellingDiceLabel";
            this.lblCounterspellingDiceLabel.Size = new System.Drawing.Size(124, 13);
            this.lblCounterspellingDiceLabel.TabIndex = 62;
            this.lblCounterspellingDiceLabel.Tag = "Label_CounterspellingDice";
            this.lblCounterspellingDiceLabel.Text = "Counterspelling Dice";
            this.tipTooltip.SetToolTip(this.lblCounterspellingDiceLabel, "Physical CM is calculated as 8 + (BOD / 2).");
            //
            // lblFoci
            //
            this.lblFoci.AutoSize = true;
            this.lblFoci.Location = new System.Drawing.Point(307, 332);
            this.lblFoci.Name = "lblFoci";
            this.lblFoci.Size = new System.Drawing.Size(67, 13);
            this.lblFoci.TabIndex = 92;
            this.lblFoci.Tag = "Label_BondedFoci";
            this.lblFoci.Text = "Bonded Foci";
            this.tipTooltip.SetToolTip(this.lblFoci, "Each bonded Focus costs a number of karma depending on its type and force\r\n");
            //
            // lblBuildAIAdvancedPrograms
            //
            this.lblBuildAIAdvancedPrograms.AutoSize = true;
            this.lblBuildAIAdvancedPrograms.Location = new System.Drawing.Point(10, 450);
            this.lblBuildAIAdvancedPrograms.Name = "lblBuildAIAdvancedPrograms";
            this.lblBuildAIAdvancedPrograms.Size = new System.Drawing.Size(103, 13);
            this.lblBuildAIAdvancedPrograms.TabIndex = 87;
            this.lblBuildAIAdvancedPrograms.Tag = "Label_SummaryAIAdvancedPrograms";
            this.lblBuildAIAdvancedPrograms.Text = "Advanced Programs";
            this.tipTooltip.SetToolTip(this.lblBuildAIAdvancedPrograms, "Each Martial Art Maneuver costs 2 BP.");
            //
            // lblBuildRitualsBPLabel
            //
            this.lblBuildRitualsBPLabel.AutoSize = true;
            this.lblBuildRitualsBPLabel.Location = new System.Drawing.Point(10, 282);
            this.lblBuildRitualsBPLabel.Name = "lblBuildRitualsBPLabel";
            this.lblBuildRitualsBPLabel.Size = new System.Drawing.Size(39, 13);
            this.lblBuildRitualsBPLabel.TabIndex = 133;
            this.lblBuildRitualsBPLabel.Tag = "Label_SummaryRituals";
            this.lblBuildRitualsBPLabel.Text = "Rituals";
            this.tipTooltip.SetToolTip(this.lblBuildRitualsBPLabel, "Each Spell costs 3 BP.");
            //
            // lblBuildPrepsBPLabel
            //
            this.lblBuildPrepsBPLabel.AutoSize = true;
            this.lblBuildPrepsBPLabel.Location = new System.Drawing.Point(10, 261);
            this.lblBuildPrepsBPLabel.Name = "lblBuildPrepsBPLabel";
            this.lblBuildPrepsBPLabel.Size = new System.Drawing.Size(66, 13);
            this.lblBuildPrepsBPLabel.TabIndex = 131;
            this.lblBuildPrepsBPLabel.Tag = "Label_SummaryPreparations";
            this.lblBuildPrepsBPLabel.Text = "Preparations";
            this.tipTooltip.SetToolTip(this.lblBuildPrepsBPLabel, "Each Spell costs 3 BP.");
            //
            // lblAIProgramsAdvancedPrograms
            //
            this.lblAIProgramsAdvancedPrograms.AutoSize = true;
            this.lblAIProgramsAdvancedPrograms.Location = new System.Drawing.Point(8, 9);
            this.lblAIProgramsAdvancedPrograms.Name = "lblAIProgramsAdvancedPrograms";
            this.lblAIProgramsAdvancedPrograms.Size = new System.Drawing.Size(184, 13);
            this.lblAIProgramsAdvancedPrograms.TabIndex = 28;
            this.lblAIProgramsAdvancedPrograms.Tag = "Label_AIProgramsAdvancedPrograms";
            this.lblAIProgramsAdvancedPrograms.Text = "AI Programs and Advanced Programs";
            //
            // lblPublicAware
            //
            this.lblPublicAware.AutoSize = true;
            this.lblPublicAware.Location = new System.Drawing.Point(654, 142);
            this.lblPublicAware.Name = "lblPublicAware";
            this.lblPublicAware.Size = new System.Drawing.Size(94, 13);
            this.lblPublicAware.TabIndex = 86;
            this.lblPublicAware.Tag = "Label_PublicAwareness";
            this.lblPublicAware.Text = "Public Awareness:";
            //
            // lblSocialLabel
            //
            this.lblSocialLabel.AutoSize = true;
            this.lblSocialLabel.Location = new System.Drawing.Point(328, 84);
            this.lblSocialLabel.Name = "lblSocialLabel";
            this.lblSocialLabel.Size = new System.Drawing.Size(36, 13);
            this.lblSocialLabel.TabIndex = 76;
            this.lblSocialLabel.Tag = "Node_Social";
            this.lblSocialLabel.Text = "Social";
            //
            // lblMentalLabel
            //
            this.lblMentalLabel.AutoSize = true;
            this.lblMentalLabel.Location = new System.Drawing.Point(328, 58);
            this.lblMentalLabel.Name = "lblMentalLabel";
            this.lblMentalLabel.Size = new System.Drawing.Size(39, 13);
            this.lblMentalLabel.TabIndex = 74;
            this.lblMentalLabel.Tag = "Node_Mental";
            this.lblMentalLabel.Text = "Mental";
            //
            // lblPhysicalLabel
            //
            this.lblPhysicalLabel.AutoSize = true;
            this.lblPhysicalLabel.Location = new System.Drawing.Point(328, 32);
            this.lblPhysicalLabel.Name = "lblPhysicalLabel";
            this.lblPhysicalLabel.Size = new System.Drawing.Size(46, 13);
            this.lblPhysicalLabel.TabIndex = 72;
            this.lblPhysicalLabel.Tag = "Node_Physical";
            this.lblPhysicalLabel.Text = "Physical";
            //
            // lblAstralLabel
            //
            this.lblAstralLabel.AutoSize = true;
            this.lblAstralLabel.Location = new System.Drawing.Point(328, 111);
            this.lblAstralLabel.Name = "lblAstralLabel";
            this.lblAstralLabel.Size = new System.Drawing.Size(33, 13);
            this.lblAstralLabel.TabIndex = 78;
            this.lblAstralLabel.Tag = "Node_Astral";
            this.lblAstralLabel.Text = "Astral";
            //
            // cmsMartialArts
            //
            this.cmsMartialArts.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsMartialArtsAddAdvantage,
            this.tsMartialArtsNotes});
            this.cmsMartialArts.Name = "cmsWeapon";
            this.cmsMartialArts.Size = new System.Drawing.Size(155, 48);
            this.cmsMartialArts.Opening += new System.ComponentModel.CancelEventHandler(this.ContextMenu_Opening);
            //
            // tsMartialArtsAddAdvantage
            //
            this.tsMartialArtsAddAdvantage.Image = global::Chummer.Properties.Resources.medal_gold_add;
            this.tsMartialArtsAddAdvantage.Name = "tsMartialArtsAddAdvantage";
            this.tsMartialArtsAddAdvantage.Size = new System.Drawing.Size(154, 22);
            this.tsMartialArtsAddAdvantage.Tag = "Menu_AddAdvantage";
            this.tsMartialArtsAddAdvantage.Text = "&Add Technique";
            this.tsMartialArtsAddAdvantage.Click += new System.EventHandler(this.tsMartialArtsAddAdvantage_Click);
            //
            // tsMartialArtsNotes
            //
            this.tsMartialArtsNotes.Image = global::Chummer.Properties.Resources.note_edit;
            this.tsMartialArtsNotes.Name = "tsMartialArtsNotes";
            this.tsMartialArtsNotes.Size = new System.Drawing.Size(154, 22);
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
            this.cmsSpellButton.Opening += new System.ComponentModel.CancelEventHandler(this.ContextMenu_Opening);
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
            this.cmsComplexForm.Opening += new System.ComponentModel.CancelEventHandler(this.ContextMenu_Opening);
            //
            // tsAddComplexFormOption
            //
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
            this.cmsCyberware.Opening += new System.ComponentModel.CancelEventHandler(this.ContextMenu_Opening);
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
            // cmsLifestyle
            //
            this.cmsLifestyle.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsAdvancedLifestyle});
            this.cmsLifestyle.Name = "cmsLifestyle";
            this.cmsLifestyle.Size = new System.Drawing.Size(174, 26);
            this.cmsLifestyle.Opening += new System.ComponentModel.CancelEventHandler(this.ContextMenu_Opening);
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
            this.cmsArmor.Opening += new System.ComponentModel.CancelEventHandler(this.ContextMenu_Opening);
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
            this.cmsWeapon.Opening += new System.ComponentModel.CancelEventHandler(this.ContextMenu_Opening);
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
            // tsWeaponAddModification
            //
            this.tsWeaponAddModification.Name = "tsWeaponAddModification";
            this.tsWeaponAddModification.Size = new System.Drawing.Size(32, 19);
            //
            // cmsGearButton
            //
            this.cmsGearButton.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsGearButtonAddAccessory,
            this.tsGearAddNexus});
            this.cmsGearButton.Name = "cmsGearButton";
            this.cmsGearButton.Size = new System.Drawing.Size(153, 48);
            this.cmsGearButton.Opening += new System.ComponentModel.CancelEventHandler(this.ContextMenu_Opening);
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
            // tsGearAddNexus
            //
            this.tsGearAddNexus.Image = global::Chummer.Properties.Resources.computer_add;
            this.tsGearAddNexus.Name = "tsGearAddNexus";
            this.tsGearAddNexus.Size = new System.Drawing.Size(152, 22);
            this.tsGearAddNexus.Tag = "Menu_AddNexus";
            this.tsGearAddNexus.Text = "A&dd Nexus";
            this.tsGearAddNexus.Visible = false;
            this.tsGearAddNexus.Click += new System.EventHandler(this.tsGearAddNexus_Click);
            //
            // cmsVehicle
            //
            this.cmsVehicle.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsVehicleAddWeaponMount,
            this.tsVehicleAddMod,
            this.tsVehicleAddCyberware,
            this.tsVehicleAddSensor,
            this.tsVehicleAddWeapon,
            this.tsVehicleName,
            this.tsVehicleNotes});
            this.cmsVehicle.Name = "cmsWeapon";
            this.cmsVehicle.Size = new System.Drawing.Size(193, 158);
            this.cmsVehicle.Opening += new System.ComponentModel.CancelEventHandler(this.ContextMenu_Opening);
            //
            // tsVehicleAddWeaponMount
            //
            this.tsVehicleAddWeaponMount.Image = global::Chummer.Properties.Resources.car_add;
            this.tsVehicleAddWeaponMount.Name = "tsVehicleAddWeaponMount";
            this.tsVehicleAddWeaponMount.Size = new System.Drawing.Size(192, 22);
            this.tsVehicleAddWeaponMount.Text = "Add Weapon Mount";
            this.tsVehicleAddWeaponMount.Visible = false;
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
            this.tsVehicleSensorAddAsPlugin,
            this.tsVehicleAddNexus});
            this.tsVehicleAddSensor.Image = global::Chummer.Properties.Resources.camera_add;
            this.tsVehicleAddSensor.Name = "tsVehicleAddSensor";
            this.tsVehicleAddSensor.Size = new System.Drawing.Size(192, 22);
            this.tsVehicleAddSensor.Tag = "Menu_Gear";
            this.tsVehicleAddSensor.Text = "&Gear";
            this.tsVehicleAddSensor.DropDownOpening += new System.EventHandler(this.ContextMenu_DropDownOpening);
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
            // tsVehicleAddNexus
            //
            this.tsVehicleAddNexus.Image = global::Chummer.Properties.Resources.computer_add;
            this.tsVehicleAddNexus.Name = "tsVehicleAddNexus";
            this.tsVehicleAddNexus.Size = new System.Drawing.Size(147, 22);
            this.tsVehicleAddNexus.Tag = "Menu_AddNexus";
            this.tsVehicleAddNexus.Text = "A&dd Nexus";
            this.tsVehicleAddNexus.Visible = false;
            this.tsVehicleAddNexus.Click += new System.EventHandler(this.tsVehicleAddNexus_Click);
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
            this.tsVehicleAddWeapon.DropDownOpening += new System.EventHandler(this.ContextMenu_DropDownOpening);
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
            // mnuCreateMenu
            //
            this.mnuCreateMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuCreateFile,
            this.mnuCreateEdit,
            this.mnuCreateSpecial});
            this.mnuCreateMenu.Location = new System.Drawing.Point(0, 0);
            this.mnuCreateMenu.Name = "mnuCreateMenu";
            this.mnuCreateMenu.Size = new System.Drawing.Size(1040, 24);
            this.mnuCreateMenu.TabIndex = 51;
            this.mnuCreateMenu.Text = "Top Level Menu";
            this.mnuCreateMenu.Visible = false;
            //
            // mnuCreateFile
            //
            this.mnuCreateFile.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuFileSave,
            this.mnuFileSaveAs,
            this.toolStripSeparator1,
            this.mnuFileClose,
            this.toolStripSeparator2,
            this.mnuFilePrint});
            this.mnuCreateFile.MergeAction = System.Windows.Forms.MergeAction.MatchOnly;
            this.mnuCreateFile.Name = "mnuCreateFile";
            this.mnuCreateFile.Size = new System.Drawing.Size(37, 20);
            this.mnuCreateFile.Tag = "Menu_Main_File";
            this.mnuCreateFile.Text = "&File";
            //
            // mnuFileSave
            //
            this.mnuFileSave.Image = ((System.Drawing.Image)(resources.GetObject("mnuFileSave.Image")));
            this.mnuFileSave.ImageTransparentColor = System.Drawing.Color.Black;
            this.mnuFileSave.MergeAction = System.Windows.Forms.MergeAction.Insert;
            this.mnuFileSave.MergeIndex = 3;
            this.mnuFileSave.Name = "mnuFileSave";
            this.mnuFileSave.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
            this.mnuFileSave.Size = new System.Drawing.Size(148, 22);
            this.mnuFileSave.Tag = "Menu_FileSave";
            this.mnuFileSave.Text = "&Save";
            this.mnuFileSave.Click += new System.EventHandler(this.mnuFileSave_Click);
            //
            // mnuFileSaveAs
            //
            this.mnuFileSaveAs.MergeAction = System.Windows.Forms.MergeAction.Insert;
            this.mnuFileSaveAs.MergeIndex = 4;
            this.mnuFileSaveAs.Name = "mnuFileSaveAs";
            this.mnuFileSaveAs.Size = new System.Drawing.Size(148, 22);
            this.mnuFileSaveAs.Tag = "Menu_FileSaveAs";
            this.mnuFileSaveAs.Text = "Save &As";
            this.mnuFileSaveAs.Click += new System.EventHandler(this.mnuFileSaveAs_Click);
            //
            // toolStripSeparator1
            //
            this.toolStripSeparator1.MergeAction = System.Windows.Forms.MergeAction.Insert;
            this.toolStripSeparator1.MergeIndex = 5;
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(145, 6);
            //
            // mnuFileClose
            //
            this.mnuFileClose.MergeAction = System.Windows.Forms.MergeAction.Insert;
            this.mnuFileClose.MergeIndex = 6;
            this.mnuFileClose.Name = "mnuFileClose";
            this.mnuFileClose.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.W)));
            this.mnuFileClose.Size = new System.Drawing.Size(148, 22);
            this.mnuFileClose.Tag = "Menu_FileClose";
            this.mnuFileClose.Text = "&Close";
            this.mnuFileClose.Click += new System.EventHandler(this.mnuFileClose_Click);
            //
            // toolStripSeparator2
            //
            this.toolStripSeparator2.MergeIndex = 7;
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(145, 6);
            //
            // mnuFilePrint
            //
            this.mnuFilePrint.Image = global::Chummer.Properties.Resources.printer;
            this.mnuFilePrint.MergeAction = System.Windows.Forms.MergeAction.Insert;
            this.mnuFilePrint.MergeIndex = 8;
            this.mnuFilePrint.Name = "mnuFilePrint";
            this.mnuFilePrint.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.P)));
            this.mnuFilePrint.Size = new System.Drawing.Size(148, 22);
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
            this.mnuCreateEdit.DropDownOpening += new System.EventHandler(this.Menu_DropDownOpening);
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
            this.mnuSpecialMutantCritter,
            this.mnuSpecialToxicCritter,
            this.mnuSpecialCyberzombie,
            this.mnuSpecialConvertToFreeSprite,
            this.mnuSpecialReapplyImprovements,
            this.mnuSpecialBPAvailLimit,
            this.mnuSpecialConfirmValidity});
            this.mnuCreateSpecial.MergeAction = System.Windows.Forms.MergeAction.Insert;
            this.mnuCreateSpecial.MergeIndex = 3;
            this.mnuCreateSpecial.Name = "mnuCreateSpecial";
            this.mnuCreateSpecial.Size = new System.Drawing.Size(56, 20);
            this.mnuCreateSpecial.Tag = "Menu_Special";
            this.mnuCreateSpecial.Text = "&Special";
            this.mnuCreateSpecial.DropDownOpening += new System.EventHandler(this.Menu_DropDownOpening);
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
            this.mnuSpecialCyberzombie.Image = global::Chummer.Properties.Resources.emoticon_evilgrin;
            this.mnuSpecialCyberzombie.Name = "mnuSpecialCyberzombie";
            this.mnuSpecialCyberzombie.Size = new System.Drawing.Size(213, 22);
            this.mnuSpecialCyberzombie.Tag = "Menu_SpecialConverToCyberzombie";
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
            this.mnuSpecialReapplyImprovements.Image = global::Chummer.Properties.Resources.arrow_redo;
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
            this.mnuSpecialConfirmValidity.Image = global::Chummer.Properties.Resources.cog_edit;
            this.mnuSpecialConfirmValidity.Name = "mnuSpecialConfirmValidity";
            this.mnuSpecialConfirmValidity.Size = new System.Drawing.Size(213, 22);
            this.mnuSpecialConfirmValidity.Tag = "Menu_ValidCharacter";
            this.mnuSpecialConfirmValidity.Text = "Confirm Character Validity";
            this.mnuSpecialConfirmValidity.Click += new System.EventHandler(this.mnuSpecialConfirmValidity_Click);
            //
            // toolStrip
            //
            this.toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsbSave,
            this.tsbPrint,
            this.tsbSeparator,
            this.tsbCopy,
            this.tsbPaste});
            this.toolStrip.Location = new System.Drawing.Point(0, 0);
            this.toolStrip.Name = "toolStrip";
            this.toolStrip.Size = new System.Drawing.Size(1040, 25);
            this.toolStrip.TabIndex = 53;
            this.toolStrip.Text = "ToolStrip";
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
            // tsbSeparator
            //
            this.tsbSeparator.MergeIndex = 5;
            this.tsbSeparator.Name = "tsbSeparator";
            this.tsbSeparator.Size = new System.Drawing.Size(6, 25);
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
            this.cmsGear.Name = "cmsWeapon";
            this.cmsGear.Size = new System.Drawing.Size(148, 70);
            this.cmsGear.Opening += new System.ComponentModel.CancelEventHandler(this.ContextMenu_Opening);
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
            // cmsVehicleWeapon
            //
            this.cmsVehicleWeapon.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsVehicleAddWeaponAccessoryAlt,
            this.tsVehicleAddUnderbarrelWeaponAlt,
            this.tsVehicleWeaponNotes});
            this.cmsVehicleWeapon.Name = "cmsWeapon";
            this.cmsVehicleWeapon.Size = new System.Drawing.Size(209, 70);
            this.cmsVehicleWeapon.Opening += new System.ComponentModel.CancelEventHandler(this.ContextMenu_Opening);
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
            this.tsVehicleWeaponNotes.Click += new System.EventHandler(this.tsVehicleWeaponNotes_Click);
            //
            // cmsVehicleGear
            //
            this.cmsVehicleGear.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsVehicleGearAddAsPlugin,
            this.tsVehicleGearNotes});
            this.cmsVehicleGear.Name = "cmsWeapon";
            this.cmsVehicleGear.Size = new System.Drawing.Size(148, 48);
            this.cmsVehicleGear.Opening += new System.ComponentModel.CancelEventHandler(this.ContextMenu_Opening);
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
            this.cmsArmorGear.Opening += new System.ComponentModel.CancelEventHandler(this.ContextMenu_Opening);
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
            this.tsArmorGearNotes.Click += new System.EventHandler(this.tsArmorGearNotes_Click);
            //
            // cmsArmorMod
            //
            this.cmsArmorMod.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsArmorModNotes});
            this.cmsArmorMod.Name = "cmsArmorMod";
            this.cmsArmorMod.Size = new System.Drawing.Size(106, 26);
            this.cmsArmorMod.Opening += new System.ComponentModel.CancelEventHandler(this.ContextMenu_Opening);
            //
            // tsArmorModNotes
            //
            this.tsArmorModNotes.Image = global::Chummer.Properties.Resources.note_edit;
            this.tsArmorModNotes.Name = "tsArmorModNotes";
            this.tsArmorModNotes.Size = new System.Drawing.Size(105, 22);
            this.tsArmorModNotes.Tag = "Menu_Notes";
            this.tsArmorModNotes.Text = "&Notes";
            this.tsArmorModNotes.Click += new System.EventHandler(this.tsArmorModNotes_Click);
            //
            // cmsQuality
            //
            this.cmsQuality.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsQualityNotes});
            this.cmsQuality.Name = "cmsQuality";
            this.cmsQuality.Size = new System.Drawing.Size(106, 26);
            this.cmsQuality.Opening += new System.ComponentModel.CancelEventHandler(this.ContextMenu_Opening);
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
            this.tsSpellNotes});
            this.cmsSpell.Name = "cmsSpell";
            this.cmsSpell.Size = new System.Drawing.Size(106, 26);
            this.cmsSpell.Opening += new System.ComponentModel.CancelEventHandler(this.ContextMenu_Opening);
            //
            // tsSpellNotes
            //
            this.tsSpellNotes.Image = global::Chummer.Properties.Resources.note_edit;
            this.tsSpellNotes.Name = "tsSpellNotes";
            this.tsSpellNotes.Size = new System.Drawing.Size(105, 22);
            this.tsSpellNotes.Tag = "Menu_Notes";
            this.tsSpellNotes.Text = "&Notes";
            this.tsSpellNotes.Click += new System.EventHandler(this.tsSpellNotes_Click);
            //
            // cmsCritterPowers
            //
            this.cmsCritterPowers.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsCritterPowersNotes});
            this.cmsCritterPowers.Name = "cmsCritterPowers";
            this.cmsCritterPowers.Size = new System.Drawing.Size(106, 26);
            this.cmsCritterPowers.Opening += new System.ComponentModel.CancelEventHandler(this.ContextMenu_Opening);
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
            this.cmsLifestyleNotes.Opening += new System.ComponentModel.CancelEventHandler(this.ContextMenu_Opening);
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
            this.cmsWeaponAccessory.Opening += new System.ComponentModel.CancelEventHandler(this.ContextMenu_Opening);
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
            this.tsWeaponAccessoryNotes.Click += new System.EventHandler(this.tsWeaponAccessoryNotes_Click);
            //
            // cmsGearPlugin
            //
            this.cmsGearPlugin.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsGearPluginNotes});
            this.cmsGearPlugin.Name = "cmsGearPlugin";
            this.cmsGearPlugin.Size = new System.Drawing.Size(106, 26);
            this.cmsGearPlugin.Opening += new System.ComponentModel.CancelEventHandler(this.ContextMenu_Opening);
            //
            // tsGearPluginNotes
            //
            this.tsGearPluginNotes.Image = global::Chummer.Properties.Resources.note_edit;
            this.tsGearPluginNotes.Name = "tsGearPluginNotes";
            this.tsGearPluginNotes.Size = new System.Drawing.Size(105, 22);
            this.tsGearPluginNotes.Tag = "Menu_Notes";
            this.tsGearPluginNotes.Text = "&Notes";
            this.tsGearPluginNotes.Click += new System.EventHandler(this.tsGearPluginNotes_Click);
            //
            // cmsComplexFormPlugin
            //
            this.cmsComplexFormPlugin.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsComplexFormPluginNotes});
            this.cmsComplexFormPlugin.Name = "cmsComplexFormPlugin";
            this.cmsComplexFormPlugin.Size = new System.Drawing.Size(106, 26);
            this.cmsComplexFormPlugin.Opening += new System.ComponentModel.CancelEventHandler(this.ContextMenu_Opening);
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
            this.cmsBioware.Opening += new System.ComponentModel.CancelEventHandler(this.ContextMenu_Opening);
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
            this.cmsAdvancedLifestyle.Opening += new System.ComponentModel.CancelEventHandler(this.ContextMenu_Opening);
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
            this.tsAdvancedLifestyleNotes.Text = "&Notes";
            this.tsAdvancedLifestyleNotes.Click += new System.EventHandler(this.tsAdvancedLifestyleNotes_Click);
            //
            // cmsGearLocation
            //
            this.cmsGearLocation.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsGearRenameLocation});
            this.cmsGearLocation.Name = "cmsGearLocation";
            this.cmsGearLocation.Size = new System.Drawing.Size(167, 26);
            this.cmsGearLocation.Opening += new System.ComponentModel.CancelEventHandler(this.ContextMenu_Opening);
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
            // cmsArmorLocation
            //
            this.cmsArmorLocation.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsArmorRenameLocation});
            this.cmsArmorLocation.Name = "cmsGearLocation";
            this.cmsArmorLocation.Size = new System.Drawing.Size(167, 26);
            this.cmsArmorLocation.Opening += new System.ComponentModel.CancelEventHandler(this.ContextMenu_Opening);
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
            // cmsWeaponAccessoryGear
            //
            this.cmsWeaponAccessoryGear.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsWeaponAccessoryGearMenuAddAsPlugin});
            this.cmsWeaponAccessoryGear.Name = "cmsCyberwareGear";
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
            this.tsVehicleRenameLocation});
            this.cmsVehicleLocation.Name = "cmsGearLocation";
            this.cmsVehicleLocation.Size = new System.Drawing.Size(167, 26);
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
            this.tsVehicleWeaponAccessoryNotes.Click += new System.EventHandler(this.tsVehicleWeaponAccessoryNotes_Click);
            //
            // cmsVehicleWeaponAccessoryGear
            //
            this.cmsVehicleWeaponAccessoryGear.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsVehicleWeaponAccessoryGearMenuAddAsPlugin});
            this.cmsVehicleWeaponAccessoryGear.Name = "cmsCyberwareGear";
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
            this.tsWeaponRenameLocation});
            this.cmsWeaponLocation.Name = "cmsGearLocation";
            this.cmsWeaponLocation.Size = new System.Drawing.Size(167, 26);
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
            // splitMain
            //
            this.splitMain.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitMain.BackColor = System.Drawing.SystemColors.InactiveCaption;
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
            this.splitMain.Size = new System.Drawing.Size(1040, 637);
            this.splitMain.SplitterDistance = 849;
            this.splitMain.TabIndex = 54;
            //
            // tabCharacterTabs
            //
            this.tabCharacterTabs.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
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
            this.tabCharacterTabs.Controls.Add(this.tabImprovements);
            this.tabCharacterTabs.Location = new System.Drawing.Point(0, 0);
            this.tabCharacterTabs.Name = "tabCharacterTabs";
            this.tabCharacterTabs.SelectedIndex = 0;
            this.tabCharacterTabs.Size = new System.Drawing.Size(846, 637);
            this.tabCharacterTabs.TabIndex = 33;
            this.tabCharacterTabs.SelectedIndexChanged += new System.EventHandler(this.tabCharacterTabs_SelectedIndexChanged);
            //
            // tabCommon
            //
            this.tabCommon.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.tabCommon.Controls.Add(this.pnlAttributes);
            this.tabCommon.Controls.Add(this.cmdLifeModule);
            this.tabCommon.Controls.Add(this.tabPeople);
            this.tabCommon.Controls.Add(this.label7);
            this.tabCommon.Controls.Add(this.txtAlias);
            this.tabCommon.Controls.Add(this.lblAlias);
            this.tabCommon.Controls.Add(this.lblMetatypeSource);
            this.tabCommon.Controls.Add(this.lblMetatypeSourceLabel);
            this.tabCommon.Controls.Add(this.lblQualityBP);
            this.tabCommon.Controls.Add(this.lblQualityBPLabel);
            this.tabCommon.Controls.Add(this.lblQualitySource);
            this.tabCommon.Controls.Add(this.lblQualitySourceLabel);
            this.tabCommon.Controls.Add(this.cmdDeleteQuality);
            this.tabCommon.Controls.Add(this.cmdAddQuality);
            this.tabCommon.Controls.Add(this.nudMysticAdeptMAGMagician);
            this.tabCommon.Controls.Add(this.lblAttributesAug);
            this.tabCommon.Controls.Add(this.lblMysticAdeptAssignment);
            this.tabCommon.Controls.Add(this.lblAttributesBase);
            this.tabCommon.Controls.Add(this.lblAttributesMetatype);
            this.tabCommon.Controls.Add(this.treQualities);
            this.tabCommon.Controls.Add(this.lblMetatype);
            this.tabCommon.Controls.Add(this.lblMetatypeLabel);
            this.tabCommon.Controls.Add(this.lblAttributes);
            this.tabCommon.Controls.Add(this.nudNuyen);
            this.tabCommon.Controls.Add(this.lblNuyenTotal);
            this.tabCommon.Controls.Add(this.lblNuyen);
            this.tabCommon.Location = new System.Drawing.Point(4, 22);
            this.tabCommon.Name = "tabCommon";
            this.tabCommon.Padding = new System.Windows.Forms.Padding(3);
            this.tabCommon.Size = new System.Drawing.Size(838, 611);
            this.tabCommon.TabIndex = 0;
            this.tabCommon.Tag = "Tab_Common";
            this.tabCommon.Text = "Acc";
            //
            // pnlAttributes
            //
            this.pnlAttributes.Location = new System.Drawing.Point(288, 48);
            this.pnlAttributes.Name = "pnlAttributes";
            this.pnlAttributes.Size = new System.Drawing.Size(352, 320);
            this.pnlAttributes.TabIndex = 96;
            //
            // cmdLifeModule
            //
            this.cmdLifeModule.Location = new System.Drawing.Point(187, 9);
            this.cmdLifeModule.Name = "cmdLifeModule";
            this.cmdLifeModule.Size = new System.Drawing.Size(91, 23);
            this.cmdLifeModule.TabIndex = 95;
            this.cmdLifeModule.Tag = "String_Life_Module";
            this.cmdLifeModule.Text = "Life Module";
            this.cmdLifeModule.UseVisualStyleBackColor = true;
            this.cmdLifeModule.Visible = false;
            this.cmdLifeModule.Click += new System.EventHandler(this.cmdLifeModule_Click);
            //
            // tabPeople
            //
            this.tabPeople.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabPeople.Controls.Add(this.tabContacts);
            this.tabPeople.Controls.Add(this.tabEnemies);
            this.tabPeople.Location = new System.Drawing.Point(288, 375);
            this.tabPeople.Name = "tabPeople";
            this.tabPeople.SelectedIndex = 0;
            this.tabPeople.Size = new System.Drawing.Size(544, 230);
            this.tabPeople.TabIndex = 94;
            //
            // tabContacts
            //
            this.tabContacts.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.tabContacts.Controls.Add(this.lblContactPoints);
            this.tabContacts.Controls.Add(this.lblContactPoints_Label);
            this.tabContacts.Controls.Add(this.lblContactArchtypeLabel);
            this.tabContacts.Controls.Add(this.lblContactLocationLabel);
            this.tabContacts.Controls.Add(this.lblContactNameLabel);
            this.tabContacts.Controls.Add(this.cmdAddContact);
            this.tabContacts.Controls.Add(this.panContacts);
            this.tabContacts.Location = new System.Drawing.Point(4, 22);
            this.tabContacts.Name = "tabContacts";
            this.tabContacts.Padding = new System.Windows.Forms.Padding(3);
            this.tabContacts.Size = new System.Drawing.Size(536, 204);
            this.tabContacts.TabIndex = 0;
            this.tabContacts.Text = "Contacts";
            //
            // lblContactPoints
            //
            this.lblContactPoints.AutoSize = true;
            this.lblContactPoints.Location = new System.Drawing.Point(288, 11);
            this.lblContactPoints.Name = "lblContactPoints";
            this.lblContactPoints.Size = new System.Drawing.Size(0, 13);
            this.lblContactPoints.TabIndex = 48;
            //
            // lblContactPoints_Label
            //
            this.lblContactPoints_Label.AutoSize = true;
            this.lblContactPoints_Label.Location = new System.Drawing.Point(88, 11);
            this.lblContactPoints_Label.Name = "lblContactPoints_Label";
            this.lblContactPoints_Label.Size = new System.Drawing.Size(156, 13);
            this.lblContactPoints_Label.TabIndex = 47;
            this.lblContactPoints_Label.Tag = "Label_FreeContactPoints";
            this.lblContactPoints_Label.Text = "Free Contact Points Remaining:";
            //
            // lblContactArchtypeLabel
            //
            this.lblContactArchtypeLabel.AutoSize = true;
            this.lblContactArchtypeLabel.Location = new System.Drawing.Point(255, 32);
            this.lblContactArchtypeLabel.Name = "lblContactArchtypeLabel";
            this.lblContactArchtypeLabel.Size = new System.Drawing.Size(52, 13);
            this.lblContactArchtypeLabel.TabIndex = 44;
            this.lblContactArchtypeLabel.Tag = "Label_Archtype";
            this.lblContactArchtypeLabel.Text = "Archtype:";
            //
            // lblContactLocationLabel
            //
            this.lblContactLocationLabel.AutoSize = true;
            this.lblContactLocationLabel.Location = new System.Drawing.Point(129, 32);
            this.lblContactLocationLabel.Name = "lblContactLocationLabel";
            this.lblContactLocationLabel.Size = new System.Drawing.Size(51, 13);
            this.lblContactLocationLabel.TabIndex = 43;
            this.lblContactLocationLabel.Tag = "Label_Location";
            this.lblContactLocationLabel.Text = "Location:";
            //
            // lblContactNameLabel
            //
            this.lblContactNameLabel.AutoSize = true;
            this.lblContactNameLabel.Location = new System.Drawing.Point(6, 32);
            this.lblContactNameLabel.Name = "lblContactNameLabel";
            this.lblContactNameLabel.Size = new System.Drawing.Size(38, 13);
            this.lblContactNameLabel.TabIndex = 42;
            this.lblContactNameLabel.Tag = "Label_Name";
            this.lblContactNameLabel.Text = "Name:";
            //
            // cmdAddContact
            //
            this.cmdAddContact.AutoSize = true;
            this.cmdAddContact.Location = new System.Drawing.Point(6, 6);
            this.cmdAddContact.Name = "cmdAddContact";
            this.cmdAddContact.Size = new System.Drawing.Size(76, 23);
            this.cmdAddContact.TabIndex = 24;
            this.cmdAddContact.Tag = "Button_AddContact";
            this.cmdAddContact.Text = "&Add Contact";
            this.cmdAddContact.UseVisualStyleBackColor = true;
            this.cmdAddContact.Click += new System.EventHandler(this.cmdAddContact_Click);
            //
            // panContacts
            //
            this.panContacts.AllowDrop = true;
            this.panContacts.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panContacts.AutoScroll = true;
            this.panContacts.Location = new System.Drawing.Point(3, 48);
            this.panContacts.Name = "panContacts";
            this.panContacts.Size = new System.Drawing.Size(530, 154);
            this.panContacts.TabIndex = 25;
            this.panContacts.Click += new System.EventHandler(this.panContacts_Click);
            this.panContacts.DragDrop += new System.Windows.Forms.DragEventHandler(this.panContacts_DragDrop);
            this.panContacts.DragEnter += new System.Windows.Forms.DragEventHandler(this.panContacts_DragEnter);
            this.panContacts.DragOver += new System.Windows.Forms.DragEventHandler(this.panContacts_DragOver);
            //
            // tabEnemies
            //
            this.tabEnemies.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.tabEnemies.Controls.Add(this.label6);
            this.tabEnemies.Controls.Add(this.label15);
            this.tabEnemies.Controls.Add(this.label16);
            this.tabEnemies.Controls.Add(this.panEnemies);
            this.tabEnemies.Controls.Add(this.cmdAddEnemy);
            this.tabEnemies.Location = new System.Drawing.Point(4, 22);
            this.tabEnemies.Name = "tabEnemies";
            this.tabEnemies.Padding = new System.Windows.Forms.Padding(3);
            this.tabEnemies.Size = new System.Drawing.Size(536, 204);
            this.tabEnemies.TabIndex = 1;
            this.tabEnemies.Text = "Enemies";
            //
            // label6
            //
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(255, 32);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(52, 13);
            this.label6.TabIndex = 49;
            this.label6.Tag = "Label_Archtype";
            this.label6.Text = "Archtype:";
            //
            // label15
            //
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(129, 32);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(51, 13);
            this.label15.TabIndex = 48;
            this.label15.Tag = "Label_Location";
            this.label15.Text = "Location:";
            //
            // label16
            //
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(6, 32);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(38, 13);
            this.label16.TabIndex = 47;
            this.label16.Tag = "Label_Name";
            this.label16.Text = "Name:";
            //
            // panEnemies
            //
            this.panEnemies.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panEnemies.AutoScroll = true;
            this.panEnemies.Location = new System.Drawing.Point(3, 48);
            this.panEnemies.Name = "panEnemies";
            this.panEnemies.Size = new System.Drawing.Size(530, 154);
            this.panEnemies.TabIndex = 41;
            this.panEnemies.Click += new System.EventHandler(this.panEnemies_Click);
            //
            // cmdAddEnemy
            //
            this.cmdAddEnemy.AutoSize = true;
            this.cmdAddEnemy.Location = new System.Drawing.Point(6, 6);
            this.cmdAddEnemy.Name = "cmdAddEnemy";
            this.cmdAddEnemy.Size = new System.Drawing.Size(75, 23);
            this.cmdAddEnemy.TabIndex = 40;
            this.cmdAddEnemy.Tag = "Button_AddEnemy";
            this.cmdAddEnemy.Text = "A&dd Enemy";
            this.cmdAddEnemy.UseVisualStyleBackColor = true;
            this.cmdAddEnemy.Click += new System.EventHandler(this.cmdAddEnemy_Click);
            //
            // txtAlias
            //
            this.txtAlias.Location = new System.Drawing.Point(326, 6);
            this.txtAlias.Name = "txtAlias";
            this.txtAlias.Size = new System.Drawing.Size(252, 20);
            this.txtAlias.TabIndex = 91;
            this.txtAlias.TextChanged += new System.EventHandler(this.txtAlias_TextChanged);
            //
            // lblAlias
            //
            this.lblAlias.AutoSize = true;
            this.lblAlias.Location = new System.Drawing.Point(288, 9);
            this.lblAlias.Name = "lblAlias";
            this.lblAlias.Size = new System.Drawing.Size(32, 13);
            this.lblAlias.TabIndex = 90;
            this.lblAlias.Tag = "Label_Alias";
            this.lblAlias.Text = "Alias:";
            //
            // lblMetatypeSource
            //
            this.lblMetatypeSource.AutoSize = true;
            this.lblMetatypeSource.Location = new System.Drawing.Point(710, 32);
            this.lblMetatypeSource.Name = "lblMetatypeSource";
            this.lblMetatypeSource.Size = new System.Drawing.Size(33, 13);
            this.lblMetatypeSource.TabIndex = 89;
            this.lblMetatypeSource.Text = "None";
            this.lblMetatypeSource.Click += new System.EventHandler(this.lblMetatypeSource_Click);
            //
            // lblMetatypeSourceLabel
            //
            this.lblMetatypeSourceLabel.AutoSize = true;
            this.lblMetatypeSourceLabel.Location = new System.Drawing.Point(650, 32);
            this.lblMetatypeSourceLabel.Name = "lblMetatypeSourceLabel";
            this.lblMetatypeSourceLabel.Size = new System.Drawing.Size(44, 13);
            this.lblMetatypeSourceLabel.TabIndex = 88;
            this.lblMetatypeSourceLabel.Tag = "Label_Source";
            this.lblMetatypeSourceLabel.Text = "Source:";
            //
            // lblQualityBP
            //
            this.lblQualityBP.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblQualityBP.AutoSize = true;
            this.lblQualityBP.Location = new System.Drawing.Point(58, 589);
            this.lblQualityBP.Name = "lblQualityBP";
            this.lblQualityBP.Size = new System.Drawing.Size(27, 13);
            this.lblQualityBP.TabIndex = 67;
            this.lblQualityBP.Text = "[BP]";
            //
            // lblQualityBPLabel
            //
            this.lblQualityBPLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblQualityBPLabel.AutoSize = true;
            this.lblQualityBPLabel.Location = new System.Drawing.Point(8, 589);
            this.lblQualityBPLabel.Name = "lblQualityBPLabel";
            this.lblQualityBPLabel.Size = new System.Drawing.Size(40, 13);
            this.lblQualityBPLabel.TabIndex = 66;
            this.lblQualityBPLabel.Tag = "Label_Karma";
            this.lblQualityBPLabel.Text = "Karma:";
            //
            // lblQualitySource
            //
            this.lblQualitySource.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblQualitySource.AutoSize = true;
            this.lblQualitySource.Location = new System.Drawing.Point(232, 589);
            this.lblQualitySource.Name = "lblQualitySource";
            this.lblQualitySource.Size = new System.Drawing.Size(47, 13);
            this.lblQualitySource.TabIndex = 65;
            this.lblQualitySource.Text = "[Source]";
            this.lblQualitySource.Click += new System.EventHandler(this.lblQualitySource_Click);
            //
            // lblQualitySourceLabel
            //
            this.lblQualitySourceLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblQualitySourceLabel.AutoSize = true;
            this.lblQualitySourceLabel.Location = new System.Drawing.Point(182, 589);
            this.lblQualitySourceLabel.Name = "lblQualitySourceLabel";
            this.lblQualitySourceLabel.Size = new System.Drawing.Size(44, 13);
            this.lblQualitySourceLabel.TabIndex = 64;
            this.lblQualitySourceLabel.Tag = "Label_Source";
            this.lblQualitySourceLabel.Text = "Source:";
            //
            // cmdDeleteQuality
            //
            this.cmdDeleteQuality.AutoSize = true;
            this.cmdDeleteQuality.Location = new System.Drawing.Point(101, 9);
            this.cmdDeleteQuality.Name = "cmdDeleteQuality";
            this.cmdDeleteQuality.Size = new System.Drawing.Size(80, 23);
            this.cmdDeleteQuality.TabIndex = 63;
            this.cmdDeleteQuality.Tag = "String_Delete";
            this.cmdDeleteQuality.Text = "Delete";
            this.cmdDeleteQuality.UseVisualStyleBackColor = true;
            this.cmdDeleteQuality.Click += new System.EventHandler(this.cmdDeleteQuality_Click);
            //
            // cmdAddQuality
            //
            this.cmdAddQuality.AutoSize = true;
            this.cmdAddQuality.Location = new System.Drawing.Point(6, 9);
            this.cmdAddQuality.Name = "cmdAddQuality";
            this.cmdAddQuality.Size = new System.Drawing.Size(89, 23);
            this.cmdAddQuality.TabIndex = 62;
            this.cmdAddQuality.Tag = "Button_AddQuality";
            this.cmdAddQuality.Text = "Add &Quality";
            this.cmdAddQuality.UseVisualStyleBackColor = true;
            this.cmdAddQuality.Click += new System.EventHandler(this.cmdAddQuality_Click);
            //
            // nudMysticAdeptMAGMagician
            //
            this.nudMysticAdeptMAGMagician.Location = new System.Drawing.Point(789, 287);
            this.nudMysticAdeptMAGMagician.Name = "nudMysticAdeptMAGMagician";
            this.nudMysticAdeptMAGMagician.Size = new System.Drawing.Size(40, 20);
            this.nudMysticAdeptMAGMagician.TabIndex = 60;
            this.nudMysticAdeptMAGMagician.Visible = false;
            this.nudMysticAdeptMAGMagician.ValueChanged += new System.EventHandler(this.nudMysticAdeptMAGMagician_ValueChanged);
            //
            // lblMysticAdeptAssignment
            //
            this.lblMysticAdeptAssignment.AutoSize = true;
            this.lblMysticAdeptAssignment.Location = new System.Drawing.Point(650, 289);
            this.lblMysticAdeptAssignment.Name = "lblMysticAdeptAssignment";
            this.lblMysticAdeptAssignment.Size = new System.Drawing.Size(133, 13);
            this.lblMysticAdeptAssignment.TabIndex = 56;
            this.lblMysticAdeptAssignment.Tag = "Label_MysticAdeptAssignment";
            this.lblMysticAdeptAssignment.Text = "Mystic Adept Power Points";
            this.lblMysticAdeptAssignment.Visible = false;
            //
            // treQualities
            //
            this.treQualities.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)));
            this.treQualities.Indent = 15;
            this.treQualities.Location = new System.Drawing.Point(6, 38);
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
            this.treQualities.Size = new System.Drawing.Size(273, 545);
            this.treQualities.TabIndex = 33;
            this.treQualities.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treQualities_AfterSelect);
            this.treQualities.KeyDown += new System.Windows.Forms.KeyEventHandler(this.treQualities_KeyDown);
            this.treQualities.MouseDown += new System.Windows.Forms.MouseEventHandler(this.TreeView_MouseDown);
            //
            // lblMetatype
            //
            this.lblMetatype.AutoSize = true;
            this.lblMetatype.Location = new System.Drawing.Point(710, 9);
            this.lblMetatype.Name = "lblMetatype";
            this.lblMetatype.Size = new System.Drawing.Size(33, 13);
            this.lblMetatype.TabIndex = 20;
            this.lblMetatype.Text = "None";
            //
            // lblMetatypeLabel
            //
            this.lblMetatypeLabel.AutoSize = true;
            this.lblMetatypeLabel.Location = new System.Drawing.Point(650, 9);
            this.lblMetatypeLabel.Name = "lblMetatypeLabel";
            this.lblMetatypeLabel.Size = new System.Drawing.Size(54, 13);
            this.lblMetatypeLabel.TabIndex = 19;
            this.lblMetatypeLabel.Tag = "Label_Metatype";
            this.lblMetatypeLabel.Text = "Metatype:";
            //
            // nudNuyen
            //
            this.nudNuyen.Location = new System.Drawing.Point(694, 63);
            this.nudNuyen.Maximum = new decimal(new int[] {
            1073704,
            0,
            0,
            0});
            this.nudNuyen.Name = "nudNuyen";
            this.nudNuyen.Size = new System.Drawing.Size(46, 20);
            this.nudNuyen.TabIndex = 17;
            this.nudNuyen.Value = new decimal(new int[] {
            20,
            0,
            0,
            0});
            this.nudNuyen.ValueChanged += new System.EventHandler(this.nudNuyen_ValueChanged);
            //
            // lblNuyenTotal
            //
            this.lblNuyenTotal.AutoSize = true;
            this.lblNuyenTotal.Location = new System.Drawing.Point(746, 65);
            this.lblNuyenTotal.Name = "lblNuyenTotal";
            this.lblNuyenTotal.Size = new System.Drawing.Size(28, 13);
            this.lblNuyenTotal.TabIndex = 18;
            this.lblNuyenTotal.Text = "= 0¥";
            //
            // tabSkills
            //
            this.tabSkills.Controls.Add(this.tabSkillUc);
            this.tabSkills.Location = new System.Drawing.Point(4, 22);
            this.tabSkills.Name = "tabSkills";
            this.tabSkills.Size = new System.Drawing.Size(838, 611);
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
            this.tabSkillUc.ObjCharacter = null;
            this.tabSkillUc.Size = new System.Drawing.Size(838, 611);
            this.tabSkillUc.TabIndex = 0;
            this.tabSkillUc.Tag = "";
            //
            // tabLimits
            //
            this.tabLimits.Controls.Add(this.lblAstralLabel);
            this.tabLimits.Controls.Add(this.lblAstral);
            this.tabLimits.Controls.Add(this.lblSocialLabel);
            this.tabLimits.Controls.Add(this.lblSocial);
            this.tabLimits.Controls.Add(this.lblMentalLabel);
            this.tabLimits.Controls.Add(this.lblMental);
            this.tabLimits.Controls.Add(this.lblPhysicalLabel);
            this.tabLimits.Controls.Add(this.lblPhysical);
            this.tabLimits.Controls.Add(this.cmdAddLimitModifier);
            this.tabLimits.Controls.Add(this.treLimit);
            this.tabLimits.Controls.Add(this.cmdDeleteLimitModifier);
            this.tabLimits.Location = new System.Drawing.Point(4, 22);
            this.tabLimits.Name = "tabLimits";
            this.tabLimits.Size = new System.Drawing.Size(838, 611);
            this.tabLimits.TabIndex = 13;
            this.tabLimits.Tag = "Tab_Limits";
            this.tabLimits.Text = "Limits";
            //
            // lblAstral
            //
            this.lblAstral.AutoSize = true;
            this.lblAstral.Location = new System.Drawing.Point(411, 111);
            this.lblAstral.Name = "lblAstral";
            this.lblAstral.Size = new System.Drawing.Size(19, 13);
            this.lblAstral.TabIndex = 79;
            this.lblAstral.Text = "[0]";
            //
            // lblSocial
            //
            this.lblSocial.AutoSize = true;
            this.lblSocial.Location = new System.Drawing.Point(411, 84);
            this.lblSocial.Name = "lblSocial";
            this.lblSocial.Size = new System.Drawing.Size(19, 13);
            this.lblSocial.TabIndex = 77;
            this.lblSocial.Text = "[0]";
            //
            // lblMental
            //
            this.lblMental.AutoSize = true;
            this.lblMental.Location = new System.Drawing.Point(411, 58);
            this.lblMental.Name = "lblMental";
            this.lblMental.Size = new System.Drawing.Size(19, 13);
            this.lblMental.TabIndex = 75;
            this.lblMental.Text = "[0]";
            //
            // lblPhysical
            //
            this.lblPhysical.AutoSize = true;
            this.lblPhysical.Location = new System.Drawing.Point(411, 32);
            this.lblPhysical.Name = "lblPhysical";
            this.lblPhysical.Size = new System.Drawing.Size(19, 13);
            this.lblPhysical.TabIndex = 73;
            this.lblPhysical.Text = "[0]";
            //
            // cmdAddLimitModifier
            //
            this.cmdAddLimitModifier.AutoSize = true;
            this.cmdAddLimitModifier.Location = new System.Drawing.Point(8, 4);
            this.cmdAddLimitModifier.Name = "cmdAddLimitModifier";
            this.cmdAddLimitModifier.Size = new System.Drawing.Size(100, 23);
            this.cmdAddLimitModifier.TabIndex = 58;
            this.cmdAddLimitModifier.Tag = "String_AddLimitModifier";
            this.cmdAddLimitModifier.Text = "Add Limit Modifier";
            this.cmdAddLimitModifier.UseVisualStyleBackColor = true;
            this.cmdAddLimitModifier.Click += new System.EventHandler(this.cmdAddLimitModifier_Click);
            //
            // treLimit
            //
            this.treLimit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)));
            this.treLimit.HideSelection = false;
            this.treLimit.Location = new System.Drawing.Point(8, 33);
            this.treLimit.Name = "treLimit";
            treeNode3.Name = "trePhysicalRoot";
            treeNode3.Tag = "Node_Physical";
            treeNode3.Text = "Physical";
            treeNode4.Name = "treMentalRoot";
            treeNode4.Tag = "Node_Mental";
            treeNode4.Text = "Mental";
            treeNode5.Name = "treSocialRoot";
            treeNode5.Tag = "Node_Social";
            treeNode5.Text = "Social";
            this.treLimit.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            treeNode3,
            treeNode4,
            treeNode5});
            this.treLimit.ShowNodeToolTips = true;
            this.treLimit.ShowPlusMinus = false;
            this.treLimit.ShowRootLines = false;
            this.treLimit.Size = new System.Drawing.Size(299, 573);
            this.treLimit.TabIndex = 56;
            this.treLimit.KeyDown += new System.Windows.Forms.KeyEventHandler(this.treLimit_KeyDown);
            //
            // cmdDeleteLimitModifier
            //
            this.cmdDeleteLimitModifier.AutoSize = true;
            this.cmdDeleteLimitModifier.Location = new System.Drawing.Point(227, 4);
            this.cmdDeleteLimitModifier.Name = "cmdDeleteLimitModifier";
            this.cmdDeleteLimitModifier.Size = new System.Drawing.Size(80, 23);
            this.cmdDeleteLimitModifier.TabIndex = 55;
            this.cmdDeleteLimitModifier.Tag = "String_Delete";
            this.cmdDeleteLimitModifier.Text = "Delete";
            this.cmdDeleteLimitModifier.UseVisualStyleBackColor = true;
            this.cmdDeleteLimitModifier.Click += new System.EventHandler(this.cmdDeleteLimitModifier_Click);
            //
            // tabMartialArts
            //
            this.tabMartialArts.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.tabMartialArts.Controls.Add(this.cmdAddMartialArt);
            this.tabMartialArts.Controls.Add(this.lblMartialArtSource);
            this.tabMartialArts.Controls.Add(this.lblMartialArtSourceLabel);
            this.tabMartialArts.Controls.Add(this.treMartialArts);
            this.tabMartialArts.Controls.Add(this.cmdDeleteMartialArt);
            this.tabMartialArts.Location = new System.Drawing.Point(4, 22);
            this.tabMartialArts.Name = "tabMartialArts";
            this.tabMartialArts.Size = new System.Drawing.Size(838, 611);
            this.tabMartialArts.TabIndex = 8;
            this.tabMartialArts.Tag = "Tab_MartialArts";
            this.tabMartialArts.Text = "Martial Arts";
            //
            // cmdAddMartialArt
            //
            this.cmdAddMartialArt.AutoSize = true;
            this.cmdAddMartialArt.ContextMenuStrip = this.cmsMartialArts;
            this.cmdAddMartialArt.Location = new System.Drawing.Point(8, 6);
            this.cmdAddMartialArt.Name = "cmdAddMartialArt";
            this.cmdAddMartialArt.Size = new System.Drawing.Size(107, 23);
            this.cmdAddMartialArt.SplitMenuStrip = this.cmsMartialArts;
            this.cmdAddMartialArt.TabIndex = 55;
            this.cmdAddMartialArt.Tag = "Button_AddMartialArt";
            this.cmdAddMartialArt.Text = "&Add Martial Art";
            this.cmdAddMartialArt.UseVisualStyleBackColor = true;
            this.cmdAddMartialArt.Click += new System.EventHandler(this.cmdAddMartialArt_Click);
            //
            // lblMartialArtSource
            //
            this.lblMartialArtSource.AutoSize = true;
            this.lblMartialArtSource.Location = new System.Drawing.Point(384, 35);
            this.lblMartialArtSource.Name = "lblMartialArtSource";
            this.lblMartialArtSource.Size = new System.Drawing.Size(47, 13);
            this.lblMartialArtSource.TabIndex = 25;
            this.lblMartialArtSource.Text = "[Source]";
            this.lblMartialArtSource.Click += new System.EventHandler(this.lblMartialArtSource_Click);
            //
            // lblMartialArtSourceLabel
            //
            this.lblMartialArtSourceLabel.AutoSize = true;
            this.lblMartialArtSourceLabel.Location = new System.Drawing.Point(333, 35);
            this.lblMartialArtSourceLabel.Name = "lblMartialArtSourceLabel";
            this.lblMartialArtSourceLabel.Size = new System.Drawing.Size(44, 13);
            this.lblMartialArtSourceLabel.TabIndex = 24;
            this.lblMartialArtSourceLabel.Tag = "Label_Source";
            this.lblMartialArtSourceLabel.Text = "Source:";
            //
            // treMartialArts
            //
            this.treMartialArts.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)));
            this.treMartialArts.HideSelection = false;
            this.treMartialArts.Location = new System.Drawing.Point(8, 35);
            this.treMartialArts.Name = "treMartialArts";
            treeNode6.Name = "treMartialArtsRoot";
            treeNode6.Tag = "Node_SelectedMartialArts";
            treeNode6.Text = "Selected Martial Arts";
            treeNode7.Name = "treQualitiesRoot";
            treeNode7.Text = "Selected Qualities";
            this.treMartialArts.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            treeNode6,
            treeNode7});
            this.treMartialArts.ShowNodeToolTips = true;
            this.treMartialArts.ShowPlusMinus = false;
            this.treMartialArts.ShowRootLines = false;
            this.treMartialArts.Size = new System.Drawing.Size(315, 573);
            this.treMartialArts.TabIndex = 2;
            this.treMartialArts.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treMartialArts_AfterSelect);
            this.treMartialArts.KeyDown += new System.Windows.Forms.KeyEventHandler(this.treMartialArts_KeyDown);
            this.treMartialArts.MouseDown += new System.Windows.Forms.MouseEventHandler(this.TreeView_MouseDown);
            //
            // cmdDeleteMartialArt
            //
            this.cmdDeleteMartialArt.AutoSize = true;
            this.cmdDeleteMartialArt.Location = new System.Drawing.Point(227, 7);
            this.cmdDeleteMartialArt.Name = "cmdDeleteMartialArt";
            this.cmdDeleteMartialArt.Size = new System.Drawing.Size(80, 23);
            this.cmdDeleteMartialArt.TabIndex = 1;
            this.cmdDeleteMartialArt.Tag = "String_Delete";
            this.cmdDeleteMartialArt.Text = "Delete";
            this.cmdDeleteMartialArt.UseVisualStyleBackColor = true;
            this.cmdDeleteMartialArt.Click += new System.EventHandler(this.cmdDeleteMartialArt_Click);
            //
            // tabMagician
            //
            this.tabMagician.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.tabMagician.Controls.Add(this.lblTraditionSource);
            this.tabMagician.Controls.Add(this.lblTraditionSourceLabel);
            this.tabMagician.Controls.Add(this.cboSpiritManipulation);
            this.tabMagician.Controls.Add(this.lblSpiritManipulation);
            this.tabMagician.Controls.Add(this.cboSpiritIllusion);
            this.tabMagician.Controls.Add(this.lblSpiritIllusion);
            this.tabMagician.Controls.Add(this.cboSpiritHealth);
            this.tabMagician.Controls.Add(this.lblSpiritHealth);
            this.tabMagician.Controls.Add(this.cboSpiritDetection);
            this.tabMagician.Controls.Add(this.lblSpiritDetection);
            this.tabMagician.Controls.Add(this.cboSpiritCombat);
            this.tabMagician.Controls.Add(this.lblSpiritCombat);
            this.tabMagician.Controls.Add(this.cboDrain);
            this.tabMagician.Controls.Add(this.txtTraditionName);
            this.tabMagician.Controls.Add(this.lblTraditionName);
            this.tabMagician.Controls.Add(this.lblSpellDicePool);
            this.tabMagician.Controls.Add(this.lblSpellDicePoolLabel);
            this.tabMagician.Controls.Add(this.lblMentorSpirit);
            this.tabMagician.Controls.Add(this.lblMentorSpiritLabel);
            this.tabMagician.Controls.Add(this.lblMentorSpiritInformation);
            this.tabMagician.Controls.Add(this.lblDrainAttributesValue);
            this.tabMagician.Controls.Add(this.lblDrainAttributes);
            this.tabMagician.Controls.Add(this.lblDrainAttributesLabel);
            this.tabMagician.Controls.Add(this.cboTradition);
            this.tabMagician.Controls.Add(this.lblTraditionLabel);
            this.tabMagician.Controls.Add(this.lblSpellSource);
            this.tabMagician.Controls.Add(this.lblSpellSourceLabel);
            this.tabMagician.Controls.Add(this.lblSpellType);
            this.tabMagician.Controls.Add(this.lblSpellTypeLabel);
            this.tabMagician.Controls.Add(this.lblSpellDV);
            this.tabMagician.Controls.Add(this.lblSpellDVLabel);
            this.tabMagician.Controls.Add(this.lblSpellDuration);
            this.tabMagician.Controls.Add(this.lblSpellDurationLabel);
            this.tabMagician.Controls.Add(this.lblSpellDamage);
            this.tabMagician.Controls.Add(this.lblSpellDamageLabel);
            this.tabMagician.Controls.Add(this.lblSpellRange);
            this.tabMagician.Controls.Add(this.lblSpellRangeLabel);
            this.tabMagician.Controls.Add(this.lblSpellCategory);
            this.tabMagician.Controls.Add(this.lblSpellCategoryLabel);
            this.tabMagician.Controls.Add(this.lblSpellDescriptors);
            this.tabMagician.Controls.Add(this.lblSpellDescriptorsLabel);
            this.tabMagician.Controls.Add(this.treSpells);
            this.tabMagician.Controls.Add(this.cmdDeleteSpell);
            this.tabMagician.Controls.Add(this.cmdAddSpirit);
            this.tabMagician.Controls.Add(this.lblSpirits);
            this.tabMagician.Controls.Add(this.panSpirits);
            this.tabMagician.Controls.Add(this.lblSelectedSpells);
            this.tabMagician.Controls.Add(this.cmdAddSpell);
            this.tabMagician.Location = new System.Drawing.Point(4, 22);
            this.tabMagician.Name = "tabMagician";
            this.tabMagician.Padding = new System.Windows.Forms.Padding(3);
            this.tabMagician.Size = new System.Drawing.Size(838, 611);
            this.tabMagician.TabIndex = 1;
            this.tabMagician.Tag = "Tab_Magician";
            this.tabMagician.Text = "Spells and Spirits";
            //
            // lblTraditionSource
            //
            this.lblTraditionSource.AutoSize = true;
            this.lblTraditionSource.Location = new System.Drawing.Point(404, 244);
            this.lblTraditionSource.Name = "lblTraditionSource";
            this.lblTraditionSource.Size = new System.Drawing.Size(47, 13);
            this.lblTraditionSource.TabIndex = 155;
            this.lblTraditionSource.Text = "[Source]";
            this.lblTraditionSource.Click += new System.EventHandler(this.lblTraditionSource_Click);
            //
            // lblTraditionSourceLabel
            //
            this.lblTraditionSourceLabel.AutoSize = true;
            this.lblTraditionSourceLabel.Location = new System.Drawing.Point(309, 244);
            this.lblTraditionSourceLabel.Name = "lblTraditionSourceLabel";
            this.lblTraditionSourceLabel.Size = new System.Drawing.Size(44, 13);
            this.lblTraditionSourceLabel.TabIndex = 154;
            this.lblTraditionSourceLabel.Tag = "Label_Source";
            this.lblTraditionSourceLabel.Text = "Source:";
            //
            // cboSpiritManipulation
            //
            this.cboSpiritManipulation.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboSpiritManipulation.FormattingEnabled = true;
            this.cboSpiritManipulation.Location = new System.Drawing.Point(639, 326);
            this.cboSpiritManipulation.Name = "cboSpiritManipulation";
            this.cboSpiritManipulation.Size = new System.Drawing.Size(122, 21);
            this.cboSpiritManipulation.TabIndex = 153;
            this.cboSpiritManipulation.Visible = false;
            this.cboSpiritManipulation.SelectedIndexChanged += new System.EventHandler(this.cboSpiritManipulation_SelectedIndexChanged);
            //
            // lblSpiritManipulation
            //
            this.lblSpiritManipulation.AutoSize = true;
            this.lblSpiritManipulation.Location = new System.Drawing.Point(566, 329);
            this.lblSpiritManipulation.Name = "lblSpiritManipulation";
            this.lblSpiritManipulation.Size = new System.Drawing.Size(70, 13);
            this.lblSpiritManipulation.TabIndex = 152;
            this.lblSpiritManipulation.Tag = "Label_SpiritManipulation";
            this.lblSpiritManipulation.Text = "Manipulation:";
            this.lblSpiritManipulation.Visible = false;
            //
            // cboSpiritIllusion
            //
            this.cboSpiritIllusion.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboSpiritIllusion.FormattingEnabled = true;
            this.cboSpiritIllusion.Location = new System.Drawing.Point(639, 299);
            this.cboSpiritIllusion.Name = "cboSpiritIllusion";
            this.cboSpiritIllusion.Size = new System.Drawing.Size(122, 21);
            this.cboSpiritIllusion.TabIndex = 151;
            this.cboSpiritIllusion.Visible = false;
            this.cboSpiritIllusion.SelectedIndexChanged += new System.EventHandler(this.cboSpiritIllusion_SelectedIndexChanged);
            //
            // lblSpiritIllusion
            //
            this.lblSpiritIllusion.AutoSize = true;
            this.lblSpiritIllusion.Location = new System.Drawing.Point(566, 302);
            this.lblSpiritIllusion.Name = "lblSpiritIllusion";
            this.lblSpiritIllusion.Size = new System.Drawing.Size(42, 13);
            this.lblSpiritIllusion.TabIndex = 150;
            this.lblSpiritIllusion.Tag = "Label_SpiritIllusion";
            this.lblSpiritIllusion.Text = "Illusion:";
            this.lblSpiritIllusion.Visible = false;
            //
            // cboSpiritHealth
            //
            this.cboSpiritHealth.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboSpiritHealth.FormattingEnabled = true;
            this.cboSpiritHealth.Location = new System.Drawing.Point(639, 272);
            this.cboSpiritHealth.Name = "cboSpiritHealth";
            this.cboSpiritHealth.Size = new System.Drawing.Size(122, 21);
            this.cboSpiritHealth.TabIndex = 149;
            this.cboSpiritHealth.Visible = false;
            this.cboSpiritHealth.SelectedIndexChanged += new System.EventHandler(this.cboSpiritHealth_SelectedIndexChanged);
            //
            // lblSpiritHealth
            //
            this.lblSpiritHealth.AutoSize = true;
            this.lblSpiritHealth.Location = new System.Drawing.Point(566, 275);
            this.lblSpiritHealth.Name = "lblSpiritHealth";
            this.lblSpiritHealth.Size = new System.Drawing.Size(41, 13);
            this.lblSpiritHealth.TabIndex = 148;
            this.lblSpiritHealth.Tag = "Label_SpiritHealth";
            this.lblSpiritHealth.Text = "Health:";
            this.lblSpiritHealth.Visible = false;
            //
            // cboSpiritDetection
            //
            this.cboSpiritDetection.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboSpiritDetection.FormattingEnabled = true;
            this.cboSpiritDetection.Location = new System.Drawing.Point(639, 245);
            this.cboSpiritDetection.Name = "cboSpiritDetection";
            this.cboSpiritDetection.Size = new System.Drawing.Size(122, 21);
            this.cboSpiritDetection.TabIndex = 147;
            this.cboSpiritDetection.Visible = false;
            this.cboSpiritDetection.SelectedIndexChanged += new System.EventHandler(this.cboSpiritDetection_SelectedIndexChanged);
            //
            // lblSpiritDetection
            //
            this.lblSpiritDetection.AutoSize = true;
            this.lblSpiritDetection.Location = new System.Drawing.Point(566, 248);
            this.lblSpiritDetection.Name = "lblSpiritDetection";
            this.lblSpiritDetection.Size = new System.Drawing.Size(56, 13);
            this.lblSpiritDetection.TabIndex = 146;
            this.lblSpiritDetection.Tag = "Label_SpiritDetection";
            this.lblSpiritDetection.Text = "Detection:";
            this.lblSpiritDetection.Visible = false;
            //
            // cboSpiritCombat
            //
            this.cboSpiritCombat.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboSpiritCombat.FormattingEnabled = true;
            this.cboSpiritCombat.Location = new System.Drawing.Point(639, 218);
            this.cboSpiritCombat.Name = "cboSpiritCombat";
            this.cboSpiritCombat.Size = new System.Drawing.Size(122, 21);
            this.cboSpiritCombat.TabIndex = 145;
            this.cboSpiritCombat.Visible = false;
            this.cboSpiritCombat.SelectedIndexChanged += new System.EventHandler(this.cboSpiritCombat_SelectedIndexChanged);
            //
            // lblSpiritCombat
            //
            this.lblSpiritCombat.AutoSize = true;
            this.lblSpiritCombat.Location = new System.Drawing.Point(566, 221);
            this.lblSpiritCombat.Name = "lblSpiritCombat";
            this.lblSpiritCombat.Size = new System.Drawing.Size(46, 13);
            this.lblSpiritCombat.TabIndex = 144;
            this.lblSpiritCombat.Tag = "Label_SpiritCombat";
            this.lblSpiritCombat.Text = "Combat:";
            this.lblSpiritCombat.Visible = false;
            //
            // cboDrain
            //
            this.cboDrain.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboDrain.FormattingEnabled = true;
            this.cboDrain.Location = new System.Drawing.Point(407, 218);
            this.cboDrain.Name = "cboDrain";
            this.cboDrain.Size = new System.Drawing.Size(82, 21);
            this.cboDrain.TabIndex = 143;
            this.cboDrain.Visible = false;
            this.cboDrain.SelectedIndexChanged += new System.EventHandler(this.cboDrain_SelectedIndexChanged);
            //
            // txtTraditionName
            //
            this.txtTraditionName.Location = new System.Drawing.Point(639, 195);
            this.txtTraditionName.Name = "txtTraditionName";
            this.txtTraditionName.Size = new System.Drawing.Size(122, 20);
            this.txtTraditionName.TabIndex = 142;
            this.txtTraditionName.Visible = false;
            this.txtTraditionName.TextChanged += new System.EventHandler(this.txtTraditionName_TextChanged);
            //
            // lblTraditionName
            //
            this.lblTraditionName.AutoSize = true;
            this.lblTraditionName.Location = new System.Drawing.Point(566, 198);
            this.lblTraditionName.Name = "lblTraditionName";
            this.lblTraditionName.Size = new System.Drawing.Size(38, 13);
            this.lblTraditionName.TabIndex = 141;
            this.lblTraditionName.Tag = "Label_TraditionName";
            this.lblTraditionName.Text = "Name:";
            this.lblTraditionName.Visible = false;
            //
            // lblSpellDicePool
            //
            this.lblSpellDicePool.AutoSize = true;
            this.lblSpellDicePool.Location = new System.Drawing.Point(378, 172);
            this.lblSpellDicePool.Name = "lblSpellDicePool";
            this.lblSpellDicePool.Size = new System.Drawing.Size(59, 13);
            this.lblSpellDicePool.TabIndex = 107;
            this.lblSpellDicePool.Text = "[Dice Pool]";
            //
            // lblSpellDicePoolLabel
            //
            this.lblSpellDicePoolLabel.AutoSize = true;
            this.lblSpellDicePoolLabel.Location = new System.Drawing.Point(309, 172);
            this.lblSpellDicePoolLabel.Name = "lblSpellDicePoolLabel";
            this.lblSpellDicePoolLabel.Size = new System.Drawing.Size(56, 13);
            this.lblSpellDicePoolLabel.TabIndex = 106;
            this.lblSpellDicePoolLabel.Tag = "Label_DicePool";
            this.lblSpellDicePoolLabel.Text = "Dice Pool:";
            //
            // lblMentorSpirit
            //
            this.lblMentorSpirit.AutoSize = true;
            this.lblMentorSpirit.Location = new System.Drawing.Point(404, 329);
            this.lblMentorSpirit.Name = "lblMentorSpirit";
            this.lblMentorSpirit.Size = new System.Drawing.Size(72, 13);
            this.lblMentorSpirit.TabIndex = 96;
            this.lblMentorSpirit.Text = "[Mentor Spirit]";
            this.lblMentorSpirit.Visible = false;
            //
            // lblMentorSpiritLabel
            //
            this.lblMentorSpiritLabel.AutoSize = true;
            this.lblMentorSpiritLabel.Location = new System.Drawing.Point(309, 329);
            this.lblMentorSpiritLabel.Name = "lblMentorSpiritLabel";
            this.lblMentorSpiritLabel.Size = new System.Drawing.Size(69, 13);
            this.lblMentorSpiritLabel.TabIndex = 95;
            this.lblMentorSpiritLabel.Tag = "Label_MentorSpirit";
            this.lblMentorSpiritLabel.Text = "Mentor Spirit:";
            this.lblMentorSpiritLabel.Visible = false;
            //
            // lblMentorSpiritInformation
            //
            this.lblMentorSpiritInformation.Location = new System.Drawing.Point(309, 352);
            this.lblMentorSpiritInformation.Name = "lblMentorSpiritInformation";
            this.lblMentorSpiritInformation.Size = new System.Drawing.Size(526, 91);
            this.lblMentorSpiritInformation.TabIndex = 94;
            this.lblMentorSpiritInformation.Text = "[Mentor Spirit Information]";
            this.lblMentorSpiritInformation.Visible = false;
            //
            // lblDrainAttributesValue
            //
            this.lblDrainAttributesValue.AutoSize = true;
            this.lblDrainAttributesValue.Location = new System.Drawing.Point(495, 221);
            this.lblDrainAttributesValue.Name = "lblDrainAttributesValue";
            this.lblDrainAttributesValue.Size = new System.Drawing.Size(37, 13);
            this.lblDrainAttributesValue.TabIndex = 93;
            this.lblDrainAttributesValue.Text = "[Total]";
            //
            // lblDrainAttributes
            //
            this.lblDrainAttributes.AutoSize = true;
            this.lblDrainAttributes.Location = new System.Drawing.Point(404, 221);
            this.lblDrainAttributes.Name = "lblDrainAttributes";
            this.lblDrainAttributes.Size = new System.Drawing.Size(57, 13);
            this.lblDrainAttributes.TabIndex = 92;
            this.lblDrainAttributes.Text = "[Attributes]";
            //
            // lblDrainAttributesLabel
            //
            this.lblDrainAttributesLabel.AutoSize = true;
            this.lblDrainAttributesLabel.Location = new System.Drawing.Point(309, 221);
            this.lblDrainAttributesLabel.Name = "lblDrainAttributesLabel";
            this.lblDrainAttributesLabel.Size = new System.Drawing.Size(89, 13);
            this.lblDrainAttributesLabel.TabIndex = 91;
            this.lblDrainAttributesLabel.Tag = "Label_ResistDrain";
            this.lblDrainAttributesLabel.Text = "Resist Drain with:";
            //
            // cboTradition
            //
            this.cboTradition.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboTradition.FormattingEnabled = true;
            this.cboTradition.Location = new System.Drawing.Point(407, 195);
            this.cboTradition.Name = "cboTradition";
            this.cboTradition.Size = new System.Drawing.Size(141, 21);
            this.cboTradition.TabIndex = 90;
            this.cboTradition.SelectedIndexChanged += new System.EventHandler(this.cboTradition_SelectedIndexChanged);
            //
            // lblTraditionLabel
            //
            this.lblTraditionLabel.AutoSize = true;
            this.lblTraditionLabel.Location = new System.Drawing.Point(309, 198);
            this.lblTraditionLabel.Name = "lblTraditionLabel";
            this.lblTraditionLabel.Size = new System.Drawing.Size(51, 13);
            this.lblTraditionLabel.TabIndex = 89;
            this.lblTraditionLabel.Tag = "Label_Tradition";
            this.lblTraditionLabel.Text = "Tradition:";
            //
            // lblSpellSource
            //
            this.lblSpellSource.AutoSize = true;
            this.lblSpellSource.Location = new System.Drawing.Point(378, 149);
            this.lblSpellSource.Name = "lblSpellSource";
            this.lblSpellSource.Size = new System.Drawing.Size(47, 13);
            this.lblSpellSource.TabIndex = 88;
            this.lblSpellSource.Text = "[Source]";
            this.lblSpellSource.Click += new System.EventHandler(this.lblSpellSource_Click);
            //
            // lblSpellSourceLabel
            //
            this.lblSpellSourceLabel.AutoSize = true;
            this.lblSpellSourceLabel.Location = new System.Drawing.Point(309, 149);
            this.lblSpellSourceLabel.Name = "lblSpellSourceLabel";
            this.lblSpellSourceLabel.Size = new System.Drawing.Size(44, 13);
            this.lblSpellSourceLabel.TabIndex = 87;
            this.lblSpellSourceLabel.Tag = "Label_Source";
            this.lblSpellSourceLabel.Text = "Source:";
            //
            // lblSpellType
            //
            this.lblSpellType.AutoSize = true;
            this.lblSpellType.Location = new System.Drawing.Point(557, 80);
            this.lblSpellType.Name = "lblSpellType";
            this.lblSpellType.Size = new System.Drawing.Size(37, 13);
            this.lblSpellType.TabIndex = 84;
            this.lblSpellType.Text = "[Type]";
            //
            // lblSpellTypeLabel
            //
            this.lblSpellTypeLabel.AutoSize = true;
            this.lblSpellTypeLabel.Location = new System.Drawing.Point(488, 80);
            this.lblSpellTypeLabel.Name = "lblSpellTypeLabel";
            this.lblSpellTypeLabel.Size = new System.Drawing.Size(34, 13);
            this.lblSpellTypeLabel.TabIndex = 83;
            this.lblSpellTypeLabel.Tag = "Label_Type";
            this.lblSpellTypeLabel.Text = "Type:";
            //
            // lblSpellDV
            //
            this.lblSpellDV.AutoSize = true;
            this.lblSpellDV.Location = new System.Drawing.Point(557, 126);
            this.lblSpellDV.Name = "lblSpellDV";
            this.lblSpellDV.Size = new System.Drawing.Size(28, 13);
            this.lblSpellDV.TabIndex = 82;
            this.lblSpellDV.Text = "[DV]";
            //
            // lblSpellDVLabel
            //
            this.lblSpellDVLabel.AutoSize = true;
            this.lblSpellDVLabel.Location = new System.Drawing.Point(488, 126);
            this.lblSpellDVLabel.Name = "lblSpellDVLabel";
            this.lblSpellDVLabel.Size = new System.Drawing.Size(25, 13);
            this.lblSpellDVLabel.TabIndex = 81;
            this.lblSpellDVLabel.Tag = "Label_DV";
            this.lblSpellDVLabel.Text = "DV:";
            //
            // lblSpellDuration
            //
            this.lblSpellDuration.AutoSize = true;
            this.lblSpellDuration.Location = new System.Drawing.Point(378, 126);
            this.lblSpellDuration.Name = "lblSpellDuration";
            this.lblSpellDuration.Size = new System.Drawing.Size(53, 13);
            this.lblSpellDuration.TabIndex = 80;
            this.lblSpellDuration.Text = "[Duration]";
            //
            // lblSpellDurationLabel
            //
            this.lblSpellDurationLabel.AutoSize = true;
            this.lblSpellDurationLabel.Location = new System.Drawing.Point(309, 126);
            this.lblSpellDurationLabel.Name = "lblSpellDurationLabel";
            this.lblSpellDurationLabel.Size = new System.Drawing.Size(50, 13);
            this.lblSpellDurationLabel.TabIndex = 79;
            this.lblSpellDurationLabel.Tag = "Label_Duration";
            this.lblSpellDurationLabel.Text = "Duration:";
            //
            // lblSpellDamage
            //
            this.lblSpellDamage.AutoSize = true;
            this.lblSpellDamage.Location = new System.Drawing.Point(557, 103);
            this.lblSpellDamage.Name = "lblSpellDamage";
            this.lblSpellDamage.Size = new System.Drawing.Size(53, 13);
            this.lblSpellDamage.TabIndex = 78;
            this.lblSpellDamage.Text = "[Damage]";
            //
            // lblSpellDamageLabel
            //
            this.lblSpellDamageLabel.AutoSize = true;
            this.lblSpellDamageLabel.Location = new System.Drawing.Point(488, 103);
            this.lblSpellDamageLabel.Name = "lblSpellDamageLabel";
            this.lblSpellDamageLabel.Size = new System.Drawing.Size(50, 13);
            this.lblSpellDamageLabel.TabIndex = 77;
            this.lblSpellDamageLabel.Tag = "Label_Damage";
            this.lblSpellDamageLabel.Text = "Damage:";
            //
            // lblSpellRange
            //
            this.lblSpellRange.AutoSize = true;
            this.lblSpellRange.Location = new System.Drawing.Point(378, 103);
            this.lblSpellRange.Name = "lblSpellRange";
            this.lblSpellRange.Size = new System.Drawing.Size(45, 13);
            this.lblSpellRange.TabIndex = 76;
            this.lblSpellRange.Text = "[Range]";
            //
            // lblSpellRangeLabel
            //
            this.lblSpellRangeLabel.AutoSize = true;
            this.lblSpellRangeLabel.Location = new System.Drawing.Point(309, 103);
            this.lblSpellRangeLabel.Name = "lblSpellRangeLabel";
            this.lblSpellRangeLabel.Size = new System.Drawing.Size(42, 13);
            this.lblSpellRangeLabel.TabIndex = 75;
            this.lblSpellRangeLabel.Tag = "Label_Range";
            this.lblSpellRangeLabel.Text = "Range:";
            //
            // lblSpellCategory
            //
            this.lblSpellCategory.AutoSize = true;
            this.lblSpellCategory.Location = new System.Drawing.Point(378, 80);
            this.lblSpellCategory.Name = "lblSpellCategory";
            this.lblSpellCategory.Size = new System.Drawing.Size(55, 13);
            this.lblSpellCategory.TabIndex = 74;
            this.lblSpellCategory.Text = "[Category]";
            //
            // lblSpellCategoryLabel
            //
            this.lblSpellCategoryLabel.AutoSize = true;
            this.lblSpellCategoryLabel.Location = new System.Drawing.Point(309, 80);
            this.lblSpellCategoryLabel.Name = "lblSpellCategoryLabel";
            this.lblSpellCategoryLabel.Size = new System.Drawing.Size(52, 13);
            this.lblSpellCategoryLabel.TabIndex = 73;
            this.lblSpellCategoryLabel.Tag = "Label_Category";
            this.lblSpellCategoryLabel.Text = "Category:";
            //
            // lblSpellDescriptors
            //
            this.lblSpellDescriptors.AutoSize = true;
            this.lblSpellDescriptors.Location = new System.Drawing.Point(378, 55);
            this.lblSpellDescriptors.Name = "lblSpellDescriptors";
            this.lblSpellDescriptors.Size = new System.Drawing.Size(66, 13);
            this.lblSpellDescriptors.TabIndex = 72;
            this.lblSpellDescriptors.Text = "[Descriptors]";
            //
            // lblSpellDescriptorsLabel
            //
            this.lblSpellDescriptorsLabel.AutoSize = true;
            this.lblSpellDescriptorsLabel.Location = new System.Drawing.Point(309, 55);
            this.lblSpellDescriptorsLabel.Name = "lblSpellDescriptorsLabel";
            this.lblSpellDescriptorsLabel.Size = new System.Drawing.Size(63, 13);
            this.lblSpellDescriptorsLabel.TabIndex = 71;
            this.lblSpellDescriptorsLabel.Tag = "Label_Descriptors";
            this.lblSpellDescriptorsLabel.Text = "Descriptors:";
            //
            // treSpells
            //
            this.treSpells.HideSelection = false;
            this.treSpells.Location = new System.Drawing.Point(8, 55);
            this.treSpells.Name = "treSpells";
            treeNode8.Name = "nodSpellCombatRoot";
            treeNode8.Tag = "Node_SelectedCombatSpells";
            treeNode8.Text = "Selected Combat Spells";
            treeNode9.Name = "nodSpellDetectionRoot";
            treeNode9.Tag = "Node_SelectedDetectionSpells";
            treeNode9.Text = "Selected Detection Spells";
            treeNode10.Name = "nodSpellHealthRoot";
            treeNode10.Tag = "Node_SelectedHealthSpells";
            treeNode10.Text = "Selected Health Spells";
            treeNode11.Name = "nodSpellIllusionRoot";
            treeNode11.Tag = "Node_SelectedIllusionSpells";
            treeNode11.Text = "Selected Illusion Spells";
            treeNode12.Name = "nodSpellManipulationRoot";
            treeNode12.Tag = "Node_SelectedManipulationSpells";
            treeNode12.Text = "Selected Manipulation Spells";
            treeNode13.Name = "nodSpellGeomancyRoot";
            treeNode13.Tag = "Node_SelectedGeomancyRituals";
            treeNode13.Text = "Selected Rituals";
            treeNode14.Name = "nodSpellEnchantmentRoot";
            treeNode14.Tag = "Node_SelectedEnchantments";
            treeNode14.Text = "Selected Enchantments";
            this.treSpells.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            treeNode8,
            treeNode9,
            treeNode10,
            treeNode11,
            treeNode12,
            treeNode13,
            treeNode14});
            this.treSpells.ShowNodeToolTips = true;
            this.treSpells.ShowRootLines = false;
            this.treSpells.Size = new System.Drawing.Size(295, 333);
            this.treSpells.TabIndex = 70;
            this.treSpells.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treSpells_AfterSelect);
            this.treSpells.KeyDown += new System.Windows.Forms.KeyEventHandler(this.treSpells_KeyDown);
            this.treSpells.MouseDown += new System.Windows.Forms.MouseEventHandler(this.TreeView_MouseDown);
            //
            // cmdDeleteSpell
            //
            this.cmdDeleteSpell.AutoSize = true;
            this.cmdDeleteSpell.Location = new System.Drawing.Point(89, 26);
            this.cmdDeleteSpell.Name = "cmdDeleteSpell";
            this.cmdDeleteSpell.Size = new System.Drawing.Size(80, 23);
            this.cmdDeleteSpell.TabIndex = 69;
            this.cmdDeleteSpell.Tag = "String_Delete";
            this.cmdDeleteSpell.Text = "Delete";
            this.cmdDeleteSpell.UseVisualStyleBackColor = true;
            this.cmdDeleteSpell.Click += new System.EventHandler(this.cmdDeleteSpell_Click);
            //
            // cmdAddSpirit
            //
            this.cmdAddSpirit.Location = new System.Drawing.Point(13, 420);
            this.cmdAddSpirit.Name = "cmdAddSpirit";
            this.cmdAddSpirit.Size = new System.Drawing.Size(75, 23);
            this.cmdAddSpirit.TabIndex = 68;
            this.cmdAddSpirit.Tag = "Button_AddSpirit";
            this.cmdAddSpirit.Text = "A&dd Spirit";
            this.cmdAddSpirit.UseVisualStyleBackColor = true;
            this.cmdAddSpirit.Click += new System.EventHandler(this.cmdAddSpirit_Click);
            //
            // panSpirits
            //
            this.panSpirits.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)));
            this.panSpirits.AutoScroll = true;
            this.panSpirits.Location = new System.Drawing.Point(11, 449);
            this.panSpirits.Name = "panSpirits";
            this.panSpirits.Size = new System.Drawing.Size(824, 156);
            this.panSpirits.TabIndex = 4;
            //
            // cmdAddSpell
            //
            this.cmdAddSpell.AutoSize = true;
            this.cmdAddSpell.ContextMenuStrip = this.cmsSpellButton;
            this.cmdAddSpell.Location = new System.Drawing.Point(8, 26);
            this.cmdAddSpell.Name = "cmdAddSpell";
            this.cmdAddSpell.Size = new System.Drawing.Size(80, 23);
            this.cmdAddSpell.SplitMenuStrip = this.cmsSpellButton;
            this.cmdAddSpell.TabIndex = 140;
            this.cmdAddSpell.Tag = "Button_AddSpell";
            this.cmdAddSpell.Text = "&Add Spell";
            this.cmdAddSpell.UseVisualStyleBackColor = true;
            this.cmdAddSpell.Click += new System.EventHandler(this.cmdAddSpell_Click);
            //
            // tabAdept
            //
            this.tabAdept.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.tabAdept.Controls.Add(this.tabPowerUc);
            this.tabAdept.Location = new System.Drawing.Point(4, 22);
            this.tabAdept.Name = "tabAdept";
            this.tabAdept.Size = new System.Drawing.Size(838, 611);
            this.tabAdept.TabIndex = 2;
            this.tabAdept.Tag = "Tab_Adept";
            this.tabAdept.Text = "Adept Powers";
            //
            // tabPowerUc
            //
            this.tabPowerUc.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabPowerUc.Location = new System.Drawing.Point(0, 0);
            this.tabPowerUc.Name = "tabPowerUc";
            this.tabPowerUc.ObjCharacter = null;
            this.tabPowerUc.Size = new System.Drawing.Size(838, 611);
            this.tabPowerUc.TabIndex = 0;
            //
            // tabTechnomancer
            //
            this.tabTechnomancer.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.tabTechnomancer.Controls.Add(this.lblLivingPersonaFirewall);
            this.tabTechnomancer.Controls.Add(this.lblLivingPersonaFirewallLabel);
            this.tabTechnomancer.Controls.Add(this.lblLivingPersonaDataProcessing);
            this.tabTechnomancer.Controls.Add(this.lblLivingPersonaDataProcessingLabel);
            this.tabTechnomancer.Controls.Add(this.lblLivingPersonaSleaze);
            this.tabTechnomancer.Controls.Add(this.lblLivingPersonaSleazeLabel);
            this.tabTechnomancer.Controls.Add(this.lblLivingPersonaAttack);
            this.tabTechnomancer.Controls.Add(this.lblLivingPersonaAttackLabel);
            this.tabTechnomancer.Controls.Add(this.lblFV);
            this.tabTechnomancer.Controls.Add(this.lblFVLabel);
            this.tabTechnomancer.Controls.Add(this.lblDuration);
            this.tabTechnomancer.Controls.Add(this.lblDurationLabel);
            this.tabTechnomancer.Controls.Add(this.lblTarget);
            this.tabTechnomancer.Controls.Add(this.lblTargetLabel);
            this.tabTechnomancer.Controls.Add(this.lblLivingPersonaLabel);
            this.tabTechnomancer.Controls.Add(this.lblLivingPersonaDeviceRating);
            this.tabTechnomancer.Controls.Add(this.lblLivingPersonaDeviceRatingLabel);
            this.tabTechnomancer.Controls.Add(this.lblFadingAttributesValue);
            this.tabTechnomancer.Controls.Add(this.lblFadingAttributes);
            this.tabTechnomancer.Controls.Add(this.lblFadingAttributesLabel);
            this.tabTechnomancer.Controls.Add(this.lblComplexFormSource);
            this.tabTechnomancer.Controls.Add(this.lblComplexFormSourceLabel);
            this.tabTechnomancer.Controls.Add(this.treComplexForms);
            this.tabTechnomancer.Controls.Add(this.cmdDeleteComplexForm);
            this.tabTechnomancer.Controls.Add(this.lblComplexForms);
            this.tabTechnomancer.Controls.Add(this.cmdAddSprite);
            this.tabTechnomancer.Controls.Add(this.lblSprites);
            this.tabTechnomancer.Controls.Add(this.panSprites);
            this.tabTechnomancer.Controls.Add(this.cmdAddComplexForm);
            this.tabTechnomancer.Location = new System.Drawing.Point(4, 22);
            this.tabTechnomancer.Name = "tabTechnomancer";
            this.tabTechnomancer.Size = new System.Drawing.Size(838, 611);
            this.tabTechnomancer.TabIndex = 3;
            this.tabTechnomancer.Tag = "Tab_Technomancer";
            this.tabTechnomancer.Text = "Sprites and Complex Forms";
            //
            // lblLivingPersonaFirewall
            //
            this.lblLivingPersonaFirewall.AutoSize = true;
            this.lblLivingPersonaFirewall.Location = new System.Drawing.Point(393, 317);
            this.lblLivingPersonaFirewall.Name = "lblLivingPersonaFirewall";
            this.lblLivingPersonaFirewall.Size = new System.Drawing.Size(19, 13);
            this.lblLivingPersonaFirewall.TabIndex = 161;
            this.lblLivingPersonaFirewall.Text = "[0]";
            //
            // lblLivingPersonaFirewallLabel
            //
            this.lblLivingPersonaFirewallLabel.AutoSize = true;
            this.lblLivingPersonaFirewallLabel.Location = new System.Drawing.Point(309, 317);
            this.lblLivingPersonaFirewallLabel.Name = "lblLivingPersonaFirewallLabel";
            this.lblLivingPersonaFirewallLabel.Size = new System.Drawing.Size(45, 13);
            this.lblLivingPersonaFirewallLabel.TabIndex = 160;
            this.lblLivingPersonaFirewallLabel.Tag = "Label_Firewall";
            this.lblLivingPersonaFirewallLabel.Text = "Firewall:";
            //
            // lblLivingPersonaDataProcessing
            //
            this.lblLivingPersonaDataProcessing.AutoSize = true;
            this.lblLivingPersonaDataProcessing.Location = new System.Drawing.Point(393, 297);
            this.lblLivingPersonaDataProcessing.Name = "lblLivingPersonaDataProcessing";
            this.lblLivingPersonaDataProcessing.Size = new System.Drawing.Size(19, 13);
            this.lblLivingPersonaDataProcessing.TabIndex = 159;
            this.lblLivingPersonaDataProcessing.Text = "[0]";
            //
            // lblLivingPersonaDataProcessingLabel
            //
            this.lblLivingPersonaDataProcessingLabel.AutoSize = true;
            this.lblLivingPersonaDataProcessingLabel.Location = new System.Drawing.Point(309, 297);
            this.lblLivingPersonaDataProcessingLabel.Name = "lblLivingPersonaDataProcessingLabel";
            this.lblLivingPersonaDataProcessingLabel.Size = new System.Drawing.Size(88, 13);
            this.lblLivingPersonaDataProcessingLabel.TabIndex = 158;
            this.lblLivingPersonaDataProcessingLabel.Tag = "Label_DataProcessing";
            this.lblLivingPersonaDataProcessingLabel.Text = "Data Processing:";
            //
            // lblLivingPersonaSleaze
            //
            this.lblLivingPersonaSleaze.AutoSize = true;
            this.lblLivingPersonaSleaze.Location = new System.Drawing.Point(393, 277);
            this.lblLivingPersonaSleaze.Name = "lblLivingPersonaSleaze";
            this.lblLivingPersonaSleaze.Size = new System.Drawing.Size(19, 13);
            this.lblLivingPersonaSleaze.TabIndex = 157;
            this.lblLivingPersonaSleaze.Text = "[0]";
            //
            // lblLivingPersonaSleazeLabel
            //
            this.lblLivingPersonaSleazeLabel.AutoSize = true;
            this.lblLivingPersonaSleazeLabel.Location = new System.Drawing.Point(309, 277);
            this.lblLivingPersonaSleazeLabel.Name = "lblLivingPersonaSleazeLabel";
            this.lblLivingPersonaSleazeLabel.Size = new System.Drawing.Size(42, 13);
            this.lblLivingPersonaSleazeLabel.TabIndex = 156;
            this.lblLivingPersonaSleazeLabel.Tag = "Label_Sleaze";
            this.lblLivingPersonaSleazeLabel.Text = "Sleaze:";
            //
            // lblLivingPersonaAttack
            //
            this.lblLivingPersonaAttack.AutoSize = true;
            this.lblLivingPersonaAttack.Location = new System.Drawing.Point(393, 257);
            this.lblLivingPersonaAttack.Name = "lblLivingPersonaAttack";
            this.lblLivingPersonaAttack.Size = new System.Drawing.Size(19, 13);
            this.lblLivingPersonaAttack.TabIndex = 155;
            this.lblLivingPersonaAttack.Text = "[0]";
            //
            // lblLivingPersonaAttackLabel
            //
            this.lblLivingPersonaAttackLabel.AutoSize = true;
            this.lblLivingPersonaAttackLabel.Location = new System.Drawing.Point(309, 257);
            this.lblLivingPersonaAttackLabel.Name = "lblLivingPersonaAttackLabel";
            this.lblLivingPersonaAttackLabel.Size = new System.Drawing.Size(41, 13);
            this.lblLivingPersonaAttackLabel.TabIndex = 154;
            this.lblLivingPersonaAttackLabel.Tag = "Label_Attack";
            this.lblLivingPersonaAttackLabel.Text = "Attack:";
            //
            // lblFV
            //
            this.lblFV.AutoSize = true;
            this.lblFV.Location = new System.Drawing.Point(423, 100);
            this.lblFV.Name = "lblFV";
            this.lblFV.Size = new System.Drawing.Size(39, 13);
            this.lblFV.TabIndex = 153;
            this.lblFV.Text = "[None]";
            //
            // lblFVLabel
            //
            this.lblFVLabel.AutoSize = true;
            this.lblFVLabel.Location = new System.Drawing.Point(309, 100);
            this.lblFVLabel.Name = "lblFVLabel";
            this.lblFVLabel.Size = new System.Drawing.Size(23, 13);
            this.lblFVLabel.TabIndex = 152;
            this.lblFVLabel.Tag = "Label_SelectProgram_FV";
            this.lblFVLabel.Text = "FV:";
            //
            // lblDuration
            //
            this.lblDuration.AutoSize = true;
            this.lblDuration.Location = new System.Drawing.Point(423, 77);
            this.lblDuration.Name = "lblDuration";
            this.lblDuration.Size = new System.Drawing.Size(39, 13);
            this.lblDuration.TabIndex = 151;
            this.lblDuration.Text = "[None]";
            //
            // lblDurationLabel
            //
            this.lblDurationLabel.AutoSize = true;
            this.lblDurationLabel.Location = new System.Drawing.Point(309, 77);
            this.lblDurationLabel.Name = "lblDurationLabel";
            this.lblDurationLabel.Size = new System.Drawing.Size(50, 13);
            this.lblDurationLabel.TabIndex = 150;
            this.lblDurationLabel.Tag = "Label_SelectProgram_Duration";
            this.lblDurationLabel.Text = "Duration:";
            //
            // lblTarget
            //
            this.lblTarget.AutoSize = true;
            this.lblTarget.Location = new System.Drawing.Point(423, 55);
            this.lblTarget.Name = "lblTarget";
            this.lblTarget.Size = new System.Drawing.Size(39, 13);
            this.lblTarget.TabIndex = 149;
            this.lblTarget.Text = "[None]";
            //
            // lblTargetLabel
            //
            this.lblTargetLabel.AutoSize = true;
            this.lblTargetLabel.Location = new System.Drawing.Point(309, 55);
            this.lblTargetLabel.Name = "lblTargetLabel";
            this.lblTargetLabel.Size = new System.Drawing.Size(41, 13);
            this.lblTargetLabel.TabIndex = 148;
            this.lblTargetLabel.Tag = "Label_SelectProgram_Target";
            this.lblTargetLabel.Text = "Target:";
            //
            // lblLivingPersonaLabel
            //
            this.lblLivingPersonaLabel.AutoSize = true;
            this.lblLivingPersonaLabel.Location = new System.Drawing.Point(309, 214);
            this.lblLivingPersonaLabel.Name = "lblLivingPersonaLabel";
            this.lblLivingPersonaLabel.Size = new System.Drawing.Size(77, 13);
            this.lblLivingPersonaLabel.TabIndex = 115;
            this.lblLivingPersonaLabel.Tag = "String_LivingPersona";
            this.lblLivingPersonaLabel.Text = "Living Persona";
            //
            // lblLivingPersonaDeviceRating
            //
            this.lblLivingPersonaDeviceRating.AutoSize = true;
            this.lblLivingPersonaDeviceRating.Location = new System.Drawing.Point(393, 237);
            this.lblLivingPersonaDeviceRating.Name = "lblLivingPersonaDeviceRating";
            this.lblLivingPersonaDeviceRating.Size = new System.Drawing.Size(19, 13);
            this.lblLivingPersonaDeviceRating.TabIndex = 106;
            this.lblLivingPersonaDeviceRating.Text = "[0]";
            //
            // lblLivingPersonaDeviceRatingLabel
            //
            this.lblLivingPersonaDeviceRatingLabel.AutoSize = true;
            this.lblLivingPersonaDeviceRatingLabel.Location = new System.Drawing.Point(309, 237);
            this.lblLivingPersonaDeviceRatingLabel.Name = "lblLivingPersonaDeviceRatingLabel";
            this.lblLivingPersonaDeviceRatingLabel.Size = new System.Drawing.Size(78, 13);
            this.lblLivingPersonaDeviceRatingLabel.TabIndex = 105;
            this.lblLivingPersonaDeviceRatingLabel.Tag = "Label_DeviceRating";
            this.lblLivingPersonaDeviceRatingLabel.Text = "Device Rating:";
            //
            // lblFadingAttributesValue
            //
            this.lblFadingAttributesValue.AutoSize = true;
            this.lblFadingAttributesValue.Location = new System.Drawing.Point(495, 169);
            this.lblFadingAttributesValue.Name = "lblFadingAttributesValue";
            this.lblFadingAttributesValue.Size = new System.Drawing.Size(37, 13);
            this.lblFadingAttributesValue.TabIndex = 98;
            this.lblFadingAttributesValue.Text = "[Total]";
            //
            // lblFadingAttributes
            //
            this.lblFadingAttributes.AutoSize = true;
            this.lblFadingAttributes.Location = new System.Drawing.Point(404, 169);
            this.lblFadingAttributes.Name = "lblFadingAttributes";
            this.lblFadingAttributes.Size = new System.Drawing.Size(57, 13);
            this.lblFadingAttributes.TabIndex = 97;
            this.lblFadingAttributes.Text = "[Attributes]";
            //
            // lblFadingAttributesLabel
            //
            this.lblFadingAttributesLabel.AutoSize = true;
            this.lblFadingAttributesLabel.Location = new System.Drawing.Point(309, 169);
            this.lblFadingAttributesLabel.Name = "lblFadingAttributesLabel";
            this.lblFadingAttributesLabel.Size = new System.Drawing.Size(96, 13);
            this.lblFadingAttributesLabel.TabIndex = 96;
            this.lblFadingAttributesLabel.Tag = "Label_ResistFading";
            this.lblFadingAttributesLabel.Text = "Resist Fading with:";
            //
            // lblComplexFormSource
            //
            this.lblComplexFormSource.AutoSize = true;
            this.lblComplexFormSource.Location = new System.Drawing.Point(388, 123);
            this.lblComplexFormSource.Name = "lblComplexFormSource";
            this.lblComplexFormSource.Size = new System.Drawing.Size(47, 13);
            this.lblComplexFormSource.TabIndex = 90;
            this.lblComplexFormSource.Text = "[Source]";
            this.lblComplexFormSource.Click += new System.EventHandler(this.lblComplexFormSource_Click);
            //
            // lblComplexFormSourceLabel
            //
            this.lblComplexFormSourceLabel.AutoSize = true;
            this.lblComplexFormSourceLabel.Location = new System.Drawing.Point(309, 123);
            this.lblComplexFormSourceLabel.Name = "lblComplexFormSourceLabel";
            this.lblComplexFormSourceLabel.Size = new System.Drawing.Size(44, 13);
            this.lblComplexFormSourceLabel.TabIndex = 89;
            this.lblComplexFormSourceLabel.Tag = "Label_Source";
            this.lblComplexFormSourceLabel.Text = "Source:";
            //
            // treComplexForms
            //
            this.treComplexForms.HideSelection = false;
            this.treComplexForms.Location = new System.Drawing.Point(8, 54);
            this.treComplexForms.Name = "treComplexForms";
            treeNode15.Name = "nodProgramAdvancedRoot";
            treeNode15.Tag = "Node_SelectedAdvancedComplexForms";
            treeNode15.Text = "Selected Complex Forms";
            this.treComplexForms.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            treeNode15});
            this.treComplexForms.ShowNodeToolTips = true;
            this.treComplexForms.ShowRootLines = false;
            this.treComplexForms.Size = new System.Drawing.Size(295, 333);
            this.treComplexForms.TabIndex = 71;
            this.treComplexForms.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treComplexForms_AfterSelect);
            this.treComplexForms.KeyDown += new System.Windows.Forms.KeyEventHandler(this.treComplexForms_KeyDown);
            this.treComplexForms.MouseDown += new System.Windows.Forms.MouseEventHandler(this.TreeView_MouseDown);
            //
            // cmdDeleteComplexForm
            //
            this.cmdDeleteComplexForm.AutoSize = true;
            this.cmdDeleteComplexForm.Location = new System.Drawing.Point(137, 25);
            this.cmdDeleteComplexForm.Name = "cmdDeleteComplexForm";
            this.cmdDeleteComplexForm.Size = new System.Drawing.Size(80, 23);
            this.cmdDeleteComplexForm.TabIndex = 31;
            this.cmdDeleteComplexForm.Tag = "String_Delete";
            this.cmdDeleteComplexForm.Text = "Delete";
            this.cmdDeleteComplexForm.UseVisualStyleBackColor = true;
            this.cmdDeleteComplexForm.Click += new System.EventHandler(this.cmdDeleteComplexForm_Click);
            //
            // cmdAddSprite
            //
            this.cmdAddSprite.Location = new System.Drawing.Point(8, 416);
            this.cmdAddSprite.Name = "cmdAddSprite";
            this.cmdAddSprite.Size = new System.Drawing.Size(75, 23);
            this.cmdAddSprite.TabIndex = 26;
            this.cmdAddSprite.Tag = "Button_AddSprite";
            this.cmdAddSprite.Text = "&Add Sprite";
            this.cmdAddSprite.UseVisualStyleBackColor = true;
            this.cmdAddSprite.Click += new System.EventHandler(this.cmdAddSprite_Click);
            //
            // panSprites
            //
            this.panSprites.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)));
            this.panSprites.AutoScroll = true;
            this.panSprites.Location = new System.Drawing.Point(8, 445);
            this.panSprites.Name = "panSprites";
            this.panSprites.Size = new System.Drawing.Size(827, 163);
            this.panSprites.TabIndex = 25;
            //
            // cmdAddComplexForm
            //
            this.cmdAddComplexForm.AutoSize = true;
            this.cmdAddComplexForm.ContextMenuStrip = this.cmsComplexForm;
            this.cmdAddComplexForm.Location = new System.Drawing.Point(8, 25);
            this.cmdAddComplexForm.Name = "cmdAddComplexForm";
            this.cmdAddComplexForm.Size = new System.Drawing.Size(123, 23);
            this.cmdAddComplexForm.SplitMenuStrip = this.cmsComplexForm;
            this.cmdAddComplexForm.TabIndex = 147;
            this.cmdAddComplexForm.Tag = "Button_AddComplexForm";
            this.cmdAddComplexForm.Text = "Add Complex Form";
            this.cmdAddComplexForm.UseVisualStyleBackColor = true;
            this.cmdAddComplexForm.Click += new System.EventHandler(this.cmdAddComplexForm_Click);
            //
            // tabAdvancedPrograms
            //
            this.tabAdvancedPrograms.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.tabAdvancedPrograms.Controls.Add(this.cmdAddAIProgram);
            this.tabAdvancedPrograms.Controls.Add(this.lblAIProgramsRequires);
            this.tabAdvancedPrograms.Controls.Add(this.lblAIProgramsRequiresLabel);
            this.tabAdvancedPrograms.Controls.Add(this.lblAIProgramsSource);
            this.tabAdvancedPrograms.Controls.Add(this.lblAIProgramsSourceLabel);
            this.tabAdvancedPrograms.Controls.Add(this.treAIPrograms);
            this.tabAdvancedPrograms.Controls.Add(this.cmdDeleteAIProgram);
            this.tabAdvancedPrograms.Controls.Add(this.lblAIProgramsAdvancedPrograms);
            this.tabAdvancedPrograms.Location = new System.Drawing.Point(4, 22);
            this.tabAdvancedPrograms.Name = "tabAdvancedPrograms";
            this.tabAdvancedPrograms.Size = new System.Drawing.Size(838, 611);
            this.tabAdvancedPrograms.TabIndex = 15;
            this.tabAdvancedPrograms.Tag = "Tab_AdvancedPrograms";
            this.tabAdvancedPrograms.Text = "Advanced Programs";
            //
            // cmdAddAIProgram
            //
            this.cmdAddAIProgram.AutoSize = true;
            this.cmdAddAIProgram.Location = new System.Drawing.Point(11, 25);
            this.cmdAddAIProgram.Name = "cmdAddAIProgram";
            this.cmdAddAIProgram.Size = new System.Drawing.Size(80, 23);
            this.cmdAddAIProgram.TabIndex = 150;
            this.cmdAddAIProgram.Tag = "Button_AddProgram";
            this.cmdAddAIProgram.Text = "Add Program";
            this.cmdAddAIProgram.UseVisualStyleBackColor = true;
            this.cmdAddAIProgram.Click += new System.EventHandler(this.cmdAddAIProgram_Click);
            //
            // lblAIProgramsRequires
            //
            this.lblAIProgramsRequires.AutoSize = true;
            this.lblAIProgramsRequires.Location = new System.Drawing.Point(423, 55);
            this.lblAIProgramsRequires.Name = "lblAIProgramsRequires";
            this.lblAIProgramsRequires.Size = new System.Drawing.Size(39, 13);
            this.lblAIProgramsRequires.TabIndex = 149;
            this.lblAIProgramsRequires.Tag = "";
            this.lblAIProgramsRequires.Text = "[None]";
            //
            // lblAIProgramsRequiresLabel
            //
            this.lblAIProgramsRequiresLabel.AutoSize = true;
            this.lblAIProgramsRequiresLabel.Location = new System.Drawing.Point(312, 55);
            this.lblAIProgramsRequiresLabel.Name = "lblAIProgramsRequiresLabel";
            this.lblAIProgramsRequiresLabel.Size = new System.Drawing.Size(52, 13);
            this.lblAIProgramsRequiresLabel.TabIndex = 148;
            this.lblAIProgramsRequiresLabel.Tag = "String_Requires";
            this.lblAIProgramsRequiresLabel.Text = "Requires:";
            //
            // lblAIProgramsSource
            //
            this.lblAIProgramsSource.AutoSize = true;
            this.lblAIProgramsSource.Location = new System.Drawing.Point(424, 120);
            this.lblAIProgramsSource.Name = "lblAIProgramsSource";
            this.lblAIProgramsSource.Size = new System.Drawing.Size(47, 13);
            this.lblAIProgramsSource.TabIndex = 90;
            this.lblAIProgramsSource.Text = "[Source]";
            //
            // lblAIProgramsSourceLabel
            //
            this.lblAIProgramsSourceLabel.AutoSize = true;
            this.lblAIProgramsSourceLabel.Location = new System.Drawing.Point(312, 120);
            this.lblAIProgramsSourceLabel.Name = "lblAIProgramsSourceLabel";
            this.lblAIProgramsSourceLabel.Size = new System.Drawing.Size(44, 13);
            this.lblAIProgramsSourceLabel.TabIndex = 89;
            this.lblAIProgramsSourceLabel.Tag = "Label_Source";
            this.lblAIProgramsSourceLabel.Text = "Source:";
            //
            // treAIPrograms
            //
            this.treAIPrograms.HideSelection = false;
            this.treAIPrograms.Location = new System.Drawing.Point(8, 54);
            this.treAIPrograms.Name = "treAIPrograms";
            treeNode16.Name = "nodAIProgramsRoot";
            treeNode16.Tag = "Node_SelectedAIPrograms";
            treeNode16.Text = "Selected AI Programs and Advanced Programs";
            this.treAIPrograms.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            treeNode16});
            this.treAIPrograms.ShowNodeToolTips = true;
            this.treAIPrograms.ShowRootLines = false;
            this.treAIPrograms.Size = new System.Drawing.Size(295, 554);
            this.treAIPrograms.TabIndex = 71;
            this.treAIPrograms.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treAIPrograms_AfterSelect);
            this.treAIPrograms.KeyDown += new System.Windows.Forms.KeyEventHandler(this.treAIPrograms_KeyDown);
            //
            // cmdDeleteAIProgram
            //
            this.cmdDeleteAIProgram.AutoSize = true;
            this.cmdDeleteAIProgram.Location = new System.Drawing.Point(97, 25);
            this.cmdDeleteAIProgram.Name = "cmdDeleteAIProgram";
            this.cmdDeleteAIProgram.Size = new System.Drawing.Size(80, 23);
            this.cmdDeleteAIProgram.TabIndex = 31;
            this.cmdDeleteAIProgram.Tag = "String_Delete";
            this.cmdDeleteAIProgram.Text = "Delete";
            this.cmdDeleteAIProgram.UseVisualStyleBackColor = true;
            this.cmdDeleteAIProgram.Click += new System.EventHandler(this.cmdDeleteAIProgram_Click);
            //
            // tabCritter
            //
            this.tabCritter.BackColor = System.Drawing.SystemColors.Control;
            this.tabCritter.Controls.Add(this.chkCritterPowerCount);
            this.tabCritter.Controls.Add(this.lblCritterPowerPointCost);
            this.tabCritter.Controls.Add(this.lblCritterPowerPointCostLabel);
            this.tabCritter.Controls.Add(this.lblCritterPowerPoints);
            this.tabCritter.Controls.Add(this.lblCritterPowerPointsLabel);
            this.tabCritter.Controls.Add(this.cmdDeleteCritterPower);
            this.tabCritter.Controls.Add(this.cmdAddCritterPower);
            this.tabCritter.Controls.Add(this.lblCritterPowerSource);
            this.tabCritter.Controls.Add(this.lblCritterPowerSourceLabel);
            this.tabCritter.Controls.Add(this.lblCritterPowerDuration);
            this.tabCritter.Controls.Add(this.lblCritterPowerDurationLabel);
            this.tabCritter.Controls.Add(this.lblCritterPowerRange);
            this.tabCritter.Controls.Add(this.lblCritterPowerRangeLabel);
            this.tabCritter.Controls.Add(this.lblCritterPowerAction);
            this.tabCritter.Controls.Add(this.lblCritterPowerActionLabel);
            this.tabCritter.Controls.Add(this.lblCritterPowerType);
            this.tabCritter.Controls.Add(this.lblCritterPowerTypeLabel);
            this.tabCritter.Controls.Add(this.lblCritterPowerCategory);
            this.tabCritter.Controls.Add(this.lblCritterPowerCategoryLabel);
            this.tabCritter.Controls.Add(this.lblCritterPowerName);
            this.tabCritter.Controls.Add(this.lblCritterPowerNameLabel);
            this.tabCritter.Controls.Add(this.treCritterPowers);
            this.tabCritter.Location = new System.Drawing.Point(4, 22);
            this.tabCritter.Name = "tabCritter";
            this.tabCritter.Size = new System.Drawing.Size(838, 611);
            this.tabCritter.TabIndex = 11;
            this.tabCritter.Tag = "Tab_Critter";
            this.tabCritter.Text = "Critter Powers";
            //
            // chkCritterPowerCount
            //
            this.chkCritterPowerCount.AutoSize = true;
            this.chkCritterPowerCount.Location = new System.Drawing.Point(350, 219);
            this.chkCritterPowerCount.Name = "chkCritterPowerCount";
            this.chkCritterPowerCount.Size = new System.Drawing.Size(182, 17);
            this.chkCritterPowerCount.TabIndex = 21;
            this.chkCritterPowerCount.Tag = "Checkbox_CritterPowerCount";
            this.chkCritterPowerCount.Text = "Counts towards Critter Power limit";
            this.chkCritterPowerCount.UseVisualStyleBackColor = true;
            this.chkCritterPowerCount.CheckedChanged += new System.EventHandler(this.chkCritterPowerCount_CheckedChanged);
            //
            // lblCritterPowerPointCost
            //
            this.lblCritterPowerPointCost.AutoSize = true;
            this.lblCritterPowerPointCost.Location = new System.Drawing.Point(413, 193);
            this.lblCritterPowerPointCost.Name = "lblCritterPowerPointCost";
            this.lblCritterPowerPointCost.Size = new System.Drawing.Size(75, 13);
            this.lblCritterPowerPointCost.TabIndex = 20;
            this.lblCritterPowerPointCost.Text = "[Power Points]";
            this.lblCritterPowerPointCost.Visible = false;
            //
            // lblCritterPowerPointCostLabel
            //
            this.lblCritterPowerPointCostLabel.AutoSize = true;
            this.lblCritterPowerPointCostLabel.Location = new System.Drawing.Point(347, 193);
            this.lblCritterPowerPointCostLabel.Name = "lblCritterPowerPointCostLabel";
            this.lblCritterPowerPointCostLabel.Size = new System.Drawing.Size(39, 13);
            this.lblCritterPowerPointCostLabel.TabIndex = 19;
            this.lblCritterPowerPointCostLabel.Tag = "Label_Points";
            this.lblCritterPowerPointCostLabel.Text = "Points:";
            this.lblCritterPowerPointCostLabel.Visible = false;
            //
            // lblCritterPowerPoints
            //
            this.lblCritterPowerPoints.AutoSize = true;
            this.lblCritterPowerPoints.Location = new System.Drawing.Point(273, 11);
            this.lblCritterPowerPoints.Name = "lblCritterPowerPoints";
            this.lblCritterPowerPoints.Size = new System.Drawing.Size(76, 13);
            this.lblCritterPowerPoints.TabIndex = 18;
            this.lblCritterPowerPoints.Text = "0 (0 remaining)";
            this.lblCritterPowerPoints.Visible = false;
            //
            // lblCritterPowerPointsLabel
            //
            this.lblCritterPowerPointsLabel.AutoSize = true;
            this.lblCritterPowerPointsLabel.Location = new System.Drawing.Point(195, 11);
            this.lblCritterPowerPointsLabel.Name = "lblCritterPowerPointsLabel";
            this.lblCritterPowerPointsLabel.Size = new System.Drawing.Size(72, 13);
            this.lblCritterPowerPointsLabel.TabIndex = 17;
            this.lblCritterPowerPointsLabel.Tag = "Label_PowerPoints";
            this.lblCritterPowerPointsLabel.Text = "Power Points:";
            this.lblCritterPowerPointsLabel.Visible = false;
            //
            // cmdDeleteCritterPower
            //
            this.cmdDeleteCritterPower.AutoSize = true;
            this.cmdDeleteCritterPower.Location = new System.Drawing.Point(94, 6);
            this.cmdDeleteCritterPower.Name = "cmdDeleteCritterPower";
            this.cmdDeleteCritterPower.Size = new System.Drawing.Size(80, 23);
            this.cmdDeleteCritterPower.TabIndex = 16;
            this.cmdDeleteCritterPower.Tag = "String_Delete";
            this.cmdDeleteCritterPower.Text = "Delete";
            this.cmdDeleteCritterPower.UseVisualStyleBackColor = true;
            this.cmdDeleteCritterPower.Click += new System.EventHandler(this.cmdDeleteCritterPower_Click);
            //
            // cmdAddCritterPower
            //
            this.cmdAddCritterPower.AutoSize = true;
            this.cmdAddCritterPower.Location = new System.Drawing.Point(8, 6);
            this.cmdAddCritterPower.Name = "cmdAddCritterPower";
            this.cmdAddCritterPower.Size = new System.Drawing.Size(80, 23);
            this.cmdAddCritterPower.TabIndex = 15;
            this.cmdAddCritterPower.Tag = "Button_AddCritterPower";
            this.cmdAddCritterPower.Text = "&Add Power";
            this.cmdAddCritterPower.UseVisualStyleBackColor = true;
            this.cmdAddCritterPower.Click += new System.EventHandler(this.cmdAddCritterPower_Click);
            //
            // lblCritterPowerSource
            //
            this.lblCritterPowerSource.AutoSize = true;
            this.lblCritterPowerSource.Location = new System.Drawing.Point(413, 170);
            this.lblCritterPowerSource.Name = "lblCritterPowerSource";
            this.lblCritterPowerSource.Size = new System.Drawing.Size(47, 13);
            this.lblCritterPowerSource.TabIndex = 14;
            this.lblCritterPowerSource.Text = "[Source]";
            this.lblCritterPowerSource.Click += new System.EventHandler(this.lblCritterPowerSource_Click);
            //
            // lblCritterPowerSourceLabel
            //
            this.lblCritterPowerSourceLabel.AutoSize = true;
            this.lblCritterPowerSourceLabel.Location = new System.Drawing.Point(347, 170);
            this.lblCritterPowerSourceLabel.Name = "lblCritterPowerSourceLabel";
            this.lblCritterPowerSourceLabel.Size = new System.Drawing.Size(44, 13);
            this.lblCritterPowerSourceLabel.TabIndex = 13;
            this.lblCritterPowerSourceLabel.Tag = "Label_Source";
            this.lblCritterPowerSourceLabel.Text = "Source:";
            //
            // lblCritterPowerDuration
            //
            this.lblCritterPowerDuration.AutoSize = true;
            this.lblCritterPowerDuration.Location = new System.Drawing.Point(413, 147);
            this.lblCritterPowerDuration.Name = "lblCritterPowerDuration";
            this.lblCritterPowerDuration.Size = new System.Drawing.Size(53, 13);
            this.lblCritterPowerDuration.TabIndex = 12;
            this.lblCritterPowerDuration.Text = "[Duration]";
            //
            // lblCritterPowerDurationLabel
            //
            this.lblCritterPowerDurationLabel.AutoSize = true;
            this.lblCritterPowerDurationLabel.Location = new System.Drawing.Point(347, 147);
            this.lblCritterPowerDurationLabel.Name = "lblCritterPowerDurationLabel";
            this.lblCritterPowerDurationLabel.Size = new System.Drawing.Size(50, 13);
            this.lblCritterPowerDurationLabel.TabIndex = 11;
            this.lblCritterPowerDurationLabel.Tag = "Label_Duration";
            this.lblCritterPowerDurationLabel.Text = "Duration:";
            //
            // lblCritterPowerRange
            //
            this.lblCritterPowerRange.AutoSize = true;
            this.lblCritterPowerRange.Location = new System.Drawing.Point(413, 124);
            this.lblCritterPowerRange.Name = "lblCritterPowerRange";
            this.lblCritterPowerRange.Size = new System.Drawing.Size(45, 13);
            this.lblCritterPowerRange.TabIndex = 10;
            this.lblCritterPowerRange.Text = "[Range]";
            //
            // lblCritterPowerRangeLabel
            //
            this.lblCritterPowerRangeLabel.AutoSize = true;
            this.lblCritterPowerRangeLabel.Location = new System.Drawing.Point(347, 124);
            this.lblCritterPowerRangeLabel.Name = "lblCritterPowerRangeLabel";
            this.lblCritterPowerRangeLabel.Size = new System.Drawing.Size(42, 13);
            this.lblCritterPowerRangeLabel.TabIndex = 9;
            this.lblCritterPowerRangeLabel.Tag = "Label_Range";
            this.lblCritterPowerRangeLabel.Text = "Range:";
            //
            // lblCritterPowerAction
            //
            this.lblCritterPowerAction.AutoSize = true;
            this.lblCritterPowerAction.Location = new System.Drawing.Point(413, 101);
            this.lblCritterPowerAction.Name = "lblCritterPowerAction";
            this.lblCritterPowerAction.Size = new System.Drawing.Size(43, 13);
            this.lblCritterPowerAction.TabIndex = 8;
            this.lblCritterPowerAction.Text = "[Action]";
            //
            // lblCritterPowerActionLabel
            //
            this.lblCritterPowerActionLabel.AutoSize = true;
            this.lblCritterPowerActionLabel.Location = new System.Drawing.Point(347, 101);
            this.lblCritterPowerActionLabel.Name = "lblCritterPowerActionLabel";
            this.lblCritterPowerActionLabel.Size = new System.Drawing.Size(40, 13);
            this.lblCritterPowerActionLabel.TabIndex = 7;
            this.lblCritterPowerActionLabel.Tag = "Label_Action";
            this.lblCritterPowerActionLabel.Text = "Action:";
            //
            // lblCritterPowerType
            //
            this.lblCritterPowerType.AutoSize = true;
            this.lblCritterPowerType.Location = new System.Drawing.Point(413, 78);
            this.lblCritterPowerType.Name = "lblCritterPowerType";
            this.lblCritterPowerType.Size = new System.Drawing.Size(37, 13);
            this.lblCritterPowerType.TabIndex = 6;
            this.lblCritterPowerType.Text = "[Type]";
            //
            // lblCritterPowerTypeLabel
            //
            this.lblCritterPowerTypeLabel.AutoSize = true;
            this.lblCritterPowerTypeLabel.Location = new System.Drawing.Point(347, 78);
            this.lblCritterPowerTypeLabel.Name = "lblCritterPowerTypeLabel";
            this.lblCritterPowerTypeLabel.Size = new System.Drawing.Size(34, 13);
            this.lblCritterPowerTypeLabel.TabIndex = 5;
            this.lblCritterPowerTypeLabel.Tag = "Label_Type";
            this.lblCritterPowerTypeLabel.Text = "Type:";
            //
            // lblCritterPowerCategory
            //
            this.lblCritterPowerCategory.AutoSize = true;
            this.lblCritterPowerCategory.Location = new System.Drawing.Point(413, 55);
            this.lblCritterPowerCategory.Name = "lblCritterPowerCategory";
            this.lblCritterPowerCategory.Size = new System.Drawing.Size(55, 13);
            this.lblCritterPowerCategory.TabIndex = 4;
            this.lblCritterPowerCategory.Text = "[Category]";
            //
            // lblCritterPowerCategoryLabel
            //
            this.lblCritterPowerCategoryLabel.AutoSize = true;
            this.lblCritterPowerCategoryLabel.Location = new System.Drawing.Point(347, 55);
            this.lblCritterPowerCategoryLabel.Name = "lblCritterPowerCategoryLabel";
            this.lblCritterPowerCategoryLabel.Size = new System.Drawing.Size(52, 13);
            this.lblCritterPowerCategoryLabel.TabIndex = 3;
            this.lblCritterPowerCategoryLabel.Tag = "Label_Category";
            this.lblCritterPowerCategoryLabel.Text = "Category:";
            //
            // lblCritterPowerName
            //
            this.lblCritterPowerName.AutoSize = true;
            this.lblCritterPowerName.Location = new System.Drawing.Point(413, 32);
            this.lblCritterPowerName.Name = "lblCritterPowerName";
            this.lblCritterPowerName.Size = new System.Drawing.Size(41, 13);
            this.lblCritterPowerName.TabIndex = 2;
            this.lblCritterPowerName.Text = "[Name]";
            //
            // lblCritterPowerNameLabel
            //
            this.lblCritterPowerNameLabel.AutoSize = true;
            this.lblCritterPowerNameLabel.Location = new System.Drawing.Point(347, 32);
            this.lblCritterPowerNameLabel.Name = "lblCritterPowerNameLabel";
            this.lblCritterPowerNameLabel.Size = new System.Drawing.Size(38, 13);
            this.lblCritterPowerNameLabel.TabIndex = 1;
            this.lblCritterPowerNameLabel.Tag = "Label_Name";
            this.lblCritterPowerNameLabel.Text = "Name:";
            //
            // treCritterPowers
            //
            this.treCritterPowers.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)));
            this.treCritterPowers.HideSelection = false;
            this.treCritterPowers.Location = new System.Drawing.Point(8, 32);
            this.treCritterPowers.Name = "treCritterPowers";
            treeNode17.Name = "nodCritterPowerRoot";
            treeNode17.Tag = "Node_CritterPowers";
            treeNode17.Text = "Critter Powers";
            treeNode18.Name = "nodCritterWeaknessRoot";
            treeNode18.Tag = "Node_CritterWeaknesses";
            treeNode18.Text = "Weaknesses";
            this.treCritterPowers.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            treeNode17,
            treeNode18});
            this.treCritterPowers.ShowNodeToolTips = true;
            this.treCritterPowers.ShowPlusMinus = false;
            this.treCritterPowers.ShowRootLines = false;
            this.treCritterPowers.Size = new System.Drawing.Size(333, 576);
            this.treCritterPowers.TabIndex = 0;
            this.treCritterPowers.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treCritterPowers_AfterSelect);
            this.treCritterPowers.KeyDown += new System.Windows.Forms.KeyEventHandler(this.treCritterPowers_KeyDown);
            this.treCritterPowers.MouseDown += new System.Windows.Forms.MouseEventHandler(this.TreeView_MouseDown);
            //
            // tabInitiation
            //
            this.tabInitiation.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.tabInitiation.Controls.Add(this.chkInitiationSchooling);
            this.tabInitiation.Controls.Add(this.lblMetamagicSource);
            this.tabInitiation.Controls.Add(this.lblMetamagicSourceLabel);
            this.tabInitiation.Controls.Add(this.chkInitiationOrdeal);
            this.tabInitiation.Controls.Add(this.chkInitiationGroup);
            this.tabInitiation.Controls.Add(this.treMetamagic);
            this.tabInitiation.Controls.Add(this.cmdAddMetamagic);
            this.tabInitiation.Location = new System.Drawing.Point(4, 22);
            this.tabInitiation.Name = "tabInitiation";
            this.tabInitiation.Padding = new System.Windows.Forms.Padding(3);
            this.tabInitiation.Size = new System.Drawing.Size(838, 611);
            this.tabInitiation.TabIndex = 10;
            this.tabInitiation.Tag = "Tab_Initiation";
            this.tabInitiation.Text = "Initiation & Submersion";
            //
            // chkInitiationSchooling
            //
            this.chkInitiationSchooling.AutoSize = true;
            this.chkInitiationSchooling.Location = new System.Drawing.Point(8, 52);
            this.chkInitiationSchooling.Name = "chkInitiationSchooling";
            this.chkInitiationSchooling.Size = new System.Drawing.Size(105, 17);
            this.chkInitiationSchooling.TabIndex = 129;
            this.chkInitiationSchooling.Tag = "Checkbox_InitiationSchooling";
            this.chkInitiationSchooling.Text = "Schooling (-10%)";
            this.chkInitiationSchooling.UseVisualStyleBackColor = true;
            this.chkInitiationSchooling.CheckedChanged += new System.EventHandler(this.chkInitiationSchooling_CheckedChanged);
            //
            // lblMetamagicSource
            //
            this.lblMetamagicSource.AutoSize = true;
            this.lblMetamagicSource.Location = new System.Drawing.Point(434, 75);
            this.lblMetamagicSource.Name = "lblMetamagicSource";
            this.lblMetamagicSource.Size = new System.Drawing.Size(47, 13);
            this.lblMetamagicSource.TabIndex = 109;
            this.lblMetamagicSource.Text = "[Source]";
            this.lblMetamagicSource.Click += new System.EventHandler(this.lblMetamagicSource_Click);
            //
            // lblMetamagicSourceLabel
            //
            this.lblMetamagicSourceLabel.AutoSize = true;
            this.lblMetamagicSourceLabel.Location = new System.Drawing.Point(365, 75);
            this.lblMetamagicSourceLabel.Name = "lblMetamagicSourceLabel";
            this.lblMetamagicSourceLabel.Size = new System.Drawing.Size(44, 13);
            this.lblMetamagicSourceLabel.TabIndex = 108;
            this.lblMetamagicSourceLabel.Tag = "Label_Source";
            this.lblMetamagicSourceLabel.Text = "Source:";
            //
            // chkInitiationOrdeal
            //
            this.chkInitiationOrdeal.AutoSize = true;
            this.chkInitiationOrdeal.Location = new System.Drawing.Point(8, 8);
            this.chkInitiationOrdeal.Name = "chkInitiationOrdeal";
            this.chkInitiationOrdeal.Size = new System.Drawing.Size(131, 17);
            this.chkInitiationOrdeal.TabIndex = 102;
            this.chkInitiationOrdeal.Tag = "Checkbox_InitiationOrdeal";
            this.chkInitiationOrdeal.Text = "Initiatory Ordeal (-10%)";
            this.chkInitiationOrdeal.UseVisualStyleBackColor = true;
            this.chkInitiationOrdeal.CheckedChanged += new System.EventHandler(this.chkInitiationOrdeal_CheckedChanged);
            //
            // chkInitiationGroup
            //
            this.chkInitiationGroup.AutoSize = true;
            this.chkInitiationGroup.Location = new System.Drawing.Point(8, 31);
            this.chkInitiationGroup.Name = "chkInitiationGroup";
            this.chkInitiationGroup.Size = new System.Drawing.Size(129, 17);
            this.chkInitiationGroup.TabIndex = 101;
            this.chkInitiationGroup.Tag = "Checkbox_GroupInitiation";
            this.chkInitiationGroup.Text = "Group Initiation (-10%)";
            this.chkInitiationGroup.UseVisualStyleBackColor = true;
            this.chkInitiationGroup.CheckedChanged += new System.EventHandler(this.chkInitiationGroup_CheckedChanged);
            //
            // treMetamagic
            //
            this.treMetamagic.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)));
            this.treMetamagic.HideSelection = false;
            this.treMetamagic.Location = new System.Drawing.Point(8, 75);
            this.treMetamagic.Name = "treMetamagic";
            this.treMetamagic.ShowLines = false;
            this.treMetamagic.ShowNodeToolTips = true;
            this.treMetamagic.ShowPlusMinus = false;
            this.treMetamagic.ShowRootLines = false;
            this.treMetamagic.Size = new System.Drawing.Size(351, 530);
            this.treMetamagic.TabIndex = 96;
            this.treMetamagic.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treMetamagic_AfterSelect);
            this.treMetamagic.KeyDown += new System.Windows.Forms.KeyEventHandler(this.treMetamagic_KeyDown);
            this.treMetamagic.MouseDown += new System.Windows.Forms.MouseEventHandler(this.TreeView_MouseDown);
            //
            // cmdAddMetamagic
            //
            this.cmdAddMetamagic.AutoSize = true;
            this.cmdAddMetamagic.Location = new System.Drawing.Point(257, 6);
            this.cmdAddMetamagic.Name = "cmdAddMetamagic";
            this.cmdAddMetamagic.Size = new System.Drawing.Size(102, 23);
            this.cmdAddMetamagic.TabIndex = 93;
            this.cmdAddMetamagic.Tag = "Button_AddInitiateGrade";
            this.cmdAddMetamagic.Text = "&Add Initiate Grade";
            this.cmdAddMetamagic.UseVisualStyleBackColor = true;
            this.cmdAddMetamagic.Click += new System.EventHandler(this.cmdAddMetamagic_Click);
            //
            // tabCyberware
            //
            this.tabCyberware.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.tabCyberware.Controls.Add(this.chkPrototypeTranshuman);
            this.tabCyberware.Controls.Add(this.lblCyberlimbSTR);
            this.tabCyberware.Controls.Add(this.lblCyberlimbAGI);
            this.tabCyberware.Controls.Add(this.lblCyberlimbSTRLabel);
            this.tabCyberware.Controls.Add(this.lblCyberlimbAGILabel);
            this.tabCyberware.Controls.Add(this.lblCyberFirewall);
            this.tabCyberware.Controls.Add(this.lblCyberFirewallLabel);
            this.tabCyberware.Controls.Add(this.lblCyberDataProcessing);
            this.tabCyberware.Controls.Add(this.lblCyberDataProcessingLabel);
            this.tabCyberware.Controls.Add(this.lblCyberSleaze);
            this.tabCyberware.Controls.Add(this.lblCyberSleazeLabel);
            this.tabCyberware.Controls.Add(this.lblCyberAttack);
            this.tabCyberware.Controls.Add(this.lblCyberAttackLabel);
            this.tabCyberware.Controls.Add(this.lblCyberDeviceRating);
            this.tabCyberware.Controls.Add(this.lblCyberDeviceRatingLabel);
            this.tabCyberware.Controls.Add(this.lblEssenceHoleESS);
            this.tabCyberware.Controls.Add(this.lblEssenceHoleESSLabel);
            this.tabCyberware.Controls.Add(this.label1);
            this.tabCyberware.Controls.Add(this.lblBiowareESS);
            this.tabCyberware.Controls.Add(this.lblCyberwareESS);
            this.tabCyberware.Controls.Add(this.lblBiowareESSLabel);
            this.tabCyberware.Controls.Add(this.lblCyberwareESSLabel);
            this.tabCyberware.Controls.Add(this.lblCyberwareSource);
            this.tabCyberware.Controls.Add(this.lblCyberwareSourceLabel);
            this.tabCyberware.Controls.Add(this.cmdAddBioware);
            this.tabCyberware.Controls.Add(this.cmdDeleteCyberware);
            this.tabCyberware.Controls.Add(this.nudCyberwareRating);
            this.tabCyberware.Controls.Add(this.cboCyberwareGrade);
            this.tabCyberware.Controls.Add(this.lblCyberwareRatingLabel);
            this.tabCyberware.Controls.Add(this.lblCyberwareCost);
            this.tabCyberware.Controls.Add(this.lblCyberwareCostLabel);
            this.tabCyberware.Controls.Add(this.lblCyberwareAvail);
            this.tabCyberware.Controls.Add(this.lblCyberwareAvailLabel);
            this.tabCyberware.Controls.Add(this.lblCyberwareGradeLabel);
            this.tabCyberware.Controls.Add(this.lblCyberwareCapacity);
            this.tabCyberware.Controls.Add(this.lblCyberwareCapacityLabel);
            this.tabCyberware.Controls.Add(this.lblCyberwareEssence);
            this.tabCyberware.Controls.Add(this.lblCyberwareEssenceLabel);
            this.tabCyberware.Controls.Add(this.lblCyberwareCategory);
            this.tabCyberware.Controls.Add(this.lblCyberwareCategoryLabel);
            this.tabCyberware.Controls.Add(this.lblCyberwareName);
            this.tabCyberware.Controls.Add(this.lblCyberwareNameLabel);
            this.tabCyberware.Controls.Add(this.treCyberware);
            this.tabCyberware.Controls.Add(this.cmdAddCyberware);
            this.tabCyberware.Location = new System.Drawing.Point(4, 22);
            this.tabCyberware.Name = "tabCyberware";
            this.tabCyberware.Size = new System.Drawing.Size(838, 611);
            this.tabCyberware.TabIndex = 4;
            this.tabCyberware.Tag = "Tab_Cyberware";
            this.tabCyberware.Text = "Cyberware and Bioware";
            //
            // chkPrototypeTranshuman
            //
            this.chkPrototypeTranshuman.AutoSize = true;
            this.chkPrototypeTranshuman.Location = new System.Drawing.Point(601, 57);
            this.chkPrototypeTranshuman.Name = "chkPrototypeTranshuman";
            this.chkPrototypeTranshuman.Size = new System.Drawing.Size(133, 17);
            this.chkPrototypeTranshuman.TabIndex = 226;
            this.chkPrototypeTranshuman.Tag = "Checkbox_PrototypeTranshuman";
            this.chkPrototypeTranshuman.Text = "Prototype Transhuman";
            this.chkPrototypeTranshuman.UseVisualStyleBackColor = true;
            this.chkPrototypeTranshuman.Visible = false;
            this.chkPrototypeTranshuman.CheckedChanged += new System.EventHandler(this.chkPrototypeTranshuman_CheckedChanged);
            //
            // lblCyberlimbSTR
            //
            this.lblCyberlimbSTR.AutoSize = true;
            this.lblCyberlimbSTR.Location = new System.Drawing.Point(685, 174);
            this.lblCyberlimbSTR.Name = "lblCyberlimbSTR";
            this.lblCyberlimbSTR.Size = new System.Drawing.Size(19, 13);
            this.lblCyberlimbSTR.TabIndex = 225;
            this.lblCyberlimbSTR.Text = "[0]";
            this.lblCyberlimbSTR.Visible = false;
            //
            // lblCyberlimbAGI
            //
            this.lblCyberlimbAGI.AutoSize = true;
            this.lblCyberlimbAGI.Location = new System.Drawing.Point(685, 151);
            this.lblCyberlimbAGI.Name = "lblCyberlimbAGI";
            this.lblCyberlimbAGI.Size = new System.Drawing.Size(19, 13);
            this.lblCyberlimbAGI.TabIndex = 224;
            this.lblCyberlimbAGI.Text = "[0]";
            this.lblCyberlimbAGI.Visible = false;
            //
            // lblCyberlimbSTRLabel
            //
            this.lblCyberlimbSTRLabel.Location = new System.Drawing.Point(598, 174);
            this.lblCyberlimbSTRLabel.Name = "lblCyberlimbSTRLabel";
            this.lblCyberlimbSTRLabel.Size = new System.Drawing.Size(81, 13);
            this.lblCyberlimbSTRLabel.TabIndex = 223;
            this.lblCyberlimbSTRLabel.Tag = "";
            this.lblCyberlimbSTRLabel.Text = "Strength (STR):";
            this.lblCyberlimbSTRLabel.Visible = false;
            //
            // lblCyberlimbAGILabel
            //
            this.lblCyberlimbAGILabel.AutoSize = true;
            this.lblCyberlimbAGILabel.Location = new System.Drawing.Point(598, 151);
            this.lblCyberlimbAGILabel.Name = "lblCyberlimbAGILabel";
            this.lblCyberlimbAGILabel.Size = new System.Drawing.Size(64, 13);
            this.lblCyberlimbAGILabel.TabIndex = 222;
            this.lblCyberlimbAGILabel.Tag = "";
            this.lblCyberlimbAGILabel.Text = "Agility (AGI):";
            this.lblCyberlimbAGILabel.Visible = false;
            //
            // lblCyberFirewall
            //
            this.lblCyberFirewall.AutoSize = true;
            this.lblCyberFirewall.Location = new System.Drawing.Point(753, 306);
            this.lblCyberFirewall.Name = "lblCyberFirewall";
            this.lblCyberFirewall.Size = new System.Drawing.Size(19, 13);
            this.lblCyberFirewall.TabIndex = 175;
            this.lblCyberFirewall.Text = "[0]";
            this.lblCyberFirewall.Visible = false;
            //
            // lblCyberFirewallLabel
            //
            this.lblCyberFirewallLabel.AutoSize = true;
            this.lblCyberFirewallLabel.Location = new System.Drawing.Point(706, 306);
            this.lblCyberFirewallLabel.Name = "lblCyberFirewallLabel";
            this.lblCyberFirewallLabel.Size = new System.Drawing.Size(45, 13);
            this.lblCyberFirewallLabel.TabIndex = 174;
            this.lblCyberFirewallLabel.Tag = "Label_Firewall";
            this.lblCyberFirewallLabel.Text = "Firewall:";
            this.lblCyberFirewallLabel.Visible = false;
            //
            // lblCyberDataProcessing
            //
            this.lblCyberDataProcessing.AutoSize = true;
            this.lblCyberDataProcessing.Location = new System.Drawing.Point(681, 306);
            this.lblCyberDataProcessing.Name = "lblCyberDataProcessing";
            this.lblCyberDataProcessing.Size = new System.Drawing.Size(19, 13);
            this.lblCyberDataProcessing.TabIndex = 173;
            this.lblCyberDataProcessing.Text = "[0]";
            this.lblCyberDataProcessing.Visible = false;
            //
            // lblCyberDataProcessingLabel
            //
            this.lblCyberDataProcessingLabel.AutoSize = true;
            this.lblCyberDataProcessingLabel.Location = new System.Drawing.Point(587, 306);
            this.lblCyberDataProcessingLabel.Name = "lblCyberDataProcessingLabel";
            this.lblCyberDataProcessingLabel.Size = new System.Drawing.Size(88, 13);
            this.lblCyberDataProcessingLabel.TabIndex = 172;
            this.lblCyberDataProcessingLabel.Tag = "Label_DataProcessing";
            this.lblCyberDataProcessingLabel.Text = "Data Processing:";
            this.lblCyberDataProcessingLabel.Visible = false;
            //
            // lblCyberSleaze
            //
            this.lblCyberSleaze.AutoSize = true;
            this.lblCyberSleaze.Location = new System.Drawing.Point(562, 306);
            this.lblCyberSleaze.Name = "lblCyberSleaze";
            this.lblCyberSleaze.Size = new System.Drawing.Size(19, 13);
            this.lblCyberSleaze.TabIndex = 171;
            this.lblCyberSleaze.Text = "[0]";
            this.lblCyberSleaze.Visible = false;
            //
            // lblCyberSleazeLabel
            //
            this.lblCyberSleazeLabel.AutoSize = true;
            this.lblCyberSleazeLabel.Location = new System.Drawing.Point(515, 306);
            this.lblCyberSleazeLabel.Name = "lblCyberSleazeLabel";
            this.lblCyberSleazeLabel.Size = new System.Drawing.Size(42, 13);
            this.lblCyberSleazeLabel.TabIndex = 170;
            this.lblCyberSleazeLabel.Tag = "Label_Sleaze";
            this.lblCyberSleazeLabel.Text = "Sleaze:";
            this.lblCyberSleazeLabel.Visible = false;
            //
            // lblCyberAttack
            //
            this.lblCyberAttack.AutoSize = true;
            this.lblCyberAttack.Location = new System.Drawing.Point(490, 306);
            this.lblCyberAttack.Name = "lblCyberAttack";
            this.lblCyberAttack.Size = new System.Drawing.Size(19, 13);
            this.lblCyberAttack.TabIndex = 169;
            this.lblCyberAttack.Text = "[0]";
            this.lblCyberAttack.Visible = false;
            //
            // lblCyberAttackLabel
            //
            this.lblCyberAttackLabel.AutoSize = true;
            this.lblCyberAttackLabel.Location = new System.Drawing.Point(443, 306);
            this.lblCyberAttackLabel.Name = "lblCyberAttackLabel";
            this.lblCyberAttackLabel.Size = new System.Drawing.Size(41, 13);
            this.lblCyberAttackLabel.TabIndex = 168;
            this.lblCyberAttackLabel.Tag = "Label_Attack";
            this.lblCyberAttackLabel.Text = "Attack:";
            this.lblCyberAttackLabel.Visible = false;
            //
            // lblCyberDeviceRating
            //
            this.lblCyberDeviceRating.AutoSize = true;
            this.lblCyberDeviceRating.Location = new System.Drawing.Point(409, 306);
            this.lblCyberDeviceRating.Name = "lblCyberDeviceRating";
            this.lblCyberDeviceRating.Size = new System.Drawing.Size(19, 13);
            this.lblCyberDeviceRating.TabIndex = 167;
            this.lblCyberDeviceRating.Text = "[0]";
            this.lblCyberDeviceRating.Visible = false;
            //
            // lblCyberDeviceRatingLabel
            //
            this.lblCyberDeviceRatingLabel.AutoSize = true;
            this.lblCyberDeviceRatingLabel.Location = new System.Drawing.Point(310, 306);
            this.lblCyberDeviceRatingLabel.Name = "lblCyberDeviceRatingLabel";
            this.lblCyberDeviceRatingLabel.Size = new System.Drawing.Size(78, 13);
            this.lblCyberDeviceRatingLabel.TabIndex = 166;
            this.lblCyberDeviceRatingLabel.Tag = "Label_DeviceRating";
            this.lblCyberDeviceRatingLabel.Text = "Device Rating:";
            this.lblCyberDeviceRatingLabel.Visible = false;
            //
            // lblEssenceHoleESS
            //
            this.lblEssenceHoleESS.AutoSize = true;
            this.lblEssenceHoleESS.Location = new System.Drawing.Point(387, 269);
            this.lblEssenceHoleESS.Name = "lblEssenceHoleESS";
            this.lblEssenceHoleESS.Size = new System.Drawing.Size(19, 13);
            this.lblEssenceHoleESS.TabIndex = 61;
            this.lblEssenceHoleESS.Text = "[0]";
            //
            // lblEssenceHoleESSLabel
            //
            this.lblEssenceHoleESSLabel.AutoSize = true;
            this.lblEssenceHoleESSLabel.Location = new System.Drawing.Point(310, 269);
            this.lblEssenceHoleESSLabel.Name = "lblEssenceHoleESSLabel";
            this.lblEssenceHoleESSLabel.Size = new System.Drawing.Size(76, 13);
            this.lblEssenceHoleESSLabel.TabIndex = 60;
            this.lblEssenceHoleESSLabel.Tag = "Label_EssenceHole";
            this.lblEssenceHoleESSLabel.Text = "Essence Hole:";
            //
            // label1
            //
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(310, 200);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(112, 13);
            this.label1.TabIndex = 59;
            this.label1.Tag = "Label_EssenceConsumption";
            this.label1.Text = "Essence Consumption";
            //
            // lblBiowareESS
            //
            this.lblBiowareESS.AutoSize = true;
            this.lblBiowareESS.Location = new System.Drawing.Point(387, 246);
            this.lblBiowareESS.Name = "lblBiowareESS";
            this.lblBiowareESS.Size = new System.Drawing.Size(19, 13);
            this.lblBiowareESS.TabIndex = 58;
            this.lblBiowareESS.Text = "[0]";
            //
            // lblCyberwareESS
            //
            this.lblCyberwareESS.AutoSize = true;
            this.lblCyberwareESS.Location = new System.Drawing.Point(387, 223);
            this.lblCyberwareESS.Name = "lblCyberwareESS";
            this.lblCyberwareESS.Size = new System.Drawing.Size(19, 13);
            this.lblCyberwareESS.TabIndex = 57;
            this.lblCyberwareESS.Text = "[0]";
            //
            // lblBiowareESSLabel
            //
            this.lblBiowareESSLabel.AutoSize = true;
            this.lblBiowareESSLabel.Location = new System.Drawing.Point(310, 246);
            this.lblBiowareESSLabel.Name = "lblBiowareESSLabel";
            this.lblBiowareESSLabel.Size = new System.Drawing.Size(48, 13);
            this.lblBiowareESSLabel.TabIndex = 56;
            this.lblBiowareESSLabel.Tag = "Label_Bioware";
            this.lblBiowareESSLabel.Text = "Bioware:";
            //
            // lblCyberwareESSLabel
            //
            this.lblCyberwareESSLabel.AutoSize = true;
            this.lblCyberwareESSLabel.Location = new System.Drawing.Point(310, 223);
            this.lblCyberwareESSLabel.Name = "lblCyberwareESSLabel";
            this.lblCyberwareESSLabel.Size = new System.Drawing.Size(60, 13);
            this.lblCyberwareESSLabel.TabIndex = 55;
            this.lblCyberwareESSLabel.Tag = "Label_Cyberware";
            this.lblCyberwareESSLabel.Text = "Cyberware:";
            //
            // lblCyberwareSource
            //
            this.lblCyberwareSource.AutoSize = true;
            this.lblCyberwareSource.Location = new System.Drawing.Point(387, 151);
            this.lblCyberwareSource.Name = "lblCyberwareSource";
            this.lblCyberwareSource.Size = new System.Drawing.Size(47, 13);
            this.lblCyberwareSource.TabIndex = 48;
            this.lblCyberwareSource.Text = "[Source]";
            this.lblCyberwareSource.Click += new System.EventHandler(this.lblCyberwareSource_Click);
            //
            // lblCyberwareSourceLabel
            //
            this.lblCyberwareSourceLabel.AutoSize = true;
            this.lblCyberwareSourceLabel.Location = new System.Drawing.Point(309, 151);
            this.lblCyberwareSourceLabel.Name = "lblCyberwareSourceLabel";
            this.lblCyberwareSourceLabel.Size = new System.Drawing.Size(44, 13);
            this.lblCyberwareSourceLabel.TabIndex = 47;
            this.lblCyberwareSourceLabel.Tag = "Label_Source";
            this.lblCyberwareSourceLabel.Text = "Source:";
            //
            // cmdAddBioware
            //
            this.cmdAddBioware.AutoSize = true;
            this.cmdAddBioware.Location = new System.Drawing.Point(119, 7);
            this.cmdAddBioware.Name = "cmdAddBioware";
            this.cmdAddBioware.Size = new System.Drawing.Size(90, 23);
            this.cmdAddBioware.TabIndex = 46;
            this.cmdAddBioware.Tag = "Button_AddBioware";
            this.cmdAddBioware.Text = "A&dd Bioware";
            this.cmdAddBioware.UseVisualStyleBackColor = true;
            this.cmdAddBioware.Click += new System.EventHandler(this.cmdAddBioware_Click);
            //
            // cmdDeleteCyberware
            //
            this.cmdDeleteCyberware.AutoSize = true;
            this.cmdDeleteCyberware.Location = new System.Drawing.Point(215, 7);
            this.cmdDeleteCyberware.Name = "cmdDeleteCyberware";
            this.cmdDeleteCyberware.Size = new System.Drawing.Size(80, 23);
            this.cmdDeleteCyberware.TabIndex = 45;
            this.cmdDeleteCyberware.Tag = "String_Delete";
            this.cmdDeleteCyberware.Text = "Delete";
            this.cmdDeleteCyberware.UseVisualStyleBackColor = true;
            this.cmdDeleteCyberware.Click += new System.EventHandler(this.cmdDeleteCyberware_Click);
            //
            // nudCyberwareRating
            //
            this.nudCyberwareRating.Enabled = false;
            this.nudCyberwareRating.Location = new System.Drawing.Point(666, 80);
            this.nudCyberwareRating.Maximum = new decimal(new int[] {
            6,
            0,
            0,
            0});
            this.nudCyberwareRating.Name = "nudCyberwareRating";
            this.nudCyberwareRating.Size = new System.Drawing.Size(41, 20);
            this.nudCyberwareRating.TabIndex = 44;
            this.nudCyberwareRating.ValueChanged += new System.EventHandler(this.nudCyberwareRating_ValueChanged);
            //
            // cboCyberwareGrade
            //
            this.cboCyberwareGrade.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboCyberwareGrade.Enabled = false;
            this.cboCyberwareGrade.FormattingEnabled = true;
            this.cboCyberwareGrade.Location = new System.Drawing.Point(390, 79);
            this.cboCyberwareGrade.Name = "cboCyberwareGrade";
            this.cboCyberwareGrade.Size = new System.Drawing.Size(188, 21);
            this.cboCyberwareGrade.TabIndex = 43;
            this.cboCyberwareGrade.SelectedIndexChanged += new System.EventHandler(this.cboCyberwareGrade_SelectedIndexChanged);
            //
            // lblCyberwareRatingLabel
            //
            this.lblCyberwareRatingLabel.AutoSize = true;
            this.lblCyberwareRatingLabel.Location = new System.Drawing.Point(598, 82);
            this.lblCyberwareRatingLabel.Name = "lblCyberwareRatingLabel";
            this.lblCyberwareRatingLabel.Size = new System.Drawing.Size(41, 13);
            this.lblCyberwareRatingLabel.TabIndex = 42;
            this.lblCyberwareRatingLabel.Tag = "Label_Rating";
            this.lblCyberwareRatingLabel.Text = "Rating:";
            //
            // lblCyberwareCost
            //
            this.lblCyberwareCost.AutoSize = true;
            this.lblCyberwareCost.Location = new System.Drawing.Point(663, 128);
            this.lblCyberwareCost.Name = "lblCyberwareCost";
            this.lblCyberwareCost.Size = new System.Drawing.Size(34, 13);
            this.lblCyberwareCost.TabIndex = 41;
            this.lblCyberwareCost.Text = "[Cost]";
            //
            // lblCyberwareCostLabel
            //
            this.lblCyberwareCostLabel.AutoSize = true;
            this.lblCyberwareCostLabel.Location = new System.Drawing.Point(598, 128);
            this.lblCyberwareCostLabel.Name = "lblCyberwareCostLabel";
            this.lblCyberwareCostLabel.Size = new System.Drawing.Size(31, 13);
            this.lblCyberwareCostLabel.TabIndex = 40;
            this.lblCyberwareCostLabel.Tag = "Label_Cost";
            this.lblCyberwareCostLabel.Text = "Cost:";
            //
            // lblCyberwareAvail
            //
            this.lblCyberwareAvail.AutoSize = true;
            this.lblCyberwareAvail.Location = new System.Drawing.Point(387, 128);
            this.lblCyberwareAvail.Name = "lblCyberwareAvail";
            this.lblCyberwareAvail.Size = new System.Drawing.Size(36, 13);
            this.lblCyberwareAvail.TabIndex = 39;
            this.lblCyberwareAvail.Text = "[Avail]";
            //
            // lblCyberwareAvailLabel
            //
            this.lblCyberwareAvailLabel.AutoSize = true;
            this.lblCyberwareAvailLabel.Location = new System.Drawing.Point(309, 128);
            this.lblCyberwareAvailLabel.Name = "lblCyberwareAvailLabel";
            this.lblCyberwareAvailLabel.Size = new System.Drawing.Size(33, 13);
            this.lblCyberwareAvailLabel.TabIndex = 38;
            this.lblCyberwareAvailLabel.Tag = "Label_Avail";
            this.lblCyberwareAvailLabel.Text = "Avail:";
            //
            // lblCyberwareGradeLabel
            //
            this.lblCyberwareGradeLabel.AutoSize = true;
            this.lblCyberwareGradeLabel.Location = new System.Drawing.Point(309, 82);
            this.lblCyberwareGradeLabel.Name = "lblCyberwareGradeLabel";
            this.lblCyberwareGradeLabel.Size = new System.Drawing.Size(39, 13);
            this.lblCyberwareGradeLabel.TabIndex = 37;
            this.lblCyberwareGradeLabel.Tag = "Label_Grade";
            this.lblCyberwareGradeLabel.Text = "Grade:";
            //
            // lblCyberwareCapacity
            //
            this.lblCyberwareCapacity.AutoSize = true;
            this.lblCyberwareCapacity.Location = new System.Drawing.Point(663, 105);
            this.lblCyberwareCapacity.Name = "lblCyberwareCapacity";
            this.lblCyberwareCapacity.Size = new System.Drawing.Size(54, 13);
            this.lblCyberwareCapacity.TabIndex = 36;
            this.lblCyberwareCapacity.Text = "[Capacity]";
            //
            // lblCyberwareCapacityLabel
            //
            this.lblCyberwareCapacityLabel.AutoSize = true;
            this.lblCyberwareCapacityLabel.Location = new System.Drawing.Point(598, 105);
            this.lblCyberwareCapacityLabel.Name = "lblCyberwareCapacityLabel";
            this.lblCyberwareCapacityLabel.Size = new System.Drawing.Size(51, 13);
            this.lblCyberwareCapacityLabel.TabIndex = 35;
            this.lblCyberwareCapacityLabel.Tag = "Label_Capacity";
            this.lblCyberwareCapacityLabel.Text = "Capacity:";
            //
            // lblCyberwareEssence
            //
            this.lblCyberwareEssence.AutoSize = true;
            this.lblCyberwareEssence.Location = new System.Drawing.Point(387, 105);
            this.lblCyberwareEssence.Name = "lblCyberwareEssence";
            this.lblCyberwareEssence.Size = new System.Drawing.Size(34, 13);
            this.lblCyberwareEssence.TabIndex = 34;
            this.lblCyberwareEssence.Text = "[ESS]";
            //
            // lblCyberwareEssenceLabel
            //
            this.lblCyberwareEssenceLabel.AutoSize = true;
            this.lblCyberwareEssenceLabel.Location = new System.Drawing.Point(309, 105);
            this.lblCyberwareEssenceLabel.Name = "lblCyberwareEssenceLabel";
            this.lblCyberwareEssenceLabel.Size = new System.Drawing.Size(51, 13);
            this.lblCyberwareEssenceLabel.TabIndex = 33;
            this.lblCyberwareEssenceLabel.Tag = "Label_Essence";
            this.lblCyberwareEssenceLabel.Text = "Essence:";
            //
            // lblCyberwareCategory
            //
            this.lblCyberwareCategory.AutoSize = true;
            this.lblCyberwareCategory.Location = new System.Drawing.Point(387, 59);
            this.lblCyberwareCategory.Name = "lblCyberwareCategory";
            this.lblCyberwareCategory.Size = new System.Drawing.Size(55, 13);
            this.lblCyberwareCategory.TabIndex = 32;
            this.lblCyberwareCategory.Text = "[Category]";
            //
            // lblCyberwareCategoryLabel
            //
            this.lblCyberwareCategoryLabel.AutoSize = true;
            this.lblCyberwareCategoryLabel.Location = new System.Drawing.Point(309, 59);
            this.lblCyberwareCategoryLabel.Name = "lblCyberwareCategoryLabel";
            this.lblCyberwareCategoryLabel.Size = new System.Drawing.Size(52, 13);
            this.lblCyberwareCategoryLabel.TabIndex = 31;
            this.lblCyberwareCategoryLabel.Tag = "Label_Category";
            this.lblCyberwareCategoryLabel.Text = "Category:";
            //
            // lblCyberwareName
            //
            this.lblCyberwareName.AutoSize = true;
            this.lblCyberwareName.Location = new System.Drawing.Point(387, 36);
            this.lblCyberwareName.Name = "lblCyberwareName";
            this.lblCyberwareName.Size = new System.Drawing.Size(41, 13);
            this.lblCyberwareName.TabIndex = 30;
            this.lblCyberwareName.Text = "[Name]";
            //
            // lblCyberwareNameLabel
            //
            this.lblCyberwareNameLabel.AutoSize = true;
            this.lblCyberwareNameLabel.Location = new System.Drawing.Point(309, 36);
            this.lblCyberwareNameLabel.Name = "lblCyberwareNameLabel";
            this.lblCyberwareNameLabel.Size = new System.Drawing.Size(38, 13);
            this.lblCyberwareNameLabel.TabIndex = 29;
            this.lblCyberwareNameLabel.Tag = "Label_Name";
            this.lblCyberwareNameLabel.Text = "Name:";
            //
            // treCyberware
            //
            this.treCyberware.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)));
            this.treCyberware.HideSelection = false;
            this.treCyberware.Location = new System.Drawing.Point(8, 36);
            this.treCyberware.Name = "treCyberware";
            treeNode19.Name = "nodCyberwareRoot";
            treeNode19.Tag = "Node_SelectedCyberware";
            treeNode19.Text = "Selected Cyberware";
            treeNode20.Name = "nodBioware";
            treeNode20.Tag = "Node_SelectedBioware";
            treeNode20.Text = "Selected Bioware";
            this.treCyberware.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            treeNode19,
            treeNode20});
            this.treCyberware.ShowNodeToolTips = true;
            this.treCyberware.ShowRootLines = false;
            this.treCyberware.Size = new System.Drawing.Size(295, 572);
            this.treCyberware.TabIndex = 28;
            this.treCyberware.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treCyberware_AfterSelect);
            this.treCyberware.KeyDown += new System.Windows.Forms.KeyEventHandler(this.treCyberware_KeyDown);
            this.treCyberware.MouseDown += new System.Windows.Forms.MouseEventHandler(this.TreeView_MouseDown);
            //
            // cmdAddCyberware
            //
            this.cmdAddCyberware.AutoSize = true;
            this.cmdAddCyberware.ContextMenuStrip = this.cmsCyberware;
            this.cmdAddCyberware.Location = new System.Drawing.Point(8, 7);
            this.cmdAddCyberware.Name = "cmdAddCyberware";
            this.cmdAddCyberware.Size = new System.Drawing.Size(107, 23);
            this.cmdAddCyberware.SplitMenuStrip = this.cmsCyberware;
            this.cmdAddCyberware.TabIndex = 91;
            this.cmdAddCyberware.Tag = "Button_AddCyberware";
            this.cmdAddCyberware.Text = "&Add Cyberware";
            this.cmdAddCyberware.UseVisualStyleBackColor = true;
            this.cmdAddCyberware.Click += new System.EventHandler(this.cmdAddCyberware_Click);
            //
            // tabStreetGear
            //
            this.tabStreetGear.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.tabStreetGear.Controls.Add(this.tabStreetGearTabs);
            this.tabStreetGear.Location = new System.Drawing.Point(4, 22);
            this.tabStreetGear.Name = "tabStreetGear";
            this.tabStreetGear.Size = new System.Drawing.Size(838, 611);
            this.tabStreetGear.TabIndex = 5;
            this.tabStreetGear.Tag = "Tab_StreeGear";
            this.tabStreetGear.Text = "Street Gear";
            //
            // tabStreetGearTabs
            //
            this.tabStreetGearTabs.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabStreetGearTabs.Controls.Add(this.tabLifestyle);
            this.tabStreetGearTabs.Controls.Add(this.tabArmor);
            this.tabStreetGearTabs.Controls.Add(this.tabWeapons);
            this.tabStreetGearTabs.Controls.Add(this.tabGear);
            this.tabStreetGearTabs.Controls.Add(this.tabPets);
            this.tabStreetGearTabs.Location = new System.Drawing.Point(3, 3);
            this.tabStreetGearTabs.Name = "tabStreetGearTabs";
            this.tabStreetGearTabs.SelectedIndex = 0;
            this.tabStreetGearTabs.Size = new System.Drawing.Size(835, 605);
            this.tabStreetGearTabs.TabIndex = 87;
            this.tabStreetGearTabs.SelectedIndexChanged += new System.EventHandler(this.tabStreetGearTabs_SelectedIndexChanged);
            //
            // tabLifestyle
            //
            this.tabLifestyle.BackColor = System.Drawing.SystemColors.Control;
            this.tabLifestyle.Controls.Add(this.cmdAddLifestyle);
            this.tabLifestyle.Controls.Add(this.lblLifestyleQualities);
            this.tabLifestyle.Controls.Add(this.lblLifestyleQualitiesLabel);
            this.tabLifestyle.Controls.Add(this.lblBaseLifestyle);
            this.tabLifestyle.Controls.Add(this.lblLifestyleComfortsLabel);
            this.tabLifestyle.Controls.Add(this.lblLifestyleStartingNuyenLabel);
            this.tabLifestyle.Controls.Add(this.lblLifestyleStartingNuyen);
            this.tabLifestyle.Controls.Add(this.lblLifestyleSource);
            this.tabLifestyle.Controls.Add(this.lblLifestyleSourceLabel);
            this.tabLifestyle.Controls.Add(this.lblLifestyleTotalCost);
            this.tabLifestyle.Controls.Add(this.lblLifestyleCostLabel);
            this.tabLifestyle.Controls.Add(this.treLifestyles);
            this.tabLifestyle.Controls.Add(this.lblLifestyleCost);
            this.tabLifestyle.Controls.Add(this.cmdDeleteLifestyle);
            this.tabLifestyle.Controls.Add(this.lblLifestyleMonthsLabel);
            this.tabLifestyle.Controls.Add(this.nudLifestyleMonths);
            this.tabLifestyle.Location = new System.Drawing.Point(4, 22);
            this.tabLifestyle.Name = "tabLifestyle";
            this.tabLifestyle.Padding = new System.Windows.Forms.Padding(3);
            this.tabLifestyle.Size = new System.Drawing.Size(827, 579);
            this.tabLifestyle.TabIndex = 0;
            this.tabLifestyle.Tag = "Tab_Lifestyle";
            this.tabLifestyle.Text = "Lifestyle";
            //
            // cmdAddLifestyle
            //
            this.cmdAddLifestyle.AutoSize = true;
            this.cmdAddLifestyle.ContextMenuStrip = this.cmsLifestyle;
            this.cmdAddLifestyle.Location = new System.Drawing.Point(6, 7);
            this.cmdAddLifestyle.Name = "cmdAddLifestyle";
            this.cmdAddLifestyle.Size = new System.Drawing.Size(95, 23);
            this.cmdAddLifestyle.SplitMenuStrip = this.cmsLifestyle;
            this.cmdAddLifestyle.TabIndex = 91;
            this.cmdAddLifestyle.Tag = "Button_AddLifestyle";
            this.cmdAddLifestyle.Text = "&Add Lifestyle";
            this.cmdAddLifestyle.UseVisualStyleBackColor = true;
            this.cmdAddLifestyle.Click += new System.EventHandler(this.cmdAddLifestyle_Click);
            //
            // lblLifestyleQualities
            //
            this.lblLifestyleQualities.Location = new System.Drawing.Point(412, 144);
            this.lblLifestyleQualities.Name = "lblLifestyleQualities";
            this.lblLifestyleQualities.Size = new System.Drawing.Size(323, 290);
            this.lblLifestyleQualities.TabIndex = 103;
            this.lblLifestyleQualities.Text = "[0]";
            //
            // lblLifestyleQualitiesLabel
            //
            this.lblLifestyleQualitiesLabel.AutoSize = true;
            this.lblLifestyleQualitiesLabel.Location = new System.Drawing.Point(412, 128);
            this.lblLifestyleQualitiesLabel.Name = "lblLifestyleQualitiesLabel";
            this.lblLifestyleQualitiesLabel.Size = new System.Drawing.Size(50, 13);
            this.lblLifestyleQualitiesLabel.TabIndex = 102;
            this.lblLifestyleQualitiesLabel.Tag = "Label_LifestyleQualities";
            this.lblLifestyleQualitiesLabel.Text = "Qualities:";
            //
            // lblBaseLifestyle
            //
            this.lblBaseLifestyle.AutoSize = true;
            this.lblBaseLifestyle.Location = new System.Drawing.Point(361, 128);
            this.lblBaseLifestyle.Name = "lblBaseLifestyle";
            this.lblBaseLifestyle.Size = new System.Drawing.Size(19, 13);
            this.lblBaseLifestyle.TabIndex = 93;
            this.lblBaseLifestyle.Text = "[0]";
            //
            // lblLifestyleComfortsLabel
            //
            this.lblLifestyleComfortsLabel.AutoSize = true;
            this.lblLifestyleComfortsLabel.Location = new System.Drawing.Point(307, 128);
            this.lblLifestyleComfortsLabel.Name = "lblLifestyleComfortsLabel";
            this.lblLifestyleComfortsLabel.Size = new System.Drawing.Size(48, 13);
            this.lblLifestyleComfortsLabel.TabIndex = 92;
            this.lblLifestyleComfortsLabel.Tag = "Label_SelectAdvancedLifestyle_Lifestyle";
            this.lblLifestyleComfortsLabel.Text = "Lifestyle:";
            //
            // lblLifestyleStartingNuyenLabel
            //
            this.lblLifestyleStartingNuyenLabel.AutoSize = true;
            this.lblLifestyleStartingNuyenLabel.Location = new System.Drawing.Point(412, 95);
            this.lblLifestyleStartingNuyenLabel.Name = "lblLifestyleStartingNuyenLabel";
            this.lblLifestyleStartingNuyenLabel.Size = new System.Drawing.Size(80, 13);
            this.lblLifestyleStartingNuyenLabel.TabIndex = 90;
            this.lblLifestyleStartingNuyenLabel.Tag = "Label_SelectLifestyle_StartingNuyen";
            this.lblLifestyleStartingNuyenLabel.Text = "Starting Nuyen:";
            //
            // lblLifestyleStartingNuyen
            //
            this.lblLifestyleStartingNuyen.AutoSize = true;
            this.lblLifestyleStartingNuyen.Location = new System.Drawing.Point(498, 95);
            this.lblLifestyleStartingNuyen.Name = "lblLifestyleStartingNuyen";
            this.lblLifestyleStartingNuyen.Size = new System.Drawing.Size(83, 13);
            this.lblLifestyleStartingNuyen.TabIndex = 89;
            this.lblLifestyleStartingNuyen.Text = "[Starting Nuyen]";
            //
            // lblLifestyleSource
            //
            this.lblLifestyleSource.AutoSize = true;
            this.lblLifestyleSource.Location = new System.Drawing.Point(359, 95);
            this.lblLifestyleSource.Name = "lblLifestyleSource";
            this.lblLifestyleSource.Size = new System.Drawing.Size(47, 13);
            this.lblLifestyleSource.TabIndex = 88;
            this.lblLifestyleSource.Text = "[Source]";
            this.lblLifestyleSource.Click += new System.EventHandler(this.lblLifestyleSource_Click);
            //
            // lblLifestyleSourceLabel
            //
            this.lblLifestyleSourceLabel.AutoSize = true;
            this.lblLifestyleSourceLabel.Location = new System.Drawing.Point(308, 95);
            this.lblLifestyleSourceLabel.Name = "lblLifestyleSourceLabel";
            this.lblLifestyleSourceLabel.Size = new System.Drawing.Size(44, 13);
            this.lblLifestyleSourceLabel.TabIndex = 87;
            this.lblLifestyleSourceLabel.Tag = "Label_Source";
            this.lblLifestyleSourceLabel.Text = "Source:";
            //
            // lblLifestyleTotalCost
            //
            this.lblLifestyleTotalCost.AutoSize = true;
            this.lblLifestyleTotalCost.Location = new System.Drawing.Point(498, 64);
            this.lblLifestyleTotalCost.Name = "lblLifestyleTotalCost";
            this.lblLifestyleTotalCost.Size = new System.Drawing.Size(34, 13);
            this.lblLifestyleTotalCost.TabIndex = 86;
            this.lblLifestyleTotalCost.Text = "[Cost]";
            //
            // lblLifestyleCostLabel
            //
            this.lblLifestyleCostLabel.AutoSize = true;
            this.lblLifestyleCostLabel.Location = new System.Drawing.Point(307, 36);
            this.lblLifestyleCostLabel.Name = "lblLifestyleCostLabel";
            this.lblLifestyleCostLabel.Size = new System.Drawing.Size(66, 13);
            this.lblLifestyleCostLabel.TabIndex = 85;
            this.lblLifestyleCostLabel.Tag = "Label_SelectLifestyle_CostPerMonth";
            this.lblLifestyleCostLabel.Text = "Cost/Month:";
            //
            // treLifestyles
            //
            this.treLifestyles.AllowDrop = true;
            this.treLifestyles.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)));
            this.treLifestyles.HideSelection = false;
            this.treLifestyles.Location = new System.Drawing.Point(6, 36);
            this.treLifestyles.Name = "treLifestyles";
            treeNode21.Name = "nodLifestylesRoot";
            treeNode21.Tag = "Node_SelectedLifestyles";
            treeNode21.Text = "Selected Lifestyles";
            this.treLifestyles.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            treeNode21});
            this.treLifestyles.ShowNodeToolTips = true;
            this.treLifestyles.ShowRootLines = false;
            this.treLifestyles.Size = new System.Drawing.Size(295, 537);
            this.treLifestyles.TabIndex = 80;
            this.treLifestyles.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treLifestyles_AfterSelect);
            this.treLifestyles.DragOver += new System.Windows.Forms.DragEventHandler(this.treLifestyles_DragOver);
            this.treLifestyles.DoubleClick += new System.EventHandler(this.treLifestyles_DoubleClick);
            this.treLifestyles.KeyDown += new System.Windows.Forms.KeyEventHandler(this.treLifestyles_KeyDown);
            this.treLifestyles.MouseDown += new System.Windows.Forms.MouseEventHandler(this.TreeView_MouseDown);
            //
            // lblLifestyleCost
            //
            this.lblLifestyleCost.AutoSize = true;
            this.lblLifestyleCost.Location = new System.Drawing.Point(379, 36);
            this.lblLifestyleCost.Name = "lblLifestyleCost";
            this.lblLifestyleCost.Size = new System.Drawing.Size(34, 13);
            this.lblLifestyleCost.TabIndex = 84;
            this.lblLifestyleCost.Text = "[Cost]";
            //
            // cmdDeleteLifestyle
            //
            this.cmdDeleteLifestyle.AutoSize = true;
            this.cmdDeleteLifestyle.Location = new System.Drawing.Point(107, 7);
            this.cmdDeleteLifestyle.Name = "cmdDeleteLifestyle";
            this.cmdDeleteLifestyle.Size = new System.Drawing.Size(80, 23);
            this.cmdDeleteLifestyle.TabIndex = 81;
            this.cmdDeleteLifestyle.Tag = "String_Delete";
            this.cmdDeleteLifestyle.Text = "Delete";
            this.cmdDeleteLifestyle.UseVisualStyleBackColor = true;
            this.cmdDeleteLifestyle.Click += new System.EventHandler(this.cmdDeleteLifestyle_Click);
            //
            // lblLifestyleMonthsLabel
            //
            this.lblLifestyleMonthsLabel.AutoSize = true;
            this.lblLifestyleMonthsLabel.Location = new System.Drawing.Point(354, 64);
            this.lblLifestyleMonthsLabel.Name = "lblLifestyleMonthsLabel";
            this.lblLifestyleMonthsLabel.Size = new System.Drawing.Size(138, 13);
            this.lblLifestyleMonthsLabel.TabIndex = 83;
            this.lblLifestyleMonthsLabel.Tag = "Label_LifestyleMonths";
            this.lblLifestyleMonthsLabel.Text = "Months (100 for Permanent)";
            //
            // nudLifestyleMonths
            //
            this.nudLifestyleMonths.Enabled = false;
            this.nudLifestyleMonths.Location = new System.Drawing.Point(310, 62);
            this.nudLifestyleMonths.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudLifestyleMonths.Name = "nudLifestyleMonths";
            this.nudLifestyleMonths.Size = new System.Drawing.Size(42, 20);
            this.nudLifestyleMonths.TabIndex = 82;
            this.nudLifestyleMonths.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudLifestyleMonths.ValueChanged += new System.EventHandler(this.nudLifestyleMonths_ValueChanged);
            //
            // tabArmor
            //
            this.tabArmor.BackColor = System.Drawing.SystemColors.Control;
            this.tabArmor.Controls.Add(this.lblArmorFirewall);
            this.tabArmor.Controls.Add(this.lblArmorFirewallLabel);
            this.tabArmor.Controls.Add(this.lblArmorDataProcessing);
            this.tabArmor.Controls.Add(this.lblArmorDataProcessingLabel);
            this.tabArmor.Controls.Add(this.lblArmorSleaze);
            this.tabArmor.Controls.Add(this.lblArmorSleazeLabel);
            this.tabArmor.Controls.Add(this.lblArmorAttack);
            this.tabArmor.Controls.Add(this.lblArmorAttackLabel);
            this.tabArmor.Controls.Add(this.lblArmorDeviceRating);
            this.tabArmor.Controls.Add(this.lblArmorDeviceRatingLabel);
            this.tabArmor.Controls.Add(this.lblArmorValueLabel);
            this.tabArmor.Controls.Add(this.lblArmorValue);
            this.tabArmor.Controls.Add(this.chkIncludedInArmor);
            this.tabArmor.Controls.Add(this.lblArmorEquipped);
            this.tabArmor.Controls.Add(this.lblArmorEquippedLabel);
            this.tabArmor.Controls.Add(this.cmdArmorUnEquipAll);
            this.tabArmor.Controls.Add(this.cmdArmorEquipAll);
            this.tabArmor.Controls.Add(this.cmdAddArmorBundle);
            this.tabArmor.Controls.Add(this.lblArmorCapacity);
            this.tabArmor.Controls.Add(this.lblArmorCapacityLabel);
            this.tabArmor.Controls.Add(this.nudArmorRating);
            this.tabArmor.Controls.Add(this.lblArmorRatingLabel);
            this.tabArmor.Controls.Add(this.lblArmorSource);
            this.tabArmor.Controls.Add(this.lblArmorSourceLabel);
            this.tabArmor.Controls.Add(this.chkArmorEquipped);
            this.tabArmor.Controls.Add(this.lblArmorCost);
            this.tabArmor.Controls.Add(this.lblArmorCostLabel);
            this.tabArmor.Controls.Add(this.cmdDeleteArmor);
            this.tabArmor.Controls.Add(this.lblArmorAvail);
            this.tabArmor.Controls.Add(this.treArmor);
            this.tabArmor.Controls.Add(this.lblArmorAvailLabel);
            this.tabArmor.Controls.Add(this.cmdAddArmor);
            this.tabArmor.Location = new System.Drawing.Point(4, 22);
            this.tabArmor.Name = "tabArmor";
            this.tabArmor.Padding = new System.Windows.Forms.Padding(3);
            this.tabArmor.Size = new System.Drawing.Size(827, 579);
            this.tabArmor.TabIndex = 1;
            this.tabArmor.Tag = "Tab_Armor";
            this.tabArmor.Text = "Armor";
            //
            // lblArmorFirewall
            //
            this.lblArmorFirewall.AutoSize = true;
            this.lblArmorFirewall.Location = new System.Drawing.Point(750, 86);
            this.lblArmorFirewall.Name = "lblArmorFirewall";
            this.lblArmorFirewall.Size = new System.Drawing.Size(19, 13);
            this.lblArmorFirewall.TabIndex = 175;
            this.lblArmorFirewall.Text = "[0]";
            //
            // lblArmorFirewallLabel
            //
            this.lblArmorFirewallLabel.AutoSize = true;
            this.lblArmorFirewallLabel.Location = new System.Drawing.Point(703, 86);
            this.lblArmorFirewallLabel.Name = "lblArmorFirewallLabel";
            this.lblArmorFirewallLabel.Size = new System.Drawing.Size(45, 13);
            this.lblArmorFirewallLabel.TabIndex = 174;
            this.lblArmorFirewallLabel.Tag = "Label_Firewall";
            this.lblArmorFirewallLabel.Text = "Firewall:";
            //
            // lblArmorDataProcessing
            //
            this.lblArmorDataProcessing.AutoSize = true;
            this.lblArmorDataProcessing.Location = new System.Drawing.Point(678, 86);
            this.lblArmorDataProcessing.Name = "lblArmorDataProcessing";
            this.lblArmorDataProcessing.Size = new System.Drawing.Size(19, 13);
            this.lblArmorDataProcessing.TabIndex = 173;
            this.lblArmorDataProcessing.Text = "[0]";
            //
            // lblArmorDataProcessingLabel
            //
            this.lblArmorDataProcessingLabel.AutoSize = true;
            this.lblArmorDataProcessingLabel.Location = new System.Drawing.Point(584, 86);
            this.lblArmorDataProcessingLabel.Name = "lblArmorDataProcessingLabel";
            this.lblArmorDataProcessingLabel.Size = new System.Drawing.Size(88, 13);
            this.lblArmorDataProcessingLabel.TabIndex = 172;
            this.lblArmorDataProcessingLabel.Tag = "Label_DataProcessing";
            this.lblArmorDataProcessingLabel.Text = "Data Processing:";
            //
            // lblArmorSleaze
            //
            this.lblArmorSleaze.AutoSize = true;
            this.lblArmorSleaze.Location = new System.Drawing.Point(559, 86);
            this.lblArmorSleaze.Name = "lblArmorSleaze";
            this.lblArmorSleaze.Size = new System.Drawing.Size(19, 13);
            this.lblArmorSleaze.TabIndex = 171;
            this.lblArmorSleaze.Text = "[0]";
            //
            // lblArmorSleazeLabel
            //
            this.lblArmorSleazeLabel.AutoSize = true;
            this.lblArmorSleazeLabel.Location = new System.Drawing.Point(512, 86);
            this.lblArmorSleazeLabel.Name = "lblArmorSleazeLabel";
            this.lblArmorSleazeLabel.Size = new System.Drawing.Size(42, 13);
            this.lblArmorSleazeLabel.TabIndex = 170;
            this.lblArmorSleazeLabel.Tag = "Label_Sleaze";
            this.lblArmorSleazeLabel.Text = "Sleaze:";
            //
            // lblArmorAttack
            //
            this.lblArmorAttack.AutoSize = true;
            this.lblArmorAttack.Location = new System.Drawing.Point(487, 86);
            this.lblArmorAttack.Name = "lblArmorAttack";
            this.lblArmorAttack.Size = new System.Drawing.Size(19, 13);
            this.lblArmorAttack.TabIndex = 169;
            this.lblArmorAttack.Text = "[0]";
            //
            // lblArmorAttackLabel
            //
            this.lblArmorAttackLabel.AutoSize = true;
            this.lblArmorAttackLabel.Location = new System.Drawing.Point(440, 86);
            this.lblArmorAttackLabel.Name = "lblArmorAttackLabel";
            this.lblArmorAttackLabel.Size = new System.Drawing.Size(41, 13);
            this.lblArmorAttackLabel.TabIndex = 168;
            this.lblArmorAttackLabel.Tag = "Label_Attack";
            this.lblArmorAttackLabel.Text = "Attack:";
            //
            // lblArmorDeviceRating
            //
            this.lblArmorDeviceRating.AutoSize = true;
            this.lblArmorDeviceRating.Location = new System.Drawing.Point(406, 86);
            this.lblArmorDeviceRating.Name = "lblArmorDeviceRating";
            this.lblArmorDeviceRating.Size = new System.Drawing.Size(19, 13);
            this.lblArmorDeviceRating.TabIndex = 167;
            this.lblArmorDeviceRating.Text = "[0]";
            //
            // lblArmorDeviceRatingLabel
            //
            this.lblArmorDeviceRatingLabel.AutoSize = true;
            this.lblArmorDeviceRatingLabel.Location = new System.Drawing.Point(307, 86);
            this.lblArmorDeviceRatingLabel.Name = "lblArmorDeviceRatingLabel";
            this.lblArmorDeviceRatingLabel.Size = new System.Drawing.Size(78, 13);
            this.lblArmorDeviceRatingLabel.TabIndex = 166;
            this.lblArmorDeviceRatingLabel.Tag = "Label_DeviceRating";
            this.lblArmorDeviceRatingLabel.Text = "Device Rating:";
            //
            // lblArmorValueLabel
            //
            this.lblArmorValueLabel.AutoSize = true;
            this.lblArmorValueLabel.Location = new System.Drawing.Point(307, 36);
            this.lblArmorValueLabel.Name = "lblArmorValueLabel";
            this.lblArmorValueLabel.Size = new System.Drawing.Size(37, 13);
            this.lblArmorValueLabel.TabIndex = 132;
            this.lblArmorValueLabel.Tag = "Label_Armor";
            this.lblArmorValueLabel.Text = "Armor:";
            //
            // lblArmorValue
            //
            this.lblArmorValue.AutoSize = true;
            this.lblArmorValue.Location = new System.Drawing.Point(358, 36);
            this.lblArmorValue.Name = "lblArmorValue";
            this.lblArmorValue.Size = new System.Drawing.Size(20, 13);
            this.lblArmorValue.TabIndex = 133;
            this.lblArmorValue.Text = "[A]";
            //
            // chkIncludedInArmor
            //
            this.chkIncludedInArmor.AutoSize = true;
            this.chkIncludedInArmor.Enabled = false;
            this.chkIncludedInArmor.Location = new System.Drawing.Point(503, 132);
            this.chkIncludedInArmor.Name = "chkIncludedInArmor";
            this.chkIncludedInArmor.Size = new System.Drawing.Size(113, 17);
            this.chkIncludedInArmor.TabIndex = 113;
            this.chkIncludedInArmor.Tag = "Checkbox_BaseArmor";
            this.chkIncludedInArmor.Text = "Part of base Armor";
            this.chkIncludedInArmor.UseVisualStyleBackColor = true;
            this.chkIncludedInArmor.CheckedChanged += new System.EventHandler(this.chkIncludedInArmor_CheckedChanged);
            //
            // lblArmorEquipped
            //
            this.lblArmorEquipped.Location = new System.Drawing.Point(307, 212);
            this.lblArmorEquipped.Name = "lblArmorEquipped";
            this.lblArmorEquipped.Size = new System.Drawing.Size(514, 339);
            this.lblArmorEquipped.TabIndex = 111;
            this.lblArmorEquipped.Text = "[Armor Bundle Equipped Items]";
            this.lblArmorEquipped.Visible = false;
            //
            // lblArmorEquippedLabel
            //
            this.lblArmorEquippedLabel.AutoSize = true;
            this.lblArmorEquippedLabel.Location = new System.Drawing.Point(307, 189);
            this.lblArmorEquippedLabel.Name = "lblArmorEquippedLabel";
            this.lblArmorEquippedLabel.Size = new System.Drawing.Size(52, 13);
            this.lblArmorEquippedLabel.TabIndex = 110;
            this.lblArmorEquippedLabel.Tag = "Checkbox_Equipped";
            this.lblArmorEquippedLabel.Text = "Equipped";
            this.lblArmorEquippedLabel.Visible = false;
            //
            // cmdArmorUnEquipAll
            //
            this.cmdArmorUnEquipAll.AutoSize = true;
            this.cmdArmorUnEquipAll.Location = new System.Drawing.Point(475, 153);
            this.cmdArmorUnEquipAll.Name = "cmdArmorUnEquipAll";
            this.cmdArmorUnEquipAll.Size = new System.Drawing.Size(82, 23);
            this.cmdArmorUnEquipAll.TabIndex = 109;
            this.cmdArmorUnEquipAll.Tag = "Button_UnEquipAll";
            this.cmdArmorUnEquipAll.Text = "Un-Equip All";
            this.cmdArmorUnEquipAll.UseVisualStyleBackColor = true;
            this.cmdArmorUnEquipAll.Visible = false;
            this.cmdArmorUnEquipAll.Click += new System.EventHandler(this.cmdArmorUnEquipAll_Click);
            //
            // cmdArmorEquipAll
            //
            this.cmdArmorEquipAll.AutoSize = true;
            this.cmdArmorEquipAll.Location = new System.Drawing.Point(387, 153);
            this.cmdArmorEquipAll.Name = "cmdArmorEquipAll";
            this.cmdArmorEquipAll.Size = new System.Drawing.Size(82, 23);
            this.cmdArmorEquipAll.TabIndex = 108;
            this.cmdArmorEquipAll.Tag = "Button_EquipAll";
            this.cmdArmorEquipAll.Text = "Equip All";
            this.cmdArmorEquipAll.UseVisualStyleBackColor = true;
            this.cmdArmorEquipAll.Visible = false;
            this.cmdArmorEquipAll.Click += new System.EventHandler(this.cmdArmorEquipAll_Click);
            //
            // cmdAddArmorBundle
            //
            this.cmdAddArmorBundle.AutoSize = true;
            this.cmdAddArmorBundle.Location = new System.Drawing.Point(221, 7);
            this.cmdAddArmorBundle.Name = "cmdAddArmorBundle";
            this.cmdAddArmorBundle.Size = new System.Drawing.Size(102, 23);
            this.cmdAddArmorBundle.TabIndex = 104;
            this.cmdAddArmorBundle.Tag = "Button_AddBundle";
            this.cmdAddArmorBundle.Text = "Add Armor Bundle";
            this.cmdAddArmorBundle.UseVisualStyleBackColor = true;
            this.cmdAddArmorBundle.Click += new System.EventHandler(this.cmdAddArmorBundle_Click);
            //
            // lblArmorCapacity
            //
            this.lblArmorCapacity.AutoSize = true;
            this.lblArmorCapacity.Location = new System.Drawing.Point(358, 110);
            this.lblArmorCapacity.Name = "lblArmorCapacity";
            this.lblArmorCapacity.Size = new System.Drawing.Size(54, 13);
            this.lblArmorCapacity.TabIndex = 84;
            this.lblArmorCapacity.Text = "[Capacity]";
            //
            // lblArmorCapacityLabel
            //
            this.lblArmorCapacityLabel.AutoSize = true;
            this.lblArmorCapacityLabel.Location = new System.Drawing.Point(307, 110);
            this.lblArmorCapacityLabel.Name = "lblArmorCapacityLabel";
            this.lblArmorCapacityLabel.Size = new System.Drawing.Size(51, 13);
            this.lblArmorCapacityLabel.TabIndex = 83;
            this.lblArmorCapacityLabel.Tag = "Label_Capacity";
            this.lblArmorCapacityLabel.Text = "Capacity:";
            //
            // nudArmorRating
            //
            this.nudArmorRating.Enabled = false;
            this.nudArmorRating.Location = new System.Drawing.Point(361, 58);
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
            this.nudArmorRating.Size = new System.Drawing.Size(33, 20);
            this.nudArmorRating.TabIndex = 82;
            this.nudArmorRating.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudArmorRating.ValueChanged += new System.EventHandler(this.nudArmorRating_ValueChanged);
            //
            // lblArmorRatingLabel
            //
            this.lblArmorRatingLabel.AutoSize = true;
            this.lblArmorRatingLabel.Location = new System.Drawing.Point(307, 60);
            this.lblArmorRatingLabel.Name = "lblArmorRatingLabel";
            this.lblArmorRatingLabel.Size = new System.Drawing.Size(41, 13);
            this.lblArmorRatingLabel.TabIndex = 81;
            this.lblArmorRatingLabel.Tag = "Label_Rating";
            this.lblArmorRatingLabel.Text = "Rating:";
            //
            // lblArmorSource
            //
            this.lblArmorSource.AutoSize = true;
            this.lblArmorSource.Location = new System.Drawing.Point(358, 133);
            this.lblArmorSource.Name = "lblArmorSource";
            this.lblArmorSource.Size = new System.Drawing.Size(47, 13);
            this.lblArmorSource.TabIndex = 80;
            this.lblArmorSource.Text = "[Source]";
            this.lblArmorSource.Click += new System.EventHandler(this.lblArmorSource_Click);
            //
            // lblArmorSourceLabel
            //
            this.lblArmorSourceLabel.AutoSize = true;
            this.lblArmorSourceLabel.Location = new System.Drawing.Point(307, 133);
            this.lblArmorSourceLabel.Name = "lblArmorSourceLabel";
            this.lblArmorSourceLabel.Size = new System.Drawing.Size(44, 13);
            this.lblArmorSourceLabel.TabIndex = 79;
            this.lblArmorSourceLabel.Tag = "Label_Source";
            this.lblArmorSourceLabel.Text = "Source:";
            //
            // lblArmorCost
            //
            this.lblArmorCost.AutoSize = true;
            this.lblArmorCost.Location = new System.Drawing.Point(551, 60);
            this.lblArmorCost.Name = "lblArmorCost";
            this.lblArmorCost.Size = new System.Drawing.Size(34, 13);
            this.lblArmorCost.TabIndex = 77;
            this.lblArmorCost.Text = "[Cost]";
            //
            // lblArmorCostLabel
            //
            this.lblArmorCostLabel.AutoSize = true;
            this.lblArmorCostLabel.Location = new System.Drawing.Point(500, 60);
            this.lblArmorCostLabel.Name = "lblArmorCostLabel";
            this.lblArmorCostLabel.Size = new System.Drawing.Size(31, 13);
            this.lblArmorCostLabel.TabIndex = 76;
            this.lblArmorCostLabel.Tag = "Label_Cost";
            this.lblArmorCostLabel.Text = "Cost:";
            //
            // cmdDeleteArmor
            //
            this.cmdDeleteArmor.AutoSize = true;
            this.cmdDeleteArmor.Location = new System.Drawing.Point(96, 7);
            this.cmdDeleteArmor.Name = "cmdDeleteArmor";
            this.cmdDeleteArmor.Size = new System.Drawing.Size(80, 23);
            this.cmdDeleteArmor.TabIndex = 68;
            this.cmdDeleteArmor.Tag = "String_Delete";
            this.cmdDeleteArmor.Text = "Delete";
            this.cmdDeleteArmor.UseVisualStyleBackColor = true;
            this.cmdDeleteArmor.Click += new System.EventHandler(this.cmdDeleteArmor_Click);
            //
            // lblArmorAvail
            //
            this.lblArmorAvail.AutoSize = true;
            this.lblArmorAvail.Location = new System.Drawing.Point(459, 60);
            this.lblArmorAvail.Name = "lblArmorAvail";
            this.lblArmorAvail.Size = new System.Drawing.Size(36, 13);
            this.lblArmorAvail.TabIndex = 75;
            this.lblArmorAvail.Text = "[Avail]";
            //
            // treArmor
            //
            this.treArmor.AllowDrop = true;
            this.treArmor.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)));
            this.treArmor.HideSelection = false;
            this.treArmor.Location = new System.Drawing.Point(6, 36);
            this.treArmor.Name = "treArmor";
            treeNode22.Name = "nodArmorRoot";
            treeNode22.Tag = "Node_SelectedArmor";
            treeNode22.Text = "Selected Armor";
            this.treArmor.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            treeNode22});
            this.treArmor.ShowNodeToolTips = true;
            this.treArmor.Size = new System.Drawing.Size(295, 537);
            this.treArmor.TabIndex = 69;
            this.treArmor.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treArmor_AfterSelect);
            this.treArmor.DragOver += new System.Windows.Forms.DragEventHandler(this.treArmor_DragOver);
            this.treArmor.KeyDown += new System.Windows.Forms.KeyEventHandler(this.treArmor_KeyDown);
            this.treArmor.MouseDown += new System.Windows.Forms.MouseEventHandler(this.TreeView_MouseDown);
            //
            // lblArmorAvailLabel
            //
            this.lblArmorAvailLabel.AutoSize = true;
            this.lblArmorAvailLabel.Location = new System.Drawing.Point(408, 60);
            this.lblArmorAvailLabel.Name = "lblArmorAvailLabel";
            this.lblArmorAvailLabel.Size = new System.Drawing.Size(33, 13);
            this.lblArmorAvailLabel.TabIndex = 74;
            this.lblArmorAvailLabel.Tag = "Label_Avail";
            this.lblArmorAvailLabel.Text = "Avail:";
            //
            // cmdAddArmor
            //
            this.cmdAddArmor.AutoSize = true;
            this.cmdAddArmor.ContextMenuStrip = this.cmsArmor;
            this.cmdAddArmor.Location = new System.Drawing.Point(6, 7);
            this.cmdAddArmor.Name = "cmdAddArmor";
            this.cmdAddArmor.Size = new System.Drawing.Size(84, 23);
            this.cmdAddArmor.SplitMenuStrip = this.cmsArmor;
            this.cmdAddArmor.TabIndex = 131;
            this.cmdAddArmor.Tag = "Button_AddArmor";
            this.cmdAddArmor.Text = "&Add Armor";
            this.cmdAddArmor.UseVisualStyleBackColor = true;
            this.cmdAddArmor.Click += new System.EventHandler(this.cmdAddArmor_Click);
            //
            // tabWeapons
            //
            this.tabWeapons.BackColor = System.Drawing.SystemColors.Control;
            this.tabWeapons.Controls.Add(this.lblWeaponRating);
            this.tabWeapons.Controls.Add(this.lblWeaponRatingLabel);
            this.tabWeapons.Controls.Add(this.lblWeaponFirewall);
            this.tabWeapons.Controls.Add(this.lblWeaponFirewallLabel);
            this.tabWeapons.Controls.Add(this.lblWeaponDataProcessing);
            this.tabWeapons.Controls.Add(this.lblWeaponDataProcessingLabel);
            this.tabWeapons.Controls.Add(this.lblWeaponSleaze);
            this.tabWeapons.Controls.Add(this.lblWeaponSleazeLabel);
            this.tabWeapons.Controls.Add(this.lblWeaponAttack);
            this.tabWeapons.Controls.Add(this.lblWeaponAttackLabel);
            this.tabWeapons.Controls.Add(this.lblWeaponDeviceRating);
            this.tabWeapons.Controls.Add(this.lblWeaponDeviceRatingLabel);
            this.tabWeapons.Controls.Add(this.lblWeaponAccuracyLabel);
            this.tabWeapons.Controls.Add(this.lblWeaponAccuracy);
            this.tabWeapons.Controls.Add(this.cmdAddWeaponLocation);
            this.tabWeapons.Controls.Add(this.lblWeaponDicePool);
            this.tabWeapons.Controls.Add(this.lblWeaponDicePoolLabel);
            this.tabWeapons.Controls.Add(this.lblWeaponConceal);
            this.tabWeapons.Controls.Add(this.lblWeaponConcealLabel);
            this.tabWeapons.Controls.Add(this.lblWeaponRangeExtreme);
            this.tabWeapons.Controls.Add(this.lblWeaponRangeLong);
            this.tabWeapons.Controls.Add(this.lblWeaponRangeMedium);
            this.tabWeapons.Controls.Add(this.lblWeaponRangeShort);
            this.tabWeapons.Controls.Add(this.lblWeaponRangeExtremeLabel);
            this.tabWeapons.Controls.Add(this.lblWeaponRangeLongLabel);
            this.tabWeapons.Controls.Add(this.lblWeaponRangeMediumLabel);
            this.tabWeapons.Controls.Add(this.lblWeaponRangeShortLabel);
            this.tabWeapons.Controls.Add(this.lblWeaponRangeLabel);
            this.tabWeapons.Controls.Add(this.chkIncludedInWeapon);
            this.tabWeapons.Controls.Add(this.chkWeaponAccessoryInstalled);
            this.tabWeapons.Controls.Add(this.lblWeaponSlots);
            this.tabWeapons.Controls.Add(this.lblWeaponSlotsLabel);
            this.tabWeapons.Controls.Add(this.lblWeaponSource);
            this.tabWeapons.Controls.Add(this.lblWeaponSourceLabel);
            this.tabWeapons.Controls.Add(this.lblWeaponAmmo);
            this.tabWeapons.Controls.Add(this.lblWeaponAmmoLabel);
            this.tabWeapons.Controls.Add(this.treWeapons);
            this.tabWeapons.Controls.Add(this.lblWeaponMode);
            this.tabWeapons.Controls.Add(this.cmdDeleteWeapon);
            this.tabWeapons.Controls.Add(this.lblWeaponModeLabel);
            this.tabWeapons.Controls.Add(this.lblWeaponNameLabel);
            this.tabWeapons.Controls.Add(this.lblWeaponReach);
            this.tabWeapons.Controls.Add(this.lblWeaponName);
            this.tabWeapons.Controls.Add(this.lblWeaponReachLabel);
            this.tabWeapons.Controls.Add(this.lblWeaponCategoryLabel);
            this.tabWeapons.Controls.Add(this.lblWeaponAP);
            this.tabWeapons.Controls.Add(this.lblWeaponCategory);
            this.tabWeapons.Controls.Add(this.lblWeaponAPLabel);
            this.tabWeapons.Controls.Add(this.lblWeaponDamageLabel);
            this.tabWeapons.Controls.Add(this.lblWeaponCost);
            this.tabWeapons.Controls.Add(this.lblWeaponDamage);
            this.tabWeapons.Controls.Add(this.lblWeaponCostLabel);
            this.tabWeapons.Controls.Add(this.lblWeaponRCLabel);
            this.tabWeapons.Controls.Add(this.lblWeaponAvail);
            this.tabWeapons.Controls.Add(this.lblWeaponRC);
            this.tabWeapons.Controls.Add(this.lblWeaponAvailLabel);
            this.tabWeapons.Controls.Add(this.cmdAddWeapon);
            this.tabWeapons.Location = new System.Drawing.Point(4, 22);
            this.tabWeapons.Name = "tabWeapons";
            this.tabWeapons.Size = new System.Drawing.Size(827, 579);
            this.tabWeapons.TabIndex = 2;
            this.tabWeapons.Tag = "Tab_Weapons";
            this.tabWeapons.Text = "Weapons";
            //
            // lblWeaponRating
            //
            this.lblWeaponRating.AutoSize = true;
            this.lblWeaponRating.Location = new System.Drawing.Point(371, 195);
            this.lblWeaponRating.Name = "lblWeaponRating";
            this.lblWeaponRating.Size = new System.Drawing.Size(44, 13);
            this.lblWeaponRating.TabIndex = 167;
            this.lblWeaponRating.Text = "[Rating]";
            //
            // lblWeaponRatingLabel
            //
            this.lblWeaponRatingLabel.AutoSize = true;
            this.lblWeaponRatingLabel.Location = new System.Drawing.Point(308, 195);
            this.lblWeaponRatingLabel.Name = "lblWeaponRatingLabel";
            this.lblWeaponRatingLabel.Size = new System.Drawing.Size(41, 13);
            this.lblWeaponRatingLabel.TabIndex = 166;
            this.lblWeaponRatingLabel.Tag = "Label_Rating";
            this.lblWeaponRatingLabel.Text = "Rating:";
            //
            // lblWeaponFirewall
            //
            this.lblWeaponFirewall.AutoSize = true;
            this.lblWeaponFirewall.Location = new System.Drawing.Point(750, 327);
            this.lblWeaponFirewall.Name = "lblWeaponFirewall";
            this.lblWeaponFirewall.Size = new System.Drawing.Size(19, 13);
            this.lblWeaponFirewall.TabIndex = 165;
            this.lblWeaponFirewall.Text = "[0]";
            //
            // lblWeaponFirewallLabel
            //
            this.lblWeaponFirewallLabel.AutoSize = true;
            this.lblWeaponFirewallLabel.Location = new System.Drawing.Point(703, 327);
            this.lblWeaponFirewallLabel.Name = "lblWeaponFirewallLabel";
            this.lblWeaponFirewallLabel.Size = new System.Drawing.Size(45, 13);
            this.lblWeaponFirewallLabel.TabIndex = 164;
            this.lblWeaponFirewallLabel.Tag = "Label_Firewall";
            this.lblWeaponFirewallLabel.Text = "Firewall:";
            //
            // lblWeaponDataProcessing
            //
            this.lblWeaponDataProcessing.AutoSize = true;
            this.lblWeaponDataProcessing.Location = new System.Drawing.Point(678, 327);
            this.lblWeaponDataProcessing.Name = "lblWeaponDataProcessing";
            this.lblWeaponDataProcessing.Size = new System.Drawing.Size(19, 13);
            this.lblWeaponDataProcessing.TabIndex = 163;
            this.lblWeaponDataProcessing.Text = "[0]";
            //
            // lblWeaponDataProcessingLabel
            //
            this.lblWeaponDataProcessingLabel.AutoSize = true;
            this.lblWeaponDataProcessingLabel.Location = new System.Drawing.Point(584, 327);
            this.lblWeaponDataProcessingLabel.Name = "lblWeaponDataProcessingLabel";
            this.lblWeaponDataProcessingLabel.Size = new System.Drawing.Size(88, 13);
            this.lblWeaponDataProcessingLabel.TabIndex = 162;
            this.lblWeaponDataProcessingLabel.Tag = "Label_DataProcessing";
            this.lblWeaponDataProcessingLabel.Text = "Data Processing:";
            //
            // lblWeaponSleaze
            //
            this.lblWeaponSleaze.AutoSize = true;
            this.lblWeaponSleaze.Location = new System.Drawing.Point(559, 327);
            this.lblWeaponSleaze.Name = "lblWeaponSleaze";
            this.lblWeaponSleaze.Size = new System.Drawing.Size(19, 13);
            this.lblWeaponSleaze.TabIndex = 161;
            this.lblWeaponSleaze.Text = "[0]";
            //
            // lblWeaponSleazeLabel
            //
            this.lblWeaponSleazeLabel.AutoSize = true;
            this.lblWeaponSleazeLabel.Location = new System.Drawing.Point(512, 327);
            this.lblWeaponSleazeLabel.Name = "lblWeaponSleazeLabel";
            this.lblWeaponSleazeLabel.Size = new System.Drawing.Size(42, 13);
            this.lblWeaponSleazeLabel.TabIndex = 160;
            this.lblWeaponSleazeLabel.Tag = "Label_Sleaze";
            this.lblWeaponSleazeLabel.Text = "Sleaze:";
            //
            // lblWeaponAttack
            //
            this.lblWeaponAttack.AutoSize = true;
            this.lblWeaponAttack.Location = new System.Drawing.Point(487, 327);
            this.lblWeaponAttack.Name = "lblWeaponAttack";
            this.lblWeaponAttack.Size = new System.Drawing.Size(19, 13);
            this.lblWeaponAttack.TabIndex = 159;
            this.lblWeaponAttack.Text = "[0]";
            //
            // lblWeaponAttackLabel
            //
            this.lblWeaponAttackLabel.AutoSize = true;
            this.lblWeaponAttackLabel.Location = new System.Drawing.Point(440, 327);
            this.lblWeaponAttackLabel.Name = "lblWeaponAttackLabel";
            this.lblWeaponAttackLabel.Size = new System.Drawing.Size(41, 13);
            this.lblWeaponAttackLabel.TabIndex = 158;
            this.lblWeaponAttackLabel.Tag = "Label_Attack";
            this.lblWeaponAttackLabel.Text = "Attack:";
            //
            // lblWeaponDeviceRating
            //
            this.lblWeaponDeviceRating.AutoSize = true;
            this.lblWeaponDeviceRating.Location = new System.Drawing.Point(406, 327);
            this.lblWeaponDeviceRating.Name = "lblWeaponDeviceRating";
            this.lblWeaponDeviceRating.Size = new System.Drawing.Size(19, 13);
            this.lblWeaponDeviceRating.TabIndex = 157;
            this.lblWeaponDeviceRating.Text = "[0]";
            //
            // lblWeaponDeviceRatingLabel
            //
            this.lblWeaponDeviceRatingLabel.AutoSize = true;
            this.lblWeaponDeviceRatingLabel.Location = new System.Drawing.Point(307, 327);
            this.lblWeaponDeviceRatingLabel.Name = "lblWeaponDeviceRatingLabel";
            this.lblWeaponDeviceRatingLabel.Size = new System.Drawing.Size(78, 13);
            this.lblWeaponDeviceRatingLabel.TabIndex = 156;
            this.lblWeaponDeviceRatingLabel.Tag = "Label_DeviceRating";
            this.lblWeaponDeviceRatingLabel.Text = "Device Rating:";
            //
            // lblWeaponAccuracyLabel
            //
            this.lblWeaponAccuracyLabel.AutoSize = true;
            this.lblWeaponAccuracyLabel.Location = new System.Drawing.Point(446, 212);
            this.lblWeaponAccuracyLabel.Name = "lblWeaponAccuracyLabel";
            this.lblWeaponAccuracyLabel.Size = new System.Drawing.Size(55, 13);
            this.lblWeaponAccuracyLabel.TabIndex = 115;
            this.lblWeaponAccuracyLabel.Tag = "Label_Accuracy";
            this.lblWeaponAccuracyLabel.Text = "Accuracy:";
            //
            // lblWeaponAccuracy
            //
            this.lblWeaponAccuracy.AutoSize = true;
            this.lblWeaponAccuracy.Location = new System.Drawing.Point(507, 212);
            this.lblWeaponAccuracy.Name = "lblWeaponAccuracy";
            this.lblWeaponAccuracy.Size = new System.Drawing.Size(58, 13);
            this.lblWeaponAccuracy.TabIndex = 116;
            this.lblWeaponAccuracy.Text = "[Accuracy]";
            //
            // cmdAddWeaponLocation
            //
            this.cmdAddWeaponLocation.AutoSize = true;
            this.cmdAddWeaponLocation.Location = new System.Drawing.Point(221, 7);
            this.cmdAddWeaponLocation.Name = "cmdAddWeaponLocation";
            this.cmdAddWeaponLocation.Size = new System.Drawing.Size(80, 23);
            this.cmdAddWeaponLocation.TabIndex = 114;
            this.cmdAddWeaponLocation.Tag = "Button_AddLocation";
            this.cmdAddWeaponLocation.Text = "Add Location";
            this.cmdAddWeaponLocation.UseVisualStyleBackColor = true;
            this.cmdAddWeaponLocation.Click += new System.EventHandler(this.cmdAddWeaponLocation_Click);
            //
            // lblWeaponDicePool
            //
            this.lblWeaponDicePool.AutoSize = true;
            this.lblWeaponDicePool.Location = new System.Drawing.Point(405, 286);
            this.lblWeaponDicePool.Name = "lblWeaponDicePool";
            this.lblWeaponDicePool.Size = new System.Drawing.Size(34, 13);
            this.lblWeaponDicePool.TabIndex = 107;
            this.lblWeaponDicePool.Text = "[Pool]";
            //
            // lblWeaponDicePoolLabel
            //
            this.lblWeaponDicePoolLabel.AutoSize = true;
            this.lblWeaponDicePoolLabel.Location = new System.Drawing.Point(307, 286);
            this.lblWeaponDicePoolLabel.Name = "lblWeaponDicePoolLabel";
            this.lblWeaponDicePoolLabel.Size = new System.Drawing.Size(56, 13);
            this.lblWeaponDicePoolLabel.TabIndex = 106;
            this.lblWeaponDicePoolLabel.Tag = "Label_DicePool";
            this.lblWeaponDicePoolLabel.Text = "Dice Pool:";
            //
            // lblWeaponConceal
            //
            this.lblWeaponConceal.AutoSize = true;
            this.lblWeaponConceal.Location = new System.Drawing.Point(640, 128);
            this.lblWeaponConceal.Name = "lblWeaponConceal";
            this.lblWeaponConceal.Size = new System.Drawing.Size(52, 13);
            this.lblWeaponConceal.TabIndex = 100;
            this.lblWeaponConceal.Text = "[Conceal]";
            //
            // lblWeaponConcealLabel
            //
            this.lblWeaponConcealLabel.AutoSize = true;
            this.lblWeaponConcealLabel.Location = new System.Drawing.Point(583, 128);
            this.lblWeaponConcealLabel.Name = "lblWeaponConcealLabel";
            this.lblWeaponConcealLabel.Size = new System.Drawing.Size(49, 13);
            this.lblWeaponConcealLabel.TabIndex = 99;
            this.lblWeaponConcealLabel.Tag = "Label_Conceal";
            this.lblWeaponConcealLabel.Text = "Conceal:";
            //
            // lblWeaponRangeExtreme
            //
            this.lblWeaponRangeExtreme.Location = new System.Drawing.Point(519, 252);
            this.lblWeaponRangeExtreme.Name = "lblWeaponRangeExtreme";
            this.lblWeaponRangeExtreme.Size = new System.Drawing.Size(64, 13);
            this.lblWeaponRangeExtreme.TabIndex = 98;
            this.lblWeaponRangeExtreme.Text = "[0]";
            this.lblWeaponRangeExtreme.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            //
            // lblWeaponRangeLong
            //
            this.lblWeaponRangeLong.Location = new System.Drawing.Point(449, 252);
            this.lblWeaponRangeLong.Name = "lblWeaponRangeLong";
            this.lblWeaponRangeLong.Size = new System.Drawing.Size(64, 13);
            this.lblWeaponRangeLong.TabIndex = 97;
            this.lblWeaponRangeLong.Text = "[0]";
            this.lblWeaponRangeLong.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            //
            // lblWeaponRangeMedium
            //
            this.lblWeaponRangeMedium.Location = new System.Drawing.Point(379, 252);
            this.lblWeaponRangeMedium.Name = "lblWeaponRangeMedium";
            this.lblWeaponRangeMedium.Size = new System.Drawing.Size(64, 13);
            this.lblWeaponRangeMedium.TabIndex = 96;
            this.lblWeaponRangeMedium.Text = "[0]";
            this.lblWeaponRangeMedium.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            //
            // lblWeaponRangeShort
            //
            this.lblWeaponRangeShort.Location = new System.Drawing.Point(309, 252);
            this.lblWeaponRangeShort.Name = "lblWeaponRangeShort";
            this.lblWeaponRangeShort.Size = new System.Drawing.Size(64, 13);
            this.lblWeaponRangeShort.TabIndex = 95;
            this.lblWeaponRangeShort.Text = "[0]";
            this.lblWeaponRangeShort.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            //
            // lblWeaponRangeExtremeLabel
            //
            this.lblWeaponRangeExtremeLabel.AutoSize = true;
            this.lblWeaponRangeExtremeLabel.Location = new System.Drawing.Point(521, 235);
            this.lblWeaponRangeExtremeLabel.Name = "lblWeaponRangeExtremeLabel";
            this.lblWeaponRangeExtremeLabel.Size = new System.Drawing.Size(63, 13);
            this.lblWeaponRangeExtremeLabel.TabIndex = 94;
            this.lblWeaponRangeExtremeLabel.Tag = "Label_RangeExtreme";
            this.lblWeaponRangeExtremeLabel.Text = "Extreme (-6)";
            this.lblWeaponRangeExtremeLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            //
            // lblWeaponRangeLongLabel
            //
            this.lblWeaponRangeLongLabel.AutoSize = true;
            this.lblWeaponRangeLongLabel.Location = new System.Drawing.Point(458, 235);
            this.lblWeaponRangeLongLabel.Name = "lblWeaponRangeLongLabel";
            this.lblWeaponRangeLongLabel.Size = new System.Drawing.Size(49, 13);
            this.lblWeaponRangeLongLabel.TabIndex = 93;
            this.lblWeaponRangeLongLabel.Tag = "Label_RangeLong";
            this.lblWeaponRangeLongLabel.Text = "Long (-3)";
            this.lblWeaponRangeLongLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            //
            // lblWeaponRangeMediumLabel
            //
            this.lblWeaponRangeMediumLabel.AutoSize = true;
            this.lblWeaponRangeMediumLabel.Location = new System.Drawing.Point(381, 235);
            this.lblWeaponRangeMediumLabel.Name = "lblWeaponRangeMediumLabel";
            this.lblWeaponRangeMediumLabel.Size = new System.Drawing.Size(62, 13);
            this.lblWeaponRangeMediumLabel.TabIndex = 92;
            this.lblWeaponRangeMediumLabel.Tag = "Label_RangeMedium";
            this.lblWeaponRangeMediumLabel.Text = "Medium (-1)";
            this.lblWeaponRangeMediumLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            //
            // lblWeaponRangeShortLabel
            //
            this.lblWeaponRangeShortLabel.AutoSize = true;
            this.lblWeaponRangeShortLabel.Location = new System.Drawing.Point(317, 235);
            this.lblWeaponRangeShortLabel.Name = "lblWeaponRangeShortLabel";
            this.lblWeaponRangeShortLabel.Size = new System.Drawing.Size(50, 13);
            this.lblWeaponRangeShortLabel.TabIndex = 91;
            this.lblWeaponRangeShortLabel.Tag = "Label_RangeShort";
            this.lblWeaponRangeShortLabel.Text = "Short (-0)";
            this.lblWeaponRangeShortLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            //
            // lblWeaponRangeLabel
            //
            this.lblWeaponRangeLabel.AutoSize = true;
            this.lblWeaponRangeLabel.Location = new System.Drawing.Point(308, 212);
            this.lblWeaponRangeLabel.Name = "lblWeaponRangeLabel";
            this.lblWeaponRangeLabel.Size = new System.Drawing.Size(39, 13);
            this.lblWeaponRangeLabel.TabIndex = 90;
            this.lblWeaponRangeLabel.Tag = "Label_RangeHeading";
            this.lblWeaponRangeLabel.Text = "Range";
            //
            // chkIncludedInWeapon
            //
            this.chkIncludedInWeapon.AutoSize = true;
            this.chkIncludedInWeapon.Enabled = false;
            this.chkIncludedInWeapon.Location = new System.Drawing.Point(448, 150);
            this.chkIncludedInWeapon.Name = "chkIncludedInWeapon";
            this.chkIncludedInWeapon.Size = new System.Drawing.Size(127, 17);
            this.chkIncludedInWeapon.TabIndex = 73;
            this.chkIncludedInWeapon.Tag = "Checkbox_BaseWeapon";
            this.chkIncludedInWeapon.Text = "Part of base Weapon";
            this.chkIncludedInWeapon.UseVisualStyleBackColor = true;
            this.chkIncludedInWeapon.CheckedChanged += new System.EventHandler(this.chkIncludedInWeapon_CheckedChanged);
            //
            // lblWeaponSlots
            //
            this.lblWeaponSlots.AutoSize = true;
            this.lblWeaponSlots.Location = new System.Drawing.Point(371, 174);
            this.lblWeaponSlots.Name = "lblWeaponSlots";
            this.lblWeaponSlots.Size = new System.Drawing.Size(36, 13);
            this.lblWeaponSlots.TabIndex = 71;
            this.lblWeaponSlots.Text = "[Slots]";
            //
            // lblWeaponSlotsLabel
            //
            this.lblWeaponSlotsLabel.AutoSize = true;
            this.lblWeaponSlotsLabel.Location = new System.Drawing.Point(308, 174);
            this.lblWeaponSlotsLabel.Name = "lblWeaponSlotsLabel";
            this.lblWeaponSlotsLabel.Size = new System.Drawing.Size(57, 13);
            this.lblWeaponSlotsLabel.TabIndex = 70;
            this.lblWeaponSlotsLabel.Tag = "Label_ModSlots";
            this.lblWeaponSlotsLabel.Text = "Mod Slots:";
            //
            // lblWeaponSource
            //
            this.lblWeaponSource.AutoSize = true;
            this.lblWeaponSource.Location = new System.Drawing.Point(372, 151);
            this.lblWeaponSource.Name = "lblWeaponSource";
            this.lblWeaponSource.Size = new System.Drawing.Size(47, 13);
            this.lblWeaponSource.TabIndex = 69;
            this.lblWeaponSource.Text = "[Source]";
            this.lblWeaponSource.Click += new System.EventHandler(this.lblWeaponSource_Click);
            //
            // lblWeaponSourceLabel
            //
            this.lblWeaponSourceLabel.AutoSize = true;
            this.lblWeaponSourceLabel.Location = new System.Drawing.Point(309, 151);
            this.lblWeaponSourceLabel.Name = "lblWeaponSourceLabel";
            this.lblWeaponSourceLabel.Size = new System.Drawing.Size(44, 13);
            this.lblWeaponSourceLabel.TabIndex = 68;
            this.lblWeaponSourceLabel.Tag = "Label_Source";
            this.lblWeaponSourceLabel.Text = "Source:";
            //
            // lblWeaponAmmo
            //
            this.lblWeaponAmmo.AutoSize = true;
            this.lblWeaponAmmo.Location = new System.Drawing.Point(640, 105);
            this.lblWeaponAmmo.Name = "lblWeaponAmmo";
            this.lblWeaponAmmo.Size = new System.Drawing.Size(42, 13);
            this.lblWeaponAmmo.TabIndex = 67;
            this.lblWeaponAmmo.Text = "[Ammo]";
            //
            // lblWeaponAmmoLabel
            //
            this.lblWeaponAmmoLabel.AutoSize = true;
            this.lblWeaponAmmoLabel.Location = new System.Drawing.Point(583, 105);
            this.lblWeaponAmmoLabel.Name = "lblWeaponAmmoLabel";
            this.lblWeaponAmmoLabel.Size = new System.Drawing.Size(39, 13);
            this.lblWeaponAmmoLabel.TabIndex = 66;
            this.lblWeaponAmmoLabel.Tag = "Label_Ammo";
            this.lblWeaponAmmoLabel.Text = "Ammo:";
            //
            // treWeapons
            //
            this.treWeapons.AllowDrop = true;
            this.treWeapons.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)));
            this.treWeapons.HideSelection = false;
            this.treWeapons.Location = new System.Drawing.Point(6, 36);
            this.treWeapons.Name = "treWeapons";
            treeNode23.Name = "nodWeaponsRoot";
            treeNode23.Tag = "Node_SelectedWeapons";
            treeNode23.Text = "Selected Weapons";
            this.treWeapons.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            treeNode23});
            this.treWeapons.ShowNodeToolTips = true;
            this.treWeapons.Size = new System.Drawing.Size(295, 540);
            this.treWeapons.TabIndex = 29;
            this.treWeapons.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treWeapons_AfterSelect);
            this.treWeapons.DragOver += new System.Windows.Forms.DragEventHandler(this.treWeapons_DragOver);
            this.treWeapons.KeyDown += new System.Windows.Forms.KeyEventHandler(this.treWeapons_KeyDown);
            this.treWeapons.MouseDown += new System.Windows.Forms.MouseEventHandler(this.TreeView_MouseDown);
            //
            // lblWeaponMode
            //
            this.lblWeaponMode.AutoSize = true;
            this.lblWeaponMode.Location = new System.Drawing.Point(488, 105);
            this.lblWeaponMode.Name = "lblWeaponMode";
            this.lblWeaponMode.Size = new System.Drawing.Size(40, 13);
            this.lblWeaponMode.TabIndex = 65;
            this.lblWeaponMode.Text = "[Mode]";
            //
            // cmdDeleteWeapon
            //
            this.cmdDeleteWeapon.AutoSize = true;
            this.cmdDeleteWeapon.Location = new System.Drawing.Point(110, 7);
            this.cmdDeleteWeapon.Name = "cmdDeleteWeapon";
            this.cmdDeleteWeapon.Size = new System.Drawing.Size(80, 23);
            this.cmdDeleteWeapon.TabIndex = 46;
            this.cmdDeleteWeapon.Tag = "String_Delete";
            this.cmdDeleteWeapon.Text = "Delete";
            this.cmdDeleteWeapon.UseVisualStyleBackColor = true;
            this.cmdDeleteWeapon.Click += new System.EventHandler(this.cmdDeleteWeapon_Click);
            //
            // lblWeaponModeLabel
            //
            this.lblWeaponModeLabel.AutoSize = true;
            this.lblWeaponModeLabel.Location = new System.Drawing.Point(445, 105);
            this.lblWeaponModeLabel.Name = "lblWeaponModeLabel";
            this.lblWeaponModeLabel.Size = new System.Drawing.Size(37, 13);
            this.lblWeaponModeLabel.TabIndex = 64;
            this.lblWeaponModeLabel.Tag = "Label_Mode";
            this.lblWeaponModeLabel.Text = "Mode:";
            //
            // lblWeaponNameLabel
            //
            this.lblWeaponNameLabel.AutoSize = true;
            this.lblWeaponNameLabel.Location = new System.Drawing.Point(307, 36);
            this.lblWeaponNameLabel.Name = "lblWeaponNameLabel";
            this.lblWeaponNameLabel.Size = new System.Drawing.Size(38, 13);
            this.lblWeaponNameLabel.TabIndex = 47;
            this.lblWeaponNameLabel.Tag = "Label_Name";
            this.lblWeaponNameLabel.Text = "Name:";
            //
            // lblWeaponReach
            //
            this.lblWeaponReach.AutoSize = true;
            this.lblWeaponReach.Location = new System.Drawing.Point(370, 105);
            this.lblWeaponReach.Name = "lblWeaponReach";
            this.lblWeaponReach.Size = new System.Drawing.Size(45, 13);
            this.lblWeaponReach.TabIndex = 63;
            this.lblWeaponReach.Text = "[Reach]";
            //
            // lblWeaponName
            //
            this.lblWeaponName.AutoSize = true;
            this.lblWeaponName.Location = new System.Drawing.Point(370, 36);
            this.lblWeaponName.Name = "lblWeaponName";
            this.lblWeaponName.Size = new System.Drawing.Size(41, 13);
            this.lblWeaponName.TabIndex = 48;
            this.lblWeaponName.Text = "[Name]";
            //
            // lblWeaponReachLabel
            //
            this.lblWeaponReachLabel.AutoSize = true;
            this.lblWeaponReachLabel.Location = new System.Drawing.Point(307, 105);
            this.lblWeaponReachLabel.Name = "lblWeaponReachLabel";
            this.lblWeaponReachLabel.Size = new System.Drawing.Size(42, 13);
            this.lblWeaponReachLabel.TabIndex = 62;
            this.lblWeaponReachLabel.Tag = "Label_Reach";
            this.lblWeaponReachLabel.Text = "Reach:";
            //
            // lblWeaponCategoryLabel
            //
            this.lblWeaponCategoryLabel.AutoSize = true;
            this.lblWeaponCategoryLabel.Location = new System.Drawing.Point(307, 59);
            this.lblWeaponCategoryLabel.Name = "lblWeaponCategoryLabel";
            this.lblWeaponCategoryLabel.Size = new System.Drawing.Size(52, 13);
            this.lblWeaponCategoryLabel.TabIndex = 49;
            this.lblWeaponCategoryLabel.Tag = "Label_Category";
            this.lblWeaponCategoryLabel.Text = "Category:";
            //
            // lblWeaponAP
            //
            this.lblWeaponAP.AutoSize = true;
            this.lblWeaponAP.Location = new System.Drawing.Point(640, 82);
            this.lblWeaponAP.Name = "lblWeaponAP";
            this.lblWeaponAP.Size = new System.Drawing.Size(27, 13);
            this.lblWeaponAP.TabIndex = 61;
            this.lblWeaponAP.Text = "[AP]";
            //
            // lblWeaponCategory
            //
            this.lblWeaponCategory.AutoSize = true;
            this.lblWeaponCategory.Location = new System.Drawing.Point(370, 59);
            this.lblWeaponCategory.Name = "lblWeaponCategory";
            this.lblWeaponCategory.Size = new System.Drawing.Size(55, 13);
            this.lblWeaponCategory.TabIndex = 50;
            this.lblWeaponCategory.Text = "[Category]";
            //
            // lblWeaponAPLabel
            //
            this.lblWeaponAPLabel.AutoSize = true;
            this.lblWeaponAPLabel.Location = new System.Drawing.Point(583, 82);
            this.lblWeaponAPLabel.Name = "lblWeaponAPLabel";
            this.lblWeaponAPLabel.Size = new System.Drawing.Size(24, 13);
            this.lblWeaponAPLabel.TabIndex = 60;
            this.lblWeaponAPLabel.Tag = "Label_AP";
            this.lblWeaponAPLabel.Text = "AP:";
            //
            // lblWeaponDamageLabel
            //
            this.lblWeaponDamageLabel.AutoSize = true;
            this.lblWeaponDamageLabel.Location = new System.Drawing.Point(307, 82);
            this.lblWeaponDamageLabel.Name = "lblWeaponDamageLabel";
            this.lblWeaponDamageLabel.Size = new System.Drawing.Size(50, 13);
            this.lblWeaponDamageLabel.TabIndex = 51;
            this.lblWeaponDamageLabel.Tag = "Label_Damage";
            this.lblWeaponDamageLabel.Text = "Damage:";
            //
            // lblWeaponCost
            //
            this.lblWeaponCost.AutoSize = true;
            this.lblWeaponCost.Location = new System.Drawing.Point(488, 128);
            this.lblWeaponCost.Name = "lblWeaponCost";
            this.lblWeaponCost.Size = new System.Drawing.Size(34, 13);
            this.lblWeaponCost.TabIndex = 59;
            this.lblWeaponCost.Text = "[Cost]";
            //
            // lblWeaponDamage
            //
            this.lblWeaponDamage.AutoSize = true;
            this.lblWeaponDamage.Location = new System.Drawing.Point(370, 82);
            this.lblWeaponDamage.Name = "lblWeaponDamage";
            this.lblWeaponDamage.Size = new System.Drawing.Size(53, 13);
            this.lblWeaponDamage.TabIndex = 52;
            this.lblWeaponDamage.Text = "[Damage]";
            //
            // lblWeaponCostLabel
            //
            this.lblWeaponCostLabel.AutoSize = true;
            this.lblWeaponCostLabel.Location = new System.Drawing.Point(445, 128);
            this.lblWeaponCostLabel.Name = "lblWeaponCostLabel";
            this.lblWeaponCostLabel.Size = new System.Drawing.Size(31, 13);
            this.lblWeaponCostLabel.TabIndex = 58;
            this.lblWeaponCostLabel.Tag = "Label_Cost";
            this.lblWeaponCostLabel.Text = "Cost:";
            //
            // lblWeaponRCLabel
            //
            this.lblWeaponRCLabel.AutoSize = true;
            this.lblWeaponRCLabel.Location = new System.Drawing.Point(445, 82);
            this.lblWeaponRCLabel.Name = "lblWeaponRCLabel";
            this.lblWeaponRCLabel.Size = new System.Drawing.Size(25, 13);
            this.lblWeaponRCLabel.TabIndex = 53;
            this.lblWeaponRCLabel.Tag = "Label_RC";
            this.lblWeaponRCLabel.Text = "RC:";
            //
            // lblWeaponAvail
            //
            this.lblWeaponAvail.AutoSize = true;
            this.lblWeaponAvail.Location = new System.Drawing.Point(370, 128);
            this.lblWeaponAvail.Name = "lblWeaponAvail";
            this.lblWeaponAvail.Size = new System.Drawing.Size(36, 13);
            this.lblWeaponAvail.TabIndex = 57;
            this.lblWeaponAvail.Text = "[Avail]";
            //
            // lblWeaponRC
            //
            this.lblWeaponRC.AutoSize = true;
            this.lblWeaponRC.Location = new System.Drawing.Point(488, 82);
            this.lblWeaponRC.Name = "lblWeaponRC";
            this.lblWeaponRC.Size = new System.Drawing.Size(28, 13);
            this.lblWeaponRC.TabIndex = 54;
            this.lblWeaponRC.Text = "[RC]";
            //
            // lblWeaponAvailLabel
            //
            this.lblWeaponAvailLabel.AutoSize = true;
            this.lblWeaponAvailLabel.Location = new System.Drawing.Point(307, 128);
            this.lblWeaponAvailLabel.Name = "lblWeaponAvailLabel";
            this.lblWeaponAvailLabel.Size = new System.Drawing.Size(33, 13);
            this.lblWeaponAvailLabel.TabIndex = 56;
            this.lblWeaponAvailLabel.Tag = "Label_Avail";
            this.lblWeaponAvailLabel.Text = "Avail:";
            //
            // cmdAddWeapon
            //
            this.cmdAddWeapon.AutoSize = true;
            this.cmdAddWeapon.ContextMenuStrip = this.cmsWeapon;
            this.cmdAddWeapon.Location = new System.Drawing.Point(6, 7);
            this.cmdAddWeapon.Name = "cmdAddWeapon";
            this.cmdAddWeapon.Size = new System.Drawing.Size(98, 23);
            this.cmdAddWeapon.SplitMenuStrip = this.cmsWeapon;
            this.cmdAddWeapon.TabIndex = 154;
            this.cmdAddWeapon.Tag = "Button_AddWeapon";
            this.cmdAddWeapon.Text = "&Add Weapon";
            this.cmdAddWeapon.UseVisualStyleBackColor = true;
            this.cmdAddWeapon.Click += new System.EventHandler(this.cmdAddWeapon_Click);
            //
            // tabGear
            //
            this.tabGear.BackColor = System.Drawing.SystemColors.Control;
            this.tabGear.Controls.Add(this.cboGearDataProcessing);
            this.tabGear.Controls.Add(this.cboGearFirewall);
            this.tabGear.Controls.Add(this.cboGearSleaze);
            this.tabGear.Controls.Add(this.cboGearAttack);
            this.tabGear.Controls.Add(this.lblGearFirewallLabel);
            this.tabGear.Controls.Add(this.lblGearDataProcessingLabel);
            this.tabGear.Controls.Add(this.lblGearSleazeLabel);
            this.tabGear.Controls.Add(this.lblGearAttackLabel);
            this.tabGear.Controls.Add(this.nudAdeptWayDiscount);
            this.tabGear.Controls.Add(this.lblAdeptWayDiscount);
            this.tabGear.Controls.Add(this.chkActiveCommlink);
            this.tabGear.Controls.Add(this.chkCommlinks);
            this.tabGear.Controls.Add(this.cmdCreateStackedFocus);
            this.tabGear.Controls.Add(this.chkGearHomeNode);
            this.tabGear.Controls.Add(this.lblGearAP);
            this.tabGear.Controls.Add(this.lblGearAPLabel);
            this.tabGear.Controls.Add(this.lblGearDamage);
            this.tabGear.Controls.Add(this.lblGearDamageLabel);
            this.tabGear.Controls.Add(this.cmdAddLocation);
            this.tabGear.Controls.Add(this.chkGearEquipped);
            this.tabGear.Controls.Add(this.lblFoci);
            this.tabGear.Controls.Add(this.treFoci);
            this.tabGear.Controls.Add(this.lblGearSource);
            this.tabGear.Controls.Add(this.lblGearSourceLabel);
            this.tabGear.Controls.Add(this.lblGearDeviceRating);
            this.tabGear.Controls.Add(this.lblGearDeviceRatingLabel);
            this.tabGear.Controls.Add(this.nudGearQty);
            this.tabGear.Controls.Add(this.lblGearQtyLabel);
            this.tabGear.Controls.Add(this.lblGearCost);
            this.tabGear.Controls.Add(this.lblGearCostLabel);
            this.tabGear.Controls.Add(this.lblGearAvail);
            this.tabGear.Controls.Add(this.lblGearAvailLabel);
            this.tabGear.Controls.Add(this.lblGearCapacity);
            this.tabGear.Controls.Add(this.lblGearCapacityLabel);
            this.tabGear.Controls.Add(this.lblGearCategory);
            this.tabGear.Controls.Add(this.lblGearCategoryLabel);
            this.tabGear.Controls.Add(this.lblGearName);
            this.tabGear.Controls.Add(this.lblGearNameLabel);
            this.tabGear.Controls.Add(this.nudGearRating);
            this.tabGear.Controls.Add(this.lblGearRatingLabel);
            this.tabGear.Controls.Add(this.treGear);
            this.tabGear.Controls.Add(this.cmdDeleteGear);
            this.tabGear.Controls.Add(this.cmdAddGear);
            this.tabGear.Location = new System.Drawing.Point(4, 22);
            this.tabGear.Name = "tabGear";
            this.tabGear.Size = new System.Drawing.Size(827, 579);
            this.tabGear.TabIndex = 3;
            this.tabGear.Tag = "Tab_Gear";
            this.tabGear.Text = "Gear";
            //
            // cboGearDataProcessing
            //
            this.cboGearDataProcessing.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboGearDataProcessing.FormattingEnabled = true;
            this.cboGearDataProcessing.Location = new System.Drawing.Point(667, 180);
            this.cboGearDataProcessing.Name = "cboGearDataProcessing";
            this.cboGearDataProcessing.Size = new System.Drawing.Size(60, 21);
            this.cboGearDataProcessing.TabIndex = 159;
            this.cboGearDataProcessing.Visible = false;
            this.cboGearDataProcessing.SelectedIndexChanged += new System.EventHandler(this.cboGearDataProcessing_SelectedIndexChanged);
            //
            // cboGearFirewall
            //
            this.cboGearFirewall.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboGearFirewall.FormattingEnabled = true;
            this.cboGearFirewall.Location = new System.Drawing.Point(733, 180);
            this.cboGearFirewall.Name = "cboGearFirewall";
            this.cboGearFirewall.Size = new System.Drawing.Size(60, 21);
            this.cboGearFirewall.TabIndex = 158;
            this.cboGearFirewall.Visible = false;
            this.cboGearFirewall.SelectedIndexChanged += new System.EventHandler(this.cboGearFirewall_SelectedIndexChanged);
            //
            // cboGearSleaze
            //
            this.cboGearSleaze.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboGearSleaze.FormattingEnabled = true;
            this.cboGearSleaze.Location = new System.Drawing.Point(598, 180);
            this.cboGearSleaze.Name = "cboGearSleaze";
            this.cboGearSleaze.Size = new System.Drawing.Size(60, 21);
            this.cboGearSleaze.TabIndex = 157;
            this.cboGearSleaze.Visible = false;
            this.cboGearSleaze.SelectedIndexChanged += new System.EventHandler(this.cboGearSleaze_SelectedIndexChanged);
            //
            // cboGearAttack
            //
            this.cboGearAttack.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboGearAttack.FormattingEnabled = true;
            this.cboGearAttack.Location = new System.Drawing.Point(531, 180);
            this.cboGearAttack.Name = "cboGearAttack";
            this.cboGearAttack.Size = new System.Drawing.Size(60, 21);
            this.cboGearAttack.TabIndex = 156;
            this.cboGearAttack.Visible = false;
            this.cboGearAttack.SelectedIndexChanged += new System.EventHandler(this.cboGearAttack_SelectedIndexChanged);
            //
            // lblGearFirewallLabel
            //
            this.lblGearFirewallLabel.Location = new System.Drawing.Point(733, 166);
            this.lblGearFirewallLabel.Name = "lblGearFirewallLabel";
            this.lblGearFirewallLabel.Size = new System.Drawing.Size(60, 13);
            this.lblGearFirewallLabel.TabIndex = 154;
            this.lblGearFirewallLabel.Tag = "Label_Firewall";
            this.lblGearFirewallLabel.Text = "Firewall:";
            this.lblGearFirewallLabel.Visible = false;
            //
            // lblGearDataProcessingLabel
            //
            this.lblGearDataProcessingLabel.Location = new System.Drawing.Point(667, 166);
            this.lblGearDataProcessingLabel.Name = "lblGearDataProcessingLabel";
            this.lblGearDataProcessingLabel.Size = new System.Drawing.Size(60, 13);
            this.lblGearDataProcessingLabel.TabIndex = 152;
            this.lblGearDataProcessingLabel.Tag = "Label_DataProcessing";
            this.lblGearDataProcessingLabel.Text = "Data Proc:";
            this.lblGearDataProcessingLabel.Visible = false;
            //
            // lblGearSleazeLabel
            //
            this.lblGearSleazeLabel.Location = new System.Drawing.Point(598, 166);
            this.lblGearSleazeLabel.Name = "lblGearSleazeLabel";
            this.lblGearSleazeLabel.Size = new System.Drawing.Size(60, 13);
            this.lblGearSleazeLabel.TabIndex = 150;
            this.lblGearSleazeLabel.Tag = "Label_Sleaze";
            this.lblGearSleazeLabel.Text = "Sleaze:";
            this.lblGearSleazeLabel.Visible = false;
            //
            // lblGearAttackLabel
            //
            this.lblGearAttackLabel.Location = new System.Drawing.Point(531, 166);
            this.lblGearAttackLabel.Name = "lblGearAttackLabel";
            this.lblGearAttackLabel.Size = new System.Drawing.Size(60, 13);
            this.lblGearAttackLabel.TabIndex = 148;
            this.lblGearAttackLabel.Tag = "Label_Attack";
            this.lblGearAttackLabel.Text = "Attack:";
            this.lblGearAttackLabel.Visible = false;
            //
            // nudAdeptWayDiscount
            //
            this.nudAdeptWayDiscount.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.nudAdeptWayDiscount.Location = new System.Drawing.Point(571, 552);
            this.nudAdeptWayDiscount.Maximum = new decimal(new int[] {
            9000,
            0,
            0,
            0});
            this.nudAdeptWayDiscount.Name = "nudAdeptWayDiscount";
            this.nudAdeptWayDiscount.Size = new System.Drawing.Size(48, 20);
            this.nudAdeptWayDiscount.TabIndex = 147;
            this.nudAdeptWayDiscount.ValueChanged += new System.EventHandler(this.nudAdeptWayDiscount_ValueChanged);
            //
            // lblAdeptWayDiscount
            //
            this.lblAdeptWayDiscount.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblAdeptWayDiscount.AutoSize = true;
            this.lblAdeptWayDiscount.Location = new System.Drawing.Point(307, 554);
            this.lblAdeptWayDiscount.Name = "lblAdeptWayDiscount";
            this.lblAdeptWayDiscount.Size = new System.Drawing.Size(235, 13);
            this.lblAdeptWayDiscount.TabIndex = 146;
            this.lblAdeptWayDiscount.Tag = "Label_AdeptWayDiscount";
            this.lblAdeptWayDiscount.Text = "Bonded Foci Qualifying for Adept Way Discount:";
            //
            // chkActiveCommlink
            //
            this.chkActiveCommlink.AutoSize = true;
            this.chkActiveCommlink.Location = new System.Drawing.Point(310, 276);
            this.chkActiveCommlink.Name = "chkActiveCommlink";
            this.chkActiveCommlink.Size = new System.Drawing.Size(104, 17);
            this.chkActiveCommlink.TabIndex = 115;
            this.chkActiveCommlink.Tag = "Checkbox_ActiveCommlink";
            this.chkActiveCommlink.Text = "Active Commlink";
            this.chkActiveCommlink.UseVisualStyleBackColor = true;
            this.chkActiveCommlink.Visible = false;
            this.chkActiveCommlink.CheckedChanged += new System.EventHandler(this.chkActiveCommlink_CheckedChanged);
            //
            // chkCommlinks
            //
            this.chkCommlinks.AutoSize = true;
            this.chkCommlinks.Location = new System.Drawing.Point(307, 11);
            this.chkCommlinks.Name = "chkCommlinks";
            this.chkCommlinks.Size = new System.Drawing.Size(128, 17);
            this.chkCommlinks.TabIndex = 114;
            this.chkCommlinks.Tag = "Checkbox_Commlinks";
            this.chkCommlinks.Text = "Only show Commlinks";
            this.chkCommlinks.UseVisualStyleBackColor = true;
            this.chkCommlinks.CheckedChanged += new System.EventHandler(this.chkCommlinks_CheckedChanged);
            //
            // cmdCreateStackedFocus
            //
            this.cmdCreateStackedFocus.AutoSize = true;
            this.cmdCreateStackedFocus.Location = new System.Drawing.Point(624, 348);
            this.cmdCreateStackedFocus.Name = "cmdCreateStackedFocus";
            this.cmdCreateStackedFocus.Size = new System.Drawing.Size(123, 23);
            this.cmdCreateStackedFocus.TabIndex = 109;
            this.cmdCreateStackedFocus.Tag = "Button_CreateStackedFocus";
            this.cmdCreateStackedFocus.Text = "Create Stacked Focus";
            this.cmdCreateStackedFocus.UseVisualStyleBackColor = true;
            this.cmdCreateStackedFocus.Visible = false;
            this.cmdCreateStackedFocus.Click += new System.EventHandler(this.cmdCreateStackedFocus_Click);
            //
            // chkGearHomeNode
            //
            this.chkGearHomeNode.AutoSize = true;
            this.chkGearHomeNode.Location = new System.Drawing.Point(409, 253);
            this.chkGearHomeNode.Name = "chkGearHomeNode";
            this.chkGearHomeNode.Size = new System.Drawing.Size(83, 17);
            this.chkGearHomeNode.TabIndex = 108;
            this.chkGearHomeNode.Tag = "Checkbox_HomeNode";
            this.chkGearHomeNode.Text = "Home Node";
            this.chkGearHomeNode.UseVisualStyleBackColor = true;
            this.chkGearHomeNode.Visible = false;
            this.chkGearHomeNode.CheckedChanged += new System.EventHandler(this.chkGearHomeNode_CheckedChanged);
            //
            // lblGearAP
            //
            this.lblGearAP.AutoSize = true;
            this.lblGearAP.Location = new System.Drawing.Point(451, 189);
            this.lblGearAP.Name = "lblGearAP";
            this.lblGearAP.Size = new System.Drawing.Size(19, 13);
            this.lblGearAP.TabIndex = 107;
            this.lblGearAP.Text = "[0]";
            //
            // lblGearAPLabel
            //
            this.lblGearAPLabel.AutoSize = true;
            this.lblGearAPLabel.Location = new System.Drawing.Point(406, 189);
            this.lblGearAPLabel.Name = "lblGearAPLabel";
            this.lblGearAPLabel.Size = new System.Drawing.Size(24, 13);
            this.lblGearAPLabel.TabIndex = 106;
            this.lblGearAPLabel.Tag = "Label_AP";
            this.lblGearAPLabel.Text = "AP:";
            //
            // lblGearDamage
            //
            this.lblGearDamage.AutoSize = true;
            this.lblGearDamage.Location = new System.Drawing.Point(371, 189);
            this.lblGearDamage.Name = "lblGearDamage";
            this.lblGearDamage.Size = new System.Drawing.Size(19, 13);
            this.lblGearDamage.TabIndex = 105;
            this.lblGearDamage.Text = "[0]";
            //
            // lblGearDamageLabel
            //
            this.lblGearDamageLabel.AutoSize = true;
            this.lblGearDamageLabel.Location = new System.Drawing.Point(307, 189);
            this.lblGearDamageLabel.Name = "lblGearDamageLabel";
            this.lblGearDamageLabel.Size = new System.Drawing.Size(50, 13);
            this.lblGearDamageLabel.TabIndex = 104;
            this.lblGearDamageLabel.Tag = "Label_Damage";
            this.lblGearDamageLabel.Text = "Damage:";
            //
            // cmdAddLocation
            //
            this.cmdAddLocation.AutoSize = true;
            this.cmdAddLocation.Location = new System.Drawing.Point(221, 7);
            this.cmdAddLocation.Name = "cmdAddLocation";
            this.cmdAddLocation.Size = new System.Drawing.Size(80, 23);
            this.cmdAddLocation.TabIndex = 103;
            this.cmdAddLocation.Tag = "Button_AddLocation";
            this.cmdAddLocation.Text = "Add Location";
            this.cmdAddLocation.UseVisualStyleBackColor = true;
            this.cmdAddLocation.Click += new System.EventHandler(this.cmdAddLocation_Click);
            //
            // chkGearEquipped
            //
            this.chkGearEquipped.AutoSize = true;
            this.chkGearEquipped.Location = new System.Drawing.Point(310, 253);
            this.chkGearEquipped.Name = "chkGearEquipped";
            this.chkGearEquipped.Size = new System.Drawing.Size(71, 17);
            this.chkGearEquipped.TabIndex = 93;
            this.chkGearEquipped.Tag = "Checkbox_Equipped";
            this.chkGearEquipped.Text = "Equipped";
            this.chkGearEquipped.UseVisualStyleBackColor = true;
            this.chkGearEquipped.Visible = false;
            this.chkGearEquipped.CheckedChanged += new System.EventHandler(this.chkGearEquipped_CheckedChanged);
            //
            // treFoci
            //
            this.treFoci.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)));
            this.treFoci.CheckBoxes = true;
            this.treFoci.Location = new System.Drawing.Point(310, 348);
            this.treFoci.Name = "treFoci";
            this.treFoci.ShowLines = false;
            this.treFoci.ShowPlusMinus = false;
            this.treFoci.ShowRootLines = false;
            this.treFoci.Size = new System.Drawing.Size(308, 198);
            this.treFoci.TabIndex = 91;
            this.treFoci.BeforeCheck += new System.Windows.Forms.TreeViewCancelEventHandler(this.treFoci_BeforeCheck);
            this.treFoci.AfterCheck += new System.Windows.Forms.TreeViewEventHandler(this.treFoci_AfterCheck);
            this.treFoci.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treFoci_AfterSelect);
            //
            // lblGearSource
            //
            this.lblGearSource.AutoSize = true;
            this.lblGearSource.Location = new System.Drawing.Point(358, 227);
            this.lblGearSource.Name = "lblGearSource";
            this.lblGearSource.Size = new System.Drawing.Size(47, 13);
            this.lblGearSource.TabIndex = 74;
            this.lblGearSource.Text = "[Source]";
            this.lblGearSource.Click += new System.EventHandler(this.lblGearSource_Click);
            //
            // lblGearSourceLabel
            //
            this.lblGearSourceLabel.AutoSize = true;
            this.lblGearSourceLabel.Location = new System.Drawing.Point(307, 227);
            this.lblGearSourceLabel.Name = "lblGearSourceLabel";
            this.lblGearSourceLabel.Size = new System.Drawing.Size(44, 13);
            this.lblGearSourceLabel.TabIndex = 73;
            this.lblGearSourceLabel.Tag = "Label_Source";
            this.lblGearSourceLabel.Text = "Source:";
            //
            // lblGearDeviceRating
            //
            this.lblGearDeviceRating.AutoSize = true;
            this.lblGearDeviceRating.Location = new System.Drawing.Point(406, 166);
            this.lblGearDeviceRating.Name = "lblGearDeviceRating";
            this.lblGearDeviceRating.Size = new System.Drawing.Size(19, 13);
            this.lblGearDeviceRating.TabIndex = 66;
            this.lblGearDeviceRating.Text = "[0]";
            //
            // lblGearDeviceRatingLabel
            //
            this.lblGearDeviceRatingLabel.AutoSize = true;
            this.lblGearDeviceRatingLabel.Location = new System.Drawing.Point(307, 166);
            this.lblGearDeviceRatingLabel.Name = "lblGearDeviceRatingLabel";
            this.lblGearDeviceRatingLabel.Size = new System.Drawing.Size(78, 13);
            this.lblGearDeviceRatingLabel.TabIndex = 65;
            this.lblGearDeviceRatingLabel.Tag = "Label_DeviceRating";
            this.lblGearDeviceRatingLabel.Text = "Device Rating:";
            //
            // nudGearQty
            //
            this.nudGearQty.Enabled = false;
            this.nudGearQty.Location = new System.Drawing.Point(368, 126);
            this.nudGearQty.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.nudGearQty.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudGearQty.Name = "nudGearQty";
            this.nudGearQty.Size = new System.Drawing.Size(48, 20);
            this.nudGearQty.TabIndex = 64;
            this.nudGearQty.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudGearQty.ValueChanged += new System.EventHandler(this.nudGearQty_ValueChanged);
            //
            // lblGearQtyLabel
            //
            this.lblGearQtyLabel.AutoSize = true;
            this.lblGearQtyLabel.Location = new System.Drawing.Point(307, 128);
            this.lblGearQtyLabel.Name = "lblGearQtyLabel";
            this.lblGearQtyLabel.Size = new System.Drawing.Size(26, 13);
            this.lblGearQtyLabel.TabIndex = 63;
            this.lblGearQtyLabel.Tag = "Label_Qty";
            this.lblGearQtyLabel.Text = "Qty:";
            //
            // lblGearCost
            //
            this.lblGearCost.AutoSize = true;
            this.lblGearCost.Location = new System.Drawing.Point(568, 82);
            this.lblGearCost.Name = "lblGearCost";
            this.lblGearCost.Size = new System.Drawing.Size(34, 13);
            this.lblGearCost.TabIndex = 62;
            this.lblGearCost.Text = "[Cost]";
            //
            // lblGearCostLabel
            //
            this.lblGearCostLabel.AutoSize = true;
            this.lblGearCostLabel.Location = new System.Drawing.Point(531, 82);
            this.lblGearCostLabel.Name = "lblGearCostLabel";
            this.lblGearCostLabel.Size = new System.Drawing.Size(31, 13);
            this.lblGearCostLabel.TabIndex = 61;
            this.lblGearCostLabel.Tag = "Label_Cost";
            this.lblGearCostLabel.Text = "Cost:";
            //
            // lblGearAvail
            //
            this.lblGearAvail.AutoSize = true;
            this.lblGearAvail.Location = new System.Drawing.Point(456, 82);
            this.lblGearAvail.Name = "lblGearAvail";
            this.lblGearAvail.Size = new System.Drawing.Size(36, 13);
            this.lblGearAvail.TabIndex = 60;
            this.lblGearAvail.Text = "[Avail]";
            //
            // lblGearAvailLabel
            //
            this.lblGearAvailLabel.AutoSize = true;
            this.lblGearAvailLabel.Location = new System.Drawing.Point(417, 82);
            this.lblGearAvailLabel.Name = "lblGearAvailLabel";
            this.lblGearAvailLabel.Size = new System.Drawing.Size(33, 13);
            this.lblGearAvailLabel.TabIndex = 59;
            this.lblGearAvailLabel.Tag = "Label_Avail";
            this.lblGearAvailLabel.Text = "Avail:";
            //
            // lblGearCapacity
            //
            this.lblGearCapacity.AutoSize = true;
            this.lblGearCapacity.Location = new System.Drawing.Point(365, 105);
            this.lblGearCapacity.Name = "lblGearCapacity";
            this.lblGearCapacity.Size = new System.Drawing.Size(54, 13);
            this.lblGearCapacity.TabIndex = 58;
            this.lblGearCapacity.Text = "[Capacity]";
            //
            // lblGearCapacityLabel
            //
            this.lblGearCapacityLabel.AutoSize = true;
            this.lblGearCapacityLabel.Location = new System.Drawing.Point(307, 105);
            this.lblGearCapacityLabel.Name = "lblGearCapacityLabel";
            this.lblGearCapacityLabel.Size = new System.Drawing.Size(51, 13);
            this.lblGearCapacityLabel.TabIndex = 57;
            this.lblGearCapacityLabel.Tag = "Label_Capacity";
            this.lblGearCapacityLabel.Text = "Capacity:";
            //
            // lblGearCategory
            //
            this.lblGearCategory.AutoSize = true;
            this.lblGearCategory.Location = new System.Drawing.Point(365, 59);
            this.lblGearCategory.Name = "lblGearCategory";
            this.lblGearCategory.Size = new System.Drawing.Size(55, 13);
            this.lblGearCategory.TabIndex = 56;
            this.lblGearCategory.Text = "[Category]";
            //
            // lblGearCategoryLabel
            //
            this.lblGearCategoryLabel.AutoSize = true;
            this.lblGearCategoryLabel.Location = new System.Drawing.Point(307, 59);
            this.lblGearCategoryLabel.Name = "lblGearCategoryLabel";
            this.lblGearCategoryLabel.Size = new System.Drawing.Size(52, 13);
            this.lblGearCategoryLabel.TabIndex = 55;
            this.lblGearCategoryLabel.Tag = "Label_Category";
            this.lblGearCategoryLabel.Text = "Category:";
            //
            // lblGearName
            //
            this.lblGearName.AutoSize = true;
            this.lblGearName.Location = new System.Drawing.Point(365, 36);
            this.lblGearName.Name = "lblGearName";
            this.lblGearName.Size = new System.Drawing.Size(41, 13);
            this.lblGearName.TabIndex = 54;
            this.lblGearName.Text = "[Name]";
            //
            // lblGearNameLabel
            //
            this.lblGearNameLabel.AutoSize = true;
            this.lblGearNameLabel.Location = new System.Drawing.Point(307, 36);
            this.lblGearNameLabel.Name = "lblGearNameLabel";
            this.lblGearNameLabel.Size = new System.Drawing.Size(38, 13);
            this.lblGearNameLabel.TabIndex = 53;
            this.lblGearNameLabel.Tag = "Label_Name";
            this.lblGearNameLabel.Text = "Name:";
            //
            // nudGearRating
            //
            this.nudGearRating.Enabled = false;
            this.nudGearRating.Location = new System.Drawing.Point(368, 80);
            this.nudGearRating.Maximum = new decimal(new int[] {
            6,
            0,
            0,
            0});
            this.nudGearRating.Name = "nudGearRating";
            this.nudGearRating.Size = new System.Drawing.Size(48, 20);
            this.nudGearRating.TabIndex = 52;
            this.nudGearRating.ValueChanged += new System.EventHandler(this.nudGearRating_ValueChanged);
            //
            // lblGearRatingLabel
            //
            this.lblGearRatingLabel.AutoSize = true;
            this.lblGearRatingLabel.Location = new System.Drawing.Point(307, 82);
            this.lblGearRatingLabel.Name = "lblGearRatingLabel";
            this.lblGearRatingLabel.Size = new System.Drawing.Size(41, 13);
            this.lblGearRatingLabel.TabIndex = 51;
            this.lblGearRatingLabel.Tag = "Label_Rating";
            this.lblGearRatingLabel.Text = "Rating:";
            //
            // treGear
            //
            this.treGear.AllowDrop = true;
            this.treGear.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)));
            this.treGear.HideSelection = false;
            this.treGear.Location = new System.Drawing.Point(6, 36);
            this.treGear.Name = "treGear";
            treeNode24.Name = "nodGearRoot";
            treeNode24.Tag = "Node_SelectedGear";
            treeNode24.Text = "Selected Gear";
            this.treGear.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            treeNode24});
            this.treGear.ShowNodeToolTips = true;
            this.treGear.Size = new System.Drawing.Size(295, 540);
            this.treGear.TabIndex = 49;
            this.treGear.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treGear_AfterSelect);
            this.treGear.DragOver += new System.Windows.Forms.DragEventHandler(this.treGear_DragOver);
            this.treGear.KeyDown += new System.Windows.Forms.KeyEventHandler(this.treGear_KeyDown);
            this.treGear.MouseDown += new System.Windows.Forms.MouseEventHandler(this.TreeView_MouseDown);
            //
            // cmdDeleteGear
            //
            this.cmdDeleteGear.AutoSize = true;
            this.cmdDeleteGear.Location = new System.Drawing.Point(92, 7);
            this.cmdDeleteGear.Name = "cmdDeleteGear";
            this.cmdDeleteGear.Size = new System.Drawing.Size(80, 23);
            this.cmdDeleteGear.TabIndex = 50;
            this.cmdDeleteGear.Tag = "String_Delete";
            this.cmdDeleteGear.Text = "Delete";
            this.cmdDeleteGear.UseVisualStyleBackColor = true;
            this.cmdDeleteGear.Click += new System.EventHandler(this.cmdDeleteGear_Click);
            //
            // cmdAddGear
            //
            this.cmdAddGear.AutoSize = true;
            this.cmdAddGear.ContextMenuStrip = this.cmsGearButton;
            this.cmdAddGear.Location = new System.Drawing.Point(3, 7);
            this.cmdAddGear.Name = "cmdAddGear";
            this.cmdAddGear.Size = new System.Drawing.Size(80, 23);
            this.cmdAddGear.SplitMenuStrip = this.cmsGearButton;
            this.cmdAddGear.TabIndex = 145;
            this.cmdAddGear.Tag = "Button_AddGear";
            this.cmdAddGear.Text = "&Add Gear";
            this.cmdAddGear.UseVisualStyleBackColor = true;
            this.cmdAddGear.Click += new System.EventHandler(this.cmdAddGear_Click);
            //
            // tabPets
            //
            this.tabPets.BackColor = System.Drawing.SystemColors.Control;
            this.tabPets.Controls.Add(this.panPets);
            this.tabPets.Controls.Add(this.cmdAddPet);
            this.tabPets.Location = new System.Drawing.Point(4, 22);
            this.tabPets.Name = "tabPets";
            this.tabPets.Padding = new System.Windows.Forms.Padding(3);
            this.tabPets.Size = new System.Drawing.Size(827, 579);
            this.tabPets.TabIndex = 4;
            this.tabPets.Tag = "Tab_Pets";
            this.tabPets.Text = "Pets and Cohorts";
            //
            // panPets
            //
            this.panPets.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panPets.AutoScroll = true;
            this.panPets.Location = new System.Drawing.Point(6, 36);
            this.panPets.Margin = new System.Windows.Forms.Padding(0);
            this.panPets.Name = "panPets";
            this.panPets.Size = new System.Drawing.Size(815, 537);
            this.panPets.TabIndex = 24;
            //
            // cmdAddPet
            //
            this.cmdAddPet.AutoSize = true;
            this.cmdAddPet.Location = new System.Drawing.Point(6, 7);
            this.cmdAddPet.Name = "cmdAddPet";
            this.cmdAddPet.Size = new System.Drawing.Size(76, 23);
            this.cmdAddPet.TabIndex = 23;
            this.cmdAddPet.Tag = "Button_AddPet";
            this.cmdAddPet.Text = "&Add Pet";
            this.cmdAddPet.UseVisualStyleBackColor = true;
            this.cmdAddPet.Click += new System.EventHandler(this.cmdAddPet_Click);
            //
            // tabVehicles
            //
            this.tabVehicles.BackColor = System.Drawing.SystemColors.Control;
            this.tabVehicles.Controls.Add(this.lblVehicleSeats);
            this.tabVehicles.Controls.Add(this.lblVehicleSeatsLabel);
            this.tabVehicles.Controls.Add(this.lblVehicleDroneModSlots);
            this.tabVehicles.Controls.Add(this.lblVehicleDroneModSlotsLabel);
            this.tabVehicles.Controls.Add(this.lblVehicleCosmetic);
            this.tabVehicles.Controls.Add(this.lblVehicleElectromagnetic);
            this.tabVehicles.Controls.Add(this.lblVehicleBodymod);
            this.tabVehicles.Controls.Add(this.lblVehicleWeaponsmod);
            this.tabVehicles.Controls.Add(this.lblVehicleProtection);
            this.tabVehicles.Controls.Add(this.lblVehiclePowertrain);
            this.tabVehicles.Controls.Add(this.lblVehicleCosmeticLabel);
            this.tabVehicles.Controls.Add(this.lblVehicleElectromagneticLabel);
            this.tabVehicles.Controls.Add(this.lblVehicleBodymodLabel);
            this.tabVehicles.Controls.Add(this.lblVehicleWeaponsmodLabel);
            this.tabVehicles.Controls.Add(this.lblVehicleProtectionLabel);
            this.tabVehicles.Controls.Add(this.lblVehiclePowertrainLabel);
            this.tabVehicles.Controls.Add(this.cboVehicleGearDataProcessing);
            this.tabVehicles.Controls.Add(this.cboVehicleGearFirewall);
            this.tabVehicles.Controls.Add(this.cboVehicleGearSleaze);
            this.tabVehicles.Controls.Add(this.cboVehicleGearAttack);
            this.tabVehicles.Controls.Add(this.lblVehicleFirewallLabel);
            this.tabVehicles.Controls.Add(this.lblVehicleDataProcessingLabel);
            this.tabVehicles.Controls.Add(this.lblVehicleSleazeLabel);
            this.tabVehicles.Controls.Add(this.lblVehicleAttackLabel);
            this.tabVehicles.Controls.Add(this.lblVehicleWeaponRangeExtreme);
            this.tabVehicles.Controls.Add(this.lblVehicleWeaponRangeLong);
            this.tabVehicles.Controls.Add(this.lblVehicleWeaponRangeMedium);
            this.tabVehicles.Controls.Add(this.lblVehicleWeaponRangeShort);
            this.tabVehicles.Controls.Add(this.lblVehicleWeaponRangeExtremeLabel);
            this.tabVehicles.Controls.Add(this.lblVehicleWeaponRangeLongLabel);
            this.tabVehicles.Controls.Add(this.lblVehicleWeaponRangeMediumLabel);
            this.tabVehicles.Controls.Add(this.lblVehicleWeaponRangeShortLabel);
            this.tabVehicles.Controls.Add(this.lblVehicleWeaponRangeLabel);
            this.tabVehicles.Controls.Add(this.lblVehicleWeaponAmmo);
            this.tabVehicles.Controls.Add(this.lblVehicleWeaponAmmoLabel);
            this.tabVehicles.Controls.Add(this.lblVehicleWeaponMode);
            this.tabVehicles.Controls.Add(this.lblVehicleWeaponModeLabel);
            this.tabVehicles.Controls.Add(this.lblVehicleWeaponNameLabel);
            this.tabVehicles.Controls.Add(this.lblVehicleWeaponName);
            this.tabVehicles.Controls.Add(this.lblVehicleWeaponCategoryLabel);
            this.tabVehicles.Controls.Add(this.lblVehicleWeaponAP);
            this.tabVehicles.Controls.Add(this.lblVehicleWeaponCategory);
            this.tabVehicles.Controls.Add(this.lblVehicleWeaponAPLabel);
            this.tabVehicles.Controls.Add(this.lblVehicleWeaponDamageLabel);
            this.tabVehicles.Controls.Add(this.lblVehicleWeaponDamage);
            this.tabVehicles.Controls.Add(this.cmdAddVehicleLocation);
            this.tabVehicles.Controls.Add(this.chkVehicleHomeNode);
            this.tabVehicles.Controls.Add(this.lblVehicleDevice);
            this.tabVehicles.Controls.Add(this.lblVehicleDeviceLabel);
            this.tabVehicles.Controls.Add(this.nudVehicleGearQty);
            this.tabVehicles.Controls.Add(this.lblVehicleGearQtyLabel);
            this.tabVehicles.Controls.Add(this.chkVehicleIncludedInWeapon);
            this.tabVehicles.Controls.Add(this.chkVehicleWeaponAccessoryInstalled);
            this.tabVehicles.Controls.Add(this.lblVehicleSource);
            this.tabVehicles.Controls.Add(this.lblVehicleSourceLabel);
            this.tabVehicles.Controls.Add(this.lblVehicleSlots);
            this.tabVehicles.Controls.Add(this.lblVehicleSlotsLabel);
            this.tabVehicles.Controls.Add(this.nudVehicleRating);
            this.tabVehicles.Controls.Add(this.lblVehicleRatingLabel);
            this.tabVehicles.Controls.Add(this.lblVehicleNameLabel);
            this.tabVehicles.Controls.Add(this.lblVehicleName);
            this.tabVehicles.Controls.Add(this.lblVehicleCategoryLabel);
            this.tabVehicles.Controls.Add(this.lblVehicleCategory);
            this.tabVehicles.Controls.Add(this.lblVehicleSensor);
            this.tabVehicles.Controls.Add(this.lblVehicleSensorLabel);
            this.tabVehicles.Controls.Add(this.lblVehiclePilot);
            this.tabVehicles.Controls.Add(this.lblVehiclePilotLabel);
            this.tabVehicles.Controls.Add(this.lblVehicleArmor);
            this.tabVehicles.Controls.Add(this.lblVehicleArmorLabel);
            this.tabVehicles.Controls.Add(this.lblVehicleBody);
            this.tabVehicles.Controls.Add(this.lblVehicleBodyLabel);
            this.tabVehicles.Controls.Add(this.lblVehicleSpeed);
            this.tabVehicles.Controls.Add(this.lblVehicleSpeedLabel);
            this.tabVehicles.Controls.Add(this.lblVehicleCost);
            this.tabVehicles.Controls.Add(this.lblVehicleCostLabel);
            this.tabVehicles.Controls.Add(this.lblVehicleAvail);
            this.tabVehicles.Controls.Add(this.lblVehicleAvailLabel);
            this.tabVehicles.Controls.Add(this.lblVehicleAccel);
            this.tabVehicles.Controls.Add(this.lblVehicleAccelLabel);
            this.tabVehicles.Controls.Add(this.lblVehicleHandling);
            this.tabVehicles.Controls.Add(this.lblVehicleHandlingLabel);
            this.tabVehicles.Controls.Add(this.cmdDeleteVehicle);
            this.tabVehicles.Controls.Add(this.treVehicles);
            this.tabVehicles.Controls.Add(this.cmdAddVehicle);
            this.tabVehicles.Location = new System.Drawing.Point(4, 22);
            this.tabVehicles.Name = "tabVehicles";
            this.tabVehicles.Size = new System.Drawing.Size(838, 611);
            this.tabVehicles.TabIndex = 7;
            this.tabVehicles.Tag = "Tab_Vehicles";
            this.tabVehicles.Text = "Vehicles & Drones";
            //
            // lblVehicleSeats
            //
            this.lblVehicleSeats.AutoSize = true;
            this.lblVehicleSeats.Location = new System.Drawing.Point(485, 216);
            this.lblVehicleSeats.Name = "lblVehicleSeats";
            this.lblVehicleSeats.Size = new System.Drawing.Size(40, 13);
            this.lblVehicleSeats.TabIndex = 212;
            this.lblVehicleSeats.Text = "[Seats]";
            //
            // lblVehicleSeatsLabel
            //
            this.lblVehicleSeatsLabel.AutoSize = true;
            this.lblVehicleSeatsLabel.Location = new System.Drawing.Point(416, 216);
            this.lblVehicleSeatsLabel.Name = "lblVehicleSeatsLabel";
            this.lblVehicleSeatsLabel.Size = new System.Drawing.Size(34, 13);
            this.lblVehicleSeatsLabel.TabIndex = 211;
            this.lblVehicleSeatsLabel.Tag = "Label_VehicleSeats";
            this.lblVehicleSeatsLabel.Text = "Seats";
            //
            // lblVehicleDroneModSlots
            //
            this.lblVehicleDroneModSlots.AutoSize = true;
            this.lblVehicleDroneModSlots.Location = new System.Drawing.Point(788, 172);
            this.lblVehicleDroneModSlots.Name = "lblVehicleDroneModSlots";
            this.lblVehicleDroneModSlots.Size = new System.Drawing.Size(57, 13);
            this.lblVehicleDroneModSlots.TabIndex = 210;
            this.lblVehicleDroneModSlots.Text = "[ModSlots]";
            this.lblVehicleDroneModSlots.Visible = false;
            //
            // lblVehicleDroneModSlotsLabel
            //
            this.lblVehicleDroneModSlotsLabel.AutoSize = true;
            this.lblVehicleDroneModSlotsLabel.Location = new System.Drawing.Point(725, 172);
            this.lblVehicleDroneModSlotsLabel.Name = "lblVehicleDroneModSlotsLabel";
            this.lblVehicleDroneModSlotsLabel.Size = new System.Drawing.Size(57, 13);
            this.lblVehicleDroneModSlotsLabel.TabIndex = 209;
            this.lblVehicleDroneModSlotsLabel.Tag = "Label_DroneModSlots";
            this.lblVehicleDroneModSlotsLabel.Text = "Mod Slots:";
            this.lblVehicleDroneModSlotsLabel.Visible = false;
            //
            // lblVehicleCosmetic
            //
            this.lblVehicleCosmetic.AutoSize = true;
            this.lblVehicleCosmetic.Location = new System.Drawing.Point(675, 194);
            this.lblVehicleCosmetic.Name = "lblVehicleCosmetic";
            this.lblVehicleCosmetic.Size = new System.Drawing.Size(56, 13);
            this.lblVehicleCosmetic.TabIndex = 208;
            this.lblVehicleCosmetic.Text = "[Cosmetic]";
            //
            // lblVehicleElectromagnetic
            //
            this.lblVehicleElectromagnetic.AutoSize = true;
            this.lblVehicleElectromagnetic.Location = new System.Drawing.Point(573, 194);
            this.lblVehicleElectromagnetic.Name = "lblVehicleElectromagnetic";
            this.lblVehicleElectromagnetic.Size = new System.Drawing.Size(34, 13);
            this.lblVehicleElectromagnetic.TabIndex = 207;
            this.lblVehicleElectromagnetic.Text = "[Elec]";
            //
            // lblVehicleBodymod
            //
            this.lblVehicleBodymod.AutoSize = true;
            this.lblVehicleBodymod.Location = new System.Drawing.Point(486, 194);
            this.lblVehicleBodymod.Name = "lblVehicleBodymod";
            this.lblVehicleBodymod.Size = new System.Drawing.Size(37, 13);
            this.lblVehicleBodymod.TabIndex = 206;
            this.lblVehicleBodymod.Text = "[Body]";
            //
            // lblVehicleWeaponsmod
            //
            this.lblVehicleWeaponsmod.AutoSize = true;
            this.lblVehicleWeaponsmod.Location = new System.Drawing.Point(486, 172);
            this.lblVehicleWeaponsmod.Name = "lblVehicleWeaponsmod";
            this.lblVehicleWeaponsmod.Size = new System.Drawing.Size(42, 13);
            this.lblVehicleWeaponsmod.TabIndex = 205;
            this.lblVehicleWeaponsmod.Text = "[Weap]";
            //
            // lblVehicleProtection
            //
            this.lblVehicleProtection.AutoSize = true;
            this.lblVehicleProtection.Location = new System.Drawing.Point(573, 172);
            this.lblVehicleProtection.Name = "lblVehicleProtection";
            this.lblVehicleProtection.Size = new System.Drawing.Size(32, 13);
            this.lblVehicleProtection.TabIndex = 204;
            this.lblVehicleProtection.Text = "[Prot]";
            //
            // lblVehiclePowertrain
            //
            this.lblVehiclePowertrain.AutoSize = true;
            this.lblVehiclePowertrain.Location = new System.Drawing.Point(675, 172);
            this.lblVehiclePowertrain.Name = "lblVehiclePowertrain";
            this.lblVehiclePowertrain.Size = new System.Drawing.Size(43, 13);
            this.lblVehiclePowertrain.TabIndex = 203;
            this.lblVehiclePowertrain.Text = "[Power]";
            //
            // lblVehicleCosmeticLabel
            //
            this.lblVehicleCosmeticLabel.AutoSize = true;
            this.lblVehicleCosmeticLabel.Location = new System.Drawing.Point(626, 194);
            this.lblVehicleCosmeticLabel.Name = "lblVehicleCosmeticLabel";
            this.lblVehicleCosmeticLabel.Size = new System.Drawing.Size(53, 13);
            this.lblVehicleCosmeticLabel.TabIndex = 202;
            this.lblVehicleCosmeticLabel.Tag = "Label_Cosmetic";
            this.lblVehicleCosmeticLabel.Text = "Cosmetic:";
            //
            // lblVehicleElectromagneticLabel
            //
            this.lblVehicleElectromagneticLabel.AutoSize = true;
            this.lblVehicleElectromagneticLabel.Location = new System.Drawing.Point(533, 194);
            this.lblVehicleElectromagneticLabel.Name = "lblVehicleElectromagneticLabel";
            this.lblVehicleElectromagneticLabel.Size = new System.Drawing.Size(31, 13);
            this.lblVehicleElectromagneticLabel.TabIndex = 201;
            this.lblVehicleElectromagneticLabel.Tag = "Label_Electromagnetic";
            this.lblVehicleElectromagneticLabel.Text = "Elec:";
            //
            // lblVehicleBodymodLabel
            //
            this.lblVehicleBodymodLabel.AutoSize = true;
            this.lblVehicleBodymodLabel.Location = new System.Drawing.Point(416, 194);
            this.lblVehicleBodymodLabel.Name = "lblVehicleBodymodLabel";
            this.lblVehicleBodymodLabel.Size = new System.Drawing.Size(63, 13);
            this.lblVehicleBodymodLabel.TabIndex = 200;
            this.lblVehicleBodymodLabel.Tag = "Label_Bodymod";
            this.lblVehicleBodymodLabel.Text = "Body Mods:";
            //
            // lblVehicleWeaponsmodLabel
            //
            this.lblVehicleWeaponsmodLabel.AutoSize = true;
            this.lblVehicleWeaponsmodLabel.Location = new System.Drawing.Point(416, 172);
            this.lblVehicleWeaponsmodLabel.Name = "lblVehicleWeaponsmodLabel";
            this.lblVehicleWeaponsmodLabel.Size = new System.Drawing.Size(56, 13);
            this.lblVehicleWeaponsmodLabel.TabIndex = 199;
            this.lblVehicleWeaponsmodLabel.Tag = "Label_Weapons";
            this.lblVehicleWeaponsmodLabel.Text = "Weapons:";
            //
            // lblVehicleProtectionLabel
            //
            this.lblVehicleProtectionLabel.AutoSize = true;
            this.lblVehicleProtectionLabel.Location = new System.Drawing.Point(533, 172);
            this.lblVehicleProtectionLabel.Name = "lblVehicleProtectionLabel";
            this.lblVehicleProtectionLabel.Size = new System.Drawing.Size(29, 13);
            this.lblVehicleProtectionLabel.TabIndex = 198;
            this.lblVehicleProtectionLabel.Tag = "Label_Protection";
            this.lblVehicleProtectionLabel.Text = "Prot:";
            //
            // lblVehiclePowertrainLabel
            //
            this.lblVehiclePowertrainLabel.AutoSize = true;
            this.lblVehiclePowertrainLabel.Location = new System.Drawing.Point(626, 172);
            this.lblVehiclePowertrainLabel.Name = "lblVehiclePowertrainLabel";
            this.lblVehiclePowertrainLabel.Size = new System.Drawing.Size(40, 13);
            this.lblVehiclePowertrainLabel.TabIndex = 197;
            this.lblVehiclePowertrainLabel.Tag = "Label_Powertrain";
            this.lblVehiclePowertrainLabel.Text = "Power:";
            //
            // cboVehicleGearDataProcessing
            //
            this.cboVehicleGearDataProcessing.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboVehicleGearDataProcessing.FormattingEnabled = true;
            this.cboVehicleGearDataProcessing.Location = new System.Drawing.Point(693, 123);
            this.cboVehicleGearDataProcessing.Name = "cboVehicleGearDataProcessing";
            this.cboVehicleGearDataProcessing.Size = new System.Drawing.Size(30, 21);
            this.cboVehicleGearDataProcessing.TabIndex = 196;
            this.cboVehicleGearDataProcessing.Visible = false;
            this.cboVehicleGearDataProcessing.SelectedIndexChanged += new System.EventHandler(this.cboVehicleGearDataProcessing_SelectedIndexChanged);
            //
            // cboVehicleGearFirewall
            //
            this.cboVehicleGearFirewall.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboVehicleGearFirewall.FormattingEnabled = true;
            this.cboVehicleGearFirewall.Location = new System.Drawing.Point(777, 123);
            this.cboVehicleGearFirewall.Name = "cboVehicleGearFirewall";
            this.cboVehicleGearFirewall.Size = new System.Drawing.Size(30, 21);
            this.cboVehicleGearFirewall.TabIndex = 195;
            this.cboVehicleGearFirewall.Visible = false;
            this.cboVehicleGearFirewall.SelectedIndexChanged += new System.EventHandler(this.cboVehicleGearFirewall_SelectedIndexChanged);
            //
            // cboVehicleGearSleaze
            //
            this.cboVehicleGearSleaze.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboVehicleGearSleaze.FormattingEnabled = true;
            this.cboVehicleGearSleaze.Location = new System.Drawing.Point(576, 123);
            this.cboVehicleGearSleaze.Name = "cboVehicleGearSleaze";
            this.cboVehicleGearSleaze.Size = new System.Drawing.Size(30, 21);
            this.cboVehicleGearSleaze.TabIndex = 194;
            this.cboVehicleGearSleaze.Visible = false;
            this.cboVehicleGearSleaze.SelectedIndexChanged += new System.EventHandler(this.cboVehicleGearSleaze_SelectedIndexChanged);
            //
            // cboVehicleGearAttack
            //
            this.cboVehicleGearAttack.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboVehicleGearAttack.FormattingEnabled = true;
            this.cboVehicleGearAttack.Location = new System.Drawing.Point(486, 123);
            this.cboVehicleGearAttack.Name = "cboVehicleGearAttack";
            this.cboVehicleGearAttack.Size = new System.Drawing.Size(30, 21);
            this.cboVehicleGearAttack.TabIndex = 193;
            this.cboVehicleGearAttack.Visible = false;
            this.cboVehicleGearAttack.SelectedIndexChanged += new System.EventHandler(this.cboVehicleGearAttack_SelectedIndexChanged);
            //
            // lblVehicleFirewallLabel
            //
            this.lblVehicleFirewallLabel.AutoSize = true;
            this.lblVehicleFirewallLabel.Location = new System.Drawing.Point(725, 128);
            this.lblVehicleFirewallLabel.Name = "lblVehicleFirewallLabel";
            this.lblVehicleFirewallLabel.Size = new System.Drawing.Size(45, 13);
            this.lblVehicleFirewallLabel.TabIndex = 192;
            this.lblVehicleFirewallLabel.Tag = "Label_Firewall";
            this.lblVehicleFirewallLabel.Text = "Firewall:";
            //
            // lblVehicleDataProcessingLabel
            //
            this.lblVehicleDataProcessingLabel.AutoSize = true;
            this.lblVehicleDataProcessingLabel.Location = new System.Drawing.Point(626, 128);
            this.lblVehicleDataProcessingLabel.Name = "lblVehicleDataProcessingLabel";
            this.lblVehicleDataProcessingLabel.Size = new System.Drawing.Size(58, 13);
            this.lblVehicleDataProcessingLabel.TabIndex = 190;
            this.lblVehicleDataProcessingLabel.Tag = "Label_DataProcessing";
            this.lblVehicleDataProcessingLabel.Text = "Data Proc:";
            //
            // lblVehicleSleazeLabel
            //
            this.lblVehicleSleazeLabel.AutoSize = true;
            this.lblVehicleSleazeLabel.Location = new System.Drawing.Point(533, 128);
            this.lblVehicleSleazeLabel.Name = "lblVehicleSleazeLabel";
            this.lblVehicleSleazeLabel.Size = new System.Drawing.Size(42, 13);
            this.lblVehicleSleazeLabel.TabIndex = 188;
            this.lblVehicleSleazeLabel.Tag = "Label_Sleaze";
            this.lblVehicleSleazeLabel.Text = "Sleaze:";
            //
            // lblVehicleAttackLabel
            //
            this.lblVehicleAttackLabel.AutoSize = true;
            this.lblVehicleAttackLabel.Location = new System.Drawing.Point(416, 128);
            this.lblVehicleAttackLabel.Name = "lblVehicleAttackLabel";
            this.lblVehicleAttackLabel.Size = new System.Drawing.Size(41, 13);
            this.lblVehicleAttackLabel.TabIndex = 186;
            this.lblVehicleAttackLabel.Tag = "Label_Attack";
            this.lblVehicleAttackLabel.Text = "Attack:";
            //
            // lblVehicleWeaponRangeExtreme
            //
            this.lblVehicleWeaponRangeExtreme.Location = new System.Drawing.Point(627, 444);
            this.lblVehicleWeaponRangeExtreme.Name = "lblVehicleWeaponRangeExtreme";
            this.lblVehicleWeaponRangeExtreme.Size = new System.Drawing.Size(64, 13);
            this.lblVehicleWeaponRangeExtreme.TabIndex = 150;
            this.lblVehicleWeaponRangeExtreme.Text = "[0]";
            this.lblVehicleWeaponRangeExtreme.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            //
            // lblVehicleWeaponRangeLong
            //
            this.lblVehicleWeaponRangeLong.Location = new System.Drawing.Point(557, 444);
            this.lblVehicleWeaponRangeLong.Name = "lblVehicleWeaponRangeLong";
            this.lblVehicleWeaponRangeLong.Size = new System.Drawing.Size(64, 13);
            this.lblVehicleWeaponRangeLong.TabIndex = 149;
            this.lblVehicleWeaponRangeLong.Text = "[0]";
            this.lblVehicleWeaponRangeLong.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            //
            // lblVehicleWeaponRangeMedium
            //
            this.lblVehicleWeaponRangeMedium.Location = new System.Drawing.Point(487, 444);
            this.lblVehicleWeaponRangeMedium.Name = "lblVehicleWeaponRangeMedium";
            this.lblVehicleWeaponRangeMedium.Size = new System.Drawing.Size(64, 13);
            this.lblVehicleWeaponRangeMedium.TabIndex = 148;
            this.lblVehicleWeaponRangeMedium.Text = "[0]";
            this.lblVehicleWeaponRangeMedium.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            //
            // lblVehicleWeaponRangeShort
            //
            this.lblVehicleWeaponRangeShort.Location = new System.Drawing.Point(417, 444);
            this.lblVehicleWeaponRangeShort.Name = "lblVehicleWeaponRangeShort";
            this.lblVehicleWeaponRangeShort.Size = new System.Drawing.Size(64, 13);
            this.lblVehicleWeaponRangeShort.TabIndex = 147;
            this.lblVehicleWeaponRangeShort.Text = "[0]";
            this.lblVehicleWeaponRangeShort.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            //
            // lblVehicleWeaponRangeExtremeLabel
            //
            this.lblVehicleWeaponRangeExtremeLabel.AutoSize = true;
            this.lblVehicleWeaponRangeExtremeLabel.Location = new System.Drawing.Point(629, 427);
            this.lblVehicleWeaponRangeExtremeLabel.Name = "lblVehicleWeaponRangeExtremeLabel";
            this.lblVehicleWeaponRangeExtremeLabel.Size = new System.Drawing.Size(63, 13);
            this.lblVehicleWeaponRangeExtremeLabel.TabIndex = 146;
            this.lblVehicleWeaponRangeExtremeLabel.Tag = "Label_RangeExtreme";
            this.lblVehicleWeaponRangeExtremeLabel.Text = "Extreme (-6)";
            this.lblVehicleWeaponRangeExtremeLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            //
            // lblVehicleWeaponRangeLongLabel
            //
            this.lblVehicleWeaponRangeLongLabel.AutoSize = true;
            this.lblVehicleWeaponRangeLongLabel.Location = new System.Drawing.Point(566, 427);
            this.lblVehicleWeaponRangeLongLabel.Name = "lblVehicleWeaponRangeLongLabel";
            this.lblVehicleWeaponRangeLongLabel.Size = new System.Drawing.Size(49, 13);
            this.lblVehicleWeaponRangeLongLabel.TabIndex = 145;
            this.lblVehicleWeaponRangeLongLabel.Tag = "Label_RangeLong";
            this.lblVehicleWeaponRangeLongLabel.Text = "Long (-3)";
            this.lblVehicleWeaponRangeLongLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            //
            // lblVehicleWeaponRangeMediumLabel
            //
            this.lblVehicleWeaponRangeMediumLabel.AutoSize = true;
            this.lblVehicleWeaponRangeMediumLabel.Location = new System.Drawing.Point(489, 427);
            this.lblVehicleWeaponRangeMediumLabel.Name = "lblVehicleWeaponRangeMediumLabel";
            this.lblVehicleWeaponRangeMediumLabel.Size = new System.Drawing.Size(62, 13);
            this.lblVehicleWeaponRangeMediumLabel.TabIndex = 144;
            this.lblVehicleWeaponRangeMediumLabel.Tag = "Label_RangeMedium";
            this.lblVehicleWeaponRangeMediumLabel.Text = "Medium (-1)";
            this.lblVehicleWeaponRangeMediumLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            //
            // lblVehicleWeaponRangeShortLabel
            //
            this.lblVehicleWeaponRangeShortLabel.AutoSize = true;
            this.lblVehicleWeaponRangeShortLabel.Location = new System.Drawing.Point(425, 427);
            this.lblVehicleWeaponRangeShortLabel.Name = "lblVehicleWeaponRangeShortLabel";
            this.lblVehicleWeaponRangeShortLabel.Size = new System.Drawing.Size(50, 13);
            this.lblVehicleWeaponRangeShortLabel.TabIndex = 143;
            this.lblVehicleWeaponRangeShortLabel.Tag = "Label_RangeShort";
            this.lblVehicleWeaponRangeShortLabel.Text = "Short (-0)";
            this.lblVehicleWeaponRangeShortLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            //
            // lblVehicleWeaponRangeLabel
            //
            this.lblVehicleWeaponRangeLabel.AutoSize = true;
            this.lblVehicleWeaponRangeLabel.Location = new System.Drawing.Point(416, 404);
            this.lblVehicleWeaponRangeLabel.Name = "lblVehicleWeaponRangeLabel";
            this.lblVehicleWeaponRangeLabel.Size = new System.Drawing.Size(39, 13);
            this.lblVehicleWeaponRangeLabel.TabIndex = 142;
            this.lblVehicleWeaponRangeLabel.Tag = "Label_RangeHeading";
            this.lblVehicleWeaponRangeLabel.Text = "Range";
            //
            // lblVehicleWeaponAmmo
            //
            this.lblVehicleWeaponAmmo.AutoSize = true;
            this.lblVehicleWeaponAmmo.Location = new System.Drawing.Point(773, 376);
            this.lblVehicleWeaponAmmo.Name = "lblVehicleWeaponAmmo";
            this.lblVehicleWeaponAmmo.Size = new System.Drawing.Size(42, 13);
            this.lblVehicleWeaponAmmo.TabIndex = 141;
            this.lblVehicleWeaponAmmo.Text = "[Ammo]";
            //
            // lblVehicleWeaponAmmoLabel
            //
            this.lblVehicleWeaponAmmoLabel.AutoSize = true;
            this.lblVehicleWeaponAmmoLabel.Location = new System.Drawing.Point(724, 376);
            this.lblVehicleWeaponAmmoLabel.Name = "lblVehicleWeaponAmmoLabel";
            this.lblVehicleWeaponAmmoLabel.Size = new System.Drawing.Size(39, 13);
            this.lblVehicleWeaponAmmoLabel.TabIndex = 140;
            this.lblVehicleWeaponAmmoLabel.Tag = "Label_Ammo";
            this.lblVehicleWeaponAmmoLabel.Text = "Ammo:";
            //
            // lblVehicleWeaponMode
            //
            this.lblVehicleWeaponMode.AutoSize = true;
            this.lblVehicleWeaponMode.Location = new System.Drawing.Point(680, 376);
            this.lblVehicleWeaponMode.Name = "lblVehicleWeaponMode";
            this.lblVehicleWeaponMode.Size = new System.Drawing.Size(40, 13);
            this.lblVehicleWeaponMode.TabIndex = 139;
            this.lblVehicleWeaponMode.Text = "[Mode]";
            //
            // lblVehicleWeaponModeLabel
            //
            this.lblVehicleWeaponModeLabel.AutoSize = true;
            this.lblVehicleWeaponModeLabel.Location = new System.Drawing.Point(625, 376);
            this.lblVehicleWeaponModeLabel.Name = "lblVehicleWeaponModeLabel";
            this.lblVehicleWeaponModeLabel.Size = new System.Drawing.Size(37, 13);
            this.lblVehicleWeaponModeLabel.TabIndex = 138;
            this.lblVehicleWeaponModeLabel.Tag = "Label_Mode";
            this.lblVehicleWeaponModeLabel.Text = "Mode:";
            //
            // lblVehicleWeaponNameLabel
            //
            this.lblVehicleWeaponNameLabel.AutoSize = true;
            this.lblVehicleWeaponNameLabel.Location = new System.Drawing.Point(416, 330);
            this.lblVehicleWeaponNameLabel.Name = "lblVehicleWeaponNameLabel";
            this.lblVehicleWeaponNameLabel.Size = new System.Drawing.Size(38, 13);
            this.lblVehicleWeaponNameLabel.TabIndex = 130;
            this.lblVehicleWeaponNameLabel.Tag = "Label_Name";
            this.lblVehicleWeaponNameLabel.Text = "Name:";
            //
            // lblVehicleWeaponName
            //
            this.lblVehicleWeaponName.AutoSize = true;
            this.lblVehicleWeaponName.Location = new System.Drawing.Point(486, 330);
            this.lblVehicleWeaponName.Name = "lblVehicleWeaponName";
            this.lblVehicleWeaponName.Size = new System.Drawing.Size(41, 13);
            this.lblVehicleWeaponName.TabIndex = 131;
            this.lblVehicleWeaponName.Text = "[Name]";
            //
            // lblVehicleWeaponCategoryLabel
            //
            this.lblVehicleWeaponCategoryLabel.AutoSize = true;
            this.lblVehicleWeaponCategoryLabel.Location = new System.Drawing.Point(416, 353);
            this.lblVehicleWeaponCategoryLabel.Name = "lblVehicleWeaponCategoryLabel";
            this.lblVehicleWeaponCategoryLabel.Size = new System.Drawing.Size(52, 13);
            this.lblVehicleWeaponCategoryLabel.TabIndex = 132;
            this.lblVehicleWeaponCategoryLabel.Tag = "Label_Category";
            this.lblVehicleWeaponCategoryLabel.Text = "Category:";
            //
            // lblVehicleWeaponAP
            //
            this.lblVehicleWeaponAP.AutoSize = true;
            this.lblVehicleWeaponAP.Location = new System.Drawing.Point(573, 376);
            this.lblVehicleWeaponAP.Name = "lblVehicleWeaponAP";
            this.lblVehicleWeaponAP.Size = new System.Drawing.Size(27, 13);
            this.lblVehicleWeaponAP.TabIndex = 137;
            this.lblVehicleWeaponAP.Text = "[AP]";
            //
            // lblVehicleWeaponCategory
            //
            this.lblVehicleWeaponCategory.AutoSize = true;
            this.lblVehicleWeaponCategory.Location = new System.Drawing.Point(485, 353);
            this.lblVehicleWeaponCategory.Name = "lblVehicleWeaponCategory";
            this.lblVehicleWeaponCategory.Size = new System.Drawing.Size(55, 13);
            this.lblVehicleWeaponCategory.TabIndex = 133;
            this.lblVehicleWeaponCategory.Text = "[Category]";
            //
            // lblVehicleWeaponAPLabel
            //
            this.lblVehicleWeaponAPLabel.AutoSize = true;
            this.lblVehicleWeaponAPLabel.Location = new System.Drawing.Point(537, 376);
            this.lblVehicleWeaponAPLabel.Name = "lblVehicleWeaponAPLabel";
            this.lblVehicleWeaponAPLabel.Size = new System.Drawing.Size(24, 13);
            this.lblVehicleWeaponAPLabel.TabIndex = 136;
            this.lblVehicleWeaponAPLabel.Tag = "Label_AP";
            this.lblVehicleWeaponAPLabel.Text = "AP:";
            //
            // lblVehicleWeaponDamageLabel
            //
            this.lblVehicleWeaponDamageLabel.AutoSize = true;
            this.lblVehicleWeaponDamageLabel.Location = new System.Drawing.Point(416, 376);
            this.lblVehicleWeaponDamageLabel.Name = "lblVehicleWeaponDamageLabel";
            this.lblVehicleWeaponDamageLabel.Size = new System.Drawing.Size(50, 13);
            this.lblVehicleWeaponDamageLabel.TabIndex = 134;
            this.lblVehicleWeaponDamageLabel.Tag = "Label_Damage";
            this.lblVehicleWeaponDamageLabel.Text = "Damage:";
            //
            // lblVehicleWeaponDamage
            //
            this.lblVehicleWeaponDamage.AutoSize = true;
            this.lblVehicleWeaponDamage.Location = new System.Drawing.Point(485, 376);
            this.lblVehicleWeaponDamage.Name = "lblVehicleWeaponDamage";
            this.lblVehicleWeaponDamage.Size = new System.Drawing.Size(53, 13);
            this.lblVehicleWeaponDamage.TabIndex = 135;
            this.lblVehicleWeaponDamage.Text = "[Damage]";
            //
            // cmdAddVehicleLocation
            //
            this.cmdAddVehicleLocation.AutoSize = true;
            this.cmdAddVehicleLocation.Location = new System.Drawing.Point(220, 7);
            this.cmdAddVehicleLocation.Name = "cmdAddVehicleLocation";
            this.cmdAddVehicleLocation.Size = new System.Drawing.Size(80, 23);
            this.cmdAddVehicleLocation.TabIndex = 128;
            this.cmdAddVehicleLocation.Tag = "Button_AddLocation";
            this.cmdAddVehicleLocation.Text = "Add Location";
            this.cmdAddVehicleLocation.UseVisualStyleBackColor = true;
            this.cmdAddVehicleLocation.Click += new System.EventHandler(this.cmdAddVehicleLocation_Click);
            //
            // chkVehicleHomeNode
            //
            this.chkVehicleHomeNode.AutoSize = true;
            this.chkVehicleHomeNode.Location = new System.Drawing.Point(574, 270);
            this.chkVehicleHomeNode.Name = "chkVehicleHomeNode";
            this.chkVehicleHomeNode.Size = new System.Drawing.Size(83, 17);
            this.chkVehicleHomeNode.TabIndex = 127;
            this.chkVehicleHomeNode.Tag = "Checkbox_HomeNode";
            this.chkVehicleHomeNode.Text = "Home Node";
            this.chkVehicleHomeNode.UseVisualStyleBackColor = true;
            this.chkVehicleHomeNode.Visible = false;
            this.chkVehicleHomeNode.CheckedChanged += new System.EventHandler(this.chkVehicleHomeNode_CheckedChanged);
            //
            // lblVehicleDevice
            //
            this.lblVehicleDevice.AutoSize = true;
            this.lblVehicleDevice.Location = new System.Drawing.Point(675, 150);
            this.lblVehicleDevice.Name = "lblVehicleDevice";
            this.lblVehicleDevice.Size = new System.Drawing.Size(47, 13);
            this.lblVehicleDevice.TabIndex = 118;
            this.lblVehicleDevice.Text = "[Device]";
            //
            // lblVehicleDeviceLabel
            //
            this.lblVehicleDeviceLabel.AutoSize = true;
            this.lblVehicleDeviceLabel.Location = new System.Drawing.Point(626, 150);
            this.lblVehicleDeviceLabel.Name = "lblVehicleDeviceLabel";
            this.lblVehicleDeviceLabel.Size = new System.Drawing.Size(44, 13);
            this.lblVehicleDeviceLabel.TabIndex = 117;
            this.lblVehicleDeviceLabel.Tag = "Label_Device";
            this.lblVehicleDeviceLabel.Text = "Device:";
            //
            // nudVehicleGearQty
            //
            this.nudVehicleGearQty.Enabled = false;
            this.nudVehicleGearQty.Location = new System.Drawing.Point(489, 269);
            this.nudVehicleGearQty.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.nudVehicleGearQty.Name = "nudVehicleGearQty";
            this.nudVehicleGearQty.Size = new System.Drawing.Size(54, 20);
            this.nudVehicleGearQty.TabIndex = 79;
            this.nudVehicleGearQty.ValueChanged += new System.EventHandler(this.nudVehicleGearQty_ValueChanged);
            //
            // lblVehicleGearQtyLabel
            //
            this.lblVehicleGearQtyLabel.AutoSize = true;
            this.lblVehicleGearQtyLabel.Location = new System.Drawing.Point(416, 271);
            this.lblVehicleGearQtyLabel.Name = "lblVehicleGearQtyLabel";
            this.lblVehicleGearQtyLabel.Size = new System.Drawing.Size(52, 13);
            this.lblVehicleGearQtyLabel.TabIndex = 78;
            this.lblVehicleGearQtyLabel.Tag = "Label_GearQty";
            this.lblVehicleGearQtyLabel.Text = "Gear Qty:";
            //
            // chkVehicleIncludedInWeapon
            //
            this.chkVehicleIncludedInWeapon.AutoSize = true;
            this.chkVehicleIncludedInWeapon.Enabled = false;
            this.chkVehicleIncludedInWeapon.Location = new System.Drawing.Point(574, 244);
            this.chkVehicleIncludedInWeapon.Name = "chkVehicleIncludedInWeapon";
            this.chkVehicleIncludedInWeapon.Size = new System.Drawing.Size(127, 17);
            this.chkVehicleIncludedInWeapon.TabIndex = 75;
            this.chkVehicleIncludedInWeapon.Tag = "Checkbox_BaseWeapon";
            this.chkVehicleIncludedInWeapon.Text = "Part of base Weapon";
            this.chkVehicleIncludedInWeapon.UseVisualStyleBackColor = true;
            //
            // lblVehicleSource
            //
            this.lblVehicleSource.AutoSize = true;
            this.lblVehicleSource.Location = new System.Drawing.Point(485, 294);
            this.lblVehicleSource.Name = "lblVehicleSource";
            this.lblVehicleSource.Size = new System.Drawing.Size(47, 13);
            this.lblVehicleSource.TabIndex = 60;
            this.lblVehicleSource.Text = "[Source]";
            this.lblVehicleSource.Click += new System.EventHandler(this.lblVehicleSource_Click);
            //
            // lblVehicleSourceLabel
            //
            this.lblVehicleSourceLabel.AutoSize = true;
            this.lblVehicleSourceLabel.Location = new System.Drawing.Point(416, 294);
            this.lblVehicleSourceLabel.Name = "lblVehicleSourceLabel";
            this.lblVehicleSourceLabel.Size = new System.Drawing.Size(44, 13);
            this.lblVehicleSourceLabel.TabIndex = 59;
            this.lblVehicleSourceLabel.Tag = "Label_Source";
            this.lblVehicleSourceLabel.Text = "Source:";
            //
            // lblVehicleSlots
            //
            this.lblVehicleSlots.AutoSize = true;
            this.lblVehicleSlots.Location = new System.Drawing.Point(764, 194);
            this.lblVehicleSlots.Name = "lblVehicleSlots";
            this.lblVehicleSlots.Size = new System.Drawing.Size(36, 13);
            this.lblVehicleSlots.TabIndex = 58;
            this.lblVehicleSlots.Text = "[Slots]";
            this.lblVehicleSlots.Visible = false;
            //
            // lblVehicleSlotsLabel
            //
            this.lblVehicleSlotsLabel.AutoSize = true;
            this.lblVehicleSlotsLabel.Location = new System.Drawing.Point(725, 194);
            this.lblVehicleSlotsLabel.Name = "lblVehicleSlotsLabel";
            this.lblVehicleSlotsLabel.Size = new System.Drawing.Size(33, 13);
            this.lblVehicleSlotsLabel.TabIndex = 57;
            this.lblVehicleSlotsLabel.Tag = "Label_Slots";
            this.lblVehicleSlotsLabel.Text = "Slots:";
            this.lblVehicleSlotsLabel.Visible = false;
            //
            // nudVehicleRating
            //
            this.nudVehicleRating.Enabled = false;
            this.nudVehicleRating.Location = new System.Drawing.Point(489, 243);
            this.nudVehicleRating.Maximum = new decimal(new int[] {
            6,
            0,
            0,
            0});
            this.nudVehicleRating.Name = "nudVehicleRating";
            this.nudVehicleRating.Size = new System.Drawing.Size(54, 20);
            this.nudVehicleRating.TabIndex = 56;
            this.nudVehicleRating.ValueChanged += new System.EventHandler(this.nudVehicleRating_ValueChanged);
            //
            // lblVehicleRatingLabel
            //
            this.lblVehicleRatingLabel.AutoSize = true;
            this.lblVehicleRatingLabel.Location = new System.Drawing.Point(416, 248);
            this.lblVehicleRatingLabel.Name = "lblVehicleRatingLabel";
            this.lblVehicleRatingLabel.Size = new System.Drawing.Size(41, 13);
            this.lblVehicleRatingLabel.TabIndex = 55;
            this.lblVehicleRatingLabel.Tag = "Label_Rating";
            this.lblVehicleRatingLabel.Text = "Rating:";
            //
            // lblVehicleNameLabel
            //
            this.lblVehicleNameLabel.AutoSize = true;
            this.lblVehicleNameLabel.Location = new System.Drawing.Point(416, 36);
            this.lblVehicleNameLabel.Name = "lblVehicleNameLabel";
            this.lblVehicleNameLabel.Size = new System.Drawing.Size(38, 13);
            this.lblVehicleNameLabel.TabIndex = 51;
            this.lblVehicleNameLabel.Tag = "Label_Name";
            this.lblVehicleNameLabel.Text = "Name:";
            //
            // lblVehicleName
            //
            this.lblVehicleName.AutoSize = true;
            this.lblVehicleName.Location = new System.Drawing.Point(486, 36);
            this.lblVehicleName.Name = "lblVehicleName";
            this.lblVehicleName.Size = new System.Drawing.Size(41, 13);
            this.lblVehicleName.TabIndex = 52;
            this.lblVehicleName.Text = "[Name]";
            //
            // lblVehicleCategoryLabel
            //
            this.lblVehicleCategoryLabel.AutoSize = true;
            this.lblVehicleCategoryLabel.Location = new System.Drawing.Point(416, 59);
            this.lblVehicleCategoryLabel.Name = "lblVehicleCategoryLabel";
            this.lblVehicleCategoryLabel.Size = new System.Drawing.Size(52, 13);
            this.lblVehicleCategoryLabel.TabIndex = 53;
            this.lblVehicleCategoryLabel.Tag = "Label_Category";
            this.lblVehicleCategoryLabel.Text = "Category:";
            //
            // lblVehicleCategory
            //
            this.lblVehicleCategory.AutoSize = true;
            this.lblVehicleCategory.Location = new System.Drawing.Point(486, 58);
            this.lblVehicleCategory.Name = "lblVehicleCategory";
            this.lblVehicleCategory.Size = new System.Drawing.Size(55, 13);
            this.lblVehicleCategory.TabIndex = 54;
            this.lblVehicleCategory.Text = "[Category]";
            //
            // lblVehicleSensor
            //
            this.lblVehicleSensor.AutoSize = true;
            this.lblVehicleSensor.Location = new System.Drawing.Point(774, 104);
            this.lblVehicleSensor.Name = "lblVehicleSensor";
            this.lblVehicleSensor.Size = new System.Drawing.Size(46, 13);
            this.lblVehicleSensor.TabIndex = 46;
            this.lblVehicleSensor.Text = "[Sensor]";
            //
            // lblVehicleSensorLabel
            //
            this.lblVehicleSensorLabel.AutoSize = true;
            this.lblVehicleSensorLabel.Location = new System.Drawing.Point(725, 104);
            this.lblVehicleSensorLabel.Name = "lblVehicleSensorLabel";
            this.lblVehicleSensorLabel.Size = new System.Drawing.Size(43, 13);
            this.lblVehicleSensorLabel.TabIndex = 45;
            this.lblVehicleSensorLabel.Tag = "Label_Sensor";
            this.lblVehicleSensorLabel.Text = "Sensor:";
            //
            // lblVehiclePilot
            //
            this.lblVehiclePilot.AutoSize = true;
            this.lblVehiclePilot.Location = new System.Drawing.Point(486, 104);
            this.lblVehiclePilot.Name = "lblVehiclePilot";
            this.lblVehiclePilot.Size = new System.Drawing.Size(33, 13);
            this.lblVehiclePilot.TabIndex = 40;
            this.lblVehiclePilot.Text = "[Pilot]";
            //
            // lblVehiclePilotLabel
            //
            this.lblVehiclePilotLabel.AutoSize = true;
            this.lblVehiclePilotLabel.Location = new System.Drawing.Point(416, 104);
            this.lblVehiclePilotLabel.Name = "lblVehiclePilotLabel";
            this.lblVehiclePilotLabel.Size = new System.Drawing.Size(30, 13);
            this.lblVehiclePilotLabel.TabIndex = 39;
            this.lblVehiclePilotLabel.Tag = "Label_Pilot";
            this.lblVehiclePilotLabel.Text = "Pilot:";
            //
            // lblVehicleArmor
            //
            this.lblVehicleArmor.AutoSize = true;
            this.lblVehicleArmor.Location = new System.Drawing.Point(690, 104);
            this.lblVehicleArmor.Name = "lblVehicleArmor";
            this.lblVehicleArmor.Size = new System.Drawing.Size(40, 13);
            this.lblVehicleArmor.TabIndex = 44;
            this.lblVehicleArmor.Text = "[Armor]";
            //
            // lblVehicleArmorLabel
            //
            this.lblVehicleArmorLabel.AutoSize = true;
            this.lblVehicleArmorLabel.Location = new System.Drawing.Point(626, 104);
            this.lblVehicleArmorLabel.Name = "lblVehicleArmorLabel";
            this.lblVehicleArmorLabel.Size = new System.Drawing.Size(37, 13);
            this.lblVehicleArmorLabel.TabIndex = 43;
            this.lblVehicleArmorLabel.Tag = "Label_Armor";
            this.lblVehicleArmorLabel.Text = "Armor:";
            //
            // lblVehicleBody
            //
            this.lblVehicleBody.AutoSize = true;
            this.lblVehicleBody.Location = new System.Drawing.Point(573, 104);
            this.lblVehicleBody.Name = "lblVehicleBody";
            this.lblVehicleBody.Size = new System.Drawing.Size(37, 13);
            this.lblVehicleBody.TabIndex = 42;
            this.lblVehicleBody.Text = "[Body]";
            //
            // lblVehicleBodyLabel
            //
            this.lblVehicleBodyLabel.AutoSize = true;
            this.lblVehicleBodyLabel.Location = new System.Drawing.Point(533, 104);
            this.lblVehicleBodyLabel.Name = "lblVehicleBodyLabel";
            this.lblVehicleBodyLabel.Size = new System.Drawing.Size(34, 13);
            this.lblVehicleBodyLabel.TabIndex = 41;
            this.lblVehicleBodyLabel.Tag = "Label_Body";
            this.lblVehicleBodyLabel.Text = "Body:";
            //
            // lblVehicleSpeed
            //
            this.lblVehicleSpeed.AutoSize = true;
            this.lblVehicleSpeed.Location = new System.Drawing.Point(690, 82);
            this.lblVehicleSpeed.Name = "lblVehicleSpeed";
            this.lblVehicleSpeed.Size = new System.Drawing.Size(44, 13);
            this.lblVehicleSpeed.TabIndex = 38;
            this.lblVehicleSpeed.Text = "[Speed]";
            //
            // lblVehicleSpeedLabel
            //
            this.lblVehicleSpeedLabel.AutoSize = true;
            this.lblVehicleSpeedLabel.Location = new System.Drawing.Point(626, 82);
            this.lblVehicleSpeedLabel.Name = "lblVehicleSpeedLabel";
            this.lblVehicleSpeedLabel.Size = new System.Drawing.Size(41, 13);
            this.lblVehicleSpeedLabel.TabIndex = 37;
            this.lblVehicleSpeedLabel.Tag = "Label_Speed";
            this.lblVehicleSpeedLabel.Text = "Speed:";
            //
            // lblVehicleCost
            //
            this.lblVehicleCost.AutoSize = true;
            this.lblVehicleCost.Location = new System.Drawing.Point(573, 150);
            this.lblVehicleCost.Name = "lblVehicleCost";
            this.lblVehicleCost.Size = new System.Drawing.Size(34, 13);
            this.lblVehicleCost.TabIndex = 50;
            this.lblVehicleCost.Text = "[Cost]";
            //
            // lblVehicleCostLabel
            //
            this.lblVehicleCostLabel.AutoSize = true;
            this.lblVehicleCostLabel.Location = new System.Drawing.Point(533, 150);
            this.lblVehicleCostLabel.Name = "lblVehicleCostLabel";
            this.lblVehicleCostLabel.Size = new System.Drawing.Size(31, 13);
            this.lblVehicleCostLabel.TabIndex = 49;
            this.lblVehicleCostLabel.Tag = "Label_Cost";
            this.lblVehicleCostLabel.Text = "Cost:";
            //
            // lblVehicleAvail
            //
            this.lblVehicleAvail.AutoSize = true;
            this.lblVehicleAvail.Location = new System.Drawing.Point(486, 150);
            this.lblVehicleAvail.Name = "lblVehicleAvail";
            this.lblVehicleAvail.Size = new System.Drawing.Size(36, 13);
            this.lblVehicleAvail.TabIndex = 48;
            this.lblVehicleAvail.Text = "[Avail]";
            //
            // lblVehicleAvailLabel
            //
            this.lblVehicleAvailLabel.AutoSize = true;
            this.lblVehicleAvailLabel.Location = new System.Drawing.Point(416, 150);
            this.lblVehicleAvailLabel.Name = "lblVehicleAvailLabel";
            this.lblVehicleAvailLabel.Size = new System.Drawing.Size(33, 13);
            this.lblVehicleAvailLabel.TabIndex = 47;
            this.lblVehicleAvailLabel.Tag = "Label_Avail";
            this.lblVehicleAvailLabel.Text = "Avail:";
            //
            // lblVehicleAccel
            //
            this.lblVehicleAccel.AutoSize = true;
            this.lblVehicleAccel.Location = new System.Drawing.Point(573, 82);
            this.lblVehicleAccel.Name = "lblVehicleAccel";
            this.lblVehicleAccel.Size = new System.Drawing.Size(40, 13);
            this.lblVehicleAccel.TabIndex = 36;
            this.lblVehicleAccel.Text = "[Accel]";
            //
            // lblVehicleAccelLabel
            //
            this.lblVehicleAccelLabel.AutoSize = true;
            this.lblVehicleAccelLabel.Location = new System.Drawing.Point(533, 82);
            this.lblVehicleAccelLabel.Name = "lblVehicleAccelLabel";
            this.lblVehicleAccelLabel.Size = new System.Drawing.Size(37, 13);
            this.lblVehicleAccelLabel.TabIndex = 35;
            this.lblVehicleAccelLabel.Tag = "Label_Accel";
            this.lblVehicleAccelLabel.Text = "Accel:";
            //
            // lblVehicleHandling
            //
            this.lblVehicleHandling.AutoSize = true;
            this.lblVehicleHandling.Location = new System.Drawing.Point(486, 82);
            this.lblVehicleHandling.Name = "lblVehicleHandling";
            this.lblVehicleHandling.Size = new System.Drawing.Size(55, 13);
            this.lblVehicleHandling.TabIndex = 34;
            this.lblVehicleHandling.Text = "[Handling]";
            //
            // lblVehicleHandlingLabel
            //
            this.lblVehicleHandlingLabel.AutoSize = true;
            this.lblVehicleHandlingLabel.Location = new System.Drawing.Point(416, 82);
            this.lblVehicleHandlingLabel.Name = "lblVehicleHandlingLabel";
            this.lblVehicleHandlingLabel.Size = new System.Drawing.Size(52, 13);
            this.lblVehicleHandlingLabel.TabIndex = 33;
            this.lblVehicleHandlingLabel.Tag = "Label_Handling";
            this.lblVehicleHandlingLabel.Text = "Handling:";
            //
            // cmdDeleteVehicle
            //
            this.cmdDeleteVehicle.AutoSize = true;
            this.cmdDeleteVehicle.Location = new System.Drawing.Point(104, 7);
            this.cmdDeleteVehicle.Name = "cmdDeleteVehicle";
            this.cmdDeleteVehicle.Size = new System.Drawing.Size(80, 23);
            this.cmdDeleteVehicle.TabIndex = 32;
            this.cmdDeleteVehicle.Tag = "String_Delete";
            this.cmdDeleteVehicle.Text = "Delete";
            this.cmdDeleteVehicle.UseVisualStyleBackColor = true;
            this.cmdDeleteVehicle.Click += new System.EventHandler(this.cmdDeleteVehicle_Click);
            //
            // treVehicles
            //
            this.treVehicles.AllowDrop = true;
            this.treVehicles.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)));
            this.treVehicles.HideSelection = false;
            this.treVehicles.Location = new System.Drawing.Point(8, 36);
            this.treVehicles.Name = "treVehicles";
            treeNode25.Name = "nodVehiclesRoot";
            treeNode25.Tag = "Node_SelectedVehicles";
            treeNode25.Text = "Selected Vehicles";
            this.treVehicles.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            treeNode25});
            this.treVehicles.ShowNodeToolTips = true;
            this.treVehicles.ShowRootLines = false;
            this.treVehicles.Size = new System.Drawing.Size(403, 572);
            this.treVehicles.TabIndex = 30;
            this.treVehicles.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treVehicles_AfterSelect);
            this.treVehicles.DragOver += new System.Windows.Forms.DragEventHandler(this.treVehicles_DragOver);
            this.treVehicles.KeyDown += new System.Windows.Forms.KeyEventHandler(this.treVehicles_KeyDown);
            this.treVehicles.MouseDown += new System.Windows.Forms.MouseEventHandler(this.TreeView_MouseDown);
            //
            // cmdAddVehicle
            //
            this.cmdAddVehicle.AutoSize = true;
            this.cmdAddVehicle.ContextMenuStrip = this.cmsVehicle;
            this.cmdAddVehicle.Location = new System.Drawing.Point(8, 7);
            this.cmdAddVehicle.Name = "cmdAddVehicle";
            this.cmdAddVehicle.Size = new System.Drawing.Size(92, 23);
            this.cmdAddVehicle.SplitMenuStrip = this.cmsVehicle;
            this.cmdAddVehicle.TabIndex = 185;
            this.cmdAddVehicle.Tag = "Button_AddVehicle";
            this.cmdAddVehicle.Text = "&Add Vehicle";
            this.cmdAddVehicle.UseVisualStyleBackColor = true;
            this.cmdAddVehicle.Click += new System.EventHandler(this.cmdAddVehicle_Click);
            //
            // tabCharacterInfo
            //
            this.tabCharacterInfo.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.tabCharacterInfo.Controls.Add(this.cboPrimaryArm);
            this.tabCharacterInfo.Controls.Add(this.lblHandedness);
            this.tabCharacterInfo.Controls.Add(this.chkIsMainMugshot);
            this.tabCharacterInfo.Controls.Add(this.lblNumMugshots);
            this.tabCharacterInfo.Controls.Add(this.nudMugshotIndex);
            this.tabCharacterInfo.Controls.Add(this.lblMugshotDimensions);
            this.tabCharacterInfo.Controls.Add(this.btnCreateBackstory);
            this.tabCharacterInfo.Controls.Add(this.lblPublicAwareTotal);
            this.tabCharacterInfo.Controls.Add(this.lblNotorietyTotal);
            this.tabCharacterInfo.Controls.Add(this.lblStreetCredTotal);
            this.tabCharacterInfo.Controls.Add(this.lblPublicAware);
            this.tabCharacterInfo.Controls.Add(this.lblNotoriety);
            this.tabCharacterInfo.Controls.Add(this.lblStreetCred);
            this.tabCharacterInfo.Controls.Add(this.lblCharacterName);
            this.tabCharacterInfo.Controls.Add(this.txtCharacterName);
            this.tabCharacterInfo.Controls.Add(this.txtPlayerName);
            this.tabCharacterInfo.Controls.Add(this.lblPlayerName);
            this.tabCharacterInfo.Controls.Add(this.chkCharacterCreated);
            this.tabCharacterInfo.Controls.Add(this.txtNotes);
            this.tabCharacterInfo.Controls.Add(this.lblNotes);
            this.tabCharacterInfo.Controls.Add(this.cmdDeleteMugshot);
            this.tabCharacterInfo.Controls.Add(this.cmdAddMugshot);
            this.tabCharacterInfo.Controls.Add(this.lblMugshot);
            this.tabCharacterInfo.Controls.Add(this.txtConcept);
            this.tabCharacterInfo.Controls.Add(this.lblConcept);
            this.tabCharacterInfo.Controls.Add(this.txtBackground);
            this.tabCharacterInfo.Controls.Add(this.lblBackground);
            this.tabCharacterInfo.Controls.Add(this.txtDescription);
            this.tabCharacterInfo.Controls.Add(this.lblDescription);
            this.tabCharacterInfo.Controls.Add(this.txtSkin);
            this.tabCharacterInfo.Controls.Add(this.lblSkin);
            this.tabCharacterInfo.Controls.Add(this.txtWeight);
            this.tabCharacterInfo.Controls.Add(this.lblWeight);
            this.tabCharacterInfo.Controls.Add(this.txtHeight);
            this.tabCharacterInfo.Controls.Add(this.lblHeight);
            this.tabCharacterInfo.Controls.Add(this.txtHair);
            this.tabCharacterInfo.Controls.Add(this.lblHair);
            this.tabCharacterInfo.Controls.Add(this.txtEyes);
            this.tabCharacterInfo.Controls.Add(this.lblEyes);
            this.tabCharacterInfo.Controls.Add(this.txtAge);
            this.tabCharacterInfo.Controls.Add(this.lblAge);
            this.tabCharacterInfo.Controls.Add(this.txtSex);
            this.tabCharacterInfo.Controls.Add(this.lblSex);
            this.tabCharacterInfo.Controls.Add(this.picMugshot);
            this.tabCharacterInfo.Location = new System.Drawing.Point(4, 22);
            this.tabCharacterInfo.Name = "tabCharacterInfo";
            this.tabCharacterInfo.Size = new System.Drawing.Size(838, 611);
            this.tabCharacterInfo.TabIndex = 9;
            this.tabCharacterInfo.Tag = "Tab_CharacterInfo";
            this.tabCharacterInfo.Text = "Character Info";
            //
            // cboPrimaryArm
            //
            this.cboPrimaryArm.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboPrimaryArm.FormattingEnabled = true;
            this.cboPrimaryArm.Location = new System.Drawing.Point(730, 61);
            this.cboPrimaryArm.Name = "cboPrimaryArm";
            this.cboPrimaryArm.Size = new System.Drawing.Size(100, 21);
            this.cboPrimaryArm.TabIndex = 93;
            this.cboPrimaryArm.SelectedIndexChanged += new System.EventHandler(this.cboPrimaryArm_SelectedIndexChanged);
            //
            // lblHandedness
            //
            this.lblHandedness.AutoSize = true;
            this.lblHandedness.Location = new System.Drawing.Point(654, 64);
            this.lblHandedness.Name = "lblHandedness";
            this.lblHandedness.Size = new System.Drawing.Size(70, 13);
            this.lblHandedness.TabIndex = 92;
            this.lblHandedness.Tag = "Label_Handedness";
            this.lblHandedness.Text = "Handedness:";
            //
            // chkIsMainMugshot
            //
            this.chkIsMainMugshot.AutoSize = true;
            this.chkIsMainMugshot.Location = new System.Drawing.Point(637, 224);
            this.chkIsMainMugshot.Name = "chkIsMainMugshot";
            this.chkIsMainMugshot.Size = new System.Drawing.Size(104, 17);
            this.chkIsMainMugshot.TabIndex = 95;
            this.chkIsMainMugshot.Text = "Is Main Mugshot";
            this.chkIsMainMugshot.UseVisualStyleBackColor = true;
            this.chkIsMainMugshot.CheckedChanged += new System.EventHandler(this.chkIsMainMugshot_CheckedChanged);
            //
            // lblNumMugshots
            //
            this.lblNumMugshots.AutoSize = true;
            this.lblNumMugshots.Location = new System.Drawing.Point(758, 169);
            this.lblNumMugshots.Name = "lblNumMugshots";
            this.lblNumMugshots.Size = new System.Drawing.Size(21, 13);
            this.lblNumMugshots.TabIndex = 94;
            this.lblNumMugshots.Tag = "";
            this.lblNumMugshots.Text = "/ 0";
            //
            // nudMugshotIndex
            //
            this.nudMugshotIndex.Location = new System.Drawing.Point(715, 166);
            this.nudMugshotIndex.Name = "nudMugshotIndex";
            this.nudMugshotIndex.Size = new System.Drawing.Size(42, 20);
            this.nudMugshotIndex.TabIndex = 93;
            this.nudMugshotIndex.ValueChanged += new System.EventHandler(this.nudMugshotIndex_ValueChanged);
            //
            // lblMugshotDimensions
            //
            this.lblMugshotDimensions.AutoSize = true;
            this.lblMugshotDimensions.Location = new System.Drawing.Point(764, 200);
            this.lblMugshotDimensions.Name = "lblMugshotDimensions";
            this.lblMugshotDimensions.Size = new System.Drawing.Size(78, 13);
            this.lblMugshotDimensions.TabIndex = 92;
            this.lblMugshotDimensions.Tag = "Label_MugshotDimensions";
            this.lblMugshotDimensions.Text = "210px X 310px";
            //
            // btnCreateBackstory
            //
            this.btnCreateBackstory.Location = new System.Drawing.Point(632, 128);
            this.btnCreateBackstory.Name = "btnCreateBackstory";
            this.btnCreateBackstory.Size = new System.Drawing.Size(97, 23);
            this.btnCreateBackstory.TabIndex = 91;
            this.btnCreateBackstory.Text = "Create Backstory";
            this.btnCreateBackstory.UseVisualStyleBackColor = true;
            this.btnCreateBackstory.Visible = false;
            this.btnCreateBackstory.Click += new System.EventHandler(this.btnCreateBackstory_Click);
            //
            // lblPublicAwareTotal
            //
            this.lblPublicAwareTotal.AutoSize = true;
            this.lblPublicAwareTotal.Location = new System.Drawing.Point(784, 143);
            this.lblPublicAwareTotal.Name = "lblPublicAwareTotal";
            this.lblPublicAwareTotal.Size = new System.Drawing.Size(19, 13);
            this.lblPublicAwareTotal.TabIndex = 90;
            this.lblPublicAwareTotal.Tag = "Label_StreetCred";
            this.lblPublicAwareTotal.Text = "[0]";
            //
            // lblNotorietyTotal
            //
            this.lblNotorietyTotal.AutoSize = true;
            this.lblNotorietyTotal.Location = new System.Drawing.Point(784, 116);
            this.lblNotorietyTotal.Name = "lblNotorietyTotal";
            this.lblNotorietyTotal.Size = new System.Drawing.Size(19, 13);
            this.lblNotorietyTotal.TabIndex = 89;
            this.lblNotorietyTotal.Tag = "Label_StreetCred";
            this.lblNotorietyTotal.Text = "[0]";
            //
            // lblStreetCredTotal
            //
            this.lblStreetCredTotal.AutoSize = true;
            this.lblStreetCredTotal.Location = new System.Drawing.Point(784, 90);
            this.lblStreetCredTotal.Name = "lblStreetCredTotal";
            this.lblStreetCredTotal.Size = new System.Drawing.Size(19, 13);
            this.lblStreetCredTotal.TabIndex = 88;
            this.lblStreetCredTotal.Tag = "Label_StreetCred";
            this.lblStreetCredTotal.Text = "[0]";
            //
            // lblCharacterName
            //
            this.lblCharacterName.AutoSize = true;
            this.lblCharacterName.Location = new System.Drawing.Point(493, 38);
            this.lblCharacterName.Name = "lblCharacterName";
            this.lblCharacterName.Size = new System.Drawing.Size(38, 13);
            this.lblCharacterName.TabIndex = 65;
            this.lblCharacterName.Tag = "Label_CharacterName";
            this.lblCharacterName.Text = "Name:";
            //
            // txtCharacterName
            //
            this.txtCharacterName.Location = new System.Drawing.Point(543, 35);
            this.txtCharacterName.Name = "txtCharacterName";
            this.txtCharacterName.Size = new System.Drawing.Size(100, 20);
            this.txtCharacterName.TabIndex = 66;
            this.txtCharacterName.TextChanged += new System.EventHandler(this.txtCharacterName_TextChanged);
            //
            // txtPlayerName
            //
            this.txtPlayerName.Location = new System.Drawing.Point(705, 35);
            this.txtPlayerName.Name = "txtPlayerName";
            this.txtPlayerName.Size = new System.Drawing.Size(100, 20);
            this.txtPlayerName.TabIndex = 64;
            this.txtPlayerName.TextChanged += new System.EventHandler(this.txtPlayerName_TextChanged);
            //
            // lblPlayerName
            //
            this.lblPlayerName.AutoSize = true;
            this.lblPlayerName.Location = new System.Drawing.Point(654, 38);
            this.lblPlayerName.Name = "lblPlayerName";
            this.lblPlayerName.Size = new System.Drawing.Size(39, 13);
            this.lblPlayerName.TabIndex = 63;
            this.lblPlayerName.Tag = "Label_Player";
            this.lblPlayerName.Text = "Player:";
            //
            // txtNotes
            //
            this.txtNotes.Location = new System.Drawing.Point(10, 434);
            this.txtNotes.Multiline = true;
            this.txtNotes.Name = "txtNotes";
            this.txtNotes.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtNotes.Size = new System.Drawing.Size(618, 100);
            this.txtNotes.TabIndex = 25;
            this.txtNotes.TextChanged += new System.EventHandler(this.txtNotes_TextChanged);
            this.txtNotes.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtNotes_KeyDown);
            //
            // lblNotes
            //
            this.lblNotes.AutoSize = true;
            this.lblNotes.Location = new System.Drawing.Point(8, 418);
            this.lblNotes.Name = "lblNotes";
            this.lblNotes.Size = new System.Drawing.Size(38, 13);
            this.lblNotes.TabIndex = 24;
            this.lblNotes.Tag = "Label_Notes";
            this.lblNotes.Text = "Notes:";
            //
            // cmdDeleteMugshot
            //
            this.cmdDeleteMugshot.Location = new System.Drawing.Point(701, 195);
            this.cmdDeleteMugshot.Name = "cmdDeleteMugshot";
            this.cmdDeleteMugshot.Size = new System.Drawing.Size(58, 23);
            this.cmdDeleteMugshot.TabIndex = 23;
            this.cmdDeleteMugshot.Tag = "String_Delete";
            this.cmdDeleteMugshot.Text = "Delete";
            this.cmdDeleteMugshot.UseVisualStyleBackColor = true;
            this.cmdDeleteMugshot.Click += new System.EventHandler(this.cmdDeleteMugshot_Click);
            //
            // cmdAddMugshot
            //
            this.cmdAddMugshot.Location = new System.Drawing.Point(637, 195);
            this.cmdAddMugshot.Name = "cmdAddMugshot";
            this.cmdAddMugshot.Size = new System.Drawing.Size(58, 23);
            this.cmdAddMugshot.TabIndex = 22;
            this.cmdAddMugshot.Tag = "Button_AddMugshot";
            this.cmdAddMugshot.Text = "Add";
            this.cmdAddMugshot.UseVisualStyleBackColor = true;
            this.cmdAddMugshot.Click += new System.EventHandler(this.cmdAddMugshot_Click);
            //
            // lblMugshot
            //
            this.lblMugshot.AutoSize = true;
            this.lblMugshot.Location = new System.Drawing.Point(634, 168);
            this.lblMugshot.Name = "lblMugshot";
            this.lblMugshot.Size = new System.Drawing.Size(51, 13);
            this.lblMugshot.TabIndex = 21;
            this.lblMugshot.Tag = "Label_Mugshot";
            this.lblMugshot.Text = "Mugshot:";
            //
            // txtConcept
            //
            this.txtConcept.Location = new System.Drawing.Point(10, 315);
            this.txtConcept.Multiline = true;
            this.txtConcept.Name = "txtConcept";
            this.txtConcept.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtConcept.Size = new System.Drawing.Size(618, 100);
            this.txtConcept.TabIndex = 19;
            this.txtConcept.TextChanged += new System.EventHandler(this.txtConcept_TextChanged);
            this.txtConcept.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtNotes_KeyDown);
            //
            // lblConcept
            //
            this.lblConcept.AutoSize = true;
            this.lblConcept.Location = new System.Drawing.Point(8, 299);
            this.lblConcept.Name = "lblConcept";
            this.lblConcept.Size = new System.Drawing.Size(50, 13);
            this.lblConcept.TabIndex = 18;
            this.lblConcept.Tag = "Label_Concept";
            this.lblConcept.Text = "Concept:";
            //
            // txtBackground
            //
            this.txtBackground.Location = new System.Drawing.Point(10, 200);
            this.txtBackground.Multiline = true;
            this.txtBackground.Name = "txtBackground";
            this.txtBackground.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtBackground.Size = new System.Drawing.Size(618, 96);
            this.txtBackground.TabIndex = 17;
            this.txtBackground.TextChanged += new System.EventHandler(this.txtBackground_TextChanged);
            this.txtBackground.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtNotes_KeyDown);
            this.txtBackground.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtBackground_KeyPress);
            //
            // lblBackground
            //
            this.lblBackground.AutoSize = true;
            this.lblBackground.Location = new System.Drawing.Point(8, 184);
            this.lblBackground.Name = "lblBackground";
            this.lblBackground.Size = new System.Drawing.Size(68, 13);
            this.lblBackground.TabIndex = 16;
            this.lblBackground.Tag = "Label_Background";
            this.lblBackground.Text = "Background:";
            //
            // txtDescription
            //
            this.txtDescription.Location = new System.Drawing.Point(10, 80);
            this.txtDescription.Multiline = true;
            this.txtDescription.Name = "txtDescription";
            this.txtDescription.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtDescription.Size = new System.Drawing.Size(618, 101);
            this.txtDescription.TabIndex = 15;
            this.txtDescription.TextChanged += new System.EventHandler(this.txtDescription_TextChanged);
            this.txtDescription.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtNotes_KeyDown);
            //
            // lblDescription
            //
            this.lblDescription.AutoSize = true;
            this.lblDescription.Location = new System.Drawing.Point(8, 64);
            this.lblDescription.Name = "lblDescription";
            this.lblDescription.Size = new System.Drawing.Size(63, 13);
            this.lblDescription.TabIndex = 14;
            this.lblDescription.Tag = "Label_Description";
            this.lblDescription.Text = "Description:";
            //
            // txtSkin
            //
            this.txtSkin.Location = new System.Drawing.Point(375, 35);
            this.txtSkin.Name = "txtSkin";
            this.txtSkin.Size = new System.Drawing.Size(100, 20);
            this.txtSkin.TabIndex = 13;
            this.txtSkin.TextChanged += new System.EventHandler(this.txtSkin_TextChanged);
            //
            // lblSkin
            //
            this.lblSkin.AutoSize = true;
            this.lblSkin.Location = new System.Drawing.Point(340, 38);
            this.lblSkin.Name = "lblSkin";
            this.lblSkin.Size = new System.Drawing.Size(31, 13);
            this.lblSkin.TabIndex = 12;
            this.lblSkin.Tag = "Label_Skin";
            this.lblSkin.Text = "Skin:";
            //
            // txtWeight
            //
            this.txtWeight.Location = new System.Drawing.Point(222, 35);
            this.txtWeight.Name = "txtWeight";
            this.txtWeight.Size = new System.Drawing.Size(100, 20);
            this.txtWeight.TabIndex = 11;
            this.txtWeight.TextChanged += new System.EventHandler(this.txtWeight_TextChanged);
            //
            // lblWeight
            //
            this.lblWeight.AutoSize = true;
            this.lblWeight.Location = new System.Drawing.Point(172, 38);
            this.lblWeight.Name = "lblWeight";
            this.lblWeight.Size = new System.Drawing.Size(44, 13);
            this.lblWeight.TabIndex = 10;
            this.lblWeight.Tag = "Label_Weight";
            this.lblWeight.Text = "Weight:";
            //
            // txtHeight
            //
            this.txtHeight.Location = new System.Drawing.Point(54, 35);
            this.txtHeight.Name = "txtHeight";
            this.txtHeight.Size = new System.Drawing.Size(100, 20);
            this.txtHeight.TabIndex = 9;
            this.txtHeight.TextChanged += new System.EventHandler(this.txtHeight_TextChanged);
            //
            // lblHeight
            //
            this.lblHeight.AutoSize = true;
            this.lblHeight.Location = new System.Drawing.Point(8, 38);
            this.lblHeight.Name = "lblHeight";
            this.lblHeight.Size = new System.Drawing.Size(41, 13);
            this.lblHeight.TabIndex = 8;
            this.lblHeight.Tag = "Label_Height";
            this.lblHeight.Text = "Height:";
            //
            // txtHair
            //
            this.txtHair.Location = new System.Drawing.Point(543, 8);
            this.txtHair.Name = "txtHair";
            this.txtHair.Size = new System.Drawing.Size(100, 20);
            this.txtHair.TabIndex = 7;
            this.txtHair.TextChanged += new System.EventHandler(this.txtHair_TextChanged);
            //
            // lblHair
            //
            this.lblHair.AutoSize = true;
            this.lblHair.Location = new System.Drawing.Point(493, 12);
            this.lblHair.Name = "lblHair";
            this.lblHair.Size = new System.Drawing.Size(29, 13);
            this.lblHair.TabIndex = 6;
            this.lblHair.Tag = "Label_Hair";
            this.lblHair.Text = "Hair:";
            //
            // txtEyes
            //
            this.txtEyes.Location = new System.Drawing.Point(375, 9);
            this.txtEyes.Name = "txtEyes";
            this.txtEyes.Size = new System.Drawing.Size(100, 20);
            this.txtEyes.TabIndex = 5;
            this.txtEyes.TextChanged += new System.EventHandler(this.txtEyes_TextChanged);
            //
            // lblEyes
            //
            this.lblEyes.AutoSize = true;
            this.lblEyes.Location = new System.Drawing.Point(340, 12);
            this.lblEyes.Name = "lblEyes";
            this.lblEyes.Size = new System.Drawing.Size(33, 13);
            this.lblEyes.TabIndex = 4;
            this.lblEyes.Tag = "Label_Eyes";
            this.lblEyes.Text = "Eyes:";
            //
            // txtAge
            //
            this.txtAge.Location = new System.Drawing.Point(222, 9);
            this.txtAge.Name = "txtAge";
            this.txtAge.Size = new System.Drawing.Size(100, 20);
            this.txtAge.TabIndex = 3;
            this.txtAge.TextChanged += new System.EventHandler(this.txtAge_TextChanged);
            //
            // lblAge
            //
            this.lblAge.AutoSize = true;
            this.lblAge.Location = new System.Drawing.Point(172, 12);
            this.lblAge.Name = "lblAge";
            this.lblAge.Size = new System.Drawing.Size(29, 13);
            this.lblAge.TabIndex = 2;
            this.lblAge.Tag = "Label_Age";
            this.lblAge.Text = "Age:";
            //
            // txtSex
            //
            this.txtSex.Location = new System.Drawing.Point(54, 9);
            this.txtSex.Name = "txtSex";
            this.txtSex.Size = new System.Drawing.Size(100, 20);
            this.txtSex.TabIndex = 1;
            this.txtSex.TextChanged += new System.EventHandler(this.txtSex_TextChanged);
            //
            // lblSex
            //
            this.lblSex.AutoSize = true;
            this.lblSex.Location = new System.Drawing.Point(8, 12);
            this.lblSex.Name = "lblSex";
            this.lblSex.Size = new System.Drawing.Size(28, 13);
            this.lblSex.TabIndex = 0;
            this.lblSex.Tag = "Label_Sex";
            this.lblSex.Text = "Sex:";
            //
            // picMugshot
            //
            this.picMugshot.Location = new System.Drawing.Point(628, 247);
            this.picMugshot.Name = "picMugshot";
            this.picMugshot.Size = new System.Drawing.Size(210, 310);
            this.picMugshot.TabIndex = 20;
            this.picMugshot.TabStop = false;
            //
            // tabImprovements
            //
            this.tabImprovements.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.tabImprovements.Controls.Add(this.panImprovements);
            this.tabImprovements.Location = new System.Drawing.Point(4, 22);
            this.tabImprovements.Name = "tabImprovements";
            this.tabImprovements.Size = new System.Drawing.Size(838, 611);
            this.tabImprovements.TabIndex = 12;
            this.tabImprovements.Tag = "Tab_Improvements";
            this.tabImprovements.Text = "Improvements";
            //
            // panImprovements
            //
            this.panImprovements.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panImprovements.AutoScroll = true;
            this.panImprovements.Location = new System.Drawing.Point(11, 9);
            this.panImprovements.Margin = new System.Windows.Forms.Padding(0);
            this.panImprovements.Name = "panImprovements";
            this.panImprovements.Size = new System.Drawing.Size(827, 599);
            this.panImprovements.TabIndex = 0;
            //
            // tabInfo
            //
            this.tabInfo.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabInfo.Controls.Add(this.tabBPSummary);
            this.tabInfo.Controls.Add(this.tabOtherInfo);
            this.tabInfo.Controls.Add(this.tabDefences);
            this.tabInfo.Location = new System.Drawing.Point(0, 0);
            this.tabInfo.Name = "tabInfo";
            this.tabInfo.SelectedIndex = 0;
            this.tabInfo.Size = new System.Drawing.Size(187, 637);
            this.tabInfo.TabIndex = 50;
            //
            // tabBPSummary
            //
            this.tabBPSummary.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.tabBPSummary.Controls.Add(this.lblPBuildSpecial);
            this.tabBPSummary.Controls.Add(this.lblPBuildSpecialLabel);
            this.tabBPSummary.Controls.Add(this.lblBuildRitualsBPLabel);
            this.tabBPSummary.Controls.Add(this.lblBuildRitualsBP);
            this.tabBPSummary.Controls.Add(this.lblBuildPrepsBPLabel);
            this.tabBPSummary.Controls.Add(this.lblBuildPrepsBP);
            this.tabBPSummary.Controls.Add(this.lblAINormalProgramsBP);
            this.tabBPSummary.Controls.Add(this.lblBuildAINormalPrograms);
            this.tabBPSummary.Controls.Add(this.lblAIAdvancedProgramsBP);
            this.tabBPSummary.Controls.Add(this.lblBuildAIAdvancedPrograms);
            this.tabBPSummary.Controls.Add(this.lblInitiationBP);
            this.tabBPSummary.Controls.Add(this.lblBuildInitiation);
            this.tabBPSummary.Controls.Add(this.lblBuildFoci);
            this.tabBPSummary.Controls.Add(this.lblFociBP);
            this.tabBPSummary.Controls.Add(this.lblManeuversBP);
            this.tabBPSummary.Controls.Add(this.lblBuildManeuvers);
            this.tabBPSummary.Controls.Add(this.lblNuyenBP);
            this.tabBPSummary.Controls.Add(this.lblBuildNuyen);
            this.tabBPSummary.Controls.Add(this.lblEnemiesBP);
            this.tabBPSummary.Controls.Add(this.lblBuildEnemies);
            this.tabBPSummary.Controls.Add(this.lblKarmaMetatypeBP);
            this.tabBPSummary.Controls.Add(this.label12);
            this.tabBPSummary.Controls.Add(this.lblComplexFormsBP);
            this.tabBPSummary.Controls.Add(this.lblBuildComplexForms);
            this.tabBPSummary.Controls.Add(this.lblBuildSprites);
            this.tabBPSummary.Controls.Add(this.lblBuildSpirits);
            this.tabBPSummary.Controls.Add(this.lblSpiritsBP);
            this.tabBPSummary.Controls.Add(this.lblSpritesBP);
            this.tabBPSummary.Controls.Add(this.lblBuildSpells);
            this.tabBPSummary.Controls.Add(this.lblKnowledgeSkillsBP);
            this.tabBPSummary.Controls.Add(this.lblBuildKnowledgeSkills);
            this.tabBPSummary.Controls.Add(this.lblActiveSkillsBP);
            this.tabBPSummary.Controls.Add(this.lblSpellsBP);
            this.tabBPSummary.Controls.Add(this.lblBuildActiveSkills);
            this.tabBPSummary.Controls.Add(this.lblSkillGroupsBP);
            this.tabBPSummary.Controls.Add(this.lblBuildSkillGroups);
            this.tabBPSummary.Controls.Add(this.lblBuildContacts);
            this.tabBPSummary.Controls.Add(this.lblBuildPrimaryAttributes);
            this.tabBPSummary.Controls.Add(this.lblBuildNegativeQualities);
            this.tabBPSummary.Controls.Add(this.lblBuildPositiveQualities);
            this.tabBPSummary.Controls.Add(this.lblPositiveQualitiesBP);
            this.tabBPSummary.Controls.Add(this.lblNegativeQualitiesBP);
            this.tabBPSummary.Controls.Add(this.lblAttributesBP);
            this.tabBPSummary.Controls.Add(this.lblContactsBP);
            this.tabBPSummary.Location = new System.Drawing.Point(4, 22);
            this.tabBPSummary.Name = "tabBPSummary";
            this.tabBPSummary.Padding = new System.Windows.Forms.Padding(3);
            this.tabBPSummary.Size = new System.Drawing.Size(179, 611);
            this.tabBPSummary.TabIndex = 0;
            this.tabBPSummary.Tag = "Tab_BPSummary";
            this.tabBPSummary.Text = "Karma Summary";
            //
            // lblPBuildSpecial
            //
            this.lblPBuildSpecial.AutoSize = true;
            this.lblPBuildSpecial.Location = new System.Drawing.Point(126, 51);
            this.lblPBuildSpecial.Name = "lblPBuildSpecial";
            this.lblPBuildSpecial.Size = new System.Drawing.Size(34, 13);
            this.lblPBuildSpecial.TabIndex = 135;
            this.lblPBuildSpecial.Text = "0 of 0";
            //
            // lblPBuildSpecialLabel
            //
            this.lblPBuildSpecialLabel.AutoSize = true;
            this.lblPBuildSpecialLabel.Location = new System.Drawing.Point(10, 51);
            this.lblPBuildSpecialLabel.Name = "lblPBuildSpecialLabel";
            this.lblPBuildSpecialLabel.Size = new System.Drawing.Size(89, 13);
            this.lblPBuildSpecialLabel.TabIndex = 134;
            this.lblPBuildSpecialLabel.Tag = "String_Special";
            this.lblPBuildSpecialLabel.Text = "Special Attributes";
            //
            // lblBuildRitualsBP
            //
            this.lblBuildRitualsBP.AutoSize = true;
            this.lblBuildRitualsBP.Location = new System.Drawing.Point(126, 282);
            this.lblBuildRitualsBP.Name = "lblBuildRitualsBP";
            this.lblBuildRitualsBP.Size = new System.Drawing.Size(30, 13);
            this.lblBuildRitualsBP.TabIndex = 132;
            this.lblBuildRitualsBP.Text = "0 BP";
            //
            // lblBuildPrepsBP
            //
            this.lblBuildPrepsBP.AutoSize = true;
            this.lblBuildPrepsBP.Location = new System.Drawing.Point(126, 261);
            this.lblBuildPrepsBP.Name = "lblBuildPrepsBP";
            this.lblBuildPrepsBP.Size = new System.Drawing.Size(30, 13);
            this.lblBuildPrepsBP.TabIndex = 130;
            this.lblBuildPrepsBP.Text = "0 BP";
            //
            // lblAINormalProgramsBP
            //
            this.lblAINormalProgramsBP.AutoSize = true;
            this.lblAINormalProgramsBP.Location = new System.Drawing.Point(126, 429);
            this.lblAINormalProgramsBP.Name = "lblAINormalProgramsBP";
            this.lblAINormalProgramsBP.Size = new System.Drawing.Size(30, 13);
            this.lblAINormalProgramsBP.TabIndex = 88;
            this.lblAINormalProgramsBP.Text = "0 BP";
            //
            // lblBuildAINormalPrograms
            //
            this.lblBuildAINormalPrograms.AutoSize = true;
            this.lblBuildAINormalPrograms.Location = new System.Drawing.Point(10, 429);
            this.lblBuildAINormalPrograms.Name = "lblBuildAINormalPrograms";
            this.lblBuildAINormalPrograms.Size = new System.Drawing.Size(87, 13);
            this.lblBuildAINormalPrograms.TabIndex = 89;
            this.lblBuildAINormalPrograms.Tag = "Label_SummaryAINormalPrograms";
            this.lblBuildAINormalPrograms.Text = "Normal Programs";
            //
            // lblAIAdvancedProgramsBP
            //
            this.lblAIAdvancedProgramsBP.AutoSize = true;
            this.lblAIAdvancedProgramsBP.Location = new System.Drawing.Point(126, 450);
            this.lblAIAdvancedProgramsBP.Name = "lblAIAdvancedProgramsBP";
            this.lblAIAdvancedProgramsBP.Size = new System.Drawing.Size(30, 13);
            this.lblAIAdvancedProgramsBP.TabIndex = 86;
            this.lblAIAdvancedProgramsBP.Text = "0 BP";
            //
            // lblInitiationBP
            //
            this.lblInitiationBP.AutoSize = true;
            this.lblInitiationBP.Location = new System.Drawing.Point(126, 387);
            this.lblInitiationBP.Name = "lblInitiationBP";
            this.lblInitiationBP.Size = new System.Drawing.Size(30, 13);
            this.lblInitiationBP.TabIndex = 82;
            this.lblInitiationBP.Text = "0 BP";
            //
            // lblBuildInitiation
            //
            this.lblBuildInitiation.AutoSize = true;
            this.lblBuildInitiation.Location = new System.Drawing.Point(10, 387);
            this.lblBuildInitiation.Name = "lblBuildInitiation";
            this.lblBuildInitiation.Size = new System.Drawing.Size(106, 13);
            this.lblBuildInitiation.TabIndex = 83;
            this.lblBuildInitiation.Tag = "Label_SummaryInitiation";
            this.lblBuildInitiation.Text = "Initiation/Submersion";
            //
            // lblFociBP
            //
            this.lblFociBP.AutoSize = true;
            this.lblFociBP.Location = new System.Drawing.Point(126, 303);
            this.lblFociBP.Name = "lblFociBP";
            this.lblFociBP.Size = new System.Drawing.Size(30, 13);
            this.lblFociBP.TabIndex = 80;
            this.lblFociBP.Text = "0 BP";
            //
            // lblManeuversBP
            //
            this.lblManeuversBP.AutoSize = true;
            this.lblManeuversBP.Location = new System.Drawing.Point(126, 408);
            this.lblManeuversBP.Name = "lblManeuversBP";
            this.lblManeuversBP.Size = new System.Drawing.Size(30, 13);
            this.lblManeuversBP.TabIndex = 78;
            this.lblManeuversBP.Text = "0 BP";
            //
            // lblNuyenBP
            //
            this.lblNuyenBP.AutoSize = true;
            this.lblNuyenBP.Location = new System.Drawing.Point(126, 156);
            this.lblNuyenBP.Name = "lblNuyenBP";
            this.lblNuyenBP.Size = new System.Drawing.Size(30, 13);
            this.lblNuyenBP.TabIndex = 77;
            this.lblNuyenBP.Text = "0 BP";
            //
            // lblEnemiesBP
            //
            this.lblEnemiesBP.AutoSize = true;
            this.lblEnemiesBP.Location = new System.Drawing.Point(126, 135);
            this.lblEnemiesBP.Name = "lblEnemiesBP";
            this.lblEnemiesBP.Size = new System.Drawing.Size(30, 13);
            this.lblEnemiesBP.TabIndex = 66;
            this.lblEnemiesBP.Text = "0 BP";
            //
            // lblKarmaMetatypeBP
            //
            this.lblKarmaMetatypeBP.AutoSize = true;
            this.lblKarmaMetatypeBP.Location = new System.Drawing.Point(126, 9);
            this.lblKarmaMetatypeBP.Name = "lblKarmaMetatypeBP";
            this.lblKarmaMetatypeBP.Size = new System.Drawing.Size(30, 13);
            this.lblKarmaMetatypeBP.TabIndex = 74;
            this.lblKarmaMetatypeBP.Text = "0 BP";
            //
            // label12
            //
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(10, 9);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(51, 13);
            this.label12.TabIndex = 73;
            this.label12.Tag = "Label_SummaryMetatype";
            this.label12.Text = "Metatype";
            //
            // lblComplexFormsBP
            //
            this.lblComplexFormsBP.AutoSize = true;
            this.lblComplexFormsBP.Location = new System.Drawing.Point(126, 366);
            this.lblComplexFormsBP.Name = "lblComplexFormsBP";
            this.lblComplexFormsBP.Size = new System.Drawing.Size(30, 13);
            this.lblComplexFormsBP.TabIndex = 61;
            this.lblComplexFormsBP.Text = "0 BP";
            //
            // lblSpiritsBP
            //
            this.lblSpiritsBP.AutoSize = true;
            this.lblSpiritsBP.Location = new System.Drawing.Point(126, 324);
            this.lblSpiritsBP.Name = "lblSpiritsBP";
            this.lblSpiritsBP.Size = new System.Drawing.Size(30, 13);
            this.lblSpiritsBP.TabIndex = 52;
            this.lblSpiritsBP.Text = "0 BP";
            //
            // lblSpritesBP
            //
            this.lblSpritesBP.AutoSize = true;
            this.lblSpritesBP.Location = new System.Drawing.Point(126, 345);
            this.lblSpritesBP.Name = "lblSpritesBP";
            this.lblSpritesBP.Size = new System.Drawing.Size(30, 13);
            this.lblSpritesBP.TabIndex = 58;
            this.lblSpritesBP.Text = "0 BP";
            //
            // lblKnowledgeSkillsBP
            //
            this.lblKnowledgeSkillsBP.AutoSize = true;
            this.lblKnowledgeSkillsBP.Location = new System.Drawing.Point(126, 219);
            this.lblKnowledgeSkillsBP.Name = "lblKnowledgeSkillsBP";
            this.lblKnowledgeSkillsBP.Size = new System.Drawing.Size(30, 13);
            this.lblKnowledgeSkillsBP.TabIndex = 68;
            this.lblKnowledgeSkillsBP.Text = "0 BP";
            //
            // lblActiveSkillsBP
            //
            this.lblActiveSkillsBP.AutoSize = true;
            this.lblActiveSkillsBP.Location = new System.Drawing.Point(126, 198);
            this.lblActiveSkillsBP.Name = "lblActiveSkillsBP";
            this.lblActiveSkillsBP.Size = new System.Drawing.Size(30, 13);
            this.lblActiveSkillsBP.TabIndex = 67;
            this.lblActiveSkillsBP.Text = "0 BP";
            //
            // lblSpellsBP
            //
            this.lblSpellsBP.AutoSize = true;
            this.lblSpellsBP.Location = new System.Drawing.Point(126, 240);
            this.lblSpellsBP.Name = "lblSpellsBP";
            this.lblSpellsBP.Size = new System.Drawing.Size(30, 13);
            this.lblSpellsBP.TabIndex = 51;
            this.lblSpellsBP.Text = "0 BP";
            //
            // lblSkillGroupsBP
            //
            this.lblSkillGroupsBP.AutoSize = true;
            this.lblSkillGroupsBP.Location = new System.Drawing.Point(126, 177);
            this.lblSkillGroupsBP.Name = "lblSkillGroupsBP";
            this.lblSkillGroupsBP.Size = new System.Drawing.Size(30, 13);
            this.lblSkillGroupsBP.TabIndex = 65;
            this.lblSkillGroupsBP.Text = "0 BP";
            //
            // lblPositiveQualitiesBP
            //
            this.lblPositiveQualitiesBP.AutoSize = true;
            this.lblPositiveQualitiesBP.Location = new System.Drawing.Point(126, 72);
            this.lblPositiveQualitiesBP.Name = "lblPositiveQualitiesBP";
            this.lblPositiveQualitiesBP.Size = new System.Drawing.Size(30, 13);
            this.lblPositiveQualitiesBP.TabIndex = 53;
            this.lblPositiveQualitiesBP.Text = "0 BP";
            //
            // lblNegativeQualitiesBP
            //
            this.lblNegativeQualitiesBP.AutoSize = true;
            this.lblNegativeQualitiesBP.Location = new System.Drawing.Point(126, 93);
            this.lblNegativeQualitiesBP.Name = "lblNegativeQualitiesBP";
            this.lblNegativeQualitiesBP.Size = new System.Drawing.Size(30, 13);
            this.lblNegativeQualitiesBP.TabIndex = 55;
            this.lblNegativeQualitiesBP.Text = "0 BP";
            //
            // lblAttributesBP
            //
            this.lblAttributesBP.AutoSize = true;
            this.lblAttributesBP.Location = new System.Drawing.Point(126, 30);
            this.lblAttributesBP.Name = "lblAttributesBP";
            this.lblAttributesBP.Size = new System.Drawing.Size(30, 13);
            this.lblAttributesBP.TabIndex = 56;
            this.lblAttributesBP.Text = "0 BP";
            //
            // lblContactsBP
            //
            this.lblContactsBP.AutoSize = true;
            this.lblContactsBP.Location = new System.Drawing.Point(126, 114);
            this.lblContactsBP.Name = "lblContactsBP";
            this.lblContactsBP.Size = new System.Drawing.Size(46, 13);
            this.lblContactsBP.TabIndex = 60;
            this.lblContactsBP.Text = "0 Karma";
            //
            // tabOtherInfo
            //
            this.tabOtherInfo.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.tabOtherInfo.Controls.Add(this.lblRiggingINI);
            this.tabOtherInfo.Controls.Add(this.lblRiggingINILabel);
            this.tabOtherInfo.Controls.Add(this.lblMatrixINIHot);
            this.tabOtherInfo.Controls.Add(this.lblMatrixINIHotLabel);
            this.tabOtherInfo.Controls.Add(this.lblMatrixINICold);
            this.tabOtherInfo.Controls.Add(this.lblMatrixINIColdLabel);
            this.tabOtherInfo.Controls.Add(this.lblFly);
            this.tabOtherInfo.Controls.Add(this.lblFlyLabel);
            this.tabOtherInfo.Controls.Add(this.lblSwim);
            this.tabOtherInfo.Controls.Add(this.lblSwimLabel);
            this.tabOtherInfo.Controls.Add(this.lblMemory);
            this.tabOtherInfo.Controls.Add(this.lblMemoryLabel);
            this.tabOtherInfo.Controls.Add(this.lblLiftCarry);
            this.tabOtherInfo.Controls.Add(this.lblLiftCarryLabel);
            this.tabOtherInfo.Controls.Add(this.lblJudgeIntentions);
            this.tabOtherInfo.Controls.Add(this.lblJudgeIntentionsLabel);
            this.tabOtherInfo.Controls.Add(this.lblComposure);
            this.tabOtherInfo.Controls.Add(this.lblComposureLabel);
            this.tabOtherInfo.Controls.Add(this.lblMovement);
            this.tabOtherInfo.Controls.Add(this.lblMovementLabel);
            this.tabOtherInfo.Controls.Add(this.lblRemainingNuyen);
            this.tabOtherInfo.Controls.Add(this.lblRemainingNuyenLabel);
            this.tabOtherInfo.Controls.Add(this.lblESSMax);
            this.tabOtherInfo.Controls.Add(this.lblESS);
            this.tabOtherInfo.Controls.Add(this.lblArmor);
            this.tabOtherInfo.Controls.Add(this.lblArmorLabel);
            this.tabOtherInfo.Controls.Add(this.lblAstralINI);
            this.tabOtherInfo.Controls.Add(this.lblMatrixINI);
            this.tabOtherInfo.Controls.Add(this.lblINI);
            this.tabOtherInfo.Controls.Add(this.lblCMStun);
            this.tabOtherInfo.Controls.Add(this.lblCMPhysical);
            this.tabOtherInfo.Controls.Add(this.lblAstralINILabel);
            this.tabOtherInfo.Controls.Add(this.lblMatrixINILabel);
            this.tabOtherInfo.Controls.Add(this.lblINILabel);
            this.tabOtherInfo.Controls.Add(this.lblCMStunLabel);
            this.tabOtherInfo.Controls.Add(this.lblCMPhysicalLabel);
            this.tabOtherInfo.Location = new System.Drawing.Point(4, 22);
            this.tabOtherInfo.Name = "tabOtherInfo";
            this.tabOtherInfo.Padding = new System.Windows.Forms.Padding(3);
            this.tabOtherInfo.Size = new System.Drawing.Size(179, 611);
            this.tabOtherInfo.TabIndex = 1;
            this.tabOtherInfo.Tag = "Tab_OtherInfo";
            this.tabOtherInfo.Text = "Other Info";
            //
            // lblRiggingINI
            //
            this.lblRiggingINI.AutoSize = true;
            this.lblRiggingINI.Location = new System.Drawing.Point(139, 167);
            this.lblRiggingINI.Name = "lblRiggingINI";
            this.lblRiggingINI.Size = new System.Drawing.Size(13, 13);
            this.lblRiggingINI.TabIndex = 76;
            this.lblRiggingINI.Text = "0";
            //
            // lblMatrixINIHot
            //
            this.lblMatrixINIHot.AutoSize = true;
            this.lblMatrixINIHot.Location = new System.Drawing.Point(139, 144);
            this.lblMatrixINIHot.Name = "lblMatrixINIHot";
            this.lblMatrixINIHot.Size = new System.Drawing.Size(13, 13);
            this.lblMatrixINIHot.TabIndex = 74;
            this.lblMatrixINIHot.Text = "0";
            //
            // lblMatrixINICold
            //
            this.lblMatrixINICold.AutoSize = true;
            this.lblMatrixINICold.Location = new System.Drawing.Point(139, 121);
            this.lblMatrixINICold.Name = "lblMatrixINICold";
            this.lblMatrixINICold.Size = new System.Drawing.Size(13, 13);
            this.lblMatrixINICold.TabIndex = 72;
            this.lblMatrixINICold.Text = "0";
            //
            // lblFly
            //
            this.lblFly.AutoSize = true;
            this.lblFly.Location = new System.Drawing.Point(139, 393);
            this.lblFly.Name = "lblFly";
            this.lblFly.Size = new System.Drawing.Size(13, 13);
            this.lblFly.TabIndex = 55;
            this.lblFly.Text = "0";
            //
            // lblFlyLabel
            //
            this.lblFlyLabel.AutoSize = true;
            this.lblFlyLabel.Location = new System.Drawing.Point(6, 393);
            this.lblFlyLabel.Name = "lblFlyLabel";
            this.lblFlyLabel.Size = new System.Drawing.Size(23, 13);
            this.lblFlyLabel.TabIndex = 54;
            this.lblFlyLabel.Tag = "Label_OtherFly";
            this.lblFlyLabel.Text = "Fly:";
            //
            // lblSwim
            //
            this.lblSwim.AutoSize = true;
            this.lblSwim.Location = new System.Drawing.Point(139, 370);
            this.lblSwim.Name = "lblSwim";
            this.lblSwim.Size = new System.Drawing.Size(13, 13);
            this.lblSwim.TabIndex = 53;
            this.lblSwim.Text = "0";
            //
            // lblSwimLabel
            //
            this.lblSwimLabel.AutoSize = true;
            this.lblSwimLabel.Location = new System.Drawing.Point(6, 370);
            this.lblSwimLabel.Name = "lblSwimLabel";
            this.lblSwimLabel.Size = new System.Drawing.Size(35, 13);
            this.lblSwimLabel.TabIndex = 52;
            this.lblSwimLabel.Tag = "Label_OtherSwim";
            this.lblSwimLabel.Text = "Swim:";
            //
            // lblMemory
            //
            this.lblMemory.AutoSize = true;
            this.lblMemory.Location = new System.Drawing.Point(139, 324);
            this.lblMemory.Name = "lblMemory";
            this.lblMemory.Size = new System.Drawing.Size(13, 13);
            this.lblMemory.TabIndex = 51;
            this.lblMemory.Text = "0";
            //
            // lblLiftCarry
            //
            this.lblLiftCarry.AutoSize = true;
            this.lblLiftCarry.Location = new System.Drawing.Point(139, 301);
            this.lblLiftCarry.Name = "lblLiftCarry";
            this.lblLiftCarry.Size = new System.Drawing.Size(13, 13);
            this.lblLiftCarry.TabIndex = 49;
            this.lblLiftCarry.Text = "0";
            //
            // lblJudgeIntentions
            //
            this.lblJudgeIntentions.AutoSize = true;
            this.lblJudgeIntentions.Location = new System.Drawing.Point(139, 278);
            this.lblJudgeIntentions.Name = "lblJudgeIntentions";
            this.lblJudgeIntentions.Size = new System.Drawing.Size(13, 13);
            this.lblJudgeIntentions.TabIndex = 47;
            this.lblJudgeIntentions.Text = "0";
            //
            // lblComposure
            //
            this.lblComposure.AutoSize = true;
            this.lblComposure.Location = new System.Drawing.Point(139, 255);
            this.lblComposure.Name = "lblComposure";
            this.lblComposure.Size = new System.Drawing.Size(13, 13);
            this.lblComposure.TabIndex = 45;
            this.lblComposure.Text = "0";
            //
            // lblMovement
            //
            this.lblMovement.AutoSize = true;
            this.lblMovement.Location = new System.Drawing.Point(139, 347);
            this.lblMovement.Name = "lblMovement";
            this.lblMovement.Size = new System.Drawing.Size(13, 13);
            this.lblMovement.TabIndex = 43;
            this.lblMovement.Text = "0";
            //
            // lblMovementLabel
            //
            this.lblMovementLabel.AutoSize = true;
            this.lblMovementLabel.Location = new System.Drawing.Point(6, 347);
            this.lblMovementLabel.Name = "lblMovementLabel";
            this.lblMovementLabel.Size = new System.Drawing.Size(60, 13);
            this.lblMovementLabel.TabIndex = 42;
            this.lblMovementLabel.Tag = "Label_OtherMovement";
            this.lblMovementLabel.Text = "Movement:";
            //
            // lblRemainingNuyen
            //
            this.lblRemainingNuyen.AutoSize = true;
            this.lblRemainingNuyen.Location = new System.Drawing.Point(139, 232);
            this.lblRemainingNuyen.Name = "lblRemainingNuyen";
            this.lblRemainingNuyen.Size = new System.Drawing.Size(13, 13);
            this.lblRemainingNuyen.TabIndex = 37;
            this.lblRemainingNuyen.Text = "0";
            //
            // lblESSMax
            //
            this.lblESSMax.AutoSize = true;
            this.lblESSMax.Location = new System.Drawing.Point(139, 209);
            this.lblESSMax.Name = "lblESSMax";
            this.lblESSMax.Size = new System.Drawing.Size(13, 13);
            this.lblESSMax.TabIndex = 35;
            this.lblESSMax.Text = "0";
            //
            // lblArmor
            //
            this.lblArmor.AutoSize = true;
            this.lblArmor.Location = new System.Drawing.Point(139, 188);
            this.lblArmor.Name = "lblArmor";
            this.lblArmor.Size = new System.Drawing.Size(13, 13);
            this.lblArmor.TabIndex = 31;
            this.lblArmor.Text = "0";
            //
            // lblAstralINI
            //
            this.lblAstralINI.AutoSize = true;
            this.lblAstralINI.Location = new System.Drawing.Point(139, 75);
            this.lblAstralINI.Name = "lblAstralINI";
            this.lblAstralINI.Size = new System.Drawing.Size(13, 13);
            this.lblAstralINI.TabIndex = 29;
            this.lblAstralINI.Text = "0";
            //
            // lblMatrixINI
            //
            this.lblMatrixINI.AutoSize = true;
            this.lblMatrixINI.Location = new System.Drawing.Point(139, 98);
            this.lblMatrixINI.Name = "lblMatrixINI";
            this.lblMatrixINI.Size = new System.Drawing.Size(13, 13);
            this.lblMatrixINI.TabIndex = 28;
            this.lblMatrixINI.Text = "0";
            //
            // lblINI
            //
            this.lblINI.AutoSize = true;
            this.lblINI.Location = new System.Drawing.Point(139, 53);
            this.lblINI.Name = "lblINI";
            this.lblINI.Size = new System.Drawing.Size(13, 13);
            this.lblINI.TabIndex = 26;
            this.lblINI.Text = "0";
            //
            // lblCMStun
            //
            this.lblCMStun.AutoSize = true;
            this.lblCMStun.Location = new System.Drawing.Point(139, 31);
            this.lblCMStun.Name = "lblCMStun";
            this.lblCMStun.Size = new System.Drawing.Size(13, 13);
            this.lblCMStun.TabIndex = 25;
            this.lblCMStun.Text = "0";
            //
            // lblCMPhysical
            //
            this.lblCMPhysical.AutoSize = true;
            this.lblCMPhysical.Location = new System.Drawing.Point(139, 9);
            this.lblCMPhysical.Name = "lblCMPhysical";
            this.lblCMPhysical.Size = new System.Drawing.Size(13, 13);
            this.lblCMPhysical.TabIndex = 24;
            this.lblCMPhysical.Text = "0";
            //
            // tabDefences
            //
            this.tabDefences.BackColor = System.Drawing.SystemColors.Control;
            this.tabDefences.Controls.Add(this.lblCounterspellingDiceLabel);
            this.tabDefences.Controls.Add(this.nudCounterspellingDice);
            this.tabDefences.Controls.Add(this.lbllSpellDefenceManipPhysical);
            this.tabDefences.Controls.Add(this.lbllSpellDefenceManipPhysicalLabel);
            this.tabDefences.Controls.Add(this.lblSpellDefenceManipMental);
            this.tabDefences.Controls.Add(this.lblSpellDefenceManipMentalLabel);
            this.tabDefences.Controls.Add(this.lblSpellDefenceIllusionPhysical);
            this.tabDefences.Controls.Add(this.lblSpellDefenceIllusionPhysicalLabel);
            this.tabDefences.Controls.Add(this.lblSpellDefenceIllusionMana);
            this.tabDefences.Controls.Add(this.lblSpellDefenceIllusionManaLabel);
            this.tabDefences.Controls.Add(this.lblSpellDefenceDecAttWIL);
            this.tabDefences.Controls.Add(this.lblSpellDefenceDecAttLOG);
            this.tabDefences.Controls.Add(this.lblSpellDefenceDecAttINT);
            this.tabDefences.Controls.Add(this.lblSpellDefenceDecAttCHA);
            this.tabDefences.Controls.Add(this.lblSpellDefenceDecAttSTR);
            this.tabDefences.Controls.Add(this.lblSpellDefenceDecAttWILLabel);
            this.tabDefences.Controls.Add(this.lblSpellDefenceDecAttLOGLabel);
            this.tabDefences.Controls.Add(this.lblSpellDefenceDecAttINTLabel);
            this.tabDefences.Controls.Add(this.lblSpellDefenceDecAttCHALabel);
            this.tabDefences.Controls.Add(this.lblSpellDefenceDecAttSTRLabel);
            this.tabDefences.Controls.Add(this.lblSpellDefenceDecAttREALabel);
            this.tabDefences.Controls.Add(this.lblSpellDefenceDecAttAGILabel);
            this.tabDefences.Controls.Add(this.lblSpellDefenceDecAttREA);
            this.tabDefences.Controls.Add(this.lblSpellDefenceDecAttAGI);
            this.tabDefences.Controls.Add(this.lblSpellDefenceDecAttBOD);
            this.tabDefences.Controls.Add(this.lblSpellDefenceDecAttBODLabel);
            this.tabDefences.Controls.Add(this.lblSpellDefenceDetection);
            this.tabDefences.Controls.Add(this.lblSpellDefenceDetectionLabel);
            this.tabDefences.Controls.Add(this.lblSpellDefenceDirectSoakPhysical);
            this.tabDefences.Controls.Add(this.lblSpellDefenceDirectSoakPhysicalLabel);
            this.tabDefences.Controls.Add(this.lblSpellDefenceDirectSoakMana);
            this.tabDefences.Controls.Add(this.lblSpellDefenceDirectSoakManaLabel);
            this.tabDefences.Controls.Add(this.lblSpellDefenceIndirectSoak);
            this.tabDefences.Controls.Add(this.lblSpellDefenceIndirectSoakLabel);
            this.tabDefences.Controls.Add(this.lblSpellDefenceIndirectDodge);
            this.tabDefences.Controls.Add(this.lblSpellDefenceIndirectDodgeLabel);
            this.tabDefences.Location = new System.Drawing.Point(4, 22);
            this.tabDefences.Name = "tabDefences";
            this.tabDefences.Padding = new System.Windows.Forms.Padding(3);
            this.tabDefences.Size = new System.Drawing.Size(179, 611);
            this.tabDefences.TabIndex = 3;
            this.tabDefences.Text = "Spell Defence";
            //
            // nudCounterspellingDice
            //
            this.nudCounterspellingDice.Location = new System.Drawing.Point(133, 9);
            this.nudCounterspellingDice.Name = "nudCounterspellingDice";
            this.nudCounterspellingDice.Size = new System.Drawing.Size(40, 20);
            this.nudCounterspellingDice.TabIndex = 61;
            this.nudCounterspellingDice.ValueChanged += new System.EventHandler(this.nudCounterspellingDice_Changed);
            //
            // lbllSpellDefenceManipPhysical
            //
            this.lbllSpellDefenceManipPhysical.AutoSize = true;
            this.lbllSpellDefenceManipPhysical.Location = new System.Drawing.Point(160, 352);
            this.lbllSpellDefenceManipPhysical.Name = "lbllSpellDefenceManipPhysical";
            this.lbllSpellDefenceManipPhysical.Size = new System.Drawing.Size(13, 13);
            this.lbllSpellDefenceManipPhysical.TabIndex = 60;
            this.lbllSpellDefenceManipPhysical.Text = "0";
            //
            // lblSpellDefenceManipMental
            //
            this.lblSpellDefenceManipMental.AutoSize = true;
            this.lblSpellDefenceManipMental.Location = new System.Drawing.Point(160, 332);
            this.lblSpellDefenceManipMental.Name = "lblSpellDefenceManipMental";
            this.lblSpellDefenceManipMental.Size = new System.Drawing.Size(13, 13);
            this.lblSpellDefenceManipMental.TabIndex = 58;
            this.lblSpellDefenceManipMental.Text = "0";
            //
            // lblSpellDefenceIllusionPhysical
            //
            this.lblSpellDefenceIllusionPhysical.AutoSize = true;
            this.lblSpellDefenceIllusionPhysical.Location = new System.Drawing.Point(160, 312);
            this.lblSpellDefenceIllusionPhysical.Name = "lblSpellDefenceIllusionPhysical";
            this.lblSpellDefenceIllusionPhysical.Size = new System.Drawing.Size(13, 13);
            this.lblSpellDefenceIllusionPhysical.TabIndex = 56;
            this.lblSpellDefenceIllusionPhysical.Text = "0";
            //
            // lblSpellDefenceIllusionMana
            //
            this.lblSpellDefenceIllusionMana.AutoSize = true;
            this.lblSpellDefenceIllusionMana.Location = new System.Drawing.Point(160, 292);
            this.lblSpellDefenceIllusionMana.Name = "lblSpellDefenceIllusionMana";
            this.lblSpellDefenceIllusionMana.Size = new System.Drawing.Size(13, 13);
            this.lblSpellDefenceIllusionMana.TabIndex = 54;
            this.lblSpellDefenceIllusionMana.Text = "0";
            //
            // lblSpellDefenceDecAttWIL
            //
            this.lblSpellDefenceDecAttWIL.AutoSize = true;
            this.lblSpellDefenceDecAttWIL.Location = new System.Drawing.Point(160, 272);
            this.lblSpellDefenceDecAttWIL.Name = "lblSpellDefenceDecAttWIL";
            this.lblSpellDefenceDecAttWIL.Size = new System.Drawing.Size(13, 13);
            this.lblSpellDefenceDecAttWIL.TabIndex = 52;
            this.lblSpellDefenceDecAttWIL.Text = "0";
            //
            // lblSpellDefenceDecAttLOG
            //
            this.lblSpellDefenceDecAttLOG.AutoSize = true;
            this.lblSpellDefenceDecAttLOG.Location = new System.Drawing.Point(160, 252);
            this.lblSpellDefenceDecAttLOG.Name = "lblSpellDefenceDecAttLOG";
            this.lblSpellDefenceDecAttLOG.Size = new System.Drawing.Size(13, 13);
            this.lblSpellDefenceDecAttLOG.TabIndex = 51;
            this.lblSpellDefenceDecAttLOG.Text = "0";
            //
            // lblSpellDefenceDecAttINT
            //
            this.lblSpellDefenceDecAttINT.AutoSize = true;
            this.lblSpellDefenceDecAttINT.Location = new System.Drawing.Point(160, 232);
            this.lblSpellDefenceDecAttINT.Name = "lblSpellDefenceDecAttINT";
            this.lblSpellDefenceDecAttINT.Size = new System.Drawing.Size(13, 13);
            this.lblSpellDefenceDecAttINT.TabIndex = 50;
            this.lblSpellDefenceDecAttINT.Text = "0";
            //
            // lblSpellDefenceDecAttCHA
            //
            this.lblSpellDefenceDecAttCHA.AutoSize = true;
            this.lblSpellDefenceDecAttCHA.Location = new System.Drawing.Point(160, 212);
            this.lblSpellDefenceDecAttCHA.Name = "lblSpellDefenceDecAttCHA";
            this.lblSpellDefenceDecAttCHA.Size = new System.Drawing.Size(13, 13);
            this.lblSpellDefenceDecAttCHA.TabIndex = 49;
            this.lblSpellDefenceDecAttCHA.Text = "0";
            //
            // lblSpellDefenceDecAttSTR
            //
            this.lblSpellDefenceDecAttSTR.AutoSize = true;
            this.lblSpellDefenceDecAttSTR.Location = new System.Drawing.Point(160, 192);
            this.lblSpellDefenceDecAttSTR.Name = "lblSpellDefenceDecAttSTR";
            this.lblSpellDefenceDecAttSTR.Size = new System.Drawing.Size(13, 13);
            this.lblSpellDefenceDecAttSTR.TabIndex = 48;
            this.lblSpellDefenceDecAttSTR.Text = "0";
            //
            // lblSpellDefenceDecAttREA
            //
            this.lblSpellDefenceDecAttREA.AutoSize = true;
            this.lblSpellDefenceDecAttREA.Location = new System.Drawing.Point(160, 172);
            this.lblSpellDefenceDecAttREA.Name = "lblSpellDefenceDecAttREA";
            this.lblSpellDefenceDecAttREA.Size = new System.Drawing.Size(13, 13);
            this.lblSpellDefenceDecAttREA.TabIndex = 40;
            this.lblSpellDefenceDecAttREA.Text = "0";
            //
            // lblSpellDefenceDecAttAGI
            //
            this.lblSpellDefenceDecAttAGI.AutoSize = true;
            this.lblSpellDefenceDecAttAGI.Location = new System.Drawing.Point(160, 152);
            this.lblSpellDefenceDecAttAGI.Name = "lblSpellDefenceDecAttAGI";
            this.lblSpellDefenceDecAttAGI.Size = new System.Drawing.Size(13, 13);
            this.lblSpellDefenceDecAttAGI.TabIndex = 38;
            this.lblSpellDefenceDecAttAGI.Text = "0";
            //
            // lblSpellDefenceDecAttBOD
            //
            this.lblSpellDefenceDecAttBOD.AutoSize = true;
            this.lblSpellDefenceDecAttBOD.Location = new System.Drawing.Point(160, 132);
            this.lblSpellDefenceDecAttBOD.Name = "lblSpellDefenceDecAttBOD";
            this.lblSpellDefenceDecAttBOD.Size = new System.Drawing.Size(13, 13);
            this.lblSpellDefenceDecAttBOD.TabIndex = 36;
            this.lblSpellDefenceDecAttBOD.Text = "0";
            //
            // lblSpellDefenceDetection
            //
            this.lblSpellDefenceDetection.AutoSize = true;
            this.lblSpellDefenceDetection.Location = new System.Drawing.Point(160, 112);
            this.lblSpellDefenceDetection.Name = "lblSpellDefenceDetection";
            this.lblSpellDefenceDetection.Size = new System.Drawing.Size(13, 13);
            this.lblSpellDefenceDetection.TabIndex = 34;
            this.lblSpellDefenceDetection.Text = "0";
            //
            // lblSpellDefenceDirectSoakPhysical
            //
            this.lblSpellDefenceDirectSoakPhysical.AutoSize = true;
            this.lblSpellDefenceDirectSoakPhysical.Location = new System.Drawing.Point(160, 92);
            this.lblSpellDefenceDirectSoakPhysical.Name = "lblSpellDefenceDirectSoakPhysical";
            this.lblSpellDefenceDirectSoakPhysical.Size = new System.Drawing.Size(13, 13);
            this.lblSpellDefenceDirectSoakPhysical.TabIndex = 32;
            this.lblSpellDefenceDirectSoakPhysical.Text = "0";
            //
            // lblSpellDefenceDirectSoakMana
            //
            this.lblSpellDefenceDirectSoakMana.AutoSize = true;
            this.lblSpellDefenceDirectSoakMana.Location = new System.Drawing.Point(160, 72);
            this.lblSpellDefenceDirectSoakMana.Name = "lblSpellDefenceDirectSoakMana";
            this.lblSpellDefenceDirectSoakMana.Size = new System.Drawing.Size(13, 13);
            this.lblSpellDefenceDirectSoakMana.TabIndex = 30;
            this.lblSpellDefenceDirectSoakMana.Text = "0";
            //
            // lblSpellDefenceIndirectSoak
            //
            this.lblSpellDefenceIndirectSoak.AutoSize = true;
            this.lblSpellDefenceIndirectSoak.Location = new System.Drawing.Point(160, 52);
            this.lblSpellDefenceIndirectSoak.Name = "lblSpellDefenceIndirectSoak";
            this.lblSpellDefenceIndirectSoak.Size = new System.Drawing.Size(13, 13);
            this.lblSpellDefenceIndirectSoak.TabIndex = 28;
            this.lblSpellDefenceIndirectSoak.Text = "0";
            //
            // lblSpellDefenceIndirectDodge
            //
            this.lblSpellDefenceIndirectDodge.AutoSize = true;
            this.lblSpellDefenceIndirectDodge.Location = new System.Drawing.Point(160, 32);
            this.lblSpellDefenceIndirectDodge.Name = "lblSpellDefenceIndirectDodge";
            this.lblSpellDefenceIndirectDodge.Size = new System.Drawing.Size(13, 13);
            this.lblSpellDefenceIndirectDodge.TabIndex = 26;
            this.lblSpellDefenceIndirectDodge.Text = "0";
            //
            // panel1
            //
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)));
            this.panel1.AutoScroll = true;
            this.panel1.Location = new System.Drawing.Point(9, 447);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(824, 156);
            this.panel1.TabIndex = 110;
            //
            // cmsLimitModifier
            //
            this.cmsLimitModifier.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tssLimitModifierNotes,
            this.tssLimitModifierEdit});
            this.cmsLimitModifier.Name = "cmsLimitModifier";
            this.cmsLimitModifier.Size = new System.Drawing.Size(106, 48);
            //
            // tssLimitModifierNotes
            //
            this.tssLimitModifierNotes.Image = global::Chummer.Properties.Resources.note_edit;
            this.tssLimitModifierNotes.Name = "tssLimitModifierNotes";
            this.tssLimitModifierNotes.Size = new System.Drawing.Size(105, 22);
            this.tssLimitModifierNotes.Tag = "Menu_Notes";
            this.tssLimitModifierNotes.Text = "&Notes";
            this.tssLimitModifierNotes.Click += new System.EventHandler(this.tssLimitModifierNotes_Click);
            //
            // tssLimitModifierEdit
            //
            this.tssLimitModifierEdit.Image = global::Chummer.Properties.Resources.house_edit;
            this.tssLimitModifierEdit.Name = "tssLimitModifierEdit";
            this.tssLimitModifierEdit.Size = new System.Drawing.Size(105, 22);
            this.tssLimitModifierEdit.Tag = "Menu_Edit";
            this.tssLimitModifierEdit.Text = "&Edit";
            this.tssLimitModifierEdit.Click += new System.EventHandler(this.tssLimitModifierEdit_Click);
            //
            // cmsCustomLimitModifier
            //
            this.cmsCustomLimitModifier.Name = "cmsCustomLimitModifier";
            this.cmsCustomLimitModifier.Size = new System.Drawing.Size(61, 4);
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
            this.cmsAdvancedProgram.Opening += new System.ComponentModel.CancelEventHandler(this.ContextMenu_Opening);
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
            // frmCreate
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.ClientSize = new System.Drawing.Size(1040, 664);
            this.Controls.Add(this.splitMain);
            this.Controls.Add(this.StatusStrip);
            this.Controls.Add(this.toolStrip);
            this.Controls.Add(this.mnuCreateMenu);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.mnuCreateMenu;
            this.Name = "frmCreate";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Chummer - Create New Character";
            this.Activated += new System.EventHandler(this.frmCreate_Activated);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmCreate_FormClosing);
            this.Load += new System.EventHandler(this.frmCreate_Load);
            this.Shown += new System.EventHandler(this.frmCreate_Shown);
            this.Resize += new System.EventHandler(this.frmCreate_Resize);
            this.StatusStrip.ResumeLayout(false);
            this.StatusStrip.PerformLayout();
            this.cmsMartialArts.ResumeLayout(false);
            this.cmsSpellButton.ResumeLayout(false);
            this.cmsComplexForm.ResumeLayout(false);
            this.cmsCyberware.ResumeLayout(false);
            this.cmsLifestyle.ResumeLayout(false);
            this.cmsArmor.ResumeLayout(false);
            this.cmsWeapon.ResumeLayout(false);
            this.cmsGearButton.ResumeLayout(false);
            this.cmsVehicle.ResumeLayout(false);
            this.mnuCreateMenu.ResumeLayout(false);
            this.mnuCreateMenu.PerformLayout();
            this.toolStrip.ResumeLayout(false);
            this.toolStrip.PerformLayout();
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
            this.tabPeople.ResumeLayout(false);
            this.tabContacts.ResumeLayout(false);
            this.tabContacts.PerformLayout();
            this.tabEnemies.ResumeLayout(false);
            this.tabEnemies.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudMysticAdeptMAGMagician)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudNuyen)).EndInit();
            this.tabSkills.ResumeLayout(false);
            this.tabLimits.ResumeLayout(false);
            this.tabLimits.PerformLayout();
            this.tabMartialArts.ResumeLayout(false);
            this.tabMartialArts.PerformLayout();
            this.tabMagician.ResumeLayout(false);
            this.tabMagician.PerformLayout();
            this.tabAdept.ResumeLayout(false);
            this.tabTechnomancer.ResumeLayout(false);
            this.tabTechnomancer.PerformLayout();
            this.tabAdvancedPrograms.ResumeLayout(false);
            this.tabAdvancedPrograms.PerformLayout();
            this.tabCritter.ResumeLayout(false);
            this.tabCritter.PerformLayout();
            this.tabInitiation.ResumeLayout(false);
            this.tabInitiation.PerformLayout();
            this.tabCyberware.ResumeLayout(false);
            this.tabCyberware.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudCyberwareRating)).EndInit();
            this.tabStreetGear.ResumeLayout(false);
            this.tabStreetGearTabs.ResumeLayout(false);
            this.tabLifestyle.ResumeLayout(false);
            this.tabLifestyle.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudLifestyleMonths)).EndInit();
            this.tabArmor.ResumeLayout(false);
            this.tabArmor.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudArmorRating)).EndInit();
            this.tabWeapons.ResumeLayout(false);
            this.tabWeapons.PerformLayout();
            this.tabGear.ResumeLayout(false);
            this.tabGear.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudAdeptWayDiscount)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudGearQty)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudGearRating)).EndInit();
            this.tabPets.ResumeLayout(false);
            this.tabPets.PerformLayout();
            this.tabVehicles.ResumeLayout(false);
            this.tabVehicles.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudVehicleGearQty)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudVehicleRating)).EndInit();
            this.tabCharacterInfo.ResumeLayout(false);
            this.tabCharacterInfo.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudMugshotIndex)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picMugshot)).EndInit();
            this.tabImprovements.ResumeLayout(false);
            this.tabInfo.ResumeLayout(false);
            this.tabBPSummary.ResumeLayout(false);
            this.tabBPSummary.PerformLayout();
            this.tabOtherInfo.ResumeLayout(false);
            this.tabOtherInfo.PerformLayout();
            this.tabDefences.ResumeLayout(false);
            this.tabDefences.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudCounterspellingDice)).EndInit();
            this.cmsLimitModifier.ResumeLayout(false);
            this.cmsInitiationNotes.ResumeLayout(false);
            this.cmsMetamagic.ResumeLayout(false);
            this.cmsTechnique.ResumeLayout(false);
            this.cmsAdvancedProgram.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion

        internal System.Windows.Forms.StatusStrip StatusStrip;
        internal System.Windows.Forms.ToolStripStatusLabel tssBPLabel;
        internal System.Windows.Forms.ToolStripStatusLabel tssBP;
        internal System.Windows.Forms.ToolStripStatusLabel tssBPRemainLabel;
        internal System.Windows.Forms.ToolStripStatusLabel tssBPRemain;
        private System.Windows.Forms.Label lblAttributes;
        private System.Windows.Forms.Label lblNuyen;
        private System.Windows.Forms.NumericUpDown nudNuyen;
        private System.Windows.Forms.Label lblNuyenTotal;
        private System.Windows.Forms.Label lblMetatypeLabel;
        private System.Windows.Forms.Label lblMetatype;
        private System.Windows.Forms.SaveFileDialog dlgSaveFile;
        private TheArtOfDev.HtmlRenderer.WinForms.HtmlToolTip tipTooltip;
        private System.Windows.Forms.TabControl tabCharacterTabs;
        private System.Windows.Forms.TabPage tabCommon;
        private System.Windows.Forms.TabPage tabMagician;
        private System.Windows.Forms.TabPage tabAdept;
        private System.Windows.Forms.TabPage tabTechnomancer;
        private System.Windows.Forms.TabPage tabCyberware;
        private System.Windows.Forms.TabPage tabStreetGear;
        private TreeView treQualities;
        private System.Windows.Forms.Button cmdAddSpirit;
        private System.Windows.Forms.Label lblSpirits;
        private System.Windows.Forms.Panel panSpirits;
        private System.Windows.Forms.Button cmdAddSprite;
        private System.Windows.Forms.Label lblSprites;
        private System.Windows.Forms.Panel panSprites;
        private System.Windows.Forms.Label lblComplexForms;
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
        private TreeView treCyberware;
        private System.Windows.Forms.ComboBox cboCyberwareGrade;
        private System.Windows.Forms.NumericUpDown nudCyberwareRating;
        private System.Windows.Forms.Button cmdDeleteCyberware;
        private System.Windows.Forms.Button cmdAddBioware;
        private System.Windows.Forms.TabControl tabInfo;
        private System.Windows.Forms.TabPage tabBPSummary;
        private System.Windows.Forms.Label lblEnemiesBP;
        private System.Windows.Forms.Label lblBuildEnemies;
        private System.Windows.Forms.Label lblKarmaMetatypeBP;
        private System.Windows.Forms.Label label12;
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
        private System.Windows.Forms.Label lblArmor;
        private System.Windows.Forms.Label lblArmorLabel;
        private System.Windows.Forms.Label lblAstralINI;
        private System.Windows.Forms.Label lblMatrixINI;
        private System.Windows.Forms.Label lblINI;
        private System.Windows.Forms.Label lblCMStun;
        private System.Windows.Forms.Label lblCMPhysical;
        private System.Windows.Forms.Label lblAstralINILabel;
        private System.Windows.Forms.Label lblMatrixINILabel;
        private System.Windows.Forms.Label lblINILabel;
        private System.Windows.Forms.Label lblCMStunLabel;
        private System.Windows.Forms.Label lblCMPhysicalLabel;
        private System.Windows.Forms.Button cmdDeleteWeapon;
        private TreeView treWeapons;
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
        private TreeView treArmor;
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
        private System.Windows.Forms.ToolStripMenuItem tsCyberwareAddAsPlugin;
        private System.Windows.Forms.Button cmdDeleteLifestyle;
        private TreeView treLifestyles;
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
        private TreeView treGear;
        private System.Windows.Forms.Button cmdDeleteGear;
        private System.Windows.Forms.NumericUpDown nudGearRating;
        private System.Windows.Forms.Label lblGearRatingLabel;
        private System.Windows.Forms.ContextMenuStrip cmsWeapon;
        private System.Windows.Forms.ToolStripMenuItem tsWeaponAddAccessory;
        private System.Windows.Forms.TabPage tabVehicles;
        private System.Windows.Forms.ContextMenuStrip cmsArmor;
        private System.Windows.Forms.ToolStripMenuItem tsAddArmorMod;
        private System.Windows.Forms.ToolStrip toolStrip;
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
        private TreeView treVehicles;
        private System.Windows.Forms.Label lblVehicleNameLabel;
        private System.Windows.Forms.Label lblVehicleName;
        private System.Windows.Forms.Label lblVehicleCategoryLabel;
        private System.Windows.Forms.Label lblVehicleCategory;
        private System.Windows.Forms.Label lblVehicleSensor;
        private System.Windows.Forms.Label lblVehicleSensorLabel;
        private System.Windows.Forms.Label lblVehiclePilot;
        private System.Windows.Forms.Label lblVehiclePilotLabel;
        private System.Windows.Forms.Label lblVehicleArmor;
        private System.Windows.Forms.Label lblVehicleArmorLabel;
        private System.Windows.Forms.Label lblVehicleBody;
        private System.Windows.Forms.Label lblVehicleBodyLabel;
        private System.Windows.Forms.Label lblVehicleSpeed;
        private System.Windows.Forms.Label lblVehicleSpeedLabel;
        private System.Windows.Forms.Label lblVehicleCost;
        private System.Windows.Forms.Label lblVehicleCostLabel;
        private System.Windows.Forms.Label lblVehicleAvail;
        private System.Windows.Forms.Label lblVehicleAvailLabel;
        private System.Windows.Forms.Label lblVehicleAccel;
        private System.Windows.Forms.Label lblVehicleAccelLabel;
        private System.Windows.Forms.Label lblVehicleHandling;
        private System.Windows.Forms.Label lblVehicleHandlingLabel;
        private System.Windows.Forms.ToolStripButton tsbPrint;
        private System.Windows.Forms.ContextMenuStrip cmsVehicle;
        private System.Windows.Forms.ToolStripMenuItem tsVehicleAddMod;
        private System.Windows.Forms.ToolStripMenuItem tsVehicleAddWeaponMount;
        private System.Windows.Forms.NumericUpDown nudVehicleRating;
        private System.Windows.Forms.Label lblVehicleRatingLabel;
        private System.Windows.Forms.ToolStripMenuItem tsVehicleAddWeapon;
        private System.Windows.Forms.ToolStripMenuItem tsVehicleAddWeaponAccessory;
        private System.Windows.Forms.Label lblVehicleSlots;
        private System.Windows.Forms.Label lblVehicleSlotsLabel;
        private System.Windows.Forms.ContextMenuStrip cmsVehicleWeapon;
        private System.Windows.Forms.ToolStripMenuItem tsVehicleAddWeaponAccessoryAlt;
        private System.Windows.Forms.CheckBox chkArmorEquipped;
        private System.Windows.Forms.TabPage tabMartialArts;
        private TreeView treMartialArts;
        private System.Windows.Forms.Button cmdDeleteMartialArt;
        private System.Windows.Forms.ContextMenuStrip cmsMartialArts;
        private System.Windows.Forms.ToolStripMenuItem tsMartialArtsAddAdvantage;
        private System.Windows.Forms.TabPage tabCharacterInfo;
        private System.Windows.Forms.TextBox txtConcept;
        private System.Windows.Forms.Label lblConcept;
        private System.Windows.Forms.TextBox txtBackground;
        private System.Windows.Forms.Label lblBackground;
        private System.Windows.Forms.TextBox txtDescription;
        private System.Windows.Forms.Label lblDescription;
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
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel3;
        private System.Windows.Forms.ToolStripStatusLabel tssEssence;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel4;
        private System.Windows.Forms.ToolStripStatusLabel tssNuyenRemaining;
        private System.Windows.Forms.Label lblGearDeviceRating;
        private System.Windows.Forms.Label lblGearDeviceRatingLabel;
        private System.Windows.Forms.Label lblManeuversBP;
        private System.Windows.Forms.Label lblBuildManeuvers;
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
        private System.Windows.Forms.Label lblVehicleSource;
        private System.Windows.Forms.Label lblVehicleSourceLabel;
        private System.Windows.Forms.Label lblMugshot;
        private System.Windows.Forms.PictureBox picMugshot;
        private System.Windows.Forms.Button cmdDeleteMugshot;
        private System.Windows.Forms.Button cmdAddMugshot;
        private System.Windows.Forms.TextBox txtNotes;
        private System.Windows.Forms.Label lblNotes;
        private System.Windows.Forms.Label lblAttributesAug;
        private System.Windows.Forms.Label lblAttributesBase;
        private System.Windows.Forms.Label lblAttributesMetatype;
        private System.Windows.Forms.ToolStripMenuItem mnuFilePrint;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.TabPage tabInitiation;
        private System.Windows.Forms.Button cmdAddMetamagic;
        private System.Windows.Forms.Label lblInitiationBP;
        private System.Windows.Forms.Label lblBuildInitiation;
        private System.Windows.Forms.Label lblWeaponSlots;
        private System.Windows.Forms.Label lblWeaponSlotsLabel;
        private System.Windows.Forms.NumericUpDown nudMysticAdeptMAGMagician;
        private System.Windows.Forms.Label lblMysticAdeptAssignment;
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
        private System.Windows.Forms.CheckBox chkVehicleIncludedInWeapon;
        private System.Windows.Forms.CheckBox chkVehicleWeaponAccessoryInstalled;
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
        private System.Windows.Forms.ToolStripMenuItem tsWeaponAddUnderbarrel;
        private System.Windows.Forms.Button cmdDeleteComplexForm;
        private System.Windows.Forms.Label lblComplexFormSource;
        private System.Windows.Forms.Label lblComplexFormSourceLabel;
        private TreeView treComplexForms;
        private System.Windows.Forms.Label lblFadingAttributesValue;
        private System.Windows.Forms.Label lblFadingAttributes;
        private System.Windows.Forms.Label lblFadingAttributesLabel;
        private System.Windows.Forms.ContextMenuStrip cmsComplexForm;
        private System.Windows.Forms.ToolStripMenuItem tsAddComplexFormOption;
        private System.Windows.Forms.NumericUpDown nudVehicleGearQty;
        private System.Windows.Forms.Label lblVehicleGearQtyLabel;
        private System.Windows.Forms.Label lblWeaponConceal;
        private System.Windows.Forms.Label lblWeaponConcealLabel;
        private System.Windows.Forms.ContextMenuStrip cmsGearButton;
        private System.Windows.Forms.ToolStripMenuItem tsGearButtonAddAccessory;
        private System.Windows.Forms.ToolStripMenuItem tsGearAddNexus;
        private System.Windows.Forms.ToolStripMenuItem tsVehicleAddNexus;
        private System.Windows.Forms.Button cmdDeleteQuality;
        private System.Windows.Forms.Button cmdAddQuality;
        private System.Windows.Forms.Label lblQualitySource;
        private System.Windows.Forms.Label lblQualitySourceLabel;
        private System.Windows.Forms.Label lblQualityBP;
        private System.Windows.Forms.Label lblQualityBPLabel;
        private System.Windows.Forms.CheckBox chkInitiationOrdeal;
        private System.Windows.Forms.CheckBox chkInitiationGroup;
        private System.Windows.Forms.Label lblMovement;
        private System.Windows.Forms.Label lblMovementLabel;
        private System.Windows.Forms.Label lblCritterPowerPoints;
        private System.Windows.Forms.Label lblCritterPowerPointsLabel;
        private System.Windows.Forms.Label lblCritterPowerPointCost;
        private System.Windows.Forms.Label lblCritterPowerPointCostLabel;
        private System.Windows.Forms.ToolStripMenuItem mnuSpecialChangeMetatype;
        private System.Windows.Forms.ToolStripMenuItem mnuSpecialCreatePACKSKit;
        private System.Windows.Forms.Label lblMemory;
        private System.Windows.Forms.Label lblMemoryLabel;
        private System.Windows.Forms.Label lblLiftCarry;
        private System.Windows.Forms.Label lblLiftCarryLabel;
        private System.Windows.Forms.Label lblJudgeIntentions;
        private System.Windows.Forms.Label lblJudgeIntentionsLabel;
        private System.Windows.Forms.Label lblComposure;
        private System.Windows.Forms.Label lblComposureLabel;
        private System.Windows.Forms.CheckBox chkGearEquipped;
        private System.Windows.Forms.ToolStripMenuItem mnuSpecialMutantCritter;
        private System.Windows.Forms.ToolStripMenuItem mnuSpecialToxicCritter;
        private System.Windows.Forms.Label lblArmorCapacity;
        private System.Windows.Forms.Label lblArmorCapacityLabel;
        private System.Windows.Forms.ToolStripMenuItem tsAddArmorGear;
        private System.Windows.Forms.ContextMenuStrip cmsArmorGear;
        private System.Windows.Forms.ToolStripMenuItem tsArmorGearAddAsPlugin;
        private System.Windows.Forms.Label lblMetamagicSource;
        private System.Windows.Forms.Label lblMetamagicSourceLabel;
        private System.Windows.Forms.ToolStripMenuItem tsArmorNotes;
        private System.Windows.Forms.ToolStripMenuItem tsWeaponNotes;
        private System.Windows.Forms.ToolStripMenuItem tsCyberwareNotes;
        private System.Windows.Forms.ContextMenuStrip cmsArmorMod;
        private System.Windows.Forms.ToolStripMenuItem tsArmorModNotes;
        private System.Windows.Forms.ContextMenuStrip cmsQuality;
        private System.Windows.Forms.ToolStripMenuItem tsQualityNotes;
        private System.Windows.Forms.ToolStripMenuItem tsMartialArtsNotes;
        private System.Windows.Forms.ContextMenuStrip cmsSpell;
        private System.Windows.Forms.ToolStripMenuItem tsSpellNotes;
        private System.Windows.Forms.ToolStripMenuItem tsComplexFormNotes;
        private System.Windows.Forms.ContextMenuStrip cmsCritterPowers;
        private System.Windows.Forms.ToolStripMenuItem tsCritterPowersNotes;
        private System.Windows.Forms.ToolStripMenuItem tsGearNotes;
        private System.Windows.Forms.ToolStripMenuItem tsVehicleNotes;
        private System.Windows.Forms.ContextMenuStrip cmsLifestyleNotes;
        private System.Windows.Forms.ToolStripMenuItem tsLifestyleNotes;
        private System.Windows.Forms.ToolStripMenuItem tsArmorGearNotes;
        private System.Windows.Forms.ContextMenuStrip cmsWeaponAccessory;
        private System.Windows.Forms.ToolStripMenuItem tsWeaponAccessoryNotes;
        private System.Windows.Forms.ContextMenuStrip cmsWeaponMod;
        private System.Windows.Forms.ToolStripMenuItem tsWeaponModNotes;
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
        private System.Windows.Forms.Label lblGearAP;
        private System.Windows.Forms.Label lblGearAPLabel;
        private System.Windows.Forms.Label lblGearDamage;
        private System.Windows.Forms.Label lblGearDamageLabel;
        private System.Windows.Forms.Label lblVehicleDevice;
        private System.Windows.Forms.Label lblVehicleDeviceLabel;
        private System.Windows.Forms.ToolStripMenuItem tsArmorName;
        private System.Windows.Forms.SplitContainer splitMain;
        private System.Windows.Forms.Label lblLivingPersonaLabel;
        private System.Windows.Forms.Label lblLivingPersonaDeviceRating;
        private System.Windows.Forms.Label lblLivingPersonaDeviceRatingLabel;
        private System.Windows.Forms.Label lblMetatypeSource;
        private System.Windows.Forms.Label lblMetatypeSourceLabel;
        private System.Windows.Forms.TextBox txtAlias;
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
        private System.Windows.Forms.CheckBox chkVehicleHomeNode;
        private System.Windows.Forms.CheckBox chkGearHomeNode;
        private System.Windows.Forms.ToolStripMenuItem tsLifestyleName;
        private System.Windows.Forms.ToolStripMenuItem mnuFileClose;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.TabPage tabImprovements;
        private System.Windows.Forms.FlowLayoutPanel panImprovements;
        private System.Windows.Forms.ContextMenuStrip cmsGearLocation;
        private System.Windows.Forms.ToolStripMenuItem tsGearRenameLocation;
        private System.Windows.Forms.ToolStripMenuItem mnuSpecialReapplyImprovements;
        private System.Windows.Forms.ContextMenuStrip cmsSpellButton;
        private System.Windows.Forms.ToolStripMenuItem tsCreateSpell;
        private System.Windows.Forms.Label lblPublicAwareTotal;
        private System.Windows.Forms.Label lblNotorietyTotal;
        private System.Windows.Forms.Label lblStreetCredTotal;
        private System.Windows.Forms.Label lblPublicAware;
        private System.Windows.Forms.Label lblNotoriety;
        private System.Windows.Forms.Label lblStreetCred;
        private System.Windows.Forms.Label lblWeaponDicePool;
        private System.Windows.Forms.Label lblWeaponDicePoolLabel;
        private System.Windows.Forms.ContextMenuStrip cmsArmorLocation;
        private System.Windows.Forms.ToolStripMenuItem tsArmorRenameLocation;
        private System.Windows.Forms.Button cmdAddArmorBundle;
        private System.Windows.Forms.Button cmdArmorUnEquipAll;
        private System.Windows.Forms.Button cmdArmorEquipAll;
        private System.Windows.Forms.Label lblArmorEquipped;
        private System.Windows.Forms.Label lblArmorEquippedLabel;
        private System.Windows.Forms.ToolStripMenuItem tsEditLifestyle;
        private System.Windows.Forms.ContextMenuStrip cmsCyberwareGear;
        private System.Windows.Forms.ToolStripMenuItem tsCyberwareGearMenuAddAsPlugin;
        private System.Windows.Forms.ContextMenuStrip cmsWeaponAccessoryGear;
        private System.Windows.Forms.ToolStripMenuItem tsWeaponAccessoryGearMenuAddAsPlugin;
        private System.Windows.Forms.ToolStripMenuItem tsWeaponAccessoryAddGear;
        private System.Windows.Forms.ToolStripMenuItem tsCyberwareAddGear;
        private System.Windows.Forms.Button cmdAddVehicleLocation;
        private System.Windows.Forms.ContextMenuStrip cmsVehicleLocation;
        private System.Windows.Forms.ToolStripMenuItem tsVehicleRenameLocation;
        private System.Windows.Forms.ToolStripSeparator tsbSeparator;
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
        private System.Windows.Forms.ContextMenuStrip cmsVehicleWeaponMod;
        private System.Windows.Forms.ToolStripMenuItem tsVehicleWeaponModNotes;
        private System.Windows.Forms.TabPage tabPets;
        private System.Windows.Forms.Button cmdAddPet;
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
        private System.Windows.Forms.CheckBox chkActiveCommlink;
        private System.Windows.Forms.ToolStripMenuItem mnuSpecialBPAvailLimit;
        private System.Windows.Forms.ToolStripMenuItem mnuSpecialConvertToFreeSprite;
        private System.Windows.Forms.Label lblVehicleWeaponRangeExtreme;
        private System.Windows.Forms.Label lblVehicleWeaponRangeLong;
        private System.Windows.Forms.Label lblVehicleWeaponRangeMedium;
        private System.Windows.Forms.Label lblVehicleWeaponRangeShort;
        private System.Windows.Forms.Label lblVehicleWeaponRangeExtremeLabel;
        private System.Windows.Forms.Label lblVehicleWeaponRangeLongLabel;
        private System.Windows.Forms.Label lblVehicleWeaponRangeMediumLabel;
        private System.Windows.Forms.Label lblVehicleWeaponRangeShortLabel;
        private System.Windows.Forms.Label lblVehicleWeaponRangeLabel;
        private System.Windows.Forms.Label lblVehicleWeaponAmmo;
        private System.Windows.Forms.Label lblVehicleWeaponAmmoLabel;
        private System.Windows.Forms.Label lblVehicleWeaponMode;
        private System.Windows.Forms.Label lblVehicleWeaponModeLabel;
        private System.Windows.Forms.Label lblVehicleWeaponNameLabel;
        private System.Windows.Forms.Label lblVehicleWeaponName;
        private System.Windows.Forms.Label lblVehicleWeaponCategoryLabel;
        private System.Windows.Forms.Label lblVehicleWeaponAP;
        private System.Windows.Forms.Label lblVehicleWeaponCategory;
        private System.Windows.Forms.Label lblVehicleWeaponAPLabel;
        private System.Windows.Forms.Label lblVehicleWeaponDamageLabel;
        private System.Windows.Forms.Label lblVehicleWeaponDamage;
        private System.Windows.Forms.Label lblBiowareESS;
        private System.Windows.Forms.Label lblCyberwareESS;
        private System.Windows.Forms.Label lblBiowareESSLabel;
        private System.Windows.Forms.Label lblCyberwareESSLabel;
        private System.Windows.Forms.Label lblEssenceHoleESS;
        private System.Windows.Forms.Label lblEssenceHoleESSLabel;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lblRiggingINI;
        private System.Windows.Forms.Label lblRiggingINILabel;
        private System.Windows.Forms.Label lblMatrixINIHot;
        private System.Windows.Forms.Label lblMatrixINIHotLabel;
        private System.Windows.Forms.Label lblMatrixINICold;
        private System.Windows.Forms.Label lblMatrixINIColdLabel;
        private SplitButton cmdAddLifestyle;
        private System.Windows.Forms.TabPage tabLimits;
        private System.Windows.Forms.Button cmdAddLimitModifier;
        private TreeView treLimit;
        private System.Windows.Forms.Button cmdDeleteLimitModifier;
        private System.Windows.Forms.ContextMenuStrip cmsLimitModifier;
        private System.Windows.Forms.ToolStripMenuItem tssLimitModifierNotes;
        private System.Windows.Forms.ContextMenuStrip cmsCustomLimitModifier;
        private System.Windows.Forms.ToolStripMenuItem tssLimitModifierEdit;
        private System.Windows.Forms.Label lblSocialLabel;
        private System.Windows.Forms.Label lblSocial;
        private System.Windows.Forms.Label lblMentalLabel;
        private System.Windows.Forms.Label lblMental;
        private System.Windows.Forms.Label lblPhysicalLabel;
        private System.Windows.Forms.Label lblPhysical;
        private System.Windows.Forms.Label lblWeaponAccuracy;
        private System.Windows.Forms.Label lblWeaponAccuracyLabel;
        private SplitButton cmdAddMartialArt;
        private System.Windows.Forms.Label label33;
        private System.Windows.Forms.Panel panel1;
        private SplitButton cmdAddSpell;
        private SplitButton cmdAddComplexForm;
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
        private System.Windows.Forms.Label lblDrainAttributesValue;
        private System.Windows.Forms.Label lblDrainAttributes;
        private System.Windows.Forms.Label lblDrainAttributesLabel;
        private ComboBox cboTradition;
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
        private TreeView treSpells;
        private System.Windows.Forms.Button cmdDeleteSpell;
        private System.Windows.Forms.Label lblSelectedSpells;
        private System.Windows.Forms.Label lblFV;
        private System.Windows.Forms.Label lblFVLabel;
        private System.Windows.Forms.Label lblDuration;
        private System.Windows.Forms.Label lblDurationLabel;
        private System.Windows.Forms.Label lblTarget;
        private System.Windows.Forms.Label lblTargetLabel;
        private System.Windows.Forms.Label lblLivingPersonaFirewall;
        private System.Windows.Forms.Label lblLivingPersonaFirewallLabel;
        private System.Windows.Forms.Label lblLivingPersonaDataProcessing;
        private System.Windows.Forms.Label lblLivingPersonaDataProcessingLabel;
        private System.Windows.Forms.Label lblLivingPersonaSleaze;
        private System.Windows.Forms.Label lblLivingPersonaSleazeLabel;
        private System.Windows.Forms.Label lblLivingPersonaAttack;
        private System.Windows.Forms.Label lblLivingPersonaAttackLabel;
        private System.Windows.Forms.Label lblArmorValueLabel;
        private System.Windows.Forms.Label lblArmorValue;
        private System.Windows.Forms.Label lblGearFirewallLabel;
        private System.Windows.Forms.Label lblGearDataProcessingLabel;
        private System.Windows.Forms.Label lblGearSleazeLabel;
        private System.Windows.Forms.Label lblGearAttackLabel;
        private System.Windows.Forms.Label lblCyberFirewall;
        private System.Windows.Forms.Label lblCyberFirewallLabel;
        private System.Windows.Forms.Label lblCyberDataProcessing;
        private System.Windows.Forms.Label lblCyberDataProcessingLabel;
        private System.Windows.Forms.Label lblCyberSleaze;
        private System.Windows.Forms.Label lblCyberSleazeLabel;
        private System.Windows.Forms.Label lblCyberAttack;
        private System.Windows.Forms.Label lblCyberAttackLabel;
        private System.Windows.Forms.Label lblCyberDeviceRating;
        private System.Windows.Forms.Label lblCyberDeviceRatingLabel;
        private System.Windows.Forms.Label lblVehicleFirewallLabel;
        private System.Windows.Forms.Label lblVehicleDataProcessingLabel;
        private System.Windows.Forms.Label lblVehicleSleazeLabel;
        private System.Windows.Forms.Label lblVehicleAttackLabel;
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
        private System.Windows.Forms.Label lblAstralLabel;
        private System.Windows.Forms.Label lblAstral;
        private ComboBox cboDrain;
        private System.Windows.Forms.TextBox txtTraditionName;
        private System.Windows.Forms.Label lblTraditionName;
        private ComboBox cboSpiritCombat;
        private System.Windows.Forms.Label lblSpiritCombat;
        private ComboBox cboSpiritManipulation;
        private System.Windows.Forms.Label lblSpiritManipulation;
        private ComboBox cboSpiritIllusion;
        private System.Windows.Forms.Label lblSpiritIllusion;
        private ComboBox cboSpiritHealth;
        private System.Windows.Forms.Label lblSpiritHealth;
        private ComboBox cboSpiritDetection;
        private System.Windows.Forms.Label lblSpiritDetection;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TabControl tabPeople;
        private System.Windows.Forms.TabPage tabContacts;
        private System.Windows.Forms.Button cmdAddContact;
        private System.Windows.Forms.FlowLayoutPanel panContacts;
        private System.Windows.Forms.TabPage tabEnemies;
        private System.Windows.Forms.FlowLayoutPanel panEnemies;
        private System.Windows.Forms.Button cmdAddEnemy;
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
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.ContextMenuStrip cmsTechnique;
        private System.Windows.Forms.ToolStripMenuItem tsAddTechniqueNotes;
        private System.Windows.Forms.Label lblContactPoints;
        private System.Windows.Forms.Label lblContactPoints_Label;
        private System.Windows.Forms.Button cmdLifeModule;
        private TreeView treCritterPowers;
        private System.Windows.Forms.Label lblWeaponRating;
        private System.Windows.Forms.Label lblWeaponRatingLabel;
        private System.Windows.Forms.Button btnCreateBackstory;
        private System.Windows.Forms.ComboBox cboGearDataProcessing;
        private System.Windows.Forms.ComboBox cboGearFirewall;
        private System.Windows.Forms.ComboBox cboGearSleaze;
        private System.Windows.Forms.ComboBox cboGearAttack;
        private System.Windows.Forms.ComboBox cboVehicleGearDataProcessing;
        private System.Windows.Forms.ComboBox cboVehicleGearFirewall;
        private System.Windows.Forms.ComboBox cboVehicleGearSleaze;
        private System.Windows.Forms.ComboBox cboVehicleGearAttack;
        private System.Windows.Forms.TabPage tabDefences;
        private System.Windows.Forms.Label lblSpellDefenceDirectSoakPhysicalLabel;
        private System.Windows.Forms.Label lblSpellDefenceDirectSoakMana;
        private System.Windows.Forms.Label lblSpellDefenceDirectSoakManaLabel;
        private System.Windows.Forms.Label lblSpellDefenceIndirectSoak;
        private System.Windows.Forms.Label lblSpellDefenceIndirectSoakLabel;
        private System.Windows.Forms.Label lblSpellDefenceIndirectDodge;
        private System.Windows.Forms.Label lblSpellDefenceIndirectDodgeLabel;
        private System.Windows.Forms.Label lblCounterspellingDiceLabel;
        internal System.Windows.Forms.NumericUpDown nudCounterspellingDice;
        private System.Windows.Forms.Label lbllSpellDefenceManipPhysical;
        private System.Windows.Forms.Label lbllSpellDefenceManipPhysicalLabel;
        private System.Windows.Forms.Label lblSpellDefenceManipMental;
        private System.Windows.Forms.Label lblSpellDefenceManipMentalLabel;
        private System.Windows.Forms.Label lblSpellDefenceIllusionPhysical;
        private System.Windows.Forms.Label lblSpellDefenceIllusionPhysicalLabel;
        private System.Windows.Forms.Label lblSpellDefenceIllusionMana;
        private System.Windows.Forms.Label lblSpellDefenceIllusionManaLabel;
        private System.Windows.Forms.Label lblSpellDefenceDecAttWIL;
        private System.Windows.Forms.Label lblSpellDefenceDecAttLOG;
        private System.Windows.Forms.Label lblSpellDefenceDecAttINT;
        private System.Windows.Forms.Label lblSpellDefenceDecAttCHA;
        private System.Windows.Forms.Label lblSpellDefenceDecAttSTR;
        private System.Windows.Forms.Label lblSpellDefenceDecAttWILLabel;
        private System.Windows.Forms.Label lblSpellDefenceDecAttLOGLabel;
        private System.Windows.Forms.Label lblSpellDefenceDecAttINTLabel;
        private System.Windows.Forms.Label lblSpellDefenceDecAttCHALabel;
        private System.Windows.Forms.Label lblSpellDefenceDecAttSTRLabel;
        private System.Windows.Forms.Label lblSpellDefenceDecAttREALabel;
        private System.Windows.Forms.Label lblSpellDefenceDecAttAGILabel;
        private System.Windows.Forms.Label lblSpellDefenceDecAttREA;
        private System.Windows.Forms.Label lblSpellDefenceDecAttAGI;
        private System.Windows.Forms.Label lblSpellDefenceDecAttBOD;
        private System.Windows.Forms.Label lblSpellDefenceDecAttBODLabel;
        private System.Windows.Forms.Label lblSpellDefenceDetection;
        private System.Windows.Forms.Label lblSpellDefenceDetectionLabel;
        private System.Windows.Forms.Label lblSpellDefenceDirectSoakPhysical;
        private TreeView treMetamagic;
        private System.Windows.Forms.Label lblVehiclePowertrainLabel;
        private System.Windows.Forms.Label lblVehicleWeaponsmod;
        private System.Windows.Forms.Label lblVehicleProtection;
        private System.Windows.Forms.Label lblVehiclePowertrain;
        private System.Windows.Forms.Label lblVehicleCosmeticLabel;
        private System.Windows.Forms.Label lblVehicleElectromagneticLabel;
        private System.Windows.Forms.Label lblVehicleBodymodLabel;
        private System.Windows.Forms.Label lblVehicleWeaponsmodLabel;
        private System.Windows.Forms.Label lblVehicleProtectionLabel;
        private System.Windows.Forms.Label lblVehicleBodymod;
        private System.Windows.Forms.Label lblVehicleCosmetic;
        private System.Windows.Forms.Label lblVehicleElectromagnetic;
        private System.Windows.Forms.Label lblVehicleDroneModSlots;
        private System.Windows.Forms.Label lblVehicleDroneModSlotsLabel;
        private System.Windows.Forms.Label lblCyberlimbSTR;
        private System.Windows.Forms.Label lblCyberlimbAGI;
        private System.Windows.Forms.Label lblCyberlimbSTRLabel;
        private System.Windows.Forms.Label lblCyberlimbAGILabel;
        private System.Windows.Forms.TabPage tabSkills;
        private UI.Skills.SkillsTabUserControl tabSkillUc;
        private System.Windows.Forms.NumericUpDown nudAdeptWayDiscount;
        private System.Windows.Forms.Label lblAdeptWayDiscount;
        private System.Windows.Forms.Button cmdCreateStackedFocus;
        private System.Windows.Forms.Label lblFoci;
        private System.Windows.Forms.TreeView treFoci;
        private System.Windows.Forms.Label lblVehicleSeats;
        private System.Windows.Forms.Label lblVehicleSeatsLabel;
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
        private TreeView treAIPrograms;
        private System.Windows.Forms.Button cmdDeleteAIProgram;
        private System.Windows.Forms.Label lblAIProgramsAdvancedPrograms;
        private System.Windows.Forms.ContextMenuStrip cmsAdvancedProgram;
        private System.Windows.Forms.ToolStripMenuItem tsAddAdvancedProgramOption;
        private System.Windows.Forms.ToolStripMenuItem tsAIProgramNotes;
        private System.Windows.Forms.Label lblHandedness;
        private ComboBox cboPrimaryArm;
        private System.Windows.Forms.CheckBox chkIsMainMugshot;
        private System.Windows.Forms.Label lblNumMugshots;
        private System.Windows.Forms.NumericUpDown nudMugshotIndex;
        private System.Windows.Forms.Label lblMugshotDimensions;
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
    }
}
